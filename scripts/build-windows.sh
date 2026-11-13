#!/usr/bin/env bash
# Baut das portable Windows-Paket (self-contained, win-x64) nach dist/.
# Aufruf: scripts/build-windows.sh [zielordner]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

OUT="${1:-dist/RunicLab-win-x64}"
# Native .NET-Tools verstehen MSYS-Pfade wie /c/... nicht -> in Windows-Pfad wandeln.
if command -v cygpath >/dev/null 2>&1; then
  OUT_NATIVE="$(cygpath -w "$OUT")"
else
  OUT_NATIVE="$OUT"
fi

dotnet publish src/RunicLab/RunicLab.csproj \
  -f net10.0-windows10.0.19041.0 \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:TargetFrameworks=net10.0-windows10.0.19041.0 \
  -p:WindowsPackageType=None \
  -p:WindowsAppSDKSelfContained=true \
  -o "$OUT_NATIVE"

echo "Fertig: $OUT/RunicLab.exe"
