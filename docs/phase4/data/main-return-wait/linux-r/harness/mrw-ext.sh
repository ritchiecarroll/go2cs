#!/usr/bin/env bash
# mrw-ext.sh -- the linux leg's STATED EXTENSION, after the brief's arms all read 0 (R0 JIT included, which the i9 lost
# at about 1/100, so N=100 at this load has little power): (a) R0 and R1 JIT loaded, 300 more runs each (mode load2;
# N=400 with the first 100), and (b) a "hi" load, the 32 busy loops at nice -10 (above the test's priority; the linux
# analogue of the i9's above-normal spinners), N=100 for R0 JIT, R1 JIT and the recovering probe JIT and AOT. Same run
# shape as mrw-run.sh: GOMAXPROCS=4 timeout 60 taskset -c 0-3, spinners pinned to 0-3, spinner alive per run.
set -u
R=/root/h10-mrw
LOG="$R/run.log"; ROWS="$R/rows.tsv"; TALLY="$R/tally.tsv"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$LOG"; }
abort(){ stamp "ABORT: $*"; echo "MRW-EXT-DONE rc=2" >> "$LOG"; exit 2; }
export PATH="/root/dotnet10:$PATH" DOTNET_ROOT=/root/dotnet10
unset DOTNET_TieredCompilation
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
grep -q 'MRW-RUN-DONE rc=0' "$LOG" || abort "the brief's run has not finished clean"
[ -x /root/mrw/spin/spin ] || abort "no spinner"

arm(){ # label exe mode runs
  local label=$1 exe=$2 mode=$3 runs=$4 W=$R/work/$1-$3 spin= s=$SECONDS
  mkdir -p "$W"
  [ -x "$exe" ] || abort "missing $exe"
  case "$mode" in
    load2) GOMAXPROCS=32 taskset -c 0-3 /root/mrw/spin/spin 32 5400 & spin=$! ;;
    hi) GOMAXPROCS=32 nice -n -10 taskset -c 0-3 /root/mrw/spin/spin 32 5400 & spin=$! ;;
  esac
  sleep 3
  [ "$mode" = hi ] && stamp "hi spinner pid=$spin nice=$(ps -o ni= -p "$spin" | tr -d ' ')"
  for i in $(seq 1 "$runs"); do
    GOMAXPROCS=4 timeout 60 taskset -c 0-3 "$exe" > "$W/out-$i.txt" 2> "$W/err-$i.txt"
    rc=$?
    alive=0; kill -0 "$spin" 2>/dev/null && alive=1
    pl=0; grep -qF 'panic: panic after deferred Done' "$W/err-$i.txt" && pl=1
    rv=0; grep -qF 'recovered' "$W/out-$i.txt" && rv=1
    printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "$label" "$mode" "$i" "$rc" "$(stat -c %s "$W/err-$i.txt")" "$pl" "$rv" "$alive" "$(head -1 "$W/err-$i.txt" | tr -d '\r\t')" >> "$ROWS"
  done
  kill "$spin" 2>/dev/null; wait "$spin" 2>/dev/null
  stamp "ARM $label $mode N=$runs $((SECONDS-s))s $(awk -F'\t' -v l="$label" -v m="$mode" '$1==l && $2==m {n++; if ($4==0) z++; if ($4==2) t++; if ($5==0) e++; if ($4==2 && $6==1) p++; r+=$7; a+=$8} END {printf "exit0=%d exit2=%d empty_stderr=%d exit2_panic=%d recovered=%d alive=%d/%d", z, t, e, p, r, a, n}' "$ROWS")"
}

stamp "EXT START loadavg=$(cut -d' ' -f1-3 /proc/loadavg)"
arm r0-jit "$R/bin/jit-base-pad/$PAD" load2 300
arm r1-jit "$R/bin/jit-tip-pad/$PAD" load2 300
arm r0-jit "$R/bin/jit-base-pad/$PAD" hi 100
arm r1-jit "$R/bin/jit-tip-pad/$PAD" hi 100
arm rec-jit "$R/bin/jit-tip-rec/$REC" hi 100
arm rec-aot "$R/bin/aot-tip-rec/$REC" hi 100
printf 'arm\tmode\tN\texit0\texit2\tother\tempty_stderr\texit2_with_panic_line\tstdout_recovered\tspinner_alive\n' > "$TALLY"
awk -F'\t' 'NR > 1 {k=$1"\t"$2; n[k]++; if ($4==0) z[k]++; else if ($4==2) t[k]++; else o[k]++; if ($5==0) e[k]++; if ($4==2 && $6==1) p[k]++; r[k]+=$7; a[k]+=$8}
  END {for (k in n) printf "%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", k, n[k], z[k], t[k], o[k], e[k], p[k], r[k], a[k]}' "$ROWS" | sort >> "$TALLY"
stamp "rows=$(($(wc -l < "$ROWS")-1))"
echo "MRW-EXT-DONE rc=0" >> "$LOG"
