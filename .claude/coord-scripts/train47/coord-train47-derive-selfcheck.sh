#!/bin/bash
# DERIVE-TIME SELF-CHECK for coord-train47-assemble.sh (throwaway, read-only).
#
# It EXTRACTS blocks from the assembly script BY GREP ANCHOR and runs them, so what is exercised is
# the assembly script's OWN definitions rather than a retyped copy -- a retyped copy is the thing that
# drifts.  It NEVER runs the assembly script: no cd into the assembly worktree, no fetch, no merge,
# no leg, no build.
#
#   usage:  bash coord-train47-derive-selfcheck.sh [<path to coord-train47-assemble.sh>]
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
#   1  the BASE: EXPECT_46 is declared, the script ASSERTS it, and the assertion REFUSES a PENDING
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
#      read ZERO on the train-47 files AND NON-ZERO on the train-46 ones, because an arm that has
#      never been made to fire is an assertion rather than a measurement.  A control that cannot RUN
#      (the train-46 originals absent) is reported UNMEASURED and FAILS: an instrument that could not
#      run has not found nothing.
set -u
F="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/coord-train47-assemble.sh}"
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
TAG=t47selfcheck
FAIL=0
say(){ printf '%s\n' "$*"; }
[ -f "$F" ] || { say "   ABORT: $F does not exist"; exit 3; }
# the BASE this checker measures against is READ FROM THE SCRIPT, never retyped beside it.
BASE_EXPECT=$(grep -aE "^EXPECT_46='" "$F" | head -1 | sed -E "s/^EXPECT_46='([^']*)'.*/\1/")
[ -n "$BASE_EXPECT" ] || BASE_EXPECT=UNREADABLE

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
say "== 1. the BASE: EXPECT_46 is declared, ASSERTED, and a PENDING base REFUSES =="
E47=$(grep -aE "^EXPECT_46='" "$F" | head -1 | sed -E "s/^EXPECT_46='([^']*)'.*/\1/")
E46=$(grep -aE "^EXPECT_45='" "$F" | head -1 | sed -E "s/^EXPECT_45='([^']*)'.*/\1/")
say "   the script declares EXPECT_46=[$E47] (this train's BASE) and EXPECT_45=[$E46] (the ORDER pin)"
BA=$(grep -ac '^stamp "BASE ASSERTION ::' "$F")
BR=$(grep -ac 'BASE REFUSED: this script declares' "$F")
BP=$(grep -ac 'BASE REFUSED: EXPECT_46 reads' "$F")
say "   the script's OWN base assertion :: 'BASE ASSERTION ::' stamps=$BA (want 1) :: wrong-base refusal=$BR (want 1) :: PENDING-base refusal=$BP (want 1)"
{ [ "$BA" = "1" ] && [ "$BR" = "1" ] && [ "$BP" = "1" ]; } && say "   PASS: the script ASSERTS its base, REFUSES a different one, and REFUSES an UNFILLED one" || { say "   FAIL: the base is derived but not asserted in all three directions -- HEAD == origin/master says the tree is AT master, not that master is the master this was derived against, and a PENDING base that RUNS is a battery measuring a tree nobody named"; FAIL=1; }
ECON=$(grep -aE "^EXPECT_CONTAINS='" "$F" | head -1 | sed -E "s/^EXPECT_CONTAINS='([^']*)'.*/\1/")
case "$E47" in
  PENDING|'')
    say "   ⚠ EXPECT_46 reads PENDING, which is the EXPECTED state of a TEMPLATE.  It is FILL POINT F2."
    say "     Arms 3 and 8 are therefore UNMEASURABLE against real seat diffs and say so; they are not passes."
    ;;
  origin/master)
    # ⚠ THIS TRAIN DECLARES ITS BASE AS A **REVSPEC**, and the arm changes shape accordingly rather
    #   than refusing a spelling it was not written for.  What is asserted here is what the coordinator
    #   RULED: the base is origin/master resolved at launch, and it CONTAINS the named pin.  A literal
    #   SHA is refused as a base BY THIS ARM, because a baked base refuses a healthy launch the moment
    #   any lane lands and the operator then reaches for the switch that makes it stop refusing.
    say "   the script declares its base as the REVSPEC [origin/master] plus the CONTAINMENT pin EXPECT_CONTAINS=[${ECON:-ABSENT}]"
    B1OK=1
    [ -n "${ECON:-}" ] || { say "   FAIL: base=origin/master with NO EXPECT_CONTAINS pin -- the revspec alone asserts nothing about the base's CONTENT, which is the whole reason a revspec is admissible here"; B1OK=0; }
    ANC=$(grep -ac 'EXPECT_CONTAINS:the CONTAINMENT pin' "$F" || true)
    say "   the containment pin is CONSUMED by the base-ancestry loop=$ANC (want 1 -- a pin declared and never asserted is a decoration)"
    [ "${ANC:-0}" = "1" ] || B1OK=0
    if [ -d "$G/.git" ] && [ -n "${ECON:-}" ]; then
      git -C "$G" fetch -q origin master 2>/dev/null
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
      git -C "$G" fetch -q origin master 2>/dev/null
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
eval "$(blk '^SEAT_TABLE="1\|' '\|tip"$' 'SEAT_TABLE')" || exit 3
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
NUMS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $1}')
EXPN=$(awk -v n="$ROWS" 'BEGIN{for(i=1;i<=n;i++) print i}')
DUPREF=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $2}' | sort | uniq -d | grep -ac . || true)
DUPSHA=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$3!="PENDING"{print $3}' | sort | uniq -d | grep -ac . || true)
SHORTR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' 'NF<5' | grep -ac . || true)
ALLOWR=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '$6 ~ /^allowed=/' | grep -ac . || true)
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
say "     EXPECTED reading; the assembly SKIPS them with a stamp by default, and TRAIN47_REQUIRE_ALL=1"
say "     is what makes them refuse -- the shape a LANDING run of the full table takes."
say "   malformed SHAs=$BADSHA (want 0)"
[ "$BADSHA" = "0" ] && say "   PASS: every non-PENDING SHA is nine hex digits" || FAIL=1
CLSOK=1
for c in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $4}' | sort -u); do
  # ⚠ A **FIXED-STRING** LOOKUP, NEVER AN ERE.  A class name may carry a regex metacharacter
  #   (`converter-test+docs` does), and an ERE built from it silently matches nothing -- which would
  #   report a correctly implemented class as missing.  The glyph-substring family, one layer over.
  if [ "$(grep -acF -- "    ${c})" "$F" || true)" != "0" ]; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi
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
  case "$b" in claude/PENDING-*) [ "$s" = "PENDING" ] || { say "   FAIL: row $n has a FILLED SHA ($s) against the PLACEHOLDER ref [$b]"; PHBAD=1; } ;; esac
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
    git -C "$G" fetch -q origin "+refs/heads/$b:refs/remotes/origin/$b" 2>/dev/null
    if ! git -C "$G" cat-file -e "${s}^{commit}" 2>/dev/null; then
      say "   FAIL: seat $n's SHA $s ($b) does not resolve even after the fetch -- an unresolved SHA and an INVENTED one read the same"
      FAIL=1; continue
    fi
    tip=$(git -C "$G" rev-parse --short=9 "origin/$b" 2>/dev/null)
    if [ "$tip" = "$s" ]; then
      say "   seat $n ($c) $b :: tip == pin $s  (tip mode satisfied)"
    elif git -C "$G" merge-base --is-ancestor "$s" "origin/$b" 2>/dev/null; then
      ahead=$(git -C "$G" rev-list --count "${s}..origin/$b")
      say "   seat $n ($c) $b :: ⚠ THE BRANCH GREW -- tip is $tip, $ahead commit(s) BEYOND the pin $s, which IS an ancestor.  merge_seat will ABORT with this diagnosis, which is the mechanism WORKING.  The added commit(s):"
      git -C "$G" log --oneline "${s}..origin/$b" | sed 's/^/       /'
      say "     ⚠ THE PIN IS **NOT** MOVED HERE.  A SHA typed into the table is the one place a seat's pin lives; re-pinning is the coordinator's, and this arm's job is to make the growth impossible to miss rather than to absorb it."
    else
      say "   FAIL: seat $n's pin $s is NOT an ancestor of the tip $tip -- the branch was REWRITTEN, or the pin names a different branch.  Ask the owner before re-pinning."
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
WTLINE=$(grep -n '^WT="\${TRAIN47_WT:-' "$F" | head -1)
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
  SH_BAD=0
  while IFS='|' read -r n b s c m a; do
    [ -n "$n" ] || continue
    [ "$s" = "PENDING" ] && { say "   seat $n ($c) $b :: PENDING -- shape NOT checkable, and EXPECTED on this derive"; continue; }
    if ! git -C "$G" cat-file -e "origin/$b^{commit}" 2>/dev/null; then
      say "   seat $n ($c) $b :: the remote ref is not present locally -- fetch it before believing this arm; UNMEASURED"
      SH_BAD=1; continue
    fi
    mb=$(git -C "$G" merge-base "$BASE_EXPECT" "$s" 2>/dev/null)
    pat=$(shape_of "$c"); frb=$(forb_of "$c")
    [ -n "$pat" ] || { say "   seat $n :: could not read the shape pattern for class '$c' out of the script"; SH_BAD=1; continue; }
    files=$(git -C "$G" -c core.quotepath=false diff --name-only "$mb" "$s")
    nfiles=$(printf '%s\n' "$files" | grep -c .)
    # A7 CONSISTENCY (2026-09-13 18:25): a row's own allowed= ruling admits exactly those paths at assembly time (A7 asserts
    # blob identity); this pre-flight arm judged the class WITHOUT it and refused train 48's seat 1 for the one path its ruling
    # names.  The ruling's paths are subtracted here and COUNTED beside the reading, never hidden.
    alw=''; case "${a:-}" in allowed=*) alw="${a#allowed=}";; esac
    if [ -n "$alw" ]; then files_judged=$(printf '%s\n' "$files" | grep -avE "$alw" || true); nalw=$(printf '%s\n' "$files" | grep -acE "$alw" || true); else files_judged="$files"; nalw=0; fi
    out=$(printf '%s\n' "$files_judged" | grep -avE "$pat" | grep -ac . || true)
    # shellcheck disable=SC2086
    bad=$(git -C "$G" -c core.quotepath=false diff --name-only "$mb" "$s" -- $frb | { if [ -n "$alw" ]; then grep -avE "$alw"; else cat; fi; } | grep -ac . || true)
    say "   seat $n ($c) $b @$s :: files=$nfiles outsideShape=$out forbiddenHits=$bad allowedByRuling=$nalw"
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
  [ "$SH_BAD" = "0" ] && say "   PASS: every FILLED seat's REAL diff is admitted by its own class shape and touches none of its forbidden paths" || { say "   FAIL: a seat would be REFUSED by its own class -- and this is a RULING THE COORDINATOR OWNS, not a shape to widen here."; FAIL=1; }
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
NEX=$(grep -c 'is NOT \\`-text\\`-exempt has a non-CRLF working-tree form' "$F")
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
say "     the train-47 file AND NON-ZERO on the train-46 one: an arm that has never been made to fire"
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
#   MEASURED 2026-09-13: train47 assemble/land/dry-read/rehearse/self-check = 0; train46 land = 1 and
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

