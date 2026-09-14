#!/bin/bash
# ============================================================================================
# TRAIN 48 REHEARSAL -- SEQUENTIAL REAL MERGES in a THROWAWAY WORKTREE, plus a **PER-SEAT TYPE
# CHECK**, removed at the end.
#
# ⚠ IT RUNS THE REAL COMMAND.  A TEMP-INDEX dry run (`read-tree -m --aggressive` plus `merge-file`)
# answers "would the 3-way CONTENT merge conflict"; it does NOT answer "does `git merge` do what the
# ASSEMBLY script will do", and a pairwise three-way against master cannot see SEAT-VERSUS-SEAT
# collisions at all.  The accumulation is run for real and the accumulation is the real accumulation.
#
# ⚠⚠ **NEW IN TRAIN 47: `go vet ./...` IN src/go2cs AFTER EVERY SEAT MERGE, AND IT NAMES THE FIRST
#   FAILING SEAT.**  Train 46 run 4 died forty minutes into its battery at LEG C with
#   `FAIL go2cs [build failed]`: two edits to ONE `_test.go` -- one seat's new call sites, another
#   train's widened signature -- that git merged CLEAN BY CONTENT, so the package failed to compile
#   ONLY AT THE UNION and no per-seat gate could have seen it.  A clean merge is not a compiling
#   tree; this leg is the cheapest instrument that knows the difference, and finding it HERE costs
#   seconds where finding it in the battery costs a leg.
#   ⚠ CONSEQUENCE, STATED RATHER THAN GLOSSED: **THIS SCRIPT IS NO LONGER "git only".**  `go vet`
#   type-checks the package, which means it reads the Go toolchain and writes into the build cache.
#   It writes nothing into the repository, runs no converter and runs no dotnet, and it still creates
#   its OWN worktree -- but it is CPU, and it must not be run inside a worktree a battery is using.
#   ⚠ IT RUNS UNDER THE **CONVERTER BUILD PIN** (the 1.24.13 root, GOTOOLCHAIN=local), because the
#   converter module declares `go 1.24.13`: under the corpus pin it would refuse for the RIGHT reason
#   and read as a seat defect.  The pin RUNS `go version` and ABORTS on a mismatch -- printing a pin
#   is a decoration, and this repository has already paid for an instrument that printed one and
#   carried on.  A vet leg that could not establish its pin is reported UNMEASURED and is never a
#   pass: an instrument that cannot run has not found nothing, it has found nothing out.
#
# ⚠⚠ **AND THE LICENSING GUARD AFTER EVERY SEAT (2026-09-13, G's SUGGEST e55707277):**
#   `go test -count=1 -run TestLicensing ./...` in src/go2cs, under the same pin, recorded PASS/FAIL
#   PER SEAT with the FINAL (== union) reading printed prominently in the summary.  A GUARD COLLISION
#   IS A PROPERTY OF THE BASE: a seat adds a file whose header the guard refuses, or master moves the
#   rule under a file a seat already carries, and the pair exists only at the accumulation.  `go vet`
#   type-checks and cannot see it -- the guard is a TEST.
#   ⚠ A LICENSING RED IS **REPORTED, NOT FATAL**: it does not move this script's exit code, because
#   the rehearsal's question is "do the seats merge and type-check" and the guard's answer is a
#   finding to route (to the seat's lane, or to the base) rather than a reason to stop accumulating.
#
# ⚠ IT RESOLVES NOTHING, AND IT **SAVES A RESOLUTION SLOT** FOR EACH CONFLICTED PATH.  The
# resolutions are made HERE, where a mistake costs nothing, VERIFIED here, and then applied
# MECHANICALLY at assembly and stamped PRE-RESOLVED.  What this script writes is the SLOT and the
# evidence -- the three versions plus the conflicted working file -- and what it never writes is an
# answer.  A conflict is the coordinator's RULING.
#     $SELFDIR/coord-train48-resolutions/seat<N>/
#         MANIFEST.txt          seat=<N> pin=<sha> branch=<ref> base=<sha> paths=<n>
#         conflicted/<path>     the merged file WITH its markers, exactly as git left it
#         base/<path>  ours/<path>  theirs/<path>     the three stages, for reading
#         files/<path>          ⚠ **EMPTY UNTIL THE COORDINATOR WRITES THE RESOLVED FILE HERE.**
#                               merge_seat applies EVERY path from `files/` or REFUSES; a partial set
#                               is a tree nobody ruled on.
# ⚠ AND IT VERIFIES WHAT IT IS GIVEN.  `--verify` re-reads a saved set and asserts: zero conflict
# markers; the file is non-empty; and -- for a `.go`, `.cs` or `.csproj` -- that BOTH sides' distinct
# top-level symbols survive.  A both-kept resolution proven only by the ABSENCE of markers is how one
# lane split a function in half behind a green `gofmt`, and a symbol COUNT is what caught it.
#
# ⚠ A CLEAN REHEARSAL IS NOT A CLEAN ASSEMBLY, AND THE DIFFERENCE IS PERMANENT.  This script runs
#
# ⚠⚠ **WHAT CHANGED FOR TRAIN 48, AND WHAT DID NOT.**  The mechanism is train 47's, unchanged: the
#   seat table and the base are READ OUT OF THE ASSEMBLY SCRIPT and never retyped, the merges are
#   real, the resolutions are SLOTS the coordinator fills, and `go vet ./...` type-checks the
#   accumulation after every seat.  Three readings moved with the assembly:
#     * the base variable is `EXPECT_BASE` (train 47 spelled it `EXPECT_46`, a name that carried the
#       previous train's NUMBER into the one after it -- which is how a derive inherits a premise);
#     * the containment pin is `T48_CONTAIN_PIN`, and it reads PENDING until train 47 lands.  A
#       rehearsal against a base that does not contain it is STATED, not refused: the rehearsal onto
#       the CURRENT origin/master is exactly what a coordinator wants before the pin is fillable;
#     * the seat table's optional fields are now ORDER-FREE `key=value` pairs (`allowed=`,
#       `stack-on=`), so the row read here puts fields six and beyond into one variable and the
#       rehearsal does not interpret them at all -- the ASSEMBLY validates them, and two instruments
#       parsing one field is how they drift.
# ⚠ THE REHEARSAL SAYS NOTHING ABOUT DUPLICATION.  A cherry-pick that puts one diff on two seats
#   MERGES CLEAN -- git applies the same content twice and the second application is a no-op or a
#   trivial conflict -- so a green rehearsal is not evidence against it.  The PATCH-ID ARM in the
#   assembly is that instrument, and it runs before any merge.
# `git merge` and `go vet`; the assembly ALSO applies each seat's CLASS SHAPE and its FORBIDDEN path
# set, which only the assembly does.  Two instruments, two questions.
#
# ⚠ `set -u` ONLY -- **NOT** `set -o pipefail`.  Under pipefail a `grep -q` that exits on its first
# match SIGPIPEs its producer and a TRUE match reads as a failure; R measured that on this fleet.
# This file carries ZERO `| grep -q` pipes for the same reason.
#
# ⚠ THE SEAT TABLE AND THE BASE ARE **READ OUT OF THE ASSEMBLY SCRIPT**, NEVER RETYPED.  A second
# copy of a seat list is the thing that drifts, and a rehearsal of a different list than the assembly
# will merge is a rehearsal of a train nobody is assembling.  That includes the BASE: train 46's
# rehearsal carried `BASE_EXPECT=<sha>` as a literal beside the assembly's own constant, which is two
# copies of one fact.
#
#   usage:  bash coord-train48-rehearse.sh                 # base = the CURRENT origin/master
#           M=<sha> bash coord-train48-rehearse.sh         # rehearse onto a named master
#           KEEP=1 bash coord-train48-rehearse.sh          # leave the worktree standing for inspection
#           NOVET=1 bash coord-train48-rehearse.sh         # skip the per-seat build+vet, STATED in the summary
#           REHEARSE_WT=<dir> bash coord-train48-rehearse.sh   # the throwaway worktree (default: under .claude)
#           bash coord-train48-rehearse.sh --union-out <path>  # write the union's own facts to a file
#           bash coord-train48-rehearse.sh --verify        # verify the SAVED resolutions, merge nothing
#   exit 0  every seat merged clean AND both `go build ./...` and `go vet ./...` passed at every step
#           (or every saved resolution verified)
#        1  a seat conflicted, a seat's merge broke the type check, or a saved resolution failed
#        2  a precondition refused
# ============================================================================================
set -u
G="${GO2CS_REPO:-/c/Projects/go2cs}"
# ⚠ THE THROWAWAY WORKTREE IS OVERRIDABLE BY NAME.  A rehearsal must never be run inside a tree a
#   battery may be holding, and the coordinator's scratch root is not always under the repository.
WT="${REHEARSE_WT:-$G/.claude/worktrees/coord-t48-rehearse}"
TAG=t48rehearse
# --- the UNION OUT file.  ⚠ A rehearsal's verdict is read by somebody who was not watching it, and a
#     summary that exists only in a terminal is a measurement nobody can quote.  --union-out <path>
#     (or UNION_OUT=<path>) writes the union's own facts -- base, final commit, TREE HASH, per-seat
#     verdicts, the file count and the file list -- as a file.  It is written even when a seat
#     conflicted, carrying the CONFLICT and the UNMEASURED statement, because a partial union is a
#     result and hiding it would make a red rehearsal indistinguishable from one that never ran.
UNION_OUT="${UNION_OUT:-}"
say(){ printf '%s\n' "$*"; }

