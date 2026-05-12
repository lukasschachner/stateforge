using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Visualization.Graphviz.Rendering;
using StateMachineLibrary.Visualization.Mermaid.Rendering;
using StateMachineLibrary.Visualization.PlantUML.Rendering;

namespace StateMachineLibrary.Visualization.Tests;

public sealed class ParallelHistoryMetadataRenderingTests
{
    [Fact]
    public void Visualization_adapters_consume_exported_parallel_history_metadata()
    {
        var graph = StateMachineDefinition<State, Event>.Create(builder =>
        {
            builder.ParallelComposite(State.Operational)
                .WithHistory()
                .Region("A", State.A1, State.A1, State.A2)
                .Region("B", State.B1, State.B1);
        }).ExportGraph().Graph;

        Assert.Contains("history=Shallow", MermaidGraphRenderer.Render(graph));
        Assert.Contains("history=Shallow", GraphvizDotRenderer.Render(graph));
        Assert.Contains("history=Shallow", PlantUmlGraphRenderer.Render(graph));
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