using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Controls;
using Content.Client.Guidebook.Richtext;
using Content.Client.Message;
using Content.Client.UserInterface.ControlExtensions;
using Content.Shared._HL.Guidebook;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._HL.Guidebook;

/// <summary>
///     Control for displaying a single server rule.
/// </summary>
[UsedImplicitly]
public sealed class GuideServerRuleEmbed : BoxContainer, IDocumentTag, ISearchableControl
{
    private const string CodeColor = "#a4885c";
    private const string HardlitColor = "#00fffb";
    private const string ZeroToleranceColor = "#ff0000";

    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly ISawmill _sawmill;

    public GuideServerRuleEmbed()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _logManager.GetSawmill("guidebook.serverrules");
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        Margin = new Thickness(0, 0, 0, 8);
    }

    public GuideServerRuleEmbed(ServerRulePrototype rule) : this()
    {
        GenerateControl(rule);
    }

    public bool CheckMatchesSearch(string query)
    {
        return this.ChildrenContainText(query);
    }

    public void SetHiddenState(bool state, string query)
    {
        Visible = CheckMatchesSearch(query) ? state : !state;
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;
        if (!args.TryGetValue("Rule", out var id))
        {
            _sawmill.Error("Server Rule embed tag is missing `Rule` prototype argument");
            return false;
        }

        if (!_prototype.TryIndex<ServerRulePrototype>(id, out var rule))
        {
            _sawmill.Error($"Specified rule prototype \"{id}\" is not a valid ServerRule prototype");
            return false;
        }

        GenerateControl(rule);
        control = this;
        return true;
    }

    private void GenerateControl(ServerRulePrototype rule)
    {
        var text = GetTextColor(rule.Hardlit) is { } color
            ? $"[color={color}]{rule.Text}[/color]"
            : rule.Text;

        var header = new RichTextLabel { HorizontalExpand = true };
        header.SetMarkup($"[color={CodeColor}]{rule.Code}[/color] {text}");
        AddChild(header);

        if (rule.Description.Length == 0)
            return;

        var description = new RichTextLabel
        {
            HorizontalExpand = true,
            Margin = new Thickness(25, 4, 10, 0)
        };
        description.SetMarkup(rule.Description);
        AddChild(description);
    }

    private static string? GetTextColor(RuleType status)
    {
        return status switch
        {
            RuleType.Hardlit => HardlitColor,
            RuleType.ZeroTolerance => ZeroToleranceColor,
            _ => null
        };
    }
}
