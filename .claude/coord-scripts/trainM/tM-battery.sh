#!/usr/bin/env bash
# TRAIN M battery on the i7 -- the union of record (COORD). DRAFT 2026-10-02, derived from tL-battery.sh (TRAIN L's
# battery of record, run 2: coord-scratch/tL/run2). Every change against L is in tM-CHANGES.md. NOT RUN by its author.
# Launch from a PER-RUN COPY (floor 4): copy this folder (tM-battery.sh, tM-emitcheck.sh, emitdrift.py, tM-helpers.py,
# tM-modules-legs.sh, tM-i9-te.sh, tM-ng-parse.ps1, tM-follow.txt, tM-i7-sweeps.txt, tM-i9-shard.txt) AND the seat list
# (hnd .claude/coord-scripts/trainM/tM-seats-draft.txt) to a FRESH run folder (PRE refuses a folder that already holds a
# SUMMARY.txt) and run the copy:
#   EXPECT_HEAD=<fixup sha, 10 chars> [MASTER=aa0a07d5fd] [DEADLINE='YYYY-MM-DD HH:MM'] [I9_PATCH=<i9 tracked-changes-U0.patch>] \
#     [CNR_EXPECT_N=<n>] [REALMOD_TEST_TIMEOUT=2m] bash tM-battery.sh
# A ruled FOLLOW-UP merge is listed in tM-follow.txt, and its origin ref is fetched into the tM worktree BEFORE launch:
# the battery reads that remote-tracking ref and never fetches.
# Logs land beside the copy in tM-logs/ (the module legs in mod-logs/).
# DEADLINE (R5; verify round 2, RULED): no leg STARTS after it. An ABSOLUTE local time, 'YYYY-MM-DD HH:MM', resolved
# once to epoch seconds at launch; a deadline already in the past is refused; unset = launch + 14 hours, stamped. (The
# old same-day HH:MM integer was 'past' at launch for a next-morning value and void once the clock passed midnight.)
# ONE BATTERY AT A TIME (floors 1, 11): PRE takes /h/go2cs-tmp-coord/coord-scratch/tM/.battery.lock (mkdir) and the
# EXIT trap releases it; tM-modules-legs.sh and tM-fixup.sh take the same lock.
# UNION SHAPE (PRE refuses anything else): TRAIN L's landed master aa0a07d5fd + one signed merge per row of the seat
# list, in row order (24 rows in the list FROZEN at 08:20 on 2026-10-02; the count is READ from the list) [+ the
# ruled follow-up merges of tM-follow.txt: EMPTY, none is owed at the frozen list] + the signed TRAIN M FIXUP 'fixup: TRAIN M'
# (tM-fixup.sh) [+ zero or more 'fixup-N: TRAIN M' commits on top of it, in order: floor 9, a pushed sha is never
# replaced]. The fixup is OWED (seat-footprints.md H1, H2; measured on the first pre-map union 8d7305053f):
#   (G1) two of TRAIN L's goldens predate G's go-creator frame and are stale under the union converter (PREDICTED:
#        ForeverWaitWorkersMainReturns, MainSelectForeverWorkerExits; the fixup measures with CNR and re-baselines);
#   (G2) the corpus files two seats' converter changes reach and no seat committed (F1+F6's go/internal/srcimporter
#        hunk; G's Extension A): the fixup copies the union converter's own emission, so leg E reads 0 here.
#   A Go-only repro directory under the behavioral root (hazard H4) is NOT the fixup's: PRE-1's H4 arm refuses one.
#   At the frozen list it reads 0: row p2-test-overload-references (e002a552a8) deletes the repro module
#   TestNeedsTransitiveApiRef in-seat, so no follow-up merge is owed (tM-README.md, M2).
# The i9 sweeps the complement shard (tM-i9-shard.txt); P1/P2 run tM-linux-legs.sh. Legs run SEQUENTIALLY; the tM
# worktree is FROZEN while this runs (floor 4); every leg's rc is captured before any pipe (floor 7). One battery at a
# time on this box.
#
# LEG -> SEAT MAP (every leg's comment repeats its seats; refs are rows of tM-seats-draft.txt):
#   C      every converter seat: g-godebug-pc-line (G), c1-darwin-linkname-pulls (S7), c1-fixture-tracking, the i9's
#          byRefReceiverGuard, P2's F4/F5/F3/F7/F1+F6/F8; repoguard (S7's pull census; the docs suites meet P1's FINDING)
#   CB     the M seats' converter and repoguard tests BY NAME (-v, 0 SKIP) + the 5 projitems tests (five seats register)
#   FX     c1-fixture-tracking (ten fixtures join the currency guard; the count is READ); FXc its two controls (floor 13)
#   E      union-attributable 0 AT THE FIXUP HEAD (the fixup regenerated what no seat committed); csproj drift none
#   E-bisect (only on E > 0)  one arm per named emission seat, the shas read from the union's own merges
#   G1/G2  G's two roster commits beside master's TRAIN L provenance block: 1 row with an execution config (log/slog)
#   SY     symbol sync (cheap, every train)        SI/SIc the 7 new go2cs.slnx projects (i9 1, P2 6)
#   ST51/ST7, NV*  regression: G edits src/_roster.ps1 (a comment), which the sweep and push-nuget dot-source
#   2b     go2cs.slnx END TO END: i9-crashclass (golib + src/gen), c1-darwin-std-hygiene (golib), G (Goroutine.cs)
#   CT     i9-channeltests-makechan-panic: ChannelTests Debug + Release, 24/24 each (no battery ever ran this project)
#   GN     i9-crashclass and p1-warnings-tranche1 (src/gen): GenTests; Failed 0 and Total >= a derived floor (never below 68)
#   TR     the testing host: i9 ([GoRecv] on 25 testing.cs methods), F4 + F8 (testConversion.go)
#   GT     G (SystemGoroutineTests, ReturnSiteLineTests), i9 (CopyBoundReceiver*, NoUncountedBackingAllocations), the
#          checkdead classes under G's entry classification (hazard H3), c1-darwin-std-hygiene (builtin.cs)
#   4      CNR: every golden under the union converter, the fixup's re-baselined ones included; N is REPORTED
#   5 + B: the 7 new M projects, TRAIN L's five checkdead guards under G's classification (H3), 8 neighbours
#   H7     i9 (siginfo_linux.cs builds only for linux), S7's 9 darwin files, std-hygiene's darwin arm, golib x3
#   MOD    tM-modules-legs.sh: L's module legs as the REGRESSION of the -tests -recurse driver (F4 and F8 edit
#          testConversion.go), then XS + XM: golang.org/x/sync and golang.org/x/mod, P2's fix batch end acceptance
#   PB     F4: publishTestHost's new floor composed with -test-publish-binlog (L's c2-binlog arm, kept for that reason)
#   S      every row on this box at ITS roster execution config: net/http now at TC0 (G), by name; + X_ROWS
#          (internal/godebug at TC0, log/slog release-tiered: i9 rows this battery ALSO reads, like the canaries)
#   NR     hazard H3: net/rpc x5 total, 0 'all goroutines are asleep' lines each
#   TE/HS  G's frame class on the rewritten test sources (a READING, with TRAIN L's patch as baseline); host censuses
#   UF     c1-fixture-tracking: 0 untracked, not-ignored files under src/core after the sweeps and after the T legs
#   T      runtime/pprof (TestMemoryProfiler by name) + runtime (TestTracebackSystem/panic DISCLOSED, by name: the
#          first reading of checkdead and G's entry classification together); TBS-W before T:runtime; TE-T/HS-T;
#          TL-WALL (row wall vs TRAIN L's run 2; a prompt to read, never a gate)
#   TE-i9  the same TE reading over the i9's shard patch (only when I9_PATCH names it; else tM-i9-te.sh later)
#   NG*    L's nugetgo legs, KEPT: regression while no seated row touches src/tools (the leg STAMPS the union's own count
#          of changed paths there); the i7 half of two ANNOUNCED rows' acceptance once they seat (tL-seats-draft.txt:75)
# NOT here (tM-README.md): HOP, SPB (no seat touches run-validated-sweep.ps1), the C1 token probe, L's TE signatures and bisect arms.
# VERIFY ROUND 1 (tM-CHANGES.md section 9): a LIVE gate across trains before the lock; MASTER defaults to aa0a07d5fd;
# findings where a gate was only a stamp (the two BANK readings against the roster, XS/XM not read, SIc and NVR not
# firing, csproj among the sweep rewrites, GN/TR/2b/FX counts, the row that keeps release-tiered, leg E's verdict);
# CNR's rewrites are restored before MOD; each row's converted-host elapsed is stamped (HOSTWALL).
# VERIFY ROUND 2 (tM-CHANGES.md section 10; COORD's rulings wf/notes/COORD-RULINGS-r2.md):
#  R1  NO BY-NAME LIST IS A LITERAL ONLY COORD CAN KEEP CURRENT. Section PRE-D derives, from git between $MASTER and
#      HEAD, and stamps: the converter tests the union adds (leg CB), the GolibTests classes it adds or changes and the
#      ones this box does not compile (GT), the behavioral projects it adds (legs 4 and 5), the goldens the fixup
#      re-baselined (leg 5), the rows whose execution config the union moves or keeps (leg S), the seats that change
#      the converter (E-bisect's superset). The literals that remain are chosen by a hazard, not by a seat list
#      (GT_NEIGH, GT_TESTS, CHECKDEAD_GUARDS, NEIGH, E_BISECT_KNOWN, X_ROWS, T_ROWS, HS_WANT): each is checked against
#      the tree and a miss is a FINDING by name. Totals (GN, CT, TR, FX, ST) are floors: 0 failed AND total >= a
#      derived or measured floor, stamped with its source.
#  *   A record a leg reads must be THIS run's: the row's gitignored record files are deleted before every S row, NR
#      repeat and T leg (the converter rewrites the comparison record only when its bytes change, so a byte-identical
#      one kept an old mtime and read STALE, and a stale one could read as the run's own), and a leg that leaves no
#      fresh record is a finding. No gate passes by absence ('DEADLOCK total=0' counts only with record=fresh).
#  *   Tracked rewrites of CNR and of the behavioral legs are restored again before MOD (tracked changes asserted 0).
#  *   GT, CT, GN, TR run under the wall cap (45m, 10m, 20m, 20m): a fired cap stops the battery, lock kept, exit 5.
#  *   PRE admits 'fixup: TRAIN M' + zero or more 'fixup-N: TRAIN M' at the head (floor 9).
#  *   XS: singleflight runs only when the H1 host seat is in the union; a process census by executable path after MOD.
#  *   The battery's exit status is 6 when any leg outside the expected-non-zero set is non-zero or any FINDING exists.
# VERIFY ROUND 3 (tM-CHANGES.md section 11; COORD's rulings wf/notes/COORD-RULINGS-r3.md; the seat list is FROZEN):
#  *   seats-effective.txt is built as tM-fixup.sh builds it (row lines only, each terminated) and its row count asserted.
#  *   FXGOLD holds only behavioral PROJECT directories (a csproj directly in them), never a file of the behavioral root.
#  *   A process still running from the module out root after MOD STOPS the battery (exit 5, lock kept), RULED.
#  *   MOD's controls MR4c / MR6c: their TRAIN L converters carry the H2 seat's stream reader (tM-modules-legs.sh).
W=/h/go2cs-tmp-coord/tM
WB='H:\go2cs-tmp-coord\tM'
EXPECT_HEAD=${EXPECT_HEAD:?the TRAIN M fixup commit, 10 chars}
MASTER=${MASTER:-aa0a07d5fd}            # TRAIN L's landed master: the ONE base every tM script names (verify round 1: no longer typed at launch)
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tM-seats-draft.txt}   # ONE name (L also looked for a frozen tL-seats.txt that never existed)
FOLLOWF=$SD/tM-follow.txt
SEATS_EXPECTED=${SEATS_EXPECTED:-}       # optional second derivation of the seat count (the GO's number); the list is the first
I7LIST=$SD/tM-i7-sweeps.txt
I9LIST=${I9LIST:-$SD/tM-i9-shard.txt}
T_ROWS='runtime runtime/pprof'           # roster rows on the i7 list read by the T legs (K's tleg), never by the S loop
# M: rows on the i9's list this battery ALSO sweeps (stated in the GO, like the canaries). Verify round 2: the rows whose
# execution config the union decides are DERIVED in PRE-D from the roster at $MASTER and at HEAD (at the 24-row list:
# internal/godebug and net/http drop release-tiered, TC0; log/slog keeps it) and join the S run list by themselves;
# X_ROWS holds only what a derivation cannot name: encoding/json (RULED: read on two boxes; one of the 17 rows of G's
# line-attribution A/B, and not a canary).
# ONE RULING, ITS SITES (which rows carry an execution config: log/slog alone, tL-seats-draft.txt:31, :63): the ruled
# set is tM-helpers.py EXEC_RULED, which precheck asserts against the roster by identity; EXEC_ROWS_EXPECT below defaults
# to its count; TC0_ROWS and TIERED_ROWS are derived from the roster; tM-linux-legs.sh and tM-i9-shard.sh read the roster
# of the union they run on. If the ruling moves, EXEC_RULED is the one edit.
X_ROWS='encoding/json'
TC0_ROWS=''                              # derived in PRE-D: annotated at $MASTER, not at HEAD -> must read tiered=False in their own record
TIERED_ROWS=''                           # derived in PRE-D: annotated at HEAD -> must read tiered=True
L_SUMS="/h/go2cs-tmp-coord/coord-scratch/tL/run2/tL-logs/SUMMARY.txt"                 # TL-WALL: TRAIN L's battery of record
L_REWRITES=/h/go2cs-tmp-coord/coord-scratch/tL/run2/tL-logs/S-rewrites-U0.patch       # TE baseline: L's sweep rewrites, same box
L_REWRITES_T=/h/go2cs-tmp-coord/coord-scratch/tL/run2/tL-logs/T-rewrites-U0.patch     # TE-T baseline: L's T-leg rewrites
PRECHECK_MODE=${PRECHECK_MODE:-abort}   # 'warn' runs on past a failed PRE assert (COORD's explicit call, stated in SUMMARY)
DEADLINE=${DEADLINE:-}                  # R5: no leg STARTS after this ABSOLUTE local time 'YYYY-MM-DD HH:MM'; unset = launch + 14 h (verify round 2, RULED)
SWEEP_ROW_CAP=${SWEEP_ROW_CAP:-4h}      # outer wall cap per sweep row (critic 1 #7); the sweep's own per-row timeouts fire first
I9_PATCH=${I9_PATCH:-}                  # the i9's tracked-changes-U0.patch, when it has arrived (critic 0 #5)
I9_BASELINE=${I9_BASELINE:-}            # optional: the i9's TRAIN L shard patch, as the TE-i9 baseline
EXEC_ROWS_EXPECT=${EXEC_ROWS_EXPECT:-}  # rows carrying an execution config; unset = the count of tM-helpers.py EXEC_RULED (see the note above X_ROWS)
# The seats bisected if leg E reads above 0. The default is the two seats with a KNOWN uncommitted corpus footprint (which
# seat's emission the tree lacks is what leg E measures: it cannot be derived from git). Verify round 2 (R1): the list is
# asserted in PRE-D against the DERIVED superset, the seats whose own merge changes a non-test Go source of the converter;
# a named seat outside it is a finding, and the superset's other members are stamped by name (each arm is about 46 m,
# so they are COORD's to add: E_BISECT_SEATS='a b c').
E_BISECT_KNOWN='g-godebug-pc-line p2-func-literal-result-type'
E_BISECT_SEATS=${E_BISECT_SEATS:-$E_BISECT_KNOWN}
LOCK=/h/go2cs-tmp-coord/coord-scratch/tM/.battery.lock
LOGDIR=$SD/tM-logs
SUM="$LOGDIR/SUMMARY.txt"
# Run-copy checks BEFORE anything is written (critic 1 #4 and #9): a folder that already holds a SUMMARY would mix two
# runs' lines into NONZERO LEGS, wallcmp and the MOD stamp; a launch from a worktree or a draft folder breaks floor 4;
# a missing companion file would surface hours in as rc 127.
[ -e "$SUM" ] && { echo "ABORT: $SUM exists -- use a fresh per-run copy"; exit 2; }
[ -e "$SD/mod-logs/SUMMARY.txt" ] && { echo "ABORT: $SD/mod-logs/SUMMARY.txt exists -- use a fresh per-run copy"; exit 2; }
case "$SD" in
  /h/go2cs-tmp-coord/tM|/h/go2cs-tmp-coord/tM/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
for f in tM-emitcheck.sh emitdrift.py tM-helpers.py tM-modules-legs.sh tM-i9-te.sh tM-ng-parse.ps1 tM-follow.txt tM-i7-sweeps.txt tM-i9-shard.txt; do
  [ -f "$SD/$f" ] || { echo "ABORT: $SD/$f missing from the run copy"; exit 2; }
done
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS): copy trainM/tM-seats-draft.txt into the run folder"; exit 2; }
# Verify round 2 (RULED): the deadline is resolved ONCE, to epoch seconds, before anything else runs.
T_LAUNCH=$(date +%s)
if [ -z "$DEADLINE" ]; then
  DL_EPOCH=$((T_LAUNCH + 14 * 3600)); DEADLINE=$(date -d "@$DL_EPOCH" '+%Y-%m-%d %H:%M'); DL_SRC='the default: launch + 14 h'
else
  [[ "$DEADLINE" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}\ ([01][0-9]|2[0-3]):[0-5][0-9]$ ]] || { echo "ABORT: DEADLINE '$DEADLINE' is not 'YYYY-MM-DD HH:MM' (local time; the same-day HH:MM form is gone)"; exit 2; }
  DL_EPOCH=$(date -d "$DEADLINE" +%s 2>/dev/null) || { echo "ABORT: DEADLINE '$DEADLINE' is not a date"; exit 2; }
  [ "$DL_EPOCH" -gt "$T_LAUNCH" ] || { echo "ABORT: DEADLINE '$DEADLINE' is already in the past (now $(date '+%Y-%m-%d %H:%M'))"; exit 2; }
  DL_SRC='set by COORD'
fi
[ -x /usr/bin/timeout ] || { echo "ABORT: no /usr/bin/timeout (the outer wall caps need coreutils timeout, not timeout.exe)"; exit 2; }
# Verify round 1 (floor 1 ACROSS trains): the lock below is TRAIN M's own; TRAIN L's battery takes another one and
# tL-postmerge2.sh takes none. tM-helpers.py live reads the other trains' locks, the converter and harness processes
# (twice, 20 s apart; listed, never killed: floor 5) and any run log still being written. LIVE_ACK=1 runs past a hit by
# COORD's explicit call; the reading is stamped in PRE either way.
if [ "${LIVE_ACK:-0}" = 1 ]; then
  LIVEV='ACKNOWLEDGED by COORD (LIVE_ACK=1): not read'
