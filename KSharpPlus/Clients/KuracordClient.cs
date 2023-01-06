using System.Collections.Concurrent;
using Emzi0767.Utilities;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;
using KSharpPlus.EventArgs;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.EventArgs.Socket;
using KSharpPlus.Exceptions;
using KSharpPlus.Logging;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Clients;

public sealed partial class KuracordClient : BaseKuracordClient {
    #region Internal Fields and Properties

    ManualResetEventSlim ConnectionLock { get; } = new(true); 

    #endregion

    #region Public Fields and Properties

    public static Uri GatewayUri => new("wss://gateway.kuracord.tk/v3");

    /// <summary>
    /// Gets a list of guilds that this client is in. Note that the
    /// guild objects in this list will not be filled in if the specific guilds aren't available (the
    /// <see cref="GuildAvailable"/> or <see cref="GuildDownloadCompleted"/> events haven't been fired yet)
    /// </summary>
    public override IReadOnlyDictionary<ulong, KuracordGuild> Guilds { get; }
    internal ConcurrentDictionary<ulong, KuracordGuild> _guilds = new();

    #endregion

    #region Constructor and Setup

    /// <summary>
    /// Initializes a new instance of KuracordClient.
    /// </summary>
    /// <param name="config">Specifies configuration parameters.</param>
    public KuracordClient(KuracordConfiguration config) : base(config) {
        InternalSetup();

        Guilds = new ReadOnlyConcurrentDictionary<ulong, KuracordGuild>(_guilds);
    }
    
    void InternalSetup() {
        _socketOpened = new AsyncEvent<KuracordClient, SocketEventArgs>("SOCKET_OPENED", EventExecutionLimit, EventErrorHandler);
        _socketClosed = new AsyncEvent<KuracordClient, SocketCloseEventArgs>("SOCKET_CLOSED", EventExecutionLimit, EventErrorHandler);
        _socketErrored = new AsyncEvent<KuracordClient, SocketErrorEventArgs>("SOCKET_ERRORED", EventExecutionLimit, Goof);
        _clientErrored = new AsyncEvent<KuracordClient, ClientErrorEventArgs>("CLIENT_ERRORED", EventExecutionLimit, Goof);
        _guildCreated = new AsyncEvent<KuracordClient, GuildCreateEventArgs>("GUILD_CREATED", EventExecutionLimit, EventErrorHandler);
        _guildAvailable = new AsyncEvent<KuracordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", EventExecutionLimit, EventErrorHandler);
        _guildUpdated = new AsyncEvent<KuracordClient, GuildUpdateEventArgs>("GUILD_UPDATED", EventExecutionLimit, EventErrorHandler);
        _guildDownloadCompletedEvent = new AsyncEvent<KuracordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", EventExecutionLimit, EventErrorHandler);
        _ready = new AsyncEvent<KuracordClient, ReadyEventArgs>("READY", EventExecutionLimit, EventErrorHandler);
        _heartbeated = new AsyncEvent<KuracordClient, HeartbeatEventArgs>("HEARTBEATED", EventExecutionLimit, EventErrorHandler);
        _resumed = new AsyncEvent<KuracordClient, ReadyEventArgs>("RESUMED", EventExecutionLimit, EventErrorHandler);
        _zombied = new AsyncEvent<KuracordClient, ZombiedEventArgs>("ZOMBIED", EventExecutionLimit, EventErrorHandler);

        _guilds.Clear();
    }

    #endregion
    
    #region Public Connection Methods

    //todo: activity, status
    public async Task ConnectAsync() {
        if (!ConnectionLock.Wait(0)) throw new InvalidOperationException("This client is already connected.");
        ConnectionLock.Set();

        int w = 7500;
        int i = 5;
        bool s = false;
        Exception? cex = null;
        
        Logger.LogInformation(LoggerEvents.Startup, $"KSharpPlus, version {VersionString}");

        while (i-- > 0 || Configuration.ReconnectIndefinitely) {
            try {
                await InternalConnectAsync().ConfigureAwait(false);
                s = true;
                break;
            } catch (UnauthorizedException e) {
                FailConnection(ConnectionLock);
                throw new Exception("Authentication failed. Check your token and try again.", e);
            } catch (PlatformNotSupportedException) {
                FailConnection(ConnectionLock);
                throw;
            } catch (NotImplementedException) {
                FailConnection(ConnectionLock);
                throw;
            } catch (Exception e) {
                FailConnection(null);
                cex = e;
                
                if (i <= 0 && !Configuration.ReconnectIndefinitely) break;

                Logger.LogError(LoggerEvents.ConnectionFailure, e, $"Connection attempt failed, retrying in {w / 1000}s");
                await Task.Delay(w, _cancelToken).ConfigureAwait(false);

                if (i > 0) w *= 2;
            }
        }

        if (!s && cex != null) {
            ConnectionLock.Set();
            throw new Exception("Could not connect to Kuracord.", cex);
        }

        static void FailConnection(ManualResetEventSlim? cl) => cl?.Set();
    }
    
