using StateForge.Logging.Tests.TestSupport;
using StateForge.Core.Validation;
using StateForge.Logging.Diagnostics;
using StateForge.Logging.Observers;

namespace StateForge.Logging.Tests.Observers;

public sealed class ValidationFindingLoggingTests
{
    [Fact]
    public void LogsValidationFindingWithStableEventId()
    {
        var logger = new ListLogger();
        new ValidationFindingLogger(logger).Log("machine", new ValidationFinding(ValidationSeverity.Error, "X001", "problem", "target"));
        Assert.Contains(logger.Entries, e => e.EventId == StateMachineLoggingEvents.ValidationFinding);
    }
}
