# TRAIN P: COORD's launch checklist, from the seat table to master AND THE RELEASE (DERIVED 2026-10-04 from TRAIN O's)

Git Bash on the i7. Every command matches `trainP/` as derived on 2026-10-04 (`tP-CHANGES.md`). State at the derive:
TRAIN O's battery RUNNING in `/h/go2cs-tmp-coord/tO` (bat2, launched 06:37); O lands as its union eb88ab9492 (+ a
possible fixup-2) + the bank step + the MS13 refresh, and **that landed master is P's BASE: it does not exist yet, so
every command below reads it into `$BASE` and no script carries it**. The draft seat list holds **22 rows**
(`rows-sha256=8491acbec194`: the table of COORD's 13:35 and 13:48 notes); it pre-maps with 0 conflicted rows on the
stand-in base; one seated sha owes an acceptance read (c2-ide-spike); **the one re-cut owed (C2: go-cmp L as
`claude/c2-reflect-newat-field-r2`, accepted in substance 14:05) was PUSHED by 14:58 as 92500eb260: it reads as expected
and trial pre-maps clean as row 23, and it is NOT seated until COORD accepts it at that sha**; two slots
are open (README MS21.4: the BOARD docs row, and L's -r2). No tP worktree, no union branch, no `tP/.battery.lock`; the follow list is EMPTY. One heavy job at a
time on this box (floors 1, 11): nothing below runs while O's battery or O's landing holds the box. A reading that
differs from an EXPECT line is a STOP. **Review round 1 (2026-10-04, `tP-CHANGES.md` section 8) is applied to every
command below.**

**P is THE RELEASE TRAIN for go.\* 1.24.13.4** (owner, 2026-10-04 10:05: "OK to extend 1.24.13.4 for best release
yet"): sections 5b (the release gate, two CI runs) and 7 (the release) are part of this checklist.

```
# A FRESH shell (review round 1): a value left exported from O's shell is read by P's scripts as COORD's explicit call
# (REGEN_ALLOW skips the composition; SEATS, LIVE_ACK, DEADLINE, E_BISECT_SEATS likewise). W is refused by the fixup and
# the emission check unless it names tP's worktree, and N_RUN is refused by the battery; the others are not.
env | grep -E '^(W|SEATS|REGEN_ALLOW|REGEN|GOLDENS|LIVE_ACK|N_RUN|O_RUN|E_BISECT_SEATS|DEADLINE|PRECHECK_MODE|FIXUP_N|EXPECT_HEAD|BASE|MASTER)='   # EXPECT no line
unset W
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainP
X=/h/go2cs-tmp-coord/coord-scratch/tP
R=$X/run1                                   # a FRESH run folder: the map, the assembly, the fixup AND the battery run from copies here
hp(){ python -B "$(cygpath -w "$R/tP-helpers.py")" "$@"; }
rh(){ grep -E '^[A-Za-z0-9._-]+\|' "$1" | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12; }   # a list's rows-sha256 (the assembler's and the map's hash)
git -C /h/Projects/go2cs fetch -q origin
BASE=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master | cut -f1)   # READ, never typed (floor 15): TRAIN O's landed master
echo "BASE=$BASE"; git -C /h/Projects/go2cs log -1 --format='%h %s' "$BASE"   # EXPECT O's landing commit (the MS13 refresh or O's fast-forward head), NOT eb88ab9492 and NOT 54f7f4439d
# ASSERTS, not a human read of the subject above (a stale base on origin/master passes every other check):
git -C /h/Projects/go2cs merge-base --is-ancestor eb88ab9492 "$BASE" && echo "O's union is in BASE"   # EXPECT the line
git -C /h/Projects/go2cs log --first-parent --format=%s eb88ab9492.."$BASE" | grep -c '^refresh: TRAIN O'   # EXPECT 1 (O's MS13 refresh is IN the base)
git -C /h/Projects/go2cs log --first-parent --format='%h %s' eb88ab9492.."$BASE" | grep -E '^[0-9a-f]+ fixup-[0-9]+: TRAIN O' | cut -c1-120   # a READING: O's fixup-N commits, if its battery needed one (each line is a commit every P seat cut on eb88ab9492 lacks: the map reads them)
git -C /h/Projects/go2cs log --first-parent --format='%h %s' "$BASE"..origin/master | grep -cE '^[0-9a-f]+ (Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )'   # EXPECT 0 (the assembler and the map refuse otherwise)
# The shared object store (COORD's 13:07 flag: about 7,100 loose objects, past gc.auto 6,700; the remedy waits for O's landing
# and is never run beside a battery). NOW is that point: rule it (a prune, or gc.auto 0 for the assembly) before section 1.
git -C /h/Projects/go2cs count-objects -v | grep -E '^(count|in-pack|packs|prune-packable|garbage):'   # a READING; EXPECT count below 6700 once the remedy has run
# The release fix's control at the base (the defect is IN O's master; the fix is a P row):
git -C /h/Projects/go2cs show "$BASE:src/core/sort/sort.cs" | tr -d '\r' | grep -cE '^[[:space:]]*Sort\(x\);[[:space:]]*$'   # EXPECT 3
```

## 0. The seat table (minutes; nothing is created)

Before the table (README MS21): **the one owed acceptance is ruled** (c2-ide-spike a4bfed39f5; the else-if -r3, the
i9's crash-verdict row, C1's sort arm, P2's warning entries, G's go.go -r2 and G's x/sync bank were accepted 12:37 to
13:48); **each slot is a pushed, read-back, accepted row or is struck** (COORD's P BOARD docs row cut on `$BASE`;
C2's go-cmp L: `claude/c2-reflect-newat-field` dec8cee4b4 **conflicts with the last row as cut**; ACCEPTED IN
SUBSTANCE 14:05 with **ONE RE-CUT OWED (C2)**: `claude/c2-reflect-newat-field-r2` stacked on
c2-elseif-position-record-r3 e00629855f, placed last, due by the freeze. EXPECT at the -r2 (COORD's post cb0a997dcb):
exactly one line of `reflect/package_info.cs` differs from dec8cee4b4's corpus commit, `reflect/value.cs` unchanged;
go test, CNR, ReflectNewAtField and GolibTests' five re-read at its tip; the runtime and net/http canary lines
posted. **PUSHED by 14:58: 92500eb260.** Read with git: MET (README MS21.4 (iii)); trial pre-map as row 23:
`conflicted-rows=0` (`premap-run3/trial-L-r2`). To seat it once the notes say P ACCEPTED at that sha: in
`$HND/tP-seats-draft.txt` remove the leading `# ` of the line `# c2-reflect-newat-field-r2|92500eb260|...` and
delete the SLOT comment above it; EXPECT `<n>` = 23 and `<h>` = `a9b5da13f551` (with the BOARD row seated too,
both move again). README Q16 before it is seated); and
COORD's table edits are in `$HND/tP-seats-draft.txt` (each new or re-pointed row's sha, its `stack-on` / `after`
tokens, its `CORPUS FOOTPRINT n files (...)` group when it commits corpus, **its paths written RELATIVE to src/core:
the fixup refuses a token that starts `src/core/` and dies in PRE on a token that is no file at HEAD**). Every row
edit moves `<n>` and `<h>` (a notes-only edit moves neither). A new
ref always pushes, is read back with `ls-remote`, THEN is announced. The run folder takes the SCRIPTS once and the
LIST on EVERY pass (section 0 is re-run as rows land; a list copied once goes stale, and every later step reads the
run copy):
```
mkdir -p $R && { [ -e $R/tP-assemble.sh ] || { cp $HND/*.sh $HND/*.py $HND/*.ps1 $HND/*.txt $HND/*.md $R/ && cp -r $HND/controls $R/; }; }   # the script set: ONCE (never the premap-run* / step1-run1 folders: run output and scratch object directories)
cp $HND/tP-seats-draft.txt $HND/tP-follow.txt $R/                     # the LIST: on EVERY pass
for f in $HND/*.sh $HND/*.py; do cmp -s "$f" "$R/$(basename "$f")" || echo "STOP: $(basename "$f") changed in HND since $R was made: make a NEW run folder"; done   # EXPECT no line
echo "rows HND=$(rh $HND/tP-seats-draft.txt) RUN=$(rh $R/tP-seats-draft.txt)"; cmp -s $HND/tP-seats-draft.txt $R/tP-seats-draft.txt || echo "STOP: the run copy's list differs"   # EXPECT the two equal, no STOP
# The pre-map at BASE (read-only: every merge is a git merge-tree in a scratch object directory; 6 and 12 minutes for the derive's two runs, beside O's battery):
( cd $R && BASE=$BASE ROWS=$R/tP-seats-draft.txt OUT=$X/premap-base bash ./tP-premap-all.sh ) | tail -n 6; echo rc=${PIPESTATUS[0]}
#  EXPECT  ALONE DONE rows=<n> not-clean=0
#          CHAIN DONE rows=<n> conflicted-steps=0 onconflict=union ...   and the same with onconflict=skip
#          PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0 ...   rc=0
#  (the derive's run on this list at the stand-in base read exactly these lines: premap-run2/v8-eb88, tP-DERIVE-REPORT.md
#   section 3. Any conflicted row, a regcheck FAIL or a parse error = STOP: route it back to its seat.)
#  READ $X/premap-base/unioncheck.log: HashSet census 0 (MS19), H4 0 (MS2), the registration files at expect (MS14),
#   and (review round 1) 'C GODECL scan control: ok' above 'C GODECL package main: ... NEW at head=0': the duplicate-declaration
#   census reads declarations by the battery's lexical scan now (the stand-in base read base=3616 head=3708; O's backtick
#   parity read 3455 / 3533 and could not see a duplicate in convCallExpr.go or testConversion.go). 'CONTROL FAILED' there = STOP.
#  The pre-map needs GITX to be a git >= 2.44 (it aborts by name otherwise); the default is the Visual Studio git 2.55.
( cd $R && BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 bash ./tP-assemble.sh ) | tail -n 3; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n> rows (tP-seats-draft.txt) rows-sha256=<h> ...   then   TABLE_ONLY=1: stopping ...   rc=0
#          An 'ABORT: BASE ... is BEHIND a landed train' line or a 'carries <m> train-assembly commit(s)' line means BASE is
#          not O's landed master, or a row was cut above it: STOP.
echo "HND=$(rh $HND/tP-seats-draft.txt) RUN=$(rh $R/tP-seats-draft.txt)"   # both = <h>
```
Then MS9c's five table controls (README MS9c), once, each with `TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<a planted copy>`.

## 1. The draft into hnd (the lanes read it from origin), the map, the assembly

```
ls -a $HND | grep -c __pycache__                                         # EXPECT 0
git -C /h/go2cs-tmp-coord/hnd add -f -n .claude/coord-scripts/trainP  # DRY RUN: read every path it would add (.claude/ is excluded: plain status lists nothing)
#  EXPECT  only trainP/*.sh *.py *.md *.txt *.ps1, controls/*; premap-run1/, premap-run2/ and step1-run1/ hold RUN OUTPUT
#          (scratch object directories among it): leave them out by naming the files instead of the folder, unless COORD wants the logs
#  REVIEW ROUND 1: the add line is CWD-INDEPENDENT. The earlier line left its '*' to the shell, which expands it only when the
#  shell's cwd is hnd; from any other cwd the globs reach git literally, git's own '*' crosses '/', and the run output under
#  premap-run*/ and step1-run1/ is staged too (measured with a dry run: 217 paths instead of 36) and then pushed on claude/coord-handover.
#  ':(glob)' stops '*' at '/', so the same six pathspecs mean the same files from every cwd.
git -C /h/go2cs-tmp-coord/hnd add -f -- ':(glob).claude/coord-scripts/trainP/*.sh' ':(glob).claude/coord-scripts/trainP/*.py' ':(glob).claude/coord-scripts/trainP/*.md' ':(glob).claude/coord-scripts/trainP/*.txt' ':(glob).claude/coord-scripts/trainP/*.ps1' .claude/coord-scripts/trainP/controls
want=$(ls $HND/*.sh $HND/*.py $HND/*.md $HND/*.txt $HND/*.ps1 $HND/controls/* | wc -l); got=$(git -C /h/go2cs-tmp-coord/hnd diff --cached --name-only -- .claude/coord-scripts/trainP | grep -c .); echo "staged=$got want=$want"
#  EXPECT  staged = want (36 at review round 1: 15 .sh, 5 .py, 7 .md, 5 .txt, 1 .ps1, 3 under controls/)
git -C /h/go2cs-tmp-coord/hnd diff --cached --name-only | grep -cE 'trainP/(premap-run|step1-run)'   # EXPECT 0 (no run-output path is staged)
git -C /h/go2cs-tmp-coord/hnd status --porcelain | grep -v '^A  \.claude/coord-scripts/trainP/'       # EXPECT only what COORD means to leave out of this commit (the notes file ' M .../trainL/tL-seats-draft.txt' rides its own commit)
git -C /h/go2cs-tmp-coord/hnd commit -S -m "coord-scripts: TRAIN P script set (derived from TRAIN O's)"
git -C /h/go2cs-tmp-coord/hnd log -1 --format='%G? %GK'                  # EXPECT G or U and 941694536F21BAFF. ITS OWN COMMAND: read the line, THEN type the push
git -C /h/go2cs-tmp-coord/hnd push origin claude/coord-handover          # an EXISTING ref: announce, then push (floor 9)
cmp -s $HND/tP-seats-draft.txt $R/tP-seats-draft.txt && echo "rows RUN=$(rh $R/tP-seats-draft.txt) COMMITTED=$(git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainP/tP-seats-draft.txt | grep -E '^[A-Za-z0-9._-]+\|' | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12)"   # EXPECT the line, both = <h> (the lanes read the COMMITTED list)
( cd $R && BASE=$BASE SEATS=$R/tP-seats-draft.txt bash ./tP-conflict-map.sh ) | tail -n 4
#  EXPECT  MAP base <BASE10> ...  MAP DONE head=<map10> rows=<n> rows-sha256=<h> (every row clean: kept as refs/coord/tP-map/rows-<h> ...)
#          A 'SEAT k <ref>: REFUSED: carries <m> train-assembly commit(s)' line, or an 'ABORT: BASE ... is BEHIND a landed
#          train' line, means BASE is not O's landed master: STOP.
#  READ every 'shared paths' list of a clean row that is not a registration file (README MS1; tP-premap.md names them)
( cd $R && BASE=$BASE bash ./tP-assemble.sh ) | tail -n 8
#  EXPECT  ASSEMBLED head=<asm10> seats=<n>; MAP TREE: EQUAL to <map10>; ANCESTRY; SHAPE: <n> first-parent merges; ORDER; TREE CLEAN; NEXT: ...
git -C /h/go2cs-tmp-coord/tP rev-parse --short=10 HEAD                   # <ASSEMBLED>
```
`grep -vE '^[[:space:]]*(#|$)' $R/tP-follow.txt | tr -d '\r' | sha256sum | cut -c1-12` reads `e3b0c44298fc` (EMPTY).

## 2. Controls, once, before the fixup (floor 13; read-only except 2e, which aborts before creating anything)
```
hp teattr    "$(cygpath -w $R/controls/te-pos-goframe.patch)"      # EXPECT TE files=3 hunks=5 g-frame=4 (... g-frame-only files=2) map-only=1 other=0
hp hunkclass "$(cygpath -w $R/controls/te-pos-goframe.patch)"; echo rc=$?    # EXPECT HUNKCLASS-VERDICT files=3 hunks=5 other-files=0 ... rc=0
hp teattr    "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"   # EXPECT TE files=1 hunks=25 g-frame=0 (...) map-only=0 other=25
hp hunkclass "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"; echo rc=$? # EXPECT ... other-files=1 ... rc=1
hp cnrexpect 'H:\go2cs-tmp-coord\tP' "$BASE" --base eb88ab9492 | tail -n 2   # EXPECT CNRDELTA ... added=0 removed=0; CNREXPECT ... n=819 (O's battery CNR line: O's bank and MS13 add no behavioral directory)
hp cnrexpect 'H:\go2cs-tmp-coord\tP' HEAD --base "$BASE" | tail -n 2         # EXPECT CNRDELTA ... added=16 removed=1; CNREXPECT ... n=834 (at the 22-row draft; a slot that adds a project moves it)
# 2b. precheck on the assembled union (the fixup's PRERES, predicted: README MS9b.1)
hp precheck 'H:\go2cs-tmp-coord\tP' "$BASE" "$(cygpath -w $R/tP-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +(COUNT|REG|ROSTER|H3|H4|S1))'; echo rc=${PIPESTATUS[0]}
#  EXPECT  rc=0; 'ok COUNT' x6, no identical-insert credit and no 'COUNTBASE multi-base row' line;
#          'ok REG src/go2cs.slnx: ... seat-removed=[c2-sibling-package-name-r2:tests/Behavioral/AliasNamespaceShadow/sortlocal/AliasNamespaceShadow.sortlocal.csproj]';
#          'ok ROSTER rows base=225 merged=225'; 'ok ROSTER linux annotations ... none';
#          'ok ROSTER darwin annotations base=22 merged=22 expect=22 (the base + 0 row-own edit(s): none) mismatched: none';
#          'ok H3 ... [[System.Diagnostics.DebuggerNonUserCode]]=1 (want 1, g-debugger-views)' beside the four master identifiers;
#          'ok H4 ... 0'; 'ok S1 census'; PRECHECK hard-failures=0 notes=0 mode=head.
# 2c. the same reader made to FAIL: (i) on the base: a checkout whose HEAD is $BASE (a fresh child worktree; remove it after with git worktree remove)
#  EXPECT  one 'FAIL BOTH <seat>: no first-parent merge' per row, FAIL COUNT / REG on the registration files the seats add to,
#          and 'FAIL H3 ... [[System.Diagnostics.DebuggerNonUserCode]]=0 (want 1, g-debugger-views)' (the H3 entry's negative control);
#      (ii) the H3 arm's OTHER refusal: a COPY of the helper with H3_TOKENS = {} run on the assembled union:
#  EXPECT  'FAIL H3 seat g-debugger-views edits ... holds no identifier for it', rc 1
# 2d. the LIVE gate
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock
hp live 'H:\go2cs-tmp-coord\coord-scratch\tP\.battery.lock' 1; echo rc=$?     # EXPECT one 'LIVE lock ...tZ...' line, rc=1
rmdir /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock /h/go2cs-tmp-coord/coord-scratch/tZ
hp live 'H:\go2cs-tmp-coord\coord-scratch\tP\.battery.lock' 1; echo rc=$?     # EXPECT 'LIVE-VERDICT none ...', rc=0 (an idle box: O's lock gone)
```
2e. the assembly's map refusal (safe only now, with every row merged and no fixup): a copy of the list without its LAST
row (c2-elseif-position-record-r3 at the draft: no row stacks on it; if the last row changed, drop THAT one):
```
grep -v '^c2-elseif-position-record-r3|' $R/tP-seats-draft.txt > $R/c4-seats.txt
( cd $R && BASE=$BASE SIGN_PROBE=0 SEATS=$R/c4-seats.txt bash ./tP-assemble.sh ) | tail -n 2; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n-1> rows (c4-seats.txt) ...  then  ABORT: no map run is recorded for THIS list (...). ...   rc=2
git -C /h/go2cs-tmp-coord/tP rev-parse --short=10 HEAD                        # EXPECT <ASSEMBLED> still
```

## 3. The fixup (INFERRED about 1 h 35 m to its first stop: the emission check is most of it; takes the lock)
Values:
- `BASE=$BASE`, `EXPECT_HEAD=<ASSEMBLED>`. No `PROSE_PATCH`.
- **`CSPROJ_TEMPLATE=accept`** (COORD's 11:28 confirmation for `LiteralFloatConstFold.csproj`: README MS20).
- `EXP_DESC`, `EXP_TH`: TO BE SET BY COORD, or unset (reported). Read-only derivation:
  `python "$(cygpath -w /h/go2cs-tmp-coord/tP/docs/phase4/recipes/quickjit-optin/optin.py)" check 'H:\go2cs-tmp-coord\tP' | tail -n 2`.
- Defaults: `REGEN=apply GOLDENS=regen SIBLINGS=report GOLDEN_CLASS=gframe EDITORCONFIG_NEW=stop FIXUP_N=1`;
  `REGEN_ALLOW` composed (the six footprint rows); `EXEC_ROWS_EXPECT` from the helper (0). `EDITORCONFIG_NEW=accept`
  only by COORD's ruling (MS22).
- MS19: warm the module cache first: `( cd /h/go2cs-tmp-coord/tP/src/go2cs && go mod download ); git -C /h/go2cs-tmp-coord/tP status --porcelain | grep -vc '^??'` (EXPECT rc 0, then 0).
```
cmp -s $HND/tP-seats-draft.txt $R/tP-seats-draft.txt && [ "$(rh $R/tP-seats-draft.txt)" = <h> ] && echo "list = the mapped and assembled one"   # EXPECT the line
cd $R && BASE=$BASE EXPECT_HEAD=<ASSEMBLED> SEATS_EXPECTED=<n> CSPROJ_TEMPLATE=accept bash ./tP-fixup.sh > fixup.console.log 2>&1; echo "fixup rc=$?" >> fixup.console.log
```
EXPECT, in order (read `$R/tP-fixup-logs/SUMMARY.txt`):
- `PRE head=<ASSEMBLED> seats=<n> ... CSPROJ_TEMPLATE=accept EDITORCONFIG_NEW=... EXEC_ROWS_EXPECT=0 PROSE_PATCH=none lists=rows:<h>/follow:<f>`
- `PRE derived (R1): REGEN_ALLOW=/^src/core/(...)$/ :: composed from the seat list: g-sort-self-capture:sort/sort.cs c2-sibling-package-name-r2:... (4) c2-literal-float-fold:... (1) p2-test-warning-entries:... (2) g-method-value-fm-record-r3:... (11) c2-elseif-position-record-r3:... (123)` (142 path tokens, 133 distinct, at the draft), then
  `PRE derived (R1): REGEN_ALLOW composition: 142 footprint token(s), 133 distinct, from 6 row(s); none written with a
  src/core/ prefix, every one a file at HEAD (review round 1)`. An `ABORT: <k> footprint token(s) ... written with a
  'src/core/' prefix` at launch, or a die `REGEN_ALLOW: <k> of <n> footprint token(s) ... are no file at HEAD` in PRE,
  names the row and the token: correct that row's group in the list (it happens BEFORE the emission check, so it costs
  seconds; as first drafted the else-if row's 123 tokens carried the prefix and 114 of its files would have been
  refused at step 4)
- `PRERES precheck PRECHECK hard-failures=0 notes=0 mode=worktree`; the roster guard x2 with `0 with an execution config`
- **`PRERES release fix (go.sort self-recursion): src/core/sort/sort.cs holds 0 bare 'Sort(x);' line(s) (EXPECT 0) and 3
  of the three .Sort() methods (EXPECT 3); the reader's control at the base: 3 (EXPECT 3, the published defect; ...)`**
  (P9). A die here (`THE RELEASE FIX is not in the tree (PRERES)`) means the sort seat is not in the union, or a merge
  lost its lines: STOP, nothing was written
- `4 regen`: PREDICTED nothing to apply (every footprint is committed in-seat). A listed path inside REGEN_ALLOW is
  regenerated: READ `regen.patch` (and FIRST of all if it lists `src/core/sort/sort.cs`); a listed path outside it
  STOPS (MS4). c2-named-basic-conv's footprint is measured HERE for the first time (a census predicted 0). A `REGEN
  REFUSED ... emitted by <k> of the targets that carry it` line is rule 4 under incremental writes (P10): read the path
  on each target before any relaunch
- `5 csproj CNR moved: 1 :: CSPROJTEMPLATE-VERDICT files=1 template-only=1 other=0 ... + the template file delta at 1 cut(s)`, carried under `CSPROJ_TEMPLATE=accept`; `other` above 0 STOPS
- `5 .editorconfig NEW ...` only if the union CNR wrote one (MS22): a STOP unless `EDITORCONFIG_NEW=accept`
- `5 goldens ...`; a moved golden STOPS at its class (`GOLDEN_CLASS=gframe`): READ `gold-cs.patch`, then relaunch with `GOLDEN_CLASS=any` (MS4b; O needed it for two goldens)
- `6 release fix (go.sort self-recursion): ... holds 0 ... and 3 of the three ...` (the same predicate, read again on the tree the fixup is about to commit), then `6 purge: purged=<k> remaining=0 retries=<k> tracked-deletions=0` (a handle still open is retried 6 x 5 s before the die: O's BATTERY STOP 1)
- last: `FIXUP DONE head=<10 hex> subject='fixup: TRAIN P' lists=rows:<h>/follow:<f>`, rc 0.

## 4. After the fixup: the signature by KEY as its OWN command, THEN the push, THEN the read-back, THEN the announcement and the GOs
Each of the next three lines is typed, run and READ by itself. The push line is typed only after the signature lines
read as expected (at N the check was composed into the push's command, so its verdict could not have stopped the push):
```
git -C /h/go2cs-tmp-coord/tP log -1 --format='%H %G? %GK %s' | cut -c1-140
#  EXPECT  <40 hex> <G or U> 941694536F21BAFF fixup: TRAIN P -- ...
git -C /h/go2cs-tmp-coord/tP log --first-parent --format='%G? %GK' "$BASE"..HEAD | sort | uniq -c
#  EXPECT ONE line '<n+1> <G|U> 941694536F21BAFF' (--first-parent: without it the range takes the seats' own commits,
#  which lanes leave unsigned or ssh-signed). Any other line, a second line, or an 'N' / 'B' / 'E' status = STOP: no push.
git -C /h/go2cs-tmp-coord/tP status --porcelain | grep -vc '^??'              # EXPECT 0
```
Only then:
```
UNION=$(git -C /h/go2cs-tmp-coord/tP rev-parse HEAD)                          # the GO's UNION and FIXUP (one sha)
git -C /h/go2cs-tmp-coord/tP push origin claude/coord-trainP-union            # a NEW ref: PUSH first
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainP-union # READ BACK: EXPECT exactly $UNION
#  THEN ANNOUNCE, quoting the read-back line (mailbox skill). The order for a NEW ref is push, read back, announce: an
#  announcement made first names a ref no lane can fetch. The sha is never replaced afterwards (floor 9): a correction is fixup-2 on top.
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainP/tP-i9-shard.txt | sha256sum | cut -d' ' -f1   # EXPECT 057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d (O's, N's, M's and L's bytes)
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainP/tP-i9-shard.txt | grep -c .                   # EXPECT 132
git -C /h/go2cs-tmp-coord/hnd rev-parse --short=10 HEAD                       # the briefs' <HND> value
git -C /h/Projects/go2cs log -1 --format=%h -S'execution: release-tiered' "$BASE" -- docs/ValidatedTestPackages.md   # EXPECT 2d46eba8f0
#   another sha = a landing changed the phrase's count: the lane drivers then WALK back (at most 12 commits) to the first
#   whose parent annotates log/slog and stamp the walk; tell the lanes their PRE line will name 2d46eba8f0 after a walk
```
GO to the i9 (`tP-lane-brief-i9.md`): `UNION`, `FIXUP`, **`BASE`** (the driver requires it), `EXPECT_LIST_SHA`,
`EXPECT_ROWS=132`, the handover tip; FOUR files in its run folder; **the sort row is the release fix's row there, and
the i7 sweeps it too as an X row** (state it, as O's GO did for the canaries). GO to P1 and P2
(`tP-lane-brief-linux.md`): `UNION`, `FIXUP`, **`BASE`**, the handover tip; three files. **Tell C1 the union is pushed:
section 5b's two dispatches start now.** Tell G: the sort row sweep and the pflag / cobra re-read at `$UNION` (the
release fix's acceptance, README MS23 (d)), and the x/sync re-read (28/28) its bank row owes (COORD 13:48). OWED too once
the union is pushed: a docs-site build at the union (g-xsync-bank's pages under `docs/validation/modules`, an `@` in
the path).

## 5. The battery (INFERRED 11 to 12 h, as README step 6)
Preflight: `ls -d $X/.battery.lock` (absent); `ls $R/tP-logs $R/mod-logs` (absent); free disk at least 30 G on H: and
8 G on C:; nothing else converting or building on the box (the LIVE gate reads it; O's battery and landing are DONE).
Values: `MASTER=$BASE`; `EXPECT_HEAD` = the first 10 chars of `$UNION`; `O_RUN` = O's battery of record (default
`coord-scratch/tO/bat2`; give the re-run's folder if O re-ran; **`N_RUN` must be UNSET: the script refuses it**).
`CNR_EXPECT_N`: leave UNSET (PRE-D derives it). `I9_PATCH` when the i9's patch has arrived;
`I9_BASELINE=/h/go2cs-tmp-coord/coord-scratch/tO/i9-patches/tO-tracked-changes-U0.patch` (present at the derive).
`E_BISECT_SEATS`: leave UNSET (PRE-D derives the footprint converter rows from the list: five at the draft, about
46 m an arm, run only if leg E reads above 0); a shorter list or `none` by COORD's call (README Q9): a name that is no
row is a FINDING, exit 6. **DECIDE AT LAUNCH (review round 1): five arms are about 3 h 50 m, and they run by
themselves right after leg E when it reads above 0. With the 11 to 12 h estimate that passes the default `DEADLINE`
(launch + 14 h): the battery would then stop at exit 9 before the sweeps finish (O's default was two arms).** Either
pass `DEADLINE='<YYYY-MM-DD HH:MM>'` at about launch + 18 h on the launch line, or cut the arms with
`E_BISECT_SEATS='g-sort-self-capture c2-elseif-position-record-r3'` (the two whose files the fixup may regenerate).
PRE-D stamps the arithmetic either way (`PRE-D E-bisect NOTE: 5 arm(s) at about 46 m each = about 230 m IF leg E
reads above 0, against DEADLINE=...`).
```
cd $R && MASTER=$BASE EXPECT_HEAD=${UNION:0:10} SEATS_EXPECTED=<n> bash ./tP-battery.sh > battery.console.log 2>&1; echo "battery rc=$?" >> battery.console.log
```
EXPECT within the first minutes:
- `PRE union shape OK: first-parent=<n + 1> (= <n> seat merges + 0 follow-up merge(s) + 1 fixup commit(s) ...)`
- `PRE head=<10 hex> ... master=<BASE> (<BASE10>) previous-train record (TRAIN O) O_RUN=.../tO/bat2 (SUMMARY <k> lines) seats=<n> ...`
- `PRE-D CB: derived name(s) that are NOT tests (no declaration in code at HEAD: ...), dropped: [TestString]`, then `PRE-D CB: 35 test(s) the union adds under src/go2cs + 6 base tree guards = 41 names ...` (a slot that adds a test moves it). A `FINDING: PRE-D CB_SCAN: ...` line means the name scan's control misread on this box's awk: nothing was filtered and `TestString` will read not-PASS in CB (P13)
- `PRE-D CNR: EXPECT N=834 (derived (cnrexpect at HEAD)); ... the reader's control at <BASE>: N=819 vs TRAIN O's battery 819 (...); reconciliation: 819 + 16 measurable added - 1 removed = 834 ...` (a re-routed list moves these, never by hand)
- `PRE-D behavioral: projects the union adds=[...] (8; slnx agrees: yes ...)`
- `PRE-D execution configs ...: rows that keep an annotation (EXPECT tiered=True)=[] rows that drop it (EXPECT tiered=False)=[]; EXEC_ROWS_EXPECT=0`
- `PRE-D E-bisect: seats whose merge changes the converter=[...] (15); footprint rows (a CORPUS FOOTPRINT in the notes)=[g-sort-self-capture c2-sibling-package-name-r2 c2-literal-float-fold p2-test-warning-entries g-method-value-fm-record-r3 c2-elseif-position-record-r3], of them not converter seats (not bisectable)=[p2-test-warning-entries]; bisected if leg E reads above 0: [g-sort-self-capture c2-sibling-package-name-r2 c2-literal-float-fold g-method-value-fm-record-r3 c2-elseif-position-record-r3] (derived: the list's CORPUS FOOTPRINT rows that are converter seats; about 46 m an arm); converter seats NOT in that list ...: [the other ten]`
- `PRE-D UF known class (P6): [src/core/math/bits/.editorconfig src/core/weak/.editorconfig]; the row that commits them: p2-test-warning-entries; tracked at HEAD: [src/core/math/bits/.editorconfig src/core/weak/.editorconfig]` (then UF and UF-T EXPECT 0 untracked). WITHOUT that row the line reads `the row that commits them: NOT A ROW (...)`, and UF reads `KNOWN class (...)=1 [src/core/math/bits/.editorconfig]; other=0 (EXPECT 0)` on the i7 (the i9's is `src/core/weak/.editorconfig`); any OTHER path is a finding
- **`PRE-D release fix (go.sort self-recursion, P9): src/core/sort/sort.cs at HEAD holds 0 bare 'Sort(x);' line(s) (EXPECT 0) and 3 of the three .Sort() methods (EXPECT 3); the reader's control at <BASE>: 3 (EXPECT 3, the published defect; 0 = the base already holds the fix)`**; any other reading is `FINDING: PRE-D RELFIX: ...`
- `PRE-1 precheck rc=0 :: PRECHECK hard-failures=0 notes=0 mode=head` and the H3 line (2b's)
- later: `NGa51:selfdescription ... ran 16, failed 0` and `NGa7:selfdescription ... ran 16, failed 0`; `IDC (...): rc=0 :: SELF-TEST: pass=<>=217> fail=0`;
  `PUB (...): rc=0 :: published-output gate: 6 of 6 published programs match go, none hung`; `4 asserts: N=834 (EXPECT 834, ...)`;
  the isolated `SortMethodSelfCapture` reading in leg 5's B: lines; `WE (...): rc=0 final='PASS'`; `WEc (...): rc=0 caught=1 :: CONTROL FIRED`;
  `S:sort` at its banked count with the three named tests PASS
STOP conditions (the exit status): **2** an ABORT in PRE (a `MASTER ... is not on the union's first-parent line` ABORT
means the wrong base was given; `N_RUN is set` means O's variable name was used); **3** a disk or build abort (after
the purge's six retries); **5** a wall cap fired (PUB 30m, WE 150m, WEc 45m among them) OR a process is still running
from `$R/out` after MOD: the lock is KEPT, kill the listed PIDs by PID (never by name: floor 5), `rmdir
$X/.battery.lock`, relaunch from a FRESH folder; **9** the deadline; **6** at END = a leg outside FXc1 / FXc2 / SIc /
NVR is non-zero or a FINDING exists; **0** clean. A red needing a tree change after the push is a `fixup-2` ON TOP
(`FIXUP_N=2`), never a replaced sha; **a fixup-N on a release path owes 5b again** (README 4b).

## 5b. THE RELEASE GATE (MS23): two CI runs at the P union, dispatched by C1, read by COORD BEFORE landing
The i7 battery runs no `release-smoke.ps1`, no per-flavour pack, no walkthrough arm D and nothing on darwin (README
MS23). Once the union is pushed and read back (they run on hosted runners beside the battery):
```
gh workflow run os-matrix.yml --ref claude/coord-trainP-union -f goos=windows -f stage=release-smoke
gh workflow run os-matrix.yml --ref claude/coord-trainP-union -f goos=darwin  -f stage=behavioral-full
gh run list --workflow os-matrix.yml --branch claude/coord-trainP-union --limit 4    # the two run ids; check each run's head sha = $UNION
gh run watch <run id>; gh run view <run id> --log | grep -E 'release-smoke|exit `' | tail -n 20
```
(The `-f` names are O's as COORD's 5b ran them; C1 owns the workflow: if it renamed an input, C1's dispatch line leads.)
EXPECT, release-smoke: the pack job green (344 packages, 0 failed, as O's run 37193550625 at eb88ab9492; a count that
moved is READ against the rows that add a packable project: none is known) and **on EVERY shipped RID (win-x64,
linux-x64, osx-x64, osx-arm64) A, B, C and D PASS, with D GATING on all four**. **Arm B carries the sort arm** (the
three `.Sort()` forms and the bare form, c1-release-smoke-sort-arm): its control run 37212642354 (O's union + the arm,
no fix) read B FAIL '(a STACK OVERFLOW)' on all four RIDs, so B green here is the release fix read from the PACKED
go.sort. EXPECT, darwin behavioral FULL: every measurable project passes on arm64 and on x64 (O: 766/766 and 767/767,
run 37193552307), the 8 new projects among them, `SortMethodSelfCapture` by name.
A red in either run HOLDS the landing. **The sort fix's acceptance is five readings** (README MS23 (a) to (e)): arm B on
four RIDs; `SortMethodSelfCapture` on the i7, linux and both Macs; the `sort` row on the i9 and as the i7's X row; G's
sort sweep and pflag / cobra re-read at the union; the scripts' tree assertion (P9).

## 6. Landing (README MS11, then MS13, MS26), after the battery, the three lanes' readings, 5b and G's re-read
1. Merge master into the union if it moved; `git diff --stat $BASE <master>`, read whole (MS11). A commit that touches a
   release path (README 4b's release column) owes 5b again at the new head.
2. The landing precheck (`hp precheck <tP> <master> <run>/tP-logs/seats-effective.txt head`) and the roster guard x2 with
   `EXEC_ROWS_EXPECT=0`: BEFORE the bank step.
3. The bank step (as at O).
4. **MS13, the refresh of committed -tests sources** (a bank step, never a leg): the tree back at HEAD, then
   `hp testsrc-refresh 'H:\go2cs-tmp-coord\tP' --out <run>\tP-logs\testsrc-refresh.patch <run>\tP-logs\S-rewrites.patch <run>\tP-logs\T-rewrites.patch <the i9's tracked-changes.patch>`
   (EXPECT `REFRESH-VERDICT ... refused=0 conflicts=0`), `git apply --check` then `git apply`, then
   `hp testsrc-refresh 'H:\go2cs-tmp-coord\tP' --check-worktree` (EXPECT rc 0, `ONLY committed test sources changed`; in the
   no-row UF case the untracked `src/core/math/bits/.editorconfig` is the known class, not a refusal to chase), then
   ONE signed commit `refresh: TRAIN P -- committed -tests sources from the windows re-emission (MS13)`, its signature
   read by KEY as its own command, as in section 4.
5. MS26, by COORD's ruling (README Q4): the C# consumer runner at the union, after the battery released the box:
   `pwsh src/tests/CSharpConsumer/run-csharp-consumer.ps1 -WorkRoot <a fresh folder outside the repo>` (EXPECT exit 0,
   `checks: 21 ok, 0 failed`; it downloads two modules and converts them: one conversion job, floor 1).
6. Fast-forward master to the union (an EXISTING ref: announce, then push); the ledger line; the FLEET post.

## 7. THE RELEASE of go.\* 1.24.13.4 (README MS27): after the landing, from P's LANDED master
Not a script of this set; the release runbook leads. The preconditions this checklist can state:
```
git -C /h/Projects/go2cs fetch -q origin; REL=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master | cut -f1); echo "REL=$REL"
git -C /h/Projects/go2cs merge-base --is-ancestor "$UNION" "$REL" && echo "P's union is in the release base"                 # EXPECT the line
git -C /h/Projects/go2cs log --first-parent --format=%s "$UNION".."$REL" | grep -c '^refresh: TRAIN P'                      # EXPECT 1
git -C /h/Projects/go2cs show "$REL:src/core/sort/sort.cs" | tr -d '\r' | grep -cE '^[[:space:]]*Sort\(x\);[[:space:]]*$'    # EXPECT 0 (the release fix is in the tree the release is cut from)
git -C /h/Projects/go2cs diff --stat "$UNION" "$REL" | tail -n 3                                                             # READ: only the bank step and the MS13 refresh (docs, the roster, committed -tests sources)
```
- The battery's exit 0 at `$UNION` (or at the newest fixup-N), the i9's and P1's / P2's readings, 5b's two runs green at
  the SAME head, G's re-read: all five on record (the ledger) before the release starts.
- **The version bump is the release's own step** (`src/version.props` to 1.24.13.4, the README retargets, the frozen
  snapshot `docs/validation/1.24.13.4`): a commit on master by the release procedure, never a commit of this train.
- **RULED 13:07 (COORD): BEFORE THE PIN, on the i7, against the release rehearsal feed** (no CI arm reads it yet):
  `pwsh src/tests/PackageTests/ConsumerUsings/test-consumer-usings.ps1 -Version 1.24.13.4 -Source <the rehearsal's folder
  of .nupkg>` (parameters read at 86553e51b5: `-Version` mandatory, `-Source` a local folder or a URL; it restores into a
  fresh cache and writes only under a temporary work directory). EXPECT exit 0 = BOTH arms as expected: GREEN (the
  fixture, with no using for golib, builds and runs) and CONTROL (with `-p:GoConsumerUsings=false` the build fails with
  CS0246). A non-zero exit holds the PIN.
- OWNER HANDS (route through COORD): the GPG passphrase if the agent's cache is empty (it was after 1.24.13.3), and the
  NuGet signing PIN, once. The release-smoke stage is read AGAIN at the release commit if the bump touched a packed path.
- The owner-approved macOS line rides the release record verbatim (README MS27).
