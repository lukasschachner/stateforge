using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using StateMachineLibrary.SourceGenerators.Declarations;
using StateMachineLibrary.SourceGenerators.Diagnostics;
using StateMachineLibrary.SourceGenerators.Emission;

namespace StateMachineLibrary.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class StateMachineGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postContext =>
            postContext.AddSource(DeclarationApiSource.HintName,
                SourceText.From(DeclarationApiSource.Source, Encoding.UTF8)));

        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(AttributeDeclarationProvider.IsCandidate, AttributeDeclarationProvider.Transform)
            .Where(static t => t is not null)
            .Select(static (t, _) => t!)
            .Collect();

        context.RegisterSourceOutput(context.CompilationProvider.Combine(candidates),
            static (productionContext, source) =>
            {
                var (compilation, types) = source;
                foreach (var type in types.Distinct()) ProcessType(productionContext, compilation, type);
            });
    }

    private static void ProcessType(SourceProductionContext context, Compilation compilation,
        TypeDeclarationSyntax type)
    {
        var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);
        var reporter = new DiagnosticReporter();
        var declaration = AttributeDeclarationParser.Parse(type, semanticModel, context.CancellationToken);
        if (declaration is null) return;

        if (declaration.States.Count == 0 && declaration.Transitions.Count == 0 &&
            DslDeclarationProvider.FindDeclarationMethods(type).Any())
            declaration = new MachineDeclaration(declaration.DeclarationId, declaration.ContainingType,
                declaration.StateTypeName, declaration.EventTypeName, DeclarationStyle.CompactDsl,
                declaration.SourceLocation);

        DslDeclarationParser.ParseDsl(type, semanticModel, declaration, reporter, context.CancellationToken);

        DeclarationNormalizer.Normalize(declaration);
        DeclarationValidator.Validate(declaration, reporter);
        reporter.ReportTo(context);
        if (reporter.HasErrors) return;

        var source = DefinitionSourceEmitter.Emit(declaration);
        context.AddSource(GeneratedNameHelper.SourceName(declaration), SourceText.From(source, Encoding.UTF8));
    }
}