using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Observation;

public class MachineNameObservationTests
{
    [Fact]
    public async Task ObservationIncludesMachineNameFromWellKnownMetadata()
    {
        var definition = StateMachineDefinition<ObservationState, ObservationEvent>.Create(builder =>
        {
            builder.WithMetadata(StateMachineMetadataKeys.Name, "orders");
            builder.State(ObservationState.A).On<Go>().GoTo(ObservationState.B);
            builder.State(ObservationState.B);
        });
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();

        await definition.ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.All(observer.Observations, observation => Assert.Equal("orders", observation.MachineName));
    }
}