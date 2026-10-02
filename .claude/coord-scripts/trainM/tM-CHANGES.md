# TRAIN M script set: every change against TRAIN L's

DRAFT 1, 2026-10-02. Source set: `hnd/.claude/coord-scripts/trainL/` (read-only). Each `tM-` file began as a byte copy
(`cp`) of its `tL-` source and was then edited; line endings are LF throughout (0 CR bytes in every file, counted with
`tr -cd '\r' | wc -c`). **Nothing in this set was run by its author.** `bash -n` and `python -m py_compile` are the
only executions (section 6).

**VERIFY ROUND 1 (2026-10-02) is section 9**: the dispositions of three verifiers' findings and every edit made for
them. Sections 1 to 8 are DRAFT 1's text; their line references (and the `lines L -> M` column of section 1) describe
the files BEFORE round 1. Section 9 names each changed construct by its text and gives the files' new line counts.

**VERIFY ROUND 2 (2026-10-02) is section 10**: COORD's rulings (`wf/notes/COORD-RULINGS-r2.md`), the dispositions of
the second round's 31 findings, and every edit made for them. Its largest change is ruling R1 (section 10.1): the
by-name lists are derived at run time. **No line number in sections 1 to 9 is valid for the files as they are now**:
each row there names its construct, and `tM-README.md` section 9 is the current line index (generated with `grep -n`
after round 2 by `wf/notes/_r2-lineindex.sh`). Counts quoted in sections 1 to 8 ("19 M test names", "15 seats",
"14 open questions", `SIBLINGS=refresh` as the default, `DEADLINE` default `none`) are DRAFT 1's and are superseded
where section 10 says so.

**VERIFY ROUND 3 (2026-10-02) is section 11**: COORD's rulings (`wf/notes/COORD-RULINGS-r3.md`), the dispositions of
the third round's 16 findings, and every edit made for them. **The seat list is FROZEN** (24 rows,
`rows-sha256=0b3fd4358320`, map union 11c188daf3, assembled head d4aae0aca0): hazard H4 is resolved inside its seat, the
follow list is empty, and every statement in sections 1 to 10 that predicts an H4 FAIL, an M2 follow-up merge, the map
union 382ceeb838 or the rows hash e1ef84e97561 describes an EARLIER list and is superseded by section 11.
`tM-README.md` section 9 is the current line index (`wf/notes/_r3-lineindex.sh`).

Line references are to the DRAFT (`tM-`) files unless they say `tL-`:
**A** = tM-assemble.sh, **CM** = tM-conflict-map.sh, **F** = tM-fixup.sh, **RA** = tM-regen-apply.py,
**B** = tM-battery.sh, **H** = tM-helpers.py, **E** = tM-emitcheck.sh, **ML** = tM-modules-legs.sh,
**LX** = tM-linux-legs.sh, **I9** = tM-i9-shard.sh.

Sources for a change (last column of every table):
- **M:n** = `trainM/tM-seats-draft.txt` line n. **L:n** = `trainL/tL-seats-draft.txt` line n.
- **SF-Hn** = hazard n of `wf/notes/seat-footprints.md` section 4; **SF 1d / 2a / 2c** = its overlap tables.
- **MR §n** = `wf/notes/M-requirements.md` section n.
- **LA An** = lesson n of `wf/notes/L-apparatus-map.md` section 4A (measured in L's runs); **LA B / C / D** = 4B, 4C, 4D.
- **FND** = P1's FINDING record for x/sync and x/mod (`wf/notes/_finding-xsync-xmod.md`).
- **METHOD n** = step n of the derivation task.

## 1. Inventory

| L file | M file | lines L -> M | + / - | Kind of change |
|---|---|---|---|---|
| tL-conflict-map.sh | tM-conflict-map.sh | 29 -> 31 | +6 / -4 | re-point; one comment corrected |
| tL-assemble.sh | tM-assemble.sh | 38 -> 89 | +65 / -14 | re-point; seat-table check; resume rule; count assert |
| (none) | tM-follow.txt | 0 -> 11 | new | the FOLLOW list, one file for two readers |
| tL-fixup.sh | tM-fixup.sh | 217 -> 429 | +352 / -140 | L's frame kept; L's content removed; M's two steps added |
| (none) | tM-regen-apply.py | 0 -> 87 | new | the corpus copy of fixup step 4 |
| tL-battery.sh | tM-battery.sh | 801 -> 987 | +490 / -305 | L's leg framework kept; legs re-pointed, added, dropped |
| tL-helpers.py | tM-helpers.py | 514 -> 784 | +373 / -108 | precheck and teattr re-derived; hunkclass, realmod new |
| tL-emitcheck.sh | tM-emitcheck.sh | 126 -> 142 | +44 / -28 | re-point; KEEP_U_ROOTS; derived HANDOWN; sa-<os> |
| emitdrift.py | emitdrift.py | 34 -> 34 | 0 | byte-identical |
| tL-modules-legs.sh | tM-modules-legs.sh | 384 -> 511 | +153 / -27 | L's legs unchanged; XS and XM added |
| tL-linux-legs.sh | tM-linux-legs.sh | 305 -> 460 | +228 / -74 | taken from P1's template seat (L's file + 3 fixes), then M's legs |
| tL-i9-shard.sh | tM-i9-shard.sh | 191 -> 211 | +49 / -29 | seat asserts read from the list; two named rows; UF |
| tL-i9-te.sh | tM-i9-te.sh | 17 -> 25 | +18 / -10 | optional baseline argument |
| tL-i7-sweeps.txt | tM-i7-sweeps.txt | 93 -> 93 | 0 | byte-identical (sha256 `40c6ec39...`) |
| tL-i9-shard.txt | tM-i9-shard.txt | 132 -> 132 | 0 | byte-identical (sha256 `057c78c1...`) |
| tL-lane-brief-i9.md | tM-lane-brief-i9.md | 163 -> 150 | rewritten | M's seats, expectations, post format |
| tL-lane-brief-linux.md | tM-lane-brief-linux.md | 167 -> 258 | rewritten | from P1's template brief, M's legs |
| tL-README.md | tM-README.md | 111 -> 228 | rewritten | launch, env, legs, manual steps, open questions |
| tL-CHANGES.md | tM-CHANGES.md | 107 -> this | rewritten | this file |
| controls/ (3 patches) | controls/ (2 patches + README.txt) | | replaced | controls for M's TE classes |
| tL-ng-parse.ps1 | tM-ng-parse.ps1 | 19 -> 19 | +6 / -6 | header only; the reader's body is L's bytes |
| tL-seats-draft.txt | (not carried) | | | M's list is `hnd/.claude/coord-scripts/trainM/tM-seats-draft.txt` |

