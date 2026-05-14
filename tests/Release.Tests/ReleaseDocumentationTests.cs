using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class ReleaseDocumentationTests
{
    [Fact]
    public void ReleaseReadinessDocsAndScriptsUseConsistentCommands()
    {
        var docs = ProjectPaths.ReadAllText("docs/release-readiness.md");
        var script = ProjectPaths.ReadAllText("eng/validate-release.sh");
        foreach (var command in ReleaseValidationFacts.OrderedCliCommands)
        {
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
        var guide = ProjectPaths.ReadAllText("docs/examples/parallel-regions.md");
        var readme = ProjectPaths.ReadAllText("README.md");
        var core = ProjectPaths.ReadAllText("docs/examples/core-fsm.md");
        var graph = ProjectPaths.ReadAllText("docs/examples/graph-introspection.md");
        var rendering = ProjectPaths.ReadAllText("docs/examples/graph-rendering.md");

        Assert.Contains("orthogonal regions", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("when to use parallel regions", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("when not to use parallel regions", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Operational", guide, StringComparison.Ordinal);
        Assert.Contains("Fulfillment", guide, StringComparison.Ordinal);
        Assert.Contains("Billing", guide, StringComparison.Ordinal);
        Assert.Contains("initial states", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("terminal states", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("declaration order", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("deterministic", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("single-region dispatch", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("same-event multi-region dispatch", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("completion after all regions are terminal", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("flattened state combinations", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("independent dimensions", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shared events", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("invalid model diagnostic", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("duplicate or invalid region names", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("missing initial states", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("illegal boundaries", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ambiguous handling", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("conflicts are detected before commit", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("active-state shape", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DefinitionGraph.Regions", guide, StringComparison.Ordinal);
        Assert.Contains("optional visualization adapters", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("workflow orchestration", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hosted services", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("event sourcing", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence providers", guide, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("image rendering", guide, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("parallel-regions.md", readme, StringComparison.Ordinal);
        Assert.Contains("parallel-regions.md", core, StringComparison.Ordinal);
        Assert.Contains("parallel-regions.md", graph, StringComparison.Ordinal);
        Assert.Contains("parallel-regions.md", rendering, StringComparison.Ordinal);
        Assert.Contains("ParallelComposite", core, StringComparison.Ordinal);
        Assert.Contains("DefinitionGraph.Regions", graph, StringComparison.Ordinal);
    }

    [Fact]
    public void TransitionPreviewDocumentationCoversExamplesAndCaveats()
    {
        var readme = ProjectPaths.ReadAllText("README.md");
        var core = ProjectPaths.ReadAllText("docs/examples/core-fsm.md");

        Assert.Contains("PreviewAsync", readme, StringComparison.Ordinal);
        Assert.Contains("transition preview", core, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TransitionDenialReason", core, StringComparison.Ordinal);
        Assert.Contains("pure/idempotent", core, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("observers", core, StringComparison.OrdinalIgnoreCase);
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