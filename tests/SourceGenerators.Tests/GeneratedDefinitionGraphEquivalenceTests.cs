namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedDefinitionGraphEquivalenceTests
{
    [Fact]
    public void GeneratedDefinitionsPreserveTransitionTopologyForIntrospectionAndGraphExport()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".On(OrderEvent.Pay).GoTo(OrderState.Paid)", result.GeneratedSource);
        Assert.Contains(".On(OrderEvent.Cancel).GoTo(OrderState.Cancelled)", result.GeneratedSource);
        Assert.Contains(".On(OrderEvent.Ship).GoTo(OrderState.Shipped)", result.GeneratedSource);
    }

    [Fact]
    public void GeneratedAdvancedDefinitionsPreserveRendererNeutralMetadataBuilderCalls()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Operational, A, Done }
                     public enum E { Finish }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "Main", S.A, IsInitial = true)]
                     [Region(S.Operational, "Main", S.Done, IsTerminal = true)]
                     [Event(E.Finish)]
                     [Transition(S.A, E.Finish, S.Done)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.ParallelComposite(S.Operational).Region", result.GeneratedSource);
        Assert.DoesNotContain("Mermaid", result.GeneratedSource);
        Assert.DoesNotContain("Graphviz", result.GeneratedSource);
        Assert.DoesNotContain("PlantUML", result.GeneratedSource);
    }

}
