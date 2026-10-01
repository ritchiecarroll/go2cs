#!/usr/bin/env bash
# TRAIN L FIXUP on the i7 (COORD runs it). DRAFT 2026-10-01 -- NOT RUN. ONE signed commit 'fixup: TRAIN L -- ...' on
# the assembled union /h/go2cs-tmp-coord/tL (branch claude/coord-trainL-union: 11 signed seat merges onto TRAIN K's
# landed master 75648a022b, head 6960c8071f at assembly).
#
# WHY L OWES A FIXUP (the seat specs, measured read-only at 6960c8071f; the task's 'no fixup unless proven' default is
# overturned by these two facts, neither of which is a -stdlib emission footprint):
#  (F1) TWO behavioral csproj were emitted at J's template (f819887fa3) and never saw K's FIXUP ORDER:
#         src/tests/Behavioral/ForClauseSpill/ForClauseSpill.csproj                       (r-d6-for-clause-spill)
#         src/tests/Behavioral/CrossPackagePromotedValueMethod/CrossPackagePromotedValueMethod.csproj (i9-crosspkg)
#       Both carry <LangVersion>latest</LangVersion> and lack the commented OSR opt-in; optin.py check at the union reads
#       'CHECK FAIL: 2 problem(s)' naming exactly these two (1140 template descendants, 229 test hosts). The S1 COUNT at
#       K master is 0. Without this step CNR reads both CHANGED (it compares the csproj), the battery's PRE-1 S1 census
#       FAILS, and leg E's csprojdrift reads them new-MISSING-EDIT. g-deadlock's three new projects were emitted at K's
#       template and already carry both edits.
#  (F2) g-deadlock-checkdead (0377dbff39) added [GoTestMatchingConsoleOutput] to ChannelReceiveFromNil and
#       ChannelSendToNil but not their D4 rows: OutputComparisonTests.cs lists 716, 718 projects carry the attribute,
#       and src/go2cs's TestOutputComparisonListMatchesConsoleOutputAttribute FAILS on both names (leg C red).
#       RULED R1 (COORD, 2026-10-01): the two rows go HERE, at UpdateTestTargets' sorted positions (ordinal:
#       ChannelReceiveFromClosed < ChannelReceiveFromNil < ChannelRendezvous < ChannelSendToNil < ClearBuiltinShadow, read
#       at 6960c8071f), CRLF kept, and the commit message attributes them to the g-deadlock-checkdead seat. The draft's
#       OCT_REPAIR=seat route is withdrawn: any value other than 'fixup' is refused.
#  R2: the i9's nugetgo-pack.ps1 Windows PowerShell 5.1 parse fix may be merged into the union BEFORE this fixup: PRE
#      accepts 11 seat merges, or 12 when HEAD itself is a merge whose second parent is on origin/claude/i9-nugetgo-pack
#      (fetched before the run), descends from the seat 1f99ee7e9f and is not it. The subject stays 'fixup: TRAIN L'.
# Steps, every one gated on the previous one's rc (floor 7):
#   PRE  head/branch/ancestry/clean tree/GOROOT pin; the measured preconditions (exactly the F1 pair, the F2 gap)
#   1    S1 COUNT (== 2, derived twice: union-vs-base new csproj, and each seat's own commits), then K's S1 sed (sed -b)
#   2    optin.py insert: 'insert: 2 edited, 1138 already carried it, 1140 descendants'
#   3    optin.py check: CHECK PASS, 'check: 1140 template descendants, 229 test hosts, 1 hand-owned skipped'
#   4    the two D4 rows (R1: always here, attributed to g-deadlock-checkdead), CRLF kept; outparity OK; numstat 6/0
#   5    numstat shape: each csproj 4/1 (K's 'S1 + opt-in' shape); tL-helpers.py precheck rc=0 on the edited worktree
#   6    explicit git add of exactly those paths (never add -A, floor 8), git commit -S, verify-commit
# Nothing here pushes. On any ABORT the worktree edits made so far are listed with the exact command to discard them.
set -u
W=${W:-/h/go2cs-tmp-coord/tL}
BRANCH=claude/coord-trainL-union
EXPECT_HEAD=${EXPECT_HEAD:?the assembled union head (6960c8071f at assembly)}
OCT_REPAIR=${OCT_REPAIR:-fixup}
[ "$OCT_REPAIR" = fixup ] || { echo "ABORT: OCT_REPAIR=$OCT_REPAIR -- R1 rules the two D4 rows into this fixup (only 'fixup' is accepted)"; exit 2; }
BASE=75648a022b
SEATS_EXPECTED=11
F1A=src/tests/Behavioral/ForClauseSpill/ForClauseSpill.csproj
F1B=src/tests/Behavioral/CrossPackagePromotedValueMethod/CrossPackagePromotedValueMethod.csproj
OCT=src/tests/Behavioral/BehavioralTests/OutputComparisonTests.cs
OPT=docs/phase4/recipes/quickjit-optin/optin.py
EXP_DESC=1140   # template descendants at the union (optin.py check, read-only, 2026-10-01)
EXP_TH=229      # test hosts (same reading)
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tL-seats.txt}; [ -f "$SEATS" ] || SEATS=$SD/tL-seats-draft.txt
LOGDIR=$SD/tL-fixup-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
STEP=PRE
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
w(){ cygpath -w "$1"; }
die(){
  stamp "ABORT at step $STEP: $*"
  local ch; ch=$(git -C "$W" status --porcelain | grep -v '^??')
  [ -n "$ch" ] && stamp "  worktree edits so far (discard with: git -C $W restore --staged --worktree -- <paths>): $(echo "$ch" | tr '\n' ' ' | cut -c1-400)"
  exit 2
}
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
cd "$W" || die "no worktree $W"
HEADF=$(git rev-parse HEAD)
[ "${HEADF:0:10}" = "${EXPECT_HEAD:0:10}" ] || die "HEAD ${HEADF:0:10} != $EXPECT_HEAD"
[ "$(git symbolic-ref --short HEAD 2>/dev/null)" = "$BRANCH" ] || die "not on $BRANCH"
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || die "GOROOT pin $(go env GOROOT)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || die "tracked changes before the fixup"
git merge-base --is-ancestor "$BASE" HEAD || die "base $BASE not an ancestor"
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1)); git merge-base --is-ancestor "$sha" HEAD || die "seat $ref $sha not an ancestor"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
[ "$n" = "$SEATS_EXPECTED" ] || die "seat list has $n seats, expected $SEATS_EXPECTED"
fpc=$(git rev-list --first-parent --count "$BASE..HEAD")
# FOLLOW-UP SEAT MERGES (COORD 2026-10-01): after the 11 seat merges, up to TWO ruled follow-ups may ride, one each
# for i9-nugetgo-pack (R2, the 5.1 parse fix) and c1-token-ids (C1's token fix): a merge whose second parent is on that
# seat's origin branch, descends from the seated sha, is not it, and comes after the seat's own merge.
FOLLOW='i9-nugetgo-pack:1f99ee7e9f c1-token-ids:667d052869'
NGFIX=''; FIXES=''; nfix=0
for c in $(git rev-list --first-parent "$BASE..HEAD"); do
  [ "$(git rev-list --parents -n 1 "$c" | wc -w)" = 3 ] || die "first-parent commit ${c:0:10} is not a two-parent merge (a fixup already present?)"
  p2=$(git rev-parse "$c^2")
  grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | cut -d'|' -f2 | while read -r s; do git rev-parse "$s^{commit}"; done | grep -qxF "$p2" && continue
  fx=''
  for fr in $FOLLOW; do
    fref=refs/remotes/origin/claude/${fr%%:*}; fseat=$(git rev-parse "${fr##*:}^{commit}")
    git rev-parse --verify -q "$fref" > /dev/null || continue
    if [ "$p2" != "$fseat" ] && git merge-base --is-ancestor "$p2" "$fref" && git merge-base --is-ancestor "$fseat" "$p2" && git merge-base --is-ancestor "$fseat" "$c^1"; then fx=${fr%%:*}; break; fi
  done
  [ -n "$fx" ] || die "merge ${c:0:10} ^2=${p2:0:10} is neither a listed seat nor a ruled follow-up (fetch origin/claude/<seat> first if it is one)"
  nfix=$((nfix + 1)); FIXES="$FIXES $fx:${p2:0:10}"; [ "$fx" = i9-nugetgo-pack ] && NGFIX=$p2
