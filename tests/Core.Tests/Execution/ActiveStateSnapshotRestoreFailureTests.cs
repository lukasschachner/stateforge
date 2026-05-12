using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Execution;

public sealed class ActiveStateSnapshotRestoreFailureTests
{
    [Fact]
    public void InvalidSnapshotRestoreThrowsBeforeRuntimeIsReturned()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var snapshot = ActiveStateSnapshot<SnapshotState>.Parallel(SnapshotState.Other, []);

        var exception = Assert.Throws<ActiveStateSnapshotValidationException<SnapshotState>>(() =>
            definition.CreateRuntime(snapshot));

        Assert.False(exception.ValidationResult.IsValid);
        Assert.Contains(exception.ValidationResult.Diagnostics,
            diagnostic => diagnostic.Code == ActiveStateSnapshotValidationCodes.UnknownState);
    }
}
