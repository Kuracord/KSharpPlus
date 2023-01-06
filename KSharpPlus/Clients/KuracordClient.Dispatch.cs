using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.Logging;
using KSharpPlus.Net.Abstractions.Gateway;
using KSharpPlus.Net.Abstractions.Transport;
using KSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Clients; 

public sealed partial class KuracordClient {
    string _sessionId;
    bool _guildDownloadCompleted;

    #region Dispatch Handler

    internal async Task HandleDispatchAsync(GatewayPayload payload) {
        if (payload.Data is not JObject data) {
            Logger.LogWarning(LoggerEvents.WebSocketReceive, $"Invalid payload body (this message is probably safe to ignore); opcode: {payload.OpCode} event: {payload.EventName}; payload: {payload.Data}");
            return;
        }

        KuracordChannel channel;
        ulong guildId;
        ulong channelId;
        TransportUser user = default;
        TransportMember member = default;
        TransportUser referencedUser = default;
        TransportMember referencedMember = default;
        JToken rawMember = default;
        JToken? rawReferencedMessage = data["referenced_message"];

        switch (payload.EventName.ToLowerInvariant()) {
            
            #region Guild

            case "guild_create":
                await OnGuildCreateEventAsync(data.ToKuracordObject<KuracordGuild>());
                break;
            
            case "guild_update":
                //todo
                break;

            #endregion

            #region Member

            case "member_update":
                //todo
                break;

            #endregion
            
            #region Message

            case "message_create":
                //todo
                break;
            
            case "message_update":
                //todo
                break;
            
            case "message_delete":
                //todo
                break;

            #endregion
            
        }
    }

    #endregion

    #region Events

    #region Guild

    internal async Task OnGuildCreateEventAsync(KuracordGuild guild) {
        bool exists = _guilds.TryGetValue(guild.Id, out KuracordGuild? foundGuild);

        guild.Kuracord = this;
        KuracordGuild eventGuild = guild;
        
        if (exists && foundGuild != null) guild = foundGuild;

        guild._channels ??= new List<KuracordChannel>();
        guild._roles ??= new List<KuracordRole>();
        guild._members ??= new List<KuracordMember>();

        UpdateCachedGuild(guild);

        guild.VanityCode = eventGuild.VanityCode;
        guild.Description = eventGuild.Description;

        foreach (KuracordChannel channel in guild._channels) {
            channel.GuildId = guild.Id;
            channel.Kuracord = this;
        }

        foreach (KuracordRole role in guild._roles) {
            role.Kuracord = this;
            role._guild_id = guild.Id;
        }

        bool old = Volatile.Read(ref _guildDownloadCompleted);
        Volatile.Write(ref _guildDownloadCompleted, true);

        if (exists)
            await _guildAvailable.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild }).ConfigureAwait(false);
        else
            await _guildCreated.InvokeAsync(this, new GuildCreateEventArgs { Guild = guild }).ConfigureAwait(false);

        if (!old)
            await _guildDownloadCompletedEvent.InvokeAsync(this, new GuildDownloadCompletedEventArgs(Guilds)).ConfigureAwait(false);
    }

    #endregion

    #endregion
}