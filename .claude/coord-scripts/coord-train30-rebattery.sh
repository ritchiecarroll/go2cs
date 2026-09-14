#!/usr/bin/env bash
# coord-train30-rebattery.sh -- re-run train 30's battery on the EXISTING assembly head plus its assembly commits (the rtlGetVersion hand-own merge and the
# CompositeLiteralElements golden re-baseline), without re-merging the sixteen seats. Gates: no live chain in the worktree, clean tree, HEAD descends from 75758cf06
# and is NOT 75758cf06 itself (the assembly commits must be present). Runs the PATCHED copy of the assemble script (doctrine 583) with SKIP_ASSEMBLE=1.
set -u; S=/c/Projects/go2cs/.claude/coord-scripts; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; SC="$S"
cd "$W" || exit 1
# NOTE: the age filter is load-bearing -- a wildcard over a COMMAND LINE matches any shell whose own text carries the pattern,
# including the one performing the check (measured 2026-09-05: three self-matches, all age 0m, and concatenating the pattern
# does not help because the substrings remain in the command line). A real chain is minutes old; the querying shell is seconds old.
chains=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*coord-train30-*assemble.sh*' -and \$_.Name -eq 'bash.exe' -and \$_.CreationDate -lt (Get-Date).AddSeconds(-45) }).Count" 2>/dev/null | tr -d '\r '); [ "${chains:-0}" = "0" ] || { echo "a train-30 chain is LIVE ($chains) -- not relaunching"; exit 1; }
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner|powershell' }).Count" 2>/dev/null | tr -d '\r '); [ "${live:-0}" = "0" ] || { echo "live processes in the train worktree ($live) -- not relaunching"; exit 1; }
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not relaunching"; exit 1; }
H=$(git rev-parse --short=9 HEAD); git merge-base --is-ancestor 75758cf06 HEAD || { echo "HEAD $H does not descend from the assembly head 75758cf06"; exit 1; }
[ "$H" != "75758cf06" ] || { echo "HEAD is the bare assembly head -- the assembly commits are missing"; exit 1; }
git log --format='%s' 75758cf06..HEAD | grep -q 'rtlGetVersion' || { echo "the rtlGetVersion assembly commit is not on HEAD"; exit 1; }
echo "REBATTERY from $H: $(git log --oneline 75758cf06..HEAD | wc -l) assembly commit(s) on 75758cf06"; git log --format='  %h %s' 75758cf06..HEAD | cut -c1-140
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10'; export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"; export MSBUILDDISABLENODEREUSE=1
go version | grep -q go1.23.12 || { echo "toolchain pin FAILED: $(go version)"; exit 1; }
SKIP_ASSEMBLE=1 NEED_UTT=0 CONTROL_SHA=b91684991 bash $S/coord-train30-rebattery-assemble.sh > $SC/coord-train30-rebattery.log 2>&1; rc=$?
echo "REBATTERY CHAIN END exit=$rc $(date +%H:%M)"; tail -4 $SC/coord-train30-rebattery.log | cut -c1-200; exit $rc
