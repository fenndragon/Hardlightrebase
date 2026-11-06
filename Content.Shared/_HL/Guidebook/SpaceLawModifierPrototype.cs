using Robust.Shared.Prototypes;

namespace Content.Shared._HL.Guidebook;

[Prototype]
[DataDefinition]
public sealed partial class SpaceLawModifierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Code { get; private set; }

    [DataField]
    public string Name { get; private set; }

    [DataField]
    public string Description { get; private set; }

    [DataField]
    public string Adjustment { get; private set; }
}
