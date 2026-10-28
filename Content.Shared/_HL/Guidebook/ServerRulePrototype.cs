using Robust.Shared.Prototypes;

namespace Content.Shared._HL.Guidebook;

[Prototype]
[DataDefinition]
public sealed partial class ServerRulePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Dotted rule number such as "1.1" or "6.8.1". Rules are ordered by comparing each part numerically.
    /// </summary>
    [DataField(required: true)]
    public string Code { get; private set; } = default!;

    [DataField(required: true)]
    public int Section { get; private set; }

    /// <summary>
    ///     Optional sub-grouping within a section, used so a page can place its own prose between groups.
    /// </summary>
    [DataField]
    public string Group { get; private set; } = string.Empty;

    [DataField(required: true)]
    public string Text { get; private set; } = default!;

    [DataField]
    public RuleType Hardlit { get; private set; } = RuleType.NonHardlit;

    /// <summary>
    ///     Extra explanation shown below the rule text.
    /// </summary>
    [DataField]
    public string Description { get; private set; } = string.Empty;

    public static int CompareByCode(ServerRulePrototype x, ServerRulePrototype y)
    {
        var left = x.Code.Split('.');
        var right = y.Code.Split('.');

        for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
        {
            var compare = int.Parse(left[i]).CompareTo(int.Parse(right[i]));
            if (compare != 0)
                return compare;
        }

        return left.Length.CompareTo(right.Length);
    }
}

/// <summary>
///     How strictly a server rule is enforced.
/// </summary>
public enum RuleType : byte
{
    /// <summary>Only enforced when a user report is received.</summary>
    NonHardlit,

    /// <summary>Enforced at all times.</summary>
    Hardlit,

    /// <summary>Enforced at all times, with an immediate one-day ban at minimum.</summary>
    ZeroTolerance
}
