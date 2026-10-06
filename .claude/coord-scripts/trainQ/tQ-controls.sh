#!/usr/bin/env bash
# TRAIN Q -- the CONTROLS of every reader Q adds or changes (floor 13: a gate that has never been made to fail proves
# nothing). Nothing here builds, converts or runs a test: each arm feeds a reader planted or kept input and compares
# what it prints with what is written beside it.
#   OUT=<scratch dir under /h/go2cs-tmp-coord/coord-scratch/tQ/> bash tQ-controls.sh
# Arms (each prints 'CONTROL <arm>: ok|FAILED -- ...', or 'CONTROL <arm>: NOT RUN (...)' when its input is not on the box):
#   CB   the battery's cbknown, extracted from tQ-battery.sh between its two markers, over five planted logs: the known
#        self-skip alone (0 findings); beside another test's SKIP (1); the known NAME failing (1); the known name
#        skipping for ANOTHER reason (1); the switch off (1). P's cb-known.control.sh, five of five.
#   EXT  realmod's KNOWN-EXTERNAL class (Q4): a Go-side failure with a network text (EXTERNAL); the same with no such
#        text (FAIL); a C#-side failure with the text in the C# stderr (FAIL); an external name beside an unexplained
#        one (FAIL); a C#-SIDE failure whose GO output holds the text, go=pass (FAIL: review round 1); and, when TRAIN
#        P's run2 is on this box, its REAL x/mod records (sumdb/tlog EXTERNAL, quota exhausted).
#   TC   tcn, the tracked-changes reader (Q5), EXTRACTED from every script that carries one (review round 1: the arm
#        tested its own inline copy, so the vacuous form in a landing script passed): tQ-reread.sh, tQ-land-prep.sh and
#        tQ-land-final-reads.sh (a clean tree 0; one planted change 1; no such tree UNREAD; a Git Bash path with
#        MSYS_NO_PATHCONV=1 UNREAD, the vacuous '0' of P's landing) and tQ-battery.sh's cwd form (0; 1; no repository
#        UNREAD).
#   RT   runtie and rungate, EXTRACTED from both landing scripts (review round 1, the two landing blockers): planted run
#        folders and, when it is on this box, TRAIN P's REAL run2 (rc 6 at d843ff263d). A battery at this head EXIT 0
#        (OK); EXIT 7 with one owed line (OK); a STALE run folder, another head (REFUSED); a red rc (REFUSED) and the
#        same with BATTERY_RC_OK (OK); no rc line (REFUSED); the owed file MISSING (STOP); the owed count not the
#        SUMMARY's (STOP); the parent tie (final OK, prep REFUSED); BATTERY_HEAD_OK with and without ancestry.
#   CS   the assembler's candidate and slot predicates, EXTRACTED from tQ-assemble.sh (review round 1): CAND alone
#        refused; with 'READING IN (...)' released; with the bare two words in prose still refused; a lower-case
#        'candidate' not seen; 2 slot comments refused, SLOTS_OK=2 accepted, SLOTS_OK=1 refused, 0 comments accepted.
#   PF   pdbfloor, EXTRACTED from the three second-run arms (tQ-modules-legs.sh XS2, tQ-linux-legs.sh LM:xsync2,
#        tQ-reread.sh run 1b; review round 1): every count above one passes; all zero FAILS (the 'before = after' that
#        passed on nothing); one package at 1 FAILS; an empty reading FAILS.
#   PS   the battery's pubsym, EXTRACTED (review round 1): a refusal line in a console log counts; one that sits only in
#        the 'full output:' file a sweep log names counts too; a log with neither reads 0.
#   RG   tQ-regen-apply.py rule 4 without a written list (Q10): a flat path all three roots agree on (applies); a flat
#        path one root moved (REFUSED, 'NOT KNOWN'); a GOOS-foldered path (applies); the flat case with output lists
#        that say one target emits it (applies) and that say two do (REFUSED, per-target).
#   ED   emitdrift.py reads BYTES (Q10): a file whose bytes changed under an OLD modification time is drift.
#   TE   te_class's N-PARTIAL class (Q18): TE-plant, three planted hunks (review round 1: the class had no NEGATIVE
#        control, and it gates the fixup's golden step) -- the exact pair N-PARTIAL, hunkclass rc 0; the prefix removed
#        and ' partial ' added with ONE OTHER TOKEN changed OTHER, rc 1; the prefix removed against an unrelated line
#        that holds ' partial ' OTHER, rc 1. TE-carrier, the carrier's own corpus diff from the object store (every
#        hunk N-PARTIAL, 0 OTHER). TE-M, M's two TE controls unchanged in what they count.
#   HW   hostwall's baseline from the previous train's SUMMARY stamp (Q11): P= read, SLOWER set, the wrong leg not read.
#        Review round 1: the plant lists S:os BEFORE S:os/exec, P's real order, so a prefix-matching leg pattern (which
#        takes the last match) reads the wrong leg and fails the arm.
#   RES  tQ-resolve-controls.sh (the ruled resolutions, eight arms of their own).
# Last line: Q-CONTROLS DONE failed=<n> not-run=<n>. EXIT 1 when an arm failed, OR when an arm did NOT RUN and
# NOT_RUN_OK does not name it (review round 1: three arms printed NOT RUN, were not counted, and the suite said
# failed=0). NOT_RUN_OK='EXT-real TE-M' acknowledges arms by name; an arm that then runs anyway is simply run.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
OUT=${OUT:?set OUT to a scratch directory under /h/go2cs-tmp-coord/coord-scratch/tQ/}
case "$OUT" in /h/go2cs-tmp-coord/coord-scratch/tQ/?*) ;; *) echo "ABORT: OUT=$OUT is not under /h/go2cs-tmp-coord/coord-scratch/tQ/"; exit 2 ;; esac
case "$OUT" in *..*) echo "ABORT: OUT holds '..'"; exit 2 ;; esac
REPO=${REPO:-/h/Projects/go2cs}
P_RUN=${P_RUN:-/h/go2cs-tmp-coord/coord-scratch/tP/run2}
# REVIEW ROUND 1: 'rm -rf "$OUT"' accepted ANY existing folder under coord-scratch/tQ/, a battery's run folder among
# them. An existing OUT is deleted only when it carries the marker file this script wrote into it and holds nothing of
# a run; anything else is refused, never deleted.
MARK=.tQ-controls-scratch
if [ -e "$OUT" ]; then
  if [ -d "$OUT" ] && [ -f "$OUT/$MARK" ] && [ ! -e "$OUT/tQ-logs" ] && [ ! -e "$OUT/mod-logs" ] && [ ! -e "$OUT/tQ-fixup-logs" ] && [ ! -e "$OUT/battery.console.log" ]; then rm -rf "$OUT"
  else echo "ABORT: OUT=$OUT exists and is not a scratch folder this script made (no $MARK in it, or it holds a run's tQ-logs / mod-logs / tQ-fixup-logs / battery.console.log): it is NOT deleted. Give a folder that does not exist yet"; exit 2; fi
