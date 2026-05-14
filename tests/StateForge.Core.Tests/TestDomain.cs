namespace StateForge.Core.Tests;

internal enum OrderState
{
    Created,
    Paid,
    Shipped,
    Cancelled
}

internal abstract record OrderEvent;

internal sealed record Pay(decimal Amount = 1m) : OrderEvent;

internal sealed record Ship(string Tracking = "T") : OrderEvent;

internal sealed record Cancel(string Reason = "R") : OrderEvent;

internal sealed record Refund : OrderEvent;