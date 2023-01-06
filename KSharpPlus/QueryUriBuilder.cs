namespace KSharpPlus; 

internal class QueryUriBuilder {
    public Uri SourceUri { get; }

    public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => _queryParams;
    readonly List<KeyValuePair<string, string>> _queryParams = new();

    public QueryUriBuilder(string uri) {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        SourceUri = new Uri(uri);
    }

    public QueryUriBuilder(Uri uri) {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        SourceUri = uri;
    }

    public QueryUriBuilder AddParameter(string key, string value) {
        _queryParams.Add(new KeyValuePair<string, string>(key, value));
        return this;
    }

    public Uri Build() => new UriBuilder(SourceUri) {
        Query = string.Join("&", _queryParams.Select(e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)))
    }.Uri;

    public override string ToString() => Build().ToString();
}