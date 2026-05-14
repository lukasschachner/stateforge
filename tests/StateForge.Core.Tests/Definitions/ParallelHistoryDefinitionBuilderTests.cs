using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Definitions;

public sealed class ParallelHistoryDefinitionBuilderTests
{
    [Fact]
    public void Parallel_composite_builder_accepts_shallow_history()
    {
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);

        var state = Assert.Single(definition.HistoryEnabledStates, s => s.Value == ParallelHistoryState.Operational);
        Assert.Equal(HistoryMode.Shallow, state.HistoryMode);
        Assert.DoesNotContain(definition.Validate().Errors,
            f => f.Code == ParallelValidationCodes.DirectHistoryUnsupported);
    }

    [Fact]
    public void Parallel_composite_builder_accepts_deep_history()
    {
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Deep);

        var state = Assert.Single(definition.HistoryEnabledStates, s => s.Value == ParallelHistoryState.Operational);
        Assert.Equal(HistoryMode.Deep, state.HistoryMode);
        Assert.True(definition.Validate().IsValid);
    }
}