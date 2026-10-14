#!/usr/bin/env bash
# tT4-plantreal-test.sh -- kit change T4-19's red-first proof (the Go 1.26 hop's H0, F-H0-2; ruled COORD 2026-10-14).
# A fixture in the emission check's own shapes: a seed copy of PLANT_CS with the plant block appended as
# tT4-emitcheck.sh appends it, an emitted copy, and a drift list naming the path. Four cases:
#   pure          the emission is the committed file      -> OLD drops the path; NEW drops it      (both 0)
#   real          the emission adds a REAL comment line   -> OLD drops it (RED: real drift hidden); NEW keeps it (1)
#   real-crlf     as real, the emission written CRLF      -> NEW keeps it (CR stripped on both sides)
#   unplanted     the seed carries no plant block         -> tT4-plantreal.py refuses (rc 2), never a guess
# Self-contained: run from anywhere, needs python. Prints PLANTREAL-TEST OK or FAILED; exit 0 only on OK.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
PLANT_CS=src/core/unicode/utf8/utf8.cs
PLANT_LINE='// TRAIN T4 emission-check plant (positive control; never committed)'
unplant(){ if grep -qxF "$1" "$2" 2>/dev/null; then cat; else grep -vxF "$1"; fi; }   # verbatim from tT4-emitcheck.sh
X=$(mktemp -d); trap 'rm -rf "$X"' EXIT
printf '%s\n' "$PLANT_LINE" > "$X/plantline.txt"
pyw(){ command -v cygpath >/dev/null && cygpath -w "$1" || echo "$1"; }
committed='package utf8;
// RuneError is the "error" Rune.
public static UntypedInt RuneError => 65533;
public static UntypedInt UTFMax => 4;
'
bad=0
run(){ # <case> <emitted text> <crlf 0|1> <want-old> <want-new>
  local c=$1 d="$X/$1"; mkdir -p "$d/seed/src/core/unicode/utf8" "$d/root/src/core/unicode/utf8"
  printf '%s' "$committed" > "$d/seed/$PLANT_CS"; printf '\n%s\n' "$PLANT_LINE" >> "$d/seed/$PLANT_CS"
  if [ "$3" = 1 ]; then printf '%s' "$2" | sed 's/$/\r/' > "$d/root/$PLANT_CS"; else printf '%s' "$2" > "$d/root/$PLANT_CS"; fi
  echo "$PLANT_CS" > "$d/drift.txt"; : > "$d/plantreal.txt"
  old=$(grep -vxF "$PLANT_CS" "$d/drift.txt" | grep -c .)
  pv=$(python "$(pyw "$SD/tT4-plantreal.py")" "$(pyw "$d/root/$PLANT_CS")" "$(pyw "$d/seed/$PLANT_CS")" "$(pyw "$X/plantline.txt")" | tr -d '\r')
  [ "$pv" = REAL ] && echo "$PLANT_CS" >> "$d/plantreal.txt"
  new=$(unplant "$PLANT_CS" "$d/plantreal.txt" < "$d/drift.txt" | grep -c .)
  v=ok; [ "$old" = "$4" ] && [ "$new" = "$5" ] || { v=WRONG; bad=$((bad + 1)); }
  echo "  $c: plantreal=$pv OLD filter keeps $old (want $4) NEW keeps $new (want $5) $v"
}
echo "T4-19 plant subtraction, OLD (by path) against NEW (by line):"
run pure "$committed" 0 0 0
run real "${committed}// The default lowest and highest continuation byte.
" 0 0 1
run real-crlf "${committed}// The default lowest and highest continuation byte.
" 1 0 1
mkdir -p "$X/u"; printf '%s' "$committed" > "$X/u/seed.cs"; printf '%s' "$committed" > "$X/u/emit.cs"
python "$(pyw "$SD/tT4-plantreal.py")" "$(pyw "$X/u/emit.cs")" "$(pyw "$X/u/seed.cs")" "$(pyw "$X/plantline.txt")" > /dev/null; urc=$?
[ "$urc" = 2 ] && echo "  unplanted: rc=2 (refused) ok" || { echo "  unplanted: rc=$urc WRONG (want 2)"; bad=$((bad + 1)); }
echo "RED (the defect, measured): case 'real' under the OLD filter keeps 0 of 1 drifting file."
[ "$bad" = 0 ] && { echo "PLANTREAL-TEST OK"; exit 0; } || { echo "PLANTREAL-TEST FAILED ($bad)"; exit 1; }
