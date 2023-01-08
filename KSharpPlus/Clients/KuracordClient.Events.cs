using Emzi0767.Utilities;
using KSharpPlus.EventArgs;
using KSharpPlus.EventArgs.Channel;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.EventArgs.Guild.Member;
using KSharpPlus.EventArgs.Message;
using KSharpPlus.EventArgs.Socket;
using KSharpPlus.Logging;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Clients; 

public sealed partial class KuracordClient {
    internal static TimeSpan EventExecutionLimit { get; } = TimeSpan.FromSeconds(1);

    #region WebSocket

    /// <summary>
    /// Fired whenever a WebSocket error occurs within the client.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, SocketErrorEventArgs> SocketErrored {
        add => _socketErrored.Register(value);
        remove => _socketErrored.Unregister(value);
    }
    
    AsyncEvent<KuracordClient, SocketErrorEventArgs> _socketErrored;

    /// <summary>
    /// Fired whenever WebSocket connection is established.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, SocketEventArgs> SocketOpened {
        add => _socketOpened.Register(value);
        remove => _socketOpened.Unregister(value);
    }

    AsyncEvent<KuracordClient, SocketEventArgs> _socketOpened;

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, SocketCloseEventArgs> SocketClosed {
        add => _socketClosed.Register(value);
        remove => _socketClosed.Unregister(value);
    }

    AsyncEvent<KuracordClient, SocketCloseEventArgs> _socketClosed;

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="Guilds" /> will not be populated when this event is fired.</i><br />
    /// See also: <see cref="GuildAvailable" />, <see cref="GuildDownloadCompleted" />
    /// </remarks>
    public event AsyncEventHandler<KuracordClient, ReadyEventArgs> Ready {
        add => _ready.Register(value);
        remove => _ready.Unregister(value);
    }

    AsyncEvent<KuracordClient, ReadyEventArgs> _ready;

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, ReadyEventArgs> Resumed {
        add => _resumed.Register(value);
        remove => _resumed.Unregister(value);
    }

    AsyncEvent<KuracordClient, ReadyEventArgs> _resumed;

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, HeartbeatEventArgs> Heartbeated {
        add => _heartbeated.Register(value);
        remove => _heartbeated.Unregister(value);
    }

    AsyncEvent<KuracordClient, HeartbeatEventArgs> _heartbeated;

    /// <summary>
    /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, ZombiedEventArgs> Zombied {
        add => _zombied.Register(value);
        remove => _zombied.Unregister(value);
    }

    AsyncEvent<KuracordClient, ZombiedEventArgs> _zombied;

    #endregion

    #region Guild

    /// <summary>
    /// Fired when the user joins a new guild.
    /// </summary>
    /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
    public event AsyncEventHandler<KuracordClient, GuildCreateEventArgs> GuildCreated {
        add => _guildCreated.Register(value);
        remove => _guildCreated.Unregister(value);
    }
    
    AsyncEvent<KuracordClient, GuildCreateEventArgs> _guildCreated;

    /// <summary>
    /// Fired when a guild is becoming available.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, GuildCreateEventArgs> GuildAvailable {
        add => _guildAvailable.Register(value);
        remove => _guildAvailable.Unregister(value);
    }
    
    AsyncEvent<KuracordClient, GuildCreateEventArgs> _guildAvailable;

    /// <summary>
    /// Fired when a guild is updated.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, GuildUpdateEventArgs> GuildUpdated {
        add => _guildUpdated.Register(value);
        remove => _guildUpdated.Unregister(value);
    }
    
    AsyncEvent<KuracordClient, GuildUpdateEventArgs> _guildUpdated;
    
    /// <summary>
    /// Fired when all guilds finish streaming from Kuracord.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted {
        add => _guildDownloadCompletedEvent.Register(value);
        remove => _guildDownloadCompletedEvent.Unregister(value);
    }

    AsyncEvent<KuracordClient, GuildDownloadCompletedEventArgs> _guildDownloadCompletedEvent;

    #endregion

    #region Channel

    /// <summary>
    /// Fired when a new channel is created.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, ChannelCreateEventArgs> ChannelCreated {
        add => _channelCreated.Register(value);
        remove => _channelCreated.Unregister(value);
    }

    AsyncEvent<KuracordClient, ChannelCreateEventArgs> _channelCreated;

    #endregion

    #region Message

    /// <summary>
    /// Fired when a message is created.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, MessageCreateEventArgs> MessageCreated {
        add => _messageCreated.Register(value);
        remove => _messageCreated.Unregister(value);
    }

    AsyncEvent<KuracordClient, MessageCreateEventArgs> _messageCreated;
    
    /// <summary>
    /// Fired when a message is updated.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, MessageUpdateEventArgs> MessageUpdated {
        add => _messageUpdated.Register(value);
        remove => _messageUpdated.Unregister(value);
    }

    AsyncEvent<KuracordClient, MessageUpdateEventArgs> _messageUpdated;

    #endregion

    #region Member

    /// <summary>
    /// Fired when a new user joins a guild.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, MemberJoinedEventArgs> MemberJoined {
        add => _memberJoined.Register(value);
        remove => _memberJoined.Unregister(value);
    }

    AsyncEvent<KuracordClient, MemberJoinedEventArgs> _memberJoined;
    
    /// <summary>
    /// Fired when a guild member is updated.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, MemberUpdatedEventArgs> MemberUpdated {
        add => _memberUpdated.Register(value);
        remove => _memberUpdated.Unregister(value);
    }

    AsyncEvent<KuracordClient, MemberUpdatedEventArgs> _memberUpdated;

    #endregion

    #region Misc
    
    /// <summary>
    /// Fired whenever an error occurs within an event handler.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, ClientErrorEventArgs> ClientErrored {
        add => _clientErrored.Register(value);
        remove => _clientErrored.Unregister(value);
    }

    AsyncEvent<KuracordClient, ClientErrorEventArgs> _clientErrored;

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    public event AsyncEventHandler<KuracordClient, UnknownEventArgs> UnknownEvent {
        add => _unknownEvent.Register(value);
        remove => _unknownEvent.Unregister(value);
    }

    AsyncEvent<KuracordClient, UnknownEventArgs> _unknownEvent;

    #endregion
    
    #region Error Handling

    internal void EventErrorHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex,
        AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs) where TArgs : AsyncEventArgs {
        if (ex is AsyncEventTimeoutException) {
            Logger.LogWarning(LoggerEvents.EventHandlerException, $"An event handler for {asyncEvent.Name} took too long to execute. Defined as \"{handler.Method.ToString()?.Replace(handler.Method.ReturnType.ToString(), "").TrimStart()}\" located in \"{handler.Method.DeclaringType}\".");
            return;
        }

        Logger.LogError(LoggerEvents.EventHandlerException, ex, $"Event handler exception for event {asyncEvent.Name} thrown from {handler.Method} (defined in {handler.Method.DeclaringType})");
        _clientErrored.InvokeAsync(this, new ClientErrorEventArgs(asyncEvent.Name, ex)).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    void Goof<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex,
        AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs) where TArgs : AsyncEventArgs =>
        Logger.LogCritical(LoggerEvents.EventHandlerException, ex, $"Exception event handler {handler.Method} (defined in {handler.Method.DeclaringType}) threw an exception");

    #endregion
}