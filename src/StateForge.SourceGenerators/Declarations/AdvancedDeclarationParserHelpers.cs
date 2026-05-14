using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateForge.SourceGenerators.Declarations;

internal static class AdvancedDeclarationParserHelpers
{
    public static AttributeArgumentSyntax? NamedArgument(IEnumerable<AttributeArgumentSyntax> arguments, string name)
    {
        return arguments.FirstOrDefault(a =>
            string.Equals(a.NameEquals?.Name.Identifier.ValueText, name, StringComparison.Ordinal));
    }

    public static string StringConstant(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constant.HasValue) return constant.Value?.ToString() ?? string.Empty;

        return StringExpressionFallback(expression);
    }

    public static bool BoolConstant(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        if (!constant.HasValue || constant.Value is not bool value) return false;
        return value;
    }

    private static string StringExpressionFallback(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal && literal.Token.Value is string literalValue)
            return literalValue;

        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.AddExpression))
            return StringExpressionFallback(binary.Left) + StringExpressionFallback(binary.Right);

        if (expression is InterpolatedStringExpressionSyntax interpolated)
            return string.Concat(interpolated.Contents.Select(content =>
                content is InterpolatedStringTextSyntax text ? text.TextToken.ValueText : string.Empty));

        return string.Empty;
    }

    public static DeclaredHistoryMode ParseHistoryMode(ExpressionSyntax expression)
    {
        var text = expression.ToString();
        if (text.EndsWith("Shallow", StringComparison.Ordinal)) return DeclaredHistoryMode.Shallow;
        if (text.EndsWith("Deep", StringComparison.Ordinal)) return DeclaredHistoryMode.Deep;
        if (text.EndsWith("None", StringComparison.Ordinal)) return DeclaredHistoryMode.None;
        return DeclaredHistoryMode.Unsupported;
    }

    public static string CoreHistoryMode(DeclaredHistoryMode mode)
    {
        // AdvancedDeclarationValidator reports unsupported history before emission; keep this invariant explicit here.
        if (mode == DeclaredHistoryMode.Unsupported)
            throw new InvalidOperationException("Unsupported history modes must be diagnosed before emission.");

        return mode == DeclaredHistoryMode.Deep
            ? "global::StateForge.Core.Definitions.HistoryMode.Deep"
            : "global::StateForge.Core.Definitions.HistoryMode.Shallow";
    }
}
