#!/usr/bin/env python3
# DERIVE coord-train46-assemble.sh FROM coord-train45-assemble.sh.
# Every block replacement and every substitution ASSERTS IT CHANGED SOMETHING and PRINTS what it did:
# a sed that matches nothing and a sed that does the work exit the same way, and a derivation chain
# built on silent no-ops compounds across generations.
import sys, os, re, io

SP = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(SP, 'coord-train45-assemble.sh')
DST = os.path.join(SP, 'coord-train46-assemble.sh')

with io.open(SRC, encoding='utf-8', newline='') as f:
    lines = f.read().split(chr(10))
if lines and lines[-1] == '':
    lines.pop()
orig_n = len(lines)
report = []

def find(pat, start=0, label=''):
    rx = re.compile(pat)
    for i in range(start, len(lines)):
        if rx.search(lines[i]):
            return i
    raise SystemExit('   ANCHOR NOT FOUND [%s]: %s' % (label, pat))

def block(startpat, endpat, newfile, label, start_off=0, end_off=0, end_index=None):
    """replace lines[a..b] inclusive with the content of newfile; ASSERT the range is sane.
    ⚠ THE END ANCHOR IS SEARCHED FROM **THE START ANCHOR + 1**, NEVER FROM `a`.  With a negative
    start_off `a` sits BEFORE the anchor, and an end pattern that also matches the line at `a`
    resolves to a range of length zero -- which is a silent no-op wearing a replacement's clothes."""
    anchor = find(startpat, 0, label + '/start')
    a = anchor + start_off
    b = (end_index if end_index is not None else find(endpat, anchor + 1, label + '/end')) + end_off
    if b < a:
        raise SystemExit('   BAD RANGE [%s]: %d..%d' % (label, a + 1, b + 1))
    with io.open(os.path.join(SP, newfile), encoding='utf-8', newline='') as f:
        new = f.read().split(chr(10))
    if new and new[-1] == '':
        new.pop()
    old_n = b - a + 1
    lines[a:b + 1] = new
    report.append('BLOCK %-14s lines %5d..%-5d (%4d) -> %4d from %s' % (label, a + 1, b + 1, old_n, len(new), newfile))

def subline(pat, newtext, label, want=1):
    """replace WHOLE lines matching pat; assert the count."""
    rx = re.compile(pat)
    n = 0
    for i in range(len(lines)):
        if rx.search(lines[i]):
            lines[i] = newtext
            n += 1
    if n != want:
        raise SystemExit('   SUBLINE [%s] matched %d line(s), wanted %d' % (label, n, want))
    report.append('LINE  %-14s %d line(s) replaced' % (label, n))

def gsub(old, new, label, minimum=1):
    n = 0
    for i in range(len(lines)):
        if old in lines[i]:
            n += lines[i].count(old)
            lines[i] = lines[i].replace(old, new)
    if n < minimum:
        raise SystemExit('   GSUB [%s] replaced %d occurrence(s), wanted >= %d -- a substitution that '
                         'matches nothing is a SILENT NO-OP and the derivation chain compounds it'
                         % (label, n, minimum))
    report.append('GSUB  %-14s %d occurrence(s)  [%s -> %s]' % (label, n, old[:44], new[:44]))

# ---- BLOCKS, in DESCENDING line order so earlier replacements cannot move later anchors ----------
# (each anchor is re-resolved from the CURRENT buffer, so the order is belt and braces; a descending
#  order also keeps the printed line numbers honest against the ORIGINAL file)
#
# tail: from the final '# ====' separator (the one immediately above the SEAT 13 NOTE) to EOF
a = find(r"^stamp \"SEAT 13 NOTE ::", 0, 'tail/anchor') - 1
with io.open(os.path.join(SP, 't46-block-tail.txt'), encoding='utf-8', newline='') as f:
    new = f.read().split(chr(10))
if new and new[-1] == '':
    new.pop()
old_n = len(lines) - a
lines[a:] = new
TAIL_START = a
report.append('BLOCK %-14s lines %5d..EOF   (%4d) -> %4d from %s' % ('tail', a + 1, old_n, len(new), 't46-block-tail.txt'))

# LEG K ends exactly where the tail block now begins.  ⚠ The end is given as an INDEX rather than a
# pattern: `^# =+$` also matches the separator that CLOSES the LEG K comment header, and an end anchor
# that can resolve inside its own block is how a replacement silently truncates.
block(r'^# LEG K -- THE COST CANARY PAIR', None, 't46-block-legk.txt', 'legK',
      start_off=-1, end_index=TAIL_START - 1)

