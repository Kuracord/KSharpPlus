namespace KSharpPlus.Net.Abstractions.Gateway; 

/// <summary>
/// Specifies an OP code in a gateway payload.
/// </summary>
public enum GatewayOpCode {
    /// <summary>
    /// Used for initial handshake with the gateway.
    /// </summary>
    Identify = 0,
    
    /// <summary>
    /// Used for dispatching events.
    /// </summary>
    Dispatch = 1,
    
    /// <summary>
    /// Used to resume a closed connection.
    /// </summary>
    Resume = 2,
    
    /// <summary>
    /// Used when identify is successful
    /// </summary>
    Ready = 3,
    
    /// <summary>
    /// Used by the gateway upon connecting.
    /// </summary>
    Hello = 4,
    
    /// <summary>
    /// Used for pinging the gateway to ensure the connection is still alive.
    /// </summary>
    Heartbeat = 5,
    
    /// <summary>
    /// Used to acknowledge a heartbeat.
    /// </summary>
    HeartbeatAck = 6
}