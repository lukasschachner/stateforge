# Dependency Audit

## Scope

- Feature or release: Repository baseline and release-capable NuGet package set
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-13
- Package ecosystems involved: NuGet; GitHub Actions marketplace actions; npm only for local Pi/Spec Kit tooling and not as a shipped product dependency

## Dependencies Changed

- Added: None in this baseline document.
- Updated: None in this baseline document.
- Removed: None in this baseline document.

Feature-level plans that add, update, or remove dependencies must update this document or add a dated section referencing the relevant spec.

## Per-Dependency Review

| Name | Version | Source | Maintained? | Known critical CVEs? | License OK? | Decision |
|------|---------|--------|-------------|----------------------|-------------|----------|
| .NET SDK / runtime | `10.0.x` target | Microsoft | Yes | Review before release | Yes for intended use | Approved baseline platform |
| Microsoft.CodeAnalysis.CSharp / Roslyn | See `src/SourceGenerators/SourceGenerators.csproj` | NuGet | Yes | Review before release | Yes for analyzer/source-generator use | Approved build/analyzer dependency; keep private to SourceGenerators package |
| xUnit and test tooling | See test project files | NuGet | Yes | Review before release | Yes for test use | Approved test-only dependency |
| GitHub Actions `actions/checkout` | `v4` | GitHub Actions marketplace | Yes | Review before release | Yes for CI use | Approved CI action; pinning hardening may be added later |
| GitHub Actions `actions/setup-dotnet` | `v4` | GitHub Actions marketplace | Yes | Review before release | Yes for CI use | Approved CI action; pinning hardening may be added later |
| GitHub Actions `actions/upload-artifact` | `v4` | GitHub Actions marketplace | Yes | Review before release | Yes for CI use | Approved CI action; review artifact retention/exposure settings before public release |
| GitHub Actions `actions/download-artifact` | `v4` | GitHub Actions marketplace | Yes | Review before release | Yes for CI/release use | Approved release workflow action |
| GitHub Actions `actions/attest-build-provenance` | `v2` | GitHub Actions marketplace | Yes | Review before release | Yes for supply-chain provenance | Approved for release provenance attestation |
| CycloneDX .NET tool (`cyclonedx`) | `6.2.0` | NuGet | Yes | Review before release | Yes for SBOM generation | Approved release-time SBOM tool |

## Lock Files and Provenance

- Lock files updated and committed: No NuGet lock file policy is recorded yet.
- Hashes / integrity values verified: Not yet recorded for NuGet dependencies or GitHub Actions.
- Build provenance recorded (where applicable): Release workflow records provenance attestations for package and SBOM artifacts via `actions/attest-build-provenance`.

## Automation Posture

Static dependency audits are supplementary, not a replacement for continuous monitoring. Current baseline:

- Renovatebot or Dependabot configured for this repository: Renovate is configured in `renovate.json`.
- Automated update PRs reviewed and triaged on a defined cadence: Renovate grouping/rate controls are configured; maintainer review cadence should continue as an operational process.
- SBOM produced by CI for each release artefact: yes in `.github/workflows/release.yml` (`dotnet CycloneDX ...` output to `artifacts/sbom/`).
- Dependency Track (or equivalent) ingests CI-built SBOMs for continuous CVE monitoring: no evidence found. Decide whether to adopt Dependency Track or equivalent CVE monitoring before public release.
- OpenSSF Scorecard reviewed for high-impact public dependencies: no evidence found. Run once the repository/package posture is public or before a major public release.

## License Compliance

- Outbound licence compatibility checked: Baseline package and test dependencies should be reviewed before public release; no incompatible license is currently recorded.
- Copyleft obligations recorded (where applicable): None currently recorded.

## Follow-Up

- Open risks:
  - VEX records and advisory triage cadence are not yet populated for a real public release.
  - Signing certificate lifecycle/rotation and secure recovery process must be operated and periodically tested.
  - GitHub Actions are version-pinned by major version, not immutable SHA.
- Required mitigations and owners: Maintainers should operate VEX issuance per release, maintain certificate lifecycle controls, and consider SHA-pinning release-critical GitHub Actions before public package publication.
- Next audit trigger: dependency change, release workflow change, public NuGet release candidate, or security advisory affecting .NET/Roslyn/test/build dependencies.

## Feature Audit: Completion Transitions (2026-05-12)

- Runtime dependency change: none. Core completion-transition implementation uses existing Core definition/runtime/validation/introspection components only.
- Package-boundary expectation: Core remains dependency-light with no new `Microsoft.Extensions.*`, hosting, persistence, workflow, rendering, network, or crypto dependency.
- Re-audit trigger: revisit if completion-transition follow-up work adds packages, generated artifacts, package metadata changes, or release workflow changes.

## Feature Audit: Source Generator Hierarchy and Regions (2026-05-12)

