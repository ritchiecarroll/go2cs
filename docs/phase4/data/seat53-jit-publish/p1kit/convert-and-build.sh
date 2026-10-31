#!/bin/bash
# Seat #53 P1 kit, step 2: convert a Go program with one arm's converter against that arm's root, then build it (JIT)
# or publish it (Native AOT) into its own output directory.
#
# usage: convert-and-build.sh <converter> <goroot> <root-src> <go-module-dir> <work> <name> <jit|aot> <rid>
#   <converter>      W/H/go2cs.exe or W/A/go2cs.exe from seed-and-emit.sh
#   <goroot>         as in seed-and-emit.sh
#   <root-src>       the same arm's W/H/root/src or W/A/root/src
#   <go-module-dir>  the program's Go module (a folder holding hotphase.go and a go.mod; see the README)
#   <work>           a working directory; the conversion, artifacts and output go under <work>/<name>
#   <name>           this arm's name, unique within <work> (H, HA, A, ...)
#   <rid>            the runtime identifier, e.g. win-x64
# The executable lands at <work>/<name>/bin/<AssemblyName>.exe; the script prints that path.
set -euo pipefail
[ $# -eq 8 ] || { sed -n '5,14p' "$0"; exit 2; }
CONV=$1; GR=$2; ROOT=$3; MOD=$4; W=$5; NAME=$6; KIND=$7; RID=$8
KIT=$(cd "$(dirname "$0")" && pwd)
native() { if command -v cygpath > /dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
posix() { if command -v cygpath > /dev/null; then cygpath -u "$1"; else echo "$1"; fi; }
export GOROOT="$GR" GOTOOLCHAIN=local PATH="$(posix "$GR")/bin:$PATH" MSBUILDDISABLENODEREUSE=1
D=$W/$NAME
[ -e "$D" ] && { echo "refusing: $D exists"; exit 2; }
mkdir -p "$D"

# -recurse converts a module; the output directory is the second positional.
"$CONV" -recurse -go2cspath "$(native "$ROOT")\\" "$(native "$MOD")" "$(native "$D/out")" > "$D/convert.log" 2>&1
proj=$(find "$D/out/src" -name '*.csproj' -not -path '*/obj/*' | head -1)
exe=$(sed -n 's/.*<AssemblyName>\([^<]*\)<.*/\1/p' "$proj" | head -1)

if [ "$KIND" = jit ]; then
  (cd "$(dirname "$proj")" && dotnet build "$(basename "$proj")" -c Release -nologo -clp:ErrorsOnly -nodeReuse:false \
     "-p:Go2csArtifactsRoot=$(native "$D/art")\\" "-p:OutDir=$(native "$D/bin")\\") > "$D/build.log" 2>&1
else
  # Native AOT is scoped to the Exe by aot.targets: a global -p:PublishAot=true reaches go2cs-gen and fails (NETSDK1207).
  (cd "$(dirname "$proj")" && dotnet publish "$(basename "$proj")" -c Release -r "$RID" -nologo -clp:ErrorsOnly \
     -nodeReuse:false "-p:Go2csArtifactsRoot=$(native "$D/art")\\" -p:GoAot=true \
     "-p:CustomAfterMicrosoftCommonTargets=$(native "$KIT/aot.targets")" -o "$(native "$D/bin")") > "$D/build.log" 2>&1
fi
echo "$(native "$D/bin/$exe.exe")"
