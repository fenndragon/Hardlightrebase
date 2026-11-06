using Robust.Shared.GameStates;

namespace Content.Shared.Carrying // HL: Moved this to Shared so the client can use it for verb drawing.
{
    /// <summary>
    /// Added to an entity when they are carrying somebody.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class CarryingComponent : Component
    {
        public EntityUid Carried = default!;
    }
}
