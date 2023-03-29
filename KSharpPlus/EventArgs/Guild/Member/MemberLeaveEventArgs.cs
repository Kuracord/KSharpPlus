using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild.Member; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MemberLeave"/> event.
/// </summary>
public class MemberLeaveEventArgs : KuracordEventArgs {
    internal MemberLeaveEventArgs(KuracordMember member, KuracordGuild guild) {
        Member = member;
        Guild = guild;
    }
    
    /// <summary>
    /// Gets the guild the member was removed from.
    /// </summary>
    public KuracordGuild Guild { get; }

    /// <summary>
    /// Gets the member that was removed.
    /// </summary>
    public KuracordMember Member { get; }
}