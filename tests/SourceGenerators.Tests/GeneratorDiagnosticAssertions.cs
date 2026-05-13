using System;
using Microsoft.CodeAnalysis;
using Xunit;

namespace StateMachineLibrary.SourceGenerators.Tests;

internal static class GeneratorDiagnosticAssertions
{
    /// <summary>Asserts that a single diagnostic with the expected ID, severity, location, and message exists.</summary>
    /// <param name="result">The generator run result to inspect.</param>
    /// <param name="id">The expected diagnostic ID.</param>
    /// <param name="severity">The expected diagnostic severity.</param>
    /// <param name="messageFragment">A deterministic message fragment expected in the diagnostic text.</param>
    /// <returns>The matching diagnostic.</returns>
    public static Diagnostic AssertDiagnostic(GeneratorRunResult result, string id, DiagnosticSeverity severity,
        string messageFragment)
    {
        var diagnostic = Assert.Single(result.GeneratorDiagnostics(id));
        Assert.Equal(severity, diagnostic.Severity);
        Assert.Contains(messageFragment, diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.NotEqual(Location.None, diagnostic.Location);
        return diagnostic;
    }

    /// <summary>Asserts that no diagnostics with the specified ID exist in the generator run result.</summary>
    /// <param name="result">The generator run result to inspect.</param>
    /// <param name="id">The diagnostic ID that should not be present.</param>
    public static void AssertNoDiagnostic(GeneratorRunResult result, string id)
    {
        Assert.Empty(result.GeneratorDiagnostics(id));
    }
}
