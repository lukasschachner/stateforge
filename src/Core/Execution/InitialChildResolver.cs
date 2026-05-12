using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Resolves composite state targets through initial children until a leaf is reached.</summary>
internal static class InitialChildResolver
{
    public static TState ResolveTargetLeaf<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState targetState)
    {
        if (!definition.HasHierarchy || !definition.IsCompositeState(targetState)) return targetState;

        if (definition.IsParallelComposite(targetState))
            return ParallelRegionInitialResolver.FirstLeaf(definition, targetState);

        var comparer = EqualityComparer<TState>.Default;
        var seen = new HashSet<StateKey<TState>>(new StateKeyComparer<TState>(comparer));
        var current = targetState;
        while (definition.IsCompositeState(current))
        {
            if (!seen.Add(new StateKey<TState>(current))) return current;

            if (definition.IsParallelComposite(current))
                return ParallelRegionInitialResolver.FirstLeaf(definition, current);

            if (!definition.TryGetInitialChild(current, out var child)) return current;

            current = child!;
        }

        return current;
    }

    private readonly record struct StateKey<T>(T Value);

    private sealed class StateKeyComparer<T>(IEqualityComparer<T> comparer) : IEqualityComparer<StateKey<T>>
    {
        public bool Equals(StateKey<T> x, StateKey<T> y)
        {
            return comparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(StateKey<T> obj)
        {
            return obj.Value is null ? 0 : comparer.GetHashCode(obj.Value);
        }
    }
}