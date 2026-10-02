#!/usr/bin/env bash
# TRAIN M PRE-map: merge each seat AT ITS SEATED SHA (column 2), in draft order, onto TRAIN L's LANDED master aa0a07d5fd in a
# scratch worktree; on a conflict record the files and ABORT that seat's merge (later seats merge without it), then
# continue. Also reports any seat whose branch tip has MOVED past its seated SHA. Output = the full map in one pass.
# Run 1 (2026-10-02, coord-scratch/tM/map-run1.log): 15 seats clean in draft order, map union 8d7305053f. RE-RUN it after
# any change to the seat list (a late seat, a moved sha, a re-ordered row): a map is a reading of ONE list.
# VERIFY ROUND 2 (tM-CHANGES.md section 10): the list is overridable (SEATS=...; the default is still COORD's live hnd
# file); a map that read EVERY row clean stamps the rows it read (rows-sha256 = sha256 over the 'ref|sha' columns of the
# seat rows, 12 hex: comment lines do not move it) and keeps its union under a second LOCAL ref named by that hash,
# refs/coord/tM-map/rows-<hash>. tM-assemble.sh computes the same hash over the list IT merges and finds its map union
# through that ref, so no script carries a map union sha (the list grew from 15 to 20 to 24 rows in four hours).
# VERIFY ROUND 3 (tM-CHANGES.md section 11): the list is read ONCE, into a snapshot under coord-scratch/tM; the merges
# and the hash both read the snapshot. The FROZEN list (08:20, 24 rows, rows-sha256=0b3fd4358320) mapped 24 of 24 clean:
# MAP DONE head=11c188daf3 (coord-scratch/tM/asm1/map-frozen.log), kept as refs/coord/tM-map/rows-0b3fd4358320.
set -u
WT=/h/go2cs-tmp-coord/tM-map
SEATS=${SEATS:-/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainM/tM-seats-draft.txt}
BASE=$(git -C /h/Projects/go2cs rev-parse aa0a07d5fd) || exit 1
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS)"; exit 2; }
# Verify round 3 (RULED): the list is read ONCE. The default list is COORD's LIVE hnd file, and the map read it twice:
# the rows it merged, and again at the end for the hash it stamped and named the rows ref by. A row re-pointed between
# the two reads (it happened at 07:43 on 2026-10-02) wrote refs/coord/tM-map/rows-<hash of the NEW list> onto the union
# of the OLD rows. The rows are snapshotted to the scratch folder here; the merges read the snapshot and the hash is the
# snapshot's (the same bytes tM-assemble.sh hashes: the 'ref|sha' columns of the row lines, CR-stripped).
X=/h/go2cs-tmp-coord/coord-scratch/tM
mkdir -p "$X" || exit 1
ROWS=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
[ -n "$ROWS" ] || { echo "ABORT: no seat rows in $SEATS"; exit 2; }
RSHA=$(printf '%s\n' "$ROWS" | cut -d'|' -f1,2 | sha256sum | cut -c1-12)
SNAP="$X/map-rows-$RSHA.txt"
printf '%s\n' "$ROWS" > "$SNAP" || { echo "ABORT: cannot write the row snapshot $SNAP"; exit 2; }
[ "$(cut -d'|' -f1,2 "$SNAP" | sha256sum | cut -c1-12)" = "$RSHA" ] || { echo "ABORT: the snapshot $SNAP does not hash to $RSHA"; exit 2; }
echo "MAP list read ONCE: $(grep -c . "$SNAP") rows of $SEATS, rows-sha256=$RSHA, snapshot $SNAP (the merges below read the snapshot)"
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
# Verify round 1: a map union is cited by sha in the scripts and the notes (8d7305053f: tM-assemble.sh compares its tree
# with it), and the detached map worktree is all that holds it. Before the worktree is removed its head is kept under
# a LOCAL ref (refs/coord/..., never pushed), so a cited map union stays in the object store across re-runs.
if [ -d "$WT" ]; then
  prev=$(git -C "$WT" rev-parse HEAD 2>/dev/null) && git update-ref "refs/coord/tM-map/${prev:0:10}" "$prev" && echo "PREVIOUS map union ${prev:0:10} kept as refs/coord/tM-map/${prev:0:10} (local)"
  git worktree remove --force "$WT" || exit 1
fi
git worktree add -q --detach "$WT" "$BASE" || exit 1
cd "$WT" || exit 1
n=0; nbad=0
while IFS='|' read -r ref sha rest; do
    n=$((n + 1))
    full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "SEAT $n $ref: seated sha $sha MISSING"; nbad=$((nbad + 1)); continue; }
    tip=$(git rev-parse --verify -q "origin/claude/$ref" || echo none)
    [ "$tip" != "$full" ] && echo "SEAT $n $ref: NOTE branch tip ${tip:0:10} != seated ${full:0:10}"
    if git merge-base --is-ancestor "$full" HEAD; then echo "SEAT $n $ref: already contained"; nbad=$((nbad + 1)); continue; fi
    # Verify round 1 (merge-hazards: "a merge probe's green is readable only if the two sides are independent AND
    # overlapping ... print the shared-path count beside every row"): the paths this seat and the union so far BOTH
    # changed since their merge-base. A clean row with shared paths is where a silent duplicate or a dropped line can
    # sit; precheck's BOTH arm reads each of them in the merge's own tree, and every shared path that is not a
    # registration file is READ by a person before the fixup (tM-README.md M1).
    mbx=$(git merge-base HEAD "$full")
    shared=$(LC_ALL=C comm -12 <(git diff --name-only "$mbx" "$full" | LC_ALL=C sort) <(git diff --name-only "$mbx" HEAD | LC_ALL=C sort))
    ns=$(printf '%s\n' "$shared" | grep -c .)
    if git -c user.name=map -c user.email=map@invalid merge -q --no-ff --no-edit -m "map: merge $ref" "$full" >/dev/null 2>&1; then
        echo "SEAT $n $ref: clean (shared paths with the union so far: $ns: $(echo $shared | cut -c1-300))"
    else
        files=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
        echo "SEAT $n $ref: CONFLICT in: $files"; nbad=$((nbad + 1))
        git merge --abort
    fi
done < "$SNAP"
git update-ref "refs/coord/tM-map/$(git rev-parse --short=10 HEAD)" HEAD   # local, never pushed: the map union stays reachable
# (verify round 3: RSHA is the SNAPSHOT's hash, computed before the first merge; the list file is not read again)
if [ "$nbad" = 0 ] && [ "$n" -ge 1 ]; then
  git update-ref "refs/coord/tM-map/rows-$RSHA" HEAD   # local: the union of THIS list, found by tM-assemble.sh through the list's own hash
  echo "MAP DONE head=$(git rev-parse --short=10 HEAD) rows=$n rows-sha256=$RSHA (every row clean: kept as refs/coord/tM-map/rows-$RSHA, the ref tM-assemble.sh reads)"
else
  echo "MAP DONE head=$(git rev-parse --short=10 HEAD) rows=$n rows-sha256=$RSHA NOT-CLEAN rows=$nbad (missing, already contained or conflicting: NO rows ref written; this head is not the union of the list)"
fi
