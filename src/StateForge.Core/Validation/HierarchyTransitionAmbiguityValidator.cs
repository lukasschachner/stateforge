using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class HierarchyTransitionAmbiguityValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        List<ValidationFinding> findings)
    {
        var comparer = EqualityComparer<TState>.Default;
        var groups = definition.Transitions
            .GroupBy(t => new TransitionKey<TState>(t.SourceState, t.Event.Identity),
                new TransitionKeyComparer<TState>(comparer))
            .Where(g => g.Count() > 1);

        foreach (var group in groups)
            findings.Add(new ValidationFinding(ValidationSeverity.Error,
                definition.HasHierarchy ? HierarchyValidationCodes.AmbiguousTransition : "TRANSITION003",
                $"Duplicate or ambiguous transitions exist for state '{group.Key.SourceState}' and event '{group.Key.EventIdentity}'.",
                $"transition:{group.Key.SourceState}:{group.Key.EventIdentity}",
                "Keep exactly one transition per source state and event matcher."));
    }

    private sealed record TransitionKey<TState>(TState SourceState, string EventIdentity);

    private sealed class TransitionKeyComparer<TState>(IEqualityComparer<TState> stateComparer)
        : IEqualityComparer<TransitionKey<TState>>
    {
        public bool Equals(TransitionKey<TState>? x, TransitionKey<TState>? y)
        {
            return x is not null && y is not null && stateComparer.Equals(x.SourceState, y.SourceState) &&
                   StringComparer.Ordinal.Equals(x.EventIdentity, y.EventIdentity);
        }

        public int GetHashCode(TransitionKey<TState> obj)
        {
            return HashCode.Combine(obj.SourceState is null ? 0 : stateComparer.GetHashCode(obj.SourceState),
                StringComparer.Ordinal.GetHashCode(obj.EventIdentity));
        }
    }
}