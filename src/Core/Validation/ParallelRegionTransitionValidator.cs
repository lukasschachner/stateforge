using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

internal static class ParallelRegionTransitionValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var transition in definition.Transitions)
            if (definition.TryGetCommonParallelOwner(transition.SourceState, transition.TargetState, out var owner,
                    out var sourceRegion, out var targetRegion)
                && !StringComparer.Ordinal.Equals(sourceRegion, targetRegion))
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    ParallelValidationCodes.IllegalBoundaryTransition,
                    $"Transition from '{transition.SourceState}' to '{transition.TargetState}' crosses sibling parallel regions.",
                    transition.ToString(),
                    "Route through an explicit parent-level transition instead of jumping between sibling regions.",
                    owner, sourceRegion, null, transition.SourceState, transition.TargetState,
                    transition.Event.DisplayName, transition.ToString()));
    }
}