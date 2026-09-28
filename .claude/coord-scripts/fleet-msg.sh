#!/usr/bin/env bash
# =================================================================================================
# fleet-msg.sh -- PROTOCOL v4 (owner order 2026-09-20 13:15) addressed comms.
#
#   FLEET_LANE=<you> fleet-msg.sh <TO> "<SUBJECT>" <FILE>
#
# ONE message is ONE NEW FILE under docs/phase4/inbox/<TO>/. A file is never written twice, so there
# are no anchors, no merges and no races: a rejected push is a rebase and a second push, never a
# conflict. The gate is the shared identifier census, materialised from origin/master at every run
# (NO TOOL CARRIES A PRIVATE COPY OF ANY ARM) and run on the OUTGOING message ONLY -- never over the
# inbox, never over the channel. Nothing is written into the worktree until both gates are clean, so
# a refusal leaves the clone byte-identical. Every exit code is captured before any pipe.
#
# Env: FLEET_LANE (required, the sender) · FLEET_MAILBOX_CLONE (a clone with claude/mailbox checked
#      out; default: the clone this is run from) · FLEET_LONG=1 (allow a body over 60 lines).
# =================================================================================================
set -u

PROG="fleet-msg.sh"
LANES=" COORD C1 C2 R G i9 P1 P2 FLEET "
MAXLINES=60
MIN_CENSUS=80000
MIN_PATTERNS=20000
MIN_HASHES=1000

die() { echo "REFUSED: $*" >&2; exit 2; }
usage() { echo "usage: FLEET_LANE=<you> $PROG <TO> \"<SUBJECT>\" <FILE>   TO/FLEET_LANE one of:$LANES" >&2; exit 2; }
# `git show <ref>:<path>` in ONE clone. The cd is the SHELL's and the no-path-conversion is GIT's,
# and they cannot be combined on one command line: under MSYS, `MSYS_NO_PATHCONV=1 git -C /c/...`
# hands git.exe an unconverted POSIX directory it cannot chdir to and every read exits 128. Measured
# 2026-09-20 -- it took the census down to "a gate that cannot load", which is at least a refusal.
gshow() { ( cd -- "$1" && MSYS_NO_PATHCONV=1 git show "$2" ); }

[ "$#" -eq 3 ] || usage
TO="$1"; SUBJECT="$2"; BODY="$3"
FROM="${FLEET_LANE:-}"

case "$LANES" in *" $TO "*) ;; *) die "'$TO' is not a lane. One of:$LANES" ;; esac
[ -n "$FROM" ] || die "FLEET_LANE is unset -- a message with no sender is not a message"
case "$LANES" in *" $FROM "*) ;; *) die "FLEET_LANE='$FROM' is not a lane. One of:$LANES" ;; esac
[ -n "$SUBJECT" ] || die "the subject is empty"
[ -f "$BODY" ] && [ -r "$BODY" ] || die "cannot read '$BODY' -- it cannot know, so it does not pass"

# ---- the clone -----------------------------------------------------------------------------------
CLONE="${FLEET_MAILBOX_CLONE:-}"
if [ -z "$CLONE" ]; then
    CLONE="$(git rev-parse --show-toplevel 2>/dev/null)"; rc=$?
    { [ "$rc" -eq 0 ] && [ -n "$CLONE" ]; } || die "not inside a clone and FLEET_MAILBOX_CLONE is unset"
fi
BR="$(git -C "$CLONE" rev-parse --abbrev-ref HEAD 2>/dev/null)"; rc=$?
[ "$rc" -eq 0 ] || die "'$CLONE' is not a git clone"
[ "$BR" = "claude/mailbox" ] || die "'$CLONE' has '$BR' checked out, not claude/mailbox"
git -C "$CLONE" diff --cached --quiet; rc=$?
[ "$rc" -eq 0 ] || die "the index in '$CLONE' has staged changes -- this tool commits what it stages and will not carry someone else's work"
git -C "$CLONE" diff --quiet; rc=$?
[ "$rc" -eq 0 ] || die "'$CLONE' has modified tracked files -- the push retry rebases, and a dirty tree cannot"

