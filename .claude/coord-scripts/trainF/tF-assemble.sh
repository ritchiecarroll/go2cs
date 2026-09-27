#!/usr/bin/env bash
# TRAIN F assembly: signed --no-ff seat merges onto the landed master (TRAIN E), in ruled order
# (tF-seats.txt). Pre-resolutions come from the rehearsal (tF-reh): each known conflict file is taken from the
# rehearsal commit that resolved it; runtime/<goos>/package_info.cs map conflicts take the incoming side and are
# re-derived from the union converter's emission in a FIXUP commit after the last seat. Anything else stops the run.
set -u
BASE=${BASE:?the landed master tip}
W=/h/go2cs-tmp-coord/tF
SD=$(dirname "$0")
cd /h/Projects/go2cs || exit 1
git fetch -q origin || { echo FETCH-FAILED; exit 1; }
[ -d "$W" ] || git worktree add -q -b claude/coord-trainF "$W" "$BASE" || exit 1
cd "$W" || exit 1
[ "$(git rev-parse HEAD)" = "$(git rev-parse "$BASE")" ] || { echo "HEAD is not BASE"; exit 1; }
resolve_from(){ # commit files...
  local c=$1; shift
  git checkout "$c" -- "$@" && git add "$@"
}
while IFS='|' read -r b sha msg; do
  [ -z "$b" ] && continue
  tip=$(git rev-parse --short=10 "origin/claude/$b")
  [ "$tip" = "$sha" ] || { echo "MOVED $b: origin $tip, pinned $sha"; exit 1; }
  if git merge -q --no-ff -S -m "Merge claude/$b ($sha) into master -- TRAIN F, $msg" "$sha" >/dev/null 2>&1; then
    echo "OK   $b $sha -> $(git rev-parse --short=10 HEAD)"; continue
  fi
  U=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  rest=""
  for f in $U; do
    case "$b|$f" in
      "p1-traceregion|src/core/runtime/managed_impl.cs") resolve_from 8aa9ef8afc "$f" ;;
      *"|src/core/runtime/"*"/package_info.cs") git checkout --theirs -- "$f" && git add "$f" ;;
      *) rest="$rest $f" ;;
    esac
  done
  if [ -n "$rest" ]; then echo "CONFLICT $b (unresolved):$rest"; exit 1; fi
  git commit -q -S --no-edit || { echo "COMMIT-FAILED $b"; exit 1; }
  echo "OK*  $b $sha -> $(git rev-parse --short=10 HEAD) (pre-resolved: $U)"
done < "$SD/tF-seats.txt"
echo "tip=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 HEAD^{tree}) rehearsal-tree=$(git rev-parse --short=10 fcc50a4d43^{tree})"
