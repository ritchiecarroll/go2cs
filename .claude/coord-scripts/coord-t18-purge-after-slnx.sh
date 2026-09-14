#!/bin/bash
# Wait for train 18's slnx leg to end (the GolibTests leg's stamp), then purge the worktree's behavioral build output
# (the slnx leg's ~20 GB) so the sweeps leg clears the 25 GB disk floor. GolibTests/reflect-build use other trees; the
# runner leg rebuilds its two filtered projects and the shared deps.
T="/c/Projects/go2cs/.claude/coord-scripts/coord-train18-merge-20260903-081734.log"  # the CHAIN LOG, not the task output: a redirected chain leaves its task output EMPTY
stamp(){ echo "[$(date '+%F %T')] $*"; }
stamp "waiting for the slnx leg to end"
for i in $(seq 1 240); do grep -aq 'BATTERY: GolibTests' "$T" && break; sleep 30; done
grep -aq 'BATTERY: GolibTests' "$T" || { stamp "slnx leg did not end in 2h -- giving up"; exit 1; }
stamp "slnx leg ended; purging behavioral build output"
powershell -NoProfile -Command "\$n=0; Get-ChildItem -Path 'C:/Projects/go2cs/.claude/worktrees/musing-moser-d4552c/src/tests/Behavioral' -Include bin,obj,Generated -Directory -Recurse -ErrorAction SilentlyContinue | ForEach-Object { try { Remove-Item -LiteralPath \$_.FullName -Recurse -Force -ErrorAction Stop; \$n++ } catch {} }; 'removed=' + \$n; '{0:N1} GB free' -f ((Get-PSDrive C).Free/1GB)"
stamp "purge done"
