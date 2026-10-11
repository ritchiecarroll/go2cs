# TRAIN T3 -- the anchored edits applied after R0 (tT3-derive.py). Every edit asserts its anchor occurs EXACTLY the
# number of times written; a moved anchor stops the derive (nothing half-applied is kept: each file is written only
# after all of its edits applied). Run once by the derive, 2026-10-10, as the record of T3's changes (tT3-CHANGES.md).
#   python -B tT3-derive-edits.py <this folder>
import os, re, sys

D = sys.argv[1]
def path(f): return os.path.join(D, f)
def load(f):
    with open(path(f), encoding='utf-8', newline='') as fh: return fh.read()
def save(f, t):
    assert '\r' not in t, f
    with open(path(f), 'w', encoding='utf-8', newline='\n') as fh: fh.write(t)

class Ed:
    def __init__(self, f): self.f = f; self.t = load(f)
    def rep(self, old, new, count=1):
        n = self.t.count(old)
        assert n == count, f'{self.f}: anchor x{n} (want {count}): {old[:90]!r}'
        self.t = self.t.replace(old, new); return self
    def before(self, anchor, text): return self.rep(anchor, text + anchor)
    def after(self, anchor, text): return self.rep(anchor, anchor + text)
    def resub(self, rx, new, count=1):
        t, n = re.subn(rx, new, self.t, flags=re.S)
        assert n == count, f'{self.f}: regex x{n} (want {count}): {rx[:90]!r}'
        self.t = t; return self
    def done(self): save(self.f, self.t)

SHEBANG = '#!/usr/bin/env bash\n'

