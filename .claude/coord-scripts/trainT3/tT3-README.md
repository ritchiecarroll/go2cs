# TRAIN T3: the apparatus, restated for T3 (a SHORT README)

**DRAFT 2026-10-10.** T3 is NOT a corpus re-conversion: it changes the converter, the generator, golib and scripts, and
ONE seat (row 16) regenerates the tracked go.* package READMEs. This file keeps every anchor the scripts cite (MS1 to
MS27, 4b, the sections); where FL's procedure text did not change it points to `trainFL/tFL-README.md` (and through it to
`trainQ/tQ-README.md`). Where this file and `COORD-LAUNCH-CHECKLIST.md` differ, the checklist leads. Changes:
`tT3-CHANGES.md`. The seat table: `tT3-seats-draft.txt` (24 rows, FROZEN at the derive).

## 1. The order of work

| Step | What | Script |
|---|---|---|
| 0 | BASE from the remote (4e6322d770 at the derive); fresh shell; the table frozen (every tip read at the remote) | checklist 0 |
| 1 | Commit the kit on the handover branch as `trainT3/` (COORD: announce, push) | checklist 1 |
| 2 | Controls: `tT3-controls.sh` (27 arms), `tT3-selfcheck.sh`, the table check | checklist 2 |
| 3 | The map FROM BASE (FL8's MAP_FROM is gone), then the assembler on the LOCAL branch `train-t3-union`, with the i9 tree arm (`I9_TREE=466166dd21`). DONE BY THE DERIVE: union `498de280bf`, MAP TREE EQUAL, I9 TREE EQUAL | `tT3-conflict-map.sh`, `tT3-assemble.sh` |
| 4 | The fixup (FIXUP_N=1): PRERES, the emission check x3, the -tests arm (4t), the goldens, ONE signed commit; the push as `claude/coord-trainT3-union` (COORD's name; the local branch is `train-t3-union`); the module rehearsal at the fixup head BEFORE the battery (FL10) | `tT3-fixup.sh`, `tT3-modules-legs.sh` |
| 5 | The probe worktree at `04239ec85e`, then the battery (MOD early, testing off the i7, TP and GN's trim guards new) | `tT3-battery.sh`, `launch-battery.sh` |
| 6 | The lanes' readings at the tip COORD names: the i9 shard, P1 / P2 linux, C1's hosted gate | the two briefs |
| 7 | The landing: prep, the refresh (predicted EMPTY or near it), a fixup-N if one is owed (FL2), the re-reads at the head that lands, announce then push | `tT3-land-prep.sh`, `tT3-land-rereads.sh` |

## 2. What the fixup is predicted to carry

Nothing. Row by row (read from each row's diff at the derive): the only rows that touch non-test converter source are
1 (linknameOperations.go: one darwin var-alias row; the seat CARRIES its own darwin emission, os/darwin/executable_darwin.cs
and runtime/darwin/os_darwin.cs), 8 / 15 (positionMapOperations.go: emission moves only for sources UNDER THE MODULE
CACHE, never GOROOT's; testConversion.go adds a host env var), 10-13 / 15 (nugetgo: -recurse=nuget lock, feed, flags; the
input digest is written only for -recurse non-stdlib projects), 16 (readme.go, readmeValidationBadge.go, projectFileWriter.go:
README text; the seat regenerated 343 tracked READMEs). Gen and golib rows (18-23) change generated code at compile time,
not converter output. So step 4 reads **union-attributable 0 on windows, linux and darwin**, `regen applied 0`, HANDOWN 0;
step 4t **0 moved hunks** on its three rows; step 5 **0 goldens moved**. A README the fixup's -stdlib reconversion
rewrites (row 16's emitter at the union) is the one class worth reading by name: predicted 0 (row 16 regenerated them
with its own emitter and no later row touches readme.go).

## 4. Manual steps (FL's; only what differs at T3 is written)

- **MS1. The seat list.** 24 rows, no SLOT, no CAND; FROZEN. Row 15 carries pack-exclude (cut 8) INSIDE it; rows 19 and 20
  both carry 64804ed9b5; row 20 merges only with 07411b26f7 (row 19's tip) in HEAD (T3-2 tokens). LITS: FL's list.
- **MS2. H4.** Rows 1, 2 and 17 add behavioral projects WITH their csproj (OsExecutablePath, OsGetpagesize,
  ReexecArgv0Token, Phase5Breakpoint, Phase5CoverageAPIs): no Go-only directory.
- **MS4. REGEN_ALLOW** composes to nothing.
- **MS8, MS14. Registration** (precheck at 498de280bf, 0 hard failures): go2cs.slnx 967 projects (+5), the four lists
  824 / 824 / 824 / 795 `[TestMethod]` = Check, projitems +9 lines.
- **MS9.** The controls are `tT3-controls.sh` (27 arms with TE-M: TE_CONTROLS=<hnd trainP/controls>) and
  `tT3-resolve-controls.sh` (11 arms).
- **MS10. CNR's script changed** (row 5): two names in the documented alias-drift set, admitted EXACTLY by the battery's
  CNR-FRESH arm (T3-9); anything else in the script is a finding (floor 10).
- **MS11. At landing, when master moved:** as at FL.
- **MS13. THE REFRESH** of committed -tests sources: predicted EMPTY or the standing OTHER set (no T3 row changes -tests
  emission; the face-lift classes are off, so a face-lift-shaped hunk is OTHER and read by name).
- **MS15. PUB.** `PUB_MIN` 6.
- **MS23. THE HOSTED GATE** (C1): release-smoke and aot-smoke; whether a release follows T3 is COORD's call at landing.
  Rows 1, 2 and 17 are darwin / runtime rows: the darwin behavioral FULL is the reading of OsExecutablePath on darwin.
- **MS26, MS27.** As at FL.

## 5. Legs

FL's legs, MOD early (FL10), testing off the i7 (FL6), plus **TP** (G's trim probes, default publish, pinned at
`04239ec85e`) and **GN by name** (G's 15 trim guards). Predictions derived from the objects at 498de280bf:

| Leg | Prediction |
|---|---|
| CNR (4) | N = **871** (878 enumerated, 7 linux skips; master 866 + OsExecutablePath, OsGetpagesize, Phase5Breakpoint, Phase5CoverageAPIs, ReexecArgv0Token), byte-identical (the i9 read 871 / 878 at the same tree) |
| CB | **24** `+func Test` added under src/go2cs + the 6 base guards = 30 names |
| GN | Total **208** (floor 189 method lines; the i9 read 208 / 208); the 15 GN_TRIM names Passed |
| TP | consumer RUNS-CLEAN; genprobe, c32a, c32b, c32c GO-EQUAL (5 of 5) |
| GT | the trim guard classes (TrimStage3a/3c1/3c2a/3c2b, GoZeroConstructionGuardTests) derived by PRE-D, at their floors |
| E | union-attributable 0 x3, csproj OK |
| G1/G2 | the roster guard at 2110 checks under both PowerShells (the i9's reading) |
| MOD | as FL; row 8 changes the position-map identity of MODULE-CACHE sources, which is exactly what the -recurse module rows convert: read the module rehearsal at the fixup head first |
| TE / TE-T / TE-i9 | ~0 content rewrites (Q's classes; FL's run2 patches as baselines) |

## 6. Deliberately NOT in the battery

As at FL, plus: the trim probes' F and P arms (Native AOT; P takes hours), a darwin run.

## 8. What was checked, and what was not

`tT3-CHANGES.md` section 3. In one line: the assembler and the map RAN for real (the union is local, unpushed); every
reader T3 adds or changes ran on planted input or git objects; nothing was built, converted or tested.