done
[ "$fpc" = $((SEATS_EXPECTED + nfix)) ] || die "first-parent count $fpc != $SEATS_EXPECTED seat merges + $nfix follow-up(s): a stray commit, or a fixup already present"
SEATS_EFF="$LOGDIR/seats-effective.txt"
{ cat "$SEATS"; for f in $FIXES; do echo "${f%%:*}-followup|${f##*:}|ruled follow-up merge of ${f%%:*}"; done; } > "$SEATS_EFF"
stamp "PRE head=${HEADF:0:10} seats=$n first-parent=$fpc follow-ups=[${FIXES# }] OCT_REPAIR=$OCT_REPAIR $(go version | cut -d' ' -f3)"

# ------------------------------------------------------------------------------------ STEP 1: S1 COUNT, then the sed
STEP=1-S1
PS=('src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' 'src/tests/Performance/**/*.csproj')
OLD='<LangVersion>latest</LangVersion>'
NEW="<LangVersion Condition=\"'\$(LangVersion)'==''\">14</LangVersion>"
git grep -l -F "$OLD" "$BASE" -- "${PS[@]}" > "$LOGDIR/old-at-base.raw"; rc=$?
[ "$rc" = 1 ] || [ "$rc" = 0 ] || die "git grep at base rc=$rc"
[ "$(grep -c . "$LOGDIR/old-at-base.raw")" = 0 ] || die "K master already carries the old token in $(grep -c . "$LOGDIR/old-at-base.raw") csproj (expected 0)"
git diff --diff-filter=A --name-only "$BASE" HEAD -- '*.csproj' | LC_ALL=C sort > "$LOGDIR/newL-union.txt"
: > "$LOGDIR/newL-seats.raw"
while IFS='|' read -r ref sha desc; do
  mb=$(git merge-base "$BASE" "$sha")
  git log "$mb..$sha" --diff-filter=A --name-only --format= -- '*.csproj' >> "$LOGDIR/newL-seats.raw" || die "git log $ref"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS_EFF")