# =============================================================================================================== assemble
e = Ed('tT3-assemble.sh')
e.after(SHEBANG, """# TRAIN T3 (2026-10-10; derived from TRAIN FL's run2 kit, every change in tT3-CHANGES.md). T3 is NOT a corpus
# re-conversion. The rows of tT3-seats-draft.txt (24 at the derive) merge signed --no-ff, in row order, onto the base
# (origin/master's tip: 4e6322d770 at the derive) on the LOCAL branch train-t3-union in /h/go2cs-tmp-coord/tT3, after
# tT3-conflict-map.sh mapped THIS list from the base (FL's MAP_FROM resume is gone: no T3 row is a hand merge).
# NOTHING HERE PUSHES. T3 adds:
#   T3-2  two notes tokens. 'contains <sha>': in the TABLE check, the row's seated sha carries that commit (row 15 carries
#         cut 8 pack-exclude; rows 19 and 20 carry 64804ed9b5). 'in-head <sha>': in the TABLE check an EARLIER row
#         carries it, and at MERGE time the union's HEAD holds it before the row merges (row 20: 07411b26f7, row 19's tip).
#   T3-3  the i9 tree arm (i9tree, between markers; tT3-controls.sh arm I9 runs it on planted heads): the tree at the
#         last seat merge must EQUAL the tree of the i9's rehearsal union (claude/i9-union-t3-ref, read from the same
#         ls-remote as the seats; I9_TREE=<the posted tree, a prefix is enough> pins what that ref must hold). EQUAL is
#         the only pass; DIFFERENT prints git diff --stat (i9 -> this union) and stops at exit 4 after every other line;
#         a missing ref is a NOTE, and a STOP when I9_TREE is given.
""")
e.before("  ROWOF[$ref]=$i; FULLOF[$ref]=$full\n", """  # T3 (T3-2): 'contains <sha>' -- the row CARRIES that commit; 'in-head <sha>' -- an EARLIER row carries it (the merge
  # loop asserts it is in HEAD before this row merges).
  for cs in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])contains [0-9a-f]{7,40}' | sed 's/.*contains //'); do
    csf=$(git rev-parse --verify -q "$cs^{commit}") || { echo "TABLE row $i $ref: 'contains $cs' does not resolve"; tbad=$((tbad + 1)); continue; }
    if git merge-base --is-ancestor "$csf" "$full"; then echo "TABLE row $i $ref: contains ${csf:0:10} (declared; ancestor verified)"
    else echo "TABLE row $i $ref: 'contains ${csf:0:10}' but it is NOT an ancestor of ${full:0:10}"; tbad=$((tbad + 1)); fi
  done
  for ih in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])in-head [0-9a-f]{7,40}' | sed 's/.*in-head //'); do
    ihf=$(git rev-parse --verify -q "$ih^{commit}") || { echo "TABLE row $i $ref: 'in-head $ih' does not resolve"; tbad=$((tbad + 1)); continue; }
    ihr=''; for r in "${!FULLOF[@]}"; do git merge-base --is-ancestor "$ihf" "${FULLOF[$r]}" && ihr="$ihr$r "; done
    if [ -n "$ihr" ]; then echo "TABLE row $i $ref: in-head ${ihf:0:10} (declared; carried by the earlier row(s) ${ihr% }; asserted in HEAD at the merge)"
    else echo "TABLE row $i $ref: 'in-head ${ihf:0:10}' but NO earlier row carries it (the merge could never find it in HEAD)"; tbad=$((tbad + 1)); fi
  done
""")
e.before("  printf 'Merge claude/%s (%s) into TRAIN T3 -- %s\\n\\nCo-Authored-By", """  # T3 (T3-2): 'in-head <sha>': the union's HEAD holds that commit BEFORE this row merges (row 20: 07411b26f7, row 19's).
  for ih in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])in-head [0-9a-f]{7,40}' | sed 's/.*in-head //'); do
    git merge-base --is-ancestor "$ih" HEAD || { echo "ABORT seat $n $ref: in-head $ih is NOT in HEAD $(git rev-parse --short=10 HEAD): the row that carries it has not merged"; exit 3; }
    echo "SEAT $n $ref: in-head ${ih:0:10} verified in HEAD $(git rev-parse --short=10 HEAD)"
  done
""")
e.before("MTBAD=1   # verify round 3", """# T3 (T3-3) >>> i9tree (tT3-controls.sh arm I9 extracts the lines between the markers and runs them on planted heads)
i9tree(){ # head i9-tip [posted i9 tree, a prefix] -> one verdict line; rc 0 EQUAL; 1 DIFFERENT, or the i9 tip does not hold the posted tree; 2 not comparable
  local h=$1 t=$2 e=${3:-} ht tt
  [ -n "$t" ] || { echo "I9 TREE: NOT COMPARED (no i9 union ref at origin)"; return 2; }
  git cat-file -e "$t^{commit}" 2>/dev/null || { echo "I9 TREE: NOT COMPARED (${t:0:10} is not in the object store: fetch it)"; return 2; }
  tt=$(git rev-parse "$t^{tree}"); ht=$(git rev-parse "$h^{tree}")
  if [ -n "$e" ] && [ "${tt#"$e"}" = "$tt" ]; then echo "I9 TREE: REFUSED -- the i9 union ${t:0:10} holds tree ${tt:0:10}, not the posted $e"; return 1; fi
  if [ "$ht" = "$tt" ]; then echo "I9 TREE: EQUAL -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} = the i9 union ${t:0:10} tree ${tt:0:10}"; return 0; fi
  echo "I9 TREE: DIFFERENT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} != the i9 union ${t:0:10} tree ${tt:0:10}; the paths (git diff --stat, i9 -> this union):"
  git diff --stat=160 "$t" "$h" | sed 's/^/  /'; return 1
}
# <<< i9tree
""")
e.after("""echo "head=$(git rev-parse --short=10 HEAD) rows=$NSEATS rows-sha256=$RSHA :: $MT" > "$X/tT3/assemble-maptree.txt"
""", """# T3 (T3-3): the i9's rehearsal union, read from the SAME ls-remote as the seat tips (never a local tracking ref).
I9_REF=${I9_REF:-claude/i9-union-t3-ref}
I9TIP=$(awk -v r="refs/heads/$I9_REF" '$2 == r { print $1 }' "$X/tT3/assemble-lsremote.txt")
I9V=$(i9tree "$SEATTOP" "$I9TIP" "${I9_TREE:-}"); i9rc=$?
printf '%s\\n' "$I9V"
echo "head=$(git rev-parse --short=10 HEAD) :: $(printf '%s' "$I9V" | head -n 1) (I9_REF=$I9_REF I9_TREE=${I9_TREE:-unset})" >> "$X/tT3/assemble-maptree.txt"
I9BAD=0
case "$i9rc" in 0) ;; 1) I9BAD=1 ;; *) [ -z "${I9_TREE:-}" ] || I9BAD=1 ;; esac   # not comparable: a NOTE, a STOP when COORD posted a tree
""")
e.before('[ "$MTBAD" = 0 ] || { echo "STOP: $MT', """[ "$I9BAD" = 0 ] || { echo "STOP: $(printf '%s' "$I9V" | head -n 1) -- the assembled union stands at $(git rev-parse --short=10 HEAD) and is NOT accepted (exit 4): COORD reads the listed paths before anything else"; exit 4; }
""")
e.done()

