using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.History;

public enum HistoryState
{
    Offline,
    Operational,
    Idle,
    Processing,
    Done,
    Suspended,
    Nested,
    NestedIdle,
    NestedBusy,
    Other
}

public abstract record HistoryEvent;

public sealed record Start : HistoryEvent;

public sealed record Pause : HistoryEvent;

public sealed record Resume : HistoryEvent;

public sealed record Finish : HistoryEvent;

public sealed record Reset : HistoryEvent;

public sealed record Fail : HistoryEvent;

public static class HistoryTestDomain
{
    public static StateMachineDefinition<HistoryState, HistoryEvent> CreateOperationalMachine(List<string>? log = null)
    {
        return StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Offline).On<Resume>().GoTo(HistoryState.Operational);
            builder.State(HistoryState.Operational)
                .InitialChild(HistoryState.Idle)
                .WithShallowHistory()
                .On<Pause>().GoTo(HistoryState.Suspended);
            builder.State(HistoryState.Idle)
                .OnEntry(_ => log?.Add("entry Idle"))
                .OnExit(_ => log?.Add("exit Idle"))
                .On<Start>().GoTo(HistoryState.Processing);
            builder.State(HistoryState.Processing)
                .ChildOf(HistoryState.Operational)
                .OnEntry(_ => log?.Add("entry Processing"))
                .OnExit(_ => log?.Add("exit Processing"))
                .On<Finish>().GoTo(HistoryState.Done);
            builder.State(HistoryState.Done).ChildOf(HistoryState.Operational).Terminal();
            builder.State(HistoryState.Suspended).On<Resume>().GoTo(HistoryState.Operational);
        });
    }
}