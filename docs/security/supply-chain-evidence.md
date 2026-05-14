# Supply-Chain Evidence

## Context

- System or release: .NET StateForge release-capable package set
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-13
- Released version(s) covered: No public release version covered by this baseline; applies to release-candidate readiness work

## SBOM (Software Bill of Materials)

- Format used (CycloneDX, SPDX): CycloneDX JSON.
- Generator and version: `cyclonedx` .NET local tool (`6.2.0`).
- Storage location of the SBOM artefact: `artifacts/sbom/StateForge.cdx.json` in release workflow artifacts.
- Generated per release artefact set: yes in `.github/workflows/release.yml`.
- Published externally: N/A at baseline because no public package publication is in scope yet; reassess before public release.

## VEX (Vulnerability Exploitability eXchange)

| CVE / advisory | Component | Status | Justification |
|----------------|-----------|--------|---------------|
| None recorded at baseline | N/A | under investigation | Establish CVE triage and VEX recording before public release. |

- Storage location of the VEX statements: `docs/security/vex/` (directory established; per-release records required before/at publication).
- Disclosure cadence: On demand for advisories and with each public release once release publication begins.

## SLSA (Supply-chain Levels for Software Artefacts)

- Targeted SLSA level: L1 minimum where feasible for release candidates; aim for L2+ over time.
- Build platform and isolation: GitHub Actions hosted runners via `.github/workflows/ci.yml` (readiness) and `.github/workflows/release.yml` (release pipeline).
- Provenance generation tool and storage location: `actions/attest-build-provenance@v2` in release workflow for package and SBOM artifacts.
- Signing and verification approach: Author signing in release workflow via `dotnet nuget sign` with protected environment secrets and timestamping; signature verification enforced via `dotnet nuget verify --all`. Operational details are defined in `docs/security/signing.md`.
- Gaps to next level and planned mitigations:
  - Operate signing certificate lifecycle controls (rotation/revocation/recovery) with documented runbooks.
  - Consider pinning GitHub Actions to immutable SHAs for release workflows.
  - Add external publication/retention process for SBOM and VEX artifacts.

## OpenSSF Scorecard

- Applicable (public OSS repository or high-impact external dependency): conditional yes if repository/packages are public OSS or intended for broad external consumption.
- Last Scorecard run date and overall score: Not run.
- Findings reviewed and follow-ups recorded: None yet.

## Build and Distribution Integrity

- CI build provenance recorded: Partial in CI; release workflow generates attestations for artifacts.
- Release artefacts signed: Configured in release workflow for `.nupkg` artifacts before publication.
- Distribution channel verified (registry, store, internal mirror): Release workflow supports NuGet publication through protected `nuget-prod` environment approval and environment-scoped secret.

## Cross-References

- Dependency audit: `docs/security/dependency-audit.md`
- Signing runbook: `docs/security/signing.md`
- ASVS verification (with Level): `docs/security/asvs-verification.md` — current baseline N/A.
- CRA applicability (if release affects EU market reach): `docs/security/cra-applicability.md`

## Follow-Up

- Open risks: immutable action SHA pinning is not yet implemented; VEX records must be produced per public release; signing certificate operational controls must be maintained.
- Required mitigations and owners: Maintainers should issue per-release VEX statements, operate certificate lifecycle controls, optionally add OpenSSF Scorecard automation, and evaluate SHA-pinning for release-critical workflows before broad public package publication.
- Re-evaluation trigger: public release candidate, package publication workflow, dependency changes, supply-chain incident, or repository visibility change.

## Feature Evidence: Completion Transitions (2026-05-12)

- Package/dependency changes: none expected for the feature implementation.
- Release snapshot trigger: Core public API snapshot was updated to reflect new completion-transition APIs and graph trigger metadata.
- Supply-chain impact: no new package sources, build tools, CI actions, signing steps, or distribution channels were introduced.

## Feature Evidence: Source Generator Hierarchy and Regions (2026-05-12)

- Package/dependency changes: none. SourceGenerators remains an analyzer package with existing Roslyn private build-time assets.
- Release snapshot trigger: SourceGenerators public API snapshot requires review because advanced declaration model types, diagnostics, and reporter helpers are exposed from the generator assembly.
- Supply-chain impact: no new package sources, build tools, CI actions, signing steps, external parsers, generated artifact ingestion, or distribution channels were introduced.

## Feature review: 016-transition-conflict-diagnostics

- Package/dependency changes: none expected; no package-boundary expansion.
- Release snapshot trigger: Core public API snapshot updated for `StateForge.Core.Diagnostics` types and additive `ConflictDiagnostics` properties.
- SBOM/VEX/provenance impact: no dependency inventory change; normal release SBOM/provenance/signing evidence remains required before publication.

## Feature Evidence: Runtime Graph Overlays (2026-05-13)

- Package/dependency changes: none expected; no package-boundary expansion.
- Release snapshot trigger: Core public API snapshot requires review for additive runtime overlay/export contracts. Mermaid, Graphviz, and PlantUML snapshots require review for additive `RenderRuntimeOverlay` options.
- SBOM/VEX/provenance impact: no dependency inventory, build tool, CI action, signing, provenance, package source, or distribution-channel change was introduced. Normal release SBOM/VEX/provenance/signing evidence remains required before publication.

