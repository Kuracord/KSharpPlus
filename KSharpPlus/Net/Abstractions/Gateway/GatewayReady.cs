using KSharpPlus.Net.Abstractions.Transport;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Represents data for a websocket ready payload.
/// </summary>
internal sealed class GatewayReady {
    /// <summary>
    /// Gets the current session's ID.
    /// </summary>
    [JsonProperty("sessionId")]
    public string SessionId { get; private set; }
    
    /// <summary>
    /// Gets the current user.
    /// </summary>
    [JsonProperty("user")]
    public TransportUser CurrentUser { get; private set; }
}