grep . "$LOGDIR/newL-seats.raw" | LC_ALL=C sort -u > "$LOGDIR/newL-seats.txt"
cmp -s "$LOGDIR/newL-union.txt" "$LOGDIR/newL-seats.txt" || die "new csproj: union diff vs seats' own commits disagree (newL-union.txt / newL-seats.txt)"
[ "$(wc -l < "$LOGDIR/newL-union.txt")" = 5 ] || die "L adds $(wc -l < "$LOGDIR/newL-union.txt") csproj, authored 5 (2 F1 + g-deadlock's 3)"
git grep -l -z -F "$OLD" -- "${PS[@]}" > "$LOGDIR/s1-list.z"; rc=$?
[ "$rc" = 0 ] || die "git grep (COUNT) rc=$rc"
tr '\0' '\n' < "$LOGDIR/s1-list.z" | LC_ALL=C sort > "$LOGDIR/s1-list.txt"
printf '%s\n' "$F1B" "$F1A" | LC_ALL=C sort > "$LOGDIR/s1-expected.txt"
cmp -s "$LOGDIR/s1-list.txt" "$LOGDIR/s1-expected.txt" || die "S1 COUNT list is not exactly the F1 pair (s1-list.txt)"
for f in "$F1A" "$F1B"; do
  echo "$f crlf=$(grep -c $'\r$' "$f") lines=$(wc -l < "$f") old=$(grep -cF "$OLD" "$f")" >> "$LOGDIR/s1-eol-before.txt"
done
xargs -0 sed -b -i "s|$OLD|$NEW|" < "$LOGDIR/s1-list.z" > "$LOGDIR/s1-sed.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "S1 sed rc=$rc"
for f in "$F1A" "$F1B"; do
  b=$(grep -F "$f " "$LOGDIR/s1-eol-before.txt"); crl=$(grep -c $'\r$' "$f"); lin=$(wc -l < "$f")
  [ "$(grep -cF "$OLD" "$f")" = 0 ] && [ "$(grep -cF "$NEW" "$f")" = 1 ] || die "$f: token counts after the sed"
  [ "$b" = "$f crlf=$crl lines=$lin old=1" ] || die "$f: line endings/count moved ($b -> crlf=$crl lines=$lin)"
