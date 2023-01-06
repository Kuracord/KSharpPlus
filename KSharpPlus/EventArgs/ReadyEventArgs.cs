using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.Ready"/> event.
/// </summary>
public sealed class ReadyEventArgs : KuracordEventArgs {
    internal ReadyEventArgs() { }
}