subline(r'^      stamp "      \$nm \[\$ph\] -- one of the EIGHT, in the .-drop artifact',
        '      stamp "      $nm [$ph] -- one of the EIGHT alias-carrying projects.  ⚠ THE NAMED PAIRING-FAILURE READING THAT ONCE SAT HERE IS RETIRED (2026-09-08): the alias fold makes the emission PIN-INDEPENDENT, so a Compile+Target red on one of the eight is a finding about a SEAT like any other, and the seat that changes converter source is where to look"',
        'l5-eight-note')

subline(r'^  stamp "  \^ LEG 5 FINDING: \$L5N project\(s\) failed, where E2. expects NONE',
        '  stamp "  ^ LEG 5 FINDING: $L5N project(s) failed, where E2\' expects NONE.  EVERY member is a finding BY NAME -- there is no absorbed set under the pairing.  ⚠ WHERE TO LOOK, IN THIS TRAIN\'S OWN ORDER: seat 2 first (it changes src/gen -- a generator template is invisible to CNR and to the stdlib solution alike, and THIS phase is route #7\'s named gate), then seat 3 (golib + corpus hand-owns), then seat 1 (the converter change and its nested sub-libraries, which route #3 says no other gate enumerates).  The members:"',
        'l5-failing-note')

subline(r'^stamp "LEG 5 CALIBRATION OF RECORD ::',
        'stamp "LEG 5 CALIBRATION OF RECORD :: ⚠ THE CALIBRATION IS **DERIVED AT RUN TIME AND SCORED**, NEVER CARRIED.  Train 45\'s independent enumeration at ITS base (a2e3b51c1) read enumerated=685 marked=659 unmarked=26; train 45 then LANDED two guard projects, so THIS base (44f858717) is PREDICTED to read 687 / 661 / 26 and this run\'s seats add their own on top.  ⚠ THE SEATS DO NOT ADD ONE PROJECT EACH: seat 1\'s guard carries TWO SUB-LIBRARIES and the runner enumerates RECURSIVELY and DEEPEST-FIRST (route #3), so the enumeration grows by MORE than the top-level count -- derived here as newTopLevel=$NEWBEH newAllDepths=$NEWBEH_ALL.  PREDICTION for this run: N = 687 + $NEWBEH_ALL, marked = 661 + (the new projects whose package_info.cs carries the attribute), unmarked = 26 + (those that do not).  The numbers above are THIS tree\'s; what is ASSERTED below is the RELATION, never a literal, so a wrong prediction is a stamped miss and not a false red."',
        'l5-calibration')

block(r"^# --- SEAT 5's OWN ARMS\.", r'^# =+$', 't46-block-leg3arms.txt', 'leg3arms',
      start_off=0, end_off=-1)

# LEG 1's registration arithmetic compared the slnx delta against the TOP-LEVEL new-project count.
# Correct for train 45 (both its guards were flat) and a FALSE RED here: seat 1 adds a guard with TWO
# sub-libraries, so the solution grows by 3 for 1 top-level project.
block(r'^stamp "LEG 1 registration arithmetic \(DERIVED', r'^\[ "\$L1REGD" -ge 0 \]',
      't46-block-leg1arith.txt', 'leg1arith')

# LEG 3's own comment HEADER carries train 45's literal prediction (780 / 793 / 752 / 744) and names
# that train's seat-7 test files -- both stale-figure class, and the second one makes the land script's
# dry-read negative control read those words in THIS tree.
block(r'^# LEG 3: GolibTests at BOTH configurations', r'^GTCSPROJ=',
      't46-block-leg3hdr.txt', 'leg3hdr', end_off=-1)

block(r'^# LEG 4b -- CNR UNDER THE', r'^pin_go_pairing "LEG-5"', 't46-block-leg4b.txt', 'leg4b',
      start_off=-1, end_off=-1)

block(r'^  stamp "LEG 4 DIAGNOSIS \(drift is present', r'^  rm -f "/tmp/\$\{TAG\}-l4-diff\.txt"$',
      't46-block-leg4diag.txt', 'leg4diag')

block(r'^# LEG D -- THE UNION TWO-SEEDED', r'^pin_go 1\.24\.13 "POST-LEG-D"',
      't46-block-legd.txt', 'legD', start_off=-1)

