#!/usr/bin/env bash
# TRAIN K FIXUP on the i7 (COORD runs it). ONE signed commit 'fixup: TRAIN K -- ...' on the assembled union
# /h/go2cs-tmp-coord/tK (branch claude/coord-trainK-union: 35 signed seat merges onto TRAIN J's landed master f819887fa3).
# Launch from a PER-RUN COPY (floor 4): copy this directory (tK-fixup.sh, tK-seats-draft.txt, tK-s1census.py,
# tK-countassert.py, tK-k6check.py, tK-postasserts.sh) to a run folder and run the copy; logs land beside it.
#   EXPECT_HEAD=<assembled union head> bash tK-fixup.sh
#
# It implements tK-seats-draft.txt's rulings, in this order, every step gated on the previous one's rc (floor 7: each
# rc is captured before any pipe; every pipeline that matters reads PIPESTATUS):
#   PRE  head/branch/ancestry/clean-tree/GOROOT pin, and the K8/K9 post-asserts (tK-postasserts.sh) as a precondition
#   1    '# FIXUP ORDER' (1): P1's S1 COUNT, then the S1 sed                     (seat p1-s1-dotnet-pin 272afeef93)
#   2    '# FIXUP ORDER' (2): C2's optin.py insert                               (seat c2-osr-optin 9fd1737e84)
#   3    '# FIXUP ORDER' (3): S1's (1,0,1) completeness census                   (tK-s1census.py)
#   4    '# FIXUP ORDER' (4): optin.py check = CHECK PASS, then the per-file numstat shape of every edited csproj
#   5    PRERES-K6: os/{linux,darwin}/file_unix.cs refreshed from the UNION converter's seeded emission (one
#        [StackTraceHidden] hunk each, by name; tK-k6check.py refuses anything else)
#   6    PRERES-K1 + K3 count asserts (go2cs-src.projitems; go2cs.slnx + the 4 BehavioralTests registration files)
#   7    PRERES-K2 token asserts on src/push-nuget.ps1 (both seats' lines present, none doubled)
#   8    explicit git add of exactly the paths steps 1, 2 and 5 changed (never add -A, floor 8), git commit -S
# '# FIXUP ORDER' (5) -- the emission check's M-vs-U arms showing both csproj edits as expected template drift only --
# is the TRAIN K battery's leg E, run at THIS script's commit; it is not repeated here.
# NOT HERE: the REVIEW-SIBLING refresh (ruling 857afcb47e; TRAIN J's PRERES.txt 'FIXUP DUTY': refresh every *.cs.auto in
# the emission check's U-arm list from the U arm, after its CONTROL lines read OK). Its input is leg E's output, which
# exists only after this commit, and PRE refuses a HEAD that is not a seat merge, so this script cannot run again for
# it. Where it lives for TRAIN K is COORD's OPEN ruling: (a) a separate fixup2 step after leg E, gated on leg E's U-arm
# list and CONTROL OK lines, that the battery then accepts as HEAD; (b) the emission check run first and the refresh
# folded into this commit; (c) ruled out for K in the ledger. No map regeneration is owed: PRERES-K9 ruled a by-line
# resolution at assembly, which tK-postasserts.sh asserts.
# Nothing here pushes. On any ABORT the worktree edits made so far are listed with the exact command to discard them.
set -u
W=${W:-/h/go2cs-tmp-coord/tK}   # override ONLY for a rehearsal on a scratch clone (see REHEARSE)
REHEARSE=${REHEARSE:-0}          # 1 = steps PRE..4, 6, 7 only: no K6 conversion, no add, no commit (a dry run of the
                                 # csproj recipe and the asserts; its edits stay in W's worktree to inspect or discard)
