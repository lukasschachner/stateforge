using Core.Tests.History;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class HistoryDefinitionIntrospectionTests
{
    [Fact]
    public void IntrospectionExposesHistoryMetadata()
    {
        var introspection = HistoryTestDomain.CreateOperationalMachine().Introspect();

        Assert.True(introspection.HasHistory);
        var state = Assert.Single(introspection.HistoryEnabledStates);
        Assert.Equal(HistoryState.Operational, state.Value);
        Assert.True(state.HasHistory);
        Assert.Equal(HistoryMode.Shallow, state.HistoryMode);
    }
}