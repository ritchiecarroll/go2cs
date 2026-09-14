#!/usr/bin/env bash
# coord-train27-pair2.sh -- the ONE leg the relegs could not run (nistec COST pair; solo check aborted on a sub-agent's go2cs.exe). Waits for a solo box (positive PowerShell poll, max 30 min), then runs the pair with the relegs' stamps. The reflect run is NOT owed: the original chain measured it on this head.
set -u
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train27-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 27 PAIR2 START head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l) free=$(df -h /c | tail -1 | awk '{print $4}')"
n=0
while :; do
  busy=$(powershell -NoProfile -Command "(Get-Process go2cs,BehavioralRunner,testhost,vstest.console -ErrorAction SilentlyContinue | Measure-Object).Count")
  if [ "${busy:-0}" -eq 0 ]; then stamp "box solo after $((n*30)) s"; break; fi
  n=$((n+1)); if [ $n -ge 60 ]; then stamp "PAIR2 ABORT: box not solo after 30 min (busy=$busy)"; exit 3; fi
  sleep 30
done
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train27-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train27-nistec-pair-$ts.log"
stamp "PAIR: $(grep -E 'mean' "$SP/coord-train27-nistec-pair-$ts.log" | cut -c23-120 | tr '\n' ';')"
stamp "TRAIN 27 PAIR2 DONE $(tail -1 "$SP/coord-train27-nistec-pair-$ts.log")"
