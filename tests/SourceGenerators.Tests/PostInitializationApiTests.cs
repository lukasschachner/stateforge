namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class PostInitializationApiTests
{
    [Fact]
    public void GeneratorProvidedDeclarationApisCompile()
    {
        var result = GeneratorTestHost.Run(TestSources.DslLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("StateMachineDeclarationApi.g.cs", result.GeneratedHintNames);
    }
}