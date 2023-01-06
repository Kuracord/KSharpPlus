using KSharpPlus.Entities.Guild;
using KSharpPlus.Enums.Channel;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Channel; 

public class KuracordChannel : SnowflakeObject, IEquatable<KuracordChannel> {
    internal KuracordChannel() { }
    
    #region Fields and Properties

    /// <summary>
    /// Gets the name of this channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }
    
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
    [JsonIgnore] public KuracordGuild? Guild => GuildId.HasValue && Kuracord.Guilds.TryGetValue(GuildId.Value, out KuracordGuild? guild) ? guild : null;
    
    /// <summary>
    /// Gets this channel's mention string.
    /// </summary>
    [JsonIgnore] public string Mention => Formatter.Mention(this);

    #endregion
    
    #region Utils

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
    public static bool operator !=(KuracordChannel e1, KuracordChannel e2) => !(e1 == e2);

    #endregion
}