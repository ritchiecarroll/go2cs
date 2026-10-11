#!/usr/bin/env bash
# TRAIN Q whole-corpus emission check (COORD, i7). DERIVED 2026-10-04 from trainO/tO-emitcheck.sh (O's bat2 read
# attributable 0, standing drift 1 / 3 / 3, 2815 s), itself DERIVED 2026-10-03 from trainN/tN-emitcheck.sh (from
# trainM/, from tL-emitcheck.sh): the same TWO ARMS, three targets each, every root seeded by git archive of its own
# commit, conversions strictly one at a time (floor 1). No line of logic changed at P (trainP/tP-CHANGES.md P1, P10).
# Q changes TWO things (trainQ/tQ-CHANGES.md Q1, Q10): the base arm is TRAIN P's landed line, and the drift reader
# (emitdrift.py) compares BYTES over the whole emitted root; the marker scan reads the written list AND the drift list.
#   arm M = M's converter over M's tree; arm U = U's converter over U's tree. (The arm letters are the check's own:
#   M = the base arm, U = the union arm; at Q the base arm is TRAIN P's landed line, passed in as M by the caller.)
#   main leg (tT3-battery.sh leg E):   M=<TRAIN P's landed line>  U=<Q fixup HEAD>            EXPECT attributable 0
#   fixup leg (tT3-fixup.sh step 4):   M=<TRAIN P's landed line>  U=<the ASSEMBLED union, before the fixup>  KEEP_U_ROOTS=1 CSPROJ_GATE=0
#       Here a union-attributable list is the corpus the union's converter emits and no seat committed, or committed
#       from another converter (at P: six rows' notes carry a CORPUS FOOTPRINT, every one committed in-seat; the fixup
#       composes REGEN_ALLOW from them, and any other listed path stops the fixup for COORD: tT3-README.md MS4).
# P (P10): written=<n> UNDER THE i9's INCREMENTAL WRITES. i9-incremental-cs-writes-r2 is a P row, so arm U's converter
# writes a converted source only when its bytes differ from the file already in the seeded root (writeSourceIfChanged);
# only the census path writes unconditionally (runCensusTarget sets alwaysWriteSources), and this check converts ONE
# target per run (main.go routes two or more -platforms targets to the census path, never one). So at P arm U's
# written= is small (the files that moved, the two plants, whatever the base arm also drifts on) while arm M, TRAIN
# O's converter, still writes every source (O read 1830 / 1901 / 1905 in both arms). What that changes, read line by line:
#   - DRIFT (written files whose bytes differ from the seed): unchanged. A file the U run does not write holds its
#     seed's bytes, and a file whose emission differs is written. The two plants append a line to the seeded root AND
#     the seed, so the emission differs from the root, is written, and reads as drift: the plant control still fires.
#   - markers, HANDOWN WRITTEN: a marker-bearing or hand-owned file is only a finding when it was written with
#     different bytes; both still are.
#   - EDITORCONFIG WRITTEN: csproj and .editorconfig were already written only when changed at O (needToWriteFile): O
#     read editorconfig-written=0 with ten committed entry files. Unchanged.
#   - 'which targets EMIT a path' can no longer be read from written-U-<os>.txt alone: tT3-regen-apply.py's rule 4 now
#     also reads written-M-<os>.txt (kept in the scratch beside it). This script's own verdicts never asked that.
# The written= figure is a READING. P asked what the train AFTER it needs (both arms then carry the i9's row). Q's
# answer (trainQ/tQ-CHANGES.md Q10, the i9's ruling): NO always-write flag. emitdrift.py reads drift from BYTES over the
# whole emitted root against its seed, so neither arm's written list decides anything here; at Q both arms' written=
# are small. tT3-regen-apply.py's rule 4 no longer reads written-M-<os>.txt (its header's ladder).
# CHANGED AT O (trainO/tO-CHANGES.md O9; N's Q16, applied because p2-converter-warning-clears is an O row): a per-file
# .editorconfig under src/core (the warning entries the CONVERTER writes beside a package's csproj) is ORDINARY EMISSION:
# emitdrift.py keeps it IN written/drift, so it is attributed, standing-drift-classified and copied like any other file.
# The EDCONF line ('EDITORCONFIG derived') and the EDITORCONFIG WRITTEN lines stay as READINGS, and the verdict's
# editorconfig-written=<n> is a count, no longer a gate anywhere (N died on it in the fixup and raised a finding in
# leg E, because no row of N's list carried the converter's entries).
# One residual, stated: the converter's file is shared by the three targets, each run replacing only its OWN GOOS's facts
# (warningEntries.go). If the union's facts for one GOOS differ from the committed file, the three U runs write three
# different files and tT3-regen-apply.py refuses the path ('per-target emission of one path'): the fixup STOPS for COORD
# (the remedy is one root converted for the three targets in turn, never a hand merge). p2's committed set is the
# expected reading (0 such paths).
#   E-bisect arms (run by tT3-battery.sh ONLY when the main leg's union-attributable total is > 0): one arm per named
#       emission seat, M=<the first parent of that seat's own merge on the union> U=<that merge>. The shas are read from
#       the union at run time; none is written in a script.
# Each arm's DRIFT = the files its conversion WRITES (newer than a sentinel, *.cs.auto excluded) whose CR-stripped bytes
# differ from that arm's own seed. Union-ATTRIBUTABLE = drift in U that is not in M, plus drift in both on a file whose
# committed bytes differ between the two trees. REVIEW SIBLINGS (*.cs.auto) are their own class (ruling 857afcb47e).
# CHANGED FROM TRAIN L (at TRAIN M, deliberately; trainM/tM-CHANGES.md):
#  * W/T are tQ's. KEEP_U_ROOTS=1 keeps the three U roots after the run (the fixup copies from them, then removes $T).
#  * HANDOWN is DERIVED from the two trees (every src/core .cs that differs between M and U and is golib, testing,
#    unsafe, an *_impl.cs companion or carries a line-anchored [module: GoManualConversion]), no longer a list of
#    TRAIN L's nine files. A -stdlib conversion must not write a hand-owned file: read, stated, never subtracted.
#  * L's K9 line is renamed RUNTIME-MAP and its premise re-derived: G's seat DOES re-encode runtime/*/package_info.cs
#    map lines and commits them, so they must not be union-attributable at the fixup HEAD either. Same count, M's reason.
#  * The GOROOT spelling regex takes the battery's bracket form (L's E:38 still held the older double-backslash form).
#  * CSPROJ_GATE=0 for the fixup leg and the bisect arms (their U is not a fixup commit; csprojdrift's question is the
#    main leg's).
# VERIFY ROUND 1: W is overridable (the fixup passes its own); each conversion's written files are scanned for an
# unresolved deferred marker, and a hit fails the emission (EMIT ... markers=N, the list in markers-<arm>-<os>.txt);
# the hand-owned prefix under testing/ is the package's own files, not its converted sub-packages.
# VERIFY ROUND 2 (COORD's ruling: a hand launch is as safe as a call from the battery): the script ran six -stdlib
# conversions with no lock, no LIVE gate, no launch refusal and no disk preflight of its own, safe only under a caller
# that held the lock, while two texts send COORD to run it by hand (a further bisect arm; "re-run the emission check
# alone on the box"). tT3-fixup.sh and tT3-battery.sh now pass TFL_LOCK_HELD=1; WITHOUT it the script refuses a launch
# from a worktree, hnd or a draft folder (floor 4), reads the LIVE gate (LIVE_ACK=1 runs past a hit), takes TRAIN Q's
# lock and checks the disk (30G on H:, 8G on C:), and its EXIT trap releases the lock it took.
set -u
M=${M:?master sha}; U=${U:?union sha}
TAG=${TAG:-emitcheck}
CSPROJ_GATE=${CSPROJ_GATE:-1}
KEEP_U_ROOTS=${KEEP_U_ROOTS:-0}
SD=$(cd "$(dirname "$0")" && pwd)   # absolute: the script cd's into $W below
W=${W:-/h/go2cs-tmp-coord/tT3}   # verify round 1: overridable; tT3-fixup.sh and tT3-battery.sh pass their own W
# REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-9): only under a tQ name (TRAIN Q's worktree or a rehearsal worktree
# /h/go2cs-tmp-coord/tT3-<name>). A W left exported from another train's shell would have this script cd into THAT
# worktree and read its commits; refused here, before the lock and before any cd.
case "${W%/*}:${W##*/}" in
  /h/go2cs-tmp-coord:tT3|/h/go2cs-tmp-coord:tT3[-_.]?*) ;;
  *) echo "ABORT: W=$W is not TRAIN T3's worktree (/h/go2cs-tmp-coord/tT3, or a rehearsal worktree /h/go2cs-tmp-coord/tT3-<name>): unset W (a value left exported from another train's shell?)"; exit 2 ;;
