using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelAmbiguousTransitionValidationTests
{
    [Fact]
    public void Duplicate_source_event_transition_is_invalid()
    {
        var definition = StateMachineDefinition<string, string>.Create(b =>
        {
            b.ParallelComposite("P").Region("A", "A1", "A2", "A3");
            b.State("A1").On("go").GoTo("A2");
            b.State("A1").On("go").GoTo("A3");
        });
        Assert.Contains(definition.Validate().Errors, f => f.Code == ParallelValidationCodes.AmbiguousEvent);
    }
}