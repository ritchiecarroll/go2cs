#!/usr/bin/env bash
# TRAIN Q -- the module legs (leg MOD of tFL-battery.sh). DERIVED 2026-10-06 from trainP/tP-modules-legs.sh (P's run2
# read every verdict PASS but ONE, x/mod sumdb/tlog, an EXTERNAL oracle failure). Q's changes (trainQ/tQ-CHANGES.md):
# the base is TRAIN P's landed line (Q1); every Go ORACLE of this script is built with the tag set the union's
# converter gives the same conversion, 'safe' included (Q16: g-module-safe-tag is a Q row, and a count that moved
# because one side built other files would be an instrument's red); a Go-side oracle failure with a network or quota
# text is KNOWN-EXTERNAL, named and OWED, never passed (Q4); x/sync is read a SECOND time into the same output root
# (XS2, Q7), which is also the control run of the timing class; a symbol-file refusal is counted (Q15). NOT RUN by its
# author. At Q these legs are the regression of the -tests -recurse driver under eleven rows that edit
# testConversion.go and four that rewrite the hand-owned testing package.
# P's header: DERIVED 2026-10-04 from trainO/tO-modules-legs.sh (names and
# the text that names the base: trainP/tP-CHANGES.md R0, P1; no leg changed), itself DERIVED 2026-10-03 from
# trainN/tN-modules-legs.sh (from trainM/, from tL-modules-legs.sh). At P these legs were the
# regression of the -tests -recurse driver under four rows that edit -tests conversion (c2-sibling-package-name-r2,
# c2-embed-promoted-refs, c2-lifted-iface-test-cast, i9-incremental-cs-writes-r2) and P2's diagnostics rows.
# O's change (trainO/tO-CHANGES.md O1): BASE is REQUIRED (the previous train's
# landed master: TRAIN O's at P; the battery passes it, a standalone run names it); the landed H1/H2 host seats are found on its
# first-parent history exactly as at N (they landed with M, below N). N's changes (trainN/tN-CHANGES.md): the base was TRAIN M's landed master;
# the two host seats H1/H2 LANDED with M, so when the seat list does not hold them they are read from master's
# first-parent merges (the controls' stream reader and the XS/XM expectations then stay what they were in M's run3);
# the regression these legs read is now also r-module-driver-gomod-less's (it edits the driver and the testing host).
# Two halves:
#  (1) TRAIN L's legs, UNCHANGED in content: the R module chain's readings (M2 lock, M3 -tests -recurse e2e, M4 module
#      ancestry e2e, M6 DefaultGODEBUG e2e + program arm) and the converter suite's module tests READ BY NAME. Those
#      seats are on master now; the legs stay as the REGRESSION of the -tests -recurse driver, whose test conversion M's
#      F4 (publishTestHost's deadline) and F8 (declarationClosureImports) edit. Their two control converters are still
#      built from TRAIN L's seat shas, which are ancestors of TRAIN L's master (verify round 3: plus the H2 seat's own
#      stream-reader change when that seat is a row, so they can read the union host's framed events).
#  (2) NEW in M, at the end: XS and XM, two REAL third-party modules through `go2cs -tests -recurse <moduleDir>
#      <outRoot>` -- golang.org/x/sync@v0.19.0 and golang.org/x/mod@v0.33.0, from the H: module cache (R6: exact paths,
#      GOPROXY=off, never a download). They are P2's fix batch END ACCEPTANCE (tL-seats-draft.txt:67), the first gate
#      that reads a real module on this box, and the leg TRAIN L's own blocker asked for ("NEW GATE for the next battery
#      template: a -tests run of a real module-cache module", tL-seats-draft.txt:23).
# COORD reviews, copies into a per-run folder (floor 4), and runs it AFTER (or as a tail of) the main battery, on the
# i7, one battery at a time:
#   EXPECT_HEAD=<union head, 10 chars> bash tFL-modules-legs.sh            # main legs
#   EXPECT_HEAD=<...> CONTROLS=1 bash tFL-modules-legs.sh                   # + the two discriminating controls
#   ... REALMOD=0 ...                                                     # L's legs only (no real module)
# Env, leg() and purge semantics are tK-reread.sh's (verbatim where marked). Fixtures and output roots live in the
# run folder ($SD/fx, $SD/out), OUTSIDE every worktree; the only tree write is the converter binary under
# src/go2cs/bin (as in K's T legs), purged at the end. rc captured per leg before any pipe (floor 7).
# It exits 4 when any verdict FAILED (the battery's MOD leg then reaches NONZERO LEGS); a KNOWN verdict is not a
# failure. REVIEW ROUND 1: mod-logs/OWED-rereads.txt is ALWAYS created (empty = nothing owed; its absence is never a
# clean state), and a STANDALONE run that owes a re-read and failed nothing exits 7, never 0 (under the battery it
# stays 0 and the battery's own END carries the owed lines to EXIT 7). It refuses a run folder that already holds mod-logs/SUMMARY.txt; takes the battery lock when run standalone
# (and refuses beside a running battery unless the battery launched it, IN_BATTERY=1); checks the GOROOT spelling before
# the export; honours DEADLINE (R5: no leg STARTS after it, exit 9). Control converters build under
# $CTL (coord-scratch/tFL), never a worktree.
# VERIFY ROUND 2 (trainM/tM-CHANGES.md section 10; COORD's rulings):
#  * A standalone launch gets the battery's floor-4 refusal (it wrote mod-logs, fx and out beside itself, which from hnd
#    is inside a git worktree).
#  * DEADLINE is an ABSOLUTE local time, 'YYYY-MM-DD HH:MM', resolved once to epoch seconds; a deadline already past is
#    refused; unset = launch + 14 h. Under the battery the resolved DL_EPOCH arrives in the environment.
#  * XS and host finding H1: golang.org/x/sync/singleflight is RUN only when the H1 host seat is a row of the seat list
#    (a ref p2-*test-list*, or a row noted H1); then 12 of 12 is expected. With no such row the package is NOT RUN on
#    this box (its folder is removed from the module COPY, stated) and stamped 'NOT RUN (KNOWN: H1, child fan-out)'.
#    The H2 host seat is read the same way for sumdb/tlog (17 expected when it is a row; KNOWN otherwise).
#  * After the last leg: a census of processes BY EXECUTABLE PATH under the module out root. A survivor is a FAIL
#    verdict, listed by PID and path, never killed (floor 5), and the lock is KEPT.
#  * END also fails on any change or untracked file under docs/ (no proof page for x/sync or x/mod: M-requirements 3).
# VERIFY ROUND 3 (trainM/tM-CHANGES.md section 11; COORD's rulings trainM/COORD-RULINGS-r3.md):
#  * HIGH: the two CONTROL converters (MR4c, MR6c) are built from TRAIN L seat shas and run against the UNION's testing
#    host. With the H2 host seat in the union that host frames every --json event line with ^V, and a converter that
#    predates H2 reads ZERO C# verdicts from such a stream: both controls went red because the old reader could not read,
#    not because M4 / M6 were absent. RULED: build the control converter WITH the union's comparer change. When the H2
#    seat is a row of the seat list, ctlconv applies that seat's OWN change to src/go2cs/testConversion.go (the stream
#    reader, nothing else: its diff against its merge-base with the base) to the archived control source before building
#    it. The control's own reader (cmpnames, by name from the control's comparison record) is unchanged: no gate was
#    loosened. A patch that does not apply, or a control converter that does not build, is NOT MEASURED and counted.
#  * The host seats (H1, H2) are read from the seat list once, near the top (the controls need H2's sha before MR4).
#  * With the H2 seat in the union a red on sumdb/tlog is stamped 'read against the network first' (RULED).
set -u
W=/h/go2cs-tmp-coord/tFL
WB='H:\go2cs-tmp-coord\tFL'
EXPECT_HEAD=${EXPECT_HEAD:?union head, 10 chars}
CONTROLS=${CONTROLS:-0}
IN_BATTERY=${IN_BATTERY:-0}
DEADLINE=${DEADLINE:-}           # verify round 2: 'YYYY-MM-DD HH:MM' local (absolute); unset = launch + 14 h
DL_EPOCH=${DL_EPOCH:-}           # the battery passes its own resolved deadline (epoch seconds) with IN_BATTERY=1
REALMOD=${REALMOD:-1}
# The package deadline handed to BOTH sides of every real-module package, stated explicitly. 2m is the converter's own
# default and the value P2's end acceptance was read at: F4's gate is that a first publish no longer dies at it (the
# publish alone gets max(-test-timeout, 30m)), and with host finding H1 unseated singleflight's child fan-out runs until
# this deadline. A slower box may need more for the RUN of a large package (modfile: 323 tests): TO BE SET BY COORD.
REALMOD_TEST_TIMEOUT=${REALMOD_TEST_TIMEOUT:-2m}
SD=$(cd "$(dirname "$0")" && pwd)
LOCK=/h/go2cs-tmp-coord/coord-scratch/tFL/.battery.lock
CTL=/h/go2cs-tmp-coord/coord-scratch/tFL/mod-ctl-build    # R7: control converters are built here from git archive
LOGDIR=$SD/mod-logs; FX=$SD/fx; OUT=$SD/out
SUM="$LOGDIR/SUMMARY.txt"
[ -e "$SUM" ] && { echo "ABORT: $SUM exists -- use a fresh per-run copy (critic 1 #4)"; exit 2; }
# Verify round 2 (floor 4): the refusal tFL-assemble.sh, tFL-fixup.sh and tFL-battery.sh carry. A standalone launch from
# hnd wrote $SD/mod-logs, $SD/fx and $SD/out INSIDE a git worktree, against this header's own "OUTSIDE every worktree".
case "$SD" in
  /h/go2cs-tmp-coord/tFL|/h/go2cs-tmp-coord/tFL/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
# Verify round 2 (RULED): an ABSOLUTE deadline, resolved once. The same-day HH:MM integer was already 'past' at launch
# for a next-morning value and silently void once the clock passed midnight.
T_LAUNCH=$(date +%s)
if [ "$IN_BATTERY" = 1 ] && [[ "$DL_EPOCH" =~ ^[0-9]+$ ]]; then :
elif [ -z "$DEADLINE" ]; then
  DL_EPOCH=$((T_LAUNCH + 14 * 3600)); DEADLINE="$(date -d "@$DL_EPOCH" '+%Y-%m-%d %H:%M') (default: launch + 14 h)"
else
  [[ "$DEADLINE" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}\ ([01][0-9]|2[0-3]):[0-5][0-9]$ ]] || { echo "ABORT: DEADLINE '$DEADLINE' is not 'YYYY-MM-DD HH:MM' (local time)"; exit 2; }
  DL_EPOCH=$(date -d "$DEADLINE" +%s 2>/dev/null) || { echo "ABORT: DEADLINE '$DEADLINE' is not a date"; exit 2; }
  [ "$DL_EPOCH" -gt "$T_LAUNCH" ] || { echo "ABORT: DEADLINE '$DEADLINE' is already in the past (now $(date '+%Y-%m-%d %H:%M'))"; exit 2; }
fi
[ -f "$SD/tFL-helpers.py" ] || { echo "ABORT: $SD/tFL-helpers.py missing (the by-name readers)"; exit 2; }
if [ "$IN_BATTERY" = 1 ]; then
  [ -d "$LOCK" ] || { echo "ABORT: IN_BATTERY=1 but no battery lock at $LOCK"; exit 2; }
