using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

public sealed class ParallelHistoryFallbackValidationTests
{
    [Fact]
    public void Direct_parallel_history_reports_region_fallback_diagnostics()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.ParallelComposite("Operational")
                .WithHistory()
                .Region("Fulfillment");
        });

        var finding = Assert.Single(definition.Validate().Errors,
            f => f.Code == ParallelValidationCodes.MissingFallback);
        Assert.Equal("Operational", finding.CompositeState);
        Assert.Contains("Fulfillment", finding.Message);
        Assert.Equal("Fulfillment", finding.RegionName);
    }
}