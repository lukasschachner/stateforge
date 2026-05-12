using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class GraphRenderingSampleTests
{
    [Fact]
    public void GraphRenderingSampleRunsAndCreatesExpectedArtifacts()
    {
        var output = CommandRunner.Run("dotnet",
            "run --project samples/Graph.RenderingSample/Graph.RenderingSample.csproj --configuration Release --no-build");

        Assert.Contains("Graph rendering sample completed", output, StringComparison.Ordinal);

        foreach (var relativePath in new[]
                 {
                     "artifacts/graph-rendering/order-flow.mmd",
                     "artifacts/graph-rendering/order-flow.dot",
                     "artifacts/graph-rendering/order-flow.puml",
                     "artifacts/graph-rendering/offer-order-invoice-cancellation-flow.mmd",
                     "artifacts/graph-rendering/offer-order-invoice-cancellation-flow.dot",
                     "artifacts/graph-rendering/offer-order-invoice-cancellation-flow.puml"
                 })
        {
            var fullPath = ProjectPaths.FullPath(relativePath);
            Assert.True(File.Exists(fullPath), $"Expected sample artifact '{relativePath}' to exist.");
            var text = File.ReadAllText(fullPath);
            Assert.False(string.IsNullOrWhiteSpace(text),
                $"Expected sample artifact '{relativePath}' to contain diagram text.");
        }
    }
}