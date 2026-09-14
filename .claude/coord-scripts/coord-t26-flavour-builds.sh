#!/usr/bin/env bash
# coord-t26-flavour-builds.sh -- train-26 union: stdlib solution at linux and darwin --no-incremental in the dry-run worktree (G Q35 per-GOOS hunks + C1RT2 hand-own on the targets the battery cannot see). Run from coord-t25-dryrun at the assembled head.
set -u
export DOTNET_ROOT='$HOME\dotnet10' GOROOT='$HOME\sdk\go1.23.12' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
SP="C:/Projects/go2cs/.claude/coord-scripts"
SC="$HOME/AppData/Local/Temp/claude/C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7/388822cf-bade-49e8-96a9-7ab378101ad4/scratchpad"
WT_U="C:/Projects/go2cs/.claude/worktrees/coord-t25-dryrun"; WT_W='C:\Projects\go2cs\.claude\worktrees\coord-t25-dryrun'
ts=$(date +%Y%m%d-%H%M%S); LOG="$SC/coord-t26-flavour-$ts.log"
stamp(){ echo "[$(date '+%F %T')] $*" | tee -a "$LOG"; }
cd "$WT_U" || { echo "no dry-run worktree"; exit 2; }
if pgrep -f 'coord-union-battery|BehavioralRunner|coordRunner' >/dev/null 2>&1 && ps -W 2>/dev/null | grep -qiE 'BehavioralRunner|coordRunner24'; then stamp "ABORT: a behavioral runner is alive on this box (the train-24 battery?) -- never run two"; exit 3; fi
stamp "T25 REHEARSAL GATES head=$(git rev-parse --short=9 HEAD) go=$(go version) dotnet=$(dotnet --version) dirty=$(git status --porcelain | wc -l) seats: $(git log --oneline --merges -8 | cut -c1-60 | tr '\n' '|')"
for goos in linux darwin; do
  stamp "GATE 5: stdlib slnx -p:GoTargetOS=$goos --no-incremental (purge first)"
  powershell -NoProfile -ExecutionPolicy Bypass -File src/clean-bin.ps1 -Force > /dev/null 2>&1
  L5="$SC/coord-t26-flav-stdlib-$goos-$ts.log"
  dotnet build src/go2cs-stdlib.slnx -c Debug -m -p:UseSharedCompilation=false -p:GoTargetOS=$goos --no-incremental -clp:ErrorsOnly > "$L5" 2>&1; rc=$?
  stamp "LEG stdlib-$goos exit=$rc :: strictErrors=$(grep -acE 'error (CS|MSB|NETSDK)[0-9]+' "$L5") :: $(grep -aE 'error (CS|MSB|NETSDK)[0-9]+' "$L5" | sed -E 's/.*(error [A-Z]+[0-9]+[^\[]*).*/\1/' | sort | uniq -c | sort -rn | head -3 | tr '\n' ' ' | cut -c1-200)"
done
powershell -NoProfile -ExecutionPolicy Bypass -File src/clean-bin.ps1 -Force > /dev/null 2>&1
stamp "T26 FLAVOUR BUILDS DONE head=$(git rev-parse --short=9 HEAD) -- read every LEG line above; a CHANGED set names the rung, a strictErrors>0 names the target"