block(r'^# --- G11\(b\) THE CONVERTER OBLIGATIONS', r'^# --- G11\(c\) THE BATTERY LEGS',
      't46-block-g11b.txt', 'g11b', end_off=-2)

# LEG 0: the NAMED pairing-failure branch is retired for the same reason LEG 4's was, and its header
# carries three sentences that are now false (the rebuild line, the named diagnosis, the seat pointer).
block(r'^  elif \[ "\$\{F_CO:-0\}" -ge 1 \] && \[ "\$\{F_TA:-0\}" -ge 1 \]', r'^  fi$',
      't46-block-leg0branch.txt', 'leg0branch')
block(r"^# LEG 0 -- THE PAIRING'S CHEAPEST POSITIVE CONTROL", r'^pin_go_pairing "LEG-0"',
      't46-block-leg0hdr.txt', 'leg0hdr', end_off=-1)

subline(r'^LEGCG_FILTER=',
        "LEGCG_FILTER='ImportAliasRename|RootShadow|StripLocalTypeQualifier|SiblingClosureContributes|SiblingTestDeclarators|ValueCloneStamp|FleetIdentifier|Clearance|Denied|ConverterStalenessConsultsTheToolchain|EmbedDirectivesStayWithin|HarnessRebuildPredicates|LinknamePushRegistry|ManualConversionRegistrations|Projitems|RunActionHostArgv|NamespaceShadow|AliasNamespace|ImportSpec|TypeParamNil|SliceNil|ManualType'",
        'legcg-filter')

block(r'^# --- G10c THE EIGHT ARTIFACT GOLDENS', r'^\[ "\$\{G10DCR:-1\}" = "0" \]',
      't46-block-g10.txt', 'g10')

# DOCS-DELTA's zero branch called a zero IMPOSSIBLE because FOUR of train 45's seats were docs seats.
# Train 46 has ONE record-carrying named seat, so a zero is a PARTIAL-train reading and is checked
# against the skip count rather than refused outright.
block(r"^  stamp \"DOCS-DELTA :: 0 docs/\*\.md files in this train's delta", r'^  FAILED=1$',
      't46-block-docsdelta.txt', 'docsdelta')

# G10b REFUSED on `.cs.auto count >= 1` -- correct for train 45's seat 2 and a FALSE RED here, since
# NO seat in train 46 carries a review sibling.  The shape and orphan arms stay ASSERTED.
block(r'^stamp "G10b seat 2 metadata ::', r'^\[ "\$\{G10BN:-0\}" -ge 1 \]',
      't46-block-g10b-tail.txt', 'g10b')

# LEG R's post-restore re-read asserted train 45 seat 2's `.cs.auto` hunk -- a landed fact that could
# not go red about THIS train.  Its subject becomes seat 3's displacement, which the pipeline CAN move.
block(r"^  # --- and RE-ASSERT seat 2's corpus hunk AFTER the pipeline pass\.",
      r'^  \{ \[ "\$\{A2OK2:-0\}" = "1" \]', 't46-block-legr-a2.txt', 'legr-a2')

block(r"^# --- A1 SEAT 1's ALIAS FOLD", r'^\[ "\$FAILED" = "0" \] \|\| \{ stamp "=== A-ASSERTIONS FAILED',
      't46-block-assertions.txt', 'assertions', end_off=-2)

block(r'^SEATS_LISTED=\$\(printf', r'^stamp "NEW behavioral projects in this train',
      't46-block-loop.txt', 'loop')

# merge_seat's failure branch gains the PRE-RESOLVED path the rehearsal feeds.
block(r'^  if \[ "\$mrc" != 0 \]; then$', r'^  fi$', 't46-block-mergefail.txt', 'mergefail')

block(r'^  case "\$cls" in$', r'^  esac$', 't46-block-classes.txt', 'classes')

block(r"^# --- THERE IS NO ENV SEAT IN THIS TRAIN\.", r'^ALIAS_NEW=',
      't46-block-seats.txt', 'seats')

# the ancestry pins
with io.open(os.path.join(SP, 't46-block-ancestry.txt'), encoding='utf-8', newline='') as f:
    anc = f.read().split(chr(10))
if anc and anc[-1] == '':
    anc.pop()
