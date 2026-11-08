#!/usr/bin/env bash
# canarywalls.sh -- .3 re-bank plan input: the i9's canary per-row walls, parsed from each wave's report on the i9
# (S:\go2cs-build\logs\canary3{,w2,w3,w4}\msg-report.txt). Three report formats: wave 1 "row  N = N  PASS  W s  D";
# waves 2-3 "row N=N W s D" and wave 4 "row N Ws D", both " | "-separated. Prints "row<TAB>wave<TAB>banked<TAB>wall_s".
L=/s/go2cs-build/logs
grep -E '^\s+[a-z][a-z0-9/._-]*\s+[0-9]+\s*=\s*[0-9]+\s+PASS\s+[0-9]+ s' $L/canary3/msg-report.txt \
  | awk '{printf "%s\tw1\t%s\t%s\n", $1, $2, $6}'
for w in 2 3; do
  awk '/^PER ROW/{f=1;next} f && /^[^ ]/{f=0} f' $L/canary3w$w/msg-report.txt | tr '|' '\n' \
    | sed -nE 's/^\s*([a-z][a-z0-9/._-]*)\s+([0-9]+)=[0-9]+\s+([0-9]+) s.*/\1\tw'$w'\t\2\t\3/p'
done
awk '/^PER ROW/{f=1;next} f && /^[-A-Z]/{f=0} f' $L/canary3w4/msg-report.txt | tr '|' '\n' \
  | sed -nE 's/^\s*([a-z][a-z0-9/._-]*)\s+([0-9]+)\s+([0-9]+)s\s.*/\1\tw4\t\2\t\3/p; s/^\s*([a-z][a-z0-9/._-]*)\s+([0-9]+)\s+([0-9]+)s$/\1\tw4\t\2\t\3/p'
