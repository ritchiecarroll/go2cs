#!/usr/bin/env bash
# tP-premap-all.sh -- the WHOLE pre-map reading for one row list, in one command (DRAFT 2026-10-04, UNCOMMITTED).
# No worktree, no build, no conversion, no ref: git object reads, ort merges into a SCRATCH object directory
# (OUT/objects; nothing is written to the shared store), two python censuses and a gofmt PARSE of the merged Go files.
#
#   BASE=<commit> ROWS=<file> OUT=<dir> bash tP-premap-all.sh
#   BASE  P's base (TRAIN O's landed master, read with ls-remote; a stand-in only when given explicitly)
#   ROWS  the merge order: 'name|sha|...' (tP-seats-draft.txt) or 'name sha'; '#' lines skipped
#   OUT   a fresh folder for this reading (either path style)
# Writes into OUT:
#   alone.log, onbase.tsv            each row alone onto BASE
#   chain-union.log / .tsv           the cumulative chain in ROWS order, a conflicted path carried as an unrefined union
#   chain-skip.log / .tsv            the same chain with a conflicted row left out (what the assembler would hold)
#   regcheck.log                     tP-regcheck.py at both heads: the six registration files, then EVERY path ort
#                                    content-merged at any step (COUNT and KEYS: nothing dropped, nothing duplicated)
#   unioncheck.log                   tP-unioncheck.py at both heads (projitems, behavioral registration, H4, duplicate
#                                    Go declarations, MS19's HashSet scan, markers, package_info line kinds)
#   gofmt.log                        gofmt -e over the Go files each head changes under src/go2cs (parse only)
#   own-footprints.tsv, writers.tsv  each row's own footprint; every path two or more rows write
#   pairs.log / .tsv                 every pair of rows, each first put on BASE (seat versus seat)
# To read a chain commit or tree afterwards:
#   GIT_ALTERNATE_OBJECT_DIRECTORIES=<OUT, Windows style>/objects git -C /h/Projects/go2cs show <sha>:<path>
# Exit: 0 when the skip chain kept every row (no conflicted step) and both censuses of record are ok; 1 otherwise.
set -u
HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
: "${BASE:?BASE is required: the P base commit, or an explicit stand-in}"
: "${ROWS:?ROWS is required}"
: "${OUT:?OUT is required (a fresh folder)}"
export MSYS_NO_PATHCONV=1
mkdir -p "$OUT/objects" || exit 2
OUT=$(cd "$OUT" && pwd)
ROWS=$(cd "$(dirname "$ROWS")" && pwd)/$(basename "$ROWS")
export GD=${GD:-H:/Projects/go2cs/.git}
export OBJDIR=$OUT/objects
export GIT_OBJECT_DIRECTORY=$(cygpath -m "$OBJDIR")
export GIT_ALTERNATE_OBJECT_DIRECTORIES="$GD/objects"
GOFMT=${GOFMT:-/h/sdk/go1.24.13/bin/gofmt.exe}
g() { git --git-dir="$GD" -c core.quotepath=false "$@"; }
B=$(g rev-parse --verify -q "$BASE^{commit}") || { echo "ABORT: BASE $BASE does not resolve"; exit 2; }
echo "PREMAP-ALL base=${B:0:10} rows=$(grep -vcE '^[[:space:]]*(#|$)' "$ROWS") ($(basename "$ROWS")) out=$OUT $(date '+%Y-%m-%d %H:%M %Z')"
run() { ( cd "$HERE" && BASE=$B ROWS=$ROWS OUT=$OUT/tmp MODE=$1 ONCONFLICT=$2 bash ./tP-premap.sh ); }
run alone union > "$OUT/alone.log" 2>&1; cp "$OUT/tmp/onbase.tsv" "$OUT/onbase.tsv"; tail -n 1 "$OUT/alone.log"
run chain union > "$OUT/chain-union.log" 2>&1; cp "$OUT/tmp/chain.tsv" "$OUT/chain-union.tsv"; cp "$OUT/tmp/chain-merged-paths.txt" "$OUT/chain-merged-paths.txt"; tail -n 1 "$OUT/chain-union.log"
run chain skip > "$OUT/chain-skip.log" 2>&1; cp "$OUT/tmp/chain.tsv" "$OUT/chain-skip.tsv"; tail -n 1 "$OUT/chain-skip.log"
HU=$(tail -n 1 "$OUT/chain-union.tsv" | cut -f4); HS=$(tail -n 1 "$OUT/chain-skip.tsv" | cut -f4)
# every path ort content-merged at any step of the union chain; the registration files are read on their own, and
# package_info.cs is emission (never merged: a union-carried one holds both sides' lines by construction)
mapfile -t SHARED < <(tr -d '\r' < "$OUT/chain-merged-paths.txt" | grep -vE '^src/go2cs\.slnx$|go2cs-src\.projitems$|/BehavioralTests/|/package_info\.cs$' | LC_ALL=C sort -u)
{
  echo "=== union-carry head $HU: the six registration files"; python -B "$(cygpath -m "$HERE/tP-regcheck.py")" "$B" "$HU" "$(cygpath -m "$OUT/chain-union.tsv")"; r1=$?; echo "rc=$r1"
  echo "=== union-carry head $HU: ${#SHARED[@]} other path(s) ort content-merged along the chain"; [ "${#SHARED[@]}" = 0 ] || python -B "$(cygpath -m "$HERE/tP-regcheck.py")" "$B" "$HU" "$(cygpath -m "$OUT/chain-union.tsv")" "${SHARED[@]}"; echo "rc=$?"
  echo "=== skip head $HS: the six registration files"; python -B "$(cygpath -m "$HERE/tP-regcheck.py")" "$B" "$HS" "$(cygpath -m "$OUT/chain-skip.tsv")"; r2=$?; echo "rc=$r2"
  echo "=== skip head $HS: the other content-merged paths"; [ "${#SHARED[@]}" = 0 ] || python -B "$(cygpath -m "$HERE/tP-regcheck.py")" "$B" "$HS" "$(cygpath -m "$OUT/chain-skip.tsv")" "${SHARED[@]}"; r3=$?; echo "rc=$r3"
  echo "REGCHECK-OF-RECORD skip-head registration rc=$r2 other rc=$r3"
} > "$OUT/regcheck.log" 2>&1
{ echo "=== union-carry head $HU"; python -B "$(cygpath -m "$HERE/tP-unioncheck.py")" "$B" "$HU"; echo "=== skip head $HS"; python -B "$(cygpath -m "$HERE/tP-unioncheck.py")" "$B" "$HS"; } > "$OUT/unioncheck.log" 2>&1
{ echo "=== union-carry head $HU"; GOFMT=$GOFMT bash "$HERE/tP-gofmt-parse.sh" "$B" "$HU" "$OUT/tmp-gofmt"; echo "=== skip head $HS"; GOFMT=$GOFMT bash "$HERE/tP-gofmt-parse.sh" "$B" "$HS" "$OUT/tmp-gofmt"; } > "$OUT/gofmt.log" 2>&1
bash "$HERE/tP-footprints.sh" "$B" "$ROWS" "$OUT" > "$OUT/footprints.log" 2>&1
run pairs union > "$OUT/pairs.log" 2>&1; cp "$OUT/tmp/pairs.tsv" "$OUT/pairs.tsv"; tail -n 1 "$OUT/pairs.log"
rm -rf "$OUT/tmp" "$OUT/tmp-gofmt"
# pack the scratch objects (one pack: two files) so OUT stays small and every chain commit stays readable
nloose=$(find "$OUT/objects" -mindepth 2 -type f -not -path '*/pack/*' | wc -l)
if [ "$nloose" -gt 0 ]; then
  find "$OUT/objects" -mindepth 2 -type f -not -path '*/pack/*' | sed -E 's|^.*/objects/(..)/([0-9a-f]{38})$|\1\2|' > "$OUT/objlist.txt"
  mkdir -p "$OUT/objects/pack"
  pk=$(g pack-objects -q "$GIT_OBJECT_DIRECTORY/pack/pack" < "$OUT/objlist.txt") && g verify-pack "$GIT_OBJECT_DIRECTORY/pack/pack-$pk.idx" \
    && for d in "$OUT"/objects/??; do rm -rf "$d"; done
  rm -f "$OUT/objlist.txt"
fi
g cat-file -e "$HU^{tree}" && g cat-file -e "$HS^{tree}" || echo "WARNING: a chain head does not resolve through $GIT_OBJECT_DIRECTORY"
echo "PREMAP-ALL objects: $nloose written to the scratch directory (packed); 0 to the shared store"
nconf=$(grep -c '^STEP .*: CONFLICT' "$OUT/chain-skip.log")
nreg=$(grep -c '^REGCHECK-OF-RECORD skip-head registration rc=0 other rc=0' "$OUT/regcheck.log")
nparse=$(grep -E '^GOFMT-PARSE' "$OUT/gofmt.log" | tail -n 1 | sed -E 's/.*parse-errors=([0-9]+).*/\1/')
echo "PREMAP-ALL DONE conflicted-rows=$nconf regcheck-of-record=$([ "$nreg" = 1 ] && echo ok || echo FAIL) skip-head-parse-errors=${nparse:-?} union-head=$HU skip-head=$HS $(date '+%H:%M:%S')"
[ "$nconf" = 0 ] && [ "$nreg" = 1 ] && [ "${nparse:-1}" = 0 ]
