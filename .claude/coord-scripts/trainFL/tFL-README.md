# TRAIN FL (the face lift): the apparatus, restated for FL (a SHORT README)

**DRAFT 2026-10-08.** FL re-converts the corpus and a release follows its landing (a separate runbook step: the owner
runs it). This file keeps every anchor the scripts cite (MS1 to MS27, 4b, the sections) and says what holds AT FL; where
the procedure text did not change it points to `trainQ/tQ-README.md`. Where this file and `COORD-LAUNCH-CHECKLIST.md`
differ, the checklist leads. Changes: `tFL-CHANGES.md`. The seat table: `tFL-seats-draft.txt`. The face lift's -tests
classes: `facelift-tests-hunk-classes.md` (G's, verbatim) and C2's three mailbox posts (62566dca42, 02ade5ea32,
21e7b8984a), as rules in `tFL-helpers.py` (FL1).

## 1. The order of work

| Step | What | Script |
|---|---|---|
| 0 | BASE from the remote (541766413e at the derive); fresh shell; row 12's sha written into the table; the table frozen | checklist 0 |
| 1 | Commit the drafts on the handover branch (done by the derive: COORD pushes) | checklist 1 |
| 2 | Controls: `tFL-controls.sh`, `tFL-selfcheck.sh`, the table check | checklist 2 |
| 3 | Rows 11 and 12: the map FROM the hand union (MAP_FROM=14c543bb06, FL8), then the assembler RESUMES on the union branch and merges them signed | `tFL-conflict-map.sh`, `tFL-assemble.sh` |
| 4 | The fixup (FIXUP_N=1): PRERES, the emission check, the -tests arm (4t), the goldens, ONE signed commit; the push; the module rehearsal (MOD standalone at the fixup head, FL10) and G's module re-reads, both BEFORE the battery | `tFL-fixup.sh`, `tFL-modules-legs.sh` |
| 5 | The battery (MOD early, testing off the i7) | `tFL-battery.sh` |
| 6 | The lanes' readings at the tip COORD names: the i9 shard, P1 / P2 linux, C1 release-smoke on four RIDs + darwin, G's modules | the two briefs |
| 7 | The landing: prep (with the i9's patch), the signed refresh, a fixup-N if one is owed (FL2), the re-reads at the head that lands, announce then push | `tFL-land-prep.sh`, `tFL-land-rereads.sh` |
| 8 | The release: the runbook's (`docs/GoCorpusMigration.md`), the owner runs it; never a script here | none |

## 2. What the fixup is predicted to carry

Nothing by name. Row 10 regenerated the -stdlib corpus with the union's converter (C2: "NO line outside the families on
any target"), so step 4 reads union-attributable 0 on three targets, as at Q; no row carries a corpus-footprint group,
so REGEN_ALLOW admits nothing. Row 10 re-baselined 809 behavioral projects, so 0 goldens are predicted to move; a
golden that moves by a face-lift class STOPS the fixup (GOLDEN_CLASS=facelift, COORD's call). Rows 11 and 12 carry no
emission.

## 4. Manual steps (Q's numbers; only what differs at FL is written)

- **MS1. The seat list.** Row 12's sha is OWED (a SLOT row until written). Rows 1-10 are hand merges: the map starts AT
  them (MAP_FROM). The literal sites the assembler prints (LITS) are Q's list with FL's names.
- **MS2. H4.** Rows 1, 3 and 7 add behavioral projects WITH their csproj (CallerLineCallSite, CallerLineMultiLine,
  GenericFuncInstantiationArg): no Go-only directory.
- **MS4. REGEN_ALLOW** composes to nothing (no footprint group in any row).
- **MS8, MS14. The registration files** (read with precheck at 14c543bb06, objects only): go2cs.slnx 962 projects,
  the four lists 819 / 819 / 819 / 790 `[TestMethod]` lines, each equal to its Check count; projitems +17 lines.
  Rows 11 and 12 may add more: read at the fixup head.
- **MS9.** The controls are `tFL-controls.sh` (25 arms) and `tFL-resolve-controls.sh` (11 arms).
- **MS11. At landing, when master moved:** merge it into the union, read `git diff --stat $BASE <that sha>` whole,
  precheck again. `tFL-land-prep.sh` refuses a moved master.
- **MS13. THE REFRESH OF COMMITTED -tests SOURCES** is the face lift's largest landing step: 5251 leaving-attribute
  lines in 924 committed test sources at the base (git grep, the regex of tFL-fixup.sh step 4t). The sources of record
  are the WINDOWS emission (the i7's S and T patches and the i9's patch at ONE head). 4e prints the FACELIFT count and
  its steps; every OTHER hunk is read by name before the refresh is signed.
- **MS15. PUB.** `PUB_MIN` 6 (no FL row touches the gate).
- **MS20. The template-class csproj.** None predicted (the three new behavioral csproj read `new-ok`).
- **MS22, MS24.** As at Q.
- **MS23. THE HOSTED GATE** (C1): release-smoke on four RIDs and aot-smoke (row 5 reads three RIDs and states the
  fourth), the darwin behavioral FULL, the darwin census, the docs-site build (row 12 regenerates the samples). Read
  BEFORE landing, and again after any fixup-N on a release-visible path. A release follows FL: this gate is not optional.
- **MS26. The C# consumer runner** gates the landing; `tFL-land-rereads.sh` step 5 runs it with `-WorkRoot`.
- **MS27. The release**: the runbook's; give the owner's shell the environment block (`release-nuget.ps1`).

### 4b. A fixup-N after the push

A red that needs a tree change after the union was pushed lands as `fixup-N: TRAIN FL` ON TOP, signed,
single-parent; at the landing it may sit ON the refresh (Q's route: fixup, refresh, fixup-2; the fixup, the battery
and the lane drivers admit that chain since FL2). What it obliges is Q's list (a fresh battery or the named re-reads,
the i9 and linux readings again, C1's gate on a release-visible path).

## 5. Legs

The battery's legs are Q's, with MOD moved to the first hour (FL10) and `testing` off the i7 (FL6). Predictions
derived from the objects at 14c543bb06: CNR N = 866 (863 + 3 new projects; 7 linux skips); CB 21 `+func Test` added
under src/go2cs plus the base guards (rows 11 and 12 add their own: derived at run time); GenTests `[TestMethod]`
lines 166 (C2 read 188 / 188 at the regeneration); x/sync 28, x/mod 9 of 9 (Q's re-read at de97fb2d6a).

## 6. Deliberately NOT in the battery

As at Q: HOP, SPB, a darwin run, the hosted gate, the PackageTests runners, the MSTest behavioral tier, any release step.

## 8. What was checked, and what was not

`tFL-CHANGES.md` section 3. In one line: every reader FL adds or changes was RUN on planted input, on Q's kept records
or on git objects (the union's own regenerated corpus among them); no script was run against the tFL worktree, and
nothing was built, converted or tested.
