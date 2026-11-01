#!/bin/bash
# race4.sh <idle|load|loadhi> <runs> <label>=<exe>[:<max>] ... -- race3.sh plus one traced label, for the
# main-return-wait trace arm: when TRACE_LABEL names one of the labels, each of that label's runs gets
# GO2CS_MRW_TRACE=<TRACE_DIR>/trace-<run>.txt; every other run has it unset. Two labels on the same exe, traced first,
# alternate the runs: round i runs traced i, then untraced i. Rows, files and the spinner are as in race3.sh
# (SPIN_S default 5400; 7th column spinner_alive).
mode=$1; runs=$2; shift 2
unset GO2CS_MRW_TRACE
D=/c/go2cs-build/logs/pado
SPIN_N=${SPIN_N:-8}; SPIN_S=${SPIN_S:-5400}; SPIN_CMD=aff.cmd; [ "$mode" = loadhi ] && SPIN_CMD=affhi.cmd
labels=(); exes=(); maxs=()
for a in "$@"; do
  l=${a%%=*}; r=${a#*=}; m=$runs
  case "$r" in *.exe:*) m=${r##*:}; r=${r%:*} ;; esac
  [ -f "$r" ] || { echo "missing exe for $l: $r"; exit 2; }
  labels+=("$l"); exes+=("$(cygpath -w "$r")"); maxs+=("$m"); mkdir -p "$D/work/$l-$mode"
done
if [ -n "$TRACE_LABEL" ]; then
  [ -n "$TRACE_DIR" ] || { echo "TRACE_LABEL without TRACE_DIR"; exit 2; }
  mkdir -p "$TRACE_DIR"; TW=$(cygpath -w "$TRACE_DIR")
fi
spinpid=
if [ "$mode" != idle ]; then
  GOMAXPROCS=$SPIN_N cmd //c "$(cygpath -w $D/$SPIN_CMD)" "$(cygpath -w $D/go/spin/spin.exe)" $SPIN_N $SPIN_S &
  spinpid=$!
  sleep 3
fi
for i in $(seq 1 "$runs"); do
  for k in "${!labels[@]}"; do
    [ "$i" -le "${maxs[$k]}" ] || continue
    W=$D/work/${labels[$k]}-$mode
    if [ -n "$TRACE_LABEL" ] && [ "${labels[$k]}" = "$TRACE_LABEL" ]; then
      GO2CS_MRW_TRACE="$TW\\trace-$i.txt" GOMAXPROCS=4 timeout 60 cmd //c "$(cygpath -w $D/aff.cmd)" "${exes[$k]}" >"$W/out-$i.txt" 2>"$W/err-$i.txt"
    else
      GOMAXPROCS=4 timeout 60 cmd //c "$(cygpath -w $D/aff.cmd)" "${exes[$k]}" >"$W/out-$i.txt" 2>"$W/err-$i.txt"
    fi
    rc=$?
    alive=1; [ -n "$spinpid" ] && { kill -0 "$spinpid" 2>/dev/null || alive=0; }
    e1=$(head -1 "$W/err-$i.txt" | tr -d '\r')
    o1=$(head -1 "$W/out-$i.txt" | tr -d '\r')
    printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "${labels[$k]}" "$mode" "$i" "$rc" "$e1" "$o1" "$alive" >> $D/rows.tsv
  done
done
if [ -n "$spinpid" ]; then
  # the spinner is a Go program under cmd; stop it by its image path
  powershell -NoProfile -Command "Get-CimInstance Win32_Process -Filter \"Name='spin.exe'\" | Where-Object { \$_.ExecutablePath -like '*\\pado\\go\\spin\\spin.exe' } | ForEach-Object { Stop-Process -Id \$_.ProcessId -Force; \$_.ProcessId }"
  wait $spinpid 2>/dev/null
fi
for l in "${labels[@]}"; do
  awk -F'\t' -v l="$l" -v m="$mode" '$1==l && $2==m {k=$4" | "$5" | alive="$7; c[k]++} END {for (k in c) print l, m, c[k], "x", k}' $D/rows.tsv
done
