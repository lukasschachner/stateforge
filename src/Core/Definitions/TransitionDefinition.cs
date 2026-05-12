namespace StateMachineLibrary.Core.Definitions;

/// <summary>Declares a transition rule from a source state and event matcher.</summary>
public sealed class TransitionDefinition<TState, TEvent>
{
    public TransitionDefinition(
        TState sourceState,
        EventDefinition<TEvent> eventDefinition,
        TState targetState,
        TransitionKind kind,
        IEnumerable<ConditionDefinition<TState, TEvent>>? conditions = null,
        IEnumerable<TransitionBehaviorDefinition<TState, TEvent>>? behaviors = null,
        MetadataCollection? metadata = null,
        IEnumerable<TransitionActionDefinition<TState, TEvent>>? transitionActions = null,
        TransitionTriggerKind triggerKind = TransitionTriggerKind.Event,
        CompletionTransitionDefinition<TState, TEvent>? completionTransition = null)
    {
        SourceState = sourceState;
        Event = eventDefinition ?? throw new ArgumentNullException(nameof(eventDefinition));
        TargetState = targetState;
        Kind = kind;
        Conditions = (conditions ?? []).ToArray();
        Behaviors = (behaviors ?? []).ToArray();
        Metadata = metadata ?? MetadataCollection.Empty;
        TransitionActions = (transitionActions ?? []).ToArray();
        TriggerKind = triggerKind;
        CompletionTransition = completionTransition;
    }

    public TState SourceState { get; }
    public EventDefinition<TEvent> Event { get; }
    public TState TargetState { get; }
    public TransitionKind Kind { get; }
    public IReadOnlyList<ConditionDefinition<TState, TEvent>> Conditions { get; }
    public IReadOnlyList<TransitionBehaviorDefinition<TState, TEvent>> Behaviors { get; }
    public IReadOnlyList<TransitionActionDefinition<TState, TEvent>> TransitionActions { get; }
    public MetadataCollection Metadata { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public bool IsCompletionTriggered => TriggerKind == TransitionTriggerKind.Completion;
    public CompletionTransitionDefinition<TState, TEvent>? CompletionTransition { get; }

    public override string ToString()
    {
        return IsCompletionTriggered
            ? $"{SourceState} --completion--> {TargetState} ({Kind})"
            : $"{SourceState} --{Event.DisplayName}--> {TargetState} ({Kind})";
    }
}