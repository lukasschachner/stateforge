namespace StateForge.Core.Execution;

public enum NestedCompletionKind
{
    Child,
    Composite,
    Machine
}

public sealed record NestedCompletion<TState>(
    TState CompletedState,
    TState? CompletionScopeState,
    NestedCompletionKind Kind,
    bool IsParallelCompletion = false,
    IReadOnlyList<string>? CompletedRegionIds = null);