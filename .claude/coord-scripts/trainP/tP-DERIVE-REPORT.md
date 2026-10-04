# TRAIN P derive report (step 3: the scripts, the seat table, the release-train documents)

2026-10-04, 12:16 to 14:15 Central, the i7, a workflow agent. Everything is under
`/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainP/`, UNCOMMITTED. Nothing was pushed, posted, built, converted or
tested; `/h/go2cs-tmp-coord/tO` and `tN` were not touched; `trainO/`, `trainN/` and `trainL/` were not edited (trainO's
newest file is 06:36, before the derive; the one modified file `git -C hnd status` shows, `trainL/tL-seats-draft.txt`,
is COORD's notes file, committed by COORD at 14:05 as handover 6cd9cf5041; `trainP/` is not in that commit).

## 1. Verdict

**The P script set is derived and the 22-row table pre-maps clean on the stand-in base.** It cannot be launched yet,
for reasons outside this folder:

1. **O has not landed** (origin/master is still 54f7f4439d at 14:00; O's bat2 was in its S sweep at 13:55, 352 SUMMARY
   lines, 0 findings). P's base is a REQUIRED variable in every script; no script carries it.
2. **One acceptance read is owed**: `c2-ide-spike` a4bfed39f5 (docs only; no 'P ACCEPTED' line names it).
3. **Two slots are open**: COORD's P BOARD docs row (no ref) and C2's go-cmp L, `claude/c2-reflect-newat-field`
   dec8cee4b4 (pushed, not seated here). **It conflicts with the last row as cut** (section 3.3); RULED 14:05 a
   re-cut -r2 stacked on the else-if -r3, placed last, leaving 'by a revert on top' if the battery reds on it: the
   scripts are not ready for that shape (README Q16).
4. **Nothing here has run for real**: no precheck, fixup, emission check or battery on any union (none exists).

## 2. What was produced

| | |
|---|---|
| Scripts (15 `.sh`, 5 `.py`, 1 `.ps1`) | O's set renamed (R0) + P1 to P13: `tP-CHANGES.md` has each (id, where, what, why) |
| `tP-seats-draft.txt` | 22 rows, `rows-sha256=8491acbec194`: the 21 rows of 'P TABLE AS IT STANDS' in COORD's 13:35 note, in that order, + `g-xsync-bank` (accepted 13:48, 'a docs row near the top') as row 4; six `stack-on` tokens, no `after` token, six footprint groups (142 path tokens, 133 distinct); the slots, NOT-P, the hazards as comments |
| `tP-README.md` | the release-train summary: order of work, environment, the predicted fixup, manual steps MS1 to MS27, the leg table with P's expectations, 17 open questions |
| `COORD-LAUNCH-CHECKLIST.md` | from the seat table to master AND the release (sections 0 to 7) |
| `tP-lane-brief-i9.md`, `tP-lane-brief-linux.md` | the lanes' briefs for P |
| `tP-CHANGES.md`, this file | the record |
| `premap-run2/` | this step's pre-map runs and ls-remote reads |

## 3. What was measured (all read-only: git objects, or synthetic inputs in the scratchpad)

### 3.1 The table (the assembler's own table-check block, run at the stand-in base eb88ab9492 with a fresh ls-remote)

```
TABLE row 3 r-csharp-consumer-smoke-r2: stacked on row 2 r-csharp-consumer-guide (declared; ancestor verified)
TABLE row 7 c1-release-smoke-sort-arm: stacked on row 6 g-sort-self-capture (declared; ancestor verified)
TABLE row 9 p2-untyped-region-nil-safety-r2: stacked on row 8 p2-cli-version-diagnostics-r2 (declared; ancestor verified)
TABLE row 11 g-go-namespace-shadow-r2: stacked on row 10 c2-sibling-package-name-r2 (declared; ancestor verified)
TABLE row 15 i9-tests-host-crash-verdict: stacked on row 14 i9-incremental-cs-writes-r2 (declared; ancestor verified)
TABLE row 22 c2-elseif-position-record-r3: stacked on row 21 g-method-value-fm-record-r3 (declared; ancestor verified)
TABLE OK: 22 rows (tP-seats-draft.txt) rows-sha256=8491acbec194, no duplicate ref or sha, no row already on the base, every declared stack-on verified, no undeclared stack, every REMOTE tip at its seated sha (or acknowledged), no patch under two shas (the seats' and master's)
```
484 patch-id lines. All 22 remote tips equal their seated shas at 14:00 (`premap-run2/lsremote-end.txt`). An unplanned
control: run against a remote listing saved before G's -r2 was pushed, the same block REFUSED that row ('no such
branch on the remote').

### 3.2 The pre-map on the table (the step-2 instrument, `tP-premap-all.sh`; real ort merges in a scratch object directory)

| Run | Table | Last line |
|---|---|---|
| `premap-run2/v5-eb88` (13:08) | 20 rows (the i9's crash-verdict row still a slot) | `conflicted-rows=1 regcheck-of-record=ok skip-head-parse-errors=0`, rc 1: the original go.go seat |
| `premap-run2/v6-eb88` (13:26) | 21 rows, the original go.go seat c5cb51d3b6 | `conflicted-rows=1 ...`, rc 1: the same seat (three conflicting pairs, all with it) |
| `premap-run2/v7-eb88` (13:42) | 21 rows, G's -r2 05b61c616a | `conflicted-rows=0 ...`, rc 0 (one tree, c7a1b7270c25) |
| **`premap-run2/v8-eb88` (14:01), OF RECORD** | **22 rows: v7's + g-xsync-bank as row 4** | **`PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0 union-head=f9a7b0b11b skip-head=f9a7b0b11b`, rc 0** |

v8 in full: `ALONE DONE rows=22 not-clean=0`; both chains `conflicted-steps=0`, ONE tree (e31f01ef9b24);
`PAIRS DONE pairs-with-a-both-changed-path=104 conflicting=0`; 702 objects to the scratch directory, 0 to the shared
store. Its censuses at the chain head: registration files at expect (slnx 1111 -> 1126, 8 writers; projitems 492 ->
510, 12 writers; the four lists +24 each; `shadowing.md` 733 -> 809, 3 writers; `testConversion.go` 4 writers);
behavioral projects +8, H4 0; package-main names declared twice: 0 new; unqualified HashSet uses 0; conflict markers 0
in 425 changed files; `package_info.cs` changed 127 (GoPositionMap 816, using 4, namespace 4); gofmt parse errors 0 of
46 changed Go files. v8's tree differs from v7's in the bank row's 7 docs paths and nothing else (read with git).

### 3.3 The unaccepted go-cmp L ref, read in its slot (`premap-run2/trial-slots/`, 23 rows = the table of record + L; NOT a seat table)

`PREMAP-ALL DONE conflicted-rows=1 ...`, rc 1. (Run at 13:48, when the bank row was unaccepted too: its 23 rows are
exactly the 22 of record plus L.)
- `g-xsync-bank` 51f5b68770 at row 4: clean, 0 content merges (accepted at 13:48 and seated since).
- `c2-reflect-newat-field` dec8cee4b4 at the pre-map's row-19 slot: clean there (5 content merges: slnx and the four
  lists), and then **STEP 23 `c2-elseif-position-record-r3`: CONFLICT in `src/core/reflect/package_info.cs` (1 hunk)**;
  the pair reading agrees (the one conflicting pair of 114). Both rows re-encode the single `reflect/value.go`
  position-map line. As cut it cannot ride; a re-cut stacked on e00629855f, placed last, is the route (README MS21.4).
  A first grep of mine read 'not among the else-if row's paths' (an anchored pattern against a list whose paths start
  `src/core/`): the trial corrected it, and no document carries the wrong reading.

