#!/usr/bin/env bash
# mrw-run.sh -- the main-return-wait linux leg's runs (brief: leg-briefs.md at bbf1725aa7). Every run is
# `GOMAXPROCS=4 timeout 60 taskset -c 0-3 <exe>`; the loaded arms add 32 busy loops (a Go spinner, GOMAXPROCS=32)
# pinned to the same 4 CPUs, started 3 s before the arm and stopped at its end; `alive` records the spinner at each
# run's end (1 when idle). One arm at a time, N=100. Rows: arm, mode, run, exit, stderr bytes, stderr has the panic
# line, stdout has "recovered", spinner alive, first stderr line. Then a per-arm tally.
set -u
R=/root/h10-mrw
GOROOT=/root/sdk/go1.26.10
N=${N:-100}
LOG="$R/run.log"; ROWS="$R/rows.tsv"; TALLY="$R/tally.tsv"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$LOG"; }
abort(){ stamp "ABORT: $*"; echo "MRW-RUN-DONE rc=2" >> "$LOG"; exit 2; }
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 PATH="$GOROOT/bin:/root/dotnet10:$PATH" DOTNET_ROOT=/root/dotnet10
unset DOTNET_TieredCompilation
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
grep -q 'MRW-AOT-DONE rc=0' "$R/build.log" || abort "the builds have not finished clean"
[ "$(nproc)" -ge 4 ] || abort "fewer than 4 CPUs"

mkdir -p /root/mrw/spin
cat > /root/mrw/spin/main.go <<'X'
// spin <n> <seconds>: n goroutines busy-looping for the given time (the leg's "busy loops").
package main

import (
	"os"
	"strconv"
	"time"
)

func main() {
	n, _ := strconv.Atoi(os.Args[1])
	s, _ := strconv.Atoi(os.Args[2])
	for i := 0; i < n; i++ {
		go func() {
			x := 0
			for {
				x++
			}
		}()
	}
	time.Sleep(time.Duration(s) * time.Second)
}
X
( cd /root/mrw/spin && [ -f go.mod ] || go mod init spin > /dev/null 2>&1; cd /root/mrw/spin && go build -o spin . ) || abort "spinner build"

printf 'arm\tmode\trun\texit\tstderr_bytes\tpanic_line\trecovered\talive\tstderr_first\n' > "$ROWS"
arm(){ # label exe mode
  local label=$1 exe=$2 mode=$3 W=$R/work/$1-$3 spin= s=$SECONDS
  mkdir -p "$W"
  [ -x "$exe" ] || abort "missing $exe"
  if [ "$mode" = load ]; then
    GOMAXPROCS=32 taskset -c 0-3 /root/mrw/spin/spin 32 5400 &
    spin=$!
    sleep 3
  fi
  for i in $(seq 1 "$N"); do
    GOMAXPROCS=4 timeout 60 taskset -c 0-3 "$exe" > "$W/out-$i.txt" 2> "$W/err-$i.txt"
    rc=$?
    alive=1; [ -n "$spin" ] && { kill -0 "$spin" 2>/dev/null && alive=1 || alive=0; }
    pl=0; grep -qF 'panic: panic after deferred Done' "$W/err-$i.txt" && pl=1
    rv=0; grep -qF 'recovered' "$W/out-$i.txt" && rv=1
    printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "$label" "$mode" "$i" "$rc" "$(stat -c %s "$W/err-$i.txt")" "$pl" "$rv" "$alive" "$(head -1 "$W/err-$i.txt" | tr -d '\r\t')" >> "$ROWS"
  done
  if [ -n "$spin" ]; then kill "$spin" 2>/dev/null; wait "$spin" 2>/dev/null; fi
  stamp "ARM $label $mode N=$N $((SECONDS-s))s $(awk -F'\t' -v l="$label" -v m="$mode" '$1==l && $2==m {n++; if ($4==0) z++; if ($4==2) t++; if ($5==0) e++; if ($4==2 && $6==1) p++; r+=$7; a+=$8} END {printf "exit0=%d exit2=%d empty_stderr=%d exit2_panic=%d recovered=%d alive=%d/%d", z, t, e, p, r, a, n}' "$ROWS")"
}

stamp "START N=$N nproc=$(nproc) loadavg=$(cut -d' ' -f1-3 /proc/loadavg)"
for mode in idle load; do
  arm r0-jit "$R/bin/jit-base-pad/$PAD" $mode
  arm r0-aot "$R/bin/aot-base-pad/$PAD" $mode
  arm r1-jit "$R/bin/jit-tip-pad/$PAD" $mode
  arm r1-aot "$R/bin/aot-tip-pad/$PAD" $mode
  arm rec-jit "$R/bin/jit-tip-rec/$REC" $mode
  arm rec-aot "$R/bin/aot-tip-rec/$REC" $mode
  arm rec-go "$R/bin/go-jit-tip-rec/$REC" $mode
  arm pad-go "$R/bin/go-jit-tip-pad/$PAD" $mode
done
printf 'arm\tmode\tN\texit0\texit2\tother\tempty_stderr\texit2_with_panic_line\tstdout_recovered\tspinner_alive\n' > "$TALLY"
awk -F'\t' 'NR > 1 {k=$1"\t"$2; n[k]++; if ($4==0) z[k]++; else if ($4==2) t[k]++; else o[k]++; if ($5==0) e[k]++; if ($4==2 && $6==1) p[k]++; r[k]+=$7; a[k]+=$8}
  END {for (k in n) printf "%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", k, n[k], z[k], t[k], o[k], e[k], p[k], r[k], a[k]}' "$ROWS" | sort >> "$TALLY"
stamp "rows=$(($(wc -l < "$ROWS")-1))"
echo "MRW-RUN-DONE rc=0" >> "$LOG"