    public Task ReconnectAsync(bool startNewSession = false) => InternalReconnectAsync(startNewSession, code: startNewSession ? 1000 : 4002);
    
    /// <summary>
    /// Disconnects from the gateway
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync() {
        Configuration.AutoReconnect = false;
        if (_webSocketClient != null) await _webSocketClient.DisconnectAsync().ConfigureAwait(false);
    }

    #endregion

    #region Public REST Methods

    #region Guild

    /// <summary>
    /// Gets a guild.
    /// </summary>
    /// <param name="id">The guild ID to search for.</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<KuracordGuild> GetGuildAsync(ulong id) {
        if (_guilds.TryGetValue(id, out KuracordGuild? guild)) return guild;

        guild = await ApiClient.GetGuildAsync(id).ConfigureAwait(false);
        return guild;
    }

    #endregion

    #region Channel

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="id">The ID of the channel to get.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown when the channel does not exist.</exception>
    public KuracordChannel GetChannel(ulong id) => InternalGetCachedChannel(id)!;

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="id">The ID of the channel to get.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<KuracordChannel> GetChannelAsync(ulong guildId, ulong channelId) => 
        InternalGetCachedChannel(channelId, false) ?? await ApiClient.GetChannelAsync(guildId, channelId).ConfigureAwait(false);

    #endregion

    #endregion

    #region Internal Caching Methods
    
    internal KuracordChannel? InternalGetCachedChannel(ulong channelId, bool throwException = true) {
        foreach (KuracordGuild guild in Guilds.Values)
            if (guild.Channels.TryGetValue(channelId, out KuracordChannel? foundChannel))
                return foundChannel;

        if (throwException)
            throw new KeyNotFoundException($"Cannot find channel with id {channelId}.");
                
        return null;
    }

    void UpdateCachedGuild(KuracordGuild newGuild) {
        if (_disposed) return;

        newGuild._channels ??= new List<KuracordChannel>();
        newGuild._members ??= new List<KuracordMember>();
        newGuild._roles ??= new List<KuracordRole>();
        
        if (!_guilds.ContainsKey(newGuild.Id)) _guilds[newGuild.Id] = newGuild;

        KuracordGuild guild = _guilds[newGuild.Id];

        if (newGuild._channels.Any())
            foreach (KuracordChannel channel in newGuild._channels.Where(channel => !guild._channels!.Exists(c => c.Id == channel.Id)))
                guild._channels!.Add(channel);
        
        if (newGuild._members.Any())
            foreach (KuracordMember member in newGuild._members) {
                member.Kuracord = this;
                member._guildId = newGuild.Id;
                member._guild ??= newGuild;
                
                guild._members!.Add(member);
            }
        
        if (newGuild._roles.Any())
            foreach (KuracordRole role in newGuild._roles) {
                role.Kuracord = this;
                role._guild_id = newGuild.Id;
                
                guild._roles!.Add(role);
            }

        guild.Name = newGuild.Name;
        guild.ShortName = newGuild.ShortName;
        guild.Description = newGuild.Description;
        guild.Disabled = newGuild.Disabled;
        guild.Owner = newGuild.Owner;
        guild.IconHash = newGuild.IconHash;
        guild.VanityCode = newGuild.VanityCode;
    }

    #endregion

    #region Disposal

    ~KuracordClient() => Dispose();

    bool _disposed;

    /// <summary>
    /// Disposes your KuracordClient.
    /// </summary>
    public override void Dispose() {
        if (_disposed) return;

        _disposed = true;
        GC.SuppressFinalize(this);

        DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        ApiClient.Rest.Dispose();
        CurrentUser = null;

        try {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource?.Dispose();
        } catch {/**/}

        _guilds = null!;
        _heartbeatTask = null!;
    }

    #endregion
}