using System.Text;
using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public static class GeneratedGraphEmitter
{
    public static string Emit(MachineDeclaration declaration, string indent)
    {
        var graph = GeneratedGraphMetadataBuilder.Build(declaration);
        var nodeNames = graph.Nodes.ToDictionary(n => n.StateKey, n => n.DisplayName, StringComparer.Ordinal);
        var lines = new List<string>();
        lines.AddRange(graph.Nodes.Select(n => "node:" + n.DisplayName + ":terminal=" + n.IsTerminal.ToString().ToLowerInvariant()));
        lines.AddRange(graph.Edges.Select(e => "edge:" + Display(nodeNames, e.SourceStateKey) + ":" + e.Kind + ":" + Display(nodeNames, e.TargetStateKey)));

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append(indent).AppendLine("public static global::System.Collections.Generic.IReadOnlyList<string> GeneratedGraph { get; } = new string[]");
        GeneratedMetadataEmitter.EmitStringArray(sb, indent, lines);
        sb.AppendLine(";");
        return sb.ToString();
    }

    private static string Display(IReadOnlyDictionary<string, string> nodeNames, string key)
    {
        return nodeNames.TryGetValue(key, out var name) ? name : key;
    }
}
