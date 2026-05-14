using StateForge.Core.Definitions;
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests;

public sealed class ParallelRegionAdapterConsumptionTests
{
    [Fact]
    public void Renderers_consume_graph_region_metadata_as_comments()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.ParallelComposite("Operational")
                .Region("Fulfillment", "WaitingForPick", "Packing")
                .Region("Billing", "WaitingForPayment", "CapturingPayment");
            builder.State("WaitingForPick").On("go").GoTo("Packing");
            builder.State("WaitingForPayment").On("pay").GoTo("CapturingPayment");
        });
        var graph = definition.ExportGraph().Graph!;

        Assert.Contains("parallel-region", MermaidGraphRenderer.Render(graph));
        Assert.Contains("parallel-region", GraphvizDotRenderer.Render(graph));
        Assert.Contains("parallel-region", PlantUmlGraphRenderer.Render(graph));
    }
}