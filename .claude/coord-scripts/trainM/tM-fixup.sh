#!/usr/bin/env bash
# TRAIN M FIXUP on the i7 (COORD runs it). DRAFT 2026-10-02 -- NOT RUN. ONE signed commit 'fixup: TRAIN M -- ...' on
# the assembled union /h/go2cs-tmp-coord/tM (branch claude/coord-trainM-union: the signed seat merges of
# tM-seats-draft.txt, in row order, onto TRAIN L's landed master aa0a07d5fd; 24 rows in the list FROZEN at 08:20 on
# 2026-10-02). The seat count is READ from the seat list; ruled follow-up merges are read from tM-follow.txt (EMPTY:
# none is owed at the frozen list).
#
# WHY M OWES A FIXUP (seat-footprints.md hazards H1 and H2, measured read-only on the pre-map union 8d7305053f; git
# reports every one of these merges clean, and none of them is finished):
#  (G1) GOLDENS. G's go-creator frame puts NoInlining on every function or literal whose body executes a `go`. G
#       re-baselined the goldens that existed at ITS base (c2591d5b95, before TRAIN L): 39 projects. TRAIN L's own guards
#       were born on master afterwards. Census at the map union: 40 behavioral projects hold a go statement and two of
#       them carry no NoInlining line, ForeverWaitWorkersMainReturns and MainSelectForeverWorkerExits. PREDICTED stale;
#       step 5 MEASURES it with the union's own CNR and re-baselines exactly what CNR moves, by content class.
#  (G2) CORPUS. Two seats' converter changes reach corpus files that no seat committed: P2's F1+F6
#       (go/internal/srcimporter/srcimporter.cs:183, on a function G's seat also re-emits) and G's Extension A (the
#       constant-skip Caller/Callers window: '+7 functions per target'; aa0f1ee188 changes the converter only, and
#       f77c71ee64 took only its own hunks). COORD: "That one hunk rides M's corpus regeneration". Step 4 MEASURES the
#       set with the emission check at the assembled head and copies the union converter's own emission.
#  NOT here, because a check can name them and only a person can decide them (tM-README.md, manual steps):
#       H4  a behavioral directory with Go sources and no C# (src/tests/Behavioral/TestNeedsTransitiveApiRef was one):
#           PRERES's H4 arm REFUSES a union that holds one. At the frozen list it reads 0: row
#           p2-test-overload-references (e002a552a8) deletes that repro module in-seat, so no follow-up merge is owed;
#       H5  the roster sentence that still counts three release-tiered rows: stamped as a NOTE, G's to word.
#  L's fixup content does not apply: no M csproj was cut at an older template (0 carry <LangVersion>latest</LangVersion>
#  at the map union; step 1 derives it again and still knows how to repair one), and the output-comparison list has no
#  gap (724 attributes, 724 rows). Both are asserted, neither is assumed.
# Steps, every one gated on the previous one's rc (floor 7):
#   PRE     head/branch/ancestry/clean tree/GOROOT pin; the union's shape (seat merges + ruled follow-ups, no fixup yet)
#   PRERES  tM-helpers.py precheck on the assembled tree (COUNT, REG, BOTH, ROSTER, H3, H4, S1 census, optin, markers);
#           outparity; check-roster-format under pwsh 7 and Windows PowerShell 5.1 with the execution-config row count;
#           the stdlib-metadata guard by name, and the asset's regeneration when it is stale (rides the fixup)
#   0b      PROSE: COORD's comment-and-prose patch (PROSE_PATCH, manual step M12), applied and checked; skipped when unset
#   1       S1 COUNT, derived twice (EXPECT 0); a non-empty list is repaired with K's sed exactly as L did
#   2       optin.py insert (only when step 1 edited something)
#   3       optin.py check: CHECK PASS; the two counts are REPORTED (EXP_DESC / EXP_TH assert them when COORD sets them)
#   4       CORPUS: tM-emitcheck.sh at the assembled head (KEEP_U_ROOTS=1), then tM-regen-apply.py check and apply
#   5       GOLDENS: the census prediction, CNR at the assembled head, the content class, run-behavioral.ps1
#           --update-targets --filter per moved project, .cs == .cs.target, the runner's four phases per project
#   6       precheck again on the edited worktree (the battery's PRE-1, predicted), purge of the build output
#   7       explicit git add of exactly the paths steps 1-5 changed (never add -A, floor 8), git commit -S, verify-commit
# Nothing here pushes. On any ABORT the worktree edits made so far are listed with the exact command to discard them.
# ONE HEAVY JOB AT A TIME (floors 1, 11): steps 4 and 5 convert and build, so this script takes the battery's lock.
# VERIFY ROUND 1 (tM-CHANGES.md section 9): a LIVE gate across trains before the lock; the battery's GOROOT block; the
# stdlib-metadata guard BY NAME at PRERES and again after a regenerated package_info.cs; a csproj step 1 repaired is
# kept out of the step 4 and step 5 sets; the commit message states measurements and labels expectations as such.
# VERIFY ROUND 2 (tM-CHANGES.md section 10; COORD's rulings wf/notes/COORD-RULINGS-r2.md):
#  * REGEN_ALLOW is COMPOSED from the seat list (R1): a row whose notes MEASURE a footprint ('CORPUS FOOTPRINT n file
#    (<path>.cs ...)') admits exactly that file, and g-godebug-pc-line, while it is a row, admits the files directly in
#    REGEN_G_PKGS. Any other regenerated file STOPS the fixup before a byte is written, for COORD. Never widened here.
#  * SIBLINGS defaults to report (no widening for review siblings).
#  * A STALE stdlib-metadata.txt is REGENERATED and rides this fixup (it is derived from the union's package_info.cs
#    files: no single seat owns it); the file joins the expected set and the commit message says so.
#  * PROSE_PATCH=<file> (manual step M12): a comment-and-prose-only patch COORD wrote (src/_roster.ps1 comment lines,
#    docs/ValidatedTestPackages.md prose) is applied, checked to be exactly that, and rides this fixup.
#  * FIXUP_N=2 (3, ...): a commit ON TOP of a pushed union whose head already holds the fixup(s) below it; subject
#    'fixup-N: TRAIN M'. Never a replaced sha (floor 9). The battery's PRE admits that chain and nothing else.
#  * The four switches are validated one by one (a typo in one no longer hid behind 'skip' in another); a launch refused
#    on the lock stamps ABORT (its SUMMARY no longer read as a live run to the next launch's LIVE gate); the emission
#    check is told the lock is held (TM_LOCK_HELD=1); tM-regen-apply.py's refusal arm is run once as a control before
#    the real check; the PRE and FIXUP DONE lines carry the hashes of the two lists this run read.
# VERIFY ROUND 3 (tM-CHANGES.md section 11; COORD's rulings wf/notes/COORD-RULINGS-r3.md; the seat list is FROZEN):
#  * seats-effective.txt is built exactly as tM-battery.sh builds it, and its row count is asserted.
#  * The commit message's precheck paragraph quotes this run's own precheck lines (step 6); a sentence about a seat is
#    printed only when that seat is a row, with its base read from git.
set -u
W=${W:-/h/go2cs-tmp-coord/tM}
BRANCH=claude/coord-trainM-union
EXPECT_HEAD=${EXPECT_HEAD:?the assembled union head (tM-assemble.sh prints it)}
BASE=aa0a07d5fd
REGEN=${REGEN:-apply}                 # apply | skip   (skip = COORD's explicit call, stamped; leg E then reads what it reads)
GOLDENS=${GOLDENS:-regen}             # regen | skip   (skip = COORD's explicit call, stamped; leg 4 then reads what it reads)
SIBLINGS=${SIBLINGS:-report}          # report | refresh: the *.cs.auto review siblings only the UNION arm rewrites (verify round 2, RULED: the default launch reports; no widening for siblings)
GOLDEN_CLASS=${GOLDEN_CLASS:-gframe}  # gframe | any  (any = COORD has read an OTHER hunk and rules it a seat's intended line)
FIXUP_N=${FIXUP_N:-1}                 # 1 = the train's fixup on the assembled union; k+1 = a commit ON TOP of k fixups ('fixup-N: TRAIN M'), never a replaced sha
PROSE_PATCH=${PROSE_PATCH:-}          # manual step M12: COORD's comment-and-prose-only patch (src/_roster.ps1 comment lines, the roster's prose); unset = none
# The corpus paths step 4 may write. RULED (verify round 2): only files a seat's own acceptance MEASURED as corpus
# footprint. The pattern is COMPOSED below from the seat list, never widened automatically, and stamped in PRE:
#   - a row whose notes say 'CORPUS FOOTPRINT <n> file(s) (<path>.cs ...)' admits exactly src/core/<path>.cs (today
#     p2-func-literal-result-type: go/internal/srcimporter/srcimporter.cs);
#   - REGEN_G_SEAT, while it is a row, admits the files DIRECTLY in REGEN_G_PKGS: where Extension A's functions were
#     READ unmarked on G's tip (M-requirements.md, section 1). The seat's own note says '3 files x3' and the third file
#     is named nowhere: the emission check reports it, and a file outside these folders STOPS the fixup for COORD.
# Verify round 1's narrowing stands: files directly in a package folder, no sub-package (net/http has cgi, cookiejar,
# fcgi, httptest, httptrace, httputil, internal, pprof), no GOOS folder. A value given at launch is COORD's explicit call.
REGEN_ALLOW=${REGEN_ALLOW:-}
REGEN_G_SEAT=g-godebug-pc-line
REGEN_G_PKGS='net/http internal/reflectlite'
EXEC_ROWS_EXPECT=${EXEC_ROWS_EXPECT:-}    # rows carrying an execution config; unset = the count of tM-helpers.py EXEC_RULED (the ONE site of that ruling: log/slog alone, tL-seats-draft.txt:31, :63)
EXP_DESC=${EXP_DESC:-}                # optin.py check's template-descendant count: TO BE SET BY COORD (L read 1140; M adds 7 projects)
EXP_TH=${EXP_TH:-}                    # ... and its test-host count: TO BE SET BY COORD (L read 229)
SEATS_EXPECTED=${SEATS_EXPECTED:-}    # optional second derivation of the seat count (the GO's number); the list is the first
OPT=docs/phase4/recipes/quickjit-optin/optin.py
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tM-seats-draft.txt}
FOLLOWF=$SD/tM-follow.txt
LOGDIR=$SD/tM-fixup-logs
LOCK=/h/go2cs-tmp-coord/coord-scratch/tM/.battery.lock
ES=/h/go2cs-tmp-coord/tMemitfix       # step 4's emission scratch (tM-emitcheck.sh accepts only /h/go2cs-tmp-coord/tMemit*)
case "$SD" in
  /h/go2cs-tmp-coord/tM|/h/go2cs-tmp-coord/tM/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
