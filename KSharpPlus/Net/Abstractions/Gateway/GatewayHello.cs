using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Represents data for a websocket hello payload.
/// </summary>
internal sealed class GatewayHello {
    /// <summary>
    /// Gets the target heartbeat interval (in milliseconds) requested by Kuracord.
    /// </summary>
    [JsonProperty("heartbeat")]
    public int HeartbeatInterval { get; private set; }
}