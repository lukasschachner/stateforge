using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using StateMachineLibrary.SourceGenerators.Declarations;

namespace StateMachineLibrary.SourceGenerators.Emission;

public static class GeneratedNameHelper
{
    public static string IdentifierFromExpression(string expression, string prefix)
    {
        var builder = new StringBuilder(prefix);
        foreach (var ch in expression)
            if (char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (builder.Length == 0 || builder[builder.Length - 1] != '_') builder.Append('_');
        if (builder.Length == prefix.Length) builder.Append(ShortHash(expression));
        return builder.ToString().TrimEnd('_');
    }

    public static string EventHelperName(DeclaredEvent declaredEvent)
    {
        if (declaredEvent is null) throw new ArgumentNullException(nameof(declaredEvent));
        var identifier = declaredEvent.GeneratedIdentifier ?? string.Empty;
        const string prefix = "Event_";
        if (!string.IsNullOrWhiteSpace(identifier) && identifier.StartsWith(prefix, StringComparison.Ordinal))
            identifier = identifier.Substring(prefix.Length);
        if (string.IsNullOrWhiteSpace(identifier)) identifier = ShortHash(declaredEvent.IdentityKey);
        return "Apply" + identifier + "Async";
    }

    public static string SourceName(MachineDeclaration declaration)
    {
        var name = declaration.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", string.Empty);
        return SanitizeFileName(name) + ".StateMachine.g.cs";
    }

    public static string ShortHash(string input)
    {
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash, 0, 4).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    private static string SanitizeFileName(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value) builder.Append(char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' ? ch : '_');
        return builder.ToString();
    }
}