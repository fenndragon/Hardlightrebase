using Robust.Shared.GameStates;

namespace Content.Shared.Abilities.Psionics
{
    /// <summary>
    ///     Component that marks an entity as having a broken mind, preventing psionic abilities.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class MindbrokenComponent : Component
    {
        // This component is just a marker, no additional data needed
    }
}
