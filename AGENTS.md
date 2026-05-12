# AGENTS.md

Guidance for AI coding agents working in this repository.

<!-- SPECKIT START -->
For feature-specific implementation context, read the active Spec Kit plan referenced by `.specify/feature.json`, especially when implementing a `/speckit.*` task.
<!-- SPECKIT END -->

## Repository Overview

This is a multi-project .NET state machine library targeting `net10.0` with a `netstandard2.0` source-generator package.

The project is governed by Spec Kit and a project constitution. Treat the library as a finite-state-machine platform with strong boundaries between immutable definitions, runtime execution, validation, introspection, optional visualization, persistence contracts, OpenTelemetry instrumentation, and source generation.

Important top-level files and directories:

- `StateMachineLibrary.sln` — main solution.
- `Directory.Build.props` — common target framework, version, package metadata, documentation XML, deterministic build settings.
- `src/` — packable library projects.
- `tests/` — xUnit test projects and release validation tests.
- `samples/` — runnable reference samples validated by release tests.
- `docs/` — examples, release readiness, security governance, feature notes.
- `eng/` — release validation scripts and package boundary rules.
- `specs/` — Spec Kit feature specifications, plans, contracts, tasks, checklists, and implementation notes.
- `.specify/` — Spec Kit integration, templates, scripts, memory/constitution, presets.
- `.pi/` — Pi prompts/tasks for Spec Kit workflows.
- `.github/workflows/ci.yml` — CI restore/build/test/format/pack pipeline.

## Core Principles

Read `.specify/memory/constitution.md` before substantial work. Key requirements:

- Keep the project rooted in deterministic finite-state-machine semantics.
- Preserve definition/runtime separation.
- Keep Core generic and type-safe over `TState` and `TEvent`.
- Prefer async-first APIs with explicit cancellation semantics.
- Preserve performance and avoid hidden synchronization/allocation costs.
- Keep `StateMachineLibrary.Core` dependency-light with no external runtime dependencies.
- Add tests, docs, and public API snapshots for public behavior changes.
- Consider security governance docs under `docs/security/` for behavior-changing work.

## Project Layout

### Source Projects

Packable projects live under `src/`:

- `src/Core/Core.csproj` — primary FSM package.
  - `Definitions/` — immutable definition model and fluent builders.
  - `Execution/` — runtimes, transition execution, active state, hierarchy, history, parallel regions, completion, observers.
  - `Validation/` — definition and snapshot validators plus validation codes.
  - `Introspection/` — definition introspection, graph export, permitted-event queries.
  - `CorePublicApi.cs` — marker used by release/API snapshot tests.
- `src/Persistence/Persistence.csproj` — provider-neutral persistence contracts and persistent runtime wrappers over Core.
- `src/OpenTelemetry/OpenTelemetry.csproj` — optional instrumentation via Core `ITransitionObserver`.
- `src/SourceGenerators/SourceGenerators.csproj` — Roslyn incremental-style analyzer/generator package targeting `netstandard2.0`; Roslyn dependencies are private build-time assets.
- `src/Visualization.Mermaid/Visualization.Mermaid.csproj` — deterministic Mermaid text renderer for Core graph data.
- `src/Visualization.Graphviz/Visualization.Graphviz.csproj` — deterministic Graphviz DOT text renderer.
- `src/Visualization.PlantUML/Visualization.PlantUML.csproj` — deterministic PlantUML text renderer.

### Test Projects

Test projects live under `tests/`:

- `tests/Core.Tests/` — Core definitions, execution, hierarchy, history, parallel, completion, validation, introspection, observation, actions, concurrency.
- `tests/Persistence.Tests/` — persistence contracts, snapshots, stores, hooks.
- `tests/OpenTelemetry.Tests/` — telemetry observer behavior.
- `tests/SourceGenerators.Tests/` — generator parsing, diagnostics, and generated output behavior.
- `tests/Visualization.Tests/` — renderer determinism, metadata, snapshots, adapter consumption.
- `tests/Release.Tests/` — release validation, package contents/metadata/boundaries, samples, public API snapshots.

### Samples

Samples under `samples/` are runnable reference implementations and are release-tested:

- `Core.FluentSample`
- `Core.ActionsSample`
- `Core.HierarchySample`
- `Core.ObservationSample`
- `Graph.IntrospectionSample`
- `Graph.RenderingSample`
- `Persistence.Sample`
- `OpenTelemetry.InstrumentationSample`
- `SourceGenerators.Sample`

Keep samples aligned with README/docs and avoid introducing unrelated infrastructure or workflow-engine behavior.

### Specs and Governance

Each feature directory under `specs/NNN-feature-name/` may contain:

- `spec.md` — user stories, requirements, edge cases, success criteria.
- `plan.md` — technical context, architecture, structure, constitution/security checks.
- `research.md` — decisions and alternatives.
- `data-model.md` — entities and relationships.
- `quickstart.md` — expected user experience.
- `contracts/` — public/runtime/validation/graph contracts.
- `tasks.md` — executable task breakdown.
- `implementation-notes.md` — validation log and progress notes.
- `checklists/requirements.md` — quality checklist.

