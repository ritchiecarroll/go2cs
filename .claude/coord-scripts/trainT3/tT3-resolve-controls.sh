#!/usr/bin/env bash
# TRAIN FL -- the controls of tT3-resolve.py. DERIVED 2026-10-08 from trainQ/tQ-resolve-controls.sh: Q's eight arms
# unchanged (their objects are fixed), plus FL7's kind append-both-sorted:
#   C9  synthetic: the two sides of one hunk written in REVERSE order, ruled append-both-sorted
#                                                         EXPECT rc 0, the hunk's lines sorted, the line count kept
#   C10 REAL blobs, src/go2cs/go2cs-src.projitems of FL row 8 (the hand merge 4613dfe676: its two parents and their
#       merge-base), ruled append-both-sorted             EXPECT rc 0 and the index blob EQUAL to the hand merge's blob
#   C10n the same merge ruled append-both                 EXPECT rc 0 and the blob DIFFERENT (the arm can fail)
# Q's header follows.
# (Q) TRAIN Q -- the controls of tQ-resolve.py (floor 13: a gate that has never been made to fail proves nothing).
#   OUT=<scratch dir under /h/go2cs-tmp-coord/coord-scratch/tT3/> bash tT3-resolve-controls.sh
# It builds THROWAWAY repositories under OUT, merges two branches in each so that git stops on a conflict, and runs the
# resolver as the map and the assembly run it. Nothing is built, converted or tested, and no worktree of the real
# repository is touched: the two REAL arms read three blobs each from the shared object store with `git show` and
# commit them into a throwaway repository of their own.
#   C1  synthetic BOARD shape, ruled                      EXPECT rc 0, RESOLVED, lines = base + both appends, guard last
#   C2  the same merge, NO ruling                         EXPECT rc 3, UNRULED naming the row, the path and the hunk
#   C3  the same merge, the ruling names ANOTHER sha      EXPECT rc 3 (a re-cut does not inherit a ruling)
#   C4  both sides EDIT one existing line, ruled          EXPECT rc 3 ('not an insert against an insert')
#   C5  a ruled path and an UNRULED path conflict         EXPECT rc 3 and NOTHING written (both still unmerged)
#   C6  REAL blobs, TranspileTests.cs (the hunk of g-named-pointer-equality: base 446d2c8ba0, ours 8d4ab2d7fd, theirs
#       0c6a2618fa)                                       EXPECT rc 0, 1 hunk ours 6 / theirs 3, [TestMethod] = Check,
#                                                         the worktree file CRLF and the index blob LF
#   C7  REAL blobs, the BOARD (hunk 1 of the map: base = P's landed line, ours c585fa199d, theirs 3196a93cdc)
#                                                         EXPECT rc 0, 1 hunk ours 139 / theirs 35, guard the final line
#   C8  a READING, the trap the diff3 form avoids: `git merge-file --union` on C6's blobs
#                                                         EXPECT fewer [TestMethod] lines than Check methods
# Last line: RESOLVE-CONTROLS DONE failed=<n>; exit 1 when n > 0.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
OUT=${OUT:?set OUT to a scratch directory under /h/go2cs-tmp-coord/coord-scratch/tT3/}
case "$OUT" in /h/go2cs-tmp-coord/coord-scratch/tT3/?*) ;; *) echo "ABORT: OUT=$OUT is not under /h/go2cs-tmp-coord/coord-scratch/tT3/"; exit 2 ;; esac
case "$OUT" in *..*) echo "ABORT: OUT holds '..'"; exit 2 ;; esac
REPO=${REPO:-/h/Projects/go2cs}          # read-only: git show of blobs
RS="$SD/tT3-resolve.py"
[ -f "$RS" ] || { echo "ABORT: $RS missing"; exit 2; }
# REVIEW ROUND 1: 'rm -rf "$OUT"' accepted ANY existing folder under coord-scratch/tT3/ (a battery's run folder among
# them). An existing OUT is deleted only when it carries the marker file this script wrote into it and holds nothing of
# a run; anything else is refused, never deleted.
MARK=.tT3-resolve-controls-scratch
if [ -e "$OUT" ]; then
  if [ -d "$OUT" ] && [ -f "$OUT/$MARK" ] && [ ! -e "$OUT/tT3-logs" ] && [ ! -e "$OUT/mod-logs" ] && [ ! -e "$OUT/tT3-fixup-logs" ] && [ ! -e "$OUT/battery.console.log" ]; then rm -rf "$OUT"
  else echo "ABORT: OUT=$OUT exists and is not a scratch folder this script made (no $MARK in it, or it holds a run's tT3-logs / mod-logs / tT3-fixup-logs / battery.console.log): it is NOT deleted. Give a folder that does not exist yet"; exit 2; fi
