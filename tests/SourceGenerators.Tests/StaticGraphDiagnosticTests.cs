using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class StaticGraphDiagnosticTests
{
    [Fact]
    public void ReportsUnreachableDeclaredStates()
    {
        var source = ValidationDiagnosticTestSources.Machine("""
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [State(S.C)]
                     [State(S.Done, IsTerminal = true)]
                     [Event(E.Go)]
                     [Event(E.Finish)]
                     [Transition(S.A, E.Go, S.B)]
                     [Transition(S.B, E.Finish, S.Done)]
                     public static partial class M { }
                     """);

        var diagnostic = GeneratorDiagnosticAssertions.AssertDiagnostic(
            GeneratorTestHost.Run(source), "SMG016", DiagnosticSeverity.Error, "unreachable");
        Assert.Contains("S.C", diagnostic.GetMessage());
    }

    [Fact]
    public void ReportsReachableDeadEndWhenTerminalExists()
    {
        var source = ValidationDiagnosticTestSources.Machine("""
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [State(S.Done, IsTerminal = true)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """);

        var diagnostic = GeneratorDiagnosticAssertions.AssertDiagnostic(
            GeneratorTestHost.Run(source), "SMG017", DiagnosticSeverity.Error, "non-terminal");
        Assert.Contains("S.B", diagnostic.GetMessage());
    }

    [Fact]
    public void ReportsTerminalReachabilityWhenNoTerminalCanBeReached()
    {
        var source = ValidationDiagnosticTestSources.Machine("""
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [State(S.Done, IsTerminal = true)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """);

        var diagnostic = GeneratorDiagnosticAssertions.AssertDiagnostic(
            GeneratorTestHost.Run(source), "SMG018", DiagnosticSeverity.Error, "terminal");
        Assert.Contains("S.Done", diagnostic.GetMessage());
    }

    [Fact]
    public void DoesNotReportDeadEndForLegacyMachinesWithoutTerminalStates()
    {
        var source = ValidationDiagnosticTestSources.Machine("""
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """);

        var result = GeneratorTestHost.Run(source);
        GeneratorDiagnosticAssertions.AssertNoDiagnostic(result, "SMG017");
        GeneratorDiagnosticAssertions.AssertNoDiagnostic(result, "SMG018");
        GeneratorTestHost.AssertCompiles(result);
    }
}
