#!/usr/bin/env bash
# TRAIN FL landing, THE RE-READS AT THE HEAD THAT LANDS (checklist section 6, step 3), one after the other. NEW at FL
# (FL11): TRAIN Q's two hand fragments written down as one script -- coord-scratch/tQ/run2-reread/reread-chain.sh (the
# module legs, the final reads, T2) and pub-consumer.sh (PUB and the C# consumer runner, launched as TRAIN P's
# consumer-land.sh launched them). Q's first launch of the last two put .NET 10 AHEAD of PowerShell 7's own runtime
# (pwsh rc 150) and gave the consumer runner no -WorkRoot (rc 1): launch errors, nothing ran (ledger 09:04, 2026-10-07).
#   RUN=<the battery of record's run folder> R2=<a FRESH per-run copy of this folder, under coord-scratch/tFL/>
#   HEADFULL=<the head that lands, full sha> BASE=<the union's base, full sha> [BATTERY_HEAD_OK=<10>] [BATTERY_RC_OK=<n>]
#   bash "$R2/tFL-land-rereads.sh"
#   STEP 1 MOD    tFL-modules-legs.sh at HEADFULL, into R2 (every -recurse module -tests row of the battery, again: FL10)
#   STEP 2 FINAL  tFL-land-final-reads.sh with RUN (the converter suite at the head that lands; what the battery owes)
#   STEP 3 T2     runtime/debug twice into one tree: the same file names, more than one .pdb, 0 symbol refusals
#   STEP 4 PUB    check-published-output.ps1: pwsh STARTS with DOTNET_ROOT unset and the login PATH; .NET 10 is put
#                 first INSIDE the session (the battery's PUB leg, TRAIN P's consumer-land.sh)
#   STEP 5 CONS   run-csharp-consumer.ps1 -WorkRoot <R2>/consumer-land, launched the same way
# Every step restores the tree after itself and the END line reads tracked changes 0, deletions 0, master unmoved.
# It never commits, pushes or posts. Exit 0 only when every step read green; the first red step's number otherwise.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
RUN=${RUN:?set RUN to the run folder of the battery of record}
R2=${R2:?set R2 to this script s own fresh run folder}
HEADFULL=${HEADFULL:?set HEADFULL to the head that lands, full sha}
BASE=${BASE:?set BASE to the base the union was assembled on, full sha}
W=${W:-/h/go2cs-tmp-coord/tFL}
st(){ echo "$(date +%H:%M:%S) $*"; }
case "$R2" in /h/go2cs-tmp-coord/coord-scratch/tFL/?*) ;; *) st "ABORT: R2=$R2 is not under /h/go2cs-tmp-coord/coord-scratch/tFL/"; exit 2 ;; esac
case "$R2" in *..*) st "ABORT: R2 holds '..'"; exit 2 ;; esac
[ "$SD" = "$(cd "$R2" 2>/dev/null && pwd)" ] || { st "ABORT: launch the copy INSIDE R2 ($R2), not $SD (floor 4: a per-run copy)"; exit 2; }
[ "$RUN" != "$R2" ] || { st "ABORT: R2 is the battery's own run folder: the re-reads write mod-logs and out of their own"; exit 2; }
for f in tFL-modules-legs.sh tFL-land-final-reads.sh tFL-helpers.py tFL-seats-draft.txt; do [ -f "$R2/$f" ] || { st "ABORT: $R2/$f missing (copy the script set into R2)"; exit 2; }; done
[ ! -e "$R2/mod-logs" ] && [ ! -e "$R2/consumer-land" ] || { st "ABORT: $R2 already holds mod-logs or consumer-land: use a FRESH folder"; exit 2; }
[ -z "${MSYS_NO_PATHCONV:-}" ] || { st "ABORT: MSYS_NO_PATHCONV is set in this shell (Q5): unset it"; exit 2; }
ORIGPATH="$PATH"
D10W=$(cygpath -w "$HOME/dotnet10")
GR=$(cygpath -w "$HOME/sdk/go1.24.13")   # floor 6: GOROOT spelled as go env GOROOT prints it (backslashes)
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
export MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
unset GO2CS_MODULE_ROOT
BUILDPATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$ORIGPATH"      # the build shell (steps 1 to 3)
GOPIN_PATH="$HOME/sdk/go1.24.13/bin:$(printf '%s' "$ORIGPATH" | tr ':' '\n' | grep -v -i -E '/dotnet10/?$' | paste -sd: -)"   # pwsh STARTS here (steps 4, 5)
PWSH=$HOME/.dotnet/tools/pwsh
cd "$W" || exit 2
[ "$(git rev-parse HEAD)" = "$HEADFULL" ] || { st "ABORT: $W is not at $HEADFULL"; exit 2; }
[ "$(git status --porcelain --untracked-files=no | grep -c .)" = 0 ] || { st "ABORT: tracked changes in $W"; exit 2; }
clean(){ git -C "$W" checkout -q -- . ; st "  tracked after $1 restore: $(git -C "$W" status --porcelain --untracked-files=no | grep -c .) deletions: $(git -C "$W" status --porcelain | grep -c '^ D')"; }
RED=0
st "STEP 1 MOD: tFL-modules-legs.sh at ${HEADFULL:0:10} (fresh folder $R2)"
env PATH="$BUILDPATH" DOTNET_ROOT="$D10W" EXPECT_HEAD="${HEADFULL:0:10}" BASE="$BASE" CONTROLS=1 bash "$R2/tFL-modules-legs.sh" > "$R2/modules-legs.console.log" 2>&1; mrc=$?
st "MOD rc=$mrc (0 green; 7 green with a re-read owed; 4 a failed verdict; 2/3 abort)"
grep -aE ': (PASS|FAIL|KNOWN|EXTERNAL) -- |REALMOD-VERDICT|^[0-9:]+ END ' "$R2/mod-logs/SUMMARY.txt" 2>/dev/null | cut -c1-260
case "$mrc" in 0|7) ;; *) [ "$RED" != 0 ] || RED=1 ;; esac
clean MOD
st "STEP 2 FINAL READS (RUN=$RUN${BATTERY_HEAD_OK:+, BATTERY_HEAD_OK=$BATTERY_HEAD_OK}${BATTERY_RC_OK:+, BATTERY_RC_OK=$BATTERY_RC_OK})"
env PATH="$BUILDPATH" DOTNET_ROOT="$D10W" RUN="$RUN" HEADFULL="$HEADFULL" W="$W" bash "$R2/tFL-land-final-reads.sh" > "$R2/final-reads.console.log" 2>&1; frc=$?
st "FINAL rc=$frc"; tail -n 4 "$R2/final-reads.console.log" | cut -c1-260
[ "$frc" = 0 ] || [ "$RED" != 0 ] || RED=2
clean FINAL
st "STEP 3 T2: runtime/debug twice into one tree"
for p in 1 2; do
  env PATH="$BUILDPATH" DOTNET_ROOT="$D10W" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter runtime/debug -Exact > "$R2/t2-p$p.log" 2>&1; trc=$?
  n=$(find src/core/runtime/debug/bin/tests/publish -maxdepth 1 -name '*.pdb' 2>/dev/null | wc -l)
  ( cd src/core/runtime/debug/bin/tests/publish 2>/dev/null && find . -maxdepth 1 -type f -printf '%f\n' | LC_ALL=C sort ) > "$R2/t2-p$p.files"
  refu=$(grep -ac 'cannot be found' "$R2/t2-p$p.log")
  st "T2 pass $p: rc=$trc $(grep -aE 'PASS|FAIL' "$R2/t2-p$p.log" | grep -a 'runtime/debug' | tail -n 1 | tr -s ' ') .pdb=$n refusals=$refu"
  { [ "$trc" = 0 ] && [ "$n" -gt 1 ] && [ "$refu" = 0 ]; } || { [ "$RED" != 0 ] || RED=3; }
