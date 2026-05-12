using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal static class ParallelCompletionEvaluator
{
    public static bool IsComplete<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> shape)
    {
        if (!shape.IsParallel || shape.OwningCompositeState is null) return false;
        return shape.ActiveRegions.Count > 0 && shape.ActiveRegions.All(r => r.IsTerminal);
    }
}