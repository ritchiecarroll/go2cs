#!/usr/bin/env bash
# TRAIN J whole-corpus emission check (COORD, i7). TWO ARMS, three targets each, every root seeded by git archive of its
# own commit: arm M = master's converter over master's tree, arm U = the union's converter over the union's tree.
# REVIEW SIBLINGS (*.cs.auto, never compiled) are a SEPARATE class since TRAIN J (ruling 857afcb47e): reported per arm,
# refreshed by the fixup from the U arm (flat siblings from U-windows, the L3 default), and positive-controlled below by
# C2's census of 2026-09-29 (8 stale at 2ff42f7a16), which must be a SUBSET of the M arm's sibling drift per target.
# Each arm's DRIFT = the files its conversion WRITES (newer than a sentinel, *.cs.auto excluded) whose CR-stripped bytes
# differ from that arm's own seed. Union-ATTRIBUTABLE = drift in U that is not in M, plus drift in both on a file whose
# committed bytes differ between the two trees (a seat touched it). Pre-existing drift (in both, file untouched) is reported
# as a count, not a finding. Conversions run strictly one at a time (safety floor 1).
set -u
M=${M:?master sha}; U=${U:?union sha}
W=/h/go2cs-tmp-coord/tJ
T=/h/go2cs-tmp-coord/tJemitfull
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
  python "$(cygpath -w "$(dirname "$0")")/emitdrift.py" "$(cygpath -w "$R")" "$(cygpath -w "$T/seed-$arm")" "$(cygpath -w "$T/sentinel-$arm-$os")" "$(cygpath -w "$T/written-$arm-$os.txt")" "$(cygpath -w "$T/drift-$arm-$os.txt")" "$(cygpath -w "$T/siblings-$arm-$os.txt")" > /dev/null || { echo "ABORT drift $arm $os"; exit 3; }
  echo "EMIT $arm-$os rc=$rc wall=$(( $(date +%s) - s ))s written=$(wc -l < "$T/written-$arm-$os.txt") drift=$(wc -l < "$T/drift-$arm-$os.txt") siblings=$(wc -l < "$T/siblings-$arm-$os.txt") :: $(tail -n 1 "$T/emit-$arm-$os.log" | tr -d '\r' | cut -c1-160)"
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
echo "--- REVIEW SIBLINGS (*.cs.auto; the fixup refreshes the U arm's list; flat siblings from U-windows)"
CTL_ALL="src/core/hash/crc32/crc32_amd64.cs.auto src/core/internal/sync/hashtriemap.cs.auto src/core/sync/pool.cs.auto src/core/sync/poolqueue.cs.auto src/core/crypto/internal/fips140/subtle/xor_generic.cs.auto src/core/runtime/runtime2.cs.auto"
CTL_LINUX="src/core/syscall/linux/exec_unix.cs.auto src/core/os/linux/wait_waitid.cs.auto"
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/siblings-M-$os.txt" | LC_ALL=C sort -u > "$T/sm"; cut -d' ' -f1 "$T/siblings-U-$os.txt" | LC_ALL=C sort -u > "$T/su"
  miss=""; ctl="$CTL_ALL"; [ "$os" = linux ] && ctl="$ctl $CTL_LINUX"
  for f in $ctl; do grep -qxF "$f" "$T/sm" || miss="$miss $f"; done
  if [ -z "$miss" ]; then c="CONTROL OK ($(echo $ctl | wc -w) of C2's census present in M)"; else c="CONTROL FAILED, absent from M:$miss"; fi
  echo "$os: M-siblings=$(wc -l < "$T/sm") U-siblings=$(wc -l < "$T/su") :: $c"
  sed 's/^/    U /' "$T/su" | head -n 40
done
echo "EMITCHECK DONE"
