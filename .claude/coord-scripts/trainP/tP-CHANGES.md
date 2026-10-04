# TRAIN P: every change against TRAIN O's script set (id, where, what, why)

Derived 2026-10-04, 12:16 to 14:15 Central, by a workflow agent (step 3 of the P derive). Source: `trainO/` as frozen
at O's battery launch (never edited: its files' mtimes were read before and after). Nothing here was run for real; what
was measured is in `tP-DERIVE-REPORT.md`. UNCOMMITTED: `git -C hnd status` lists nothing under `trainP/` (`.claude/` is
excluded; a commit needs `add -f`: checklist section 1).

## 0. Inventory

| File | Origin | Lines changed against the mechanical rename (+ / -) |
|---|---|---|
| `tP-assemble.sh` | O, R0 + P1, P2 | +34 / -26 |
| `tP-conflict-map.sh` | O, R0 + P1 | +13 / -10 |
| `tP-fixup.sh` | O, R0 + P1, P7, P9 | +104 / -54 |
| `tP-regen-apply.py` | O, R0 + P10 | +37 / -10 |
| `tP-emitcheck.sh`, `emitdrift.py` | O, R0 + texts (P10's reading note) | +29 / -10, +6 / -2 |
| `tP-battery.sh` | O, R0 + P1, P3, P4, P6, P9, P12, P13 and the leg map | +358 / -168 |
| `tP-helpers.py` | O, R0 + P8 | +32 / -9 |
| `tP-modules-legs.sh` | O, R0 + P1, P11 | +18 / -8 |
| `tP-linux-legs.sh` | O, R0 + P1, P5, P11, P13 | +63 / -19 |
| `tP-i9-shard.sh`, `tP-i9-te.sh` | O, R0 + P1, P6, P7, P12 | +66 / -26, +10 / -8 |
| `tP-ng-parse.ps1` | O, R0 only | 0 |
| `tP-i7-sweeps.txt`, `tP-i9-shard.txt`, `controls/te-*.patch` | O, BYTE COPIES (`cmp` equal) | 0 |
| `tP-follow.txt`, `controls/README.txt` | O, R0 + texts | EMPTY list: entries hash `e3b0c44298fc` |
| `tP-seats-draft.txt` | NEW (D1) | 22 rows, `rows-sha256=8491acbec194` |
| `tP-README.md`, `COORD-LAUNCH-CHECKLIST.md` | REWRITTEN for P on O's structure (D2, D3) | |
| `tP-lane-brief-i9.md`, `tP-lane-brief-linux.md` | REWRITTEN for P on O's structure (D4) | |
| `tP-CHANGES.md`, `tP-DERIVE-REPORT.md` | NEW | |
| `tP-premap*.sh`, `tP-regcheck.py`, `tP-unioncheck.py`, `tP-gofmt-parse.sh`, `tP-footprints.sh`, `tP-hunks.sh`, `tP-premap.md`, `tP-seats-candidates-step1.txt`, `premap-run1/`, `step1-run1/` | steps 1 and 2 of this workflow | NOT EDITED by step 3. **What they change against O's pre-map set: section 2b** (added by review round 1: this inventory gave them no id). Review round 1 edited five of them: section 8 |
| `premap-run3/` | review round 1's run output | `controls/` (the changed instruments' control run, failed=0) and `v8-eb88-rr1/` (the 22-row table read again) |
| `premap-run2/` | this step's run output (v5: 20 rows; v6: 21 rows with the original go.go seat; v7: 21 rows with G's -r2; **v8: the 22-row table of record**; `trial-slots/`: 23 rows, with the unaccepted go-cmp L ref; three ls-remote reads) | |

Not copied from O: `tO-CHANGES.md`, `tO-DERIVE-REPORT.md` (O's records: P's pointers name them as `trainO/...`),
`tO-seats-draft.txt` (P's list is new), O's pre-map files and `premap-run1` (P has its own).

## 1. R0: the mechanical copy

Every copied file: `tO` -> `tP`, `TRAIN O` -> `TRAIN P`, `trainO` -> `trainP`, `TO_` -> `TP_`, `tOemit` -> `tPemit`,
with the pointers to O's own records protected (`trainO/tO-CHANGES.md`, `trainO/tO-DERIVE-REPORT.md`,
`trainO/tO-premap.md`) and every statement ABOUT O (its battery, its rows, its stops) restored by hand where the rename
had turned it into a statement about P (one residue found late and fixed: `emitdrift.py` named p2-converter-warning-clears
"a TRAIN P row"). What R0 gives without a further edit: `W=/h/go2cs-tmp-coord/tP`, `BRANCH=claude/coord-trainP-union`,
the lock and scratch under `coord-scratch/tP`, the emission roots `/h/go2cs-tmp-coord/tPemit*`, the map refs
`refs/coord/tP-map/rows-<hash>`, the subjects `fixup: TRAIN P` / `fixup-N: TRAIN P` / `refresh: TRAIN P`, the log folders
`tP-logs`, `tP-fixup-logs`.

## 2. P's changes

