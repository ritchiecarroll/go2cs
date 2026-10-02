# TRAIN M battery: draft summary

**DRAFT 1 (2026-10-02), derived from TRAIN L's script set** (`hnd/.claude/coord-scripts/trainL/`) by a read-only
workflow agent. **Nothing here was run.** The only commands run were git reads, `[ -e ]` on exact module-cache paths,
`bash -n` on every `.sh` and `python -m py_compile` on every `.py`. `tM-CHANGES.md` lists every change against L, file
by file, with its reason and source.

**VERIFY ROUND 1 (2026-10-02)** applied three verifiers' findings (stale literals, leg coverage, hazards and floor):
`tM-CHANGES.md` section 9.

**VERIFY ROUND 2 (2026-10-02, 06:35 to 07:50)** applied the second round's 31 findings under COORD's rulings
(`wf/notes/COORD-RULINGS-r2.md`): `tM-CHANGES.md` section 10. Its largest change is **ruling R1: no by-name list is a
literal that only COORD can keep current.** The battery, the fixup and the two lane drivers DERIVE their lists at run
time from git between the base and the union (or read them from the seat list) and stamp what they derived; the few
literals left are chosen by a hazard, are checked against the tree, and are listed in M1. Still nothing here was run:
this round's commands were git reads, `date`, three read-only probes kept in `wf/notes/` (`_r2-roster-probe.py`,
`_r2-table-probe.sh`, a re-run of `_r1-both-permerge.py`), `bash -n` and `python -m py_compile`.

**VERIFY ROUND 3 (2026-10-02, 08:13 to 09:00)** applied the third round's 16 findings under COORD's rulings
(`wf/notes/COORD-RULINGS-r3.md`): `tM-CHANGES.md` section 11. **The seat list is FROZEN** and this file describes
that list. The three HIGH findings: both lane drivers derived a row's execution config with a grep that matched the
roster's PROSE (they now call the helper's one reader, `execrows`), and the two module-leg controls ran converters that
cannot read the union host's framed event stream (they now carry the H2 seat's stream reader). Commands of this
round: git reads, `bash -n`, `python -m py_compile`, `gofmt -e`, three read-only probes kept under `wf/notes/`
(`_r3fix/`, `_r3-both-permerge-*.txt`, `_r3-lineindex.sh`), and, by mistake, the draft helper's own `execrows`
subcommand three times (two `git show` reads each; it confirmed the lists quoted below).

**Line references.** This file names a construct by its text first. Where a line number helps, it is in section 9's
index, taken with `grep -n` from the files as they are after round 3. Sections 1 to 8 of `tM-CHANGES.md` still describe
DRAFT 1 and their line numbers are that draft's. Seat notes: **M:n** = `trainM/tM-seats-draft.txt` line n,
**L:n** = `trainL/tL-seats-draft.txt` line n.