# --- THE CONVERTER BUILD PIN, DERIVED FROM $HOME AND ASSERTED.  ⚠ No profile path is spelled here:
#     a literal would breach the security convention on any pushed surface and be wrong on every
#     other box.  A DERIVED root that is not there is refused BY NAME rather than left to produce a
#     confusing toolchain error three steps later.
GO_SDK_ROOT="${GO2CS_SDK_ROOT:-$HOME/sdk}"
PIN_GO_1_24_ROOT="$GO_SDK_ROOT/go1.24.13"
VETPIN=0
pin_for_vet(){
  [ -d "$PIN_GO_1_24_ROOT" ] || { say "VET PIN UNAVAILABLE :: [$PIN_GO_1_24_ROOT] does not exist (override with GO2CS_SDK_ROOT)"; return 1; }
  export GOROOT="$PIN_GO_1_24_ROOT" GOTOOLCHAIN=local
  export PATH="$PIN_GO_1_24_ROOT/bin:$PATH"
  gv=$(go version 2>/dev/null | tr -d '\r')
  gr=$(go env GOROOT 2>/dev/null | tr -d '\r')
  case "$gv" in *go1.24.13*) : ;; *) say "VET PIN MISMATCH :: \`go version\` reads [$gv] where go1.24.13 is required -- printing a pin is a decoration, so this REFUSES"; return 1 ;; esac
  say "VET PIN OK :: $gv :: GOROOT=$gr GOTOOLCHAIN=local -- the converter module declares go 1.24.13, so the vet runs under the BUILD pin and not the corpus one"
  return 0
}

# ⚠ RESOLVE THIS SCRIPT'S OWN DIRECTORY **BEFORE** ANY cd, AND ABSOLUTELY.  A ${BASH_SOURCE[0]}
#   evaluated AFTER `cd $G` resolves against the REPOSITORY ROOT, names a file that is not there, and
#   every read off it comes back well-formed and about the wrong path.  Train 45's rehearsal walked
#   into exactly that on its first run.
SELFDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ASM="$SELFDIR/coord-train48-assemble.sh"
RESROOT="$SELFDIR/coord-train48-resolutions"
# --- THE BASE, READ OUT OF THE ASSEMBLY SCRIPT rather than retyped beside it.  ⚠ It may read
#     PENDING: this is a TEMPLATE and the base is one of its three fill points.  A PENDING base
#     is a READING here (the rehearsal onto the CURRENT origin/master is still meaningful and is
#     what a coordinator wants before the seats are pinned), and it is STAMPED as such.
BASE_EXPECT=$(grep -aE "^EXPECT_BASE='" "$ASM" 2>/dev/null | head -1 | sed -E "s/^EXPECT_BASE='([^']*)'.*/\1/")
[ -n "$BASE_EXPECT" ] || BASE_EXPECT=UNREADABLE