else
  lv=$(python -B "$(cygpath -w "$SD/tM-helpers.py")" live "$(cygpath -w "$LOCK")" 2>&1); lrc=$?
  if [ "$lrc" != 0 ]; then
    echo "ABORT (floor 1): another conversion, harness or battery looks alive on this box (listed, not killed; LIVE_ACK=1 runs past it by COORD's explicit call):"
    printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE' | head -n 12
    exit 2
  fi
  LIVEV=$(printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE-VERDICT' | cut -c1-200)
fi
# One battery at a time (floors 1 and 11): an atomic mkdir lock, released by the EXIT trap below.
mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- another TRAIN M battery, a standalone tM-modules-legs.sh or tM-fixup.sh holds it; rmdir it only after checking"; exit 2; }
POLLPID=''
FXC=''   # leg FXc's scratch (a GOROOT copy): the trap removes it on ANY exit (deadline stop, disk ABORT, timeout stop)
cleanup(){ local rc=$?; [ -n "$POLLPID" ] && kill "$POLLPID" 2>/dev/null; [ -n "$FXC" ] && rm -rf "$FXC" 2>/dev/null; if [ "${KEEP_LOCK:-0}" = 1 ]; then echo "$(date '+%H:%M:%S') lock KEPT: rmdir $LOCK after the listed PIDs are killed" | tee -a "$SUM"; else rmdir "$LOCK" 2>/dev/null; fi; exit $rc; }
trap cleanup EXIT
mkdir -p "$LOGDIR"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
FINDINGS=''; NFIND=0
finding(){ stamp "  FINDING: $*"; FINDINGS="$FINDINGS[$1] "; NFIND=$((NFIND + 1)); }   # a gate that read wrong without a nonzero rc; listed at END (and, verify round 2, counted into the exit status)
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
hp(){ python -B "$(cygpath -w "$SD/tM-helpers.py")" "$@"; }
# Verify round 2: EXEC_ROWS_EXPECT's default is the count of the helper's EXEC_RULED (the one site of that ruling), and
# the two lists this run reads are hashed for the PRE line (per-run-folder copies: tM-fixup.sh stamps the same pair on
# its PRE and FIXUP DONE lines, so a battery launched from a folder whose tM-follow.txt is not the fixup's shows it).
[ -n "$EXEC_ROWS_EXPECT" ] || EXEC_ROWS_EXPECT=$(hp execruled 2>/dev/null | tr -d '\r' | sed -n 's/^EXECRULED n=\([0-9][0-9]*\).*/\1/p')
[ -n "$EXEC_ROWS_EXPECT" ] || { stamp "ABORT: tM-helpers.py execruled printed no count (EXEC_ROWS_EXPECT cannot be derived)"; exit 2; }
rowsha(){ grep -E '^[A-Za-z0-9._-]+\|' "$1" | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12; }
followsha(){ grep -vE '^[[:space:]]*(#|$)' "$1" | tr -d '\r' | sha256sum | cut -c1-12; }
LISTS="lists=rows:$(rowsha "$SEATS")/follow:$(followsha "$FOLLOWF")"

# ================================================================================================= ENV (K, + the GOROOT checks)
ORIGPATH="$PATH"
GOPIN_PATH="$HOME/sdk/go1.24.13/bin:$ORIGPATH"   # critic 1 #12: every leg that drops dotnet10 from PATH keeps the pinned go first
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
# GOROOT stays on C: in its BACKSLASH spelling (floor 6): GOROOT on ReFS H: deadlocks runtime's TestTracebackSystem
# panic child (2026-09-30), and a forward-slash spelling misroutes the emission (and, since r-m6, also breaks the
# stdLibImportPathOf exclusion that keeps stdlib hosts unstamped).
# Critic 1 #5: checked on the VALUE before the export (a `go env GOROOT` after the export only echoes the env back).
# The expected spelling is derived from $HOME, never written out (no user-profile path in a pushed script).
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) echo "ABORT: GOROOT spelling '$GR' has a forward slash (floor 6)"; exit 2 ;; esac
[[ "$GR" =~ ^[A-Za-z]:[\\].*[\\]sdk[\\]go1\.24\.13$ ]] || { echo "ABORT: GOROOT spelling '$GR' is not a drive-rooted backslash path ending sdk<backslash>go1.24.13"; exit 2; }
[ "$GR" = "$(cygpath -w "$HOME")\\sdk\\go1.24.13" ] || { echo "ABORT: GOROOT spelling '$GR' is not <profile>\\sdk\\go1.24.13"; exit 2; }
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || { echo "ABORT: $HOME/sdk/go1.24.13/VERSION is not go1.24.13"; exit 2; }
GR_TC=$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')
[ "$GR_TC" = "$GR" ] || { echo "ABORT: the toolchain's own GOROOT spelling '$GR_TC' != '$GR'"; exit 2; }
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { echo "ABORT: go on PATH is $(go version), not go1.24.13"; exit 2; }
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
# Keep the system drive out of the battery (2026-09-27: C: filled to 0 bytes mid-run): temp, Go cache, NuGet on H:.
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
# r-m4: GO2CS_MODULE_ROOT must NOT be ambient -- the converter sets it per host child; an inherited value would mask a
# converter that stopped setting it. (GOTOOLCHAIN=local above is also JunctionGodebugToolchainChildTests' precondition.)
unset GO2CS_MODULE_ROOT
PWSH=$HOME/.dotnet/tools/pwsh
LOGW=$(cygpath -w "$LOGDIR")
WW=$(cygpath -w "$W")

# ================================================================================================= PRE (no builds)
cd "$W" || { stamp "ABORT: no worktree $W"; exit 2; }
h=$(git rev-parse --short=10 HEAD)
[ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
subj=$(git log -1 --format=%s HEAD)
# Verify round 2 (floor 9, RULED): the head of the union is 'fixup: TRAIN M' followed by ZERO OR MORE 'fixup-N: TRAIN M'
# single-parent commits, in order (fixup-2 on the fixup, fixup-3 on fixup-2, ...), each signed, and nothing else
# single-parent. A red that needs a tree change after the union was pushed lands as a commit ON TOP, never as a replaced
# sha. NFU = how many; UTOP = the newest two-parent commit (the last seat or follow-up merge) under them.
NFU=0; t=HEAD
while [ "$(git rev-list --parents -n 1 "$t" | wc -w)" = 2 ] && [ "$NFU" -le 9 ]; do NFU=$((NFU + 1)); t="$t^"; done
[ "$NFU" -ge 1 ] && [ "$NFU" -le 9 ] || { stamp "ABORT: HEAD carries $NFU single-parent commit(s) above the merges (want 1 to 9: 'fixup: TRAIN M', then 'fixup-2', ...); subject of HEAD: ${subj:0:120}"; exit 2; }
k=$NFU; c=HEAD; FUSUBJ=''
while [ "$k" -ge 1 ]; do
  want='fixup: TRAIN M'; [ "$k" = 1 ] || want="fixup-$k: TRAIN M"
  cs=$(git log -1 --format=%s "$c")
  case "$cs" in "$want"*) ;; *) stamp "ABORT: the single-parent commit $(git rev-parse --short=10 "$c") is not '$want...' (subject: ${cs:0:120}): the head must be 'fixup: TRAIN M' then 'fixup-2: TRAIN M', ... in order"; exit 2 ;; esac
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) stamp "ABORT: the fixup commit $(git rev-parse --short=10 "$c") is not signed (want G or U)"; exit 2 ;; esac
  FUSUBJ="$(git rev-parse --short=10 "$c") ${cs:0:60} | $FUSUBJ"
  k=$((k - 1)); c="$c^"
done
UTOP=$(git rev-parse "$t")
# (the GOROOT pin was checked on its value before the export, ENV above)
git merge-base --is-ancestor "$MASTER" HEAD || { stamp "ABORT: master $MASTER is not an ancestor"; exit 2; }
[ "$(git rev-parse "$MASTER^{commit}")" = "$(git rev-parse 'aa0a07d5fd^{commit}')" ] || stamp "NOTE: MASTER=$MASTER is not TRAIN L's landed master aa0a07d5fd, the base tM-assemble.sh and tM-fixup.sh name (COORD's explicit call)"
ns=0
while IFS='|' read -r b sha msg; do
  ns=$((ns + 1)); git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
# M: the seat count is READ from the list (L wrote 11 here and again in its fixup). What makes the count mean something
# is the first-parent arithmetic below: one merge per row, plus the ruled follow-ups, plus the fixup. SEATS_EXPECTED, when
# COORD passes the GO's number, is a second derivation. L's 'REPLACED originals' assert (two onK re-cuts) left with L.
[ "$ns" -ge 1 ] || { stamp "ABORT: the seat list $SEATS holds no seat row"; exit 2; }
[ -z "$SEATS_EXPECTED" ] || [ "$ns" = "$SEATS_EXPECTED" ] || { stamp "ABORT: the seat list has $ns seats, SEATS_EXPECTED says $SEATS_EXPECTED"; exit 2; }
# A row that declares 'stack-on <ref>' must have that ref's seated sha as an ancestor (the seat table's own rule).
while IFS='|' read -r b sha msg; do
  for so in $(printf '%s' "$msg" | grep -oE 'stack-on [A-Za-z0-9._-]+' | cut -d' ' -f2); do
    ssha=$(grep -E "^$so\|" "$SEATS" | head -n 1 | cut -d'|' -f2)
    [ -n "$ssha" ] && git merge-base --is-ancestor "$ssha" "$sha" || { stamp "ABORT: seat $b declares stack-on $so, and that row's sha '${ssha:-absent}' is not its ancestor"; exit 2; }
  done
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
fsig=$(git log -1 --format=%G? HEAD)
case "$fsig" in G|U) ;; *) stamp "ABORT: the fixup HEAD signature reads '$fsig' (want G or U)"; exit 2 ;; esac
SEATFULL=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | cut -d'|' -f2 | while read -r s; do git rev-parse "$s^{commit}"; done)
# FOLLOW-UP SEAT MERGES (the mechanism is TRAIN L's): a seat's own follow-up may ride as ONE extra merge each, after that
# seat's merge and before the fixup, when its second parent is on the entry's origin branch, descends from the entry's
# seated sha and is not it. The entries are read from tM-follow.txt, the SAME file tM-fixup.sh reads: EMPTY, and none
# is owed at the frozen list (hazard H4 was resolved inside its seat, row p2-test-overload-references e002a552a8).
FOLLOW=$(grep -vE '^[[:space:]]*(#|$)' "$FOLLOWF" | tr -d '\r' | tr '\n' ' ')
mbad=0; mcontain=0; munsigned=0; nfix=0; FIXES=''
for c in $(git rev-list --first-parent "$MASTER..$UTOP"); do
  [ "$(git rev-list --parents -n 1 "$c" | wc -w)" = 3 ] || { stamp "  PRE union: $(git rev-parse --short=10 "$c") is not a two-parent merge"; mbad=$((mbad + 1)); continue; }
  p2=$(git rev-parse "$c^2")
  if echo "$SEATFULL" | grep -qxF "$p2"; then :
  else
    fx=''
    for fr in $FOLLOW; do
      fref=refs/remotes/origin/claude/${fr%%:*}; fseat=$(git rev-parse "${fr##*:}^{commit}")
      git rev-parse --verify -q "$fref" > /dev/null || continue
      if [ "$p2" != "$fseat" ] && git merge-base --is-ancestor "$p2" "$fref" && git merge-base --is-ancestor "$fseat" "$p2" && git merge-base --is-ancestor "$fseat" "$c^1"; then fx=${fr%%:*}; break; fi
    done
    if [ -n "$fx" ]; then
      nfix=$((nfix + 1)); FIXES="$FIXES $fx:$(git rev-parse --short=10 "$p2")"
      stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") is $fx's FOLLOW-UP (on origin/claude/$fx, descends from its seat, seated after it): $(git log -1 --format=%s "$p2" | cut -c1-120)"
    elif echo "$SEATFULL" | while read -r s; do git merge-base --is-ancestor "$s" "$p2" && ! git merge-base --is-ancestor "$s" "$c^1" && echo y; done | grep -q y; then
      mcontain=$((mcontain + 1)); stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") CONTAINS a listed seat (not the seat sha itself)"
    else
      stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") is NOT a listed seat or a ruled follow-up (if it is one: add <ref>:<seat sha> to THIS run folder's tM-follow.txt -- the fixup's run folder holds the entry -- and fetch origin/claude/<ref>; this run read $LISTS)"; mbad=$((mbad + 1))
    fi
  fi
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) munsigned=$((munsigned + 1)) ;; esac
done
[ "$mbad" = 0 ] || { stamp "ABORT: $mbad first-parent commit(s) on the union are not seat merges"; exit 2; }
fpc=$(git rev-list --first-parent --count "$MASTER..HEAD")
[ "$fpc" = $((ns + nfix + NFU)) ] || { stamp "ABORT: union first-parent count $fpc != seats $ns + follow-ups $nfix + $NFU fixup commit(s) -- a stray or missing commit on the union"; exit 2; }
SEATS_EFF="$LOGDIR/seats-effective.txt"
# Verify round 3: built as tM-fixup.sh builds it -- the ROW lines only, CR-stripped, each terminated, then the follow-up
# rows -- and the row count asserted. `cat "$SEATS"` welded the first follow-up row onto the last line of a list that
# ends without a newline (a comment, or a seat's notes), and precheck then read that follow-up as nobody's.
{ grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r'; for f in $FIXES; do echo "${f%%:*}-followup|${f##*:}|ruled follow-up merge of ${f%%:*} (origin/claude/${f%%:*})"; done; } > "$SEATS_EFF"
[ "$(grep -cE '^[A-Za-z0-9._-]+\|' "$SEATS_EFF")" = $((ns + nfix)) ] || { stamp "ABORT: seats-effective.txt holds another row count than $ns seat rows + $nfix follow-up row(s)"; exit 2; }
stamp "PRE union shape OK: first-parent=$fpc (= $ns seat merges + $nfix follow-up merge(s):$FIXES + $NFU fixup commit(s): ${FUSUBJ% | }), ^2-contains-seat=$mcontain, fixup signed=$fsig, unsigned seat merges=$munsigned (stated, not gated)"
[ "$NFU" = 1 ] || stamp "PRE NOTE: $((NFU - 1)) fixup commit(s) ON TOP of 'fixup: TRAIN M' (floor 9: the pushed sha was kept). This battery reads the tree at HEAD; which legs a fixup-N obliges COORD to re-read on the lanes is tM-README.md section 4b, by the paths it touches: $(git diff --name-only "$(git rev-parse "HEAD~$((NFU - 1))")" HEAD | head -n 8 | tr '\n' ' ')"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before the battery"; exit 2; }
dv=$(dotnet --version 2>&1); dvrc=$?
case "$dvrc:$dv" in 0:10.0.*) ;; *) stamp "ABORT: dotnet under global.json rc=$dvrc: $(echo "$dv" | tr '\r\n' '  ' | cut -c1-200)"; exit 2 ;; esac
stamp "PRE head=$h (fixup: ${subj:0:90}) parent=$(git rev-parse --short=10 HEAD^) tree=$(git rev-parse --short=10 HEAD^{tree}) signed=$fsig master=$MASTER seats=$ns ($(basename "$SEATS")) $LISTS $(go version) dotnet=$(echo "$dv" | tr -d '\r') free=$(freegb)G C:=$(cfree)G GO2CS_MODULE_ROOT=${GO2CS_MODULE_ROOT:-unset}"

# ================================================================================================= PRE-D (verify round 2, ruling R1)
# Every list a leg reads BY NAME is derived here from git between $MASTER and HEAD, written to tM-logs/derived-*.txt and
# stamped, so the SUMMARY shows what this run read. Nothing below is typed from a seat list: a row accepted after this
# draft (the list grew from 15 to 24 rows in four hours) is read by name without an edit. What cannot be derived stays a
# literal chosen by a hazard, and is checked against the tree: a miss is a finding BY NAME (it does not stop PRE).
dfind(){ finding "PRE-D ${1%%|*}: ${1#*|}"; }   # 'label|text': a literal the tree does not bear out, listed at END like any other finding
# (1) leg CB -- the converter and repoguard tests the union ADDS (a '+func Test...(' line in the diff of src/go2cs: a new
#     test, or one moved or renamed), plus six tree guards that exist at the base and that leg C (no -v) would read 'ok'
#     on a skip: the stdlib-metadata guard and the five projitems tests. At the frozen list's map union 11c188daf3: 73 added.
CB_BASE='TestStdLibMetadataInSync TestProjitemsRegistersEveryGoSource TestProjitemsHasNoDanglingEntries TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings TestProjitemsRegistrationClassifierFires TestProjitemsInsertionHintTakesTheNearestPredecessor'
CB_ADDED=$(git diff "$MASTER" HEAD -- src/go2cs | sed -n 's/^+func \(Test[A-Za-z0-9_]*\)(.*/\1/p' | LC_ALL=C sort -u | tr '\n' ' ')
CB_TESTS=$(printf '%s\n' $CB_ADDED $CB_BASE | LC_ALL=C sort -u | tr '\n' ' ')
printf '%s\n' $CB_TESTS > "$LOGDIR/derived-cb-tests.txt"
for t in $CB_BASE; do git grep -q "^func $t(" HEAD -- src/go2cs || dfind "CB_BASE|the literal CB_BASE names $t and no 'func $t(' is in src/go2cs at HEAD"; done
stamp "PRE-D CB: $(echo $CB_ADDED | wc -w) test(s) the union adds under src/go2cs + $(echo $CB_BASE | wc -w) base tree guards = $(echo $CB_TESTS | wc -w) names (derived-cb-tests.txt): $(echo $CB_ADDED | cut -c1-900)"
# (2) leg GT -- the GolibTests classes the union ADDS or CHANGES (the file name is the class name: true of every class
#     the union adds), minus the classes this box does not compile (GolibTests.csproj removes them unless GoTargetOS is
#     linux: read from the csproj at HEAD, never listed), plus the hazard-H3 neighbours and the testing-host classes, a
#     literal. A neighbour with no file at HEAD is a finding.
GT_NEIGH='TestGoroutineCreatorTests CreatedByPositionTests TracebackDecorationTests SkipCountedWalkerFrameTests RuntimeCallerPCSpanTests SyntheticPCRegistryTests MainGoroutineIdentityTests ForeverWaitDeadlockDecisionTests GoroutineProfileInstantTests GoroutineProfileTests GoroutineParkAccountingTests RuntimeParkTransitionTests GoroutineReadyTests GoroutineFatalHostTests BubbledChannelTests ChannelWakeupStrainTests SyncMutexProfileTests NoUncountedBackingAllocationsTests ModuleAncestryTests FixtureLinkStagingTests'
GT_TESTS='ForeverWaitDeadlockDecisionTests.ResidualAnOrdinaryChannelOrSyncWaitBlocksInsteadOfReporting SkipCountedWalkerFrameTests.EveryHandOwnedSkipCountedWalkerKeepsItsFrame'
GT_ADDED=$(git diff --name-only --diff-filter=A "$MASTER" HEAD -- 'src/tests/GolibTests/*Tests.cs' | sed 's#.*/##; s#\.cs$##' | LC_ALL=C sort -u | tr '\n' ' ')
GT_CHANGED=$(git diff --name-only --diff-filter=M "$MASTER" HEAD -- 'src/tests/GolibTests/*Tests.cs' | sed 's#.*/##; s#\.cs$##' | LC_ALL=C sort -u | tr '\n' ' ')
GT_OFFBOX=$(git show HEAD:src/tests/GolibTests/GolibTests.csproj | tr -d '\r' | awk '/<ItemGroup/ { c = $0 } /<\/ItemGroup>/ { c = "" } /<Compile Remove="/ { if (c ~ /!= .linux./) { f = $0; sub(/.*Remove="/, "", f); sub(/".*/, "", f); sub(/\.cs$/, "", f); print f } }' | tr '\n' ' ')
GT_CLASSES=''; GT_EXPECT=''; GT_SKIPPED=''
for c in $(printf '%s\n' $GT_ADDED $GT_CHANGED $GT_NEIGH | LC_ALL=C sort -u); do
  case " $GT_OFFBOX " in *" $c "*) GT_SKIPPED="$GT_SKIPPED$c "; continue ;; esac
  GT_CLASSES="$GT_CLASSES$c "
done
for c in $GT_ADDED; do   # the added classes' floors: uncommented [TestMethod] lines of the class file at HEAD (a DataRow adds results)
  case " $GT_OFFBOX " in *" $c "*) continue ;; esac
  GT_EXPECT="$GT_EXPECT$c=$(git show "HEAD:src/tests/GolibTests/$c.cs" | grep -cE '^[[:space:]]*\[(Data)?TestMethod') "
