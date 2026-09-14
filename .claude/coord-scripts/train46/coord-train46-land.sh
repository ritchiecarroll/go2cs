#!/bin/bash
# ============================================================================================
# TRAIN 46 LAND -- THREE named seats (two converter+guard, one golib/corpus/registry stack) plus up
# to TWO placeholder rows.  DERIVED from coord-train45-land.sh: same skeleton -- read the assembly's
# own stdout for its DONE stamp and its NAMED gate stamps, do the tree arithmetic, and push NOTHING
# unless every read agrees.  LAND_VERIFY_ONLY=1 runs every gate and pushes nothing.
#
# ⚠⚠ **THIS FILE CARRIES NO NAMED ACCEPTANCE PATH, AND THAT IS THE POINT.**  Train 45's land grew
#   FIVE of them, each for a real fault measured on that train:
#     * G10D-TEXTATTR       -- G10d asserted CRLF over every docs/*.md "under the eol=crlf pin" when
#                              docs/ is outside that pin, and refused a seated file carrying the
#                              `-text` verbatim-bytes exemption.
#     * LEGD-STANDALONE     -- the wired LEG D refused at a seeding control written one layer off its
#                              hazard and measured nothing, so its eight gate stamps had to be read
#                              out of a standalone re-measurement.
#     * LEGD-LINUX-LINEFORM -- that standalone's own linux target read MISSED because the committed
#                              hunk it `cmp`ed against was WINDOWS-derived.
#     * LEG4-DRIFT-REBASELINED / BEHAVIORAL-FIXUP
#                           -- one attributed golden drift the assembly commit re-baselined.
#   **EVERY ONE OF THOSE FAULTS IS FIXED IN coord-train46-assemble.sh RATHER THAN ACCEPTED HERE.**
#   G10d now CONSULTS `git check-attr text` and exempts a `-text` file only when its two layers
#   agree, stamping the exemption COUNT beside the mismatch count; LEG D is corrected to the
#   standalone's shape (three-arm seeding control, both arms from `git archive`, six seeds from one
#   snapshot, write-evidence per arm per target) AND its per-target line comparison is BY MECHANISM,
#   which is what the linux finding forced.  So the composition below accepts ONLY when the refusal
#   residual is EMPTY.
#   ⚠ A FRESH TRAIN STARTS WITH ZERO ACCEPTED CLASSES.  An acceptance path that outlives its fault is
#   a LIE-LEVER: it accepts a SHAPE, and the next thing that produces that shape may be a real defect.
#   If this train needs one, it is written HERE, against a MEASUREMENT, with its own control -- and
#   the next derive deletes it again.
#
# ⚠ THE **ONE** EXCLUSION THAT SURVIVES IS THE WRAPPER'S TRAILING `assembly exit=N` LINE, and it is
#   not an acceptance path.  The battery is launched by a shell that echoes the assembly script's
#   exit status after it ends; that echo lands in the record as its LAST line.  It is NOT a leg stamp
#   -- no leg printed it, none can be attributed to it -- and it carries the SAME FACT the DONE stamp
#   already carries as overallFailed=N.  Its three conditions are ALL MEASURED (see the block below)
#   and it fires ONLY when the composition accepted.
#
# ⚠ EVERY GATE HERE IS A **READ** OF A RECORD PLUS TREE ARITHMETIC.  Nothing is re-run: the assembly
#   is the measurement and this is the reconciliation.  A stamp that is absent is a leg that did not
#   report it, which is not the same as a leg that failed -- and it is never a pass either.
#
#   usage:  bash coord-train46-land.sh                     # gates, then LEASE-PUSH master
#           LAND_VERIFY_ONLY=1 bash coord-train46-land.sh  # gates only, nothing pushed
#           LAND_ASM=<path> bash coord-train46-land.sh     # a differently-named assembly record
#   exit 0 landed (or verify-only clean) | 2 wrong tree / dirty / head mismatch |
#        3 the assembly record is missing, empty, or did not finish | 4 a gate failed (nothing pushed) |
#        5 the push did not land
# ============================================================================================
set -u
WT_DEFAULT=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c
WT="${LAND_WT:-$WT_DEFAULT}"
WT_OVERRIDE=$( [ "$WT" = "$WT_DEFAULT" ] && echo no || echo YES )
# ⚠ THE TOPLEVEL EXPECTATION IS **DERIVED FROM $WT BY ONE RULE**, NEVER WRITTEN TWICE.  msys spells a
#   drive `/c/...` while git answers `C:/...`; the two spellings are reconciled with cygpath -- the
#   tool that knows the mapping -- and the hand rule is only the fallback for a shell without it.
if command -v cygpath >/dev/null 2>&1; then WT_TOP=$(cygpath -m "$WT" 2>/dev/null)
else case "$WT" in /?/*) WT_TOP="$(printf '%s' "$WT" | cut -c2 | tr 'a-z' 'A-Z'):$(printf '%s' "$WT" | cut -c3-)" ;; *) WT_TOP="$WT" ;; esac; fi
BASE_EXPECT=44f858717            # train 45's landing; this train's base
KEEP_BRANCHES='claude/g-weak-rekey'   # never pruned by this train, whatever its ancestry reads
SELF="${LAND_SELF_ABS:-$0}"
# ⚠ EVERY PATH THIS SCRIPT READS AFTER THE `cd` IS RESOLVED TO AN ABSOLUTE ONE **HERE**, BEFORE IT.
#   A path left RELATIVE resolves against the WORKTREE afterwards, names a file that is not there, and
#   every read off it comes back a well-formed EMPTY.
SELFDIR=$(cd "$(dirname "$SELF")" 2>/dev/null && pwd) || SELFDIR=$(dirname "$SELF")
ASM="${LAND_ASM:-$SELFDIR/coord-train46-assemble-run1.stdout}"
case "$ASM" in /*|[A-Za-z]:[\\/]*) ;; *) ASM="$(cd "$(dirname "$ASM")" 2>/dev/null && pwd)/$(basename "$ASM")" ;; esac
LOG="$SELFDIR/coord-train46-land.log"
stamp(){ echo "[$(date '+%F %T')] $*" | tee -a "$LOG"; }
FAIL=0

# ============================================================================================
# THE REFUSAL / FINDING PATTERNS, DEFINED **ONCE**, WITH TWO CONSUMERS: the refusal scan and that
#   scan's own positive control.
#   ⚠ EVERY PATTERN IS SHAPED SO IT CANNOT MATCH AN UNCONDITIONAL STAMP: the assembly's own SEAT
#   NOTES and its LIGHT-GATES-DONE stamp discuss refusals in PROSE, and a bare `ABORT` or
#   `UNMEASURED` scan would go RED on a healthy run for exactly that reason -- route #8's
#   text-grepping family, met from the reading side.  So the anchors are the REFUSAL FORMS the stamps
#   use and never the bare words.
# ============================================================================================
BADPATS=(
  '-- ABORT'
  '\] ABORT '
  'ABORT:'
  'REFUSED:'
  'REFUSED ::'
  'A-ASSERTIONS FAILED'
  'CHAIN STOPPED'
  'JUSTIFICATION FALSE'
  'LOCK HELD'
  'WRONG WORKTREE'
  'UNMEASURED( \([^)]*\))? ?::'
  'UNMEASURED( \([^)]*\))?:'
  '-- UNMEASURED'
  'MERGE FAILED'
  'PRESERVE FAILED'
  'NO VERDICT LINE'
  '(LEG [0-9A-Za-z]+|G[0-9]+[a-z]*|A[0-9]+) (FINDING|SHORT)[:,( ]'
)
# ⚠ THE EXCLUSION IS A CONSTANT AND NOTHING IN THIS FILE ASSIGNS IT.  Train 45's became the accepted
#   G10d line when its fault path fired; with every fault fixed in the assembly there is no code that
#   assigns this, so the refusal scan below excludes NOTHING and the stamp says so.
RXCL='<<<NO-REFUSAL-EXCLUSION-IN-FORCE>>>'

# ⚠ THE WRAPPER'S OWN `assembly exit=N` LINE.  DECLARED here, ASSIGNED at the exit scan and NOWHERE
#   else, so a reader can bound its reach by grepping this file for the variable name.
WRAPEXIT_XCL='<<<NO-WRAPPER-EXIT-EXCLUSION-IN-FORCE>>>'
WRAPEXIT_PAT='^assembly exit=[0-9]+$'
WX_N=0; WX_ANY=0; WX_ISLAST=0; WX_LAST=ABSENT; WX_CODE=x

[ -s "$ASM" ] || { stamp "assembly record $ASM is missing or empty -- ABORT (set LAND_ASM if the run used another name)"; exit 3; }
stamp "assembly record :: $ASM ($(wc -l < "$ASM") lines, last written $(date -r "$ASM" '+%F %T' 2>/dev/null || echo unknown))"
stamp "worktree in force :: $WT :: LAND_WT override=$WT_OVERRIDE (default $WT_DEFAULT) :: toplevel expectation DERIVED from it = $WT_TOP.  ⚠ A run whose override reads YES is a CONTROL run: its verdicts are about this script and never about a train"
cd "$WT" || { stamp "cannot cd $WT -- ABORT"; exit 2; }
WT_TOPREAD=$(git rev-parse --show-toplevel | tr -d '\r')
[ "$WT_TOPREAD" = "$WT_TOP" ] || { stamp "WRONG WORKTREE -- ABORT :: git answers [$WT_TOPREAD] where the derivation from \$WT wants [$WT_TOP]"; exit 2; }

# --- the assembly's own DONE stamp.  A record with no DONE stamp is a run that did not finish, which
#     is a different state from a run that failed, and it is reported as its own state.
DONE=$(grep -a '=== train46 ASSEMBLE DONE' "$ASM" | tail -1 | tr -d '\r')
[ -n "$DONE" ] || { stamp "no ASSEMBLE DONE stamp in $ASM -- the assembly did not finish; ABORT"; exit 3; }
stamp "DONE stamp :: $DONE"
HEAD_EXPECT=$(printf '%s' "$DONE" | grep -oE 'head=[0-9a-f]+' | cut -d= -f2)
OF=$(printf '%s' "$DONE" | grep -oE 'overallFailed=[0-9]+' | cut -d= -f2)
SEATSN=$(printf '%s' "$DONE" | grep -oE 'seats=[0-9]+' | cut -d= -f2)
SKIPN=$(printf '%s' "$DONE" | grep -oE 'skippedPending=[0-9]+' | cut -d= -f2)
REQALL=$(printf '%s' "$DONE" | grep -oE 'requireAll=[0-9]+' | cut -d= -f2)
NEWB=$(printf '%s' "$DONE" | grep -oE 'newBehavioralProjects=[0-9]+' | cut -d= -f2)
NEWBALL=$(printf '%s' "$DONE" | grep -oE 'newBehavioralProjectsAllDepths=[0-9]+' | cut -d= -f2)
HEAD=$(git rev-parse --short HEAD)

# ============================================================================================
# THE ACCEPTANCE COMPOSITION.  ⚠ IT ACCEPTS ONLY AN EMPTY RESIDUAL.
#   The refusal scan below partitions the record's refusal lines; with NO named path in this file the
#   partition has exactly one cell, and `overallFailed` is accepted only when it reads 0.  This is
#   deliberately the SIMPLEST form the composition can take, because a fresh train starts with zero
#   accepted classes and every path this file might grow has to be argued from a measurement first.
# ============================================================================================
OF_OK=0
if [ "${OF:-1}" = "0" ]; then
  OF_OK=1
  stamp "ACCEPTANCE COMPOSITION :: overallFailed=0 -- the battery's own OR of its legs is ZERO, so there is no refusal class to accept and the residual is empty BY CONSTRUCTION.  ⚠ This file carries NO named acceptance path: every fault train 45's land had to accept is FIXED in the train-46 assembly script (G10d consults the -text attribute; LEG D is corrected to the standalone's shape and compares BY MECHANISM), and an acceptance path that outlives its fault is a lie-lever."
else
  stamp "ACCEPTANCE COMPOSITION :: **NOT** accepted :: overallFailed=$OF.  This file has no named path that can accept ANY refusal class, by design.  Read the leg stamps: a leg that REFUSED is a leg that measured nothing, and a leg that FOUND something is a HOLD.  If a class here genuinely deserves acceptance it is argued from a MEASUREMENT, written into this file with its own control, and deleted again by the next derive."
fi
[ "$OF_OK" = 1 ] || { stamp "assembly overallFailed=$OF -- ABORT (nothing pushed)"; exit 4; }

# --- THE SEAT ACCOUNTING.  ⚠ A LANDING RUN MUST BE THE TRAIN THE COORDINATOR ANNOUNCED.
#     Train 46 was derived with TWO PLACEHOLDER rows, and the assembly SKIPS a PENDING row with a
#     stamp by default -- which produces a perfectly consistent record for a train nobody announced.
#     `TRAIN46_REQUIRE_ALL=1` is what makes a PENDING row ABORT instead, so a landing record must
#     carry requireAll=1 AND skippedPending=0.  Both are read, because they are two claims: the first
#     says the run was CONFIGURED to demand a full table, the second says it MET that demand.
stamp "seat accounting :: seats=$SEATSN skippedPending=${SKIPN:-ABSENT} requireAll=${REQALL:-ABSENT} newBehavioralProjects(top)=${NEWB:-ABSENT} (allDepths)=${NEWBALL:-ABSENT}"
[ "${REQALL:-x}" = "1" ] || { stamp "  ^ ABORT: the record reads requireAll=${REQALL:-ABSENT}.  A landing run sets TRAIN46_REQUIRE_ALL=1, which is what makes a PENDING row REFUSE rather than be skipped -- without it this record describes a PARTIAL train, however internally consistent its numbers are."; exit 4; }
[ "${SKIPN:-x}" = "0" ] || { stamp "  ^ ABORT: the record reads skippedPending=${SKIPN:-ABSENT}.  A skipped seat means this tree is not the announced train."; exit 4; }
[ "${SEATSN:-0}" -ge 3 ] || { stamp "  ^ ABORT: the record reads seats=$SEATSN where this train's three NAMED seats are unconditional."; exit 4; }
SEATROWS=$(grep -a 'SEAT TABLE ::' "$ASM" | tail -1 | grep -oE '^\[[^]]*\] SEAT TABLE :: [0-9]+' | grep -oE '[0-9]+$')
stamp "seat table as the assembly read it :: rows=${SEATROWS:-UNREADABLE} :: merged=$SEATSN (they must agree -- a row that merged is a row that was listed, and the assembly asserts the same thing at run time)"
[ "${SEATROWS:-x}" = "$SEATSN" ] || { stamp "  ^ ABORT: the assembly listed ${SEATROWS:-?} row(s) and merged $SEATSN.  With requireAll=1 those cannot differ, so the record is not the one this gate can read."; exit 4; }

if [ "$HEAD" = "$HEAD_EXPECT" ]; then
  stamp "worktree HEAD $HEAD == assembled head $HEAD_EXPECT"
else
  stamp "worktree HEAD $HEAD != assembled head $HEAD_EXPECT -- ABORT (this record describes a tree this worktree is not holding, and this file carries NO path that can accept a head mismatch)"
  exit 2
fi
[ "$(git status --porcelain | wc -l)" = 0 ] || { stamp "tree dirty -- ABORT"; git status --porcelain | head -20 | sed 's/^/    /' | tee -a "$LOG"; exit 2; }
grep -a "base=$BASE_EXPECT" <<<"$DONE" >/dev/null || { stamp "DONE stamp base != $BASE_EXPECT -- ABORT"; FAIL=1; }

# --- THE SEAT LIST, DERIVED FROM THE RECORD.  ⚠ NOT HARDCODED HERE.  Two rows were PLACEHOLDERS when
#     this file was written; a hardcoded list would name a SHA nobody has, and a list edited later is
#     a second copy of the seat table that drifts.  The record's own merge stamps are the measurement,
#     and a PRE-RESOLVED merge is counted here exactly as a clean one is -- it is still a seat.
SEATS=$(grep -aoE 'seat [0-9]+ merged [^ ]+ @[0-9a-f]+' "$ASM" | awk '{print $4":"substr($5,2)}' | sort -u)
SEATN=$(printf '%s\n' "$SEATS" | grep -c .)
stamp "seat list DERIVED from the record's own merge stamps :: $SEATN row(s)"
printf '%s\n' "$SEATS" | sed 's/^/      /' | tee -a "$LOG"
[ "$SEATN" = "$SEATSN" ] || { stamp "  ^ the record carries $SEATN 'seat N merged' stamp(s) where its DONE stamp says $SEATSN merged -- a seat did not merge, or the record is truncated"; FAIL=1; }
PRERES=$(grep -ac 'PRE-RESOLVED MERGE COMMITTED' "$ASM" || true)
[ "${PRERES:-0}" = "0" ] && stamp "pre-resolved merges :: 0 -- every seat merged CLEAN" || stamp "pre-resolved merges :: $PRERES seat(s) were committed from the rehearsal's SAVED resolution set.  ⚠ STATED, NEVER ABSORBED: those conflicts were resolved by the coordinator at rehearsal time and applied MECHANICALLY here, and every later reading of this train describes that tree."

# ============================================================================================
# THE NAMED GATE STAMPS THIS TRAIN MUST CARRY.
#   ⚠ EVERY ANCHOR BELOW WAS READ OUT OF coord-train46-assemble.sh's OWN STAMP STRINGS.  None is
#   carried and none is remembered.  A required-stamp list copied across trains is the stale-figure
#   class wearing a gate's clothes.
#   ⚠ A stamp counted here is a leg that REPORTED its own success in the words it was written to use.
#   An anchor that goes missing is either a leg that did not run or a script that was re-worded, and
#   BOTH are refusals: this file cannot tell them apart and must not guess.
# ============================================================================================
req(){ # $1 = ERE, $2 = minimum count, $3 = what it means, $4 = an alternative record file
  # ⚠ THE `--` IS LOAD-BEARING.  A pattern that begins with `-` is parsed by grep as an OPTION, and
  # without the separator grep exits "unknown option", prints NOTHING, and the count reads EMPTY.
  local pat="$1" want="$2" what="$3" f="${4:-$ASM}" n
  if [ ! -s "$f" ]; then stamp "gate stamp UNREADABLE :: $f is missing or empty :: $what"; FAIL=1; return; fi
  n=$(grep -acE -- "$pat" "$f")
  if [ "$n" -ge "$want" ]; then stamp "gate stamp present ($n, want >= $want) in $(basename "$f") :: $what"
  else stamp "gate stamp MISSING (found $n, want >= $want) in $(basename "$f") :: $what   [pattern: $pat]"; FAIL=1; fi
}

# --- the preflight controls
req 'CONTROL C5 toolchain pin ::' 1 'C5 the toolchain pin refused the old release for a module that needs the new one'
req 'CONTROL C6 LEG K oracleGoVersion predicate' 1 'C6 the LEG K oracle predicate was CONTROLLED over six planted values before the run (the real recorded form accepted; the wrong release, the bare token, the PREFIX release go1.23.1 and an EMPTY value all refused)'
req 'CONTROL C3 live-process census positive control' 1 'C3 the live-process census was proven able to ANSWER before its zero was believed'
req 'CONTROL C4 CR byte probe' 1 'C4 the CR/LF byte probe was controlled in BOTH directions'
# --- the base, asserted rather than derived
req 'BASE ASSERTION :: derived-time base' 1 "the assembly ASSERTED its own base against $BASE_EXPECT rather than deriving BASE from HEAD and carrying on -- a gate reading has a TREE, and when the tree moves the reading expires whether or not the change does"
# --- the toolchain pins
req 'PIN\[POST-MERGE\] OK :: go version go1\.24\.13' 1 'the post-merge pin established go1.24.13'
req 'PAIRING\[LEG-[045]\] OK :: go version go1\.23\.12' 3 'the two-pin PAIRING established go1.23.12 on LEG 0, LEG 4 and LEG 5'
req 'PAIRING\[LEG-[045]\] OK :: .*GOTOOLCHAIN=auto' 3 'each pairing leg read GOTOOLCHAIN=auto (local BREAKS the pairing, so this is not a decoration)'
req 'PAIRING\[LEG-[045]\] OK :: .*no go\.mod here' 3 'each pairing assertion was taken at a cwd with NO go.mod'
req 'AFTER-GUARD\[LEG-[45]\] OK ::.*go1\.24\.13' 2 'the after-guard read go1.24.13 off the produced binary after LEG 4 AND after LEG 5'
# --- LEG C / LEG Cg, the converter suite
req 'LEG C toolchain, read AT THE SUITE' 1 "LEG C read the toolchain AT src/go2cs's own cwd with go env GOVERSION"
req 'LEG C FULL converter suite exit=0 ' 1 'LEG C the FULL converter suite exit=0 at go1.24.13 -- a REAL GATE this train, because THREE seats change src/go2cs'
req 'LEG Cg named guards exit=0 ' 1 "LEG Cg the seats' own named guards exit=0"
req 'G7b old-pin refusal ::' 1 'G7b the old-pin control ran on the real tree (its RED is the pass)'
# --- LEG D, the union two-seeded three-target emission diff, CORRECTED
req 'LEG D CONTROL arm 1 \(the seeding method itself, CORRECTED\)' 1 'LEG D the CORRECTED seeding control ran arm 1 -- NON-test converter source CR-strip-identical between the archive and the checkout, with the RAW count stated beside it and the _test.go population counted'
req 'LEG D CONTROL arm 2 \(the raw-differing set, CLASSIFIED\)' 1 'LEG D arm 2 CLASSIFIED the raw-differing set rather than subtracting it -- every raw-differing file, `_test.go` included, must be CR-strip-identical'
req 'LEG D CONTROL arm 3 \(the //go:embed EMISSION INPUTS, BYTE-exact\)' 1 'LEG D arm 3 compared the //go:embed targets BYTE-exactly over a population DERIVED FROM THE DIRECTIVES, with the count asserted > 0 -- the arm the whole seeding argument rests on, and the one that would be a vacuous green if the derivation resolved nothing'
req 'LEG D binaries :: the two hashes DIFFER=yes' 1 'LEG D the two converter binaries are genuinely two converters -- with three converter seats aboard, byte-identical binaries would mean the base arm was built from the union source'
req 'LEG D binaries :: BOTH arms are built from' 1 'LEG D BOTH arms were built from `git archive`, which is the tightening that makes the seeding question symmetric'
req 'LEG D seed CONTENT control ::' 1 'LEG D a hand-own inside a package this train TOUCHES is byte-identical across all six seeds and the worktree -- a CONTENT test, because mtimes are only a hint'
req 'LEG D ARM (base|cut)/(windows|linux|darwin) :: exit=0 .*filesWRITTEN-this-run=[1-9]' 6 'LEG D all SIX arms (2 arms x 3 targets) exited 0 having WRITTEN a non-zero number of files.  ⚠ Two untouched seeds compared against each other read 0/0/0 and are indistinguishable from a clean gate'
req 'LEG D \*\*PREDICTION\*\* \(stamped BEFORE the diff' 1 'LEG D the prediction was stamped BEFORE the diff, derived at run time from EVERY MERGED SEAT, never carried'
req 'LEG D PREDICTION derivation :: THREE-dot per seat' 1 'LEG D the expected set was derived with the THREE-DOT form per seat.  A two-dot query against a base a seat forked before returns master OWN newer content REVERSED'
req 'LEG D DIFF (windows|linux|darwin) content artifact ::' 3 "LEG D the per-target diff CONTENT was written out BEFORE the purge, for MET and MISSED alike -- a gate whose cleanup destroys the artifact it measures reads as a clean sweep"
req 'LEG D (windows|linux|darwin) \*\*MET\*\*' 3 'LEG D MET on ALL THREE targets, every changed line by the predicted MECHANISM (the comparison the linux finding forced: a WINDOWS-derived committed hunk `cmp`ed against a linux emission read MISSED on a target that had MET perfectly)'
req 'LEG D exit=0 :: \*\*MET on all three targets\*\*' 1 "LEG D's own closing verdict"
# --- LEG R
req 'LEG R converter build exit=0 ' 1 'LEG R built the converter at 1.24.13 before running the pipeline at 1.23.12 (the two-pin shape)'
req 'LEG R (reflect|errors) OK :: ' 2 "LEG R BOTH rows' test emission CONVERTS and their test assemblies BUILD at the merge result -- the class NO standing gate compiles, reached in this train by seat 3's manual-conversion REGISTRY change"
# --- the integrity, solution and GolibTests legs
req 'LEG 1 integrity-(windows|linux|darwin) exit=0 ' 3 'LEG 1 corpus/solution integrity, all THREE GOOS, exit=0'
req 'LEG 1 registration arithmetic \(DERIVED' 1 'LEG 1 the behavioral registration DELTA was derived from the tree.  ⚠ ROUTE #3: a project on disk that no solution entry names passes every harness gate and breaks only Visual Studio, and this train adds NESTED sub-libraries whose registrations are NOT the top-level count'
req 'LEG 2 stdlib slnx exit=0 .* CS=0 MSB/NETSDK=0' 1 'LEG 2 go2cs-stdlib.slnx exit=0 with CS=0 and MSB/NETSDK=0'
req 'LEG 2b go2cs\.slnx exit=0 .* CS=0 MSB/NETSDK=0' 1 'LEG 2b go2cs.slnx exit=0 -- CLAUDE.md names this build BY NAME for a golib API change AND it is route #7 cross-assembly consumer gate for the src/gen seat'
req 'LEG 3 golib-(Release|Debug) exit=0 ' 2 'LEG 3 GolibTests exit=0 at BOTH configurations'
req 'LEG 3 ASSERT skipDelta=3 \[asserted 3\]' 1 'LEG 3 skip delta == 3 (the GC/pin-liveness class runs at Release and self-skips at Debug, so a Debug-only reading under-measures it by three)'
req 'LEG 3 TOTAL is ADMISSIBLE :: [0-9]+ matches the unset/windows column' 1 'LEG 3 the reported total matches an ADMISSIBLE total computed from the csproj at run time'
req 'LEG 3 DECLARED COUNT RECONCILED ::' 1 'LEG 3 the declared count at the UNION equals the declared count at the BASE plus this train own arms.  ⚠ That is the check a self-consistent count cannot make on its own: the base side is anchored to a NAMED REF, which is exactly how a Total-against-declared check once passed on a run 42 methods short'
# --- the behavioral legs
req "LEG 0 E3' MET ::" 1 "LEG 0 E3' MET (the dial guard green in all four phases, Output 1 compared / 0 failed, the emission KEEPING the alias)"
req 'LEG 4 CNR exit=0 ' 1 'LEG 4 CNR exit=0 under the pairing'
req "LEG 4 \\*\\*E1' MET\\*\\*" 1 "LEG 4 E1' MET -- CHANGED == 0 on BOTH readings under the two-pin pairing"
req 'LEG 5 CONVERTER REBUILD exit=0 ' 1 "LEG 5 rebuilt the converter ITSELF immediately before the suite.  ⚠ Train 45 reported `Transpile pass 687` over 687 SKIPPED transpiles because a restore had stamped every .cs newer than the binary -- FALSE-GREEN ROUTE #2 through the door no binary opens"
req 'LEG 5 TRANSPILE PREDICATE \(BEFORE the suite\)' 1 'LEG 5 the transpile predicate was asserted on the TREE before the suite (zero behavioral .cs newer than the converter)'
req 'LEG 5 TRANSPILE RAN ::' 1 'LEG 5 the suite REWROTE the corpus, so the Transpile phase MEASURED an emission rather than reporting a skip as a pass.  A `Transpile pass N` line is not evidence that N projects were transpiled'
req 'LEG 5 SUITE exit=0 ' 1 'LEG 5 the FULL behavioral suite exit=0'
req 'LEG 5 SET OK :: no project failed any phase' 1 "LEG 5 the failing set is EMPTY, which is E2'"
req 'LEG 5 RECONCILED :: Output pass ' 1 "LEG 5 Output pass == marked and skip == unmarked against the RUNNER's own enumeration and predicate, fail 0, timeout 0, and Transpile/Compile/Target pass == N"
# --- LEG K
req 'LEG K CANARY CONTROLS \(run BEFORE the set is believed, both directions\)' 1 'LEG K the reflect-canary derivation was CONTROLLED both directions (encoding/json IN, go/doc/comment OUT) before its membership was used.  ⚠ A dated top-five has drifted THREE times in this project history, so this train DERIVES the set and carries no list'
req 'LEG K CANARY SET DERIVED ::' 1 'LEG K the canary set was DERIVED from THIS tree roster and THIS toolchain parsed import declarations'
req 'LEG K [a-z/]+ VERDICT :: PASS' 3 'LEG K at least three rows PASS at their roster-derived banked counts -- the derived canaries, plus `sync` (SEAT 3 OWN GATE: its src/core/sync/mutex.cs retarget emits NOTHING, so LEG D and CNR are structurally blind to it and only a RUN can see it), plus the nistec cost canary'
req 'LEG K [a-z/]+ oracle OK :: oracleGoVersion reads \[go version go1\.23\.12 [a-z0-9]+/[a-z0-9]+\]' 3 "LEG K every row's comparison record carries the oracle at release go1.23.12, read with the WHOLE-LINE predicate.  The field is omitempty, so an ABSENT one is UNMEASURED and never a pass"
req 'LEG K row loop :: processed=' 1 'LEG K the row loop stated processed == listed (a child that ate the loop stdin would swallow a row silently)'
# --- the A-assertions and the light gates
req 'A5 TOP-LEVEL [A-Za-z0-9_]+ :: missingFiles=\[ none\]' 2 'A5 BOTH new TOP-LEVEL guard projects are COMPLETE -- source, emission, package_info, golden, solution registration and all four MSTest registrations'
req 'A5 NESTED .+ :: slnxRegistrations=1' 2 "A5 BOTH nested sub-libraries are registered EXACTLY ONCE in go2cs.slnx.  ⚠ ROUTE #3: an unregistered sub-library passes every harness gate and breaks only Visual Studio, and a sub-library's package_info.cs is an INPUT to its parent's transpile"
req 'A3 seat 3 registry AND displacement ::' 1 "A3 seat 3's registry entry AND the corpus body it displaces are BOTH in the delta.  ⚠ A registration split from its footprint is the syscall.Uname silent-subtraction class: it merges clean, compiles nowhere, and no standing gate sees it until somebody builds the flavour it broke"
req 'A2 seat 2 ISlice\.IsNil ::' 1 "A2 seat 2's four halves are in the tree, the GENERATOR template line included -- the half NO standing gate compiles"
req 'A6 pins UNMOVED ::' 1 'A6 the toolchain declaration and the corpus pin are unmoved and no seat touched a pin file'
req 'G1 roster guard exit=0 :: checks-pass line\(s\)=[1-9]' 1 'G1 the roster guard reached its VERDICT, not merely exit 0'
req 'G10d docs CR/LF ::' 1 "G10d the docs line-ending arm ran.  ⚠ IT CONSULTS THE `-text` ATTRIBUTE IN THIS TRAIN: train 45's arm asserted CRLF over docs/ 'under the eol=crlf pin' when docs/ is OUTSIDE that pin, refused a correct seat, and the landing grew an acceptance path for it.  The exemption here is BY ATTRIBUTE, CONDITIONAL on the index and worktree layers agreeing, and its COUNT is stamped beside the mismatch count"
req 'G10c the eight artifact goldens :: differing from their blob at [0-9a-f]+ = 0 ' 1 "G10c none of the eight alias-carrying goldens differs from its committed blob at the base"
req 'G11\(a\) justification CHECKED ::' 1 'G11(a) the golib gate families are owed AND wired'
req 'G11\(b\) justification CHECKED ::' 1 'G11(b) the CONVERTER obligations are owed'
req 'G11\(b\) ALL THREE CONVERTER OBLIGATIONS ARE WIRED IN THIS BATTERY ::' 1 'G11(b) all three converter obligations -- LEG C, LEG R and LEG D -- are WIRED rather than named as a gap'
req 'G11\(b\) src/gen JUSTIFICATION ::' 1 'G11(b) the src/gen half was justified SEPARATELY.  ⚠ ROUTE #7 is a DIFFERENT obligation from the three: a src/gen change is invisible to CNR and to the stdlib solution alike, so the FULL behavioral COMPILE and the go2cs.slnx build are its named gates'
req 'G11\(c\) battery legs WIRED IN THIS SCRIPT :: 9 of 9' 1 'G11(c) all NINE battery launch lines present in the assembly script itself, read from its ABSOLUTE self-path'
req 'G12 golib box-kind census :: DIRECT box kinds found=[0-9]+ .*withNoStorageKindAnswer=0 ' 1 'G12 every direct golib box kind states a StorageKind over a real, non-zero population'
req 'disk preflight :: freeGB=[0-9]+ \(floor 30' 1 'the disk-floor preflight ran BEFORE the first leg (a battery is a disk INPUT)'

# ============================================================================================
# THE REFUSAL SCAN, AND ITS OWN POSITIVE CONTROL.
#   ⚠ A SCAN THAT CANNOT FIRE IS WORSE THAN NO SCAN.  Two of the patterns begin with `-`, so the `--`
#   separators are what make them run at all; the control below plants one line of each shape and
#   requires every pattern to find it.  A refusal scan reading a clean zero over a healthy log and one
#   reading a clean zero because grep never started are the same output.
# ============================================================================================
for bad in "${BADPATS[@]}"; do
  n=$(grep -aE -- "$bad" "$ASM" | grep -avE -- "$RXCL" | grep -ac . | tr -d '\r')
  if [ "${n:-0}" = "0" ]; then stamp "refusal scan clean for '$bad'"
  else stamp "$n line(s) match '$bad' -- read them:"; grep -aE -- "$bad" "$ASM" | grep -avE -- "$RXCL" | head -5 | cut -c1-220 | sed 's/^/    /' | tee -a "$LOG"; FAIL=1; fi
done
stamp "refusal scan :: patterns=${#BADPATS[@]} exclusions in force=[$RXCL] -- a SENTINEL that matches nothing, and NOTHING IN THIS FILE ASSIGNS IT.  ⚠ Train 45's land carried FIVE named acceptance paths and this one carries NONE: every fault they existed for is fixed in the assembly script, and a fresh train starts with zero accepted classes"
SCANCTL=$(mktemp "/tmp/t46-land-scanctl.XXXXXX")
{ echo '[2026-01-01 00:00:00] planted -- ABORT'; echo '[2026-01-01 00:00:00] ABORT disk 1GB < 30'
  echo '[2026-01-01 00:00:00]   ^ ABORT: planted'; echo '[2026-01-01 00:00:00]   ^ G3 REFUSED: planted'
  echo '[2026-01-01 00:00:00] BASE ANCESTRY REFUSED :: planted'; echo '[2026-01-01 00:00:00] === A-ASSERTIONS FAILED ==='
  echo '[2026-01-01 00:00:00] === CHAIN STOPPED at LEG 2 ==='; echo '[2026-01-01 00:00:00] G11(a) JUSTIFICATION FALSE :: planted'
  echo '[2026-01-01 00:00:00] LOCK HELD: planted'; echo '[2026-01-01 00:00:00] WRONG WORKTREE: planted'
  echo '[2026-01-01 00:00:00] LEG K UNMEASURED :: planted'; echo '[2026-01-01 00:00:00]   ^ G5 UNMEASURED: planted'
  echo '[2026-01-01 00:00:00] DOCS-DELTA x :: numstat not numeric -- UNMEASURED'
  echo '[2026-01-01 00:00:00] seat 3 MERGE FAILED for x'; echo '[2026-01-01 00:00:00]   PRESERVE FAILED for x'
  echo '[2026-01-01 00:00:00]   ^ LEG K os :: NO VERDICT LINE -- instrument fault.'
  echo '[2026-01-01 00:00:00]   ^ LEG 5 FINDING: planted'
  echo '[2026-01-01 00:00:00]   ^ A7 REFUSED: planted'; } > "$SCANCTL"
SCANDEAD=0
for bad in "${BADPATS[@]}"; do
  c=$(grep -acE -- "$bad" "$SCANCTL" 2>/dev/null | tr -d '\r')
  case "${c:-0}" in ''|0) SCANDEAD=$((SCANDEAD+1)); stamp "  ^ REFUSAL-SCAN CONTROL: pattern [$bad] found NOTHING on a planted line of its own shape -- that pattern is DEAD" ;; esac
done
rm -f "$SCANCTL"
stamp "refusal-scan positive control :: dead patterns=$SCANDEAD of ${#BADPATS[@]} (must be 0 -- a scan that cannot fire reads exactly like a clean log)"
[ "$SCANDEAD" = "0" ] || FAIL=1

# ============================================================================================
# THE EXIT SCAN.  ⚠ THE STANDING EXCLUSIONS ARE THE BATTERY'S OWN CONTROL STAMPS, and nothing else.
#   Two preflight controls report a non-zero child on a HEALTHY run and both name themselves
#   `CONTROL`: C2's broken-.ps1 arm (want 1) and C5's old-pin arm (want NON-ZERO).  G7b's old-pin
#   refusal is the third by-design non-zero and prints `exits N` rather than `exit=N`, so it is
#   excluded BY NAME and not by luck.
#   ⚠ THERE IS NO BY-DESIGN NON-ZERO **LEG** IN THIS TRAIN and this scan must not grow one: E1', E2',
#   E3' and ED are all green expectations, so a non-zero leg exit here is a FINDING.
# ============================================================================================
EXCL='CONTROL|\(want [1-9]\)|\(want NON-ZERO|G7b .*\(want NON-ZERO'
# === WRAPPER-EXIT-EXCLUSION BEGIN ===
# THE LAUNCH WRAPPER'S OWN TRAILING 'assembly exit=N' LINE.  It is NOT a leg stamp: no leg printed it,
# no leg can be attributed to it, and it carries the SAME FACT the DONE stamp already carries as
# overallFailed=N -- the fact the composition above has just accepted.
#   ⚠ THE CONDITIONS ARE ALL THREE MEASURED, and any other shape REFUSES rather than being absorbed:
#     * the composition ACCEPTED ($OF_OK reads 1).  ⚠ STATED HONESTLY: the OF_OK gate above has
#       already ABORTED a run where that reads 0, so this condition can only read 1 here.  It is READ
#       rather than assumed because the gate's POSITION is not this block's to guarantee -- a later
#       derive that moves either one must not silently turn this into an unconditional exclusion --
#       and it is never ASSIGNED here: this block decides nothing about the record's refusals.
#     * the line is the record's LAST non-empty line.  One anywhere else is not the wrapper's closing
#       echo at all, and this exclusion says nothing about it.
#     * there is EXACTLY ONE of them, counted BOTH as the anchored whole line and as the bare text
#       anywhere -- two means the record was concatenated from two runs, and a record nobody can bound
#       is not a record this landing may read around.
WX_N=$(grep -acE -- "$WRAPEXIT_PAT" "$ASM" || true)
WX_ANY=$(grep -ac 'assembly exit=' "$ASM" || true)
WX_LAST=$(grep -av '^[[:space:]]*$' "$ASM" | tail -1 | tr -d '\r')
printf '%s' "$WX_LAST" | grep -qE -- "$WRAPEXIT_PAT" && WX_ISLAST=1 || WX_ISLAST=0
WX_CODE=$(printf '%s' "$WX_LAST" | grep -oE '[0-9]+$')
stamp "WRAPPER-EXIT :: anchored matches=$WX_N (want exactly 1) :: bare 'assembly exit=' anywhere=$WX_ANY (want exactly 1) :: it IS the record's last non-empty line=$WX_ISLAST (want 1) :: the line reads [$WX_LAST] code=[${WX_CODE:-none}] :: compositionAccepted=$OF_OK"
if [ "$OF_OK" = 1 ] && [ "${WX_N:-0}" = "1" ] && [ "${WX_ANY:-0}" = "1" ] && [ "$WX_ISLAST" = "1" ]; then
  WRAPEXIT_XCL="$WRAPEXIT_PAT"
  stamp "WRAPPER-EXIT EXCLUSION :: IN FORCE -- the launch wrapper's own closing echo is removed from the EXIT scan (and from nothing else).  It is not a leg stamp and it carries the same fact the DONE stamp carries as overallFailed=$OF, which the composition has already accepted"
else
  stamp "WRAPPER-EXIT EXCLUSION :: NOT IN FORCE -- one of its three measured conditions does not hold, so the sentinel stands and the line, if the record carries one, is a finding this landing must answer for.  It is stamped rather than silent, because an exclusion nobody can see is an exclusion nobody re-reads"
fi
# === WRAPPER-EXIT-EXCLUSION END ===
EX=$(grep -aE -- 'exit=[1-9]' "$ASM" | grep -avE -- "$EXCL" | grep -avE -- "$WRAPEXIT_XCL" | grep -ac . | tr -d '\r')
if [ "${EX:-0}" = "0" ]; then stamp "exit scan clean :: no non-zero leg exit outside the by-design control stamps"
else
  stamp "exit scan :: $EX non-zero exit line(s) OUTSIDE the by-design control stamps -- read them:"
  grep -aE -- 'exit=[1-9]' "$ASM" | grep -avE -- "$EXCL" | grep -avE -- "$WRAPEXIT_XCL" | head -10 | cut -c1-220 | sed 's/^/    /' | tee -a "$LOG"
  FAIL=1
fi

# ============================================================================================
# THE TREE ARITHMETIC, POINTED AS THIS TRAIN NEEDS IT.
# ============================================================================================
n=$SEATN
FP=$(git rev-list --first-parent --merges --count $BASE_EXPECT..HEAD)
[ "$FP" = "$n" ] && stamp "first-parent merges=$FP == seats=$n" || { stamp "first-parent merges=$FP != seats=$n -- the head carries a merge no seat accounts for, or a seat did not merge as its own commit"; FAIL=1; }

D_CONV=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/go2cs | grep -c . || true)
D_SLNX=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/go2cs.slnx | grep -c . || true)
D_GOLIB=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/core/golib | grep -c . || true)
D_GEN=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/gen | grep -c . || true)
D_GT=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/tests/GolibTests | grep -c . || true)
D_BEH=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -c . || true)
D_DOCS=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- docs | grep -c . || true)
D_CORE=$(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD -- src/core | grep -c . || true)
D_BEHMOD=$(git -c core.quotepath=false diff --name-only --diff-filter=MD $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\.cs$' | grep -c . || true)
stamp "delta by directory :: src/go2cs=$D_CONV (>=1) src/go2cs.slnx=$D_SLNX (1) src/core/golib=$D_GOLIB (>=1) src/gen=$D_GEN (>=1) src/tests/GolibTests=$D_GT (>=1) src/tests/Behavioral=$D_BEH (>=1) src/core=$D_CORE (>=1) docs=$D_DOCS (>=1) :: behavioral files MODIFIED or DELETED other than the four MSTest classes=$D_BEHMOD (must be 0)"
stamp "⚠ src/gen IS **REQUIRED** IN THIS TRAIN AND THAT INVERTS TRAIN 45's ASSERTION, which required it to be ZERO.  Seat 2 changes a go2cs-gen inherited-type template, which is FALSE-GREEN ROUTE #7's own population: invisible to CNR (transpile-only) and to the stdlib solution (one assembly at a time), and gated only by LEG 2b and LEG 5's COMPILE phase.  A zero here would mean the seat landed its golib half and not its generator half."
{ [ "$D_CONV" -ge 1 ] && [ "$D_SLNX" = "1" ] && [ "$D_GOLIB" -ge 1 ] && [ "$D_GEN" -ge 1 ] && [ "$D_GT" -ge 1 ] && [ "$D_BEH" -ge 1 ] && [ "$D_CORE" -ge 1 ] && [ "$D_DOCS" -ge 1 ]; } || { stamp "  ^ a directory this train's own justification says MUST move did not -- a seat did not land what it was pinned for, and every heavy leg was then measuring nothing new"; FAIL=1; }
[ "$D_BEHMOD" = "0" ] || { stamp "  ^ a committed behavioral file other than the four MSTest classes was MODIFIED or DELETED.  Seats 1 and 2 ADD projects; a CHANGED golden is a RE-BASELINE, and a re-baseline is a ruling rather than a merge.  Files:"; git -c core.quotepath=false diff --name-only --diff-filter=MD $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -avE '^src/tests/Behavioral/BehavioralTests/[^/]+Tests\.cs$' | sed 's/^/    /' | tee -a "$LOG"; FAIL=1; }

# --- THE REGISTRATION ARITHMETIC, READ AT **TWO DEPTHS**.  ⚠ ROUTE #3 IS EXACTLY THIS DISTINCTION.
NB_ALL=$(git -c core.quotepath=false diff --name-only --diff-filter=A $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -acE '^src/tests/Behavioral/.*\.csproj$' || true)
NB_TOP=$(git -c core.quotepath=false diff --name-only --diff-filter=A $BASE_EXPECT..HEAD -- src/tests/Behavioral | grep -aE '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | grep -ac . || true)
SLNXADD=$(git diff $BASE_EXPECT..HEAD -- src/go2cs.slnx | grep -acE '^\+[[:space:]]*<Project ' || true)
stamp "registration arithmetic :: new behavioral .csproj TOP-LEVEL=$NB_TOP ALL DEPTHS=$NB_ALL (nested sub-libraries=$(( NB_ALL - NB_TOP ))) :: <Project> lines ADDED to go2cs.slnx=$SLNXADD"
stamp "⚠ THE SOLUTION COUNT MUST MATCH **ALL DEPTHS**, NOT TOP-LEVEL.  Only the TOP-LEVEL projects owe a golden and four MSTest registrations (UpdateTestTargets is top-level-only); EVERY new .csproj at ANY depth owes a solution entry, and an unregistered sub-library passes every harness gate and breaks the solution in Visual Studio only -- which is exactly how nsshadow slipped through."
[ "$SLNXADD" = "$NB_ALL" ] || { stamp "  ^ $SLNXADD registration(s) for $NB_ALL new project(s) -- a project on disk that no solution entry names, or an entry for a project that is not there"; FAIL=1; }
[ "${NEWB:-x}" = "$NB_TOP" ] || { stamp "  ^ the assembly measured newBehavioralProjects=${NEWB:-?} where the tree reads $NB_TOP top-level.  Two derivations of one number disagree, and the disagreement is the finding"; FAIL=1; }
[ "${NEWBALL:-x}" = "$NB_ALL" ] || { stamp "  ^ the assembly measured newBehavioralProjectsAllDepths=${NEWBALL:-?} where the tree reads $NB_ALL"; FAIL=1; }

# --- and the PIN files, which no seat may touch
D_PIN=$(git diff --name-only $BASE_EXPECT..HEAD -- src/go2cs/go.mod src/go2cs/go.sum src/version.props | grep -c . || true)
stamp "pin files in the delta :: $D_PIN (must be 0 -- every per-leg toolchain decision in the battery was derived for go.mod=1.24.13 and version.props=1.23.12)"
[ "$D_PIN" = "0" ] || FAIL=1
stamp "delta summary :: $(git -c core.quotepath=false diff --numstat $BASE_EXPECT..HEAD | awk '{a+=$1;d+=$2} END{printf "+%d/-%d over %d file(s)", a+0, d+0, NR+0}')"
stamp "delta file list: $(git -c core.quotepath=false diff --name-only $BASE_EXPECT..HEAD | tr '\n' ' ')"

# --- marker scan over the COMMITTED BLOBS, with processed == listed asserted.
MK=0; SC=0; LS=$(git -c core.quotepath=false diff --name-only --diff-filter=d $BASE_EXPECT..HEAD | grep -c . || true)
while IFS= read -r f; do
  [ -n "$f" ] || continue
  SC=$((SC+1))
  c=$(git show "HEAD:$f" < /dev/null 2>/dev/null | grep -acE -- '^(<<<<<<<|=======|>>>>>>>)( |$)' || true)
  MK=$((MK + ${c:-0}))
done < <(git -c core.quotepath=false diff --name-only --diff-filter=d $BASE_EXPECT..HEAD)
stamp "marker scan :: listed=$LS scanned=$SC markerLines=$MK (markers must be 0, and scanned must equal listed or the scan is unreliable)"
[ "$MK" = 0 ] || FAIL=1
[ "$SC" = "$LS" ] || { stamp "  ^ the marker scan processed $SC of $LS files"; FAIL=1; }

# ============================================================================================
# THE LIVE GATE.  Read the remote TWICE around a fetch: a master that moved between the readings is a
# race, and a master that is not the base this train was assembled on is a different tree.
# ============================================================================================
git fetch -q origin master || { stamp "fetch origin master FAILED"; FAIL=1; }
R1=$(git ls-remote origin refs/heads/master | cut -c1-40); R2=$(git rev-parse origin/master); R3=$(git ls-remote origin refs/heads/master | cut -c1-40)
stamp "LIVE :: ls-remote=$(echo $R1|cut -c1-9) fetched=$(echo $R2|cut -c1-9) again=$(echo $R3|cut -c1-9) expected base=$BASE_EXPECT"
{ [ "$R1" = "$R3" ] && [ "$(echo $R1|cut -c1-9)" = "$BASE_EXPECT" ] && [ "$(echo $R2|cut -c1-9)" = "$BASE_EXPECT" ]; } || { stamp "LIVE gate: remote master moved or disagrees -- ABORT"; FAIL=1; }

[ "$FAIL" = 0 ] || { stamp "GATES FAILED -- nothing pushed"; exit 4; }
if [ "${LAND_VERIFY_ONLY:-0}" = 1 ]; then stamp "=== TRAIN 46 LAND VERIFY-ONLY DONE head=$HEAD (nothing pushed) ==="; exit 0; fi

# ============================================================================================
# THE LEASE PUSH
# ============================================================================================
stamp "PUSH TARGET :: HEAD=$HEAD -- that is what the push below sends (HEAD:refs/heads/master) and what the ls-remote verification after it compares against.  It IS the assembled head $HEAD_EXPECT: this file carries no path that can accept a head other than the one the record names"
stamp "LEASE :: git push --force-with-lease=refs/heads/master:$R1 origin HEAD:refs/heads/master"
git push --force-with-lease=refs/heads/master:$R1 origin HEAD:refs/heads/master 2>&1 | tail -2 | tee -a "$LOG"; prc=${PIPESTATUS[0]}
NM=$(git ls-remote origin refs/heads/master | cut -c1-9)
[ "$NM" = "$HEAD" ] || { stamp "push exit=$prc but remote master reads $NM != $HEAD -- NOT LANDED (an exit code does not settle a push; ls-remote does, in both directions)"; exit 5; }
stamp "LANDED :: master = $NM (verified from ls-remote; push exit=$prc)"
stamp "LANDING NOTES :: (1) **THIS LANDING EXCLUDED NOTHING FROM ITS REFUSAL SCAN.**  Train 45's land carried FIVE named acceptance paths -- G10D-TEXTATTR, LEGD-STANDALONE, LEGD-LINUX-LINEFORM, LEG4-DRIFT-REBASELINED and BEHAVIORAL-FIXUP -- each for a real fault measured on that train.  Every one of those faults is FIXED IN THE INSTRUMENT: G10d consults \`git check-attr text\` and exempts a \`-text\` file only when its two layers agree (stamping the exemption COUNT beside the mismatch count), and LEG D is corrected to the standalone's shape with its per-target line comparison done BY MECHANISM.  A fresh train starts with zero accepted classes."
stamp "LANDING NOTES :: (2) LEG 4b IS RETIRED.  It was train 45 seat 1's OWN acceptance -- CNR under the explicit 1.24.13 RUN pin, expecting CHANGED 0 where the pre-fix reading was eight projects -- and that seat LANDED with train 45.  The named Δ-drop DIAGNOSIS in LEG 4 and LEG 0 went with it, because the alias fold makes the emission PIN-INDEPENDENT and that signature can no longer be produced by a failed pairing.  What is LOST is stated at each site: those legs no longer separate an ENVIRONMENT failure from a TREE failure by the drift's shape, and the pairing stamps plus the after-guard are what answer that now."
stamp "LANDING NOTES :: (3) **ROUTE #7 IS LIVE IN THIS TRAIN.**  Seat 2 changes a go2cs-gen inherited-type template, which CNR (transpile-only) and the stdlib solution (one assembly at a time) are both structurally blind to.  LEG 2b's go2cs.slnx build and LEG 5's FULL behavioral COMPILE phase are its named gates, G11(b) REFUSES if either is unwired, and the delta arithmetic above REQUIRES src/gen to move -- which inverts train 45's assertion that it must not."
stamp "LANDING NOTES :: (4) **ROUTE #3 IS LIVE TOO.**  Seat 1's guard carries TWO NESTED SUB-LIBRARIES.  Solution registrations are owed at ALL DEPTHS while goldens and MSTest registrations are owed only at TOP LEVEL, and those are two numbers -- derived separately by the assembly (A5, LEG 1) and re-derived independently here.  A single 'new projects' figure cannot tell them apart, and an unregistered sub-library passes every harness gate and breaks only Visual Studio."
stamp "LANDING NOTES :: (5) SEAT 3's REGISTRY ENTRY AND THE BODY IT DISPLACES LANDED TOGETHER (A3).  A registration split from its corpus footprint is the syscall.Uname silent-subtraction class: both diffs are pure additions and removals, git merges them without a conflict, and the result compiles nowhere -- discovered days later by a lane building the flavour it broke.  Its seat gate is LEG K's banked \`sync\` row, because the hand-own retarget EMITS NOTHING and LEG D and CNR are blind to it by construction."
stamp "LANDING NOTES :: (6) the converter's toolchain stays go1.24.13 and the corpus pin stays 1.23.12 -- A6 asserted both on the assembled tree and the delta carries zero pin files."

# ============================================================================================
# PRUNE, BY ANCESTRY, WITH A FETCH IMMEDIATELY BEFORE THE CHECK.  `origin/<branch>` as last fetched at
# assembly time is the SEATED SHA by construction, so a stale read would delete a branch a lane has
# kept cutting on.  A branch whose tip is NOT an ancestor of the new master is KEPT, named, and left
# for its owner -- and the KEEP list is honoured whatever the ancestry says.
# ⚠ A REFUSED DELETE ANSWERS `Everything up-to-date` WITH EXIT 0, so every deletion is verified from
#   ls-remote afterwards and never from the exit code.
# ⚠ THE LOOP WALKS THE **DERIVED** SEAT LIST, so a PLACEHOLDER row that never merged is not in it and
#   cannot be pruned by accident: a branch nobody merged is a branch nobody may delete.
# ============================================================================================
PR=0
while IFS=':' read -r b c <&3; do
  [ -n "$b" ] || continue
  PR=$((PR+1))
  case " $KEEP_BRANCHES " in *" $b "*) stamp "prune $b :: on the KEEP list -- not touched"; continue ;; esac
  case "$b" in claude/PENDING-*) stamp "prune $b :: a PLACEHOLDER ref -- not touched (and it should not be in the derived seat list at all)"; continue ;; esac
  git fetch -q origin "+refs/heads/$b:refs/remotes/origin/$b" < /dev/null 2>/dev/null
  rt=$(git ls-remote origin "refs/heads/$b" < /dev/null | cut -f1)
  [ -n "$rt" ] || { stamp "prune $b :: absent from the remote already"; continue; }
  if git merge-base --is-ancestor "$rt" HEAD < /dev/null 2>/dev/null; then
    git push -q origin ":refs/heads/$b" < /dev/null 2>/dev/null
    a=$(git ls-remote origin "refs/heads/$b" < /dev/null | cut -f1)
    [ -z "$a" ] && stamp "prune $b @$(echo $rt|cut -c1-9) :: fully merged -> DELETED (ls-remote verified)" \
                || stamp "prune $b @$(echo $rt|cut -c1-9) :: delete REFUSED, still on the remote ($(echo $a|cut -c1-9)) -- a refused delete answers 'Everything up-to-date' with exit 0, so this is read from ls-remote and never from the exit code"
  else
    stamp "prune $b :: remote tip $(echo $rt|cut -c1-9) is NOT an ancestor of master -- KEPT (the branch grew after it was seated; that growth is its owner's, and deleting it would destroy unmerged work)"
  fi
done 3<<< "$SEATS"
stamp "prune loop :: processed=$PR of $SEATN seat rows (a child that ate the loop's stdin would swallow a row silently, so the count is stated)"
[ "$PR" = "$SEATN" ] || { stamp "  ^ the prune loop processed $PR of $SEATN"; }
for k in $KEEP_BRANCHES; do
  kt=$(git ls-remote origin "refs/heads/$k" < /dev/null | cut -f1 | cut -c1-9)
  stamp "KEEP $k :: remote tip ${kt:-absent} -- never pruned by this train, whatever its ancestry reads"
done
stamp "=== TRAIN 46 LAND DONE master=$NM seats=$SEATN ==="
