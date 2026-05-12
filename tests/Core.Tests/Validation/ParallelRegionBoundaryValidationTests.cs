using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelRegionBoundaryValidationTests
{
    [Fact]
    public void Direct_sibling_region_transition_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P").Region("A", "A1", "A2").Region("B", "B1", "B2");
            b.State("A1").On("go").GoTo("B2");
        });
        Assert.Contains(definition.Validate().Errors, f => f.Code == ParallelValidationCodes.IllegalBoundaryTransition);
    }
}