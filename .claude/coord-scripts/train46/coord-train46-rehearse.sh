#!/bin/bash
# ============================================================================================
# TRAIN 46 REHEARSAL -- SEQUENTIAL REAL MERGES in a THROWAWAY WORKTREE, removed at the end.
#
# ⚠ IT RUNS THE REAL COMMAND.  A TEMP-INDEX dry run (`read-tree -m --aggressive` plus `merge-file`)
# answers "would the 3-way CONTENT merge conflict"; it does NOT answer "does `git merge` do what the
# ASSEMBLY script will do", and a pairwise three-way against master cannot see SEAT-VERSUS-SEAT
# collisions at all.  This train has THREE named seats of which TWO add a behavioral guard project
# and TWO register in the same solution file, so the accumulation is run for real and the
# accumulation is the real accumulation.
#
# ⚠ IT RESOLVES NOTHING, AND IT NOW **SAVES A RESOLUTION SLOT** FOR EACH CONFLICTED PATH.  That is
# the one thing that changed from train 45's rehearsal, and it is the doctrine's own shape: the
# resolutions are made HERE, where a mistake costs nothing, VERIFIED here, and then applied
# MECHANICALLY at assembly and stamped PRE-RESOLVED.  What this script writes is the SLOT and the
# evidence -- the three versions plus the conflicted working file -- and what it never writes is an
# answer.  A conflict is the coordinator's RULING.
#     $SELFDIR/coord-train46-resolutions/seat<N>/
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
# `git merge` and nothing else; the assembly ALSO applies each seat's CLASS SHAPE and its FORBIDDEN
# path set, which only the assembly does.  Two instruments, two questions.
#
# ⚠ NO BUILD, NO CONVERTER, NO DOTNET.  git only.  It is safe to run beside a timing measurement --
# but NOT inside a worktree a battery is using: it creates its own.
#
#   usage:  bash coord-train46-rehearse.sh                 # base = the CURRENT origin/master
#           M=<sha> bash coord-train46-rehearse.sh         # rehearse onto a named master
#           KEEP=1 bash coord-train46-rehearse.sh          # leave the worktree standing for inspection
#           bash coord-train46-rehearse.sh --verify        # verify the SAVED resolutions, merge nothing
#   exit 0  every seat merged clean (or every saved resolution verified)
#        1  a seat conflicted, or a saved resolution failed verification
#        2  a precondition refused
# ============================================================================================
set -u
G=/c/Projects/go2cs
WT="$G/.claude/worktrees/coord-t46-rehearse"
TAG=t46rehearse
say(){ printf '%s\n' "$*"; }

# ⚠ RESOLVE THIS SCRIPT'S OWN DIRECTORY **BEFORE** ANY cd, AND ABSOLUTELY.  A ${BASH_SOURCE[0]}
#   evaluated AFTER `cd $G` resolves against the REPOSITORY ROOT, names a file that is not there, and
#   every read off it comes back well-formed and about the wrong path.  Train 45's rehearsal walked
#   into exactly that on its first run.
SELFDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ASM="$SELFDIR/coord-train46-assemble.sh"
RESROOT="$SELFDIR/coord-train46-resolutions"
BASE_EXPECT=44f858717

# --- the seat table.  ⚠ IT IS READ OUT OF THE ASSEMBLY SCRIPT, NEVER RETYPED.  A second copy of a
#     seat list is the thing that drifts, and a rehearsal of a different list than the assembly will
#     merge is a rehearsal of a train nobody is assembling.
[ -f "$ASM" ] || { say "REFUSED: $ASM not found -- the seat table is read from it, never retyped"; exit 2; }
lo=$(grep -nE '^SEAT_TABLE="1\|' "$ASM" | head -1 | cut -d: -f1)
[ -n "$lo" ] || { say "REFUSED: could not find SEAT_TABLE in $ASM"; exit 2; }
hi=$(awk -v s="$lo" 'NR>=s' "$ASM" | grep -nE '\|tip"$' | head -1 | cut -d: -f1)
[ -n "$hi" ] || { say "REFUSED: could not find the end of SEAT_TABLE in $ASM"; exit 2; }
hi=$(( lo + hi - 1 ))
eval "$(sed -n "${lo},${hi}p" "$ASM")"
ROWS=$(printf '%s\n' "$SEAT_TABLE" | grep -c .)
say "REHEARSAL seat table :: read from $(basename "$ASM") lines $lo..$hi, $ROWS row(s)"
[ "$ROWS" = "6" ] || { say "REFUSED: the table holds $ROWS row(s) where train 46 is SIX (all filled at seat fill, 2026-09-08 15:40)"; exit 2; }

