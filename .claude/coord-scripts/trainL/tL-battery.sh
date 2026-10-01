#!/usr/bin/env bash
# TRAIN L battery on the i7 -- the union of record (COORD). DRAFT 2 2026-10-01 (battery-prep workflow; both critiques
# and COORD rulings R1-R8 applied, see CHANGES.md), adapted from tK-battery.sh + tK-reread.sh. NOT RUN by its author.
# Launch from a PER-RUN COPY (floor 4): copy this folder (tL-battery.sh, tL-emitcheck.sh, emitdrift.py, tL-helpers.py,
# tL-modules-legs.sh, tL-ng-parse.ps1, tL-i9-te.sh, tL-seats-draft.txt or the frozen tL-seats.txt, tL-i7-sweeps.txt,
# tL-i9-shard.txt) to a FRESH run folder (PRE refuses a folder that already holds a SUMMARY.txt) and run the copy:
#   EXPECT_HEAD=<fixup sha, 10 chars> MASTER=75648a022b [DEADLINE=17:30] [I9_PATCH=<i9 tracked-changes-U0.patch>] bash tL-battery.sh
# Fetch origin claude/i9-nugetgo-pack into the tL worktree BEFORE launch when the union carries the i9's nugetgo-pack
# 5.1 fix merge (R2): the battery reads that remote-tracking ref and never fetches.
# Logs land beside the copy in tL-logs/ (the module legs in mod-logs/).
# DEADLINE (R5): no leg STARTS after $DEADLINE (HH:MM, default 17:30): 'DEADLINE STOP before <leg>', purge, exit 9.
# ONE BATTERY AT A TIME (floors 1, 11): PRE takes /h/go2cs-tmp-coord/coord-scratch/tL/.battery.lock (mkdir) and the
# EXIT trap releases it; tL-modules-legs.sh refuses to run beside it unless the battery launched it (IN_BATTERY=1).
# UNION SHAPE (PRE refuses anything else): TRAIN K's landed master 75648a022b + the 11 signed seat merges (head
# 6960c8071f at assembly) [+ at most ONE merge whose second parent is on origin/claude/i9-nugetgo-pack: the i9's
# Windows PowerShell 5.1 parse fix for nugetgo-pack.ps1 (R2), seated immediately before the fixup] + ONE signed TRAIN L
# FIXUP (tL-fixup.sh). The fixup is OWED, not optional -- measured at 6960c8071f, read-only:
#   (F1) ForClauseSpill.csproj and CrossPackagePromotedValueMethod.csproj still carry <LangVersion>latest</LangVersion>
#        and lack the OSR opt-in (both cut at J's template); optin.py check reads CHECK FAIL naming exactly those two.
#        Without the fixup, CNR reads both CHANGED, PRE-1's S1 census fails, and leg E's csprojdrift flags them.
#   (F2) g-deadlock-checkdead added [GoTestMatchingConsoleOutput] to ChannelReceiveFromNil/ChannelSendToNil without
#        their OutputComparisonTests.cs rows (718 marked vs 716 listed): leg C's
#        TestOutputComparisonListMatchesConsoleOutputAttribute is RED until the rows land (fixup step 4; RULED R1: the
#        rows go in the fixup, attributed to the g-deadlock-checkdead seat).
#   No seat claims, and nothing read here shows, a -stdlib EMISSION footprint: all eleven claim 0 (D6, crosspkg and
#   M2-M6 measured two-seeded; c1-token, g-deadlock, c2, c1-fixture, nugetgo by construction). Leg E proves it; if E
#   reads union-attributable > 0, the bisect arms below name the seat and the fixup is re-designed, never hidden.
# The i9 sweeps the complement shard (tL-i9-shard.txt); P1/P2 run tL-linux-legs.sh. Legs run SEQUENTIALLY; the tL
# worktree is FROZEN while this runs (floor 4); every leg's rc is captured before any pipe (floor 7). One battery at a
# time on this box.
#
# LEG -> SEAT MAP (every leg's comment repeats its seats):
#   C      all converter seats (r-d6, i9-crosspkg, r-m2/m3/m4/m6, c2-binlog, c1-fixture, nugetgo's repoguard census)
#   CB     c2-binlog: its 3 binlog tests + K's 3 testHost GOOS tests + the 5 projitems tests BY NAME (-v, 0 SKIP)
#   FX     c1-fixture-currency (the Logf reading leg C hides); FXc its two discriminating controls (floor 13)
#   E      footprint 0 of r-d6, i9-crosspkg, r-m2..m6, c2, c1-token, g-deadlock; csproj template drift = the fixup
#   E-bisect (only on E > 0)  r-d6 alone, i9-crosspkg alone
#   G1/G2  c2-binlog (_roster.ps1's Copy-KeptPublishBinlog)        SY  r-d6 (R ran it)
#   SI/SIc r-d6, i9-crosspkg, g-deadlock (5 new go2cs.slnx projects)
#   ST51/ST7 c2-binlog (62 checks, was 57)                          NV*  c2-binlog (_roster.ps1 read by push-nuget)
#   NG     i9-nugetgo-pack (29 identity arms; the 5.1 parse is GATED (R2): red = a finding in NONZERO LEGS);
#          NGF-j0* the J0 consumer's PackageReference ID, default and nugetgo (R8 / critic 0 #12)
#   2b     c1-token (zh API), g-deadlock (RegisterPendingTimerProbe + 3 projects), r-m4 (TestRegistry ctor), r-m6
#          (GoDefaultGodebugAttribute), r-d6 + i9-crosspkg (projects)
#   TR     r-m4 (TestHost/TestRegistry), r-m6 (writeTestHost)
#   GT     c1-token, g-deadlock, r-m4, r-m6 (+ the SyncMutexProfile pair x3 Release), c1-token's OWED probe (by name)
#   4      r-d6, i9-crosspkg, g-deadlock goldens (first reading under K's converter) + the fixup's 2 csproj
#   5 + B: r-d6, i9-crosspkg, g-deadlock (5 projects + 8 neighbours), every golib seat at runtime
#   H7     c1-token, g-deadlock (golib + time_impl.cs), r-m4 (testing), r-m6 (golib + runtime + internal/godebug)
#   MOD    r-m2/m3/m4/m6 (CM by name with -v; MR2 [module cache, R6], MR3, MR4 + MR4v + MR4c, MR6 + MR6c, MR6p +
#          MR6e + MR6pc; JWT [module cache, R6] i9-crosspkg) -- tL-modules-legs.sh
#   HOP    c2-binlog (run-validated-sweep.ps1's live single-row path)   PB  c2-binlog (-test-publish-binlog live, polled)
#   SPB    c2-binlog (the sweep's -PublishBinlog switch on one passing row, cmp)
#   S      every seat that reaches a row: c1-token (reflect, fmt + canaries), g-deadlock (net/rpc, net/rpc/jsonrpc,
#          time), r-m4 (every host runs TryStageModule), r-m6 (every host's godebug init), c2 (Save-RowOutput -OutDir)
#   NR     g-deadlock: net/rpc x5 total, 0 'all goroutines are asleep' lines each
#   TE/HS  r-d6 + i9-crosspkg (test-source signature attribution), r-m4 + r-m6 (host censuses)
#   T      runtime/pprof + runtime: c1-token, g-deadlock (crash family by name), r-m6; TBS-W g-deadlock (one
#          windows TestTracebackSystem control iteration, run BEFORE T:runtime so the bank record stays in the tree);
#          TE-T/HS-T the TE/HS readings repeated over the T legs' rewrites; TK-WALL c1-token (wall time vs K)
#   TE-i9  r-d6 + i9-crosspkg over the i9's shard patch (only when I9_PATCH names it; else tL-i9-te.sh later)
W=/h/go2cs-tmp-coord/tL
WB='H:\go2cs-tmp-coord\tL'
EXPECT_HEAD=${EXPECT_HEAD:?the TRAIN L fixup commit, 10 chars}
MASTER=${MASTER:?TRAIN K landed master, 75648a022b}
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tL-seats.txt}; [ -f "$SEATS" ] || SEATS=$SD/tL-seats-draft.txt
I7LIST=$SD/tL-i7-sweeps.txt
I9LIST=${I9LIST:-$SD/tL-i9-shard.txt}
T_ROWS='runtime runtime/pprof'           # roster rows on the i7 list read by the T legs (K's tleg), never by the S loop
K_SUMS="/h/go2cs-tmp-coord/coord-scratch/tK/battery-run1/tK-logs/SUMMARY.txt /h/go2cs-tmp-coord/coord-scratch/tK/reread-run1/rr-logs/SUMMARY.txt"
PRECHECK_MODE=${PRECHECK_MODE:-abort}   # 'warn' runs on past a failed PRE assert (COORD's explicit call, stated in SUMMARY)
DEADLINE=${DEADLINE:-17:30}             # R5: no leg STARTS after this local clock time (the i7 is quiet for the release)
SWEEP_ROW_CAP=${SWEEP_ROW_CAP:-4h}      # outer wall cap per sweep row (critic 1 #7); the sweep's own per-row timeouts fire first
I9_PATCH=${I9_PATCH:-}                  # the i9's tracked-changes-U0.patch, when it has arrived (critic 0 #5)
LOCK=/h/go2cs-tmp-coord/coord-scratch/tL/.battery.lock
LOGDIR=$SD/tL-logs
SUM="$LOGDIR/SUMMARY.txt"
# Run-copy checks BEFORE anything is written (critic 1 #4 and #9): a folder that already holds a SUMMARY would mix two
# runs' lines into NONZERO LEGS, wallcmp and the MOD stamp; a launch from a worktree or a draft folder breaks floor 4;
# a missing companion file would surface hours in as rc 127.
[ -e "$SUM" ] && { echo "ABORT: $SUM exists -- use a fresh per-run copy"; exit 2; }
[ -e "$SD/mod-logs/SUMMARY.txt" ] && { echo "ABORT: $SD/mod-logs/SUMMARY.txt exists -- use a fresh per-run copy"; exit 2; }
case "$SD" in
  /h/go2cs-tmp-coord/tL|/h/go2cs-tmp-coord/tL/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft|*/wf/draft2|*/wf/draft*/*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
for f in tL-emitcheck.sh emitdrift.py tL-helpers.py tL-modules-legs.sh tL-ng-parse.ps1 tL-i9-te.sh tL-i7-sweeps.txt tL-i9-shard.txt; do
  [ -f "$SD/$f" ] || { echo "ABORT: $SD/$f missing from the run copy"; exit 2; }
done
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS)"; exit 2; }
[[ "$DEADLINE" =~ ^([01][0-9]|2[0-3]):[0-5][0-9]$ ]] || { echo "ABORT: DEADLINE '$DEADLINE' is not HH:MM"; exit 2; }
[ -x /usr/bin/timeout ] || { echo "ABORT: no /usr/bin/timeout (the outer wall caps need coreutils timeout, not timeout.exe)"; exit 2; }
# One battery at a time (floors 1 and 11): an atomic mkdir lock, released by the EXIT trap below.
mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- another TRAIN L battery (or a standalone tL-modules-legs.sh) holds it; rmdir it only after checking"; exit 2; }
POLLPID=''
FXC=''   # leg FXc's scratch (a GOROOT copy): the trap removes it on ANY exit (deadline stop, disk ABORT, timeout stop)
cleanup(){ local rc=$?; [ -n "$POLLPID" ] && kill "$POLLPID" 2>/dev/null; [ -n "$FXC" ] && rm -rf "$FXC" 2>/dev/null; if [ "${KEEP_LOCK:-0}" = 1 ]; then echo "$(date '+%H:%M:%S') lock KEPT: rmdir $LOCK after the listed PIDs are killed" | tee -a "$SUM"; else rmdir "$LOCK" 2>/dev/null; fi; exit $rc; }
trap cleanup EXIT
mkdir -p "$LOGDIR"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
FINDINGS=''
finding(){ stamp "  FINDING: $*"; FINDINGS="$FINDINGS[$1] "; }   # a gate that read wrong without a nonzero rc; listed at END
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
hp(){ python -B "$(cygpath -w "$SD/tL-helpers.py")" "$@"; }

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
case "$subj" in "fixup: TRAIN L"*) ;; *) stamp "ABORT: HEAD is not the TRAIN L fixup (subject: ${subj:0:120})"; exit 2 ;; esac
np=$(git rev-list --parents -n 1 HEAD | wc -w)
[ "$np" = 2 ] || { stamp "ABORT: the fixup HEAD has $((np - 1)) parents (a single-parent commit on the assembled union is ruled)"; exit 2; }
# (the GOROOT pin was checked on its value before the export, ENV above)
git merge-base --is-ancestor "$MASTER" HEAD || { stamp "ABORT: master $MASTER is not an ancestor"; exit 2; }
ns=0
while IFS='|' read -r b sha msg; do
  ns=$((ns + 1)); git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
[ "$ns" = 11 ] || { stamp "ABORT: the seat list has $ns seats (TRAIN L's union has 11)"; exit 2; }
# The two REPLACED originals ride inside their onK re-cuts (stated by the seat lines; asserted, never seated twice).
for s in 0050064e81 1e306358cb; do git merge-base --is-ancestor "$s" HEAD || { stamp "ABORT: replaced original $s is not contained"; exit 2; }; done
fsig=$(git log -1 --format=%G? HEAD)
case "$fsig" in G|U) ;; *) stamp "ABORT: the fixup HEAD signature reads '$fsig' (want G or U)"; exit 2 ;; esac
SEATFULL=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | cut -d'|' -f2 | while read -r s; do git rev-parse "$s^{commit}"; done)
# FOLLOW-UP SEAT MERGES (COORD 2026-10-01): a seat's own follow-up may ride as ONE extra merge each, after that seat's
# merge and before the fixup, when its second parent is on that seat's origin branch, descends from the seated sha and is
# not it. Two are ruled: i9-nugetgo-pack (R2, the Windows PowerShell 5.1 parse fix) and c1-token-ids (C1's
# field-of-element / field-of-field token fix, red 2c3589bd05 + fix ec0f0bdd6c).
FOLLOW='i9-nugetgo-pack:1f99ee7e9f c1-token-ids:667d052869'
mbad=0; mcontain=0; munsigned=0; nfix=0; FIXES=''
for c in $(git rev-list --first-parent "$MASTER..HEAD^"); do
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
      stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") is NOT a listed seat or a ruled follow-up (fetch origin/claude/<seat> before launch if it is one)"; mbad=$((mbad + 1))
    fi
  fi
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) munsigned=$((munsigned + 1)) ;; esac
done
[ "$mbad" = 0 ] || { stamp "ABORT: $mbad first-parent commit(s) on the union are not seat merges"; exit 2; }
fpc=$(git rev-list --first-parent --count "$MASTER..HEAD")
[ "$fpc" = $((ns + nfix + 1)) ] || { stamp "ABORT: union first-parent count $fpc != seats $ns + follow-ups $nfix + 1 (fixup) -- a stray or missing commit on the union"; exit 2; }
SEATS_EFF="$LOGDIR/seats-effective.txt"
{ cat "$SEATS"; for f in $FIXES; do echo "${f%%:*}-followup|${f##*:}|ruled follow-up merge of ${f%%:*} (origin/claude/${f%%:*})"; done; } > "$SEATS_EFF"
stamp "PRE union shape OK: first-parent=$fpc (= $ns seat merges + $nfix follow-up merge(s):$FIXES + fixup), ^2-contains-seat=$mcontain, fixup signed=$fsig, unsigned seat merges=$munsigned (stated, not gated)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before the battery"; exit 2; }
dv=$(dotnet --version 2>&1); dvrc=$?
case "$dvrc:$dv" in 0:10.0.*) ;; *) stamp "ABORT: dotnet under global.json rc=$dvrc: $(echo "$dv" | tr '\r\n' '  ' | cut -c1-200)"; exit 2 ;; esac
stamp "PRE head=$h (fixup: ${subj:0:90}) parent=$(git rev-parse --short=10 HEAD^) tree=$(git rev-parse --short=10 HEAD^{tree}) signed=$fsig master=$MASTER seats=$ns ($(basename "$SEATS")) $(go version) dotnet=$(echo "$dv" | tr -d '\r') free=$(freegb)G C:=$(cfree)G GO2CS_MODULE_ROOT=${GO2CS_MODULE_ROOT:-unset}"

# PRE-1: registration count asserts (projitems 411 = K 400 + 11; go2cs.slnx + the 4 BehavioralTests files = base + each
# seat's own inserts), K6/K8/K9 regression asserts, S1 census (the fixup's 2 csproj now read (1,0,1)), optin.py check
# (CHECK PASS, 1140 descendants), no conflict markers.
stamp "PRE GOROOT verified before export: spelling=toolchain's own, VERSION go1.24.13, no forward slash; DEADLINE=$DEADLINE SWEEP_ROW_CAP=$SWEEP_ROW_CAP lock=$LOCK GOMODCACHE(go env)=$(go env GOMODCACHE | tr -d '\r')"
hp precheck "$WW" "$MASTER" "$(cygpath -w "$SEATS_EFF")" > "$LOGDIR/precheck.log" 2>&1; prc=$?
stamp "PRE-1 precheck rc=$prc :: $(grep -a '^PRECHECK' "$LOGDIR/precheck.log") :: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 4 | tr '\n' ' ' | cut -c1-500)"
grep -aE '^ok +COUNT' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  PRE-1 ${l:0:220}"; done
# PRE-1b: g-deadlock's HAZARD, read from the tree (the rule leg C's TestOutputComparisonListMatchesConsoleOutputAttribute
# enforces): 0 attribute-not-listed after the fixup's step 4 (or G's re-cut). 718/716 at 6960c8071f.
hp outparity "$WW" > "$LOGDIR/outparity.log" 2>&1; oprc=$?
stamp "PRE-1b outparity rc=$oprc :: $(head -n 1 "$LOGDIR/outparity.log" | tr -d '\r')"
[ "$oprc" = 0 ] || prc=$((prc + 10))
# PRE-1c: the host censuses at HEAD (r-m4, r-m6): no committed stdlib host carries a module-path argument; GoDefaultGodebug
# is named by exactly 3 corpus files (measured so at 6960c8071f). Re-read after the sweeps (leg HS).
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

# PRE-2: sweep coverage (225 = tL-i7-sweeps.txt + tL-i9-shard.txt, disjoint). PRE-3: the reflect-bridge canaries,
# derived now (c1-token-ids is bridge-touching: Value.Pointer answers PointerOrderToken). T_ROWS leave the S run list.
hp coverage "$WW" "$(cygpath -w "$I7LIST")" "$(cygpath -w "$I9LIST")" > "$LOGDIR/coverage.log" 2>&1; cvrc=$?
hp canaries "$WW" > "$LOGDIR/canaries.log" 2>&1; carc=$?
RUNLIST="$LOGDIR/sweep-runlist.txt"
{ grep -vE '^\s*(#|$)' "$I7LIST"; grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2; grep -a '^CANARY ' "$LOGDIR/canaries.log" | cut -d' ' -f2; } \
  | LC_ALL=C sort -u | grep -vxF -f <(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2; echo '#none#') \
  | grep -vxF -f <(printf '%s\n' $T_ROWS) > "$RUNLIST"
stamp "PRE-2 coverage rc=$cvrc :: $(grep -a '^COVERAGE' "$LOGDIR/coverage.log" | tr '\n' ' ') :: missing->i7: $(grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') extra(dropped): $(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') dup(i7+i9): $(grep -ac '^DUP ' "$LOGDIR/coverage.log")"
stamp "PRE-3 canaries rc=$carc :: $(grep -a '^CANARY' "$LOGDIR/canaries.log" | tr '\n' ' ' | cut -c1-300) :: S run list $(wc -l < "$RUNLIST") rows (T rows read by the T legs: $T_ROWS)"
if [ "$cvrc" != 0 ] || [ "$carc" != 0 ]; then
  [ "$PRECHECK_MODE" = warn ] || { stamp "ABORT: PRE-2 coverage rc=$cvrc / PRE-3 canaries rc=$carc; PRECHECK_MODE=warn runs past it by COORD's explicit call"; exit 2; }
  stamp "PRE-2/PRE-3 FAILED (coverage rc=$cvrc canaries rc=$carc) and PRECHECK_MODE=warn: running on by COORD's explicit call"
fi
[ -s "$RUNLIST" ] || { stamp "ABORT: the sweep run list is EMPTY"; exit 2; }

past_deadline(){ local now dl; now=$(date +%H%M); dl=${DEADLINE/:/}; [ "$((10#$now))" -ge "$((10#$dl))" ]; }
deadline_check(){ # leg name -- R5: refuse to START a leg after $DEADLINE; purge first, then exit 9 cleanly
  # Verifier: a stop inside the S/NR loop or between the T legs finds a sweep's tracked rewrites (or a legitimate ' D') in
  # the tree. They are saved as DEADLINE-rewrites.patch and restored FIRST, and the purge runs in its no-abort mode, so
  # the stop is always exit 9 (a deletion the restore could not undo is stamped and listed as a FINDING, never exit 3).
  past_deadline || return 0
  stamp "DEADLINE STOP before $1 (DEADLINE=$DEADLINE, now $(date +%H:%M)); restoring tracked rewrites (patch kept), purging, then exit 9"
  if [ "${BANK_IN_TREE:-0}" = 1 ]; then
    stamp "  DEADLINE: bank material is in the tree (a bank T leg has run): NOT restored; tracked rewrites listed in DEADLINE-tracked.txt for COORD"
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
  local P="$LOGDIR/$1-rewrites.paths.z" rrc=0 left
  git diff --binary HEAD > "$LOGDIR/$1-rewrites.patch"
  git diff --name-only -z HEAD > "$P"
  if [ -s "$P" ]; then
    git restore --source=HEAD --staged --worktree --pathspec-from-file="$(cygpath -w "$P")" --pathspec-file-nul; rrc=$?
  fi
  left=$(git status --porcelain | grep -vc '^??')
  stamp "  RESTORE($1): $(tr -cd '\0' < "$P" | wc -c) paths; restore rc=$rrc; tracked now $left; deletions $(git status --porcelain | grep -c '^ D')"
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
  # Candidates are processes whose command line names the tL worktree. The pattern is assembled inside PowerShell from
  # pieces, so the querying shell's own command line cannot match it, and the querying process is excluded (floor 5).
  # COORD kills any survivor BY PID after reading the list.
  local f="$LOGDIR/timeout-orphans-$(echo "$1" | tr '/:' '._').txt"
  powershell -NoProfile -Command "\$p1 = 'go2cs-tmp-coord' + [char]92 + 'tL' + [char]92; \$p2 = 'go2cs-tmp-coord' + '/' + 'tL' + '/'; Get-CimInstance Win32_Process | Where-Object { \$_.ProcessId -ne \$PID -and \$_.CommandLine -and (\$_.CommandLine.Contains(\$p1) -or \$_.CommandLine.Contains(\$p2)) } | ForEach-Object { '{0} {1} {2}' -f \$_.ProcessId, \$_.Name, (\$_.CommandLine.Substring(0, [Math]::Min(220, \$_.CommandLine.Length))) }" > "$f" 2>&1
  stamp "TIMEOUT STOP: the outer wall cap fired on $1; $(grep -c . "$f") candidate process(es) listed in $(basename "$f") (NOT killed: COORD kills by PID, floor 5); no purge (a live orphan may hold files); lock KEPT; exit 5"
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

# ================================================================================================= LEGS
# LEG C -- the full converter suite, NO -short (r-m2/m3/m6 integration tests t.Skip under -short; CM in leg MOD reads
# them by name). Gates r-d6 + i9-crosspkg (behavioralPackageInfoAttributes guards, every for/selector emission test),
# r-m2 (moduleCacheLock/modulesLock), r-m3 (moduleTestsDriver x2), r-m4 (moduleAncestryEmission, testConversion), r-m6
# (godebugDefault), c2-binlog (publishBinlog + K's testHostGoTargetOS), c1-fixture (TestTrackedFixturesMatchPinnedGoRoot,
# TestFixtureCurrencyScannerFires), i9-nugetgo (repoguard fleet-identifier census over the new tool files), and the
# projitems integrity tests (411 = K 400 + 11; SIX seats register files). EXPECT ok throughout -- g-deadlock's attribute
# guard is green ONLY because the fixup (or G's re-cut) added the two D4 rows.
leg C conv-suite bash -c 'cd src/go2cs && go test -count=1 -timeout 40m ./...'
stamp "  C: $(grep -aE '^(ok|FAIL|---)' "$LOGDIR/conv-suite.log" | tr '\n' ' ' | cut -c1-500) :: projitems FAIL lines: $(grep -aE 'TestProjitems[A-Za-z]*' "$LOGDIR/conv-suite.log" | grep -ac FAIL) :: attribute-guard FAIL lines: $(grep -aE 'TestOutputComparisonListMatchesConsoleOutputAttribute|TestBehavioralPackageInfoCarriesNoDuplicateAttribute' "$LOGDIR/conv-suite.log" | grep -ac FAIL)"
# r-m3's integration test leaves its driver binary in TEMP (os.MkdirTemp('', 'go2cs-m3-driver'), never removed):
# count, then remove exactly those directories (the name is the test's own prefix; nothing else in TEMP is touched).
nd=$(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l)
find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' -exec rm -rf {} + 2>/dev/null
stamp "  C: r-m3 driver dirs left in TEMP by the suite: $nd (removed: $(( nd - $(find /h/go2cs-tmp-coord/coord-scratch/tmp -maxdepth 1 -type d -name 'go2cs-m3-driver*' | wc -l) )))"

# LEG CB -- c2-publish-binlog-onK BY NAME (critic 0 #2): leg C runs without -v and reads package ok/FAIL only, so a
# missing, renamed or skipped test reads green there. Each name below must print '--- PASS', and no SKIP/FAIL line.
CB_TESTS='TestPublishBinlogIsPassedOnlyWhenAsked TestAStaleBinlogNeverSurvivesIntoANewAttempt TestTheBinlogIsKeptOnlyForAFailedPublish TestTestHostBuildCarriesTheTargetGOOS TestTestHostBuildWithoutATargetLeavesTheCsprojDefault TestTestHostBuildKeepsItsExistingShape TestProjitemsRegistersEveryGoSource TestProjitemsHasNoDanglingEntries TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings TestProjitemsRegistrationClassifierFires TestProjitemsInsertionHintTakesTheNearestPredecessor'
leg CB conv-binlog-named bash -c "cd src/go2cs && go test -count=1 -timeout 20m -v -run '^(TestPublishBinlogIsPassedOnlyWhenAsked|TestAStaleBinlogNeverSurvivesIntoANewAttempt|TestTheBinlogIsKeptOnlyForAFailedPublish|TestTestHostBuildCarriesTheTargetGOOS|TestTestHostBuildWithoutATargetLeavesTheCsprojDefault|TestTestHostBuildKeepsItsExistingShape|TestProjitems.*)\$' ."
cbrc=$LEG_RC; cbmiss=''
for t in $CB_TESTS; do grep -aqE -- "--- PASS: $t \(" "$LOGDIR/conv-binlog-named.log" || cbmiss="$cbmiss $t"; done
cbsk=$(grep -acE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-binlog-named.log")
stamp "  CB (EXPECT 11/11 '--- PASS', 0 SKIP/FAIL): rc=$cbrc not-PASS=[${cbmiss# }] skip/fail-lines=$cbsk projitems-PASS=$(grep -acE -- '--- PASS: TestProjitems' "$LOGDIR/conv-binlog-named.log")"
[ "$cbrc" = 0 ] && [ -z "$cbmiss" ] && [ "$cbsk" = 0 ] || finding "CB not-PASS=[${cbmiss# }] skip/fail=$cbsk rc=$cbrc"

# LEG FX -- c1-fixture-currency's Logf reading (leg C hides Logf on pass). EXPECT 'tracked fixtures 1281 · current
# 1281 · stale 0' at go1.24.13 (C1's pre-read; read the number, do not carry it) and the scanner control PASS.
leg FX fixture-currency bash -c "cd src/go2cs && go test -count=1 -timeout 15m -v -run 'TestTrackedFixturesMatchPinnedGoRoot|TestFixtureCurrencyScannerFires' ./internal/repoguard/"
stamp "  FX: $(grep -aE -- '--- (PASS|FAIL|SKIP)|tracked fixtures|other cached' "$LOGDIR/fixture-currency.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-400)"
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
    bash -c 'cp -r "$SRC/src" "$DST/src" && cp "$SRC/VERSION" "$DST/VERSION" && printf "\n// TRAIN L FXc2 plant\n" >> "$DST/src/go/printer/testdata/alignment.golden"'
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

# LEG E -- whole-corpus emission check, M=K master vs the fixup HEAD, x3 targets, planted controls. Gates the
# footprint-0 claims of r-d6, i9-crosspkg, r-m2..m6, c2 and (by construction) c1-token/g-deadlock; csprojdrift reads the
# fixup's 2 csproj + g-deadlock's 3 as template-only, J0UuidConsumer.csproj as a seat edit (i9-nugetgo). HANDOWN: L's
# hand-owned corpus files must not be written by either arm (a reading).
leg E emitcheck env M="$MASTER" U="$EXPECT_HEAD" LOGDIR="$LOGDIR" TAG=emitcheck bash "$SD/tL-emitcheck.sh"
erc=$LEG_RC
stamp "  E: $(grep -aE '^(EMIT |PLANT|windows:|linux:|darwin:|UNION-ATTRIBUTABLE TOTAL|K9|HANDOWN|CSPROJ|EMITCHECK VERDICT|EVIDENCE|ABORT)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-1600)"
EATTR=$(grep -a '^UNION-ATTRIBUTABLE TOTAL:' "$LOGDIR/emitcheck.log" | awk '{print $3}')
# LEG E-bisect -- ONLY when E reads union-attributable > 0: the two one-axis arms the seats name (the converter seats
# with an emission predicate). Neither arm owns the fixup's csproj edits, so CSPROJ_GATE=0. A drift in neither arm
# belongs to an M-chain seat (r-m2..m6, whose -stdlib path the specs say never enters the driver): COORD bisects further.
if [ "${EATTR:-0}" != 0 ] && [ -n "$EATTR" ]; then
  stamp "  E: union-attributable=$EATTR > 0 -- running the bisect arms (the fixup is RE-DESIGNED around what they name, never absorbed)"
  leg E-bisect-d6 emitcheck-bisect-d6 env M=75648a022b U=566e28d894 LOGDIR="$LOGDIR" TAG=emitcheck-bisect-d6 CSPROJ_GATE=0 bash "$SD/tL-emitcheck.sh"
  stamp "  E-bisect-d6 (K master vs K master + D6): $(grep -aE '^(UNION-ATTRIBUTABLE TOTAL|EMITCHECK VERDICT)' "$LOGDIR/emitcheck-bisect-d6.log" | tr '\r\n' '  ')"
  leg E-bisect-xpkg emitcheck-bisect-xpkg env M=8f73f7e88f U=bb518139bf LOGDIR="$LOGDIR" TAG=emitcheck-bisect-xpkg CSPROJ_GATE=0 bash "$SD/tL-emitcheck.sh"
  stamp "  E-bisect-xpkg (the crosspkg merge alone): $(grep -aE '^(UNION-ATTRIBUTABLE TOTAL|EMITCHECK VERDICT)' "$LOGDIR/emitcheck-bisect-xpkg.log" | tr '\r\n' '  ')"
else
  stamp "  E-bisect: not run (E union-attributable=${EATTR:-unread}; rc=$erc)"
fi

# LEG G1/G2 -- the roster guard under pwsh 7 and Windows PowerShell 5.1. c2-binlog adds Copy-KeptPublishBinlog to
# _roster.ps1. EXPECT pass; the count is NOT K's 1977 (225 rows now): read it.
leg G1 roster env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1
stamp "  G1: $(tail -n 3 "$LOGDIR/roster.log" | tr '\r\n' '  ' | cut -c1-300)"
leg G2 roster51 env -u DOTNET_ROOT PATH="$GOPIN_PATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1
stamp "  G2: $(tail -n 1 "$LOGDIR/roster51.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SY -- symbol sync (r-d6 ran it in-seat; cheap).
leg SY symbol-sync env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-symbol-sync.ps1
stamp "  SY: $(tail -n 3 "$LOGDIR/symbol-sync.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SI + SIc -- check-solution-integrity: FIVE new go2cs.slnx projects (ForClauseSpill r-d6, CrossPackagePromotedValueMethod
# i9-crosspkg, ForeverWaitWorkersMainReturns / MainSelectForeverWorkerExits / MainSelectForeverAfterFunc g-deadlock),
# then K's POSITIVE CONTROL (floor 13): the injected runtime -> internal/syscall/windows edge prints W1's six cycles, exit 1.
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

# LEG ST -- the sweep's classifier self-test under both editions. c2-binlog adds 5 'binlog:' asserts: EXPECT
# '62 checks, 0 violations' (K read 57; never gate on 57).
leg ST51 sweep-selftest51 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST51 (EXPECT 62/0): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest51.log" | tail -n 1 | cut -c1-200) :: binlog asserts: $(grep -aic 'binlog' "$LOGDIR/sweep-selftest51.log")"
leg ST7 sweep-selftest7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST7 (EXPECT 62/0): $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest7.log" | tail -n 1 | cut -c1-200)"

# LEG NV -- push-nuget's release pre-flight (it dot-sources _roster.ps1, which c2-binlog edits; INFERRED harmless),
# read-only -VerifyOnly in both editions, plus K's safe refusal arm (ruling K(5): only that arm).
capped NV51 push-nuget-verify51 30m powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV51: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify51.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
capped NV7 push-nuget-verify7 30m env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV7: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify7.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
capped NVR push-nuget-suffix-refusal 30m powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly -VersionSuffix local.1
nvr=$LEG_RC; nvm=$(grep -ac 'VersionSuffix is a local-rehearsal pack and is refused' "$LOGDIR/push-nuget-suffix-refusal.log")
stamp "  NVR (EXPECT rc!=0 + the refusal by name): rc=$nvr named=$nvm :: $([ "$nvr" != 0 ] && [ "$nvm" -ge 1 ] && echo 'REFUSED BY NAME' || echo 'REFUSAL ARM FAILED')"

# LEG NG -- i9-nugetgo-pack's tooling, read-only. (a) the 29 table-driven identity arms under both editions: EXPECT
# 'ran 29, failed 0', rc=0. (b) ParseFile of the three nugetgo files under both editions: EXPECT 0 errors; MEASURED RED
# TODAY under 5.1 (nugetgo-pack.ps1: 10 errors from line 116, a BOM-less em dash). RULED R2: nugetgo-pack.ps1 MUST parse
# under Windows PowerShell 5.1 (release-path scripts run there); the i9 fixes it on its branch and that fix may be merged
# before the fixup (PRE accepts the 12th merge). NGb51 is GATED: a red is a finding in NONZERO LEGS, never absorbed.
# NGF also carries K's j0-consume.ps1, which the seat edited (critic 0 #12), parsed under both editions.
NGF='src/tools/nugetgo/nugetgo-pack.ps1 src/tools/nugetgo/NugetgoIdentity.psm1 src/tools/nugetgo/Test-NugetgoIdentity.ps1 src/tools/j0-uuid-rehearsal/j0-consume.ps1'
leg NGa51 nugetgo-identity51 powershell -NoProfile -ExecutionPolicy Bypass -File src/tools/nugetgo/Test-NugetgoIdentity.ps1
stamp "  NGa51 (EXPECT ran 29, failed 0): $(grep -aiE 'ran [0-9]+' "$LOGDIR/nugetgo-identity51.log" | tail -n 1 | cut -c1-160)"
leg NGa7 nugetgo-identity7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/tools/nugetgo/Test-NugetgoIdentity.ps1
stamp "  NGa7 (EXPECT ran 29, failed 0): $(grep -aiE 'ran [0-9]+' "$LOGDIR/nugetgo-identity7.log" | tail -n 1 | cut -c1-160)"
leg NGb51 nugetgo-parse51 powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w "$SD/tL-ng-parse.ps1")" $NGF
stamp "  NGb51 (EXPECT 0 errors; GATED, R2; union carries the i9 fix merge: $(case "$FIXES" in *i9-nugetgo-pack:*) echo "yes ${FIXES#*i9-nugetgo-pack:}" | cut -c1-14 ;; *) echo no ;; esac)): $(grep -aE '^(PARSE|NGPARSE)' "$LOGDIR/nugetgo-parse51.log" | tr '\r\n' '  ' | cut -c1-400)"
leg NGb7 nugetgo-parse7 env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File "$(cygpath -w "$SD/tL-ng-parse.ps1")" $NGF
stamp "  NGb7 (EXPECT 0 errors): $(grep -aE '^NGPARSE' "$LOGDIR/nugetgo-parse7.log" | tr -d '\r')"
# LEG NGF-j0 (R8 / critic 0 #12) -- the J0 consumer the seat edited, evaluated (no restore, no build): with no -p its
# PackageReference must stay J0's 'go.github.com.google.uuid'; with -p:J0UuidPackageId the nugetgo ID.
J0C=src/tools/j0-uuid-rehearsal/consumer/J0UuidConsumer.csproj
leg NGF-j0default j0-consumer-default dotnet msbuild "$J0C" -getItem:PackageReference
j0a=$LEG_RC; j0an=$(grep -acE '"Identity": *"go\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-default.log")
leg NGF-j0nugetgo j0-consumer-nugetgo dotnet msbuild "$J0C" -getItem:PackageReference -p:J0UuidPackageId=nugetgo.github.com.google.uuid
j0b=$LEG_RC; j0bn=$(grep -acE '"Identity": *"nugetgo\.github\.com\.google\.uuid"' "$LOGDIR/j0-consumer-nugetgo.log")
stamp "  NGF-j0 (EXPECT default=go.github.com.google.uuid, -p=nugetgo.github.com.google.uuid): default rc=$j0a matched=$j0an :: nugetgo rc=$j0b matched=$j0bn"
[ "$j0a" = 0 ] && [ "$j0an" = 1 ] && [ "$j0b" = 0 ] && [ "$j0bn" = 1 ] || finding "NGF-j0 consumer PackageReference (default rc=$j0a n=$j0an; nugetgo rc=$j0b n=$j0bn)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after the read-only legs: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"

# LEG 2b -- go2cs.slnx (golib API: c1-token's zh base field + IAllocationIdentity + internal virtual AllocationId;
# g-deadlock's Goroutine.RegisterPendingTimerProbe; r-m4's TestRegistry ctor; r-m6's GoDefaultGodebugAttribute; the five
# new behavioral projects).
leg 2b go2cs-slnx dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental
stamp "  2b: errors=$(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/go2cs-slnx.log") gen-load=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log") warnings-line=$(grep -aE 'Warning\(s\)|Error\(s\)' "$LOGDIR/go2cs-slnx.log" | tr '\n' ' ' | cut -c1-120)"

# LEG TR -- BehavioralTests filtered to TestingRuntimeTests (the testing host moved: r-m4 TestHost.cs -> TryStageModule,
# TestRegistry's optional modulePath; r-m6's writeTestHost argument). Critic 1 #13: build and test are SEPARATE legs, so
# a build failure reads as BUILD FAILED, never as an empty Passed/Failed stamp.
leg TR-build testing-runtime-build dotnet build src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug -p:UseSharedCompilation=false -p:go2csPath=H:/go2cs-tmp-coord/tL/src/
if [ "$LEG_RC" = 0 ]; then
  leg TR testing-runtime dotnet test src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug --no-build -p:go2csPath=H:/go2cs-tmp-coord/tL/src/ --filter FullyQualifiedName~TestingRuntimeTests
  stamp "  TR: $(grep -aE '(Passed|Failed)!' "$LOGDIR/testing-runtime.log" | tail -n 1 | cut -c1-200)"
else
  stamp "  TR: BUILD FAILED (testing-runtime-build.log: $(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/testing-runtime-build.log") errors) -- the test leg did not run (NOT MEASURED)"
fi

# LEG GT -- GolibTests, Debug x1 and Release x3 FULL (K's re-read pattern), each with a TRX read BY NAME (a class with 0
# results is NOT FOUND, never green). Classes, by seat:
#   c1-token-ids    PointerTokenUniquenessTests (NEW) AliasOverlapTests ReflectHashTokenBandTests RuntimePinnerTests
#                   (CHANGED) FieldPointerEqualityTests (K's r-field-ptr-equality, first met here) + the token-path
#                   neighbours; the alloc asserts FieldRefTokenTests.AFreshViews... and NoUncountedBackingAllocationsTests
#   g-deadlock      ForeverWaitDeadlockDecisionTests (NEW, 6) + the Goroutine.cs / s_profileGate neighbours
#   r-m4            ModuleAncestryTests (NEW, 5) + FixtureLinkStagingTests (the i7's master set: 0 failed in K's re-read)
#   r-m6            ParseDebugVarsAtStartTests InternalCpuGodebugTests NilPanicHookTests JunctionGodebugToolchainChildTests;
#                   the SyncMutexProfileTests pair + SkipCountedWalkerFrameTests in EVERY Release run (R's 2/7 was on a
#                   base without bbbb3ff022; these three runs are that arm)
GT_CLASSES="PointerTokenUniquenessTests AliasOverlapTests ReflectHashTokenBandTests RuntimePinnerTests FieldPointerEqualityTests FieldRefTokenTests PointerTokenLayoutTruthTests AliasOverlapRaceTests PointerTokenConversionTests ManagedPointerTokenMintTests ManagedPointerTokenRegistryGrowthTests ManagedPointerTokenRunningCountTests TokenArithmeticRefusalTests TokenValueTagRefusalTests TokenDoorWiredTests OrderTokenOffsetZeroRefusalTests ZeroBaseIdentityTests ZeroSizeFieldIdentityTests NamedPointerTokenCarrierTests PointerMintRetentionTests PinnedBoxStalenessWitnessTests Q44RegistryCensusControlTests SyntheticPCRegistryTests RuntimeCallerPCSpanTests NoUncountedBackingAllocationsTests ForeverWaitDeadlockDecisionTests GoroutineProfileInstantTests GoroutineProfileTests GoroutineParkAccountingTests RuntimeParkTransitionTests GoroutineReadyTests MainGoroutineIdentityTests GoroutineFatalHostTests BubbledChannelTests ChannelWakeupStrainTests TracebackDecorationTests CreatedByPositionTests TestGoroutineCreatorTests SyncMutexProfileTests SkipCountedWalkerFrameTests ModuleAncestryTests FixtureLinkStagingTests ParseDebugVarsAtStartTests InternalCpuGodebugTests NilPanicHookTests JunctionGodebugToolchainChildTests"
GT_TESTS="PointerTokenUniquenessTests.APointerRoundTripReturnsItsOwnBoxAmongCollidingIdentityHashes AliasOverlapTests.TokenCollisionDoesNotReachThePredicate ReflectHashTokenBandTests.PointerTokensKeepBit62ClearSoTheHashBandIsDisjoint RuntimePinnerTests.TheBoxGainsNoInstanceState FieldRefTokenTests.AFreshViewsFirstTokenAllocatesNothingOnceItsAccessorIsKnown FieldRefTokenTests.OneAccessorOverTwoSourceTypesKeepsEachSourcesDisplacement AliasOverlapRaceTests.VendoredAnyOverlapDoesNotConfuseArraysWithCollidingIdentityHashes GoroutineProfileInstantTests.TheSnapshotIsOneInstantAcrossTheSetAndTheLabels ForeverWaitDeadlockDecisionTests.ResidualAnOrdinaryChannelOrSyncWaitBlocksInsteadOfReporting SyncMutexProfileTests.EachEventsTopFrameIsGosLockOrUnlock SyncMutexProfileTests.TheEventsSecondFrameIsTheGoCallerThroughAPointerOrALocker SkipCountedWalkerFrameTests.EveryHandOwnedSkipCountedWalkerKeepsItsFrame ModuleAncestryTests.AModuleThatContainsTheSandboxIsCopiedWithoutRecursingIntoIt JunctionGodebugToolchainChildTests.AToolchainChildThatSetsItsOwnGodebugKeepsTheJunctionSetting"
# c1-token's OWED probe (field-of-element token uniqueness; R3): C1 is writing it in PointerTokenUniquenessTests. It is
# read BY METHOD NAME from each TRX: any method of that class whose name contains FieldOfElement or ElementField is
# FOUND (names + outcomes printed); none is NOT-FOUND. Stated, NOT gated: the bank waits on it, the battery does not.
GT_PROBE_CLASS=PointerTokenUniquenessTests; GT_PROBE_RX='FieldOfElement|ElementField'
GT_NF=''; GT_SMP=''; GT_PROBE=''
for run in Debug Release-1 Release-2 Release-3; do
  cfg=${run%%-*}
  # critic 1 #13: build and test are SEPARATE legs (a build failure is BUILD FAILED, never an empty stamp)
  leg GT-build-$run golibtests-build-$run dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false
  if [ "$LEG_RC" != 0 ]; then
    GT_NF="$GT_NF$run:[BUILD FAILED] "; GT_PROBE="$GT_PROBE$run:[BUILD FAILED] "
    stamp "  GT-$run: BUILD FAILED ($(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/golibtests-build-$run.log") errors, golibtests-build-$run.log) -- the test leg did not run (NOT MEASURED)"
    continue
  fi
  leg GT-$run golibtests-$run dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build --logger "trx;LogFileName=golibtests-$run.trx" --results-directory "$LOGW"
  stamp "  GT-$run: $(grep -aE '(Passed|Failed)!' "$LOGDIR/golibtests-$run.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/golibtests-$run.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
  if [ -f "$LOGDIR/golibtests-$run.trx" ]; then
    hp trx "$LOGW\\golibtests-$run.trx" $GT_CLASSES $GT_TESTS > "$LOGDIR/golibtests-$run.classes.txt" 2>&1
    nf=$(grep -a 'NOT FOUND' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX \(class\|test\) //; s/:.*//' | tr '\n' ' ')
    [ -n "$nf" ] && GT_NF="$GT_NF$run:[${nf% }] "
    smp=$(grep -a '^TRX class SyncMutexProfileTests:' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX class SyncMutexProfileTests: //')
    GT_SMP="$GT_SMP$run:[$smp] "
    hp trxmethods "$LOGW\\golibtests-$run.trx" "$GT_PROBE_CLASS" "$GT_PROBE_RX" > "$LOGDIR/golibtests-$run.probe.txt" 2>&1
    pr=$(grep -a '^TRXM ' "$LOGDIR/golibtests-$run.probe.txt" | head -n 1 | tr -d '\r' | cut -c1-400)
    GT_PROBE="$GT_PROBE$run:[$(echo "$pr" | grep -q ' FOUND ' && echo FOUND || echo NOT-FOUND)] "
    stamp "  GT-$run classes: NOT-FOUND=[${nf% }] $(grep -a 'Failed=' "$LOGDIR/golibtests-$run.classes.txt" | sed 's/^TRX class //' | tr '\n' ' ' | cut -c1-300) :: SyncMutexProfileTests=[$smp] (per-class table: golibtests-$run.classes.txt)"
    stamp "  GT-$run C1 PROBE (R3, stated not gated): ${pr:-TRXM unreadable (golibtests-$run.probe.txt)}"
  else
    GT_NF="$GT_NF$run:[NO TRX] "; GT_PROBE="$GT_PROBE$run:[NO TRX] "
    stamp "  GT-$run classes: NO TRX written -- the by-name reading is NOT MEASURED"
  fi
done
purge after-GT

# LEG 4 -- CNR (byte-identical behavioral C# AND csproj under the union converter). The FIRST reading of r-d6's
# ForClauseSpill, i9-crosspkg's CrossPackagePromotedValueMethod and g-deadlock's 3 goldens under K's converter, and of
# the fixup's 2 csproj against K's template. EXPECT NO REGRESSION over N = 778 measured (784 behavioral .go dirs at
# 6960c8071f - 6 platform skips: INFERRED, read the number). A changed .cs is a K x seat INTERACTION: re-baseline in a
# new fixup and read that guard isolated -- never waived.
# Critic 0 #18: N is read from the verdict line (a mismatch with 778 is a FINDING to read: 778 is INFERRED), and none of
# the 5 new L dirs may sit in the platform-exclusive skip list (a dir dropped there still reads NO REGRESSION).
CNR_EXPECT_N=${CNR_EXPECT_N:-778}
L5DIRS='ForClauseSpill CrossPackagePromotedValueMethod ForeverWaitWorkersMainReturns MainSelectForeverWorkerExits MainSelectForeverAfterFunc'
leg 4 cnr powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1
stamp "  4: $(grep -aiE 'NO REGRESSION|REGRESSION|byte-identical|CHANGED|verdict' "$LOGDIR/cnr.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
cnrn=$(grep -aoE 'NO REGRESSION: generated C# and \.csproj are byte-identical across all [0-9]+ behavioral packages' "$LOGDIR/cnr.log" | grep -oE '[0-9]+' | tail -n 1)
cnrskip=$(for d in $L5DIRS; do grep -aqE "^\s+$d \[" "$LOGDIR/cnr.log" && printf '%s ' "$d"; done)
stamp "  4 asserts: N=${cnrn:-unread} (EXPECT $CNR_EXPECT_N) skip-list holds L dirs: [${cnrskip% }] (EXPECT none) skipped(platform-exclusive) header: $(grep -aoE 'SKIPPED \(platform-exclusive, [0-9]+\)' "$LOGDIR/cnr.log" | head -n 1)"
[ "${cnrn:-x}" = "$CNR_EXPECT_N" ] || finding "CNR N=${cnrn:-unread} != $CNR_EXPECT_N (INFERRED expectation: read the skip list before trusting either)"
[ -z "$cnrskip" ] || finding "CNR platform-exclusive skip list holds new L dir(s): ${cnrskip% }"
purge after-CNR

# LEG 5 -- the full behavioral suite (every golib seat reaches it at runtime: c1-token's ids, g-deadlock's checkdead,
# r-m6's startup attribute read -- no attribute => ""), then EACH L guard ISOLATED by name, then g-deadlock's neighbours,
# then every NOT MEASURED name ISOLATED. g-deadlock's RESIDUAL is now a HANG (main on a real channel/sync, others
# nil-blocked): read the timeout list for that shape.
leg 5 behavioral powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --build-timeout 10800 --build-one-timeout 900
stamp "  5: $(grep -aiE 'Transpile|Compile|Target|Output|compared|pass|fail|skip' "$LOGDIR/behavioral.log" | tail -n 8 | tr '\r\n' '  ' | cut -c1-600)"
NM=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:timeout[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMBE=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:best-effort[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMT=$(grep -aoE '^\s+\.\.\. and [0-9]+ more\.' "$LOGDIR/behavioral.log" | tail -n 1 | tr -s ' ')
[ -n "$NMT" ] && stamp "  5 NOT-MEASURED LIST TRUNCATED ($NMT) -- the isolated set below is NOT complete; COORD re-runs"
[ -n "$NMBE" ] && stamp "  5 BEST-EFFORT (not isolated: a bigger budget cannot help): [${NMBE% }]"
# r-d6: ForClauseSpill. i9-crosspkg: CrossPackagePromotedValueMethod. g-deadlock: the 3 new + the 2 now Output-compared
# (EXPECT stderr 'fatal error: all goroutines are asleep - deadlock!', exit 2 on both sides). --filter is a SUBSTRING
# match; each name below matches exactly one project at 6960c8071f (checked).
LGUARDS="ForClauseSpill CrossPackagePromotedValueMethod ForeverWaitWorkersMainReturns MainSelectForeverWorkerExits MainSelectForeverAfterFunc ChannelReceiveFromNil ChannelSendToNil"
NEIGH="NilChannelInSelect NilChannelSelectDefault CloseWakesBlocked SelectSendDefault ChanDirectionChain ChannelCapLen StdoutCloseEofBarrier PointerEmbedValueChainPromotion"
stamp "  5 named: NOT-MEASURED=[${NM}] :: L-guard mentions on non-pass lines: $(for g in $LGUARDS $NEIGH; do n=$(grep -aE "\b$g\b" "$LOGDIR/behavioral.log" | grep -aciE 'fail|timeout|mismatch|differ'); [ "$n" != 0 ] && printf '%s=%s ' "$g" "$n"; done)"
for p in $LGUARDS $NEIGH $NM; do
  leg "B:$p" "behav-$p" powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$p"
  stamp "  B:$p: $(grep -aE '^\s+(Transpile|Compile|Target|Output)\s+pass' "$LOGDIR/behav-$p.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300) :: $(grep -aE '^(PASS|FAIL)|compared' "$LOGDIR/behav-$p.log" | tail -n 1 | cut -c1-120)"
done
purge after-behavioral

# LEG H7 -- compile parity per flavour: c1-token + g-deadlock (golib, time_impl.cs), r-m4 (src/core/testing), r-m6
# (golib, runtime/goenvs_impl.cs, internal/godebug).
export MSYS_NO_PATHCONV=1
for FL in windows linux darwin; do
  leg H7-$FL h7-$FL dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=$FL --no-incremental -m -p:UseSharedCompilation=false
  L="$LOGDIR/h7-$FL.log"
  stamp "  H7-$FL: CS=$(grep -aoE 'error CS[0-9]+' "$L" | wc -l) MSB/NETSDK=$(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$L" | wc -l) :: $(grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$L" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u | head -n 5 | tr '\n' ' ')"
  purge after-H7-$FL
done
unset MSYS_NO_PATHCONV

# LEG MOD -- r-m2/m3/m4/m6's manual readings as runnable legs (tL-modules-legs.sh, same env/leg/purge semantics):
# CM (14 module tests + 5 subtests by name with -v: '--- PASS' each, zero SKIP -- leg C cannot see a skipped one),
# MR3 (jwt-shaped -tests -recurse: 'Converting 5 packages', Validated 'example.test/jwtlike 1;example.test/jwtlike/request 1;',
# stamp == go list -test DefaultGODEBUG), MR4 (+ MR4c control at c5e934c464: rc!=0 + the 3 tests go=pass cs=fail by name, ENOENT count information only), MR6 (TestRandSeedIsHonored
# is the ONLY end-to-end gate of internal/godebug's default layer), MR6p (+ MR6pc scope control). It needs a CLEAN tree
# (it refuses otherwise): it runs here, after the H7 purges and before HOP/PB/S write anything. Phase B builds restore
# from H:\nuget\packages (warm) -- a cold cache with egress down times out (R's first reading did).
# DRAFT 2 adds: MR2 + JWT (only when their modules are in the H: module cache, R6; GOPROXY=off, never a download),
# MR4v (vendor/ copied), MR6e (GODEBUG env over the stamp), MR6c (e9009f2945's converter from git archive into a
# coord-scratch/tL build dir, R7). The script exits 4 on any failed verdict (critic 0 #1 / critic 1 #1), so MOD reaches
# NONZERO LEGS; each verdict line is stamped here on its own line (no 900-char cut can drop END or a FAIL).
# Outer wall cap 4h (critic 1 #7). IN_BATTERY=1 lets it run under the battery's lock; DEADLINE passes through (R5).
capped MOD modules-legs 4h env EXPECT_HEAD="$EXPECT_HEAD" CONTROLS=1 IN_BATTERY=1 DEADLINE="$DEADLINE" bash "$SD/tL-modules-legs.sh"
modrc=$LEG_RC
while IFS= read -r l; do stamp "  MOD ${l:0:300}"; done < <(grep -aE ': (PASS|FAIL) -- |NOT MEASURED|^[0-9:]+ (ABORT|END |LEG [^ ]+ ABORT|DEADLINE STOP)' "$SD/mod-logs/SUMMARY.txt" 2>/dev/null | sed 's/^[0-9:]* *//' | tr -d '\r')
[ "$modrc" = 0 ] || stamp "  MOD rc=$modrc (4 = a failed verdict, 2/3 = ABORT, 9 = deadline) -- read mod-logs/SUMMARY.txt"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after MOD: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"
purge after-MOD

# LEG HOP -- c2-binlog: run-validated-sweep.ps1's live single-row path (-Hop over cmp, K's control row). EXPECT a HOP
# verdict for cmp, rc=0, the population read; the -Hop and binlog/Save-RowOutput hunks do not interact on a healthy row.
capped HOP hop-arm "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Hop -Filter cmp -Exact
stamp "  HOP: $(grep -aE 'hop population:|HOP re-derivation|^\s+HOP|HOPNONE|^(sweep|hop):|RERUN|refus' "$LOGDIR/hop-arm.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-500)"
restore_paths HOP

# LEG SPB -- c2-binlog's sweep switch (critic 0 #4): one passing row (cmp, K's control row; an i9 row, read here as an
# extra) through run-validated-sweep.ps1 -PublishBinlog. HOP keeps K's exact shape, so the switch rides its own leg.
# EXPECT PASS rc=0, the binlog SEEN during the run (polled) and ABSENT afterwards (a passing publish deletes it), and no
# publish.binlog copied into the sweep's evidence root for cmp (Copy-KeptPublishBinlog copies a KEPT log only).
# Verifier: a binlog left over from an earlier run would read SEEN before this leg's publish starts, so record whether
# one already exists (pre=PRESENT is a FINDING) and delete it before the poll starts. The evidence root
# scratchpad/sweep-oracle-flake/<stamp>/ is gitignored and persists, so only the stamp folders that did not exist
# before this leg are counted (a name-set difference, not -newer: an old folder's mtime can move).
spbpre=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent); rm -f src/core/cmp/bin/tests/publish.binlog
ls -1 scratchpad/sweep-oracle-flake 2>/dev/null | LC_ALL=C sort > "$LOGDIR/spb-evroot-before.txt"
poll_start "$W/src/core/cmp/bin/tests/publish.binlog" "$LOGDIR/spb-binlog.seen"
capped SPB sweep-cmp-publishbinlog "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter cmp -Exact -PublishBinlog
spbrc=$LEG_RC; poll_stop
spbseen=$([ -e "$LOGDIR/spb-binlog.seen" ] && echo SEEN || echo never-seen)
spbleft=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent)
ls -1 scratchpad/sweep-oracle-flake 2>/dev/null | LC_ALL=C sort > "$LOGDIR/spb-evroot-after.txt"
spbnew=$(LC_ALL=C comm -13 "$LOGDIR/spb-evroot-before.txt" "$LOGDIR/spb-evroot-after.txt")
spbev=0; for d in $spbnew; do spbev=$((spbev + $(ls scratchpad/sweep-oracle-flake/"$d"/cmp/*/publish.binlog 2>/dev/null | wc -l))); done
stamp "  SPB (EXPECT PASS rc=0, pre=absent, binlog SEEN then absent, 0 evidence copies): rc=$spbrc :: $(grep -aE '^\s*(PASS|FAIL|COUNT|DRIFT|RERUN)' "$LOGDIR/sweep-cmp-publishbinlog.log" | tail -n 1 | tr -s ' ' | cut -c1-160) :: pre=$spbpre during=$spbseen after=$spbleft evidence-copies=$spbev (new evidence folders: $(echo $spbnew | wc -w) $(echo $spbnew | cut -c1-120))"
[ "$spbpre" = absent ] || finding "SPB pre=PRESENT: a publish.binlog already existed before SPB (deleted before the poll)"
[ "$spbrc" = 0 ] && [ "$spbseen" = SEEN ] && [ "$spbleft" = absent ] && [ "$spbev" = 0 ] || finding "SPB -PublishBinlog (rc=$spbrc pre=$spbpre during=$spbseen after=$spbleft evidence-copies=$spbev)"
restore_paths SPB

# LEG PB -- c2-binlog's live arm: -test-publish-binlog over cmp (banked 4). EXPECT rc=0, the same Validated line as the
# sweep, and src/core/cmp/bin/tests/publish.binlog ABSENT afterwards (a passing publish deletes it). Read BEFORE any
# purge (the log lives under bin/). The keep-on-failure arm is unit-tested only (TestTheBinlogIsKeptOnlyForAFailedPublish).
# Critic 0 #3: 'absent afterwards' reads the same whether -bl was passed and the log deleted or never passed (the dotnet
# argv is not echoed), so a background poll records whether the binlog EXISTED during the leg: EXPECT SEEN.
export MSYS_NO_PATHCONV=1
convbuild PB
# Verifier: if SPB went red its binlog is KEPT, and the poll would read that leftover as SEEN before PB's own publish
# starts. Record it (pre=PRESENT is a FINDING) and delete it before the poll starts.
pbpre=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent); rm -f src/core/cmp/bin/tests/publish.binlog
poll_start "$W/src/core/cmp/bin/tests/publish.binlog" "$LOGDIR/pb-binlog.seen"
leg PB publish-binlog "$EXE" -tests -test-action all -test-timeout 10m -test-config Release -test-publish-binlog -go2cspath "$WB\\src" "$GOROOT\\src\\cmp" "$WB\\src\\core\\cmp"
pbrc=$LEG_RC; poll_stop
pbseen=$([ -e "$LOGDIR/pb-binlog.seen" ] && echo SEEN || echo never-seen)
pbleft=$([ -e src/core/cmp/bin/tests/publish.binlog ] && echo PRESENT || echo absent)
stamp "  PB (EXPECT rc=0, Validated 4, pre=absent, binlog SEEN during, absent after): rc=$pbrc :: $(grep -aE 'Validated [0-9]+ tests' "$LOGDIR/publish-binlog.log" | tail -n 1 | cut -c1-160) :: pre=$pbpre during=$pbseen after=$pbleft :: 'publish.binlog' in log: $(grep -ac 'publish.binlog' "$LOGDIR/publish-binlog.log") (INFERRED 0: the argv is not echoed)"
[ "$pbpre" = absent ] || finding "PB pre=PRESENT: a publish.binlog already existed before PB (deleted before the poll)"
[ "$pbrc" = 0 ] && [ "$pbseen" = SEEN ] && [ "$pbleft" = absent ] || finding "PB -test-publish-binlog (rc=$pbrc pre=$pbpre during=$pbseen after=$pbleft)"
unset MSYS_NO_PATHCONV
restore_paths PB
purge after-PB

# LEG S -- the banked rows this box sweeps (tL-i7-sweeps.txt minus T_ROWS, plus derived canaries / PRE-2 MISSING). One
# row at a time, exact match. Named readings: net/rpc (g-deadlock: TestSendDeadlock; and the shutdown fatal's absence,
# counted from the log), reflect + fmt (c1-token: 0 movers at 396 + 22 / 62 + 1), time + net/rpc/jsonrpc (g-deadlock's
# timer probe / channel neighbours). Every row reaches r-m4's TryStageModule (declines for stdlib) and r-m6's godebug init.
# A for-loop over the list, never `while read < file` (powershell.exe inside freegb would eat rows).
for pkg in $(cat "$RUNLIST"); do
  n=$(echo "$pkg" | tr '/' '.')
  capped "S:$pkg" "sweep-$n" "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  src=$LEG_RC; st0=$LEG_T0
  dl=$(dlcount "S:$pkg" "$LOGDIR/sweep-$n.log" "src/core/$pkg" "$st0")
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT|RERUN)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200) :: ${dl:-DEADLOCK unread}"
  case "$pkg" in net/rpc|net/rpc/jsonrpc) echo "$dl" | grep -q 'DEADLOCK total=0 ' || finding "S:$pkg deadlock lines: $dl" ;; esac
  case "$pkg" in
    net/rpc) named='TestSendDeadlock' ;;
    *) named='' ;;
  esac
  if [ "$src" != 0 ] || [ -n "$named" ]; then
    hp treport --dir "$WW\\src\\core\\$(echo "$pkg" | tr '/' '\\')" --log "$LOGW\\sweep-$n.log" --t0 "$st0" --rc "$src" --label "S:$pkg" --named "$named" > "$LOGDIR/sweep-$n.reading.txt" 2>&1
    while IFS= read -r l; do stamp "    $l"; done < <(grep -aE 'FRESHNESS|ERROR-NAMES|NAMED ' "$LOGDIR/sweep-$n.reading.txt" | cut -c1-400 | head -n 12)
    [ -f "src/core/$pkg/go2cs_test_comparison.json" ] && cp "src/core/$pkg/go2cs_test_comparison.json" "$LOGDIR/sweep-$n.go2cs_test_comparison.json"
  fi
done

# LEG NR -- g-deadlock: net/rpc four MORE times (5 in all with the S row): EXPECT each PASS 15, rc 0, 0 'all goroutines are
# asleep' lines (K's run 1 died at host shutdown on TestSendDeadlock's leaked select{}; timing-dependent, hence the loop).
# Critic 0 #16: the count reads the console, the 'full output:' file and the fresh record's stderr tails (dlcount).
for i in 2 3 4 5; do
  capped "NR:$i" "sweep-net.rpc-nr$i" "$SWEEP_ROW_CAP" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter net/rpc -Exact
  dl=$(dlcount "NR:$i" "$LOGDIR/sweep-net.rpc-nr$i.log" src/core/net/rpc "$LEG_T0")
  stamp "  NR:$i: $(grep -aE '^\s*(PASS|FAIL|COUNT|DRIFT)' "$LOGDIR/sweep-net.rpc-nr$i.log" | tail -n 1 | cut -c1-160) :: ${dl:-DEADLOCK unread}"
  echo "$dl" | grep -q 'DEADLOCK total=0 ' || finding "NR:$i deadlock lines: $dl"
done

# LEG TE + HS -- post-sweep readings, no new runs (K ruling (4): sweep-rewritten test sources are readings only).
#   TE (r-d6, i9-crosspkg): added lines in the rewritten sources carrying the D6 or embed-hop signature. EXPECT 0. The
#      patterns are INFERRED but positive-controlled (they fire on both seats' own golden diffs) and negative-controlled
#      (0 on L's own src/core diff). The i9 ships its shard's patch; COORD runs `tL-helpers.py teattr` on it too.
#   HS (r-m4, r-m6): no rewritten stdlib host carries a module-path argument; GoDefaultGodebug still names exactly 3 files.
#   csproj among the rewrites: EXPECT 0 (the fixup's S1 + opt-in equal the template's emission).
git diff -U0 > "$LOGDIR/S-rewrites-U0.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/S-tracked-changes.txt"
hp teattr "$(cygpath -w "$LOGDIR/S-rewrites-U0.patch")" > "$LOGDIR/te.log" 2>&1; terc=$?
stamp "  TE rc=$terc :: $(grep -aE '^TE (files|VERDICT)' "$LOGDIR/te.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-300)"
hsa=$(hs_m4); hsb=$(hs_m6)
stamp "  HS: module-path hosts (worktree)=$hsa (want 0) GoDefaultGodebug files=[${hsb% }] ($([ "$hsb" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'MOVED')) :: tracked rewrites=$(wc -l < "$LOGDIR/S-tracked-changes.txt") csproj among them=$(grep -c 'csproj' "$LOGDIR/S-tracked-changes.txt") go2cs_test_host.cs among them=$(grep -c 'go2cs_test_host.cs' "$LOGDIR/S-tracked-changes.txt")"
[ "$terc" = 0 ] || finding "TE (sweep rewrites) rc=$terc: read te.log"
[ "$hsa" = 0 ] && [ "$hsb" = "$HS_WANT" ] || finding "HS after the sweep: module-path hosts=$hsa GoDefaultGodebug=[${hsb% }]"
# Critic 1 #3: the sweep's rewrites are saved (the -U0 patch + list above, and restore_paths' binary patch) and then
# RESTORED, so the T legs start from HEAD's sources and the END count holds only T-leg material. Then the purge (which
# would otherwise abort on a sweep's legitimate ' D').
restore_paths S
purge after-sweep

# LEG T -- direct -tests (K's tleg) for the two rows K banked, each compared with the roster of record (rosterrow).
#  T:runtime/pprof (banked 145 + 7): c1-token (token path), g-deadlock (AllGoroutinesForeverBlocked takes s_profileGate
#    EXCLUSIVE, the goroutine profile's gate), r-m6. BANK-READING + roster compare.
#  T:runtime (banked 10819 + 71; K's re-read 6162 s, K's battery 6792 s): c1-token ('runtime over 10,827 reached';
#    mcleanup.cs's token), g-deadlock's crash family BY NAME (the seat's owed 'union vs fix by name' read: pass/pass each;
#    TestTracebackSystem/panic and /trap disclosed-divergent on their pinned signatures), r-m6 (ᴛInitEnvs reads the
#    entry-assembly attribute: stdlib hosts carry none).
#  TBS-W: ONE windows control iteration of the TestTracebackSystem filter (P2's TBS-LOOP x10 is the linux arm). A
#    filtered run is DIAGNOSTIC ONLY (never a bank reading). It writes the same src/core/runtime record files, so it
#    runs BEFORE T:runtime (critic 1 #2): the bank reading is the LAST record left in the tree for the bank step.
export MSYS_NO_PATHCONV=1
convbuild T
tleg(){ # pkg, timeout, suffix, named-prefixes, bank(yes|no), extra args...   (K verbatim + the roster compare)
  local pkg=$1 to=$2 suf=$3 named=$4 bank=$5; shift 5
  local pb n L trc tt0 rd
  deadline_check "T:$pkg:$suf"   # R5 (leg() checks again; this stops before any tleg bookkeeping)
  pb=$(echo "$pkg" | tr '/' '\\'); n=$(echo "$pkg" | tr '/' '.')
  L="$LOGDIR/tests-$n-$suf.log"; rd="$LOGDIR/tests-$n-$suf.reading.txt"
  leg "T:$pkg:$suf" "tests-$n-$suf" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  trc=$LEG_RC; tt0=$LEG_T0
  if [ "$bank" = yes ]; then
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" --bank > "$rd" 2>&1
    v=$(grep -aoE 'BANK-READING rc=[0-9]+ validated=[0-9None]+ disclosed-divergent=[0-9]+' "$rd" | sed -E 's/.*validated=([0-9None]+) disclosed-divergent=([0-9]+)/\1 \2/')
    hp rosterrow "$WW" "$pkg" ${v:-None 0} >> "$rd" 2>&1
  else
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" > "$rd" 2>&1
  fi
  while IFS= read -r l; do stamp "  $l"; done < <(grep -aE 'FRESHNESS|RESULTS-TAIL|VALIDATED-LINE|COMPARISON|ERROR-NAMES|NAMED |BANK-READING|ROSTERROW' "$rd" | cut -c1-500 | head -n 90)
  for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$n-$suf.$f"; done
}
CRASH='TestSimpleDeadlock,TestInitDeadlock,TestLockedDeadlock,TestLockedDeadlock2,TestGoexitDeadlock,TestGoexitCrash,TestGoNil,TestGoexitInPanic,TestPanicAfterGoexit,TestRecoveredPanicAfterGoexit,TestRecoverBeforePanicAfterGoexit,TestRecoverBeforePanicAfterGoexit2,TestMainGoroutineID,TestNoHelperGoroutines,TestNetpollDeadlock,TestPanicDeadlockGosched,TestPanicDeadlockSyscall,TestPanicLoop,TestStopTheWorldDeadlock,TestCrashHandler,TestTracebackSystem'
# K's named list verbatim (TestBlockMutexProfileInlineExpansion and TestMutexProfile read 'absent from both sides' at
# K's re-read) + TestBlockProfile (g-deadlock's exclusive profile gate sits beside the block/mutex profile readers).
tleg runtime/pprof 30m all 'TestGoroutineProfileConcurrency,TestGenericsHashKeyInPprofBuilder,TestGenericsInlineLocations,TestHeapRuntimeFrames,TestBlockMutexProfileInlineExpansion,TestMutexProfile,TestBlockProfile' yes
# TBS-W BEFORE T:runtime (critic 1 #2). Its deadlock count reads the console, any full-output file and the fresh
# record's stderr tails (critic 0 #16); 'all goroutines are asleep' must not appear (the /panic child's expected text is
# its own pinned signature, read through the disclosed-divergent verdict).
BANK_IN_TREE=1   # runtime/pprof's bank reading is in the tree from here on: a deadline stop must not revert it
tleg runtime 30m tbs 'TestTracebackSystem' no -test-filter '^TestTracebackSystem$'
tbsdl=$(dlcount "TBS-W" "$LOGDIR/tests-runtime-tbs.log" src/core/runtime "$LEG_T0")
stamp "  TBS-W (EXPECT /panic and /trap disclosed-divergent, 0 deadlock lines): ${tbsdl:-DEADLOCK unread}"
echo "$tbsdl" | grep -q 'DEADLOCK total=0 ' || finding "TBS-W deadlock lines: $tbsdl"
tleg runtime 150m all "TestRuntimeLockMetricsAndProfile,TestTracebackParentChildGoroutines,TestFinalizerRegisterABI,TestCleanupAfterFinalizer,TestPeriodicGC,TestNumCPU,$CRASH" yes
unset MSYS_NO_PATHCONV

# LEG TE-T + HS-T (critic 0 #6) -- the TE and HS readings repeated over the T legs' rewrites (the sweep's were restored
# before the T legs, so this patch is runtime + runtime/pprof only: the largest test corpus, and the host ᴛInitEnvs
# reads). No restore: the T legs' bank material stays in the tree for the bank step.
git diff -U0 > "$LOGDIR/T-rewrites-U0.patch"
git status --porcelain | grep -v '^??' > "$LOGDIR/T-tracked-changes.txt"
hp teattr "$(cygpath -w "$LOGDIR/T-rewrites-U0.patch")" > "$LOGDIR/te-T.log" 2>&1; tetrc=$?
stamp "  TE-T rc=$tetrc :: $(grep -aE '^TE (files|VERDICT)' "$LOGDIR/te-T.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-300)"
hsta=$(hs_m4); hstb=$(hs_m6)
stamp "  HS-T: module-path hosts (worktree)=$hsta (want 0) GoDefaultGodebug files=[${hstb% }] ($([ "$hstb" = "$HS_WANT" ] && echo 'exactly the 3' || echo 'MOVED')) :: tracked rewrites=$(wc -l < "$LOGDIR/T-tracked-changes.txt") csproj among them=$(grep -c 'csproj' "$LOGDIR/T-tracked-changes.txt")"
[ "$tetrc" = 0 ] || finding "TE-T (T-leg rewrites) rc=$tetrc: read te-T.log"
[ "$hsta" = 0 ] && [ "$hstb" = "$HS_WANT" ] || finding "HS-T after the T legs: module-path hosts=$hsta GoDefaultGodebug=[${hstb% }]"

# LEG TE-i9 (critic 0 #5) -- the i9 shard's patch through the same TE reader, scripted (tL-i9-te.sh). Runs here only
# when I9_PATCH names a file that has arrived; otherwise COORD runs `bash tL-i9-te.sh <patch>` from this run folder
# when it lands (same reader, same gate: rc != 0 is a finding to read).
if [ -n "$I9_PATCH" ] && [ -f "$I9_PATCH" ]; then
  bash "$SD/tL-i9-te.sh" "$I9_PATCH" > "$LOGDIR/te-i9.log" 2>&1; LEG_RC=$?
  stamp "  TE-i9 rc=$LEG_RC :: $(grep -aE '^TE (files|VERDICT)' "$LOGDIR/te-i9.log" | tr -d '\r' | tr '\n' ' ' | cut -c1-300)"
  [ "$LEG_RC" = 0 ] || finding "TE-i9 rc=$LEG_RC: read te-i9.log"
else
  stamp "  TE-i9: NOT RUN here (I9_PATCH=${I9_PATCH:-unset}) -- run 'bash tL-i9-te.sh <tracked-changes-U0.patch>' from this run folder when the i9's patch arrives"
fi

# LEG TK-WALL -- c1-token's stated element-token cost (~12x a first &s[i] token read): each leg's wall vs the FASTEST
# K reading of the same leg (K's battery and K's re-read). A ratio > 1.25 is a FINDING to read, never a gate.
hp wallcmp "$(cygpath -w "$SUM")" $(for k in $K_SUMS; do [ -f "$k" ] && cygpath -w "$k"; done) > "$LOGDIR/wallcmp.log" 2>&1
while IFS= read -r l; do stamp "  $l"; done < <(grep -a '^WALLCMP' "$LOGDIR/wallcmp.log" | head -n 25)

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') (T-leg material only; the sweep's were restored) tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD) SIc=[$SICV] E=[attr ${EATTR:-unread}] GT-NOT-FOUND=[${GT_NF% }] SyncMutexProfile=[${GT_SMP% }] C1-PROBE(R3, stated)=[${GT_PROBE% }] 5-TRUNCATED=[${NMT:-no}]"
stamp "NONZERO LEGS (SIc, NVR and FXc1/FXc2 are EXPECTED non-zero controls; TBS-W is a filtered diagnostic; NGb51 is GATED (R2): any red there is a finding): $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ')"
stamp "FINDINGS (gates that read wrong without a nonzero rc): ${FINDINGS:-none}"
stamp "BATTERY DONE"
