using System.Net;
using KSharpPlus.Clients;
using KSharpPlus.Entities;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.Invite;
using KSharpPlus.Entities.User;
using KSharpPlus.Logging;
using KSharpPlus.Net.Abstractions.Rest;
using KSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Net.Rest; 

public sealed class KuracordApiClient {
    BaseKuracordClient Kuracord { get; } = null!;
    internal RestClient Rest { get; }

    internal KuracordApiClient(BaseKuracordClient client) {
        Kuracord = client;
        Rest = new RestClient(client);
    }
    
    // This is for meta-clients, such as the webhook client
    internal KuracordApiClient(IWebProxy proxy, TimeSpan timeout, ILogger logger) => Rest = new RestClient(proxy, timeout, logger);

    KuracordMessage PrepareMessage(JToken rawMessage) {
        KuracordUser author = rawMessage["author"]!.ToKuracordObject<KuracordUser>();
        KuracordMessage message = rawMessage.ToKuracordObject<KuracordMessage>();
        message.Kuracord = Kuracord;

        PopulateMessage(author, message);
        
        return message;
    }

    void PopulateMessage(KuracordUser author, KuracordMessage message) {
        KuracordGuild guild = message.Channel.Guild ?? message.Guild;

        if (author.IsBot && int.Parse(author.Discriminator) == 0) {
            message.Author = new KuracordUser(author) { Kuracord = Kuracord };
        } else {
            if (!Kuracord.UserCache.TryGetValue(author.Id, out KuracordUser? user))
                Kuracord.UserCache[author.Id] = user = new KuracordUser(author) { Kuracord = Kuracord };

            if (guild != null!) {
                if (guild.Members.Values.FirstOrDefault(m => m.User.Id == author.Id) == null) {
                    KuracordMember member = new(user) { Kuracord = Kuracord, _guildId = guild.Id };
                    guild._members!.Add(member);
                }
            }
            
            message.Author = user;
        }
    }

    async Task<RestResponse> DoRequestAsync(BaseKuracordClient client, Uri uri, RestRequestMethod method, IReadOnlyDictionary<string, string>? headers = null, string? payload = null) {
        RestRequest request = new(client, uri, method, headers, payload);

        if (Kuracord != null!) Rest.ExecuteRequestAsync(request).LogTaskFault(Kuracord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request.");
        else await Rest.ExecuteRequestAsync(request).ConfigureAwait(false);

        return await request.WaitForCompletionAsync().ConfigureAwait(false);
    }

    #region Guild

    internal async Task<KuracordGuild> GetGuildAsync(ulong guildId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);
        
        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;

        foreach (KuracordRole role in guild._roles ??= new List<KuracordRole>()) {
            role.Kuracord = Kuracord;
            role._guildId = guild.Id;
        }

        foreach (KuracordMember member in guild._members ??= new List<KuracordMember>()) {
            member.Kuracord = Kuracord;
            member._guildId = guild.Id;
        }

        foreach (KuracordChannel channel in guild._channels ??= new List<KuracordChannel>()) {
            channel.Kuracord = Kuracord;
            channel.GuildId = guild.Id;
        }

        guild.Kuracord = Kuracord;
        return guild;
    }

    internal async Task<KuracordGuild> CreateGuildAsync(string name, Optional<string> iconBase64) {
        RestGuildCreatePayload payload = new() {
            Name = name,
            IconBase64 = iconBase64
        };

        Uri uri = Utilities.GetApiUriFor(Endpoints.Guilds);

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;
        
        foreach (KuracordRole role in guild._roles ??= new List<KuracordRole>()) {
            role.Kuracord = Kuracord;
            role._guildId = guild.Id;
        }

        foreach (KuracordMember member in guild._members ??= new List<KuracordMember>()) {
            member.Kuracord = Kuracord;
            member._guildId = guild.Id;
        }

        foreach (KuracordChannel channel in guild._channels ??= new List<KuracordChannel>()) {
            channel.Kuracord = Kuracord;
            channel.GuildId = guild.Id;
        }
        
        guild.Kuracord = Kuracord;

        return guild;
    }

