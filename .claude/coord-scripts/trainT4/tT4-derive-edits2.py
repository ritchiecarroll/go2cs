# TRAIN T4 -- the anchored edits of the BATTERY, the CONTROLS, the selfcheck, the i9 TE header and the lane briefs,
# applied after tT4-derive-edits.py. Same rules: every anchor asserted, each file written only after all its edits.
#   python -B tT4-derive-edits2.py <this folder>
import os, re, sys
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
SHEBANG = '#!/usr/bin/env bash\n'
GN_T3 = ('EscapeRegistryGeneratorTests.EveryEscapingClosedContainerIsRegistered GoTypeOpsSourceTests.AStructNamesItsOwnOps '
         'GoTypeOpsSourceTests.AGenericStructNamesItsOwnClosedOps GoTypeOpsSourceTests.ANeedyStructCarriesTheFaceBesideItsZeroHook '
         'GoTypeOpsSourceTests.ANamedStructAndSliceTypeNameTheirOwnOps GoTypeOpsSourceTests.ANeverPointedToStructCarriesNoFace '
         'GoTypeOpsSourceTests.ANamedPointerTypeCarriesNoFace GoTypeOpsSourceTests.EveryFaceMemberNamesOnlyItself '
         'GoTypeOpsSourceTests.TheHazardGuardNamesASelfNestingMember GoZeroConstructionHookTests.ANeedyStructNamesItsOwnZero '
         'GoZeroConstructionHookTests.AGenericNeedyStructNamesItsOwnClosedZero GoZeroConstructionHookTests.AStructWhoseOnlyBuiltFieldIsBlankStillNamesItsZero '
         'GoZeroConstructionHookTests.ADefinedTypeOverAStructNamesItsOwnZero GoZeroConstructionHookTests.AStructWhoseConstructorBuildsNothingHasNoHook '
         'MemberRecordGeneratorTests.EveryParamDimsRecordedMethodIsKeptForTrimming')
GN_T4 = ('EscapeRegistryGeneratorTests.AContainerTheCompilationCannotNameIsNotRegistered EscapeRegistryGeneratorTests.ANeutralPackageDoesNotWalkAPerOSAssemblysFields '
         'EscapeRegistryGeneratorTests.APerOSPackageIsMarkedAndWalksFreely EscapeRegistryGeneratorTests.APerOSPackageRegistersWhatItsExportedStructsReach '
         'FieldBoxTableTests.EveryStructWithFieldsCarriesTheTable MethodSetKeepTests.EachGoTypesToStringKeepsItsGoMethods')

# ================================================================================================================ battery
e = Ed('tT4-battery.sh')
e.after(SHEBANG, """# TRAIN T4 battery on the i7 (2026-10-12; derived from TRAIN T3's run2 kit, which ran EXIT 6 at 38f1ce2510 with its
# reds ruled by name, fixed by fixup-2 9adab1a13d and landed at 4c9c68a32f; every change in tT4-CHANGES.md). Against T3:
#   T4-1  the previous train's record is TRAIN T3's run2 (T3_RUN; N_RUN, O_RUN, P_RUN, Q_RUN and FL_RUN are refused).
#         Q_SUMS / Q_REWRITES / Q_REWRITES_T keep their names and point INTO T3's run2; fallbacks 871 (CNR) and 1291 (FX).
#   T4-8  LEG TP reads G's probe ref claude/g-trim-probes-more at TP_PIN=5f363aeae4 (04239ec85e plus the registry,
#         stringer and fieldbox probes): 8 programs, consumer RUNS-CLEAN, seven GO-EQUAL, each name once.
#   T4-9  LEG GN: GN_TRIM = T3's 15 trim guards + the 6 escape-registry / field-box / method-set guards row 3 carries
#         (5 new and AContainerTheCompilationCannotNameIsNotRegistered, which row 3's ruled 'theirs' file must keep);
#         the floor is the union's 195 method lines, Total >= 214 (the i9 read 214).
#   T4-10 PRE-D CNR-FRESH: no T4 row is ruled to change check-no-regression.ps1 (CNR_ALIAS_RULED is EMPTY), so ANY change
#         to the CNR script or _paths.ps1 is the finding it always was; T3's cnralias stays, inert, under its control.
# Predictions at the 11-row union (derived from the objects at the assembled head; restated in COORD-LAUNCH-CHECKLIST.md):
# CNR N = 873 (880 enumerated, 7 platform skips; master 871 + RawStringLineEndings and X509SystemVerify); GN Total 214
# (floor 195); E union-attributable 0 x3. T3's header follows.
""")
e.rep("""[ -z "${N_RUN:-}${O_RUN:-}${P_RUN:-}${Q_RUN:-}" ] || { echo "ABORT: N_RUN, O_RUN, P_RUN or Q_RUN is set (an earlier train's variable). TRAIN T4 reads the previous train's record from FL_RUN (default coord-scratch/tFL/run2): unset it and set FL_RUN"; exit 2; }
FL_RUN=${FL_RUN:-/h/go2cs-tmp-coord/coord-scratch/tFL/run2}   # T3 (T3-6): TRAIN FL's battery of record (EXIT 0 at 9b7dfdb2ec)
Q_SUMS="$FL_RUN/tFL-logs/SUMMARY.txt"                 # TL-WALL, FX floor, CNR control, the S rows' HOSTWALL baseline: TRAIN FL's battery (the name is Q's, kept so every reader below is unchanged)
Q_REWRITES="$FL_RUN/tFL-logs/S-rewrites-U0.patch"     # TE baseline: FL's sweep rewrites, same box
Q_REWRITES_T="$FL_RUN/tFL-logs/T-rewrites-U0.patch"   # TE-T baseline: FL's T-leg rewrites
""", """[ -z "${N_RUN:-}${O_RUN:-}${P_RUN:-}${Q_RUN:-}${FL_RUN:-}" ] || { echo "ABORT: N_RUN, O_RUN, P_RUN, Q_RUN or FL_RUN is set (an earlier train's variable). TRAIN T4 reads the previous train's record from T3_RUN (default coord-scratch/tT3/run2): unset it and set T3_RUN"; exit 2; }
T3_RUN=${T3_RUN:-/h/go2cs-tmp-coord/coord-scratch/tT3/run2}   # T4 (T4-1): TRAIN T3's battery of record (EXIT 6 at 38f1ce2510, its reds ruled by name; landed 4c9c68a32f)
Q_SUMS="$T3_RUN/tT3-logs/SUMMARY.txt"                 # TL-WALL, FX floor, CNR control, the S rows' HOSTWALL baseline: TRAIN T3's battery (the name is Q's, kept so every reader below is unchanged)
Q_REWRITES="$T3_RUN/tT3-logs/S-rewrites-U0.patch"     # TE baseline: T3's sweep rewrites, same box
Q_REWRITES_T="$T3_RUN/tT3-logs/T-rewrites-U0.patch"   # TE-T baseline: T3's T-leg rewrites
""")
e.rep("# optional: the i9's TRAIN FL shard patch (coord-scratch/tFL/i9-patches/tFL-tracked-changes-U0.patch), as the TE-i9 baseline",
      "# optional: the i9's TRAIN T3 shard patch (coord-scratch/tT3/i9-patches/tT3-tracked-changes-U0.patch, when it is on this box), as the TE-i9 baseline")
