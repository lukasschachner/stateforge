using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Tests;

public sealed class AdvancedDeclarationDiagnosticTests
{
    [Fact]
    public void ReportsDuplicateExplicitRegions()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [ParallelRegion(S.Operational, "Billing")]
                     [ParallelRegion(S.Operational, "Billing")]
                     [Region(S.Operational, "Billing", S.A, IsInitial = true)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG009"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.Contains("Billing", diagnostic.GetMessage());
    }

    [Fact]
    public void ReportsMissingRegionalInitial()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "Billing", S.A)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG010"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("initial", diagnostic.GetMessage(), System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportsDuplicateSiblingRegionMembership()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "One", S.A, IsInitial = true)]
                     [Region(S.Operational, "Two", S.B, IsInitial = true)]
                     [Region(S.Operational, "Two", S.A)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG011"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.DoesNotContain(System.Environment.CurrentDirectory, diagnostic.GetMessage());
    }

    [Fact]
    public void ReportsUnknownRegionOwner()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [Region(S.Operational, "Billing", S.A, IsInitial = true)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG012"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("Operational", diagnostic.GetMessage());
    }

    [Fact]
    public void ReportsUnsupportedHistoryMode()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, History = (HistoryMode)99)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG013"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("history", diagnostic.GetMessage(), System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportsInvalidTerminalRoleCombination()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Operational }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true, IsTerminal = true)]
                     public static partial class M { }
                     """;

        var diagnostic = Assert.Single(GeneratorTestHost.Run(source).GeneratorDiagnostics("SMG014"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("terminal", diagnostic.GetMessage());
    }
}
