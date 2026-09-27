#!/usr/bin/env bash
# TRAIN F whole-corpus emission check (COORD, i7). TWO ARMS, three targets each, every root seeded by git archive of its
# own commit: arm M = master's converter over master's tree, arm U = the union's converter over the union's tree.
# Each arm's DRIFT = the files its conversion WRITES (newer than a sentinel, *.cs.auto excluded) whose CR-stripped bytes
# differ from that arm's own seed. Union-ATTRIBUTABLE = drift in U that is not in M, plus drift in both on a file whose
# committed bytes differ between the two trees (a seat touched it). Pre-existing drift (in both, file untouched) is reported
# as a count, not a finding. Conversions run strictly one at a time (safety floor 1).
set -u
M=${M:?master sha}; U=${U:?union sha}
W=/h/go2cs-tmp-coord/tF
T=/h/go2cs-tmp-coord/tFemitfull
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { echo "ABORT GOROOT"; exit 2; }
cd "$W" || exit 2
rm -rf "$T"; mkdir -p "$T"
for arm in M U; do
  sha=${!arm}
  mkdir -p "$T/src-$arm" && git archive "$sha" src/go2cs | tar -x -C "$T/src-$arm" || { echo "ABORT archive $arm"; exit 3; }
  ( cd "$T/src-$arm/src/go2cs" && go build -o "$T/go2cs-$arm.exe" . ) || { echo "ABORT build $arm"; exit 3; }
  echo "converter $arm @ $sha sha256=$(sha256sum "$T/go2cs-$arm.exe" | cut -c1-16)"
  mkdir -p "$T/seed-$arm" && git archive "$sha" src/core | tar -x -C "$T/seed-$arm" || { echo "ABORT pristine $arm"; exit 3; }
  for os in windows linux darwin; do
    R="$T/$arm-$os"; mkdir -p "$R"
    git archive "$sha" src/core src/version.props docs/validation | tar -x -C "$R" || { echo "ABORT seed $arm $os"; exit 3; }
  done
done
for arm in M U; do for os in windows linux darwin; do
  R="$T/$arm-$os"; s=$(date +%s)
  echo "seed $arm-$os: $(find "$R/src/core" -name '*.cs' | wc -l) .cs"
  touch "$T/sentinel-$arm-$os"; sleep 1
  "$T/go2cs-$arm.exe" -stdlib -comments -platforms "$os/amd64" -go2cspath "$R/src" > "$T/emit-$arm-$os.log" 2>&1; rc=$?
  python "$(cygpath -w "$(dirname "$0")")/emitdrift.py" "$(cygpath -w "$R")" "$(cygpath -w "$T/seed-$arm")" "$(cygpath -w "$T/sentinel-$arm-$os")" "$(cygpath -w "$T/written-$arm-$os.txt")" "$(cygpath -w "$T/drift-$arm-$os.txt")" > /dev/null || { echo "ABORT drift $arm $os"; exit 3; }
  echo "EMIT $arm-$os rc=$rc wall=$(( $(date +%s) - s ))s written=$(wc -l < "$T/written-$arm-$os.txt") drift=$(wc -l < "$T/drift-$arm-$os.txt") :: $(tail -n 1 "$T/emit-$arm-$os.log" | tr -d '\r' | cut -c1-160)"
done; done
echo "--- UNION-ATTRIBUTABLE (per target)"
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/drift-M-$os.txt" | LC_ALL=C sort -u > "$T/dm"; cut -d' ' -f1 "$T/drift-U-$os.txt" | LC_ALL=C sort -u > "$T/du"
  LC_ALL=C comm -13 "$T/dm" "$T/du" > "$T/attr-$os.txt"
  LC_ALL=C comm -12 "$T/dm" "$T/du" | while IFS= read -r f; do
    a=$(git show "$M:$f" 2>/dev/null | md5sum | cut -c1-12); b=$(git show "$U:$f" 2>/dev/null | md5sum | cut -c1-12)
    [ "$a" = "$b" ] || echo "$f (drifts in both; a seat touched it)" >> "$T/attr-$os.txt"
  done
  echo "$os: master-drift=$(wc -l < "$T/dm") union-drift=$(wc -l < "$T/du") union-attributable=$(wc -l < "$T/attr-$os.txt")"
  sed 's/^/    /' "$T/attr-$os.txt" | head -n 60
done
echo "EMITCHECK DONE"
