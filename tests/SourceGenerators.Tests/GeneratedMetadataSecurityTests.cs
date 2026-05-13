namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedMetadataSecurityTests
{
    [Fact]
    public void GeneratedMetadataAvoidsEnvironmentSpecificContent()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        GeneratedSourceAssertions.DoesNotContainEnvironmentSpecificContent(result.GeneratedSource);
        Assert.DoesNotContain("System.Exception", result.GeneratedSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception: ", result.GeneratedSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\n   at ", result.GeneratedSource, StringComparison.Ordinal);
    }
}
