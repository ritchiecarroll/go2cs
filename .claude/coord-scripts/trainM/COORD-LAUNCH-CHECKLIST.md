# TRAIN M: COORD's launch checklist, from the assembled union to the battery (written 2026-10-02 08:55)

Git Bash on the i7. Every command matches the round-3 draft (`wf/draft/`). State read at 08:50: the tM worktree is at
the ASSEMBLED head **d4aae0aca0** on `claude/coord-trainM-union` (24 seat merges, `MAP TREE: EQUAL to 11c188daf3`,
`asm1/assemble.log`); no fixup yet; no `.battery.lock`; the follow list is EMPTY; H: 3550 G free, C: 88 G.
One heavy job at a time on this box (floors 1, 11). A reading that differs from an EXPECT line is a STOP.

```
D=/h/go2cs-tmp-coord/coord-scratch/tM/wf/draft
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainM
X=/h/go2cs-tmp-coord/coord-scratch/tM
R=$X/run1                                   # a FRESH run folder: the fixup AND the battery run from it
hp(){ python -B "$(cygpath -w "$R/tM-helpers.py")" "$@"; }
```

## 0. State (seconds)
```
git -C /h/go2cs-tmp-coord/tM rev-parse --short=10 HEAD                   # EXPECT d4aae0aca0
git -C /h/go2cs-tmp-coord/tM status --porcelain | grep -vc '^??'         # EXPECT 0
ls -d $X/.battery.lock                                                   # EXPECT: No such file or directory
grep -E '^[A-Za-z0-9._-]+\|' $HND/tM-seats-draft.txt | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12   # EXPECT 0b3fd4358320
```
STOP if the list hash differs: the list moved after the freeze, and a new map and a new assembly come first.
`g-lazy-callers|483d4ea217` (last row, `stack-on g-godebug-pc-line`) can be added ONLY HERE, before the fixup: add the
row, run per-run copies of `tM-conflict-map.sh` (`SEATS=$HND/tM-seats-draft.txt`; EXPECT `MAP DONE ... rows=25 ...
every row clean`) and `tM-assemble.sh` (a resume: EXPECT `SEAT 25 ...: merged`, `MAP TREE: EQUAL`), then use the NEW
head and rows hash everywhere below.

## 1. The draft into hnd (the lanes read it from origin), then the run folder (floor 4)
```
cp -r $D/. $HND/                                                         # the seat list is not in the draft: untouched
git -C /h/go2cs-tmp-coord/hnd add .claude/coord-scripts/trainM
git -C /h/go2cs-tmp-coord/hnd commit -S -m "coord-scripts: TRAIN M script set (verify round 3; the seat list frozen at 24 rows)"
git -C /h/go2cs-tmp-coord/hnd push origin claude/coord-handover          # announce, then push (floor 9)
[ ! -e $R ] && mkdir $R && cp -r $HND/. $R/ && ls $R/tM-seats-draft.txt $R/tM-follow.txt $R/tM-helpers.py
grep -vE '^[[:space:]]*(#|$)' $R/tM-follow.txt | tr -d '\r' | sha256sum | cut -c1-12      # EXPECT e3b0c44298fc (EMPTY)
```

