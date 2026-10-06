# TRAIN Q: the rehearsal map, restated as the seat table needs it

**DRAFT 2026-10-06 (step 1 of the tQ derive), UNCOMMITTED.** It replaces a pre-map: the i9 merged the accepted
seats for real, on Windows, and read the whole union. This page restates that rehearsal against
`tQ-seats-draft.txt` (64 rows since review round 1), adds what was read from git objects on the i7 while the
table was written, and lists every place the two differ.

**REVIEW ROUND 1 (2026-10-06, 16:50 to 17:50 Central): four changes to the table, all restated below.** (1) The
table has 64 rows. `c1-aot-smoke` 44f29d50e6 is row 56, directly below `c1-golib-trim-default`; the rehearsal never
met it and AS CUT IT DOES NOT MERGE (section D2). (4, added at the round's third pass) Two BOARD docs rows the
ledger accepted for Q at 16:55 and 17:38 are rows 63 and 64, seated last: `c1-board-aot-partial-cost` 513ab62499
and `g-board-logrus-launchdir-line` 5497b2d7b9; each is one more BOARD hunk of the ruled kind (section A). (2) `g-board-cs8500-census-limit` 2b3aeb0630 is seated directly below
its base row (row 41), the position the i9 rehearsed; the eleventh pair of the first draft is gone (sections A and
D). (3) `c2-scout-marker-parity` is seated at 63eb75c444 (one more docs commit on the plan, taken by COORD at
16:34). Row numbers in this page are the 64-row table's (rows 1 to 62 are the 62-row state's). Against the first draft: rows 1 to 40 and 44 to 55 kept
their numbers; the three G BOARD rows are 41 (census-limit), 42 (foreign-defined-directions) and 43
(testing-t-deepequal), where the first draft had 43, 41 and 42; rows 56 to 61 of the first draft are rows 57 to 62.

**The base moved twice while the round ran**: master was 7098b8d3f9 (every reading of the first draft), then
b6e4856883 (16:49 Central) and 0457242046 (16:55), two signed docs commits that touch `docs/KnownIssues.md` and
`docs/Limitations.md` only. No row touches either page, every row's merge base is unchanged, and the simulation's
whole output on the 62 rows is byte-identical on the three bases (`diff` of the three result files: empty). 'BASE'
below names 7098b8d3f9 wherever a count is given; each count reads the same at today's master.

Nothing here was built, converted or tested on the i7. What was run: `git ls-remote`, object reads through
`/h/Projects/go2cs` (`rev-parse`, `merge-base`, `diff`, `log`, `ls-tree`, `cat-file`, `patch-id`, `grep`),
fetches by ref name (the fix-up ref once, the mailbox branch three times), and `git merge-file -p` on temporary
files under `coord-scratch/tQ/derive/sim/`. No commit, tree or blob was written to the repository.

Names: **row n** = the nth row of `tQ-seats-draft.txt`; **v0 n** = the nth row of COORD's generated list
`coord-scratch/tQ/tQ-seats-draft-v0.txt` (v0 1 to 40 are rows 1 to 40; v0 41, 42, 43 are rows 42, 43, 41; v0 44 to 55 are rows 44 to 55; v0 56 to 59 are
rows 57 to 60; v0 60 is row 62; rows 56 and 61 are not rows of v0); **L:n** =
`trainL/tL-seats-draft.txt` line n.

## 0. The sources

| What | Where | Read |
|---|---|---|
| The rehearsal, step 1: the conflict map | the i9's post `20261006T143431Z-i9.md` (mailbox) | whole |
| The restack | the i9's post `20261006T145239Z-i9.md` | whole |
| The rehearsal, step 1: the readings | the i9's post `20261006T174143Z-i9.md` | whole |
| The rehearsal, step 2: the five late rows | the i9's post `20261006T181555Z-i9.md` (NOT in the local mailbox clone at b4ebd4d2b0: the mailbox branch was fetched by ref name and read with `git show`, tip 14d7a70ed0) | whole |
| The fix-up ref | C2's post `20261006T181627Z-C2.md` | whole |
| The rehearsal, the fix-up re-read | the i9's post `20261006T185725Z-i9.md` (mailbox commit 90129ee345; newer than COORD's notes, whose last line is L:368 of 13:47) | whole |
| COORD's rulings | `trainL/tL-seats-draft.txt` L:257 to L:369 (L:369 was written at 15:00, after the first draft) and the ledger's lines of 10-06 through 15:29 (mailbox, `docs/phase4/LEDGER.md`) | whole |
| The aot-smoke seat and the plan branch's new tip (review round 1) | C1's posts `20261006T200223Z-C1.md` (the cut) and `20261006T210418Z-C1.md` (release-smoke 37523436862 green on four RIDs); COORD's `20261006T213417Z-COORD.md` (accepted) and `20261006T213437Z-COORD.md` ('I TAKE 63eb75c444 as the row's tip'); the mailbox branch fetched by ref name, tip c62137e061 | whole |
| The two BOARD docs rows and P2's reading (review round 1, third pass) | the ledger's lines of 16:55 and 17:38 on 10-06; C1's post `20261006T213643Z-C1.md`, COORD's `20261006T223702Z-COORD.md` (G) and `20261006T223745Z-COORD.md` (P2); the mailbox branch at 30e76cfcd8 | whole |
| Every row's own change | `git diff --name-status <merge-base with BASE> <sha>` and the row's own commits | 64 rows |

