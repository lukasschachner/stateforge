# Interactive API + Frontend Sample

This sample demonstrates a relatively complex finite-state machine with an HTTP API and a lightweight frontend.

Run it:

```bash
dotnet run --project samples/Interactive.ApiFrontendSample/Interactive.ApiFrontendSample.csproj --configuration Release
```

Open <http://localhost:5000> or the URL printed by ASP.NET Core.

## What it showcases

- Hierarchical review flow (`Reviewing` with child states and completion transition).
- Parallel processing composite (`Processing`) with independent `Fulfillment`, `Billing`, and `Compliance` regions.
- Payment-progress gating: partial captures keep fulfillment blocked, so packing/shipping remain denied until `capturedTotal >= requiredAmount`.
- Parallel completion transition to `Completed` once all regions are terminal.
- Guarded transitions and denial diagnostics (`SubmitOrder.TotalAmount`, `ApproveReview.RiskScore`, `CapturePayment.Amount`, `CapturePayment.CapturedTotal`).
- History restore when moving `Processing -> OnHold -> Processing`.

## API endpoints

All endpoints are under `/api/order-workflow`:

- `GET /events/catalog` — available event types, payload fields, and example payloads.
- `GET /runtime/state` — current active shape and permitted events.
- `GET /definition/graph` — definition graph plus runtime active overlay.
- `GET /definition/diagram/mermaid` — Mermaid state diagram text (with runtime overlay hints).
- `POST /runtime/events/preview` — side-effect-free event preview.
- `POST /runtime/events/apply` — apply event and return transition outcome.
- `POST /runtime/reset` — reset runtime back to the initial state.

Example preview request:

```json
{
  "eventType": "CapturePayment",
  "payload": {
    "amount": 650.00,
    "capturedTotal": 650.00,
    "requiredAmount": 1200.50
  }
}
```

## Frontend behavior

The frontend (`wwwroot/`) consumes only the sample API and visualizes:

- a rendered Mermaid graph diagram (via Mermaid.js in the browser),
- diagram edge highlighting for preview-selected/candidate/applied transitions,
- current runtime state and active parallel regions,
- payment progress (required/captured/remaining/percent),
- permitted events,
- graph nodes/edges from introspection (raw details view),
- runtime overlay highlighting active nodes in the diagram,
- preview/apply result payloads for event effect comparison.

## Release-test smoke mode

For CI/release validation, the sample supports a non-interactive smoke run:

```bash
dotnet run --project samples/Interactive.ApiFrontendSample/Interactive.ApiFrontendSample.csproj --configuration Release -- --smoke-test
```

Smoke mode drives a deterministic event script and exits with:

- `Interactive API frontend sample smoke test completed: state=Completed`
