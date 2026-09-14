#!/usr/bin/env bash
# coord-mbmon-r3-run1.sh -- PERSISTENT mailbox watcher (PROTOCOL v3.6 form), derived 2026-09-13 from
# coord-mbmon-template.sh in the MAIN checkout's .claude/coord-scripts/ (path asserted by the launcher).
# Differences from the template: never exits on MOVED or on a poll count; advances its own remembered
# anchor after each event so one move fires once; stamps HISTORY REWRITTEN when the previous anchor is
# not an ancestor of the new tip; emits ONLY event lines (the Monitor tool turns each into a notification).
ANCHOR="__ANCHOR__"
REPO="C:/Projects/go2cs-mailbox"
INTERVAL=60
SENTINEL="$(printf '__%s__' ANCHOR)"   # built at runtime so the per-run sed cannot touch it
if [ "$ANCHOR" = "$SENTINEL" ]; then echo "MBMON REFUSED: anchor not substituted"; exit 2; fi
if [ ${#ANCHOR} -ne 40 ]; then echo "MBMON REFUSED: anchor is ${#ANCHOR} chars, not 40"; exit 2; fi
cd "$REPO" || { echo "MBMON REFUSED: no clone at $REPO"; exit 2; }
echo "MBMON ARMED pid=$$ anchor=$ANCHOR interval=${INTERVAL}s at $(date '+%Y-%m-%d %H:%M:%S')"
i=0; fails=0
while true; do
  i=$((i+1))
  TIP="$(git --no-optional-locks ls-remote origin refs/heads/claude/mailbox 2>/dev/null | cut -f1)"
  rc=$?
  if [ $rc -ne 0 ] || [ -z "$TIP" ]; then
    fails=$((fails+1))
    if [ $((fails % 10)) -eq 1 ]; then echo "MBMON LS-REMOTE FAILED rc=$rc (consecutive $fails) at $(date '+%H:%M:%S')"; fi
  else
    fails=0
    if [ "$TIP" != "$ANCHOR" ]; then
      echo "MAILBOX MOVED: $ANCHOR -> $TIP (poll $i, $(date '+%Y-%m-%d %H:%M:%S'))"
      git --no-optional-locks fetch -q origin claude/mailbox 2>/dev/null
      if git --no-optional-locks merge-base --is-ancestor "$ANCHOR" "$TIP" 2>/dev/null; then
        echo "ANCESTRY OK: $ANCHOR is an ancestor of $TIP"
      else
        echo "HISTORY REWRITTEN: previous anchor $ANCHOR is NOT an ancestor of $TIP -- a post may have been DROPPED; read git log --graph before believing the tip"
      fi
      git --no-optional-locks log --format='  %h %ad %s' --date=format-local:'%m-%d %H:%M' "$ANCHOR..$TIP" 2>/dev/null | cut -c1-200
      ANCHOR="$TIP"
    fi
  fi
  sleep $INTERVAL
done