| Id | Where | What | Why |
|---|---|---|---|
| **P1** | `tP-assemble.sh` (BASE, line 63), `tP-conflict-map.sh` (25), `tP-fixup.sh`, `tP-modules-legs.sh`, `tP-linux-legs.sh`, `tP-i9-shard.sh` (BASE), `tP-battery.sh` (MASTER, 140) | The base is **TRAIN O's landed master**, REQUIRED from the environment in every script; no script writes a base sha. The texts, the `${VAR:?...}` messages and the BEHIND / train-commit refusals name O's landing; eb88ab9492 appears only as the stand-in the P seats were cut on and as the 'stale base' example. | O has not landed: its master does not exist. O's own O1 rule (no literal base) is kept; a P seat cut on eb88ab9492 is an ancestor of the real base, so the O2 refusals (a row carrying another train's assembly commits; a base behind a landed train) are what catch a launch on the stand-in. Measured: an eb88-cut row reads 39 train commits over N's master and 0 over eb88ab9492. |
| **P2** | `tP-assemble.sh` header, its 'AT P' note, its literal-site list (LITS) and NEXT line; `tP-conflict-map.sh` header | P's pre-map facts (the two rows that conflicted and their ruled re-cuts, both done; which rows the train-commit check cannot see: those cut on 59ee0d21bf, on an O row or on an O row's parent); LITS names P's literals (`E_BISECT_KNOWN is DERIVED`, `H3_TOKENS (g-debugger-views at P)`, `S1_NONE (ConsumerUsings.csproj added at P)`, `IDC_MIN`, `UF_KNOWN`, `RF_SELF / RF_DECL`); NEXT says H4 reads 0. | The assembler prints its literal-site list at the end of every run: a list that still named O's literals would send COORD to walk sites that no longer exist. |
| **P3** | `tP-battery.sh` 170-174 and every baseline reader | `N_RUN` -> **`O_RUN`** (default `coord-scratch/tO/bat2`), `O_SUMS`, `O_REWRITES`, `O_REWRITES_T` under `tO-logs`; **a launch that sets `N_RUN` is REFUSED** (exit 2, before the lock); the fallback literals are O's readings (CNR 819, FX 1291), stamped when used. | The task's item: the previous-train record is O's battery of record. The refusal exists because a command line copied from O's checklist would otherwise set a variable nothing reads and silently take the default. Unit-run: the refusal fires, rc 2. |
| **P4** | `tP-battery.sh` 193-194, PRE-D (5) at 544-560, leg E's bisect | **`E_BISECT_KNOWN` is DERIVED**: the rows of the frozen list whose notes hold a `CORPUS FOOTPRINT ...(...)` group, intersected with the rows whose merge changes converter source. `E_BISECT_SEATS` at launch overrides (each name validated: a name that is no row is a FINDING); `none` = no arm; a footprint row that is not a converter seat is stamped 'never bisected'. | The task's item (b). At O the default was a typed pair of names and a re-cut under a new ref name silently dropped a seat from the bisect (O's review round 1, F2-5). At P's list the default is five rows (the sort seat, the sibling seat, the float fold, fm-record-r3, the else-if -r3); `p2-test-warning-entries` is the footprint row that is not a converter seat. Unit-run on the real list: default, a launch list, `none`, a stale name. |
| **P5** | `tP-linux-legs.sh`, the LCn name derivation (383-425) | The i7's test-name filter in the OTHER place that derives test names from a diff: LCn's list. Dropped names are stamped; the DERIVED line counts them. | The task's item (a). At O, P1's LCn read `not-PASS=[TestRunning]` as a MOVER (driver exit 4) on a correct union: the name is a line of fixture source inside a raw string. `tP-i9-shard.sh` was read whole: it derives NO test name from a diff (its named readings are literals), so it has no site. `tP-modules-legs.sh`, `tP-fixup.sh`, `tP-emitcheck.sh`: none either. The filter itself is P13's, not O's. |
| **P6** | `tP-battery.sh` `UF_KNOWN` (198), PRE-D (6) (564-572), `ufarm` (820), its two call sites (UF, UF-T); `tP-i9-shard.sh` 320-340 | The `-tests`-only `.editorconfig` class: `src/core/math/bits/.editorconfig` (an i7 row) and `src/core/weak/.editorconfig` (an i9 row) are a KNOWN class. PRE-D reads whether the list seats a row named `p2-test-warning-entries*` and whether the two paths are tracked at HEAD. **With the row: EXPECT 0 untracked**, any untracked path is a finding (the i9: a SOFT line). **Without it: exactly those two paths are stamped KNOWN and anything else is a finding.** A `UF_KNOWN` path that is neither tracked nor ever seen is a `UF_KNOWN` finding in PRE-D only when the row claims to commit it. | The task's UF item. O's arm treated every untracked `src/core` path as a finding, and the i7 and the i9 each raised one on a correct union (a `-tests` run writes the entry file; no `-stdlib` run owns it). COORD's 13:08 line: 'UF arm at P: EXPECT 0 untracked on both lists'. Unit-run: five cases on the battery's arm, four on the i9's. |
| **P7** | `tP-fixup.sh` step 6 (666-679), `tP-i9-shard.sh` end purge (346-358) | O's BATTERY STOP 1 remedy (the purge reads `remaining` again up to 6 times, 5 s apart, before it dies; `retries=` is stamped) carried to the two other purges that die or flag on `remaining > 0`. `tP-battery.sh` already has it (O's own fix, copied). | O's bat1 aborted at a purge that read `remaining=1` on an EMPTY obj folder a build node had just let go, with no test failed. The fixup's step-6 purge and the i9's end-of-shard purge had the same one-shot read. Run against a real Windows file lock: clean after 2 retries when the lock is held 12 s; 6 retries then the die when held 50 s. |
| **P8** | `tP-helpers.py` 110-114, 147-152, labels | `S1_NONE` += `ConsumerUsings.csproj`; `H3_TOKENS = {'g-debugger-views': ['[System.Diagnostics.DebuggerNonUserCode]']}`; the wall and TE labels name P and O (`P=<w>s O(min)=`). | precheck HARD-FAILS on a hand csproj that names no LangVersion and is not listed (the smoke re-cut's fixture), and on a row that edits golib's Goroutine.cs with no identifier (g-debugger-views: +3 lines; the attribute reads 0 at the base and 1 at the seat). Both would stop PRERES on a correct union. The H3 entry is keyed by the ROW NAME: a re-point under another name moves it (README MS1). |
| **P9** | `tP-fixup.sh` `relfix` (370-378, called at PRERES and at step 6: 660); `tP-battery.sh` PRE-D (7) (583-588) | **The release fix is asserted in the tree.** Predicate on `src/core/sort/sort.cs`: 0 lines matching a bare `Sort(x);` and 3 `public static void Sort(this (IntSlice\|Float64Slice\|StringSlice) x)` declarations. The fixup DIES before writing anything, and again on the tree it is about to commit; the battery raises FINDING `RELFIX`, with the reader's control at `$MASTER` (3 bare lines expected there; 0 accepted once a later base carries the fix). | P is the release train BECAUSE of this file. The file is in `REGEN_ALLOW` (the sort seat's footprint), so a seat x seat interaction that made the union converter emit the old self-call would be copied over the fix by step 4 and leg E would read 0. Read with git: 3 / 3 at eb88ab9492 and 54f7f4439d (the defect), 0 / 3 at b719825826 and c49e2b7063 (the fix). Unit-run on three planted worktrees. |
| **P10** | `tP-regen-apply.py` rule 4 (75-100) and its header; `tP-emitcheck.sh`, `emitdrift.py`, `tP-battery.sh` leg E (texts) | Rule 4 ('a flat path is copied only when every target that emits it agrees') now takes 'emits it' from the union arm's written list **OR the base arm's** (`written-M-<os>.txt`); a missing base list is 'evidence incomplete', rc 2. The refusal text says the union tree's bytes were left in place. Leg E's texts say its U arm's `written=` count DROPS (a reading). | `i9-incremental-cs-writes-r2` is a P row: the union converter leaves an unchanged source unwritten, and the emission check converts ONE target per run (main.go routes only two or more `-platforms` targets to the census path, which alone forces writes). So 'written by the union arm' stopped meaning 'emitted', and a flat file ONE target emits differently would have passed rule 4 and been copied over the shared location. Unit-run on synthetic trees: P's reader refuses it; O's reader accepts the same evidence. **Lifetime: holds while the base arm's converter writes every source (O's does). A tQ derive item: Q11.** |
| **P11** | `tP-modules-legs.sh` 167-181, `tP-linux-legs.sh` 435-438; the seat list's grammar line | The NOTES arm that marks M's two host seats wants the phrase `host seat H1` / `host seat H2`, no longer a bare `H1` / `H2`. | C2's go-cmp classes carry those names (COORD's notes: 'H1 accepted', 'go-cmp H2'), and two of them are P rows. A bare token in such a row's notes made that row 'the H2 seat', whose `testConversion.go` diff the control converters are then given: for a go-cmp row that diff is empty, `ctlconv` returns 1, and MR4c / MR6c cannot be built (read from the code, not run). The draft writes the classes H-1 and H-2 as well. |
| **P12** | `tP-battery.sh` `X_ROWS` (163), the S leg's `named` case (1695), `NPOST` (499), the LEG -> SEAT map and every per-leg note; `tP-i9-shard.sh` `named` case (262) | `X_ROWS='encoding/json sort'`: the i7 sweeps `sort` too (it is an i9 row), read by name on both boxes (`TestSortIntSlice`, `TestSortFloat64Slice`, `TestSortStringSlice`). `NPOST` = P's 16 package directories; the predictions are P's (CNR 834, 8 isolated projects, GT classes, walls from O's bat2). | The release fix's package is read on the box of record as well as on the i9, as O read `encoding/json` for its fm-record footprint. It is a REGRESSION reading: sort's own suite sorts through `sort.Sort(...)` and never calls the three methods (stated in the comment), so it cannot prove the fix; `SortMethodSelfCapture`, C1's arm B and P9 do. |
| **P13** | `tP-battery.sh` PRE-D (1) (395-436), `tP-linux-legs.sh` (386-423) | **O's backtick-parity filter is REPLACED by a lexical scan.** `gotests` (POSIX awk, state carried across lines: block comment, raw string; line comments, interpreted strings and rune literals with their escapes end on their line) prints the `func TestX(` declarations that sit in CODE; a derived name is kept only when a test file the union changed declares it. `gotests_ctl` runs the scan's own control first on the box's awk (EXPECT `TestReal1 TestReal2 `): a misread filters NOTHING and is a FINDING `CB_SCAN` on the i7, a MOVER on a lane. | Found in this derive, on P's own list. The parity ('an even count of backticks above the line') misreads every line below a backtick held in an interpreted string, a rune literal or a comment. `crashVerdict_test.go` (i9-tests-host-crash-verdict, accepted 12:37) holds a .NET arity tick inside a quoted stack line: **the parity filter drops 8 of its 9 real tests** from CB and LCn, stamped as 'NOT tests'. Measured on git objects: the seat alone, 15 derived, parity keeps 6, the scan keeps 14 (drops `TestString`, a real fixture line); over all 261 converter test files of O's union, 1113 text matches, the scan keeps 1108, the parity 1100: 13 disagreements read by hand, the scan right in all (5 fixture lines both drop; 8 real tests only the parity drops). At O's union against N's master both give 28 of 29 (`TestRunning`), so O's CB reading stands. **Deviation from the task's wording ('the SAME filter'), stated: the same filter is measurably wrong on P's table.** |

