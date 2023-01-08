using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild.Member; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MemberJoined"/> event.
/// </summary>
public class MemberJoinedEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the member that was added.
    /// </summary>
    public KuracordMember Member { get; }

    /// <summary>
    /// Gets the guild the member was added to.
    /// </summary>
    public KuracordGuild Guild { get; }

    internal MemberJoinedEventArgs(KuracordMember member, KuracordGuild guild) {
        Member = member;
        Guild = guild;
    }
}