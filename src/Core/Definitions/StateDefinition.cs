namespace StateMachineLibrary.Core.Definitions;

/// <summary>Declares a finite state machine state.</summary>
public sealed record StateDefinition<TState>
{
    public StateDefinition(
        TState value,
        bool isTerminal = false,
        MetadataCollection? metadata = null,
        IEnumerable<StateActionDefinition<TState>>? entryActions = null,
        IEnumerable<StateActionDefinition<TState>>? exitActions = null,
        StateHierarchyDefinition<TState>? hierarchy = null)
    {
        Value = value;
        IsTerminal = isTerminal;
        Metadata = metadata ?? MetadataCollection.Empty;
        EntryActions = (entryActions ?? []).ToArray();
        ExitActions = (exitActions ?? []).ToArray();
        Hierarchy = hierarchy ?? StateHierarchyDefinition<TState>.Empty;
    }

    public TState Value { get; init; }
    public bool IsTerminal { get; init; }
    public MetadataCollection Metadata { get; init; }
    public IReadOnlyList<StateActionDefinition<TState>> EntryActions { get; init; }
    public IReadOnlyList<StateActionDefinition<TState>> ExitActions { get; init; }
    public StateHierarchyDefinition<TState> Hierarchy { get; init; }
    public bool HasParent => Hierarchy.HasParent;
    public TState? ParentState => Hierarchy.ParentState;
    public bool HasInitialChild => Hierarchy.HasInitialChild;
    public TState? InitialChildState => Hierarchy.InitialChildState;
    public HistoryMode HistoryMode => Hierarchy.HistoryMode;
    public bool HasHistory => Hierarchy.HasHistory;
    public bool HasHistoryFallback => Hierarchy.HasHistoryFallback;
    public TState? HistoryFallbackState => Hierarchy.HistoryFallbackState;
}