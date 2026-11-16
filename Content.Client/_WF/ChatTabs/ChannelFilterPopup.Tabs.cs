using Content.Shared.Chat;
using Robust.Shared.Utility; // WOLFGATE(ChatTabs)

namespace Content.Client.UserInterface.Systems.Chat.Controls;

// The filter checkboxes mirror the selected chat tab; see Content.Client/_WF/ChatTabs.
public sealed partial class ChannelFilterPopup
{
    /// <summary>
    /// Every channel a chat box can show, in display order.
    /// </summary>
    public static IReadOnlyList<ChatChannel> WfChannels => ChannelFilterOrder;

    /// <summary>
    /// Sets the checkboxes to a chat tab's channels without raising <see cref="OnChannelFilter"/>.
    /// </summary>
    public void WfSetActive(ChatChannel channels)
    {
        foreach (var (channel, checkbox) in _filterStates)
        {
            checkbox.Pressed = (channels & channel) != 0;
        }
    }

    /// <summary>
    /// Displays generated name and job keywords separately from custom highlights.
    /// </summary>
    public void UpdateAutoFill(string autoFillKeywords)
    {
        AutoFillVBox.Visible = !string.IsNullOrEmpty(autoFillKeywords);
        if (string.IsNullOrEmpty(autoFillKeywords))
            return;

        var message = new FormattedMessage();
        var lines = autoFillKeywords.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                message.PushNewline();

            message.AddText(lines[i]);
        }

        AutoFillLabel.SetMessage(message);
    }
}