# --- the seat table.  ⚠ IT IS READ OUT OF THE ASSEMBLY SCRIPT, NEVER RETYPED.  A second copy of a
#     seat list is the thing that drifts, and a rehearsal of a different list than the assembly will
#     merge is a rehearsal of a train nobody is assembling.
[ -f "$ASM" ] || { say "REFUSED: $ASM not found -- the seat table is read from it, never retyped"; exit 2; }
lo=$(grep -nE '^SEAT_TABLE="1\|' "$ASM" | head -1 | cut -d: -f1)
[ -n "$lo" ] || { say "REFUSED: could not find SEAT_TABLE in $ASM"; exit 2; }
hi=$(awk -v s="$lo" 'NR>=s' "$ASM" | grep -nE '"$' | head -1 | cut -d: -f1)
[ -n "$hi" ] || { say "REFUSED: could not find the end of SEAT_TABLE in $ASM"; exit 2; }
hi=$(( lo + hi - 1 ))
eval "$(sed -n "${lo},${hi}p" "$ASM")"
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
say "REHEARSAL seat table :: read from $(basename "$ASM") lines $lo..$hi, $ROWS row(s)"
# ⚠ THE ROW COUNT IS **DERIVED AND STRUCTURALLY CHECKED**, NEVER COMPARED TO A LITERAL.  Train
#   46's rehearsal carried `[ "$ROWS" = "6" ]`, which is a fact about that derive; what a
#   rehearsal can honestly assert is what a hand-edited table breaks -- contiguous numbers and
#   unique refs -- and the assembly asserts the same thing over the same table.
RNUMS=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $1}')
REXP=$(awk -v n="$ROWS" 'BEGIN{for(i=1;i<=n;i++) print i}')
RDUP=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' '{print $2}' | sort | uniq -d | grep -ac . || true)
say "REHEARSAL table structure :: numbers [$(printf '%s' "$RNUMS" | tr '\n' ' ')] (must be 1..$ROWS contiguous) :: duplicate refs=$RDUP (must be 0)"
{ [ "$(printf '%s\n' "$RNUMS")" = "$(printf '%s\n' "$REXP")" ] && [ "${RDUP:-0}" = "0" ]; } || { say "REFUSED: the seat table is structurally unsound -- the ROW ORDER IS THE MERGE ORDER, so a duplicated or misnumbered row merges in an order nobody ruled on"; exit 2; }

# ============================================================================================
# THE PENDING SWITCH, GOVERNED **HERE AS WELL AS IN THE ASSEMBLY**.  Added 2026-09-13.
# ============================================================================================
# ⚠ ONE SWITCH IS SUPPOSED TO GOVERN BOTH INSTRUMENTS, and until this block existed it governed only
#   one of them.  The assembly refuses three things about TRAIN48_SKIP_PENDING before it merges
#   anything; the rehearsal read `${TRAIN48_SKIP_PENDING:-0}` at the PENDING row and nothing else --
#   so the three shapes the assembly refuses BY NAME were all silently accepted by the rehearsal:
#     (1) an INHERITED UN-PREFIXED `SKIP_PENDING=1`.  It configures NOTHING here (this script reads
#         the prefixed spelling), so the rehearsal would REFUSE at the first PENDING row with a
#         message about the prefixed switch while the operator was looking at the un-prefixed one
#         they had just exported.  Worse, the assembly ABORTS on that same environment: the operator
#         would be reading a rehearsal refusal as a fact about the train.
#     (2) a DECLARED-BUT-EMPTY `TRAIN48_SKIP_PENDING=`.  `${X:-0}` reads an empty override as unset,
#         so a blank authorisation silently becomes "no" -- the same class the assembly refuses.
#     (3) `TRAIN48_REQUIRE_ALL=1` AND `TRAIN48_SKIP_PENDING=1` TOGETHER.  The assembly calls that a
#         CONTRADICTION and refuses rather than picking a winner.  The rehearsal ignored REQUIRE_ALL
#         entirely, so the one environment the assembly will not run is the one the rehearsal would
#         happily rehearse -- a green about a train that cannot be assembled.
#   All three are refusals, never warnings: an authorisation this script cannot read is not an
#   authorisation, and a rehearsal that runs in an environment the assembly refuses is measuring a
#   different train.
if [ "${SKIP_PENDING+x}" = "x" ]; then
  say "REFUSED: SKIP_PENDING is in the ENVIRONMENT reading [${SKIP_PENDING:-<empty>}].  That is NOT this script's switch -- the switch is TRAIN48_SKIP_PENDING -- so an export of it configures NOTHING here AND ABORTS THE ASSEMBLY, which would make this rehearsal's verdict a statement about a run that cannot happen.  Re-launch with TRAIN48_SKIP_PENDING=<0|1>."
  exit 2
fi
if [ "${REQUIRE_ALL+x}" = "x" ]; then
  say "REFUSED: REQUIRE_ALL is in the ENVIRONMENT reading [${REQUIRE_ALL:-<empty>}].  That is NOT the switch -- the switch is TRAIN48_REQUIRE_ALL -- and the assembly ABORTS on this environment, so a rehearsal run under it would describe a train nobody can assemble."
  exit 2
fi
if [ "${TRAIN48_SKIP_PENDING+x}" = "x" ]; then
  if [ -z "${TRAIN48_SKIP_PENDING:-}" ]; then
    say "REFUSED: TRAIN48_SKIP_PENDING is DECLARED but EMPTY.  An empty authorisation is not 'unset' and must not be read as either answer -- name it or do not declare it."
    exit 2
  fi
  case "$TRAIN48_SKIP_PENDING" in
    1) say "TRAIN48_SKIP_PENDING=1 :: a PENDING row is SKIPPED WITH A STAMP and THIS REHEARSAL IS OF A **PARTIAL UNION** -- the skipped row's collisions with every other row are UNMEASURED, which is not the same as clean." ;;
    0) say "TRAIN48_SKIP_PENDING=0 :: stated explicitly; a PENDING row REFUSES by name." ;;
    *) say "REFUSED: TRAIN48_SKIP_PENDING reads [$TRAIN48_SKIP_PENDING], which is neither 0 nor 1 -- an authorisation this script cannot read is not an authorisation."; exit 2 ;;
  esac
else
  say "TRAIN48_SKIP_PENDING is not in the environment :: a PENDING row REFUSES by name (the same default the assembly takes)."
fi
if [ "${TRAIN48_REQUIRE_ALL+x}" = "x" ] && [ -z "${TRAIN48_REQUIRE_ALL:-}" ]; then
  say "REFUSED: TRAIN48_REQUIRE_ALL is DECLARED but EMPTY.  The assembly refuses this environment before it merges anything, so the rehearsal refuses it too -- one switch, both instruments."
  exit 2
fi
if [ "${TRAIN48_REQUIRE_ALL:-0}" = "1" ] && [ "${TRAIN48_SKIP_PENDING:-0}" = "1" ]; then
  say "REFUSED: TRAIN48_REQUIRE_ALL=1 AND TRAIN48_SKIP_PENDING=1 are a CONTRADICTION: one demands every announced row and the other permits a gap.  The assembly refuses this pair rather than ordering them by precedence, and a rehearsal that accepted it would be rehearsing an environment the assembly will not run -- name ONE of them."
  exit 2
fi

