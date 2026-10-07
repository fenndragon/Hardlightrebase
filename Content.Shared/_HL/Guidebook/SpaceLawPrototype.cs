
using Robust.Shared.Prototypes;

namespace Content.Shared._HL.Guidebook;

[Prototype]
[DataDefinition]
public sealed partial class SpaceLawPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Code { get; private set; }

    [DataField]
    public string Name { get; private set; }

    [DataField]
    public string[] Elements { get; private set; }

    [DataField]
    public List<ProtoId<SpaceLawPrototype>> Greater { get; private set; } = [];

    [DataField]
    public string[] Punishment { get; private set; }

    [DataField]
    public LawSeverity Severity { get; private set; }

    [DataField]
    public string Description { get; private set; } = "";
}

public enum LawSeverity : byte
{
    Infraction,
    Minor,
    Misdemeanor,
    Felony,
    Capital
}
