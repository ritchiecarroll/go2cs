#!/usr/bin/env bash
# Assert every board seat's CURRENT REMOTE TIP is an ancestor of a merge result.
#
#   usage: coord-seat-ancestry.sh <merge-result-ref> [seatlist-file]
#          coord-seat-ancestry.sh --self-test          (the gate must go RED)
#
# WHY THIS EXISTS -- two traps measured 2026-09-06/07, and ONE assertion catches both:
#
#   APPEND form (R): seatB forks from seatA's WORK commit; seatA later APPENDS a
#     correction. The correction is not an ancestor of seatB, so merging seatB alone
#     carries seatA's work and SILENTLY DROPS its correction. Nothing conflicts,
#     nothing errors -- "merging the child brings the parent" is TRUE for the work and
#     FALSE for anything appended after the fork.
#
#   AMEND form (G): a seat's body is amended, moving its tip. An assembler holding the
#     ANNOUNCED sha merges a superseded commit. Severity splits: a message-only amend
#     ships correct code with a stale body (risks the RECORD); a CONTENT amend ships a
#     retracted figure in a shipped document (risks the ARTIFACT).
#
# The collapse: read each seat's CURRENT remote tip -- never the announced sha, which is
# exactly what an amend invalidates -- and require it to be an ancestor of the result.
#
# DERIVING THE SEATLIST for a landed train (exact, and NOT from merge messages):
#   git log --merges --format=%P <base>..<tip> | awk {print $2}
# One second parent per merge = one seat tip. A regex over merge MESSAGES over-counts,
# because a message legitimately mentions branches it did not merge -- measured 2026-09-06,
# 21 reported for a train with 20 merge commits.
#
# Exit: 0 all seats reachable · 1 a seat is missing · 2 refused (bad input).

set -u
REPO="${REPO:-/c/Projects/go2cs}"
cd "$REPO" || { echo "REFUSED: cannot enter $REPO"; exit 2; }

SELFTEST=0
[ "${1:-}" = "--self-test" ] && SELFTEST=1

if [ "$SELFTEST" = "0" ]; then
  RESULT="${1:-}"
  [ -z "$RESULT" ] && { echo "REFUSED: name the merge-result ref explicitly."; exit 2; }
  case "$RESULT" in
    HEAD|head) echo "REFUSED: 'HEAD' is ambiguous -- \$REPO is fixed, so it resolves in the MAIN"
               echo "         checkout regardless of the caller's worktree. Name a sha or branch."
               exit 2 ;;
  esac
  git rev-parse -q --verify "$RESULT^{commit}" >/dev/null 2>&1 \
    || { echo "REFUSED: '$RESULT' is not a commit in $REPO"; exit 2; }
fi

FAIL=0

assert_reachable() { # seat-branch  result-ref
  local seat="$1" result="$2" ref tip
  ref="refs/remotes/origin/claude/$seat"
  tip=$(git rev-parse -q --verify "$ref" 2>/dev/null)
  if [ -z "$tip" ]; then
    printf '  GONE %-32s (no remote ref -- deleted or renamed since the board was built)\n' "$seat"
    FAIL=1; return
  fi
  if git merge-base --is-ancestor "$tip" "$result" 2>/dev/null; then
    printf '  OK   %-32s %s\n' "$seat" "${tip:0:9}"
  else
    printf '  MISS %-32s %s NOT an ancestor of %s\n' "$seat" "${tip:0:9}" "${result:0:9}"
    printf '       the seat'"'"'s CURRENT tip is absent from the result. Either it was not merged,\n'
    printf '       or a superseded sha was merged in its place (amend), or a parent'"'"'s later\n'
    printf '       commit was dropped by merging only its child (append).\n'
    FAIL=1
  fi
}

if [ "$SELFTEST" = "1" ]; then
  # POSITIVE CONTROL -- rebuild R's measured trap in a throwaway repo and require RED.
  T=$(mktemp -d) || exit 2
  ( cd "$T" && git init -q . && git config user.email c@x && git config user.name c
    echo base > f && git add -A && git commit -qm base
    echo a > a && git add -A && git commit -qm 'seatA work'
    SEATA_WORK=$(git rev-parse HEAD)
    git checkout -q -b seatB
    echo b > b && git add -A && git commit -qm 'seatB work'
    git checkout -q master 2>/dev/null || git checkout -q main
    git commit -q --allow-empty -m 'seatA STAMP appended after seatB forked'
    git checkout -q -b result "$SEATA_WORK"
    git merge -q --no-edit seatB -m 'merge ONLY the child'
    RESULT=$(git rev-parse HEAD)
    STAMP=$(git rev-parse master 2>/dev/null || git rev-parse main)
    echo "  trap built: stamp=${STAMP:0:9} result=${RESULT:0:9}"
    if git merge-base --is-ancestor "$STAMP" "$RESULT"; then
      echo "  SELF-TEST FAILED: the trap did not reproduce -- stamp IS an ancestor"; exit 1
    else
      echo "  SELF-TEST OK: stamp is NOT an ancestor of the child-only merge -- the gate fires"; exit 0
    fi )
  rc=$?
  rm -rf "$T"
  exit $rc
fi

SEATLIST="${2:-}"
if [ -n "$SEATLIST" ] && [ -f "$SEATLIST" ]; then
  SEATS=$(grep -oE '^[a-zA-Z0-9][a-zA-Z0-9._-]*' "$SEATLIST" | sort -u)
else
  SEATS=$(git for-each-ref --format='%(refname:short)' 'refs/remotes/origin/claude/*' \
          | sed 's|origin/claude/||' | grep -v '^mailbox$' \
          | while read -r b; do
              n=$(git rev-list --count "$RESULT..origin/claude/$b" 2>/dev/null)
              [ "${n:-0}" -gt 0 ] && echo "$b"
            done)
  echo "  (no seatlist given -- reporting every ref NOT reachable from $RESULT)"
fi

echo "=== seat ancestry against ${RESULT:0:9} ==="
for s in $SEATS; do assert_reachable "$s" "$RESULT"; done

echo
if [ "$FAIL" = "0" ]; then echo "ALL SEATS REACHABLE"; else echo "SEATS MISSING FROM THE RESULT -- see MISS/GONE above"; fi
exit $FAIL