fi
mkdir -p "$OUT" && : > "$OUT/$MARK" || exit 2
FAILED=0
G(){ git -c user.name=ctl -c user.email=ctl@invalid -c commit.gpgsign=false -c core.autocrlf=false "$@"; }
mk(){ # name -> a fresh repository with the tree's two line-ending pins
  rm -rf "$OUT/$1"; mkdir -p "$OUT/$1" && ( cd "$OUT/$1" && G init -q . && printf '*.cs text eol=crlf\n*.slnx text eol=crlf\n' > .gitattributes && G add .gitattributes && G commit -qm attrs )
}
three(){ # repo, path, base file, ours file, theirs file -> branches o and t off a base commit; leaves the merge of t into o stopped
  ( cd "$OUT/$1" && mkdir -p "$(dirname "$2")" && cp "$3" "$2" && G add "$2" && G commit -qm base && G branch t && G checkout -q -b o \
    && cp "$4" "$2" && G commit -qam ours && G checkout -q t && cp "$5" "$2" && G commit -qam theirs && G checkout -q o ) || { echo "ABORT: could not build $1"; exit 2; }
}
stopmerge(){ ( cd "$OUT/$1" && G merge -q --no-ff -m m t > /dev/null 2>&1 ); }
res(){ # repo, ruled file, ref [, --dry] -> runs the resolver on the stopped merge; prints its lines; returns its rc
  local sha; sha=$(git -C "$OUT/$1" rev-parse MERGE_HEAD 2>/dev/null)
  python -B "$(cygpath -w "$RS")" "$(cygpath -w "$OUT/$1")" "$(cygpath -w "$2")" "$3" "${sha:-none}" ${4:-} 2>&1 | tr -d '\r'
  return "${PIPESTATUS[0]}"
}
verdict(){ if [ "$2" = 0 ]; then echo "CONTROL $1: ok -- $3"; else echo "CONTROL $1: FAILED -- $3"; FAILED=$((FAILED + 1)); fi; }
rule(){ # ruled file, ref, repo, path [, sha10 override [, kind]]
  local sha; sha=${5:-$(git -C "$OUT/$3" rev-parse t | cut -c1-10)}
  printf '%s|%s|%s|%s|hunks=1|control\n' "$2" "$sha" "$4" "${6:-append-both}" >> "$1"
}
BP=docs/phase4/BOARD-next-validation-candidates.md
GUARD='<!-- {% endraw %} keep this the FINAL line -->'
printf '%s\n' '# board' '<!-- {% raw %} guard -->' 'entry one' '' "$GUARD" > "$OUT/b.base"
printf '%s\n' '# board' '<!-- {% raw %} guard -->' 'entry one' '' 'ours A' 'ours B' '' "$GUARD" > "$OUT/b.ours"
printf '%s\n' '# board' '<!-- {% raw %} guard -->' 'entry one' '' 'theirs A' '' "$GUARD" > "$OUT/b.theirs"

# ---- C1
mk c1; three c1 "$BP" "$OUT/b.base" "$OUT/b.ours" "$OUT/b.theirs"; stopmerge c1
: > "$OUT/c1.ruled"; rule "$OUT/c1.ruled" row-x c1 "$BP"
o=$(res c1 "$OUT/c1.ruled" row-x); rc=$?
nl=$(wc -l < "$OUT/c1/$BP"); last=$(tail -n 1 "$OUT/c1/$BP"); un=$(git -C "$OUT/c1" diff --name-only --diff-filter=U | wc -l)
[ "$rc" = 0 ] && [ "$nl" = 10 ] && [ "$last" = "$GUARD" ] && [ "$un" = 0 ] && grep -q 'ours B' "$OUT/c1/$BP" && grep -q 'theirs A' "$OUT/c1/$BP" \
  && [ "$(grep -n 'ours A' "$OUT/c1/$BP" | cut -d: -f1)" -lt "$(grep -n 'theirs A' "$OUT/c1/$BP" | cut -d: -f1)" ]
