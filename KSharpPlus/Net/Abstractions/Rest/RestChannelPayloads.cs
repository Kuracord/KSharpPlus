using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Rest; 

internal sealed class RestChannelCreateModifyPayload {
    public RestChannelCreateModifyPayload(string name) => Name = name;
    
    [JsonProperty("name")] public string Name { get; set; }
}

internal sealed class RestChannelMessageCreatePayload {
    public RestChannelMessageCreatePayload(string content) => Content = content;

    [JsonProperty("content")] public string Content { get; set; }
    
    //todo: payload_json
}

internal sealed class RestChannelMessageModifyPayload {
    public RestChannelMessageModifyPayload(string content) => Content = content;
    
    [JsonProperty("content")] public string Content { get; set; }
}