esac
T=${TT3_EMIT_SCRATCH:-/h/go2cs-tmp-coord/tT3emitfull}
case "$T" in /h/go2cs-tmp-coord/tT3emit*) ;; *) echo "ABORT scratch $T is not under /h/go2cs-tmp-coord/tT3emit*"; exit 2;; esac
# Verify round 2: the guards of a HAND launch (floors 1, 4, 11, 12). A caller that holds the lock says so.
LOCK=/h/go2cs-tmp-coord/coord-scratch/tT3/.battery.lock; OWNLOCK=0
if [ "${TT3_LOCK_HELD:-0}" != 1 ]; then
  case "$SD" in
    /h/go2cs-tmp-coord/tT3|/h/go2cs-tmp-coord/tT3/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
      echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
  esac
  [ -f "$SD/tT3-helpers.py" ] && [ -f "$SD/emitdrift.py" ] || { echo "ABORT: tT3-helpers.py or emitdrift.py missing from $SD"; exit 2; }
  if [ "${LIVE_ACK:-0}" != 1 ]; then
    lv=$(python -B "$(cygpath -w "$SD/tT3-helpers.py")" live "$(cygpath -w "$LOCK")" 2>&1); lrc=$?
    if [ "$lrc" != 0 ]; then
      echo "ABORT (floor 1): another conversion, harness or battery looks alive on this box (listed, not killed; LIVE_ACK=1 runs past it by COORD's explicit call):"
      printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE' | head -n 12
      exit 2
    fi
  fi
  mkdir -p "$(dirname "$LOCK")"
  mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- a TRAIN T3 battery, fixup, module-legs run or emission check holds it; rmdir it only after checking"; exit 2; }
  OWNLOCK=1; trap 'rmdir "$LOCK" 2>/dev/null' EXIT   # replaced below by archive_and_clean, which releases the lock too
  fh=$(powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null < /dev/null | tr -d '\r'); fc=$(powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null < /dev/null | tr -d '\r')
  [ "${fh:-0}" -ge 30 ] && [ "${fc:-0}" -ge 8 ] || { echo "ABORT: free disk H:=${fh:-unread}G (floor 30) C:=${fc:-unread}G (floor 8)"; exit 3; }
  echo "HAND LAUNCH (no TT3_LOCK_HELD): floor-4 refusal passed, LIVE gate $([ "${LIVE_ACK:-0}" = 1 ] && echo 'ACKNOWLEDGED (LIVE_ACK=1)' || echo 'read: none'), lock taken, free H:=${fh}G C:=${fc}G"