BRANCH=claude/coord-trainK-union
EXPECT_HEAD=${EXPECT_HEAD:?the assembled union head}
BASE=f819887fa3          # TRAIN J's landed master = TRAIN K's base
S1SEAT=272afeef93        # p1-s1-dotnet-pin
OPTSEAT=9fd1737e84       # c2-osr-optin
SEATS_EXPECTED=35
# AUTHORED second derivations (read at the seats' own commits, 2026-09-30); each is re-derived at run time and the two
# must agree -- a disagreement means the seat list or a seat SHA moved since this script was written.
P1_BASE_S1=1358          # P1's COUNT at f819887fa3 + S1 (inbox COORD 20260930T102227Z-P1)
NEWK_AUTH=7              # csproj K's seats add: 6 behavioral guards + src/tools/j0-uuid-rehearsal/consumer
NEWK_S1_AUTH=6           # ... of which under the S1 pathspecs and carrying <LangVersion>latest</LangVersion>
C2_BASE_OPT=1129         # C2's template-descendant count at f819887fa3 (773 behavioral + 341 src/core + 15 perf)
NEWK_OPT_AUTH=6          # ... K's new template descendants (the 6 behavioral guards)
CENSUS_TOTAL_AUTH=1378   # tracked csproj outside src/archived and docs: 1371 at f819887fa3 + 7
CENSUS_101_AUTH=1373     # P1's 1,367 (1,0,1) at f819887fa3 + S1, + 6
# K6 scratch: the converter build and two seeded roots (outside the worktree, on H:). Its override is a DISTINCTIVE
# name, never a generic one a caller's shell might export: the path is rm -rf'd, and k6_safe() refuses anything but a
# leaf named tKk6 under /h/go2cs-tmp-coord/ (Git Bash spelling) that is neither the worktree nor an ancestor of it.
K6D=${TK_K6_SCRATCH:-/h/go2cs-tmp-coord/tKk6}
KEEP_K6=${KEEP_K6:-0}            # 1 = keep the K6 scratch after an ABORT (to inspect the seeded roots); default removes it
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=$SD/tK-seats-draft.txt
LOGDIR=$SD/tK-fixup-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
STEP=PRE
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
k6_safe(){
  case "$K6D" in /h/go2cs-tmp-coord/tKk6|/h/go2cs-tmp-coord/?*/tKk6) ;; *) return 1 ;; esac
  case "$K6D" in *//*|*/./*|*/../*) return 1 ;; esac
  local wa; wa=$(cd "$W" 2>/dev/null && pwd -P) || wa=$W
  case "${wa%/}/" in "$K6D"/*) return 1 ;; esac       # K6D is W itself, or an ancestor of W
  return 0
}
k6_rm(){ k6_safe && rm -rf "$K6D"; }
die(){
  stamp "ABORT [$STEP]: $*"
  if [ -n "${HEADF:-}" ] && [ -d "$W" ] && [ "$(git -C "$W" rev-parse HEAD 2>/dev/null)" != "$HEADF" ]; then
    stamp "  HEAD moved past the assembled head ${HEADF:0:10}: a fixup commit $(git -C "$W" rev-parse --short=10 HEAD 2>/dev/null) exists (unpushed); COORD decides whether it stands"
  fi
  if [ -d "$W" ]; then
    # HEAD vs the WORKTREE, so edits already STAGED by step 8 are listed too (a failed commit -S leaves them staged)
    git -C "$W" diff HEAD --name-only -z > "$LOGDIR/touched.z" 2>/dev/null
    if [ -s "$LOGDIR/touched.z" ]; then
      stamp "  worktree edits so far: $(tr -cd '\0' < "$LOGDIR/touched.z" | wc -c) file(s), listed NUL-separated in $LOGDIR/touched.z"
      stamp "  to discard them: git -C $W restore --source=HEAD --staged --worktree --pathspec-from-file='$(cygpath -w "$LOGDIR/touched.z")' --pathspec-file-nul"
    fi
  fi
  if [ "$KEEP_K6" = 1 ]; then stamp "  K6 scratch kept (KEEP_K6=1): $K6D"
  elif [ -d "$K6D" ]; then k6_rm && stamp "  K6 scratch removed: $K6D" || stamp "  K6 scratch NOT removed (k6_safe refused '$K6D')"; fi
  exit 3
}
# floor 7 for git reads whose output is then COUNTED: write to a file, capture rc, die on nonzero -- a failed git must
# never read as 0 lines / an empty list = a pass.
gitf(){ local out=$1; shift; git "$@" > "$out"; local r=$?; [ "$r" = 0 ] || die "git $* rc=$r"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
w(){ cygpath -w "$1"; }

# The build shell, exactly as TRAIN J's battery sets it up: go1.24.13 from C: (GOROOT on the ReFS H: deadlocks
# runtime's TestTracebackSystem panic child), its BACKSLASH spelling (floor 6), dotnet10 on PATH, caches and temp on H:.
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"

# ------------------------------------------------------------------------------------------------------------ PRE
for f in tK-seats-draft.txt tK-s1census.py tK-countassert.py tK-k6check.py tK-postasserts.sh; do
  [ -f "$SD/$f" ] || die "the per-run copy lacks $f"
done
cd "$W" || die "no worktree $W"
[ "$(git symbolic-ref --short HEAD 2>/dev/null)" = "$BRANCH" ] || die "not on $BRANCH"
HEADF=$(git rev-parse HEAD)
[ "$HEADF" = "$(git rev-parse --verify -q "$EXPECT_HEAD^{commit}")" ] || die "HEAD ${HEADF:0:10} != EXPECT_HEAD $EXPECT_HEAD"
[ "$(git rev-list --parents -n 1 HEAD | wc -w)" = 3 ] || die "HEAD is not a two-parent seat merge (a fixup already on top?)"
git log --format=%s "$BASE..HEAD" > "$LOGDIR/subjects.txt"; rc=$?
[ "$rc" = 0 ] || die "git log rc=$rc"
grep -q '^fixup: TRAIN K' "$LOGDIR/subjects.txt" && die "a TRAIN K fixup commit already exists on the branch"
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1))
  git merge-base --is-ancestor "$sha" HEAD || die "seat $ref $sha is not an ancestor"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
[ "$n" = "$SEATS_EXPECTED" ] || die "the seat list has $n seat lines, expected $SEATS_EXPECTED"
gitf "$LOGDIR/status-pre.txt" status --porcelain
[ "$(grep -vc '^??' "$LOGDIR/status-pre.txt")" = 0 ] || die "tracked changes before the fixup"
# Signing preflight BEFORE the long K6 conversions (step 8 commits with -S; a failure there would come last).
SIGNFMT=$(git config --get gpg.format || true); SIGNKEY=$(git config --get user.signingkey || true)
[ -n "$SIGNKEY" ] || die "no user.signingkey: the step-8 commit -S cannot sign"
case "${SIGNFMT:-openpgp}" in
  openpgp)
    gp=$(git config --get gpg.program || echo gpg); case "$gp" in ?:*) gp=$(cygpath -u "$gp") ;; esac
    "$gp" --batch --list-secret-keys "$SIGNKEY" > /dev/null 2>&1; rc=$?
    [ "$rc" = 0 ] || die "the signing key is not in the secret keyring (gpg rc=$rc)"
    if [ "${SIGN_PROBE:-1}" = 1 ]; then  # a real signature through the agent; SIGN_PROBE=0 skips it
      echo "tK-fixup signing probe" | "$gp" --batch --yes --local-user "$SIGNKEY" --clearsign > "$LOGDIR/sign-probe.asc" 2> "$LOGDIR/sign-probe.err"; rc=$?
      [ "$rc" = 0 ] || die "the signing probe failed (gpg rc=$rc, sign-probe.err): unlock the agent, then rerun"
    fi ;;
  *) stamp "PRE signing format '$SIGNFMT': key configured, no agent probe for this format" ;;
esac
f=$(freegb); [ "${f:-0}" -ge 30 ] || die "free disk ${f}G < 30G on H:"
c=$(cfree); [ "${c:-0}" -ge 8 ] || die "C: free ${c}G < 8G"
k6_safe || die "refusing K6 scratch '$K6D': it must be /h/go2cs-tmp-coord/[<dir>/]tKk6 and neither W nor an ancestor of W"
k6_rm; mkdir -p "$K6D/nomod" || die "cannot create $K6D"
# The pin, asserted from a NO-MODULE directory in this run's own environment (converter.md: a go.mod can re-exec a
# different toolchain and rewrite GOROOT).
gr=$(cd "$K6D/nomod" && go env GOROOT); gv=$(cd "$K6D/nomod" && go version); gm=$(cd "$K6D/nomod" && go env GOMOD)
[ "$gr" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || die "GOROOT pin reads '$gr'"
case "$gv" in *go1.24.13*) ;; *) die "go version reads '$gv'";; esac
case "$gm" in ""|NUL|/dev/null) ;; *) die "the no-module directory is inside a module ($gm)";; esac
bash "$SD/tK-postasserts.sh" "$W" > "$LOGDIR/postasserts-pre.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "K8/K9 post-asserts rc=$rc (log postasserts-pre.log): the assembled union is not the tree the fixup was written for"
stamp "PRE head=${HEADF:0:10} tree=$(git rev-parse --short=10 HEAD^{tree}) seats=$n $gv GOROOT=$gr free=${f}G C=${c}G postasserts=PASS"

# ------------------------------------------------------------------------------------ STEP 1: S1 COUNT, then the sed
STEP=1-S1-count
PS=('src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' 'src/tests/Performance/**/*.csproj')   # QUOTED: git expands them, not bash
OLD='<LangVersion>latest</LangVersion>'
NEW="<LangVersion Condition=\"'\$(LangVersion)'==''\">14</LangVersion>"
# K's new projects, derived twice: from the union against its base, and from each seat's own commits.
git diff --diff-filter=A --name-only "$BASE" HEAD -- '*.csproj' > "$LOGDIR/newk-union.raw"; rc=$?
[ "$rc" = 0 ] || die "git diff (new csproj) rc=$rc"
LC_ALL=C sort "$LOGDIR/newk-union.raw" > "$LOGDIR/newk-union.txt"
: > "$LOGDIR/newk-seats.raw"
while IFS='|' read -r ref sha desc; do
  git log "$BASE..$sha" --diff-filter=A --name-only --format= -- '*.csproj' >> "$LOGDIR/newk-seats.raw" || die "git log $ref rc!=0"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
