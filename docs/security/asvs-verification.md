# ASVS Verification

## Context

- System or feature: .NET State Machine Library repository baseline
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-11
- ASVS version used: OWASP ASVS 4.x family; exact version to be selected if ASVS becomes applicable

## Verification Level (MUST be explicit)

Selected Level: N/A for current baseline.

Rationale for level choice: The current project is a .NET library/package set, not a web application, HTTP API, authentication-bearing service, or session-managed multi-user application. No ASVS Level 1/2/3 controls are directly applicable to the current runtime surface.

If a future feature adds web, HTTP API, authentication, authorization, session management, hosted service endpoints, or browser-facing output, that feature MUST select ASVS Level 1, 2, or 3 explicitly and update this document or create feature-specific ASVS evidence.

## Verification Scope

Current baseline:

- Authentication: N/A — no auth surface.
- Session management: N/A — no session surface.
- Access control: N/A — no multi-user authorization surface.
- Input validation, sanitisation, and encoding: covered by `docs/security/security-checklist.md` rather than ASVS because validation is library/runtime validation, not web/API request validation.
- Stored cryptography: N/A — no cryptographic storage surface.
- Error handling and logging: covered by `docs/security/security-checklist.md` and `docs/security/secure-coding-language-rules.md`.
- Data protection: N/A for current library; persistence contracts are provider-neutral and application-owned.
- Communication security (TLS configuration): N/A — no network listener/client runtime dependency.
- Malicious code defence (deserialisation, dependency boundary): covered by secure coding and dependency audit documents.
- Business logic: covered by specs, plans, tests, and constitution checks.
- Files and resources: covered by security checklist and package/release validation.
- API and web service: N/A.
- Configuration: N/A for runtime packages; CI/release workflow configuration covered by dependency/supply-chain evidence.

## Results

- Passed (control IDs): N/A for current baseline.
- Failed (control IDs + finding): N/A.
- Not applicable (control IDs + rationale): All ASVS web/API/session/auth controls are N/A for the current library-only surface; see rationale above.

## Follow-Up

- Required remediations and owners: None for current baseline; future web/API/auth features must select an ASVS level.
- Re-verification trigger: introduction of web/API/auth/session/hosted endpoint features, browser-facing output, or an application wrapper around the library.
- Cross-references:
  - `docs/security/security-checklist.md`
  - `docs/security/dependency-audit.md`
  - `docs/security/supply-chain-evidence.md`

## Feature Review: Source Generator Hierarchy and Regions (2026-05-12)

Selected Level: N/A. The feature extends a compile-time C# source generator and generated Core definitions only; it adds no web application, HTTP API, authentication, authorization, session management, or browser-facing service surface.

## Feature review: 016-transition-conflict-diagnostics

Selected Level: N/A. The feature adds in-process FSM validation/runtime diagnostic data only; it introduces no web application, HTTP API, authentication, authorization, session management, browser, or service endpoint surface.