done
for c in $GT_NEIGH; do git cat-file -e "HEAD:src/tests/GolibTests/$c.cs" 2>/dev/null || dfind "GT_NEIGH|the literal GT_NEIGH names $c and src/tests/GolibTests/$c.cs is not in the tree at HEAD"; done
for x in $GT_TESTS; do git grep -q "void ${x##*.}(" HEAD -- "src/tests/GolibTests/${x%%.*}.cs" || dfind "GT_TESTS|the literal GT_TESTS names $x and that method is not in the class file at HEAD"; done
printf '%s\n' $GT_CLASSES > "$LOGDIR/derived-gt-classes.txt"
stamp "PRE-D GT: added=[${GT_ADDED% }] changed=[${GT_CHANGED% }] + $(echo $GT_NEIGH | wc -w) literal neighbours = $(echo $GT_CLASSES | wc -w) classes read by name (derived-gt-classes.txt); not compiled on this box (csproj, GoTargetOS != linux) and so the linux lanes': [${GT_SKIPPED% }]; floors of the added classes: [${GT_EXPECT% }]"
# (3) legs 4 and 5 -- the behavioral projects the union ADDS: top-level directories with an added csproj directly in
#     them (second derivation: the go2cs.slnx Project lines the union adds under tests/Behavioral; a difference is a
#     finding), and the projects whose goldens the fixup commit(s) changed (leg 5 reads each isolated). The checkdead
#     guards and the neighbours are literals (hazard H3; TRAIN L's five and its eight): each must be a directory at HEAD.
M7DIRS=$(git diff --name-only --diff-filter=A "$MASTER" HEAD -- src/tests/Behavioral | grep -E '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | cut -d/ -f4 | grep -vxE 'BehavioralTests|BehavioralRunner' | LC_ALL=C sort -u | tr '\n' ' ')
M7SLNX=$(git diff "$MASTER" HEAD -- src/go2cs.slnx | sed -n 's#^+.*<Project Path="tests/Behavioral/\([^/"]*\)/[^/"]*\.csproj".*#\1#p' | LC_ALL=C sort -u | tr '\n' ' ')
[ "$M7DIRS" = "$M7SLNX" ] || dfind "M7DIRS|behavioral projects the union adds, by tree [${M7DIRS% }] != by go2cs.slnx lines [${M7SLNX% }] (a project added without its registration, or the reverse)"
# Verify round 3 (RULED): only directories that are behavioral PROJECTS (a csproj directly in them at HEAD). Field 4 of
# every changed path took the NAME of a file directly in the behavioral root (run-behavioral.ps1, _paths.ps1, ...) as a
# 'project'; its isolated leg then exits 2 ('No behavioral projects matched'), a non-zero leg and exit 6 on a green tree
# (reachable through a hand-made fixup-N). A changed directory that holds no csproj is stamped, never read as a project.
FXGOLD=''; FXNOPROJ=''
for d in $(git diff --name-only "$UTOP" HEAD -- src/tests/Behavioral | grep -E '^src/tests/Behavioral/[^/]+/' | cut -d/ -f4 | grep -vxE 'BehavioralTests|BehavioralRunner' | LC_ALL=C sort -u); do
  if git ls-tree --name-only HEAD "src/tests/Behavioral/$d/" | grep -q '\.csproj$'; then FXGOLD="$FXGOLD$d "; else FXNOPROJ="$FXNOPROJ$d "; fi
done
CHECKDEAD_GUARDS='ForeverWaitWorkersMainReturns MainSelectForeverWorkerExits MainSelectForeverAfterFunc ChannelReceiveFromNil ChannelSendToNil'
NEIGH='NilChannelInSelect NilChannelSelectDefault CloseWakesBlocked SelectSendDefault ChanDirectionChain ChannelCapLen StdoutCloseEofBarrier PointerEmbedValueChainPromotion'
for g in $CHECKDEAD_GUARDS $NEIGH; do git cat-file -e "HEAD:src/tests/Behavioral/$g" 2>/dev/null || dfind "MGUARDS|the literal guard $g is not a directory under src/tests/Behavioral at HEAD"; done
stamp "PRE-D behavioral: projects the union adds=[${M7DIRS% }] ($(echo $M7DIRS | wc -w); slnx agrees: $([ "$M7DIRS" = "$M7SLNX" ] && echo yes || echo NO)); projects whose goldens the fixup commit(s) changed=[${FXGOLD% }]$([ -z "$FXNOPROJ" ] || echo " (directories the fixup commit(s) changed that hold no csproj, not read as projects: ${FXNOPROJ% })"); literal guards: checkdead $(echo $CHECKDEAD_GUARDS | wc -w), neighbours $(echo $NEIGH | wc -w)"
# (4) leg S -- the rows whose execution config the union decides, from the roster itself: annotated at HEAD = must read
#     tiered=True; annotated at $MASTER and not at HEAD = must read tiered=False. precheck (PRE-1) asserts the HEAD set
#     against the ruling by identity, so a wrong roster stops there; these two lists follow the roster.
hp execrows "$WW" "$MASTER" > "$LOGDIR/execrows-master.txt" 2>&1; xr1=$?
hp execrows "$WW" HEAD > "$LOGDIR/execrows-head.txt" 2>&1; xr2=$?
[ "$xr1" = 0 ] && [ "$xr2" = 0 ] || { stamp "ABORT: tM-helpers.py execrows rc=$xr1/$xr2 (the roster did not read: execrows-master.txt / execrows-head.txt)"; exit 2; }
TIERED_ROWS=$(tr -d '\r' < "$LOGDIR/execrows-head.txt" | sed -n 's/^EXECROW \([^ ]*\) .*/\1/p' | LC_ALL=C sort -u | tr '\n' ' '); TIERED_ROWS=${TIERED_ROWS% }
TC0_ROWS=$(tr -d '\r' < "$LOGDIR/execrows-master.txt" | sed -n 's/^EXECROW \([^ ]*\) .*/\1/p' | LC_ALL=C sort -u | grep -vxF -f <(printf '%s\n' $TIERED_ROWS '#none#') | tr '\n' ' '); TC0_ROWS=${TC0_ROWS% }
stamp "PRE-D execution configs (roster at $MASTER vs HEAD): rows that keep an annotation (EXPECT tiered=True)=[$TIERED_ROWS] rows that drop it (EXPECT tiered=False)=[$TC0_ROWS]; EXEC_ROWS_EXPECT=$EXEC_ROWS_EXPECT; all of them and X_ROWS=[$X_ROWS] join the S run list"
# (5) E-bisect -- the superset: seats whose own merge on the union changes a non-test Go source of the converter.
#     union-merges.txt = '<second parent> <merge>' for every merge on the union's first-parent line (leg E-bisect reads it too).
for c in $(git rev-list --first-parent "$MASTER..$UTOP"); do echo "$(git rev-parse "$c^2") $c"; done > "$LOGDIR/union-merges.txt"
CONVSEATS=''
while IFS='|' read -r b sha msg; do
  bf=$(git rev-parse "$sha^{commit}"); bm=$(awk -v s="$bf" '$1 == s { print $2 }' "$LOGDIR/union-merges.txt" | head -n 1)
  [ -n "$bm" ] && [ "$(git diff --name-only "$bm^1" "$bm" -- 'src/go2cs/*.go' | grep -vc '_test\.go$')" != 0 ] && CONVSEATS="$CONVSEATS$b "
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
ebx=''; for s in $E_BISECT_SEATS; do case " $CONVSEATS " in *" $s "*) ;; *) ebx="$ebx$s "; dfind "E_BISECT|E_BISECT_SEATS names $s, which is not a seat whose merge changes the converter (not a row, or no non-test .go under src/go2cs in its merge)" ;; esac; done
ebrest=$(for s in $CONVSEATS; do case " $E_BISECT_SEATS " in *" $s "*) ;; *) printf '%s ' "$s" ;; esac; done)
stamp "PRE-D E-bisect: seats whose merge changes the converter=[${CONVSEATS% }] ($(echo $CONVSEATS | wc -w)); bisected if leg E reads above 0: [$E_BISECT_SEATS]; converter seats NOT in that list (COORD adds them with E_BISECT_SEATS, about 46 m an arm): [${ebrest% }]"

# PRE-1: tM-helpers.py precheck at HEAD. COUNT: the six registration files = base + each seat's OWN net inserts + the
# fixup's own (measured on the pre-map union 8d7305053f: projitems 425 -> 432, go2cs.slnx 1069 -> 1076, CompileTests /
# TargetComparisonTests / TranspileTests +21 each, OutputComparisonTests +18; the fixup is expected to add 0 to them).
# REG: every base key and every seat-added key is registered once. BOTH (per merge since verify round 1): in every
# merge on the union's first-parent line, each path both sides changed keeps both sides' lines, none lost, none back,
# none duplicated (the roster: G's two rows AND master's TRAIN L provenance block). ROSTER: no row's N + D
# moved, and log/slog alone carries an execution config. H3: Goroutine.cs holds checkdead's and the entry classification's identifiers. H4: no Go-only behavioral
# directory. S1 census, optin.py check (CHECK PASS), no conflict markers. The same arms ran in the fixup's step 6 on
# the tree it was about to commit, so this battery is launched in PRECHECK_MODE=abort.
stamp "PRE GOROOT verified before export: spelling=toolchain's own, VERSION go1.24.13, no forward slash; DEADLINE='$DEADLINE' ($DL_SRC; epoch $DL_EPOCH) SWEEP_ROW_CAP=$SWEEP_ROW_CAP lock=$LOCK GOMODCACHE(go env)=$(go env GOMODCACHE | tr -d '\r') :: LIVE gate before the lock: ${LIVEV:-unread}"
hp precheck "$WW" "$MASTER" "$(cygpath -w "$SEATS_EFF")" head > "$LOGDIR/precheck.log" 2>&1; prc=$?
stamp "PRE-1 precheck rc=$prc :: $(grep -a '^PRECHECK' "$LOGDIR/precheck.log") :: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 4 | tr '\n' ' ' | cut -c1-500)"
grep -aE '^ok +(COUNT|REG|BOTH:|ROSTER|H3|H4)' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  PRE-1 ${l:0:230}"; done   # BOTH: its summary line (one ok line per merged path is in the log)
grep -a '^NOTE' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  PRE-1 ${l:0:300}"; done
# PRE-1b: the rule leg C's TestOutputComparisonListMatchesConsoleOutputAttribute enforces, read from the tree: 0
# attribute-not-listed, 0 listed-no-attribute, 0 duplicates. The M seats register 7 projects in four files and 6 in
# OutputComparisonTests.cs (AliasStructToInterfaceLib is a library); the pre-map union reads 724 == 724.
hp outparity "$WW" > "$LOGDIR/outparity.log" 2>&1; oprc=$?
stamp "PRE-1b outparity rc=$oprc :: $(head -n 1 "$LOGDIR/outparity.log" | tr -d '\r')"
[ "$oprc" = 0 ] || prc=$((prc + 10))
# PRE-1c: the host censuses at HEAD (TRAIN L's r-m4 and r-m6, now on master: a REGRESSION reading for M, whose F4 and F8
# edit testConversion.go): no committed stdlib host carries a module-path argument; GoDefaultGodebug is named by exactly
# 3 corpus files (measured the same 3 at aa0a07d5fd and at the pre-map union). Re-read after the sweeps (leg HS).
hs_m4(){ git grep -lE '\}, "[^"]+"\);' "$@" -- 'src/core/*go2cs_test_host.cs' | wc -l; }
hs_m6(){ git grep -l GoDefaultGodebug "$@" -- src/core | sed 's/^HEAD://' | LC_ALL=C sort | tr '\n' ' '; }
HS_WANT='src/core/golib/GoDefaultGodebugAttribute.cs src/core/internal/godebug/godebug.cs src/core/runtime/goenvs_impl.cs '
hs0a=$(hs_m4 HEAD); hs0b=$(hs_m6 HEAD)
stamp "PRE-1c host census at HEAD: module-path hosts=$hs0a (want 0) GoDefaultGodebug files=[${hs0b% }] ($([ "$hs0b" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'NOT the measured 3'))"
[ "$hs0a" = 0 ] && [ "$hs0b" = "$HS_WANT" ] || prc=$((prc + 100))
if [ "$prc" != 0 ]; then
  [ "$PRECHECK_MODE" = warn ] || { stamp "ABORT: a PRE assert failed (precheck.log / outparity.log / host census); PRECHECK_MODE=warn runs past it by COORD's explicit call"; exit 2; }
  stamp "PRE-1 FAILED (code $prc) and PRECHECK_MODE=warn: running on by COORD's explicit call"
fi

# PRE-2: sweep coverage (225 = tM-i7-sweeps.txt + tM-i9-shard.txt, disjoint; both lists are L's bytes, the roster's row
# SET is unchanged by M). PRE-3: the reflect-bridge canaries, derived now (the i9's crash-class seat changes golib's
# reflect method-set and adapter binders: bridge-touching). T_ROWS leave the S run list; X_ROWS join it.
hp coverage "$WW" "$(cygpath -w "$I7LIST")" "$(cygpath -w "$I9LIST")" > "$LOGDIR/coverage.log" 2>&1; cvrc=$?
hp canaries "$WW" > "$LOGDIR/canaries.log" 2>&1; carc=$?
RUNLIST="$LOGDIR/sweep-runlist.txt"
{ grep -vE '^\s*(#|$)' "$I7LIST"; grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2; grep -a '^CANARY ' "$LOGDIR/canaries.log" | cut -d' ' -f2; printf '%s\n' $X_ROWS $TC0_ROWS $TIERED_ROWS; } \
  | LC_ALL=C sort -u | grep -vxF -f <(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2; echo '#none#') \
  | grep -vxF -f <(printf '%s\n' $T_ROWS) > "$RUNLIST"
stamp "PRE-2 coverage rc=$cvrc :: $(grep -a '^COVERAGE' "$LOGDIR/coverage.log" | tr '\n' ' ') :: missing->i7: $(grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') extra(dropped): $(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') dup(i7+i9): $(grep -ac '^DUP ' "$LOGDIR/coverage.log")"
stamp "PRE-3 canaries rc=$carc :: $(grep -a '^CANARY' "$LOGDIR/canaries.log" | tr '\n' ' ' | cut -c1-300) :: S run list $(wc -l < "$RUNLIST") rows (T rows read by the T legs: $T_ROWS; rows added to this box's list, read here AND wherever the lists place them: X_ROWS=[$X_ROWS] + the derived execution-config rows [$TC0_ROWS $TIERED_ROWS])"
if [ "$cvrc" != 0 ] || [ "$carc" != 0 ]; then
  [ "$PRECHECK_MODE" = warn ] || { stamp "ABORT: PRE-2 coverage rc=$cvrc / PRE-3 canaries rc=$carc; PRECHECK_MODE=warn runs past it by COORD's explicit call"; exit 2; }
  stamp "PRE-2/PRE-3 FAILED (coverage rc=$cvrc canaries rc=$carc) and PRECHECK_MODE=warn: running on by COORD's explicit call"
fi
[ -s "$RUNLIST" ] || { stamp "ABORT: the sweep run list is EMPTY"; exit 2; }

