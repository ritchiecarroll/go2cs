# TRAIN T3: COORD's launch checklist

**DRAFT 2026-10-10, derived from `trainFL/COORD-LAUNCH-CHECKLIST.md`.** Every EXPECT is read from git objects at the
assembled union 498de280bf or from FL's run2. A line that prints otherwise is a STOP until it is read. FL's conventions
hold: one command, one reading; a NEW ref is pushed, read back, THEN announced; an EXISTING ref is ANNOUNCED, THEN
pushed; a signature is read as its own command before any push.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainT3   # once COORD commits the kit there (step 1)
X=/h/go2cs-tmp-coord/coord-scratch/tT3
R=$X/run1                     # the derive's run folder (map + assembly done); a FRESH run folder for any re-run
U=/h/go2cs-tmp-coord/tT3      # the union worktree, LOCAL branch train-t3-union (pushed as claude/coord-trainT3-union)
```

## 0. The shell, the base, the table

```
env | grep -E '^(W|BASE|MASTER|EXPECT_HEAD|FL_RUN|Q_RUN|P_RUN|MSYS_NO_PATHCONV|SEATS|RULED|MAP_FROM|MAP_UNION|FIXUP_N|I9_TREE|TP_PIN)='   # EXPECT no line
git -C /h/Projects/go2cs ls-remote origin refs/heads/master             # EXPECT 4e6322d770bbe52ca0dd47dbca2b3c3392a4133f (else MS11)
export BASE=4e6322d770bbe52ca0dd47dbca2b3c3392a4133f
git -C $U rev-parse HEAD HEAD^{tree}                                   # EXPECT 498de280bf6ac4f6a5670cd681fcfeb4a1ba5427 / 466166dd21c2...
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/i9-union-t3-ref refs/heads/claude/g-trim-probes   # EXPECT 71e5f69dda... / 04239ec85e...
```
The table (`tT3-seats-draft.txt`, 24 rows) is FROZEN; the assembler re-reads every tip at the remote (TABLE OK).

## 1. The kit on the handover branch

Copy `$R`'s kit files (not its logs) to `$HND`, commit signed, read the signature, announce, push.

## 2. Controls

```
( cd $R && OUT=$X/controls-$(date +%H%M) TE_CONTROLS=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainP/controls bash ./tT3-controls.sh ) | cut -c1-200 | tail -n 29; echo rc=${PIPESTATUS[0]}
#  EXPECT 27 'CONTROL <arm>: ok' lines (FL-OFF, TABLE, I9, CN, TP, GN new; RT-real and FX9 on FL's records), then
#         T3-CONTROLS DONE failed=0 not-run=0   rc=0   (about 8 minutes: TABLE runs the assembler's table check 4 times)
( cd $R && OUT=$X/selfcheck-$(date +%H%M) bash ./tT3-selfcheck.sh ) | grep -v '^    '   # EXPECT T3-SELFCHECK DONE failed=0
```

## 3. The map and the assembly (DONE by the derive; re-run only on a moved master or a changed table)

```
( cd $R && BASE=$BASE SEATS=$R/tT3-seats-draft.txt bash ./tT3-conflict-map.sh ) > $R/conflict-map.console.log 2>&1; tail -n 4 $R/conflict-map.console.log
#  EXPECT SEAT 1..24 clean; RULED 0; MAP DONE head=5caf722fc6 rows=24 rows-sha256=4dbb292d6358
( cd $R && BASE=$BASE SEATS=$R/tT3-seats-draft.txt I9_TREE=466166dd21 bash ./tT3-assemble.sh ) > $R/assemble.console.log 2>&1; tail -n 14 $R/assemble.console.log
#  EXPECT TABLE OK: 24 rows; SEAT 20 'in-head 07411b26f7 verified'; ASSEMBLED head=498de280bf...; MAP TREE: EQUAL;
#         I9 TREE: EQUAL -- ... tree 466166dd21 = the i9 union 71e5f69dda tree 466166dd21; SHAPE 24; ORDER; TREE CLEAN
git -C $U log --first-parent --format='%G? %GK' $BASE..HEAD | sort | uniq -c   # ITS OWN COMMAND: 24 lines, one key
```

## 4. The fixup, the push, the module rehearsal

```
( cd $R && hp(){ python -B "$(cygpath -w $R/tT3-helpers.py)" "$@"; }; hp precheck 'H:\go2cs-tmp-coord\tT3' "$BASE" "$(cygpath -w $R/tT3-seats-draft.txt)" head | grep -E '^(FAIL|PRECHECK)'; echo rc=${PIPESTATUS[0]} )
#  EXPECT PRECHECK hard-failures=0 notes=0 mode=head, rc=0 (the derive read this at 498de280bf)
( cd $R && BASE=$BASE EXPECT_HEAD=498de280bf FIXUP_N=1 bash ./tT3-fixup.sh ) > $R/fixup.console.log 2>&1; echo "fixup rc=$?" >> $R/fixup.console.log
```
EXPECT in `tT3-fixup-logs/SUMMARY.txt`: PRERES 0; step 4 `union-attributable=0` on windows, linux and darwin, `regen
applied 0`, HANDOWN 0, csproj **OK**; step 4t `encoding/json:np0/other0 encoding/binary:np0/other0 net/rpc:np0/other0`
(no T3 row changes -tests emission); step 5 CNR `NO REGRESSION`, **0 goldens moved**; `7 COMMITTED <fix10>`.
```
git -C $U log -1 --format='%G? %GK %s'                                  # ITS OWN COMMAND: 'fixup: TRAIN T3 -- ...'
git -C $U push origin train-t3-union:claude/coord-trainT3-union        # a NEW ref: push, read back, THEN announce
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainT3-union
mkdir -p $R/modrehearsal && cp $R/*.sh $R/*.py $R/*.txt $R/modrehearsal/
( cd $R/modrehearsal && EXPECT_HEAD=<fix10> BASE=$BASE CONTROLS=1 bash ./tT3-modules-legs.sh ) > $R/modrehearsal/console.log 2>&1; echo rc=$?
#  EXPECT rc 0 (7 = green with an owed re-read). Row 8 changes the position-map identity of MODULE-CACHE sources, the
#  exact input of these rows: this is the first reading of that change on real modules. rc 4 = STOP before the battery.
```

## 5. The probe worktree, then the battery

```
git -C /h/Projects/go2cs fetch -q origin claude/g-trim-probes
git -C /h/Projects/go2cs worktree add --detach /h/go2cs-tmp-coord/tT3-probes 04239ec85e
git -C /h/go2cs-tmp-coord/tT3-probes rev-parse HEAD                    # EXPECT 04239ec85e...
EXPECT_HEAD=<fix10> bash $R/launch-battery.sh                           # detached; DEADLINE = launch + 16 h; TP_PIN=04239ec85e
```
The tT3 worktree is FROZEN until `battery rc=` prints (floor 4).

| PRE / leg | EXPECT at the 24-row union |
|---|---|
| PRE | shape `24 seat merges + 1 fixup`; `previous-train record (TRAIN FL) FL_RUN=.../tFL/run2` |
| PRE-D CNR | `EXPECT N=871`; the control at MASTER `N=866 vs TRAIN FL's battery 866`; added 5 |
| PRE-D CNR-FRESH | `the CNR script's change is row 5's RULED alias-drift entries and nothing else (names=OsGetpagesize SyscallKeystonePulls other=0 removed=0 ...): admitted` |
| PRE-D CB | 24 `+func Test` added + 6 base guards = 30 names |
| C, CC, CB, MOD | ok; MOD every verdict PASS |
| E | union-attributable 0 x3, csproj OK |
| GN | Total >= 208 (floor 189), Failed 0; `GN trim guards (G's, by name): 15 of 15 read, not declared [], not Passed []` |
| TP | `TP rows: consumer:0/0/RUNS-CLEAN/... genprobe:0/0/GO-EQUAL/... c32a c32b c32c GO-EQUAL :: TP rows=5 ok=5` |
| GT | the trim classes (TrimStage3aTests, TrimStage3c1Tests, TrimStage3c2aTests, TrimStage3c2bTests, GoZeroConstructionGuardTests) at their floors |
| G1/G2 | the roster guard, 2110 checks under both editions |
| 4 | `NO REGRESSION ... all 871 behavioral packages`, 7 skips |
| TE / TE-T / TE-i9 | ~0 content hunks; OTHER read by name against FL's patches |
| END | `EXIT 0` (7 = an owed re-read) |

## 6. Landing

FL's section 6 with the names moved (`tT3-land-prep.sh`, then `refresh: TRAIN T3` only if the sweeps rewrote anything,
`tT3-land-rereads.sh`, master unmoved, announce, push `claude/coord-trainT3-union` and `...:master` as a fast-forward).

## 9. What this derive could NOT derive

- **The fixup's own head** (EXPECT_HEAD), and so the battery's start.
- **TP's real verdicts** (a publish of five programs against the union: the battery's first reading on this box).
- **MOD at T3** (row 8 moves module-cache source identity: first read by the module rehearsal).
- **HOSTWALL and wall times**: not measured; FL's run took 10 h 43 m.
