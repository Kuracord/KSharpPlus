using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KSharpPlus.Clients;
using KSharpPlus.Enums;
using KSharpPlus.Net.Rest;
using Microsoft.Extensions.Logging;

namespace KSharpPlus; 

/// <summary>
/// Various Kuracord-related utilities.
/// </summary>
public static class Utilities {
    /// <summary>
    /// Gets the version of the library
    /// </summary>
    static string VersionHeader { get; }

    static Dictionary<Permissions, string> PermissionStrings { get; }
    
    internal static UTF8Encoding UTF8 { get; } = new(false);

    static Utilities() {
        PermissionStrings = new Dictionary<Permissions, string>();
        Type type = typeof(Permissions);
        TypeInfo typeInfo = type.GetTypeInfo();
        IEnumerable<Permissions> values = Enum.GetValues(type).Cast<Permissions>();

        foreach (Permissions xv in values) {
            string xsv = xv.ToString();
            MemberInfo? xmv = typeInfo.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
            PermissionStringAttribute? xav = xmv?.GetCustomAttribute<PermissionStringAttribute>();

            PermissionStrings[xv] = xav?.String!;
        }

        Assembly a = typeof(KuracordClient).GetTypeInfo().Assembly;

        string vs;
        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null) vs = iv.InformationalVersion;
        else {
            Version? v = a.GetName().Version;
            vs = v?.ToString(3)!;
        }

        VersionHeader = $"KuracordBot (https://github.com/Kuracord/KSharpPlus, v{vs})";
    }
    
    internal static string GetApiBaseUri() => Endpoints.BaseURI;

    internal static Uri GetApiUriFor(string path) => new($"{GetApiBaseUri()}{path}");

    internal static Uri GetApiUriFor(string path, string queryString) => new($"{GetApiBaseUri()}{path}{queryString}");

    internal static QueryUriBuilder GetApiUriBuilderFor(string path) => new($"{GetApiBaseUri()}{path}");
    
    internal static string GetFormattedToken(BaseKuracordClient client) => GetFormattedToken(client.Configuration);

    internal static string GetFormattedToken(KuracordConfiguration config) => config.TokenType switch {
        TokenType.User => $"User {config.Token}",
        TokenType.Bearer => $"Bearer {config.Token}",
        TokenType.Bot => $"Bot {config.Token}",
        _ => throw new ArgumentException("Invalid token type specified.", nameof(config.Token)),
    };
    
    internal static string GetUserAgent()
        => VersionHeader;

    internal static bool ContainsUserMentions(string message) {
        Regex regex = new(@"<@(\d+)>", RegexOptions.ECMAScript);
        return regex.IsMatch(message);
    }

    internal static bool ContainsNicknameMentions(string message) {
        Regex regex = new(@"<@!(\d+)>", RegexOptions.ECMAScript);
        return regex.IsMatch(message);
    }

    internal static bool ContainsChannelMentions(string message) {
        Regex regex = new(@"<#(\d+)>", RegexOptions.ECMAScript);
        return regex.IsMatch(message);
    }

    internal static bool ContainsRoleMentions(string message) {
        Regex regex = new(@"<@&(\d+)>", RegexOptions.ECMAScript);
        return regex.IsMatch(message);
    }

    internal static bool ContainsEmojis(string message) {
        Regex regex = new(@"<a?:(.*):(\d+)>", RegexOptions.ECMAScript);
        return regex.IsMatch(message);
    }
    
    internal static void LogTaskFault(this Task task, ILogger logger, LogLevel level, EventId eventId, string message) {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (logger == null!) return;

        task.ContinueWith(t => logger.Log(level, eventId, t.Exception, message), TaskContinuationOptions.OnlyOnFaulted);
    }
    
    public static List<T> Replace<T>(this List<T> source, Predicate<T> predicate, T newValue) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (newValue == null) throw new ArgumentNullException(nameof(newValue));

        int index = source.FindIndex(predicate);
        if (index != -1) source[index] = newValue;

        return source;
    }
}