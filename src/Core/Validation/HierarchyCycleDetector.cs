using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

internal static class HierarchyCycleDetector
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        if (!definition.HasHierarchy) return;

        var comparer = EqualityComparer<TState>.Default;
        foreach (var state in definition.States)
        {
            var seen = new List<TState>();
            var current = state.Value;
            while (definition.TryGetParent(current, out var parent))
            {
                if (seen.Any(s => comparer.Equals(s, parent)) || comparer.Equals(state.Value, parent))
                {
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, HierarchyValidationCodes.Cycle,
                        $"Hierarchy cycle detected involving state '{state.Value}'.", $"state:{state.Value}:hierarchy",
                        "Remove or redirect one parent relationship to make the hierarchy acyclic."));
                    break;
                }

                seen.Add(parent);
                current = parent;
            }
        }
    }
}