for f in tM-helpers.py tM-emitcheck.sh emitdrift.py tM-regen-apply.py tM-follow.txt; do
  [ -f "$SD/$f" ] || { echo "ABORT: $SD/$f missing from the run copy"; exit 2; }
done
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS): copy trainM/tM-seats-draft.txt into the run folder"; exit 2; }
# Verify round 2: each switch on its own. As one string, the arm 'skip:*|*:skip:*' accepted ANY value in the other fields
# (REGEN=skip with a mistyped GOLDENS skipped step 5 and stamped it "COORD's explicit call").
case "$REGEN" in apply|skip) ;; *) echo "ABORT: REGEN=$REGEN (apply|skip)"; exit 2 ;; esac
case "$GOLDENS" in regen|skip) ;; *) echo "ABORT: GOLDENS=$GOLDENS (regen|skip)"; exit 2 ;; esac
case "$SIBLINGS" in refresh|report) ;; *) echo "ABORT: SIBLINGS=$SIBLINGS (refresh|report)"; exit 2 ;; esac
case "$GOLDEN_CLASS" in gframe|any) ;; *) echo "ABORT: GOLDEN_CLASS=$GOLDEN_CLASS (gframe|any)"; exit 2 ;; esac
case "$FIXUP_N" in [1-9]) ;; *) echo "ABORT: FIXUP_N=$FIXUP_N (1 = the train's fixup; 2..9 = a fixup on top of the ones already at the head)"; exit 2 ;; esac
[ -z "$PROSE_PATCH" ] || [ -f "$PROSE_PATCH" ] || { echo "ABORT: PROSE_PATCH=$PROSE_PATCH is not a file"; exit 2; }
[ -z "$PROSE_PATCH" ] || PROSE_PATCH="$(cd "$(dirname "$PROSE_PATCH")" && pwd)/$(basename "$PROSE_PATCH")"   # absolute: the script cd's into the worktree before it reads the patch
[ "$REGEN" = apply ] && [ "$GOLDENS" = regen ] || echo "NOTE: REGEN=$REGEN GOLDENS=$GOLDENS -- a skipped step is COORD's explicit call and is stamped"
FXSUBJ='fixup: TRAIN M'; [ "$FIXUP_N" = 1 ] || FXSUBJ="fixup-$FIXUP_N: TRAIN M"
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
STEP=PRE
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
w(){ cygpath -w "$1"; }
hp(){ python -B "$(cygpath -w "$SD/tM-helpers.py")" "$@"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
# Verify round 2: the two lists are PER-RUN-FOLDER copies, so each run stamps the bytes it read (PRE and FIXUP DONE; the
# battery stamps the same pair): rows = sha256 over the 'ref|sha' columns of the seat rows (tM-conflict-map.sh and
# tM-assemble.sh print the same hash), follow = sha256 over the entries of tM-follow.txt (e3b0c44298fc = EMPTY).
rowsha(){ grep -E '^[A-Za-z0-9._-]+\|' "$1" | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12; }
followsha(){ grep -vE '^[[:space:]]*(#|$)' "$1" | tr -d '\r' | sha256sum | cut -c1-12; }
LISTS="lists=rows:$(rowsha "$SEATS")/follow:$(followsha "$FOLLOWF")"
# EXEC_ROWS_EXPECT's default is the count of the helper's EXEC_RULED (the one site of that ruling).
[ -n "$EXEC_ROWS_EXPECT" ] || EXEC_ROWS_EXPECT=$(hp execruled 2>/dev/null | tr -d '\r' | sed -n 's/^EXECRULED n=\([0-9][0-9]*\).*/\1/p')
[ -n "$EXEC_ROWS_EXPECT" ] || { echo "ABORT: tM-helpers.py execruled printed no count (EXEC_ROWS_EXPECT cannot be derived)"; exit 2; }
# REGEN_ALLOW, composed from the seat list unless COORD gave one (see the header and the variable's comment).
REGEN_SRC="given at launch (COORD's explicit call)"
if [ -z "$REGEN_ALLOW" ]; then
  ralt=''; REGEN_SRC=''
  while IFS='|' read -r ref sha desc; do
    for fp in $(printf '%s' "$desc" | grep -oE 'CORPUS FOOTPRINT[^()]*\([^)]*\)' | grep -oE '[A-Za-z0-9_/.-]+\.cs'); do
      ralt="$ralt|$(printf '%s' "$fp" | sed 's/[.]/\\./g')"; REGEN_SRC="$REGEN_SRC $ref:$fp"
    done
  done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
  if grep -qE "^$REGEN_G_SEAT\|" "$SEATS"; then
    for gpk in $REGEN_G_PKGS; do ralt="$ralt|$gpk/[^/]+"; REGEN_SRC="$REGEN_SRC $REGEN_G_SEAT:$gpk/*"; done
  fi
  if [ -n "$ralt" ]; then REGEN_ALLOW="^src/core/(${ralt#|})\$"; else REGEN_ALLOW='^$'; REGEN_SRC=' NOTHING (no row measures a footprint and the G seat is not a row: every regenerated path is refused)'; fi
  REGEN_SRC="composed from the seat list:$REGEN_SRC"
fi
die(){
  stamp "ABORT at step $STEP: $*"
  if [ -d "$W" ]; then
    # HEAD vs the WORKTREE, NUL-separated (K's form): staged edits are listed too, and glyph paths survive.
    git -C "$W" diff HEAD --name-only -z > "$LOGDIR/touched.z" 2>/dev/null
    if [ -s "$LOGDIR/touched.z" ]; then
      stamp "  worktree edits so far: $(tr -cd '\0' < "$LOGDIR/touched.z" | wc -c) tracked file(s), listed NUL-separated in $LOGDIR/touched.z"
      stamp "  to discard them: git -C $W restore --source=HEAD --staged --worktree --pathspec-from-file='$(cygpath -w "$LOGDIR/touched.z")' --pathspec-file-nul"
    fi
  fi
  [ -d "$ES" ] && stamp "  the emission scratch $ES is KEPT: a re-run at the same assembled head re-uses it (remove it by hand when it is no longer wanted)"
  exit 2
}
# Verify round 1 (floor 1 ACROSS trains): the lock below is TRAIN M's own, and TRAIN L's jobs take another one or none
# (tL-postmerge2.sh). tM-helpers.py live reads the other trains' locks, the converter and harness processes (twice,
# 20 s apart; listed, never killed) and any run log still being written. LIVE_ACK=1 runs past it: COORD's explicit call.
if [ "${LIVE_ACK:-0}" = 1 ]; then
  stamp "PRE LIVE gate ACKNOWLEDGED by COORD (LIVE_ACK=1): not read"
else
  lv=$(hp live "$(cygpath -w "$LOCK")" 2>&1); lrc=$?
  if [ "$lrc" != 0 ]; then
    stamp "ABORT (floor 1): another conversion, harness or battery looks alive on this box (listed, not killed; LIVE_ACK=1 runs past it by COORD's explicit call):"
    printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE' | head -n 12 | while IFS= read -r l; do stamp "  ${l:0:300}"; done
    exit 2
  fi
  stamp "PRE LIVE gate: $(printf '%s\n' "$lv" | tr -d '\r' | grep -a '^LIVE-VERDICT' | cut -c1-200)"
fi
# Verify round 2: stamped, not echoed. The LIVE stamp above already created this run's SUMMARY.txt, and a SUMMARY whose
# tail holds no DONE / ABORT / STOP word reads as a live run to every TRAIN M launch for the next 15 minutes (tM-helpers.py
# live, reading 3), this folder's own relaunch included.
mkdir "$LOCK" 2>/dev/null || { stamp "ABORT: $LOCK exists -- a TRAIN M battery, module-legs run, emission check or fixup holds it; rmdir it only after checking"; exit 2; }
trap 'rmdir "$LOCK" 2>/dev/null' EXIT
# The build shell, as tM-battery.sh sets it up: go1.24.13 from C: in its BACKSLASH spelling (floor 6), dotnet10 on PATH,
# temp and caches on H:. Steps 4 and 5 convert and build; L's fixup only edited text and needed the go half alone.
ORIGPATH="$PATH"
GOPIN_PATH="$HOME/sdk/go1.24.13/bin:$ORIGPATH"
# Verify round 1: the battery's GOROOT block (tM-battery.sh ENV), with die for exit 2. The value is checked BEFORE the
# export (a `go env GOROOT` after it only echoes the env back), and `go` on PATH must be go1.24.13: step 5 re-baselines
# goldens under THIS environment, and a golden cut under another run GOROOT is the 'Target green, Compile red' trap.
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) die "GOROOT spelling '$GR' has a forward slash (floor 6)" ;; esac
[[ "$GR" =~ ^[A-Za-z]:[\\].*[\\]sdk[\\]go1\.24\.13$ ]] || die "GOROOT spelling '$GR' is not a drive-rooted backslash path ending sdk<backslash>go1.24.13"
[ "$GR" = "$(cygpath -w "$HOME")\\sdk\\go1.24.13" ] || die "GOROOT spelling '$GR' is not <profile>\\sdk\\go1.24.13"
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || die "$HOME/sdk/go1.24.13/VERSION is not go1.24.13"
GR_TC=$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')
[ "$GR_TC" = "$GR" ] || die "the toolchain's own GOROOT spelling '$GR_TC' != '$GR'"
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH" GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || die "go on PATH is $(go version), not go1.24.13"
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
unset GO2CS_MODULE_ROOT
PWSH=$HOME/.dotnet/tools/pwsh
cd "$W" || die "no worktree $W"
HEADF=$(git rev-parse HEAD)
BASEF=$(git rev-parse "$BASE^{commit}") || die "base $BASE does not resolve"
[ "${HEADF:0:10}" = "${EXPECT_HEAD:0:10}" ] || die "HEAD ${HEADF:0:10} != $EXPECT_HEAD"
[ "$(git symbolic-ref --short HEAD 2>/dev/null)" = "$BRANCH" ] || die "not on $BRANCH"
# (the GOROOT pin was checked on its value before the export, above)
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || die "tracked changes before the fixup"
git merge-base --is-ancestor "$BASE" HEAD || die "base $BASE not an ancestor"
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1)); git merge-base --is-ancestor "$sha" HEAD || die "seat $ref $sha not an ancestor"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
[ "$n" -ge 1 ] || die "the seat list $SEATS holds no seat row"
[ -z "$SEATS_EXPECTED" ] || [ "$n" = "$SEATS_EXPECTED" ] || die "seat list has $n seats, SEATS_EXPECTED says $SEATS_EXPECTED"
fpc=$(git rev-list --first-parent --count "$BASE..HEAD")
# FOLLOW-UP SEAT MERGES: read from tM-follow.txt (EMPTY; none is owed at the frozen list). After the seat merges, a ruled follow-up may
# ride: a merge whose second parent is on that entry's origin branch, descends from the entry's seated sha, is not it,
# and comes after that seat's own merge.
FOLLOW=$(grep -vE '^[[:space:]]*(#|$)' "$FOLLOWF" | tr -d '\r' | tr '\n' ' ')
# Verify round 2 (floor 9, RULED): the head may already hold fixups. FIXUP_N=1 wants none (the assembled union's head is
# a merge); FIXUP_N=k+1 wants exactly k single-parent commits above the merges, subjects 'fixup: TRAIN M', then
# 'fixup-2: TRAIN M', ... in order, and this run commits 'fixup-(k+1): TRAIN M' ON TOP. A pushed sha is never replaced.
nprev=0; t=HEAD
while [ "$(git rev-list --parents -n 1 "$t" | wc -w)" = 2 ] && [ "$nprev" -le 9 ]; do nprev=$((nprev + 1)); t="$t^"; done
[ "$nprev" = $((FIXUP_N - 1)) ] || die "HEAD carries $nprev single-parent commit(s) above the merges and FIXUP_N=$FIXUP_N wants $((FIXUP_N - 1)) (FIXUP_N=1 is the train's fixup on the assembled union; FIXUP_N=k+1 is a commit on top of k fixups)"
k=$nprev; c=HEAD
while [ "$k" -ge 1 ]; do
  want='fixup: TRAIN M'; [ "$k" = 1 ] || want="fixup-$k: TRAIN M"
  case "$(git log -1 --format=%s "$c")" in "$want"*) ;; *) die "the single-parent commit $(git rev-parse --short=10 "$c") is not '$want...' (subject: $(git log -1 --format=%s "$c" | cut -c1-80)): the head of a pushed union is 'fixup: TRAIN M', then 'fixup-2', 'fixup-3', ... in order" ;; esac
  k=$((k - 1)); c="$c^"
