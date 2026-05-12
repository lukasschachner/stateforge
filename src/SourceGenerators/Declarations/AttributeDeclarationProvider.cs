using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class AttributeDeclarationProvider
{
    public static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is TypeDeclarationSyntax type && type.Modifiers.Any(m => m.ValueText == "partial") &&
               type.AttributeLists.Count > 0;
    }

    public static TypeDeclarationSyntax? Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var type = (TypeDeclarationSyntax)context.Node;
        foreach (var list in type.AttributeLists)
            foreach (var attribute in list.Attributes)
            {
                var name = attribute.Name.ToString();
                if (name.EndsWith("StateMachine", StringComparison.Ordinal) ||
                    name.EndsWith("StateMachineAttribute", StringComparison.Ordinal)) return type;
            }

        return null;
    }
}