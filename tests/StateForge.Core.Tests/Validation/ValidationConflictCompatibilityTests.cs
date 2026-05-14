using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;

namespace StateForge.Core.Tests.Validation;

public sealed class ValidationConflictCompatibilityTests
{
    [Fact]
    public void Existing_validation_findings_remain_available_with_structured_diagnostics()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("B").On("go").GoTo("C");
            builder.State("B");
            builder.State("C");
        });

        var validation = definition.Validate();

        Assert.Contains(validation.Errors, finding => finding.Code == "TRANSITION003" && finding.Message.Contains("Duplicate"));
        Assert.Contains(validation.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.DuplicateSourceScope && diagnostic.ValidationCode == "TRANSITION003");
    }
}
