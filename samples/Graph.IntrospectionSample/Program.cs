using StateForge.Core.Definitions;

internal static class Program
{
    private static async Task Main()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On(OrderEvent.Pay).GoTo(OrderState.Paid)
                .On(OrderEvent.Cancel).GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid).On(OrderEvent.Ship).GoTo(OrderState.Shipped);
            builder.State(OrderState.Reviewing)
                .InitialChild(OrderState.ReviewDone)
                .OnCompletion()
                .GoTo(OrderState.Shipped);
            builder.State(OrderState.ReviewDone).ChildOf(OrderState.Reviewing).Terminal();
            builder.State(OrderState.Shipped).Terminal();
            builder.State(OrderState.Cancelled).Terminal();
        });

        var export = definition.ExportGraph();
        if (!export.Succeeded)
            throw new InvalidOperationException(string.Join("; ", export.Validation.Findings.Select(f => f.Message)));

        var graph = export.Graph!;
        foreach (var edge in graph.Edges)
            Console.WriteLine($"{edge.SourceNodeId} --{edge.Label} ({edge.TriggerKind})--> {edge.TargetNodeId}");

        var parallelDefinition = StateMachineDefinition<ParallelOrderState, ParallelOrderEvent>.Create(builder =>
        {
            builder.State(ParallelOrderState.Operational).On(ParallelOrderEvent.Cancel)
                .GoTo(ParallelOrderState.Cancelled);
            builder.State(ParallelOrderState.Cancelled).On(ParallelOrderEvent.Resume)
                .GoTo(ParallelOrderState.Operational);
            builder.ParallelComposite(ParallelOrderState.Operational)
                .WithHistory()
                .Region("Fulfillment", ParallelOrderState.WaitingForPick, ParallelOrderState.Packing)
                .Region("Billing", ParallelOrderState.WaitingForPayment, ParallelOrderState.CapturingPayment);
            builder.State(ParallelOrderState.WaitingForPick).On(ParallelOrderEvent.Advance)
                .GoTo(ParallelOrderState.Packing);
            builder.State(ParallelOrderState.WaitingForPayment).On(ParallelOrderEvent.Advance)
                .GoTo(ParallelOrderState.CapturingPayment);
        });
        var parallelGraph = parallelDefinition.ExportGraph().Graph!;
        foreach (var region in parallelGraph.Regions)
            Console.WriteLine(
                $"Parallel region: {region.RegionName} initial={region.InitialState} members={region.MemberStates.Count} history={region.ParallelHistoryMode}");
        foreach (var history in parallelDefinition.Introspect().ParallelHistoryDefinitions)
            Console.WriteLine(
                $"Parallel history definition: {history.CompositeState} mode={history.HistoryMode} fallbacks={history.RegionFallbacks.Count}");
        var parallelRuntime = parallelDefinition.CreateRuntime(ParallelOrderState.Operational);
        await parallelRuntime.ApplyAsync(ParallelOrderEvent.Advance);
        Console.WriteLine("Active parallel shape: " + string.Join(", ",
            parallelRuntime.ActiveStateShape.ActiveRegions.Select(r => $"{r.RegionName}={r.ActiveLeafState}")));
        var activeSnapshot = parallelRuntime.CaptureActiveStateSnapshot();
        Console.WriteLine($"Active snapshot kind: {activeSnapshot.Kind} regions={activeSnapshot.RegionSnapshots.Count}");
        var runtimeOverlayGraph = parallelRuntime.ExportGraph().Graph!;
        var runtimeOverlay = runtimeOverlayGraph.RuntimeOverlay!;
        Console.WriteLine(
            $"Runtime graph overlay: shape={runtimeOverlay.ShapeKind} sequence={runtimeOverlay.Sequence} regions={runtimeOverlay.Regions.Count}");
        foreach (var region in runtimeOverlay.Regions)
            Console.WriteLine(
                $"Runtime overlay region: {region.RegionOrder}:{region.RegionName} active={region.ActiveLeafState} terminal={region.IsTerminal}");
        Console.WriteLine($"Introspection snapshot kind: {parallelDefinition.Introspect().GetActiveStateSnapshotKind(ParallelOrderState.Operational)}");
        Console.WriteLine("Recorded parallel history: " + string.Join(", ",
            parallelRuntime.ParallelHistorySnapshots.Select(s =>
                $"{s.CompositeState}:{s.HistoryMode}:{s.RegionEntries.Count}")));

        Console.WriteLine(
            $"Graph introspection sample completed: nodes={graph.Nodes.Count}, edges={graph.Edges.Count}, regions={parallelGraph.Regions.Count}");
    }
}

internal enum OrderState
{
    Created,
    Paid,
    Shipped,
    Cancelled,
    Reviewing,
    ReviewDone
}

internal enum OrderEvent
{
    Pay,
    Ship,
    Cancel
}

internal enum ParallelOrderState
{
    Operational,
    WaitingForPick,
    Packing,
    WaitingForPayment,
    CapturingPayment,
    Cancelled
}

internal enum ParallelOrderEvent
{
    Advance,
    Cancel,
    Resume
}