using System.Net;
using KSharpPlus.Enums;
using KSharpPlus.Net.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace KSharpPlus; 

/// <summary>
/// Represents configuration for <see cref="Clients.KuracordClient"/>.
/// </summary>
public sealed class KuracordConfiguration {
    /// <summary>
    /// Sets the token used to identify the client.
    /// </summary>
    public string Token {
        internal get => _token;
        init {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value), "Token cannot be null, empty, or all whitespace.");
            _token = value.Trim();
        }
    }

    readonly string _token = "";

    /// <summary>
    /// <para>Sets the type of the token used to identify the client.</para>
    /// <para>Defaults to <see cref="Enums.TokenType.Bot"/>.</para>
    /// </summary>
    public TokenType TokenType { internal get; set; } = TokenType.Bot;
    
    /// <summary>
    /// <para>Sets the minimum logging level for messages.</para>
    /// <para>Typically, the default value of <see cref="Information"/> is ok for most uses.</para>
    /// </summary>
    public LogLevel MinimumLogLevel { internal get; set; } = LogLevel.Information;
    
    /// <summary>
    /// <para>Sets whether to rely on Kuracord for NTP (Network Time Protocol) synchronization with the "X-Ratelimit-Reset-After" header.</para>
    /// <para>If the system clock is not synced, setting this to true will ensure ratelimits are synced with Kuracord and reduce the risk of hitting one.</para>
    /// <para>This should only be set to false if the system clock is synced with NTP.</para>
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool UseRelativeRatelimit { internal get; set; } = true;

    
    /// <summary>
    /// <para>Allows you to overwrite the time format used by the internal debug logger.</para>
    /// <para>Only applicable when <see cref="LoggerFactory"/> is set left at default value. Defaults to ISO 8601-like format.</para>
    /// </summary>
    public string LogTimestampFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";
    
    /// <summary>
    /// <para>Sets whether to automatically reconnect in case a connection is lost.</para>
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool AutoReconnect { internal get; set; } = true;

    /// <summary>
    /// <para>Sets the size of the global message cache.</para>
    /// <para>Setting this to 0 will disable message caching entirely. Defaults to 1024.</para>
    /// </summary>
    public int MessageCacheSize { internal get; set; } = 1024;

    /// <summary>
    /// <para>Sets the proxy to use for HTTP and WebSocket connections to Kuracord.</para>
    /// <para>Defaults to null.</para>
    /// </summary>
    public IWebProxy Proxy { internal get; set; } = null!;

    /// <summary>
    /// <para>Sets the timeout for HTTP requests.</para>
    /// <para>Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</para>
    /// <para>Defaults to 10 seconds.</para>
    /// </summary>
    public TimeSpan HttpTimeout { internal get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// <para>Defines that the client should attempt to reconnect indefinitely.</para>
    /// <para>This is typically a very bad idea to set to <c>true</c>, as it will swallow all connection errors.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public bool ReconnectIndefinitely { internal get; set; }
    
    /// <summary>
    /// <para>Sets the factory method used to create instances of WebSocket clients.</para>
    /// <para>Use <see cref="WebSocketClient.CreateNew(IWebProxy)"/> and equivalents on other implementations to switch out client implementations.</para>
    /// <para>Defaults to <see cref="WebSocketClient.CreateNew(IWebProxy)"/>.</para>
    /// </summary>
    public WebSocketClientFactoryDelegate WebSocketClientFactory {
        internal get => _webSocketClientFactory;
        set => _webSocketClientFactory = value ?? throw new InvalidOperationException("You need to supply a valid WebSocket client factory method.");
    }

    WebSocketClientFactoryDelegate _webSocketClientFactory = WebSocketClient.CreateNew;
    
    /// <summary>
    /// <para>Sets the logger implementation to use.</para>
    /// <para>To create your own logger, implement the <see cref="ILoggerFactory"/> instance.</para>
    /// <para>Defaults to built-in implementation.</para>
    /// </summary>
    public ILoggerFactory LoggerFactory { internal get; set; } = null!;

    /// <summary>
    /// Whether to log unknown events or not. Defaults to true.
    /// </summary>
    public bool LogUnknownEvents { internal get; set; } = true;
    
    /// <summary>
    /// Creates a new configuration with default values.
    /// </summary>
    public KuracordConfiguration() { }
    
    /// <summary>
    /// Creates a clone of another kuracord configuration.
    /// </summary>
    /// <param name="other">Client configuration to clone.</param>
    public KuracordConfiguration(KuracordConfiguration other) {
        Token = other.Token;
        TokenType = other.TokenType;
        MinimumLogLevel = other.MinimumLogLevel;
        LogTimestampFormat = other.LogTimestampFormat;
        LoggerFactory = other.LoggerFactory;
        LogUnknownEvents = other.LogUnknownEvents;
    }
}