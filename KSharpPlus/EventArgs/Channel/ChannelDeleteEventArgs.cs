using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Channel; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.ChannelDeleted"/> event.
/// </summary>
public class ChannelDeleteEventArgs : KuracordEventArgs {
    internal ChannelDeleteEventArgs(KuracordGuild guild, KuracordChannel channel) {
        Guild = guild;
        Channel = channel;
    }
    
    /// <summary>
    /// Gets the channel that was deleted.
    /// </summary>
    public KuracordChannel Channel { get; }

    /// <summary>
    /// Gets the guild this channel belonged to.
    /// </summary>
    public KuracordGuild Guild { get; }
}