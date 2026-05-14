using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.DependencyInjection.Tests.TestSupport;

public enum TestState { Created, Paid, Cancelled }
public enum OtherState { A, B }
public abstract record TestEvent;
public sealed record Pay : TestEvent;
public sealed record Cancel : TestEvent;
public enum OtherEvent { Go }

public static class DependencyInjectionTestDomain
{
    public static StateMachineDefinition<TestState, TestEvent> Definition() => StateMachineDefinition<TestState, TestEvent>.Create(b =>
    {
        b.State(TestState.Created).On<Pay>().GoTo(TestState.Paid).On<Cancel>().GoTo(TestState.Cancelled);
        b.State(TestState.Paid);
        b.State(TestState.Cancelled).Terminal();
    });

    public static StateMachineDefinition<OtherState, OtherEvent> OtherDefinition() => StateMachineDefinition<OtherState, OtherEvent>.Create(b =>
    {
        b.State(OtherState.A).On(OtherEvent.Go).GoTo(OtherState.B);
        b.State(OtherState.B);
    });
}

public sealed class RecordingObserver : ITransitionObserver<TestState, TestEvent>
{
    public List<TransitionObservation<TestState, TestEvent>> Observations { get; } = [];
    public ValueTask ObserveAsync(TransitionObservation<TestState, TestEvent> observation, CancellationToken cancellationToken = default)
    {
        Observations.Add(observation);
        return ValueTask.CompletedTask;
    }
}
