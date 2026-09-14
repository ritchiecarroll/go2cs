#!/usr/bin/env bash
# coord-train28-relegs.sh -- re-run the three train-28 battery legs the disk preflight refused (sweeps, nistec pair, reflect run) on the assembled head; stamps into the same assembly log.
set -u
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
ts=$(date +%Y%m%d-%H%M%S)
TRAIN_LABEL=train28
log="$SP/coord-train28-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 28 RELEGS START head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l) free=$(df -h /c | tail -1 | awk '{print $4}')"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train28-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train28-sweeps-$ts.log"
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train28-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train28-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train28-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train28-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train28-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train28-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train28-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train28-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train28-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train28-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 28 RELEGS DONE"
