# Speckit Prompts for Roadmap Features

Use these as separate `/speckit.specify` prompts, one feature at a time. They are derived from `docs/feature-specifications-next.md` and assume the orthogonal parallel-region feature is already implemented or available as the current baseline.

## Recommended order

1. Parallel-region history restore
2. First-class active-state persistence shape
3. Completion events and explicit completion transitions
4. Source generator support for hierarchy and parallel regions
5. Richer transition conflict diagnostics
6. Runtime graph export with active-state overlays
7. Region builder ergonomics
8. Dedicated parallel regions guide and samples

---

## Prompt 1 — Parallel-Region History Restore

```text
Add first-class history restore support for parallel composite states.

Feature scope:
- Allow direct shallow and deep history configuration on a parallel composite state.
- Restore the full active parallel-region shape when re-entering a parallel composite through history, not just a single state or single active path.
- Preserve existing non-parallel history behavior exactly for flat and ordinary hierarchical machines.
- Preserve existing nested history behavior inside individual regions while also supporting history on the owning parallel composite.
- Require every restored parallel composite shape to contain at most one active leaf per region and exactly one effective active leaf for each owned region after fallback rules are applied.
- Keep the feature provider-neutral and persistence-friendly, but do not add a database provider or event sourcing implementation.

User stories and scenarios:
- As a state machine designer, I can enable shallow history on a parallel composite so that re-entering the composite restores each region to its last direct active child or to the region initial state when no history exists.
- As a state machine designer, I can enable deep history on a parallel composite so that re-entering the composite restores each region to its deepest recorded active leaf path.
- As a runtime user, I can leave and later re-enter a parallel composite and receive a complete active-state shape with one active leaf per region.
- As a library consumer, I can inspect recorded parallel history entries for diagnostics or persistence-friendly snapshotting.
- Existing machines with shallow/deep history on ordinary non-parallel composites continue to validate and execute unchanged.

Proposed public API shape to evaluate:
- Add or enable `.WithHistory(HistoryMode.Shallow)` and `.WithHistory(HistoryMode.Deep)` on parallel composite builders.
- Support usage similar to:
  `builder.ParallelComposite(OrderState.Operational).WithHistory(HistoryMode.Deep).Region("Fulfillment", OrderState.WaitingForPick, [OrderState.Packing]).Region("Billing", OrderState.WaitingForPayment, [OrderState.CapturingPayment]);`
- Consider additive runtime/introspection records such as:
  `ParallelHistorySnapshot<TState>` with composite state, history mode, region history entries, and last updated sequence.
  `ParallelRegionHistoryEntry<TState>` with region id, region name, last active leaf state, and last active path.
- Public APIs must be generic over `TState` and remain compatible with existing state and event type-safety rules.

Behavioral requirements:
- Entering a parallel composite through history MUST restore all owned regions.
- If a region has no recorded history, restore that region to its configured region initial state.
- Shallow history MUST restore each region according to existing shallow-history semantics at the region boundary.
- Deep history MUST restore each region's deepest recorded active leaf and active path.
- Restored region entries MUST be ordered by parallel-region declaration order.
- History restore MUST be atomic: failure, validation error, action failure, or cancellation during restore must not expose a partially restored active shape.
- History recording MUST update when active regional leaves change according to existing history timing semantics.
- History restore MUST preserve existing entry/exit action lifecycle, observer ordering, transition outcome, and cancellation behavior.
- A terminal regional state may be recorded and restored according to the same semantics as ordinary history unless validation rules define a safer fallback.
- History restore MUST not introduce concurrent regional action scheduling.

Validation requirements:
- Direct history on a parallel composite becomes valid when the parallel composite itself is valid and has valid regions.
- A parallel composite with history MUST still pass all normal parallel-region validation: region names, initials, terminal states, memberships, legal boundaries, ambiguity, reachability, and completion rules.
- Invalid fallback configuration MUST identify the composite state, region, fallback state, and history mode where applicable.
- Stale or externally supplied history data that references removed states, unknown states, unknown regions, duplicate regions, or invalid active paths MUST be rejected before runtime use.
- Diagnostics MUST be actionable and identify the relevant composite state, region id/name, state, path, and mode whenever known.

Introspection and graph requirements:
- Definition introspection MUST expose whether a parallel composite has no history, shallow history, or deep history.
- Runtime introspection MUST expose recorded region history entries and distinguish them from the current active-state shape.
- Graph export MUST include renderer-neutral parallel-history metadata on history-enabled parallel composites.
- Visualization adapters MAY render metadata as comments, labels, or visual hints, but MUST consume only exported graph metadata and must not infer runtime behavior.

Testing requirements:
- Restore shallow parallel history after leaving and re-entering a composite.
- Restore deep parallel history with nested composites inside two or more regions.
- Restore a shape where one region has history and another falls back to its region initial state.
- Reject history references to unknown states, unknown regions, duplicate region entries, and invalid active paths.
- Verify failure or cancellation during restore does not partially restore only some regions.
- Verify existing non-parallel history tests remain unchanged.
- Verify public API snapshots, docs, samples, and release validation updates are intentional.

Explicit exclusions:
- Do not add event sourcing providers.
- Do not add database-specific persistence implementation.
- Do not add workflow orchestration, hosted services, checkpoint scheduling, or background execution.
- Do not add image rendering behavior to Core.
- Do not change existing flat or non-parallel hierarchical behavior.

Acceptance criteria:
- A direct history-enabled parallel composite restores one valid active leaf per region.
- Missing regional history falls back deterministically to the region initial state.
- Existing history-enabled non-parallel machines pass unchanged.
- Parallel history metadata is available through runtime introspection and graph export.
- Release validation includes documentation, samples, compatibility checks, and public API snapshot coverage.
```

