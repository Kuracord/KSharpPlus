﻿using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Enums.User;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.User; 

public class KuracordUser : SnowflakeObject, IEquatable<KuracordUser> {
    #region Constructors

    internal KuracordUser() { }

    internal KuracordUser(KuracordUser transport) {
        Kuracord = transport.Kuracord;
        Id = transport.Id;
        Username = transport.Username;
        Discriminator = transport.Discriminator;
        Biography = transport.Biography;
        AvatarUrl = transport.AvatarUrl;
        IsBot = transport.IsBot;
        Disabled = transport.Disabled;
        Verified = transport.Verified;
        Email = transport.Email;
        PremiumType = transport.PremiumType;
        Flags = transport.Flags;
        GuildsMember = transport.GuildsMember;
    }

    #endregion

    #region Fields and Properties

    /// <summary>
    /// Gets this user's username.
    /// </summary>
    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public string Username { get; internal set; } = null!;

    /// <summary>
    /// Gets the user's 4-digit discriminator.
    /// </summary>
    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public string Discriminator { get; internal set; } = null!;
    
    /// <summary>
    /// Gets the user's token. This field will not be null only if the user was get from
    /// <see cref="KuracordClient.RegisterUserAsync"/> method. 
    /// </summary>
    [JsonIgnore] public string? Token { get; internal set; }
    
    /// <summary>
    /// Gets the user's biography.
    /// </summary>
    [JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
    public string Biography { get; internal set; } = null!;

    /// <summary>
    /// Gets the user's avatar URL.
    /// </summary>
    [JsonIgnore] public string AvatarUrl { get; internal set; } = null!;

    /// <summary>
    /// Gets whether the user is a bot.
    /// </summary>
    [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsBot { get; internal set; }
    
    /// <summary>
    /// Gets whether the user is disabled. Only for the current user.
    /// </summary>
    [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Disabled { get; internal set; }

    /// <summary>
    /// Gets whether the user is verified. Only for the current user.
    /// </summary>
    [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Verified { get; internal set; }

    /// <summary>
    /// Gets the user's email address. Only for the current user.
    /// </summary>
    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public string? Email { get; internal set; }

    /// <summary>
    /// Gets the user's premium type. Only for the current user.
    /// </summary>
    [JsonProperty("premiumType", NullValueHandling = NullValueHandling.Ignore)]
    public PremiumType? PremiumType { get; internal set; }

    /// <summary>
    /// Gets the user's flags.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public UserFlags? Flags { get; internal set; }
    
    /// <summary>
    /// Gets user's guilds. Only for the current user.
    /// </summary>
    [JsonProperty("guilds", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<KuracordMember>? GuildsMember { get; internal set; }

    /// <summary>
    /// Gets the user's mention string.
    /// </summary>
    [JsonIgnore] public string Mention => Formatter.Mention(this);

    /// <summary>
    /// Gets whether this user is the Client which created this object.
    /// </summary>
    [JsonIgnore] public bool IsCurrent => this == Kuracord!.CurrentUser;

    #endregion
    
    #region Utils

    /// <summary>
    /// Returns a string representation of this user.
    /// </summary>
    /// <returns>String representation of this user.</returns>
    public override string ToString() => $"User {Id}; {Username}#{Discriminator}";

    /// <summary>
    /// Checks whether this <see cref="KuracordUser" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="KuracordUser" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as KuracordUser);

    /// <summary>
    /// Checks whether this <see cref="KuracordUser" /> is equal to another <see cref="KuracordUser" />.
    /// </summary>
    /// <param name="e"><see cref="KuracordUser" /> to compare to.</param>
    /// <returns>Whether the <see cref="KuracordUser" /> is equal to this <see cref="KuracordUser" />.</returns>
    public bool Equals(KuracordUser? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || Id == e.Id;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="KuracordUser" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="KuracordUser" />.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="KuracordUser" /> objects are equal.
    /// </summary>
    /// <param name="e1">First user to compare.</param>
    /// <param name="e2">Second user to compare.</param>
    /// <returns>Whether the two users are equal.</returns>
    public static bool operator ==(KuracordUser? e1, KuracordUser? e2) {
        object? o1 = e1;
        object? o2 = e2;

        if (o1 == null && o2 != null || o1 != null && o2 == null)
            return false;

        return o1 == null && o2 == null || e1?.Id == e2?.Id;
    }

    /// <summary>
    /// Gets whether the two <see cref="KuracordUser" /> objects are not equal.
    /// </summary>
    /// <param name="e1">First user to compare.</param>
    /// <param name="e2">Second user to compare.</param>
    /// <returns>Whether the two users are not equal.</returns>
    public static bool operator !=(KuracordUser? e1, KuracordUser? e2) => !(e1 == e2);

    #endregion
}