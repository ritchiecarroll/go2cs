#!/bin/bash
# DERIVE-TIME SELF-CHECK for coord-train48-assemble.sh (throwaway, read-only).
#
# It EXTRACTS blocks from the assembly script BY GREP ANCHOR and runs them, so what is exercised is
# the assembly script's OWN definitions rather than a retyped copy -- a retyped copy is the thing that
# drifts.  It NEVER runs the assembly script: no cd into the assembly worktree, no fetch, no merge,
# no leg, no build.
#
#   usage:  bash coord-train48-derive-selfcheck.sh [<path to coord-train48-assemble.sh>]
#   exit 0  every arm passed
#   exit 1  an arm failed
#   exit 3  a BLOCK COULD NOT BE RESOLVED or does not parse -- the checker itself is broken, and that
#           is reported as its own state rather than as a finding about the script
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`, for the reason the assembly's own header states: under
#   pipefail a `grep -q` SIGPIPEs its producer and a TRUE match reads as a failure.
#
# ⚠ WHAT THIS CHECKS, AND WHY EACH ARM EXISTS:
#   0  the script parses, carries no CR bytes and no BOM.
#   1  the BASE: EXPECT_BASE is declared, the script ASSERTS it, and the assertion REFUSES a PENDING
#      base.  ⚠ ON A TEMPLATE THE BASE **IS** PENDING and that is the expected reading; what is
#      asserted is that the refusal EXISTS, because a template whose placeholder base can run is a
#      battery measuring a tree nobody named.
#   2  the SEAT TABLE -- its STRUCTURE (contiguous numbers, unique refs, unique filled SHAs, five
#      fields or more, an optional `allowed=` sixth), every SHA nine hex digits or PENDING, the
#      PENDING rows PRINTED BY NAME, and every class known to merge_seat.  ⚠ NO ROW-COUNT LITERAL,
#      here or in the assembly: train 46's `[ "$SEATS_LISTED" = "5" ]` refused a correct six-row
#      table AFTER merging all six.
#   3  every FILLED seat SHA RESOLVES, and a grown branch is REPORTED with its ancestry diagnosis
#      rather than silently re-pinned.
#   4  NO train-46 SHA survives outside a dated comment -- and the residual comment hits are PRINTED,
#      because "zero in code" is a different claim from "none anywhere" and only one of them is true.
#   5  the WORKTREE is a FILL POINT that REFUSES a placeholder, and SCRIPT_PATH is ABSOLUTE and
#      precedes the cd.
#   6  G11(c)'s NINE launch lines present, WITH negative controls -- the arm must go RED on a copy
#      with one removed, or its green is an assertion wearing a measurement's clothes.
#   7  the LEG K oracleGoVersion predicate, run FROM THE SCRIPT'S OWN DEFINITION over six values.
#   8  the per-seat CLASS SHAPES against the REAL seat diffs (for FILLED rows) and against decoys.
#   9  the PER-RUN-COPY discipline and the MID-BATTERY FREEZE announcement lines.
#  10  the G10d `-text` arm consults the ATTRIBUTE, with a decoy.
#  11  blk()'s own negative control: a broken anchor must be reported as the CHECKER being broken.
#  14  ⚠ **EVERY `req` ANCHOR IN THE LAND SCRIPT NAMES A STAMP THE ASSEMBLY EMITS, BY LITERAL
#      PREFIX.**  The dry-read's ARM A reads an anchor's substantial WORDS, which survive elsewhere in
#      the file when a seat's content arm is DELETED -- so an unseated row leaves the land requiring a
#      stamp nothing writes, and the landing refuses a healthy battery.  This arm reads the LITERAL
#      leading run of every anchor instead.  (Numbered 14 and printed last; arms 12 and 13 kept their
#      numbers so a reader can match this file against the runs that quote them.)
#  12  ⚠ **THE FIVE MECHANISED LESSONS, EACH WITH ITS OWN NEGATIVE CONTROL AGAINST THE TRAIN-46
#      ORIGINALS.**  Every one of these is a defect a train-46 launch actually hit; each arm must
#      read ZERO on the train-48 files AND NON-ZERO on the train-46 ones, because an arm that has
#      never been made to fire is an assertion rather than a measurement.  A control that cannot RUN
#      (the train-46 originals absent) is reported UNMEASURED and FAILS: an instrument that could not
#      run has not found nothing.
set -u
F="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/coord-train48-assemble.sh}"
SELFDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# ⚠ THE TRAIN-46 ORIGINALS ARE **INPUTS TO A MEASUREMENT**, and where they live is a fact rather than a
#   preference: on this fleet each train's scripts live in their own directory, so the originals are
#   the SIBLING `train46/` and not this directory.  The default therefore PREFERS the sibling and falls
#   back to this directory, and T46DIR= still overrides by name.  ⚠ The fallback is not a convenience:
#   arm 12 reports UNMEASURED AND FAILS when the originals cannot be found at whichever path is in
#   force, so a wrong default can never read as a pass.
if [ -z "${T46DIR:-}" ] && [ -f "$SELFDIR/../train46/coord-train46-assemble.sh" ]; then
  T46DIR="$(cd "$SELFDIR/../train46" && pwd)"
fi
T46DIR="${T46DIR:-$SELFDIR}"
TAG=t48selfcheck
FAIL=0
say(){ printf '%s\n' "$*"; }
[ -f "$F" ] || { say "   ABORT: $F does not exist"; exit 3; }
# the BASE this checker measures against is READ FROM THE SCRIPT, never retyped beside it.
BASE_EXPECT=$(grep -aE "^EXPECT_BASE='" "$F" | head -1 | sed -E "s/^EXPECT_BASE='([^']*)'.*/\1/")
[ -n "$BASE_EXPECT" ] || BASE_EXPECT=UNREADABLE

# --- ⚠ **THE OFFLINE SWITCH, AND WHY IT IS NOT A LOOSENING.**  Arms 1, 3 and 8 FETCH each seat's ref
#     before deciding whether its pin resolves, because "unresolved" before a fetch is a stale clone
#     and after one is a typo or an invention -- opposite severities.  A box under a FROZEN battery
#     may forbid a fetch outright, and an instrument that silently skipped the fetch would turn the
#     second reading into the first.  So the switch is NAMED, it is OFF by default, and when it is ON
#     an unresolvable pin is reported UNMEASURED **and still FAILS**: an instrument that could not run
#     has not found nothing, it has found nothing out.  The count is carried separately so a reader
#     can tell a fault from a blindness.
T48_SELFCHECK_OFFLINE="${T48_SELFCHECK_OFFLINE:-0}"
OFFLINE_UNMEASURED=0
sc_fetch(){ # $1 = a refspec (or the word master)
  if [ "$T48_SELFCHECK_OFFLINE" = "1" ]; then return 0; fi
  git -C "$G" fetch -q origin "$1" 2>/dev/null || true
}
[ "$T48_SELFCHECK_OFFLINE" = "1" ] && say "⚠ T48_SELFCHECK_OFFLINE=1 :: NO fetch is performed.  Every pin that does not resolve in this clone is reported UNMEASURED and FAILS -- it is NOT reported as an invention, because without a fetch those two states are indistinguishable."

# --- the TRAIN-47 originals, for the NEW arms' negative controls.  ⚠ COMPOSED FROM A NUMBER, NEVER
#     SPELLED: this file is written by a derive that runs a global coord-train47-* -> coord-train48-*
#     rename over it, and a spelled name here would be rewritten to point each control at the very
#     file it controls AGAINST.  Train 47's own arm 12 did exactly that on its first run.
T47NUM=47
if [ -z "${T47DIR:-}" ] && [ -d "$SELFDIR/../train${T47NUM}" ]; then T47DIR="$(cd "$SELFDIR/../train${T47NUM}" && pwd)"; fi
T47DIR="${T47DIR:-$SELFDIR}"
T47ASM="$T47DIR/coord-train${T47NUM}-assemble.sh"
T47LAND="$T47DIR/coord-train${T47NUM}-land.sh"
T47DRY="$T47DIR/coord-train${T47NUM}-land-dryread.sh"

ex(){ sed -n "$1,$2 p" "$F"; }

# ⚠ EVERY EXTRACTION IS ANCHORED BY GREP, NEVER BY A LINE NUMBER.  A line-numbered clearance goes
# stale on the next edit above it.  blk() resolves a block by its FIRST and LAST anchor lines,
# REFUSES when either anchor is absent or out of order, and every block is bash -n'd before it runs.
blk(){ # $1 = start anchor (ERE), $2 = end anchor, $3 = a label
  local lo hi
  lo=$(grep -nE "$1" "$F" | head -1 | cut -d: -f1)
  [ -n "$lo" ] && hi=$(awk -v s="$lo" 'NR>=s' "$F" | grep -nE "$2" | head -1 | cut -d: -f1)
  [ -n "${hi:-}" ] && hi=$(( lo + hi - 1 ))
  # ⚠ blk runs inside $( ), so a FAIL=1 set here is lost in the subshell and the caller would sail
  # on.  A failed resolution therefore EMITS an aborting statement on stdout -- the caller evals it.
  if [ -z "$lo" ] || [ -z "${hi:-}" ] || [ "$hi" -lt "$lo" ]; then
    say >&2 "   FAIL: cannot resolve block '$3' (start='$1' -> ${lo:-none}, end='$2' -> ${hi:-none})"
    printf 'printf "%%s\\n" "   ABORT: block %s unresolved"; exit 3\n' "$3"
    return 1
  fi
  ex "$lo" "$hi" > "/tmp/${TAG}-blk.sh"
  if ! bash -n "/tmp/${TAG}-blk.sh"; then
    say >&2 "   FAIL: block '$3' (lines $lo..$hi) does not PARSE -- the anchors cut a construct open"
    printf 'printf "%%s\\n" "   ABORT: block %s does not parse"; exit 3\n' "$3"
    return 1
  fi
  say >&2 "   block '$3' resolved to lines $lo..$hi and parses clean"
  cat "/tmp/${TAG}-blk.sh"
  rm -f "/tmp/${TAG}-blk.sh"
}

G=/c/Projects/go2cs

say "== 0. the script under check =="
say "   file=$F  lines=$(wc -l < "$F")  CRbytes=$(tr -cd '\r' < "$F" | wc -c) (must be 0)  BOM=[$(head -c 3 "$F" | od -An -tx1 | tr -d ' ')]"
bash -n "$F" && say "   PASS: the whole script parses" || { say "   FAIL: the whole script does not parse"; FAIL=1; }
[ "$(tr -cd '\r' < "$F" | wc -c)" = "0" ] && say "   PASS: no CR bytes" || { say "   FAIL: the script carries CR bytes -- bash reads a script by byte offset and a CRLF here is a parse hazard"; FAIL=1; }

# ---- 1. THE BASE -------------------------------------------------------------------------------
say ""
say "== 1. the BASE: EXPECT_BASE is declared, ASSERTED, and a PENDING base REFUSES =="
E47=$(grep -aE "^EXPECT_BASE='" "$F" | head -1 | sed -E "s/^EXPECT_BASE='([^']*)'.*/\1/")
E46=$(grep -aE "^EXPECT_ORDER='" "$F" | head -1 | sed -E "s/^EXPECT_ORDER='([^']*)'.*/\1/")
say "   the script declares EXPECT_BASE=[$E47] (this train's BASE) and EXPECT_ORDER=[$E46] (the ORDER pin)"
BA=$(grep -ac '^stamp "BASE ASSERTION ::' "$F")
BR=$(grep -ac 'BASE REFUSED: this script declares' "$F")
BP=$(grep -ac 'BASE REFUSED: EXPECT_BASE reads' "$F")
say "   the script's OWN base assertion :: 'BASE ASSERTION ::' stamps=$BA (want 1) :: wrong-base refusal=$BR (want 1) :: PENDING-base refusal=$BP (want 1)"
{ [ "$BA" = "1" ] && [ "$BR" = "1" ] && [ "$BP" = "1" ]; } && say "   PASS: the script ASSERTS its base, REFUSES a different one, and REFUSES an UNFILLED one" || { say "   FAIL: the base is derived but not asserted in all three directions -- HEAD == origin/master says the tree is AT master, not that master is the master this was derived against, and a PENDING base that RUNS is a battery measuring a tree nobody named"; FAIL=1; }
ECON=$(grep -aE "^T48_CONTAIN_PIN='" "$F" | head -1 | sed -E "s/^T48_CONTAIN_PIN='([^']*)'.*/\1/")
case "$E47" in
  PENDING|'')
    say "   ⚠ EXPECT_BASE reads PENDING, which is the EXPECTED state of a TEMPLATE.  It is FILL POINT F2."
    say "     Arms 3 and 8 are therefore UNMEASURABLE against real seat diffs and say so; they are not passes."
    ;;
  origin/master)
    # ⚠ THIS TRAIN DECLARES ITS BASE AS A **REVSPEC**, and the arm changes shape accordingly rather
    #   than refusing a spelling it was not written for.  What is asserted here is what the coordinator
    #   RULED: the base is origin/master resolved at launch, and it CONTAINS the named pin.  A literal
    #   SHA is refused as a base BY THIS ARM, because a baked base refuses a healthy launch the moment
    #   any lane lands and the operator then reaches for the switch that makes it stop refusing.
    say "   the script declares its base as the REVSPEC [origin/master] plus the CONTAINMENT pin T48_CONTAIN_PIN=[${ECON:-ABSENT}]"
    B1OK=1
    [ -n "${ECON:-}" ] || { say "   FAIL: base=origin/master with NO T48_CONTAIN_PIN pin -- the revspec alone asserts nothing about the base's CONTENT, which is the whole reason a revspec is admissible here"; B1OK=0; }
    ANC=$(grep -ac 'T48_CONTAIN_PIN:the CONTAINMENT pin' "$F" || true)
    say "   the containment pin is CONSUMED by the base-ancestry loop=$ANC (want 1 -- a pin declared and never asserted is a decoration)"
    [ "${ANC:-0}" = "1" ] || B1OK=0
    # ⚠ A **PENDING** CONTAINMENT PIN IS THE EXPECTED STATE OF THIS TEMPLATE AND IS NOT A FAILED
    #   CONTAINMENT.  The SHA train 47 lands did not exist when this was derived.  What is asserted
    #   here instead is that the assembly REFUSES the placeholder at launch -- an unfilled pin that
    #   RUNS is a battery measuring a tree nobody named -- and containment itself is left UNMEASURED
    #   and SAID to be, rather than claimed either way.
    case "${ECON:-}" in
      PENDING|*PENDING*)
        say "   ⚠ T48_CONTAIN_PIN reads [$ECON] -- FILL POINT F2, and the EXPECTED state of a template derived before train 47 landed.  CONTAINMENT IS UNMEASURABLE until it is filled and is NOT claimed here."
        FP2=$(grep -ac 'FILL POINT F2 REFUSED' "$F" || true)
        say "   the assembly's own FILL-POINT F2 refusal is present x${FP2:-0} (want >= 1)"
        [ "${FP2:-0}" -ge 1 ] || { say "   FAIL: the containment pin is a placeholder AND nothing refuses it -- the run would proceed against a base nobody asserted"; B1OK=0; }
        ;;
    esac
    if [ -d "$G/.git" ] && [ -n "${ECON:-}" ] && [ "${ECON#*PENDING}" = "$ECON" ]; then
      sc_fetch master
      RM=$(git -C "$G" ls-remote origin refs/heads/master 2>/dev/null | cut -c1-9)
      OM=$(git -C "$G" rev-parse --short=9 origin/master 2>/dev/null)
      say "   origin/master :: ls-remote reads [$RM] :: the fetched ref reads [$OM] :: BOTH ARE PRINTED because a base declared and a base MEASURED are two claims"
      [ "$RM" = "$OM" ] || { say "   FAIL: the two readings of origin/master disagree -- the remote moved between them, which is a race and not a base"; B1OK=0; }
      if git -C "$G" merge-base --is-ancestor "$ECON" origin/master 2>/dev/null; then
        say "   origin/master ($OM) CONTAINS $ECON -- the base the assembly would resolve at launch satisfies this train's containment pin"
      else
        say "   FAIL: origin/master ($OM) does NOT contain $ECON, so the assembly would REFUSE at its base ancestry arm"
        B1OK=0
      fi
    elif [ -n "${ECON:-}" ]; then
      say "   SKIPPED: $G is not a git repository from here -- the containment could not be MEASURED, only read from the script"
      B1OK=0
    fi
    [ "$B1OK" = "1" ] && say "   PASS: the base is a revspec resolved at launch AND a containment pin that is asserted, which is what this train ruled in place of a baked base SHA" || FAIL=1
    ;;
  *)
    if [ -d "$G/.git" ]; then
      sc_fetch master
      RM=$(git -C "$G" ls-remote origin refs/heads/master 2>/dev/null | cut -c1-9)
      OM=$(git -C "$G" rev-parse --short=9 origin/master 2>/dev/null)
      say "   origin/master :: ls-remote reads [$RM] :: the fetched ref reads [$OM] :: the script's base is [$E47]"
      say "   ⚠ BOTH ARE PRINTED ON PURPOSE.  A base declared and a base MEASURED are two claims, and this arm exists to compare them rather than to restate one of them."
      { [ "$RM" = "$OM" ] && [ "$RM" = "$E47" ]; } && say "   PASS: origin/master IS $E47 and the two readings agree" || { say "   FAIL: origin/master reads [$RM]/[$OM] where this script declares $E47 -- the tree moved, so RE-DERIVE rather than adjust"; FAIL=1; }
    else
      say "   SKIPPED: $G is not a git repository from here -- the base could not be MEASURED, only read from the script"
      FAIL=1
    fi ;;
esac
# ---- 2. THE SEAT TABLE -------------------------------------------------------------------------
say ""
say "== 2. the seat table: STRUCTURE (no row-count literal anywhere), nine-hex or PENDING, the classes =="
eval "$(blk '^SEAT_TABLE="1\|' '"$' 'SEAT_TABLE')" || exit 3
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
NUMS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $1}')
EXPN=$(awk -v n="$ROWS" 'BEGIN{for(i=1;i<=n;i++) print i}')
DUPREF=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $2}' | sort | uniq -d | grep -ac . || true)
DUPSHA=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3!="PENDING"{print $3}' | sort | uniq -d | grep -ac . || true)
SHORTR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' 'NF<5' | grep -ac . || true)
ALLOWR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print $1; break}}' | grep -ac . || true)
say "   rows=$ROWS :: merge order [$(printf '%s' "$NUMS" | tr '\n' ' ')] :: duplicate refs=$DUPREF :: duplicate filled SHAs=$DUPSHA :: rows with <5 fields=$SHORTR :: rows carrying an allowed= ruling=$ALLOWR"
say "   ⚠ THERE IS NO ROW-COUNT LITERAL HERE, AND ARM 12 REFUSES ONE IN THE ASSEMBLY.  A count compared"
say "     against itself is a check that cannot fail; what a hand-edited table actually breaks is the"
say "     STRUCTURE, and that is what both instruments assert."
{ [ "$(printf '%s\n' "$NUMS")" = "$(printf '%s\n' "$EXPN")" ] && [ "${DUPREF:-0}" = "0" ] && [ "${DUPSHA:-0}" = "0" ] && [ "${SHORTR:-0}" = "0" ]; } && say "   PASS: numbers 1..$ROWS contiguous, refs unique, filled SHAs unique, every row five fields or more" || { say "   FAIL: the seat table is structurally unsound -- the ROW ORDER IS THE MERGE ORDER"; FAIL=1; }
BADSHA=0; PEND=0; PENDNAMES=""
while IFS='|' read -r n b s c m a; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then PEND=$(( PEND + 1 )); PENDNAMES="$PENDNAMES $n($b)"; continue; fi
  if [ "$(printf '%s' "$s" | grep -acE '^[0-9a-f]{9}$' || true)" = "0" ]; then
    BADSHA=$(( BADSHA + 1 )); say "   FAIL: seat $n's SHA [$s] is neither nine hex digits nor PENDING"
  fi
done <<< "$SEAT_TABLE"
say "   PENDING rows=$PEND [$PENDNAMES]"
say "   ⚠ PRINTED BY NAME, not merely counted.  On a TEMPLATE every row is a placeholder and that is the"
say "     EXPECTED reading; the assembly SKIPS them with a stamp by default, and TRAIN48_REQUIRE_ALL=1"
say "     is what makes them refuse -- the shape a LANDING run of the full table takes."
say "   malformed SHAs=$BADSHA (want 0)"
[ "$BADSHA" = "0" ] && say "   PASS: every non-PENDING SHA is nine hex digits" || FAIL=1
CLSOK=1
for c in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $4}' | sort -u); do
  # ⚠ A **FIXED-STRING** LOOKUP, NEVER AN ERE.  A class name may carry a regex metacharacter
  #   (`converter-test+docs` does), and an ERE built from it silently matches nothing -- which would
  #   report a correctly implemented class as missing.  The glyph-substring family, one layer over.
  # ⚠ `PENDING-class` IS A **CLASS FILL POINT**, AND THE CORRECT READING IS THAT IT HAS **NO**
  #   merge_seat ARM.  A class decides the shape a seat is held to; implementing an arm for the
  #   placeholder would let a row whose class nobody decided merge under a shape nobody ruled.  What
  #   is asserted instead is the pair of refusals that make the placeholder safe: the assembly's
  #   structural check refuses a FILLED row carrying it, and merge_seat's unknown-class arm refuses it
  #   if one ever reached the loop.
  if [ "$c" = "PENDING-class" ]; then
    PCF=$(grep -acF -- 'seat_class_placeholders' "$F" || true)
    PCA=$(grep -acF -- "    ${c})" "$F" || true)
    say "   class '$c' is a FILL POINT :: the structural placeholder refusal is wired x${PCF:-0} (want >= 2 -- the reader and its consumer) :: it has a merge_seat arm x${PCA:-0} (want 0 -- a placeholder with an arm is a shape nobody ruled)"
    { [ "${PCF:-0}" -ge 2 ] && [ "${PCA:-0}" = "0" ]; } || { say "   FAIL: the class placeholder is not refused structurally, or an arm was written for it"; CLSOK=0; }
  elif [ "$(grep -acF -- "    ${c})" "$F" || true)" != "0" ]; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi
done
[ "$CLSOK" = "1" ] && say "   PASS: every class in the table is a class merge_seat implements" || FAIL=1
for m in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $5}' | sort -u); do
  case "$m" in tip|sha) say "   tip mode '$m' is known" ;; *) say "   FAIL: unknown tip mode '$m'"; FAIL=1 ;; esac
done
# the PLACEHOLDER REF discipline: a row whose SHA is filled must not still name a placeholder ref,
# and a row whose SHA is PENDING must not carry an allowed= ruling.
PHBAD=0
while IFS='|' read -r n b s c m a; do
  [ -n "$n" ] || continue
  case "$b" in
    claude/PENDING-BRANCH)
      say "   row $n is a REF FILL POINT :: the SHA [$s] is known and the BRANCH NAME was not given.  The assembly REFUSES this shape at its SEAT PREFLIGHT; the name is NOT guessed here, because a row whose branch nobody named cannot have its class shape or its forbidden paths measured at all."
      REFFILL=$(( ${REFFILL:-0} + 1 ))
      RFR=$(grep -ac 'carries a FILLED SHA' "$F" | tr -d '\r')
      say "   the assembly's own preflight refusal for that shape is present x${RFR:-0} (want >= 1 -- a fill point with no refusal behind it is a default)"
      [ "${RFR:-0}" -ge 1 ] || { say "   FAIL: a row carries a REF fill point and NOTHING refuses it"; PHBAD=1; } ;;
    claude/PENDING-*) [ "$s" = "PENDING" ] || { say "   FAIL: row $n has a FILLED SHA ($s) against the PLACEHOLDER ref [$b]"; PHBAD=1; } ;;
  esac
  case "${a:-}" in allowed=*) [ "$s" != "PENDING" ] || { say "   FAIL: row $n declares an allowed= ruling while its SHA reads PENDING -- a ruling about a seat that does not exist"; PHBAD=1; } ;; esac
done <<< "$SEAT_TABLE"
[ "$PHBAD" = "0" ] && say "   PASS: no row carries a filled SHA against a placeholder ref, and no PENDING row carries a ruling (the assembly's own preflight asserts the same before it touches the worktree)" || FAIL=1

# ---- 3. EVERY FILLED SEAT SHA RESOLVES ---------------------------------------------------------
say ""
say "== 3. every FILLED seat SHA resolves, and a grown branch is REPORTED rather than re-pinned =="
if [ ! -d "$G/.git" ]; then
  say "   SKIPPED: $G is not a git repository from here"
  FAIL=1
else
  while IFS='|' read -r n b s c m a; do
    [ -n "$n" ] || continue
    if [ "$s" = "PENDING" ]; then say "   seat $n ($c) $b :: PENDING -- NOT checkable, and EXPECTED on this derive"; continue; fi
    sc_fetch "+refs/heads/$b:refs/remotes/origin/$b"
    if ! git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null; then
      if [ "$T48_SELFCHECK_OFFLINE" = "1" ]; then
        say "   FAIL (UNMEASURED, offline): row $n's SHA $s ($b) is not in this clone and T48_SELFCHECK_OFFLINE=1 forbade the fetch.  ⚠ THIS IS NOT THE CLAIM THAT THE SHA IS INVENTED -- without a fetch a stale clone and an invention are indistinguishable, and an arm that could not run is not a pass.  Re-run WITHOUT the switch to turn this into a measurement."
        OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))
      else
        say "   FAIL: row $n's SHA $s ($b) does not resolve even after the fetch -- an unresolved SHA and an INVENTED one read the same"
      fi
      FAIL=1; continue
    fi
    tip=$(git -C "$G" rev-parse --short=9 "origin/$b" 2>/dev/null)
    if [ "$tip" = "$s" ]; then
      say "   seat $n ($c) $b :: tip == pin $s  (tip mode satisfied)"
    elif git -C "$G" merge-base --is-ancestor "$s" "origin/$b" 2>/dev/null; then
      ahead=$(git -C "$G" rev-list --count "${s}..origin/$b")
      case "$m" in
        sha) say "   seat $n ($c, SHA mode) $b :: tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor -- **DELIBERATELY NOT CARRIED**.  merge_seat will PROCEED and STAMP the excluded commit(s) BY NAME; a SHA-mode row is a row pinned BEHIND its tip on purpose.  The excluded commit(s):" ;;
        *)   say "   seat $n ($c, tip mode) $b :: ⚠ THE BRANCH GREW -- tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor.  merge_seat will ABORT with this diagnosis, which is the mechanism WORKING.  The added commit(s):" ;;
      esac
      git -C "$G" log --oneline "${s}..origin/$b" | sed 's/^/       /'
      say "     ⚠ THE PIN IS **NOT** MOVED HERE.  A SHA typed into the table is the one place a seat's pin lives; re-pinning is the coordinator's, and this arm's job is to make the growth impossible to miss rather than to absorb it."
    else
      case "$b" in
        claude/PENDING-BRANCH) say "   row $n :: REF FILL POINT -- the branch name was not given, so there is NO TIP to compare the pin $s against.  UNMEASURED by construction, and the assembly refuses the row until the name is filled."; continue ;;
      esac
      say "   FAIL: row $n's pin $s is NOT an ancestor of the tip $tip -- the branch was REWRITTEN, or the pin names a different branch.  Ask the owner before re-pinning."
      FAIL=1
    fi
  done <<< "$SEAT_TABLE"
fi

# ---- 4. NO TRAIN-45 SHA SURVIVES OUTSIDE A DATED COMMENT ---------------------------------------
say ""
say "== 4. no train-45/46 seat SHA survives in CODE (comment hits are PRINTED, because they are a different claim) =="
# ⚠ THE RESIDUAL SET IS TRAIN 46's SEAT SHAS **AND** TRAIN 45's.  A template derived from train
#   46 inherits its seat pins wherever a block was not replaced whole, and a live SHA from a
#   landed train is a pin that resolves and means nothing.
T45SHAS='234cf8e8d|1922e3ec1|31668f43e|787159ed7|aa7abc006|3e5ead2d1|7d377e27b|a29253a2b|044116000|9092ab8b7|4a8642e7e|c08cb29c5|acab60084|13908a888|d839cb1d7|19bb74012|f613d5cfa|35105cdcd|193af90f5|489c5553c|dddd46493|8d7c348bb|34cf4ad02|05b50de63|9893b70e1|8adf8875a|5c5ef371d|e7201a405|18cb44b19|238dfefea'
CODEHITS=$(grep -nE "$T45SHAS" "$F" | grep -vE '^[0-9]+: *#' | grep -c . || true)
CMTHITS=$(grep -nE "$T45SHAS" "$F" | grep -E '^[0-9]+: *#' | grep -c . || true)
say "   train-45/46 seat SHAs in CODE=$CODEHITS (must be 0) :: in COMMENTS=$CMTHITS (allowed -- dated lessons and retirement notes)"
if [ "$CODEHITS" != "0" ]; then
  say "   FAIL: a train-45/46 seat SHA is live in this script:"
  grep -nE "$T45SHAS" "$F" | grep -vE '^[0-9]+: *#' | cut -c1-160 | sed 's/^/       /'
  FAIL=1
else
  say "   PASS: none in code"
fi
if [ "$CMTHITS" != "0" ]; then
  say "   the comment hits, PRINTED so 'zero in code' is not read as 'none anywhere':"
  grep -nE "$T45SHAS" "$F" | grep -E '^[0-9]+: *#' | cut -c1-150 | sed 's/^/       /'
fi

# ---- 5. THE WORKTREE AND SCRIPT_PATH -----------------------------------------------------------
say ""
say "== 5. the worktree is a FILL POINT that REFUSES a placeholder; SCRIPT_PATH is ABSOLUTE and precedes the cd =="
WTLINE=$(grep -n '^WT="\${TRAIN48_WT:-' "$F" | head -1)
CDLINE=$(grep -n '^cd "\$WT"' "$F" | head -1)
REFUSE=$(grep -ac 'WRONG WORKTREE' "$F")
UNFILLED=$(grep -ac 'WORKTREE UNFILLED ::' "$F")
say "   $WTLINE"
say "   $CDLINE"
say "   'WRONG WORKTREE' refusals=$REFUSE (want >= 1) :: 'WORKTREE UNFILLED' refusals=$UNFILLED (want >= 1)"
{ [ -n "$WTLINE" ] && [ -n "$CDLINE" ] && [ "$REFUSE" -ge 1 ] && [ "$UNFILLED" -ge 1 ]; } && say "   PASS: the worktree is a named FILL POINT, an unfilled one ABORTS, and any other tree is refused.  ⚠ IT IS NOT INHERITED FROM THE PREVIOUS TRAIN: a script pointing at a neighbour's worktree would run inside a tree another battery may be holding" || { say "   FAIL: the worktree is not fill-point-and-refused"; FAIL=1; }
SPLINE=$(grep -n '^SCRIPT_PATH=' "$F" | head -1)
say "   $SPLINE"
if [ "$(printf '%s' "$SPLINE" | grep -acF 'cd "$(dirname "${BASH_SOURCE[0]}")" && pwd' || true)" != "0" ]; then
  say "   PASS: SCRIPT_PATH is resolved through cd+pwd BEFORE the script cd's into the assembly worktree, so G11(c)'s grep can open the file"
else
  say "   FAIL: SCRIPT_PATH is not the absolute form.  A relative BASH_SOURCE grepped after the cd once read '0 of 6 launch lines' -- the instrument's own blindness reported as a finding about the script."
  FAIL=1
fi
SPN=$(grep -ac '^SCRIPT_PATH=' "$F"); CDN=$(printf '%s' "$CDLINE" | cut -d: -f1); SPL=$(printf '%s' "$SPLINE" | cut -d: -f1)
say "   SCRIPT_PATH assignments=$SPN (want 1) at line $SPL; the cd is at line ${CDN:-none} (SCRIPT_PATH must come FIRST)"
{ [ "$SPN" = "1" ] && [ -n "${CDN:-}" ] && [ "$SPL" -lt "$CDN" ]; } && say "   PASS: the assignment precedes the cd" || { say "   FAIL: the ordering does not hold, so the absolute form buys nothing"; FAIL=1; }

# ---- 6. G11(c): the NINE launch lines, with NEGATIVE CONTROLS -----------------------------------
say ""
say "== 6. G11(c)'s wiring arm: the NINE launch lines it greps for must be present =="
LEGPATS=('run-behavioral.ps1 --filter' 'go2cs-stdlib.slnx' 'go2cs.slnx' 'GolibTests/GolibTests.csproj' 'check-no-regression.ps1' 'run-validated-sweep.ps1' 'go test -count=1 -timeout 30m ./...' '-test-action' '-stdlib -comments')
W=0; MISSW=""
for legpat in "${LEGPATS[@]}"; do
  if grep -aqF -- "$legpat" "$F"; then W=$(( W + 1 )); else MISSW="$MISSW [$legpat]"; fi
done
say "   launch lines found: $W of 9$( [ -n "$MISSW" ] && echo " missing:$MISSW" )"
[ "$W" = "9" ] && say "   PASS: every gate family this train's justifications declare OWED is wired" || { say "   FAIL: G11(c) would refuse -- and correctly"; FAIL=1; }
for pair in 'run-validated-sweep\.ps1|LEG K' '-test-action|LEG R' '-stdlib -comments|LEG D' 'check-no-regression\.ps1|LEG 4'; do
  pat="${pair%%|*}"; nm="${pair#*|}"
  CP=/tmp/${TAG}-neg.sh
  sed "s#$pat#REMOVED-BY-CONTROL#g" "$F" > "$CP"
  W2=0
  for legpat in "${LEGPATS[@]}"; do grep -aqF -- "$legpat" "$CP" && W2=$(( W2 + 1 )); done
  if [ "$W2" -lt 9 ]; then say "   PASS (negative control): removing $nm's launch line takes the wiring count 9 -> $W2, so the arm CAN go red"
  else say "   FAIL (negative control): the wiring count still read 9 with $nm's launch line removed -- that arm cannot fail"; FAIL=1; fi
  rm -f "$CP"
done

# ---- 7. THE LEG K ORACLE PREDICATE -------------------------------------------------------------
say ""
say "== 7. LEG K's oracleGoVersion predicate, run from the SCRIPT'S OWN regex =="
eval "$(blk "^LEGK_ORACLE_RE=" '^legk_oracle_ok\(\)' 'LEGK oracle predicate')" || exit 3
# ⚠ THE PREDICATE'S OWN BODY IS EXTRACTED TOO, so what runs below is the assembly's live definition
#   and not a copy: the block above stops one line short of it, and this pulls the function itself.
eval "$(grep -a '^legk_oracle_ok()' "$F" | head -1)"
say "   predicate as the script defines it :: [$LEGK_ORACLE_RE]"
P_OK=0
chk(){ local got=0; legk_oracle_ok "$1" && got=1
  if [ "$got" = "$2" ]; then say "   PASS: $3 -- [$1] -> $( [ "$got" = 1 ] && echo ACCEPTED || echo REFUSED )"
  else say "   FAIL: $3 -- [$1] -> $( [ "$got" = 1 ] && echo ACCEPTED || echo REFUSED ), want $( [ "$2" = 1 ] && echo ACCEPTED || echo REFUSED )"; P_OK=1; fi; }
chk 'go version go1.23.12 windows/amd64' 1 'the REAL recorded form is ACCEPTED'
chk 'go version go1.24.13 windows/amd64' 0 'a WRONG RELEASE is REFUSED'
chk 'go1.23.12'                          0 'the BARE TOKEN is REFUSED'
chk 'go version go1.23.1 windows/amd64'  0 'the PREFIX release go1.23.1 is REFUSED'
chk 'go version go1.23.12 linux/arm64'   1 'another GOOS/GOARCH on the right release is ACCEPTED'
chk ''                                   0 'an EMPTY value is REFUSED (the field is omitempty, so an absent one is UNMEASURED and never a pass)'
[ "$P_OK" = "0" ] && say "   PASS: the predicate discriminates in BOTH directions" || FAIL=1
CALLS=$(grep -ac 'legk_oracle_ok "\$OGV"' "$F" || true)
DEFS=$(grep -ac '^LEGK_ORACLE_RE=' "$F" || true)
say "   the predicate is defined $DEFS time(s) (want 1) and LEG K calls it $CALLS time(s) (want 1)"
{ [ "$DEFS" = "1" ] && [ "$CALLS" = "1" ]; } && say "   PASS: one definition, and LEG K consumes it" || { say "   FAIL: the predicate is duplicated or LEG K does not call it"; FAIL=1; }

# ---- 8. THE CLASS SHAPES AGAINST THE **REAL** SEAT DIFFS ---------------------------------------
say ""
say "== 8. the per-seat class shapes against the REAL seat diffs (and against decoys) =="
if [ ! -d "$G/.git" ]; then
  say "   SKIPPED: $G is not a git repository from here"
else
  # ⚠ THE EXTRACTORS READ THE ARM BY A **FIXED PREFIX**, NEVER BY AN ERE BUILT FROM THE CLASS NAME.
  #   `converter-test+docs)` as an ERE means "converter-tes" then one-or-more "t" then "docs)", which
  #   matches nothing -- and the arm would then report a correctly implemented class as missing while
  #   every path read "outside shape".  The live values are still read out of THIS case block, which
  #   is the point: a retyped copy of a pattern is the thing that drifts.
  arm_of(){ awk -v c="$1)" '{ s=$0; sub(/^[ \t]+/,"",s); if (substr(s,1,length(c))==c) { print; exit } }' "$F"; }
  shape_of(){ arm_of "$1" | sed -E "s/.*shapepat='([^']*)'.*/\1/"; }
  forb_of(){  arm_of "$1" | sed -E "s/.*forbidden='([^']*)'.*/\1/"; }
  SH_BAD=0; SH_UNM=0; SHN=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
  while IFS='|' read -r n b s c m a; do
    [ -n "$n" ] || continue
    [ "$s" = "PENDING" ] && { say "   seat $n ($c) $b :: PENDING -- shape NOT checkable, and EXPECTED on this derive"; continue; }
    if ! git -C "$G" cat-file -e "origin/$b^{commit}" 2>/dev/null; then
      say "   row $n ($c) $b :: the remote ref is not present locally -- fetch it before believing this arm; UNMEASURED"
      SH_UNM=$(( SH_UNM + 1 ))
      [ "$T48_SELFCHECK_OFFLINE" = "1" ] && OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))
      SH_BAD=1; continue
    fi
    sob=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    mbref="$BASE_EXPECT"; mbwhy="the train's base [$BASE_EXPECT]"
    if [ -n "$sob" ]; then
      sobsha=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$sob" '$1==r{print $3}')
      if [ -n "$sobsha" ] && [ "$sobsha" != "PENDING" ] && git -C "$G" cat-file -e "${sobsha}^{commit}" 2>/dev/null; then
        mbref="$sobsha"; mbwhy="row $sob's PINNED SHA [$sobsha] (this row DECLARES stack-on=$sob, and merge_seat will see row $sob in HEAD before it reaches this seat)"
      else
        say "   row $n ($c) $b :: declares stack-on=$sob and row $sob pins [${sobsha:-ABSENT}], which does not resolve in this clone -- the STACKED BASE is UNMEASURABLE, and NO reading is taken.  ⚠ Reading it against the train's base instead would credit this row with every file its PARENT changed, which is the over-read this rule exists to remove."
        SH_UNM=$(( SH_UNM + 1 ))
        [ "$T48_SELFCHECK_OFFLINE" = "1" ] && OFFLINE_UNMEASURED=$(( OFFLINE_UNMEASURED + 1 ))
        SH_BAD=1; continue
      fi
    fi
    mb=$(git -C "$G" merge-base "$mbref" "$s" 2>/dev/null)
    pat=$(shape_of "$c"); frb=$(forb_of "$c")
    [ -n "$pat" ] || { say "   seat $n :: could not read the shape pattern for class '$c' out of the script"; SH_BAD=1; continue; }
    files=$(git -C "$G" -c core.quotepath=false diff --name-only "$mb" "$s")
    nfiles=$(printf '%s\n' "$files" | grep -c .)
    # A7 CONSISTENCY (2026-09-13 18:25): a row's own allowed= ruling admits exactly those paths at assembly time (A7 asserts
    # blob identity); this pre-flight arm judged the class WITHOUT it and refused train 48's seat 1 for the one path its ruling
    # names.  The ruling's paths are subtracted here and COUNTED beside the reading, never hidden.
    # ⚠ READ THE RULING AS A key=value FIELD, NEVER POSITIONALLY (COORD 2026-09-13 19:05, D6).  With
    #   two optional keys `read -r n b s c m a` puts fields SIX AND BEYOND into `a` DELIMITERS AND ALL,
    #   so `${a#allowed=}` yields the ERE with `|stack-on=NN` WELDED ONTO IT -- an ALTERNATION nobody
    #   wrote, which widens the exemption to anything matching that literal and makes the ruling's
    #   named paths un-enumerable.  Row 13 of this table carries exactly that shape.
    alw=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')
    if [ -n "$alw" ]; then files_judged=$(printf '%s\n' "$files" | grep -avE "$alw" || true); nalw=$(printf '%s\n' "$files" | grep -acE "$alw" || true); else files_judged="$files"; nalw=0; fi
    out=$(printf '%s\n' "$files_judged" | grep -avE "$pat" | grep -ac . || true)
    # shellcheck disable=SC2086
    bad=$(git -C "$G" -c core.quotepath=false diff --name-only "$mb" "$s" -- $frb | { if [ -n "$alw" ]; then grep -avE "$alw"; else cat; fi; } | grep -ac . || true)
    say "   seat $n ($c) $b @$s :: files=$nfiles outsideShape=$out forbiddenHits=$bad allowedByRuling=$nalw :: measured against $mbwhy -> merge-base $mb"
    if [ "$out" != "0" ] || [ "$bad" != "0" ]; then
      say "      ^ the seat would be REFUSED by its own class.  Paths outside the shape:"
      printf '%s\n' "$files" | grep -avE "$pat" | sed 's/^/        /'
      say "      ⚠ WIDENING A CLASS TO ADMIT THE SEAT THAT VIOLATES IT IS HOW A GATE STOPS BEING A GATE."
      say "        Two remedies, both the coordinator's: (a) the seat moves the offending file to a branch"
      say "        of its own, or (b) the class is widened IN THE SAME COMMIT THAT STATES WHY, with the"
      say "        forbidden list narrowed to match -- a widened shape buys nothing while a forbidden"
      say "        pathspec still refuses the same path."
      SH_BAD=1
    fi
  done <<< "$SEAT_TABLE"
  say "   arm 8 coverage :: rows read=$(( SHN - SH_UNM )) of $SHN :: rows UNMEASURED because their ref is absent from this clone=$SH_UNM"
  if [ "$SH_BAD" = "0" ]; then
    say "   PASS: every FILLED row's REAL diff is admitted by its own class shape and touches none of its forbidden paths"
  elif [ "$SH_UNM" -ge 1 ]; then
    say "   FAIL (UNMEASURED): $SH_UNM row(s) could not be read at all because their ref is not in this clone.  ⚠ THIS IS NOT THE CLAIM THAT A ROW VIOLATES ITS CLASS -- an instrument that could not run has not found nothing.  Any row that DID read outside its shape is NAMED above; if none is, this arm is blind rather than red."
    FAIL=1
  else
    say "   FAIL: a row would be REFUSED by its own class -- and this is a RULING THE COORDINATOR OWNS, not a shape to widen here."
    FAIL=1
  fi
  # DECOYS: each class must REFUSE a path it has no business admitting.
  D1=$(shape_of 'golib-corpus-handown')
  if [ "$(printf '%s\n' 'src/tests/Behavioral/Anything/main.cs' | grep -acE "$D1" || true)" != "0" ]; then
    say "   FAIL (decoy): the golib-corpus-handown shape admits a behavioral project file -- seat 3 adds no guard project and must not be able to"
    FAIL=1
  else
    say "   PASS (decoy): golib-corpus-handown refuses a behavioral project path"
  fi
  D2=$(shape_of 'converter-guard')
  if [ "$(printf '%s\n' 'src/core/golib/slice.cs' | grep -acE "$D2" || true)" != "0" ]; then
    say "   FAIL (decoy): the converter-guard shape admits a golib file -- a converter-guard seat must never touch the corpus"
    FAIL=1
  else
    say "   PASS (decoy): converter-guard refuses a golib file"
  fi
  D3=$(shape_of 'golib-gen')
  if [ "$(printf '%s\n' 'src/gen/go2cs-gen/Generators/ImplementGenerator.cs' | grep -acE "$D3" || true)" != "0" ]; then
    say "   FAIL (decoy): the golib-gen shape admits a GENERATOR outside the Templates tree -- the seat's src/gen half is ONE template and the shape must say so"
    FAIL=1
  else
    say "   PASS (decoy): golib-gen refuses a src/gen file outside the Templates tree, so the widest class in this train is still the tightest shape that admits its seat"
  fi
fi

# ---- 9. THE PER-RUN-COPY DISCIPLINE AND THE FREEZE ANNOUNCEMENTS -------------------------------
say ""
say "== 9. the per-run-copy discipline and the mid-battery freeze announcement lines =="
PRC=$(grep -c 'PER-RUN COPY' "$F")
BYTEOFF=$(grep -c 'BYTE OFFSET' "$F")
STARTS=$(grep -c 'ASSEMBLE START' "$F")
DONES=$(grep -c 'ASSEMBLE DONE' "$F")
say "   'PER-RUN COPY' mentions=$PRC (want >= 1) :: 'BYTE OFFSET' rationale=$BYTEOFF (want >= 1)"
say "   'ASSEMBLE START' stamps=$STARTS (want >= 1) :: 'ASSEMBLE DONE' stamps=$DONES (want >= 3 -- the clean exit, the A-assertion stop and the chain_stop path each own one, and a battery that ends without one is a run nobody can bound)"
{ [ "$PRC" -ge 1 ] && [ "$BYTEOFF" -ge 1 ]; } && say "   PASS: the header carries the per-run-copy discipline WITH its reason (bash reads a script incrementally by byte offset, so an insertion above the running position reparses the next command from the middle of a line)" || { say "   FAIL: the per-run-copy discipline is missing or unexplained"; FAIL=1; }
{ [ "$STARTS" -ge 1 ] && [ "$DONES" -ge 3 ]; } && say "   PASS: the battery announces its START and its DONE -- which is what the MID-BATTERY SOURCE FREEZE is read against: while a battery is running, converter/gen/golib source in that worktree is untouchable, and the freeze binds the harness SCRIPTS too" || { say "   FAIL: the freeze announcements are incomplete (START=$STARTS DONE=$DONES)"; FAIL=1; }

# ---- 10. THE G10d `-text` ARM ------------------------------------------------------------------
say ""
say "== 10. G10d consults the -text ATTRIBUTE (this derive's own instrument fix) =="
CA=$(grep -c 'git check-attr text' "$F")
EOLR=$(grep -c 'git ls-files --eol' "$F")
EXN=$(grep -c 'textExempt=' "$F")
LAYERS=$(grep -c 'LAYERS DISAGREE' "$F")
say "   'git check-attr text' calls=$CA (want >= 1) :: 'git ls-files --eol' reads=$EOLR (want >= 1) :: the exemption COUNT is stamped=$EXN (want >= 1) :: the layers-disagree refusal exists=$LAYERS (want >= 1)"
{ [ "$CA" -ge 1 ] && [ "$EOLR" -ge 1 ] && [ "$EXN" -ge 1 ] && [ "$LAYERS" -ge 1 ]; } && say "   PASS: the exemption is BY ATTRIBUTE and CONDITIONAL on the two layers agreeing, and its count is stamped beside the mismatch count" || { say "   FAIL: G10d does not consult the attribute, or the exemption is unconditional, or its count is not stamped -- an exclusion that is not printed is an exclusion that grows quietly"; FAIL=1; }
# the DECOY: a non-exempt LF docs file must still refuse.  The arm's own refusal branch is the read.
NEX=$(grep -c 'has a non-CRLF working-tree form' "$F")
say "   the refusal for a NON-exempt LF docs file is present=$NEX (want 1) -- the half that keeps this a gate rather than a formality"
[ "$NEX" = "1" ] && say "   PASS: a non-exempt LF docs file still REFUSES" || { say "   FAIL: the non-exempt refusal is missing, so the arm is a blanket pass for docs/"; FAIL=1; }

# ---- 11. blk()'s own negative control -----------------------------------------------------------
say ""
say "== 11. blk()'s negative control: a BROKEN anchor must exit 3, not carry on =="
if [ "${SELFCHECK_NESTED:-0}" = "1" ]; then
  say "   SKIPPED: nested invocation (SELFCHECK_NESTED=1)"
  say ""
  say "=== SELF-CHECK DONE overallFail=$FAIL ==="
  exit $FAIL
fi
BRK=/tmp/${TAG}-broken.sh
sed 's/^SEAT_TABLE="1|/SEAT_TABLE_RENAMED="1|/' "$F" > "$BRK"
sub=$(SELFCHECK_NESTED=1 bash "$0" "$BRK" 2>&1); subrc=$?
say "   self-check against a copy whose SEAT_TABLE anchor is renamed :: exit=$subrc (want 3)"
if [ "$subrc" = "3" ]; then
  say "   PASS (negative control): a broken anchor is reported as the CHECKER being broken, not as a finding about the script"
else
  say "   FAIL (negative control): exit was $subrc, so an unresolvable block would have been read as something other than an instrument fault"
  printf '%s\n' "$sub" | tail -5 | sed 's/^/       /'
  FAIL=1
fi
rm -f "$BRK"

say ""
say "== 12. THE FIVE MECHANISED LESSONS, each with a NEGATIVE CONTROL against the train-46 originals =="
say "   ⚠ EVERY ONE OF THESE IS A DEFECT A TRAIN-46 LAUNCH ACTUALLY HIT.  Each arm must read ZERO on"
say "     the train-48 file AND NON-ZERO on the train-46 one: an arm that has never been made to fire"
say "     is an assertion wearing a measurement's clothes, and this project has already shipped a"
say "     positive control that stayed green because the thing it neutered was subsumed elsewhere."
say "   ⚠ A CONTROL THAT CANNOT RUN IS **UNMEASURED AND FAILS**.  If the train-46 originals are not"
say "     beside this script (override with T46DIR=<dir>), the arm reports that and does not pass:"
say "     an instrument that could not run has not found nothing, it has found nothing out."

# --- the five predicates.  ONE definition each, run over whatever file it is handed, so the arm and
#     its control are literally the same code -- which is what makes the control a control.
#     ⚠ EVERY ONE OF THEM READS **CODE ONLY**.  A file that documents the defect it forbids
#     necessarily SPELLS it, and a predicate that counted the header prose would refuse the very
#     sentence explaining the rule -- route #8's text-grepping family, met from the checker's side.
#     The comment hits are a different claim and are printed separately below.
codeonly(){ grep -av '^[[:space:]]*#' "$1" 2>/dev/null; }
l1_count(){ codeonly "$1" | grep -ac 'SEATS_LISTED" = "[0-9]' | tr -d '\r'; }
l3_count(){ if [ "$(codeonly "$1" | grep -ac 'OWED_CLAUDEMD')" = "0" ]; then echo 1; else echo 0; fi; }
# L4 IS A **MULTI-LINE** PREDICATE, and it was single-line until 2026-09-13.  `grep -acE 'case ... in
#   .{0,4}''` can only see a case whose '' arm sits on the SAME LINE as the `in`; the ordinary
#   way to write the defect --
#       case "${c:-0}" in
#         '') ... ;;
#   -- ESCAPES IT ENTIRELY, so the arm read a clean zero on the shape it exists to find.  A predicate
#   that can only see one spelling of a defect reports the absence of that SPELLING and is quoted as
#   the absence of the DEFECT.  It now tracks the case BLOCK (open at the `case ... in`, close at
#   `esac`) and counts an '' arm anywhere inside it, on the opening line or below.
#   ⚠ AND THE DEFAULT MUST BE **NON-EMPTY**.  `${c:-0}` substitutes 0, so the '' arm is
#   UNREACHABLE -- that is the defect.  `${pa:-}` substitutes the empty string, so its '' arm is
#   REACHABLE and correct; the assembly's seat-preflight loop carries exactly that shape.  The old
#   single-line pattern wrote `:-[^}]*` and would have called the legitimate site a defect the moment
#   it could see across lines, so the class is narrowed to `:-[^}]+` in the same edit.
#   MEASURED 2026-09-13: train48 assemble/land/dry-read/rehearse/self-check = 0; train46 land = 1 and
#   train46 land-dryread = 1 (both same-line, so the controls still fire); a PLANTED multi-line site
#   reads old=0 new=1, which is the arm's own positive control.
# LB1: the unconditional modified-behavioral gate -- `[ "$D_BEHMOD" = "0" ] || {` -- is the defect shape
# LC1: `ls-remote ... | cut -c1-9` compared against a --short HEAD -- the defect shape
lc1_count(){ codeonly "$1" | grep -acE 'refs/heads/master \| cut -c1-9\)' | tr -d '\r'; }
lb1_count(){ codeonly "$1" | grep -acE '^\[ "\$D_BEHMOD" = "0" \] \|\| \{' | tr -d '\r'; }
l4_count(){ codeonly "$1" | awk -v q="''" '
  BEGIN{ casepat="case \"\\$\\{[A-Za-z_][A-Za-z0-9_]*:-[^}]+\\}\" in";
         armpat="(^|[ \t(|;])" q "[ \t]*[)|]";
         esacpat="(^|[ \t;])esac([ \t;]|$)" }
  { if (inc) { if ($0 ~ armpat) { c++; inc=0 } else if ($0 ~ esacpat) { inc=0 } }
    if (match($0, casepat)) { rest=substr($0, RSTART+RLENGTH);
      if (rest ~ armpat) c++; else inc=1 } }
  END{ print c+0 }' | tr -d '\r'; }
l9_count(){ codeonly "$1" | grep -ac -- '| grep -q' | tr -d '\r'; }
l11_count(){ codeonly "$1" | awk '/(^|[^A-Za-z0-9_])FAILED=1/ { if ($0 !~ /stamp/ && prev !~ /stamp/) c++ } { prev=$0 } END{ print c+0 }'; }
# LD1: a train-number LITERAL in LEG D's prediction artifact header (code lines only) -- the original spells its own train, the derive must spell none.
ld1_count(){ codeonly "$1" | grep -acE "printf 'TRAIN [0-9]+ LEG D" | tr -d '\r'; }
# LD2: the LEG D emittable/blind classifier LACKS the hand-owned disclosure-manifest class -- 1 when the case line is absent, 0 when present.
# ⚠ anchored on a LIVE `case` at line start: codeonly strips whole-line comments only, so an unanchored match would
#   read a TRAILING comment as the classifier and pass a file with no classifier at all (verifier finding, 2026-09-13).
# LR1: a -tests pipeline leg (LEG R / LEG U) whose restore names only its corpus root -- the pipeline also rewrites
#   docs/validation/current/<row>.md and index.md, and a root-scoped restore leaves them behind (run 5's red at LEG U).
lr1_count(){ codeonly "$1" | grep -acE '^[[:space:]]*git checkout -- (src/core|"\$LEGU_DIR") 2>/dev/null' | tr -d '\r'; }
# LA1: a FAILED=1 setter sitting under a stamp that says 'never fatal' -- the stamp's TEXT contradicts the flag it precedes
#   (run 6's LEG 4 advisory-warning arm: 52 vs a baseline of 2, 'counted, never fatal', FAILED=1 on the next line).
la1_count(){ codeonly "$1" | awk 'BEGIN{c=0} { if ($0 ~ /(^|[^A-Za-z0-9_])FAILED=1/ && prev ~ /never fatal/) c++; prev=$0 } END{ print c+0 }' | tr -d '\r'; }
ld2_count(){ if [ "$(codeonly "$1" | grep -acE '^[[:space:]]*case "\$f" in \*/go2cs_test_disclosures\.json\) blind=1' | tr -d '\r')" = "0" ]; then echo 1; else echo 0; fi; }

lesson(){ # $1 label  $2 predicate  $3 the train-48 file  $4 the train-46 original  $5 what it measures
  local lab="$1" fn="$2" new="$3" old="$4" what="$5" n o
  n=$("$fn" "$new"); n="${n:-0}"
  if [ ! -f "$old" ]; then
    say "   $lab :: train48=$n (want 0) :: ⚠ **CONTROL UNMEASURED** -- [$old] is not present, so the arm has not been shown able to fire.  $what"
    FAIL=1; return
  fi
  o=$("$fn" "$old"); o="${o:-0}"
  if [ "$n" = "0" ] && [ "${o:-0}" -ge 1 ]; then
    say "   PASS $lab :: train48=$n train46=$o -- the arm reads ZERO on the derived file and FIRES on the original.  $what"
  elif [ "$n" != "0" ]; then
    say "   FAIL $lab :: train48=$n (want 0) train46=$o -- the defect SURVIVED the derive.  $what"
    FAIL=1
  else
    say "   FAIL $lab (CONTROL) :: train48=$n train46=$o (want >= 1) -- the arm did NOT fire on the file that carries the defect, so its zero on the derived file says nothing.  $what"
    FAIL=1
  fi
}

# ⚠ THE CONTROL FILENAMES ARE **COMPOSED**, NOT SPELLED.  The derive that writes this file runs a
#   global `coord-train46-* -> coord-train48-*` rename over it, and a spelled name here would be
#   rewritten to point the control at the file it is supposed to be controlling AGAINST -- which is
#   exactly what happened on this arm's first run: L3 and L11 read their own output as the original
#   and reported the control as dead.  An instrument built out of the thing under test cannot
#   independently measure it.
T46NUM=46
T46ASM="$T46DIR/coord-train${T46NUM}-assemble.sh"
T46LAND="$T46DIR/coord-train${T46NUM}-land.sh"
LAND47="$SELFDIR/coord-train48-land.sh"
say "   the originals this arm controls against :: $T46ASM :: $T46LAND"
lesson 'L1 no literal seat count' l1_count "$F" "$T46ASM" \
  "Train 46 run 1 merged all six seats and then refused on \`[ \"\$SEATS_LISTED\" = \"5\" ]\` -- a fact about the derive, not about the train.  The count is now DERIVED and what is asserted is the table's STRUCTURE."
lesson 'L3 G4 derived from the classes' l3_count "$F" "$T46ASM" \
  "Train 46 run 3 refused with 'CLAUDE.md is UNTOUCHED ... seat 1 IS the doctrine batch' -- train 45's premise carried into a train with no doctrine seat.  G4 now reads OWED_CLAUDEMD and refuses only the MISMATCH, in either direction."
# X L4's CONTROL IS POINTED AT THE **LAND** AND **DRY-READ** FILES, NOT THE ASSEMBLY, AND THAT IS A
#   MEASUREMENT RATHER THAN A CHOICE: train 46's G10a site was CORRECTED IN FLIGHT after run 3
#   refused on it, so the train-46 assembly no longer carries the shape and a control against it
#   reads a dead ZERO.  The identical shape DID survive in the land script and in its dry-read,
#   where nothing on that train was looking -- a defect fixed in one file and left standing in
#   another is a defect that survives the derive.  The control therefore fires where the defect
#   lives, and the assembly's own zero is reported beside it as an ASSERTION carried by the SAME
#   predicate -- which is the only reason that zero is worth anything.
say "   reading  L4 (assembly) :: train48=$(l4_count "$F") train46=$(l4_count "$T46ASM") -- BOTH zero is EXPECTED here: the train-46 site was corrected in flight at run 3, so this predicate's control lives on the two files below"
[ "$(l4_count "$F")" = "0" ] || { say "   FAIL L4 (assembly) :: the case-default shape is present in the derived assembly"; FAIL=1; }
lesson 'LC1 the land read-back compares full SHAs' lc1_count "$LAND47" "$T46LAND" \
  "Train 47 run 8 LANDED (origin/master == the announced 40-char SHA) and the land declared NOT LANDED, exit 5, because it compared a 9-char ls-remote cut against a 10-char --short HEAD; the prune loop and the LAND DONE stamp never ran.  A short-SHA comparison is a comparison of two different quantities."
lesson 'LB1 the land honours A7 admissions read from the record' lb1_count "$LAND47" "$T46LAND" \
  "Train 47 run 8's record carried seven behavioral files admitted by seat 8's allowed= ruling (A7 stamped each with a blob identity) and the land refused the landing on an UNCONDITIONAL modified-count gate that contradicted its own LANDING NOTES (5).  The gate now reads the admissions from the record and fails only on an unadmitted file."
lesson 'L4 no case-default (land)' l4_count "$LAND47" "$T46LAND" \
  "The shape survived in the LAND script, where nothing in train 46 looked for it.  ${VAR:-0} substitutes before the '' arm can be reached, so the empty case is unreachable and the value is never normalised -- which is how G10a printed 'deletions=' EMPTY and refused."
lesson 'L4 no case-default (dry-read)' l4_count "$SELFDIR/coord-train48-land-dryread.sh" "$T46DIR/coord-train${T46NUM}-land-dryread.sh" \
  "And in the DRY-READ, whose whole job is to check the land script -- a checker carrying the very defect it checks for is the shape this arm exists to catch."
lesson 'L9 zero | grep -q' l9_count "$F" "$T46ASM" \
  "Under \`set -o pipefail\` a \`grep -q\` SIGPIPEs its producer and a TRUE match reads as a failure.  This tree does not set pipefail; carrying the shape anyway leaves a landmine per site for the derive that does."
lesson 'L11 every FAILED=1 names its gate' l11_count "$F" "$T46ASM" \
  "Train 46 carried FIFTEEN setters with no stamp on the same or the previous line; a reader scanning for the flag could not tell which gate set it, and one of them cost a run its attribution.  Each is now fail_gate <GATE>."
lesson 'LD1 no train-number literal in the LEG D artifact header' ld1_count "$F" "$T46ASM" \
  "Train 47 run 4's prediction artifact was headed 'TRAIN 46 LEG D' -- a literal carried through the derive.  The header is now composed from \$TAG, so it names the train that wrote it."
lesson 'LR1 pipeline-leg restores name docs/validation' lr1_count "$F" "$T46ASM" \
  "Train 47 run 5 read LEG U's post-restore tree dirty=2 (the row's proof page and the index, rewritten by the -tests emission) with the package byte-identical -- a restore scoped to the corpus root alone.  Both pipeline legs now restore src/core|the package AND docs/validation, as the final cleanup always did."
lesson 'LA1 no fatal flag under a never-fatal stamp' la1_count "$F" "$T46ASM" \
  "Train 47 run 6 ended overallFailed=1 with every leg green: LEG 4 stamped the advisory count as a FINDING, 'counted, never fatal', and set FAILED=1 on the next line.  The arm now derives its expectation from the classified kinds and refuses only a count outside it or an unnamed kind, and the stamp says which."
lesson 'LD2 LEG D classifier knows the disclosure-manifest class' ld2_count "$F" "$T46ASM" \
  "Train 47 run 4 predicted two hand-owned go2cs_test_disclosures.json manifests as EMITTABLE and read MISSED x3 against a diff that correctly read ZERO -- a false red of the prediction; -stdlib never writes a manifest.  The classifier now names the class beside golib, *_impl.cs and whole-file hand-owns."

# --- and the two arms that are ASSERTIONS about the derived file alone (no control is possible: they
#     assert the PRESENCE of a mechanism the original does not have at all, and 'absent from the
#     original' is what every other arm here already measures).
for tok in 'OWED VECTOR ::' 'SEAT-CONTENT ASSERTIONS ::' 'fail_gate(){' 'A7 ALLOWED SET ::' 'CENSUS_TEMPLATE=' 'CONTROL C1/G6T template class'; do
  n=$(grep -acF -- "$tok" "$F" || true)
  if [ "${n:-0}" -ge 1 ]; then say "   PASS mechanism present :: [$tok] x$n"
  else say "   FAIL mechanism ABSENT :: [$tok] -- the derive did not land it"; FAIL=1; fi
done
say "   ⚠ CENSUS_TEMPLATE AND THE C1/G6T SIX-ARM CONTROL ARE CARRIED **BYTE-FOR-BYTE** FROM TRAIN 46."
say "     They were ruled after run 5 read residual=11 on the security guard's own Sprintf fixtures;"
say "     they are proven, and a derive that retyped them would be re-deriving a measured answer."
say "   ⚠ AND THE set-u/pipefail RATIONALE MUST BE **STATED IN EVERY HEADER**, because a rule whose reason is"
say "     not written down is a rule the next derive silently 'improves'."
for f in "$F" "$SELFDIR/coord-train48-rehearse.sh" "$LAND47" "$SELFDIR/coord-train48-land-dryread.sh" "${BASH_SOURCE[0]}"; do
  [ -f "$f" ] || { say "   FAIL: [$f] is not present, so its header could not be read"; FAIL=1; continue; }
  n=$(grep -ac 'NOT.*pipefail\|not.*pipefail' "$f" || true)
  p=$(grep -ac -- '| grep -q' "$f" || true)
  if [ "${n:-0}" -ge 1 ]; then say "   ok   $(basename "$f") :: states the set -u/pipefail rationale ($n line(s)) :: '| grep -q' sites=$p"
  else say "   FAIL $(basename "$f") :: does NOT state why it is \`set -u\` and not pipefail"; FAIL=1; fi
done

say ""
say "== 13. THE FILL POINTS AND THIS TRAIN'S OWN RULED CHANGES, each asserted rather than assumed =="
say "   ⚠ ARM 12 ABOVE ASKS 'did the TEMPLATE's known defects survive the derive'.  THIS ARM ASKS THE"
say "     OTHER HALF: 'did what the coordinator RULED for train 48 actually land in the file'.  A fill"
say "     point that is merely no longer a placeholder is not a fill point that was filled correctly."
A13=0
a13(){ # $1 ok(0/1)  $2 what
  if [ "$1" = "1" ]; then say "   ok   $2"; else say "   FAIL: $2"; A13=1; fi
}
LANDF="$SELFDIR/coord-train48-land.sh"
# --- F1/F2/F3, each read as a FILLED value and not merely as a non-placeholder.
WTVAL=$(grep -aE '^WT="\$\{TRAIN48_WT:-' "$F" | head -1 | sed -E 's/^WT="\$\{TRAIN48_WT:-(.*)\}"$/\1/')
case "$WTVAL" in
  ''|*PENDING*) a13 "$(grep -ac 'WORKTREE UNFILLED ::' "$F" | tr -d '\r')" "F1 the assembly worktree is a FILL POINT reading [$WTVAL], and the run ABORTS until it is named -- what is asserted on a TEMPLATE is that the REFUSAL exists" ;;
  *) a13 1 "F1 the assembly worktree is FILLED :: [$WTVAL]" ;;