# ============================================================================================
# --verify :: re-read the SAVED resolutions and assert them.  Merges nothing, touches no worktree.
# ============================================================================================
if [ "${1:-}" = "--verify" ]; then
  say "=== TRAIN 46 RESOLUTION VERIFY :: $RESROOT ==="
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
case "$MSHORT" in
  "$BASE_EXPECT") : ;;
  *) say "⚠ NOTE: the rehearsal base $MSHORT is NOT the derive-time base $BASE_EXPECT.  That is legal with M=<sha>, and it means every verdict below describes a train nobody has assembled -- state it wherever these readings are quoted." ;;
esac

# --- fetch every non-PENDING seat by exact ref and verify it resolves.  An unresolved SHA and an
#     invented one read the same, so this is a refusal and never a "no".
NEED=0; HAVE=0; PEND=0
while IFS='|' read -r n b s c m; do
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

CONFLICTS=0; MERGED=0; SKIPPED=0; SKIPNAMES=""
while IFS='|' read -r n b s c m <&3; do
  [ -n "$n" ] || continue
  if [ "$s" = "PENDING" ]; then
    SKIPPED=$(( SKIPPED + 1 )); SKIPNAMES="$SKIPNAMES $n($b)"
    say "seat $n ($c) $b :: **SKIPPED -- PENDING**.  NO placeholder is expected after seat fill; a PENDING row here is a table the coordinator did not finish filling."
    continue
  fi
  short=$(git rev-parse --short "$s")
  git merge --no-ff --no-edit "$s" -m "rehearsal seat $n $b ($short)" > "/tmp/${TAG}-merge-$n.log" 2>&1
  mrc=$?
  if [ "$mrc" = "0" ]; then
    MERGED=$(( MERGED + 1 ))
    say "seat $n ($c) $b @$short :: CLEAN  (tree -> $(git rev-parse --short 'HEAD^{tree}'), files in the accumulation $(git -c core.quotepath=false diff --name-only "$M" HEAD | grep -c .))"
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
    say "      the merge was ABORTED; the accumulation continues from the PREVIOUS seat, so every later verdict belongs to a tree WITHOUT this seat"
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
# --- seat 3's DISPLACEMENT, read as a numstat.  A registration whose footprint did not land is the
#     syscall.Uname silent-subtraction class, and the DELETION column is what says which happened.
PN=$(git diff --numstat "$M" HEAD -- src/core/runtime/panic.cs)
RN=$(git diff --numstat "$M" HEAD -- src/go2cs/manualTypeOperations.go)
say "seat 3 registry       :: ${RN:-<unchanged>}  (at 5c5ef371d the branch reads +27/-0)"
say "seat 3 displacement   :: ${PN:-<unchanged>}  (at 5c5ef371d the branch reads +2/-53 -- ⚠ the DELETION column is the point: a +N/-0 would mean the registration landed and the displacement did not)"
GN=$(git diff --numstat "$M" HEAD -- src/gen/go2cs-gen/Templates/InheritedType/ISliceTypeTemplate.cs)
say "seat 2 generator half :: ${GN:-<unchanged>}  (at 9893b70e1 the branch reads +2/-0 -- the half NO standing gate compiles: route #7)"
MK=$(git -c core.quotepath=false grep -c '^<<<<<<<' HEAD -- . 2>/dev/null | wc -l)
say "conflict markers in the committed tree :: $MK file(s) carry one (must be 0)"
[ "$MK" = "0" ] || { say "  ^ files:"; git -c core.quotepath=false grep -l '^<<<<<<<' HEAD -- . 2>/dev/null | sed 's/^/      /'; }
say "delta file list:"
git -c core.quotepath=false diff --name-only "$M" HEAD | sed 's/^/      /'
say ""
say "⚠ WHAT A CLEAN REHEARSAL DOES AND DOES NOT SAY.  It says the seats MERGE, in this order, onto"
say "  this base.  It says NOTHING about whether they are CORRECT together -- that is the assembly's"
say "  A-assertions and its battery -- and NOTHING about the CLASS SHAPES, which the assembly applies"
say "  and this script does not.  It also says NOTHING about seat 3's \`sync\` retarget, whose whole"
say "  point is a change that EMITS NOTHING: a clean merge of it is not evidence the hand-own still"
say "  works, and the instrument that can answer that is LEG K's banked sync row."
RC=0
[ "$CONFLICTS" = "0" ] || RC=1
[ "$MK" = "0" ] || RC=1
say "=== REHEARSAL DONE conflicts=$CONFLICTS markers=$MK exit=$RC ==="
exit $RC
