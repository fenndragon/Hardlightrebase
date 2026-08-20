using Content.Server.Body.Components;
using Content.Server.FloofStation.Traits;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FloofStation.Traits.Events.Components;

namespace Content.Server._HL.Traits;

/// <summary>
///     Stripped down version of the Vamp system. Only here to edit the metabolizer.
///     Making this a Flavour trait.
/// </summary>
public sealed class SuccubusSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SuccubusComponent, MapInitEvent>(OnInitSuccubus);
    }

    private void OnInitSuccubus(Entity<SuccubusComponent> ent, ref MapInitEvent args)
    {

        if (!TryComp<BodyComponent>(ent, out var body)
            || !_body.TryGetBodyOrganEntityComps<MetabolizerComponent>((ent, body), out var comps))
            return;

        foreach (var (organUid, metabolizer, organ) in comps)
        {
            if (!TryComp<StomachComponent>(organUid, out var stomach))
                continue;

            if (metabolizer.MetabolizerTypes is null)
                metabolizer.MetabolizerTypes = ent.Comp.MetabolizerPrototypes;
            else metabolizer.MetabolizerTypes.UnionWith(ent.Comp.MetabolizerPrototypes);
        }

        if (TryComp<CumProducerComponent>(ent, out var cumcomp)
        && _solutionContainerSystem.ResolveSolution(ent.Owner, cumcomp.SolutionName, ref cumcomp.Solution, out var cumsol))
            EnsureCumTag(ent, cumsol);

    }

    private void EnsureCumTag(Entity<SuccubusComponent> ent, Solution cumsol)
    {
        var taint = ent.Comp.Taint;

        foreach (var reagent in cumsol)
        {
            var reagentData = reagent.Reagent.EnsureReagentData();
            if (!reagentData.Exists(x => taint == x)) //Putting it left. I haven't refactored DnaData to be safe yet.
                reagentData.Add(taint);
        }
    }

}
