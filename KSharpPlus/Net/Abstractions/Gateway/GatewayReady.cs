using KSharpPlus.Entities.User;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Represents data for a websocket ready payload.
/// </summary>
internal sealed class GatewayReady {
    /// <summary>
    /// Gets the current session's ID.
    /// </summary>
    [JsonProperty("sessionId")] public string SessionId { get; private set; } = null!;
    
    /// <summary>
    /// Gets the current user.
    /// </summary>
    [JsonProperty("user")] public KuracordUser CurrentUser { get; private set; } = null!;
}