#!/usr/bin/env bash
# TRAIN K whole-corpus emission check (COORD, i7), adapted from tJ-emitcheck.sh. TWO ARMS, three targets each, every
# root seeded by git archive of its own commit: arm M = master's converter over master's tree, arm U = the union's
# converter (the FIXUP HEAD) over the union's tree. Conversions run strictly one at a time (safety floor 1).
# Each arm's DRIFT = the files its conversion WRITES (newer than a sentinel, *.cs.auto excluded) whose CR-stripped bytes
# differ from that arm's own seed. Union-ATTRIBUTABLE = drift in U that is not in M, plus drift in both on a file whose
# committed bytes differ between the two trees (a seat touched it). REVIEW SIBLINGS (*.cs.auto) are their own class
# (ruling 857afcb47e), reported per arm.
# CHANGED FROM TRAIN J (deliberately):
#  * the sibling positive control was C2's 2026-09-29 census of 8 stale siblings at 2ff42f7a16; TRAIN J's fixup
#    refreshed exactly those, so at K's base that census is no longer drift and would fail for the wrong reason. It is
#    replaced by PLANTED controls (floor 13): one sibling and one ordinary converted file are perturbed in BOTH the
#    emitted root (forcing the converter to rewrite them) and the arm's comparison seed (so the rewrite reads as drift).
#    Each plant MUST appear in its class for all six conversions, and is then subtracted before attribution.
#  * every conversion's rc must be 0, and no emission log may carry S3's "Refusing to convert" (p1-s3-cgo-refusal:
#    0 selected cgo files at CGO_ENABLED=0 on all three targets).
#  * named readings: PRERES-K6 (os/{linux,darwin}/file_unix.cs -- expected in M's drift, pre-existing, and ABSENT from
#    U's once the fixup refreshed them), PRERES-K9 (runtime/*/package_info.cs must not be union-attributable: the
#    union converter's reconvert of runtime equals the line-wise resolution), and the committed-csproj M-vs-U check
#    (FIXUP ORDER step 5: the S1 LangVersion edit and the OSR opt-in are the ONLY csproj differences; the -stdlib
#    emission writes a csproj only when its bytes differ, so U's csproj drift must also be 0).
set -u
M=${M:?master sha}; U=${U:?union sha}
SD=$(cd "$(dirname "$0")" && pwd)   # absolute: the script cd's into $W below
W=/h/go2cs-tmp-coord/tK
T=/h/go2cs-tmp-coord/tKemitfull
PLANT_SIB=src/core/sync/pool.cs.auto
PLANT_CS=src/core/unicode/utf8/utf8.cs
PLANT_LINE='// TRAIN K emission-check plant (positive control; never committed)'
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { echo "ABORT GOROOT"; exit 2; }
cd "$W" || exit 2
for f in "$PLANT_SIB" "$PLANT_CS"; do
  git cat-file -e "$M:$f" 2>/dev/null && git cat-file -e "$U:$f" 2>/dev/null || { echo "ABORT plant target $f absent from an arm"; exit 2; }
