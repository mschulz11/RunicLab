#!/usr/bin/env bash
# Baut und startet die Tests der Chiffre-Logik.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

dotnet test tests/RunicLab.Tests/RunicLab.Tests.csproj "$@"