    internal async Task<KuracordGuild> ModifyGuildAsync(ulong guildId, string name) {
        RestGuildModifyPayload payload = new(name);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;

        foreach (KuracordRole role in guild._roles ??= new List<KuracordRole>()) {
            role.Kuracord = Kuracord;
            role._guildId = guild.Id;
        }

        foreach (KuracordMember member in guild._members ??= new List<KuracordMember>()) {
            member.Kuracord = Kuracord;
            member._guildId = guild.Id;
        }

        foreach (KuracordChannel channel in guild._channels ??= new List<KuracordChannel>()) {
            channel.Kuracord = Kuracord;
            channel.GuildId = guild.Id;
        }

        return guild;
    }

    internal Task DeleteGuildAsync(ulong guildId, string password) {
        RestGuildDeletePayload payload = new(password); 
        
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}");

        return DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE, null, KuracordJson.SerializeObject(payload));
    }

    internal Task LeaveGuildAsync(ulong guildId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}/@me");

        return DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE);
    }

    #endregion

    #region Channel
    
    internal async Task<KuracordChannel> GetChannelAsync(ulong guildId, ulong channelId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);
        
        KuracordChannel channel = JsonConvert.DeserializeObject<KuracordChannel>(rest.Response)!;
        channel.Kuracord = Kuracord;
        
        return channel;
    }

    internal async Task<KuracordChannel> CreateChannelAsync(ulong guildId, string name) {
        if (name is not { Length: > 0 }) throw new ArgumentException("Channel name must not be empty.");

        RestChannelCreateModifyPayload payload = new(name);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordChannel channel = JsonConvert.DeserializeObject<KuracordChannel>(rest.Response)!;
        channel.Kuracord = Kuracord;
        channel.GuildId = guildId;

        return channel;
    }

    internal async Task<KuracordChannel> ModifyChannelAsync(ulong guildId, ulong channelId, string name) {
        if (name is not { Length: > 0 }) throw new ArgumentException("Channel name must not be empty.");
        
        RestChannelCreateModifyPayload payload = new(name);
        
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordChannel channel = JsonConvert.DeserializeObject<KuracordChannel>(rest.Response)!;
        channel.Kuracord = Kuracord;

        return channel;
    }

    internal async Task<IReadOnlyList<KuracordChannel>> GetChannelsAsync(ulong guildId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);

        List<KuracordChannel> channels = JsonConvert.DeserializeObject<List<KuracordChannel>>(rest.Response)!;
        foreach (KuracordChannel channel in channels) {
            channel.Kuracord = Kuracord;
            channel.GuildId = guildId;
        }

        return channels;
    }

    internal Task DeleteChannelAsync(ulong guildId, ulong channelId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}");

        return DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE);
    }

    #endregion

    #region Message

    internal async Task<KuracordMessage> GetMessageAsync(ulong guildId, ulong channelId, ulong messageId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}{Endpoints.Messages}/{messageId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);
        
        KuracordMessage message = PrepareMessage(JObject.Parse(rest.Response));
        return message;
    }

    internal async Task<IReadOnlyList<KuracordMessage>> GetMessagesAsync(ulong guildId, ulong channelId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}{Endpoints.Messages}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);
        JArray messagesArr = JArray.Parse(rest.Response);
        
        return messagesArr.Select(PrepareMessage).ToList();
    }

    internal Task DeleteMessageAsync(ulong guildId, ulong channelId, ulong messageId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}{Endpoints.Messages}/{messageId}");
        
        return DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE);
    }

    internal async Task<KuracordMessage> CreateMessageAsync(ulong guildId, ulong channelId, string content) {
        if (content is not { Length: > 0 }) throw new ArgumentException("Message content must not be empty.");

        RestChannelMessageCreatePayload payload = new(content);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}{Endpoints.Messages}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordMessage message = PrepareMessage(JObject.Parse(rest.Response));
        return message;
    }

    internal async Task<KuracordMessage> EditMessageAsync(ulong guildId, ulong channelId, ulong messageId, string content) {
        if (content is not { Length: > 0 }) throw new ArgumentException("Message content must not be empty.");

        RestChannelMessageModifyPayload payload = new(content);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}/{channelId}{Endpoints.Messages}/{messageId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);
        KuracordMessage message = PrepareMessage(JObject.Parse(rest.Response));

        return message;
    }

    #endregion

    #region Invite

    internal async Task<KuracordGuild> AcceptInviteAsync(string inviteCode) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Invites}/{inviteCode}");

        RestResponse mbrRest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST).ConfigureAwait(false);
        KuracordMember member = JsonConvert.DeserializeObject<KuracordMember>(mbrRest.Response)!;

        uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{member.Guild.Id}");

        RestResponse gldRest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);
        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(gldRest.Response)!;
        guild.Kuracord = Kuracord;

        return guild;
    }

    internal async Task<KuracordInviteGuild> GetInviteInfoAsync(string inviteCode) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Invites}/{inviteCode}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);

        KuracordInviteGuild inviteGuild = JsonConvert.DeserializeObject<KuracordInviteGuild>(rest.Response)!;
        inviteGuild.Kuracord = Kuracord;
        
        return inviteGuild;
    }

    #endregion

    #region Member & User

    internal Task<KuracordUser> GetCurrentUserAsync() => GetUserAsync("@me");
    
    internal Task<KuracordUser> GetUserAsync(ulong userId) => GetUserAsync($"{userId}");
    
    internal async Task<KuracordUser> GetUserAsync(string userId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Users}/{userId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);

        KuracordUser user = JsonConvert.DeserializeObject<KuracordUser>(rest.Response)!;
        user.Kuracord = Kuracord;
        
        return user;
    }

    internal async Task<KuracordUser> ModifyCurrentUserAsync(string username, string discriminator) {
        RestUserUpdatePayload payload = new(username) { Discriminator = discriminator };

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Users}/@me");
        await DoRequestAsync(Kuracord, uri, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordUser user = await GetCurrentUserAsync().ConfigureAwait(false);
        user.Kuracord = Kuracord;

        return user;
    }

    internal async Task DisableCurrentUserAsync() {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Users}/@me");
        await DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE).ConfigureAwait(false);

        Kuracord.Logger.LogWarning(LoggerEvents.Misc, "Current user is successfully disabled. Disposing client...");
        Kuracord.Dispose();
    }

    internal async Task<KuracordUser> RegisterUserAsync(string username, string email, string password) {
        RestUserRegisterPayload payload = new(username, email, password);

        Uri uri = Utilities.GetApiUriFor(Endpoints.Users);
        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);
        JObject userObj = JObject.Parse(rest.Response);

        KuracordUser user = userObj.ToKuracordObject<KuracordUser>();
        user.Kuracord = Kuracord;
        user.GuildsMember = new List<KuracordMember>();
        user.Token = userObj["token"]!.ToString();

        return user;
    }

    internal async Task<string> GetUserTokenAsync(string email, string password) {
        RestUserLoginPayload payload = new(email, password);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Users}/login");
        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        string token = JObject.Parse(rest.Response)["token"]!.ToString();
        return token;
    }

    internal async Task<IReadOnlyList<KuracordMember>> GetMembersAsync(ulong guildId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);

        List<KuracordMember> members = JsonConvert.DeserializeObject<List<KuracordMember>>(rest.Response)!;
        foreach (KuracordMember member in members) {
            member.Kuracord = Kuracord;
            member._guildId = guildId;
        }
        
        return members;
    }

    internal async Task<KuracordMember> GetMemberAsync(ulong guildId, ulong memberId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}/{memberId}");

        RestResponse rest = await DoRequestAsync(Kuracord, uri, RestRequestMethod.GET).ConfigureAwait(false);

        KuracordMember member = JsonConvert.DeserializeObject<KuracordMember>(rest.Response)!;
        member.Kuracord = Kuracord;

        Kuracord.UpdateUserCache(member.User);

        return member;
    }

    internal async Task<KuracordMember> ModifyMemberAsync(ulong guildId, ulong memberId, string? nickname) {
        RestGuildMemberModifyPayload payload = new(nickname);

        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}/{memberId}");
        await DoRequestAsync(Kuracord, uri, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordMember member = await GetMemberAsync(guildId, memberId).ConfigureAwait(false);
        member.Kuracord = Kuracord;

        Kuracord.UpdateUserCache(member.User);

        return member;
    }
    
    internal Task DeleteMemberAsync(ulong guildId, ulong memberId) {
        Uri uri = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}/{memberId}");
        
        return DoRequestAsync(Kuracord, uri, RestRequestMethod.DELETE);
    }

    #endregion
}