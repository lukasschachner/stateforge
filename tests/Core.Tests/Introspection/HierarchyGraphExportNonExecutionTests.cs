using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class HierarchyGraphExportNonExecutionTests
{
    [Fact]
    public void ExportGraphDoesNotExecuteHierarchyGuardsOrActions()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Draft)
                .On<Submit>()
                .When(_ => throw new InvalidOperationException("guard should not execute during graph export"))
                .ExecuteAction(_ =>
                    throw new InvalidOperationException("transition action should not execute during graph export"))
                .GoTo(HierarchyState.Reviewing);

            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .OnEntry(_ => throw new InvalidOperationException("entry should not execute during graph export"));

            builder.State(HierarchyState.AuthorReview)
                .OnExit(_ => throw new InvalidOperationException("exit should not execute during graph export"));
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded);
        Assert.NotNull(export.Graph);
    }
}