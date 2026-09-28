#!/usr/bin/env bash
# =================================================================================================
# fleet-read.sh -- PROTOCOL v4 (owner order 2026-09-20 13:15). THE TICK'S FIRST STEP.
#
#   fleet-read.sh <LANE> [SINCE]
#
# THE CURSOR IS A POSITION, NEVER A TIME. SINCE is the mailbox TIP SHA this lane last read, and
# everything printed is the commit range SINCE..<the tip after the fetch>: the inbox files ADDED in
# that range under docs/phase4/inbox/<LANE>/ and docs/phase4/inbox/FLEET/, then the lines ADDED to
# docs/phase4/LEDGER.md in that range, then `NEXT-SINCE <the tip it read>` for the caller to store
# and pass back next tick. Because it prints the tip whose CONTENT it printed, anything landing
# while the read runs is read next tick rather than skipped, and no clock -- not the poster's, not
# the reader's, not the one in a filename or a ledger stamp -- decides what a lane sees.
#
# WHAT A TIME CURSOR COST, all of it on 2026-09-20 (C1 and C2 measured it independently, three arms
# each, at mailbox f3918a2001):
#   * A ledger line stamped EARLIER than the cursor but appended AFTER it was permanently invisible:
#     `fleet-read.sh C2 20260920T155500Z` printed LEDGER 0 with the line sitting in the file at that
#     tip, while 20260920T135900Z printed it. A back-stamped correction is exactly the line a lane
#     most needs -- the first one the fleet wrote was the time correction itself.
#   * The same hole on the inbox: a message whose FILENAME sorts before names the lane had already
#     consumed could never be seen again, which is what a poster's box running behind the reader's
#     produces on its own (clock skew needs no mistake to happen).
#   * Eight ledger lines stamped 14:15-15:55 while the clock read 13:2x-13:5x handed every lane that
#     derived NEXT-SINCE from max(stamp) a cursor HOURS IN THE FUTURE, which reads LEDGER 0 for as
#     long as it stands and is indistinguishable from a quiet channel.
#
# Legacy: a 14-digit time cursor (20260920T175208Z | 20260920-175208 | 20260920) is accepted for one
# transition and RESOLVED TO A POSITION -- the last mailbox commit whose committer time is at or
# before it -- so a lane mid-tick keeps working. A time LATER than this box's `date -u` is not a
# cursor any lane can legitimately hold: it is repaired to three hours ago, out loud, on one line.
#
# With no SINCE: the ledger's last 20 lines and the lane's 10 newest files.
#
# READ-ONLY AND WRITES NOTHING, ANYWHERE: every read is a `git show` / `git diff` / `git ls-tree`
# against origin/claude/mailbox, so no checkout, no working tree and no temp file are needed. Two
# filename spellings are accepted -- 20260920T175208Z-C2.md (what fleet-msg.sh writes, and what the
# first hand-written files used) and 20260920-175208-C2.md -- because both are already on the branch
# and a mixed directory must still sort chronologically: both normalise to the same 14-digit key
# before anything is compared. Names order the printing; they never decide membership.
#
# Env: FLEET_MAILBOX_CLONE (a clone of the repo; default: the clone this is run from).
# =================================================================================================
set -u

PROG="fleet-read.sh"
LANES=" COORD C1 C2 R G i9 P1 P2 FLEET "
REF="origin/claude/mailbox"
LEDGERP="docs/phase4/LEDGER.md"

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
# that is not a message is skipped without a name list to keep in step. It ORDERS the output only.
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
TIP="$(git -C "$CLONE" rev-parse --verify --quiet "$REF^{commit}" 2>/dev/null)"; rc=$?
{ [ "$rc" -eq 0 ] && [ -n "$TIP" ]; } || die "$REF does not exist in '$CLONE' -- nothing to read"

# ---- SINCE -> a POSITION ------------------------------------------------------------------------
# A strict legacy spelling (it carries a T...Z or a dash) is read as a time before anything else; a
# bare digit run is ambiguous -- 20260920 is also eight hex characters -- so a commit that resolves
# wins and only then is it read as a date.
BASE=""
if [ -n "$SINCE" ]; then
    SLEGACY=0
    case "$SINCE" in
        [0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]T[0-9][0-9][0-9][0-9][0-9][0-9]Z) SLEGACY=1 ;;
        [0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9])  SLEGACY=1 ;;
    esac
    if [ "$SLEGACY" -eq 0 ]; then
        BASE="$(git -C "$CLONE" rev-parse --verify --quiet "$SINCE^{commit}" 2>/dev/null)"; rc=$?
        { [ "$rc" -eq 0 ] && [ -n "$BASE" ]; } || BASE=""
    fi
    if [ -z "$BASE" ]; then
        SN="$(norm "$SINCE")"
        [ "$SN" != "00000000000000" ] || die "SINCE '$SINCE' is neither a commit in this clone nor a timestamp -- pass the NEXT-SINCE of the last tick"
        NOW="$(date -u +%Y%m%d%H%M%S)"; rc=$?
        { [ "$rc" -eq 0 ] && [ "${#NOW}" -eq 14 ]; } || die "could not read this box's clock"
        # A FUTURE CURSOR IS REPAIRED, LOUDLY: no lane can legitimately hold one, and the one the
        # mis-stamped ledger lines handed out would have read LEDGER 0 for hours without a word.
        if [[ "$SN" > "$NOW" ]]; then
            BACK="$(date -u -d "@$(( $(date -u +%s) - 10800 ))" +%Y%m%d%H%M%S 2>/dev/null)"; rc=$?
            { [ "$rc" -eq 0 ] && [ "${#BACK}" -eq 14 ]; } || die "could not read this box's clock"
            echo "SINCE $SINCE is in the future -- repaired to $(canon "$BACK")"
            SN="$BACK"
        fi
        ISO="${SN:0:4}-${SN:4:2}-${SN:6:2} ${SN:8:2}:${SN:10:2}:${SN:12:2} +0000"
        BASE="$(git -C "$CLONE" log -1 --before="$ISO" --format=%H "$TIP" 2>/dev/null)"; rc=$?
        [ "$rc" -eq 0 ] || die "could not resolve the legacy cursor '$SINCE' to a position on $REF"
        [ -n "$BASE" ] || echo "  note: no mailbox commit at or before '$SINCE' -- reading the head of the channel" >&2
    else
        git -C "$CLONE" merge-base --is-ancestor "$BASE" "$TIP" >/dev/null 2>&1; rc=$?
        [ "$rc" -eq 0 ] || die "SINCE $SINCE is not an ancestor of $REF ($TIP) -- it is not a position on this channel"
    fi
