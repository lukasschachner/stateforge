# Release Readiness

Release readiness validates package artifacts before any publish process exists. It is intentionally a review flow only: it does not publish to NuGet.org or any other registry.

## Local validation flow

Run the same command categories used by CI:

```bash
dotnet restore StateMachineLibrary.sln
dotnet build StateMachineLibrary.sln --configuration Release --no-restore
dotnet test StateMachineLibrary.sln --configuration Release --no-build
dotnet run --project samples/Core.HierarchySample/Core.HierarchySample.csproj --configuration Release --no-build
dotnet format StateMachineLibrary.sln --verify-no-changes
dotnet pack StateMachineLibrary.sln --configuration Release --no-build --output artifacts/packages
```

Or run the helper script:

```bash
./eng/validate-release.sh
pwsh ./eng/validate-release.ps1
```

The flow must produce Core, SourceGenerators, Persistence, OpenTelemetry, and Visualization renderer `.nupkg`/`.snupkg` artifacts. Package inspection checks metadata, README/license visibility, XML docs, symbols/source support, dependency groups, analyzer placement, and forbidden files.

## Public API snapshot workflow

Release tests compare generated public API snapshots with approved files under `tests/Release.Tests/PublicApi/`:

- `Core.approved.txt`
- `SourceGenerators.approved.txt`
- `Persistence.approved.txt`
- `OpenTelemetry.approved.txt`
- `Visualization.Mermaid.approved.txt`
- `Visualization.Graphviz.approved.txt`
- `Visualization.PlantUML.approved.txt`

If validation fails, review whether the public contract changed intentionally. For intentional changes, update the approved snapshot together with documentation or versioning notes. Do not update snapshots as a mechanical fix for an accidental breaking change.

To regenerate approved snapshots after review:

```bash
UPDATE_PUBLIC_API_SNAPSHOTS=1 dotnet test tests/Release.Tests/Release.Tests.csproj --configuration Release --filter PublicApi
```

## Package boundary checks

`eng/package-boundaries.json` records allowed and forbidden dependency/file categories. Core remains dependency-light. Persistence may depend on Core and remains provider-neutral. SourceGenerators may carry Roslyn compiler dependencies as private build-time assets and must place generated analyzer output under analyzer package paths without runtime dependency leakage.

## CI validation

The CI workflow in `.github/workflows/ci.yml` runs restore, build, test, format verification, and pack, then uploads artifacts for review. A passing CI run means artifacts are ready for inspection, not publication.

## Graph rendering feature notes

Graph rendering adapters are validated by:

- `tests/Visualization.Tests` deterministic renderer behavior and snapshots for Mermaid, Graphviz DOT, and PlantUML.
- `tests/Release.Tests/GraphRenderingSampleTests.cs` sample artifact generation (`.mmd`, `.dot`, `.puml`).
- `tests/Release.Tests/VisualizationPackageBoundaryTests.cs` optional package boundary constraints.
- `tests/Release.Tests/VisualizationPublicApiSnapshotTests.cs` approved public API snapshots.

## Sample validation

Runnable samples cover fluent runtime usage, hierarchical state modeling, source-generator declarations, graph export/introspection, optional graph rendering adapters, and provider-neutral persistence contracts. These samples must stay aligned with README and docs examples and must not introduce workflow orchestration, event sourcing, parallel regions/history states, database providers, hosted services, dependency injection integrations, browser rendering, or image generation.

## History-state release coverage

Release validation includes Core tests for history restore/fallback behavior, commit-only history updates, terminal restoration compatibility, validation diagnostics, graph export metadata, renderer consumption, sample execution, and public API snapshot approval.

## Completion Transition Release Readiness

Before release, validate completion transitions with focused Core execution/validation/introspection tests, visualization rendering tests, public API snapshots, `dotnet format StateMachineLibrary.sln --verify-no-changes`, full `dotnet test StateMachineLibrary.sln`, and `dotnet pack StateMachineLibrary.sln --configuration Release --output artifacts/packages`.

## Source Generator Hierarchy and Regions Release Readiness

Advanced source-generator declarations require focused validation in `tests/SourceGenerators.Tests` for:

- hierarchy/history emission, including `InitialChild`, `ChildOf`, `WithHistory`, and `Terminal` builder output;
- parallel composite and named-region emission, including membership, regional terminals, and deterministic declaration-order region output;
- byte-stable generated source for repeated advanced declaration generation;
- advanced diagnostics `SMG009`-`SMG015` for duplicate explicit regions, missing regional initials, duplicate sibling membership, unknown owners, unsupported history modes, invalid role combinations, and invalid region declarations;
- compatibility coverage proving existing flat attribute and compact DSL declarations keep their generated `Definition`/`CreateDefinition` members and do not emit advanced builder calls;
- renderer-neutral graph/metadata assertions proving generated advanced definitions do not introduce Mermaid, Graphviz, PlantUML, image-rendering, or visual-designer behavior.

Release review must include `tests/Release.Tests/PublicApi/SourceGenerators.approved.txt` because declaration-model records and diagnostic descriptors are part of the generator assembly surface, plus runnable sample validation for `samples/SourceGenerators.Sample`, including the `GeneratedAdvancedOrderMachine` validation and parallel-region runtime path.
