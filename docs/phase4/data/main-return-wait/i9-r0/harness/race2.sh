#!/bin/bash
# race.sh <exe> <label> <runs> <idle|load> -- run <exe> <runs> times under affinity 0xF; one TSV row per run:
# label, mode, run, exit code, first stderr line, first stdout line. "load" runs 8 spinning threads pinned to the
# same 4 CPUs for the whole row. Appends to rows.tsv; prints the tally.
exe=$1; label=$2; runs=$3; mode=$4
D=/c/go2cs-build/logs/pado
W=$D/work/$label-$mode; mkdir -p "$W"
winexe=$(cygpath -w "$exe")
spinpid=
SPIN_N=${SPIN_N:-8}; SPIN_CMD=aff.cmd; [ "$mode" = loadhi ] && SPIN_CMD=affhi.cmd
if [ "$mode" != idle ]; then
  GOMAXPROCS=$SPIN_N cmd //c "$(cygpath -w $D/$SPIN_CMD)" "$(cygpath -w $D/go/spin/spin.exe)" $SPIN_N 600 &
  spinpid=$!
  sleep 3
fi
for i in $(seq 1 "$runs"); do
  GOMAXPROCS=4 timeout 60 cmd //c "$(cygpath -w $D/aff.cmd)" "$winexe" >"$W/out-$i.txt" 2>"$W/err-$i.txt"
  rc=$?
  e1=$(head -1 "$W/err-$i.txt" | tr -d '\r')
  o1=$(head -1 "$W/out-$i.txt" | tr -d '\r')
  printf '%s\t%s\t%s\t%s\t%s\t%s\n' "$label" "$mode" "$i" "$rc" "$e1" "$o1" >> $D/rows.tsv
done
if [ -n "$spinpid" ]; then
  # the spinner is a Go program under cmd; stop it by its image path
  powershell -NoProfile -Command "Get-CimInstance Win32_Process -Filter \"Name='spin.exe'\" | Where-Object { \$_.ExecutablePath -like '*\\pado\\go\\spin\\spin.exe' } | ForEach-Object { Stop-Process -Id \$_.ProcessId -Force; \$_.ProcessId }"
  wait $spinpid 2>/dev/null
fi
awk -F'\t' -v l="$label" -v m="$mode" '$1==l && $2==m {k=$4" | "$5; c[k]++} END {for (k in c) print l, m, c[k], "x", k}' $D/rows.tsv
