using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

internal static class HierarchyInitialChildValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        if (!definition.HasHierarchy) return;

        foreach (var composite in definition.States.Where(s =>
                     definition.IsCompositeState(s.Value) && s.Hierarchy.HasInitialChild))
        {
            var seen = new HashSet<TState>();
            var current = composite.Value;
            while (definition.IsCompositeState(current))
            {
                if (!seen.Add(current))
                {
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, HierarchyValidationCodes.Cycle,
                        $"Initial-child chain for composite state '{composite.Value}' contains a cycle.",
                        $"state:{composite.Value}:initial-child"));
                    break;
                }

                if (!definition.TryGetInitialChild(current, out var child)) break;

                current = child;
            }
        }
    }
}