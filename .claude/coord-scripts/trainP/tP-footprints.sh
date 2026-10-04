#!/usr/bin/env bash
# tP-footprints.sh <BASE> <rows> <outdir> -- each row's OWN footprint and the writers of every path
# (DRAFT 2026-10-04, UNCOMMITTED). Reads git objects only.
#   own = merge-base(row, P)..row, P = the nearest OTHER row that is an ancestor of the row (its stack), else BASE.
# Writes <outdir>/own-footprints.tsv (row, sha, own base, stack, best common ancestors, commits, merges, files,
# numstat, renames, deletions), <outdir>/own/<row>.txt, <outdir>/writers.tsv (path, writers, names: paths two or more
# rows write) and <outdir>/shared-hunk-positions.log (each writer's '@@' positions in every shared path that is not a
# registration file or a package_info.cs).
set -u
export MSYS_NO_PATHCONV=1
GD=${GD:-H:/Projects/go2cs/.git}
g() { git --git-dir="$GD" -c core.quotepath=false "$@"; }
B=$(g rev-parse "$1^{commit}") || exit 2; rows=$2; out=$3; mkdir -p "$out/own"
N=(); S=()
while IFS= read -r line; do
  line=${line%$'\r'}
  case "$line" in ''|'#'*) continue;; esac
  if [[ "$line" == *'|'* ]]; then n=${line%%|*}; r=${line#*|}; s=${r%%|*}; else read -r n s _ <<<"$line"; fi
  N+=("$n"); S+=("$(g rev-parse "$s^{commit}")")
done < "$rows"
: > "$out/own-footprints.tsv"; : > "$out/all-own.txt"
declare -A MB
for i in "${!N[@]}"; do
  P=$B; pn=BASE
  for j in "${!N[@]}"; do [ "$j" != "$i" ] || continue
    if g merge-base --is-ancestor "${S[$j]}" "${S[$i]}"; then
      # the NEAREST stack: an ancestor row that descends from the stack chosen so far
      if [ "$pn" = BASE ] || g merge-base --is-ancestor "$P" "${S[$j]}"; then P=${S[$j]}; pn=${N[$j]}; fi
    fi
  done
  mb=$(g merge-base "${S[$i]}" "$P"); nb=$(g merge-base --all "${S[$i]}" "$P" | grep -c .); MB[${N[$i]}]=$mb
  g diff --name-only "$mb" "${S[$i]}" | LC_ALL=C sort > "$out/own/${N[$i]}.txt"
  nf=$(grep -c . "$out/own/${N[$i]}.txt")
  ns=$(g diff --numstat "$mb" "${S[$i]}" | awk '{a+=$1; d+=$2} END {printf "+%d/-%d", a, d}')
  ren=$(g diff -M --diff-filter=R --name-status "$mb" "${S[$i]}" | grep -c .)
  del=$(g diff -M --diff-filter=D --name-only "$mb" "${S[$i]}" | grep -c .)
  nc=$(g rev-list --count "$mb..${S[$i]}"); nm=$(g rev-list --count --merges "$mb..${S[$i]}")
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "${N[$i]}" "${S[$i]:0:10}" "${mb:0:10}" "$pn" "$nb" "$nc" "$nm" "$nf" "$ns" "$ren" "$del" >> "$out/own-footprints.tsv"
  sed "s|\$|\t${N[$i]}|" "$out/own/${N[$i]}.txt" >> "$out/all-own.txt"
done
LC_ALL=C sort "$out/all-own.txt" | awk -F'\t' '{ if ($1==p) { w=w" "$2; n++ } else { if (n>1) print p"\t"n"\t"w; p=$1; w=$2; n=1 } } END { if (n>1) print p"\t"n"\t"w }' > "$out/writers.tsv"
rm -f "$out/all-own.txt"
{
  while IFS=$'\t' read -r p k ws; do
    case "$p" in src/go2cs.slnx|src/go2cs/go2cs-src.projitems|src/tests/Behavioral/BehavioralTests/*|*/package_info.cs) continue;; esac
    echo "== $p ($k writers)"
    for w in $ws; do
      for i in "${!N[@]}"; do [ "${N[$i]}" = "$w" ] && sh=${S[$i]}; done
      echo "   $w (${sh:0:10} vs ${MB[$w]:0:10}): $(g diff --numstat "${MB[$w]}" "$sh" -- "$p" | awk '{print "+"$1"/-"$2}')"
      g diff -U0 "${MB[$w]}" "$sh" -- "$p" | grep '^@@' | sed -E 's/^@@ -([0-9,]+) \+([0-9,]+) @@ ?(.*)$/      old \1 -> new \2   \3/' | cut -c1-170
    done
  done < "$out/writers.tsv"
} > "$out/shared-hunk-positions.log"
echo "FOOTPRINTS rows=${#N[@]} shared-paths=$(grep -c . "$out/writers.tsv") stacked=$(awk -F'\t' '$4!="BASE"' "$out/own-footprints.tsv" | grep -c .) multi-base=$(awk -F'\t' '$5>1' "$out/own-footprints.tsv" | grep -c .) with-renames=$(awk -F'\t' '$10>0' "$out/own-footprints.tsv" | grep -c .)"
