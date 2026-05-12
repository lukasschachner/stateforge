using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

internal static class HierarchyStructureValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        if (!definition.HasHierarchy) return;

        var comparer = EqualityComparer<TState>.Default;

        foreach (var state in definition.States)
        {
            if (state.Hierarchy.HasParent)
            {
                var parent = state.Hierarchy.ParentState!;
                if (!definition.ContainsState(parent))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, HierarchyValidationCodes.ParentMissing,
                        $"State '{state.Value}' declares missing parent state '{parent}'.",
                        $"state:{state.Value}:parent", "Declare the parent state or remove the parent relationship."));
                else if (comparer.Equals(state.Value, parent))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, HierarchyValidationCodes.SelfParent,
                        $"State '{state.Value}' cannot be its own parent.", $"state:{state.Value}:parent"));
            }

            if (state.Hierarchy.HasInitialChild)
            {
                var initialChild = state.Hierarchy.InitialChildState!;
                if (!definition.ContainsState(initialChild))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        HierarchyValidationCodes.InitialChildMissing,
                        $"Composite state '{state.Value}' declares missing initial child '{initialChild}'.",
                        $"state:{state.Value}:initial-child"));
                else if (!definition.GetChildren(state.Value).Any(c => comparer.Equals(c.Value, initialChild)))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        HierarchyValidationCodes.InvalidInitialChild,
                        $"Initial child '{initialChild}' must be a direct child of composite state '{state.Value}'.",
                        $"state:{state.Value}:initial-child"));
            }
        }

        foreach (var composite in definition.States.Where(s => definition.IsCompositeState(s.Value)))
        {
            if (definition.IsParallelComposite(composite.Value)) continue;

            if (!composite.Hierarchy.HasInitialChild)
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    HierarchyValidationCodes.MissingInitialChild,
                    $"Composite state '{composite.Value}' must declare exactly one initial child.",
                    $"state:{composite.Value}:initial-child",
                    "Call InitialChild/WithInitialChild for this composite state."));
        }
    }
}