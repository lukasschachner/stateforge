# Security Checklist

## Scope

- Feature: Repository baseline for .NET State Machine Library specs and release-capable packages
- Owner: Maintainers
- Review date: 2026-05-13
- Primary language(s): C#, Bash, PowerShell
- Frameworks involved: .NET `net10.0`, `netstandard2.0` analyzer target, GitHub Actions

## Generic Code-Review Checks

- Input validation reviewed: applicable to machine definitions, transitions, snapshots, graph labels/metadata, source-generator inputs, and package/release helper inputs.
- Output encoding reviewed: applicable to graph text renderers and future docs/web outputs; not applicable to Core runtime UI because Core has no UI.
- Authentication and session handling reviewed: N/A for current package set; no HTTP/auth/session surface.
- Authorisation and access control reviewed: N/A for current package set; no multi-user service or protected resource surface.
- Secrets handling reviewed (no hard-coded secrets): applicable to CI/release workflows and future package publishing; release workflow uses environment-scoped NuGet credentials and approval gate.
- Cryptographic primitives reviewed (current algorithms only): N/A for current runtime semantics; required if future signing, hashing, or crypto APIs are added.
- Error handling reviewed (no internal state leakage): applicable to validation diagnostics, source-generator diagnostics, persistence outcomes, and telemetry samples.
- File and network I/O reviewed: applicable to release/package validation scripts, generated artifacts, graph rendering samples, and future network adapters.
- Logging and sensitive data reviewed (no secrets in logs): applicable to observer/OpenTelemetry features and samples.

## CWE Top 25 Mapping

| CWE ID | Name | Affected? | Mitigation / Rationale |
|--------|------|-----------|------------------------|
| CWE-79 | XSS | Not currently | Core has no web/HTML output. Future HTML/docs/web renderer work must encode labels and metadata. |
| CWE-89 | SQL Injection | Not currently | No SQL execution or database provider. Any future database integration must use parameterized statements only. |
| CWE-20 | Improper Input Validation | Yes | Validate definitions, state/event references, transition targets, snapshots, graph metadata, and source-generator declarations before use. |
| CWE-78 | OS Command Injection | Low/conditional | Runtime packages do not execute shell commands. Repository scripts must avoid `eval`, quote variables, and use fixed command arguments. |
| CWE-22 | Path Traversal | Conditional | Package/archive/sample tooling must constrain artifact paths and avoid trusting arbitrary paths. |
| CWE-352 | CSRF | N/A | No web endpoint surface. |
| CWE-434 | Unrestricted File Upload | N/A | No upload surface. |
| CWE-862 | Missing Authorisation | N/A | No multi-user service authorization surface. |
| CWE-863 | Incorrect Authorisation | N/A | No multi-user service authorization surface. |
| CWE-287 | Improper Authentication | N/A | No authentication surface. |
| CWE-798 | Hard-coded Credentials | Conditional | CI/release workflows must not hard-code package registry tokens or signing credentials. |
| CWE-918 | SSRF | N/A currently | No outbound HTTP runtime dependency. Future HTTP/network adapters require SSRF review. |
| CWE-502 | Deserialisation of Untrusted Data | Conditional | Persistence/snapshot contracts must remain provider-neutral and must not require unsafe deserialization such as `BinaryFormatter` for untrusted input. |
| CWE-77 | Command Injection | Low/conditional | Same mitigation as shell command injection for repository scripts and tooling. |
| CWE-119 | Buffer Bounds | N/A | C#/.NET primary implementation is memory-safe; no C/C++ buffer handling. |

## Language-Specific Checks

- C / C89 rules location: N/A
- C# / .NET rules location: `docs/security/secure-coding-language-rules.md`
- SQL rules location: N/A until a database provider is proposed
- Bash rules location: `docs/security/secure-coding-language-rules.md`
- PowerShell rules location: `docs/security/secure-coding-language-rules.md`
- Other: N/A

## Cross-References

- Threat model entry/section: Not yet maintained separately; use feature specs and this checklist until a threat model is introduced.
- Dependency audit entry/section: `docs/security/dependency-audit.md`
- ASVS verification entry (with Level): `docs/security/asvs-verification.md` — current baseline N/A because no web/API/auth surface.
- Supply-chain evidence entry: `docs/security/supply-chain-evidence.md`
- Signing runbook entry: `docs/security/signing.md`
- CRA applicability entry: `docs/security/cra-applicability.md`

## Follow-Up

- Open findings: Per-release VEX records are not yet implemented; signing certificate lifecycle controls and OpenSSF Scorecard review are not yet automated.
- Required mitigations and owners: Maintainers should issue VEX statements each public release, operate certificate lifecycle controls, and continue release security tasks in package-affecting specs before public package publication.
- Re-review trigger: dependency additions, package/publication workflow changes, new HTTP/API/auth features, new serialization/storage providers, telemetry exporter changes, or public release readiness review.

