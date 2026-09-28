#!/usr/bin/env bash
# TRAIN H ASSEMBLY: signed --no-ff seat merges onto the landed master, in tH-seats.txt order, on branch claude/coord-trainH in
# the assembly worktree. A conflict whose (seat, file) pair has a saved pre-resolution under preres/ takes that file (the six hand
# resolutions of the first pass; no commit since touches those files, checked 11:10). Any other conflict STOPS the run.
set -u
S=$(cd "$(dirname "$0")" && pwd); P="$S/preres"
BASE=${BASE:?the landed master tip}
W=${W:-/h/go2cs-tmp-coord/tH}
cd /h/Projects/go2cs || exit 1
[ -d "$W" ] || git worktree add -b claude/coord-trainH "$W" "$BASE" >/dev/null 2>&1 || { echo "ABORT worktree"; exit 1; }
cd "$W" || exit 1
[ "$(git rev-parse HEAD)" = "$(git rev-parse "$BASE")" ] || { echo "ABORT: HEAD is not BASE"; exit 1; }
echo "ASSEMBLY onto master $(git rev-parse --short=10 HEAD)"
pre(){ # seat file -> preres path or empty
  case "$1|$2" in
    "i9-disclosure-record-pin|src/core/testing/TestExecution.cs") echo "$P/src/core/testing/TestExecution.cs" ;;
    "c2-source-paths|src/core/runtime/goenvs_impl.cs") echo "$P/src/core/runtime/goenvs_impl.cs" ;;
    "c2-negative-shift|src/core/runtime/panicvalues_impl.cs") echo "$P/src/core/runtime/panicvalues_impl.cs" ;;
    "c1-runtime-error-factories|src/core/runtime/panicvalues_impl.cs") echo "$P/src/core/runtime/panicvalues_impl.c1.cs" ;;
    "c1-unsigned-index-ulong|src/core/math/bits/bits.cs") echo "$P/src/core/math/bits/bits.cs" ;;
    "r-panic-frames|src/core/reflect/value_impl.cs") echo "$P/src/core/reflect/value_impl.cs" ;;
    "r-panic-frames|src/core/runtime/hash_impl.cs") echo "$P/src/core/runtime/hash_impl.cs" ;;
    *) echo "" ;;
  esac
}
while IFS='|' read -r b sha m; do
  [ -z "$b" ] && continue
  git merge-base --is-ancestor "$sha" "origin/claude/$b" 2>/dev/null && [ "$(git rev-parse --short=10 "origin/claude/$b")" = "$sha" ] || { echo "ABORT: $b pinned $sha != origin $(git rev-parse --short=10 "origin/claude/$b")"; exit 2; }
  if git merge -q --no-ff -S -m "Merge claude/$b ($sha) into master -- TRAIN H, $m" "$sha" >/dev/null 2>&1; then echo "OK   $b -> $(git rev-parse --short=10 HEAD)"; continue; fi
  git rev-parse -q --verify MERGE_HEAD >/dev/null || { echo "ABORT: merge of $b never ran"; exit 2; }
  rest=""; used=""
  for f in $(git diff --name-only --diff-filter=U); do
    r=$(pre "$b" "$f")
    if [ -n "$r" ] && [ -s "$r" ]; then cp -p "$r" "$f" && git add "$f" && used="$used $f"; else rest="$rest $f"; fi
  done
  if [ -n "$rest" ]; then echo "CONFLICT $b:$rest"; exit 2; fi
  if git grep -q -E '^(<<<<<<<|>>>>>>>) ' -- $used; then echo "ABORT: markers after pre-resolution in$used"; exit 2; fi
  git commit -q -S --no-edit && echo "OK*  $b -> $(git rev-parse --short=10 HEAD) ($used )"
done < "$S/tH-seats.txt"
echo "ASSEMBLE DONE tip=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 HEAD^{tree})"
