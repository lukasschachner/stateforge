using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal static class HierarchyCompletionEvaluator
{
    public static NestedCompletion<TState>? Evaluate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeShape)
    {
        if (ParallelCompletionEvaluator.IsComplete(definition, activeShape))
            return new NestedCompletion<TState>(activeShape.OwningCompositeState!, activeShape.OwningCompositeState,
                NestedCompletionKind.Composite, true, activeShape.ActiveRegions.Select(r => r.RegionId).ToArray());

        return activeShape.ActiveLeafState is null ? null : Evaluate(definition, activeShape.ActiveLeafState);
    }

    public static NestedCompletion<TState>? Evaluate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState activeLeaf)
    {
        var state = definition.FindState(activeLeaf);
        if (state?.IsTerminal != true) return null;

        var path = definition.GetActiveStatePath(activeLeaf).StatesRootToLeaf;
        if (path.Count == 1) return new NestedCompletion<TState>(activeLeaf, default, NestedCompletionKind.Machine);

        return new NestedCompletion<TState>(activeLeaf, path[^2], NestedCompletionKind.Child);
    }
}