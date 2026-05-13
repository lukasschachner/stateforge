using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;
using StateMachineLibrary.Visualization.Shared.Rendering;

namespace StateMachineLibrary.Visualization.Mermaid.Rendering;

internal static class MermaidMetadataFormatter
{
    public static string Format(MetadataCollection metadata) => GraphRenderingMetadataFormatter.Format(metadata);

    public static string FormatActions(IEnumerable<GraphActionSummary> actions) =>
        GraphRenderingMetadataFormatter.FormatActions(actions);
}
