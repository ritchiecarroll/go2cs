#!/usr/bin/env bash
# reconcile.sh -- the row-for-row join behind the design record's forwarded-row figure.
#
#   reconcile.sh <census output> <sim output> [b]
#
# The census side is cmd/census's method rows, filtered to the rows the cut forwards:
#   exported, not `genencl` (generic metadata embed), not `internalfwd` (C4: through a pre-existing internal
#   same-package forwarder), and -- under the literal (i') rule only -- not `collide`.
# Pass `b` as the third argument for refinement (b), which keeps the `collide` rows (pair it with `sim -b`).
# The sim side is cmd/sim's "added" section. Both sides are reduced to ONE label:
#   package <TAB> type <TAB> declaration file:line <TAB> method
# and compared both ways. Exit status 0 iff the two sets are equal. A mismatch is a STOP, not a reconciliation.
# NOTE: this is a CROSS-CHECK, not an independent derivation -- cmd/census and cmd/sim share treeOccurrences (and
# go/types itself).
set -u
census=${1:?census output}
sim=${2:?sim output}
mode=${3:-literal}
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

sed -n '/^## methods/,/^## drops/p' "$census" | grep -v '^##' |
  awk -F'\t' -v mode="$mode" 'NF > 5 {
      flags = $10; decl = $11; sub(/^decl=/, "", decl)
      if (flags ~ /unexported|genencl|internalfwd/) next
      if (mode != "b" && flags ~ /collide/) next
      method = $6; sub(/.*\./, "", method)
      print $1 "\t" $4 "\t" decl "\t" method
  }' | sort -u > "$work/census.txt"

sed -n '/^## added/,/^## cut OVER/p' "$sim" | grep -v '^##' |
  awk -F'\t' 'NF >= 5 { print $1 "\t" $3 "\t" $4 "\t" $5 }' | sort -u > "$work/sim.txt"

nc=$(wc -l < "$work/census.txt"); ns=$(wc -l < "$work/sim.txt")
co=$(comm -23 "$work/census.txt" "$work/sim.txt" | wc -l); so=$(comm -13 "$work/census.txt" "$work/sim.txt" | wc -l)
echo "census rows $nc; sim added rows $ns; census-only $co; sim-only $so (mode $mode)"
comm -23 "$work/census.txt" "$work/sim.txt" | sed 's/^/census-only: /'
comm -13 "$work/census.txt" "$work/sim.txt" | sed 's/^/sim-only: /'
[ "$co" = 0 ] && [ "$so" = 0 ]
