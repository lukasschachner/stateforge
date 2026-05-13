using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratorRunResult
{
    public GeneratorRunResult(Compilation compilation, GeneratorDriverRunResult runResult)
    {
        Compilation = compilation;
        RunResult = runResult;
    }

    public Compilation Compilation { get; }
    public GeneratorDriverRunResult RunResult { get; }
    public ImmutableArray<Diagnostic> Diagnostics => RunResult.Diagnostics.AddRange(Compilation.GetDiagnostics());

    public string GeneratedSource => string.Join("\n---\n",
        RunResult.Results.SelectMany(r => r.GeneratedSources).Select(s => s.SourceText.ToString()));

    public IReadOnlyList<string> GeneratedHintNames =>
        RunResult.Results.SelectMany(r => r.GeneratedSources).Select(s => s.HintName).ToArray();
}

public static class GeneratorTestHost
{
    public static GeneratorRunResult Run(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver.Create(new StateMachineGenerator()).WithUpdatedParseOptions(parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return new GeneratorRunResult(outputCompilation, driver.GetRunResult());
    }

    public static IEnumerable<Diagnostic> GeneratorDiagnostics(this GeneratorRunResult result, string id)
    {
        return result.RunResult.Diagnostics.Concat(result.Compilation.GetDiagnostics()).Where(d => d.Id == id);
    }

    public static void AssertCompiles(GeneratorRunResult result)
    {
        var errors = result.Compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.True(errors.Length == 0,
            string.Join("\n", errors.Select(e => e.ToString())) + "\nGenerated:\n" + result.GeneratedSource);
    }

    private static MetadataReference[] References()
    {
        var trusted = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ??
                      Array.Empty<string>();
        var references = trusted.Select(p => MetadataReference.CreateFromFile(p)).ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(StateMachineDefinition<,>).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        return references.DistinctBy(r => r.Display).ToArray();
    }
}