esac
a13 "$( [ -n "${ECON:-}" ] && echo 1 || echo 0 )" "F2 the CONTAINMENT pin is declared :: [${ECON:-ABSENT}] -- this train's base is a revspec, so the pin is the only thing that says anything about the base's CONTENT"
FILLEDN=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3!="PENDING"' | grep -ac . || true)
PENDN=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3=="PENDING"' | grep -ac . || true)
say "   F3 the seat table :: rows=$ROWS filled=$FILLEDN pending=$PENDN"
a13 "$( [ "${FILLEDN:-0}" -ge 1 ] && echo 1 || echo 0 )" "F3 at least one row is FILLED (arm 2 asserts the structure and arm 3 that every filled SHA resolves; arm 8 that each one's real diff fits its class)"
# --- one SEAT-CONTENT arm per FILLED row.  ⚠ The assembly refuses at run time when the arm count is
#     below the MERGED count; this reads the same relation statically, against the FILLED count, so a
#     table filled without its arms is caught at derive time rather than after the merges.
#     ⚠ CODE ONLY, by the SAME predicate arm 12 uses: the fenced block carries a WORKED EXAMPLE in a
#     comment, and a counter that read it would report one arm more than the file has.
ARMN=$(codeonly "$F" | grep -ac 'SEATASSERT_N=$(( SEATASSERT_N + 1 ))' || true)
say "   SEAT-CONTENT arms written=$ARMN :: filled rows=$FILLEDN (the arms must not be fewer; a PENDING row legitimately has none, and the assembly REFUSES a REQUIRE_ALL run until they are written)"
SCBLK=$(awk '/=== SEAT-CONTENT ASSERTIONS BEGIN/{inb=1; next} /=== SEAT-CONTENT ASSERTIONS END ===/{inb=0} inb' "$F" | grep -av '^[[:space:]]*#')
SCARM=$(printf '%s\n' "$SCBLK" | grep -acF 'SEATASSERT_N=$(( SEATASSERT_N + 1 ))' || true)
# ⚠ THE REGRESSED-COPY CONTROL: the SAME predicate over a copy of the block with ONE increment
#   deleted must read one lower AND must MISS the filled-row count.  A count that agreed either way
#   would be a green that cannot go red, which is the safety floor's item 13 verbatim.
SCCTL=$(printf '%s\n' "$SCBLK" | awk 'f==0 && index($0, "SEATASSERT_N + 1") > 0 {f=1; next} {print}' | grep -acF 'SEATASSERT_N=$(( SEATASSERT_N + 1 ))' || true)
say "   SEAT-CONTENT arms INSIDE the fenced block=$SCARM :: filled rows=$FILLEDN :: the regressed copy (one increment deleted) reads $SCCTL"
a13 "$( [ "${SCARM:-0}" = "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "the FENCED BLOCK carries EXACTLY one arm per FILLED row ($SCARM == $FILLEDN) -- a file-wide count cannot say this"
a13 "$( { [ "${SCCTL:-0}" = "$(( SCARM - 1 ))" ] && [ "${SCCTL:-0}" != "${FILLEDN:-0}" ]; } && echo 1 || echo 0 )" "CONTROL RED :: the same predicate over a REGRESSED copy reads $SCCTL and misses $FILLEDN, so the equality above can go red"
# ⚠ ON A **TEMPLATE** THE SLOT IS EMPTY AND THAT IS THE CORRECT STATE.  What is asserted is the
#   SLOT and the GATE: the fenced block exists and the assembly REFUSES when the arm count is
#   below the merged-seat count.  A template shipping arms would ship assertions about content
#   nobody seated, and a landing would then refuse a healthy battery under a dead row's name.
if [ "${ARMN:-0}" = "0" ]; then
  SLOTB=$(grep -ac 'SEAT-CONTENT ASSERTIONS BEGIN' "$F" | tr -d '\r')
  SLOTG=$(grep -ac 'SEAT-CONTENT ASSERTIONS REFUSED' "$F" | tr -d '\r')
  a13 "$( [ "${SLOTB:-0}" -ge 1 ] && [ "${SLOTG:-0}" -ge 1 ] && echo 1 || echo 0 )" "the SEAT-CONTENT block is an EMPTY SLOT carrying its GATE (fenced block x${SLOTB:-0}, shortfall refusal x${SLOTG:-0}); the arms are written at seat fill, one per named row"
