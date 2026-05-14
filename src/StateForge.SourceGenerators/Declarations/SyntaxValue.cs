using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateForge.SourceGenerators.Declarations;

internal static class SyntaxValue
{
    public static string ExpressionText(ExpressionSyntax expression)
    {
        return expression.ToString();
    }

    public static string IdentityForExpression(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var type = semanticModel.GetTypeInfo(expression, cancellationToken).Type
            ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "?";
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constant.HasValue) return type + ":" + (constant.Value?.ToString() ?? "<null>");
        var symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
        if (symbol is not null) return type + ":" + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return type + ":" + expression;
    }

    public static string StringArgument(SemanticModel semanticModel, AttributeArgumentSyntax arg,
        CancellationToken cancellationToken)
    {
        var constant = semanticModel.GetConstantValue(arg.Expression, cancellationToken);
        return constant.HasValue ? constant.Value?.ToString() ?? string.Empty : arg.Expression.ToString().Trim('"');
    }

    public static string TypeNameFromTypeOf(TypeOfExpressionSyntax typeOf, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var type = semanticModel.GetTypeInfo(typeOf.Type, cancellationToken).Type;
        return type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? typeOf.Type.ToString();
    }

    public static string TypeIdentityFromTypeOf(TypeOfExpressionSyntax typeOf, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return "type:" + TypeNameFromTypeOf(typeOf, semanticModel, cancellationToken);
    }
}