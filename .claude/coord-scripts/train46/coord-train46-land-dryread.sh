#!/bin/bash
# ============================================================================================
# TRAIN 46 LAND -- DRY READ.  It runs NOTHING that lands and touches NO tree: it checks that the land
# script's own gate list can be SATISFIED by the assembly script it is written against, that its
# refusal scan is LIVE, and that the five named acceptance paths train 45's land carried are GONE.
#
# ⚠ WHY THIS EXISTS.  A land header claims "every anchor was read out of the assembly script's own
# stamp strings", and nothing checks that claim unless something like this does.  A required-stamp
# list is the stale-figure class wearing a gate's clothes: an anchor whose words no longer appear
# anywhere in the assembly script can never be satisfied, so the landing refuses a HEALTHY battery --
# and the operator then reaches for the switch that makes it stop refusing.  This turns the claim into
# a measurement.
#
#   ARM A  every `req` anchor's SUBSTANTIAL WORDS must appear in the ASSEMBLY SCRIPT.
#   ARM B  the refusal-scan POSITIVE CONTROL, run here rather than only at landing time: every BADPATS
#          pattern must FIRE on a planted line of its own shape -- and must NOT fire on healthy prose.
#   ARM C  the NEGATIVE CONTROL for arm A: words no train has must NOT be found, or arm A is a check
#          that cannot fail.
#   ARM D  **the FIVE named acceptance paths train 45's land carried must be ABSENT from this land
#          script as CODE.**  They may be discussed in its header -- that is the record of why they
#          are gone -- but no variable, no branch and no exclusion may exist.  This is the arm that
#          keeps "a fresh train starts with zero accepted classes" a measurement rather than a claim.
#   ARM E  the ONE surviving exclusion (the wrapper's trailing `assembly exit=N` line) is a SENTINEL
#          declared once and ASSIGNED IN EXACTLY ONE PLACE, so a reader can bound its reach by
#          grepping for the variable name.
#
#   usage:  bash coord-train46-land-dryread.sh
#   exit 0 every arm passed | 1 an arm failed | 3 an input is missing
# ============================================================================================
set -u
SELFDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LAND="$SELFDIR/coord-train46-land.sh"
ASM="$SELFDIR/coord-train46-assemble.sh"
FAIL=0
say(){ printf '%s\n' "$*"; }
[ -f "$LAND" ] || { say "ABORT: $LAND missing"; exit 3; }
[ -f "$ASM" ]  || { say "ABORT: $ASM missing"; exit 3; }
say "land     :: $LAND ($(wc -l < "$LAND") lines)"
say "assemble :: $ASM ($(wc -l < "$ASM") lines)"
say "⚠ TRAIN 46's LAND READS **ONE** RECORD.  Train 45's read two -- the assembly's and a LEG D"
say "  STANDALONE re-measurement -- because its wired LEG D refused at a seeding control and measured"
say "  nothing.  That leg is CORRECTED in the train-46 assembly, so there is no second record and no"
say "  per-anchor source rule: every anchor below is looked up in the assembly script."

# --- the WORD extractor.  ONE definition, used by arm A and by its own negative control.
#     It takes an ERE, unescapes it, drops the regex metacharacters and the bracket/paren groups, and
#     returns the WORDS of length >= 6 that remain.  Those are the words a stamp must contain
#     LITERALLY somewhere -- a leg name, a label, a release, a count word.
#     ⚠ A CHARACTER SCANNER, NOT A REGEX.  Three earlier forms of this helper were all wrong and none
#     of their "findings" was about the anchors: one compared a stamp's RENDERED output against the
#     script's SOURCE (unsound for any interpolating stamp), one lost every word to python's CRLF, and
#     one dropped ESCAPED brackets where it meant to drop UNESCAPED classes.  What caught each round
#     was an implausible number, not a failure.
#     ⚠ `tr -d '\r'` IS LOAD-BEARING: python's print emits CRLF on this box EVEN THROUGH A PIPE.
words_of(){ # $1 = an ERE -> its substantial literal words, one per line
  python - "$1" <<'PY' | tr -d '\r'
import sys
pat = sys.argv[1]
out = []
i = 0
while i < len(pat):
    c = pat[i]
    if c == chr(92) and i + 1 < len(pat):
        out.append(pat[i+1]); i += 2; continue          # an ESCAPED char is a literal
    if c == '[':                                        # an UNESCAPED class is a separator
        j = i + 1
        if j < len(pat) and pat[j] == '^': j += 1
        if j < len(pat) and pat[j] == ']': j += 1
        while j < len(pat) and pat[j] != ']':
            if pat[j] == chr(92): j += 1
            j += 1
        out.append(' '); i = j + 1; continue
    if c in '().*+?^$|{}':                              # every other metachar is a separator
        out.append(' '); i += 1; continue
    out.append(c); i += 1
s = ''.join(out)
# an ESCAPED bracket unescapes to a literal one, and a stamp writes it around an INTERPOLATION
# (`PIN[$lbl]`), so a bracket is a word separator here too.
for ch in '[]()':
    s = s.replace(ch, ' ')
for w in s.split():
    w = w.strip('\"' + chr(39) + ',;:')
    # a <name>=<value> token takes its VALUE from a variable at run time, so only the part up to
    # and including the '=' can be literal in the source.  Necessary, not sufficient.
    if '=' in w:
        w = w.split('=')[0] + '='
    if len(w) >= 6:
        print(w)
PY
}