---

## Prompt 2 — First-Class Active-State Persistence Shape

```text
Introduce a first-class serializable active-state snapshot abstraction in Core that can represent flat, hierarchical, and parallel-region active configurations.

Feature scope:
- Provide a Core-level active-state snapshot that is persistence-provider neutral.
- Represent non-parallel single-leaf machines, hierarchical active paths, and parallel active region shapes.
- Support round-trip capture and restore of the active runtime state into a compatible definition.
- Include sequence/version metadata and optional definition identity metadata for optimistic concurrency and diagnostics.
- Preserve existing persistence package boundaries and keep Core free of database-provider dependencies.
- Keep existing single-state persistence APIs available and compatible for non-parallel machines.

User stories and scenarios:
- As a user of a flat state machine, I can capture and restore a simple active-state snapshot without being forced to provide parallel metadata.
- As a user of a hierarchical state machine, I can capture and restore the active leaf plus active ancestor path.
- As a user of a parallel-region state machine, I can capture and restore one active leaf and active path for each active region.
- As an application developer, I can serialize the snapshot using my own storage technology without Core depending on that storage technology.
- As a maintainer, I can validate a supplied snapshot against a definition before creating a runtime to avoid corrupt or incompatible runtime state.

Proposed public API shape to evaluate:
- Add runtime capture API similar to: `ActiveStateSnapshot<TState> snapshot = runtime.CaptureSnapshot();`
- Add restore API similar to: `using var restored = definition.CreateRuntime(snapshot);`
- Consider immutable snapshot types such as:
  `ActiveStateSnapshot<TState>` with kind, single active leaf, active path, owning composite state, region snapshots, sequence, and definition fingerprint.
  `ActiveRegionSnapshot<TState>` with region id, region name, active leaf state, active path, and terminal status.
  `ActiveStateSnapshotKind` for single-leaf, hierarchical, parallel, and any necessary future-compatible categories.
- Snapshot types must be serializable-friendly, immutable or effectively immutable, and generic over `TState`.
- Avoid APIs that require visualization, database, hosting, or event-sourcing packages.

Behavioral requirements:
- Capturing a snapshot MUST be side-effect free and must not dispatch events, run guards, run actions, or notify transition observers as if a transition occurred.
- Snapshot capture MUST reflect a consistent runtime active-state shape at a single sequence point.
- Restoring a snapshot MUST validate all referenced states, active paths, region ids, region names where applicable, terminal status consistency, and definition fingerprint compatibility when available.
- Restoring a parallel snapshot MUST produce one active region entry for each owned region.
- Missing, duplicate, stale, or unknown region entries MUST fail validation before runtime use.
- Flat snapshots MUST remain simple and must not require region metadata.
- Hierarchical snapshots MUST preserve active path ordering from root/composite ancestry to active leaf according to existing hierarchy semantics.
- Parallel snapshots MUST preserve deterministic region declaration order.
- Restore MUST preserve existing action, observer, concurrency, unhandled-event, and transition semantics after the runtime is created.
- Restore failure MUST not create a partially initialized runtime visible to callers.

Persistence package requirements:
- Persistence packages MAY store and retrieve the new active-state snapshot shape.
- Persistence packages MUST NOT depend on visualization packages.
- Existing single-state persistence APIs MUST remain available for machines that do not need active-shape snapshots.
- Provide migration guidance for moving from single-state persistence to active-shape persistence for hierarchical and parallel machines.

Validation and diagnostics requirements:
- Add validation diagnostics for unknown state references, invalid active paths, unknown region ids, missing region entries, duplicate region entries, mismatched owning composite state, invalid terminal flags, unsupported snapshot kind, and incompatible definition fingerprint.
- Diagnostics MUST identify the snapshot kind, state, region id/name, composite state, path segment, and sequence where known.
- Validation APIs should be usable before constructing a runtime from an externally supplied snapshot.

Introspection and graph requirements:
- Runtime introspection and snapshot capture should share a consistent active-state shape vocabulary where possible.
- Graph export remains separate; snapshot types should not introduce renderer-specific concepts.
- If graph export or runtime overlay features consume snapshots in the future, the data model must remain renderer-neutral.

Testing requirements:
- Capture and restore a flat machine snapshot.
- Capture and restore a hierarchical active path.
- Capture and restore a parallel active shape with multiple regions.
- Validate snapshot sequence/version is preserved or updated according to the chosen runtime semantics.
- Reject snapshot with unknown state.
- Reject snapshot with invalid active path.
- Reject snapshot with unknown, missing, or duplicate region entry.
- Verify definition fingerprint mismatch behavior.
- Verify persistence package boundaries and dependency policies remain unchanged.
- Verify public API snapshots, docs, samples, and release validation updates are intentional.

Explicit exclusions:
- Do not add a database provider to Core.
- Do not add event sourcing stream models.
- Do not add long-running workflow checkpoint scheduling.
- Do not add hosted services, background workers, or orchestration engines.
- Do not add visualization rendering dependencies.

Acceptance criteria:
- Parallel active-state shape can be captured, serialized by user code, validated, and restored without losing regional active leaves.
- Flat and ordinary hierarchical machines can use the snapshot abstraction without behavior changes.
- Existing single-state persistence tests remain compatible.
- Snapshot types and migration guidance are documented and included in public API validation.
```