a = find(r"^EXPECT_43='", 0, 'ancestry/start')
b = find(r"^EXPECT_44='", a, 'ancestry/end')
lines[a:b + 1] = anc
report.append('BLOCK %-14s %d line(s) -> %d from t46-block-ancestry.txt' % ('ancestry', b - a + 1, len(anc)))

# the header: line 0 .. the line before `set -u`
b = find(r'^set -u$', 0, 'header/end') - 1
with io.open(os.path.join(SP, 't46-block-header.txt'), encoding='utf-8', newline='') as f:
    hdr = f.read().split(chr(10))
if hdr and hdr[-1] == '':
    hdr.pop()
old_n = b + 1
lines[0:b + 1] = hdr
report.append('BLOCK %-14s lines %5d..%-5d (%4d) -> %4d from %s' % ('header', 1, b + 1, old_n, len(hdr), 't46-block-header.txt'))

# the ancestry LOOP consumes the two constants by name, so it moves with them
subline(r'^for anc in "\$EXPECT_43:',
        'for anc in "$EXPECT_44:train 44 (C1\'s finalizer chain and six docs seats)" "$EXPECT_45:train 45 (four converter seats, two golib seats and C1\'s corpus stack -- THIS derive\'s own base)"; do',
        'ancestry-loop')

# the BASE assignment gains an EXPLICIT assertion against the derive-time base, both SHAs PRINTED
subline(r'^BASE=\$\(git rev-parse --short HEAD\)$',
        'BASE=$(git rev-parse --short HEAD)\n'
        '# --- ⚠ THE BASE IS **ASSERTED**, NOT MERELY DERIVED, AND BOTH SHAS ARE PRINTED.  HEAD == origin/master\n'
        '#     above says the tree is AT master; it does NOT say master is the master this script was derived\n'
        '#     against.  A gate reading has a TREE, and when the tree moves the reading expires whether or not\n'
        '#     the change does -- so every per-leg expectation, every class shape and LEG D\'s whole prediction\n'
        '#     were derived for EXACTLY this base, and a different one means re-deriving rather than adjusting.\n'
        'stamp "BASE ASSERTION :: derived-time base EXPECT_45=$EXPECT_45 :: this run\'s BASE (== HEAD == origin/master) = $BASE :: origin/master right now reads $(git ls-remote origin refs/heads/master | cut -c1-9)"\n'
        'case "$BASE" in\n'
        '  "$EXPECT_45"*) : ;;\n'
        '  *) stamp "  ^ BASE REFUSED: this script was derived against $EXPECT_45 and the tree is at $BASE.  Every expectation, class shape and prediction in it was derived for that base -- RE-DERIVE, do not adjust -- ABORT"; exit 2 ;;\n'
        'esac',
        'base-assertion')

# ---- GLOBAL SUBSTITUTIONS over what remains, each ASSERTED -------------------------------------
# ---- the STAMPS that still name train-45 seats or the retired leg -------------------------------
LEGS = ("LEG 0 dial guard | LEG 1 integrity x3 GOOS | LEG 2 go2cs-stdlib.slnx | LEG 2b go2cs.slnx | "
        "LEG D two-seeded three-target -stdlib emission diff | LEG R -tests convert-then-build of reflect and errors | "
        "LEG 3 GolibTests x2 configurations | LEG 4 CNR under the pairing | LEG 5 FULL behavioral suite | "
        "LEG K derived reflect canary set + sync + nistec")
subline(r'^LEGS_ALL=', 'LEGS_ALL="%s"' % LEGS, 'legs-all')

subline(r'^  stamp "G11\(a\) justification CHECKED ::',
        '  stamp "G11(a) justification CHECKED :: golib=$G11GOLIB syscall/windows=$G11SYS GolibTests=$G11GT -- the golib gate families ARE owed and this battery RUNS them: go2cs-stdlib.slnx (LEG 2), go2cs.slnx (LEG 2b -- CLAUDE.md\'s named gate for a golib API change AND route #7\'s cross-assembly consumer gate for the src/gen half), GolibTests at BOTH configurations (LEG 3), and the FULL behavioral suite (LEG 5, route #7\'s behavioral twin -- the only leg that sees a golib change emitting byte-identical .cs).  ⚠ TWO SEATS ARE IN THIS COUNT: seat 2 (ISlice.IsNil in golib AND the go2cs-gen inherited-type template) and seat 3 (the FatalReport primitive, its corpus consumers and the sync retarget).  Seat 3 EMITS a displacement, so LEG D can see part of it; the sync retarget emits NOTHING, so LEG K\'s banked \\`sync\\` row is the whole of what can see that half."',
        'g11a-checked')
