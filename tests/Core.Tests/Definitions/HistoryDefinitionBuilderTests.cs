using Core.Tests.History;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Definitions;

public class HistoryDefinitionBuilderTests
{
    [Fact]
    public void BuilderSupportsShallowHistoryAndFallbackConfiguration()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Operational).InitialChild(HistoryState.Idle)
                .WithShallowHistory(HistoryState.Processing);
            builder.State(HistoryState.Processing).ChildOf(HistoryState.Operational);
        });

        Assert.True(definition.HasHistory);
        var state = Assert.Single(definition.HistoryEnabledStates);
        Assert.Equal(HistoryState.Operational, state.Value);
        Assert.Equal(HistoryMode.Shallow, state.HistoryMode);
        Assert.True(state.HasHistoryFallback);
        Assert.Equal(HistoryState.Processing, state.HistoryFallbackState);
    }

    [Fact]
    public void BuilderSupportsDeepHistoryConfiguration()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.CompositeState(HistoryState.Operational, HistoryState.Idle);
            builder.EnableHistory(HistoryState.Operational, HistoryMode.Deep);
        });

        Assert.Equal(HistoryMode.Deep, definition.HistoryEnabledStates.Single().HistoryMode);
    }
}