using Microsoft.Extensions.Logging;
using StateForge.Core.Definitions;

namespace StateForge.Logging.Tests.TestSupport;

public enum LogState { Start, Done }
public enum LogEvent { Go }

public static class LoggingTestDomain
{
    public static StateMachineDefinition<LogState, LogEvent> Definition() => StateMachineDefinition<LogState, LogEvent>.Create(b =>
    {
        b.State(LogState.Start).On(LogEvent.Go).GoTo(LogState.Done);
        b.State(LogState.Done);
    });
}

public sealed record CapturedLog(LogLevel Level, EventId EventId, string Message, IReadOnlyList<KeyValuePair<string, object?>> State);

public sealed class ListLogger : ILogger
{
    public List<CapturedLog> Entries { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var pairs = state as IReadOnlyList<KeyValuePair<string, object?>> ?? [];
        Entries.Add(new CapturedLog(logLevel, eventId, formatter(state, exception), pairs));
    }
    private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
}
