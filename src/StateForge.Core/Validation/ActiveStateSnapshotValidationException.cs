namespace StateForge.Core.Validation;

/// <summary>Exception thrown when an active-state snapshot cannot be restored safely.</summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed class ActiveStateSnapshotValidationException<TState> : InvalidOperationException
{
    public ActiveStateSnapshotValidationException(ActiveStateSnapshotValidationResult<TState> validationResult)
        : base("Active-state snapshot validation failed.")
    {
        ValidationResult = validationResult;
    }

    public ActiveStateSnapshotValidationResult<TState> ValidationResult { get; }
}
