using KSharpPlus.Clients;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Logging; 

internal class DefaultLoggerFactory : ILoggerFactory {
    List<ILoggerProvider> Providers { get; } = new();
    bool _isDisposed;

    public void AddProvider(ILoggerProvider provider) => Providers.Add(provider);

    public ILogger CreateLogger(string categoryName) {
       if (_isDisposed) throw new InvalidOperationException("This logger factory is already disposed.");

       return categoryName != typeof(BaseKuracordClient).FullName && categoryName != typeof(KuracordWebhookClient).FullName
           ? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseKuracordClient).FullName} or {typeof(KuracordWebhookClient).FullName}.", nameof(categoryName))
           : new CompositeDefaultLogger(Providers);
    }

    public void Dispose() {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (ILoggerProvider provider in Providers) provider.Dispose();

        Providers.Clear();
    }
}