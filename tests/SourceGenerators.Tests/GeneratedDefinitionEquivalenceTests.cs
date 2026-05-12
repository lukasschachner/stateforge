namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedDefinitionEquivalenceTests
{
    [Fact]
    public void GeneratedDefinitionUsesSameCoreBuilderAndValidationSurfaceAsManualDefinitions()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("StateMachineDefinition<global::OrderState, global::OrderEvent>.Create(machine =>",
            result.GeneratedSource);
        Assert.Contains("machine.State(OrderState.Created)", result.GeneratedSource);
    }
}