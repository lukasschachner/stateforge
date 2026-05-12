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
  - Release artifact signing policy/implementation is not configured.
  - GitHub Actions are version-pinned by major version, not immutable SHA.
- Required mitigations and owners: Maintainers should operate VEX issuance per release, decide signing strategy, and consider SHA-pinning release-critical GitHub Actions before public package publication.
- Next audit trigger: dependency change, release workflow change, public NuGet release candidate, or security advisory affecting .NET/Roslyn/test/build dependencies.

## Feature Audit: Completion Transitions (2026-05-12)

- Runtime dependency change: none. Core completion-transition implementation uses existing Core definition/runtime/validation/introspection components only.
- Package-boundary expectation: Core remains dependency-light with no new `Microsoft.Extensions.*`, hosting, persistence, workflow, rendering, network, or crypto dependency.
- Re-audit trigger: revisit if completion-transition follow-up work adds packages, generated artifacts, package metadata changes, or release workflow changes.

## Feature Audit: Source Generator Hierarchy and Regions (2026-05-12)

- Dependency change: none. The implementation extends the existing Roslyn source-generator package and tests without adding NuGet, CI action, runtime, renderer, persistence, telemetry, hosting, network, or crypto dependencies.
- Package-boundary expectation: SourceGenerators keeps Roslyn as a private build/analyzer asset and emits calls into Core APIs; Core still has no dependency on SourceGenerators.
- Re-audit trigger: revisit if generated declaration support later adds a textual parser package, external files, analyzer packaging changes, or release workflow changes.
