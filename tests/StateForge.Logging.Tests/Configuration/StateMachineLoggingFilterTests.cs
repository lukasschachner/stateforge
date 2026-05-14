using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StateForge.Logging.Tests.Configuration;

public sealed class StateMachineLoggingFilterTests
{
    [Fact]
    public void AppliesMachineFilterBeforeEmission()
    {
        var options = new StateMachineLoggingOptions().FilterMachine("checkout");
        var record = new StateMachineLogRecord("other", StateMachineLogCategory.TransitionSuccess, new EventId(1), LogLevel.Information, null, null, null, null, [], "message");
        Assert.False(StateMachineLogFilter.ShouldLog(options, record));
    }
}