fi
mkdir -p "$OUT" && : > "$OUT/$MARK" || exit 2
FAILED=0; NOTRUN=''
verdict(){ if [ "$2" = 0 ]; then echo "CONTROL $1: ok -- $3"; else echo "CONTROL $1: FAILED -- $3"; FAILED=$((FAILED + 1)); fi; }
notrun(){ echo "CONTROL $1: NOT RUN ($2)"; NOTRUN="$NOTRUN$1 "; }
hp(){ python -B "$(cygpath -w "$SD/tQ-helpers.py")" "$@"; }
w(){ cygpath -w "$1"; }
frag(){ # script, marker name -> the lines between '# >>> <name>' and '# <<< <name>' (leading blanks allowed)
  sed -n "/^[[:space:]]*# >>> $2/,/^[[:space:]]*# <<< $2/p" "$SD/$1"
}

# ------------------------------------------------------------------------------------------------ CB
frag tQ-battery.sh cbknown > "$OUT/cbknown.frag.sh"
if grep -q '^cbknown(){' "$OUT/cbknown.frag.sh"; then
  K=TestSelectedCgoRefusalPrintsCanonicalDiagnostic; KT='no C compiler: a real cgo load is not possible here'
  cbplant(){ mkdir -p "$OUT/cb-$1"; printf '%s\n' "$2" > "$OUT/cb-$1/conv-m-named.log"; }
  cbplant A "=== RUN   TestA
--- PASS: TestA (0.01s)
=== RUN   $K
    canonicalDiagnostics_test.go:125: $KT
--- SKIP: $K (0.16s)"
  cbplant B "=== RUN   TestA
    x_test.go:1: some other reason
--- SKIP: TestA (0.01s)
=== RUN   $K
    canonicalDiagnostics_test.go:125: $KT
--- SKIP: $K (0.16s)"
  cbplant C "--- PASS: TestA (0.01s)
=== RUN   $K
    canonicalDiagnostics_test.go:130: the diagnostic is wrong
--- FAIL: $K (0.16s)"
  cbplant D "--- PASS: TestA (0.01s)
=== RUN   $K
    canonicalDiagnostics_test.go:99: skipped for a reason nobody ruled on
