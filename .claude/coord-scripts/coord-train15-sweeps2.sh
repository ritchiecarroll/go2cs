#!/bin/bash
# Train 15 sweep rows 7-14, re-run after the 25 GB floor aborted them twice (69.9 GB free after the main-checkout purge).
set -u
SP="/c/Projects/go2cs/.claude/coord-scripts"
export DOTNET_ROOT="$HOME\dotnet10"; export GOROOT="$HOME\sdk\go1.23.12"; export PATH="$HOME/dotnet10:$HOME/sdk/go1.23.12/bin:$PATH"; export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
ts=$(date '+%Y%m%d-%H%M%S'); stamp(){ echo "[$(date '+%F %T')] $*"; }
stamp "SWEEPS2 START head=$(git rev-parse --short HEAD) go=[$(go version)] free=$(powershell -NoProfile -Command '(Get-PSDrive C).Free/1GB' | tr -d '\r' | cut -c1-5)GB dirty=$(git status --porcelain | wc -l)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5 -SweepTimeout 30m > "$SP/coord-train15-sweeps2-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train15-sweeps2-$ts.log"
stamp "SWEEPS2: $(grep -aoE 'UNION BATTERY EXIT [0-9]+' "$SP/coord-train15-sweeps2-$ts.log" | tail -1) :: $(grep -aE '^\s+(PASS|FAIL)\s' "$SP/coord-train15-sweeps2-$ts.log" | awk '{print $1":"$2}' | tr '\n' ' ')"
git checkout HEAD -- src/core docs/validation 2>/dev/null; git clean -fdq src/core 2>/dev/null
stamp "SWEEPS2 DONE dirty=$(git status --porcelain | wc -l)"
