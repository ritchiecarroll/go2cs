#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
T=$HOME/AppData/Local/Temp/claude
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1
V=$(go version 2>&1); case "$V" in *go1.23.12*) echo "PIN OK: $V";; *) echo "ABORT pin: $V"; exit 9;; esac
cd "$W" || exit 8
echo "=== TREE $(git log -1 --format=%h)"
git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null && { echo "  ANCESTRY: token seat PRESENT -- WRONG TREE"; exit 8; } || echo "  ANCESTRY: token seat absent"
echo "=== restoring pipeline dirt before the sweep (BOTH roots)"
git checkout -- src/core docs/validation 2>/dev/null; git clean -qfd src/core 2>/dev/null
echo "  dirty: $(git status --porcelain | wc -l)"
echo "=== FULL VALIDATED SWEEP (defaults: Release + tiering off, bank-eligible)"
S=$(date +%s)
powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 > "$T/drop-sweep.log" 2>&1
echo "  exit $? after $(( $(date +%s) - S ))s"
grep -iE 'NO REGRESSION|REGRESSION|FAIL|NOT MEASURED|banked|matched|summary' "$T/drop-sweep.log" 2>/dev/null | tail -12 | sed 's/^/    /'
echo "=== SWEEP LEG COMPLETE"
