using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;
using KSharpPlus.Enums;
using KSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Channel.Message;

/// <summary>
/// Represents a Kuracord text message.
/// </summary>
public class KuracordMessage : SnowflakeObject, IEquatable<KuracordMessage> {
    #region Constructors

    internal KuracordMessage() { }

    internal KuracordMessage(KuracordMessage other) {
        Kuracord = other.Kuracord;

        _attachments = new List<KuracordAttachment>(other._attachments);

        Author = other.Author;
        _channel = other._channel;
        Content = other.Content;
        EditedTimestamp = other.EditedTimestamp;
        Id = other.Id;
        EditedTimestamp = other.EditedTimestamp;
    }

    #endregion

    #region Fields and Properties
    
    /// <summary>
    /// Gets the guild in which the message was sent.
    /// </summary>
    [JsonIgnore] public KuracordGuild Guild {
        get {
            _guild.Kuracord ??= Kuracord;

            if (_guild.Channels.Any() && _guild.Members.Any() || Kuracord is not KuracordClient client) return _guild;

            _guild = client.GetGuildAsync(_guild.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            _guild.Kuracord = Kuracord;

            return _guild;
        } 
    }
    
    [JsonProperty("guild")] internal KuracordGuild _guild { get; set; }

    /// <summary>
    /// Gets the channel in which the message was sent.
    /// </summary>
    [JsonIgnore] public KuracordChannel Channel {
        get {
            _channel.GuildId ??= Guild.Id;
            _channel.Kuracord ??= Kuracord;
            return _channel;
        }
    }
     
    [JsonProperty("channel")] internal KuracordChannel _channel { get; set; }

    /// <summary>
    /// Gets the user that sent the message.
    /// </summary>
    [JsonProperty("author")] public KuracordUser Author { get; internal set; }
    
    /// <summary>
    /// Gets the member that sent the message.
    /// </summary>
    [JsonIgnore] public KuracordMember Member {
        get {
            _member.User = Author;
            _member.Kuracord = Kuracord;
            return _member;
        }
    }

    [JsonProperty("member")] internal KuracordMember _member { get; set; }

    /// <summary>
    /// Gets the ID of the channel in which the message was sent.
    /// </summary>
    [JsonIgnore] public ulong ChannelId => Channel.Id;

    /// <summary>
    /// Gets the message's content.
    /// </summary>
    [JsonProperty("content")] public string Content { get; internal set; }
    
    /// <summary>
    /// Gets the message's edit timestamp. Will be null if the message was not edited.
    /// </summary>
    [JsonProperty("editedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? EditedTimestamp { get; internal set; }
    
    /// <summary>
    /// Gets whether this message was edited.
    /// </summary>
    [JsonIgnore] public bool IsEdited => EditedTimestamp != null;
    
    /// <summary>
    /// Gets files attached to this message.
    /// </summary>
    [JsonIgnore] public IReadOnlyList<KuracordAttachment> Attachments => _attachments;

    [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordAttachment> _attachments = new();

    [JsonIgnore] internal ulong? _guildId => Guild.Id;

    #endregion

    #region Methods

    /// <summary>
    /// Edits the message.
    /// </summary>
    /// <param name="content">New content.</param>
    /// <returns>Modified message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<KuracordMessage> ModifyAsync(string content) => Kuracord!.ApiClient.EditMessageAsync(ChannelId, Id, content);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAsync() => Kuracord!.ApiClient.DeleteMessageAsync(ChannelId, Id);

    #endregion
    
    #region Utils

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>String representation of this message.</returns>
    public override string ToString() => $"Message {Id}; Attachment count: {_attachments.Count}; Contents: {Content}";

    /// <summary>
    /// Checks whether this <see cref="KuracordMessage" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordMessage" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordMessage);

    /// <summary>
    /// Checks whether this <see cref="KuracordMessage" /> is equal to another <see cref="KuracordMessage" />.
    /// </summary>
    /// <param name="e"><see cref="KuracordMessage" /> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordMessage" /> is equal to this <see cref="KuracordMessage" />.</returns>
    public bool Equals(KuracordMessage? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || Id == e.Id && ChannelId == e.ChannelId;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="KuracordMessage" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordMessage" />.</returns>
    public override int GetHashCode() {
        int hash = 13;

        hash = hash * 7 + Id.GetHashCode();
        hash = hash * 7 + ChannelId.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordMessage" /> objects are equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are equal.</returns>
    public static bool operator ==(KuracordMessage? e1, KuracordMessage? e2) {
        object? o1 = e1;
        object? o2 = e2;

        if (o1 == null && o2 != null || o1 != null && o2 == null)
            return false;

        return o1 == null && o2 == null || e1?.Id == e2?.Id && e1.ChannelId == e2.ChannelId;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordMessage" /> objects are not equal.
    /// </summary>
    /// <param name="e1">First message to compare.</param>
    /// <param name="e2">Second message to compare.</param>
    /// <returns>Whether the two messages are not equal.</returns>
    public static bool operator !=(KuracordMessage? e1, KuracordMessage? e2) => !(e1 == e2);

    #endregion
}