namespace StateForge.Visualization.Graphviz.Rendering;

/// <summary>Options for deterministic Graphviz DOT rendering.</summary>
public sealed class GraphvizRenderOptions
{
    /// <summary>Gets or sets whether graph, node, and edge metadata annotations are emitted.</summary>
    public bool IncludeMetadata { get; set; }

    /// <summary>Gets or sets an optional DOT graph title override.</summary>
    public string? DiagramTitle { get; set; }

    /// <summary>Gets or sets whether runtime active-state overlay hints are emitted when graph data contains them.</summary>
    public bool RenderRuntimeOverlay { get; set; }

    /// <summary>Gets or sets the line ending used in the rendered output.</summary>
    public string NewLine { get; set; } = "\n";

    /// <summary>Gets or sets the indentation prefix used in DOT statements.</summary>
    public string Indentation { get; set; } = "  ";

    internal void Validate()
    {
        if (string.IsNullOrEmpty(NewLine)) throw new ArgumentException("NewLine must be non-empty.", nameof(NewLine));
    }
}