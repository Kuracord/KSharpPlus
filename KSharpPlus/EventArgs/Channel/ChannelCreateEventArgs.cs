using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Channel; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.ChannelCreated"/> event.
/// </summary>
public class ChannelCreateEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the channel that was created.
    /// </summary>
    public KuracordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the guild in which the channel was created.
    /// </summary>
    public KuracordGuild Guild { get; internal set; }

    internal ChannelCreateEventArgs(KuracordChannel channel) {
        Channel = channel;
        Guild = channel.Guild;
    }
}