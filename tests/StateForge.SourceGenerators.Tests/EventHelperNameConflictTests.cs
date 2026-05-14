namespace StateForge.SourceGenerators.Tests;

public sealed class EventHelperNameConflictTests
{
    [Fact]
    public void HelperNameConflictsSkipHelperAndRecordReason()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B, IsTerminal = true)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M
                     {
                         public static object? ApplyE_GoAsync => null;
                     }
                     """;

        var result = GeneratorTestHost.Run(source);
        Assert.DoesNotContain("runtime.ApplyAsync(E.Go", result.GeneratedSource);
        Assert.Contains("reason=GeneratedNameConflict", result.GeneratedSource);
    }
}