---

## Prompt 3 — Completion Events and Explicit Completion Transitions

```text
Add explicit completion-transition modeling for ordinary composite states and parallel composite states.

Feature scope:
- Add a type-safe way to model completion transitions, such as `.OnCompletion().GoTo(...)`, without requiring users to invent artificial events.
- Support completion transitions for ordinary hierarchical composites.
- Support completion transitions for parallel composites after all owned regions are terminal.
- Preserve existing event-triggered transition behavior and existing completion semantics.
- Define deterministic ordering for completion recognition, guard evaluation, transition actions, entry/exit actions, observer notifications, and conflict handling.
- Expose completion transitions through introspection and renderer-neutral graph export.

User stories and scenarios:
- As a state machine designer, I can transition from an ordinary composite to another state when its child flow reaches completion.
- As a state machine designer, I can transition from a parallel composite only after every region has reached a terminal state.
- As a state machine designer, I can attach guards and actions to completion transitions using the same lifecycle semantics as ordinary transitions.
- As a library consumer, I can inspect completion transitions separately from event-triggered transitions and see completion edges in exported graph data.
- Existing event transitions continue to behave unchanged.

Proposed public API shape to evaluate:
- Add builder API similar to:
  `builder.State(OrderState.Reviewing).InitialChild(OrderState.AuthorReview).OnCompletion().GoTo(OrderState.Approved);`
- Add parallel composite API similar to:
  `builder.ParallelComposite(OrderState.Operational).Region("Fulfillment", OrderState.WaitingForPick, terminalStates: [OrderState.FulfillmentDone]).Region("Billing", OrderState.WaitingForPayment, terminalStates: [OrderState.BillingDone]).OnCompletion().GoTo(OrderState.ReadyToClose);`
- Decide whether completion is represented by an internal sentinel transition trigger, a `CompletionEventDefinition<TEvent>`, or a separate completion transition type.
- Public API must not require users to allocate fake `TEvent` values for completion unless explicitly selected as a compatibility option.

Behavioral requirements:
- Completion transitions MUST be evaluated after the terminal state entry actions that caused completion have completed successfully.
- A parallel composite MUST be considered complete only after every region in that composite is terminal.
- Completion MUST be recognized exactly once per completion episode.
- If a completed composite is left and later re-entered, completion can be recognized again only after a new completion episode according to deterministic rules.
- Completion guards and actions MUST follow existing guard/action lifecycle, cancellation, failure, and observer semantics.
- Completion transitions MUST participate in deterministic transition planning and conflict detection before commit.
- Completion transitions MUST NOT run while some parallel regions are still non-terminal.
- If a completion transition exits a composite, regional exit ordering for parallel composites MUST remain deterministic.
- Completion processing MUST not introduce timed, scheduled, background, or workflow orchestration behavior.

Validation requirements:
- Validate that completion transitions are declared only from scopes where completion is meaningful: ordinary composites, parallel composites, or other terminal/completion-capable scopes defined by the existing model.
- Validate that completion transition targets are valid states and legal hierarchy/region targets.
- Validate that ambiguous completion transitions are rejected or resolved by an explicit deterministic priority rule.
- At most one unguarded completion transition should be allowed per completion scope unless the design defines deterministic priority rules.
- Guarded completion transitions must have deterministic selection semantics when multiple guards are true.
- Diagnostics MUST identify the completion scope, competing transitions, guards if representable, source, and target.

Introspection and graph requirements:
- Definition introspection MUST list outgoing completion transitions separately from event-triggered transitions or clearly classify their trigger as completion.
- Graph export MUST classify completion edges as `Completion` or an equivalent renderer-neutral edge kind.
- Graph metadata MUST identify completion source scope, target, guard/action summaries if existing graph export supports summaries, and whether the completion source is parallel.
- Documentation MUST explain completion ordering relative to terminal entry actions and regional all-terminal checks.

Testing requirements:
- Ordinary composite completion transition fires after terminal child entry.
- Parallel composite completion transition fires only after all regions are terminal.
- Completion transition with guards selects deterministically.
- Ambiguous completion transitions fail validation or dispatch before partial commit.
- Completion action ordering is deterministic and matches ordinary transition lifecycle where applicable.
- Cancellation/failure before commit and after commit follows existing runtime semantics.
- Completion is recognized exactly once per completion episode.
- Existing event-triggered transitions and unhandled-event behavior remain unchanged.
- Graph export distinguishes completion transitions.
- Public API snapshots, docs, samples, and release validation are updated.

Explicit exclusions:
- Do not add workflow joins beyond finite-state-machine completion semantics.
- Do not add timed, delayed, scheduled, or background completion triggers.
- Do not add hosted services, orchestration engines, persistence providers, or event sourcing.
- Do not add image rendering behavior in Core.

Acceptance criteria:
- Users can model ordinary and parallel composite completion transitions without artificial user events.
- Completion ordering and conflict behavior are deterministic and documented.
- Graph export clearly distinguishes completion transitions.
- Existing event transition behavior remains unchanged.
```

