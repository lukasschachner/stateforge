using Microsoft.Extensions.Logging;
using StateForge.Core.Validation;
using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Observers;

public sealed class ValidationFindingLogger
{
    private readonly ILogger _logger;
    private readonly StateMachineLoggingOptions _options;

    public ValidationFindingLogger(ILogger logger, StateMachineLoggingOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new StateMachineLoggingOptions();
    }

    public void Log(string? machineIdentity, ValidationFinding finding)
    {
        var record = new StateMachineLogRecord(machineIdentity, StateMachineLogCategory.ValidationFinding,
            _options.ValidationFindingEventId,
            finding.Severity == ValidationSeverity.Error ? LogLevel.Error : LogLevel.Warning,
            null, null, null, finding.Severity.ToString(), SafeDiagnosticFormatter.DiagnosticCodes(finding),
            SafeDiagnosticFormatter.SafeValue(finding.Message, _options));
        if (!StateMachineLogFilter.ShouldLog(_options, record)) return;
        using var scope = StateMachineLoggingScope.Begin(_logger, _options, record);
        _logger.Log(record.Level, record.EventId, "{Message} Machine={MachineIdentity} Codes={DiagnosticCodes}", record.Message, record.MachineIdentity, string.Join(',', record.DiagnosticCodes));
    }
}
