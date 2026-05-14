using StateForge.Core.Definitions;

namespace StateForge.Core.Execution;

/// <summary>Context passed to conditions and behaviors for one transition attempt.</summary>
public sealed class TransitionContext<TState, TEvent>
{
    public TransitionContext(
        StateMachineDefinition<TState, TEvent> definition,
        TState sourceState,
        TEvent @event,
        CancellationToken cancellationToken,
        TransitionDefinition<TState, TEvent>? transition = null,
        TState? targetState = default,
        MetadataCollection? metadata = null,
        TransitionTriggerKind? triggerKind = null)
    {
        Definition = definition;
        SourceState = sourceState;
        Event = @event;
        CancellationToken = cancellationToken;
        Transition = transition;
        TargetState = targetState;
        Metadata = metadata ?? MetadataCollection.Empty;
        TriggerKind = triggerKind ?? transition?.TriggerKind ?? TransitionTriggerKind.Event;
    }

    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public TState SourceState { get; }
    public TEvent Event { get; }
    public CancellationToken CancellationToken { get; }
    public TransitionDefinition<TState, TEvent>? Transition { get; }
    public TState? TargetState { get; }
    public MetadataCollection Metadata { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public bool IsCompletionTrigger => TriggerKind == TransitionTriggerKind.Completion;
}