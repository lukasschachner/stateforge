using System.Text;

namespace StateForge.Visualization.Mermaid.Rendering;

internal static class MermaidEscaper
{
    public static string EncodeIdentifier(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        return "n_" + Convert.ToHexString(Encoding.UTF8.GetBytes(nodeId));
    }

    public static string EscapeLabel(string value)
    {
        if (string.IsNullOrEmpty(value)) return "<empty>";

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r\n", "<br/>", StringComparison.Ordinal)
            .Replace("\n", "<br/>", StringComparison.Ordinal)
            .Replace("\r", "<br/>", StringComparison.Ordinal);
    }

    public static string EscapeComment(string value)
    {
        return value
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);
    }
}