using System.Text;

namespace StateMachineLibrary.SourceGenerators.Emission;

public static class DefinitionMemberEmitter
{
    public static string EmitMembers(string stateTypeName, string eventTypeName, string createBody, string indent)
    {
        var definitionType = "global::StateMachineLibrary.Core.Definitions.StateMachineDefinition<" + stateTypeName +
                             ", " + eventTypeName + ">";
        var sb = new StringBuilder();

        sb.Append(indent).Append("private static readonly global::System.Lazy<").Append(definitionType)
            .Append("> __generatedDefinition = new global::System.Lazy<").Append(definitionType)
            .AppendLine(">(() => CreateDefinition());");
        sb.AppendLine();
        sb.Append(indent).Append("public static ").Append(definitionType)
            .AppendLine(" Definition => __generatedDefinition.Value;");
        sb.AppendLine();
        sb.Append(indent).Append("public static ").Append(definitionType).AppendLine(" CreateDefinition()");
        sb.Append(indent).AppendLine("{");
        foreach (var line in createBody.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            if (line.Length == 0) continue;

            sb.Append(indent).Append("    ").AppendLine(line);
        }

        sb.Append(indent).AppendLine("}");
        return sb.ToString();
    }
}