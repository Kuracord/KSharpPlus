using KSharpPlus.Entities;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Rest; 

internal sealed class RestGuildCreatePayload {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; } = null!;
    
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
    public Optional<string> IconBase64 { get; set; }
}

internal sealed class RestGuildModifyPayload {
    public RestGuildModifyPayload(string name) => Name = name;
    
    [JsonProperty("name")] public string Name { get; set; }
    
    //todo: file (icon)
}

internal sealed class RestGuildMemberModifyPayload {
    public RestGuildMemberModifyPayload(string? nickname) => Nickname = nickname;
    
    [JsonProperty("nickname")] public string? Nickname { get; set; }
}

internal sealed class RestGuildDeletePayload {
    public RestGuildDeletePayload(string password) => Password = password;
    
    [JsonProperty("password")] public string Password { get; set; } = null!;
}