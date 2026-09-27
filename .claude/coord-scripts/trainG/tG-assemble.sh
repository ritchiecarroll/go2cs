#!/usr/bin/env bash
# TRAIN G assembly: signed --no-ff seat merges onto the landed master, in ruled order (tG-seats.txt).
# Pre-resolutions come from the rehearsal (branch rehearsal/tG): each known conflict file is taken from the
# rehearsal commit that resolved it; runtime/<goos>/package_info.cs map conflicts take the incoming side.
# After the last seat, ONE fixup commit takes the merge-hazard review's two fixes from the rehearsal fixup 64209be705
# (the ReadTrace block after StopTrace; P1's four linkname forwarders carry StackTraceHidden). Anything else stops the run.
set -u
BASE=${BASE:?the landed master tip}
W=/h/go2cs-tmp-coord/tG
SD=$(cd "$(dirname "$0")" && pwd)
cd /h/Projects/go2cs || exit 1
git fetch -q origin || { echo FETCH-FAILED; exit 1; }
[ -d "$W" ] || git worktree add -q -b claude/coord-trainG "$W" "$BASE" || exit 1
cd "$W" || exit 1
[ "$(git rev-parse HEAD)" = "$(git rev-parse "$BASE")" ] || { echo "HEAD is not BASE"; exit 1; }
resolve_from(){ local c=$1; shift; git checkout "$c" -- "$@" && git add "$@"; }
while IFS='|' read -r b sha msg; do
  [ -z "$b" ] && continue
  tip=$(git rev-parse --short=10 "origin/claude/$b")
  [ "$tip" = "$sha" ] || { echo "MOVED $b: origin $tip, pinned $sha"; exit 1; }
  if git merge -q --no-ff -S -m "Merge claude/$b ($sha) into master -- TRAIN G, $msg" "$sha" >/dev/null 2>&1; then
    echo "OK   $b $sha -> $(git rev-parse --short=10 HEAD)"; continue
  fi
  U=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  rest=""
  for f in $U; do
    case "$b|$f" in
      "g-managed-tracer|src/go2cs/manualTypeOperations.go") resolve_from f2e3255bff "$f" ;;
      "r-runtime-claims|src/core/internal/runtime/atomic/atomic_impl.cs") resolve_from 8fffa24923 "$f" ;;
      *"|src/core/runtime/"*"/package_info.cs") git checkout --theirs -- "$f" && git add "$f" ;;
      *) rest="$rest $f" ;;
    esac
  done
  if [ -n "$rest" ]; then echo "CONFLICT $b (unresolved):$rest"; exit 1; fi
  git commit -q -S --no-edit || { echo "COMMIT-FAILED $b"; exit 1; }
  echo "OK*  $b $sha -> $(git rev-parse --short=10 HEAD) (pre-resolved: $U)"
done < "$SD/tG-seats.txt"
resolve_from 64209be705 src/go2cs/manualTypeOperations.go src/core/runtime/heap_test.cs src/core/runtime/rand_test.cs
git diff --cached --quiet && echo "FIXUP: nothing to change" || git commit -q -S -m "TRAIN G fixup: the ReadTrace registry block follows StopTrace again, and P1's four linkname forwarders (heapObjectsCanMove, fastrand, fastrandn, fastrand64) carry StackTraceHidden as the union converter emits them" -m "Both from the adversarial merge-hazard review of the rehearsal (13 files, 0 blockers): the hand resolution of manualTypeOperations.go at g-managed-tracer had separated the seat's ReadTrace comment from the StopTrace line it refers to; p1-runtime-linkname was cut before master's 5d8e113e4b, so its committed test-source forwarders lacked the attribute its converter now adds. No emission or behaviour changes." -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
echo "tip=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 HEAD^{tree})"