done
rm -rf "$T"; mkdir -p "$T"
# EVIDENCE + DISK: on ANY exit (verdict or ABORT) the drift/sibling/written/attr lists and the emit logs -- the U-arm
# sibling list is what the fixup's refresh still owes -- are copied into the battery's log folder (LOGDIR, passed by
# tK-battery.sh; the next run's rm -rf above would otherwise destroy them), then the six seeded roots, the two
# comparison seeds and the converter source archives are removed (the battery's purge covers only src/**). The
# converter exes stay in $T; their sha256 is printed above each arm.
EVD="${LOGDIR:-$SD/tK-logs}/emitcheck"
archive_and_clean(){
  local rc=$? f n=0
  mkdir -p "$EVD"
  for f in "$T"/drift-* "$T"/siblings-* "$T"/written-* "$T"/attr-* "$T"/dm-* "$T"/du-* "$T"/sm-* "$T"/su-* "$T"/emit-*; do
    [ -f "$f" ] && cp "$f" "$EVD/" && n=$((n + 1))
  done
  rm -rf "$T"/M-windows "$T"/M-linux "$T"/M-darwin "$T"/U-windows "$T"/U-linux "$T"/U-darwin "$T"/seed-M "$T"/seed-U "$T"/src-M "$T"/src-U
  echo "EVIDENCE copied $n file(s) to $EVD; roots, seeds and source archives removed from $T (exes kept) rc=$rc"
  exit $rc
}
trap archive_and_clean EXIT
for arm in M U; do
  sha=${!arm}
  mkdir -p "$T/src-$arm" && git archive "$sha" src/go2cs | tar -x -C "$T/src-$arm" || { echo "ABORT archive $arm"; exit 3; }
  ( cd "$T/src-$arm/src/go2cs" && go build -o "$T/go2cs-$arm.exe" . ) || { echo "ABORT build $arm"; exit 3; }
  echo "converter $arm @ $sha sha256=$(sha256sum "$T/go2cs-$arm.exe" | cut -c1-16)"
  mkdir -p "$T/seed-$arm" && git archive "$sha" src/core | tar -x -C "$T/seed-$arm" || { echo "ABORT pristine $arm"; exit 3; }
  for f in "$PLANT_SIB" "$PLANT_CS"; do printf '\n%s\n' "$PLANT_LINE" >> "$T/seed-$arm/$f"; done
  for os in windows linux darwin; do
    R="$T/$arm-$os"; mkdir -p "$R"
    git archive "$sha" src/core src/version.props docs/validation | tar -x -C "$R" || { echo "ABORT seed $arm $os"; exit 3; }
    for f in "$PLANT_SIB" "$PLANT_CS"; do printf '\n%s\n' "$PLANT_LINE" >> "$R/$f"; done
  done
done
emitbad=0
for arm in M U; do for os in windows linux darwin; do
  R="$T/$arm-$os"; s=$(date +%s)
  echo "seed $arm-$os: $(find "$R/src/core" -name '*.cs' | wc -l) .cs"
  touch "$T/sentinel-$arm-$os"; sleep 1
  "$T/go2cs-$arm.exe" -stdlib -comments -platforms "$os/amd64" -go2cspath "$R/src" > "$T/emit-$arm-$os.log" 2>&1; rc=$?
  python "$(cygpath -w "$SD")/emitdrift.py" "$(cygpath -w "$R")" "$(cygpath -w "$T/seed-$arm")" "$(cygpath -w "$T/sentinel-$arm-$os")" "$(cygpath -w "$T/written-$arm-$os.txt")" "$(cygpath -w "$T/drift-$arm-$os.txt")" "$(cygpath -w "$T/siblings-$arm-$os.txt")" > /dev/null || { echo "ABORT drift $arm $os"; exit 3; }
  refuse=$(grep -ac 'Refusing to convert' "$T/emit-$arm-$os.log")
  [ "$rc" = 0 ] && [ "$refuse" = 0 ] || emitbad=$((emitbad + 1))
  pc="plants:"; grep -qxF "$PLANT_SIB" "$T/siblings-$arm-$os.txt" && pc="$pc sib=OK" || pc="$pc sib=MISSING"
  grep -qxF "$PLANT_CS" "$T/drift-$arm-$os.txt" && pc="$pc cs=OK" || pc="$pc cs=MISSING"
  echo "EMIT $arm-$os rc=$rc refusals=$refuse wall=$(( $(date +%s) - s ))s written=$(wc -l < "$T/written-$arm-$os.txt") drift=$(cut -d' ' -f1 "$T/drift-$arm-$os.txt" | grep -vcxF "$PLANT_CS") siblings=$(cut -d' ' -f1 "$T/siblings-$arm-$os.txt" | grep -vcxF "$PLANT_SIB") (plants excluded) $pc :: $(tail -n 1 "$T/emit-$arm-$os.log" | tr -d '\r' | cut -c1-140)"