done
UTOP=$(git rev-parse "$t")
FIXES=''; nfix=0
for c in $(git rev-list --first-parent "$BASE..$UTOP"); do
  [ "$(git rev-list --parents -n 1 "$c" | wc -w)" = 3 ] || die "first-parent commit ${c:0:10} is not a two-parent merge (a stray commit below the seats' merges)"
  p2=$(git rev-parse "$c^2")
  grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | cut -d'|' -f2 | while read -r s; do git rev-parse "$s^{commit}"; done | grep -qxF "$p2" && continue
  fx=''
  for fr in $FOLLOW; do
    fref=refs/remotes/origin/claude/${fr%%:*}; fseat=$(git rev-parse "${fr##*:}^{commit}")
    git rev-parse --verify -q "$fref" > /dev/null || continue
    if [ "$p2" != "$fseat" ] && git merge-base --is-ancestor "$p2" "$fref" && git merge-base --is-ancestor "$fseat" "$p2" && git merge-base --is-ancestor "$fseat" "$c^1"; then fx=${fr%%:*}; break; fi
  done
  [ -n "$fx" ] || die "merge ${c:0:10} ^2=${p2:0:10} is neither a listed seat nor a ruled follow-up (add '<ref>:<seat sha>' to THIS run folder's tM-follow.txt and fetch origin/claude/<ref> first if it is one)"
  nfix=$((nfix + 1)); FIXES="$FIXES $fx:${p2:0:10}"
done
[ "$fpc" = $((n + nfix + nprev)) ] || die "first-parent count $fpc != $n seat merges + $nfix follow-up(s) + $nprev earlier fixup(s): a stray commit, or a seat that rode in through another seat"
SEATS_EFF="$LOGDIR/seats-effective.txt"
# Verify round 3: the SAME construction as tM-battery.sh's (row lines only, CR-stripped, each terminated, then the
# follow-up rows), and the row count asserted in both, so the file README M11 feeds to the landing precheck is one file.
{ grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r'; for f in $FIXES; do echo "${f%%:*}-followup|${f##*:}|ruled follow-up merge of ${f%%:*} (origin/claude/${f%%:*})"; done; } > "$SEATS_EFF"
[ "$(grep -cE '^[A-Za-z0-9._-]+\|' "$SEATS_EFF")" = $((n + nfix)) ] || die "seats-effective.txt holds another row count than $n seat rows + $nfix follow-up row(s)"
# Signing preflight BEFORE the long steps (K's block: step 7 commits with -S, and a failure there would come last).
SIGNFMT=$(git config --get gpg.format || true); SIGNKEY=$(git config --get user.signingkey || true)
[ -n "$SIGNKEY" ] || die "no user.signingkey: the step-7 commit -S cannot sign"
case "${SIGNFMT:-openpgp}" in
  openpgp)
    gp=$(git config --get gpg.program || echo gpg); case "$gp" in ?:*) gp=$(cygpath -u "$gp") ;; esac
    "$gp" --batch --list-secret-keys "$SIGNKEY" > /dev/null 2>&1; rc=$?
    [ "$rc" = 0 ] || die "the signing key is not in the secret keyring (gpg rc=$rc)"
    if [ "${SIGN_PROBE:-1}" = 1 ]; then  # a real signature through the agent; SIGN_PROBE=0 skips it
      echo "tM-fixup signing probe" | "$gp" --batch --yes --local-user "$SIGNKEY" --clearsign > "$LOGDIR/sign-probe.asc" 2> "$LOGDIR/sign-probe.err"; rc=$?
      [ "$rc" = 0 ] || die "the signing probe failed (gpg rc=$rc, sign-probe.err): unlock the agent, then rerun"
    fi ;;
  *) stamp "PRE signing format '$SIGNFMT': key configured, no agent probe for this format" ;;
esac
f=$(freegb); [ "${f:-0}" -ge 30 ] || die "free disk ${f}G < 30G on H:"
c=$(cfree); [ "${c:-0}" -ge 8 ] || die "C: free ${c}G < 8G"
stamp "PRE head=${HEADF:0:10} seats=$n ($(basename "$SEATS")) first-parent=$fpc follow-ups=[${FIXES# }] earlier-fixups=$nprev (this run commits '$FXSUBJ') REGEN=$REGEN GOLDENS=$GOLDENS SIBLINGS=$SIBLINGS GOLDEN_CLASS=$GOLDEN_CLASS EXEC_ROWS_EXPECT=$EXEC_ROWS_EXPECT PROSE_PATCH=${PROSE_PATCH:-none} $LISTS $(go version | cut -d' ' -f3) free=${f}G C:=${c}G"
stamp "PRE derived (R1): REGEN_ALLOW=/$REGEN_ALLOW/ :: $REGEN_SRC"
# Verify round 2: the assembly's MAP TREE line was console text nobody read again; tM-assemble.sh now keeps it, and it is
# stamped here with the head it was written at (a follow-up merged since moves the head, and says so).
MTF=/h/go2cs-tmp-coord/coord-scratch/tM/assemble-maptree.txt
stamp "PRE assembly: $([ -f "$MTF" ] && tr -d '\r' < "$MTF" | head -n 1 | cut -c1-600 || echo "no $MTF (the assembly did not write its MAP TREE line: an older tM-assemble.sh, or a union assembled by hand)")"

