#!/usr/bin/env bash
# TRAIN P battery on the i7 -- the union of record (COORD). TRAIN P IS THE RELEASE TRAIN for go.* 1.24.13.4 (owner
# ruling 2026-10-04 10:05). DERIVED 2026-10-04 from trainO/tO-battery.sh (TRAIN O's battery, bat2: coord-scratch/tO/bat2,
# launched 06:37 on 2026-10-04, RUNNING at the derive, 0 findings at 12:16), itself DERIVED 2026-10-03 from
# trainN/tN-battery.sh, from trainM/. Every change against O is in trainP/tP-CHANGES.md (R0, P1..); O's against N stay
# in trainO/tO-CHANGES.md (O1.., BATTERY STOP 1). NOT RUN by its author.
# Launch from a PER-RUN COPY (floor 4): copy this folder (tP-battery.sh, tP-emitcheck.sh, emitdrift.py, tP-helpers.py,
# tP-modules-legs.sh, tP-i9-te.sh, tP-ng-parse.ps1, tP-follow.txt, tP-i7-sweeps.txt, tP-i9-shard.txt) AND the seat list
# (hnd .claude/coord-scripts/trainP/tP-seats-draft.txt) to a FRESH run folder (PRE refuses a folder that already holds a
# SUMMARY.txt) and run the copy:
#   EXPECT_HEAD=<fixup sha, 10 chars> MASTER=<TRAIN O's landed master> [O_RUN=<TRAIN O's battery run folder>] \
#     [DEADLINE='YYYY-MM-DD HH:MM'] [I9_PATCH=<i9 tracked-changes-U0.patch>] [CNR_EXPECT_N=<n>] [REALMOD_TEST_TIMEOUT=2m] bash tP-battery.sh
# MASTER is REQUIRED (O1: no script of this set carries a base sha; O had not landed at the derive) and must sit on the
# union's first-parent line. O_RUN defaults to O's battery of record as the derive knew it (coord-scratch/tO/bat2: bat1
# aborted at a purge with no test failed, BATTERY STOP 1); its SUMMARY and patches are the previous-train baselines (FX
# floor, CNR control, TE / TE-T, HOSTWALL, TL-WALL), stamped. At O the variable was N_RUN: a launch that still sets
# N_RUN is REFUSED (P3), so a command line copied from O cannot silently read the default.
# A ruled FOLLOW-UP merge is listed in tP-follow.txt, and its origin ref is fetched into the tP worktree BEFORE launch:
# the battery reads that remote-tracking ref and never fetches.
# Logs land beside the copy in tP-logs/ (the module legs in mod-logs/).
# DEADLINE (R5; verify round 2, RULED): no leg STARTS after it. An ABSOLUTE local time, 'YYYY-MM-DD HH:MM', resolved
# once to epoch seconds at launch; a deadline already in the past is refused; unset = launch + 14 hours, stamped.
# ONE BATTERY AT A TIME (floors 1, 11): PRE takes /h/go2cs-tmp-coord/coord-scratch/tP/.battery.lock (mkdir) and the
# EXIT trap releases it; tP-modules-legs.sh and tP-fixup.sh take the same lock.
# UNION SHAPE (PRE refuses anything else): TRAIN O's landed master + one signed merge per row of the seat list, in row
# order (22 rows drafted at 14:05 on 2026-10-04; the count is READ from the list) [+ the ruled follow-up merges of
# tP-follow.txt: EMPTY at the draft] + the signed TRAIN P FIXUP 'fixup: TRAIN P' (tP-fixup.sh) [+ zero or more
# 'fixup-N: TRAIN P' commits on top of it, in order: floor 9, a pushed sha is never replaced]. WHAT the fixup carries is
# MEASURED by tP-fixup.sh (S1, the metadata asset, corpus under the REGEN_ALLOW its footprint rows compose, goldens,
# the template-class csproj, NEW per-file .editorconfig): anything outside that stops the fixup for COORD.
#   A Go-only repro directory under the behavioral root (hazard H4) is NOT the fixup's: PRE-1's H4 arm refuses one. At
#   the draft list it reads 0 (no P row adds one: the pre-map's unioncheck read H4 = 0 at its chain head).
# The i9 sweeps the complement shard (tP-i9-shard.txt); P1/P2 run tP-linux-legs.sh. Legs run SEQUENTIALLY; the tP
# worktree is FROZEN while this runs (floor 4); every leg's rc is captured before any pipe (floor 7). One battery at a
# time on this box.
#
# LEG -> SEAT MAP (refs are rows of tP-seats-draft.txt; every by-name list is DERIVED in PRE-D, so a row seated after
# this draft is read by name without an edit):
#   C      every converter row: g-sort-self-capture (THE RELEASE FIX: convCallExpr / convExpr / convExprList),
#          c2-named-basic-conv and c2-anon-struct-named-conv (convCallExpr.go, the release fix's file), the two P2 rows
#          (conversionDriver, diagnosticOutput, versionReport, untypedOperands), c2-sibling-package-name-r2,
#          g-go-namespace-shadow-r2 and c2-alias-table-same-name (the import / namespace machinery), c2-embed-promoted-refs
#          and c2-lifted-iface-test-cast (-tests conversion), i9-incremental-cs-writes-r2 (11 converter files),
#          i9-tests-host-crash-verdict (crashVerdict.go, testConversion.go),
#          c2-literal-float-fold, g-method-value-fm-record-r3 and c2-elseif-position-record-r3 (the position map);
#          repoguard
#   CC     regression (N6): src/tools/comparison-classifier's own Go module
#   CB     the union's converter and repoguard tests BY NAME (derived; kept by a lexical scan of the declaring file,
#          P13: O's BATTERY STOP 1 and the parity filter's own misread) + the 6 base tree guards
#   FX     regression (the fixture-currency guard; floor = TRAIN O's own reading, read from O_RUN)
#   E      union-attributable 0 AT THE FIXUP HEAD: the footprint rows' corpus must be IN the tree (the sort seat's
#          sort/sort.cs, the sibling seat's 4 generated .cs, the float fold's mmu.cs, p2-test-warning-entries' two
#          .editorconfig, fm-record-r3's 11 package_info.cs, the else-if seat's 123). The U arm's written= count DROPS
#          under i9-incremental-cs-writes-r2 (a reading; the verdicts do not move: trainP/tP-CHANGES.md P10)
#   G1/G2  regression: no P row touches the roster (225 rows, 22 darwin annotations in the base, 0 rows with an
#          execution config)
#   SY     symbol sync          SI/SIc the projects P adds (derived: 8 at the draft list, all registered in go2cs.slnx)
#   ST51/ST7, NV*, NG*, IDC   regression: no P row touches the sweep or its self-test, src/push-nuget.ps1, src/tools
#          or the identifier census (read with git 2026-10-04: each row's merge-base..sha)
#   2b     go2cs.slnx END TO END: the golib/gen rows (g-debugger-views: 8 golib files, go2cs-gen Common.cs + 9
#          templates; r-csharp-consumer-smoke-r2: golib buildTransitive/go.lib.targets, a PACKED go.lib file that no
#          in-tree build imports (a project reference never reads buildTransitive: its gates are MS23 and MS26);
#          g-method-value-fm-record-r3: GoPositionMapAttribute.cs; c2-anon-struct-named-conv: ImplicitConvGenerator.cs)
#   CT     regression (ChannelTests)    GN  c2-anon-struct-named-conv's ForeignStructTargetConversionTests (GenTests)
#          TR  i9-tests-host-crash-verdict (the hand-own testing/TestHost.cs: the package-timeout event names the tests
#          still running; its GolibTests class HostPackageTimeoutRunningTestsTests is read by GT)
#   GT     the GolibTests classes the union adds or changes (derived: g-debugger-views' DebuggerViewsTests and the i9's
#          HostPackageTimeoutRunningTestsTests, added; NoUncountedBackingAllocationsTests, changed); every golib row
#          reaches the four full runs
#   4      CNR: every golden under the union converter; N DERIVED (cnrexpect, the runner's own predicate: PREDICTED 834
#          at the draft list); three converter rows meet in convCallExpr.go, three in the import / namespace files
#   5 + B: the projects P adds (derived), the goldens the fixup re-baselined (derived), the literal guards;
#          SortMethodSelfCapture isolated is the release fix's behavioral reading on this box
#   PUB    g-publish-keep's check-published-output.ps1 (regression at P; PUB_MIN 6, O's bat2 read 6 of 6)
#   H7     golib x3 (g-debugger-views); the corpus rows on all three flavours
#   WE     p2-test-warning-entries: arm B now also builds a package's tests csproj for a _test.cs section (math/bits,
#          weak); 12 entry files at the draft list (O read 10); then its planted control WEc (EXPECT 'CONTROL CAUGHT')
#   MOD    tP-modules-legs.sh: the module legs as the REGRESSION of the -tests -recurse driver (the sibling seat,
#          c2-embed-promoted-refs, c2-lifted-iface-test-cast and the i9's two rows edit -tests conversion, the
#          P2 rows the driver's diagnostics), then XS + XM (H1/H2 host seats landed at M, read from the base's
#          first-parent history)
#   PB     regression (F4's floor composed with -test-publish-binlog)
#   S      every row on this box at ITS roster execution config (none carries one); X_ROWS holds SORT since P (an i9
#          row: the release fix's package is read on BOTH boxes) beside encoding/json; the footprint rows this box
#          sweeps (strings, crypto/ecdsa, crypto/tls, database/sql, internal/buildcfg, math/bits) at their banked counts
#   NR     regression (net/rpc x5, 0 'all goroutines are asleep' lines)
#   TE/HS  what P moves in committed test sources (a READING; TRAIN O's own patch as baseline)
#   UF     untracked, not-ignored files under src/core after the sweeps and after the T legs. The -tests-only
#          .editorconfig class (math/bits on this box, weak on the i9) is KNOWN since O: with p2-test-warning-entries a
#          row EXPECT 0; without it exactly those paths are stamped KNOWN and anything else is a finding (P6)
#   T      runtime/pprof + runtime (TestTracebackSystem/panic DISCLOSED; fm-record-r3's runtime position maps and the
#          hand-owned managed_impl.cs: -fm frames); TBS-W; TE-T/HS-T; TL-WALL vs TRAIN O's battery
#   TE-i9  the same TE reading over the i9's shard patch
# NOT A LEG ON THIS BOX (P IS THE RELEASE TRAIN): the release gate itself, the os-matrix 'release-smoke' stage (one
# pack on windows, consume on all four RIDs, arms A-D with D gating; arm B exercises the three sort .Sort() forms and
# the bare form since c1-release-smoke-sort-arm), and the darwin behavioral FULL: C1 dispatches both at the P union and
# again after any fixup-N that touches a release path (tP-README.md MS23, checklist section 5b). The sort fix's
# acceptance beyond this box: the sort row on the i9 shard and G's pflag / cobra re-read at the union. The version
# bump and the release itself come AFTER the landing (the release runbook): never a step of this battery.
# After the battery, NOT a leg: the MS13 refresh of committed -tests sources (tP-README.md MS13) reads the S and T
# legs' saved patches (S-rewrites.patch, T-rewrites.patch) with the i9's.
# NOT here (tP-README.md section 6): HOP, SPB, the C1 token probe, L's TE signatures and bisect arms; a darwin RUN.
# VERIFY ROUNDS 1-3 of TRAIN M (trainM/tM-CHANGES.md sections 9-11) built the guards below; their notes stay beside the
# code they justify. N changed no guard's doctrine (it added PUB and CC, derived CNR's N, moved the base); O neither: it
# makes the base REQUIRED, re-points the previous-train baselines at N's battery, fixes PUB's environment (O5), raises
# PUB_MIN to 6, adds WE / WEc and IDC, and reads .editorconfig as ordinary emission. P neither: it re-points the base
# and the previous-train baselines at O (P1, P3), derives E_BISECT_KNOWN from the list's footprint rows (P4), reads the
# -tests-only .editorconfig class in UF (P6), adds sort to X_ROWS (P12) and restates the leg map and the predictions.
# VERIFY ROUND 1 (trainM/tM-CHANGES.md section 9): a LIVE gate across trains before the lock; MASTER defaults to the base;
# findings where a gate was only a stamp (the two BANK readings against the roster, XS/XM not read, SIc and NVR not
# firing, csproj among the sweep rewrites, GN/TR/2b/FX counts, the rows' execution configs, leg E's verdict);
# CNR's rewrites are restored before MOD; each row's converted-host elapsed is stamped (HOSTWALL).
# VERIFY ROUND 2 (trainM/tM-CHANGES.md section 10; COORD's rulings trainM/COORD-RULINGS-r2.md):
#  R1  NO BY-NAME LIST IS A LITERAL ONLY COORD CAN KEEP CURRENT. Section PRE-D derives, from git between $MASTER and
#      HEAD, and stamps: the converter tests the union adds (leg CB), the GolibTests classes it adds or changes and the
#      ones this box does not compile (GT), the behavioral projects it adds (legs 4 and 5), the goldens the fixup
#      re-baselined (leg 5), the rows whose execution config the union moves or keeps (leg S), the seats that change
#      the converter (E-bisect's superset), and (N) CNR's package count. The literals that remain are chosen by a hazard,
#      not by a seat list (GT_NEIGH, GT_TESTS, CHECKDEAD_GUARDS, NEIGH, X_ROWS, T_ROWS, HS_WANT, PUB_MIN, IDC_MIN, UF_KNOWN;
#      E_BISECT_KNOWN is derived since P):
#      each is checked against the tree and a miss is a FINDING by name. Totals (GN, CT, TR, FX, ST, PUB) are floors:
#      0 failed AND total >= a derived or measured floor, stamped with its source.
#  *   A record a leg reads must be THIS run's: the row's gitignored record files are deleted before every S row, NR
#      repeat and T leg, and a leg that leaves no fresh record is a finding. No gate passes by absence.
#  *   Tracked rewrites of CNR and of the behavioral legs are restored again before MOD (tracked changes asserted 0).
#  *   GT, CT, GN, TR run under the wall cap (45m, 10m, 20m, 20m); PUB too (N: 30m): a fired cap stops the battery,
#      lock kept, exit 5.
#  *   PRE admits 'fixup: TRAIN P' + zero or more 'fixup-N: TRAIN P' at the head (floor 9).
#  *   A process census by executable path after MOD.
#  *   The battery's exit status is 6 when any leg outside the expected-non-zero set is non-zero or any FINDING exists.
# VERIFY ROUND 3 (trainM/tM-CHANGES.md section 11; COORD's rulings trainM/COORD-RULINGS-r3.md):
#  *   seats-effective.txt is built as tP-fixup.sh builds it (row lines only, each terminated) and its row count asserted.
#  *   FXGOLD holds only behavioral PROJECT directories (a csproj directly in them), never a file of the behavioral root.
#  *   A process still running from the module out root after MOD STOPS the battery (exit 5, lock kept), RULED.
#  *   MOD's controls MR4c / MR6c: their TRAIN L converters carry the H2 seat's stream reader (tP-modules-legs.sh; since N
#      the H2 seat is LANDED, so its sha is read from master's first-parent line, not from the seat list).
W=/h/go2cs-tmp-coord/tP
WB='H:\go2cs-tmp-coord\tP'
EXPECT_HEAD=${EXPECT_HEAD:?the TRAIN P fixup commit, 10 chars}
MASTER=${MASTER:?set MASTER to the landed master of TRAIN O (the base the union was assembled on: tP-README.md step 6)}   # O1: REQUIRED, never a literal
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tP-seats-draft.txt}   # ONE name (L also looked for a frozen tL-seats.txt that never existed)
FOLLOWF=$SD/tP-follow.txt
SEATS_EXPECTED=${SEATS_EXPECTED:-}       # optional second derivation of the seat count (the GO's number); the list is the first
I7LIST=$SD/tP-i7-sweeps.txt
I9LIST=${I9LIST:-$SD/tP-i9-shard.txt}
T_ROWS='runtime runtime/pprof'           # roster rows on the i7 list read by the T legs (K's tleg), never by the S loop
# Rows on the i9's list this battery ALSO sweeps (stated in the GO, like the canaries). Verify round 2: the rows whose
# execution config the union decides are DERIVED in PRE-D from the roster at $MASTER and at HEAD (at O both read none:
# N's g-slog-roster-tc0 left no annotated row, and no O row adds one) and join the S run list by themselves; X_ROWS holds
# only what a derivation cannot name: encoding/json (RULED at M: read on two boxes; carried at N as a regression reading;
# at O it is ALSO in two footprint rows: p2-converter-warning-clears' decode.cs + package_info.cs and
# g-method-value-fm-record's package_info.cs, a banked row owing its sweep as the seats' own gate; at P it is in
# g-method-value-fm-record-r3's and the else-if seat's footprints). Drop it only by COORD's ruling.
# P (P12): SORT joins it. sort is an i9 row, and g-sort-self-capture (THE RELEASE FIX) commits sort/sort.cs: the release
# train reads the package on BOTH boxes. Stated limit: sort's own suite never calls the three .Sort() methods the fix
# repairs (G, 08:41), so the row is a REGRESSION reading of the package; the fix's readings are SortMethodSelfCapture
# (leg 5, isolated), C1's release-smoke arm B on four RIDs and G's pflag / cobra re-read.
# ONE RULING, ITS SITES (which rows carry an execution config: NONE since N, G's seat g-slog-roster-tc0 582de36d1b): the
# ruled set is tP-helpers.py EXEC_RULED (EMPTY), which precheck asserts against the roster by identity; EXEC_ROWS_EXPECT
# below defaults to its count (0); TC0_ROWS and TIERED_ROWS are derived from the roster; tP-linux-legs.sh and
# tP-i9-shard.sh read the roster of the union they run on. If the ruling moves, EXEC_RULED is the one edit.
X_ROWS='encoding/json sort'
TC0_ROWS=''                              # derived in PRE-D: annotated at $MASTER, not at HEAD -> must read tiered=False in their own record
TIERED_ROWS=''                           # derived in PRE-D: annotated at HEAD -> must read tiered=True
# P (P3; O4 at O): the previous train's record is TRAIN O's battery. O_RUN is its run folder (bat2: launched 06:37 on
# 2026-10-04, running at the derive; bat1 aborted at a purge and is NOT the record; COORD gives another folder at launch
# if O's battery of record is a later re-run). Stamped in PRE. O's variable was N_RUN: refused here when set, so a launch
# line copied from O cannot silently read the default instead of the folder it names.
[ -z "${N_RUN:-}" ] || { echo "ABORT: N_RUN is set (TRAIN O's variable). TRAIN P reads the previous train's record from O_RUN (default coord-scratch/tO/bat2): unset N_RUN and set O_RUN"; exit 2; }
O_RUN=${O_RUN:-/h/go2cs-tmp-coord/coord-scratch/tO/bat2}
O_SUMS="$O_RUN/tO-logs/SUMMARY.txt"                 # TL-WALL, FX floor, CNR control: TRAIN O's battery
O_REWRITES="$O_RUN/tO-logs/S-rewrites-U0.patch"     # TE baseline: O's sweep rewrites, same box
O_REWRITES_T="$O_RUN/tO-logs/T-rewrites-U0.patch"   # TE-T baseline: O's T-leg rewrites
PRECHECK_MODE=${PRECHECK_MODE:-abort}   # 'warn' runs on past a failed PRE assert (COORD's explicit call, stated in SUMMARY)
DEADLINE=${DEADLINE:-}                  # R5: no leg STARTS after this ABSOLUTE local time 'YYYY-MM-DD HH:MM'; unset = launch + 14 h (verify round 2, RULED)
SWEEP_ROW_CAP=${SWEEP_ROW_CAP:-4h}      # outer wall cap per sweep row (critic 1 #7); the sweep's own per-row timeouts fire first
I9_PATCH=${I9_PATCH:-}                  # the i9's tracked-changes-U0.patch, when it has arrived (critic 0 #5)
I9_BASELINE=${I9_BASELINE:-}            # optional: the i9's TRAIN O shard patch (coord-scratch/tO/i9-patches/tO-tracked-changes-U0.patch), as the TE-i9 baseline
EXEC_ROWS_EXPECT=${EXEC_ROWS_EXPECT:-}  # rows carrying an execution config; unset = the count of tP-helpers.py EXEC_RULED (see the note above X_ROWS)
# The seats bisected if leg E reads above 0: which seat's emission the tree lacks is what leg E measures, and it cannot
# be derived from git. Verify round 2 (R1): the list is asserted in PRE-D against the DERIVED superset, the seats whose
# own merge changes a non-test Go source of the converter; a named seat outside it is a finding, and the superset's other
# members are stamped by name (each arm is about 46 m, so they are COORD's to add: E_BISECT_SEATS='a b c').
# P (P4; O's BATTERY STOP 1, BS3): the DEFAULT is no longer a literal of row names. At O it named
# g-method-value-fm-record after that row had left the list at the freeze, and PRE-D raised a FINDING on a correct union
# (exit 6 at END unless COORD passed E_BISECT_SEATS at launch). E_BISECT_KNOWN is DERIVED in PRE-D (5) from the seat list
# this run reads: the rows whose notes carry a 'CORPUS FOOTPRINT' (the token tP-fixup.sh composes REGEN_ALLOW from: the
# seats with a measured corpus footprint) AND whose own merge changes the converter. A footprint row that changes no
# converter source (a row that only commits files) is stamped and not bisected. E_BISECT_SEATS given at launch still
# overrides (validated as before); E_BISECT_SEATS=none bisects nothing, by COORD's explicit call (an EMPTY value cannot
# say that: ${X:-default} reads it as unset).
E_BISECT_KNOWN=''                        # derived in PRE-D (5) from the list's footprint rows, never typed
E_BISECT_GIVEN=${E_BISECT_SEATS:-}       # COORD's list at launch ('none' = no arm), or empty = the derived default
# P (P6): UF's KNOWN class, a literal chosen by a ruling (COORD 2026-10-04 08:21 and 09:19), not by a seat list: the
# per-file .editorconfig a -tests run writes for a package whose only warning fact sits in a _test.cs and whose
# committed corpus held no entry file: math/bits (an i7 row) and weak (an i9 row). PRE-D (6) checks it against the tree.
UF_KNOWN='src/core/math/bits/.editorconfig src/core/weak/.editorconfig'
LOCK=/h/go2cs-tmp-coord/coord-scratch/tP/.battery.lock
LOGDIR=$SD/tP-logs
SUM="$LOGDIR/SUMMARY.txt"
# Run-copy checks BEFORE anything is written (critic 1 #4 and #9): a folder that already holds a SUMMARY would mix two
# runs' lines into NONZERO LEGS, wallcmp and the MOD stamp; a launch from a worktree or a draft folder breaks floor 4;
# a missing companion file would surface hours in as rc 127.
[ -e "$SUM" ] && { echo "ABORT: $SUM exists -- use a fresh per-run copy"; exit 2; }
[ -e "$SD/mod-logs/SUMMARY.txt" ] && { echo "ABORT: $SD/mod-logs/SUMMARY.txt exists -- use a fresh per-run copy"; exit 2; }
case "$SD" in
  /h/go2cs-tmp-coord/tP|/h/go2cs-tmp-coord/tP/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