say ""
say "== ARM A: every SUBSTANTIAL WORD of every req anchor must appear in the ASSEMBLY script =="
say "   ⚠ THIS IS A **NECESSARY CONDITION**, AND IT IS LABELLED AS ONE.  A stamp's rendered OUTPUT is"
say "     not in its SOURCE when it interpolates -- the script says stamp \"PIN[\$lbl] OK :: \$gv\" and"
say "     the anchor is a statement about the RUNTIME line -- so no static read can prove an anchor is"
say "     SATISFIABLE.  What it CAN prove is that the anchor names words this battery has: a leg name,"
say "     a label, a release, a count word.  That is the hazard the arm exists for, and claiming more"
say "     would be an over-reach."
N=0; MISS=0; THIN=0
while IFS= read -r line; do
  pat=$(printf '%s' "$line" | sed -E "s/^[[:space:]]*req +'([^']*)'.*/\1/")
  [ -n "$pat" ] || continue
  [ "$pat" != "$line" ] || continue
  N=$(( N + 1 ))
  ws=$(words_of "$pat")
  nw=$(printf '%s\n' "$ws" | grep -ac . || true)
  if [ "${nw:-0}" -lt 1 ]; then
    THIN=$(( THIN + 1 ))
    say "   ⚠ no substantial word in [$pat] -- reported, not asserted"
    continue
  fi
  bad=""
  while IFS= read -r w; do
    [ -n "$w" ] || continue
    grep -aqF -- "$w" "$ASM" || bad="$bad [$w]"
  done <<< "$ws"
  if [ -z "$bad" ]; then
    say "   ok   ($nw word(s)) $pat"
  else
    MISS=$(( MISS + 1 ))
    say "   MISS word(s) absent from the assembly script:$bad   <- from req [$pat]"
  fi
done < <(grep -aE "^[[:space:]]*req '" "$LAND")
say "   req anchors=$N  anchors naming a word the battery does not have=$MISS  anchors with no substantial word=$THIN"
[ "$MISS" = "0" ] && say "   PASS: every anchor names words the assembly script actually carries" || { say "   FAIL: an anchor names a leg, label or token this battery does not have -- it can NEVER be satisfied, and the landing would refuse a healthy battery"; FAIL=1; }

say ""
say "== ARM C (negative control for ARM A): words no train has must NOT be found =="
# ⚠ THE CONTROL WORDS ARE CHOSEN TO BE ABSENT **EVEN AS PROSE**, and that is not a detail.  An earlier
#   form used `TokenValueTagRefusalTests`, which the train-46 assembly legitimately MENTIONS in a dated
#   note recording what train 45's LEG 3 arm asserted and why it was retired -- so the control failed
#   for the right reason and about the wrong thing.  A negative control whose token can appear in a
#   RETIREMENT NOTE cannot discriminate a carried anchor from an honest record.
for bad in 'FinalizerBindingTests' 'coord-doctrine-batch18' 'seat-7 arm COUNT' 'LEG 4b CNR exit=' 'SwitchPointerSentinelCase' 'WindowsNewCallback' 'laneR-golib-sema'; do
  if grep -aqF -- "$bad" "$ASM"; then say "   FAIL (negative control): [$bad] was FOUND in the assembly script, so arm A's lookup cannot discriminate a carried anchor -- these are train 45's own words"; FAIL=1
  else say "   PASS (negative control): [$bad] is correctly ABSENT -- train 45's own words, and an anchor carrying one would be a list copied across trains"; fi
done

