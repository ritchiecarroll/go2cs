#!/usr/bin/env bash
# =================================================================================================
# fleet-watch.sh -- a lane's zero-token idle wait (PROTOCOL v4, for lanes on the mailbox only).
#
#   fleet-watch.sh <LANE> <SINCE>
#
# Run it IN THE BACKGROUND when a turn has nothing left to do. It fetches claude/mailbox every 60 s
# and EXITS the moment a file is ADDED under docs/phase4/inbox/<LANE>/ or docs/phase4/inbox/FLEET/
# after SINCE, the NEXT-SINCE your last fleet-read.sh printed. The background task's completion
# wakes you; then run `fleet-read.sh <LANE> <SINCE>` with the SAME SINCE to read the messages and
# get the next cursor. Waiting costs no tokens. After FLEET_WATCH_MAX seconds (default 1500, under
# the 30-minute clamp some harnesses put on a background task) it exits IDLE: re-arm it with the
# same SINCE. It is read-only: it fetches, and never pulls, checks out or writes.
#
# This is a TOOL, not a design surface: a lane that finds a defect reports it to COORD in one line
# and keeps working; COORD owns the fix (owner order 2026-09-28).
#
# Env: FLEET_MAILBOX_CLONE (a clone of the repo; default: the clone this is run from);
#      FLEET_WATCH_MAX (seconds before the IDLE exit).
# =================================================================================================
set -u
LANES=" COORD C1 C2 R G i9 P1 P2 "
[ "$#" -eq 2 ] || { echo "usage: fleet-watch.sh <LANE> <SINCE>   LANE one of:$LANES" >&2; exit 2; }
LANE="$1"; SINCE="$2"
case "$LANES" in *" $LANE "*) ;; *) echo "REFUSED: '$LANE' is not a lane. One of:$LANES" >&2; exit 2 ;; esac

CLONE="${FLEET_MAILBOX_CLONE:-$(git rev-parse --show-toplevel 2>/dev/null)}"
{ [ -n "$CLONE" ] && git -C "$CLONE" rev-parse --git-dir >/dev/null 2>&1; } || { echo "REFUSED: not inside a clone and FLEET_MAILBOX_CLONE is unset" >&2; exit 2; }

REF="refs/remotes/origin/claude/mailbox"
fetch() { git -C "$CLONE" fetch --quiet origin "+refs/heads/claude/mailbox:$REF" >/dev/null 2>&1; }
fetch
BASE="$(git -C "$CLONE" rev-parse --verify --quiet "$SINCE^{commit}")" || BASE=""
[ -n "$BASE" ] || { echo "REFUSED: SINCE '$SINCE' is not a commit in this clone -- pass the NEXT-SINCE fleet-read.sh printed" >&2; exit 2; }

MAX="${FLEET_WATCH_MAX:-1500}"; START="$(date +%s)"; FAILS=0
echo "WATCH $LANE armed $(date -u +%H:%M:%SZ) from ${BASE:0:10}"
while :; do
    if fetch; then FAILS=0; else FAILS=$((FAILS + 1)); [ "$FAILS" -ge 5 ] && echo "  note: $FAILS consecutive fetch failures" >&2; fi
    TIP="$(git -C "$CLONE" rev-parse --verify --quiet "$REF^{commit}")"
    if [ -n "$TIP" ] && [ "$TIP" != "$BASE" ]; then
        NEW="$(git -C "$CLONE" diff --name-only --diff-filter=A --no-renames "$BASE" "$TIP" -- "docs/phase4/inbox/$LANE/" "docs/phase4/inbox/FLEET/" | grep -v '/README\.md$')"
        if [ -n "$NEW" ]; then
            echo "NEW $(date -u +%H:%M:%SZ):"
            echo "$NEW"
            echo "NOW RUN: fleet-read.sh $LANE $SINCE"
            exit 0
        fi
    fi
    if [ $(( $(date +%s) - START )) -ge "$MAX" ]; then
        echo "IDLE $(date -u +%H:%M:%SZ): nothing new for $LANE -- re-arm with the same SINCE"
        exit 0
    fi
    sleep 60
done