---

## Prompt 4 — Source Generator Support for Hierarchy and Parallel Regions

```text
Extend source generator support so compile-time declarations can express hierarchical states, history, terminal states, parallel composites, named regions, region membership, and regional terminal states.

Feature scope:
- Add source-generator syntax for hierarchical parent/child relationships.
- Add source-generator syntax for initial child states and history settings.
- Add source-generator syntax for parallel composites and named regions.
- Add source-generator syntax for assigning states to regions and marking regional initial and terminal states.
- Emit equivalent builder calls or immutable definitions that validate and execute the same as fluent builder definitions.
- Produce compile-time diagnostics for invalid declarations where statically knowable.
- Keep runtime validation authoritative for guard-dependent or runtime-dependent ambiguity.
- Preserve existing source generator APIs and generated output compatibility where possible.

User stories and scenarios:
- As a user who prefers attributes, I can declare a hierarchical and parallel state model on an enum or supported declaration form.
- As a user who prefers a DSL, I can express parallel composites, regions, regional initials, terminal states, and transitions in source-generated declarations if a DSL is already supported or planned.
- As a developer, I receive compile-time diagnostics for duplicate region names, missing static initial states, and states assigned to multiple regions.
- As a maintainer, I can compare generated source snapshots to ensure generator output is stable.
- Existing source-generated flat state machine definitions continue to compile and run unchanged.

Attribute API sketch to evaluate:
- Support declaration styles similar to:
  `[StateMachine] public enum OrderState { Draft, [Composite(InitialChild = nameof(WaitingForPick))] [ParallelComposite] Operational, [Region(nameof(Operational), "Fulfillment", IsInitial = true)] WaitingForPick, [Region(nameof(Operational), "Fulfillment", IsTerminal = true)] FulfillmentDone, [Region(nameof(Operational), "Billing", IsInitial = true)] WaitingForPayment, [Region(nameof(Operational), "Billing", IsTerminal = true)] BillingDone }`
- Consider attributes for composite, initial child, history mode, parallel composite marker, region membership, regional terminal state, and transition declarations if transitions are source-generated.
- Attribute names and property names should be discoverable, type-safe where possible, and compatible with existing generator conventions.

DSL sketch to evaluate if the project has or adds DSL generator support:
- Support declarations similar to:
  `parallel Operational { region Fulfillment initial WaitingForPick { WaitingForPick -> FulfillmentDone on PickComplete terminal FulfillmentDone } region Billing initial WaitingForPayment { WaitingForPayment -> BillingDone on PaymentComplete terminal BillingDone } }`
- The DSL must generate equivalent definitions to the fluent builder model and must produce actionable diagnostics with source locations.

Behavioral requirements:
- Generated definitions MUST be semantically equivalent to fluent builder definitions for hierarchy, history, terminal states, parallel composites, regions, and region membership.
- Generated code MUST preserve deterministic region order from declaration order or an explicitly documented order.
- Generated definitions MUST validate using the same Core validation rules as hand-written fluent definitions.
- Compile-time diagnostics MUST not replace runtime validation for guard-dependent ambiguity or runtime-dependent transition conflicts.
- Generator output MUST avoid runtime reflection dependencies in Core.
- Generator must preserve existing source-generated machine behavior for users not using advanced hierarchy or parallel syntax.

Validation and diagnostics requirements:
- Compile-time diagnostics for duplicate region names in the same parallel composite when statically knowable.
- Compile-time diagnostics for missing region initial states when statically knowable.
- Compile-time diagnostics for a state assigned to multiple sibling regions when statically knowable.
- Compile-time diagnostics for unknown composite names, unknown region owner names, invalid attribute combinations, unsupported history mode, and terminal states outside their declared region where statically knowable.
- Diagnostics MUST include source locations and actionable messages.
- Runtime validation remains required and authoritative for complete definition validation.

Introspection and graph requirements:
- Definitions produced by the source generator MUST expose the same introspection and graph export metadata as fluent builder definitions.
- Region names, region ids if generated, owner composite state, initial states, terminal states, history settings, and membership must be visible through existing Core introspection APIs.
- Generated source should not embed renderer-specific behavior.

Testing requirements:
- Attribute-based generation for ordinary hierarchy.
- Attribute-based generation for parallel composite with two named regions.
- Attribute-based generation for region initial and terminal states.
- Attribute-based generation for history settings on composites where supported.
- DSL-based generation for parallel composite if DSL support is in scope.
- Generated definition validates and runs through representative event sequences.
- Compile-time diagnostics for duplicate regions, missing initials, duplicate membership, unknown owner, invalid attribute combinations, and invalid terminal declarations.
- Snapshot tests for generated source output.
- Existing source generator tests remain compatible.
- Public API snapshots, analyzer diagnostic ids, docs, samples, and release validation are updated.

Explicit exclusions:
- Do not add runtime reflection dependencies in Core.
- Do not add a separate visual designer.
- Do not change Core runtime semantics while adding generator support.
- Do not require source generators for users of fluent builders.
- Do not add image rendering, workflow orchestration, hosted services, persistence providers, or event sourcing.

Acceptance criteria:
- Source-generated definitions can express the same hierarchy and parallel-region model available through fluent builders.
- Invalid declarations produce stable, actionable compile-time diagnostics where statically knowable.
- Generated definitions validate, run, introspect, and export graph metadata like fluent definitions.
- Existing source generator users remain compatible.
```

