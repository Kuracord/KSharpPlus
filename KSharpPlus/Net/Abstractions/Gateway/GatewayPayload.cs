﻿using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Represents a websocket payload exchanged between Kuracord and the client.
/// </summary>
internal sealed class GatewayPayload {
    /// <summary>
    /// Gets or sets the OP code of the payload.
    /// </summary>
    [JsonProperty("op")]
    public GatewayOpCode OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data of the payload.
    /// </summary>
    [JsonProperty("d")]
    public object Data { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sequence number of the payload. Only present for OP 1.
    /// </summary>
    [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
    public int? Sequence { get; set; }

    /// <summary>
    /// Gets or sets the event name of the payload. Only present for OP 1.
    /// </summary>
    [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
    public string EventName { get; set; } = null!;
}