using StateForge.Core.Definitions;
using StateForge.Core.Introspection;
using StateForge.Visualization.Shared.Rendering;

namespace StateForge.Visualization.Mermaid.Rendering;

internal static class MermaidMetadataFormatter
{
    public static string Format(MetadataCollection metadata) => GraphRenderingMetadataFormatter.Format(metadata);

    public static string FormatActions(IEnumerable<GraphActionSummary> actions) =>
        GraphRenderingMetadataFormatter.FormatActions(actions);
}
