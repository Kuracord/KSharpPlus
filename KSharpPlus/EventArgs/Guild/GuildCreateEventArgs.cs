using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.GuildCreated"/> event.
/// </summary>
public class GuildCreateEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the guild that was created.
    /// </summary>
    public KuracordGuild Guild { get; }

    internal GuildCreateEventArgs(KuracordGuild guild) => Guild = guild;
}