namespace StateForge.SourceGenerators.Tests;

public sealed class DslGenerationTests
{
    [Fact]
    public void CompactDslLifecycleGeneratesDefinition()
    {
        var result = GeneratorTestHost.Run(TestSources.DslLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("DeclarationStyle\", \"CompactDsl", result.GeneratedSource);
        Assert.Contains(".On(OrderEvent.Pay).GoTo(OrderState.Paid)", result.GeneratedSource);
    }

    [Fact]
    public void ExistingFlatDslDeclarationsKeepGeneratedDefinitionMembers()
    {
        var result = GeneratorTestHost.Run(TestSources.DslLifecycle);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("public static global::StateForge.Core.Definitions.StateMachineDefinition<global::OrderState, global::OrderEvent> Definition", result.GeneratedSource);
        Assert.Contains("public static global::StateForge.Core.Definitions.StateMachineDefinition<global::OrderState, global::OrderEvent> CreateDefinition()", result.GeneratedSource);
        Assert.DoesNotContain("machine.ParallelComposite(", result.GeneratedSource);
    }

}
