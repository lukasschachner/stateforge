using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelHistoryCompatibilityValidationTests
{
    [Fact]
    public void Direct_history_on_valid_parallel_composite_is_accepted()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P").Region("A", "A1");
            b.EnableHistory("P");
        });

        Assert.DoesNotContain(definition.Validate().Errors,
            f => f.Code == ParallelValidationCodes.DirectHistoryUnsupported);
        Assert.True(definition.Validate().IsValid);
    }
}