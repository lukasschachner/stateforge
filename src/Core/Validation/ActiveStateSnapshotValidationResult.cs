using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Validation;

/// <summary>Structured issue emitted while validating an active-state snapshot.</summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed record ActiveStateSnapshotValidationDiagnostic<TState>(
    string Code,
    string Message,
    ActiveStateSnapshotKind? SnapshotKind = null,
    long? Sequence = null,
    TState? ReferencedState = default,
    TState? OwningCompositeState = default,
    string? RegionId = null,
    string? RegionName = null,
    TState? PathSegment = default);

/// <summary>Result of validating an active-state snapshot against a definition.</summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed class ActiveStateSnapshotValidationResult<TState>
{
    public ActiveStateSnapshotValidationResult(IEnumerable<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics)
    {
        Diagnostics = diagnostics.ToArray();
    }

    public static ActiveStateSnapshotValidationResult<TState> Valid { get; } = new([]);

    public IReadOnlyList<ActiveStateSnapshotValidationDiagnostic<TState>> Diagnostics { get; }

    public bool IsValid => Diagnostics.Count == 0;
}
