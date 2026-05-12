using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelRegionInitialValidationTests
{
    [Fact]
    public void Zero_region_parallel_composite_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b => b.ParallelCompositeState("P"));
        Assert.Contains(definition.Validate().Errors, f => f.Code == ParallelValidationCodes.ZeroRegions);
    }

    [Fact]
    public void Missing_region_initial_state_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b => b.ParallelComposite("P").Region("R"));
        Assert.Contains(definition.Validate().Errors, f => f.Code == ParallelValidationCodes.MissingInitial);
    }
}