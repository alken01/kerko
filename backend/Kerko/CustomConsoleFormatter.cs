using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Kerko;

public class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base("customFormatter") { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        // Belgian time (Central European Time)
        var belgianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        var belgianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, belgianTimeZone);
        var timestamp = belgianTime.ToString("yyyy-MM-dd HH:mm:ss");

        var logLevel = logEntry.LogLevel.ToString().ToLower();
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        // Simple format: just timestamp, log level, and message (no category name)
        textWriter.WriteLine($"{timestamp} {logLevel}: {message}");

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }
}
