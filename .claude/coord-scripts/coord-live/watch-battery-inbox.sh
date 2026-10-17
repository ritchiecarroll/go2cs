#!/bin/bash
# TRAIN P: watch the battery console and the COORD inbox. Exits on an event, so the exit wakes COORD.
#   usage: watch-battery-inbox.sh <40-char anchor = the last mailbox tip READ> [once|inbox]
#   "once"  = run the inbox arm a single time and exit (the positive control).
#   "inbox" = no battery: watch the COORD inbox only.
# BATCHING (each wake-up of COORD costs a whole-context turn): a battery event exits at once; an inbox post exits
# at once only when its subject is URGENT-class, and otherwise after INBOX_BATCH_MIN minutes from the first unread
# post, so several lane posts are read in one turn. INBOX_BATCH_MIN=0 restores exit-on-first-post.
F=${BATTERY_LOG:-/h/go2cs-tmp-coord/coord-scratch/tP/run2/battery.console.log}
BATCH_MIN=${INBOX_BATCH_MIN:-25}
URGENT_RE=${INBOX_URGENT_RE:-'URGENT|STOP|HOLD|ABORT|BLOCKED|OWNER-HAND|SAVED|SAVE-STATE|TRAIN P|shard|linux legs|P gate|release gate|RED'}
A="$1"
[ ${#A} -eq 40 ] || { echo "REFUSED: anchor length ${#A}, need 40"; exit 2; }
cd /h/Projects/go2cs-mailbox || { echo "REFUSED: no mailbox clone"; exit 2; }
git cat-file -e "$A^{commit}" 2>/dev/null || { echo "REFUSED: anchor is not a commit here"; exit 2; }

LASTTIP=""; NPOSTS=0; FIRST_SEEN=0
inbox_poll() { # sets NPOSTS; rc 0 = polled, 3 = the remote could not be read
  local tip
  tip=$(git ls-remote origin refs/heads/claude/mailbox 2>/dev/null | cut -c1-40)
  [ ${#tip} -eq 40 ] || return 3
  if [ "$tip" != "$LASTTIP" ]; then
    git fetch -q origin claude/mailbox 2>/dev/null || return 3
    LASTTIP="$tip"
    NPOSTS=$(git log --format=%h "$A..origin/claude/mailbox" -- docs/phase4/inbox/COORD | wc -l)
    [ "$NPOSTS" -gt 0 ] && [ "$FIRST_SEEN" = 0 ] && FIRST_SEEN=$(date +%s)
  fi
  return 0
}
inbox_due() { # rc 0 = report now
  [ "$NPOSTS" -gt 0 ] || return 1
  git log --format=%s "$A..origin/claude/mailbox" -- docs/phase4/inbox/COORD | grep -qE -- "$URGENT_RE" && return 0
  [ $(( $(date +%s) - FIRST_SEEN )) -ge $(( BATCH_MIN * 60 )) ]
}
inbox_report() {
  echo "COORD INBOX $(date +%H:%M): $NPOSTS post(s) since the anchor (batch window ${BATCH_MIN}m)"
  git log --reverse --format='%h %s' "$A..origin/claude/mailbox" -- docs/phase4/inbox/COORD | cut -c1-220
}

if [ "${2:-}" = "once" ]; then inbox_poll; prc=$?; if [ "$NPOSTS" -gt 0 ]; then inbox_report; inbox_due; echo "due-now rc=$?"; fi; echo "inbox poll rc=$prc posts=$NPOSTS"; exit 0; fi

BATTERY=1; [ "${2:-}" = "inbox" ] && BATTERY=0
legs_nonzero() { grep -a -E '^[0-9:]+ LEG ' "$F" | grep -a -v -E 'LEG (FXc1|FXc2|SIc|NVR) ' | grep -ac -v ' rc=0 '; }
if [ $BATTERY = 1 ]; then N0=$(grep -ac 'FINDING' "$F"); R0=$(legs_nonzero); echo "armed $(date +%H:%M) anchor=$A findings=$N0 nonzero-legs=$R0 batch=${BATCH_MIN}m"; else echo "armed $(date +%H:%M) inbox-only anchor=$A batch=${BATCH_MIN}m"; fi
blind=0
for i in $(seq 1 115); do
  sleep 60
  if [ $BATTERY = 1 ]; then
    if grep -aq 'battery rc=' "$F"; then echo "BATTERY ENDED $(date +%H:%M): $(grep -a 'battery rc=' "$F" | tail -n 1)"; inbox_poll; [ "$NPOSTS" -gt 0 ] && inbox_report; exit 0; fi
    N=$(grep -ac 'FINDING' "$F")
    if [ "$N" != "$N0" ]; then echo "NEW FINDING $(date +%H:%M): $N0 -> $N"; grep -a 'FINDING' "$F" | tail -n 3 | cut -c1-240; inbox_poll; [ "$NPOSTS" -gt 0 ] && inbox_report; exit 0; fi
    R=$(legs_nonzero)
    if [ "$R" != "$R0" ]; then echo "NEW NON-ZERO LEG $(date +%H:%M): $R0 -> $R"; grep -a -E '^[0-9:]+ LEG ' "$F" | grep -a -v ' rc=0 ' | tail -n 3 | cut -c1-240; inbox_poll; [ "$NPOSTS" -gt 0 ] && inbox_report; exit 0; fi
  fi
  inbox_poll; rc=$?
  if [ $rc -eq 3 ]; then blind=$((blind+1)); [ $blind -eq 5 ] && echo "MAILBOX UNREACHABLE x5 $(date +%H:%M) (watcher is blind, not quiet)"; else blind=0; fi
  if inbox_due; then inbox_report; exit 0; fi
done
[ "$NPOSTS" -gt 0 ] && inbox_report
echo "WATCH WINDOW ENDED $(date +%H:%M): no event in 115 minutes"
