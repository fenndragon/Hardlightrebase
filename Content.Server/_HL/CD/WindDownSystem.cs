using System.Runtime.InteropServices;
using Content.Server.Chat.Managers;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.Chat;
using Robust.Server;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._HL.CD;

public sealed class WindDownSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    private const string Sawmill = "WindDownSystem";
    private ISawmill _sawmill = null!;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Constant representing the SIGUSR1 signal.
    /// Domain-specific to POSIX-compliant OSes, such as Linux and macOS.
    /// For HardLight, this is defined as a signal even more gracefully than SIGINT.
    /// This signal schedules a graceful shutdown at a good stopping point between rounds.
    /// </summary>
    private const int SIGUSR1 = 10;

    /// <summary>
    /// Just to hold a reference to give the ol middle finger to the GC.
    /// </summary>
    private PosixSignalRegistration? _sigUsr1Registration;

    /// <summary>
    /// LARPing thread safety. We love pretending globals are thread safe.
    /// </summary>
    private volatile bool _sigUsr1Received;

    /// <summary>
    /// Flag indicating a server shutdown has been scheduled.
    /// When set to <see langword="true"/>, it signals that the server is preparing to "wind down".
    /// Triggered in response to the SIGUSR1 signal, which schedules a shutdown at the end of the shift
    /// and informs players of the impending shutdown, optionally through a vote mechanism.
    /// </summary>
    public bool ShutdownScheduled;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _log.GetSawmill(Sawmill);

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Bind the catch to SIGUSR1. SIGUSR1 is the signal used to most gracefully schedule a shutdown. (service manager is expected to restart the server.)
            _sigUsr1Registration = PosixSignalRegistration.Create((PosixSignal)SIGUSR1, context =>
            {
                context.Cancel = true; // Ain't very useful if the process shits itself instantly.
                _sigUsr1Received = true; // Dealing with thread BS, so best we got is sliding a bool like it's cookies for santa.
            });
        }
        else _sawmill.Warning("This operating system is not supported. The Wind Down System will not function!");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Gate this function behind an if check.
        if (!_sigUsr1Received) return;
        _sigUsr1Received = false; // Close the door behind us. Let's *not* spawn a new vote/notice every single tick, ya?

        _sawmill.Info("SIGUSR1 Caught! Winding down...");
        ShutdownScheduled = true;
        _sawmill.Info("Pushing notice to all players.");
        const string shuttingDownSoonMsg = "Heads Up! At the end of this shift, the server will be shutting down!";
        _chatManager.ChatMessageToAll(ChatChannel.Server,
            shuttingDownSoonMsg,
            $"[bold]{shuttingDownSoonMsg}[/bold]",
            default,
            false,
            true);
        _audioSystem.PlayGlobal(new SoundPathSpecifier("/Audio/_Goobstation/Effects/ding.ogg"),
            Filter.Broadcast(),
            true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _sigUsr1Registration?.Dispose();
    }

    public void BeginShutDown(int requiredVotesToDelay = -1)
    {
        var normalizedRequiredVotesToDelay = requiredVotesToDelay < 1 ? 1 : requiredVotesToDelay;
        var voteOptions = new VoteOptions
        {
            InitiatorText = "Server",
            Title = $"""
                Everyone ready for shutdown? Make sure you have everything saved!
                Anything unsaved is lost progress!
                Required votes to delay shutdown: {normalizedRequiredVotesToDelay}
                """,
            Options = [("Ready!", true), ("Wait!", false)],
            VoterEligibility = VoteManager.VoterEligibility.All,
            DisplayVotes = false
        };
        _voteManager.CreateVote(voteOptions).OnFinished += (sender, _) =>
        {
            var enoughVotesToDelay = sender.VotesPerOption.GetValueOrDefault(false, 0) >= normalizedRequiredVotesToDelay;
            if (enoughVotesToDelay)
            {
                _sawmill.Info("Too many votes! Delaying shutdown!");
                // Timer.Spawn defers execution to prevent running the vote-manager a second time inside its own callback.
                // It loses its shit if you do.
                Robust.Shared.Timing.Timer.Spawn(TimeSpan.Zero, () => BeginShutDown(requiredVotesToDelay + 1));
            }
            else
                _server.Shutdown("Wind Down System performing scheduled shutdown.");
        };
    }
}
