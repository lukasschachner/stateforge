namespace StateMachineLibrary.Core.Execution;

/// <summary>Identifies the shape represented by an active-state snapshot.</summary>
public enum ActiveStateSnapshotKind
{
    /// <summary>A non-parallel machine represented by one active leaf state.</summary>
    SingleLeaf,

    /// <summary>A non-parallel hierarchical machine represented by an ordered active path.</summary>
    Hierarchical,

    /// <summary>A parallel composite represented by one active entry per declared region.</summary>
    Parallel
}
