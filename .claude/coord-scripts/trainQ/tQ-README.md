# TRAIN Q: the apparatus, restated for Q (a SHORT README)

**DRAFT 2026-10-06, UNCOMMITTED.** Q is an ORDINARY train on today's rendering. Whether a release follows is decided
at its landing, so no script assumes one; C1's hosted release gate still reads the union. The train after Q (the face
lift) re-converts the corpus, so Q must land a clean, fully gated tree.

This file is NOT `trainP/tP-README.md` renamed. P's README is a 618-line manual of P's facts; carried under Q's name
it would state them as Q's. This one keeps every anchor the scripts cite (the steps, MS1 to MS27, 4b, sections 5 to
8), says what holds AT Q, and points to `trainP/tP-README.md` where the procedure text did not change. Where this
file and `COORD-LAUNCH-CHECKLIST.md` differ, the checklist leads. Changes: `tQ-CHANGES.md`. What was measured:
`tQ-DERIVE-REPORT.md`. The seat table and the rehearsal map: `tQ-seats-draft.txt`, `tQ-map.md`.

## 1. The order of work

| Step | What | Script |
|---|---|---|
| 0 | Read BASE from the remote (master's tip; it moves when COORD lands a master commit: twice on 10-06); fresh shell; rule the table's open questions (c1-aot-smoke's re-cut, the windows Native AOT question, the slots, the two acceptance words) and FREEZE the table | checklist 0 |
| 1 | Commit the drafts on the handover branch | checklist 1 |
| 2 | Controls: `tQ-controls.sh`, `tQ-selfcheck.sh`, the table check's planted lists | checklist 2 |
| 3 | The map: every row merged in order in a scratch worktree, ruled conflicts resolved, anything else recorded | `tQ-conflict-map.sh` |
| 4 | The assembly: signed merges, the same resolver, the tree equal to the map's | `tQ-assemble.sh` |
| 5 | The fixup: PRERES, the emission check, the -tests arm, the goldens, ONE signed commit | `tQ-fixup.sh` |
| 6 | The union's push (a NEW ref: push, read back, announce), the GOs, the battery | `tQ-battery.sh` |
| 7 | The lanes' readings (C1 hosted, P1 / P2 linux, the i9 shard) at the tip COORD names | the two briefs |
| 8 | The landing: prep, the signed refresh, the final reads AT THAT HEAD, the consumer runner, announce then push; the question for the owner | `tQ-land-prep.sh`, `tQ-land-final-reads.sh`, `tQ-reread.sh` |

## 2. What the fixup is predicted to carry

Nothing by name. Both footprint rows commit their corpus in-seat (the carrier's 74 generated files, g-cs8500-managed-view's
`runtime/iface.cs`: 75 `CORPUS FOOTPRINT` tokens), so step 4 EXPECTS 0 union-attributable paths on three targets.
The one golden the rehearsal read owed is a ROW (c2-noinline-partial-on-nameof). Every csproj a row adds carries the
current template (read at each tip). Step 4t is a reading and commits nothing. A fixup of one metadata file, as at
P, is the likely shape; whatever it measures stops it for COORD when it is outside the allowed set.

## 3. Files

`tQ-CHANGES.md` section 0 is the inventory. New at Q: `tQ-resolve.py`, `tQ-ruled.txt`, `tQ-resolve-controls.sh`,
`tQ-controls.sh`, `tQ-selfcheck.sh`, `tQ-reread.sh`, `tQ-land-prep.sh`, `tQ-land-final-reads.sh`. A per-run copy
(floor 4) takes the folder's FILES and the seat list; M's two TE control patches are read at `../trainP/controls/`.
The premap set (`tQ-premap*.sh`, `tQ-regcheck.py`, `tQ-unioncheck.py`, `tQ-gofmt-parse.sh`, `tQ-footprints.sh`,
`tQ-hunks.sh`) is carried by rename only and is NOT part of this order of work; each file says so on its second line.

## 4. Manual steps

- **MS1. The seat list.** A row added, dropped or re-pointed: re-run the map, then walk the literal sites the
  assembler prints at its end (LITS). At Q the first of them is `tQ-ruled.txt`: a ruling names a row AND its sha.
- **MS2. H4.** No Q row adds a Go-only behavioral directory (read from each row's diff). precheck's H4 arm refuses one.
- **MS3, MS12, MS16 to MS18, MS25. DROPPED** (earlier trains' rows).
- **MS4. Read the REGEN_ALLOW the fixup stamps.** 75 tokens from two rows, each a file at HEAD, none with a
  `src/core/` prefix (the fixup refuses one at launch: P's RR1-1).
- **MS5. Review siblings and hand-owns.** `SIBLINGS=report`; HANDOWN must read 0 written. Q's hand-own writers:
  `reflect/value_impl.cs` (three rows), `testing/*.cs` (four), `runtime/managed_impl.cs`, `internal/abi/type_impl.cs`,
  `reflect/deepequal_impl.cs`, two darwin companions. READ `reflect/value_impl.cs` whole at the union.
- **MS7. SetegidBroadcastSeam** is never re-baselined whole (linux-exclusive; P1's LB and LX read it).
- **MS8, MS14. The registration files.** 22 rows insert into `go2cs.slnx` and the four lists (23 projects, 28
  csproj); ONE row conflicts there, by ruling (`tQ-map.md` B). EXPECT at the union: 959 Project lines, 816 / 816 /
  816 / 787 `[TestMethod]` lines, each equal to its list's `Check` method count; projitems +11 lines.
- **MS9. The TE controls** are arm TE-M of `tQ-controls.sh`. **MS9b** (precheck's and the LIVE gate's controls) and
  **MS9c** (the table's planted lists): the procedure is `trainP/tP-README.md`'s, with Q's names; MS9c gains three
  plants: a SLOT row, a CAND row with no `READING IN (` (the bare two words in prose do not release it), and the
  list's SLOT comments with `SLOTS_OK` unset (each must be refused, rc 2). Review round 1 ran them on the real table.
- **MS10. Darwin** is a mac CI dispatch, and so is the release gate (MS23).
- **MS11. At landing, when master moved:** merge it into the union, `git diff --stat $BASE <that sha>` read whole,
  the precheck again. `tQ-land-prep.sh` refuses a moved master rather than guess.
- **MS13. THE REFRESH OF COMMITTED -tests SOURCES** is a post-battery bank step (`tQ-land-prep.sh` 4a to 4e). The
  sources of record are the WINDOWS emission: the i7's S and T patches and the i9's patch, at ONE head, and the
  prep PROVES the i7's are that head's (it reads the run folder's own PRE and END lines: a stale run folder is
  refused; review round 1). At Q it is
  LARGE by construction: the carrier renders a no-inline method as a partial method and regenerated no committed
  test source (738 lines in 173 files at the base). The refresh's class census names the N-PARTIAL count; every OTHER
  hunk is read before the commit is signed.
- **MS15. PUB.** `PUB_MIN` 6 (no Q row touches the script). Every in-tree publish now keeps its dependency symbol
  files loose; a -tests publish refuses a host that lacks one (PUBSYM reads 0 such lines across the run). PUB reads a
  published program's OUTPUT only: leg **PUB2** (review round 1) publishes one behavioral program twice into one
  folder and gates the same file names and more than one `.pdb` after the second, the outcome i9-pack-symbols
  exists for; its control PUB2c is the row's off switch, a reading.
- **MS19. The hashset module** must be in the module cache (the module legs read `src/go2cs/go.mod` and refuse a cold
  cache by name).
- **MS20. The template-class csproj.** None predicted (`CSPROJ_TEMPLATE` stays at its default, `stop`).
- **MS21. BEFORE THE MAP.** The table's header line 4: c1-aot-smoke's RE-CUT (as cut it conflicts with
  c1-release-smoke-published in `os-matrix.yml`, a non-empty base no ruling can cover: `tQ-map.md` D2; the row says
  CAND until the re-cut tip is seated and read); the windows Native AOT failure behind the trim default (QUESTION 8);
  the two acceptance words COORD still owes by name; the two SLOT comments (seat them, delete them, or `SLOTS_OK=2`).
  The census-limit row's position is answered in the draft (directly below its base row, as rehearsed).
- **MS22. A NEW per-file `.editorconfig` in a behavioral project** stops the fixup for COORD's ruling, as at O and P.
- **MS23. THE HOSTED GATE** (C1): release-smoke on four RIDs (A to D, D gating; E and F measured, not gating), the
  darwin behavioral FULL, the darwin census, the docs-site build. Read BEFORE landing, and again after any fixup-N on
  a release-visible path. It is read at Q whether or not a release follows (checklist 5b).
- **MS24. A committed `.editorconfig` a conversion DELETES** is drift, refused by the regeneration, routed to a row.
- **MS26. `run-csharp-consumer.ps1` gates the landing**; COORD runs it on the i7 after the battery (checklist 6).
- **MS27. A release, IF the owner rules one:** the runbook's (`docs/GoCorpusMigration.md`), never a script here.
  `release-nuget.ps1` reads the SDK in Phase 0 since c1-release-nuget-preflight; give the owner's shell the
  environment block all the same.

### 4b. A fixup-N after the push

A red that needs a tree change after the union was pushed lands as `fixup-N: TRAIN Q` ON TOP (floor 9), signed,
single-parent; a row that leaves, leaves by a revert made as such a commit, and the battery relaunches with
`PRECHECK_MODE=warn` stamped and read by hand. What it obliges: a fresh battery from a fresh run folder; the i9 and
linux readings again; the i9's refresh patch is stale; C1's gate again when the commit touches a release-visible
path (golib, its packed targets, go2cs-gen, a hand-own under `src/core`, the corpus, `src/push-nuget.ps1`,
`src/tests/PackageTests`, the converter).

## 5. Legs

The battery's header (LEG -> SEAT MAP) is the table. New at Q: TRH, PPS51 / PPS7, T2, XS2 (inside MOD), PUBSYM, the
HOSTWALL baselines, EXIT 7, and since review round 1 PUB2 / PUB2c. Predictions, each derived again at run time: CB 44
added names + 6 base guards; CNR N = 863; 23 projects; GenTests 138; UF 0; T2 `runtime/debug` 8 twice; x/sync 28;
x/mod 9 of 9; step 4t net/rpc 6, log/slog 3, encoding/json 1. Exit statuses that mean 'green, and a re-read is
owed': the battery's 7, a standalone `tQ-modules-legs.sh`'s 7 and `tQ-linux-legs.sh`'s 7.

## 6. Deliberately NOT in the battery

HOP, SPB, a darwin run, the hosted gate, the PackageTests runners (they need a package feed), the MSTest behavioral
tier (the runner reads the programs here; leg TRH reads that tier's two new guards), any release step.

## 7. Open questions

`tQ-CHANGES.md` section 9 (Q-A to Q-L), its REVIEW ROUND 1 section (R1-A to R1-G) and the seat table's header line 4.

## 8. What was checked, and what was not

`tQ-DERIVE-REPORT.md`. In one line: every reader Q adds or changes was RUN on planted input, on P's kept records or
on git objects; no script was run for real, and nothing was built, converted or tested.