lesson(){ # $1 label  $2 predicate  $3 the train-47 file  $4 the train-46 original  $5 what it measures
  local lab="$1" fn="$2" new="$3" old="$4" what="$5" n o
  n=$("$fn" "$new"); n="${n:-0}"
  if [ ! -f "$old" ]; then
    say "   $lab :: train47=$n (want 0) :: ⚠ **CONTROL UNMEASURED** -- [$old] is not present, so the arm has not been shown able to fire.  $what"
    FAIL=1; return
  fi
  o=$("$fn" "$old"); o="${o:-0}"
  if [ "$n" = "0" ] && [ "${o:-0}" -ge 1 ]; then
    say "   PASS $lab :: train47=$n train46=$o -- the arm reads ZERO on the derived file and FIRES on the original.  $what"
  elif [ "$n" != "0" ]; then
    say "   FAIL $lab :: train47=$n (want 0) train46=$o -- the defect SURVIVED the derive.  $what"
    FAIL=1
  else
    say "   FAIL $lab (CONTROL) :: train47=$n train46=$o (want >= 1) -- the arm did NOT fire on the file that carries the defect, so its zero on the derived file says nothing.  $what"
    FAIL=1
  fi
}

# ⚠ THE CONTROL FILENAMES ARE **COMPOSED**, NOT SPELLED.  The derive that writes this file runs a
#   global `coord-train46-* -> coord-train47-*` rename over it, and a spelled name here would be
#   rewritten to point the control at the file it is supposed to be controlling AGAINST -- which is
#   exactly what happened on this arm's first run: L3 and L11 read their own output as the original
#   and reported the control as dead.  An instrument built out of the thing under test cannot
#   independently measure it.
T46NUM=46
T46ASM="$T46DIR/coord-train${T46NUM}-assemble.sh"
T46LAND="$T46DIR/coord-train${T46NUM}-land.sh"
LAND47="$SELFDIR/coord-train47-land.sh"
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
say "   reading  L4 (assembly) :: train47=$(l4_count "$F") train46=$(l4_count "$T46ASM") -- BOTH zero is EXPECTED here: the train-46 site was corrected in flight at run 3, so this predicate's control lives on the two files below"
[ "$(l4_count "$F")" = "0" ] || { say "   FAIL L4 (assembly) :: the case-default shape is present in the derived assembly"; FAIL=1; }
lesson 'LC1 the land read-back compares full SHAs' lc1_count "$LAND47" "$T46LAND" \
  "Train 47 run 8 LANDED (origin/master == the announced 40-char SHA) and the land declared NOT LANDED, exit 5, because it compared a 9-char ls-remote cut against a 10-char --short HEAD; the prune loop and the LAND DONE stamp never ran.  A short-SHA comparison is a comparison of two different quantities."