grep . "$LOGDIR/newk-seats.raw" | LC_ALL=C sort -u > "$LOGDIR/newk-seats.txt"
cmp -s "$LOGDIR/newk-union.txt" "$LOGDIR/newk-seats.txt" || die "new csproj: the union's diff and the seats' own commits disagree (newk-union.txt vs newk-seats.txt)"
gitf "$LOGDIR/gone.txt" diff --diff-filter=DR --name-only "$BASE" HEAD -- '*.csproj'
gone=$(grep -c . "$LOGDIR/gone.txt")
[ "$gone" = 0 ] || die "$gone csproj deleted or renamed by a seat: unruled for the S1 census"
NEWK=$(wc -l < "$LOGDIR/newk-union.txt")
git grep -l -F "$OLD" HEAD -- "${PS[@]}" > "$LOGDIR/old-at-head.raw"; rc=$?
[ "$rc" = 0 ] || die "git grep (old token at HEAD) rc=$rc"
NEWK_S1=$(sed 's/^HEAD://' "$LOGDIR/old-at-head.raw" | grep -cxF -f "$LOGDIR/newk-union.txt")
git grep -l -F "$OLD" "$BASE" -- "${PS[@]}" > "$LOGDIR/old-at-base.raw"; rc=$?
[ "$rc" = 0 ] || die "git grep (old token at base) rc=$rc"
git diff --name-only "$S1SEAT^" "$S1SEAT" -- "${PS[@]}" > "$LOGDIR/s1-own.raw"; rc=$?
[ "$rc" = 0 ] || die "git diff (S1's own csproj) rc=$rc"
BASE_S1=$(( $(wc -l < "$LOGDIR/old-at-base.raw") - $(wc -l < "$LOGDIR/s1-own.raw") ))
[ "$NEWK" = "$NEWK_AUTH" ] || die "K adds $NEWK csproj, authored $NEWK_AUTH ($(tr '\n' ' ' < "$LOGDIR/newk-union.txt"))"
[ "$NEWK_S1" = "$NEWK_S1_AUTH" ] || die "K adds $NEWK_S1 csproj the S1 sed reaches, authored $NEWK_S1_AUTH"
[ "$BASE_S1" = "$P1_BASE_S1" ] || die "base count at f819887fa3+S1 derives as $BASE_S1, P1 read $P1_BASE_S1"
EXP_S1=$((BASE_S1 + NEWK_S1))
git grep -l -z -F "$OLD" -- "${PS[@]}" > "$LOGDIR/s1-list.z"; rc=$?
[ "$rc" = 0 ] || die "git grep (the COUNT) rc=$rc"
COUNT=$(tr -cd '\0' < "$LOGDIR/s1-list.z" | wc -c)
tr '\0' '\n' < "$LOGDIR/s1-list.z" | LC_ALL=C sort > "$LOGDIR/s1-list.txt"
[ "$COUNT" = "$EXP_S1" ] || die "S1 COUNT $COUNT != expected $EXP_S1 (= $BASE_S1 at f819887fa3+S1 + $NEWK_S1 new K)"
miss=$(sed 's/^HEAD://' "$LOGDIR/old-at-head.raw" | grep -xF -f "$LOGDIR/newk-union.txt" | grep -cvxF -f "$LOGDIR/s1-list.txt")
[ "$miss" = 0 ] || die "$miss new K project(s) missing from the COUNT list"
stamp "1 S1 COUNT=$COUNT (= P1's $BASE_S1 at f819887fa3+S1 + $NEWK_S1 new K; K adds $NEWK csproj in all: $(tr '\n' ' ' < "$LOGDIR/newk-union.txt"))"
STEP=1-S1-sed
python "$SD/tK-s1census.py" eolsnap "$(w "$W")" "$(w "$LOGDIR/s1-list.z")" "$(w "$LOGDIR/s1-eol.json")" > "$LOGDIR/s1-eolsnap.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "eolsnap rc=$rc"
# P1's sed, verbatim, plus -b: Git Bash's GNU sed 4.8 opens files in TEXT mode and 'sed -i' silently rewrites CRLF to LF
# (measured 2026-09-30); P1's dry run was on linux, where the two are the same. -b keeps every byte but the token.
xargs -0 sed -b -i "s|$OLD|$NEW|" < "$LOGDIR/s1-list.z" > "$LOGDIR/s1-sed.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "the S1 sed rc=$rc (s1-sed.log)"
python "$SD/tK-s1census.py" eolcheck "$(w "$W")" "$(w "$LOGDIR/s1-list.z")" "$(w "$LOGDIR/s1-eol.json")" > "$LOGDIR/s1-eolcheck.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "after the sed: line endings or token counts off (s1-eolcheck.log)"
stamp "1 S1 sed: $COUNT files, old token 0x / new token 1x in each, line endings unchanged ($(tail -n 1 "$LOGDIR/s1-eolcheck.log" | tr -d '\r'))"

