using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

internal static class RuntimeGraphExportAssertions
{
    public static DefinitionGraph<TState, TEvent> SucceededGraph<TState, TEvent>(
        GraphExportResult<TState, TEvent> result)
    {
        Assert.True(result.Succeeded, result.FailureSummary);
        return Assert.IsType<DefinitionGraph<TState, TEvent>>(result.Graph);
    }

    public static GraphActiveStateOverlay<TState> RuntimeOverlay<TState, TEvent>(
        GraphExportResult<TState, TEvent> result)
    {
        var graph = SucceededGraph(result);
        return Assert.IsType<GraphActiveStateOverlay<TState>>(graph.RuntimeOverlay);
    }

    public static string NodeIdFor<TState, TEvent>(DefinitionGraph<TState, TEvent> graph, TState state)
    {
        return graph.Nodes.Single(node => EqualityComparer<TState>.Default.Equals(node.State, state)).Id;
    }
}