## 2. Controls, once, before the fixup (floor 13; all read-only except 2e, which aborts before creating anything)
```
hp teattr    "$(cygpath -w $R/controls/te-pos-goframe.patch)"
#  EXPECT  TE files=3 hunks=5 g-frame=4 (noinline-lines=2 using-lines=2, in 2 file(s); g-frame-only files=2) map-only=1 other=0
hp hunkclass "$(cygpath -w $R/controls/te-pos-goframe.patch)"; echo rc=$?
#  EXPECT  HUNKCLASS-VERDICT files=3 hunks=5 other-files=0 ...   rc=0
hp teattr    "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"
#  EXPECT  TE files=1 hunks=25 g-frame=0 (noinline-lines=0 using-lines=0, in 0 file(s); g-frame-only files=0) map-only=0 other=25
hp hunkclass "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"; echo rc=$?
#  EXPECT  HUNKCLASS-VERDICT files=1 hunks=25 other-files=1 ...   rc=1
# 2b. precheck on the assembled union (the fixup's PRERES, predicted)
hp precheck 'H:\go2cs-tmp-coord\tM' aa0a07d5fd "$(cygpath -w $R/tM-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +H4)'; echo rc=${PIPESTATUS[0]}
#  EXPECT  rc=0 and exactly three lines:  ok   H4 Go-only behavioral directories (tracked *.go, no tracked *.cs): 0
#          NOTE H5 docs/ValidatedTestPackages.md: the prose says "Three rows opt back out" while 1 row(s) carry ...
#          PRECHECK hard-failures=0 notes=1 mode=head
# 2c. the same reader made to FAIL, on the base: the main checkout (git -C /h/Projects/go2cs rev-parse --short=10 HEAD = aa0a07d5fd)
hp precheck 'H:\Projects\go2cs' aa0a07d5fd "$(cygpath -w $R/tM-seats-draft.txt)" head | grep -c '^FAIL'
#  EXPECT  a count above 24 (24 lines 'FAIL BOTH <seat>: no first-parent merge', FAIL COUNT, FAIL REG, FAIL PROOF,
#          FAIL ROSTER execution annotations: internal/godebug[release-tiered] log/slog[release-tiered] net/http[release-tiered], FAIL H3)
# 2d. the LIVE gate
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock
hp live 'H:\go2cs-tmp-coord\coord-scratch\tM\.battery.lock' 1; echo rc=$?     # EXPECT one 'LIVE lock ...tZ...' line, rc=1
rmdir /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock /h/go2cs-tmp-coord/coord-scratch/tZ
hp live 'H:\go2cs-tmp-coord\coord-scratch\tM\.battery.lock' 1; echo rc=$?     # EXPECT 'LIVE-VERDICT none ...', rc=0 (an idle box)
```
2e. the assembly's map refusal (new in round 3; safe only now, with all 24 rows merged and no fixup):
```
grep -v '^p1-warnings-tranche1|' $R/tM-seats-draft.txt > $R/c4-seats.txt
( cd $R && SIGN_PROBE=0 SEATS=$R/c4-seats.txt bash ./tM-assemble.sh ) | tail -n 2; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: 23 rows (c4-seats.txt) rows-sha256=<h> ...  then
#          ABORT: no map run is recorded for THIS list (rows-sha256=<h>, 23 rows: no refs/coord/tM-map/rows-<h>, and no MAP_UNION given). ...   rc=2
git -C /h/go2cs-tmp-coord/tM rev-parse --short=10 HEAD                        # EXPECT d4aae0aca0 still
```
2f. OPTIONAL, the H4 arm's firing control (without it that arm has never been made to fail). A copy of the list with
row 18 at `5556ef86cb`, on a checkout of the 06:31 list's union:
```
sed 's/^p2-test-overload-references|e002a552a8|/p2-test-overload-references|5556ef86cb|/' $R/tM-seats-draft.txt > $R/h4-seats.txt
git -C /h/Projects/go2cs worktree add --detach /h/go2cs-tmp-coord/tM-h4ctl 382ceeb838
hp precheck 'H:\go2cs-tmp-coord\tM-h4ctl' aa0a07d5fd "$(cygpath -w $R/h4-seats.txt)" head | grep '^FAIL'
#  EXPECT  exactly ONE line:  FAIL H4 Go-only behavioral directories (tracked *.go, no tracked *.cs): 2 ...TestNeedsTransitiveApiRef/a ...TestNeedsTransitiveApiRef/b
git -C /h/Projects/go2cs worktree remove /h/go2cs-tmp-coord/tM-h4ctl
```

