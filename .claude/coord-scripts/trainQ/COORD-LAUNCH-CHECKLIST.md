# TRAIN Q: COORD's launch checklist

**DRAFT 2026-10-06, REVIEW ROUND 1 applied (`tQ-CHANGES.md`, its last section), UNCOMMITTED.** Q is an ordinary train: nothing below assumes a release, and C1's hosted release
gate reads the union all the same (5b). The train after Q re-converts the corpus, so Q lands a clean, fully gated
tree. Every EXPECT is a prediction read from git objects, from the i9's rehearsal (`tQ-map.md`) or from P's run2;
none was read from a Q run. A line that prints otherwise is a STOP until it is read.

Conventions: one command, one reading. A NEW ref is pushed, read back, THEN announced; an EXISTING ref (the handover
branch, master) is ANNOUNCED, THEN pushed (floor 9). A signature is read as ITS OWN command before any push. After a
rejected or interrupted call that posts or pushes, READ the target before reporting or re-sending. The mailbox
anchor is the last tip READ.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainQ
X=/h/go2cs-tmp-coord/coord-scratch/tQ
R=$X/run1                     # a FRESH run folder for every battery or fixup launch (run2, run3 ...)
rh(){ grep -E '^[A-Za-z0-9._-]+\|' "$1" | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12; }
```

## 0. Before anything: the shell, the base, the table

```
env | grep -E '^(W|BASE|MASTER|EXPECT_HEAD|O_RUN|N_RUN|P_RUN|MSYS_NO_PATHCONV|SEATS|RULED|TWOPASS_ROW|T2_ROW)='   # EXPECT no line (a fresh shell)
echo "$PATH" | tr ':' '\n' | head -n 4                                   # EXPECT no .NET 10 SDK folder ahead of the rest (pwsh would not START; the battery's PRE probes it)
git -C /h/Projects/go2cs ls-remote origin refs/heads/master              # the sha it prints IS the base (7098b8d3f9 at the derive; b6e4856883 from 16:49 and 0457242046c8d5c29a6ff547bcbcc0e74c1fe973 from 16:55 on 10-06: it moved twice during review round 1)
export BASE=<that full sha>
git -C /h/Projects/go2cs fetch origin master
git -C /h/Projects/go2cs merge-base --is-ancestor 40a1f839c55ed708c7733885c9d5079716babf1e $BASE && echo "P's landing is inside the base"   # EXPECT the line
git -C /h/Projects/go2cs log --first-parent --format=%s $BASE -n 400 | grep -c 'into TRAIN Q \|: TRAIN Q'    # EXPECT 0
```
BASE MOVES when COORD lands a master commit before the freeze. It moved twice on 10-06 (the known-issues entry for a
hand-written Native AOT consumer, 16:49, and its narrowing to linux, 16:55: two docs commits, `docs/KnownIssues.md`
and `docs/Limitations.md`, which no Q row touches; the row-order simulation reads byte-identical on all three
bases). The census tool is still queued (on a branch first, with every lane's self-test). Each move: read BASE
again, re-read the table at it, re-run the map. `tQ-assemble.sh` REFUSES a BASE that is not master's tip.

The table (`$HND/tQ-seats-draft.txt`, header line 4) is frozen only when all of these are answered:

| Open | Where | What the scripts do until then |
|---|---|---|
| **c1-aot-smoke 44f29d50e6 DOES NOT MERGE AS CUT (QUESTION 7)** | it conflicts with c1-release-smoke-published in `.github/workflows/os-matrix.yml` (2 hunks, a non-empty base: workflow logic, no ruling can cover it) and `docs/CIMatrix.md` (`tQ-map.md` D2). C1 RE-CUTS (a new ref that merges fd2b421bba in and composes the two conditions); the row then names the re-cut tip with a second stack token, the hosted release-smoke is read AT that tip, and COORD writes `READING IN (<when>, <where>)` in the row or drops the candidate word. Or COORD strikes both trim rows | the row says CAND: the table check refuses the list BY NAME (rc 2) before anything is created |
| The windows Native AOT failure behind the trim default (QUESTION 8; C1's URGENT post 21:49Z, mailbox 079b80b37b) | COORD's notes answer it in part (L:371): the trim rows ride as they are (the known-issues entry already says linux only) and the fix is a golib seat for the train after Q, 'unless it is small and ready before the freeze'. Open: that 'unless', on the i9's sizing | nothing: aot-smoke is non-gating at Q |
| The census-limit row's position (QUESTION 1) | ANSWERED IN THE DRAFT BY THE MOVE: g-board-cs8500-census-limit sits directly below g-cs8500-managed-view, where the i9 rehearsed it. To keep v0's order: move the row back below g-board-testing-t-deepequal AND uncomment its line in `tQ-ruled.txt` | as seated: clean (the table's twelve ruled lines are other rows'). Moved back without the line: the map reads that row NOT CLEAN and the assembly aborts there, naming the hunk |
| The two rows that were candidates at the first draft | NOTHING OWED: g-r2m-caller-closure-v2 633b045e8a ACCEPTED 14:59 (ledger f2f8b9686e) and c1-golib-trim-default ab6aa8f443's condition met (release-smoke 37523436862 green on four RIDs; COORD 21:34Z: 'the table has no conditional row left'). Neither row spells the candidate word any more. c1-golib-trim-default rides ONLY with c1-aot-smoke behind it | the table check passes both |
| The two SLOT comments (the Limitations docs row; COORD's own Q BOARD row. G's g-board-logrus-launchdir-line 5497b2d7b9 and C1's c1-board-aot-partial-cost 513ab62499, accepted by the ledger at 17:38 and 16:55, are ROWS 64 and 63 now, each with its line in `tQ-ruled.txt`) | seat each as a row with its sha (a BOARD row also takes ITS OWN line in `tQ-ruled.txt`, before the map), or delete the comment, or acknowledge them: `SLOTS_OK=2` on the map and on the assembly | the map and the assembly REFUSE a list that still holds a `#   SLOT` comment unless `SLOTS_OK` equals their count (review round 1); a `ref||notes` row is refused by name |
| The acceptance words still owed by name (header line 4, item 4): g-board-foreign-defined-directions 6481ddbabb and c1-release-smoke-safe-tag 3b093f935a | COORD's notes or the ledger | nothing: a script cannot read a ruling |
| The rulings of record for THREE BOARD hunks (`tQ-ruled.txt`: g-cs8500-managed-view's, taken from the rehearsal's step 2 through L:368; and the two docs rows' of rows 63 and 64, written from the ledger's acceptance words 'BOARD appends', never rehearsed) | confirm each, or comment its line out | as written the map resolves those hunks by rule |
| S1's three hand-written csproj (QUESTION 5) | confirm `tQ-helpers.py` S1_NONE / S1_PLAIN, or ask the i9 for one commit | precheck passes with the amendment as drafted |
| X_ROWS (`encoding/json testing encoding/gob`) | confirm in the i9's GO | the battery sweeps those three i9 rows too |

