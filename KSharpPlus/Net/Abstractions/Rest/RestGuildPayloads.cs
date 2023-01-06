using KSharpPlus.Entities;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Rest; 

internal class RestGuildCreatePayload {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
    
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
    public Optional<string> IconBase64 { get; set; }
}

internal sealed class RestGuildModifyPayload {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
    
    //todo: file (icon)
}

internal sealed class RestGuildMemberModifyPayload {
    [JsonProperty("nickname")] public string Nickname { get; set; }
}