# --------------------------------------------------------------------------------------- STEP 2: optin.py insert
STEP=2-optin-insert
OPT=docs/phase4/recipes/quickjit-optin/optin.py
MAIN_SIG='Enable native compiled output optimizations'
TEST_SIG='The test project shares its directory with the production project'
[ -f "$OPT" ] || die "no $OPT in the union"
[ "$(git rev-parse "HEAD:$OPT")" = "$(git rev-parse "$OPTSEAT:$OPT")" ] || die "$OPT differs from the seat's ($OPTSEAT)"
git grep -l -F "$MAIN_SIG" "$BASE" -- '*.csproj' > "$LOGDIR/opt-base.raw"; rc=$?
[ "$rc" = 0 ] || die "git grep (template descendants at base) rc=$rc"
BASE_OPT=$(grep -cv ":src/core/golib/golib.csproj$" "$LOGDIR/opt-base.raw")
git grep -l -F "$MAIN_SIG" HEAD -- '*.csproj' > "$LOGDIR/opt-head.raw"; rc=$?
[ "$rc" = 0 ] || die "git grep (template descendants at HEAD) rc=$rc"
NEWK_OPT=$(sed 's/^HEAD://' "$LOGDIR/opt-head.raw" | grep -cxF -f "$LOGDIR/newk-union.txt")
[ "$BASE_OPT" = "$C2_BASE_OPT" ] || die "template descendants at base derive as $BASE_OPT, C2 read $C2_BASE_OPT"
[ "$NEWK_OPT" = "$NEWK_OPT_AUTH" ] || die "K adds $NEWK_OPT template descendants, authored $NEWK_OPT_AUTH"
EXP_OPT=$((BASE_OPT + NEWK_OPT))
python "$(w "$W/$OPT")" insert "$(w "$W")" > "$LOGDIR/optin-insert.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "optin.py insert rc=$rc (optin-insert.log)"
got=$(grep -a '^insert:' "$LOGDIR/optin-insert.log" | tr -d '\r')
# S1 is a sed, not a re-emission, so nothing carries the block yet: insert does ALL the work here (FIXUP ORDER (2)).
[ "$got" = "insert: $EXP_OPT edited, 0 already carried it, $EXP_OPT descendants" ] || die "optin.py insert read '$got', expected $EXP_OPT edited / 0 already / $EXP_OPT descendants"
stamp "2 optin insert: $got (= C2's $BASE_OPT at f819887fa3 + $NEWK_OPT new K)"

