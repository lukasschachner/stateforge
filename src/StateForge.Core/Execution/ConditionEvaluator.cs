using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;

namespace StateForge.Core.Execution;

/// <summary>Evaluates transition conditions in declaration order.</summary>
internal sealed class ConditionEvaluator<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public ConditionEvaluator(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

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

    public async ValueTask<GuardPreviewResult> EvaluateWithDiagnosticsAsync(
        TransitionDefinition<TState, TEvent> transition,
        TransitionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<TransitionPreviewGuardDiagnostic>();
        var transitionId = GetTransitionId(transition);

        for (var index = 0; index < transition.Conditions.Count; index++)
        {
            var condition = transition.Conditions[index];
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var allowed = await condition.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
                diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, index, condition.DisplayName,
                    allowed ? TransitionPreviewGuardStatus.Passed : TransitionPreviewGuardStatus.Failed));
                if (!allowed)
                {
                    for (var skipped = index + 1; skipped < transition.Conditions.Count; skipped++)
                        diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, skipped,
                            transition.Conditions[skipped].DisplayName, TransitionPreviewGuardStatus.Skipped));

                    return GuardPreviewResult.Denied(diagnostics);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, index, condition.DisplayName,
                    TransitionPreviewGuardStatus.Cancelled, "Guard evaluation was cancelled."));
                for (var skipped = index + 1; skipped < transition.Conditions.Count; skipped++)
                    diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, skipped,
                        transition.Conditions[skipped].DisplayName, TransitionPreviewGuardStatus.Skipped));

                return GuardPreviewResult.Cancelled(diagnostics);
            }
            catch (Exception ex)
            {
                diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, index, condition.DisplayName,
                    TransitionPreviewGuardStatus.Error, ex.Message));
                for (var skipped = index + 1; skipped < transition.Conditions.Count; skipped++)
                    diagnostics.Add(new TransitionPreviewGuardDiagnostic(transitionId, skipped,
                        transition.Conditions[skipped].DisplayName, TransitionPreviewGuardStatus.Skipped));

                return GuardPreviewResult.Error(diagnostics);
            }
        }

        return GuardPreviewResult.Allowed(diagnostics);
    }

    private string GetTransitionId(TransitionDefinition<TState, TEvent> transition)
    {
        return TransitionIdentityProvider.GetTransitionId(_definition, transition) ?? transition.ToString();
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