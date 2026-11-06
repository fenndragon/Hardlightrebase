using Content.Shared.FixedPoint;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.GameStates;

namespace Content.Shared.FloofStation.Traits.Events.Components; // HL: Moved this to Shared so the client can use it for verb drawing.

[RegisterComponent, NetworkedComponent, Access(typeof(SharedLewdTraitSystem))]
public sealed partial class MilkProducerComponent : Component
{
    [DataField("solutionname")]
    public string SolutionName = "breasts";

    [DataField]
    public ProtoId<ReagentPrototype> ReagentId = "Breast-Milk";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("maxVol")]
    public FixedPoint2 MaxVolume = FixedPoint2.New(50);

    public Entity<SolutionComponent>? Solution = null;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("reVol")]
    public FixedPoint2 QuantityPerUpdate = 5;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("reHunger")]
    public float HungerUsage = 10f;

    [DataField]
    public TimeSpan GrowthDelay = TimeSpan.FromSeconds(10);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextGrowth = TimeSpan.FromSeconds(0);
}
