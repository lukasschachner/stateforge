using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class HierarchyReachabilityValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        if (!definition.HasHierarchy || definition.States.Count == 0) return;

        var comparer = EqualityComparer<TState>.Default;
        foreach (var child in definition.States.Where(s => s.Hierarchy.HasParent))
        {
            var reachedByTransition = definition.Transitions.Any(t => comparer.Equals(t.TargetState, child.Value));
            var reachedAsInitial = definition.States.Any(s =>
                s.Hierarchy.HasInitialChild && comparer.Equals(s.Hierarchy.InitialChildState, child.Value));
            if (!reachedByTransition && !reachedAsInitial)
                findings.Add(new ValidationFinding(ValidationSeverity.Warning,
                    HierarchyValidationCodes.UnreachableNestedState,
                    $"Nested state '{child.Value}' appears unreachable because no transition or initial-child chain targets it.",
                    $"state:{child.Value}", "Target the state from a transition or make it an initial child."));
        }
    }
}