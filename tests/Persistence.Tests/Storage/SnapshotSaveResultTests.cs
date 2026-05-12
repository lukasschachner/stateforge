using Persistence.Tests.TestSupport;
using StateMachineLibrary.Persistence.Diagnostics;
using StateMachineLibrary.Persistence.Snapshots;
using StateMachineLibrary.Persistence.Storage;

namespace Persistence.Tests.Storage;

public class SnapshotSaveResultTests
{
    [Fact]
    public void SavedContainsCommittedSnapshot()
    {
        var expected = PersistenceVersion.From("v1");
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, expected);
        var committed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Paid,
            PersistenceVersion.From("v2"));

        var result = SnapshotSaveResult<OrderState>.Saved(expected, proposed, committed);

        Assert.Equal(SnapshotSaveCategory.Saved, result.Category);
        Assert.Same(committed, result.CommittedSnapshot);
    }

    [Fact]
    public void ConcurrentStateChangeIsNotCommitted()
    {
        var expected = PersistenceVersion.From("v1");
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, expected);

        var result =
            SnapshotSaveResult<OrderState>.ConcurrentStateChange(expected, proposed, PersistenceVersion.From("v2"));

        Assert.Equal(SnapshotSaveCategory.ConcurrentStateChange, result.Category);
        Assert.Null(result.CommittedSnapshot);
    }

    [Fact]
    public void StorageFailureContainsDiagnostics()
    {
        var expected = PersistenceVersion.From("v1");
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, expected);
        var diagnostics = new PersistenceDiagnostics("failure", exception: new InvalidOperationException("boom"));

        var result = SnapshotSaveResult<OrderState>.StorageFailure(expected, proposed, diagnostics);

        Assert.Equal(SnapshotSaveCategory.StorageFailure, result.Category);
        Assert.Same(diagnostics, result.Diagnostics);
    }

    [Fact]
    public void CancelledIsNotCommitted()
    {
        var expected = PersistenceVersion.From("v1");
        var proposed = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, expected);

        var result = SnapshotSaveResult<OrderState>.Cancelled(expected, proposed);

        Assert.Equal(SnapshotSaveCategory.Cancelled, result.Category);
        Assert.Null(result.CommittedSnapshot);
    }
}