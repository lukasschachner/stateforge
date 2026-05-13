using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

internal enum PreviewState
{
    Draft,
    Review,
    PreviewApproved,
    PreviewRejected,
    PreviewClosed,
    Parent,
    ChildA,
    ChildB,
    Parallel,
    LeftIdle,
    LeftDone,
    RightIdle,
    RightDone
}

internal abstract record PreviewEvent;

internal sealed record PreviewSubmit : PreviewEvent;
internal sealed record PreviewApprove(bool Allowed = true) : PreviewEvent;
internal sealed record PreviewReject : PreviewEvent;
internal sealed record PreviewClose : PreviewEvent;
internal sealed record UnknownPreviewEvent : PreviewEvent;

internal static class TransitionPreviewTestDomain
{
    public static StateMachineDefinition<PreviewState, PreviewEvent> Guarded(bool allow = true,
        Action? guardSideEffect = null)
    {
        return StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.State(PreviewState.Draft).On<PreviewSubmit>().GoTo(PreviewState.Review);
            builder.State(PreviewState.Review).On<PreviewApprove>().When(_ =>
            {
                guardSideEffect?.Invoke();
                return allow;
            }, "approval guard").GoTo(PreviewState.PreviewApproved);
            builder.State(PreviewState.PreviewApproved).Terminal();
        });
    }

    public static StateMachineDefinition<PreviewState, PreviewEvent> Hierarchical()
    {
        return StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.CompositeState(PreviewState.Parent, PreviewState.ChildA);
            builder.ChildState(PreviewState.ChildB, PreviewState.Parent);
            builder.State(PreviewState.ChildA).On<PreviewSubmit>().GoTo(PreviewState.ChildB);
            builder.State(PreviewState.ChildB);
        });
    }

    public static StateMachineDefinition<PreviewState, PreviewEvent> Parallel()
    {
        return StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.ParallelComposite(PreviewState.Parallel, parallel =>
            {
                parallel.Region("Left", region =>
                {
                    region.Initial(PreviewState.LeftIdle);
                    region.Terminal(PreviewState.LeftDone);
                });
                parallel.Region("Right", region =>
                {
                    region.Initial(PreviewState.RightIdle);
                    region.Terminal(PreviewState.RightDone);
                });
            });
            builder.State(PreviewState.LeftIdle).On<PreviewSubmit>().GoTo(PreviewState.LeftDone);
            builder.State(PreviewState.RightIdle).On<PreviewSubmit>().GoTo(PreviewState.RightDone);
        });
    }

    public static ActiveStateShape<PreviewState> Shape(PreviewState state) => ActiveStateShape<PreviewState>.Single(state);
}