---

## Prompt 5 — Richer Transition Conflict Diagnostics

```text
Enhance transition conflict diagnostics with stable structured metadata, especially for parallel-region dispatch and completion conflicts.

Feature scope:
- Add stable conflict categories for validation-time and runtime transition conflicts.
- Include structured participants for each conflict: region, composite, transition, event, source state, target state, and conflict scope where known.
- Preserve deterministic ordering of conflict diagnostics.
- Make structured diagnostics available through validation results and runtime transition outcomes.
- Preserve existing human-readable summaries for compatibility.
- Do not add logging, UI, rendering, or telemetry dependencies to Core.

User stories and scenarios:
- As a library user, when parallel dispatch fails because parent and regional transitions conflict, I can inspect a structured diagnostic that identifies both transitions and the affected regions.
- As a tooling author, I can categorize conflicts without parsing English strings.
- As a maintainer, I can rely on stable diagnostic ordering for repeatable tests and deterministic user output.
- As an existing consumer, I can continue using existing `TransitionDiagnostics.Summary` or equivalent string summaries without breaking changes.

Proposed public API shape to evaluate:
- Add enum values similar to:
  `DuplicateSourceScope`, `ParentRegionalConflict`, `CrossRegionBoundary`, `InvalidPostShape`, `AmbiguousGuardOutcome`, `CompletionConflict`.
- Add typed diagnostic record similar to:
  `TransitionConflictDiagnostic<TState, TEvent>` with kind, event, composite state, region ids, participating transitions, source/target states where useful, and message.
- Consider whether diagnostics should be exposed as strongly typed generic collections, non-generic base diagnostic interfaces, or additive properties on existing diagnostics objects.
- Preserve existing summary/string APIs and avoid source-breaking changes.

Behavioral requirements:
- Conflict diagnostics MUST be stable for the same definition, active state, event, guard outcomes, and completion status.
- Conflict diagnostics MUST be ordered deterministically by existing transition/region/source ordering rules.
- Runtime conflict diagnostics MUST be produced before any partial state change is committed.
- Validation-time and runtime-time diagnostics SHOULD use the same category vocabulary where possible.
- Structured diagnostics MUST not require renderer-specific, logger-specific, or UI-specific data.
- Guard-dependent conflicts that can only be known at runtime MUST include guard outcome context where safely available.
- Completion conflicts MUST identify completion scope and competing completion/event/regional transitions where applicable.

Validation requirements:
- Validation results MUST expose structured conflict diagnostics for statically knowable duplicate source scopes, ambiguous transition selection, illegal region-boundary transitions, invalid hierarchy targets, and completion-transition ambiguity.
- Diagnostics MUST identify source state, target state, event or completion trigger, owning composite, region ids/names, and transition ids if available.
- Existing validation diagnostics must remain compatible; new fields should be additive.

Runtime requirements:
- Runtime transition outcomes MUST expose structured conflict diagnostics when dispatch fails due to conflicts.
- Parent-vs-regional conflicts MUST identify the parent transition and each enabled regional transition.
- Cross-region boundary conflicts MUST identify source and target region ids/names.
- Duplicate source conflicts MUST include all competing transitions from the same source scope.
- Invalid post-shape conflicts MUST identify affected composite/region and the invalid resulting active-state shape.
- Ambiguous guard outcomes MUST identify competing transitions whose guards selected true or equivalent enabled state.

Introspection and graph requirements:
- Conflict diagnostics are separate from static graph export, but transition ids or stable identities used in diagnostics should correspond to introspection/graph transition metadata where feasible.
- Do not add visualization-specific formatting.

Testing requirements:
- Parent-vs-regional conflict includes both parent and regional transitions.
- Cross-region boundary conflict includes source and target region ids/names.
- Duplicate source conflict includes all competing transitions in deterministic order.
- Invalid post-shape conflict identifies affected region or composite.
- Ambiguous guard outcome includes competing enabled transitions.
- Completion conflict includes completion scope and competing completion transitions.
- Existing `TransitionDiagnostics.Summary` or equivalent remains compatible.
- Diagnostic ordering is deterministic across repeated runs.
- Public API snapshots, docs, and release validation are updated.

Explicit exclusions:
- Do not add a logging dependency to Core.
- Do not add UI-specific diagnostic formatting.
- Do not add renderer-specific metadata.
- Do not change transition semantics only to improve diagnostics.
- Do not add workflow orchestration, hosted services, persistence providers, or event sourcing.

Acceptance criteria:
- Conflict outcomes are actionable without parsing strings.
- Validation and runtime conflicts share a stable category vocabulary where feasible.
- Existing diagnostic summaries remain compatible.
- Public API snapshots document all intentional additions.
```