NL="$(awk 'END{print NR}' "$BODY")"; rc=$?
[ "$rc" -eq 0 ] && [ -n "$NL" ] || die "could not count the lines of '$BODY'"
if [ "$NL" -gt "$MAXLINES" ] && [ "${FLEET_LONG:-0}" != "1" ]; then
    die "'$BODY' is $NL lines and the budget is $MAXLINES. Put long evidence on a ref and name it by path and blob id, or set FLEET_LONG=1."
fi

# ---- scratch, INSIDE the clone (this tool writes nothing outside it) -------------------------------
GITDIR="$(git -C "$CLONE" rev-parse --absolute-git-dir 2>/dev/null)"; rc=$?
{ [ "$rc" -eq 0 ] && [ -n "$GITDIR" ]; } || die "could not resolve the git directory of '$CLONE'"
TMPD="$GITDIR/fleet-msg.tmp.$$"
cleanup() { if [ -n "${TMPD:-}" ] && [ -d "$TMPD" ]; then rm -rf -- "$TMPD"; fi; return 0; }
trap cleanup EXIT INT TERM
mkdir -p -- "$TMPD"; rc=$?
[ "$rc" -eq 0 ] || die "could not create the scratch directory under the git directory"

# ---- compose (still outside the worktree) ---------------------------------------------------------
TS="$(date -u +%Y%m%dT%H%M%SZ)"
ISO="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
MSGFILE="$TMPD/message.md"
{ printf '# %s\n' "$SUBJECT"
  printf 'from: %s · to: %s · %s\n\n' "$FROM" "$TO" "$ISO"
  cat -- "$BODY"; } > "$MSGFILE"
rc=$?
[ "$rc" -eq 0 ] || die "could not compose the message"

# ---- THE GATE: the shared census, materialised from origin/master ---------------------------------
# The explicit refspec, not `fetch origin master`: a clone made --single-branch has no refspec that
# would create refs/remotes/origin/master, and the census would then be read from nothing.
git -C "$CLONE" fetch --quiet origin "+refs/heads/master:refs/remotes/origin/master" >/dev/null 2>&1; frc=$?
[ "$frc" -eq 0 ] || echo "  note: could not fetch origin master (rc=$frc) -- censusing with the local origin/master" >&2
for f in coord-identifier-census.sh coord-identifier-patterns.txt coord-identifier-hashes.txt; do
    gshow "$CLONE" "origin/master:.claude/coord-scripts/$f" > "$TMPD/$f" 2>/dev/null; rc=$?
    [ "$rc" -eq 0 ] || die "could not read origin/master:.claude/coord-scripts/$f (rc=$rc) -- a gate that cannot load does not pass"
done
for pair in "coord-identifier-census.sh $MIN_CENSUS" "coord-identifier-patterns.txt $MIN_PATTERNS" "coord-identifier-hashes.txt $MIN_HASHES"; do
    set -- $pair
    sz="$(wc -c < "$TMPD/$1" 2>/dev/null)"; rc=$?
    sz="${sz// /}"
    { [ "$rc" -eq 0 ] && [ -n "$sz" ] && [ "$sz" -gt "$2" ]; } || die "$1 read $sz bytes, under the floor of $2 -- a truncated instrument is not a clean read"
    echo "  census input: $1 $sz bytes (floor $2)"
done

bash "$TMPD/coord-identifier-census.sh" entry "$MSGFILE" > "$TMPD/census.entry" 2>&1; crc=$?
cat -- "$TMPD/census.entry"
[ "$crc" -eq 0 ] || { echo "REFUSED: the census refused the OUTGOING MESSAGE (rc=$crc). Nothing was written, staged, committed or pushed." >&2; exit 3; }
SUBJLINE="inbox: $FROM -> $TO -- $SUBJECT"
bash "$TMPD/coord-identifier-census.sh" subject "$SUBJLINE" > "$TMPD/census.subject" 2>&1; src=$?
cat -- "$TMPD/census.subject"
[ "$src" -eq 0 ] || { echo "REFUSED: the census refused the COMMIT SUBJECT (rc=$src). Nothing was written, staged, committed or pushed." >&2; exit 3; }