lesson 'LB1 the land honours A7 admissions read from the record' lb1_count "$LAND47" "$T46LAND" \
  "Train 47 run 8's record carried seven behavioral files admitted by seat 8's allowed= ruling (A7 stamped each with a blob identity) and the land refused the landing on an UNCONDITIONAL modified-count gate that contradicted its own LANDING NOTES (5).  The gate now reads the admissions from the record and fails only on an unadmitted file."
lesson 'L4 no case-default (land)' l4_count "$LAND47" "$T46LAND" \
  "The shape survived in the LAND script, where nothing in train 46 looked for it.  ${VAR:-0} substitutes before the '' arm can be reached, so the empty case is unreachable and the value is never normalised -- which is how G10a printed 'deletions=' EMPTY and refused."
lesson 'L4 no case-default (dry-read)' l4_count "$SELFDIR/coord-train47-land-dryread.sh" "$T46DIR/coord-train${T46NUM}-land-dryread.sh" \
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
for f in "$F" "$SELFDIR/coord-train47-rehearse.sh" "$LAND47" "$SELFDIR/coord-train47-land-dryread.sh" "${BASH_SOURCE[0]}"; do
  [ -f "$f" ] || { say "   FAIL: [$f] is not present, so its header could not be read"; FAIL=1; continue; }
  n=$(grep -ac 'NOT.*pipefail\|not.*pipefail' "$f" || true)
  p=$(grep -ac -- '| grep -q' "$f" || true)
  if [ "${n:-0}" -ge 1 ]; then say "   ok   $(basename "$f") :: states the set -u/pipefail rationale ($n line(s)) :: '| grep -q' sites=$p"
  else say "   FAIL $(basename "$f") :: does NOT state why it is \`set -u\` and not pipefail"; FAIL=1; fi
