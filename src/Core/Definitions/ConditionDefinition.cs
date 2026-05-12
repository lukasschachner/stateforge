using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Async guard evaluated before state-changing behavior.</summary>
public sealed class ConditionDefinition<TState, TEvent>
{
    public ConditionDefinition(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask<bool>> evaluateAsync,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        EvaluateAsync = evaluateAsync ?? throw new ArgumentNullException(nameof(evaluateAsync));
        DisplayName = displayName ?? "Condition";
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public string DisplayName { get; }
    public MetadataCollection Metadata { get; }
    public Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask<bool>> EvaluateAsync { get; }
}