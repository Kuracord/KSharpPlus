using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using KSharpPlus.Clients;
using KSharpPlus.Entities;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;
using KSharpPlus.Logging;
using KSharpPlus.Net.Abstractions.Rest;
using KSharpPlus.Net.Abstractions.Transport;
using KSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KSharpPlus.Net.Rest; 

public sealed class KuracordApiClient {
    internal BaseKuracordClient Kuracord { get; }
    internal RestClient Rest { get; }

    internal KuracordApiClient(BaseKuracordClient client) {
        Kuracord = client;
        Rest = new RestClient(client);
    }
    
    // This is for meta-clients, such as the webhook client
    internal KuracordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger) => 
        Rest = new RestClient(proxy, timeout, useRelativeRateLimit, logger);

    Task<RestResponse> DoRequestAsync(BaseKuracordClient client, Uri uri, RestRequestMethod method, IReadOnlyDictionary<string, string> headers = null, string payload = null) {
        RestRequest request = new(client, uri, method, headers, payload);

        if (Kuracord != null) Rest.ExecuteRequestAsync(request).LogTaskFault(Kuracord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request.");
        else Rest.ExecuteRequestAsync(request);

        return request.WaitForCompletionAsync();
    }

    #region Guild

    internal async Task<KuracordGuild> GetGuildAsync(ulong guildId) {
        Uri url = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.GET).ConfigureAwait(false);

        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;
        guild.Kuracord = Kuracord;
        
        return guild;
    }

    internal async Task<KuracordGuild> CreateGuildAsync(string name, Optional<string> iconBase64) {
        RestGuildCreatePayload payload = new() {
            Name = name,
            IconBase64 = iconBase64
        };

        Uri url = Utilities.GetApiUriFor(Endpoints.Guilds);

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.POST, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;
        guild.Kuracord = Kuracord;
        
        if (Kuracord is KuracordClient client) await client.OnGuildCreateEventAsync(guild);
        return guild;
    }

    internal async Task<KuracordGuild> ModifyGuildAsync(ulong guildId, string name) {
        RestGuildModifyPayload payload = new() { Name = name };

        Uri url = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.PATCH, null, KuracordJson.SerializeObject(payload)).ConfigureAwait(false);

        KuracordGuild guild = JsonConvert.DeserializeObject<KuracordGuild>(rest.Response)!;
        guild.Kuracord = Kuracord;
        
        return guild;
    }

    internal async Task<List<KuracordMember>> GetGuildMembersAsync(ulong guildId) {
        Uri url = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Members}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.GET).ConfigureAwait(false);

        List<KuracordMember> members = JsonConvert.DeserializeObject<List<KuracordMember>>(rest.Response)!;
        members.ForEach(m => {
            m.Kuracord = Kuracord;
            m._guildId = guildId;
        });
        
        return members;
    }

    #endregion

    #region Channel

    internal async Task<KuracordChannel> GetChannelAsync(ulong guildId, ulong channelId) {
        Uri url = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.GET).ConfigureAwait(false);

        List<KuracordChannel> channels = JsonConvert.DeserializeObject<List<KuracordChannel>>(rest.Response)!;
        KuracordChannel channel = channels.First(c => c.Id == channelId);

        channel.Kuracord = Kuracord;
        channel.GuildId = guildId;

        return channel;
    }

    internal async Task<List<KuracordChannel>> GetGuildChannelsAsync(ulong guildId) {
        Uri url = Utilities.GetApiUriFor($"{Endpoints.Guilds}/{guildId}{Endpoints.Channels}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.GET).ConfigureAwait(false);

        List<KuracordChannel> channels = JsonConvert.DeserializeObject<List<KuracordChannel>>(rest.Response)!;
        channels.ForEach(c => {
            c.Kuracord = Kuracord;
            c.GuildId = guildId;
        });

        return channels;
    }

    #endregion

    #region Invite

    

    #endregion

    #region Member

    internal Task<KuracordUser> GetCurrentUserAsync() => GetUserAsync("@me");
    
    internal Task<KuracordUser> GetUserAsync(ulong userId) => GetUserAsync(userId.ToString(CultureInfo.InvariantCulture));
    
    internal async Task<KuracordUser> GetUserAsync(string userId) {
        Uri url = Utilities.GetApiUriFor($"{Endpoints.Users}/{userId}");

        RestResponse rest = await DoRequestAsync(Kuracord, url, RestRequestMethod.GET).ConfigureAwait(false);

        TransportUser? userRaw = JsonConvert.DeserializeObject<TransportUser>(rest.Response);
        KuracordUser user = new(userRaw) { Kuracord = Kuracord };

        return user;
    }

    #endregion
}