#!/bin/bash
# DERIVE-TIME SELF-CHECK for coord-train46-assemble.sh (throwaway, read-only).
#
# It EXTRACTS blocks from the assembly script BY GREP ANCHOR and runs them, so what is exercised is
# the assembly script's OWN definitions rather than a retyped copy -- a retyped copy is the thing that
# drifts.  It NEVER runs the assembly script: no cd into the assembly worktree, no fetch, no merge,
# no leg, no build.
#
#   usage:  bash coord-train46-derive-selfcheck.sh [<path to coord-train46-assemble.sh>]
#   exit 0  every arm passed
#   exit 1  an arm failed
#   exit 3  a BLOCK COULD NOT BE RESOLVED or does not parse -- the checker itself is broken, and that
#           is reported as its own state rather than as a finding about the script
#
# ⚠ WHAT THIS CHECKS, AND WHY EACH ARM EXISTS:
#   0  the script parses, carries no CR bytes and no BOM.
#   1  the BASE: EXPECT_45 is 44f858717, it IS origin/master right now (fetched, BOTH printed), and
#      the script ASSERTS its own base rather than merely deriving it.
#   2  the SEAT TABLE -- FIVE rows, every SHA nine hex digits or the literal PENDING, the PENDING rows
#      PRINTED BY NAME, the merge order, and every class known to merge_seat.
#   3  every FILLED seat SHA RESOLVES, and each PENDING row is NAMED rather than counted.  ⚠ Row 3's
#      branch had already grown past its pin at derive time; that is REPORTED with the ancestry
#      diagnosis rather than silently re-pinned, because a SHA typed into the table is the one place a
#      seat's pin lives and moving it is the coordinator's.
#   4  NO train-45 SHA survives outside a dated comment -- and the residual comment hits are PRINTED,
#      because "zero in code" is a different claim from "none anywhere" and only one of them is true.
#   5  the WORKTREE is NAMED and no other is accepted; SCRIPT_PATH is ABSOLUTE and precedes the cd.
#   6  G11(c)'s NINE launch lines present, WITH negative controls -- the arm must go RED on a copy
#      with one removed, or its green is an assertion wearing a measurement's clothes.
#   7  the LEG K oracleGoVersion predicate, run FROM THE SCRIPT'S OWN DEFINITION over six values.
#   8  the per-seat CLASS SHAPES against the REAL seat diffs and against decoys.
#   9  the PER-RUN-COPY discipline and the MID-BATTERY FREEZE announcement lines.
#  10  the G10d `-text` arm consults the ATTRIBUTE (this derive's own fix), with a decoy.
#  11  blk()'s own negative control: a broken anchor must be reported as the CHECKER being broken.
set -u
F="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/coord-train46-assemble.sh}"
TAG=t46selfcheck
BASE_EXPECT=44f858717
FAIL=0
say(){ printf '%s\n' "$*"; }
[ -f "$F" ] || { say "   ABORT: $F does not exist"; exit 3; }

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
say "== 1. the BASE: EXPECT_45 == $BASE_EXPECT, it IS origin/master, and the script ASSERTS it =="
E45=$(grep -E "^EXPECT_45='" "$F" | head -1 | sed -E "s/^EXPECT_45='([^']*)'.*/\1/")
say "   the script declares EXPECT_45=[$E45]"
[ "$E45" = "$BASE_EXPECT" ] && say "   PASS: the declared base is $BASE_EXPECT" || { say "   FAIL: the declared base is [$E45], wanted $BASE_EXPECT"; FAIL=1; }
if [ -d "$G/.git" ]; then
  git -C "$G" fetch -q origin master 2>/dev/null
  RM=$(git -C "$G" ls-remote origin refs/heads/master 2>/dev/null | cut -c1-9)
  OM=$(git -C "$G" rev-parse --short=9 origin/master 2>/dev/null)
  say "   origin/master :: ls-remote reads [$RM] :: the fetched ref reads [$OM] :: the script's base is [$E45]"
  say "   ⚠ BOTH ARE PRINTED ON PURPOSE.  A base declared and a base MEASURED are two claims, and this arm exists to compare them rather than to restate one of them."
  { [ "$RM" = "$OM" ] && [ "$RM" = "$BASE_EXPECT" ]; } && say "   PASS: origin/master IS $BASE_EXPECT and the two readings agree" || { say "   FAIL: origin/master reads [$RM]/[$OM] where this derive was made against $BASE_EXPECT -- the tree moved, so RE-DERIVE rather than adjust"; FAIL=1; }
else
  say "   SKIPPED: $G is not a git repository from here -- the base could not be MEASURED, only read from the script"
  FAIL=1