else
  a13 "$( [ "${ARMN:-0}" -ge "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "every FILLED row has a content assertion"
fi
# --- carried defect (a): the un-prefixed switch is REFUSED, and the documented launcher EXPORTS the
#     prefixed one.  A launcher exporting REQUIRE_ALL would configure nothing and read as a landing.
a13 "$( [ "$(grep -ac 'REQUIRE_ALL is in the ENVIRONMENT reading' "$F")" = "1" ] && echo 1 || echo 0 )" "defect (a) the inherited un-prefixed REQUIRE_ALL is REFUSED BY NAME before the script's own assignment overwrites it"
a13 "$( [ "$(grep -ac 'export TRAIN48_REQUIRE_ALL=1' "$F")" -ge 1 ] && echo 1 || echo 0 )" "defect (a) the documented launch wrapper EXPORTS the PREFIXED switch"
a13 "$( [ "$(grep -ac 'SKIP_PENDING is in the ENVIRONMENT reading' "$F")" = "1" ] && echo 1 || echo 0 )" "the un-prefixed SKIP_PENDING is refused too -- one switch fixed and its twin left open is the defect surviving the derive"
# --- ⚠ THE **INVERTED DEFAULT**: with a FILLED table a PENDING row REFUSES, and the skip is an opt-in.
SKD=$(codeonly "$F" | grep -ac 'TRAIN48_SKIP_PENDING' || true)
SKREF=$(grep -ac 'TRAIN48_SKIP_PENDING is not 1' "$F" || true)
SKCON=$(grep -ac 'are a CONTRADICTION' "$F" || true)
say "   the PENDING default :: TRAIN48_SKIP_PENDING read in CODE=$SKD (want >= 3: the un-prefixed guard, the declared/empty arm and the preflight) :: the preflight REFUSAL naming it=$SKREF (want 1) :: REQUIRE_ALL+SKIP_PENDING refused as a contradiction=$SKCON (want 1)"
a13 "$( { [ "${SKD:-0}" -ge 3 ] && [ "${SKREF:-0}" = "1" ] && [ "${SKCON:-0}" = "1" ]; } && echo 1 || echo 0 )" "a PENDING row REFUSES by default and the skip must be opted into BY NAME, with the two switches refused together rather than ordered by precedence"
# --- carried defect (b): nested depth by SEGMENT COUNT in BOTH instruments, and no case glob anywhere.
# ⚠ CODE ONLY, by the same predicate arm 12 uses.  BOTH instruments now carry a COMMENT that SPELLS the
#   forbidden glob in order to record why it is forbidden, and a counter that read the prose would
#   refuse the very sentence explaining the rule -- route #8's text-grepping family, met from the
#   checker's side.  The comment hits are printed beside the code count so the zero is not read as
#   "the shape appears nowhere".
GLOBHITS=$(codeonly "$F" | grep -ac 'src/tests/Behavioral/\[!/\]\*' || true)
GLOBHITSL=$(codeonly "$LANDF" | grep -ac 'src/tests/Behavioral/\[!/\]\*' || true)
GLOBCMT=$(grep -ac 'src/tests/Behavioral/\[!/\]\*' "$F" || true)
say "   defect (b) shell-glob depth tests remaining IN CODE :: assembly=$GLOBHITS land=$GLOBHITSL (must both be 0 -- \`*\` CROSSES \`/\` in a shell pattern, so a nested sub-library matches the top-level form and is asserted by nothing) :: the same shape in the assembly's PROSE=$GLOBCMT (EXPECTED and required: it is the record of why the rule exists)"
a13 "$( { [ "${GLOBHITS:-1}" = "0" ] && [ "${GLOBHITSL:-1}" = "0" ]; } && echo 1 || echo 0 )" "defect (b) no case-glob depth test survives in either instrument"
a13 "$( [ "$(grep -ac 'SEGMENT COUNT' "$F")" -ge 1 ] && echo 1 || echo 0 )" "defect (b) the assembly decides TOP-LEVEL by SEGMENT COUNT and says so"
a13 "$( [ "$(grep -ac 'SEGMENT COUNT' "$LANDF")" -ge 1 ] && echo 1 || echo 0 )" "defect (b) the LAND script does the same -- both, because route #3 is the two populations being confused and one fixed instrument does not fix the pair"
a13 "$( [ "$(grep -ac "awk -F/ 'NF==5'" "$LANDF")" -ge 1 ] && echo 1 || echo 0 )" "defect (b) the land script's TOP-LEVEL population is NF==5 (src/tests/Behavioral/P/P.csproj)"
# --- carried defect (c): the land's req anchors name THIS train's stamps.  The dry-read's ARM A is the
#     full measurement; these are the anchors that could ONLY come from this train.
# ⚠ THE `A-row8 hand-own metadata unfreeze` MEMBER WAS REMOVED 2026-09-13 WITH ITS SEAT: the former
#   row 8 was UNSEATED, its content arm DELETED from the assembly and its anchor DELETED from the
#   land, so requiring it here would be this checker asserting a stamp nobody writes.  It is
#   REPLACED by the surviving row 8's own both-halves anchor (the generic-alias seat), which is
#   equally a stamp only THIS train produces -- and arm 14 below now reads EVERY anchor this way.
for rq in 'LEG U ARM 1' 'LEG U ARM 2 MET' 'LEG U ARM 3 MET' 'prediction FILTERED per target' 'FILL POINTS OK ::' 'SEAT TABLE DECLARED STACKS ::' 'PATCH-ID ARM' 'BASE ANCESTRY OK'; do
  a13 "$( [ "$(grep -acF -- "$rq" "$LANDF")" -ge 1 ] && echo 1 || echo 0 )" "defect (c) the land script requires a stamp only THIS train produces :: [$rq]"