```
( cd $HND && OUT=$X/controls-$(date +%H%M) bash ./tQ-controls.sh ) | cut -c1-200 | tail -n 18; echo rc=${PIPESTATUS[0]}
#  EXPECT 16 'CONTROL <arm>: ok' lines (CB, EXT, EXT-real, TC, RT, RT-real, CS, PF, PS, RG, ED, TE-plant, TE-carrier, TE-M, HW, RES),
#         then  Q-CONTROLS DONE failed=0 not-run=0   rc=0.   An arm that prints NOT RUN makes rc 1 unless NOT_RUN_OK names it.
#         OUT must NOT exist (or be a folder this script made: it carries a marker file); any other existing folder is refused, never deleted.
( cd $HND && OUT=$X/selfcheck bash ./tQ-selfcheck.sh ) | grep -v '^    ' # EXPECT PLANTS ok, S1 to S6 ok, Q-SELFCHECK DONE failed=0
```
(Both ran at the derive and again at review round 1 with those results, and at review round 1 every control arm was
also MADE TO FAIL once on a scratch copy: `tQ-CHANGES.md`, REVIEW ROUND 1. Neither builds, converts or tests.)

## 1. The drafts into hnd

```
ls -a $HND | grep -c __pycache__                                          # EXPECT 0
git -C /h/go2cs-tmp-coord/hnd add -f -- ':(glob).claude/coord-scripts/trainQ/*.sh' ':(glob).claude/coord-scripts/trainQ/*.py' ':(glob).claude/coord-scripts/trainQ/*.md' ':(glob).claude/coord-scripts/trainQ/*.txt' ':(glob).claude/coord-scripts/trainQ/*.ps1'
want=$(ls $HND/*.sh $HND/*.py $HND/*.md $HND/*.txt $HND/*.ps1 | wc -l); got=$(git -C /h/go2cs-tmp-coord/hnd ls-files -- .claude/coord-scripts/trainQ | grep -c .); echo "in-index=$got want=$want"   # EXPECT equal: 42 (22 .sh, 6 .py, 7 .md, 6 .txt, 1 .ps1). The INDEX is counted, not the staged diff: tQ-seats-gen.sh and tQ-seats-v0-coord.txt are TRACKED already (handover commit 8094d8c0a7), so 'diff --cached' reads 40 and never 42 (review round 1)
git -C /h/go2cs-tmp-coord/hnd diff --cached --stat -- .claude/coord-scripts/trainQ | tail -n 1            # READ it: 40 files added when COORD's two are unchanged
git -C /h/go2cs-tmp-coord/hnd commit -S -m "coord-scripts: TRAIN Q script set (derived from TRAIN P's)"
git -C /h/go2cs-tmp-coord/hnd log -1 --format='%G? %GK'                   # ITS OWN COMMAND: G or U and the signing key, read BEFORE the push
```
Announce, then `git -C /h/go2cs-tmp-coord/hnd push origin claude/coord-handover`, then read the remote tip back.

