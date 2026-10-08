# TRAIN FL (the face lift): COORD's launch checklist

**DRAFT 2026-10-08, derived from `trainQ/COORD-LAUNCH-CHECKLIST.md`.** FL re-converts the corpus; a release follows the
landing (section 8: the owner runs it). Every EXPECT below is read from git objects at the union head 14c543bb06 or
from Q's run1; none was read from an FL run. A line that prints otherwise is a STOP until it is read. Section 9 lists
the numbers this derive could NOT derive and that are read at the union.

Conventions (Q's): one command, one reading. A NEW ref is pushed, read back, THEN announced; an EXISTING ref (the
handover branch, the union branch, master) is ANNOUNCED, THEN pushed. A signature is read as its own command before any
push. After a rejected or interrupted call that posts or pushes, READ the target before reporting or re-sending.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainFL
X=/h/go2cs-tmp-coord/coord-scratch/tFL
R=$X/run1                     # a FRESH run folder for every map, assembly, fixup or battery launch (run2, run3 ...)
U=/h/go2cs-tmp-coord/tFL      # the union worktree, branch claude/coord-trainFL-union
```

## 0. Before anything: the shell, the base, the table, the frozen tree

```
env | grep -E '^(W|BASE|MASTER|EXPECT_HEAD|P_RUN|Q_RUN|MSYS_NO_PATHCONV|SEATS|RULED|MAP_FROM|MAP_UNION|FIXUP_N)='   # EXPECT no line
git -C /h/Projects/go2cs ls-remote origin refs/heads/master             # EXPECT 541766413e618ae38a684f10c1501812afc4e014 (it IS the base; if master moved, MS11)
export BASE=541766413e618ae38a684f10c1501812afc4e014
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainFL-union   # EXPECT 14c543bb06... (rows 1-10, hand-merged, signed)
git -C $U status --porcelain --untracked-files=no | grep -c .           # EXPECT 0: the battery-class run that rewrote goldens in place is OVER and restored
```
The table (`$HND/tFL-seats-draft.txt`) is frozen when **row 12 carries R's final sha**: read
`git ls-remote origin refs/heads/claude/r-facelift-docs` AFTER R posts its tip, write its first ten characters over
`SLOT`, and probe it: `git -C /h/Projects/go2cs merge-tree $(git -C /h/Projects/go2cs merge-base 14c543bb06 <R10>) 14c543bb06 <R10> | grep -c '^+<<<<<<<'`
(EXPECT 0; 6ae0918484 read 0 at the derive). A conflict is a line in `tFL-ruled.txt` BEFORE the map, or a re-cut.

| Row | Ref | Sha | State |
|---|---|---|---|
| 1 | g-caller-line-callee | b107d02e2c | merged (hand) a94b4de0be |
| 2 | g-launch-dir-remap | bc20fb77cb | merged 8b8461e56b (stack-on 1) |
| 3 | g-caller-line-per-call | 7ead2cf761 | merged 06d951c084 (stack-on 2; BOARD kept both by hand) |
| 4 | g-board-caller-line-rows | 77caa9d156 | merged 06fa3940d5 (BOARD kept both by hand) |
| 5 | c1-aot-smoke-three-rids | 4224258807 | merged ebc204b2ac |
| 6 | c1-board-aot-osx-on-q | d6bc86ffc3 | merged c77ac97c86 |
| 7 | p1-generic-instantiation-arg | ec68a9baf3 | merged 4f225be6db |
| 8 | c2-facelift-chain-q | 18e9323dc3 | merged 4613dfe676 (projitems kept both, alphabetical) |
| 9 | g-rule-tests-forward-slash | 518dfb8f0c | merged 34ccf6981f (stack-on 8) |
| 10 | c2-facelift-regen-q | 2faf310cb5 | merged 14c543bb06 (stack-on 9; cut on the union: FL9) |
| 11 | i9-aot-metadata-token-on-g | cfe3d6bae7 | TO MERGE (stack-on 3; merge-tree: clean on 14c543bb06) |
| 12 | r-facelift-docs | SLOT | TO MERGE, LAST (its sha OWED) |

## 1. The drafts on the handover branch

The derive committed them locally (signed); COORD reads the signature, announces, pushes, reads the remote back.
```
git -C /h/go2cs-tmp-coord/hnd log -1 --format='%G? %GK %s'               # ITS OWN COMMAND
```

## 2. Controls, the table check

```
( cd $HND && OUT=$X/controls-$(date +%H%M) bash ./tFL-controls.sh ) | cut -c1-200 | tail -n 27; echo rc=${PIPESTATUS[0]}
#  EXPECT 25 'CONTROL <arm>: ok' lines, then  FL-CONTROLS DONE failed=0 not-run=0   rc=0   (about 2.5 minutes)
( cd $HND && OUT=$X/selfcheck-$(date +%H%M) bash ./tFL-selfcheck.sh ) | grep -v '^    '   # EXPECT FL-SELFCHECK DONE failed=0
mkdir -p $R && cp $HND/*.sh $HND/*.py $HND/*.ps1 $HND/*.txt $HND/*.md $R/
( cd $R && BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 bash ./tFL-assemble.sh ) | tail -n 4; echo rc=${PIPESTATUS[0]}
#  EXPECT the MULTI-BASE NOTE for c2-facelift-regen-q (6 best common ancestors), then TABLE OK: 12 rows ... rc=0
#  (with row 12 still SLOT: 'ABORT: 1 seat-table problem(s)', row 12 named; the 11 other rows read TABLE OK at the derive)
```

## 3. Rows 11 and 12: the map FROM the hand union, then the assembler RESUMES

The first ten rows are hand merges; no ruled kind reproduces the two BOARD hand resolutions byte for byte, so a map
from BASE cannot equal the union's tree (FL8). The map starts AT the union and merges only rows 11 and 12; the assembler
merges the same two rows on the union branch through the same resolver. **Rows 11 and 12 are NEVER merged by hand.**
```
( cd $R && BASE=$BASE MAP_FROM=14c543bb06 bash ./tFL-conflict-map.sh ) > $R/conflict-map.console.log 2>&1; tail -n 6 $R/conflict-map.console.log
#  EXPECT 'MAP FROM 14c543bb06 ...: its 10 first-parent merges are the list's rows 1-10 in order'; SEAT 1..10 'merged at
#         MAP_FROM'; SEAT 11 and SEAT 12 'clean' (shared paths read: row 11 touches src/core/runtime/managed_impl.cs after
#         G's rows); RULED: 0 line(s) ... UNUSED: none; RULED-FILES (the BOARD's guard final); MAP DONE ... rows=12
( cd $R && BASE=$BASE bash ./tFL-assemble.sh ) > $R/assemble.console.log 2>&1; tail -n 12 $R/assemble.console.log
#  EXPECT TABLE OK; 'RESUME at 14c543bb06'; SEAT 1..10 'already merged (resume)'; SEAT 11, SEAT 12 'merged <sha>';
#         MAP TREE: EQUAL; ANCESTRY; SHAPE: 12 first-parent merges; ORDER; RULED-FILES; TREE CLEAN; NEXT: EXPECT_HEAD=<asm10>
git -C $U log --first-parent --format='%G? %GK' $BASE..HEAD | sort | uniq -c   # ITS OWN COMMAND: 12 lines, G or U, one key
```

## 4. The fixup, the push, the module rehearsal, the GOs

```
( cd $R && hp(){ python -B "$(cygpath -w $R/tFL-helpers.py)" "$@"; }; hp precheck 'H:\go2cs-tmp-coord\tFL' "$BASE" "$(cygpath -w $R/tFL-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +(COUNT|REG|BOTH))' | cut -c1-160; echo rc=${PIPESTATUS[0]} )
#  EXPECT rc=0, 'COUNTBASE multi-base row c2-facelift-regen-q', COUNT ok x6, REG ok x5, every BOTH ok (read at 14c543bb06
#         for rows 1-10 through a no-checkout probe: go2cs.slnx 962 projects, lists 819/819/819/790, projitems +17 lines)
( cd $R && BASE=$BASE EXPECT_HEAD=<asm10> FIXUP_N=1 bash ./tFL-fixup.sh ) > $R/fixup.console.log 2>&1; echo "fixup rc=$?" >> $R/fixup.console.log
```
EXPECT in `tFL-fixup-logs/SUMMARY.txt`: PRERES 0 hard failures; step 4 `union-attributable=0` on windows, linux and
darwin (row 10 IS the union converter's emission), `regen applied 0`, HANDOWN 0, the csproj word **OK** (TEMPLATE DRIFT
ONLY: new-ok 3, seat-edit GenTests.csproj; any UNEXPLAINED word STOPS the fixup, FL5); **step 4t** rows
`encoding/json:fl<n>/lines414-><left>/np0/other0 encoding/binary:fl<n>/lines21-><left>/... net/rpc:fl<n>/lines41-><left>/...`
(the leaving-attribute lines at 14c543bb06; FACELIFT n > 0 for each; OTHER read by name from `4t-<row>.patch`); step 5
CNR `NO REGRESSION` with **0 goldens moved** (a face-lift golden STOPS: GOLDEN_CLASS=facelift after reading it);
`7 COMMITTED <fix10>`.
```
git -C $U log -1 --format='%G? %GK %s'                                  # ITS OWN COMMAND: 'fixup: TRAIN FL -- ...'
# ANNOUNCE (an existing ref), then:
git -C $U push origin claude/coord-trainFL-union
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainFL-union   # the full sha IS the tip every GO names
mkdir -p $R/modrehearsal && cp $R/*.sh $R/*.py $R/*.txt $R/modrehearsal/
( cd $R/modrehearsal && EXPECT_HEAD=<fix10> BASE=$BASE CONTROLS=1 bash ./tFL-modules-legs.sh ) > $R/modrehearsal/console.log 2>&1; echo rc=$?
#  FL10, Q lesson d: every -recurse MODULE -tests row read at the fixup head BEFORE the battery (Q's refusal surfaced
#  6.5 h in). EXPECT rc 0 (7 = green with an owed re-read): MR2..MR6 and their controls PASS, MR3-tags PASS, x/sync 3
#  pass + semaphore KNOWN, XS2 equal, x/mod 9 of 9, orphans 0, tracked changes 0. rc 4 = STOP before the battery.
```
Then the GOs (each names the union tip, BASE and the handover commit; the i9's names the shard list sha256, unchanged):

| Lane | Owes, at the fixup tip | Brief |
|---|---|---|
| i9 | the 132-row shard (testing is ITS ALONE now, FL6), S2, the two patches with their sha256 | `tFL-lane-brief-i9.md` |
| P1 | the linux legs (LM: x/sync, x/sync2, x/mod read FIRST) | `tFL-lane-brief-linux.md` |
| P2 | the second linux box (GolibTests, T2, runtime/pprof, runtime FULL) | `tFL-lane-brief-linux.md` |
| C1 | release-smoke on four RIDs and aot-smoke, the darwin behavioral FULL, the darwin census, the docs-site build (MS23) | its own |
| G | the five module re-reads at the FIXUP tip (x/sync 28/28; pflag 177; cobra 405 + doc 19; logrus: Q read the root failing on ONE differ, TestNestedLoggingReportsCorrectCaller, which G's caller-line rows of THIS train are predicted to close; testify as at Q) | none |

## 5. The battery

```
cat > $R/launch-battery.sh <<'EOF'
cd /h/go2cs-tmp-coord/coord-scratch/tFL/run1 || exit 1
MASTER=<BASE, full sha> EXPECT_HEAD=<fix10> SEATS_EXPECTED=12 DEADLINE='<YYYY-MM-DD HH:MM, launch + 16 h>' bash ./tFL-battery.sh > battery.console.log 2>&1
rc=$?; echo "battery rc=$rc" >> battery.console.log; exit $rc
EOF
```
Launch it detached. The tFL worktree is FROZEN until `battery rc=` prints (floor 4). Q_RUN defaults to Q's run1.

| PRE / leg | EXPECT at the 12-row union |
|---|---|
| PRE | shape `12 seat merges + 1 fixup`; `previous-train record (TRAIN Q) Q_RUN=.../tQ/run1 (SUMMARY <k> lines)` |
| PRE-D CNR | `EXPECT N=866` + whatever rows 11/12 add (none predicted); the control at MASTER `N=863 vs TRAIN Q's battery 863`; added 3 |
| PRE-D CB | 21 `+func Test` added under src/go2cs at 14c543bb06, plus rows 11/12's, plus the base guards (derived) |
| PRE-3 | `FL6: testing on this box's run list: no` |
| C, CC, CB | ok; CB every name `--- PASS`, 0 SKIP/FAIL |
| **MOD (now right after CB)** | every verdict PASS, as the module rehearsal read; `END failures=0 external=0 owed-lines=0` |
| FX | tracked fixtures at Q's floor (1291) or above, stale 0 |
| E | plants OK, union-attributable 0 x3, handown 0, **csproj=OK** (never UNEXPLAINED-UNGATED: that word means the gate was off) |
| GN, GT, TR | GenTests Failed 0, Total >= the `[TestMethod]` lines at HEAD (166 at 14c543bb06; C2 read 188/188 at the regeneration); GolibTests the box's known trio only (C2: 1626 pass at the regeneration) |
| 4 | `NO REGRESSION ... all 866 behavioral packages`, 7 skips |
| 5 | every phase green |
| PUB / PUB2 | `6 of 6 published programs match go, none hung`; PUB2 as at Q |
| H7 x3 | rc 0 (the face-lifted corpus compiled on windows, linux, darwin) |
| PRE-PB | tracked changes 0 |
| S | every row at its banked count, `record=fresh`; HOSTWALL a prompt (Q's run1 stamps are the baseline) |
| **TE / TE-T** | FACELIFT IN BULK (5251 leaving-attribute lines in 924 committed test sources at the base: the windows sweeps re-emit their share); its steps line; OTHER read by name against Q's patch |
| T2, T | as at Q |
| END | `OWED AT THE LANDING HEAD ... owed-lines=0`; `EXIT 0` |

EXIT 6 = a red or a finding, read by name (BATTERY_RC_OK=6 at the landing only for a red ruled by name). EXIT 7 =
green and a reading OWED at the landing head.

## 6. Landing

The union lands when: the battery ended EXIT 0 (or 7, or 6 ruled by name), the i9's shard and both linux lanes read as
their briefs expect, C1's hosted gate is green on four RIDs (a release follows: MS23 is not optional), G's five modules
are in.
```
# 1. the prep, at the battery's own head (the i9's patch copied, its sha256 verified against the post)
RUN=$R MASTER=$BASE HEADFULL=<battery head, full sha> I9P=<the i9's tracked-changes.patch> I9P_SHA256=<as posted> [BATTERY_RC_OK=<n>] bash $R/tFL-land-prep.sh
#  EXPECT 'PRE battery of record ... the battery read this head itself'; 2a PRECHECK 0/0; 2b the roster guard x2;
#         4b 'REFRESH-VERDICT ... facelift=<n> ... refused=0 conflicts=0'; 4d 'ONLY committed test sources changed';
#         4e the TE line, 'TE FACELIFT steps', and 'FACE LIFT (FL1): leaving-attribute lines ... 5251-ish at HEAD, <few> in
#         the refreshed worktree'; every OTHER hunk READ by name before signing
# 2. COORD commits 'refresh: TRAIN FL -- <files> committed test sources, the windows emission at <head10>' (single-parent, signed)
git -C $U log -1 --format='%G? %GK %s'                                   # ITS OWN COMMAND
# 3. a follow-up owed after the push? It lands ON the refresh as 'fixup-2: TRAIN FL' (FL2: Q's route):
#    ( cd <fresh run folder> && BASE=$BASE EXPECT_HEAD=<refresh10> FIXUP_N=2 bash ./tFL-fixup.sh )
# 4. the re-reads at the head that lands, ONE script (FL11), from a FRESH per-run copy:
R2=$X/reread1; mkdir -p $R2 && cp $HND/*.sh $HND/*.py $HND/*.txt $R2/
RUN=$R R2=$R2 HEADFULL=<the head that lands, full sha> BASE=$BASE [BATTERY_HEAD_OK=<battery head10, when a fixup-N sits above the refresh>] [BATTERY_RC_OK=<n>] bash $R2/tFL-land-rereads.sh
#  EXPECT STEP 1 MOD rc 0; STEP 2 'FINAL READS at <head>: GREEN'; STEP 3 T2 runtime/debug PASS 8 twice, same files,
#         .pdb > 1, 0 refusals; STEP 4 PUB '6 of 6 ... none hung'; STEP 5 CONSUMER 'checks: 21 ok, 0 failed';
#         END tracked=0 deletions=0 master=<BASE10>; 'REREADS DONE: GREEN'
# 5. master must not have moved:
git -C /h/Projects/go2cs ls-remote origin refs/heads/master             # EXPECT $BASE (else MS11)
# 6. ANNOUNCE on FLEET (the head, the battery's exit line, what each lane read), THEN:
git -C $U push origin claude/coord-trainFL-union
git -C $U push origin claude/coord-trainFL-union:master                 # a fast-forward; never forced
git -C /h/Projects/go2cs ls-remote origin refs/heads/master refs/heads/claude/coord-trainFL-union   # both = the head that landed
```
After it: the ledger line; the lanes release their FL worktrees (children first); purge build output (floor 12).

## 8. The release (a separate runbook step: the owner runs it)

Not a script of this set. `docs/GoCorpusMigration.md`'s release section, from a fresh shell with the environment block
in the owner's shell FIRST (release-nuget.ps1's Phase 0 reads the SDK); `-VerifyOnly`; the rehearsal pack and the
consumer test against it; then the owner's PIN. C1's `published_version` arms read nuget.org AFTER the release.

## 9. What this derive could NOT derive (read at the union)

- **Row 12's sha** and what R's final tip adds (projitems lines, a repoguard test name for CB, conflicts): owed by R.
- **Everything rows 11 and 12 add to a count**: the registration counts and CB's names at the fixup head (derived at
  run time from the tree; the derive read rows 1-10 only), GenTests' and GolibTests' totals there.
- **The face lift's volume per box**: how many FACELIFT hunks and lines the i7's S and T sweeps, the i9's shard and
  step 4t each carry (only the base census, 5251 lines in 924 files, and the three 4t rows' 414 / 21 / 41 are read).
- **Whether the union CNR at the fixup head moves 0 goldens**: predicted 0 (row 10 re-baselined at its own head);
  it was not run, and the tFL worktree's in-progress run is the first reading.
- **HOSTWALL and wall times**: the face lift changes no code path on purpose; not measured.
- **C1's hosted readings, the darwin legs, G's four third-party modules at FL**: lane readings, predicted from Q.
