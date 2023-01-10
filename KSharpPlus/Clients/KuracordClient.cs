using System.Collections.Concurrent;
using Emzi0767.Utilities;
using KSharpPlus.Entities;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.Invite;
using KSharpPlus.Entities.User;
using KSharpPlus.Enums;
using KSharpPlus.Enums.Channel;
using KSharpPlus.EventArgs;
using KSharpPlus.EventArgs.Channel;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.EventArgs.Guild.Member;
using KSharpPlus.EventArgs.Message;
using KSharpPlus.EventArgs.Socket;
using KSharpPlus.Exceptions;
using KSharpPlus.Logging;
using KSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Clients;

public sealed partial class KuracordClient : BaseKuracordClient {
    #region Internal Fields and Properties

    internal RingBuffer<KuracordMessage>? MessageCache { get; }
    
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
    
    /// <summary>
    /// Gets the WS latency for this client.
    /// </summary>
    public int Ping => Volatile.Read(ref _ping);
    int _ping;

    #endregion

    #region Constructor and Setup

    /// <summary>
    /// Initializes a new instance of KuracordClient.
    /// </summary>
    /// <param name="config">Specifies configuration parameters.</param>
    public KuracordClient(KuracordConfiguration config) : base(config) {
        if (Configuration.MessageCacheSize > 0) MessageCache = new RingBuffer<KuracordMessage>(Configuration.MessageCacheSize);

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
        _channelCreated = new AsyncEvent<KuracordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", EventExecutionLimit, EventErrorHandler);
        _memberJoined = new AsyncEvent<KuracordClient, MemberJoinedEventArgs>("MEMBER_JOINED", EventExecutionLimit, EventErrorHandler);
        _memberUpdated = new AsyncEvent<KuracordClient, MemberUpdatedEventArgs>("MEMBER_UPDATED", EventExecutionLimit, EventErrorHandler);
        _messageCreated = new AsyncEvent<KuracordClient, MessageCreateEventArgs>("MESSAGE_CREATED", EventExecutionLimit, EventErrorHandler);
        _messageUpdated = new AsyncEvent<KuracordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", EventExecutionLimit, EventErrorHandler);
        _messageDeleted = new AsyncEvent<KuracordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", EventExecutionLimit, EventErrorHandler);
        _ready = new AsyncEvent<KuracordClient, ReadyEventArgs>("READY", EventExecutionLimit, EventErrorHandler);
        _heartbeated = new AsyncEvent<KuracordClient, HeartbeatEventArgs>("HEARTBEATED", EventExecutionLimit, EventErrorHandler);
        _resumed = new AsyncEvent<KuracordClient, ReadyEventArgs>("RESUMED", EventExecutionLimit, EventErrorHandler);
        _zombied = new AsyncEvent<KuracordClient, ZombiedEventArgs>("ZOMBIED", EventExecutionLimit, EventErrorHandler);
        _unknownEvent = new AsyncEvent<KuracordClient, UnknownEventArgs>("UNKNOWN_EVENT", EventExecutionLimit, EventErrorHandler);

        _guilds.Clear();
    }

    #endregion
    
    #region Public Connection Methods

    /// <summary>
    /// Connects to the gateway
    /// </summary>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
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
    /// <param name="id">ID of the guild to get.</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordGuild> GetGuildAsync(ulong id) {
        if (_guilds.TryGetValue(id, out KuracordGuild? guild)) return guild;

