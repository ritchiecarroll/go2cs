#!/usr/bin/env bash
# tT4-controls-r2.sh -- TRAIN T4 run2: the controls of the run2 kit changes (floor 13: each reader is made to fail on a
# plant first). Nothing builds, converts or runs a test; git objects and the kit's own text only.
#   OUT=<scratch dir under /h/go2cs-tmp-coord/coord-scratch/tT4/> R1KIT=<run1 kit folder> bash tT4-controls-r2.sh
# Arms:
#   R1D   T4-13: r1delta EXTRACTED from tT4-assemble.sh (between its markers), 7 arms on git objects: the row's own range
#         read against its base tree EXACT; a head missing one range path DIFFERENT (MISSING named); a reference tree one
#         commit older DIFFERENT (EXTRA named); run1's union against its own tree DIFFERENT (all three MISSING); no tree,
#         a malformed range and an unknown head NOT COMPARED (rc 2).
#   NGJ0  T4-14: the target is absent at BASE and present at cfe57cd718's parent; the retirement guard EXTRACTED from
#         tT4-battery.sh reads both (stamp at BASE, finding at the parent); run2's battery holds no NGF-j0 leg and no
#         j0-consume.ps1 in NGF, run1's holds 3 and 1 (the control fires on the old copy); the leg-name sets differ by
#         EXACTLY the three retired legs; bash -n.
#   R2N   the run2 names: the union worktree, its branch, the map worktree, the stamps folder and the emission scratch,
#         read from the code lines; no code line points W, WT or the branch at run1's tT4 / train-t4-union.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
OUT=${OUT:?set OUT to a scratch directory under /h/go2cs-tmp-coord/coord-scratch/tT4/}
case "$OUT" in /h/go2cs-tmp-coord/coord-scratch/tT4/?*) ;; *) echo "ABORT: OUT=$OUT"; exit 2 ;; esac
R1KIT=${R1KIT:-/h/go2cs-tmp-coord/coord-scratch/tT4/run1}
REPO=/h/Projects/go2cs
BASE=${BASE:-226afb810a21408ebda2412dd74d526345c46f65}
mkdir -p "$OUT" || exit 2
FAILED=0
verdict(){ if [ "$2" = 0 ]; then echo "CONTROL $1: ok -- $3"; else echo "CONTROL $1: FAILED -- $3"; FAILED=$((FAILED + 1)); fi; }
frag(){ sed -n "/^# >>> $2/,/^# <<< $2/p" "$SD/$1"; }

# ------------------------------------------------------------------------------------------------ R1D (T4-13)
frag tT4-assemble.sh r1delta > "$OUT/r1delta.frag"
if ! grep -q '^r1delta(){' "$OUT/r1delta.frag"; then verdict R1D 1 "no r1delta between the markers of tT4-assemble.sh"
else
  A=a0e2099e9a; B=fbf2a5ebc5
  r=$( cd "$REPO" && . "$OUT/r1delta.frag"
    a(){ local want=$1 wrc=$2 grepx=$3; shift 3; local o rc; o=$(r1delta "$@"); rc=$?
      case "$o" in "$want"*) if [ "$rc" = "$wrc" ] && { [ -z "$grepx" ] || printf '%s\n' "$o" | grep -qE "$grepx"; }; then printf 'ok '; else printf 'WRONG(rc=%s) ' "$rc"; fi ;;
        *) printf 'WRONG(%s) ' "$(printf '%s' "$o" | head -n 1 | cut -c1-50)" ;; esac; }
    a 'R1 DELTA: EXACT' 0 '3 of 3 hold' "$B" "$(git rev-parse $A^{tree})" "$A..$B"
    a 'R1 DELTA: DIFFERENT' 1 '^  MISSING docs/PLAN-comment-and-spacing-parity.md$' 07cacea829 "$(git rev-parse $A^{tree})" "$A..$B"
    a 'R1 DELTA: DIFFERENT' 1 '^  EXTRA ' "$B" "$(git rev-parse $A~1^{tree})" "$A..$B"
    a 'R1 DELTA: DIFFERENT' 1 'MISSING \(in the range, not different\) 3' 7dfd1d493c 7e1f0969c11c86b14b216bd64e76f4afb8ea5bb6 "$A..$B"
    a 'R1 DELTA: NOT COMPARED' 2 '' "$B" '' "$A..$B"
    a 'R1 DELTA: NOT COMPARED' 2 '' "$B" "$(git rev-parse $A^{tree})" "$A"
    a 'R1 DELTA: NOT COMPARED' 2 '' 0123456789 "$(git rev-parse $A^{tree})" "$A..$B" )
  [ "$r" = 'ok ok ok ok ok ok ok ' ]
  verdict R1D "$?" "r1delta extracted, 7 arms: ${r}(EXPECT 7 ok: the row's range vs its base tree EXACT 3 of 3 blobs; 07cacea829 MISSING the PLAN doc; a reference one commit older EXTRA; run1's union vs its own tree MISSING 3; no tree, no range, an unknown head NOT COMPARED rc 2)"
