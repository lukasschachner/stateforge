using StateForge.Core.Definitions;
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests;

public sealed class CompletionEdgeRenderingTests
{
    [Fact]
    public void Renderers_can_consume_completion_edge_classification()
    {
        var definition = StateMachineDefinition<State, Event>.Create(builder =>
        {
            builder.State(State.Parent).InitialChild(State.Child).OnCompletion().GoTo(State.Done);
            builder.State(State.Child).ChildOf(State.Parent).Terminal();
            builder.State(State.Done);
        });
        var graph = definition.ExportGraph().Graph!;

        Assert.Contains("completion", GraphvizDotRenderer.Render(graph));
        Assert.Contains("completion", MermaidGraphRenderer.Render(graph));
        Assert.Contains("completion", PlantUmlGraphRenderer.Render(graph));
    }

    private enum State
    {
        Parent,
        Child,
        Done
    }

    private enum Event
    {
        Go
    }
}