# ============================================================================================================ conflict-map
e = Ed('tT3-conflict-map.sh')
e.after(SHEBANG, """# TRAIN T3 (2026-10-10, tT3-CHANGES.md): FL's MAP_FROM (FL8) is DROPPED -- no T3 row is a hand merge, so the map
# always starts at BASE (Q's behaviour) and reads the whole list. Everything else is FL's. Run it with SEATS=<the run
# folder's tT3-seats-draft.txt> (the default names the hnd copy, which exists only once COORD commits the kit).
""")
e.resub(r"MAPFROMP=''\nif \[ -n \"\$\{MAP_FROM:-\}\" \]; then\n.*?\nelse\n  git worktree add -q --detach \"\$WT\" \"\$BASE\" \|\| exit 1\nfi\n",
        "[ -z \"${MAP_FROM:-}\" ] || { echo \"ABORT: MAP_FROM is set: T3 maps from BASE only (FL8 is dropped: tT3-CHANGES.md)\"; exit 2; }\n"
        "git worktree add -q --detach \"$WT\" \"$BASE\" || exit 1\n")
e.rep("""    if [ -n "$MAPFROMP" ] && printf '%s\\n' "$MAPFROMP" | grep -qxF "$full"; then echo "SEAT $n $ref: merged at MAP_FROM (the hand union's own merge; not merged again)"; continue; fi
""", "")
e.done()

# ================================================================================================================ helpers
e = Ed('tT3-helpers.py')
e.before("def te_class2(rem, add, fname=None):\n", """# T3 (T3-4): THE FACE LIFT'S CLASSES ARE OFF. FL's refresh (0574b8336c) committed the face-lifted test sources and no T3
# row changes -stdlib or -tests emission, so a face-lift-shaped hunk at T3 is not an expected class: it reads OTHER and
# is read by name. fl_normalize, its plants and flcontrol stay in the file, INERT (tT3-controls.sh arm FL-OFF proves
# the switch: every planted face-lift positive now reads OTHER). The FACELIFT key stays in every count line (always 0)
# so each reader's line format holds.
FL_CLASSES = False

""")
e.rep("    r, a, fl = fl_normalize(rem, add, fname)\n",
      "    r, a, fl = fl_normalize(rem, add, fname) if FL_CLASSES else (list(rem), list(add), {})   # T3 (T3-4)\n")
e.done()

# ================================================================================================================== fixup
e = Ed('tT3-fixup.sh')
e.after(SHEBANG, """# TRAIN T3 (2026-10-10, tT3-CHANGES.md): R0 names (the LOCAL branch train-t3-union); T3-4 the face lift's classes are
# off (a golden moved by one reads OTHER: GOLDEN_CLASS=facelift is inert); T3-5 step 4t drops FL4's leaving-attribute
# census and keeps Q's -tests arm: PREDICTED 0 moved hunks on all three rows (no T3 row changes -stdlib -tests
# emission). Step 4's PREDICTION at T3: union-attributable 0 on windows, linux and darwin -- the only emission a T3 row
# changes is row 1's darwin os.executablePath pair, which the seat CARRIES (os/darwin/executable_darwin.cs,
# runtime/darwin/os_darwin.cs), and row 16's package READMEs, which the seat regenerated. Step 5: 0 goldens moved.
""")
e.rep("""# FL (FL4): the leaving attributes, as a line census: what the prediction counts at HEAD and the reading counts after
FL_LEAVE_RX='\\[(GoRecv|GoStr|GoEmbedded|GoType)\\]|\\[GoTag\\(|\\[GoArrayDims\\(|\\[GoMapKeyDims\\(|\\[GoDescriptorType\\(|\\[global::System\\.Diagnostics\\.StackTraceHidden\\]|\\[GoType\\("[^"*/]*"\\)\\]'
""", "# T3 (T3-5): FL4's leaving-attribute census is dropped (the face lift landed with FL's refresh 0574b8336c).\n")
e.rep("""    flpred=$(git grep -hcE "$FL_LEAVE_RX" HEAD -- ":(glob)src/core/$pkg/*_test.cs" ":(glob)src/core/$pkg/package_test_info.cs" ":(glob)src/core/$pkg/package_info_internal_test.cs" 2>/dev/null | awk '{s+=$1} END {print s+0}')
""", "")
e.rep("""    flsteps=$(tr -d '\\r' < "$LOGDIR/4t-$pn.te.txt" | grep -a '^TE FACELIFT steps' | sed 's/^TE FACELIFT steps[^:]*: //')
    flgot=$(printf '%s' "$tel" | sed -n 's/.* facelift=\\([0-9]*\\) .*/\\1/p')
    flleft=$(cat src/core/$pkg/*_test.cs src/core/$pkg/package_test_info.cs src/core/$pkg/package_info_internal_test.cs 2>/dev/null | grep -cE "$FL_LEAVE_RX")
    [ "${flpred:-0}" = 0 ] || [ "${flgot:-0}" != 0 ] || T4MIS="$T4MIS$pkg(facelift 0 of $flpred leaving lines) "
""", "")
e.rep(""":: FACELIFT hunks=${flgot:-0} steps [${flsteps:-none}] :: leaving-attribute lines at HEAD=$flpred, left after the conversion=$flleft (by design some stay: a definition holding a comment marker, a nested channel direction, a lambda's parameter) """, "")
e.rep("""(read 4t-$pn.patch BY NAME: what an FL row moves besides the face lift's stated classes)""",
      """(read 4t-$pn.patch BY NAME: T3 PREDICTS 0 -- no T3 row changes -stdlib -tests emission)""")
