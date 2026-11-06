using Robust.Shared.GameStates;

namespace Content.Shared.Flash.Components;

/// <summary>
/// Makes the entity immune to being flashed.
/// When given to clothes in the "head", "eyes" or "mask" slot it protects the wearer.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFlashSystem))]
public sealed partial class FlashImmunityComponent : Component
{
    /// <summary>
    /// Is this component currently enabled?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    // WOLFGATE(EngineCompat) START: field used by HardLight night-vision flash handling
    /// <summary>Whether this also blocks special vision such as night vision.</summary>
    [DataField, AutoNetworkedField]
    public bool BlocksSpecialVision = true;
    // WOLFGATE END
}