done
# --- carried defect (d): LEG D's per-target filter, present AND consumed.  A filter computed and not
#     used is the shape that produced three false reds on train 46 run 7.
EXPG=$(grep -ac 'EXPGOOS=' "$F" || true)
EXPGUSE=$(grep -ac 'sort -u "\$EXPGOOS"' "$F" || true)
OLDUSE=$(grep -ac 'sort -u "\$LEGD_EXPSET") "\$GOTSET"' "$F" || true)
say "   defect (d) LEG D per-target filter :: EXPGOOS assignments=$EXPG (want 1) :: comparisons reading the FILTERED set=$EXPGUSE (want 4 -- EXTRA, MISSING and the two named-file listings) :: comparisons still reading the UNFILTERED union=$OLDUSE (must be 0)"
a13 "$( { [ "${EXPG:-0}" = "1" ] && [ "${EXPGUSE:-0}" -ge 4 ] && [ "${OLDUSE:-1}" = "0" ]; } && echo 1 || echo 0 )" "defect (d) the per-target filter is computed, STAMPED, and consumed by every comparison"
# --- G4, RE-INVERTED BY DERIVATION.  This train seats NO doctrine row (batch19 is retired and
#     preserved by tag), so the expectation is CLAUDE.md NOT in the delta and a touched CLAUDE.md is a
#     refusal.  ⚠ ASSERTED AS A PROPERTY OF THE TABLE, not as a literal: OWED_CLAUDEMD is derived from
#     the merged CLASSES, so the table carrying no `doctrine` row IS the inversion.
DOCTROWS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$4=="doctrine"' | grep -ac . || true)
say "   G4 :: rows carrying class \`doctrine\`=$DOCTROWS (must be 0 for this train) -- so OWED_CLAUDEMD derives to 0 and G4's expectation is CLAUDE.md UNTOUCHED"
a13 "$( [ "${DOCTROWS:-1}" = "0" ] && echo 1 || echo 0 )" "G4 is RE-INVERTED by derivation: no doctrine row, so an untouched CLAUDE.md is the EXPECTED reading"
a13 "$( [ "$(grep -ac 'G4 REFUSED: CLAUDE.md is in the delta and NO merged seat carries class' "$F")" = "1" ] && echo 1 || echo 0 )" "G4 REFUSES a TOUCHED CLAUDE.md when none is owed -- the half that makes the inversion a gate rather than a silence"
# --- LEG U, the utf8 arm: three arms and a restore, each required, because a one-direction control is
#     an assertion.
for lu in 'LEG U ARM 1 (CLEAN, no manifest)' 'LEG U ARM 2 (NEGATIVE' 'LEG U ARM 3 (POSITIVE' 'LEG U post-restore ::' '-test-action all -test-timeout 10m'; do
  a13 "$( [ "$(grep -acF -- "$lu" "$F")" -ge 1 ] && echo 1 || echo 0 )" "LEG U carries [$lu]"