e.resub(r"GN_TRIM='[^'\n]*'\nGN_RESULTS_MIN=208[^\n]*\n", lambda m: f"""# T4 (T4-9): + the 6 guards row 3 (g-promoted-field-identity) carries in GenTests: 5 it ADDS (read from the three-dot
# diff 226afb810a...0726750708) and AContainerTheCompilationCannotNameIsNotRegistered, which master holds since T3's
# fixup-2 and row 3's RULED 'theirs' EscapeRegistryGeneratorTests.cs must still hold. GN_TRIM_T4 is the T4 part (the
# controls' GN arm reads both lines).
GN_TRIM='{GN_T3} {GN_T4}'
GN_TRIM_T4='{GN_T4}'
GN_RESULTS_MIN=214                       # T4: the results the union's GenTests produce (195 method lines, DataRows expand; the i9 read 214)
""")
e.rep("""# T3 (T3-7a): the trim probe runner. TP_WT is a detached worktree of claude/g-trim-probes made BEFORE launch (the
# checklist); the battery never fetches. CNR_ALIAS_RULED: row 5's ruled additions to the CNR alias-drift set (T3-9).
TP_REF=${TP_REF:-claude/g-trim-probes}
""", """# T3 (T3-7a): the trim probe runner. TP_WT is a detached worktree of the probe ref made BEFORE launch (the
# checklist); the battery never fetches. T4 (T4-8): the ref is claude/g-trim-probes-more (G: three probes over 04239ec85e).
# T4 (T4-10): CNR_ALIAS_RULED is EMPTY: no T4 row is ruled to change the CNR script.
TP_REF=${TP_REF:-claude/g-trim-probes-more}
""")
e.rep("TP_PIN=${TP_PIN:-04239ec85e}   # COORD 2026-10-10: the probe ref's tip carrying G's CRLF fix (a fast-forward of 2de88c20fa)\nCNR_ALIAS_RULED='OsGetpagesize SyscallKeystonePulls'\n",
      "TP_PIN=${TP_PIN:-5f363aeae4}   # COORD 2026-10-12: claude/g-trim-probes-more, 04239ec85e + registry, stringer, fieldbox (a fast-forward)\nCNR_ALIAS_RULED=''\n")
e.rep("previous-train record (TRAIN FL) FL_RUN=$FL_RUN (", "previous-train record (TRAIN T3) T3_RUN=$T3_RUN (")
e.rep("""[ -n "$CNR_MPREV" ] || { CNR_MPREV=866; CNR_MSRC="the literal 866, TRAIN FL's battery CNR line ('4 asserts: N=866', run2; its SUMMARY is not readable at $Q_SUMS)"; }""",
      """[ -n "$CNR_MPREV" ] || { CNR_MPREV=871; CNR_MSRC="the literal 871, TRAIN T3's battery CNR line ('4 asserts: N=871', run2; its SUMMARY is not readable at $Q_SUMS)"; }""")
