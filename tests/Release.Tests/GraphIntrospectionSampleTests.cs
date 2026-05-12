using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class GraphIntrospectionSampleTests
{
    [Fact]
    public void GraphIntrospectionSampleRuns()
    {
        var output = CommandRunner.Run("dotnet",
            "run --project samples/Graph.IntrospectionSample/Graph.IntrospectionSample.csproj --configuration Release");
        Assert.Contains("Graph introspection sample completed", output);
        Assert.Contains("Parallel region:", output);
        Assert.Contains("Parallel history definition:", output);
        Assert.Contains("Active parallel shape:", output);
        Assert.Contains("Active snapshot kind:", output);
        Assert.Contains("Introspection snapshot kind:", output);
        Assert.Contains("Recorded parallel history:", output);
    }
}