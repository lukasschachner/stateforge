# Memory-Safe Language Applicability

## Context

- System or feature: .NET State Machine Library repository baseline
- Primary implementation language: C# on .NET `net10.0` for runtime packages; source generator package may target `netstandard2.0` for analyzer-host compatibility while emitting C# for the runtime API
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-11

## MSL Status

- Memory-safe: Yes. C#/.NET is on the security-governance MSL allow-list.
- Not memory-safe: No for primary implementation code.

## Allow-List Summary

Applicable allow-list entry:

- C#, F#, VB.NET

Other allow-listed languages are not primary implementation languages for this repository.

## Common Non-MSL Examples

The project does not plan to use C, C++, Assembly, classic Objective-C, pre-1.0 Zig, manual-memory Nim, or D without the default GC for primary implementation work.

## Justification

- Constraint: N/A — primary implementation language is memory-safe.
- Why this constraint applies: N/A.
- Alternative considered: N/A.

MSL status is not treated as sufficient by itself. Feature plans and implementation tasks still need to apply C#/.NET secure-coding guidance, dependency review, validation/error-handling checks, and supply-chain evidence where relevant.

## Follow-Up

- Additional mitigations needed: Maintain C#/.NET secure coding rules in `docs/security/secure-coding-language-rules.md`; update feature-level tasks when changes touch input validation, serialization, file/network I/O, cryptography, authentication, authorization, package/build tooling, or dependency boundaries.
- Related evidence:
  - `docs/security/secure-coding-language-rules.md`
  - `docs/security/security-checklist.md`
  - `docs/security/dependency-audit.md`
  - `docs/security/supply-chain-evidence.md`

## Feature Review: Source Generator Hierarchy and Regions (2026-05-12)

- MSL status: C#/.NET remains the only implementation language for source-generator parsing, diagnostics, tests, docs, and samples.
- Non-MSL justification: N/A; no C/C++/Assembly/manual-memory code was introduced.
- Secure-coding follow-up: advanced generator inputs are compiler syntax/semantic data and are validated with deterministic diagnostics before generated code is emitted.
