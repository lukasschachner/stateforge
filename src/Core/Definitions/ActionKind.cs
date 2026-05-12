namespace StateMachineLibrary.Core.Definitions;

/// <summary>Identifies the lifecycle action category configured on a state machine definition.</summary>
public enum ActionKind
{
    /// <summary>An action that runs before committing entry into a state.</summary>
    Entry,

    /// <summary>An action that runs before leaving a state.</summary>
    Exit,

    /// <summary>An action that runs between exit and entry actions for a matched transition.</summary>
    Transition
}