# TRAIN FL: every change against TRAIN Q's script set

Derived 2026-10-08 by a sub-agent from `trainQ/` as committed on `claude/coord-handover` (never edited) and from Q's
run folder `coord-scratch/tQ/run1` (read only: its scripts are byte-equal to the hnd set; its logs show the battery's
EXIT 6 on E, MOD and S:testing). Nothing was built, converted or tested; no script ran against `/h/go2cs-tmp-coord/tFL`
(it was READ with git: log, show, diff, merge-tree, and through a no-checkout probe worktree at 14c543bb06 made from
the shared repository, removed after). Section 3 lists what WAS run.

## 0. Inventory

| File | Origin | FL ids |
|---|---|---|
| `tFL-assemble.sh`, `tFL-conflict-map.sh` | Q, R0 | FL8 (map), FL9 |
| `tFL-resolve.py`, `tFL-resolve-controls.sh` | Q, R0 | FL7 |
| `tFL-ruled.txt`, `tFL-ruled-both.txt`, `tFL-seats-draft.txt` | REWRITTEN for FL | FL13 |
| `tFL-fixup.sh` | Q, R0 | FL2, FL4, FL5, FL1 (golden stop) |
| `tFL-emitcheck.sh` | Q, R0 | FL5 |
| `tFL-battery.sh` | Q, R0 | FL1 (through the helper), FL2, FL3, FL6, FL10 |
| `tFL-helpers.py` | Q, R0 | FL1, FL5, FL12 (flcontrol) |
| `tFL-land-prep.sh`, `tFL-land-final-reads.sh` | Q, R0 | FL1 (4e census), FL2 (headers) |
| `tFL-land-rereads.sh` | NEW (Q's two hand fragments) | FL11 |
| `tFL-i9-shard.sh`, `tFL-linux-legs.sh` | Q, R0 | FL2 (refresh admitted) |
| `tFL-i9-te.sh` | Q, R0 | FL1, FL3 (header, baseline) |
| `tFL-controls.sh`, `tFL-selfcheck.sh` | Q, R0 | FL12 |
| `tFL-modules-legs.sh`, `tFL-reread.sh`, `tFL-regen-apply.py`, `emitdrift.py`, `tFL-ng-parse.ps1`, `tFL-follow.txt`, `tFL-i7-sweeps.txt`, `tFL-i9-shard.txt` | Q, R0 only | none |
| `tFL-lane-brief-i9.md`, `tFL-lane-brief-linux.md`, `tFL-README.md`, `COORD-LAUNCH-CHECKLIST.md` | REWRITTEN for FL | FL13 |
| `facelift-tests-hunk-classes.md` | COORD's (16fa5b48e4), kept byte-equal | the input of FL1 |

Not carried: `tQ-CHANGES.md`, `tQ-DERIVE-REPORT.md`, `tQ-map.md`, `tQ-seats-gen.sh`, `tQ-seats-v0-coord.txt` (Q's records)
and the premap set (`tQ-premap*.sh`, `tQ-regcheck.py`, `tQ-unioncheck.py`, `tQ-gofmt-parse.sh`, `tQ-footprints.sh`,
`tQ-hunks.sh`: not part of the order of work at Q either). Pointers to them read `trainQ/tQ-<name>`.

## 1. R0, the names

CODE lines: `tQ-` -> `tFL-`, `trainQ` -> `trainFL`, `TRAIN Q` -> `TRAIN FL`, `/tQ` -> `/tFL`, `tQemit` -> `tFLemit`,
`TQ_` -> `TFL_`, `Q-CONTROLS`/`Q-SELFCHECK` -> `FL-...` (so W=/h/go2cs-tmp-coord/tFL, BRANCH=claude/coord-trainFL-union,
coord-scratch/tFL, refs/coord/tFL-map, the subjects 'fixup: TRAIN FL', 'fixup-N: TRAIN FL', 'refresh: TRAIN FL').
COMMENT lines: script names renamed; 'TRAIN Q' renamed ONLY inside a quoted subject ('fixup: ', 'refresh: ', 'into ');
every other 'TRAIN Q' in a comment is Q's history and stays. Pointers to Q's records stay `trainQ/tQ-*`. Base wording
in messages: 'TRAIN Q's landed master line (and the docs commits COORD landed after it)'. Five hand fixes after the
rename (the two `WB='H:\...\tQ'` literals, the orphan census's `'tQ'` tokens, two `:tQ` case arms, the selfcheck's
EXEMPT list).

## 2. FL's changes (one line each)

- **FL1** `tFL-helpers.py`: the face lift's -tests classes, one per step (FL-A, FL-S, FL-F, FL-E, FL-D, FL-B, FL-C, FL-C6, FL-R3, FL-R5, FL-R1, FL-BR, FL-R4, FL-R6) from `facelift-tests-hunk-classes.md` and C2's posts 62566dca42 / 02ade5ea32 / 21e7b8984a; `fl_normalize` takes them out first, then Q's classes read the rest; new class FACELIFT counted in teattr, hunkclass, testsrc-refresh (per step); N-PARTIAL and G-FRAME stay; anything else stays OTHER.
- **FL1b** `fl_prime`: a descriptor carrier's blank line that git places in a neighbour hunk is credited once per carrier, per file.
- **FL1c** combined attribute lines (`[GoArrayDims(2), GoMapKeyDims(2)]`) are read item by item (FieldDimsCargo's golden).
- **FL1d** `tFL-fixup.sh` step 5: a golden that moves by a face-lift class STOPS (0 predicted: row 10 re-baselined 809 projects); `GOLDEN_CLASS=facelift` admits it by COORD's call.
- **FL1e** `tFL-land-prep.sh` 4e: the face lift's census of the refresh (leaving-attribute lines at HEAD and in the refreshed worktree) and the TE steps line.
- **FL2** `fixchain` (fixup and battery, one predicate between markers): 'fixup', then 'fixup-N' in order, and at most ONE 'refresh: TRAIN FL' after the fixup (Q's route: fixup, refresh, fixup-2); FIXUP_N counts fixups only; the i9 and linux drivers admit the refresh subject.
- **FL3** the previous train's record is Q's run1 (`Q_RUN`, `Q_SUMS`, `Q_REWRITES*`; P_RUN refused like N_RUN and O_RUN); fallbacks 863 / 1291; the TE-i9 baseline is Q's i9 patch.
- **FL4** step 4t: rows `encoding/json encoding/binary net/rpc`; per row the leaving-attribute lines at HEAD (414 / 21 / 41 at 14c543bb06), the FACELIFT hunks and steps, the lines left, OTHER by name; flagged when a row with leaving lines reads 0 FACELIFT.
- **FL5** (Q lesson b) csprojdrift ADMITS a seat-added hand-written csproj with no `latest` token, no template condition and no converter signature (Q's PackageSymbols.csproj); the emission check's word is `csprojword`: OK only on rc 0, UNEXPLAINED-UNGATED with the gate off (never OK); the fixup's step 4 STOPS on any UNEXPLAINED word (`CSPROJ_UNEXPLAINED_OK=1` by COORD's call).
- **FL6** (Q lesson c) `testing` leaves X_ROWS (the i9 shard reads it); a PRE-3 guard takes it off the i7 run list as a finding if anything puts it back; the i9 brief says it is the i9's alone.
- **FL7** `tFL-resolve.py` kind `append-both-sorted` (both sides of a hunk in sorted order): the hand resolution of row 8's projitems, reproduced byte for byte (resolve-controls C10; append-both gives another blob, C10n). Rows 3 and 4's hand BOARD merges dropped ONE separator blank line (26514 lines against 26515 by rule): no kind reproduces them.
- **FL8** `tFL-conflict-map.sh MAP_FROM=<union head>`: the map starts AT the hand union, whose first-parent merges must be the list's first k rows in order; those rows are not merged again, the rest merge (and resolve by rule) on top; the assembler resumes on the union branch and compares trees as before.
- **FL9** (row 10 cut on the union) the assembler's and the map's 'another train's assembly commit' check admits THIS train's seat merges in a row, when each second parent is an EARLIER row (TABLE_ONLY: 11 rows OK; a planted list with row 10 first reads 9 refusals by name).
- **FL10** (Q lesson d) MOD runs right after CB (Q read its module refusal 6.5 h in); Q's restore before MOD stays as PRE-PB; the checklist runs the module legs standalone at the fixup head before the battery, and G's module re-reads are dispatched at the fixup head.
- **FL11** (Q lesson e) `tFL-land-rereads.sh`: MOD, the final reads, T2, PUB and the C# consumer runner at the head that lands; pwsh STARTS with DOTNET_ROOT unset and the login PATH, .NET 10 first INSIDE the session, the runner with `-WorkRoot` (TRAIN P's consumer-land.sh form); exit = the first red step.
- **FL12** controls: new arms FL-plant (32 plants), FL-real, FL-gold, FL-Q, CSP, CSP-real, EUG, FX9, TX; RT-real reads Q's run1 and Q's landing route; resolve-controls C9, C10, C10n; selfcheck S7 counts Q's names, EXEMPT empty.
- **FL13** the seat table (12 rows, row 12 a SLOT), the ruled files (no active line: rows 1-10 are hand merges, row 11 merges clean, row 12 clean at 6ae0918484), the briefs, the README, the checklist.
- **Q lesson a** (fixup-N after a pushed fixup, refresh between): FL2, FL11, checklist 6.

## 3. What was run (every result as read)

- `bash -n` on all 16 .sh: pass. Python parse of the 4 .py: pass. `tFL-selfcheck.sh`: failed=0 (its S2 caught one
  apostrophe this derive put in a `${MASTER:?...}` message, which `bash -n` passed: fixed).
- `tFL-controls.sh` (OUT=coord-scratch/tFL/derive-ctl2): 25 arms ok, `FL-CONTROLS DONE failed=0 not-run=0`, rc 0. Its
  first run failed FL-plant on the ARM's own count (the verdict line counted as a file): fixed, re-run green.
- `tFL-resolve-controls.sh`: 11 arms ok.
- FL1 on real objects: production 541766413e..14c543bb06 = 10070 hunks, 9786 FACELIFT, 253 MAP, 31 OTHER, OTHER only in
  six hand-owned files; goldens = 5332 hunks, 5263 FACELIFT, 48 MAP, 21 OTHER (three new projects, the four lists, one
  carrier hunk git aligned across two hunks); Q's own S rewrites: facelift 0, n-partial 375.
- csprojdrift on Q's leg E pair: TEMPLATE DRIFT ONLY (Q's reader: UNEXPLAINED); on 541766413e..14c543bb06: new-ok 3,
  seat-edit 1 (GenTests.csproj), rc 0.
- precheck (head mode) at 14c543bb06 through a no-checkout probe worktree, rows 1-10: every COUNT / REG / BOTH arm ok;
  one FAIL, 'optin.py check rc=2', because the probe had no files (not a finding).
- `tFL-assemble.sh TABLE_ONLY=1` (a run copy; it fetched origin): the 12-row table reads ONE problem, row 12's SLOT;
  rows 1-11 read TABLE OK (rows-sha256 d7c2ec79517e).
- cnrexpect at 14c543bb06: N = 866 (enumerated 873, 7 linux skips; added CallerLineCallSite, CallerLineMultiLine,
  GenericFuncInstantiationArg). merge-tree: row 11 cfe3d6bae7 and row 12 at 6ae0918484 merge clean on 14c543bb06.

## 4. Not done, and why

- No reading of the union's own battery-class run in progress (tFL is FROZEN). Every EXPECT that depends on rows 11
  and 12 (registration counts, CB names, CNR N if R adds a project) is derived at run time, as at Q.
- MAP_FROM (FL8) has no planted control: it is inline in the map script and its first real run is the reading.
- G's four third-party modules (pflag, cobra, logrus, testify) are NOT battery legs: their versions and EXPECTs are
  G's; the checklist dispatches them at the fixup head (FL10) with Q's stamped readings as the prediction.

## 5. Amendment 2026-10-08: FL-E's trailing-comment form and the diff-alignment realignment

- tFL-helpers.py fl_normalize: FL-E admits a field line with a TRAILING `// comment` (the tag comment lands before the
  `//`, the blanks ahead of it re-aligned; code and comment text byte-identical). The pending attributes apply E LAST.
- tFL-helpers.py te_class2 / _fl_realign: a hunk read OTHER is re-diffed (git `--patience`, `-U0`) on its OWN region text
  (te_hunks now keeps context + changes as .pre/.post) and is FACELIFT only when every sub-hunk is explained and a
  face-lift step fired (counted as `realigned`). LIMITATION: a -U0 hunk has no context, so a misalignment ACROSS two
  -U0 hunks stays OTHER (i9 FL shard U0: 7 hunks of encoding/xml marshal_test.cs, 2 of unique/handle_test.cs; the
  full-context patch of the same run reads them FACELIFT).
- Read: 4t encoding/json OTHER 7 -> 0 (FACELIFT 266 -> 273); i9 FL shard full context OTHER 44 -> 36, -U0 114 -> 101;
  every remaining OTHER file is TRAIN Q's standing set; TRAIN Q's i9 patches unchanged (0 hunks moved class).
- tFL-controls.sh: 11 new plants under FL-plant (43), arms FL-E (by name) and FL-E-real (the kept patches).
