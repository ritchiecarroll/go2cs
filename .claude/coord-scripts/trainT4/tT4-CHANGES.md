# TRAIN T4: every change against TRAIN T3's kit

Derived 2026-10-12 by a sub-agent from T3's run2 kit (`coord-scratch/tT3/run2`, read only: it landed TRAIN T3 at
`4c9c68a32f`). The derivation is mechanical and re-runnable: `tT4-derive.py` (R0) then `tT4-derive-edits.py` and
`tT4-derive-edits2.py` (every edit anchored and asserted). T3's two run2 patches ride R0 unchanged: the rc regex
`\( (DEADLINE was [^)]*)\)\{0,1\}` in both landing scripts, and `MASTER_MOVED_OK` in `tT4-land-prep.sh` (plus run2's
emitcheck seed of `docs/ValidatedTestPackages.md`).

## 0. Inventory

| File | Origin | T4 ids |
|---|---|---|
| `tT4-assemble.sh` | T3, R0 | T4-2, T4-3, T4-4 |
| `tT4-conflict-map.sh` | T3, R0 | header, the MAP_FROM message |
| `tT4-resolve.py`, `tT4-resolve-controls.sh` | T3, R0 | T4-3 |
| `tT4-helpers.py` | T3, R0 | T4-7 |
| `tT4-fixup.sh` | T3, R0 | T4-5, T4-6 |
| `tT4-battery.sh` | T3, R0 | T4-1, T4-8, T4-9, T4-10 |
| `tT4-controls.sh`, `tT4-selfcheck.sh` | T3, R0 | T4-11 |
| `tT4-i9-te.sh`, `tT4-lane-brief-i9.md`, `tT4-lane-brief-linux.md` | T3, R0 | header / a T4 block that leads; SHAs to placeholders |
| `tT4-emitcheck.sh`, `tT4-i9-shard.sh`, `tT4-linux-legs.sh`, `tT4-modules-legs.sh`, `tT4-reread.sh`, `tT4-land-*.sh`, `tT4-regen-apply.py`, `emitdrift.py`, `tT4-ng-parse.ps1`, `tT4-follow.txt`, `tT4-i7-sweeps.txt`, `tT4-i9-shard.txt` | T3, R0 only | none |
| `tT4-seats-draft.txt`, `tT4-ruled.txt`, `tT4-README.md`, `COORD-LAUNCH-CHECKLIST.md`, `launch-battery.sh` | REWRITTEN | T4-0 |

## 1. R0, the names

CODE lines: `tT3` -> `tT4`, `trainT3` -> `trainT4`, `TRAIN T3` -> `TRAIN T4`, `TT3_` -> `TT4_`, `T3-CONTROLS` /
`T3-SELFCHECK` -> `T4-...`, the local union `train-t3-union` -> `train-t4-union`, the pushed `claude/coord-trainT3-union`
-> `claude/coord-trainT4-union`. COMMENT lines: pointers to T3's records stay; sibling names, scratch paths and quoted
subjects move. T3's own change ids (T3-1 .. T3-9) stay as history in the comments.

## 2. T4's changes (one line each)

- **T4-0** the seat table (11 rows, frozen, every tip read at the remote), the ruled file (row 3, two lines, kind
  `theirs`), the README, the checklist, `launch-battery.sh` (it runs in `run1/go`, the folder the map, the assembly
  and the fixup ran in).
- **T4-1** the previous train's record is T3's run2 (`T3_RUN`; N/O/P/Q/FL_RUN refused): SUMMARY, S and T rewrites;
  fallbacks 871 (CNR, T3's battery) and 1291 (FX); the TE-i9 baseline is T3's i9 patch when on the box.
- **T4-2** row 5 is seated BY SHA: `TIP_MOVED_OK` DEFAULTS to `g-gocmp-perf-floor` alone (origin's tip 701430cf6d is
  one docs commit past a5ef12237c, NOT seated; COORD 2026-10-12). Admitted only as a DESCENDANT, the commits past the
  seat listed; `TIP_MOVED_OK=''` admits nothing. No other row's check is weakened (control TABLE plants row 3 at its
  parent: refused).