done
a13 "$( [ "$(grep -ac 'LEG U UNMEASURED :: .*ALREADY EXISTS' "$F")" -ge 1 ] && echo 1 || echo 0 )" "LEG U REFUSES rather than backing up a committed manifest at the plant path -- 'restore byte-identical' has to be a property, not a claim"
# --- the KEEP list: four branches, named, and none of them is a seat in this table.
KEEPL=$(grep -aE "^KEEP_BRANCHES='" "$LANDF" | head -1 | sed -E "s/^KEEP_BRANCHES='([^']*)'.*/\1/")
say "   KEEP_BRANCHES :: [$KEEPL]"
KOK=1
for k in claude/laneR-waitreason-47 claude/c1-h6-rewrites claude/g-b1-box-design claude/mailbox; do
  case " $KEEPL " in *" $k "*) : ;; *) say "   FAIL: [$k] is NOT on the land script's KEEP list"; KOK=0 ;; esac
  if [ "$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$k" '$2==r' | grep -ac . || true)" != "0" ]; then
    say "   FAIL: [$k] is BOTH a seat in this table and on the KEEP list -- one of the two is wrong and this instrument must not pick"
    KOK=0
  fi
done
a13 "$KOK" "the four never-pruned branches are on the KEEP list and none of them is a seat"
# --- and the unbound-variable fault found at derive time.
a13 "$( [ "$(grep -acE '^WT_DEFAULT=' "$LANDF")" = "1" ] && echo 1 || echo 0 )" "the land script ASSIGNS WT_DEFAULT before stamping it (it was referenced and never assigned, which is an unbound-variable abort on the FIRST stamp of every landing under \`set -u\`)"
[ "$A13" = "0" ] && say "   PASS: every fill point and every ruled change for this train is present in the files" || FAIL=1

say ""
say "== 14. EVERY \`req\` ANCHOR IN THE LAND SCRIPT NAMES A STAMP THE ASSEMBLY **EMITS**, BY LITERAL =="
say "   ⚠ WHY THIS ARM EXISTS, AND WHAT IT IS NOT.  Arm 13's defect-(c) loop names a HANDFUL of anchors"
say "     and the dry-read's ARM A checks every anchor's substantial WORDS -- and a word-level read"
say "     passes when those words survive somewhere else in the file, which is exactly what happens"
say "     when a SEAT IS UNSEATED and its content arm is deleted while the land script still requires"
say "     that arm's stamp BY NAME.  The landing would then refuse a HEALTHY battery, and the operator"
say "     would reach for the switch that makes it stop refusing.  ⚠ THAT IS THE CLASS THIS TRAIN CAME"
say "     WITHIN ONE EDIT OF SHIPPING: the former row 8 was UNSEATED after run 1 and its A-row8 arm"
say "     deleted, and the land carried the matching anchor until this arm was written (2026-09-13)."
say "   ⚠ WHAT IT ASSERTS IS A **LITERAL PREFIX**, NOT THE WHOLE PATTERN, and the difference is the"
say "     honest one: a stamp INTERPOLATES (\`stamp \"LEG 2 stdlib slnx exit=\$rc\"\`), so no static read"
say "     can match past the first variable.  The prefix is taken up to the first regex metacharacter,"
say "     the first bracket or parenthesis (escaped or not -- a stamp writes those around an"
say "     interpolation), and the first \`=\` (inclusive).  A prefix under ten characters is REPORTED as"
say "     too thin to assert rather than counted as a pass -- a check that asserts nothing must say so."
lit_prefix(){ # $1 = an ERE -> its leading LITERAL run, the part a stamp's SOURCE can carry verbatim
  python - "$1" <<'PYEOF' | tr -d '\r'
import sys
pat = sys.argv[1]
out = []
i = 0
while i < len(pat):
    c = pat[i]
    if c == chr(92) and i + 1 < len(pat):
        if pat[i + 1] in '[]()':
            break                                    # an escaped bracket is a LITERAL one, and a
        out.append(pat[i + 1]); i += 2; continue     # stamp writes brackets around an INTERPOLATION
    if c in '().*+?^$|{}[]':
        break
    out.append(c); i += 1
s = ''.join(out)
if '=' in s:
    s = s[:s.index('=') + 1]                         # the VALUE comes from a variable at run time
print(s.rstrip())
PYEOF
}
RQN=0; RQMISS=0; RQTHIN=0; RQDQ=0; RQCOMMENTONLY=0
while IFS= read -r line; do
  pat=$(printf '%s' "$line" | sed -E "s/^[[:space:]]*req +'([^']*)'.*/\1/")
  if [ "$pat" = "$line" ]; then
    # LL1: a DOUBLE-quoted req (the form a pattern with an apostrophe must take).  The raw text carries bash's
    # own escapes, so a doubled backslash collapses to one before the ERE is read the way grep will read it.
    pat=$(printf '%s' "$line" | sed -E 's/^[[:space:]]*req +"((\\.|[^"\\])*)".*/\1/' | sed -E 's/\\\\/\\/g')
    [ "$pat" != "$line" ] || continue
    RQDQ=$(( RQDQ + 1 ))
  fi
  RQN=$(( RQN + 1 ))
  pre=$(lit_prefix "$pat")
  if [ "${#pre}" -lt 10 ]; then
    RQTHIN=$(( RQTHIN + 1 ))
    say "   thin (${#pre}-char prefix) [$pre] <- req [$pat]"
    continue
  fi
  if grep -aF -- "$pre" "$F" | grep -avE '^[[:space:]]*#' | grep -aq .; then
    say "   ok   [$pre]"
  elif grep -aqF -- "$pre" "$F"; then
    RQMISS=$(( RQMISS + 1 )); RQCOMMENTONLY=$(( RQCOMMENTONLY + 1 ))
    say "   MISS the literal [$pre] appears ONLY IN A COMMENT of the assembly -- prose is not a stamp   <- from req [$pat]"
  else
    RQMISS=$(( RQMISS + 1 ))
    say "   MISS the assembly emits NO stamp containing [$pre]   <- from req [$pat]"
  fi
done < <(grep -aE "^[[:space:]]*req ['\"]" "$LANDF")
say "   LL1 double-quoted req anchors read=$RQDQ (must be >= 1: the E1' MET req carries an apostrophe and can only be double-quoted; a zero here means the extraction is dead, not that none exist) :: comment-only matches=$RQCOMMENTONLY"
[ "$RQDQ" -ge 1 ] || { say "   FAIL: LL1 the double-quoted req extraction read ZERO anchors -- it is dead, and a dead reader passed the markdown E1' MET form for two trains"; FAIL=1; }
say "   req anchors read=$RQN :: asserted by LITERAL PREFIX=$(( RQN - RQTHIN )) :: anchors the assembly does NOT emit=$RQMISS :: too thin to assert (a metacharacter or an interpolation inside the first ten characters) and REPORTED rather than counted=$RQTHIN"
[ "$RQMISS" = "0" ] && say "   PASS: every land anchor with a substantial literal prefix names a stamp this assembly actually writes" || { say "   FAIL: a land anchor names a stamp the assembly does not emit -- it can NEVER be satisfied and the landing would refuse a healthy battery.  An anchor whose ARM was deleted is deleted WITH it."; FAIL=1; }

say ""
say "== (a) NO SEAT-NUMBER LITERAL CARRIES A CLAIM, in the assemble, the land and the dry-read =="
say "   ⚠ Train 47's assembly made THIRTY-EIGHT assertions of the form \"seat N is ...\", \"seat N's ...\""
say "     or \"row N is a <class> seat\" -- in stamps AND in the prose beside gates.  Each was a fact"
say "     about THAT table, and a derive inherits them silently because they read like documentation."
say "   ⚠ THE PREDICATE IS ONE DEFINITION RUN OVER WHATEVER FILE IT IS HANDED, so the arm and its"
say "     control are literally the same code -- which is what makes the control a control."
say "   ⚠ THE PREDICATE IS **CASE-INSENSITIVE**, AND THAT IS A CORRECTION.  The first cut of this arm"
say "     used grep -acE and read ZERO on the derived assembly while ELEVEN of the forbidden shape"
say "     survived in it -- ten as SHOUTED prose beside a gate, and one as a LIVE LEG K STAMP that"
say "     announced a row number as the provenance of the \`sync\` gate.  An arm that cannot see upper"
say "     case reports the loudest instance of its own defect as clean."
SEATLIT_RE="seat [0-9]+ is |seat [0-9]+'s |row [0-9]+ is a"
seatlit_count(){ grep -aciE "$SEATLIT_RE" "$1" 2>/dev/null | tr -d '\r'; }
ABAD=0
for f in "$F" "$SELFDIR/coord-train48-land.sh" "$SELFDIR/coord-train48-land-dryread.sh"; do
  if [ ! -f "$f" ]; then say "   FAIL: [$f] is absent, so the arm could not run over it"; ABAD=1; continue; fi
  n=$(seatlit_count "$f"); n="${n:-0}"
  if [ "$n" = "0" ]; then say "   ok   $(basename "$f") :: seat-number literals=0"
  else say "   FAIL $(basename "$f") :: $n seat-number literal(s) survive:"; grep -aniE "$SEATLIT_RE" "$f" | head -8 | cut -c1-180 | sed 's/^/        /'; ABAD=1; fi
done
# --- THE RED CONTROL.  ⚠ IT IS **MEASURED**, NOT ASSUMED: the train-47 assembly carries the shape and
#     is the honest control; the train-47 LAND and DRY-READ read ZERO for it, so for those two the
#     control is a PLANTED line instead, and that difference is REPORTED rather than papered over.
if [ -f "$T47ASM" ]; then
  c=$(seatlit_count "$T47ASM"); c="${c:-0}"
  if [ "$c" -ge 1 ]; then say "   ok   CONTROL (train-47 assembly) :: the same predicate reads $c -- the arm CAN fire"
  else say "   FAIL CONTROL: the predicate reads 0 on the train-47 assembly, so its zero on the derived file says nothing"; ABAD=1; fi
else
  say "   FAIL CONTROL UNMEASURED :: [$T47ASM] is not present (override with T47DIR=<dir>) -- an arm that has never been made to fire is an assertion wearing a measurement's clothes"
  ABAD=1