fi
BA=$(grep -c '^stamp "BASE ASSERTION ::' "$F")
BR=$(grep -c 'BASE REFUSED: this script was derived against' "$F")
say "   the script's OWN base assertion :: 'BASE ASSERTION ::' stamps=$BA (want 1) :: refusal branch=$BR (want 1)"
{ [ "$BA" = "1" ] && [ "$BR" = "1" ]; } && say "   PASS: the script ASSERTS its base and REFUSES a different one, rather than deriving BASE from HEAD and carrying on" || { say "   FAIL: the base is derived but not asserted -- HEAD == origin/master says the tree is AT master, not that master is the master this was derived against"; FAIL=1; }

# ---- 2. THE SEAT TABLE -------------------------------------------------------------------------
say ""
say "== 2. the seat table: SIX rows (filled at seat fill), nine-hex or PENDING, the merge order, the classes =="
eval "$(blk '^SEAT_TABLE="1\|' '\|tip"$' 'SEAT_TABLE')" || exit 3
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
say "   rows=$ROWS (want 6 -- SIX after the coordinator filled rows 4 and 5 and added row 6 at seat fill, 2026-09-08 15:40)"
[ "$ROWS" = "6" ] && say "   PASS: six rows" || { say "   FAIL: $ROWS rows"; FAIL=1; }
ORDER=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{printf "%s ", $1}')
say "   merge order (row order) :: $ORDER"
[ "$ORDER" = "1 2 3 4 5 6 " ] && say "   PASS: the merge order is the ruled one (seat fill 2026-09-08 15:40) -- the two converter+guard seats (1, 2), C1's golib/corpus stack (3), C2's instrument (4), G's convIdent cut (5), C1's docs (6)" || { say "   FAIL: the row order is not the ruled merge order"; FAIL=1; }
BADSHA=0; PEND=0; PENDNAMES=""
while IFS='|' read -r n b s c m; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then PEND=$(( PEND + 1 )); PENDNAMES="$PENDNAMES $n($b)"; continue; fi
  if printf '%s' "$s" | grep -qE '^[0-9a-f]{9}$'; then : ; else
    BADSHA=$(( BADSHA + 1 )); say "   FAIL: seat $n's SHA [$s] is neither nine hex digits nor PENDING"
  fi
done <<< "$SEAT_TABLE"
say "   PENDING rows=$PEND [$PENDNAMES]"
say "   ⚠ PRINTED BY NAME, not merely counted.  TWO placeholders are EXPECTED on this derive, and the"
say "     assembly SKIPS them with a stamp by default; TRAIN46_REQUIRE_ALL=1 is what makes them refuse,"
say "     which is the shape a LANDING run of the full five-seat train takes."
[ "$PEND" = "0" ] && say "   PASS: zero placeholders read PENDING (both were FILLED at seat fill; the 2 this derive was made with is history)" || { say "   NOTE: $PEND row(s) read PENDING where this derive was made with 2.  That is a READING and not a failure -- the coordinator fills them -- but a run with fewer is a DIFFERENT train from the one this script was derived for, and TRAIN46_REQUIRE_ALL=1 is how a landing asserts they are all filled."; }
say "   malformed SHAs=$BADSHA (want 0)"
[ "$BADSHA" = "0" ] && say "   PASS: every non-PENDING SHA is nine hex digits" || FAIL=1
CLSOK=1
for c in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $4}' | sort -u); do
  if grep -qE "^ *${c}\)" "$F"; then say "   class '$c' has a merge_seat arm"; else say "   FAIL: class '$c' has NO merge_seat arm"; CLSOK=0; fi
done
[ "$CLSOK" = "1" ] && say "   PASS: every class in the table is a class merge_seat implements" || FAIL=1
for m in $(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $5}' | sort -u); do
  case "$m" in tip|sha) say "   tip mode '$m' is known" ;; *) say "   FAIL: unknown tip mode '$m'"; FAIL=1 ;; esac
done
# the PLACEHOLDER REF discipline: a row whose SHA is filled must not still name a placeholder ref.
PHBAD=0
while IFS='|' read -r n b s c m; do
  [ -n "$n" ] || continue
  case "$b" in claude/PENDING-*) [ "$s" = "PENDING" ] || { say "   FAIL: row $n has a FILLED SHA ($s) against the PLACEHOLDER ref [$b]"; PHBAD=1; } ;; esac
done <<< "$SEAT_TABLE"
[ "$PHBAD" = "0" ] && say "   PASS: no row carries a filled SHA against a placeholder ref (the assembly's own preflight asserts the same thing before it touches the worktree)" || FAIL=1

# ---- 3. EVERY FILLED SEAT SHA RESOLVES ---------------------------------------------------------
say ""
say "== 3. every FILLED seat SHA resolves, and a grown branch is REPORTED rather than re-pinned =="
if [ ! -d "$G/.git" ]; then
  say "   SKIPPED: $G is not a git repository from here"
  FAIL=1
