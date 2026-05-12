using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Actions;

internal enum ActionState
{
    Created,
    Paid,
    Closed
}

internal abstract record ActionEvent;

internal sealed record Pay : ActionEvent;

internal sealed record Close : ActionEvent;

internal sealed record Stay : ActionEvent;

internal static class ActionTestDomain
{
    public static StateMachineDefinition<ActionState, ActionEvent> CreateWithOrderedActions(IList<string> log)
    {
        return StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => log.Add("exit Created"), "exit Created")
                .On<Pay>()
                .Execute(_ => log.Add("transition Pay"), "transition Pay")
                .GoTo(ActionState.Paid);

            builder.State(ActionState.Paid)
                .OnEntry(_ => log.Add("entry Paid"), "entry Paid");
        });
    }

    public static Action<ActionExecutionContext<ActionState, ActionEvent>> ThrowingAction(string message)
    {
        return _ => throw new InvalidOperationException(message);
    }
}