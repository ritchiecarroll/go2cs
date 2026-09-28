#!/usr/bin/env bash
# TRAIN H rehearsal: signed --no-ff seat merges onto master in tH-seats.txt order, in the rehearsal worktree.
# runtime/<goos>/package_info.cs map conflicts take the incoming side (re-derived in a fixup after the last seat);
# every other conflict STOPS the run and is listed, so it can be pre-resolved by hand and the run continued with $1 = the seat that stopped.
S=$(cd "$(dirname "$0")" && pwd); start=${1:-}; go=0; [ -z "$start" ] && go=1
W=${W:-/h/go2cs-tmp-coord/tH-reh}
cd "$W" || exit 1
while IFS='|' read -r b sha m; do
  [ -z "$b" ] && continue
  if [ $go = 0 ]; then [ "$b" = "$start" ] && go=1; continue; fi
  if git merge -q --no-ff -S -m "rehearsal: $b ($sha)" "$sha" >/dev/null 2>&1; then echo "clean  $b -> $(git rev-parse --short=10 HEAD)"; continue; fi
  rest=""
  for f in $(git diff --name-only --diff-filter=U); do
    case "$f" in src/core/runtime/*/package_info.cs) git checkout --theirs -- "$f" && git add "$f" ;; *) rest="$rest $f" ;; esac
  done
  if [ -n "$rest" ]; then echo "CONFLICT $b:$rest"; exit 2; fi
  git commit -q -S --no-edit && echo "maps   $b -> $(git rev-parse --short=10 HEAD)"
done < "$S/tH-seats.txt"
echo "REHEARSAL tip=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 HEAD^{tree})"
