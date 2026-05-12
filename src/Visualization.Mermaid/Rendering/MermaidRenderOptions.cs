namespace StateMachineLibrary.Visualization.Mermaid.Rendering;

/// <summary>Options for deterministic Mermaid rendering.</summary>
public sealed class MermaidRenderOptions
{
    /// <summary>Gets or sets whether graph, node, and edge metadata annotations are emitted.</summary>
    public bool IncludeMetadata { get; set; }

    /// <summary>Gets or sets an optional diagram title comment override.</summary>
    public string? DiagramTitle { get; set; }

    /// <summary>Gets or sets the line ending used in the rendered output.</summary>
    public string NewLine { get; set; } = "\n";

    /// <summary>Gets or sets the indentation prefix used for nested Mermaid statements.</summary>
    public string Indentation { get; set; } = "  ";

    internal void Validate()
    {
        if (string.IsNullOrEmpty(NewLine)) throw new ArgumentException("NewLine must be non-empty.", nameof(NewLine));
    }
}