fi
PLANT_SIB=src/core/sync/pool.cs.auto
PLANT_CS=src/core/unicode/utf8/utf8.cs
PLANT_LINE='// TRAIN T3 emission-check plant (positive control; never committed)'
# DRAFT 2 (critic 1 #5): the GOROOT spelling is checked on its VALUE before the export (a `go env GOROOT` after the
# export only echoes the env back); the expected spelling is derived from $HOME, never written out.
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) echo "ABORT GOROOT spelling '$GR' has a forward slash (floor 6)"; exit 2 ;; esac
[[ "$GR" =~ ^[A-Za-z]:[\\].*[\\]sdk[\\]go1\.24\.13$ ]] || { echo "ABORT GOROOT spelling '$GR'"; exit 2; }
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || { echo "ABORT GOROOT VERSION is not go1.24.13"; exit 2; }
[ "$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')" = "$GR" ] || { echo "ABORT the toolchain's own GOROOT spelling != '$GR'"; exit 2; }
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { echo "ABORT go on PATH is $(go version)"; exit 2; }
cd "$W" || { echo "ABORT no worktree $W"; exit 2; }
for f in "$PLANT_SIB" "$PLANT_CS"; do
  git cat-file -e "$M:$f" 2>/dev/null && git cat-file -e "$U:$f" 2>/dev/null || { echo "ABORT plant target $f absent from an arm"; exit 2; }
