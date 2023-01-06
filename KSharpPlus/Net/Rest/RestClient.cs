using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Exceptions;
using KSharpPlus.Logging;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Net.Rest; 

internal sealed class RestClient : IDisposable {
    #region Constructors

    internal RestClient(BaseKuracordClient client) 
        : this(client.Configuration.Proxy, client.Configuration.HttpTimeout, client.Configuration.UseRelativeRatelimit, client.Logger) {
        Kuracord = client;
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));
    }

    // This is for meta-clients, such as the webhook client
    internal RestClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRatelimit, ILogger logger) { 
        Logger = logger;

        HttpClientHandler httphandler = new() {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseProxy = proxy != null,
            Proxy = proxy
        };

        HttpClient = new HttpClient(httphandler) {
            BaseAddress = new Uri(Utilities.GetApiBaseUri()),
            Timeout = timeout
        };

        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        RoutesToHashes = new ConcurrentDictionary<string, string>();
        RequestQueue = new ConcurrentDictionary<string, int>();

        UseResetAfter = useRelativeRatelimit;
    }

    #endregion
    
    #region Fields and Properties

    static Regex RouteArgumentRegex { get; } = new(@":([a-z_]+)");
    HttpClient HttpClient { get; }
    BaseKuracordClient Kuracord { get; }
    ILogger Logger { get; }
    ConcurrentDictionary<string, string> RoutesToHashes { get; }
    ConcurrentDictionary<string, int> RequestQueue { get; }
    bool UseResetAfter { get; }

    CancellationTokenSource _bucketCleanerTokenSource;
    volatile bool _cleanerRunning;
    Task _cleanerTask;
    volatile bool _disposed;

    #endregion

    #region Methods

    public async Task ExecuteRequestAsync(BaseRestRequest request) {
        if (_disposed) return;

        HttpResponseMessage res = default;

        try {
            HttpRequestMessage req = BuildRequest(request);
            RestResponse response = new();

            try {
                if (_disposed) return;

                res = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

                byte[] bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                string txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

                Logger.LogTrace(LoggerEvents.RestRx, txt);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            } catch (HttpRequestException e) {
                Logger.LogError(LoggerEvents.RestError, e, $"Request to {request.Url} triggered an HttpException");
                request.SetFaulted(e);
                return;
            }

            Exception ex = null;

            switch (response.ResponseCode) {
                case 400:
                case 405:
                    ex = new BadRequestException(request, response);
                    break;

                case 401:
                case 403:
                    ex = new UnauthorizedException(request, response);
                    break;

                case 404:
                    ex = new NotFoundException(request, response);
                    break;

                case 413:
                    ex = new RequestSizeException(request, response);
                    break;

                case 500:
                case 502:
                case 503:
                case 504:
                    ex = new ServerErrorException(request, response);
                    break;
            }

            if (ex != null)
                request.SetFaulted(ex);
            else
                request.SetCompleted(response);
        } catch (Exception ex) {
            Logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} triggered an exception", request.Url);
            if (!request.TrySetFaulted(ex)) throw;
        } finally { res?.Dispose(); }
    }

    HttpRequestMessage BuildRequest(BaseRestRequest request) {
        HttpRequestMessage req = new(new HttpMethod(request.Method.ToString()), request.Url);

        if (request.Headers != null && request.Headers.Any())
            foreach (KeyValuePair<string, string> kvp in request.Headers)
                req.Headers.Add(kvp.Key, kvp.Value);

        switch (request) {
            case RestRequest nmprequest when !string.IsNullOrWhiteSpace(nmprequest.Payload):
                Logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

                req.Content = new StringContent(nmprequest.Payload);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                break;

            case MultipartWebRequest mprequest: {
                Logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

                if (mprequest.Values.TryGetValue("payload_json", out string? payload))
                    Logger.LogTrace(LoggerEvents.RestTx, payload);

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                req.Headers.Add("Connection", "keep-alive");
                req.Headers.Add("Keep-Alive", "600");

                MultipartFormDataContent content = new(boundary);

                if (mprequest.Values != null && mprequest.Values.Any())
                    foreach (KeyValuePair<string, string> kvp in mprequest.Values)
                        content.Add(new StringContent(kvp.Value), kvp.Key);

                if (mprequest.Files != null && mprequest.Files.Any()) {
                    int i = 1;

                    foreach (KuracordMessageFile f in mprequest.Files) {
                        StreamContent sc = new(f.Stream);

                        if (f.ContentType != null) sc.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);
                        if (f.FileType != null) f.FileName += '.' + f.FileType;

                        string count = !mprequest._removeFileCount ? i++.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        content.Add(sc, $"file{count}", f.FileName);
                    }
                }

                req.Content = content;
                break;
            }
        }

        return req;
    }

    void Handle429(RestResponse response, out Task waitTask, out bool global) {
        waitTask = null;
        global = false;

        if (response.Headers == null) return;

        IReadOnlyDictionary<string, string> hs = response.Headers;

        if (hs.TryGetValue("Retry-After", out string? retryAfterRaw)) {
            TimeSpan retryAfter = TimeSpan.FromSeconds(int.Parse(retryAfterRaw, CultureInfo.InvariantCulture));
            waitTask = Task.Delay(retryAfter);
        }

        if (hs.TryGetValue("X-RateLimit-Global", out string? isGlobal) && isGlobal.Equals("true", StringComparison.InvariantCultureIgnoreCase)) global = true;
    }

    #endregion

    #region Utils

    ~RestClient() => Dispose();

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;

        if (_bucketCleanerTokenSource.IsCancellationRequested == false) {
            _bucketCleanerTokenSource.Cancel();
            Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
        }

        try {
            _cleanerTask.Dispose();
            _bucketCleanerTokenSource.Dispose();
            HttpClient.Dispose();
        } catch {/**/}

        RoutesToHashes.Clear();
        RequestQueue.Clear();
    }

    #endregion
}