using StateForge.Core.Execution;

namespace StateForge.Core.Definitions;

/// <summary>Async behavior associated with one transition lifecycle phase.</summary>
public sealed class TransitionBehaviorDefinition<TState, TEvent>
{
    public TransitionBehaviorDefinition(
        TransitionLifecyclePhase phase,
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> executeAsync,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        Phase = phase;
        ExecuteAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        DisplayName = displayName ?? phase.ToString();
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public TransitionLifecyclePhase Phase { get; }
    public string DisplayName { get; }
    public MetadataCollection Metadata { get; }
    public Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> ExecuteAsync { get; }
}