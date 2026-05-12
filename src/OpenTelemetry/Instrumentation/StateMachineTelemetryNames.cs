namespace StateMachineLibrary.OpenTelemetry;

/// <summary>Stable source, meter, instrument, activity, and attribute names emitted by the adapter.</summary>
public static class StateMachineTelemetryNames
{
    public const string ActivitySourceName = "StateMachineLibrary.OpenTelemetry";
    public const string MeterName = "StateMachineLibrary.OpenTelemetry";
    public const string TransitionActivityName = "state_machine.transition";
    public const string TransitionAttemptsInstrumentName = "state_machine.transition.attempts";
    public const string TransitionDurationInstrumentName = "state_machine.transition.duration";

    public const string SourceStateAttribute = "state_machine.source_state";
    public const string TargetStateAttribute = "state_machine.target_state";
    public const string ResultingStateAttribute = "state_machine.resulting_state";
    public const string EventTypeAttribute = "state_machine.event_type";
    public const string EventAttribute = "state_machine.event";
    public const string TransitionKindAttribute = "state_machine.transition_kind";
    public const string LifecyclePhaseAttribute = "state_machine.lifecycle_phase";
    public const string OutcomeAttribute = "state_machine.outcome";
    public const string CommittedAttribute = "state_machine.committed";
    public const string AttemptIdAttribute = "state_machine.attempt_id";
    public const string MachineNameAttribute = "state_machine.name";
    public const string ErrorTypeAttribute = "error.type";
    public const string ErrorMessageAttribute = "error.message";
}