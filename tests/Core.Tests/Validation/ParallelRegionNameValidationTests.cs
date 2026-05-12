using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelRegionNameValidationTests
{
    [Fact]
    public void Blank_and_duplicate_region_names_are_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P").Region(" ", "A").Region("R", "B").Region("r", "C");
        });
        var errors = definition.Validate().Errors;
        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.BlankRegionName);
        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.DuplicateRegionName);
    }
}