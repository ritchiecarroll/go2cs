#!/usr/bin/env bash
# coord-t30-evidence-json.sh -- the CHEAP decisive check after the rtlGetVersion fix merges: encoding/json was 491 PASS at
# master and died after 89 verdicts at the union, at its first HTTP-server test. It is the smallest row that carries the
# crash, so it answers "is the crash class cleared" in ~2-3 minutes instead of waiting for the battery's sweep leg (hours).
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not measuring"; exit 1; }
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED: $(go version)"; exit 1; }
ts=$(date +%Y%m%d-%H%M%S); L="$S/coord-t30-evidence-json-$ts.log"
echo "head $(git rev-parse --short=9 HEAD) -- sweeping encoding/json (master read 491 PASS, union read 89 then death)"
powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter encoding/json -Exact -TestTimeout 30m > "$L" 2>&1; rc=$?
echo "exit=$rc"; grep -aE '^\s+(PASS|FAIL|NOT MEASURED)|sweep: [0-9]' "$L" | cut -c1-110
git checkout -q HEAD -- src/core docs/validation 2>/dev/null; echo "restored: $(git status --porcelain | wc -l) entries left"
