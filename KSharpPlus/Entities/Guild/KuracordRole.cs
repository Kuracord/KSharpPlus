using KSharpPlus.Entities.Color;
using KSharpPlus.Enums;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Guild; 

public class KuracordRole : SnowflakeObject, IEquatable<KuracordRole> {
    #region Fields and Properties

    /// <summary>
    /// Gets the name of this role.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the color of this role.
    /// </summary>
    [JsonIgnore] public KuracordColor Color => new(_color);

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    internal int _color;

    /// <summary>
    /// Gets whether this role is hoisted.
    /// </summary>
    [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsHoisted { get; internal set; }
    
    /// <summary>
    /// Gets the permissions set for this role.
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public Permissions Permissions { get; internal set; }
    
    [JsonIgnore] internal ulong _guildId;
    
    /// <summary>
    /// Gets a mention string for this role. If the role is mentionable, this string will mention all the users that belong to this role.
    /// </summary>
    public string Mention => Formatter.Mention(this);

    #endregion
    
    #region Utils

    /// <summary>
    /// Returns a string representation of this role.
    /// </summary>
    /// <returns>String representation of this role.</returns>
    public override string ToString() => $"Role {Id}; {Name}";

    /// <summary>
    /// Checks whether this <see cref="KuracordRole" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordRole" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordRole);

    /// <summary>
    /// Checks whether this <see cref="KuracordRole" /> is equal to another <see cref="KuracordRole" />.
    /// </summary>
    /// <param name="e"><see cref="KuracordRole" /> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordRole" /> is equal to this <see cref="KuracordRole" />.</returns>
    public bool Equals(KuracordRole? e) => e switch {
        null => false,
        _ => ReferenceEquals(this, e) || Id == e.Id
    };

    /// <summary>
    /// Gets the hash code for this <see cref="KuracordRole" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordRole" />.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="KuracordRole" /> objects are equal.
    /// </summary>
    /// <param name="e1">First role to compare.</param>
    /// <param name="e2">Second role to compare.</param>
    /// <returns>Whether the two roles are equal.</returns>
    public static bool operator ==(KuracordRole? e1, KuracordRole? e2) => e1 is null == e2 is null && (e1 is null && e2 is null || e1?.Id == e2?.Id);

    /// <summary>
    /// Gets whether the two <see cref="KuracordRole" /> objects are not equal.
    /// </summary>
    /// <param name="e1">First role to compare.</param>
    /// <param name="e2">Second role to compare.</param>
    /// <returns>Whether the two roles are not equal.</returns>
    public static bool operator !=(KuracordRole e1, KuracordRole e2) => !(e1 == e2);

    #endregion
}