for f in tP-emitcheck.sh emitdrift.py tP-helpers.py tP-modules-legs.sh tP-i9-te.sh tP-ng-parse.ps1 tP-follow.txt tP-i7-sweeps.txt tP-i9-shard.txt; do
  [ -f "$SD/$f" ] || { echo "ABORT: $SD/$f missing from the run copy"; exit 2; }
done
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS): copy trainP/tP-seats-draft.txt into the run folder"; exit 2; }
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
# Verify round 1 (floor 1 ACROSS trains): the lock below is TRAIN P's own; TRAIN L's battery takes another one and
# tL-postmerge2.sh takes none. tP-helpers.py live reads the other trains' locks, the converter and harness processes
# (twice, 20 s apart; listed, never killed: floor 5) and any run log still being written. LIVE_ACK=1 runs past a hit by
# COORD's explicit call; the reading is stamped in PRE either way.
if [ "${LIVE_ACK:-0}" = 1 ]; then
  LIVEV='ACKNOWLEDGED by COORD (LIVE_ACK=1): not read'
else
  lv=$(python -B "$(cygpath -w "$SD/tP-helpers.py")" live "$(cygpath -w "$LOCK")" 2>&1); lrc=$?
  if [ "$lrc" != 0 ]; then
    echo "ABORT (floor 1): another conversion, harness or battery looks alive on this box (listed, not killed; LIVE_ACK=1 runs past it by COORD's explicit call):"
    printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE' | head -n 12
    exit 2
  fi
  LIVEV=$(printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE-VERDICT' | cut -c1-200)
fi
# One battery at a time (floors 1 and 11): an atomic mkdir lock, released by the EXIT trap below.
mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- another TRAIN P battery, a standalone tP-modules-legs.sh or tP-fixup.sh holds it; rmdir it only after checking"; exit 2; }
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
hp(){ python -B "$(cygpath -w "$SD/tP-helpers.py")" "$@"; }
# Verify round 2: EXEC_ROWS_EXPECT's default is the count of the helper's EXEC_RULED (the one site of that ruling), and
# the two lists this run reads are hashed for the PRE line (per-run-folder copies: tP-fixup.sh stamps the same pair on
# its PRE and FIXUP DONE lines, so a battery launched from a folder whose tP-follow.txt is not the fixup's shows it).
[ -n "$EXEC_ROWS_EXPECT" ] || EXEC_ROWS_EXPECT=$(hp execruled 2>/dev/null | tr -d '\r' | sed -n 's/^EXECRULED n=\([0-9][0-9]*\).*/\1/p')
[ -n "$EXEC_ROWS_EXPECT" ] || { stamp "ABORT: tP-helpers.py execruled printed no count (EXEC_ROWS_EXPECT cannot be derived)"; exit 2; }
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
# Verify round 2 (floor 9, RULED): the head of the union is 'fixup: TRAIN P' followed by ZERO OR MORE 'fixup-N: TRAIN P'
# single-parent commits, in order (fixup-2 on the fixup, fixup-3 on fixup-2, ...), each signed, and nothing else
# single-parent. A red that needs a tree change after the union was pushed lands as a commit ON TOP, never as a replaced
# sha. NFU = how many; UTOP = the newest two-parent commit (the last seat or follow-up merge) under them.
NFU=0; t=HEAD
while [ "$(git rev-list --parents -n 1 "$t" | wc -w)" = 2 ] && [ "$NFU" -le 9 ]; do NFU=$((NFU + 1)); t="$t^"; done
[ "$NFU" -ge 1 ] && [ "$NFU" -le 9 ] || { stamp "ABORT: HEAD carries $NFU single-parent commit(s) above the merges (want 1 to 9: 'fixup: TRAIN P', then 'fixup-2', ...); subject of HEAD: ${subj:0:120}"; exit 2; }
k=$NFU; c=HEAD; FUSUBJ=''
while [ "$k" -ge 1 ]; do
  want='fixup: TRAIN P'; [ "$k" = 1 ] || want="fixup-$k: TRAIN P"
  cs=$(git log -1 --format=%s "$c")
  case "$cs" in "$want"*) ;; *) stamp "ABORT: the single-parent commit $(git rev-parse --short=10 "$c") is not '$want...' (subject: ${cs:0:120}): the head must be 'fixup: TRAIN P' then 'fixup-2: TRAIN P', ... in order"; exit 2 ;; esac
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) stamp "ABORT: the fixup commit $(git rev-parse --short=10 "$c") is not signed (want G or U)"; exit 2 ;; esac
  FUSUBJ="$(git rev-parse --short=10 "$c") ${cs:0:60} | $FUSUBJ"
  k=$((k - 1)); c="$c^"
done
UTOP=$(git rev-parse "$t")
# (the GOROOT pin was checked on its value before the export, ENV above)
git merge-base --is-ancestor "$MASTER" HEAD || { stamp "ABORT: master $MASTER is not an ancestor"; exit 2; }
# O1: MASTER is required; it must resolve and sit on the union's FIRST-PARENT line (the base the seats were merged onto:
# an ancestor elsewhere in the graph would make every first-parent count below a count of the wrong range).
MASTERF=$(git rev-parse --verify -q "$MASTER^{commit}") || { stamp "ABORT: MASTER=$MASTER does not resolve to a commit"; exit 2; }
git rev-list --first-parent HEAD | grep -qxF "$MASTERF" || { stamp "ABORT: MASTER ${MASTERF:0:10} is not on the union's first-parent line (the union was assembled on another base: read tP-assemble.sh's MAP TREE line)"; exit 2; }
git merge-base --is-ancestor "$MASTERF" origin/master 2>/dev/null || stamp "NOTE: MASTER ${MASTERF:0:10} is not an ancestor of the local origin/master (a stale fetch, or not a landed master: COORD reads it)"
ns=0
while IFS='|' read -r b sha msg; do
  ns=$((ns + 1)); git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
# The seat count is READ from the list (L wrote 11 here and again in its fixup; M read 25, N drafts 27). What makes the count mean something
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
# seated sha and is not it. The entries are read from tP-follow.txt, the SAME file tP-fixup.sh reads: EMPTY at the draft
# list. At N hazard H4 fired at the draft (r-jwt-promoted-iface-repros, not seated). O: H4 reads 0 at the draft list only
# because r-atlas-batch-repros boards with both rows that complete it; a follow-up for H4 would be added before the fixup
# (tP-README.md MS2).
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
      stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") is NOT a listed seat or a ruled follow-up (if it is one: add <ref>:<seat sha> to THIS run folder's tP-follow.txt -- the fixup's run folder holds the entry -- and fetch origin/claude/<ref>; this run read $LISTS)"; mbad=$((mbad + 1))
    fi
  fi
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) munsigned=$((munsigned + 1)) ;; esac
done
[ "$mbad" = 0 ] || { stamp "ABORT: $mbad first-parent commit(s) on the union are not seat merges"; exit 2; }
fpc=$(git rev-list --first-parent --count "$MASTER..HEAD")
[ "$fpc" = $((ns + nfix + NFU)) ] || { stamp "ABORT: union first-parent count $fpc != seats $ns + follow-ups $nfix + $NFU fixup commit(s) -- a stray or missing commit on the union"; exit 2; }
SEATS_EFF="$LOGDIR/seats-effective.txt"
# Verify round 3: built as tP-fixup.sh builds it -- the ROW lines only, CR-stripped, each terminated, then the follow-up
# rows -- and the row count asserted. `cat "$SEATS"` welded the first follow-up row onto the last line of a list that
# ends without a newline (a comment, or a seat's notes), and precheck then read that follow-up as nobody's.
{ grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r'; for f in $FIXES; do echo "${f%%:*}-followup|${f##*:}|ruled follow-up merge of ${f%%:*} (origin/claude/${f%%:*})"; done; } > "$SEATS_EFF"
[ "$(grep -cE '^[A-Za-z0-9._-]+\|' "$SEATS_EFF")" = $((ns + nfix)) ] || { stamp "ABORT: seats-effective.txt holds another row count than $ns seat rows + $nfix follow-up row(s)"; exit 2; }
stamp "PRE union shape OK: first-parent=$fpc (= $ns seat merges + $nfix follow-up merge(s):$FIXES + $NFU fixup commit(s): ${FUSUBJ% | }), ^2-contains-seat=$mcontain, fixup signed=$fsig, unsigned seat merges=$munsigned (stated, not gated)"
[ "$NFU" = 1 ] || stamp "PRE NOTE: $((NFU - 1)) fixup commit(s) ON TOP of 'fixup: TRAIN P' (floor 9: the pushed sha was kept). This battery reads the tree at HEAD; which legs a fixup-N obliges COORD to re-read on the lanes is tP-README.md section 4b, by the paths it touches: $(git diff --name-only "$(git rev-parse "HEAD~$((NFU - 1))")" HEAD | head -n 8 | tr '\n' ' ')"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before the battery"; exit 2; }
dv=$(dotnet --version 2>&1); dvrc=$?
case "$dvrc:$dv" in 0:10.0.*) ;; *) stamp "ABORT: dotnet under global.json rc=$dvrc: $(echo "$dv" | tr '\r\n' '  ' | cut -c1-200)"; exit 2 ;; esac
stamp "PRE head=$h (fixup: ${subj:0:90}) parent=$(git rev-parse --short=10 HEAD^) tree=$(git rev-parse --short=10 HEAD^{tree}) signed=$fsig master=$MASTER (${MASTERF:0:10}) previous-train record (TRAIN O) O_RUN=$O_RUN ($([ -f "$O_SUMS" ] && echo "SUMMARY $(grep -c . "$O_SUMS") lines" || echo "NO SUMMARY: the FX and CNR fallbacks are stamped")) seats=$ns ($(basename "$SEATS")) $LISTS $(go version) dotnet=$(echo "$dv" | tr -d '\r') free=$(freegb)G C:=$(cfree)G GO2CS_MODULE_ROOT=${GO2CS_MODULE_ROOT:-unset}"

