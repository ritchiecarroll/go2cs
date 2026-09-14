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
[ -n "$(git status --porcelain)" ] && { echo "ABORT dirty"; exit 8; }
R=src/tests/Behavioral/BehavioralRunner/bin/Debug/net10.0/BehavioralRunner.exe
echo "=== building the runner"
dotnet build src/tests/Behavioral/BehavioralRunner/BehavioralRunner.csproj -c Debug -v q --nologo > "$T/drop-runner-build.log" 2>&1
echo "  build exit $?; runner present: $([ -f "$R" ] && echo yes || echo NO)"
[ -f "$R" ] || { echo "ABORT -- runner missing at $R"; exit 8; }
echo "=== FULL behavioral suite (direct invocation, generous budgets per the doctrine table)"
S=$(date +%s)
"./$R" --build-timeout 10800 --build-one-timeout 900 > "$T/drop-behavioral.log" 2>&1
echo "  exit $? after $(( $(date +%s) - S ))s"
tail -14 "$T/drop-behavioral.log" | sed 's/^/    /'
echo "=== BEHAVIORAL LEG COMPLETE"
