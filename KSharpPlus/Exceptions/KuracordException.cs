using KSharpPlus.Net.Rest;

namespace KSharpPlus.Exceptions; 

public abstract class KuracordException : Exception {
    /// <summary>
    /// Gets the request that caused the exception.
    /// </summary>
    public BaseRestRequest WebRequest { get; internal init; } = null!;

    /// <summary>
    /// Gets the response to the request.
    /// </summary>
    public RestResponse WebResponse { get; internal init; } = null!;

    /// <summary>
    /// Gets the JSON message received.
    /// </summary>
    public string JsonMessage { get; internal init; } = null!;

    public KuracordException() { }
    public KuracordException(string message) : base(message) { }
    public KuracordException(string message, Exception innerException) : base(message, innerException) { }
}