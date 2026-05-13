using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.Parallel;

namespace Core.Tests.Introspection;

public class PermittedEventsTests
{
    [Fact]
    public async Task PermittedEventsCanBeQueriedForSuppliedAndRuntimeState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid).On<Cancel>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid).On<Ship>().GoTo(OrderState.Shipped);
            builder.State(OrderState.Cancelled);
            builder.State(OrderState.Shipped);
        });
        var runtime = definition.CreateRuntime(OrderState.Paid);

        var created = await definition.GetPermittedEventsAsync(OrderState.Created);
        var paid = await runtime.GetPermittedEventsAsync();

        Assert.Equal(2, created.Count);
        Assert.Contains(created, e => e.DisplayName == nameof(Pay));
        var onlyPaid = Assert.Single(paid);
        Assert.Equal(nameof(Ship), onlyPaid.DisplayName);
    }

    [Fact]
    public async Task RuntimePermittedEventsIncludeAllActiveParallelRegions()
    {
        var definition = ParallelGraphTestData.CreateTwoRegionDefinition();
        var runtime = definition.CreateRuntime(ParallelState.Operational);

        var events = await runtime.GetPermittedEventsAsync();

        Assert.Contains(events, e => e.DisplayName == ParallelEvent.PickStarted.ToString());
        Assert.Contains(events, e => e.DisplayName == ParallelEvent.PaymentStarted.ToString());
    }
}