## 2. The run folder, the table check and its controls

```
mkdir -p $R && cp $HND/*.sh $HND/*.py $HND/*.ps1 $HND/*.txt $HND/*.md $R/     # the script set: ONCE a run folder; the LIST again on every pass
for f in $HND/*.sh $HND/*.py $HND/tQ-ruled.txt; do cmp -s "$f" "$R/$(basename "$f")" || echo "STOP: $(basename "$f") changed in HND since $R was made"; done   # EXPECT no line
( cd $R && BASE=$BASE SLOTS_OK=2 TABLE_ONLY=1 SIGN_PROBE=0 bash ./tQ-assemble.sh ) | grep -v 'stacked on row\|ordered after row' | tail -n 7; echo rc=${PIPESTATUS[0]}
#  EXPECT  two MULTI-BASE NOTE lines (c2-module-disclosures, c2-noinline-partial-on-nameof; a third for c1-aot-smoke once it is re-cut on two rows),
#          'TABLE: NOTE SLOTS_OK=2 ...' while the two SLOT comments stay, then
#          TABLE OK: <n> rows (tQ-seats-draft.txt) rows-sha256=<h> ...   TABLE_ONLY=1: stopping ...   rc=0
#          (19 'stacked on row' lines and 3 'ordered after row' lines at the 64-row draft; it takes several minutes: the patch-id pass)
#  AT REVIEW ROUND 1, on the table as it stands, BASE=0457242046 (a control copy, the fetch stubbed): rc=2 and ONE problem,
#          'TABLE row 56 c1-aot-smoke: a CAND row ...'; without SLOTS_OK a second, the two SLOT comments. With that row and
#          c1-golib-trim-default commented: TABLE OK, rc=0 (read at the 62-row state: 60 rows, rows-sha256=518c29cb3ab5; the 64-row
#          table less those two is 62 rows, rows-sha256=20856873cf11).
```
MS9c's planted lists, once each (`SLOTS_OK=2 TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<a planted copy>`; EXPECT `ABORT: 1
seat-table problem(s)`, rc 2, the row named): a ref listed twice; a stacked row above its base row; a seated sha
that is not its remote tip; **a SLOT row** (`some-ref||notes`); **a CAND row** whose notes do not hold `READING IN (`
(the bare two words in prose do NOT release it); and, with `SLOTS_OK` unset or wrong, **the SLOT comments**.

## 3. The map, then the assembly

```
( cd $R && BASE=$BASE SLOTS_OK=2 SEATS=$R/tQ-seats-draft.txt bash ./tQ-conflict-map.sh ) > $R/conflict-map.console.log 2>&1; tail -n 5 $R/conflict-map.console.log
grep -c ': clean ' $R/conflict-map.console.log; grep 'RULED resolution\|CONFLICT\|SLOT\|CAND' $R/conflict-map.console.log | cut -c1-200
```
EXPECT, with the 64 rows, c1-aot-smoke RE-CUT (QUESTION 7) and its row released:

| Line | Expect |
|---|---|
| `SEAT n <ref>: clean` | 56 rows of 64 (54 of 62 with the two trim rows commented; 55 when the census-limit row was moved back to v0's position and its hunk RULED) |
| `SEAT n <ref>: RULED resolution in:` | 8 rows: c2-func-chan-dir, c2-board-warnings, g-named-pointer-equality (5 files), g-cs8500-managed-view, g-board-foreign-defined-directions, g-board-testing-t-deepequal, and the two docs rows seated last, c1-board-aot-partial-cost and g-board-logrus-launchdir-line (9 with the census-limit row in v0's position and ruled) |
| `RULED: 12 line(s) ... 8 merge(s) resolved by rule; UNUSED: none; SLOT comments in the list (not rows): 2 (acknowledged: SLOTS_OK=2)` | ruled-sha256 `13300e8b417d` as the file stands at review round 1 (it moves with any edit of a ruled line, its note included). 13 and 9 with the census-limit hunk ruled. One more line and one more merge if COORD's own Q BOARD row is seated from its slot |
| `SEAT 56 c1-aot-smoke: CONFLICT (UNRULED) in: .github/workflows/os-matrix.yml docs/CIMatrix.md` | ONLY if the row was released without its re-cut: that is the measured conflict of `tQ-map.md` D2. STOP |
| `RULED-FILES ... BOARD lines=26507 (base 26181) guard-final=1; go2cs.slnx Project lines=959 (base 931); lists [TestMethod]/Check: TranspileTests=816/816 CompileTests=816/816 TargetComparisonTests=816/816 OutputComparisonTests=787/787` | `tQ-map.md` A and B. A list whose two numbers differ, or a BOARD count below 26507 (26467 without the two docs rows), is a resolution that dropped lines: STOP |
| `MAP DONE head=<map10> rows=<n> rows-sha256=<h> ruled-sha256=<r> (every row clean or resolved by rule (8) ...)` | `NOT-CLEAN rows=<k>` = STOP: read each `CONFLICT (UNRULED)` line: it names the row, the file and every hunk |

The rehearsal merged six seats in other positions than the table's, so THIS run is the first reading of the table's
order. READ the `shared paths` list of every clean row that is not a registration file, and READ WHOLE at the map
head: `src/go2cs/testConversion.go` (eleven writers), `convCallExpr.go` (five), `reflect/value_impl.cs` (three),
`src/core/golib/buildTransitive/go.lib.targets` (two; a PACKED file), and `.github/workflows/os-matrix.yml` (three
writers with c1-aot-smoke: read the two `if:` lines the re-cut COMPOSES, the pack job's and the feed download's).

```
( cd $R && BASE=$BASE SLOTS_OK=2 bash ./tQ-assemble.sh ) > $R/assemble.console.log 2>&1; tail -n 12 $R/assemble.console.log
#  EXPECT  TABLE OK; RULED: ... ruled-sha256=<r> (the map's: <r>); <n> SEAT lines, 8 of them 'merged WITH RULED RESOLUTION';
#          ASSEMBLED head=<asm> seats=<n>; MAP TREE: EQUAL to <map10>; ANCESTRY; SHAPE: <n> first-parent merges; ORDER;
#          RULED-FILES (the map's numbers); RULED: ... 12 line(s) ... 12 used ... UNUSED: none; TREE CLEAN; NEXT: ...
git -C /h/go2cs-tmp-coord/tQ log --first-parent --format='%G? %GK' $BASE..HEAD | sort | uniq -c    # ITS OWN COMMAND: <n> lines, all G or U, one key
git -C /h/go2cs-tmp-coord/tQ log --first-parent --format=%B $BASE..HEAD | grep -c '^Resolved by rule'   # EXPECT 8
```
An `ABORT seat n <ref>: UNRULED conflict` leaves the union at the last good merge: nothing is resolved by hand.
Either a ruling is written in `tQ-ruled.txt` (then the MAP runs again first), or the row is re-cut or struck.

## 4. The fixup, the push, the GOs

```
( cd $R && hp(){ python -B "$(cygpath -w $R/tQ-helpers.py)" "$@"; }; hp precheck 'H:\go2cs-tmp-coord\tQ' "$BASE" "$(cygpath -w $R/tQ-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +(COUNT|REG|ROSTER|H3|H4|S1))'; echo rc=${PIPESTATUS[0]} )
#  EXPECT  rc=0; 'ok COUNT' x6; two 'COUNTBASE multi-base row' lines; 'ok ROSTER rows base=225 merged=225'; linux annotations none
#          moved; 'ok S1 census'; no FAIL. A 'FAIL S1' on a PackageTests csproj = QUESTION 5 was answered the other way.
( cd $R && BASE=$BASE EXPECT_HEAD=<asm10> SEATS_EXPECTED=<n> bash ./tQ-fixup.sh ) > $R/fixup.console.log 2>&1; echo "fixup rc=$?" >> $R/fixup.console.log
```
EXPECT in `tQ-fixup-logs/SUMMARY.txt`: PRERES precheck 0 hard failures; the sort-fix regression 0 / 3 (control at
the base 0); step 4 `union-attributable=0` on windows, linux, darwin, `regen applied 0`, HANDOWN 0 written; **step
4t** `rows [row:read/predicted/other] net/rpc:6/6/other0 log/slog:3/3/other0 encoding/json:1/1/other0 :: rows whose
read count is NOT its prediction (either direction): [none] :: rows with OTHER hunks: [none]` (the predictions are
read from the tree at run time, each from the package's OWN directory: review round 1 corrected net/rpc from 14,
which counted net/rpc/jsonrpc's 8; a row named in either list is read from `4t-<row>.patch` BEFORE the battery);
step 5 CNR `NO REGRESSION` with 0 goldens moved. A golden that moves STOPS the fixup whatever its class: by the
N-PARTIAL class (0 predicted: the rehearsal's one is a row) for `GOLDEN_CLASS=npartial`, by any OTHER line kind for
`GOLDEN_CLASS=any`, each COORD's ruling after reading `gold-cs.patch`; the commit message then quotes the
classifier's own counts (review round 1); `7 COMMITTED <fix10>`.

```
git -C /h/go2cs-tmp-coord/tQ log -1 --format='%G? %GK %s'                 # ITS OWN COMMAND
git -C /h/go2cs-tmp-coord/tQ push origin claude/coord-trainQ-union        # a NEW ref
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainQ-union   # read back: the full sha IS the tip the GOs name
```
Then the announcement and the GOs. Every GO names: the union tip (full sha), BASE (full sha), the handover commit
that holds the drivers, and for the i9 the shard list's sha256 (`057c78c1...`, unchanged since M) and TWOPASS_ROW.

| Lane | Owes, at that tip | Brief |
|---|---|---|
| i9 | the 132-row complement shard, S2, the two patches with their sha256 | `tQ-lane-brief-i9.md` |
| P1 | the linux legs (L1, T2, GolibTests x2, LCn, the rows, LPB, LX, LB, LM with xsync2). Its exit status: 0 green; 4 a mover; **7 = green and a re-read of one module alone is owed on that box: a post that says exit 7 carries BOTH lines, the EXTERNAL one and the re-read's** | `tQ-lane-brief-linux.md` |
| P2 | the second linux box (GolibTests, T2, runtime/pprof, runtime FULL, the TBS loop); its linux full behavioral for g-r2m if that reading is still out | `tQ-lane-brief-linux.md` |
| C1 | the hosted gate (5b) | its own |
| G | its module re-reads at the union (logrus, testify, cobra, x/sync, pflag), as at P | none |

## 5. The battery

```
cat > $R/launch-battery.sh <<'EOF'
cd /h/go2cs-tmp-coord/coord-scratch/tQ/run1 || exit 1
MASTER=<BASE, full sha> EXPECT_HEAD=<fix10> SEATS_EXPECTED=<n> DEADLINE='<YYYY-MM-DD HH:MM, launch + 16 h>' bash ./tQ-battery.sh > battery.console.log 2>&1
rc=$?; echo "battery rc=$rc" >> battery.console.log; exit $rc
EOF
```
Launch it detached. The tQ worktree is FROZEN until `battery rc=` prints (floor 4). Read `tQ-logs/SUMMARY.txt`.

| PRE / leg | EXPECT at the 64-row union |
|---|---|
| PRE | shape `<n> seat merges + 1 fixup`; `pwsh=7.x`; `previous-train record (TRAIN P) P_RUN=.../tP/run2 (SUMMARY <k> lines)` |
| PRE-D CB | 44 tests the union adds + 6 base guards (the scan may drop a fixture line: stamped) |
| PRE-D CNR | `EXPECT N=863`; the control at MASTER `N=835 vs TRAIN P's battery 835`; added 28, removed 0 |
| PRE-D behavioral | 23 projects, slnx agrees |
| PRE-D E-bisect | converter seats 30; footprint rows `c2-noinline-partial g-cs8500-managed-view`, both bisectable |
| PRE-D UF / sort fix / CNR-FRESH / records / module tags | no known class, both files tracked; 0 and 3, control 0; `[none]`, harness classes `CompileSkipTests TranspileMemoTests`; `ALWAYS writes`; `[safe]`, addressPairs yes |
| PRE-1 precheck | rc 0, as in section 4 |
| C, CC | ok; CB `50/50 '--- PASS', 0 SKIP/FAIL` (no KNOWN self-skip is listed at Q) |
| FX | tracked 1291 = current, stale 0; the two controls fire |
| E | plants OK, 6 x rc 0, union-attributable 0, handown-written 0; `written=` SMALL in both arms |
| G1 / G2 | pass x2; the check count is READ (2092 at c2-roster-ceiling-scope's own tip); 0 execution configs |
| NV51 / NV7 / NVR; PPS51 / PPS7 | pre-flight clean x2, the refusal by name; `pack-paths selftest: PASS` x2 |
| 2b | 0 errors, 959 projects |
| GN, CT, TR, TRH | GenTests 138 (the rehearsal); 24; TestingRuntimeTests at its derived floor; TRH Failed 0 |
| GT x4 | the three added classes at their floors; the box's known trio only |
| 4 | `NO REGRESSION ... all 863 behavioral packages`, 7 skips |
| 5, B:* | every phase green; each of the 23 new projects isolated |
| PUB | `6 of 6 published programs match go, none hung` |
| **PUB2** (review round 1) | `PUB2 InterfaceAssertionMapKey (single-file profile, twice into one folder ...; union holds the symbols targets: yes): publish rc 0 then 0; ... same names=yes; .pdb <n> then <n>` with n above one: the outcome of i9-pack-symbols for a USER program, which no other leg of this box gates. `PUB2c ... The control FIRED` (the row's off switch loses the files): a READING; if it says it did NOT fire, read `publish-twice-control.log` before trusting PUB2 as a gate. This arm has never run: its first run is this battery |
| H7 x3 | rc 0 (the first three-flavour compile of the carrier's partial methods on this box) |
| MOD | every verdict PASS; `MR3-tags` PASS; `XS2` PASS: `.pdb` equal before and after AND `floor before=ok after=ok` (more than one beside every host: equal zeros no longer pass), the second run's rc equal to the first's; x/sync 28, x/mod 9 of 9; `END failures=0 external=0 owed-lines=0`. `EXTERNAL` = a Go-side network or quota failure: named, OWED, exit 7 if nothing else stands. A `TIMING-CLASS` line = the first run failed a sub-2-second window and the second validated: the FAIL stays counted. `mod-logs/OWED-rereads.txt` ALWAYS exists after MOD (empty = nothing owed); a `MOD-OWED` finding = it does not |
| S | every row at its banked count; `record=fresh` on each WITHOUT a pre-delete; `HOSTWALL cs-host=..s (pass) P=..s x..`; `testing`, `encoding/gob`, `encoding/json` read here too; no `evidence copy NOT taken` line (it means the record in the tree was an earlier leg's) |
| TE | N-PARTIAL in bulk (the carrier's class), OTHER read against P's patch |
| T2 | `runtime/debug` twice: Validated 8 and 8, two fresh records, same file names, `.pdb` > 1 and equal, 0 symbol refusals |
| T | runtime/pprof and runtime at their banked counts, BANK-ELIGIBLE YES; /panic and /trap disclosed |
| PUBSYM, UF, UF-T | 0, 0, 0 |
| END | `HOSTWALL` list (a prompt); `OWED AT THE LANDING HEAD (...) owed-lines=0: nothing`; `EXIT 0` |

EXIT 6 = a red or a finding, read by name. EXIT 7 = green and a reading is OWED at the landing head (section 6,
step 3: the final reads re-read it and refuse the landing until it is CLEAN).

### 5b. C1's hosted gate, as it prints at Q

Dispatched by C1 at the union tip; read BEFORE landing, again after any fixup-N on a release-visible path.

| Run | EXPECT |
|---|---|
| release-smoke: the pack | 344 packages merged; `Pack paths: CLEAN -- no absolute path in any assembly or symbol file of <n> package(s)` (the i9's guard ends every pack) |
| release-smoke: each of win-x64, linux-x64, osx-arm64, osx-x64 | A PASS; B PASS with **5 lines** (the fifth is `BuildTag = safe` on both sides: c1-release-smoke-safe-tag); C PASS; D PASS, GATING |
| the same, arms E and F | three `MEASURED, NOT GATING:` lines: `PASS (E-publish-symbols)`, `PASS (E-publish-symbols-aot)`, `PASS (F-package-symbols): RUN ... \| PUBLISH 1 ... \| PUBLISH 2 ... \| FDD ... \| OFF ...`. They do not move the exit code; Q's union is the FIRST of the two trains C1 wants green before they gate. WITH c1-aot-smoke SEATED arm F prints NO AOT part (C1's run 37523436862 at 44f29d50e6: 'F PASS -- RUN, PUBLISH 1, PUBLISH 2, FDD ..., 31 of 31 matching .pdb, OFF none ...; no AOT arm in release-smoke', legs of two to six minutes). If c1-golib-trim-default rode WITHOUT it, arm F would still carry its AOT sub-check, the one that cancelled three of four RIDs on the leg budget: that pair never rides split. The run that decides the seat is owed AT THE RE-CUT TIP (QUESTION 7) |
| **aot-smoke** (ruled 14:59, L:369: its own dispatch, four RIDs, leg budget 240 minutes and 360 on the Macs, it prints the publish wall time) | NON-GATING at Q; read before every release after. FIRST READING, run 37523439849 at 44f29d50e6 (C1, 21:49Z): linux-x64 PASS in 56 minutes; **win-x64 FAIL**: the publish succeeds (71 minutes) and the executable exits 2 at startup, 'There is no metadata token available for the given member' (golib orders a struct's fields by `FieldInfo.MetadataToken` at four sites: read at the tree by C1, not run); both Macs were still compiling. QUESTION 8 of the table: COORD's ruling, not a script's |
| darwin behavioral FULL | both Macs: every project, the 23 new ones among them |
| darwin census | 344 / 344 on both Macs (P1's darwin warnings row changes two hand-owned companions) |
| docs-site | builds, links against the baseline (the BOARD takes ten appends; `docs/PLAN-marker-comment-parity.md` is new) |

`published_version` (c1-release-smoke-published) runs the same arms against nuget.org AFTER a release; it is not a
pre-landing reading.

## 6. Landing

The union lands when: the battery ended EXIT 0 (or 7, with the final reads of step 3 below green), the i9's shard
and both linux lanes read as their briefs expect (a lane's exit 7 with its re-read line), C1's gate read as 5b, G's
module re-reads are in.

**RUN is the run folder of THE battery that ended at this head, and both landing scripts now PROVE it** (review
round 1: they took any folder with an rc line, so the run1 of a battery relaunched as run2 could feed the refresh
the older converter's rewrites, or read 'none owed' from a folder that never held the owed file). Each reads the
folder's own `battery.console.log` and `tQ-logs/SUMMARY.txt`: the battery's PRE and END heads, its `EXIT` line and
its `battery rc=` line. The prep requires that head to be HEADFULL; the final reads require it to be HEADFULL or
its parent, and `mod-logs/OWED-rereads.txt` to EXIST with the line count the battery stamped. Two switches, each
COORD's explicit call and stamped: `BATTERY_RC_OK=<n>` for a red ruled by name, `BATTERY_HEAD_OK=<10 chars>` (final
reads only) when a hand bank commit sits between the battery's head and the refresh.

```
# 1. precondition: the battery's rc line, the lane posts read, the i9's patch copied and its sha256 verified against the post
RUN=$R MASTER=$BASE HEADFULL=<battery head, full sha> I9P=<the i9's tracked-changes.patch> I9P_SHA256=<as posted> bash $R/tQ-land-prep.sh
#  EXPECT  'PRE battery of record: <RUN> -- the battery read this head (<10>) itself; EXIT 0; owed-lines=0' (a 'STOP: ... read head <x>, not <y>' = a STALE run folder);
#          PRE pwsh starts; 2a 'PRECHECK hard-failures=0 notes=0 mode=head'; 2b the roster guard x2, 0 execution configs;
#          3 rows NOT at their banked counts: 0; 4b 'REFRESH-VERDICT ... refused=0 conflicts=0';
#          4d 'ONLY committed test sources changed'; 4e the class census; 'PREP DONE at head <10>, from the battery of record <RUN> ...', NOT committed
```
**The refresh.** The converted test sources of record are the WINDOWS emission (the i7's sweeps and T legs and the
i9's shard, at ONE head); a linux lane's rewrites are never input. It COMMITS tracked `*_test.cs`,
`package_test_info.cs` and `package_info_internal_test.cs` under `src/core` that are neither hand-owned nor golib. It
REFUSES, by category and never writes: production files (`package_init.cs`, `go2cs_test_host.cs`), proof pages,
csproj, review siblings, hand-owned files, a block that creates, deletes, renames or binary-patches a file, and any
file two windows inputs rewrite differently (a CONFLICT: root it, never pick one). At Q it is large by construction:
READ step 4e's line (the N-PARTIAL count against the tree's prediction) and EVERY OTHER hunk before signing.

```
# 2. COORD commits the refresh, single-parent, signed:  'refresh: TRAIN Q -- <files> committed test sources ...' (the message quotes 4e's census)
git -C /h/go2cs-tmp-coord/tQ log -1 --format='%G? %GK %s'                 # ITS OWN COMMAND
# 3. the final reads AT THAT HEAD (the converter suite again; every owed re-read). THE SAME $R AS STEP 1.
RUN=$R HEADFULL=<refresh head, full sha> bash $R/tQ-land-final-reads.sh
#  EXPECT  'PRE battery of record: <RUN> -- the battery read this head's parent <10> (the refresh sits on it); EXIT 0; owed-lines=0
#           (mod-logs/OWED-rereads.txt exists and holds 0 line(s) ...)';
#          '1. go test ./... at the head that lands: rc=0', packages ok, tracked changes after 0;
#          '2. owed re-reads: 0 line(s)' (or each line re-read CLEAN by tQ-reread.sh);  FINAL READS at <head>: GREEN
#          A 'REFUSED: ... read head <x>, not <y> and not its parent' = the wrong run folder (exit 2). A 'STOP: mod-logs/OWED-rereads.txt
#          does not exist' = the module legs never ran in that folder (exit 1): never 'none owed'.
# 4. the C# consumer runner at that head (MS26; P read 'checks: 21 ok, 0 failed'):
( cd /h/go2cs-tmp-coord/tQ && powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/CSharpConsumer/run-csharp-consumer.ps1 ); echo rc=$?
# 5. master must not have moved:
git -C /h/Projects/go2cs ls-remote origin refs/heads/master               # EXPECT $BASE (else merge it into the union, MS11, and read again)
```
A read-only audit of the whole landing (P's returned LAND three times) is COORD's call before step 6.

```
# 6. ANNOUNCE on FLEET (the head, the battery's exit line, what each lane read), THEN:
git -C /h/go2cs-tmp-coord/tQ push origin claude/coord-trainQ-union
git -C /h/go2cs-tmp-coord/tQ push origin claude/coord-trainQ-union:master      # a fast-forward; never forced
git -C /h/Projects/go2cs ls-remote origin refs/heads/master refs/heads/claude/coord-trainQ-union   # read back: both = the refresh head
```
After it: the ledger line; the lanes release their Q worktrees; the KnownIssues entry and the Limitations page's
stale-on-landing items (the table's SLOT comment) if they did not ride; purge build output (floor 12).

## 7. The question for the owner, at the landing

Ask it with the facts on one screen; do not decide it.

> TRAIN Q is landed at `<head>`. Two ways to ship it:
> **(a) a small release now** (1.24.13.5 from Q's master): it carries the symbols in the packages (frames in a NuGet
> consumer resolve to Go file:line), the trim default for hand-written Native AOT consumers (if those rows rode: it
> makes Native AOT run on linux, and on windows the executable still fails at startup on a second defect), the
> 'safe' tag default for modules, the Go-shaped test argv, and the converter fixes of the testify / logrus / go-cmp
> rounds. It ships the shape the face lift is about to change, so first-wave packages built on it are rebuilt after.
> **(b) everything with the face-lift release**: one release, one shape, nothing rebuilt; the fixes above wait for it.
> C1's hosted gate read Q's union on four RIDs either way (5b); arms E and F are green for the first of two trains.

If the answer is (a): the runbook's release section, the environment block in the owner's shell FIRST (the Phase 0
SDK gate now refuses before anything is bumped, and prints the SDK it read), `-VerifyOnly` from a fresh shell, the
rehearsal pack and the consumer-usings test against it, then the PIN. None of it is a script of this set.
