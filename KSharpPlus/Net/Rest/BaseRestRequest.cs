using KSharpPlus.Clients;

namespace KSharpPlus.Net.Rest;

/// <summary>
/// Represents a request sent over HTTP.
/// </summary>
public abstract class BaseRestRequest {
    protected internal BaseKuracordClient Kuracord { get; }
    protected internal TaskCompletionSource<RestResponse> RequestTaskSource { get; }

    /// <summary>
    /// Gets the url to which this request is going to be made.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the HTTP method used for this request.
    /// </summary>
    public RestRequestMethod Method { get; }

    /// <summary>
    /// Gets the headers sent with this request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; } = null!;

    /// <summary>
    /// Creates a new <see cref="BaseRestRequest" /> with specified parameters.
    /// </summary>
    /// <param name="client"><see cref="KuracordClient" /> from which this request originated.</param>
    /// <param name="url">Uri to which this request is going to be sent to.</param>
    /// <param name="method">Method to use for this request,</param>
    /// <param name="headers">Additional headers for this request.</param>
    internal BaseRestRequest(BaseKuracordClient client, Uri url, RestRequestMethod method, IReadOnlyDictionary<string, string>? headers = null) {
        Kuracord = client;
        RequestTaskSource = new TaskCompletionSource<RestResponse>();
        Url = url;
        Method = method;

        if (headers == null) return;

        headers = headers.Select(x => new KeyValuePair<string, string>(x.Key, Uri.EscapeDataString(x.Value)))
            .ToDictionary(x => x.Key, x => x.Value);

        Headers = headers; 
    }

    /// <summary>
    /// Asynchronously waits for this request to complete.
    /// </summary>
    /// <returns>HTTP response to this request.</returns>
    public Task<RestResponse> WaitForCompletionAsync() => RequestTaskSource.Task;
    protected internal void SetCompleted(RestResponse response) => RequestTaskSource.SetResult(response);
    protected internal void SetFaulted(Exception ex) => RequestTaskSource.SetException(ex);
    protected internal bool TrySetFaulted(Exception ex) => RequestTaskSource.TrySetException(ex);
}