# ------------------------------------------------------------------------------- STEP 3: S1 completeness census
STEP=3-S1-census
# The census fails CLOSED on any (0,0,0) file the '# S1 REGENERATION' ruling does not name -- including K's own
# hand-written j0 consumer project -- until COORD amends a '# S1 REGENERATION' line of tK-seats-draft.txt to name it,
# or sets K_J0_EXCEPTION=1 for this run. The census prints which authority admitted it.
python "$SD/tK-s1census.py" census "$(w "$W")" "$(w "$SEATS")" > "$LOGDIR/s1-census.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "the (1,0,1) census rc=$rc (s1-census.log): $(grep -a 'FAIL' "$LOGDIR/s1-census.log" | head -n 3 | tr '\r\n' '  ')"
tally=$(grep -a '^CENSUS tally' "$LOGDIR/s1-census.log" | tr -d '\r')
ctot=$(echo "$tally" | sed -n 's/.*files=\([0-9]*\).*/\1/p'); c101=$(echo "$tally" | sed -n 's/.*(1,0,1)=\([0-9]*\).*/\1/p')
BASE_TOTAL=$(git ls-tree -r --name-only "$BASE" | grep -E '\.csproj$' | grep -cvE '^(src/archived/|docs/)')
[ "$ctot" = "$((BASE_TOTAL + NEWK))" ] && [ "$ctot" = "$CENSUS_TOTAL_AUTH" ] || die "census read $ctot files, expected $((BASE_TOTAL + NEWK)) (authored $CENSUS_TOTAL_AUTH)"
[ "$c101" = "$CENSUS_101_AUTH" ] || die "census (1,0,1)=$c101, authored $CENSUS_101_AUTH"
stamp "3 S1 census PASS: $tally $(grep -a 'K-EXCEPTION' "$LOGDIR/s1-census.log" | tr -d '\r' | sed 's/^ *//' | tr '\n' ' ')"