### 3.4 Derived readings at the table's chain head (f9a7b0b11b, read through the scratch object directory)

| Reading | Value | Where the scripts derive it |
|---|---|---|
| CB names (battery block, as written) | 36 derived, 35 kept, dropped `[TestString]`; + 6 guards = **41** | PRE-D (1) |
| LCn names (linux block, as written) | 36, 35 kept, dropped `[TestString]`; + 5 = **40** | the linux driver |
| GolibTests classes | added `DebuggerViewsTests`, `HostPackageTimeoutRunningTestsTests`; changed `NoUncountedBackingAllocationsTests` | PRE-D (2) |
| Behavioral directories holding a `.go` | 826 -> 841: +16, -1; 7 platform-exclusive, none added: **CNR 834** | `cnrexpect` |
| csproj added | 19 (15 behavioral, 2 under docs/, `CSharpConsumer.csproj`, `ConsumerUsings.csproj`) + 1 renamed (R099: the sibling seat's sub-package) | S1, csprojtemplate |
| Line counts (`wc -l`) | projitems 491 -> 509; slnx 1110 -> 1125; CompileTests.cs 2385 -> 2409; importAliasOperations.go 473 -> 504 | precheck COUNT |
| The release fix (P9) | 0 bare `Sort(x);`, 3 methods (base: 3 / 3) | PRE-D (7), `relfix` |
| H3 token (P8) | 1 in Goroutine.cs (base: 0) | precheck H3 |
| Rows that edit golib's Goroutine.cs (each row's own diff) | `g-debugger-views` only: the one `H3_TOKENS` entry | precheck H3 |
| Added csproj outside docs/ that name no LangVersion | `ConsumerUsings.csproj` only: the one new `S1_NONE` entry | precheck S1 |
| E-bisect default (P4) | g-sort-self-capture, c2-sibling-package-name-r2, c2-literal-float-fold, g-method-value-fm-record-r3, c2-elseif-position-record-r3 | PRE-D (5) |

