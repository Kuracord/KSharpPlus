using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs.Socket; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.SocketClosed"/> event.
/// </summary>
public class SocketCloseEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the close code sent by remote host.
    /// </summary>
    public int CloseCode { get; }

    /// <summary>
    /// Gets the close message sent by remote host.
    /// </summary>
    public string CloseMessage { get; }

    internal SocketCloseEventArgs(int closeCode, string closeMessage) {
        CloseCode = closeCode;
        CloseMessage = closeMessage;
    }
}

/// <summary>
/// Represents arguments for <see cref="KuracordClient.SocketErrored"/> event.
/// </summary>
public class SocketErrorEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the exception thrown by websocket client.
    /// </summary>
    public Exception Exception { get; }

    internal SocketErrorEventArgs(Exception exception) => Exception = exception;
}