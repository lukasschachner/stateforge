namespace StateForge.SourceGenerators.Tests;

public sealed class PayloadEventGenerationTests
{
    [Fact]
    public void PayloadEventDeclarationGeneratesTypedOnCall()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Created, Paid }
                     public interface E { }
                     public sealed record Pay(decimal Amount) : E;
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Created)]
                     [State(S.Paid)]
                     [Event(typeof(Pay))]
                     [Transition(S.Created, typeof(Pay), S.Paid)]
                     public static partial class M { }
                     """;
        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".On<global::Pay>().GoTo(S.Paid)", result.GeneratedSource);
    }
}