using System.Text;
using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public static class GeneratedMetadataEmitter
{
    public static string Emit(MachineDeclaration declaration, string indent)
    {
        var metadata = declaration.GeneratedMetadata ?? GeneratedMetadataBuilder.Build(declaration);
        var lines = new List<string>
        {
            "machine:" + metadata.MachineId,
            "style:" + declaration.DeclarationStyle
        };
        lines.AddRange(metadata.States.Select(s => "state:" + s.DisplayName + ":terminal=" + s.IsTerminal.ToString().ToLowerInvariant()));
        lines.AddRange(metadata.Events.Select(e => "event:" + e.DisplayName + ":kind=" + e.EventKind + ":helper=" + e.HelperAvailability + ":reason=" + e.SkippedReason));
        lines.AddRange(metadata.Transitions.Select(t => "transition:" + t.SourceStateKey + ":" + t.EventKey + ":" + t.TargetStateKey + ":" + t.TransitionKind));

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append(indent).AppendLine("public static global::System.Collections.Generic.IReadOnlyList<string> GeneratedMetadata { get; } = new string[]");
        EmitStringArray(sb, indent, lines);
        sb.AppendLine(";");
        return sb.ToString();
    }

    internal static void EmitStringArray(StringBuilder sb, string indent, IReadOnlyList<string> lines)
    {
        sb.Append(indent).AppendLine("{");
        foreach (var line in lines.OrderBy(l => l, StringComparer.Ordinal))
            sb.Append(indent).Append("    ").Append(ConditionReferenceEmitter.Literal(line)).AppendLine(",");
        sb.Append(indent).Append("}");
    }
}
