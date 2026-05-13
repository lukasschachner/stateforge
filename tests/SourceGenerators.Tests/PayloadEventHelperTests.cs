namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class PayloadEventHelperTests
{
    [Fact]
    public void PayloadEventsHaveDeterministicSkippedHelperMetadata()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public abstract record Ev;
                     public sealed record Pay : Ev;
                     public enum S { Created, Paid }
                     [StateMachine(typeof(S), typeof(Ev))]
                     [State(S.Created)]
                     [State(S.Paid, IsTerminal = true)]
                     [Event(typeof(Pay))]
                     [Transition(S.Created, typeof(Pay), S.Paid)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
        Assert.DoesNotContain("ApplyPayAsync", result.GeneratedSource);
        Assert.Contains("reason=UnsupportedEventShape", result.GeneratedSource);
    }
}
