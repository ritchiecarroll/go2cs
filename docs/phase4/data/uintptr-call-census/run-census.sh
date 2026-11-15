#!/bin/bash
# run-census.sh <worktree> <outdir> -- the typed (uintptr)<call> census, one flavour at a time (R, 2026-11-14).
# Builds analyzer/ first, then for each flavour purges the worktree's src/core build output (git clean -fdXq: ignored
# files only, so run it in a worktree of your own) and builds the census projects with the analyzer injected, so every
# project in their closure compiles once with it. Writes <outdir>/<flavour>.log and <outdir>/<flavour>.ucc.
# DOTNET names the dotnet executable (default: dotnet on PATH).
set -u
W="$1"; O="$2"
D="${DOTNET:-dotnet}"
HERE="$(cd "$(dirname "$0")" && pwd)"
T="$HERE/census.targets"
mkdir -p "$O"
"$D" build "$HERE/analyzer/UintptrCallCensus.csproj" -c Release -nologo -v q || { echo "ABORT: the analyzer did not build"; exit 2; }
cd "$W" || exit 9
for fl in windows-amd64 linux-amd64 darwin-amd64 linux-arm64; do
  os=${fl%-*}; arch=${fl#*-}
  git clean -fdXq src/core
  [ "$(git status --porcelain | grep -c '^ D')" = "0" ] || { echo "ABORT: tracked deletions after the purge"; exit 3; }
  projs="src/core/reflect/reflect.csproj src/core/internal/poll/internal.poll.csproj src/core/internal/reflectlite/internal.reflectlite.csproj src/core/runtime/runtime.tests.csproj src/core/reflect/reflect.tests.csproj"
  [ "$os" = darwin ] && projs="$projs src/core/crypto/x509/internal/macos/crypto.x509.internal.macos.csproj"
  : > "$O/$fl.log"
  for p in $projs; do
    echo "== $fl $p" >> "$O/$fl.log"
    "$D" build "$p" -c Release -nologo -v:m "-p:GoTargetOS=$os" "-p:GoTargetArch=$arch" "-p:CustomAfterMicrosoftCommonTargets=$T" >> "$O/$fl.log" 2>&1
    echo "== rc=$? $p" >> "$O/$fl.log"
  done
  grep -a 'warning UCC001' "$O/$fl.log" | sed 's/ \[[^]]*\]$//' | sort -u > "$O/$fl.ucc"
  echo "$fl: $(wc -l < "$O/$fl.ucc") UCC lines; builds: $(grep -c '^== rc=0' "$O/$fl.log") ok of $(grep -c '^== rc=' "$O/$fl.log")"
done
git clean -fdXq src/core
echo CENSUS-DONE
