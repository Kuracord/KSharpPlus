using System.Collections.ObjectModel;
using System.Net;
using System.Net.WebSockets;
using Emzi0767.Utilities;
using KSharpPlus.EventArgs.Socket;

namespace KSharpPlus.Net.WebSocket;

public class WebSocketClient : IWebSocketClient {
    const int OutgoingChunkSize = 8192; // 8 KiB
    const int IncomingChunkSize = 32768; // 32 KiB

    public IWebProxy Proxy { get; }

    public IReadOnlyDictionary<string, string> DefaultHeaders { get; }
    readonly Dictionary<string, string>? _defaultHeaders;

    Task _receiverTask;
    CancellationTokenSource? _receiverTokenSource;
    CancellationToken _receiverToken;
    readonly SemaphoreSlim _senderLock;

    CancellationTokenSource? _socketTokenSource;
    CancellationToken _socketToken;
    ClientWebSocket? _ws;

    volatile bool _isClientClose;
    volatile bool _isConnected;
    bool _isDisposed;

    /// <summary>
    /// Instantiates a new WebSocket client with specified proxy settings.
    /// </summary>
    /// <param name="proxy">Proxy settings for the client.</param>
    WebSocketClient(IWebProxy proxy) {
        _connected = new AsyncEvent<WebSocketClient, SocketEventArgs>("WS_CONNECT", TimeSpan.Zero, EventErrorHandler);
        _disconnected = new AsyncEvent<WebSocketClient, SocketCloseEventArgs>("WS_DISCONNECT", TimeSpan.Zero, EventErrorHandler);
        _messageReceived = new AsyncEvent<WebSocketClient, SocketMessageEventArgs>("WS_MESSAGE", TimeSpan.Zero, EventErrorHandler);
        _exceptionThrown = new AsyncEvent<WebSocketClient, SocketErrorEventArgs>("WS_ERROR", TimeSpan.Zero, null);

        Proxy = proxy;
        _defaultHeaders = new Dictionary<string, string>();
        DefaultHeaders = new ReadOnlyDictionary<string, string>(_defaultHeaders);

        _receiverTokenSource = null;
        _receiverToken = CancellationToken.None;
        _senderLock = new SemaphoreSlim(1);

        _socketTokenSource = null;
        _socketToken = CancellationToken.None;
    }

    public async Task ConnectAsync(Uri uri) {
        // Disconnect first
        try { await DisconnectAsync().ConfigureAwait(false); } catch {/**/}

        // Disallow sending messages
        await _senderLock.WaitAsync(_receiverToken).ConfigureAwait(false);

        try {
            // This can be null at this point
            _receiverTokenSource?.Dispose();
            _socketTokenSource?.Dispose();

            _ws?.Dispose();
            _ws = new ClientWebSocket();
            _ws.Options.Proxy = Proxy;
            _ws.Options.KeepAliveInterval = TimeSpan.Zero;

            if (_defaultHeaders != null && _defaultHeaders.Any())
                foreach ((string k, string v) in _defaultHeaders)
                    _ws.Options.SetRequestHeader(k, v);

            _receiverTokenSource = new CancellationTokenSource();
            _receiverToken = _receiverTokenSource.Token;

            _socketTokenSource = new CancellationTokenSource();
            _socketToken = _socketTokenSource.Token;

            _isClientClose = false;
            _isDisposed = false;
            await _ws.ConnectAsync(uri, _socketToken).ConfigureAwait(false);
            _receiverTask = Task.Run(ReceiverLoopAsync, _receiverToken);
        } finally { _senderLock.Release(); }
    }

    public async Task DisconnectAsync(int code = 1000, string message = "") {
        // Ensure that messages cannot be sent
        await _senderLock.WaitAsync(_receiverToken).ConfigureAwait(false);

        try {
            _isClientClose = true;

            if (_ws is { State: WebSocketState.Open or WebSocketState.CloseReceived }) await _ws.CloseOutputAsync((WebSocketCloseStatus)code, message, CancellationToken.None).ConfigureAwait(false);
            if (_receiverTask != null) await _receiverTask.ConfigureAwait(false); // Ensure that receiving completed
            if (_isConnected) _isConnected = false;

            if (!_isDisposed) {
                // Cancel all running tasks
                if (_socketToken.CanBeCanceled) _socketTokenSource?.Cancel();
                _socketTokenSource?.Dispose();

                if (_receiverToken.CanBeCanceled) _receiverTokenSource?.Cancel();
                _receiverTokenSource?.Dispose();

                _isDisposed = true;
            }
        } catch {/**/} finally { _senderLock.Release(); }
    }

