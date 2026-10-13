# TRAIN T4: the apparatus, restated for T4 (a SHORT README)

**RUN2 2026-10-13.** Row 2 is re-seated at claude/c1-comment-parity-gap-fix fbf2a5ebc5 (the var/const doc-gap fix 07cacea829). The union is train-t4-union-r2 in /h/go2cs-tmp-coord/tT4r2 over the same BASE 226afb810a; there is no i9 rehearsal (T4-13 compares with run1's union tree instead); battery leg NGF-j0 is retired (T4-14). Run1 is the reference and is not modified. Changes: `tT4-CHANGES.md` section 5. Where run1's numbers below (shas, trees, the footprint) differ for run2, the run2 report and section 5 lead.

**DRAFT 2026-10-12.** T4 is NOT a corpus re-conversion, though two rows change the converter's emission and carry their
own footprint IN the seat (row 2, comment parity A to G; row 9, `.Raw()` on multi-line raw strings). This file keeps
every anchor the scripts cite (MS1 to MS27, the sections); where T3's procedure text did not change it points to
`trainT3/tT3-README.md` (and through it to FL's and Q's). Where this file and `COORD-LAUNCH-CHECKLIST.md` differ, the
checklist leads. Changes: `tT4-CHANGES.md`. The seat table: `tT4-seats-draft.txt` (11 rows, FROZEN at the derive).

## 1. The order of work

| Step | What | Script |
|---|---|---|
| 0 | BASE from the remote (226afb810a at the derive); fresh shell; the table frozen (every tip read at the remote; row 5 by sha) | checklist 0 |
| 1 | Commit the kit on the handover branch as `trainT4/` (COORD: announce, push) | checklist 1 |
| 2 | Controls: `tT4-controls.sh` (27 arms), `tT4-selfcheck.sh` | checklist 2 |
| 3 | The map FROM BASE, then the assembler on the LOCAL branch `train-t4-union`, with `I9_TREE=7e1f0969c1`. DONE BY THE DERIVE: union `7dfd1d493c`, MAP TREE EQUAL, I9 TREE EQUAL | `tT4-conflict-map.sh`, `tT4-assemble.sh` |
| 4 | The fixup (FIXUP_N=1): PRERES + PRERES-reg (the ruled projitems line), the emission check x3, 4t, the goldens (the two ruled projects), ONE signed commit; the module rehearsal at the fixup head. DONE BY THE DERIVE (see the checklist) | `tT4-fixup.sh`, `tT4-modules-legs.sh` |
| 5 | The probe worktree at `5f363aeae4` (`claude/g-trim-probes-more`), then the battery | `tT4-battery.sh`, `launch-battery.sh` |
| 6 | The lanes' readings at the tip COORD names: the i9 shard, P1 / P2 linux, C1's hosted gate | the two briefs |
| 7 | The landing: prep, the refresh, a fixup-N if one is owed, the re-reads at the head that lands, announce then push | `tT4-land-prep.sh`, `tT4-land-rereads.sh` |

## 2. What the fixup is predicted to carry

**MEASURED 2026-10-12 20:25 (the derive's fixup run): STEP 4 RED.** union-attributable 1241 / 1271 / 1260 (windows / linux /
darwin, 3772 in all), HANDOWN 0, csproj OK; the fixup stopped at the regen check with nothing written. The drift is
row 2's comment parity (comments moved to their Go position, blank lines kept) in production files: row 2 re-baselined
its goldens but committed NO src/core file. The prediction below is COORD's, kept as written.


Exactly two things (COORD 2026-10-12, and the i9's rehearsal by TREE): (a) ONE line in `src/go2cs/go2cs-src.projitems`,
`retiredHandOwns_test.go`, which row 11 adds and does not register (TestProjitemsRegistersEveryGoSource fails at the
union without it); (b) the union regeneration of RawStringLineEndings and X509SystemVerify (6 files: each project's main
.cs, its .cs.target and package_info.cs), where row 9's `.Raw()` and row 2's scope-A blank lines meet. Step 4 reads
union-attributable 0 on windows, linux and darwin, regen applied 0, HANDOWN 0, csproj OK; 4t as T3's; step 5 moves
exactly those two projects. The fixup's tree is PREDICTED equal to the i9's `0145a977f0`.

## 4. Manual steps (T3's; only what differs at T4 is written)

- **MS1. The seat list.** 11 rows, no SLOT, no CAND, no stack; FROZEN. Row 5 BY SHA (its remote tip is one docs commit
  past the seat: T4-2). LITS: T3's list, plus `H3_TOKENS` (one entry, T4-7), `GOLDEN_RULED`, `GN_TRIM_T4`, `TP_PIN`.
- **MS2. H4.** Rows 1 and 9 add behavioral projects WITH their csproj (X509SystemVerify, RawStringLineEndings): no
  Go-only directory (precheck H4 read 0).
- **MS4. REGEN_ALLOW** composes to nothing (no row's notes carry a CORPUS FOOTPRINT group: rows 2 and 9 committed theirs).
- **MS8, MS14. Registration** (precheck at 7dfd1d493c, 0 hard failures): go2cs.slnx 969 projects (+2), the four lists
  826 / 826 / 826 / 797 `[TestMethod]` = Check, projitems 576 lines / 562 keys (+25 lines from rows 2 and 6), +1 line
  after the fixup.
- **MS9.** The controls are `tT4-controls.sh` (27 arms) and `tT4-resolve-controls.sh` (14 arms with C11, C11n, C11k).
- **MS10. CNR's script**: no T4 row changes it (T4-10).
- **MS11, MS13, MS15, MS23, MS26, MS27.** As at T3.

## 5. Legs

T3's legs; predictions derived from the objects at 7dfd1d493c:

| Leg | Prediction |
|---|---|
| CNR (4) | N = **873** (880 enumerated, 7 platform skips; master 871 + RawStringLineEndings, X509SystemVerify; the i9 read 873) |
| CB | **31** `+func Test` added under src/go2cs + the 6 base guards = 37 names |
| GN | Total **214** (floor 195 method lines; the i9 read 214); the 21 GN_TRIM names Passed |
| TP | consumer RUNS-CLEAN; genprobe, c32a, c32b, c32c, registry, stringer, fieldbox GO-EQUAL (8 of 8) |
| GT | the added GolibTests classes FieldBoxTableRouteTests, GoroutineCreatorSiteTests, PromotedFieldIdentityTests, RawStringLiteralTests (derived by PRE-D) |
| E | union-attributable 0 x3, csproj OK |
| G1/G2 | the roster guard at 2115 checks under both PowerShells (the i9's reading; row 7 changes the guard) |
| MOD | as T3; read the module rehearsal at the fixup head first |
| TE / TE-T / TE-i9 | ~0 content rewrites (rows 2 and 9 committed their -tests footprints in-seat) |

## 6. Deliberately NOT in the battery

As at T3.

## 8. What was checked, and what was not

`tT4-CHANGES.md` section 3.
