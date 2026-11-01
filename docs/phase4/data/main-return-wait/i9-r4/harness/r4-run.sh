#!/bin/bash
# r4-run.sh -- the trace arm's runs, from the ONE traced Native AOT binary of GoroutineRecoveringWhenMainReturns,
# through race4.sh: each round runs the traced label (GO2CS_MRW_TRACE=traces-<load>/trace-<i>.txt), then the untraced
# label (unset), so traced runs are the odd runs and untraced the even ones. hi (loadhi n8) first, N=200 (100 + 100),
# then n32 the same way. One arm at a time; WHEA after each; SPIN_S=14400. Writes run.log and DONE-R4-RUN.
P=/c/go2cs-build/logs/pado
R=$P/r4
log=$R/run.log
export SPIN_S=14400
EXE=$R/aot-rec-trace/GoroutineRecoveringWhenMainReturns.exe
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
say() { echo "$(date '+%F %T') $*" >> $log; }
count() { awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m {n++; if ($4==0) z++; if ($7==1) a++} END {printf "N=%d exit0=%d alive=%d", n, z, a}' $P/rows.tsv; }
recov() { grep -l recovered $P/work/$1-$2/out-*.txt 2>/dev/null | wc -l; }
arm() { # tag mode spin_n
  local tag=$1 mode=$2 sn=$3 t=$(now) s=$(date +%s)
  say "ARM $tag $mode n$sn START"
  TRACE_LABEL=m4-tr-$tag TRACE_DIR=$R/traces-$tag SPIN_N=$sn bash $P/race4.sh $mode 100 m4-tr-$tag=$EXE m4-un-$tag=$EXE > $R/arm-$tag.txt 2>&1
  say "ARM $tag $mode n$sn END elapsed=$(( $(date +%s)-s ))s traced:{$(count m4-tr-$tag $mode) recovered=$(recov m4-tr-$tag $mode) trace-files=$(ls $R/traces-$tag 2>/dev/null | wc -l)} untraced:{$(count m4-un-$tag $mode) recovered=$(recov m4-un-$tag $mode)} [$(whea "$t")]"
}
t0=$(now)
[ -f $EXE ] || { say "MISSING $EXE"; say "DONE-R4-RUN"; exit 2; }
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r1\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r4\affinity.tsv','C:\go2cs-build\logs\pado\r4\sampler.stop'; \$p.Id" > $R/sampler.pid
say "START r4 exe-sha=$(sha256sum $EXE | cut -c1-16)"
arm hi loadhi 8
arm n32 load 32
touch $R/sampler.stop
say "END r4 [$(whea "$t0")]"
say "DONE-R4-RUN"
