using Core.Tests.History;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public class HistoryFallbackValidationTests
{
    [Fact]
    public void HistoryOnNonCompositeIsRejected()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Idle).WithShallowHistory();
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.HistoryOnNonComposite);
    }

    [Fact]
    public void HistoryFallbackMustBeDirectChild()
    {
        var definition = StateMachineDefinition<HistoryState, HistoryEvent>.Create(builder =>
        {
            builder.State(HistoryState.Operational).InitialChild(HistoryState.Idle)
                .WithShallowHistory(HistoryState.Other);
            builder.State(HistoryState.Processing).ChildOf(HistoryState.Operational);
            builder.State(HistoryState.Other);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.InvalidHistoryFallback);
    }
}