#!/bin/bash
# race3.sh <idle|load|loadhi> <runs> <label>=<exe>[:<max>] ... -- race2.sh with two changes, for main-return-wait R1-R3:
#  1. the spinner runs SPIN_S seconds (default 5400) instead of 600, so it outlasts the arm (R0's tails outlasted the
#     600 s spinner); it is still stopped at the arm's end;
#  2. each row gets a 7th column: 1 if the spinner was still alive when the run ended (always 1 for idle), else 0.
# Several programs share one load session: for i in 1..runs, each label runs once, in the order given, if i <= its max
# (default runs). Otherwise as race2.sh: each run through aff.cmd (0xF), GOMAXPROCS=4, a 60 s cap; rows.tsv rows
# label, mode, run, exit code, first stderr line, first stdout line, alive; per-run files in work/<label>-<mode>/.
mode=$1; runs=$2; shift 2
D=/c/go2cs-build/logs/pado
SPIN_N=${SPIN_N:-8}; SPIN_S=${SPIN_S:-5400}; SPIN_CMD=aff.cmd; [ "$mode" = loadhi ] && SPIN_CMD=affhi.cmd
labels=(); exes=(); maxs=()
for a in "$@"; do
  l=${a%%=*}; r=${a#*=}; m=$runs
  case "$r" in *.exe:*) m=${r##*:}; r=${r%:*} ;; esac
  [ -f "$r" ] || { echo "missing exe for $l: $r"; exit 2; }
  labels+=("$l"); exes+=("$(cygpath -w "$r")"); maxs+=("$m"); mkdir -p "$D/work/$l-$mode"
done
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
    GOMAXPROCS=4 timeout 60 cmd //c "$(cygpath -w $D/aff.cmd)" "${exes[$k]}" >"$W/out-$i.txt" 2>"$W/err-$i.txt"
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
