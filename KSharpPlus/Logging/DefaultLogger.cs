﻿using KSharpPlus.Clients;
using Microsoft.Extensions.Logging;

namespace KSharpPlus.Logging;

public class DefaultLogger : ILogger<BaseKuracordClient> {
    static readonly object Lock = new();
    LogLevel MinimumLevel { get; }
    string TimestampFormat { get; }

    internal DefaultLogger(BaseKuracordClient client) : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat) { }

    internal DefaultLogger(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz") {
        MinimumLevel = minLevel;
        TimestampFormat = timestampFormat;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) return;

        lock (Lock) {
            string? eventName = eventId.Name;
            eventName = eventName?.Length > 12 ? eventName[..12] : eventName;
            Console.Write($"[{DateTimeOffset.Now.ToString(TimestampFormat)}] [{eventId.Id,-4}/{eventName,-12}] ");

            switch (logLevel) {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogLevel.Critical:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
            }

            Console.Write(logLevel switch {
                LogLevel.Trace => "[Trace] ",
                LogLevel.Debug => "[Debug] ",
                LogLevel.Information => "[Info] ",
                LogLevel.Warning => "[Warn] ",
                LogLevel.Error => "[Error] ",
                LogLevel.Critical => "[Crit]",
                LogLevel.None => "[None] ",
                _ => "[?????] "
            });

            Console.ResetColor();

            //The foreground color is off.
            if (logLevel == LogLevel.Critical) Console.Write(" ");

            string message = formatter(state, exception);
            Console.WriteLine(message);
            if (exception != null) Console.WriteLine(exception);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinimumLevel;

    public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
}