# -------------------------------------------------------------------- STEP 4: optin.py check, then numstat shape
STEP=4-optin-check
comm -23 <(git grep -l -F "$TEST_SIG" "$BASE" -- '*.csproj' | LC_ALL=C sort) <(LC_ALL=C sort "$LOGDIR/opt-base.raw") > "$LOGDIR/th-base.txt"
EXP_TH=$(( $(wc -l < "$LOGDIR/th-base.txt") + $(git grep -l -F "$TEST_SIG" HEAD -- '*.csproj' | sed 's/^HEAD://' | grep -cxF -f "$LOGDIR/newk-union.txt") ))
python "$(w "$W/$OPT")" check "$(w "$W")" > "$LOGDIR/optin-check.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "optin.py check rc=$rc (optin-check.log)"
grep -aqx 'CHECK PASS' <(tr -d '\r' < "$LOGDIR/optin-check.log") || die "optin.py check did not print CHECK PASS"
got=$(grep -a '^check:' "$LOGDIR/optin-check.log" | tr -d '\r')
[ "$got" = "check: $EXP_OPT template descendants, $EXP_TH test hosts, 1 hand-owned skipped" ] || die "optin.py check read '$got' (expected $EXP_OPT / $EXP_TH / 1)"
# Every edited csproj has exactly one of three shapes: S1 only 1/1, S1 + opt-in 4/1, or opt-in only 3/0 -- the last is
# a template descendant S1 converted BY HAND in its own seat, so the sed never saw it (at authoring: exactly one,
# src/core/unsafe/unsafe.csproj, hand-owned and inside C2's measured 1129). Derived here, not assumed.
sed 's/^HEAD://' "$LOGDIR/opt-head.raw" | grep -vx 'src/core/golib/golib.csproj' | LC_ALL=C sort | LC_ALL=C comm -23 - "$LOGDIR/s1-list.txt" > "$LOGDIR/optonly-expected.txt"
git diff --numstat -- '*.csproj' > "$LOGDIR/csproj-numstat.txt"; rc=$?
[ "$rc" = 0 ] || die "git diff --numstat rc=$rc"
shape=$(awk -F'\t' -v oo="$LOGDIR/optonly-got.raw" 'NR==FNR { s1[$0] = 1; next }
  { k = $1 "/" $2; i = ($3 in s1)
    if (i && k == "1/1") a++; else if (i && k == "4/1") b++; else if (!i && k == "3/0") { c++; print $3 > oo }
    else { bad++; print "BAD " k " " $3 > "/dev/stderr" } }
  END { printf "%d %d %d %d", a + 0, b + 0, c + 0, bad + 0 }' "$LOGDIR/s1-list.txt" "$LOGDIR/csproj-numstat.txt" 2> "$LOGDIR/csproj-shape.err")
set -- $shape; s_a=${1:-x}; s_b=${2:-x}; s_c=${3:-x}; s_bad=${4:-x}
[ "$s_bad" = 0 ] || die "$s_bad csproj with an unexpected numstat shape (csproj-shape.err)"
[ "$((s_a + s_b))" = "$COUNT" ] || die "S1 files in the diff $((s_a + s_b)) != COUNT $COUNT"
[ "$((s_b + s_c))" = "$EXP_OPT" ] || die "opt-in files in the diff $((s_b + s_c)) != $EXP_OPT"
touch "$LOGDIR/optonly-got.raw"; LC_ALL=C sort "$LOGDIR/optonly-got.raw" > "$LOGDIR/optonly-got.txt"
cmp -s "$LOGDIR/optonly-expected.txt" "$LOGDIR/optonly-got.txt" || die "the opt-in-only files are not exactly the hand-converted template descendants (optonly-expected.txt vs optonly-got.txt)"
awk -F'\t' '$1 "/" $2 == "4/1" || $1 "/" $2 == "3/0" { print $3 }' "$LOGDIR/csproj-numstat.txt" | LC_ALL=C sort > "$LOGDIR/opt-list.txt"
stamp "4 optin check: CHECK PASS ($got); numstat S1-only 1/1=$s_a, S1+opt-in 4/1=$s_b, opt-in-only 3/0=$s_c ($(tr '\n' ' ' < "$LOGDIR/optonly-got.txt")), other=0"

# ------------------------------------------------------------ STEP 5: PRERES-K6, file_unix.cs from the union converter
STEP=5-K6
if [ "$REHEARSE" = 1 ]; then stamp "5 K6 SKIPPED (REHEARSE=1)"; else
f=$(freegb); [ "${f:-0}" -ge 30 ] || die "free disk ${f}G < 30G on H: before the K6 conversions"
mkdir -p "$K6D/src-U" || die "mkdir $K6D/src-U"
git archive HEAD src/go2cs | tar -x -C "$K6D/src-U"; ps=("${PIPESTATUS[@]}")
[ "${ps[0]}" = 0 ] && [ "${ps[1]}" = 0 ] || die "archive of the union's src/go2cs rc=${ps[*]}"
( cd "$K6D/src-U/src/go2cs" && go build -o "$K6D/go2cs-U.exe" . ) > "$LOGDIR/k6-conv-build.log" 2>&1; rc=$?
[ "$rc" = 0 ] && [ -x "$K6D/go2cs-U.exe" ] || die "the union converter did not build (k6-conv-build.log)"
stamp "5 K6 union converter @ ${HEADF:0:10} sha256=$(sha256sum "$K6D/go2cs-U.exe" | cut -c1-16)"
for os in linux darwin; do                 # strictly one at a time, each into its OWN root (floor 1)
  R="$K6D/$os"; F="src/core/os/$os/file_unix.cs"
  mkdir -p "$R" || die "mkdir $R"
  # Floor 2: the root is SEEDED from the union's src/core, plus version.props beside it (else the corpus toolchain pin
  # is inert) and docs/validation -- the same seed TRAIN J's emission check gives every root.
  git archive HEAD src/core src/version.props docs/validation | tar -x -C "$R"; ps=("${PIPESTATUS[@]}")
  [ "${ps[0]}" = 0 ] && [ "${ps[1]}" = 0 ] || die "seed $os rc=${ps[*]}"
  [ -f "$R/src/core/golib/golib.csproj" ] && [ -f "$R/src/version.props" ] && [ -f "$R/$F" ] || die "seed $os incomplete"
  touch "$K6D/sentinel-$os"; sleep 1
  # Single-target -stdlib, package filter 'os'. Under -stdlib the positionals are PACKAGE NAMES and the output root IS
  # -go2cspath (main.go), so floor 3's second positional does not apply: the seeded root is passed as -go2cspath, and a
  # second positional would be read as a second package. NOT '-platforms linux/amd64,darwin/amd64' in one run: a list
  # performs the L3 MERGE (platformEmit.go), which classifies over only the targets given, so a file identical on linux
  # and darwin would be written FLAT and its per-GOOS copies removed. A single target adopts the seed's existing L3
  # layout (platformLayout.go rule 1) and writes os/<goos>/file_unix.cs; tK-k6check.py asserts that it did.
  MSYS_NO_PATHCONV=1 "$K6D/go2cs-U.exe" -stdlib -comments -convert-timeout 30m -platforms "$os/amd64" -go2cspath "$(w "$R/src")" os \
    > "$LOGDIR/k6-emit-$os.log" 2>&1 < /dev/null; rc=$?
  [ "$rc" = 0 ] || die "the $os conversion rc=$rc (k6-emit-$os.log)"
  grep -aq '(VERSION go1.24.13, read in-process)' "$LOGDIR/k6-emit-$os.log" || die "the $os conversion's toolchain provenance line does not read go1.24.13"
  grep -aq 'loader root NOT ASKED' "$LOGDIR/k6-emit-$os.log" && die "the $os conversion could not ask the loader root"
  [ -z "$(find "$R/src/core" -name 'std.*.csproj' -newer "$K6D/sentinel-$os" -print -quit)" ] || die "std.*.csproj written ($os): the GOROOT-spelling misroute"
  python "$SD/tK-k6check.py" "$(w "$W")" "$F" "$(w "$R")" "$(w "$K6D/sentinel-$os")" --apply > "$LOGDIR/k6-check-$os.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "K6 $os: $(tr -d '\r' < "$LOGDIR/k6-check-$os.log" | grep -a 'K6 FAIL' | head -n 1)"
  ns=$(git diff --numstat -- "$F")
  [ "$ns" = "$(printf '1\t1\t%s' "$F")" ] || die "K6 $os: git reads '$ns' for $F, expected 1/1"
  stamp "5 K6 $os: $(grep -a '^K6 OK' "$LOGDIR/k6-check-$os.log" | tr -d '\r' | cut -c1-200)"
done
fi

# ------------------------------------------------------------------------ STEP 6: PRERES-K1 + K3 count asserts
STEP=6-K1-K3
REGF=(src/go2cs/go2cs-src.projitems src/go2cs.slnx src/tests/Behavioral/BehavioralTests/CompileTests.cs src/tests/Behavioral/BehavioralTests/OutputComparisonTests.cs src/tests/Behavioral/BehavioralTests/TargetComparisonTests.cs src/tests/Behavioral/BehavioralTests/TranspileTests.cs)
git diff --quiet HEAD -- "${REGF[@]}" || die "a registration file differs from HEAD in the worktree"
python "$SD/tK-countassert.py" regs "$(w "$W")" "$BASE" HEAD > "$LOGDIR/k1k3.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "K1/K3 count asserts rc=$rc (k1k3.log): $(grep -a 'FAIL' "$LOGDIR/k1k3.log" | head -n 2 | tr '\r\n' '  ')"
stamp "6 K1/K3: $(grep -aE '^  OK ' "$LOGDIR/k1k3.log" | sed -E 's/^  OK +([^:]+): base ([0-9]+)\/[0-9]+ \+ ([0-9]+) seat.*union ([0-9]+)\/([0-9]+).*/\1 \2+\3seats=\4 lines,\5 keys;/' | tr -d '\r' | tr '\n' ' ')"

# ------------------------------------------------------------------------------ STEP 7: PRERES-K2 token asserts
STEP=7-K2
git diff --quiet HEAD -- src/push-nuget.ps1 || die "push-nuget.ps1 differs from HEAD in the worktree"
python "$SD/tK-countassert.py" nuget "$(w "$W")" "$BASE" HEAD > "$LOGDIR/k2.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "K2 asserts rc=$rc (k2.log): $(grep -a 'FAIL\|OFF' "$LOGDIR/k2.log" | head -n 2 | tr '\r\n' '  ')"
stamp "7 K2: $(grep -a 'lines:\|multiset' "$LOGDIR/k2.log" | tr -d '\r' | sed 's/^ *//' | tr '\n' ' ') tokens $(grep -ac '^  OK ' "$LOGDIR/k2.log") OK"

# -------------------------------------------------------------------------------- STEP 8: explicit add, signed commit
STEP=8-commit
if [ "$REHEARSE" = 1 ]; then stamp "REHEARSAL DONE through step 7; nothing added or committed; W=$W keeps the step 1-2 edits"; exit 0; fi
{ cat "$LOGDIR/s1-list.txt" "$LOGDIR/opt-list.txt"; echo src/core/os/linux/file_unix.cs; echo src/core/os/darwin/file_unix.cs; } | LC_ALL=C sort -u > "$LOGDIR/expected.txt"
git diff --name-only > "$LOGDIR/changed.raw"; rc=$?
[ "$rc" = 0 ] || die "git diff --name-only rc=$rc"
LC_ALL=C sort "$LOGDIR/changed.raw" > "$LOGDIR/changed.txt"
if ! cmp -s "$LOGDIR/expected.txt" "$LOGDIR/changed.txt"; then
  LC_ALL=C comm -3 "$LOGDIR/expected.txt" "$LOGDIR/changed.txt" | head -n 10 > "$LOGDIR/changed-vs-expected.txt"
  die "the worktree's changed set is not exactly steps 1, 2 and 5's (changed-vs-expected.txt: col 1 expected-only, col 2 changed-only)"
fi
gitf "$LOGDIR/status-add.txt" status --porcelain
[ "$(grep -c '^ D' "$LOGDIR/status-add.txt")" = 0 ] || die "tracked deletions in the worktree (floor 8)"
tr '\n' '\0' < "$LOGDIR/expected.txt" > "$LOGDIR/add.z"
git --literal-pathspecs add --pathspec-from-file="$(w "$LOGDIR/add.z")" --pathspec-file-nul; rc=$?
[ "$rc" = 0 ] || die "git add rc=$rc"
git diff --cached --name-only | LC_ALL=C sort > "$LOGDIR/staged.txt"; ps=("${PIPESTATUS[@]}")
[ "${ps[0]}" = 0 ] || die "git diff --cached rc=${ps[0]}"
cmp -s "$LOGDIR/expected.txt" "$LOGDIR/staged.txt" || die "the staged set is not exactly the expected set"
gitf "$LOGDIR/unstaged.txt" diff --name-only
[ ! -s "$LOGDIR/unstaged.txt" ] || die "unstaged tracked edits remain after the add (unstaged.txt)"
NFILES=$(wc -l < "$LOGDIR/expected.txt")
cat > "$LOGDIR/commit-msg.txt" <<EOF
fixup: TRAIN K -- S1 csproj regeneration ($COUNT), OSR opt-in insert ($EXP_OPT), os/{linux,darwin}/file_unix.cs from the union converter

The FIXUP ORDER of the TRAIN K draft, on the assembled union ${HEADF:0:10} ($SEATS_EXPECTED seat merges onto $BASE):
(1) S1 (P1's recipe, p1-s1-dotnet-pin $S1SEAT): COUNT $COUNT csproj carrying <LangVersion>latest</LangVersion>
    under src/core, src/tests/Behavioral and src/tests/Performance (P1's $BASE_S1 at $BASE + S1, plus $NEWK_S1 new K
    behavioral projects); the sed to the conditioned 14 (sed -b, so line endings are kept; checked per file).
(2) C2's optin.py insert (c2-osr-optin $OPTSEAT): $EXP_OPT template descendants gain the commented
    TieredCompilationQuickJitForLoops block (C2's $BASE_OPT at $BASE plus $NEWK_OPT new K).
(3) S1's completeness census: ${tally#CENSUS tally }.
(4) optin.py check: CHECK PASS ($got).
    Every edited csproj reads S1 only 1/1 ($s_a), S1 + opt-in 4/1 ($s_b) or opt-in only 3/0 ($s_c).
PRERES-K6: the union converter's seeded single-target emission of os (linux/amd64, darwin/amd64) differs from
the committed file_unix.cs by exactly one line each, the sigpipe() declaration gaining
[global::System.Diagnostics.StackTraceHidden] (missing since TRAIN I's converter; both arms of the emission check
carry it, so M-vs-U cannot flag it). Both files are refreshed from that emission.
PRERES-K1/K3/K2 asserts, no content change: the registration files equal base + each seat's own inserts with no
duplicate key (go2cs-src.projitems 7 seats, go2cs.slnx and the 4 BehavioralTests files 6 seats each), and
push-nuget.ps1 carries both seats' lines exactly once (r-hop-population-gen, i9-j0-uuid-rehearsal).
FIXUP ORDER (5), the emission check's M-vs-U arms, is the battery's leg E at this commit.
$NFILES files.

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
EOF
git commit -q -S -F "$(w "$LOGDIR/commit-msg.txt")"; rc=$?
[ "$rc" = 0 ] || die "git commit -S rc=$rc (nothing committed; the index holds the staged set)"
NEWH=$(git rev-parse HEAD)
[ "$(git rev-parse HEAD^)" = "$HEADF" ] || die "the fixup's parent is not the assembled head"
git verify-commit HEAD > "$LOGDIR/verify-commit.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "git verify-commit rc=$rc on ${NEWH:0:10}"
gitf "$LOGDIR/status-post.txt" status --porcelain
[ "$(grep -vc '^??' "$LOGDIR/status-post.txt")" = 0 ] || die "tracked changes remain after the commit"
stamp "8 COMMITTED ${NEWH:0:10} tree=$(git rev-parse --short=10 HEAD^{tree}) signed=ok $(git show --shortstat --format= HEAD | tr -d '\n' | sed 's/^ *//')"
k6_rm || stamp "  K6 scratch NOT removed (k6_safe refused '$K6D')"
stamp "FIXUP DONE head=${NEWH:0:10} (the battery's EXPECT_HEAD; nothing pushed) free=$(freegb)G"
