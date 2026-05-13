using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CoreParallelRegionsSampleTests
{
    [Fact]
    public void CoreParallelRegionsSampleRuns()
    {
        var output = CommandRunner.Run("dotnet",
            "run --project samples/Core.ParallelRegionsSample/Core.ParallelRegionsSample.csproj --configuration Release");

        Assert.Contains("Active regions:", output, StringComparison.Ordinal);
        Assert.Contains("Fulfillment: WaitingForPick terminal=False", output, StringComparison.Ordinal);
        Assert.Contains("Billing: WaitingForPayment terminal=False", output, StringComparison.Ordinal);
        Assert.Contains("Dispatch PickStarted: Fulfillment advances; Billing stays waiting.", output,
            StringComparison.Ordinal);
        Assert.Contains("Dispatch PaymentStarted: Billing advances; Fulfillment stays packing.", output,
            StringComparison.Ordinal);
        Assert.Contains("Dispatch CompleteOrder: shared event advances both independent regions.", output,
            StringComparison.Ordinal);
        Assert.Contains("Completion status before all terminal: Operational complete=False", output,
            StringComparison.Ordinal);
        Assert.Contains("Completion status after all terminal: Operational complete=True", output,
            StringComparison.Ordinal);
        Assert.Contains("Graph region: Fulfillment owner=Operational", output, StringComparison.Ordinal);
        Assert.Contains("Graph region: Billing owner=Operational", output, StringComparison.Ordinal);
        Assert.Contains("Invalid model diagnostic: PARALLEL002", output, StringComparison.Ordinal);
        Assert.Contains("Parallel regions sample completed", output, StringComparison.Ordinal);
    }
}
