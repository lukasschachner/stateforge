using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelRegionReachabilityValidationTests
{
    [Fact]
    public void Unreachable_regional_state_is_reported()
    {
        var definition =
            StateMachineDefinition<string, string>.Create(b => b.ParallelComposite("P").Region("A", "A1", "A2"));
        Assert.Contains(definition.Validate().Warnings,
            f => f.Code == ParallelValidationCodes.UnreachableRegionalState);
    }
}