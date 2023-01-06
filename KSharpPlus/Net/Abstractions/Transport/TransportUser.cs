using KSharpPlus.Entities.Guild;
using KSharpPlus.Enums.User;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Transport;

internal class TransportUser {
    internal TransportUser() { }

    internal TransportUser(TransportUser other) {
        Id = other.Id;
        Username = other.Username;
        Discriminator = other.Discriminator;
        Biography = other.Biography;
        AvatarUrl = other.AvatarUrl;
        IsBot = other.IsBot;
        Disabled = other.Disabled;
        Verified = other.Verified;
        Email = other.Email;
        PremiumType = other.PremiumType;
        Flags = other.Flags;
        GuildsMember = other.GuildsMember;
    }

    [JsonProperty("id")] public ulong Id { get; internal set; }

    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public string Username { get; internal set; }

    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public string Discriminator { get; internal set; }
    
    [JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
    public string Biography { get; internal set; }

    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public string AvatarUrl { get; internal set; }

    [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsBot { get; internal set; }
    
    [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool Disabled { get; internal set; }

    [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
    public bool Verified { get; internal set; }

    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public string Email { get; internal set; }

    [JsonProperty("premiumType", NullValueHandling = NullValueHandling.Ignore)]
    public PremiumType? PremiumType { get; internal set; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public UserFlags? Flags { get; internal set; }
    
    [JsonProperty("guilds", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<KuracordMember> GuildsMember { get; internal set; }
}