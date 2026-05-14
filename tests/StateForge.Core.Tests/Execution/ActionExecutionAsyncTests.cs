using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class ActionExecutionAsyncTests
{
    [Fact]
    public async Task SyncAndAsyncActionsRunInConfiguredOrder()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => log.Add("sync exit 1"))
                .OnExitAsync(async (_, _) =>
                {
                    await Task.Yield();
                    log.Add("async exit 2");
                })
                .On<Actions.Pay>()
                .Execute(_ => log.Add("sync transition 1"))
                .ExecuteAsync(async (_, _) =>
                {
                    await Task.Yield();
                    log.Add("async transition 2");
                })
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ => log.Add("sync entry 1"))
                .OnEntryAsync(async (_, _) =>
                {
                    await Task.Yield();
                    log.Add("async entry 2");
                });
        });

        await definition.ApplyAsync(ActionState.Created, new Actions.Pay());

        Assert.Equal([
            "sync exit 1",
            "async exit 2",
            "sync transition 1",
            "async transition 2",
            "sync entry 1",
            "async entry 2"
        ], log);
    }
}