past_deadline(){ [ "$(date +%s)" -ge "$DL_EPOCH" ]; }   # verify round 2: epoch seconds against the deadline resolved at launch (it survives midnight)
deadline_check(){ # leg name -- R5: refuse to START a leg after $DEADLINE; purge first, then exit 9 cleanly
  # Verifier: a stop inside the S/NR loop or between the T legs finds a sweep's tracked rewrites (or a legitimate ' D') in
  # the tree. They are saved as DEADLINE-rewrites.patch and restored FIRST, and the purge runs in its no-abort mode, so
  # the stop is always exit 9 (a deletion the restore could not undo is stamped and listed as a FINDING, never exit 3).
  past_deadline || return 0
  stamp "DEADLINE STOP before $1 (DEADLINE=$DEADLINE, now $(date '+%Y-%m-%d %H:%M')); restoring tracked rewrites (patch kept), purging, then exit 9"
  if [ "${BANK_IN_TREE:-0}" = 1 ]; then
    stamp "  DEADLINE: bank material is in the tree (a bank T leg has run): NOT restored; tracked rewrites listed in DEADLINE-tracked.txt for COORD"
    [ "${TBS_IN_TREE:-0}" = 1 ] && stamp "  DEADLINE: src/core/runtime holds TBS-W's FILTERED record (diagnostic only, never a bank reading); only src/core/runtime/pprof is bank material"
    git status --porcelain | grep -v '^??' > "$LOGDIR/DEADLINE-tracked.txt"
  else
    [ "$(git status --porcelain | grep -vc '^??')" = 0 ] || restore_paths DEADLINE
  fi
  [ -n "$FXC" ] && rm -rf "$FXC"
  purge "deadline-stop" noabort
  stamp "NONZERO LEGS so far: $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ') :: FINDINGS so far: ${FINDINGS:-none}"
  stamp "BATTERY STOPPED AT DEADLINE"
  exit 9
}
leg(){ # name, logfile-suffix, command...   (K verbatim + the R5 deadline check)
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0 f c
  deadline_check "$name"
  t0=$(date +%s); LEG_T0=$t0
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
  LEG_RC=$rc
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
purge(){ # label [noabort]   (K verbatim, + the deadline path's no-abort mode: stamp, FINDING, return -- the caller exits 9)
  local P left td
  P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  td=$(git status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  if [ "$left" != 0 ] || [ "$td" != 0 ]; then
    [ "${2:-}" = noabort ] && { finding "PURGE-$1 incomplete or tracked deletions (remaining=$left deletions=$td): no abort on the deadline path"; return 0; }
    stamp "ABORT: purge incomplete or tracked deletions"; exit 3
  fi
}
restore_paths(){ # label -- the rewrites a reading leg left: saved as a patch, then EXACTLY those paths restored (tK-reread)
  # critic 1 #11: NUL-separated names from git itself (renames, spaces and the golib ж.* paths survive), the restore's
  # rc read, and a restore that leaves tracked changes is a FINDING, never only a count.
  # M (TRAIN L's run 2, FINDING RESTORE-S: 275 paths restored, 56 LEFT): git status lists a file whose only difference
  # is its line endings, git diff --name-only does not, so the restore never named it and the T legs did not start from
  # HEAD's sources. Second pass, the remedy L's post-merge re-read measured (tL-postmerge2.sh restore_all: tracked now 0):
  # every path still tracked-modified whose diff is EMPTY once CR-at-EOL is ignored is removed and checked out again.
  # NUL-separated from git (glyph paths survive); a path with a real content difference is never touched here.
  local P="$LOGDIR/$1-rewrites.paths.z" rrc=0 left e p nz cro=0
  git diff --binary HEAD > "$LOGDIR/$1-rewrites.patch"
  git diff --name-only -z HEAD > "$P"
  if [ -s "$P" ]; then
    git restore --source=HEAD --staged --worktree --pathspec-from-file="$(cygpath -w "$P")" --pathspec-file-nul; rrc=$?
  fi
  while IFS= read -r -d '' e <&3; do
    case "${e:0:2}" in ' M'|'M '|'MM') ;; *) continue ;; esac
    p=${e:3}
    nz=$(git diff --ignore-cr-at-eol --numstat HEAD -- "$p" 2>/dev/null | awk '{ if ($1 == "-" || $2 == "-") s += 1; else s += $1 + $2 } END { print s + 0 }')   # '-' = binary: counted as a real difference
    if [ "$nz" = 0 ]; then rm -f -- "$p" && git checkout -q HEAD -- "$p" && cro=$((cro + 1)); fi
  done 3< <(git -c core.quotepath=false status --porcelain -z)
  left=$(git status --porcelain | grep -vc '^??')
  stamp "  RESTORE($1): $(tr -cd '\0' < "$P" | wc -c) paths; restore rc=$rrc; CR-only files reset: $cro; tracked now $left; deletions $(git status --porcelain | grep -c '^ D')"
  RESTORE_LEFT=$left
  [ "$rrc" = 0 ] && [ "$left" = 0 ] || finding "RESTORE-$1 rc=$rrc tracked-left=$left"
}
convbuild(){ # label -- purge removes every src/**/bin, the converter's own included (TRAIN J run 2's rc=127)
  # critic 1 #6: the build's rc is read, and a stale exe a sweep left cannot stand in for a failed build.
  local brc
  EXE="$W/src/go2cs/bin/go2cs.exe"
  rm -f "$EXE"
  ( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build-$1.log" 2>&1; brc=$?
  [ "$brc" = 0 ] && [ -x "$EXE" ] || { stamp "LEG $1 ABORT: converter build rc=$brc, exe present=$([ -x "$EXE" ] && echo yes || echo no) (conv-build-$1.log)"; exit 3; }
  stamp "BUILD converter ($1) sha=$(sha256sum "$EXE" | cut -c1-16)"
}
timeout_stop(){ # leg name -- an outer wall cap fired: list (never kill) what may be orphaned, then stop (critic 1 #7)
  # Candidates are processes whose command line names the tM worktree. The pattern is assembled inside PowerShell from
  # pieces, so the querying shell's own command line cannot match it, and the querying process is excluded (floor 5).
  # COORD kills any survivor BY PID after reading the list.
  # Verify round 2 (the test-host legs are capped now): a testhost's command line need not name the worktree, so the
  # EXECUTABLE PATH is matched too, and listed: PID, name, path, then the command line.
  local f="$LOGDIR/timeout-orphans-$(echo "$1" | tr '/:' '._').txt"
  powershell -NoProfile -Command "\$p1 = 'go2cs-tmp-coord' + [char]92 + 'tM' + [char]92; \$p2 = 'go2cs-tmp-coord' + '/' + 'tM' + '/'; Get-CimInstance Win32_Process | Where-Object { \$_.ProcessId -ne \$PID -and ((\$_.CommandLine -and (\$_.CommandLine.Contains(\$p1) -or \$_.CommandLine.Contains(\$p2))) -or (\$_.ExecutablePath -and \$_.ExecutablePath.Contains(\$p1))) } | ForEach-Object { '{0} {1} {2} :: {3}' -f \$_.ProcessId, \$_.Name, \$_.ExecutablePath, \$(if (\$_.CommandLine) { \$_.CommandLine.Substring(0, [Math]::Min(220, \$_.CommandLine.Length)) } else { '' }) }" > "$f" 2>&1 < /dev/null
  stamp "TIMEOUT STOP: the outer wall cap fired on $1; $(grep -c . "$f") candidate process(es) listed by PID and path in $(basename "$f") (NOT killed: COORD kills by PID, floor 5); no purge (a live orphan may hold files); lock KEPT; exit 5"
  KEEP_LOCK=1
  exit 5
}
capped(){ # name, logfile-suffix, cap, command... -- leg() under coreutils timeout; a fired cap stops the battery
  local name=$1 suf=$2 cap=$3; shift 3
  leg "$name" "$suf" /usr/bin/timeout -k 120 "$cap" "$@"
  local rc=$LEG_RC
  if [ "$rc" = 124 ] || [ "$rc" = 137 ]; then timeout_stop "$name"; fi
  return $rc
}
poll_start(){ # file-to-watch (unix), seen-marker -- critic 0 #3: proves the file EXISTED during the leg (the converter
  # deletes a passing publish's binlog, so 'absent afterwards' alone cannot tell passed-and-deleted from never-passed)
  rm -f "$2"
  ( while :; do [ -e "$1" ] && : > "$2"; sleep 1; done ) > /dev/null 2>&1 &
  POLLPID=$!
}
poll_stop(){ [ -n "$POLLPID" ] && kill "$POLLPID" 2>/dev/null; wait "$POLLPID" 2>/dev/null; POLLPID=''; }
dlcount(){ # label, console log (unix), package dir under src/core (unix, relative), leg t0 -- critic 0 #16: the sweep
  # console prints no host output on PASS (and 3 lines on FAIL), so a console-only count is structurally 0. This also
  # reads the 'full output:' file and the fresh comparison record's stderr tails (go and csharp).
  hp deadlock --label "$1" --log "$(cygpath -w "$2")" --dir "$(cygpath -w "$W/$3")" --t0 "$4" 2>&1 | tr -d '\r'
}
# ---- verify round 2
recclean(){ # package dir under the worktree (relative) -- delete the row's three gitignored record files BEFORE a leg
  # Freshness is read by mtime, and the converter writes go2cs_test_comparison.json ONLY when its bytes change
  # (testConversion.go writeJSONFile -> needToWriteFile). The files are gitignored (src/core/.gitignore), so they survive
  # purge, restore_paths and a whole battery: a byte-identical record kept its old mtime and read STALE (TRAIN L's run 2:
  # NR:2 to NR:5 all read 'DEADLOCK total=0 ... record=STALE', so four of the five net/rpc deadlock readings were not
  # measured), and a host that died before its compare left an earlier run's record to be read as its own. Deleted
  # first, a record that exists after the leg is THIS leg's, and a missing one is a failed reading, never 'no lines'.
  rm -f "$W/$1/go2cs_test_comparison.json" "$W/$1/go2cs_test_results.json" "$W/$1/go2cs_test_results.xml"
}
trline(){ # dotnet-test log -> 'failed passed skipped total' from its LAST result line, or nothing
  grep -aE '(Passed|Failed)!' "$1" | tail -n 1 | tr -s ' ' | sed -nE 's/.*Failed: ([0-9]+), Passed: ([0-9]+), Skipped: ([0-9]+), Total: ([0-9]+).*/\1 \2 \3 \4/p'
}
tmfloor(){ # pathspec... -> the uncommented [TestMethod] lines under it at HEAD: a FLOOR for a run's Total (a DataRow adds results, none removes one)
  git grep -hE '^[[:space:]]*\[(Data)?TestMethod' HEAD -- "$@" | wc -l
}
totalgate(){ # label, log, floor, measured minimum, source text, skipped-must-be-0 (yes|no) -- R1: 0 failed AND total >= the floor
  local tl f p s t shown='NO RESULT LINE'
  tl=$(trline "$2"); set -- "$1" "$2" "$3" "$4" "$5" "$6" $tl; f=${7:-}; p=${8:-}; s=${9:-}; t=${10:-}
  [ -z "$tl" ] || shown="Failed $f, Passed $p, Skipped $s, Total $t"
  stamp "  $1 (EXPECT Failed 0, Total >= $3: $5; the measured minimum is $4): $shown :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$2" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
  [ "$3" -ge "$4" ] || finding "$1: the derived floor $3 is below the measured $4 ($5): test methods LEFT the tree"
  [ -n "$tl" ] && [ "$f" = 0 ] && [ "$t" -ge "$3" ] && { [ "$6" != yes ] || [ "$s" = 0 ]; } || finding "$1: '${tl:-no result line}' (failed passed skipped total): expected Failed 0, Total >= $3$([ "$6" = yes ] && echo ', Skipped 0')"
}
gtclassoff(){ # 'TRX class' line body (e.g. 'Inconclusive=1 Passed=5'), floor -> a reason when the class is off, nothing when it is not
  local kv tot=0
  case "$1" in ''|*'NOT FOUND'*) echo 'NOT FOUND'; return ;; esac
  for kv in $1; do
    case "${kv%%=*}" in Failed|Error|Timeout|Aborted) echo "$kv"; return ;; esac
    case "${kv##*=}" in *[!0-9]*|'') ;; *) tot=$((tot + ${kv##*=})) ;; esac
  done
  [ "$tot" -ge "$2" ] || echo "results $tot < floor $2"
}
orphans(){ # label, folder (unix) -- processes whose EXECUTABLE lives under that folder: listed by PID and path, never killed (floor 5)
  # Sets ORPH_N (the count, or 'unread' when the census itself failed: no pass by absence).
  local f="$LOGDIR/orphans-$1.txt" rc
  powershell -NoProfile -Command "\$p = '$(cygpath -w "$2")' + [char]92; Get-CimInstance Win32_Process | Where-Object { \$_.ProcessId -ne \$PID -and \$_.ExecutablePath -and \$_.ExecutablePath.StartsWith(\$p, [StringComparison]::OrdinalIgnoreCase) } | ForEach-Object { '{0} {1} {2}' -f \$_.ProcessId, \$_.Name, \$_.ExecutablePath }" > "$f" 2>&1 < /dev/null; rc=$?
  if [ "$rc" = 0 ]; then ORPH_N=$(tr -d '\r' < "$f" | grep -c .); else ORPH_N=unread; fi
}

# ================================================================================================= LEGS
# LEG C -- the full converter suite, NO -short (the module integration tests t.Skip under -short; CM in leg MOD reads
# them by name). Gates every M converter seat: G (callerSkipWindowFrames, goCreatorFrame), c1-darwin-linkname-pulls
# (linknameForwardDarwin, linknameForwardRegistry, repoguard's darwinLinknamePulls census), c1-fixture-tracking
# (fixtureWritesTracked beside L's fixturesCurrent: the two guards meet here for the first time, H6), the i9's
# byRefReceiverGuard (it walks ..\core ON DISK: this leg runs FIRST, on a tree no -tests run has written), P2's F4
# (publishTimeout), F8 (testConversion_test), F5/F3/F7/F1+F6 (their behavioral guards' attribute and registration
# guards), repoguard's docs suites over P1's FINDING record, and the projitems integrity tests (five seats register files).
# EXPECT ok throughout. The suite reads git state for the fixture guards: -count=1, on the COMMITTED union.
leg C conv-suite bash -c 'cd src/go2cs && go test -count=1 -timeout 40m ./...'
stamp "  C: $(grep -aE '^(ok|FAIL|---)' "$LOGDIR/conv-suite.log" | tr '\n' ' ' | cut -c1-500) :: projitems FAIL lines: $(grep -aE 'TestProjitems[A-Za-z]*' "$LOGDIR/conv-suite.log" | grep -ac FAIL) :: attribute-guard FAIL lines: $(grep -aE 'TestOutputComparisonListMatchesConsoleOutputAttribute|TestBehavioralPackageInfoCarriesNoDuplicateAttribute' "$LOGDIR/conv-suite.log" | grep -ac FAIL)"
# r-m3's integration test (TRAIN L, now on master) leaves its driver binary in TEMP (os.MkdirTemp('', 'go2cs-m3-driver'), never removed):
# count, then remove exactly those directories (the name is the test's own prefix; nothing else in TEMP is touched).
nd=$(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l)
find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' -exec rm -rf {} + 2>/dev/null
stamp "  C: r-m3 driver dirs left in TEMP by the suite: $nd (removed: $(( nd - $(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l) )))"

# LEG CB -- the M seats' tests BY NAME (critic 0 #2): leg C runs without -v and reads package ok/FAIL only, so a
# missing, renamed or skipped test reads green there. Each name below must print '--- PASS', and no SKIP/FAIL line.
# Names read at the pre-map union 8d7305053f (git grep 'func <name>('), by seat:
#   G                           TestCallerSkipWindowFramesAreNotInlined TestGoCreatorFramesAreNotInlined
#   i9-crashclass               TestByRefReceiversCarryGoRecv (seat: 4233 files scanned, 5985 marked, 6 allowlisted)
#   c1-fixture-tracking         TestEveryStagedFixtureIsTrackedOrIgnored + its control TestFixtureWriteScannerFires
#   c1-darwin-linkname-pulls    TestDarwinSyscallPullsForward TestLinknameForwardTargetsExposeNoUnexportedTypes (+ its
#                               control TestUnexportedSignatureTypesFires); in ./internal/repoguard/:
#                               TestDarwinLinknamePullsAreFilledOrDeclared (+ controls TestOpenDarwinPullProblemsFires,
#                               TestDarwinLinknamePullScannerFires). The census line is stamped: the seat read '17 pulls,
#                               13 filled, 4 unfilled: 3 declared dormant and 1 declared OPEN' (route.sysctl); with the
#                               in-flight c1-route-sysctl seated it reads 0 open. READ it; neither count is asserted.
#   p2-test-publish-timeout     TestPublishTimeoutHasAFloor TestPublishTestHostUsesThePublishBudget
#   p2-test-overload-references TestDeclarationClosureImportsSurfacesOverloadCandidateEdges
#   five registering seats      the 5 projitems tests
#   the merged tree (verify round 1)  TestStdLibMetadataInSync: the union changes 30 src/core package_info.cs and no
#                               stdlib-metadata.txt, the fixup's step 4 can copy more, and the guard has a t.Skipf that
#                               leg C (no -v) would read as ok. The fixup reads it by name too, before its commit.
# Verify round 2 (R1): the names above are what the 15 drafted rows added, kept as a description. The LIST is CB_TESTS,
# derived in PRE-D from the union's own diff (73 added names at the 24-row map union, plus the six base tree guards), so
# a test a late row adds is read by name without an edit, and the package list is './...' (a seat can add a package:
# c2-s2-source-metadata adds ./internal/sourcemeta and ./internal/gensourcemeta). A '+func Test' the union adds that
# skips on this box (0 SKIP is the gate) is a finding to READ, by name: that is the point of the leg.
CB_RE="^($(echo $CB_TESTS | tr ' ' '|'))\$"
CB_N=$(echo $CB_TESTS | wc -w)
leg CB conv-m-named bash -c "cd src/go2cs && go test -count=1 -timeout 30m -v -run '$CB_RE' ./..."
cbrc=$LEG_RC; cbmiss=''
for t in $CB_TESTS; do grep -aqE -- "--- PASS: $t \(" "$LOGDIR/conv-m-named.log" || cbmiss="$cbmiss $t"; done
cbsk=$(grep -acE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-m-named.log")
stamp "  CB (EXPECT $CB_N/$CB_N '--- PASS', 0 SKIP/FAIL; the names are derived, PRE-D): rc=$cbrc not-PASS=[${cbmiss# }] skip/fail-lines=$cbsk [$(grep -aE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-m-named.log" | awk '{ print $3 }' | head -n 8 | tr '\n' ' ')] projitems-PASS=$(grep -acE -- '--- PASS: TestProjitems' "$LOGDIR/conv-m-named.log")"
stamp "  CB readings (stated, not gated): $(grep -aiE 'pulls|scanned|marked|allowlist|unfilled|dormant' "$LOGDIR/conv-m-named.log" | grep -av '^=== ' | tr -s ' ' | tr '\r\n' '  ' | cut -c1-500)"
[ "$cbrc" = 0 ] && [ -z "$cbmiss" ] && [ "$cbsk" = 0 ] || finding "CB not-PASS=[${cbmiss# }] skip/fail=$cbsk rc=$cbrc"

# LEG FX -- the fixture-currency guard's Logf reading (leg C hides Logf on pass). TRAIN L read 'tracked fixtures 1281 ·
# current 1281 · stale 0' at go1.24.13; c1-fixture-tracking commits ten more fixtures, which join this guard (INFERRED
# 1291: read the number, do not carry it; all ten were measured byte-equal to the go1.24.13 sources). EXPECT stale 0
# and the scanner control PASS.
leg FX fixture-currency bash -c "cd src/go2cs && go test -count=1 -timeout 15m -v -run 'TestTrackedFixturesMatchPinnedGoRoot|TestFixtureCurrencyScannerFires' ./internal/repoguard/"
stamp "  FX: $(grep -aE -- '--- (PASS|FAIL|SKIP)|tracked fixtures|other cached' "$LOGDIR/fixture-currency.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-400)"
grep -aqE 'stale 0( |$)' "$LOGDIR/fixture-currency.log" || finding "FX: no 'stale 0' reading in fixture-currency.log"
# Verify round 1: 'stale 0' alone passes a union whose ten new fixtures are tracked but OUTSIDE the guard's scan. The
# floor is sourced without inventing 1291: TRAIN L read 1281 on this box and the seat's commit says the ten "come under
# the fixture-currency guard" (20f932b859), so tracked must exceed 1281 and equal current.
# Verify round 2 (RULED, R1): a FLOOR, stamped with its source: tracked >= TRAIN L's reading on this box, which is read
# from L's own SUMMARY when it is still there (it reads 1281) and is the literal 1281 otherwise; and tracked == current.
FX_FLOOR=$(grep -aoE 'tracked fixtures [0-9]+' "$L_SUMS" 2>/dev/null | tail -n 1 | grep -oE '[0-9]+'); FX_SRC="TRAIN L run 2's own FX stamp ($(basename "$(dirname "$(dirname "$L_SUMS")")")/tL-logs/SUMMARY.txt)"
[ -n "$FX_FLOOR" ] || { FX_FLOOR=1281; FX_SRC="the literal 1281, TRAIN L's reading (its SUMMARY is no longer on this box)"; }
fxt=$(grep -aoE 'tracked fixtures [0-9]+' "$LOGDIR/fixture-currency.log" | tail -n 1 | grep -oE '[0-9]+'); fxcur=$(grep -aoE 'current [0-9]+' "$LOGDIR/fixture-currency.log" | tail -n 1 | grep -oE '[0-9]+')
stamp "  FX floor: tracked=${fxt:-unread} current=${fxcur:-unread} (EXPECT tracked >= $FX_FLOOR: $FX_SRC; and tracked == current)"
[ -n "$fxt" ] && [ "$fxt" -ge "$FX_FLOOR" ] && [ "$fxt" = "$fxcur" ] || finding "FX tracked=${fxt:-unread} current=${fxcur:-unread}: expected tracked >= $FX_FLOOR ($FX_SRC) and tracked == current"
# LEG FXc -- the guard's two DISCRIMINATING controls on REAL data (critic 0 #13, floor 13). The repoguard test binary is
# compiled once, OUTSIDE the worktree, and run from the package dir (it finds the repo root through .git) against:
#  FXc1 GOROOT = go1.23.12 (not the pin) and GOMODCACHE = an EMPTY folder: EXPECT rc!=0 and 'no GOROOT at the pinned
#       go1.24.13' (the guard FAILS rather than skips). GOMODCACHE must be empty because the H: cache HOLDS
#       golang.org/toolchain@v0.0.1-go1.24.13.windows-amd64, which the guard would otherwise find (listed 2026-10-01).
#  FXc2 GOROOT = an H: copy of go1.24.13's src + VERSION with ONE go/printer fixture altered: EXPECT rc!=0 and exactly one
#       STALE FIXTURE line naming go/printer/testdata/alignment.golden (the source prints 'STALE FIXTURE at go1.24.13:
#       src/core/go/printer/testdata/alignment.golden: differs from the pinned GOROOT (...)': the pin comes first).
FXC=$SD/fxc; rm -rf "$FXC"; mkdir -p "$FXC/emptymodcache" "$FXC/goroot"
leg FXc-build fxc-build bash -c "cd src/go2cs && go test -c -o '$(cygpath -w "$FXC/repoguard.test.exe")' ./internal/repoguard/"; fxb=$LEG_RC
if [ "$fxb" = 0 ] && [ -x "$FXC/repoguard.test.exe" ] && [ -d "$HOME/sdk/go1.23.12" ]; then
  leg FXc1 fixture-currency-c1 env GOROOT="$(cygpath -w "$HOME/sdk/go1.23.12")" GOMODCACHE="$(cygpath -w "$FXC/emptymodcache")" \
    bash -c "cd src/go2cs/internal/repoguard && '$FXC/repoguard.test.exe' -test.count=1 -test.timeout 10m -test.v -test.run '^TestTrackedFixturesMatchPinnedGoRoot\$'"
  f1rc=$LEG_RC; f1m=$(grep -ac 'no GOROOT at the pinned go1.24.13' "$LOGDIR/fixture-currency-c1.log")
  stamp "  FXc1 (EXPECT rc!=0 + 'no GOROOT at the pinned go1.24.13'): rc=$f1rc named=$f1m :: $([ "$f1rc" != 0 ] && [ "$f1m" -ge 1 ] && echo 'CONTROL FIRED' || echo 'CONTROL DID NOT FIRE')"
  [ "$f1rc" != 0 ] && [ "$f1m" -ge 1 ] || finding "FXc1 control did not fire (rc=$f1rc named=$f1m)"
  # Verifier: the few-hundred-MB GOROOT copy runs as its own leg, so it gets the R5 deadline check and the 30G/8G disk
  # preflight; $FXC is removed by the EXIT trap and by deadline_check if a stop or abort skips the rm below.
  leg FXc2-prep fixture-currency-c2-prep env SRC="$HOME/sdk/go1.24.13" DST="$FXC/goroot" \
    bash -c 'cp -r "$SRC/src" "$DST/src" && cp "$SRC/VERSION" "$DST/VERSION" && printf "\n// TRAIN M FXc2 plant\n" >> "$DST/src/go/printer/testdata/alignment.golden"'
  fxcp=$LEG_RC
  if [ "$fxcp" = 0 ]; then
    leg FXc2 fixture-currency-c2 env GOROOT="$(cygpath -w "$FXC/goroot")" \
      bash -c "cd src/go2cs/internal/repoguard && '$FXC/repoguard.test.exe' -test.count=1 -test.timeout 10m -test.v -test.run '^TestTrackedFixturesMatchPinnedGoRoot\$'"
    f2rc=$LEG_RC; f2m=$(grep -acE 'STALE FIXTURE at .*go/printer/testdata/alignment\.golden' "$LOGDIR/fixture-currency-c2.log"); f2all=$(grep -ac 'STALE FIXTURE at ' "$LOGDIR/fixture-currency-c2.log")
    stamp "  FXc2 (EXPECT rc!=0 + exactly 1 STALE FIXTURE, the planted go/printer file): rc=$f2rc planted-named=$f2m stale-lines=$f2all :: $([ "$f2rc" != 0 ] && [ "$f2m" = 1 ] && [ "$f2all" = 1 ] && echo 'CONTROL FIRED BY NAME' || echo 'CONTROL DID NOT FIRE AS PREDICTED')"
    [ "$f2rc" != 0 ] && [ "$f2m" = 1 ] && [ "$f2all" = 1 ] || finding "FXc2 control (rc=$f2rc planted=$f2m stale=$f2all)"
  else
    finding "FXc2 NOT MEASURED: the GOROOT copy failed"
  fi
else
  finding "FXc NOT MEASURED: repoguard test binary build rc=$fxb (fxc-build.log) or no go1.23.12 root"
fi
rm -rf "$FXC"; FXC=''

