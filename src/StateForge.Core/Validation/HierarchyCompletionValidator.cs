using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class HierarchyCompletionValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        // Completion transitions reuse ordinary event transitions in the current public model.
        // Ambiguity is therefore covered by HierarchyTransitionAmbiguityValidator.
    }
}