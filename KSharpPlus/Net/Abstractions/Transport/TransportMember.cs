using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Transport; 

internal class TransportMember {
    [JsonProperty("id")] public ulong Id { get; internal set; }
    
    [JsonProperty("nickname")] public string? Nickname { get; internal set; }
    
    [JsonProperty("joinedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime JoinedAt { get; internal set; }
    
    [JsonIgnore] public TransportUser User { get; internal set; }
}