- Dependency change: none. The implementation extends the existing Roslyn source-generator package and tests without adding NuGet, CI action, runtime, renderer, persistence, telemetry, hosting, network, or crypto dependencies.
- Package-boundary expectation: SourceGenerators keeps Roslyn as a private build/analyzer asset and emits calls into Core APIs; Core still has no dependency on SourceGenerators.
- Re-audit trigger: revisit if generated declaration support later adds a textual parser package, external files, analyzer packaging changes, or release workflow changes.

## Feature review: 016-transition-conflict-diagnostics

- Runtime dependency change: none. Core diagnostics use existing BCL/Core definition, validation, and execution types only.
- Package-boundary expectation: Core remains dependency-light with no logging, UI, renderer, OpenTelemetry, persistence, hosting, workflow, network, or crypto dependency added.
- Validation evidence: package-boundary release tests should continue to pass; public API snapshot changed intentionally for additive diagnostic types/properties.

## Feature Audit: Runtime Graph Overlays (2026-05-13)

- Runtime dependency change: none. Core overlay export uses existing BCL/Core definition, validation, active-shape, and graph export types only.
- Package-boundary expectation: Visualization packages consume `DefinitionGraph.RuntimeOverlay` only; Core remains independent of Mermaid, Graphviz, PlantUML, logging, telemetry, persistence, hosting, workflow, network, and crypto dependencies.
- Re-audit trigger: revisit if future overlay work adds live streaming, serialization, hosted polling, renderer/image dependencies, persistence providers, or package-boundary changes.

## Feature Planning: Fluent Region Builders (2026-05-13)

- Dependency change expected: none. The feature should use existing Core definition, validation, introspection, testing, documentation, and sample infrastructure only.
- Package-boundary expectation: Core remains dependency-light with no logging, UI, renderer, OpenTelemetry, persistence, hosting, workflow, network, crypto, source-generator, or external parser dependency added.
- Re-audit trigger: revisit if planning introduces a new package, build tool, analyzer dependency, CI action, renderer dependency, source-generator syntax support, or release workflow change.

## Feature Implementation: Fluent Region Builders (2026-05-13)

- Dependency change: none. The implementation uses existing Core builder/validation/introspection/runtime code, xUnit tests, docs, and samples only.
- Package-boundary expectation: Core remains dependency-light with no logging, UI, renderer, OpenTelemetry, persistence, hosting, workflow, network, crypto, source-generator, or external parser dependency added.
- Validation evidence: focused Core tests and release public API/package validation remain the acceptance gates for the additive public builder surface.

## Feature Implementation: Parallel Regions Documentation and Sample (2026-05-13)

- Dependency change: none. The new sample references `src/Core/Core.csproj` only and adds no NuGet package, build tool, CI action, renderer dependency, hosting dependency, persistence provider, network client, crypto library, or source-generator dependency.
- Package-boundary expectation: Core remains dependency-light; the sample demonstrates renderer-neutral graph metadata without referencing Mermaid, Graphviz, PlantUML, browser tooling, image rendering, persistence providers, hosted services, workflow orchestration, or external services.
- Validation evidence: release tests run the sample and assert stable active-region, completion, graph metadata, invalid diagnostic, and completion-label output.

## Feature Planning: Transition Preview Diagnostics (2026-05-13)

- Dependency change expected: none. The feature should use existing Core definition/runtime/validation/introspection, test, documentation, and release-validation infrastructure only.
- Package-boundary expectation: Core remains dependency-light with no logging, dependency-injection, hosting, persistence-provider, telemetry-exporter, workflow-orchestration, visualization-rendering, network, crypto, serializer, or external parser dependency added.
- Re-audit trigger: revisit if planning introduces a new package, build tool, analyzer dependency, CI action, renderer dependency, persistence/telemetry provider, serialization dependency, or release workflow change.

## Feature Implementation: Transition Preview Diagnostics (2026-05-13)

- Dependency change: none. The implementation uses existing Core definition/runtime/validation components, BCL collections, xUnit tests, documentation, and release-test infrastructure only.
- Package-boundary expectation: Core remains dependency-light with no logging, dependency-injection, hosting, persistence-provider, telemetry-exporter, workflow, visualization-rendering, network, crypto, serializer, or external parser dependency added.
- Validation evidence: Core package-boundary tests and `src/Core/Core.csproj` review remain the release gate; public API snapshot changed intentionally for additive preview/diagnostic contracts.

## Feature Planning: Source Generator Validation (2026-05-13)

- Dependency change expected: none. The feature should extend the existing source-generator package, existing Roslyn private build-time assets, tests, documentation, and release-validation infrastructure.
- Package-boundary expectation: SourceGenerators keeps Roslyn dependencies private to build/analyzer use, emits compatible Core definitions, and does not add runtime visualization, renderer, network, hosting, persistence-provider, serializer, crypto, logging, or dependency-injection dependencies.
- Re-audit trigger: revisit if planning introduces a new NuGet package, textual parser, analyzer packaging change, generated artifact ingestion step, CI action, visualization dependency, release workflow change, or package-boundary expansion.

## Source generator validation dependency evidence (2026-05-13)

No new package dependency was added. Roslyn remains a private analyzer dependency and SourceGenerators has no runtime visualization dependency.
