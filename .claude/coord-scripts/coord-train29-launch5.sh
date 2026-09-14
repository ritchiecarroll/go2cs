#!/bin/bash
# coord-train29-launch5.sh -- launch train 29's FIFTH chain after a clean rehearsal: gate on GUARD exit CODES, the worktree state, no live chain, the go pin.
S=/c/Projects/go2cs/.claude/coord-scripts; SC=$HOME/AppData/Local/Temp/claude/C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7/388822cf-bade-49e8-96a9-7ab378101ad4/scratchpad
R=$(ls -t $SC/coord-train29-rehearse-*.log | head -1); echo "rehearsal log: $R"
echo "guard exits: $(grep -aoE '[A-Z]+ GUARD exit=[0-9]+' "$R" | tr '\n' ' ')"
bad=$(grep -acE 'GUARD exit=[1-9]' "$R"); done_line=$(grep -ac 'REHEARSAL DONE' "$R"); echo "nonzero guard exits: $bad; REHEARSAL DONE: $done_line"
[ "$bad" = 0 ] && [ "$done_line" = 1 ] || { echo "REHEARSAL NOT CLEAN -- not launching"; exit 1; }
W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; cd $W || exit 2
[ "$(git rev-parse --short=9 HEAD)" = 9c44a6d6a ] && [ "$(git status --porcelain | wc -l)" = 0 ] || { echo "WORKTREE NOT READY: $(git rev-parse --short=9 HEAD) dirty $(git status --porcelain | wc -l)"; exit 1; }
chains=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*coord-train29-assemble.sh*' -and \$_.Name -eq 'bash.exe' -and \$_.CommandLine -notlike '*Get-CimInstance*' }).Count"); echo "live chains before launch: $chains"; [ "$chains" -le 1 ] || { echo "ANOTHER CHAIN ALIVE"; exit 1; }
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10'; export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED: $(go version)"; exit 1; }
echo "toolchain ok: $(go version | cut -d' ' -f3) dotnet $(dotnet --version); LAUNCHING chain 5 $(date +%H:%M:%S)"
CONTROL_SHA=9c44a6d6a bash $S/coord-train29-assemble.sh > $SC/coord-train29-assemble.log 2>&1; rc=$?
echo "ASSEMBLE CHAIN END exit=$rc $(date +%H:%M)"; tail -4 $SC/coord-train29-assemble.log | cut -c1-200; exit $rc
