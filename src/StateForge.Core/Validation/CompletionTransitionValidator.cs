using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class CompletionTransitionValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        var comparer = EqualityComparer<TState>.Default;

        foreach (var transition in definition.CompletionTransitions)
        {
            var sourceExists = definition.ContainsState(transition.SourceState);
            var targetExists = definition.ContainsState(transition.TargetState);

            if (!sourceExists)
            {
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    CompletionTransitionValidationCodes.InvalidSource,
                    $"Completion transition source state '{transition.SourceState}' is not declared.",
                    transition.ToString(),
                    "Declare the source state as an ordinary composite or parallel composite."));
                continue;
            }

            if (!targetExists)
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    CompletionTransitionValidationCodes.InvalidTarget,
                    $"Completion transition target state '{transition.TargetState}' is not declared.",
                    transition.ToString(),
                    "Declare the target state or choose a declared target."));

            var source = definition.FindState(transition.SourceState);
            var isParallel = definition.IsParallelComposite(transition.SourceState);
            var isOrdinaryComposite = definition.GetChildren(transition.SourceState).Count > 0 && !isParallel;
            if (source?.IsTerminal == true || (!isOrdinaryComposite && !isParallel))
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    CompletionTransitionValidationCodes.InvalidSource,
                    $"Completion transition source '{transition.SourceState}' is not a completion-capable composite scope.",
                    transition.ToString(),
                    "Declare completion transitions from ordinary composite states or parallel composite states."));

            if (isParallel && definition.GetParallelRegions(transition.SourceState).Count == 0)
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    CompletionTransitionValidationCodes.InvalidParallelScope,
                    $"Parallel completion source '{transition.SourceState}' does not declare any regions.",
                    transition.ToString(),
                    "Declare one or more valid regions before adding a parallel completion transition."));

            if (targetExists && definition.TryGetCommonParallelOwner(transition.SourceState, transition.TargetState,
                    out var owner, out var sourceRegion, out var targetRegion) &&
                !StringComparer.Ordinal.Equals(sourceRegion, targetRegion))
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    CompletionTransitionValidationCodes.InvalidRegionBoundary,
                    $"Completion transition from '{transition.SourceState}' to '{transition.TargetState}' crosses sibling parallel regions.",
                    transition.ToString(),
                    "Route through an explicit parent-level target instead of jumping between sibling regions.",
                    owner, sourceRegion, null, transition.SourceState, transition.TargetState,
                    "completion", transition.ToString()));
        }

        foreach (var group in definition.CompletionTransitions
                     .Where(t => t.Conditions.Count == 0)
                     .GroupBy(t => t.SourceState, comparer)
                     .Where(g => g.Count() > 1))
        {
            var targets = string.Join(", ", group.Select(t => $"'{t.TargetState}' at #{t.DeclarationOrder}"));
            findings.Add(new ValidationFinding(ValidationSeverity.Error,
                CompletionTransitionValidationCodes.AmbiguousUnguarded,
                $"Multiple unguarded completion transitions are declared for scope '{group.Key}': {targets}.",
                $"completion:{group.Key}",
                "Keep at most one unguarded completion transition per completion scope."));
        }
    }
}