--- SKIP: $K (0.16s)"
  cbrun(){ # label, expected findings, [off]
    ( stamp(){ :; }; NF=0; finding(){ NF=$((NF + 1)); }
      . "$OUT/cbknown.frag.sh"
      LOGDIR="$OUT/cb-$1"; cbrc=0; [ "$1" = C ] && cbrc=1; cbmiss=''
      for t in TestA $K; do grep -aqE -- "--- PASS: $t \(" "$LOGDIR/conv-m-named.log" || cbmiss="$cbmiss $t"; done
      cbsk=$(grep -acE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-m-named.log")
      if [ "${3:-}" = off ]; then CB_KNOWN_SKIPS=''; else CB_KNOWN_SKIPS="$K|$KT"; fi
      cbknown; [ "$NF" = "$2" ] )
  }
  cbbad=0; cbr=''
  for arm in 'A 0' 'B 1' 'C 1' 'D 1' 'A 1 off'; do if cbrun $arm; then cbr="$cbr[$arm: ok] "; else cbr="$cbr[$arm: WRONG] "; cbbad=1; fi; done
  verdict CB "$cbbad" "five arms 'log expected-findings [switch]': $cbr"
else
  verdict CB 1 "no cbknown function between the markers '# >>> cbknown' and '# <<< cbknown' of tQ-battery.sh"
fi

# ------------------------------------------------------------------------------------------------ EXT
E=$OUT/ext; mkdir -p "$E"
extrec(){ # pkg, go verdict, cs verdict, go-test output text for TestNet, cs stderr text [, a second differing name with no text]
  mkdir -p "$E/root/$1"
  python -B - "$(w "$E/root/$1/go2cs_test_comparison.json")" "$2" "$3" "$4" "$5" "${6:-}" <<'PY'
import json, sys
path, gv, cv, gotext, cstext, extra = sys.argv[1:7]
g = {'TestNet': gv, 'TestOk': 'pass'}; c = {'TestNet': cv, 'TestOk': 'pass'}
errs = ['TestNet: Go="%s" C#="%s"' % (gv, cv)]
ev = json.dumps({'Action': 'output', 'Package': 'example.test/m', 'Test': 'TestNet', 'Output': gotext})
if extra:
    g[extra] = 'fail'; c[extra] = 'pass'; errs.append('%s: Go="fail" C#="pass"' % extra)
errs.append('go test: go test -json -count=1 . failed: exit status 1\n' + ev)
json.dump({'package': 'example.test/m', 'status': 'failing', 'matched': False, 'go': g, 'csharp': c, 'errors': errs,
           'disclosed': [], 'stderr': {'csharp': {'text': cstext}}}, open(path, 'w', encoding='utf-8'))
PY
}
extrec e1 fail pass 'Get "https://example.invalid/x": dial tcp: lookup example.invalid: no such host' ''
extrec e2 fail pass 'got 3, want 4' ''
extrec e3 pass fail 'ok' 'rpc error: code = ResourceExhausted desc = quota exhausted'
extrec e4 fail pass 'read tcp 192.0.2''.1:1: i/o timeout' '' TestOther
# REVIEW ROUND 1: the arm that pins 'go=fail'. A C#-SIDE failure (go=pass, cs=fail) whose GO output for that very test
# holds a network text is NOT external: with the 'g.get(k) != fail' line of external_go removed, e1 to e4 all still
# read as expected and this one read EXTERNAL.
extrec e5 pass fail 'log: dial tcp 192.0.2''.1:443: i/o timeout (retrying)' ''
for p in e1 e2 e3 e4 e5; do printf -- '-tests example.test/m/%s -> out\n' "$p"; done > "$E/run.log"
hp realmod --log "$(w "$E/run.log")" --root "$(w "$E/root")" --module example.test/m e1=2 e2=2 e3=2 e4=3 e5=2 > "$E/realmod.txt" 2>&1; erc=$?
ev1=$(grep -c '^REALMOD example.test/m/e1: EXTERNAL -- .*no such host' "$E/realmod.txt"); ev2=$(grep -c '^REALMOD example.test/m/e2: FAIL -- ' "$E/realmod.txt")
ev3=$(grep -c '^REALMOD example.test/m/e3: FAIL -- ' "$E/realmod.txt"); ev4=$(grep -c '^REALMOD example.test/m/e4: FAIL -- ' "$E/realmod.txt")
ev5=$(grep -c '^REALMOD example.test/m/e5: FAIL -- ' "$E/realmod.txt")
evv=$(grep -c '^REALMOD-VERDICT .* pass=0 known=0 fail=4 external=1 ' "$E/realmod.txt")
[ "$erc" = 1 ] && [ "$ev1$ev2$ev3$ev4$ev5$evv" = 111111 ]
verdict EXT "$?" "planted: Go-side network text EXTERNAL=$ev1, Go-side no text FAIL=$ev2, C#-side text FAIL=$ev3, external beside an unexplained name FAIL=$ev4, C#-side failure with the text in the GO output FAIL=$ev5, verdict line fail=4 external=1: $evv, rc=$erc (EXPECT 1 1 1 1 1 1, rc 1)"
if [ -f "$P_RUN/mod-logs/tests-recurse-xmod.log" ] && [ -f "$P_RUN/out/xmod/src/golang.org/x/mod/sumdb/tlog/go2cs_test_comparison.json" ]; then
  hp realmod --log "$(w "$P_RUN/mod-logs/tests-recurse-xmod.log")" --root "$(w "$P_RUN/out/xmod/src/golang.org/x/mod")" --module golang.org/x/mod modfile=323 module=16 semver=9 sumdb/dirhash=6 sumdb=4 sumdb/note=7 sumdb/storage=1 sumdb/tlog=17 zip=build:TestVCS > "$E/realmod-P.txt" 2>&1; prc=$?
  [ "$prc" = 0 ] && grep -q '^REALMOD golang.org/x/mod/sumdb/tlog: EXTERNAL -- .*TestCertificateTransparency go=fail cs=pass .*quota exhausted' "$E/realmod-P.txt" && grep -q 'pass=8 known=0 fail=0 external=1' "$E/realmod-P.txt"
  verdict EXT-real "$?" "TRAIN P's run2 x/mod records (its battery read sumdb/tlog FAIL): $(grep '^REALMOD-VERDICT' "$E/realmod-P.txt" | cut -c1-120) rc=$prc (EXPECT pass=8 fail=0 external=1, rc 0, TestCertificateTransparency named with 'quota exhausted')"
else
  notrun EXT-real "no kept x/mod records under $P_RUN"
fi

# ------------------------------------------------------------------------------------------------ TC
# REVIEW ROUND 1: every copy is EXTRACTED and run. The arm used to define its own tcn here, so the vacuous
# 'git -C | wc -l' form put into a landing script left this arm ok and the self-check at failed=0 (measured by the
# review on a scratch copy, one site a script).
TR=$OUT/tc; mkdir -p "$TR" "$OUT/norepo" && ( cd "$TR" && git init -q . && git config core.autocrlf false && git -c user.name=ctl -c user.email=ctl@invalid -c commit.gpgsign=false commit -q --allow-empty -m e && printf 'a\n' > f && git add f && git -c user.name=ctl -c user.email=ctl@invalid -c commit.gpgsign=false commit -q -m f ) 2>/dev/null
case "$(uname -s)" in MINGW*|MSYS*) t4want=UNREAD ;; *) t4want=any ;; esac
tcbad=0; tcr=''
for s in tQ-reread.sh tQ-land-prep.sh tQ-land-final-reads.sh; do
  frag "$s" tcn > "$OUT/tcn-$s.frag"
  if ! grep -q '^tcn(){' "$OUT/tcn-$s.frag"; then tcr="$tcr[$s: NO tcn between its markers] "; tcbad=1; continue; fi
  r=$( . "$OUT/tcn-$s.frag"; ( cd "$TR" && git checkout -q -- f ); t1=$(tcn "$TR"); printf 'b\n' >> "$TR/f"; t2=$(tcn "$TR"); t3=$(tcn "$OUT/no-such-tree"); t4=$(MSYS_NO_PATHCONV=1 tcn "$TR"); echo "$t1 $t2 $t3 $t4" )
  set -- $r; [ "$t4want" = any ] && want4=${4:-} || want4=UNREAD
  if [ "${1:-}" = 0 ] && [ "${2:-}" = 1 ] && [ "${3:-}" = UNREAD ] && [ "${4:-}" = "$want4" ]; then tcr="$tcr[$s: $r ok] "; else tcr="$tcr[$s: $r WRONG] "; tcbad=1; fi
done
frag tQ-battery.sh tcn > "$OUT/tcn-battery.frag"
if grep -q '^tcn(){' "$OUT/tcn-battery.frag"; then
  r=$( . "$OUT/tcn-battery.frag"; ( cd "$TR" && git checkout -q -- f ); t1=$(cd "$TR" && tcn); printf 'b\n' >> "$TR/f"; t2=$(cd "$TR" && tcn); t3=$(cd "$OUT/norepo" && GIT_CEILING_DIRECTORIES="$(cygpath -w "$OUT" 2>/dev/null || echo "$OUT")" tcn); echo "$t1 $t2 $t3" )
  set -- $r
  if [ "${1:-}" = 0 ] && [ "${2:-}" = 1 ] && [ "${3:-}" = UNREAD ]; then tcr="$tcr[tQ-battery.sh (the cwd form): $r ok] "; else tcr="$tcr[tQ-battery.sh (the cwd form): $r WRONG] "; tcbad=1; fi
else tcr="$tcr[tQ-battery.sh: NO tcn between its markers] "; tcbad=1; fi
old=$(MSYS_NO_PATHCONV=1 git -C "$TR" status --porcelain --untracked-files=no 2>/dev/null | wc -l)
verdict TC "$tcbad" "every copy extracted, 'clean / one planted change / no such tree / a Git Bash path with MSYS_NO_PATHCONV=1' (EXPECT 0 1 UNREAD $t4want; the battery's cwd form: 0 1 UNREAD): $tcr(the old 'git -C | wc -l' form reads $old on the changed tree with path conversion off: the vacuous count)"

