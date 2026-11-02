using Content.Shared.Administration.Logs;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.CustomControls;

public sealed class AdminLogLabel : RichTextLabel
{
    public AdminLogLabel(ref SharedAdminLog log, HSeparator separator)
    {
        Log = log;
        Separator = separator;

        var localTime = log.Date.ToLocalTime();

        SetMessage($"{localTime:HH:mm:ss}: {log.Message}");
    }

    public SharedAdminLog Log { get; }

    public HSeparator Separator { get; }

    private void VisibilityChanged(Control control)
    {
        Separator.Visible = Visible;
    }

    // HL START: Move Event Listeners in ExitedTree from Constructor to EnteredTree so they're re-registered correctly
    protected override void EnteredTree()
    {
        base.EnteredTree();
        OnVisibilityChanged += VisibilityChanged;
    }
    // HL END

    protected override void ExitedTree()
    {
        base.ExitedTree();

        OnVisibilityChanged -= VisibilityChanged;
    }
}
