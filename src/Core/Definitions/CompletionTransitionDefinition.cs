namespace StateMachineLibrary.Core.Definitions;

/// <summary>Declares a transition selected when a completion-capable scope completes.</summary>
public sealed class CompletionTransitionDefinition<TState, TEvent>
{
    private static readonly EventDefinition<TEvent> CompletionEvent = EventDefinition<TEvent>.ForCompletion();

    public CompletionTransitionDefinition(
        TState sourceState,
        TState targetState,
        TransitionKind kind,
        IEnumerable<ConditionDefinition<TState, TEvent>>? conditions = null,
        IEnumerable<TransitionBehaviorDefinition<TState, TEvent>>? behaviors = null,
        MetadataCollection? metadata = null,
        IEnumerable<TransitionActionDefinition<TState, TEvent>>? transitionActions = null,
        int declarationOrder = 0)
    {
        SourceState = sourceState;
        TargetState = targetState;
        Kind = kind;
        Conditions = (conditions ?? []).ToArray();
        Behaviors = (behaviors ?? []).ToArray();
        Metadata = metadata ?? MetadataCollection.Empty;
        TransitionActions = (transitionActions ?? []).ToArray();
        DeclarationOrder = declarationOrder;
    }

    public TState SourceState { get; }
    public TState TargetState { get; }
    public TransitionKind Kind { get; }
    public IReadOnlyList<ConditionDefinition<TState, TEvent>> Conditions { get; }
    public IReadOnlyList<TransitionBehaviorDefinition<TState, TEvent>> Behaviors { get; }
    public IReadOnlyList<TransitionActionDefinition<TState, TEvent>> TransitionActions { get; }
    public MetadataCollection Metadata { get; }
    public int DeclarationOrder { get; }
    public TransitionTriggerKind TriggerKind => TransitionTriggerKind.Completion;

    internal TransitionDefinition<TState, TEvent> ToExecutableTransition(bool includeConditions = true)
    {
        return new TransitionDefinition<TState, TEvent>(
            SourceState,
            CompletionEvent,
            TargetState,
            Kind,
            includeConditions ? Conditions : [],
            Behaviors,
            Metadata,
            TransitionActions,
            TriggerKind,
            this);
    }

    public override string ToString()
    {
        return $"{SourceState} --completion--> {TargetState} ({Kind})";
    }
}