done
# HANDOWN, derived (see the header): the hand-owned src/core files the two trees differ in. No path here holds a space.
HANDOWN=$(git -c core.quotepath=false diff --name-only "$M" "$U" -- src/core | grep -E '\.cs$' | while IFS= read -r f; do
  # verify round 1: testing's SUB-packages are converted (TRAIN L's windows arm wrote 13 files under them and not
  # testing.cs); only the files directly in src/core/testing/ are the hand-owned package.
  case "$f" in src/core/golib/*|src/core/unsafe/*|*_impl.cs) echo "$f"; continue ;; src/core/testing/*/*) ;; src/core/testing/*) echo "$f"; continue ;; esac
  git cat-file -e "$U:$f" 2>/dev/null || continue
  git show "$U:$f" | grep -qE '^\[module: (go\.)?GoManualConversion\]' && echo "$f"
done | tr '\n' ' ')
echo "HANDOWN derived ($M..$U): $(echo $HANDOWN | wc -w) file(s): ${HANDOWN:-none}"
# O9: the per-file .editorconfig files under src/core the two trees differ in (a READING: ordinary emission since O, see
# the header; at P: p2-test-warning-entries' two, math/bits and weak, when that row is in the union).
EDCONF=$(git -c core.quotepath=false diff --name-only "$M" "$U" -- src/core | grep -E '(^|/)\.editorconfig$' | tr '\n' ' ')
echo "EDITORCONFIG derived ($M..$U): $(echo $EDCONF | wc -w) file(s) (converter emission, drift-compared like any file since O): ${EDCONF:-none}"
rm -rf "$T"; mkdir -p "$T"
EVD="${LOGDIR:-$SD/tT3-logs}/$TAG"
archive_and_clean(){
  local rc=$? f n=0
  mkdir -p "$EVD"
  for f in "$T"/drift-* "$T"/siblings-* "$T"/written-* "$T"/attr-* "$T"/dm-* "$T"/du-* "$T"/sm-* "$T"/su-* "$T"/sa-* "$T"/emit-* "$T"/markers-* "$T"/editorconfig-* "$T"/sd-*; do
    [ -f "$f" ] && cp "$f" "$EVD/" && n=$((n + 1))
  done
  rm -rf "$T"/M-windows "$T"/M-linux "$T"/M-darwin "$T"/seed-M "$T"/seed-U "$T"/src-M "$T"/src-U
  if [ "$KEEP_U_ROOTS" = 1 ]; then
    echo "KEEP_U_ROOTS=1: $T/U-windows, U-linux and U-darwin are KEPT for the caller, which removes $T when it has read them"
  else
    rm -rf "$T"/U-windows "$T"/U-linux "$T"/U-darwin
  fi
  echo "EVIDENCE copied $n file(s) to $EVD; roots, seeds and source archives removed from $T (exes kept) rc=$rc"
  [ "$OWNLOCK" = 1 ] && rmdir "$LOCK" 2>/dev/null   # verify round 2: only the lock a hand launch took itself
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
    git archive "$sha" src/core src/version.props docs/validation docs/ValidatedTestPackages.md | tar -x -C "$R" || { echo "ABORT seed $arm $os"; exit 3; }
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
  python "$(cygpath -w "$SD")/emitdrift.py" "$(cygpath -w "$R")" "$(cygpath -w "$T/seed-$arm")" "$(cygpath -w "$T/sentinel-$arm-$os")" "$(cygpath -w "$T/written-$arm-$os.txt")" "$(cygpath -w "$T/drift-$arm-$os.txt")" "$(cygpath -w "$T/siblings-$arm-$os.txt")" "$(cygpath -w "$T/editorconfig-$arm-$os.txt")" "$(cygpath -w "$T/dcopy-$arm-$os")" > /dev/null || { echo "ABORT drift $arm $os"; exit 3; }
  refuse=$(grep -ac 'Refusing to convert' "$T/emit-$arm-$os.log")
  # Verify round 1 (floor 1's signature): a raced or interrupted conversion exits 0 and leaves ONE written file holding
  # an unresolved deferred marker (the converter's two prefixes: dynamicTypeOperations.go:25, adapterNameCollisions.go:41;
  # the left guillemet is the two bytes C2 AB). Counted over every file this conversion wrote; review siblings are not
  # in that list (src/core/internal/godebug/godebug.cs.auto:185 is the one standing sibling hit). Any hit fails the emission.
  ( cd "$R" && { cat "$T/written-$arm-$os.txt"; grep -v ' DELETED$' "$T/drift-$arm-$os.txt" | sed 's/ NEW$//'; } | LC_ALL=C sort -u | tr '\n' '\0' | LC_ALL=C xargs -0 -r grep -laF -e $'\xc2\xabDYNTYPE:' -e $'\xc2\xabADAPTER:' ) > "$T/markers-$arm-$os.txt" 2>/dev/null
  mk=$(grep -c . "$T/markers-$arm-$os.txt")
  [ "$rc" = 0 ] && [ "$refuse" = 0 ] && [ "$mk" = 0 ] || emitbad=$((emitbad + 1))
  pc="plants:"; grep -qxF "$PLANT_SIB" "$T/siblings-$arm-$os.txt" && pc="$pc sib=OK" || pc="$pc sib=MISSING"
  grep -qxF "$PLANT_CS" "$T/drift-$arm-$os.txt" && pc="$pc cs=OK" || pc="$pc cs=MISSING"
  echo "EMIT $arm-$os rc=$rc refusals=$refuse markers=$mk wall=$(( $(date +%s) - s ))s written=$(wc -l < "$T/written-$arm-$os.txt") drift=$(cut -d' ' -f1 "$T/drift-$arm-$os.txt" | grep -vcxF "$PLANT_CS") siblings=$(cut -d' ' -f1 "$T/siblings-$arm-$os.txt" | grep -vcxF "$PLANT_SIB") (plants excluded) $pc :: $(tail -n 1 "$T/emit-$arm-$os.log" | tr -d '\r' | cut -c1-140)"
done; done
pbad=$(for arm in M U; do for os in windows linux darwin; do grep -qxF "$PLANT_SIB" "$T/siblings-$arm-$os.txt" || echo x; grep -qxF "$PLANT_CS" "$T/drift-$arm-$os.txt" || echo x; done; done | wc -l)
echo "PLANT CONTROL: $([ "$pbad" = 0 ] && echo "OK (both plants read as drift in all six conversions)" || echo "FAILED ($pbad of 12 plant readings missing) -- the drift instrument is not measuring; every count below is VOID")"
echo "EMIT RC/REFUSAL: $([ "$emitbad" = 0 ] && echo "OK (6 conversions rc=0, 0 S3 refusals, 0 unresolved markers)" || echo "FAILED on $emitbad conversion(s) (rc, an S3 refusal or an unresolved deferred marker: markers-<arm>-<os>.txt)")"
echo "--- UNION-ATTRIBUTABLE (per target; plants subtracted)"
# FIXUP STOP 2026-10-03 (COORD): a file BOTH arms drift on, whose committed bytes a seat changed, was attributed to the
# union outright. At N's fixup that listed log/syslog's csproj on linux and darwin: two seats edited its ReadyToRun and
# TrimMode lines (reproduced byte for byte by the windows emission), while the drift itself is the InternalsVisibleTo
# block the converter emits only where the package has same-package tests, the SAME drift at the base. Such a file is now
# STANDING drift when the base arm's drift hunks (committed -> emitted, positions dropped) equal the union arm's: listed in
# sd-<os>.txt, counted, not attributable. A missing copy, or any difference in the hunks, keeps it attributable.
drifthunks(){ # <committed sha> <path> <emitted copy> -- the -U0 hunk bodies, @@ positions and file headers dropped
  [ -f "$3" ] || { echo "MISSING-COPY"; return; }
  diff -U0 <(git show "$1:$2" 2>/dev/null | tr -d '\r') "$3" | sed -e '1,2d' -e '/^@@/d'
}
samedrift(){ # <M sha> <U sha> <path> <M copy> <U copy> -- rc 0 only when both drifts exist and are the same hunks
  local a b; a=$(drifthunks "$1" "$3" "$4"); b=$(drifthunks "$2" "$3" "$5")
  [ -n "$a" ] && [ "$a" != MISSING-COPY ] && [ "$a" = "$b" ]
}
# Control (floor 13), every launch: three hermetic cases through samedrift itself, on a throwaway repository.
sdc="$T/sdcontrol"; rm -rf "$sdc"; mkdir -p "$sdc"
sm=$(cd "$sdc" && git init -q . && git config user.email c@c && git config user.name c && git config commit.gpgsign false \
     && printf 'a\nb\n' > f && git add f && git commit -qm m && git rev-parse HEAD) \
  && su=$(cd "$sdc" && printf 'a2\nb\n' > f && git commit -qam u && git rev-parse HEAD) \
  || { echo "ABORT standing-drift control could not build its repository"; exit 3; }
printf 'a\nX\nb\n' > "$sdc/eM"; printf 'a2\nX\nb\n' > "$sdc/eU"; printf 'a2\nY\nb\n' > "$sdc/eU2"
sdv=$(cd "$sdc" && { samedrift "$sm" "$su" f eM eU && echo same || echo diff; samedrift "$sm" "$su" f eM eU2 && echo same || echo diff; samedrift "$sm" "$su" f eM nonexistent && echo same || echo diff; } | tr '\n' ' ')
rm -rf "$sdc"
[ "$sdv" = "same diff diff " ] || { echo "ABORT standing-drift CONTROL did not read 'same diff diff' (read: '$sdv'): the classifier is not measuring"; exit 3; }
echo "STANDING-DRIFT CONTROL: OK (equal hunks -> standing; a changed hunk -> attributable; a missing copy -> attributable)"
attrtot=0; sdtot=0
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/drift-M-$os.txt" | grep -vxF "$PLANT_CS" | LC_ALL=C sort -u > "$T/dm-$os"; cut -d' ' -f1 "$T/drift-U-$os.txt" | grep -vxF "$PLANT_CS" | LC_ALL=C sort -u > "$T/du-$os"
  LC_ALL=C comm -13 "$T/dm-$os" "$T/du-$os" > "$T/attr-$os.txt"; : > "$T/sd-$os.txt"
  LC_ALL=C comm -12 "$T/dm-$os" "$T/du-$os" | while IFS= read -r f; do
    a=$(git show "$M:$f" 2>/dev/null | md5sum | cut -c1-12); b=$(git show "$U:$f" 2>/dev/null | md5sum | cut -c1-12)
    [ "$a" = "$b" ] && continue
    if samedrift "$M" "$U" "$f" "$T/dcopy-M-$os/$f" "$T/dcopy-U-$os/$f"; then
      echo "$f (standing drift: a seat touched it; the base arm's drift hunks equal the union arm's)" >> "$T/sd-$os.txt"
    else
      echo "$f (drifts in both; a seat touched it; the drift hunks differ)" >> "$T/attr-$os.txt"
    fi
  done
  n=$(wc -l < "$T/attr-$os.txt"); attrtot=$((attrtot + n)); ns=$(wc -l < "$T/sd-$os.txt"); sdtot=$((sdtot + ns))
  echo "$os: master-drift=$(wc -l < "$T/dm-$os") union-drift=$(wc -l < "$T/du-$os") union-attributable=$n standing-seat-touched=$ns csproj-in-union-drift=$(grep -c '\.csproj$' "$T/du-$os")"
  sed 's/^/    /' "$T/attr-$os.txt" | head -n 60
  sed 's/^/    STANDING /' "$T/sd-$os.txt" | head -n 20
done
echo "STANDING-DRIFT (seat-touched, drift unchanged) TOTAL: $sdtot"
echo "UNION-ATTRIBUTABLE TOTAL: $attrtot"
echo "--- NAMED"
k9=$(cat "$T/attr-windows.txt" "$T/attr-linux.txt" "$T/attr-darwin.txt" | grep -c 'src/core/runtime/[a-z]*/package_info\.cs')
echo "RUNTIME-MAP runtime package_info union-attributable: $k9 $([ "$k9" = 0 ] && echo "(OK)" || echo "(CHECK: the union converter re-encodes a runtime position map the U tree does not carry; G's seat commits its own)")"
hw=0
# REVIEW ROUND 1 (Q10's own rule, applied here too): the question 'did a conversion touch a hand-owned file' is read
# from BYTES as well as from time. The written-by-time list alone missed nothing a time can show, but under an
# incremental converter time is a reading; the drift list (bytes against the seed, the whole root) is the measurement,
# as for the marker scan above. A file in EITHER list counts, and the line says which.
for f in $HANDOWN; do
  for arm in M U; do for os in windows linux darwin; do
    hk=''
    grep -qxF "$f" "$T/written-$arm-$os.txt" && hk=WRITTEN
    cut -d' ' -f1 "$T/drift-$arm-$os.txt" | grep -qxF "$f" && hk="${hk:+$hk+}DRIFT"
    [ -z "$hk" ] || { echo "HANDOWN $hk $arm-$os $f"; hw=$((hw + 1)); }
  done; done
done
echo "HANDOWN hand-owned files the two trees differ in, written by any arm (by time) or differing from their seed in any emitted root (by bytes): $hw (EXPECT 0: -stdlib leaves hand-owned files alone)"
# O9: a .editorconfig under src/core written by ANY conversion (emitdrift.py's seventh list: newer than the sentinel), with
# its NEW / changed / same tag. A READING since O: the U arm writes one for every package whose facts it derives (p2's
# eleven, 'same' when the committed file is what the union emits); the M arm (N's converter, no warningEntries.go)
# writes none. (P: both arms carry warningEntries.go and neither rewrites an unchanged entry file: O's bat2 read 0.) Whether a written file DRIFTS is decided above, in the union-attributable lists.
# Review round 1 (F1-2): emitdrift.py also lists a COMMITTED .editorconfig a conversion DELETED (tag DELETED; it is in the
# drift lists too, so it is attributed above like any drifting path). Stated on its own line kind and counted apart.
ew=0; ed=0
for arm in M U; do for os in windows linux darwin; do
  while IFS= read -r l; do
    [ -n "$l" ] || continue
    case "$l" in
      *' DELETED') echo "EDITORCONFIG DELETED $arm-$os ${l% DELETED} (a committed file the conversion removed: its model came out empty)"; ed=$((ed + 1)) ;;
      *) echo "EDITORCONFIG WRITTEN $arm-$os $l"; ew=$((ew + 1)) ;;
    esac
  done < <(cat "$T/editorconfig-$arm-$os.txt" 2>/dev/null)
