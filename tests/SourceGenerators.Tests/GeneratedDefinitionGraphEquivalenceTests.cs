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
}