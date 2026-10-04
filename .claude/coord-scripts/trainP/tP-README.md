# TRAIN P battery: draft summary (THE RELEASE TRAIN for go.* 1.24.13.4)

**DERIVED 2026-10-04 (12:16 to 14:15 Central) from TRAIN O's script set** (`hnd/.claude/coord-scripts/trainO/`, frozen
while O's battery runs: never edited) by a workflow agent, step 3 of the P derive (step 1: the candidate list,
`tP-seats-candidates-step1.txt`; step 2: the pre-map, `tP-premap.md`). It was read against O's battery as it stood at
the derive (`coord-scratch/tO/bat2`, launched 06:37, RUNNING, 0 findings through leg WEc at 12:16: a read-only copy of
its SUMMARY, taken once), the P pre-map on O's union `eb88ab9492` as a stand-in base, and COORD's notes
(`trainL/tL-seats-draft.txt` to its 13:35 line: handover tip 64734a5115 plus one uncommitted line in the hnd
worktree). **Nothing here was run for real.** What was run: git object reads through `/h/Projects/go2cs` (refs fetched by name), `bash -n` on every `.sh`, `compile()` on
every `.py`, the assembler's own table-check block and the name-derivation blocks against git objects, the pre-map on
the proposed table, and unit runs of the changed arms on synthetic inputs in the session scratchpad (section 8). O's
worktree `/h/go2cs-tmp-coord/tO` and N's were not touched. Every change against O is in `tP-CHANGES.md` (R0, P1 to
P13), the verification in `tP-DERIVE-REPORT.md`.

**REVIEW ROUND 1 (2026-10-04, 14:35 to 15:20 Central) is applied**: two reviewers' findings, one blocker (the else-if
row's 123 footprint tokens were written with a `src/core/` prefix, so `REGEN_ALLOW` admitted 9 of that row's 123
files), five should-fix items and the nits. What changed, where, and what was measured: `tP-CHANGES.md` section 8.
The table's rows did not move (`rows-sha256=8491acbec194`); its notes and comments did.

**Why P is the release train.** Owner ruling 2026-10-04 10:05: "OK to extend 1.24.13.4 for best release yet". The
published `go.sort` (1.24.13.3) holds a converter defect: `IntSlice`, `Float64Slice` and `StringSlice` `.Sort()` call
themselves (a stack overflow under Debug or tiered JIT, a spin under Release with tiering off). P carries the fix
(`g-sort-self-capture`), and the release is cut from P's landed master. So P's landing is followed by the release, and
P's gates are the release tree's gates (MS23, MS27).

