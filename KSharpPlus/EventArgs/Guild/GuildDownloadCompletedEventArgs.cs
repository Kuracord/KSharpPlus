using KSharpPlus.Clients;
using KSharpPlus.Entities.Guild;

namespace KSharpPlus.EventArgs.Guild; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.GuildDownloadCompleted"/> event.
/// </summary>
public class GuildDownloadCompletedEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets the dictionary of guilds that just finished downloading.
    /// </summary>
    public IReadOnlyDictionary<ulong, KuracordGuild> Guilds { get; }

    internal GuildDownloadCompletedEventArgs(IReadOnlyDictionary<ulong, KuracordGuild> guilds) => Guilds = guilds;
}