using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateMachineLibrary.SourceGenerators.Emission;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public sealed class DslTransitionParser
{
    private readonly CancellationToken _cancellationToken;
    private readonly SemanticModel _semanticModel;

    public DslTransitionParser(SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        _semanticModel = semanticModel;
        _cancellationToken = cancellationToken;
    }

    public DeclaredTransition CreateTransition(string sourceKey, string eventKey, string targetKey,
        string targetExpression, DeclaredTransitionKind kind, Location? location)
    {
        return new DeclaredTransition(GeneratedNameHelper.ShortHash(sourceKey + eventKey + targetKey + kind), sourceKey,
            eventKey, targetKey, targetExpression, kind, location);
    }

    public string Identity(ExpressionSyntax expression)
    {
        return SyntaxValue.IdentityForExpression(_semanticModel, expression, _cancellationToken);
    }

    public string Text(ExpressionSyntax expression)
    {
        return SyntaxValue.ExpressionText(expression);
    }
}