using System.Collections.Concurrent;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;
using KSharpPlus.EventArgs;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.EventArgs.Socket;
using KSharpPlus.Logging;
using KSharpPlus.Net.Abstractions.Gateway;
using KSharpPlus.Net.Serialization;
using KSharpPlus.Net.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Clients; 

public sealed partial class KuracordClient {
    #region Fields and Properties

    int _heartbeatInterval;
    DateTimeOffset _lastHeartbeat;
    Task _heartbeatTask;

    int _skippedHeartbeats;
    long _lastSequence;

    internal IWebSocketClient _webSocketClient;
    PayloadDecompressor _payloadDecompressor;

    CancellationTokenSource? _cancelTokenSource;
    CancellationToken _cancelToken;

    #endregion
    
    #region Connection Semaphore

    static ConcurrentDictionary<ulong, SocketLock> SocketLocks { get; } = new();
    ManualResetEventSlim SessionLock { get; } = new(true);

    #endregion

    #region Internal Connection Methods

    Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "") {
        if (startNewSession) _sessionId = null!;

        _webSocketClient.DisconnectAsync(code, message);
        return Task.CompletedTask;
    }

    /* GATEWAY VERSION IS IN THIS METHOD!! If you need to update the Gateway Version, look for gwuri ~Velvet */
    internal async Task InternalConnectAsync() {
        SocketLock? socketLock = null;

        try {
            await InitializeAsync().ConfigureAwait(false);

            socketLock = GetSocketLock();
            await socketLock.LockAsync().ConfigureAwait(false);
        } catch {
            socketLock?.UnlockAfter(TimeSpan.Zero);
            throw;
        }

        Volatile.Write(ref _skippedHeartbeats, 0);

        _webSocketClient = Configuration.WebSocketClientFactory(Configuration.Proxy);

        _payloadDecompressor = new PayloadDecompressor();

        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;

        _webSocketClient.Connected += SocketOnConnect;
        _webSocketClient.Disconnected += SocketOnDisconnect;
        _webSocketClient.MessageReceived += SocketOnMessage;
        _webSocketClient.ExceptionThrown += SocketOnException;

        QueryUriBuilder gatewayUri = new(GatewayUri);

        await _webSocketClient.ConnectAsync(gatewayUri.Build()).ConfigureAwait(false);

        Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e) => _socketOpened.InvokeAsync(this, e);

        async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e) {
            string msg = null!;

            switch (e) {
                case SocketTextMessageEventArgs etext:
                    msg = etext.Message;
                    break;

                case SocketBinaryMessageEventArgs ebin: {
                    using MemoryStream ms = new();

                    if (!_payloadDecompressor.TryDecompress(new ArraySegment<byte>(ebin.Message), ms)) {
                        Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
                        return;
                    }

                    ms.Position = 0;
                    using StreamReader sr = new(ms, Utilities.UTF8);
                    msg = await sr.ReadToEndAsync(_cancelToken).ConfigureAwait(false);
                    break;
                }
            }

            try {
                Logger.LogTrace(LoggerEvents.GatewayWsRx, msg);
                await HandleSocketMessageAsync(msg).ConfigureAwait(false);
            } catch (Exception ex) {
                Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Socket handler suppressed an exception");
            }
        }

        Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e) => _socketErrored.InvokeAsync(this, e);

        async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e) {
            ConnectionLock.Set();
            SessionLock.Set();

            if (!_disposed) _cancelTokenSource.Cancel();

            Logger.LogDebug(LoggerEvents.ConnectionClose, $"Connection closed ({e.CloseCode}, '{e.CloseMessage}')");
            await _socketClosed.InvokeAsync(this, e).ConfigureAwait(false);

            if (Configuration.AutoReconnect && e.CloseCode is < 4001 or >= 5000) {
                Logger.LogCritical(LoggerEvents.ConnectionClose, $"Connection terminated ({e.CloseCode}, '{e.CloseMessage}'), reconnecting");

                await ConnectAsync().ConfigureAwait(false);
            } else Logger.LogInformation(LoggerEvents.ConnectionClose, $"Connection terminated ({e.CloseCode}, '{e.CloseMessage}')");
        }
    }

    #endregion

    #region WebSocket (Events)

    internal async Task HandleSocketMessageAsync(string data) {
        GatewayPayload? payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
        _lastSequence = payload?.Sequence ?? _lastSequence;
        switch (payload?.OpCode) {
            case GatewayOpCode.Dispatch:
                await HandleDispatchAsync(payload).ConfigureAwait(false);
                break;

            case GatewayOpCode.Hello:
                await OnHelloAsync((payload.Data as JObject)!.ToKuracordObject<GatewayHello>()).ConfigureAwait(false);
                break;
            
            case GatewayOpCode.Ready:
                await OnReadyAsync((payload.Data as JObject)!.ToKuracordObject<GatewayReady>()).ConfigureAwait(false);
                break;
            
            case GatewayOpCode.HeartbeatAck:
                await OnHeartbeatAckAsync().ConfigureAwait(false);
                break;

            default:
                Logger.LogWarning(LoggerEvents.WebSocketReceive, $"Unknown Kuracord opcode: {payload!.OpCode}\nPayload: {payload.Data}");
                break;
        }
    }

    internal async Task OnHelloAsync(GatewayHello hello) {
        Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HELLO (OP4)");
        
        if (SessionLock.Wait(0)) {
            SessionLock.Reset();
            GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
        } else {
            Logger.LogWarning(LoggerEvents.SessionUpdate, "Attempt to start a session while another session is active");
            return;
        }

        Interlocked.CompareExchange(ref _skippedHeartbeats, 0, 0);
        _heartbeatInterval = hello.HeartbeatInterval;
        _heartbeatTask = Task.Run(HeartbeatLoopAsync, _cancelToken);

        await SendIdentifyAsync().ConfigureAwait(false); //todo: resume
    }

    internal async Task OnReadyAsync(GatewayReady ready) {
        KuracordUser readyUser = ready.CurrentUser;
        CurrentUser.Username = readyUser.Username;
        CurrentUser.Discriminator = readyUser.Discriminator;
        CurrentUser.AvatarUrl = readyUser.AvatarUrl;
        CurrentUser.Disabled = readyUser.Disabled;
        CurrentUser.Verified = readyUser.Verified;
        CurrentUser.IsBot = readyUser.IsBot;
        CurrentUser.Biography = readyUser.Biography;
        CurrentUser.Email = readyUser.Email;
        CurrentUser.Flags = readyUser.Flags;
        CurrentUser.GuildsMember = readyUser.GuildsMember;
        CurrentUser.PremiumType = readyUser.PremiumType;
        CurrentUser.Id = readyUser.Id;

        _sessionId = ready.SessionId;
        
        _guilds.Clear();

        foreach (KuracordMember gMember in readyUser.GuildsMember!) {
            KuracordGuild guild = await GetGuildAsync(gMember.Guild.Id);
            guild.Kuracord = this;
            
            gMember.Kuracord = this;
            gMember.Guild = guild;
            gMember._guildId = gMember.Guild.Id;

            foreach (KuracordChannel channel in gMember.Guild.Channels.Values) {
                channel.Kuracord = this;
                channel.GuildId = gMember.Guild.Id;
            }

            foreach (KuracordRole role in gMember.Guild.Roles.Values) {
                role.Kuracord = this;
                role._guildId = gMember.Guild.Id;
            }

            foreach (KuracordMember member in gMember.Guild.Members.Values) {
                member.Kuracord = this;
                member._guildId = gMember.Guild.Id;
            }

            _guilds[gMember.Guild.Id] = gMember.Guild;

            await _guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(gMember.Guild)).ConfigureAwait(false);
        }

        Volatile.Write(ref _guildDownloadCompleted, true);
        await _guildDownloadCompletedEvent.InvokeAsync(this, new GuildDownloadCompletedEventArgs(Guilds)); 
        
        await _ready.InvokeAsync(this, new ReadyEventArgs()).ConfigureAwait(false);
    }

    internal async Task OnHeartbeatAckAsync() {
        int ping = (int)(DateTime.Now - _lastHeartbeat).TotalMilliseconds;
        
        Interlocked.Decrement(ref _skippedHeartbeats);
        
        Logger.LogTrace(LoggerEvents.WebSocketReceive, $"Received HEARTBEAT_ACK (OP6, {ping}ms)");
        
        Volatile.Write(ref _ping, ping);

        HeartbeatEventArgs args = new(Ping, DateTimeOffset.Now);

        await _heartbeated.InvokeAsync(this, args).ConfigureAwait(false);
    }
    
    internal async Task HeartbeatLoopAsync() {
        Logger.LogDebug(LoggerEvents.Heartbeat, "Heartbeat task started");
        
        CancellationToken token = _cancelToken;

        try {
            while (true) {
                await SendHeartbeatAsync(_lastSequence).ConfigureAwait(false);
                await Task.Delay(_heartbeatInterval, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }
        } catch (OperationCanceledException) {/**/}
    }

    #endregion
    
    #region Internal Gateway Methods

    internal async Task SendHeartbeatAsync(long sequence) {
        bool moreThan5 = Volatile.Read(ref _skippedHeartbeats) > 5;
        bool guildsDownloadCompleted = Volatile.Read(ref _guildDownloadCompleted);

        if (moreThan5 && guildsDownloadCompleted) {
            Logger.LogCritical(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats - connection is zombie");

            ZombiedEventArgs args = new(Volatile.Read(ref _skippedHeartbeats), true);
            
            await _zombied.InvokeAsync(this, args).ConfigureAwait(false);
            await InternalReconnectAsync(false, 4001, "Too many heartbeats missed").ConfigureAwait(false);

            return;
        }

        if (!guildsDownloadCompleted && moreThan5) {
            ZombiedEventArgs args = new(Volatile.Read(ref _skippedHeartbeats), false);
            
            await _zombied.InvokeAsync(this, args).ConfigureAwait(false);
            Logger.LogWarning(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
        }
        
        Volatile.Write(ref _lastSequence, sequence);
        Logger.LogTrace(LoggerEvents.Heartbeat, "Sending heartbeat");
        
        GatewayPayload payload = new() { OpCode = GatewayOpCode.Heartbeat };
        string heartbeatString = JsonConvert.SerializeObject(payload);
        
        await WsSendAsync(heartbeatString).ConfigureAwait(false);

        _lastHeartbeat = DateTimeOffset.Now;

        Interlocked.Increment(ref _skippedHeartbeats);
    }

    internal async Task SendIdentifyAsync() {
        GatewayIdentify identify = new() {
            Token = Utilities.GetFormattedToken(this)
        };

        GatewayPayload payload = new() {
            OpCode = GatewayOpCode.Identify,
            Data = identify
        };

        string payloadString = JsonConvert.SerializeObject(payload);
        await WsSendAsync(payloadString).ConfigureAwait(false);
    }

    internal async Task WsSendAsync(string payload) {
        Logger.LogTrace(LoggerEvents.GatewayWsTx, payload);
        await _webSocketClient.SendMessageAsync(payload).ConfigureAwait(false);
    }

    #endregion
    
    #region Semaphore Methods

    SocketLock GetSocketLock() => SocketLocks.GetOrAdd(CurrentUser.Id, _ => new SocketLock(1));

    #endregion
}