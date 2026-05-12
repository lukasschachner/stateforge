using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class CompletionTransitionSelector<TState, TEvent>
{
    private readonly ConditionEvaluator<TState, TEvent> _conditionEvaluator = new();
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public CompletionTransitionSelector(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public async ValueTask<CompletionTransitionDefinition<TState, TEvent>?> SelectAsync(
        TState activeState,
        TState completionScope,
        CancellationToken cancellationToken)
    {
        foreach (var candidate in _definition.CompletionTransitions
                     .Where(t => EqualityComparer<TState>.Default.Equals(t.SourceState, completionScope))
                     .OrderBy(t => t.DeclarationOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (candidate.Conditions.Count == 0) return candidate;

            var executable = candidate.ToExecutableTransition();
            var context = new TransitionContext<TState, TEvent>(
                _definition,
                completionScope,
                default!,
                cancellationToken,
                executable,
                candidate.TargetState,
                candidate.Metadata,
                TransitionTriggerKind.Completion);
            var result = await _conditionEvaluator.EvaluateAsync(executable, context, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsAllowed) return candidate;
        }

        return null;
    }

    public bool HasCandidates(TState completionScope)
    {
        return _definition.CompletionTransitions.Any(t =>
            EqualityComparer<TState>.Default.Equals(t.SourceState, completionScope));
    }
}
