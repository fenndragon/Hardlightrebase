using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nyanotrasen.Abilities.Psionics.Abilities;

public sealed partial class SummonPsionicFamiliarActionEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId? FamiliarProto;

    [DataField]
    public string PowerName = string.Empty;

    [DataField]
    public float ManaCost = 5f;

    [DataField]
    public bool CheckInsulation = true;

    [DataField]
    public bool DoGlimmerEffects = true;

    [DataField]
    public bool FollowMaster = true;

    [DataField]
    public int MinGlimmer = 2;

    [DataField]
    public int MaxGlimmer = 5;
}
