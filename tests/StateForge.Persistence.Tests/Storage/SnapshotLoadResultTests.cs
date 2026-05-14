using StateForge.Persistence.Tests.TestSupport;
using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Tests.Storage;

public class SnapshotLoadResultTests
{
    [Fact]
    public void LoadedContainsSnapshot()
    {
        var snapshot = new StateSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId, OrderState.Draft,
            PersistenceVersion.From("v1"));

        var result = SnapshotLoadResult<OrderState>.Loaded(snapshot);

        Assert.Equal(SnapshotLoadCategory.Loaded, result.Category);
        Assert.Same(snapshot, result.Snapshot);
    }

    [Fact]
    public void MissingContainsDiagnostics()
    {
        var diagnostics = new PersistenceDiagnostics("missing");

        var result = SnapshotLoadResult<OrderState>.MissingSnapshot(diagnostics);

        Assert.Equal(SnapshotLoadCategory.MissingSnapshot, result.Category);
        Assert.Null(result.Snapshot);
        Assert.Same(diagnostics, result.Diagnostics);
    }
}