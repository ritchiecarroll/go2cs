#!/bin/bash
# r1-run.sh -- main-return-wait R1 then R2 at G's tip (e592e782ca), through race3.sh (race2.sh with a spinner that
# outlasts the arm, and a per-run "spinner alive" column). One arm at a time; WHEA after each. Loads as in R0:
# idle, load n8 (8 spinners), load n32 (32 spinners), loadhi n8 (8 above-normal spinners), all on the same 4 CPUs.
#  R1: GoroutinePanicAfterDeferredDone (pad) at the tip; JIT N=100 per load (n32 N=400), AOT N=100 per load.
#  R2: GoroutineRecoveringWhenMainReturns (rec), OLD shape (971f018d7e) and NEW shape (the tip). Per load, one session
#      interleaves old JIT, old Go, new JIT, new Go (N=100 each) and old AOT, new AOT (N=50 each, the first 50 rounds).
# Writes run.log and a final DONE-R1-RUN line.
P=/c/go2cs-build/logs/pado
R=$P/r1
log=$R/run.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
count() { # label mode: N, exit0, alive
  awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m {n++; if ($4==0) z++; if ($7==1) a++} END {printf "N=%d exit0=%d alive=%d", n, z, a}' $P/rows.tsv; }
arm() { # name mode spin_n runs label=exe[:max]...
  local name=$1 mode=$2 sn=$3 runs=$4; shift 4
  local t=$(now) s=$(date +%s)
  echo "$(date '+%F %T') ARM $name $mode n$sn runs=$runs START" >> $log
  SPIN_N=$sn bash $P/race3.sh $mode $runs "$@" > $R/arm-$name-$mode-n$sn.txt 2>&1
  local c=""; for a in "$@"; do c="$c ${a%%=*}:{$(count ${a%%=*} $mode)}"; done
  echo "$(date '+%F %T') ARM $name $mode n$sn END elapsed=$(( $(date +%s)-s ))s$c [$(whea "$t")]" >> $log
}
for f in jit-tip-pad/$PAD.exe aot-tip-pad/$PAD.exe jit-tip-rec/$REC.exe aot-tip-rec/$REC.exe go-tip-rec/$REC.exe \
         jit-old-rec/$REC.exe aot-old-rec/$REC.exe go-old-rec/$REC.exe; do
  [ -f $R/$f ] || { echo "$(date '+%F %T') MISSING $f" >> $log; echo "DONE-R1-RUN" >> $log; exit 2; }
done
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r1\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r1\affinity.tsv','C:\go2cs-build\logs\pado\r1\sampler.stop'; \$p.Id" > $R/sampler.pid
t0=$(now)
echo "$(date '+%F %T') START r1 tip=e592e782ca old=971f018d7e sampler=$(tr -d '\r' < $R/sampler.pid)" >> $log
# R1
for row in "idle 8 100" "load 8 100" "load 32 400" "loadhi 8 100"; do
  set -- $row; arm r1-jit-pad $1 $2 $3 r1-jit-pad-n$2=$R/jit-tip-pad/$PAD.exe
done
for row in "idle 8" "load 8" "load 32" "loadhi 8"; do
  set -- $row; arm r1-aot-pad $1 $2 100 r1-aot-pad-n$2=$R/aot-tip-pad/$PAD.exe
done
# R2
for row in "idle 8" "load 8" "load 32" "loadhi 8"; do
  set -- $row; n=n$2
  arm r2-rec $1 $2 100 \
    r2-old-jit-$n=$R/jit-old-rec/$REC.exe r2-old-go-$n=$R/go-old-rec/$REC.exe \
    r2-new-jit-$n=$R/jit-tip-rec/$REC.exe r2-new-go-$n=$R/go-tip-rec/$REC.exe \
    r2-old-aot-$n=$R/aot-old-rec/$REC.exe:50 r2-new-aot-$n=$R/aot-tip-rec/$REC.exe:50
done
touch $R/sampler.stop
echo "$(date '+%F %T') END r1 [$(whea "$t0")]" >> $log
echo "DONE-R1-RUN" >> $log