When executing Spec Kit tasks, follow the relevant `tasks.md` order and update task checkboxes only for work actually completed.

## Build, Test, Format, Pack

Use repo root as working directory.

Common commands:

```bash
dotnet restore StateMachineLibrary.sln
dotnet build StateMachineLibrary.sln --configuration Release --no-restore
dotnet test StateMachineLibrary.sln --configuration Release --no-build
dotnet format StateMachineLibrary.sln --verify-no-changes
dotnet pack StateMachineLibrary.sln --configuration Release --no-build --output artifacts/packages
```

For quick local iteration, Debug builds/tests are acceptable, but release validation must use Release configuration where scripts require it.

Full release validation scripts:

```bash
./eng/validate-release.sh
pwsh ./eng/validate-release.ps1
```

These scripts restore, build, test, run validated samples, verify formatting, pack packages, and intentionally do **not** publish.

## Public API Snapshots

Public API baselines live in `tests/Release.Tests/PublicApi/*.approved.txt`.

If a public API change is intentional:

1. Review the new public contract.
2. Update snapshots explicitly:

   ```bash
   UPDATE_PUBLIC_API_SNAPSHOTS=1 dotnet test tests/Release.Tests/Release.Tests.csproj --filter PublicApi
   ```

3. Inspect the diff before committing.

Do not update snapshots just to make tests pass without reviewing the public surface.

## Package Boundaries

Package boundary rules live in `eng/package-boundaries.json` and are enforced by release tests.

General rules:

- Core has no external runtime dependencies.
- Persistence depends only on Core and remains provider-neutral.
- OpenTelemetry depends only on Core and uses `System.Diagnostics` instrumentation concepts; no exporters or hosting dependencies.
- Visualization packages depend only on Core graph data and do text rendering only; do not add image generation or renderer tool dependencies.
- SourceGenerators keeps Roslyn dependencies private and build-time only.
- Avoid cross-dependencies among optional packages unless explicitly specified and tested.

## Architecture Notes

### Core Definitions

Key files:

- `src/Core/Definitions/StateMachineDefinition.cs`
- `src/Core/Definitions/StateMachineDefinitionBuilder.cs`
- `src/Core/Definitions/TransitionDefinition.cs`
- `src/Core/Definitions/EventDefinition.cs`
- `src/Core/Definitions/StateDefinition.cs`
- `src/Core/Definitions/MetadataCollection.cs`

Definitions are immutable reusable machine blueprints. Fluent builders construct states, transitions, hierarchy, parallel regions, history, actions, metadata, and completion transitions.

### Runtime Execution

Key files:

- `src/Core/Execution/StateMachineRuntime.cs`
- `src/Core/Execution/ExternalStateMachineRuntime.cs`
- `src/Core/Execution/TransitionExecutor.cs`
- `src/Core/Execution/TransitionMatcher.cs`
- `src/Core/Execution/ConditionEvaluator.cs`
- `src/Core/Execution/TransitionActionRunner.cs`
- `src/Core/Execution/TransitionBehaviorRunner.cs`
- `src/Core/Execution/TransitionOutcome.cs`
- `src/Core/Execution/TransitionContext.cs`
- `src/Core/Execution/ActionExecutionContext.cs`

Runtime APIs are async-first. Transition outcomes are structured and should preserve existing lifecycle semantics: matching, conditions, exit actions, transition actions/behaviors, entry actions, commit, observations, failure/cancellation handling.

Concurrency modes are explicit. Do not imply thread safety without using the provided serialized mode or documented synchronization.

### Hierarchy, History, Parallel Regions, Completion

Important execution files include:

- `HierarchyEntryExitPlanner.cs`
- `HierarchicalTransitionResolver.cs`
- `HierarchyCompletionEvaluator.cs`
- `InitialChildResolver.cs`
- `HistoryTargetResolver.cs`
- `ParallelDispatchPlanner.cs`
- `ParallelTransitionResolver.cs`
- `ParallelCompletionEvaluator.cs`
- `ParallelConflictDetector.cs`
- `CompletionTransitionSelector.cs`
- `CompletionEpisodeTracker.cs`

When changing one of these areas, run focused Core tests plus full solution tests. Be especially careful about entry/exit ordering, active-state shape updates, history snapshots, observer ordering, and public outcome compatibility.

### Validation

Main validator:

- `src/Core/Validation/MachineDefinitionValidator.cs`

Validation is split by concern:

- hierarchy structure/cycles/reachability/initial child/history/ambiguity;
- parallel structure/membership/reachability/transitions/history;
- active state snapshot validation;
- completion transition validation.

Add specific validation codes in `*ValidationCodes.cs` files. Prefer validation findings over late runtime failures when definition errors are statically knowable.

### Introspection and Graph Export

Key files:

