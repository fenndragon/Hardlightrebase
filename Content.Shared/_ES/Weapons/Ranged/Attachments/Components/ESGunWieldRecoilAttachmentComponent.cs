using Robust.Shared.GameStates;

namespace Content.Shared._ES.Weapons.Ranged.Attachments.Components;

// MONO COMPONENT - NOT ORIGINAL ES
// WOLFGATE: correct malformed XML documentation after merge
/// <summary>
/// A <see cref="ESGunAttachmentComponent"/> that changes a gun's wielded spread and recoil control.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedGunAttachmentsSystem))]
public sealed partial class ESGunWieldRecoilAttachmentComponent : Component
{
    [DataField]
    public float WieldRecoilRecoveryModifier = 1.0f;

    [DataField]
    public float WieldRecoilIncreaseModifier = 1.0f;

    [DataField]
    public float WieldMinSpreadModifier = 1.0f;

    [DataField]
    public float WieldMaxSpreadModifier = 1.0f;
}
