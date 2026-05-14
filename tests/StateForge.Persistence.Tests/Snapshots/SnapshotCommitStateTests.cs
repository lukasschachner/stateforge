using StateForge.Persistence.Tests.TestSupport;
using StateForge.Core.Execution;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.Tests.Snapshots;

public class SnapshotCommitStateTests
{
    [Fact]
    public void ProposedSnapshotRetainsPreviousVersion()
    {
        var transition = TransitionOutcome<OrderState, OrderEvent>.Success(
            OrderState.Draft,
            OrderState.Submitted,
            new Submit(),
            PersistenceTestDomain.CreateDefinition().Transitions[0]);

        var snapshot = new ProposedSnapshot<OrderState, OrderEvent>(
            "order-1",
            PersistenceTestDomain.DefinitionId,
            OrderState.Submitted,
            PersistenceVersion.From("v2"),
            PersistenceVersion.From("v1"),
            transition);

        Assert.Equal("v1", snapshot.PreviousVersion.Value);
    }

    [Fact]
    public void CommittedSnapshotRequiresVersion()
    {
        var snapshot = new CommittedSnapshot<OrderState>(
            "order-1",
            PersistenceTestDomain.DefinitionId,
            OrderState.Submitted,
            PersistenceVersion.From("v2"));

        Assert.Equal("v2", snapshot.CommittedVersion.Value);
    }
}