using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Definitions;

public class DefinitionValidationTests
{
    [Fact]
    public void DuplicateTransitionsProduceActionableErrors()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>().GoTo(OrderState.Paid)
                .On<Pay>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid);
            builder.State(OrderState.Cancelled);
        });

        var result = definition.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.Code == "TRANSITION003" && f.Message.Contains("Duplicate"));
        Assert.Contains(result.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.DuplicateSourceScope);
    }

    [Fact]
    public void InvalidTargetStateProducesError()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
        });

        var result = definition.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.Code == "TRANSITION002");
        Assert.Contains(result.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.InvalidPostShape);
    }

    [Fact]
    public void TerminalStateWithOutgoingTransitionProducesError()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).Terminal().On<Pay>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var result = definition.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.Code == "TERMINAL001" && f.Severity == ValidationSeverity.Error);
    }
}