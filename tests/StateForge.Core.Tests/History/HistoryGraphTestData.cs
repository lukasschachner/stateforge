namespace StateForge.Core.Tests.History;

public static class HistoryGraphTestData
{
    public static string[] OperationalHistoryLabels =>
    [
        nameof(HistoryState.Operational),
        nameof(HistoryState.Idle),
        nameof(HistoryState.Processing),
        nameof(HistoryState.Done)
    ];
}