# LEG E -- whole-corpus emission check, M=TRAIN L's master vs the fixup HEAD, x3 targets, planted controls. At M the
# seats DO have -stdlib footprints, and every one of them must now be IN THE TREE: G's go-creator frame (76 corpus files
# committed by the seat), c1-darwin-linkname-pulls (9 darwin files committed: the darwin arm must reproduce them), and
# the two footprints no seat committed (F1+F6's go/internal/srcimporter hunk, G's Extension A), which the fixup's step 4
# copied from this same instrument. EXPECT plants OK, 6 x rc 0, union-attributable 0, csproj drift none (the 7 new
# csproj read new-ok), HANDOWN 0, RUNTIME-MAP 0. The i9's crash-class seat claims a -stdlib footprint of 0 'by identity'.
leg E emitcheck env W="$W" M="$MASTER" U="$EXPECT_HEAD" LOGDIR="$LOGDIR" TAG=emitcheck TM_LOCK_HELD=1 bash "$SD/tM-emitcheck.sh"   # verify round 2: the check has guards of its own for a HAND launch; the battery holds the lock and says so
erc=$LEG_RC
stamp "  E: $(grep -aE '^(EMIT |PLANT|windows:|linux:|darwin:|UNION-ATTRIBUTABLE TOTAL|CSPROJ|EVIDENCE|ABORT)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-1800)"
# Verify round 1: at TRAIN L the same stamp matched 2499 characters and the cut ended inside 'UNION-ATTRIBUTABLE TOTAL',
# so the verdict never reached SUMMARY; M's new 'HANDOWN derived' line would take another 500. The verdict and the three
# named readings get their own stamp, and a hand-owned file written by a conversion (stated in the verdict, not part of
# the emission check's rc) or a missing verdict is a FINDING here, as it is a die in the fixup.
ev=$(grep -a '^EMITCHECK VERDICT' "$LOGDIR/emitcheck.log" | tail -n 1 | tr -d '\r')
stamp "  E verdict: ${ev:-NO VERDICT LINE} :: $(grep -aE '^(RUNTIME-MAP|HANDOWN hand-owned|CSPROJ rc)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-400)"
case "$ev" in *"handown-written=0 "*) ;; *) finding "E: a hand-owned file written by a conversion, or no verdict (${ev:-none})" ;; esac
EATTR=$(grep -a '^UNION-ATTRIBUTABLE TOTAL:' "$LOGDIR/emitcheck.log" | awk '{print $3}')
# LEG E-bisect -- ONLY when E reads union-attributable > 0 (the fixup's regeneration did not close it): one one-axis arm
# per seat in E_BISECT_SEATS, M = the first parent of that seat's own merge on the union, U = that merge. The shas are
# read from the union; none is written here. The arm of a seat with a KNOWN uncommitted footprint reads > 0 by itself
# (that is the footprint): the reading is WHICH FILES each arm names, compared with the main leg's list. A file no arm
# names belongs to another seat or to an interaction: COORD bisects further by hand. CSPROJ_GATE=0 (not a fixup head).
if [ "${EATTR:-0}" != 0 ] && [ -n "$EATTR" ]; then
  stamp "  E: union-attributable=$EATTR > 0 -- running the bisect arms for [$E_BISECT_SEATS] (the fixup is RE-DESIGNED around what they name, never absorbed)"
  for bs in $E_BISECT_SEATS; do
    bsha=$(grep -E "^$bs\|" "$SEATS" | head -n 1 | cut -d'|' -f2)
    bfull=$(git rev-parse --verify -q "${bsha:-none}^{commit}") || { stamp "  E-bisect-$bs: NOT RUN (no seat row '$bs' in $(basename "$SEATS"))"; continue; }
    bm=$(awk -v s="$bfull" '$1 == s { print $2 }' "$LOGDIR/union-merges.txt" | head -n 1)   # PRE-D's map of the union's merges
    [ -n "$bm" ] || { stamp "  E-bisect-$bs: NOT RUN (no first-parent merge of ${bfull:0:10} on the union)"; continue; }
    leg "E-bisect-$bs" "emitcheck-bisect-$bs" env W="$W" M="$(git rev-parse "$bm^1")" U="$bm" LOGDIR="$LOGDIR" TAG="emitcheck-bisect-$bs" CSPROJ_GATE=0 TM_LOCK_HELD=1 bash "$SD/tM-emitcheck.sh"
    stamp "  E-bisect-$bs (M=$(git rev-parse --short=10 "$bm^1") vs U=$(git rev-parse --short=10 "$bm"), that seat's merge alone): $(grep -aE '^(UNION-ATTRIBUTABLE TOTAL|EMITCHECK VERDICT)' "$LOGDIR/emitcheck-bisect-$bs.log" | tr '\r\n' '  ') :: files: $(cat "$LOGDIR/emitcheck-bisect-$bs"/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u | head -n 12 | tr '\n' ' ')"
  done
  # Verify round 2 (R1: a list that is short for the union is a finding, never silence): the files the main leg names
  # and NO arm names belong to a converter seat outside E_BISECT_SEATS, or to an interaction of seats.
  eun=$(LC_ALL=C comm -23 <(cat "$LOGDIR/emitcheck"/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u) <(cat "$LOGDIR"/emitcheck-bisect-*/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u) | tr '\n' ' ')
  [ -z "$eun" ] || finding "E-bisect: file(s) the main leg reads union-attributable and no bisected seat's arm names: [$(echo $eun | cut -c1-400)] -- the converter seats not bisected are [${ebrest% }] (PRE-D): run their arms (tM-emitcheck.sh by hand, or E_BISECT_SEATS)"
else
  stamp "  E-bisect: not run (E union-attributable=${EATTR:-unread}; rc=$erc)"
fi

# LEG G1/G2 -- the roster guard under pwsh 7 and Windows PowerShell 5.1: M's roster gate. G's two roster commits drop
# 'execution: release-tiered' from net/http and internal/godebug; master's TRAIN L commit inserted a provenance block;
# git merged the three hunks clean. EXPECT pass under both editions and EXACTLY $EXEC_ROWS_EXPECT row with an execution
# config (log/slog; tL-seats-draft.txt:31 'execution-config count becomes 1 with both'). L read '1991 checks pass (225
# rows, 223 with a linux annotation, 3 with an execution config, 5 excluded)': read the check count, do not carry it.
leg G1 roster env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1
stamp "  G1: $(tail -n 3 "$LOGDIR/roster.log" | tr '\r\n' '  ' | cut -c1-300)"
leg G2 roster51 env -u DOTNET_ROOT PATH="$GOPIN_PATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1
stamp "  G2: $(tail -n 1 "$LOGDIR/roster51.log" | tr '\r\n' '  ' | cut -c1-300)"
for gl in roster roster51; do
  ger=$(grep -a 'roster format guard:' "$LOGDIR/$gl.log" | tail -n 1 | grep -oE '[0-9]+ with an execution config' | cut -d' ' -f1)
  [ "${ger:-x}" = "$EXEC_ROWS_EXPECT" ] || finding "G ($gl.log): '${ger:-unread}' rows with an execution config, expected $EXEC_ROWS_EXPECT"
done
stamp "  G execution configs in the roster: $(grep -aA 6 'per-row execution configs in the roster' "$LOGDIR/roster.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300)"

# LEG SY -- symbol sync (cheap; every train).
leg SY symbol-sync env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-symbol-sync.ps1
stamp "  SY: $(tail -n 3 "$LOGDIR/symbol-sync.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SI + SIc -- check-solution-integrity: SEVEN new go2cs.slnx projects (PromotedPtrMethodValueSet, the i9;
# VariadicClosureShadowParam, AliasStructToInterface, AliasStructToInterfaceLib, NamedArrayVsUnnamedCompare,
# PanicOnlyFuncLiteralVar, FuncLiteralDeclaredResultIface, P2; 874 -> 881 Project lines on the pre-map union). It also
# reads a csproj on disk that go2cs.slnx does not register: hazard H4's second face, should a Go-only repro directory
# ever be transpiled in place. Then K's POSITIVE CONTROL (floor 13): the injected runtime -> internal/syscall/windows
# edge prints W1's six cycles, exit 1.
leg SI solution-integrity powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-solution-integrity.ps1
stamp "  SI: $(tail -n 3 "$LOGDIR/solution-integrity.log" | tr '\r\n' '  ' | cut -c1-300)"
leg SIc solution-integrity-control env MSYS_NO_PATHCONV=1 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-solution-integrity.ps1 -TargetOS windows -InjectReference 'runtime=internal/syscall/windows'
sic=$LEG_RC
sicl=$(grep -ac 'runtime -> internal/syscall/windows' "$LOGDIR/solution-integrity-control.log")
sica=$(grep -ac 'ABORTED on an unhandled error' "$LOGDIR/solution-integrity-control.log")
if [ "$sic" = 1 ] && [ "$sicl" = 6 ] && [ "$sica" = 0 ]; then SICV='control FIRED (six cycles)'
elif [ "$sic" = 1 ] && [ "$sicl" -ge 1 ] && [ "$sica" = 0 ]; then SICV="CONTROL FIRED WITH THE WRONG CYCLE SET ($sicl, want 6) -- read the log before trusting SI"
else SICV='CONTROL DID NOT FIRE -- SI proves nothing'; fi
stamp "  SIc (EXPECT rc=1, six cycles, no abort): rc=$sic cycle-lines=$sicl aborted=$sica :: $SICV"
# Verify round 1: a control that does not fire exits 0, so it reached neither end list (FXc1/FXc2 already raise one).
case "$SICV" in 'control FIRED (six cycles)') ;; *) finding "SIc $SICV (rc=$sic cycle-lines=$sicl aborted=$sica)" ;; esac

# LEG ST -- the sweep's classifier self-test under both editions (REGRESSION: the sweep dot-sources src/_roster.ps1, whose
# comment G rewrites; a BOM-less non-ASCII character in a .ps1 is exactly what broke Windows PowerShell 5.1 at L).
# TRAIN L read '62 checks, 0 violations' and no M seat touches the self-test: EXPECT the same line. (L's 'binlog
# asserts' count is dropped: it read 0 under 5.1 on a passing leg, so it was never a reading of the five asserts.)
leg ST51 sweep-selftest51 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST51 (EXPECT 0 violations, checks >= 62): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest51.log" | tail -n 1 | cut -c1-200)"
leg ST7 sweep-selftest7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST7 (EXPECT 0 violations, checks >= 62): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest7.log" | tail -n 1 | cut -c1-200)"
# Verify round 2: the stamps said EXPECT and the gate was rc alone, and a self-test that ran 0 checks prints
# '0 checks, 0 violations' and exits 0 the same way (its last two lines). A floor (R1): 0 violations AND checks >= 62,
# TRAIN L's reading under both editions (run 2's SUMMARY); no row touches the self-test at the 24-row map union.
for st in 51 7; do
  stn=$(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest$st.log" | tail -n 1 | sed -nE 's/.*: ([0-9]+) checks, 0 violations.*/\1/p')
  [ -n "$stn" ] && [ "$stn" -ge 62 ] || finding "ST$st: no '<n> checks, 0 violations' line with n >= 62 (read '${stn:-none}'; TRAIN L read 62)"
done

# LEG NV -- push-nuget's release pre-flight (it dot-sources _roster.ps1, which G edits: a comment, under BOTH editions),
# read-only -VerifyOnly in both editions, plus K's safe refusal arm (ruling K(5): only that arm).
capped NV51 push-nuget-verify51 30m powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV51: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify51.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
capped NV7 push-nuget-verify7 30m env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV7: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify7.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
capped NVR push-nuget-suffix-refusal 30m powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly -VersionSuffix local.1
nvr=$LEG_RC; nvm=$(grep -ac 'VersionSuffix is a local-rehearsal pack and is refused' "$LOGDIR/push-nuget-suffix-refusal.log")
stamp "  NVR (EXPECT rc!=0 + the refusal by name): rc=$nvr named=$nvm :: $([ "$nvr" != 0 ] && [ "$nvm" -ge 1 ] && echo 'REFUSED BY NAME' || echo 'REFUSAL ARM FAILED')"
[ "$nvr" != 0 ] && [ "$nvm" -ge 1 ] || finding "NVR refusal arm did not fire (rc=$nvr named=$nvm)"   # verify round 1: was a stamp only

# LEG NG -- the nugetgo tooling, read-only (TRAIN L's legs, KEPT; about 2 s each at L). Whether a seated row touches
# src/tools is READ from the union and stamped below (0 paths at the 15 rows of the draft list: regression legs).
# COORD's note of 04:26 (tL-seats-draft.txt:75) announces two more M rows, c2-nuget-map and c2-s2-source-metadata
# ("nugetgo-pack.ps1 packs go2cs/source-metadata.txt + third-party edges; windows acceptance = M's nugetgo legs / i9
# uuid pack"): once those rows are seated the stamp reads > 0 and these legs ARE the i7 half of that acceptance.
# Verify round 2: both ARE rows of the 24-row list, and its map union (11c188daf3) changes three paths under src/tools
# (NugetgoSelfDescription.psm1, Test-NugetgoSelfDescription.ps1, nugetgo-pack.ps1), so these legs are that seat's i7
# acceptance now. What they read is taken from the tree, never from a seat's numbers:
#  (a) the table-driven identity arms under both editions: gated on rc 0 and a 'ran N, failed 0' line; N is REPORTED
#      (L read 'ran 29, failed 0'; a seat that adds arms moves it: TO BE SET BY COORD).
#  (b) ParseFile, under BOTH editions, of every tracked .ps1 / .psm1 under src/tools/nugetgo -- the list is READ from
#      the tree, so a script a seat adds is parsed -- plus K's j0-consume.ps1. EXPECT 0 errors. GATED (L's ruling R2:
#      release-path scripts must parse under Windows PowerShell 5.1; L's red was a BOM-less em dash in nugetgo-pack.ps1).
#  (c) the J0 consumer's PackageReference ID, default and nugetgo (L's R8), unchanged.
NGF="$(git ls-files 'src/tools/nugetgo/*.ps1' 'src/tools/nugetgo/*.psm1' | tr '\n' ' ')src/tools/j0-uuid-rehearsal/j0-consume.ps1"
stamp "  NG files parsed (read from the tree): $(echo $NGF | wc -w): $NGF"
stamp "  NG paths under src/tools the union changes against $MASTER: $(git diff --name-only "$MASTER" HEAD -- src/tools | grep -c .) (0 = regression legs; > 0 = a seated row's own acceptance: $(git diff --name-only "$MASTER" HEAD -- src/tools | head -n 6 | tr '\n' ' '))"
# Verify round 2 (R1): leg NGa named ONE script, Test-NugetgoIdentity.ps1. c2-s2-source-metadata (a row of the 24-row
# list, "WINDOWS acceptance = the battery's nugetgo legs") adds Test-NugetgoSelfDescription.ps1 beside it, with the same
# contract (no parameters, a fabricated root in the temp directory, a last line 'ran N, failed M', exit code = M): only
# PARSED by NGb, never RUN. The table-driven test scripts are now READ from the tree (every Test-*.ps1 under
# src/tools/nugetgo at HEAD) and each runs under both editions, gated on a 'ran N, failed 0' line with N >= 1; N is
# REPORTED (L read 29 for the identity arms). The identity script's log keeps L's name (nugetgo-identity51 / 7).
NGT=$(git ls-files 'src/tools/nugetgo/Test-*.ps1' | tr '\n' ' ')
stamp "  NGa test scripts run (read from the tree): $(echo $NGT | wc -w): $NGT"
[ -n "$NGT" ] || finding "NGa: no Test-*.ps1 under src/tools/nugetgo at HEAD (TRAIN L ran Test-NugetgoIdentity.ps1)"
for ngs in $NGT; do
  ngn=$(basename "$ngs" .ps1 | sed 's/^Test-Nugetgo//' | tr 'A-Z' 'a-z')
  leg "NGa51:$ngn" "nugetgo-${ngn}51" powershell -NoProfile -ExecutionPolicy Bypass -File "$ngs"
  stamp "  NGa51:$ngn (EXPECT failed 0; N REPORTED): $(grep -aiE 'ran [0-9]+' "$LOGDIR/nugetgo-${ngn}51.log" | tail -n 1 | tr -d '\r' | cut -c1-160)"
  leg "NGa7:$ngn" "nugetgo-${ngn}7" env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File "$ngs"
  stamp "  NGa7:$ngn (EXPECT failed 0; N REPORTED): $(grep -aiE 'ran [0-9]+' "$LOGDIR/nugetgo-${ngn}7.log" | tail -n 1 | tr -d '\r' | cut -c1-160)"
  for ng in 51 7; do
    grep -aiE 'ran [0-9]+, failed [0-9]+' "$LOGDIR/nugetgo-$ngn$ng.log" | tail -n 1 | tr -d '\r' | grep -qE 'ran [1-9][0-9]*, failed 0$' || finding "NGa$ng:$ngn: no 'ran N, failed 0' line (nugetgo-$ngn$ng.log)"
  done
done
leg NGb51 nugetgo-parse51 powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w "$SD/tM-ng-parse.ps1")" $NGF
stamp "  NGb51 (EXPECT 0 errors under Windows PowerShell 5.1; GATED): $(grep -aE '^(PARSE|NGPARSE)' "$LOGDIR/nugetgo-parse51.log" | tr '\r\n' '  ' | cut -c1-600)"
leg NGb7 nugetgo-parse7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File "$(cygpath -w "$SD/tM-ng-parse.ps1")" $NGF
stamp "  NGb7 (EXPECT 0 errors): $(grep -aE '^NGPARSE' "$LOGDIR/nugetgo-parse7.log" | tr -d '\r')"
J0C=src/tools/j0-uuid-rehearsal/consumer/J0UuidConsumer.csproj
leg NGF-j0default j0-consumer-default dotnet msbuild "$J0C" -getItem:PackageReference
j0a=$LEG_RC; j0an=$(grep -acE '"Identity": *"go\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-default.log")
leg NGF-j0nugetgo j0-consumer-nugetgo dotnet msbuild "$J0C" -getItem:PackageReference -p:J0UuidPackageId=nugetgo.github.com.google.uuid
j0b=$LEG_RC; j0bn=$(grep -acE '"Identity": *"nugetgo\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-nugetgo.log")
stamp "  NGF-j0 (EXPECT default=go.github.com.google.uuid, -p=nugetgo.github.com.google.uuid): default rc=$j0a matched=$j0an :: nugetgo rc=$j0b matched=$j0bn"
[ "$j0a" = 0 ] && [ "$j0an" = 1 ] && [ "$j0b" = 0 ] && [ "$j0bn" = 1 ] || finding "NGF-j0 consumer PackageReference (default rc=$j0a n=$j0an; nugetgo rc=$j0b n=$j0bn)"

[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after the read-only legs: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"

# LEG 2b -- go2cs.slnx END TO END (M's seat draft names it; nothing else builds this solution, so a broken member rots
# unseen). Owed by: i9-crashclass-byref-recv (golib's four binders + src/gen's RecvGenerator and two templates: "a seat's
# proof spans src/gen the moment it touches it: build go2cs.slnx"), c1-darwin-std-hygiene (golib builtin.cs + a new
# file), G (Goroutine.cs: the constructor and Register gain systemBasis), and the seven new behavioral projects. The i9
# read 'rc 0, 875 projects' at L's union + its own project; the union registers 881. EXPECT rc 0, errors 0, gen-load 0;
# the project count is REPORTED.
leg 2b go2cs-slnx dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental
stamp "  2b: errors=$(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/go2cs-slnx.log") gen-load=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log") warnings-line=$(grep -aE 'Warning\(s\)|Error\(s\)' "$LOGDIR/go2cs-slnx.log" | tr '\n' ' ' | cut -c1-120) :: Project lines registered in go2cs.slnx: $(grep -c '<Project Path=' src/go2cs.slnx)"
# Verify round 1: CS8032/CS8034/CS8784/CS8785 are WARNINGS (a generator that failed to load, or threw), so rc stays 0 on
# the train whose i9 seat edits RecvGenerator and two templates: the count is now a finding, not only a stamp.
gl=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log"); [ "$gl" = 0 ] || finding "2b: $gl generator load/exception line(s) in go2cs-slnx.log"

# LEG CT -- i9-channeltests-makechan-panic: src/tests/ChannelTests, Debug and Release. The project was RED on master from
# TRAIN H (017bfe23a5) until this seat and no battery ever ran it (tL-seats-draft.txt:65). EXPECT 24/24 in each
# configuration (25 [TestMethod] lines, one commented out). Build and test are SEPARATE legs, as for TR and GT.
for cfg in Debug Release; do
  leg CT-build-$cfg channeltests-build-$cfg dotnet build src/tests/ChannelTests/ChannelTests.csproj -c $cfg -p:UseSharedCompilation=false
  if [ "$LEG_RC" != 0 ]; then
    stamp "  CT-$cfg: BUILD FAILED ($(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/channeltests-build-$cfg.log") errors, channeltests-build-$cfg.log) -- the test leg did not run (NOT MEASURED)"
    finding "CT-$cfg NOT MEASURED: the ChannelTests build failed"
    continue
  fi
  # Verify round 2 (RULED): the test-host legs had no time limit of any kind, and a hang in one stopped the battery for
  # good (the deadline is only checked between legs). CT, GN, TR and GT run under `capped`: a fired wall cap lists the
  # orphans by PID and path, kills nothing, keeps the lock and stops the battery (exit 5), as the sweep's caps do.
  # The total is a FLOOR (R1): Failed 0, Skipped 0, Total >= the uncommented [TestMethod] lines of the project at HEAD
  # (24 at the map union: 25 lines, one commented out), and that floor must not be below the measured 24.
  capped CT-$cfg channeltests-$cfg 10m dotnet test src/tests/ChannelTests/ChannelTests.csproj -c $cfg --no-build
  totalgate "CT-$cfg" "$LOGDIR/channeltests-$cfg.log" "$(tmfloor src/tests/ChannelTests)" 24 "the uncommented [TestMethod] lines under src/tests/ChannelTests at HEAD" yes