# ------------------------------------------------------------------- PRERES: what the merged tree must already be
STEP=PRERES
hp precheck "$(w "$W")" "$BASE" "$(w "$SEATS_EFF")" worktree > "$LOGDIR/precheck-preres.log" 2>&1; rc=$?
grep -aE '^ok +(COUNT|REG|BOTH:|ROSTER|H3|H4)' "$LOGDIR/precheck-preres.log" | while IFS= read -r l; do stamp "  PRERES ${l:0:230}"; done   # BOTH: its summary line (one ok line per merged path is in the log)
grep -a '^NOTE' "$LOGDIR/precheck-preres.log" | while IFS= read -r l; do stamp "  PRERES ${l:0:300}"; done
[ "$rc" = 0 ] || die "precheck rc=$rc: $(grep -a '^FAIL' "$LOGDIR/precheck-preres.log" | head -n 4 | tr '\n' ' ' | cut -c1-700) (precheck-preres.log; an H4 line is NOT expected at the frozen list, whose row p2-test-overload-references deletes the repro in-seat: read tM-README.md M2 before cutting anything)"
stamp "PRERES precheck $(grep -a '^PRECHECK' "$LOGDIR/precheck-preres.log")"
hp outparity "$(w "$W")" > "$LOGDIR/outparity.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "outparity: $(tr -d '\r' < "$LOGDIR/outparity.log" | head -n 1) -- the seat that added an attribute or a project owes its OutputComparisonTests row as a follow-up merge (L's fixup carried such rows under COORD ruling R1; M has no such ruling)"
stamp "PRERES outparity: $(tr -d '\r' < "$LOGDIR/outparity.log" | head -n 1)"
# The roster of record after G's two roster commits met master's TRAIN L provenance block (tL-seats-draft.txt:31
# "re-run check-roster-format x2"): both editions pass, and exactly EXEC_ROWS_EXPECT rows carry an execution config.
env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1 > "$LOGDIR/roster7.log" 2>&1; r7=$?
env -u DOTNET_ROOT PATH="$GOPIN_PATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$LOGDIR/roster51.log" 2>&1; r5=$?
for ed in 7 51; do
  gl=$(grep -a 'roster format guard:' "$LOGDIR/roster$ed.log" | tail -n 1 | tr -d '\r')
  er=$(echo "$gl" | grep -oE '[0-9]+ with an execution config' | cut -d' ' -f1)
  stamp "PRERES roster guard (edition $ed): ${gl:-NO VERDICT LINE}"
  [ "${er:-x}" = "$EXEC_ROWS_EXPECT" ] || die "roster guard (edition $ed) reads '${er:-unread}' rows with an execution config, expected $EXEC_ROWS_EXPECT (roster$ed.log): G's two row changes and master's block did not both land"
done
[ "$r7" = 0 ] && [ "$r5" = 0 ] || die "check-roster-format rc: pwsh 7 = $r7, Windows PowerShell 5.1 = $r5"
# Verify round 1 (merge-hazards: "a merge touching package_info.cs must carry the matching stdlib-metadata.txt change --
# check it in the PREFLIGHT"). The pre-map union changes 30 src/core package_info.cs and no stdlib-metadata.txt, and the
# guard that reads it (TestStdLibMetadataInSync: it regenerates the asset in-process from ../core ON DISK and compares)
# otherwise runs first in the battery's leg C, after the signed commit. Read here BY NAME on the merged tree, before the
# long steps, and again after step 4 when the regeneration wrote a package_info.cs.
# Verify round 2, RULED: a stale asset is REGENERATED here and rides this fixup (it is derived from the union's
# package_info.cs files: no single seat owns it). The regeneration is the directive's own command (stdlibMetadata.go:
# `//go:generate go run ./internal/genstdlibmeta`), run alone: `go generate .` would also run the symbols generator.
# It must change exactly src/go2cs/stdlib-metadata.txt, and the guard must then PASS by name. A red that is not the
# STALE message (a skip, a build failure, the constants check) is not regenerated over: it dies, to be read.
: > "$LOGDIR/meta-files.txt"; METAV='in sync (nothing regenerated)'
stdlibmeta(){ # label -- '--- PASS' by name; a STALE asset is regenerated, re-read by name, and joins the expected set
  ( cd src/go2cs && go test -count=1 -timeout 15m -v -run '^TestStdLibMetadataInSync$' . ) > "$LOGDIR/stdlibmeta-$1.log" 2>&1; rc=$?
  if [ "$rc" = 0 ] && grep -aq -- '--- PASS: TestStdLibMetadataInSync' "$LOGDIR/stdlibmeta-$1.log"; then
    stamp "$1 stdlib-metadata: TestStdLibMetadataInSync --- PASS (the asset in the worktree equals a regeneration from src/core on disk)"; return 0
  fi
  grep -aq 'stdlib-metadata.txt is STALE' "$LOGDIR/stdlibmeta-$1.log" \
    || die "TestStdLibMetadataInSync did not PASS and did not report a STALE asset ($1; rc=$rc, stdlibmeta-$1.log): a skip, a build failure or another assert -- read it"
  ( cd src/go2cs && go run ./internal/genstdlibmeta ) > "$LOGDIR/stdlibmeta-$1-generate.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "the stdlib-metadata regeneration failed ($1; go run ./internal/genstdlibmeta rc=$rc, stdlibmeta-$1-generate.log)"
  mchg=$(git diff --name-only -- src/go2cs | tr '\n' ' ')
  [ "$mchg" = 'src/go2cs/stdlib-metadata.txt ' ] || die "the stdlib-metadata regeneration changed [$mchg] under src/go2cs, expected exactly src/go2cs/stdlib-metadata.txt"
  ( cd src/go2cs && go test -count=1 -timeout 15m -v -run '^TestStdLibMetadataInSync$' . ) > "$LOGDIR/stdlibmeta-$1-after.log" 2>&1; rc=$?
  [ "$rc" = 0 ] && grep -aq -- '--- PASS: TestStdLibMetadataInSync' "$LOGDIR/stdlibmeta-$1-after.log" || die "the regenerated stdlib-metadata.txt does not PASS its own guard ($1; rc=$rc, stdlibmeta-$1-after.log)"
  echo src/go2cs/stdlib-metadata.txt > "$LOGDIR/meta-files.txt"
  METAV="STALE at $1, regenerated from the union's src/core ($(git diff --numstat -- src/go2cs/stdlib-metadata.txt | awk '{ print "+" $1 " / -" $2 " lines" }')), TestStdLibMetadataInSync --- PASS after"
  stamp "$1 stdlib-metadata: $METAV -- the asset rides this fixup (COORD's ruling)"
}
stdlibmeta PRERES

# ------------------------------------------------- STEP 0b: PROSE (manual step M12), only when COORD passes PROSE_PATCH
# RULED (verify round 2): the correction of the log/slog reason rides the FIXUP as a comment-and-prose-only hunk: the
# comment in src/_roster.ps1 and the roster's prose. COORD writes the exact words as a patch; this step applies it and
# checks it is ONLY that: no path but those two; in src/_roster.ps1 every changed line is a '#' comment line and no added
# line carries a non-ASCII byte (the file has no BOM, and a BOM-less non-ASCII character in a .ps1 is what broke Windows
# PowerShell 5.1 at TRAIN L); the roster guard passes again under both editions with the same execution-config count.
# That no row, count, linux annotation or execution annotation moved is step 6's precheck (its ROSTER arms). The roster
# sentence of hazard H5 (manual step M3) may ride the same patch.
STEP=0b-prose
: > "$LOGDIR/prose-files.txt"; PROSEV='none (no PROSE_PATCH given: manual step M12 is still owed)'
if [ -n "$PROSE_PATCH" ]; then
  pp=$(w "$PROSE_PATCH")
  git apply --numstat "$pp" > "$LOGDIR/prose-numstat.txt" 2> "$LOGDIR/prose-apply.err" || die "PROSE_PATCH is not a patch git can read (prose-apply.err)"
  pbad=$(awk '{ print $3 }' "$LOGDIR/prose-numstat.txt" | grep -vxF -e src/_roster.ps1 -e docs/ValidatedTestPackages.md | tr '\n' ' ')
  [ -z "$pbad" ] || die "PROSE_PATCH touches [$pbad]: only src/_roster.ps1 (comment lines) and docs/ValidatedTestPackages.md (prose) are ruled"
  [ -s "$LOGDIR/prose-numstat.txt" ] || die "PROSE_PATCH changes no file"
  git apply --check "$pp" 2>> "$LOGDIR/prose-apply.err" || die "PROSE_PATCH does not apply to this tree (prose-apply.err): cut it on the assembled union ${HEADF:0:10}"
  git apply "$pp" 2>> "$LOGDIR/prose-apply.err" || die "git apply PROSE_PATCH"
  if git diff --name-only | grep -qxF src/_roster.ps1; then
    pnc=$(git diff -U0 -- src/_roster.ps1 | tr -d '\r' | grep -E '^[+-]' | grep -vE '^(\+\+\+|---) ' | grep -vcE '^[+-][[:space:]]*(#.*)?$')
    [ "$pnc" = 0 ] || die "PROSE_PATCH: $pnc changed line(s) of src/_roster.ps1 are not '#' comment lines (the ruling is comment-and-prose only)"
    pna=$(git diff -U0 -- src/_roster.ps1 | tr -d '\r' | grep -E '^\+' | grep -vE '^\+\+\+ ' | LC_ALL=C grep -c $'[^ -~\t]')
    [ "$pna" = 0 ] || die "PROSE_PATCH: $pna added line(s) of src/_roster.ps1 carry a non-ASCII byte (the file has no BOM: write the correction in ASCII)"
  fi
  env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1 > "$LOGDIR/roster7-prose.log" 2>&1; r7=$?
  env -u DOTNET_ROOT PATH="$GOPIN_PATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$LOGDIR/roster51-prose.log" 2>&1; r5=$?
  for ed in 7 51; do
    er=$(grep -a 'roster format guard:' "$LOGDIR/roster$ed-prose.log" | tail -n 1 | tr -d '\r' | grep -oE '[0-9]+ with an execution config' | cut -d' ' -f1)
    [ "${er:-x}" = "$EXEC_ROWS_EXPECT" ] || die "after PROSE_PATCH the roster guard (edition $ed) reads '${er:-unread}' rows with an execution config, expected $EXEC_ROWS_EXPECT (roster$ed-prose.log)"
  done
  [ "$r7" = 0 ] && [ "$r5" = 0 ] || die "after PROSE_PATCH check-roster-format rc: pwsh 7 = $r7, Windows PowerShell 5.1 = $r5"
  git diff --name-only -- src/_roster.ps1 docs/ValidatedTestPackages.md | LC_ALL=C sort > "$LOGDIR/prose-files.txt"
  PROSEV="$(grep -c . "$LOGDIR/prose-files.txt") file(s) [$(tr '\n' ' ' < "$LOGDIR/prose-files.txt")] from $(basename "$PROSE_PATCH") sha256=$(sha256sum "$PROSE_PATCH" | cut -c1-12): $(awk '{ printf "%s +%s/-%s ", $3, $1, $2 }' "$LOGDIR/prose-numstat.txt")-- _roster.ps1 comment lines only, ASCII; the roster guard passes x2 with $EXEC_ROWS_EXPECT execution-config row(s)"
fi
stamp "0b prose: $PROSEV"

# ------------------------------------------------------------------------------------ STEP 1: S1 COUNT, then the sed
STEP=1-S1
PS=('src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' 'src/tests/Performance/**/*.csproj')
OLD='<LangVersion>latest</LangVersion>'
NEW="<LangVersion Condition=\"'\$(LangVersion)'==''\">14</LangVersion>"
git grep -l -F "$OLD" "$BASE" -- "${PS[@]}" > "$LOGDIR/old-at-base.raw"; rc=$?
[ "$rc" = 1 ] || [ "$rc" = 0 ] || die "git grep at base rc=$rc"
[ "$(grep -c . "$LOGDIR/old-at-base.raw")" = 0 ] || die "TRAIN L's master already carries the old token in $(grep -c . "$LOGDIR/old-at-base.raw") csproj (expected 0)"
git diff --diff-filter=A --name-only "$BASE" HEAD -- '*.csproj' | LC_ALL=C sort > "$LOGDIR/newM-union.txt"
: > "$LOGDIR/newM-seats.raw"
while IFS='|' read -r ref sha desc; do
  mb=$(git merge-base "$BASE" "$sha")
  git log "$mb..$sha" --diff-filter=A --name-only --format= -- '*.csproj' >> "$LOGDIR/newM-seats.raw" || die "git log $ref"
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS_EFF")
grep . "$LOGDIR/newM-seats.raw" | LC_ALL=C sort -u > "$LOGDIR/newM-seats.txt"
cmp -s "$LOGDIR/newM-union.txt" "$LOGDIR/newM-seats.txt" || die "new csproj: union diff vs seats' own commits disagree (newM-union.txt / newM-seats.txt)"
NEWN=$(grep -c . "$LOGDIR/newM-union.txt")
git grep -l -z -F "$OLD" -- "${PS[@]}" > "$LOGDIR/s1-list.z"; rc=$?
[ "$rc" = 0 ] || [ "$rc" = 1 ] || die "git grep (COUNT) rc=$rc"
tr '\0' '\n' < "$LOGDIR/s1-list.z" | grep . | LC_ALL=C sort > "$LOGDIR/s1-list.txt"
S1N=$(grep -c . "$LOGDIR/s1-list.txt")
if [ "$S1N" != 0 ]; then
  # A csproj that still carries the old token must be one M ADDS (cut at an older template), never an older project.
  [ "$(LC_ALL=C comm -23 "$LOGDIR/s1-list.txt" "$LOGDIR/newM-union.txt" | grep -c .)" = 0 ] || die "S1 COUNT lists a csproj M did not add (s1-list.txt vs newM-union.txt)"
  : > "$LOGDIR/s1-eol-before.txt"
  while IFS= read -r f; do
    echo "$f crlf=$(grep -c $'\r$' "$f") lines=$(wc -l < "$f") old=$(grep -cF "$OLD" "$f")" >> "$LOGDIR/s1-eol-before.txt"
  done < "$LOGDIR/s1-list.txt"
  xargs -0 sed -b -i "s|$OLD|$NEW|" < "$LOGDIR/s1-list.z" > "$LOGDIR/s1-sed.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "S1 sed rc=$rc"
  while IFS= read -r f; do
    b=$(grep -F "$f " "$LOGDIR/s1-eol-before.txt"); crl=$(grep -c $'\r$' "$f"); lin=$(wc -l < "$f")
    [ "$(grep -cF "$OLD" "$f")" = 0 ] && [ "$(grep -cF "$NEW" "$f")" = 1 ] || die "$f: token counts after the sed"
    [ "$b" = "$f crlf=$crl lines=$lin old=1" ] || die "$f: line endings/count moved ($b -> crlf=$crl lines=$lin)"
  done < "$LOGDIR/s1-list.txt"
  stamp "1 S1: COUNT=$S1N of the $NEWN csproj M adds were cut at an older template ($(tr '\n' ' ' < "$LOGDIR/s1-list.txt")); sed -b, line endings kept"
else
  stamp "1 S1: COUNT=0 (TRAIN L's master 0; M adds $NEWN csproj, all already conditioned): nothing edited"
fi

# --------------------------------------------------------------------------------------- STEP 2: optin.py insert
STEP=2-optin
got='not run (step 1 edited nothing)'
if [ "$S1N" != 0 ]; then
  python "$(w "$W/$OPT")" insert "$(w "$W")" > "$LOGDIR/optin-insert.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "optin.py insert rc=$rc"
  got=$(grep -a '^insert:' "$LOGDIR/optin-insert.log" | tr -d '\r')
  case "$got" in "insert: $S1N edited, "*) ;; *) die "optin insert read '$got', expected '$S1N edited'" ;; esac
