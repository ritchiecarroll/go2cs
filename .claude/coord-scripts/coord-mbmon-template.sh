#!/usr/bin/env bash
set -uo pipefail
ANCHOR="7d4e3dd5f2d48baaf182a521246a1ff19ecb351f"
cd /c/Projects/go2cs-mailbox || { echo "MBMON ABORT: clone missing"; exit 2; }
RS="$(git config --get-all remote.origin.fetch)"
case "$RS" in
  *claude/mailbox*) : ;;
  *) echo "MBMON ABORT: refspec is not mailbox-only: $RS"; exit 3 ;;
esac
echo "MBMON ARMED pid=$$ anchor=$ANCHOR at $(date '+%H:%M:%S')"
i=0
while :; do
  TIP="$(git ls-remote origin claude/mailbox 2>/dev/null | cut -f1)"
  rc=$?
  if [ $rc -eq 0 ] && [ -n "$TIP" ] && [ "$TIP" != "$ANCHOR" ]; then
    echo "MAILBOX MOVED: $ANCHOR -> $TIP  (after ${i} polls, $(date '+%H:%M:%S'))"
    # 1169: a reader that REMEMBERS the previous tip is the only party that can see a dropped post.
    git fetch -q origin claude/mailbox 2>/dev/null
    if git merge-base --is-ancestor "$ANCHOR" "$TIP" 2>/dev/null; then
      echo "ANCESTRY OK: $ANCHOR is an ancestor of $TIP"
    else
      echo "HISTORY REWRITTEN: previous anchor $ANCHOR is NOT an ancestor of $TIP -- a post may have been DROPPED; read git log --graph before believing the tip"
    fi
    exit 0
  fi
  i=$((i+1))
  if [ $i -ge 90 ]; then echo "MBMON TIMEOUT after ${i} polls, tip still $ANCHOR"; exit 1; fi
  sleep 60
done