        return await ApiClient.GetGuildAsync(id).ConfigureAwait(false);
    }

    /// <summary>
    /// Joins a guild.
    /// </summary>
    /// <param name="inviteCode">The invite code to the guild to join.</param>
    /// <returns>The requested guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordGuild> JoinGuildAsync(string inviteCode) => ApiClient.AcceptInviteAsync(inviteCode);
    
    /// <summary>
    /// Creates a guild.
    /// </summary>
    /// <param name="name">Name of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <returns>The created guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordGuild> CreateGuildAsync(string name, Optional<Stream> icon = default) {
        Optional<string> iconB64;

        if (icon is { HasValue: true, Value: { } }) {
            Stream sourceStream = icon.Value;
            
            sourceStream.Seek(0, SeekOrigin.Begin);
            
            byte[] buff = new byte[sourceStream.Length];
            int br = 0;
            
            while (br < buff.Length) br += sourceStream.Read(buff, br, (int)sourceStream.Length - br);

            iconB64 = Convert.ToBase64String(buff);
        } else iconB64 = null!;

        return ApiClient.CreateGuildAsync(name, iconB64);
    }

    /// <summary>
    /// Modifies a guild.
    /// </summary>
    /// <param name="guild">The guild to modify.</param>
    /// <param name="name">New name.</param>
    /// <returns>Modified guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordGuild> ModifyGuildAsync(KuracordGuild guild, string name) => ModifyGuildAsync(guild.Id, name);

    /// <summary>
    /// Modifies a guild.
    /// </summary>
    /// <param name="guildId">ID of the guild to modify.</param>
    /// <param name="name">New name.</param>
    /// <returns>Modified guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordGuild> ModifyGuildAsync(ulong guildId, string name) => ApiClient.ModifyGuildAsync(guildId, name);

    /// <summary>
    /// Gets the collection with the guild members.
    /// </summary>
    /// <param name="guild">The guild to get the members.</param>
    /// <returns>Readonly list with the members of the <paramref name="guild"/></returns>
    public Task<IReadOnlyList<KuracordMember>> GetGuildMembersAsync(KuracordGuild guild) => GetGuildMembersAsync(guild.Id);
    
    /// <summary>
    /// Gets the collection with the guild members.
    /// </summary>
    /// <param name="guildId">ID of the guild to get the members.</param>
    /// <returns>Readonly list with the members of the guild</returns>
    public Task<IReadOnlyList<KuracordMember>> GetGuildMembersAsync(ulong guildId) => ApiClient.GetMembersAsync(guildId);

    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="guild">The guild where the required member to modify is located.</param>
    /// <param name="member">The member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyGuildMemberAsync(KuracordGuild guild, KuracordMember member, string? nickname) => ModifyGuildMemberAsync(guild.Id, member.Id, nickname);
    
    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="guildId">ID of the guild where the required member to modify is located.</param>
    /// <param name="member">The member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyGuildMemberAsync(ulong guildId, KuracordMember member, string? nickname) => ModifyGuildMemberAsync(guildId, member.Id, nickname);
    
    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="guild">The guild where the required member to modify is located.</param>
    /// <param name="memberId">ID of the member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyGuildMemberAsync(KuracordGuild guild, ulong memberId, string? nickname) => ModifyGuildMemberAsync(guild.Id, memberId, nickname);
    
    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="guildId">ID of the guild where the required member to modify is located.</param>
    /// <param name="memberId">ID of the member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyGuildMemberAsync(ulong guildId, ulong memberId, string? nickname) => ApiClient.ModifyMemberAsync(guildId, memberId, nickname);

    #endregion

    #region Channel

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="id">The ID of the channel to get.</param>
    /// <returns>Requested channel.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the channel does not exist.</exception>
    public KuracordChannel GetChannel(ulong id) => InternalGetCachedChannel(id)!;

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="guildId">ID of the guild where the required channel is located.</param>
    /// <param name="channelId">The ID of the channel to get.</param>
    /// <returns>Requested channel.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordChannel> GetChannelAsync(ulong guildId, ulong channelId) => 
        InternalGetCachedChannel(channelId, false) ?? await ApiClient.GetChannelAsync(guildId, channelId).ConfigureAwait(false);

    /// <summary>
    /// Creates a new text channel in the guild.
    /// </summary>
    /// <param name="guild">The guild where create the channel.</param>
    /// <param name="name">Name of the new channel.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> CreateTextChannelAsync(KuracordGuild guild, string name) => CreateTextChannelAsync(guild.Id, name);

    /// <summary>
    /// Creates a new text channel in the guild.
    /// </summary>
    /// <param name="guildId">ID of the guild where create the channel.</param>
    /// <param name="name">Name of the new channel.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> CreateTextChannelAsync(ulong guildId, string name) => ApiClient.CreateChannelAsync(guildId, name);

    #endregion

    #region Message

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> SendMessageAsync(KuracordChannel channel, string content) => ApiClient.CreateMessageAsync(channel.Id, content);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> SendMessageAsync(ulong channelId, string content) {
        KuracordChannel channel = GetChannel(channelId);
        return SendMessageAsync(channel, content);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="guildId">ID of the guild where the required channel is located.</param>
    /// <param name="channelId">ID of the channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordMessage> SendMessageAsync(ulong guildId, ulong channelId, string content) {
        KuracordChannel channel = await GetChannelAsync(guildId, channelId).ConfigureAwait(false);
        return await SendMessageAsync(channel, content).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a specific message.
    /// </summary>
    /// <param name="channel">The channel where the required message is located.</param>
    /// <param name="messageId">ID of the message to get.</param>
    /// <returns>Requested message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> GetMessageAsync(KuracordChannel channel, ulong messageId) => GetMessageAsync(channel.Id, messageId);

    /// <summary>
    /// Returns a specific message.
    /// </summary>
    /// <param name="channelId">ID of the channel where the required message is located.</param>
    /// <param name="messageId">ID of the message to get.</param>
    /// <returns>Requested message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> GetMessageAsync(ulong channelId, ulong messageId) => ApiClient.GetMessageAsync(channelId, messageId);

    /// <summary>
    /// Returns a list of messages from the last message in the channel.
    /// </summary>
    /// <param name="channel">The channel to get messages from.</param>
    /// <returns>A list of messages from the last message in the channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<KuracordMessage>> GetMessagesAsync(KuracordChannel channel) => GetMessagesAsync(channel.Id);

    /// <summary>
    /// Returns a list of messages from the last message in the channel.
    /// <param name="channelId">ID of the channel to get messages from.</param>
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<IReadOnlyList<KuracordMessage>> GetMessagesAsync(ulong channelId) => ApiClient.GetMessagesAsync(channelId);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to be deleted.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteMessageAsync(KuracordMessage message) => DeleteMessageAsync(message.Channel.Id, message.Id);
    
    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="channel">The channel in where the required message to delete is located.</param>
    /// <param name="messageId">ID of the message to be deleted.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteMessageAsync(KuracordChannel channel, ulong messageId) => DeleteMessageAsync(channel.Id, messageId);
    
    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="channelId">ID of the channel in where the required message to delete is located.</param>
    /// <param name="messageId">ID of the message to be deleted.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteMessageAsync(ulong channelId, ulong messageId) => ApiClient.DeleteMessageAsync(channelId, messageId);
    
    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="message">The message to edit.</param>
    /// <param name="content">New content.</param>
    /// <returns>Edited message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> EditMessageAsync(KuracordMessage message, string content) => EditMessageAsync(message.Channel.Id, message.Id, content);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="channel">The channel where the required message to edit is located.</param>
    /// <param name="messageId">ID of the message to edit.</param>
    /// <param name="content">New content.</param>
    /// <returns>Edited message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> EditMessageAsync(KuracordChannel channel, ulong messageId, string content) => EditMessageAsync(channel.Id, messageId, content);
    
    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="channelId">ID of the channel where the required message to edit is located.</param>
    /// <param name="messageId">ID of the message to edit.</param>
    /// <param name="content">New content.</param>
    /// <returns>Edited message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> EditMessageAsync(ulong channelId, ulong messageId, string content) => ApiClient.EditMessageAsync(channelId, messageId, content);

    #endregion

    #region Invite

    /// <summary>
    /// Gets an invite info.
    /// </summary>
    /// <param name="inviteCode">The invite code.</param>
    /// <returns>The requested Invite.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordInviteGuild> GetInviteInfoAsync(string inviteCode) => ApiClient.GetInviteInfoAsync(inviteCode);

    #endregion

    #region Member & User

    /// <summary>
    /// Gets a user.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>User</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordUser> GetUserAsync(ulong userId) {
        if (TryGetCachedUserInternal(userId, out KuracordUser? user)) return user!;

        user = await ApiClient.GetUserAsync(userId).ConfigureAwait(false);
        UserCache.AddOrUpdate(userId, user, (_, _) => user);
        
        return user;
    }
    
    /// <summary>
    /// Edits current user.
    /// </summary>
    /// <param name="username">New username.</param>
    /// <param name="discriminator">New discriminator.</param>
    /// <returns>Updated user.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordUser> UpdateCurrentUserAsync(string username, string? discriminator = null) => ApiClient.ModifyCurrentUserAsync(username, discriminator!);

    /// <summary>
    /// Disables current user. THIS ACTION CANNOT BE UNDONE.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task DisableCurrentUserAsync() => ApiClient.DisableCurrentUserAsync();

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="username">Username for the new user.</param>
    /// <param name="email">Email for the new user.</param>
    /// <param name="password">Password for the new user.</param>
    /// <param name="updateCache">Should the client adds this user to the cache or not. Defaults to false.</param>
    /// <returns>User with it's token.</returns>
    public async Task<KuracordUser> RegisterUserAsync(string username, string email, string password, bool updateCache = false) {
        KuracordUser user = await ApiClient.RegisterUserAsync(username, email, password).ConfigureAwait(false);
        return updateCache ? UpdateUserCache(user) : user;
    }

    /// <summary>
    /// Gets the user's token.
    /// </summary>
    /// <param name="email">Email of the user's account.</param>
    /// <param name="password">Password of the user's account.</param>
    /// <returns>Token string.</returns>
    public Task<string> GetUserTokenAsync(string email, string password) => ApiClient.GetUserTokenAsync(email, password);

    #endregion

    #endregion

    #region Internal Caching Methods
    
    internal KuracordChannel? InternalGetCachedChannel(ulong channelId, bool throwException = true) {
        foreach (KuracordGuild guild in Guilds.Values) {
            guild.Kuracord ??= this;
            
            if (guild.Channels.TryGetValue(channelId, out KuracordChannel? foundChannel)) return foundChannel;
        }

        if (throwException)
            throw new KeyNotFoundException($"Cannot find channel with id {channelId}.");
                
        return null;
    }
    
    internal KuracordGuild? InternalGetCachedGuild(ulong? guildId) => _guilds != null && guildId.HasValue && _guilds.TryGetValue(guildId.Value, out KuracordGuild? guild) ? guild : null;

    void UpdateCachedGuild(KuracordGuild newGuild, JArray? rawMembers) {
        if (_disposed) return;

        newGuild._channels ??= new List<KuracordChannel>();
        newGuild._members ??= new List<KuracordMember>();
        newGuild._roles ??= new List<KuracordRole>();
        
        if (!_guilds.ContainsKey(newGuild.Id)) _guilds[newGuild.Id] = newGuild;

        KuracordGuild guild = _guilds[newGuild.Id];

        if (newGuild._channels.Any())
            foreach (KuracordChannel channel in newGuild._channels.Where(channel => !guild._channels!.Exists(c => c.Id == channel.Id))) {
                channel.Kuracord = this;
                channel.GuildId = guild.Id;
                
                guild._channels!.Add(channel);
            }
                

        if (rawMembers != null) {
            guild._members ??= new List<KuracordMember>();
            guild._members.Clear();

            foreach (JToken rawMember in rawMembers) {
                KuracordMember member = rawMember.ToKuracordObject<KuracordMember>();
                member.Kuracord = this;
                member._guildId = guild.Id;

                UpdateUserCache(member.User);
                
                if (!guild._members!.Exists(m => m.Id == member.Id)) guild._members.Add(member);
            }
        }
        
        if (newGuild._roles.Any())
            foreach (KuracordRole role in newGuild._roles.Where(role => !guild._roles!.Exists(r => r.Id == role.Id))) {
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

    void UpdateMessage(KuracordMessage message, KuracordUser author, KuracordGuild guild, KuracordMember member) {
        author.Kuracord = this;
        member.Kuracord = this;
        member.User = author;
        message.Author = UpdateUser(author, guild, member);

        KuracordChannel? channel = InternalGetCachedChannel(message.ChannelId);
        
        if (channel != null) return;

        channel = !message._guildId.HasValue 
            ? new KuracordDmChannel {
                Kuracord = this,
                Id = message.ChannelId,
                Type = ChannelType.Unknown,
                Recipients = new[] { message.Author }
            } : new KuracordChannel {
                Kuracord = this,
                Id = message.ChannelId,
                Type = ChannelType.Text,
                GuildId = guild.Id
            };

        message._channel = channel;
    }

    KuracordUser UpdateUser(KuracordUser user, KuracordGuild guild, KuracordMember member) {
        if (member != null!) {
            member.Kuracord = this;
            member._guildId = guild.Id;

            if (member.User == null!) return user;

            user = member.User;
            user.Kuracord = this;

            member.User = UpdateUserCache(user);
        } else if (!string.IsNullOrWhiteSpace(user.Username)) UpdateUserCache(user);

        return user;
    }

    void CacheMessage(KuracordMessage message, KuracordUser author, KuracordMember member) {
        KuracordGuild guild = message.Channel.Guild ?? message.Guild;
        
        UpdateMessage(message, author, guild, member);

        if (Configuration.MessageCacheSize > 0 && message.Channel != null!) MessageCache?.Add(message);
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