- **T4-3** resolver kind `theirs`: the ROW's blob WHOLE (stage 3), the ruled hunk count asserted, the written blob
  asserted equal to stage 3, and the assembler re-reads it from the merge commit. Whole-file, never per hunk: a
  per-hunk pick of EscapeRegistryGenerator.cs keeps master's `Nameable` / `ReferencesDirectly` BESIDE the seat's own
  (two definitions each, measured). Controls C11, C11n, C11k.
- **T4-4** `i9tree` compares TREES (no i9 union ref is pushed for T4): the last seat merge's tree must EQUAL
  `I9_TREE` (10 to 40 hex); DIFFERENT lists `git diff --stat` when the tree is in the store; exit 4 as before.
- **T4-5** fixup PRERES-reg: ONE ruled line in `src/go2cs/go2cs-src.projitems` (retiredHandOwns_test.go, directly after
  publishedCounterReset_test.go, the anchor's indentation and line ending), an explicit logged edit (T3's PRERES only
  reads); controlled by `TestProjitemsRegistersEveryGoSource` by name, FAIL naming the file before, the five
  `TestProjitems*` PASS after. The stdlib-metadata regeneration's own-file assert excludes it; step 7 commits it.
- **T4-6** fixup `GOLDEN_RULED='RawStringLineEndings X509SystemVerify'`: the step-5 prediction by name; an OTHER-class
  golden is admitted under the default class only when the projects CNR moves EQUAL that set (a third project, or a
  ruled one not moving, dies). Narrower than `GOLDEN_CLASS=any`, which stays COORD's.
- **T4-7** helper `H3_TOKENS`: g-golib-creator-o1 edits Goroutine.cs (`s_creatorBySite` x3, `CreatorWalks` x1). Without
  the entry precheck FAILs H3 by name (measured: hard-failures=1).
- **T4-8** LEG TP: `TP_REF=claude/g-trim-probes-more`, `TP_PIN=5f363aeae4`, `TP_WT=/h/go2cs-tmp-coord/tT4-probes`;
  `tpread` wants 8 rows (consumer RUNS-CLEAN; genprobe c32a c32b c32c registry stringer fieldbox GO-EQUAL), each name once.
- **T4-9** LEG GN: `GN_TRIM` = T3's 15 + row 3's 6 (`GN_TRIM_T4`); floor 195 method lines; `GN_RESULTS_MIN=214`.
- **T4-10** PRE-D CNR-FRESH: `CNR_ALIAS_RULED=''` (no T4 row touches the CNR script): any change is a finding again;
  `cnralias` stays inert under its control (CN).
- **T4-11** controls: RT-real and FX9 read T3's run2 and T3's landing route (fixup 38f1ce2510, fixup-2 9adab1a13d, MS11
  merge 4c9c68a32f; BATTERY_RC_OK=6); TABLE, I9, TP, GN rewritten for T4; RES gains C11..; selfcheck S7 counts T3's names.
