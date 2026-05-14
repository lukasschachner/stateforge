using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Observation;

public sealed class ParallelObservationOrderingTests
{
    [Fact]
    public async Task Regional_observations_follow_region_order()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment,
                    ParallelState.CapturingPayment);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.Packing);
            builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.Cancel)
                .GoTo(ParallelState.CapturingPayment);
        });
        var observer = new Observer();
        var runtime = definition.CreateRuntime(ParallelState.Operational, observer: observer);
        await runtime.ApplyAsync(ParallelEvent.Cancel);
        Assert.Equal([ParallelState.WaitingForPick, ParallelState.WaitingForPayment], observer.CommittedSources);
    }

    private sealed class Observer : ITransitionObserver<ParallelState, ParallelEvent>
    {
        public List<ParallelState> CommittedSources { get; } = [];

        public ValueTask ObserveAsync(TransitionObservation<ParallelState, ParallelEvent> observation,
            CancellationToken cancellationToken)
        {
            if (observation.Kind == TransitionObservationKind.Committed) CommittedSources.Add(observation.SourceState);
            return ValueTask.CompletedTask;
        }
    }
}