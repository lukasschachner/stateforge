namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedDefinitionCachingTests
{
    [Fact]
    public void GeneratedDefinitionsExposeReusableCachedAccessorAndFactory()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("Lazy<global::StateMachineLibrary.Core.Definitions.StateMachineDefinition",
            result.GeneratedSource);
        Assert.Contains("Definition => __generatedDefinition.Value", result.GeneratedSource);
        Assert.Contains("CreateDefinition()", result.GeneratedSource);
    }
}