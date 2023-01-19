using KSharpPlus.Clients;

namespace KSharpPlus.Net.Rest; 

/// <summary>
/// Represents a non-multipart HTTP request.
/// </summary>
public class RestRequest : BaseRestRequest {
    /// <summary>
    /// Gets the payload sent with this request.
    /// </summary>
    public string? Payload { get; }
    
    internal RestRequest(BaseKuracordClient client, Uri url, RestRequestMethod method, IReadOnlyDictionary<string, string>? headers = null, string? payload = null) 
        : base(client, url, method, headers) {
        Payload = payload;
    }
}