subline(r'^  stamp "G11\(a\) JUSTIFICATION FALSE ::',
        '  stamp "G11(a) JUSTIFICATION FALSE :: golib=$G11GOLIB syscall/windows=$G11SYS GolibTests=$G11GT -- at least one is ZERO.  ⚠ syscall/windows may legitimately read 0 in THIS train (no seat here is a syscall seat, unlike train 45\'s), so read the THREE numbers separately rather than the verdict: golib=0 or GolibTests=0 with seats 2 and 3 aboard means a seat did not land what it was pinned for, and the golib legs would be measuring a tree with nothing new in it."',
        'g11a-false')

subline(r'^  chain_stop "LEG 2 \(go2cs-stdlib\.slnx\)"',
        '  chain_stop "LEG 2 (go2cs-stdlib.slnx)" "the corpus does not compile with seat 2\'s golib change and seat 3\'s golib primitive, corpus hand-owns and displacement in it, so nothing below can be read as a statement about this train." "$LEGS_ALL"',
        'leg2-chainstop')
subline(r'^  chain_stop "LEG 2b \(go2cs\.slnx\)"',
        '  chain_stop "LEG 2b (go2cs.slnx)" "the non-generated solution members do not compile against seat 2\'s golib API and its go2cs-gen template.  ⚠ THIS IS ROUTE #7\'s NAMED GATE: a src/gen change is invisible to CNR (transpile-only) and to the stdlib solution (one assembly at a time), so this build and LEG 5\'s COMPILE phase are the only legs that compile a CROSS-ASSEMBLY consumer of the generated shell." "LEG D two-seeded three-target -stdlib emission diff | LEG R -tests convert-then-build | LEG 3 GolibTests x2 | LEG 4 CNR | LEG 5 FULL behavioral suite | LEG K derived canary set + sync + nistec"',
        'leg2b-chainstop')

subline(r'^stamp "LEG 3 GolibTests arithmetic',
        'stamp "LEG 3 GolibTests arithmetic (computed from THIS tree and THIS csproj, no build) :: declared=$DECL :: removed unset=$GT_R_UNSET windows=$GT_R_WIN linux=$GT_R_LNX darwin=$GT_R_DRW :: ADMISSIBLE TOTALS unset=$GT_T_UNSET windows=$GT_T_WIN linux=$GT_T_LNX darwin=$GT_T_DRW :: unknownConditionShapes=$GTUNKNOWN (must be 0).  ⚠ NO LITERAL PREDICTION IS CARRIED HERE.  Train 45\'s stamp named 793 declared and 752 admissible, both measured at ONE seat\'s pin -- correct that day and the stale-figure class the moment a GolibTests file grows.  This train has TWO seats adding arms, so the DECLARED count is reconciled against the BASE further down (declared(base) + this train\'s delta == declared(union)), which is the only form of that check whose two sides are not both this tree."',
        'leg3-arith')
subline(r'^stamp "LEG 3 ⚠ the ordinary run is the UNSET column',
        'stamp "LEG 3 ⚠ the ordinary run is the UNSET column, so the expected Total is $GT_T_UNSET -- COMPUTED from this csproj at run time, never a literal.  GolibTests sets no \\$(GoTargetOS), so the \'!= linux\' Remove group APPLIES and the \'!= \\"\\" and != windows\' group does NOT (its condition requires GoTargetOS to be non-empty), which is why the ordinary run compiles the windows-only files."',
        'leg3-unset')
subline(r'^\[ "\$\(\( \$\{SD:-0\} - \$\{SR:-0\} \)\)" = "3" \]',
        '[ "$(( ${SD:-0} - ${SR:-0} ))" = "3" ] || { stamp "  ^ SKIP DELTA != 3 -- one configuration under-measured the GC/pin-liveness class, which RUNS at Release and SELF-SKIPS at Debug (a non-optimizing frame would root its temporaries and make the assertion unfalsifiable).  ⚠ ADJUDICATE against THIS TRAIN\'S OWN NEW ARMS before reading it as a regression: seat 3 adds FatalReportTests.cs and seat 2 extends ArrayRangeAllocationTests.cs, and a NEW test that self-skips at one configuration MOVES this delta.  3 is the measured number carried into a tree with new methods in it; the raw skip counts are stamped above and the DECLARED-count reconciliation below says how many arms this train added."; FAILED=1; }',
        'leg3-skipdelta')

