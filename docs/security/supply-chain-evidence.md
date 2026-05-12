# Supply-Chain Evidence

## Context

- System or release: .NET State Machine Library release-capable package set
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-13
- Released version(s) covered: No public release version covered by this baseline; applies to release-candidate readiness work

## SBOM (Software Bill of Materials)

- Format used (CycloneDX, SPDX): CycloneDX JSON.
- Generator and version: `cyclonedx` .NET local tool (`6.2.0`).
- Storage location of the SBOM artefact: `artifacts/sbom/StateMachineLibrary.cdx.json` in release workflow artifacts.
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
- Signing and verification approach: Not configured yet.
- Gaps to next level and planned mitigations:
  - Add provenance/attestation generation for package artifacts.
  - Decide artifact signing strategy.
  - Consider pinning GitHub Actions to immutable SHAs for release workflows.
  - Add SBOM generation and retention.

## OpenSSF Scorecard

- Applicable (public OSS repository or high-impact external dependency): conditional yes if repository/packages are public OSS or intended for broad external consumption.
- Last Scorecard run date and overall score: Not run.
- Findings reviewed and follow-ups recorded: None yet.

## Build and Distribution Integrity

- CI build provenance recorded: Partial in CI; release workflow generates attestations for artifacts.
- Release artefacts signed: Not configured.
- Distribution channel verified (registry, store, internal mirror): Release workflow supports NuGet publication through protected `nuget-prod` environment approval and environment-scoped secret.

## Cross-References

- Dependency audit: `docs/security/dependency-audit.md`
- ASVS verification (with Level): `docs/security/asvs-verification.md` — current baseline N/A.
- CRA applicability (if release affects EU market reach): `docs/security/cra-applicability.md`

## Follow-Up

- Open risks: release artifact signing and immutable action SHA pinning are not yet implemented; VEX records must be produced per public release.
- Required mitigations and owners: Maintainers should issue per-release VEX statements, decide signing strategy, optionally add OpenSSF Scorecard automation, and evaluate SHA-pinning for release-critical workflows before broad public package publication.
- Re-evaluation trigger: public release candidate, package publication workflow, dependency changes, supply-chain incident, or repository visibility change.

## Feature Evidence: Completion Transitions (2026-05-12)

- Package/dependency changes: none expected for the feature implementation.
- Release snapshot trigger: Core public API snapshot was updated to reflect new completion-transition APIs and graph trigger metadata.
- Supply-chain impact: no new package sources, build tools, CI actions, signing steps, or distribution channels were introduced.

## Feature Evidence: Source Generator Hierarchy and Regions (2026-05-12)

- Package/dependency changes: none. SourceGenerators remains an analyzer package with existing Roslyn private build-time assets.
- Release snapshot trigger: SourceGenerators public API snapshot requires review because advanced declaration model types, diagnostics, and reporter helpers are exposed from the generator assembly.
- Supply-chain impact: no new package sources, build tools, CI actions, signing steps, external parsers, generated artifact ingestion, or distribution channels were introduced.