# ============================================================================================
# --verify :: re-read the SAVED resolutions and assert them.  Merges nothing, touches no worktree.
# ============================================================================================
# --- the argument scan.  ⚠ `--union-out` takes its value as the NEXT argument and REFUSES an empty
#     one: a flag that silently writes nowhere is a flag that reads as having written.
while [ "$#" -gt 0 ]; do
  case "$1" in
    --union-out) shift; [ -n "${1:-}" ] || { say "REFUSED: --union-out needs a path"; exit 2; }; UNION_OUT="$1"; shift ;;
    --union-out=*) UNION_OUT="${1#--union-out=}"; [ -n "$UNION_OUT" ] || { say "REFUSED: --union-out= was given an empty path"; exit 2; }; shift ;;
    --verify) VERIFY=1; shift ;;
    *) say "REFUSED: unknown argument [$1] -- usage is in this file's header"; exit 2 ;;
  esac
done
if [ "${VERIFY:-0}" = "1" ]; then
  say "=== TRAIN 48 RESOLUTION VERIFY :: $RESROOT ==="
  if [ ! -d "$RESROOT" ]; then say "no resolution set exists at $RESROOT -- nothing to verify (which is the EXPECTED state when the rehearsal found no conflict)"; exit 0; fi
  VBAD=0; VSEATS=0; VFILES=0
  for d in "$RESROOT"/seat*/; do
    [ -d "$d" ] || continue
    VSEATS=$(( VSEATS + 1 ))
    man="$d/MANIFEST.txt"
    if [ ! -f "$man" ]; then say "  seat set $d has NO MANIFEST.txt -- a resolution set that cannot name its seat and its pin is a saved answer to an unknown question"; VBAD=1; continue; fi
    sn=$(grep -aE '^seat=' "$man" | head -1 | cut -d= -f2 | tr -d '\r')
    pin=$(grep -aE '^pin=' "$man" | head -1 | cut -d= -f2 | tr -d '\r')
    npath=$(grep -aE '^paths=' "$man" | head -1 | cut -d= -f2 | tr -d '\r')
    tabpin=$(printf '%s\n' "$SEAT_TABLE" | awk -F'|' -v n="$sn" '$1==n{print $3}')
    say "  seat $sn :: manifest pin=[$pin] :: the seat table's pin for that row is [$tabpin] :: paths=$npath"
    [ "$pin" = "$tabpin" ] || { say "    ^ FAIL: the saved set was made against a DIFFERENT pin.  A resolution carried over from an earlier derive answers a different question, and a SHA is the only thing that can tell the two apart."; VBAD=1; }
    have=0
    while IFS= read -r p; do
      [ -n "$p" ] || continue
      f="$d/files/$p"
      if [ ! -f "$f" ]; then say "    UNRESOLVED: $p (no file at $f -- the coordinator has not written this one yet)"; VBAD=1; continue; fi
      have=$(( have + 1 )); VFILES=$(( VFILES + 1 ))
      mk=$(grep -acE '^(<<<<<<<|=======|>>>>>>>)( |$)' "$f" || true)
      ln=$(grep -ac . "$f" || true)
      # ⚠ A BOTH-KEPT RESOLUTION IS PROVEN BY A **SYMBOL COUNT**, NEVER BY THE ABSENCE OF MARKERS.
      #   A failed three-way apply once left a clean file MISSING one side's function entirely, behind
      #   a green `gofmt`; what caught it was counting the symbols both sides own.
      case "$p" in
        *.go)    sym=$(grep -acE '^func [A-Za-z0-9_(]' "$f" || true); symk='func' ;;
        *.cs)    sym=$(grep -acE '^[[:space:]]*(public|internal|private|protected|static|partial)[[:space:]].*[({]' "$f" || true); symk='member' ;;
        *.csproj|*.slnx) sym=$(grep -acE '<(Project|Compile|PackageReference|ProjectReference)' "$f" || true); symk='element' ;;
        *)       sym='n/a'; symk='none' ;;
      esac
      say "    RESOLVED: $p :: lines=$ln conflictMarkers=$mk (must be 0) ${symk}Count=$sym"
      [ "$mk" = "0" ] || { say "      ^ FAIL: the saved resolution still carries conflict markers"; VBAD=1; }
      [ "${ln:-0}" -ge 1 ] || { say "      ^ FAIL: the saved resolution is EMPTY"; VBAD=1; }
    done < "$d/PATHS.txt"
    say "  seat $sn :: resolved $have of $npath path(s)"
    [ "$have" = "$npath" ] || { say "    ^ FAIL: the set is INCOMPLETE.  merge_seat applies EVERY path or refuses; a partial set is a tree nobody ruled on."; VBAD=1; }
  done
  say "=== VERIFY DONE seats=$VSEATS files=$VFILES bad=$VBAD ==="
  say "⚠ WHAT THIS VERIFY DOES **NOT** SAY: that the resolution is CORRECT.  It says there are no markers, the file is not empty, the set is complete and each side's symbols are COUNTED and PRINTED.  Reading those counts against what the two sides own is the coordinator's, and it is the step no script can take."
  exit $(( VBAD ? 1 : 0 ))
fi

cd "$G" || { say "REFUSED: cannot cd $G"; exit 2; }

# --- the base.  A rehearsal against a stale base is the stale-base illusion wearing a green, so the
#     remote is FETCHED and the resolved SHA is PRINTED before anything is merged -- and it is
#     ASSERTED against the base this train was derived for.
git fetch -q origin master || { say "REFUSED: git fetch origin master failed"; exit 2; }
M="${M:-$(git rev-parse origin/master)}"
git cat-file -e "${M}^{commit}" 2>/dev/null || { say "REFUSED: M=$M does not resolve to a commit here"; exit 2; }
MSHORT=$(git rev-parse --short=9 "$M")
say "REHEARSAL base :: $MSHORT  (origin/master reads $(git ls-remote origin refs/heads/master | cut -c1-9) right now; this train was DERIVED against $BASE_EXPECT)"
case "$BASE_EXPECT" in
  origin/master)
    # ⚠ THE ASSEMBLY DECLARES ITS BASE AS A **REVSPEC** IN THIS TRAIN, not as a SHA, so there is no
    #   derive-time SHA to compare against and the honest statement is the CONTAINMENT one.  The pin is
    #   READ OUT OF THE ASSEMBLY SCRIPT, never retyped beside it.
    CPIN=$(grep -aE "^T48_CONTAIN_PIN='" "$ASM" 2>/dev/null | head -1 | sed -E "s/^T48_CONTAIN_PIN='([^']*)'.*/\1/")
    if [ -z "${CPIN:-}" ]; then
      say "⚠ NOTE: the assembly declares base=origin/master but no T48_CONTAIN_PIN pin could be read out of it, so this rehearsal's base is UNCHECKED against anything."
    elif git merge-base --is-ancestor "$CPIN" "$M" 2>/dev/null; then
      say "REHEARSAL base containment :: $MSHORT CONTAINS $CPIN (the assembly's own containment pin, read out of it rather than retyped)"
    else
      say "⚠ NOTE: the rehearsal base $MSHORT does NOT contain the assembly's containment pin $CPIN.  The assembly would REFUSE this base, so every verdict below describes a train the assembly will not build."
    fi ;;
  "$MSHORT") : ;;
  *) say "⚠ NOTE: the rehearsal base $MSHORT is NOT the derive-time base $BASE_EXPECT.  That is legal with M=<sha>, and it means every verdict below describes a train nobody has assembled -- state it wherever these readings are quoted." ;;
