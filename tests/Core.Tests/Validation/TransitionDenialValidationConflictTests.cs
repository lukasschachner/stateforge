using Core.Tests.Execution;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Validation;

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