# ------------------------------------------------------------------------------------------------ RT
# REVIEW ROUND 1: the two landing blockers. runtie reads a run folder's own files; rungate decides. Both are extracted
# from EACH landing script and run on the same arms; a planted folder is 'battery.console.log' + 'tQ-logs/SUMMARY.txt'.
rtplant(){ # name, rc ('' = no rc line), pre head, end head, exit ('' = no EXIT line), owed-lines ('' = no OWED line), the owed FILE: a line count, or 'none' = no file
  local d=$OUT/rt-$1 i; mkdir -p "$d/tQ-logs"
  if [ "${7:-0}" != none ]; then mkdir -p "$d/mod-logs"; : > "$d/mod-logs/OWED-rereads.txt"; i=0; while [ "$i" -lt "${7:-0}" ]; do echo "XM-EXTERNAL golang.org/x/mod/sumdb/tlog: a planted owed line $i" >> "$d/mod-logs/OWED-rereads.txt"; i=$((i + 1)); done; fi
  { echo "some console line"; [ -z "$2" ] || echo "battery rc=$2"; } > "$d/battery.console.log"
  { echo "08:00:01 PRE head=$3 (fixup: fixup: TRAIN Q -- x) parent=aaaaaaaaaa tree=bbbbbbbbbb signed=G master=m"
    echo "09:00:00   MOD PRE head=ffffffffff go version go1.24.13"
    echo "09:00:01   MOD END tracked-changes=0 tracked-deletions=0 docs-status-lines=0 head=ffffffffff (fixtures)"
    echo "19:00:00 END tracked-changes=16 (the T legs' material) tracked-deletions=0 head=$4 SIc=[control FIRED]"
    [ -z "$6" ] || echo "19:00:00 OWED AT THE LANDING HEAD (Q4; never a pass by silence) owed-lines=$6: nothing"
    echo "19:00:00 BATTERY DONE"
    [ -z "$5" ] || echo "19:00:01 EXIT $5: no leg outside the four expected-non-zero controls is non-zero"
  } > "$d/tQ-logs/SUMMARY.txt"
}
HD=1111111111; PR=2222222222; OLD=3333333333
rtplant ok0 0 $HD $HD 0 0 0; rtplant ok7 7 $HD $HD 7 1 1; rtplant stale 0 $OLD $OLD 0 0 0; rtplant red 6 $HD $HD 6 0 0
rtplant norc '' $HD $HD 0 0 0; rtplant par 0 $PR $PR 0 0 0; rtplant two 0 $HD $OLD 0 0 0; rtplant noexit 0 $HD $HD '' 0 0
rtplant ok0-nofile 0 $HD $HD 0 0 none; rtplant ok7-empty 7 $HD $HD 7 1 0; rtplant ok0-has1 0 $HD $HD 0 0 1
mkdir -p "$OUT/rt-empty"
rtbad=0; rtr=''
for s in tQ-land-prep.sh tQ-land-final-reads.sh; do
  frag "$s" runtie > "$OUT/runtie-$s.frag"
  if ! grep -q '^runtie(){' "$OUT/runtie-$s.frag" || ! grep -q '^rungate(){' "$OUT/runtie-$s.frag" || ! grep -q '^owedcount(){' "$OUT/runtie-$s.frag"; then rtr="$rtr[$s: NO runtie / rungate / owedcount between its markers] "; rtbad=1; continue; fi
  got=$( . "$OUT/runtie-$s.frag"
    one(){ # label, expected first word, expected rc, mode, plant, head, [parent, ancestor]; the owed count is READ from the plant's own file by owedcount; BATTERY_* from the caller's environment
      local lab=$1 wantw=$2 wantrc=$3 mode=$4 pl=$5 h=$6 par=${7:-} anc=${8:-no} rt g grc ow
      rt=$(runtie "$OUT/rt-$pl"); ow=$(owedcount "$OUT/rt-$pl/mod-logs/OWED-rereads.txt")
      if [ "$mode" = final ]; then g=$(rungate final "$rt" "$h" "$par" "$ow" "$anc"); grc=$?; else g=$(rungate prep "$rt" "$h"); grc=$?; fi
      if [ "${g%%:*}" = "$wantw" ] && [ "$grc" = "$wantrc" ]; then printf '%s=ok ' "$lab"; else printf '%s=WRONG(%s,rc=%s,owed-file=%s) ' "$lab" "${g%%:*}" "$grc" "$ow"; fi
    }
    one prep-ok OK 0 prep ok0 $HD
    one prep-exit7 OK 0 prep ok7 $HD
    one prep-STALE REFUSED 2 prep stale $HD
    one prep-red REFUSED 2 prep red $HD
    BATTERY_RC_OK=6 one prep-red-ruled OK 0 prep red $HD
    one prep-norc REFUSED 2 prep norc $HD
    one prep-noexit REFUSED 2 prep noexit $HD
    one prep-two-heads REFUSED 2 prep two $HD
    one prep-parent REFUSED 2 prep par $HD
    one final-ok OK 0 final ok0 $HD $PR
    one final-parent OK 0 final par $HD $PR
    one final-STALE REFUSED 2 final stale $HD $PR
    BATTERY_HEAD_OK=$OLD one final-headok-ancestor OK 0 final stale $HD $PR yes
    BATTERY_HEAD_OK=$OLD one final-headok-NOT-ancestor REFUSED 2 final stale $HD $PR no
    one final-owed-FILE-ABSENT STOP 1 final ok0-nofile $HD $PR
    one final-exit7-owed1 OK 0 final ok7 $HD $PR
    one final-exit7-file-empty STOP 1 final ok7-empty $HD $PR
    one final-exit0-file-has-1 STOP 1 final ok0-has1 $HD $PR
    one final-red REFUSED 2 final red $HD $PR
    rt=$(runtie "$OUT/rt-empty"); [ "$?" = 1 ] && [ "$rt" = 'rc=? pre=? end=? exit=? owed=?' ] && printf 'no-files=ok ' || printf 'no-files=WRONG(%s) ' "$rt"
    rt=$(runtie "$OUT/rt-ok7"); [ "$rt" = "rc=7 pre=$HD end=$HD exit=7 owed=1" ] && printf 'reads=ok ' || printf 'reads=WRONG(%s) ' "$rt"
    oc="$(owedcount "$OUT/rt-ok0-nofile/mod-logs/OWED-rereads.txt") $(owedcount "$OUT/rt-ok0/mod-logs/OWED-rereads.txt") $(owedcount "$OUT/rt-ok7/mod-logs/OWED-rereads.txt")"; [ "$oc" = 'MISSING 0 1' ] && printf 'owedcount=ok ' || printf 'owedcount=WRONG(%s) ' "$oc"
  )
  case "$got" in *WRONG*) rtbad=1 ;; esac
  [ "$(printf '%s' "$got" | tr ' ' '\n' | grep -c '=ok$')" = 22 ] || rtbad=1
  rtr="$rtr[$s: $(printf '%s' "$got" | tr ' ' '\n' | grep -c '=ok$') of 22 arms ok$(printf '%s' "$got" | tr ' ' '\n' | grep 'WRONG' | tr '\n' ' ' | sed 's/^/; /')] "