esac

# --- fetch every non-PENDING seat by exact ref and verify it resolves.  An unresolved SHA and an
#     invented one read the same, so this is a refusal and never a "no".
NEED=0; HAVE=0; PEND=0
while IFS='|' read -r n b s c m a; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then PEND=$(( PEND + 1 )); continue; fi
  NEED=$(( NEED + 1 ))
  git fetch -q origin "+refs/heads/$b:refs/remotes/origin/$b" 2>/dev/null
  if git cat-file -e "${s}^{commit}" 2>/dev/null; then HAVE=$(( HAVE + 1 )); else say "REFUSED: seat $n's SHA $s ($b) does not resolve here even after the fetch"; exit 2; fi
done <<< "$SEAT_TABLE"
say "REHEARSAL refs :: $HAVE of $NEED filled seat SHAs resolve; PENDING rows=$PEND (they are SKIPPED WITH A STAMP below, never silently -- NONE are expected after seat fill)"

# --- the throwaway worktree.  ⚠ REFUSE rather than reuse: a leftover from an earlier run would make
#     the accumulation start from a tree nobody named.
if [ -e "$WT" ]; then
  say "REFUSED: $WT already exists.  Remove it first:  git -C $G worktree remove --force $WT && git -C $G worktree prune"
  exit 2
fi
git worktree add -q --detach "$WT" "$M" || { say "REFUSED: git worktree add failed"; exit 2; }
cleanup(){
  if [ "${KEEP:-0}" = "1" ]; then
    say "KEEP=1 :: the rehearsal worktree is LEFT STANDING at $WT -- remove it with: git -C $G worktree remove --force $WT && git -C $G worktree prune"
  else
    git -C "$G" worktree remove --force "$WT" >/dev/null 2>&1
    git -C "$G" worktree prune >/dev/null 2>&1
    if [ -e "$WT" ]; then say "⚠ the rehearsal worktree at $WT could NOT be removed -- remove it by hand"; else say "rehearsal worktree removed"; fi
  fi
}
trap cleanup EXIT
cd "$WT" || { say "REFUSED: cannot cd $WT"; exit 2; }
say "REHEARSAL worktree :: $(git rev-parse --show-toplevel) at $(git rev-parse --short HEAD) (detached; no branch is created and none is written)"