---

## Prompt 6 — Runtime Graph Export with Active-State Overlays

```text
Add runtime-aware graph export that overlays active states, active paths, terminal regions, and completion status on top of existing definition graph data.

Feature scope:
- Add an API to export graph data from a runtime instance, not only from a static definition.
- Mark the active leaf for flat machines.
- Mark the active path for hierarchical machines.
- Mark active leaves per parallel region for parallel machines.
- Mark terminal/completed regions and composite completion status where available.
- Keep the output renderer-neutral and additive.
- Allow visualization adapters to consume overlay metadata without inspecting runtime internals.

User stories and scenarios:
- As a developer debugging a running flat machine, I can export graph data that identifies the current active state.
- As a developer debugging hierarchy, I can export graph data that identifies the active path from parent composites to the active leaf.
- As a developer debugging parallel regions, I can export graph data that identifies the active leaf and terminal status in each region.
- As a visualization adapter author, I can render active-state hints using graph export metadata only.
- Existing static definition graph export remains unchanged unless runtime overlay export is explicitly requested.

Proposed public API shape to evaluate:
- Add runtime API similar to: `GraphExportResult<TState, TEvent> export = runtime.ExportGraph();`
- Consider overloads or options to include/exclude active overlays.
- Consider metadata types such as:
  `GraphActiveStateOverlay<TState>` with active-state shape kind, active leaf state, active regions, active path, completion status, and sequence.
  `GraphActiveRegionOverlay<TState>` with region id, region name, active leaf state, active path if applicable, and terminal status.
- Reuse existing `GraphExportResult<TState,TEvent>` where possible and keep new overlay data additive.

Behavioral requirements:
- Runtime graph export MUST be side-effect free.
- Runtime graph export MUST NOT dispatch events, evaluate guards for side effects, run actions, or mutate history.
- Overlay data MUST reflect the runtime's current active-state shape at a consistent sequence point.
- Non-parallel graph export MUST remain unchanged when callers use static definition export.
- Runtime overlay data MUST be additive and safely ignored by consumers that do not support overlays.
- Parallel overlay region ordering MUST follow region declaration order.
- Terminal/completion status MUST match runtime introspection semantics.
- Export must preserve existing graph export data for states, transitions, hierarchy, regions, and metadata.

Adapter requirements:
- Mermaid, Graphviz, PlantUML, or other optional adapters MAY render active states through comments, labels, styling, or configurable visual hints.
- Adapters MUST consume only graph export data and MUST NOT inspect runtime internals.
- Adapters MUST gracefully ignore overlays if unsupported.
- Adapter behavior must remain optional; Core must not depend on adapter packages.

Validation and diagnostics requirements:
- Runtime export should fail clearly if the runtime active-state shape is invalid or inconsistent, though normal runtime operation should prevent this.
- Diagnostics must identify affected state, region, shape kind, or sequence where possible.
- Export options should reject unsupported option combinations with actionable errors.

Testing requirements:
- Runtime graph export marks active leaf for a flat machine.
- Runtime graph export marks active path for a hierarchical machine.
- Runtime graph export marks active leaves per parallel region.
- Runtime graph export marks terminal/completed regions after regional completion.
- Overlay sequence updates after dispatch.
- Static definition graph export remains unchanged.
- Visualization adapters ignore or render overlays using only graph metadata.
- Runtime export is side-effect free.
- Public API snapshots, docs, samples, and release validation are updated.

Explicit exclusions:
- Do not add image rendering to Core.
- Do not add live streaming graph updates.
- Do not add debugger UI.
- Do not add hosted services or background graph polling.
- Do not add persistence providers or event sourcing.

Acceptance criteria:
- Users can export renderer-neutral graph data that reflects current runtime state.
- Flat, hierarchical, and parallel active shapes are represented accurately.
- Optional adapters can display active-state overlays using graph data only.
- Existing static graph export consumers remain compatible.
```

