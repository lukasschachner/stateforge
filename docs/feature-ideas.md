# Feature Ideas and SpecKit Specify Prompts

This document captures candidate feature ideas for StateMachineLibrary and groups them into manageable SpecKit feature bundles. Each prompt is intended to be used as a separate `/speckit.specify` invocation.

## Product boundaries to preserve

- Keep Core dependency-light and focused on plain finite state machines.
- Do not turn the library into a workflow engine or orchestration framework.
- Keep visualization rendering, DI, logging, persistence providers, and other integrations in optional packages.
- Preserve explicit transition outcomes, async-first execution, generic typed states/events, and existing package boundaries.

## Feature idea inventory

1. Transition preview / dry-run API.
2. Richer not-permitted and denial diagnostics.
3. Reachability analysis for states, events, dead ends, and paths.
4. Definition diffing for version comparison.
5. Strict enum coverage validation.
6. Transition priority or deterministic conflict handling.
7. Built-in testing helper package.
8. Generated strongly typed event helper methods.
9. Compile-time graph and definition validation in source generators.
10. Generated Mermaid / DOT graph output from source-generator declarations.
11. Optional graph rendering packages for Mermaid, DOT, and PlantUML.
12. Optional persistence providers, such as EF Core, Redis, and file/in-memory testing providers.
13. Optional Microsoft.Extensions.DependencyInjection integration.
14. Optional Microsoft.Extensions.Logging adapter.
15. Runtime snapshot export/import.
16. Batch/event sequence helper for applying multiple events and collecting outcomes.

## Suggested implementation bundles

### Bundle 1: Transition explainability

Includes:

- Transition preview / dry-run API.
- Richer not-permitted diagnostics.
- Transition priority or deterministic conflict handling.

Use this first if users need better debugging and safer transition analysis.

### Bundle 2: Graph analysis and definition versioning

Includes:

- Reachability analysis.
- Definition diffing.
- Strict enum coverage validation.

Use this for design-time validation, migration planning, and release safety.

### Bundle 3: Testing helpers

Includes:

- Dedicated testing helper package.
- Assertions for permitted events, transition outcomes, graph quality, and validation findings.

Use this to improve consumer test ergonomics without adding runtime dependencies to Core.

### Bundle 4: Source generator ergonomics

Includes:

- Generated strongly typed event helper methods.
- Compile-time graph and definition validation.
- Generated graph assets from source-generator declarations.

Use this to make declarative machines more useful at build time.

### Bundle 5: Graph rendering adapters

Includes:

- Optional Mermaid renderer.
- Optional DOT renderer.
- Optional PlantUML renderer.

Use this to keep Core renderer-neutral while providing convenient documentation outputs.

### Bundle 6: Persistence provider adapters

Includes:

- Optional EF Core provider.
- Optional Redis provider.
- Optional file/in-memory testing provider.

Use this only as optional packages layered over existing provider-neutral persistence contracts.

### Bundle 7: Application integration adapters

Includes:

- Optional Microsoft.Extensions.DependencyInjection integration.
- Optional Microsoft.Extensions.Logging adapter.

Use this to improve app integration without adding DI/logging dependencies to Core.

### Bundle 8: Runtime convenience APIs

Includes:

- Runtime snapshot export/import.
- Batch/event sequence helper.

Use this for operational ergonomics while avoiding workflow orchestration semantics.

## SpecKit specify prompts

### Prompt 1: Transition explainability

```text
/speckit.specify Add transition explainability features to StateMachineLibrary. Consumers should be able to ask a state machine definition what would happen for a given current state and event without executing transition behaviors or mutating runtime state. The preview result should explain whether a transition would be permitted, which transition would be selected, which guards were evaluated, which guards passed or failed, the resulting target state when known, and why an event is not permitted. The feature should also improve structured denial diagnostics for actual transition attempts so callers can distinguish no matching event, terminal state, failed guard, invalid current state, validation failure, and ambiguous transition cases. Add a bounded way to handle multiple matching transitions deterministically, such as validation failure by default with optional explicit priority metadata or equivalent conflict-resolution semantics. Preserve existing transition execution behavior unless users opt into the new explainability/conflict behavior. Core must remain dependency-light and must not add logging, DI, telemetry, hosting, persistence provider, or visualization dependencies.
```

### Prompt 2: Graph analysis and definition versioning

