namespace StateForge.Core.Diagnostics;

/// <summary>Outcome of evaluating a guard during transition preview or denial diagnostics.</summary>
public enum TransitionPreviewGuardStatus
{
    /// <summary>The guard evaluated to <see langword="true" />.</summary>
    Passed,

    /// <summary>The guard evaluated to <see langword="false" />.</summary>
    Failed,

    /// <summary>The guard was not evaluated because an earlier guard short-circuited the decision.</summary>
    Skipped,

    /// <summary>Guard evaluation was cancelled.</summary>
    Cancelled,

    /// <summary>Guard evaluation threw an exception.</summary>
    Error
}