say ""
say "== ARM B: the refusal scan's POSITIVE CONTROL, run here rather than only at landing time =="
# the BADPATS array is EXTRACTED from the land script, never retyped -- a retyped copy is what drifts.
eval "$(awk '/^BADPATS=\(/{f=1} f{print} /^\)$/{if(f) exit}' "$LAND")"
say "   BADPATS extracted from the land script :: ${#BADPATS[@]} pattern(s)"
[ "${#BADPATS[@]}" -ge 15 ] || { say "   FAIL: only ${#BADPATS[@]} pattern(s) extracted -- the extraction, not the scan, is broken"; FAIL=1; }
CTL=$(mktemp /tmp/t46-dryread-ctl.XXXXXX)
{ echo '[t] planted -- ABORT'; echo '[t] ABORT disk 1GB < 30'; echo '[t]   ^ ABORT: planted'
  echo '[t]   ^ G3 REFUSED: planted'; echo '[t] BASE ANCESTRY REFUSED :: planted'
  echo '[t] === A-ASSERTIONS FAILED ==='; echo '[t] === CHAIN STOPPED at LEG 2 ==='
  echo '[t] G11(a) JUSTIFICATION FALSE :: planted'; echo '[t] LOCK HELD: planted'
  # ⚠ THE PLANTED RECORD DELIBERATELY CARRIES NO **LEG D**, NO **LEG K VERDICT** AND NO **A5** LINES.
  #   The land script REQUIRES three `LEG D <target> **MET**` stamps, three `LEG K <row> VERDICT ::
  #   PASS` stamps and two `A5 NESTED ... slnxRegistrations=1` stamps; a control that quietly planted
  #   them could not tell you the checker is looking for them.  A control that plants every line a
  #   checker wants cannot say the checker is checking.
  echo '[t] WRONG WORKTREE: planted'; echo '[t] LEG K UNMEASURED :: planted'
  echo '[t]   ^ G5 UNMEASURED: planted'; echo '[t] DOCS-DELTA x :: numstat not numeric -- UNMEASURED'
  echo '[t] seat 3 MERGE FAILED for x'; echo '[t]   PRESERVE FAILED for x'
  echo '[t]   ^ LEG K os :: NO VERDICT LINE -- instrument fault.'
  echo '[t]   ^ LEG 5 FINDING: planted'; echo '[t]   ^ A7 REFUSED: planted'; } > "$CTL"
DEAD=0
for bad in "${BADPATS[@]}"; do
  c=$(grep -acE -- "$bad" "$CTL" 2>/dev/null | tr -d '\r')
  case "${c:-0}" in ''|0) DEAD=$(( DEAD + 1 )); say "   DEAD pattern [$bad] found NOTHING on a planted line of its own shape" ;; esac
done
say "   dead patterns=$DEAD of ${#BADPATS[@]} (must be 0)"
[ "$DEAD" = "0" ] && say "   PASS: every refusal pattern can fire" || { say "   FAIL: a refusal pattern is dead -- a scan that cannot fire reads exactly like a clean log"; FAIL=1; }
HEALTHY=$(mktemp /tmp/t46-dryread-ok.XXXXXX)
{ echo '[t] LEG C FULL converter suite exit=0 wall=210s'; echo '[t] LEG 4 CNR exit=0 wall=1200s'
  echo "[t] LEG 4 E1' MET :: exit 0, CHANGED == 0 on BOTH readings"; echo '[t] seat 1 merged claude/x @abc123456 :: files=25'
  echo '[t] LEG D windows **MET** :: the base-vs-cut emission delta is EXACTLY the predicted set'
  echo '[t] SEAT 1 NOTE :: a seat that ABORTS would say so, and this prose must not be read as one'
  echo '[t] seat 3 :: **PRE-RESOLVED MERGE COMMITTED** -- 2 path(s) came from the rehearsal saved set'; } > "$HEALTHY"
HITS=0
for bad in "${BADPATS[@]}"; do
  c=$(grep -acE -- "$bad" "$HEALTHY" 2>/dev/null | tr -d '\r'); HITS=$(( HITS + ${c:-0} ))
done
say "   the scan over a HEALTHY synthetic record :: $HITS hit(s) (must be 0 -- a pattern that matches ordinary prose reds every landing)"
say "   ⚠ THE HEALTHY RECORD INCLUDES A **PRE-RESOLVED MERGE** LINE ON PURPOSE.  Train 46's merge_seat"
say "     can commit a conflict from the rehearsal's SAVED resolution set, and that stamp must not read"
say "     as a refusal: it contains the words MERGE and RESOLVED, and a bare-word scan would fire on it."
[ "$HITS" = "0" ] && say "   PASS: no pattern fires on healthy prose" || { say "   FAIL: a refusal pattern matches a healthy line"; FAIL=1; }
rm -f "$CTL" "$HEALTHY"