- **T4-12** (COORD 2026-10-12, after the fixup's step 4 RED) `REGEN_ALLOW_FILE=<file>`: a file of EXACT repo-relative
  `src/core/...` paths, one per line; `tT4-regen-apply.py` takes it as `@<file>` and admits a listed path iff it is in
  the file (no pattern compiled). A file entry no attribution list holds (STALE), a duplicate, or an entry outside
  `src/core/` is refused too: exact in both directions. `REGEN_ALLOW_NOTE` (required with the file) is COORD's reason,
  stamped in PRE and written into the commit message; the file is copied to `tT4-fixup-logs/regen-allow-file.txt`.
  Exclusive with `REGEN_ALLOW`. Why: row 2's ruled footprint is 1,548 paths; an exact ERE is 45 KB, over Windows'
  32,767-character command line, and a directory-grouped pattern was ruled out by COORD. Control arm RGF in
  `tT4-controls.sh` (a.cs alone listed refuses its sibling ab.cs and admits a.cs; both listed admit both; a stale entry
  and a duplicate are refused); `tT4-controls-rg.sh` runs the RG and RGF arms alone. Measured: RG ok, RGF ok, failed=0;
  RGF FAILED against the pre-T4-12 `tT4-regen-apply.py` (the control fires). The file for T4 is
  `tT4-regen-allow-row2.txt` (1,548 paths, the union of the three attr lists, sha256 268702927ca7); proof that it is
  comment/whitespace only plus two named code classes: `tT4-drift-tokens.py`, `tT4-drift-classify.py`,
  `drift-tokens-report.txt`, `drift-classes.txt`.

## 3. What was run (every result as read)

- `bash -n` on all 16 .sh, Python parse of every .py: pass (selfcheck S6).
- `tT4-controls.sh` (OUT=coord-scratch/tT4/controls-1907, TE_CONTROLS=hnd trainP/controls): 27 arms ok,
  `T4-CONTROLS DONE failed=0 not-run=0`, rc 0. `tT4-selfcheck.sh`: `T4-SELFCHECK DONE failed=0`.
- A probe map (run1/probe0, no ruling): row 3 UNRULED on exactly the two ruled files, one hunk each; every other row clean.
- `tT4-conflict-map.sh` from BASE 226afb810a: 11 rows, row 3 RULED (theirs x2), `MAP DONE head=789c1ac576
  rows-sha256=330f1ce10728 ruled-sha256=452cce41f0e6`; its tree 7e1f0969c1 = the i9's.
- `tT4-assemble.sh` (I9_TREE=7e1f0969c1): TABLE OK 11 rows (row 5 acknowledged, its one commit listed); union
  `7dfd1d493c`; MAP TREE EQUAL; I9 TREE EQUAL; SHAPE 11; ORDER; TREE CLEAN; signatures 11 of 11, one key.
- precheck at the union head: `PRECHECK hard-failures=0 notes=0 mode=head`.
- The fixup, the module rehearsal: see COORD-LAUNCH-CHECKLIST.md section 4 and the report.

## 4. Not done, and why

- The battery, the probe worktree, any push: COORD's.
- TP was not run (a publish of eight programs). `tpread` ran on planted logs only.

## 5. RUN2 (2026-10-13): row 2 re-seated, the kit changes over run1's kit

Run1 (`coord-scratch/tT4/run1`, union 7dfd1d493c, fixup 1438ed20ea pushed as claude/coord-trainT4-union, battery stopped
after leg 4) is the reference and is not modified. Run2's kit is `coord-scratch/tT4/run2` (a copy of run1's kit files,
not its logs); the runs happen in `run2/go`. Every code edit is made by `tT4-r2-edits.py` (anchored, asserted).

