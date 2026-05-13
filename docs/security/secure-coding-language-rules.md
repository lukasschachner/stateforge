# Secure Coding Language Rules

## Context

- Feature or component: .NET State Machine Library repository baseline
- Primary language(s): C#, Bash, PowerShell
- Frameworks involved: .NET `net10.0`, `netstandard2.0` analyzer/source-generator target where required by compiler hosts, GitHub Actions, shell/PowerShell validation scripts
- Reviewer: Spec Kit security-governance baseline
- Date: 2026-05-11

## How to Use

For every feature that touches input handling, serialization, authentication, authorisation, cryptography, file I/O, network I/O, package/build tooling, or dependency boundaries, record one of: `applied`, `not applicable (reason)`, or `deviation (justification + mitigation)` for the relevant rules below.

## C / C89 (CERT C)

- Bounds checking on all buffer writes: not applicable (no C/C89 implementation planned)
- No `gets()`: not applicable
- No unchecked `sprintf()`/`strcpy()`/`strcat()`: not applicable
- Integer overflow guards on size arithmetic: not applicable
- `malloc`/`free` ownership documented; no use-after-free or double-free: not applicable
- Format strings are constant, never user-controlled: not applicable

## C# / .NET (Microsoft Secure Coding Guidelines)

- Parameterised queries (no string-concatenated SQL): not applicable for Core and current optional packages (no database provider or SQL execution); required for any future database integration package.
- Output encoding for HTML/JS/URL contexts (anti-XSS): not applicable to Core runtime; required for any future HTML/web renderer or docs site that emits untrusted labels.
- Anti-forgery tokens on state-changing endpoints: not applicable (no HTTP endpoint or web application surface).
- Secure deserialisation only (no `BinaryFormatter` on untrusted input): applied as a design rule. Snapshot/persistence contracts are provider-neutral and must not require unsafe deserialization; any future serializer guidance must reject `BinaryFormatter` for untrusted input.
- `HttpClient` reuse via `IHttpClientFactory`: not applicable to Core and current package set (no outbound HTTP runtime dependency); required for any future HTTP/network adapter package.
- Secrets via `IConfiguration` + secret store, never hard-coded: applied as a repository rule. Current runtime packages should not require secrets; CI/package workflows must not hard-code credentials.

Additional .NET rules for this repository:

- Public APIs validate state/event/transition/snapshot inputs before runtime use.
- Diagnostics should identify invalid user definitions without exposing local paths, secrets, or internal stack traces as user-facing messages.
- Source generators must treat syntax/semantic inputs as compiler-provided data and emit deterministic code only; generated code must not execute user callbacks at generation time.
- Graph/rendering text adapters must escape or quote labels according to the target text format when user-controlled labels or metadata are supported.
- Core package remains free of hidden networking, hosting, database, logging, dependency-injection, and serializer dependencies unless a future spec explicitly changes package boundaries.

## SQL

- Parameterised statements only; no dynamic SQL from untrusted input: not applicable (no SQL execution)
- Least-privilege database accounts (no application use of DBA roles): not applicable
- Stored procedures parameterised; no `EXEC(@sql)` on user input: not applicable
- Error messages do not leak schema or query text to end users: not applicable

## Bash

- All variables quoted (`"$var"`, `"${arr[@]}"`): applied to repository-maintained scripts and required for future scripts.
- No `eval` on untrusted input: applied as a repository rule.
- `--` end-of-options sentinel before user-supplied paths: required where scripts accept file/path arguments.
- `set -euo pipefail` (or documented justification for omission): required for release/security-sensitive scripts; deviations must be documented.
- Temporary files via `mktemp`, never predictable names: required where temporary files are created.

## PowerShell

- `Set-StrictMode -Version Latest`: required for repository-maintained PowerShell release/security scripts unless documented otherwise.
- Validated parameters (`[ValidateSet]`, `[ValidateRange]`, `[ValidatePattern]`): required where scripts accept constrained user input.
- No `Invoke-Expression` on untrusted input: applied as a repository rule.
- No `ConvertFrom-Json -AsHashtable` on attacker-controlled input without validation: required where JSON is accepted from untrusted sources.
- Secrets via `SecretManagement` module or platform credential store: required if future scripts handle release or registry secrets.

## Cryptography

- Symmetric: AES-256 (GCM or CBC + HMAC): not applicable to current runtime semantics.
- Asymmetric: RSA ≥ 3072 bit, or Ed25519/X25519: not applicable to current runtime semantics.
- Hashing: SHA-256 or stronger; passwords via Argon2id, scrypt, or bcrypt: not applicable to current runtime semantics.
- Random: cryptographically secure RNG only: not applicable unless future features require randomness.
- Deprecated algorithms (MD5, SHA-1 for signatures, DES, 3DES, RC4) — only with explicit risk acknowledgement and documented compensating control: applied as a repository rule.

## Error Handling and Logging

- No stack traces, internal state, or connection strings in user-facing errors: applied; validation diagnostics should be actionable without leaking sensitive environment data.
- Sensitive data (secrets, tokens, PII) not logged: applied; OpenTelemetry/observer features must not encourage logging event payload secrets by default.
- Log injection prevented (no unescaped user input in log lines): applicable to future logging/telemetry adapters and sample code.

## Follow-Up

- Open deviations: SBOM/VEX/SLSA automation is not yet fully implemented; see `docs/security/supply-chain-evidence.md`.
- Required compensating controls: dependency and package-boundary tests; manual release review until automated SBOM/provenance is added.
- Re-review trigger: before public package publication, dependency additions, new serializer/storage providers, HTTP/API surfaces, release workflow changes, or cryptography/secrets handling.

## Feature Planning: Source Generator Validation (2026-05-13)

- C# / .NET input validation: applied. Declarative state, event, transition, hierarchy, parallel-region, completion, history, metadata, and graph inputs should be validated from compiler syntax/semantic data before generated code is treated as trustworthy.
- Source-generator execution safety: applied. Generation must remain deterministic and must not execute user-authored callbacks, guards, actions, transition behavior, runtime definitions, external processes, network calls, or visualization/rendering tools during generation.
- Safe diagnostics: applied. Diagnostic messages should include stable user-facing identifiers and remediation guidance only, avoiding stack traces, local paths, environment values, secrets, callback internals, telemetry payloads, and serialized event payload contents.
- HTML/JS/URL output encoding: not applicable. The planned generated metadata and graph data are renderer-neutral and in-process; any future browser-facing or HTML renderer output must perform context-appropriate encoding.
- SQL, authentication, authorization, cryptography, network I/O, and runtime file I/O rules: not applicable because the planned feature has no database, web/API/auth/session, cryptographic, network, or user-controlled runtime file surface.

## Source generator validation feature evidence (2026-05-13)

The generator inspects compiler syntax/semantic data and does not execute user guards, actions, transition behaviors, renderers, shell commands, network calls, or arbitrary file I/O. Diagnostics and generated metadata use declared identifiers and deterministic strings, avoiding stack traces, environment values, temporary paths, and serialized payload contents.
