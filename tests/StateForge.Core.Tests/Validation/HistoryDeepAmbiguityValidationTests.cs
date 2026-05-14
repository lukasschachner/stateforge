using StateForge.Core.Tests.History;
using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

public class HistoryDeepAmbiguityValidationTests
{
    [Fact]
    public void AmbiguousNestedDeepHistoryIsRejected()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Operational).InitialChild(HistoryState.Nested).WithDeepHistory();
            builder.State(HistoryState.Nested).ChildOf(HistoryState.Operational).InitialChild(HistoryState.NestedIdle);
            builder.State(HistoryState.NestedBusy).ChildOf(HistoryState.Nested);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.AmbiguousDeepHistory);
    }
}