CONFLICTS=0; MERGED=0; SKIPPED=0; SKIPNAMES=""; VETFAIL=0; VETUNMEASURED=0; VETFIRST=""; NEEDSHAND=""
# --- the LICENSING GUARD's own counters.  ⚠ SEPARATE FROM THE VET COUNTERS ON PURPOSE: a licensing
#     red is REPORTED and does not move this script's exit code, so it must not be summed into a
#     number that does.  LICLAST/LICLASTFILES carry the LAST seat's reading, which is the UNION's.
LICFAIL=0; LICPASS=0; LICFIRST=""; LICLAST="UNMEASURED"; LICLASTFILES=""; LICLASTTESTS=""
if [ "${NOVET:-0}" = "1" ]; then say "⚠ NOVET=1 :: the per-seat type check is OFF for this run, and the summary says so.  A rehearsal without it answers only 'do the seats MERGE'."; else pin_for_vet && VETPIN=1; fi
while IFS='|' read -r n b s c m a <&3; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then
    # ⚠ THE SAME OPT-IN THE ASSEMBLY USES, and for the same reason: with a FILLED table, a rehearsal
    #   that quietly leaves rows out rehearses a train nobody announced, and its CLEAN is then a
    #   statement about a different union.  One switch, two instruments.
    if [ "${TRAIN48_SKIP_PENDING:-0}" != "1" ]; then
      say "REFUSED: seat $n ($c) $b reads PENDING and TRAIN48_SKIP_PENDING is not 1.  Rehearsing a subset of a FILLED table produces a CLEAN verdict about a union nobody announced -- opt into the partial rehearsal BY NAME, or fill the row."
      exit 2
    fi
    SKIPPED=$(( SKIPPED + 1 )); SKIPNAMES="$SKIPNAMES $n($b)"
    say "seat $n ($c) $b :: **SKIPPED -- PENDING** (TRAIN48_SKIP_PENDING=1).  ⚠ This rehearsal is of a PARTIAL union: every verdict below describes the rows that merged, and the skipped row's collisions with all of them are UNMEASURED."
    continue
  fi
  short=$(git rev-parse --short "$s")
  git merge --no-ff --no-edit "$s" -m "rehearsal seat $n $b ($short)" > "/tmp/${TAG}-merge-$n.log" 2>&1
  mrc=$?
  if [ "$mrc" = "0" ]; then
    MERGED=$(( MERGED + 1 ))
    say "seat $n ($c) $b @$short :: CLEAN  (tree -> $(git rev-parse --short 'HEAD^{tree}'), files in the accumulation $(git -c core.quotepath=false diff --name-only "$M" HEAD | grep -c .))"
    # --- ⚠ THE PER-SEAT TYPE CHECK.  A CLEAN MERGE IS NOT A COMPILING TREE: two edits to one file
    #     that git merges by CONTENT can leave a package that compiles on neither side's terms, and
    #     the only instrument that sees it is one that type-checks the ACCUMULATION.  The FIRST seat
    #     whose accumulation fails is the one named, because every later failure is downstream of it.
    if [ "${NOVET:-0}" = "1" ]; then
      say "      vet SKIPPED (NOVET=1) -- ⚠ STATED: this rehearsal says the seats MERGE and says NOTHING about whether the union COMPILES"
      say "      licensing guard SKIPPED with it (NOVET=1) -- it is a go test and needs the same pin"
    elif [ "$VETPIN" != "1" ]; then
      say "      vet UNMEASURED -- the converter build pin could not be established, and an instrument that cannot run has not found nothing"
      say "      licensing guard UNMEASURED for the same reason -- an UNMEASURED guard is not a PASS"
      VETUNMEASURED=$(( VETUNMEASURED + 1 ))
    else
      # ⚠ **BOTH `go build` AND `go vet`, AND THEY ANSWER DIFFERENT QUESTIONS.**  `go vet ./...` type-
      #   checks every package INCLUDING the `_test.go` files, which is where the union-only failure
      #   that killed train 46 run 4 lived; `go build ./...` compiles the NON-test package and is the
      #   one that says the CONVERTER ITSELF still builds -- LEG D and LEG R both `go build` the
      #   converter before they measure anything, so a build that breaks here breaks them at the
      #   forty-minute mark instead of at the second.  Neither subsumes the other: vet's type check is
      #   not a compile, and build never sees a _test.go at all.
      ( cd "$WT/src/go2cs" && go build ./... > "/tmp/${TAG}-build-$n.log" 2>&1 ); brc=$?
      if [ "$brc" = "0" ]; then
        say "      go build ./... in src/go2cs :: exit=0 -- the converter itself still COMPILES through this seat"
      else
        VETFAIL=$(( VETFAIL + 1 ))
        [ -n "$VETFIRST" ] || VETFIRST="seat $n ($c) $b @$short [go build]"
        say "      ** go build ./... in src/go2cs :: exit=$brc -- THE CONVERTER DOES NOT COMPILE THROUGH THIS SEAT **"
        head -12 "/tmp/${TAG}-build-$n.log" | sed 's/^/          /'
      fi
      # ⚠ **AND THE LICENSING GUARD, PER SEAT (G's SUGGEST e55707277, 2026-09-13).**  A guard
      #   COLLISION IS A PROPERTY OF THE BASE, NOT OF THE SEAT THAT TRIPS IT: a seat adds a file whose
      #   header the guard refuses, or master moves the header rule under a file a seat already
      #   carries, and either way the pair only exists at the ACCUMULATION.  `go vet` type-checks and
      #   cannot see it -- the guard is a TEST, and a test that is never run is a gate nobody has.
      #   ⚠ A FAILURE HERE IS **REPORTED, NOT FATAL** to the rehearsal: the rehearsal's exit code
      #   answers "do the seats merge and type-check", and a licensing red is a finding for the
      #   coordinator to route to the seat (or to the lane that owes the header fix) rather than a
      #   reason to stop accumulating.  The FINAL seat's reading is the UNION's, and it is printed
      #   again, prominently, in the summary -- with the FILES THE GUARD NAMES, because "the licensing
      #   guard failed" and "the licensing guard failed on THESE files" are different findings.
      ( cd "$WT/src/go2cs" && go test -count=1 -run TestLicensing ./... > "/tmp/${TAG}-lic-$n.log" 2>&1 ); lrc=$?
      # ⚠ THE NAMES ARE READ OUT OF THE **FAILURE LINES ONLY** (`<file>.go:<line>: <message>`), with the
      #   reporting file's own name stripped off as part of that prefix.  A whole-log grep for
      #   file-shaped tokens also reads the WARNING lines the PASSING licensing tests print about their
      #   own temp fixtures, and truncates `.csproj` to `.cs` on the way -- five invented names beside
      #   the one real one, which is a finding nobody can route.  Measured on this train's own union.
      licdetail=$(grep -aE '^[[:space:]]*[A-Za-z0-9_./-]+\.go:[0-9]+:' "/tmp/${TAG}-lic-$n.log" | sed -E 's/^[[:space:]]*[A-Za-z0-9_./-]+\.go:[0-9]+:[[:space:]]*//')
      licfiles=$(printf '%s\n' "$licdetail" | grep -aoE '[A-Za-z0-9_./-]+\.(go|cs|csproj|ps1|json|md)\b' | sort -u | tr '\n' ' ')
      lictests=$(grep -aoE '^--- FAIL: [A-Za-z0-9_]+' "/tmp/${TAG}-lic-$n.log" | sed 's/^--- FAIL: //' | sort -u | tr '\n' ' ')
      if [ "$lrc" = "0" ]; then
        LICPASS=$(( LICPASS + 1 )); LICLAST="PASS"; LICLASTFILES=""; LICLASTTESTS=""
        say "      go test -run TestLicensing ./... in src/go2cs :: exit=0 -- the accumulation THROUGH THIS SEAT passes the licensing guard"
      else
        LICFAIL=$(( LICFAIL + 1 )); LICLAST="FAIL"; LICLASTFILES="$licfiles"; LICLASTTESTS="$lictests"
        [ -n "$LICFIRST" ] || LICFIRST="seat $n ($c) $b @$short"
        say "      ** go test -run TestLicensing ./... in src/go2cs :: exit=$lrc -- THE LICENSING GUARD FAILS THROUGH THIS SEAT ** (REPORTED, not fatal to this rehearsal)"
        say "         failing test(s) :: [${lictests:-none parsed}] :: file(s) the guard names :: [${licfiles:-none parsed -- read the log}]"
        grep -aE '^(---|\s+---)? *(FAIL|--- FAIL|.*licensing)' "/tmp/${TAG}-lic-$n.log" | head -8 | sed 's/^/          /'
      fi
      ( cd "$WT/src/go2cs" && go vet ./... > "/tmp/${TAG}-vet-$n.log" 2>&1 ); vrc=$?
      if [ "$vrc" = "0" ]; then
        say "      go vet ./... in src/go2cs :: exit=0 -- the accumulation THROUGH THIS SEAT type-checks (tests included)"
      else
        VETFAIL=$(( VETFAIL + 1 ))
        [ -n "$VETFIRST" ] || VETFIRST="seat $n ($c) $b @$short"
        say "      ** go vet ./... in src/go2cs :: exit=$vrc -- THE ACCUMULATION THROUGH THIS SEAT DOES NOT TYPE-CHECK **"
        say "      ⚠ THIS IS A **UNION-ONLY** FAILURE IF THE SEAT IS GREEN ALONE, which is the class that once"
        say "        died forty minutes into a battery at LEG C.  Its remedy is an ASSEMBLY commit (the union"
        say "        fix), not an edit to a seated branch: neither side is wrong on its own."
        head -12 "/tmp/${TAG}-vet-$n.log" | sed 's/^/          /'
      fi
    fi
  else
    UNM=$(git diff --name-only --diff-filter=U | grep -c . || true)
    CONFLICTS=$(( CONFLICTS + 1 ))
    say "seat $n ($c) $b @$short :: ** MERGE FAILED ** exit=$mrc  unmergedPaths=$UNM"
    if [ "${UNM:-0}" = "0" ]; then
      say "      ⚠ ZERO unmerged paths -- so it was NOT a conflict.  A failure branch prints what it OBSERVED, never what it assumes; read the merge log:"
      tail -8 "/tmp/${TAG}-merge-$n.log" | sed 's/^/        /'
    else
      say "      unmerged paths, BY NAME (this rehearsal RESOLVES NOTHING -- the ruling is the coordinator's):"
      git diff --name-only --diff-filter=U | sed 's/^/        /'
      # --- SAVE THE RESOLUTION SLOT.  The three stages plus the conflicted file, and an EMPTY
      #     files/ the coordinator fills.  ⚠ Nothing here decides anything.
      d="$RESROOT/seat$n"
      rm -rf "$d"; mkdir -p "$d/conflicted" "$d/base" "$d/ours" "$d/theirs" "$d/files"
      : > "$d/PATHS.txt"
      git diff --name-only --diff-filter=U | while IFS= read -r p; do
        mkdir -p "$d/conflicted/$(dirname "$p")" "$d/base/$(dirname "$p")" "$d/ours/$(dirname "$p")" "$d/theirs/$(dirname "$p")"
        cp "$p" "$d/conflicted/$p" 2>/dev/null
        git show ":1:$p" > "$d/base/$p"   2>/dev/null
        git show ":2:$p" > "$d/ours/$p"   2>/dev/null
        git show ":3:$p" > "$d/theirs/$p" 2>/dev/null
        printf '%s\n' "$p" >> "$d/PATHS.txt"
      done
      NP=$(grep -ac . "$d/PATHS.txt" || true)
      { printf 'seat=%s\n' "$n"; printf 'pin=%s\n' "$s"; printf 'branch=%s\n' "$b"; printf 'base=%s\n' "$MSHORT"
        printf 'paths=%s\n' "$NP"; printf 'saved=%s\n' "$(date '+%F %T')"; } > "$d/MANIFEST.txt"
      say "      RESOLUTION SLOT SAVED :: $d ($NP path(s))"
      say "        conflicted/  the merged file WITH markers, as git left it"
      say "        base/ ours/ theirs/   the three stages, for reading"
      say "        files/       ⚠ EMPTY -- write the RESOLVED file for EACH path here, then run:"
      say "                        bash $(basename "$0") --verify"
      say "      ⚠ merge_seat applies EVERY path from files/ or REFUSES.  A partial set is a tree"
      say "        nobody ruled on, and the MANIFEST's pin is what stops an old set answering a new question."
      say "      conflict hunks per path:"
      git diff --name-only --diff-filter=U | while IFS= read -r p; do
        printf '        %s :: %s hunk(s)\n' "$p" "$(grep -ac '^<<<<<<<' "$p" 2>/dev/null || echo '?')"
      done
    fi
    git merge --abort 2>/dev/null || git reset --hard -q HEAD
    NEEDSHAND="$NEEDSHAND $n($b)"
    say "      ** NEEDS A HAND :: seat $n ($c) $b @$short **  The merge was ABORTED and the accumulation continues from the PREVIOUS seat."
    say "      ⚠ AND EVERY COLLISION THIS SEAT WOULD HAVE HAD WITH A **LATER** SEAT IS NOW **UNMEASURED**."
    say "        It is not clean and it is not absent: this rehearsal simply never put the two trees"
    say "        together, so a later seat reporting CLEAN below says nothing about its relationship"
    say "        with this one.  Resolve this seat (files/ + --verify) and RE-RUN before reading any"
    say "        later verdict as a statement about the announced train."
  fi