e.rep('''T4SUM="$T4SUM$pkg:fl${flgot:-0}/lines${flpred}->${flleft}/np${got:-0}/other${oth:-0} "''',
      '''T4SUM="$T4SUM$pkg:np${got:-0}/other${oth:-0} "''')
e.rep("rows [row:fl<FACELIFT hunks>/lines<leaving lines at HEAD>-><left>/np<N-PARTIAL read>/other<OTHER hunks>]",
      "rows [row:np<N-PARTIAL read>/other<OTHER hunks>]")
e.done()

# =============================================================================================================== land-prep
e = Ed('tT3-land-prep.sh')
e.resub(r"# FL \(FL1\): the face lift's census of what is about to be committed[^\n]*\nFL_LEAVE_RX=[^\n]*\nflh=[^\n]*\nflw=[^\n]*\nstamp \"        FACE LIFT \(FL1\)[^\n]*\ngrep -a '\^TE FACELIFT steps'[^\n]*\n",
        "# T3 (T3-4): FL1's face-lift census of the refresh is dropped (the face lift landed with FL's refresh 0574b8336c).\n")
e.done()

# ================================================================================================================ battery
e = Ed('tT3-battery.sh')
e.after(SHEBANG, """# TRAIN T3 battery on the i7 (2026-10-10; derived from TRAIN FL's run2 kit, which ran EXIT 0 at 9b7dfdb2ec; every
# change in tT3-CHANGES.md). T3 is NOT a corpus re-conversion. What changes against FL:
#   T3-4  the face lift's -tests classes are OFF (tT3-helpers.py FL_CLASSES): TE, TE-T and TE-i9 read Q's classes; a
#         face-lift-shaped hunk is OTHER, read by name (FACELIFT counts print 0).
#   T3-6  the previous train's record is TRAIN FL's run2 (FL_RUN; N_RUN, O_RUN, P_RUN and Q_RUN are refused). The
#         variables Q_SUMS / Q_REWRITES / Q_REWRITES_T keep their names and point INTO FL's run2.
#   T3-7a LEG TP: G's trim probes (claude/g-trim-probes, a MEASUREMENT REF that never lands) published with the
#         template-default publish against THIS union's src: 5 programs, consumer RUNS-CLEAN, four GO-EQUAL.
#   T3-7b LEG GN reads G's trim guards BY NAME (GN_TRIM: 15 GenTests methods the trim rows add) from a TRX, and its
#         total floor is the union's (189 [TestMethod]/[DataTestMethod] lines; 208 results, the i9 read 208/208).
#   T3-9  PRE-D CNR-FRESH admits row 5's ruled change to check-no-regression.ps1 EXACTLY (cnralias: only quoted names
#         and comments added inside $documentedAliasDriftPackages, nothing removed, the names = CNR_ALIAS_RULED);
#         anything else in the script is still a finding (floor 10).
# Predictions at the 24-row union (derived from the objects at the i9's equal-tree union 71e5f69dda; restated at the
# assembled head in tT3-README.md section 5): CNR N = 871 (878 enumerated, 7 platform skips; +5 behavioral projects:
# OsExecutablePath, OsGetpagesize, ReexecArgv0Token, Phase5Breakpoint, Phase5CoverageAPIs); GN Total 208 (floor 189);
# E union-attributable 0 x3. FL's header follows.
""")
e.rep("""[ -z "${N_RUN:-}${O_RUN:-}${P_RUN:-}" ] || { echo "ABORT: N_RUN, O_RUN or P_RUN is set (an earlier train's variable). TRAIN T3 reads the previous train's record from Q_RUN (default coord-scratch/tQ/run1): unset it and set Q_RUN"; exit 2; }
Q_RUN=${Q_RUN:-/h/go2cs-tmp-coord/coord-scratch/tQ/run1}
Q_SUMS="$Q_RUN/tQ-logs/SUMMARY.txt"                 # TL-WALL, FX floor, CNR control, the S rows' HOSTWALL baseline: TRAIN Q's battery (FL: the record shifts from P to Q)
Q_REWRITES="$Q_RUN/tQ-logs/S-rewrites-U0.patch"     # TE baseline: Q's sweep rewrites, same box (at FL the face-lift classes are counted first; OTHER is compared with it)
Q_REWRITES_T="$Q_RUN/tQ-logs/T-rewrites-U0.patch"   # TE-T baseline: Q's T-leg rewrites
""", """[ -z "${N_RUN:-}${O_RUN:-}${P_RUN:-}${Q_RUN:-}" ] || { echo "ABORT: N_RUN, O_RUN, P_RUN or Q_RUN is set (an earlier train's variable). TRAIN T3 reads the previous train's record from FL_RUN (default coord-scratch/tFL/run2): unset it and set FL_RUN"; exit 2; }
FL_RUN=${FL_RUN:-/h/go2cs-tmp-coord/coord-scratch/tFL/run2}   # T3 (T3-6): TRAIN FL's battery of record (EXIT 0 at 9b7dfdb2ec)
Q_SUMS="$FL_RUN/tFL-logs/SUMMARY.txt"                 # TL-WALL, FX floor, CNR control, the S rows' HOSTWALL baseline: TRAIN FL's battery (the name is Q's, kept so every reader below is unchanged)
Q_REWRITES="$FL_RUN/tFL-logs/S-rewrites-U0.patch"     # TE baseline: FL's sweep rewrites, same box
Q_REWRITES_T="$FL_RUN/tFL-logs/T-rewrites-U0.patch"   # TE-T baseline: FL's T-leg rewrites
""")
e.rep("""I9_BASELINE=${I9_BASELINE:-}            # optional: the i9's TRAIN Q shard patch (coord-scratch/tQ/i9-patches/tQ-tracked-changes-U0.patch), as the TE-i9 baseline
""", """I9_BASELINE=${I9_BASELINE:-}            # optional: the i9's TRAIN FL shard patch (coord-scratch/tFL/i9-patches/tFL-tracked-changes-U0.patch), as the TE-i9 baseline
# T3 (T3-7b): G's trim guards in GenTests, BY NAME (the methods the trim rows 19-23 add: read from the diff 4e6322d770..
# the union at the derive; PRE-D below asserts each is declared at HEAD, so a renamed guard is a finding, never a skip).
GN_TRIM='EscapeRegistryGeneratorTests.EveryEscapingClosedContainerIsRegistered GoTypeOpsSourceTests.AStructNamesItsOwnOps GoTypeOpsSourceTests.AGenericStructNamesItsOwnClosedOps GoTypeOpsSourceTests.ANeedyStructCarriesTheFaceBesideItsZeroHook GoTypeOpsSourceTests.ANamedStructAndSliceTypeNameTheirOwnOps GoTypeOpsSourceTests.ANeverPointedToStructCarriesNoFace GoTypeOpsSourceTests.ANamedPointerTypeCarriesNoFace GoTypeOpsSourceTests.EveryFaceMemberNamesOnlyItself GoTypeOpsSourceTests.TheHazardGuardNamesASelfNestingMember GoZeroConstructionHookTests.ANeedyStructNamesItsOwnZero GoZeroConstructionHookTests.AGenericNeedyStructNamesItsOwnClosedZero GoZeroConstructionHookTests.AStructWhoseOnlyBuiltFieldIsBlankStillNamesItsZero GoZeroConstructionHookTests.ADefinedTypeOverAStructNamesItsOwnZero GoZeroConstructionHookTests.AStructWhoseConstructorBuildsNothingHasNoHook MemberRecordGeneratorTests.EveryParamDimsRecordedMethodIsKeptForTrimming'
GN_RESULTS_MIN=208                       # T3: the results the union's GenTests produce (189 method lines, DataRows expand; the i9 read 208/208)
# T3 (T3-7a): the trim probe runner. TP_WT is a detached worktree of claude/g-trim-probes made BEFORE launch (the
# checklist); the battery never fetches. CNR_ALIAS_RULED: row 5's ruled additions to the CNR alias-drift set (T3-9).
TP_REF=${TP_REF:-claude/g-trim-probes}
TP_WT=${TP_WT:-/h/go2cs-tmp-coord/tT3-probes}
TP_CAP=${TP_CAP:-90m}
TP_PIN=${TP_PIN:-04239ec85e}   # COORD 2026-10-10: the probe ref's tip carrying G's CRLF fix (a fast-forward of 2de88c20fa)
CNR_ALIAS_RULED='OsGetpagesize SyscallKeystonePulls'
""")
e.rep("previous-train record (TRAIN Q) Q_RUN=$Q_RUN (", "previous-train record (TRAIN FL) FL_RUN=$FL_RUN (")
e.rep("""[ -n "$CNR_MPREV" ] || { CNR_MPREV=863; CNR_MSRC="the literal 863, TRAIN Q's battery CNR line ('4 asserts: N=863', run1; its SUMMARY is not readable at $Q_SUMS)"; }""",
      """[ -n "$CNR_MPREV" ] || { CNR_MPREV=866; CNR_MSRC="the literal 866, TRAIN FL's battery CNR line ('4 asserts: N=866', run2; its SUMMARY is not readable at $Q_SUMS)"; }""")