verdict C1 $? "rc=$rc lines=$nl (EXPECT 10 = 5 + 3 + 2) unmerged-left=$un guard-last=$([ "$last" = "$GUARD" ] && echo yes || echo NO) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-160)"
# ---- C2
mk c2; three c2 "$BP" "$OUT/b.base" "$OUT/b.ours" "$OUT/b.theirs"; stopmerge c2
: > "$OUT/c2.ruled"
o=$(res c2 "$OUT/c2.ruled" row-x); rc=$?
[ "$rc" = 3 ] && echo "$o" | grep -q "^UNRULED row=row-x .*path=$BP: no ruling" && echo "$o" | grep -q 'hunk 1: ours 3 / base 0 / theirs 2' && [ "$(git -C "$OUT/c2" diff --name-only --diff-filter=U | wc -l)" = 1 ]
verdict C2 $? "rc=$rc (EXPECT 3, the row, the path and 'hunk 1: ours 3 / base 0 / theirs 2' named; the path still unmerged) :: $(echo "$o" | grep '^UNRULED' | cut -c1-200)"
# ---- C3
mk c3; three c3 "$BP" "$OUT/b.base" "$OUT/b.ours" "$OUT/b.theirs"; stopmerge c3
: > "$OUT/c3.ruled"; rule "$OUT/c3.ruled" row-x c3 "$BP" 0123456789
o=$(res c3 "$OUT/c3.ruled" row-x); rc=$?
[ "$rc" = 3 ] && echo "$o" | grep -q '^UNRULED row=row-x'
verdict C3 $? "rc=$rc (EXPECT 3: the ruling names sha 0123456789, the row is $(git -C "$OUT/c3" rev-parse t | cut -c1-10))"
# ---- C4
printf '%s\n' '# board' '<!-- {% raw %} guard -->' 'entry ONE (ours edit)' '' "$GUARD" > "$OUT/b4.ours"
printf '%s\n' '# board' '<!-- {% raw %} guard -->' 'entry one, theirs edit' '' "$GUARD" > "$OUT/b4.theirs"
mk c4; three c4 "$BP" "$OUT/b.base" "$OUT/b4.ours" "$OUT/b4.theirs"; stopmerge c4
: > "$OUT/c4.ruled"; rule "$OUT/c4.ruled" row-x c4 "$BP"
o=$(res c4 "$OUT/c4.ruled" row-x); rc=$?
[ "$rc" = 3 ] && echo "$o" | grep -q 'not an insert against an insert' && [ "$(git -C "$OUT/c4" diff --name-only --diff-filter=U | wc -l)" = 1 ]
verdict C4 $? "rc=$rc (EXPECT 3: both sides edit one existing line, the base part is not empty) :: $(echo "$o" | grep '^UNRULED' | cut -c1-200)"
# ---- C5
mk c5
( cd "$OUT/c5" && mkdir -p docs/phase4 && cp "$OUT/b.base" "$BP" && printf 'x\n' > other.txt && G add . && G commit -qm base && G branch t && G checkout -q -b o \
  && cp "$OUT/b.ours" "$BP" && printf 'x ours\n' > other.txt && G commit -qam ours && G checkout -q t && cp "$OUT/b.theirs" "$BP" && printf 'x theirs\n' > other.txt && G commit -qam theirs && G checkout -q o ) || { echo "ABORT c5"; exit 2; }
