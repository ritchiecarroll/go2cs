#!/usr/bin/env bash
# coord-resume-verify.sh -- every `BRANCH:` line in a RESUME-SESSIONS file names a SHA that is on origin.
#
#   bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md [remote]
#
# Reads lines of the exact STATE BLOCK shape the save-state skill orders:
#   BRANCH: <name> <sha40> <yes|no> <state> -- <clause>
# For each: fetches the ref from the remote and asserts the SHA is reachable from it (the branch may
# have moved PAST the SHA; it must still contain it).  A line whose FOURTH field is neither `yes` nor
# `no` carries no on-origin claim at all: it is verified by reach exactly as a `yes` line is, but it
# cannot declare itself local-only, so it is announced UNDECLARED and counted as `undeclared=` beside
# `missing=` -- the fourth word is a content word and must never be read as a claim.
# `LOCAL-ONLY:` lines are listed, never verified --
# they are the never-push content and the block must name how each is preserved.  Exit 1 on any miss.
# A resume that finds a branch this file does not mention has found a mystery; a file that names a SHA
# origin does not have has recorded a wish.
set -u
f="${1:?resume file}"; remote="${2:-origin}"
[ -s "$f" ] || { echo "MISSING FILE: $f"; exit 2; }
n=0; miss=0; local_only=0; landed=0; declared_local=0; undeclared=0
while IFS= read -r raw; do
  line="${raw#"${raw%%[![:space:]]*}"}"   # the keys sit indented inside each lane's prompt block
  case "$line" in
    BRANCH:*)
      set -- $line; name="$2"; sha="$3"; claimed="${4:-}"
      n=$((n+1))
      if ! printf '%s' "$sha" | grep -qE '^[0-9a-f]{40}$'; then
        echo "  BAD SHA   $name [$sha] -- 40 hex expected"; miss=$((miss+1)); continue
      fi
      if [ "$claimed" = "no" ]; then
        # The lane itself declares this ref local-only (never-push content); its preservation is the
        # LOCAL-ONLY line that must accompany it.  Listed, never counted as a miss.
        declared_local=$((declared_local+1)); echo "  DECLARED-LOCAL $name ${sha:0:9} -- the block says on-origin=no; a LOCAL-ONLY line must name its bundle"; continue
      fi
      if [ "$claimed" != "yes" ]; then
        # Neither `yes` nor `no`: field 4 is the first word of the clause, so this line was written
        # WITHOUT the grammar of record's `yes <state> --`.  It makes no claim, and above all it cannot
        # declare itself local-only.  Verified by reach below exactly as a `yes` line is; announced and
        # counted here so the gap between the grammar and the file is visible, never read as a claim.
        undeclared=$((undeclared+1))
        echo "  UNDECLARED $name ${sha:0:9} -- no on-origin field (the line cannot declare itself local; verified by reach)"
      fi
      if ! git fetch -q "$remote" "refs/heads/$name" 2>/dev/null; then
        # A LANDED seat branch is pruned by design after its train lands; its content is then reachable
        # from the default branch.  That is preserved.  Anything else with no ref is a wish.
        [ -n "${MASTER_FETCHED:-}" ] || { git fetch -q "$remote" refs/heads/master 2>/dev/null && MASTER_FETCHED=$(git rev-parse FETCH_HEAD); }
        if [ -n "${MASTER_FETCHED:-}" ] && git merge-base --is-ancestor "$sha" "$MASTER_FETCHED" 2>/dev/null; then
          landed=$((landed+1)); echo "  OK-LANDED $name -- ref pruned; ${sha:0:9} reachable from refs/heads/master as fetched at the act"
        else
          case "$claimed" in
            yes|no) why="(block claimed on-origin=$claimed)" ;;
            *)      why="(on-origin field ABSENT)" ;;
          esac
          echo "  NO REF    $name -- not on $remote and ${sha:0:9} NOT reachable from refs/heads/master as fetched at the act $why"; miss=$((miss+1))
        fi
        continue
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
echo "branches=$n missing=$miss landed-and-pruned=$landed declared-local=$declared_local undeclared=$undeclared local-only=$local_only"
[ "$n" -gt 0 ] || { echo "REFUSED: no BRANCH: lines read -- the reader is dead or the file is empty"; exit 1; }
[ "$miss" = "0" ] || exit 1
exit 0
