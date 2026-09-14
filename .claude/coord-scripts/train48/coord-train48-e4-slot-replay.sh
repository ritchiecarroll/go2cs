#!/usr/bin/env bash
# TRAIN 48 -- E4 SLOT FILL: a SCRATCH REPLAY of the accumulation, resolving the append-only BOARD
# by `git merge-file --union` and NOTHING else.  COORD ruling E4, 2026-09-13 ~20:00.
#
# ⚠ WHY A REPLAY AND NOT THE SAVED STAGES.  Rehearsal run 1 ABORTED seat 9, so the accumulation it
#   presented to seats 10 and 15 did NOT contain seat 9's appended entries.  A resolution built from
#   those saved `ours/` would SUBTRACT seat 9 the moment the assembly (which pre-resolves seat 9 and
#   carries it) reached seat 10 -- the silent-subtraction class.  The replay resolves each conflict
#   as it happens, so every `ours` is the union the assembly will actually hold.
#
# It resolves ONLY the append-only ledger, only by --union, and only when the seat's own diff against
# its merge-base is a PURE APPEND.  Anything else REFUSES that slot and reports it.
set -u
SELF=/c/Projects/go2cs/.claude/coord-scripts/train48
RES="$SELF/coord-train48-resolutions"
WT=/c/go2cs-tmp-coord/t48-rehearse
G=/c/Projects/go2cs
M=271300cea03a2f47bd7dd8d9ed392c6249dac4c4
MSHORT=271300cea
BOARD=docs/phase4/BOARD-next-validation-candidates.md
PY=$HOME/AppData/Local/Temp/claude/C--Projects-go2cs/1e6e99a3-6151-40a5-abe6-501eb96e0a7f/scratchpad/t48-r3/slotcheck.py
BAD=0; CONF=0; CLEAN=0; RESOLVED=0

# --- the seat table is READ OUT OF THE ASSEMBLY SCRIPT and never retyped.
A="$SELF/coord-train48-assemble.sh"
TBL=$(awk '/^SEAT_TABLE="1\|/{f=1} f{print} f&&/"$/{exit}' "$A" | sed '1s/^SEAT_TABLE="//' | sed '$s/"$//')
echo "=== E4 SLOT REPLAY :: base $M ==="
echo "rows read out of $A = $(printf '%s\n' "$TBL" | grep -c .)"

git -C "$G" worktree add --detach -f "$WT" "$M" >/dev/null 2>&1 || { echo "REFUSED: cannot create $WT"; exit 2; }
cd "$WT" || exit 2
echo "worktree HEAD = $(git rev-parse HEAD)"
[ "$(git rev-parse HEAD)" = "$M" ] || { echo "REFUSED: worktree HEAD is not the base"; exit 2; }

