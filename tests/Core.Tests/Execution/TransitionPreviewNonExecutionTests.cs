using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public sealed class TransitionPreviewNonExecutionTests
{
    [Fact]
    public async Task PreviewDoesNotRunActionsBehaviorsOrCompletionCascades()
    {
        var entry = 0;
        var exit = 0;
        var transitionAction = 0;
        var behavior = 0;
        var completion = 0;
        var definition = StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.State(PreviewState.Draft)
                .OnExit(_ => exit++)
                .On<PreviewSubmit>()
                .Execute(_ => transitionAction++)
                .OnEntry(_ => behavior++)
                .GoTo(PreviewState.ChildA);
            builder.State(PreviewState.Parent)
                .InitialChild(PreviewState.ChildA)
                .OnCompletion().Execute(_ => completion++).GoTo(PreviewState.PreviewApproved);
            builder.State(PreviewState.ChildA)
                .ChildOf(PreviewState.Parent)
                .OnEntry(_ => entry++)
                .Terminal();
            builder.State(PreviewState.PreviewApproved).Terminal();
        });

        var preview = await definition.PreviewAsync(ActiveStateShape<PreviewState>.Single(PreviewState.Draft),
            new PreviewSubmit());

        Assert.True(preview.IsPermitted);
        Assert.Equal(0, entry);
        Assert.Equal(0, exit);
        Assert.Equal(0, transitionAction);
        Assert.Equal(0, behavior);
        Assert.Equal(0, completion);
    }
}