e.rep("""[ -n "$FX_FLOOR" ] || { FX_FLOOR=1291; FX_SRC="the literal 1291, TRAIN Q's (and P's, O's, N's and M's) reading (Q's SUMMARY is not readable at $Q_SUMS)"; }""",
      """[ -n "$FX_FLOOR" ] || { FX_FLOOR=1291; FX_SRC="the literal 1291, TRAIN FL's (and Q's, P's, O's, N's and M's) reading (FL's SUMMARY is not readable at $Q_SUMS)"; }""")
e.rep("""[ -z "$CNRDIFF" ] || dfind "CNR_FRESH|the union changes [${CNRDIFF% }]: READ the change for an up-to-date skip before leg 4 is trusted (floor 10: check-no-regression re-transpiles unconditionally, and that asymmetry is why it is immune to stale output)"
""", """# T3 (T3-9): row 5 (c1-cnr-aliasdrift-skp, COORD rulings 2026-10-10) adds two names to the CNR script's documented
# platform-alias drift set. It is admitted EXACTLY: the script is the only file of the two changed, nothing is removed,
# every added line is a comment or one quoted name, the names are CNR_ALIAS_RULED, and each sits inside the
# $documentedAliasDriftPackages array at HEAD. Anything else is the finding it always was (an up-to-date skip hides there).
# >>> cnralias (tT3-controls.sh arm CN extracts the lines between the markers and runs them on planted diffs)
cnralias(){ # a -U0 diff on stdin -> 'names=<sorted, space-joined> other=<n> removed=<n>'
  awk '/^(\\+\\+\\+|---) / { next } /^-/ { rm++; next } /^\\+/ { l = substr($0, 2); if (l ~ /^[ \\t]*#/ || l ~ /^[ \\t]*$/) next; if (match(l, /^[ \\t]*\\047[A-Za-z0-9_]+\\047[ \\t]*(#.*)?$/)) { s = l; gsub(/^[ \\t]*\\047|\\047.*$/, "", s); print "N " s } else ot++ } END { print "O " ot + 0; print "R " rm + 0 }' \\
  | { nm=''; o=0; r=0; while read -r k v; do case "$k" in N) nm="$nm$v"$'\\n' ;; O) o=$v ;; R) r=$v ;; esac; done; printf 'names=%s other=%s removed=%s\\n' "$(printf '%s' "$nm" | LC_ALL=C sort -u | tr '\\n' ' ' | sed 's/ $//')" "$o" "$r"; }
}
# <<< cnralias
if [ -n "$CNRDIFF" ]; then
  CNRA=$(git diff -U0 "$MASTER" HEAD -- src/tests/Behavioral/check-no-regression.ps1 | cnralias)
  CNRARR=$(git show HEAD:src/tests/Behavioral/check-no-regression.ps1 | tr -d '\\r' | awk '/^\\$documentedAliasDriftPackages = @\\(/ { on = 1; next } on && /^\\)/ { on = 0 } on')
  cnrin=''; for nm in $CNR_ALIAS_RULED; do printf '%s\\n' "$CNRARR" | grep -qE "^[[:space:]]*'$nm'" || cnrin="$cnrin$nm "; done
  if [ "$CNRDIFF" = 'src/tests/Behavioral/check-no-regression.ps1 ' ] && [ "$CNRA" = "names=$CNR_ALIAS_RULED other=0 removed=0" ] && [ -z "$cnrin" ]; then
    stamp "PRE-D CNR-FRESH (T3-9): the CNR script's change is row 5's RULED alias-drift entries and nothing else ($CNRA; each inside \\$documentedAliasDriftPackages at HEAD): admitted"
  else
    dfind "CNR_FRESH|the union changes [${CNRDIFF% }] beyond row 5's ruled alias-drift entries ($CNRA; CNR_ALIAS_RULED=[$CNR_ALIAS_RULED]; not inside the array at HEAD: [${cnrin% }]): READ the change for an up-to-date skip before leg 4 is trusted (floor 10: check-no-regression re-transpiles unconditionally, and that asymmetry is why it is immune to stale output)"
  fi
fi
""")
e.rep("""  capped GN gentests 20m dotnet test src/tests/GenTests/GenTests.csproj -c Debug --no-build
  totalgate GN "$LOGDIR/gentests.log" "$(tmfloor src/tests/GenTests)" 68 "the [TestMethod] lines under src/tests/GenTests at HEAD" no
""", """  capped GN gentests 20m dotnet test src/tests/GenTests/GenTests.csproj -c Debug --no-build --logger "trx;LogFileName=gentests.trx" --results-directory "$LOGW"
  totalgate GN "$LOGDIR/gentests.log" "$(tmfloor src/tests/GenTests)" 189 "the [TestMethod] lines under src/tests/GenTests at HEAD" no
  # T3 (T3-7b): the results count against the union's own (DataRows expand: 208 at the union), and G's trim guards BY
  # NAME: each must be declared at HEAD and read Passed in the TRX (a guard NOT FOUND is never green).
  gntot=$(trline "$LOGDIR/gentests.log" | awk '{ print $4 }')
  [ "${gntot:-0}" -ge "$GN_RESULTS_MIN" ] || finding "GN: Total ${gntot:-unread} is below the union's $GN_RESULTS_MIN results"
  gndecl=''; for gm in $GN_TRIM; do git grep -qE "public void ${gm#*.}\\(" HEAD -- "src/tests/GenTests/${gm%%.*}.cs" || gndecl="$gndecl$gm "; done
  if [ -f "$LOGDIR/gentests.trx" ]; then
    hp trx "$LOGW\\\\gentests.trx" $GN_TRIM > "$LOGDIR/gentests.trim.txt" 2>&1
    gnoff=$(tr -d '\\r' < "$LOGDIR/gentests.trim.txt" | grep -a '^TRX test ' | grep -vE ': Passed(,Passed)*$' | sed 's/^TRX test //' | tr '\\n' ' ')
    gnn=$(tr -d '\\r' < "$LOGDIR/gentests.trim.txt" | grep -ac '^TRX test ')
    stamp "  GN trim guards (G's, by name): $gnn of $(echo $GN_TRIM | wc -w) read, not declared at HEAD [${gndecl% }], not Passed [${gnoff% }] (gentests.trim.txt); Total ${gntot:-unread} (EXPECT >= $GN_RESULTS_MIN)"
    [ -z "$gnoff$gndecl" ] && [ "$gnn" = "$(echo $GN_TRIM | wc -w)" ] || finding "GN trim guards: not declared [${gndecl% }], not Passed or NOT FOUND [${gnoff% }]"
  else
    stamp "  GN trim guards: NO TRX written -- the by-name reading is NOT MEASURED"
    finding "GN NO TRX: G's trim guards are NOT MEASURED"
  fi
""")
e.before("""# LEG TR -- N: r-module-driver-gomod-less edits the testing host itself""", """# LEG TP -- T3 (T3-7a): G's trim probes, the default-publish arm, against THIS union's src (claude/g-trim-probes is a
# MEASUREMENT REF and never lands: its programs and runner live in a detached worktree TP_WT made before launch; the
# union tree is never touched by it, only its build output under src/ is written, as every build leg's is). The runner
# needs only the .NET 10 SDK. Expected (G's README, COORD's gate 2026-10-09): every program publishes and runs, consumer
# RUNS-CLEAN, genprobe c32a c32b c32c GO-EQUAL; a failure here is a release blocker for any trim stage. The probe
# worktree must sit at TP_PIN (04239ec85e: G's CRLF fix, the runner strips CR on BOTH sides and .gitattributes pins
# expected.txt to LF), and origin's tip must still be TP_PIN; either moved is a finding. No CR workaround lives here.
# >>> tpread (tT3-controls.sh arm TP extracts the lines between the markers and runs them on planted rows)
tpread(){ # the runner's console log -> 'TP rows=<n> ok=<k> off=[...]'; rc 0 only for 5 default rows, each at its expectation
  tr -d '\\r' < "$1" | awk -F'\\t' 'NF == 6 && $2 == "default" { n++; want = ($1 == "consumer") ? "RUNS-CLEAN" : "GO-EQUAL"; if ($3 == "0" && $5 == want) ok++; else off = off $1 ":" $3 "/" $4 "/" $5 " " } END { printf "TP rows=%d ok=%d off=[%s]\\n", n, ok, off; exit !(n == 5 && ok == 5) }'
}
# <<< tpread
tptip=$(git ls-remote origin "refs/heads/$TP_REF" 2>/dev/null | cut -f1)
if [ ! -f "$TP_WT/src/tests/TrimProbes/trimprobes.sh" ]; then
  stamp "  TP: NOT MEASURED -- no $TP_WT/src/tests/TrimProbes/trimprobes.sh (make the probe worktree before launch: COORD-LAUNCH-CHECKLIST.md section 5)"
  finding "TP NOT MEASURED: no probe worktree at $TP_WT"
else
  tph=$(git -C "$TP_WT" rev-parse HEAD 2>/dev/null)
  case "$tph" in "$TP_PIN"*) ;; *) finding "TP: the probe worktree is at ${tph:0:10}, not the pin TP_PIN=$TP_PIN: remake it detached at the pin" ;; esac
  case "${tptip:-unread}" in "$TP_PIN"*) ;; *) finding "TP: origin $TP_REF is at ${tptip:-unread} (ls-remote), not the pin TP_PIN=$TP_PIN: G moved the ref; read the new commit(s) before TP is trusted" ;; esac
  stamp "  TP: probes at ${tph:0:10} (pin $TP_PIN; origin ${tptip:0:10}); tracked changes in the probe worktree: $(git -C "$TP_WT" status --porcelain --untracked-files=no | grep -c .) (EXPECT 0)"
  capped TP trimprobes "$TP_CAP" bash "$TP_WT/src/tests/TrimProbes/trimprobes.sh" "$W/src" default
  tpv=$(tpread "$LOGDIR/trimprobes.log"); tprc=$?
  stamp "  TP rows: $(tr -d '\\r' < "$LOGDIR/trimprobes.log" | awk -F'\\t' 'NF == 6 { printf "%s:%s/%s/%s/%s ", $1, $3, $4, $5, $6 }') :: $tpv (EXPECT rows=5 ok=5; runner rc $LEG_RC)"
  [ "$tprc" = 0 ] || finding "TP: $tpv (the publish logs and run outputs: $TP_WT/src/tests/TrimProbes/out-default-*)"
fi

""")
e.done()

