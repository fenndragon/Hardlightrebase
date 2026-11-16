using System.Text.Json.Serialization;

namespace Content.Server.Discord;

public struct WebhookMentions
{
    [JsonPropertyName("parse")]
    public HashSet<string> Parse { get; set; } = new();

    [JsonPropertyName("roles")]
    public HashSet<string> Roles { get; set; } = new(); // WOLFGATE(Announcements): allow specific configured role mentions

    public WebhookMentions()
    {
    }

    public void AllowRoleMentions()
    {
        Parse.Add("roles");
    }
}
