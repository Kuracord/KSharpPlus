using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Represents data for websocket identify payload.
/// </summary>
internal sealed class GatewayIdentify {
    /// <summary>
    /// Gets or sets the token used to identify the client to Kuracord.
    /// </summary>
    [JsonProperty("token")]
    public string Token { get; set; }
}