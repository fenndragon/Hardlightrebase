using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on a client when it wishes to FTL dock to a station.
/// This triggers TryFTLDock to properly dock and assign the shuttle to the station.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShuttleConsoleFTLStationDockMessage : BoundUserInterfaceMessage
{
    public NetEntity Station;
    public Angle Angle;
}
