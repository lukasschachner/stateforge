using StateForge.Core.Tests.History;
using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

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