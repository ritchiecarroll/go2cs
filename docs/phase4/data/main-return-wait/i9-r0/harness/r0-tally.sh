#!/bin/bash
# r0-tally.sh -- one line per R0 arm (an -ext extension folded into its arm), from the saved per-run files:
# N, exit 0 / 2 / other, empty stderr, exit 2 whose stderr holds "panic: panic after deferred Done",
# runs whose stdout holds "recovered". Population check: the run files counted equal the rows.tsv rows.
P=/c/go2cs-build/logs/pado
printf 'arm\tmode\tN\texit0\texit2\tother\tempty_stderr\texit2_with_panic_line\tstdout_recovered\n'
awk -F'\t' '$1 ~ /^r0-/ {print $1"\t"$2"\t"$3"\t"$4}' $P/rows.tsv | sort -u |
while IFS=$'\t' read -r label mode run rc; do
  dir=$P/work/$label-$mode
  err=$dir/err-$run.txt; out=$dir/out-$run.txt
  es=0; [ -s "$err" ] || es=1
  pl=0; [ "$rc" = 2 ] && tr -d '\r' < "$err" | grep -qF 'panic: panic after deferred Done' && pl=1
  rv=0; tr -d '\r' < "$out" | grep -qF 'recovered' && rv=1
  printf '%s\t%s\t%s\t%s\t%s\t%s\n' "${label%-ext}" "$mode" "$rc" "$es" "$pl" "$rv"
done | awk -F'\t' '{k=$1"\t"$2; n[k]++; if ($3==0) z[k]++; else if ($3==2) t[k]++; else o[k]++; e[k]+=$4; p[k]+=$5; r[k]+=$6}
  END {for (k in n) printf "%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", k, n[k], z[k], t[k], o[k], e[k], p[k], r[k]}' | sort
echo "population: rows.tsv r0 rows=$(awk -F'\t' '$1 ~ /^r0-/' $P/rows.tsv | wc -l), distinct (label,mode,run)=$(awk -F'\t' '$1 ~ /^r0-/ {print $1, $2, $3}' $P/rows.tsv | sort -u | wc -l)"
