using Newtonsoft.Json;

namespace KSharpPlus.Net.Abstractions.Rest; 

internal class RestUserLoginPayload {
    public RestUserLoginPayload(string email, string password) {
        Email = email;
        Password = password;
    }
    
    [JsonProperty("email")] public string Email { get; set; }
    
    [JsonProperty("password")] public string Password { get; set; }
}

internal sealed class RestUserRegisterPayload : RestUserLoginPayload {
    public RestUserRegisterPayload(string username, string email, string password) : base(email, password) => Username = username;

    [JsonProperty("username")] public string Username { get; set; }
}

internal sealed class RestUserUpdatePayload {
    public RestUserUpdatePayload(string username) => Username = username;
    
    [JsonProperty("username")] public string Username { get; set; }
    
    [JsonProperty("discriminator")] public string Discriminator { get; set; } = null!;
    
    //todo: file
}

