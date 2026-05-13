using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;

namespace StateMachineLibrary.Core.Execution;

internal sealed class CompletionTransitionSelector<TState, TEvent>
{
    private readonly ConditionEvaluator<TState, TEvent> _conditionEvaluator;
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public CompletionTransitionSelector(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _conditionEvaluator = new ConditionEvaluator<TState, TEvent>(_definition);
    }

    public async ValueTask<CompletionTransitionDefinition<TState, TEvent>?> SelectAsync(
        TState activeState,
        TState completionScope,
        CancellationToken cancellationToken)
    {
        var result = await SelectWithDiagnosticsAsync(activeState, completionScope, cancellationToken).ConfigureAwait(false);
        return result.IsAmbiguous ? null : result.Selected;
    }

    public async ValueTask<CompletionSelectionResult<TState, TEvent>> SelectWithDiagnosticsAsync(
        TState activeState,
        TState completionScope,
        CancellationToken cancellationToken)
    {
        var candidates = _definition.CompletionTransitions
            .Where(t => EqualityComparer<TState>.Default.Equals(t.SourceState, completionScope))
            .OrderBy(t => t.DeclarationOrder)
            .ToArray();
        var enabled = new List<CompletionTransitionDefinition<TState, TEvent>>();
        var guardOutcomes = new List<GuardOutcomeDiagnostic>();

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var executable = candidate.ToExecutableTransition();
            var conditionSummaries = candidate.Conditions.Select(c => c.DisplayName).ToArray();
            if (candidate.Conditions.Count == 0)
            {
                enabled.Add(candidate);
                guardOutcomes.Add(new GuardOutcomeDiagnostic(true, conditionSummaries,
                    TransitionIdentityProvider.GetTransitionId(_definition, candidate)));
                continue;
            }

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
            guardOutcomes.Add(new GuardOutcomeDiagnostic(result.IsAllowed, conditionSummaries,
                TransitionIdentityProvider.GetTransitionId(_definition, candidate)));
            if (result.IsAllowed) enabled.Add(candidate);
        }

        if (enabled.Count == 0) return CompletionSelectionResult<TState, TEvent>.None(candidates, guardOutcomes);
        if (enabled.Count == 1) return CompletionSelectionResult<TState, TEvent>.FromSelected(enabled[0], candidates, guardOutcomes);

        var diagnostic = TransitionConflictDiagnosticBuilder.CompletionConflict(
            _definition,
            completionScope,
            enabled,
            $"Multiple completion transitions are enabled for scope '{completionScope}'.");
        return CompletionSelectionResult<TState, TEvent>.Ambiguous(enabled, candidates, guardOutcomes, diagnostic);
    }

    public bool HasCandidates(TState completionScope)
    {
        return _definition.CompletionTransitions.Any(t =>
            EqualityComparer<TState>.Default.Equals(t.SourceState, completionScope));
    }
}

internal sealed class CompletionSelectionResult<TState, TEvent>
{
    private CompletionSelectionResult(
        CompletionTransitionDefinition<TState, TEvent>? selected,
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> enabledCandidates,
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> candidates,
        IReadOnlyList<GuardOutcomeDiagnostic> guardOutcomes,
        TransitionConflictDiagnostic? conflictDiagnostic)
    {
        Selected = selected;
        EnabledCandidates = enabledCandidates;
        Candidates = candidates;
        GuardOutcomes = guardOutcomes;
        ConflictDiagnostic = conflictDiagnostic;
    }

    public CompletionTransitionDefinition<TState, TEvent>? Selected { get; }
    public IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> EnabledCandidates { get; }
    public IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> Candidates { get; }
    public IReadOnlyList<GuardOutcomeDiagnostic> GuardOutcomes { get; }
    public TransitionConflictDiagnostic? ConflictDiagnostic { get; }
    public bool IsAmbiguous => ConflictDiagnostic is not null;

    public static CompletionSelectionResult<TState, TEvent> None(
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> candidates,
        IReadOnlyList<GuardOutcomeDiagnostic> guardOutcomes)
    {
        return new CompletionSelectionResult<TState, TEvent>(null, [], candidates, guardOutcomes, null);
    }

    public static CompletionSelectionResult<TState, TEvent> FromSelected(
        CompletionTransitionDefinition<TState, TEvent> selected,
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> candidates,
        IReadOnlyList<GuardOutcomeDiagnostic> guardOutcomes)
    {
        return new CompletionSelectionResult<TState, TEvent>(selected, [selected], candidates, guardOutcomes, null);
    }

    public static CompletionSelectionResult<TState, TEvent> Ambiguous(
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> enabledCandidates,
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> candidates,
        IReadOnlyList<GuardOutcomeDiagnostic> guardOutcomes,
        TransitionConflictDiagnostic conflictDiagnostic)
    {
        return new CompletionSelectionResult<TState, TEvent>(null, enabledCandidates, candidates, guardOutcomes,
            conflictDiagnostic);
    }
}