# ================================================================================================= PRE-D (verify round 2, ruling R1)
# Every list a leg reads BY NAME is derived here from git between $MASTER and HEAD, written to tP-logs/derived-*.txt and
# stamped, so the SUMMARY shows what this run read. Nothing below is typed from a seat list: a row accepted after this
# draft (the list grew from 15 to 24 rows in four hours) is read by name without an edit. What cannot be derived stays a
# literal chosen by a hazard, and is checked against the tree: a miss is a finding BY NAME (it does not stop PRE).
dfind(){ finding "PRE-D ${1%%|*}: ${1#*|}"; }   # 'label|text': a literal the tree does not bear out, listed at END like any other finding
# (1) leg CB -- the converter and repoguard tests the union ADDS (a '+func Test...(' line in the diff of src/go2cs: a new
#     test, or one moved or renamed), plus six tree guards that exist at the base and that leg C (no -v) would read 'ok'
#     on a skip: the stdlib-metadata guard and the five projitems tests. (TRAIN M read 74 added at its head; N: derived.)
CB_BASE='TestStdLibMetadataInSync TestProjitemsRegistersEveryGoSource TestProjitemsHasNoDanglingEntries TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings TestProjitemsRegistrationClassifierFires TestProjitemsInsertionHintTakesTheNearestPredecessor'
CB_ADDED=$(git diff "$MASTER" HEAD -- src/go2cs | sed -n 's/^+func \(Test[A-Za-z0-9_]*\)(.*/\1/p' | LC_ALL=C sort -u | tr '\n' ' ')
# BATTERY STOP 1 at O (bat1, 04:58): CB read not-PASS=[TestRunning], a FINDING. The name is no test: it is a line of a
# fixture's Go source held in a RAW STRING (warningEntries_test.go:259, p2-converter-warning-clears), and the text
# derivation above cannot tell. O's remedy kept a name when an EVEN count of backticks sat above its line.
# P (P13): that parity misreads every line below a backtick held in an interpreted string, a rune literal or a comment.
# At P's own list it DROPS 8 of the 9 real tests of crashVerdict_test.go (i9-tests-host-crash-verdict: a .NET arity
# tick inside a quoted stack line), and over every converter test file of O's union it drops 8 real tests of three
# files (read with git: 1113 text matches, 1108 declarations in code, the parity keeps 1100). A derived name is now
# kept only when a LEXICAL scan of a test file the union changed finds its declaration in CODE at HEAD (gotests: line
# and block comments, interpreted strings and rune literals with their escapes, raw strings). The scan's own control
# runs first, on this box's awk (floor 13): if it misreads, NO name is filtered (a fixture's name then reads not-PASS in
# CB: visible) and the miss is a finding. Names dropped are stamped, never silent.
gotests(){ LC_ALL=C awk '
  { line = $0; sub(/\r$/, "", line)
    if (st == 0 && match(line, /^func Test[A-Za-z0-9_]*\(/)) print substr(line, 6, RLENGTH - 6)
    n = length(line); i = 1
    while (i <= n) {
      c = substr(line, i, 1); d = substr(line, i, 2)
      if (st == 1) { if (d == "*/") { st = 0; i += 2 } else i++; continue }
      if (st == 2) { if (c == "`") st = 0; i++; continue }
      if (d == "//") break
      if (d == "/*") { st = 1; i += 2; continue }
      if (c == "`") { st = 2; i++; continue }
      if (c == "\"" || c == "\047") {
        i++
        while (i <= n) { e = substr(line, i, 1); if (e == "\\") { i += 2; continue }; i++; if (e == c) break }
        continue
      }
      i++
    }
  }'; }
gotests_ctl(){ printf '%s\n' 'package p' 'var a = "x`y\"" // one backtick in an interpreted string' 'func TestReal1(t *testing.T) {}' 'var b = `' 'func TestInRaw(t *testing.T) {' '`' "var c = '\`'" '/* a block comment' 'func TestInComment(t *testing.T) {}' '*/ // a ` in a line comment' 'func TestReal2(t *testing.T) {}' | gotests | tr '\n' ' '; }
CB_SCAN=$(gotests_ctl)
CB_KEPT=''; CB_DROPPED=''
if [ "$CB_SCAN" = 'TestReal1 TestReal2 ' ]; then
  CB_DECL=" $( { git diff --name-only "$MASTER" HEAD -- src/go2cs | grep -E '_test\.go$' || true; } | while IFS= read -r cbf; do if git cat-file -e "HEAD:$cbf" 2>/dev/null; then git show "HEAD:$cbf" | gotests; fi; done | LC_ALL=C sort -u | tr '\n' ' ')"
  for t in $CB_ADDED; do case "$CB_DECL" in *" $t "*) CB_KEPT="$CB_KEPT$t " ;; *) CB_DROPPED="$CB_DROPPED$t " ;; esac; done
else
  CB_KEPT=$CB_ADDED; dfind "CB_SCAN|the test-name scan's control read [$CB_SCAN] where [TestReal1 TestReal2 ] is expected (this box's awk): no derived name was filtered"
fi
[ -z "$CB_DROPPED" ] || stamp "PRE-D CB: derived name(s) that are NOT tests (no declaration in code at HEAD: a 'func TestX(' line inside a raw-string fixture or a comment), dropped: [${CB_DROPPED% }]"
CB_ADDED=$CB_KEPT
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
NDIRS=$(git diff --name-only --diff-filter=A "$MASTER" HEAD -- src/tests/Behavioral | grep -E '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | cut -d/ -f4 | grep -vxE 'BehavioralTests|BehavioralRunner' | LC_ALL=C sort -u | tr '\n' ' ')
NSLNX=$(git diff "$MASTER" HEAD -- src/go2cs.slnx | sed -n 's#^+.*<Project Path="tests/Behavioral/\([^/"]*\)/[^/"]*\.csproj".*#\1#p' | LC_ALL=C sort -u | tr '\n' ' ')
# (N: the go2cs.slnx agreement is read after (3b): a project the tree declares exclusive to another platform is not
#  registered in go2cs.slnx by design, as the six linux-only projects at the base are not; NativeFieldPortAlias at N.)
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
for g in $CHECKDEAD_GUARDS $NEIGH; do git cat-file -e "HEAD:src/tests/Behavioral/$g" 2>/dev/null || dfind "NGUARDS|the literal guard $g is not a directory under src/tests/Behavioral at HEAD"; done
# (3b) leg 4 -- CNR's package count, DERIVED (item N2; never typed): tP-helpers.py cnrexpect applies check-no-regression's
#     own predicate to the tree (every behavioral directory with a *.go file, minus the [GoPlatformExclusive] /
#     [GoArchExclusive] packages a windows/amd64 host cannot measure). TWO readings: at $MASTER (its control: it must
#     reproduce TRAIN O's battery CNR, N=819 with the seven platform-exclusive skips, read from O_SUMS: O's landing
#     commits, a possible fixup-2 and the MS13 refresh, add no behavioral directory) and at HEAD (the expectation).
#     A RECONCILIATION beside them, never the expectation: the base's N + the package directories the union adds that
#     windows CAN measure - the ones it removes that windows could = the derived N at HEAD; a difference is a finding.
#     N's case in point was NativeFieldPortAlias, added AND skipped (785 + 17 - 1 = 801); O read 801 + 18 = 819 (bat2,
#     08:39). P's draft list adds 16 package DIRECTORIES in 8 projects (four projects hold sub-packages, and CNR counts
#     every directory with a *.go) and removes 1 (c2-sibling-package-name-r2 renames AliasNamespaceShadow/sortlocal to
#     sort), none declared exclusive (read with git 2026-10-04: 826 enumerated at O's union, 7 skipped), so the
#     reconciliation PREDICTS 819 + 16 - 1 = 834 at the 22-row draft (a slot that adds a project, or a re-cut, moves it;
#     read PRE-D's line, never this comment). The posts' list (NPOST, a literal: what the seats SAID they add, as
#     DIRECTORIES since P) is stamped beside the derived delta, and a name in one and not the other is stamped too (a
#     NOTE, not a finding).
NPOST='AliasNamespaceShadow/sort AnonStructNamedConversion AnonStructNamedConversion/structs GoHostModuleShadow GoHostModuleShadow/consumer GoHostModuleShadow/hostlib LiteralFloatConstFold MethodValueFmRecord NamedBasicConversion SameNameImportAlias SameNameImportAlias/a/foo SameNameImportAlias/b/foo SiblingPackageNames SiblingPackageNames/teststructs/foo1 SiblingPackageNames/teststructs/foo2 SortMethodSelfCapture'   # P: the 16 package directories the P rows add (8 projects; read with git at each row's tip, 2026-10-04); the sibling seat also REMOVES AliasNamespaceShadow/sortlocal
hp cnrexpect "$WW" HEAD --base "$MASTER" > "$LOGDIR/cnrexpect.txt" 2>&1; cxrc=$?
hp cnrexpect "$WW" "$MASTER" > "$LOGDIR/cnrexpect-master.txt" 2>&1; cxrb=$?
cxn(){ tr -d '\r' < "$1" | sed -n 's/^CNREXPECT .* n=\([0-9][0-9]*\)$/\1/p'; }
cxs(){ tr -d '\r' < "$1" | sed -n 's/^CNRSKIP \([^ ]*\) .*/\1/p' | LC_ALL=C sort | tr '\n' ' '; }
CNR_DERIVED=$(cxn "$LOGDIR/cnrexpect.txt"); CNR_SKIPS=$(cxs "$LOGDIR/cnrexpect.txt")
CNR_BASEN=$(cxn "$LOGDIR/cnrexpect-master.txt"); CNR_BSKIPS=$(cxs "$LOGDIR/cnrexpect-master.txt")
CNR_ADDED=$(tr -d '\r' < "$LOGDIR/cnrexpect.txt" | sed -n 's/^CNRDELTA .* added=\([0-9]*\) \[\([^]]*\)\].*/\2/p')
CNR_REMOVED=$(tr -d '\r' < "$LOGDIR/cnrexpect.txt" | sed -n 's/^CNRDELTA .* removed=\([0-9]*\) \[\([^]]*\)\].*/\2/p')
CNR_MPREV=$(grep -aoE '4 asserts: N=[0-9]+' "$O_SUMS" 2>/dev/null | tail -n 1 | grep -oE '[0-9]+$'); CNR_MSRC="TRAIN O's own leg-4 stamp ($O_SUMS)"
# Review round 1 (floor 13): the control used to be SKIPPED, with no finding, when M's SUMMARY was missing or unreadable
# (an empty value passed the test). It falls back to the literal TRAIN O's battery printed ('4 asserts: N=819 (EXPECT 819'
# at 08:39 on 2026-10-04, bat2; N's was 801), as FX_FLOOR falls back to 1291, and the fallback is stamped as such.
[ -n "$CNR_MPREV" ] || { CNR_MPREV=819; CNR_MSRC="the literal 819, TRAIN O's battery CNR line ('4 asserts: N=819', bat2; its SUMMARY is not readable at $O_SUMS)"; }
[ "$cxrc" = 0 ] && [ "$cxrb" = 0 ] && [ -n "$CNR_DERIVED" ] && [ -n "$CNR_BASEN" ] || dfind "CNR|tP-helpers.py cnrexpect did not read the tree (rc=$cxrc at HEAD, $cxrb at $MASTER; cnrexpect*.txt): CNR's N cannot be derived"
[ "$CNR_MPREV" = "${CNR_BASEN:-x}" ] || dfind "CNR|the reader's control: cnrexpect at $MASTER reads N=${CNR_BASEN:-unread}, TRAIN O's battery CNR printed $CNR_MPREV ($CNR_MSRC): the predicate is not the runner's (or O's landing commits moved a behavioral directory: read git diff --stat between O's union and $MASTER)"
cna=$(printf '%s\n' $CNR_ADDED | grep -vxF -f <(printf '%s\n' $CNR_SKIPS '#none#') | grep -c .)
cnr_=$(printf '%s\n' $CNR_REMOVED | grep -vxF -f <(printf '%s\n' $CNR_BSKIPS '#none#') | grep -c .)
CNR_ADDSKIP=$(printf '%s\n' $CNR_ADDED | grep -xF -f <(printf '%s\n' $CNR_SKIPS '#none#') | tr '\n' ' ')
CNR_RECON=''; [ -n "$CNR_BASEN" ] && CNR_RECON=$((CNR_BASEN + cna - cnr_))
[ -z "$CNR_RECON" ] || [ "$CNR_RECON" = "${CNR_DERIVED:-x}" ] || dfind "CNR|reconciliation: the base's N $CNR_BASEN + $cna measurable added - $cnr_ measurable removed = $CNR_RECON, the derived N at HEAD is ${CNR_DERIVED:-unread}"
npo=$(LC_ALL=C comm -23 <(printf '%s\n' $NPOST | LC_ALL=C sort) <(printf '%s\n' $CNR_ADDED | LC_ALL=C sort) | tr '\n' ' '); npd=$(LC_ALL=C comm -13 <(printf '%s\n' $NPOST | LC_ALL=C sort) <(printf '%s\n' $CNR_ADDED | LC_ALL=C sort) | tr '\n' ' ')
CNR_SRC="derived (cnrexpect at HEAD)"; [ -z "${CNR_EXPECT_N:-}" ] || CNR_SRC="given at launch (COORD's explicit call; the derived N is ${CNR_DERIVED:-unread})"
CNR_EXPECT_N=${CNR_EXPECT_N:-$CNR_DERIVED}
stamp "PRE-D CNR: EXPECT N=${CNR_EXPECT_N:-unread} ($CNR_SRC); platform-exclusive skips at HEAD [${CNR_SKIPS% }] (added AND skipped: [${CNR_ADDSKIP% }]); the reader's control at $MASTER: N=${CNR_BASEN:-unread} vs TRAIN O's battery $CNR_MPREV ($CNR_MSRC); reconciliation: $CNR_BASEN + $cna measurable added - $cnr_ removed = ${CNR_RECON:-unread}; added by the union [${CNR_ADDED}] removed [${CNR_REMOVED}] :: posts' list: in posts not in the tree [${npo% }] in the tree not in posts [${npd% }]"
# (3) continued -- the go2cs.slnx agreement, read now that the platform-exclusive set is known (PRE-D 3b).
NDIRS_SL=$(printf '%s\n' $NDIRS | grep . | grep -vxF -f <(printf '%s\n' $CNR_SKIPS '#none#') | LC_ALL=C sort -u | tr '\n' ' ')
NDIRS_X=$(printf '%s\n' $NDIRS | grep . | grep -xF -f <(printf '%s\n' $CNR_SKIPS '#none#') | tr '\n' ' ')
[ "$NDIRS_SL" = "$NSLNX" ] || dfind "NDIRS|behavioral projects the union adds, by tree, minus the platform-exclusive [${NDIRS_SL% }] != by go2cs.slnx lines [${NSLNX% }] (a project added without its registration, or the reverse)"
stamp "PRE-D behavioral: projects the union adds=[${NDIRS% }] ($(echo $NDIRS | wc -w); slnx agrees: $([ "$NDIRS_SL" = "$NSLNX" ] && echo yes || echo NO), the platform-exclusive excepted: [${NDIRS_X% }]); projects whose goldens the fixup commit(s) changed=[${FXGOLD% }]$([ -z "$FXNOPROJ" ] || echo " (directories the fixup commit(s) changed that hold no csproj, not read as projects: ${FXNOPROJ% })"); literal guards: checkdead $(echo $CHECKDEAD_GUARDS | wc -w), neighbours $(echo $NEIGH | wc -w)"
# (4) leg S -- the rows whose execution config the union decides, from the roster itself: annotated at HEAD = must read
#     tiered=True; annotated at $MASTER and not at HEAD = must read tiered=False. precheck (PRE-1) asserts the HEAD set
#     against the ruling by identity, so a wrong roster stops there; these two lists follow the roster.
hp execrows "$WW" "$MASTER" > "$LOGDIR/execrows-master.txt" 2>&1; xr1=$?
hp execrows "$WW" HEAD > "$LOGDIR/execrows-head.txt" 2>&1; xr2=$?
[ "$xr1" = 0 ] && [ "$xr2" = 0 ] || { stamp "ABORT: tP-helpers.py execrows rc=$xr1/$xr2 (the roster did not read: execrows-master.txt / execrows-head.txt)"; exit 2; }
TIERED_ROWS=$(tr -d '\r' < "$LOGDIR/execrows-head.txt" | sed -n 's/^EXECROW \([^ ]*\) .*/\1/p' | LC_ALL=C sort -u | tr '\n' ' '); TIERED_ROWS=${TIERED_ROWS% }
TC0_ROWS=$(tr -d '\r' < "$LOGDIR/execrows-master.txt" | sed -n 's/^EXECROW \([^ ]*\) .*/\1/p' | LC_ALL=C sort -u | grep -vxF -f <(printf '%s\n' $TIERED_ROWS '#none#') | tr '\n' ' '); TC0_ROWS=${TC0_ROWS% }
stamp "PRE-D execution configs (roster at $MASTER vs HEAD): rows that keep an annotation (EXPECT tiered=True)=[$TIERED_ROWS] rows that drop it (EXPECT tiered=False)=[$TC0_ROWS]; EXEC_ROWS_EXPECT=$EXEC_ROWS_EXPECT; all of them and X_ROWS=[$X_ROWS] join the S run list"
# (5) E-bisect -- the superset: seats whose own merge on the union changes a non-test Go source of the converter.
#     union-merges.txt = '<second parent> <merge>' for every merge on the union's first-parent line (leg E-bisect reads it too).
#     P (P4): the DEFAULT list is derived here. FOOTROWS = the rows whose notes carry a 'CORPUS FOOTPRINT' group (list
#     order; tP-fixup.sh's own pattern), E_BISECT_KNOWN = the FOOTROWS that are converter seats. Nothing is typed, so a
#     row that leaves the list leaves the default with it.
for c in $(git rev-list --first-parent "$MASTER..$UTOP"); do echo "$(git rev-parse "$c^2") $c"; done > "$LOGDIR/union-merges.txt"
CONVSEATS=''; FOOTROWS=''
while IFS='|' read -r b sha msg; do
  bf=$(git rev-parse "$sha^{commit}"); bm=$(awk -v s="$bf" '$1 == s { print $2 }' "$LOGDIR/union-merges.txt" | head -n 1)
  [ -n "$bm" ] && [ "$(git diff --name-only "$bm^1" "$bm" -- 'src/go2cs/*.go' | grep -vc '_test\.go$')" != 0 ] && CONVSEATS="$CONVSEATS$b "
  printf '%s' "$msg" | grep -qE 'CORPUS FOOTPRINT[^()]*\([^)]*\)' && FOOTROWS="$FOOTROWS$b "
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
E_BISECT_KNOWN=$(for s in $FOOTROWS; do case " $CONVSEATS " in *" $s "*) printf '%s ' "$s" ;; esac; done); E_BISECT_KNOWN=${E_BISECT_KNOWN% }
ebnoconv=$(for s in $FOOTROWS; do case " $CONVSEATS " in *" $s "*) ;; *) printf '%s ' "$s" ;; esac; done)
case "$E_BISECT_GIVEN" in
  '') E_BISECT_SEATS=$E_BISECT_KNOWN; EB_SRC="derived: the list's CORPUS FOOTPRINT rows that are converter seats" ;;
  none) E_BISECT_SEATS=''; EB_SRC="E_BISECT_SEATS=none at launch (COORD's explicit call: no arm)" ;;
  *) E_BISECT_SEATS=$E_BISECT_GIVEN; EB_SRC="given at launch (COORD's explicit call; the derived default is [$E_BISECT_KNOWN])" ;;
