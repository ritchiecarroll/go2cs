# TRAIN O: COORD's launch checklist, from the seat table to master (DERIVED 2026-10-03 from TRAIN N's)

Git Bash on the i7. Every command matches `trainO/` as derived on 2026-10-03 (`tO-CHANGES.md`). State at the derive:
TRAIN N's battery RUNNING in `/h/go2cs-tmp-coord/tN` (bat1, deadline 2026-10-04 02:06); N lands tomorrow as its union
59ee0d21bf + fixup-2 + the bank step + the MS13 refresh, and **that landed master is O's BASE: it does not exist yet, so
every command below reads it into `$BASE` and no script carries it**. The draft seat list holds **35 rows** after review
round 1 (six owe a re-cut or a new ref: README MS21; three seated shas owe an acceptance read: Q1, Q8, Q22; MS24 proposes
one more row; `tO-CHANGES.md` section 5); no tO worktree, no union branch, no `tO/.battery.lock`; the follow list is EMPTY. One
heavy job at a time on this box (floors 1, 11): nothing below runs while N's battery or N's landing holds the box. A
reading that differs from an EXPECT line is a STOP.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainO
X=/h/go2cs-tmp-coord/coord-scratch/tO
R=$X/run1                                   # a FRESH run folder: the map, the assembly, the fixup AND the battery run from copies here
hp(){ python -B "$(cygpath -w "$R/tO-helpers.py")" "$@"; }
rh(){ grep -E '^[A-Za-z0-9._-]+\|' "$1" | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12; }   # a list's rows-sha256 (the assembler's and the map's hash)
git -C /h/Projects/go2cs fetch -q origin
BASE=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master | cut -f1)   # READ, never typed (floor 15): TRAIN N's landed master
echo "BASE=$BASE"; git -C /h/Projects/go2cs log -1 --format='%h %s' "$BASE"   # EXPECT N's landing commit (the MS13 refresh or N's fast-forward head), NOT 59ee0d21bf
git -C /h/Projects/go2cs merge-base --is-ancestor 59ee0d21bf "$BASE" && echo "N's union is in BASE"   # EXPECT the line
# Review round 1 (F2-1): ASSERTS, not a human read of the subject above (a stale base on origin/master passes every check):
git -C /h/Projects/go2cs log --first-parent --format=%s 59ee0d21bf.."$BASE" | grep -c '^refresh: TRAIN N'   # EXPECT 1 (N's MS13 refresh is IN the base)
{ git -C /h/Projects/go2cs merge-base --is-ancestor 1ddbac142d "$BASE" && echo "fixup-2 in BASE: 1ddbac142d is an ancestor"; } \
  || { n=$(git -C /h/Projects/go2cs log --first-parent --format=%s 59ee0d21bf.."$BASE" | grep -c '^fixup-2: TRAIN N'); [ "$n" -ge 1 ] && echo "fixup-2 in BASE: $n 'fixup-2: TRAIN N' commit(s) on its first-parent line"; }
