#!/usr/bin/env bash
# tT4-i9-drift.sh -- TRAIN T4 RUN2 (COORD 2026-10-13): the token-identity classification of the i9 shard's TRACKED
# rewrites, the same proof the T4 fixup used for row 2's -stdlib footprint (tT4-drift-tokens.py, tT4-drift-classify.py),
# pointed at the i9's worktree instead of an emission root. A READING: it never edits the worktree; it copies the
# changed files OUT and compares each with its HEAD blob.
#   bash tT4-i9-drift.sh <worktree, POSIX path> <out dir OUTSIDE the worktree>
# Run it AFTER `I9 SHARD DONE (TRAIN T4)`, at the same head, before anything resets the tree.
# Classes, per changed tracked file (CR stripped on both sides first):
#   LINE-ENDING     identical after CR strip
#   COMMENT-WS      tT4-drift-tokens.py PASS: comment / whitespace only (package_info.cs: GoPositionMap lines only)
#   then tT4-drift-classify.py on every TOKENS DIFFER pair, one count per hunk:
#   DEREF-ALIAS-DROPPED (11ef3a1207), LAMBDA-EXPR-BODY (58413a5f57), DEREF-ALIAS-ADDED and OTHER (read BY NAME)
#   NOT-CLASSED     anything the token reader refused (a lex failure, a non-.cs kind, a deleted file): named
set -u
SD=$(cd "$(dirname "$0")" && pwd)
W=${1:?usage: tT4-i9-drift.sh <worktree> <out dir>}; O=${2:?usage: tT4-i9-drift.sh <worktree> <out dir>}
[ -n "$W" ] && [ -d "$W/.git" -o -f "$W/.git" ] || { echo "I9-DRIFT ABORT: $W is not a worktree"; exit 2; }
for f in tT4-drift-tokens.py tT4-drift-classify.py; do [ -f "$SD/$f" ] || { echo "I9-DRIFT ABORT: $SD/$f missing"; exit 2; }; done
case "$(realpath -m "$O")/" in "$(realpath "$W")/"*) echo "I9-DRIFT ABORT: the out dir is inside the worktree"; exit 2;; esac
mkdir -p "$O/U-windows" || exit 2
[ -z "$(ls -A "$O/U-windows")" ] || { echo "I9-DRIFT ABORT: $O/U-windows is not empty (use a fresh out dir)"; exit 2; }
: > "$O/attr-windows.txt"; : > "$O/attr-linux.txt"; : > "$O/attr-darwin.txt"; : > "$O/not-classed.txt"
head=$(git -C "$W" rev-parse HEAD)
nm=0; nx=0
while IFS= read -r -d '' st && IFS= read -r -d '' p; do
  case "$st" in
    M) case "$p" in
         *.cs|*.cs.auto) mkdir -p "$O/U-windows/$(dirname "$p")" && cp "$W/$p" "$O/U-windows/$p" || exit 2
                         echo "$p" >> "$O/attr-windows.txt"; nm=$((nm + 1)) ;;
         *) echo "NOT-CLASSED $p :: not a .cs kind" >> "$O/not-classed.txt"; nx=$((nx + 1)) ;;
       esac ;;
    *) echo "NOT-CLASSED $p :: status $st" >> "$O/not-classed.txt"; nx=$((nx + 1)) ;;
  esac
done < <(git -C "$W" -c core.quotepath=false diff --no-renames --name-status -z HEAD)
echo "I9-DRIFT head=$head modified .cs files=$nm not-classed=$nx"
PY="python -B"
$PY "$(cygpath -w "$SD/tT4-drift-tokens.py")" all "$(cygpath -w "$W")" "$(cygpath -w "$O")" "$(cygpath -w "$O/drift-tokens-report.txt")" > "$O/drift-tokens.console.log" 2>&1; trc=$?
[ $trc = 0 ] || [ $trc = 1 ] || { echo "I9-DRIFT ABORT: tT4-drift-tokens.py rc=$trc (tail of $O/drift-tokens.console.log follows)"; tail -n 5 "$O/drift-tokens.console.log"; exit 2; }
R="$O/drift-tokens-report.txt"
nle=$(grep -c '^FAIL .*:: IDENTICAL after CR strip' "$R"); ncw=$(grep -c '^PASS ' "$R")
grep '^FAIL .*:: TOKENS DIFFER' "$R" > "$O/drift-tokens-differ.txt"; ntd=$(wc -l < "$O/drift-tokens-differ.txt")
grep '^FAIL ' "$R" | grep -v ':: IDENTICAL after CR strip' | grep -v ':: TOKENS DIFFER' | sed 's/^FAIL windows [a-z]* /NOT-CLASSED /' >> "$O/not-classed.txt"
crc=0; CL="DRIFT-CLASSES pairs=0"
if [ "$ntd" -gt 0 ]; then
  $PY "$(cygpath -w "$SD/tT4-drift-classify.py")" "$(cygpath -w "$W")" "$(cygpath -w "$O")" "$(cygpath -w "$O/drift-tokens-differ.txt")" "$(cygpath -w "$O/drift-classes.txt")" > "$O/drift-classify.console.log" 2>&1; crc=$?
  CL=$(tail -n 1 "$O/drift-classify.console.log")
fi
echo "I9-DRIFT LINE-ENDING=$nle COMMENT-WS=$ncw TOKENS-DIFFER=$ntd NOT-CLASSED=$(wc -l < "$O/not-classed.txt") classify-rc=$crc"
echo "I9-DRIFT $CL"
[ -s "$O/not-classed.txt" ] && { echo "I9-DRIFT not-classed, by name:"; cat "$O/not-classed.txt"; }
[ -f "$O/drift-classes.txt" ] && grep -q '^    OTHER ' "$O/drift-classes.txt" && { echo "I9-DRIFT OTHER, by name (drift-classes.txt):"; grep -B1 '^    OTHER ' "$O/drift-classes.txt" | grep -v '^--$'; }
exit $crc
