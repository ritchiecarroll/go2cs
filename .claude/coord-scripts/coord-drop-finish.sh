#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 8
git checkout -q --detach ea0d5573c || exit 8
git clean -qfd 2>/dev/null
echo "=== RE2B with the one conflict resolved (seat side: a strict superset -- adds AttributeTargets.Class + docs)"
git merge --no-ff --no-edit -q -F "$S/coord-merge-r-e2b.txt" ca74dd433 </dev/null >/dev/null 2>&1
git checkout --theirs src/core/golib/GoArrayDimsAttribute.cs 2>/dev/null
git add src/core/golib/GoArrayDimsAttribute.cs
grep -c '^<<<<<<<' src/core/golib/GoArrayDimsAttribute.cs | sed 's/^/  markers remaining: /'
git commit -q --no-edit && echo "  ok RE2B -> $(git log -1 --format=%h)"
for row in "C1Q61|e33e14ccf|coord-merge-c1-q61.txt" "C1Q64|7ab3d6fa6|coord-merge-c1-q64.txt" "C1Q58D|44fba8cf6|coord-merge-c1-q58-design.txt" "GDC|67eba534f|coord-merge-g-deferred-class.txt"; do
  k=${row%%|*}; rest=${row#*|}; sha=${rest%%|*}; msg=${rest#*|}
  if ! git merge --no-ff --no-edit -q -F "$S/$msg" "$sha" </dev/null >/dev/null 2>&1; then
    echo "  MERGE FAILED at $k"; git diff --name-only --diff-filter=U | sed 's/^/      /'; git merge --abort </dev/null 2>/dev/null; exit 1
  fi
  echo "  ok $k -> $(git log -1 --format=%h)"
done
echo "=== the version-wrapper hand-own (independent of the dropped seat)"
if git cherry-pick -x 12ee83671 </dev/null >/dev/null 2>&1; then echo "  ok rtlGetVersion -> $(git log -1 --format=%h)"; else echo "  CHERRY-PICK FAILED"; git diff --name-only --diff-filter=U | sed 's/^/      /'; git cherry-pick --abort </dev/null 2>/dev/null; fi
echo "=== DROP ASSEMBLY HEAD: $(git log -1 --format=%h)  seats: $(git log --first-parent --format=%h b91684991..HEAD | wc -l)"
echo "=== markers anywhere: $(git grep -c '^<<<<<<<' HEAD -- 2>/dev/null | wc -l)"
