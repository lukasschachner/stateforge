using StateForge.Core.Definitions;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Introspection;

public sealed class ParallelGraphCompatibilityTests
{
    [Fact]
    public void Non_parallel_graph_has_no_region_metadata()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.State(ParallelState.Idle).On(ParallelEvent.Start).GoTo(ParallelState.Operational);
            b.State(ParallelState.Operational);
        });
        Assert.Empty(definition.ExportGraph().Graph!.Regions);
    }
}