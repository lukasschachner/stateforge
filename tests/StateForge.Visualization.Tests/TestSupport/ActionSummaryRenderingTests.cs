using StateForge.Core.Definitions;
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

namespace StateForge.Visualization.Tests.TestSupport;

public class ActionSummaryRenderingTests
{
    [Fact]
    public void RenderersIncludeActionSummariesWithoutExecutingActions()
    {
        var calls = 0;
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => calls++, "exit created")
                .On<ActionPay>()
                .Execute(_ => calls++, "transition pay")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ => calls++, "entry paid");
        });
        var graph = definition.ExportGraph().Graph!;

        var mermaid = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions { IncludeMetadata = true });
        var graphviz = GraphvizDotRenderer.Render(graph, new GraphvizRenderOptions { IncludeMetadata = true });
        var plantUml = PlantUmlGraphRenderer.Render(graph, new PlantUmlRenderOptions { IncludeMetadata = true });

        Assert.Equal(0, calls);
        Assert.Contains("exit created", mermaid);
        Assert.Contains("transition pay", graphviz);
        Assert.Contains("entry paid", plantUml);
    }

    private enum ActionState
    {
        Created,
        Paid
    }

    private abstract record ActionEvent;

    private sealed record ActionPay : ActionEvent;
}