fi
stamp "2 optin insert: $got"

# --------------------------------------------------------------------------------------- STEP 3: optin.py check
STEP=3-optin-check
python "$(w "$W/$OPT")" check "$(w "$W")" > "$LOGDIR/optin-check.log" 2>&1; rc=$?
[ "$rc" = 0 ] && grep -aqx 'CHECK PASS' <(tr -d '\r' < "$LOGDIR/optin-check.log") || die "optin.py check rc=$rc / no CHECK PASS"
got3=$(grep -a '^check:' "$LOGDIR/optin-check.log" | tr -d '\r')
if [ -n "$EXP_DESC" ] && [ -n "$EXP_TH" ]; then
  [ "$got3" = "check: $EXP_DESC template descendants, $EXP_TH test hosts, 1 hand-owned skipped" ] || die "optin check read '$got3' (EXP_DESC=$EXP_DESC EXP_TH=$EXP_TH)"
  stamp "3 optin check: CHECK PASS ($got3) == EXP_DESC/EXP_TH"
else
  stamp "3 optin check: CHECK PASS ($got3) -- counts REPORTED, not asserted (EXP_DESC/EXP_TH are TO BE SET BY COORD)"
fi

# ------------------------------------------------------- STEP 4: CORPUS, the union converter's own emission (G2)
STEP=4-regen
: > "$LOGDIR/regen-applied.txt"
REGV='skipped'; RCLASS=''
if [ "$REGEN" = apply ]; then
  f=$(freegb); [ "${f:-0}" -ge 30 ] || die "free disk ${f}G < 30G on H: before the emission check"
  MARK="$ES/REGEN-U.txt"
  if [ -f "$MARK" ] && [ "$(tr -d '\r' < "$MARK")" = "$HEADF $BASEF" ] && [ -d "$ES/U-windows" ] && [ -d "$ES/U-linux" ] && [ -d "$ES/U-darwin" ] && [ -f "$ES/attr-windows.txt" ]; then
    stamp "4 regen: RE-USING the kept emission evidence in $ES (same assembled head ${HEADF:0:10}, same base ${BASEF:0:10}); the emission check is not run again"
  else
    # Read-only for the worktree: tM-emitcheck.sh reads the two COMMITS and writes only its scratch (six -stdlib
    # conversions, one at a time, each into its own seeded root: floors 1 and 2 are the script's own).
    env W="$W" M="$BASEF" U="$HEADF" LOGDIR="$LOGDIR" TAG=regen CSPROJ_GATE=0 KEEP_U_ROOTS=1 TM_EMIT_SCRATCH="$ES" TM_LOCK_HELD=1 bash "$SD/tM-emitcheck.sh" > "$LOGDIR/regen-emitcheck.log" 2>&1; erc=$?
    ev=$(grep -a '^EMITCHECK VERDICT' "$LOGDIR/regen-emitcheck.log" | tail -n 1 | tr -d '\r')
    case "$ev" in *"plants=OK emit=OK"*) ;; *) die "the emission check did not MEASURE (rc=$erc): ${ev:-no verdict line} :: $(grep -aE '^(ABORT|PLANT CONTROL|EMIT RC)' "$LOGDIR/regen-emitcheck.log" | tr '\r\n' '  ' | cut -c1-400)" ;; esac
    case "$ev" in *"handown-written=0 "*) ;; *) die "the emission check reads a hand-owned file written by a conversion: $ev (regen-emitcheck.log, HANDOWN WRITTEN lines)" ;; esac
    printf '%s %s\n' "$HEADF" "$BASEF" > "$MARK"
    stamp "4 regen: emission check at the assembled head rc=$erc (non-zero is EXPECTED here when the list below is non-empty): $ev"
  fi
  for os in windows linux darwin; do
    stamp "  4 regen $os: union-attributable=$(grep -c . "$ES/attr-$os.txt") union-only siblings=$(grep -c . "$ES/sa-$os" 2>/dev/null) :: $(cut -d' ' -f1 "$ES/attr-$os.txt" | head -n 12 | tr '\n' ' ')"
  done
  sib=0; [ "$SIBLINGS" = refresh ] && sib=1
  # Verify round 2 (floor 13): tM-regen-apply.py's refusal arm had no control ("a wrong acceptance is caught by the
  # applied == changed compare" covers the copy, not the refusal). When the emission check lists anything, the SAME check
  # is run first with an allow pattern that admits nothing: EXPECT rc 1, every listed path refused, nothing written
  # (check mode never writes). Seconds. A control that does not fire stops the fixup: the real check proves nothing.
  if [ "$(cat "$ES"/attr-windows.txt "$ES"/attr-linux.txt "$ES"/attr-darwin.txt 2>/dev/null | grep -c .)" != 0 ]; then
    python -B "$(w "$SD/tM-regen-apply.py")" check "$(w "$W")" "$(w "$ES")" "$(w "$LOGDIR/regen-control-unused.txt")" "$sib" '^$' > "$LOGDIR/regen-control.log" 2>&1; rc=$?
    cl=$(grep -a '^REGEN check:' "$LOGDIR/regen-control.log" | tr -d '\r' | tail -n 1)
    cli=$(printf '%s' "$cl" | sed -n 's/.* listed=\([0-9]*\) .*/\1/p'); cre=$(printf '%s' "$cl" | sed -n 's/.* refused=\([0-9]*\) .*/\1/p')
    [ "$rc" = 1 ] && [ -n "$cli" ] && [ "$cli" != 0 ] && [ "$cli" = "$cre" ] && [ "$(git diff --name-only -- src/core | grep -vxF -f "$LOGDIR/s1-list.txt" | grep -c .)" = 0 ] \
      || die "the regen refusal CONTROL did not fire (allow=/^\$/: rc=$rc, want 1; listed=${cli:-unread} refused=${cre:-unread}, want equal and > 0; nothing may be written): tM-regen-apply.py's refusal arm is not measuring (regen-control.log)"
    stamp "  4 regen control (allow=/^\$/ admits nothing): rc=$rc listed=$cli refused=$cre, nothing written :: CONTROL FIRED"
  else
    stamp "  4 regen control: not run (the emission check lists no union-attributable path: nothing for the refusal arm to refuse)"
  fi
  python -B "$(w "$SD/tM-regen-apply.py")" check "$(w "$W")" "$(w "$ES")" "$(w "$LOGDIR/regen-applied.txt")" "$sib" "$REGEN_ALLOW" > "$LOGDIR/regen-check.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "regen check rc=$rc (nothing written): $(grep -a '^REGEN' "$LOGDIR/regen-check.log" | tr -d '\r' | head -n 6 | tr '\n' ' ' | cut -c1-700) -- a regenerated path outside the allowed set is for COORD to rule (REGEN_ALLOW=<pattern> at the relaunch admits it by COORD's explicit call; or it goes back to its seat): the emission evidence is kept, so the re-run takes minutes. REGEN_ALLOW was /$REGEN_ALLOW/ ($REGEN_SRC)"
  python -B "$(w "$SD/tM-regen-apply.py")" apply "$(w "$W")" "$(w "$ES")" "$(w "$LOGDIR/regen-applied.txt")" "$sib" "$REGEN_ALLOW" > "$LOGDIR/regen-apply.log" 2>&1; rc=$?
  [ "$rc" = 0 ] || die "regen apply rc=$rc (regen-apply.log)"
  LC_ALL=C sort -o "$LOGDIR/regen-applied.txt" "$LOGDIR/regen-applied.txt"
  # (verify round 1: a csproj step 1 repaired under src/core is step 1's, s1-list.txt, and is not a regenerated file)
  git diff --name-only -- src/core | grep -vxF -f "$LOGDIR/s1-list.txt" | LC_ALL=C sort > "$LOGDIR/regen-changed.txt"
  cmp -s "$LOGDIR/regen-applied.txt" "$LOGDIR/regen-changed.txt" || die "the corpus files git reads changed are not exactly the applied list (regen-applied.txt / regen-changed.txt)"
  REGV="$(grep -c . "$LOGDIR/regen-applied.txt") file(s): $(tr '\n' ' ' < "$LOGDIR/regen-applied.txt" | cut -c1-600)"
  git diff --numstat -- 'src/core/*.cs' 'src/core/*.cs.auto' > "$LOGDIR/regen-numstat.txt"
  stamp "4 regen: applied $REGV :: $(grep -a '^REGEN apply' "$LOGDIR/regen-apply.log" | tr -d '\r')"
  while IFS= read -r l; do stamp "    numstat $l"; done < <(head -n 20 "$LOGDIR/regen-numstat.txt")
  # Verify round 1: what the copied files CHANGE, by content class (a READING for the commit message, which states what
  # was measured and attributes nothing by fixed text): G's frame class and position-map re-encodes, or OTHER.
  git diff -U0 -- 'src/core/*.cs' 'src/core/*.cs.auto' > "$LOGDIR/regen.patch"
  hp hunkclass "$(w "$LOGDIR/regen.patch")" > "$LOGDIR/regen-class.log" 2>&1
  RCLASS=$(grep -a '^HUNKCLASS-VERDICT' "$LOGDIR/regen-class.log" | tr -d '\r' | cut -c1-300)
  stamp "  4 regen class (a reading): ${RCLASS:-unread}"
  if grep -q 'package_info\.cs$' "$LOGDIR/regen-applied.txt"; then stdlibmeta 4-regen; fi
else
  stamp "4 regen: SKIPPED by COORD's explicit call (REGEN=$REGEN): the corpus is NOT regenerated here, and the battery's leg E reads the union's uncommitted footprint as union-attributable"
fi

# ------------------------------------------------- STEP 5: GOLDENS the union's own CNR moves (G1), by content class
STEP=5-goldens
: > "$LOGDIR/gold-files.txt"
GOLDV='skipped'; GN=0; GCLASS_RC=0
if [ "$GOLDENS" = regen ]; then
  # The census PREDICTION, from HEAD (read-only): behavioral projects whose Go source holds a go statement and whose
  # emitted C# carries no NoInlining line at all. A weak predicate (a project can carry NoInlining for another reason),
  # so it is scored against the measurement below, never used in its place.
  git grep -l -E '(^|[^A-Za-z0-9_."/])go[[:space:]]+[A-Za-z_][A-Za-z0-9_.]*[[:space:]]*\(' HEAD -- 'src/tests/Behavioral/*.go' > "$LOGDIR/gold-go.raw"; rc=$?
  [ "$rc" = 0 ] || die "git grep (go statements) rc=$rc"
  cut -d/ -f4 "$LOGDIR/gold-go.raw" | LC_ALL=C sort -u > "$LOGDIR/gold-go.txt"
  git grep -l -F 'MethodImplOptions.NoInlining' HEAD -- 'src/tests/Behavioral/*.cs' > "$LOGDIR/gold-noinl.raw"; rc=$?
  [ "$rc" = 0 ] || die "git grep (NoInlining) rc=$rc"
  cut -d/ -f4 "$LOGDIR/gold-noinl.raw" | LC_ALL=C sort -u > "$LOGDIR/gold-noinl.txt"
  LC_ALL=C comm -23 "$LOGDIR/gold-go.txt" "$LOGDIR/gold-noinl.txt" > "$LOGDIR/gold-predicted.txt"
  stamp "5 goldens PREDICTION: $(grep -c . "$LOGDIR/gold-go.txt") projects hold a go statement, $(grep -c . "$LOGDIR/gold-predicted.txt") of them carry no NoInlining: [$(tr '\n' ' ' < "$LOGDIR/gold-predicted.txt")]"
  [ "$(git status --porcelain -- src/tests/Behavioral | grep -c '^??')" = 0 ] || die "untracked files under src/tests/Behavioral before the CNR: $(git status --porcelain -- src/tests/Behavioral | grep '^??' | head -n 3 | tr '\n' ' ')"
  # The MEASUREMENT: the union's CNR, in place, no -Revert (the regenerated .cs IS the evidence the copy below needs).
  powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1 > "$LOGDIR/cnr-assembled.log" 2>&1; crc=$?
  nmc=$(grep -acE '\[(NOT MEASURED|transpile FAILED)\]|ENUMERATION BROKEN|DEPTH SORT BROKEN' "$LOGDIR/cnr-assembled.log")
  [ "$nmc" = 0 ] || die "CNR at the assembled head could not MEASURE $nmc package(s) (cnr-assembled.log): $(grep -aE '\[(NOT MEASURED|transpile FAILED)\]' "$LOGDIR/cnr-assembled.log" | head -n 3 | tr '\r\n' '  ' | cut -c1-300)"
  git -c core.quotepath=false status --porcelain -z -- src/tests/Behavioral > "$LOGDIR/gold-status.z"
  : > "$LOGDIR/gold-moved.txt"; : > "$LOGDIR/gold-untracked.txt"; : > "$LOGDIR/gold-harness.txt"
  while IFS= read -r -d '' e <&3; do
    st=${e:0:2}; p=${e:3}
    case "$p" in src/tests/Behavioral/BehavioralTests/*|src/tests/Behavioral/BehavioralRunner/*) echo "$p" >> "$LOGDIR/gold-harness.txt"; continue ;; esac
    # verify round 1: a csproj step 1 repaired (s1-list.txt) is modified against HEAD by step 1, not moved by CNR
    case "$st" in '??') echo "$p" >> "$LOGDIR/gold-untracked.txt" ;; *) grep -qxF -- "$p" "$LOGDIR/s1-list.txt" || echo "$p" >> "$LOGDIR/gold-moved.txt" ;; esac
  done 3< "$LOGDIR/gold-status.z"
  [ "$(grep -c . "$LOGDIR/gold-harness.txt")" = 0 ] || die "CNR left the harness sources dirty: $(head -n 3 "$LOGDIR/gold-harness.txt" | tr '\n' ' ')"
  [ "$(grep -c . "$LOGDIR/gold-untracked.txt")" = 0 ] || die "CNR left UNTRACKED emission (a Go-only behavioral directory: hazard H4, which PRERES read as 0; tM-README.md M2):$(head -n 4 "$LOGDIR/gold-untracked.txt" | tr '\n' ' ')"
  [ "$(grep -c '\.csproj$' "$LOGDIR/gold-moved.txt")" = 0 ] || die "CNR moved a csproj (template drift is step 1's class, not a golden): $(grep '\.csproj$' "$LOGDIR/gold-moved.txt" | head -n 3 | tr '\n' ' ')"
  cut -d/ -f4 "$LOGDIR/gold-moved.txt" | LC_ALL=C sort -u > "$LOGDIR/gold-projects.txt"
  GN=$(grep -c . "$LOGDIR/gold-projects.txt")
  stamp "5 goldens MEASURED: CNR rc=$crc :: $(grep -aE '==> (NO REGRESSION|CHANGED)' "$LOGDIR/cnr-assembled.log" | head -n 1 | tr -d '\r' | cut -c1-200) :: moved files=$(grep -c . "$LOGDIR/gold-moved.txt") in $GN project(s): [$(tr '\n' ' ' < "$LOGDIR/gold-projects.txt")]"
  stamp "  5 goldens prediction scored: moved-not-predicted=[$(LC_ALL=C comm -23 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\n' ' ')] predicted-not-moved=[$(LC_ALL=C comm -13 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\n' ' ')]"
  if [ "$GN" = 0 ]; then
    [ "$crc" = 0 ] || die "CNR rc=$crc with no moved file and nothing unmeasured: read cnr-assembled.log"
    GOLDV='0 projects: the union CNR reads NO REGRESSION (the H1 prediction did not hold, or its goldens were already regenerated)'
  else
    git diff -U0 -- 'src/tests/Behavioral/*.cs' 'src/tests/Behavioral/*.cs.target' > "$LOGDIR/gold-cs.patch"   # the goldens' own kinds (a moved csproj died above; step 1's are not goldens)
    hp hunkclass "$(w "$LOGDIR/gold-cs.patch")" > "$LOGDIR/gold-class.log" 2>&1; rc=$?
    stamp "  5 goldens class: $(grep -a '^HUNKCLASS-VERDICT' "$LOGDIR/gold-class.log" | tr -d '\r')"
    if [ "$rc" != 0 ]; then
      [ "$GOLDEN_CLASS" = any ] || die "a moved golden carries a line kind that is not G's frame class (gold-class.log, gold-cs.patch): READ it; GOLDEN_CLASS=any re-baselines it by COORD's explicit call"
      stamp "  5 goldens: OTHER line kinds present and GOLDEN_CLASS=any: re-baselining by COORD's explicit call"
    fi
    for p in $(cat "$LOGDIR/gold-projects.txt"); do
      # --filter is a SUBSTRING match: the name must select exactly one top-level project
      nm=$(git ls-tree --name-only HEAD src/tests/Behavioral/ | sed 's#.*/##' | grep -c -- "$p")
      [ "$nm" = 1 ] || die "the runner's --filter '$p' would match $nm projects: re-baseline that one by hand"
      powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --update-targets --filter "$p" > "$LOGDIR/gold-update-$p.log" 2>&1; rc=$?
      [ "$rc" = 0 ] && grep -aq 'Updated .cs.target goldens for 1 project' "$LOGDIR/gold-update-$p.log" || die "run-behavioral --update-targets --filter $p rc=$rc: $(grep -aiE 'REFUSED|Updated' "$LOGDIR/gold-update-$p.log" | head -n 2 | tr '\r\n' '  ' | cut -c1-300)"
    done
    git diff --name-only -- src/tests/Behavioral | grep -vxF -f "$LOGDIR/s1-list.txt" | LC_ALL=C sort > "$LOGDIR/gold-files.txt"
    # Every changed file is a .cs or a .cs.target under a project CNR moved, and every .cs with a golden equals it.
    while IFS= read -r gf; do
      case "$gf" in *.cs|*.cs.target) ;; *) die "an unexpected file kind changed under the behavioral root: $gf" ;; esac
      grep -qxF "$(echo "$gf" | cut -d/ -f4)" "$LOGDIR/gold-projects.txt" || die "$gf changed outside the projects CNR moved"
      case "$gf" in *.cs)
        if [ -f "$gf.target" ]; then cmp -s <(tr -d '\r' < "$gf") <(tr -d '\r' < "$gf.target") || die "$gf differs from its golden $gf.target after the re-baseline"; fi ;;
      esac
    done < "$LOGDIR/gold-files.txt"
    git diff -U0 -- 'src/tests/Behavioral/*.cs' 'src/tests/Behavioral/*.cs.target' > "$LOGDIR/gold-all.patch"
    hp hunkclass "$(w "$LOGDIR/gold-all.patch")" > "$LOGDIR/gold-class-all.log" 2>&1; rc=$?; GCLASS_RC=$rc
    [ "$rc" = 0 ] || [ "$GOLDEN_CLASS" = any ] || die "after the re-baseline a golden carries a line kind that is not G's frame class (gold-class-all.log)"
    # The runner's four phases for each re-baselined project: the check the train-assembly skill names.
    for p in $(cat "$LOGDIR/gold-projects.txt"); do
      powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$p" > "$LOGDIR/gold-verify-$p.log" 2>&1; rc=$?
      stamp "  5 goldens verify $p rc=$rc :: $(grep -aE '^\s+(Transpile|Compile|Target|Output)\s+pass' "$LOGDIR/gold-verify-$p.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300)"
      [ "$rc" = 0 ] || die "the four phases of $p are not green after the re-baseline (gold-verify-$p.log)"
    done
    git diff --name-only -- src/tests/Behavioral | grep -vxF -f "$LOGDIR/s1-list.txt" | LC_ALL=C sort | cmp -s - "$LOGDIR/gold-files.txt" || die "the verify run changed tracked behavioral files (compare with gold-files.txt)"
    GOLDV="$GN project(s) [$(tr '\n' ' ' < "$LOGDIR/gold-projects.txt")], $(grep -c . "$LOGDIR/gold-files.txt") files: $(grep -a '^HUNKCLASS-VERDICT' "$LOGDIR/gold-class-all.log" | tr -d '\r' | cut -c1-200)"
  fi
  stamp "5 goldens: $GOLDV"