done
verdict RT "$rtbad" "runtie + rungate + owedcount extracted from both landing scripts, 22 arms each on planted run folders (a stale run folder REFUSED in both modes; the owed file ABSENT a STOP, never 'none owed'; EXIT 7 needs its owed line in the file; a red needs BATTERY_RC_OK; the parent tie is the final reads' alone): $rtr"
if [ -f "$P_RUN/battery.console.log" ] && [ -f "$P_RUN/tP-logs/SUMMARY.txt" ]; then
  # P's run2 keeps its SUMMARY under tP-logs: read through a planted folder that holds COPIES of the two real files
  mkdir -p "$OUT/rt-Preal/tQ-logs" && cp "$P_RUN/battery.console.log" "$OUT/rt-Preal/battery.console.log" && cp "$P_RUN/tP-logs/SUMMARY.txt" "$OUT/rt-Preal/tQ-logs/SUMMARY.txt"
  pr=$( . "$OUT/runtie-tQ-land-prep.sh.frag"; rt=$(runtie "$OUT/rt-Preal"); g=$(rungate prep "$rt" d843ff263d); printf '%s' "$rt :: rc=$? ${g%%:*}"; g=$(BATTERY_RC_OK=6 rungate prep "$rt" d843ff263d); printf '%s' " :: with BATTERY_RC_OK=6 rc=$? ${g%%:*}"; g=$(BATTERY_RC_OK=6 rungate prep "$rt" 446d2c8ba0); printf '%s' " :: at the head run1 read rc=$? ${g%%:*}" )
  [ "$pr" = 'rc=6 pre=d843ff263d end=d843ff263d exit=6 owed=? :: rc=2 REFUSED :: with BATTERY_RC_OK=6 rc=0 OK :: at the head run1 read rc=2 REFUSED' ]
  verdict RT-real "$?" "TRAIN P's REAL run2 files (its battery ended EXIT 6 at fixup-2 d843ff263d; P stamped no owed-lines): $pr (EXPECT rc=6 pre=end=d843ff263d exit=6 owed=?; REFUSED as it stands; OK only with BATTERY_RC_OK=6; REFUSED for the head P's run1 read, 446d2c8ba0, even with it)"
else
  notrun RT-real "no battery.console.log / tP-logs/SUMMARY.txt under $P_RUN"
fi

# ------------------------------------------------------------------------------------------------ CS
frag tQ-assemble.sh candpred > "$OUT/candpred.frag"; frag tQ-assemble.sh slotpred > "$OUT/slotpred.frag"
if grep -q '^candrow(){' "$OUT/candpred.frag" && grep -q '^slotsok(){' "$OUT/slotpred.frag"; then
  printf '%s\n' '# row comment' 'a-row|0123456789|notes' '#   SLOT (no ref; QUESTION 4): a docs row' '#   SLOT (no ref; QUESTION 6): a BOARD row' > "$OUT/cs-seats2.txt"
  printf '%s\n' '# row comment' 'a-row|0123456789|notes' > "$OUT/cs-seats0.txt"
  cs=$( . "$OUT/candpred.frag"
    t(){ if candrow "$2"; then r=refused; else r=passes; fi; [ "$r" = "$3" ] && printf '%s=ok ' "$1" || printf '%s=WRONG(%s) ' "$1" "$r"; }
    t cand-alone 'C1 -- CAND, CONDITIONAL on a hosted run (stack-on x)' refused
    t cand-released 'C1 -- CAND: READING IN (16:34 10-06, mailbox b040b1eebe) the run is green' passes
    t cand-bare-words 'C1 -- CAND: the hosted READING IN the mailbox is still out' refused
    t lower-case 'G (a candidate 12:24; ACCEPTED 14:59)' passes
    t no-word 'C2 (cut=BASE, 1 commit)' passes
    t word-inside 'the CANDIDATE list; SCANDAL' passes
    s(){ SEATS=$2; . "$OUT/slotpred.frag"; if slotsok "$NSLOT" "$3"; then r=accepted; else r=refused; fi; [ "$r" = "$4" ] && printf '%s=ok ' "$1" || printf '%s=WRONG(%s,n=%s) ' "$1" "$r" "$NSLOT"; }
    s slots2-unset "$OUT/cs-seats2.txt" '' refused
    s slots2-ok2 "$OUT/cs-seats2.txt" 2 accepted
    s slots2-ok1 "$OUT/cs-seats2.txt" 1 refused
    s slots0-unset "$OUT/cs-seats0.txt" '' accepted
  )
  case "$cs" in *WRONG*) csb=1 ;; *) csb=0 ;; esac; [ "$(printf '%s' "$cs" | tr ' ' '\n' | grep -c '=ok$')" = 10 ] || csb=1
  verdict CS "$csb" "the assembler's candidate and slot predicates, extracted, 10 arms: $cs"
else
  verdict CS 1 "no candrow / slotsok between the markers '# >>> candpred' / '# >>> slotpred' of tQ-assemble.sh"
fi