done
stamp "1 S1: COUNT=2 (K master 0 + the F1 pair; L adds 5 csproj, g-deadlock's 3 already conditioned); sed -b, line endings kept"

# --------------------------------------------------------------------------------------- STEP 2: optin.py insert
STEP=2-optin
python "$(w "$W/$OPT")" insert "$(w "$W")" > "$LOGDIR/optin-insert.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "optin.py insert rc=$rc"
got=$(grep -a '^insert:' "$LOGDIR/optin-insert.log" | tr -d '\r')
[ "$got" = "insert: 2 edited, $((EXP_DESC - 2)) already carried it, $EXP_DESC descendants" ] || die "optin insert read '$got'"
stamp "2 optin insert: $got"

# --------------------------------------------------------------------------------------- STEP 3: optin.py check
STEP=3-optin-check
python "$(w "$W/$OPT")" check "$(w "$W")" > "$LOGDIR/optin-check.log" 2>&1; rc=$?
[ "$rc" = 0 ] && grep -aqx 'CHECK PASS' <(tr -d '\r' < "$LOGDIR/optin-check.log") || die "optin.py check rc=$rc / no CHECK PASS"
got3=$(grep -a '^check:' "$LOGDIR/optin-check.log" | tr -d '\r')
[ "$got3" = "check: $EXP_DESC template descendants, $EXP_TH test hosts, 1 hand-owned skipped" ] || die "optin check read '$got3'"
stamp "3 optin check: CHECK PASS ($got3)"

# ------------------------------------------------------------------- STEP 4: the two D4 rows (OCT_REPAIR=fixup)
STEP=4-oct
if [ "$OCT_REPAIR" = fixup ]; then
  python - "$(w "$W/$OCT")" > "$LOGDIR/oct.log" 2>&1 <<'PY'; rc=$?
import sys
p = sys.argv[1]
b = open(p, 'rb').read(); assert b.count(b'\r\n') == b.count(b'\n'), 'not uniformly CRLF'
L = b.decode('utf-8').split('\r\n')
def after(anchor_name, new_name):
    k = L.index(f'    public void Check{anchor_name}() => CheckTarget("{anchor_name}");')
    assert L[k + 1] == '' and L[k - 1] == '    [TestMethod]', anchor_name
    L[k + 2:k + 2] = ['    [TestMethod]', f'    public void Check{new_name}() => CheckTarget("{new_name}");', '']
for a, n in (('ChannelRendezvous', 'ChannelSendToNil'), ('ChannelReceiveFromClosed', 'ChannelReceiveFromNil')):
    assert f'CheckTarget("{n}")' not in '\n'.join(L), n + ' already listed'
    after(a, n)
open(p, 'wb').write('\r\n'.join(L).encode('utf-8'))
print('OCT inserted ChannelReceiveFromNil after ChannelReceiveFromClosed, ChannelSendToNil after ChannelRendezvous')
PY
  [ "$rc" = 0 ] || die "OCT insert rc=$rc ($(tail -n 1 "$LOGDIR/oct.log"))"
  ns=$(git diff --numstat -- "$OCT" | cut -f1-2 | tr '\t' '/')
  [ "$ns" = 6/0 ] || die "$OCT numstat $ns, expected 6/0"
else
  stamp "4 OCT: OCT_REPAIR=$OCT_REPAIR -- not edited here; the union must already carry the repair (checked next)"
fi
python "$(w "$SD/tL-helpers.py")" outparity "$(w "$W")" > "$LOGDIR/outparity.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "outparity: $(tr -d '\r' < "$LOGDIR/outparity.log" | head -n 1)"
stamp "4 OCT: $(tr -d '\r' < "$LOGDIR/outparity.log" | head -n 1)"

