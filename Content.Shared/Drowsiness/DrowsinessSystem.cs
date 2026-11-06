using Robust.Shared.Prototypes; // WOLFGATE: keep one prototype namespace import after merge
using Content.Shared.StatusEffect;

namespace Content.Shared.Drowsiness;

public abstract class SharedDrowsinessSystem : EntitySystem
{
    public static readonly ProtoId<StatusEffectPrototype> DrowsinessKey = "Drowsiness";
}