# ------------------------------------------------------------------------------------------------ PF
pfbad=0; pfr=''
for s in tQ-modules-legs.sh tQ-linux-legs.sh tQ-reread.sh; do
  frag "$s" pdbfloor | sed 's/^[[:space:]]*//' > "$OUT/pdbfloor-$s.frag"
  if ! grep -q '^pdbfloor(){' "$OUT/pdbfloor-$s.frag"; then pfr="$pfr[$s: NO pdbfloor between its markers] "; pfbad=1; continue; fi
  got=$( . "$OUT/pdbfloor-$s.frag"
    t(){ pdbfloor "$2"; local rc=$?; [ "$rc" = "$3" ] && printf '%s=ok ' "$1" || printf '%s=WRONG(rc=%s) ' "$1" "$rc"; }
    t P-real 'errgroup=173 semaphore=66 singleflight=67 syncmap=67 ' 0
    t all-zero 'errgroup=0 semaphore=0 singleflight=0 syncmap=0 ' 1
    t one-at-1 'errgroup=173 semaphore=1 singleflight=67 syncmap=67 ' 1
    t empty '' 1
    t not-a-number 'errgroup=x semaphore=66 ' 1
    t two 'errgroup=2 semaphore=2 syncmap=2 ' 0 )
  case "$got" in *WRONG*) pfbad=1 ;; esac; [ "$(printf '%s' "$got" | tr ' ' '\n' | grep -c '=ok$')" = 6 ] || pfbad=1
  pfr="$pfr[$s: $got] "
done
verdict PF "$pfbad" "pdbfloor extracted from the three second-run arms, 6 arms each (P's real counts pass; ALL ZERO fails: the 'before = after' that passed on nothing): $pfr"

# ------------------------------------------------------------------------------------------------ PS
frag tQ-battery.sh pubsym > "$OUT/pubsym.frag"
if grep -q '^pubsym(){' "$OUT/pubsym.frag"; then
  PSD=$OUT/ps; mkdir -p "$PSD"
  RL='go2cs: -tests: the published test host lacks 3 dependency symbol file(s) its build produced'
  printf '%s\n' 'PASS  os  1104 [32s]' > "$PSD/clean.log"
  printf '%s\n' 'FAIL  x  0 [3s]' "$RL" > "$PSD/console.log"
  printf '%s\n' 'converter output line' "$RL" 'another line' > "$PSD/full.txt"
  printf '%s\n' 'FAIL  y  0 [3s]' "    full output: $(w "$PSD/full.txt")" > "$PSD/sweep.log"
  printf '%s\n' 'FAIL  z  0 [3s]' "    full output: $(w "$PSD/no-such-file.txt")" > "$PSD/sweep-gone.log"
  ps=$( . "$OUT/pubsym.frag"; echo "$(pubsym "$PSD/clean.log") $(pubsym "$PSD/console.log") $(pubsym "$PSD/sweep.log") $(pubsym "$PSD/sweep-gone.log") $(pubsym "$PSD/clean.log" "$PSD/console.log" "$PSD/sweep.log" "$PSD/no-such-log.log")" )
  [ "$ps" = '0 1 1 0 2' ]
  verdict PS "$?" "the battery's pubsym, extracted: a clean log / a refusal in the console log / a refusal only in the 'full output:' file the sweep log names / a named file that is gone / all four logs and a missing one = [$ps] (EXPECT 0 1 1 0 2)"
else
  verdict PS 1 "no pubsym function between the markers '# >>> pubsym' and '# <<< pubsym' of tQ-battery.sh"
fi