fi
PLANT="/tmp/${TAG}-seatlit-control.txt"
{ printf '%s\n' 'stamp "row 4 is a docs seat, which is the premise this arm forbids"'; printf '%s\n' "# seat 7's own deliverable"; } > "$PLANT"
c=$(seatlit_count "$PLANT"); c="${c:-0}"
if [ "$c" -ge 2 ]; then say "   ok   CONTROL (planted, for the LAND and DRY-READ whose train-47 originals read ZERO) :: the predicate reads $c on two planted lines"
else say "   FAIL CONTROL: the predicate reads $c on a planted file carrying two of the forbidden shapes"; ABAD=1; fi
rm -f "$PLANT"
[ "$ABAD" = "0" ] && say "   PASS: no seat-number literal carries a claim in any of the three files, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (b) EVERY UNANCHORED SUBSTRING MATCH ON A TOOL'S OUTPUT IS ON A WRITTEN WHITELIST =="
say "   ⚠ THE HAZARD, MEASURED RATHER THAN ASSERTED: the behavioral runner prints NOT MEASURED both as"
say "     a per-project ROW and inside its own COUNT lines (BehavioralRunner/Program.cs lines 549, 858"
say "     and 1171 build them), so a substring match counts a tally that reads ZERO as a hit.  A"
say "     verdict read out of a report is an ANCHORED ROW MATCH; what is LEFT unanchored is left"
say "     DELIBERATELY, each with a reason, and this arm refuses a NEW one."
say "   ⚠ THE DETECTOR IS NARROW ON PURPOSE: it fires only where a grep with -c or -q reads one of the"
say "     named TOOL-OUTPUT sigils and its pattern does not begin with ^.  Widening it to every grep in"
say "     the file would bury the finding in matches over the repository's own source."
TOOLSIGILS='"$L5" "$L5ENUM" "$L2B" "$L2" "$L0" "$LR2" "$LR" "$LD" "$L" "$G1LOG" "$G3LOG" "$lg" "$ASM" "$PIDCEN" "$PIDST"'
unanchored_sites(){ # $1 = file -> "<line>\t<text>" per in-scope site
  awk -v sig="$TOOLSIGILS" '
    { l=$0
      s=l; sub(/^[ \t]+/, "", s)
      if (substr(s,1,1) == "#") next
      if (l !~ /grep -a?[a-zA-Z]*[cq]/) next
      n=split(sig, S, " "); hit=0
      for (i=1;i<=n;i++) if (index(l, S[i])) hit=1
      if (!hit) next
      p=l
      if (!sub(/^.*grep -[a-zA-Z]+[ \t]+(--[ \t]+)?/, "", p)) next
      q=substr(p,1,1)
      if (q != "\047" && q != "\"") next
      if (substr(p,2,1) == "^") next
      printf "%d\t%s\n", NR, l
    }' "$1"
}
# --- THE WHITELIST.  ⚠ ONE LINE PER SITE, EACH JUSTIFIED, AND THE COUNT IS ASSERTED BOTH WAYS: a NEW
#     site is a refusal, and a REMOVED one is a refusal too, because a whitelist that outlives its
#     site is an exemption nobody re-reads.
WL_ASM="G1PASS_ANY=
G3CLEAN_ANY=
G3BAD_ANY=
CS0576=\$(grep -ac 'CS0576' \"\$L0\"
REBUILT=\$(grep -ac 'Building go2cs.exe
CS=\$(grep -acE 'error CS[0-9]+' \"\$L2\")
CS=\$(grep -acE 'error CS[0-9]+' \"\$L2B\")
RCS=\$(grep -acE 'error CS[0-9]+' \"\$LR2\"
RSTALE=\$(grep -acE 'test manifest is
orph=\$(grep -acF -- 'ORPHANED DISCLOSURE'
named=0; [ -n \"\${LEGU_NAME:-}\" ]
LEG 3 golib-\$cfg BUILD exit=\$brc
ABORT=\$(grep -ac 'Test Run Aborted'
nr=\$(grep -ac \"\$arm\" \"\$LR\"
S_NM=\$(grep -acEi 'NOT MEASURED'
S_CS0576=\$(grep -ac 'CS0576'
S_REBUILT=\$(grep -ac 'Building go2cs.exe
S_SKIP=\$(grep -acEi 'platform-exclusive"
#   why each is LEFT (in the order above):
#     1-3  the three CONVERTED sites keep their unanchored count as a deliberate SECOND READING beside
#          the anchored one, and a disagreement between the two is STAMPED as a finding.
#     4-5  'CS0576' is a compiler error CODE and 'Building go2cs.exe (converter sources changed)' is a
#          whole emitted line; both are READINGS, stamped and never asserted.
#     6-9  MSBuild diagnostics are printed INDENTED behind a `file(line,col): ` prefix, so a `^` anchor
#          would read ZERO and turn a RED build GREEN -- strictly worse than the substring.
#    10-11 the orphan-disclosure tool's own row spelling is not measurable from any artifact in this
#          directory, and an anchor that does not match what the producer prints refuses a healthy run.
#    12    an MSTest 'Test Run Aborted' line's column is set by the runner, not by this script.
#    13    the arm-name mentions are a READING: zeros are normal on a passing MSTest run.
#    14    ⚠ THE MEASURED DUAL-FORM SITE.  The runner prints NOT MEASURED in count lines as well as in
#          rows; it is LEFT because its only consumer is a STAMP -- it is never asserted -- and the
#          dual form is recorded here so the next derive meets it rather than discovering it.
#    15-17 readings, stamped and never asserted.
WL_LAND="PRERES=\$(grep -ac 'PRE-RESOLVED MERGE COMMITTED'
WX_N=\$(grep -acE -- \"\$WRAPEXIT_PAT\"
WX_ANY=\$(grep -ac 'assembly exit='
grep -aq \"union BLOB == that seat's BLOB\""
#     the record's stamps are '[<ts>] <text>', so a ^ anchor would have to carry the timestamp shape
#     and would then assert the stamp FORMAT rather than the verdict;
#     WX_ANY is DELIBERATELY the unanchored SECOND reading of the anchored WX_N and the gate requires
#     the two to AGREE -- converting it would collapse two readings into one.
#     \u26a0 THE REFUSAL-SCAN AND EXIT-SCAN LINES ARE **NOT** ON THIS LIST AND THAT IS A MEASUREMENT, NOT AN
#     OMISSION: the detector reads the LAST grep on a line, and on both of those the last grep is
#     `grep -ac .` -- a pattern, not a phrase -- so they are out of scope by construction rather than
#     by exemption.  Recorded here so the next derive meets the detector's rule instead of rediscovering it.
#     ⚠ THE FOURTH ENTRY IS **NEW IN TRAIN 48** AND IT ARRIVED BY INHERITANCE.  Train 47's land grew an
#     A7-ADMISSION READER on 2026-09-13 (run 8 refused a landing whose seven modified goldens its own
#     assembly had admitted), and that reader's second grep is an unanchored phrase over the RECORD.  It is
#     LEFT unanchored for the same reason as the two above -- the record's stamps carry a '[<ts>] ' prefix,
#     so a ^ anchor would assert the stamp FORMAT -- and it is on the list rather than exempted silently,
#     which is the whole point of a whitelist whose LENGTH is asserted against the site count.
WL_DRY="grep -aqF -- \"\$w\" \"\$ASM\" || bad=
if grep -aqF -- \"\$bad\" \"\$ASM\"; then say"
#     both read the ASSEMBLY SCRIPT's own text rather than a tool's verdict, and the second is arm A's
#     own NEGATIVE CONTROL -- a control that anchored its lookup would stop being one.
BBAD=0
wl_check(){ # $1 = file  $2 = the whitelist
  local f="$1" wl="$2" n=0 miss=0 want line txt
  [ -f "$f" ] || { say "   FAIL: [$f] is absent"; BBAD=1; return; }
  want=$(printf '%s\n' "$wl" | grep -ac .)
  while IFS="$(printf '\t')" read -r line txt; do
    [ -n "${line:-}" ] || continue
    n=$(( n + 1 ))
    if [ "$(printf '%s\n' "$wl" | while IFS= read -r w; do [ -n "$w" ] || continue; case "$txt" in *"$w"*) printf 'x' ;; esac; done | grep -ac x || true)" = "0" ]; then
      say "   FAIL NEW unanchored site :: $(basename "$f"):$line :: $(printf '%s' "$txt" | sed 's/^[[:space:]]*//' | cut -c1-150)"
      miss=$(( miss + 1 ))
    fi
  done <<< "$(unanchored_sites "$f")"
  say "   $(basename "$f") :: in-scope unanchored sites=$n :: whitelist entries=$want :: NOT on the whitelist=$miss"
  { [ "$miss" = "0" ] && [ "$n" = "$want" ]; } || BBAD=1
  [ "$n" = "$want" ] || say "   FAIL: the site count ($n) and the whitelist length ($want) disagree -- a site was added or removed and the whitelist was not re-read"
}
wl_check "$F" "$WL_ASM"
wl_check "$SELFDIR/coord-train48-land.sh" "$WL_LAND"
wl_check "$SELFDIR/coord-train48-land-dryread.sh" "$WL_DRY"
# the RED control: a planted line of exactly the forbidden shape must be SEEN by the detector.
PLANTB="/tmp/${TAG}-unanchored-control.sh"
printf '%s\n' 'PLANTED=$(grep -ac "0 DUPLICATE patch-id(s)" "$L5" || true)' > "$PLANTB"
c=$(unanchored_sites "$PLANTB" | grep -ac . || true)
if [ "${c:-0}" -ge 1 ]; then say "   ok   CONTROL :: the detector FIRES on a planted unanchored verdict match over a tool log ($c)"
else say "   FAIL CONTROL: the detector did not fire on a planted site, so every zero above says nothing"; BBAD=1; fi
rm -f "$PLANTB"
[ "$BBAD" = "0" ] && say "   PASS: every in-scope unanchored site is on the written whitelist, the counts agree, and the detector was SHOWN to fire" || FAIL=1

say ""
say "== (c) the SEAT TABLE parser: stack-on= is ACCEPTED and the three bad shapes REFUSE =="
say "   ⚠ A VALIDATOR THAT IS ONLY EVER RUN ON THE REAL TABLE IS A VALIDATOR WHOSE REFUSALS HAVE NEVER"
say "     BEEN SEEN TO FIRE.  The functions below are EXTRACTED FROM THE ASSEMBLY and run over planted"
say "     tables, so what is exercised is the live definition rather than a retyped copy."
stamp(){ say "        $*"; }
eval "$(blk '^SEAT_OPT_KEYS=' '^SEAT_OPT_KEYS=' 'SEAT_OPT_KEYS')" || exit 3
eval "$(blk '^seat_opt\(\)' '^\}$' 'seat_opt')" || exit 3
eval "$(blk '^seat_row\(\)' '^seat_row\(\)' 'seat_row')" || exit 3
eval "$(blk '^seat_opts_validate\(\)' '^\}$' 'seat_opts_validate')" || exit 3
eval "$(blk '^seat_class_placeholders\(\)' '^\}$' 'seat_class_placeholders')" || exit 3
CBAD=0
armc(){ # $1 label  $2 planted table  $3 wanted rc  $4 a fragment the refusal must carry ('' = none)
  local out rc
  out=$(SEAT_TABLE="$2"; seat_opts_validate 2>&1); rc=$?
  if [ "$rc" != "$3" ]; then
    say "   FAIL $1 :: the validator exited $rc where $3 was wanted"; printf '%s\n' "$out" | sed 's/^/        /'; CBAD=1; return
  fi
  if [ -n "$4" ]; then
    case "$out" in
      *"$4"*) say "   ok   $1 :: refused (exit $rc) AND named the reason [$4]" ;;
      *) say "   FAIL $1 :: refused with exit $rc but did NOT name the reason [$4] -- a refusal nobody can read is a refusal nobody acts on"; printf '%s\n' "$out" | sed 's/^/        /'; CBAD=1 ;;
    esac
  else
    say "   ok   $1 :: ACCEPTED (exit $rc)"
  fi
}
armc 'stack-on naming an EARLIER row' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=1" 0 ''
armc 'stack-on naming a MISSING row' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=9" 1 'exists in this table'
armc 'stack-on naming a LATER row' "1|claude/a|111111111|docs|tip|stack-on=2
2|claude/b|222222222|docs|tip" 1 'merges AFTER it'
armc 'stack-on naming ITSELF' "1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip|stack-on=2" 1 'stacked on ITSELF'
armc 'an UNKNOWN option key' "1|claude/a|111111111|docs|tip|stacked-on=1" 1 'unknown option key'
armc 'an optional field that is not key=value' "1|claude/a|111111111|docs|tip|[a-z]+\.cs\$" 1 'not <key>=<value>'
# and the allowed= reader, over a row that carries BOTH keys in EITHER order.
for tbl in "3|claude/c|333333333|docs|tip|allowed=^x/y\.cs\$|stack-on=1" "3|claude/c|333333333|docs|tip|stack-on=1|allowed=^x/y\.cs\$"; do
  SEAT_TABLE="1|claude/a|111111111|docs|tip
2|claude/b|222222222|docs|tip
$tbl"
  got=$(seat_opt "$(seat_row 3)" allowed)
  sot=$(seat_opt "$(seat_row 3)" stack-on)
  if [ "$got" = '^x/y\.cs$' ] && [ "$sot" = "1" ]; then say "   ok   both keys read ORDER-FREE from row 3 :: allowed=[$got] stack-on=[$sot]"
  else say "   FAIL: the optional keys are read POSITIONALLY :: allowed=[$got] (want ^x/y\\.cs\$) stack-on=[$sot] (want 1)"; CBAD=1; fi
done
# and a FILLED row carrying the class placeholder must be REPORTED by the reader the assembly consumes.
SEAT_TABLE="1|claude/a|111111111|PENDING-class|tip
2|claude/b|PENDING|PENDING-class|tip"
pc=$(seat_class_placeholders | tr '\n' ' ')
if [ "$(printf '%s' "$pc" | tr -d ' ')" = "1" ]; then say "   ok   the class placeholder is reported for the FILLED row only :: [$pc]"
else say "   FAIL: seat_class_placeholders read [$pc] where only the FILLED row should be reported"; CBAD=1; fi
unset -f stamp
[ "$CBAD" = "0" ] && say "   PASS: stack-on= is accepted, the three bad shapes and two malformed fields REFUSE BY NAME, and both keys are order-free" || FAIL=1

say ""
say "== (d) the PATCH-ID ARM's four branches are present in the assembly, each by its STAMP =="
say "   ⚠ THE BRANCH THAT MATTERS MOST IS THE ONE THAT CANNOT RUN: an arm whose tool is missing must"
say "     set FAILED, because an instrument that could not run has not found nothing."
say "   ⚠ THE PREDICATE IS A **FUNCTION OVER A FILE**, so the arm and its control are the same code."
say "     Its first cut read \$F directly and had no in-run control at all: its reds were shown once,"
say "     by hand, against a planted copy that is not reproducible from the shipped instrument.  An arm"
say "     whose control lives in a note is an assertion wearing a measurement's clothes."
d_defects(){ # $1 = file -> one "ok<TAB>..." or "FAIL<TAB>..." line per branch and per anchored token
  local f="$1" pair a b ca cb tok c
  for pair in \
    'PATCH-ID ARM UNMEASURED ::|fail_gate PATCH-ID-ARM-UNMEASURED' \
    'PATCH-ID ARM self-test exit=|fail_gate PATCH-ID-ARM-SELFTEST' \
    'PATCH-ID ARM CLEAN ::|==> CENSUS (CLEAN|RED)' \
    'PATCH-ID ARM REFUSED :: exit=1|fail_gate PATCH-ID-ARM' ; do
    a="${pair%%|*}"; b="${pair#*|}"
    ca=$(grep -acF -- "$a" "$f" || true); cb=$(grep -acF -- "$b" "$f" || true)
    if [ "${ca:-0}" -ge 1 ] && [ "${cb:-0}" -ge 1 ]; then printf 'ok\t[%s] x%s with [%s] x%s\n' "$a" "$ca" "$b" "$cb"
    else printf 'FAIL\tthe patch-id branch [%s] (x%s) or its consequence [%s] (x%s) is absent\n' "$a" "${ca:-0}" "$b" "${cb:-0}"; fi
  done
  for tok in '^DUPLICATE patch-id ' '^UNDECLARED STACK: ' '^SELF-TEST CLEAN -- ' '^declared-stack SHA ' 'seat-duplication-census.sh' '--self-test' '--stack'; do
    c=$(grep -acF -- "$tok" "$f" || true)
    if [ "${c:-0}" -ge 1 ]; then printf 'ok\tthe arm reads [%s] x%s\n' "$tok" "$c"
    else printf 'FAIL\t[%s] is absent -- the arm would read its tool'"'"'s verdict as a SUBSTRING, or would not run the tool'"'"'s own controls\n' "$tok"; fi
  done
}
DBAD=0
while IFS="$(printf '\t')" read -r v m; do
  [ -n "${v:-}" ] || continue
  if [ "$v" = "ok" ]; then say "   ok   $m"; else say "   FAIL: $m"; DBAD=1; fi
done <<< "$(d_defects "$F")"
# --- THE RED CONTROL, IN-RUN.  A COPY of the assembly with the CLEAN stamp renamed out of it must be
#     SEEN, or every ok above says only that this arm can read a file.
PLANTD="/tmp/${TAG}-patchid-control.sh"
sed 's/PATCH-ID ARM CLEAN ::/PATCH-ID ARM DELETED-BY-PLANT ::/' "$F" > "$PLANTD"
DCTL=$(d_defects "$PLANTD" | grep -ac '^FAIL' || true)
if [ "${DCTL:-0}" -ge 1 ]; then
  say "   ok   CONTROL :: with the CLEAN stamp renamed out of a COPY of the assembly the predicate reports ${DCTL} absence(s) -- the arm CAN fire"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$(d_defects "$PLANTD" | grep -a '^FAIL' | head -2 | sed 's/^FAIL\t//' | cut -c1-150)"
else say "   FAIL CONTROL: the predicate found nothing wrong with a COPY whose CLEAN stamp was renamed away, so its zeros say nothing"; DBAD=1; fi
rm -f "$PLANTD"
[ "$DBAD" = "0" ] && say "   PASS: UNMEASURED->FAILED, the self-test gate, the CLEAN stamp and the named refusal are all present, every verdict read is anchored, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (e) the LAUNCH WRAPPER exports TRAIN48_REQUIRE_ALL and ends in 'exit \$rc' =="
say "   ⚠ A WRAPPER WHOSE LAST STATEMENT IS A PIPE REPORTS THE LAST COMMAND'S STATUS, so a script that"
say "     exited 1 is announced as 0.  And the switch must be EXPORTED rather than one-shot prefixed: a"
say "     nohup/start layer can drop a one-shot prefix between the assignment and the child."
LW="$SELFDIR/launch-run1.sh"
EBAD=0
# ⚠ THE PREDICATE IS A FUNCTION OVER A FILE, for the same reason (d)'s is: its reds were shown once by
#   hand against a planted wrapper and were not reproducible from the shipped instrument.
e_defects(){ # $1 = file -> a READING line, then one "FAIL<TAB>..." per defect
  local w="$1" exp last rcc
  exp=$(grep -acE '^[[:space:]]*export TRAIN48_REQUIRE_ALL=1[[:space:]]*$' "$w" || true)
  last=$(grep -av '^[[:space:]]*$' "$w" | tail -1 | tr -d '\r')
  rcc=$(grep -acE '; rc=\$\?' "$w" || true)
  printf 'READ\texport TRAIN48_REQUIRE_ALL=1 lines=%s (want 1) :: rc=$? capture lines=%s (want 1) :: last non-empty line=[%s]\n' "${exp:-0}" "${rcc:-0}" "$last"
  [ "${exp:-0}" = "1" ] || printf 'FAIL\tthe switch is not EXPORTED on its own line\n'
  [ "${rcc:-0}" -ge 1 ] || printf 'FAIL\tthe wrapper does not capture rc=$? as the first statement after the run\n'
  case "$last" in
    'exit $rc') printf 'ok\tthe wrapper ends in '"'"'exit $rc'"'"'\n' ;;
    *) printf 'FAIL\tthe wrapper'"'"'s last statement is [%s], not '"'"'exit $rc'"'"'\n' "$last" ;;
  esac
}
if [ ! -f "$LW" ]; then say "   FAIL: [$LW] is absent -- the launch shape is asserted on a file that does not exist"; EBAD=1
else
  while IFS="$(printf '\t')" read -r v m; do
    [ -n "${v:-}" ] || continue
    case "$v" in
      READ) say "   $m" ;;
      ok)   say "   ok   $m" ;;
      *)    say "   FAIL: $m"; EBAD=1 ;;
    esac
  done <<< "$(e_defects "$LW")"
  # --- THE RED CONTROL, IN-RUN: a planted wrapper that one-shot-prefixes the switch and ends in a tail.
  PLANTE="/tmp/${TAG}-launch-control.sh"
  { printf '%s\n' 'TRAIN48_REQUIRE_ALL=1 bash coord-train48-assemble-run1.sh > out.stdout 2>&1'
    printf '%s\n' 'tail -40 out.stdout'; } > "$PLANTE"
  ECTL=$(e_defects "$PLANTE" | grep -ac '^FAIL' || true)
  if [ "${ECTL:-0}" -ge 3 ]; then
    say "   ok   CONTROL :: a planted wrapper that one-shot-prefixes the switch and ends in a tail reads ${ECTL} defect(s) -- the arm CAN fire on all three"
    while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$(e_defects "$PLANTE" | grep -a '^FAIL' | sed 's/^FAIL\t//' | cut -c1-150)"
  else say "   FAIL CONTROL: the predicate reads ${ECTL:-0} defect(s) on a wrapper carrying all three, so its greens above say nothing"; EBAD=1; fi
  rm -f "$PLANTE"
fi
[ "$EBAD" = "0" ] && say "   PASS: the launch wrapper exports the switch and exits on the captured status, and the predicate was SHOWN to fire" || FAIL=1

say ""
say "== (f) NO 'train47' TOKEN SURVIVES OUTSIDE A PROVENANCE COMMENT =="   # [t47-control]
say "   ⚠ A PROVENANCE COMMENT IS A COMMENT LINE THAT NAMES THE TEMPLATE FILE.  Keeping those is the"
say "     point: a derive that scrubbed its own parentage would delete the record of what this file was"
say "     derived FROM, which is the one fact a re-derive needs."
t47_live(){ # $1 = file -> lines carrying `train47` that are NOT provenance comments  [t47-control]
  awk '{ l=$0
         if (index(l, "train47") == 0) next   # [t47-control]
         s=l; sub(/^[ \t]+/, "", s)
         if (substr(s,1,1) == "#" && index(l, "coord-train47") > 0) next   # a provenance comment names the template  [t47-control]
         if (index(l, "[t47-control]") > 0) next                              # an explicitly MARKED control-wiring line
         printf "%d\t%s\n", NR, l }' "$1"
}
FBAD=0
for f in "$F" "$SELFDIR/coord-train48-rehearse.sh" "$SELFDIR/coord-train48-land.sh" "$SELFDIR/coord-train48-land-dryread.sh" "$SELFDIR/launch-run1.sh" "${BASH_SOURCE[0]}"; do
  [ -f "$f" ] || { say "   FAIL: [$f] is absent"; FBAD=1; continue; }
  n=$(t47_live "$f" | grep -ac . || true)
  prov=$(grep -acF -- 'coord-train47' "$f" || true)   # [t47-control]
  if [ "${n:-0}" = "0" ]; then say "   ok   $(basename "$f") :: live train47 tokens=0 (provenance mentions=$prov)"   # [t47-control]
  else say "   FAIL $(basename "$f") :: $n live train47 token(s):"; t47_live "$f" | head -6 | cut -c1-170 | sed 's/^/        /'; FBAD=1; fi   # [t47-control]
done
if [ -f "$T47ASM" ]; then
  c=$(t47_live "$T47ASM" | grep -ac . || true)
  if [ "${c:-0}" -ge 1 ]; then say "   ok   CONTROL (train-47 assembly) :: the same predicate reads $c live token(s) -- the arm CAN fire"
  else say "   FAIL CONTROL: the predicate reads 0 on the train-47 original, so its zeros above say nothing"; FBAD=1; fi
else
  say "   FAIL CONTROL UNMEASURED :: [$T47ASM] is absent (override with T47DIR=<dir>)"; FBAD=1
fi
[ "$FBAD" = "0" ] && say "   PASS: no live train47 token survives, provenance comments are kept, and the predicate was SHOWN to fire" || FAIL=1   # [t47-control]

say ""
say "== (g) THE FOUR FILL POINTS REFUSE AT LAUNCH, each BY NAME =="
say "   ⚠ 'A GATE REFUSED' AND 'A GATE COULD NOT BE CONFIGURED' ARE DIFFERENT STATES, so each refusal"
say "     names its own fill point.  F2 and F4 refuse BEFORE the run lock is taken -- a placeholder met"
say "     three hundred lines and one lock into the run has already cost the operator what a refusal is for."
say "   ⚠ THE PREDICATE IS A FUNCTION OVER A FILE, AND IT HAS **TWO INDEPENDENT BRANCHES** -- the"
say "     refusal-and-reason presence check and the refusal-BEFORE-the-lock ORDER check.  Its first cut"
say "     read \$F directly and had no control at all, which is the one state this whole arm exists to"
say "     forbid: an arm that has never been made to fire is an assertion wearing a measurement's clothes."
GBAD=0
CP=$(grep -aE "^T48_CONTAIN_PIN='" "$F" | head -1 | sed -E "s/^T48_CONTAIN_PIN='([^']*)'.*/\1/")
G3E=$(grep -aE "^EXPECT_G3='" "$F" | head -1 | sed -E "s/^EXPECT_G3='([^']*)'.*/\1/")
say "   T48_CONTAIN_PIN=[${CP:-ABSENT}] (F2) :: EXPECT_G3=[${G3E:-ABSENT}] (F4) :: WT fill point and SEAT_TABLE are F1 and F3"
g_defects(){ # $1 = file -> "ok/READ/FAIL<TAB>..." per fill point, then the ORDER branch
  local f="$1" pair a b ca cb fpo lk fp
  for pair in 'FILL POINT F2 REFUSED|the containment pin is not filled' 'FILL POINT F4 REFUSED|census expectation is not filled' 'WORKTREE UNFILLED ::|name the assembly worktree' ; do
    a="${pair%%|*}"; b="${pair#*|}"
    ca=$(grep -acF -- "$a" "$f" || true); cb=$(grep -acF -- "$b" "$f" || true)
    if [ "${ca:-0}" -ge 1 ] && [ "${cb:-0}" -ge 1 ]; then printf 'ok\t[%s] x%s names its reason [%s] x%s\n' "$a" "$ca" "$b" "$cb"
    else printf 'FAIL\tthe refusal [%s] (x%s) or its reason [%s] (x%s) is absent\n' "$a" "${ca:-0}" "$b" "${cb:-0}"; fi
  done
  fpo=$(grep -acF -- 'FILL POINTS OK ::' "$f" || true)
  lk=$(grep -nF -- 'LOCK="/tmp/' "$f" | head -1 | cut -d: -f1)
  fp=$(grep -nF -- 'FILL POINTS UNFILLED' "$f" | head -1 | cut -d: -f1)
  printf 'READ\tthe positive stamp '"'"'FILL POINTS OK ::'"'"' x%s (want 1) :: the fill-point refusal is at line %s, the run lock at line %s (the refusal must come FIRST)\n' "${fpo:-0}" "${fp:-none}" "${lk:-none}"
  { [ "${fpo:-0}" = "1" ] && [ -n "${fp:-}" ] && [ -n "${lk:-}" ] && [ "$fp" -lt "$lk" ]; } || printf 'FAIL\tthe fill points are not both stamped and refused BEFORE the run lock is taken\n'
}
while IFS="$(printf '\t')" read -r v m; do
  [ -n "${v:-}" ] || continue
  case "$v" in
    READ) say "   $m" ;;
    ok)   say "   ok   $m" ;;
    *)    say "   FAIL: $m"; GBAD=1 ;;
  esac
done <<< "$(g_defects "$F")"
# --- THE RED CONTROLS, IN-RUN, ONE PER BRANCH.  ⚠ TWO PLANTS, BECAUSE TWO BRANCHES: a single plant
#     that reddened both would leave either one able to hide behind the other.
PLANTG1="/tmp/${TAG}-fillpoint-reason-control.sh"
sed 's/the containment pin is not filled/the containment pin is REDACTED-BY-PLANT/' "$F" > "$PLANTG1"
G1CTL=$(g_defects "$PLANTG1" | grep -ac '^FAIL' || true)
PLANTG2="/tmp/${TAG}-fillpoint-order-control.sh"
{ printf '%s\n' 'LOCK="/tmp/planted-early.lock"'; cat "$F"; } > "$PLANTG2"
G2CTL=$(g_defects "$PLANTG2" | grep -ac '^FAIL' || true)
if [ "${G1CTL:-0}" -ge 1 ] && [ "${G2CTL:-0}" -ge 1 ]; then
  say "   ok   CONTROL (reason branch) :: a COPY with F2's reason text redacted reads ${G1CTL} defect(s) -- that branch CAN fire"
  say "   ok   CONTROL (order branch)  :: a COPY with the run lock moved to line 1 reads ${G2CTL} defect(s) -- that branch CAN fire"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $l"; done <<< "$( { g_defects "$PLANTG1"; g_defects "$PLANTG2"; } | grep -a '^FAIL' | sed 's/^FAIL\t//' | cut -c1-150)"
else say "   FAIL CONTROL: the reason branch reads ${G1CTL:-0} and the order branch ${G2CTL:-0} on their own planted copies, so the greens above say nothing"; GBAD=1; fi
rm -f "$PLANTG1" "$PLANTG2"
[ "$GBAD" = "0" ] && say "   PASS: every fill point refuses by name, before the lock, and BOTH branches were SHOWN to fire" || FAIL=1

say ""
say "== (e) merge_seat HONOURS THE PER-ROW allowed= RULING -- STATIC, WITH A REGRESSED COPY AS ITS RED CONTROL =="
say "   COORD ruling 2026-09-13 19:05 (D5).  Train 47's merge_seat consulted the CLASS alone: the"
say "   forbidden arm and the shape arm never read allowed=, and only A7 did -- scoped to"
say "   src/tests/Behavioral.  Four rows of this table rule paths OUTSIDE that scope, and every one of"
say "   them would have ABORTED at assembly on its own class while the ruling at the row read as prose."
alw_arm(){ # $1 = a script -> "forbiddenArmFilters shapeArmFilters identityBlocks a7bArms"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'diff --name-only "$mb" "$want" -- $forbidden | alw_filter "$alw"' "$1" || true)" \
    "$(grep -acF -- 'diff --name-only "$mb" "$want" | alw_filter "$alw"' "$1" || true)" \
    "$(grep -acF -- 'ALLOWED-IDENTITY-AFTER-MERGE' "$1" || true)" \
    "$(grep -acF -- 'fail_gate A7b-allowed-identity' "$1" || true)"
}
PLANTH="/tmp/${TAG}-allowed-filter-control.sh"
# ⚠ THE CONTROL IS A COPY WITH THE FILTER **SED'd OUT**, i.e. exactly train 47's shape.  A control
#   built by any other edit would be reddening something else and calling it this arm.
sed 's/ | alw_filter "$alw"//g' "$F" > "$PLANTH"
read -r AF AS AI AB <<EOF
$(alw_arm "$F")
EOF
read -r RF RS RI RB <<EOF
$(alw_arm "$PLANTH")
EOF
say "   derived   :: forbiddenArmFilters=$AF (want >= 2) shapeArmFilters=$AS (want >= 2) identityBlocks=$AI (want >= 1) a7bArms=$AB (want >= 1)"
say "   REGRESSED :: forbiddenArmFilters=$RF shapeArmFilters=$RS identityBlocks=$RI a7bArms=$RB"
if [ "${AF:-0}" -ge 2 ] && [ "${AS:-0}" -ge 2 ] && [ "${AI:-0}" -ge 1 ] && [ "${AB:-0}" -ge 1 ]; then
  say "   PASS: BOTH merge_seat readings filter through the seat's own allowed= ruling, the post-merge BLOB-IDENTITY block is present, and A7b is wired beside A7"
else
  say "   FAIL: merge_seat does NOT honour allowed= (forbiddenArmFilters=$AF want>=2, shapeArmFilters=$AS want>=2, identityBlocks=$AI want>=1, a7bArms=$AB want>=1).  A row whose ruling names a path outside its class would ABORT at assembly and the ruling would be decoration."
  FAIL=1
fi
if [ "${RF:-1}" = "0" ] && [ "${RS:-1}" = "0" ]; then
  say "   ok   CONTROL (regressed copy) :: with ' | alw_filter \"\$alw\"' sed'd out the SAME predicate reads forbiddenArmFilters=0 shapeArmFilters=0 and NAMES the regression -- a gate never made to fail proves nothing"
else
  say "   FAIL CONTROL: the regressed copy still reads forbiddenArmFilters=$RF shapeArmFilters=$RS, so the green above says nothing about the filter it claims to read"
  FAIL=1
fi
rm -f "$PLANTH"

say ""
# ⚠⚠ **SEAT_TABLE IS RE-EXTRACTED FROM THE FILE HERE, AND THAT IS A DEFECT THESE ARMS FOUND IN
#   THEMSELVES ON THEIR FIRST RUN.**  The arms above PLANT synthetic tables into this shell to
#   exercise the validators' refusals, and the last plant LEAVES the global SEAT_TABLE holding TWO
#   synthetic rows.  The first cut of the arms below read that global and reported `rows carrying an
#   allowed= ruling=0 :: STALE ruled paths=0` -- a PASS taken over a table nobody seated, which is the
#   vacuous-green shape exactly: every count zero, nothing refused, and the arm reads healthy.  The
#   table is re-read FROM THE FILE, which is the only authority, and the row count is STAMPED so a
#   reader can see WHICH table these arms measured rather than assuming.
eval "$(blk '^SEAT_TABLE="1\|' '"$' 'SEAT_TABLE (re-read for the allowed= arms)')" || exit 3
ALWTBLROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
say "   the seat table was RE-READ from the file for arms (f) and (g) :: rows=$ALWTBLROWS -- the planted controls above leave the global holding SYNTHETIC rows"
# ⚠ AND THE RE-READ IS ITSELF CHECKED, against arm 2's INDEPENDENT reading of the same table.  A
#   re-read that silently returned the plant again would restore exactly the vacuous green this block
#   exists to remove, and nothing else in the run would say so.
if [ "${ALWTBLROWS:-0}" = "${ROWS:-0}" ] && [ "${ALWTBLROWS:-0}" -ge 1 ]; then
  say "   ok   the re-read table has $ALWTBLROWS rows, matching arm 2's independent reading of $ROWS -- arms (f) and (g) measure the SEATED table"
else
  say "   FAIL: the re-read table has ${ALWTBLROWS:-0} rows against arm 2's reading of ${ROWS:-0} -- arms (f) and (g) would be measuring a table nobody seated, and every count they print would be vacuous"
  FAIL=1
fi
say "== (f) EVERY allowed= RULING NAMES A PATH THE ROW ACTUALLY TOUCHES -- ONLINE, AGAINST THE REAL DIFF =="
say "   A ruled path the seat does not change exempts NOTHING and hides that it exempts nothing.  The"
say "   assembly refuses it as a STALE RULING at seat time; this arm reads it BEFORE the worktree exists."
say "   ⚠⚠ AND IT USES THE **ASSEMBLER'S OWN BASE RULE** (COORD E2, 2026-09-13 ~20:00).  merge_seat"
say "   measures a seat against merge-base(HEAD, want); by the time a row declaring stack-on=N merges,"
say "   HEAD ALREADY CARRIES ROW N, so the seat's measured diff is its OWN delta.  Reading a stacked row"
say "   against the TRAIN'S BASE instead credits it with every file its PARENT changed -- which is"
say "   precisely how one inherited ruling read LIVE in this arm and STALE at assembly."
# ⚠ THE PREDICATE IS A FUNCTION OVER A TABLE **GIVEN TO IT**, not a loop over the global, for one
#   reason: a predicate that can only read the live table can never be shown to FAIL, and safety-floor
#   item 13 says such a gate proves nothing.  The controls below hand it synthetic tables built from
#   REAL SHAs of this very train, so the red control reddens the arm ITSELF and not a copy of it.
stale_ruled(){ # $1 = a seat table.  stdout: tab-tagged ROW/PATH/ok/FAIL/UNM lines.  rc=1 if any STALE.
  local T="$1" sbad=0 n b s c m rest aw sob sobsha mbref mbwhy mbf files p
  while IFS='|' read -r n b s c m rest; do
    [ -n "$n" ] || continue
    [ "$s" = "PENDING" ] && continue
    aw=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')
    [ -n "$aw" ] || continue
    printf 'ROW\t%s\n' "$n"
    if ! git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null; then
      printf 'UNM\trow %s (%s) :: the pin %s is not in this clone -- UNMEASURED, and an arm that could not run has not found nothing\n' "$n" "$b" "$s"; continue
    fi
    sob=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    mbref="$BASE_EXPECT"; mbwhy="the train's base"
    if [ -n "$sob" ]; then
      sobsha=$(printf '%s\n' "$T" | awk -F'|' -v r="$sob" '$1==r{print $3}')
      if [ -n "$sobsha" ] && [ "$sobsha" != "PENDING" ] && git -C "$G" cat-file -e "${sobsha}^{commit}" 2>/dev/null; then
        mbref="$sobsha"; mbwhy="row $sob's PINNED SHA $sobsha (DECLARED stack-on=$sob)"
      else
        printf 'UNM\trow %s (%s) :: declares stack-on=%s and row %s pins [%s], which does not resolve here -- the STACKED BASE is UNMEASURABLE and no reading is taken.  Falling back to the train base would credit this row with its PARENT files, which is the over-read this rule removes\n' "$n" "$b" "$sob" "$sob" "${sobsha:-ABSENT}"; continue
      fi
    fi
    mbf=$(git -C "$G" merge-base "$mbref" "$s" 2>/dev/null)
    files=$(git -C "$G" -c core.quotepath=false diff --name-only "$mbf" "$s")
    for p in $(printf '%s' "$aw" | sed 's/^\^//; s/\$$//; s/[()]//g' | tr '?' '\n' | sed 's/\\//g' | grep -a .); do
      printf 'PATH\t%s\n' "$p"
      if [ "$(printf '%s\n' "$files" | grep -acxF -- "$p" || true)" = "0" ]; then
        printf 'FAIL\trow %s (%s @%s) rules allowed= [%s] and its REAL diff against %s does NOT touch it -- a STALE RULING\n' "$n" "$b" "$s" "$p" "$mbwhy"; sbad=1
      else
        printf 'ok\trow %s (%s) allowed= [%s] IS in the real diff measured against %s\n' "$n" "$b" "$p" "$mbwhy"
      fi
    done
  done <<< "$T"
  return $sbad
}
SRO=$(stale_ruled "$SEAT_TABLE"); SRRC=$?
ALWROWS=$(printf '%s\n' "$SRO" | grep -ac '^ROW	' || true)
ALWPATHS=$(printf '%s\n' "$SRO" | grep -ac '^PATH	' || true)
ALWSTALE=$(printf '%s\n' "$SRO" | grep -ac '^FAIL	' || true)
ALWUNM=$(printf '%s\n' "$SRO" | grep -ac '^UNM	' || true)
while IFS= read -r l; do
  case "$l" in
    ROW*|PATH*|'') continue ;;
    FAIL*) say "   FAIL: $(printf '%s' "$l" | sed 's/^FAIL\t//')" ;;
    UNM*)  say "   FAIL (UNMEASURED): $(printf '%s' "$l" | sed 's/^UNM\t//')" ;;
    *)     say "   ok   $(printf '%s' "$l" | sed 's/^ok\t//')" ;;
  esac
done <<< "$SRO"
say "   arm (f) :: rows carrying an allowed= ruling=$ALWROWS :: ruled paths read=$ALWPATHS :: STALE ruled paths=$ALWSTALE (must be 0) :: rows UNMEASURED=$ALWUNM"
if [ "$SRRC" = "0" ] && [ "${ALWUNM:-0}" = "0" ]; then
  say "   PASS: every ruled path is in its row's real diff, and every stacked row was measured against THE BASE merge_seat WILL USE"
else
  say "   FAIL: a ruling is STALE or a row could not be read -- $ALWSTALE stale, $ALWUNM unmeasured"
  FAIL=1
fi

# ⚠ **THE CONTROLS FOR THE BASE RULE, BUILT FROM REAL SHAs OF THIS TRAIN** (COORD E2).  A synthetic
#   pair is DERIVED from the first row of this table that declares stack-on= and whose parent pin
#   resolves: the child, its parent, a path only the PARENT touches (the RED path) and a path in the
#   child's OWN delta (the GREEN path).  No row number and no SHA is typed here -- a control written
#   against a literal row is a control that rots the next time the table is re-seated.
CTLC=''; CTLP=''; CTLB=''; CTLPB=''; CTLRED=''; CTLGREEN=''; CTLPAIR=''
while IFS='|' read -r n b s c m rest; do
  [ -n "$n" ] || continue
  [ "$s" = "PENDING" ] && continue
  cso=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
  [ -n "$cso" ] || continue
  cpb=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$cso" '$1==r{print $2}')
  cps=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v r="$cso" '$1==r{print $3}')
  [ -n "$cps" ] && [ "$cps" != "PENDING" ] || continue
  git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null || continue
  git -C "$G" cat-file -e "${cps}^{commit}" 2>/dev/null || continue
  own=$(git -C "$G" -c core.quotepath=false diff --name-only "$(git -C "$G" merge-base "$cps" "$s")" "$s")
  par=$(git -C "$G" -c core.quotepath=false diff --name-only "$(git -C "$G" merge-base "$BASE_EXPECT" "$cps")" "$cps")
  red=$(printf '%s\n' "$par" | while IFS= read -r pp; do [ -n "$pp" ] || continue; printf '%s\n' "$own" | grep -aqxF -- "$pp" || { printf '%s\n' "$pp"; break; }; done | head -1)
  green=$(printf '%s\n' "$own" | grep -a . | head -1)
  [ -n "$red" ] && [ -n "$green" ] || continue
  CTLC="$s"; CTLP="$cps"; CTLB="$b"; CTLPB="$cpb"; CTLRED="$red"; CTLGREEN="$green"; CTLPAIR="row $n stacked on row $cso"
  break
