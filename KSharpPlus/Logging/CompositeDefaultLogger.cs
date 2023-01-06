using KSharpPlus.Clients;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Logging; 

internal class CompositeDefaultLogger : ILogger<BaseKuracordClient> {
    IEnumerable<ILogger<BaseKuracordClient>> Loggers { get; }
    
    public CompositeDefaultLogger(IEnumerable<ILoggerProvider> providers) => 
        Loggers = providers.Select(x => x.CreateLogger(typeof(BaseKuracordClient).FullName))
        .OfType<ILogger<BaseKuracordClient>>()
        .ToList();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
        foreach (ILogger<BaseKuracordClient> logger in Loggers) logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
}