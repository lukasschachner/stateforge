using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelRegionMembershipValidationTests
{
    [Fact]
    public void State_cannot_belong_to_multiple_sibling_regions()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
            b.ParallelComposite("P").Region("A", "A1", "Shared").Region("B", "B1", "Shared"));
        Assert.Contains(definition.Validate().Errors, f => f.Code == ParallelValidationCodes.InvalidMembership);
    }
}