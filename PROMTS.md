 ## 1. Transition explainability / dry-run API

    /speckit.specify Add transition explainability and dry-run support to StateForge. Consumers should be able to ask a state machine definition or runtime what would happen for a supplied current active state shape and event
  without mutating runtime state or executing entry, exit, transition actions, transition behaviors, observers, persistence hooks, telemetry, or completion side effects. The preview result should explain whether the event is permitted,
  which transition would be selected, which guards would be evaluated, which guards pass or fail, the expected target state or active-state shape when knowable, and why no transition is permitted. Actual transition attempts should expose
  richer structured denial diagnostics for cases such as unknown current state, no matching event, terminal state, failed guards, invalid active shape, ambiguous transitions, and validation conflicts. Preserve existing execution behavior
  by default, keep Core dependency-light, and do not add logging, DI, hosting, persistence-provider, telemetry-exporter, workflow orchestration, or visualization-rendering dependencies.

  ## 2. Graph analysis and definition versioning

    /speckit.specify Add graph analysis and definition versioning support to StateForge Core. Consumers should be able to analyze a state machine definition for unreachable states, dead-end non-terminal states, terminal
  reachability, unused events, never-permitted events, orphaned hierarchy or region nodes, and shortest paths between states where deterministic analysis is possible. Consumers should also be able to compare two definitions and receive a
  structured diff describing added, removed, and changed states, events, transitions, guard display names, transition kinds, terminal markings, hierarchy relationships, parallel-region membership, completion transitions, history
  configuration, and metadata. For enum-backed state or event models, provide optional validation that reports enum values that are undeclared, unused, or explicitly ignored. Keep graph rendering, persistence migration execution, and
  workflow orchestration out of scope. Existing validation, execution, and graph export behavior must remain compatible.

  ## 3. Consumer testing helper package

    /speckit.specify Add an optional StateForge.Testing package that provides fluent test helpers for consumers of StateForge. The package should make it easy to assert permitted and denied events, transition outcomes,
  resulting states, active-state paths, parallel active-region shapes, validation findings, graph analysis results, observer notifications, and expected action/guard behavior. Helpers must support async transition execution and produce
  clear assertion failure messages suitable for test output. The package should be optional and intended for test projects only. Core must not depend on the testing package. Avoid coupling Core to xUnit, NUnit, or MSTest; if framework-
  specific helpers are needed, isolate them in adapter-specific packages or keep the base testing package framework-neutral.

  ## 4. Source generator ergonomics and compile-time validation

    /speckit.specify Improve StateForge.SourceGenerators with stronger generated ergonomics and build-time validation. For declarative state machine definitions, generate strongly typed convenience helpers for applying declared
  events where possible, expose generated metadata useful to tests and documentation, and report compile-time diagnostics for duplicate transitions, undeclared states, undeclared enum values, unreachable states, dead-end non-terminal
  states, terminal reachability issues, invalid event references, invalid hierarchy declarations, invalid parallel-region declarations, ambiguous transitions, and invalid completion/history declarations where this can be determined at
  compile time. Support generated graph metadata or graph export helpers for declarative machines without requiring runtime visualization dependencies. Existing generated Core definitions must remain compatible, deterministic, and
  equivalent to fluent definitions. Keep the generator netstandard2.0-compatible and keep Roslyn dependencies private build-time assets.

  ## 5. Runtime convenience APIs

    /speckit.specify Add runtime convenience APIs to StateForge for snapshotting and batch event application. Consumers should be able to export a runtime snapshot containing the current state or active-state shape, active paths,
  parallel active regions, relevant history metadata, sequence/version information, and definition identity metadata where available. Consumers should be able to restore or initialize runtimes from compatible snapshots with validation
  before use. Consumers should also be able to apply a sequence of events and receive an ordered list of transition outcomes, with explicit options for stopping on first failure, stopping on denied events, or continuing where safe. The
  feature should improve diagnostics, tests, simple event processing, and persistence-friendly usage while avoiding workflow orchestration, retries, scheduling, compensation, distributed execution, database providers, and event-sourcing
  semantics.

  ## 6. Optional application integration adapters

    /speckit.specify Add optional application integration adapter packages for StateForge. Provide a Microsoft.Extensions.DependencyInjection integration package that lets applications register named or typed state machine
  definitions, runtime factories, observers, validation startup checks, and optional persistence coordination without changing Core APIs. Provide a Microsoft.Extensions.Logging adapter that converts Core transition observations, denials,
  failures, and validation findings into structured log messages with configurable filtering, event IDs, scopes, and safe diagnostic content. These integrations must be optional packages; Core must not depend on dependency injection,
  logging, hosting, telemetry exporters, persistence providers, or application startup infrastructure. Consumers remain responsible for application lifetime, exporter setup, persistence provider selection, runtime ownership, and
  concurrency mode selection.