The rehearsal unions are LOCAL to the i9 and were never pushed: `04141b5143` (54 seats), `5306e760c4` (the same
without the carrier), `07abaa0e9c` (+ the restacked i9 row: 55 seats), `65f087cc5a` (the i9's 'without step 5 +
the restack': the 55 less the carrier), `7d9fa629e3` (+ the five late rows: 60 seats), `2503342347` (+ the fix-up:
61 seats: the table's set less `c1-aot-smoke`, which was pushed after it). None of those objects is on the i7, so no tree of the rehearsal could be compared with
anything here: the comparison below is between the i9's WORDS and a second, object-read derivation.

## 1. The instrument used here, and its controls

`coord-scratch/tQ/derive/sim.py` walks the rows in order, one path at a time. For each row it takes the merge base
the real merge would use (`git merge-base --all <row> BASE <every earlier row>`; when that is two commits it takes
the row's own internal merge of exactly those two), and for each path whose blob the row changes it either takes
the row's blob (the union still holds the base's), or runs `git merge-file -p --diff3` on three temporary files.
A conflict is counted, its hunks are read (ours / base / theirs line counts) and it is resolved the ruled way, both
sides whole, ours first, so the next row has a file to meet. It is NOT a merge: no rename detection, no tree. Its
only claim is WHICH row meets WHICH earlier rows in WHICH file, and with what kind of hunk.

A gate that has never been made to fail proves nothing, so it was run on four lists at the first draft, and on
three more at review round 1 (a copy of the script under `derive/rr1/`, the base a variable):

| List | Rows | Conflict (row, file) pairs | Reading |
|---|---|---|---|
| The rehearsal's own merge sequence (v0 1-39, 41-42, 44-47, 49-53, 56-59, 48, 40, 43, 54-55, 60) | 60 | **10** | exactly the i9's enumeration: the BOARD at 3196a93cdc, 5aab7c91f2, 6481ddbabb, e13e0e2ecc and (step 2) be4ce10078; `go2cs.slnx` + the four lists at 0c6a2618fa; nothing at the restack, 2b3aeb0630, dae4ce596f, ab6aa8f443, 633b045e8a. **The instrument reproduces the rehearsal.** |
| The 56 tips of step 1 with the two ORIGINAL refs in their places (850ff04577, 9ff16da875) | 56 | **11** | the nine of step 1 + `src/go2cs/testConversion.go` at 850ff04577 (1 hunk, empty base, 7 lines against 5) + `src/tests/PackageTests/README.md` at 9ff16da875 (1 hunk, empty base, 22 against 10): the two the i9 left out, in the files it named. **The instrument fires where the rehearsal fired.** |
| v0's order, the FIRST DRAFT's table (v0's 60; and the 61 with the fix-up row) | 60 / 61 | **11** | the ten + ONE the rehearsal never met: the BOARD at 2b3aeb0630 (section D) |
| v0 with 2b3aeb0630 moved to directly below be4ce10078 | 60 | **10** | the rehearsal's count again |
| THE TABLE AS SEATED at review round 1, without `c1-aot-smoke` (the plan branch at 63eb75c444) | 61 | **10** | exactly the ten lines of `tQ-ruled.txt`; 159 content merges clean; 637 paths touched |
| THE TABLE AS SEATED, 62 rows | 62 | **12** | the ten + TWO at row 56 `c1-aot-smoke`: `.github/workflows/os-matrix.yml`, 2 hunks with a NON-EMPTY base, and `docs/CIMatrix.md`, 1 hunk, empty base, 6 lines against 11 (section D2); 161 content merges clean. The same 161 and 12 with COORD's unpushed master commit 3a1260d859 as the base |

| THE TABLE AS SEATED, 64 rows (base 0457242046, master at 16:55) | 64 | **14** | the twelve lines of `tQ-ruled.txt`, every one used and none unused (the ten above + the BOARD at rows 63 and 64: 286 lines against 11, 297 against 29, empty base) + the two at row 56; 161 content merges clean; the BOARD 26507 lines, guard final |

Every one of those hunks has an EMPTY BASE (an insert against an insert at one point) but two: the simulation read
no hunk in which both sides edit the same lines in any of the first five lists, and exactly two in the sixth and
the seventh, both in `os-matrix.yml` at `c1-aot-smoke` (section D2).

## A. The BOARD: ten writers, one insertion point (eight at the first draft; two docs rows seated last since)

`docs/phase4/BOARD-next-validation-candidates.md` is append-only, and every append goes directly above its final
guard line. A row that is not cut on an earlier writer therefore conflicts with whatever has been appended before
it. Master's BOARD is byte-identical from the oldest BOARD cut (446d2c8ba0) to BASE, so row 1 takes the file clean.

| Row | Ref | Sha | Own lines | In the table's order | Hunk (ours / theirs lines) | In the rehearsal |
|---|---|---|---|---|---|---|
| 1 | c2-board-assignability-gap | 476fbd381c | +50 | takes the file | none | clean |
| 18 | c2-module-disclosures | c585fa199d | +89 | clean: it carries row 1 inside it (its internal merge b124dfe215) and its lines sit below row 1's | none | clean |
| 19 | c2-func-chan-dir | 3196a93cdc | +35 | **hunk 1** | 139 / 35 | hunk, kept both |
| 20 | c2-board-warnings | 5aab7c91f2 | +45 | **hunk 2** | 174 / 45 | hunk, kept both |
| 40 | g-cs8500-managed-view | be4ce10078 | +27 | **hunk 3** | 219 / 27 | hunk, kept both (step 2: merged LAST, so against 249 lines) |
| 41 | g-board-cs8500-census-limit | 2b3aeb0630 | +10 | clean: seated directly below its base row since review round 1 (its 10 lines go below that row's 27) | none | clean (merged directly behind be4ce10078) |
| 42 | g-board-foreign-defined-directions | 6481ddbabb | +26 | **hunk 4** | 256 / 26 | hunk, kept both (against 219) |
| 43 | g-board-testing-t-deepequal | e13e0e2ecc | +4 | **hunk 5** | 282 / 4 | hunk, kept both (against 245) |

| 63 | c1-board-aot-partial-cost | 513ab62499 | +11 | **hunk 6** (review round 1, third pass) | 286 / 11 | NOT REHEARSED (pushed 21:36Z, behind the last union) |
| 64 | g-board-logrus-launchdir-line | 5497b2d7b9 | +29 | **hunk 7** | 297 / 29 | NOT REHEARSED (pushed 15:00 Central; accepted 17:37) |

SEVEN hunks as seated: five among the first 62 rows and one for each of the two docs rows seated last. In v0's
position (below row 43) the census-limit row is one more, one the rehearsal never made: 30 lines against its 10,
empty base (at the 61-row draft: the sixth BOARD hunk and the seventh conflicted merge: section D).

**The ruled resolution: keep both in merge order** (L:352 for row 19: 'E's rows then D's'; L:359 and the i9's
map for the rest): the lines already in the union stay where they are, the row's lines go below them, the guard
line stays the final line.

EXPECT at the union, read from the simulation: 26181 lines at BASE, **26507** with all ten rows (+326 = 50 + 89 +
35 + 45 + 27 + 10 + 26 + 4 + 11 + 29; 26467 with the first eight, the first draft's figure); the board guard reads one raw opener, one endraw, endraw the final line (2 guard lines
in the simulated file). A count below 26467 means a resolution dropped lines; `merge-file --union` does exactly
that on this file (it read 26458: nine lines the two sides shared were written once).

The ORDER of the blocks in the file follows the merge order, so the table's BOARD is not the rehearsal's: in the
rehearsal be4ce10078's entry and its addendum come last; in the table they come before 6481ddbabb's and
e13e0e2ecc's entries. No reading depends on that order; the docs-site build and the board guard read the file.

## B. The registration files: one row, five files, and the form the resolution is made from

`src/go2cs.slnx` and the four behavioral lists (`TranspileTests.cs`, `CompileTests.cs`,
`TargetComparisonTests.cs`, `OutputComparisonTests.cs` under `src/tests/Behavioral/BehavioralTests/`) take
inserts from 22 rows: 23 new fixture folders, 28 csproj. Entries are kept in name order, so two rows conflict
only when their fixtures sort next to each other.

| Row | Ref | Sha | File | Hunk (ours / theirs lines) | Why |
|---|---|---|---|---|---|
| 35 | g-named-pointer-equality | 0c6a2618fa | `src/go2cs.slnx` | 2 / 1 | `NamedPointerEquality` sorts directly before `NamedPointerFieldAddress` and `NamedPointerReflectElem`, which rows 15 and 14 inserted at that one point (below `NamedArrayZeroValue`, above `NamedPointerReinterpret`) |
| 35 | | | each of the four lists | 6 / 3 | the same three fixtures, three lines an entry |

That is the i9's map exactly (0c6a2618fa: slnx and all four classes, one hunk each). No other of the 22 rows
conflicts in a registration file, in the rehearsal or in the simulation. `src/go2cs/go2cs-src.projitems` takes one
line from each of 10 rows (two from c2-module-disclosures) and never conflicts.

**The resolution MUST be made from the diff3 form.** Measured on this row's `TranspileTests.cs` with
`git merge-file` (git 2.35.2) on the three blobs the merge meets (the union so far, the row's cut, the row's tip):

| Form | What git writes at the hunk | Result of 'keep both' |
|---|---|---|
| `--diff3` (the i9's form; the base part is empty) | ours = 6 lines (two entries, each a method line, a blank line, a `[TestMethod]` line); theirs = 3 lines | 2468 lines = ours + 3. Right: every method keeps its attribute line (at the union, 816 `[TestMethod]` lines for 816 `Check` methods in this list) |
| the default conflict style | git REFINES the hunk: the blank line and the `[TestMethod]` line the two sides share move OUT of the conflict; ours = 4 lines, theirs = 1 line | deleting the markers leaves `CheckNamedPointerEquality` directly below `CheckNamedPointerReflectElem` with no attribute line above it |
| `git merge-file --union` | the same refinement, markers already gone, rc 0 | 2466 lines = ours + 1: the method compiles and **silently is not a test**, once in each of the four lists |

`go2cs.slnx` loses nothing under any form (one line an entry). A real merge writes its conflict through the same
refinement unless the conflict style is diff3 (not measured on a real merge here: no merge was made on the i7;
`git checkout --conflict=diff3 <path>` rewrites a conflicted path in that form). The gates that catch the wrong
form: precheck's COUNT arm (each list +3 for this row; +69 for the train), and a count of `[TestMethod]` lines
against `Check` methods per list. The i9 resolved in diff3 form and its behavioral total matched its own
prediction (3268), so the rehearsal union holds the right text.

EXPECT at the union, read from the simulation with the resolutions made in diff3 form:

| File | BASE | Union (61 rows, and the same at 62 and at 64: `c1-aot-smoke` and the two docs rows write none of these) | Delta |
|---|---|---|---|
| `src/go2cs.slnx` | 1126 lines, 931 Project lines | 1154 lines, **959** Project lines | +28 (the i9 predicted 959 projects and built them with 0 errors) |
| `TranspileTests.cs` | 2408 lines, 793 `[TestMethod]` | 2477, 816 | +69, +23 |
| `CompileTests.cs` | 2412, 793 | 2481, 816 | +69, +23 |
| `TargetComparisonTests.cs` | 2462, 793 | 2531, 816 | +69, +23 |
| `OutputComparisonTests.cs` | 2407, 764 | 2476, 787 | +69, +23 |
| `src/go2cs/go2cs-src.projitems` | 510 lines | 521 | +11 |

The 23 new fixture folders, by row: AnonStructAliasedConversion (5), SelfContainingMapHolder (6),
NamedMapNestedAssign (7), NamedMapClone (8), NamedMapMethodShadow (9), NamedPointerValueSlot (12),
DefinedForeignStructConvert (13), NamedPointerReflectElem (14), NamedPointerFieldAddress (15),
ReflectNamedFuncAssign (16), FuncValueMethodClash (17), ReflectFuncChanDir (19), ExportedAliasUnexportedTarget
(22), LocalNamedPointerConversion (24), TestingStructZeroLiteral (25), ParenthesizedTypeAssertion (26),
PromotedTargetParam (27), ForeignDefinedConversion (28), IfaceLiteralStringConversion (34), NamedPointerEquality
(35), StatementTableFuncNames (38), InitOrderNoInline and NoInlinePartial (60). Five of them hold a library
sub-project (rows 5, 13, 22, 27, 28): 28 csproj. All 28 carry the current template (the conditioned LangVersion
line once, TrimMode three times: read at each row's tip), so the fixup's csproj-template class has nothing
predicted.

## C. The stacks, the orders and the two multi-base rows

Read with `git merge-base --is-ancestor` for every pair of rows. A row carries ONE stack token, its nearest base
row; the rows above that one in the chain are covered through it.

| Chain | Rows (in merge order) | Note |
|---|---|---|
| The named-map generator chain | 6 c2-self-map-holder, 7 c2-map-wrapper-set, 8 g-named-map-clone, 9 c2-wrapper-member-shadow | row together and in this order (L:326, L:330); row 7 has a known defect that row 9 closes |
| The same-name alias pair | 10 g-ambiguous-alias-target, 36 g-same-pkgname-iface | 36 is cut on 10's tip |
| An ORDER, not a stack | 10 g-ambiguous-alias-target, then 11 c2-alias-twin-reach | 11 is cut on cc8b5f29e6, row 10's FIRST commit; row 10's tip is not its ancestor; the two share that commit under one sha |
| The named-pointer pair | 14 c2-namedptr-reflect-elem, 15 c2-namedptr-field-address | |
| The BOARD carry | 1 c2-board-assignability-gap, 18 c2-module-disclosures | by an internal merge: row 18 has TWO best common ancestors (d843ff263d, 476fbd381c) |
| The foreign-conversion chain | 24 g-local-namedptr-lift, 28 g-foreign-defined-hop, 29 g-foreign-written-rhs-on-hop | |
| The testing chain | 25 g-testing-nil-ctor, 31 g-detached-testing-t, 32 g-testing-runtests, 48 i9-address-variant-pairing-on-runtests | the last is the i9's restack |
| The CS8500 pair | 40 g-cs8500-managed-view, 41 g-board-cs8500-census-limit | adjacent since review round 1 (v0 has two BOARD rows between them: section D) |
| The symbols chain | 52 i9-pack-symbols, 54 c1-release-smoke-package-symbols, 55 c1-golib-trim-default (its condition met), 56 c1-aot-smoke (CAND: a re-cut is owed) | row 52 carries two accepted seats that are not rows (a395442f7b, 23450a2a6d) by an internal merge; row 54 carries two more refs by two internal merges, ONE best common ancestor; row 56 is one commit on row 55 and does NOT hold row 53 (section D2) |
| An ORDER, not a stack | 53 c1-release-smoke-published, then 54 and 56 | ruled 'fd2b421bba first' (L:354); row 56's order token becomes a second stack token with its re-cut |
| The safe-tag pair | 23 g-module-safe-tag, 58 c1-release-smoke-safe-tag | |
| The carrier and its riders | 60 c2-noinline-partial, 61 c2-noinline-partial-on-nameof, 62 g-r2m-caller-closure-v2 (ACCEPTED 14:59: no longer a candidate) | row 61 is also stacked on 38 g-nameof-runtime: TWO best common ancestors (e7fcff2244, ca436147c0); row 62 is cut on the carrier, not on row 61 |

Read from git for the 62 rows (re-read at review round 1), the table check's other arms: 0 rows already on the
base; 0 rows an ancestor of an earlier row; 0 rows that descend from an earlier row with no token covering it; 0
rows carrying a train-assembly commit the base lacks; 215 non-merge commits and 215 patch-ids across the rows and
master's own commits since each of the 8 distinct cuts, so 0 patches under two shas; 19 stack tokens and 3 order
tokens, every one verified; two MULTI-BASE notes (rows 18 and 61), which the assembler prints and does not refuse.
The table check's ONE refusal on the table as it stands is row 56 (CAND), by name. With rows 63 and 64 (one commit
each, cut on 7098b8d3f9, no stack) and master at 0457242046: 219 commits and 219 patch-ids, the same 19 stack
tokens and 3 order tokens.

## D. The rehearsal's seat set against the table's

**The set.** The rehearsal's step-2 union (7d9fa629e3) holds v0's 60 seats: the 54 of step 1, the restacked i9
row, and the five late rows. Its last union (2503342347) adds the fix-up: 61 seats, the table's set less `c1-aot-smoke`
(row 56, pushed 20:02Z, after that union: section D2), and with one tip older than the table's
(`c2-scout-marker-parity` a8db3c7e98 there, 63eb75c444 here: one docs commit, +20 lines in the plan, a file no
other row touches).

| Ref | Sha | Lane | In the rehearsal | In the table |
|---|---|---|---|---|
| i9-address-variant-pairing | 850ff04577 | i9 | LEFT OUT at step 1: `testConversion.go`, the testComparison struct, one hunk in CODE | superseded; not a row |
| i9-address-variant-pairing-on-runtests | 1f6e822670 | i9 | merged clean onto 04141b5143, giving 07abaa0e9c; every reading of step 1 was taken there | row 48, stacked on g-testing-runtests |
| c1-release-smoke-publish-symbols | 9ff16da875 | C1 | LEFT OUT at step 1: `src/tests/PackageTests/README.md`, one hunk in a README | rides inside row 54; not a row |
| g-cs8500-managed-view | be4ce10078 | G | step 2: merged, one BOARD hunk kept both | row 40 |
| g-board-cs8500-census-limit | 2b3aeb0630 | G | step 2: clean | row 41, directly below its base row: clean (in v0's position, two rows lower: **one more BOARD hunk**) |
| c1-release-smoke-package-symbols | dae4ce596f | C1 | step 2: clean ('its README resolution meets no conflict here') | row 54 |
| c1-golib-trim-default | ab6aa8f443 | C1 | step 2: clean | row 55 (its condition met; it rides only with row 56) |
| c1-aot-smoke | 44f29d50e6 | C1 | NOT IN THE REHEARSAL (pushed 20:02Z, after the last union) | row 56, CAND: **does not merge as cut** (section D2) |
| g-r2m-caller-closure-v2 | 633b045e8a | G | step 2: clean | row 62 (ACCEPTED 14:59, the condition met) |
| c2-noinline-partial-on-nameof | 404e1ddb75 | C2 | pushed 18:16Z, ACCEPTED 13:47 Central (L:368); merged LAST and clean into 7d9fa629e3, giving 2503342347 (the i9, 18:57Z) | row 61, directly below the carrier |

**The order.** v0 was generated lane by lane; the rehearsal merged in COORD's 09:21 order (L:358) and then added
what arrived. In v0 row numbers the rehearsal's merge sequence is 1-39, 41-42, 44-47, 49-53, 56-59, then 48, 40,
43, 54-55, 60, and the fix-up (row 61 of the table) last of all. FIVE seats were therefore merged later in the
rehearsal than the table merges them (rows 48, 40, 41, 54, 55); row 62 was the last of the 60 in both; and the
fix-up, merged last of all in the rehearsal, is seated one row above the last (it shares no path with row 62).
Row by row:

| Row | Ref | Rehearsed | Seated | What the simulation reads for the seated position |
|---|---|---|---|---|
| 48 | i9-address-variant-pairing-on-runtests | on top of the 54, the carrier included | before the i9's four later rows, C1's rows, P1's and the carrier | clean; `testConversion.go` content-merges (i9-tests-host-symbol-check then merges onto it: clean) |
| 40 | g-cs8500-managed-view | last but four | before the two G BOARD rows | the same ONE BOARD hunk, against 219 lines where the rehearsal's was against 249 |
| 41 | g-board-cs8500-census-limit | directly behind be4ce10078 | directly below it (review round 1) | clean, as rehearsed. In v0's position, two BOARD rows lower: **a hunk the rehearsal did not have**, 30 lines (the two G BOARD rows between) against its 10, empty base |
| 54 | c1-release-smoke-package-symbols | behind c1-release-smoke-safe-tag | before it | clean both ways: four content merges here (os-matrix.yml, CIMatrix.md, the PackageTests README, release-smoke.ps1), and row 58 then content-merges three files onto it |
| 55 | c1-golib-trim-default | behind everything but the last | directly behind row 54 | takes its two files |
| 62 | g-r2m-caller-closure-v2 | last | last | three content merges (conversionDriver.go, projitems, testConversion.go), clean |

So v0's order costs ONE resolution the rehearsal never made. Two ways out, both simulated:

1. **Rule it** (keep both in merge order): the addendum then sits two entries below the entry it annotates. Its
   words ('the CS8500 row above') still hold; it no longer reads as part of that entry.
2. **Seat the census-limit row directly below g-cs8500-managed-view** (the two G BOARD rows follow it):
   2b3aeb0630 is clean there and the count is the rehearsal's ten. The rows hash moves; nothing else does.

The first draft kept v0's order, as its task said, and asked (QUESTION 1). **REVIEW ROUND 1 TOOK WAY 2**: the
table seats the row at 41, so the order differs from v0 by that one row, and `tQ-ruled.txt` keeps way 1's line
commented, for COORD to uncomment if it moves the row back. Re-read on the seated order: ten pairs, the ten ruled
lines, the BOARD hunks 139/35 (row 19), 174/45 (row 20), 219/27 (row 40), 256/26 (row 42), 282/4 (row 43).

### D2. `c1-aot-smoke` 44f29d50e6 (row 56): a row the rehearsal never met, and it does not merge as cut

Pushed 20:02Z on 2026-10-06, one commit on `c1-golib-trim-default` ab6aa8f443; ruled its own dispatch at 14:59
(L:369); ACCEPTED 16:34 Central on C1's release-smoke 37523436862 (COORD's post, mailbox b040b1eebe). The i9's
last union predates it, so nothing below is a rehearsal reading: all of it is read from git objects.

| Check | Reading |
|---|---|
| The remote tip | 44f29d50e6483f98bafe181d822da2263511e384 (ls-remote, 16:35 Central) |
| Its shape | one commit on ab6aa8f443; `c1-release-smoke-published` fd2b421bba is NOT its ancestor (`merge-base --is-ancestor` rc 1) |
| Its own change | 5 files, +99/-18: `.github/workflows/os-matrix.yml` +56/-5, `docs/CIMatrix.md` +15/-3, `PackageSymbols/test-package-symbols.ps1` +17/-3, `PackageTests/README.md` +7/-4, `release-smoke.ps1` +4/-3 |
| In the union as seated (the simulation, 62 rows) | 1 file taken (`test-package-symbols.ps1`), 2 content merges clean (the README, `release-smoke.ps1`), **2 files in conflict** |
| `.github/workflows/os-matrix.yml` | **2 hunks, NON-EMPTY base** (ours / base / theirs lines 2 / 1 / 1 and 1 / 1 / 1). Both sides rewrite ONE line in two places, the pack job's `if:` and the feed-download step's `if:`. Base: `if: needs.plan.outputs.stage == 'release-smoke'`. Ours, from row 53: the same with `&& needs.plan.outputs.published-version == ''` appended (and a comment line above the first site). Theirs: the same with a second alternative appended, `needs.plan.outputs.stage == 'aot-smoke'` |
| `docs/CIMatrix.md` | 1 hunk, EMPTY base: 6 lines (rows 53 and 54) against 11 |
| The same, directly | `git merge-file -p --diff3` on three blobs: ours = fd2b421bba's `os-matrix.yml` merged with dae4ce596f's over c0c876fb27's (rc 0, clean); base = ab6aa8f443's (byte-equal to dae4ce596f's); theirs = 44f29d50e6's: **rc 2**, the two hunks above |
| The rows behind it | `c1-release-smoke-safe-tag` (row 58) content-merges `CIMatrix.md`, the README and `release-smoke.ps1` onto the kept-both text, clean |

What it means. The `os-matrix.yml` hunks are workflow LOGIC (which stages run the pack job and fetch its feed):
neither side's line is right for the union, which needs the two conditions COMPOSED. `tQ-ruled.txt` has one kind,
append-both on an empty base, and `tQ-resolve.py` refuses a non-empty base (control C4), so `tQ-conflict-map.sh`
reads row 56 NOT CLEAN and `tQ-assemble.sh` would stop there with exit 3. No order avoids it: the row is stacked
on the symbols chain, which is ruled to follow fd2b421bba, and seating fd2b421bba later only moves the same
conflict to that row's merge.

The way out is C1's: a re-cut, a new ref on 44f29d50e6 that merges fd2b421bba in and composes the conditions (and
keeps both inserts in `CIMatrix.md`), as C1 did for 9ff16da875 inside dae4ce596f. The row then names the re-cut
tip and carries two stack tokens (its base row and `c1-release-smoke-published`), the table check prints a
MULTI-BASE NOTE for it, and the hosted reading is owed at the re-cut tip: run 37523436862 read 44f29d50e6 alone,
a tree WITHOUT fd2b421bba's `published_version` condition. Until then the table marks the row CAND and the table
check refuses it by name. The other way is COORD's: strike both trim rows for Q (`c1-golib-trim-default` rides
only with this row: without it, release-smoke at the union still runs arm F's Native AOT sub-check, the one that
cancelled three of four RIDs in probe 37495989079).

## E. The one golden owed, and the row that carries it

The rehearsal read every leg as predicted but one fixture, on both instruments:

- `StatementTableFuncNames`, new in g-nameof-runtime ca436147c0 (row 38). Its committed `main.cs` and
  `main.cs.target` are one blob (6535478deb) cut before the carrier, with one go:noinline function in the
  attribute form: `[MethodImpl(MethodImplOptions.NoInlining)] internal static void table() {`.
- The union's converter (the carrier, row 60) writes `internal static partial void table() {`. So
  C3_TargetComparison fails (1 of 3268) and CNR lists that one `main.cs` (1 of 870 package folders). Transpile,
  Compile and Output pass: a golden, not a behaviour.
- Attribution, measured by the i9: byte-identical on its union without the carrier (65f087cc5a).
- Step 2 read the same one file and nothing else with the five late rows merged (the three fixtures those rows
  touch re-emit byte-identical: NoinlineDirectiveFrame, RuntimeCallerFrames, StdLibInternalAbi).

RULED 13:11 (L:366): C2 cuts `claude/c2-noinline-partial-on-nameof` = the carrier's unmoved tip + a merge of
ca436147c0 + the re-baseline, tabled directly below the carrier. Read from git at 404e1ddb75:

| Check | Reading |
|---|---|
| The remote tip | 404e1ddb75ff87e4dd95dd71d0f91d840ef576f6 = the sha in C2's post and in L:368 |
| Its shape | e7fcff2244, then 87b05957f3 (a merge of ca436147c0, parents exactly those two, combined diff empty), then 404e1ddb75 |
| Its own change | 2 files, +2/-2: the one line in `main.cs` and in `main.cs.target` |
| The pair at its tip | one blob, cedecfa246 |
| Against the carrier | 14 files, +733/-3 = g-nameof-runtime's commit and the two lines (C2's post says the same) |
| In the union (simulation, 61 rows; the same at 62) | the row brings in exactly the two files; 0 content merges; the union's `main.cs` and `main.cs.target` are both cedecfa246 |
| g-r2m-caller-closure-v2 | does NOT descend from it (cut on the carrier's tip); the two share no path |

**The i9's re-read is IN** (its post of 18:57Z): the ref merged `--no-ff` into 7d9fa629e3 CLEAN, union 2503342347;
it brought in exactly `StatementTableFuncNames/main.cs` and `main.cs.target`, one line each (what the simulation
predicted from objects); CNR whole reads NO REGRESSION, generated C# and csproj byte-identical across the 863
behavioral packages measured (870 transpiled, the 7 linux-exclusive skipped, changed files none); the fixture's four
phases 4 of 4 with 0 build errors. The i9 calls the rehearsal complete; COORD's ledger line of 14:24 (mailbox
15e39e869d) records it, and the running notes still end at L:368.

## F. The rehearsal's readings, with the union each was taken at

| Leg | Reading | Union | Note |
|---|---|---|---|
| go test ./... (src/go2cs) | 5 packages ok, 0 FAIL, 458 s | 07abaa0e9c | 282 s and test funcs 1188 -> 1193 at 7d9fa629e3 |
| go2cs.slnx | 0 build errors, 959 projects | 07abaa0e9c | inside the suite |
| GenTests | 138 / 138 | both | |
| ChannelTests | 24 / 24 | 07abaa0e9c | |
| GolibTests Debug | 1597 total, 1564 passed, 30 skipped, 3 failed | 07abaa0e9c | the 3 = that box's link-staging trio (no symlink privilege) |
| GolibTests Release | 1597 total, 1572 passed, 22 skipped, 3 failed | 07abaa0e9c | the same trio |
| BehavioralTests, every phase | 3268 total, 3239 passed, 28 skipped, 1 failed | 07abaa0e9c | 28 = the 7 linux-exclusive fixtures x 4; the 1 = section E; 2 h 21 m |
| CNR (-Revert) | 870 package folders transpiled, 7 platform-exclusive skipped, 1 changed | both | the 1 = section E |
| The five fixtures of step 2, four phases | 20 tests, 19 passed, 1 failed | 7d9fa629e3 | the 1 = section E |
| CNR (-Revert), whole | NO REGRESSION: 870 transpiled, 7 skipped, 0 changed | 2503342347 | the fix-up merged |
| StatementTableFuncNames, four phases | 4 of 4, 0 build errors | 2503342347 | section E's red closed |
| WHEA | 0 on every leg | all three | |

The full behavioral suite, the two GolibTests configurations, ChannelTests and the solution build were read at
07abaa0e9c only (55 seats). The five late rows and the fix-up were read by go test, GenTests, CNR and their own
fixtures. No leg of the rehearsal read the full suite at the 61-seat set: the battery's leg 5 is that reading.

NOT read by the rehearsal: the validated sweeps (S and T rows), the module legs, the linux and darwin legs, leg E
(the emission check against the corpus), the hosted release gate, the PackageTests runners, precheck. Those are
the battery's, the lanes' and C1's.

## G. Where this reading and the i9's map differ

1. **The count 'six'.** The step-1 post's title says '6 append-only kept both'. Its own list is NINE (merge,
   file) hunks: the BOARD in four merges and five files in one merge. Six is the number of distinct FILES (the
   BOARD, slnx, four lists). With step 2: ten hunks, six conflicted merges, the same six files. The simulation
   reads the same ten in the rehearsal's order.
2. **The census-limit row.** One hunk more in v0's order than in the rehearsal's (section D); none as seated since
   review round 1 (row 41).
3. **The BOARD's block order** differs between the two unions (section A): a text difference, no reading.
4. **The restack's numstat.** The i9's post words the restack's footprint as `testConversion.go` +126/-21 and its
   test +91. `git diff --numstat` reads +106/-20 and +90/-1 for 1f6e822670 against 24bb3cf68b, and the SAME two
   lines for 850ff04577 against its own base: the post's claim that the footprint is identical holds; its two
   figures are not numstat's.
5. **`c1-aot-smoke` (row 56)** is in the table and not in the rehearsal, and as cut it conflicts (section D2).
6. **Nothing else.** The two left-out refs conflict in the simulation's control exactly where the i9 named them;
   every other merge is clean in both.

## H. The hot files and who writes them

Own changes only (a row that inherits a file through its base row is not a writer).

| File | Writers | Rows | Outcome |
|---|---|---|---|
| `src/go2cs.slnx` | 22 | 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17, 19, 22, 24, 25, 26, 27, 28, 34, 35, 38, 60 | 1 hunk (row 35), section B |
| the four lists | 22 | the same rows | 1 hunk each (row 35), section B |
| `src/go2cs/go2cs-src.projitems` | 10 | 5, 18, 22, 29, 36, 37, 40, 45, 57, 62 | clean; +11 lines |
| `src/go2cs/testConversion.go` | 11 | 18, 22, 30, 32, 33, 36, 45, 46, 48, 51, 62 | ten content merges, clean; 10154 -> 10478 lines; READ WHOLE |
| the BOARD | 10 | 1, 18, 19, 20, 40, 41, 42, 43, 63, 64 | 7 hunks as seated (rows 19, 20, 40, 42, 43, 63, 64), section A |
| `src/tests/PackageTests/README.md` | 5 | 52, 53, 54, 56, 58 | clean; 96 -> 162 lines (159 without row 56) |
| `src/tests/PackageTests/PackageSymbols/test-package-symbols.ps1` | 4 | 52, 54, 55, 56 (one stacked chain) | clean |
| `src/core/golib/buildTransitive/go.lib.targets` | 2 | 52, 55 (stacked) | clean; 92 -> 122 lines; a PACKED file: read whole |
| `src/go2cs/convCallExpr.go` | 5 | 5, 24, 28, 29, 34 | four content merges, clean; READ WHOLE |
| `src/tests/PackageTests/release-smoke.ps1` | 4 | 53, 54, 56, 58 | clean |
| `.github/workflows/os-matrix.yml` | 3 | 53, 54, 56 | **CONFLICT at row 56 as cut: 2 hunks, non-empty base** (section D2); clean without that row |
| `docs/CIMatrix.md` | 4 | 53, 54, 56, 58 | **CONFLICT at row 56 as cut: 1 hunk, empty base**; clean without that row |
| `src/core/reflect/value_impl.cs` (hand-own) | 3 | 13, 14, 19 | clean; READ WHOLE |
| `src/core/testing/testing.cs` (hand-own) | 3 | 25, 31, 32 (a chain) | clean |
| `src/core/testing/TestOptions.cs` (hand-own) | 2 | 32, 33 | clean |
| `src/tests/Behavioral/BehavioralTests/TestingRuntimeTests.cs` | 3 | 31, 32, 33 | clean |
| `src/tests/Behavioral/BehavioralTests/BehavioralTestBase.cs` | 2 | 49, 50 | clean |
| `src/go2cs/typeNameResolution.go` | 3 | 10, 17, 36 | clean |
| `src/gen/go2cs-gen/Templates/InheritedType/IMapTypeTemplate.cs` | 4 | 6, 7, 8, 9 (a chain) | clean |
| `docs/ConversionStrategies.md` | 4 | 9, 18, 23, 60 | clean |
| `docs/README.md`, `commandLineOptions.go`, `main.go` | 2 each | 18, 23 | clean |
| `docs/ConversionStrategies-Reference/type-aliasing.md` | 2 | 10, 22 | clean since row 10's docs commit moved its paragraph (L:293) |
| `docs/ConversionStrategies-Reference/manual-conversions.md` | 3 | 8, 25, 31 | clean |

49 paths have more than one writer (47 among v0's 60; the fix-up row adds the fixture's two files; `c1-aot-smoke`
adds none: its five files all had a writer, and neither do the two docs rows); 637 paths are touched by the 64
rows; as seated, 161 (row, path) content merges are clean and 14 conflict: the ruled twelve and row 56's two (12
at the 62-row state; 159 and 10 without row 56 there; the first draft's v0 order read 158 and 11). No file under `src/core` outside golib is written by two rows except the three hand-owns
above: the carrier's 74 generated files and g-cs8500-managed-view's one do not overlap.

## I. What this page does not hold

- No tree. The simulation wrote no object, so there is no union sha here: the map union is the assembler's, on the
  frozen table.
- No rename detection (no Q row renames a path: `git diff --name-status` reads only A and M across the 64 rows: 239 A, 595 M).
- No build, no conversion, no test on the i7.
- The rehearsal merged six seats in other positions than the table's (five rows later, the fix-up last: section
  D) and never merged row 56: its trees are not the assembly's trees. Its readings are evidence about the seat SET; the map union and the battery read the ORDER.
- Whether the registration files' entries stay in name order once both sides are kept: the resolution keeps both
  in merge order, which at row 35 places `NamedPointerFieldAddress` and `NamedPointerReflectElem` above
  `NamedPointerEquality`. The lists were not strictly sorted at BASE either (`NamedMapMakeNonNil` sits above
  `NamedArrayPointerConversion` in `go2cs.slnx`); no gate reads the order.
