using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class HierarchyHistoryValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        foreach (var state in definition.States.Where(s => s.HasHistory))
        {
            if (definition.IsParallelComposite(state.Value)) continue;

            if (!definition.IsCompositeState(state.Value))
            {
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    HierarchyValidationCodes.HistoryOnNonComposite,
                    $"History is configured on non-composite state '{state.Value}'.",
                    $"state:{state.Value}:history",
                    "Enable history only on states that own child states."));
                continue;
            }

            if (!definition.TryGetEffectiveHistoryFallback(state.Value, out var fallback))
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    HierarchyValidationCodes.MissingHistoryFallback,
                    $"History-enabled composite state '{state.Value}' has no deterministic fallback child.",
                    $"state:{state.Value}:history:fallback"));
            else if (!definition.GetChildren(state.Value)
                         .Any(c => EqualityComparer<TState>.Default.Equals(c.Value, fallback)))
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    HierarchyValidationCodes.InvalidHistoryFallback,
                    $"History fallback state '{fallback}' is not a direct child of composite state '{state.Value}'.",
                    $"state:{state.Value}:history:fallback",
                    "Choose a direct child as the history fallback target."));

            if (state.HistoryMode == HistoryMode.Deep && DeepHistoryHasAmbiguousNestedChoice(definition, state.Value))
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    HierarchyValidationCodes.AmbiguousDeepHistory,
                    $"Deep history for composite state '{state.Value}' is ambiguous and is not supported by this deterministic definition.",
                    $"state:{state.Value}:history:deep",
                    "Use shallow history or simplify nested history restoration."));
        }
    }

    private static bool DeepHistoryHasAmbiguousNestedChoice<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition, TState composite)
    {
        foreach (var child in definition.GetChildren(composite))
        {
            if (!definition.IsCompositeState(child.Value)) continue;

            if (definition.GetChildren(child.Value).Count > 1) return true;

            if (DeepHistoryHasAmbiguousNestedChoice(definition, child.Value)) return true;
        }

        return false;
    }
}