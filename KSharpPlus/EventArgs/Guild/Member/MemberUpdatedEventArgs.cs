using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild.Member; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.MemberUpdated"/> event.
/// </summary>
public class MemberUpdatedEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public KuracordGuild Guild { get; }

    /// <summary>
    /// Get the member with post-update info
    /// </summary>
    public KuracordMember MemberAfter { get; }

    /// <summary>
    /// Get the member with pre-update info
    /// </summary>
    public KuracordMember MemberBefore { get; }
    
    /// <summary>
    /// Gets the member's new nickname.
    /// </summary>
    public string? NicknameAfter => MemberAfter.Nickname;

    /// <summary>
    /// Gets the member's old nickname.
    /// </summary>
    public string? NicknameBefore => MemberBefore.Nickname;

    internal MemberUpdatedEventArgs(KuracordMember memberBefore, KuracordMember memberAfter, KuracordGuild guild) {
        Guild = guild;
        MemberBefore = memberBefore;
        MemberAfter = memberAfter;
    }
}