### 3.5 Unit runs of the changed arms

| Arm | Cases | Result |
|---|---|---|
| P13 `gotests` control | the planted 11-line file | `TestReal1 TestReal2 ` |
| P13 on real files | crashVerdict_test.go at the seat; incrementalWrites_test.go; warningEntries_test.go | 9 of 9 kept; `TestString` dropped; `TestRunning` dropped |
| P13 against O's parity, every converter test file of O's union | 261 files, 1113 text matches | scan keeps 1108, parity 1100; 13 disagreements read by hand, the scan right in all 13 |
| P13 / P5, the two blocks as written | O's union vs N's master; the i9 seat alone; the control forced off | 29 -> 28 (`TestRunning`: O's bat2 line); 15 -> 14 where the parity keeps 6; the finding / the mover fire and nothing is filtered |
| P4 E-bisect | default, a launch list, `none`, a stale name | 5 seats; the list; no arm; a finding |
| P3 | `N_RUN` set | refused, rc 2 |
| P6 `ufarm` (battery) and the i9's UF block | 5 and 4 cases (row seated or not; known, other, both, none) | as designed |
| P7 purge retry | a real exclusive Windows lock held 12 s, then 50 s | clean after 2 retries; 6 retries then the die |
| P9 `relfix` / `rfx` | sort.cs at eb88ab9492, 54f7f4439d, b719825826, c49e2b7063; a worktree without the file | 3/3, 3/3, 0/3, 0/3; dies on the defect and on the missing file, passes on the fix |
| P10 rule 4 | synthetic trees: a flat file one target emits differently | refused (O's reader accepts it); rc 2 on a missing base list |

`bash -n`: 15 of 15 `.sh`. `compile()`: 5 of 5 `.py`. No `__pycache__`, no CR byte, no apostrophe inside a
`${VAR:?...}` message. The two sweep lists and the two control patches are `cmp`-equal to O's. The follow list's
entries hash is `e3b0c44298fc` (EMPTY).

## 4. The task's items

| Item | Status |
|---|---|
| Copy `trainO/*` renamed, record every change | DONE (`tP-CHANGES.md` R0, P1 to P13, D1 to D5) |
| Names and paths; `N_RUN` -> the previous train's battery of record; BASE / MASTER REQUIRED, no literal base | DONE (R0, P1, P3) |
| `tP-seats-draft.txt`: O's format, the pre-map's order, pushed rows only, tokens, footprint notes, slots as comments | DONE (D1); it tracked COORD's notes to 13:35 |
| Carry O's purge retry and CB filter | DONE; the filter was found wrong on P's list and REPLACED (P13) |
| (a) the filter in every other place that derives test names from a diff | DONE (P5: the linux driver's LCn is the one other site) |
| (b) `E_BISECT_KNOWN` derived from the footprint rows | DONE (P4) |
| (c) the pre-map converts OUT to Windows-style and merges with real ort | ALREADY DONE by step 2; recorded; exercised three more times here |
| (d) the checklist: the signature as its own command before the push; a NEW ref pushes, is read back, then is announced | DONE (checklist sections 1 and 4) |
| RELEASE TRAIN specifics in the README and the checklist | DONE (README MS23, MS26, MS27, 4b; checklist 5b, 6, 7) |
| UF: the `-tests`-only `.editorconfig` class | DONE (P6) |
| `PUB_MIN` stays 6; the CNR expectation derived at HEAD | UNCHANGED, as asked |
| `bash -n` every `.sh`, compile every `.py` | DONE |
| Do not commit | NOT COMMITTED |

## 5. Open items (the README's section 7 has the wording for COORD's rulings)

**Before the map**
1. O lands; then `BASE` from ls-remote, the step-0 asserts, the pre-map at BASE (EXPECT the v8 line), MS9c's five
   controls, the map.
2. The acceptance read for `c2-ide-spike`.
3. The slots: the BOARD docs row; C2's go-cmp L as its -r2 (RULED 14:05: stacked on the else-if -r3, placed last).
   Before L is seated: README Q16 (a revert on top must be a `fixup-N` commit, and precheck still reads a reverted
   row as a row).
4. The shared object store: 7,072 loose objects at 13:55 (past `gc.auto`); the remedy before the assembly (Q14).

**Before the launch**
5. The handover commit of `trainP/` (a dry run first: `premap-run*/` and `step1-run1/` hold run output and scratch
   object directories; the checklist names the files instead of the folder).
6. The controls that have never run on a P union: precheck (MS9b, with the H3 entry's own negative control), the LIVE
   gate, `csprojtemplate`'s verdict, the map refusal.
7. Rulings: P4's five-arm default (Q9), P9's die (Q10), P11's marker phrase (Q12), P12's X row (Q13), P13's scan
   (Q15), MS26's first runner (Q4), the sibling seat's namespace move as release-visible (Q6), REGEN_ALLOW's grammar
   against that seat's csproj and -tests files (Q8).

**At the union**
8. The three import / namespace rows have not been read together (Q2); `c2-named-basic-conv`'s footprint has never
   been measured by a reconvert (the fixup's step 4 is the first); `g-debugger-views` was measured on N's generator.
9. MS23: C1's two dispatches at the P union, and again for a fixup-N on a release path. The sort fix's five readings.
   Owed by the bank row once the union is pushed (COORD 13:48): G's x/sync re-read (28/28) and a docs-site build at
   the union (pages under `docs/validation/modules`, an `@` in the path).
10. MS13 after the battery; MS26; then MS27 with the ruled usings step before the PIN.

**For the next train**
11. P10's lifetime: once BOTH arms of the emission check carry the i9's incremental writes, `written-M` stops
    meaning 'emits' and rule 4 needs a converter that writes unconditionally for a single-target run (Q11).
12. The i9's queued findings (the `.cs.auto` sibling marker restore; `runtimeSourcesDigest`'s order dependence); P2's
    arm-A extension over `*.tests.csproj`; the thread dump at the package deadline; G's three sizings ruled 13:51 for
    the train after P (the CS0122 alias-publicize seat, cuttable now on eb88ab9492; the CS0246 local-lift seat, cut on
    P's union: `convCallExpr.go` has three P writers; the host-argv seat F1).
13. `tP-regcheck.py` and the pre-map are step 2's: a tQ derive should carry `premap-run*` OUT of the script folder.

## 6. Not done, and why

- **No real run of any script**: there is no P union, O's battery owns the box, and the rules of this step forbid
  builds, conversions and `go test`.
- **The pre-map at the real base**: it does not exist. Every pre-map reading here is on eb88ab9492, which lacks O's
  possible fixup-2, its bank step and its MS13 refresh. The refresh commits re-emitted -tests sources, and P's sibling
  seat commits 24 -tests files, so a both-changed path is possible. Read here: the i9's half of O's MS13 input
  (`coord-scratch/tO/i9-patches/tO-tracked-changes.patch`, 21 files) shares NO path with the 418 paths P's rows
  change. The i7's half (O's S and T rewrites) is not complete while its battery runs: UNREAD. Step 0's pre-map at
  BASE is the reading.
- **MR4c / MR6c under P11**: the marker fix is read from the code, not run.
- **The lane drivers on a lane box**: P13's scan has its own control for that (mawk or gawk); it has run only under
  this box's gawk 5.0.
- **The go-cmp L ref was not seated**: it is not accepted, and it conflicts as cut. The trial list is not a seat
  table and says so in its first line.

## 7. AMENDMENT 2026-10-04, review round 1 (dated; sections 1 to 6 are the derive's report and are left as written)

Two reviewers read this step's output once the derive closed (their remote read is stamped 14:12; the fixes were
applied 14:35 to 15:20). Their findings and what was done are `tP-CHANGES.md` section 8. What they correct IN THIS REPORT:

1. **Section 2 and section 3.1, '142 path tokens, 133 distinct': not what the composer yielded from the list as
   written.** The else-if row's 123 footprint tokens carried a `src/core/` prefix. Run with the fixup's own two greps,
   the list read 142 tokens, **142 distinct**, and the composed pattern admitted **9** of that row's 123 files (the
   nine fm-record-r3 also lists); 114 were refused. '133 distinct' counted the paths, not what the code composes. It
   failed closed (step 4 would have died 'outside the allowed set' on a path the documents call admitted). The prefix
   is stripped; the figure is true now and is measured: 142 tokens, 133 distinct, 123 of 123 admitted, every token a
   file at the chain head. Section 3.3's last sentence had met the same prefix from the other side ('an anchored
   pattern against a list whose paths start `src/core/`') and did not draw the consequence.
2. **Section 1 item 3 and section 5 item 3, go-cmp L 'pushed, not seated': the ruling is wider.** COORD's 14:05 post
   (cb0a997dcb) ACCEPTED L IN SUBSTANCE with ONE RE-CUT OWED by C2 (`claude/c2-reflect-newat-field-r2`, stacked on
   e00629855f, due by the freeze). The table's header named 'RE-CUT: none owed' while its own slot comment carried the
   ruling; both now say the same thing. **The -r2 arrived during the round**: 92500eb260 (C2's post of 14:58), three
   commits on e00629855f. Read with git it meets the post's EXPECT, and a trial pre-map with it as row 23
   (`premap-run3/trial-L-r2`) reads `conflicted-rows=0`, head a9f04ec55b, tree 81e6b28012c9. It is NOT seated: its
   acceptance at that sha is COORD's, and its runtime and net/http canary lines are still owed.
3. **Section 3.4 / 3.5, the pre-map's duplicate-declaration census ('package-main names declared twice: 0 new')** was
   read by a backtick PARITY, the filter P13 replaced in the battery and the linux driver and left in
   `tP-unioncheck.py`. It skipped 161 real declarations at eb88ab9492. Re-read with the lexical scan: base 3616, head
   3708, NEW at head = 0. The reading stands; the instrument was blind in the multi-writer files.
4. **Section 6, 'no real run of any script'** still holds for the fixup, the emission check and the battery. Review
   round 1 ran: `tP-premap-controls.sh` (24 control lines, failed=0) and `tP-premap-all.sh` on the 22-row list
   (`premap-run3/`: the same head f9a7b0b11b and tree e31f01ef9b24, every log identical to `premap-run2/v8-eb88` but
   the census line of item 3), and `tP-fixup.sh` from scratch copies up to its launch refusal (the old list) and up
   to the LIVE gate (the new list: O's battery is alive, so it stopped there, as it must).
5. Per-file counts corrected in the table and the README (git numstat): `testing/TestHost.cs` +29/-1,
   `interfaceConversion.go` +9/-2, `convCallExpr.go` +59/-3 for H-1, L's `reflect/value_impl.cs` +32/-7; go.go -r2's
   `shadowing.md` section is inserted below base line 695, not 'at line 693'. No row total moved.
