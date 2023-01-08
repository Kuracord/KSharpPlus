using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="KuracordClient.UnknownEvent"/> event.
/// </summary>
public class UnknownEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the event's name.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the event's data.
    /// </summary>
    public string Json { get; }

    internal UnknownEventArgs(string eventName, string json) {
        EventName = eventName;
        Json = json;
    }
}