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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void New_composite_region_block_rejects_blank_names(string? name)
    {
        var builder = new StateMachineDefinitionBuilder<string, string>();
        var composite = builder.ParallelComposite("P");

        Assert.Throws<ArgumentException>(() => composite.Region(name!, _ => { }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void New_top_level_parallel_region_block_rejects_blank_names(string? name)
    {
        var builder = new StateMachineDefinitionBuilder<string, string>();

        Assert.Throws<ArgumentException>(() => builder.ParallelRegion("P", name!, _ => { }));
    }
}