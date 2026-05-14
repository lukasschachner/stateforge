using StateForge.Core.Tests.Execution;
using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Validation;

public sealed class TransitionDenialValidationConflictTests
{
    [Fact]
    public async Task PreviewReportsValidationConflictsForInvalidDefinition()
    {
        var definition = StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.State(PreviewState.Draft).On<PreviewSubmit>().GoTo(PreviewState.Review);
        });

        var preview = await definition.PreviewAsync(ActiveStateShape<PreviewState>.Single(PreviewState.Draft),
            new PreviewSubmit());

        Assert.Equal(TransitionPreviewStatus.ValidationFailure, preview.Status);
        Assert.Equal(TransitionDenialReason.ValidationConflicts, preview.DenialDiagnostic?.Reason);
        Assert.NotEmpty(preview.ValidationFindings);
    }
}