done
same=$(cmp -s "$R2/t2-p1.files" "$R2/t2-p2.files" && echo yes || echo NO); st "T2 same files: $same"
[ "$same" = yes ] || [ "$RED" != 0 ] || RED=3
clean T2
st "STEP 4 PUB: check-published-output.ps1 (pwsh starts under the login PATH, DOTNET_ROOT unset; .NET 10 first INSIDE the session)"
PUB_CMD="\$env:PATH = '$D10W' + [IO.Path]::PathSeparator + \$env:PATH; & ./check-published-output.ps1; exit \$LASTEXITCODE"
( cd "$W/src/tests/Behavioral" && env -u DOTNET_ROOT PATH="$GOPIN_PATH" GOROOT="$GR" "$PWSH" -NoProfile -Command "$PUB_CMD" ) > "$R2/published-output.log" 2>&1; prc=$?
pubv=$(tr -d '\r' < "$R2/published-output.log" | grep -a 'published-output gate:' | tail -n 1)
st "PUB rc=$prc ${pubv:-NO VERDICT LINE (rc 150 = pwsh did not START: read the first lines of the log)}"
tr -d '\r' < "$R2/published-output.log" | grep -aE ' (HUNG|DIFFERS|NOT MEASURED)' | head -n 4
[ "$prc" = 0 ] && [ -n "$pubv" ] || [ "$RED" != 0 ] || RED=4
clean PUB
st "STEP 5 C# consumer runner (-WorkRoot $R2/consumer-land)"
WR="$R2/consumer-land"; mkdir -p "$WR" || exit 2
CMD="\$env:PATH = '$D10W' + [IO.Path]::PathSeparator + \$env:PATH; & ./src/tests/CSharpConsumer/run-csharp-consumer.ps1 -WorkRoot '$(cygpath -w "$WR")'; exit \$LASTEXITCODE"
( cd "$W" && unset GOROOT && env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -Command "$CMD" ) > "$R2/csharp-consumer.log" 2>&1; crc=$?
cv=$(tr -d '\r' < "$R2/csharp-consumer.log" | grep -a -E '^checks: ' | tail -n 1)
st "CONSUMER rc=$crc ${cv:-NO checks LINE}"
[ "$crc" = 0 ] && [ -n "$cv" ] || [ "$RED" != 0 ] || RED=5
clean CONSUMER
st "END tracked=$(git status --porcelain --untracked-files=no | grep -c .) deletions=$(git status --porcelain | grep -c '^ D') master=$(git ls-remote origin refs/heads/master | cut -c1-10) (EXPECT the base ${BASE:0:10})"
st "REREADS DONE: $([ "$RED" = 0 ] && echo GREEN || echo "RED at step $RED")"
exit "$RED"
