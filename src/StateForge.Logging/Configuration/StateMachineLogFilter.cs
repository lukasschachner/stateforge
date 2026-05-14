using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Configuration;

public static class StateMachineLogFilter
{
    public static bool ShouldLog(StateMachineLoggingOptions options, StateMachineLogRecord record)
    {
        if (record.MachineIdentity is not null && options.IncludedMachineNames.Count > 0 && !options.IncludedMachineNames.Contains(record.MachineIdentity))
            return false;
        if (record.Category == StateMachineLogCategory.TransitionSuccess && !options.IncludeTransitions) return false;
        if (record.Category == StateMachineLogCategory.TransitionDenied && !options.IncludeDenials) return false;
        if (record.Category == StateMachineLogCategory.TransitionFailure && !options.IncludeFailures) return false;
        if (record.Category == StateMachineLogCategory.ValidationFinding && !options.IncludeValidationFindings) return false;
        return options.Filter?.Invoke(record) ?? true;
    }
}