Union: `/h/go2cs-tmp-coord/tM`, branch `claude/coord-trainM-union`, base TRAIN L's landed master `aa0a07d5fd`. One
signed merge per row of the seat list, in row order. **The list is FROZEN at 24 rows** (08:20; it held 15 rows at
DRAFT 1, 20 at 05:55, 24 at 06:31, and row 18 was re-pointed at 07:43: `p2-test-overload-references` 5556ef86cb ->
**e002a552a8**, one commit that deletes the repro module `TestNeedsTransitiveApiRef`). The frozen list hashes to
**`rows-sha256=0b3fd4358320`** (the hash `tM-conflict-map.sh`, `tM-assemble.sh`, `tM-fixup.sh` and `tM-battery.sh`
print); its map union is **11c188daf3** (`refs/coord/tM-map/rows-0b3fd4358320`; `coord-scratch/tM/asm1/map-frozen.log`:
24 of 24 clean). COORD assembled it at 08:20: head **d4aae0aca0**, `MAP TREE: EQUAL to 11c188daf3`
(`asm1/assemble.log`). No script carries a row count, a row sha, the rows hash or a map union sha. Measurements in
this file that say "at the 24-row map union" were first read at 382ceeb838 (the 06:31 list's union) and hold at
11c188daf3: the two trees differ by the four deleted files of `TestNeedsTransitiveApiRef` and nothing else
(`git diff --stat 382ceeb838 11c188daf3`), and round 3 re-read the ones a deletion could move.
**COORD may still ADD one row before the battery** (`g-lazy-callers|483d4ea217`, last, noted
`stack-on g-godebug-pc-line`): M1 says what that takes.

## 1. The order of work

| Step | Command (from a FRESH per-run copy of this folder + the seat list) | Wall |
|---|---|---|
| 0 | The seat list is FROZEN (08:20). **DONE for the frozen list:** `tM-conflict-map.sh` printed `MAP DONE head=11c188daf3 rows=24 rows-sha256=0b3fd4358320` and kept the union under `refs/coord/tM-map/rows-0b3fd4358320`. Re-run it (`SEATS=<the list> bash tM-conflict-map.sh`, from a per-run copy) only if a row is added, dropped, moved or re-pointed: the map reads the list ONCE into `coord-scratch/tM/map-rows-<h>.txt`, and **the assembly REFUSES a list no map has read** (no rows ref for its hash and no explicit `MAP_UNION`) | minutes |
| 1 | The manual steps of section 4 that come BEFORE the fixup (M1, M9, M9b; M9c's table controls come before an assembly) | |
| 2 | `bash tM-assemble.sh` from the per-run copy (fetch; `git ls-remote` for the seat tips; signing preflight; the seat table first; it finds its map union through the rows ref, refuses without one, and writes its MAP TREE line to `coord-scratch/tM/assemble-maptree.txt`; a tree that is not the map's is exit 4). **DONE for the frozen list:** head d4aae0aca0, `MAP TREE: EQUAL to 11c188daf3` (`asm1/assemble.log`). That run used the round-2 copy and did NOT write `assemble-maptree.txt`'s round-3 form: the fixup stamps whatever that file holds | minutes |
| 3 | Ruled follow-up merges, signed `--no-ff`, each listed in `tM-follow.txt`, each on a branch of its OWN. **NONE is owed at the frozen list** (hazard H4 was resolved inside its seat, M2): `tM-follow.txt` stays EMPTY and this step is skipped. Should COORD rule one later: edit `trainM/tM-follow.txt` in `hnd` BEFORE any run copy is taken, or run `tM-fixup.sh` and `tM-battery.sh` from the SAME run folder (they do not collide: `tM-fixup-logs/` against `tM-logs/` and `mod-logs/`) | |
| 4 | `EXPECT_HEAD=<the assembled head: d4aae0aca0> [PROSE_PATCH=<M12's patch>] bash tM-fixup.sh` | INFERRED about 1 h 25 m: the emission check is 46 m of it (L's leg E: 2772 s), CNR 15 m (L: 928 s), the stdlib-metadata guard a few minutes |
| 5 | **Push the union; GO to the i9 (`tM-lane-brief-i9.md`) and to P1/P2 (`tM-lane-brief-linux.md`)**: before the battery, as at TRAIN L, so the lanes run beside it and `I9_PATCH` can arrive. Both lane drivers now read `tM-helpers.py` beside them (the i9's run folder holds FOUR files, the linux lanes' THREE), so the handover commit must carry the round-3 helper | |
| 6 | `EXPECT_HEAD=<fixup sha, 10 chars> bash tM-battery.sh`, from the fixup's run folder or from a copy holding the SAME `tM-seats-draft.txt` and `tM-follow.txt` (compare `lists=rows:<h>/follow:<h>` on the fixup's FIXUP DONE line with the battery's PRE line) | INFERRED 10.5 to 11.5 h (L's run 2: 10 h 02 m); the default deadline is launch + 14 h |
| 6b | Only if a red needs a tree change AFTER the push (floor 9: a pushed sha is never replaced): a commit ON TOP, subject `fixup-2: TRAIN M -- ...` (then `fixup-3`, ...), signed. `FIXUP_N=2 EXPECT_HEAD=<the pushed head> bash tM-fixup.sh` makes one by re-measuring; a hand commit with that subject is admitted too. Then section 4b | |
| 7 | Landing: merge master into the union, then M11 | |

Per-run copy (floor 4): copy the whole folder, and copy `hnd/.claude/coord-scripts/trainM/tM-seats-draft.txt` into
it. The assembly, the fixup, the battery, a standalone `tM-modules-legs.sh` and a hand-launched `tM-emitcheck.sh` all
refuse a launch from the worktree, from `hnd`, from any `wf/draft*` folder or from the main checkout. The fixup, the
battery, a standalone `tM-modules-legs.sh` and a hand-launched `tM-emitcheck.sh` take one shared lock
(`coord-scratch/tM/.battery.lock`) and, before it, read the LIVE gate (`tM-helpers.py live`: another train's lock; a
`go2cs.exe`, `go2cs.test.exe` or `BehavioralRunner.exe` anywhere on the box; a `dotnet.exe` or `testhost.exe` only when
its command line or executable path names the tM worktree or a path under `coord-scratch`; a run log still being
written. `LIVE_ACK=1` runs past a hit by COORD's explicit call). The battery also refuses a run folder that already
holds a `SUMMARY.txt`.

Exit statuses. Battery: 0 clean; **6** = a leg outside the four expected-non-zero controls is non-zero, or a FINDING
exists; 2 abort; 3 a leg's disk or build abort; **5** a wall cap fired, or a process is still running from the module
out root after MOD (RULED, round 3): the lock is KEPT, the battery STOPS; 9 the deadline. Assembly: 2 a table problem
or no map for the list; 3 a merge that failed; 4 a shape, order or MAP TREE miss. Module legs: 4 on any FAIL verdict.
Linux driver: **4** on any MOVER or freshness fail. i9 shard: **4** on a failed row or a `SOFT:` line.

Environment variables:

| Script | Variable | Default | Meaning |
|---|---|---|---|
| map | `SEATS` | COORD's live `hnd` list | the list the map reads |
| assemble | `TABLE_ONLY` | unset | `1` stops after the seat-table check: nothing created, nothing merged (the check's controls, M9c) |
| assemble | `TIP_MOVED_OK` | unset | refs (space-separated) whose REMOTE tip is past the seated sha and descends from it, acknowledged by COORD; any other moved or missing tip is a table problem |
| assemble | `MAP_UNION` | derived | normally unset: the union `tM-conflict-map.sh` kept for the same rows (`refs/coord/tM-map/rows-<rows-sha256>`; for the frozen list `rows-0b3fd4358320` = 11c188daf3). A sha given here overrides it (COORD's explicit call). **With neither, the assembly ABORTS before creating anything** (round 3, RULED); a sha that is not a commit is refused too, and a tree that differs from the map union's is exit 4 |
| assemble, fixup | `SIGN_PROBE` | `1` | `0` skips the real probe signature of the signing preflight |
| fixup, battery, modules, emitcheck | `LIVE_ACK` | unset | `1` runs past the LIVE gate (floor 1 across trains); stamped |
| fixup | `EXPECT_HEAD` | required | the assembled union head: **d4aae0aca0** for the frozen list (no follow-up merge was made; re-read it with `git -C /h/go2cs-tmp-coord/tM rev-parse --short=10 HEAD`); with `FIXUP_N` >= 2, the pushed head |
| fixup | `FIXUP_N` | `1` | `k+1` = a fixup ON TOP of the k already at the head; subject `fixup-N: TRAIN M` |
| fixup | `PROSE_PATCH` | unset | M12: COORD's comment-and-prose patch (`src/_roster.ps1` comment lines, the roster's prose); applied, checked, rides the fixup |
| fixup | `REGEN` | `apply` | `skip` leaves the corpus alone (COORD's explicit call; leg E then reads the uncommitted footprints) |
| fixup | `REGEN_ALLOW` | composed from the seat list | the corpus paths step 4 may write: a row whose notes say `CORPUS FOOTPRINT n file (<path>.cs ...)` admits that file; `g-godebug-pc-line`, while a row, admits the files directly in `net/http` and `internal/reflectlite` (`REGEN_G_PKGS`). Stamped in PRE. A value given at launch is COORD's explicit call. Anything else STOPS the fixup before a byte is written |
| fixup | `SIBLINGS` | `report` | `refresh` also refreshes the `*.cs.auto` review siblings only the UNION arm rewrites (through the same allowed set: no widening) |
| fixup | `GOLDENS` | `regen` | `skip` leaves stale goldens for leg 4 to report |
| fixup | `GOLDEN_CLASS` | `gframe` | `any` re-baselines a moved golden that carries another line kind, after COORD has read it |
| fixup | `EXP_DESC`, `EXP_TH` | unset | optin.py check's two counts; unset = reported, not asserted (**TO BE SET BY COORD**) |
| fixup, battery | `EXEC_ROWS_EXPECT` | the count of `tM-helpers.py` `EXEC_RULED` | rows carrying an execution config (L:31, L:63). The ruling has ONE site now, `EXEC_RULED` (log/slog alone); see M1 |
| fixup, battery | `SEATS_EXPECTED` | unset | optional second derivation of the seat count |
| battery | `EXPECT_HEAD` | required | the head of the union: the fixup commit, or the newest `fixup-N` on top of it (10 chars) |
| battery | `MASTER` | `aa0a07d5fd` | TRAIN L's landed master; another value is stamped as COORD's explicit call |
| battery, modules | `DEADLINE` | launch + 14 h | an ABSOLUTE local time `'YYYY-MM-DD HH:MM'`; a time already past is refused; no leg starts after it (exit 9) |
| battery | `CNR_EXPECT_N` | unset | CNR's package count; unset = reported (**TO BE SET BY COORD**) |
| battery | `E_BISECT_SEATS` | `E_BISECT_KNOWN` (G, F1+F6) | the seats bisected if leg E reads above 0; checked against the derived converter seats, the rest stamped by name |
| battery, modules | `REALMOD_TEST_TIMEOUT` | `2m` (in `tM-modules-legs.sh`, the one default) | the real-module legs' `-test-timeout`, always passed explicitly. **F4's verdict exists only at 2m** |
| battery | `REALMOD` | `1` | `0` skips XS/XM by COORD's explicit call (otherwise fewer than two REALMOD verdicts is a finding) |
| linux P2 | `RT_CHILD`, `RT_OUTER` | `210m`, `450m` | the full runtime row's per-child and outer deadlines (RULED, round 2) |
| battery | `PRECHECK_MODE` | `abort` | `warn` runs past a failed PRE assert, stamped |
| battery | `I9_PATCH`, `I9_BASELINE` | unset | the i9's shard patch for TE-i9, and its TRAIN L patch as the baseline |
| battery | `SWEEP_ROW_CAP` | `4h` | outer wall cap per sweep row |
| emitcheck | `TM_LOCK_HELD` | unset | set to `1` by the fixup and the battery; without it the check takes its own guards (a hand launch) |

## 2. Why the union owes a fixup

Git merges every row clean. Several things are still unfinished afterwards (seat-footprints.md, hazards H1, H2, H5;
H4 is resolved in-seat at the frozen list). The fixup does the scripted ones; the others need a person.

| | Fact | What breaks without it | Where |
|---|---|---|---|
| G1 | G's go-creator frame puts `NoInlining` on every function that executes a `go`. G re-baselined the goldens that existed at its base `c2591d5b95`, before TRAIN L. Two of TRAIN L's own guards are PREDICTED stale: `ForeverWaitWorkersMainReturns`, `MainSelectForeverWorkerExits` (first pre-map union: 40 projects hold a go statement, 2 carry no `NoInlining`) | Leg 4 (CNR) reads them CHANGED; TargetComparison fails | fixup step 5: the union's own CNR measures, the content class is checked, `run-behavioral.ps1 --update-targets --filter` re-baselines, the four phases verify. Leg 5 then reads every project the fixup changed, isolated (derived) |
| G2 | Two converter changes reach corpus files no seat committed: F1+F6's `go/internal/srcimporter/srcimporter.cs` hunk, and G's Extension A (L:17 "+7 functions per target"; its commit changes the converter only) | Leg E reads them union-attributable | fixup step 4: the emission check at the assembled head (kept roots), a control of the refusal arm, then the union converter's own emission is copied, inside `REGEN_ALLOW` only |
| META | The 24-row map union changes 30 `src/core` `package_info.cs` and no `src/go2cs/stdlib-metadata.txt` | `TestStdLibMetadataInSync` fails in leg C / CB if the asset is stale | fixup PRERES reads the guard by name; a STALE asset is regenerated (`go run ./internal/genstdlibmeta`), re-read by name and RIDES the fixup (RULED). Whether it IS stale is not measured here |
| M12 | The log/slog reason in `src/_roster.ps1`'s comment and the roster prose is mis-rooted (L:76, L:77) | nothing is gated on it | **manual step M12**: COORD writes the words as a patch, `PROSE_PATCH=<file>` rides the fixup (RULED). COORD's patch of 08:21 is `coord-scratch/tM/fix1/prose.patch` (made by `coord-scratch/tM/prose_patch.py` on the assembled union); it also rewrites H5's sentence |
| H4 | A behavioral directory with Go sources and no C# (`src/tests/Behavioral/TestNeedsTransitiveApiRef/{a,b}`, added by p1-m-repros). **RESOLVED inside the seat:** row `p2-test-overload-references` was seated at e002a552a8, whose one commit past 5556ef86cb deletes the module. 0 such directories at the map union 11c188daf3 | (when one exists) CNR transpiles it and lists the untracked output as CHANGED; check-solution-integrity sees unregistered csproj | Nothing is owed: no follow-up merge, `tM-follow.txt` EMPTY. precheck's H4 arm still guards it in the fixup and the battery and reads 0 (M2) |
| H5 | `docs/ValidatedTestPackages.md:528` says "Three rows opt back out" while one row carries `execution: release-tiered` | nothing is gated on it | **manual step M3.** Stamped as a NOTE; it may ride M12's patch |

L's fixup content does not apply to M. No M csproj was cut at an older template (fixup step 1 derives it again and can
still repair one), and the output-comparison list has no gap at the first pre-map union (724 == 724; `outparity`
reads it again at the real union).

## 3. Files

| File | What |
|---|---|
| `tM-conflict-map.sh` | The pre-map. Round 2: `SEATS` overridable; `MAP DONE` prints the rows hash; a fully clean map keeps its union under `refs/coord/tM-map/rows-<hash>`. Round 3: the list is read ONCE into a snapshot (`coord-scratch/tM/map-rows-<hash>.txt`); the merges and the hash read the snapshot. |
| `tM-assemble.sh` | Assembly: the seat-table check (now with remote tips, landed rows, master's patch-ids, `TABLE_ONLY`), the merges, the shape and ORDER asserts, the MAP TREE line. Round 3: it REFUSES a list no map has read, and a MAP TREE that is not EQUAL is exit 4. |
| `tM-follow.txt` | The ruled follow-up merges, one `ref:seat-sha` per line. EMPTY, and it stays empty at the frozen list (no follow-up is owed). One copy per run folder; hashed on every PRE line (`follow:e3b0c44298fc` = EMPTY). |
| `tM-fixup.sh` | The fixup: PRE, PRERES (+ the metadata asset), 0b PROSE, S1/optin (expected to edit nothing), CORPUS, GOLDENS, shape, commit. `FIXUP_N` for a fixup on top. Round 3: the commit message quotes this run's own precheck lines; a seat is named only when it is a row. |
| `tM-regen-apply.py` | Copies the union converter's emission over the listed corpus files, after checking every path. Unchanged in round 2. |
| `tM-battery.sh` | The i7 battery. L's framework with M's legs; section PRE-D derives the by-name lists. Round 3: `FXGOLD` holds project directories only; `seats-effective.txt` is the fixup's construction; a process left by MOD stops the run (exit 5). |
| `tM-helpers.py` | L's readers; `precheck` re-derived for M; `teattr`, `hunkclass`, `realmod`, `live`, `hostwall`; round 2: `execrows`, `execruled`, `rosterlinux`, three roster/docs arms in `precheck`, a narrower `live`. Round 3: comments only; `execrows` is now the ONE reader of a row's execution config for the battery and both lane drivers. |
| `tM-emitcheck.sh`, `emitdrift.py` | Leg E. `emitdrift.py` is L's bytes. Round 2: the guards of a hand launch. |
| `tM-modules-legs.sh` | Leg MOD: L's module legs, then XS and XM. Round 2: the H1 / H2 seats read from the seat list, the process census, the absolute deadline. Round 3: the control converters of MR4c / MR6c carry the H2 seat's stream reader; the census is read up to three times. |
| `tM-i7-sweeps.txt`, `tM-i9-shard.txt` | **93** and **132** rows, byte-identical to L's. 93 + 132 = 225. |
| `tM-ng-parse.ps1` | L's ParseFile reader for the NG legs (unchanged). |
| `tM-i9-shard.sh`, `tM-i9-te.sh`, `tM-lane-brief-i9.md` | The i9 driver, its TE reader and its brief. Round 3: the driver needs `tM-helpers.py` beside it (FOUR files in the lane's run folder) and reads the config rows with `execrows`. |
| `tM-linux-legs.sh`, `tM-lane-brief-linux.md` | The P1/P2 driver and brief. Round 2: derived lists, MOVERS, exit 4. Round 3: the config rows through `execrows`; every remaining stamp-only expectation is a MOVER. |
| `controls/` | Two patches for `teattr` and `hunkclass`, with PREDICTED readings. Not run. |

## 4. Manual steps (a check can name them; a person decides them)

- **M1. The seat list is FROZEN (08:20): 24 rows. Walk the literal sites once if COORD adds the one row still
  possible.** A row added or moved means: re-run the pre-map (step 0) BEFORE the assembly reads the list (it refuses
  otherwise), and read M8 and section 5. The row order is the merge order; a stacked seat sits below its base (the
  assembly's table check on the frozen list: `TABLE OK: 24 rows ... rows-sha256=0b3fd4358320`, nine declared stack-on
  rows, `asm1/table-real.log`). The pre-map prints, for every clean row, the paths that row and the union so far BOTH
  changed: **every shared path of a late row that is not a registration file is READ by a person before the fixup**
  (precheck's BOTH arm then checks the merge's own tree line by line: 24 merges, 44 pairs, 0 lost / back / duplicated
  at the map union 11c188daf3 AND at the assembled head d4aae0aca0, by the commits-only re-implementation:
  `wf/notes/_r3-both-permerge-*.txt`).
  **If `g-lazy-callers|483d4ea217` is added (last row, noted `stack-on g-godebug-pc-line`):** its own two commits
  change three files, `src/core/golib/PanicException.cs` (one line), `src/core/runtime/managed_impl.cs` (a hand-own)
  and a new `src/tests/GolibTests/LazyCallersTests.cs` (read with git at 483d4ea217 against 0af55d033e). It does NOT
  edit `Goroutine.cs`, so `H3_TOKENS` needs no entry and precheck's H3 arm does not name it; its test class is read by
  name by derivation (GT on the i7, the linux drivers); `REGEN_ALLOW`, `EXEC_RULED`, `S1_NONE`, `E_BISECT_KNOWN` and
  the registration files are untouched. What it takes: the row in the list WITH its `stack-on` note (the table check
  refuses it otherwise), a fresh map run (a new rows hash), a re-launch of the assembly (a RESUME merges it last and
  compares the tree with the new map's), then the fixup. **It can only be seated BEFORE the fixup commit exists**:
  with a fixup at the head a resume would merge the row ABOVE the fixup and then fail its shape check (exit 4), so
  after the fixup the row waits for TRAIN N. A golib seat: it owes 2b, GT at both configurations and the linux lanes'
  GolibTests, all of which the battery and the lane drivers run anyway.
  **What is DERIVED at run time and needs no edit for a new row:** the converter tests the union adds (CB; LCn on
  linux), the GolibTests classes it adds or changes and the ones a box does not compile (GT), the behavioral projects
  it adds and the ones the fixup re-baselined (legs 4, 5; LB), the nugetgo test scripts (NGa), the execution-config
  rows (S; the lane drivers), the linux expected counts, `REGEN_ALLOW`, the map union, the H1 / H2 host seats.
  **What is still a literal: walk it when a row is added** (the assembly's MAP TREE: DIFFERENT line prints the same list):

  | File | Literal | What it is | Checked how |
  |---|---|---|---|
  | `tM-helpers.py` | `EXEC_RULED` | THE ruling: which rows carry an execution config (log/slog alone). The ONE site: `EXEC_ROWS_EXPECT`, `TC0_ROWS`, `TIERED_ROWS` and the lane drivers follow the roster, and precheck asserts the roster against this by identity | precheck FAIL |
  | `tM-helpers.py` | `H3_TOKENS` | one identifier per seat that edits `golib/runtime/Goroutine.cs` (`g-godebug-pc-line`: `systemBasis`) | a row that edits the file with no entry FAILS precheck by name |
  | `tM-helpers.py` | `S1_NONE` | the four hand-written csproj with no LangVersion | precheck FAIL S1 |
  | `tM-fixup.sh` | `REGEN_G_SEAT`, `REGEN_G_PKGS` | where G's Extension A is allowed to land | a path outside STOPS the fixup |
  | `tM-battery.sh` | `CB_BASE` | six tree guards that exist at the base (metadata, five projitems) | a name missing from the tree is a finding |
  | `tM-battery.sh` | `GT_NEIGH`, `GT_TESTS` | the hazard-H3 neighbour classes and two Class.Method names | a class file or method missing at HEAD is a finding |
  | `tM-battery.sh` | `CHECKDEAD_GUARDS`, `NEIGH` | L's five checkdead guards, L's eight neighbours | a missing directory is a finding |
  | `tM-battery.sh` | `E_BISECT_KNOWN` | the two seats with a KNOWN uncommitted footprint | a name outside the derived converter seats is a finding; the rest are stamped |
  | `tM-battery.sh` | `X_ROWS`, `T_ROWS`, `HS_WANT`, the `named` cases of leg S, `CRASH` | encoding/json; the two T rows; the 3 GoDefaultGodebug files; test names read by name per row | stamped; a moved set is a finding |
  | `tM-battery.sh` | the measured minimums 24 / 68 / 26 / 62 / 1281 | CT, GN, TR, ST, FX: each total is gated against a floor derived from the tree (or L's own record), which must not fall below these | finding |
  | `tM-linux-legs.sh` | `GT_NEIGH`, `LB_FIXED`, the rows of L4 / L5 / L6 with their names, `OS_ROOT_EXPECT` | the linux lanes' hazard-chosen sets; `os` as root = 903 + 2 | a missing file or directory is a MOVER |
  | `tM-i9-shard.sh` | the `named` cases | test names read by name on four rows | stamped |
- **M2. H4 is RESOLVED inside its seat: nothing to do, no follow-up merge is owed.** Row `p2-test-overload-references`
  was seated at **e002a552a8** (its tip moved from 5556ef86cb before seating): one commit, "Remove the
  TestNeedsTransitiveApiRef repro module from the behavioral root (F8 follow-up)", deletes that module's four files. Read with
  git: 0 behavioral directories hold Go sources and no C# at the map union 11c188daf3 (2 at the 06:31 list's union
  382ceeb838). `tM-follow.txt` stays EMPTY and step 3 is skipped. precheck's H4 arm still runs in the fixup (PRERES,
  step 6) and in the battery (PRE-1) and is expected to read `ok   H4 Go-only behavioral directories ...: 0`. **Only
  if that arm FIRES** (a list other than the frozen one) do the two routes apply, both a ruled FOLLOW-UP merge on a
  branch of its OWN, cut on the seated sha, never pushed onto the seated ref: move the Go-only directory out of
  `src/tests/Behavioral`, or commit its emission and register it; then `<its ref>:<seat sha>` in `tM-follow.txt`.
  The drop constraint from the same hazard stands: `p1-m-repros` cannot seat without every seat that goldens one of
  its directories (F5, F3, F7, F1+F6), or those directories become Go-only the same way.
- **M3. H5, the roster sentence.** No count in the table or the header moves (precheck's ROSTER arms). It rides M12's
  `PROSE_PATCH`: COORD's `fix1/prose.patch` rewrites "Three rows opt back out" to name log/slog alone. Expect
  precheck's H5 NOTE at PRERES (the patch is applied after it, in step 0b) and no H5 NOTE at step 6 or in the
  battery's PRE-1.
- **M4. Read the `REGEN_ALLOW` the fixup stamps** (`PRE derived (R1): REGEN_ALLOW=/.../`). RULED: it holds only files a
  seat's own acceptance measured. At the 24-row list it composes to
  `^src/core/(go/internal/srcimporter/srcimporter\.cs|net/http/[^/]+|internal/reflectlite/[^/]+)$`. A regenerated
  path outside it stops the fixup BEFORE writing anything; the emission evidence is kept, so a relaunch with
  `REGEN_ALLOW=<pattern>` (COORD's explicit call) takes minutes. Extension A's third file (L:20 "3 files x3") is named
  nowhere: expect that stop if it is outside those two folders.
- **M5. Review siblings and hand-owns.** The default is `SIBLINGS=report` (RULED): the fixup counts the `*.cs.auto`
  files only the union arm rewrites and refreshes none. A sibling beside a hand-owned `.cs` (G committed
  `net/windows/lookup_windows.cs.auto` and `runtime/mfinal.cs.auto`) may mean the hand-own owes a re-derive: read each
  one the fixup lists. TRAIN L left standing siblings (7 / 5 / 3 per target in its base arm); those are not M's.
- **M6. `c1-route-sysctl` IS seated** (row 7 of the 24): its commit removes the `route.sysctl` row from
  `declaredOpenDarwinPulls` ("Empty since S7b"), so `TestDarwinLinknamePullsAreFilledOrDeclared` reads 0 open (hazard
  H6). CB stamps the census line and asserts neither count: read it.
- **M7. `SetegidBroadcastSeam` is never re-baselined whole** (L:34). It is linux-exclusive; the windows CNR skips
  it. P1's LB reports its residual; the expected residual is the go1.24 alias family only.
- **M8. If the row order changes,** the two adjacent inserts in `go2cs-src.projitems` (i9 above
  `callerInliningAnalysis.go`, G below it) can become one conflict hunk (hazard H7). The pre-map shows it.
- **M9. Run the two control patches** through `teattr` and `hunkclass` once before the first battery
  (`controls/README.txt`; the TE line gained `g-frame-only files=<n>` in round 2): the readers have never executed.
- **M9b. Run the precheck's and the LIVE gate's controls once before the first fixup** (floor 13). All three are
  read-only and need no new tree; precheck in `head` mode reads the HEAD of the directory it is given.
  1. `python -B tM-helpers.py precheck <the tM-map worktree, or the assembled tM worktree> aa0a07d5fd <the frozen seat
     list> head` (the map worktree stands at 11c188daf3, the assembled one at d4aae0aca0: one tree). **PREDICTED rc 0,
     `PRECHECK hard-failures=0 notes=1 mode=head`, NO `FAIL` line**; the one NOTE is H5 (until M12's patch is in the
     tree). Every arm ok: COUNT x6 (the files read 425 -> 449 lines for projitems, 1069 -> 1076 for go2cs.slnx, +21
     for CompileTests / TargetComparisonTests / TranspileTests, +18 for OutputComparisonTests), REG x6,
     `BOTH: 24 first-parent merge(s), 44 (merge, path) pair(s)`, ROSTER rows, ROSTER linux annotations (223 == 223),
     SNAPSHOT 0, PROOF 2 pages (net.http.md, internal.godebug.md, both G's blobs), ROSTER execution annotations
     `log/slog[release-tiered]` alone, H3 `systemBasis` 7, **`ok   H4 Go-only behavioral directories ...: 0`**, no
     markers. (Rounds 1 and 2 predicted "rc 1 with exactly one FAIL, H4" here: that described the 06:31 list. The frozen
     list resolves H4 in-seat, so a FAIL H4 line here would now be a finding, not the expected reading.)
     **The H4 arm's FIRING control (optional, floor 13: without it that arm has never been made to fail):** the same
     command on a checkout whose HEAD is 382ceeb838, the 06:31 list's union (kept as `refs/coord/tM-map/382ceeb838`),
     with a copy of the list whose row 18 reads `p2-test-overload-references|5556ef86cb|...`. It needs a checked-out
     tree (the S1 census and optin.py read files on disk): `git -C /h/Projects/go2cs worktree add --detach
     /h/go2cs-tmp-coord/tM-h4ctl 382ceeb838`, run it, then `git -C /h/Projects/go2cs worktree remove
     /h/go2cs-tmp-coord/tM-h4ctl`. PREDICTED rc 1 with exactly ONE `FAIL` line: H4, `2` directories,
     `TestNeedsTransitiveApiRef/a` and `/b`.
  2. The same command on any checkout whose HEAD is `aa0a07d5fd` (no seat merged). PREDICTED rc 1: `FAIL COUNT` on
     the registration files the seats add to, `FAIL REG` seat-keys-missing, one `FAIL BOTH <seat>: no first-parent
     merge` per row, `FAIL PROOF` (the base holds its own two pages, not G's), `FAIL ROSTER execution annotations`
     (three rows annotated at the base), `FAIL H3` (`systemBasis` 0, want 7). H4, SNAPSHOT and the linux annotations ok.
  3. `mkdir -p <coord-scratch>/tZ/.battery.lock`, then `python -B tM-helpers.py live <coord-scratch>\tM\.battery.lock 1`.
     PREDICTED rc 1 and one `LIVE lock ...tZ...` line; `rmdir` it and the same command reads `LIVE-VERDICT none`, rc 0,
     on an idle box. (Round 2 narrowed the process arm: an unrelated `dotnet.exe` no longer counts.)
  A reading that differs is a defect in the reader.
- **M9c. Run the seat table's controls once before the first assembly** (floor 13, new in round 2; RULED). From a
  per-run copy: `TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<planted copy of the list> bash tM-assemble.sh`. `TABLE_ONLY=1` stops
  after the table check, so nothing is created or merged whatever the table says. Three planted copies:
  1. **A duplicated sha.** Append `dup-control|73a7a15425|control` (row 3's sha under a second name). EXPECT exit 2
     with `TABLE row 25 dup-control: the same sha as row 3 i9-channeltests-makechan-panic` (and `origin tip none`
     for the invented ref).
  2. **A stacked row above its base.** Move `c1-route-sysctl` above `c1-darwin-linkname-pulls`. EXPECT exit 2 with
     `'stack-on c1-darwin-linkname-pulls' names no EARLIER row` and `is an ANCESTOR of the earlier row 6 c1-route-sysctl`.
  3. **A row already on the base.** Append `landed-control|bbbb3ff022|control` (a TRAIN L seat, an ancestor of
     `aa0a07d5fd`). EXPECT exit 2 with `is already an ancestor of the base aa0a07d5fd (a LANDED seat: drop the row)`.
  Then the real list: EXPECT `TABLE OK: 24 rows ... rows-sha256=0b3fd4358320` (the frozen list; COORD read exactly
  that at 08:15, `asm1/table-real.log`) and `TABLE_ONLY=1: stopping`. `TABLE_ONLY=1` exits BEFORE the round-3 map
  refusal, so the planted lists need no map.
  4. **The map refusal (new in round 3; run it BEFORE the fixup, while the union's head is its last seat merge).**
     A list that passes the table and that no map has read: the frozen list WITHOUT its last row (no row stacks on
     row 24). `SIGN_PROBE=0 SEATS=<that 23-row copy> bash tM-assemble.sh`, with NO `TABLE_ONLY` and NO `MAP_UNION`.
     EXPECT `TABLE OK: 23 rows ...`, then exit 2 with `ABORT: no map run is recorded for THIS list (rows-sha256=<h>,
     23 rows: no refs/coord/tM-map/rows-<h>, and no MAP_UNION given). Remedy: ...`. Nothing is created or merged.
     (Should the refusal NOT fire, the run can only resume on the assembled union, where all 23 rows read `already
     merged (resume)`: it cannot add a merge. That is why this control is run after the assembly and not before.)
  `tM-regen-apply.py`'s refusal arm has its control inside the fixup (step 4: the same check with an allow pattern
  that admits nothing must refuse every listed path, or the fixup stops).
- **M10. The darwin run** of the C1 darwin seats and c1-darwin-std-hygiene is a mac CI dispatch, not a leg of any box
  here.
- **M11. At landing, after merging master into the union:** first run `git diff --stat aa0a07d5fd <that master sha>`
  and read it whole. **If it names any path under `src/` other than `src/version.props`, the battery's verdicts do not
  transfer**: re-run the legs that read those paths (section 4b's table) before landing. State the diffstat in the
  landing message. Then run `python -B tM-helpers.py precheck <tM> <that master sha> <the battery run folder's
  tM-logs/seats-effective.txt> head` (the seat rows PLUS the ruled follow-up rows: the plain list reads a follow-up's
  registrations as nobody's) and `check-roster-format.ps1` under both editions with `EXEC_ROWS_EXPECT`. precheck's
  BOTH arm reads every merge on the union's first-parent line, the landing merge of master included. **RULED (round
  3): this precheck runs BEFORE the bank step.** The bank step regenerates `net.http.md` and `internal.godebug.md`
  from the union's own bank readings, and precheck's PROOF arm fails BY DESIGN once a page a seat committed is another
  blob: run after the bank step, that one FAIL line is the bank's change and not a finding. At the frozen list there
  is no follow-up row, so `seats-effective.txt` holds exactly the 24 seat rows. No script lands the train.
- **M12. The log/slog reason: COORD writes the words, the fixup carries them (RULED).** The reason is "a timing flake
  on a busy disk: symbol-file reads at the first source-line resolution; rooted in runtime.Callers symbolizing at
  capture", not "first-call JIT latency". The hunk is comment-and-prose only: the `#` comment lines in
  `src/_roster.ps1` (lines 124-125 at the 24-row map union: "TestSetDefault's 1 s deadline against first-call full-opt
  JIT latency on a slow box") and the roster's prose. No row, count or execution config changes. **COORD's patch
  exists** (08:21): `coord-scratch/tM/fix1/prose.patch`, cut on the assembled union d4aae0aca0 by
  `coord-scratch/tM/prose_patch.py`; it also carries M3's sentence. How such a patch is made: make the edit
  in the ASSEMBLED tM worktree, `git diff -- src/_roster.ps1 docs/ValidatedTestPackages.md > <run folder>/m12.patch`,
  `git checkout -- src/_roster.ps1 docs/ValidatedTestPackages.md` (the fixup wants a clean tree), then launch the
  fixup with `PROSE_PATCH=<run folder>/m12.patch`. The fixup refuses the patch if it touches another path, if a
  changed line of `_roster.ps1` is not a `#` comment line or adds a non-ASCII byte (the file has no BOM; that is what
  broke Windows PowerShell 5.1 at L), or if the roster guard no longer passes under both editions with the same
  execution-config count; step 6's precheck asserts no row moved. Unset, the fixup stamps "M12 is still owed".
- **M13. Stale G frame class in committed TEST sources: a RULED deferral, not in M's fixup.** G regenerated no
  committed `*_test.cs` (158 of 1081 hold a go launch), so every `-tests` run rewrites them with the `NoInlining`
  class and `restore_paths` puts the committed bytes back. RULED: the corpus-wide refresh of committed `-tests`
  sources (this class and the older `.slice` spelling class) is a queued COORD item, fed by a windows sweep's
  re-emission AFTER M lands. Until then every post-M sweep (and c1-run-timeout's CI diff-upload step) shows that
  standing tracked diff: say so in the landing message, with TE's number. The TE leg stays a reading; its line now
  prints `g-frame-only files=<n>`, the files stale by that class alone (`te.log`, `te-T.log`).

### 4b. A fixup-N after the push: what COORD re-reads

RULED: a red that needs a tree change after the union was pushed lands as a commit ON TOP, subject `fixup-2: TRAIN M`
(then `fixup-3`, ...); the battery's PRE admits `fixup: TRAIN M` followed by zero or more of them, in order, each
signed, and nothing else single-parent. The battery has no leg-selective mode: the readings below are re-run by hand
(each leg's command is in `tM-battery.sh`), or with a fresh battery when the first column says so. The lanes are told
the new head only when their rows are in the last column.

| The fixup-N touches | Re-read on the i7 | Lanes |
|---|---|---|
| `src/go2cs/**` non-test Go (the converter) | a fresh battery (every leg reads the converter's output) | i9 and linux: again |
| `src/core/golib/**`, `src/gen/**` | 2b, GT x4, GN, CT, leg 5, H7 x3, MOD, then every S / T row: in practice a fresh battery | i9 and linux: again |
| `src/core/**` corpus `.cs` outside golib (a regenerated or hand-owned file) | E, H7 x3, the S or T row of that package and of the canaries, MOD if it is under `testing/` | the lane that sweeps that row |
| `src/core/**/*_test.cs`, `*.tests.csproj` (test-side) | that row's S or T leg; TE | the lane that sweeps that row |
| `src/tests/Behavioral/**` goldens (`.cs`, `.cs.target`) | leg 4 (CNR), `run-behavioral.ps1 --filter <project>` for each changed project | P1's LB when the project is in its list |
| a registration file (`go2cs-src.projitems`, `go2cs.slnx`, the four BehavioralTests files) | precheck, SI, leg C | none |
| `src/go2cs/stdlib-metadata.txt` | CB (`TestStdLibMetadataInSync`) | none |
| `src/_roster.ps1`, `docs/ValidatedTestPackages.md` | G1 / G2, ST51 / ST7, NV51 / NV7, precheck; if a ROW or its execution config moved, that row on every box | the lane that sweeps a moved row |
| other `docs/**` | leg C (repoguard's docs suites) | none |

Always: `python -B tM-helpers.py precheck <tM> aa0a07d5fd <seats-effective.txt> head` at the new head (it counts
every fixup commit's inserts), and the linux and i9 drivers' own asserts accept the new head as they are (they want at
least one `fixup: TRAIN M` commit).

## 5. Legs, the seats they gate, and expected readings

Walls are TRAIN L's run 2 on the same box (`coord-scratch/tL/run2/tL-logs/SUMMARY.txt`), given as a size, not as M's
budget. "REPORTED" means the leg prints the number and nothing is asserted. "derived" means PRE-D or the leg computes
the list or the floor from the tree and stamps it.

| Leg | Gates | Expect | L wall / cap |
|---|---|---|---|
| PRE / PRE-D / PRE-1 / 1b / 1c | the union's shape (24 seat merges + follow-ups, none at the frozen list + `fixup: TRAIN M` + any `fixup-N`); the derived lists; every seat | shape OK; PRE-D stamps its lists and raises a finding for a literal the tree does not bear out; precheck rc 0 (M9b's arms); outparity no gap; 0 module-path hosts, GoDefaultGodebug in the same 3 files | seconds |
| PRE-2 / 3 | coverage, canaries | 225 = 93 + 132; five canaries derived; the S list also takes the derived execution-config rows and `encoding/json` | seconds |
| C | every converter seat, repoguard | ok throughout | 561 s / 40 m |
| CB | every test the union ADDS under `src/go2cs`, by name (derived: 73 at the 24-row map union) + six base tree guards, over `./...` | N/N `--- PASS`, 0 SKIP/FAIL; the darwin pull census line stamped, not asserted | new list / 30 m |
| FX, FXc1, FXc2 | c1-fixture-tracking; the currency guard's controls | `stale 0`; tracked >= the floor (L's own FX stamp: 1281) and tracked == current; both controls fire | about 25 s |
| E (+ bisect) | every emission footprint is IN the tree | plants OK, 6 x rc 0, union-attributable **0**, csproj drift none, HANDOWN 0, RUNTIME-MAP 0. Above 0: arms for `E_BISECT_SEATS`, and a finding for a file no arm names | 2772 s; each bisect arm the same |
| G1 / G2 | G's roster commits beside master's block | pass x2; `EXEC_ROWS_EXPECT` (1) row with an execution config; check count REPORTED (L 1991) | 19 s |
| SY, SI / SIc | the new projects | clean; SIc fires six cycles | 37 s |
| ST51 / ST7, NV51 / NV7 / NVR | regression (G edits `_roster.ps1`) | 0 violations and checks >= 62, x2; pre-flight clean x2; NVR refused by name | 19 s / 30 m caps |
| NGa51 / NGa7 (per script), NGb51 / NGb7, NGF-j0 x2 | c2-s2-source-metadata's i7 acceptance (3 paths under `src/tools` at the 24-row map union); regression of the rest | every `Test-*.ps1` under `src/tools/nugetgo` (derived: Identity, SelfDescription) `ran N, failed 0` under both editions; 0 parse errors x2 (NGb51 GATED); both PackageReference IDs | about 2 s each |
| 2b | the golib and gen seats (the i9, the C1 darwin seats, std-hygiene, G, p1-warnings-tranche1) | rc 0, errors 0, gen-load 0; Project lines REPORTED | 1435 s |
| CT | i9-channeltests-makechan-panic | Debug and Release: Failed 0, Skipped 0, Total >= the derived floor (24) | new / 10 m cap |
| GN | the gen seats | Failed 0, Total >= the derived floor (68) | new / 20 m cap |
| TR | the testing host (the i9, F4, F8, H1, H2) | Failed 0, Skipped 0, Total >= the derived floor (26; L 26/26) | 30 s / 20 m cap |
| GT Debug + Release x3 | every golib seat, H3 | 0 failed, 0 NOT FOUND over the derived classes; each ADDED class holds at least its `[TestMethod]` count and no failed result; totals REPORTED | about 1250 s / 45 m cap per run |
| 4 | every golden under the union converter | NO REGRESSION; N REPORTED (**TO BE SET BY COORD**: L 778; INFERRED 785 = 791 directories with Go source at the map union 11c188daf3, read with git, minus L's 6 platform skips); no derived new project in the platform skip list | 928 s |
| 5 + B: | the projects the union adds (derived: 7), the ones the fixup re-baselined (derived), L's 5 checkdead guards, 8 neighbours | all pass; the main-alone pair exit 2. Tracked rewrites are restored after the B legs | 7970 s + the isolated legs |
| H7 x3 | linux, darwin, all | CS=0 x3 | about 2400 s |
| MOD | L's module legs (regression); XS, XM | PRE-MOD tracked changes 0 (restored and asserted). L's verdicts all PASS, **the two controls MR4c and MR6c included: they are expected to read rc != 0 with the named tests go=pass cs=fail, which is a PASS verdict** (round 3: their TRAIN L converters carry the H2 seat's stream reader, `ctl-reader-m3.patch` / `ctl-reader-m4.patch` in `mod-logs/`; a patch that does not apply or a converter that does not build is NOT MEASURED and counted as a failure). XS: errgroup 5, syncmap 3, semaphore KNOWN (`TestWeightedAcquire`: the known timing-class non-pass, cause not measured), singleflight **12 with the H1 seat in the union** (it is a row of the 24), else NOT RUN. XM: modfile 323, module 16, semver 9, dirhash 6, sumdb 4, note 7, storage 1, tlog **17 with the H2 seat in the union** (a row of the 24; a red there is stamped `sumdb/tlog red: read it against this box's NETWORK first`), else KNOWN; zip builds. F4: 0 `dotnet timed out` lines. Both module copies unchanged. `MOD-orphans` 0 (three readings, 20 s apart; **a survivor STOPS the battery, exit 5, lock kept**) | 920 s + new / 4 h |
| PB | F4 composed with the binlog switch | rc 0, cmp at its banked count (from the roster: 4), binlog SEEN then absent | 30 s |
| S (the 93-row i7 list minus the 2 T rows, + canaries + the derived config rows + encoding/json) | every row at ITS roster config, on a record deleted before the row | each PASS at banked counts with a fresh record. net/http 1387, `tiered=False`, TestRegisterErr pass/pass. internal/godebug 5, `tiered=False`, TestCmdBisect. log/slog 199 + 17, `tiered=True`. net/rpc 0 deadlock lines on a fresh record | 9145 s / 4 h per row |
| NR x4 | H3 | 15 each, 0 deadlock lines **on a fresh record** | 327 s |
| TE / HS / UF | G's frame class in rewrites; host censuses; fixture tracking | TE a READING (M13); HS 0 / the 3; UF **0** untracked under src/core | seconds |
| T:runtime/pprof | G, H3 | **145 + 7**, BANK-ELIGIBLE, roster match; TestMemoryProfiler by name | 627 s / 30 m |
| TBS-W, T:runtime | **the re-read** (L:59) | TBS-W rc 0; **10819 + 71**, roster match; `TestTracebackSystem/panic` AND `/trap` DISCLOSED by name on a fresh record; crash family pass/pass; 0 deadlock lines | 317 s + 5891 s / 150 m |
| TE-T / HS-T / UF-T, TE-i9, TL-WALL | the same readings over the T legs; wall vs L | readings | seconds |
| i9 shard | the same seats on 132 rows | 132/132; internal/godebug at TC0, log/slog tiered; UF 0; exit 0 | L: 3278 s |
| linux P1 | golib seats, TC0 rows, i9's linux build, LB, LM | see the brief; exit 0 | |
| linux P2 | runtime with /panic by name, pprof, TBS x10 | 10810 + 73; 147 + 7; 10/10 disclosed; exit 0 | L: runtime 9313 s; 210 m per child |

NONZERO LEGS expected by design: FXc1, FXc2, SIc, NVR. **Nothing else**: TBS-W left that list in round 2 (TRAIN L's
run 2 read it rc 0). MOD is non-zero only on a FAIL verdict; a KNOWN line is not one. The battery exits 6 otherwise.

**Gates that are FINDINGS** (a finding is listed at END and makes the exit status 6; none of them aborts):
- T:runtime/pprof and T:runtime: `BANK-ELIGIBLE NO`, a roster compare that is not `AT BANKED COUNTS`, or a leg that
  left no comparison record.
- Any S row that exits 0 with no fresh record; net/rpc, net/rpc/jsonrpc, the NR repeats and TBS-W unless they read
  `DEADLOCK total=0` on `record=fresh`; TBS-W non-zero; `/panic` or `/trap` not disclosed, or read from a stale record.
- MOD: fewer than two `REALMOD-VERDICT` lines unless `REALMOD=0`; a differing name in x/mod `zip` outside `TestVCS`;
  a copy of x/sync that is not read-only throughout; tracked changes that remain before MOD after two restores. A
  process still running from the out root 40 s after the module legs is a finding that ALSO STOPS the battery (exit
  5, lock KEPT, no purge; RULED in round 3): COORD kills the listed PIDs by PID, removes the lock and launches a
  fresh battery, so no S row and no bank leg is ever read beside an orphan.
- E: a hand-owned file written by a conversion, no verdict line, a file no bisect arm names; an unresolved deferred
  marker fails the emission.
- SIc or NVR not firing; `csproj` among the sweep's or the T legs' tracked rewrites; CT / GN / TR / ST / FX off their
  floors; a derived floor below the measured minimum; generator load lines in 2b; an S row other than the derived
  tiered rows whose PASS line carries `[release-tiered]`; PB's cmp off its banked count; every PRE-D literal miss.
- Every S row and T leg stamps `HOSTWALL`, the converted host's own elapsed (RULED: the stamp is enough; no result
  file is kept per S row). TL-WALL's row-wall list stays, labelled as build noise.

Reading note for S:log/slog (L:77): M's PC-to-line seat adds a second PDB reader over the loose `.pdb` files of a
fresh publish, and the i7's disk is busy; **a `TestSetDefault` red in M's battery is read as that first-launch
symbolization class first**, before it is read as a seat's regression.

**TO BE SET BY COORD** (the leg reports; nothing is asserted until a value is given): CNR's N; optin.py's two counts;
the real-module `-test-timeout` if 2m does not suit the i7; a DEADLINE other than launch + 14 h.

## 6. Deliberately NOT in the battery

| Not carried | Why |
|---|---|
| G's GOTRACEBACK=system go-creator control ("net/http TC0 ... prints `created by runtime.unique_runtime_registerUniqueMapCleanup`", L:60) | **RULED (round 3): NOT a battery leg.** It is a reading COORD MAY take by hand after S:net/http, with a command G supplies; no command is inferred here and none is in any script. The union reads the creator frame through GT (`TestGoroutineCreatorTests`, `CreatedByPositionTests`, `TracebackDecorationTests` by name) and CB (`TestGoCreatorFramesAreNotInlined`) |
| The i9's NGE step (the uuid pack end to end) | i9-nugetgo-pack banked at L. A new one belongs to c2-s2-source-metadata (a row of the frozen list) and needs that seat's numbers: COORD writes it into the i9's GO (question 14). |
| L's HOP and SPB legs | No M seat touches `src/run-validated-sweep.ps1`. PB stays, for F4. |
| L's C1 token probe (GT_PROBE, R3) | c1-token-ids landed in L. |
| L's D6 and cross-package TE signatures, their controls, and the two pinned bisect arms | They read two L seats. M's TE classifies G's frame class and compares with L's patch; M's bisect arms are derived from the union's merges. |
| L's K9 and K6 precheck arms | TRAIN K's resolutions, two trains old. |
| A 20-row tiered or R2R sample, the i7 log/slog R2R+TC0 x10 (L:68) | The user-module execution default is a DESIGN item with its own owed readings, not a seat. |
| `--blame-hang-timeout` on the i7's test-host legs | RULED: a wall cap (`capped`), which stops the battery with the lock kept. A blame collector would change the instrument against TRAIN L's readings of the same legs; the linux driver keeps its own (it always had one). |
| Any darwin RUN | A mac CI dispatch (M10). H7-darwin compiles. |
| The linux build and run of the golib seats | P1's legs (GolibTests, `os`, `os/exec`, LB). |
| The non-root real-module reading | Both linux boxes are uid 0; the brief says how P1 reads it non-root or posts NOT MEASURED. |
| A roster row or proof page for x/sync or x/mod | "Nothing here is a roster row" (P1's FINDING record). The module legs' END fails on any status line under `docs/`. |
| The refresh of committed `-tests` sources | M13, a ruled deferral. |
| The M docs of M:30 | No seat carries them. |

## 7. Open questions

Answered by COORD's round-2 rulings and removed from this list: P2's runtime budget (210m / 450m), `REGEN_ALLOW`, a
second fixup after the push (`fixup-N`), a stale `stdlib-metadata.txt` (rides the fixup), SIBLINGS (`report`), the
LIVE gate's dotnet arm, GN and FX (floors), encoding/json (in `X_ROWS`), M12 (rides the fixup), HOSTWALL (the stamp
is enough), the DEADLINE (absolute, launch + 14 h), TE (a ruled deferral).

Answered by COORD's round-3 rulings (`wf/notes/COORD-RULINGS-r3.md`) and removed from this list: **12** (the bank step
regenerates `net.http.md` and `internal.godebug.md` from the union's own bank readings; the landing precheck runs
BEFORE it: M11), **17** (`g-lazy-callers` is not a row of the frozen list; COORD may add it last before the battery:
M1 says what it takes, and nothing in the scripts needs a literal for it), **18** ("TrimTests" = leg TR, 20 m cap,
floor 26), **19** (the LIVE gate as drafted: the three converter and harness names count wherever they run;
`dotnet.exe` and `testhost.exe` stay path-bound), **20** (G's GOTRACEBACK control is NOT a battery leg: section 6),
**21** (H2 CONFIRMED: with `p2-host-event-line-start` in the union both drivers expect `sumdb/tlog` = 17, and a red
there is read against the network first and stamped so), **22** (`OS_ROOT_EXPECT='903 2'` stays). Also ruled:
MOD-orphans STOPS the battery (exit 5); the test-host legs keep the wall cap without a blame collector; the DEADLINE
has no "none" value (a far date is enough); `REGEN_ALLOW` is file-exact for F1+F6; NGa runs every `Test-*.ps1` under
Windows PowerShell 5.1 too, and a 5.1 red is a finding to read, not an instrument error; `FIXUP_N` keeps its scripted
mode.

1. **Which box owns the real-module reading?** Every ruled count is a LINUX reading on P2's local merge. This draft
   reads both modules on the i7 (leg MOD: the first windows reading, so a difference is a FAIL to read, not a known
   regression) AND on P1 (leg LM). If COORD wants linux only, launch the battery with `REALMOD=0` and say so.
2. **Extension A's third file.** L:20 says "3 files x3"; the two read unmarked on G's tip are `net/http/server.cs` and
   `internal/reflectlite/value.cs`. If the third is outside those two folders the fixup stops at step 4 (M4).
3. **`-test-timeout` for XS/XM.** `2m` is the default P2's acceptance used. The i7 is slower than P2's box; `modfile`
   runs 323 tests under that deadline.
4. **x/mod on the i7 needs P1's pin** (the cache holds x/tools v0.42.0, not v0.41.0). The leg applies it to the COPY
   and stamps it. If M2's preflight still refuses the module, the leg reads NOT MEASURED.
5. **zip.** "builds" (M:29) or "builds+runs 109/9/3" (L:67)? The leg gates on build-and-run and reports the counts.
14. **The i9's uuid pack (NGE) for c2-s2-source-metadata.** L:75's "windows acceptance = M's nugetgo legs / i9 uuid
    pack": the i7 half is NGa (now running the seat's own `Test-NugetgoSelfDescription.ps1`), NGb and NGF-j0; the i9's
    pack step, with that seat's numbers, is COORD's to write into the GO.
23. **A proof-page or roster change by a late row.** The PROOF, SNAPSHOT and linux-annotation arms are new and
    unexercised; a legitimate late row that regenerates a page it owns passes (the arm follows the LAST row that
    changed the page), a merge or fixup that edits one fails.
24. **The module-leg controls (round 3).** RULED: either build the control converter with the union's comparer change,
    or drop the control. This round took the first: MR4c and MR6c build TRAIN L seat converters (c5e934c464,
    e9009f2945) with the H2 seat's own `testConversion.go` change applied (+31 / -4 lines: `testStreamLines` and its
    call sites). Measured without running the scripts: the patch applies to both archived sources with `git apply`
    and both results parse (`gofmt -e`); NOT measured: that either patched converter BUILDS, or that the controls then
    read their predicted red. If MOD reads `MR4c` or `MR6c` as NOT MEASURED or FAIL, read `mod-logs/ctl-reader-*.log`
    and `conv-build-m3.log` / `-m4.log` first: it is the instrument until shown otherwise. The alternative COORD
    named (a `-go2cspath` tree from the control's own sha) was not taken: it needs a second and third copy of `src`
    (490 MB each, read with git) and two cold builds of the stdlib closure under the old converter's 20 m publish
    deadline, none of which this draft could time.
25. **The i9 needs `python` 3.8 or newer** for `tM-helpers.py execrows` (round 3). Its L driver already called
    `python`; the version on that box is NOT MEASURED. The brief asks the lane to check before launching.

## 8. What was checked, and what was not

- `bash -n`: all nine `.sh` pass. `python -m py_compile`: `tM-helpers.py`, `tM-regen-apply.py`, `emitdrift.py` pass
  (again after round 3).
- Every edited file is LF; no CR was introduced (0 CR bytes in every file of the folder after round 3).
- The go.mod `h1:` computation in the x/mod pin was checked against two cached `go.sum` lines (DRAFT 1).
- NOT run by the draft's authors: every script; `teattr`, `hunkclass`, `realmod`, the `precheck` arms, `live`,
  `hostwall`, `tM-regen-apply.py`, `execruled`, `rosterlinux` have never executed there. The fixup's steps 0b, 4 and 5
  are new machinery with no rehearsal. (COORD has since run THE ROUND-2 COPIES of `tM-conflict-map.sh` and
  `tM-assemble.sh`, with `TABLE_ONLY` and for real, on the frozen list: `coord-scratch/tM/asm1/`. Round 3's changes
  to those two scripts, the map's snapshot and the assembly's refusal, have NOT run.)
- **Verify round 3, measured:** the helper's `execrows` at aa0a07d5fd reads internal/godebug, log/slog and net/http;
  at 11c188daf3 and at 382ceeb838 it reads log/slog alone; the round-2 driver pattern (`grep 'execution:
  *release-tiered'` over the row) reads all three rows at 11c188daf3, which is the first two HIGH findings. The H2
  seat (932778788f) changes `src/core/testing/TestReporter.cs` and `TestHost.cs` (every `--json` line framed with
  U+0016) and the converter's reader (`testStreamLines`, absent from c5e934c464 and e9009f2945, whose
  `terminalTestResults` unmarshals the raw line): the third HIGH finding. The seat's `testConversion.go` diff (+31 /
  -4) applies with `git apply -` to the `git archive` copy of that file at both control shas and the results parse
  (`gofmt -e`). 0 Go-only behavioral directories at 11c188daf3 (2 at 382ceeb838); 791 directories hold Go source
  there (784 at the base); the tree of the assembled head d4aae0aca0 equals the map union's; 24 merges, 44 pairs, 0
  lost / back / duplicated at both; `g-lazy-callers`' own commits change three files and not `Goroutine.cs`; G's
  merge-base with the base is c2591d5b95; the behavioral runner returns 0 / 1 / 2 (all pass / a failed phase / no
  project matched), read in `BehavioralRunner/Program.cs` at 11c188daf3. **NOT measured:** that a patched control
  converter builds; that python on the i9 is 3.8 or newer; every new gate of the linux driver (none can run here).
- Verify round 2, read with git only at the 06:31 list's map union 382ceeb838 (nothing from this folder was executed;
  the frozen list's union 11c188daf3 differs from it by four deleted files under `TestNeedsTransitiveApiRef`, which
  none of these readings counts, and round 3 re-read the 73, the 8 and 2, the 7, the 30 and the 24 / 44 there):
  the union adds 73 `func Test` under `src/go2cs` in 21 test files (0 with a `//go:build` line), 8 GolibTests class
  files and changes 2 (every file name is its class name; one added class has a linux-only arm that reads
  Inconclusive elsewhere); `GolibTests.csproj` removes 16 files unless GoTargetOS is linux and 12 when it is set and
  not windows, and no `Compile Remove` sits under another condition; 7 behavioral projects added, by the tree and by
  the go2cs.slnx lines alike; 68 / 24 / 26 uncommented `[TestMethod]` lines (GenTests, ChannelTests,
  TestingRuntimeTests.cs); the roster reads 225 rows, 223 linux annotations, three execution annotations at the base
  and log/slog alone at the union, by the helper's own three regexes (`wf/notes/_r2-roster-probe.py`); the linux
  annotations of the rows the brief names equal the brief's counts; 12 seats' merges change a non-test Go source of
  the converter; only `g-godebug-pc-line` edits `Goroutine.cs`; only G's two proof pages change under
  `docs/validation`, and both are G's blobs at the union; 24 merges, 44 (merge, path) pairs, 0 lost / back /
  duplicated; 13 descents, all declared; 0 patches under two shas among the rows and master.
- Read in TRAIN L's run 2 SUMMARY: 94 `record=fresh` and 4 `record=STALE` (NR:2 to NR:5, each rc 0);
  `TestTracebackSystem/trap` disclosed in TBS-W and T:runtime; `T:runtime:tbs rc=0`; ST51 and ST7 `62 checks, 0
  violations`; FX `tracked fixtures 1281`; PB `Validated 4`.
- Read in the converter at the 24-row map union: `Validated %d tests against go test (... %d disclosed-divergent ...)`
  (testConversion.go:9495); `writeJSONFile` writes through `needToWriteFile` (testConversion.go:6557-6563); the sweep
  rejects a record older than its attempt (run-validated-sweep.ps1, `Get-OracleOnlyVerdict`); the three record files
  are gitignored (`src/core/.gitignore`); the metadata generator is `go run ./internal/genstdlibmeta`
  (stdlibMetadata.go:19) and `go generate .` would also run the symbols generator (symbols.go:19); the converter has no
  package filter for `-tests -recurse`.

## 9. Line index (grep -n, after verify round 3)

| File | Construct | Line |
|---|---|---|
| `tM-conflict-map.sh` | the list read ONCE: the row snapshot and its hash (round 3) | 30 |
| `tM-conflict-map.sh` | the merges read the snapshot | 67 |
| `tM-conflict-map.sh` | the rows ref, written when every row is clean | 71 |
| `tM-assemble.sh` | remote tips (`git ls-remote`) | 49 |
| `tM-assemble.sh` | the seat-table check starts (`ROWS=`, then `rows-sha256`) | 81 |
| `tM-assemble.sh` | a row already on the base | 93 |
| `tM-assemble.sh` | master patch-ids | 139 |
| `tM-assemble.sh` | `TABLE OK`, then `TABLE_ONLY` | 146 |
| `tM-assemble.sh` | the map union, derived from the rows ref | 156 |
| `tM-assemble.sh` | the map REFUSAL: no rows ref and no `MAP_UNION` (round 3) | 161 |
| `tM-assemble.sh` | the MAP TREE line, the literal sites (`LITS`), `assemble-maptree.txt` | 205 |
| `tM-assemble.sh` | `MTBAD`, `SEATTOP`: only EQUAL passes (round 3) | 206 |
| `tM-assemble.sh` | merge ORDER == row order | 241 |
| `tM-assemble.sh` | the exit-4 stop on a MAP TREE that is not EQUAL, then NEXT | 248 |
| `tM-fixup.sh` | the switches and their defaults (`SIBLINGS`, `FIXUP_N`, `PROSE_PATCH`, `REGEN_ALLOW`, `REGEN_G_PKGS`) | 73 |
| `tM-fixup.sh` | launch refusal (floor 4) | 102 |
| `tM-fixup.sh` | the four switch checks | 110 |
| `tM-fixup.sh` | the `lists=rows:/follow:` hashes | 132 |
| `tM-fixup.sh` | `REGEN_ALLOW` composed from the seat list | 138 |
| `tM-fixup.sh` | the LIVE gate, then the lock (the refusal is stamped) | 170 |
| `tM-fixup.sh` | the fixup chain walk (`FIXUP_N`) | 227 |
| `tM-fixup.sh` | `seats-effective.txt` (the battery builds it the same way; row count asserted: round 3) | 252 |
| `tM-fixup.sh` | the PRE line, the derived stamp, the assembly MAP TREE stamp | 273 |
| `tM-fixup.sh` | PRERES (precheck, outparity, roster guard x2) | 281 |
| `tM-fixup.sh` | `stdlibmeta` (by name; regenerates a stale asset) | 312 |
| `tM-fixup.sh` | step 0b PROSE (M12) | 339 |
| `tM-fixup.sh` | step 1 S1 | 368 |
| `tM-fixup.sh` | step 4 CORPUS (the kept-evidence re-use follows) | 431 |
| `tM-fixup.sh` | the regen refusal control | 458 |
| `tM-fixup.sh` | step 5 GOLDENS | 491 |
| `tM-fixup.sh` | step 6 (precheck on the edited worktree, purge) | 571 |
| `tM-fixup.sh` | step 7 (expected set, commit, `FIXUP DONE`) | 584 |
| `tM-fixup.sh` | the commit message: this run's own precheck lines (round 3) | 616 |
| `tM-battery.sh` | `X_ROWS` and the ruling-sites note above it | 115 |
| `tM-battery.sh` | `E_BISECT_KNOWN` | 132 |
| `tM-battery.sh` | run-copy checks, launch refusal | 140 |
| `tM-battery.sh` | the deadline, resolved once | 151 |
| `tM-battery.sh` | the LIVE gate, then the lock | 168 |
| `tM-battery.sh` | `finding()` (counted) | 185 |
| `tM-battery.sh` | the `EXEC_ROWS_EXPECT` default, the list hashes | 192 |
| `tM-battery.sh` | PRE: the fixup chain (`NFU`, `UTOP`) | 237 |
| `tM-battery.sh` | PRE: the union shape, the follow-ups | 277 |
| `tM-battery.sh` | `seats-effective.txt` (row lines only, count asserted: round 3) | 303 |
| `tM-battery.sh` | PRE-D, the derived lists (CB, GT, behavioral, execution rows, E-bisect) | 316 |
| `tM-battery.sh` | `FXGOLD`: project directories only (round 3) | 364 |
| `tM-battery.sh` | PRE-1 precheck (PRE-1b and PRE-1c with `HS_WANT` follow) | 403 |
| `tM-battery.sh` | PRE-2 / PRE-3, the S run list | 432 |
| `tM-battery.sh` | `past_deadline`, `deadline_check` | 444 |
| `tM-battery.sh` | `leg` (then `purge`, `restore_paths`, `convbuild`) | 464 |
| `tM-battery.sh` | `timeout_stop`, `capped` | 522 |
| `tM-battery.sh` | `recclean`, `trline`, `tmfloor`, `totalgate`, `gtclassoff`, `orphans` | 554 |
| `tM-battery.sh` | leg C | 602 |
| `tM-battery.sh` | leg CB | 635 |
| `tM-battery.sh` | leg FX and its floor | 647 |
| `tM-battery.sh` | leg E, its verdict stamp, the bisect arms | 701 |
| `tM-battery.sh` | G1 / G2 | 740 |
| `tM-battery.sh` | SI / SIc | 760 |
| `tM-battery.sh` | ST51 / ST7 and their gate | 777 |
| `tM-battery.sh` | NV51 / NV7 / NVR | 791 |
| `tM-battery.sh` | NGa (every `Test-*.ps1`), then NGb, NGF-j0 | 823 |
| `tM-battery.sh` | 2b | 856 |
| `tM-battery.sh` | CT | 877 |
| `tM-battery.sh` | GN | 890 |
| `tM-battery.sh` | TR | 906 |
| `tM-battery.sh` | GT | 950 |
| `tM-battery.sh` | leg 4 (CNR) and the restore after it | 987 |
| `tM-battery.sh` | leg 5, `MGUARDS`, the isolated legs, the restore after them | 1009 |
| `tM-battery.sh` | H7 x3 | 1052 |
| `tM-battery.sh` | the PRE-MOD assert, MOD, its stamps | 1088 |
| `tM-battery.sh` | the MOD process census (three readings) and the MOD-ORPHANS STOP, exit 5 (round 3) | 1113 |
| `tM-battery.sh` | PB | 1144 |
| `tM-battery.sh` | the S loop | 1179 |
| `tM-battery.sh` | NR x4 | 1233 |
| `tM-battery.sh` | TE / HS / UF, the restore after the sweeps | 1253 |
| `tM-battery.sh` | `tbs_named`, `tleg` | 1289 |
| `tM-battery.sh` | T:runtime/pprof, TBS-W, T:runtime | 1337 |
| `tM-battery.sh` | END, NONZERO LEGS, FINDINGS, the exit status | 1397 |
| `tM-helpers.py` | `S1_NONE`, `EXEC` (the annotation regex), `EXEC_RULED`, `LINUX_ANN`, `H3_TOKENS`, `FIXUP_SUBJ` | 79 |
| `tM-helpers.py` | `precheck`: its modes, the fixup commits it counts | 102 |
| `tM-helpers.py` | COUNT, then REG | 152 |
| `tM-helpers.py` | BOTH (per merge) | 183 |
| `tM-helpers.py` | ROSTER rows, linux annotations, SNAPSHOT, PROOF, execution annotations, H5 | 232 |
| `tM-helpers.py` | H3 (derived seats, `H3_TOKENS`) | 290 |
| `tM-helpers.py` | H4, markers, S1 census, optin | 309 |
| `tM-helpers.py` | `trx` (prints `TRX totals` first) | 385 |
| `tM-helpers.py` | `treport` | 407 |
| `tM-helpers.py` | `teattr` (with the `g-frame-only files` count), `hunkclass` | 616 |
| `tM-helpers.py` | `deadlock` | 721 |
| `tM-helpers.py` | `cmpnames` (the controls' reader, unchanged) | 749 |
| `tM-helpers.py` | `realmod` | 786 |
| `tM-helpers.py` | `hostwall`, `live` | 874 |
| `tM-helpers.py` | `execrows` (the ONE reader of a row's execution config), `execruled`, `rosterlinux` | 957 |
| `tM-emitcheck.sh` | the guards of a hand launch (`TM_LOCK_HELD`) | 45 |
| `tM-emitcheck.sh` | HANDOWN derived (the six conversions and the marker scan follow) | 83 |
| `tM-modules-legs.sh` | launch refusal, then the deadline | 75 |
| `tM-modules-legs.sh` | the LIVE gate and the lock (`KEEP_LOCK`) | 96 |
| `tM-modules-legs.sh` | the H1 / H2 seats read from the list, ONCE (`H2SHA` for the controls: round 3) | 162 |
| `tM-modules-legs.sh` | CM (the R-chain tests by name) | 195 |
| `tM-modules-legs.sh` | `ctlconv`: the control converter + the H2 seat's stream reader (round 3) | 210 |
| `tM-modules-legs.sh` | MR4c (the M3 control converter) | 370 |
| `tM-modules-legs.sh` | MR6c (the M4 control converter) | 431 |
| `tM-modules-legs.sh` | XS and XM (`rmrun`, `realread`) | 507 |
| `tM-modules-legs.sh` | the process census (three readings), the docs check, END | 630 |
| `tM-linux-legs.sh` | `mover`, `hpx` | 187 |
| `tM-linux-legs.sh` | the execution-config rows through `execrows`, `tiered_row` (round 3) | 202 |
| `tM-linux-legs.sh` | `EXPECT_V`, `OS_ROOT_EXPECT` | 216 |
| `tM-linux-legs.sh` | `tleg` (rc, counts, names, fresh record) | 268 |
| `tM-linux-legs.sh` | `rowleg`, then the DERIVED lists (GolibTests, converter tests, LB, host seats) | 316 |
| `tM-linux-legs.sh` | `gt`: the TRX reader gate (round 3) | 380 |
| `tM-linux-legs.sh` | P1: L1, L2, LCn / LCf, L4, LPB, LB, LM | 413 |
| `tM-linux-legs.sh` | LPB and UF gated (round 3) | 474 |
| `tM-linux-legs.sh` | LB: the build and each project gated (round 3) | 495 |
| `tM-linux-legs.sh` | LM: `lmread` with the F4 gate (round 3) | 543 |
| `tM-linux-legs.sh` | P2: the full runtime row (`RT_CHILD` 210m, `RT_OUTER` 450m) | 637 |
| `tM-linux-legs.sh` | TBS: a deadlock line per iteration is a MOVER (round 3) | 661 |
| `tM-linux-legs.sh` | END and the exit status | 673 |
| `tM-i9-shard.sh` | the helper beside the driver, `exrows` through `execrows`, the config rows (round 3) | 131 |
| `tM-i9-shard.sh` | the row loop (record deletion, infra rerun, named reading) | 197 |
| `tM-i9-shard.sh` | the ENV verdict gated positively (round 3) | 245 |
| `tM-i9-shard.sh` | the soft expectations and the exit status | 262 |
