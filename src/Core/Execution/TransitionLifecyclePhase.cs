namespace StateMachineLibrary.Core.Execution;

/// <summary>Documented transition lifecycle phases.</summary>
public enum TransitionLifecyclePhase
{
    None,
    Matching,
    Condition,
    Exit,
    Transition,
    Commit,
    Entry
}