else
  stamp "5 goldens: SKIPPED by COORD's explicit call (GOLDENS=$GOLDENS): the battery's leg 4 (CNR) then reads any stale golden as CHANGED"
fi

# ------------------------------------------------------------- STEP 6: the battery's PRE-1, predicted; then the purge
STEP=6-shape
hp precheck "$(w "$W")" "$BASE" "$(w "$SEATS_EFF")" worktree > "$LOGDIR/precheck.log" 2>&1; rc=$?
[ "$rc" = 0 ] || die "precheck on the edited worktree rc=$rc: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 3 | tr '\n' ' ' | cut -c1-600)"
stamp "6 shape: precheck on the edited worktree $(grep -a '^PRECHECK' "$LOGDIR/precheck.log")"
# Steps 4 and 5 built a converter and, when a golden moved, the runner and that project's closure: purge them so the
# battery starts from the tree L's battery started from (no build output). Same find the battery's purge uses.
P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
stamp "6 purge: purged=$P remaining=$left tracked-deletions=$(git status --porcelain | grep -c '^ D')"
[ "$left" = 0 ] || die "purge incomplete: $left build directories remain"

# -------------------------------------------------------------------------------- STEP 7: explicit add, signed commit
STEP=7-commit
cat "$LOGDIR/s1-list.txt" "$LOGDIR/regen-applied.txt" "$LOGDIR/gold-files.txt" "$LOGDIR/meta-files.txt" "$LOGDIR/prose-files.txt" | grep . | LC_ALL=C sort -u > "$LOGDIR/expected.txt"   # verify round 2: + the regenerated stdlib-metadata asset and COORD's prose patch, when either exists
git diff --name-only | LC_ALL=C sort > "$LOGDIR/changed.txt"
cmp -s "$LOGDIR/expected.txt" "$LOGDIR/changed.txt" || die "changed set != expected (expected.txt / changed.txt)"
[ "$(git status --porcelain | grep -c '^ D')" = 0 ] || die "tracked deletions (floor 8)"
NFILES=$(grep -c . "$LOGDIR/expected.txt")
EMPTY=''
if [ "$NFILES" != 0 ]; then
  tr '\n' '\0' < "$LOGDIR/expected.txt" > "$LOGDIR/add.z"
  git --literal-pathspecs add --pathspec-from-file="$(w "$LOGDIR/add.z")" --pathspec-file-nul || die "git add"
  git diff --cached --name-only | LC_ALL=C sort | cmp -s - "$LOGDIR/expected.txt" || die "staged set != expected"
