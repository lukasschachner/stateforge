using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DslDeclarationProvider
{
    public static IEnumerable<MethodDeclarationSyntax> FindDeclarationMethods(TypeDeclarationSyntax type)
    {
        return type.Members.OfType<MethodDeclarationSyntax>().Where(m =>
            m.ParameterList.Parameters.Count == 1 && (m.Body is not null || m.ExpressionBody is not null));
    }
}