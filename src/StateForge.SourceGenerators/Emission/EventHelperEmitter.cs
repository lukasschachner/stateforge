using System.Text;
using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public static class EventHelperEmitter
{
    public static string Emit(MachineDeclaration declaration, string indent)
    {
        var sb = new StringBuilder();
        var runtimeType = "global::StateForge.Core.Execution.StateMachineRuntime<" + declaration.StateTypeName + ", " + declaration.EventTypeName + ">";
        var outcomeType = "global::System.Threading.Tasks.ValueTask<global::StateForge.Core.Execution.TransitionOutcome<" + declaration.StateTypeName + ", " + declaration.EventTypeName + ">>";
        var eventsByKey = declaration.Events.ToDictionary(e => e.IdentityKey, StringComparer.Ordinal);
        foreach (var helper in declaration.GeneratedHelpers.Where(h => h.Availability == GeneratedHelperAvailability.Generated))
        {
            if (!eventsByKey.TryGetValue(helper.EventKey, out var declaredEvent) || declaredEvent.ValueExpression is null)
                continue;
            sb.AppendLine();
            sb.Append(indent).Append("public static ").Append(outcomeType).Append(' ').Append(helper.HelperName)
                .Append('(').Append(runtimeType).Append(" runtime, global::System.Threading.CancellationToken cancellationToken = default)").AppendLine();
            sb.Append(indent).AppendLine("{");
            sb.Append(indent).Append("    return runtime.ApplyAsync(").Append(declaredEvent.ValueExpression).AppendLine(", cancellationToken);");
            sb.Append(indent).AppendLine("}");
            sb.AppendLine();
            sb.Append(indent).Append("public static ").Append(outcomeType).Append(' ').Append(helper.HelperName)
                .Append('(').Append(declaration.StateTypeName).Append(" currentState, global::System.Threading.CancellationToken cancellationToken = default)").AppendLine();
            sb.Append(indent).AppendLine("{");
            sb.Append(indent).Append("    return Definition.ApplyAsync(currentState, ").Append(declaredEvent.ValueExpression).AppendLine(", cancellationToken);");
            sb.Append(indent).AppendLine("}");
        }

        return sb.ToString();
    }
}
