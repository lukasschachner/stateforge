namespace StateMachineLibrary.SourceGenerators.Tests;

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
}