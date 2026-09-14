#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
HEAD=${TREE:?}
FIX=${FIX:-}
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export MSBUILDDISABLENODEREUSE=1
V=$(go version 2>&1); case "$V" in *go1.23.12*) echo "PIN OK: $V";; *) echo "ABORT pin: $V"; exit 9;; esac
cd "$W" || exit 8
[ -n "$(git status --porcelain)" ] && { echo "ABORT dirty"; exit 8; }
ORIG=$(git rev-parse --abbrev-ref HEAD); [ "$ORIG" = HEAD ] && ORIG=$(git rev-parse HEAD)
echo "=== detaching at $HEAD, cherry-picking $FIX (the refusal)"
git checkout -q --detach "$HEAD" || exit 8
if [ -n "$FIX" ]; then git -c user.name=coord -c user.email=c@x cherry-pick "$FIX" </dev/null >/dev/null 2>&1 || { echo "ABORT cherry-pick"; git cherry-pick --abort 2>/dev/null; git checkout -q "$ORIG"; exit 7; }; fi
echo "  tree = $(git log -1 --format=%h)"
if git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null; then echo "  ANCESTRY: token seat IS PRESENT"; else echo "  ANCESTRY: token seat absent"; fi
git diff --stat HEAD~1 | tail -2 | sed 's/^/  /'
echo "=== go2cs.slnx Debug --no-incremental (the leg C2 cannot run)"
S=$(date +%s)
dotnet build src/go2cs.slnx -c Debug --no-incremental -m -p:UseSharedCompilation=false \
  > $HOME/AppData/Local/Temp/claude/coord-slnx-leg.log 2>&1
RC=$?
E=$(( $(date +%s) - S ))
echo "  exit $RC after ${E}s"
echo "  errors (strict pattern): $(grep -cE 'error (CS|MSB|NETSDK)[0-9]+' $HOME/AppData/Local/Temp/claude/coord-slnx-leg.log)"
echo "  warnings: $(grep -cE 'warning (CS|MSB)[0-9]+' $HOME/AppData/Local/Temp/claude/coord-slnx-leg.log)"
grep -E 'error (CS|MSB|NETSDK)[0-9]+' $HOME/AppData/Local/Temp/claude/coord-slnx-leg.log | head -8 | sed 's/^/    /'
tail -3 $HOME/AppData/Local/Temp/claude/coord-slnx-leg.log | sed 's/^/  /'
echo "=== restoring"
git checkout -q "$ORIG"; git clean -qfd src/core 2>/dev/null
echo "  back at $(git log -1 --format=%h), dirty=$(git status --porcelain | wc -l)"
