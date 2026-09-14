#!/usr/bin/env bash
# coord-t25-rehearsal-gates.sh -- train-25 REHEARSAL gates at the dry-run head (coord-t25-dryrun), run AFTER every seat is
# merged there and AFTER the train-24 battery has freed the box (never concurrently: two CNRs on one machine contend, and
# the leg scripts' defaults point at the coordinator worktree -- every call below passes -Worktree explicitly).
# Legs: (1) converter suite + full CNR with the CHANGED set stamped by name (the union golden check for RINC2's goldens
# and B's guard project); post-CNR behavioral restore; (2) go2cs.slnx dev build (the behavioral COMPILE gate);
# (3) GolibTests --no-build + each-class-alone (RINC2's golib change); (4) reflect -tests build (lift/dedup neighbourhood);
# (5) stdlib solution at linux and darwin --no-incremental with a purge between (G's B footprint on the targets the
# assembly battery's windows slnx leg cannot see). The FULL behavioral suite is the assembly battery's, not the rehearsal's.
set -u
export DOTNET_ROOT='$HOME\dotnet10' GOROOT='$HOME\sdk\go1.23.12' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
SP="C:/Projects/go2cs/.claude/coord-scripts"
SC="$HOME/AppData/Local/Temp/claude/C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7/388822cf-bade-49e8-96a9-7ab378101ad4/scratchpad"
WT_U="C:/Projects/go2cs/.claude/worktrees/coord-t25-dryrun"; WT_W='C:\Projects\go2cs\.claude\worktrees\coord-t25-dryrun'
ts=$(date +%Y%m%d-%H%M%S); LOG="$SC/coord-t25-rehearsal-$ts.log"
stamp(){ echo "[$(date '+%F %T')] $*" | tee -a "$LOG"; }
cd "$WT_U" || { echo "no dry-run worktree"; exit 2; }
if pgrep -f 'coord-union-battery|BehavioralRunner|coordRunner' >/dev/null 2>&1 && ps -W 2>/dev/null | grep -qiE 'BehavioralRunner|coordRunner24'; then stamp "ABORT: a behavioral runner is alive on this box (the train-24 battery?) -- never run two"; exit 3; fi
stamp "T25 REHEARSAL GATES head=$(git rev-parse --short=9 HEAD) go=$(go version) dotnet=$(dotnet --version) dirty=$(git status --porcelain | wc -l) seats: $(git log --oneline --merges -8 | cut -c1-60 | tr '\n' '|')"
stamp "GATE 1: converter suite + full CNR (-Worktree dry-run)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -Worktree "$WT_W" -Label t25-rehearsal > "$SC/coord-t25-reh-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SC/coord-t25-reh-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SC/coord-t25-reh-suite-cnr-$ts.log") :: $(grep -aE '^ok|^FAIL|LEG1 END|LEG2 END' "$SC/coord-t25-reh-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SC/coord-t25-reh-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "GATE 2: go2cs.slnx dev build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" -Worktree "$WT_W" > "$SC/coord-t25-reh-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SC/coord-t25-reh-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SC/coord-t25-reh-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SC/coord-t25-reh-slnx-$ts.log" | tail -1)"
stamp "GATE 3: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -Worktree "$WT_W" -NoBuild > "$SC/coord-t25-reh-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SC/coord-t25-reh-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SC/coord-t25-reh-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SC/coord-t25-reh-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" -Worktree "$WT_W" > "$SC/coord-t25-reh-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SC/coord-t25-reh-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SC/coord-t25-reh-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SC/coord-t25-reh-alone-$ts.log" | tail -1)"
stamp "GATE 4: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" -Worktree "$WT_W" > "$SC/coord-t25-reh-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SC/coord-t25-reh-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SC/coord-t25-reh-reflect-$ts.log") :: $(grep -aE 'exit=' "$SC/coord-t25-reh-reflect-$ts.log" | tr '\n' ' ')"
for goos in linux darwin; do
  stamp "GATE 5: stdlib slnx -p:GoTargetOS=$goos --no-incremental (purge first)"
  powershell -NoProfile -ExecutionPolicy Bypass -File src/clean-bin.ps1 -Force > /dev/null 2>&1
  L5="$SC/coord-t25-reh-stdlib-$goos-$ts.log"
  dotnet build src/go2cs-stdlib.slnx -c Debug -m -p:UseSharedCompilation=false -p:GoTargetOS=$goos --no-incremental -clp:ErrorsOnly > "$L5" 2>&1; rc=$?
  stamp "LEG stdlib-$goos exit=$rc :: strictErrors=$(grep -acE 'error (CS|MSB|NETSDK)[0-9]+' "$L5") :: $(grep -aE 'error (CS|MSB|NETSDK)[0-9]+' "$L5" | sed -E 's/.*(error [A-Z]+[0-9]+[^\[]*).*/\1/' | sort | uniq -c | sort -rn | head -3 | tr '\n' ' ' | cut -c1-200)"
done
powershell -NoProfile -ExecutionPolicy Bypass -File src/clean-bin.ps1 -Force > /dev/null 2>&1
stamp "T25 REHEARSAL GATES DONE head=$(git rev-parse --short=9 HEAD) -- read every LEG line above; a CHANGED set names the rung, a strictErrors>0 names the target"
