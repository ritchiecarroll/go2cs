#!/bin/bash
# r4-build.sh -- the trace arm's one build: a Native AOT publish of GoroutineRecoveringWhenMainReturns from the
# trace-patched checkout (r1-aot.cmd's recipe). Writes build.log with rc, elapsed and WHEA, then DONE-R4-BUILD.
R=/c/go2cs-build/logs/pado/r4
log=$R/build.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
t=$(powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'); s=$(date +%s)
echo "$(date '+%F %T') START aot-rec-trace tree=$(git -C D:/go2cs-build/i9-mrw4-tr rev-parse --short=10 HEAD) trace-lines=$(git -C D:/go2cs-build/i9-mrw4-tr diff | grep -c '^+.*// TRACE')" >> $log
cmd //c 'C:\go2cs-build\logs\pado\r1\r1-aot.cmd' 'D:\go2cs-build\i9-mrw4-tr\src\tests\Behavioral\GoroutineRecoveringWhenMainReturns' GoroutineRecoveringWhenMainReturns 'C:\go2cs-build\logs\pado\r4\aot-rec-trace' 'D:/go2cs-build/i9-mrw4-tr/src/' > $R/build-aot-rec-trace.txt 2>&1
rc=$?
echo "$(date '+%F %T') END aot-rec-trace rc=$rc elapsed=$(( $(date +%s)-s ))s exe-sha=$(sha256sum $R/aot-rec-trace/GoroutineRecoveringWhenMainReturns.exe 2>/dev/null | cut -c1-16) [$(whea "$t")]" >> $log
echo "DONE-R4-BUILD" >> $log
