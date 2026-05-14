namespace StateForge.SourceGenerators.Tests;

public sealed class GeneratedMemberContractTests
{
    [Fact]
    public void GeneratedMembersExposeDefinitionAndCreateDefinition()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(" Definition => __generatedDefinition.Value", result.GeneratedSource);
        Assert.Contains(" CreateDefinition()", result.GeneratedSource);
    }
}