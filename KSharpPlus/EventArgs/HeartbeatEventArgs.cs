using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.Heartbeated"/> event.
/// </summary>
public class HeartbeatEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the round-trip time of the heartbeat.
    /// </summary>
    public int Ping { get; }
    
    /// <summary>
    /// Gets the timestamp of the heartbeat.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    internal HeartbeatEventArgs(int ping, DateTimeOffset timestamp) {
        Ping = ping;
        Timestamp = timestamp;
    }
}