else
  while IFS='|' read -r n b s c m; do
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
say "== 4. no train-45 SHA survives in CODE (comment hits are PRINTED, because they are a different claim) =="
T45SHAS='234cf8e8d|1922e3ec1|31668f43e|787159ed7|aa7abc006|3e5ead2d1|7d377e27b|a29253a2b|044116000|9092ab8b7|4a8642e7e|c08cb29c5|acab60084|13908a888|d839cb1d7|19bb74012|f613d5cfa|35105cdcd|193af90f5|489c5553c|dddd46493|8d7c348bb|34cf4ad02'
CODEHITS=$(grep -nE "$T45SHAS" "$F" | grep -vE '^[0-9]+: *#' | grep -c . || true)
CMTHITS=$(grep -nE "$T45SHAS" "$F" | grep -E '^[0-9]+: *#' | grep -c . || true)
say "   train-45 seat SHAs in CODE=$CODEHITS (must be 0) :: in COMMENTS=$CMTHITS (allowed -- dated lessons and retirement notes)"
if [ "$CODEHITS" != "0" ]; then
  say "   FAIL: a train-45 SHA is live in this script:"
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
say "== 5. the worktree is NAMED and no other accepted; SCRIPT_PATH is ABSOLUTE and precedes the cd =="
WTLINE=$(grep -n "^WT='" "$F" | head -1)
CDLINE=$(grep -n '^cd /c/Projects/go2cs' "$F" | head -1)
REFUSE=$(grep -c 'WRONG WORKTREE' "$F")
say "   $WTLINE"
say "   $CDLINE"
say "   'WRONG WORKTREE' refusals=$REFUSE (want >= 1)"
{ [ -n "$WTLINE" ] && [ -n "$CDLINE" ] && [ "$REFUSE" -ge 1 ]; } && say "   PASS: the worktree is named in the first lines and any other is refused" || { say "   FAIL: the worktree is not named-and-refused"; FAIL=1; }
SPLINE=$(grep -n '^SCRIPT_PATH=' "$F" | head -1)
say "   $SPLINE"
if printf '%s' "$SPLINE" | grep -qF 'cd "$(dirname "${BASH_SOURCE[0]}")" && pwd'; then
  say "   PASS: SCRIPT_PATH is resolved through cd+pwd BEFORE the script cd's into the assembly worktree, so G11(c)'s grep can open the file"
else
  say "   FAIL: SCRIPT_PATH is not the absolute form.  Train 44 run 1 read '0 of 6 launch lines' from exactly this -- a relative BASH_SOURCE grepped after the cd, reporting the instrument's own blindness as a finding about the script."
  FAIL=1
fi
SPN=$(grep -c '^SCRIPT_PATH=' "$F"); CDN=$(printf '%s' "$CDLINE" | cut -d: -f1); SPL=$(printf '%s' "$SPLINE" | cut -d: -f1)
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
  shape_of(){ grep -E "^ *$1\)" "$F" | head -1 | sed -E "s/.*shapepat='([^']*)'.*/\1/"; }
  forb_of(){  grep -E "^ *$1\)" "$F" | head -1 | sed -E "s/.*forbidden='([^']*)'.*/\1/"; }
  SH_BAD=0
  while IFS='|' read -r n b s c m; do
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
    out=$(printf '%s\n' "$files" | grep -avE "$pat" | grep -ac . || true)
    # shellcheck disable=SC2086
    bad=$(git -C "$G" -c core.quotepath=false diff --name-only "$mb" "$s" -- $frb | grep -ac . || true)
    say "   seat $n ($c) $b @$s :: files=$nfiles outsideShape=$out forbiddenHits=$bad"
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
  if printf '%s\n' 'src/tests/Behavioral/Anything/main.cs' | grep -qE "$D1"; then
    say "   FAIL (decoy): the golib-corpus-handown shape admits a behavioral project file -- seat 3 adds no guard project and must not be able to"
    FAIL=1
  else
    say "   PASS (decoy): golib-corpus-handown refuses a behavioral project path"
  fi
  D2=$(shape_of 'converter-guard')
  if printf '%s\n' 'src/core/golib/slice.cs' | grep -qE "$D2"; then
    say "   FAIL (decoy): the converter-guard shape admits a golib file -- a converter-guard seat must never touch the corpus"
    FAIL=1
  else
    say "   PASS (decoy): converter-guard refuses a golib file"
  fi
  D3=$(shape_of 'golib-gen')
  if printf '%s\n' 'src/gen/go2cs-gen/Generators/ImplementGenerator.cs' | grep -qE "$D3"; then
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
say "=== SELF-CHECK DONE overallFail=$FAIL ==="
exit $FAIL
