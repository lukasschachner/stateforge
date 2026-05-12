using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateMachineLibrary.SourceGenerators.Emission;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class AttributeDeclarationParser
{
    public static MachineDeclaration? Parse(TypeDeclarationSyntax syntax, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var typeSymbol = semanticModel.GetDeclaredSymbol(syntax, cancellationToken) as INamedTypeSymbol;
        if (typeSymbol is null) return null;
        AttributeSyntax? machineAttribute = null;
        foreach (var attribute in AllAttributes(syntax))
            if (Is(attribute, "StateMachine", semanticModel, cancellationToken))
            {
                machineAttribute = attribute;
                break;
            }

        if (machineAttribute?.ArgumentList is null || machineAttribute.ArgumentList.Arguments.Count < 2) return null;
        if (machineAttribute.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax stateTypeOf ||
            machineAttribute.ArgumentList.Arguments[1].Expression is not TypeOfExpressionSyntax eventTypeOf)
            return null;

        var stateType = SyntaxValue.TypeNameFromTypeOf(stateTypeOf, semanticModel, cancellationToken);
        var eventType = SyntaxValue.TypeNameFromTypeOf(eventTypeOf, semanticModel, cancellationToken);
        var declaration = new MachineDeclaration(
            new DeclarationIdentity(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                typeSymbol.Name),
            typeSymbol,
            stateType,
            eventType,
            DeclarationStyle.Attribute,
            machineAttribute.GetLocation());

        foreach (var attribute in AllAttributes(syntax))
            if (Is(attribute, "State", semanticModel, cancellationToken))
                ParseState(attribute, semanticModel, declaration, cancellationToken);
            else if (Is(attribute, "Event", semanticModel, cancellationToken))
                ParseEvent(attribute, semanticModel, declaration, cancellationToken);
            else if (Is(attribute, "Transition", semanticModel, cancellationToken))
                ParseTransition(attribute, semanticModel, declaration, cancellationToken);
            else if (Is(attribute, "Metadata", semanticModel, cancellationToken))
                ParseMetadata(attribute, semanticModel, declaration, cancellationToken);

        return declaration;
    }

    private static IEnumerable<AttributeSyntax> AllAttributes(TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists.SelectMany(l => l.Attributes);
    }

    private static bool Is(AttributeSyntax attribute, string name, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
        var attributeType = symbol?.ContainingType;
        return attributeType?.Name == name + "Attribute" &&
               attributeType.ContainingNamespace.ToDisplayString() == "StateMachineLibrary.SourceGeneration";
    }

    private static void ParseState(AttributeSyntax attribute, SemanticModel semanticModel,
        MachineDeclaration declaration, CancellationToken cancellationToken)
    {
        var args = attribute.ArgumentList?.Arguments;
        if (args is null || args.Value.Count == 0) return;
        var expr = args.Value[0].Expression;
        var expression = SyntaxValue.ExpressionText(expr);
        var key = SyntaxValue.IdentityForExpression(semanticModel, expr, cancellationToken);
        var terminal = args.Value.Any(a =>
            a.NameEquals?.Name.Identifier.ValueText == "IsTerminal" &&
            semanticModel.GetConstantValue(a.Expression, cancellationToken).Value is true);
        declaration.States.Add(new DeclaredState(expression, expression, key,
            GeneratedNameHelper.IdentifierFromExpression(expression, "State_"), terminal, attribute.GetLocation()));
    }

    private static void ParseEvent(AttributeSyntax attribute, SemanticModel semanticModel,
        MachineDeclaration declaration, CancellationToken cancellationToken)
    {
        var args = attribute.ArgumentList?.Arguments;
        if (args is null || args.Value.Count == 0) return;
        var expr = args.Value[0].Expression;
        if (expr is TypeOfExpressionSyntax typeOf)
        {
            var typeName = SyntaxValue.TypeNameFromTypeOf(typeOf, semanticModel, cancellationToken);
            declaration.Events.Add(new DeclaredEvent(typeName, DeclaredEventKind.PayloadType,
                SyntaxValue.TypeIdentityFromTypeOf(typeOf, semanticModel, cancellationToken),
                GeneratedNameHelper.IdentifierFromExpression(typeName, "Event_"), null, typeName,
                attribute.GetLocation()));
        }
        else
        {
            var expression = SyntaxValue.ExpressionText(expr);
            declaration.Events.Add(new DeclaredEvent(expression, DeclaredEventKind.Value,
                SyntaxValue.IdentityForExpression(semanticModel, expr, cancellationToken),
                GeneratedNameHelper.IdentifierFromExpression(expression, "Event_"), expression, null,
                attribute.GetLocation()));
        }
    }

    private static void ParseTransition(AttributeSyntax attribute, SemanticModel semanticModel,
        MachineDeclaration declaration, CancellationToken cancellationToken)
    {
        var args = attribute.ArgumentList?.Arguments;
        if (args is null || args.Value.Count < 3) return;
        var source = args.Value[0].Expression;
        var ev = args.Value[1].Expression;
        var target = args.Value[2].Expression;
        var sourceKey = SyntaxValue.IdentityForExpression(semanticModel, source, cancellationToken);
        var eventKey = ev is TypeOfExpressionSyntax evType
            ? SyntaxValue.TypeIdentityFromTypeOf(evType, semanticModel, cancellationToken)
            : SyntaxValue.IdentityForExpression(semanticModel, ev, cancellationToken);
        var targetKey = SyntaxValue.IdentityForExpression(semanticModel, target, cancellationToken);
        var kind = DeclaredTransitionKind.External;
        var kindArg = args.Value.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == "Kind");
        if (kindArg is not null)
        {
            var kindText = kindArg.Expression.ToString();
            if (kindText.EndsWith("Self", StringComparison.Ordinal)) kind = DeclaredTransitionKind.Self;
            else if (kindText.EndsWith("Internal", StringComparison.Ordinal)) kind = DeclaredTransitionKind.Internal;
        }

        var transition = new DeclaredTransition(GeneratedNameHelper.ShortHash(sourceKey + eventKey + targetKey),
            sourceKey, eventKey, targetKey, SyntaxValue.ExpressionText(target), kind, attribute.GetLocation());
        foreach (var named in args.Value.Where(a => a.NameEquals is not null))
        {
            var name = named.NameEquals!.Name.Identifier.ValueText;
            if (name == "Condition")
                transition.Conditions.Add(new ConditionReference(
                    StringConstant(semanticModel, named.Expression, cancellationToken), null, named.GetLocation()));
            if (name == "Behavior")
                transition.Behaviors.Add(new BehaviorReference(
                    StringConstant(semanticModel, named.Expression, cancellationToken), BehaviorPhase.Transition, null,
                    named.GetLocation()));
        }

        declaration.Transitions.Add(transition);
    }

    private static void ParseMetadata(AttributeSyntax attribute, SemanticModel semanticModel,
        MachineDeclaration declaration, CancellationToken cancellationToken)
    {
        var args = attribute.ArgumentList?.Arguments;
        if (args is null || args.Value.Count < 2) return;
        declaration.Metadata.Add(new MetadataEntry(
            StringConstant(semanticModel, args.Value[0].Expression, cancellationToken),
            args.Value[1].Expression.ToString(), attribute.GetLocation()));
    }

    private static string StringConstant(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        return constant.HasValue ? constant.Value?.ToString() ?? string.Empty : expression.ToString().Trim('"');
    }
}