done

# LEG GN -- i9-crashclass-byref-recv touches src/gen (RecvGenerator, the Str and StructType templates), and its seat line
# owes GenTests (tM-seats-draft.txt:4). No command or configuration is ruled: Debug, one run. Verify round 1: the total
# is gated at 68, the [TestMethod] lines under src/tests/GenTests at the pre-map union (0 DataRow, 0 Ignore; the seat
# touches none), so a run that discovers no test is no longer green. A different ruled total is COORD's to set here.
leg GN-build gentests-build dotnet build src/tests/GenTests/GenTests.csproj -c Debug -p:UseSharedCompilation=false
if [ "$LEG_RC" = 0 ]; then
  # Verify round 2 (RULED): capped 20m; the total is a floor (R1): Failed 0 and Total >= the [TestMethod] lines under
  # src/tests/GenTests at HEAD, which must not be below the measured 68 (two golib+gen seats ride now: the i9's and
  # p1-warnings-tranche1; a seat that adds an arm moves the floor with it).
  capped GN gentests 20m dotnet test src/tests/GenTests/GenTests.csproj -c Debug --no-build
  totalgate GN "$LOGDIR/gentests.log" "$(tmfloor src/tests/GenTests)" 68 "the [TestMethod] lines under src/tests/GenTests at HEAD" no
else
  stamp "  GN: BUILD FAILED ($(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/gentests-build.log") errors, gentests-build.log) -- the test leg did not run (NOT MEASURED)"
  finding "GN NOT MEASURED: the GenTests build failed"
fi

# LEG TR -- BehavioralTests filtered to TestingRuntimeTests (the testing host: the i9's seat marks 25 testing.cs logging
# methods [GoRecv]; F4 and F8 edit testConversion.go, which writes the host). Critic 1 #13: build and test are SEPARATE
# legs, so a build failure reads as BUILD FAILED, never as an empty Passed/Failed stamp. TRAIN L read 26/26.
leg TR-build testing-runtime-build dotnet build src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug -p:UseSharedCompilation=false -p:go2csPath=H:/go2cs-tmp-coord/tM/src/
if [ "$LEG_RC" = 0 ]; then
  # Verify round 1: a filter that matches nothing prints no result line and (INFERRED: vstest) exits 0. 26 = the
  # [TestMethod] lines of TestingRuntimeTests.cs at the pre-map union (no DataRow) and TRAIN L's reading.
  # Verify round 2 (RULED): capped 20m; a floor (R1): Failed 0, Skipped 0, Total >= the [TestMethod] lines of
  # TestingRuntimeTests.cs at HEAD (two testing-host seats ride now, H1 and H2), never below the measured 26.
  capped TR testing-runtime 20m dotnet test src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug --no-build -p:go2csPath=H:/go2cs-tmp-coord/tM/src/ --filter FullyQualifiedName~TestingRuntimeTests
  totalgate TR "$LOGDIR/testing-runtime.log" "$(tmfloor src/tests/Behavioral/BehavioralTests/TestingRuntimeTests.cs)" 26 "the [TestMethod] lines of TestingRuntimeTests.cs at HEAD" yes
else
  stamp "  TR: BUILD FAILED (testing-runtime-build.log: $(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/testing-runtime-build.log") errors) -- the test leg did not run (NOT MEASURED)"
  finding "TR NOT MEASURED: the BehavioralTests build failed"
fi

# LEG GT -- GolibTests, Debug x1 and Release x3 FULL (K's re-read pattern), each with a TRX read BY NAME (a class with 0
# results is NOT FOUND, never green). "A golib-touching seat runs GolibTests at BOTH configurations": M has three (G,
# the i9, c1-darwin-std-hygiene). The three Release runs stay for hazard H3: G moves IsSystem from the creator frame to
# the start function, and TRAIN L's checkdead builds its user set from that fact; neither lane read the two together,
# and the classes that would show it are timing-shaped. Every class name below was read at the pre-map union 8d7305053f.
#   G        SystemGoroutineTests (5 methods; +43 lines by the seat) ReturnSiteLineTests (NEW, 1; red at the seat's base
#            at TC0 and tiered) + the creator/caller neighbours TestGoroutineCreatorTests CreatedByPositionTests
#            TracebackDecorationTests SkipCountedWalkerFrameTests RuntimeCallerPCSpanTests SyntheticPCRegistryTests
#   H3       ForeverWaitDeadlockDecisionTests (TRAIN L's, 6) + the Goroutine.cs / s_profileGate neighbours L named
#   i9       CopyBoundReceiverTests (NEW, 9) CopyBoundReceiverAllowlistTests (NEW, 1: the 201-row allowlist)
#            NoUncountedBackingAllocationsTests (the seat's last commit fixed the one failure it had introduced)
#   host     ModuleAncestryTests FixtureLinkStagingTests (the testing package the i9's seat edits; the i7's set is green:
#            the i9's '3 failed' are symlink-privilege reds of that box)
#   NOT on windows by construction: DarwinStdDescriptorContractTests (c1-darwin-std-hygiene; compiled only for linux:
#            GolibTests.csproj removes it off GoTargetOS=linux). It is P1's and P2's arm, never listed here.
# G read 'D 1454/0/29 R 1462/0/21' on its pre-L base and L's battery read 'D 1467/0/29 R 1475/0/21': the totals are
# REPORTED, not asserted (no M total is ruled).
# Verify round 2 (R1): the class names above describe the 15 drafted rows. The LISTS are PRE-D's: GT_CLASSES = the
# classes the union adds or changes (minus the ones this box does not compile, read from GolibTests.csproj) + the
# literal neighbours GT_NEIGH; GT_TESTS the two literal Class.Method names; GT_EXPECT = each ADDED class with its floor
# (the uncommented [TestMethod] lines of its file at HEAD). An added class is off when it is NOT FOUND, holds a Failed /
# Error / Timeout / Aborted result, or holds fewer results than its floor (a DataRow adds results; an Inconclusive one
# counts: DarwinArm64VariadicSlotTests has a linux-only arm). At the 24-row map union the union adds, on this box,
# CopyBoundReceiverAllowlistTests 1, CopyBoundReceiverTests 9, DarwinArm64VariadicSlotTests 6, DarwinInode64SymbolTests
# 4, EventLineFramingTests 2, HostTestListTests 4, ReturnSiteLineTests 1, and changes ConsoleEventLineAtomicityTests and
# SystemGoroutineTests. Each run is capped at 45m (RULED; TRAIN L read 213 to 290 s).
# (L's C1 token probe, GT_PROBE / R3, left with its seat: c1-token-ids landed in L.)
GT_NF=''; GT_SMP=''
for run in Debug Release-1 Release-2 Release-3; do
  cfg=${run%%-*}
  # critic 1 #13: build and test are SEPARATE legs (a build failure is BUILD FAILED, never an empty stamp)
  leg GT-build-$run golibtests-build-$run dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false
  if [ "$LEG_RC" != 0 ]; then
    GT_NF="$GT_NF$run:[BUILD FAILED] "
    stamp "  GT-$run: BUILD FAILED ($(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/golibtests-build-$run.log") errors, golibtests-build-$run.log) -- the test leg did not run (NOT MEASURED)"
    continue
  fi
  capped GT-$run golibtests-$run 45m dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build --logger "trx;LogFileName=golibtests-$run.trx" --results-directory "$LOGW"
  stamp "  GT-$run: $(grep -aE '(Passed|Failed)!' "$LOGDIR/golibtests-$run.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/golibtests-$run.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
  if [ -f "$LOGDIR/golibtests-$run.trx" ]; then
    hp trx "$LOGW\\golibtests-$run.trx" $GT_CLASSES $GT_TESTS > "$LOGDIR/golibtests-$run.classes.txt" 2>&1
    nf=$(grep -a 'NOT FOUND' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX \(class\|test\) //; s/:.*//' | tr '\n' ' ')
    [ -n "$nf" ] && GT_NF="$GT_NF$run:[${nf% }] "
    smp=$(grep -a '^TRX class SyncMutexProfileTests:' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX class SyncMutexProfileTests: //')
    GT_SMP="$GT_SMP$run:[$smp] "
    gtx=''
    for ex in $GT_EXPECT; do
      got=$(grep -a "^TRX class ${ex%%=*}:" "$LOGDIR/golibtests-$run.classes.txt" | tr -d '\r' | sed 's/^TRX class [^:]*: //')
      why=$(gtclassoff "$got" "${ex##*=}")   # verify round 2: a floor, not an exact 'Passed=N'
      [ -z "$why" ] || gtx="$gtx${ex%%=*}:[${got:-unread} -- $why] "
    done
    stamp "  GT-$run classes: NOT-FOUND=[${nf% }] $(grep -a 'Failed=' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX class //' | tr '\n' ' ' | cut -c1-300) :: SyncMutexProfileTests=[$smp] :: added classes off their floors ($GT_EXPECT): [${gtx% }] (per-class table: golibtests-$run.classes.txt)"
    [ -z "$nf" ] && [ -z "$gtx" ] || finding "GT-$run by name: NOT-FOUND=[${nf% }] off-count=[${gtx% }]"
  else
    GT_NF="$GT_NF$run:[NO TRX] "
    stamp "  GT-$run classes: NO TRX written -- the by-name reading is NOT MEASURED"
    finding "GT-$run NO TRX: the by-name reading is NOT MEASURED"
  fi
done
purge after-GT

# LEG 4 -- CNR (byte-identical behavioral C# AND csproj under the union converter): the gate of record for every
# golden. At M it is the FIRST reading of G's 39 re-baselined goldens beside L's converter and the P2 batch, of the
# seven new projects' goldens under G's rules (none of their sources holds a go statement or a runtime.Caller: measured),
# and of the goldens the fixup re-baselined. EXPECT NO REGRESSION. A changed .cs is a seat x seat INTERACTION the
# fixup's own CNR did not see: re-baseline in a new fixup and read that guard isolated -- never waived.
# N is REPORTED, not asserted, unless COORD passes CNR_EXPECT_N: TRAIN L read 778 (784 directories with Go source - 6
# platform skips). M adds 7 project directories and, at the frozen list, nothing else (hazard H4's repro module is
# deleted inside its seat: 791 directories hold Go source at the map union 11c188daf3). INFERRED 785: TO BE SET BY COORD.
# None of the 7 new M dirs may sit in the platform-exclusive skip list (a dir dropped there still reads NO REGRESSION).
CNR_EXPECT_N=${CNR_EXPECT_N:-}
# Verify round 2 (R1): M7DIRS is PRE-D's (the behavioral projects the union adds, from the tree; seven at the 24-row map
# union: PromotedPtrMethodValueSet, VariadicClosureShadowParam, AliasStructToInterface, AliasStructToInterfaceLib,
# NamedArrayVsUnnamedCompare, PanicOnlyFuncLiteralVar, FuncLiteralDeclaredResultIface). No list is written here.
leg 4 cnr powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1
stamp "  4: $(grep -aiE 'NO REGRESSION|REGRESSION|byte-identical|CHANGED|verdict' "$LOGDIR/cnr.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
cnrn=$(grep -aoE 'NO REGRESSION: generated C# and \.csproj are byte-identical across all [0-9]+ behavioral packages' "$LOGDIR/cnr.log" | grep -oE '[0-9]+' | tail -n 1)
cnrskip=$(for d in $M7DIRS; do grep -aqE "^\s+$d \[" "$LOGDIR/cnr.log" && printf '%s ' "$d"; done)
stamp "  4 asserts: N=${cnrn:-unread} (EXPECT ${CNR_EXPECT_N:-not set: REPORTED, TO BE SET BY COORD}) skip-list holds M dirs: [${cnrskip% }] (EXPECT none) skipped(platform-exclusive) header: $(grep -aoE 'SKIPPED \(platform-exclusive, [0-9]+\)' "$LOGDIR/cnr.log" | head -n 1)"
[ -n "$cnrn" ] || finding "CNR N unread: no NO REGRESSION verdict line (read cnr.log: CHANGED and NOT MEASURED lists)"
[ -z "$CNR_EXPECT_N" ] || [ "${cnrn:-x}" = "$CNR_EXPECT_N" ] || finding "CNR N=${cnrn:-unread} != $CNR_EXPECT_N (read the skip list before trusting either)"
[ -z "$cnrskip" ] || finding "CNR platform-exclusive skip list holds new M dir(s): ${cnrskip% }"
# Verify round 1: CNR regenerates in place and a CHANGED golden used to stay in the tree. tM-modules-legs.sh refuses a
# tree with tracked changes (exit 2), so a red leg 4 cost the run CM, the module legs and XS/XM. The rewrites are saved
# as CNR-rewrites.patch (restore_paths) and restored, so every later leg reads HEAD; a restore that leaves something is
# a finding of its own.
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes after CNR (saved as CNR-rewrites.patch, then restored so the later legs read HEAD): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths CNR; }
purge after-CNR

# LEG 5 -- the full behavioral suite (every golib seat reaches it at runtime: G's entry classification under every
# goroutine, the i9's copy-bound receivers under every promoted forwarder, c1-darwin-std-hygiene's builtin.cs call),
# then EACH M guard ISOLATED by name, then the neighbours, then every NOT MEASURED name ISOLATED. TRAIN L's residual
# stands: a deadlock through ordinary channel/sync waits is a HANG, so read the timeout list for that shape; under
# hazard H3 a goroutine G now classes SYSTEM leaves checkdead's user set, which would show as a false deadlock report
# (exit 2) on a program that used to run, or as a hang on one that used to report.
# The i9 read '2956/0/24' (four phases) at L's union + its own project; L's battery read 738 / 738 / 738 / 712 + 26 skip.
leg 5 behavioral powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --build-timeout 10800 --build-one-timeout 900
stamp "  5: $(grep -aiE 'Transpile|Compile|Target|Output|compared|pass|fail|skip' "$LOGDIR/behavioral.log" | tail -n 8 | tr '\r\n' '  ' | cut -c1-600)"
NM=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:timeout[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMBE=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:best-effort[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMT=$(grep -aoE '^\s+\.\.\. and [0-9]+ more\.' "$LOGDIR/behavioral.log" | tail -n 1 | tr -s ' ')
[ -n "$NMT" ] && stamp "  5 NOT-MEASURED LIST TRUNCATED ($NMT) -- the isolated set below is NOT complete; COORD re-runs"
[ -n "$NMBE" ] && stamp "  5 BEST-EFFORT (not isolated: a bigger budget cannot help): [${NMBE% }]"
# The M guards, by seat. i9-crashclass: PromotedPtrMethodValueSet (prints Go's 63 lines). P2: VariadicClosureShadowParam
# (F5), AliasStructToInterface + AliasStructToInterfaceLib (F3, with its control arm), NamedArrayVsUnnamedCompare (F7),
# PanicOnlyFuncLiteralVar + FuncLiteralDeclaredResultIface (F1+F6). H3: TRAIN L's five checkdead guards, re-read under
# G's classification, the first two with the goldens the fixup re-baselined (ForeverWaitWorkersMainReturns,
# MainSelectForeverWorkerExits, MainSelectForeverAfterFunc: Go exit 0; ChannelReceiveFromNil, ChannelSendToNil: stderr
# 'fatal error: all goroutines are asleep - deadlock!', exit 2 on both sides). --filter is a SUBSTRING match: each name
# below matches exactly one project at the pre-map union EXCEPT AliasStructToInterface, which also selects
# AliasStructToInterfaceLib (so that one leg reads two projects, and the Lib leg reads the library alone).
# Verify round 2 (R1): MGUARDS = PRE-D's lists, in one order with no project read twice: the projects the union adds
# (M7DIRS, leg 4's list), the projects whose goldens the fixup commit(s) changed (FXGOLD: derived, so a golden a fixup
# re-baselined is read isolated whichever it is), and the two hazard literals checked against the tree in PRE-D:
# CHECKDEAD_GUARDS (TRAIN L's five) and NEIGH (L's eight, kept for M's reasons: the channel and select projects sit
# beside checkdead, H3; StdoutCloseEofBarrier beside c1-darwin-std-hygiene's builtin.cs; PointerEmbedValueChainPromotion
# beside the i9's forwarders).
MGUARDS=$(printf '%s\n' $M7DIRS $FXGOLD $CHECKDEAD_GUARDS | awk '!seen[$0]++' | tr '\n' ' ')
stamp "  5 named: NOT-MEASURED=[${NM}] :: M-guard mentions on non-pass lines: $(for g in $MGUARDS $NEIGH; do n=$(grep -aE "\b$g\b" "$LOGDIR/behavioral.log" | grep -aciE 'fail|timeout|mismatch|differ'); [ "$n" != 0 ] && printf '%s=%s ' "$g" "$n"; done)"
for p in $(printf '%s\n' $MGUARDS $NEIGH $NM | awk '!seen[$0]++'); do
  leg "B:$p" "behav-$p" powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$p"
  stamp "  B:$p: $(grep -aE '^\s+(Transpile|Compile|Target|Output)\s+pass' "$LOGDIR/behav-$p.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300) :: $(grep -aE '^(PASS|FAIL)|compared' "$LOGDIR/behav-$p.log" | tail -n 1 | cut -c1-120)"
done
# Verify round 2 (both lenses; RULED): round 1's restore after CNR was undone by the next leg. `purge after-CNR` removes
# src/go2cs/bin, leg 5 rebuilds the converter, and the runner then re-transpiles every project IN PLACE (its up-to-date
# test compares each .cs with the exe), so a golden leg 4 read CHANGED is a tracked change again, and the isolated legs
# repeat it. Nothing looked at the tree before MOD, which refuses tracked changes: a red leg 4 still cost the run CM,
# the module legs and XS/XM. Restored here (the patch is kept), with restore_paths' CR-only pass (the handling TRAIN L's
# post-merge re-read measured), and asserted 0 again immediately before MOD.
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes after leg 5 and its isolated legs (the runner re-transpiles in place once a purge removed go2cs.exe; saved as BEH-rewrites.patch, then restored so H7, MOD, PB and the sweeps read HEAD): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths BEH; }
purge after-behavioral

# LEG H7 -- compile parity per flavour. linux: the i9's seat edits internal/syscall/unix/linux/siginfo_linux.cs, which
# compiles ONLY here (and on the linux lanes); golib's LinuxStdDescriptors. darwin: c1-darwin-linkname-pulls' nine files
# (the seat built os.csproj and the route project clean) and c1-darwin-std-hygiene's new builtin.DarwinStdDescriptors.cs.
# All three: G's 76 regenerated corpus files and runtime/managed_impl.cs, the i9's golib and testing.cs, the fixup's
# regenerated corpus files. EXPECT CS=0 x3. A darwin RUN is a mac CI dispatch, never a leg of this box.
export MSYS_NO_PATHCONV=1
for FL in windows linux darwin; do
  leg H7-$FL h7-$FL dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=$FL --no-incremental -m -p:UseSharedCompilation=false
  L="$LOGDIR/h7-$FL.log"
  stamp "  H7-$FL: CS=$(grep -aoE 'error CS[0-9]+' "$L" | wc -l) MSB/NETSDK=$(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$L" | wc -l) :: $(grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$L" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u | head -n 5 | tr '\n' ' ')"
  purge after-H7-$FL
done
unset MSYS_NO_PATHCONV

# LEG MOD -- tM-modules-legs.sh (same env/leg/purge semantics), two halves:
#  (1) TRAIN L's module legs, kept WHOLE as the regression of the -tests -recurse driver, whose test conversion F4
#      (publishTestHost) and F8 (declarationClosureImports) edit: CM (14 module tests + 5 subtests by name with -v),
#      MR2 + JWT (module cache, R6), MR3, MR4 + MR4v + MR4c, MR6 + MR6c, MR6p + MR6e + MR6pc. L's run 2 read all PASS, 920 s.
#  (2) M's REAL MODULES: XS golang.org/x/sync@v0.19.0 and XM golang.org/x/mod@v0.33.0 through
#      'go2cs -tests -recurse <moduleDir> <outRoot>' with -test-timeout $REALMOD_TEST_TIMEOUT stated explicitly. They are
#      P2's fix batch end acceptance (tL-seats-draft.txt:67), read there on a LINUX root box; this is the first WINDOWS
#      reading. Expected per package: x/sync errgroup 5, syncmap 3; x/mod modfile 323, module 16, semver 9,
#      sumdb/dirhash 6, sumdb 4, sumdb/note 7, sumdb/storage 1; zip builds and runs (counts reported). KNOWN
#      non-passes, read BY NAME and stamped KNOWN, never red: semaphore TestWeightedAcquire (the known timing-class
#      non-pass; its cause is NOT MEASURED), and, ONLY for a list without the two host seats, singleflight
#      TestPanicDoChan (host finding H1) and sumdb/tlog TestCertificateTransparency (host finding H2). The frozen list
#      seats both (p2-host-test-list, p2-host-event-line-start): singleflight 12 and sumdb/tlog 17 are expected.
#      Anything else is a FAIL. F4's gate rides here: 0 'dotnet timed out' lines.
#      Verify round 3: the controls MR4c / MR6c run TRAIN L seat converters that carry the H2 seat's stream reader
#      (tM-modules-legs.sh ctlconv), because the union's host frames its --json events and an older reader reads none.
# It needs a CLEAN tree (it refuses otherwise): it runs here, after the H7 purges and before PB/S write anything. Phase
# B builds restore from H:\nuget\packages (warm) -- a cold cache with egress down times out (R's first reading did).
# The script exits 4 on any failed verdict (critic 0 #1 / critic 1 #1), so MOD reaches NONZERO LEGS; each verdict line
# is stamped here on its own line (no 900-char cut can drop END or a FAIL). KNOWN lines are stamped too.
# Outer wall cap 4h (critic 1 #7). IN_BATTERY=1 lets it run under the battery's lock; DEADLINE passes through (R5).
# Verify round 2 (RULED): which of the KNOWN non-passes still applies is read from the SEAT LIST by tM-modules-legs.sh.
# With the H1 host seat in the union (a ref p2-*test-list*, or a row noted H1: p2-host-test-list at the 24-row list)
# singleflight is RUN and 12 of 12 is expected; with no such row it is NOT RUN on this box at all ('NOT RUN (KNOWN: H1,
# child fan-out)': the fan-out was measured unbounded). The H2 seat is read the same way for sumdb/tlog (17 expected).
# Verify round 1: REALMOD_TEST_TIMEOUT is passed on only when COORD set it; its ONE default is tM-modules-legs.sh's.
# Verify round 2 (RULED): tracked changes are asserted 0 HERE, immediately before the module legs (the H7 builds above
# write build output only, but this is the line where a leftover costs MOD). Anything found is restored once more with
# the CR-only pass and stamped; a tree that is still not clean is a finding, and MOD's own refusal then says so too.
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes immediately before MOD (saved as PREMOD-rewrites.patch, then restored): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths PREMOD; }
pmt=$(git status --porcelain | grep -vc '^??')
stamp "  PRE-MOD tracked changes: $pmt (ASSERT 0: tM-modules-legs.sh refuses a tree with tracked changes)"
[ "$pmt" = 0 ] || finding "PRE-MOD: $pmt tracked change(s) remain before the module legs after two restores (CNR's and the behavioral legs' rewrites): MOD will refuse the tree"
# Verify round 2: the deadline travels resolved (DL_EPOCH); DEADLINE is passed for the stamp.
capped MOD modules-legs 4h env EXPECT_HEAD="$EXPECT_HEAD" CONTROLS=1 IN_BATTERY=1 DEADLINE="$DEADLINE" DL_EPOCH="$DL_EPOCH" ${REALMOD_TEST_TIMEOUT:+REALMOD_TEST_TIMEOUT="$REALMOD_TEST_TIMEOUT"} bash "$SD/tM-modules-legs.sh"
modrc=$LEG_RC
while IFS= read -r l; do stamp "  MOD ${l:0:340}"; done < <(grep -aE ': (PASS|FAIL|KNOWN) -- |NOT MEASURED|NOT RUN|host seats read|sumdb/tlog red|REALMOD|^[0-9:]+ (ABORT|END |LEG [^ ]+ ABORT|DEADLINE STOP)' "$SD/mod-logs/SUMMARY.txt" 2>/dev/null | sed 's/^[0-9:]* *//' | tr -d '\r')
[ "$modrc" = 0 ] || stamp "  MOD rc=$modrc (4 = a failed verdict, 2/3 = ABORT, 9 = deadline) -- read mod-logs/SUMMARY.txt"
# Verify round 1: XS/XM are P2's fix batch END ACCEPTANCE, and tM-modules-legs.sh reads a missing module, a failed copy
# or pin and a preflight refusal as NOT MEASURED (never a pass, never a fail: rc 0). Measured on this box: the cache
# holds x/tools v0.42.0 and not v0.41.0, so XM always takes the pin path. Each module that WAS read stamps one
# REALMOD-VERDICT line; fewer than two is a finding, unless COORD skipped the real modules (REALMOD=0).
rv=$(grep -ac 'REALMOD-VERDICT' "$SD/mod-logs/SUMMARY.txt" 2>/dev/null)
[ "${REALMOD:-1}" != 1 ] || [ "${rv:-0}" = 2 ] || finding "MOD real modules: ${rv:-0} of 2 REALMOD-VERDICT lines (XS/XM NOT MEASURED: P2's fix-batch end acceptance was not read on this box; mod-logs/SUMMARY.txt)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after MOD: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"
# Verify round 2 (RULED): XS is the first WINDOWS run of golang.org/x/sync through the -tests -recurse driver, ahead of
# every sweep row and both bank legs. A census of processes BY EXECUTABLE PATH under the module out root (every test
# host of the module legs is published there; the path names them and nothing else). A survivor is a FINDING, listed by
# PID and path, never killed (floor 5), and the lock is KEPT at exit. An unreadable census is a finding too (no pass by
# absence). The module legs' own END runs the same census (its verdict MOD-orphans); this one also covers a MOD that
# aborted before its END.
# Verify round 3 (RULED): a survivor also STOPS THE BATTERY, exit 5, as a fired wall cap does: the timing-sensitive rows
# and the two bank legs never run beside an orphan. A host that is still EXITING when the leg returns is not an orphan,
# so the census is read up to three times, 20 s apart, and the stop is taken on the last reading (each one is stamped).
for orn in 1 2 3; do
  orphans MOD "$SD/out"
  stamp "  MOD-orphans (reading $orn of 3): processes still running from $SD/out after the module legs: $ORPH_N (EXPECT 0; orphans-MOD.txt) $(tr -d '\r' < "$LOGDIR/orphans-MOD.txt" 2>/dev/null | head -n 4 | cut -c1-160 | tr '\n' ' ')"
  [ "$ORPH_N" = 0 ] && break
  [ "$orn" = 3 ] || sleep 20
