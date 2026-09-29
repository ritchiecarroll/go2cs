#!/usr/bin/env bash
# COORD inbox watcher (for the cloud lanes C1, C2, P1, P2, which cannot SendMessage back): fetch claude/mailbox every
# 60 s and EXIT the moment a file is ADDED under docs/phase4/inbox/COORD/ (so the background task's completion wakes
# COORD). The cursor is a mailbox TIP (a position, never a time); COORD re-arms it after reading. Read-only on the clone:
# it fetches, never pulls, so COORD's own posts are never raced.
M=/h/Projects/go2cs-mailbox
CUR=${COORD_SCRATCH:?set COORD_SCRATCH to the COORD scratch directory}/watch-cursor.txt
git -C "$M" fetch -q --no-write-fetch-head origin claude/mailbox 2>/dev/null
base=$(cat "$CUR" 2>/dev/null); [ -n "$base" ] || base=$(git -C "$M" rev-parse origin/claude/mailbox)
echo "WATCH armed $(date '+%H:%M:%S') from $(echo $base | cut -c1-10)"
while true; do
  git -C "$M" fetch -q --no-write-fetch-head origin claude/mailbox 2>/dev/null
  tip=$(git -C "$M" rev-parse origin/claude/mailbox)
  if [ "$tip" != "$base" ]; then
    new=$(git -C "$M" diff --name-only --diff-filter=A "$base" "$tip" -- docs/phase4/inbox/COORD/ | grep -v README.md)
    echo "$tip" > "$CUR"
    if [ -n "$new" ]; then echo "NEW $(date '+%H:%M:%S') at $(echo $tip | cut -c1-10):"; echo "$new"; exit 0; fi
    base=$tip
  fi
  sleep 60
done
