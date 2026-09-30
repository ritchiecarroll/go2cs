#!/usr/bin/env bash
# TRAIN J RUN 2 on the i7 -- the re-gate after the post-run-1 seat (the i9's A11 windows fix, 019ed294d3, merged as
# d616cc686c). Launch from a PER-RUN COPY (floor 4): copy tJ-regate2.sh + tJ-seats.txt to a run folder and run the copy.
# Scope, ruled in PRERES (POST-RUN-1 SEAT + RUN-1 LEG 5): the fix touches ONLY src/core/runtime/mfinal.cs and
# GolibTests/CleanupDispatchTests.cs -- no converter, gen or emission change, so C/E/SY/2b/GN/TR/CNR/H7 stand from run 1.
# Re-read here: GolibTests x2; the 6 run-1 Output timeouts + the one finalizer behavioral project, ISOLATED; runtime's
# finalizer rows first (the canary), then 9 finalizer-sensitive sweeps, runtime/debug, and the FULL windows runtime row
# (run 1's hung at TestCleanupAfterFinalizer: 65 of 10,890 verdicts).
W=/h/go2cs-tmp-coord/tJ
WB='H:\go2cs-tmp-coord\tJ'
EXPECT_HEAD=${EXPECT_HEAD:?}
SD=$(cd "$(dirname "$0")" && pwd)
LOGDIR=$SD/tJ-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }

export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"

cd "$W" || { stamp "ABORT: no worktree $W"; exit 2; }
h=$(git rev-parse --short=10 HEAD)
[ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { stamp "ABORT: GOROOT pin $(go env GOROOT)"; exit 2; }
while IFS='|' read -r b sha msg; do [ -z "$b" ] && continue; git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }; done < "$SD/tJ-seats.txt"
git merge-base --is-ancestor 019ed294d3 HEAD || { stamp "ABORT: the A11 fix 019ed294d3 is not an ancestor"; exit 2; }
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before run 2"; exit 2; }
stamp "PRE run2 head=$h tree=$(git rev-parse --short=10 HEAD^{tree}) $(go version) dotnet=$(dotnet --version | tr -d '\r') free=$(freegb)G"

leg(){ # name, logfile-suffix, command...
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0=$(date +%s) f c
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
purge(){ # label
  local P left td
  P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  td=$(git status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  [ "$left" = 0 ] && [ "$td" = 0 ] || { stamp "ABORT: purge incomplete or tracked deletions"; exit 3; }
}

# LEG GT -- GolibTests, Debug and Release (run 1: Arm6 red in both; the fix's own red arm is Arm10)
for cfg in Debug Release; do
  leg GT-$cfg golibtests-$cfg bash -c "dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false && dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build"
  stamp "  GT-$cfg: $(grep -aE '(Passed|Failed)!' "$LOGDIR/golibtests-$cfg.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/golibtests-$cfg.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
done
purge after-GT

# LEG B7 -- run 1's six Output timeouts (a load window) plus the one behavioral project that uses finalizers, each ISOLATED
for p in ClosureBareReturnNamedResults ClosureLocalNoHeapBox ClosureMixedReturnUnsigned ClosureReturnAnonStruct DeferFinallyLowering EmbeddedValuePointerMethod SetFinalizerBridge; do
  leg "B:$p" "behav-$p" powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$p"
  stamp "  B:$p: $(grep -aE '^\s+(Transpile|Compile|Target|Output)\s+pass' "$LOGDIR/behav-$p.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300) :: $(grep -aE '^(PASS|FAIL)' "$LOGDIR/behav-$p.log" | tail -n 1 | cut -c1-120)"
done
purge after-behavioral

export MSYS_NO_PATHCONV=1
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1
EXE="$W/src/go2cs/bin/go2cs.exe"
[ -x "$EXE" ] || { stamp "LEG T ABORT: no converter at $EXE"; exit 3; }
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
tleg(){ # pkg, timeout, suffix, extra args...
  local pkg=$1 to=$2 suf=$3; shift 3
  local pb=$(echo "$pkg" | tr '/' '\\')
  leg "T:$pkg:$suf" "tests-$(echo "$pkg" | tr '/' '.')-$suf" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  local L="$LOGDIR/tests-$(echo "$pkg" | tr '/' '.')-$suf.log"
  stamp "  T:$pkg:$suf: $(grep -aiE 'Validated|comparison failed|match|diverg|verdict' "$L" | tail -n 2 | tr '\r\n' '  ' | cut -c1-500)"
  for f in go2cs_test_comparison.json go2cs_test_results.json; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$(echo "$pkg" | tr '/' '.')-$suf.$f"; done
}

# LEG T0 -- the CANARY: runtime's finalizer and cleanup rows by name (the i9's windows reading at the fix: 14 of 15 = Go,
# TestFinalizerRegisterABI pre-existing). Read before the long legs; a hang here is the A11 defect still live.
tleg runtime 30m fin -test-filter '^(TestCleanup.*|TestFinalizer.*)$'
unset MSYS_NO_PATHCONV

# LEG S -- the finalizer-sensitive banked rows (i7 and i9-shard rows alike), one at a time, exact match
for pkg in sync unique internal/poll weak runtime/metrics internal/sync os reflect crypto/tls; do
  n=$(echo "$pkg" | tr '/' '.')
  leg "S:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200)"
done
purge after-sweep

# LEG T -- runtime/debug (the row TRAIN J banks) and the FULL windows runtime row
export MSYS_NO_PATHCONV=1
tleg runtime/debug 30m all
tleg runtime 150m all
unset MSYS_NO_PATHCONV

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD)"
stamp "RUN2 DONE"