esac
ebx=''; for s in $E_BISECT_SEATS; do case " $CONVSEATS " in *" $s "*) ;; *) ebx="$ebx$s "; dfind "E_BISECT|E_BISECT_SEATS names $s, which is not a seat whose merge changes the converter (not a row, or no non-test .go under src/go2cs in its merge)" ;; esac; done
ebrest=$(for s in $CONVSEATS; do case " $E_BISECT_SEATS " in *" $s "*) ;; *) printf '%s ' "$s" ;; esac; done)
stamp "PRE-D E-bisect: seats whose merge changes the converter=[${CONVSEATS% }] ($(echo $CONVSEATS | wc -w)); footprint rows (a CORPUS FOOTPRINT in the notes)=[${FOOTROWS% }], of them not converter seats (not bisectable)=[${ebnoconv% }]; bisected if leg E reads above 0: [$E_BISECT_SEATS] ($EB_SRC; about 46 m an arm); converter seats NOT in that list (COORD adds them with E_BISECT_SEATS): [${ebrest% }]"
# REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-10): P4's consequence, stated where the launch reads it. O's typed default
# was two arms; the derived default at P's draft is FIVE: about 3 h 50 m if leg E reads above 0, early in the run, on
# top of an 11 to 12 h battery. That passes the default DEADLINE (launch + 14 h), where the battery stops at exit 9
# before the sweeps finish. Nothing is capped here (which arms to run is COORD's): the arithmetic is stamped.
nba=$(echo $E_BISECT_SEATS | wc -w)
[ "$nba" -le 2 ] || stamp "PRE-D E-bisect NOTE: $nba arm(s) at about 46 m each = about $((nba * 46)) m IF leg E reads above 0, against DEADLINE='$DEADLINE' ($DL_SRC). Under the default deadline (launch + 14 h) and an 11 to 12 h battery, more than two arms run past it (exit 9 before the sweeps finish): on an E red, relaunch with DEADLINE set explicitly, or cut the arms with E_BISECT_SEATS (tP-README.md Q9)"
# (6) UF's KNOWN class against the tree (P6). UF_ROW = the row of THIS list that commits the two -tests-only
#     .editorconfig files (found by its ref prefix, so a -rN re-cut of it still reads). With the row, both paths must be
#     TRACKED at HEAD; without it, neither may be (another row commits them: UF_KNOWN or the prefix is stale). A miss is a
#     finding by name, like every literal the tree does not bear out.
UF_ROW=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r' | cut -d'|' -f1 | grep -E '^p2-test-warning-entries' | head -n 1)
ufk_tracked=''; ufk_absent=''
for p in $UF_KNOWN; do if git cat-file -e "HEAD:$p" 2>/dev/null; then ufk_tracked="$ufk_tracked$p "; else ufk_absent="$ufk_absent$p "; fi; done
if [ -n "$UF_ROW" ]; then
  [ -z "$ufk_absent" ] || dfind "UF_KNOWN|$UF_ROW is a row and the tree at HEAD does not track [${ufk_absent% }]: the row no longer commits the -tests-only .editorconfig files, and UF would read them untracked"
else
  [ -z "$ufk_tracked" ] || dfind "UF_KNOWN|no p2-test-warning-entries row is in this list and the tree at HEAD tracks [${ufk_tracked% }]: another row commits them (the UF_ROW prefix is stale) or UF_KNOWN is"
fi
stamp "PRE-D UF known class (P6): [$UF_KNOWN]; the row that commits them: ${UF_ROW:-NOT A ROW (a path of the class a sweep leaves untracked is stamped KNOWN; any other untracked path is a finding)}; tracked at HEAD: [${ufk_tracked% }]"
# (7) THE RELEASE FIX IS IN THE TREE (P is the release train; trainP/tP-CHANGES.md P9). go.sort 1.24.13.3 ships three
#     convenience methods that call themselves: 'public static void Sort(this IntSlice x) { Sort(x); }' binds to the
#     extension itself, not to Sort(Interface) (G, 2026-10-04 08:41 and 11:35: a stack overflow under Debug or tiered
#     JIT, a spin under Release with TieredCompilation off). g-sort-self-capture commits the cast in
#     src/core/sort/sort.cs. The predicate is the DEFECT's own line, not the fix's spelling: 0 bare 'Sort(x);' lines at
#     HEAD, with the three methods still declared (the reader's own check that it reads the right file). CONTROL: the
#     same reader at $MASTER, where the fix is absent, must read 3 (measured with git at O's union eb88ab9492: 3, lines
#     130 / 160 / 179; at the seat b719825826: 0). A miss at HEAD is a FINDING by name: a merge, a re-cut or the fixup's
#     regeneration (the file is in REGEN_ALLOW) dropped the fix the release waits for. Nothing here RUNS the fix: that
#     is SortMethodSelfCapture (leg 5, isolated), C1's release-smoke arm B on four RIDs and G's pflag / cobra re-read.
rfx(){ git show "$1:src/core/sort/sort.cs" 2>/dev/null | tr -d '\r' | grep -cE "$2"; }
RF_SELF='^[[:space:]]*Sort\(x\);[[:space:]]*$'; RF_DECL='public static void Sort\(this (IntSlice|Float64Slice|StringSlice) x\)'
rfh=$(rfx HEAD "$RF_SELF"); rfd=$(rfx HEAD "$RF_DECL"); rfb=$(rfx "$MASTER" "$RF_SELF")
stamp "PRE-D release fix (go.sort self-recursion, P9): src/core/sort/sort.cs at HEAD holds $rfh bare 'Sort(x);' line(s) (EXPECT 0) and $rfd of the three .Sort() methods (EXPECT 3); the reader's control at $MASTER: $rfb (EXPECT 3, the published defect; 0 = the base already holds the fix)"
[ "$rfh" = 0 ] && [ "$rfd" = 3 ] || dfind "RELFIX|src/core/sort/sort.cs at HEAD reads $rfh self-recursive 'Sort(x);' line(s) and $rfd of the three .Sort() methods (expected 0 and 3): THE RELEASE FIX (g-sort-self-capture) is not in the tree as seated"
[ "$rfb" = 3 ] || [ "$rfb" = 0 ] || dfind "RELFIX|the reader's control at $MASTER reads $rfb bare 'Sort(x);' line(s) (expected 3, or 0 when the base holds the fix): the predicate is not reading what it names"

# PRE-1: tP-helpers.py precheck at HEAD. COUNT: the six registration files = base + each seat's OWN net inserts + the
# fixup's own (TRAIN M's run3 read projitems 425 -> 450, go2cs.slnx 1069 -> 1076; at N the eight behavioral seats insert
# into go2cs.slnx and the four BehavioralTests files, and go2cs-src.projitems takes many adds plus p2-n-cleanup's
# deletions: the line-count invariant is this arm, tP-README.md MS14).
# P: 8 rows insert behavioral registrations (8 projects, 16 package directories) into go2cs.slnx and the four
# BehavioralTests files, 12 rows touch go2cs-src.projitems; the pre-map's chain read slnx 1111 -> 1126, projitems
# 492 -> 510 and +24 lines in each of the four lists on O's union (the BASE moves the base counts, never the deltas).
# No identical insert and no MULTI-BASE row at the draft list (D1's credit and O3's synthesis stay, unused). REG
# predicts, on go2cs.slnx, seat-removed=[c2-sibling-package-name-r2:tests/Behavioral/AliasNamespaceShadow/sortlocal/
# AliasNamespaceShadow.sortlocal.csproj] (the seat renames that project to sort/: one key removed, four added).
# O's note: 16 rows inserted behavioral registrations (18 projects), 13 rows plus the hashset re-cut touched
# go2cs-src.projitems (HashSet.go removed); COUNT credited A1 / A2's identical CheckPtrToAnonStructPtr hunk once (D1)
# and synthesized c2-nuget-followups-r2's own change (MULTI-BASE, O3); REG read seat-removed=[p1-hashset-module-r2:
# ...HashSet.go] (bat2, PRE-1).
# REG: every base key and every seat-added key is registered once, and a key a seat REMOVES on purpose is gone
# (review round 1: p1-hashset-module's HashSet.go and p2-n-cleanup's five guard tests; the line names them as
# seat-removed=[...], and a removed key still present is 'removed-key-back'). BOTH (per merge since verify round 1): in
# every merge on the union's first-parent line, each path both sides changed keeps both sides' lines, none lost, none
# back, none duplicated. ROSTER: no row's N + D moved, and no row carries an execution config (EXEC_RULED is empty:
# g-slog-roster-tc0). H3: Goroutine.cs holds checkdead's four master identifiers at their base counts (no N row edits
# it; at P g-debugger-views does, +3 lines: its H3_TOKENS entry is the attribute it adds, counted at the seat's sha). H4: no Go-only behavioral directory. S1 census, optin.py check (CHECK PASS), no conflict markers. The same arms ran
# in the fixup's step 6 on the tree it was about to commit, so this battery is launched in PRECHECK_MODE=abort.
stamp "PRE GOROOT verified before export: spelling=toolchain's own, VERSION go1.24.13, no forward slash; DEADLINE='$DEADLINE' ($DL_SRC; epoch $DL_EPOCH) SWEEP_ROW_CAP=$SWEEP_ROW_CAP lock=$LOCK GOMODCACHE(go env)=$(go env GOMODCACHE | tr -d '\r') :: LIVE gate before the lock: ${LIVEV:-unread}"
hp precheck "$WW" "$MASTER" "$(cygpath -w "$SEATS_EFF")" head > "$LOGDIR/precheck.log" 2>&1; prc=$?
stamp "PRE-1 precheck rc=$prc :: $(grep -a '^PRECHECK' "$LOGDIR/precheck.log") :: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 4 | tr '\n' ' ' | cut -c1-500)"
grep -aE '^ok +(COUNT|REG|BOTH:|ROSTER|H3|H4)' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  PRE-1 ${l:0:230}"; done   # BOTH: its summary line (one ok line per merged path is in the log)
grep -a '^NOTE' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  PRE-1 ${l:0:300}"; done
# PRE-1b: the rule leg C's TestOutputComparisonListMatchesConsoleOutputAttribute enforces, read from the tree: 0
# attribute-not-listed, 0 listed-no-attribute, 0 duplicates (TRAIN M's run3: 724 == 724; N's library projects register
# in three files and not in OutputComparisonTests.cs).
hp outparity "$WW" > "$LOGDIR/outparity.log" 2>&1; oprc=$?
stamp "PRE-1b outparity rc=$oprc :: $(head -n 1 "$LOGDIR/outparity.log" | tr -d '\r')"
[ "$oprc" = 0 ] || prc=$((prc + 10))
# PRE-1c: the host censuses at HEAD (TRAIN L's r-m4 and r-m6, now on master: a REGRESSION reading for M, whose F4 and F8
# edit testConversion.go): no committed stdlib host carries a module-path argument; GoDefaultGodebug is named by exactly
# 3 corpus files (measured the same 3 at aa0a07d5fd and at M's pre-map union; N: the same 3 at 8f46a9adae and at five N
# seat tips, read with git 2026-10-03; N's battery read exactly the 3 at 59ee0d21bf; no O row touches GoDefaultGodebug
# or a committed test host: read with git, each row's merge-base..sha). P: c2-sibling-package-name-r2 commits three
# test hosts (their namespace line moves with the seat's rule); both censuses still read 0 and exactly the 3 at the
# pre-map's chain head (git grep, 2026-10-04). Re-read after the sweeps (leg HS).
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

# PRE-2: sweep coverage (225 = tP-i7-sweeps.txt + tP-i9-shard.txt, disjoint; both lists are M's, N's (and L's) bytes,
# and no O row adds or drops a roster row: the two C1 banks annotate 22 rows for darwin, which no windows list reads; no
# P row touches the roster at all). PRE-3: the reflect-bridge canaries, derived now (the i9's crash-class seat changes golib's
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
  local P left td try=0 leftnames
  P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  # BATTERY STOP 1 at O (2026-10-04 06:30, bat1): after-GT read 'purged=3049 remaining=1' and ABORTED in the same second;
  # the one directory (src/tests/Behavioral/TypeAssert/obj, holding only an EMPTY Debug folder) deleted by hand minutes
  # later with no process to kill: a TRANSIENT handle (a build node or test host letting go late), not a leftover. The
  # purge had no retry. It now waits and retries up to 6 times (5 s apart), names what is left on each pass, and aborts
  # only when a directory SURVIVES all of them -- that is still a process to find by PID, never by name (floor 5).
  while [ "$left" != 0 ] && [ "$try" -lt 6 ]; do
    try=$((try + 1)); leftnames=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | head -n 5 | tr '\n' ' ')
    stamp "  PURGE($1) retry $try of 6: $left left [$leftnames]"
    sleep 5
    find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
    left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  done
  td=$(git status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td retries=$try free=$(freegb)G"
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
  # Candidates are processes whose command line names the tP worktree. The pattern is assembled inside PowerShell from
  # pieces, so the querying shell's own command line cannot match it, and the querying process is excluded (floor 5).
  # COORD kills any survivor BY PID after reading the list.
  # Verify round 2 (the test-host legs are capped now): a testhost's command line need not name the worktree, so the
  # EXECUTABLE PATH is matched too, and listed: PID, name, path, then the command line.
  local f="$LOGDIR/timeout-orphans-$(echo "$1" | tr '/:' '._').txt"
  powershell -NoProfile -Command "\$p1 = 'go2cs-tmp-coord' + [char]92 + 'tP' + [char]92; \$p2 = 'go2cs-tmp-coord' + '/' + 'tP' + '/'; Get-CimInstance Win32_Process | Where-Object { \$_.ProcessId -ne \$PID -and ((\$_.CommandLine -and (\$_.CommandLine.Contains(\$p1) -or \$_.CommandLine.Contains(\$p2))) -or (\$_.ExecutablePath -and \$_.ExecutablePath.Contains(\$p1))) } | ForEach-Object { '{0} {1} {2} :: {3}' -f \$_.ProcessId, \$_.Name, \$_.ExecutablePath, \$(if (\$_.CommandLine) { \$_.CommandLine.Substring(0, [Math]::Min(220, \$_.CommandLine.Length)) } else { '' }) }" > "$f" 2>&1 < /dev/null
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

ufarm(){ # label, list file (git status '?? <path>' lines) -- P (P6): the UF reading with its KNOWN class
  # O's i9 shard read 'UF 1 SOFT' on src/core/weak/.editorconfig (COORD 08:21: RULED a known class; P2 sized it, 09:19:
  # two packages, math/bits and weak, option 1 = commit the merged files, which is the row p2-test-warning-entries).
  # With that row seated (UF_ROW, PRE-D 6) both paths are tracked and the EXPECTATION is 0: any untracked path is a
  # finding. Without it, a path of UF_KNOWN is stamped KNOWN and every OTHER path is the finding, by name.
  local lbl=$1 f=$2 l p kn='' ot='' nk=0 no=0
  while IFS= read -r l; do
    p=${l#'?? '}; p=${p#\"}; p=${p%\"}
    [ -n "$p" ] || continue
    case " $UF_KNOWN " in *" $p "*) kn="$kn$p "; nk=$((nk + 1)) ;; *) ot="$ot$p "; no=$((no + 1)) ;; esac
  done < "$f"
  if [ -n "$UF_ROW" ]; then
    stamp "  $lbl: untracked, not-ignored paths under src/core=$((nk + no)) (EXPECT 0: $UF_ROW is a row, so the -tests-only .editorconfig files are tracked) $(echo "$kn$ot" | cut -c1-400)"
    [ $((nk + no)) = 0 ] || finding "$lbl: $((nk + no)) untracked, not-ignored path(s) under src/core with $UF_ROW seated ($(basename "$f")): $(echo "$kn$ot" | cut -c1-300)"
  else
    stamp "  $lbl: untracked, not-ignored paths under src/core=$((nk + no)): KNOWN class (the -tests-only .editorconfig; p2-test-warning-entries is NOT a row)=$nk [${kn% }]; other=$no (EXPECT 0) $(echo "$ot" | cut -c1-400)"
    [ "$no" = 0 ] || finding "$lbl: $no untracked, not-ignored path(s) under src/core OUTSIDE the known class ($(basename "$f")): $(echo "$ot" | cut -c1-300)"
  fi
}

