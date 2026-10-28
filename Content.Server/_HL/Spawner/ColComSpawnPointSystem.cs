using System.Collections.Frozen;
using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.Preferences;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.RoundEnd;

namespace Content.Server.Spawners.EntitySystems;

public sealed class ColComSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly RoundEndSystem _endSystem = default!;

    // List of JobEntities that will still go through the ColCom spawns.
    // The ones not on the list will spawn normally.
    private static readonly FrozenSet<string?> SpecialStationJobs =
    [
        null,
        "PlayerBorgQuadEngineer",
        "PlayerBorgQuad",
        "PlayerBorgQuadmed",
        "PlayerBorgQuadserv",
        "PlayerBorgQuadsalv",
        "PlayerBorgBattery",
    ];

    private static readonly FrozenSet<string> JobBlacklist =
    [
        "Prisoner",
    ];

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning, before: [typeof(SpawnPointSystem)]);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        // Observer spawns should not take priority into account.
        if (args.DesiredSpawnPointType == SpawnPointType.Observer)
            return;

        var normalJobCheck = _proto.TryIndex(args.Job, out var jobProto)
                                && SpecialStationJobs.Contains(jobProto.JobEntity)
                                && !JobBlacklist.Contains(jobProto.ID);
        // Violation of '!', it can absolutely be null and will often be, but we want false in this case anyway.

        if (args.HumanoidCharacterProfile?.SpawnPriority != SpawnPriorityPreference.ColComm
            || !normalJobCheck)
            return;

        // Oops no Colcom
        if (_endSystem.GetColcomm() is not { } colcom)
            return;


        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out _, out var spawnPoint, out var xform))
        {
            // We should be using _stationsystem.GetOwningStation but ColCom has no station member component
            if (args.Station != null && xform.MapUid != colcom)
                continue;

            if (_gameTicker.RunLevel == GameRunLevel.InRound && spawnPoint.SpawnType == SpawnPointType.LateJoin
                || _gameTicker.RunLevel != GameRunLevel.InRound && spawnPoint.SpawnType == SpawnPointType.Job
                                                                && (spawnPoint.Job == null || spawnPoint.Job == args.Job)
            )
                possiblePositions.Add(xform.Coordinates);
        }

        // Mappers STOLE all the points
        if (possiblePositions.Count == 0)
            return;

        var spawnLoc = _random.Pick(possiblePositions);

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station, // hardlight: We keep the original target station. This is only used for PDA initialization.
            session: args.Session); // Frontier
    }
}
