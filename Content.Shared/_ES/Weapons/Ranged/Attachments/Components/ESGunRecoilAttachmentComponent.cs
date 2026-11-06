using Robust.Shared.GameStates;

namespace Content.Shared._ES.Weapons.Ranged.Attachments.Components;

// MONO COMPONENT - NOT ORIGINAL ES
// WOLFGATE: correct malformed XML documentation after merge
/// <summary>
/// A <see cref="ESGunAttachmentComponent"/> that changes a gun's spread and recoil control.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedGunAttachmentsSystem))]
public sealed partial class ESGunRecoilAttachmentComponent : Component
{
    [DataField]
    public float RecoilRecoveryModifier = 1.0f;

    [DataField]
    public float RecoilIncreaseModifier = 1.0f;

    [DataField]
    public float MinSpreadModifier = 1.0f;

    [DataField]
    public float MaxSpreadModifier = 1.0f;
}