## Feature Planning: Fluent Region Builders (2026-05-13)

- SBOM: relevant for release packaging; no dependency inventory change is expected, but normal release SBOM generation remains required before publication.
- VEX: relevant for release governance; no feature-specific VEX statement is expected unless advisory or dependency status changes before release.
- SLSA: relevant for package provenance because the feature changes release-capable public surface evidence; no new build platform, package source, signing step, or provenance mechanism is expected.
- Release snapshot trigger: Core public API snapshot should be reviewed for additive fluent builder surface changes.

## Feature Implementation: Fluent Region Builders (2026-05-13)

- SBOM impact: no dependency additions or package-boundary expansion; existing release SBOM generation remains applicable.
- VEX/advisory impact: no new third-party component requires feature-specific VEX entries.
- SLSA/provenance impact: existing release provenance workflow remains applicable to packages containing the additive Core API.
- Public API snapshot: Core snapshot review is required for `ParallelRegionDefinitionBuilder<TState,TEvent>` and new block overloads before release approval.

## Feature Implementation: Parallel Regions Documentation and Sample (2026-05-13)

- SBOM impact: no dependency inventory change. The new sample references Core only, so existing release SBOM generation remains applicable without feature-specific third-party additions.
- VEX/advisory impact: no new third-party component or vulnerability status requires a feature-specific VEX entry.
- SLSA/provenance impact: existing CI/release provenance remains applicable; the feature adds release-test evidence for the sample and documentation contract but no new build platform, signing step, package source, or distribution channel.
- Release validation impact: `CoreParallelRegionsSampleTests` runs the sample and documentation release tests assert guide boundaries and discoverability.

## Feature Planning: Transition Preview Diagnostics (2026-05-13)

- SBOM: relevant for release packaging; no dependency inventory change is expected, but normal release SBOM generation remains required before publication.
- VEX: relevant for release governance; no feature-specific VEX statement is expected unless advisory or dependency status changes before release.
- SLSA: relevant for package provenance because the feature changes release-capable public behavior/diagnostic evidence; no new build platform, package source, signing step, or provenance mechanism is expected.
- Release snapshot trigger: Core public API snapshot should be reviewed for additive preview and denial diagnostic contracts before release approval.

## Feature Implementation: Transition Preview Diagnostics (2026-05-13)

- SBOM impact: no dependency additions, build-tool changes, package-source changes, or package-boundary expansion; existing release SBOM generation remains applicable.
- VEX/advisory impact: no new third-party component requires feature-specific VEX entries.
- SLSA/provenance impact: existing release provenance workflow remains applicable to packages containing the additive Core preview/diagnostic API.
- Public API snapshot: Core snapshot was updated and reviewed for `PreviewAsync`, preview result/status/completeness contracts, guard diagnostics, denial reasons, candidates, and additive denial diagnostic forwarding properties.

## Feature Planning: Source Generator Validation (2026-05-13)

- SBOM: relevant for release packaging because SourceGenerators is a distributable analyzer package; no dependency inventory change is expected, but normal release SBOM generation remains required before publication.
- VEX: relevant for release governance; no feature-specific VEX statement is expected unless an advisory or dependency status changes before release.
- SLSA: relevant for package provenance because the feature changes release-capable generator diagnostics, generated helpers, and metadata evidence; no new build platform, package source, signing step, or provenance mechanism is expected.
- Release snapshot trigger: SourceGenerators public API snapshot and generated-output snapshots should be reviewed for additive diagnostics, helper generation, and metadata contracts before release approval.

## Source generator validation supply-chain evidence (2026-05-13)

No dependency inventory change or advisory status requiring VEX was introduced. Package-boundary tests cover analyzer packaging, private Roslyn assets, and no runtime visualization dependency.

## Feature Planning: Application Integration Adapters (2026-05-14)

- SBOM: relevant for release packaging because the feature adds optional distributable packages and expected dependency inventory changes; release SBOM generation must include the new packages and dependencies.
- VEX: relevant for release governance; no feature-specific VEX statement is expected unless an advisory or dependency vulnerability status applies before release.
- SLSA: relevant for package provenance because new distributable artifacts, package metadata, package-boundary evidence, signing, and provenance attestations must cover the integration packages.
- Release snapshot trigger: public API snapshots should be reviewed for additive integration package contracts, logging configuration contracts, validation startup-check contracts, and provider-neutral persistence coordination contracts before release approval.

## Feature 022 application integration adapters

Two new distributable packages are included in packable-project discovery: `StateForge.DependencyInjection` and `StateForge.Logging`. Package-boundary rules limit each package to Core plus its relevant Microsoft.Extensions abstraction package. Public API snapshots and package validation cover both packages. No vulnerability advisory currently requires a VEX statement for this feature.

## Feature 023 efcore persistence adapter

A new distributable package `StateForge.Persistence.EntityFrameworkCore` is included in packable-project discovery and release validation. SBOM/provenance/signing coverage follows existing release workflow controls. No current advisory requires a feature-specific VEX entry.