done <<< "$SEAT_TABLE"
if [ -z "$CTLC" ]; then
  say "   FAIL (UNMEASURED): no stacked pair in this table could supply BOTH a parent-only path and a child path, so the base rule has NOT been made to fail and its green above says nothing"
  FAIL=1
else
  # ⚠ ONLY `.` IS ESCAPED FOR THE ERE.  These are repository paths -- alphanumerics, `/`, `.`, `-`,
  #   `_` -- and `|` can never appear in one because the table itself is `awk -F'|'`.
  ere_of(){ printf '%s' "$1" | sed 's/\./\\./g'; }
  say "   the control pair is DERIVED :: $CTLPAIR :: child $CTLB @$CTLC on parent $CTLPB @$CTLP"
  say "   RED path (the PARENT touches it, the child's own delta does NOT) :: [$CTLRED]"
  say "   GREEN path (in the child's OWN delta) :: [$CTLGREEN]"
  CTBL_R="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLRED")\$|stack-on=1"
  CTBL_G="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLGREEN")\$|stack-on=1"
  CTBL_N="1|$CTLPB|$CTLP|docs|tip
2|$CTLB|$CTLC|docs|tip|allowed=^$(ere_of "$CTLRED")\$"
  CR_OUT=$(stale_ruled "$CTBL_R"); CR_RC=$?
  CG_OUT=$(stale_ruled "$CTBL_G"); CG_RC=$?
  CN_OUT=$(stale_ruled "$CTBL_N"); CN_RC=$?
  if [ "$CR_RC" != "0" ] && [ "$(printf '%s\n' "$CR_OUT" | grep -acF -- "$CTLRED" || true)" != "0" ]; then
    say "   ok   CONTROL RED :: a stacked row ruling [$CTLRED] -- a path only its BASE touches -- reads STALE and the path is NAMED:"
    printf '%s\n' "$CR_OUT" | grep -a '^FAIL	' | while IFS= read -r l; do say "        CONTROL red: $(printf '%s' "$l" | sed 's/^FAIL\t//' | cut -c1-170)"; done
  else
    say "   FAIL CONTROL RED: the stacked row ruling a parent-only path read rc=$CR_RC and did not name [$CTLRED] -- the stale-ruling predicate never looked, and arm (f)'s green says nothing"
    FAIL=1
  fi
  if [ "$CG_RC" = "0" ] && [ "$(printf '%s\n' "$CG_OUT" | grep -ac '^ok	' || true)" != "0" ]; then
    say "   ok   CONTROL GREEN :: the SAME stacked row ruling [$CTLGREEN] -- a path in its OWN delta -- PASSES, so the red above is a reading and not a predicate that refuses everything"
  else
    say "   FAIL CONTROL GREEN: a ruling naming a path in the child's own delta read rc=$CG_RC with no ok line -- the predicate refuses even a LIVE ruling, which would make every red above meaningless"
    FAIL=1
  fi
  # ⚠ THE THIRD READING IS WHAT MAKES THE FIRST TWO ABOUT **THE BASE RULE** RATHER THAN ABOUT THE
  #   PATHS.  Strip `stack-on=` from the red table and nothing else: the row is then measured against
  #   the train's base, the parent's commits are inside that diff, and the SAME ruling PASSES.  If
  #   this reading also redded, the red control would be evidence about the path and not about E2.
  if [ "$CN_RC" = "0" ]; then
    say "   ok   DISCRIMINATOR :: the SAME table with stack-on= REMOVED passes on [$CTLRED] -- so the red control is produced BY the assembler base rule and by nothing else about that path"
  else
    say "   FAIL DISCRIMINATOR: the red ruling reds even WITHOUT stack-on= (rc=$CN_RC), so the control is not evidence about the base rule -- it would red under train 47's shape too"
    FAIL=1
  fi
