using StateForge.DependencyInjection.Validation;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal static class OrderWorkflowSmokeRunner
{
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSimpleConsole(options => options.SingleLine = true));
        services.AddOrderWorkflowDemo();

        await using var provider = services.BuildServiceProvider();
        var validation = await provider.GetRequiredService<IStateMachineRegistrationValidator>().ValidateAsync();
        if (!validation.Succeeded)
            throw new InvalidOperationException(validation.ToDisplayString());

        var service = provider.GetRequiredService<OrderWorkflowRuntimeService>();

        var preview = await service.PreviewAsync(new PartialCapturePayment(0m, 0m, 1299.99m));
        Console.WriteLine(
            $"[smoke] Preview CapturePayment from Draft -> status={preview.Status}, permitted={preview.IsPermitted}");

        var prelude = new OrderDemoEvent[]
        {
            new SubmitOrder(1299.99m, "ACME-42"),
            new EscalateReview("manual approval required"),
            new ApproveEscalation("manager-1"),
            new AuthorizePayment("AUTH-1001"),
            new PartialCapturePayment(650m, 650m, 1299.99m)
        };

        foreach (var @event in prelude)
        {
            var outcome = await service.ApplyAsync(@event);
            Console.WriteLine(
                $"[smoke] {@event.GetType().Name} -> category={outcome.Category}, committed={outcome.Committed}, state={outcome.ResultingState}");

            if (!outcome.Committed)
                throw new InvalidOperationException(
                    $"Smoke script failed for '{@event.GetType().Name}': {outcome.Summary}");
        }

        var deniedPacking = await service.ApplyAsync(new StartPacking("packer-early"));
        Console.WriteLine(
            $"[smoke] StartPacking before full capture -> category={deniedPacking.Category}, committed={deniedPacking.Committed}");
        if (deniedPacking.Committed)
            throw new InvalidOperationException("StartPacking should be blocked before full payment capture.");

        var completionScript = new OrderDemoEvent[]
        {
            new PlaceOnHold("awaiting customer shipping confirmation"),
            new ResumeProcessing("customer confirmed shipping"),
            new FinalCapturePayment(649.99m, 1299.99m, 1299.99m),
            new StartPacking("packer-7"),
            new ClearFraudCheck("analyst-2"),
            new ShipOrder("TRK-1001")
        };

        foreach (var @event in completionScript)
        {
            var outcome = await service.ApplyAsync(@event);
            Console.WriteLine(
                $"[smoke] {@event.GetType().Name} -> category={outcome.Category}, committed={outcome.Committed}, state={outcome.ResultingState}");

            if (!outcome.Committed)
                throw new InvalidOperationException(
                    $"Smoke script failed for '{@event.GetType().Name}': {outcome.Summary}");
        }

        var state = await service.GetRuntimeStateAsync();
        if (!string.Equals(state.CurrentState, nameof(OrderDemoState.Completed), StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected final state Completed but got {state.CurrentState}.");

        Console.WriteLine($"Interactive API frontend sample smoke test completed: state={state.CurrentState}");
    }
}
