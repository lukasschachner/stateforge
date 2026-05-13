using System.Globalization;
using System.Text.Json;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal static class OrderWorkflowEvents
{
    public static IReadOnlyList<EventDescriptorResponse> Catalog { get; } =
    [
        new(
            nameof(SubmitOrder),
            "Move Draft -> Reviewing. Requires a positive order total.",
            [
                new EventFieldResponse("totalAmount", "decimal", "e.g. 1200.50", true),
                new EventFieldResponse("customerId", "string", "e.g. ACME-42", true)
            ],
            new Dictionary<string, object?>
            {
                ["totalAmount"] = 1200.50m,
                ["customerId"] = "ACME-42"
            }),
        new(
            nameof(ApproveReview),
            "Approve review when risk score is <= 80.",
            [
                new EventFieldResponse("riskScore", "int", "0 - 100", true),
                new EventFieldResponse("approvedBy", "string", "reviewer name", true)
            ],
            new Dictionary<string, object?>
            {
                ["riskScore"] = 67,
                ["approvedBy"] = "casey"
            }),
        new(
            nameof(EscalateReview),
            "Escalate review for manual approval.",
            [new EventFieldResponse("reason", "string", "why escalation is needed", true)],
            new Dictionary<string, object?> { ["reason"] = "high-value order" }),
        new(
            nameof(ApproveEscalation),
            "Approve an escalated review.",
            [new EventFieldResponse("approvedBy", "string", "manager name", true)],
            new Dictionary<string, object?> { ["approvedBy"] = "morgan" }),
        new(
            nameof(StartPacking),
            "Advance fulfillment Picking -> Packed. Not permitted until payment capture is complete.",
            [new EventFieldResponse("worker", "string", "packer name", true)],
            new Dictionary<string, object?> { ["worker"] = "packer-7" }),
        new(
            nameof(ShipOrder),
            "Advance fulfillment Packed -> Shipped. Not permitted until payment capture is complete.",
            [new EventFieldResponse("trackingNumber", "string", "shipment tracking id", true)],
            new Dictionary<string, object?> { ["trackingNumber"] = "TRK-1001" }),
        new(
            nameof(AuthorizePayment),
            "Advance billing PaymentPending -> PaymentAuthorized.",
            [new EventFieldResponse("authorizationCode", "string", "payment gateway auth id", true)],
            new Dictionary<string, object?> { ["authorizationCode"] = "AUTH-1001" }),
        new(
            nameof(CapturePayment),
            "Advance billing payment progress. Packing and shipping stay blocked until capturedTotal >= requiredAmount.",
            [
                new EventFieldResponse("amount", "decimal", "capture amount for this operation", true),
                new EventFieldResponse("capturedTotal", "decimal", "cumulative captured amount", true),
                new EventFieldResponse("requiredAmount", "decimal", "required order total", true)
            ],
            new Dictionary<string, object?>
            {
                ["amount"] = 650m,
                ["capturedTotal"] = 650m,
                ["requiredAmount"] = 1200.50m
            }),
        new(
            nameof(ClearFraudCheck),
            "Advance compliance FraudCheckPending -> FraudCheckCleared.",
            [new EventFieldResponse("analyst", "string", "compliance analyst", true)],
            new Dictionary<string, object?> { ["analyst"] = "finley" }),
        new(
            nameof(PlaceOnHold),
            "Pause Reviewing or Processing and move to OnHold.",
            [new EventFieldResponse("reason", "string", "hold reason", true)],
            new Dictionary<string, object?> { ["reason"] = "awaiting customer response" }),
        new(
            nameof(ResumeProcessing),
            "Resume from OnHold back into Processing (history restore).",
            [new EventFieldResponse("note", "string", "resume note", true)],
            new Dictionary<string, object?> { ["note"] = "customer confirmed details" }),
        new(
            nameof(CancelOrder),
            "Cancel the order from a non-terminal state.",
            [new EventFieldResponse("reason", "string", "cancellation reason", true)],
            new Dictionary<string, object?> { ["reason"] = "customer requested cancellation" })
    ];

    public static bool TryCreate(DemoEventRequest request, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        error = null;

        var payload = request.Payload;
        switch (request.EventType)
        {
            case nameof(SubmitOrder):
                return TryCreateSubmitOrder(payload, out @event, out error);
            case nameof(ApproveReview):
                return TryCreateApproveReview(payload, out @event, out error);
            case nameof(EscalateReview):
                return TryCreateEscalateReview(payload, out @event, out error);
            case nameof(ApproveEscalation):
                return TryCreateApproveEscalation(payload, out @event, out error);
            case nameof(StartPacking):
                return TryCreateStartPacking(payload, out @event, out error);
            case nameof(ShipOrder):
                return TryCreateShipOrder(payload, out @event, out error);
            case nameof(AuthorizePayment):
                return TryCreateAuthorizePayment(payload, out @event, out error);
            case nameof(CapturePayment):
                return TryCreateCapturePayment(payload, out @event, out error);
            case nameof(ClearFraudCheck):
                return TryCreateClearFraudCheck(payload, out @event, out error);
            case nameof(PlaceOnHold):
                return TryCreatePlaceOnHold(payload, out @event, out error);
            case nameof(ResumeProcessing):
                return TryCreateResumeProcessing(payload, out @event, out error);
            case nameof(CancelOrder):
                return TryCreateCancelOrder(payload, out @event, out error);
            default:
                error = $"Unknown event type '{request.EventType}'.";
                return false;
        }
    }

    private static bool TryCreateSubmitOrder(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadDecimal(payload, "totalAmount", out var totalAmount, out error)) return false;
        if (!TryReadString(payload, "customerId", out var customerId, out error)) return false;
        @event = new SubmitOrder(totalAmount, customerId);
        return true;
    }

    private static bool TryCreateApproveReview(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadInt(payload, "riskScore", out var riskScore, out error)) return false;
        if (!TryReadString(payload, "approvedBy", out var approvedBy, out error)) return false;
        @event = new ApproveReview(riskScore, approvedBy);
        return true;
    }

    private static bool TryCreateEscalateReview(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "reason", out var reason, out error)) return false;
        @event = new EscalateReview(reason);
        return true;
    }

    private static bool TryCreateApproveEscalation(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "approvedBy", out var approvedBy, out error)) return false;
        @event = new ApproveEscalation(approvedBy);
        return true;
    }

    private static bool TryCreateStartPacking(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "worker", out var worker, out error)) return false;
        @event = new StartPacking(worker);
        return true;
    }

    private static bool TryCreateShipOrder(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "trackingNumber", out var trackingNumber, out error)) return false;
        @event = new ShipOrder(trackingNumber);
        return true;
    }

    private static bool TryCreateAuthorizePayment(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "authorizationCode", out var authorizationCode, out error)) return false;
        @event = new AuthorizePayment(authorizationCode);
        return true;
    }

    private static bool TryCreateCapturePayment(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadDecimal(payload, "amount", out var amount, out error)) return false;
        if (!TryReadDecimal(payload, "capturedTotal", out var capturedTotal, out error)) return false;
        if (!TryReadDecimal(payload, "requiredAmount", out var requiredAmount, out error)) return false;
        @event = requiredAmount > 0m && capturedTotal >= requiredAmount
            ? new FinalCapturePayment(amount, capturedTotal, requiredAmount)
            : new PartialCapturePayment(amount, capturedTotal, requiredAmount);
        return true;
    }

    private static bool TryCreateClearFraudCheck(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "analyst", out var analyst, out error)) return false;
        @event = new ClearFraudCheck(analyst);
        return true;
    }

    private static bool TryCreatePlaceOnHold(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "reason", out var reason, out error)) return false;
        @event = new PlaceOnHold(reason);
        return true;
    }

    private static bool TryCreateResumeProcessing(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "note", out var note, out error)) return false;
        @event = new ResumeProcessing(note);
        return true;
    }

    private static bool TryCreateCancelOrder(JsonElement payload, out OrderDemoEvent? @event, out string? error)
    {
        @event = null;
        if (!TryReadString(payload, "reason", out var reason, out error)) return false;
        @event = new CancelOrder(reason);
        return true;
    }

    private static bool TryReadString(JsonElement payload, string propertyName, out string value, out string? error)
    {
        value = string.Empty;
        if (!TryReadProperty(payload, propertyName, out var property, out error)) return false;

        if (property.ValueKind != JsonValueKind.String)
        {
            error = $"Payload property '{propertyName}' must be a string.";
            return false;
        }

        var text = property.GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            error = $"Payload property '{propertyName}' cannot be empty.";
            return false;
        }

        value = text;
        error = null;
        return true;
    }

    private static bool TryReadInt(JsonElement payload, string propertyName, out int value, out string? error)
    {
        value = 0;
        if (!TryReadProperty(payload, propertyName, out var property, out error)) return false;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            error = null;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            error = null;
            return true;
        }

        error = $"Payload property '{propertyName}' must be an int.";
        return false;
    }

    private static bool TryReadDecimal(JsonElement payload, string propertyName, out decimal value, out string? error)
    {
        value = 0m;
        if (!TryReadProperty(payload, propertyName, out var property, out error)) return false;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out value))
        {
            error = null;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            error = null;
            return true;
        }

        error = $"Payload property '{propertyName}' must be a decimal.";
        return false;
    }

    private static bool TryReadProperty(JsonElement payload, string propertyName, out JsonElement property,
        out string? error)
    {
        property = default;

        if (payload.ValueKind != JsonValueKind.Object)
        {
            error = "Payload must be a JSON object.";
            return false;
        }

        if (!payload.TryGetProperty(propertyName, out property))
        {
            error = $"Payload is missing required property '{propertyName}'.";
            return false;
        }

        error = null;
        return true;
    }
}
