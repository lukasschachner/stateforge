using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class ReleaseDocumentationTests
{
    [Fact]
    public void QuickstartDocsAndScriptsUseConsistentCommands()
    {
        var quickstart = ProjectPaths.ReadAllText("specs/005-nuget-release-readiness/quickstart.md");
        var docs = ProjectPaths.ReadAllText("docs/release-readiness.md");
        var script = ProjectPaths.ReadAllText("eng/validate-release.sh");
        foreach (var command in ReleaseValidationFacts.OrderedCliCommands)
        {
            Assert.Contains(command, quickstart);
            Assert.Contains(command, docs);
            Assert.Contains(command, script);
        }
    }

    [Fact]
    public void GraphRenderingDocumentationCoversAllSupportedFormats()
    {
        var graphRendering = ProjectPaths.ReadAllText("docs/examples/graph-rendering.md");
        Assert.Contains("Mermaid", graphRendering, StringComparison.Ordinal);
        Assert.Contains("Graphviz", graphRendering, StringComparison.Ordinal);
        Assert.Contains("PlantUML", graphRendering, StringComparison.Ordinal);

        var introspection = ProjectPaths.ReadAllText("docs/examples/graph-introspection.md");
        Assert.Contains("graph-rendering.md", introspection, StringComparison.Ordinal);
    }

    [Fact]
    public void ParallelRegionDocumentationCoversBoundariesAndGraphMetadata()
    {
        var readme = ProjectPaths.ReadAllText("README.md");
        var core = ProjectPaths.ReadAllText("docs/examples/core-fsm.md");
        var graph = ProjectPaths.ReadAllText("docs/examples/graph-introspection.md");
        Assert.Contains("Parallel regions", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("workflow orchestration", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ParallelComposite", core, StringComparison.Ordinal);
        Assert.Contains("DefinitionGraph.Regions", graph, StringComparison.Ordinal);
    }

    [Fact]
    public void ActiveStateSnapshotDocumentationCoversCaptureValidationRestoreAndMigration()
    {
        var readme = ProjectPaths.ReadAllText("README.md");
        var core = ProjectPaths.ReadAllText("docs/examples/core-fsm.md");
        var graph = ProjectPaths.ReadAllText("docs/examples/graph-introspection.md");
        var persistence = ProjectPaths.ReadAllText("docs/examples/persistence.md");

        Assert.Contains("Active state snapshots", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CaptureActiveStateSnapshot", core, StringComparison.Ordinal);
        Assert.Contains("ValidateActiveStateSnapshot", core, StringComparison.Ordinal);
        Assert.Contains("GetActiveStateSnapshotKind", graph, StringComparison.Ordinal);
        Assert.Contains("Migrating to active-shape snapshots", persistence, StringComparison.Ordinal);
    }
}