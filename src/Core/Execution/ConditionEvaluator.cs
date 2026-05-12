using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Evaluates transition conditions in declaration order.</summary>
internal sealed class ConditionEvaluator<TState, TEvent>
{
    public async ValueTask<ConditionEvaluationResult<TState, TEvent>> EvaluateAsync(
        TransitionDefinition<TState, TEvent> transition,
        TransitionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        foreach (var condition in transition.Conditions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var allowed = await condition.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
            if (!allowed) return ConditionEvaluationResult<TState, TEvent>.Denied(condition);
        }

        return ConditionEvaluationResult<TState, TEvent>.Allowed;
    }
}

internal sealed class ConditionEvaluationResult<TState, TEvent>
{
    private ConditionEvaluationResult(bool isAllowed, ConditionDefinition<TState, TEvent>? deniedCondition)
    {
        IsAllowed = isAllowed;
        DeniedCondition = deniedCondition;
    }

    public static ConditionEvaluationResult<TState, TEvent> Allowed { get; } = new(true, null);
    public bool IsAllowed { get; }
    public ConditionDefinition<TState, TEvent>? DeniedCondition { get; }

    public static ConditionEvaluationResult<TState, TEvent> Denied(ConditionDefinition<TState, TEvent> condition)
    {
        return new ConditionEvaluationResult<TState, TEvent>(false, condition);
    }
}