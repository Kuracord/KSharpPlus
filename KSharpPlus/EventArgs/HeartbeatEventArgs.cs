using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.Heartbeated"/> event.
/// </summary>
public class HeartbeatEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the timestamp of the heartbeat.
    /// </summary>
    public DateTimeOffset Timestamp { get; internal set; }

    internal HeartbeatEventArgs() { }
}