```text
/speckit.specify Add graph analysis and definition versioning support to StateMachineLibrary Core. Consumers should be able to analyze a state machine definition for unreachable states, dead-end non-terminal states, terminal reachability, unused or never-permitted events, and shortest paths between states. Consumers should also be able to compare two definitions and receive a structured diff describing added, removed, and changed states, events, transitions, guard display names, transition kinds, terminal markings, and metadata. For enum-backed state or event models, provide optional validation that reports enum values that are undeclared or intentionally ignored. The feature should improve design-time safety and migration review while keeping graph rendering and persistence migrations out of Core scope. Existing validation and graph export behavior must remain compatible.
```

### Prompt 3: Testing helper package

```text
/speckit.specify Add an optional StateMachineLibrary.Testing package that provides fluent test helpers for consumers of StateMachineLibrary. The package should make it easy to assert that events are permitted or denied from a state, transitions result in expected states and outcomes, validation findings match expectations, graph analysis has no unreachable or dead-end states, and observers receive expected lifecycle notifications. The helpers should produce clear assertion failure messages and support async transition execution. The package should be optional and intended for test projects only. Core must not depend on the testing package or any test framework-specific dependencies unless those dependencies are isolated in adapter-specific testing packages.
```

### Prompt 4: Source generator ergonomics and validation

```text
/speckit.specify Improve StateMachineLibrary.SourceGenerators with stronger generated ergonomics and build-time validation. For declarative state machine definitions, generate strongly typed convenience helpers for applying declared events where possible, expose generated metadata useful to tests and documentation, and report compile-time diagnostics for duplicate transitions, undeclared enum values, unreachable states, dead-end non-terminal states, terminal reachability issues, invalid event references, and ambiguous transition declarations. Support generated graph assets or graph export helpers for declarative machines without requiring runtime visualization dependencies. Existing generated Core definitions must remain compatible, deterministic, and equivalent to fluent definitions.
```

### Prompt 5: Graph rendering adapters

```text
/speckit.specify Add optional graph rendering adapter packages for StateMachineLibrary graph export data. Consumers should be able to render an existing Core graph export into Mermaid, Graphviz DOT, and PlantUML text formats for documentation, diagrams, and CI artifacts. Rendering packages must consume Core introspection data but Core must remain renderer-neutral and must not depend on visualization packages. Renderers should support stable node IDs, readable labels, terminal-state styling, optional metadata display, and deterministic output suitable for snapshot tests. The feature must not include hosted services, UI rendering, browser integration, or image generation as part of Core.
```

### Prompt 6: Persistence provider adapters

```text
/speckit.specify Add optional persistence provider adapter packages on top of the existing StateMachineLibrary.Persistence contracts. Provide consumer-friendly providers for common storage scenarios such as EF Core, Redis, and file or in-memory testing storage while preserving the current provider-neutral persistence abstractions. Providers should support loading and saving state snapshots, optimistic concurrency or equivalent conflict detection when the storage backend supports it, clear persistence outcomes, and compatibility with apply-and-persist coordination. Core must not depend on any persistence provider, database client, retry library, hosting integration, or application startup behavior. The feature must not introduce event sourcing or workflow orchestration.
```

### Prompt 7: Application integration adapters

```text
/speckit.specify Add optional application integration adapters for StateMachineLibrary. Provide a Microsoft.Extensions.DependencyInjection integration package that lets applications register named or typed state machine definitions, runtimes, observers, and optional persistence coordination without changing Core APIs. Provide a Microsoft.Extensions.Logging adapter that converts Core transition observations into structured log messages with configurable filtering and event IDs. These integrations must be optional packages; Core must not depend on dependency injection, logging, hosting, telemetry, or application startup infrastructure. Consumers remain responsible for application lifetime, exporter setup, persistence provider selection, and runtime ownership.
```

### Prompt 8: Runtime convenience APIs

```text
/speckit.specify Add runtime convenience APIs to StateMachineLibrary for snapshotting and batch event application. Consumers should be able to export a runtime snapshot containing the current state and relevant runtime metadata, then import or restore that snapshot where appropriate without coupling Core to a storage provider. Consumers should also be able to apply a sequence of events and receive an ordered list of transition outcomes, with clear behavior for stopping on first failure versus continuing after denied events. The feature should improve ergonomics for diagnostics, tests, and simple event processing while avoiding workflow orchestration, retries, scheduling, compensation, distributed execution, or event sourcing semantics.
```

## Suggested order

1. Transition explainability.
2. Graph analysis and definition versioning.
3. Testing helper package.
4. Source generator ergonomics and validation.
5. Runtime convenience APIs.
6. Graph rendering adapters.
7. Application integration adapters.
8. Persistence provider adapters.
