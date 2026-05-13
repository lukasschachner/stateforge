using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class RuntimeGraphExportNonExecutionTests
{
    [Fact]
    public void ExportGraph_does_not_evaluate_guards_or_run_actions()
    {
        var guardCalls = 0;
        var actionCalls = 0;
        var definition = StateMachineDefinition<RuntimeGraphState, RuntimeGraphEvent>.Create(builder =>
        {
            builder.State(RuntimeGraphState.Created)
                .OnExit(_ => actionCalls++)
                .On(RuntimeGraphEvent.Pay)
                .When(_ =>
                {
                    guardCalls++;
                    return true;
                })
                .Execute(_ => actionCalls++)
                .GoTo(RuntimeGraphState.Paid);
            builder.State(RuntimeGraphState.Paid).OnEntry(_ => actionCalls++).Terminal();
        });
        var runtime = definition.CreateRuntime(RuntimeGraphState.Created);

        _ = RuntimeGraphExportAssertions.SucceededGraph(runtime.ExportGraph());

        Assert.Equal(0, guardCalls);
        Assert.Equal(0, actionCalls);
        Assert.Equal(RuntimeGraphState.Created, runtime.CurrentState);
    }

    [Fact]
    public void ExportGraph_does_not_mutate_history_snapshots()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateHierarchicalDefinition()
            .CreateRuntime(RuntimeGraphState.Reviewing);
        var before = runtime.HistorySnapshots.Select(snapshot => snapshot.LastUpdatedSequence).ToArray();

        _ = runtime.ExportGraph();

        Assert.Equal(before, runtime.HistorySnapshots.Select(snapshot => snapshot.LastUpdatedSequence));
    }
}