# ================================================================================================= LEGS
# LEG C -- the full converter suite, NO -short (the module integration tests t.Skip under -short; CM in leg MOD reads
# them by name). P: gates every P converter row (the LEG -> SEAT MAP above: 15 rows change converter source, the release
# fix among them; the suite carries the i9's write-if-changed tests and P2's diagnostics and untyped-region tests).
# N's note: gates every N converter seat: the converter's own tests of
# c2-native-array-view, c2-s3b-nuget-map, r-module-license, c2-nugetgo-id-pattern (the three go.-ID refusals),
# g-using-static-alias-guards, r-shadowed-import-alias, r-module-driver-gomod-less, c2-generic-map-chan,
# c2-multi-func-define, c2-named-composite-infer, g-slice-view-note; p1-hashset-module (the converter now REQUIRES
# github.com/ritchiecarroll/hashset v1.0.0 and src/go2cs/HashSet.go is gone: the module must resolve from the H: module
# cache, GOFLAGS empty); p2-n-cleanup (it deletes four census guards and two dead helpers, with their projitems lines);
# the byRefReceiverGuard (it walks ..\core ON DISK: this leg runs FIRST, on a tree no -tests run has written; the i9's
# forwarders seat moves its allowlist), repoguard, and the projitems integrity tests.
# EXPECT ok throughout. The suite reads git state for the fixture guards: -count=1, on the COMMITTED union.
leg C conv-suite bash -c 'cd src/go2cs && go test -count=1 -timeout 40m ./...'
stamp "  C: $(grep -aE '^(ok|FAIL|---)' "$LOGDIR/conv-suite.log" | tr '\n' ' ' | cut -c1-500) :: projitems FAIL lines: $(grep -aE 'TestProjitems[A-Za-z]*' "$LOGDIR/conv-suite.log" | grep -ac FAIL) :: attribute-guard FAIL lines: $(grep -aE 'TestOutputComparisonListMatchesConsoleOutputAttribute|TestBehavioralPackageInfoCarriesNoDuplicateAttribute' "$LOGDIR/conv-suite.log" | grep -ac FAIL)"
# r-m3's integration test (TRAIN L, now on master) leaves its driver binary in TEMP (os.MkdirTemp('', 'go2cs-m3-driver'), never removed):
# count, then remove exactly those directories (the name is the test's own prefix; nothing else in TEMP is touched).
nd=$(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l)
find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' -exec rm -rf {} + 2>/dev/null
stamp "  C: r-m3 driver dirs left in TEMP by the suite: $nd (removed: $(( nd - $(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l) )))"

# LEG CC -- NEW at N (item N6): src/tools/comparison-classifier, a Go module of its own (its go.mod: no requirements,
# go 1.23.12) that no other leg and no CI job runs; gate-forensics relies on its host-crash short-circuit (a crashed
# host is classified before its verdicts are read). '-count=1' (no cached pass: leg C's rule) with the 120 s limit COORD
# named. EXPECT rc 0, an 'ok' line and no FAIL line. A non-zero rc reaches NONZERO LEGS by itself; a green rc with no
# 'ok' line is a finding (a module that ran no package proves nothing).
leg CC comparison-classifier bash -c 'cd src/tools/comparison-classifier && go test -count=1 ./... -timeout 120s'
ccrc=$LEG_RC
stamp "  CC (EXPECT rc 0, 'ok', no FAIL): rc=$ccrc :: $(grep -aE '^(ok|FAIL|---|panic)' "$LOGDIR/comparison-classifier.log" | tr '\n' ' ' | cut -c1-300)"
[ "$ccrc" = 0 ] && grep -aqE '^ok ' "$LOGDIR/comparison-classifier.log" && ! grep -aqE '^(FAIL|--- FAIL)' "$LOGDIR/comparison-classifier.log" || finding "CC comparison-classifier: rc=$ccrc, no 'ok' line or a FAIL line (comparison-classifier.log)"

# LEG CB -- the union's tests BY NAME (critic 0 #2): leg C runs without -v and reads package ok/FAIL only, so a
# missing, renamed or skipped test reads green there. Each name below must print '--- PASS', and no SKIP/FAIL line.
# (TRAIN M's description of its 15 drafted rows, kept as provenance; N's names are PRE-D's CB_TESTS, derived.)
# Names read at TRAIN M's pre-map union 8d7305053f (git grep 'func <name>('), by seat:
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
# and the scanner control PASS. N, O and P: a regression reading; the floor is the previous train's own stamp (at P:
# TRAIN O's bat2 read 'tracked fixtures 1291 · current 1291 · stale 0' at 06:49 on 2026-10-04, as N's battery did); no
# P row adds a fixture under src/core (read with git: each row's merge-base..sha; the two .editorconfig that
# p2-test-warning-entries adds are converter emission, which the guard does not count: O added 11, 1291 unchanged).
leg FX fixture-currency bash -c "cd src/go2cs && go test -count=1 -timeout 15m -v -run 'TestTrackedFixturesMatchPinnedGoRoot|TestFixtureCurrencyScannerFires' ./internal/repoguard/"
stamp "  FX: $(grep -aE -- '--- (PASS|FAIL|SKIP)|tracked fixtures|other cached' "$LOGDIR/fixture-currency.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-400)"
grep -aqE 'stale 0( |$)' "$LOGDIR/fixture-currency.log" || finding "FX: no 'stale 0' reading in fixture-currency.log"
# Verify round 1: 'stale 0' alone passes a union whose ten new fixtures are tracked but OUTSIDE the guard's scan. The
# floor is sourced without inventing 1291: TRAIN L read 1281 on this box and the seat's commit says the ten "come under
# the fixture-currency guard" (20f932b859), so tracked must exceed 1281 and equal current.
# Verify round 2 (RULED, R1): a FLOOR, stamped with its source: tracked >= TRAIN L's reading on this box, which is read
# from L's own SUMMARY when it is still there (it reads 1281) and is the literal 1281 otherwise; and tracked == current.
FX_FLOOR=$(grep -aoE 'tracked fixtures [0-9]+' "$O_SUMS" 2>/dev/null | tail -n 1 | grep -oE '[0-9]+'); FX_SRC="TRAIN O's own FX stamp ($O_SUMS)"
[ -n "$FX_FLOOR" ] || { FX_FLOOR=1291; FX_SRC="the literal 1291, TRAIN O's (and N's and M's) reading (O's SUMMARY is not readable at $O_SUMS)"; }
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
    bash -c 'cp -r "$SRC/src" "$DST/src" && cp "$SRC/VERSION" "$DST/VERSION" && printf "\n// TRAIN P FXc2 plant\n" >> "$DST/src/go/printer/testdata/alignment.golden"'
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

# LEG E -- whole-corpus emission check, M=TRAIN O's landed master vs the fixup HEAD, x3 targets, planted controls. Every
# P -stdlib footprint must be IN THE TREE: g-sort-self-capture's sort/sort.cs (3 lines, committed in-seat: THE RELEASE
# FIX's corpus half), c2-sibling-package-name-r2's four generated .cs and two production csproj (fips140deps and
# internal/trace/internal/testgen/go122: the namespace keeps the directory), c2-literal-float-fold's
# internal/trace/traceviewer/mmu.cs, p2-test-warning-entries' two .editorconfig (-tests sections only: a -stdlib run
# owns the production sections and leaves the others alone, warningEntries.go), g-method-value-fm-record-r3's 11
# package_info.cs and the else-if seat's 123 (nine files are both rows': the -r3 was regenerated ON fm-record-r3, so
# the shared GoPositionMap lines carry both changes), and whatever any other converter row reaches (the fixup's step 4
# stops on it, so at the fixup HEAD it reads 0; c2-named-basic-conv's footprint was PREDICTED 0 from a census and is
# first MEASURED by the fixup's check). csproj: the sibling seat's csproj are SEAT EDITS to csprojdrift.
# P (P10): i9-incremental-cs-writes-r2 is IN the union's converter, and this check converts ONE target per run, which
# is not the census path (main.go: only two or more -platforms targets go through runCensusTarget, which sets
# alwaysWriteSources), so the U arm writes a source only when its bytes differ from the seeded root's. EXPECT the U
# arm's written= far below the M arm's (O read 1830 / 1901 / 1905 in BOTH arms; at P the M arm is O's converter and
# still writes every source). The verdicts do not move: drift is a subset of written, and a file the U run leaves
# unwritten equals its seed; the two plants still fire (their line makes the seeded file differ from the emission).
# written= is a READING, never a gate. The one consumer of 'which targets emit a path' that the change DID reach is
# tP-regen-apply.py's rule 4 (the fixup's step 4), which now also reads the base arm's written lists.
# EXPECT plants OK, 6 x rc 0, union-attributable 0, csproj drift explained (seat-edit), HANDOWN 0 (runtime/managed_impl.cs
# is the hand-owned file fm-record-r3 edits, golib's files are g-debugger-views': a conversion must not write them),
# RUNTIME-MAP 0 (fm-record-r3 and the else-if seat re-encode runtime/*/package_info.cs and commit them),
# editorconfig-deleted 0.
# O's note: the four O footprint rows (xsys-libc's vendor file, B1's os_windows.cs regenerated by the fixup, p2's 8 .cs
# and 11 .editorconfig, fm-record's package_info.cs); .editorconfig is ORDINARY EMISSION since O (O9); a committed
# .editorconfig the union converter DELETES is union-attributable drift (review round 1, MS24). O's bat2 read
# attributable 0, standing drift 1 / 3 / 3, handown 0, editorconfig-written 0, deleted 0.
leg E emitcheck env W="$W" M="$MASTER" U="$EXPECT_HEAD" LOGDIR="$LOGDIR" TAG=emitcheck TP_LOCK_HELD=1 bash "$SD/tP-emitcheck.sh"   # verify round 2: the check has guards of its own for a HAND launch; the battery holds the lock and says so
erc=$LEG_RC
stamp "  E: $(grep -aE '^(EMIT |PLANT|windows:|linux:|darwin:|UNION-ATTRIBUTABLE TOTAL|CSPROJ|EVIDENCE|ABORT)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-1800)"
# Verify round 1: at TRAIN L the same stamp matched 2499 characters and the cut ended inside 'UNION-ATTRIBUTABLE TOTAL',
# so the verdict never reached SUMMARY; M's new 'HANDOWN derived' line would take another 500. The verdict and the three
# named readings get their own stamp, and a hand-owned file written by a conversion (stated in the verdict, not part of
# the emission check's rc) or a missing verdict is a FINDING here, as it is a die in the fixup.
ev=$(grep -a '^EMITCHECK VERDICT' "$LOGDIR/emitcheck.log" | tail -n 1 | tr -d '\r')
stamp "  E verdict: ${ev:-NO VERDICT LINE} :: $(grep -aE '^(RUNTIME-MAP|HANDOWN hand-owned|CSPROJ rc)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-400)"
case "$ev" in *"handown-written=0 "*) ;; *) finding "E: a hand-owned file written by a conversion, or no verdict (${ev:-none})" ;; esac
stamp "  E .editorconfig (a READING since O, O9: ordinary emission; review round 1: a committed one a conversion DELETED is union-attributable drift, MS24): $(printf '%s' "$ev" | grep -oE 'editorconfig-(written|deleted)=[0-9]+' | tr '\n' ' ')$(grep -a '^EDITORCONFIG DELETED' "$LOGDIR/emitcheck.log" | head -n 3 | cut -c1-120 | tr '\r\n' '; ') ::$(grep -a '^EDITORCONFIG derived' "$LOGDIR/emitcheck.log" | tail -n 1 | cut -c1-200)"
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
    leg "E-bisect-$bs" "emitcheck-bisect-$bs" env W="$W" M="$(git rev-parse "$bm^1")" U="$bm" LOGDIR="$LOGDIR" TAG="emitcheck-bisect-$bs" CSPROJ_GATE=0 TP_LOCK_HELD=1 bash "$SD/tP-emitcheck.sh"
    stamp "  E-bisect-$bs (M=$(git rev-parse --short=10 "$bm^1") vs U=$(git rev-parse --short=10 "$bm"), that seat's merge alone): $(grep -aE '^(UNION-ATTRIBUTABLE TOTAL|EMITCHECK VERDICT)' "$LOGDIR/emitcheck-bisect-$bs.log" | tr '\r\n' '  ') :: files: $(cat "$LOGDIR/emitcheck-bisect-$bs"/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u | head -n 12 | tr '\n' ' ')"
  done
  # Verify round 2 (R1: a list that is short for the union is a finding, never silence): the files the main leg names
  # and NO arm names belong to a converter seat outside E_BISECT_SEATS, or to an interaction of seats.
  eun=$(LC_ALL=C comm -23 <(cat "$LOGDIR/emitcheck"/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u) <(cat "$LOGDIR"/emitcheck-bisect-*/attr-*.txt 2>/dev/null | cut -d' ' -f1 | LC_ALL=C sort -u) | tr '\n' ' ')
  [ -z "$eun" ] || finding "E-bisect: file(s) the main leg reads union-attributable and no bisected seat's arm names: [$(echo $eun | cut -c1-400)] -- the converter seats not bisected are [${ebrest% }] (PRE-D): run their arms (tP-emitcheck.sh by hand, or E_BISECT_SEATS)"
else
  stamp "  E-bisect: not run (E union-attributable=${EATTR:-unread}; rc=$erc)"
fi

# LEG G1/G2 -- the roster guard under pwsh 7 and Windows PowerShell 5.1. P: a REGRESSION reading (no P row touches the
# roster or the guard; O's bat2 read 1994 checks, 225 rows, 223 linux annotations, 0 execution configs under both
# editions). O: the gate of c1-darwin-pilot-bank and
# c1-darwin-wave2-bank (22 darwin annotations; the header numbers recomposed from the merged table), G2 being the
# Windows PowerShell 5.1 reading both banks owe (they measured pwsh 7: 1994 checks); EXPECT pass x2, the check count
# REPORTED (it moves with 22 more annotations), 0 rows with an execution config. N's note: N's roster gate (item N3). g-slog-roster-tc0
# (582de36d1b) drops log/slog's 'execution: release-tiered', the last annotated row, rewrites its proof page and the
# roster's sentence ('No row opts back out'). EXPECT pass under both editions and EXACTLY $EXEC_ROWS_EXPECT (0) rows with an
# execution config: the guard prints the count whatever it is ('0 with an execution config', src/check-roster-format.ps1
# at 582de36d1b). TRAIN M's run3 read '1994 checks pass (225 rows, 223 with a linux annotation, 1 with an execution
# config, 5 excluded)': read the check count, do not carry it.
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

# LEG IDC -- a REGRESSION reading at P (no P row touches the census script; O's bat2 read pass=217 fail=0, 94 s). NEW at
# O (O7): coord-census-dotted-handle (COORD's row, 2b6213d821) changes the fleet's ONE identifier census,
# .claude/coord-scripts/coord-identifier-census.sh (a tracked file no other leg or test runs: the Go guard
# TestNoFleetIdentifiersInTrackedFiles is a separate instrument). Its own self-test is the seat's gate: the triad of PLANTS
# that must refuse, KNOWN NEGATIVES that must pass and the DECLARED SET, with synthetic handles (no box name is read).
# EXPECT rc 0, 'SELF-TEST PASSED', 'fail=0' and pass >= IDC_MIN (217: the seat's own reading, '217/0 with four refusing
# siblings', ledger 2026-10-03 20:08). Seconds.
IDC_MIN=217
leg IDC identifier-census bash .claude/coord-scripts/coord-identifier-census.sh selftest
idcrc=$LEG_RC; idcl=$(grep -a 'SELF-TEST: pass=' "$LOGDIR/identifier-census.log" | tail -n 1 | tr -d '\r')
idcp=$(printf '%s' "$idcl" | sed -nE 's/.*pass=([0-9]+).*/\1/p'); idcf=$(printf '%s' "$idcl" | sed -nE 's/.*fail=([0-9]*).*/\1/p')
stamp "  IDC (EXPECT rc 0, SELF-TEST PASSED, fail=0, pass >= $IDC_MIN): rc=$idcrc :: ${idcl:-NO SELF-TEST LINE} :: $(grep -a '^SELF-TEST \(PASSED\|FAILED\)' "$LOGDIR/identifier-census.log" | tail -n 1 | tr -d '\r')"
[ "$idcrc" = 0 ] && grep -aq '^SELF-TEST PASSED' "$LOGDIR/identifier-census.log" && [ "${idcf:-0}" = 0 ] && [ -n "$idcp" ] && [ "$idcp" -ge "$IDC_MIN" ] || finding "IDC identifier-census self-test: rc=$idcrc, '${idcl:-no SELF-TEST line}' (expected PASSED, fail=0, pass >= $IDC_MIN; identifier-census.log)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes after IDC (restored): $(git status --porcelain | grep -v '^??' | head -n 4 | tr '\n' ' ')"; restore_paths IDC; }

