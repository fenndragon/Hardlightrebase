using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.UserInterface;
using Content.Shared.Shuttles.Components;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleConsoleSystem
{
    public void InitializeNFDrone()
    {
        SubscribeLocalEvent<NFDroneConsoleComponent, ConsoleShuttleEvent>(OnNFCargoGetConsole);
        SubscribeLocalEvent<NFDroneConsoleComponent, AfterActivatableUIOpenEvent>(OnNFDronePilotConsoleOpen);
        Subs.BuiEvents<NFDroneConsoleComponent>(ShuttleConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIClosedEvent>(OnNFDronePilotConsoleClose);
        });
    }

    /// <summary>
    /// Gets the drone console target if applicable otherwise returns itself.
    /// </summary>
    public EntityUid? GetNFDroneConsole(EntityUid consoleUid)
    {
        var getShuttleEv = new ConsoleShuttleEvent
        {
            Console = consoleUid,
        };

        RaiseLocalEvent(consoleUid, ref getShuttleEv);
        return getShuttleEv.Console;
    }

    private void OnNFDronePilotConsoleOpen(EntityUid uid, NFDroneConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        component.Entity = GetNFShuttleConsole(uid);
    }

    private void OnNFDronePilotConsoleClose(EntityUid uid, NFDroneConsoleComponent component, BoundUIClosedEvent args)
    {
        // Only if last person closed UI.
        if (!_ui.IsUiOpen(uid, args.UiKey))
            component.Entity = null;
    }

    private void OnNFCargoGetConsole(EntityUid uid, NFDroneConsoleComponent component, ref ConsoleShuttleEvent args)
    {
        args.Console = GetNFShuttleConsole(uid, component);
    }

    /// <summary>
    /// Gets the relevant shuttle console to proxy from the drone console.
    /// </summary>
    private EntityUid? GetNFShuttleConsole(EntityUid uid, NFDroneConsoleComponent? sourceComp = null)
    {
        if (!Resolve(uid, ref sourceComp))
            return null;
        //Hardlight Start: Added check for cached entity to avoid unnecessary query.
        if (sourceComp.Entity is { } cachedUid &&
            Exists(cachedUid) &&
            TryComp(cachedUid, out ShuttleConsoleComponent? _) &&
            TryComp(cachedUid, out NFDroneConsoleTargetComponent? cachedTarget) &&
            sourceComp.Id == cachedTarget.Id)
        {
            return cachedUid;
        }

        sourceComp.Entity = null;
        //Hardlight End

        var query = AllEntityQuery<ShuttleConsoleComponent, NFDroneConsoleTargetComponent>();

        while (query.MoveNext(out var cUid, out _, out var targetComp))
        //Hardlight Start
        {
            if (sourceComp.Id == targetComp.Id)
            {
                sourceComp.Entity = cUid;
                return cUid;
            }
        }
        //Hardlight End

        return null;
    }
}
