# TRAIN T3: every change against TRAIN FL's kit

Derived 2026-10-10 by a sub-agent from FL's run2 kit (`coord-scratch/tFL/run2`, read only; the hnd copy
`trainFL/` at 48fdd24b65 differs from run2 only by FL-E's face-lift classifier amendment and the seat table, both moot
here). The derivation is mechanical and re-runnable: `tT3-derive.py` (R0) then `tT3-derive-edits.py`,
`tT3-derive-edits2.py`, `tT3-derive-edits3.py` (every edit anchored and asserted).

## 0. Inventory

| File | Origin | T3 ids |
|---|---|---|
| `tT3-assemble.sh` | FL, R0 | T3-2, T3-3 |
| `tT3-conflict-map.sh` | FL, R0 | T3-1 (MAP_FROM dropped) |
| `tT3-helpers.py` | FL, R0 | T3-4 |
| `tT3-fixup.sh` | FL, R0 | T3-5 |
| `tT3-battery.sh` | FL, R0 | T3-4 (through the helper), T3-6, T3-7a, T3-7b, T3-9 |
| `tT3-land-prep.sh` | FL, R0 | T3-4 (FL1's refresh census dropped) |
| `tT3-controls.sh`, `tT3-selfcheck.sh` | FL, R0 | T3-8 |
| `tT3-i9-te.sh` | FL, R0 | header only (T3-4, T3-6) |
| `tT3-lane-brief-i9.md`, `tT3-lane-brief-linux.md` | FL, R0 | a T3 block on top that leads; the pushed union ref |
| `tT3-emitcheck.sh`, `tT3-i9-shard.sh`, `tT3-linux-legs.sh`, `tT3-modules-legs.sh`, `tT3-reread.sh`, `tT3-land-final-reads.sh`, `tT3-land-rereads.sh`, `tT3-resolve.py`, `tT3-resolve-controls.sh`, `tT3-regen-apply.py`, `emitdrift.py`, `tT3-ng-parse.ps1`, `tT3-follow.txt`, `tT3-i7-sweeps.txt`, `tT3-i9-shard.txt` | FL, R0 only | none |
| `tT3-seats-draft.txt`, `tT3-ruled.txt`, `tT3-README.md`, `COORD-LAUNCH-CHECKLIST.md`, `launch-battery.sh` | REWRITTEN | T3-1 |

**Dropped (FL-only):** `facelift-tests-hunk-classes.md` (FL1's input); FL's MAP_FROM hand-union resume (FL8: the map
refuses MAP_FROM now); the controls FL-plant, FL-real, FL-gold, FL-Q; the fixup's FL4 leaving-attribute census in step
4t; land-prep's FL1 face-lift census of the refresh; `tFL-ruled-both.txt` (FL's record). **Disabled, kept inert:** the
face-lift classifier (`fl_normalize`, its plants, `flcontrol`) behind `FL_CLASSES = False`; the FACELIFT key stays in
every count line (always 0) so the readers' formats hold; the fixup's `GOLDEN_CLASS=facelift` value is accepted and can
never fire.

## 1. R0, the names

CODE lines: `tFL` -> `tT3` (paths, script names, logs, locks, `refs/coord/tT3-map`, the `.tT3-*-scratch` markers),
`trainFL` -> `trainT3`, `TRAIN FL` -> `TRAIN T3` (the subjects 'Merge ... into TRAIN T3', 'fixup: TRAIN T3',
'fixup-N: TRAIN T3', 'refresh: TRAIN T3'), `TFL_` -> `TT3_`, `FL-CONTROLS`/`FL-SELFCHECK` -> `T3-...`. The union branch
`claude/coord-trainFL-union` -> the LOCAL `train-t3-union` (assembler, fixup) and the PUSHED `claude/coord-trainT3-union`
(the linux driver's fetch; the briefs). COMMENT lines: pointers to FL's records (`trainFL/tFL-*`, `coord-scratch/tFL/run2`)
and FL's union name stay as history; sibling script names, scratch paths and quoted subjects move.

## 2. T3's changes (one line each)

- **T3-1** the seat table (24 rows, frozen, every tip read at the remote), the ruled file (no active line), the README,
  the checklist, `launch-battery.sh`; the map starts at BASE (MAP_FROM refused by name).
- **T3-2** assembler notes tokens: `contains <sha>` (table: the row carries it; rows 15, 19, 20) and `in-head <sha>`
  (table: an EARLIER row carries it; merge: HEAD holds it; row 20 with 07411b26f7).
- **T3-3** assembler i9 tree arm `i9tree` (between markers): the last seat merge's tree must EQUAL the tree of
  `claude/i9-union-t3-ref` (read from the assembler's own ls-remote); `I9_TREE=<posted>` pins what the ref must hold;
  DIFFERENT lists `git diff --stat` and exits 4; not comparable is a NOTE, a STOP when I9_TREE is set.
- **T3-4** the face lift's -tests classes are off (`FL_CLASSES = False`); TE / TE-T / TE-i9 / hunkclass / refresh read
  Q's classes; a face-lift-shaped hunk is OTHER, read by name.
- **T3-5** fixup step 4t keeps Q's -tests arm on `encoding/json encoding/binary net/rpc` (PREDICTED 0 moved hunks) and
  drops FL4's leaving-attribute census; the header states the T3 predictions.
- **T3-6** the previous train's record is FL's run2 (`FL_RUN`; N/O/P/Q_RUN refused): SUMMARY, S and T rewrites, the T
  legs' result files; fallbacks 866 (CNR) and 1291 (FX); the TE-i9 baseline is FL's i9 patch.
- **T3-7a** LEG TP: `src/tests/TrimProbes/trimprobes.sh <union src, absolute> default` from a detached worktree of
  `claude/g-trim-probes` at `TP_PIN=04239ec85e` (COORD 2026-10-10: G's CRLF fix, compare strips CR on both sides; the
  CR-strip workaround this derive first wrote is REMOVED), capped 90 m; `tpread` (between markers) gates 5 rows,
  consumer RUNS-CLEAN and four GO-EQUAL; a worktree off the pin, or origin moved off it, is a finding.
- **T3-7b** LEG GN: a TRX, `GN_TRIM` (15 Class.Method names the trim rows add) read Passed by name and asserted declared
  at HEAD; the floor is the union's 189 method lines (FL's literal 68 was stale) and Total >= 208 (`GN_RESULTS_MIN`).
- **T3-8** controls: new arms FL-OFF, TABLE, I9, CN, TP, GN; RT-real and FX9 read FL's run2 and FL's landing route
  (fixup 9b7dfdb2ec, refresh 0574b8336c, fixup-2 56f0f1f254); selfcheck S7 counts FL's names.
- **T3-9** battery PRE-D CNR-FRESH: row 5 changes `check-no-regression.ps1` (two names into the documented alias-drift
  set, COORD rulings 2026-10-10). `cnralias` (between markers) admits EXACTLY that: no removed line, every added line a
  comment or one quoted name, the names = `CNR_ALIAS_RULED`, each inside `$documentedAliasDriftPackages` at HEAD.
  Unchanged, FL's arm would have raised a PRE-D finding (EXIT 6) on a ruled seat.

## 3. What was run (every result as read)

- `bash -n` on all 16 .sh, Python parse of the 7 .py: pass. `tT3-selfcheck.sh`: `T3-SELFCHECK DONE failed=0`.
- `tT3-controls.sh` (OUT=coord-scratch/tT3/controls-1): every arm ok except TE-M NOT RUN (no TE_CONTROLS). Re-run with
  `TE_CONTROLS=<hnd trainP/controls>` (controls-2, after the assembly): 27 arms ok, `T3-CONTROLS DONE failed=0
  not-run=0`, rc 0 (about 8 minutes).
- `tT3-conflict-map.sh` from BASE 4e6322d770: 24 of 24 clean, 0 ruled, `MAP DONE head=5caf722fc6 rows-sha256=4dbb292d6358`;
  its tree 466166dd21 = the i9 union's.
- `tT3-assemble.sh` for real (I9_TREE=466166dd21): TABLE OK 24 rows (NOTE: row 15 has 8 best common ancestors, the
  expected MULTI-BASE of a seat stack); 24 signed merges on the LOCAL branch `train-t3-union` in /h/go2cs-tmp-coord/tT3;
  `ASSEMBLED head=498de280bf6ac4f6a5670cd681fcfeb4a1ba5427`; MAP TREE EQUAL; **I9 TREE EQUAL (tree 466166dd21 = the i9
  union 71e5f69dda's)**; ANCESTRY; SHAPE 24 = 24; ORDER; RULED-FILES (slnx 967, lists 824/824/824/795); TREE CLEAN.
  Signatures: 24 of 24 first-parent merges signed by one key. 58 non-merge commits, 58 unique patch-ids (the i9's 58).
  NOT pushed.
- `tT3-helpers.py precheck ... head` at 498de280bf: `PRECHECK hard-failures=0 notes=0`, 94 ok lines (COUNT x6, REG x7,
  BOTH on every shared path).
- `cnrexpect` at 498de280bf: N=871 (enumerated 878, 7 skips; added 5, removed 0); at the base 866.

## 4. Not done, and why

- The fixup, the module rehearsal and the battery: COORD's (the brief forbids them here). EXPECT_HEAD is owed.
- TP was not run (it publishes five programs: a build). Its reader `tpread` ran on planted logs only.
- No TE baseline was re-read: FL's run2 patches are on this box and are the baselines by default.
- The briefs' FL prose below the T3 block is FL's (names renamed); the T3 block leads.
