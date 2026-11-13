#!/usr/bin/env bash
# Baut die Android-APK (arm64) nach artifacts/apk.
# Ohne Signing-Variablen entsteht eine unsignierte APK (nur zum Testen).
#
# Signieren:
#   export ANDROID_KEYSTORE_PATH=~/dev/keys/runiclab-release.keystore
#   export ANDROID_KEY_ALIAS=runiclab
#   export ANDROID_KEYSTORE_PASSWORD=...
#   export ANDROID_KEY_PASSWORD=...
#
# Aufruf: scripts/build-apk.sh [version]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VERSION="${1:-1.0}"
OUT="artifacts/apk"

if command -v cygpath >/dev/null 2>&1; then
  OUT_NATIVE="$(cygpath -w "$OUT")"
else
  OUT_NATIVE="$OUT"
fi

ARGS=(
  publish src/RunicLab/RunicLab.csproj
  -f net10.0-android
  -c Release
  -p:TargetFrameworks=net10.0-android
  -p:RuntimeIdentifier=android-arm64
  -p:AndroidPackageFormat=apk
  "-p:ApplicationDisplayVersion=$VERSION"
  -o "$OUT_NATIVE"
)

if [[ -n "${ANDROID_KEYSTORE_PATH:-}" ]]; then
  KEYSTORE_NATIVE="$ANDROID_KEYSTORE_PATH"
  if command -v cygpath >/dev/null 2>&1; then
    KEYSTORE_NATIVE="$(cygpath -w "$ANDROID_KEYSTORE_PATH")"
  fi
  ARGS+=(
    -p:AndroidKeyStore=true
    "-p:AndroidSigningKeyStore=$KEYSTORE_NATIVE"
    "-p:AndroidSigningKeyAlias=${ANDROID_KEY_ALIAS:?ANDROID_KEY_ALIAS fehlt}"
    "-p:AndroidSigningKeyPass=${ANDROID_KEY_PASSWORD:?ANDROID_KEY_PASSWORD fehlt}"
    "-p:AndroidSigningStorePass=${ANDROID_KEYSTORE_PASSWORD:?ANDROID_KEYSTORE_PASSWORD fehlt}"
  )
  echo "Signiere mit $ANDROID_KEYSTORE_PATH (Alias ${ANDROID_KEY_ALIAS})"
else
  echo "Kein ANDROID_KEYSTORE_PATH gesetzt -> unsignierte APK"
fi

dotnet "${ARGS[@]}"

echo "Fertig: $OUT/"
ls -1 "$OUT"