## 2b. The pre-map set against TRAIN O's (steps 1 and 2 of the workflow; recorded here by review round 1)

Section 0 said only 'steps 1 and 2: NOT EDITED here', and 4 (c) gave two sentences. These files are a change against
O's set like any other. Measured after the mechanical rename: `tP-premap.sh` differs from `tO-premap.sh` in 283 diff
lines (156 -> 227 lines), `tP-regcheck.py` from `tO-regcheck.py` in 92 (50 -> 92 lines); six files have no O
counterpart. The controls of all of them are `tP-premap.md` section 0 (run 12:04, `premap-run1/controls/`, failed=0)
and, with review round 1's edits, `premap-run3/controls/` (failed=0).

| File | Against O | What changed, and why |
|---|---|---|
| `tP-premap.sh` | a rewrite of `tO-premap.sh` | **The merge is git's own ort**: `git merge-tree --write-tree --messages <ours> <theirs>` through `GITX` (the Visual Studio git, 2.55), where O ran a temp-index `read-tree -m --aggressive -i <first merge base>` plus `merge-file` per unmerged path on the box's git 2.35. Why: O's instrument used the FIRST base on a criss-cross and read a false README conflict on c2-nuget-followups-r2 (COORD's 03:15 derive item), and `read-tree` has no rename detection (c2-sibling-package-name-r2 carries 6 renames). **Objects go to a scratch directory** (`OUT/objects`, the shared store as an alternate; `OBJDIR=shared` is the old behaviour by explicit call): the first run wrote into the shared store and took it from 5,633 to 7,017 loose objects, past `gc.auto`. **OUT takes either path style** (`cygpath -m` before native git sees it: O's second derive item). **Modes**: `replay` is new (the control: a landed train's merges re-merged from their real parents and compared tree for tree); `alone` writes `onbase.tsv`; `pairs` puts each row on BASE first, so what remains is seat versus seat (O's pairs merged tip against tip where the two footprints shared a path, and repeated every row's own conflict with the base in each pair). **The union carry is unrefined** (`merge-file --union --diff3 --diff-algorithm histogram`): the refined union keeps a line both regions share once, and where that line is a closing brace the carried Go file stops parsing (measured on `importAliasOperations.go`: 502 lines and 15 gofmt errors refined, 504 and 0 unrefined). **Chain commits are made at a fixed date**, so a re-run reproduces the commit shas and not only the trees; `chain.tsv` has six columns and `chain-merged-paths.txt` lists every path ort content-merged. |
| `tP-regcheck.py` | derived from `tO-regcheck.py` | Reads the six-column `chain.tsv`; a row the chain SKIPPED is left out of the expectation and named; a row with two best common ancestors is diffed against ort's merge of the two, not the first (O's Q20 / O3 class); an IDENTICAL INSERT (a `-U0` hunk of the row's own diff that the union side of its step holds identically) is credited once (O's D1 class), stamped on the writer. Why: O's COUNT arm read LOW on a correct union for an identical insert (its header told the reader to read the KEYS before calling a COUNT miss a loss), and a multi-base row's own change was diffed against one of its bases only. |
| `tP-premap-all.sh` | NEW | The whole reading for one list in one command: alone, both chains (union carry and skip), the registration check at both heads over the six registration files and over every other path ort content-merged, the union census, the gofmt parse, the footprints, the pairs; packs the scratch objects; ONE last line and an exit status (0 only when the skip chain kept every row, the registration check of record is ok and the skip head parses). Why: one command and one verdict line per table (P's table moved four times during the derive, and is re-read at the real BASE). |
| `tP-premap-controls.sh` | NEW | The instruments made to pass on a landed train's union and to fail on planted inputs (floor 13): C1 replay, C2 chain, C3 registration check pass / fail, C4 known conflicts, C5 the union census on a planted commit and on head = base, C6 the gofmt arms. Its range and planted-on commit are control inputs, not P's base. |
| `tP-unioncheck.py` | NEW | What a clean merge cannot say, read from two trees: projitems one entry per Go file, behavioral registration of every added project, H4, duplicate top-level declarations of package main, MS19's HashSet scan, conflict markers, `package_info.cs` line kinds with the `stdlib-metadata.txt` pair. A census (exit 0 always), never a gate. |
| `tP-gofmt-parse.sh` | NEW | `gofmt -e -l` over the Go files a head changes under `src/go2cs`, written from git objects to a scratch folder: a parse error is a merge that cannot compile. Why: a union-carried or mis-resolved Go file is invisible to every other pre-map reading. |
| `tP-footprints.sh` | NEW | Each row's OWN footprint against its nearest stack parent or BASE (files, numstat, renames, deletions, best common ancestors), the writers of every shared path and each writer's hunk positions. Why: the table's per-row figures and the 'three regions, two in one function' hazards are read from it. |
| `tP-hunks.sh` | NEW | The conflict regions ort writes for one merge, diff3 style, for a named path: a reader for a human. It writes into the shared store unless the caller exports `GIT_OBJECT_DIRECTORY` (stated in its header; NOT changed by review round 1). |

## 3. O's lessons, and where each is at P

| O's lesson | At P |
|---|---|
| BATTERY STOP 1 (bat1, 04:58): a purge that read `remaining=1` on an empty folder aborted the battery | `tP-battery.sh`: O's retry, copied. **Added: the fixup's step-6 purge and the i9's end purge (P7).** |
| BATTERY STOP 1's CB finding: `TestRunning` is a fixture line, not a test | `tP-battery.sh`: filtered. **Added: the linux driver's LCn (P5), and the filter replaced by a lexical scan with its own control (P13).** |
| O's review round 1, F2-5: a typed E-bisect pair goes stale under a re-cut | **Derived from the list (P4).** |
| O1 / O2: no literal base; refuse a base behind a landed train and a row carrying train commits | Kept; the stand-in eb88ab9492 is the worked example (README MS9c.4, MS9c.5). |
| N's lesson, restated at O: the signature check composed into the push's command cannot stop the push | **Checklist section 4: three commands typed and read by themselves, then the push (the task's item (d)).** |
| O's MS24: a committed `.editorconfig` a conversion deletes | Kept (rule and refusal). None predicted: P's two `.editorconfig` hold `_test.cs` sections only, which a `-stdlib` run neither owns nor removes. |
| O's MS20: a pre-N-template csproj the union CNR moves | Kept; ONE at P (`LiteralFloatConstFold.csproj`), `CSPROJ_TEMPLATE=accept` on the launch line (COORD 11:28). |

## 4. The task's four ADD items

| Item | Status |
|---|---|
| (a) the SAME raw-string filter in every other place that derives test names from a diff | P5 (the one other site: the linux driver's LCn), with P13's replacement of the filter itself. The i9 driver, the module legs, the fixup and the emission check derive no test name from a diff (each read whole). |
| (b) `E_BISECT_KNOWN` derived from the frozen list's footprint rows | P4. |
| (c) `tP-premap.sh` converts OUT to Windows-style and reads a criss-cross row with a real ort merge | **Already done by step 2** (not edited here): `OUT` goes through `cygpath -m` before any native git sees it, and every merge is `git merge-tree --write-tree` (ort, VS git 2.55 through `GITX`) into a scratch object directory. Its C1 control replayed O's 38 merges to identical trees, the criss-cross row c2-nuget-followups-r2 (2 bases) among them (`tP-premap.md`). This step ran it five more times as the table moved (20, 21, 21, 23 and 22 rows): 0 objects written to the shared store. |
| (d) the checklist: the signature check as its own command before the push; a NEW ref pushes then announces with the read-back | D3: checklist sections 1 and 4. An EXISTING ref (`claude/coord-handover`, master) keeps floor 9's order: announce, then push. |

## 5. Documents

| Id | File | What |
|---|---|---|
| **D1** | `tP-seats-draft.txt` | The proposed table in O's exact format (`ref\|sha\|notes`, `#` comments), the pre-map's order v5 plus the i9's crash-verdict row in the pre-map's slot for it G's go.go -r2 in place of the original, and G's x/sync bank docs row as row 4: **22 rows, pushed and accepted rows only (one acceptance read owed: c2-ide-spike): the 21 rows of COORD's 13:35 note in that order + the row its 13:48 note adds**, six `stack-on` tokens, no `after` token, six `CORPUS FOOTPRINT n files (...)` groups (142 path tokens, 133 distinct: **CORRECTED by review round 1. As this step wrote the list, the else-if row's 123 tokens carried a `src/core/` prefix, so the fixup's composer read 142 DISTINCT tokens and admitted 9 of that row's 123 files; '133 distinct' was a count of the paths, not of what the code composes. The prefix is stripped and the figure is now measured with the fixup's own greps: section 8, RR1-1**), no host-seat marker. In-flight work as commented SLOT lines (COORD's P BOARD docs row; C2's go-cmp L, pushed and NOT accepted at 14:00: never seated here). Header: the grammar the scripts parse, what is owed before the map, NOT-P, the battery additions, the assembly hazards. Generated by a script from one input (the 123 else-if paths) so the footprint group is never hand-typed. |
| **D2** | `tP-README.md` | O's structure, rewritten: why P is the release train; the order of work (steps 0 to 8, the release as step 8); the environment table; the predicted fixup; the manual steps (MS21 the acceptance and the slots; **MS23 the release gate: two CI runs, D gating on every shipped RID, darwin FULL, the sort fix's five readings; MS26 the C# consumer runners; MS27 the release after landing: the version bump is the release's own step**); 4b with a release-gate column; the leg table with P's expectations; 17 open questions; what was checked. |
| **D3** | `COORD-LAUNCH-CHECKLIST.md` | O's, re-derived: step-0 asserts on `$BASE` (O's union inside it, O's refresh inside it, nothing above it, the defect's control = 3); the pre-map at BASE; the handover commit; the map; the assembly; the controls; the fixup with `CSPROJ_TEMPLATE=accept`; **the signature as its own command, the NEW-ref push, the read-back, then the announcement**; the GOs; the battery with P's EXPECT lines; **5b the release gate; 6 landing; 7 the release** (the ruled usings step before the PIN). |
| **D4** | `tP-lane-brief-i9.md`, `tP-lane-brief-linux.md` | O's, rewritten for P: the base, the sort row, the UF known class, the purge retry, the i9's two rows, LCn's scan and its control on the lane's awk, the one known P1 mover, LX's first reading of the windows-cut rows, LB's 8 projects, the spin-versus-overflow note. |
| **D5** | `tP-follow.txt`, `controls/README.txt` | Texts only; the follow list is EMPTY. |

## 6. Defects and traps met in this derive

1. **O's parity filter drops real tests** (P13): the carried lesson was itself wrong on P's list. Found only because the
   i9's crash-verdict row was accepted during the derive and its test file was read.
2. **A bare `H1` / `H2` in a row's notes is a host-seat marker** (P11): two P rows would have tripped it.
3. **Rule 4 reads 'written' as 'emitted'** (P10): no longer true once the union converter skips unchanged writes.
4. **The table moved under the derive, four times.** COORD's notes of 12:37, 13:07 and 13:08 (handover 64734a5115,
   read at 13:22) accepted four refs, one of them a row the draft held as a slot; the 13:35 note (one uncommitted line
   in the hnd worktree, read at 13:40) accepted G's go.go re-cut and listed the table. The list was regenerated at
   13:35 (20 -> 21 rows, `80e65788bf4a` -> `ff258261c4a4`), at 13:50 (row 10 re-pointed, `7be5eeae5edf`) and at 14:02
   (the 13:48 note, read at 14:01: G's x/sync bank docs row accepted, 22 rows, `8491acbec194`); the
   pre-map and the table check were re-run each time and every count that moved was re-derived (CB 32 -> 41 names,
   LCn 31 -> 40, projitems +16 -> +18, GT +1 class; CNR 834, 8 projects and the E-bisect default did not move).
   The table check's first run on the final list REFUSED row 10 ('no such branch on the remote'): it was reading a
   remote listing saved before the -r2 was pushed. The real script takes its own `ls-remote` at every run; the stale
   file was the simulation's, and the refusal is the check working.
5. **A purge-retry test needs a real Windows lock**: an MSYS `sleep` with its cwd in the folder does not hold it; the
   test that proved the retry used an exclusive `[IO.File]::Open`.
6. **Tooling**: a multi-KB heredoc is truncated by the Bash tool and nothing runs; `\\` collapses inside a heredoc;
   `grep -P` fails in this box's locale ('supports only unibyte and UTF-8 locales': a seated-tip comparison written with
   it read every row MOVED until it was redone with awk). Every long edit went through a spec file.
7. **The shared object store** prints 'too many unreachable loose objects' on every fetch (README Q14; COORD's 13:07
   flag). The pre-map's three runs wrote 0 objects to it.

## 7. Deliberately NOT changed

- `PUB_MIN` stays 6; `IDC_MIN` 217; the floors 24 / 68 / 26 / 62; `EXEC_RULED` empty.
- The CNR expectation stays DERIVED at HEAD (`cnrexpect`); 834 is a prediction in comments and documents, never a literal
  the gate reads.
- The two sweep lists (93 + 132 rows) are O's bytes: no P row adds or moves a roster row.
- `tP-helpers.py` beyond P8: `cnrexpect`, `csprojtemplate`, `precheck`, `testsrc-refresh`, `realmod`, `live` are O's
  code.
- No release step is in any script: the version bump, the pack, the signing and the push are MS27.
- Nothing under `trainO/`, `trainN/`, `trainL/`; nothing in `/h/go2cs-tmp-coord/tO` or `tN`.

## 8. REVIEW ROUND 1 (2026-10-04, applied 14:35 to 15:20 Central)

Two reviewers' findings on the derive's output (one read the table against git, the remote and COORD's notes; the
other read the scripts against O's set): **19 findings = 1 blocker, 6 should-fix, 12 nits.** The blocker and one
should-fix are ONE defect seen from both sides (RR1-1), and two nits are information with no change owed, so 16
items were applied (RR1-1 to RR1-16). Applied by a workflow agent. Nothing was pushed, posted, built, converted or
tested with `go test`; `/h/go2cs-tmp-coord/tO` and `tN` were not touched; `trainO/`, `trainN/` and `trainL/` were not
edited (trainO was read: `tO-premap.sh`, `tO-regcheck.py`); `trainP/` is still UNCOMMITTED. **No ref and no sha of
the table moved: `rows-sha256=8491acbec194` before and after.**

### 8.1 What changed

| Id | Severity | Where | The defect | The change |
|---|---|---|---|---|
| **RR1-1** | BLOCKER (and a should-fix) | `tP-seats-draft.txt` row 22; `tP-fixup.sh` composition, PRE, the PRE stamp; README MS4, the environment table, section 8; checklist section 3; D1 above | `c2-elseif-position-record-r3` wrote its 123 footprint tokens WITH a `src/core/` prefix; the composer builds `^src/core/(<tokens>)$`, so each was a dead alternative. Run with the fixup's own greps: 142 tokens, **142 distinct** (four documents said 133), **9 of that row's 123 files admitted**, 114 refused. Fails closed: step 4 would have died 'outside the allowed set' after the emission check, and `tP-regen-apply.py` applies nothing when one path is refused. | The prefix is stripped from the 123 tokens by a script (never by hand; the derive's generator was not kept, so the strip asserts 123 tokens, all `package_info.cs`, 123 distinct). **Two controls in `tP-fixup.sh`**: a token that starts `src/core/` or `/` is refused AT LAUNCH, before the LIVE gate and the lock; and in PRE every composed token must be a FILE at HEAD (`git cat-file -e HEAD:src/core/<token>`), else it dies naming row and token. A new PRE line stamps the composition's count. The grammar line of the list states the rule. |
| **RR1-2** | should-fix | `tP-unioncheck.py` C GODECL; `tP-premap-controls.sh` C5 | The duplicate-declaration census still used O's backtick PARITY, which P13 proved wrong and replaced everywhere else. At eb88ab9492 it skipped 161 real top-level declarations of package main (72 in `convCallExpr.go`, 60 in `testConversion.go`: the multi-writer files). | The battery's `gotests` state machine ported line for line (`codelines`); a planted control of its own, stamped `C GODECL scan control: ok` (a miss prints `CONTROL FAILED: the census below is NOT a reading`); C5 plants P13's class (a second declaration below a backtick held in an interpreted string) and asserts it is NAMED. |
| **RR1-3** | should-fix | checklist section 1 | The handover `git add -f .../trainP/*.sh ...` left its globs to the shell: from any cwd but hnd they reach git literally, git's `*` crosses `/`, and the run output under `premap-run*/` and `step1-run1/` is staged and pushed. | `:(glob)` pathspecs (cwd-independent; `*` stops at `/`), a `staged=<n> want=<n>` line derived from the folder (36 at this round), and a count of staged run-output paths that must read 0. |
| **RR1-4** | should-fix | this file | Not complete against its own title: the pre-map set had no id, and D1's '133 distinct' was not what the code composed. | Section 2b (one row per file); D1 corrected in place with the measurement. |
| **RR1-5** | nit | `tP-premap.sh` | The version gate admitted git >= 2.38, but the hunk count and the union carry call `merge-file --diff-algorithm` (2.44's); stderr went to /dev/null and no rc was read, so on an older git the carried blob was EMPTY and the hunk count read 0, silently. | Gate 2.44; `mt()` reads merge-file's rc (128 and above is an error by name) and refuses an empty carry when both sides hold bytes. |
| **RR1-6** | nit | `tP-regcheck.py`, `tP-unioncheck.py` | `GD` was a literal in both; a standalone `tP-regcheck.py` on a two-base row merged into the SHARED store; more than two bases fell back to the first, tagged but not refused. | `GD` from the environment; with no `GIT_OBJECT_DIRECTORY` exported the merge goes to a scratch object directory the script makes and removes (the caller's alternates kept); more than two bases is `REFUSED row ...`, rc 1. |
| **RR1-7** | nit | `tP-gofmt-parse.sh` | Blobs were written by BASENAME while the pathspec crosses directories: two changed files of one name overwrote each other, one never parsed, both counted. | The blob keeps its path under `src/go2cs`. |
| **RR1-8** | nit | `tP-linux-legs.sh`, `tP-i9-shard.sh`; the two lane briefs; README's `BASE` row | The lane drivers accepted ANY commit of the union's first-parent line as BASE, the stand-in eb88ab9492 among them (the i7 scripts refuse it). | Every first-parent commit of `BASE..HEAD` must be a TRAIN P seat merge or `fixup(-N): TRAIN P`; anything else is named and the driver stops. |
| **RR1-9** | nit | `tP-fixup.sh`, `tP-emitcheck.sh`; checklist step 0; README's environment table | `W` was overridable to any path: a `W=/h/go2cs-tmp-coord/tO` left exported from O's shell was refused only at the HEAD check, and `die()` first runs `git -C $W diff HEAD` (it can refresh that worktree's index). | `W` must be `/h/go2cs-tmp-coord/tP` or a rehearsal worktree `/h/go2cs-tmp-coord/tP-<name>`, refused before anything else runs; the checklist's step 0 asks for a fresh shell (an `env` read that EXPECTS no line) and unsets `W`. |
| **RR1-10** | nit | `tP-battery.sh` PRE-D (5); checklist section 5; README Q9 | P4's consequence was unstated: five derived arms are about 3 h 50 m, early in the run; with the 11 to 12 h estimate that passes the default DEADLINE (launch + 14 h): exit 9 before the sweeps finish. O's default was two arms. | Nothing is capped (which arms run is COORD's). PRE-D stamps the arithmetic (`PRE-D E-bisect NOTE`) when the list holds more than two arms; the checklist and Q9 say the choice is made AT LAUNCH (an explicit DEADLINE, or a shorter `E_BISECT_SEATS`). |
| **RR1-11** | should-fix | `tP-seats-draft.txt` header line 4 and the L slot comment; README MS21, Q3, the opening; checklist header and section 0 | The header said 'RE-CUT: none owed' and 'pushed, not accepted' for go-cmp L, while COORD's 14:05 ruling (post cb0a997dcb) is 'ACCEPTED IN SUBSTANCE; ONE RE-CUT OWED'. The file disagreed with its own slot comment. | 'RE-CUT OWED (one, C2): `claude/c2-reflect-newat-field-r2`, stacked on e00629855f, due by the freeze', with the post's EXPECT (one line of `reflect/package_info.cs` differs, `value.cs` unchanged; go test, CNR, ReflectNewAtField, GolibTests' five at the -r2 tip; the remaining canary lines). It stays a slot until pushed, accepted and pre-mapped. **The -r2 ARRIVED during the round (92500eb260, C2's post of 14:58)**: it was read against the EXPECT and trial pre-mapped (8.2's last rows), and is written into the list as a slot comment with its row spelled out as a COMMENT. It is NOT seated: no line of COORD's notes accepts that sha. |
| **RR1-12** | should-fix | `tP-seats-draft.txt` row 10 and the hazards line; README Q6, MS23 | The sibling row's release-visible effect was stated as 'the production namespace of two corpus packages'. The rule is wider: EVERY converted package named differently from its directory keeps the directory segment, hyphenated directories included. That reaches the README walkthrough (release-smoke arm D, four RIDs; both green runs predate the rule), the MOD leg's M2 fixture and first-wave go-humanize. | The row's notes name those three as its union readings; Q6 asks COORD to rule the real fact (third-party modules), and who re-reads go-humanize at the P union. |
| **RR1-13** | nit | `tP-seats-draft.txt` | The L SLOT comment sat between rows 18 and 19 while its ruled position is LAST; row 22's notes ended 'The LAST row'. | The comment sits below row 22 and says what the row carries when it seats; row 22's closing sentence holds before and once L seats. |
| **RR1-14** | nit | `tP-seats-draft.txt` rows 6, 11, 15, 16, 18, the L slot, the hazards line; README MS21 | Four per-file counts added the deletions into the additions, and one position was off. | git numstat at the seated shas: `testing/TestHost.cs` +29/-1, `interfaceConversion.go` +9/-2, `convCallExpr.go` +59/-3 (H-1), `reflect/value_impl.cs` +32/-7 (L, at dec8cee4b4); go.go -r2's `shadowing.md` section is inserted below base line 695 (hunk `-695,0 +696,33`), not 'at line 693'. Row totals did not move. |
| **RR1-15** | nit | `tP-premap.md` | It is the 12:15 v4 map: read alone it still says two rows conflict. | One dated block at the top (the record's convention: amended, never rewritten) pointing at the 22-row table's readings and listing what changed since v4. |
| **RR1-16** | nit | `tP-seats-draft.txt` NOT-P line; README Q18, Q19 | Three refs on the remote, outside O's union and outside P, were not named; `docs/phase4/RESUME-SESSIONS.md` 1f.2 on the handover branch still lists five superseded tips as P's accepted list. | The three refs are on the NOT-P line. The handover record is OUTSIDE `trainP/` and was not edited: Q18 carries it to COORD's next save-state refresh. |

Information, no change owed: the table reviewer's census of what read clean (every seated sha at its remote tip, no
duplicate, no patch-id among O's 102, six stacks declared, footprint groups equal to the real `src/core` diffs); the
script reviewer's rename survivors, each a READ of O's record or a provenance comment.

### 8.2 What was measured (read-only: git objects, the remote by `ls-remote`, scratch copies in the session scratchpad)

| Reading | Result |
|---|---|
| The fixup's composition over the list, BEFORE the strip | 142 tokens, 142 distinct, 123 with the prefix; of the else-if row's 123 real paths (`git diff 07b53bc99d e00629855f -- src/core`) 9 admitted, 114 refused; `src/core/reflect/package_info.cs` REFUSED, `src/core/src/core/fmt/package_info.cs` ADMITTED |
| The same, AFTER | 142 tokens, **133 distinct**, 0 with the prefix; **123 of 123 admitted**; per row 1 / 4 / 1 / 2 / 11 / 123 |
| The at-HEAD control's predicate, on git objects at the pre-map's chain head f9a7b0b11b | the old list: 123 of 142 tokens no file; the new list: 0 of 142 |
| `tP-fixup.sh` itself, from two scratch copies (BASE the stand-in, a head it never reached) | the OLD list: `ABORT: 123 footprint token(s) ... written with a 'src/core/' prefix or rooted (row:token): c2-elseif-position-record-r3:src/core/archive/tar/darwin/package_info.cs ...`, before the LIVE gate; the NEW list: past the composition, stopped by the LIVE gate (`LIVE lock ...tO...`, O's battery alive), nothing created (no `tP/.battery.lock`, no tP worktree) |
| The `W` guard, `tP-fixup.sh` and `tP-emitcheck.sh` | REFUSED: `tO`, `tN`, `tPemitfix`, `tP-reh/../tO`, `tP/`; passed: `tP`, `tP-reh`, unset |
| The lane drivers' predicate on O's real history (its pattern read for TRAIN O) | BASE = N's landed master: empty, 39 of 39 commits are O's own (pass); BASE = N's union 59ee0d21bf (the analogue of eb88ab9492): names `refresh: TRAIN N` and `fixup-2: TRAIN N` (refused); read for TRAIN P on O's union: names O's commits (refused) |
| `tP-unioncheck.py` old against new, at eb88ab9492 and at the chain head | declarations 3455 / 3533 by the parity, **3616 / 3708** by the scan (161 and 175 more); names declared twice 5 / 5; NEW at head 0 in both; every other line of the census identical |
| The same on C5's planted commit | the OLD census names 8 new duplicates and NOT `plantedSetUser`; the NEW census names 9, `DUPLICATE func plantedSetUser: plantedHashSet.go plantedTick.go` among them |
| `tP-premap-controls.sh` with the changed instruments (`premap-run3/controls`, 2 minutes) | 24 control lines, `PREMAP-CONTROLS DONE failed=0`: C1 38 of 38 identical trees (1 criss-cross), C2 head tree 65b092d15c8c, C3 ok x6 / FAIL x6, C4 six known conflicts by path, C5 eleven arms, C6 three |
| `tP-premap-all.sh` on the 22-row list (`premap-run3/v8-eb88-rr1`, 7 minutes) | `ALONE DONE rows=22 not-clean=0`; both chains `conflicted-steps=0`, head f9a7b0b11b, tree e31f01ef9b24; `PAIRS DONE pairs-with-a-both-changed-path=104 conflicting=0`; `PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0`, rc 0. Fifteen logs compared with `premap-run2/v8-eb88`: identical but the census line of RR1-2 |
| `tP-premap.sh` under a git older than 2.44 (the box's 2.35.2 as `GITX`) | `ABORT: GITX is git 2.35.2.windows.1; this script needs >= 2.44 ...`, no OUT folder created |
| merge-file made to fail (a wrapper around the real git: rc 129 on `merge-file`), a row known to conflict in `docs/README.md` | the OLD script: `CONFLICT ... docs/README.md:content(0 hunk(s))`, silently; the NEW script: `ERROR: merge-file rc=129 on docs/README.md: error: unknown option ...` and `ALONE ctl: ERROR`; with the real git: `content(1 hunk(s))` |
| `tP-regcheck.py` standalone, no `GIT_OBJECT_DIRECTORY` exported, O's 38-row chain (one two-base row) | the NOTE line, `MULTI-BASE(the two bases merged)`, ok x6, rc 0; the scratch directory removed; the shared store 7,072 loose objects before and after |
| `tP-gofmt-parse.sh` on a planted commit (two changed `main.go`, the nested one broken) | the OLD reader: 2 files counted, `parse-errors=0`; the NEW reader: `parse-errors=4`, named `internal\plantedpkg\main.go` |
| The checklist's add line, as dry runs (`git add -f -n`, nothing staged) | the old line from another cwd: 217 paths; the `:(glob)` line from another cwd: 36 (15 + 5 + 7 + 5 + 1 + 3) |
| The table's grammar tokens, old list against new (the scripts' own greps) | identical: six stack tokens, no order token, six footprint groups (one per row), no host-seat marker; 22 rows of 3 fields, no CR |
| The remote at 14:37 and at 15:07 (`ls-remote`; the 15:07 listing is `premap-run3/lsremote-end.txt`) | both: all 22 seated shas at their tips; master 54f7f4439d (O has not landed); O's union eb88ab9492; no BOARD row ref. 14:37: no `claude/c2-reflect-newat-field-r2`. **15:07: `claude/c2-reflect-newat-field-r2` 92500eb260** (and the mailbox tip moved to c119c85a2d, C2's post about it) |
| The -r2 against COORD's EXPECT (post cb0a997dcb), read with git | e00629855f is its ancestor; 3 commits (red eca3603bbd, green 18bdcf71cb, corpus 92500eb260), no merge; red and green patch-ids EQUAL the originals' (a617a55044fc, 7853624bf52b); `reflect/value.cs` is the original's blob (f507573cf658); the corpus commit is 1 / 1 in `reflect/package_info.cs`, the `value.go` map line; against dec8cee4b4's file 3 lines differ, the else-if row's own three; the golib, converter, hand-own, behavioral and GolibTests blobs equal the original's; 18 files +621/-24 on e00629855f. MET |
| TRIAL pre-map, the 22 rows + the -r2 as row 23 (`premap-run3/trial-L-r2`, 7 minutes; NOT a seat table) | `ALONE DONE rows=23 not-clean=0`; both chains `conflicted-steps=0`, head a9f04ec55b, tree 81e6b28012c9; step 23 clean (5 content merges: slnx and the four lists); `PAIRS DONE pairs-with-a-both-changed-path=118 conflicting=0`; `PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0`, rc 0; behavioral projects +9, `ReflectNewAtField` registered once in each of the five files, current template; declarations NEW at head 0; markers 0 in 437 files |
| The list WITH the proposed row (a scratch copy) | 23 rows, `rows-sha256=a9b5da13f551`; the composition 144 tokens, 134 distinct, 7 rows; all 144 a file at the trial head; the row's own tokens: one stack token, one footprint group, no order token, 3 fields |
| The shared object store | 7,072 loose objects from the start of the round through every instrument run (nothing was written to it by the pre-map, the controls or the readers); 7,105 once the two fetches by name at 15:08 had run (`claude/c2-reflect-newat-field-r2`, `claude/mailbox`; each as `git -c gc.auto=0 fetch origin <ref>`, so no background pack started) |

`bash -n`: 15 of 15 `.sh`. `compile()`: 5 of 5 `.py`. No `__pycache__`, no CR byte.

### 8.3 Not done, and stated

- **The fixup's at-HEAD control has not run inside a worktree** (there is no tP worktree): its predicate was run on
  git objects at the pre-map's chain head, in both directions. Its launch refusal ran in the real script.
- **`tP-regcheck.py`'s refusal of a row with more than two bases is read from the code, not run**: no such row exists
  in P's table or in O's history.
- **The lane drivers' new refusal has not run on a lane box**; its predicate was run on O's real history here.
- **The derive's list generator was not used, and is guarded.** D1's 'generated by a script from one input' is
  `mk_seats.py` with `elseif-paths.txt` (123 paths written WITH the prefix: the defect's source), both in the workflow
  session's scratchpad, not in this folder. This round edited the list IN PLACE with a script of its own (assertions:
  123 tokens, all `package_info.cs`, 123 distinct, the row hash unchanged). A re-run of the generator would bring the
  prefix back (the fixup now refuses it at launch) and silently drop this round's notes, so its second line now exits
  with a SUPERSEDED message (the unguarded copy is kept beside it). From here the list is COORD's file, edited in place.
- **A tooling trap of this box**: `grep $'\r'` is not a CR detector under this Git Bash (it read 0 on a CRLF line and,
  in another form, listed every file). The 'no CR byte' line above is byte-wise (`tr -cd '\r' | wc -c` per file).
- **`tP-hunks.sh` still writes into the shared store** when the caller exports no `GIT_OBJECT_DIRECTORY` (its header
  says so): a hand reader, not changed.
- **The E-bisect default is not capped.** Five arms (six with L -r2) and the default deadline do not fit: a launch
  decision (README Q9).
- **`docs/phase4/RESUME-SESSIONS.md` 1f.2** is stale on P's list and outside this folder (README Q18).
- **The shared-store remedy** (a prune, or `gc.auto 0`) is still COORD's call after O lands and before P's assembly
  (README Q14). A fetch run as `git -c gc.auto=0 fetch origin <ref>` does not start the background pack: this round's
  two fetches (the -r2 and the mailbox ref, to read what had arrived) were run that way and printed no gc line.
- **The go-cmp L -r2 is not seated.** It arrived at 14:58, reads as expected and pre-maps clean as row 23; seating it
  is COORD's acceptance at that sha. Its runtime and net/http canary lines are still owed by C2.
- The derive's line counts in section 0's inventory ('lines changed against the mechanical rename') predate this
  round. This round's own deltas, against the files as the derive left them: `tP-fixup.sh` +39 / -2,
  `tP-unioncheck.py` +55 / -13, `tP-regcheck.py` +27 / -8, `tP-premap.sh` +14 / -5, `tP-premap-controls.sh` +14 / -5,
  `tP-linux-legs.sh` +8, `tP-emitcheck.sh` +7, `tP-battery.sh` +6, `tP-i9-shard.sh` +6, `tP-gofmt-parse.sh` +6 / -2;
  the list 15 of its 31 lines (notes and comments).
