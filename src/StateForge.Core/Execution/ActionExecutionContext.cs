using StateForge.Core.Definitions;

namespace StateForge.Core.Execution;

/// <summary>Runtime context supplied to a state or transition lifecycle action.</summary>
public sealed class ActionExecutionContext<TState, TEvent>
{
    public ActionExecutionContext(
        StateMachineDefinition<TState, TEvent> definition,
        TState sourceState,
        TState targetState,
        TEvent @event,
        TransitionDefinition<TState, TEvent> transition,
        TransitionLifecyclePhase phase,
        CancellationToken cancellationToken,
        MetadataCollection? metadata = null,
        string? regionId = null,
        string? regionName = null,
        bool isRegionalCompletion = false,
        TransitionTriggerKind? triggerKind = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        SourceState = sourceState;
        TargetState = targetState;
        Event = @event;
        Transition = transition ?? throw new ArgumentNullException(nameof(transition));
        Phase = phase;
        CancellationToken = cancellationToken;
        Metadata = metadata ?? MetadataCollection.Empty;
        RegionId = regionId;
        RegionName = regionName;
        IsRegionalCompletion = isRegionalCompletion;
        TriggerKind = triggerKind ?? transition.TriggerKind;
    }

    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public TState SourceState { get; }
    public TState TargetState { get; }
    public TEvent Event { get; }
    public TransitionDefinition<TState, TEvent> Transition { get; }
    public TransitionLifecyclePhase Phase { get; }
    public CancellationToken CancellationToken { get; }
    public MetadataCollection Metadata { get; }
    public string? RegionId { get; }
    public string? RegionName { get; }
    public bool IsRegionalCompletion { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public bool IsCompletionTrigger => TriggerKind == TransitionTriggerKind.Completion;
}