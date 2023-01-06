namespace KSharpPlus.Net.Rest; 

internal static class Endpoints {
    public const string APIVersion = "3";
    public const string BaseURI = $"https://api.kuracord.tk/api/v{APIVersion}";

    public const string Channels = "/channels";
    public const string Guilds = "/guilds";
    public const string Invites = "/invites";
    public const string News = "/news";
    public const string Users = "/users";

    public const string Members = "/members";
    public const string Messages = "/messages";
}