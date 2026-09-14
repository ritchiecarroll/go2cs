#!/usr/bin/env bash
# The BEFORE arm for the two class-reached banked rows the union never swept: net (472) and internal/poll (19).
# Taken at the assembly head 75758cf06 with the fix ABSENT, so the post-fix re-sweep is a PAIR rather than a single reading.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 1
[ "$(git rev-parse --short=9 HEAD)" = "75758cf06" ] || { echo "head is $(git rev-parse --short=9 HEAD), not the bare assembly head -- the BEFORE arm needs the fix ABSENT"; exit 1; }
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not measuring"; exit 1; }
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner' }).Count" 2>/dev/null | tr -d '\r ')
[ "${live:-0}" = "0" ] || { echo "legs live in the worktree ($live) -- not measuring"; exit 1; }
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED: $(go version)"; exit 1; }
ts=$(date +%Y%m%d-%H%M%S)
for pkg in internal/poll net; do
  L="$S/coord-t30-before-$(echo $pkg | tr '/' '.')-$ts.log"
  echo "[$(date '+%F %T')] BEFORE arm: $pkg at $(git rev-parse --short=9 HEAD)"
  powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact -TestTimeout 45m > "$L" 2>&1; rc=$?
  echo "[$(date '+%F %T')] $pkg exit=$rc :: $(grep -aE '^\s+(PASS|FAIL|NOT MEASURED)' "$L" | tail -1 | cut -c1-90)"
  D="$S/coord-pkg-run-record-$(echo $pkg | tr '/' '.')-t30before-$ts"
  if [ "$rc" != "0" ]; then mkdir -p "$D"; for f in go2cs_test_comparison.json go2cs_test_results.json; do p="src/core/$pkg/$f"; [ -f "$p" ] && cp "$p" "$D/"; done; echo "  record preserved -> $D ($(ls "$D" 2>/dev/null | tr '\n' ' '))"; fi
  git checkout -q HEAD -- src/core docs/validation 2>/dev/null
done
echo "[$(date '+%F %T')] BEFORE ARM DONE; tree dirty: $(git status --porcelain | wc -l)"
