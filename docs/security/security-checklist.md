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
- CRA applicability entry: `docs/security/cra-applicability.md`

## Follow-Up

- Open findings: Per-release VEX records and release artifact signing are not yet implemented; OpenSSF Scorecard review is not yet automated.
- Required mitigations and owners: Maintainers should issue VEX statements each public release, finalize signing policy, and continue release security tasks in package-affecting specs before public package publication.
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
