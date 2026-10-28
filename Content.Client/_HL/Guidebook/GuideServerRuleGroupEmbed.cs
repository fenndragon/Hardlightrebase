using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Shared._HL.Guidebook;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._HL.Guidebook;

/// <summary>
///     Control for displaying a list of server rules, optionally filtered by `Section` and `Group`.
/// </summary>
[UsedImplicitly]
public sealed class GuideServerRuleGroupEmbed : BoxContainer, IDocumentTag
{
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly ISawmill _sawmill;

    public GuideServerRuleGroupEmbed()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _logManager.GetSawmill("guidebook.serverrules");
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        int? section = null;
        if (args.TryGetValue("Section", out var sectionArg))
        {
            if (!int.TryParse(sectionArg, out var parsed))
            {
                _sawmill.Error($"Server Rule group `Section` argument \"{sectionArg}\" is not a number");
                return false;
            }

            section = parsed;
        }

        args.TryGetValue("Group", out var group);

        var rules = _prototype.EnumeratePrototypes<ServerRulePrototype>()
            .Where(r => (section == null || r.Section == section) && (group == null || r.Group == group))
            .ToList();
        rules.Sort(ServerRulePrototype.CompareByCode);

        foreach (var rule in rules)
        {
            AddChild(new GuideServerRuleEmbed(rule));
        }

        control = this;
        return true;
    }
}