- **R2 (names).** The union worktree is `/h/go2cs-tmp-coord/tT4r2` on the LOCAL branch `train-t4-union-r2` from BASE
  226afb810a (kept although origin/master moved by docs/workflow merges only: af4d081f3f, 052c1410d9, d332665cd5; the
  assembler runs with `BASE_NOT_TIP_OK=1`, the landing reads them through `MASTER_MOVED_OK`). The map worktree is
  `tT4r2-map`; the map and assembly stamps go to `run2/stamps` (run1's in coord-scratch/tT4 are left); the fixup's emission
  scratch is `tT4emitr2`; W / WT / WB / go2csPath / the LIVE census fragment name tT4r2; the launch-folder guards refuse
  tT4r2 as well as tT4. The lock stays `coord-scratch/tT4/.battery.lock` (run1's battery left it when it was stopped: removed
  02:11 after the process census read no T4 process).
- **Seat table.** Row 2 is `c1-comment-parity-gap-fix|fbf2a5ebc5` (a0e2099e9a + b7b5a758e5 red test, 07cacea829
  writeStatementGap, fbf2a5ebc5 PLAN doc). Every other row, the row-3 ruling, row 5 BY SHA (TIP_MOVED_OK default) as run1.
- **T4-13** the assembler's R1 DELTA arm (`r1delta`, between markers): with no i9 rehearsal, I9_TREE does not apply; the
  tree at the last seat merge must differ from run1's union tree `R1_TREE=7e1f0969c1` by EXACTLY the paths of
  `R1_DELTA=a0e2099e9a..fbf2a5ebc5` (it also reads how many hold the range tip's blob). DIFFERENT, or not comparable when
  either variable is given, is exit 4. Control arm R1D (`tT4-controls-r2.sh`, 7 arms); it FAILS on run1's kit (no arm).
  `tT4-treecheck.sh` (run2): the fixup tree with the regenerated paths put back to the run2 union's blobs must differ from
  the i9's tree 0145a977f0 by exactly the same T4-13 paths (the dry run at the union head reads 10 paths: DIFFERENT, fires).
- **T4-14** battery leg NGF-j0 RETIRED: its target `src/tools/j0-uuid-rehearsal` was removed by master's cleanup
  cfe57cd718 (in the base); run1's battery read rc=1 on its three msbuild reads, and NGb51 / NGb7 errors=1 on the missing
  j0-consume.ps1, which leaves NGF's parse list too. A guard stamps the retirement and raises a finding if the folder is back
  at HEAD. Control arm NGJ0: absent at BASE, present at cfe57cd718^, the extracted guard reads STAMP / FINDING on the two,
  the leg-name sets differ by exactly the three NGF-j0 legs (56 -> 53), run1's copy still holds them (fires).
- `tT4-controls-r2.sh` also carries R2N (the names, static). `tT4-r2-fixdiff.py`: run2 vs run1, by emission and by fixup
  tree, kinds BLANK-DEL / MAP-ONLY / DELTA / OTHER, with 7 plants (`plants` mode).
- Controls ran with `TABLE_BASE=226afb810a BASE_NOT_TIP_OK=1` (the TABLE arm otherwise reads origin/master's moved tip).

### 5.1 What run2 read (2026-10-13)

- Controls: `tT4-controls.sh` 28 arms ok (`T4-CONTROLS DONE failed=0 not-run=0`); `tT4-controls-r2.sh` R1D, NGJ0, R2N ok,
  and FAILED=3 on run1's kit (each fires); selfcheck failed=0.
- Map from BASE: rows-sha256=a2f727a60342, row 3 RULED theirs x2, every other row clean; MAP DONE head=9d962cbb02.
- Assembly: union b258fe81db (tree fb3447e6df); MAP TREE EQUAL; R1 DELTA EXACT (3 paths, 3 of 3 at fbf2a5ebc5's blob);
  SHAPE 11, ORDER, TREE CLEAN; signatures 11 U 941694536F21BAFF. Precheck at head: hard-failures=0 notes=0.
- Fixup launch A (measurement): fresh emission 1241 / 1271 / 1260, stopped at the regen check as predicted. Proof: plants
  OK 6/6; DRIFT-TOKENS 3,772 pairs / 223 code-class (the same FAIL set as run1); DRIFT-CLASSES DEREF-ALIAS-DROPPED=410
  LAMBDA-EXPR-BODY=6 OTHER=0. Path file: 1,548 paths, sha256 268702927ca7, byte-identical to run1's.
- Fixup launch B: 94ca118a20 (tree 3ab3420c12), 1555 files, signed U 941694536F21BAFF; CNR moved exactly the two ruled
  projects. vs run1's fixup tree: 133 paths = 3 DELTA + 77 BLANK-DEL (98 blank lines) + 53 MAP-ONLY, OTHER 0.
  treecheck: the put-back tree differs from the i9's 0145a977f0 by exactly the 3 T4-13 paths.
- Module rehearsal at 94ca118a20: rc 0, failures 0, owed 0.
- A READING for COORD: TestCommentParityCensus still FAILS at the run2 fixup head (body:extra:unexplained 866 -> 2278,
  run1's battery read 2353; eol / misplaced rises unchanged): run2's battery legs C and CB will read it as run1's did.

### 5.2 The lane readings for run2 (COORD 2026-10-13; the handover copy only, not the running kit)

- `tT4-lane-brief-i9.md`, `tT4-lane-brief-linux.md`: a T4 RUN2 block leads (ref `claude/coord-trainT4-union-r2`,
  58f17868fe; row 2's footprint corrected: the fixup regenerated 1,548 src/core paths and no `_test.cs`; the TE
  prediction is the comment-parity classes, in bulk); the T4 block's in-seat claim is marked SUPERSEDED in place.
- `tT4-linux-legs.sh`: fetches `UNION_REF` (default `claude/coord-trainT4-union-r2`); run1's copy hard-coded
  `claude/coord-trainT4-union` and would abort on the SHA check at run2. Nothing else changed.
- `tT4-i9-drift.sh` (new): `tT4-drift-tokens.py` and `tT4-drift-classify.py` pointed at a worktree's tracked rewrites
  against HEAD: LINE-ENDING, COMMENT-WS, DEREF-ALIAS-DROPPED, LAMBDA-EXPR-BODY, DEREF-ALIAS-ADDED, OTHER, NOT-CLASSED
  (OTHER and NOT-CLASSED printed by name). Control: a synthetic repo with one file per class read each class once.
- `launch-fixup.sh`, `launch-fixup-measure.sh`: GOROOT reads the profile from `USERPROFILE` (the identifier census
  refused the as-run copies' literal profile path); the spelled value is the same.

### 5.3 T4-19: a plant is subtracted by its LINE (the i9, 2026-10-14; the Go 1.26 hop's H0, F-H0-2, ruled by COORD)

- **T4-19** `tT4-emitcheck.sh` dropped PLANT_CS (`unicode/utf8/utf8.cs`) and PLANT_SIB (`sync/pool.cs.auto`) from the
  drift and sibling lists BY PATH (`grep -vxF`), so any REAL drift in those two files was subtracted with the plant.
  H0's seeded three-target regen found `utf8.cs` carrying T4's comment parity since TRAIN FL, unseen by every check.
  Now, after each conversion, `tT4-plantreal.py` (new) compares each FIRED plant file's emission with its seed minus
  the appended plant block, CR stripped on both sides: PURE is subtracted as before; REAL stays in the lists (listed
  in `plantreal-<arm>-<os>.txt`, archived as evidence, and named on the EMIT line as REAL-DRIFT-IN-PLANT-FILE) and is
  attributed like any other file. The filter is `unplant`, used at all four former `grep -vxF "$PLANT_..."` sites.
  The plant LINE reaches the helper by FILE (`$T/plantline.txt`): MSYS rewrites an argv beginning `//` into `/` when
  it starts a native program, measured by the test below on its first run.
- RED-FIRST: `tT4-plantreal-test.sh` (new) runs the old filter and the new one over a fixture in the check's own
  shapes. The case `real` (a comment line added to the emission) is kept by the OLD filter 0 times of 1 (the defect)
  and by the NEW 1 of 1; `pure` is subtracted by both; `real-crlf` reads REAL; a seed with no plant block is refused
  (rc 2). Output: `PLANTREAL-TEST OK`.
- `tT4-selfcheck.sh` on this folder: S1-S4, S6 ok. S5 reads CR bytes in files with no eol attribute; that is the
  i9's autocrlf checkout, not the blobs (the committed blobs hold 0 CR; the three files above hold 0 CR on disk).
