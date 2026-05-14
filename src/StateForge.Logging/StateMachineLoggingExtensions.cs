using Microsoft.Extensions.Logging;
using StateForge.Core.Execution;
using StateForge.Logging.Configuration;
using StateForge.Logging.Observers;

namespace StateForge.Logging;

public static class StateMachineLoggingExtensions
{
    public static StateMachineLoggingOptions AddStateMachineLogging(Action<StateMachineLoggingOptions>? configure = null)
    {
        var options = new StateMachineLoggingOptions();
        configure?.Invoke(options);
        return options;
    }

    public static ITransitionObserver<TState, TEvent> CreateStateMachineLoggingObserver<TState, TEvent>(
        this ILogger logger,
        StateMachineLoggingOptions? options = null) =>
        new LoggingTransitionObserver<TState, TEvent>(logger, options);
}
