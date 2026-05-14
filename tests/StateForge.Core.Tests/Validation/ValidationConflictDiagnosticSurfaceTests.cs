using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;

namespace StateForge.Core.Tests.Validation;

public sealed class ValidationConflictDiagnosticSurfaceTests
{
    [Fact]
    public void Duplicate_transition_validation_exposes_structured_conflict_diagnostic()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("B").On("go").GoTo("C");
            builder.State("B");
            builder.State("C");
        });

        var validation = definition.Validate();

        var conflict = Assert.Single(validation.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.DuplicateSourceScope);
        Assert.Equal("A", conflict.ConflictScope);
        Assert.Equal("value:go", conflict.EventIdentity);
        Assert.Equal("TRANSITION003", conflict.ValidationCode);
        Assert.Equal(new[] { "transition-000", "transition-001" },
            conflict.Participants.Select(p => p.TransitionId).ToArray());
    }
}
