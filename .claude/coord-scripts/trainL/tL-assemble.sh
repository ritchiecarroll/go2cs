#!/usr/bin/env bash
# TRAIN L assembly (COORD): SIGNED merges of every seat AT ITS SEATED SHA, in draft order, onto TRAIN K's landed master
# 75648a022b, on the new local branch claude/coord-trainL-union in /h/go2cs-tmp-coord/tL. The pre-map (run 2) read all
# 11 seats CLEAN on that base, so there is NO ruled resolution: ANY conflict ABORTS the run. Launch from a per-run copy
# (floor 4). Resumable onto its own clean union branch.
set -u
WT=/h/go2cs-tmp-coord/tL
D=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainL
SEATS=$D/tL-seats-draft.txt
X=/h/go2cs-tmp-coord/coord-scratch
mkdir -p "$X/tL"
BASE=$(git -C /h/Projects/go2cs rev-parse 75648a022b) || exit 1
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
if [ -d "$WT" ]; then
  [ "$(git -C "$WT" rev-parse --abbrev-ref HEAD)" = claude/coord-trainL-union ] || { echo "ABORT: $WT is not on the union branch"; exit 2; }
  [ -z "$(git -C "$WT" status --porcelain | grep -v '^??')" ] || { echo "ABORT: $WT is dirty"; exit 2; }
  echo "RESUME at $(git -C "$WT" rev-parse --short=10 HEAD)"
else
  git worktree add -q -b claude/coord-trainL-union "$WT" "$BASE" || exit 1
fi
cd "$WT" || exit 1
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1))
  full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "ABORT seat $n $ref: $sha missing"; exit 2; }
  tip=$(git rev-parse --verify -q "origin/claude/$ref" || echo none)
  [ "$tip" != "$full" ] && echo "SEAT $n $ref: NOTE branch tip ${tip:0:10} != seated ${full:0:10}"
  git merge-base --is-ancestor "$full" HEAD && { echo "SEAT $n $ref: already contained"; continue; }
  printf 'Merge claude/%s (%s) into TRAIN L -- %s\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" > "$X/tL/msg-seat$n.txt" || exit 2
  if git merge -q -S --no-ff -F "$X/tL/msg-seat$n.txt" "$full" >/dev/null 2>&1; then echo "SEAT $n $ref: merged $(git rev-parse --short=10 HEAD)"; continue; fi
  un=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  echo "ABORT seat $n $ref: UNRULED conflict in [$un]"; git merge --abort; exit 3
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
echo "ASSEMBLED head=$(git rev-parse HEAD) seats=$n"
while IFS='|' read -r ref sha desc; do git merge-base --is-ancestor "$sha" HEAD || { echo "ANCESTRY FAIL $ref $sha"; exit 4; }; done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
echo "ANCESTRY: all seats contained"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] && echo "TREE CLEAN" || { echo "TREE DIRTY"; git status --porcelain | head; exit 5; }