done 3<<< "$SEAT_TABLE"

say ""
say "=== REHEARSAL SUMMARY ==="
say "base                 :: $MSHORT"
say "final commit         :: $(git rev-parse --short HEAD)"
say "final TREE           :: $(git rev-parse 'HEAD^{tree}')"
say "seats merged         :: $MERGED"
say "seats skipped PENDING:: $SKIPPED [$SKIPNAMES]"
say "seats conflicted     :: $CONFLICTS"
say "delta                :: $(git -c core.quotepath=false diff --numstat "$M" HEAD | awk '{a+=$1;d+=$2} END{printf "+%d/-%d over %d file(s)", a+0, d+0, NR+0}')"
# --- the two numbers that answer DIFFERENT questions.  ⚠ ROUTE #3: a new .csproj at ANY depth owes a
#     solution registration, while only a TOP-LEVEL project owes a golden and four MSTest entries.
NB_ALL=$(git -c core.quotepath=false diff --name-only --diff-filter=A "$M" HEAD -- src/tests/Behavioral | grep -acE '^src/tests/Behavioral/.*\.csproj$' || true)
NB_TOP=$(git -c core.quotepath=false diff --name-only --diff-filter=A "$M" HEAD -- src/tests/Behavioral | grep -aE '^src/tests/Behavioral/[^/]+/[^/]+\.csproj$' | grep -ac . || true)
SLNXADD=$(git diff "$M" HEAD -- src/go2cs.slnx | grep -acE '^\+[[:space:]]*<Project ' || true)
say "new behavioral .csproj :: TOP-LEVEL=$NB_TOP  ALL DEPTHS=$NB_ALL  (nested sub-libraries = $(( NB_ALL - NB_TOP )))"
say "go2cs.slnx <Project> lines ADDED :: $SLNXADD -- ⚠ this must equal ALL DEPTHS ($NB_ALL), not TOP-LEVEL: an unregistered sub-library passes EVERY harness gate and breaks only Visual Studio"
[ "$SLNXADD" = "$NB_ALL" ] && say "  ^ registrations MATCH the new projects at all depths" || say "  ^ ⚠ MISMATCH -- $SLNXADD registration(s) for $NB_ALL new project(s).  The assembly's LEG 1 derives this too; a disagreement here is the cheaper place to find it."
# --- WHAT THE ACCUMULATION MOVED, DERIVED FROM THE DELTA RATHER THAN NAMED.  Train 46's rehearsal
#     printed three hand-written numstat lines naming that train's own files, which is the
#     stale-figure class: the moment a seat is re-pinned they describe nothing.  What a template can
#     print is the delta's own shape, grouped by the directories every class in the table can reach.
for d in src/go2cs src/go2cs.slnx src/gen src/core/golib src/core src/tests/GolibTests src/tests/Behavioral docs CLAUDE.md; do
  nd=$(git -c core.quotepath=false diff --name-only "$M" HEAD -- "$d" | grep -ac . || true)
  ad=$(git -c core.quotepath=false diff --numstat "$M" HEAD -- "$d" | awk '{a+=$1;r+=$2} END{printf "+%d/-%d", a+0, r+0}')
  say "delta $d :: files=$nd $ad"
