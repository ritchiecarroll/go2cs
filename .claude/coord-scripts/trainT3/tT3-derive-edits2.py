# TRAIN T3 -- the anchored edits of the CONTROLS (and the i9tree marker form), applied after tT3-derive-edits.py.
# Same rules: every anchor asserted, each file written only after all its edits applied. Run once by the derive.
#   python -B tT3-derive-edits2.py <this folder>
import os, re, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
D = sys.argv[1]
def load(f):
    with open(os.path.join(D, f), encoding='utf-8', newline='') as fh: return fh.read()
def save(f, t):
    assert '\r' not in t, f
    with open(os.path.join(D, f), 'w', encoding='utf-8', newline='\n') as fh: fh.write(t)
class Ed:
    def __init__(self, f): self.f = f; self.t = load(f)
    def rep(self, old, new, count=1):
        n = self.t.count(old); assert n == count, f'{self.f}: anchor x{n} (want {count}): {old[:90]!r}'
        self.t = self.t.replace(old, new); return self
    def before(self, a, text): return self.rep(a, text + a)
    def after(self, a, text): return self.rep(a, a + text)
    def resub(self, rx, new, count=1):
        t, n = re.subn(rx, new, self.t, flags=re.S); assert n == count, f'{self.f}: regex x{n} (want {count}): {rx[:90]!r}'
        self.t = t; return self
    def done(self): save(self.f, self.t)

# the marker form frag() reads ('# >>> <name>' at the start of the line)
e = Ed('tT3-assemble.sh')
e.rep("# T3 (T3-3) >>> i9tree (tT3-controls.sh arm I9 extracts the lines between the markers and runs them on planted heads)\n",
      "# >>> i9tree (T3-3; tT3-controls.sh arm I9 extracts the lines between the markers and runs them on planted heads)\n")
e.done()

