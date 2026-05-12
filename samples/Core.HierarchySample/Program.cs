using StateMachineLibrary.Core.Definitions;

var definition = StateMachineDefinition<DocumentState, DocumentEvent>.Create(builder =>
{
    builder.State(DocumentState.Draft)
        .On<Submit>().GoTo(DocumentState.Reviewing);
    builder.State(DocumentState.Reviewing)
        .InitialChild(DocumentState.AuthorReview)
        .WithShallowHistory()
        .OnCompletion().GoTo(DocumentState.Published)
        .On<Pause>().GoTo(DocumentState.Suspended)
        .On<Cancel>().GoTo(DocumentState.Rejected);
    builder.State(DocumentState.AuthorReview)
        .On<Submit>().GoTo(DocumentState.LegalReview);
    builder.State(DocumentState.LegalReview)
        .ChildOf(DocumentState.Reviewing)
        .On<Approve>().GoTo(DocumentState.Approved);
    builder.State(DocumentState.Approved).ChildOf(DocumentState.Reviewing).Terminal();
    builder.State(DocumentState.Suspended)
        .On<Resume>().GoTo(DocumentState.Reviewing);
    builder.State(DocumentState.Published).Terminal();
    builder.State(DocumentState.Rejected).Terminal();
});

var runtime = definition.CreateRuntime(DocumentState.Draft);
var enteredReview = await runtime.ApplyAsync(new Submit());
Console.WriteLine($"Active leaf: {enteredReview.ActiveLeafState}");
Console.WriteLine($"Active path: {enteredReview.ActiveStatePath}");

var legal = await runtime.ApplyAsync(new Submit());
Console.WriteLine($"After submit: {legal.ActiveStatePath}");
var activeSnapshot = runtime.CaptureActiveStateSnapshot();
Console.WriteLine($"Active snapshot: {activeSnapshot.Kind} {activeSnapshot.ActivePath}");
var restoredFromSnapshot = definition.CreateRuntime(activeSnapshot);
Console.WriteLine($"Snapshot restored leaf: {restoredFromSnapshot.CurrentState}");

var suspended = await runtime.ApplyAsync(new Pause());
Console.WriteLine($"Suspended at: {suspended.ResultingState}");

var resumed = await runtime.ApplyAsync(new Resume());
Console.WriteLine($"History restored path: {resumed.ActiveStatePath}");

var rejected = await runtime.ApplyAsync(new Cancel());
Console.WriteLine($"Parent fallback result: {rejected.ResultingState}");

var completionRuntime = definition.CreateRuntime(DocumentState.Draft);
await completionRuntime.ApplyAsync(new Submit());
await completionRuntime.ApplyAsync(new Submit());
var completedReview = await completionRuntime.ApplyAsync(new Approve());
Console.WriteLine($"Completion transition result: {completedReview.ResultingState} ({completedReview.Transition?.TriggerKind})");

var parallelDefinition = StateMachineDefinition<ParallelSampleState, ParallelSampleEvent>.Create(builder =>
{
    builder.State(ParallelSampleState.Operational).On(ParallelSampleEvent.Cancel).GoTo(ParallelSampleState.Cancelled);
    builder.State(ParallelSampleState.Cancelled).On(ParallelSampleEvent.Resume).GoTo(ParallelSampleState.Operational);
    builder.ParallelComposite(ParallelSampleState.Operational)
        .WithHistory()
        .Region("Fulfillment", ParallelSampleState.WaitingForPick, ParallelSampleState.Packing)
        .Region("Billing", ParallelSampleState.WaitingForPayment, ParallelSampleState.CapturingPayment);
    builder.State(ParallelSampleState.WaitingForPick).On(ParallelSampleEvent.Advance).GoTo(ParallelSampleState.Packing);
    builder.State(ParallelSampleState.WaitingForPayment).On(ParallelSampleEvent.Advance)
        .GoTo(ParallelSampleState.CapturingPayment);
});
var parallelRuntime = parallelDefinition.CreateRuntime(ParallelSampleState.Operational);
await parallelRuntime.ApplyAsync(ParallelSampleEvent.Advance);
Console.WriteLine("Parallel regions: " + string.Join(", ",
    parallelRuntime.ActiveStateShape.ActiveRegions.Select(r => $"{r.RegionName}={r.ActiveLeafState}")));
var parallelSnapshot = parallelRuntime.CaptureActiveStateSnapshot();
Console.WriteLine("Parallel active snapshot: " + string.Join(", ",
    parallelSnapshot.RegionSnapshots.Select(r => $"{r.RegionName}={r.ActiveLeafState}")));
await parallelRuntime.ApplyAsync(ParallelSampleEvent.Cancel);
await parallelRuntime.ApplyAsync(ParallelSampleEvent.Resume);
Console.WriteLine("Parallel history restored regions: " + string.Join(", ",
    parallelRuntime.ActiveStateShape.ActiveRegions.Select(r => $"{r.RegionName}={r.ActiveLeafState}")));
Console.WriteLine("Parallel history snapshots: " + string.Join(", ",
    parallelRuntime.ParallelHistorySnapshots.Select(s =>
        $"{s.CompositeState}:{s.HistoryMode}:{s.RegionEntries.Count}")));

public enum DocumentState
{
    Draft,
    Reviewing,
    AuthorReview,
    LegalReview,
    Approved,
    Published,
    Rejected,
    Suspended
}

public abstract record DocumentEvent;

public sealed record Submit : DocumentEvent;

public sealed record Approve : DocumentEvent;

public sealed record Cancel : DocumentEvent;

public sealed record Pause : DocumentEvent;

public sealed record Resume : DocumentEvent;

public enum ParallelSampleState
{
    Operational,
    WaitingForPick,
    Packing,
    WaitingForPayment,
    CapturingPayment,
    Cancelled
}

public enum ParallelSampleEvent
{
    Advance,
    Cancel,
    Resume
}