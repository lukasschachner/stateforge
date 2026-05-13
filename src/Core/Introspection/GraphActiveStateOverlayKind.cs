namespace StateMachineLibrary.Core.Introspection;

/// <summary>Classifies the active-state shape represented by runtime graph overlay metadata.</summary>
public enum GraphActiveStateOverlayKind
{
    /// <summary>A flat machine with a single active leaf.</summary>
    SingleLeaf = 0,

    /// <summary>A hierarchical machine with one active path from composite ancestor to leaf.</summary>
    Hierarchical = 1,

    /// <summary>A parallel composite with one active leaf per declared region.</summary>
    Parallel = 2
}
