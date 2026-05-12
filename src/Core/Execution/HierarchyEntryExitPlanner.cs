namespace StateMachineLibrary.Core.Execution;

internal static class HierarchyEntryExitPlanner
{
    public static IReadOnlyList<ActiveRegionEntry<TState>> ExitOrder<TState>(
        IEnumerable<ActiveRegionEntry<TState>> activeRegions)
    {
        return activeRegions.Reverse().ToArray();
    }

    public static HierarchyEntryExitPlan<TState> Plan<TState>(ActiveStatePath<TState> sourcePath,
        ActiveStatePath<TState> targetPath)
    {
        var comparer = EqualityComparer<TState>.Default;
        var source = sourcePath.StatesRootToLeaf;
        var target = targetPath.StatesRootToLeaf;
        var common = 0;
        while (common < source.Count && common < target.Count &&
               comparer.Equals(source[common], target[common])) common++;

        var exit = source.Skip(common).Reverse().ToArray();
        var entry = target.Skip(common).ToArray();
        var lca = common == 0 ? default : source[common - 1];
        return new HierarchyEntryExitPlan<TState>(exit, entry, lca, common > 0);
    }
}

internal sealed record HierarchyEntryExitPlan<TState>(
    IReadOnlyList<TState> ExitStatesLeafToBoundary,
    IReadOnlyList<TState> EntryStatesBoundaryToLeaf,
    TState? LeastCommonAncestorState,
    bool HasLeastCommonAncestor);