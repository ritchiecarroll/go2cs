#!/usr/bin/env bash
# coord-resume-verify.sh -- every `BRANCH:` line in a RESUME-SESSIONS file names a SHA that is on origin.
#
#   bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md [remote]
#
# Reads lines of the exact STATE BLOCK shape the save-state skill orders:
#   BRANCH: <name> <sha40> <yes|no> <state> -- <clause>
# For each: fetches the ref from the remote and asserts the SHA is reachable from it (the branch may
# have moved PAST the SHA; it must still contain it).  `LOCAL-ONLY:` lines are listed, never verified --
# they are the never-push content and the block must name how each is preserved.  Exit 1 on any miss.
# A resume that finds a branch this file does not mention has found a mystery; a file that names a SHA
# origin does not have has recorded a wish.
set -u
f="${1:?resume file}"; remote="${2:-origin}"
[ -s "$f" ] || { echo "MISSING FILE: $f"; exit 2; }
n=0; miss=0; local_only=0
while IFS= read -r raw; do
  line="${raw#"${raw%%[![:space:]]*}"}"   # the keys sit indented inside each lane's prompt block
  case "$line" in
    BRANCH:*)
      set -- $line; name="$2"; sha="$3"; claimed="$4"
      n=$((n+1))
      if ! printf '%s' "$sha" | grep -qE '^[0-9a-f]{40}$'; then
        echo "  BAD SHA   $name [$sha] -- 40 hex expected"; miss=$((miss+1)); continue
      fi
      if ! git fetch -q "$remote" "refs/heads/$name" 2>/dev/null; then
        echo "  NO REF    $name -- not on $remote (block claimed on-origin=$claimed)"; miss=$((miss+1)); continue
      fi
      if git merge-base --is-ancestor "$sha" FETCH_HEAD 2>/dev/null; then
        tip=$(git rev-parse --short FETCH_HEAD)
        if [ "$(git rev-parse FETCH_HEAD)" = "$sha" ]; then echo "  OK        $name @$tip (tip)"; else echo "  OK        $name contains ${sha:0:9} (tip $tip, moved past it)"; fi
      else
        echo "  MISSING   $name -- $remote has the ref but NOT ${sha:0:9}"; miss=$((miss+1))
      fi ;;
    LOCAL-ONLY:*)
      local_only=$((local_only+1)); echo "  LOCAL-ONLY (listed, not verifiable from origin): ${line#LOCAL-ONLY: }" ;;
  esac
done < "$f"
echo "branches=$n missing=$miss local-only=$local_only"
[ "$n" -gt 0 ] || { echo "REFUSED: no BRANCH: lines read -- the reader is dead or the file is empty"; exit 1; }
[ "$miss" = "0" ] || exit 1
exit 0
