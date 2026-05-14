using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public static class ConditionReferenceEmitter
{
    public static string Emit(ConditionReference reference)
    {
        var display = reference.DisplayName is null ? string.Empty : ", " + Literal(reference.DisplayName);
        var methodName = reference.UseAsyncBuilder ? "WhenAsync" : "When";
        return "." + methodName + "(" + reference.MemberName + display + ")";
    }

    internal static string Literal(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}