using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Tests;

public sealed class ParallelRegionDiagnosticTests
{
    [Fact]
    public void ReportsInvalidParallelRegionDeclarationsWithStableDiagnostics()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [ParallelRegion(S.Operational, "Billing")]
                     [ParallelRegion(S.Operational, "Billing")]
                     [Region(S.Operational, "Billing", S.A)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);
        Assert.Contains(result.GeneratorDiagnostics("SMG009"), d => d.Severity == DiagnosticSeverity.Error);
        Assert.Contains(result.GeneratorDiagnostics("SMG010"), d => d.GetMessage().Contains("Billing"));
    }

    [Fact]
    public void ReportsUnknownParallelRegionOwner()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational)]
                     [Region(S.Operational, "Billing", S.A, IsInitial = true)]
                     public static partial class M { }
                     """;

        GeneratorDiagnosticAssertions.AssertDiagnostic(
            GeneratorTestHost.Run(source), "SMG012", DiagnosticSeverity.Error, "parallel composite");
    }
}
