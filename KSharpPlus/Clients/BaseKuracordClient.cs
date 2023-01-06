using System.Collections.Concurrent;
using System.Reflection;
using KSharpPlus.Entities.Guild;
using KSharpPlus.Entities.User;
using KSharpPlus.Logging;
using KSharpPlus.Net.Rest;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Clients; 

public abstract class BaseKuracordClient : IDisposable {
    #region Fields and Properties

    protected internal KuracordApiClient ApiClient { get; }
    protected internal KuracordConfiguration Configuration { get; }

    /// <summary>
    /// Gets the instance of the logger for this client.
    /// </summary>
    public ILogger<BaseKuracordClient> Logger { get; }
    
    /// <summary>
    /// Gets the string representing the version of K#+.
    /// </summary>
    public string VersionString { get; }
    
    /// <summary>
    /// Gets the current user.
    /// </summary>
    public KuracordUser? CurrentUser { get; internal set; }
    
    /// <summary>
    /// Gets the cached guilds for this client.
    /// </summary>
    public abstract IReadOnlyDictionary<ulong, KuracordGuild> Guilds { get; }
    
    /// <summary>
    /// Gets the cached users for this client.
    /// </summary>
    protected internal ConcurrentDictionary<ulong, KuracordUser> UserCache { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Initializes this client. This method fetches information about current user, application, and voice regions.
    /// </summary>
    /// <returns></returns>
    public virtual async Task InitializeAsync() {
        if (CurrentUser == null) {
            CurrentUser = await ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
            UpdateUserCache(CurrentUser);
        }

        //todo bot
        /*if (Configuration.TokenType == TokenType.Bot && CurrentApplication == null)
            CurrentApplication = await GetCurrentApplicationAsync().ConfigureAwait(false);*/
    }

    #endregion
    
    protected BaseKuracordClient(KuracordConfiguration config) {
        Configuration = new KuracordConfiguration(config);

        if (Configuration.LoggerFactory == null) {
            Configuration.LoggerFactory = new DefaultLoggerFactory();
            Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
        }

        Logger = Configuration.LoggerFactory.CreateLogger<BaseKuracordClient>();

        ApiClient = new KuracordApiClient(this);
        UserCache = new ConcurrentDictionary<ulong, KuracordUser>();

        Assembly assembly = typeof(KuracordClient).GetTypeInfo().Assembly;
        AssemblyInformationalVersionAttribute? infVer = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (infVer != null) VersionString = infVer.InformationalVersion;
        else {
            Version ver = assembly.GetName().Version!;
            string verStr = ver.ToString(3);

            if (ver.Revision > 0) VersionString = $"{verStr}, CI build {ver.Revision}";
        }
    }
    
    internal KuracordUser UpdateUserCache(KuracordUser newUser) => UserCache.AddOrUpdate(newUser.Id, newUser, (_, _) => newUser);
    
    public abstract void Dispose();
}