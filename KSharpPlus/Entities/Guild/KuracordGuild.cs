using System.Collections.Concurrent;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.User;
using KSharpPlus.Enums;
using KSharpPlus.Enums.Channel;
using KSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Guild; 

public class KuracordGuild : SnowflakeObject, IEquatable<KuracordGuild> {
    internal KuracordGuild() { }
    
    #region Fields and Properties

    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name")] public string Name { get; internal set; } = null!;

    /// <summary>
    /// Gets the guild's short name.
    /// </summary>
    [JsonProperty("shortName")] public string ShortName { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the guild's vanity invite code.
    /// </summary>
    [JsonProperty("vanityUrl")] public string VanityCode { get; internal set; } = null!;

    /// <summary>
    /// Gets the guild icon's url.
    /// </summary>
    [JsonIgnore] public string IconUrl => GetIconUrl();

    /// <summary>
    /// Gets the ID of the guild's owner.
    /// </summary>
    [JsonIgnore] public ulong OwnerId => Owner.Id;
    
    /// <summary>
    /// Gets whether the guild is disabled.
    /// </summary>
    [JsonProperty("disabled")] public bool Disabled { get; internal set; }

    /// <summary>
    /// Gets the guild's owner.
    /// </summary>
    [JsonProperty("owner")] public KuracordUser Owner { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the guild's description.
    /// </summary>
    [JsonProperty("description")] public string? Description { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; } = null!;

    /// <summary>
    /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordChannel> Channels {
        get {
			if (_channels == null || !_channels.Any()) _channels = GetChannelsAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToList();
            
            foreach (KuracordChannel channel in _channels.Where(m => m.Kuracord == null)) channel.Kuracord = Kuracord;
            
            return new ReadOnlyConcurrentDictionary<ulong, KuracordChannel>(new ConcurrentDictionary<ulong, KuracordChannel>(_channels.ToDictionary(c => c.Id)));
        }
    }

    [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordChannel>? _channels;

    /// <summary>
    /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordMember> Members {
        get {
            if (_members == null || !_members.Any()) _members = GetMembersAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToList();

            foreach (KuracordMember member in _members.Where(m => m.Kuracord == null)) member.Kuracord = Kuracord;

            return new ReadOnlyConcurrentDictionary<ulong, KuracordMember>(new ConcurrentDictionary<ulong, KuracordMember>(_members.ToDictionary(m => m.Id)));
        }
    }

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordMember>? _members;

    /// <summary>
    /// Gets a dictionary of all the roles associated with this guild. The dictionary's key is the role ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordRole> Roles {
        get {
            if (_roles == null || !_members.Any()) _roles = new List<KuracordRole>();

            foreach (KuracordRole role in _roles.Where(r => r.Kuracord == null)) role.Kuracord = Kuracord;

            return new ReadOnlyConcurrentDictionary<ulong, KuracordRole>(new ConcurrentDictionary<ulong, KuracordRole>(_roles.ToDictionary(r => r.Id)));
        }
    }

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordRole>? _roles;

    internal bool _isSynced { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Gets guild's icon URL.
    /// </summary>
    /// <returns>The URL of the guild's icon.</returns>
    public string GetIconUrl() => string.IsNullOrWhiteSpace(IconHash) ? null! : $"https://cdn.kuracord.tk/icons/{Id}/{IconHash}";

    /// <summary>
    /// Modifies this guild.
    /// </summary>
    /// <param name="name">New guild name.</param>
    /// <returns>The modified guild object.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordGuild> ModifyAsync(string name) => Kuracord!.ApiClient.ModifyGuildAsync(Id, name);
    
    /// <summary>
    /// Deletes this guild. Requires the caller to be the owner of the guild.
    /// </summary>
    /// <param name="password">Your account password.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client is not the owner of the guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task DeleteAsync(string password) => Kuracord!.ApiClient.DeleteGuildAsync(Id, password);

    /// <summary>
    /// Creates a new text channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> CreateTextChannelAsync(string name) => CreateChannelAsync(name, ChannelType.Text);

    /// <summary>
    /// Creates a new channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="type">Type of the new channel.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordChannel> CreateChannelAsync(string name, ChannelType type) => Kuracord!.ApiClient.CreateChannelAsync(Id, name);

    /// <summary>
    /// Gets all the channels this guild has.
    /// </summary>
    /// <returns>A collection of this guild's channels.</returns>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<IReadOnlyList<KuracordChannel>> GetChannelsAsync() => Kuracord!.ApiClient.GetChannelsAsync(Id);

    /// <summary>
    /// Gets a channel from this guild by its ID.
    /// </summary>
    /// <param name="channelId">ID of the channel to get.</param>
    /// <returns>Requested channel.</returns>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the channel does not found.</exception>
    public async Task<KuracordChannel> GetChannelAsync(ulong channelId) => 
        Channels.TryGetValue(channelId, out KuracordChannel? channel) && channel != null!
            ? channel
            : await Kuracord!.ApiClient.GetChannelAsync(Id, channelId).ConfigureAwait(false);
    
    // ReSharper disable once InvertIf
    /// <summary>
    /// Gets a member of this guild by their ID.
    /// </summary>
    /// <param name="memberId">ID of the member to get.</param>
    /// <param name="updateCache">Update member cache for this guild or not. Defaults to false.</param>
    /// <returns>The requested member.</returns>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<KuracordMember> GetMemberAsync(ulong memberId, bool updateCache = false) {
        if (!Members.TryGetValue(memberId, out KuracordMember? member)) {
            member = await Kuracord!.ApiClient.GetMemberAsync(Id, memberId);
            _members?.Add(member);
        } else if (updateCache) {
            member = await Kuracord!.ApiClient.GetMemberAsync(Id, memberId);
            _members?.Replace(m => m == member, member);
        }

        member.Kuracord ??= Kuracord;
        return member;
    }

    /// <summary>
    /// Retrieves a full list of members from Kuracord. This method will bypass cache.
    /// </summary>
    /// <returns>A collection of all members in this guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public async Task<IReadOnlyList<KuracordMember>> GetMembersAsync() {
        IReadOnlyList<KuracordMember> members = await Kuracord!.ApiClient.GetMembersAsync(Id);
        _members = members.ToList();
        return members;
    }

    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="member">Member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyMemberAsync(KuracordMember member, string? nickname) => ModifyMemberAsync(member.Id, nickname);

    /// <summary>
    /// Modifies the member.
    /// </summary>
    /// <param name="memberId">ID of the member to modify.</param>
    /// <param name="nickname">New member nickname.</param>
    /// <returns>Modified member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.Administrator"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMember> ModifyMemberAsync(ulong memberId, string? nickname) => Kuracord!.ApiClient.ModifyMemberAsync(Id, memberId, nickname);
    
    /// <summary>
    /// Kicks the member from this guild.
    /// </summary>
    /// <param name="member">The member to kick.</param>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task RemoveMemberAsync(KuracordMember member) => RemoveMemberAsync(member.Id);
    
    /// <summary>
    /// Kicks the member from this guild.
    /// </summary>
    /// <param name="memberId">ID of the member to kick.</param>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task RemoveMemberAsync(ulong memberId) => Kuracord!.ApiClient.DeleteMemberAsync(Id, memberId);
    
    /// <summary>
    /// Leaves this guild.
    /// </summary>
    /// <exception cref="ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task LeaveAsync() => Kuracord!.ApiClient.LeaveGuildAsync(Id);

    /// <summary>
    /// Gets a role from this guild by its ID.
    /// </summary>
    /// <param name="roleId">ID of the role to get.</param>
    /// <returns>Requested role or null if the role does not exists.</returns>
    public KuracordRole? GetRole(ulong roleId) => Roles.TryGetValue(roleId, out KuracordRole? role) ? role : null;

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> SendMessageAsync(KuracordChannel channel, string content) => SendMessageAsync(channel.Id, content);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">ID of the channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Kuracord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Kuracord is unable to process the request.</exception>
    public Task<KuracordMessage> SendMessageAsync(ulong channelId, string content) => Kuracord!.ApiClient.CreateMessageAsync(Id, channelId, content);

    #endregion
    
    #region Utils
    
    /// <summary>
    /// Returns a string representation of this guild.
    /// </summary>
    /// <returns>String representation of this guild.</returns>
    public override string ToString() => $"Guild {Id}; {Name}";

    /// <summary>
    /// Checks whether this <see cref="KuracordGuild"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordGuild"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordGuild);

    /// <summary>
    /// Checks whether this <see cref="KuracordGuild"/> is equal to another <see cref="KuracordGuild"/>.
    /// </summary>
    /// <param name="e"><see cref="KuracordGuild"/> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordGuild"/> is equal to this <see cref="KuracordGuild"/>.</returns>
    public bool Equals(KuracordGuild? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || Id == e.Id;
    }
    
    /// <summary>
    /// Gets the hash code for this <see cref="KuracordGuild"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordGuild"/>.</returns>
    public override int GetHashCode() => Id.GetHashCode();
    
    /// <summary>
    /// Gets whether the two <see cref="KuracordGuild"/> objects are equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(KuracordGuild? e1, KuracordGuild? e2) {
        object? o1 = e1;
        object? o2 = e2;

        if (o1 == null && o2 != null || o1 != null && o2 == null) return false;

        return o1 == null && o2 == null || e1?.Id == e2?.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordGuild"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(KuracordGuild? e1, KuracordGuild? e2) => !(e1 == e2);

    #endregion
}