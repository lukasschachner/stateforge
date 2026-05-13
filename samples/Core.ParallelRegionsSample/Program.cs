using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

internal static class Program
{
    private static async Task Main()
    {
        var definition = CreateOrderProcessingDefinition();
        var validation = definition.Validate();
        if (!validation.IsValid)
            throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(f => $"{f.Code}: {f.Message}")));

        var runtime = definition.CreateRuntime(OrderState.Operational);
        PrintActiveRegions(runtime.ActiveStateShape);
        PrintCompletionStatus("Completion status before all terminal", runtime.ActiveStateShape);

        await runtime.ApplyAsync(OrderEvent.PickStarted);
        Console.WriteLine("Dispatch PickStarted: Fulfillment advances; Billing stays waiting.");
        PrintActiveRegions(runtime.ActiveStateShape);

        await runtime.ApplyAsync(OrderEvent.PaymentStarted);
        Console.WriteLine("Dispatch PaymentStarted: Billing advances; Fulfillment stays packing.");
        PrintActiveRegions(runtime.ActiveStateShape);

        await runtime.ApplyAsync(OrderEvent.CompleteOrder);
        Console.WriteLine("Dispatch CompleteOrder: shared event advances both independent regions.");
        PrintActiveRegions(runtime.ActiveStateShape);
        PrintCompletionStatus("Completion status after all terminal", runtime.ActiveStateShape);

        var graphExport = definition.ExportGraph();
        if (!graphExport.Succeeded)
            throw new InvalidOperationException(string.Join("; ", graphExport.Validation.Errors.Select(f => $"{f.Code}: {f.Message}")));

        foreach (var region in graphExport.Graph!.Regions.OrderBy(region => region.RegionOrder))
        {
            Console.WriteLine(
                $"Graph region: {region.RegionName} owner={region.CompositeState} initial={region.InitialState} members={string.Join(",", region.MemberStates)} terminals={string.Join(",", region.TerminalStates)}");
        }

        PrintInvalidModelDiagnostic();
        Console.WriteLine("Parallel regions sample completed");
    }

    private static StateMachineDefinition<OrderState, OrderEvent> CreateOrderProcessingDefinition()
    {
        return StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.ParallelComposite(OrderState.Operational, composite =>
            {
                composite.Region("Fulfillment", region =>
                {
                    region.Initial(OrderState.WaitingForPick)
                        .On(OrderEvent.PickStarted)
                        .GoTo(OrderState.Packing);
                    region.State(OrderState.Packing)
                        .On(OrderEvent.CompleteOrder)
                        .GoTo(OrderState.FulfillmentDone);
                    region.Terminal(OrderState.FulfillmentDone);
                });

                composite.Region("Billing", region =>
                {
                    region.Initial(OrderState.WaitingForPayment)
                        .On(OrderEvent.PaymentStarted)
                        .GoTo(OrderState.CapturingPayment);
                    region.State(OrderState.CapturingPayment)
                        .On(OrderEvent.CompleteOrder)
                        .GoTo(OrderState.BillingDone);
                    region.Terminal(OrderState.BillingDone);
                });
            });
        });
    }

    private static void PrintActiveRegions(ActiveStateShape<OrderState> shape)
    {
        Console.WriteLine("Active regions:");
        foreach (var region in shape.ActiveRegions)
        {
            Console.WriteLine($"  {region.RegionName}: {region.ActiveLeafState} terminal={region.IsTerminal}");
        }
    }

    private static void PrintCompletionStatus(string label, ActiveStateShape<OrderState> shape)
    {
        var complete = shape.IsParallel &&
                       shape.ActiveRegions.Count > 0 &&
                       shape.ActiveRegions.All(region => region.IsTerminal);
        Console.WriteLine($"{label}: Operational complete={complete}");
    }

    private static void PrintInvalidModelDiagnostic()
    {
        var invalid = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.ParallelComposite(OrderState.Operational, composite =>
            {
                composite.Region("Fulfillment", region => region.State(OrderState.WaitingForPick));
            });
        });

        var finding = invalid.Validate().Errors.OrderBy(error => error.Code, StringComparer.Ordinal).First();
        Console.WriteLine($"Invalid model diagnostic: {finding.Code} {finding.Message}");
    }
}

internal enum OrderState
{
    Operational,
    WaitingForPick,
    Packing,
    FulfillmentDone,
    WaitingForPayment,
    CapturingPayment,
    BillingDone
}

internal enum OrderEvent
{
    PickStarted,
    PaymentStarted,
    CompleteOrder
}
