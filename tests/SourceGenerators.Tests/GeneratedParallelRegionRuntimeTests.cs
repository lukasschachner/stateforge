namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedParallelRegionRuntimeTests
{
    [Fact]
    public void GeneratedParallelRegionDefinitionsCompileThroughCoreBuilderSurface()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Operational, Fulfillment, FulfillmentDone, Billing, BillingDone }
                     public enum E { Pick, Pay }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "Fulfillment", S.Fulfillment, IsInitial = true)]
                     [Region(S.Operational, "Fulfillment", S.FulfillmentDone, IsTerminal = true)]
                     [Region(S.Operational, "Billing", S.Billing, IsInitial = true)]
                     [Region(S.Operational, "Billing", S.BillingDone, IsTerminal = true)]
                     [Event(E.Pick)]
                     [Event(E.Pay)]
                     [Transition(S.Fulfillment, E.Pick, S.FulfillmentDone)]
                     [Transition(S.Billing, E.Pay, S.BillingDone)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".Region(\"Fulfillment\", S.Fulfillment", result.GeneratedSource);
        Assert.Contains(".Region(\"Billing\", S.Billing", result.GeneratedSource);
    }
}
