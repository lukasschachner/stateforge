namespace StateForge.Core.Definitions;

/// <summary>Declares optional hierarchy metadata for a state.</summary>
public sealed record StateHierarchyDefinition<TState>
{
    public StateHierarchyDefinition(
        bool hasParent = false,
        TState? parentState = default,
        bool hasInitialChild = false,
        TState? initialChildState = default,
        HistoryMode historyMode = HistoryMode.None,
        bool hasHistoryFallback = false,
        TState? historyFallbackState = default,
        bool isParallelComposite = false,
        string? parallelRegionId = null)
    {
        HasParent = hasParent;
        ParentState = parentState;
        HasInitialChild = hasInitialChild;
        InitialChildState = initialChildState;
        HistoryMode = historyMode;
        HasHistoryFallback = hasHistoryFallback;
        HistoryFallbackState = historyFallbackState;
        IsParallelComposite = isParallelComposite;
        ParallelRegionId = parallelRegionId;
    }

    public static StateHierarchyDefinition<TState> Empty { get; } = new();

    public bool HasParent { get; init; }
    public TState? ParentState { get; init; }
    public bool HasInitialChild { get; init; }
    public TState? InitialChildState { get; init; }
    public HistoryMode HistoryMode { get; init; }
    public bool HasHistory => HistoryMode != HistoryMode.None;
    public bool HasHistoryFallback { get; init; }
    public TState? HistoryFallbackState { get; init; }
    public bool IsParallelComposite { get; init; }
    public string? ParallelRegionId { get; init; }
    public bool HasParallelRegionMembership => !string.IsNullOrWhiteSpace(ParallelRegionId);

    public bool HasHierarchyMetadata => HasParent || HasInitialChild || HasHistory || IsParallelComposite ||
                                        HasParallelRegionMembership;

    public StateHierarchyDefinition<TState> WithParent(TState parentState)
    {
        return this with { HasParent = true, ParentState = parentState };
    }

    public StateHierarchyDefinition<TState> WithInitialChild(TState initialChildState)
    {
        return this with { HasInitialChild = true, InitialChildState = initialChildState };
    }

    public StateHierarchyDefinition<TState> WithHistory(HistoryMode mode, bool hasFallback = false,
        TState? fallbackState = default)
    {
        return this with { HistoryMode = mode, HasHistoryFallback = hasFallback, HistoryFallbackState = fallbackState };
    }

    public StateHierarchyDefinition<TState> AsParallelComposite()
    {
        return this with { IsParallelComposite = true };
    }

    public StateHierarchyDefinition<TState> InParallelRegion(string regionId)
    {
        return this with { ParallelRegionId = regionId };
    }
}