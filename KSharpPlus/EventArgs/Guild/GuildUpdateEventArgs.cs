using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.GuildUpdated"/> event.
/// </summary>
public class GuildUpdateEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the guild before it was updated.
    /// </summary>
    public KuracordGuild GuildBefore { get; internal set; }

    /// <summary>
    /// Gets the guild after it was updated.
    /// </summary>
    public KuracordGuild GuildAfter { get; internal set; }

    internal GuildUpdateEventArgs() { }
}