fi

say ""
say "== (g) TWO ROWS RULING THE SAME PATH NEED A DECLARED STACK -- TABLE ARM, WITH A PLANTED RED CONTROL =="
say "   A7b's owner rule is 'the LAST row in merge order'.  That is a DECISION and it is only defensible"
say "   when the two rows are declared a stack; otherwise two seats rule one path with no stated order"
say "   and the union's blob would be compared against whichever row the loop happened to read last."
dupruled(){ # $1 = a seat table -> prints FAIL lines, returns 1 if any
  local T="$1" bad=0 p rws last so
  ALLP=$(while IFS='|' read -r n b s c m rest; do
    [ -n "$n" ] || continue
    aw=$(printf '%s\n' "$T" | awk -F'|' -v r="$n" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^allowed=/) {print substr($i,9); exit}}')
    [ -n "$aw" ] || continue
    printf '%s' "$aw" | sed 's/^\^//; s/\$$//; s/[()]//g' | tr '?' '\n' | sed 's/\\//g' | grep -a . | sed "s|^|$n |"
  done <<< "$T")
  for p in $(printf '%s\n' "$ALLP" | awk 'NF==2{print $2}' | sort | uniq -d); do
    rws=$(printf '%s\n' "$ALLP" | awk -v q="$p" '$2==q{print $1}' | sort -n | tr '\n' ' ')
    last=$(printf '%s\n' "$ALLP" | awk -v q="$p" '$2==q{print $1}' | sort -n | tail -1)
    so=$(printf '%s\n' "$T" | awk -F'|' -v r="$last" '$1==r{for(i=6;i<=NF;i++) if ($i ~ /^stack-on=/) {print substr($i,10); exit}}')
    case " $rws" in
      *" ${so:-<none>} "*) printf 'ok\t[%s] is ruled by rows [%s] and the LAST of them (row %s) DECLARES stack-on=%s -- the owner is unambiguous and A7b compares the union against THAT row\n' "$p" "$rws" "$last" "$so" ;;
      *) printf 'FAIL\t[%s] is ruled by rows [%s] with NO declared stack among them (row %s declares stack-on=[%s]) -- two seats rule one path and nothing says which owns it\n' "$p" "$rws" "$last" "${so:-none}"; bad=1 ;;
    esac
  done
  return $bad
}
DUPOUT=$(dupruled "$SEAT_TABLE"); DUPRC=$?
while IFS= read -r l; do [ -n "$l" ] && say "   $(printf '%s' "$l" | sed 's/\t/ /')"; done <<< "$DUPOUT"
if [ "$DUPRC" = "0" ]; then
  say "   PASS: no path is ruled by two rows without a declared stack"
else
  say "   FAIL: a path is ruled by two rows with no declared stack -- the A7b owner rule has nothing to stand on"
  FAIL=1
fi
# ⚠ THE PLANTED RED CONTROL.  The same predicate over a table whose two rows rule one path with NO
#   stack-on must REFUSE and must NAME the path.  Without it the PASS above is indistinguishable from
#   a predicate that never looked.
DUPPLANT="1|claude/a|111111111|docs|tip|allowed=^(docs/x\.md)?(docs/y\.md)?$
2|claude/b|222222222|docs|tip|allowed=^(docs/x\.md)?(docs/z\.md)?$"
DUPCTL=$(dupruled "$DUPPLANT"); DUPCTLRC=$?
if [ "$DUPCTLRC" != "0" ] && [ "$(printf '%s\n' "$DUPCTL" | grep -acF 'docs/x.md' || true)" != "0" ]; then
  say "   ok   CONTROL (planted) :: two rows ruling docs/x.md with no stack-on is REFUSED and the path is NAMED:"
  while IFS= read -r l; do [ -n "$l" ] && say "        CONTROL red: $(printf '%s' "$l" | sed 's/\t/ /' | cut -c1-150)"; done <<< "$DUPCTL"
else
  say "   FAIL CONTROL: the planted overlap read rc=$DUPCTLRC and did not name the path, so the green above says nothing"
  FAIL=1
fi

say ""
say ""
say "== (h) F1a/F1b :: G10d's eol=lf PIN EXEMPTION AND THE PRE-RESOLVED APPLY'S RE-MATERIALISATION, EACH WITH A REGRESSED COPY AS ITS RED CONTROL =="
say "   COORD rulings 2026-09-13, both MEASURED by run 2 on a healthy tree: DATA-sweep-row-walltimes.md"
say "   was refused on a CORRECT text eol=lf pin (attr=[set] i/lf w/lf, CR=0 LF=395), and"
say "   BOARD-next-validation-candidates.md (attr unspecified, i/lf w/lf, CR=0 LF=24779) was refused"
say "   because the PRE-RESOLVED apply copied an LF union file into the worktree and added it, leaving"
say "   a worktree form this box's autocrlf would never have produced."
f1_arm(){ # $1 = a script -> "eolAttrReads pinBranches pinCountStamped rematerialisations"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'eolv=$(git check-attr eol --' "$1" || true)" \
    "$(grep -acF -- 'if [ "$eolv" = "lf" ]; then' "$1" || true)" \
    "$(grep -acF -- 'eolPinned=$G10DEOL' "$1" || true)" \
    "$(grep -acF -- 'git add -- "$up" && rm -f -- "$up" && git checkout -- "$up"' "$1" || true)"
}
F1CTL="/tmp/${TAG}-f1-regressed.sh"
# ⚠ THE CONTROL IS A COPY REGRESSED TO **TRAIN 47'S EXACT SHAPE**: the pin branch's guard neutered so
#   it can never be taken, and the re-materialising checkout sed'd out of the apply.  A control built
#   by any other edit would be reddening something else and calling it these arms.
sed -e 's/if \[ "\$eolv" = "lf" \]; then/if [ "$eolv" = "lf-NEVER" ]; then/' -e 's/ && rm -f -- "\$up" && git checkout -- "\$up"//' "$F" > "$F1CTL"
read -r EA EB ES ER <<EOF
$(f1_arm "$F")
EOF
read -r RA RB RS RR <<EOF
$(f1_arm "$F1CTL")
EOF
say "   derived   :: eolAttrReads=$EA (want 1) pinBranches=$EB (want 1) pinCountStamped=$ES (want 1) rematerialisations=$ER (want 1)"
say "   REGRESSED :: eolAttrReads=$RA pinBranches=$RB pinCountStamped=$RS rematerialisations=$RR"
if [ "${EA:-0}" -ge 1 ] && [ "${EB:-0}" -ge 1 ] && [ "${ES:-0}" -ge 1 ] && [ "${ER:-0}" -ge 1 ]; then
  say "   PASS: G10d reads the eol attribute, exempts an eol=lf pin whose two layers AGREE, stamps that count SEPARATELY from textExempt, and the PRE-RESOLVED apply re-materialises every resolved path through git"
else
  say "   FAIL: a half is missing (eolAttrReads=$EA pinBranches=$EB pinCountStamped=$ES rematerialisations=$ER, each want >= 1) -- a correct pin would be refused, or the apply's own LF artefact would be read as a seat's defect"
  FAIL=1
fi
if [ "${RB:-1}" = "0" ] && [ "${RR:-1}" = "0" ]; then
  say "   ok   CONTROL (regressed copy) :: with the pin branch neutered and the checkout sed'd out -- train 47's exact shape -- the SAME predicate reads pinBranches=0 rematerialisations=0, so the green above can go red"
else
  say "   FAIL CONTROL: the regressed copy still reads pinBranches=$RB rematerialisations=$RR, so the green above says nothing about the two sites it claims to read"
  FAIL=1
fi
rm -f "$F1CTL"

say ""
say "== (i) F2 :: G11(a)'s golibtests OWE IS DELTA-DERIVED -- THE ASSEMBLY'S OWN g11a_arm OVER A SYNTHETIC OWED VECTOR, WITH THE CLASS-DERIVED OWE AS ITS RED CONTROL =="
say "   COORD ruling 2026-09-13.  Run 2 read JUSTIFICATION FALSE :: src/tests/GolibTests=0 where the"
say "   OWED vector says a merged class owes it -- on a tree whose golib-corpus-handown rows touch"
say "   src/core/runtime and src/go2cs and nothing under src/tests/GolibTests.  A class says what a"
say "   seat MAY touch; the arm refuses on what a seat DID touch, and those are different questions."
f2_arm(){ # $1 = a script -> "deltaDerivations classSetters stampNamesDelta golibStillClassDerived"
  printf '%s %s %s %s' \
    "$(grep -acF -- 'OWED_GT_FILES=$(git -c core.quotepath=false diff --name-only "$BASE" HEAD -- src/tests/GolibTests' "$1" || true)" \
    "$(grep -acF -- 'OWED_GOLIB=1; OWED_GT=1' "$1" || true)" \
    "$(grep -acF -- 'golibtests=$OWED_GT (from the DELTA: $OWED_GT_FILES file(s))' "$1" || true)" \
    "$(grep -acF -- 'OWED_GOLIB=1 ;; esac' "$1" || true)"
}
read -r D1 D2 D3 D4 <<EOF
$(f2_arm "$F")
EOF
say "   deltaDerivations=$D1 (want 1) :: class-derived OWED_GT setters=$D2 (want 0) :: the stamp NAMES the delta=$D3 (want 1) :: OWED_GOLIB still class-derived=$D4 (want 1)"
if [ "${D1:-0}" = "1" ] && [ "${D2:-1}" = "0" ] && [ "${D3:-0}" = "1" ] && [ "${D4:-0}" = "1" ]; then
  say "   PASS: golibtests is measured from the merged DELTA and stamped as such, and OWED_GOLIB is left class-derived so LEG 3 still RUNS for the golib family"
else
  say "   FAIL: the golibtests owe is still a class's potential (deltaDerivations=$D1 classSetters=$D2 stampNamesDelta=$D3 golibClassDerived=$D4)"
  FAIL=1
fi
# the BEHAVIOURAL half: the assembly's OWN justification arm, run over a SYNTHETIC owed vector.
eval "$(blk '^g11a_arm\(\)' '^\}$' 'g11a_arm (the justification arm itself)')" || exit 3
G11ABAD=0
g11a_arm 0 0 'src/tests/GolibTests' 'SYNTHETIC: a golib-corpus-handown class whose merged delta carries NO GolibTests file'
SYNTH="$G11ABAD"
G11ABAD=0
g11a_arm 1 0 'src/tests/GolibTests' 'SYNTHETIC REGRESSION: the class-derived owe, which is run 2 exactly'
REGR="$G11ABAD"
say "   the arm over a DELTA-derived owe (owed=0 measured=0) sets G11ABAD=$SYNTH (want 0) :: over the CLASS-derived owe (owed=1 measured=0) it sets G11ABAD=$REGR (want 1)"
if [ "$SYNTH" = "0" ] && [ "$REGR" = "1" ]; then
  say "   ok   CONTROL :: the SAME function passes the synthetic vector and REFUSES the regressed one -- the arm keeps its teeth and simply cannot go FALSE on a class's potential any more"
else
  say "   FAIL CONTROL: synthetic=$SYNTH regressed=$REGR -- either the arm no longer refuses at all (its teeth are gone) or it still refuses a delta-derived zero"
  FAIL=1
fi

[ "${OFFLINE_UNMEASURED:-0}" = "0" ] || say "⚠ OFFLINE :: $OFFLINE_UNMEASURED pin(s) could not be resolved because T48_SELFCHECK_OFFLINE=1 forbade the fetch.  They are counted as FAILURES above and they are UNMEASURED, not findings: re-run without the switch, outside any freeze, to turn them into measurements."
say "=== SELF-CHECK DONE overallFail=$FAIL offlineUnmeasured=${OFFLINE_UNMEASURED:-0} ==="
exit $FAIL