fi

# ---- the inbox: the files ADDED in the range, ordered by name ------------------------------------
DIRS="docs/phase4/inbox/$LANE/ docs/phase4/inbox/FLEET/"
[ "$LANE" = "FLEET" ] && DIRS="docs/phase4/inbox/FLEET/"

# $DIRS is deliberately unquoted: it is two pathspecs, not one path with a space in it.
if [ -n "$BASE" ]; then
    RAW="$(git -C "$CLONE" diff --name-only --diff-filter=A --no-renames "$BASE" "$TIP" -- $DIRS 2>/dev/null)"; rc=$?
    [ "$rc" -eq 0 ] || die "could not list the files added between $BASE and $TIP"
else
    RAW="$(git -C "$CLONE" ls-tree -r --name-only "$TIP" -- $DIRS 2>/dev/null)"; rc=$?
    [ "$rc" -eq 0 ] || die "could not list the inbox at $TIP"
fi

LIST=""
while IFS= read -r p || [ -n "$p" ]; do
    [ -n "$p" ] || continue
    s="$(stamp_of "${p##*/}")"
    [ -n "$s" ] || continue
    LIST="$LIST$(norm "$s")"$'\t'"$p"$'\n'
done <<< "$RAW"

MSGS=0
if [ -n "$LIST" ]; then
    SORTED="$(sort <<< "$LIST")"; rc=$?
    [ "$rc" -eq 0 ] || die "could not order the messages"
    SKIP=0
    if [ -z "$BASE" ]; then
        # No cursor: the lane's 10 newest files, so a first tick is a read and not a download.
        N=0
        while IFS= read -r _l || [ -n "$_l" ]; do [ -n "$_l" ] && N=$((N + 1)); done <<< "$SORTED"
        [ "$N" -gt 10 ] && SKIP=$((N - 10))
    fi
    I=0
    while IFS=$'\t' read -r k p || [ -n "$k" ]; do
        [ -n "$p" ] || continue
        I=$((I + 1))
        [ "$I" -gt "$SKIP" ] || continue
        echo "===== $p"
        gshow "$CLONE" "$TIP:$p" 2>/dev/null; rc=$?
        [ "$rc" -eq 0 ] || echo "  (unreadable at $TIP: rc=$rc)"
        echo
        MSGS=$((MSGS + 1))
    done <<< "$SORTED"
fi

# ---- the ledger: the lines ADDED in the range, whatever they are stamped -------------------------
LINES=0
OUT=""
add_line() { local l="${1%$'\r'}"; OUT="$OUT$l"$'\n'; LINES=$((LINES + 1)); }
if [ -n "$BASE" ]; then
    DIFF="$(git -C "$CLONE" diff --no-color --no-ext-diff --unified=0 "$BASE" "$TIP" -- "$LEDGERP" 2>/dev/null)"; rc=$?
    [ "$rc" -eq 0 ] || die "could not diff $LEDGERP between $BASE and $TIP"
    while IFS= read -r line || [ -n "$line" ]; do
        case "$line" in
            +++*) continue ;;
            +*)   add_line "${line#+}" ;;
        esac
    done <<< "$DIFF"
else
    LEDGER="$(gshow "$CLONE" "$TIP:$LEDGERP" 2>/dev/null)"; rc=$?
    if [ "$rc" -eq 0 ] && [ -n "$LEDGER" ]; then
        TAIL="$(tail -n 20 <<< "$LEDGER")"; rc=$?
        [ "$rc" -eq 0 ] || die "could not read the tail of $LEDGERP"
        while IFS= read -r line || [ -n "$line" ]; do
            [ -n "${line%$'\r'}" ] && add_line "$line"
        done <<< "$TAIL"
    fi
fi
if [ "$LINES" -gt 0 ]; then
    echo "===== $LEDGERP"
    printf '%s' "$OUT"
    echo
fi

echo "MESSAGES $MSGS"
echo "LEDGER $LINES"
echo "NEXT-SINCE $TIP"
exit 0
