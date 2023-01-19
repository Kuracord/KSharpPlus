using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Channel; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.ChannelUpdated"/> event.
/// </summary>
public class ChannelUpdateEventArgs : KuracordEventArgs {
    internal ChannelUpdateEventArgs(KuracordChannel channelBefore, KuracordChannel channelAfter, KuracordGuild guild) {
        ChannelAfter = channelAfter;
        ChannelBefore = channelBefore;
        Guild = guild;
    }

    /// <summary>
    /// Gets the pre-update channel.
    /// </summary>
    public KuracordChannel ChannelBefore { get; }
    
    /// <summary>
    /// Gets the post-update channel.
    /// </summary>
    public KuracordChannel ChannelAfter { get; }
    
    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public KuracordGuild Guild { get; }
}