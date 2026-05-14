namespace StateForge.Core.Definitions;

/// <summary>Immutable definition of one ordered region owned by a parallel composite state.</summary>
public sealed record ParallelRegionDefinition<TState>(
    string RegionId,
    string Name,
    TState OwnerCompositeState,
    int Order,
    bool HasInitialState,
    TState? InitialState,
    IReadOnlyList<TState> MemberStates,
    IReadOnlyList<TState> TerminalStates,
    MetadataCollection Metadata)
{
    public ParallelRegionDefinition(
        string regionId,
        string name,
        TState ownerCompositeState,
        int order,
        TState? initialState,
        IEnumerable<TState>? memberStates = null,
        IEnumerable<TState>? terminalStates = null,
        MetadataCollection? metadata = null,
        bool hasInitialState = true)
        : this(regionId, name, ownerCompositeState, order, hasInitialState, initialState,
            (memberStates ?? []).ToArray(), (terminalStates ?? []).ToArray(), metadata ?? MetadataCollection.Empty)
    {
    }
}