#   EXPECT one 'fixup-2 in BASE' line (whichever form N's landing used); NO line = STOP
git -C /h/Projects/go2cs log --first-parent --format='%h %s' "$BASE"..origin/master | grep -cE '^[0-9a-f]+ (Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )'   # EXPECT 0 (the assembler and the map refuse otherwise)
```

## 0. The seat table (seconds; nothing is created)

Before the table: **the six MS21 routes are pushed and accepted** (README MS21: A2's new ref at b967f9fa7f; the BOARD docs
row cut by COORD on `$BASE`, with the design note b061154615 beside it or B2's row reworded (Q14); B2's re-cut on B5; C2's
re-cut on r-docs-refresh **cdb9eb8a23** (told before it cuts; r-docs-refresh frozen from then: Q22); G's two re-cuts; P1's
hashset re-cut on xsys-libc), each a NEW ref that was pushed, read back with `ls-remote`, then announced; **MS24's
deletion row** (or its ruled alternative: Q21); and COORD's table edits are in `$HND/tO-seats-draft.txt` (each
re-pointed row's sha and its `stack-on` notes, the token naming its one parent row; the BOARD row added; the Q1, Q8,
Q17, Q22, Q23, Q24 rulings). Every edit moves `<n>` and `<h>`. Then the pre-map at BASE (read-only, minutes per chain;
PM section 7). The run folder takes the SCRIPTS once and the LIST on EVERY pass (review round 1, F2-3: section 0 is
re-run as the re-cuts land; a list copied once goes stale, and every later step reads the run copy):
```
mkdir -p $R && { [ -e $R/tO-assemble.sh ] || cp -r $HND/. $R/; }     # the script set: ONCE
cp $HND/tO-seats-draft.txt $HND/tO-follow.txt $R/                     # the LIST: on EVERY pass
for f in $HND/*.sh $HND/*.py; do cmp -s "$f" "$R/$(basename "$f")" || echo "STOP: $(basename "$f") changed in HND since $R was made: make a NEW run folder"; done   # EXPECT no line
echo "rows HND=$(rh $HND/tO-seats-draft.txt) RUN=$(rh $R/tO-seats-draft.txt)"; cmp -s $HND/tO-seats-draft.txt $R/tO-seats-draft.txt || echo "STOP: the run copy's list differs"   # EXPECT the two equal, no STOP
( cd $R && BASE=$BASE ROWS=$R/tO-seats-draft.txt OUT=$X/premap-base MODE=chain ONCONFLICT=skip bash ./tO-premap.sh ) | tail -n 4
#  EXPECT every row clean (0 conflicted steps), then:  python -B tO-regcheck.py <BASE> <head from OUT/chain.tsv> OUT/chain.tsv
#  EXPECT (review round 1, F2-2: tO-regcheck.py has no D1 credit; the FAIL is by design, PM section 4):
#          'ok' on src/go2cs.slnx, src/go2cs/go2cs-src.projitems and OutputComparisonTests.cs;
#          'FAIL ... head <k> expected <k+3> ... missing=0 dup=0 removed-back=0' on CompileTests.cs, TargetComparisonTests.cs
#          and TranspileTests.cs (A1's and A2's identical CheckPtrToAnonStructPtr insert, which git keeps once and precheck's
#          COUNT arm credits: D1). Any other FAIL, a missing / dup / removed-back above 0, or a delta other than 3 = STOP.
( cd $R && BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 bash ./tO-assemble.sh ) | tail -n 3; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n> rows (tO-seats-draft.txt) rows-sha256=<h> ...   then   TABLE_ONLY=1: stopping ...   rc=0
#          (a 'NOTE <k> best common ancestors' line names a row cut on a local merge: precheck reads it MULTI-BASE, O3, unrun)
echo "HND=$(rh $HND/tO-seats-draft.txt) RUN=$(rh $R/tO-seats-draft.txt)"   # both = <h> (review round 1: the hash of the list in HND, not only the run copy)
```
If G's fm-record re-cut is a NEW ref name, note it now: the battery takes `E_BISECT_SEATS='r-funclit-interface-target
<that ref>'` (section 5; review round 1, F2-5).
MS19's HashSet scan (README MS19) must print NOTHING once the hashset re-cut is a row (as cut it prints
`c1-darwin-xsys-libc 2`). An H4 census (Go-only behavioral directories per row) must read 0 once the trio rides whole.

## 1. The draft into hnd (the lanes read it from origin), the map, the assembly

```
ls -a $HND | grep -c __pycache__                                         # EXPECT 0
git -C /h/go2cs-tmp-coord/hnd add -f -n .claude/coord-scripts/trainO  # DRY RUN: read every path it would add (.claude/ is excluded: plain status lists nothing)
#  EXPECT  only trainO/*.sh *.py *.md *.txt *.ps1, controls/*, premap-run1/* (or leave premap-run1 out by COORD's call); no __pycache__, no *.pyc, no run output
git -C /h/go2cs-tmp-coord/hnd add -f .claude/coord-scripts/trainO
git -C /h/go2cs-tmp-coord/hnd status --porcelain --ignored .claude/coord-scripts/trainO   # EXPECT the trainO files as 'A ' and no '!!' line
git -C /h/go2cs-tmp-coord/hnd commit -S -m "coord-scripts: TRAIN O script set (derived from TRAIN N's)"
git -C /h/go2cs-tmp-coord/hnd log -1 --format='%G? %GK'                  # EXPECT G or U and 941694536F21BAFF, read as its OWN command before the push
git -C /h/go2cs-tmp-coord/hnd push origin claude/coord-handover          # an EXISTING ref: announce, then push (floor 9)
cmp -s $HND/tO-seats-draft.txt $R/tO-seats-draft.txt && echo "rows RUN=$(rh $R/tO-seats-draft.txt) COMMITTED=$(git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainO/tO-seats-draft.txt | grep -E '^[A-Za-z0-9._-]+\|' | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12)"   # EXPECT the line, both = <h> (the lanes read the COMMITTED list; review round 1, F2-3)
( cd $R && BASE=$BASE SEATS=$R/tO-seats-draft.txt bash ./tO-conflict-map.sh ) | tail -n 4
#  EXPECT  MAP base <BASE10> ...  MAP DONE head=<map10> rows=<n> rows-sha256=<h> (every row clean: kept as refs/coord/tO-map/rows-<h> ...)
#          A 'SEAT k <ref>: REFUSED: carries <m> train-assembly commit(s)' line, or an 'ABORT: BASE ... is BEHIND a landed
#          train' line (review round 1), means BASE is not N's landed master: STOP.
#  READ every 'shared paths' list of a clean row that is not a registration file (README MS1; PM section 4 names them)
( cd $R && BASE=$BASE bash ./tO-assemble.sh ) | tail -n 8
#  EXPECT  ASSEMBLED head=<asm10> seats=<n>; MAP TREE: EQUAL to <map10>; ANCESTRY; SHAPE: <n> first-parent merges; ORDER; TREE CLEAN; NEXT: ...
git -C /h/go2cs-tmp-coord/tO rev-parse --short=10 HEAD                   # <ASSEMBLED>
```
`grep -vE '^[[:space:]]*(#|$)' $R/tO-follow.txt | tr -d '\r' | sha256sum | cut -c1-12` reads `e3b0c44298fc` (EMPTY).

## 2. Controls, once, before the fixup (floor 13; read-only except 2e, which aborts before creating anything)
```
hp teattr    "$(cygpath -w $R/controls/te-pos-goframe.patch)"      # EXPECT TE files=3 hunks=5 g-frame=4 (... g-frame-only files=2) map-only=1 other=0
hp hunkclass "$(cygpath -w $R/controls/te-pos-goframe.patch)"; echo rc=$?    # EXPECT HUNKCLASS-VERDICT files=3 hunks=5 other-files=0 ... rc=0
hp teattr    "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"   # EXPECT TE files=1 hunks=25 g-frame=0 (...) map-only=0 other=25
hp hunkclass "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"; echo rc=$? # EXPECT ... other-files=1 ... rc=1
hp cnrexpect 'H:\go2cs-tmp-coord\tO' "$BASE" --base 59ee0d21bf | tail -n 2   # EXPECT CNRDELTA ... added=0 removed=0; CNREXPECT ... n=801 (N's battery CNR line: N's bank and MS13 add no behavioral directory)
hp cnrexpect 'H:\go2cs-tmp-coord\tO' HEAD --base "$BASE" | tail -n 2         # EXPECT CNRDELTA ... added=18 ...; CNREXPECT ... n=819 (at the draft list as routed)
# 2b. precheck on the assembled union (the fixup's PRERES, predicted: README MS9b.1)
hp precheck 'H:\go2cs-tmp-coord\tO' "$BASE" "$(cygpath -w $R/tO-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +(COUNT|REG|ROSTER|H4))'; echo rc=${PIPESTATUS[0]}
#  EXPECT  rc=0; 'ok COUNT' x6 with 'r-ptrptr-anon-struct-lift:+<k>(identical-insert credit -3)' in Compile/Target/TranspileTests (D1);
#          'ok REG src/go2cs/go2cs-src.projitems: ... seat-removed=[p1-hashset-module:...HashSet.go]'; 'ok ROSTER rows base=225 merged=225';
#          'ok ROSTER linux annotations ... none'; 'ok ROSTER darwin annotations ... (the base + 22 row-own edit(s): c1-darwin-pilot-bank, c1-darwin-wave2-bank) mismatched: none' (O6);
#          'ok H4 ... 0'; a 'COUNTBASE multi-base row' line only for a row cut on a local merge (O3); PRECHECK hard-failures=0 notes=0 mode=head.
# 2c. the same reader made to FAIL, on the base: a checkout whose HEAD is $BASE (a fresh child worktree; remove it after with git worktree remove)
#  EXPECT  one 'FAIL BOTH <seat>: no first-parent merge' per row, FAIL COUNT / REG on the registration files the seats add to,
#          and 'FAIL ROSTER darwin annotations ... mismatched:' naming the 22 (the DARWIN arm's negative control)
# 2d. the LIVE gate
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock
hp live 'H:\go2cs-tmp-coord\coord-scratch\tO\.battery.lock' 1; echo rc=$?     # EXPECT one 'LIVE lock ...tZ...' line, rc=1
rmdir /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock /h/go2cs-tmp-coord/coord-scratch/tZ
hp live 'H:\go2cs-tmp-coord\coord-scratch\tO\.battery.lock' 1; echo rc=$?     # EXPECT 'LIVE-VERDICT none ...', rc=0 (an idle box: N's lock gone)
```
2e. the assembly's map refusal (safe only now, with every row merged and no fixup): a copy of the list without its LAST
row (p1-hashset-module: no row stacks on it):
```
grep -v '^p1-hashset-module|' $R/tO-seats-draft.txt > $R/c4-seats.txt
( cd $R && BASE=$BASE SIGN_PROBE=0 SEATS=$R/c4-seats.txt bash ./tO-assemble.sh ) | tail -n 2; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n-1> rows (c4-seats.txt) ...  then  ABORT: no map run is recorded for THIS list (...). ...   rc=2
git -C /h/go2cs-tmp-coord/tO rev-parse --short=10 HEAD                        # EXPECT <ASSEMBLED> still
```

## 3. The fixup (INFERRED about 1 h 30 m: the emission check is most of it; takes the lock)
Values:
- `BASE=$BASE`, `EXPECT_HEAD=<ASSEMBLED>`. No `PROSE_PATCH`.
- `EXP_DESC`, `EXP_TH`: TO BE SET BY COORD, or unset (reported). Read-only derivation:
  `python "$(cygpath -w /h/go2cs-tmp-coord/tO/docs/phase4/recipes/quickjit-optin/optin.py)" check 'H:\go2cs-tmp-coord\tO' | tail -n 2`.
- Defaults: `REGEN=apply GOLDENS=regen SIBLINGS=report GOLDEN_CLASS=gframe CSPROJ_TEMPLATE=stop EDITORCONFIG_NEW=stop FIXUP_N=1`;
  `REGEN_ALLOW` composed (the four footprint rows); `EXEC_ROWS_EXPECT` from the helper (0). `CSPROJ_TEMPLATE=accept` and
  `EDITORCONFIG_NEW=accept` only by COORD's rulings (MS20, MS22).
- MS19: warm the module cache first: `( cd /h/go2cs-tmp-coord/tO/src/go2cs && go mod download ); git -C /h/go2cs-tmp-coord/tO status --porcelain | grep -vc '^??'` (EXPECT rc 0, then 0).
```
cmp -s $HND/tO-seats-draft.txt $R/tO-seats-draft.txt && [ "$(rh $R/tO-seats-draft.txt)" = <h> ] && echo "list = the mapped and assembled one"   # EXPECT the line (review round 1, F2-3)
cd $R && BASE=$BASE EXPECT_HEAD=<ASSEMBLED> SEATS_EXPECTED=<n> bash ./tO-fixup.sh > fixup.console.log 2>&1; echo "fixup rc=$?" >> fixup.console.log
```
EXPECT, in order (read `$R/tO-fixup-logs/SUMMARY.txt`):
- `PRE head=<ASSEMBLED> seats=<n> ... CSPROJ_TEMPLATE=... EDITORCONFIG_NEW=... EXEC_ROWS_EXPECT=0 PROSE_PATCH=none lists=rows:<h>/follow:<f>`
- `PRE derived (R1): REGEN_ALLOW=/^src/core/(...)$/ :: composed from the seat list: c1-darwin-xsys-libc:vendor/... r-funclit-interface-target:runtime/windows/os_windows.cs p2-converter-warning-clears:... (19) g-method-value-fm-record:... (11)` (32 paths; re-read after the re-cuts: fm-record's is re-measured)
- `PRERES precheck PRECHECK hard-failures=0 notes=0 mode=worktree`; the roster guard x2 with `0 with an execution config` (the 5.1 run is the C1 banks' owed reading)
- `4 regen`: the regenerated files are within REGEN_ALLOW (EXPECT at least `runtime/windows/os_windows.cs` and, on R5's single-parent route, `encoding/json/package_info.cs`); a listed path outside it STOPS (MS4). A `REGEN REFUSED src/core/.../.editorconfig: the darwin run DELETED this committed per-file warning-entries file` line (review round 1) means MS24's deletion row is not in the union: STOP, rule Q21, re-assemble
- `5 csproj CNR moved: 18 :: CSPROJTEMPLATE-VERDICT files=18 template-only=18 other=0 ... + the template file delta at <k> cut(s) (O, D2)`, then a STOP unless `CSPROJ_TEMPLATE=accept`
- `5 .editorconfig NEW ...` only if the union CNR wrote one (MS22): a STOP unless `EDITORCONFIG_NEW=accept`
- `5 goldens PREDICTION: none by name ...`; a moved golden STOPS at its class (`GOLDEN_CLASS=gframe`): READ `gold-cs.patch`, then relaunch with `GOLDEN_CLASS=any` (MS4b)
- last: `FIXUP DONE head=<10 hex> subject='fixup: TRAIN O' lists=rows:<h>/follow:<f>`, rc 0.

## 4. After the fixup: the signature by KEY, push, then GO (before the battery, so the lanes run beside it)
```
git -C /h/go2cs-tmp-coord/tO log -1 --format='%H %G? %GK %s' | cut -c1-140
#  EXPECT  <40 hex> <G or U> 941694536F21BAFF fixup: TRAIN O -- ...
git -C /h/go2cs-tmp-coord/tO log --first-parent --format='%G? %GK' "$BASE"..HEAD | sort | uniq -c
#  EXPECT ONE line '<n+1> <G|U> 941694536F21BAFF' -- run as its OWN command and READ it BEFORE the push below (CORRECTED at N:
#  without --first-parent the range takes the seats' own commits, which lanes leave unsigned or ssh-signed; and at N the
#  check was composed into the push's command, so its verdict could not have stopped the push)
git -C /h/go2cs-tmp-coord/tO status --porcelain | grep -vc '^??'              # EXPECT 0
UNION=$(git -C /h/go2cs-tmp-coord/tO rev-parse HEAD)                          # the GO's UNION and FIXUP (one sha)
git -C /h/go2cs-tmp-coord/tO push origin claude/coord-trainO-union            # a NEW ref: PUSH, then READ BACK, then ANNOUNCE with the read-back (mailbox skill)
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainO-union # EXPECT exactly $UNION (the read-back the announcement quotes); never replaced afterwards
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainO/tO-i9-shard.txt | sha256sum | cut -d' ' -f1   # EXPECT 057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d (N's, M's and L's bytes)
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainO/tO-i9-shard.txt | grep -c .                   # EXPECT 132
git -C /h/go2cs-tmp-coord/hnd rev-parse --short=10 HEAD                       # the briefs' <HND> value
git -C /h/Projects/go2cs log -1 --format=%h -S'execution: release-tiered' "$BASE" -- docs/ValidatedTestPackages.md   # EXPECT 2d46eba8f0 (review round 1, F2-4)
#   another sha = N's landing changed the phrase's count: the lane drivers then WALK back (at most 12 commits) to the first
#   whose parent annotates log/slog and stamp the walk; tell the lanes their PRE line will name 2d46eba8f0 after a walk
```
GO to the i9 (`tO-lane-brief-i9.md`): `UNION`, `FIXUP`, **`BASE`** (the driver requires it), `EXPECT_LIST_SHA`,
`EXPECT_ROWS=132`, the handover tip; FOUR files in its run folder. GO to P1 and P2 (`tO-lane-brief-linux.md`): `UNION`,
`FIXUP`, **`BASE`**, the handover tip; three files. P1 is unblocked since 19:59 (its N legs were never run: COORD rules
whether P1 reads N's union first or O's only).

## 5. The battery (INFERRED 11 to 12 h, as README step 6)
Preflight: `ls -d $X/.battery.lock` (absent); `ls $R/tO-logs $R/mod-logs` (absent); free disk at least 30 G on H: and
8 G on C:; nothing else converting or building on the box (the LIVE gate reads it; N's battery and landing are DONE).
Values: `MASTER=$BASE`; `EXPECT_HEAD` = the first 10 chars of `$UNION`; `N_RUN` = N's battery of record (default
`coord-scratch/tN/bat1`; give the re-run's folder if N re-ran). `CNR_EXPECT_N`: leave UNSET (PRE-D derives it).
`I9_PATCH` when the i9's patch has arrived; `I9_BASELINE=coord-scratch/tN/i9-patches/tN-tracked-changes-U0.patch`.
`E_BISECT_SEATS`: unset while G's fm-record row keeps the name `g-method-value-fm-record`; if its R5 re-cut seated a NEW
ref name, `E_BISECT_SEATS='r-funclit-interface-target <that ref>'` (review round 1, F2-5: PRE-D turns a name that is no
row into a FINDING, exit 6).
```
cd $R && MASTER=$BASE EXPECT_HEAD=${UNION:0:10} SEATS_EXPECTED=<n> bash ./tO-battery.sh > battery.console.log 2>&1; echo "battery rc=$?" >> battery.console.log
```
EXPECT within the first minutes:
- `PRE union shape OK: first-parent=<n + 1> (= <n> seat merges + 0 follow-up merge(s) + 1 fixup commit(s) ...)`
- `PRE head=<10 hex> ... master=<BASE> (<BASE10>) previous-train record N_RUN=.../tN/bat1 (SUMMARY <k> lines) seats=<n> ...`
- `PRE-D CNR: EXPECT N=819 (derived (cnrexpect at HEAD)); ... the reader's control at <BASE>: N=801 vs TRAIN N's battery 801 (...); reconciliation: 801 + 18 measurable added - 0 removed = 819 ...` (a re-routed list moves these, never by hand)
- `PRE-D behavioral: projects the union adds=[...] (18; slnx agrees: yes ...)`
- `PRE-D execution configs ...: rows that keep an annotation (EXPECT tiered=True)=[] rows that drop it (EXPECT tiered=False)=[]; EXEC_ROWS_EXPECT=0`
- `PRE-1 precheck rc=0 :: PRECHECK hard-failures=0 notes=0 mode=head` and the ROSTER darwin line (2b's)
- later: `NGa51:selfdescription ... ran 16, failed 0` and `NGa7:selfdescription ... ran 16, failed 0`; `IDC (...): rc=0 :: SELF-TEST: pass=<>=217> fail=0`;
  `PUB environment: ... = 10.0.* ... pwsh starts under the pinned-go PATH (O5)`; `PUB (...): rc=0 :: published-output gate: 6 of 6 published programs match go, none hung`;
  `WE (...): rc=0 final='PASS'`; `WEc (...): rc=0 caught=1 :: CONTROL FIRED`
STOP conditions (the exit status): **2** an ABORT in PRE (a `MASTER ... is not on the union's first-parent line` ABORT
means the wrong base was given); **3** a disk or build abort; **5** a wall cap fired (PUB 30m, WE 150m, WEc 45m among
them) OR a process is still running from `$R/out` after MOD: the lock is KEPT, kill the listed PIDs by PID, `rmdir
$X/.battery.lock`, relaunch from a FRESH folder; **9** the deadline; **6** at END = a leg outside FXc1 / FXc2 / SIc / NVR
is non-zero or a FINDING exists; **0** clean. A red needing a tree change after the push is a `fixup-2` ON TOP
(`FIXUP_N=2`), never a replaced sha.

## 5b. The release gate (MS23): a CI run at the union, COORD's manual step, BEFORE landing
The i7 battery runs no `release-smoke.ps1`, no per-flavour pack and no walkthrough arm D (README MS23). Dispatch once the
union is pushed (it runs on hosted runners beside the battery):
```
gh workflow run os-matrix.yml --ref claude/coord-trainO-union -f goos=windows -f stage=release-smoke
gh run list --workflow os-matrix.yml --branch claude/coord-trainO-union --limit 1    # <run id>
gh run watch <run id>; gh run view <run id> --log | grep -E 'release-smoke|exit `' | tail -n 20
```
EXPECT the pack job green (344 packages, 0 failed, as C1's proof run 37144621021) and on every RID (win-x64, linux-x64,
osx-x64, osx-arm64) A, B, C PASS and D GATING and PASS (C1's 37155599211 at 5cda875206). A red holds the landing; a
`fixup-N` touching the release paths owes this run again (README 4b).

## 6. Landing (README MS11, then MS13), after the battery, the lane readings and 5b
1. Merge master into the union if it moved; `git diff --stat $BASE <master>`, read whole (MS11).
2. The landing precheck (`hp precheck <tO> <master> <run>/tO-logs/seats-effective.txt head`) and the roster guard x2 with
   `EXEC_ROWS_EXPECT=0`: BEFORE the bank step.
3. The bank step (as at N).
4. **MS13, the refresh of committed -tests sources** (a bank step, never a leg): the tree back at HEAD, then
   `hp testsrc-refresh 'H:\go2cs-tmp-coord\tO' --out <run>\tO-logs\testsrc-refresh.patch <run>\tO-logs\S-rewrites.patch <run>\tO-logs\T-rewrites.patch <the i9's tracked-changes.patch>`
   (EXPECT `REFRESH-VERDICT ... refused=0 conflicts=0`), `git apply --check` then `git apply`, then
   `hp testsrc-refresh 'H:\go2cs-tmp-coord\tO' --check-worktree` (EXPECT rc 0, `ONLY committed test sources changed`), then
   ONE signed commit `refresh: TRAIN O -- committed -tests sources from the windows re-emission (MS13)`, its signature read
   by KEY as in section 4.
5. Fast-forward master to the union; the ledger line; the FLEET post. The 1.24.13.4 release (COORD's 13:30 ruling on C2's finding:
   "land N, 1.24.13.4 right after O") follows O and is not a step of this checklist.
