# EU Cyber Resilience Act (CRA) Applicability

## Context

- System or product: .NET State Machine Library and related NuGet package artifacts
- Released version(s) covered: No public release version covered by this baseline
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-11
- Regulation reference: Regulation (EU) 2024/2847

## Scope Decision

- Is the software a "product with digital elements" placed on the EU market?
  - Current baseline: N/A for the current repository state because no public package publication is in scope yet.
  - Release gate: must be reassessed before any public NuGet publication, commercial offering, or externally distributed release candidate intended for EU users.
- Does the software qualify under CRA Annex III (important products) or Annex IV (critical products)?
  - Current baseline: N/A; not assessed because no market placement decision is recorded.
  - Preliminary expectation: a generic state machine library is unlikely to be Annex III/IV by itself, but this must be confirmed if distribution/commercial context changes.
- Conformity assessment approach:
  - Current baseline: N/A.
  - If CRA-scoped later, record whether self-assessment is sufficient or whether third-party assessment is required by product classification.

## Required Records (when CRA-scoped)

- SBOM availability per released version: not yet implemented; see `docs/security/supply-chain-evidence.md`.
- Vulnerability disclosure and handling process documented: not yet recorded.
- 24-hour reporting expectation for actively exploited vulnerabilities acknowledged and operationalised: not yet operationalised.
- Secure-by-design alignment recorded: partially via constitution, specs, tests, validation, package-boundary checks, and this security baseline.
- Secure-by-default alignment recorded: partially via dependency-light Core, opt-in optional packages, and no hidden network/storage/hosting behavior.
- Documentation provided to users about secure use: partial; future release docs should include security/vulnerability reporting and package integrity guidance.

## Out-of-Scope Justification

The current baseline does not assert final CRA non-applicability. It records that the repository is not yet placed on the EU market as a product release in the evidence currently reviewed. Because NuGet distribution is planned by release-readiness work, CRA applicability must be revisited before publication or commercial distribution.

Silent omission is not allowed: feature plans that affect distribution, EU market reach, vulnerability handling, conformity assessment, package publication, SBOM/VEX, or release documentation must reference this decision and update it if the answer changes.

## Follow-Up

- Open compliance gaps:
  - Final market-placement and product-with-digital-elements decision before public release.
  - Vulnerability disclosure and handling process.
  - SBOM/VEX per release.
  - Secure-use documentation and package integrity guidance.
- Required mitigations: Add release-readiness/security tasks before public package publication.
- Next CRA review date: Before any public NuGet publication, commercial distribution decision, or release process change.

## Feature Review: Source Generator Hierarchy and Regions (2026-05-12)

- Distribution applicability: unchanged. The feature modifies an existing release-capable SourceGenerators package but does not add a publication workflow, distribution channel, commercial offering, or EU market-placement decision.
- Release-impact rationale: public API snapshot and sample/release validation are required before release, but CRA applicability remains governed by the baseline pre-publication reassessment gate.

## Feature review: 016-transition-conflict-diagnostics

CRA applicability remains unchanged. The feature adds additive Core diagnostics and public API snapshot evidence but does not alter product distribution, vulnerability-handling workflow, networked behavior, or EU market-placement assumptions. Reassess before public package publication as usual.
