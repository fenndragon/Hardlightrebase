namespace Content.Shared._HL.Engraving;

[RegisterComponent]
public sealed partial class EngravingToolComponent : Component
{
    [DataField, ViewVariables]
    public int MaxUses = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public int RemainingUses = -1;

    [DataField]
    public bool CanReengrave;

    [DataField]
    public bool Ephemeral = true;
}
