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

## Feature review: 016-transition-conflict-diagnostics

- MSL status: PASS. The structured diagnostics implementation uses C#/.NET only and adds immutable in-memory Core result data.
- Non-MSL code: none.
- Follow-up: Continue to apply validation, safe diagnostic content, dependency-boundary, and release snapshot review; memory safety does not replace those checks.

## Feature Review: Runtime Graph Overlays (2026-05-13)

- MSL status: PASS. Runtime overlay contracts, builder logic, visualization hints, tests, docs, and samples use C#/.NET and Markdown only.
- Non-MSL code: none.
- Follow-up: Continue to apply active-shape validation, safe diagnostics, dependency-boundary, and public API/release snapshot review.

## Feature Planning: Fluent Region Builders (2026-05-13)

- MSL status: PASS. Planned implementation remains in C#/.NET for Core builder ergonomics, validation tests, docs, samples, and public API evidence.
- Non-MSL justification: N/A; the feature does not require C, C++, Assembly, classic Objective-C, or other manual-memory implementation code.
- Follow-up: Continue to apply validation, safe diagnostic content, dependency-boundary, and release snapshot review; memory safety does not replace these checks.

## Feature Implementation: Fluent Region Builders (2026-05-13)

- MSL status: PASS. Implementation, tests, docs, and samples stayed in C#/.NET and Markdown.
- Non-MSL code: none; no unsafe/native/manual-memory component was introduced.
- Follow-up: Continue to validate builder inputs and public diagnostics; memory safety remains complementary to validation and release snapshot review.

## Feature Implementation: Parallel Regions Documentation and Sample (2026-05-13)

- MSL status: PASS. The implementation uses C#/.NET for the runnable sample and release tests, plus Markdown documentation and evidence updates.
- Non-MSL code: none; no C, C++, Assembly, classic Objective-C, native renderer, browser automation, or manual-memory component was introduced.
- Follow-up: Continue safe diagnostic and dependency-boundary review for sample output and release validation; memory safety does not replace validation of user-facing guidance.

## Feature Planning: Transition Preview Diagnostics (2026-05-13)

- MSL status: PASS. Planned production implementation remains in C#/.NET, with Markdown documentation and tests only.
- Non-MSL justification: N/A; the feature does not require C, C++, Assembly, classic Objective-C, native extensions, or manual-memory components.
- Secure-coding follow-up: apply C#/.NET validation and safe diagnostic rules to active-shape/event inputs, guard-result reporting, cancellation/error reporting, and public denial diagnostics.

## Feature Implementation: Transition Preview Diagnostics (2026-05-13)

- MSL status: PASS. Preview planner, diagnostic/result contracts, runtime entry points, tests, docs, and evidence updates use C#/.NET and Markdown only.
- Non-MSL code: none; no C, C++, Assembly, classic Objective-C, native callback bridge, or manual-memory component was introduced.
- Follow-up: Continue to treat guard callbacks as consumer code and rely on validation, cancellation, safe diagnostics, and public API review in addition to memory safety.

## Feature Planning: Source Generator Validation (2026-05-13)

- MSL status: PASS. Planned production implementation remains in C#/.NET for source-generator parsing, diagnostics, generated helpers, generated metadata, tests, and documentation.
- Non-MSL justification: N/A; the feature does not require C, C++, Assembly, classic Objective-C, native extensions, or manual-memory implementation code.
- Secure-coding follow-up: apply C#/.NET validation and safe diagnostic rules to declarative syntax/semantic inputs, generated metadata, generated graph data, and generated helper naming.