# ------------------------------------------------------------------------------------------------ RG
rgcase(){ # name, files 'path=wtBytes:winBytes:linBytes:darBytes' ..., then '--' and the attr listing 'os:path' ...
  local name=$1; shift; local d=$OUT/rg-$name o p spec
  mkdir -p "$d/wt" "$d/s"; for o in windows linux darwin; do mkdir -p "$d/s/U-$o"; : > "$d/s/attr-$o.txt"; : > "$d/s/written-U-$o.txt"; : > "$d/s/dm-$o"; done
  while [ "$1" != -- ]; do
    p=${1%%=*}; spec=${1#*=}; IFS=: read -r bw b1 b2 b3 <<< "$spec"
    mkdir -p "$d/wt/$(dirname "$p")"; printf '%s\n' "$bw" > "$d/wt/$p"
    for o in windows:$b1 linux:$b2 darwin:$b3; do [ "${o#*:}" = - ] || { mkdir -p "$d/s/U-${o%%:*}/$(dirname "$p")"; printf '%s\n' "${o#*:}" > "$d/s/U-${o%%:*}/$p"; }; done
    shift
  done; shift
  for a in "$@"; do echo "${a#*:}" >> "$d/s/attr-${a%%:*}.txt"; echo "${a#*:}" >> "$d/s/written-U-${a%%:*}.txt"; done
}
rgrun(){ python -B "$(w "$SD/tQ-regen-apply.py")" check "$(w "$OUT/rg-$1/wt")" "$(w "$OUT/rg-$1/s")" "$(w "$OUT/rg-$1/out.txt")" 0 '^src/core/' > "$OUT/rg-$1.log" 2>&1; echo $?; }
rgcase flat3 'src/core/p/a.cs=old:new:new:new' -- windows:src/core/p/a.cs linux:src/core/p/a.cs darwin:src/core/p/a.cs
rgcase flat1 'src/core/p/a.cs=old:new:old:old' -- windows:src/core/p/a.cs
rgcase goos 'src/core/p/windows/b.cs=old:new:old:old' -- windows:src/core/p/windows/b.cs
rgcase list1 'src/core/p/a.cs=old:new:old:old' -- windows:src/core/p/a.cs
echo src/core/p/a.cs > "$OUT/rg-list1/s/emitted-U-windows.txt"; : > "$OUT/rg-list1/s/emitted-U-linux.txt"; : > "$OUT/rg-list1/s/emitted-U-darwin.txt"
rgcase list2 'src/core/p/a.cs=old:new:old:old' -- windows:src/core/p/a.cs
echo src/core/p/a.cs > "$OUT/rg-list2/s/emitted-U-windows.txt"; echo src/core/p/a.cs > "$OUT/rg-list2/s/emitted-U-linux.txt"; : > "$OUT/rg-list2/s/emitted-U-darwin.txt"
r1=$(rgrun flat3); r2=$(rgrun flat1); r3=$(rgrun goos); r4=$(rgrun list1); r5=$(rgrun list2)
[ "$r1" = 0 ] && grep -q 'applicable=1 .*refused=0' "$OUT/rg-flat3.log" && [ "$r2" = 1 ] && grep -q 'REGEN REFUSED src/core/p/a.cs: .*NOT KNOWN' "$OUT/rg-flat1.log" \
  && [ "$r3" = 0 ] && grep -q 'applicable=1 .*refused=0' "$OUT/rg-goos.log" && [ "$r4" = 0 ] && grep -q 'applicable=1 .*refused=0' "$OUT/rg-list1.log" \
  && [ "$r5" = 1 ] && grep -q "REGEN REFUSED src/core/p/a.cs: the linux run emits this path (the converter's output list" "$OUT/rg-list2.log"
verdict RG "$?" "flat, three roots agree rc=$r1 (EXPECT 0, applies); flat, one root moved rc=$r2 (EXPECT 1, 'NOT KNOWN'); GOOS folder rc=$r3 (EXPECT 0); output lists, one emitter rc=$r4 (EXPECT 0); output lists, two emitters rc=$r5 (EXPECT 1, per-target)"

# ------------------------------------------------------------------------------------------------ ED
D=$OUT/ed; mkdir -p "$D/root/src/core/x" "$D/seed/src/core/x"
for f in same old new; do printf 'seed %s\n' "$f" > "$D/seed/src/core/x/$f.cs"; cp "$D/seed/src/core/x/$f.cs" "$D/root/src/core/x/$f.cs"; done
printf 'emitted old\n' > "$D/root/src/core/x/old.cs"; touch -d '2020-01-01 00:00:00' "$D/root/src/core/x/old.cs" "$D/root/src/core/x/same.cs"
touch "$D/sentinel"; sleep 1; printf 'emitted new\n' > "$D/root/src/core/x/new.cs"
edn=$(python -B "$(w "$SD/emitdrift.py")" "$(w "$D/root")" "$(w "$D/seed")" "$(w "$D/sentinel")" "$(w "$D/written.txt")" "$(w "$D/drift.txt")" 2>&1 | tr -d '\r')
[ "$(tr '\n' ' ' < "$D/drift.txt")" = 'src/core/x/new.cs src/core/x/old.cs ' ] && [ "$(tr '\n' ' ' < "$D/written.txt")" = 'src/core/x/new.cs ' ] && [ "$edn" = '1 2 0 1' ]
verdict ED "$?" "drift=[$(tr '\n' ' ' < "$D/drift.txt")] (EXPECT new.cs AND old.cs: bytes, whatever the time) written-by-time=[$(tr '\n' ' ' < "$D/written.txt")] (EXPECT new.cs alone: a reading) counts=[$edn] (EXPECT '1 2 0 1': one file changed under an old time)"

# ------------------------------------------------------------------------------------------------ TE
# REVIEW ROUND 1: TE-plant, the class's own POSITIVE and NEGATIVE controls, on this box with no object store. With
# N-PARTIAL loosened to pair a removed prefix line with ANY added line that holds ' partial ', TE-carrier and TE-M
# both stayed ok (measured by the review): hunkclass rc 0 on that class is what lets the fixup re-baseline a golden.
TP=$OUT/te-plant; mkdir -p "$TP"
teplant(){ # name, removed line, added line
  printf '%s\n' 'diff --git a/src/tests/Behavioral/X/main.cs b/src/tests/Behavioral/X/main.cs' '--- a/src/tests/Behavioral/X/main.cs' '+++ b/src/tests/Behavioral/X/main.cs' '@@ -10 +10 @@' "-$2" "+$3" > "$TP/$1.patch"
}
teplant pos '    [MethodImpl(MethodImplOptions.NoInlining)] internal static void table(int a) {' '    internal static partial void table(int a) {'
teplant neg-token '    [MethodImpl(MethodImplOptions.NoInlining)] internal static void table(int a) {' '    internal static partial void tablx(int a) {'
teplant neg-other '    [MethodImpl(MethodImplOptions.NoInlining)] internal static void table(int a) {' '    public static partial void other() {'
teplant neg-two '    [MethodImpl(MethodImplOptions.NoInlining)] internal static void table(int a) {' '    internal static partial partial void table(int a) {'
tep=''; tepbad=0
for c in 'pos N-PARTIAL 0' 'neg-token OTHER 1' 'neg-other OTHER 1' 'neg-two OTHER 1'; do
  set -- $c
  tl=$(hp teattr "$(w "$TP/$1.patch")" | tr -d '\r' | head -n 1); hp hunkclass "$(w "$TP/$1.patch")" > "$TP/$1.class.txt" 2>&1; hrc=$?
  np=$(printf '%s' "$tl" | sed -n 's/.* n-partial=\([0-9]*\) .*/\1/p'); ot=$(printf '%s' "$tl" | sed -n 's/.* other=\([0-9]*\) .*/\1/p')
  if [ "$2" = N-PARTIAL ]; then cls=$([ "$np" = 1 ] && [ "$ot" = 0 ] && echo N-PARTIAL || echo "np=$np,other=$ot"); else cls=$([ "$np" = 0 ] && [ "$ot" = 1 ] && echo OTHER || echo "np=$np,other=$ot"); fi
  if [ "$cls" = "$2" ] && [ "$hrc" = "$3" ]; then tep="$tep[$1: $cls rc=$hrc ok] "; else tep="$tep[$1: $cls rc=$hrc WRONG, EXPECT $2 rc $3] "; tepbad=1; fi
done
vl=$(grep '^HUNKCLASS-VERDICT' "$TP/pos.class.txt" | tr -d '\r'); case "$vl" in *' n-partial-hunks=1 partial-lines=1 g-frame-hunks=0 map-only-hunks=0 other-hunks=0 n-partial-files=1 '*) ;; *) tepbad=1; tep="$tep[the verdict line's counts WRONG: ${vl:0:160}] " ;; esac
verdict TE-plant "$tepbad" "planted hunks, 'class hunkclass-rc': the exact pair; the prefix removed and partial added with ONE OTHER TOKEN changed; against an unrelated partial line; two partial words: $tep(and the positive's verdict line carries n-partial-hunks=1 ... n-partial-files=1, the counts the fixup's golden step reads)"
BASEC=${TE_BASE:-7098b8d3f9}; CARRIER=${TE_CARRIER:-e7fcff2244}   # two fixed OBJECTS of a control: the carrier's tip and the commit it was cut on (not the train's base, which is read at launch)
if git -C "$REPO" cat-file -e "$BASEC^{commit}" 2>/dev/null && git -C "$REPO" cat-file -e "$CARRIER^{commit}" 2>/dev/null; then
  git -C "$REPO" diff -U0 "$BASEC" "$CARRIER" -- src/core ':!src/core/golib' > "$OUT/carrier-U0.patch"
  tl=$(hp teattr "$(w "$OUT/carrier-U0.patch")" | tr -d '\r' | head -n 1); hp hunkclass "$(w "$OUT/carrier-U0.patch")" > "$OUT/carrier-class.txt" 2>&1; hrc=$?
  tot=$(printf '%s' "$tl" | sed -n 's/^TE files=[0-9]* hunks=\([0-9]*\) .*/\1/p'); np=$(printf '%s' "$tl" | sed -n 's/.* n-partial=\([0-9]*\) .*/\1/p'); ot=$(printf '%s' "$tl" | sed -n 's/.* other=\([0-9]*\) .*/\1/p')
  [ -n "$tot" ] && [ "$tot" != 0 ] && [ "$np" = "$tot" ] && [ "$ot" = 0 ] && [ "$hrc" = 0 ]
  verdict TE-carrier "$?" "the carrier's own corpus diff ($BASEC..$CARRIER, src/core less golib): $tl :: hunkclass rc=$hrc (EXPECT every hunk N-PARTIAL, other=0, rc 0)"
