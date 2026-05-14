namespace StateForge.SourceGenerators.Tests;

public sealed class AttributeParallelRegionGenerationTests
{
    [Fact]
    public void AttributeDeclarationsEmitParallelRegionBuilderCallsInDeclarationOrder()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Draft, Operational, Pick, PickDone, Pay, PayDone }
                     public enum E { Start, Picked, Paid }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Draft)]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "Fulfillment", S.Pick, IsInitial = true)]
                     [Region(S.Operational, "Fulfillment", S.PickDone, IsTerminal = true)]
                     [Region(S.Operational, "Billing", S.Pay, IsInitial = true)]
                     [Region(S.Operational, "Billing", S.PayDone, IsTerminal = true)]
                     [Event(E.Start)]
                     [Event(E.Picked)]
                     [Event(E.Paid)]
                     [Transition(S.Draft, E.Start, S.Operational)]
                     [Transition(S.Pick, E.Picked, S.PickDone)]
                     [Transition(S.Pay, E.Paid, S.PayDone)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.State(S.Operational).ParallelComposite();", result.GeneratedSource);
        var fulfillment = "machine.ParallelComposite(S.Operational).Region(\"Fulfillment\", S.Pick, new global::S[] { S.PickDone }, new global::S[] { S.PickDone });";
        var billing = "machine.ParallelComposite(S.Operational).Region(\"Billing\", S.Pay, new global::S[] { S.PayDone }, new global::S[] { S.PayDone });";
        Assert.Contains(fulfillment, result.GeneratedSource);
        Assert.Contains(billing, result.GeneratedSource);
        Assert.True(result.GeneratedSource.IndexOf(fulfillment, System.StringComparison.Ordinal) <
                    result.GeneratedSource.IndexOf(billing, System.StringComparison.Ordinal));
    }
}
