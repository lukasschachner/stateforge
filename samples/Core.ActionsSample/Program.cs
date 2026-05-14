using StateForge.Core.Definitions;

internal static class Program
{
    private static async Task Main()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .OnExit(ctx => log.Add($"exit {ctx.SourceState}"), "leave Created")
                .On<Pay>()
                .Execute(ctx => log.Add($"transition {ctx.Event.GetType().Name}"), "record payment")
                .GoTo(OrderState.Paid)
                .On<FailPayment>()
                .Execute(_ => throw new InvalidOperationException("payment gateway rejected the request"),
                    "failing payment")
                .GoTo(OrderState.Paid)
                .On<CancelPayment>()
                .Execute(ctx => throw new OperationCanceledException(ctx.CancellationToken), "cancel payment")
                .GoTo(OrderState.Paid);

            builder.State(OrderState.Paid)
                .OnEntry(ctx => log.Add($"entry {ctx.TargetState}"), "enter Paid")
                .Terminal();
        });

        var success = await definition.ApplyAsync(OrderState.Created, new Pay("PAY-1001"));
        Console.WriteLine($"Outcome: {success.Category}; state: {success.ResultingState}");
        Console.WriteLine("Action log: " + string.Join(" -> ", log));

        var failure = await definition.ApplyAsync(OrderState.Created, new FailPayment());
        Console.WriteLine(
            $"Failure outcome: {failure.Category}; committed: {failure.Committed}; state: {failure.ResultingState}; diagnostics: {failure.Diagnostics.Summary}");

        using var cts = new CancellationTokenSource();
        var cancelled = await definition.ApplyAsync(OrderState.Created, new CancelPayment(), cts.Token);
        Console.WriteLine(
            $"Cancellation outcome: {cancelled.Category}; committed: {cancelled.Committed}; state: {cancelled.ResultingState}");
    }
}

internal enum OrderState
{
    Created,
    Paid
}

internal abstract record OrderEvent;

internal sealed record Pay(string PaymentReference) : OrderEvent;

internal sealed record FailPayment : OrderEvent;

internal sealed record CancelPayment : OrderEvent;