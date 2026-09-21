using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// System that gives the speaker an alternate faux-French accent.
/// </summary>
public sealed class NeoFrenchAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexTh = new(@"th", RegexOptions.IgnoreCase);
    private static readonly Regex RegexH = new(@"h", RegexOptions.IgnoreCase);
    private static readonly Regex RegexSpacePunctuation = new(@"(?<=\w\w)[!?;:](?!\w)", RegexOptions.IgnoreCase);
    private static readonly Regex RegexI = new(@"i", RegexOptions.IgnoreCase);
    private static readonly Regex RegexS = new(@"s", RegexOptions.IgnoreCase);
    private static readonly Regex RegexR = new(@"r", RegexOptions.IgnoreCase);
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeoFrenchAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, NeoFrenchAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "neofrench");

        // replaces th with z
        msg = RegexTh.Replace(msg, "'z");

        // replaces h with '
        msg = RegexH.Replace(msg, "'");

        // spaces out ! ? : and ;.
        msg = RegexSpacePunctuation.Replace(msg, " $&");

        // Replaces i with e.
        msg = RegexI.Replace(msg, "e");

        // Replaces s with z.
        msg = RegexS.Replace(msg, "z");

        // Replaces r with rr.
        msg = RegexR.Replace(msg, "rr");

        return msg;
    }

    private void OnAccentGet(EntityUid uid, NeoFrenchAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