done; done
echo "EDITORCONFIG .editorconfig files under src/core written by any arm: $ew; committed ones a conversion DELETED: $ed (READINGS since O: ordinary emission; drift, a deletion included, is read in the union-attributable lists)"
echo "--- REVIEW SIBLINGS (*.cs.auto; plant subtracted). Ruling 857afcb47e: the U list is what the train's sibling refresh owes"
for os in windows linux darwin; do
  cut -d' ' -f1 "$T/siblings-M-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/sm-$os"; cut -d' ' -f1 "$T/siblings-U-$os.txt" | grep -vxF "$PLANT_SIB" | LC_ALL=C sort -u > "$T/su-$os"
  LC_ALL=C comm -13 "$T/sm-$os" "$T/su-$os" > "$T/sa-$os"   # M: the siblings only the UNION arm rewrites (this train's own)
  echo "$os: M-siblings=$(wc -l < "$T/sm-$os") U-siblings=$(wc -l < "$T/su-$os") U-not-M=$(wc -l < "$T/sa-$os") (U = the refresh owed; 0 only if nothing drifted; U-not-M = this train's share)"
  sed 's/^/    U /' "$T/su-$os" | head -n 40
done
echo "--- COMMITTED CSPROJ, M vs U"
python "$(cygpath -w "$SD")/tT3-helpers.py" csprojdrift "$(cygpath -w "$W")" "$M" "$U"; crc=$?
echo "CSPROJ rc=$crc (gate=$CSPROJ_GATE)"
# >>> csprojword (tT3-controls.sh arm EUG extracts the lines between the markers and runs its four arms)
# FL (FL5; TRAIN Q's battery, ledger 23:12 on 2026-10-06): the fixup's own run of this check printed the csproj
# classifier's UNEXPLAINED line at 20:32 with gate=0 and summarized it as csproj=OK, so the red first surfaced hours
# later in the battery's leg E. The WORD is the classifier's, whatever the gate: OK only on rc 0; UNEXPLAINED when the
# gate is on; UNEXPLAINED-UNGATED when the gate is off (the verdict's rc still ignores it then: the gate decides the
# rc, never the word). csprojword <classifier rc> <gate 0|1> -> the word.
csprojword(){ if [ "$1" = 0 ]; then echo OK; elif [ "$2" = 1 ]; then echo UNEXPLAINED; else echo UNEXPLAINED-UNGATED; fi; }
# <<< csprojword
CSWORD=$(csprojword "$crc" "$CSPROJ_GATE")
[ "$CSWORD" != UNEXPLAINED-UNGATED ] || echo "CSPROJ NOTE: UNEXPLAINED csproj drift with CSPROJ_GATE=0: NOT a pass; the caller (tT3-fixup.sh step 4) stamps this word and COORD reads the OTHER lines above before anything is committed"
[ "$CSPROJ_GATE" = 1 ] || crc=0
echo "EMITCHECK VERDICT tag=$TAG plants=$([ "$pbad" = 0 ] && echo OK || echo FAILED) emit=$([ "$emitbad" = 0 ] && echo OK || echo FAILED) union-attributable=$attrtot handown-written=$hw editorconfig-written=$ew editorconfig-deleted=$ed csproj=$CSWORD"
echo "EMITCHECK DONE"
# HANDOWN is a READING (stated in the verdict line), not part of rc: a new gate is COORD's call, never the draft's.
[ "$pbad" = 0 ] && [ "$emitbad" = 0 ] && [ "$attrtot" = 0 ] && [ "$crc" = 0 ]