e.rep("""[ -n "$FX_FLOOR" ] || { FX_FLOOR=1291; FX_SRC="the literal 1291, TRAIN FL's (and Q's, P's, O's, N's and M's) reading (FL's SUMMARY is not readable at $Q_SUMS)"; }""",
      """[ -n "$FX_FLOOR" ] || { FX_FLOOR=1291; FX_SRC="the literal 1291, TRAIN T3's (and FL's, Q's, P's, O's, N's and M's) reading (T3's SUMMARY is not readable at $Q_SUMS)"; }""")
e.rep("""if [ -n "$CNRDIFF" ]; then
  CNRA=$(git diff -U0 "$MASTER" HEAD -- src/tests/Behavioral/check-no-regression.ps1 | cnralias)
""", """# T4 (T4-10): with CNR_ALIAS_RULED empty there is no ruled change to admit: ANY change is the finding it always was.
if [ -n "$CNRDIFF" ] && [ -z "$CNR_ALIAS_RULED" ]; then
  dfind "CNR_FRESH|the union changes [${CNRDIFF% }] and no T4 row is ruled to change the CNR script or _paths.ps1 (CNR_ALIAS_RULED is empty): READ the change for an up-to-date skip before leg 4 is trusted (floor 10: check-no-regression re-transpiles unconditionally, and that asymmetry is why it is immune to stale output)"
elif [ -n "$CNRDIFF" ]; then
  CNRA=$(git diff -U0 "$MASTER" HEAD -- src/tests/Behavioral/check-no-regression.ps1 | cnralias)
""")
e.rep("""  totalgate GN "$LOGDIR/gentests.log" "$(tmfloor src/tests/GenTests)" 189 "the [TestMethod] lines""",
      """  totalgate GN "$LOGDIR/gentests.log" "$(tmfloor src/tests/GenTests)" 195 "the [TestMethod] lines""")
e.rep("""tpread(){ # the runner's console log -> 'TP rows=<n> ok=<k> off=[...]'; rc 0 only for 5 default rows, each at its expectation
  tr -d '\\r' < "$1" | awk -F'\\t' 'NF == 6 && $2 == "default" { n++; want = ($1 == "consumer") ? "RUNS-CLEAN" : "GO-EQUAL"; if ($3 == "0" && $5 == want) ok++; else off = off $1 ":" $3 "/" $4 "/" $5 " " } END { printf "TP rows=%d ok=%d off=[%s]\\n", n, ok, off; exit !(n == 5 && ok == 5) }'
}
""", """tpread(){ # the runner's console log -> 'TP rows=<n> ok=<k> off=[...]'; rc 0 only for the 8 default rows of TP_PIN's runner (T4-8: registry, stringer, fieldbox join T3's five), each at its expectation, each name exactly once
  tr -d '\\r' < "$1" | awk -F'\\t' 'BEGIN { split("consumer genprobe c32a c32b c32c registry stringer fieldbox", e, " "); for (i in e) want[e[i]] = 1 } NF == 6 && $2 == "default" { n++; if (($1 in want) && !($1 in seen)) { seen[$1] = 1; k++ } w = ($1 == "consumer") ? "RUNS-CLEAN" : "GO-EQUAL"; if ($3 == "0" && $5 == w) ok++; else off = off $1 ":" $3 "/" $4 "/" $5 " " } END { printf "TP rows=%d ok=%d names=%d off=[%s]\\n", n, ok, k, off; exit !(n == 8 && ok == 8 && k == 8) }'
}
""")
e.rep(":: $tpv (EXPECT rows=5 ok=5; runner rc $LEG_RC)", ":: $tpv (EXPECT rows=8 ok=8 names=8; runner rc $LEG_RC)")
e.done()