else
  # Verify round 1 (floor 1 ACROSS trains; the battery and the fixup carry the same gate): the lock is TRAIN Q's own, so
  # tFL-helpers.py live also reads the other trains' locks, the converter and harness processes (listed, never killed)
  # and any run log still being written. LIVE_ACK=1 runs past a hit by COORD's explicit call.
  if [ "${LIVE_ACK:-0}" != 1 ]; then
    lv=$(python -B "$(cygpath -w "$SD/tFL-helpers.py")" live "$(cygpath -w "$LOCK")" 2>&1); lrc=$?
    if [ "$lrc" != 0 ]; then
      echo "ABORT (floor 1): another conversion, harness or battery looks alive on this box (listed, not killed; LIVE_ACK=1 runs past it by COORD's explicit call):"
      printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE' | head -n 12
      exit 2
    fi
  fi
  mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- a TRAIN FL battery or fixup is running (floors 1, 11)"; exit 2; }
  # verify round 2: the lock is KEPT when the end-of-run census finds a process still running from the out root
  trap 'if [ "${KEEP_LOCK:-0}" = 1 ]; then echo "lock KEPT: rmdir $LOCK after the listed PIDs are killed (mod-logs/orphans.txt)"; else rmdir "$LOCK" 2>/dev/null; fi' EXIT
fi
mkdir -p "$LOGDIR" "$FX" "$OUT"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
verdict(){ # label, ok(0|1), detail
  if [ "$2" = 0 ]; then stamp "  $1: PASS -- $3"; else stamp "  $1: FAIL -- $3"; FAILS=$((FAILS + 1)); fi
}
notmeasured(){ stamp "  $1: NOT MEASURED -- $2"; }   # R6: stated, never a pass and never a fail
hp(){ python -B "$(cygpath -w "$SD/tFL-helpers.py")" "$@"; }
FAILS=0
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
# ---- env: tK-reread.sh verbatim
ORIGPATH="$PATH"
# critic 1 #5: the GOROOT spelling is checked on its VALUE before the export (derived from $HOME, never written out)
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) stamp "ABORT: GOROOT spelling '$GR' has a forward slash (floor 6)"; exit 2 ;; esac
[[ "$GR" =~ ^[A-Za-z]:[\\].*[\\]sdk[\\]go1\.24\.13$ ]] || { stamp "ABORT: GOROOT spelling '$GR'"; exit 2; }
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || { stamp "ABORT: GOROOT VERSION is not go1.24.13"; exit 2; }
[ "$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')" = "$GR" ] || { stamp "ABORT: the toolchain's own GOROOT spelling != '$GR'"; exit 2; }
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { stamp "ABORT: go on PATH is $(go version)"; exit 2; }
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
# ---- module legs: hermetic (replace-only fixtures), no proxy, no workspace. GO2CS_MODULE_ROOT must NOT be ambient:
#      the converter sets it per host child (M4); an inherited value would mask a converter that stopped setting it.
export GOPROXY=off GOWORK=off GOSUMDB=off
unset GO2CS_MODULE_ROOT
cd "$W" || exit 2
h=$(git rev-parse --short=10 HEAD); [ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
[ "$(git status --porcelain | grep -vc "^??")" = 0 ] || { stamp "ABORT: tracked changes at start"; exit 2; }
# Q (Q16; g-module-safe-tag f6c182c23f, a Q row): a -recurse module conversion builds with the 'safe' tag on BOTH
# sides by default (the conversion, and under -tests its go test baseline; resolveBuildTags: the stdlib default set
# when -tests, then the module default set when -recurse; an explicit -tags replaces both). Every oracle THIS script
# runs beside a conversion is built with the same set, DERIVED from the converter's own source at HEAD and never
# typed. If the row is not in the union the module set is empty: the plain -recurse oracles (MOD_TAGS_PROG) are then
# what they were at P, and the -tests oracles are NOT (review round 1): they still pass the stdlib default set, purego
# and math_big_pure_go, where no oracle line of trainP/tP-modules-legs.sh passed any tag (0 '-tags' in that file).
#   MOD_TAGS_TESTS  a -tests -recurse conversion and its go test oracle (MR3, MR4, MR4v, MR6, XS, XM)
#   MOD_TAGS_PROG   a plain -recurse program conversion and its go run oracle (MR2, JWT, MR6p, MR6e)
# PREDICTED, not discovered: NO count of these legs moves with the tag. The six fixtures are written by this script and
# hold no build constraint; the two real modules name none of safe / purego / math_big_pure_go / appengine in any
# constraint (read at the derive from P's kept copies: x/sync 13 Go files, x/mod 39, 0 such lines), and G's seat read
# x/sync 28 and pflag 177 unchanged. A count that moves here is therefore NOT the tag's: read it as a red.
tagset(){ git show "HEAD:src/go2cs/commandLineOptions.go" 2>/dev/null | tr -d '\r' | sed -n "s/^var $1 = \[\]string{\(.*\)}\$/\1/p" | tr -d '\" '; }
STDTAGS=$(tagset defaultStdLibBuildTags); MODTAGS=$(tagset defaultModuleBuildTags)
MOD_TAGS_FALLBACK='purego,math_big_pure_go'   # the stdlib default set as TRAIN P's landed converter spells it: used, and stamped, only when the line above did not read
[ -n "$STDTAGS" ] || { STDTAGS=$MOD_TAGS_FALLBACK; stamp "  NOTE: defaultStdLibBuildTags did not read from HEAD:src/go2cs/commandLineOptions.go: the fallback literal [$MOD_TAGS_FALLBACK] is used (read the file: the declaration moved)"; }
MOD_TAGS_TESTS="$STDTAGS${MODTAGS:+,$MODTAGS}"; MOD_TAGS_PROG="$MODTAGS"
OT=${MOD_TAGS_TESTS:+-tags $MOD_TAGS_TESTS}; OP=${MOD_TAGS_PROG:+-tags $MOD_TAGS_PROG}
stamp "  module tags (Q16, derived at HEAD): a -tests -recurse run and its oracle [$MOD_TAGS_TESTS]; a plain -recurse run and its oracle [${MOD_TAGS_PROG:-none: g-module-safe-tag is not in this union}]"
pubsym(){ cat "$@" 2>/dev/null | grep -acE 'the published test host lacks [0-9]+ dependency symbol file'; }   # Q (Q15)
# Q (Q4): what these legs owe at the landing head, one line a package. REVIEW ROUND 1: the file ALWAYS EXISTS from
# here on (it was removed, and created only by an EXTERNAL or TIMING line, so 'no file' was both the clean state and
# what a wrong or stale run folder looks like, and tFL-land-final-reads.sh read 'none owed' from either). An EMPTY file
# is the clean state; an ABSENT one means these legs did not run in this folder, and the landing refuses it.
EXTERNALS=0; OWEDF="$LOGDIR/OWED-rereads.txt"; : > "$OWEDF" || { stamp "ABORT: cannot create $OWEDF (the owed re-reads file: its absence is never a clean state)"; exit 2; }
owedn(){ local n; n=$(grep -c . "$OWEDF" 2>/dev/null); echo "${n:-UNREAD}"; }   # grep -c prints 0 and exits 1 on an empty file: no '|| echo 0' (it would print a second line)
# R6: the module cache as go reports it under this environment (the battery does not override GOMODCACHE; `go env -w`
# puts it on H:). Presence is read by listing EXACT module directories only, never a walk, never a download.
GMC=$(go env GOMODCACHE | tr -d '\r'); GMCU=$(cygpath -u "$GMC")
# TRAIN L's seats these legs read (on master since L landed; the two control converters are built from two of them).
for s in 27f2199b30 c5e934c464 e9009f2945 6a079a9675 bbbb3ff022; do
  git merge-base --is-ancestor "$s" HEAD || { stamp "ABORT: $s not an ancestor of the union"; exit 2; }
done
# M: every row of the seat list is an ancestor (the real-module legs read P2's six seats and P1's repros). The list is
# the run copy's own; a standalone run without it says so.
SEATS=${SEATS:-$SD/tFL-seats-draft.txt}; nseat=0
if [ -f "$SEATS" ]; then
  while IFS='|' read -r ref sha desc; do
    nseat=$((nseat + 1)); git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $ref $sha is not an ancestor of the union"; exit 2; }
  done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
fi
stamp "PRE head=$h $(go version) dotnet=$(dotnet --version | tr -d "\r") free=$(freegb)G GOMODCACHE=$GMC DEADLINE=$DEADLINE in-battery=$IN_BATTERY REALMOD=$REALMOD REALMOD_TEST_TIMEOUT=$REALMOD_TEST_TIMEOUT (TRAIN L's module chain contained; N seat rows checked: $([ -f "$SEATS" ] && echo "$nseat" || echo 'NONE, no seat list in the run folder'))"
# Verify round 2 (RULED; R1: read from the seat list, never written here) and round 3 (read ONCE, here: the control
# converters of MR4c / MR6c need the H2 seat's sha before XS and XM do). H1 = a ref p2-*test-list*, or a row noted
# 'host seat H1'; H2 = a ref p2-*event-line*, or a row noted 'host seat H2' (the phrase since P: P11). What each one decides is stated at its use (ctlconv; XS; XM). A
# standalone run with no seat list beside it reads both as ABSENT.
BASE=${BASE:?set BASE to the landed master line of TRAIN Q (and the docs commits COORD landed after it) (tFL-battery.sh passes it)}   # O1: REQUIRED, never a literal
BASE=$(git rev-parse --verify -q "$BASE^{commit}") || { stamp "ABORT: BASE does not resolve to a commit"; exit 2; }
git merge-base --is-ancestor "$BASE" HEAD || { stamp "ABORT: BASE ${BASE:0:10} is not an ancestor of the union"; exit 2; }
H1SEAT=''; H2SEAT=''; H2SHA=''; H2MB=''; HSRC='the seat list'
if [ -f "$SEATS" ]; then
  # P (trainP/tP-CHANGES.md P11): the NOTES arm wants the phrase 'host seat H1' / 'host seat H2', no longer a bare H1 / H2.
  # C2's go-cmp classes are named H1 and H2 too (c2-anon-struct-named-conv, c2-named-basic-conv: COORD's notes of
  # 2026-10-04 09:19 and 10:17), and a bare token in such a row's notes read it as the HOST SEAT: its sha then went to
  # ctlconv as 'the H2 seat', whose testConversion.go diff is what the control converters are given. For a go-cmp row
  # that diff is empty (or another change altogether), ctlconv returns 1, and MR4c / MR6c cannot be built: a FAIL on a
  # correct union (READ from the code, not run). Both host seats LANDED with M and are read from the base below.
  H1SEAT=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | awk -F'|' '$1 ~ /^p2-.*test-list/ || $3 ~ /(^|[^A-Za-z0-9])host seat H1([^A-Za-z0-9]|$)/ { print $1 }' | head -n 1)
  H2SEAT=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | awk -F'|' '$1 ~ /^p2-.*event-line/ || $3 ~ /(^|[^A-Za-z0-9])host seat H2([^A-Za-z0-9]|$)/ { print $1 }' | head -n 1)
  [ -z "$H2SEAT" ] || H2SHA=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | awk -F'|' -v r="$H2SEAT" '$1 == r { print $2 }' | head -n 1)
  # a seat ROW: its own change is its diff against its merge-base with the base (M's rule)
  [ -z "$H2SHA" ] || H2MB=$(git merge-base "$BASE" "$H2SHA")