**Naming.** O's manual steps keep their numbers where they are still procedure (MSn carries O's MSn); P's new ones are
**MS26** (the C# consumer runners) and **MS27** (the release sequence after landing). Seat notes: **P:n** =
`trainP/tP-seats-draft.txt` line n; **L:n** = `trainL/tL-seats-draft.txt` line n; **PM** = the pre-map, `tP-premap.md`.

Union: `/h/go2cs-tmp-coord/tP`, branch `claude/coord-trainP-union`, base **TRAIN O's LANDED master** (O's union
eb88ab9492 + a possible fixup-2 + its MS13 refresh). **It does not exist at the derive, so every script takes it as a
REQUIRED variable** (`BASE` in the assembler, the map, the fixup, the module legs and the two lane drivers; `MASTER` in
the battery) and no script of this set writes a base sha. One signed merge per row of the seat list, in row order.
**The list is the 22-row draft** (rows cut on eb88ab9492, on 59ee0d21bf, on an O row and on an O row's parent; tips
read by ls-remote 12:33, 13:05, 13:22, 13:43 and 14:00 Central, every tip at its seated sha). Its rows hash is
**`rows-sha256=8491acbec194`**: the 21 rows of 'P TABLE AS IT STANDS' in COORD's 13:35 note, in that order, plus
`g-xsync-bank` (accepted 13:48, 'a docs row near the top') as row 4. **It pre-maps with 0 conflicted rows at the
stand-in base** (`premap-run2/v8-eb88`, read again by review round 1 as `premap-run3/v8-eb88-rr1`; the two ruled
re-cuts R1 and R2 are both done). What is still open before the map (MS21): one acceptance read (`c2-ide-spike`),
**ONE RE-CUT was OWED (C2: go-cmp L as `claude/c2-reflect-newat-field-r2`, accepted in substance 14:05, stacked on
the else-if -r3) and ARRIVED during the review round: 92500eb260, pushed by 14:58. It reads as expected and trial
pre-maps clean as row 23 (`premap-run3/trial-L-r2`); it is NOT seated, because its acceptance at that sha is COORD's
(MS21.4 (iii))**; two slots (the BOARD docs row; L's -r2), and the re-read at the real BASE; each new row moves the
hash.

## 1. The order of work

| Step | Command (from a FRESH per-run copy of this folder + the seat list) | Wall |
|---|---|---|
| 0 | O lands. `BASE=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master \| cut -f1)` (read, never typed; floor 15), then the checklist's step-0 ASSERTS (O's union and its `refresh: TRAIN O` inside BASE; no train-assembly commit above BASE). Re-run the pre-map at BASE (`tP-premap-all.sh`: 6 and 12 minutes in this derive's two runs, beside O's battery), then MS9c's table controls and the real list: `BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 bash tP-assemble.sh` (EXPECT `TABLE OK`), then `BASE=$BASE SEATS=<the list> bash tP-conflict-map.sh`: the map keeps a fully clean union under `refs/coord/tP-map/rows-<hash>`; **the assembly REFUSES a list no map has read** | minutes |
| 1 | The manual steps of section 4 that come BEFORE the map, the assembly or the fixup: **MS21 (the acceptance; the slots)**, MS1 (walk the literal sites), MS14 (registration files), MS19 (the HashSet scan), MS20 (the template-class csproj ruling), MS22 | |
| 2 | `BASE=$BASE bash tP-assemble.sh` (fetch; `ls-remote` for the seat tips; signing preflight; the table; the map refusal; the merges; shape, ORDER and MAP TREE). Writes `coord-scratch/tP/assemble-maptree.txt` | minutes |
| 3 | Ruled follow-up merges, signed `--no-ff`, each listed in `tP-follow.txt`, each on a branch of its OWN: **none planned** | |
| 4 | `BASE=$BASE EXPECT_HEAD=<the assembled head> CSPROJ_TEMPLATE=accept bash tP-fixup.sh` (REGEN_ALLOW composes from the six footprint rows; `CSPROJ_TEMPLATE=accept` is COORD's 11:28 confirmation for the one predicted csproj; `EDITORCONFIG_NEW=accept` only by a ruling, MS22). **It dies in PRERES when the release fix is not in the tree (P9)** | O's two runs: about 1 h 35 m to the first stop; INFERRED the same |
| 5 | **The signature read as its OWN command, then push the union (a NEW ref: push, read back, THEN announce); GO to the i9 (`tP-lane-brief-i9.md`) and to P1/P2 (`tP-lane-brief-linux.md`)** with `UNION`, `FIXUP` and `BASE`, before the battery, so the lanes run beside it. **MS23: C1 dispatches the os-matrix release-smoke run AND the darwin behavioral FULL at the union** (CI; they run beside everything). G's pflag / cobra re-read at the union | |
| 6 | `MASTER=$BASE EXPECT_HEAD=<fixup sha, 10 chars> bash tP-battery.sh`, from the fixup's run folder or a copy holding the SAME `tP-seats-draft.txt` and `tP-follow.txt` | O's bat2: 06:37 to an expected 17:30-18:30 (11 to 12 h); P has 8 isolated projects where O had 18, one more S row (sort) and a longer WE arm B: INFERRED about the same; default deadline launch + 14 h |
| 6b | Only if a red needs a tree change AFTER the push (floor 9): `fixup-2: TRAIN P` ON TOP (`FIXUP_N=2`). Then section 4b; **a fixup-N that touches a release path owes MS23 again** | |
| 7 | Landing: MS23's verdicts read (release-smoke A-D on four RIDs with D gating; darwin FULL); merge master into the union (MS11), the landing precheck BEFORE the bank step, the bank step, **MS13 the refresh of committed -tests sources**, MS26, then master fast-forwarded | |
| 8 | **MS27: the release of go.\* 1.24.13.4 from P's landed master** (the release runbook; the version bump is the release's own step; the owner's PIN) | the owner's |

Per-run copy (floor 4): copy the folder's FILES and `controls/` (the checklist's line; never `premap-run*/` or
`step1-run1/`, which hold run output and scratch object directories), the seat list among them.
Every script refuses a launch from the worktree, from `hnd`, from any `wf/draft*` folder or from the main checkout.
The fixup, the battery, a standalone `tP-modules-legs.sh` and a hand-launched `tP-emitcheck.sh` take one shared lock
(`coord-scratch/tP/.battery.lock`) and, before it, read the LIVE gate (`tP-helpers.py live` reads every train's
`coord-scratch/t*/.battery.lock`, so an O battery still running refuses P's fixup; `LIVE_ACK=1` runs past a hit by
COORD's explicit call). The battery refuses a run folder that already holds a `SUMMARY.txt`.

Exit statuses (unchanged from O). Battery: 0 clean; **6** a leg outside the four expected-non-zero controls (FXc1,
FXc2, SIc, NVR) is non-zero, or a FINDING exists; 2 abort; 3 a leg's disk or build abort (a purge now retries six times
first: O's BATTERY STOP 1); **5** a wall cap fired, or a process is still running from the module out root after MOD:
the lock is KEPT; 9 the deadline. Assembly: 2 a table problem or no map for the list; 3 a merge that failed; 4 a shape,
order or MAP TREE miss. Module legs: 4 on any FAIL verdict. Linux driver: **4** on any MOVER or freshness fail. i9
shard: **4** on a failed row or a `SOFT:` line.

Environment variables (O's, with P's values and the changed rows):

| Script | Variable | Default | Meaning |
|---|---|---|---|
| assemble, map, fixup, modules, i9, linux | `BASE` | **required** | TRAIN O's landed master; must resolve, be on origin/master (assemble, map) and on the union's first-parent line (lanes: since review round 1 with nothing above it but TRAIN P's seat merges and fixups, so the stand-in eb88ab9492 is refused there too) |
| battery | `MASTER` | **required** | the same base; must sit on the union's first-parent line |
| battery | `O_RUN` | `coord-scratch/tO/bat2` (P3) | TRAIN O's battery run folder: its SUMMARY, S / T patches and host records are the previous-train baselines (FX floor, CNR control, TE / TE-T, HOSTWALL, TL-WALL), stamped in PRE. **`N_RUN` (O's name) is REFUSED when set** |
| map | `SEATS` | COORD's live `hnd` list | the list the map reads |
| assemble | `TABLE_ONLY` | unset | `1` stops after the seat-table check: nothing created, nothing merged (MS9c) |
| assemble | `TIP_MOVED_OK` | unset | refs whose REMOTE tip descends from the seated sha, acknowledged by COORD |
| assemble | `MAP_UNION` | derived | normally unset: the union `tP-conflict-map.sh` kept for the same rows |
| assemble, map | `BASE_BEHIND_OK` | unset | `1` runs past a BASE that has a train-assembly commit above it on origin/master's first-parent line: COORD's explicit call, stated (the default refuses a stale base such as O's union eb88ab9492 once O has landed) |
| assemble, fixup | `SIGN_PROBE` | `1` | `0` skips the real probe signature |
| fixup, battery, modules, emitcheck | `LIVE_ACK` | unset | `1` runs past the LIVE gate; stamped |
| fixup | `EXPECT_HEAD` | required | the assembled union head; with `FIXUP_N` >= 2, the pushed head |
| fixup | `FIXUP_N` | `1` | `k+1` = a fixup ON TOP of the k already at the head; subject `fixup-N: TRAIN P` |
| fixup | `PROSE_PATCH` | unset | a comment-and-prose patch; none is owed at P |
| fixup | `REGEN` | `apply` | `skip` leaves the corpus alone (COORD's explicit call) |
| fixup | `REGEN_ALLOW` | composed from the seat list | at the draft: **142 path tokens (133 distinct) of six rows' `CORPUS FOOTPRINT` notes** (sort 1, sibling 4, float-fold 1, p2-test-warning-entries 2, fm-record-r3 11, else-if -r3 123), stamped in PRE (MS4) with its own count line. **True since review round 1**: as first drafted the else-if row's tokens carried a `src/core/` prefix (142 distinct, 114 of that row's files refused). A token with the prefix is now refused at launch, and a token that is no file at HEAD dies in PRE. PREDICTED: nothing to apply |
| fixup | `SIBLINGS` | `report` | `refresh` also refreshes the `*.cs.auto` siblings only the union arm rewrites |
| fixup | `GOLDENS` | `regen` | `skip` leaves stale goldens for leg 4 to report |
| fixup | `GOLDEN_CLASS` | `gframe` | `any` re-baselines a moved golden that carries another line kind, after COORD has read it (MS4b; O needed it) |
| fixup | `CSPROJ_TEMPLATE` | `stop` | `accept` carries the csproj the union added that its CNR regenerated with the union template's delta alone: ONE predicted, `LiteralFloatConstFold.csproj` (MS20; COORD confirmed accept 11:28) |
| fixup | `EDITORCONFIG_NEW` | `stop` | (MS22) `accept` carries a NEW per-file `.editorconfig` the union CNR writes in a behavioral project no seat gave one; 0 predicted |
| fixup | `EXP_DESC`, `EXP_TH` | unset | optin.py check's two counts; unset = reported (TO BE SET BY COORD) |
| fixup, battery | `EXEC_ROWS_EXPECT` | the count of `EXEC_RULED` | **0** (as at N and O) |
| fixup, battery | `SEATS_EXPECTED` | unset | optional second derivation of the seat count |
| battery | `EXPECT_HEAD` | required | the fixup commit, or the newest `fixup-N` on top of it |
| battery, modules | `DEADLINE` | launch + 14 h | an ABSOLUTE local time `'YYYY-MM-DD HH:MM'` |
| battery | `CNR_EXPECT_N` | **derived** | PRE-D derives it with `cnrexpect` at HEAD and stamps its reconciliation (PREDICTED 834 at the draft) |
| battery | `E_BISECT_SEATS` | **derived (P4)** | unset = the list's `CORPUS FOOTPRINT` rows that are converter seats (five at the draft: the sort seat, the sibling seat, the float fold, fm-record-r3, the else-if -r3), bisected only if leg E reads above 0, about 46 m an arm; a list at launch overrides (validated); `none` = no arm |
| battery | `WE_FLAVOURS` | `linux,windows,darwin` | leg WE's flavours |
| battery, modules | `REALMOD_TEST_TIMEOUT` | `2m` | the real-module legs' `-test-timeout` |
| battery | `REALMOD` | `1` | `0` skips XS/XM by COORD's explicit call |
| linux P2 | `RT_CHILD`, `RT_OUTER` | `210m`, `450m` | the full runtime row's deadlines |
| battery | `PRECHECK_MODE` | `abort` | `warn` runs past a failed PRE assert, stamped |
| battery | `I9_PATCH`, `I9_BASELINE` | unset | the i9's TE patch; its TRAIN O patch as the baseline (`coord-scratch/tO/i9-patches/tO-tracked-changes-U0.patch`) |
| battery | `SWEEP_ROW_CAP` | `4h` | outer wall cap per sweep row |
| emitcheck | `TP_LOCK_HELD` | unset | set to `1` by the fixup and the battery |
| premap | `GITX` | the Visual Studio git (2.55) | a git >= **2.44** (`merge-tree --write-tree` is 2.38's, `merge-file --diff-algorithm` is 2.44's: review round 1 raised the gate and reads merge-file's rc); the box's default git is 2.35.2 and the pre-map aborts by name without one |
| premap, regcheck, unioncheck | `GD` | `H:/Projects/go2cs/.git` | the shared git directory; since review round 1 the two python readers take it from the environment too |
| fixup, emitcheck | `W` | `/h/go2cs-tmp-coord/tP` | overridable only to a rehearsal worktree `/h/go2cs-tmp-coord/tP-<name>`: any other value is REFUSED before anything runs (review round 1: a `W` left exported from O's shell) |

## 2. Why the union owes a fixup, and what it is predicted to carry

| | Fact | What breaks without it | Where |
|---|---|---|---|
| T1 | `LiteralFloatConstFold.csproj` carries the pre-N template (c2-literal-float-fold was cut on an O row that sits on M's master; TrimMode count 0, read with git; the other 15 added behavioral csproj hold the current template) | step 5 dies 'CNR moved a csproj' | `csprojtemplate` classifies it from the template file's delta since its cut (D2); `CSPROJ_TEMPLATE=accept` (confirmed 11:28). PREDICTED verdict `files=1 template-only=1 other=0` |
| G1 | Goldens a seat x seat interaction moves. Likely at P: the sort seat's cast, H-2's and H-1's conversions (one file), the three import / namespace rows, and two position-map rows each meet projects born on other rows | leg 4 reads them CHANGED | fixup step 5: the union's CNR measures; `GOLDEN_CLASS=gframe` STOPS on any non-G-frame hunk; COORD reads `gold-cs.patch` and relaunches with `GOLDEN_CLASS=any` (MS4b). O needed exactly that for two goldens |
| G2 | Corpus: every one of the six footprints is committed in-seat, the else-if -r3 on a converter carrying fm-record-r3's change | nothing predicted (an EMPTY regeneration). A path the union re-emits differently is regenerated when it is in REGEN_ALLOW and STOPS the fixup otherwise (MS4). **c2-named-basic-conv's footprint was never measured by a reconvert** (a census predicted 0): step 4 is its first measurement | fixup step 4 |
| REL | **THE RELEASE FIX** is three lines of `src/core/sort/sort.cs`, a file in REGEN_ALLOW: a seat x seat interaction that made the union converter emit the old self-call would be COPIED IN by step 4 | the release would ship the defect it waits to fix, with leg E reading 0 | **P9**: the fixup's `relfix` dies in PRERES and again at step 6 unless the worktree's file holds 0 bare `Sort(x);` lines and the three methods; the battery's PRE-D raises a FINDING on the same predicate. Control: the base reads 3 |
| W1 | `i9-incremental-cs-writes-r2` is in the union's converter: a single-target conversion leaves an unchanged source unwritten, so `written-U-<os>.txt` no longer means 'the targets that emit this path' | `tP-regen-apply.py` rule 4 would have passed a flat file only ONE target emits differently, and copied it over the shared location | **P10**: rule 4 also reads the base arm's written lists (TRAIN O's converter writes every source). Measured on synthetic trees: refused; O's reader accepts the same evidence |
| E2 | A committed `.editorconfig` a conversion DELETES (O's MS24 class) | leg E reads it union-attributable; step 4 REFUSES it by name | none predicted: p2-test-warning-entries' two files hold `_test.cs` sections only, which a `-stdlib` run does not own and does not remove (warningEntries.go: ownership by run kind) |
| META | fm-record-r3 and the else-if seat change GoPositionMap lines; the sibling seat changes 4 `using` and 4 `namespace` lines of package_info.cs | none expected | fixup PRERES reads `TestStdLibMetadataInSync` by name; a STALE asset is regenerated and rides |
| H3 | `g-debugger-views` edits golib's Goroutine.cs (+3 lines) | precheck hard-fails 'H3 seat ... holds no identifier for it' | **P8**: `H3_TOKENS = {'g-debugger-views': ['[System.Diagnostics.DebuggerNonUserCode]']}` (0 at the base, 1 at the seat: read with git) |
| S1 | `ConsumerUsings.csproj` (the smoke re-cut's hand fixture) names no LangVersion | precheck hard-fails 'S1 UNLISTED-NONE' | **P8**: `S1_NONE` holds it. `CSharpConsumer.csproj` is conditioned in the re-cut; c2-ide-spike's two csproj are under docs/ (skipped) |
| R1 | `c2-sibling-package-name-r2` renames a behavioral sub-package: go2cs.slnx loses one key | nothing (the REG arm knows seat-removed keys; it reads keys, so no rename detection is needed) | PREDICTED `seat-removed=[c2-sibling-package-name-r2:tests/Behavioral/AliasNamespaceShadow/sortlocal/AliasNamespaceShadow.sortlocal.csproj]` on go2cs.slnx |

## 3. Files

| File | What |
|---|---|
| `tP-seats-draft.txt` | The proposed P table (22 rows: pre-map order v5 + the i9's crash-verdict row in its slot + G's go.go -r2 in place of the original + G's x/sync bank docs row as row 4; slots, hazards and NOT-P as comments). DRAFT for COORD. The step-1 candidate list is `tP-seats-candidates-step1.txt`. |
| `tP-premap.md`, `tP-premap.sh`, `tP-premap-all.sh`, `tP-premap-controls.sh`, `tP-regcheck.py`, `tP-unioncheck.py`, `tP-gofmt-parse.sh`, `tP-footprints.sh`, `tP-hunks.sh`, `premap-run1/` | The worktree-free pre-map (this workflow's step 2), not edited by step 3. **Review round 1 edited five of them** (`tP-premap.sh`: git >= 2.44 and merge-file's rc; `tP-regcheck.py`: GD from the environment, a scratch object directory for a multi-base merge, more than two bases refused; `tP-unioncheck.py`: the duplicate-declaration census by the battery's lexical scan, with its own control; `tP-gofmt-parse.sh`: blobs kept by path; `tP-premap-controls.sh`: P13's class planted) and re-ran the controls and the table: `premap-run3/` (`tP-CHANGES.md` sections 2b and 8). `premap-run2/` holds this step's re-runs (v5: 20 rows; v6: 21 rows with the original go.go seat; v7: 21 rows, 0 conflicted; **v8: the 22-row table of record, 0 conflicted rows**; `trial-slots/`: 23 rows, the bank row and the unaccepted go-cmp L ref read in their slots). |
| `tP-conflict-map.sh` | The map; base REQUIRED; refuses a row carrying another train's assembly commits. |
| `tP-assemble.sh` | Assembly; base REQUIRED and on origin/master; the table checks; the literal-site list. |
| `tP-follow.txt` | The ruled follow-up merges: EMPTY. |
| `tP-fixup.sh` | The fixup: O's steps + `relfix` (P9) + the step-6 purge retry (P7). |
| `tP-regen-apply.py` | Copies the union converter's emission over listed corpus files; rule 4 under incremental writes (P10). |
| `tP-battery.sh` | The i7 battery: O's legs; MASTER REQUIRED; O_RUN baselines (P3); E-bisect default derived (P4); UF's known class (P6); X_ROWS + sort (P12); PRE-D release fix (P9); CB's lexical test-name scan (P13); the P leg map and predictions. |
| `tP-helpers.py` | O's readers; H3_TOKENS and S1_NONE entries (P8); labels. |
| `tP-emitcheck.sh`, `emitdrift.py` | Leg E; texts only (P10's reading note). |
| `tP-modules-legs.sh` | Leg MOD; BASE REQUIRED; the host-seat marker is a phrase (P11). |
| `tP-i7-sweeps.txt`, `tP-i9-shard.txt` | **93** and **132** rows, byte-identical to O's (sha256 40c6ec39...3724 and 057c78c1...cdf3d, `cmp`). `sort` is an i9 row; the i7 reads it too through X_ROWS. |
| `tP-ng-parse.ps1` | L's ParseFile reader (header only). |
| `tP-i9-shard.sh`, `tP-i9-te.sh`, `tP-lane-brief-i9.md` | The i9 driver (UF known class P6, purge retry P7, the sort named reading P12), its TE reader, its brief. |
| `tP-linux-legs.sh`, `tP-lane-brief-linux.md` | The P1/P2 driver (LCn's test-name scan P5 + P13, with its own control on the lane's awk; host-seat marker P11) and brief. |
| `COORD-LAUNCH-CHECKLIST.md` | From the seat table to master and the release, with P's EXPECT lines. |
| `controls/` | M's two TE patches (byte copies) and the controls note. |
| `tP-CHANGES.md`, `tP-DERIVE-REPORT.md` | Every change from O; the verification. |

## 4. Manual steps (a check can name them; a person decides them)

- **MS1. The seat list; walk the literal sites when a row is added, dropped or re-pointed.** Re-run the pre-map and
  the map BEFORE the assembly reads the list. **DERIVED at run time**: the converter tests the union adds (CB, LCn, both
  kept by the lexical scan, P13), the GolibTests classes (GT), the behavioral projects (legs 4, 5, LB), CNR's N and its
  platform-exclusive set, the nugetgo test scripts (NGa), the execution-config rows, `REGEN_ALLOW`, the map union, the
  host seats, the darwin annotation expectation, **the E-bisect default (P4)**. **Still a literal: walk it:**

  | File | Literal | What it is at P |
  |---|---|---|
  | `tP-helpers.py` | `EXEC_RULED` (empty), `H3_TOKENS` (**keyed by the ROW NAME `g-debugger-views`**: a re-point under another ref name moves the entry), `S1_NONE` (+ `ConsumerUsings.csproj`) | the ruling; Goroutine.cs editors; hand csproj |
  | `tP-fixup.sh` | `REGEN_G_SEAT`, `REGEN_G_PKGS` (empty); `RF_SELF`, `RF_DECL` (the release-fix predicate) | a named seat's uncommitted footprint; P9 |
  | `tP-battery.sh` | `CB_BASE`, `GT_NEIGH`, `GT_TESTS`, `CHECKDEAD_GUARDS`, `NEIGH`, `X_ROWS` (`encoding/json sort`), `T_ROWS`, `HS_WANT`, `NPOST` (P's 16 directories), `PUB_MIN` (6), `IDC_MIN` (217), `WE_FLAVOURS`, `UF_KNOWN` and the `p2-test-warning-entries` ref prefix, `RF_SELF` / `RF_DECL`, the floors 24 / 68 / 26 / 62 / 1291, the fallbacks 819 (CNR) and 1291 (FX) | as at O, plus P's |
  | `tP-linux-legs.sh` | `GT_NEIGH`, `LB_FIXED`, `LX_KNOWN`, the L4 / L5 / L6 rows, `OS_ROOT_EXPECT` | the lanes' hazard sets |
  | `tP-i9-shard.sh` | the `named` cases (+ `sort`), `UF_KNOWN` | test names read by name |
  | the seat list's notes | `stack-on`, `after`, `CORPUS FOOTPRINT n files (...)`, `host seat H1` / `host seat H2` | the grammar the scripts parse (P:3) |
- **MS2. H4.** No P row adds a Go-only behavioral directory (the pre-map's unioncheck read 0 with 16 directories
  added). A slot that adds a repro without its emission would fire PRERES's H4 arm: it is a re-cut with committed
  emission and registrations, never a follow-up.
- **MS3. DROPPED** (as at N and O): the roster sentence reads 'No row opts back out' with 0 annotated rows.
- **MS4. Read the `REGEN_ALLOW` the fixup stamps.** At the draft it admits exactly the six footprint rows' paths
  (`src/core/<path>`; generated `.cs` and per-file `.editorconfig`). **A group's paths are RELATIVE to src/core**
  (review round 1): a token written `src/core/...` composes a dead alternative, so the fixup refuses it at launch, and
  every composed token must be a file at HEAD or the fixup dies in PRE naming the row and the token (the count line
  reads `142 footprint token(s), 133 distinct, from 6 row(s)` at the draft). Any OTHER union-attributable path stops the fixup
  before a byte is written, with the evidence kept (`/h/go2cs-tmp-coord/tPemitfix`): COORD reads the list, then relaunches
  with `REGEN_ALLOW=<pattern>` or routes the path back to its seat. The rule-refused classes are O's: a NEW emitted path
  (rule 2), a per-target difference of one path (rule 4, **now also a path only SOME targets moved: P10**), a committed
  `.editorconfig` a conversion DELETED (MS24). **Outside the grammar by design at P:** the sibling seat's two
  production csproj and its 24 committed `-tests` files. A csproj the union re-emits differently is a STOP to read
  (never copied); `-tests` files are not `-stdlib` emission (the sweeps read them: TE, then MS13). **If step 4 lists
  `src/core/sort/sort.cs`, READ `regen.patch` before anything else**: it is the release fix's file (`relfix` at step 6
  dies if the defect's line is back, P9).
  **MS4b:** `GOLDEN_CLASS=gframe` stops on the first moved golden: READ `gold-cs.patch`, then relaunch with
  `GOLDEN_CLASS=any`.
- **MS5. Review siblings and hand-owns.** `SIBLINGS=report` (RULED). HANDOWN must read 0 written:
  `runtime/managed_impl.cs` (fm-record-r3), `testing/TestHost.cs` (the i9's crash-verdict row; `testing` is a hand-owned
  package) and golib's files (g-debugger-views, fm-record-r3's GoPositionMapAttribute.cs) are the hand-owned files P
  rows edit.
- **MS7. `SetegidBroadcastSeam` is never re-baselined whole.** Linux-exclusive; P1's LB and LX read it.
- **MS8. If the row order changes**, adjacent inserts in a registration file can become one conflict hunk (MS14).
- **MS9. Run the two TE control patches** through `teattr` and `hunkclass` once before the first battery (checklist 2).
- **MS9b. Run the precheck's and the LIVE gate's controls once before the first fixup** (floor 13). PREDICTED at the
  assembled draft list:
  1. precheck on the assembled union, `head` mode: rc 0, no NOTE; `ok COUNT` x6 (go2cs-src.projitems +18, go2cs.slnx
     +15 net, each of the four lists +24, against whatever BASE holds: O's bat2 read 491 / 1110 / 2385 / 2380 / 2435 /
     2381 at eb88ab9492), no identical-insert credit and no `COUNTBASE multi-base row` line; `ok REG ... go2cs.slnx ...
     seat-removed=['c2-sibling-package-name-r2:tests/Behavioral/AliasNamespaceShadow/sortlocal/AliasNamespaceShadow.sortlocal.csproj']`,
     `seat-removed=[]` on the other five; `ok ROSTER rows base=225 merged=225 ... none`; `ok ROSTER linux annotations
     base=223 merged=223 ... none`; `ok ROSTER darwin annotations base=22 merged=22 expect=22 (the base + 0 row-own
     edit(s): none) mismatched: none`; **`ok H3 ... [[System.Diagnostics.DebuggerNonUserCode]]=1 (want 1,
     g-debugger-views)`** beside the four master identifiers; `ok H4 ... 0`; `ok S1 census`.
  2. The same command on a child worktree whose HEAD is BASE: PREDICTED rc 1 with one `FAIL BOTH <seat>: no first-parent
     merge` per row, FAIL COUNT / REG on the registration files, and **`FAIL H3 ...
     [[System.Diagnostics.DebuggerNonUserCode]]=0 (want 1, g-debugger-views)`**: the arm takes WHICH rows edit
     Goroutine.cs from the list (each row's own diff), so on the base the token is wanted and absent (read from the
     code: that is the H3 entry's negative control). The OTHER H3 refusal, a row that edits the file with no entry, is
     `FAIL H3 seat g-debugger-views edits ... holds no identifier for it`: run it once on a COPY of the helper with
     `H3_TOKENS = {}`, on the assembled union.
  3. The LIVE gate with a planted `tZ/.battery.lock`: rc 1 and one `LIVE lock ...tZ...` line; removed: rc 0, `none`.
  4. `csprojtemplate` is run by the fixup itself; PREDICTED `files=1 template-only=1 other=0` with one `template delta
     at the cut` line naming the TrimMode comment and line.
- **MS9c. Run the seat table's controls once before the first assembly** (floor 13), from a per-run copy with
  `BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<planted copy>` (nothing is created whatever the table says):
  1. a duplicated sha (append `dup-control|b719825826|control`): a no-remote line and `the same sha as row 6
     g-sort-self-capture`;
  2. a stacked row above its base (move c1-release-smoke-sort-arm above g-sort-self-capture): `names no EARLIER row`
     and `is an ANCESTOR of the earlier row`;
  3. a landed row (append `landed-control|cca633f1ba|control`, O's g-untyped-float-operators, an ancestor of BASE):
     `already an ancestor of the base`; 3b. an `after` naming a later row;
  4. **(O2's positive control) `BASE=54f7f4439d BASE_BEHIND_OK=1`** (N's landed master: an ancestor of origin/master,
     with O's whole train above it): EXPECT one `carries 39 train-assembly commit(s) the base ... lacks` line for each
     row cut on O's union (measured with git on p2-cli-version-diagnostics-r2: 39 = O's 38 seat merges + its fixup),
     and NONE for the rows cut on 59ee0d21bf, on an O row or on an O row's parent (c2-ide-spike, g-debugger-views,
     r-csharp-consumer-guide, the smoke re-cut, c2-literal-float-fold: their stated blind spot);
  5. **(the BEHIND check's control)** `BASE=eb88ab9492` (O's union, on origin/master once O lands, with O's refresh
     above it) WITHOUT `BASE_BEHIND_OK`: EXPECT `ABORT: BASE eb88ab9492 is BEHIND a landed train ... (... refresh: TRAIN
     O ...)`, rc 2, from the assembler AND from `tP-conflict-map.sh`. This is the likely slip: every P seat was told to
     cut on eb88ab9492.
  Then the real list: EXPECT `TABLE OK: <n> rows ... rows-sha256=<h>` (read `<h>` from the line; the derive's read-only
  run of the same block at the stand-in base read `TABLE OK: 22 rows ... rows-sha256=8491acbec194` and six declared
  stacks).
- **MS10. Darwin is a mac CI dispatch**, not a leg of any box here; so is the release gate (MS23).
- **MS11. At landing, after merging master into the union:** `git diff --stat $BASE <that master sha>`, read whole;
  then `python -B tP-helpers.py precheck <tP> <that master sha> <run>/tP-logs/seats-effective.txt head` and
  `check-roster-format.ps1` under both editions with `EXEC_ROWS_EXPECT=0`, **BEFORE the bank step**.
- **MS12. DROPPED** (no prose owed).
- **MS13. THE REFRESH OF COMMITTED -tests SOURCES: a POST-BATTERY BANK STEP, as at N and O** (`tP-helpers.py
  testsrc-refresh`, unchanged): inputs = the WINDOWS rewrite patches of P's own runs at ONE union head
  (`tP-logs/S-rewrites.patch`, `tP-logs/T-rewrites.patch`, the i9's `tracked-changes.patch`); filter = committed test
  sources only; apply on the union after the battery, `--check-worktree`, one signed commit `refresh: TRAIN P -- ...`
  before master's fast-forward. PREDICTED small; `c2-anon-struct-named-conv` names one line in the `unique` test
  variant. **The release is cut from the tree that holds this commit.** In the no-row UF case
  `src/core/math/bits/.editorconfig` sits untracked in the worktree: read `--check-worktree` with that in mind.
- **MS14. The registration files.** go2cs.slnx and the four BehavioralTests files take inserts from 8 rows (8 projects,
  16 csproj); go2cs-src.projitems from 12 rows. KEEP EVERY SIDE'S INSERT. The line-count invariant is precheck's COUNT
  arm, the key union its REG arm, the per-merge line check its BOTH arm. The pre-map's chain read every registration
  file at expect on eb88ab9492, and no two rows insert at one point. No P row touches the BOARD; the BOARD docs row is
  COORD's slot.
- **MS15. PUB's context.** A regression leg at P: O's bat2 read rc 0 and 6 of 6 in 629 s (its first real reading on a
  union; N's never started). PUB_MIN stays 6.
- **MS16, MS17, MS18. DROPPED** (N's rows).
- **MS19. The HashSet scan.** The converter requires `github.com/ritchiecarroll/hashset v1.0.0` since O (in BASE; the
  i7's module cache holds it). An unqualified `HashSet[` / `NewHashSet` a P row adds would merge clean and not compile.
  The pre-map's unioncheck read 0 at its chain head (the i9's as-cut row held 2; its -r2 writes `hashset.NewHashSet`).
  Re-read at BASE with `tP-premap-all.sh`, or:
  ```
  for r in $(grep -E '^[a-z0-9.-]+\|' tP-seats-draft.txt | cut -d'|' -f1,2); do sha=${r#*|}; mb=$(git merge-base $BASE $sha)
    n=$(git diff $mb $sha -- 'src/go2cs/*.go' | grep '^+' | grep -v '^+++' | grep -cE '(^|[^.A-Za-z_])(HashSet\[|NewHashSet\b)'); [ "$n" = 0 ] || echo "${r%|*} $n"; done
  ```
- **MS20. The template-class csproj: COORD's ruling before the fixup's step 5.** ONE csproj at P
  (`LiteralFloatConstFold.csproj`); confirmed accept at 11:28: pass `CSPROJ_TEMPLATE=accept`. Stated limit: the renamed
  `AliasNamespaceShadow.sort.csproj` is not in the 'added by the union' list (git reads a rename), so a CNR move of it
  would read outside the class; none is predicted.
- **MS21. BEFORE THE MAP: one acceptance, ONE RE-CUT OWED (C2: go-cmp L -r2, item 4 (iii)) and two slots (the ruled
  re-cuts R1 and R2 are DONE).**
  1. **R1 (DONE, P ACCEPTED 13:35)**: `g-go-namespace-shadow-r2` 05b61c616a, two commits on the sibling row's tip
     6f54f28be2 (single parent), replaces c5cb51d3b6. Read with git at the seat and at the pre-map's chain head:
     `importAliasOperations.go` keeps BOTH if blocks, each with its own closing brace (**504** lines = 473 + 19 + 12;
     `packageKeepsDirectorySegment(` 1 and `RootNamespace+"."+RootNamespace` 2; gofmt parse errors 0 at the chain
     head); its `shadowing.md` section is inserted below base line 695 (hunk `-695,0 +696,33`; review round 1: not
     'at line 693'), away from the sort seat's tail section (below 729) and the embed row's (below 405) (the merged file reads 809 lines = 733 + 27 + 33 + 16 by the pre-map's count). The table seats it with
     its stack token; the pre-map's step for it (row 11) reads clean. Its gates at the stacked tip: GoHostModuleShadow 4/4 and
     AliasNamespaceShadow 4/4, CNR 825 (COORD's note). NOT read by anyone: the -r2 beside c2-alias-table-same-name
     (the third import / namespace row): the union's leg 5 and the go-cmp / testify / yaml.v3 re-reads are that reading.
  2. **R2 (DONE, P ACCEPTED 12:37)**: `c2-elseif-position-record-r3` e00629855f, stacked on
     g-method-value-fm-record-r3 07b53bc99d as ruled 11:28 (three commits; the same 123 paths; five added lines differ
     from the -r2, each carrying both changes). The table seats it. The pushed `-r2` 2753954c39 must NOT be seated.
  3. **Acceptance owed, one**: `c2-ide-spike` a4bfed39f5 (no P ACCEPTED line names it; docs only). Accepted since the
     pre-map and seated: `i9-tests-host-crash-verdict` 3c9c2a74df (12:37; the 21st row, directly below the i9's -r2),
     `c1-release-smoke-sort-arm` c49e2b7063 (13:07, on its green run 37218568547), `p2-test-warning-entries` 8d89695095
     (13:08).
  4. **Slots** (the table's SLOT comments; COORD's 13:48 note: 'OPEN SLOTS: COORD BOARD docs row; C2 go-cmp L').
     (i) COORD's P BOARD docs row (cut on BASE once O lands): NO REF. (ii) G's x/sync bank is NO LONGER a slot:
     `g-xsync-bank` 51f5b68770 was P ACCEPTED 13:48 and is row 4 (one commit on eb88ab9492, docs only:
     `docs/ValidatedModules.md`, five pages under `docs/validation/modules/golang.org/x/sync@v0.19.0/`, one line of
     `docs/Background.md`; 7 files +186/-1; it OWES, once the union is pushed, G's x/sync re-read (28/28) and a
     docs-site build at the union). (iii) **C2's go-cmp L: ACCEPTED IN SUBSTANCE 14:05, ONE RE-CUT OWED (C2)
     before it seats** (COORD's post cb0a997dcb): `claude/c2-reflect-newat-field-r2` STACKED ON
     c2-elseif-position-record-r3 e00629855f, pushed and read back by the freeze (about 02:00Z 10-05). EXPECT at the
     -r2, as the post words it: red 7f62cb16b8 and green fc64167f28 carry over byte for byte; exactly ONE line of
     `reflect/package_info.cs` differs from dec8cee4b4's corpus commit and `reflect/value.cs` is unchanged ('say so or
     say what moved'); go test, CNR, the ReflectNewAtField guard and GolibTests' five re-read at the -r2 tip; the
     canaries read at dec8cee4b4 stand for the -r2, their remaining lines posted as they finish (C2's 19:08Z post
     41bb594fdc: crypto/cipher 27272 and crypto/tls 1341 on the non-BoGo path MET, go/types 574 earlier; runtime in
     compare, the net/http rerun owed). **State at 15:07: the -r2 is PUSHED, 92500eb260 (C2's post c119c85a2d,
     14:58).** Read with git against that EXPECT: e00629855f is its ancestor, three commits (red eca3603bbd, green
     18bdcf71cb, corpus 92500eb260), no merge; red and green patch-ids EQUAL the originals'; `reflect/value.cs` is the
     original's blob; the corpus commit is ONE line of `reflect/package_info.cs`, the `value.go` map line (against
     dec8cee4b4's FILE three lines differ: the else-if row's own three, one of them that line); every other file's
     blob equals the original's; 18 files +621/-24 on e00629855f. **Trial pre-map, the 22 rows + this ref as row 23
     (`premap-run3/trial-L-r2`, NOT a seat table): `conflicted-rows=0 regcheck-of-record=ok
     skip-head-parse-errors=0`, step 23 clean (5 content merges: slnx and the four lists), 118 both-changed pairs, 0
     conflicting, head a9f04ec55b, tree 81e6b28012c9.** C2's gates at the tip (linux, alone): go test rc 0, CNR 818 NO
     REGRESSION, ReflectNewAtField 4 phases, GolibTests' five. **STILL OWED before it seats: COORD's acceptance AT
     THIS SHA; the runtime canary (its first run was NOT MEASURED: the host hit its 20 m package timeout under load,
     all 69 mismatches Go pass / C# never reached; C2 reruns it alone at 60 m) and net/http.** The table carries its
     row written out as a COMMENT below the slot line: seating it is removing two characters, and gives 23 rows,
     `rows-sha256=a9b5da13f551`, a footprint composition of 144 tokens / 134 distinct / 7 rows. Until then it stays a
     slot. The ORIGINAL ref is pushed and is never a seat (`claude/c2-reflect-newat-field`
     dec8cee4b4, three commits on eb88ab9492: golib `GoReflect.InteriorAlias.cs` +37, the hand-own
     `reflect/value_impl.cs` +32/-7 (git numstat; re-measure at the -r2 tip), converter `manualTypeOperations.go` +5, corpus `reflect/value.cs` and
     `reflect/package_info.cs`, behavioral `ReflectNewAtField` (new), GolibTests `ReflectNewAtResolutionTests.cs`;
     18 files +621/-24; IN only with route #7 + the canaries + the full behavioral suite read by the freeze).
     **L is not seated here.** It was read IN ITS SLOT by a trial pre-map (23 rows: the table of record + L,
     `premap-run2/trial-slots/`, `tP-DERIVE-REPORT.md` section 3). **L CANNOT RIDE AS CUT: it merges clean
     in its slot, and the LAST row then conflicts**, `c2-elseif-position-record-r3` x L in
     `src/core/reflect/package_info.cs` (one hunk: both re-encode the ONE `reflect/value.go` position-map line; the
     class of R2). **RULED 14:05** (COORD's own pairwise read the same one conflict): a re-cut -r2 STACKED ON
     c2-elseif-position-record-r3 e00629855f (the corpus commit re-measured there, the map line carrying both
     changes), placed LAST; 'if the battery reds on this seat it leaves the union by a revert on top; the release
     does not wait' (Q16: what that revert must look like to these scripts). The full behavioral suite was NOT run at
     the seat (disk): leg 5 is its first reading. What seating it moves, so
     COORD can walk it (MS1): its notes need the stack token and a footprint group for its two corpus files
     (`reflect/package_info.cs`, `reflect/value.cs`), which makes it a sixth E-bisect arm; CNR 834 -> 835 and 9
     isolated projects (the trial read `ReflectNewAtField` registered in slnx and the four lists, current template);
     GT gains `ReflectNewAtResolutionTests`; MS25 re-opens (read the merged `value_impl.cs` whole; the reflect canaries
     are its gates). Each new row: its notes' tokens, the literal walk, the pre-map, the map.
  After any of these: re-run `tP-premap-all.sh` with the new list (EXPECT `conflicted-rows=0`), MS9c, the map, MS19.
- **MS22. A NEW per-file `.editorconfig` in a behavioral project: COORD's ruling before the fixup's step 5.** If the
  union CNR writes one for a project no seat gave one, step 5 STOPS naming it (`edconf-new.txt`);
  `EDITORCONFIG_NEW=accept` carries it. 0 is the likely reading (the 8 new projects were cut on a converter that
  already writes them).
- **MS23. THE RELEASE GATE IS TWO CI RUNS AT THE P UNION, DISPATCHED BY C1, READ BEFORE LANDING.** Nothing on the i7
  runs `release-smoke.ps1`, the per-flavour pack or the walkthrough arm D, and nothing here runs darwin. P is the tree
  1.24.13.4 is cut from, and it reaches the PACKED tree in at least six places: `sort/sort.cs` (the fix), the
  production namespace of two corpus packages AND the namespace every converted third-party package named differently
  from its directory gets (the sibling seat's rule; hyphenated directories are not exempt, so the walkthrough's
  `mattn/go-colorable` and `mattn/go-isatty` move: **arm D on every shipped RID is that rule's release reading, and
  both green release-smoke runs so far predate it**: Q6), `go.lib.targets` (the smoke re-cut: global usings for every
  C# package consumer), golib's debugger views and `GoPositionMapAttribute`, go.testing's `TestHost.cs` (the i9's
  crash-verdict row), and `release-smoke.ps1` itself (C1's arm).
  Once the union is pushed (and again after any `fixup-N` that touches a release path: section 4b):
  ```
  gh workflow run os-matrix.yml --ref claude/coord-trainP-union -f goos=windows -f stage=release-smoke
  gh workflow run os-matrix.yml --ref claude/coord-trainP-union -f goos=darwin  -f stage=behavioral-full
  gh run list --workflow os-matrix.yml --branch claude/coord-trainP-union --limit 4     # the run ids; then: gh run watch <id>
  ```
  EXPECT, release-smoke (O's reading at eb88ab9492, run 37193550625: one pack, 344 packages, 0 failed): the pack job
  green; **A, B, C and D PASS on all four RIDs (win-x64, linux-x64, osx-x64, osx-arm64), D GATING on all four**; and
  **arm B now exercises the three `.Sort()` forms and the bare form** (c1-release-smoke-sort-arm): its control at O's
  union + the arm read B FAIL '(a STACK OVERFLOW)' on all four RIDs (run 37212642354), so a green B at P is the release
  fix read from the PACKED go.sort on every shipped RID. EXPECT, darwin FULL (O: arm64 766/766, x64 767/767, run
  37193552307): every measurable project passes on both Macs, the 8 new ones among them. A red in either holds the
  landing. The pack count moves only if a P row adds a packable project (none is known: read the pack job's line).
  **The sort fix's acceptance, in full:** (a) these runs' arm B; (b) `SortMethodSelfCapture` on the i7 (leg 5,
  isolated), on linux (P1's LB) and on both Macs (darwin FULL); (c) the `sort` row at its banked count on the i9 shard
  and, as an X row, on the i7 (a regression reading: sort's own suite never calls the three methods); (d) **G's own
  acceptance: the sort row sweep and the pflag / cobra re-read at the union** (pflag 0 -> 177 at the seat; cobra root
  runs); (e) the scripts' tree assertion (P9).
- **MS24 (carried from O). A committed `.editorconfig` a conversion DELETES** is listed by emitdrift.py (`DELETED`),
  read union-attributable by leg E and REFUSED by name in the fixup's step 4 (the fixup never applies a deletion: floor
  8); the remedy is a ROW that commits the deletion. None is predicted at P (section 2, E2).
- **MS25. DROPPED** (O's value_impl.cs ruling). It RE-OPENS if C2's go-cmp L lands: a reflect hand-own row.
- **MS26 (P). The C# consumer runners: two readings no leg takes.** `r-csharp-consumer-smoke-r2` adds
  `src/tests/CSharpConsumer/run-csharp-consumer.ps1` (it DOWNLOADS hashset v1.0.0 and uuid v1.6.0 through the Go module
  proxy, converts each with `go2cs -recurse` into a work root OUTSIDE the repository, builds and runs the consumer;
  EXPECT exit 0 and `checks: 21 ok, 0 failed`) and `src/tests/PackageTests/ConsumerUsings/test-consumer-usings.ps1`
  (restores the fixture from ONE feed into a fresh cache; EXPECT the green arm and the CS0246 control arm). R read both
  at its seat. At the union: the first is one conversion job (floor 1: after the battery, never beside it, or a lane's),
  `pwsh src/tests/CSharpConsumer/run-csharp-consumer.ps1 -WorkRoot <a fresh folder outside the repo>`; the second needs
  a feed that holds go.lib WITH the usings. **RULED (COORD 13:07): `test-consumer-usings.ps1` is a RELEASE STEP, run
  against the release rehearsal feed on the i7 BEFORE the PIN (no CI arm reads it yet)**: MS27 and the checklist's section 7
  carry it (`-Version 1.24.13.4 -Source <the rehearsal's .nupkg folder>`, EXPECT exit 0 = the green arm AND the CS0246 control). COORD rules
  whether the FIRST runner gates the landing (Q4). The owner's word on the usings (12:00): still safe for breaking changes; a sore spot is worked on later.
- **MS27 (P). AFTER P LANDS: the release of go.\* 1.24.13.4.** Not a step of any script here. From P's LANDED master
  (the union + the MS13 refresh, fast-forwarded): the release runbook and `src/release-nuget.ps1`; **the version bump
  (`src/version.props`, the README retargets, the frozen snapshot `docs/validation/1.24.13.4`) is the release's own
  step**, as is `test-consumer-usings.ps1` against the rehearsal feed before the PIN (MS26, ruled 13:07), a commit on master, never a commit of this train (`go2cs -version`, a P row, prints that tuple). OWNER HANDS:
  the GPG passphrase if the agent asks, and the signing PIN, once (Monday, COORD 10:05). The owner-approved macOS line,
  VERBATIM, for the release record (R's O docs seat already carries it in the README): "macOS (Intel and Apple silicon):
  packages ship for both chips, the behavioral suite passes on both, and the README walkthrough runs. The standard
  library is not yet validated package by package on macOS, so don't expect it to be fully operational there yet."
  Preconditions read at the union: the battery's exit 0 (NV51 / NV7 are the pre-flight the release starts with), the
  three lanes' readings, MS23's two runs, G's re-read. A commit on top of the landed master that touches a release path
  owes MS23 again.

### 4b. A fixup-N after the push: what COORD re-reads

O's table holds; the release column is P's:

| The fixup-N touches | Re-read on the i7 | Lanes | **Release gate (MS23)** |
|---|---|---|---|
| `src/go2cs/**` non-test Go (the converter) | a fresh battery | i9 and linux: again; MS13 patches STALE | **again, both runs** |
| `src/core/golib/**`, `src/gen/**` | 2b, GT x4, GN, CT, leg 5, H7 x3, WE, MOD, PUB, then every S / T row | i9 and linux: again | **again, both runs** |
| `src/core/**` corpus `.cs` or `.editorconfig` outside golib | E, H7 x3, WE, the S or T row of that package and of the canaries | the lane that sweeps that row | **release-smoke again** (a packed file) |
| `src/core/sort/**` | the above + PRE-D's release fix line, `SortMethodSelfCapture` isolated | the i9's sort row | **again, both runs; G's re-read** |
| `src/core/**/*_test.cs`, `*.tests.csproj` | that row's S or T leg; TE | the lane that sweeps that row | no (not packed) |
| `src/tests/Behavioral/**` goldens | leg 4 (CNR), `run-behavioral.ps1 --filter <project>` per project; PUB if one of its projects | P1's LB, and LX when linux-only | darwin FULL again |
| a registration file | precheck, SI, leg C | none | no |
| `src/go2cs/stdlib-metadata.txt` | CB | none | no |
| `src/_roster.ps1`, `docs/ValidatedTestPackages.md` | G1 / G2, ST51 / ST7, NV51 / NV7, precheck (ROSTER x3) | a moved row on every box | no |
| `src/go2cs/csproj-template.xml`, `src/go2cs/profiles/**` | PUB, leg 4, E | P1's LX, LB | **release-smoke again** |
| `src/tools/**` | NG*, CC | none | no |
| `.github/workflows/os-matrix.yml`, `src/tests/PackageTests/**`, `src/push-nuget.ps1`, `src/core/golib/buildTransitive/**` | NV51 / NV7 | none | **release-smoke again** |
| other `docs/**` | leg C | none | no |

Always: `python -B tP-helpers.py precheck <tP> $BASE <seats-effective.txt> head` at the new head.

## 5. Legs, the rows they gate, and expected readings

Walls are TRAIN O's bat2 (`coord-scratch/tO/bat2/tO-logs/SUMMARY.txt`, read to leg WEc) or N's where O's had not run at
the derive. "derived" means PRE-D or the leg computes it and stamps it. The PRE-D predictions are for the 22-row draft
with G's re-cut in place (a slot that adds a project, a test or a registration moves them: read the stamps).

| Leg | Gates | Expect | O wall / cap |
|---|---|---|---|
| PRE / PRE-D / PRE-1 / 1b / 1c | the union's shape; MASTER on the first-parent line; the derived lists; every seat | shape OK (first-parent = rows + 1); `PRE ... previous-train record (TRAIN O) O_RUN=.../tO/bat2 (SUMMARY <k> lines)`; **CB: 35 derived names kept + 6 base guards = 41, `dropped: [TestString]`** (the i9's fixture; the scan's control reads on this box's awk first; read with git at the table of record's pre-map chain head: 36 derived); GT `added=[DebuggerViewsTests HostPackageTimeoutRunningTestsTests] changed=[NoUncountedBackingAllocationsTests]`; **CNR `EXPECT N=834`** (819 + 16 measurable added - 1 removed; the control reads 819 at BASE from O_SUMS, fallback literal 819 stamped when used); behavioral: 8 added, slnx agrees; execution configs keep=[] drop=[]; **E-bisect default = the five footprint converter rows, `p2-test-warning-entries` stamped not bisectable**; **UF known class: the row that commits them = p2-test-warning-entries, tracked at HEAD: both**; **release fix: 0 bare `Sort(x);` lines and 3 methods at HEAD, control 3 at MASTER**; precheck rc 0 (MS9b.1); outparity no gap; 0 module-path hosts, GoDefaultGodebug in the same 3 files | 2 min |
| PRE-2 / 3 | coverage, canaries | 225 = 93 + 132; five canaries derived (O read crypto/cipher, runtime, crypto/tls, net/http, go/types); X_ROWS `encoding/json sort` join the S list (95 rows) | seconds |
| C | every converter row (15), repoguard | ok throughout | 541 s / 40 m |
| CC | regression | rc 0, `ok`, no FAIL | 3 s / 120 s |
| CB | every test the union ADDS under `src/go2cs` (derived, kept by the lexical scan: P13) + six base tree guards | N/N `--- PASS`, 0 SKIP/FAIL (41 names at the draft; the nine `TestHostCrash*` / `TestInFlight*` among them) | 32 s / 30 m |
| FX, FXc1, FXc2 | regression; the guard's controls | `stale 0`; tracked >= 1291 (O's stamp) and == current; both controls fire | about 70 s |
| E (+ bisect) | every footprint IN the tree | plants OK, 6 x rc 0, union-attributable **0**, standing drift stamped (O read 1 / 3 / 3), csproj explained (the sibling seat's are seat edits), HANDOWN 0, RUNTIME-MAP 0, editorconfig-deleted 0. **The U arm's `written=` reads far below the M arm's** (the i9's row: a READING, P10; O read 1830 / 1901 / 1905 in both arms). Above 0: the derived arms run | 2815 s |
| G1 / G2 | regression (no P row touches the roster) | pass x2; **0** rows with an execution config; O read 1994 checks | 27 s |
| SY, IDC, SI / SIc | symbol sync; regression; the projects P adds | clean; IDC rc 0 `SELF-TEST PASSED` fail=0 pass >= 217; SI clean (O read 821 behavioral projects), SIc six cycles | 2 min |
| ST51 / ST7, NV51 / NV7 / NVR | regression; **NV = the release's own pre-flight** | 0 violations and checks >= 62, x2; pre-flight clean x2; NVR refused by name | 20 s |
| NGa / NGb / NGF-j0 | regression (no P row touches src/tools) | identity `ran 43, failed 0` x2; selfdescription `ran 16, failed 0` x2; 0 parse errors x2; J0 nugetgo in all four reads | 30 s |
| 2b | g-debugger-views (golib + gen, measured on N's generator), c2-anon-struct-named-conv (gen), fm-record-r3 (golib), the 16 csproj P adds | rc 0, errors 0, gen-load 0 | 1145 s |
| CT | regression | Failed 0, Skipped 0, Total >= 24 | 25 s / 10 m |
| GN | c2-anon-struct-named-conv's ForeignStructTargetConversionTests | Failed 0, Total >= the derived floor (O read 92; the seat read 93) | 10 s / 20 m |
| TR | i9-tests-host-crash-verdict (`testing/TestHost.cs`) | Failed 0, Skipped 0, Total >= the derived floor (>= 26) | 9 s / 20 m |
| GT Debug + Release x3 | every golib row; DebuggerViewsTests and HostPackageTimeoutRunningTestsTests (added), NoUncountedBackingAllocationsTests (changed): derived | 0 failed, 0 NOT FOUND (O read 1566 total) | about 220 s each / 45 m |
| 4 | every golden under the union converter | NO REGRESSION; **N == the derived CNR_EXPECT_N** (834 predicted); CNR's skip list == the derived one (the seven) | 985 s |
| 5 + B: | the 8 projects P adds (each isolated: **SortMethodSelfCapture is the release fix's reading**), the goldens the fixup re-baselined, the literal guards | all pass; the main-alone pair exit 2 | 4205 s + about 23 m isolated |
| PUB | regression (PUB_MIN 6) | rc 0 and `published-output gate: 6 of 6 published programs match go, none hung` | 629 s / 30 m |
| H7 x3 | windows, linux, darwin (the corpus rows; golib's debugger views) | CS=0 x3 | about 750 s each |
| WE / WEc | the warning entries vs the compiler; p2-test-warning-entries' arm B on two test projects | WE rc 0, final `PASS`, 12 entry files and 14 entries with the row (10 and 12 without), stale 0, missing 0; WEc rc 0 `CONTROL CAUGHT`; plant gone | 4101 s + 357 s / 150 m + 45 m |
| MOD | the -tests -recurse driver (five P rows edit -tests conversion); XS, XM. XS is also the i7's reading of the module `g-xsync-bank` banks | N's verdicts: singleflight 12, tlog 17, semaphore KNOWN (7 of 8 by TestWeightedAcquire) or CLEARED (8: the banked row reads 28/28 solo and notes that test failing once under battery load: either reading passes the leg, and KNOWN here is the row's note class, not a finding); F4 0; MOD-orphans 0; `host seats read from ... (landed)` for both (P11) | N: about 28 min / 4 h |
| PB | regression | rc 0, cmp at its banked count, binlog SEEN then absent | about 3 min |
| S | every row at ITS roster config (none annotated); the canaries; X_ROWS encoding/json and **sort** (named: TestSortIntSlice, TestSortFloat64Slice, TestSortStringSlice) | each PASS at banked counts with a fresh record; crypto/tls reads the BoGo host-limit disclosure as at N and O. Under the i9's crash-verdict row a host that crashes or runs to its package deadline reads NAMED failing tests (`running tests: <names>`) where it read no verdict: a new named failure on a row is read as that first | N: about 2 h 50 m / 4 h per row |
| NR x4 | regression | 15 each, 0 deadlock lines | about 6 min |
| TE / HS / UF | P's test-source moves vs O's patch; host censuses; **UF's known class (P6)** | TE a READING; HS 0 / the 3; **UF 0 with p2-test-warning-entries seated**; without it `KNOWN class ...=1 [src/core/math/bits/.editorconfig]; other=0` | seconds |
| T:runtime/pprof | regression; frame names (-fm) | **145 + 7**, BANK-ELIGIBLE, roster match; TestMemoryProfiler by name | M: 618 s / 30 m |
| TBS-W, T:runtime | regression; fm-record-r3's and the else-if seat's runtime position maps, managed_impl.cs | TBS-W rc 0; **10819 + 71**, roster match; `/panic` AND `/trap` DISCLOSED | M: 263 s + 6130 s / 150 m |
| TE-T / HS-T / UF-T, TE-i9, TL-WALL | as O; baselines O's | readings; UF-T reads the known class again in the no-row case | seconds |
| i9 shard | 132 rows, **sort among them** | 132/132; UF 0 (KNOWN 1 without the row); the MS13 patch posted; exit 0 | O: 58 min |
| linux P1 | golib rows, rows, LX, LB, LM | exit 0, or 4 with the ONE known mover (os/exec 86 + 2); LCn 40 names, `dropped: [TestString]`, no scan-control mover; LX 0 hunks (the first reading of the windows-cut rows on the seven linux-only projects); LB SortMethodSelfCapture pass | O: 46 legs |
| linux P2 | runtime, pprof, TBS x10 | 10810 + 73; 147 + 7; 10/10 disclosed; exit 0 | O: 15 legs, 0 movers |
| **release-smoke (CI, C1)** | the release tree on every shipped RID | MS23: pack green; A-D PASS x4, D gating; B exercises the sort forms | CI |
| **darwin FULL (CI, C1)** | the behavioral suite on both Macs | MS23: all measurable pass on arm64 and x64 | CI |

NONZERO LEGS expected by design: FXc1, FXc2, SIc, NVR. **Nothing else.** Findings: O's list, plus: PRE-D `RELFIX`
(the release fix not in the tree, or its reader's control off), PRE-D `UF_KNOWN` (the literal against the tree), PRE-D
`CB_SCAN` (the test-name scan's control misread on this box's awk), a UF path outside the known class.

## 6. Deliberately NOT in the battery

| Not carried | Why |
|---|---|
| The release-smoke stage and the darwin behavioral FULL | CI runs by construction: MS23, C1's dispatch, COORD's read before landing |
| The C# consumer runners | MS26: one downloads and converts two modules (a conversion job with network), the other needs a feed |
| G's pflag / cobra re-read, the go-cmp / testify / yaml.v3 re-reads | the Target Atlas lanes' (real modules through -tests -recurse at the union); MOD reads x/sync and x/mod only |
| G's x/sync re-read (28/28) and the docs-site build at the union | OWED once the union is pushed (COORD 13:48, the g-xsync-bank row): the new pages sit under `docs/validation/modules/golang.org/x/sync@v0.19.0/`, an `@` in the path. Leg C's repoguard reads the docs guards over them (kramdown blocks, link text, titles); it does not BUILD the site |
| The release itself (version bump, pack, sign, push) | MS27, after the landing |
| M's G GOTRACEBACK=system control, L's HOP and SPB, the C1 token probe, L's TE signatures | as at O |
| crypto/tls on the i9 | the standing ban |
| The MS13 refresh | a bank step after the battery, never a leg |
| Any darwin RUN on a box here | a mac CI dispatch (MS10) |

## 7. Open questions: OWED COORD RULINGS (P's; O's are `trainO/tO-README.md` section 7)

1. **The one acceptance the table still owes**: `c2-ide-spike` a4bfed39f5 (and that the owner's desktop-checklist
   results land as a separate dated block, never on the seated ref). The other three were ruled 12:37, 13:07, 13:08.
2. **The three import / namespace rows have not been read together** (sibling -r2, go.go -r2 cut on it, alias-table
   cut on eb88ab9492): the union's leg 5 is the first reading. Accept that, or ask G / C2 for a local three-way
   reading before the freeze?
3. **The slots and the freeze** (about 21:00): the P BOARD docs row (who cuts it); go-cmp L was ACCEPTED IN
   SUBSTANCE 14:05 and **owed ONE re-cut (C2)**, `claude/c2-reflect-newat-field-r2`: **pushed by 14:58 as 92500eb260,
   read as expected and trial pre-mapped clean as row 23 by review round 1 (MS21.4 (iii)). TO RULE NOW: accept it at
   that sha (the row is written out in the table as a comment), with or without waiting for its runtime and net/http
   canary lines** (runtime's first run was not measured: a package timeout under load). If it is not accepted by the
   freeze the slot is struck and L rides the next train. The original `claude/c2-reflect-newat-field` dec8cee4b4 is never a seat.
   **L conflicts with the else-if -r3 as cut** (`reflect/package_info.cs`, the trial pre-map and COORD's pairs6.log):
   RULED 14:05 a re-cut -r2 stacked on e00629855f, placed last. It re-opens MS25 (a reflect hand-own) and is a golib
   + converter + corpus row landing on the release train with its full behavioral suite unread (disk): leg 5 is that
   reading, and the ruling is that a red there leaves by a revert on top (Q16). (The i9's crash-verdict seat
   was accepted 12:37 and is row 15 of the draft; G's x/sync bank was accepted 13:48 and is row 4.)
4. **MS26**: does `run-csharp-consumer.ps1` gate the landing, and who runs it (the i7 after its battery, or a lane)?
   The usings fixture is ruled a release step (13:07). `go.lib.targets` is a PACKED go.lib file: it ships in 1.24.13.4.
5. **MS23's scope**: confirm C1's two dispatches at P's union (release-smoke on four RIDs with D gating, darwin FULL)
   and again after any fixup-N on a release path. Optional third reading for the release fix: a `sweep-shard` of `sort`
   on darwin and linux (`-f stage=sweep-shard -f filter=sort`): wanted?
6. **The sibling seat's namespace rule is release-visible, and wider than the two corpus packages** (reworded by
   review round 1). (a) In the packed corpus `crypto/internal/fips140deps` and `internal/trace/internal/testgen/go122`
   change production namespace (both internal packages). (b) **Every converted THIRD-PARTY package whose name differs
   from its directory keeps the directory segment, and a hyphenated directory is NOT exempt** (the seat's design note:
   `github.com/mattn/go-isatty` becomes `go.github.com.mattn.go_isatty.isatty_package`). That reaches: the README
   walkthrough (fatih/color pulls `mattn/go-colorable` and `mattn/go-isatty`), which is release-smoke **arm D on four
   RIDs**, and both green release-smoke readings (37193550625 at eb88ab9492, 37218568547 at c49e2b7063) PREDATE the
   rule; the battery MOD leg's **M2 fixture** (`mattn/go-runewidth`); and first-wave **go-humanize** (package
   `humanize`), read 40+4 on linux at eb88ab9492, before the rule. The sibling row's notes now name these three as its
   union readings, so a red arm D or M2 points at that row. **To rule:** (i) the namespace move for third-party
   modules is intended for 1.24.13.4 (by the rule, the converter released with it gives such a package another namespace
   than 1.24.13.3's converter did: an inference from the rule, not a measured conversion); (ii) MS23's arm D at the P union is accepted as its release reading; (iii) who re-reads
   go-humanize at the P union before its first-wave package publishes (R is on standby; P1 read it at O).
7. **CSPROJ_TEMPLATE=accept** for `LiteralFloatConstFold.csproj` (confirmed 11:28: restated so the launch line carries
   it), and that **the fixup's emission check is accepted as c2-named-basic-conv's first footprint measurement**.
8. **The sibling seat's 2 production csproj and 24 committed -tests files are outside REGEN_ALLOW's grammar** (it admits
   generated `.cs` and per-file `.editorconfig`): keep the STOP for a csproj the union re-emits differently (the draft),
   or widen the grammar by ruling? `fips140deps` and `runtime/internal/wasitest` are on neither sweep list, so their
   committed -tests files are read by no leg.
9. **P4 (E-bisect)**: the derived default bisects FIVE seats (about 3 h 50 m) if leg E reads above 0. Accept, or pass a
   shorter `E_BISECT_SEATS` at launch (the sort seat and the else-if -r3 are the two whose files the fixup may
   regenerate)? **The consequence, stated by review round 1:** O's default was two arms. Five arms on top of the 11 to
   12 h estimate pass the default `DEADLINE` (launch + 14 h), and the arms run by themselves right after leg E, early
   in the run: the battery would stop at exit 9 before the sweeps finish. So the choice is made AT LAUNCH: an explicit
   `DEADLINE` (about launch + 18 h), or the shorter list. Nothing is capped in the script (which arms run is COORD's);
   PRE-D stamps the arithmetic (`PRE-D E-bisect NOTE`). With L -r2 seated the default is SIX arms (about 4 h 36 m).
10. **P9 (the release-fix assertion)**: the fixup DIES when `sort/sort.cs` holds the defect's line; the battery raises
    a FINDING. Confirm both (a train without the sort seat would need the check removed by hand).
11. **P10's lifetime**: at the train AFTER P both arms of the emission check carry the i9's incremental writes, and
    `written-M` stops meaning 'emits'. tQ derive item: the check needs the converter to write unconditionally (an
    option the i9 exposes for the single-target run, or the census path). Ask the i9 now?
12. **P11 (host-seat markers)**: a row is read as M's host seat only when its notes say `host seat H1` / `host seat
    H2`. Confirm (C2's go-cmp classes carry the bare names; the draft writes them H-1 and H-2 anyway).
13. **X_ROWS + sort (P12)**: the i7 sweeps `sort` too (an i9 row), as it does `encoding/json`. Confirm, and state it in
    the i9's GO as O's did for the canaries.
14. **The shared object store (PM, D10)**: `H:/Projects/go2cs/.git` holds about 7,100 loose objects (COORD's 13:07
    count), past `gc.auto` 6,700: every fetch, commit or merge in any worktree prints 'too many unreachable loose
    objects' and runs `gc --auto` (this derive's fetches printed it). FLAGGED by COORD 13:07: the remedy (a prune or
    `gc.auto 0`) comes once O has landed, never during a battery; WHICH of the two is still open, and it belongs
    before P's assembly (each of its signed merges starts a `gc --auto`). The pre-map writes to a scratch object directory
    (its three runs here: 0 objects to the shared store).
15. **P13 (the test-name scan)**: CB and LCn keep a derived name by a lexical scan of its file, where O kept it by
    backtick parity. Confirm; and the scan's control misreading on a lane's awk is a MOVER there (driver exit 4), by
    design visible. O's frozen battery script still carries the parity filter: at O it dropped only `TestRunning`
    (measured: the two filters agree on O's 29 derived names), so O's CB reading stands.
16. **A row that leaves by a revert on top (COORD 14:05, for C2's L): what the scripts need.** (a) The battery's shape
    check counts first-parent commits as rows + follow-ups + fixups: a bare revert commit above the fixup is REFUSED in
    PRE, so the revert must be made AS a `fixup-N: TRAIN P` commit (`FIXUP_N`), signed like one. (b) (READ from the
    arms' code, not run) precheck still
    reads the reverted row as a row: its BOTH arm finds the merge, and its COUNT / REG arms then miss the registrations
    the revert removed (L adds slnx and four-list lines) and HARD-FAIL; PRE-D's derived lists (CNR, the isolated
    projects, GT classes) read HEAD and follow the revert by themselves. So a relaunch needs either
    `PRECHECK_MODE=warn` (stamped, COORD's explicit call) or a 'reverted' marker in the list that precheck honours:
    NOT BUILT here (untested machinery on the release train). Rule which, BEFORE L is seated. (c) A revert of L
    touches golib, the converter and `src/core/reflect`: README 4b's rows for those paths (a fresh battery; MS23
    again). The cheaper order: seat L only once its -r2 has the full behavioral suite read somewhere.
17. **Low**: `claude/c1-darwin-plan` bccccbe9d6 is not in O's union and no P line names it (C1: 'no P seat'): left out.
    The logrus CS0122 / CS0246 layer waits for the freeze (G sizes it, no cut).
18. **(Review round 1) The handover record is stale on P's list, and `trainP/` is uncommitted.**
    `docs/phase4/RESUME-SESSIONS.md` section 1f.2 on `claude/coord-handover` still lists five superseded tips as P's
    accepted list (338ef4a2af, b4d7ff1cf9, 2265dcdb6e, 240512b217, d2e09a681c) and 're-base owed' for the guide, which
    COORD reversed at 11:30. A session resumed from GitHub has only the notes file's 13:35 and 13:48 lines for the
    real list. At the next save-state refresh: point 1f.2 at the frozen `trainP/tP-seats-draft.txt` (or restate the
    22 rows) once `trainP/` is committed (checklist section 1). Not editable from here: this folder's rule is to write
    only under `trainP/`.
19. **(Review round 1) Three refs are on the remote, outside O's union and outside P**: the original L
    `claude/c2-reflect-newat-field` dec8cee4b4, `claude/c2-sibling-pkgname-design` f4d823e5f9 (inside the sibling row
    through its internal merge) and `claude/r-jwt-promoted-iface-repros` ee52639557 (superseded). The NOT-P line names
    them now. Prune them at P's landing, or leave them to their lanes?

## 8. What was checked, and what was not

- `bash -n`: every `.sh` of trainP passes (the step-2 scripts included); `compile()`: every `.py` passes; no
  `__pycache__`; every file LF. A grep for an apostrophe inside `${VAR:?...}` reads none.
- **The table** (the assembler's OWN table-check block, lines `ROWS=` to `TABLE OK`, run read-only at the stand-in base
  eb88ab9492 with a fresh `ls-remote`): `TABLE OK: 22 rows ... rows-sha256=8491acbec194`; six `stack-on` tokens, each an
  earlier ancestor; no `after` token; no undeclared stack; every remote tip at its seated sha; the patch-id census
  reads none under two shas (`tP-DERIVE-REPORT.md` section 3 has the line). The footprint composition reads 142 path
  tokens, 133 distinct, in six rows, **measured by review round 1 with the fixup's own two greps AFTER the else-if
  row's `src/core/` prefix was stripped** (before: 142 distinct, 9 of that row's 123 files admitted; the derive's '133
  distinct' was a count the code did not produce). All 123 of that row's real paths (`git diff 07b53bc99d e00629855f
  -- src/core`) are admitted, and every one of the 142 tokens is a file at the pre-map's chain head.
- **The pre-map on the table of record** (`premap-run2/v8-eb88`, the step-2 instrument): `PREMAP-ALL DONE
  conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0`, rc 0: every row clean alone and in the chain,
  both chains one tree (e31f01ef9b24), 0 conflicting pairs. Earlier runs as the table moved: v5 (20 rows) and v6 (21
  rows, the original go.go seat) read `conflicted-rows=1`, that seat, as the step-2 pre-map had; v7 (21 rows, G's
  -r2) read 0. `tP-DERIVE-REPORT.md`
  section 3.
- **Read with git:** the train-commit predicate (39 over N's master for an eb88-cut row, 0 on eb88; 0 for the rows not
  cut on eb88); the 16 package directories P adds and the 1 it removes (826 enumerated at eb88, 7 skipped: 819); the 19
  csproj P adds with their LangVersion census and TrimMode counts; the two derivations of the added-csproj set agree at
  the pre-map's chain head (19 = 19; the rename reads R099 in both); the host censuses (0 and the 3) at that head; the
  release-fix predicate (3 at eb88ab9492 and 54f7f4439d, 0 at b719825826 and c49e2b7063); g-debugger-views' Goroutine.cs
  hunk; the footprints of the six rows; the three sort tests' names; `go.lib.targets` is packed and never imported
  in-tree; main.go routes one `-platforms` target outside the census path; warningEntries.go's ownership rule.
- **Run on git objects, the scripts' own lines:** the CB derivation and the linux driver's LCn block with the lexical
  scan (P13), at the table of record's pre-map chain head (36 derived, 35 kept, dropped `TestString`), at O's union against N's
  master (29, 28, dropped `TestRunning`: O's bat2 line), and on the i9's crash-verdict seat alone (15 derived, 14
  kept; O's parity filter on the same input keeps 6 and drops 8 real tests). The scan against the parity over every
  converter test file of O's union (261 files, 1113 text matches): 13 disagreements, each read by hand, the scan
  right in all 13 (5 fixture lines dropped by both; 8 real tests the parity drops). Both blocks' control-failure
  path (the finding / the mover, nothing filtered).
- **Run on synthetic inputs:** `tP-regen-apply.py` rule 4 (four cases, and O's reader on the same evidence); the
  battery's `ufarm` (five cases) and E-bisect derivation (three launch settings, the stale-name finding); the N_RUN
  refusal; the i9 driver's UF block (four cases); the fixup's `relfix` (three worktrees); the two carried purge retries
  against a real Windows file lock (clean after 2 retries at 12 s; 6 retries then a die at 50 s).
- **NOT run:** every script for real; no precheck, csprojtemplate, fixup, emission check or battery on any union (none
  exists); the release-smoke and darwin runs; the pre-map at BASE (BASE does not exist); the host-seat fix's effect on
  MR4c / MR6c (read from the code).
- **Review round 1 (`tP-CHANGES.md` 8.2 has every reading):** the footprint composition before and after the strip
  (142 distinct -> 133 distinct; 9 -> 123 of the else-if row's files admitted); `tP-fixup.sh` itself from scratch
  copies, to its launch refusal on the old list and to the LIVE gate on the new one (O's battery alive: it stopped
  there, nothing created); the `W` guard's seven cases; the lane drivers' base predicate on O's real history (pass,
  and N's landing commits named for a stale base); the changed pre-map instruments' controls (`premap-run3/controls`:
  24 lines, failed=0) and the 22-row list again (`premap-run3/v8-eb88-rr1`: the same head and tree, every log
  identical but the declaration census, now 3616 / 3708 with NEW = 0); merge-file made to fail (the old script read 0
  hunks silently, the new one an ERROR by name); a planted pair of same-named Go files (the old parse reader 0 errors,
  the new one 4). Still NOT run: the fixup's at-HEAD control inside a worktree (its predicate was run on git objects),
  the lane drivers on a lane box, `tP-regcheck.py`'s refusal of a row with more than two bases.
