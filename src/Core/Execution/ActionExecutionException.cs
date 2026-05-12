using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Wraps a lifecycle action failure with non-executable summary context.</summary>
public sealed class ActionExecutionException : Exception
{
    public ActionExecutionException(ActionSummary action, object? owner, Exception innerException)
        : base($"{action.DisplayName} failed during {action.Phase} action execution: {innerException.Message}",
            innerException)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Owner = owner;
    }

    public ActionSummary Action { get; }
    public object? Owner { get; }
    public TransitionLifecyclePhase Phase => Action.Phase;
}