fi

# ------------------------------------------------------------------------------------------------ NGJ0 (T4-14)
j0p=src/tools/j0-uuid-rehearsal
b0=1; git -C "$REPO" cat-file -e "$BASE:$j0p" 2>/dev/null && b0=0
p1=0; git -C "$REPO" cat-file -e "cfe57cd718^:$j0p" 2>/dev/null && p1=1
anc=0; git -C "$REPO" merge-base --is-ancestor cfe57cd718 "$BASE" && anc=1
# the guard, extracted (the if ... fi block that follows the T4-14 comment), run with HEAD -> a ref
sed -n '/^if git cat-file -e "HEAD:src\/tools\/j0-uuid-rehearsal"/,/^fi$/p' "$SD/tT4-battery.sh" > "$OUT/j0guard.frag"
gl=$(grep -c . "$OUT/j0guard.frag")
gread(){ ( cd "$REPO" && finding(){ echo "FINDING $*"; }; stamp(){ echo "STAMP $*"; }; eval "$(sed "s/\"HEAD:/\"$1:/" "$OUT/j0guard.frag")" ); }
gb=$(gread "$BASE" | cut -c1-30); gp=$(gread "cfe57cd718^" | cut -c1-30)
n2=$(grep -cE '^[[:space:]]*leg NGF-j0' "$SD/tT4-battery.sh"); n1=$(grep -cE '^[[:space:]]*leg NGF-j0' "$R1KIT/tT4-battery.sh")   # code lines only
f2=$(grep -E '^NGF=' "$SD/tT4-battery.sh" | sed 's/[[:space:]]#.*//' | grep -c 'j0-consume'); f1=$(grep -E '^NGF=' "$R1KIT/tT4-battery.sh" | sed 's/[[:space:]]#.*//' | grep -c 'j0-consume')   # the assignment, not its comment
legs(){ grep -oE '^[[:space:]]*(leg|capped) "?[A-Za-z0-9:_.$-]+' "$1" | sed -E 's/^[[:space:]]*(leg|capped) "?//' | LC_ALL=C sort; }
gone=$(LC_ALL=C comm -23 <(legs "$R1KIT/tT4-battery.sh") <(legs "$SD/tT4-battery.sh") | tr '\n' ' '); added=$(LC_ALL=C comm -13 <(legs "$R1KIT/tT4-battery.sh") <(legs "$SD/tT4-battery.sh") | tr '\n' ' ')
nl1=$(legs "$R1KIT/tT4-battery.sh" | grep -c .); nl2=$(legs "$SD/tT4-battery.sh" | grep -c .)
bn=0; bash -n "$SD/tT4-battery.sh" 2>/dev/null && bn=1
[ "$b0" = 1 ] && [ "$p1" = 1 ] && [ "$anc" = 1 ] && [ "$gl" = 5 ] && [ "${gb:0:5}" = STAMP ] && [ "${gp:0:7}" = FINDING ] && [ "$n2" = 0 ] && [ "$n1" = 3 ] && [ "$f2" = 0 ] && [ "$f1" = 1 ] \
  && [ "$gone" = 'NGF-j0default NGF-j0nugetgo NGF-j0prop ' ] && [ -z "$added" ] && [ "$nl2" = $((nl1 - 3)) ] && [ "$bn" = 1 ]
verdict NGJ0 "$?" "$j0p absent at BASE ${BASE:0:10}=$b0 present at cfe57cd718^=$p1 cfe57cd718 in BASE=$anc (EXPECT 1 1 1); the guard extracted ($gl lines, EXPECT 5): at BASE [$gb] (EXPECT STAMP), at cfe57cd718^ [$gp] (EXPECT FINDING); 'leg NGF-j0' run2=$n2 run1=$n1 (EXPECT 0 3); j0-consume in NGF run2=$f2 run1=$f1 (EXPECT 0 1); leg names run1 $nl1 -> run2 $nl2, gone [$gone] added [${added}] (EXPECT exactly the three NGF-j0 legs gone, none added); bash -n=$bn"

# ------------------------------------------------------------------------------------------------ R2N (names)
code(){ grep -vE '^[[:space:]]*#' "$SD/$1"; }
c1=$(code tT4-assemble.sh | grep -cxF 'WT=/h/go2cs-tmp-coord/tT4r2   # R2 (run2)'); c2=$(code tT4-assemble.sh | grep -c 'train-t4-union-r2')
c3=$(code tT4-fixup.sh | grep -c '^BRANCH=train-t4-union-r2 '); c4=$(code tT4-fixup.sh | grep -c '^W=${W:-/h/go2cs-tmp-coord/tT4r2}')
c5=$(code tT4-conflict-map.sh | grep -c '^WT=/h/go2cs-tmp-coord/tT4r2-map '); c6=$(code tT4-conflict-map.sh | grep -c '^X=/h/go2cs-tmp-coord/coord-scratch/tT4/run2/stamps ')
c7=$(code tT4-fixup.sh | grep -c '^ES=/h/go2cs-tmp-coord/tT4emitr2 '); c8=$(code tT4-fixup.sh | grep -c '^MTF=/h/go2cs-tmp-coord/coord-scratch/tT4/run2/stamps/assemble-maptree.txt')
c9=$(code tT4-assemble.sh | grep -c '"$X/tT4/[^r]')
c10=$(for f in tT4-assemble.sh tT4-fixup.sh tT4-emitcheck.sh tT4-battery.sh tT4-modules-legs.sh tT4-reread.sh tT4-land-prep.sh tT4-land-final-reads.sh tT4-land-rereads.sh; do code "$f" | grep -E '^(W|WT)=' | grep -vc 'tT4r2'; done | awk '{ t += $1 } END { print t + 0 }')
c11=$(for f in tT4-assemble.sh tT4-fixup.sh; do code "$f"; done | grep -cE 'train-t4-union([^-]|$)')
[ "$c1" = 1 ] && [ "$c2" = 2 ] && [ "$c3" = 1 ] && [ "$c4" = 1 ] && [ "$c5" = 1 ] && [ "$c6" = 1 ] && [ "$c7" = 1 ] && [ "$c8" = 1 ] && [ "$c9" = 0 ] && [ "$c10" = 0 ] && [ "$c11" = 0 ]
verdict R2N "$?" "assembler WT tT4r2=$c1 branch -r2 lines=$c2 (EXPECT 1 2); fixup BRANCH -r2=$c3 W tT4r2=$c4 ES tT4emitr2=$c7 MTF stamps=$c8 (EXPECT 1 1 1 1); map WT tT4r2-map=$c5 X stamps=$c6 (EXPECT 1 1); assembler '\$X/tT4/' outside run2/stamps=$c9, W= / WT= lines not naming tT4r2=$c10, bare train-t4-union=$c11 (EXPECT 0 0 0)"

echo "T4-CONTROLS-R2 DONE failed=$FAILED (scratch: $OUT)"
[ "$FAILED" = 0 ]
