namespace StateForge.Core.Introspection;

/// <summary>Renderer-neutral metadata describing one graph-exported parallel region.</summary>
public sealed record GraphRegionMetadata<TState>(
    TState CompositeState,
    string RegionId,
    string RegionName,
    int RegionOrder,
    TState? InitialState,
    IReadOnlyList<TState> TerminalStates,
    IReadOnlyList<TState> MemberStates,
    TState? ActiveLeafState = default,
    string? ParallelHistoryMode = null,
    bool ParallelHistorySupported = false,
    TState? ParallelHistoryFallbackState = default);