say ""
say "== ARM D: the FIVE named acceptance paths train 45's land carried must be ABSENT as CODE =="
say "   ⚠ THIS IS THE ARM THAT KEEPS 'a fresh train starts with zero accepted classes' A MEASUREMENT."
say "     Each of those paths existed for a REAL fault measured on train 45, and each of those faults"
say "     is FIXED IN THE TRAIN-46 ASSEMBLY SCRIPT rather than accepted here.  An acceptance path that"
say "     outlives its fault is a LIE-LEVER: it accepts a SHAPE, and the next thing that produces that"
say "     shape may be a real defect."
DBAD=0
for v in 'G10D_FAULT' 'G10D_REFUSAL' 'LEGD_STANDALONE_PATH' 'LEGD_XCL' 'LEGD_REC' 'LEG4DRIFT_PATH' 'LEG4EXIT_XCL' 'LEG4_XCL' 'BEHFIXUP_PATH' 'BEHFIXUP_XCL' 'LEGD_SRC'; do
  n=$(grep -acF -- "$v" "$LAND" || true)
  if [ "${n:-0}" = "0" ]; then say "   ok   [$v] absent"
  else say "   FAIL: [$v] appears $n time(s) in the land script -- a train-45 acceptance path survives as CODE"; DBAD=1; fi
done
for m in 'G10D-TEXTATTR-FAULT BEGIN' 'LEGD-STANDALONE BEGIN' 'LEGD-LINUX-LINEFORM BEGIN' 'LEG4-DRIFT-REBASELINED BEGIN' 'BEHAVIORAL-FIXUP BEGIN' 'LEG4-ANCHORS-PERSHAPE BEGIN'; do
  n=$(grep -acF -- "$m" "$LAND" || true)
  if [ "${n:-0}" = "0" ]; then say "   ok   [$m] block absent"
  else say "   FAIL: the [$m] block survives in the land script"; DBAD=1; fi
done
# and the DISCUSSION of them is REQUIRED, because a path removed without a record of why is a removal
# nobody can audit.
DISC=$(grep -acF -- 'G10D-TEXTATTR' "$LAND" || true)
say "   the header's RECORD of why they are gone :: 'G10D-TEXTATTR' mentioned $DISC time(s) (want >= 1 -- discussion is REQUIRED, code is FORBIDDEN, and the two are different things)"
[ "${DISC:-0}" -ge 1 ] || { say "   FAIL: the land script removes the paths without saying why -- a removal nobody can audit"; DBAD=1; }
[ "$DBAD" = "0" ] && say "   PASS: zero accepted classes as CODE, with the record of why kept as PROSE" || FAIL=1

say ""
say "== ARM E: the ONE surviving exclusion is a SENTINEL assigned in exactly ONE place =="
WD=$(grep -acE '^WRAPEXIT_XCL=' "$LAND" || true)
WA=$(grep -acE '^[[:space:]]*WRAPEXIT_XCL="\$WRAPEXIT_PAT"' "$LAND" || true)
WU=$(grep -acF -- 'WRAPEXIT_XCL' "$LAND" || true)
say "   WRAPEXIT_XCL :: declarations at column 0=$WD (want 1) :: assignments to the live pattern=$WA (want 1) :: total mentions=$WU"
{ [ "$WD" = "1" ] && [ "$WA" = "1" ]; } && say "   PASS: declared once, assigned in exactly one place, so a reader can bound its reach by grepping the variable name" || { say "   FAIL: the sentinel is declared or assigned more than once -- an exclusion whose reach a reader cannot bound is the lever this shape exists to avoid"; FAIL=1; }
# the three MEASURED conditions must all be read
for cond in 'WX_N=' 'WX_ISLAST=' 'compositionAccepted='; do
  grep -aqF -- "$cond" "$LAND" && say "   ok   the condition [$cond] is read and stamped" || { say "   FAIL: the condition [$cond] is not read -- the exclusion would fire on fewer than its three measured conditions"; FAIL=1; }
done
RX=$(grep -acE "^RXCL='<<<NO-REFUSAL-EXCLUSION-IN-FORCE>>>'$" "$LAND" || true)
RXA=$(grep -acE '^[[:space:]]*RXCL=' "$LAND" || true)
say "   RXCL :: the constant sentinel is declared $RX time(s) (want 1) and assigned anywhere $RXA time(s) (want 1 -- the declaration itself, and NOTHING else may assign it)"
{ [ "$RX" = "1" ] && [ "$RXA" = "1" ]; } && say "   PASS: the refusal-scan exclusion is a constant nothing can change" || { say "   FAIL: something assigns RXCL -- the refusal scan has grown an acceptance path"; FAIL=1; }

say ""
say "=== DRY READ DONE overallFail=$FAIL ==="
exit $FAIL
