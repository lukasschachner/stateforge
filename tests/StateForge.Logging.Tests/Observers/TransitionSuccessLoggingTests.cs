using StateForge.Logging.Tests.TestSupport;
using StateForge.Core.Execution;
using StateForge.Logging.Diagnostics;
using StateForge.Logging.Observers;

namespace StateForge.Logging.Tests.Observers;

public sealed class TransitionSuccessLoggingTests
{
    [Fact]
    public async Task LogsSuccessfulTransitionWithStableEventId()
    {
        var logger = new ListLogger();
        var runtime = LoggingTestDomain.Definition().CreateRuntime(LogState.Start, ConcurrencyMode.Fast, new LoggingTransitionObserver<LogState, LogEvent>(logger));
        await runtime.ApplyAsync(LogEvent.Go);
        Assert.Contains(logger.Entries, e => e.EventId == StateMachineLoggingEvents.TransitionSucceeded);
    }
}
