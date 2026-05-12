namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DeterministicGenerationTests
{
    [Fact]
    public void UnchangedAttributeDeclarationsGenerateByteStableSource()
    {
        var first = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        var second = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(first);
        GeneratorTestHost.AssertCompiles(second);
        Assert.Equal(first.GeneratedSource, second.GeneratedSource);
    }
}