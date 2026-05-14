using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;

namespace StateForge.Core.Tests.Validation;

public sealed class TransitionConflictValidationDiagnosticTests
{
    [Fact]
    public void Duplicate_source_scope_diagnostic_lists_competing_transitions_in_declaration_order()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("B").On("go").GoTo("C");
            builder.State("B");
            builder.State("C");
        });

        var conflict = definition.Validate().ConflictDiagnostics.Single(diagnostic =>
            diagnostic.Kind == TransitionConflictKind.DuplicateSourceScope);

        Assert.Equal("A", conflict.ConflictScope);
        Assert.Equal(new[] { "B", "C" }, conflict.Participants.Select(p => p.TargetState).Cast<string>().ToArray());
    }
}
