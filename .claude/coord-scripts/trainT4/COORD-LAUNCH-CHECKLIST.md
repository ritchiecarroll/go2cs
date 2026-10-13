# TRAIN T4: COORD's launch checklist

**DRAFT 2026-10-12, derived from `trainT3/COORD-LAUNCH-CHECKLIST.md`.** Every EXPECT is read from git objects at the
assembled union 7dfd1d493c, from the derive's own runs, or from T3's run2. A line that prints otherwise is a STOP until
it is read. T3's conventions hold: one command, one reading; a NEW ref is pushed, read back, THEN announced; an EXISTING
ref is ANNOUNCED, THEN pushed; a signature is read as its own command before any push.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainT4   # once COORD commits the kit there (step 1)
X=/h/go2cs-tmp-coord/coord-scratch/tT4
R=$X/run1                     # the derive's folder (the kit, its logs)
G=$R/go                       # the per-run copy the map, the assembly, the fixup ran in; the battery runs here too
U=/h/go2cs-tmp-coord/tT4      # the union worktree, LOCAL branch train-t4-union (pushed as claude/coord-trainT4-union)
```

## 0. The shell, the base, the table

```
env | grep -E '^(W|BASE|MASTER|EXPECT_HEAD|FL_RUN|T3_RUN|Q_RUN|P_RUN|MSYS_NO_PATHCONV|SEATS|RULED|MAP_FROM|MAP_UNION|FIXUP_N|I9_TREE|TP_PIN|TIP_MOVED_OK)='   # EXPECT no line
git -C /h/Projects/go2cs ls-remote origin refs/heads/master             # EXPECT 226afb810a21408ebda2412dd74d526345c46f65 (else MS11)
export BASE=226afb810a21408ebda2412dd74d526345c46f65
git -C $U rev-parse HEAD HEAD^{tree}                                   # EXPECT the fixup commit (section 4) / 0145a977f02e3a5c3d61070640356b4b96dbcf0f
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/g-gocmp-perf-floor refs/heads/claude/g-trim-probes-more   # EXPECT 701430cf6d... (row 5's tip, ONE docs commit past the seat a5ef12237c) / 5f363aeae4...
```
The table (`tT4-seats-draft.txt`, 11 rows) is FROZEN; the assembler re-reads every tip at the remote (TABLE OK; row 5
acknowledged by the ruled TIP_MOVED_OK default, its one commit listed).

## 1. The kit on the handover branch

Copy `$R`'s kit files (not its logs, not `probe0/`, `ctl1/`, `go/`) to `$HND`, commit signed, read the signature,
announce, push.

## 2. Controls

```
( cd $G && OUT=$X/controls-$(date +%H%M) TE_CONTROLS=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainP/controls bash ./tT4-controls.sh ) | cut -c1-200 | tail -n 28; echo rc=${PIPESTATUS[0]}
#  EXPECT 27 'CONTROL <arm>: ok' lines, then T4-CONTROLS DONE failed=0 not-run=0   rc=0   (about 6 minutes; the derive read exactly this)
( cd $G && OUT=$X/selfcheck-$(date +%H%M) bash ./tT4-selfcheck.sh ) | grep -v '^    '   # EXPECT T4-SELFCHECK DONE failed=0
```
NOTE: the controls' planted SUMMARY files read as a LIVE run to the fixup's and the battery's LIVE gate for 15 minutes
after they are written (the derive's first fixup launch aborted on exactly that: `fixup.console.abort1.log`). Wait
15 minutes after the controls before a heavy launch, or delete the controls' scratch folder.

## 3. The map and the assembly (DONE by the derive; re-run only on a moved master or a changed table)

```
( cd $G && BASE=$BASE SEATS=$G/tT4-seats-draft.txt bash ./tT4-conflict-map.sh ) > $R/conflict-map.console.log 2>&1; tail -n 4 $R/conflict-map.console.log
#  EXPECT SEAT 1..11 clean except SEAT 3 'RULED resolution' (kind=theirs x2); SEAT 5 'NOTE branch tip 701430cf6d != seated a5ef12237c';
#         RULED 2 line(s), 1 merge(s) resolved by rule, UNUSED none; MAP DONE head=789c1ac576 rows=11 rows-sha256=330f1ce10728 ruled-sha256=452cce41f0e6
( cd $G && env -u TIP_MOVED_OK BASE=$BASE SEATS=$G/tT4-seats-draft.txt I9_TREE=7e1f0969c1 bash ./tT4-assemble.sh ) > $R/assemble.console.log 2>&1; tail -n 14 $R/assemble.console.log
#  EXPECT TABLE OK: 11 rows (row 5 acknowledged, '1 commit(s) past the seat: 701430cf6d'); SEAT 3 'ruled theirs ... = the row's blob' x2;
#         ASSEMBLED head=7dfd1d493c0453db63b0c7d5f0673edd4324d018; MAP TREE: EQUAL; I9 TREE: EQUAL -- 7dfd1d493c tree 7e1f0969c1;
#         SHAPE 11; ORDER; RULED-FILES slnx 969 (+2), lists 826/826/826/797; RULED 2 used; TREE CLEAN
git -C $U log --first-parent --format='%G? %GK' $BASE..train-t4-union~1 | sort | uniq -c   # ITS OWN COMMAND: 11 lines, one key (the derive read '11 U 941694536F21BAFF')
```

## 4. The fixup, the push, the module rehearsal (the fixup and the rehearsal DONE by the derive)

```
( cd $G && hp(){ python -B "$(cygpath -w $G/tT4-helpers.py)" "$@"; }; hp precheck 'H:\go2cs-tmp-coord\tT4' "$BASE" "$(cygpath -w $G/tT4-seats-draft.txt)" head | grep -E '^(FAIL|PRECHECK)'; echo rc=${PIPESTATUS[0]} )
#  EXPECT PRECHECK hard-failures=0 notes=0 mode=head, rc=0 (the derive read this at 7dfd1d493c; at the fixup head the projitems COUNT reads fixup:+1)
( cd $G && BASE=$BASE EXPECT_HEAD=7dfd1d493c FIXUP_N=1 bash ./tT4-fixup.sh ) > $R/fixup.console.log 2>&1; echo "fixup rc=$?" >> $R/fixup.console.log
```
EXPECT in `$G/tT4-fixup-logs/SUMMARY.txt`: PRERES precheck 0 hard failures; the roster guard 2115 under both editions,
0 execution configs; `PRERES-reg (T4-5 ...)` +1 line, CONTROL FIRED before, 5 TestProjitems* PASS after; step 4
`union-attributable=0` on windows, linux and darwin, `regen applied 0`, HANDOWN 0, csproj OK; step 4t
`encoding/json:np0/other0 encoding/binary:np0/other0 net/rpc:np0/other0`; step 5 PREDICTION [RawStringLineEndings
X509SystemVerify], moved exactly those two, 6 files, each project's four phases green; `7 COMMITTED <fix10>`, 7 files;
the commit's tree **0145a977f02e3a5c3d61070640356b4b96dbcf0f** (the i9's).
```
git -C $U log -1 --format='%G? %GK %s'                                  # ITS OWN COMMAND: 'fixup: TRAIN T4 -- ...'
git -C $U rev-parse HEAD^{tree}                                        # EXPECT 0145a977f02e3a5c3d61070640356b4b96dbcf0f
git -C $U push origin train-t4-union:claude/coord-trainT4-union        # a NEW ref: push, read back, THEN announce
git -C /h/Projects/go2cs ls-remote origin refs/heads/claude/coord-trainT4-union
mkdir -p $R/modrehearsal && cp $G/*.sh $G/*.py $G/*.txt $R/modrehearsal/
( cd $R/modrehearsal && EXPECT_HEAD=<fix10> BASE=$BASE CONTROLS=1 bash ./tT4-modules-legs.sh ) > $R/modrehearsal/console.log 2>&1; echo rc=$?
#  EXPECT rc 0 (7 = green with an owed re-read). rc 4 = STOP before the battery.
```

## 5. The probe worktree, then the battery

```
git -C /h/Projects/go2cs fetch -q origin claude/g-trim-probes-more
git -C /h/Projects/go2cs worktree add --detach /h/go2cs-tmp-coord/tT4-probes 5f363aeae4
git -C /h/go2cs-tmp-coord/tT4-probes rev-parse HEAD                    # EXPECT 5f363aeae43774e45205f916cd8e1a798190e4f9
EXPECT_HEAD=<fix10> bash $R/launch-battery.sh                           # detached; runs in $G; DEADLINE = launch + 16 h; TP_PIN=5f363aeae4
```
The tT4 worktree is FROZEN until `battery rc=` prints (floor 4).

| PRE / leg | EXPECT at the 11-row union |
|---|---|
| PRE | shape `11 seat merges + 1 fixup`; `previous-train record (TRAIN T3) T3_RUN=.../tT3/run2`; lists rows 330f1ce10728 / follow e3b0c44298fc |
| PRE-D CNR | `EXPECT N=873` (880 enumerated, 7 skips: MulticastGroupJoin NativeFieldPortAlias ScmRightsSeam SendtoSeam SetegidBroadcastSeam UnixAbstractAddrName WritevIovecSeam); the control at MASTER `N=871 vs TRAIN T3's battery 871`; added 2 (RawStringLineEndings X509SystemVerify), removed 0 |
| PRE-D CNR-FRESH | `(Q14) ... [none] (EXPECT none)`; no T4-10 finding (no row changes the CNR script or _paths.ps1) |
| PRE-D CB | 31 `+func Test` added + 6 base guards = 37 names (0 dropped) |
| PRE-D GT | added classes FieldBoxTableRouteTests GoroutineCreatorSiteTests PromotedFieldIdentityTests RawStringLiteralTests (+ T3's neighbours) |
| PRE-1 | precheck 0 hard failures (COUNT x6, REG x6, BOTH, ROSTER, H3 with g-golib-creator-o1, H4 0) |
| C, CC, CB, MOD | ok; MOD every verdict PASS (the derive's module rehearsal: section 4) |
| E | union-attributable 0 x3, csproj OK |
| GN | Total >= 214 (floor 195), Failed 0; `GN trim guards (G's, by name): 21 of 21 read, not declared at HEAD [], not Passed []` |
| TP | `TP rows: consumer:0/0/RUNS-CLEAN/... genprobe c32a c32b c32c registry stringer fieldbox :0/0/GO-EQUAL :: TP rows=8 ok=8 names=8 off=[]` |
| GT | the trim classes and the four added classes at their floors |
| G1/G2 | the roster guard, 2115 checks under both editions (225 rows, 0 with an execution config) |
| 4 | `NO REGRESSION ... all 873 behavioral packages`, 7 skips |
| TE / TE-T / TE-i9 | ~0 content hunks; OTHER read by name against T3's run2 patches |
| END | `EXIT 0` (7 = an owed re-read) |

## 6. Landing

T3's section 6 with the names moved (`tT4-land-prep.sh` with `MASTER_MOVED_OK` if master moved by non-converter
commits, the refresh only if the sweeps rewrote anything, `tT4-land-rereads.sh`, announce, push
`claude/coord-trainT4-union` and `...:master` as a fast-forward).

## 9. What this derive could NOT derive

- **TP's real verdicts** (a publish of eight programs against the union: the battery's first reading on this box).
- **The roster guard's 2115** was read by the fixup's PRERES on this box (see its SUMMARY); G1/G2 re-read it.
- **HOSTWALL and wall times**: not measured; T3's battery ran about 11.5 h.
