namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedGraphMetadataTests
{
    [Fact]
    public void GeneratedGraphIncludesRendererNeutralNodesAndEdges()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("GeneratedGraph", result.GeneratedSource);
        Assert.Contains("node:S.A:terminal=false", result.GeneratedSource);
        Assert.Contains("node:S.B:terminal=true", result.GeneratedSource);
        Assert.Contains("edge:S.A:Transition:S.B", result.GeneratedSource);
        Assert.DoesNotContain("Mermaid", result.GeneratedSource);
        Assert.DoesNotContain("Graphviz", result.GeneratedSource);
        Assert.DoesNotContain("PlantUML", result.GeneratedSource);
    }
}
