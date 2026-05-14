using StateForge.Persistence.Tests.TestSupport;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.Tests.Snapshots;

public class StateSnapshotTests
{
    [Fact]
    public void ConstructorRequiresFields()
    {
        Assert.Throws<ArgumentException>(() => new StateSnapshot<OrderState>("", PersistenceTestDomain.DefinitionId,
            OrderState.Draft, PersistenceVersion.From("v1")));
        Assert.Throws<ArgumentException>(() =>
            new StateSnapshot<OrderState>("order-1", "", OrderState.Draft, PersistenceVersion.From("v1")));
    }

    [Fact]
    public void SnapshotIsImmutable()
    {
        var snapshot = new StateSnapshot<OrderState>(
            "order-1",
            PersistenceTestDomain.DefinitionId,
            OrderState.Submitted,
            PersistenceVersion.From("v2"),
            PersistencePropertyBag.Empty.With("source", "test"));

        Assert.Equal("order-1", snapshot.InstanceId);
        Assert.Equal(PersistenceTestDomain.DefinitionId, snapshot.DefinitionId);
        Assert.Equal(OrderState.Submitted, snapshot.ActiveState);
        Assert.Equal("v2", snapshot.Version.Value);
        Assert.Equal("test", snapshot.Properties["source"]);
    }
}