    public async Task SendMessageAsync(string message) {
        if (_ws == null) return;
        if (_ws.State != WebSocketState.Open && _ws.State != WebSocketState.CloseReceived) return;

        byte[] bytes = Utilities.UTF8.GetBytes(message);
        await _senderLock.WaitAsync(_receiverToken).ConfigureAwait(false);

        try {
            int len = bytes.Length;
            int segCount = len / OutgoingChunkSize;

            if (len % OutgoingChunkSize != 0) segCount++;

            for (int i = 0; i < segCount; i++) {
                int segStart = OutgoingChunkSize * i;
                int segLen = Math.Min(OutgoingChunkSize, len - segStart);

                await _ws.SendAsync(new ArraySegment<byte>(bytes, segStart, segLen), WebSocketMessageType.Text, i == segCount - 1, CancellationToken.None).ConfigureAwait(false);
            }
        } finally { _senderLock.Release(); }
    }

    public bool AddDefaultHeader(string name, string value) {
        _defaultHeaders[name] = value;
        return true;
    }

    public bool RemoveDefaultHeader(string name) => _defaultHeaders.Remove(name);

    /// <summary>
    /// Disposes of resources used by this WebSocket client instance.
    /// </summary>
    public void Dispose() {
        if (_isDisposed) return;
        _isDisposed = true;

        DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        _receiverTokenSource?.Dispose();
        _socketTokenSource?.Dispose();
    }

    internal async Task ReceiverLoopAsync() {
        await Task.Yield();

        CancellationToken token = _receiverToken;
        ArraySegment<byte> buffer = new(new byte[IncomingChunkSize]);

        try {
            using MemoryStream bs = new();

            while (!token.IsCancellationRequested) {
                // See https://github.com/RogueException/Kuracord.Net/commit/ac389f5f6823e3a720aedd81b7805adbdd78b66d
                // for explanation on the cancellation token

                WebSocketReceiveResult result;

                do {
                    result = await _ws.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    bs.Write(buffer.Array, 0, result.Count);
                } while (!result.EndOfMessage);

                byte[] resultBytes = new byte[bs.Length];
                bs.Position = 0;
                bs.Read(resultBytes, 0, resultBytes.Length);
                bs.Position = 0;
                bs.SetLength(0);

                if (!_isConnected && result.MessageType != WebSocketMessageType.Close) {
                    _isConnected = true;
                    await _connected.InvokeAsync(this, new SocketEventArgs()).ConfigureAwait(false);
                }

                if (result.MessageType == WebSocketMessageType.Binary) {
                    await _messageReceived.InvokeAsync(this, new SocketBinaryMessageEventArgs(resultBytes)).ConfigureAwait(false);
                } else if (result.MessageType == WebSocketMessageType.Text) {
                    await _messageReceived.InvokeAsync(this, new SocketTextMessageEventArgs(Utilities.UTF8.GetString(resultBytes))).ConfigureAwait(false);
                } else {
                    if (!_isClientClose) {
                        WebSocketCloseStatus code = result.CloseStatus.Value;

                        code = code is WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable
                            ? (WebSocketCloseStatus)4000
                            : code;

                        await _ws.CloseOutputAsync(code, result.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
                    }

                    await _disconnected.InvokeAsync(this, new SocketCloseEventArgs { CloseCode = (int)result.CloseStatus, CloseMessage = result.CloseStatusDescription }).ConfigureAwait(false);

                    break;
                }
            }
        } catch (Exception ex) {
            await _exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs { Exception = ex }).ConfigureAwait(false);
            await _disconnected.InvokeAsync(this, new SocketCloseEventArgs { CloseCode = -1, CloseMessage = "" }).ConfigureAwait(false);
        }

        // Don't await or you deadlock
        // DisconnectAsync waits for this method
        DisconnectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new instance of <see cref="WebSocketClient" />.
    /// </summary>
    /// <param name="proxy">Proxy to use for this client instance.</param>
    /// <returns>An instance of <see cref="WebSocketClient" />.</returns>
    public static IWebSocketClient CreateNew(IWebProxy proxy) => new WebSocketClient(proxy);

    #region Events

    /// <summary>
    /// Triggered when the client connects successfully.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketEventArgs> Connected {
        add => _connected.Register(value);
        remove => _connected.Unregister(value);
    }

    readonly AsyncEvent<WebSocketClient, SocketEventArgs> _connected;

    /// <summary>
    /// Triggered when the client is disconnected.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketCloseEventArgs> Disconnected {
        add => _disconnected.Register(value);
        remove => _disconnected.Unregister(value);
    }

    readonly AsyncEvent<WebSocketClient, SocketCloseEventArgs> _disconnected;

    /// <summary>
    /// Triggered when the client receives a message from the remote party.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs> MessageReceived {
        add => _messageReceived.Register(value);
        remove => _messageReceived.Unregister(value);
    }

    readonly AsyncEvent<WebSocketClient, SocketMessageEventArgs> _messageReceived;

    /// <summary>
    /// Triggered when an error occurs in the client.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs> ExceptionThrown {
        add => _exceptionThrown.Register(value);
        remove => _exceptionThrown.Unregister(value);
    }

    readonly AsyncEvent<WebSocketClient, SocketErrorEventArgs> _exceptionThrown;

    void EventErrorHandler<TArgs>(AsyncEvent<WebSocketClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<WebSocketClient, TArgs> handler, WebSocketClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
        => _exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();

    #endregion
}