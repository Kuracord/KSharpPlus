using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Message; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MessageDeleted"/> event.
/// </summary>
public class MessageDeleteEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the guild this message belonged to.
    /// </summary>
    public KuracordGuild Guild { get; }
    
    /// <summary>
    /// Gets the channel this message belonged to.
    /// </summary>
    public KuracordChannel Channel { get; }
    
    /// <summary>
    /// Gets the message that was deleted.
    /// </summary>
    public KuracordMessage Message { get; }

    internal MessageDeleteEventArgs(KuracordGuild guild, KuracordChannel channel, KuracordMessage message) {
        Guild = guild;
        Channel = channel;
        Message = message;
    }
}