using Microsoft.Extensions.Logging;

namespace KSharpPlus.Logging;

/// <summary>
/// Contains well-defined event IDs used by core of KSharpPlus.
/// </summary>
public static class LoggerEvents {
    /// <summary>
    /// Miscellaneous events, that do not fit in any other category.
    /// </summary>
    public static EventId Misc { get; } = new(100, "KSharpPlus");

    /// <summary>
    /// Events pertaining to startup tasks.
    /// </summary>
    public static EventId Startup { get; } = new(101, nameof(Startup));

    /// <summary>
    /// Events typically emitted whenever WebSocket connections fail or are terminated.
    /// </summary>
    public static EventId ConnectionFailure { get; } = new(102, nameof(ConnectionFailure));

    /// <summary>
    /// Events pertaining to Kuracord-issued session state updates.
    /// </summary>
    public static EventId SessionUpdate { get; } = new(103, nameof(SessionUpdate));

    /// <summary>
    /// Events emitted when exceptions are thrown in handlers attached to async events.
    /// </summary>
    public static EventId EventHandlerException { get; } = new(104, nameof(EventHandlerException));

    /// <summary>
    /// Events emitted for various high-level WebSocket receive events.
    /// </summary>
    public static EventId WebSocketReceive { get; } = new(105, nameof(WebSocketReceive));

    /// <summary>
    /// Events emitted for various low-level WebSocket receive events.
    /// </summary>
    public static EventId WebSocketReceiveRaw { get; } = new(106, nameof(WebSocketReceiveRaw));

    /// <summary>
    /// Events emitted for various low-level WebSocket send events.
    /// </summary>
    public static EventId WebSocketSendRaw { get; } = new(107, nameof(WebSocketSendRaw));

    /// <summary>
    /// Events emitted for various WebSocket payload processing failures, typically when deserialization or decoding fails.
    /// </summary>
    public static EventId WebSocketReceiveFailure { get; } = new(108, nameof(WebSocketReceiveFailure));

    /// <summary>
    /// Events pertaining to connection lifecycle, specifically, heartbeats.
    /// </summary>
    public static EventId Heartbeat { get; } = new(109, nameof(Heartbeat));

    /// <summary>
    /// Events pertaining to various heartbeat failures, typically fatal.
    /// </summary>
    public static EventId HeartbeatFailure { get; } = new(110, nameof(HeartbeatFailure));

    /// <summary>
    /// Events pertaining to clean connection closes.
    /// </summary>
    public static EventId ConnectionClose { get; } = new(111, nameof(ConnectionClose));

    /// <summary>
    /// Events emitted when REST processing fails for any reason.
    /// </summary>
    public static EventId RestError { get; } = new(112, nameof(RestError));

    /// <summary>
    /// Events pertaining to ratelimit exhaustion.
    /// </summary>
    public static EventId RatelimitHit { get; } = new(114, nameof(RatelimitHit));

    /// <summary>
    /// Events pertaining to ratelimit diagnostics. Typically contain raw bucket info.
    /// </summary>
    public static EventId RatelimitDiag { get; } = new(115, nameof(RatelimitDiag));

    /// <summary>
    /// Events emitted when a ratelimit is exhausted and a request is preemptively blocked.
    /// </summary>
    public static EventId RatelimitPreemptive { get; } = new(116, nameof(RatelimitPreemptive));

    /// <summary>
    /// Events pertaining to audit log processing.
    /// </summary>
    public static EventId AuditLog { get; } = new(117, nameof(AuditLog));

    /// <summary>
    /// Events containing raw (but decompressed) payloads, received from Kuracord Gateway.
    /// </summary>
    public static EventId GatewayWsRx { get; } = new(118, "Gateway ↓");

    /// <summary>
    /// Events containing raw payloads, as they're being sent to Kuracord Gateway.
    /// </summary>
    public static EventId GatewayWsTx { get; } = new(119, "Gateway ↑");

    /// <summary>
    /// Events pertaining to Gateway Intents. Typically diagnostic information.
    /// </summary>
    public static EventId Intents { get; } = new(120, nameof(Intents));

    /// <summary>
    /// Events containing raw payloads, as they're received from Kuracord's REST API.
    /// </summary>
    public static EventId RestRx { get; } = new(123, "REST ↓");

    /// <summary>
    /// Events containing raw payloads, as they're sent to Kuracord's REST API.
    /// </summary>
    public static EventId RestTx { get; } = new(124, "REST ↑");

    public static EventId RestCleaner { get; } = new(125, nameof(RestCleaner));

    public static EventId RestHashMover { get; } = new(126, nameof(RestHashMover));
}