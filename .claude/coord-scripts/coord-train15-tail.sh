#!/bin/bash
# Train 15 tail legs re-run after the disk floor (18 GB) aborted them; 38 GB free after purging behavioral build output.
set -u
SP="/c/Projects/go2cs/.claude/coord-scripts"
export DOTNET_ROOT="$HOME\dotnet10"; export GOROOT="$HOME\sdk\go1.23.12"; export PATH="$HOME/dotnet10:$HOME/sdk/go1.23.12/bin:$PATH"; export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
ts=$(date '+%Y%m%d-%H%M%S'); stamp(){ echo "[$(date '+%F %T')] $*"; }
stamp "TAIL START head=$(git rev-parse --short HEAD) go=[$(go version)] free=$(powershell -NoProfile -Command '(Get-PSDrive C).Free/1GB' | tr -d '\r' | cut -c1-5)GB"
stamp "BATTERY: filtered behavioral run"
(cd src/tests/Behavioral && for f in TypedNilPtrArrayDims TypedNilPtrArrayPositions; do powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter $f; echo "runner $f exit=$?"; done) > "$SP/coord-train15-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
stamp "RUNNER: $(grep -aoE 'runner [A-Za-z]+ exit=[0-9-]+' "$SP/coord-train15-runner-$ts.log" | tr '\n' ' ')"
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5 -SweepTimeout 30m > "$SP/coord-train15-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train15-sweeps-$ts.log"
stamp "SWEEPS: $(grep -aoE 'UNION BATTERY EXIT [0-9]+' "$SP/coord-train15-sweeps-$ts.log" | tail -1)"
stamp "BATTERY: nistec COST PAIR vs the train-14 head (control worktree at 8c15217c8), -AllowBusy"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "8c15217c8" -SkipWarmup -Rounds 2 -AllowBusy > "$SP/coord-train15-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train15-nistec-pair-$ts.log"
stamp "PAIR: $(grep -aE 'mean' "$SP/coord-train15-nistec-pair-$ts.log" | cut -c23-120 | tr '\n' ' ')"
stamp "BATTERY: reflect RUN -AllowBusy"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train15 -AllowBusy > "$SP/coord-train15-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train15-reflectrun-$ts.log"
stamp "REFLECT: $(grep -aoE '(matched|disclosed|mismatch)[^,]*[0-9]+' "$SP/coord-train15-reflectrun-$ts.log" | tr '\n' ' ' | cut -c1-160)"
git checkout HEAD -- src/core docs/validation 2>/dev/null; git clean -fdq src/core 2>/dev/null
stamp "TAIL DONE dirty=$(git status --porcelain | wc -l)"
