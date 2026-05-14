using StateForge.Logging.Configuration;
using StateForge.Logging.Diagnostics;
using StateForge.Logging.Observers;

namespace StateForge.Logging;

/// <summary>Marker type for public API snapshot validation of the logging adapter package.</summary>
public sealed class LoggingPublicApi
{
    public Type OptionsType => typeof(StateMachineLoggingOptions);
    public Type RecordType => typeof(StateMachineLogRecord);
    public Type ObserverType => typeof(LoggingTransitionObserver<,>);
}
