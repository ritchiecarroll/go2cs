#!/usr/bin/env bash
# coord-train30-relegs.sh -- re-run the three train-28 battery legs the disk preflight refused (sweeps, nistec pair, reflect run) on the assembled head; stamps into the same assembly log.
set -u
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
ts=$(date +%Y%m%d-%H%M%S)
TRAIN_LABEL=train30
log="$SP/coord-train30-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 30 RELEGS TAIL START head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l) free=$(df -h /c | tail -1 | awk '{print $4}')"
stamp "BATTERY: nistec COST PAIR vs the train-29 head (b91684991) (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train30-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train30-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train30-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train30-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train30-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train30-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train30-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 30 RELEGS DONE"