stopmerge c5
: > "$OUT/c5.ruled"; rule "$OUT/c5.ruled" row-x c5 "$BP"
o=$(res c5 "$OUT/c5.ruled" row-x); rc=$?
[ "$rc" = 3 ] && echo "$o" | grep -q '^UNRULED row=row-x .*path=other.txt' && [ "$(git -C "$OUT/c5" diff --name-only --diff-filter=U | wc -l)" = 2 ] && echo "$o" | grep -q 'NOTHING was written'
verdict C5 $? "rc=$rc (EXPECT 3 naming other.txt, and BOTH paths still unmerged: $(git -C "$OUT/c5" diff --name-only --diff-filter=U | wc -l))"
# ---- C6 (real blobs)
LP=src/tests/Behavioral/BehavioralTests/TranspileTests.cs
if git -C "$REPO" cat-file -e "446d2c8ba0:$LP" 2>/dev/null && git -C "$REPO" cat-file -e "8d4ab2d7fd:$LP" 2>/dev/null && git -C "$REPO" cat-file -e "0c6a2618fa:$LP" 2>/dev/null; then
  git -C "$REPO" show "446d2c8ba0:$LP" > "$OUT/l.base"; git -C "$REPO" show "8d4ab2d7fd:$LP" > "$OUT/l.ours"; git -C "$REPO" show "0c6a2618fa:$LP" > "$OUT/l.theirs"
  mk c6; three c6 "$LP" "$OUT/l.base" "$OUT/l.ours" "$OUT/l.theirs"; stopmerge c6
  : > "$OUT/c6.ruled"; rule "$OUT/c6.ruled" g-named-pointer-equality c6 "$LP"
  o=$(res c6 "$OUT/c6.ruled" g-named-pointer-equality); rc=$?
  tm=$(tr -d '\r' < "$OUT/c6/$LP" | grep -c '^[[:space:]]*\[TestMethod\]$'); ck=$(tr -d '\r' < "$OUT/c6/$LP" | grep -cE '^[[:space:]]*public void Check[A-Za-z0-9_]*\(')
  crw=$(tr -cd '\r' < "$OUT/c6/$LP" | wc -c); cri=$(git -C "$OUT/c6" cat-file blob ":$LP" | tr -cd '\r' | wc -c)
  nlr=$(wc -l < "$OUT/c6/$LP"); nlo=$(wc -l < "$OUT/l.ours")
  [ "$rc" = 0 ] && echo "$o" | grep -q 'hunks=1 ours=6 theirs=3' && [ "$tm" = "$ck" ] && [ "$nlr" = $((nlo + 3)) ] && [ "$crw" = "$nlr" ] && [ "$cri" = 0 ] \
    && grep -q 'CheckNamedPointerEquality' "$OUT/c6/$LP" && grep -q 'CheckNamedPointerFieldAddress' "$OUT/c6/$LP" && grep -q 'CheckNamedPointerReflectElem' "$OUT/c6/$LP"
  verdict C6 $? "rc=$rc [TestMethod]=$tm Check=$ck lines=$nlr (EXPECT ours $nlo + 3) CR in the worktree file=$crw (EXPECT one a line) CR in the index blob=$cri (EXPECT 0) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-150)"
  # ---- C8 (a reading)
  git merge-file -p --union "$OUT/l.ours" "$OUT/l.base" "$OUT/l.theirs" > "$OUT/l.union" 2>/dev/null
  utm=$(grep -c '^[[:space:]]*\[TestMethod\]$' "$OUT/l.union"); uck=$(grep -cE '^[[:space:]]*public void Check[A-Za-z0-9_]*\(' "$OUT/l.union")
  [ "$utm" -lt "$uck" ]
  verdict C8 $? "the trap, read: git merge-file --union gives [TestMethod]=$utm for Check=$uck (EXPECT fewer attribute lines than methods: a method that compiles and is not a test)"
else
  verdict C6 1 "the three real blobs of $LP are not in $REPO's object store (446d2c8ba0, 8d4ab2d7fd, 0c6a2618fa): NOT MEASURED"
fi
# ---- C7 (real blobs)
BB=${BOARD_BASE:-446d2c8ba0}   # master's BOARD is byte-identical from that cut to TRAIN P's landed line (trainQ/tQ-map.md A)
if git -C "$REPO" cat-file -e "$BB:$BP" 2>/dev/null && git -C "$REPO" cat-file -e "c585fa199d:$BP" 2>/dev/null && git -C "$REPO" cat-file -e "3196a93cdc:$BP" 2>/dev/null; then
  git -C "$REPO" show "$BB:$BP" > "$OUT/r.base"; git -C "$REPO" show "c585fa199d:$BP" > "$OUT/r.ours"; git -C "$REPO" show "3196a93cdc:$BP" > "$OUT/r.theirs"
  mk c7; three c7 "$BP" "$OUT/r.base" "$OUT/r.ours" "$OUT/r.theirs"; stopmerge c7
  : > "$OUT/c7.ruled"; rule "$OUT/c7.ruled" c2-func-chan-dir c7 "$BP"
  o=$(res c7 "$OUT/c7.ruled" c2-func-chan-dir); rc=$?
  nlr=$(wc -l < "$OUT/c7/$BP"); nlb=$(wc -l < "$OUT/r.base")
  [ "$rc" = 0 ] && echo "$o" | grep -q 'hunks=1 ours=139 theirs=35' && [ "$nlr" = $((nlb + 139 + 35)) ] && tail -n 1 "$OUT/c7/$BP" | grep -q '^<!-- {% endraw %}'
  verdict C7 $? "rc=$rc lines=$nlr (EXPECT base $nlb + 139 + 35) final line is the endraw guard=$(tail -n 1 "$OUT/c7/$BP" | grep -q '^<!-- {% endraw %}' && echo yes || echo NO) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-150)"
