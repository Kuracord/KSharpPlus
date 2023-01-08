using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;

namespace KSharpPlus.EventArgs.Message; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MessageCreated"/> event.
/// </summary>
public class MessageCreateEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the message that was created.
    /// </summary>
    public KuracordMessage Message { get; }

    /// <summary>
    /// Gets the guild this message belongs to.
    /// </summary>
    public KuracordGuild Guild { get; }
    
    /// <summary>
    /// Gets the channel this message belongs to.
    /// </summary>
    public KuracordChannel Channel { get; }
    
    /// <summary>
    /// Gets the author of the message.
    /// </summary>
    public KuracordUser Author { get; }
    
    /// <summary>
    /// Gets the guild member who is the author of the message.
    /// </summary>
    public KuracordMember Member { get; }

    internal MessageCreateEventArgs(KuracordMessage message, KuracordGuild guild, KuracordChannel channel, KuracordUser author, KuracordMember member) {
        Message = message;
        Guild = guild;
        Channel = channel;
        Author = author;
        Member = member;
    }
}