e = Ed('tT3-controls.sh')
e.after('#!/usr/bin/env bash\n', """# TRAIN T3 -- the CONTROLS (2026-10-10; derived from TRAIN FL's run2 kit, tT3-CHANGES.md). Nothing here builds,
# converts or runs a test: each arm feeds a reader planted input, kept records or git objects. Against FL:
#   DROPPED  FL-plant, FL-real, FL-gold, FL-Q (the face lift's classes are off at T3: T3-4)
#   FL-OFF   the switch itself: every planted face-lift POSITIVE now reads OTHER (flcontrol rc 1, no FACELIFT line;
#            hunkclass on the positives rc 1), the negatives stay OTHER
#   RT-real  TRAIN FL's REAL run2 (EXIT 0 at 9b7dfdb2ec) and FL's landing route (fixup 9b7dfdb2ec, refresh 0574b8336c,
#            fixup-2 56f0f1f254), in place of Q's
#   FX9      the real chain read is FL's (the same shape as Q's: fixup, refresh, fixup-2)
#   TABLE    tT3-assemble.sh TABLE_ONLY on the REAL 24-row list (TABLE OK, every contains / in-head / stack-on / after
#            line read) and on three planted lists: a 'contains' that is no ancestor, an 'in-head' no earlier row
#            carries, rows 19 and 20 swapped (each REFUSED by name; nothing created)
#   I9       i9tree extracted from the assembler: the i9 union against itself with the posted tree (EQUAL), the base
#            against it (DIFFERENT, paths listed), a wrong posted tree (REFUSED), no ref / an unknown sha (rc 2)
#   CN       the battery's cnralias extracted: row 5's REAL diff reads exactly the two ruled names; a planted code line
#            (an up-to-date skip's shape) is 'other', a removed line is 'removed'
#   TP       the battery's tpread extracted, on planted runner logs (5 rows ok; a DIFFERS; 4 rows; the consumer's
#            expectation; CRLF rows)
#   GN       every GN_TRIM name is declared at the i9 union's tree; hp trx reads Passed / Failed / NOT FOUND by name
# FL's header follows.
""")
# --- RT-real: TRAIN FL's run2 and FL's landing route
e.resub(r"Q_RUN_CTL=\$\{Q_RUN_CTL:-/h/go2cs-tmp-coord/coord-scratch/tQ/run1\}\nif .*?\n  notrun RT-real \"no battery.console.log / tQ-logs/SUMMARY.txt under \$Q_RUN_CTL\"\nfi\n",
lambda m: """FL_RUN_CTL=${FL_RUN_CTL:-/h/go2cs-tmp-coord/coord-scratch/tFL/run2}
if [ -f "$FL_RUN_CTL/battery.console.log" ] && [ -f "$FL_RUN_CTL/tFL-logs/SUMMARY.txt" ] && [ -f "$FL_RUN_CTL/mod-logs/OWED-rereads.txt" ]; then
  # T3: TRAIN FL's battery of record (run2, EXIT 0 at 9b7dfdb2ec) and the route it landed by (fixup 9b7dfdb2ec, refresh
  # 0574b8336c, fixup-2 56f0f1f254: Q's shape). FL's run2 keeps its SUMMARY under tFL-logs: read through a planted
  # folder that holds COPIES of the real files.
  mkdir -p "$OUT/rt-FLreal/tT3-logs" "$OUT/rt-FLreal/mod-logs" && cp "$FL_RUN_CTL/battery.console.log" "$OUT/rt-FLreal/battery.console.log" && cp "$FL_RUN_CTL/tFL-logs/SUMMARY.txt" "$OUT/rt-FLreal/tT3-logs/SUMMARY.txt" && cp "$FL_RUN_CTL/mod-logs/OWED-rereads.txt" "$OUT/rt-FLreal/mod-logs/OWED-rereads.txt"
  pr=$( . "$OUT/runtie-tT3-land-prep.sh.frag"; rt=$(runtie "$OUT/rt-FLreal"); g=$(rungate prep "$rt" 9b7dfdb2ec); printf '%s' "$rt :: rc=$? ${g%%:*}"; g=$(rungate prep "$rt" 0574b8336c); printf '%s' " :: another head rc=$? ${g%%:*}" )
  fr=$( . "$OUT/runtie-tT3-land-final-reads.sh.frag"; rt=$(runtie "$OUT/rt-FLreal"); ow=$(owedcount "$OUT/rt-FLreal/mod-logs/OWED-rereads.txt")
        g=$(rungate final "$rt" 56f0f1f254 0574b8336c "$ow" yes); printf '%s' "fixup-2 head, no HEAD_OK rc=$? ${g%%:*}"
        g=$(BATTERY_HEAD_OK=9b7dfdb2ec rungate final "$rt" 56f0f1f254 0574b8336c "$ow" yes); printf '%s' " :: with BATTERY_HEAD_OK=9b7dfdb2ec rc=$? ${g%%:*}"
        g=$(rungate final "$rt" 0574b8336c 9b7dfdb2ec "$ow" yes); printf '%s' " :: the refresh head (parent tie) rc=$? ${g%%:*}" )
  [ "$pr" = "${RT_FL_PREP_EXPECT:-rc=0 pre=9b7dfdb2ec end=9b7dfdb2ec exit=0 owed=0 :: rc=0 OK :: another head rc=2 REFUSED}" ] \\
    && [ "$fr" = 'fixup-2 head, no HEAD_OK rc=2 REFUSED :: with BATTERY_HEAD_OK=9b7dfdb2ec rc=0 OK :: the refresh head (parent tie) rc=0 OK' ]
  verdict RT-real "$?" "TRAIN FL's REAL run2 (EXIT 0 at 9b7dfdb2ec) and FL's landing route: prep [$pr] (EXPECT OK at the battery's head, REFUSED at another) :: final [$fr] (EXPECT the fixup-2 head REFUSED without BATTERY_HEAD_OK and OK with it; the refresh head OK by the parent tie)"
else
  notrun RT-real "no battery.console.log / tFL-logs/SUMMARY.txt / mod-logs/OWED-rereads.txt under $FL_RUN_CTL"
fi
""")
# --- the FL block -> FL-OFF
e.resub(r"# -{20,} FL \(FL1\)\n.*?\n(# -{20,} CSP \(FL5\)\n)",
lambda m: r"""# ------------------------------------------------------------------------------------------------ FL-OFF (T3-4)
# The face lift's classes are OFF at T3 (tT3-helpers.py FL_CLASSES = False). FL's plants are the control of the switch:
# every POSITIVE (a hunk FL read FACELIFT) must now read OTHER, every negative stays OTHER, no line reads FACELIFT, and
# the hunkclass CLI over the positives exits 1 with facelift-hunks=0.
FLO=$OUT/fl; mkdir -p "$FLO"
hp flcontrol "$(w "$FLO")" > "$FLO/flcontrol.txt" 2>&1; fcrc=$?
hp hunkclass "$(w "$FLO/fl-pos.patch")" > "$FLO/pos.class.txt" 2>&1; fprc=$?
fpos=$(tr -d '\r' < "$FLO/flcontrol.txt" | grep -c '(EXPECT FACELIFT'); fposoth=$(tr -d '\r' < "$FLO/flcontrol.txt" | grep '(EXPECT FACELIFT' | grep -c ' class=OTHER ')
fneg=$(tr -d '\r' < "$FLO/flcontrol.txt" | grep -c '(EXPECT OTHER) ok$'); fnegall=$(tr -d '\r' < "$FLO/flcontrol.txt" | grep -c '(EXPECT OTHER)')
ffl=$(tr -d '\r' < "$FLO/flcontrol.txt" | grep -c ' class=FACELIFT '); fhv=$(tr -d '\r' < "$FLO/pos.class.txt" | grep -a '^HUNKCLASS-VERDICT' | sed -n 's/.* facelift-hunks=\([0-9]*\) .*/\1/p')
[ "$fcrc" = 1 ] && [ "$fpos" -ge 20 ] && [ "$fposoth" = "$fpos" ] && [ "$fneg" = "$fnegall" ] && [ "$fnegall" -ge 8 ] && [ "$ffl" = 0 ] && [ "$fprc" = 1 ] && [ "${fhv:-x}" = 0 ]
verdict FL-OFF "$?" "flcontrol rc=$fcrc (EXPECT 1): positives read OTHER $fposoth of $fpos (EXPECT all), negatives OTHER $fneg of $fnegall, FACELIFT lines $ffl (EXPECT 0); hunkclass over the positives rc=$fprc (EXPECT 1) facelift-hunks=${fhv:-unread} (EXPECT 0)"

""" + m.group(1))
# --- FX9: FL's real chain
e.rep("""# the REAL route Q landed by: fixup, refresh, fixup-2 (subjects read from the objects, the train's name swapped)
if git -C "$REPO" cat-file -e de97fb2d6a^{commit} 2>/dev/null; then
  qs=(); for c in 13c0800c21 34d287aa0f de97fb2d6a; do qs+=("$(git -C "$REPO" log -1 --format=%s "$c" | sed 's/TRAIN Q/TRAIN T3/')"); done
  qr=$( . "$OUT/fixchain-tT3-fixup.sh.frag"; fixchain "${qs[@]}" )
  [ "$qr" = 'OK fixups=2 refresh=1' ] || fxbad=1
  fxr="$fxr[Q's real chain 13c0800c21, 34d287aa0f, de97fb2d6a: $qr (EXPECT OK fixups=2 refresh=1)]"
fi
verdict FX9 "$fxbad" "fixchain extracted from the fixup and the battery, 11 arms each, and Q's real landing chain: $fxr"
""", """# the REAL route FL landed by (T3): fixup, refresh, fixup-2 (subjects read from the objects, the train's name swapped)
if git -C "$REPO" cat-file -e 56f0f1f254^{commit} 2>/dev/null; then
  qs=(); for c in 9b7dfdb2ec 0574b8336c 56f0f1f254; do qs+=("$(git -C "$REPO" log -1 --format=%s "$c" | sed 's/TRAIN FL/TRAIN T3/')"); done
  qr=$( . "$OUT/fixchain-tT3-fixup.sh.frag"; fixchain "${qs[@]}" )
  [ "$qr" = 'OK fixups=2 refresh=1' ] || fxbad=1
  fxr="$fxr[FL's real chain 9b7dfdb2ec, 0574b8336c, 56f0f1f254: $qr (EXPECT OK fixups=2 refresh=1)]"
else
  fxbad=1; fxr="$fxr[FL's real chain: 56f0f1f254 is not in $REPO's object store] "
fi
verdict FX9 "$fxbad" "fixchain extracted from the fixup and the battery, 11 arms each, and FL's real landing chain: $fxr"
""")
# --- T3's own arms, before RES
e.before("# ------------------------------------------------------------------------------------------------ RES\n", r"""# ------------------------------------------------------------------------------------------------ TABLE (T3-2)
# tT3-assemble.sh TABLE_ONLY=1 on the REAL list and on three planted lists (each a copy of the real list with ONE change),
# from a per-run copy under OUT (the assembler refuses hnd and the union worktree). Nothing is created or merged.
TB=$OUT/table; mkdir -p "$TB" && cp "$SD/tT3-assemble.sh" "$TB/" || exit 2
TBASE=${TABLE_BASE:-$(git -C "$REPO" ls-remote origin refs/heads/master | cut -f1)}
tbrun(){ ( cd "$TB" && SEATS="$1" BASE="$TBASE" TABLE_ONLY=1 SIGN_PROBE=0 bash ./tT3-assemble.sh ) > "$2" 2>&1; echo $?; }
S0=$SD/tT3-seats-draft.txt
sed 's/contains 64804ed9b5, the golib half/contains 5bd97d2765, the golib half/' "$S0" > "$TB/plant-contains.txt"
sed 's/in-head 07411b26f7/in-head 247a73f54c/' "$S0" > "$TB/plant-inhead.txt"
awk '/^g-trim-3b-records-zero\|/ { held = $0; next } { print } /^g-trim-3c1-held-value\|/ { print held }' "$S0" > "$TB/plant-swap.txt"
tbm=0; for p in plant-contains plant-inhead plant-swap; do cmp -s "$S0" "$TB/$p.txt" && tbm=1; done
t0=$(tbrun "$S0" "$TB/real.log"); t1=$(tbrun "$TB/plant-contains.txt" "$TB/contains.log"); t2=$(tbrun "$TB/plant-inhead.txt" "$TB/inhead.log"); t3=$(tbrun "$TB/plant-swap.txt" "$TB/swap.log")
r0=$(grep -c '^TABLE OK: 24 rows' "$TB/real.log"); r0c=$(grep -cE "^TABLE row (15|19|20) [^:]*: contains [0-9a-f]{10} \(declared; ancestor verified\)" "$TB/real.log"); r0i=$(grep -c '^TABLE row 20 g-trim-3c1-held-value: in-head 07411b26f7 (declared; carried by the earlier row(s) g-trim-3b-records-zero' "$TB/real.log")
r1=$(grep -c "^TABLE row 19 g-trim-3b-records-zero: 'contains 5bd97d2765' but it is NOT an ancestor" "$TB/contains.log")
r2=$(grep -c "^TABLE row 20 g-trim-3c1-held-value: 'in-head 247a73f54c' but NO earlier row carries it" "$TB/inhead.log")
r3=$(grep -cE "^TABLE row 19 g-trim-3c1-held-value: 'after g-trim-3b-records-zero' names a LATER row|^TABLE row 19 g-trim-3c1-held-value: 'in-head 07411b26f7' but NO earlier row carries it" "$TB/swap.log")
[ "$tbm" = 0 ] && [ "$t0" = 0 ] && [ "$r0" = 1 ] && [ "$r0c" = 3 ] && [ "$r0i" = 1 ] && [ "$t1" = 2 ] && [ "$r1" = 1 ] && [ "$t2" = 2 ] && [ "$r2" = 1 ] && [ "$t3" = 2 ] && [ "$r3" = 2 ]
verdict TABLE "$?" "base ${TBASE:0:10}: real list rc=$t0 'TABLE OK: 24 rows'=$r0 contains-verified=$r0c (EXPECT 3: rows 15, 19, 20) in-head-verified=$r0i (EXPECT 1) :: planted 'contains' no ancestor rc=$t1 named=$r1 :: planted 'in-head' no earlier carrier rc=$t2 named=$r2 :: rows 19/20 swapped rc=$t3 named=$r3 (EXPECT 2: the 'after' and the 'in-head' lines) :: each plant differs from the list: $([ "$tbm" = 0 ] && echo yes || echo NO) (logs: $TB)"

# ------------------------------------------------------------------------------------------------ I9 (T3-3)
frag tT3-assemble.sh i9tree > "$OUT/i9tree.frag"
if ! grep -q '^i9tree(){' "$OUT/i9tree.frag"; then
  verdict I9 1 "no i9tree between the markers '# >>> i9tree' and '# <<< i9tree' of tT3-assemble.sh"
elif git -C "$REPO" cat-file -e 71e5f69dda^{commit} 2>/dev/null; then
  i9r=$( cd "$REPO" && . "$OUT/i9tree.frag"
    a(){ local want=$1 wrc=$2; shift 2; local o rc; o=$(i9tree "$@"); rc=$?; case "$o" in "$want"*) [ "$rc" = "$wrc" ] && printf 'ok ' || printf 'WRONG(rc=%s:%s) ' "$rc" "$(printf '%s' "$o" | head -n 1 | cut -c1-60)" ;; *) printf 'WRONG(%s) ' "$(printf '%s' "$o" | head -n 1 | cut -c1-80)" ;; esac; }
    a 'I9 TREE: EQUAL' 0 71e5f69dda 71e5f69dda 466166dd21
    a 'I9 TREE: EQUAL' 0 71e5f69dda 71e5f69dda
    a 'I9 TREE: DIFFERENT' 1 4e6322d770 71e5f69dda 466166dd21
    a 'I9 TREE: REFUSED' 1 71e5f69dda 71e5f69dda 0123456789
    a 'I9 TREE: NOT COMPARED' 2 71e5f69dda ''
    a 'I9 TREE: NOT COMPARED' 2 71e5f69dda 0123456789abcdef0123456789abcdef01234567 )
  i9p=$( cd "$REPO" && . "$OUT/i9tree.frag"; i9tree 4e6322d770 71e5f69dda | grep -c '^  .*|' )
  case "$i9r" in *WRONG*) false ;; *) [ "$(printf '%s' "$i9r" | tr ' ' '\n' | grep -c '^ok$')" = 6 ] && [ "${i9p:-0}" -ge 100 ] ;; esac
  verdict I9 "$?" "i9tree extracted, 6 arms: $i9r(EXPECT 6 ok: the i9 union EQUAL to itself with and without the posted tree; the base DIFFERENT; a wrong posted tree REFUSED; no ref and an unknown sha NOT COMPARED rc 2) :: the DIFFERENT arm lists $i9p path line(s) (EXPECT >= 100: the base to the union is 492 files)"
else
  notrun I9 "71e5f69dda (claude/i9-union-t3-ref) is not in $REPO's object store: git fetch origin claude/i9-union-t3-ref"
fi

# ------------------------------------------------------------------------------------------------ CN (T3-9)
frag tT3-battery.sh cnralias > "$OUT/cnralias.frag"
if ! grep -q '^cnralias(){' "$OUT/cnralias.frag"; then
  verdict CN 1 "no cnralias between the markers of tT3-battery.sh"
elif git -C "$REPO" cat-file -e 34ced590c3^{commit} 2>/dev/null; then
  CNB=$(git -C "$REPO" merge-base 4e6322d770 34ced590c3)
  c1=$( . "$OUT/cnralias.frag"; git -C "$REPO" diff -U0 "$CNB" 34ced590c3 -- src/tests/Behavioral/check-no-regression.ps1 | cnralias )
  c2=$( . "$OUT/cnralias.frag"; printf '%s\n' '--- a/x.ps1' '+++ b/x.ps1' '@@ -1,0 +2,3 @@' "+    # a comment" "+    'Pkg'   # note" '+if ($upToDate) { continue }' | cnralias )
  c3=$( . "$OUT/cnralias.frag"; printf '%s\n' '--- a/x.ps1' '+++ b/x.ps1' '@@ -5,1 +5,0 @@' "-    'EnvironBlockWalk'" | cnralias )
  c4=$( . "$OUT/cnralias.frag"; printf '%s\n' '--- a/x.ps1' '+++ b/x.ps1' '@@ -1,0 +2,1 @@' "+    'B'" "+    'A' # x" | cnralias )
  [ "$c1" = 'names=OsGetpagesize SyscallKeystonePulls other=0 removed=0' ] && [ "$c2" = 'names=Pkg other=1 removed=0' ] && [ "$c3" = 'names= other=0 removed=1' ] && [ "$c4" = 'names=A B other=0 removed=0' ]
  verdict CN "$?" "cnralias extracted: row 5's REAL diff [$c1] (EXPECT the two ruled names, other 0, removed 0); a planted code line [$c2] (EXPECT other=1); a removed entry [$c3] (EXPECT removed=1); two names [$c4] (EXPECT sorted A B)"
else
  notrun CN "34ced590c3 (row 5) is not in $REPO's object store"
fi

# ------------------------------------------------------------------------------------------------ TP (T3-7a)
frag tT3-battery.sh tpread > "$OUT/tpread.frag"
if ! grep -q '^tpread(){' "$OUT/tpread.frag"; then
  verdict TP 1 "no tpread between the markers of tT3-battery.sh"
else
  TPD=$OUT/tp; mkdir -p "$TPD"
  ok5=$(printf 'consumer\tdefault\t0\t0\tRUNS-CLEAN\t100\ngenprobe\tdefault\t0\t0\tGO-EQUAL\t100\nc32a\tdefault\t0\t0\tGO-EQUAL\t100\nc32b\tdefault\t0\t0\tGO-EQUAL\t100\nc32c\tdefault\t0\t0\tGO-EQUAL\t100')
  printf '%s\n' "$ok5" > "$TPD/ok.log"
  printf '%s\n' "$ok5" | sed 's/^c32b\tdefault\t0\t0\tGO-EQUAL/c32b\tdefault\t0\t1\tDIFFERS(InvalidCastException)/' > "$TPD/differs.log"
  printf '%s\n' "$ok5" | grep -v '^c32c' > "$TPD/four.log"
  printf '%s\n' "$ok5" | sed 's/^consumer\tdefault\t0\t0\tRUNS-CLEAN/consumer\tdefault\t0\t0\tGO-EQUAL/' > "$TPD/consumer.log"
  printf '%s\n' "$ok5" | sed 's/$/\r/' > "$TPD/crlf.log"
  printf '%s\n' "$ok5" | sed 's/^genprobe\tdefault\t0/genprobe\tdefault\t1/' > "$TPD/publish.log"
  tpr=$( . "$OUT/tpread.frag"; for x in ok differs four consumer crlf publish; do o=$(tpread "$TPD/$x.log"); printf '%s:%s:%s ' "$x" "$?" "$(printf '%s' "$o" | sed -n 's/^TP rows=\([0-9]*\) ok=\([0-9]*\).*/\1\/\2/p')"; done )
  [ "$tpr" = 'ok:0:5/5 differs:1:5/4 four:1:4/4 consumer:1:5/4 crlf:0:5/5 publish:1:5/4 ' ]
  verdict TP "$?" "tpread extracted, 6 planted logs 'name:rc:rows/ok': $tpr(EXPECT ok 0, a DIFFERS 1, four rows 1, the consumer read GO-EQUAL 1, CRLF rows 0, a publish rc 1 1)"
fi

# ------------------------------------------------------------------------------------------------ GN (T3-7b)
GNT=$(sed -n "s/^GN_TRIM='\(.*\)'$/\1/p" "$SD/tT3-battery.sh")
if [ -z "$GNT" ]; then
  verdict GN 1 "no GN_TRIM line in tT3-battery.sh"
elif git -C "$REPO" cat-file -e 71e5f69dda^{commit} 2>/dev/null; then
  gnd=0; gnm=''; for gm in $GNT; do git -C "$REPO" grep -qE "public void ${gm#*.}\(" 71e5f69dda -- "src/tests/GenTests/${gm%%.*}.cs" && gnd=$((gnd + 1)) || gnm="$gnm$gm "; done
  gnb=0; for gm in $GNT; do git -C "$REPO" grep -qE "public void ${gm#*.}\(" 4e6322d770 -- "src/tests/GenTests/${gm%%.*}.cs" 2>/dev/null && gnb=$((gnb + 1)); done
  GX=$OUT/gn; mkdir -p "$GX"
  cat > "$GX/plant.trx" <<'TRX'
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testId="1" testName="APass" outcome="Passed" />
    <UnitTestResult testId="2" testName="AFail" outcome="Failed" />
  </Results>
  <TestDefinitions>
    <UnitTest id="1" name="APass"><TestMethod className="GenTests.PlantTests, GenTests" name="APass" /></UnitTest>
    <UnitTest id="2" name="AFail"><TestMethod className="GenTests.PlantTests, GenTests" name="AFail" /></UnitTest>
  </TestDefinitions>
</TestRun>
TRX
  hp trx "$(w "$GX/plant.trx")" PlantTests.APass PlantTests.AFail PlantTests.AMissing > "$GX/plant.txt" 2>&1
  gno=$(tr -d '\r' < "$GX/plant.txt" | grep -a '^TRX test ' | grep -vE ': Passed(,Passed)*$' | sed 's/^TRX test //; s/:.*//' | tr '\n' ' ')
  [ "$gnd" = "$(echo $GNT | wc -w)" ] && [ "$gnd" = 15 ] && [ "$gnb" = 0 ] && [ "$gno" = 'PlantTests.AFail PlantTests.AMissing ' ]
  verdict GN "$?" "GN_TRIM: $gnd of $(echo $GNT | wc -w) declared at the i9 union's tree (EXPECT 15 of 15; missing [${gnm% }]), $gnb at the base 4e6322d770 (EXPECT 0: each is a guard the trim rows ADD) :: the battery's not-Passed filter over hp trx on a planted TRX names [$gno] (EXPECT PlantTests.AFail PlantTests.AMissing: a Failed and a NOT FOUND)"
else
  notrun GN "71e5f69dda is not in $REPO's object store"
fi

""")
e.done()
print('EDITS2 applied')
