#!/usr/bin/env bash
# TRAIN I conflict map: merge each seat in draft order onto master in a scratch worktree; on a conflict,
# record the files and ABORT that seat's merge (so later seats merge without it), then continue. The
# output is the full conflict map in one pass; the real rehearsal resolves them in order afterwards.
set -u
WT=/h/go2cs-tmp-coord/tI-map
SEATS=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainI/tI-seats-draft.txt
BASE=2ff42f7a160b4f87b2dcf3d9df45efddc9627821
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
if [ -d "$WT" ]; then git worktree remove --force "$WT" || exit 1; fi
git worktree add -q --detach "$WT" "$BASE" || exit 1
cd "$WT" || exit 1
git config user.name >/dev/null || exit 1
n=0
grep "^[a-z0-9-]*|" "$SEATS" | cut -d'|' -f1,2 | while IFS='|' read -r ref sha; do
    n=$((n + 1))
    full=$(git rev-parse --verify -q "origin/claude/$ref") || { echo "SEAT $n $ref: ref missing"; continue; }
    if git merge-base --is-ancestor "$full" HEAD; then echo "SEAT $n $ref: already contained"; continue; fi
    if git merge -q --no-ff --no-edit -m "map: merge $ref" "$full" >/dev/null 2>&1; then
        echo "SEAT $n $ref: clean"
    else
        files=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
        echo "SEAT $n $ref: CONFLICT in: $files"
        git merge --abort
    fi
done
echo "MAP DONE head=$(git rev-parse --short=10 HEAD)"
