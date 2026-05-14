# Release Readiness

Release readiness validates package artifacts before publication. The standard local helper flow remains review-only, while the dedicated release workflow handles secure publication controls.

## Local validation flow

Run the same command categories used by CI:

```bash
dotnet restore StateForge.sln
dotnet build StateForge.sln --configuration Release --no-restore
dotnet test --solution StateForge.sln --configuration Release --no-build
dotnet run --project samples/Core.HierarchySample/Core.HierarchySample.csproj --configuration Release --no-build
dotnet format StateForge.sln --verify-no-changes
dotnet pack StateForge.sln --configuration Release --no-build --output artifacts/packages
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

Structured transition conflict diagnostics intentionally add Core public API in `StateForge.Core.Diagnostics` and
additive `ConflictDiagnostics` properties on validation/runtime result types. Release review should confirm those
additions remain dependency-light, renderer-neutral, and compatible with existing readable summaries before accepting the
Core snapshot update.

To regenerate approved snapshots after review:

```bash
UPDATE_PUBLIC_API_SNAPSHOTS=1 dotnet test --project tests/Release.Tests/Release.Tests.csproj --configuration Release --filter PublicApi
```

## Package boundary checks

`eng/package-boundaries.json` records allowed and forbidden dependency/file categories. Core remains dependency-light. Persistence may depend on Core and remains provider-neutral. SourceGenerators may carry Roslyn compiler dependencies as private build-time assets and must place generated analyzer output under analyzer package paths without runtime dependency leakage.

## CI validation

The CI workflow in `.github/workflows/ci.yml` runs restore, build, test, format verification, and pack, then uploads artifacts for review. A passing CI run means artifacts are ready for inspection, not publication.

## Secure release workflow

The release workflow in `.github/workflows/release.yml` adds secure-release controls for tagged/manual releases:

- dependency vulnerability gate (`dotnet list ... --vulnerable --include-transitive --format json` + `eng/check-vulnerabilities.py`);
- CycloneDX SBOM generation to `artifacts/sbom/StateForge.cdx.json`;
- author signing of `.nupkg` artifacts via `dotnet nuget sign` using environment-scoped signing certificate secrets (`NUGET_SIGN_CERT_PFX_B64`, `NUGET_SIGN_CERT_PASSWORD`) and RFC3161 timestamping (see `docs/security/signing.md`);
- signature verification of signed `.nupkg` artifacts via `dotnet nuget verify --all`;
- provenance attestation for signed package and SBOM artifacts via `actions/attest-build-provenance`;
- gated NuGet publication through protected `nuget-prod` environment approval and environment-scoped secrets.

## Runtime graph overlay feature notes

Runtime graph overlays are validated by focused Core tests for flat, hierarchical, parallel, side-effect-free, option-validation, invalid-shape, and accessor-backed export behavior. Visualization tests cover default ignore behavior plus opt-in Mermaid, Graphviz, and PlantUML overlay comments/hints. Public API snapshot review is required for additive Core overlay contracts and renderer `RenderRuntimeOverlay` options.

## Graph rendering feature notes

Graph rendering adapters are validated by:

- `tests/Visualization.Tests` deterministic renderer behavior and snapshots for Mermaid, Graphviz DOT, and PlantUML.
- `tests/Release.Tests/GraphRenderingSampleTests.cs` sample artifact generation (`.mmd`, `.dot`, `.puml`).
- `tests/Release.Tests/VisualizationPackageBoundaryTests.cs` optional package boundary constraints.
- `tests/Release.Tests/VisualizationPublicApiSnapshotTests.cs` approved public API snapshots.

## Sample validation

Runnable samples cover fluent runtime usage, transition preview and denial diagnostics documentation, hierarchical state modeling, parallel-region modeling, source-generator declarations, graph export/introspection, optional graph rendering adapters, provider-neutral persistence contracts, and an interactive API/frontend runtime showcase. The validated inventory includes `samples/Core.ParallelRegionsSample`, which prints active-region shape, completion status, graph region metadata, and a deterministic invalid-model diagnostic, plus `samples/Interactive.ApiFrontendSample`, which is release-validated via deterministic `--smoke-test` API script output. Samples must stay aligned with README and docs examples and must not introduce workflow orchestration, event sourcing, database providers, or image generation. The interactive sample's ASP.NET host and static frontend are demo infrastructure only and must not imply Core package hosting or browser dependencies. Parallel-region examples are limited to Core FSM definition/runtime semantics and must not imply concurrent regional action scheduling or persistence provider behavior.

## Fluent region builder release coverage

The additive Core fluent region builder APIs require focused Core coverage for nested region blocks, region-scoped initial/state/terminal helpers, old-style/new-style/mixed compatibility, validation diagnostics for blank names and conflicting membership, introspection/graph equivalence, and runtime progression through independent regions. Release review must inspect the Core public API snapshot diff for the new builder type and overloads before accepting `tests/Release.Tests/PublicApi/Core.approved.txt` updates.

## History-state release coverage

Release validation includes Core tests for history restore/fallback behavior, commit-only history updates, terminal restoration compatibility, validation diagnostics, graph export metadata, renderer consumption, sample execution, and public API snapshot approval.

## Completion Transition Release Readiness

Before release, validate completion transitions with focused Core execution/validation/introspection tests, visualization rendering tests, public API snapshots, `dotnet format StateForge.sln --verify-no-changes`, full `dotnet test --solution StateForge.sln`, and `dotnet pack StateForge.sln --configuration Release --output artifacts/packages`.

## Source Generator Hierarchy and Regions Release Readiness

Advanced source-generator declarations require focused validation in `tests/SourceGenerators.Tests` for:

- hierarchy/history emission, including `InitialChild`, `ChildOf`, `WithHistory`, and `Terminal` builder output;
- parallel composite and named-region emission, including membership, regional terminals, and deterministic declaration-order region output;
- byte-stable generated source for repeated advanced declaration generation;
- advanced diagnostics `SMG009`-`SMG015` for duplicate explicit regions, missing regional initials, duplicate sibling membership, unknown owners, unsupported history modes, invalid role combinations, and invalid region declarations;
- compatibility coverage proving existing flat attribute and compact DSL declarations keep their generated `Definition`/`CreateDefinition` members and do not emit advanced builder calls;
- renderer-neutral graph/metadata assertions proving generated advanced definitions do not introduce Mermaid, Graphviz, PlantUML, image-rendering, or visual-designer behavior.

Release review must include `tests/Release.Tests/PublicApi/SourceGenerators.approved.txt` because declaration-model records and diagnostic descriptors are part of the generator assembly surface, plus runnable sample validation for `samples/SourceGenerators.Sample`, including the `GeneratedAdvancedOrderMachine` validation and parallel-region runtime path.

## Source generator validation evidence

Source generator release readiness includes focused generator diagnostics/helper/metadata tests, public API snapshot review for the analyzer assembly, sample execution, and package-boundary checks confirming Roslyn remains private and no runtime visualization dependency is introduced.

## Application integration adapters

- Added optional `StateForge.DependencyInjection` and `StateForge.Logging` packages.
- Release validation includes public API snapshots and package-boundary rules for both packages.
- Core remains dependency-light and does not reference `Microsoft.Extensions.*`.

## EF Core persistence adapter

- Added optional `StateForge.Persistence.EntityFrameworkCore` package and release coverage.
- Release validation includes package-boundary tests, public API snapshot tests, adapter sample existence, and focused adapter test suite coverage.
- Adapter package depends on `Microsoft.EntityFrameworkCore` plus StateForge Core/Persistence only; provider packages remain test/sample-only.