done
if [ "$ORPH_N" != 0 ]; then
  finding "MOD-orphans: $ORPH_N process(es) still running from $SD/out 40 s after the module legs (listed by PID and path in orphans-MOD.txt, NOT killed; 'unread' = the census itself failed three times); the lock is KEPT and the battery STOPS"
  KEEP_LOCK=1
  stamp "NONZERO LEGS so far: $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ') :: FINDINGS so far: ${FINDINGS:-none}"
  stamp "MOD-ORPHANS STOP (RULED): no purge (a live orphan may hold files); lock KEPT; exit 5. COORD kills the listed PIDs by PID (floor 5), rmdir's the lock, and launches a FRESH battery from a fresh run folder: no S row and no bank leg was run beside the orphan"
  exit 5
fi
purge after-MOD

# (TRAIN L's legs HOP and SPB are not carried: no M seat touches src/run-validated-sweep.ps1, and G's edit to the
# src/_roster.ps1 it dot-sources is a comment, read under both editions by G1/G2, ST and NV. tM-README.md.)
#
# LEG PB -- F4 (p2-test-publish-timeout) rewrites publishTestHost's deadline: the publish alone now gets
# max(-test-timeout, 30m). That is the function TRAIN L's c2-binlog seat composed with withPublishBinlog, so L's live
# arm stays as the reading of the two together: -test-publish-binlog over cmp (banked 4). EXPECT rc=0, the same
# Validated line as the sweep, and src/core/cmp/bin/tests/publish.binlog ABSENT afterwards (a passing publish deletes
# it). Read BEFORE any purge (the log lives under bin/).
# Critic 0 #3: 'absent afterwards' reads the same whether -bl was passed and the log deleted or never passed (the dotnet
# argv is not echoed), so a background poll records whether the binlog EXISTED during the leg: EXPECT SEEN.
export MSYS_NO_PATHCONV=1
convbuild PB
# Verifier: a binlog left over from an earlier publish would read SEEN before PB's own publish starts. Record it
# (pre=PRESENT is a FINDING) and delete it before the poll starts.
pbpre=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent); rm -f src/core/cmp/bin/tests/publish.binlog
poll_start "$W/src/core/cmp/bin/tests/publish.binlog" "$LOGDIR/pb-binlog.seen"
leg PB publish-binlog "$EXE" -tests -test-action all -test-timeout 10m -test-config Release -test-publish-binlog -go2cspath "$WB\\src" "$GOROOT\\src\\cmp" "$WB\\src\\core\\cmp"
pbrc=$LEG_RC; poll_stop
pbseen=$([ -e "$LOGDIR/pb-binlog.seen" ] && echo SEEN || echo never-seen)
pbleft=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent)
stamp "  PB (EXPECT rc=0, Validated 4, pre=absent, binlog SEEN during, absent after): rc=$pbrc :: $(grep -aE 'Validated [0-9]+ tests' "$LOGDIR/publish-binlog.log" | tail -n 1 | cut -c1-160) :: pre=$pbpre during=$pbseen after=$pbleft :: 'publish.binlog' in log: $(grep -ac 'publish.binlog' "$LOGDIR/publish-binlog.log") (INFERRED 0: the argv is not echoed)"
[ "$pbpre" = absent ] || finding "PB pre=PRESENT: a publish.binlog already existed before PB (deleted before the poll)"
[ "$pbrc" = 0 ] && [ "$pbseen" = SEEN ] && [ "$pbleft" = absent ] || finding "PB -test-publish-binlog (rc=$pbrc pre=$pbpre during=$pbseen after=$pbleft)"
# Verify round 2: the stamp said 'EXPECT Validated 4' and nothing compared the count. The row's banked N + D is read
# from the roster at HEAD (rosterrow: cmp is banked 4 + 0), never typed here.
pbv=$(grep -aE 'Validated [0-9]+ tests' "$LOGDIR/publish-binlog.log" | tail -n 1 | sed -nE 's/.*Validated ([0-9]+) tests.*/\1/p'); pbd=$(grep -aE 'Validated [0-9]+ tests' "$LOGDIR/publish-binlog.log" | tail -n 1 | sed -nE 's/.*[^0-9]([0-9]+) disclosed-divergent.*/\1/p')
pbr=$(hp rosterrow "$WW" cmp "${pbv:-None}" "${pbd:-0}" 2>&1 | tr -d '\r' | tail -n 1)
stamp "  PB count vs the roster: $pbr"
case "$pbr" in *'=> AT BANKED COUNTS') ;; *) finding "PB: cmp did not validate its banked count ($pbr)" ;; esac
unset MSYS_NO_PATHCONV
restore_paths PB
purge after-PB

# LEG S -- the banked rows this box sweeps (tM-i7-sweeps.txt minus T_ROWS, plus derived canaries / PRE-2 MISSING, plus
# X_ROWS). One row at a time, exact match. EVERY row is read at the execution config its ROSTER row carries (the sweep
# takes -test-config / -test-tiered from the row), so G's two roster commits ARE this leg's change:
#   net/http          now the row default, Release with tiering OFF: EXPECT PASS 1387, rc 0 (the TestMain leak check is
#                     what TC0 exposed: clean), TestRegisterErr and TestRegisterErr/a pass/pass by name, tiered=False
#   internal/godebug  (an i9 row, read here too) TC0: EXPECT PASS 5, TestCmdBisect pass/pass, tiered=False
#   log/slog          (an i9 row, read here too) KEEPS release-tiered: EXPECT PASS 199 + 17, tiered=True; TestSetDefault,
#                     TestPanics and TestCallDepth by name (the three that decided it at L)
#   net/rpc, net/rpc/jsonrpc   hazard H3: TestSendDeadlock by name, 0 'all goroutines are asleep' lines
# G's line-attribution A/B named 17 rows ('any pass->fail blocks'). This box reads nine: encoding/base64, net, net/http,
# net/http/cgi, net/http/httputil, reflect, runtime/debug here, runtime and runtime/pprof in the T legs, plus
# internal/godebug and log/slog as X rows. The i9 reads the other eight (context, encoding/json, go/build, io, log,
# log/slog, sync, internal/godebug). Each is gated by its PASS line at the banked count. (Verify round 1: encoding/json
# is NOT a canary, as round 0 said: TRAIN L derived crypto/cipher, runtime, crypto/tls, net/http, go/types. It is an i9
# row. Verify round 2, RULED: it is in X_ROWS, so this box reads it too: ten of the 17 here, with eight on the i9 and
# internal/godebug, log/slog and encoding/json on both.)
# Every row's host also runs the i9's [GoRecv] testing.cs logging methods: the i7's rows were never read with that seat.
# A for-loop over the list, never `while read < file` (powershell.exe inside freegb would eat rows).
for pkg in $(cat "$RUNLIST"); do
  n=$(echo "$pkg" | tr '/' '.')
  recclean "src/core/$pkg"   # verify round 2: the record this row leaves is THIS row's (see recclean)
  capped "S:$pkg" "sweep-$n" "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  src=$LEG_RC; st0=$LEG_T0
  dl=$(dlcount "S:$pkg" "$LOGDIR/sweep-$n.log" "src/core/$pkg" "$st0")
  # Verify round 2: a row that exits 0 has reached its compare and written its record (TRAIN L's run 2: 94 of 94 first
  # readings read record=fresh). With the record deleted before the row, rc 0 beside no fresh record is a row whose PASS
  # was not read from this run's compare.
  if [ "$src" = 0 ]; then case "$dl" in *'record=fresh'*) ;; *) finding "S:$pkg rc=0 and NO fresh comparison record after the row (${dl:-DEADLOCK unread}): the PASS line is not backed by this run's record" ;; esac; fi
  # Verify round 1 (TRAIN L's template lesson, tL-seats-draft.txt:40): the converted HOST's own elapsed for this row,
  # from its fresh results record (the row wall is mostly MSBuild and publish). A reading; TRAIN N's baseline.
  hw=$(hp hostwall "$st0" "$WW\\src\\core\\$(echo "$pkg" | tr '/' '\\')\\go2cs_test_results.json" 2>&1 | tr -d '\r' | head -n 1)
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT|RERUN)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200) :: ${dl:-DEADLOCK unread} :: ${hw:-HOSTWALL unread}"
  case "$pkg" in net/rpc|net/rpc/jsonrpc) echo "$dl" | grep -q 'DEADLOCK total=0 .*record=fresh' || finding "S:$pkg deadlock reading is not 'total=0' on a FRESH record (no gate passes by absence): $dl" ;; esac
  case "$pkg" in
    net/rpc) named='TestSendDeadlock' ;;
    net/http) named='TestRegisterErr' ;;
    internal/godebug) named='TestCmdBisect' ;;
    log/slog) named='TestSetDefault,TestPanics,TestCallDepth' ;;
    *) named='' ;;
  esac
  want_tiered=''
  for x in $TC0_ROWS; do [ "$x" = "$pkg" ] && want_tiered=False; done
  for x in $TIERED_ROWS; do [ "$x" = "$pkg" ] && want_tiered=True; done
  if [ "$src" != 0 ] || [ -n "$named" ] || [ -n "$want_tiered" ]; then
    hp treport --dir "$WW\\src\\core\\$(echo "$pkg" | tr '/' '\\')" --log "$LOGW\\sweep-$n.log" --t0 "$st0" --rc "$src" --label "S:$pkg" --named "$named" > "$LOGDIR/sweep-$n.reading.txt" 2>&1
    while IFS= read -r l; do stamp "    $l"; done < <(grep -aE 'FRESHNESS|ERROR-NAMES|NAMED | ENV ' "$LOGDIR/sweep-$n.reading.txt" | cut -c1-400 | head -n 14)
    [ -f "src/core/$pkg/go2cs_test_comparison.json" ] && cp "src/core/$pkg/go2cs_test_comparison.json" "$LOGDIR/sweep-$n.go2cs_test_comparison.json"
    if [ -n "$want_tiered" ]; then
      # The record must be THIS row's (fresh) and must state the config the roster now carries.
      grep -aq "ENV configuration=Release tiered=$want_tiered" "$LOGDIR/sweep-$n.reading.txt" && grep -a 'FRESHNESS' "$LOGDIR/sweep-$n.reading.txt" | grep -q 'comparison=[0-9:]* (fresh)' \
        || finding "S:$pkg execution config: expected a fresh record reading Release tiered=$want_tiered, read '$(grep -a ' ENV ' "$LOGDIR/sweep-$n.reading.txt" | tr -d '\r' | cut -c1-120)'"
    fi
  fi
done
# Verify round 1: only three rows carry want_tiered above, so nothing said that no OTHER swept row ran tiered. The sweep
# prints the row's execution config on its verdict line (TRAIN L: 'PASS  net/http  1387 [release-tiered] [323s]', the
# one such line in its 103 sweep logs): the rows whose PASS line carries the tag must be exactly TIERED_ROWS.
trl=''
for pkg in $(cat "$RUNLIST"); do
  grep -aqE '^\s*PASS .*\[release-tiered\]' "$LOGDIR/sweep-$(echo "$pkg" | tr '/' '.').log" 2>/dev/null && trl="$trl$pkg "
done
[ "${trl% }" = "$TIERED_ROWS" ] || finding "S rows whose PASS line carries [release-tiered]: [${trl% }] (expected exactly: $TIERED_ROWS)"

# LEG NR -- hazard H3 (TRAIN L's g-deadlock leg, kept for M's reason): net/rpc four MORE times (5 in all with the S row).
# TRAIN L's checkdead reports a deadlock when EVERY user goroutine is in a forever wait; G's seat changes which
# goroutines count as user (IsSystem by the start function, not the creator frame). net/rpc's TestSendDeadlock leaves a
# leaked select{} and was the row where K's host died at shutdown; the outcome is timing-dependent, hence the loop.
# EXPECT each PASS 15, rc 0, 0 'all goroutines are asleep' lines.
# Critic 0 #16: the count reads the console, the 'full output:' file and the fresh record's stderr tails (dlcount).
# Verify round 2 (HIGH): at TRAIN L all four repeats read 'record=STALE' on rc 0 (a byte-identical record keeps its
# mtime), so the stderr tails were never read and the gate passed on the console and full-output counts, which are
# structurally 0 on a PASS. The record is deleted before each repeat and the gate wants total=0 ON A FRESH RECORD.
for i in 2 3 4 5; do
  recclean src/core/net/rpc
  capped "NR:$i" "sweep-net.rpc-nr$i" "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter net/rpc -Exact
  dl=$(dlcount "NR:$i" "$LOGDIR/sweep-net.rpc-nr$i.log" src/core/net/rpc "$LEG_T0")
  stamp "  NR:$i: $(grep -aE '^\s*(PASS|FAIL|COUNT|DRIFT)' "$LOGDIR/sweep-net.rpc-nr$i.log" | tail -n 1 | cut -c1-160) :: ${dl:-DEADLOCK unread}"
  echo "$dl" | grep -q 'DEADLOCK total=0 .*record=fresh' || finding "NR:$i deadlock reading is not 'total=0' on a FRESH record (no gate passes by absence): $dl"
done

