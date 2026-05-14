namespace StateForge.SourceGenerators.Tests;

public sealed class GeneratedEventHelperContractTests
{
    [Fact]
    public void SimpleValueEventsGenerateApplyHelpers()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("ApplyE_GoAsync", result.GeneratedSource);
        Assert.Contains("runtime.ApplyAsync(E.Go, cancellationToken)", result.GeneratedSource);
        Assert.Contains("Definition.ApplyAsync(currentState, E.Go, cancellationToken)", result.GeneratedSource);
    }
}
