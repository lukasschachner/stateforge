namespace StateForge.SourceGenerators.Declarations;

public static class DeclarationOrdering
{
    public static IOrderedEnumerable<DeclaredState> States(IEnumerable<DeclaredState> states)
    {
        return states.OrderBy(s => s.IdentityKey, StringComparer.Ordinal)
            .ThenBy(s => s.GeneratedIdentifier, StringComparer.Ordinal);
    }

    public static IOrderedEnumerable<DeclaredEvent> Events(IEnumerable<DeclaredEvent> events)
    {
        return events.OrderBy(e => e.IdentityKey, StringComparer.Ordinal)
            .ThenBy(e => e.GeneratedIdentifier, StringComparer.Ordinal);
    }

    public static IOrderedEnumerable<DeclaredTransition> Transitions(IEnumerable<DeclaredTransition> transitions)
    {
        return transitions.OrderBy(t => t.SourceStateKey, StringComparer.Ordinal)
            .ThenBy(t => t.EventKey, StringComparer.Ordinal)
            .ThenBy(t => t.TargetStateKey, StringComparer.Ordinal)
            .ThenBy(t => t.TransitionId, StringComparer.Ordinal);
    }
}
