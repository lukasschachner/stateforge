using StateForge.Core.Definitions;
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests;

public sealed class ParallelHistoryMetadataRenderingTests
{
    [Fact]
    public void Visualization_adapters_consume_exported_parallel_history_metadata()
    {
        var export = StateMachineDefinition<State, Event>.Create(builder =>
        {
            builder.ParallelComposite(State.Operational)
                .WithHistory()
                .Region("A", State.A1, State.A1, State.A2)
                .Region("B", State.B1, State.B1);
        }).ExportGraph();

        Assert.True(export.Succeeded);
        Assert.NotNull(export.Graph);

        Assert.Contains("history=Shallow", MermaidGraphRenderer.Render(export.Graph!));
        Assert.Contains("history=Shallow", GraphvizDotRenderer.Render(export.Graph!));
        Assert.Contains("history=Shallow", PlantUmlGraphRenderer.Render(export.Graph!));
    }

    private enum State
    {
        Operational,
        A1,
        A2,
        B1
    }

    private enum Event
    {
        Go
    }
}