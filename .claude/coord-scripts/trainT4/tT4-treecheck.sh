#!/usr/bin/env bash
# tT4-treecheck.sh -- COORD's TREE CHECK for the T4 fixup, without the i9's tree in the object store.
# RUN2 (2026-10-13): the union is train-t4-union-r2 in tT4r2 (b258fe81db). The i9's tree 0145a977f0 IS in the store now
# (run1's treecheck wrote it); the run2 fixup tree with the regenerated paths put back to the run2 union's blobs must
# differ from it by EXACTLY the paths of row 2's range a0e2099e9a..fbf2a5ebc5 (T4-13's delta), nothing else.
# The i9's fixup tree 0145a977f0 (no corpus regeneration) was never pushed: it is not on this box. Equivalent check:
# put the N regenerated paths back to their union-head blobs in a TEMPORARY index built from the fixup tree (the
# worktree's index is not touched), write that tree, and require it to EQUAL 0145a977f0; then the diff from that tree to
# the fixup tree must name EXACTLY the allow file's paths.
#   bash tT4-treecheck.sh <fixup-commit> <allow-file>
set -u
U=/h/go2cs-tmp-coord/tT4r2
UNION=b258fe81db94baf80798e5781dc85688fc2c6bb1
DELTA=a0e2099e9a..fbf2a5ebc5
I9TREE=0145a977f02e3a5c3d61070640356b4b96dbcf0f
FX=${1:?fixup commit}; AL=${2:?allow file}
FT=$(git -C "$U" rev-parse "$FX^{tree}") || exit 2
TMPI=$(mktemp -d /h/go2cs-tmp-coord/coord-scratch/tT4/run2/treecheck.XXXXXX); [ -n "$TMPI" ] && [ -d "$TMPI" ] || exit 2
export GIT_INDEX_FILE="$(cygpath -w "$TMPI")\\index"
git -C "$U" read-tree "$FT" || exit 2
tr -d '\r' < "$AL" | grep -vE '^[[:space:]]*(#|$)' | LC_ALL=C sort > "$TMPI/allow.txt"
git -C "$U" ls-tree -r --full-tree "$UNION" -- src/core | awk -F'	' 'NR == FNR { want[$0] = 1; next } ($2 in want)' "$TMPI/allow.txt" - > "$TMPI/union-entries.txt"   # no path argv (1,548 paths overflow the command line)
nu=$(grep -c . "$TMPI/union-entries.txt"); na=$(grep -c . "$TMPI/allow.txt")
[ "$nu" = "$na" ] || echo "TREECHECK NOTE: $nu of $na allow paths exist at the union head"
awk -F'\t' '{ split($1, a, " "); printf "%s %s\t%s\n", a[1], a[3], $2 }' "$TMPI/union-entries.txt" | git -C "$U" update-index --index-info || exit 2
RT=$(git -C "$U" write-tree) || exit 2
unset GIT_INDEX_FILE
git -C "$U" diff --name-only "$I9TREE" "$RT" | LC_ALL=C sort > "$TMPI/vsi9.txt"; git -C "$U" diff --name-only "${DELTA%%..*}" "${DELTA##*..}" | LC_ALL=C sort > "$TMPI/delta.txt"
D9=SAME; [ -s "$TMPI/delta.txt" ] && cmp -s "$TMPI/vsi9.txt" "$TMPI/delta.txt" || D9=DIFFERENT
echo "TREECHECK fixup tree=$FT; fixup tree with the $na regenerated paths put back to the union head = $RT; vs the i9's $I9TREE: $(grep -c . "$TMPI/vsi9.txt") path(s) [$(paste -sd' ' "$TMPI/vsi9.txt")] vs the T4-13 delta $DELTA [$(paste -sd' ' "$TMPI/delta.txt")]: $D9"
git -C "$U" diff --name-only "$RT" "$FT" | LC_ALL=C sort > "$TMPI/diff.txt"
extra=$(LC_ALL=C comm -13 "$TMPI/allow.txt" "$TMPI/diff.txt" | grep -c .); miss=$(LC_ALL=C comm -23 "$TMPI/allow.txt" "$TMPI/diff.txt" | grep -c .)
echo "TREECHECK git diff --name-only $RT $FT: $(grep -c . "$TMPI/diff.txt") path(s); EXTRA (not in the allow file) $extra; MISSING (in the file, not changed) $miss"
LC_ALL=C comm -13 "$TMPI/allow.txt" "$TMPI/diff.txt" | head -n 20 | sed 's/^/  EXTRA /'
LC_ALL=C comm -23 "$TMPI/allow.txt" "$TMPI/diff.txt" | head -n 20 | sed 's/^/  MISSING /'
if [ "$D9" != SAME ]; then echo "  the residual vs the i9's tree is not readable here (not in the store); vs the union head:"; git -C "$U" diff --name-only "$UNION" "$RT" | sed 's/^/  UNION->RT /' | head -n 20; fi
echo "TREECHECK union->fixup: $(git -C "$U" diff --name-only "$UNION" "$FT" | grep -c .) path(s) (EXPECT the allow file + 1 projitems + 6 goldens)"
case "$TMPI" in /h/go2cs-tmp-coord/coord-scratch/tT4/run2/treecheck.*) rm -rf "$TMPI" ;; esac
[ "$D9" = SAME ] && [ "$extra" = 0 ] && [ "$miss" = 0 ]