subline(r'^  \{ \[ "\$P_OFAIL" = "0" \] && \[ "\$P_OTMO" = "0" \]; \}',
        '  { [ "$P_OFAIL" = "0" ] && [ "$P_OTMO" = "0" ]; } || { stamp "  ^ LEG 5 FINDING: Output fail=$P_OFAIL timeout=$P_OTMO, and E2\' expects 0 of each.  A FAILED comparison is a C#-vs-\\`go run\\` divergence and is where seat 2\'s golib/generator change or seat 3\'s golib primitive would show; a TIMEOUT is NOT MEASURED and is never a pass."; L5RECFAIL=1; }',
        'leg5-outfail')

def assert_absent(tok, label):
    n = sum(l.count(tok) for l in lines)
    if n:
        raise SystemExit('   ABSENCE [%s]: %d occurrence(s) of [%s] survive -- the block that owned it '
                         'did not replace everything it was supposed to' % (label, n, tok))
    report.append('ABSENT %-13s 0 occurrence(s) of [%s] -- its whole mechanism was replaced' % (label, tok))

# the skip-authorisation mechanism was REPLACED WHOLESALE (train 45's PENDING default was the inverse
# of this train's), so nothing is renamed here -- the assertion is that nothing SURVIVED.
assert_absent('TRAIN45_SKIP_PENDING', 'skip-envvar')
assert_absent('TRAIN 45 LEG D', 'legd-title')
gsub('coord-train45-assemble', 'coord-train46-assemble', 'self-name', 1)

# ---- THE PROSE FIXES.  ⚠ THEY RUN FROM A SEPARATE FILE AND ARE PART OF THE DERIVE, NOT AN EDIT
#      MADE AFTERWARDS.  A derive whose output is hand-patched once is a derive nobody can re-run, and
#      the next generation inherits a file it cannot reproduce.  Each fix asserts its anchor matches
#      EXACTLY ONCE.
# ---- WRITE ----------------------------------------------------------------------------------
out = chr(10).join(lines) + chr(10)
with io.open(DST, 'w', encoding='utf-8', newline='') as f:
    f.write(out)

print('=== TRAIN 46 DERIVE ===')
print('source %s  %d line(s)' % (os.path.basename(SRC), orig_n))
# ⚠ COUNT THE REAL LINES, NOT THE LIST ELEMENTS.  A `subline` replacement may carry embedded newlines,
#   so one list element can be ten lines of the output file; reporting len(lines) would understate the
#   result by exactly those, and a derive whose own summary disagrees with `wc -l` is an instrument
#   nobody can check against the artifact.
print('dest   %s  %d line(s)  (list elements=%d; the difference is multi-line subline replacements)'
      % (os.path.basename(DST), out.count(chr(10)), len(lines)))
for r in report:
    print('  ' + r)
# residual census: any train-45 SHA left OUTSIDE a comment line
SHAS = ['234cf8e8d', '1922e3ec1', '31668f43e', '787159ed7', 'aa7abc006', '3e5ead2d1',
        '7d377e27b', 'a29253a2b', '044116000', '9092ab8b7', '4a8642e7e', 'c08cb29c5',
        'acab60084', '13908a888', 'd839cb1d7', '19bb74012', 'f613d5cfa', '35105cdcd',
        '193af90f5', '489c5553c', 'dddd46493', '8d7c348bb', '34cf4ad02', 'b6746ab18',
        '541b4fd7b', 'ce77d061c', '7ec93b2b5', '84b591309', 'e96349c54', '1cf3af3635',
        'a9b4e036f', 'bef7a6dbd', '0858372b5', 'ad83d04a3', '6adab2909', 'd3223d252']
code_hits = []
comment_hits = 0
for i, l in enumerate(lines):
    s = l.lstrip()
    for sh in SHAS:
        if sh in l:
            if s.startswith('#'):
                comment_hits += 1
            else:
                code_hits.append((i + 1, sh, l.strip()[:120]))
print('  RESIDUAL train-45-era SHAs :: in COMMENTS=%d (allowed -- dated lessons) :: in CODE=%d (must be 0)'
      % (comment_hits, len(code_hits)))
for h in code_hits:
    print('    line %d [%s] %s' % h)
print('=== DERIVE DONE ===')
sys.exit(1 if code_hits else 0)
