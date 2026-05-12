namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class AttributeGenerationTests
{
    [Fact]
    public void ValidAttributeDeclarationGeneratesDefinitionWithStatesTransitionsTerminalAndMetadata()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(
            "public static global::StateMachineLibrary.Core.Definitions.StateMachineDefinition<global::OrderState, global::OrderEvent> Definition",
            result.GeneratedSource);
        Assert.Contains("machine.State(OrderState.Shipped).Terminal()", result.GeneratedSource);
        Assert.Contains("machine.WithMetadata(\"owner\", \"sales\")", result.GeneratedSource);
        Assert.Contains(".On(OrderEvent.Pay).GoTo(OrderState.Paid)", result.GeneratedSource);
    }

    [Fact]
    public void ExistingFlatAttributeDeclarationsDoNotEmitAdvancedBuilderCalls()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.State(OrderState.Created);", result.GeneratedSource);
        Assert.DoesNotContain(".InitialChild(", result.GeneratedSource);
        Assert.DoesNotContain(".ParallelComposite()", result.GeneratedSource);
        Assert.DoesNotContain("machine.ParallelComposite(", result.GeneratedSource);
    }

}