done

say ""
say "== 13. THE FILL POINTS AND THIS TRAIN'S OWN RULED CHANGES, each asserted rather than assumed =="
say "   ⚠ ARM 12 ABOVE ASKS 'did the TEMPLATE's known defects survive the derive'.  THIS ARM ASKS THE"
say "     OTHER HALF: 'did what the coordinator RULED for train 47 actually land in the file'.  A fill"
say "     point that is merely no longer a placeholder is not a fill point that was filled correctly."
A13=0
a13(){ # $1 ok(0/1)  $2 what
  if [ "$1" = "1" ]; then say "   ok   $2"; else say "   FAIL: $2"; A13=1; fi
}
LANDF="$SELFDIR/coord-train47-land.sh"
# --- F1/F2/F3, each read as a FILLED value and not merely as a non-placeholder.
WTVAL=$(grep -aE '^WT="\$\{TRAIN47_WT:-' "$F" | head -1 | sed -E 's/^WT="\$\{TRAIN47_WT:-(.*)\}"$/\1/')
a13 "$( case "$WTVAL" in ''|*PENDING*) echo 0 ;; *) echo 1 ;; esac )" "F1 the assembly worktree is FILLED :: [$WTVAL]"
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
a13 "$( [ "${ARMN:-0}" -ge "${FILLEDN:-0}" ] && echo 1 || echo 0 )" "every FILLED row has a content assertion"
# --- carried defect (a): the un-prefixed switch is REFUSED, and the documented launcher EXPORTS the
#     prefixed one.  A launcher exporting REQUIRE_ALL would configure nothing and read as a landing.
a13 "$( [ "$(grep -ac 'REQUIRE_ALL is in the ENVIRONMENT reading' "$F")" = "1" ] && echo 1 || echo 0 )" "defect (a) the inherited un-prefixed REQUIRE_ALL is REFUSED BY NAME before the script's own assignment overwrites it"
a13 "$( [ "$(grep -ac 'export TRAIN47_REQUIRE_ALL=1' "$F")" -ge 1 ] && echo 1 || echo 0 )" "defect (a) the documented launch wrapper EXPORTS the PREFIXED switch"
a13 "$( [ "$(grep -ac 'SKIP_PENDING is in the ENVIRONMENT reading' "$F")" = "1" ] && echo 1 || echo 0 )" "the un-prefixed SKIP_PENDING is refused too -- one switch fixed and its twin left open is the defect surviving the derive"
# --- ⚠ THE **INVERTED DEFAULT**: with a FILLED table a PENDING row REFUSES, and the skip is an opt-in.
SKD=$(codeonly "$F" | grep -ac 'TRAIN47_SKIP_PENDING' || true)
SKREF=$(grep -ac 'TRAIN47_SKIP_PENDING is not 1' "$F" || true)
SKCON=$(grep -ac 'are a CONTRADICTION' "$F" || true)
say "   the PENDING default :: TRAIN47_SKIP_PENDING read in CODE=$SKD (want >= 3: the un-prefixed guard, the declared/empty arm and the preflight) :: the preflight REFUSAL naming it=$SKREF (want 1) :: REQUIRE_ALL+SKIP_PENDING refused as a contradiction=$SKCON (want 1)"
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
for rq in 'LEG U ARM 1' 'LEG U ARM 2 MET' 'LEG U ARM 3 MET' 'prediction FILTERED per target' 'A-row8 generic cross-package alias qualifier' 'BASE ANCESTRY OK'; do
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
say "=== SELF-CHECK DONE overallFail=$FAIL ==="
exit $FAIL