else
  verdict C7 1 "the three real BOARD blobs are not in $REPO's object store ($BB, c585fa199d, 3196a93cdc): NOT MEASURED"
fi
# ---- C9 (FL7, synthetic)
PJ=src/go2cs/go2cs-src.projitems
printf '%s\n' '<ItemGroup>' '  <None Include="a_test.go" />' '  <None Include="z_test.go" />' '</ItemGroup>' > "$OUT/p.base"
printf '%s\n' '<ItemGroup>' '  <None Include="a_test.go" />' '  <None Include="q_test.go" />' '  <None Include="z_test.go" />' '</ItemGroup>' > "$OUT/p.ours"
printf '%s\n' '<ItemGroup>' '  <None Include="a_test.go" />' '  <None Include="m_test.go" />' '  <None Include="z_test.go" />' '</ItemGroup>' > "$OUT/p.theirs"
mk c9; three c9 "$PJ" "$OUT/p.base" "$OUT/p.ours" "$OUT/p.theirs"; stopmerge c9
: > "$OUT/c9.ruled"; rule "$OUT/c9.ruled" row-s c9 "$PJ" "" append-both-sorted
o=$(res c9 "$OUT/c9.ruled" row-s); rc=$?
got=$(tr -d '\r' < "$OUT/c9/$PJ" | tr '\n' '|')
[ "$rc" = 0 ] && [ "$got" = '<ItemGroup>|  <None Include="a_test.go" />|  <None Include="m_test.go" />|  <None Include="q_test.go" />|  <None Include="z_test.go" />|</ItemGroup>|' ]
verdict C9 $? "rc=$rc result=[$got] (EXPECT m before q: the row's line above the union's, sorted) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-150)"
# ---- C10 / C10n (FL7, real blobs of the hand merge 4613dfe676)
HM=${FL_HAND_MERGE:-4613dfe676}
if git -C "$REPO" cat-file -e "$HM^2:$PJ" 2>/dev/null; then
  hb=$(git -C "$REPO" merge-base "$HM^1" "$HM^2")
  git -C "$REPO" show "$hb:$PJ" > "$OUT/j.base"; git -C "$REPO" show "$HM^1:$PJ" > "$OUT/j.ours"; git -C "$REPO" show "$HM^2:$PJ" > "$OUT/j.theirs"
  want=$(git -C "$REPO" rev-parse "$HM:$PJ")
  for k in append-both-sorted append-both; do
    mk "c10-$k"; three "c10-$k" "$PJ" "$OUT/j.base" "$OUT/j.ours" "$OUT/j.theirs"; stopmerge "c10-$k"
    : > "$OUT/c10-$k.ruled"; rule "$OUT/c10-$k.ruled" c2-facelift-chain-q "c10-$k" "$PJ" "" "$k"
    o=$(res "c10-$k" "$OUT/c10-$k.ruled" c2-facelift-chain-q); rc=$?
    got=$(git -C "$OUT/c10-$k" rev-parse ":$PJ" 2>/dev/null)
    if [ "$k" = append-both-sorted ]; then
      [ "$rc" = 0 ] && [ "$got" = "$want" ]
      verdict C10 $? "rc=$rc index blob ${got:0:10} vs the hand merge's ${want:0:10} (EXPECT EQUAL: 'kept both in alphabetical order' IS this kind) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-120)"
    else
      [ "$rc" = 0 ] && [ -n "$got" ] && [ "$got" != "$want" ]
      verdict C10n $? "rc=$rc index blob ${got:0:10} vs ${want:0:10} (EXPECT DIFFERENT: append-both writes the union's line first)"
    fi
  done
else
  verdict C10 1 "the hand merge $HM is not in $REPO's object store: NOT MEASURED"
fi
echo "RESOLVE-CONTROLS DONE failed=$FAILED (scratch: $OUT)"
[ "$FAILED" = 0 ]
