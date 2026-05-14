namespace StateForge.Core.Execution;

/// <summary>Runtime record of the last committed active child for a history-enabled composite.</summary>
public sealed record CompositeHistoryRecord<TState>(
    TState CompositeState,
    TState LastActiveDirectChildState,
    TState? LastActiveDescendantLeafState,
    long LastUpdatedSequence);