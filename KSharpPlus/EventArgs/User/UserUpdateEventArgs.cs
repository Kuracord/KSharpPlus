using KSharpPlus.Clients;
using KSharpPlus.Entities.User;

namespace KSharpPlus.EventArgs.User; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.UserUpdated"/> event.
/// </summary>
public class UserUpdateEventArgs : KuracordEventArgs {
    internal UserUpdateEventArgs(KuracordUser userBefore, KuracordUser userAfter) {
        UserBefore = userBefore;
        UserAfter = userAfter;
    }
    
    /// <summary>
    /// Gets the pre-update user.
    /// </summary>
    public KuracordUser UserBefore { get; }
    
    /// <summary>
    /// Gets the post-update user.
    /// </summary>
    public KuracordUser UserAfter { get; }
}