## Feature Review: Completion Transitions (2026-05-12)

- CWE-20: Completion transition declarations now validate completion-capable sources, target existence, parallel scope structure, sibling-region boundary violations, and ambiguous unguarded completion transitions before runtime execution.
- Safe diagnostics: Completion validation messages include state/scope/target identifiers and deterministic validation codes without environment, filesystem, network, or secret data.
- ASVS: N/A for this feature; no web/API/auth/session surface was added.
- Surface review: No new authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, timers, hosted services, or background scheduler behavior was introduced.
- Secure coding rule review: Implementation stayed in memory-safe C#/.NET, reused existing cancellation-aware async lifecycle paths, and kept completion processing synchronous and bounded to active hierarchy/region shape plus declared completion transitions.

## Feature Review: Source Generator Hierarchy and Regions (2026-05-12)

- CWE-20: Advanced source-generator declarations validate duplicate explicit regions, missing regional initials, duplicate sibling region membership, unsupported history modes, and invalid advanced role combinations before emitting definitions.
- Safe diagnostics: New `SMG009`-`SMG015` diagnostics use deterministic IDs/messages and source/related locations without stack traces, environment details, secrets, or local path interpolation in message text.
- Runtime boundary: Core validation remains authoritative for graph reachability, runtime/guard-dependent ambiguity, and active-state validity; generated code only emits existing Core builder calls.
- Surface review: No authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, hosted service, renderer, or background scheduler behavior was introduced.
- Secure coding rule review: Implementation stayed in memory-safe C#/.NET and parses compiler syntax/semantic model data without executing user-authored DSL methods or callbacks during generation.

## Feature review: 016-transition-conflict-diagnostics

- CWE-20 / input validation: reviewed. Invalid definitions, region-boundary transitions, duplicate source scopes, invalid targets, and completion ambiguity retain validation findings and now add structured conflict diagnostics.
- Safe diagnostics: reviewed. Structured diagnostics include state/event/region identifiers, transition IDs, guard display names, and participant roles only; no stack traces, environment variables, filesystem paths, secrets, callback internals, logging sinks, or serialized user payloads are added.
- Runtime pre-commit behavior: reviewed. Parent/regional and completion conflicts are returned as non-committed outcomes before state writes.
- New surfaces: additive Core result properties and diagnostic model types only; no auth, crypto, web/API, network, file I/O, telemetry exporter, renderer, persistence provider, or hosting surface introduced.

## Feature Review: Runtime Graph Overlays (2026-05-13)

- CWE-20 / input validation: runtime overlay export validates active-state snapshots before returning overlay metadata and rejects unsupported option enum values/combinations.
- Side-effect-free inspection: reviewed and tested. Export reads active shape/current accessor state only; it does not dispatch events, evaluate guards, run actions, invoke observers, write external state, mutate history, or start background work.
- Safe diagnostics: reviewed. Invalid active-shape failures reuse snapshot diagnostics with state/region/sequence context and without stack traces, environment details, filesystem paths, secrets, guard/action output, or callback internals.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, or image-rendering surface was introduced.

## Feature Planning: Fluent Region Builders (2026-05-13)

- NIST SSDF: applicable. The feature changes production-bound public library surface and must carry traceable requirements, tests, documentation, validation evidence, and public API snapshot review.
- CWE Top 25: applicable primarily through CWE-20 input validation and safe diagnostic handling for blank region names, missing membership, duplicate/conflicting membership, and illegal regional boundaries.
- Safe diagnostics: planned diagnostics should include only declared state, event, region, and validation identifiers needed for remediation; no stack traces, environment data, filesystem paths, secrets, callback internals, or serialized payloads should be introduced.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, or workflow orchestration surface is in scope.

## Feature Implementation: Fluent Region Builders (2026-05-13)

- NIST SSDF: reviewed. Requirements, plan, tasks, tests, docs, sample, implementation notes, and public API snapshot review are tracked for the additive builder surface.
- CWE-20 / input validation: reviewed. New block overloads reject null callbacks and blank/null/whitespace region names locally; semantic errors such as duplicate/conflicting membership remain validation findings.
- Safe diagnostics: reviewed. Membership diagnostics include declared state/composite/region identifiers and remediation guidance only; no stack traces, environment data, filesystem paths, secrets, callback internals, or serialized payloads were added.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, or workflow orchestration surface was introduced.

## Feature Implementation: Parallel Regions Documentation and Sample (2026-05-13)

