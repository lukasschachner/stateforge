namespace StateForge.Core.Execution;

/// <summary>Identifies a transition lifecycle notification delivered to an observer.</summary>
public enum TransitionObservationKind
{
    Started,
    ConditionDenied,
    BehaviorFailed,
    Committed,
    Completed,
    Cancelled,
    ValidationFailure,
    NotPermitted,
    Outcome
}