---

## Prompt 7 — Region Builder Ergonomics

```text
Improve the fluent builder surface for defining parallel regions, region states, terminal states, initial states, and region-scoped transitions with fewer accidental invalid models.

Feature scope:
- Add region-scoped state declarations that automatically assign region membership.
- Add region-scoped terminal-state helpers.
- Add region-scoped initial-state helpers.
- Consider strongly named region handles or builder objects where useful for discoverability and type safety.
- Preserve all existing parallel composite and region builder APIs as source compatible.
- Reduce common invalid membership definitions while keeping validation authoritative.

User stories and scenarios:
- As a state machine designer, I can define a two-region parallel composite with nested region builder blocks instead of manually coordinating membership across separate calls.
- As a user, when I declare a state inside a region builder, that state is automatically assigned to the region.
- As a user, when I mark a terminal state inside a region builder, it is both part of the region and terminal for that region.
- As an existing user, my current fluent builder definitions continue to compile and run unchanged.
- As a user who mixes old and new APIs, I receive clear validation diagnostics for duplicate or conflicting membership.

Proposed public API shape to evaluate:
- Add syntax similar to:
  `builder.ParallelComposite(OrderState.Operational, composite => { composite.Region("Fulfillment", region => { region.Initial(OrderState.WaitingForPick); region.State(OrderState.WaitingForPick).On(OrderEvent.PickStarted).GoTo(OrderState.Packing); region.Terminal(OrderState.FulfillmentDone); }); composite.Region("Billing", region => { region.Initial(OrderState.WaitingForPayment); region.Terminal(OrderState.BillingDone); }); });`
- Consider builder type similar to:
  `ParallelRegionDefinitionBuilder<TState, TEvent>` with `Initial(TState state)`, `State(TState state)`, and `Terminal(TState state)` methods.
- Consider whether `Region` returns a handle that can be reused to configure transitions or metadata while preserving deterministic declaration order.

Behavioral requirements:
- Region-scoped `State` MUST automatically assign membership to the current region.
- Region-scoped `Terminal` MUST mark the state as part of the current region and terminal for that region.
- Region-scoped `Initial` MUST set the region initial state and assign membership if appropriate.
- Region-scoped transitions MUST use existing transition semantics and must not create a separate transition model.
- Declaration order of regions and states MUST remain deterministic and match documented builder behavior.
- Mixing old and new APIs MUST remain supported when definitions are not contradictory.
- Duplicate or conflicting membership remains invalid and MUST be caught by validation.
- Blank region names should be rejected early where possible.

Validation requirements:
- Existing validation remains authoritative for missing initial states, duplicate names, illegal boundaries, ambiguous event handling, unreachable regional states, and duplicate membership.
- Diagnostics for missing or duplicated membership SHOULD suggest region-scoped builders when helpful.
- Builder methods SHOULD fail fast for blank/null region names and clearly invalid local arguments where consistent with existing builder behavior.
- Validation must distinguish between builder ergonomics errors and semantic model errors.

Introspection and graph requirements:
- Definitions produced with ergonomic builders MUST expose the same introspection and graph export metadata as definitions produced with existing APIs.
- No renderer-specific behavior should be introduced.
- Region ids/names, owner composite, initial states, terminal states, and membership must remain stable.

Testing requirements:
- Region-scoped initial state definition assigns membership and validates.
- Region-scoped terminal state definition assigns membership and validates.
- Region-scoped transition definition behaves like existing transitions.
- Two-region sample using block syntax validates and runs.
- Mixed old/new API definitions remain compatible when valid.
- Duplicate membership validation remains effective when APIs are mixed incorrectly.
- Blank region names fail early or validate with clear diagnostics according to chosen convention.
- Existing builder tests remain compatible.
- Public API snapshots, docs, samples, and release validation are updated.

Explicit exclusions:
- Do not remove or break current builder methods.
- Do not add a visual designer.
- Do not change runtime parallel-region semantics.
- Do not add source generator syntax in this feature unless strictly needed for builder types.
- Do not add workflow orchestration, hosted services, persistence providers, event sourcing, or image rendering.

Acceptance criteria:
- Common two-region definitions become shorter and less error-prone.
- Region-scoped declarations automatically handle membership for initial, ordinary, and terminal states.
- Existing fluent API remains source compatible.
- Generated definitions from old and new builder styles behave equivalently.
```

