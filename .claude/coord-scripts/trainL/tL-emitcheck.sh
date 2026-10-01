#!/usr/bin/env bash
# TRAIN L whole-corpus emission check (COORD, i7). DRAFT 2 2026-10-01, adapted from tK-emitcheck.sh: the same TWO ARMS,
# three targets each, every root seeded by git archive of its own commit, conversions strictly one at a time (floor 1).
#   arm M = M's converter over M's tree; arm U = U's converter over U's tree.
#   main leg:      M=75648a022b (K master)  U=<L fixup HEAD>
#   E-bisect arms (run by tL-battery.sh ONLY when the main leg's union-attributable total is > 0):
#     D6 alone:        M=75648a022b  U=566e28d894   (the union's FIRST merge: K master + r-d6-for-clause-spill)
#     crosspkg alone:  M=8f73f7e88f  U=bb518139bf   (the merge of i9-crosspkg-value-promotion and its first parent)
# Each arm's DRIFT = the files its conversion WRITES (newer than a sentinel, *.cs.auto excluded) whose CR-stripped bytes
# differ from that arm's own seed. Union-ATTRIBUTABLE = drift in U that is not in M, plus drift in both on a file whose
# committed bytes differ between the two trees. REVIEW SIBLINGS (*.cs.auto) are their own class (ruling 857afcb47e).
# CHANGED FROM TRAIN K (deliberately):
#  * W/T are tL's; TAG names the evidence subfolder so the bisect arms never overwrite the main leg's evidence.
#  * K6 is DROPPED: K's fixup refreshed os/{linux,darwin}/file_unix.cs at K, so both L arms carry it (no L seat touches
#    os). K9 (runtime/*/package_info.cs not union-attributable) is KEPT as a regression read: no L seat touches a runtime
#    map (L's only runtime file is the hand-owned goenvs_impl.cs, which -stdlib does not write).
#  * L's corpus edits are all HAND-OWNED (golib, testing, time_impl.cs, goenvs_impl.cs, internal/godebug/godebug.cs
#    [module: GoManualConversion]): a -stdlib conversion must not write them, so they must not appear in either arm's
#    written list. They are listed by name below (HANDOWN), read, never subtracted.
#  * CSPROJ_GATE=0 for a bisect arm (566e28d894 predates the fixup: ForClauseSpill.csproj still carries
#    <LangVersion>latest</LangVersion>, which csprojdrift reads as new-MISSING-EDIT by design).
set -u
M=${M:?master sha}; U=${U:?union sha}
TAG=${TAG:-emitcheck}
CSPROJ_GATE=${CSPROJ_GATE:-1}
SD=$(cd "$(dirname "$0")" && pwd)   # absolute: the script cd's into $W below
W=/h/go2cs-tmp-coord/tL
T=${TL_EMIT_SCRATCH:-/h/go2cs-tmp-coord/tLemitfull}
case "$T" in /h/go2cs-tmp-coord/tLemit*) ;; *) echo "ABORT scratch $T is not under /h/go2cs-tmp-coord/tLemit*"; exit 2;; esac
PLANT_SIB=src/core/sync/pool.cs.auto
PLANT_CS=src/core/unicode/utf8/utf8.cs
PLANT_LINE='// TRAIN L emission-check plant (positive control; never committed)'
HANDOWN='src/core/golib/GoDefaultGodebugAttribute.cs src/core/golib/channel.cs src/core/golib/runtime/Goroutine.cs src/core/internal/godebug/godebug.cs src/core/runtime/goenvs_impl.cs src/core/testing/PackageAncestry.cs src/core/testing/TestHost.cs src/core/testing/TestRegistry.cs src/core/time/time_impl.cs'
# DRAFT 2 (critic 1 #5): the GOROOT spelling is checked on its VALUE before the export (a `go env GOROOT` after the
# export only echoes the env back); the expected spelling is derived from $HOME, never written out.
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) echo "ABORT GOROOT spelling '$GR' has a forward slash (floor 6)"; exit 2 ;; esac
[[ "$GR" =~ ^[A-Za-z]:\\.*\\sdk\\go1\.24\.13$ ]] || { echo "ABORT GOROOT spelling '$GR'"; exit 2; }
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || { echo "ABORT GOROOT VERSION is not go1.24.13"; exit 2; }
[ "$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')" = "$GR" ] || { echo "ABORT the toolchain's own GOROOT spelling != '$GR'"; exit 2; }
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { echo "ABORT go on PATH is $(go version)"; exit 2; }
cd "$W" || exit 2
for f in "$PLANT_SIB" "$PLANT_CS"; do
  git cat-file -e "$M:$f" 2>/dev/null && git cat-file -e "$U:$f" 2>/dev/null || { echo "ABORT plant target $f absent from an arm"; exit 2; }
done
rm -rf "$T"; mkdir -p "$T"
EVD="${LOGDIR:-$SD/tL-logs}/$TAG"
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
  # critic 1 #15: the native exe gets a Windows path explicitly (no reliance on MSYS's automatic path conversion, which
  # MSYS_NO_PATHCONV=1 in the caller's environment would silently switch off).
  "$T/go2cs-$arm.exe" -stdlib -comments -platforms "$os/amd64" -go2cspath "$(cygpath -w "$R/src")" > "$T/emit-$arm-$os.log" 2>&1; rc=$?
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
k9=$(cat "$T/attr-windows.txt" "$T/attr-linux.txt" "$T/attr-darwin.txt" | grep -c 'src/core/runtime/[a-z]*/package_info\.cs')
echo "K9 (regression) runtime package_info union-attributable: $k9 $([ "$k9" = 0 ] && echo "(OK)" || echo "(CHECK: an L seat moved a runtime map)")"
hw=0
for f in $HANDOWN; do
  for arm in M U; do for os in windows linux darwin; do grep -qxF "$f" "$T/written-$arm-$os.txt" && { echo "HANDOWN WRITTEN $arm-$os $f"; hw=$((hw + 1)); }; done; done
done
echo "HANDOWN L-touched hand-owned files written by any arm: $hw (EXPECT 0: -stdlib leaves hand-owned files alone)"
echo "--- REVIEW SIBLINGS (*.cs.auto; plant subtracted). Ruling 857afcb47e: the U list is what the train's sibling refresh owes"
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/siblings-M-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/sm-$os"; cut -d' ' -f1 "$T/siblings-U-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/su-$os"
  echo "$os: M-siblings=$(wc -l < "$T/sm-$os") U-siblings=$(wc -l < "$T/su-$os") (U = the refresh owed; 0 only if nothing drifted)"
  sed 's/^/    U /' "$T/su-$os" | head -n 40
done
echo "--- COMMITTED CSPROJ, M vs U"
python "$(cygpath -w "$SD")/tL-helpers.py" csprojdrift "$(cygpath -w "$W")" "$M" "$U"; crc=$?
echo "CSPROJ rc=$crc (gate=$CSPROJ_GATE)"
[ "$CSPROJ_GATE" = 1 ] || crc=0
echo "EMITCHECK VERDICT tag=$TAG plants=$([ "$pbad" = 0 ] && echo OK || echo FAILED) emit=$([ "$emitbad" = 0 ] && echo OK || echo FAILED) union-attributable=$attrtot handown-written=$hw csproj=$([ "$crc" = 0 ] && echo OK || echo UNEXPLAINED)"
echo "EMITCHECK DONE"
# HANDOWN is a READING (stated in the verdict line), not part of rc: a new gate is COORD's call, never the draft's.
[ "$pbad" = 0 ] && [ "$emitbad" = 0 ] && [ "$attrtot" = 0 ] && [ "$crc" = 0 ]