done
# --- and the DISPLACEMENT SHAPE, asked of the delta rather than of a named file.  A converter
#     REGISTRY entry whose corpus footprint did not land is the syscall.Uname silent-subtraction
#     class: both diffs are pure additions and removals, git merges them without a conflict, and the
#     result compiles nowhere.  The DELETION COLUMN is what tells the two apart, so any corpus file
#     the accumulation SHRINKS is printed by name -- a reader can then check it against the seat that
#     claims to displace it.
say "corpus files whose emission SHRANK in this accumulation (a displacement's own signature; read them against the seat that claims it):"
git -c core.quotepath=false diff --numstat "$M" HEAD -- src/core | awk '$2+0 > $1+0 {printf "      %s  +%s/-%s\n", $3, $1, $2}'
MK=$(git -c core.quotepath=false grep -c '^<<<<<<<' HEAD -- . 2>/dev/null | wc -l)
say "conflict markers in the committed tree :: $MK file(s) carry one (must be 0)"
[ "$MK" = "0" ] || { say "  ^ files:"; git -c core.quotepath=false grep -l '^<<<<<<<' HEAD -- . 2>/dev/null | sed 's/^/      /'; }
say "delta file list:"
git -c core.quotepath=false diff --name-only "$M" HEAD | sed 's/^/      /'
say ""
say "⚠ WHAT A CLEAN REHEARSAL DOES AND DOES NOT SAY.  It says the seats MERGE, in this order, onto"
say "  this base, and -- unless NOVET=1 -- that the accumulation TYPE-CHECKS through every seat.  It"
say "  says NOTHING about whether the seats are CORRECT together (that is the assembly's A-assertions"
say "  and its battery) and NOTHING about the CLASS SHAPES, which the assembly applies and this script"
say "  does not.  It also says nothing about any seat whose change EMITS NOTHING -- a hand-own"
say "  retarget, a manifest entry -- for which the instrument is the banked row's own sweep."
say "per-seat type check :: failures=$VETFAIL unmeasured=$VETUNMEASURED first failing seat=[${VETFIRST:-none}] (NOVET=${NOVET:-0})"
say "per-seat LICENSING GUARD :: pass=$LICPASS fail=$LICFAIL first failing seat=[${LICFIRST:-none}] (NOVET=${NOVET:-0}) -- REPORTED, and it does NOT move this script's exit code"
say ""
say "*** UNION LICENSING READING :: $LICLAST ***  (the LAST seat merged is the union, so this line is the"
say "    accumulation's own verdict and not a per-seat one.)"
if [ "$LICLAST" = "FAIL" ]; then
  say "    failing test(s) AT THE UNION :: [${LICLASTTESTS:-none parsed}]"
  say "    file(s) the guard names AT THE UNION :: [${LICLASTFILES:-none parsed -- read /tmp/${TAG}-lic-*.log}]"
  say "    ⚠ A GUARD COLLISION IS A PROPERTY OF THE BASE.  Route it by the NAMES above: a file a seat"
  say "      ADDS is that seat's (or its lane's) header to fix, and a file already at master is the"
  say "      base's.  Neither is resolved by re-running this rehearsal."
fi
say "seats NEEDING A HAND :: [${NEEDSHAND:- none}]"
if [ -n "$NEEDSHAND" ]; then
  say "⚠ EVERY COLLISION BETWEEN A SEAT ABOVE AND A **LATER** SEAT IS UNMEASURED IN THIS RUN.  An"
  say "  aborted seat was never in the accumulation, so no later seat was ever put against it; a later"
  say "  CLEAN is a statement about the tree WITHOUT it and nothing more."
fi
TOTFILES=$(git -c core.quotepath=false diff --name-only "$M" HEAD | grep -c . || true)
if [ -n "$UNION_OUT" ]; then
  { printf 'train=47\n'
    printf 'base=%s\n' "$MSHORT"
    printf 'finalCommit=%s\n' "$(git rev-parse --short HEAD)"
    printf 'unionTree=%s\n' "$(git rev-parse 'HEAD^{tree}')"
    printf 'seatsListed=%s\n' "$ROWS"
    printf 'seatsMerged=%s\n' "$MERGED"
    printf 'seatsSkippedPending=%s\n' "$SKIPPED"
    printf 'seatsConflicted=%s\n' "$CONFLICTS"
    printf 'needsAHand=%s\n' "${NEEDSHAND:-none}"
    printf 'vetFailures=%s\n' "$VETFAIL"
    printf 'vetUnmeasured=%s\n' "$VETUNMEASURED"
    printf 'firstFailingSeat=%s\n' "${VETFIRST:-none}"
    printf 'licensingUnion=%s\n' "$LICLAST"
    printf 'licensingPass=%s\n' "$LICPASS"
    printf 'licensingFail=%s\n' "$LICFAIL"
    printf 'licensingFirstFailingSeat=%s\n' "${LICFIRST:-none}"
    printf 'licensingFilesAtUnion=%s\n' "${LICLASTFILES:-none}"
    printf 'licensingTestsAtUnion=%s\n' "${LICLASTTESTS:-none}"
    printf 'totalFiles=%s\n' "$TOTFILES"
    printf 'numstat=%s\n' "$(git -c core.quotepath=false diff --numstat "$M" HEAD | awk '{a+=$1;d+=$2} END{printf "+%d/-%d", a+0, d+0}')"
    printf 'conflictMarkerFiles=%s\n' "$MK"
    printf '# --- the union file list follows, one per line ---\n'
    git -c core.quotepath=false diff --name-only "$M" HEAD; } > "$UNION_OUT"
  say "union written :: $UNION_OUT ($(grep -ac . "$UNION_OUT" || true) line(s)) -- ⚠ it carries the CONFLICT and UNMEASURED state too, because a partial union is a result and a file that appeared only on a green run would make a red rehearsal indistinguishable from one that never ran"
fi
RC=0
[ "$CONFLICTS" = "0" ] || RC=1
[ "$MK" = "0" ] || RC=1
[ "$VETFAIL" = "0" ] || RC=1
[ "$VETUNMEASURED" = "0" ] || RC=1
say "=== REHEARSAL DONE conflicts=$CONFLICTS markers=$MK vetFailures=$VETFAIL vetUnmeasured=$VETUNMEASURED exit=$RC ==="
exit $RC
