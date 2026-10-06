# TRAIN Q: every change against TRAIN P's script set (id, where, what, why, what it answers)

Derived 2026-10-06 by a workflow agent (step 2 of the Q derive: the scripts; step 1 wrote `tQ-seats-draft.txt` and
`tQ-map.md`). Source: `trainP/` as committed on the handover branch (never edited: its 36 files hash the same before
and after, section 7) and the run-time fragments COORD wrote during P's battery and landing under
`coord-scratch/tP/`. Nothing was built, converted or tested with `go test`; what was RUN is in
`tQ-DERIVE-REPORT.md` section 3 (controls on planted input, on kept records and on git objects). UNCOMMITTED.

Notes lines are cited as **L:n** = `trainL/tL-seats-draft.txt` line n (COORD's running notes).

## 0. Inventory

| File | Origin | Q's change ids |
|---|---|---|
| `tQ-assemble.sh` | P, R0 | Q1, Q1b, Q20 |
| `tQ-conflict-map.sh` | P, R0 | Q1, Q20 |
| `tQ-resolve.py`, `tQ-ruled.txt`, `tQ-resolve-controls.sh` | NEW (the mechanism is TRAIN K's) | Q20 |
| `tQ-battery.sh` | P's **run2** copy in substance (the hnd file + the run-time fragment), R0 | Q1, Q2, Q3, Q4, Q5, Q6, Q7, Q11, Q12, Q13, Q14, Q15, Q16 |
| `tQ-helpers.py` | P, R0 | Q4, Q9, Q11, Q13, Q15, Q18 |
| `tQ-modules-legs.sh` | P, R0 | Q1, Q4, Q5, Q7, Q15, Q16 |
| `tQ-fixup.sh` | P, R0 | Q1, Q11, Q18 (through the helper) |
| `tQ-emitcheck.sh`, `emitdrift.py`, `tQ-regen-apply.py` | P, R0 | Q1, Q10 |
| `tQ-linux-legs.sh` | P, R0 | Q1, Q4, Q7, Q12, Q15 |
| `tQ-i9-shard.sh`, `tQ-i9-te.sh` | P, R0 | Q1, Q7, Q12, Q15, Q18 (texts) |
| `tQ-reread.sh`, `tQ-land-prep.sh`, `tQ-land-final-reads.sh` | NEW, from COORD's P fragments (`xsync-reread.sh`, `xmod-reread.sh`, `tP-land-prep.sh`, `land-final-reads.sh`) | Q4, Q5, Q6, Q7, Q8, Q18 |
| `tQ-controls.sh`, `tQ-selfcheck.sh` | NEW | Q3, Q4, Q5, Q6, Q9, Q10, Q11, Q18, Q20 (controls) |
| `tQ-follow.txt` | P, R0 + texts | EMPTY list: entries hash `e3b0c44298fc` |
| `tQ-i7-sweeps.txt`, `tQ-i9-shard.txt`, `tQ-ng-parse.ps1` | P, BYTE COPIES (`cmp` equal; the .ps1 by R0 only) | none |
| `tQ-premap.sh`, `tQ-premap-all.sh`, `tQ-premap-controls.sh`, `tQ-regcheck.py`, `tQ-unioncheck.py`, `tQ-gofmt-parse.sh`, `tQ-footprints.sh`, `tQ-hunks.sh` | P, R0 ONLY | NOT ADAPTED: section 6 |
| `tQ-seats-draft.txt`, `tQ-map.md` | step 1 of this derive | not edited here (edited at REVIEW ROUND 1: the last section) |
| `tQ-seats-gen.sh`, `tQ-seats-v0-coord.txt` | COORD's own (its generator, byte-equal to the scratch copy, and its 62-row list), placed here at 15:48 and TRACKED on the handover branch since 8094d8c0a7 | none: never edited by the derive or by a review round (added to this inventory at review round 1) |
| `tQ-README.md`, `COORD-LAUNCH-CHECKLIST.md`, `tQ-lane-brief-i9.md`, `tQ-lane-brief-linux.md` | REWRITTEN for Q | Q21 |
| `tQ-CHANGES.md`, `tQ-DERIVE-REPORT.md` | NEW | |

Not carried from `trainP/`: `tP-CHANGES.md`, `tP-DERIVE-REPORT.md`, `tP-premap.md`, `tP-seats-draft.txt`,
`tP-seats-candidates-step1.txt` (P's records: Q's pointers name them as `trainP/...`), and `controls/` (two patches,
byte copies since M: `tQ-controls.sh` and the checklist read them at `../trainP/controls/`, or at `TE_CONTROLS=`).

## 1. R0 and the names (task item A)

Every copied file: `tP-` -> `tQ-`, `trainP` -> `trainQ`, `TRAIN P` -> `TRAIN Q`, `/tP` -> `/tQ`, `tPemit` ->
`tQemit`, `TP_` -> `TQ_`. That gives, with no further edit: `W=/h/go2cs-tmp-coord/tQ`,
`BRANCH=claude/coord-trainQ-union`, the lock and scratch under `coord-scratch/tQ`, the emission roots
`/h/go2cs-tmp-coord/tQemit*`, the map refs `refs/coord/tQ-map/rows-<hash>`, the subjects `fixup: TRAIN Q` /
`fixup-N: TRAIN Q` / `refresh: TRAIN Q`, the log folders `tQ-logs`, `tQ-fixup-logs`. `BASE` / `MASTER` stay REQUIRED
variables: no script that READS A UNION holds the base as a literal. (Worded as measured at review round 1; the
first wording, 'no script holds a base sha', was not true of the folder. The sha 7098b8d3f9 is in two scripts,
neither a union reader: `tQ-seats-gen.sh` line 7, COORD's own generator, and `tQ-controls.sh`'s `TE_BASE` default,
the commit the carrier was CUT on, one of the two fixed objects of control TE-carrier. It is also in the checklist,
as the base 'at the derive'; master has moved twice since.)
`SEATS_EXPECTED` is read from the frozen table (the count of row lines), never typed.

The rename then turned every statement ABOUT P into one about Q. Each was restored by hand or by rule: the headers
were rewritten (every script's first block), 20 pointers to P's own records were re-pointed
(`trainQ/tQ-CHANGES.md P<n>` / `RR1-<n>` -> `trainP/tP-CHANGES.md`, `tQ-premap.md` -> `trainP/tP-premap.md`), and the
code lines that named TRAIN O as 'the previous train' now name TRAIN P (Q1).

**Survivors of the previous train's names** (`tQ-selfcheck.sh` S7 counts them per file), each justified:

| Kind | Where | Why it stays |
|---|---|---|
| `coord-scratch/tP/run2`, `tP-logs/...`, `P_RUN`, `P_SUMS`, `P_REWRITES`, `P_REWRITES_T` | `tQ-battery.sh`, `tQ-controls.sh` | the previous battery of record (item A): the FX floor, the CNR control, the TE baselines, the HOSTWALL baselines, TL-WALL |
| `coord-scratch/tP/i9-patches-run2/tP2-tracked-changes-U0.patch` | `tQ-battery.sh`, `tQ-i9-te.sh` | the i9's TE baseline: its own P shard patch |
| `trainP/tP-CHANGES.md`, `trainP/tP-premap.md`, `trainP/tP-DERIVE-REPORT.md`, `trainP/tP-<script>` in a DERIVED line | every script header, the P-era notes | pointers to P's own records and provenance |
| `../trainP/controls` | `tQ-controls.sh`, the checklist | M's two TE control patches, byte copies, not duplicated |
| `TRAIN P's landed master line`, `the landed master line of TRAIN P` | every `BASE` / `MASTER` message | what Q's base IS |
| `P:` / `at P` / `P (P<n>)` comment notes | every script | a train adds its own note and keeps the earlier ones as provenance (O's and N's notes are kept the same way) |
| `/tP` in `tQ-seats-draft.txt` | step 1's header | 'the previous-train baselines are P's run2' |

No `tPemit` and no `coord-trainP-union` survives anywhere (read with grep over the folder).

## 2. Q's changes

| Id | Where (file: function or section) | What | Why, and what it answers |
|---|---|---|---|
| **Q1** | every script's `BASE` / `MASTER` check and message; `tQ-battery.sh` previous-train block; `tQ-helpers.py` labels | The base is **TRAIN P's landed master line** (P's landing 40a1f839c5, then the docs batch, the 1.24.13.4 release record, the known-issues entry). The previous-train record is **P's run2**: `P_RUN` (default `coord-scratch/tP/run2`), `P_SUMS`, `P_REWRITES`, `P_REWRITES_T` under `tP-logs`; a launch that sets `O_RUN` or `N_RUN` is REFUSED. Fallback literals are P's readings (CNR 835, FX 1291), stamped when used. `HOSTWALL ... P=`, `WALLCMP Q= P(min)=`. | Item A. run1 is not the record: it read the red fixup-2 closed (L:306, L:319). Read from run2's SUMMARY: `4 asserts: N=835`, `tracked fixtures 1291`, roster 1994 checks, IDC 217, GN 93, TR 26, PUB 6 of 6. |
| **Q1b** | `tQ-assemble.sh` (after the ls-remote) | `BASE` must be **origin/master's tip** at the assembly: a difference was a NOTE at P and is an ABORT at Q (`BASE_NOT_TIP_OK=1` by COORD's explicit call). | Master is EXPECTED to move before the freeze (L:364, L:367: a KnownIssues entry, the census tool). A base below the tip that is still above P's landing passes every other check: the commits above it carry no train subject, so the BEHIND check cannot see them, and the battery's `MASTER..HEAD` derivations would then read the release record's 584 files as the union's. Found in this derive. |
| **Q2** | `tQ-battery.sh` header, LEG -> SEAT MAP, `X_ROWS`, `NPOST`, `UF_KNOWN`, PRE-D (6), `ufarm`, PRE-D (7); `tQ-fixup.sh` `relfix`; `tQ-i9-shard.sh` UF | The leg map restated for Q's 61 rows. `X_ROWS='encoding/json testing encoding/gob'` (sort leaves: a PROPOSAL, section 9 Q-A). `NPOST` = the 28 package directories Q's rows add (read with git; CNR PREDICTED 835 + 28 = 863, the rehearsal's count). UF has NO known class: P's two `.editorconfig` are tracked in the base; PRE-D (6) reads that they still are. The sort-fix predicate (P9) is kept as a REGRESSION reading (control at the base EXPECTS 0). | The table (step 1), and the map's counts. P's release fix and P's UF class landed with P. Rows by area, read from each row's own diff: 30 change a non-test Go source of the converter, 8 change go2cs-gen, 4 change golib C# sources, 1 edits the roster (no count, no annotation: measured). |
| **Q3** | `tQ-battery.sh` `cbknown` (between the markers `# >>> cbknown` / `# <<< cbknown`), `CB_KNOWN_SKIPS`; leg CB's gate line | P's run-time fragment folded in: a derived test that SELF-SKIPS is KNOWN only when BOTH its name and its own skip text (the line above its `--- SKIP`) are listed; subtracted from the gate, never from the stamp. Default EMPTY at Q. Control: `tQ-controls.sh` arm CB (P's five arms). | Lesson 1 (L:271, L:319; `coord-scratch/tP/cb-known.frag.sh` + `.control.sh`). P's entry names a test that landed with P, so CB does not run it at Q; no test a Q row adds skips on an environment reason (measured: 44 added names, 2 added `t.Skip` lines, both under `testing.Short()`, and CB runs without `-short`). |
| **Q4** | `tQ-helpers.py` `EXTERNAL_RX`, `go_side_text`, `external_go`, `realmod`; `tQ-modules-legs.sh` `realread`, END; `tQ-battery.sh` MOD stamp, `MODOWED`, END, **EXIT 7**; `tQ-linux-legs.sh` `lmread`; `tQ-reread.sh`; `tQ-land-final-reads.sh` step 2 | **MOD KNOWN-EXTERNAL.** A package that would read FAIL reads `EXTERNAL` when EVERY undisclosed differing name is a Go-side failure AND the oracle's own output for that test (the `go test:` entry of the record's `errors`) holds a network or quota text. It is named with the quoted text, counted apart (`external=`), written to `mod-logs/OWED-rereads.txt`, repeated on the battery's END, and the battery exits **7** (not 0, not 6) when that is all that stands; the landing's final reads REFUSE a head whose owed re-read is missing or not CLEAN. A C#-side failure with the same text stays FAIL (not ruled). | Lesson 2 (L:335, L:344). P's run2 ended EXIT 6 on this row alone. Control on P's REAL records: P's reader reads `sumdb/tlog: FAIL`, Q's reads `EXTERNAL ... TestCertificateTransparency go=fail cs=pass [ResourceExhausted ... quota exhausted]`, `pass=8 fail=0 external=1`. |
| **Q5** | `tQ-battery.sh` `tcn`, `gitalive` (PRE, and after each of the three `export MSYS_NO_PATHCONV=1`), END line; `tQ-modules-legs.sh` (after its export); `tQ-reread.sh`, `tQ-land-prep.sh`, `tQ-land-final-reads.sh` (`tcn`, a refusal when `MSYS_NO_PATHCONV` is set, the one native command gets it on its own line); `tQ-selfcheck.sh` S1; `tQ-controls.sh` arm TC | No `MSYS_NO_PATHCONV` around `git -C` anywhere, and every 'tracked changes' reading can FAIL: `tcn` prints the count or `UNREAD`. | Lesson 3 (L:344). The battery and the module legs never used `git -C` (read: 0 sites); the vacuous stamp was in COORD's landing fragment, which Q replaces. Control TC: clean 0, one planted change 1, no tree UNREAD, a Git Bash path with path conversion off UNREAD (the old `git -C | wc -l` form reads **0** there). |
| **Q6** | `tQ-battery.sh` PRE (the pwsh start probe), leg PPS7; `tQ-land-prep.sh` (PRE probe, `GOPIN_PATH` built without any `/dotnet10` entry); `tQ-selfcheck.sh` S3 | pwsh is started the battery's way everywhere (`env -u DOTNET_ROOT PATH="$GOPIN_PATH" "$PWSH"`), and whether it STARTS is read once in PRE. | Lesson 4 (L:344: 'the pwsh arm first failed to START with dotnet10 first on PATH'). Every pwsh site of P's set already had the form (read: 9 sites); the failure was in the landing fragment and in a launching shell that carried the SDK folder first, which the probe now names in the first minute. |
| **Q7** | `tQ-battery.sh` leg **T2** (`T2_ROW`, default `runtime/debug`), before the bank legs; `tQ-modules-legs.sh` **XS2**; `tQ-i9-shard.sh` **S2** (`TWOPASS_ROW`, default `unicode/utf16`); `tQ-linux-legs.sh` `twopass` (both lanes, `TLEG_KEEP`) and `LM:xsync2`; `tQ-reread.sh` xsync run 1b | Every -tests gate reads one row TWICE into one tree: the unchanged second run must leave the same verdict and the same file names beside the published host, with more than one `.pdb`. | Lesson 5 (L:313, L:315 (1), L:344; P's red, L:306 to L:314). P's battery saw the symbol loss by accident (two runtime legs into one tree). The linux `tleg` restores a row after every run, which would make the second run a changed one: the first pass keeps its rewrites. A two-pass proof is meaningful only at ONE head (L:314), which a frozen worktree is. |
| **Q8** | `tQ-land-final-reads.sh` step 1 | The converter suite (`go test ./...`, not under `GOWORK=off`) is read AT THE REFRESH HEAD, the commit that lands; EXPECT rc 0 and 0 tracked changes after. | Lesson 6 (L:344, the audit's should-fix; `coord-scratch/tP/land-final-reads.sh`). At Q the refresh is large by construction (Q18), so the suite's tree guards meet the refreshed sources for the first time there. `GOWORK=off` is the module legs' alone (L:315). |
| **Q9** | `tQ-helpers.py` `MAPLINE` | `^\[assembly: (global::)?(go\.)?GoPositionMap\(`. | Lesson 7 (L:262, L:344). Read at the base: 3526 lines in the `go.` spelling, 413 in the `global::go.` one; P's refresh message was corrected by hand (165 map lines). |
| **Q10** | `emitdrift.py` (the walk); `tQ-emitcheck.sh` header, the marker scan; `tQ-regen-apply.py` `emitters`, rule 4, the evidence block | **No always-write flag.** Drift is read from BYTES: every file under the emitted root's `src/core` against its seed, whatever its time; the written-by-time list is a reading (and the marker scan reads it AND the drift list). Rule 4 no longer reads `written-M-<os>.txt`: who EMITS a path comes from (a) `emitted-U-<os>.txt` when given, (b) the path's GOOS folder, (c) for a flat path every kept root that holds it, a disagreeing root REFUSED by name. | Lesson 8 (L:257 Q11, L:274: the i9's ruling). P's rule 4 held 'while the base arm's converter writes every source'; Q's base is P's converter, which writes incrementally. PARTLY OPEN: the converter at the base writes no output list (read with git), so (a) has no provider yet; section 9 Q-C. Controls ED and RG. |
| **Q11** | `tQ-helpers.py` `hostwall` (`--prev-summary`, `--leg`, ` SLOWER`); `tQ-battery.sh` leg S, END `HOSTWALL` line; `tQ-fixup.sh` **step 4t** (`TESTS_ARM_ROWS`) | (a) Every S row's HOSTWALL has a baseline: P's own stamp for that leg, read from P's SUMMARY (98 stamps); a host at least 1.5 times slower AND 30 s slower is listed at END, never gated. (b) The fixup's derive has a -tests arm: three rows are converted (`-test-action convert`), what moved in their committed test sources is classed, the N-PARTIAL line count is compared with a prediction read from the tree, and the rows are put back; nothing is committed. | Lesson 9 (L:262, the audit's carry items: 'HOSTWALL baseline for S rows', 'a -tests arm for the fixup's derive', and os/exec 144 s -> 234 s unexplained). Predictions at the base: net/rpc 6 (CORRECTED at review round 1 from 14: the pathspec's `*` crossed into net/rpc/jsonrpc), log/slog 3, encoding/json 1; the corpus 738 lines in 173 files. Control HW. |
| **Q12** | `tQ-battery.sh` `recclean`, `freshrec`, PRE-D (9), `tleg`; `tQ-i9-shard.sh` `recpre`, the rerun marker; `tQ-linux-legs.sh` `tleg` | The pre-clear of a row's record files is DROPPED when the union's converter always writes the comparison record (the predicate is the converter's own source line at HEAD, never a row name); a record is fresh when it is newer than its leg. The T leg's 'left NO record' test became 'left no FRESH record'; the i9's infra rerun gets a marker of its own. | Task item C (i9-comparison-record-always-written, L:331). Found: three pre-clears (the battery's `recclean` with three callers, the i9 driver's two `rm -f`, the linux `tleg`'s one). The deletion also hid what a second run into one tree leaves (Q7). |
| **Q13** | `tQ-helpers.py` `treport` (`address-pairs=`, `unpaired-address-names=`, `UNPAIRED-ADDRESS`), `realmod` facts, `ADDRSHAPE`; `tQ-reread.sh` `records`; `tQ-battery.sh` PRE-D (10) | Wherever a record's totals are read, its `addressPairs` are read too. `N:N > 0` = rows matched by RUN ORDER, never by name: stated, not a red at a banked count. An unpaired address-shaped name (one side only) = the pairing refused a group whose sizes differ: the row is NOT validated on it. | Task item C (i9-address-variant-pairing-on-runtests, L:301, L:346, L:362). The record carries `oneToOne` and `nToN` and no 'unpaired' field: the unpaired names are the one-sided ones left in the verdict maps, counted here. |
| **Q14** | `tQ-battery.sh` PRE-D (8) (`CNR-FRESH`, `TRH_CLASSES`), leg **TRH** | Stated which legs could read a stale result (none on the i7: section 4); a FINDING if the union changes `check-no-regression.ps1` or `_paths.ps1` (floor 10); the harness guards the union adds (`CompileSkipTests`, `TranspileMemoTests`) read by name in one process, since no other leg of this box runs the MSTest behavioral tier. | Task item C (i9-transpile-memo, i9-mstest-compile-skip-closure; L:315 (4)). Read with git: of the 61 rows only one touches the runner (`Program.cs`, comments), none the CNR script. |
| **Q15** | `tQ-battery.sh` `pubsym`, the `PUBSYM` gate after the T legs, legs **PPS51 / PPS7**; `tQ-modules-legs.sh`, `tQ-i9-shard.sh`, `tQ-linux-legs.sh` (PUBSYM counts); `tQ-helpers.py` `S1_NONE`, `S1_PLAIN` | 0 lines of 'the published test host lacks N dependency symbol file(s)' across every -tests log of a run; the pack path guard's self-test under both PowerShell editions; precheck's S1 admits i9-pack-symbols' three hand-written csproj BY NAME (a ruling amendment for COORD to confirm). `PUB_MIN` stays 6. | Task item C (i9-tests-host-symbol-check, i9-pack-symbols and its two ancestors; L:347, L:352 to L:355). No Q row touches `check-published-output.ps1`. As P's helper stood, precheck HARD-FAILS on those three csproj at the union (the table's QUESTION 5): section 9 Q-B. |
| **Q16** | `tQ-modules-legs.sh` `tagset`, `MOD_TAGS_TESTS`, `MOD_TAGS_PROG`, the ten oracle lines, `MR3-tags`; `tQ-battery.sh` PRE-D (10) | Every Go oracle of the module legs is built with the tag set the union's converter gives the same conversion, derived from the converter's source at HEAD (`purego,math_big_pure_go,safe` for a -tests -recurse run, `safe` for a plain -recurse run); MODULE.md's own sentence is compared with it. PREDICTED: no count of these legs moves with the tag. | Task item C (g-module-safe-tag, L:260, L:261, L:277). Measured: 0 files of P's kept x/sync (13 Go files) and x/mod (39) copies name safe / purego / math_big_pure_go / appengine in a constraint; the six fixtures hold none. Control: `tagset` on the base (module set empty) and on the seat (`safe`). |
| **Q17** | no script change | The -tests hosts are launched with Go's argv (`-test.*`), the result paths ride the environment, RunTests exists, a detached T has Go's zero-value semantics. No leg of this set launches a host by hand or parses its argv (read: the only `-test.run` sites start the repoguard TEST BINARY, a Go binary); every leg reads the driver's records. | Task item C (g-host-go-argv, g-testing-runtests, g-detached-testing-t; L:289: 'read the driver's comparison record, not the log's mismatch line'). `X_ROWS` gains `testing` (Q2); leg TR's floor is derived from the class file. |
| **Q18** | `tQ-helpers.py` `te_class` (class `N-PARTIAL`), `teattr`, `hunkclass`, `ts_class`, `testsrc_refresh`; `tQ-land-prep.sh` step 4e; `tQ-fixup.sh` step 4t; `tQ-i9-te.sh` text | The carrier renders a no-inline method as a partial method: the removed line holds the attribute prefix, the added line is the same text with the prefix gone and one ` partial ` inserted. That pair is a class of its own, counted by every reader that classed hunks (TE, the fixup's golden step, the landing refresh), never OTHER. | Task item C (c2-noinline-partial, g-r2m-caller-closure-v2; L:354, L:366). The only code that reads `MethodImplOptions.NoInlining` in converted text is this classifier. Control on the carrier's own corpus diff: 78 files, 195 hunks, 195 N-PARTIAL, 0 OTHER. The seat regenerated no committed test source: 180 hold 938 prefix lines at the base, 738 on a method declaration in 173 files, so every sweep rewrites them and the landing refresh commits them: PREDICTED, not discovered. |
| **Q19** | `COORD-LAUNCH-CHECKLIST.md` section 5b | What C1's hosted gate prints at Q: arms A to D on four RIDs (D gating; arm B now 5 lines, the fifth `BuildTag = safe` on both sides); arms E (two lines) and F `MEASURED, NOT GATING` (F: `RUN \| PUBLISH 1 \| PUBLISH 2 \| FDD \| OFF \| AOT`); the pack's `Pack paths: CLEAN` line; with the CAND trim row, F's AOT line must RUN. (REVIEW ROUND 1: no longer so. With c1-aot-smoke seated arm F has NO AOT part and the AOT run is the stage aot-smoke, non-gating at Q: checklist 5b as rewritten.) | Task item C (C1's rows: c1-release-smoke-published, -package-symbols, -safe-tag, c1-golib-trim-default, c1-release-nuget-preflight). Read from each row's diff. |
| **Q20** | `tQ-resolve.py`; `tQ-ruled.txt`; `tQ-assemble.sh` (table check, the merge loop, the stack assert, the END block, `RULEDF` / `RULSHA`); `tQ-conflict-map.sh` (the conflict branch, the END block); `tQ-resolve-controls.sh` | Section 5. | Task item D; the rehearsal (L:359, L:368), the table's item (4). |
| **Q21** | the four documents | Section 6. | Task item E. |

## 3. The recorded lessons, each with its disposition (task item B)

Collected from `trainP/tP-CHANGES.md` (its sections 6, 7, 8.3 and P10's lifetime note), from L:257 to L:368, and from
the fragments in `coord-scratch/tP/`. **done** = in the scripts; **n/a** = not a script matter, with the reason;
**open** = left for COORD (section 9).

| # | Lesson (source) | Disposition |
|---|---|---|
| 1 | CB: a self-skip with an environment reason is not a fail (L:271, L:319) | done, Q3 |
| 2 | MOD KNOWN-EXTERNAL for a Go-side network or quota failure (L:335, L:344) | done, Q4 |
| 3 | No MSYS_NO_PATHCONV around git -C; a tracked-changes stamp must be able to fail (L:344) | done, Q5 |
| 4 | pwsh starts without the .NET 10 folder first on PATH (L:344) | done, Q6 |
| 5 | Every -tests gate reads a row twice into one tree (L:313, L:315, L:344) | done, Q7 |
| 6 | The converter suite at the refresh head (L:344) | done, Q8 |
| 7 | The MAPLINE regex (L:262, L:344) | done, Q9 |
| 8 | The emission check compares bytes; no always-write flag; the converter's own output list (L:257, L:274; P10's lifetime, `tP-CHANGES.md` section 2) | done for drift (Q10); **open** for the output list (Q-C) |
| 9 | HOSTWALL baseline for S rows; a -tests arm for the fixup's derive (L:262) | done, Q11 |
| 10 | The MOD KNOWN list should class a test that fails only its own sub-2-second window, with a control run (L:287: semaphore TestWeightedAcquire, singleflight TestPanicDo) | done for the control run and the OWED line (XS2, Q7 / Q4); **open** for the subtraction: the first run's FAIL stays counted (Q-D) |
| 11 | Read the driver's comparison record, not the log's mismatch line (L:289) | done: every reader reads records (Q13, Q17) |
| 12 | A two-pass proof is meaningful only at ONE head (L:314) | done: stated at T2 and in both briefs |
| 13 | `go test ./...` is red under GOWORK=off (L:315) | done: only the module legs and `tQ-reread.sh` set it; `tQ-land-final-reads.sh` unsets it |
| 14 | The comparison record is written only when its bytes change while readers take its write time (L:315 (5), L:331) | done by the seat; Q12 drops the workaround |
| 15 | MSTest CompileCSProject reuses an exe after a golib change (L:315 (4)) | done by the seat; Q14 reads its guard |
| 16 | The publish profiles lose symbols on a second unchanged publish of a user program (L:315 (2), L:347) | done by the seat (i9-pack-symbols); C1's arms E and F read it hosted (Q19) |
| 17 | The guard pins the lever, not the outcome (L:315 (1)) | done by the seat (the symbol check) and by PUBSYM (Q15) |
| 18 | publishedTestHostPath derives the host name from the csproj file name; they differ over 93 characters (L:315 (3)) | **open**: no Q row, no leg; a real-module item (Q-E) |
| 19 | gate-forensics' `find -newermt` derivation reads 0 on a warm build (L:315 (4)) | n/a here (a skill's text); **open** for the doctrine batch |
| 20 | No retry on a locked published host (L:315 (7)) | n/a: 'a named refusal is the tell'; PUBSYM and the leg's own rc would show it |
| 21 | Two seats adding a paragraph at one doc anchor conflict: read every pair before the freeze (L:293) | done by step 1 (`tQ-map.md`, every pair read from objects) and by the rehearsal |
| 22 | A break is anchored to a line and its site count printed (L:304, L:313) | n/a to the scripts (doctrine batch G); applied to this derive's own edits (every edit printed its site count) |
| 23 | After a rejected call that posts or pushes, read the target before reporting or re-sending (L:310) | done: checklist sections 4 and 6 |
| 24 | A generator seat that adds a public member to every wrapper runs the FULL behavioral compile before it is posted (L:326); a seat changing a hand-owned package runs that package's own test project (L:340) | n/a: acceptance rules, COORD's; the battery reads both (2b + leg 5; TR + the `testing` row) |
| 25 | A frozen-source breach in a lane worktree (L:342) | done: both briefs restate floor 4 |
| 26 | The mailbox anchor is the last tip READ, re-listed after every rejected push (L:282, L:305) | n/a to the scripts; checklist section 4 says it |
| 27 | The sampler test sits on its floor on a 4-core box (L:284) | n/a to the scripts; the linux brief names it as the KNOWN mover class with its control; **open** for P2's sizing |
| 28 | os/exec host wall 144 s -> 234 s unexplained; converter hash stamps differ by build path (L:262) | done as a reading (Q11's SLOWER list; P's run2 read 129 s); the hash stamp is n/a (a build-path property: compare converters by head) |
| 29 | P1's legs stopped on its own disk floor twice; P2's launcher hit a task time limit inside the runtime row (L:273, L:328) | done: the linux brief (free space before launch; launch detached) |
| 30 | P1's x/mod zip hit its 2 m package timeout with TestVCS in flight on a loaded box (L:328) | done as a brief line (a hand re-read alone is the control); not a class in the reader |
| 31 | The refresh message's map / other split was wrong (L:344) | done, Q9; step 4e of `tQ-land-prep.sh` prints the class census the message quotes |
| 32 | The landing cross-check of three sort files against G's ref (L:272) | n/a at Q (P-only); removed from the landing script |
| 33 | `run-csharp-consumer.ps1` gates the landing (L:257 Q4, L:344) | done: checklist section 6 |
| 34 | The release shell needs the environment block; a Phase 0 SDK gate (L:349) | done by the seat (c1-release-nuget-preflight); checklist section 7 |
| 35 | The census tool lands on a branch first; every lane runs its self-test (L:367) | n/a to the scripts; checklist step 0 (it moves BASE; IDC's floor stays 217) |
| 36 | No owner name on a pushed surface (L:357) | done: `tQ-selfcheck.sh` S4 (two comments that spelled a module path with an account name were reworded) |
| 37 | The E-bisect default is not capped (`tP-CHANGES.md` RR1-10, 8.3) | kept as P's; at Q the derived default is 2 arms (the two footprint rows are both converter seats), inside the note's threshold |
| 38 | `tP-hunks.sh` writes into the shared store without `GIT_OBJECT_DIRECTORY` (`tP-CHANGES.md` 8.3) | unchanged (carried by R0): section 6 |
| 39 | The shared-store prune after P lands (L:257 Q14) | **open**: COORD's, before Q's assembly (Q-F) |
| 40 | gofmt drift of 60 lines in testConversion.go at master (L:362) | n/a: housekeeping, no seat's; the parse reader reads errors, not formatting |
| 41 | A row that leaves by a revert rides as a signed fixup-N; relaunch with PRECHECK_MODE=warn (L:257 Q16) | kept: `tQ-README.md` 4b |

## 4. What Q's seats change in the instruments (task item C)

| Seat | Read from its diff | In the instruments |
|---|---|---|
| i9-comparison-record-always-written 631100d823 | `writeComparisonRecord` writes on every run (`os.WriteFile`); the other JSON writers still skip identical bytes | Q12: three pre-clears found and dropped, conditionally |
| i9-address-variant-pairing-on-runtests 1f6e822670 | the record gains `addressPairs` `{oneToOne, nToN}`, absent when nothing was paired | Q13 |
| i9-transpile-memo 1804f36748, i9-mstest-compile-skip-closure 61cebb3ad5 | both change `BehavioralTestBase.cs` (the MSTest tier): an in-memory once-per-process memo; an executable reused only when newer than every fixture `.cs` AND than the newest file of `src/core` / `src/gen` and the build files in `src` | **Which legs could read a stale result:** none on the i7 (leg 4 is the CNR script, leg 5 and every `B:` leg the runner; neither is that tier, and every purge removes `bin` / `obj`). On a lane: an MSTest behavioral reading (the i9's pre-seat kind) when the tip moves in one worktree, because the skip cannot see a DELETED shared input and the memo dies only with the process: both briefs say 'one process a reading; purge fixture bin/obj when the tip moves'. Q14 adds the CNR-FRESH finding and leg TRH |
| i9-tests-host-symbol-check a6ba14579a | a -tests publish returns an error naming the missing symbol files | Q15 (PUBSYM), Q7 |
| i9-pack-symbols c0c876fb27 (+ a395442f7b, 23450a2a6d) | `src/Directory.Build.targets` (new, every project under src), `go.lib.symbols.targets`, the pack maps paths and ships `.pdb`, a path guard ends the pack, three hand csproj | Q15 (PPS legs, S1); `PUB_MIN` unchanged; the PackageTests runners need a feed: C1's stage reads them (Q19) |
| g-module-safe-tag f6c182c23f | `defaultModuleBuildTags = {"safe"}`; `resolveBuildTags` adds it under -recurse; MODULE.md states the set | Q16 |
| g-host-go-argv, g-testing-runtests, g-detached-testing-t | the host's argv is Go's; results paths ride the environment | Q17: nothing parses host argv; `X_ROWS` + `testing` |
| c2-noinline-partial e7fcff2244, g-r2m-caller-closure-v2 633b045e8a | `partial` where the attribute prefix stood; the using line stays; lambdas, inits and bodyless declarations keep the attribute | Q18 |
| C1's rows | arms E and F measured-not-gating; arm B's fifth line; the trim default; the Phase 0 SDK gate; `published_version` | Q19 |
| c2-roster-ceiling-scope 426f4ea780 | the guard's check count moves (1994 -> 2092 at the seat) | the legs REPORT the count and gate rc and the execution-config count only: a text in the checklist |
| c2-orphaned-disclosures-linux f378bd2251 | two rows' sentences; counts and linux annotations unmoved (measured) | precheck's ROSTER arms read ok: predicted |

## 5. The assembly (task item D)

**How TRAIN P resolved registration-list and BOARD conflicts: it did not.** P's assembler aborted on ANY conflict
(`UNRULED conflict`, exit 3) and its map kept a union only when every row was clean. P's two pre-map conflicts were
closed by stacked re-cuts before the freeze; its one BOARD writer was cut on the base; its registration lists merged
clean, and what read them was precheck's COUNT / REG / BOTH arms (still run on every merge at Q). The last train that
resolved by rule was **K** (`trainK/tK-resolve.py` + `tK-assemble.sh`): a resolver keyed by the seat, an expected
conflict set, a refusal on anything else, `Resolved by rule` in the merge message; its kind K9 was exactly this
class, an adjacent insert kept on both sides. **Q reuses K's mechanism**, generalised from three hard-coded seats to
a table and from a text search over the worktree file to the index's three stages:

- `tQ-ruled.txt`: one line per (row, file): `ref|sha10|path|append-both|hunks=1|ruling`. Ten lines: the BOARD at five
  rows, and for g-named-pointer-equality `go2cs.slnx` and the four lists. A ruling names the row's SHA: a re-cut does
  not inherit it. The sixth BOARD hunk the table's order creates (g-board-cs8500-census-limit, the table's QUESTION
  1) is a COMMENTED line: as seated the map reads that row NOT CLEAN and the assembly aborts there, naming the hunk.
- `tQ-resolve.py`: for EVERY unmerged path of the stopped merge, in two phases (nothing is written unless all pass):
  ruled for this row at this sha; three index stages; `git merge-file -p --diff3` with 23-character markers on the
  three blobs; every hunk an EMPTY base and two non-empty sides; resolved = ours then theirs; then the asserts:
  lines(result) = ours + theirs - base, the ruled hunk count, the BOARD's guard its final line, a list's
  `[TestMethod]` count equal to its `Check` method count, no project or method registered twice. The result goes in
  through `hash-object -w`, `update-index --cacheinfo` and `checkout-index`, so the checkout's line-ending pins apply.
  The diff3 form is required: `--union` and the default style drop the lines the two sides share (step 1 measured
  it; control C8 reproduces it: 795 `[TestMethod]` lines for 796 methods).
- `tQ-conflict-map.sh` and `tQ-assemble.sh` call the SAME resolver with the SAME file; the map stamps the file's hash
  beside its rows snapshot and the assembly refuses another file before it creates anything; the assembled tree must
  equal the map's (P's check, unchanged).
- A stacked row asserts its base row's own merge is ALREADY on the union's first-parent line when it merges; an
  `after` row likewise. A SLOT row (a sha field that is not a sha) is refused by name; `#   SLOT` comments are
  counted. A CAND row is refused until its notes carry `READING IN`.

## 6. Documents (task item E), and the files carried without adaptation

| File | What |
|---|---|
| `COORD-LAUNCH-CHECKLIST.md` | Rewritten for Q: step 0 on `$BASE`; the controls (`tQ-controls.sh`, `tQ-selfcheck.sh`, the table's); the map with its RULED lines; the assembly; the fixup with step 4t; the signature as its own command, the new-ref push, the read-back, the announcement; the lane GOs with the tip COORD names; the battery's EXPECT lines; 5b C1's gate as it prints now; 6 the landing (prep, the signed refresh, the final reads at that head, the consumer runner, announce then push); 7 the question for the owner. |
| `tQ-lane-brief-linux.md`, `tQ-lane-brief-i9.md` | Rewritten for Q: the readings each lane owes at the tip COORD names, the two-pass arms, the record freshness rule, the N-PARTIAL class in the rewrites, the KNOWN classes of each box, what to post. |
| `tQ-README.md` | REWRITTEN SHORT: P's 618-line manual is not carried under Q's name (a rename would state P's facts as Q's). It keeps every anchor the scripts cite (steps 0 to 8, MS1 to MS27, 4b, sections 5 to 8) and says for each what holds at Q, pointing to `trainP/tP-README.md` for procedure text that did not change. |
| `tQ-premap*.sh`, `tQ-regcheck.py`, `tQ-unioncheck.py`, `tQ-gofmt-parse.sh`, `tQ-footprints.sh`, `tQ-hunks.sh` | **R0 ONLY.** They need a git of 2.44 or later through `GITX` (this box's git is 2.35.2) and they know no ruled resolution: on Q's table they read six conflicted rows as conflicts. They are reading tools for a moved BASE; the map of record is `tQ-conflict-map.sh`. Their comments still describe P's pre-map. |

## 7. Defects and traps met in this derive

1. **An apostrophe inside `${VAR:?message}`.** `BASE_IN=${BASE:?set BASE to TRAIN P's landed master line ...}` opens a
   quoted string; `tQ-conflict-map.sh` failed `bash -n`, and `tQ-assemble.sh` PASSED it by luck (a later apostrophe
   closed the string). Both fixed; `tQ-selfcheck.sh` S2 reads every script for it, with a plant.
2. **The Bash tool collapses doubled backslashes in a heredoc** (the task's warning, met twice): a `'\\n'` became a
   newline inside a `tr` argument and a `'\\r'` became a CR byte in two scripts. Both repaired from a spec file; the
   folder holds 0 CR bytes (S5). Every edit that carries a backslash went through a written file.
3. **A stale base that every check passes** (Q1b).
4. **The self-check read a real module path as an account name** (S4): two carried comments were reworded. P's pushed
   copies hold the same text.
5. **`git status | grep -vc` reads 0 when git itself fails**: the form is still used inside the battery for its
   mid-run restores (about twenty sites, P's code). Q guards the state that made it vacuous (`gitalive` after every
   path-conversion export) and makes the PRE check and the END stamp fail closed (`tcn`); the mid-run sites are not
   rewritten.
6. **trainP was read, never written**: its 36 committed files hash `cc4e1d9690219eaf` (a hash of the folder's file
   hashes) before the copy and after the last control; P's helper was run with `python -B` (no `__pycache__`).

## 8. Deliberately NOT changed

- `PUB_MIN` 6, `IDC_MIN` 217, the floors of GN / CT / TR / ST, `EXEC_RULED` empty, `T_ROWS`, `GT_NEIGH`, `NEIGH`,
  `CHECKDEAD_GUARDS`, `HS_WANT`: no Q row moves what they name (read with git), or the floor is derived at run time.
- The CNR expectation stays DERIVED at HEAD; 863 is a prediction in comments and documents only.
- The two sweep lists are P's bytes (93 + 132 rows): no Q row adds or moves a roster row.
- precheck, csprojtemplate, cnrexpect, coverage, canaries, live: P's code.
- The mid-battery `git status | grep` restores (section 7, item 5).
- No release step is in any script.
- Nothing under `trainP/` or any earlier train directory; no other worktree; no commit, push or post.

## 9. Open items and questions for COORD

(As the derive left them. REVIEW ROUND 1, the last section, changed the state of two: Q-G is answered in the draft by
moving the row, and Q-H is replaced by the table's QUESTIONS 7 and 8. Its own open items are R1-A to R1-G there.)

- **Q-A (X_ROWS).** `encoding/json testing encoding/gob` is a proposal: confirm, or name the rows the i7 reads beside
  the i9 at Q. `sort` leaves with P's reason.
- **Q-B (the table's QUESTION 5).** S1 admits `PublishSymbols.csproj` and `PublishSymbolsLib.csproj` (no LangVersion)
  and `PackageSymbols.csproj` (a plain `LangVersion 14`) by name. Confirm the amendment, or ask the i9 for one commit
  that conditions the third file's line (then `S1_PLAIN` loses its entry).
- **Q-C (lesson 8's second half).** 'The set the converter produced comes from the converter's own output list': the
  converter at the base writes no such list. Rule 4 fails closed for a flat path one target moved; it reads
  `emitted-U-<os>.txt` when one exists. Ask the i9 what provides it (a flag that lists outputs is not an always-write
  flag), or rule that the GOOS-folder inference and the fail-closed refusal are the standing answer. Predicted
  unreached at Q: every footprint is committed in-seat.
- **Q-D (the timing class).** XS2 is the control run and writes the OWED line; the first run's FAIL still makes MOD
  non-zero, as at P's run1. Rule whether `singleflight TestPanicDo` / `semaphore TestWeightedAcquire` may be
  subtracted from the gate when the unchanged second run validates.
- **Q-E.** publishedTestHostPath and names over 93 characters (L:315 (3)): no row, no leg.
- **Q-F.** The shared object store: prune, or leave `gc.auto=0`, before Q's assembly (L:257 Q14).
- **Q-G (the table's QUESTION 1).** Rule the sixth BOARD hunk (uncomment the last line of `tQ-ruled.txt`) or move
  g-board-cs8500-census-limit directly below g-cs8500-managed-view. Until then the map is NOT-CLEAN at that row.
- **Q-H (the two CAND rows).** c1-golib-trim-default and g-r2m-caller-closure-v2 are refused by the table check as
  seated: write `READING IN (...)` in each row's notes when its reading is in, or strike the row. (Run on the real
  table at the derive: exactly those two refusals.) **L:369, written at 15:00 while this derive ran:** G's 633b045e8a
  is ACCEPTED, its condition met, so its row is owed the `READING IN` words; C1's trim default will be a NEW tip cut
  on ab6aa8f443, that tip is the Q row, and it stays CAND until release-smoke is green on four RIDs inside its
  budget; aot-smoke is a dispatch of its own, non-gating at Q (checklist 5b). The table was NOT edited by this step.
- **Q-I.** A C#-side failure with a network text is not KNOWN-EXTERNAL (not ruled): it reads FAIL.
- **Q-J.** `tQ-README.md` is a short restatement, not P's manual re-derived; the checklist leads where they differ.
- **Q-K.** The premap set is not adapted (section 6): if master moves and COORD wants a pair reading at the new base,
  the rehearsal's method (real merges on a lane) or `tQ-conflict-map.sh` is the instrument.
- **Q-L.** The E and F arms of C1's gate are 'not gating until read green on two trains' (C1's own comment, dated
  2026-10-06): Q's union is the first of the two.

## 10. REVIEW ROUND 1 (2026-10-06, 16:34 to 18:02 Central)

Two read-only reviews followed the derive: one of the seat table and the map (7 findings: 2 blockers, 2 should-fix, 3
nits), one of the scripts (16 findings: 3 blockers, 7 should-fix, 6 nits). **All 23 were applied; none was judged
wrong.** Two could not be CLOSED here, because the remedy is another lane's or a ruling: T2 (a row that does not
merge as cut: C1's re-cut) and S16 (a ruling of record to confirm). Nothing was built, converted, tested with
`go test`, committed, pushed or posted. Every change is in this folder, UNCOMMITTED; `trainP/` was not touched.
Scratch: `coord-scratch/tQ/derive/rr1/` (the edit scripts, each anchored and printing its site count; the control
outputs; the break harness).

### 10.1 The state moved while the round ran (each read at the remote or in the mailbox, with the time)

| When (Central) | What | Read where | What the folder does with it |
|---|---|---|---|
| 16:34 | c1-aot-smoke 44f29d50e6 ACCEPTED (release-smoke 37523436862 green on four RIDs); 'the table has no conditional row left' | COORD's post, mailbox b040b1eebe; ledger 16:35 | the row is seated (T1); its merge is NOT clean (T2) |
| 16:34 | the plan branch c2-scout-marker-parity moved a8db3c7e98 -> 63eb75c444 and COORD took the new tip | mailbox c62137e061; `ls-remote` 16:35 | the row names 63eb75c444 |
| 16:49, 16:55 | MASTER MOVED TWICE: 7098b8d3f9 -> b6e4856883 -> 0457242046 (two signed docs commits: `docs/KnownIssues.md`, `docs/Limitations.md`) | `ls-remote` 16:54 and 16:55; ledger 16:49, 16:55 | no row touches either page; the simulation's whole output is byte-identical on the three bases; the table's line 1 says which is which |
| 16:49 (21:49Z) | C1's aot-smoke run 37523439849: linux PASS; **win-x64 FAIL at startup** on a second Native AOT defect ('no metadata token available') | mailbox 079b80b37b | the table's QUESTION 8; non-gating at Q by ruling |
| 16:55, 17:38 | TWO BOARD DOCS ROWS ACCEPTED for Q: c1-board-aot-partial-cost 513ab62499 ('taken at the freeze as row 63') and g-board-logrus-launchdir-line 5497b2d7b9: 'the table is 64 rows' | ledger 16:55 and 17:38 | seated as rows 63 and 64, each with a line in `tQ-ruled.txt` |
| 17:38 | P2's linux full behavioral at g-r2m-caller-closure-v2's tip: 3151 / 3111 passed / 40 inconclusive / 0 failed | mailbox 368f7aa641; ledger 17:38 | the row says so; no reading holds a row |
| 16:49, 16:56 | COORD's running notes gained L:370 and L:371 (the handover branch moved 8094d8c0a7 -> ce14538f15, `trainL/tL-seats-draft.txt` +2 lines): the two landings, 'TABLE (62 rows, none conditional)', C1's BOARD row 'another BOARD append-against-append for the assembler map', and on the windows failure 'The fix is a golib seat for the train after Q unless it is small and ready before the freeze' | the file, read at 17:59 | the table's line 4 cites both lines; `tQ-ruled.txt`'s line for row 63 cites L:370 |

### 10.2 The table and the map (review 1)

| Id | Sev. | Finding | What was done |
|---|---|---|---|
| T1 | blocker | c1-aot-smoke 44f29d50e6, ruled into Q, was not a row; c1-golib-trim-default's condition and its 'nothing is cut on it' were stale | Row 56, directly below c1-golib-trim-default (one commit on it; own 5 files +99/-18, read with git). c1-golib-trim-default: its condition is met and it is no longer a candidate; it 'rides only with c1-aot-smoke behind it'. Header lines 1, 2 and 4, the hazards line and the writers' counts restated |
| T2 | blocker | **the row does not merge as cut** behind c1-release-smoke-published fd2b421bba | REPRODUCED twice from git objects: the row-order simulation (two unruled pairs at that row) and `git merge-file --diff3` on the three blobs, rc 2. `.github/workflows/os-matrix.yml`: 2 hunks, a NON-EMPTY base, one line rewritten by both rows in two places (the pack job's `if:` and the feed-download step's `if:`); `docs/CIMatrix.md`: 1 insert-against-insert hunk, 6 lines against 11. No ruled kind can cover the first and no order avoids it. The row is marked **CAND** ('a re-cut is owed'), so the table check refuses the list BY NAME before anything is created; `tQ-map.md` D2 holds the reading; `tQ-ruled.txt` says why no line can be written. NOT CLOSED: the re-cut is C1's, and the hosted release-smoke is owed at the re-cut tip (R1-A) |
| T3 | should-fix | g-r2m-caller-closure-v2 was still written as a candidate | The row's opening restated from the record (ACCEPTED 14:59; ledger f2f8b9686e); the candidate word is gone from the row (the predicate reads the bare word anywhere) |
| T4 | should-fix | g-board-cs8500-census-limit, two BOARD rows below its base row, met a hunk no ruling covers: the table was unassemblable as written | **The row is MOVED to directly below g-cs8500-managed-view**, the position the i9 rehearsed (it merges clean; the rehearsed rows keep the rehearsal's ten hunks). The table's order differs from v0 by that one row. The ruled line for the other answer stays COMMENTED in `tQ-ruled.txt` (R1-C) |
| T5 | nit | the hunk's count was worded two ways | 'the sixth on the BOARD (the seventh conflicted merge)', said of v0's position at the 61-row draft, in the row and in line 4 |
| T6 | nit | six rows were listed as owing acceptance by name; the ledger names four | Narrowed to two (g-board-foreign-defined-directions 6481ddbabb, c1-release-smoke-safe-tag 3b093f935a). The other four cite their ledger lines (06:34, 08:30, 11:49, 12:24, each read in `docs/phase4/LEDGER.md`), and the safe-tag row cites C1's post that reads its two runs |
| T7 | nit | three refs pushed after 14:30 were named nowhere | (a) g-board-logrus-launchdir-line and (b) C1's AOT-cost BOARD row were first added to the SLOT comment; the ledger then ACCEPTED both, so they are rows 64 and 63; (c) g-plain-gotype-removed f464628bf5 (and g-gotype-arg-comment 6efdecafd1, pushed later) joined the 'NOT Q BY RULING' list |

The table is **64 rows, rows-sha256 `867eedd4d363`** (62 rows `20856873cf11` with the two trim rows commented; the
first draft was 61 rows `96ddd479f81a`). Its final order is the list in this round's answer to COORD; row numbers
1 to 40 and 44 to 55 are the first draft's. `tQ-ruled.txt` holds TWELVE lines, ruled-sha256 `13300e8b417d`
(section 5's 'ten lines' and its commented sixth BOARD hunk are the derive's state): the BOARD at seven rows
and the five registration files of g-named-pointer-equality; the census-limit row needs none where it sits.

### 10.3 The scripts (review 2)

| Id | Sev. | Finding | What was done |
|---|---|---|---|
| S1 | blocker | `tQ-land-final-reads.sh`: the owed re-read gate passed by ABSENCE, and RUN was tied to nothing | `tQ-modules-legs.sh` ALWAYS creates `mod-logs/OWED-rereads.txt` (empty = nothing owed) and stamps `owed-lines=<n>`; the battery stamps the same count on its OWED line and raises a finding when the file is absent. The final reads refuse unless: RUN is a folder with `battery.console.log` and `tQ-logs/SUMMARY.txt`; the battery's PRE and END heads are one sha, and it is HEADFULL or its parent (`BATTERY_HEAD_OK=<10>` for an ancestor, COORD's call); its EXIT and rc lines read 0 or 7 (`BATTERY_RC_OK=<n>` for a ruled red); the owed file EXISTS and holds the count the battery stamped. An absent file is a STOP. RUN is never created |
| S2 | blocker | `tQ-land-prep.sh`: a stale run folder's rewrites could become the committed test sources | The same reader: the battery's PRE and END heads must both be HEADFULL, and its rc and EXIT must read 0 or 7. The head is stated on the PRE and PREP DONE lines |
| S3 | blocker | a linux lane read exit 0 and `movers=0` on a KNOWN-EXTERNAL module verdict | `tQ-linux-legs.sh` puts `external=<n>` on its END and DONE lines and EXITS 7 when that is all that stands; a STANDALONE `tQ-modules-legs.sh` exits 7 the same way (under the battery it stays 0: the battery carries the owed lines to its own EXIT 7). The linux brief and the checklist (sections 4 and 6) say 'exit 7 = a re-read is owed, post both lines' |
| S4 | should-fix | four control arms did not fail when what they guard was broken | TC extracts `tcn` from EVERY script that carries one (markers) and runs the arms on each copy, the battery's cwd form included. EXT gains the arm go=pass / cs=fail with the network text in the GO output (FAIL). TE gains TE-plant: one positive and three negatives. HW's plant lists `S:os` before `S:os/exec` |
| S5 | should-fix | the fixup re-baselined a golden that moved by the N-PARTIAL class with no stop, and its signed message would then be false | `GOLDEN_CLASS=gframe` (the default) STOPS on an N-PARTIAL golden (0 predicted); `GOLDEN_CLASS=npartial` admits it after COORD reads it. `hunkclass`'s verdict line carries every class's count and the files; the commit message quotes those counts. The 'G's frame class' texts are restated |
| S6 | should-fix | step 4t predicted net/rpc 14: the pathspec crossed into net/rpc/jsonrpc | `:(glob)src/core/$pkg/*_test.cs`; a read that is not its prediction is FLAGGED in either direction and listed. Measured at master: net/rpc 6 (glob) against 14 (plain), log/slog 3, encoding/json 1. The three documents say 6 |
| S7 | should-fix | the second-run symbol arms could pass on nothing (0 = 0) | `pdbfloor`: more than one `.pdb` beside every host, before and after, in XS2, `LM:xsync2` and `tQ-reread.sh` run 1b; XS2 also gates the second run's rc against the first's |
| S8 | should-fix | a slot written as a `#   SLOT` comment was counted and passed; `READING IN` anywhere released a candidate row | The map and the assembly REFUSE a list that holds slot comments unless `SLOTS_OK=<their count>`; the release is the form `READING IN (` and nothing looser |
| S9 | should-fix | no gating leg read i9-pack-symbols' outcome for a USER program | Leg **PUB2**: one behavioral program published TWICE through the converter's single-file profile into one folder; gate = both rc 0, the same file names, more than one `.pdb` after the second (when the union holds the symbols targets: derived at HEAD). **PUB2c**, its control, the row's off switch: a READING. NEVER RUN (R1-G) |
| S10 | should-fix | the controls suite printed failed=0 with arms not run; `rm -rf "$OUT"` took any folder | NOT RUN arms are counted and fail the suite unless `NOT_RUN_OK` names them; an existing OUT is deleted only when it carries the script's own marker file and holds nothing of a run (both control scripts) |
| S11 | nit | evidence copies and PUBSYM weaker than their comments; g-sweep-runtime-floor named nowhere | S and T evidence copies are taken only when `freshrec` says the file is the leg's; `pubsym` also opens each sweep log's 'full output:' file; the leg map names the row at ST and S (read: the T legs pass the runtime row 150 m, P2 210 m, both above the floor) |
| S12 | nit | statements in this file that were not true of the folder | Section 1's 'no script holds a base sha' reworded as measured; the inventory lists COORD's two files; the checklist counts the INDEX (42), not the staged diff; the tag sentence in `tQ-modules-legs.sh` corrected (P's oracle lines passed no tag); the table's '7th' is gone |
| S13 | nit | cross-references pointed at the wrong steps | The checklist says 'the final reads of step 3'; the two landing scripts' headers name their checklist steps; `tQ-assemble.sh` cites `tQ-README.md` step 3 for the map and `trainP/tP-README.md` step 3 for the follow-up procedure |
| S14 | nit | `ADDRSHAPE` narrower than the converter's pattern; HANDOWN read the written list only; S1's blind spot | `0x[0-9a-fA-F]+`, the seat's own; HANDOWN counts a file in the written list OR the drift list and says which; `tQ-selfcheck.sh` states S1's limit and the runtime guards that cover it |
| S15 | nit | eight files carried by rename read as Q's tools | Line 2 of each: 'R0 ONLY, NOT ADAPTED TO TRAIN Q ...' (`tQ-hunks.sh`: and the shared-store warning) |
| S16 | nit | the ruled line for g-cs8500-managed-view comes from the rehearsal's step 2 | LEFT AS WRITTEN and flagged in `tQ-ruled.txt` ('TO CONFIRM AT THE FREEZE'), with the two docs rows' lines, which have the same standing (R1-D) |

Found while applying, and fixed: `owed-lines=$(grep -c . file || echo 0)` printed TWO lines for an empty file
(`grep -c` prints 0 and exits 1); it is `owedn` now. The Bash tool's heredoc trap was met once more (a
backslash-r became a CR byte and a backslash-1 a 0x01 byte in one battery line, written through a spec): repaired
from a written file, and the folder was scanned with Python: 0 CR bytes, 0 control bytes, the non-ASCII counts of
the pre-review copy unchanged.

### 10.4 Controls that ran, with their results (none builds, converts or runs a test)

| Control | Result |
|---|---|
| `bash -n` on every `.sh`; `python -m py_compile` on every `.py` (the `.pyc` to scratch) | 22 of 22; 6 of 6; no `__pycache__` in the folder |
| `tQ-controls.sh`, final scripts (17:52) | **16 arms ok, failed=0 not-run=0, rc 0**: CB, EXT, EXT-real, TC, RT, RT-real, CS, PF, PS, RG, ED, TE-plant, TE-carrier, TE-M, HW, RES. New at this round: RT (22 arms on each landing script's extracted `runtie` / `rungate` / `owedcount`), RT-real (TRAIN P's REAL run2 files: rc=6 at d843ff263d, REFUSED as it stands, OK only with `BATTERY_RC_OK=6`, REFUSED for the head run1 read), CS (10 arms), PF (6 arms x 3 scripts), PS (5 readings), TE-plant (4 hunks) |
| **Every arm MADE TO FAIL once** (`derive/rr1/breaks.sh`: a fresh scratch copy a break, the suite run from the copy) | **12 of 12 CAUGHT, each by the arm it targets, suite rc 1**: the vacuous `tcn` in all three scripts, and in one alone (TC); `external_go` without go=fail (EXT); N-PARTIAL pairing with any `partial` line (TE-plant); `hostwall`'s leg as a prefix (HW); `rungate` without the head tie, with any rc accepted, and `owedcount` reading an absent file as 0 (RT); `pdbfloor` accepting zero (PF); the bare `READING IN`, and slots always acknowledged (CS); `pubsym` without the full-output file (PS). The review's four uncaught breaks are among them |
| The suite's own guards | an existing OUT without the marker: rc 2, nothing deleted; one with the marker AND a `tQ-logs`: rc 2, nothing deleted (both control scripts); two arms not run: rc 1, `not-run=2`; both named in `NOT_RUN_OK`: rc 0; one named: rc 1 |
| `tQ-selfcheck.sh` | PLANTS ok, S1 to S6 ok, failed=0 (run last, at 18:02, over the folder with this section in it) |
| Ad hoc controls of changed code outside a marked fragment (`derive/rr1/adhoc.sh`: the scripts' OWN lines, taken by anchor, run with stubs) | 8 of 8 ok. The fixup's golden stop: an N-PARTIAL golden dies under `gframe` and passes under `npartial` / `any`; a G-FRAME golden passes; an unread verdict dies. The message's counts by the fixup's own sed. `tQ-linux-legs.sh`'s END: exit 0 / 7 / 4 / 4 / 7 for the five cases, `external=` on both lines. `tQ-modules-legs.sh`: empty file standalone 0, one line standalone 7, one line under the battery 0, the file removed 7; created at start, never removed. `tQ-emitcheck.sh`'s HANDOWN loop: written / drift / both / another file. Step 4t's pathspec at master: net/rpc 6 : 14, log/slog 3 : 3, encoding/json 1 : 1 |
| The assembler's TABLE CHECK on a control copy (scratch redirected, the fetch stubbed, `TABLE_ONLY=1 SIGN_PROBE=0`, BASE = master 0457242046; several minutes an arm) | The 64-row table as it stands, `SLOTS_OK=2`: **rc 2, ONE problem, row 56 c1-aot-smoke (CAND)**; 19 stack lines, 3 order lines, 2 MULTI-BASE notes. With the candidate row released in the form `READING IN (` (a control plant, never a ruling): TABLE OK, 64 rows, rows-sha256 `867eedd4d363`, rc 0, 219 commits and 219 patch-ids. At the 62-row state: without `SLOTS_OK` two problems (the row and the slot comments); `SLOTS_OK=1` refused; the two trim rows commented, TABLE OK 60 rows `518c29cb3ab5` rc 0; the candidate row released in the form `READING IN (`, TABLE OK 62 rows `e0dfc18266dd` rc 0 (its stack and order tokens verify); the bare words in prose, refused (rc 2); a planted SLOT ROW (no sha), refused by name (rc 2) |
| `tQ-conflict-map.sh` on a control copy (scratch redirected): the slot refusal | `SLOTS_OK` unset, 1 and 3 against 2 comments: rc 2 each, 'Nothing was created or merged' (no worktree, no fetch) |
| The row-order simulation from git objects (a copy of the derive's `sim.py`, the base a variable) | 64 rows at master 0457242046: 637 paths, 161 content merges clean, **14 conflict pairs = the 12 lines of `tQ-ruled.txt` (every one used, none unused, each one hunk with an empty base) + 2 at c1-aot-smoke**; the BOARD 26507 lines, guard final. 62 rows: 12 = 10 + 2, byte-identical output on 7098b8d3f9, b6e4856883 and 0457242046. The 61 rows without c1-aot-smoke: 10 |
| `git merge-file -p --diff3` on `os-matrix.yml` (ours = fd2b421bba's merged with dae4ce596f's, clean; base = ab6aa8f443's; theirs = 44f29d50e6's) | rc 2; two hunks, both printed (the pack job's and the feed-download step's `if:`) |
| Remote tips (`ls-remote`, 16:35, 16:55, 17:41) | all 64 rows at their seated shas; master 0457242046; no patch under two shas (219 commits, 219 patch-ids at that base) |
| `trainP/` and the handover worktree | `trainP/`: 36 tracked files, `git diff HEAD` 0 lines, status 0 lines, 0 files under it newer than 13:00 today. The handover worktree: porcelain 0 lines, nothing staged (its HEAD moved under this round by COORD's own two notes commits; the two tracked trainQ files equal HEAD) |

### 10.5 The lessons whose disposition this round changed (section 3 stands for the others)

| # | Lesson | Disposition now |
|---|---|---|
| 2 | MOD KNOWN-EXTERNAL | done (Q4), and closed at its three exits: the owed file always exists and the landing proves it is the battery's (S1); a lane and a standalone module run exit 7 (S3) |
| 3 | no MSYS_NO_PATHCONV around git -C; a tracked-changes stamp must be able to fail | done (Q5); every copy of `tcn` is extracted and controlled (S4) |
| 5 | every -tests gate reads a row twice | done (Q7); the three x/sync second-run arms have a floor (S7); a USER program's second publish is leg PUB2, unrun (S9) |
| 8 | the emission check compares bytes | done for drift (Q10), now for HANDOWN too (S14); still open for the output list (Q-C) |
| 9 | a -tests arm for the fixup's derive | done (Q11); its prediction is the package's own directory and a miss is flagged either way (S6) |
| 14 | the record's write time | done by the seat; evidence copies are taken only when the file is the leg's (S11) |
| 16 | a second unchanged publish of a user program loses symbols | done by the seat; read hosted by C1's arm E (not gating) and, new, gated on the i7 by PUB2 (unrun) |
| 17 | the guard pins the lever, not the outcome | done by the seat and PUBSYM, which now opens the sweep's full-output file (S11) |
| 31 | the refresh message's class split | done (Q9); the FIXUP's message is written from the classifier's counts too (S5) |
| 38 | the hunks tool writes into the shared store | unchanged in code; stated on the file's second line (S15) |

### 10.6 Open items for COORD from this round

- **R1-A (the table's QUESTION 7).** c1-aot-smoke 44f29d50e6 does not merge behind c1-release-smoke-published. Ask
  C1 for the re-cut (merge fd2b421bba in, compose the two `if:` conditions, keep both inserts in `CIMatrix.md`);
  re-point the row, add its second stack token, read release-smoke AT THE RE-CUT TIP, then release the row. Or
  strike both trim rows. The record says 'no conditional row'; it has not read this merge.
- **R1-B (QUESTION 8).** Native AOT still fails at startup on windows behind the trim default (C1, 21:49Z; the i9 is
  sizing it). COORD's notes answer in part (L:371): the trim rows ride as they are and the fix is a golib seat for
  the train after Q 'unless it is small and ready before the freeze'. Open: that 'unless'. If a golib seat DOES join
  Q it is a GOLIB seat (GT x4, leg 5, H7 x3) and one more row behind the freeze.
- **R1-C (QUESTION 1).** The census-limit row is MOVED in the draft (one row against v0). Confirm, or move it back
  and uncomment its line in `tQ-ruled.txt`.
- **R1-D.** Three lines of `tQ-ruled.txt` are written from an acceptance or from the rehearsal's step 2, not from
  words that name the resolution: g-cs8500-managed-view be4ce10078 and the two docs rows of rows 63 and 64. Confirm
  each or comment it out. The two docs rows and c1-aot-smoke were never rehearsed.
- **R1-E.** Two SLOT comments remain (the Limitations docs row, which now has to be cut on today's master: COORD's
  own commit edits that page; COORD's own Q BOARD row). Seat, delete, or launch with `SLOTS_OK=2`.
- **R1-F.** Acceptance by name is still owed for g-board-foreign-defined-directions 6481ddbabb and
  c1-release-smoke-safe-tag 3b093f935a.
- **R1-G.** PUB2 is a NEW GATING leg that has never run (its control PUB2c is a reading). Confirm it gates at Q, or
  launch with `PUB2_PROJECT=''` and read C1's arm E by hand.
- **R1-H.** The fixup's default now STOPS on a golden that moves by the N-PARTIAL class (it re-baselined it before).
  Confirm; `GOLDEN_CLASS=npartial` is the switch.
- **R1-I.** `tQ-seats-v0-coord.txt` (COORD's own, tracked, not edited) is no longer the table: it has 62 rows, the
  plan branch at a8db3c7e98, the census-limit row in v0's place and no BOARD docs rows.
- **R1-J.** BASE is master 0457242046 at 17:41 and moves again with the census tool: read it at launch.

### 10.7 NOT done, and stated

No script was run for real: no map, assembly, fixup, battery, lane driver or landing script met a worktree. Leg
PUB2 / PUB2c and the changed arms (XS2's floor, `LM:xsync2`, the evidence copies, the full-output read) are unrun;
their readers are controlled on planted input. The simulation is not a merge (no rename detection, one path at a
time): the map's first real run is the first real reading of the order, of the three never-rehearsed rows and of
every hunk's shape. The i9's rehearsal unions are not in the local object store. `tQ-seats-gen.sh` and
`tQ-seats-v0-coord.txt` were not edited.
