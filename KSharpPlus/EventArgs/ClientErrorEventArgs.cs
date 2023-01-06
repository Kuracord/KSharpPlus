namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.ClientErrored"/> event.
/// </summary>
public class ClientErrorEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the exception thrown by the client.
    /// </summary>
    public Exception Exception { get; internal set; }

    /// <summary>
    /// Gets the name of the event that threw the exception.
    /// </summary>
    public string EventName { get; internal set; }

    internal ClientErrorEventArgs() { }
}