using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;
using KSharpPlus.EventArgs;
using KSharpPlus.EventArgs.Channel;
using KSharpPlus.EventArgs.Guild;
using KSharpPlus.EventArgs.Guild.Member;
using KSharpPlus.EventArgs.Message;
using KSharpPlus.EventArgs.User;
using KSharpPlus.Logging;
using KSharpPlus.Net.Abstractions.Gateway;
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

        KuracordGuild guild;
        KuracordChannel channel;
        ulong guildId;
        ulong channelId;
        KuracordUser? user;
        KuracordMember? member;

        switch (payload.EventName.ToLowerInvariant()) {

            #region Guild

            case "guild_create":
                await OnGuildCreateEventAsync(data.ToKuracordObject<KuracordGuild>(), (JArray)data["members"]!).ConfigureAwait(false);
                break;
                
            case "guild_update":
                await OnGuildUpdateEventAsync(data.ToKuracordObject<KuracordGuild>(), (JArray)data["members"]!).ConfigureAwait(false);
                break;
            
            case "guild_remove":
                await OnGuildRemoveEventAsync((ulong)data["guildId"]!).ConfigureAwait(false);
                break;

            #endregion

            #region Channel

            case "channel_create":
                channel = data.ToKuracordObject<KuracordChannel>();
                await OnChannelCreateEventAsync(channel).ConfigureAwait(false); 
                break;
            
            case "channel_update":
                await OnChannelUpdateEventAsync(data.ToKuracordObject<KuracordChannel>()).ConfigureAwait(false);
                break;
            
            case "channel_remove":
                await OnChannelDeleteEventAsync((ulong)data["guildId"]!, (ulong)data["channelId"]!).ConfigureAwait(false);
                break;

            #endregion

            #region Message

            case "message_create":
                member = data["member"]?.ToKuracordObject<KuracordMember>();

                await OnMessageCreateEventAsync(data.ToKuracordObject<KuracordMessage>(), data["author"]!.ToKuracordObject<KuracordUser>(), member!).ConfigureAwait(false);
                break;
            
            case "message_update":
                member = data["member"]?.ToKuracordObject<KuracordMember>();

                await OnMessageUpdateEventAsync(data.ToKuracordObject<KuracordMessage>(), data["author"]!.ToKuracordObject<KuracordUser>(), member!).ConfigureAwait(false);
                break;
            
            // delete event does *not* include message object
            case "message_delete":
                await OnMessageDeleteEventAsync((ulong)data["messageId"]!, (ulong)data["channelId"]!, (ulong?)data["guildId"]).ConfigureAwait(false);
                break;

            #endregion

            #region Member

            case "member_join":
                guildId = (ulong)data["guild"]!["id"]!;
                await OnMemberJoinedEventAsync(data.ToKuracordObject<KuracordMember>(), _guilds[guildId]).ConfigureAwait(false);
                break;
            
            case "member_update":
                guildId = (ulong)data["guild"]!["id"]!;
                await OnMemberUpdatedEventAsync(data.ToKuracordObject<KuracordMember>(), _guilds[guildId]).ConfigureAwait(false);
                break;
            
            case "member_leave":
                ulong userId = (ulong)data["userId"]!;
                ulong memberId = (ulong)data["memberId"]!;
                guildId = (ulong)data["guildId"]!;

                if (!_guilds.ContainsKey(guildId)) {
                    if (userId != CurrentUser.Id) Logger.LogError(LoggerEvents.WebSocketReceive, $"Could not find {guildId} in guild cache");
                    return;
                }

                member = _guilds[guildId].Members[memberId];

                await OnMemberLeaveEventAsync(member, _guilds[guildId]).ConfigureAwait(false);
                break;

            #endregion

            #region User

            case "user_update":
                await OnUserUpdateEventAsync(data["oldUser"]!.ToKuracordObject<KuracordUser>(), data["newUser"]!.ToKuracordObject<KuracordUser>()).ConfigureAwait(false);
                break;

            #endregion

            #region Misc

            default:
                await OnUnknownEventAsync(payload).ConfigureAwait(false);
                if (Configuration.LogUnknownEvents) 
                    Logger.LogWarning(LoggerEvents.WebSocketReceive, $"Unknown event: {payload.EventName}; Data:\n{payload.Data}");
                break;

            #endregion
            
        }
    }

    #endregion

    #region Events

    #region Guild

    internal async Task OnGuildCreateEventAsync(KuracordGuild guild, JArray rawMembers) {
        bool exists = _guilds.TryGetValue(guild.Id, out KuracordGuild? foundGuild);

        guild.Kuracord = this;
        KuracordGuild eventGuild = guild;
        
        if (exists && foundGuild != null) guild = foundGuild;

        guild._channels ??= new List<KuracordChannel>();
        guild._roles ??= new List<KuracordRole>();
        guild._members ??= new SynchronizedCollection<KuracordMember>();

        UpdateCachedGuild(guild, rawMembers);

        guild.VanityCode = eventGuild.VanityCode;
        guild.Description = eventGuild.Description;

        foreach (KuracordChannel channel in guild._channels) {
            channel.GuildId = guild.Id;
            channel.Kuracord = this;
        }

        foreach (KuracordRole role in guild._roles) {
            role.Kuracord = this;
            role._guildId = guild.Id;
        }

        bool old = Volatile.Read(ref _guildDownloadCompleted);
        Volatile.Write(ref _guildDownloadCompleted, true);

        if (exists)
            await _guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(guild)).ConfigureAwait(false);
        else
            await _guildCreated.InvokeAsync(this, new GuildCreateEventArgs(guild)).ConfigureAwait(false);

        if (!old)
            await _guildDownloadCompletedEvent.InvokeAsync(this, new GuildDownloadCompletedEventArgs(Guilds)).ConfigureAwait(false);
    }

    internal async Task OnGuildUpdateEventAsync(KuracordGuild guild, JArray rawMembers) {
        KuracordGuild oldGuild;

        if (!_guilds.ContainsKey(guild.Id)) {
            _guilds[guild.Id] = guild;
            oldGuild = null!;
        } else {
            KuracordGuild gld = _guilds[guild.Id];

            oldGuild = new KuracordGuild {
                Kuracord = gld.Kuracord,
                Name = gld.Name,
                Description = gld.Description,
                VanityCode = gld.VanityCode,
                IconHash = gld.IconHash,
                Id = gld.Id,
                Disabled = gld.Disabled,
                Owner = gld.Owner,
                CreationTimestamp = gld.CreationTimestamp,
                ShortName = gld.ShortName,
                _isSynced = gld._isSynced,
                _channels = gld._channels ??= new List<KuracordChannel>(),
                _members = gld._members ??= new SynchronizedCollection<KuracordMember>(),
                _roles = gld._roles ??= new List<KuracordRole>()
            };
        }

        guild.Kuracord = this;

        KuracordGuild eventGuild = guild;
        guild = _guilds[eventGuild.Id];

        guild._channels ??= new List<KuracordChannel>();
        guild._roles ??= new List<KuracordRole>();
        guild._members ??= new SynchronizedCollection<KuracordMember>();
            
        UpdateCachedGuild(eventGuild, rawMembers);

        foreach (KuracordChannel channel in guild._channels) {
            channel.GuildId = guild.Id;
            channel.Kuracord = this;
        }

        foreach (KuracordRole role in guild._roles) {
            role._guildId = guild.Id;
            role.Kuracord = this;
        }

        foreach (KuracordMember member in guild._members) {
            member._guildId = guild.Id;
            member.Kuracord = this;
        }
        
        await _guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs(oldGuild, guild)).ConfigureAwait(false);
    }

    internal async Task OnGuildRemoveEventAsync(ulong guildId) {
        if (!_guilds.TryRemove(guildId, out KuracordGuild? guild)) return;

        await _guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs(guild)).ConfigureAwait(false);
    }

    #endregion
    
    #region Channel

    internal async Task OnChannelCreateEventAsync(KuracordChannel channel) {
        channel.Kuracord = this;
        channel.GuildId = channel.Guild!.Id;

        _guilds[channel.GuildId.Value]._channels ??= new List<KuracordChannel>();
        
        if (_guilds[channel.GuildId.Value]._channels!.All(c => c.Id != channel.Id)) _guilds[channel.GuildId.Value]._channels!.Add(channel);

        await _channelCreated.InvokeAsync(this, new ChannelCreateEventArgs(channel)).ConfigureAwait(false);
    }

    internal async Task OnChannelUpdateEventAsync(KuracordChannel channel) {
        channel.Kuracord = this;
        
        KuracordGuild guild = channel.Guild!;
        KuracordChannel? channelNew = InternalGetCachedChannel(channel.Id);
        KuracordChannel channelOld = null!;

        if (channelNew != null) {
            channelOld = new KuracordChannel {
                Kuracord = this,
                GuildId = channelNew.GuildId,
                Id = channelNew.Id,
                Name = channelNew.Name,
                Type = channelNew.Type
            };

            channelNew.Name = channel.Name;
            channelNew.Type = channel.Type;
        } else if (guild != null!) guild._channels!.Replace(c => c.Id == channel.Id, channel);
        
        await _channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs(channelOld, channelNew!, guild!)).ConfigureAwait(false);
    }

    internal async Task OnChannelDeleteEventAsync(ulong guildId, ulong channelId) {
        if (!_guilds.TryGetValue(guildId, out KuracordGuild? guild)) return;
        guild.Kuracord = this;
        
        KuracordChannel? channel = guild._channels?.FirstOrDefault(c => c.Id == channelId);
        if (channel == null) return;

        channel.Kuracord = this;

        guild._channels?.RemoveAll(c => c.Id == channelId);

        await _channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs(guild, channel)).ConfigureAwait(false);
    }

    #endregion

    #region Message

    internal async Task OnMessageCreateEventAsync(KuracordMessage message, KuracordUser author, KuracordMember member) {
        message.Kuracord = this;
        message.Guild.Kuracord = this;
        message.Channel.Kuracord = this;
        author.Kuracord = this;
        member.Kuracord = this;
        member.User = author;
        
        CacheMessage(message, author, member);
        
        if (message.Channel == null!) Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");

        MessageCreateEventArgs args = new(message, message.Guild, message.Channel!, author, member);
        
        await _messageCreated.InvokeAsync(this, args).ConfigureAwait(false);
    }

    internal async Task OnMessageUpdateEventAsync(KuracordMessage message, KuracordUser author, KuracordMember member) {
        message.Kuracord = this;
        message.Guild.Kuracord = this;
        message.Channel.Kuracord = this;
        author.Kuracord = this;
        member.Kuracord = this;
        member.User = author;

        KuracordMessage eventMessage = message;

        KuracordMessage oldMessage = null!;

        if (Configuration.MessageCacheSize == 0 ||
            MessageCache == null ||
            !MessageCache.TryGet(m => m.Id == eventMessage.Id && m.ChannelId == eventMessage.ChannelId, out message)) message = eventMessage;
        else {
            oldMessage = new KuracordMessage(message);

            message.EditedTimestamp = eventMessage.EditedTimestamp;
            message.Content = eventMessage.Content;
            message._attachments.Clear();
            message._attachments.AddRange(eventMessage._attachments);
        }

        MessageUpdateEventArgs args = new(oldMessage, message, message.Guild, message.Channel, author, member);
        
        await _messageUpdated.InvokeAsync(this, args).ConfigureAwait(false);
    }

    internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId) {
        KuracordGuild? guild = guildId.HasValue ? InternalGetCachedGuild(guildId) ?? await GetGuildAsync(guildId.Value) : null;
        KuracordChannel? channel = InternalGetCachedChannel(channelId) ?? (guildId.HasValue ? await GetChannelAsync(guildId.Value, channelId) : null);
        
        if (channel == null ||
            Configuration.MessageCacheSize == 0 ||
            MessageCache == null ||
            !MessageCache.TryGet(m => m.Id == messageId && m.ChannelId == channelId, out KuracordMessage? message)) 
            message = new KuracordMessage {
                Kuracord = this,
                Id = messageId
            };
        
        if (Configuration.MessageCacheSize > 0) MessageCache?.Remove(m => m.Id == message.Id && m.ChannelId == channelId);

        MessageDeleteEventArgs args = new(guild!, channel!, message);

        await _messageDeleted.InvokeAsync(this, args).ConfigureAwait(false);
    }

    #endregion

    #region Member

    internal async Task OnMemberJoinedEventAsync(KuracordMember member, KuracordGuild guild) {
        member.Kuracord = this;
        member._guildId = guild.Id;
        member.User.Kuracord = this;

        UpdateUserCache(member.User);
        
        if (guild._members!.All(m => m.Id != member.Id)) guild._members?.Add(member);

        MemberJoinedEventArgs args = new(member, guild);
        
        await _memberJoined.InvokeAsync(this, args).ConfigureAwait(false);
    }

    internal async Task OnMemberUpdatedEventAsync(KuracordMember memberAfter, KuracordGuild guild) {
        memberAfter.Kuracord = this;
        memberAfter._guildId = guild.Id;
        memberAfter.User.Kuracord = this;

        UpdateUserCache(memberAfter.User);

        if (!guild.Members.TryGetValue(memberAfter.Id, out KuracordMember? memberBefore)) memberBefore = memberAfter;

        foreach (KuracordMember mbr in guild._members!.Where(m => m == memberBefore)) guild._members?.Remove(mbr);
        
        guild._members!.Add(memberAfter);

        MemberUpdatedEventArgs args = new(memberBefore, memberAfter, guild);

        await _memberUpdated.InvokeAsync(this, args).ConfigureAwait(false);
    }

    internal async Task OnMemberLeaveEventAsync(KuracordMember member, KuracordGuild guild) {
        member.Kuracord = this;
        member._guildId = guild.Id;

        guild._members?.Remove(member);
        
        UpdateUserCache(member.User);

        MemberLeaveEventArgs args = new(member, guild);

        await _memberLeave.InvokeAsync(this, args).ConfigureAwait(false);
    }

    #endregion

    #region User

    internal async Task OnUserUpdateEventAsync(KuracordUser oldUser, KuracordUser newUser) {
        oldUser.Kuracord = newUser.Kuracord = this;
        
        //some properties are commented because server sends these fields as null instead of it real values 
        if (newUser.IsCurrent) {
            CurrentUser.AvatarUrl = newUser.AvatarUrl;
            CurrentUser.Username = newUser.Username;
            CurrentUser.Discriminator = newUser.Discriminator;
            CurrentUser.Biography = newUser.Biography;
            CurrentUser.Flags = newUser.Flags;
            //CurrentUser.Email = newUser.Email;
            CurrentUser.IsBot = newUser.IsBot;
            //CurrentUser.Disabled = newUser.Disabled;
            //CurrentUser.Verified = newUser.Verified;
            //CurrentUser.PremiumType = newUser.PremiumType;
            CurrentUser.Id = newUser.Id;
        }

        UpdateUserCache(newUser);

        foreach (KuracordMember member in Guilds.Values.SelectMany(g => g.Members.Values).Where(m => m.User == oldUser)) member.User = newUser;

        //TODO: this will be used in backend v4
        /*foreach (KuracordGuild guild in Guilds.Values)
            if (guild.Members.TryGetValue(oldUser.Id, out KuracordMember? member)) member.User = newUser;*/
    
        UserUpdateEventArgs args = new(oldUser, newUser);
        await _userUpdated.InvokeAsync(this, args).ConfigureAwait(false);
    }

    #endregion

    #region Misc

    internal async Task OnUnknownEventAsync(GatewayPayload payload) {
        UnknownEventArgs args = new(payload.EventName, (payload.Data as JObject)?.ToString() ?? "");
        await _unknownEvent.InvokeAsync(this, args).ConfigureAwait(false);
    }

    #endregion

    #endregion
}