# ASVS Verification

## Context

- System or feature: .NET StateForge repository baseline
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

## Feature Review: Runtime Graph Overlays (2026-05-13)

Selected Level: N/A. The feature adds in-process graph metadata and optional text-renderer hints only; it introduces no web application, HTTP API, authentication, authorization, session management, browser, hosted service, or user-account surface.

## Feature Planning: Fluent Region Builders (2026-05-13)

Selected Level: N/A. The feature adds in-process fluent declaration ergonomics for library users only; it introduces no web application, HTTP API, authentication, authorization, session management, browser, hosted service, user-account, or service endpoint surface.

## Feature Implementation: Fluent Region Builders (2026-05-13)

- ASVS applicability: N/A. The feature adds in-process Core builder APIs and validation diagnostics only.
- No web, HTTP API, authentication, authorization, session management, browser, or hosted endpoint surface was introduced.
- Verification focus remains CWE-20 input validation for region names/callbacks and safe diagnostic content for membership conflicts.

## Feature Implementation: Parallel Regions Documentation and Sample (2026-05-13)

- ASVS applicability: N/A. The feature adds Markdown documentation, an in-process console sample, and xUnit release validation only.
- No web application, HTTP API, authentication, authorization, session management, browser-facing service, hosted endpoint, user-account surface, or multi-user access-control surface was introduced.
- Verification focus remains safe validation diagnostics, accurate boundary language, and release-test evidence rather than ASVS control testing.

## Feature Planning: Transition Preview Diagnostics (2026-05-13)

Selected Level: N/A. The feature adds in-process transition explainability and structured denial diagnostics only; it introduces no web application, HTTP API, authentication, authorization, session management, browser, hosted service, user-account, or service endpoint surface.

Verification focus remains secure input validation and safe diagnostic content under the project security checklist rather than ASVS control testing.

## Feature Implementation: Transition Preview Diagnostics (2026-05-13)

- ASVS applicability: N/A. The feature adds in-process FSM preview and diagnostic data only.
- No web application, HTTP API, authentication, authorization, session management, browser-facing service, hosted endpoint, user-account surface, or multi-user access-control surface was introduced.
- Verification focus remains CWE-20 active-shape/event validation, safe diagnostic content, cancellation behavior, and side-effect-free runtime inspection rather than ASVS control testing.

## Feature Planning: Source Generator Validation (2026-05-13)

Selected Level: N/A. The feature improves compile-time declaration validation, generated helpers, and in-process metadata for a library source generator; it introduces no web application, HTTP API, authentication, authorization, session management, browser-facing service, hosted endpoint, user-account surface, or multi-user access-control surface.

Verification focus remains CWE-20 declarative input validation, safe diagnostic content, and package-boundary evidence under the project security checklist rather than ASVS control testing.

## Source generator validation ASVS rationale (2026-05-13)

ASVS remains not applicable: this feature introduces no web application, HTTP API, authentication, authorization, session management, or browser-facing service surface.

## Feature Planning: Application Integration Adapters (2026-05-14)

Selected Level: N/A. The feature adds in-process optional library integration packages for application composition, structured logging, startup validation checks, and provider-neutral persistence coordination. It introduces no web application, HTTP API, authentication, authorization, session management, browser-facing service, hosted endpoint, user-account surface, or multi-user access-control surface.

Verification focus remains secure configuration validation, safe diagnostic/log content, dependency boundaries, and package supply-chain evidence under the project security checklist rather than ASVS control testing.

## Feature 022 application integration adapters

ASVS remains not applicable. The feature adds in-process library adapters only and introduces no web endpoint, HTTP API, authentication, authorization, session management, browser, or hosted service surface.

## Feature 023 efcore persistence adapter

ASVS remains not applicable. The adapter is an in-process persistence package and introduces no web/API/auth/session surface.
