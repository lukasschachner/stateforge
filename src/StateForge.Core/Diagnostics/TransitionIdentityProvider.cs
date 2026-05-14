using StateForge.Core.Definitions;

namespace StateForge.Core.Diagnostics;

internal static class TransitionIdentityProvider
{
    public static string? GetTransitionId<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TransitionDefinition<TState, TEvent>? transition)
    {
        if (transition is null) return null;

        if (transition.CompletionTransition is not null)
            return GetTransitionId(definition, transition.CompletionTransition);

        var index = IndexOfReference(definition.Transitions, transition);
        if (index < 0)
            index = definition.Transitions.ToList().FindIndex(t =>
                EqualityComparer<TState>.Default.Equals(t.SourceState, transition.SourceState) &&
                EqualityComparer<TState>.Default.Equals(t.TargetState, transition.TargetState) &&
                StringComparer.Ordinal.Equals(t.Event.Identity, transition.Event.Identity));

        return index < 0 ? null : CreateTransitionId(index);
    }

    public static string? GetTransitionId<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        CompletionTransitionDefinition<TState, TEvent>? transition)
    {
        if (transition is null) return null;

        var index = IndexOfReference(definition.CompletionTransitions, transition);
        if (index < 0)
            index = definition.CompletionTransitions.ToList().FindIndex(t =>
                EqualityComparer<TState>.Default.Equals(t.SourceState, transition.SourceState) &&
                EqualityComparer<TState>.Default.Equals(t.TargetState, transition.TargetState) &&
                t.DeclarationOrder == transition.DeclarationOrder);

        return index < 0 ? null : CreateTransitionId(definition.Transitions.Count + index);
    }

    public static string CreateTransitionId(int declarationIndex)
    {
        return $"transition-{declarationIndex:000}";
    }

    private static int IndexOfReference<T>(IReadOnlyList<T> items, T value)
        where T : class
    {
        for (var i = 0; i < items.Count; i++)
            if (ReferenceEquals(items[i], value))
                return i;

        return -1;
    }
}