- NIST SSDF: reviewed. Requirements, plan, contracts, release tests, sample output labels, documentation links, security evidence, and implementation notes provide traceability for the documentation/sample feature.
- CWE-20 / input validation: reviewed. The guide documents invalid region declarations, missing initial states, duplicate or invalid region names, illegal boundaries, ambiguous handling, and pre-commit conflict behavior using existing validation semantics.
- Safe diagnostics: reviewed. The sample prints deterministic validation code/message content only; it does not print stack traces, local paths, environment values, secrets, process IDs, timestamps, random IDs, or callback internals.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, or workflow orchestration surface was introduced.

## Feature Planning: Transition Preview Diagnostics (2026-05-13)

- NIST SSDF: applicable. The feature changes production-bound transition behavior diagnostics and should carry traceable requirements, tests, documentation, public API snapshot review, and release evidence.
- CWE Top 25: applicable primarily through CWE-20 input validation and safe error/diagnostic handling for supplied active shapes, events, guard results, ambiguous candidates, validation conflicts, and unknown/terminal states.
- Safe diagnostics: planned diagnostics should include state, event, transition, guard, region, and validation identifiers needed for remediation only; they must not expose stack traces, environment data, local filesystem paths, secrets, callback internals, telemetry payloads, persistence details, or serialized event payload contents beyond consumer-supplied identifiers.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, logging integration, dependency-injection, or workflow orchestration surface is in scope.

## Feature Implementation: Transition Preview Diagnostics (2026-05-13)

- NIST SSDF: reviewed. Requirements, plan, contracts, tests, docs, public API snapshot review, and validation notes track the additive transition preview/denial diagnostic surface.
- CWE-20 / input validation: reviewed. Preview validates machine definitions and supplied active-state shapes before authoritative matching, and maps validation failures to structured diagnostics.
- Safe diagnostics: reviewed. Denial/guard diagnostics include reason codes, transition IDs, guard display names, state/event/region identifiers, and validation codes only; they do not add stack traces, local paths, environment values, secrets, persistence details, telemetry payloads, or serialized event payloads.
- Guard purity caveat: documented. Preview may execute guards to explain permit/deny decisions; users are told to keep previewed guards pure/idempotent.
- Side-effect-free preview: tested. Preview does not run entry/exit actions, transition actions, transition behaviors, observers, completion cascades, accessor setters, persistence hooks, telemetry exporters, hosted work, or renderer behavior.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, dependency-injection, logging, or workflow orchestration surface was introduced.

## Feature Planning: Source Generator Validation (2026-05-13)

- NIST SSDF: applicable. The feature changes production-bound generator behavior and should carry traceable requirements, tests, documentation, diagnostics evidence, generated-output compatibility evidence, public API snapshot review, and release evidence.
- CWE Top 25: applicable primarily through CWE-20 input validation and safe error/diagnostic handling for declarative state, event, transition, hierarchy, parallel-region, completion, history, metadata, and graph declarations.
- OWASP ASVS: N/A. The feature has no web application, HTTP API, authentication, authorization, session management, browser-facing service, hosted endpoint, user-account, or multi-user access-control surface; `docs/security/asvs-verification.md` records the rationale.
- SBOM: relevant for release packaging because SourceGenerators is a distributable package; no dependency inventory change is expected, but release SBOM generation remains required before publication.
- VEX: relevant for release governance; no feature-specific VEX entry is expected unless a dependency advisory or vulnerability status changes before release.
- SLSA: relevant for package provenance because generated diagnostics/helpers/metadata affect release-capable artifacts; existing provenance/signing workflow remains applicable unless release tooling changes.
- Safe diagnostics: planned diagnostics should include stable machine, state, event, transition, region, completion, history, and validation identifiers needed for remediation only; they must not expose stack traces, environment data, local filesystem paths, secrets, callback internals, telemetry payloads, serialized event payload contents, or generated temporary paths.
- Surface review: no authentication, authorization, cryptography, network I/O, user-controlled file I/O, serializer, persistence provider, telemetry exporter, hosted service, browser, image-rendering, command execution, logging integration, dependency-injection, workflow orchestration, or runtime visualization dependency is in scope.
- Evidence artifacts to update: `docs/security/msl-applicability.md`, `docs/security/security-checklist.md`, `docs/security/secure-coding-language-rules.md`, `docs/security/dependency-audit.md`, `docs/security/supply-chain-evidence.md`, and `docs/security/asvs-verification.md`; create per-release VEX records under `docs/security/vex/` only if advisory status requires it.

## Source generator validation feature checkpoint (2026-05-13)

- NIST SSDF/CWE-20: malformed declarations are covered by deterministic diagnostic tests.
- Safe diagnostics: messages avoid environment-specific values and callback internals.
- N/A surfaces: no web, auth, crypto, serializer, network, or hosted service surface was added.
