using KSharpPlus.Entities.User;
using KSharpPlus.Enums;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Guild; 

/// <summary>
/// Represents a Kuracord guild member.
/// </summary>
public class KuracordMember : SnowflakeObject, IEquatable<KuracordMember> {
    #region Constructors

    internal KuracordMember() {}

    internal KuracordMember(KuracordUser user) {
        User = user;
        Kuracord = user.Kuracord;
    }

    internal KuracordMember(KuracordMember member) {
        Id = member.User.Id;
        Nickname = member.Nickname;
        JoinedAt = member.JoinedAt;
    }

    #endregion

    #region Fields and Properties

    /// <summary>
    /// Gets this member's nickname.
    /// </summary>
    [JsonProperty("nickname")] public string? Nickname { get; internal set; }

    /// <summary>
    /// Gets this member's display name.
    /// </summary>
    [JsonIgnore] public string DisplayName => Nickname ?? User.Username;
    
    /// <summary>
    /// Date the user joined the guild
    /// </summary>
    [JsonProperty("joinedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset JoinedAt { get; internal set; }
    
    [JsonIgnore] internal ulong _guildId;

    /// <summary>
    /// Gets the guild of which this member is a part of.
    /// </summary>
    [JsonIgnore] public KuracordGuild Guild {
        get {
            if (_guild == null) _guild = Kuracord!.Guilds[_guildId];
            return _guild;
        }
        set => _guild = value;
    }

    [JsonProperty("guild")] internal KuracordGuild? _guild;

    /// <summary>
    /// Gets the user associated with this member.
    /// </summary>
    [JsonProperty("user")] public KuracordUser User { get; internal set; } = null!;

    /// <summary>
    /// Gets whether this member is the Guild owner.
    /// </summary>
    [JsonIgnore] public bool IsOwner => User.Id == Guild.OwnerId;

    #endregion

    #region Methods

    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyAsync(string? nickname) => Kuracord!.ApiClient.ModifyMemberAsync(_guildId, Id, nickname);
    
    /// <summary>
    /// Kicks this member from their guild.
    /// </summary>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task RemoveAsync() => Kuracord!.ApiClient.DeleteMemberAsync(_guildId, Id);

    #endregion
    
    #region Utils

    /// <summary>
    /// Returns a string representation of this member.
    /// </summary>
    /// <returns>String representation of this member.</returns>
    public override string ToString() => $"Member {Id}; {User.Username}#{User.Discriminator} ({DisplayName})";

    /// <summary>
    /// Checks whether this <see cref="KuracordMember" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordMember" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordMember);

    /// <summary>
    /// Checks whether this <see cref="KuracordMember" /> is equal to another <see cref="KuracordMember" />.
    /// </summary>
    /// <param name="e"><see cref="KuracordMember" /> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordMember" /> is equal to this <see cref="KuracordMember" />.</returns>
    public bool Equals(KuracordMember? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || Id == e.Id && _guildId == e._guildId;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="KuracordMember" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordMember" />.</returns>
    public override int GetHashCode() {
        int hash = 13;

        hash = hash * 7 + Id.GetHashCode();
        hash = hash * 7 + _guildId.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordMember" /> objects are equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(KuracordMember? e1, KuracordMember? e2) {
        object? o1 = e1;
        object? o2 = e2;

        if (o1 == null && o2 != null || o1 != null && o2 == null) return false;

        return o1 == null && o2 == null || e1?.Id == e2?.Id && e1?._guildId == e2?._guildId;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordMember" /> objects are not equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(KuracordMember e1, KuracordMember e2) => !(e1 == e2);

    #endregion
}