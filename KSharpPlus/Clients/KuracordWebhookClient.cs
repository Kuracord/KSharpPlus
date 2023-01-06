using Microsoft.Extensions.Logging;

namespace KSharpPlus.Clients; 

public class KuracordWebhookClient {
    /// <summary>
    /// Gets the logger for this client.
    /// </summary>
    public ILogger<KuracordWebhookClient> Logger { get; }
    
    /// <summary>
    /// Gets or sets the username override for registered webhooks. Note that this only takes effect when broadcasting.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or set the avatar override for registered webhooks. Note that this only takes effect when broadcasting.
    /// </summary>
    public string AvatarUrl { get; set; }
    
    internal LogLevel _minimumLogLevel;
    internal string _logTimestampFormat;
}