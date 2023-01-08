using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.Heartbeated"/> event.
/// </summary>
public class HeartbeatEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the round-trip time of the heartbeat.
    /// </summary>
    public int Ping { get; internal set; }
    
    /// <summary>
    /// Gets the timestamp of the heartbeat.
    /// </summary>
    public DateTimeOffset Timestamp { get; internal set; }

    internal HeartbeatEventArgs() { }
}