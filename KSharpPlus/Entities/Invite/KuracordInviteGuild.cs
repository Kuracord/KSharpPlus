using KSharpPlus.Entities.Guild;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Invite; 

/// <summary>
/// Represents a guild to which the user is invited.
/// </summary>
public class KuracordInviteGuild : SnowflakeObject {
    internal KuracordInviteGuild() { }

    #region Fields and Properties

    /// <summary>
    /// Gets the name of the guild.
    /// </summary>
    [JsonProperty("name")] public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the guild's description.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)] 
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the guild's vanity invite code.
    /// </summary>
    [JsonProperty("vanityUrl")] public string VanityCode { get; internal set; }
    
    /// <summary>
    /// Gets the guild icon's url.
    /// </summary>
    [JsonIgnore] public string IconUrl => GetIconUrl();
    
    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }
    
    /// <summary>
    /// Gets whether the guild is disabled.
    /// </summary>
    [JsonProperty("disabled")] public bool Disabled { get; internal set; }

    #endregion

    #region Methods
    
    /// <summary>
    /// Gets guild's icon URL.
    /// </summary>
    /// <returns>The URL of the guild's icon.</returns>
    public string GetIconUrl() => string.IsNullOrWhiteSpace(IconHash) ? null! : $"https://cdn.kuracord.tk/icons/{Id}/{IconHash}";

    public Task<KuracordGuild> AcceptAsync() => Kuracord!.ApiClient.AcceptInviteAsync(VanityCode);

    #endregion

    #region Utils

    /// <summary>
    /// Returns a string representation of this invite.
    /// </summary>
    /// <returns>String representation of this invite.</returns>
    public override string ToString() => $"Invite {VanityCode}; Guild {Id}; {Name}";

    #endregion
}