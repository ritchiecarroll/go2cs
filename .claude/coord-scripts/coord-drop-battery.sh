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
git checkout -q --detach 8693aa5ba 2>/dev/null; git reset -q --hard 8693aa5ba; git clean -qfd 2>/dev/null
echo "=== TREE $(git log -1 --format=%h)"
if git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null; then echo "  ANCESTRY: token seat PRESENT -- WRONG TREE, ABORT"; exit 8; else echo "  ANCESTRY: token seat absent (correct for the drop)"; fi
echo "=== LEG 1: check-solution-integrity (per-GOOS cycle assertion + registration)"
powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-solution-integrity.ps1 > "$T/drop-leg-integrity.log" 2>&1
echo "  exit $?  | $(grep -ciE 'cycle|violation' "$T/drop-leg-integrity.log" 2>/dev/null) cycle/violation lines"
tail -3 "$T/drop-leg-integrity.log" | sed 's/^/    /'
echo "=== LEG 2: CNR (the drift instrument; re-transpiles unconditionally)"
S=$(date +%s)
powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1 > "$T/drop-leg-cnr.log" 2>&1
RC=$?; echo "  exit $RC after $(( $(date +%s) - S ))s"
grep -iE 'NO REGRESSION|REGRESSION|NOT MEASURED|byte-identical|skipped' "$T/drop-leg-cnr.log" 2>/dev/null | tail -6 | sed 's/^/    /'
echo "=== BATTERY PART 1 COMPLETE"