# ---- write the ONE file, stage the ONE path -------------------------------------------------------
DIR="docs/phase4/inbox/$TO"
mkdir -p -- "$CLONE/$DIR"; rc=$?
[ "$rc" -eq 0 ] || die "could not create $DIR"
REL="$DIR/$TS-$FROM.md"; n=1
while [ -e "$CLONE/$REL" ]; do
    n=$((n + 1)); REL="$DIR/$TS-$FROM-$n.md"
    [ "$n" -lt 50 ] || die "50 files from $FROM to $TO in one second -- refusing rather than guessing"
done
cp -- "$MSGFILE" "$CLONE/$REL"; rc=$?
[ "$rc" -eq 0 ] || die "could not write $REL"
git -C "$CLONE" add -- "$REL"; rc=$?
[ "$rc" -eq 0 ] || die "git add of $REL failed (rc=$rc)"
STAGED="$(git -C "$CLONE" diff --cached --name-only)"; rc=$?
[ "$rc" -eq 0 ] || die "could not read the index back"
[ "$STAGED" = "$REL" ] || die "the index holds '$STAGED' and not exactly '$REL' -- refusing to commit a set this tool did not stage"

SIGN="--no-gpg-sign"; RSIGN="--no-gpg-sign"
gs="$(git -C "$CLONE" config --get commit.gpgsign 2>/dev/null)"
sk="$(git -C "$CLONE" config --get user.signingkey 2>/dev/null)"
if [ "$gs" = "true" ] && [ -n "$sk" ]; then SIGN="-S"; RSIGN="--gpg-sign"; fi
git -C "$CLONE" commit -q "$SIGN" -m "$SUBJLINE" -m "from: $FROM · to: $TO · $ISO · $NL body lines · census clean" -- "$REL"; rc=$?
[ "$rc" -eq 0 ] || die "the commit failed (rc=$rc); $REL is written and staged in '$CLONE'"

# ---- push, with the rebase retry (one file per message never conflicts) ---------------------------
ATTEMPTS=0; PUSHED=0
while [ "$ATTEMPTS" -lt 5 ]; do
    ATTEMPTS=$((ATTEMPTS + 1))
    git -C "$CLONE" push origin "HEAD:refs/heads/claude/mailbox" > "$TMPD/push.$ATTEMPTS" 2>&1; prc=$?
    if [ "$prc" -eq 0 ]; then PUSHED=1; break; fi
    echo "  push attempt $ATTEMPTS rejected (rc=$prc) -- fetching and rebasing" >&2
    sed -n '1,4p' -- "$TMPD/push.$ATTEMPTS" >&2
    git -C "$CLONE" fetch --quiet origin claude/mailbox > "$TMPD/fetch.$ATTEMPTS" 2>&1; frc=$?
    [ "$frc" -eq 0 ] || { cat -- "$TMPD/fetch.$ATTEMPTS" >&2; die "the fetch before the rebase failed (rc=$frc)"; }
    git -C "$CLONE" rebase "$RSIGN" origin/claude/mailbox > "$TMPD/rebase.$ATTEMPTS" 2>&1; rrc=$?
    if [ "$rrc" -ne 0 ]; then
        cat -- "$TMPD/rebase.$ATTEMPTS" >&2
        git -C "$CLONE" rebase --abort >/dev/null 2>&1
        die "the rebase onto origin/claude/mailbox failed (rc=$rrc) -- one file per message cannot conflict, so this is a tree problem and not a merge to resolve here"
    fi
done
[ "$PUSHED" -eq 1 ] || die "the push was rejected on all $ATTEMPTS attempts"

SHA="$(git -C "$CLONE" rev-parse HEAD)"; rc=$?
[ "$rc" -eq 0 ] || die "could not read back the commit"
echo "FILE $REL"
echo "COMMIT $SHA"
echo "ATTEMPTS $ATTEMPTS"
echo "sent: $FROM -> $TO · $NL body lines · census clean · $REL @ ${SHA:0:9}"
exit 0
