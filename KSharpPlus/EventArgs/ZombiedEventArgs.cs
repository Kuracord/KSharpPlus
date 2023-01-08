using KSharpPlus.Clients;

namespace KSharpPlus.EventArgs; 

/// <summary>
/// Represents arguments for <see cref="KuracordClient.Zombied"/> event.
/// </summary>
public class ZombiedEventArgs : KuracordEventArgs {
    /// <summary>
    /// Gets how many heartbeat failures have occured.
    /// </summary>
    public int Failures { get; }

    /// <summary>
    /// Gets whether the zombie event occured whilst guilds are downloading.
    /// </summary>
    public bool GuildDownloadCompleted { get; }

    internal ZombiedEventArgs(int failures, bool guildDownloadCompleted) {
        Failures = failures;
        GuildDownloadCompleted = guildDownloadCompleted;
    }
}