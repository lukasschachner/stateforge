using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;

namespace StateForge.Core.Tests.Execution;

public sealed class InvalidPostShapeConflictDiagnosticTests
{
    [Fact]
    public async Task Runtime_validation_failure_for_missing_target_forwards_invalid_post_shape_diagnostic()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("Missing");
        });

        var outcome = await definition.ApplyAsync("A", "go");

        Assert.False(outcome.Committed);
        var conflict = Assert.Single(outcome.Diagnostics.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.InvalidPostShape);
        Assert.Equal("TRANSITION002", conflict.ValidationCode);
        Assert.NotNull(conflict.InvalidShape);
    }
}
