using Microsoft.CodeAnalysis;
using StateMachineLibrary.SourceGenerators.Diagnostics;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class MemberReferenceResolver
{
    private const string CancellationTokenTypeName = "global::System.Threading.CancellationToken";
    private const string ValueTaskTypeName = "global::System.Threading.Tasks.ValueTask";
    private const string BooleanValueTaskTypeName = "global::System.Threading.Tasks.ValueTask<bool>";

    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        foreach (var transition in declaration.Transitions)
        {
            foreach (var condition in transition.Conditions)
            {
                var signature = ResolveSignature(declaration, condition.MemberName, true);
                condition.SignatureStatus = signature.Status;
                condition.UseAsyncBuilder = signature.UseAsyncBuilder;
                if (signature.Status != ReferenceSignatureStatus.Compatible)
                    reporter.InvalidMember(condition.MemberName, "condition", condition.SourceLocation);
            }

            foreach (var behavior in transition.Behaviors)
            {
                var signature = ResolveSignature(declaration, behavior.MemberName, false);
                behavior.SignatureStatus = signature.Status;
                behavior.UseAsyncBuilder = signature.UseAsyncBuilder;
                if (signature.Status != ReferenceSignatureStatus.Compatible)
                    reporter.InvalidMember(behavior.MemberName, "behavior", behavior.SourceLocation);
            }
        }
    }

    private static SignatureResolution ResolveSignature(MachineDeclaration declaration, string memberName,
        bool expectsCondition)
    {
        var methods = declaration.ContainingType.GetMembers(memberName).OfType<IMethodSymbol>().ToArray();
        if (methods.Length == 0) return new SignatureResolution(ReferenceSignatureStatus.Unresolved, false);

        foreach (var method in methods)
        {
            if (!method.IsStatic) continue;

            if (IsSynchronousSignature(method, declaration, expectsCondition))
                return new SignatureResolution(ReferenceSignatureStatus.Compatible, false);

            if (IsAsynchronousSignature(method, declaration, expectsCondition))
                return new SignatureResolution(ReferenceSignatureStatus.Compatible, true);
        }

        return new SignatureResolution(ReferenceSignatureStatus.Incompatible, false);
    }

    private static bool IsSynchronousSignature(IMethodSymbol method, MachineDeclaration declaration,
        bool expectsCondition)
    {
        if (method.Parameters.Length != 1 || !IsContext(method.Parameters[0].Type, declaration)) return false;

        return expectsCondition
            ? method.ReturnType.SpecialType == SpecialType.System_Boolean
            : method.ReturnsVoid;
    }

    private static bool IsAsynchronousSignature(IMethodSymbol method, MachineDeclaration declaration,
        bool expectsCondition)
    {
        if (method.Parameters.Length != 2 ||
            !IsContext(method.Parameters[0].Type, declaration) ||
            method.Parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) !=
            CancellationTokenTypeName)
            return false;

        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return expectsCondition ? returnType == BooleanValueTaskTypeName : returnType == ValueTaskTypeName;
    }

    private static bool IsContext(ITypeSymbol type, MachineDeclaration declaration)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
               "global::StateMachineLibrary.Core.Execution.TransitionContext<" + declaration.StateTypeName + ", " +
               declaration.EventTypeName + ">";
    }

    private readonly struct SignatureResolution
    {
        public SignatureResolution(ReferenceSignatureStatus status, bool useAsyncBuilder)
        {
            Status = status;
            UseAsyncBuilder = useAsyncBuilder;
        }

        public ReferenceSignatureStatus Status { get; }
        public bool UseAsyncBuilder { get; }
    }
}