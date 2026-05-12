namespace StateMachineLibrary.Core.Execution;

/// <summary>Represents the active hierarchy path from a top-level state to the active leaf.</summary>
public sealed class ActiveStatePath<TState>
{
    public ActiveStatePath(IEnumerable<TState> statesRootToLeaf)
    {
        StatesRootToLeaf = (statesRootToLeaf ?? throw new ArgumentNullException(nameof(statesRootToLeaf))).ToArray();
        if (StatesRootToLeaf.Count == 0)
            throw new ArgumentException("An active state path must contain at least one state.",
                nameof(statesRootToLeaf));
    }

    public IReadOnlyList<TState> StatesRootToLeaf { get; }
    public TState ActiveLeafState => StatesRootToLeaf[^1];

    public IReadOnlyList<TState> AncestorsRootToParent =>
        StatesRootToLeaf.Take(Math.Max(0, StatesRootToLeaf.Count - 1)).ToArray();

    public int Depth => StatesRootToLeaf.Count;

    public bool Contains(TState state, IEqualityComparer<TState>? comparer = null)
    {
        return StatesRootToLeaf.Contains(state, comparer ?? EqualityComparer<TState>.Default);
    }

    public override string ToString()
    {
        return string.Join(" -> ", StatesRootToLeaf);
    }
}