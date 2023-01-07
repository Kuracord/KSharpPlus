using System.Collections.Concurrent;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.User;
using KSharpPlus.Enums.Channel;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Guild; 

public class KuracordGuild : SnowflakeObject, IEquatable<KuracordGuild> {
    internal KuracordGuild() { }
    
    #region Fields and Properties

    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name")] public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild's short name.
    /// </summary>
    [JsonProperty("shortName")] public string ShortName { get; internal set; }
    
    /// <summary>
    /// Gets the guild's vanity invite code.
    /// </summary>
    [JsonProperty("vanityUrl")] public string VanityCode { get; internal set; }

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
    [JsonProperty("owner")] public KuracordUser Owner { get; internal set; }
    
    /// <summary>
    /// Gets the guild's description.
    /// </summary>
    [JsonProperty("description")] public string? Description { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }

    /// <summary>
    /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordChannel> Channels {
        get {
            _channels ??= GetChannelsAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToList();
            
            ConcurrentDictionary<ulong, KuracordChannel> channels = new();
            foreach (KuracordChannel channel in _channels) channels.TryAdd(channel.Id, channel);
            return new ReadOnlyConcurrentDictionary<ulong, KuracordChannel>(channels);
        }
    }

    [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordChannel>? _channels;

    /// <summary>
    /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordMember> Members {
        get {
            _members ??= GetMembersAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToList();

            ConcurrentDictionary<ulong, KuracordMember> members = new();
            foreach (KuracordMember member in _members) members.TryAdd(member.Id, member);
            return new ReadOnlyConcurrentDictionary<ulong, KuracordMember>(members);
        }
    }

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordMember>? _members;

    /// <summary>
    /// Gets a dictionary of all the roles associated with this guild. The dictionary's key is the role ID.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<ulong, KuracordRole> Roles {
        get {
            _roles ??= new List<KuracordRole>();
            
            ConcurrentDictionary<ulong, KuracordRole> roles = new();
            foreach (KuracordRole role in _roles) roles.TryAdd(role.Id, role);
            return new ReadOnlyConcurrentDictionary<ulong, KuracordRole>(roles);
        }
    }

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<KuracordRole>? _roles;

    internal bool _isSynced { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Gets guild's icon URL, in requested format and size.
    /// </summary>
    /// <returns>The URL of the guild's icon.</returns>
    public string GetIconUrl() => string.IsNullOrWhiteSpace(IconHash) ? null! : $"https://cdn.kuracord.tk/icons/{Id}/{IconHash}";

    public Task<KuracordGuild> ModifyAsync(string name) => Kuracord.ApiClient.ModifyGuildAsync(Id, name);

    public Task<KuracordChannel> CreateTextChannelAsync(string name) => CreateChannelAsync(name, ChannelType.Text);

    public Task<KuracordChannel> CreateChannelAsync(string name, ChannelType type) => Kuracord.ApiClient.CreateChannelAsync(Id, name);

    public Task<IReadOnlyList<KuracordChannel>> GetChannelsAsync() => Kuracord.ApiClient.GetChannelsAsync(Id);

    public async Task<KuracordChannel> GetChannelAsync(ulong channelId) => 
        Channels.TryGetValue(channelId, out KuracordChannel? channel) && channel != null
            ? channel
            : await Kuracord.ApiClient.GetChannelAsync(Id, channelId).ConfigureAwait(false);

    public async Task<IReadOnlyList<KuracordMember>> GetMembersAsync() => await Kuracord.ApiClient.GetMembersAsync(Id);

    public KuracordRole GetRole(ulong roleId) => Roles.TryGetValue(roleId, out KuracordRole? role) ? role : null!;

    public Task<KuracordMessage> SendMessageAsync(KuracordChannel channel, string content) => SendMessageAsync(channel.Id, content);

    public Task<KuracordMessage> SendMessageAsync(ulong channelId, string content) => Kuracord.ApiClient.CreateMessageAsync(channelId, content);

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