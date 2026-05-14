using StateForge.Core.Definitions;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public enum TelemetryState
{
    Draft,
    Submitted,
    Done
}

public abstract record TelemetryEvent;

public sealed record Submit : TelemetryEvent;

public sealed record DenySubmit : TelemetryEvent;

public sealed record FailSubmit : TelemetryEvent;

public sealed record CancelSubmit : TelemetryEvent;

public sealed record MissingTelemetryEvent : TelemetryEvent;

internal static class TelemetryTestDomain
{
    public static StateMachineDefinition<TelemetryState, TelemetryEvent> Create()
    {
        return StateMachineDefinition<TelemetryState, TelemetryEvent>.Create(builder =>
        {
            builder.State(TelemetryState.Draft)
                .On<Submit>().GoTo(TelemetryState.Submitted)
                .On<DenySubmit>().When(_ => false, "deny").GoTo(TelemetryState.Submitted)
                .On<FailSubmit>().Execute(_ => throw new InvalidOperationException("boom"))
                .GoTo(TelemetryState.Submitted)
                .On<CancelSubmit>().Execute(ctx => throw new OperationCanceledException(ctx.CancellationToken))
                .GoTo(TelemetryState.Submitted);
            builder.State(TelemetryState.Submitted);
        });
    }

    public static StateMachineDefinition<TelemetryState, TelemetryEvent> CreateInvalid()
    {
        return StateMachineDefinition<TelemetryState, TelemetryEvent>.Create(builder =>
        {
            builder.State(TelemetryState.Draft).On<Submit>().GoTo(TelemetryState.Submitted);
        });
    }
}