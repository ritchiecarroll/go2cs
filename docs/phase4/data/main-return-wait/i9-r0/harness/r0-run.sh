#!/bin/bash
# r0-run.sh -- main-return-wait R0 (the BEFORE arm) at 2e619b9966, through the pado harness (race2.sh) UNCHANGED.
# Arms: 2 tests x {JIT N=100, AOT N=50} x {idle, load (8 spinners), n32 (32 spinners), hi (8 above-normal)}.
# A loaded GoroutinePanicAfterDeferredDone arm with 0 exit-0 runs gets 400 - N more runs (label suffix -ext), so N=400 in
# total for JIT and AOT alike (the extension is 400 - N runs). One arm at a time; WHEA after each. Writes run.log and a final DONE-R0-RUN line.
P=/c/go2cs-build/logs/pado
R=$P/r0
log=$R/run.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
exit0() { awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m && $4==0 {c++} END {print c+0}' $P/rows.tsv; }
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r0\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r0\affinity.tsv','C:\go2cs-build\logs\pado\r0\sampler.stop'; \$p.Id" > $R/sampler.pid
t0=$(now)
echo "$(date '+%F %T') START r0 base=2e619b9966 sampler=$(tr -d '\r' < $R/sampler.pid)" >> $log
for p in GoroutinePanicAfterDeferredDone GoroutineRecoveringWhenMainReturns; do
  short=pad; [ $p = GoroutineRecoveringWhenMainReturns ] && short=rec
  for kind in jit aot; do
    n=100; [ $kind = aot ] && n=50
    exe=$R/$kind-$p/$p.exe
    [ -f "$exe" ] || { echo "$(date '+%F %T') MISSING $exe" >> $log; continue; }
    for row in "idle 8" "load 8" "load 32" "loadhi 8"; do
      set -- $row; mode=$1; sn=$2
      label=r0-$kind-$short-n$sn
      t=$(now); s=$(date +%s)
      echo "$(date '+%F %T') ARM $label $mode N=$n START" >> $log
      SPIN_N=$sn bash $P/race2.sh "$exe" $label $n $mode > $R/arm-$label-$mode.txt 2>&1
      e0=$(exit0 $label $mode)
      echo "$(date '+%F %T') ARM $label $mode N=$n END exit0=$e0 elapsed=$(( $(date +%s)-s ))s [$(whea "$t")]" >> $log
      if [ $short = pad ] && [ $mode != idle ] && [ "$e0" = 0 ]; then
        t=$(now); s=$(date +%s)
        echo "$(date '+%F %T') ARM $label-ext $mode N=$((400-n)) START (0 of $n exit 0)" >> $log
        SPIN_N=$sn bash $P/race2.sh "$exe" $label-ext $((400-n)) $mode > $R/arm-$label-ext-$mode.txt 2>&1
        echo "$(date '+%F %T') ARM $label-ext $mode N=$((400-n)) END exit0=$(exit0 $label-ext $mode) elapsed=$(( $(date +%s)-s ))s [$(whea "$t")]" >> $log
      fi
    done
  done
done
touch $R/sampler.stop
echo "$(date '+%F %T') END r0 [$(whea "$t0")]" >> $log
echo "DONE-R0-RUN" >> $log
