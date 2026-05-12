using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Non-executable, deterministic description of a configured lifecycle action.</summary>
public sealed class ActionSummary
{
    public ActionSummary(ActionKind kind, TransitionLifecyclePhase phase, string displayName, int order,
        MetadataCollection? metadata = null)
    {
        Kind = kind;
        Phase = phase;
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("Action display name must be non-empty.", nameof(displayName))
            : displayName;
        Order = order < 0
            ? throw new ArgumentOutOfRangeException(nameof(order), "Action order must be non-negative.")
            : order;
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public ActionKind Kind { get; }
    public TransitionLifecyclePhase Phase { get; }
    public string DisplayName { get; }
    public int Order { get; }
    public MetadataCollection Metadata { get; }

    public static ActionSummary From<TState>(StateActionDefinition<TState> action)
    {
        return new ActionSummary(action.Kind, action.Phase, action.DisplayName, action.Order, action.Metadata);
    }

    public static ActionSummary From<TState, TEvent>(TransitionActionDefinition<TState, TEvent> action)
    {
        return new ActionSummary(ActionKind.Transition, TransitionLifecyclePhase.Transition, action.DisplayName,
            action.Order,
            action.Metadata);
    }
}