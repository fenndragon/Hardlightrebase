using Content.Server.Body.Components;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._HL.Traits;

/// <summary>
///     Enables the mob to drink cum to replenish its own saturation.
///     Must be fully initialized before being added to a mob.
/// </summary>
[RegisterComponent]
public sealed partial class SuccubusComponent : Component
{
    [DataField]
    public HashSet<ProtoId<MetabolizerTypePrototype>> MetabolizerPrototypes = new() { "Succubus" };

    [DataField]
    public List<MetabolismGroupEntry> AddedMetabolismGroups = new(), RemovedMetabolismGroups = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public TaintData Taint { get; } = new(TaintType.Succubus);

}
