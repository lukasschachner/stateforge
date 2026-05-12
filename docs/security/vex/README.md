# VEX Records

This directory stores Vulnerability Exploitability eXchange (VEX) statements for released package sets.

## Expected process

1. Generate/refresh vulnerability report during release (`artifacts/security/vulnerabilities.json`).
2. Triage each advisory for exploitability in this repository.
3. Record results in a per-release VEX file under this folder.

Suggested naming:

- `v<release-tag>.vex.json`
- Example: `v0.1.0-rc.2.vex.json`

## Minimal record template

```json
{
  "release": "v0.1.0-rc.2",
  "generatedAtUtc": "2026-05-13T12:00:00Z",
  "statements": [
    {
      "advisory": "GHSA-xxxx-yyyy-zzzz",
      "package": "Package.Id",
      "version": "1.2.3",
      "status": "not_affected",
      "justification": "code_not_present",
      "notes": "Optional maintainer notes"
    }
  ]
}
```

Use `status` values such as `not_affected`, `affected`, or `under_investigation` and keep justification deterministic and reviewable.