---

## Prompt 8 — Dedicated Parallel Regions Guide and Samples

```text
Create comprehensive user-facing documentation and samples for orthogonal parallel regions.

Feature scope:
- Add a dedicated documentation page for parallel regions.
- Add a standalone sample project for parallel regions if warranted by the repository sample structure.
- Cover valid patterns, invalid patterns, dispatch semantics, conflict behavior, completion, introspection, graph export, history/persistence limitations, and migration from flattened state combinations.
- Ensure docs and samples reflect the implemented public API and current boundaries.
- Add release validation that verifies the guide and sample stay runnable and mention key boundaries.

User stories and scenarios:
- As a new user, I can read one guide and implement a two-region parallel composite without searching across multiple docs pages.
- As an existing user with flattened state combinations, I can understand when and how to migrate to parallel regions.
- As a maintainer, I can run release validation to verify the sample compiles, runs, and demonstrates expected graph metadata.
- As a user, I can see examples of invalid parallel-region models and understand the diagnostics.

Proposed documentation structure:
- Add `docs/examples/parallel-regions.md` or equivalent.
- Add `samples/Core.ParallelRegionsSample/` or extend an existing sample only if that better matches repository conventions.
- Documentation sections should include:
  - What orthogonal regions are.
  - When to use parallel regions.
  - When not to use parallel regions.
  - Defining a parallel composite.
  - Region initial states and terminal states.
  - Region declaration order and deterministic behavior.
  - Same-event multi-region dispatch.
  - Conflict examples and diagnostics.
  - Completion after all regions are terminal.
  - Active-state shape introspection.
  - Graph export and optional visualization adapters.
  - Persistence and history limitations or links to follow-up features.
  - Migration from flattened state combinations.
  - Explicit out-of-scope boundaries: workflow orchestration, hosted services, event sourcing, persistence providers, image rendering.

Sample requirements:
- Include a two-region order processing example, such as Fulfillment and Billing under an Operational parallel composite.
- Include events that advance only one region.
- Include a shared event that advances both regions if supported by the implemented semantics.
- Include completion only after all regions reach terminal states.
- Include active-state shape introspection output.
- Include graph export output that shows region metadata.
- Include at least one validation or conflict example if it can be demonstrated clearly without making the primary sample confusing.
- Console output should be deterministic and suitable for release tests.

Behavioral/documentation requirements:
- Documentation MUST match implemented public APIs, not aspirational APIs.
- Documentation MUST distinguish FSM parallel regions from concurrent execution, background workers, and workflow orchestration.
- Documentation MUST explain that one dispatch may advance multiple independent regions when non-conflicting.
- Documentation MUST explain conflict detection before commit.
- Documentation MUST explain completion occurs only when all regions are terminal.
- Documentation MUST state current limitations for persistence/history if those features are not yet implemented.
- Documentation MUST reference existing graph/introspection docs where appropriate without duplicating every detail.

Testing and release validation requirements:
- Release test verifies the sample builds and runs.
- Release test verifies sample output includes active region information.
- Release test verifies sample output or generated graph data includes region metadata.
- Release test verifies docs mention out-of-scope boundaries.
- Documentation link checks or repository docs validation should include the new page where applicable.
- Existing samples and docs validation remain compatible.

Explicit exclusions:
- Do not add a separate hosted workflow sample.
- Do not add image rendering behavior beyond existing optional visualization adapters.
- Do not implement new Core runtime semantics as part of the documentation feature unless fixing documentation-discovered defects is explicitly scoped.
- Do not add persistence providers, event sourcing, hosted services, or workflow orchestration.

Acceptance criteria:
- Users can implement, validate, run, inspect, and export a two-region parallel composite from the guide alone.
- Docs clearly state supported behavior and unsupported workflow/persistence/history boundaries.
- Sample output is deterministic and covered by release validation.
- Graph metadata and active-state shape concepts are demonstrated in user-facing material.
```
