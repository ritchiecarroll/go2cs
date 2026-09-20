#!/usr/bin/env bash
# =================================================================================================
# fleet-read.sh -- PROTOCOL v4 (owner order 2026-09-20 13:15). THE TICK'S FIRST STEP.
#
#   fleet-read.sh <LANE> [SINCE]
#
# Prints, in time order: every message in docs/phase4/inbox/<LANE>/ and docs/phase4/inbox/FLEET/
# whose timestamp is AFTER SINCE, then the lines of docs/phase4/LEDGER.md after SINCE, then
# `NEXT-SINCE <stamp>` for the caller to store and pass back next tick. With no SINCE it prints
# everything. Nothing is read "whole since an anchor" and no census runs over the channel.
#
# READ-ONLY AND WRITES NOTHING, ANYWHERE: every read is `git show origin/claude/mailbox:<path>`, so
# no checkout, no working tree and no temp file are needed. Two filename spellings are accepted --
# 20260920T175208Z-C2.md (what fleet-msg.sh writes, and what the first hand-written files used) and
# 20260920-175208-C2.md -- because both are already on the branch and a mixed directory must still
# sort chronologically: both normalise to the same 14-digit key before anything is compared.
#
# Env: FLEET_MAILBOX_CLONE (a clone of the repo; default: the clone this is run from).
# =================================================================================================
set -u

PROG="fleet-read.sh"
LANES=" COORD C1 C2 R G i9 FLEET "
REF="origin/claude/mailbox"

die() { echo "REFUSED: $*" >&2; exit 2; }
# The cd is the SHELL's and the no-path-conversion is GIT's, and they cannot be combined on one
# command line: under MSYS, `MSYS_NO_PATHCONV=1 git -C /c/...` hands git.exe an unconverted POSIX
# directory it cannot chdir to and every read exits 128.
gshow() { ( cd -- "$1" && MSYS_NO_PATHCONV=1 git show "$2" ); }

[ "$#" -ge 1 ] && [ "$#" -le 2 ] || { echo "usage: $PROG <LANE> [SINCE]   LANE one of:$LANES" >&2; exit 2; }
LANE="$1"; SINCE="${2:-}"
case "$LANES" in *" $LANE "*) ;; *) die "'$LANE' is not a lane. One of:$LANES" ;; esac

# yyyymmddTHHMMSSZ | yyyymmdd-HHMMSS | "YYYY-MM-DD HH:MM" | yyyymmdd  ->  yyyymmddHHMMSS
norm() {
    local d="${1//[^0-9]/}"
    case "${#d}" in
        0)  printf '00000000000000' ;;
        8)  printf '%s000000' "$d" ;;
        12) printf '%s00' "$d" ;;
        *)  printf '%s' "${d:0:14}" ;;
    esac
}
canon() { printf '%sT%sZ' "${1:0:8}" "${1:8:6}"; }
# The timestamp PREFIX of a message filename, or empty -- which is how README.md and anything else
# that is not a message is skipped without a name list to keep in step.
stamp_of() {
    case "$1" in
        [0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]T[0-9][0-9][0-9][0-9][0-9][0-9]Z*) printf '%s' "${1:0:16}" ;;
        [0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9]*)  printf '%s' "${1:0:15}" ;;
        *) printf '' ;;
    esac
}

CLONE="${FLEET_MAILBOX_CLONE:-}"
if [ -z "$CLONE" ]; then
    CLONE="$(git rev-parse --show-toplevel 2>/dev/null)"; rc=$?
    { [ "$rc" -eq 0 ] && [ -n "$CLONE" ]; } || die "not inside a clone and FLEET_MAILBOX_CLONE is unset"
fi
git -C "$CLONE" rev-parse --git-dir >/dev/null 2>&1; rc=$?
[ "$rc" -eq 0 ] || die "'$CLONE' is not a git clone"
git -C "$CLONE" fetch --quiet origin "+refs/heads/claude/mailbox:refs/remotes/origin/claude/mailbox" >/dev/null 2>&1; frc=$?
[ "$frc" -eq 0 ] || echo "  note: the fetch failed (rc=$frc) -- reading the local $REF, which may be behind" >&2
git -C "$CLONE" rev-parse --verify --quiet "$REF" >/dev/null 2>&1; rc=$?
[ "$rc" -eq 0 ] || die "$REF does not exist in '$CLONE' -- nothing to read"

SN="$(norm "$SINCE")"; NEWEST="$SN"
DIRS="$LANE FLEET"
[ "$LANE" = "FLEET" ] && DIRS="FLEET"

LIST=""
for d in $DIRS; do
    raw="$(git -C "$CLONE" ls-tree --name-only "$REF" -- "docs/phase4/inbox/$d/" 2>/dev/null)"; rc=$?
    [ "$rc" -eq 0 ] && [ -n "$raw" ] || continue
    while IFS= read -r p || [ -n "$p" ]; do
        [ -n "$p" ] || continue
        s="$(stamp_of "${p##*/}")"
        [ -n "$s" ] || continue
        k="$(norm "$s")"
        if [[ "$k" > "$SN" ]]; then LIST="$LIST$k"$'\t'"$p"$'\n'; fi
    done <<< "$raw"
done

MSGS=0
if [ -n "$LIST" ]; then
    SORTED="$(sort <<< "$LIST")"; rc=$?
    [ "$rc" -eq 0 ] || die "could not order the messages"
    while IFS=$'\t' read -r k p || [ -n "$k" ]; do
        [ -n "$p" ] || continue
        echo "===== $p"
        gshow "$CLONE" "$REF:$p" 2>/dev/null; rc=$?
        [ "$rc" -eq 0 ] || echo "  (unreadable at $REF: rc=$rc)"
        echo
        if [[ "$k" > "$NEWEST" ]]; then NEWEST="$k"; fi
        MSGS=$((MSGS + 1))
    done <<< "$SORTED"
fi

LEDGER="$(gshow "$CLONE" "$REF:docs/phase4/LEDGER.md" 2>/dev/null)"; rc=$?
LINES=0
if [ "$rc" -eq 0 ] && [ -n "$LEDGER" ]; then
    echo "===== docs/phase4/LEDGER.md"
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line%$'\r'}"
        case "$line" in
            [0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]\ [0-9][0-9]:[0-9][0-9]\ *) ;;
            *) continue ;;
        esac
        k="$(norm "${line:0:16}")"
        [[ "$k" > "$SN" ]] || continue
        echo "$line"
        if [[ "$k" > "$NEWEST" ]]; then NEWEST="$k"; fi
        LINES=$((LINES + 1))
    done <<< "$LEDGER"
    echo
fi

echo "MESSAGES $MSGS"
echo "LEDGER $LINES"
echo "NEXT-SINCE $(canon "$NEWEST")"
exit 0
