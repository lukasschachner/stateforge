using StateForge.Core.Execution;

namespace StateForge.Core.Tests.History;

public static class HistoryAssertions
{
    public static void ActivePathIs<TState>(ActiveStatePath<TState> path, params TState[] expected)
    {
        Assert.Equal(expected, path.StatesRootToLeaf);
    }

    public static void SnapshotRecorded<TState>(IEnumerable<CompositeHistorySnapshot<TState>> snapshots,
        TState composite, TState child)
    {
        var snapshot = Assert.Single(snapshots,
            s => EqualityComparer<TState>.Default.Equals(s.CompositeState, composite));
        Assert.True(snapshot.HasRecordedHistory);
        Assert.Equal(child, snapshot.RecordedDirectChildState);
    }
}