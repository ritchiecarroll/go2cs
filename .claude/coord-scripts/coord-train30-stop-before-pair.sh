#!/usr/bin/env bash
# Stop train 30's running chain at the sweeps/pair boundary (doctrine 583: a red FULL suite makes the solo nistec pair
# and the reflect run measurements of a tree that is about to change). Polls the live merge log for the sweeps stamp,
# then kills the assemble bash process and any leg child still alive IN THE TRAIN WORKTREE, by PID, never by name.
set -u; S=/c/Projects/go2cs/.claude/coord-scripts; L="$S/coord-train30-merge-20260905-162609.log"
while ! grep -aqE '\] LEG sweeps ' "$L"; do sleep 45; done
echo "SWEEPS STAMP SEEN $(date +%H:%M)"; grep -aE '\] LEG sweeps ' "$L" | tail -1 | cut -c1-200
pids=$(powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*coord-train30-assemble.sh*' -and \$_.Name -eq 'bash.exe' } | ForEach-Object { \$_.ProcessId }" 2>/dev/null | tr -d '\r')
echo "assemble pid(s): ${pids:-none}"
for p in $pids; do powershell -NoProfile -Command "Stop-Process -Id $p -Force -ErrorAction SilentlyContinue" 2>/dev/null; echo "killed assemble $p"; done
sleep 5
kids=$(powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and (\$_.CommandLine -like '*nistec*' -or \$_.CommandLine -like '*reflect-run*' -or \$_.CommandLine -like '*coord-reflect*') } | ForEach-Object { \$_.ProcessId }" 2>/dev/null | tr -d '\r')
for p in $kids; do powershell -NoProfile -Command "Stop-Process -Id $p -Force -ErrorAction SilentlyContinue" 2>/dev/null; echo "killed leg child $p"; done
echo "[$(date '+%F %T')] CHAIN STOPPED BY COORD at the sweeps/pair boundary -- FULL suite was RED (3 projects); the nistec pair and reflect run are deferred to the re-battery after the rtlGetVersion fix lands (doctrine 583)" >> "$L"
echo "STOP DONE $(date +%H:%M); live in worktree now:"; powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner|bash|powershell' }).Count" 2>/dev/null