# ================================================================================================================ controls
e = Ed('tT4-controls.sh')
e.after(SHEBANG, """# TRAIN T4 -- the CONTROLS (2026-10-12; derived from TRAIN T3's run2 kit, tT4-CHANGES.md). Nothing here builds,
# converts or runs a test. Against T3:
#   RT-real  TRAIN T3's REAL run2 (EXIT 6 at 38f1ce2510, its reds ruled: BATTERY_RC_OK=6) and T3's landing route
#            (fixup 38f1ce2510, fixup-2 9adab1a13d, the MS11 merge 4c9c68a32f), in place of FL's
#   FX9      the real chain read is T3's (fixup, fixup-2: 'OK fixups=2 refresh=0')
#   TABLE    tT4-assemble.sh TABLE_ONLY on the REAL 11-row list with the ruled TIP_MOVED_OK default (TABLE OK, row 5's
#            tip acknowledged and its commit listed), with TIP_MOVED_OK='' (REFUSED on row 5), and on two planted lists:
#            row 3 seated at its parent 864bf27871 (REFUSED: the admission is row 5's alone) and two 'contains' notes
#            (T3-2: one an ancestor, verified; one not, REFUSED)
#   I9       i9tree (T4-4, the TREE form) on planted heads: equal by prefix and in full, a different tree in the store
#            (paths listed) and one not in it, no tree, a short prefix, an unknown head
#   TP       tpread (T4-8) on planted 8-row logs, a duplicate name and T3's five-row output among them
#   GN       GN_TRIM (T4-9): T3's 15 declared at the base, the T4 6 at row 3's seat, the 5 new ones absent at the base
#   RES      tT4-resolve-controls.sh now carries C11, C11n, C11k (the kind 'theirs', T4-3)
# CN is T3's arm unchanged: cnralias stays in the battery, inert at T4 (T4-10), and its reader keeps its control.
# T3's header follows.
""")
e.resub(r"FL_RUN_CTL=\$\{FL_RUN_CTL:-/h/go2cs-tmp-coord/coord-scratch/tFL/run2\}\n.*?\n  notrun RT-real \"no battery.console.log / tFL-logs/SUMMARY.txt / mod-logs/OWED-rereads.txt under \$FL_RUN_CTL\"\nfi\n",
lambda m: """T3_RUN_CTL=${T3_RUN_CTL:-/h/go2cs-tmp-coord/coord-scratch/tT3/run2}
if [ -f "$T3_RUN_CTL/battery.console.log" ] && [ -f "$T3_RUN_CTL/tT3-logs/SUMMARY.txt" ] && [ -f "$T3_RUN_CTL/mod-logs/OWED-rereads.txt" ]; then
  # T4: TRAIN T3's battery of record (run2: EXIT 6 at 38f1ce2510, its reds ruled by name; the landing read BATTERY_RC_OK=6)
  # and the route it landed by (fixup 38f1ce2510, fixup-2 9adab1a13d, then master merged in by MS11 at 4c9c68a32f). T3's
  # run2 keeps its SUMMARY under tT3-logs: read through a planted folder that holds COPIES of the real files.
  mkdir -p "$OUT/rt-T3real/tT4-logs" "$OUT/rt-T3real/mod-logs" && cp "$T3_RUN_CTL/battery.console.log" "$OUT/rt-T3real/battery.console.log" && cp "$T3_RUN_CTL/tT3-logs/SUMMARY.txt" "$OUT/rt-T3real/tT4-logs/SUMMARY.txt" && cp "$T3_RUN_CTL/mod-logs/OWED-rereads.txt" "$OUT/rt-T3real/mod-logs/OWED-rereads.txt"
  pr=$( . "$OUT/runtie-tT4-land-prep.sh.frag"; rt=$(runtie "$OUT/rt-T3real"); g=$(rungate prep "$rt" 38f1ce2510); printf '%s' "$rt :: rc=$? ${g%%:*}"
        g=$(BATTERY_RC_OK=6 rungate prep "$rt" 38f1ce2510); printf '%s' " :: BATTERY_RC_OK=6 rc=$? ${g%%:*}"; g=$(BATTERY_RC_OK=6 rungate prep "$rt" 9adab1a13d); printf '%s' " :: another head rc=$? ${g%%:*}" )
  fr=$( . "$OUT/runtie-tT4-land-final-reads.sh.frag"; rt=$(runtie "$OUT/rt-T3real"); ow=$(owedcount "$OUT/rt-T3real/mod-logs/OWED-rereads.txt")
        g=$(rungate final "$rt" 9adab1a13d 38f1ce2510 "$ow" yes); printf '%s' "fixup-2 head, no RC_OK rc=$? ${g%%:*}"
        g=$(BATTERY_RC_OK=6 rungate final "$rt" 9adab1a13d 38f1ce2510 "$ow" yes); printf '%s' " :: with BATTERY_RC_OK=6 (the parent tie) rc=$? ${g%%:*}"
        g=$(BATTERY_RC_OK=6 rungate final "$rt" 4c9c68a32f 9adab1a13d "$ow" yes); printf '%s' " :: the MS11 merge head rc=$? ${g%%:*}"
        g=$(BATTERY_RC_OK=6 BATTERY_HEAD_OK=38f1ce2510 rungate final "$rt" 4c9c68a32f 9adab1a13d "$ow" yes); printf '%s' " :: with BATTERY_HEAD_OK=38f1ce2510 rc=$? ${g%%:*}" )
  [ "$pr" = "${RT_T3_PREP_EXPECT:-rc=6 pre=38f1ce2510 end=38f1ce2510 exit=6 owed=0 :: rc=2 REFUSED :: BATTERY_RC_OK=6 rc=0 OK :: another head rc=2 REFUSED}" ] \\
    && [ "$fr" = 'fixup-2 head, no RC_OK rc=2 REFUSED :: with BATTERY_RC_OK=6 (the parent tie) rc=0 OK :: the MS11 merge head rc=2 REFUSED :: with BATTERY_HEAD_OK=38f1ce2510 rc=0 OK' ]
  verdict RT-real "$?" "TRAIN T3's REAL run2 (EXIT 6 at 38f1ce2510) and T3's landing route: prep [$pr] (EXPECT the red REFUSED, OK with BATTERY_RC_OK=6, REFUSED at another head) :: final [$fr] (EXPECT the fixup-2 head REFUSED without RC_OK and OK with it by the parent tie; the MS11 merge head REFUSED until BATTERY_HEAD_OK names the battery's head)"
else
  notrun RT-real "no battery.console.log / tT3-logs/SUMMARY.txt / mod-logs/OWED-rereads.txt under $T3_RUN_CTL"
fi
""")
e.resub(r"# the REAL route FL landed by \(T3\): fixup, refresh, fixup-2[^\n]*\n.*?\nverdict FX9 \"\$fxbad\"[^\n]*\n", lambda m: """# the REAL route T3 landed by (T4): fixup, fixup-2 (subjects read from the objects, the train's name swapped)
if git -C "$REPO" cat-file -e 9adab1a13d^{commit} 2>/dev/null; then
  qs=(); for c in 38f1ce2510 9adab1a13d; do qs+=("$(git -C "$REPO" log -1 --format=%s "$c" | sed 's/TRAIN T3/TRAIN T4/')"); done
  qr=$( . "$OUT/fixchain-tT4-fixup.sh.frag"; fixchain "${qs[@]}" )
  [ "$qr" = 'OK fixups=2 refresh=0' ] || fxbad=1
  fxr="$fxr[T3's real chain 38f1ce2510, 9adab1a13d: $qr (EXPECT OK fixups=2 refresh=0)]"
else
  fxbad=1; fxr="$fxr[T3's real chain: 9adab1a13d is not in $REPO's object store] "
fi
verdict FX9 "$fxbad" "fixchain extracted from the fixup and the battery, 11 arms each, and T3's real landing chain: $fxr"
""")
e.resub(r"# -{20,} TABLE \(T3-2\)\n.*?\n(# -{20,} I9 \(T3-3\)\n)", lambda m: r"""# ------------------------------------------------------------------------------------------------ TABLE (T4-2)
# tT4-assemble.sh TABLE_ONLY=1 on the REAL list (the ruled TIP_MOVED_OK default, and TIP_MOVED_OK='' set empty) and on two
# planted lists (each a copy of the real list with ONE kind of change), from a per-run copy under OUT. Nothing is created.
TB=$OUT/table; mkdir -p "$TB" && cp "$SD/tT4-assemble.sh" "$TB/" || exit 2
TBASE=${TABLE_BASE:-$(git -C "$REPO" ls-remote origin refs/heads/master | cut -f1)}
tbrun(){ # seats, log [, TIP_MOVED_OK value; absent = the kit's default]
  if [ "$#" -ge 3 ]; then ( cd "$TB" && SEATS="$1" BASE="$TBASE" TABLE_ONLY=1 SIGN_PROBE=0 TIP_MOVED_OK="$3" bash ./tT4-assemble.sh ) > "$2" 2>&1; echo $?
  else ( cd "$TB" && env -u TIP_MOVED_OK SEATS="$1" BASE="$TBASE" TABLE_ONLY=1 SIGN_PROBE=0 bash ./tT4-assemble.sh ) > "$2" 2>&1; echo $?; fi
}
S0=$SD/tT4-seats-draft.txt
sed 's/^g-promoted-field-identity|0726750708|/g-promoted-field-identity|864bf27871|/' "$S0" > "$TB/plant-parent.txt"
sed -e '/^g-promoted-field-identity|/ s/$/; contains ec6df12120/' -e '/^g-golib-creator-o1|/ s/$/; contains 26ebdaa8e5/' "$S0" > "$TB/plant-contains.txt"
tbm=0; for p in plant-parent plant-contains; do cmp -s "$S0" "$TB/$p.txt" && tbm=1; done
t0=$(tbrun "$S0" "$TB/real.log"); t1=$(tbrun "$S0" "$TB/real-noack.log" ''); t2=$(tbrun "$TB/plant-parent.txt" "$TB/parent.log"); t3=$(tbrun "$TB/plant-contains.txt" "$TB/contains.log")
R5TIP="^TABLE row 5 g-gocmp-perf-floor: origin tip 701430cf6d != seated a5ef12237c"
r0=$(grep -c '^TABLE OK: 11 rows' "$TB/real.log"); r0t=$(grep -c "$R5TIP" "$TB/real.log"); r0a=$(grep -c '^  acknowledged (TIP_MOVED_OK' "$TB/real.log"); r0n=$(grep -c '^  NOT seated, 1 commit(s) past the seat: 701430cf6d docs: PLAN-golib-full-trim 9.13' "$TB/real.log"); r0d=$(grep -c '^TABLE: TIP_MOVED_OK=\[g-gocmp-perf-floor\]' "$TB/real.log")
r1t=$(grep -c "$R5TIP" "$TB/real-noack.log"); r1a=$(grep -c '^  acknowledged' "$TB/real-noack.log")
r2=$(grep -c '^TABLE row 3 g-promoted-field-identity: origin tip 0726750708 != seated 864bf27871' "$TB/parent.log"); r2a=$(grep -c '^  acknowledged (TIP_MOVED_OK' "$TB/parent.log")
r3v=$(grep -c '^TABLE row 3 g-promoted-field-identity: contains ec6df12120 (declared; ancestor verified)' "$TB/contains.log"); r3r=$(grep -c "^TABLE row 4 g-golib-creator-o1: 'contains 26ebdaa8e5' but it is NOT an ancestor of 1a00003c9b" "$TB/contains.log")
[ "$tbm" = 0 ] && [ "$t0" = 0 ] && [ "$r0" = 1 ] && [ "$r0t" = 1 ] && [ "$r0a" = 1 ] && [ "$r0n" = 1 ] && [ "$r0d" = 1 ] && [ "$t1" = 2 ] && [ "$r1t" = 1 ] && [ "$r1a" = 0 ] \
  && [ "$t2" = 2 ] && [ "$r2" = 1 ] && [ "$r2a" = 1 ] && [ "$t3" = 2 ] && [ "$r3v" = 1 ] && [ "$r3r" = 1 ]
verdict TABLE "$?" "base ${TBASE:0:10}: real list, the ruled default rc=$t0 'TABLE OK: 11 rows'=$r0 default-stamped=$r0d row-5 tip line=$r0t acknowledged=$r0a its commit listed=$r0n (EXPECT 0 1 1 1 1 1) :: TIP_MOVED_OK='' rc=$t1 row-5 tip line=$r1t acknowledged=$r1a (EXPECT 2 1 0) :: row 3 at its parent rc=$t2 named=$r2 acknowledged=$r2a (EXPECT 2 1 1: row 5's line alone) :: two 'contains' notes rc=$t3 verified=$r3v refused=$r3r (EXPECT 2 1 1) :: each plant differs from the list: $([ "$tbm" = 0 ] && echo yes || echo NO) (logs: $TB)"

""" + m.group(1))
e.resub(r"(# -{20,} I9 \(T3-3\)\n).*?\n(# -{20,} CN \(T3-9\)\n)", lambda m: r"""# ------------------------------------------------------------------------------------------------ I9 (T4-4)
frag tT4-assemble.sh i9tree > "$OUT/i9tree.frag"
if ! grep -q '^i9tree(){' "$OUT/i9tree.frag"; then
  verdict I9 1 "no i9tree between the markers '# >>> i9tree' and '# <<< i9tree' of tT4-assemble.sh"
elif git -C "$REPO" cat-file -e 226afb810a^{commit} 2>/dev/null && git -C "$REPO" cat-file -e 4c9c68a32f^{commit} 2>/dev/null; then
  IB=$(git -C "$REPO" rev-parse 226afb810a^{tree}); IL=$(git -C "$REPO" rev-parse 4c9c68a32f^{tree})
  i9r=$( cd "$REPO" && . "$OUT/i9tree.frag"
    a(){ local want=$1 wrc=$2; shift 2; local o rc; o=$(i9tree "$@"); rc=$?; case "$o" in "$want"*) [ "$rc" = "$wrc" ] && printf 'ok ' || printf 'WRONG(rc=%s:%s) ' "$rc" "$(printf '%s' "$o" | head -n 1 | cut -c1-60)" ;; *) printf 'WRONG(%s) ' "$(printf '%s' "$o" | head -n 1 | cut -c1-80)" ;; esac; }
    a 'I9 TREE: EQUAL' 0 226afb810a "${IB:0:10}"
    a 'I9 TREE: EQUAL' 0 226afb810a "$IB"
    a 'I9 TREE: DIFFERENT' 1 226afb810a "${IL:0:10}"
    a 'I9 TREE: DIFFERENT' 1 226afb810a 0123456789
    a 'I9 TREE: NOT COMPARED' 2 226afb810a ''
    a 'I9 TREE: NOT COMPARED' 2 226afb810a 7e1f0969
    a 'I9 TREE: NOT COMPARED' 2 0123456789abcdef0123456789abcdef01234567 "${IB:0:10}" )
  i9p=$( cd "$REPO" && . "$OUT/i9tree.frag"; i9tree 226afb810a "${IL:0:10}" | grep -c '^  .*|' )
  i9n=$( cd "$REPO" && . "$OUT/i9tree.frag"; i9tree 226afb810a 0123456789 | grep -c 'not in this object store' )
  case "$i9r" in *WRONG*) false ;; *) [ "$(printf '%s' "$i9r" | tr ' ' '\n' | grep -c '^ok$')" = 7 ] && [ "${i9p:-0}" -ge 1 ] && [ "$i9n" = 1 ] ;; esac
  verdict I9 "$?" "i9tree extracted, 7 arms: $i9r(EXPECT 7 ok: the base's own tree EQUAL by prefix and in full; T3's landing tree DIFFERENT; an unknown tree DIFFERENT; no tree, an 8-hex prefix and an unknown head NOT COMPARED rc 2) :: the DIFFERENT arm in the store lists $i9p path line(s) (EXPECT >= 1), the one not in the store says so: $i9n (EXPECT 1)"
else
  notrun I9 "226afb810a or 4c9c68a32f is not in $REPO's object store"
fi

""" + m.group(2))
e.resub(r"(# -{20,} TP \(T3-7a\)\n).*?\n(# -{20,} GN \(T3-7b\)\n)", lambda m: r"""# ------------------------------------------------------------------------------------------------ TP (T4-8)
frag tT4-battery.sh tpread > "$OUT/tpread.frag"
if ! grep -q '^tpread(){' "$OUT/tpread.frag"; then
  verdict TP 1 "no tpread between the markers of tT4-battery.sh"
else
  TPD=$OUT/tp; mkdir -p "$TPD"
  ok8=$(printf 'consumer\tdefault\t0\t0\tRUNS-CLEAN\t100\ngenprobe\tdefault\t0\t0\tGO-EQUAL\t100\nc32a\tdefault\t0\t0\tGO-EQUAL\t100\nc32b\tdefault\t0\t0\tGO-EQUAL\t100\nc32c\tdefault\t0\t0\tGO-EQUAL\t100\nregistry\tdefault\t0\t0\tGO-EQUAL\t100\nstringer\tdefault\t0\t0\tGO-EQUAL\t100\nfieldbox\tdefault\t0\t0\tGO-EQUAL\t100')
  printf '%s\n' "$ok8" > "$TPD/ok.log"
  printf '%s\n' "$ok8" | sed 's/^fieldbox\tdefault\t0\t0\tGO-EQUAL/fieldbox\tdefault\t0\t1\tDIFFERS(InvalidCastException)/' > "$TPD/differs.log"
  printf '%s\n' "$ok8" | grep -v '^fieldbox' > "$TPD/seven.log"
  printf '%s\n' "$ok8" | sed 's/^consumer\tdefault\t0\t0\tRUNS-CLEAN/consumer\tdefault\t0\t0\tGO-EQUAL/' > "$TPD/consumer.log"
  printf '%s\n' "$ok8" | sed 's/$/\r/' > "$TPD/crlf.log"
  printf '%s\n' "$ok8" | sed 's/^registry\tdefault\t0/registry\tdefault\t1/' > "$TPD/publish.log"
  printf '%s\n' "$ok8" | sed 's/^fieldbox\t/registry\t/' > "$TPD/dup.log"
  printf '%s\n' "$ok8" | head -n 5 > "$TPD/five.log"
  tpr=$( . "$OUT/tpread.frag"; for x in ok differs seven consumer crlf publish dup five; do o=$(tpread "$TPD/$x.log"); printf '%s:%s:%s ' "$x" "$?" "$(printf '%s' "$o" | sed -n 's/^TP rows=\([0-9]*\) ok=\([0-9]*\).*/\1\/\2/p')"; done )
  [ "$tpr" = 'ok:0:8/8 differs:1:8/7 seven:1:7/7 consumer:1:8/7 crlf:0:8/8 publish:1:8/7 dup:1:8/8 five:1:5/5 ' ]
  verdict TP "$?" "tpread extracted, 8 planted logs 'name:rc:rows/ok': $tpr(EXPECT ok 0, a DIFFERS 1, seven rows 1, the consumer read GO-EQUAL 1, CRLF rows 0, a publish rc 1 1, a name twice 1 at 8/8, T3's five-row output 1)"
fi

""" + m.group(2))
e.resub(r"(# -{20,} GN \(T3-7b\)\n).*?\n(# -{20,} RES\n)", lambda m: r"""# ------------------------------------------------------------------------------------------------ GN (T4-9)
GNT=$(sed -n "s/^GN_TRIM='\(.*\)'$/\1/p" "$SD/tT4-battery.sh"); GN4=$(sed -n "s/^GN_TRIM_T4='\(.*\)'$/\1/p" "$SD/tT4-battery.sh")
if [ -z "$GNT" ] || [ -z "$GN4" ]; then
  verdict GN 1 "no GN_TRIM / GN_TRIM_T4 line in tT4-battery.sh"
elif git -C "$REPO" cat-file -e 0726750708^{commit} 2>/dev/null; then
  decl(){ git -C "$REPO" grep -qE "public void ${2#*.}\(" "$1" -- "src/tests/GenTests/${2%%.*}.cs" 2>/dev/null; }
  g3=0; g3m=''; for gm in $GNT; do case " $GN4 " in *" $gm "*) continue ;; esac; decl 226afb810a "$gm" && g3=$((g3 + 1)) || g3m="$g3m$gm "; done
  g4=0; g4m=''; for gm in $GN4; do decl 0726750708 "$gm" && g4=$((g4 + 1)) || g4m="$g4m$gm "; done
  g4b=''; for gm in $GN4; do decl 226afb810a "$gm" && g4b="$g4b${gm#*.} "; done
  ginc=0; for gm in $GN4; do case " $GNT " in *" $gm "*) ginc=$((ginc + 1)) ;; esac; done
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
  [ "$(echo $GNT | wc -w)" = 21 ] && [ "$g3" = 15 ] && [ "$g4" = 6 ] && [ "$ginc" = 6 ] && [ "$g4b" = 'AContainerTheCompilationCannotNameIsNotRegistered ' ] && [ "$gno" = 'PlantTests.AFail PlantTests.AMissing ' ]
  verdict GN "$?" "GN_TRIM holds $(echo $GNT | wc -w) names (EXPECT 21): T3's $g3 declared at the base 226afb810a (EXPECT 15; missing [${g3m% }]); GN_TRIM_T4's $g4 declared at row 3's seat 0726750708 (EXPECT 6; missing [${g4m% }]), $ginc of them inside GN_TRIM (EXPECT 6); declared at the base already: [${g4b% }] (EXPECT AContainerTheCompilationCannotNameIsNotRegistered alone: the other 5 are guards row 3 ADDS) :: the battery's not-Passed filter over hp trx on a planted TRX names [$gno] (EXPECT PlantTests.AFail PlantTests.AMissing)"
else
  notrun GN "0726750708 (row 3) is not in $REPO's object store"
fi

""" + m.group(2))
e.done()