# LEG SI + SIc -- check-solution-integrity: the projects P adds (PRE-D's NDIRS: 8 top-level projects at P's draft list,
# 16 csproj with their sub-packages, none platform-exclusive, all registered in go2cs.slnx: 1111 -> 1126 lines in the
# pre-map's chain, one key renamed by the sibling seat). O's bat2 read 821 behavioral projects registered and the
# control's six cycles. O added 18 (slnx +18 in its premap's chain C). N's note: (12 top-level behavioral projects with a
# csproj at the 27-row list, read with git; 11 of them registered in go2cs.slnx, the platform-exclusive
# NativeFieldPortAlias excepted by design, as the six linux-only projects at the base; TRAIN M's run3 read 787
# registered). It also
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
# comment g-slog-roster-tc0 rewrites, and the seat also rewrites comment lines of run-validated-sweep.ps1 itself; a
# BOM-less non-ASCII character in a .ps1 is exactly what broke Windows PowerShell 5.1 at L).
# TRAIN L and TRAIN M read '62 checks, 0 violations' and no N row touches the self-test (read with git 2026-10-03):
# EXPECT the same line. (L's 'binlog
# asserts' count is dropped: it read 0 under 5.1 on a passing leg, so it was never a reading of the five asserts.)
leg ST51 sweep-selftest51 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST51 (EXPECT 0 violations, checks >= 62): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest51.log" | tail -n 1 | cut -c1-200)"
leg ST7 sweep-selftest7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST7 (EXPECT 0 violations, checks >= 62): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest7.log" | tail -n 1 | cut -c1-200)"
# Verify round 2: the stamps said EXPECT and the gate was rc alone, and a self-test that ran 0 checks prints
# '0 checks, 0 violations' and exits 0 the same way (its last two lines). A floor (R1): 0 violations AND checks >= 62,
# TRAIN L's and TRAIN M's reading under both editions; no N row touches the self-test.
for st in 51 7; do
  stn=$(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest$st.log" | tail -n 1 | sed -nE 's/.*: ([0-9]+) checks, 0 violations.*/\1/p')
  [ -n "$stn" ] && [ "$stn" -ge 62 ] || finding "ST$st: no '<n> checks, 0 violations' line with n >= 62 (read '${stn:-none}'; TRAIN L read 62)"
done

# LEG NV -- P (THE RELEASE TRAIN): this is the pre-flight the 1.24.13.4 release itself starts with, read here at the
# union under both editions. No P row touches src/push-nuget.ps1 or src/_roster.ps1 (read with git), so it is a
# regression reading whose green is a release precondition. O's and N's notes follow.
# LEG NV -- push-nuget's release pre-flight (it dot-sources _roster.ps1, which g-slog-roster-tc0 edits: a comment; and
# c2-s3b-nuget-map edited src/push-nuget.ps1 ITSELF at N; at O c1-macos-flavors and c1-release-smoke-all-os do (the
# per-flavour pack): this leg is their i7 reading of the pre-flight, the release-smoke CI run their gate, MS23), under BOTH editions,
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
# P: no P row touches src/tools either (read with git 2026-10-04): regression legs; O's bat2 read identity 'ran 43,
# failed 0' and selfdescription 'ran 16, failed 0' under both editions.
# O: no O row touches src/tools (read with git: each row's merge-base..sha), so NG* are regression legs. N's fixup-2
# (r-ps51-selfdesc-count 1ddbac142d, in the base) fixes Test-NugetgoSelfDescription.ps1's PS 5.1 count: EXPECT
# 'ran 16, failed 0' for selfdescription and 'ran 43, failed 0' for identity under BOTH editions (N's battery read the
# 5.1 selfdescription as 'ran 16, failed 4', its finding NGa51:selfdescription).
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
#  (c) the J0 consumer's PackageReference ID, default and nugetgo (L's R8). N (item N5): the default is nugetgo too.
# N: r-module-license adds src/tools/nugetgo/NugetgoLicense.psm1 and c2-nugetgo-id-pattern stacks on it (identity,
# self-description, the pack's license names, the J0 defaults): eight paths under src/tools, so these legs are those two
# seats' i7 acceptance. The parse list and the Test-*.ps1 list are read from the tree, so the new module is parsed and the
# identity script's new arms run without an edit (N is REPORTED; M read identity 29, selfdescription 10).
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
leg NGb51 nugetgo-parse51 powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w "$SD/tP-ng-parse.ps1")" $NGF
stamp "  NGb51 (EXPECT 0 errors under Windows PowerShell 5.1; GATED): $(grep -aE '^(PARSE|NGPARSE)' "$LOGDIR/nugetgo-parse51.log" | tr '\r\n' '  ' | cut -c1-600)"
leg NGb7 nugetgo-parse7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File "$(cygpath -w "$SD/tP-ng-parse.ps1")" $NGF
stamp "  NGb7 (EXPECT 0 errors): $(grep -aE '^NGPARSE' "$LOGDIR/nugetgo-parse7.log" | tr -d '\r')"
J0C=src/tools/j0-uuid-rehearsal/consumer/J0UuidConsumer.csproj
# N (item N5): after c2-nugetgo-id-pattern (a7873ee6c2) the DEFAULT ID is nugetgo.github.com.google.uuid too (the owner's
# ruling of 2026-10-02: every converted module's ID is the nugetgo. form). C2 quoted two reads that print it: the
# j0-consume.ps1 PackageId default, and 'dotnet msbuild consumer/J0UuidConsumer.csproj -getProperty:J0UuidPackageId'.
# Both arms are KEPT: the default arm now expects nugetgo and NOT the go. form; the explicit -p arm expects the same ID
# (it proves the property is still honoured). C2's two reads are added: the property as a leg, the script's default from
# the tree (no run).
leg NGF-j0default j0-consumer-default dotnet msbuild "$J0C" -getItem:PackageReference
j0a=$LEG_RC; j0an=$(grep -acE '"Identity": *"nugetgo\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-default.log"); j0ago=$(grep -acE '"Identity": *"go\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-default.log")
leg NGF-j0nugetgo j0-consumer-nugetgo dotnet msbuild "$J0C" -getItem:PackageReference -p:J0UuidPackageId=nugetgo.github.com.google.uuid
j0b=$LEG_RC; j0bn=$(grep -acE '"Identity": *"nugetgo\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-nugetgo.log")
leg NGF-j0prop j0-consumer-property dotnet msbuild "$J0C" -getProperty:J0UuidPackageId
j0c=$LEG_RC; j0cv=$(tr -d '\r' < "$LOGDIR/j0-consumer-property.log" | grep -vE '^[[:space:]]*$' | tail -n 1 | tr -d ' ')
j0ps=$(git grep -hE '^[[:space:]]*\[string\]\$PackageId = ' HEAD -- src/tools/j0-uuid-rehearsal/j0-consume.ps1 | sed -nE "s/.*PackageId = '([^']*)'.*/\1/p" | head -n 1)
stamp "  NGF-j0 (EXPECT nugetgo.github.com.google.uuid in all four: the default PackageReference (and no go. form), -p, -getProperty, j0-consume.ps1's default): default rc=$j0a matched=$j0an go.-form=$j0ago :: nugetgo rc=$j0b matched=$j0bn :: property rc=$j0c '${j0cv:-unread}' :: script default '${j0ps:-unread}'"
[ "$j0a" = 0 ] && [ "$j0an" = 1 ] && [ "$j0ago" = 0 ] && [ "$j0b" = 0 ] && [ "$j0bn" = 1 ] && [ "$j0c" = 0 ] && [ "$j0cv" = nugetgo.github.com.google.uuid ] && [ "$j0ps" = nugetgo.github.com.google.uuid ] || finding "NGF-j0 consumer ID (default rc=$j0a n=$j0an go.=$j0ago; nugetgo rc=$j0b n=$j0bn; property rc=$j0c '${j0cv:-unread}'; script default '${j0ps:-unread}')"

[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after the read-only legs: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"

# LEG 2b -- go2cs.slnx END TO END (nothing else builds this solution, so a broken member rots unseen). P's golib and gen
# rows owe it: g-debugger-views (golib GoFrame / array / channel / map / slice / string / the heap box / Goroutine.cs:
# debugger views; go2cs-gen Common.cs + 9 templates: DebuggerNonUserCode on generated plumbing; its generated-tree and
# slnx readings were taken on N's union 59ee0d21bf, BEFORE O's four gen rows), c2-anon-struct-named-conv
# (ImplicitConvGenerator.cs, a file O's i9-generator-skip-records also changed), g-method-value-fm-record-r3
# (GoPositionMapAttribute.cs: a fifth argument) and the 16 csproj P adds. r-csharp-consumer-smoke-r2's go.lib.targets is
# NOT read here (packed; a project reference never imports buildTransitive). O's bat2 read errors 0, gen-load 0, 915
# Project lines. O's note: O's golib and gen
# rows owe it: c1-macos-flavors (golib buildTransitive targets), c1-darwin-xsys-libc (GoCgoDynamicImports.ResolveOrZero),
# c1-release-smoke-all-os, g-float-untyped-const-compare (UntypedInt), g-method-value-names (ж.PointerTokens),
# g-method-value-fm-record (GoPositionMapAttribute); gen: i9-empty-iface-record (ImplementGenerator),
# c2-gen-kind-name-collision and r-reflect-value-equality (TypeGenerator, StructTypeTemplate). N's note: N's golib and gen
# seats owe it: c2-native-array-view (golib's native boxes, unsafe), g-named-pointer-inbound (src/gen), the three g-trim
# seats (golib annotations and the GoFieldMetadata tripwire), c2-generic-map-chan (generator constructor names),
# i9-crosspkg-promoted-forwarders (GOLIB+GEN, '[GoEmbedded]' on interface embeds; the i9 added a go2cs.slnx Debug build
# to its own gates), i9-gomethodvalue-nilfunc (golib reflect), g-cctor-init-frame (runtime managed_impl.cs), and the
# projects the union adds. (M's description follows.) Owed by: i9-crashclass-byref-recv (golib's four binders + src/gen's RecvGenerator and two templates: "a seat's
# proof spans src/gen the moment it touches it: build go2cs.slnx"), c1-darwin-std-hygiene (golib builtin.cs + a new
# file), G (Goroutine.cs: the constructor and Register gain systemBasis), and the seven new behavioral projects. The i9
# read 'rc 0, 875 projects' at L's union + its own project; the union registers 881. EXPECT rc 0, errors 0, gen-load 0;
# the project count is REPORTED.
leg 2b go2cs-slnx dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental
stamp "  2b: errors=$(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/go2cs-slnx.log") gen-load=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log") warnings-line=$(grep -aE 'Warning\(s\)|Error\(s\)' "$LOGDIR/go2cs-slnx.log" | tr '\n' ' ' | cut -c1-120) :: Project lines registered in go2cs.slnx: $(grep -c '<Project Path=' src/go2cs.slnx)"
# Verify round 1: CS8032/CS8034/CS8784/CS8785 are WARNINGS (a generator that failed to load, or threw), so rc stays 0 on
# the train whose i9 seat edits RecvGenerator and two templates: the count is now a finding, not only a stamp.
gl=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log"); [ "$gl" = 0 ] || finding "2b: $gl generator load/exception line(s) in go2cs-slnx.log"

# LEG CT -- N: a regression reading (M's seat landed; no N row touches src/tests/ChannelTests). M's note:
# i9-channeltests-makechan-panic: src/tests/ChannelTests, Debug and Release. The project was RED on master from
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

# LEG GN -- P: c2-anon-struct-named-conv adds ForeignStructTargetConversionTests (its seat read GenTests 93/93; O's bat2
# read 92 at O's union); g-debugger-views changes Common.cs and nine templates with no GenTests class of its own (2b,
# leg 5 and H7 are its union gates). The floor is derived from the tree at HEAD.
# O: c2-gen-kind-name-collision (KindNamedUnderlyingTests) and i9-empty-iface-record (BadImplementRecordTests)
# add GenTests; the floor is derived from the tree at HEAD. N: the gen seats each add GenTests of their own (g-named-pointer-inbound NamedPointerInboundAddressTests,
# c2-generic-map-chan GenericInheritedShellConstructorTests, i9-crosspkg-promoted-forwarders three classes): the floor is
# derived from the tree at HEAD, never below the measured 68. M's note: i9-crashclass-byref-recv touches src/gen (RecvGenerator, the Str and StructType templates), and its seat line
# owes GenTests (tP-seats-draft.txt:4). No command or configuration is ruled: Debug, one run. Verify round 1: the total
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

# LEG TR -- N: r-module-driver-gomod-less edits the testing host itself (src/core/testing/TestHost.cs and
# PackageAncestry.cs: a go.mod-less module's overlay, the preflight's GOFLAGS). M's note follows.
# LEG TR -- BehavioralTests filtered to TestingRuntimeTests (the testing host: the i9's seat marks 25 testing.cs logging
# methods [GoRecv]; F4 and F8 edit testConversion.go, which writes the host). Critic 1 #13: build and test are SEPARATE
# legs, so a build failure reads as BUILD FAILED, never as an empty Passed/Failed stamp. TRAIN L read 26/26.
leg TR-build testing-runtime-build dotnet build src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug -p:UseSharedCompilation=false -p:go2csPath=H:/go2cs-tmp-coord/tP/src/
if [ "$LEG_RC" = 0 ]; then
  # Verify round 1: a filter that matches nothing prints no result line and (INFERRED: vstest) exits 0. 26 = the
  # [TestMethod] lines of TestingRuntimeTests.cs at the pre-map union (no DataRow) and TRAIN L's reading.
  # Verify round 2 (RULED): capped 20m; a floor (R1): Failed 0, Skipped 0, Total >= the [TestMethod] lines of
  # TestingRuntimeTests.cs at HEAD (two testing-host seats ride now, H1 and H2), never below the measured 26.
  capped TR testing-runtime 20m dotnet test src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug --no-build -p:go2csPath=H:/go2cs-tmp-coord/tP/src/ --filter FullyQualifiedName~TestingRuntimeTests
  totalgate TR "$LOGDIR/testing-runtime.log" "$(tmfloor src/tests/Behavioral/BehavioralTests/TestingRuntimeTests.cs)" 26 "the [TestMethod] lines of TestingRuntimeTests.cs at HEAD" yes
else
  stamp "  TR: BUILD FAILED (testing-runtime-build.log: $(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/testing-runtime-build.log") errors) -- the test leg did not run (NOT MEASURED)"
  finding "TR NOT MEASURED: the BehavioralTests build failed"
fi

