# NuGet Author Signing Runbook

## Scope

This runbook defines how maintainers operate NuGet author signing for release artifacts.

It applies to the release workflow (`.github/workflows/release.yml`) and protected environment (`nuget-prod`).

## Required Secrets (`nuget-prod` environment)

- `NUGET_API_KEY` — NuGet.org API key with package publish permissions.
- `NUGET_SIGN_CERT_PFX_B64` — Base64-encoded PFX certificate used for `dotnet nuget sign`.
- `NUGET_SIGN_CERT_PASSWORD` — Password protecting the PFX.
- `NUGET_TIMESTAMP_URL` (optional) — RFC3161 timestamp URL. If omitted, workflow default is used.

All signing and publish secrets MUST be environment-scoped (not repository-scoped) and protected by required reviewers.

## Certificate Requirements

Signing certificate SHOULD:

- be issued by a trusted CA suitable for code/package signing;
- include code-signing EKU;
- have a documented owner and expiration date;
- be stored in an approved secret manager before import into GitHub environment secrets.

## Preparing Secret Values

### 1) Export PFX as base64

```bash
base64 -w 0 signing-cert.pfx > signing-cert.pfx.b64
```

Use the resulting single-line value for `NUGET_SIGN_CERT_PFX_B64`.

### 2) Set environment secrets

Set all required secrets on GitHub environment `nuget-prod` and ensure required reviewers are configured.

## Release Workflow Behavior

During `publish-nuget` job:

1. Package artifacts are downloaded.
2. Signing secrets are validated (job fails if missing).
3. `.nupkg` files are author-signed via:
   - `dotnet nuget sign ... --certificate-path ... --certificate-password ... --timestamper ...`
4. Signed packages are verified via:
   - `dotnet nuget verify --all ...`
5. Provenance attestations are created for signed packages and SBOM.
6. Packages are pushed to NuGet.

If signing or verification fails, publication is blocked.

## Manual Verification (Local)

Before rotating to a new certificate, dry-run locally on a test package:

```bash
dotnet nuget sign <package.nupkg> \
  --certificate-path <cert.pfx> \
  --certificate-password <password> \
  --timestamper <rfc3161-url> \
  --overwrite

dotnet nuget verify --all <package.nupkg>
```

## Rotation Procedure

1. Generate/obtain replacement certificate.
2. Validate signing/verification locally against a test package.
3. Update `NUGET_SIGN_CERT_PFX_B64` and `NUGET_SIGN_CERT_PASSWORD` in `nuget-prod`.
4. Run a release workflow using `workflow_dispatch` with publish disabled first (validation path).
5. Run tagged/manual publish after approval.
6. Revoke/decommission old certificate according to CA policy.

## Revocation / Incident Response

If signing key compromise is suspected:

1. Immediately remove/rotate signing secrets in `nuget-prod`.
2. Pause release publication.
3. Revoke compromised certificate with CA.
4. Issue incident notice in release/security notes.
5. Re-sign and republish affected versions where possible, or publish patched versions.
6. Add/refresh VEX statements under `docs/security/vex/` where advisories apply.

## Operational Cadence

- Review certificate expiration monthly.
- Rotate certificate before expiration according to organization policy.
- Re-validate timestamp URL availability periodically.
- Re-test signing flow after major .NET SDK upgrades.