Re-pointed in EVERY script (METHOD 2), not repeated in the tables below: base `75648a022b` -> `aa0a07d5fd`; worktree
`/h/go2cs-tmp-coord/tL` -> `tM` (and the `H:\` spelling `WB`, B:65, ML:29); branch `claude/coord-trainL-union` ->
`claude/coord-trainM-union`; scratch `coord-scratch/tL` -> `coord-scratch/tM` (lock, control builds); emission scratch
`tLemit*` -> `tMemit*`; log folders `tL-logs`, `tL-fixup-logs`, `tL-i9-logs` -> `tM-`; file names `tL-` -> `tM-`;
fixup subject `fixup: TRAIN L` -> `fixup: TRAIN M`; plant and stamp texts "TRAIN L" -> "TRAIN M". What still says
"TRAIN L", `tL-seats-draft.txt:n` or `coord-scratch/tL/run2` is a deliberate reference to L (a baseline file, a note
line, a history sentence); section 6 lists the scan.

## 2. Changes, file by file

### 2.1 tM-conflict-map.sh

| Lines | Change | Reason | Source |
|---|---|---|---|
| CM:2 | Header says "TRAIN L's LANDED master aa0a07d5fd". COORD's existing `trainM/tM-conflict-map.sh` in `hnd` says "TRAIN K's LANDED master aa0a07d5fd" | a stale name beside the right sha | METHOD 7 (stale-token re-read) |
| CM:5-6 | Records run 1 (15 seats clean, map union `8d7305053f`) and says to re-run after any change to the seat list | a map is a reading of ONE list; the seat list is not frozen (M:1 cutoff, M:18 in flight) | M:1, M:18; LA 1.1 |
| CM:8-10 | `WT`, `SEATS`, `BASE` re-pointed | METHOD 2 | |

The body is otherwise identical to the copy COORD already ran.

### 2.2 tM-assemble.sh

| Lines | Change | Reason | Source |
|---|---|---|---|
| A:18-19 | scratch `$X/tM`, base `aa0a07d5fd` | METHOD 2 | |
| A:22-54 | NEW seat-table check before anything is created: the row count is read (`NSEATS`); a ref or a sha listed twice aborts; a `stack-on <ref>` note must name an EARLIER row whose sha is an ancestor of this row's; a row whose sha is an ancestor of an earlier row aborts | "The row order is the merge order; a stacked seat carries a higher row number than its base" is stated in the list and was checked by nobody. Six M rows declare `stack-on` | M:2, M:12-17; SF 2b |
| A:66-75 | A seat "already contained in HEAD" is accepted only on a RESUME, when it is the second parent of a first-parent merge on the union; otherwise exit 3 | L skipped such a seat with a line (tL A:29), which makes the first-parent count smaller than the seat count and fails the battery's shape assert hours later | LA 1.2 (the NOTE on A:29) |
| A:77 | merge subject `... into TRAIN M -- ...` | METHOD 2 | |
| A:83-86 | after the loop: every seat an ancestor, and first-parent count == rows merged-or-resumed == `NSEATS` | the count the fixup and the battery assert, asserted where it is made; read from the list, never typed | METHOD 2 ("never hard-coded twice") |
| A:89 | prints the next command with the assembled head | L's operator copied the sha by hand | |

### 2.3 tM-follow.txt (NEW)

One `ref:seat-sha` per line, comments allowed, EMPTY at draft time. Read by F:130 and B:185 with the same expression.
At L the same four-entry list was written twice (tL F:80 and tL B:162) and had to be kept equal by hand. The
MECHANISM is L's and is kept whole: a follow-up is one extra signed merge after its seat and before the fixup, its
second parent on `origin/claude/<ref>`, descending from the seated sha and not equal to it (F:132-145, B:187-211).
Source: LA A7; METHOD 2 ("FOLLOW list EMPTY but mechanism kept"). Hazard H4's resolution is expected to arrive as one.

### 2.4 tM-fixup.sh

L's fixup carried two things (LA C): csproj cut at an older template (F1) and an attribute without its registration
rows (F2). Neither applies to M, measured on the pre-map union: 0 csproj carry `<LangVersion>latest</LangVersion>`,
and outparity reads 724 attributes == 724 rows. M owes two different things (SF-H1, SF-H2).

| Lines | Change | Reason | Source |
|---|---|---|---|
| F:1-39 | header rewritten: why M owes a fixup (G1 goldens, G2 corpus), what is NOT here (H4, H5), the step list | METHOD 4 | SF-H1, H2, H4, H5 |
| F:45-48 | NEW switches `REGEN=apply\|skip`, `GOLDENS=regen\|skip`, `SIBLINGS=refresh\|report`, `GOLDEN_CLASS=gframe\|any`; an unknown combination aborts (F:73-77) | each skip is COORD's explicit call and is stamped; the default does the work | |
| F:53 | NEW `REGEN_ALLOW`, default `^src/core/(go/internal/srcimporter\|net/http\|internal/reflectlite)/` | the corpus paths step 4 may write; a path outside it is refused before any write. **TO BE CONFIRMED BY COORD** | M:16 (srcimporter.cs:183); MR §1 (Extension A read on G's tip) |
| F:54 | NEW `EXEC_ROWS_EXPECT=1` | the roster's execution-config count after G's two roster commits | L:31, L:63; LA D |
| F:55-56 | `EXP_DESC`, `EXP_TH` default UNSET (L: 1140 and 229) | no M value is in any note; step 3 reports the counts and asserts them only when COORD sets them | METHOD ("do not invent an expected count") |
| F:57 | `SEATS_EXPECTED` optional (L: `11`) | the count is read from the list; the first-parent arithmetic (F:145) is what makes it mean something | METHOD 2 |
| F:65-68, 69-72 | NEW launch refusal (worktree, hnd, a draft folder, the main checkout) and companion-file check | L's fixup had neither (LA 1.3: "does NOT refuse an existing SUMMARY and takes no lock"); the battery's rule, applied to the script that now converts and builds | LA B (C1-9) |
| F:99-100 | NEW: takes the battery's lock | steps 4 and 5 run six `-stdlib` conversions and a CNR: floors 1 and 11 | LA B (C1-10) |
| F:101-111 | NEW build environment (dotnet10, temp and caches on H:, `unset GO2CS_MODULE_ROOT`) | L's fixup only edited text | |
| F:86-98 | `die` lists the edits made so far NUL-separated in `touched.z` with the exact discard command (L printed `git status` text cut at 400 characters, tL F:57-62); adds "the emission scratch is KEPT" | M's fixup can touch files with glyph paths and many files at once; a re-run at the same head re-uses 46 minutes of conversions (F:257-259) | LA B (C1-11) |
| F:127-147 | FOLLOW read from `tM-follow.txt`; shape arithmetic unchanged | 2.3 | LA A7 |
| F:148-161 | signing preflight BEFORE the long steps (key present, a real probe signature) | step 7 commits with `-S` after about 1 h 20 m of work; `landing-blocked-gpg` is in the fleet's history | |
| F:166-186 | NEW section PRERES: `tM-helpers.py precheck ... worktree` (2.7), `outparity` rc 0, `check-roster-format.ps1` under pwsh 7 and Windows PowerShell 5.1, each reading exactly `EXEC_ROWS_EXPECT` rows with an execution config | METHOD 4: one check for every overlap a check can read. The roster is the one path G and master both changed that a guard can count | L:31 ("re-run check-roster-format x2"); SF 2c; MR §3 |
| F:188-226 | step 1 (S1): the derivation is L's (union diff vs each seat's own commits), the expectation is 0, "L adds 5 csproj" and "the list equals the F1 pair" are removed; a non-empty list is still repaired with L's `sed -b` and must be a subset of the csproj M adds | L-only premises removed, the repair kept in case a late seat is cut at an older template | LA C; METHOD 2 |
| F:228-249 | steps 2 and 3 (optin.py): insert runs only when step 1 edited; check must print CHECK PASS; the two counts are reported | F:55-56 | |
| tL F:150-177 | REMOVED: L's step 4, the two D4 rows in OutputComparisonTests.cs and its CRLF byte edit | F2 does not apply. A gap is now a `die` at PRERES naming the owing seat (F:174): M has no ruling like L's R1 | LA C |
| F:251-287 | NEW step 4 CORPUS: `tM-emitcheck.sh` at the ASSEMBLED head with `KEEP_U_ROOTS=1 CSPROJ_GATE=0` in scratch `tMemitfix`; requires `plants=OK emit=OK` and `handown-written=0`; then `tM-regen-apply.py check`, then `apply`; the applied list must equal `git diff --name-only -- src/core`. A marker file lets a re-run re-use the kept evidence | SF-H2: F1+F6's hunk and Extension A's functions are in no seat's commits; "That one hunk rides M's corpus regeneration". The instrument is the battery's own leg E, so the fixup writes exactly what leg E would otherwise report | SF-H2; M:16; MR §2 (row E); MR §3 MUST |
| F:289-366 | NEW step 5 GOLDENS: a census PREDICTION from HEAD (projects with a go statement and no NoInlining line); the union's `check-no-regression.ps1` in place as the MEASUREMENT; any NOT MEASURED, untracked emission, harness change or moved csproj aborts; `hunkclass` requires every moved hunk to be G's frame class (or `GOLDEN_CLASS=any`); `run-behavioral.ps1 --update-targets --filter <project>` per moved project (the name must select exactly one); `.cs` == `.cs.target`; the runner's four phases per project | SF-H1: two of L's goldens predate G's go-creator frame. The prediction is scored against the measurement and never used in its place. Classification is by the diff's content (train-assembly skill) | SF-H1; `0d04adbb36` (G's own re-baseline line kinds) |
| F:368-379 | step 6: precheck AGAIN in `worktree` mode on the edited tree, then a purge of bin/obj/Generated | LA A2: the fixup's step must predict the battery's PRE-1, so the battery launches in `abort` mode. The purge returns the tree to what L's battery started from | LA A2 |
| F:381-429 | step 7: expected set = S1 list + applied corpus list + golden files; `--allow-empty` when nothing was owed (the union's shape still needs the commit); message rewritten for M; H5 and any other NOTE re-stamped "STILL OWED (manual)"; the emission scratch removed | L's never-`add -A` discipline kept (floor 8) | LA 1.3 invariants |

### 2.5 tM-regen-apply.py (NEW)

`python tM-regen-apply.py check|apply <worktree> <scratch> <out-list> <siblings 0|1> <allow ERE>`. Reads
`attr-<os>.txt`, `sa-<os>`, `written-U-<os>.txt` and the kept roots `U-<os>` that E leaves. Every listed path is
checked before one byte is written (RA:12-18): inside the allowed set; present in the worktree; not hand-owned
(golib, testing, unsafe, `*_impl.cs`, a line-anchored `[module: GoManualConversion]`); every target that wrote it
emitted the same CR-stripped bytes; the emission differs from the worktree. Exit 1 on any refusal with nothing
written, 2 on missing evidence. Reason: safety floor 2 (a conversion must never overwrite a hand-owned file) and the
one-tree rule, applied to a copy step no L script had. Source: SF-H2; CLAUDE.md "One tree".

### 2.6 tM-battery.sh

Framework functions `leg`, `capped`, `timeout_stop`, `purge`, `convbuild`, `deadline_check`, `poll_start/stop`,
`dlcount`, `tleg`, `finding`, the lock, the per-run-copy refusals, the GOROOT value checks and the disk floors are L's
bytes except where a row below says otherwise.

| Lines | Change | Reason | Source |
|---|---|---|---|
| B:1-63 | header and the LEG -> SEAT map rewritten for M's 15 seats | METHOD 3 | MR §1, §2 |
| B:69 | ONE seat-list name, `tM-seats-draft.txt` | L looked for a frozen `tL-seats.txt` that never existed (tL B:67) | LA A14 |
| B:70, 185 | FOLLOW read from `tM-follow.txt` | 2.3 | LA A7 |
| B:71, 162-170 | seat count READ; `SEATS_EXPECTED` optional; L's `11` and its "REPLACED originals" assert removed | METHOD 2; the two re-cut originals were L seats | LA 1.4 (tL B:152, 154) |
| B:171-177 | NEW: a row declaring `stack-on <ref>` must have that row's sha as an ancestor | the seat table's own rule, read at the tree | M:2 |
| B:75-79 | NEW `X_ROWS='internal/godebug log/slog'` join the S run list (B:259); `TC0_ROWS`, `TIERED_ROWS` | the two i9 rows whose execution config G's roster commits decide are read on this box too | M:20; L:41, L:63; MR §2 |
| B:80-82 | `K_SUMS` -> `L_SUMS` (L's run 2 summary); NEW `L_REWRITES`, `L_REWRITES_T` (L's own rewrite patches) | wall and TE baselines are L's battery of record on the same box | LA 1.4 (K_SUMS) |
| B:84, 106, 270 | `DEADLINE` default `none` (L: 17:30); validation and `past_deadline` accept it | 17:30 was a release-day value; the comparison is same-day clock only, so a default would stop a night battery at once. **TO BE SET BY COORD** | LA C (RD #17); LA 1.4 invariant 7 |
| B:87-89 | NEW `I9_BASELINE`, `EXEC_ROWS_EXPECT`, `E_BISECT_SEATS` | below | |
| B:102 | companions: `tM-follow.txt` added (nine files; `tM-ng-parse.ps1` stays) | 2.3 | |
| B:157-159 | fixup subject and single-parent assert re-pointed | METHOD 2 | |
| B:220-232 | PRE-1 calls `precheck ... head`; the ok lines of COUNT, REG, BOTH, ROSTER, H3, H4 and every NOTE are stamped | 2.7; the fixup ran the same arms on the tree it committed | LA A1, A2 |
| B:233-247 | PRE-1b and PRE-1c comments re-derived (7 projects in four files, 6 in OutputComparisonTests; the same 3 GoDefaultGodebug files measured at the base and at the pre-map union) | L's numbers were L's | git reads |
| B:313-337 | `restore_paths` gains a second pass: every path still tracked-modified whose diff is empty with `--ignore-cr-at-eol` is removed and checked out again (NUL-separated from `git status --porcelain -z`; a binary numstat counts as a real difference); sets `RESTORE_LEFT` | L's run 2 restored 275 paths and left 56 CR-only files, so the T legs did not start from HEAD's sources | LA A3 |
| B:378-386 | leg C: comment only (which M seats it gates; why it runs first) | the i9's guard walks `..\core` on disk | SF-H6 |
| B:394-418 | leg CB: 19 M test names (was L's 11), over `. ./internal/repoguard/`; the darwin pull census and the by-ref scan counts are stamped, not asserted (round 1: 20 names; round 2: the list is derived and the package list is `./...`, section 10.1) | a suite without `-v` cannot see a skipped or missing test | LA B (C0-2); SF-H6; MR §2 (row C) |
| B:420-426 | leg FX: comment re-derived (L's comment expected 1281; the tracked count is still only stamped); NEW finding when no `stale 0` reading is in the log | ten fixtures join the guard; 1291 is INFERRED, so it is read, not carried | MR §2 (row FX); SF-H6 |
| B:427-460 | FXc1 / FXc2: plant text only | controls kept (floor 13) | |
| B:462-490 | leg E: M's expectations; the bisect arms are DERIVED: for each seat in `E_BISECT_SEATS`, M = first parent of that seat's merge on the union, U = that merge; run only when E reads > 0 | L pinned two arms by sha (tL B:411, 413) for L seats. At M the fixup regenerates, so 0 is expected and a non-zero reading names files | LA 1.4; SF-H2; M:16 |
| B:492-505 | G1 / G2: finding unless each edition reads `EXEC_ROWS_EXPECT` rows with an execution config; the check count is reported (L: 1991) | the roster merge hazard, read by the guard of record | L:31, L:63; LA D |
| B:511-526 | SI / SIc: comment (7 new projects, 874 -> 881 Project lines; H4's second face) | | SF-H4; SF 2a |
| B:528-535 | ST: L's "binlog asserts" count removed from the stamp | it read 0 under 5.1 on a passing leg | LA A15 |
| B:547-578 | legs NGa51 / NGa7, NGb51 / NGb7, NGF-j0default / NGF-j0nugetgo KEPT (tL B:459-484). Changed: the parse list is READ from the tree (`git ls-files` of `src/tools/nugetgo/*.ps1` and `*.psm1`, plus j0-consume.ps1; L listed four files); the identity count is REPORTED and gated on rc 0 and a `ran N, failed 0` line (L's stamp said 29); L's "union carries the i9 fix merge" stamp is gone | At the 15-row list no M seat touches `src/tools` (base..pre-map union changed no `.ps1` but `src/_roster.ps1`), so these are 2-second regression legs. They stay because COORD's note of 04:26, written while this draft was being derived, announces two more M rows (c2-nuget-map, c2-s2-source-metadata) and names "M's nugetgo legs / i9 uuid pack" as their windows acceptance. Neither sha was in the local object store, so nothing about those seats is asserted | L:75 (the 04:26 night-log line, uncommitted in `hnd` at draft time); git reads |
| B:582-589 | leg 2b (go2cs.slnx end to end) kept; comment names the M seats that owe it; the Project-line count is stamped | M's seat draft names it; L's leg already built it | M:4, M:20; MR §2 |
| B:591-605 | NEW legs CT-build / CT, Debug and Release: `src/tests/ChannelTests/ChannelTests.csproj`, finding unless `Failed: 0, Passed: 24, Skipped: 0, Total: 24` | red on master from TRAIN H until this seat; no battery ever ran it | M:5, M:20; L:65; LA A16 |
| B:607-617 | NEW legs GN-build / GN: `src/tests/GenTests/GenTests.csproj`, Debug, totals REPORTED | the i9's seat touches `src/gen` and owes GenTests; no command, configuration or count is ruled. **TO BE SET BY COORD** | M:4; MR §2 (New legs) |
| B:619-628 | TR: comment and the `go2csPath` spelling | | |
| B:630-683 | GT: `GT_CLASSES` and `GT_TESTS` are M's (G's, H3's, the i9's, the host's); NEW `GT_EXPECT` asserts four new classes' result counts by name; finding on NOT FOUND, an off-count class or a missing TRX; L's C1 token probe (`GT_PROBE`, `trxmethods`) removed; totals reported | "a golib-touching seat runs GolibTests at both configurations"; three Release runs stay for H3 (timing-shaped) | SF-H3; MR §1, §2 (row GT) |
| B:685-705 | leg 4 (CNR): `CNR_EXPECT_N` default UNSET (L: 778); `M7DIRS` replaces L's five dirs; finding when no verdict line is read | N depends on H4's resolution (INFERRED 785). **TO BE SET BY COORD** | MR §2 (row 4); SF-H4 |
| B:707-738 | leg 5: `MGUARDS` (7 M projects + L's five checkdead guards) and `NEIGH` (L's eight, for M's reasons) | H3; the two re-baselined goldens are read isolated | SF-H1, H3; MR §2 (row 5) |
| B:740-752 | H7 x3: comment (what each flavour compiles for M) | | MR §2 (row H7) |
| B:754-776 | leg MOD: passes `REALMOD_TEST_TIMEOUT`; stamps KNOWN and REALMOD lines beside PASS / FAIL | 2.9 | M:20; L:67 |
| tL B:623, 638 | REMOVED: legs HOP and SPB with their restore and readings | no M seat touches `src/run-validated-sweep.ps1` | git reads |
| B:781-803 | leg PB kept, reason re-derived: F4 rewrites `publishTestHost`'s deadline, the function L's binlog seat composed with | M:12 "the publish-only floor max(testTimeout, 30m)" | M:12; LA D |
| B:805-846 | leg S: named readings for net/http (TestRegisterErr), internal/godebug (TestCmdBisect), log/slog (TestSetDefault, TestPanics, TestCallDepth), net/rpc; NEW finding unless a TC0 row's FRESH record reads `tiered=False` and log/slog's reads `tiered=True` | the sweep takes the config from the roster, so the row's own record is where a wrong config would show | MR §2 (rows S); L:41, L:63 |
| B:848-859 | leg NR x4 kept, reason re-derived (H3) | checkdead under G's classification is timing-dependent | SF-H3 |
| B:861-889 | TE: `teattr --baseline <L's patch>` (2.7), a READING; HS unchanged; NEW UF: untracked, not-ignored paths under `src/core` after the sweeps, finding unless 0; `S_LEFT` keeps what the restore measured | c1-fixture-tracking's "e2e 0 untracked" | M:6; L:29, L:43; MR §2 (New legs) |
| B:891-947 | T legs: NEW `tbs_named` asserts `TestTracebackSystem/panic` reads " disclosed" in TBS-W and in T:runtime (and states /trap); runtime/pprof's named list gains TestMemoryProfiler; runtime's gains TestLineNumber | THE re-read the seat draft assigns to this battery: the first tree holding checkdead and G's branch together | M:20; L:59; MR §2 (rows T) |
| B:949-964 | TE-T / HS-T, NEW UF-T | the same readings over the T legs' rewrites | |
| B:966-975 | TE-i9 passes the optional baseline | 2.10 | |
| B:977-982 | TK-WALL -> TL-WALL against `L_SUMS` | the lesson is stated in the comment: row wall is mostly MSBuild, so the ratio is a prompt to read | LA A5; L:40 |
| B:984-987 | END states what the after-sweep restore MEASURED; NONZERO LEGS note says a MOD KNOWN line is not a red | L's END asserted "the sweep's were restored" without measuring it | LA A3 |

### 2.7 tM-helpers.py

`coverage`, `canaries`, `trx`, `csprojdrift`, `rosterrow`, `outparity`, `trxmethods`, `deadlock`, `cmpnames`,
`lockcheck` are L's bytes.

| Lines | Change | Reason | Source |
|---|---|---|---|
| H:81-108 | `precheck` takes a mode. `head` (battery): every arm reads HEAD and the fixup commit's own net inserts into a COUNT file are a contributor. `worktree` (fixup): the same arms read the worktree and the fixup's contribution is the worktree's delta against HEAD | at L the COUNT arm read HEAD while the edit sat in the worktree; PRE-1 then read expect + 6 and the battery was relaunched with `PRECHECK_MODE=warn` | LA A2 |
| H:109-116 | each seat's OWN base = `git merge-base <seat> <base> <every earlier row>` | L's ancestor rule counted a shared base seat inside every seat stacked on it; six M seats stack on p1-m-repros | LA 5 (annex); SF 2b |
| H:117-126 | COUNT: base + each seat's own net inserts + the fixup's | measured on the pre-map union: projitems 425 -> 432, slnx 1069 -> 1076, Compile / Target / Transpile +21, Output +18 | SF 2a; SF-H7 |
| H:127-141 | NEW arm REG: every base key and every seat-added key of the six registration files is present once (Include, Project Path, test method name) | a line count cannot see a dropped key replaced by a duplicate | SF 3(e); merge-hazards skill |
| H:142-146 | REMOVED: L's K9 (ThreadStateCensusTests rows, five runtime map lines pinned to K shas) and K6 (`StackTraceHidden` in os file_unix.cs) | premises of TRAIN K's assembly; no M seat touches those files, and G re-encodes runtime map lines for other sources | LA A1 ("re-derive every carried assert against the NEW base") |
| H:148-181 | NEW arm BOTH: for every path a seat AND master both changed since that seat's merge-base (derived, not listed), every substantial line either side added is in the merged file and no line either side removed is back | git reports these merges clean; the check is on the RESULT. The roster carries G's two rows AND master's block | SF 1d, 2c; L:31; MR §3 MUST |
| H:182-194 | NEW arm ROSTER: no row's N + D moved against the base; the execution annotations are printed as a reading | "No count in the table or the header changes" | MR §3 MUST NOT |
| H:195-201 | NEW NOTE H5: the prose count of rows that "opt back out" against the annotated rows; never a failure | G's to word; no guard counts it | SF-H5 |
| H:202-219 | NEW arm H3: `Goroutine.cs` holds master's checkdead identifiers at the base's counts and G's `systemBasis` at G's count, and no conflict marker | the textual half of a semantic coupling; the behaviour is the battery's | SF-H3 |
| H:220-230 | NEW arm H4, HARD: a behavioral directory with tracked `*.go` and no tracked `*.cs` | CNR enumerates every directory with a Go file and would list the emission as CHANGED; 0 such at the base, 2 on the pre-map union | SF-H4 |
| H:363-367 | `treport` prints `ENV configuration=... tiered=...` from the record | the battery's execution-config finding reads it | L:63 |
| tL H:338-394 | REMOVED: L's `teattr` (D6 and cross-package signatures) and with it the two raw-codepoint character classes at tL H:351-352 | they read two L seats; the byte hazard leaves with them | LA 1.5 (Byte hazard) |
| H:475-562 | NEW `teattr [--baseline <patch>] <patch>...`: every hunk is G-FRAME (a NoInlining prefix insertion or its using line), MAP (a position-map re-encode only) or OTHER; with a baseline, the OTHER hunks the baseline does not carry are listed. Exit 0 when it read, 2 when no patch | G regenerated no committed `*_test.cs`, so a sweep's rewrites are EXPECTED to carry G's class; what the other seats may move is not ruled, so it is a reading against L's own patch | SF 3(a); `0d04adbb36`; MR §5 |
| H:564-580 | NEW `hunkclass <patch>...`: per file the same classes; exit 1 when any hunk is OTHER | the fixup's golden step classifies by content | SF-H1 |
| H:582-606 | `wallcmp`: label `M=... L(min)=...`; comment states L's lesson | | LA A5 |
| H:693-772 | NEW `realmod --log --root --module <pkg>=<spec>...`: per package PASS (`N`), KNOWN (`N/T:Name:go:cs`: the only undisclosed differing names are Name or its subtests, on the stated sides), CLEARED (a KNOWN package that validates T), FAIL; a package in the log with no spec is FAIL; counts `dotnet timed out` lines (F4); exit 1 on any FAIL | METHOD 3: the three KNOWN non-passes are reported by name and never red; anything else is. A linux count that differs on windows is a COUNT-NOTE because how that reading counted is INFERRED | M:20; L:67; FND §2-§4 |

### 2.8 tM-emitcheck.sh, emitdrift.py

| Lines | Change | Reason | Source |
|---|---|---|---|
| E:1-24 | header: the three uses (battery leg E, fixup step 4, derived bisect arms) | | |
| E:29, 66-70 | NEW `KEEP_U_ROOTS`: the three U roots survive the EXIT trap for the caller | the fixup copies from them | SF-H2 |
| E:31-33 | `W`, scratch default and guard pattern re-pointed | METHOD 2 | |
| E:41 | GOROOT regex in the bracket form | L's E:38 still held the older double-backslash form after the battery's was rewritten | LA A13 |
| E:50-56, 123-127 | `HANDOWN` DERIVED: every `src/core` `.cs` that differs between M and U and is golib, testing, unsafe, `*_impl.cs` or carries a line-anchored `[module: GoManualConversion]` | L listed its own nine files | LA 1.6; METHOD 2 |
| E:62 | the evidence copy also takes `sa-*` | | |
| E:121-122 | L's K9 line renamed RUNTIME-MAP, premise re-derived (G commits its own map re-encodes, so none may be union-attributable) | LA A1 | |
| E:131 | NEW `sa-<os>` = siblings only the UNION arm rewrites | the train's own share of the sibling refresh | LA C (RD #8) |
| E:75-102 | the conversion loop | byte-identical to L (floors 1, 2; plants) | |

`emitdrift.py` is byte-identical.

### 2.9 tM-modules-legs.sh

| Lines | Change | Reason | Source |
|---|---|---|---|
| ML:1-28 | header | | |
| ML:33 | `DEADLINE` default `none` | as B:84 | |
| ML:34, 39 | NEW `REALMOD=1`, `REALMOD_TEST_TIMEOUT=2m` | the real-module legs' package deadline is always passed explicitly; 2m is the value P2's acceptance was read at. **TO BE SET BY COORD** | METHOD 3; MR §2 (row MOD -test-timeout); MR §5 q3 |
| ML:67-71 | GOROOT regex in the bracket form | | LA A13 |
| ML:89-99 | L's five seat shas stay (they are ancestors of `aa0a07d5fd`; two build the control converters); NEW loop: every row of the seat list is an ancestor | | git reads |
| ML:103-397 | L's legs CM, MR2, JWT, MR3, MR4, MR4v, MR4c, MR6, MR6c, MR6p, MR6e, MR6pc | UNCHANGED: kept whole as the regression of the `-tests -recurse` driver, whose test conversion F4 and F8 edit | M:12, M:17 |
| ML:399-437 | NEW `rmrun` (the whole-module driver with `-test-timeout` stated) and `realread` (one stamped line per package; FAIL counts; the F4 verdict) | | METHOD 3 |
| ML:441-454 | NEW leg XS: `golang.org/x/sync@v0.19.0` copied from the module cache, read-only files counted, `errgroup=5 syncmap=3 singleflight=11/12:TestPanicDoChan:pass:- semaphore=7/8:TestWeightedAcquire:pass:fail`, module copy hashed before and after | the gate L's own blocker asked for; the first windows reading | L:23; L:67; M:20; LA A6; FND §2 |
| ML:455-498 | NEW leg XM: `golang.org/x/mod@v0.33.0`; when the cache lacks x/tools v0.41.0 and holds v0.42.0 the COPY takes P1's stated pin (one require line, two go.sum lines built from the cache's own files); `modfile=323 module=16 semver=9 sumdb/dirhash=6 sumdb=4 sumdb/note=7 sumdb/storage=1 sumdb/tlog=16/17:TestCertificateTransparency:*:- zip=build`; a preflight refusal reads NOT MEASURED | measured on the i7's cache: v0.42.0 present, v0.41.0 absent. R6: never a download | L:49; L:67; MR §2 (MEASURED on the i7's module cache); FND §3 |

### 2.10 tM-linux-legs.sh

Derived from P1's template seat (row 8, blob `6175c999c0:.claude/coord-scripts/templates/linux-legs.sh`), which is
`tL-linux-legs.sh` with three fixes; each is still marked `TEMPLATE FIX n` (LX:51, 268, 344-348). Source for taking
the template: M:10; LA A11; LA D.

| Lines | Change | Reason | Source |
|---|---|---|---|
| LX:32-35 | `HD`, `SEATS`, `BASE`, `REALMOD_TEST_TIMEOUT` | METHOD 2 | |
| LX:88-104 | seat asserts read from the seat list beside the driver; the fixup subject is M's | L wrote shas | METHOD 2 |
| LX:165-169 | NEW `EXPECT_TIERED` in `summ`: the record's `tiered` must equal the row's expected value | log/slog is run with `-test-tiered` (the direct `-tests` path does not read the roster) | L:41, L:63 |
| LX:253 | `GTL` filter = M's classes, including `DarwinStdDescriptorContractTests` (compiled only off windows) | the linux arm of c1-darwin-std-hygiene | M:9; MR §4 |
| LX:275 | NEW `disclosed <suffix> <test>`: the name must be in the fresh record and tagged disclosed | | L:59 |
| LX:283-331 (P1) | L1 unchanged; L2 + `mseats`; LCn = the i7's 19 names; LCf; L4 = net/http, internal/godebug (TC0), log/slog (tiered), sync, os, os/exec; LPB; NEW UF stamp | the i9's `siginfo_linux.cs` compiles only here; the TC0 rows on a second OS | MR §4; SF-H6 |
| LX:333-366 (P1) | LB: 17 projects (the 7 M projects, L's five checkdead guards, the linux-exclusive set incl. SetegidBroadcastSeam's residual) | H8: the linux no-regression re-emits it whole; the residual is reported | SF-H8; L:34, L:35 |
| LX:368-420 (P1) | NEW LM: x/sync and x/mod through the same `realmod` reader | P1 owns the real-module originals; both boxes are uid 0, so a non-root reading is stated or posted NOT MEASURED | L:46, L:47, L:49; LA A12 |
| LX:421-457 (P2) | GT; runtime/pprof + TestMemoryProfiler; the FULL runtime row with `/panic` and `/trap` disclosed by name; TBS-LOOP x `TBS_N` with the disclosed check per iteration | the re-read L:59 assigns to the M union | L:33, L:59; MR §4 |

### 2.11 tM-i9-shard.sh, tM-i9-te.sh, the two lists

| Lines | Change | Reason | Source |
|---|---|---|---|
| I9:67-81 | seat asserts read from `tM-seats-draft.txt` beside the script (L: six written shas) | METHOD 2 | |
| I9:82-88 | the fixup must be in the union; the csproj check's message no longer says "S1 fixup" | M's fixup is not S1's | |
| I9:170-171 | named rows add internal/godebug (TestCmdBisect) and log/slog (three names); an `ENV configuration/tiered` line is printed from each record | the two rows G's roster commits decide | MR §4 |
| I9:198-201 | NEW UF stamp after the last row | at L this shard left fixture copies under `src/core` | L:29, L:43 |
| tM-i9-te.sh | takes an optional baseline patch (the i9's own TRAIN L shard patch) | `teattr` is a classifier with a baseline at M | 2.7 |
| the two lists | byte-identical: M adds and removes no roster row (225 = 93 + 132) | | MR §4 |

### 2.12 controls/

L's three patches tested the D6 and cross-package signatures and are not carried. NEW:
`te-pos-goframe.patch` (`git diff -U0 0d04adbb36^ 0d04adbb36 -- src/tests/Behavioral/CaptureHoistThroughConversion/`,
G's own re-baseline of one project) and `te-neg-i9-testing.patch` (`git diff -U0 02a0b44467 7a8d02f6fe --
src/core/testing/testing.cs`, an attribute insertion of the same SHAPE that is not G's). `README.txt` gives the
PREDICTED reading of each under `teattr` and `hunkclass`. Neither reader was run on them (floor 13 is owed: manual
step M9 in tM-README.md).

### 2.13 The two lane briefs

- **tM-lane-brief-i9.md**: rewritten. Kept from L: the standing exclusion, the setup steps, the concurrency and WHEA
  rules, the switches not to pass, the control rule for a failing row. Changed: the driver takes its seats from the
  list; the two rows that are also read on the i7; what each M seat does to these rows; UF; the post format. Not
  carried: L's NGE section (i9-nugetgo-pack banked at L). The brief says so, and says that an NGE step for
  c2-s2-source-metadata arrives in the GO with that seat's own numbers if COORD seats it (L:75), never from L's text.
  Added "What is NOT yours": the linux and darwin items of the i9's own seats, ChannelTests and GenTests (i7 legs CT,
  GN), the TRAIN N seat (M:19), the open owner question on net/http test sources (L:70). Sources: MR §4; M:4, M:5,
  M:19; L:75.
- **tM-lane-brief-linux.md**: rewritten from P1's template brief (row 8). Section 0 states both boxes are ROOT
  readings (LA A12); section 3's "csproj among the rewrites must be 0" is corrected for runtime and runtime/pprof
  (LA A12; L:33); the legs of 2.10 with their expectations; how a non-root reading is taken or posted NOT MEASURED.

### 2.14 tM-README.md

Rewritten: the order of work; the environment-variable table; why the union owes a fixup; files; ten manual steps
(M1 to M10) for the hazards no check can decide; the leg table with L's walls as a size; the TO BE SET BY COORD list;
what is deliberately not in the battery; 14 open questions; what was and was not checked.

## 3. L content not carried, and why

| L content | Why it is not in M | Source |
|---|---|---|
| The i9 brief's NGE section (the uuid pack end to end) | i9-nugetgo-pack banked at L; a new one needs c2-s2-source-metadata's own numbers, and that sha was not readable here | L:75 |
| Legs HOP, SPB | no M seat touches `src/run-validated-sweep.ps1`; PB stays for F4 | git reads |
| GT's C1 token probe | c1-token-ids landed in L | LA 1.4 |
| `teattr`'s D6 / cross-package signatures, their three controls, the two pinned bisect arms | they read L seats | LA 1.5 |
| precheck's K9 and K6 arms | TRAIN K's resolutions | LA A1 |
| The fixup's S1 pair assert and D4 rows | F1 and F2 do not apply to M | LA C |
| `SEATS_EXPECTED=11`, the REPLACED-originals assert, the four FOLLOW entries | L's seat table | METHOD 2 |
| `DEADLINE=17:30`, `CNR_EXPECT_N=778`, `EXP_DESC=1140`, `EXP_TH=229` (defaults), FX's 1281 (a comment) | L's numbers; M's are reported until COORD sets them | METHOD ("do not invent") |

## 4. L's lessons (L-apparatus-map 4A) and where each landed

| Lesson | Where |
|---|---|
| A1 stale carried asserts | H:142-146 (K9, K6 removed); E:121-122 (premise re-derived) |
| A2 the fixup's own inserts in COUNT | H:81-108, H:117-126; F:168, F:370; B:229 launched in `abort` mode |
| A3 CR-only files left by the restore | B:313-337; B:888, B:984 |
| A4 a purge before a leg that needs the exe | unchanged from L: `convbuild` at B:789 and B:912; the fixup purges only at its end (F:375-379) |
| A5 WALLCMP compares row wall | stated at B:977-980 and H:582-584; NOT fixed (the package-elapsed reader does not exist) |
| A6 a real module-cache module through `-tests` | ML:441-498; LX:368-420 |
| A7 follow-up merges | tM-follow.txt; F:127-147; B:181-214 |
| A8 post-battery merges owe named re-reads | not scripted, as at L: the battery's shape check describes seats + follow-ups + ONE fixup, so a merge after the battery owes its own re-read script |
| A9 `os` on the i7 is not a reading | the lists are unchanged: `os` and `testing` are i9 rows (tM-i9-shard.txt:131-132) |
| A10 log/slog keeps release-tiered | B:79, B:830, B:842; LX:316; I9:171 |
| A11 linux driver defects | LX is P1's template (TEMPLATE FIX 1 and 2) |
| A12 brief errors (root readings; csproj on linux) | tM-lane-brief-linux.md sections 0 and 3 |
| A13 GOROOT regex | E:41, ML (bracket form); B:133 unchanged |
| A14 stale names | B:69; F:60; LX:33; I9:74: one seat-list name |
| A15 ST51's binlog stamp | B:528-535 |
| A16 ChannelTests never run | B:591-605 |

## 5. Hazards (seat-footprints section 4) and where each is handled

| Hazard | Script check | Manual step (tM-README.md) |
|---|---|---|
| H1 two stale L goldens | F:289-366 (measure, classify, re-baseline, verify); B:696 CNR; B:729 isolated | |
| H2 corpus footprints no seat committed | F:251-287 + RA; B:468 leg E expects 0 | M4 (confirm `REGEN_ALLOW`), M5 (siblings beside hand-owns) |
| H3 checkdead under G's classification | H:202-219 (text); B:647 GT x4; B:729 guards; B:825, B:854 net/rpc x5; B:904-946 runtime by name | |
| H4 Go-only repro directories | H:220-230 HARD (the fixup and the battery refuse) | M2 (a ruled follow-up merge) |
| H5 the roster sentence | H:195-201 NOTE; re-stamped STILL OWED at F:428 | M3 |
| H6 guards meeting for the first time | B:386 leg C first, on a committed tree; B:409-418 by name; the census line stamped | M6 (`c1-route-sysctl`) |
| H7 adjacent projitems inserts | H:117-141 COUNT + REG; the pre-map | M8 (a changed row order) |
| H8 SetegidBroadcastSeam taken by hunk | LX LB reports its residual | M7 |

## 6. Self-check (METHOD 7)

| Check | Result |
|---|---|
| `bash -n` | OK: tM-assemble.sh, tM-battery.sh, tM-conflict-map.sh, tM-emitcheck.sh, tM-fixup.sh, tM-i9-shard.sh, tM-i9-te.sh, tM-linux-legs.sh, tM-modules-legs.sh |
| `python -m py_compile` | OK: tM-helpers.py, tM-regen-apply.py, emitdrift.py (`__pycache__` removed) |
| tM-ng-parse.ps1 | not parsed here (running it is running a draft script); its body is L's bytes, only comment lines changed, and it holds 0 non-ASCII bytes |
| CR bytes | 0 in every file of the draft folder |
| `diff -u` against each L source, re-read | done for every script; three defects of my own found and fixed on the re-read (a wrong step number in E's header, an unmeasured row count in a B comment, a row count in the README) |
| Stale-token scan (`tL-`, `trainL`, `TRAIN L`, `02a0b44467`, `75648a022b`, `f8f911df00`, L seat names) | no script line re-points to L. What remains is deliberate: "TRAIN L's landed master aa0a07d5fd"; `tL-seats-draft.txt:n` note citations; L's run-2 files as baselines (B:80-82); history comments naming r-m3 / r-m4 / r-m6 (L seats now on master); `02a0b44467` only as the base of the negative control's diff (`controls/README.txt`, and section 2.12 here); `75648a022b` only in this file's re-point list |
| Line citations in tM-README.md | each printed and compared with the file (A:22-54, B:98-101, B:109, F:53, F:65-68, F:99, F:167, F:257-259, H:142, H:182, H:195, H:220) |
| Message formats the new readers parse | read at the source: `-tests %s -> %s` (moduleTestsDriver.go:227 at aa0a07d5fd), `Validated %d tests against go test` (testConversion.go:9390), `%s timed out after %s` with name `dotnet` (testConversion.go:9727 at aa994a1a1c) |

## 7. What a first run exercises for the first time

Nothing below has executed. A rehearsal of the fixup on a scratch clone of the pre-map union is the cheapest way to
find what reading the code could not.

- F steps 4 and 5 end to end, and `tM-regen-apply.py`.
- `precheck`'s REG, BOTH, ROSTER, H3 and H4 arms and its two modes; a false FAIL stops the fixup at PRERES (fail-closed).
- `teattr`, `hunkclass` (controls/ gives predicted readings) and `realmod`.
- The CT, GN, XS and XM legs; the result-line greps of CT and GN assume the `Passed! - Failed: n, Passed: n, ...` line L's TR and GT legs read.
- `restore_paths`' CR-only pass.
- The linux LM leg and the `disclosed` helper.

## 8. Inputs that moved while this draft was derived

The seat list this set was derived for is the 15 rows of `trainM/tM-seats-draft.txt` (last written 02:42). COORD's
notes file `trainL/tL-seats-draft.txt` gained lines after the analyses were taken; the last two matter here and were
read at 04:34 (uncommitted in `hnd`):

- **L:72 (03:17)** and **L:75 (04:26)**: "M ROWS TO ADD after the apparatus workflow: c1-route-sysctl|5f4eb242ba,
  c1-darwin-inode64|0a9454986b, c2-nuget-map|cf563ac526, c2-s2-source-metadata|a6ec59d5bc,
  coord-warnings-census|fe8bfb4c96."

What the set already does for them, and what it does not:

| Announced row | Read here? | Already covered | NOT derived |
|---|---|---|---|
| c1-route-sysctl `5f4eb242ba` (stack-on c1-darwin-linkname-pulls) | yes (local object): 3 files, one new `route/darwin/syscall_impl.cs`, 4 lines of `visitFuncDecl.go`, the pull-census test | seat count, COUNT / REG, stack-on check: all read from the list. CB stamps the census line and asserts no count. The seat itself removes the `route.sysctl` row (manual step M6's condition). E derives HANDOWN, so the new `_impl.cs` is read | its darwin corpus footprint, if any (leg E reports it; the fixup refuses a path outside `REGEN_ALLOW`) |
| c1-darwin-inode64 `0a9454986b` | no (not in the local object store) | a golib seat is gated by 2b, GT Debug + Release x3, H7 x3; GT totals are reported | its GolibTests classes by name (`GT_CLASSES`); its darwin run is CI |
| c2-nuget-map `cf563ac526` | no | leg C runs `./...`; the NG legs parse every script under `src/tools/nugetgo` | its named tests for CB; any count |
| c2-s2-source-metadata `a6ec59d5bc` | no | as above; NGa / NGb / NGF-j0 are "M's nugetgo legs" on the i7 | the i9's uuid pack (NGE) with this seat's numbers; its converter tests by name |
| coord-warnings-census `fe8bfb4c96` | yes: one docs file | leg C's docs suites scan it | nothing |

Every script reads the seat count from the list, so adding rows needs no script edit. What DOES need a person after
rows are added: re-run the pre-map; re-read sections 5 and 7 of tM-README.md; add each new seat's test names to
`CB_TESTS` (B:409) and `GT_CLASSES` (B:647) if it has any; decide the NGE step.

## 9. Verify round 1 (2026-10-02)

Three verifiers read DRAFT 1 through three lenses: **SL** stale literals (9 findings), **LC** leg coverage (19),
**HF** hazards and the safety floor (19). Each finding was re-read at the cited file and line and judged before any
edit. **Nothing in the draft was executed**: the only commands were git reads, `[ -e ]` and `find` on exact
module-cache paths, reads of TRAIN L's kept run logs, `bash -n`, `python -m py_compile`, and three read-only
re-implementations kept in `wf/notes/` (`_r1-both-permerge.py` and its output, `_r1-patchids.txt`,
`_r1-hostwall-probe.py`). The pre-round files are kept in `wf/notes/_r1-before/`.

Dispositions: **fixed** = the defect is real and the draft changed; **fixed, differently** = real, repaired by another
mechanism than the one proposed (the reason is given); **COORD** = needs a ruling or a value the draft cannot derive.
No finding was rejected as wrong. Constructs are named by their text, because round 1 moved the line numbers.

### 9.1 Stale literals

| # | Finding | Disposition | What changed |
|---|---|---|---|
| SL-1 | tM-fixup.sh: step 1's S1 repair cannot coexist with steps 4 and 5 (a repaired csproj reads as "CNR moved a csproj", or as a corpus file outside the applied list) | fixed | `s1-list.txt` is kept out of the step 4 and step 5 sets: `regen-changed.txt`, `gold-moved.txt`, `gold-files.txt` and the verify compare filter it (`grep -vxF -f`); `gold-cs.patch`, `gold-all.patch`, `regen.patch` and `regen-numstat.txt` take the `*.cs` / `*.cs.target` / `*.cs.auto` pathspecs. S1N is 0 at the 15 rows, so the path is still not taken |
| SL-2 | tM-emitcheck.sh: `W` written twice, one copy not overridable; the fixup's `W` override never reached the emission check | fixed | `W=${W:-/h/go2cs-tmp-coord/tM}`, `cd "$W" \|\| { echo "ABORT no worktree $W"; exit 2; }`; the fixup passes `W="$W"`, and so do the battery's leg E and its bisect arms |
| SL-3 | tM-battery.sh: leg E's 1800-character stamp is cut before the verdict; a hand-owned file written by a conversion is neither stamped nor a finding | fixed | the stamp pattern drops `RUNTIME-MAP`, `HANDOWN`, `EMITCHECK VERDICT`; a second stamp `E verdict:` carries the verdict line and the three named readings; `finding "E: a hand-owned file written by a conversion, or no verdict"` unless the verdict reads `handown-written=0` |
| SL-4 | launch-refusal pattern written twice with different alternatives | fixed | `*/wf/draft*` in tM-fixup.sh and tM-battery.sh (and in tM-assemble.sh, which had no refusal: HF-17) |
| SL-5 | tM-helpers.py: `seat_shas` is stricter than the drivers' `^ref\|` row pattern, so a row could be merged and silently missing from precheck | fixed | precheck counts the rows the drivers' pattern takes and adds `FAIL SEATS` when the two counts differ |
| SL-6 | tM-linux-legs.sh: the x/tools v0.42.0 pin lost the i7 copy's checks | fixed | the pin branch also requires `v0.42.0.mod`; `had`, `zh`, `mh` are verified (`1:h1:*:h1:*`, one v0.42.0 line) and a failed pin stamps `LM:xmod: NOT MEASURED -- the pin failed` and runs nothing |
| SL-7 | literals written twice: (a) the base, (b) `EXEC_ROWS_EXPECT`, (c) the seven project names, (d) the 2m default | fixed (a, c, d); documented (b) | (a) `MASTER=${MASTER:-aa0a07d5fd}` and a NOTE when it resolves elsewhere; (c) `MGUARDS="$M7DIRS ..."`; (d) the battery passes `REALMOD_TEST_TIMEOUT` on only when COORD set it; (b) tM-README.md's variable table says both launches take it together, with `EXEC_RULED` in tM-helpers.py (LC-15) |
| SL-8 | tM-assemble.sh: the shape assert and the NEXT line do not know about follow-up merges | fixed | the first-parent line is walked: single-parent commits are refused, merges of no listed seat are counted (`nfu`) and the assert is `fp == NSEATS + nfu`; NEXT names the follow-up step and prints the assembled head as a fact, not as `EXPECT_HEAD` |
| SL-9 | comments tied to the 15-row list ("regression at 15 rows", "NO M seat touches src/tools") | fixed in part; COORD at the freeze | the NG leg now STAMPS the union's own count of changed paths under `src/tools` and its comments say what the stamp means, so the statement is measured, not carried. The remaining "15 rows at draft time" comments (headers of the battery, the fixup, the assembly; both briefs) are true of the draft and are COORD's to re-word when the list is frozen |

### 9.2 Leg coverage

| # | Finding | Disposition | What changed |
|---|---|---|---|
| LC-1 | the two BANK legs cannot red on a moved count | fixed | in `tleg`'s bank branch: `finding` unless the reading file holds `=> BANK-ELIGIBLE YES`, and unless it holds `=> AT BANKED COUNTS`; ` ENV ` joins the stamp pattern |
| LC-2 | XS/XM can read NOT MEASURED with a green battery | fixed | after the MOD stamps: `finding "MOD real modules: n of 2 REALMOD-VERDICT lines"` unless `REALMOD` is not 1. Confirmed on this box with `[ -e ]`: x/tools v0.41.0 `.zip` and `.mod` absent, v0.42.0 `.zip`, `.mod`, `.ziphash` present |
| LC-3 | `zip=build` swallows any new failure in the package | fixed | `realmod` takes `build[:Prefix]`: a differing undisclosed name outside the prefix is FAIL (plain `build` allows none); both drivers pass `zip=build:TestVCS` (one `func TestVCS` in the cached source) |
| LC-4 | linux: a stale `.named-<row>` from an earlier run makes `disclosed()` stamp DISCLOSED | fixed | `tleg` removes `.named-`, `.summ-` and the two kept record copies of the row before it runs; the brief tells a re-run to take a fresh `R` |
| LC-5 | F8's union gate (csproj among the rewrites) is a stamp | fixed | findings `HS: n csproj among the sweep's tracked rewrites` and `HS-T: ...` (HF-16 is the same finding) |
| LC-6 | F4's verdict cannot fail once `-test-timeout` exceeds a first publish | fixed | `realread` gives the F4 verdict only at `2m` / `2m0s` / `120s`; any other value stamps `NOT MEASURED`; the linux brief says the same |
| LC-7 | SIc and NVR can fail to fire without reaching either end list | fixed | `finding "SIc ..."` unless the control fired its six cycles; `finding "NVR refusal arm did not fire"` |
| LC-8 | linux: no by-class reading (DarwinStdDescriptorContractTests exists only there) | fixed | `gt()` writes a TRX and reads it with `tM-helpers.py trx` over every class of `GTL`; it stamps the NOT FOUND list and that class's outcome |
| LC-9 | GN, TR and 2b gate on rc alone | fixed | GN: finding unless `Failed: 0 ... Total: 68`; TR: finding unless 26/26, and a finding when its build fails; 2b: finding on any CS8032/8034/8784/8785 line. 68 and 26 are the `[TestMethod]` lines at the pre-map union (no data-driven test in either) |
| LC-10 | TL-WALL keeps the row-wall comparison TRAIN L's lesson retired | fixed, differently | NEW `tM-helpers.py hostwall`: the package-level terminal events of the fresh `go2cs_test_results.json` carry the host's `elapsed`; the reading is the LARGEST, because a host that leaves through its exit path appends a second one with elapsed 0 (L's three kept records, selected the same way in `wf/notes/_r1-hostwall-probe.py`: 377 s, 5648 s, 6 s beside legs of 627 s, 5891 s, 317 s). It is appended to every S row's stamp and stamped for each T leg beside L's kept record of the same leg. The results file is NOT copied per row (the stamp is the baseline; about 93 files of up to a megabyte were not worth keeping). TL-WALL's list stays, labelled as row wall |
| LC-11 | no UF reading on P2 | fixed | one UF stamp before END for both lanes; the P2 post line carries `UF <n>` |
| LC-12 | the i9 shard asserts neither its completeness nor a failure | fixed | the three helpers read `/dev/null`; `abort` unless the loop swept `NROWS` rows (after the purge); `exit 4` when a row failed; the two config rows' ENV line ends `ENV-OK` or `ENV-MISMATCH` |
| LC-13 | leg S's comment claims encoding/json is read here through the canaries | fixed | the comment states nine rows here and eight on the i9, and that encoding/json is an i9 row |
| LC-14 | FX cannot tell whether the ten fixtures joined the guard | fixed | finding unless tracked > 1281 (L's reading) and tracked == current |
| LC-15 | WHICH row keeps the execution config is asserted hours in | fixed | precheck: `EXEC_RULED = [('log/slog', 'release-tiered')]`, hard FAIL otherwise (read with the helper's own regex: base three rows, pre-map union log/slog alone); battery: after the S loop, the rows whose PASS line carries `[release-tiered]` must be exactly `TIERED_ROWS` (the tag format is L's: one such line, net/http, in its 103 sweep logs) |
| LC-16 | the read-only staging path is exercised only if the copy keeps its read-only files | fixed (i7) | verdict `XS-readonly-copy`: read-only files in the copy == files, and > 0 (the cache's 19 files are all read-only, measured with `find`). The linux copy keeps its stamp: both linux boxes are uid 0, where the mode is not enforced anyway |
| LC-17 | a red leg 4 costs the run its MOD reading | fixed | tracked changes after CNR are saved and restored (`restore_paths CNR`) before the purge |
| LC-18 | a deadline stop calls TBS-W's filtered record bank material | fixed | `TBS_IN_TREE` is 1 between TBS-W and the end of T:runtime, and `deadline_check` says what src/core/runtime holds |
| LC-19 | P2's runtime budget (150 m per child) against L's own 9313 s reading | COORD | the driver takes `RT_CHILD` / `RT_OUTER` (defaults unchanged); the value needs P2's TRAIN L record, which no file readable here holds (README question 15) |

### 9.3 Hazards and the safety floor

| # | Finding | Disposition | What changed |
|---|---|---|---|
| HF-1 | no gate for floor 1's signature (an unresolved deferred marker) on the path that copies emission into the corpus | fixed, differently | tM-emitcheck.sh scans every file a conversion WROTE for the two marker prefixes (as bytes), prints `markers=N` on the EMIT line, keeps `markers-<arm>-<os>.txt` and fails the emission on any hit; tM-regen-apply.py refuses a path whose emitted bytes hold one. `emitdrift.py` was NOT changed (the proposal added a fourth number to it): it stays byte-identical to L's |
| HF-2 | the lock is TRAIN M-private, so floor 1 is enforced only among M's own scripts | fixed | NEW `tM-helpers.py live <own lock>`: other trains' `.battery.lock` folders, a converter/harness process census by name (twice, 20 s apart; listed, never killed), and a run log with no DONE / ABORT / STOP stamp beside a file written in the last 15 minutes. Called before the lock by the fixup, the battery and a standalone module-legs run; `LIVE_ACK=1` runs past a hit and is stamped |
| HF-3 | a whole-file copy of the "drifts in both" class carries the base's standing drift | fixed | tM-regen-apply.py reads `dm-<os>` (the base arm's drift list, kept in the scratch) and refuses a path the base arm also drifts on; a missing `dm-<os>` is incomplete evidence (exit 2) |
| HF-4 | BOTH reads every seat against the FINAL tree (a ruled follow-up editing a seat's line reads "lost"); no upper bound for a duplicated line | fixed, differently | BOTH is now PER MERGE: for every two-parent commit on the first-parent line, each path both sides changed is read in that merge's OWN tree, with the lower bound, the removal bound and the upper bound (`duplicated`). A follow-up's edit is one side's change at its own merge, so no owner rule is needed, and later seats' additions cannot trip the upper bound. A listed row with no merge of its own is a FAIL. Read commits-only: 15 merges, 32 pairs, 0 lost / back / duplicated at the pre-map union; 15 rows without a merge at the base |
| HF-5 | the conflict-marker arm scans `src` only | fixed | `git grep` over the whole tree, pattern `^(<<<<<<<\|>>>>>>>)( \|$)`; 0 hits at the pre-map union |
| HF-6 | the precheck arms have no control (floor 13) | fixed (as a manual step) | tM-README.md M9b: two read-only precheck controls with PREDICTED readings adapted to the per-merge BOTH arm, and one control for the LIVE gate. Not run by this round |
| HF-7 | the signed commit message states things the fixup did not measure | fixed | (4) prints only when files were applied, with the measured hunk class (`regen-class.log`) and numstat, and names the seats as EXPECTED owners; (5) prints by `GN` and the hunk-class rc; the battery sentence prints only under `REGEN=apply GOLDENS=regen`, worded `PREDICTED, not measured here` |
| HF-8 | a branch tip past its seated sha is one NOTE and the stale sha is merged | fixed | a table problem unless `TIP_MOVED_OK` names the ref (all 15 tips equal their seated sha today) |
| HF-9 | every merge failure reads "UNRULED conflict in []" | fixed | the fixup's signing preflight before anything is created; the merge's output is kept and the failure is named: refused before it ran, clean but uncommitted, or a conflict |
| HF-10 | nothing ties the assembled union to the tree the by-name lists were read on; the map union is held only by the map worktree | fixed | the assembly prints `MAP TREE: EQUAL / DIFFERENT / not in the object store` against `MAP_UNION`; the pre-map keeps each map union under `refs/coord/tM-map/<sha10>` (local; one ref per union, so a second re-run does not drop the first) |
| HF-11 | a map row prints "clean" with no shared-path count; BOTH covers seat-vs-master only | fixed | the pre-map prints each clean row's shared paths with the union so far; the per-merge BOTH arm (HF-4) covers seat-vs-earlier-seat; README M1 says every non-registry shared path of a late row is read |
| HF-12 | package_info.cs changes with no stdlib-metadata check before the signed commit | fixed, more widely | `TestStdLibMetadataInSync` joins `CB_TESTS` (20 names). The fixup reads it BY NAME at PRERES on the merged tree (the union already changes 30 `package_info.cs` and no metadata file, so the proposal's condition "only when step 4 copied one" would have let a seat-made staleness through to the signed commit) and again after step 4 when it copied a `package_info.cs` |
| HF-13 | README: no landing step; L:76's correction and L:77's reading note absent; the push comes after the battery | fixed (documentation); COORD | M11 (landing: precheck against the merged master, the roster guard), M12 (the mis-rooted log/slog reason), the reading note for a `TestSetDefault` red, and the push moved before the battery. The assembly stamps a NOTE when `origin/master` is past the base. Question 16 states the tension the earlier push creates |
| HF-14 | tM-follow.txt's example asks a lane to push onto a seated branch | fixed | the example is a branch of its own (`p1-m-repros-move`), and the file says so; README M2 moved to step 3 |
| HF-15 | the fixup checks GOROOT in the vacuous form | fixed | the battery's block (value checked before the export, VERSION, the toolchain's own spelling, `go version`), with `die` |
| HF-16 | csproj among the rewrites has no finding | fixed | = LC-5 |
| HF-17 | the assembly reads COORD's live notes file three times | fixed | `SEATS` is the run folder's copy; the rows are read once (`ROWS`) and all three loops use the snapshot; launch refusal added |
| HF-18 | an undeclared stack and one patch under two shas pass the table check | fixed | a row that descends from an earlier row needs a `stack-on` note that covers it (the named row, or a row that itself descends from it); `git patch-id --stable` over each row's own commits refuses one patch under two shas. The 15 rows read 7 descents, all covered, and 47 commits = 47 patch-ids |
| HF-19 | `REGEN_ALLOW` is a directory prefix; the hand-own prefix `src/core/testing/` covers converted sub-packages | fixed by judgment; COORD to confirm | **the finding's text reached this round cut off mid-sentence, so its proposed fix was not read.** `REGEN_ALLOW` now ends `/[^/]+$` (the files directly in the three package folders; none of them holds a GOOS folder at aa0a07d5fd). The hand-own test for testing is the files DIRECTLY in `src/core/testing/` in tM-emitcheck.sh (HANDOWN) and tM-regen-apply.py: TRAIN L's windows arm wrote 13 files under testing's sub-packages and not `testing.cs` |

### 9.4 Changes no finding asked for (made while applying the above)

- `EXEC_RULED` is a constant of tM-helpers.py; `RT_CHILD` / `RT_OUTER` are knobs of tM-linux-legs.sh (LC-19).
- The fixup and the battery stamp only BOTH's summary line (`^ok +BOTH:`); the per-path ok lines (32 at the pre-map
  union) stay in the precheck log.
- The battery passes `W` to the emission check (the fixup's half is SL-2).
- tM-fixup.sh's header, tM-emitcheck.sh's header, tM-assemble.sh's header and the battery's LEG -> SEAT map each gained
  a VERIFY ROUND 1 paragraph.

### 9.5 Not done

- Nothing was run, so every new construct is unexercised: `live`, `hostwall`, the per-merge BOTH arm as written inside
  `precheck`, the marker scan, the assembly's new table checks, the TRX reading on linux, `XS-readonly-copy`.
- M9b's three controls and M9's two were not run (no draft script may be executed here).
- LC-19 (P2's runtime budget) and SL-9's remaining comments are COORD's.
- `tM-regen-apply.py` has no control (README M9b says why that is tolerable).
- The briefs still say "15 rows at draft time".

### 9.6 Self-check after round 1

| Check | Result |
|---|---|
| `bash -n` | OK: all nine `.sh` |
| `python -m py_compile` | OK: tM-helpers.py, tM-regen-apply.py, emitdrift.py (`__pycache__` removed) |
| CR bytes | 0 in every file |
| non-ASCII bytes | unchanged against DRAFT 1 in every file (tM-regen-apply.py stays ASCII: the marker prefixes are byte escapes) |
| byte-identical to DRAFT 1 | emitdrift.py (also == L's), tM-i9-te.sh, tM-ng-parse.ps1, the two lists, the two control patches |
| one incident, repaired | a shell heredoc was used once to edit tM-emitcheck.sh and collapsed its backslashes (a NUL and two raw bytes reached the file); the file was restored from `wf/notes/_r1-before/` and the edit redone with the file tool. The byte censuses above are after the repair |

Lines after round 1 (added / removed against DRAFT 1, by `diff`): tM-assemble.sh 173 (+97 / -13), tM-battery.sh 1083
(+127 / -31), tM-conflict-map.sh 46 (+17 / -2), tM-emitcheck.sh 153 (+18 / -7), tM-fixup.sh 495 (+90 / -24),
tM-follow.txt 13 (+4 / -2), tM-helpers.py 895 (the file as round 1 left it, `wc -l` of `wf/notes/_r2-before/tM-helpers.py`; this line said 891, a count taken before round 1's last edit), tM-i9-shard.sh 220 (+16 / -7), tM-linux-legs.sh 495
(+42 / -7), tM-modules-legs.sh 534 (+27 / -4), tM-regen-apply.py 105 (+23 / -5). The documents (tM-README.md, the two
briefs, controls/README.txt, this file) changed too.

### 9.7 Inputs that moved again

`trainL/tL-seats-draft.txt` held 81 lines when round 1 read it (last written 05:38). New since section 8:
- **L:80 (05:23)**: `g-lazy-callers` `483d4ea217` "rides M as the LAST row (stack-on g-godebug-pc-line) only if GREEN
  before M's battery launches". Not in the local object store. A golib seat: its class is in no by-name list.
- **L:76 (04:42)** and **L:77 (04:44)**: the log/slog reason is mis-rooted (README M12), and a `TestSetDefault` red in
  M's battery is read as the first-launch PDB class first (README section 5).
- Every announced row that descends from another row needs its `stack-on` note in the seat list, or the assembly's
  table check refuses it: `c1-route-sysctl` (on c1-darwin-linkname-pulls), `c1-darwin-inode64` (L:75: "stacked S7 ->
  S7b -> S10"), `g-lazy-callers` (on g-godebug-pc-line).

## 10. Verify round 2 (2026-10-02)

Three verifiers read the round-1 draft again through the same three lenses and returned 31 findings: **SL** stale
literals (7), **LC** leg coverage (14), **HF** hazards and the safety floor (10). COORD ruled on the drafter's open
questions and on the findings that needed a decision (`wf/notes/COORD-RULINGS-r2.md`); those rulings bind this round.
Each finding was re-read at the cited file and line and judged before any edit. **Nothing in the draft was executed.**
The commands of this round: git reads (`log`, `show`, `diff`, `ls-tree`, `merge-base`, `rev-parse`, `grep`,
`patch-id`), `date`, reads of TRAIN L's kept run-2 SUMMARY, `bash -n`, `python -m py_compile`, and read-only probes
kept in `wf/notes/`: `_r2-roster-probe.py` (the helper's three roster regexes over two saved blobs),
`_r2-table-probe.sh` (the seat table's descent arithmetic), `_r2-patchids-24.txt`, a re-run of round 1's
`_r1-both-permerge.py` at the new map union (`_r2-both-permerge-382ceeb838.txt`), and `_r2-lineindex.sh` (the README's
line index). The pre-round files are kept in `wf/notes/_r2-before/`.

**Inputs that moved under this round.** The rulings said 20 rows and map union `14dab07d87`. When the round read
`trainM/tM-seats-draft.txt` it held **24 rows** (06:31) and the map worktree stood at **`382ceeb838`** (L:82, 06:35:
"TRAIN M draft = 24 rows, pre-map 24/24 clean"). New since the 20: `c1-darwin-variadic` `c1887e5ce2` (stack-on
c1-darwin-inode64), `p2-host-test-list` `1715ced36c` (host finding H1), `p2-host-event-line-start` `932778788f` (H2),
`p1-warnings-tranche1` `4904135fda`. Every measurement of this section was read at `382ceeb838`.

Dispositions: **fixed** = real, and the draft changed as proposed; **fixed, differently** = real, repaired by another
mechanism (the reason is given); **ruled** = decided by COORD and applied as ruled; **rejected in part** = one part of
the proposed fix was not taken (the file evidence is given). No finding was rejected whole.

### 10.1 Ruling R1: no by-name list is a literal only COORD can keep current

| List (round 1) | Now | Where | Read at 382ceeb838 |
|---|---|---|---|
| `CB_TESTS`, 20 typed names | DERIVED: every `+func Test...(` in `git diff $MASTER HEAD -- src/go2cs`, plus the literal `CB_BASE` (six tree guards that exist at the base); packages `./...` | battery PRE-D (1), leg CB; linux `CM`, leg LCn | 73 added names in 21 test files, none with a `//go:build` line |
| `GT_CLASSES`, 24 typed classes; `GT_EXPECT`, 4 exact counts | DERIVED: classes of the `*Tests.cs` files the union adds or changes under `src/tests/GolibTests`, minus the files `GolibTests.csproj` removes on this box (read from the csproj's `Compile Remove` groups), plus the literal `GT_NEIGH`. `GT_EXPECT` = each ADDED class with a FLOOR (its uncommented `[TestMethod]` lines): off when NOT FOUND, a Failed / Error / Timeout / Aborted result, or fewer results than the floor | battery PRE-D (2), leg GT (`gtclassoff`); linux `GT_LIST` / `GTL` | 8 added, 2 changed; 16 files removed unless GoTargetOS is linux, 12 when set and not windows, none under another condition; `DarwinArm64VariadicSlotTests` has a linux-only arm (Inconclusive elsewhere), which is why the gate is a floor on results and not `Passed=N` |
| `M7DIRS`, 7 typed directories; `MGUARDS` | DERIVED: top-level behavioral directories with an added csproj; second derivation from the go2cs.slnx lines (a difference is a finding). `MGUARDS` = those + `FXGOLD` (the projects whose goldens the fixup commits changed: derived) + the literal `CHECKDEAD_GUARDS` | battery PRE-D (3), legs 4 and 5; linux `LB_LIST` | 7 by both derivations |
| `TC0_ROWS`, `TIERED_ROWS`, and `internal/godebug log/slog` in `X_ROWS` | DERIVED from the roster at `$MASTER` and at HEAD (`tM-helpers.py execrows`): annotated at HEAD = tiered; annotated at the base only = TC0. They join the S run list by themselves. `X_ROWS` holds `encoding/json` only (RULED) | battery PRE-D (4), leg S | base: internal/godebug, log/slog, net/http; union: log/slog |
| `EXEC_ROWS_EXPECT=1` in two scripts | default = the count of `tM-helpers.py` `EXEC_RULED` (`execruled`), the ONE site of the ruling | fixup, battery | |
| `E_BISECT_SEATS` | the literal `E_BISECT_KNOWN` kept (which seat's emission the tree lacks is what leg E measures), asserted against the DERIVED superset (seats whose merge changes a non-test Go source of the converter); the rest of the superset is stamped by name; a file no arm names is a finding | battery PRE-D (5), leg E-bisect | 12 converter seats |
| `REGEN_ALLOW`, a typed pattern | COMPOSED from the seat list: `CORPUS FOOTPRINT n file (<path>.cs ...)` in a row's notes admits that file; `REGEN_G_SEAT`, while a row, admits the files directly in `REGEN_G_PKGS`. Stamped in PRE and stated in the commit message. Never widened | fixup | composes to `^src/core/(go/internal/srcimporter/srcimporter\.cs|net/http/[^/]+|internal/reflectlite/[^/]+)$` |
| `MAP_UNION=8d7305053f` | DERIVED: `refs/coord/tM-map/rows-<rows-sha256>`, written by the map for the rows it read; the assembly hashes ITS rows and looks the ref up. `MAP_UNION=<sha>` overrides | map, assemble | rows-sha256 of the 24 rows: `e1ef84e97561` |
| the NGa legs' one script | DERIVED: every `Test-*.ps1` under `src/tools/nugetgo` at HEAD, each under both editions | battery leg NGa | 2 scripts: Identity and (c2-s2-source-metadata) SelfDescription, which round 1 only parsed |
| GN 68, CT 24, TR 26 (exact), FX > 1281, ST "62/0" (rc only) | FLOORS, each stamped with its source: Failed 0 AND Total >= the uncommented `[TestMethod]` lines at HEAD (`tmfloor`), which must not be below the measured 68 / 24 / 26; FX tracked >= L's own FX stamp (read from L's SUMMARY; 1281) and == current; ST 0 violations and checks >= 62 | battery `totalgate`, FX, ST | 68 / 24 / 26 lines |
| linux: `GTL` (17 classes), `CM` (19 names), the LB list (17 projects), `-test-tiered` on a typed row, expected counts in the brief only | DERIVED on the lane's box from the union it checked out; each row's execution config from the roster (`tiered_row`, `rowleg`); each row's expected N + D from its roster line's linux annotation (`tM-helpers.py rosterlinux`). Literals: `GT_NEIGH`, `LB_FIXED`, `OS_ROOT_EXPECT` | tM-linux-legs.sh | the annotations of the brief's rows equal the brief's counts (`_r2-roster-probe.py`) |
| i9: `wantt` on two typed rows | DERIVED from the roster at the base and at the union (`exrows`) | tM-i9-shard.sh | |
| H3's seat name | DERIVED: every row whose own commits edit `Goroutine.cs`; its identifier is the literal `H3_TOKENS`; a row with no entry FAILS by name | tM-helpers.py precheck | `g-godebug-pc-line` alone |
| H1 / H2 (singleflight, tlog) | READ from the seat list (a ref `p2-*test-list*` / `p2-*event-line*`, or a row noted H1 / H2) | tM-modules-legs.sh, tM-linux-legs.sh | both are rows of the 24 |

The literals that remain are listed, with the check each has, in `tM-README.md` M1 and printed by the assembly's
MAP TREE: DIFFERENT line.

### 10.2 COORD's answers, applied

| Ruling | What changed |
|---|---|
| P2's runtime row: 210m / 450m are the DEFAULTS | tM-linux-legs.sh `rowleg runtime "${RT_CHILD:-210m}" "${RT_OUTER:-450m}"`; both named in the brief (section 1 and the L6 row) and in the README table (= LC high, second finding) |
| `REGEN_ALLOW` holds only measured footprints; never widened automatically; the narrowing stands | 10.1; the die message names the pattern and its source and says a relaunch with `REGEN_ALLOW=` is COORD's explicit call. F1+F6's part is the exact FILE its row measures (the ruling's words), G's part the two package folders (the narrowing) |
| A second fixup: `fixup-2: TRAIN M`, ... on top; PRE admits the chain | battery PRE walks the single-parent commits at HEAD (`NFU`, 1 to 9), wants `fixup: TRAIN M` then `fixup-2`, ... in order, each signed, and counts them in the first-parent arithmetic; `UTOP` replaces `HEAD^`. tM-fixup.sh gains `FIXUP_N` (PRE accepts the chain below it; the subject is `fixup-N: TRAIN M`). precheck counts every fixup-subject single-parent commit on the first-parent line (round 1 read HEAD's subject alone, which also read 0 at the landing merge). README section 4b: the legs a fixup-N obliges COORD to re-read, by path |
| A stale `stdlib-metadata.txt` rides the fixup | `stdlibmeta`: a STALE asset is regenerated with the directive's own command (`go run ./internal/genstdlibmeta`; `go generate .` would also run the symbols generator), must change exactly that file and PASS its guard by name; `meta-files.txt` joins the expected set; the commit message states it (line (0)). A red that is not the STALE message still dies |
| `SIBLINGS=report` is the default | tM-fixup.sh; README M5 |
| LIVE gate: dotnet.exe / testhost.exe only under the tM worktree or the scratch root | `live`: those two names count only when the command line or the executable path holds one of the two paths (both slash spellings); `go2cs.exe`, `go2cs.test.exe`, `BehavioralRunner.exe` still count anywhere (README question 19) |
| GN and FX: floors | 10.1 |
| encoding/json in `X_ROWS` | `X_ROWS='encoding/json'` |
| M12 rides the fixup; COORD writes the words; a named manual step | tM-fixup.sh step 0b: `PROSE_PATCH=<file>` is applied and checked (only the two paths; `_roster.ps1` changed lines are `#` comment lines and ASCII; roster guard x2 with the same count); `prose-files.txt` joins the expected set; commit message line (0b). README M12 says how to cut the patch |
| HOSTWALL: the stamp is enough | no change (round 1 already kept no result file per S row); the README says so |

### 10.3 The 31 findings

| # | Finding | Disposition | What changed |
|---|---|---|---|
| SL-1 | the follow list and the seat list are per-run-folder copies; M2 does not say which copy; nothing stamps the bytes a run read | fixed | README step 3 and M2 ("edit the hnd file before any copy, or one run folder"); `lists=rows:<h>/follow:<h>` on the fixup's PRE and FIXUP DONE lines and the battery's PRE line; the battery's and the fixup's "not a listed seat" messages name the remedy; tM-follow.txt's header says the same |
| SL-2 | two sources for one seat list; `MAP_UNION` goes stale by design | fixed, differently (R1) | the map takes `SEATS`, prints `rows-sha256`, and keeps a fully clean union under a rows-keyed ref; the assembly DERIVES its map union from that ref and prints the same hash on TABLE OK. A required `MAP_UNION` (the proposal) would have been a literal COORD retypes on every re-run |
| SL-3 | the fixup's switch check accepts any value beside a `skip` | fixed | four independent `case` checks (and `FIXUP_N`, `PROSE_PATCH`) |
| SL-4 | a launch refused on the lock leaves a SUMMARY with no ABORT word | fixed | the refusal is `stamp "ABORT: ..."` |
| SL-5 | one ruling (the execution-config rows) written in six places | fixed, differently (R1) | reduced to ONE site, `EXEC_RULED`; the battery's two lists, the linux row and the i9's `wantt` are derived from the roster, `EXEC_ROWS_EXPECT` defaults to the ruled count. The comment above `X_ROWS` and README M1 name the site |
| SL-6 | stale numbers in the documents | fixed | section 9.6's 891 -> 895; this file's header says no line number of sections 1 to 9 is current and where the index is; the i9 brief's "fourteen seats" reworded; the README rewritten with a generated index |
| SL-7 | every by-NAME list is a literal read at the 15-row map union | fixed, differently (R1) | 10.1; README M1 holds the checklist of the literals that remain; the assembly's DIFFERENT line prints it |
| LC-1 (high) | freshness by mtime, but the comparison record is rewritten only when its bytes change and survives every purge | ruled; fixed | `recclean` deletes the row's three gitignored record files before every S row, every NR repeat and inside `tleg`; the three deadlock gates want `total=0 .*record=fresh`; an S row that exits 0 with no fresh record is a finding (TRAIN L's run 2: 94 of 94 first readings fresh); a T leg that leaves no record is a finding; `tbs_named` wants a fresh comparison record first; tM-i9-shard.sh deletes before each row and each infra rerun. Verified at the source: `writeJSONFile` -> `needToWriteFile` (testConversion.go:6557-6563 at the map union), `src/core/.gitignore` lists the three files, L's run 2 reads `record=STALE` on NR:2 to NR:5 |
| LC-2 (high) | P2's runtime row keeps 150m per child | ruled; fixed | 10.2 |
| LC-3 | round 1's restore after CNR is undone by leg 5 | ruled; fixed | `restore_paths BEH` after the isolated legs (its CR-only pass is the handling TRAIN L's postmerge2 `restore_all` uses, compared at `coord-scratch/tL/postmerge2/tL-postmerge2.sh:56-61`); immediately before MOD: restore again if needed, stamp, `PRE-MOD tracked changes: n (ASSERT 0)`, a finding when not 0 |
| LC-4 | GT, CT, GN, TR have no time limit | ruled; fixed; **rejected in part** | all four run under `capped` (45m per GT run, 10m, 20m, 20m); `timeout_stop` lists by PID, name, executable path and command line. NOT taken: `--blame-hang-timeout`. The ruling names the wall cap and its consequence (lock kept, exit 5), and a blame collector on the i7 would change the instrument against TRAIN L's readings of the same legs (README section 6) |
| LC-5 | the by-name lists do not cover a late row | fixed, differently (R1) | derivation instead of three completeness findings; CB's package list is `./...`; mirrored in tM-linux-legs.sh |
| LC-6 | DEADLINE cannot protect a battery that crosses midnight | ruled; fixed | `'YYYY-MM-DD HH:MM'`, resolved once to `DL_EPOCH`, a past time refused, default launch + 14 h, stamped with its source; the battery passes `DL_EPOCH` to MOD; tM-modules-legs.sh resolves its own when standalone. The `none` value is gone |
| LC-7 | the END legend pre-excuses TBS-W; `/trap` is never gated | ruled; fixed | legend rewritten; a non-zero TBS-W is a finding (L's run 2: `LEG T:runtime:tbs rc=0`); `tbs_named` gates `/trap` (L's run 2 read it disclosed in both legs) |
| LC-8 | XS runs singleflight with H1 unseated; nothing checks its processes are gone | ruled; fixed | 10.1 (the seat list decides; with no H1 row the package folder is removed from the module COPY before the hash and the run, stamped `NOT RUN (KNOWN: H1, child fan-out)`; with the row, `singleflight=12`). The census by executable path under the out root runs in tM-modules-legs.sh's END (verdict `MOD-orphans`, lock kept) and again in the battery after MOD (a finding, lock kept, the battery goes on: the ruling says "a finding and keeps the lock", not a stop). An unreadable census is a finding. Extension by the same rule, to confirm (README question 21): `sumdb/tlog=17` when the H2 seat is a row |
| LC-9 | the linux driver cannot fail | ruled; fixed | `mover()` + `MOVERS`: a leg's rc; N + D against the row's roster reading (`EXPECT_V`); no fresh record; `PASS_NOW` names (TestRegisterErr, TestCmdBisect); `disclosed()`; a GolibTests failed name, no `Failed: 0` line, a class NOT FOUND, no TRX; LCn; a REALMOD FAIL line or no verdict; a literal the tree does not hold. END prints `movers=`; exit 4. Deadlock lines, LB's phases, LPB and UF stay stamps (not in the ruling's list; the brief says so) |
| LC-10 (low) | the linux brief's csproj exception swallows F8's change on the rows P2 reads | fixed | section 3 bounds the exception by content and asks for the diff against the lane's TRAIN L patch |
| LC-11 (low) | i9: the freshness marker is missing after an infra rerun; soft expectations never reach the exit code | fixed | the marker is the row's (`sweep-$n.log.t0`); the ENV verdict goes to `<suf>.named.txt`; `soft` (ENV-MISMATCH, a config row with no record, HS, UF, csproj) joins the exit-4 condition; the GoDefaultGodebug count is compared with the base's own |
| LC-12 (low) | ST and PB say EXPECT and gate on rc | fixed, differently (R1) | ST: 0 violations and checks >= 62 (a floor, not `: 62 checks`); PB: the count is compared with the roster through `rosterrow` (cmp banked 4), not with a typed 4 |
| LC-13 (low) | four bank / roster items of M-requirements have no leg | fixed, differently | precheck: ROSTER linux annotations (base vs merged), SNAPSHOT (any path under a `docs/validation/<release>/` folder), PROOF (derived: every page under `docs/validation/current` a seat's own commits changed is, at HEAD, the blob of the LAST row that changed it; not two typed page names); module legs END fails on any status line under `docs/`. At 382ceeb838: 223 == 223, 0 paths, 2 pages equal to G's blobs |
| LC-14 (low) | G's GOTRACEBACK=system control has no leg | ruled out in the README (section 6), and asked (question 20) | the control's command is not in any commit message of G's chain (searched `c2591d5b95..0af55d033e`); it is not inferred |
| HF-1 | = LC-3 | ruled; fixed | LC-3; README section 5 says where the restores are |
| HF-2 | the emission check has no floor 1 / 4 / 12 guard of its own | ruled; fixed | without `TM_LOCK_HELD=1` (both callers pass it): the launch refusal, the LIVE gate, the lock (released by its EXIT trap), 30G / 8G |
| HF-3 (low) | a standalone module-legs launch lacks the floor-4 refusal | ruled; fixed | the same `case "$SD"` (its lock, LIVE gate and per-leg disk preflight were already there) |
| HF-4 | the seat table check has no control (floor 13) | ruled; fixed | `TABLE_ONLY=1`; README M9c: three planted lists (a duplicated sha, a stacked row above its base, a row already on the base), each with the line it must print; the regen refusal control inside fixup step 4 (allow `^$` must refuse every listed path and write nothing) |
| HF-5 | the stale G frame class in committed test sources has no step | ruled: a deferral | README M13; `teattr` prints `g-frame-only files=<n>`; TE stays a reading. Nothing lands in M's fixup |
| HF-6 (low) | M11 uses the plain seat list and does not bound what the master merge brings | fixed | README M11: `seats-effective.txt`; the diffstat rule for paths under `src/` |
| HF-7 (low) | the tip check reads a local tracking ref | ruled; fixed | `git ls-remote origin 'refs/heads/claude/*'`, once; a missing branch reads `none` and is refused; `TIP_MOVED_OK` acknowledges only a remote tip that descends from the seated sha. No prune |
| HF-8 (low) | four table and shape gaps | fixed | (1) a row already an ancestor of the base is a table problem; (2) master's patch-ids since each row's merge-base join the duplicate check; (3) the seats' merges must sit on the first-parent line in row order; (4) the MAP TREE line is written to `assemble-maptree.txt` and stamped by the fixup's PRE |
| HF-9 (low) | floor 9: one fixup admitted, the union pushed before the battery | ruled; fixed | 10.2 (`fixup-N`) |
| HF-10 (low) | the battery's exit status is always 0 | ruled; fixed | exit 6 when a leg outside FXc1 / FXc2 / SIc / NVR is non-zero or any finding exists; `finding()` counts |

### 10.4 Changes no finding asked for (made while applying the above)

- The NGa legs run every `Test-*.ps1` under `src/tools/nugetgo` (10.1): `Test-NugetgoSelfDescription.ps1`, added by a
  row whose note says "WINDOWS acceptance = the battery's nugetgo legs", was parsed and never run. Its header states the
  same contract as the identity script (no parameters, a fabricated root in the temp directory, `ran N, failed M`).
- tM-conflict-map.sh counts a missing sha, an "already contained" row and a conflict as NOT-CLEAN and then writes no
  rows ref; it reads its rows CR-stripped.
- `teattr`'s TE line format changed (one more field); `controls/README.txt`'s two PREDICTED lines follow it.
- The fixup's commit subject names only the classes it committed.
- tM-battery.sh: the comments that described 15 rows are kept as descriptions and say so; the lists beside them are
  PRE-D's.
- Both briefs: 24 rows; the two host seats and the three later golib seats; the driver's exit status; the DERIVED lines.

### 10.5 Not done, and unexercised

- Nothing was run. New and unexercised in this round: battery PRE-D, `recclean`, `totalgate`, `gtclassoff`, `orphans`,
  the fixup chain walk in the battery and the fixup, the exit-6 path; fixup step 0b, the metadata regeneration, the
  regen control, `FIXUP_N`; the assembly's ls-remote reading, master patch-ids, ORDER assert, `TABLE_ONLY`; the map's
  rows ref; `execrows`, `execruled`, `rosterlinux`, the three new precheck arms, the derived H3 arm, the narrowed
  `live` query; the emission check's hand-launch guards; the module legs' seat reading, folder removal and census; the
  linux driver's derivations, `rowleg` and gates; the i9 shard's roster reading and soft gates.
- M9, M9b and M9c's controls were not run (no draft script may be executed here).
- Whether `stdlib-metadata.txt` IS stale at the union is not measured (30 `package_info.cs` change, the asset does
  not; the guard decides).
- The 24-row map was run by the OLDER `hnd` copy of the map script, so no rows ref exists for it: step 0 of the README.
- Sections 1 to 8 of this file were not rewritten (they are DRAFT 1's record); their line numbers are not current.

### 10.6 Self-check after round 2

| Check | Result |
|---|---|
| `bash -n` | OK: all nine `.sh` |
| `python -m py_compile` | OK: tM-helpers.py, tM-regen-apply.py, emitdrift.py (`__pycache__` removed) |
| CR bytes | 0 in every file |
| non-ASCII lines | unchanged against round 1 in every script (tM-battery.sh 3, tM-modules-legs.sh 2, tM-helpers.py 2, the rest 0) |
| byte-identical to round 1 | emitdrift.py, tM-regen-apply.py, tM-i9-te.sh, tM-ng-parse.ps1, the two lists, the two control patches |
| editing tool | every draft file was edited with the file tool. One shell command that carried a long inline script was cut by the shell and ran NOTHING (no file changed); the edit was redone with the file tool. The README's line index was inserted from a generated file by a four-line `python -c` |

Lines after round 2 (added / removed against round 1, by `diff` with `wf/notes/_r2-before/`): tM-assemble.sh 230
(+74 / -17), tM-battery.sh 1377 (+376 / -82), tM-conflict-map.sh 58 (+19 / -7), tM-emitcheck.sh 183 (+30 / -0),
tM-fixup.sh 646 (+187 / -36), tM-follow.txt 17 (+4 / -0), tM-helpers.py 993 (+118 / -20), tM-i9-shard.sh 261
(+50 / -9), tM-linux-legs.sh 607 (+135 / -23), tM-modules-legs.sh 604 (+82 / -12), tM-lane-brief-i9.md 171 (+19 / -2),
tM-lane-brief-linux.md 291 (+40 / -17), controls/README.txt 39 (+7 / -2). tM-README.md was rewritten (533 lines).

## 11. Verify round 3 (2026-10-02, 08:13 to 09:00)

Inputs: `wf/notes/COORD-RULINGS-r3.md` (binding), `COORD-RULINGS-r2.md` (still binding), `_r3-findings.json` (16
findings, three lenses), `_r2-fix-open.json`. **The seat list is frozen**: 24 rows, row 18 =
`p2-test-overload-references|e002a552a8`, `rows-sha256=0b3fd4358320`. While this round ran, COORD mapped that list
with the round-2 copy of the map script (`MAP DONE head=11c188daf3 rows=24`, rows ref written) and assembled it
(head d4aae0aca0, `MAP TREE: EQUAL`): `coord-scratch/tM/asm1/`. Nothing of the draft was run by this round except,
by mistake, the helper's `execrows` subcommand (three times, against `H:\Projects\go2cs`; it runs two `git show`
reads and writes nothing). Every other command was a git read, `bash -n`, `python -m py_compile`, `gofmt -e`, or a
read-only probe kept under `wf/notes/` (`_r3fix/`, `_r3-both-permerge-*.txt`, `_r3-lineindex.sh`).

### 11.1 The three HIGH findings

| # | Finding | Real? | What changed |
|---|---|---|---|
| 1 | LX `tiered_row()` greps `execution: *release-tiered` over the whole roster row and so matches PROSE: net/http and internal/godebug would be run tiered, at the config G's seat retired, and the summary would agree with itself | **Real.** Measured at the frozen map union 11c188daf3: the round-2 pattern matches internal/godebug, log/slog and net/http; the helper's anchored `EXEC` matches log/slog alone (the net/http row's notes say "It carried `execution: release-tiered` from Go 1.23.12 until 2026-10-01") | RULED: ONE reader. LX calls `tM-helpers.py execrows` once at the union and once at the base, aborts when either does not read, aborts when the base's list lacks log/slog (the reader's control), stamps `DERIVED execution-config rows ...` with both lists, raises a MOVER for an execution value it has no switch for, and `tiered_row` tests membership of the derived list. The brief names the annotation field and the expected stamp |
| 2 | I9 `exrows()` uses the same unanchored grep: internal/godebug reads "still tiered at the union", the row runs correctly at TC0, and the driver posts ENV-MISMATCH and exits 4 on every run | **Real** (same measurement) | RULED: the same reader. I9 now REQUIRES `tM-helpers.py` beside it (a fourth file of the lane's run folder; the brief's step 2 and the header say so) and `exrows <ref>` calls `execrows` at the base and at HEAD, keeps `execrows-<ref>.txt` beside the logs, aborts when the helper does not read, when the base lacks log/slog, or when the union carries an execution value other than release-tiered. The PRE line states the expected lists. Chosen over the verifier's byte-anchored grep because the ruling asks for one reader and names a second copy of the parse only as the fallback; the cost is that the i9 needs python 3.8+ (NOT MEASURED; the brief asks the lane to check) |
| 3 | ML MR4c / MR6c: control converters built from TRAIN L seat shas run against the UNION's testing host, which with H2 seated frames every `--json` line with U+0016; a pre-H2 comparer reads zero C# verdicts, `cmpnames` reads `cs=None`, both controls FAIL, MOD is non-zero, the battery exits 6 | **Real.** Read with git: 932778788f changes `TestReporter.cs` (`FramedJson`, unconditional in json mode) and `testConversion.go` (`testStreamLines`); neither c5e934c464 nor e9009f2945 holds `testStreamLines`, and their `terminalTestResults` does `json.Unmarshal` on the raw line and skips a failure | RULED: "build the control converter WITH the union's comparer change ... do not make a control pass by loosening its reader". The verifier's fix (read the C# side from the host's results record in `cmpnames`) is NOT applied: it loosens the reader. Instead `ctlconv` applies the H2 seat's OWN diff of `src/go2cs/testConversion.go` (against its merge-base with the base: +31 / -4, the stream reader and its call sites) to the archived control source before `go build`, when the H2 seat is a row of the seat list. `cmpnames` is byte-identical. The H1 / H2 seat derivation moved up (it is needed before MR4) and gained `H2SHA`; a new `BASE=aa0a07d5fd`. A patch that does not apply or a converter that does not build reads NOT MEASURED and is counted, as a build failure already was. Probe (`wf/notes/_r3fix/`): the patch applies with `git apply -` to the `git archive` copy of the file at both shas, also under `MSYS_NO_PATHCONV=1`, and both results parse. NOT measured: that the patched converters build. The other route COORD named (a `-go2cspath` tree at the control's own sha) was not taken: two more 490 MB source trees and two cold stdlib-closure builds under the old converter's 20 m publish deadline, untimed |

### 11.2 The other thirteen findings

| # | File | Finding | Disposition |
|---|---|---|---|
| 4, 8 | LX | "the driver cannot fail" is partial: a failed GolibTests build skips the run; LB's build rc, each LB project's rc and LB NOT MEASURED are stamps; LM NOT MEASURED and F4 are stamps; LPB, UF and the TBS deadlock count are stamps | **Fixed, all four parts** (ruling 4). `if gtbuild; then gt; else mover`; LB-build rc, each project's rc (the ONE stated exception is `SetegidBroadcastSeam`, whose Target residual the brief expects: its rc is stamped), and `LB NOT MEASURED` are movers; three `LM ... NOT MEASURED` movers and an F4 gate in `lmread` at the 2m default (stamped NOT MEASURED at another timeout); LPB's three expectations and UF (after the rows and at END) are movers; a deadlock line in a TBS iteration's log or fresh record is a mover (an iteration with no record is already `tleg`'s). The brief's section 3 lists them and drops "are still stamps" |
| 5 | brief-linux :92, ML :456 | the stale log/slog reason; a cause asserted for semaphore | **Fixed** (ruling 5). The brief gives the ruled reason. Semaphore is "the known timing-class non-pass (TestWeightedAcquire)" with "cause not measured" in ML, B, LX, the brief and the README; P2's own label "TC0 timing" is kept only where it is quoted |
| 6, 14 | README, F and B headers | the predictions describe the 06:31 list: `MAP_UNION=382ceeb838`, M9b.1 "exactly one FAIL, H4", M2's follow-up, the old rows hash | **Fixed** (ruling 11): see 11.4 |
| 7 | CM | the live list is read twice (the merged rows, then the hash) | **Fixed** (ruling 7): the rows are snapshotted to `coord-scratch/tM/map-rows-<hash>.txt` before the fetch; the merges read the snapshot; the hash is the snapshot's, checked once more against the file |
| 9 | LX | the by-class TRX gate passes when its reader throws | **Fixed**: the reader's rc is read, and no `TRX totals` line (or rc != 0) is a MOVER |
| 10 | I9 | a config row's ENV verdict is gated negatively | **Fixed**: after the named reading, a config row with neither ` ENV-OK` nor ` ENV-MISMATCH` in its file gets a `NO-RECORD` line, which the existing end-of-shard grep turns into `soft=1` (the pattern has no end anchor: python on windows writes CRLF) |
| 11 | B | `FXGOLD` takes a file name of the behavioral root as a project | **Fixed** (ruling 9, stricter than the proposed fix): a candidate must be a directory AND hold a csproj at HEAD (`git ls-tree`); a changed directory with no csproj is stamped in PRE-D and not read as a project |
| 12, 15 | B, F | `seats-effective.txt` is built two ways; a list without a final newline welds the first follow-up row onto a comment | **Fixed** (ruling 8): both scripts write the row lines only, CR-stripped, then the follow-up rows in one format, and each asserts the row count (seats + follow-ups) |
| 13 | A | pass by absence: with no rows ref the assembly prints a NOTE and merges a list no map has read | **Fixed** (ruling 6). No rows ref and no `MAP_UNION` is `ABORT` before the worktree, with the remedy named; a `MAP_UNION` that is not a commit is refused there too. The verifier's `NO_MAP_OK=1` escape is NOT added: the ruling allows the rows ref or an explicit `MAP_UNION`, nothing else. A MAP TREE that is not EQUAL is exit 4 after the shape lines (the tree compared is the one at the newest seat merge, so a resume after a ruled follow-up still compares the seats-only tree). README M9c gains a control for the refusal |
| 16 | F | the signed commit message states seat-specific facts as fixed text | **Fixed** (ruling 10): the precheck paragraph prints this run's own step-6 lines (`PRECHECK ...`, the COUNT and REG counts, the `BOTH:` summary, the roster's BOTH line, ROSTER, SNAPSHOT, PROOF, H3, H4), the roster guard's measured count and outparity's first line. The owner sentence of step 5 prints only when `REGEN_G_SEAT` is a row, with its base from `git merge-base` (c2591d5b95 today, read from git) |

No finding was rejected as wrong. Two proposed FIXES were replaced under the rulings (3: the reader is not loosened;
13: no `NO_MAP_OK`), and finding 2's proposed grep gave way to the helper call.

### 11.3 COORD's answers to the round-2 fixer, applied

| Ruling | Applied where |
|---|---|
| "TrimTests" = leg TR | no change; README question 18 removed |
| LIVE gate as drafted | no change; question 19 removed |
| H2 confirmed: `sumdb/tlog` = 17 with the seat in the union; a red is read against the network first **and stamped so** | ML and LX stamp `sumdb/tlog red: read it against this box's NETWORK first` when that package reads FAIL; the battery's MOD line filter carries the phrase; the brief says so |
| MOD-orphans STOPS the battery, exit 5 | B: the census is read up to three times, 20 s apart (a host that is still exiting is not an orphan); a survivor on the last reading is a finding, keeps the lock, prints NONZERO LEGS and FINDINGS so far, and exits 5 with no purge. ML's own census takes the same three readings (its verdict was a false FAIL for a host caught mid-exit). The census unreadable three times stops the battery too (no pass by absence) |
| Test-host caps without a blame collector | no change |
| DEADLINE: no "none" | no change |
| `REGEN_ALLOW` file-exact for F1+F6 | no change |
| Proof pages: the landing precheck runs BEFORE the bank step, which regenerates the two pages | README M11 says so; question 12 removed |
| G's GOTRACEBACK control is not a battery leg | README section 6 says it is a reading COORD may take by hand with G's command; no command is inferred; question 20 removed |
| NGa under Windows PowerShell 5.1 is in the acceptance | no change; the README records that a 5.1 red is a finding to read |
| `FIXUP_N` scripted mode stays | no change |
| `OS_ROOT_EXPECT='903 2'` stays | no change; question 22 removed |
| One row may still be added (`g-lazy-callers`, 483d4ea217) | read with git: its own two commits change `golib/PanicException.cs`, `runtime/managed_impl.cs` and add `GolibTests/LazyCallersTests.cs`; it does not edit `Goroutine.cs`, so precheck's H3 arm (rows derived from each row's OWN commits) does not name it and `H3_TOKENS` needs no entry. No derived-with-literal list fails for it: no script changed. README M1 states what adding it takes, and that it can only be seated before the fixup commit exists |

### 11.4 The frozen list: every statement about an earlier list

- **README**: the header (the frozen list, its hash, its map union, the assembled head); step 0 (the map is done; the
  `MAP_UNION=382ceeb838` alternative is gone; the assembly refuses without a map); steps 2 to 4 (`EXPECT_HEAD` is
  d4aae0aca0; step 3 is skipped); the exit statuses; the `MAP_UNION` and `EXPECT_HEAD` rows; section 2's H4 and M12
  rows; M1 (frozen; the one row that may be added); M2 (resolved in-seat); M3; M9b.1 (PREDICTED rc 0, no FAIL, and the
  H4 arm's firing control on 382ceeb838 with the 06:31 list); M9c (the frozen hash; a fourth control, the map
  refusal); M11; M12 (COORD's patch exists); section 5's leg 4 (785 = 791 - 6), MOD row and the MOD findings; section
  6 (G's control); section 7 (eight questions answered; 24 and 25 added); section 8; section 9 regenerated.
- **Script headers**: B (the union shape lines, the H4 paragraph, the follow-up comment, CB's and NG's map union, CNR's
  N, MOD's KNOWN list, a round-3 block); F (the row count, the H4 paragraph, the follow-up comment, two `die` texts
  that pointed at M2, a round-3 block); A (the row count, the NEXT line, a round-3 block); CM (a round-3 block with the
  frozen map's reading); ML (the control converters, the H1 / H2 lines of the KNOWN table, a round-3 block); LX and I9
  (round-3 blocks); H (the H4 arm's comment, the one-reader note).
- **tM-follow.txt**: EMPTY and it stays empty; the commented example no longer names `p1-m-repros-move` (an invented
  ref now: nothing is owed). Its entry hash is unchanged (`e3b0c44298fc`, comments are not hashed).
- **Both briefs**: 24 rows FROZEN, no follow-up merge; the i9's four files and its PRE expectation (three rows at the
  base, log/slog at the union); the linux `DERIVED execution-config rows` line, the log/slog reason, semaphore, the
  new movers, `NOT MEASURED` is never a pass.
- **controls/README.txt**: the precheck control's prediction for the frozen list, and where COORD's M9c lists are.
- Re-read with git at 11c188daf3 because a deletion could move them: 73 added tests, 8 + 2 GolibTests classes, 7
  behavioral projects, 30 `package_info.cs`, 3 paths under `src/tools`, projitems 449 and go2cs.slnx 1076 lines, 40
  projects with a go statement of which 2 carry no `NoInlining`, 24 merges and 44 pairs with 0 lost / back /
  duplicated (also at d4aae0aca0), 0 Go-only behavioral directories, 791 directories with Go source. 11c188daf3 and
  382ceeb838 differ by the four deleted files of `TestNeedsTransitiveApiRef` only.

### 11.5 Not done, and unexercised

- Every round-3 construct is unexercised code: LX's `execrows` derivation and its new movers, I9's helper call and
  positive ENV gate, ML's `ctlconv` patch step and three-reading census, B's `FXGOLD` loop, row-count assert and
  MOD-orphans stop, A's map refusal, `SEATTOP` and exit 4, CM's snapshot, F's row-count assert and the new commit
  message lines. COORD's 08:15 to 08:20 runs used the ROUND-2 copies of CM and A.
- The patched control converters have not been built. The i9's python version is not known.
- No before-copy of the draft was taken at the start of this round, so per-file added / removed counts are given only
  for the four files COORD copied at 08:12 (`asm1/`): tM-assemble.sh +24 / -5, tM-conflict-map.sh +19 / -2,
  tM-helpers.py +6 / -1 (comments only), tM-follow.txt +6 / -2.
- Sections 1 to 10 of this file were not rewritten; where they describe the 06:31 list they are superseded (the header
  says so).

### 11.6 Self-check after round 3

| Check | Result |
|---|---|
| `bash -n` | OK: all nine `.sh` (and `wf/notes/_r3-lineindex.sh`) |
| `python -m py_compile` | OK: tM-helpers.py, tM-regen-apply.py, emitdrift.py (compiled into `wf/notes/_r3-pyc/`; no `__pycache__` in the draft) |
| CR bytes | 0 in every file of the draft |
| byte-identical to round 2 | emitdrift.py, tM-regen-apply.py, tM-emitcheck.sh, tM-i9-te.sh, tM-ng-parse.ps1, the two lists, the two control patches |
| editing tool | every draft file was edited with the file tool; no heredoc wrote a file. One exception, stated: the README's section 9 was replaced by `wf/notes/_r3-splice-index.py` (a script FILE written with the file tool; bytes in, bytes out, LF checked) from the table `_r3-lineindex.sh` generated |

Lines after round 3 (after round 2): tM-assemble.sh 249 (230), tM-battery.sh 1411 (1377), tM-conflict-map.sh 75 (58),
tM-fixup.sh 662 (646), tM-follow.txt 21 (17), tM-helpers.py 998 (993), tM-i9-shard.sh 287 (261), tM-linux-legs.sh 679
(607), tM-modules-legs.sh 651 (604), tM-lane-brief-i9.md 181 (171), tM-lane-brief-linux.md 306 (291),
controls/README.txt 48 (39).
