using KSharpPlus.Clients;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Logging; 

internal class DefaultLoggerProvider : ILoggerProvider { 
    LogLevel MinimumLevel { get; }
    string TimestampFormat { get; }

    bool _isDisposed;
     
    internal DefaultLoggerProvider(BaseKuracordClient client) : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat) { }

    internal DefaultLoggerProvider(KuracordWebhookClient client) : this(client._minimumLogLevel, client._logTimestampFormat) { }

    internal DefaultLoggerProvider(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz") {
        MinimumLevel = minLevel;
        TimestampFormat = timestampFormat;
    }
    
    public ILogger CreateLogger(string categoryName) {
        if (_isDisposed) throw new InvalidOperationException("This logger provider is already disposed.");

        return categoryName != typeof(BaseKuracordClient).FullName && categoryName != typeof(KuracordWebhookClient).FullName
            ? throw new ArgumentException($"This provider can only provide instances of loggers for {typeof(BaseKuracordClient).FullName} or {typeof(KuracordWebhookClient).FullName}.", nameof(categoryName))
            : new DefaultLogger(MinimumLevel, TimestampFormat);
    }

    public void Dispose() => _isDisposed = true;
}