using KSharpPlus.Net.Rest;

namespace KSharpPlus.Exceptions; 

public abstract class KuracordException : Exception {
    /// <summary>
    /// Gets the request that caused the exception.
    /// </summary>
    public virtual BaseRestRequest WebRequest { get; internal set; }

    /// <summary>
    /// Gets the response to the request.
    /// </summary>
    public virtual RestResponse WebResponse { get; internal set; }

    /// <summary>
    /// Gets the JSON message received.
    /// </summary>
    public virtual string JsonMessage { get; internal set; }

    public KuracordException() { }
    public KuracordException(string message) : base(message) { }
    public KuracordException(string message, Exception innerException) : base(message, innerException) { }
}