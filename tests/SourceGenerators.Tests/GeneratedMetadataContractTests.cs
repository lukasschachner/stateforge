namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedMetadataContractTests
{
    [Fact]
    public void GeneratedMetadataIncludesStatesEventsTransitionsAndHelperStatus()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("GeneratedMetadata", result.GeneratedSource);
        Assert.Contains("state:S.A:terminal=false", result.GeneratedSource);
        Assert.Contains("state:S.B:terminal=true", result.GeneratedSource);
        Assert.Contains("event:E.Go:kind=Value:helper=Generated:reason=None", result.GeneratedSource);
        Assert.Contains("transition:", result.GeneratedSource);
    }
}
