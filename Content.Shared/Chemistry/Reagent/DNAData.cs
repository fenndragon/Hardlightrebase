using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Reagent;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class DnaData : ReagentData
{
    [DataField]
    public string DNA = String.Empty;

    public override ReagentData Clone() => this;

    public override bool Equals(ReagentData? other) => other is DnaData data && DNA == data.DNA;
    // Hardlight : made data comparison tolerant

    public override int GetHashCode()
    {
        return DNA.GetHashCode();
    }
}
