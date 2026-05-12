namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Describes the role a participant plays in a transition conflict diagnostic.</summary>
public enum TransitionConflictParticipantRole
{
    ParentTransition,
    RegionalTransition,
    CompetingTransition,
    CompletionTransition,
    SourceState,
    TargetState,
    Composite,
    Region,
    Event,
    InvalidShapeMember
}
