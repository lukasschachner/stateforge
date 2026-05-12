# Spec Security-Governance Retrofit Assessment

## Context

- Review date: 2026-05-11
- Preset: `.specify/presets/security-governance/`
- Scope: Existing `specs/*/{spec.md,plan.md,tasks.md}` artifacts

## Summary

The security-governance preset is installed and enabled, but existing specifications were authored before the preset was applied. They do not automatically gain feature-level security sections unless they are rerun or edited.

Project-level baseline evidence has been created under `docs/security/` so future specs can reference a shared baseline instead of duplicating all standards decisions.

## Project Baseline Decisions

| Area | Baseline decision |
|------|-------------------|
| Memory-safe language | Applies; C#/.NET is memory-safe, but secure .NET practices still apply. |
| NIST SSDF | Applies for production-bound work. |
| CWE Top 25 | Applies as a review and mitigation mapping baseline. |
| OWASP ASVS | N/A for current library-only surface; required if web/API/auth/session features are introduced. |
| SBOM | Required before public release/package publication. |
| VEX | Required before public release/package publication and for vulnerability triage. |
| SLSA | Target L1 minimum where feasible for release candidates; aim for L2+ over time. |
| OpenSSF Scorecard | Conditional for public OSS/high-impact dependency posture. |
| EU CRA | Must be reassessed before public NuGet publication or commercial/EU-market distribution. |

## Retrofit Priority

High-priority feature specs for targeted security-governance updates:

1. `005-nuget-release-readiness` — package publication, SBOM/VEX/SLSA, dependency automation, CRA, package provenance.
2. `003-state-machine-generator` — generated-code safety, Roslyn dependency audit, analyzer package boundary.
3. `004-state-persistence-contracts` — storage/persistence boundary, serialization guidance, concurrent-state safety, error behavior.
4. `006-transition-observation-otel` — telemetry data leakage, exporter dependency posture, no secret/PII logging guidance.
5. `007-graph-rendering-adapters` — graph label escaping/quoting for text formats and artifact/file output safety.
6. `013-active-state-snapshot` — snapshot validation, serialization boundary, persistence-safe guidance.
7. `014-add-completion-transitions` — active feature; should include security baseline references before task generation.

Completed historical specs (`001`-`013`) can remain as implementation history if maintainers do not need full retroactive compliance, but any reopened or release-affecting work should add a short security-governance section referencing the baseline evidence.

## Recommended Feature-Level Patch Pattern

Add a section like this to `spec.md`:

```md
## Security Governance Applicability

- Primary language: C#/.NET; memory-safe. Apply `docs/security/secure-coding-language-rules.md`.
- NIST SSDF and CWE Top 25: applicable; feature-level mitigations are covered by validation/tests/docs tasks.
- OWASP ASVS: N/A because this feature has no web/API/auth/session surface. Reassess if scope changes.
- SBOM/VEX/SLSA/OpenSSF: reference `docs/security/supply-chain-evidence.md` when package/release artifacts are affected.
- CRA: reference `docs/security/cra-applicability.md` when distribution or vulnerability handling is affected.
```

Add a section like this to `plan.md`:

```md
## Security Planning Checks

- Confirmed MSL status and applicable C#/.NET secure-coding rules.
- Identified feature-specific input, serialization, file/network I/O, dependency, and supply-chain risks.
- Recorded ASVS/SBOM/VEX/SLSA/OpenSSF/CRA applicability decisions with links to `docs/security/` evidence.
- Add explicit security tasks if implementation touches relevant surfaces.
```
