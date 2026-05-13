namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class StaticAnalysisBoundaryTests
{
    [Fact]
    public void GuardedDuplicateTransitions_NotReportedAsStaticAmbiguity()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B, C }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [State(S.C)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B, Condition = nameof(CanGo))]
                     [Transition(S.A, E.Go, S.C, Condition = nameof(CanStop))]
                     public static partial class M
                     {
                         public static bool CanGo() => true;
                         public static bool CanStop() => false;
                     }
                     """;

        var result = GeneratorTestHost.Run(source);
        GeneratorDiagnosticAssertions.AssertNoDiagnostic(result, "SMG003");
        GeneratorTestHost.AssertCompiles(result);
    }
}