fi
# N (item N1): the host seats LANDED with TRAIN M (master's first-parent merges 'Merge claude/p2-host-test-list (...)'
# 89fded79dc and 'Merge claude/p2-host-event-line-start (...)' a92237f025, read 2026-10-03). A seat that is not a row of
# this list but whose merge is on the BASE's first-parent line is in the union too: read from there, by subject, never
# typed. A LANDED seat's own change is its diff against the merge-base of its merge's two parents (the merge-base with the
# base would be the seat itself: an empty diff, and the controls would build without the stream reader again).
lsub(){ git log --first-parent --merges --format='%H%x09%s' "$BASE" | awk -F'\t' -v re="$1" '$2 ~ re { print $1; exit }'; }
if [ -z "$H1SEAT" ]; then hm=$(lsub '^Merge claude/p2-[^ ]*test-list[^ ]* '); [ -z "$hm" ] || { H1SEAT="$(git log -1 --format=%s "$hm" | sed -nE 's#^Merge claude/([^ ]+) .*#\1#p') (landed)"; HSRC='the seat list, then master'; }; fi
if [ -z "$H2SEAT" ]; then
  hm=$(lsub '^Merge claude/p2-[^ ]*event-line[^ ]* ')
  if [ -n "$hm" ]; then
    H2SEAT="$(git log -1 --format=%s "$hm" | sed -nE 's#^Merge claude/([^ ]+) .*#\1#p') (landed)"; H2SHA=$(git rev-parse "$hm^2"); H2MB=$(git merge-base "$hm^1" "$hm^2"); HSRC='the seat list, then master'
  fi
