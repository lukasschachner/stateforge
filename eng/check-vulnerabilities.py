#!/usr/bin/env python3
"""Fail if `dotnet list package --vulnerable --format json` reports vulnerabilities."""

from __future__ import annotations

import json
import sys
from pathlib import Path
from typing import Any


def collect_vulnerable_packages(node: Any, findings: list[str]) -> None:
    if isinstance(node, dict):
        vulnerabilities = node.get("vulnerabilities")
        if isinstance(vulnerabilities, list) and vulnerabilities:
            package = node.get("name") or node.get("id") or "<unknown-package>"
            versions = ", ".join(
                str(v.get("severity", "unknown")) for v in vulnerabilities if isinstance(v, dict)
            )
            findings.append(f"{package} ({len(vulnerabilities)} advisories; severities: {versions or 'unknown'})")

        for value in node.values():
            collect_vulnerable_packages(value, findings)
    elif isinstance(node, list):
        for value in node:
            collect_vulnerable_packages(value, findings)


def main() -> int:
    if len(sys.argv) != 2:
        print("Usage: check-vulnerabilities.py <dotnet-vulnerabilities-json>", file=sys.stderr)
        return 2

    report_path = Path(sys.argv[1])
    if not report_path.is_file():
        print(f"Vulnerability report not found: {report_path}", file=sys.stderr)
        return 2

    data = json.loads(report_path.read_text(encoding="utf-8"))
    findings: list[str] = []
    collect_vulnerable_packages(data, findings)

    if findings:
        unique = sorted(set(findings))
        print("Vulnerability gate failed. Vulnerable packages detected:", file=sys.stderr)
        for finding in unique:
            print(f" - {finding}", file=sys.stderr)
        return 1

    print("Vulnerability gate passed: no vulnerable packages reported.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