# LEG GT -- GolibTests, Debug x1 and Release x3 FULL (K's re-read pattern), each with a TRX read BY NAME (a class with 0
# results is NOT FOUND, never green). P: g-debugger-views adds DebuggerViewsTests and changes
# NoUncountedBackingAllocationsTests (a literal neighbour too), i9-tests-host-crash-verdict adds
# HostPackageTimeoutRunningTestsTests (the i9 read GolibTests 3 failed on ITS box, the link-staging trio it knows
# there: this box's reading is the row's first here), all DERIVED by PRE-D; the debugger-views seat read D 1539/0/31 and
# R 1547/0/23 on N's union, O's bat2 read D 1535/0/31 and R 1543/0/23 of 1566: the totals are REPORTED, not asserted.
# g-method-value-fm-record-r3 (GoPositionMapAttribute.cs) is the other golib row.
# O: c1-darwin-xsys-libc (its GolibTests class) and g-float-untyped-const-compare
# (UntypedIntFloatOperandTests) add classes, DERIVED by PRE-D; every golib row reaches the full runs. N: the golib seats are c2-native-array-view (NativeFieldArrayViewTests),
# g-named-pointer-inbound (NamedPointerTokenCarrierTests), g-trim-loud-guards (FieldMetadataGuardTests),
# g-slog-roster-tc0 (StackFirstFrameWarmTests changed), r-m4-link-vendor-arm and r-module-driver-gomod-less
# (ModuleAncestryTests changed), i9-crosspkg (CopyBoundReceiverAllowlistTests changed), g-cctor-init-frame
# (InitFrameNameTests), p2-n-cleanup (Sha3ReinterpretVectorTests changed): all DERIVED by PRE-D; the literal neighbours
# stay (the frame-naming seat g-cctor-init-frame sits beside the creator and traceback classes). M's note follows. "A golib-touching seat runs GolibTests at BOTH configurations": M has three (G,
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
# golden. At N it is the first reading of every seat's goldens beside every other seat's converter change (eleven
# converter seats; c2-nugetgo-id-pattern regenerated 55 behavioral library csproj and g-publish-keep every behavioral
# csproj, both against their own bases), and of the goldens the fixup re-baselined. EXPECT NO REGRESSION. A changed .cs is a seat x seat INTERACTION the
# fixup's own CNR did not see: re-baseline in a new fixup and read that guard isolated -- never waived.
# P: the reconciliation predicts 819 + 16 - 1 = 834 at the 22-row draft (PRE-D prints the derived value; read it
# there). It is the first CNR of g-sort-self-capture's cast beside c2-named-basic-conv's and c2-anon-struct-named-conv's
# conversions (one file, convCallExpr.go) and of the three import / namespace rows together.
# O: the reconciliation predicted 801 + 18 = 819 at the 34-row draft, and bat2 read 819 with the seven skips.
# N is ASSERTED (item N2): CNR_EXPECT_N is PRE-D's derivation (tP-helpers.py cnrexpect: the runner's own predicate over
# the tree at HEAD; TRAIN M read 785 = 791 directories with Go source - 6 platform skips) unless COORD gave one at
# launch, and PRE-D stamped its reconciliation (TRAIN M's 785 + the package directories N adds - the ones it removes;
# the posts name 17 at the draft list). An H4 directory (Go, no C#) is counted by CNR AND reads CHANGED: MS2.
# None of the N dirs may sit in the platform-exclusive skip list (a dir dropped there still reads NO REGRESSION).
# Verify round 2 (R1): NDIRS is PRE-D's (the behavioral projects the union adds, from the tree). No list is written here.
leg 4 cnr powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1
stamp "  4: $(grep -aiE 'NO REGRESSION|REGRESSION|byte-identical|CHANGED|verdict' "$LOGDIR/cnr.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
cnrn=$(grep -aoE 'NO REGRESSION: generated C# and \.csproj are byte-identical across all [0-9]+ behavioral packages' "$LOGDIR/cnr.log" | grep -oE '[0-9]+' | tail -n 1)
cnrskip=$(for d in $NDIRS; do grep -aqE "^\s+$d \[" "$LOGDIR/cnr.log" && printf '%s ' "$d"; done)
# N: CNR's OWN skip list (its SKIPPED block, one '    <name> [<platforms>]' line each) against PRE-D's derived one. Equal =
# every skipped package is declared exclusive by its own package_info.cs, an N dir among them included (NativeFieldPortAlias
# at the draft list): stated, not a finding. A difference is the runner skipping what the tree does not declare, or the
# reverse: a finding.
cnrlogskip=$(tr -d '\r' < "$LOGDIR/cnr.log" | awk '/SKIPPED \(platform-exclusive/ { f = 1; next } f && /^[[:space:]]+[^ ]+ \[/ { print $1; next } f { f = 0 }' | LC_ALL=C sort -u | tr '\n' ' ')
cnrdecl=$(for d in $cnrskip; do case " $CNR_SKIPS " in *" $d "*) printf '%s ' "$d" ;; esac; done)
cnrskip=$(for d in $cnrskip; do case " $CNR_SKIPS " in *" $d "*) ;; *) printf '%s ' "$d" ;; esac; done)
stamp "  4 asserts: N=${cnrn:-unread} (EXPECT ${CNR_EXPECT_N:-UNREAD: cnrexpect did not derive it}, $CNR_SRC) skip-list holds N dirs the tree does not declare exclusive: [${cnrskip% }] (EXPECT none; declared, read on linux: [${cnrdecl% }]) CNR's skips [${cnrlogskip% }] vs derived [${CNR_SKIPS% }] skipped(platform-exclusive) header: $(grep -aoE 'SKIPPED \(platform-exclusive, [0-9]+\)' "$LOGDIR/cnr.log" | head -n 1)"
[ -n "$cnrn" ] || finding "CNR N unread: no NO REGRESSION verdict line (read cnr.log: CHANGED and NOT MEASURED lists)"
[ -z "$CNR_EXPECT_N" ] || [ "${cnrn:-x}" = "$CNR_EXPECT_N" ] || finding "CNR N=${cnrn:-unread} != $CNR_EXPECT_N (read the skip list before trusting either)"
[ -z "$cnrskip" ] || finding "CNR platform-exclusive skip list holds new N dir(s) the tree does not declare exclusive: ${cnrskip% }"
[ "$cnrlogskip" = "$CNR_SKIPS" ] || finding "CNR's own skip list [${cnrlogskip% }] != the derived one [${CNR_SKIPS% }] (the runner and the tree disagree on what this host cannot measure)"
# Verify round 1: CNR regenerates in place and a CHANGED golden used to stay in the tree. tP-modules-legs.sh refuses a
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
# P: the projects P adds are PRE-D's NDIRS (8 top-level projects at the draft list), each read isolated below:
# SortMethodSelfCapture (THE RELEASE FIX), SiblingPackageNames + GoHostModuleShadow + SameNameImportAlias (the three
# import / namespace rows' first joint reading, with CrossPkgSameNameAlias and AliasNamespaceShadow in the full
# suite), NamedBasicConversion + AnonStructNamedConversion (H2 x H1 beside the sort cast), LiteralFloatConstFold,
# MethodValueFmRecord. --filter is a SUBSTRING match: each of the eight names selects exactly one project at the
# pre-map's chain head (800 behavioral directories, read with git).
# O's note: the 18 projects O added were each read isolated; the reflect trio ReflectValueMapKeyIdentity /
# ReflectTypedNilStore / ReflectNilMapKey was B6 x B7 x K's first joint reading (all PASS in bat2).
# N: the projects N adds were PRE-D's NDIRS (NativeFieldPortAlias, UsingStaticNamespaceAlias, ShadowedStdlibImportAlias,
# GenericDefinedMapChan, MultiValueFuncLiteralDefine, NamedCompositeGenericArg, the i9's four, NilFuncIfaceMethodValue,
# InitFrameNames at the draft list); the literal guards and neighbours below stay as regression (g-cctor-init-frame names
# init frames in every traceback a guard prints). M's note follows.
# The M guards, by seat. i9-crashclass: PromotedPtrMethodValueSet (prints Go's 63 lines). P2: VariadicClosureShadowParam
# (F5), AliasStructToInterface + AliasStructToInterfaceLib (F3, with its control arm), NamedArrayVsUnnamedCompare (F7),
# PanicOnlyFuncLiteralVar + FuncLiteralDeclaredResultIface (F1+F6). H3: TRAIN L's five checkdead guards, re-read under
# G's classification, the first two with the goldens the fixup re-baselined (ForeverWaitWorkersMainReturns,
# MainSelectForeverWorkerExits, MainSelectForeverAfterFunc: Go exit 0; ChannelReceiveFromNil, ChannelSendToNil: stderr
# 'fatal error: all goroutines are asleep - deadlock!', exit 2 on both sides). --filter is a SUBSTRING match: each name
# below matches exactly one project at the pre-map union EXCEPT AliasStructToInterface, which also selects
# AliasStructToInterfaceLib (so that one leg reads two projects, and the Lib leg reads the library alone).
# Verify round 2 (R1): NGUARDS = PRE-D's lists, in one order with no project read twice: the projects the union adds
# (NDIRS, leg 4's list), the projects whose goldens the fixup commit(s) changed (FXGOLD: derived, so a golden a fixup
# re-baselined is read isolated whichever it is), and the two hazard literals checked against the tree in PRE-D:
# CHECKDEAD_GUARDS (TRAIN L's five) and NEIGH (L's eight, kept for M's reasons: the channel and select projects sit
# beside checkdead, H3; StdoutCloseEofBarrier beside c1-darwin-std-hygiene's builtin.cs; PointerEmbedValueChainPromotion
# beside the i9's forwarders).
# N: a project the tree declares exclusive to another platform (PRE-D's CNR_SKIPS: NativeFieldPortAlias at the draft list)
# cannot be read on this box (the runner skips it, and an isolated --filter would match nothing and exit 2): it leaves the
# isolated list, stamped, and the linux lanes read it (LX, LB).
NSKIPPED=$(for d in $NDIRS $FXGOLD; do case " $CNR_SKIPS " in *" $d "*) printf '%s ' "$d" ;; esac; done)
NGUARDS=$(printf '%s\n' $NDIRS $FXGOLD $CHECKDEAD_GUARDS | grep -vxF -f <(printf '%s\n' $CNR_SKIPS '#none#') | awk '!seen[$0]++' | tr '\n' ' ')
[ -z "$NSKIPPED" ] || stamp "  5: not read isolated here (declared exclusive to another platform; the linux lanes' LX and LB read them): [${NSKIPPED% }]"
stamp "  5 named: NOT-MEASURED=[${NM}] :: N-guard mentions on non-pass lines: $(for g in $NGUARDS $NEIGH; do n=$(grep -aE "\b$g\b" "$LOGDIR/behavioral.log" | grep -aciE 'fail|timeout|mismatch|differ'); [ "$n" != 0 ] && printf '%s=%s ' "$g" "$n"; done)"
for p in $(printf '%s\n' $NGUARDS $NEIGH $NM | awk '!seen[$0]++'); do
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

# LEG PUB -- P: a REGRESSION reading, PUB_MIN 6 kept (O's bat2: rc 0, '6 of 6 published programs match go, none hung',
# 629 s); at P the published programs carry g-debugger-views' golib attributes and the P converter rows' emission.
# LEG PUB -- g-publish-keep's standing gate since N (item N4), src/tests/Behavioral/check-published-output.ps1 (G,
# 3ced87fd40; at N's union also the one-CPU plain ReadyToRun arm of g-typecache-lock-inversion 2af0eeb79b). It publishes
# behavioral projects plain (InterfaceAssertionMapKey and ReflectFieldMetadata, the two that printed WRONG output under
# full trim at TRAIN M's union; DeepEqual and ZeroSizeFieldLayout, controls), the one-CPU arm, and
# InterfaceAssertionMapKey once more through the converter's own single-file profile (src/go2cs/profiles/<rid>.pubxml),
# run 10 times under a 60 s limit. A failed publish or a missing exe is NOT MEASURED and fails the leg; a run past the
# limit FAILS it as HUNG. Each published program's stdout and exit code are compared with `go run .`'s, so go must be on
# PATH. Wall about 5 to 8 minutes (G); capped at 30m (a fired cap stops the battery, lock kept, exit 5).
# EXPECT rc 0 and 'published-output gate: <n> of <n> published programs match go, none hung' with n >= PUB_MIN.
# O (O4): PUB_MIN 5 -> 6. g-single-file-r2r-restore (4b30906aea) makes the single-file arm ReadyToRun again, and its
# seat read the gate '6/6' (COORD 13:27: 'O battery PUB covers the release tree -> PUB_MIN 5 -> 6'): the floor is that
# count, so a shrunk set cannot read green. The CONTROL is documented, NOT re-run here (G's, at 2bb2e5f825's template:
# rc=1 and 'InterfaceAssertionMapKey (single-file) HUNG: 10 of 10 runs did not exit within 60s'); g-single-file-r2r-restore
# measured single-file R2R 0/10 hung on N's union, after the lock fix (the hang's root, tP-README.md MS15).
# ENVIRONMENT (O5, an N lesson): N's draft started pwsh with dotnet10 FIRST on PATH and DOTNET_ROOT unset, and N's
# battery read PUB 'rc=150 :: NO VERDICT LINE' in 2 s at 15:49 (bat1/tN-logs/published-output.log: 'You must install or
# update .NET to run this application ... Framework: Microsoft.NETCore.App, version 8.0.0 ... .NET location:
# <profile>\dotnet10\'): the pwsh global tool (7.4.6, net8.0) resolved its runtime from the dotnet on PATH, which holds
# 10.0 only, and exited 150 (0x80008096 & 0xFF, the host's framework-missing code) before the script ran. Reproduced on
# this box at 20:12 (that PATH: rc=150, the same text; the pinned-go PATH every other pwsh leg uses: rc=0, 7.4.6). So pwsh
# now STARTS under $GOPIN_PATH, as G1 / NV7 / NGa7 / ST7 start it, and the script's own PATH is changed INSIDE pwsh
# before it runs: dotnet10 first, so its `dotnet publish` is the .NET 10 SDK the battery builds with (measured at 20:12:
# (Get-Command dotnet).Source = <profile>\dotnet10\dotnet.exe, dotnet --version 10.0.400). The SDK the leg's dotnet
# resolves is still stamped first, and anything but 10.0.* is a finding. The script publishes under the system temp
# directory (TEMP is H: here) and deletes it; the projects' bin/obj are purged after.
PUB_MIN=6
PUB_PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$ORIGPATH"   # the PATH the SCRIPT sees (set inside pwsh below); pwsh itself starts under GOPIN_PATH
D10W=$(cygpath -w "$HOME/dotnet10")
pubsdk=$(env -u DOTNET_ROOT PATH="$PUB_PATH" dotnet --version 2>&1 < /dev/null | tr -d '\r' | tail -n 1)
stamp "  PUB environment: dotnet --version under the script's PATH = ${pubsdk:-unread} (EXPECT 10.0.*); $(env -u DOTNET_ROOT PATH="$PUB_PATH" go version 2>&1 < /dev/null | cut -d' ' -f3-4); pwsh starts under the pinned-go PATH (O5), then prepends $D10W"
case "$pubsdk" in 10.0.*) ;; *) finding "PUB: the leg's dotnet resolves to '${pubsdk:-unread}', not a 10.0 SDK (the publish would not be the battery's)" ;; esac
PUB_CMD="\$env:PATH = '$D10W' + [IO.Path]::PathSeparator + \$env:PATH; & ./check-published-output.ps1; exit \$LASTEXITCODE"
capped PUB published-output 30m env -u DOTNET_ROOT PATH="$GOPIN_PATH" PUB_CMD="$PUB_CMD" bash -c 'cd src/tests/Behavioral && "$0" -NoProfile -Command "$PUB_CMD"' "$PWSH"
pubrc=$LEG_RC
publ=$(grep -a 'published-output gate:' "$LOGDIR/published-output.log" | tail -n 1 | tr -d '\r')
read -r pubm pube <<< "$(printf '%s' "$publ" | sed -nE 's/.*gate: ([0-9]+) of ([0-9]+) published programs match go, none hung.*/\1 \2/p')"
stamp "  PUB (EXPECT rc 0 and '<n> of <n> published programs match go, none hung', n >= $PUB_MIN): rc=$pubrc :: ${publ:-NO VERDICT LINE} :: $(grep -aE ' (HUNG|DIFFERS|NOT MEASURED)' "$LOGDIR/published-output.log" | head -n 4 | tr -s ' ' | tr '\r\n' '  ' | cut -c1-400)"
[ "$pubrc" = 0 ] && [ -n "${pubm:-}" ] && [ "$pubm" = "${pube:-x}" ] && [ "$pubm" -ge "$PUB_MIN" ] || finding "PUB published-output gate: rc=$pubrc, verdict '${publ:-none}' (expected '<n> of <n> ... none hung' with n >= $PUB_MIN; published-output.log)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes after PUB (saved as PUB-rewrites.patch, then restored so H7 and MOD read HEAD): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths PUB; }
purge after-PUB

# LEG H7 -- compile parity per flavour. P: sort/sort.cs (the release fix), the sibling seat's two packages (their
# namespace moves: every importer of crypto/internal/fips140deps and internal/trace/internal/testgen/go122 must still
# bind on each flavour), mmu.cs, the position-map lines of 123 + 11 package_info.cs (the per-GOOS runtime ones
# included), golib's debugger views on all three flavours.
# O: c1-darwin-xsys-libc's darwin vendor x/sys/cpu file and golib's
# GoCgoDynamicImports, B1's regenerated runtime/windows/os_windows.cs, p2's runtime1.cs x3 and fm-record's
# runtime/{darwin,linux,windows}/package_info.cs, every golib row on all three flavours. N: c2-native-array-view's darwin and linux corpus files (net/darwin/cgo_unix.cs,
# syscall/{darwin,linux}/syscall_unix.cs) and golib's native boxes, i9-crosspkg's 100 corpus files, every golib seat on
# all three flavours. M's note follows. linux: the i9's seat edits internal/syscall/unix/linux/siginfo_linux.cs, which
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

# LEG WE + WEc -- P: p2-test-warning-entries changes the gate itself: arm B also builds a package's *.tests.csproj for a
# _test.cs section, and the row commits math/bits' and weak's entry files (one CS0219 section each, three flavours).
# With the row EXPECT 12 entry files and 14 entries (O's bat2 read 10 files, 12 entries, PASS, stale 0, missing 0,
# 4101 s) and a longer arm B (two test projects x three flavours). Without the row the gate is O's, unchanged. The
# control WEc is arm A's and does not move.
# LEG WE + WEc -- NEW at O (O8): p2-converter-warning-clears' own standing gate, src/check-warning-entries.sh ("a TRAIN
# and PRE-RELEASE gate": its header). The converter writes a per-file .editorconfig beside a package's csproj that turns
# off CS0219 / CS0649 / CS0675 in the files whose Go source holds the fact (warningEntries.go); the gate checks the
# entries against the COMPILER both ways, per flavour: A MISSING (build the whole stdlib in Release; any of the three
# warnings in a converted src/core file with no entry fails) and B STALE (move every marked entry file aside, build the
# packages that held one; every entry must warn on a flavour its comment names; the files come back byte for byte and the
# tree is checked for deletions). Run here for all three flavours (P2 read PASS on linux, windows and darwin), after H7,
# from a clean tree (the gate refuses an uncommitted .editorconfig), MSYS path conversion ON (dotnet.exe gets the
# script's unix paths). EXPECT rc 0 and a final 'PASS'. Then its POSITIVE CONTROL, WEc ('--control': a planted unread
# 'int zz = 1;' in a package that holds no entry; A must FAIL and name it; the script exits 0 only when it did), on
# windows only (one more stdlib build): EXPECT rc 0 and 'CONTROL CAUGHT'. INFERRED wall: about an hour in all (one
# Release stdlib build per flavour plus the entry packages, then one more for the control; H7's Debug builds read about
# 725 s each). Capped 150m and 45m. Build output purged after each.
WE_FLAVOURS=${WE_FLAVOURS:-linux,windows,darwin}
mkdir -p "$LOGDIR/we" "$LOGDIR/wec"
capped WE warning-entries 150m bash src/check-warning-entries.sh --flavours "$WE_FLAVOURS" --logs "$LOGDIR/we"
werc=$LEG_RC; wel=$(grep -avE '^\s*$' "$LOGDIR/warning-entries.log" | tail -n 1 | tr -d '\r')
stamp "  WE (EXPECT rc 0 and a final PASS; flavours $WE_FLAVOURS): rc=$werc final='${wel:0:40}' :: $(grep -aE '^(entry files:|A \[|B \[[a-z]+\] build|  (MISSING|STALE|UNMEASURED|FAIL))' "$LOGDIR/warning-entries.log" | head -n 12 | tr -s ' ' | tr '\r\n' '  ' | cut -c1-700) :: ok entries=$(grep -ac '^  ok   core/' "$LOGDIR/warning-entries.log") stale=$(grep -ac '^  STALE ' "$LOGDIR/warning-entries.log") missing=$(grep -ac '^  MISSING ' "$LOGDIR/warning-entries.log") unmeasured=$(grep -ac '^  UNMEASURED ' "$LOGDIR/warning-entries.log")"
[ "$werc" = 0 ] && [ "$wel" = PASS ] || finding "WE check-warning-entries.sh: rc=$werc, final line '${wel:0:60}' (expected rc 0 and PASS; warning-entries.log, $LOGDIR/we)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes after WE (the gate puts its entry files back byte for byte; saved and restored): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths WE; }
purge after-WE
capped WEc warning-entries-control 45m bash src/check-warning-entries.sh --control --only-a --flavours windows --logs "$LOGDIR/wec"
wecrc=$LEG_RC; wecm=$(grep -ac 'CONTROL CAUGHT' "$LOGDIR/warning-entries-control.log")
stamp "  WEc (EXPECT rc 0 and 'CONTROL CAUGHT': the planted file named by A): rc=$wecrc caught=$wecm :: $([ "$wecrc" = 0 ] && [ "$wecm" -ge 1 ] && echo 'CONTROL FIRED' || echo 'CONTROL DID NOT FIRE -- WE proves nothing')"
[ "$wecrc" = 0 ] && [ "$wecm" -ge 1 ] || finding "WEc control did not fire (rc=$wecrc caught=$wecm; warning-entries-control.log)"
[ "$(git status --porcelain -- src/core | grep -c 'zz_warning_entries_control')" = 0 ] || finding "WEc left its plant (zz_warning_entries_control.cs) in the tree: remove it by exact path"
purge after-WEc

