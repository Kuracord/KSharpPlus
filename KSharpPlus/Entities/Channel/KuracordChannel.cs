using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Enums;
using KSharpPlus.Enums.Channel;
using KSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Channel; 

public class KuracordChannel : SnowflakeObject, IEquatable<KuracordChannel> {
    internal KuracordChannel() { }
    
    #region Fields and Properties

    /// <summary>
    /// Gets the name of this channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the type of this channel.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public ChannelType Type { get; internal set; }

    /// <summary>
    /// Gets ID of the guild to which this channel belongs.
    /// </summary>
    [JsonIgnore] public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Gets the guild to which this channel belongs.
    /// </summary>
    [JsonIgnore] public KuracordGuild? Guild {
        get {
            KuracordClient client = (KuracordClient)Kuracord!;
            
            if (_guild == null) {
                if (GuildId.HasValue)
                    _guild = Kuracord!.Guilds.TryGetValue(GuildId.Value, out KuracordGuild? guild) ? guild
                        : client.GetGuildAsync(GuildId.Value).ConfigureAwait(false).GetAwaiter().GetResult();
                else return null;
            }

            if (_guild != null && !GuildId.HasValue) GuildId = _guild.Id;

            if (_guild!.Owner == null! && GuildId != null) _guild = client.GetGuildAsync(GuildId.Value).ConfigureAwait(false).GetAwaiter().GetResult();

            _guild.Kuracord = Kuracord;

            return _guild;
        }
    }

    [JsonProperty("guild")] internal KuracordGuild? _guild { get; set; }
    
    /// <summary>
    /// Gets this channel's mention string.
    /// </summary>
    [JsonIgnore] public string Mention => Formatter.Mention(this);

    #endregion

    #region Methods

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> SendMessageAsync(string content) => Kuracord!.ApiClient.CreateMessageAsync(Guild!.Id, Id, content);

    /// <summary>
    /// Returns a specific message.
    /// </summary>
    /// <param name="messageId">ID of the message to get.</param>
    /// <param name="skipCache">Whether to always make a REST request.</param>
    /// <returns>Requested message.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordMessage> GetMessageAsync(ulong messageId, bool skipCache = false) =>
        !skipCache &&
        Kuracord!.Configuration.MessageCacheSize > 0 &&
        Kuracord is KuracordClient { MessageCache: { } } client &&
        client.MessageCache.TryGet(m => m.Id == messageId && m.ChannelId == Id, out KuracordMessage message) ? message 
            : await Kuracord!.ApiClient.GetMessageAsync(Guild!.Id, Id, messageId).ConfigureAwait(false);

    /// <summary>
    /// Returns a list of messages from the last message in the channel.
    /// </summary>
    /// <returns>A list of messages from the last message in the channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<IReadOnlyList<KuracordMessage>> GetMessagesAsync() => GetMessagesInternalAsync();

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="message">The message to edit.</param>
    /// <param name="content">New content.</param>
    /// <returns>Edited message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> EditMessageAsync(KuracordMessage message, string content) => EditMessageAsync(message.Id, content);

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="messageId">ID of the message to edit.</param>
    /// <param name="content">New content.</param>
    /// <returns>Edited message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> EditMessageAsync(ulong messageId, string content) => Kuracord!.ApiClient.EditMessageAsync(Guild!.Id, Id, messageId, content);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to be deleted.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task DeleteMessageAsync(KuracordMessage message) => DeleteMessageAsync(message.Id);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="messageId">ID of the message to be deleted.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task DeleteMessageAsync(ulong messageId) => Kuracord!.ApiClient.DeleteMessageAsync(Guild!.Id, Id, messageId);
    
    Task<IReadOnlyList<KuracordMessage>> GetMessagesInternalAsync() {
        if (Type != ChannelType.Text) throw new ArgumentException($"Cannot get the messages of a {Type} channel.");

        return Kuracord!.ApiClient.GetMessagesAsync(Guild!.Id, Id);
    }
    
    /// <summary>
    /// Clones this channel. This operation will create a channel with identical settings to this one. Note that this will not copy messages.
    /// </summary>
    /// <returns>Newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> CloneAsync() {
        if (Guild == null) throw new InvalidOperationException("Non-guild channels cannot be cloned.");

        return Guild.CreateChannelAsync(Name, Type);
    }
    
    /// <summary>
    /// Modifies a channel.
    /// </summary>
    /// <param name="name">New channel name.</param>
    /// <returns>Modified channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> ModifyAsync(string name) => Kuracord!.ApiClient.ModifyChannelAsync(Guild!.Id, Id, name);
    
    /// <summary>
    /// Deletes a channel.
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task DeleteAsync() => Kuracord!.ApiClient.DeleteChannelAsync(Guild!.Id, Id);

    #endregion
    
    #region Utils
    
    /// <summary>
    /// Returns a string representation of this channel.
    /// </summary>
    /// <returns>String representation of this channel.</returns>
    public override string ToString() => $"Channel {Id}; {Name}";

    /// <summary>
    /// Checks whether this <see cref="KuracordChannel" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordChannel" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordChannel);

    /// <summary>
    /// Checks whether this <see cref="KuracordChannel" /> is equal to another <see cref="KuracordChannel" />.
    /// </summary>
    /// <param name="e"><see cref="KuracordChannel" /> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordChannel" /> is equal to this <see cref="KuracordChannel" />.</returns>
    public bool Equals(KuracordChannel? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || Id == e.Id;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="KuracordChannel" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordChannel" />.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="KuracordChannel" /> objects are equal.
    /// </summary>
    /// <param name="e1">First channel to compare.</param>
    /// <param name="e2">Second channel to compare.</param>
    /// <returns>Whether the two channels are equal.</returns>
    public static bool operator ==(KuracordChannel? e1, KuracordChannel? e2) {
        object? o1 = e1;
        object? o2 = e2;

        if (o1 == null && o2 != null || o1 != null && o2 == null) return false;

        return o1 == null && o2 == null || e1?.Id == e2?.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordChannel" /> objects are not equal.
    /// </summary>
    /// <param name="e1">First channel to compare.</param>
    /// <param name="e2">Second channel to compare.</param>
    /// <returns>Whether the two channels are not equal.</returns>
    public static bool operator !=(KuracordChannel? e1, KuracordChannel? e2) => !(e1 == e2);

    #endregion
}