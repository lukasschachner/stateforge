using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
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
        var validation = definition.Validate();

        Assert.Contains(validation.Errors, f => f.Code == ParallelValidationCodes.IllegalBoundaryTransition);
        var conflict = Assert.Single(validation.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.CrossRegionBoundary);
        Assert.Equal("P:A:0", conflict.SourceRegionId);
        Assert.Equal("P:B:1", conflict.TargetRegionId);
    }
}