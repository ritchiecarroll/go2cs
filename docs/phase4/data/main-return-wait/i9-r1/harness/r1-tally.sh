#!/bin/bash
# r1-tally.sh <runs.tsv> <tally.tsv> -- from the saved per-run files of every r1-/r2-/r3- row in rows.tsv:
#  runs.tsv:  arm, mode, run, exit, stderr bytes, stderr holds "panic: panic after deferred Done", stdout holds
#             "recovered", spinner alive at the run's end, first stderr line, first stdout line;
#  tally.tsv: per (arm, mode): N, exit 0 / 2 / other, empty stderr, exit 2 with the panic line, stdout "recovered",
#             spinner alive.
# Then a population line: rows.tsv rows against distinct (arm, mode, run) and the runs.tsv rows.
P=/c/go2cs-build/logs/pado
out=$1; tal=$2
printf 'arm\tmode\trun\texit\tstderr_bytes\tstderr_has_panic_line\tstdout_has_recovered\tspinner_alive\tstderr_first_line\tstdout_first_line\n' > $out
awk -F'\t' '$1 ~ /^r[123]-/ {print $1"\t"$2"\t"$3"\t"$4"\t"$7"\t"($5==""?"-":$5)"\t"($6==""?"-":$6)}' $P/rows.tsv |
while IFS=$'\t' read -r label mode run rc alive e1 o1; do
  dir=$P/work/$label-$mode
  err=$dir/err-$run.txt; o=$dir/out-$run.txt
  eb=$(stat -c %s "$err")
  pl=0; tr -d '\r' < "$err" | grep -qF 'panic: panic after deferred Done' && pl=1
  rv=0; tr -d '\r' < "$o" | grep -qF 'recovered' && rv=1
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "$label" "$mode" "$run" "$rc" "$eb" "$pl" "$rv" "$alive" "$e1" "$o1"
done >> $out
printf 'arm\tmode\tN\texit0\texit2\tother\tempty_stderr\texit2_with_panic_line\tstdout_recovered\tspinner_alive\n' > $tal
awk -F'\t' 'NR > 1 {k=$1"\t"$2; n[k]++; if ($4==0) z[k]++; else if ($4==2) t[k]++; else o[k]++; if ($5==0) e[k]++;
  if ($4==2 && $6==1) p[k]++; r[k]+=$7; a[k]+=$8}
  END {for (k in n) printf "%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", k, n[k], z[k], t[k], o[k], e[k], p[k], r[k], a[k]}' $out | sort >> $tal
echo "population: rows.tsv r1-r3 rows=$(awk -F'\t' '$1 ~ /^r[123]-/' $P/rows.tsv | wc -l), distinct (arm,mode,run)=$(awk -F'\t' '$1 ~ /^r[123]-/ {print $1, $2, $3}' $P/rows.tsv | sort -u | wc -l), runs.tsv rows=$(( $(wc -l < $out) - 1 ))"
