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

    [Fact]
    public void Mixed_old_and_new_duplicate_sibling_membership_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P", composite =>
            {
                composite.Region("A", region => region.State("Shared"));
                composite.Region("B", "B1", "Shared");
            });
        });

        Assert.Contains(definition.Validate().Errors,
            f => f.Code == ParallelValidationCodes.InvalidMembership && f.Message.Contains("multiple regions"));
    }

    [Fact]
    public void Conflicting_region_list_and_state_level_assignment_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P")
                .Region("A", "A1", "Shared")
                .Region("B", "B1", "B2");
            b.State("Shared").InRegion("P", "B");
        });

        Assert.Contains(definition.Validate().Errors,
            f => f.Code == ParallelValidationCodes.InvalidMembership &&
                 f.Message.Contains("state metadata assigns it to region"));
    }
}