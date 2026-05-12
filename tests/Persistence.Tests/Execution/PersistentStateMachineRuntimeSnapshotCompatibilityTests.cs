using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Persistence.Snapshots;

namespace Persistence.Tests.Execution;

public sealed class PersistentStateMachineRuntimeSnapshotCompatibilityTests
{
    [Fact]
    public void ExistingSingleStateSnapshotContractRemainsAvailableForNonParallelMachines()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.WithMetadata("persistence.definition_id", "orders");
            builder.State("Created").On("pay").GoTo("Paid");
            builder.State("Paid");
        });

        var runtime = definition.CreateRuntime("Created");
        var snapshot = new StateSnapshot<string>(
            "order-1",
            "orders",
            runtime.CurrentState,
            new PersistenceVersion("v1"));

        Assert.Equal("Created", snapshot.ActiveState);
        Assert.Equal("order-1", snapshot.InstanceId);
    }
}
