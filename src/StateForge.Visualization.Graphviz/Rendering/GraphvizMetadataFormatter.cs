using StateForge.Core.Definitions;
using StateForge.Core.Introspection;
using StateForge.Visualization.Shared.Rendering;

namespace StateForge.Visualization.Graphviz.Rendering;

internal static class GraphvizMetadataFormatter
{
    public static string Format(MetadataCollection metadata) => GraphRenderingMetadataFormatter.Format(metadata);

    public static string FormatActions(IEnumerable<GraphActionSummary> actions) =>
        GraphRenderingMetadataFormatter.FormatActions(actions);
}