else
  notrun TE-carrier "$BASEC or $CARRIER is not in $REPO's object store"
fi
MC=${TE_CONTROLS:-$SD/../trainP/controls}
if [ -f "$MC/te-pos-goframe.patch" ] && [ -f "$MC/te-neg-i9-testing.patch" ]; then
  a=$(hp teattr "$(w "$MC/te-pos-goframe.patch")" | tr -d '\r' | head -n 1); hp hunkclass "$(w "$MC/te-pos-goframe.patch")" > /dev/null 2>&1; a2=$?
  b=$(hp teattr "$(w "$MC/te-neg-i9-testing.patch")" | tr -d '\r' | head -n 1); hp hunkclass "$(w "$MC/te-neg-i9-testing.patch")" > /dev/null 2>&1; b2=$?
  case "$a" in 'TE files=3 hunks=5 g-frame=4 (noinline-lines=2 using-lines=2, in 2 file(s); g-frame-only files=2) map-only=1 other=0 n-partial=0 '*) ac=0 ;; *) ac=1 ;; esac
  case "$b" in 'TE files=1 hunks=25 g-frame=0 (noinline-lines=0 using-lines=0, in 0 file(s); g-frame-only files=0) map-only=0 other=25 n-partial=0 '*) bc=0 ;; *) bc=1 ;; esac
  [ "$ac$bc" = 00 ] && [ "$a2" = 0 ] && [ "$b2" = 1 ]
  verdict TE-M "$?" "M's two controls (trainP/controls, byte copies since M): positive [$(echo "$a" | cut -c1-110)] hunkclass rc=$a2 (EXPECT 0); negative [$(echo "$b" | cut -c1-110)] hunkclass rc=$b2 (EXPECT 1)"
else
  notrun TE-M "no te-pos-goframe.patch / te-neg-i9-testing.patch under $MC (a per-run copy has no ../trainP beside it: give TE_CONTROLS=<the hnd trainP/controls folder>)"
fi

# ------------------------------------------------------------------------------------------------ HW
H=$OUT/hw; mkdir -p "$H"
printf '%s\n' '{"events":[{"test":"TestA","action":"pass","elapsed":1.0},{"test":"","action":"pass","elapsed":200.4}]}' > "$H/results.json"
# REVIEW ROUND 1: S:os FIRST, S:os/exec second (P's real SUMMARY order). hostwall takes the LAST matching line, so with
# the two the other way round a prefix-matching leg pattern still read S:os right and this arm stayed ok.
printf '%s\n' '16:19:10   S:os:   PASS  os  1104 [32s] :: HOSTWALL cs-ho''st=190s (pass)' '16:52:50   S:os/exec:   PASS  os/exec  116 [189s] :: DEADLOCK total=0 record=fresh (S:os/exec) :: HOSTWALL cs-ho''st=129s (pass)' > "$H/SUMMARY.txt"
h1=$(hp hostwall 0 "$(w "$H/results.json")" --prev-summary "$(w "$H/SUMMARY.txt")" --leg S:os/exec | tr -d '\r')
h2=$(hp hostwall 0 "$(w "$H/results.json")" --prev-summary "$(w "$H/SUMMARY.txt")" --leg S:os | tr -d '\r')
h3=$(hp hostwall 0 "$(w "$H/results.json")" --prev-summary "$(w "$H/SUMMARY.txt")" --leg S:net | tr -d '\r')
[ "$h1" = 'HOSTWALL cs-ho''st=200s (pass) P=129s (pass) x1.55 SLOWER' ] && [ "$h2" = 'HOSTWALL cs-ho''st=200s (pass) P=190s (pass) x1.05' ] && case "$h3" in 'HOSTWALL cs-ho''st=200s (pass) P=none '*) true ;; *) false ;; esac
verdict HW "$?" "[$h1] (EXPECT P=129s x1.55 SLOWER) [$h2] (EXPECT P=190s x1.05: the leg S:os, which is listed BEFORE S:os/exec, so a prefix match would read 129) [$(echo "$h3" | cut -c1-50)] (EXPECT P=none)"

# ------------------------------------------------------------------------------------------------ RES
OUT="$OUT/resolve" bash "$SD/tQ-resolve-controls.sh" > "$OUT/resolve-controls.log" 2>&1; rrc=$?
verdict RES "$rrc" "$(tail -n 1 "$OUT/resolve-controls.log" | cut -c1-120) (its arm lines: $OUT/resolve-controls.log)"

# REVIEW ROUND 1: an arm that did NOT RUN is counted, and fails the suite unless NOT_RUN_OK names it.
NRBAD=''; for a in $NOTRUN; do case " ${NOT_RUN_OK:-} " in *" $a "*) ;; *) NRBAD="$NRBAD$a " ;; esac; done
nnr=$(echo $NOTRUN | wc -w)
echo "Q-CONTROLS DONE failed=$FAILED not-run=$nnr$([ "$nnr" = 0 ] || echo " [${NOTRUN% }]; acknowledged by NOT_RUN_OK: [${NOT_RUN_OK:-}]; NOT acknowledged: [${NRBAD% }]") (scratch: $OUT)"
[ "$FAILED" = 0 ] && [ -z "$NRBAD" ]