- `src/Core/Introspection/DefinitionIntrospection.cs`
- `src/Core/Introspection/DefinitionGraphExporter.cs`
- `src/Core/Introspection/DefinitionGraph.cs`
- `src/Core/Introspection/GraphNode.cs`
- `src/Core/Introspection/GraphEdge.cs`
- `src/Core/Introspection/PermittedEventQuery.cs`

Graph export is renderer-neutral. Optional renderers consume `DefinitionGraph` and must not own FSM semantics.

### Visualization

Renderer packages live under:

- `src/Visualization.Mermaid/Rendering/`
- `src/Visualization.Graphviz/Rendering/`
- `src/Visualization.PlantUML/Rendering/`

Each renderer has canonical ordering, metadata formatting, escaping, options, and a main graph renderer. Preserve deterministic output and update visualization tests/snapshots when output intentionally changes.

### Persistence

Persistence is provider-neutral. Key areas:

- `src/Persistence/Snapshots/`
- `src/Persistence/Storage/`
- `src/Persistence/Execution/`
- `src/Persistence/Hooks/`

Do not add database-specific behavior to Core. Database/provider-specific implementation should be an optional package or downstream consumer code.

### OpenTelemetry

OpenTelemetry integration plugs into Core observation:

- `src/OpenTelemetry/Instrumentation/StateMachineTelemetryObserver.cs`
- `src/OpenTelemetry/Instrumentation/StateMachineTelemetryOptions.cs`
- `src/OpenTelemetry/Instrumentation/TelemetryAttributeFormatter.cs`

Do not add exporter, hosting, logging, or dependency-injection requirements unless explicitly planned in a separate feature.

### Source Generators

Source generator architecture under `src/SourceGenerators/`:

- `Declarations/` — parsing, normalizing, validation.
- `Diagnostics/` — Roslyn diagnostics.
- `Emission/` — generated source output.
- `StateMachineGenerator.cs` — generator entry point.

Keep the generator `netstandard2.0` compatible. Roslyn dependencies should remain private build-time assets.

## Testing Guidelines

Use xUnit. Tests are organized by feature area.

Common patterns:

- Reusable domain enums/records in `*TestDomain.cs`.
- Reusable definition factories in `*TestFixtures.cs`.
- Release/test helpers under `tests/Release.Tests/TestSupport/`.
- Visualization expected output under `tests/Visualization.Tests/Snapshots/`.

Focused commands examples:

```bash
dotnet test tests/Core.Tests/Core.Tests.csproj --filter Completion
dotnet test tests/Core.Tests/Core.Tests.csproj --filter Parallel
dotnet test tests/Visualization.Tests/Visualization.Tests.csproj --filter Rendering
dotnet test tests/Release.Tests/Release.Tests.csproj --filter PublicApi
```

Before considering a feature complete, run:

```bash
dotnet format StateMachineLibrary.sln --verify-no-changes
dotnet test StateMachineLibrary.sln
dotnet pack StateMachineLibrary.sln --configuration Release --output artifacts/packages
```

## Documentation Guidelines

Update docs for user-facing behavior changes:

- `README.md` for top-level feature visibility and examples.
- `docs/examples/*.md` for detailed usage.
- `docs/release-readiness.md` for release-affecting validation changes.
- `tests/Release.Tests/README.md` for release test expectations.
- Feature `implementation-notes.md` for validation logs.

Keep examples small, typed, deterministic, and aligned with sample projects.

## Security Governance

Security docs live under `docs/security/`:

- `security-checklist.md`
- `secure-coding-language-rules.md`
- `dependency-audit.md`
- `supply-chain-evidence.md`
- `asvs-verification.md`
- `cra-applicability.md`
- `msl-applicability.md`
- `spec-retrofit-assessment.md`

For behavior-changing work, consider:

- input validation and diagnostics;
- serialization/deserialization;
- file/network I/O;
- auth/authz/crypto surfaces;
- dependency and package-boundary changes;
- telemetry/logging content;
- supply-chain/release artifact effects.

Record feature-specific security evidence when tasks or plans require it.

## Generated and Local Artifacts

Ignored/local outputs include:

- `bin/`, `obj/`
- `artifacts/`
- `*.nupkg`, `*.snupkg`
- `*.log`, `*.tmp`
- `.env*`
- IDE folders such as `.vscode/`, `.idea/`

Do not commit generated packages or build output unless a task explicitly requires committed test fixtures/snapshots.

## Agent Workflow Recommendations

1. Read this file, `.specify/memory/constitution.md`, and the relevant spec/plan/tasks before making changes.
2. Inspect existing patterns in adjacent files before adding new abstractions.
3. Preserve public API compatibility unless the spec explicitly requires a change.
4. Add or update tests before marking behavior complete.
5. Update public API snapshots only after review.
6. Run focused tests first, then full format/test/pack validation.
7. Keep Core dependency-light and avoid optional package concerns leaking into Core.
8. When using Spec Kit tasks, mark checkboxes only after successful implementation/validation.
9. Summarize validation commands and outcomes in feature implementation notes when present.
10. Be careful with generated artifacts and untracked local infrastructure.
