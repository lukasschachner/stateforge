using StateForge.Core.Definitions;

#pragma warning disable CS8714

namespace StateForge.Core.Execution;

/// <summary>Per-machine ordered region history records for history-enabled parallel composites.</summary>
internal sealed class ParallelHistoryStore<TState>
{
    private readonly Dictionary<TState, Dictionary<string, ParallelRegionHistoryEntry<TState>>> _records = [];
    private long _sequence;

    public void Record<TEvent>(StateMachineDefinition<TState, TEvent> definition, ActiveStateShape<TState> shape)
    {
        if (!CanRecord(definition, shape, out var compositeState)) return;

        foreach (var entry in shape.ActiveRegions) RecordEntry(definition, compositeState, entry);
    }

    public void RecordChanged<TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> previousShape, ActiveStateShape<TState> nextShape)
    {
        if (!CanRecord(definition, nextShape, out var compositeState)) return;

        var previousByRegion = previousShape.IsParallel
            ? previousShape.ActiveRegions.ToDictionary(entry => entry.RegionId, StringComparer.Ordinal)
            : new Dictionary<string, ActiveRegionEntry<TState>>(StringComparer.Ordinal);
        var comparer = EqualityComparer<TState>.Default;
        foreach (var entry in nextShape.ActiveRegions)
        {
            if (previousByRegion.TryGetValue(entry.RegionId, out var previous) &&
                comparer.Equals(previous.ActiveLeafState, entry.ActiveLeafState)) continue;

            RecordEntry(definition, compositeState, entry);
        }
    }

    private bool CanRecord<TEvent>(StateMachineDefinition<TState, TEvent> definition, ActiveStateShape<TState> shape,
        out TState compositeState)
    {
        compositeState = default!;
        if (!shape.IsParallel || shape.OwningCompositeState is null ||
            !definition.TryGetHistoryDefinition(shape.OwningCompositeState, out var historyState)) return false;

        if (historyState.HistoryMode == HistoryMode.None) return false;

        compositeState = shape.OwningCompositeState;
        return true;
    }

    private void RecordEntry<TEvent>(StateMachineDefinition<TState, TEvent> definition, TState compositeState,
        ActiveRegionEntry<TState> entry)
    {
        var region = definition.GetParallelRegions(compositeState).FirstOrDefault(r => r.RegionId == entry.RegionId);
        if (region is null) return;

        if (!_records.TryGetValue(compositeState, out var compositeRecords))
        {
            compositeRecords = new Dictionary<string, ParallelRegionHistoryEntry<TState>>(StringComparer.Ordinal);
            _records[compositeState] = compositeRecords;
        }

        compositeRecords[entry.RegionId] = new ParallelRegionHistoryEntry<TState>(
            entry.RegionId,
            entry.RegionName,
            region.Order,
            entry.ActiveLeafState,
            entry.ActivePath,
            ++_sequence);
    }

    public IReadOnlyList<ParallelRegionHistoryEntry<TState>> GetEntries(TState compositeState)
    {
        if (!_records.TryGetValue(compositeState, out var entries))
            return Array.Empty<ParallelRegionHistoryEntry<TState>>();

        return entries.Values.OrderBy(entry => entry.RegionOrder).ToArray();
    }

    public IReadOnlyList<ParallelHistorySnapshot<TState>> CreateSnapshots<TEvent>(
        StateMachineDefinition<TState, TEvent> definition)
    {
        return definition.HistoryEnabledStates
            .Where(state => state.Hierarchy.IsParallelComposite && state.HistoryMode != HistoryMode.None)
            .Select(state => CreateSnapshot(definition, state.Value, state.HistoryMode))
            .ToArray();
    }

    private ParallelHistorySnapshot<TState> CreateSnapshot<TEvent>(StateMachineDefinition<TState, TEvent> definition,
        TState compositeState, HistoryMode mode)
    {
        var entries = GetEntries(compositeState);
        var expected = definition.GetParallelRegions(compositeState).Count;
        return new ParallelHistorySnapshot<TState>(
            compositeState,
            mode,
            entries,
            entries.Count == expected && expected > 0,
            entries.Count == 0 ? 0 : entries.Max(entry => entry.LastUpdatedSequence));
    }
}

#pragma warning restore CS8714