else
  EMPTY='--allow-empty'   # nothing was owed: the commit still exists, so the union's shape is the one the battery asserts
fi
{
  # verify round 2: the subject names only the classes this run actually committed (it used to name corpus and goldens
  # whenever any file changed); the battery reads the subject's PREFIX alone.
  SUBJP=''
  [ "$S1N" = 0 ] || SUBJP="$SUBJP S1 on $S1N csproj;"
  [ -s "$LOGDIR/regen-applied.txt" ] && SUBJP="$SUBJP corpus files the union converter emits and no seat committed;"
  [ -s "$LOGDIR/gold-files.txt" ] && SUBJP="$SUBJP goldens the union CNR moves;"
  [ -s "$LOGDIR/meta-files.txt" ] && SUBJP="$SUBJP the regenerated stdlib-metadata asset;"
  [ -s "$LOGDIR/prose-files.txt" ] && SUBJP="$SUBJP a comment-and-prose correction;"
  echo "$FXSUBJ -- $([ "$NFILES" = 0 ] && echo 'nothing owed (S1 0, no golden moved, no corpus file regenerated, the metadata asset in sync)' || echo "${SUBJP# } $NFILES files")"
  echo
  echo "On the assembled union ${HEADF:0:10} ($n seat merges$([ -n "$FIXES" ] && echo " + follow-up merges:$FIXES")$([ "$nprev" = 0 ] || echo " + $nprev earlier fixup commit(s), kept: floor 9") onto TRAIN L's landed master $BASE)."
  echo "(0) stdlib-metadata.txt: $METAV."
  echo "(0b) PROSE (comment and prose only; no row, count or execution config changes): $PROSEV."
  # Verify round 3 (RULED): this block states what THIS run measured, in the reader's own lines. It used to say, as
  # fixed text whatever the seat list held, that the roster "carries G's two row changes and master's TRAIN L provenance
  # block" and that Goroutine.cs "holds checkdead's and the entry classification's identifiers": true while G is a row,
  # false in a SIGNED commit the moment that row is dropped or re-pointed, and nothing would have said so.
  echo "PRECHECK, no content change (tM-helpers.py precheck on the tree this commit holds, step 6; the reader's own lines):"
  grep -a '^PRECHECK ' "$LOGDIR/precheck.log" | tr -d '\r' | cut -c1-200 | sed 's/^/    /'
  echo "    COUNT ok on $(grep -acE '^ok +COUNT ' "$LOGDIR/precheck.log") of $(grep -acE '^(ok +|FAIL )COUNT ' "$LOGDIR/precheck.log") registration files; REG ok on $(grep -acE '^ok +REG ' "$LOGDIR/precheck.log") of $(grep -acE '^(ok +|FAIL )REG ' "$LOGDIR/precheck.log")."
  grep -aE '^ok +(BOTH:|BOTH [^ ]+ docs/ValidatedTestPackages\.md |ROSTER|SNAPSHOT|PROOF|H3|H4)' "$LOGDIR/precheck.log" | tr -d '\r' | sed -E 's/^ok +//' | cut -c1-300 | sed 's/^/    /'
  echo "    Roster guard: passes under pwsh 7 and Windows PowerShell 5.1 with $EXEC_ROWS_EXPECT row(s) carrying an execution config."
  echo "    Outparity: $(tr -d '\r' < "$LOGDIR/outparity.log" | head -n 1 | cut -c1-200)"
  echo "(1) S1: COUNT $S1N csproj carrying <LangVersion>latest</LangVersion> (TRAIN L's master 0; M adds $NEWN csproj)."
  echo "(2) optin.py insert: $got."
  echo "(3) optin.py check: CHECK PASS ($got3)."
  # Verify round 1 (merge-hazards: "never claim a union CNR the train will measure"; a gate line carries a number or it
  # is prose): every line below states what THIS run measured. The seats are named as the EXPECTED owners, labelled so;
  # the battery's readings are named as a prediction, and only when both steps ran.
  echo "(4) CORPUS: $REGV"
  if [ "$REGEN" = apply ] && [ -s "$LOGDIR/regen-applied.txt" ]; then
    echo "    Each file is the union converter's own seeded emission (tM-emitcheck.sh at ${HEADF:0:10}, three targets), copied"
    echo "    whole where it differs from the union tree; a path the base arm also drifts on is refused, never copied."
    echo "    Review siblings: SIBLINGS=$SIBLINGS. Measured, by content class: ${RCLASS:-unread}"
    sed 's/^/    numstat /' "$LOGDIR/regen-numstat.txt" | head -n 20
    echo "    Allowed set /$REGEN_ALLOW/, $REGEN_SRC"
    echo "    (the seats whose acceptance measured these footprints: seat-footprints H2; ownership is not derived per file)."
  fi
  echo "(5) GOLDENS: $GOLDV"
  if [ "$GOLDENS" = regen ] && [ "$GN" != 0 ]; then
    if [ "$GCLASS_RC" = 0 ]; then
      echo "    Every moved hunk is the go-creator frame class or a position-map re-encode (hunkclass, measured)."
      # verify round 3 (RULED): a sentence about a seat appears only when that seat is a ROW, and its base is read from git
      gsha=$(grep -E "^$REGEN_G_SEAT\|" "$SEATS" | tr -d '\r' | head -n 1 | cut -d'|' -f2)
      [ -z "$gsha" ] || echo "    Expected owner (an expectation, not a measurement): $REGEN_G_SEAT ($gsha), whose own re-baseline covered the projects that existed at its base $(git merge-base "$BASE" "$gsha" | cut -c1-10)."
    else
      echo "    OTHER line kinds are present (gold-class-all.log) and were re-baselined by COORD's call (GOLDEN_CLASS=any)."
    fi
    echo "    Re-baselined through run-behavioral.ps1 --update-targets --filter, the runner's four phases green for each."
  fi
  [ "$REGEN" = apply ] && [ "$GOLDENS" = regen ] \
    && echo "PREDICTED, not measured here: the battery's leg E (M=$BASE vs this commit) union-attributable 0, and leg 4 (CNR) NO REGRESSION."
  echo
  echo "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
} > "$LOGDIR/commit-msg.txt"
git commit -q -S $EMPTY -F "$(w "$LOGDIR/commit-msg.txt")" || die "git commit -S"
NEWH=$(git rev-parse HEAD)
[ "$(git rev-parse HEAD^)" = "$HEADF" ] || die "fixup parent is not the assembled head"
git verify-commit HEAD > "$LOGDIR/verify-commit.log" 2>&1 || die "verify-commit failed"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || die "tracked changes remain after the commit"
stamp "7 COMMITTED ${NEWH:0:10} $(git show --shortstat --format= HEAD | tr -d '\n' | sed 's/^ *//')"
case "$ES" in /h/go2cs-tmp-coord/tMemit*) rm -rf "$ES" && stamp "  emission scratch $ES removed" ;; esac
grep -a '^NOTE' "$LOGDIR/precheck.log" | while IFS= read -r l; do stamp "  STILL OWED (manual): ${l:0:300}"; done
stamp "FIXUP DONE head=${NEWH:0:10} subject='$FXSUBJ' $LISTS (the battery's EXPECT_HEAD; nothing pushed; the battery must read the SAME two lists: launch it from this run folder, or from a copy that holds these two files)"