done; done
pbad=$(for arm in M U; do for os in windows linux darwin; do grep -qxF "$PLANT_SIB" "$T/siblings-$arm-$os.txt" || echo x; grep -qxF "$PLANT_CS" "$T/drift-$arm-$os.txt" || echo x; done; done | wc -l)
echo "PLANT CONTROL: $([ "$pbad" = 0 ] && echo "OK (both plants read as drift in all six conversions)" || echo "FAILED ($pbad of 12 plant readings missing) -- the drift instrument is not measuring; every count below is VOID")"
echo "EMIT RC/REFUSAL: $([ "$emitbad" = 0 ] && echo "OK (6 conversions rc=0, 0 S3 refusals)" || echo "FAILED on $emitbad conversion(s)")"
echo "--- UNION-ATTRIBUTABLE (per target; plants subtracted)"
attrtot=0
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/drift-M-$os.txt" | grep -vxF "$PLANT_CS" | LC_ALL=C sort -u > "$T/dm-$os"; cut -d' ' -f1 "$T/drift-U-$os.txt" | grep -vxF "$PLANT_CS" | LC_ALL=C sort -u > "$T/du-$os"
  LC_ALL=C comm -13 "$T/dm-$os" "$T/du-$os" > "$T/attr-$os.txt"
  LC_ALL=C comm -12 "$T/dm-$os" "$T/du-$os" | while IFS= read -r f; do
    a=$(git show "$M:$f" 2>/dev/null | md5sum | cut -c1-12); b=$(git show "$U:$f" 2>/dev/null | md5sum | cut -c1-12)
    [ "$a" = "$b" ] || echo "$f (drifts in both; a seat touched it)" >> "$T/attr-$os.txt"
  done
  n=$(wc -l < "$T/attr-$os.txt"); attrtot=$((attrtot + n))
  echo "$os: master-drift=$(wc -l < "$T/dm-$os") union-drift=$(wc -l < "$T/du-$os") union-attributable=$n csproj-in-union-drift=$(grep -c '\.csproj$' "$T/du-$os")"
  sed 's/^/    /' "$T/attr-$os.txt" | head -n 60
done
echo "UNION-ATTRIBUTABLE TOTAL: $attrtot"
echo "--- NAMED"
for os in linux darwin; do
  f=src/core/os/$os/file_unix.cs
  inM=$(grep -cxF "$f" "$T/dm-$os"); inU=$(grep -cxF "$f" "$T/du-$os")
  echo "K6 $f: in-M-drift=$inM (expected 1, pre-existing since TRAIN I) in-U-drift=$inU (expected 0 after the fixup refresh) :: $([ "$inU" = 0 ] && echo OK || echo "NOT REFRESHED")"
done
k9=$(cat "$T/attr-windows.txt" "$T/attr-linux.txt" "$T/attr-darwin.txt" | grep -c 'src/core/runtime/[a-z]*/package_info\.cs')
echo "K9 runtime package_info union-attributable: $k9 $([ "$k9" = 0 ] && echo "(OK: the union converter reproduces the line-wise resolution x3)" || echo "(CHECK: the resolved maps differ from the union's reconvert)")"
echo "--- REVIEW SIBLINGS (*.cs.auto; plant subtracted). Ruling 857afcb47e: every train's fixup refreshes the U arm's list; tK-fixup.sh carries NO sibling step, so the U list below is what that refresh still owes"
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/siblings-M-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/sm-$os"; cut -d' ' -f1 "$T/siblings-U-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/su-$os"
  echo "$os: M-siblings=$(wc -l < "$T/sm-$os") U-siblings=$(wc -l < "$T/su-$os") (U = the refresh owed; 0 only if nothing drifted)"
  sed 's/^/    U /' "$T/su-$os" | head -n 40
done
echo "--- COMMITTED CSPROJ, M vs U (FIXUP ORDER step 5)"
python "$(cygpath -w "$SD")/tK-helpers.py" csprojdrift "$(cygpath -w "$W")" "$M" "$U"; crc=$?
echo "CSPROJ rc=$crc"
echo "EMITCHECK VERDICT plants=$([ "$pbad" = 0 ] && echo OK || echo FAILED) emit=$([ "$emitbad" = 0 ] && echo OK || echo FAILED) union-attributable=$attrtot csproj=$([ "$crc" = 0 ] && echo OK || echo UNEXPLAINED)"
echo "EMITCHECK DONE"
[ "$pbad" = 0 ] && [ "$emitbad" = 0 ] && [ "$attrtot" = 0 ] && [ "$crc" = 0 ]
