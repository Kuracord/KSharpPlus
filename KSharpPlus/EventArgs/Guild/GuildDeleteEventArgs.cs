using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.GuildDeleted"/> event.
/// </summary>
public class GuildDeleteEventArgs : KuracordEventArgs {
    internal GuildDeleteEventArgs(KuracordGuild guild) => Guild = guild;
    
    /// <summary>
    /// Gets the guild that was deleted.
    /// </summary>
    public KuracordGuild Guild { get; }
}