# LEG TE + HS + UF -- post-sweep readings, no new runs (K ruling (4): sweep-rewritten test sources are readings only).
#   TE: the rewritten sources' hunks by class (tM-helpers.py teattr). G's frame class (the NoInlining prefix on a
#      function that executes a go or sits in a constant-skip Caller window, its using line, the position-map re-encode)
#      is EXPECTED here: G regenerated no committed *_test.cs, and 158 of them hold a go launch. Every OTHER hunk is
#      compared with TRAIN L's own sweep patch from this box (L_REWRITES): the hunks L did not carry are LISTED. A
#      READING, not gated: what M's seats may move in test sources is not ruled (tM-README.md, open question).
#   HS (regression: L's r-m4, r-m6): no rewritten stdlib host carries a module-path argument; GoDefaultGodebug still
#      names exactly 3 files. csproj among the rewrites: EXPECT 0 on this box.
#   UF (c1-fixture-tracking, 'e2e 0 untracked'): a -tests run stages fixtures into src/core; the seat committed the ten
#      that were neither tracked nor ignored. EXPECT 0 untracked, not-ignored files under src/core after the S rows.
git diff -U0 > "$LOGDIR/S-rewrites-U0.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/S-tracked-changes.txt"
hp teattr --baseline "$(cygpath -w "$L_REWRITES")" "$(cygpath -w "$LOGDIR/S-rewrites-U0.patch")" > "$LOGDIR/te.log" 2>&1; terc=$?
stamp "  TE rc=$terc (a READING) :: $(grep -aE '^TE (files|BASELINE|VERDICT)' "$LOGDIR/te.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-700)"
hsa=$(hs_m4); hsb=$(hs_m6)
stamp "  HS: module-path hosts (worktree)=$hsa (want 0) GoDefaultGodebug files=[${hsb% }] ($([ "$hsb" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'MOVED')) :: tracked rewrites=$(wc -l < "$LOGDIR/S-tracked-changes.txt") csproj among them=$(grep -c 'csproj' "$LOGDIR/S-tracked-changes.txt") go2cs_test_host.cs among them=$(grep -c 'go2cs_test_host.cs' "$LOGDIR/S-tracked-changes.txt")"
[ "$terc" = 0 ] || finding "TE (sweep rewrites) rc=$terc: the reader did not read (te.log)"
[ "$hsa" = 0 ] && [ "$hsb" = "$HS_WANT" ] || finding "HS after the sweep: module-path hosts=$hsa GoDefaultGodebug=[${hsb% }]"
# Verify round 1: F8 (p2-test-overload-references) changes the references a regenerated test project gets, and every S
# row regenerates its .tests.csproj under the union converter; its reach into the committed ones is NOT MEASURED
# (seat-footprints 3a). TRAIN L's run 2 read 0 csproj here and 0 in HS-T: any is a finding, read before the restore
# below removes it from the tree (S-rewrites-U0.patch keeps the diff).
ncs=$(grep -c 'csproj' "$LOGDIR/S-tracked-changes.txt"); [ "$ncs" = 0 ] || finding "HS: $ncs csproj among the sweep's tracked rewrites (TRAIN L read 0; F8's reference edge or a host-project change; read S-rewrites-U0.patch): $(grep 'csproj' "$LOGDIR/S-tracked-changes.txt" | head -n 4 | tr '\n' ' ')"
git status --porcelain -- src/core | grep '^??' > "$LOGDIR/S-untracked-core.txt"
ufs=$(grep -c . "$LOGDIR/S-untracked-core.txt")
stamp "  UF after the sweeps: untracked, not-ignored paths under src/core=$ufs (EXPECT 0) $(head -n 6 "$LOGDIR/S-untracked-core.txt" | tr '\n' ' ' | cut -c1-400)"
[ "$ufs" = 0 ] || finding "UF after the sweeps: $ufs untracked, not-ignored path(s) under src/core (S-untracked-core.txt)"
# Critic 1 #3: the sweep's rewrites are saved (the -U0 patch + list above, and restore_paths' binary patch) and then
# RESTORED, so the T legs start from HEAD's sources. restore_paths now also resets the CR-only files L's run left
# behind, and S_LEFT keeps what it MEASURED for the END line. Then the purge (which would otherwise abort on a sweep's
# legitimate ' D').
restore_paths S
S_LEFT=$RESTORE_LEFT
purge after-sweep

# LEG T -- direct -tests (K's tleg) for the two rows read here and never by the sweep, each compared with the roster
# of record (rosterrow). Both are among the 17 rows of G's line-attribution A/B ('any pass->fail blocks').
#  T:runtime/pprof (banked 145 + 7): G's PC->line fix is in runtime/managed_impl.cs (the line a frame resolves to), and
#    its commit names TestMemoryProfiler (+ /debug=1): read BY NAME. TRAIN L's checkdead takes s_profileGate EXCLUSIVE,
#    the goroutine profile's gate, and now meets G's classification (H3). BANK-READING + roster compare.
#  T:runtime (banked 10819 + 71; TRAIN L's battery read it in 5891 s): THE re-read the seat draft assigns to this
#    battery (tL-seats-draft.txt:59, tM-seats-draft.txt:20). On G's branch ALONE P2 read TestTracebackSystem/panic RED,
#    'deadlock-first', because the branch lacked TRAIN L's checkdead; on TRAIN L alone it read DISCLOSED. This is the
#    first tree that holds both: EXPECT /panic and /trap DISCLOSED-divergent on their pinned signatures, by name, the
#    crash family pass/pass, and TestLineNumber DISCLOSED (G's commit names it). The banked counts must not move.
#  TBS-W: ONE windows control iteration of the TestTracebackSystem filter (P2's TBS-LOOP x10 is the linux arm). A
#    filtered run is DIAGNOSTIC ONLY (never a bank reading). It writes the same src/core/runtime record files, so it
#    runs BEFORE T:runtime (critic 1 #2): the bank reading is the LAST record left in the tree for the bank step.
tbs_named(){ # reading file, label -- M: TestTracebackSystem/panic AND /trap are in THIS leg's record and read DISCLOSED
  # Verify round 2: (1) treport prints NAMED lines whenever a record EXISTS, so a host that died before its compare read
  # '/panic disclosed' from the record an earlier run left in src/core/runtime: the reading counts only beside a FRESH
  # comparison record (and tleg deletes the record before the leg). (2) /trap was stated and never gated; TRAIN L's run 2
  # read both disclosed (go=pass cs=fail disclosed) in TBS-W and in T:runtime.
  local pl tl
  grep -a 'FRESHNESS' "$1" | grep -q 'comparison=[0-9:]* (fresh)' || { finding "$2: no FRESH comparison record after the leg: the by-name reading of TestTracebackSystem is NOT MEASURED ($(grep -a 'FRESHNESS' "$1" | tr -d '\r' | cut -c1-160))"; return; }
  pl=$(grep -a 'NAMED TestTracebackSystem/panic:' "$1" | head -n 1 | tr -d '\r' | sed 's/ *$//')
  tl=$(grep -a 'NAMED TestTracebackSystem/trap:' "$1" | head -n 1 | tr -d '\r' | sed 's/ *$//')
  stamp "  $2 by name: [${pl:-TestTracebackSystem/panic ABSENT from the record}] [${tl:-TestTracebackSystem/trap ABSENT from the record}]"
  case "$pl" in *" disclosed") ;; *) finding "$2: TestTracebackSystem/panic does not read DISCLOSED (${pl:-absent from both sides})" ;; esac
  case "$tl" in *" disclosed") ;; *) finding "$2: TestTracebackSystem/trap does not read DISCLOSED (${tl:-absent from both sides})" ;; esac
}
export MSYS_NO_PATHCONV=1
convbuild T
tleg(){ # pkg, timeout, suffix, named-prefixes, bank(yes|no), extra args...   (K verbatim + the roster compare)
  local pkg=$1 to=$2 suf=$3 named=$4 bank=$5; shift 5
  local pb n L trc tt0 rd
  deadline_check "T:$pkg:$suf"   # R5 (leg() checks again; this stops before any tleg bookkeeping)
  pb=$(echo "$pkg" | tr '/' '\\'); n=$(echo "$pkg" | tr '/' '.')
  L="$LOGDIR/tests-$n-$suf.log"; rd="$LOGDIR/tests-$n-$suf.reading.txt"
  recclean "src/core/$pkg"   # verify round 2: after the deadline check, before the leg: the record this leg leaves is its own (TBS-W's is gone before T:runtime writes the bank reading)
  leg "T:$pkg:$suf" "tests-$n-$suf" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  trc=$LEG_RC; tt0=$LEG_T0
  [ -f "src/core/$pkg/go2cs_test_comparison.json" ] || finding "T:$pkg:$suf left NO comparison record (the run did not reach its compare): a FAILED leg, whatever its rc ($trc)"
  if [ "$bank" = yes ]; then
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" --bank > "$rd" 2>&1
    v=$(grep -aoE 'BANK-READING rc=[0-9]+ validated=[0-9None]+ disclosed-divergent=[0-9]+' "$rd" | sed -E 's/.*validated=([0-9None]+) disclosed-divergent=([0-9]+)/\1 \2/')
    hp rosterrow "$WW" "$pkg" ${v:-None 0} >> "$rd" 2>&1
    # Verify round 1: the two BANK legs could not red on a moved count. rosterrow's rc was dropped and 'MOVED' or
    # 'BANK-ELIGIBLE NO' with a converter rc of 0 reached neither end list, on the train whose PC->line fix is exactly
    # the kind of change that turns a disclosed-divergent test into a pass (N+1 / D-1, rc 0). "The banked counts must
    # not move" (runtime 10819 + 71, runtime/pprof 145 + 7) is now two findings.
    grep -aq '=> BANK-ELIGIBLE YES' "$rd" || finding "T:$pkg bank reading is not BANK-ELIGIBLE: $(grep -ao 'BANK-READING.*' "$rd" | tr -d '\r' | cut -c1-220)"
    grep -aq '=> AT BANKED COUNTS' "$rd" || finding "T:$pkg roster compare: $(grep -a '^ROSTERROW' "$rd" | tr -d '\r' | cut -c1-160)"
  else
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" > "$rd" 2>&1
  fi
  while IFS= read -r l; do stamp "  $l"; done < <(grep -aE 'FRESHNESS|RESULTS-TAIL|VALIDATED-LINE|COMPARISON|ERROR-NAMES|NAMED | ENV |BANK-READING|ROSTERROW' "$rd" | cut -c1-500 | head -n 90)
  # Verify round 1: the host's own elapsed beside TRAIN L's kept record of the SAME leg (run 2 kept tests-runtime-all,
  # tests-runtime.pprof-all and tests-runtime-tbs): the comparison TRAIN L's lesson asked for, where a baseline exists.
  stamp "  T:$pkg:$suf $(hp hostwall "$tt0" "$WB\\src\\core\\$pb\\go2cs_test_results.json" "$(cygpath -w "$(dirname "$L_SUMS")/tests-$n-$suf.go2cs_test_results.json")" 2>&1 | tr -d '\r' | head -n 1)"
  for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$n-$suf.$f"; done
}
CRASH='TestSimpleDeadlock,TestInitDeadlock,TestLockedDeadlock,TestLockedDeadlock2,TestGoexitDeadlock,TestGoexitCrash,TestGoNil,TestGoexitInPanic,TestPanicAfterGoexit,TestRecoveredPanicAfterGoexit,TestRecoverBeforePanicAfterGoexit,TestRecoverBeforePanicAfterGoexit2,TestMainGoroutineID,TestNoHelperGoroutines,TestNetpollDeadlock,TestPanicDeadlockGosched,TestPanicDeadlockSyscall,TestPanicLoop,TestStopTheWorldDeadlock,TestCrashHandler,TestTracebackSystem'
# K's named list verbatim (TestBlockMutexProfileInlineExpansion and TestMutexProfile read 'absent from both sides' at
# K's re-read) + TestBlockProfile (checkdead's exclusive profile gate sits beside the block/mutex profile readers)
# + M: TestMemoryProfiler (the test G's PC->line commit 2acbc7144f names; its /debug=1 subtest prints under the prefix).
tleg runtime/pprof 30m all 'TestGoroutineProfileConcurrency,TestGenericsHashKeyInPprofBuilder,TestGenericsInlineLocations,TestHeapRuntimeFrames,TestBlockMutexProfileInlineExpansion,TestMutexProfile,TestBlockProfile,TestMemoryProfiler' yes
# TBS-W BEFORE T:runtime (critic 1 #2). Its deadlock count reads the console, any full-output file and the fresh
# record's stderr tails (critic 0 #16); 'all goroutines are asleep' must not appear (the /panic child's expected text is
# its own pinned signature, read through the disclosed-divergent verdict).
BANK_IN_TREE=1   # runtime/pprof's bank reading is in the tree from here on: a deadline stop must not revert it
tleg runtime 30m tbs 'TestTracebackSystem' no -test-filter '^TestTracebackSystem$'
tbsrc=$LEG_RC
TBS_IN_TREE=1    # verify round 1: src/core/runtime now holds TBS-W's FILTERED record until T:runtime overwrites it (deadline_check says so)
tbsdl=$(dlcount "TBS-W" "$LOGDIR/tests-runtime-tbs.log" src/core/runtime "$LEG_T0")
stamp "  TBS-W (EXPECT rc 0, /panic and /trap disclosed-divergent, 0 deadlock lines on a fresh record): rc=$tbsrc ${tbsdl:-DEADLOCK unread}"
echo "$tbsdl" | grep -q 'DEADLOCK total=0 .*record=fresh' || finding "TBS-W deadlock reading is not 'total=0' on a FRESH record (no gate passes by absence): $tbsdl"
# Verify round 2: TRAIN L's run 2 read TBS-W at rc 0 (Validated 0 + 3 disclosed-divergent), so a non-zero TBS-W at M is
# NEW (a host death, an orphaned disclosure, /trap undisclosed) on the first tree that holds checkdead and G's entry
# classification together. The END legend no longer lists it among the expected non-zero legs.
[ "$tbsrc" = 0 ] || finding "TBS-W rc=$tbsrc (TRAIN L's run 2 read rc 0: a red to read, not a filtered-diagnostic default)"
tbs_named "$LOGDIR/tests-runtime-tbs.reading.txt" TBS-W
tleg runtime 150m all "TestRuntimeLockMetricsAndProfile,TestTracebackParentChildGoroutines,TestFinalizerRegisterABI,TestCleanupAfterFinalizer,TestPeriodicGC,TestNumCPU,TestLineNumber,$CRASH" yes
TBS_IN_TREE=0
tbs_named "$LOGDIR/tests-runtime-all.reading.txt" "T:runtime"
unset MSYS_NO_PATHCONV

# LEG TE-T + HS-T + UF-T (critic 0 #6) -- the TE, HS and UF readings repeated over the T legs' rewrites (the sweep's were
# restored before the T legs, so this patch is runtime + runtime/pprof only: the largest test corpus, and the hosts
# whose runtime init reads the entry-assembly attribute). The TE baseline is TRAIN L's own T-leg patch. No restore: the
# T legs' bank material stays in the tree for the bank step.
git diff -U0 > "$LOGDIR/T-rewrites-U0.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/T-tracked-changes.txt"
hp teattr --baseline "$(cygpath -w "$L_REWRITES_T")" "$(cygpath -w "$LOGDIR/T-rewrites-U0.patch")" > "$LOGDIR/te-T.log" 2>&1; tetrc=$?
stamp "  TE-T rc=$tetrc (a READING) :: $(grep -aE '^TE (files|BASELINE|VERDICT)' "$LOGDIR/te-T.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-700)"
hsta=$(hs_m4); hstb=$(hs_m6)
stamp "  HS-T: module-path hosts (worktree)=$hsta (want 0) GoDefaultGodebug files=[${hstb% }] ($([ "$hstb" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'MOVED')) :: tracked rewrites=$(wc -l < "$LOGDIR/T-tracked-changes.txt") csproj among them=$(grep -c 'csproj' "$LOGDIR/T-tracked-changes.txt")"
[ "$tetrc" = 0 ] || finding "TE-T (T-leg rewrites) rc=$tetrc: the reader did not read (te-T.log)"
[ "$hsta" = 0 ] && [ "$hstb" = "$HS_WANT" ] || finding "HS-T after the T legs: module-path hosts=$hsta GoDefaultGodebug=[${hstb% }]"
ncst=$(grep -c 'csproj' "$LOGDIR/T-tracked-changes.txt"); [ "$ncst" = 0 ] || finding "HS-T: $ncst csproj among the T legs' tracked rewrites (TRAIN L read 0; read T-rewrites-U0.patch): $(grep 'csproj' "$LOGDIR/T-tracked-changes.txt" | head -n 4 | tr '\n' ' ')"
git status --porcelain -- src/core | grep '^??' > "$LOGDIR/T-untracked-core.txt"
uft=$(grep -c . "$LOGDIR/T-untracked-core.txt")
stamp "  UF-T after the T legs: untracked, not-ignored paths under src/core=$uft (EXPECT 0) $(head -n 6 "$LOGDIR/T-untracked-core.txt" | tr '\n' ' ' | cut -c1-400)"
[ "$uft" = 0 ] || finding "UF-T after the T legs: $uft untracked, not-ignored path(s) under src/core (T-untracked-core.txt)"

# LEG TE-i9 (critic 0 #5) -- the i9 shard's patch through the same TE reader, scripted (tM-i9-te.sh). Runs here only
# when I9_PATCH names a file that has arrived; otherwise COORD runs `bash tM-i9-te.sh <patch> [<baseline>]` from this
# run folder when it lands (same reader: a READING; rc != 0 means the reader did not read).
if [ -n "$I9_PATCH" ] && [ -f "$I9_PATCH" ]; then
  bash "$SD/tM-i9-te.sh" "$I9_PATCH" $I9_BASELINE > "$LOGDIR/te-i9.log" 2>&1; LEG_RC=$?
  stamp "  TE-i9 rc=$LEG_RC (a READING) :: $(grep -aE '^TE (files|BASELINE|VERDICT)' "$LOGDIR/te-i9.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-700)"
  [ "$LEG_RC" = 0 ] || finding "TE-i9 rc=$LEG_RC: the reader did not read (te-i9.log)"
else
  stamp "  TE-i9: NOT RUN here (I9_PATCH=${I9_PATCH:-unset}) -- run 'bash tM-i9-te.sh <tracked-changes-U0.patch> [<the i9's TRAIN L patch>]' from this run folder when the i9's patch arrives"
fi

# LEG TL-WALL -- each ROW leg's wall vs the fastest passing reading of the same leg in TRAIN L's battery of record (run
# 2). M's cost-bearing seats: the i9's copy-bound receivers and [GoRecv] on 25 testing.cs methods (every host), G's
# NoInlining on every go-executing function and its IL scan for the line a frame resolves to. TRAIN L's lesson stands
# (tL-seats-draft.txt:40): row wall is mostly MSBuild and publish, so a ratio > 1.25 is a prompt to read, never a gate.
# Verify round 1: the lesson's own comparison, the converted host's package elapsed, is the HOSTWALL reading stamped on
# every S row (no L baseline: L kept no per-row record) and on the three T legs beside L's kept record. The list below
# stays, labelled for what it is.
hp wallcmp "$(cygpath -w "$SUM")" $(for k in $L_SUMS; do [ -f "$k" ] && cygpath -w "$k"; done) > "$LOGDIR/wallcmp.log" 2>&1
while IFS= read -r l; do stamp "  $l (ROW wall: build and publish noise included; read the HOSTWALL lines for the host's own elapsed)"; done < <(grep -a '^WALLCMP' "$LOGDIR/wallcmp.log" | head -n 25)

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') (the T legs' material, plus whatever the after-sweep restore left: it MEASURED tracked-left=${S_LEFT:-unread}) tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD) SIc=[$SICV] E=[attr ${EATTR:-unread}] GT-NOT-FOUND=[${GT_NF% }] SyncMutexProfile=[${GT_SMP% }] 5-TRUNCATED=[${NMT:-no}]"
stamp "NONZERO LEGS (SIc, NVR and FXc1/FXc2 are EXPECTED non-zero controls; every other non-zero leg is a red to read, TBS-W included; NGb51 is GATED; MOD is non-zero only on a FAIL verdict, its KNOWN lines are not reds): $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ')"
stamp "FINDINGS (gates that read wrong without a nonzero rc): ${FINDINGS:-none}"
stamp "BATTERY DONE"
# Verify round 2 (RULED; merge-hazards: "end a launch wrapper with `exit \$rc`, never with the command that reports"):
# the last command used to be the stamp above, so a launcher, a task notice or an `&&` chain read 0 on a red battery.
# Exit 6 when any leg OUTSIDE the documented expected-non-zero set (the four controls) is non-zero, or any FINDING
# exists. The EXIT trap keeps the status. (2 = abort, 3 = a leg's disk or build abort, 5 = a wall cap, 9 = the deadline.)
NZ=$(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | grep -vE '^(SIc|NVR|FXc1|FXc2) ' | tr '\n' ' ')
if [ -n "$FINDINGS" ] || [ -n "$NZ" ]; then
  stamp "EXIT 6: unexpected non-zero legs=[${NZ% }] findings=$NFIND (listed on the FINDINGS line above)"
  exit 6
fi
stamp "EXIT 0: no leg outside the four expected-non-zero controls is non-zero, and no finding"
exit 0
