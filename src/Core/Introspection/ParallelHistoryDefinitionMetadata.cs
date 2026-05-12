using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Definition-time metadata for a history-enabled parallel composite.</summary>
public sealed record ParallelHistoryDefinitionMetadata<TState>(
    TState CompositeState,
    HistoryMode HistoryMode,
    IReadOnlyList<ParallelHistoryRegionFallbackMetadata<TState>> RegionFallbacks);

/// <summary>Definition-time fallback metadata for one parallel region.</summary>
public sealed record ParallelHistoryRegionFallbackMetadata<TState>(
    string RegionId,
    string RegionName,
    int RegionOrder,
    TState? FallbackState);