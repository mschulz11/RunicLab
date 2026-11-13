#!/usr/bin/env bash
# Prüft (oder korrigiert) die Formatierung nach .editorconfig.
# Aufruf: scripts/format.sh [--fix]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ "${1:-}" == "--fix" ]]; then
  dotnet format RunicLab.slnx
else
  dotnet format RunicLab.slnx --verify-no-changes
fi
