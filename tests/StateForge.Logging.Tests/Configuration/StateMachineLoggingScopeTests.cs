using StateForge.Logging.Tests.TestSupport;
using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;
using StateForge.Logging.Observers;
using Microsoft.Extensions.Logging;

namespace StateForge.Logging.Tests.Configuration;

public sealed class StateMachineLoggingScopeTests
{
    [Fact]
    public void CreatesScopeWhenEnabled()
    {
        var scope = StateMachineLoggingScope.Begin(new ListLogger(), new StateMachineLoggingOptions(), new StateMachineLogRecord("m", StateMachineLogCategory.TransitionSuccess, new EventId(1), LogLevel.Information, null, null, "e", "ok", [], "msg"));
        Assert.NotNull(scope);
    }
}