# =============================================================================================================== selfcheck
e = Ed('tT4-selfcheck.sh')
e.after(SHEBANG, "# TRAIN T4 (2026-10-12): S7 counts TRAIN T3's names (the previous train); the derive's own .py are parsed by S6.\n")
e.rep("""  c=$(grep -c -E '/tFL\\b|tFLemit|trainFL|coord-trainFL-union|TRAIN FL\\b|tFL-' "$f"); [ "$c" = 0 ] || printf '    %-34s %s\\n' "$(basename "$f")" "$c\"""",
      """  c=$(grep -c -E '/tT3\\b|tT3emit|trainT3|coord-trainT3-union|TRAIN T3\\b|tT3-' "$f"); [ "$c" = 0 ] || printf '    %-34s %s\\n' "$(basename "$f")" "$c\"""")
e.done()

# ================================================================================================================ i9-te
e = Ed('tT4-i9-te.sh')
e.after(SHEBANG, "# TRAIN T4 (2026-10-12): the baseline is the i9's OWN TRAIN T3 shard patch when it is on this box (T4-1); T3's text follows.\n")
e.done()

# ================================================================================================================ briefs
TOP = """**T4 (2026-10-12), READ THIS FIRST; it leads every paragraph below.** TRAIN T4 is 11 rows over base master
`226afb810a`: C1's darwin x509 roots companion (behavioral X509SystemVerify) and comment parity seats A to G (the corpus
and goldens re-emitted IN the seat); G's promoted-field identity / trim 3d family E (row 3, two files RULED to the seat's
side), the per-site goroutine creator (golib Goroutine.cs) and a docs row seated BY SHA (a5ef12237c); C2's nugetgo RID
packs; P2's hop readiness tools, the .Raw() raw-string fix (behavioral RawStringLineEndings, 117 _test.cs committed IN
the seat) and the retired-hand-own guard; the i9's H5c GOOS-child check. The union is pushed as
`claude/coord-trainT4-union` (COORD names the SHA in the GO: the T4 fixup, or a `fixup-N: TRAIN T4` on top). The fixup
carries ONE projitems line (retiredHandOwns_test.go) and the union regeneration of RawStringLineEndings and
X509SystemVerify. Rows 2 and 9 change -stdlib AND -tests emission but committed their footprints in-seat, so your sweeps
are PREDICTED to rewrite ~0 committed test sources beyond the standing line-ending / OTHER set; any content rewrite is
read BY NAME and posted. The previous train's record is T3's (your T3 shard patch is the TE baseline). CNR N at the
union = 873 on windows (880 enumerated, 7 platform skips; on linux read your own enumeration). The paragraphs below are
T3's and FL's, names moved: where they name T3's rows or SHAs they are history.
"""
for f in ('tT4-lane-brief-i9.md', 'tT4-lane-brief-linux.md'):
    e = Ed(f)
    e.resub(r"\*\*T3 \(2026-10-10\), READ THIS FIRST; it leads every paragraph below\.\*\*.*?\n\n", lambda m: TOP + "\n")
    e.t = e.t.replace('`38f1ce2510fc9b04f849afff027942d96ab92897`', '`<UNION: 40 hex, filled in by COORD at the push>`')
    e.t = e.t.replace('`f43e0a2f4e4a3d6b5891ad9430cb7bffe811a5e9`', '`226afb810a21408ebda2412dd74d526345c46f65`')
    assert '38f1ce2510' not in e.t and 'f43e0a2f4e' not in e.t, f
    e.done()
print('EDITS2 applied')
