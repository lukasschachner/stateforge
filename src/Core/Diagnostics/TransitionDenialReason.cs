namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Stable machine-readable categories for transition preview and transition attempt denials.</summary>
public enum TransitionDenialReason
{
    /// <summary>The current state or active shape references a state that is not declared by the definition.</summary>
    UnknownCurrentState,

    /// <summary>The supplied event does not match any declared event descriptor.</summary>
    UnknownEvent,

    /// <summary>The event is declared, but no transition candidate matches the current source context.</summary>
    NoMatchingEvent,

    /// <summary>The active state is terminal and no transition can be selected.</summary>
    TerminalState,

    /// <summary>One or more guards denied the selected transition.</summary>
    FailedGuards,

    /// <summary>Guard evaluation was cancelled before a permit/deny decision completed.</summary>
    GuardEvaluationCancelled,

    /// <summary>Guard evaluation failed before a permit/deny decision completed.</summary>
    GuardEvaluationFailed,

    /// <summary>The supplied or derived active state shape is structurally invalid for the definition.</summary>
    InvalidActiveShape,

    /// <summary>Transition candidates are ambiguous according to validation or runtime conflict rules.</summary>
    AmbiguousTransitions,

    /// <summary>Definition validation errors prevent an authoritative transition decision.</summary>
    ValidationConflicts,

    /// <summary>The transition can be selected, but the target active shape cannot be fully predicted.</summary>
    UnsupportedPrediction
}