# LEG MOD -- tP-modules-legs.sh (same env/leg/purge semantics), two halves:
#  (1) TRAIN L's module legs, kept WHOLE as the regression of the -tests -recurse driver, whose test conversion F4
#      (publishTestHost) and F8 (declarationClosureImports) edit: CM (14 module tests + 5 subtests by name with -v),
#      MR2 + JWT (module cache, R6), MR3, MR4 + MR4v + MR4c, MR6 + MR6c, MR6p + MR6e + MR6pc. L's run 2 read all PASS, 920 s.
#  (2) M's REAL MODULES: XS golang.org/x/sync@v0.19.0 and XM golang.org/x/mod@v0.33.0 through
#      'go2cs -tests -recurse <moduleDir> <outRoot>' with -test-timeout $REALMOD_TEST_TIMEOUT stated explicitly. They are
#      P2's fix batch end acceptance (tL-seats-draft.txt:67), read there on a LINUX root box; this is the first WINDOWS
#      reading. Expected per package: x/sync errgroup 5, syncmap 3; x/mod modfile 323, module 16, semver 9,
#      sumdb/dirhash 6, sumdb 4, sumdb/note 7, sumdb/storage 1; zip builds and runs (counts reported). KNOWN
#      non-passes, read BY NAME and stamped KNOWN, never red: semaphore TestWeightedAcquire (the known timing-class
#      non-pass; its cause is NOT MEASURED), and, ONLY for a union without the two host seats, singleflight
#      TestPanicDoChan (host finding H1) and sumdb/tlog TestCertificateTransparency (host finding H2). N: both host
#      seats LANDED at M (master's first-parent merges a92237f025 and 89fded79dc): tP-modules-legs.sh reads them from
#      master's line when the seat list does not hold them, so singleflight 12 and sumdb/tlog 17 are expected.
#      N also: r-module-driver-gomod-less edits the -tests -recurse driver and the testing host (a go.mod-less module,
#      the preflight's GOFLAGS, DefaultGODEBUG at go 1.16 for a go.mod with no go line): these legs are its regression.
#      Anything else is a FAIL. F4's gate rides here: 0 'dotnet timed out' lines.
#      Verify round 3: the controls MR4c / MR6c run TRAIN L seat converters that carry the H2 seat's stream reader
#      (tP-modules-legs.sh ctlconv), because the union's host frames its --json events and an older reader reads none.
# It needs a CLEAN tree (it refuses otherwise): it runs here, after the H7 purges and before PB/S write anything. Phase
# B builds restore from H:\nuget\packages (warm) -- a cold cache with egress down times out (R's first reading did).
# The script exits 4 on any failed verdict (critic 0 #1 / critic 1 #1), so MOD reaches NONZERO LEGS; each verdict line
# is stamped here on its own line (no 900-char cut can drop END or a FAIL). KNOWN lines are stamped too.
# Outer wall cap 4h (critic 1 #7). IN_BATTERY=1 lets it run under the battery's lock; DEADLINE passes through (R5).
# Verify round 2 (RULED): which of the KNOWN non-passes still applies is read from the SEAT LIST by tP-modules-legs.sh.
# With the H1 host seat in the union (a ref p2-*test-list*, or a row noted 'host seat H1' -- the phrase since P, P11: a
# bare H1 / H2 names C2's go-cmp classes at P; p2-host-test-list at M's 24-row list)
# singleflight is RUN and 12 of 12 is expected; with no such row it is NOT RUN on this box at all ('NOT RUN (KNOWN: H1,
# child fan-out)': the fan-out was measured unbounded). The H2 seat is read the same way for sumdb/tlog (17 expected).
# Verify round 1: REALMOD_TEST_TIMEOUT is passed on only when COORD set it; its ONE default is tP-modules-legs.sh's.
# Verify round 2 (RULED): tracked changes are asserted 0 HERE, immediately before the module legs (the H7 builds above
# write build output only, but this is the line where a leftover costs MOD). Anything found is restored once more with
# the CR-only pass and stamped; a tree that is still not clean is a finding, and MOD's own refusal then says so too.
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "  NOTE: tracked changes immediately before MOD (saved as PREMOD-rewrites.patch, then restored): $(git status --porcelain | grep -v '^??' | head -n 6 | tr '\n' ' ')"; restore_paths PREMOD; }
pmt=$(git status --porcelain | grep -vc '^??')
stamp "  PRE-MOD tracked changes: $pmt (ASSERT 0: tP-modules-legs.sh refuses a tree with tracked changes)"
[ "$pmt" = 0 ] || finding "PRE-MOD: $pmt tracked change(s) remain before the module legs after two restores (CNR's and the behavioral legs' rewrites): MOD will refuse the tree"
# Verify round 2: the deadline travels resolved (DL_EPOCH); DEADLINE is passed for the stamp.
capped MOD modules-legs 4h env EXPECT_HEAD="$EXPECT_HEAD" BASE="$MASTERF" CONTROLS=1 IN_BATTERY=1 DEADLINE="$DEADLINE" DL_EPOCH="$DL_EPOCH" ${REALMOD_TEST_TIMEOUT:+REALMOD_TEST_TIMEOUT="$REALMOD_TEST_TIMEOUT"} bash "$SD/tP-modules-legs.sh"
modrc=$LEG_RC
while IFS= read -r l; do stamp "  MOD ${l:0:340}"; done < <(grep -aE ': (PASS|FAIL|KNOWN) -- |NOT MEASURED|NOT RUN|host seats read|sumdb/tlog red|REALMOD|^[0-9:]+ (ABORT|END |LEG [^ ]+ ABORT|DEADLINE STOP)' "$SD/mod-logs/SUMMARY.txt" 2>/dev/null | sed 's/^[0-9:]* *//' | tr -d '\r')
[ "$modrc" = 0 ] || stamp "  MOD rc=$modrc (4 = a failed verdict, 2/3 = ABORT, 9 = deadline) -- read mod-logs/SUMMARY.txt"
# Verify round 1: XS/XM are P2's fix batch END ACCEPTANCE, and tP-modules-legs.sh reads a missing module, a failed copy
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

# (TRAIN L's legs HOP and SPB are not carried. N: g-slog-roster-tc0 edits src/run-validated-sweep.ps1, but COMMENT lines
# only (read with git: the opt-out paragraph above the override predicate, 582de36d1b), and a comment in the
# src/_roster.ps1 it dot-sources; both are parsed under both editions by G1/G2, ST and NV, and every S row runs the
# sweep itself. tP-README.md section 6.)
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

# LEG S -- the banked rows this box sweeps (tP-i7-sweeps.txt minus T_ROWS, plus derived canaries / PRE-2 MISSING, plus
# X_ROWS). One row at a time, exact match. EVERY row is read at the execution config its ROSTER row carries (the sweep
# takes -test-config / -test-tiered from the row), so G's two roster commits ARE this leg's change:
#   P: as at O (no annotation at the base or at HEAD). X_ROWS adds sort (P12): the S run list is the 93 i7 rows minus the
#   two T rows, plus the derived canaries, encoding/json and sort.
#   O: TC0_ROWS and TIERED_ROWS derive EMPTY (no annotation at the base or at HEAD), so log/slog stays an i9 row and is
#   not read here; the case below stays for a list that names it.
#   N (item N3):
#   log/slog          (an i9 row, read here too: PRE-D's TC0_ROWS) at TC0 NOW, g-slog-roster-tc0 drops its
#                     release-tiered: EXPECT PASS 199 + 17, tiered=False; TestSetDefault, TestPanics and TestCallDepth by
#                     name (a TestSetDefault red is read as the first-launch symbolization class first: TRAIN M's note)
#   net/http          the row default since M (no longer a config row): PASS 1387, TestRegisterErr by name
#   internal/godebug  an i9 row, no longer read here (annotated at neither the base nor HEAD); its named case stays inert
#   M's notes:
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
  # from its fresh results record (the row wall is mostly MSBuild and publish). A reading; TRAIN P's baseline.
  hw=$(hp hostwall "$st0" "$WW\\src\\core\\$(echo "$pkg" | tr '/' '\\')\\go2cs_test_results.json" 2>&1 | tr -d '\r' | head -n 1)
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT|RERUN)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200) :: ${dl:-DEADLOCK unread} :: ${hw:-HOSTWALL unread}"
  case "$pkg" in net/rpc|net/rpc/jsonrpc) echo "$dl" | grep -q 'DEADLOCK total=0 .*record=fresh' || finding "S:$pkg deadlock reading is not 'total=0' on a FRESH record (no gate passes by absence): $dl" ;; esac
  case "$pkg" in
    net/rpc) named='TestSendDeadlock' ;;
    net/http) named='TestRegisterErr' ;;
    internal/godebug) named='TestCmdBisect' ;;
    log/slog) named='TestSetDefault,TestPanics,TestCallDepth' ;;
    sort) named='TestSortIntSlice,TestSortFloat64Slice,TestSortStringSlice' ;;   # P (P12): an X row, the release fix's package: read by name. sort's own suite sorts through sort.Sort(...) and never calls the three .Sort() methods (G), so pass/pass says the PACKAGE still validates
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

# LEG NR -- hazard H3 (TRAIN L's g-deadlock leg, kept at M for H3 and at N as regression): net/rpc four MORE times (5 in all with the S row).
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
#   TE: the rewritten sources' hunks by class (tP-helpers.py teattr). G's frame class (the NoInlining prefix on a
#      function that executes a go or sits in a constant-skip Caller window, its using line, the position-map re-encode)
#      is EXPECTED here: G regenerated no committed *_test.cs, and 158 of them hold a go launch. Every OTHER hunk is
#      compared with TRAIN O's own sweep patch from this box (O_REWRITES, O_RUN): the hunks O did not carry are LISTED. A
#      READING, not gated: what M's seats may move in test sources is not ruled (tP-README.md, open question).
#   HS (regression: L's r-m4, r-m6): no rewritten stdlib host carries a module-path argument; GoDefaultGodebug still
#      names exactly 3 files. csproj among the rewrites: EXPECT 0 on this box.
#   UF (c1-fixture-tracking, 'e2e 0 untracked'): a -tests run stages fixtures into src/core; the seat committed the ten
#      that were neither tracked nor ignored. EXPECT 0 untracked, not-ignored files under src/core after the S rows.
#      P (P6): the -tests-only .editorconfig class is KNOWN (ufarm, defined before the legs): 0 with
#      p2-test-warning-entries seated; without it src/core/math/bits/.editorconfig is stamped KNOWN on this box (weak's
#      is the i9's) and any OTHER path is the finding.
git diff -U0 > "$LOGDIR/S-rewrites-U0.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/S-tracked-changes.txt"
hp teattr --baseline "$(cygpath -w "$O_REWRITES")" "$(cygpath -w "$LOGDIR/S-rewrites-U0.patch")" > "$LOGDIR/te.log" 2>&1; terc=$?
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
ufarm 'UF after the sweeps' "$LOGDIR/S-untracked-core.txt"
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
#    battery (tL-seats-draft.txt:59, tP-seats-draft.txt:20). On G's branch ALONE P2 read TestTracebackSystem/panic RED,
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
  stamp "  T:$pkg:$suf $(hp hostwall "$tt0" "$WB\\src\\core\\$pb\\go2cs_test_results.json" "$(cygpath -w "$(dirname "$O_SUMS")/tests-$n-$suf.go2cs_test_results.json")" 2>&1 | tr -d '\r' | head -n 1)"
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
# whose runtime init reads the entry-assembly attribute). The TE baseline is the PREVIOUS train's own T-leg patch
# (O_REWRITES_T: TRAIN O's bat2 at P; it was M's run3 when this comment was written). No restore: the
# T legs' bank material stays in the tree for the bank step.
git diff -U0 > "$LOGDIR/T-rewrites-U0.patch"
# N (item N10): the same rewrites with full context, binary-safe, for the MS13 refresh after the battery (the S legs'
# twin is restore_paths' S-rewrites.patch); the T legs' material stays in the tree for the bank step, as at M.
git diff --binary > "$LOGDIR/T-rewrites.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/T-tracked-changes.txt"
hp teattr --baseline "$(cygpath -w "$O_REWRITES_T")" "$(cygpath -w "$LOGDIR/T-rewrites-U0.patch")" > "$LOGDIR/te-T.log" 2>&1; tetrc=$?
stamp "  TE-T rc=$tetrc (a READING) :: $(grep -aE '^TE (files|BASELINE|VERDICT)' "$LOGDIR/te-T.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-700)"
hsta=$(hs_m4); hstb=$(hs_m6)
stamp "  HS-T: module-path hosts (worktree)=$hsta (want 0) GoDefaultGodebug files=[${hstb% }] ($([ "$hstb" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'MOVED')) :: tracked rewrites=$(wc -l < "$LOGDIR/T-tracked-changes.txt") csproj among them=$(grep -c 'csproj' "$LOGDIR/T-tracked-changes.txt")"
[ "$tetrc" = 0 ] || finding "TE-T (T-leg rewrites) rc=$tetrc: the reader did not read (te-T.log)"
[ "$hsta" = 0 ] && [ "$hstb" = "$HS_WANT" ] || finding "HS-T after the T legs: module-path hosts=$hsta GoDefaultGodebug=[${hstb% }]"
ncst=$(grep -c 'csproj' "$LOGDIR/T-tracked-changes.txt"); [ "$ncst" = 0 ] || finding "HS-T: $ncst csproj among the T legs' tracked rewrites (TRAIN L read 0; read T-rewrites-U0.patch): $(grep 'csproj' "$LOGDIR/T-tracked-changes.txt" | head -n 4 | tr '\n' ' ')"
git status --porcelain -- src/core | grep '^??' > "$LOGDIR/T-untracked-core.txt"
ufarm 'UF-T after the T legs' "$LOGDIR/T-untracked-core.txt"   # P (P6): the S leg's untracked files are still in the tree (restore_paths touches tracked paths only), so the known class reads here again

# LEG TE-i9 (critic 0 #5) -- the i9 shard's patch through the same TE reader, scripted (tP-i9-te.sh). Runs here only
# when I9_PATCH names a file that has arrived; otherwise COORD runs `bash tP-i9-te.sh <patch> [<baseline>]` from this
# run folder when it lands (same reader: a READING; rc != 0 means the reader did not read).
if [ -n "$I9_PATCH" ] && [ -f "$I9_PATCH" ]; then
  bash "$SD/tP-i9-te.sh" "$I9_PATCH" $I9_BASELINE > "$LOGDIR/te-i9.log" 2>&1; LEG_RC=$?
  stamp "  TE-i9 rc=$LEG_RC (a READING) :: $(grep -aE '^TE (files|BASELINE|VERDICT)' "$LOGDIR/te-i9.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-700)"
  [ "$LEG_RC" = 0 ] || finding "TE-i9 rc=$LEG_RC: the reader did not read (te-i9.log)"
else
  stamp "  TE-i9: NOT RUN here (I9_PATCH=${I9_PATCH:-unset}) -- run 'bash tP-i9-te.sh <tracked-changes-U0.patch> [<the i9's TRAIN O patch: coord-scratch/tO/i9-patches/tO-tracked-changes-U0.patch>]' from this run folder when the i9's patch arrives"
fi

# LEG TL-WALL -- each ROW leg's wall vs the fastest passing reading of the same leg in the PREVIOUS train's battery of
# record (O_SUMS: TRAIN O's bat2 at P). M's note: against TRAIN M's battery of record (run3;
# the leg keeps its name). M's cost-bearing seats: the i9's copy-bound receivers and [GoRecv] on 25 testing.cs methods (every host), G's
# NoInlining on every go-executing function and its IL scan for the line a frame resolves to. TRAIN L's lesson stands
# (tL-seats-draft.txt:40): row wall is mostly MSBuild and publish, so a ratio > 1.25 is a prompt to read, never a gate.
# Verify round 1: the lesson's own comparison, the converted host's package elapsed, is the HOSTWALL reading stamped on
# every S row (no L baseline: L kept no per-row record) and on the three T legs beside L's kept record. The list below
# stays, labelled for what it is.
hp wallcmp "$(cygpath -w "$SUM")" $(for k in $O_SUMS; do [ -f "$k" ] && cygpath -w "$k"; done) > "$LOGDIR/wallcmp.log" 2>&1
while IFS= read -r l; do stamp "  $l (ROW wall: build and publish noise included; read the HOSTWALL lines for the host's own elapsed)"; done < <(grep -a '^WALLCMP' "$LOGDIR/wallcmp.log" | head -n 25)

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') (the T legs' material, plus whatever the after-sweep restore left: it MEASURED tracked-left=${S_LEFT:-unread}) tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD) SIc=[$SICV] E=[attr ${EATTR:-unread}] GT-NOT-FOUND=[${GT_NF% }] SyncMutexProfile=[${GT_SMP% }] 5-TRUNCATED=[${NMT:-no}]"
stamp "NONZERO LEGS (SIc, NVR and FXc1/FXc2 are EXPECTED non-zero controls; every other non-zero leg is a red to read, TBS-W, PUB and CC included; NGb51 is GATED; MOD is non-zero only on a FAIL verdict, its KNOWN lines are not reds): $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ')"
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
