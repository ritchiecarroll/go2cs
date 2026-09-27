#!/usr/bin/env bash
# Continue the TRAIN F rehearsal from the seat after $1 (a branch name), signed merges; runtime package_info maps take theirs.
S=$(dirname "$0"); start=${1:-}; go=0; [ -z "$start" ] && go=1
cd /h/go2cs-tmp-coord/tF-reh || exit 1
while IFS='|' read -r b sha m; do
  if [ $go = 0 ]; then [ "$b" = "$start" ] && go=1; continue; fi
  if git merge -q --no-ff -S -m "rehearsal: $b" "$sha" >/dev/null 2>&1; then echo "clean  $b -> $(git rev-parse --short=10 HEAD)"; continue; fi
  rest=""
  for f in $(git diff --name-only --diff-filter=U); do
    case "$f" in src/core/runtime/*/package_info.cs) git checkout --theirs -- "$f" && git add "$f" ;; *) rest="$rest $f" ;; esac
  done
  if [ -n "$rest" ]; then echo "CONFLICT $b:$rest"; exit 2; fi
  git commit -q -S --no-edit && echo "maps   $b -> $(git rev-parse --short=10 HEAD)"
done < "$S/tF-seats.txt"
echo "REHEARSAL tip=$(git rev-parse --short=10 HEAD)"
