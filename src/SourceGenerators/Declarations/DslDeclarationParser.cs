using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateMachineLibrary.SourceGenerators.Diagnostics;
using StateMachineLibrary.SourceGenerators.Emission;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DslDeclarationParser
{
    public static void ParseDsl(TypeDeclarationSyntax syntax, SemanticModel semanticModel,
        MachineDeclaration declaration, DiagnosticReporter reporter, CancellationToken cancellationToken)
    {
        foreach (var method in DslDeclarationProvider.FindDeclarationMethods(syntax))
        {
            var parameter = method.ParameterList.Parameters[0];
            var parameterType =
                semanticModel.GetTypeInfo(parameter.Type!, cancellationToken).Type
                    ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
            if (!parameterType.Contains("StateMachineDeclaration<", StringComparison.Ordinal) &&
                !parameterType.Contains("StateMachineDeclaration", StringComparison.Ordinal)) continue;
            var parameterName = parameter.Identifier.ValueText;
            if (method.Body is not null)
                foreach (var statement in method.Body.Statements)
                {
                    if (DslSyntaxValidator.IsUnsupportedStatement(statement))
                    {
                        DslSyntaxValidator.ReportUnsupported(statement, reporter);
                        continue;
                    }

                    ParseStatement((ExpressionStatementSyntax)statement, parameterName, semanticModel, declaration,
                        reporter, cancellationToken);
                }
            else if (method.ExpressionBody is not null)
                ParseExpression(method.ExpressionBody.Expression, parameterName, semanticModel, declaration, reporter,
                    cancellationToken);
        }
    }

    private static void ParseStatement(ExpressionStatementSyntax statement, string parameterName,
        SemanticModel semanticModel, MachineDeclaration declaration, DiagnosticReporter reporter,
        CancellationToken cancellationToken)
    {
        ParseExpression(statement.Expression, parameterName, semanticModel, declaration, reporter, cancellationToken);
    }

    private static void ParseExpression(ExpressionSyntax expression, string parameterName, SemanticModel semanticModel,
        MachineDeclaration declaration, DiagnosticReporter reporter, CancellationToken cancellationToken)
    {
        if (!TryFlatten(expression, out var root, out var calls) || root != parameterName || calls.Count == 0)
        {
            reporter.UnsupportedDsl(expression.ToString(), expression.GetLocation());
            return;
        }

        var transitionParser = new DslTransitionParser(semanticModel, cancellationToken);
        if (calls[0].Name == "WithMetadata" && calls[0].Arguments.Count >= 2)
        {
            declaration.Metadata.Add(new MetadataEntry(
                StringConstant(semanticModel, calls[0].Arguments[0], cancellationToken),
                calls[0].Arguments[1].ToString(), calls[0].Location));
            return;
        }

        if (calls[0].Name != "State" || calls[0].Arguments.Count != 1)
        {
            reporter.UnsupportedDsl(calls[0].Name, calls[0].Location);
            return;
        }

        var stateExpr = calls[0].Arguments[0];
        var stateKey = transitionParser.Identity(stateExpr);
        var stateText = transitionParser.Text(stateExpr);
        var state = declaration.States.FirstOrDefault(s => s.IdentityKey == stateKey);
        if (state is null)
        {
            state = new DeclaredState(stateText, stateText, stateKey,
                GeneratedNameHelper.IdentifierFromExpression(stateText, "State_"), false, calls[0].Location);
            declaration.States.Add(state);
        }

        string? pendingEventKey = null;
        var conditions = new List<ConditionReference>();
        var behaviors = new List<BehaviorReference>();
        var transitionMetadata = new List<MetadataEntry>();

        for (var i = 1; i < calls.Count; i++)
        {
            var call = calls[i];
            switch (call.Name)
            {
                case "Terminal":
                    state.IsTerminal = true;
                    break;
                case "WithMetadata" when pendingEventKey is null && call.Arguments.Count >= 2:
                    state.Metadata.Add(new MetadataEntry(
                        StringConstant(semanticModel, call.Arguments[0], cancellationToken),
                        call.Arguments[1].ToString(), call.Location));
                    break;
                case "WithMetadata" when pendingEventKey is not null && call.Arguments.Count >= 2:
                    transitionMetadata.Add(new MetadataEntry(
                        StringConstant(semanticModel, call.Arguments[0], cancellationToken),
                        call.Arguments[1].ToString(), call.Location));
                    break;
                case "On" when call.TypeArguments.Count == 1:
                    var eventType =
                        semanticModel.GetTypeInfo(call.TypeArguments[0], cancellationToken).Type
                            ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ??
                        call.TypeArguments[0].ToString();
                    pendingEventKey = "type:" + eventType;
                    var payloadEvent = new DeclaredEvent(eventType, DeclaredEventKind.PayloadType, pendingEventKey,
                        GeneratedNameHelper.IdentifierFromExpression(eventType, "Event_"), null, eventType,
                        call.Location);
                    if (!declaration.Events.Any(e => e.IdentityKey == pendingEventKey))
                        declaration.Events.Add(payloadEvent);
                    Reset(ref conditions, ref behaviors, ref transitionMetadata);
                    break;
                case "On" when call.Arguments.Count == 1:
                    pendingEventKey = transitionParser.Identity(call.Arguments[0]);
                    var eventText = transitionParser.Text(call.Arguments[0]);
                    var valueEvent = new DeclaredEvent(eventText, DeclaredEventKind.Value, pendingEventKey,
                        GeneratedNameHelper.IdentifierFromExpression(eventText, "Event_"), eventText, null,
                        call.Location);
                    if (!declaration.Events.Any(e => e.IdentityKey == pendingEventKey))
                        declaration.Events.Add(valueEvent);
                    Reset(ref conditions, ref behaviors, ref transitionMetadata);
                    break;
                case "When" when pendingEventKey is not null && call.Arguments.Count >= 1:
                    conditions.Add(new ConditionReference(
                        MemberName(semanticModel, call.Arguments[0], cancellationToken),
                        call.Arguments.Count > 1
                            ? StringConstant(semanticModel, call.Arguments[1], cancellationToken)
                            : null, call.Location));
                    break;
                case "OnExit" when pendingEventKey is not null && call.Arguments.Count >= 1:
                    behaviors.Add(new BehaviorReference(MemberName(semanticModel, call.Arguments[0], cancellationToken),
                        BehaviorPhase.Exit,
                        call.Arguments.Count > 1
                            ? StringConstant(semanticModel, call.Arguments[1], cancellationToken)
                            : null, call.Location));
                    break;
                case "Execute" when pendingEventKey is not null && call.Arguments.Count >= 1:
                    behaviors.Add(new BehaviorReference(MemberName(semanticModel, call.Arguments[0], cancellationToken),
                        BehaviorPhase.Transition,
                        call.Arguments.Count > 1
                            ? StringConstant(semanticModel, call.Arguments[1], cancellationToken)
                            : null, call.Location));
                    break;
                case "OnEntry" when pendingEventKey is not null && call.Arguments.Count >= 1:
                    behaviors.Add(new BehaviorReference(MemberName(semanticModel, call.Arguments[0], cancellationToken),
                        BehaviorPhase.Entry,
                        call.Arguments.Count > 1
                            ? StringConstant(semanticModel, call.Arguments[1], cancellationToken)
                            : null, call.Location));
                    break;
                case "GoTo" when pendingEventKey is not null && call.Arguments.Count == 1:
                    Complete(declaration, transitionParser, stateKey, pendingEventKey, call.Arguments[0],
                        DeclaredTransitionKind.External, call.Location, conditions, behaviors, transitionMetadata);
                    pendingEventKey = null;
                    Reset(ref conditions, ref behaviors, ref transitionMetadata);
                    break;
                case "Self" when pendingEventKey is not null:
                    Complete(declaration, transitionParser, stateKey, pendingEventKey, stateExpr,
                        DeclaredTransitionKind.Self, call.Location, conditions, behaviors, transitionMetadata);
                    pendingEventKey = null;
                    Reset(ref conditions, ref behaviors, ref transitionMetadata);
                    break;
                case "Internal" when pendingEventKey is not null:
                    Complete(declaration, transitionParser, stateKey, pendingEventKey, stateExpr,
                        DeclaredTransitionKind.Internal, call.Location, conditions, behaviors, transitionMetadata);
                    pendingEventKey = null;
                    Reset(ref conditions, ref behaviors, ref transitionMetadata);
                    break;
                default:
                    reporter.UnsupportedDsl(call.Name, call.Location);
                    break;
            }
        }
    }

    private static void Complete(MachineDeclaration declaration, DslTransitionParser parser, string sourceKey,
        string eventKey, ExpressionSyntax target, DeclaredTransitionKind kind, Location? location,
        IEnumerable<ConditionReference> conditions, IEnumerable<BehaviorReference> behaviors,
        IEnumerable<MetadataEntry> metadata)
    {
        var targetKey = parser.Identity(target);
        if (!declaration.States.Any(s => s.IdentityKey == targetKey))
        {
            var text = parser.Text(target);
            declaration.States.Add(new DeclaredState(text, text, targetKey,
                GeneratedNameHelper.IdentifierFromExpression(text, "State_"), false, location));
        }

        var transition = parser.CreateTransition(sourceKey, eventKey, targetKey, parser.Text(target), kind, location);
        transition.Conditions.AddRange(conditions);
        transition.Behaviors.AddRange(behaviors);
        transition.Metadata.AddRange(metadata);
        declaration.Transitions.Add(transition);
    }

    private static void Reset(ref List<ConditionReference> conditions, ref List<BehaviorReference> behaviors,
        ref List<MetadataEntry> metadata)
    {
        conditions = new List<ConditionReference>();
        behaviors = new List<BehaviorReference>();
        metadata = new List<MetadataEntry>();
    }

    private static bool TryFlatten(ExpressionSyntax expression, out string root, out List<Call> calls)
    {
        calls = new List<Call>();
        if (expression is IdentifierNameSyntax id)
        {
            root = id.Identifier.ValueText;
            return true;
        }

        if (expression is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax member)
        {
            if (!TryFlatten(member.Expression, out root, out calls)) return false;
            var typeArgs = member.Name is GenericNameSyntax generic
                ? generic.TypeArgumentList.Arguments.ToArray()
                : Array.Empty<TypeSyntax>();
            calls.Add(new Call(member.Name.Identifier.ValueText,
                invocation.ArgumentList.Arguments.Select(a => a.Expression).ToArray(), typeArgs,
                invocation.GetLocation()));
            return true;
        }

        root = string.Empty;
        return false;
    }

    private static string StringConstant(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        var constant = semanticModel.GetConstantValue(expression, cancellationToken);
        return constant.HasValue ? constant.Value?.ToString() ?? string.Empty : expression.ToString().Trim('"');
    }

    private static string MemberName(SemanticModel semanticModel, ExpressionSyntax expression,
        CancellationToken cancellationToken)
    {
        return StringConstant(semanticModel, expression, cancellationToken);
    }

    private sealed class Call
    {
        public Call(string name, IReadOnlyList<ExpressionSyntax> arguments, IReadOnlyList<TypeSyntax> typeArguments,
            Location? location)
        {
            Name = name;
            Arguments = arguments;
            TypeArguments = typeArguments;
            Location = location;
        }

        public string Name { get; }
        public IReadOnlyList<ExpressionSyntax> Arguments { get; }
        public IReadOnlyList<TypeSyntax> TypeArguments { get; }
        public Location? Location { get; }
    }
}