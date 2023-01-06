using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel.Message;

namespace KSharpPlus.Net.Rest; 

/// <summary>
/// Represents a multipart HTTP request.
/// </summary>
internal sealed class MultipartWebRequest : BaseRestRequest {
    /// <summary>
    /// Gets the dictionary of values attached to this request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values { get; }

    /// <summary>
    /// Gets the dictionary of files attached to this request.
    /// </summary>
    public IReadOnlyCollection<KuracordMessageFile> Files { get; }

    internal bool _removeFileCount;

    internal MultipartWebRequest(BaseKuracordClient client, Uri url, RestRequestMethod method, 
        IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
        IReadOnlyCollection<KuracordMessageFile> files = null, bool removeFileCount = false) : base(client, url, method, headers) {
        Values = values;
        Files = files;
        _removeFileCount = removeFileCount;
    }
}