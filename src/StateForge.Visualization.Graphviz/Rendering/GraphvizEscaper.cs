using System.Text;

namespace StateForge.Visualization.Graphviz.Rendering;

internal static class GraphvizEscaper
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
            .Replace("\r\n", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\n", StringComparison.Ordinal);
    }

    public static string QuoteIdentifier(string value)
    {
        return $"\"{EscapeLabel(value)}\"";
    }

    public static string EscapeComment(string value)
    {
        return value
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);
    }
}