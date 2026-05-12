# Speckit Prompts for Advanced State Machine Features

Use these as separate `/speckit.specify` prompts, one feature at a time.

## Recommended order

1. Entry/exit/transition actions
2. Hierarchical/composite states
3. History states
4. Parallel regions

---

## Prompt 1 — Entry/Exit/Transition Actions

```text
Add first-class state machine action semantics to Core.

Feature scope:
- Support synchronous and asynchronous entry actions for states.
- Support synchronous and asynchronous exit actions for states.
- Support synchronous and asynchronous transition actions.
- Preserve existing plain FSM APIs and behavior for users who do not configure actions.
- Actions execute only during runtime transition application, never during validation, introspection, graph export, or rendering.
- Action execution order must be deterministic:
  1. transition starts
  2. source state exit actions
  3. transition actions
  4. target state entry actions
  5. state is committed
  6. observers receive completed/outcome notifications
- Define exact failure/cancellation behavior if an action throws or cancellation is requested.
- Preserve no-observer and existing observer semantics.
- Add validation for invalid action configuration.
- Extend introspection/graph export with non-executable action summaries only.
- Do not add dependency injection, hosting, workflow orchestration, background services, persistence providers, or external configuration systems.
- Include tests, docs, samples, public API snapshots, and release validation updates.
```

---

## Prompt 2 — Hierarchical / Composite States

```text
Add hierarchical state machine support to Core as an advanced but optional modeling capability.

Feature scope:
- Support composite parent states with nested child states.
- Support an initial child state for each composite state.
- Support transitions between leaf states, parent states, and across hierarchy boundaries.
- Preserve existing flat FSM behavior and APIs.
- Define deterministic transition resolution:
  - leaf-level transitions take precedence over ancestor transitions
  - parent transitions may handle events not handled by active child states
  - ambiguous transitions must fail validation
- Define deterministic entry/exit ordering across hierarchy boundaries.
- Define terminal/completion behavior for nested states.
- Add validation for:
  - invalid parent/child references
  - hierarchy cycles
  - missing initial child states
  - unreachable nested states
  - ambiguous transition resolution
- Extend runtime to track active leaf state and active ancestor path.
- Extend introspection and graph export with parent-child relationships, composite state indicators, initial child markers, and hierarchy-aware transition metadata.
- Keep Core free of visualization/rendering dependencies.
- Update optional graph rendering adapters only through graph export data, not by adding rendering concepts to Core.
- Do not add parallel regions, history states, workflow orchestration, persistence providers, hosted services, or external DSL/configuration systems in this feature.
- Include tests, docs, samples, public API snapshots, and release validation updates.
```

---

## Prompt 3 — History States

```text
Add history-state support for hierarchical state machines.

Feature scope:
- Support shallow history for composite states.
- Support deep history only if it can be specified without ambiguity; otherwise explicitly defer deep history.
- History states restore the last active child state when re-entering a composite state.
- Define fallback behavior when no history exists yet.
- Define how history interacts with terminal child states.
- Define how history interacts with entry/exit actions and transition actions.
- Require hierarchical state support to exist before this feature.
- Preserve existing flat FSM behavior.
- Add validation for invalid history configuration and ambiguous history targets.
- Extend runtime state tracking to retain history per composite state.
- Extend introspection/graph export with history-state markers and fallback targets.
- Keep Core renderer-neutral and dependency-light.
- Do not add persistence providers, event sourcing, parallel regions, hosted services, or workflow orchestration.
- Include tests, docs, samples, public API snapshots, and release validation updates.
```

---

## Prompt 4 — Parallel Regions

```text
Add orthogonal parallel region support for composite states.

Feature scope:
- Support composite states containing multiple named parallel regions.
- Each region has its own initial state and active state.
- Runtime may have multiple active leaf states, one per active region.
- Define event dispatch semantics across regions:
  - deterministic region ordering
  - whether one event may trigger transitions in multiple regions
  - how conflicting transitions are detected and handled
- Define completion behavior when all regions reach terminal states.
- Define entry/exit ordering across parent states and regions.
- Preserve existing flat and hierarchical FSM behavior.
- Add validation for:
  - missing initial states per region
  - duplicate region names
  - transitions crossing illegal region boundaries
  - ambiguous event handling
  - unreachable states
- Extend introspection/graph export with region identity, parallel-region metadata, and active-state shape.
- Keep visualization adapters optional and consume only graph export data.
- Do not add workflow orchestration, hosted services, persistence providers, event sourcing, or image rendering.
- Include tests, docs, samples, public API snapshots, and release validation updates.
```