# ------------------------------------------------------------------------------- STEP 5: numstat shape + precheck
STEP=5-shape
for f in "$F1A" "$F1B"; do
  ns=$(git diff --numstat -- "$f" | cut -f1-2 | tr '\t' '/')
  [ "$ns" = 4/1 ] || die "$f numstat $ns, expected 4/1 (S1 + opt-in)"
done
python "$(w "$SD/tL-helpers.py")" precheck "$(w "$W")" "$BASE" "$(w "$SEATS_EFF")" > "$LOGDIR/precheck.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "precheck rc=$rc: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 3 | tr '\n' ' ')"
stamp "5 shape: F1 pair 4/1 each; precheck $(grep -a '^PRECHECK' "$LOGDIR/precheck.log")"

# -------------------------------------------------------------------------------- STEP 6: explicit add, signed commit
STEP=6-commit
{ echo "$F1A"; echo "$F1B"; [ "$OCT_REPAIR" = fixup ] && echo "$OCT"; } | LC_ALL=C sort > "$LOGDIR/expected.txt"
git diff --name-only | LC_ALL=C sort > "$LOGDIR/changed.txt"
cmp -s "$LOGDIR/expected.txt" "$LOGDIR/changed.txt" || die "changed set != expected (expected.txt / changed.txt)"
[ "$(git status --porcelain | grep -c '^ D')" = 0 ] || die "tracked deletions (floor 8)"
tr '\n' '\0' < "$LOGDIR/expected.txt" > "$LOGDIR/add.z"
git --literal-pathspecs add --pathspec-from-file="$(w "$LOGDIR/add.z")" --pathspec-file-nul || die "git add"
git diff --cached --name-only | LC_ALL=C sort | cmp -s - "$LOGDIR/expected.txt" || die "staged set != expected"
cat > "$LOGDIR/commit-msg.txt" <<EOF
fixup: TRAIN L -- S1 + OSR opt-in on the 2 behavioral csproj cut at J's template$([ "$OCT_REPAIR" = fixup ] && echo "; the 2 D4 output-comparison rows g-deadlock owed")

On the assembled union ${HEADF:0:10} ($SEATS_EXPECTED seat merges$([ -n "$FIXES" ] && echo " + follow-up merges:$FIXES") onto TRAIN K's landed master $BASE):
(1) S1 (K's FIXUP ORDER recipe): exactly 2 csproj still carried <LangVersion>latest</LangVersion> -- ForClauseSpill
    (r-d6-for-clause-spill) and CrossPackagePromotedValueMethod (i9-crosspkg-value-promotion), both emitted at
    f819887fa3's template; K master carries 0. The sed to the conditioned 14 (sed -b, line endings kept).
(2) optin.py insert: $got.
(3) optin.py check: CHECK PASS ($got3). Each csproj reads 4/1 (S1 + opt-in).
$([ "$OCT_REPAIR" = fixup ] && printf '%s\n%s\n%s\n%s' "(4) Seat: g-deadlock-checkdead (0377dbff39). OutputComparisonTests.cs gains CheckChannelReceiveFromNil and" "    CheckChannelSendToNil at UpdateTestTargets' sorted positions, CRLF kept. The seat added" "    [GoTestMatchingConsoleOutput] to both package_info.cs files without the rows, so" "    TestOutputComparisonListMatchesConsoleOutputAttribute read 718 marked vs 716 listed (COORD ruling R1).")
The battery's leg E (M=$BASE vs this commit) reads the csproj as template-only drift.

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
git commit -q -S -F "$(w "$LOGDIR/commit-msg.txt")" || die "git commit -S"
NEWH=$(git rev-parse HEAD)
[ "$(git rev-parse HEAD^)" = "$HEADF" ] || die "fixup parent is not the assembled head"
git verify-commit HEAD > "$LOGDIR/verify-commit.log" 2>&1 || die "verify-commit failed"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || die "tracked changes remain after the commit"
stamp "6 COMMITTED ${NEWH:0:10} $(git show --shortstat --format= HEAD | tr -d '\n' | sed 's/^ *//')"
stamp "FIXUP DONE head=${NEWH:0:10} (the battery's EXPECT_HEAD; nothing pushed)"