fi
stamp "  XS/XM host seats read from $HSRC (R1): H1=${H1SEAT:-ABSENT} H2=${H2SEAT:-ABSENT}${H2SHA:+ (${H2SHA:0:10} against ${H2MB:0:10}: its stream-reader change rides the control converters of MR4c / MR6c)}"
cleanup_tree(){ rm -rf "$W/src/go2cs/bin" "$CTL"; }
leg(){ # name, logfile-suffix, command...   (tK-reread.sh verbatim + the R5 deadline)
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0 f c
  if [ "$(date +%s)" -ge "$DL_EPOCH" ]; then   # verify round 2: epoch seconds against the deadline resolved at launch
    cleanup_tree
    stamp "DEADLINE STOP before $name (DEADLINE=$DEADLINE): converter bin and control builds removed; failures so far=$FAILS; exit 9"
    exit 9
  fi
  t0=$(date +%s); LEG_T0=$t0
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
  LEG_RC=$rc
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
wf(){ mkdir -p "$(dirname "$1")"; printf '%b' "$2" > "$1"; }   # write a fixture file (LF, printf escapes)
hashtree(){ ( cd "$1" && find . -type f | LC_ALL=C sort | xargs sha256sum ); }
validated(){ # log -> "pkg N" per phase-B package (the -tests header, then that package's Validated line)
  tr -d '\r' < "$1" | awk '/^-tests [^ ]+ -> /{pkg=$2} /^Validated [0-9]+ tests against go test/{print pkg, $2}'
}
stampval(){ grep -ao 'go\.GoDefaultGodebug("[^"]*")' "$1" 2>/dev/null | sed 's/^go\.GoDefaultGodebug("//; s/")$//' | head -n 1; }

# Review round 1 (tFL-README.md MS19): p1-hashset-module makes the converter REQUIRE the hashset module (the path src/go2cs/go.mod names)
# v1.0.0, and GOPROXY=off is exported above, so on a COLD module cache CM fails and the converter build below aborts
# 'converter build rc=1' with no cause named (inside the battery leg C fetches it first; a standalone launch has no such
# leg). The converter's DIRECT requirements are read from the union's own src/go2cs/go.mod (never typed) and each must be
# in the H: module cache at its exact paths (the extracted module's go.mod, the download cache's .mod and .zip: what an
# offline build reads; R6's rule: no walk, no download). Module paths are case-encoded as the cache spells them (!x).
CONVREQ=$(tr -d '\r' < src/go2cs/go.mod | awk '/^require[ \t]*\(/ { b = 1; next } b && /^\)/ { b = 0; next } b && !/\/\/ indirect/ && NF >= 2 { print $1 "@" $2 } /^require[ \t]+[^ \t(]/ && !/\/\/ indirect/ { print $2 "@" $3 }')
crabs=''
for mv in $CONVREQ; do
  cm_=${mv%@*}; cv_=${mv##*@}; ce_=$(printf '%s' "$cm_" | sed 's/[A-Z]/!\L&/g')
  for p in "$GMCU/$ce_@$cv_/go.mod" "$GMCU/cache/download/$ce_/@v/$cv_.mod" "$GMCU/cache/download/$ce_/@v/$cv_.zip"; do [ -e "$p" ] || crabs="$crabs ${p#$GMCU/}"; done
done
[ -n "$CONVREQ" ] || { stamp "ABORT: no direct requirement read from src/go2cs/go.mod (the reader is not reading)"; exit 2; }
[ -z "$crabs" ] || { stamp "ABORT: the converter's direct module requirements are not all in $GMC, and GOPROXY=off here (absent:$crabs; required: $(echo $CONVREQ)): warm the cache at this head with the proxy ON, '(cd src/go2cs && go mod download)', then relaunch (tFL-README.md MS19)"; exit 3; }
stamp "  converter module requirements in the cache (derived from src/go2cs/go.mod; GOPROXY=off): $(echo $CONVREQ)"

# =========================================================================================== CM -- converter, BY NAME
# Leg C runs `go test ./...` WITHOUT -v: a module test that silently SKIPS (testing.Short) or is absent reads green.
# This leg reads every R-chain test by name: each must print '--- PASS', none '--- SKIP' / '--- FAIL'. (M's own tests
# of the same converter file, TestPublishTimeoutHasAFloor and F8's closure test, are read by name in the battery's CB.)
CM_TESTS='TestModuleCacheEndToEndLockAndClash TestModuleCachePreflightNamesMissingDependency TestModuleCacheOutsideGopathResolvesAsCacheDependency TestModulesLockRoundTripAndClash TestTestsRecurseConvertsAModuleAgainstItsTests TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot TestTestClosureDiscoveryAddsNoOrderEdges TestTestHostCarriesTheLogicalModulePathOnly TestTestHostModuleEnvironmentIsForModulePackagesOnly TestWriteTestHostUsesCSharpClassOverride TestFixtureDirectoriesStagePackageShape TestDefaultGODEBUGMatchesTheToolchain TestRecurseMainProgramCarriesItsDefaultGODEBUG TestModuleGodebugReadsTheGodebugBlockStrictly'
CM_SUBS='TestDefaultGODEBUGMatchesTheToolchain/go120 TestDefaultGODEBUGMatchesTheToolchain/go123 TestDefaultGODEBUGMatchesTheToolchain/go124 TestDefaultGODEBUGMatchesTheToolchain/block TestDefaultGODEBUGMatchesTheToolchain/directive'
CM_RE="^($(echo $CM_TESTS | tr ' ' '|'))\$"
leg CM conv-modules-named bash -c "cd src/go2cs && go test -count=1 -timeout 30m -v -run '$CM_RE' ."
cmrc=$LEG_RC; miss=''
for t in $CM_TESTS $CM_SUBS; do grep -aqE -- "--- PASS: $t \(" "$LOGDIR/conv-modules-named.log" || miss="$miss $t"; done
sk=$(grep -acE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-modules-named.log")
[ "$cmrc" = 0 ] && [ -z "$miss" ] && [ "$sk" = 0 ]; verdict CM $? "rc=$cmrc not-PASS=[${miss# }] skip/fail-lines=$sk (19 names incl. 5 subtests)"

# =========================================================================================== converter binary
export MSYS_NO_PATHCONV=1
[ "$(git rev-parse --short=10 HEAD 2>/dev/null)" = "$EXPECT_HEAD" ] || { stamp "ABORT: git does not read the worktree with MSYS_NO_PATHCONV=1 exported: the tracked-changes readings at END would be vacuous (Q5)"; exit 2; }
EXE="$W/src/go2cs/bin/go2cs.exe"
rm -f "$EXE"
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1; brc=$?   # critic 1 #6: the rc is read
[ "$brc" = 0 ] && [ -x "$EXE" ] || { stamp "ABORT: converter build rc=$brc, exe present=$([ -x "$EXE" ] && echo yes || echo no)"; exit 3; }
ctlconv(){ # sha, name -- R7: a control converter from `git archive` into $CTL (coord-scratch/tFL, never a worktree)
  local d="$CTL/$2"
  rm -rf "$d"; mkdir -p "$d"
  git archive "$1" src/go2cs | tar -x -C "$d" || return 1
  # Verify round 3 (HIGH, RULED: "build the control converter WITH the union's comparer change"). The control runs
  # against the UNION's testing host (-go2cspath is the union's src). With the H2 host seat in the union that host
  # frames every --json event line with ^V (TestReporter.FramedJson), and only a converter with H2's testStreamLines
  # reads such a stream: a converter cut before H2 does json.Unmarshal on the raw line and reads ZERO C# verdicts, so
  # the control went red for the wrong reason (cs=None by name, not cs=fail). The H2 seat's OWN change to
  # src/go2cs/testConversion.go (its diff against its merge-base with the base: the stream reader and its call sites,
  # +31 / -4 lines at 932778788f) is applied to the archived source before the build, so the ONE axis of the control
  # stays what it was at TRAIN L: the converter without M4 (or M6). Nothing is written to stdout here (the caller reads
  # the exe path from it). Read-only probe 2026-10-02: the patch applies to both control shas' archived file.
  if [ -n "$H2SHA" ]; then
    git diff "${H2MB:-$(git merge-base "$BASE" "$H2SHA")}" "$H2SHA" -- src/go2cs/testConversion.go > "$LOGDIR/ctl-reader-$2.patch" 2> "$LOGDIR/ctl-reader-$2.log" || return 1
    [ -s "$LOGDIR/ctl-reader-$2.patch" ] || { echo "the H2 seat $H2SEAT $H2SHA changes no line of src/go2cs/testConversion.go: nothing to give the control converter" >> "$LOGDIR/ctl-reader-$2.log"; return 1; }
    ( cd "$d" && git apply - ) < "$LOGDIR/ctl-reader-$2.patch" >> "$LOGDIR/ctl-reader-$2.log" 2>&1 || return 1
  fi
  ( cd "$d/src/go2cs" && go build -o "go2cs-$2.exe" . ) > "$LOGDIR/conv-build-$2.log" 2>&1 || return 1
  [ -x "$d/src/go2cs/go2cs-$2.exe" ] && echo "$d/src/go2cs/go2cs-$2.exe"
}
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
trec(){ # name, suffix, moduleDir(unix), outRoot(unix) -- the -tests -recurse driver, action all
  rm -rf "$4"
  leg "$1" "$2" "$EXE" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$3")" "$(cygpath -w "$4")"
}
present(){ # unix paths... -- R6: every exact path exists (no walk, no download)
  local p; for p in "$@"; do [ -e "$p" ] || { echo "absent: ${p#$GMCU/}"; return 1; }; done; return 0
}

# =========================================================================================== MR2 -- M2 real module cache
# Critic 0 #9: M2's claim 'real-cache reading runewidth->uniseg with exact go.sum hashes' has no other leg (MR3 is
# replace-only, CM hermetic). R6: runs ONLY when both modules (extracted dirs + the download cache's .mod/.zip that an
# offline resolve reads) are in the H: module cache; otherwise 'NOT MEASURED: module absent'. GOPROXY=off throughout.
# EXPECT: the oracle (go mod tidy offline + go run) prints 5; -recurse rc=0; go2cs.modules.lock holds EXACTLY
# runewidth v0.0.13 and uniseg v0.2.0, each hash == the fixture's go.sum h1 line; the fixture unchanged by the convert.
M2NEED="$GMCU/github.com/mattn/go-runewidth@v0.0.13/go.mod $GMCU/github.com/rivo/uniseg@v0.2.0/go.mod $GMCU/cache/download/github.com/mattn/go-runewidth/@v/v0.0.13.mod $GMCU/cache/download/github.com/mattn/go-runewidth/@v/v0.0.13.zip $GMCU/cache/download/github.com/rivo/uniseg/@v/v0.2.0.mod $GMCU/cache/download/github.com/rivo/uniseg/@v/v0.2.0.zip"
if m2why=$(present $M2NEED); then
  F2=$FX/m2/rw; rm -rf "$FX/m2"
  wf "$F2/go.mod" 'module example.test/rw\n\ngo 1.23\n\nrequire github.com/mattn/go-runewidth v0.0.13\n'
  wf "$F2/main.go" 'package main\n\nimport (\n\t"fmt"\n\n\t"github.com/mattn/go-runewidth"\n)\n\nfunc main() {\n\tfmt.Println(runewidth.StringWidth("go2cs"))\n}\n'
  leg MR2-oracle go-mod-m2 bash -c "cd '$F2' && go mod tidy && go run $OP ."
  o2rc=$LEG_RC; o2=$(tr -d '\r' < "$LOGDIR/go-mod-m2.log" | tail -n 1)
  [ "$o2rc" = 0 ] && [ "$o2" = 5 ]; verdict MR2-oracle $? "rc=$o2rc output=[$o2] (go mod tidy offline + go run; EXPECT 5)"
  B2=$(hashtree "$F2")
  rm -rf "$OUT/m2"
  leg MR2 recurse-m2 "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F2")" "$(cygpath -w "$OUT/m2")"
  r2=$LEG_RC
  hp lockcheck "$(cygpath -w "$OUT/m2/go2cs.modules.lock")" "$(cygpath -w "$F2/go.sum")" github.com/mattn/go-runewidth@v0.0.13 github.com/rivo/uniseg@v0.2.0 > "$LOGDIR/m2-lockcheck.txt" 2>&1; lk=$?
  [ "$r2" = 0 ] && [ "$lk" = 0 ] && [ "$(hashtree "$F2")" = "$B2" ]
  verdict MR2 $? "rc=$r2 $(grep -a '^LOCKCHECK \(OK\|FAILED\|lock absent\|module set\)' "$LOGDIR/m2-lockcheck.txt" | tr -d '\r' | tr '\n' ' ' | cut -c1-200) module-unchanged=$([ "$(hashtree "$F2")" = "$B2" ] && echo yes || echo NO)"
else
  notmeasured MR2 "module absent ($m2why) in $GMC (R6: never downloaded)"
fi

# =========================================================================================== JWT -- i9-crosspkg's jwt class
# Critic 0 #10: i9-crosspkg claims 'the jwt root now compiles and packs' (the jwt CS1929 class: a value method promoted
# through ANOTHER package's embedded struct, jwt's NumericDate embedding time.Time); MR3's fixture has no such call.
# R6: runs ONLY when github.com/golang-jwt/jwt/v5@v5.3.1 is in the H: module cache. A consumer module requiring it is
# converted with -recurse (the dependency lands under <out>/pkg/<importPath>, M2's layout: INFERRED path, a missing
# project is a FAIL), then the jwt ROOT project is built alone. EXPECT rc=0 and 0 CS1929 (GATED). The consumer's own run
# (which calls the promoted d.Unix()) is compared with `go run` as INFORMATION, not gated (the seat claims compile+pack).
JWNEED="$GMCU/github.com/golang-jwt/jwt/v5@v5.3.1/go.mod $GMCU/cache/download/github.com/golang-jwt/jwt/v5/@v/v5.3.1.mod $GMCU/cache/download/github.com/golang-jwt/jwt/v5/@v/v5.3.1.zip"
if jwwhy=$(present $JWNEED); then
  FJ=$FX/jwt/use; rm -rf "$FX/jwt"
  wf "$FJ/go.mod" 'module example.test/jwtuse\n\ngo 1.23\n\nrequire github.com/golang-jwt/jwt/v5 v5.3.1\n'
  wf "$FJ/main.go" 'package main\n\nimport (\n\t"fmt"\n\t"time"\n\n\t"github.com/golang-jwt/jwt/v5"\n)\n\nfunc main() {\n\td := jwt.NewNumericDate(time.Unix(1700000000, 0))\n\tfmt.Println(d.Unix())\n}\n'
  leg JWT-oracle go-mod-jwt bash -c "cd '$FJ' && go mod tidy && go run $OP ."
  ojrc=$LEG_RC; oj=$(tr -d '\r' < "$LOGDIR/go-mod-jwt.log" | tail -n 1)
  [ "$ojrc" = 0 ] && [ "$oj" = 1700000000 ]; verdict JWT-oracle $? "rc=$ojrc output=[$oj] (EXPECT 1700000000)"
  rm -rf "$OUT/jwt"
  leg JWT-convert recurse-jwt "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$FJ")" "$(cygpath -w "$OUT/jwt")"
  rj=$LEG_RC
  JP=$(ls "$OUT"/jwt/pkg/github.com/golang-jwt/jwt/v5/*.csproj 2>/dev/null | grep -v '\.tests\.csproj$' | head -n 1)
  if [ -n "$JP" ]; then
    leg JWT-build build-jwt-root dotnet build "$(cygpath -w "$JP")" -c Release -p:UseSharedCompilation=false
    bj=$LEG_RC; c1929=$(grep -aoE 'error CS1929' "$LOGDIR/build-jwt-root.log" | wc -l); cerr=$(grep -aoE 'error CS[0-9]+' "$LOGDIR/build-jwt-root.log" | LC_ALL=C sort | uniq -c | sort -rn | head -n 4 | tr -s ' ' | tr '\n' ' ')
    [ "$rj" = 0 ] && [ "$bj" = 0 ] && [ "$c1929" = 0 ]; verdict JWT $? "convert rc=$rj root-project=${JP#$OUT/} build rc=$bj CS1929=$c1929 (EXPECT 0) top errors=[${cerr% }]"
    PJ=$(ls "$OUT"/jwt/src/example.test/jwtuse/*.csproj 2>/dev/null | head -n 1)
    if [ -n "$PJ" ]; then
      leg JWT-run run-jwtuse bash -c "dotnet build '$(cygpath -w "$PJ")' -c Release -p:UseSharedCompilation=false && dotnet run --project '$(cygpath -w "$PJ")' -c Release --no-build"
      cj=$(tr -d '\r' < "$LOGDIR/run-jwtuse.log" | tail -n 1)
      stamp "  JWTrun (INFORMATION, not gated): rc=$LEG_RC converted=[$cj] go-run=[$oj] $([ "$cj" = "$oj" ] && echo same || echo DIFFERENT)"
    fi
  else
    verdict JWT 1 "convert rc=$rj: no jwt root project under $OUT/jwt/pkg/github.com/golang-jwt/jwt/v5 (NOT MEASURED counts as a failure: the layout is INFERRED, read recurse-jwt.log)"
  fi
else
  notmeasured JWT "module absent ($jwwhy) in $GMC (R6: never downloaded)"
fi

# =========================================================================================== MR3 -- M3 jwt-shaped
# The fixture is moduleTestsDriver_integration_test.go's writeJwtLikeModule (go 1.23), byte for byte in content.
# R's reading at c5e934c464 (00:48Z): exit 0; 5 packages in phase A (NOT 7: no synthesized test main); jwtlike and
# jwtlike/request each 'Validated 1 tests against go test'; MODULE.md 'Total: 2 matched · 0 disclosed' across 2.
# AT THE UNION (new vs R's reading): M6 is in, so a go 1.23 module's hosts carry a non-empty DefaultGODEBUG stamp
# (the Changed:24 settings), which must equal the toolchain's own answer for the test main.
F3=$FX/m3; rm -rf "$F3"
wf "$F3/dep/go.mod" 'module example.test/dep\n\ngo 1.23\n'
wf "$F3/dep/prefix.go" 'package dep\n\nfunc Prefix() string {\n\treturn "signed:"\n}\n'
wf "$F3/dep/prefix_test.go" 'package dep\n\nimport "testing"\n\nfunc TestPrefix(t *testing.T) {\n\tif Prefix() == "" {\n\t\tt.Fatal("empty")\n\t}\n}\n'
wf "$F3/testkit/go.mod" 'module example.test/testkit\n\ngo 1.23\n'
wf "$F3/testkit/same.go" 'package testkit\n\nfunc Same(a, b string) bool {\n\treturn a == b\n}\n'
wf "$F3/jwtlike/go.mod" 'module example.test/jwtlike\n\ngo 1.23\n\nrequire (\n\texample.test/dep v0.0.0\n\texample.test/testkit v0.0.0\n)\n\nreplace example.test/dep => ../dep\n\nreplace example.test/testkit => ../testkit\n'
wf "$F3/jwtlike/token.go" 'package jwtlike\n\nimport "example.test/dep"\n\nfunc Sign(claim string) string {\n\treturn dep.Prefix() + claim\n}\n'
wf "$F3/jwtlike/token_test.go" 'package jwtlike\n\nimport (\n\t"testing"\n\n\t"example.test/jwtlike/testutil"\n)\n\nfunc TestSign(t *testing.T) {\n\ttestutil.Expect(t, Sign("a"), "signed:a")\n}\n'
wf "$F3/jwtlike/request/request.go" 'package request\n\nimport "example.test/jwtlike"\n\nfunc FromHeader(h string) string {\n\treturn jwtlike.Sign(h)\n}\n'
wf "$F3/jwtlike/request/request_test.go" 'package request_test\n\nimport (\n\t"testing"\n\n\t"example.test/jwtlike"\n\t"example.test/jwtlike/request"\n\t"example.test/jwtlike/testutil"\n\t"example.test/testkit"\n)\n\nfunc TestFromHeader(t *testing.T) {\n\ttestutil.Expect(t, request.FromHeader("h"), jwtlike.Sign("h"))\n\tif !testkit.Same("x", "x") {\n\t\tt.Fatal("testkit")\n\t}\n}\n'
wf "$F3/jwtlike/testutil/expect.go" 'package testutil\n\nimport "testing"\n\nfunc Expect(t *testing.T, got, want string) {\n\tt.Helper()\n\tif got != want {\n\t\tt.Fatalf("got %q, want %q", got, want)\n\t}\n}\n'
leg MR3-oracle go-test-m3 bash -c "cd '$F3/jwtlike' && go test $OT -count=1 -timeout 10m ./..."
verdict MR3-oracle "$LEG_RC" "go test ./... over the fixture (the Go side must pass first)"
B3=$(hashtree "$F3")
trec MR3 tests-recurse-m3 "$F3/jwtlike" "$OUT/m3"; r3=$LEG_RC
L3=$LOGDIR/tests-recurse-m3.log; O3=$OUT/m3; S3=$O3/src/example.test/jwtlike
v3=$(validated "$L3" | LC_ALL=C sort | tr '\n' ';')
nconv=$(tr -d '\r' < "$L3" | grep -aoE 'Converting [0-9]+ packages in dependency order' | head -n 1)
nprod=$(find "$O3" -name '*.csproj' ! -name '*.tests.csproj' | wc -l)
abs=$(grep -rlE 'Include="([A-Za-z]:[\\/]|/)' --include='*.csproj' "$O3" | wc -l)
mains=$( { [ -e "$S3.test" ] && echo "$S3.test"; [ -e "$S3/request.test" ] && echo request.test; } | wc -l)
tk=$(ls "$O3"/pkg/example.test/testkit/*.cs 2>/dev/null | wc -l); dept=$(ls "$O3"/pkg/example.test/dep/*.tests.csproj 2>/dev/null | wc -l)
mod=$(tr -d '\r' < "$O3/validation/example.test/jwtlike/MODULE.md" 2>/dev/null | grep -a 'Total:')
lock=$([ -e "$O3/go2cs.modules.lock" ] && echo present || echo absent)
# Q (Q16): the tags both sides of this reading were built with, as the converter's own MODULE.md states them.
mtag=$(tr -d '\r' < "$O3/validation/example.test/jwtlike/MODULE.md" 2>/dev/null | grep -a -o 'Both sides of every reading were built with .*' | head -n 1)
want3=$(cd "$F3/jwtlike" && go list $OT -test -f '{{.ImportPath}}|{{.DefaultGODEBUG}}' . | tr -d '\r' | grep -a '^example.test/jwtlike.test|' | cut -d'|' -f2)
got3=$(stampval "$S3/go2cs_test_host.cs")
[ "$r3" = 0 ] && [ "$v3" = 'example.test/jwtlike 1;example.test/jwtlike/request 1;' ] && [ "$nconv" = 'Converting 5 packages in dependency order' ] \
  && [ "$abs" = 0 ] && [ "$mains" = 0 ] && [ "$tk" -ge 1 ] && [ "$dept" = 0 ] \
  && echo "$mod" | grep -q 'Total: 2 matched · 0 disclosed\*\* across 2 package(s)' && [ "$lock" = absent ] \
  && [ -n "$want3" ] && [ "$got3" = "$want3" ] && [ "$(hashtree "$F3")" = "$B3" ]
verdict MR3 $? "rc=$r3 validated=[$v3] [$nconv] prod-csproj=$nprod(info, R saw 5) abs-refs=$abs test-mains=$mains testkit-cs=$tk dep-tests-csproj=$dept MODULE=[$mod] lock=$lock stamp==go-list-test:[$([ "$got3" = "$want3" ] && echo yes || echo "NO got=${got3:0:60} want=${want3:0:60}")] module-unchanged=$([ "$(hashtree "$F3")" = "$B3" ] && echo yes || echo NO)"
if [ -n "$MODTAGS" ]; then
  [ "$mtag" = "Both sides of every reading were built with \`-tags $MOD_TAGS_TESTS\`." ]; verdict MR3-tags $? "MODULE.md states [${mtag:-no build-tags sentence}] (EXPECT the set the oracles of this script were built with: -tags $MOD_TAGS_TESTS)"
else
  notmeasured MR3-tags "g-module-safe-tag is not in this union (no defaultModuleBuildTags at HEAD): MODULE.md states no tag set to compare"
fi

# =========================================================================================== MR4 -- M4 module ancestry
# The shape of R's RED/GREEN fixture (e9009f2945's message; module path as ModuleAncestryTests' example.test/keys):
# a root test reading test/key.pem, a sub-package test reading ../test/key.pem and ../go.mod. SIX module files, as
# R hashed. EXPECT exit 0, keys 'Validated 1 tests', keys/parse 'Validated 2 tests', the module byte-identical.
F4=$FX/m4/keys; rm -rf "$FX/m4"
wf "$F4/go.mod" 'module example.test/keys\n\ngo 1.23\n'
wf "$F4/keys.go" 'package keys\n\nfunc Name() string {\n\treturn "keys"\n}\n'
wf "$F4/keys_test.go" 'package keys\n\nimport (\n\t"os"\n\t"testing"\n)\n\nfunc TestReadsTheModuleFixture(t *testing.T) {\n\tdata, err := os.ReadFile("test/key.pem")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "KEY-MATERIAL\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
wf "$F4/test/key.pem" 'KEY-MATERIAL\n'
wf "$F4/parse/parse.go" 'package parse\n\nfunc Name() string {\n\treturn "parse"\n}\n'
wf "$F4/parse/parse_test.go" 'package parse\n\nimport (\n\t"os"\n\t"strings"\n\t"testing"\n)\n\nfunc TestReadsTheParentFixture(t *testing.T) {\n\tdata, err := os.ReadFile("../test/key.pem")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "KEY-MATERIAL\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n\nfunc TestReadsTheModuleGoMod(t *testing.T) {\n\tdata, err := os.ReadFile("../go.mod")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif !strings.Contains(string(data), "module example.test/keys") {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
leg MR4-oracle go-test-m4 bash -c "cd '$F4' && go test $OT -count=1 -timeout 10m ./..."
verdict MR4-oracle "$LEG_RC" "go test ./... over the fixture (3 tests)"
B4=$(hashtree "$F4"); n4=$(echo "$B4" | wc -l)
trec MR4 tests-recurse-m4 "$F4" "$OUT/m4"; r4=$LEG_RC
v4=$(validated "$LOGDIR/tests-recurse-m4.log" | LC_ALL=C sort | tr '\n' ';')
mp=$(grep -ac '}, "example.test/keys");' "$OUT/m4/src/example.test/keys/parse/go2cs_test_host.cs" 2>/dev/null)
mach=$(grep -rlE '"([A-Za-z]:[\\/]|/)[^"]*keys' --include='go2cs_test_host.cs' "$OUT/m4" | wc -l)
[ "$r4" = 0 ] && [ "$v4" = 'example.test/keys 1;example.test/keys/parse 2;' ] && [ "$n4" = 6 ] && [ "$(hashtree "$F4")" = "$B4" ] && [ "${mp:-0}" -ge 1 ] && [ "$mach" = 0 ]
verdict MR4 $? "rc=$r4 validated=[$v4] module-files=$n4 module-unchanged=$([ "$(hashtree "$F4")" = "$B4" ] && echo yes || echo NO) host-module-path-arg=${mp:-0} machine-path-hosts=$mach"
if [ "$CONTROLS" = 1 ]; then
  # MR4c, the discriminating control (floor 13): the SAME fixture through M3's converter (c5e934c464, before M4),
  # built from `git archive` into the run folder (no worktree, no tree write). The host still compiles against the
  # union's golib testing (TryStageModule present), but the M3 converter neither emits the module path nor sets
  # GO2CS_MODULE_ROOT, so staging declines. EXPECT a non-zero rc and C# failing on the three ENOENT reads
  # ('open test/key.pem', 'open ../test/key.pem', 'open ../go.mod') -- R's RED. A pass here means MR4 proves nothing.
  # DRAFT 2: built by ctlconv under $CTL (R7's rule applied to both controls); the red is read BY NAME from the control's
  # own comparison records (critic 0 #17: '>= 3 matching lines' could be met by one echoed error).
  # Verify round 3: with the H2 seat in the union the control converter also carries that seat's stream reader (ctlconv),
  # or it could not read the union host's framed events at all and 'cs=fail by name' could never be met.
  if X3=$(ctlconv c5e934c464 m3); then
    rm -rf "$OUT/m4c"
    leg MR4c tests-recurse-m4-control "$X3" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F4")" "$(cygpath -w "$OUT/m4c")"
    c4=$LEG_RC; e4=$(grep -acE 'open (\.\./)?(test/key\.pem|go\.mod)' "$LOGDIR/tests-recurse-m4-control.log")
    hp cmpnames "$(cygpath -w "$OUT/m4c/src/example.test/keys/go2cs_test_comparison.json")" pass fail TestReadsTheModuleFixture > "$LOGDIR/m4c-names.txt" 2>&1; n4a=$?
    hp cmpnames "$(cygpath -w "$OUT/m4c/src/example.test/keys/parse/go2cs_test_comparison.json")" pass fail TestReadsTheParentFixture TestReadsTheModuleGoMod >> "$LOGDIR/m4c-names.txt" 2>&1; n4b=$?
    [ "$c4" != 0 ] && [ "$n4a" = 0 ] && [ "$n4b" = 0 ]
    verdict "MR4c (EXPECT rc!=0 + the 3 tests go=pass cs=fail by name)" $? "rc=$c4 by-name: keys=$([ "$n4a" = 0 ] && echo ok || echo "MISS($n4a)") parse=$([ "$n4b" = 0 ] && echo ok || echo "MISS($n4b)") enoent-lines=$e4 (information) (m4c-names.txt) control converter=c5e934c464$([ -n "$H2SHA" ] && echo " + $H2SEAT's stream reader")"
  else
    stamp "  MR4c: NOT MEASURED -- the M3 control converter was not built (the H2 reader patch: ctl-reader-m3.log; the build: conv-build-m3.log)"; FAILS=$((FAILS + 1))
  fi
fi

# MR4v (critic 0 #15) -- M4's 'the copy includes vendor/': a SEPARATE module (so a vendoring surprise cannot touch MR4's
# reading) whose root test reads vendor/keep.txt. go >= 1.14 with a vendor/ directory defaults to -mod=vendor; with no
# requirements the (absent) modules.txt is consistent (INFERRED: the oracle leg is the proof). EXPECT the oracle PASS,
# exit 0, 'Validated 1 tests', the module byte-identical. The in-module LINK skip has no leg here: a GolibTests arm is
# requested from R (README open question).
F4v=$FX/m4v/vend; rm -rf "$FX/m4v"
wf "$F4v/go.mod" 'module example.test/vend\n\ngo 1.23\n'
wf "$F4v/vend.go" 'package vend\n\nfunc Name() string {\n\treturn "vend"\n}\n'
wf "$F4v/vend_test.go" 'package vend\n\nimport (\n\t"os"\n\t"testing"\n)\n\nfunc TestReadsTheVendoredFile(t *testing.T) {\n\tdata, err := os.ReadFile("vendor/keep.txt")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "VENDORED\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
wf "$F4v/vendor/keep.txt" 'VENDORED\n'
leg MR4v-oracle go-test-m4v bash -c "cd '$F4v' && go test $OT -count=1 -timeout 10m ./..."
verdict MR4v-oracle "$LEG_RC" "go test ./... over the vendor fixture (1 test)"
B4v=$(hashtree "$F4v")
trec MR4v tests-recurse-m4v "$F4v" "$OUT/m4v"; r4v=$LEG_RC
v4v=$(validated "$LOGDIR/tests-recurse-m4v.log" | LC_ALL=C sort | tr '\n' ';')
[ "$r4v" = 0 ] && [ "$v4v" = 'example.test/vend 1;' ] && [ "$(hashtree "$F4v")" = "$B4v" ]
verdict MR4v $? "rc=$r4v validated=[$v4v] module-unchanged=$([ "$(hashtree "$F4v")" = "$B4v" ] && echo yes || echo NO)"

# =========================================================================================== MR6 -- M6 DefaultGODEBUG
# R's RED/GREEN shape (6a079a9675's message), fixture INFERRED to that description: a go 1.20 module whose root
# package has TestPanicNilKeepsGo120Behavior (panicnil, a runtime dbgvar) and TestRandSeedIsHonored (math/rand's
# randseednop through internal/godebug -- the ONLY end-to-end proof of internal/godebug's new default layer and its
# sentinel cache key: no GolibTests class covers them), and an 'override' package whose test file carries
# //go:debug panicnil=0. EXPECT exit 0; root 'Validated 2 tests', override 'Validated 1 tests'; each host's stamp ==
# `go list -test` DefaultGODEBUG of its test main; root carries panicnil=1 and randseednop=0, override panicnil=0.
F6=$FX/m6/gd120; rm -rf "$FX/m6"
wf "$F6/go.mod" 'module example.test/gd120\n\ngo 1.20\n'
wf "$F6/gd.go" 'package gd\n\nfunc Name() string {\n\treturn "gd"\n}\n'
wf "$F6/gd_test.go" 'package gd\n\nimport (\n\t"math/rand"\n\t"testing"\n)\n\nfunc recoverPanicNil() (r any) {\n\tdefer func() { r = recover() }()\n\tpanic(nil)\n}\n\nfunc TestPanicNilKeepsGo120Behavior(t *testing.T) {\n\tif r := recoverPanicNil(); r != nil {\n\t\tt.Fatalf("recover() = %T %v, want nil under go 1.20 (panicnil=1)", r, r)\n\t}\n}\n\nfunc TestRandSeedIsHonored(t *testing.T) {\n\trand.Seed(42)\n\ta := rand.Int63()\n\trand.Seed(42)\n\tb := rand.Int63()\n\tif a != b {\n\t\tt.Fatalf("rand.Seed ignored (%d != %d), want honored under go 1.20 (randseednop=0)", a, b)\n\t}\n}\n'
wf "$F6/override/override.go" 'package override\n\nfunc Name() string {\n\treturn "override"\n}\n'
wf "$F6/override/override_test.go" '//go:debug panicnil=0\n\npackage override\n\nimport (\n\t"runtime"\n\t"testing"\n)\n\nfunc recoverPanicNil() (r any) {\n\tdefer func() { r = recover() }()\n\tpanic(nil)\n}\n\nfunc TestPanicNilDirectiveRestoresPanicNilError(t *testing.T) {\n\tif _, ok := recoverPanicNil().(*runtime.PanicNilError); !ok {\n\t\tt.Fatal("want *runtime.PanicNilError under //go:debug panicnil=0")\n\t}\n}\n'
leg MR6-oracle go-test-m6 bash -c "cd '$F6' && go test $OT -count=1 -timeout 10m ./..."
verdict MR6-oracle "$LEG_RC" "go test ./... over the fixture (3 tests)"
B6=$(hashtree "$F6")
trec MR6 tests-recurse-m6 "$F6" "$OUT/m6"; r6=$LEG_RC
v6=$(validated "$LOGDIR/tests-recurse-m6.log" | LC_ALL=C sort | tr '\n' ';')
GL6=$(cd "$F6" && go list $OT -test -f '{{.ImportPath}}|{{.DefaultGODEBUG}}' ./... | tr -d '\r')
w6r=$(echo "$GL6" | grep -a '^example.test/gd120.test|' | cut -d'|' -f2); w6o=$(echo "$GL6" | grep -a '^example.test/gd120/override.test|' | cut -d'|' -f2)
g6r=$(stampval "$OUT/m6/src/example.test/gd120/go2cs_test_host.cs"); g6o=$(stampval "$OUT/m6/src/example.test/gd120/override/go2cs_test_host.cs")
[ "$r6" = 0 ] && [ "$v6" = 'example.test/gd120 2;example.test/gd120/override 1;' ] && [ -n "$w6r" ] && [ "$g6r" = "$w6r" ] && [ "$g6o" = "$w6o" ] \
  && echo ",$g6r," | grep -q ',panicnil=1,' && echo ",$g6r," | grep -q ',randseednop=0,' && echo ",$g6o," | grep -q ',panicnil=0,' && [ "$(hashtree "$F6")" = "$B6" ]
verdict MR6 $? "rc=$r6 validated=[$v6] root-stamp==go-list:[$([ "$g6r" = "$w6r" ] && echo yes || echo NO)] override-stamp==go-list:[$([ "$g6o" = "$w6o" ] && echo yes || echo NO)] root panicnil/randseednop=[$(echo ",$g6r," | grep -o ',panicnil=[01],' | tr -d ,)/$(echo ",$g6r," | grep -o ',randseednop=[01],' | tr -d ,)] override=[$(echo ",$g6o," | grep -o ',panicnil=[01],' | tr -d ,)]"
if [ "$CONTROLS" = 1 ]; then
  # MR6c (critic 0 #8, R7), the discriminating control for the -tests-host route, mirroring MR4c: the SAME gd120 fixture
  # through M4's converter (e9009f2945, before M6), built from `git archive` under $CTL (never a worktree), into its OWN
  # output root. The host compiles against the union's golib, but the M4 converter emits no DefaultGODEBUG stamp, so the
  # C# defaults are go1.24's. EXPECT rc!=0 and BOTH root tests go=pass cs=fail BY NAME in that root's comparison record.
  # Verify round 3: as MR4c, the control converter carries the H2 seat's stream reader when that seat is in the union.
  if X4=$(ctlconv e9009f2945 m4); then
    rm -rf "$OUT/m6c"
    leg MR6c tests-recurse-m6-control "$X4" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F6")" "$(cygpath -w "$OUT/m6c")"
    c6c=$LEG_RC
    hp cmpnames "$(cygpath -w "$OUT/m6c/src/example.test/gd120/go2cs_test_comparison.json")" pass fail TestPanicNilKeepsGo120Behavior TestRandSeedIsHonored > "$LOGDIR/m6c-names.txt" 2>&1; n6c=$?
    [ "$c6c" != 0 ] && [ "$n6c" = 0 ]
    verdict "MR6c (EXPECT rc!=0 + both root tests go=pass cs=fail by name)" $? "rc=$c6c $(grep -a '^CMPNAME' "$LOGDIR/m6c-names.txt" | tr -d '\r' | sed 's/ (want[^)]*)//' | tr '\n' ' ' | cut -c1-220) (m6c-names.txt) control converter=e9009f2945$([ -n "$H2SHA" ] && echo " + $H2SEAT's stream reader")"
  else
    stamp "  MR6c: NOT MEASURED -- the M4 control converter was not built (the H2 reader patch: ctl-reader-m4.log; the build: conv-build-m4.log)"; FAILS=$((FAILS + 1))
  fi
fi

# MR6p -- the -recurse PROGRAM arm (the csproj <AssemblyAttribute> route): a go 1.20 main recovering panic(nil).
# EXPECT `go run .` and the converted program BOTH print 'recovered nil: true'; the project stamps panicnil=1.
F6p=$FX/m6/prog120
wf "$F6p/go.mod" 'module example.test/prog120\n\ngo 1.20\n'
wf "$F6p/main.go" 'package main\n\nimport "fmt"\n\nfunc main() {\n\tvar r any = "unset"\n\tfunc() {\n\t\tdefer func() { r = recover() }()\n\t\tpanic(nil)\n\t}()\n\tfmt.Println("recovered nil:", r == nil)\n}\n'
leg MR6p-oracle go-run-prog120 bash -c "cd '$F6p' && go run $OP ."
o6=$(tr -d '\r' < "$LOGDIR/go-run-prog120.log" | tail -n 1)
rm -rf "$OUT/prog120"
leg MR6p-convert recurse-prog120 "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F6p")" "$(cygpath -w "$OUT/prog120")"
P6=$(ls "$OUT"/prog120/src/example.test/prog120/*.csproj 2>/dev/null | head -n 1)
st6=$(grep -ac 'AssemblyAttribute Include="go.GoDefaultGodebugAttribute"' "$P6" 2>/dev/null); pn6=$(grep -ac 'panicnil=1' "$P6" 2>/dev/null)
leg MR6p-run run-prog120 bash -c "dotnet build '$(cygpath -w "$P6")' -c Release -p:UseSharedCompilation=false && dotnet run --project '$(cygpath -w "$P6")' -c Release --no-build"
rr6=$LEG_RC; c6=$(tr -d '\r' < "$LOGDIR/run-prog120.log" | grep -a '^recovered nil:' | tail -n 1)
[ "$o6" = 'recovered nil: true' ] && [ "${st6:-0}" = 1 ] && [ "${pn6:-0}" -ge 1 ] && [ "$rr6" = 0 ] && [ "$c6" = 'recovered nil: true' ]
verdict MR6p $? "go-run=[$o6] project-stamp=${st6:-0} panicnil=1-in-project=${pn6:-0} run rc=$rr6 converted=[$c6]"
# MR6e (critic 0 #7) -- r-m6's claim that internal/godebug layers the default UNDER the environment: the same stamped
# program with GODEBUG=panicnil=0 in its environment. EXPECT `go run .` and the converted program BOTH print
# 'recovered nil: false' (the env beats the go 1.20 default). The optional randseednop=1 arm on gd120 is not added.
leg MR6e-oracle go-run-prog120-env bash -c "cd '$F6p' && GODEBUG=panicnil=0 go run $OP ."
o6e=$(tr -d '\r' < "$LOGDIR/go-run-prog120-env.log" | tail -n 1)
leg MR6e-run run-prog120-env env GODEBUG=panicnil=0 dotnet run --project "$(cygpath -w "$P6")" -c Release --no-build
re6=$LEG_RC; c6e=$(tr -d '\r' < "$LOGDIR/run-prog120-env.log" | grep -a '^recovered nil:' | tail -n 1)
[ "$o6e" = 'recovered nil: false' ] && [ "$re6" = 0 ] && [ "$c6e" = 'recovered nil: false' ]
verdict MR6e $? "GODEBUG=panicnil=0 over the panicnil=1 stamp: go-run=[$o6e] run rc=$re6 converted=[$c6e] (EXPECT both 'recovered nil: false')"
if [ "$CONTROLS" = 1 ]; then
  # MR6pc, the scope control: the SAME main converted PLAINLY (no -recurse; second positional = its output dir,
  # floor 3) carries NO stamp (the ruled M6 scope) and so keeps the corpus release's behaviour. EXPECT no
  # GoDefaultGodebug in its project and 'recovered nil: false' -- proving the stamp is what flips MR6p.
  rm -rf "$OUT/prog120-single"
  leg MR6pc-convert single-prog120 "$EXE" -go2cspath "$WB\\src" "$(cygpath -w "$F6p")" "$(cygpath -w "$OUT/prog120-single")"
  P6s=$(ls "$OUT"/prog120-single/*.csproj 2>/dev/null | head -n 1); ss6=$(grep -ac 'GoDefaultGodebug' "$P6s" 2>/dev/null)
  leg MR6pc-run run-prog120-single bash -c "dotnet build '$(cygpath -w "$P6s")' -c Release -p:UseSharedCompilation=false -p:go2csPath='$WB\\src\\' && dotnet run --project '$(cygpath -w "$P6s")' -c Release --no-build -p:go2csPath='$WB\\src\\'"
  cs6=$(tr -d '\r' < "$LOGDIR/run-prog120-single.log" | grep -a '^recovered nil:' | tail -n 1)
  # The label carries no ':' (critic 0 #1: the battery's old grep stopped at the colon inside 'recovered nil: false').
  [ "${ss6:-1}" = 0 ] && [ "$cs6" = 'recovered nil: false' ]; verdict "MR6pc (EXPECT no stamp, recovered nil false)" $? "stamp-lines=${ss6:-?} converted=[$cs6]"
fi

# =========================================================================================== XS / XM -- REAL MODULES (M)
# P2's fix batch END ACCEPTANCE (tL-seats-draft.txt:67, read on P2's LOCAL merge 56530163c2, a linux root box, at the
# DEFAULT -test-timeout): "x/sync errgroup 5, syncmap 3, singleflight 11/12 (H1), semaphore 7/8 (TC0 timing); x/mod
# modfile 323, module 16, semver 9, dirhash 6, sumdb 4, note 7, storage 1, tlog 16/17 (H2), zip builds+runs 109/9/3".
# The M union is the first PUSHED tree that carries all six fixes, and this is the first WINDOWS reading of either
# module (P1's FINDING record, section 8: "windows and darwin for every row" NOT MEASURED). So every count below is a
# LINUX expectation applied to this box: a difference is a FAIL to READ, not a known regression.
# Each module is COPIED from the module cache to the run folder (P1's method: the cache itself is never a working
# directory), with a fresh output root outside it, and its tree is hashed before and after. The copy of x/sync is left
# exactly as cp made it, read-only files included, and their count is stamped: staging a read-only module is the path
# TRAIN L's blocker lived on (the ReadOnly attribute surviving into the test sandbox).
# Verdicts (tFL-helpers.py realmod): a package validates its expected count, or it is one of the three KNOWN non-passes
# read BY NAME (stamped KNOWN, never a failure), or it FAILS:
#   singleflight   TestPanicDoChan, C# has no verdict   = host finding H1 (the host ignores -test.list: child fan-out
#                                                         until the deadline). P2's H1 seat (p2-host-test-list)
#                                                         landed with M: the package is RUN and 12 is expected
#   sumdb/tlog     TestCertificateTransparency, C# none = host finding H2 (a JSON event that does not start its line);
#                                                         the Go side talks to a network log, so its verdict is not pinned.
#                                                         P2's H2 seat (p2-host-event-line-start) landed with M:
#                                                         17 is expected, a red is read against the network first
#   semaphore      TestWeightedAcquire, Go pass C# fail = the known timing-class non-pass (P2's acceptance labels it
#                                                         "TC0 timing"; its cause is NOT MEASURED by anyone)
# A KNOWN package that VALIDATES instead reads CLEARED and passes (its seat landed). zip: its host builds and runs; its
# counts are REPORTED (P2 read 109 / 9 / 3 with 4 TestVCS mismatches that were that box's network). Verify round 1:
# 'zip=build:TestVCS' -- a differing name is tolerated only under TestVCS; any OTHER differing name in zip is a FAIL
# (plain 'build' read PASS on any new C#-only failure in the one package whose own test code P1 never read).
# F4's gate rides on both runs: 0 'dotnet timed out' lines.
rmrun(){ # name, suffix, moduleDir(unix), outRoot(unix) -- the whole-module driver, -test-timeout STATED
  rm -rf "$4"
  leg "$1" "$2" "$EXE" -tests -test-action all -test-config Release -test-timeout "$REALMOD_TEST_TIMEOUT" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$3")" "$(cygpath -w "$4")"
}
realread(){ # label, module path, log (unix), outRoot (unix), spec... -- one stamped line per package; FAIL counts
  local l to
  hp realmod --log "$(cygpath -w "$3")" --root "$(cygpath -w "$4/src/$2")" --module "$2" "${@:5}" > "$LOGDIR/$1-realmod.txt" 2>&1
  while IFS= read -r l; do
    case "$l" in
      REALMOD*": FAIL -- "*) stamp "  $1 ${l:0:340}"; FAILS=$((FAILS + 1)) ;;
      REALMOD*": EXTERNAL -- "*) stamp "  $1 ${l:0:460}"; EXTERNALS=$((EXTERNALS + 1)); printf '%s-EXTERNAL %s\n' "$1" "$(printf '%s' "$l" | cut -c9-300)" >> "$LOGDIR/OWED-rereads.txt" ;;   # Q (Q4): named, counted apart, OWED; never a pass and not a FAIL
      REALMOD*) stamp "  $1 ${l:0:340}" ;;
    esac
  done < <(tr -d '\r' < "$LOGDIR/$1-realmod.txt")
  grep -aq '^REALMOD-VERDICT' "$LOGDIR/$1-realmod.txt" || { stamp "  $1 realmod: FAIL -- the reader printed no verdict ($1-realmod.txt)"; FAILS=$((FAILS + 1)); }
  to=$(grep -a '^REALMOD-F4' "$LOGDIR/$1-realmod.txt" | grep -oE 'log: [0-9]+' | tr -dc '0-9')
  # Verify round 1: F4's gate discriminates only while -test-timeout is SHORTER than a first publish. The floor is
  # max(-test-timeout, 30m) (testPublishTimeoutFloor at aa994a1a1c), and P1's pre-F4 run at -test-timeout 20m had no
  # 'dotnet timed out' line either. At the 2m default the verdict stands; at any other value it is NOT MEASURED.
  case "$REALMOD_TEST_TIMEOUT" in
    2m|2m0s|120s) [ "${to:-x}" = 0 ]; verdict "$1-F4 (EXPECT 0 'dotnet timed out' lines at -test-timeout $REALMOD_TEST_TIMEOUT)" $? "lines=${to:-unread}" ;;
    *) notmeasured "$1-F4" "-test-timeout $REALMOD_TEST_TIMEOUT is not the 2m default: a deadline a first publish fits inside cannot red (lines=${to:-unread})" ;;
  esac
}
# Verify round 2 (RULED; R1: read from the seat list, never written here). Host finding H1 is "the host ignores
# -test.list": singleflight's panic tests re-execute the test binary, and COORD's later reading of the seat measured the
# growth as UNBOUNDED until the deadline (1, 1, 2, 4, 8, 12, 16, 28, 32 hosts in 12.5 s). With no H1 host seat in the
# union that package is NOT RUN on this box: its folder is removed from the module COPY before anything is hashed or
# run (the -tests -recurse driver has no package filter; the cache is never touched), and the reading is stamped.
# With the seat in the union it runs and 12 of 12 is expected (the seat's own acceptance). The H2 host seat is read the
# same way for sumdb/tlog: a row means 17 is expected (the KNOWN shape would then be the seat not working in the union);
# no row keeps the KNOWN spec. A standalone run with no seat list beside it reads both as ABSENT.
# Verify round 3: H1SEAT / H2SEAT are derived (and stamped) near the top of the script, once: the controls read H2 too.
if [ "$REALMOD" != 1 ]; then
  notmeasured XS "REALMOD=$REALMOD (COORD's explicit call)"; notmeasured XM "REALMOD=$REALMOD (COORD's explicit call)"
else
  # ---- XS: golang.org/x/sync@v0.19.0 (no requirements)
  XSNEED="$GMCU/golang.org/x/sync@v0.19.0/go.mod"
  if xswhy=$(present $XSNEED); then
    FXS=$FX/xsync; rm -rf "$FXS"; mkdir -p "$FXS"
    cp -r "$GMCU/golang.org/x/sync@v0.19.0" "$FXS/sync"; cprc=$?
    SFSPEC='singleflight=12'; SFOK=0
    if [ -n "$H1SEAT" ]; then
      stamp "  XS golang.org/x/sync/singleflight: RUN (the H1 host seat $H1SEAT is in the union): EXPECT Validated 12, 12 of 12"
    else
      SFSPEC=''
      chmod -R u+w "$FXS/sync/singleflight" 2>/dev/null; rm -rf "$FXS/sync/singleflight"
      [ ! -e "$FXS/sync/singleflight" ] || SFOK=1
      stamp "  XS golang.org/x/sync/singleflight: NOT RUN (KNOWN: H1, child fan-out) -- no H1 host seat in $(basename "$SEATS") or on master's line; the package folder was removed from the module COPY before the hash and the run (a STATED DEVIATION; removed=$([ "$SFOK" = 0 ] && echo yes || echo 'NO: nothing of XS is run'))"
    fi
    BXS=$(hashtree "$FXS/sync"); nxs=$(echo "$BXS" | grep -c .)
    ro=$(find "$FXS/sync" -type f ! -perm -u+w | wc -l)
    stamp "  XS copy: cp rc=$cprc files=$nxs read-only files in the copy=$ro (P1 hashed 19 files on linux; fewer here when singleflight was removed)"
    # Verify round 1: the cache's own files are all read-only, and staging a read-only module is the path TRAIN L's
    # blocker lived on. A copy that came out writable would read green without touching that path, so it is a verdict.
    [ "$ro" = "$nxs" ] && [ "$nxs" != 0 ]; verdict XS-readonly-copy $? "read-only files in the copy=$ro of $nxs (a writable copy does not exercise the staging path of TRAIN L's blocker)"
    if [ "$SFOK" != 0 ]; then
      verdict XS 1 "singleflight could not be removed from the module copy and no H1 host seat is in the union: the module is NOT run (the fan-out is not started on this box)"
    else
    rmrun XS tests-recurse-xsync "$FXS/sync" "$OUT/xsync"; rxs=$LEG_RC
    stamp "  XS driver: rc=$rxs (non-zero is EXPECTED while a KNOWN package fails) :: $(tr -d '\r' < "$LOGDIR/tests-recurse-xsync.log" | grep -aE 'Converting [0-9]+ packages|phase B: [0-9]+ of [0-9]+' | tr '\n' ' ' | cut -c1-200)"
    realread XS golang.org/x/sync "$LOGDIR/tests-recurse-xsync.log" "$OUT/xsync" errgroup=5 syncmap=3 $SFSPEC 'semaphore=7/8:TestWeightedAcquire:pass:fail'
    [ "$cprc" = 0 ] && [ "$(hashtree "$FXS/sync")" = "$BXS" ]; verdict XS-module-unchanged $? "the module copy's $nxs files hash the same before and after the run"
    # ---- XS2, Q (Q7): THE SECOND RUN INTO THE SAME OUTPUT ROOT, nothing changed (COORD's landing reading at P, run 1b,
    # made a leg). P's symbol loss showed only on an unchanged second publish, and through the whole-module driver it
    # is this command run twice. EXPECT every package that validated in the first run to validate the SAME count again,
    # the same number of symbol files beside every published host before and after, and 0 symbol refusals.
    # It is also the CONTROL RUN of the timing class (COORD's notes of 2026-10-05 01:52: semaphore TestWeightedAcquire
    # and singleflight TestPanicDo fail only their own sub-2-second windows, on a cold first run of a fresh host under
    # load): a package that FAILED in the first run and validates here is stamped TIMING-CLASS and written to the OWED
    # file. Its FAIL above STAYS COUNTED: whether that class may be subtracted from the gate is not ruled.
    # REVIEW ROUND 1: the arm passed on NOTHING. 'before = after' held when both read 0 (a publish folder that is not
    # where xspdb looks: x/sync's verdicts do not depend on symbols, so no other reading would have moved), and the
    # second run's rc was printed and never gated. Now (a) every package that was RUN must show MORE THAN ONE .pdb
    # beside its published host before and after (P's kept records: errgroup 173, semaphore 66, singleflight 67,
    # syncmap 67; T2, S2 and the linux two-pass arm carry the same floor); singleflight is left out of the floor when
    # this box did not run it (no H1 host seat); (b) the second run's rc must equal the first's.
    XSPK='errgroup semaphore syncmap'; [ -e "$FXS/sync/singleflight" ] && XSPK='errgroup semaphore singleflight syncmap'
    xspdb(){ for xp in $XSPK; do printf '%s=%s ' "$xp" "$(ls "$OUT/xsync/src/golang.org/x/sync/$xp/bin/tests/publish"/*.pdb 2>/dev/null | wc -l)"; done; }
    # >>> pdbfloor (tFL-controls.sh arm PF extracts the line between the markers; tFL-linux-legs.sh and tFL-reread.sh carry the same line)
    pdbfloor(){ local t n ok=0; [ -n "$1" ] || return 1; for t in $1; do n=${t#*=}; case "$n" in ''|*[!0-9]*) ok=1 ;; *) [ "$n" -gt 1 ] || ok=1 ;; esac; done; return $ok; }
    # <<< pdbfloor
    XSB=$(xspdb); xsv1=$(validated "$LOGDIR/tests-recurse-xsync.log" | LC_ALL=C sort)
    leg XS2 tests-recurse-xsync-2 "$EXE" -tests -test-action all -test-config Release -test-timeout "$REALMOD_TEST_TIMEOUT" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$FXS/sync")" "$(cygpath -w "$OUT/xsync")"; rxs2=$LEG_RC
    XSA=$(xspdb); xsv2=$(validated "$LOGDIR/tests-recurse-xsync-2.log" | LC_ALL=C sort)
    xslost=$(LC_ALL=C comm -23 <(printf '%s\n' "$xsv1") <(printf '%s\n' "$xsv2") | grep . | tr '\n' ';')
    xsgain=$(LC_ALL=C comm -13 <(printf '%s\n' "$xsv1") <(printf '%s\n' "$xsv2") | grep . | tr '\n' ';')
    xssym=$(pubsym "$LOGDIR/tests-recurse-xsync.log" "$LOGDIR/tests-recurse-xsync-2.log")
    pdbfloor "$XSB"; xsfb=$?; pdbfloor "$XSA"; xsfa=$?
    [ "$XSB" = "$XSA" ] && [ "$xsfb" = 0 ] && [ "$xsfa" = 0 ] && [ -z "$xslost" ] && [ "$xssym" = 0 ] && [ -n "$xsv2" ] && { [ "$rxs2" = "$rxs" ] || [ -n "$xsgain" ]; }
    verdict XS2 $? "the unchanged second run into the same root: rc=$rxs2 (the first: $rxs; EXPECT equal, unless a package validated only in run 2: the timing class below); .pdb beside each published host before [$XSB] after [$XSA] (EXPECT equal, and MORE THAN ONE each: floor before=$([ "$xsfb" = 0 ] && echo ok || echo FAILED) after=$([ "$xsfa" = 0 ] && echo ok || echo FAILED)); validated in run 1 and not the same in run 2: [${xslost:-none}]; validated only in run 2: [${xsgain:-none}]; symbol-refusal lines=$xssym"
    if [ -n "$xsgain" ]; then
      stamp "  XS TIMING-CLASS (the control run): [${xsgain%;}] did not validate in the first run and validate(s) in the unchanged second one -- the shape of a test that fails only its own sub-2-second window on a cold first run (semaphore TestWeightedAcquire, singleflight TestPanicDo). The first run's FAIL stays counted; OWED: three isolated re-reads at the landing head (tFL-reread.sh xsync)"
      printf 'XS-TIMING golang.org/x/sync [%s] failed the first run and validated the unchanged second one: three isolated re-reads owed (tFL-reread.sh xsync)\n' "${xsgain%;}" >> "$LOGDIR/OWED-rereads.txt"
    fi
    fi
  else
    notmeasured XS "module absent ($xswhy) in $GMC (R6: never downloaded)"
  fi
  # ---- XM: golang.org/x/mod@v0.33.0. Its go.mod requires golang.org/x/tools v0.41.0 (only zip/zip_test.go imports it).
  # When the cache holds v0.41.0 the run is LITERAL. When it holds v0.42.0 instead (the i7 and P1's box, 2026-10-02) the
  # literal run is refused by M2's preflight under GOPROXY=off, and the copy takes P1's STATED DEVIATION: go.mod's one
  # require line names v0.42.0 and go.sum gains that version's two lines, built from the cache's own files (the zip's
  # recorded h1, and the h1 of its .mod file by go's own rule -- the same computation reproduces the cached go.sum lines
  # of x/sync v0.19.0 and x/mod v0.33.0, checked 2026-10-02). Nothing else in the copy changes; the pin is stamped.
  XT=cache/download/golang.org/x/tools/@v
  XMNEED="$GMCU/golang.org/x/mod@v0.33.0/go.mod"
  XMPIN=''
  if [ -e "$GMCU/$XT/v0.41.0.zip" ] && [ -e "$GMCU/$XT/v0.41.0.mod" ]; then XMPIN=literal
  elif present "$GMCU/golang.org/x/tools@v0.42.0/go.mod" "$GMCU/$XT/v0.42.0.mod" "$GMCU/$XT/v0.42.0.zip" "$GMCU/$XT/v0.42.0.ziphash" > /dev/null; then XMPIN=v0.42.0; fi
  if xmwhy=$(present $XMNEED) && [ -n "$XMPIN" ]; then
    FXM=$FX/xmod; rm -rf "$FXM"; mkdir -p "$FXM"
    cp -r "$GMCU/golang.org/x/mod@v0.33.0" "$FXM/mod"; cprc=$?
    pinok=0
    if [ "$XMPIN" = v0.42.0 ]; then
      chmod u+w "$FXM/mod/go.mod" "$FXM/mod/go.sum"
      had=$(grep -c 'golang.org/x/tools v0.41.0' "$FXM/mod/go.mod")
      sed -b -i 's#golang.org/x/tools v0.41.0#golang.org/x/tools v0.42.0#' "$FXM/mod/go.mod"
      zh=$(tr -d '\r\n' < "$GMCU/$XT/v0.42.0.ziphash")
      mh=$(python -c "import hashlib,base64,sys; d=open(sys.argv[1],'rb').read(); s=hashlib.sha256(d).hexdigest()+'  go.mod'+chr(10); print('h1:'+base64.b64encode(hashlib.sha256(s.encode()).digest()).decode())" "$(cygpath -w "$GMCU/$XT/v0.42.0.mod")" | tr -d '\r')
      printf 'golang.org/x/tools v0.42.0 %s\ngolang.org/x/tools v0.42.0/go.mod %s\n' "$zh" "$mh" >> "$FXM/mod/go.sum"
      case "$had:$zh:$mh" in 1:h1:*:h1:*) ;; *) pinok=1 ;; esac
      [ "$(grep -c 'golang.org/x/tools v0.42.0' "$FXM/mod/go.mod")" = 1 ] || pinok=1
      stamp "  XM pin (STATED DEVIATION, P1's): go.mod require golang.org/x/tools v0.41.0 -> v0.42.0 (lines edited: $had), go.sum +2 lines ($zh ; go.mod $mh) :: $([ "$pinok" = 0 ] && echo applied || echo 'PIN FAILED')"
    else
      stamp "  XM pin: none needed (the cache holds golang.org/x/tools v0.41.0): the LITERAL module"
    fi
    if [ "$cprc" = 0 ] && [ "$pinok" = 0 ]; then
      BXM=$(hashtree "$FXM/mod"); nxm=$(echo "$BXM" | grep -c .)
      rmrun XM tests-recurse-xmod "$FXM/mod" "$OUT/xmod"; rxm=$LEG_RC
      stamp "  XM driver: rc=$rxm (non-zero is EXPECTED while a KNOWN package fails) :: $(tr -d '\r' < "$LOGDIR/tests-recurse-xmod.log" | grep -aE 'Converting [0-9]+ packages|phase B: [0-9]+ of [0-9]+|preflight' | tr '\n' ' ' | cut -c1-300)"
      if grep -aq 'go mod download preflight failed' "$LOGDIR/tests-recurse-xmod.log"; then
        notmeasured XM "M2's preflight refused the module under GOPROXY=off (a requirement is not in $GMC): $(grep -a 'preflight failed' "$LOGDIR/tests-recurse-xmod.log" | head -n 1 | tr -d '\r' | cut -c1-220)"
      else
        TLSPEC='sumdb/tlog=16/17:TestCertificateTransparency:*:-'; [ -z "$H2SEAT" ] || TLSPEC='sumdb/tlog=17'   # verify round 2: the H2 host seat in the union means 17 is expected (its Go side reads a network log: a red there is read against the network first)
        realread XM golang.org/x/mod "$LOGDIR/tests-recurse-xmod.log" "$OUT/xmod" modfile=323 module=16 semver=9 sumdb/dirhash=6 sumdb=4 sumdb/note=7 sumdb/storage=1 "$TLSPEC" zip=build:TestVCS
        # verify round 3 (RULED): with the H2 seat in the union a red on sumdb/tlog is read against the NETWORK first, and stamped so (the FAIL itself is counted above)
        grep -aqE '^REALMOD golang\.org/x/mod/sumdb/tlog: FAIL -- ' "$LOGDIR/XM-realmod.txt" && stamp "  XM sumdb/tlog red: read it against this box's NETWORK first (TestCertificateTransparency's Go side reads a network log); only then against the H2 seat ${H2SEAT:-(absent)}"
        [ "$(hashtree "$FXM/mod")" = "$BXM" ]; verdict XM-module-unchanged $? "the module copy's $nxm files hash the same before and after the run (hashed after the pin)"
      fi
    else
      notmeasured XM "the module copy or its pin failed (cp rc=$cprc pin=$pinok): nothing was run"
    fi
  else
    notmeasured XM "module absent (${xmwhy:-golang.org/x/tools at neither v0.41.0 nor v0.42.0}) in $GMC (R6: never downloaded)"
  fi
fi
unset MSYS_NO_PATHCONV

# =========================================================================================== cleanup + END
cleanup_tree
# Verify round 2 (RULED): a census of processes BY EXECUTABLE PATH under the module out root, after the last leg. Every
# test host of these legs is published under $OUT, so the path prefix names them and nothing else; the querying process
# is excluded. A survivor (H1's fan-out outliving its deadline, a host that escaped its job object) is a FAIL verdict,
# listed by PID and path and NEVER killed (floor 5): COORD kills by PID after reading orphans.txt. The lock is KEPT (the
# battery keeps its own on the same line). A census that could not be read is a FAIL too: no pass by absence.
ORPH="$LOGDIR/orphans.txt"
# Verify round 3: a host that is still EXITING when the last leg returns is not an orphan, and under the battery a
# survivor now stops the run (RULED). The census is read up to three times, 20 s apart; the verdict is the last reading.
for orn in 1 2 3; do
powershell -NoProfile -Command "\$p = '$(cygpath -w "$OUT")' + [char]92; Get-CimInstance Win32_Process | Where-Object { \$_.ProcessId -ne \$PID -and \$_.ExecutablePath -and \$_.ExecutablePath.StartsWith(\$p, [StringComparison]::OrdinalIgnoreCase) } | ForEach-Object { '{0} {1} {2}' -f \$_.ProcessId, \$_.Name, \$_.ExecutablePath }" > "$ORPH" 2>&1 < /dev/null; orc=$?
norph=$(tr -d '\r' < "$ORPH" | grep -c .)
  { [ "$orc" = 0 ] && [ "$norph" = 0 ]; } || [ "$orn" = 3 ] || { sleep 20; continue; }
  break
done
[ "$orc" = 0 ] && [ "$norph" = 0 ]; verdict MOD-orphans $? "census rc=$orc (reading $orn of 3, 20 s apart), $norph process(es) still running from $OUT (EXPECT 0; listed in mod-logs/orphans.txt, NOT killed): $(tr -d '\r' < "$ORPH" | head -n 4 | cut -c1-160 | tr '\n' ' ')"
[ "$orc" = 0 ] && [ "$norph" = 0 ] || KEEP_LOCK=1
left=$(git status --porcelain | grep -vc '^??'); td=$(git status --porcelain | grep -c '^ D')
# Verify round 2 (M-requirements section 3: "no proof page for x/sync or x/mod; nothing here is a roster row"): tracked
# changes are counted above, and an UNTRACKED page under docs/ would have passed. Any status line under docs/ fails.
ddocs=$(git status --porcelain -- docs | grep -c .)
OWEDN=$(owedn)
stamp "END failures=$FAILS external=$EXTERNALS owed-lines=$OWEDN (Q4: a KNOWN-EXTERNAL or TIMING-CLASS line is OWED an isolated re-read at the landing head; it is never a pass. The file mod-logs/OWED-rereads.txt always exists: 0 lines = nothing owed; UNREAD = it was removed under this run)"
stamp "END tracked-changes=$left tracked-deletions=$td docs-status-lines=$ddocs (EXPECT 0: no proof page, no roster edit) head=$(git rev-parse --short=10 HEAD) (fixtures + outputs kept in $SD/fx and $SD/out for reading; delete after)"
stamp "MODULE LEGS DONE"
# critic 0 #1 / critic 1 #1: a failed verdict must reach the battery's NONZERO LEGS (exit 4); tracked changes or a
# deletion left behind are a failure of this script too.
[ "$FAILS" = 0 ] && [ "$left" = 0 ] && [ "$td" = 0 ] && [ "$ddocs" = 0 ] || exit 4
# REVIEW ROUND 1: an OWED reading is never exit 0 on a STANDALONE run. Under the battery (IN_BATTERY=1) the status
# stays 0 here: the battery reads the owed file itself, repeats it on its END line and ends EXIT 7 when that is all
# that stands (a non-zero MOD leg would turn the same event into EXIT 6, a red). Run by hand, this script's status is
# the one line a person reads, and at P the same event was exit 4: 7 = green, and a re-read is owed at the landing head.
[ "$OWEDN" = 0 ] || [ "$IN_BATTERY" = 1 ] || { stamp "EXIT 7 (standalone): no failed verdict, and $OWEDN reading(s) are OWED at the landing head (mod-logs/OWED-rereads.txt; tFL-reread.sh): never read this run as green without them"; exit 7; }
exit 0
