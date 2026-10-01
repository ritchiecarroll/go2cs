#!/usr/bin/env bash
# TRAIN L PRE-map: merge each seat AT ITS SEATED SHA (column 2), in draft order, onto TRAIN K's LANDED master 75648a022b in a
# scratch worktree; on a conflict record the files and ABORT that seat's merge (later seats merge without it), then
# continue. Also reports any seat whose branch tip has MOVED past its seated SHA. Output = the full map in one pass.
set -u
WT=/h/go2cs-tmp-coord/tL-map
SEATS=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainL/tL-seats-draft.txt
BASE=$(git -C /h/Projects/go2cs rev-parse 75648a022b) || exit 1
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
if [ -d "$WT" ]; then git worktree remove --force "$WT" || exit 1; fi
git worktree add -q --detach "$WT" "$BASE" || exit 1
cd "$WT" || exit 1
n=0
while IFS='|' read -r ref sha rest; do
    n=$((n + 1))
    full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "SEAT $n $ref: seated sha $sha MISSING"; continue; }
    tip=$(git rev-parse --verify -q "origin/claude/$ref" || echo none)
    [ "$tip" != "$full" ] && echo "SEAT $n $ref: NOTE branch tip ${tip:0:10} != seated ${full:0:10}"
    if git merge-base --is-ancestor "$full" HEAD; then echo "SEAT $n $ref: already contained"; continue; fi
    if git -c user.name=map -c user.email=map@invalid merge -q --no-ff --no-edit -m "map: merge $ref" "$full" >/dev/null 2>&1; then
        echo "SEAT $n $ref: clean"
    else
        files=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
        echo "SEAT $n $ref: CONFLICT in: $files"
        git merge --abort
    fi
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
echo "MAP DONE head=$(git rev-parse --short=10 HEAD)"
