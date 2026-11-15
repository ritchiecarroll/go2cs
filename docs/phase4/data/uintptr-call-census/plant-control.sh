#!/bin/bash
# plant-control.sh <worktree> <outdir> -- the census's positive control. Re-plants batch 301's shape at
# runtime/mfinal.cs (`frame = (uintptr)mallocgc(framesz, nil, true);`), builds runtime for windows/amd64 with the
# analyzer, prints the record it gives that line (expected: BACK-TO-POINTER, callee runtime.mallocgc), then restores the
# file from HEAD and proves it byte-identical. Build analyzer/ first (run-census.sh does). DOTNET as in run-census.sh.
set -u
W="$1"; O="$2"; F=src/core/runtime/mfinal.cs
D="${DOTNET:-dotnet}"
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$W" || exit 9
mkdir -p "$O"
n=$(grep -c 'frame = mallocgc(framesz, nil, true);' $F)
[ "$n" = 1 ] || { echo "PLANT ABORT: anchor count $n"; exit 3; }
sed -i 's/frame = mallocgc(framesz, nil, true);/frame = (uintptr)mallocgc(framesz, nil, true);/' $F
echo "planted: $(grep -c 'frame = (uintptr)mallocgc(framesz, nil, true);' $F)"
git clean -fdXq src/core
"$D" build src/core/runtime/runtime.csproj -c Release -nologo -v:m -p:GoTargetOS=windows -p:GoTargetArch=amd64 \
  "-p:CustomAfterMicrosoftCommonTargets=$HERE/census.targets" > "$O/plant.log" 2>&1
echo "build rc=$?"
grep -a 'warning UCC001' "$O/plant.log" | sed 's/ \[[^]]*\]$//' | sort -u > "$O/plant.ucc"
echo "UCC lines: $(wc -l < "$O/plant.ucc")"
grep -a 'mfinal.cs(' "$O/plant.ucc"
git checkout -- $F
git diff --quiet -- $F && echo "RESTORED byte-identical to HEAD" || echo "RESTORE MISMATCH"
git clean -fdXq src/core