## 3. The fixup (about 1 h 25 m; takes the lock; its own steps 4 and 5 convert and build)
Values:
- `EXPECT_HEAD=d4aae0aca0` (step 0's reading).
- `PROSE_PATCH=$X/fix1/prose.patch` (M12 + M3, cut on d4aae0aca0 by `prose_patch.py`). Check it first, read-only:
  `git -C /h/go2cs-tmp-coord/tM apply --check "$(cygpath -w $X/fix1/prose.patch)"; echo rc=$?` (EXPECT rc=0).
- `EXP_DESC`, `EXP_TH`: **TO BE SET BY COORD**, or leave unset (reported, not asserted). To derive (read-only):
  `python "$(cygpath -w /h/go2cs-tmp-coord/tM/docs/phase4/recipes/quickjit-optin/optin.py)" check 'H:\go2cs-tmp-coord\tM' | tail -n 2`
  prints `check: <EXP_DESC> template descendants, <EXP_TH> test hosts, 1 hand-owned skipped` then `CHECK PASS` (L: 1140 / 229).
- Defaults kept: `REGEN=apply GOLDENS=regen SIBLINGS=report GOLDEN_CLASS=gframe FIXUP_N=1`; `REGEN_ALLOW` composed;
  `EXEC_ROWS_EXPECT` from the helper (1). Optional: `SEATS_EXPECTED=24`.
```
cd $R && EXPECT_HEAD=d4aae0aca0 PROSE_PATCH=$X/fix1/prose.patch SEATS_EXPECTED=24 bash ./tM-fixup.sh > fixup.console.log 2>&1; echo "fixup rc=$?" >> fixup.console.log
```
(run it in the background; read `$R/tM-fixup-logs/SUMMARY.txt`). EXPECT, in order:
- `PRE head=d4aae0aca0 seats=24 ... first-parent=24 follow-ups=[] ... lists=rows:0b3fd4358320/follow:e3b0c44298fc`
- `PRE derived (R1): REGEN_ALLOW=/^src/core/(go/internal/srcimporter/srcimporter\.cs|net/http/[^/]+|internal/reflectlite/[^/]+)$/`
- `PRE assembly: head=d4aae0aca0 rows=24 rows-sha256=0b3fd4358320 :: MAP TREE: EQUAL to 11c188daf3 ...`
- `PRERES precheck PRECHECK hard-failures=0 notes=1 mode=worktree`, the roster guard x2 with `1 with an execution config`
- `0b prose: 2 file(s) [docs/ValidatedTestPackages.md src/_roster.ps1 ] ...`
- `1 S1: COUNT=0 ...`, `3 optin check: CHECK PASS ...`, `4 regen: applied <n> file(s) ...`, `5 goldens: ...`
  (PREDICTED: ForeverWaitWorkersMainReturns, MainSelectForeverWorkerExits), `6 shape: ... hard-failures=0 notes=0`
- last: `FIXUP DONE head=<10 hex> subject='fixup: TRAIN M' lists=rows:0b3fd4358320/follow:e3b0c44298fc`, rc 0.
STOP conditions: any `ABORT at step <n>` (exit 2; the worktree edits so far and the discard command are stamped).
A stop at its step 4 on a path outside `REGEN_ALLOW` (Extension A's third file) is COORD's ruling: relaunch the same
command with `REGEN_ALLOW='<pattern>'`; the emission evidence in `/h/go2cs-tmp-coord/tMemitfix` is re-used (minutes).

## 4. After the fixup: push, then GO (before the battery, so the lanes run beside it)
```
git -C /h/go2cs-tmp-coord/tM log -1 --format='%H %G? %s' | cut -c1-120       # EXPECT <40 hex> G fixup: TRAIN M -- ...
git -C /h/go2cs-tmp-coord/tM status --porcelain | grep -vc '^??'              # EXPECT 0
UNION=$(git -C /h/go2cs-tmp-coord/tM rev-parse HEAD)                          # the GO's UNION and FIXUP (one sha)
git -C /h/go2cs-tmp-coord/tM push origin claude/coord-trainM-union            # announce, then push; never replaced afterwards
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainM/tM-i9-shard.txt | sha256sum | cut -d' ' -f1   # EXPECT 057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainM/tM-i9-shard.txt | grep -c .                   # EXPECT 132
git -C /h/go2cs-tmp-coord/hnd rev-parse --short=10 HEAD                       # the briefs' <HND> value
```
GO to the i9 (`tM-lane-brief-i9.md`): `UNION`, `FIXUP`, `EXPECT_LIST_SHA` (the line above), `EXPECT_ROWS=132`, the
handover tip; FOUR files in its run folder (driver, shard list, seat list, `tM-helpers.py`); python 3.8+; the uuid-pack
step for c2-s2-source-metadata is **TO BE WRITTEN BY COORD** with that seat's numbers. GO to P1 and P2
(`tM-lane-brief-linux.md`): `UNION`, `FIXUP`, the handover tip; three files. Both briefs still hold the placeholders
`<UNION: 40 hex ...>`, `<FIXUP: 40 hex>` and `<HND: filled in by COORD>`.

## 5. The battery (INFERRED 10.5 to 11.5 h; the tM worktree is FROZEN while it runs)
Preflight: `ls -d $X/.battery.lock` (absent); `ls $R/tM-logs $R/mod-logs` (absent: the fixup wrote `tM-fixup-logs/`
only); free disk at least 30 G on H: and 8 G on C: (each leg checks); nothing else converting or building on the box
(the battery reads the LIVE gate itself; `LIVE_ACK=1` only by COORD's explicit call).
Values: `EXPECT_HEAD` = the first 10 chars of `$UNION`. `CNR_EXPECT_N`: **TO BE SET BY COORD**, INFERRED 785 (791
directories with Go source at the union, read with git, minus TRAIN L's 6 platform skips, not re-measured); unset =
reported. `DEADLINE='YYYY-MM-DD HH:MM'` only if launch + 14 h does not suit. `REALMOD_TEST_TIMEOUT`: leave at `2m`
(F4's verdict exists only there). `I9_PATCH` when the i9's patch has arrived. Optional: `SEATS_EXPECTED=24`.
```
cd $R && EXPECT_HEAD=${UNION:0:10} SEATS_EXPECTED=24 [CNR_EXPECT_N=785] bash ./tM-battery.sh > battery.console.log 2>&1; echo "battery rc=$?" >> battery.console.log
```
(OS-detached, as TRAIN L's; read `$R/tM-logs/SUMMARY.txt`). EXPECT within the first minute:
- `PRE union shape OK: first-parent=25 (= 24 seat merges + 0 follow-up merge(s): + 1 fixup commit(s): ...`
- `PRE head=<10 hex> ... seats=24 (tM-seats-draft.txt) lists=rows:0b3fd4358320/follow:e3b0c44298fc ...`
- `PRE-D CB: 73 test(s) the union adds under src/go2cs + 6 base tree guards = ...`
- `PRE-D execution configs ...: rows that keep an annotation (EXPECT tiered=True)=[log/slog] rows that drop it (EXPECT tiered=False)=[internal/godebug net/http]; EXEC_ROWS_EXPECT=1`
- `PRE-1 precheck rc=0 :: PRECHECK hard-failures=0 notes=0 mode=head` (notes=1 if the prose patch did not ride)
STOP conditions (the exit status): **2** an ABORT in PRE (read the line; nothing ran); **3** a leg's disk or build
abort; **5** a wall cap fired OR a process is still running from `$R/out` after MOD: the lock is KEPT, kill the listed
PIDs by PID (`tM-logs/timeout-orphans-*.txt` / `orphans-MOD.txt`), `rmdir $X/.battery.lock`, relaunch from a FRESH
folder; **9** the deadline; **6** at END = a leg outside FXc1 / FXc2 / SIc / NVR is non-zero or a FINDING exists (read
the NONZERO LEGS and FINDINGS lines); **0** clean. In MOD, `MR4c` and `MR6c` reading NOT MEASURED or FAIL is read as
the INSTRUMENT first (`mod-logs/ctl-reader-m3.log`, `conv-build-m3.log`, `-m4`): their patched converters have never
been built. A red needing a tree change after the push is a `fixup-2` ON TOP (`FIXUP_N=2`), never a replaced sha.
