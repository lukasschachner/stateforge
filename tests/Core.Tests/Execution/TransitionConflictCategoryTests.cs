using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Tests.Parallel;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class TransitionConflictCategoryTests
{
    [Fact]
    public async Task Runtime_validation_failure_exposes_cross_region_boundary_category()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment,
                    ParallelState.CapturingPayment);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.CapturingPayment);
        });

        var outcome = await definition.CreateRuntime(ParallelState.Operational).ApplyAsync(ParallelEvent.Cancel);

        Assert.False(outcome.Committed);
        Assert.Contains(outcome.Diagnostics.ValidationFindings,
            finding => finding.Code == ParallelValidationCodes.IllegalBoundaryTransition);
        Assert.Contains(outcome.Diagnostics.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.CrossRegionBoundary);
    }
}
