using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class TransitionConflictDiagnosticTests
{
    [Fact]
    public void Diagnostic_model_preserves_defaults_and_collection_values()
    {
        var guard = new GuardOutcomeDiagnostic(true, ["ready"], "transition-000");
        var shape = new InvalidActiveShapeDiagnostic("Composite", expectedShape: "exactly one active state", actualActiveStates: ["A"]);
        var participant = new TransitionConflictParticipant(
            TransitionConflictParticipantRole.CompetingTransition,
            "transition-000",
            TransitionTriggerKind.Event,
            eventIdentity: "value:go",
            sourceState: "A",
            targetState: "B",
            guardOutcome: guard);

        var diagnostic = new TransitionConflictDiagnostic(
            TransitionConflictKind.DuplicateSourceScope,
            "conflict",
            eventIdentity: "value:go",
            conflictScope: "A",
            participants: [participant],
            invalidShape: shape,
            validationCode: "TRANSITION003");

        Assert.Equal(TransitionConflictKind.DuplicateSourceScope, diagnostic.Kind);
        Assert.Equal("transition-000", diagnostic.Participants.Single().TransitionId);
        Assert.Equal("ready", diagnostic.Participants.Single().GuardOutcome!.ConditionSummaries.Single());
        Assert.Equal("exactly one active state", diagnostic.InvalidShape!.ExpectedShape);
    }

    [Fact]
    public void Existing_result_types_default_to_empty_conflict_collections()
    {
        Assert.Empty(TransitionDiagnostics.None.ConflictDiagnostics);
        Assert.Empty(ValidationResult.Valid.ConflictDiagnostics);

        var diagnostics = new TransitionDiagnostics("summary");
        Assert.Empty(diagnostics.ConflictDiagnostics);
    }
}
