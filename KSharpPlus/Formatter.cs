using System.Globalization;
using KSharpPlus.Entities.Channel;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;

namespace KSharpPlus; 

public static class Formatter {
    /// <summary>
    /// Creates a block of code.
    /// </summary>
    /// <param name="content">Contents of the block.</param>
    /// <param name="language">Language to use for highlighting.</param>
    /// <returns>Formatted block of code.</returns>
    public static string BlockCode(string content, string language = "") => $"```{language}\n{content}\n```";
    
    /// <summary>
    /// Creates inline code snippet.
    /// </summary>
    /// <param name="content">Contents of the snippet.</param>
    /// <returns>Formatted inline code snippet.</returns>
    public static string InlineCode(string content) => $"`{content}`";
    
    /// <summary>
    /// Creates bold text.
    /// </summary>
    /// <param name="content">Text to bolden.</param>
    /// <returns>Formatted text.</returns>
    public static string Bold(string content) => $"**{content}**";

    /// <summary>
    /// Creates italicized text.
    /// </summary>
    /// <param name="content">Text to italicize.</param>
    /// <returns>Formatted text.</returns>
    public static string Italic(string content) => $"*{content}*";

    /// <summary>
    /// Creates spoiler from text.
    /// </summary>
    /// <param name="content">Text to spoilerize.</param>
    /// <returns>Formatted text.</returns>
    public static string Spoiler(string content) => $"||{content}||";
    
    /// <summary>
    /// Creates underlined text.
    /// </summary>
    /// <param name="content">Text to underline.</param>
    /// <returns>Formatted text.</returns>
    public static string Underline(string content) => $"__{content}__";

    /// <summary>
    /// Creates strikethrough text.
    /// </summary>
    /// <param name="content">Text to strikethrough.</param>
    /// <returns>Formatted text.</returns>
    public static string Strike(string content) => $"~~{content}~~";

    /// <summary>
    /// Creates a URL that won't create a link preview.
    /// </summary>
    /// <param name="url">Url to prevent from being previewed.</param>
    /// <returns>Formatted url.</returns>
    public static string EmbedlessUrl(Uri url) => $"<{url}>";
    
    /// <summary>
    /// Creates a mention for specified user or member. Can optionally specify to resolve nicknames.
    /// </summary>
    /// <param name="user">User to create mention for.</param>
    /// <param name="nickname">Whether the mention should resolve nicknames or not.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(KuracordUser user, bool nickname = false) => nickname
            ? $"<@!{user.Id.ToString(CultureInfo.InvariantCulture)}>"
            : $"<@{user.Id.ToString(CultureInfo.InvariantCulture)}>";
    
    /// <summary>
    /// Creates a mention for specified channel.
    /// </summary>
    /// <param name="channel">Channel to mention.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(KuracordChannel channel) => $"<#{channel.Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Creates a mention for specified role.
    /// </summary>
    /// <param name="role">Role to mention.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(KuracordRole role) => $"<@&{role.Id.ToString(CultureInfo.InvariantCulture)}>";
}