# =============================================================================================================== selfcheck
e = Ed('tT3-selfcheck.sh')
e.after(SHEBANG, "# TRAIN T3 (2026-10-10): S7 counts TRAIN FL's names (the previous train); the derive's own two .py are parsed by S6.\n")
e.rep("""  c=$(grep -c -E '/tQ\\b|tQemit|trainQ|coord-trainQ-union|TRAIN Q\\b|tQ-' "$f"); [ "$c" = 0 ] || printf '    %-34s %s\\n' "$(basename "$f")" "$c\"""",
      """  c=$(grep -c -E '/tFL\\b|tFLemit|trainFL|coord-trainFL-union|TRAIN FL\\b|tFL-' "$f"); [ "$c" = 0 ] || printf '    %-34s %s\\n' "$(basename "$f")" "$c\"""")
e.done()

# ================================================================================================================ i9-te
e = Ed('tT3-i9-te.sh')
e.after(SHEBANG, "# TRAIN T3 (2026-10-10): the face lift's classes are OFF (T3-4): the i9 shard's patch is read with Q's classes; the\n# baseline is the i9's OWN TRAIN FL patch (coord-scratch/tFL/i9-patches/tFL-tracked-changes-U0.patch). FL's text follows.\n")
e.done()

print('EDITS applied')
