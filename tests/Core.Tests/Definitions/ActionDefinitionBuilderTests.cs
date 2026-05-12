using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Definitions;

public class ActionDefinitionBuilderTests
{
    [Fact]
    public void StateActionsCaptureKindOrderDisplayNameMetadataAndDefaults()
    {
        var metadata = MetadataCollection.Empty.With("owner", "billing");
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnEntry(_ => { }, "created entry", metadata)
                .OnEntryAsync((_, _) => ValueTask.CompletedTask)
                .OnExit(_ => { }, "created exit", metadata)
                .On<Actions.Pay>()
                .Execute(_ => { }, "pay transition")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid);
        });

        var state = definition.FindState(ActionState.Created)!;
        Assert.Equal(["created entry", "Entry action 2"], state.EntryActions.Select(a => a.DisplayName));
        Assert.Equal(ActionKind.Entry, state.EntryActions[0].Kind);
        Assert.Equal(TransitionLifecyclePhase.Entry, state.EntryActions[0].Phase);
        Assert.Equal("billing", state.EntryActions[0].Metadata["owner"]);
        Assert.Equal("created exit", state.ExitActions.Single().DisplayName);

        var transitionAction = definition.Transitions.Single().TransitionActions.Single();
        Assert.Equal(ActionKind.Transition, transitionAction.Kind);
        Assert.Equal("pay transition", transitionAction.DisplayName);
    }

    [Fact]
    public void NoActionDefaultsAreEmptyCollections()
    {
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created).On<Actions.Pay>().GoTo(ActionState.Paid);
            builder.State(ActionState.Paid);
        });

        Assert.All(definition.States, state =>
        {
            Assert.Empty(state.EntryActions);
            Assert.Empty(state.ExitActions);
        });
        Assert.Empty(definition.Transitions.Single().TransitionActions);
    }
}