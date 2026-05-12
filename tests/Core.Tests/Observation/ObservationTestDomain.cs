using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public enum ObservationState
{
    A,
    B,
    C
}

public abstract record ObservationEvent;

public sealed record Go : ObservationEvent;

public sealed record Deny : ObservationEvent;

public sealed record FailBefore : ObservationEvent;

public sealed record FailAfter : ObservationEvent;

public sealed record CancelBefore : ObservationEvent;

public sealed record CancelAfter : ObservationEvent;

public sealed record Missing : ObservationEvent;

internal static class ObservationTestDomain
{
    public static StateMachineDefinition<ObservationState, ObservationEvent> Create()
    {
        return StateMachineDefinition<ObservationState, ObservationEvent>.Create(builder =>
        {
            builder.State(ObservationState.A)
                .On<Go>().GoTo(ObservationState.B)
                .On<Deny>().When(_ => false, "deny").GoTo(ObservationState.B)
                .On<FailBefore>().Execute(_ => throw new InvalidOperationException("before")).GoTo(ObservationState.B)
                .On<FailAfter>().OnEntry(_ => throw new InvalidOperationException("after")).GoTo(ObservationState.B)
                .On<CancelBefore>().Execute(ctx => throw new OperationCanceledException(ctx.CancellationToken))
                .GoTo(ObservationState.B)
                .On<CancelAfter>().OnEntry(ctx => throw new OperationCanceledException(ctx.CancellationToken))
                .GoTo(ObservationState.B);
            builder.State(ObservationState.B);
        });
    }

    public static StateMachineDefinition<ObservationState, ObservationEvent> CreateInvalid()
    {
        return StateMachineDefinition<ObservationState, ObservationEvent>.Create(builder =>
        {
            builder.State(ObservationState.A).On<Go>().GoTo(ObservationState.B);
        });
    }

    public static async Task<IReadOnlyList<TransitionObservationKind>> ObserveKindsAsync(
        ObservationEvent @event,
        CancellationToken cancellationToken = default)
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        await Create().ApplyAsync(ObservationState.A, @event, cancellationToken, observer);
        return observer.Observations.Select(o => o.Kind).ToArray();
    }
}