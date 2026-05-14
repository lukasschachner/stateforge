using Microsoft.Extensions.Logging;
using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;

namespace StateForge.Logging.Observers;

public static class StateMachineLoggingScope
{
    public static IDisposable? Begin(ILogger logger, StateMachineLoggingOptions options, StateMachineLogRecord record)
    {
        if (!options.EnableScopes) return null;
        var scope = new Dictionary<string, object?>
        {
            ["MachineIdentity"] = record.MachineIdentity,
            ["EventIdentity"] = record.EventIdentity,
            ["Outcome"] = record.Outcome
        };
        return logger.BeginScope(scope);
    }
}
