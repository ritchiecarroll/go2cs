#!/usr/bin/env bash
# TRAIN K assembly (COORD): SIGNED merges of every seat AT ITS SEATED SHA, in draft order, onto TRAIN J's landed master
# f819887fa3, on the new local branch claude/coord-trainK-union in /h/go2cs-tmp-coord/tK. The three conflicts the map
# found are resolved by tK-resolve.py (K8 at g-gpc-torn-snapshot, K9 at i9-a3-runtime-lock-profile, ROSTER at
# coord-roster-cleanup); ANY other conflict ABORTS the run. Launch from a per-run copy (floor 4).
set -u
WT=/h/go2cs-tmp-coord/tK
D=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainK
SEATS=$D/tK-seats-draft.txt
X=/h/go2cs-tmp-coord/coord-scratch
BASE=$(git -C /h/Projects/go2cs rev-parse f819887fa3) || exit 1
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
if [ -d "$WT" ]; then
  # RESUME (run 2+): only onto our own union branch, clean, with no merge in progress
  [ "$(git -C "$WT" rev-parse --abbrev-ref HEAD)" = claude/coord-trainK-union ] || { echo "ABORT: $WT is not on the union branch"; exit 2; }
  [ -z "$(git -C "$WT" status --porcelain | grep -v '^??')" ] || { echo "ABORT: $WT is dirty"; exit 2; }
  echo "RESUME at $(git -C "$WT" rev-parse --short=10 HEAD)"
else
  git worktree add -q -b claude/coord-trainK-union "$WT" "$BASE" || exit 1
fi
cd "$WT" || exit 1
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1))
  full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "ABORT seat $n $ref: $sha missing"; exit 2; }
  git merge-base --is-ancestor "$full" HEAD && { echo "SEAT $n $ref: already contained"; continue; }
  msg=$(printf 'Merge claude/%s (%s) into TRAIN K -- %s\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)")
  if git merge -q -S --no-ff -m "$msg" "$full" >/dev/null 2>&1; then echo "SEAT $n $ref: merged $(git rev-parse --short=10 HEAD)"; continue; fi
  un=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  case "$ref" in
    g-gpc-torn-snapshot) [ "$un" = "src/core/golib/runtime/Goroutine.cs " ] || { echo "ABORT seat $n: unexpected conflict set [$un]"; exit 3; }
       python H:/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainK/tK-resolve.py K8 H:/go2cs-tmp-coord/tK H:/go2cs-tmp-coord/coord-scratch/PRERES-K8-Goroutine.resolved.cs || exit 3
       git add src/core/golib/runtime/Goroutine.cs || exit 3 ;;
    i9-a3-runtime-lock-profile) [ "$un" = "src/tests/GolibTests/ThreadStateCensusTests.cs " ] || { echo "ABORT seat $n: unexpected conflict set [$un]"; exit 3; }
       python H:/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainK/tK-resolve.py K9 H:/go2cs-tmp-coord/tK || exit 3
       git add src/tests/GolibTests/ThreadStateCensusTests.cs || exit 3 ;;
    coord-roster-cleanup) [ "$un" = "docs/ConversionStrategies-Reference/manual-conversions.md " ] || { echo "ABORT seat $n: unexpected conflict set [$un]"; exit 3; }
       python H:/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainK/tK-resolve.py ROSTER H:/go2cs-tmp-coord/tK H:/go2cs-tmp-coord/coord-scratch/tK/roster-para.txt || exit 3
       git add docs/ConversionStrategies-Reference/manual-conversions.md || exit 3 ;;
    *) echo "ABORT seat $n $ref: UNRULED conflict in [$un]"; git merge --abort; exit 3 ;;
  esac
  [ -z "$(git diff --name-only --diff-filter=U)" ] || { echo "ABORT seat $n: unresolved paths remain"; exit 3; }
  # a message FILE: Git for Windows cannot read a <(...) process substitution (run 1 died on /proc/<pid>/fd/63)
  printf '%s

Resolved by rule (PRERES in the TRAIN K draft; tK-resolve.py).
' "$msg" > "$X/tK/msg-seat$n.txt" || exit 3
  git commit -q -S -F "$X/tK/msg-seat$n.txt" || exit 3
  echo "SEAT $n $ref: merged WITH RESOLUTION $(git rev-parse --short=10 HEAD)"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
echo "ASSEMBLED head=$(git rev-parse HEAD) seats=$n"
# every seated sha is an ancestor
while IFS='|' read -r ref sha desc; do git merge-base --is-ancestor "$sha" HEAD || { echo "ANCESTRY FAIL $ref $sha"; exit 4; }; done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
echo "ANCESTRY: all seats contained"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] && echo "TREE CLEAN" || { echo "TREE DIRTY"; git status --porcelain | head; exit 5; }
