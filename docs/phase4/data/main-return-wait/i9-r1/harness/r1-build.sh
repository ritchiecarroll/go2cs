#!/bin/bash
# r1-build.sh -- the main-return-wait R1/R2 builds, one heavy task at a time: the behavioral runner (JIT + Go) on the
# tip for both tests and on 971f018d7e (the OLD shape) for the recovering test; the exe folders are copied out, so
# R3's red rebuilds cannot reach them; then one Native AOT publish per (tree, test). Writes build.log with rc,
# elapsed seconds and WHEA per step, and a final DONE-R1-BUILD line.
P=/c/go2cs-build/logs/pado
R=$P/r1
log=$R/build.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
step() { # name, command...
  local name=$1; shift; local t=$(now) s=$(date +%s)
  echo "$(date '+%F %T') START $name" >> $log
  "$@" > $R/build-$name.txt 2>&1; local rc=$?
  echo "$(date '+%F %T') END $name rc=$rc elapsed=$(( $(date +%s)-s ))s [$(whea "$t")]" >> $log
}
t0=$(now)
step runner-tip-pad cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' 'D:\go2cs-build\i9-mrw-tip' --filter $PAD
step runner-tip-rec cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' 'D:\go2cs-build\i9-mrw-tip' --filter $REC
step runner-old-rec cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' 'D:\go2cs-build\i9-mrw-old' --filter $REC
for x in "tip $PAD pad" "tip $REC rec" "old $REC rec"; do
  set -- $x; B=D:/go2cs-build/i9-mrw-$1/src/tests/Behavioral/$2/bin/Release
  rm -rf $R/jit-$1-$3 $R/go-$1-$3
  cp -r $B/net10.0 $R/jit-$1-$3 && mkdir -p $R/go-$1-$3 && cp $B/Go/$2.exe $R/go-$1-$3/
  echo "$(date '+%F %T') COPY $1 $3 rc=$? jit=$(sha256sum $R/jit-$1-$3/$2.dll | cut -c1-16) golib=$(sha256sum $R/jit-$1-$3/golib.dll | cut -c1-16) go=$(sha256sum $R/go-$1-$3/$2.exe | cut -c1-16)" >> $log
done
for x in "tip $PAD pad" "tip $REC rec" "old $REC rec"; do
  set -- $x
  step aot-$1-$3 cmd //c 'C:\go2cs-build\logs\pado\r1\r1-aot.cmd' "D:\\go2cs-build\\i9-mrw-$1\\src\\tests\\Behavioral\\$2" $2 "C:\\go2cs-build\\logs\\pado\\r1\\aot-$1-$3" "D:/go2cs-build/i9-mrw-$1/src/"
  echo "$(date '+%F %T') AOT $1 $3 exe=$(ls $R/aot-$1-$3/$2.exe 2>&1) sha=$(sha256sum $R/aot-$1-$3/$2.exe 2>/dev/null | cut -c1-16)" >> $log
done
echo "$(date '+%F %T') END r1-build [$(whea "$t0")]" >> $log
echo "DONE-R1-BUILD" >> $log
