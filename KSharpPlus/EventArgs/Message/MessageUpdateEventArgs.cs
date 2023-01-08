using KSharpPlus.Clients;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Channel.Message;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;

namespace KSharpPlus.EventArgs.Message; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MessageUpdated"/> event.
/// </summary>
public class MessageUpdateEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the message before it got updated. This property will be null if the message was not cached.
    /// </summary>
    public KuracordMessage? MessageBefore { get; }

    
    /// <summary>
    /// Gets the message that was updated.
    /// </summary>
    public KuracordMessage MessageAfter { get; }
    
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

    internal MessageUpdateEventArgs(KuracordMessage messageBefore, KuracordMessage messageAfter, KuracordGuild guild, KuracordChannel channel, KuracordUser author, KuracordMember member) {
        MessageBefore = messageBefore;
        MessageAfter = messageAfter;
        Guild = guild;
        Channel = channel;
        Author = author;
        Member = member;
    }
}