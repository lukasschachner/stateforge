namespace StateMachineLibrary.Core.Execution;

/// <summary>Read-only snapshot of runtime history for a history-enabled composite.</summary>
public sealed record CompositeHistorySnapshot<TState>(
    TState CompositeState,
    bool HasRecordedHistory,
    TState? RecordedDirectChildState,
    TState? RecordedDeepLeafState,
    TState? FallbackChildState,
    long LastUpdatedSequence);