while IFS='|' read -r n b s c m rest; do
  [ -n "$n" ] || continue
  git merge --no-ff -m "replay seat $n ($b)" "$s" > "/tmp/t48e4-merge-$n.log" 2>&1
  mrc=$?
  if [ "$mrc" = "0" ]; then
    CLEAN=$(( CLEAN + 1 ))
    echo "seat $n ($c) $b @$(printf '%s' "$s" | cut -c1-10) :: CLEAN (tree $(git rev-parse HEAD^{tree} | cut -c1-10))"
    continue
  fi
  UNM=$(git diff --name-only --diff-filter=U | grep -c .)
  CONF=$(( CONF + 1 ))
  echo "seat $n ($c) $b @$(printf '%s' "$s" | cut -c1-10) :: CONFLICT unmergedPaths=$UNM"
  git diff --name-only --diff-filter=U | sed 's/^/      /'
  if [ "$UNM" != "1" ] || [ "$(git diff --name-only --diff-filter=U)" != "$BOARD" ]; then
    echo "      ** REFUSED: this slot is NOT the append-only ledger alone -- the coordinator rules it by hand"
    BAD=1; git merge --abort; continue
  fi
  d="$RES/seat$n"
  mkdir -p "$d/conflicted/$(dirname "$BOARD")" "$d/base/$(dirname "$BOARD")" "$d/ours/$(dirname "$BOARD")" "$d/theirs/$(dirname "$BOARD")" "$d/files/$(dirname "$BOARD")"
  cp "$BOARD" "$d/conflicted/$BOARD"
  git show ":1:$BOARD" > "$d/base/$BOARD"
  git show ":2:$BOARD" > "$d/ours/$BOARD"
  git show ":3:$BOARD" > "$d/theirs/$BOARD"
  printf '%s\n' "$BOARD" > "$d/PATHS.txt"
  # --- the union resolution.  `git merge-file --union <cur> <base> <other>` writes into <cur>.
  cp "$d/ours/$BOARD" "/tmp/t48e4-cur-$n"
  git merge-file --union "/tmp/t48e4-cur-$n" "$d/base/$BOARD" "$d/theirs/$BOARD" > "/tmp/t48e4-mf-$n.log" 2>&1
  mfrc=$?
  echo "      git merge-file --union exit=$mfrc  (0 = no remaining conflicts; >0 would be the count git could not union)"
  if [ "$mfrc" != "0" ]; then
    echo "      ** REFUSED: merge-file --union did not resolve cleanly"
    BAD=1; rm -f "/tmp/t48e4-cur-$n"; git merge --abort; continue
  fi
  cp "/tmp/t48e4-cur-$n" "$d/files/$BOARD"
  python "$PY" "$d/base/$BOARD" "$d/ours/$BOARD" "$d/theirs/$BOARD" "$d/files/$BOARD" || BAD=1
  { printf 'seat=%s\n' "$n"; printf 'pin=%s\n' "$s"; printf 'branch=%s\n' "$b"; printf 'base=%s\n' "$MSHORT"
    printf 'paths=%s\n' 1; printf 'saved=%s\n' "$(date '+%F %T')"
    printf 'resolution=git merge-file --union (COORD ruling E4, 2026-09-13) -- the BOARD is an APPEND-ONLY ledger\n'
    printf 'provenance=SCRATCH REPLAY of seats 1..N in table order onto %s; ours is the union THROUGH THE PREVIOUS SEAT, with every earlier conflict already resolved\n' "$MSHORT"; } > "$d/MANIFEST.txt"
  cp "/tmp/t48e4-cur-$n" "$BOARD"
  git add -- "$BOARD"
  git commit --no-edit -m "replay seat $n ($b) -- UNION-RESOLVED $BOARD" > "/tmp/t48e4-commit-$n.log" 2>&1 || { echo "      ** REFUSED: could not commit"; BAD=1; git merge --abort; continue; }
  RESOLVED=$(( RESOLVED + 1 ))
  echo "      SLOT FILLED :: $d/files/$BOARD ($(grep -c . "$d/files/$BOARD") non-empty line(s)) -- accumulation continues WITH this seat"
  rm -f "/tmp/t48e4-cur-$n"
done <<< "$TBL"

echo ""
echo "=== E4 REPLAY DONE seatsClean=$CLEAN conflicts=$CONF slotsFilled=$RESOLVED bad=$BAD ==="
echo "union HEAD  = $(git rev-parse HEAD)"
echo "union tree  = $(git rev-parse HEAD^{tree})"
# --- the marker scan, over the union's CHANGED files only, with the SAME predicate --verify uses.
MK=0; MKF=0
while IFS= read -r p; do
  [ -n "$p" ] || continue; [ -f "$p" ] || continue
  k=$(grep -acE '^(<<<<<<<|=======|>>>>>>>)( |$)' "$p" || true)
  [ "$k" = "0" ] || { MKF=$(( MKF + 1 )); echo "   marker hit :: $p x$k"; }
  MK=$(( MK + k ))
done <<< "$(git diff --name-only "$M" HEAD)"
echo "conflict-marker lines across the union's CHANGED files = $MK (in $MKF file(s))"
exit $(( BAD ? 1 : 0 ))
