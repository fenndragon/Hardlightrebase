using Robust.Shared.GameStates;

namespace Content.Shared.Carrying // HL: Moved this to Shared so the client can use it for verb drawing
{
    /// <summary>
    /// Stores the carrier of an entity being carried.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class BeingCarriedComponent : Component
    {
        public EntityUid Carrier = default!;
    }
}
