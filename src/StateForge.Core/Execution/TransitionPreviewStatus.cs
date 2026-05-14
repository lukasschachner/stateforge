namespace StateForge.Core.Execution;

/// <summary>High-level result status for a side-effect-free transition preview.</summary>
public enum TransitionPreviewStatus
{
    /// <summary>A transition would be permitted for the supplied event and active shape.</summary>
    Permitted,

    /// <summary>No transition would be permitted for the supplied event and active shape.</summary>
    Denied,

    /// <summary>The machine definition contains validation errors.</summary>
    ValidationFailure,

    /// <summary>The supplied or derived active shape is invalid for the machine definition.</summary>
    InvalidActiveShape,

    /// <summary>The preview was cancelled before a decision completed.</summary>
    Cancelled,

    /// <summary>A guard failed with an exception before a decision completed.</summary>
    GuardEvaluationFailed
}

/// <summary>Indicates how completely preview could predict the direct post-transition active shape.</summary>
public enum TransitionPredictionCompleteness
{
    /// <summary>The direct target active shape was predicted completely.</summary>
    Complete,

    /// <summary>Some direct target information is known, but the full shape is not authoritative.</summary>
    Partial,

    /// <summary>The direct target active shape could not be predicted.</summary>
    Unknown
}
