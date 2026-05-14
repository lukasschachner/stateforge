using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

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
        Assert.Equal("region-000", conflict.SourceRegionId);
        Assert.Equal("region-001", conflict.TargetRegionId);
    }
}