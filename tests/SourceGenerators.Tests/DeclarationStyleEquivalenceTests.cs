namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DeclarationStyleEquivalenceTests
{
    [Fact]
    public void AttributeAndDslStylesEmitEquivalentCoreTransitionCalls()
    {
        var attribute = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        var dsl = GeneratorTestHost.Run(TestSources.DslLifecycle);
        GeneratorTestHost.AssertCompiles(attribute);
        GeneratorTestHost.AssertCompiles(dsl);
        foreach (var call in new[]
                 {
                     ".On(OrderEvent.Pay).GoTo(OrderState.Paid)", ".On(OrderEvent.Cancel).GoTo(OrderState.Cancelled)",
                     ".On(OrderEvent.Ship).GoTo(OrderState.Shipped)"
                 })
        {
            Assert.Contains(call, attribute.GeneratedSource);
            Assert.Contains(call, dsl.GeneratedSource);
        }
    }
}