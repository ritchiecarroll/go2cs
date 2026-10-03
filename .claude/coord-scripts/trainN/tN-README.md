# TRAIN N battery: draft summary

**DERIVED 2026-10-03 (02:10 to 03:30 Central) from TRAIN M's script set** (`hnd/.claude/coord-scripts/trainM/`, the
record: never edited) by a workflow agent, against TRAIN M's battery of record (`coord-scratch/tM/run3`, EXIT 0 at
01:45) and COORD's notes (`trainL/tL-seats-draft.txt`, 2026-10-02 19:00 to the end). **Nothing here was run for real.**
The commands run: git reads (`fetch`, `ls-remote`, `show`, `log`, `diff`, `merge-base`, `grep`), `bash -n` on every
`.sh`, `python -B -m py_compile` on every `.py`, the new helper readers' read-only controls (section 8), and the
assembler's `TABLE_ONLY=1` dry run (it stops after the table check: nothing created, nothing merged). Every change
against M is in `tN-CHANGES.md`, by item (N1 to N14), file and reason.

**Naming.** M's manual steps were M1 to M13. N's are **MS1 to MS19** (MSn carries Mn where it is still procedure;
dropped ones keep their number with the reason; MS14 on are N's). The derivation's ITEMS are N1 to N14
(`tN-CHANGES.md`). Seat notes: **N:n** = `trainN/tN-seats-draft.txt` line n; **L:n** = `trainL/tL-seats-draft.txt`
line n (COORD's running notes).

Union: `/h/go2cs-tmp-coord/tN`, branch `claude/coord-trainN-union`, base TRAIN M's landed master **8f46a9adae**
(`8f46a9adaef35c804d6eba62056e9844608301f3`). One signed merge per row of the seat list, in row order. **The list is
the 27-row draft** (N:3 to N:29, tips read by ls-remote 02:10; cutoff **10:00 Central 2026-10-03**; PENDING rows at
N:30 board only if pushed AND accepted by then). Its rows hash is **`rows-sha256=d62f0b3453e0`** (over the ref|sha
columns; the hash every script prints). **The table check reads ONE problem on it** (section 7, question 1): row 23's
`stack-on` note. No script carries a row count, a row sha, the rows hash or a map union sha.

**REVIEW ROUND 1 (2026-10-03, after the derive; `tN-CHANGES.md` section 4).** The 27-row draft does **NOT assemble as
cut**. git merge-tree over all 351 row pairs predicts **three textual conflicts** (rows 9, 21, 27) and one **clean merge
that does not compile** (row 15 x row 21): **MS21**, routes owed by COORD **before the map** (section 7, Q13 and Q14).
`c2-toml-test-host` (15838bfff5) was ACCEPTED at 02:34 and is not yet a row (MS18). The fixup **stops at step 5** on 13
csproj the union added and its CNR regenerates from the union template (**MS20**, Q15). So every typed figure below that
describes the list (27 rows, `d62f0b3453e0`, row numbers, CNR's 801) is the derive's reading of the draft AS CUT: a re-cut
or an added row moves it; read the value the table, the map and PRE-D print, never these.

## 1. The order of work

| Step | Command (from a FRESH per-run copy of this folder + the seat list) | Wall |
|---|---|---|
| 0 | MS9c's table controls, then the real list: `TABLE_ONLY=1 SIGN_PROBE=0 bash tN-assemble.sh` (EXPECT `TABLE OK`). Then `SEATS=<the list> bash tN-conflict-map.sh`: the map reads the list ONCE and keeps a fully clean union under `refs/coord/tN-map/rows-<hash>`; **the assembly REFUSES a list no map has read**. READ every shared path of a clean row that is not a registration file (MS1) | minutes |
| 1 | The manual steps of section 4 that come BEFORE the map, the assembly or the fixup: **MS21 (review round 1: the three conflicting rows and the HashSet compile break: COORD's routes, re-cuts pushed by the lanes before the cutoff, BEFORE step 0's map can read the list clean)**, MS2 (H4: COORD's ruling, one decision with row 23's note: Q1), MS14 (the registration files), MS17 (row 23), MS18 (add c2-toml-test-host), MS19 (the hashset module, and the i7's cold module cache), MS20 (the template-class csproj: COORD's ruling before the fixup's step 5) | |
| 2 | `bash tN-assemble.sh` (fetch; `git ls-remote` for the seat tips; signing preflight; the table; the map refusal; the merges; shape, ORDER and MAP TREE). Writes `coord-scratch/tN/assemble-maptree.txt`, which the fixup stamps | minutes |
| 3 | Ruled follow-up merges, signed `--no-ff`, each listed in `tN-follow.txt`, each on a branch of its OWN. **MS2 (H4) may owe one; so may an MS21 route that COORD rules as a follow-up instead of a re-cut** (the HashSet break's option 2); none otherwise. A follow-up can resolve neither a textual conflict (the assembly aborts before it) nor anything the map has not read | |
| 4 | `EXPECT_HEAD=<the assembled head> bash tN-fixup.sh` (`CSPROJ_TEMPLATE=accept` only by COORD's ruling, MS20; without it step 5 STOPS at the draft list) | INFERRED about 1 h 30 m (M's emission check 46 m, CNR 15 m) |
| 5 | **Push the union; GO to the i9 (`tN-lane-brief-i9.md`) and to P1/P2 (`tN-lane-brief-linux.md`)**, before the battery, so the lanes run beside it | |
| 6 | `EXPECT_HEAD=<fixup sha, 10 chars> bash tN-battery.sh`, from the fixup's run folder or a copy holding the SAME `tN-seats-draft.txt` and `tN-follow.txt` | INFERRED 10 to 11 h (M's run3: 9 h 48 m; N adds PUB 5 to 8 min, CC seconds, 12 projects in leg 5, about 30 min; the checklist carries the same figure); default deadline launch + 14 h |
| 6b | Only if a red needs a tree change AFTER the push (floor 9): `fixup-2: TRAIN N` ON TOP (`FIXUP_N=2`). Then section 4b | |
| 7 | Landing: merge master into the union (MS11), the landing precheck BEFORE the bank step, the bank step, **MS13 the refresh of committed -tests sources**, then master fast-forwarded | |

Per-run copy (floor 4): copy the whole folder, and `hnd/.claude/coord-scripts/trainN/tN-seats-draft.txt` into it.
Every script refuses a launch from the worktree, from `hnd`, from any `wf/draft*` folder or from the main checkout.
The fixup, the battery, a standalone `tN-modules-legs.sh` and a hand-launched `tN-emitcheck.sh` take one shared lock
(`coord-scratch/tN/.battery.lock`) and, before it, read the LIVE gate (`tN-helpers.py live`; `LIVE_ACK=1` runs past a
hit by COORD's explicit call). The battery refuses a run folder that already holds a `SUMMARY.txt`.

Exit statuses (unchanged from M). Battery: 0 clean; **6** a leg outside the four expected-non-zero controls is
non-zero, or a FINDING exists; 2 abort; 3 a leg's disk or build abort; **5** a wall cap fired (PUB's 30 m among them),
or a process is still running from the module out root after MOD: the lock is KEPT; 9 the deadline. Assembly: 2 a
table problem or no map for the list; 3 a merge that failed; 4 a shape, order or MAP TREE miss. Module legs: 4 on any
FAIL verdict. Linux driver: **4** on any MOVER or freshness fail. i9 shard: **4** on a failed row or a `SOFT:` line.

Environment variables (M's, with N's values and two new rows):

| Script | Variable | Default | Meaning |
|---|---|---|---|
| map | `SEATS` | COORD's live `hnd` list | the list the map reads |
| assemble | `TABLE_ONLY` | unset | `1` stops after the seat-table check: nothing created, nothing merged (MS9c) |
| assemble | `TIP_MOVED_OK` | unset | refs whose REMOTE tip descends from the seated sha, acknowledged by COORD |
| assemble | `MAP_UNION` | derived | normally unset: the union `tN-conflict-map.sh` kept for the same rows; with neither, the assembly ABORTS before creating anything |
| assemble, fixup | `SIGN_PROBE` | `1` | `0` skips the real probe signature |
| fixup, battery, modules, emitcheck | `LIVE_ACK` | unset | `1` runs past the LIVE gate; stamped |
| fixup | `EXPECT_HEAD` | required | the assembled union head (or the head after MS2's follow-up merge); with `FIXUP_N` >= 2, the pushed head |
| fixup | `FIXUP_N` | `1` | `k+1` = a fixup ON TOP of the k already at the head; subject `fixup-N: TRAIN N` |
| fixup | `PROSE_PATCH` | unset | a comment-and-prose patch; **none is owed at N** (MS12) |
| fixup | `REGEN` | `apply` | `skip` leaves the corpus alone (COORD's explicit call) |
| fixup | `REGEN_ALLOW` | composed from the seat list | composes to **NOTHING** at the draft list (no `CORPUS FOOTPRINT` note): any regenerated path STOPS the fixup (MS4) |
| fixup | `SIBLINGS` | `report` | `refresh` also refreshes the `*.cs.auto` siblings only the union arm rewrites |
| fixup | `GOLDENS` | `regen` | `skip` leaves stale goldens for leg 4 to report |
| fixup | `GOLDEN_CLASS` | `gframe` | `any` re-baselines a moved golden that carries another line kind, after COORD has read it: **at N every moved golden needs it** (no class is predicted; MS4) |
| fixup | `CSPROJ_TEMPLATE` | `stop` | (review round 1, MS20) `accept` carries in this fixup the csproj the union ADDED that its CNR regenerated with the union template's delta alone (`tN-helpers.py csprojtemplate`); `stop` dies there for COORD's ruling. A moved csproj outside that class dies either way |
| fixup | `EXP_DESC`, `EXP_TH` | unset | optin.py check's two counts; unset = reported (**TO BE SET BY COORD**) |
| fixup, battery | `EXEC_ROWS_EXPECT` | the count of `tN-helpers.py` `EXEC_RULED` | **0** at N (item N3) |
| fixup, battery | `SEATS_EXPECTED` | unset | optional second derivation of the seat count |
| battery | `EXPECT_HEAD` | required | the head of the union: the fixup commit, or the newest `fixup-N` on top of it |
| battery | `MASTER` | `8f46a9adae` | TRAIN M's landed master; another value is stamped as COORD's explicit call |
| battery, modules | `DEADLINE` | launch + 14 h | an ABSOLUTE local time `'YYYY-MM-DD HH:MM'` |
| battery | `CNR_EXPECT_N` | **derived** (N2) | CNR's package count; PRE-D derives it with `cnrexpect` (the runner's predicate over the tree at HEAD) and stamps its reconciliation; a value given at launch is COORD's explicit call, stamped beside the derived one |
| battery | `E_BISECT_SEATS` | `E_BISECT_KNOWN` (**empty** at N) | the seats bisected if leg E reads above 0: COORD's pick from the derived converter seats |
| battery, modules | `REALMOD_TEST_TIMEOUT` | `2m` | the real-module legs' `-test-timeout`; F4's verdict exists only at 2m |
| battery | `REALMOD` | `1` | `0` skips XS/XM by COORD's explicit call |
| linux P2 | `RT_CHILD`, `RT_OUTER` | `210m`, `450m` | the full runtime row's deadlines |
| battery | `PRECHECK_MODE` | `abort` | `warn` runs past a failed PRE assert, stamped |
| battery | `I9_PATCH`, `I9_BASELINE` | unset | the i9's TE patch, and its TRAIN M patch as the baseline |
| battery | `SWEEP_ROW_CAP` | `4h` | outer wall cap per sweep row |
| emitcheck | `TN_LOCK_HELD` | unset | set to `1` by the fixup and the battery |

## 2. Why the union owes a fixup

At N the content is MEASURED, not predicted: each seat committed its own corpus footprint and re-baselined its own
goldens; what git cannot see is what the fixup measures.

| | Fact | What breaks without it | Where |
|---|---|---|---|
| G1 | Goldens a seat x seat interaction moves (eleven converter seats; two template seats: g-publish-keep's TrimMode / ReadyToRun lines in every csproj, c2-nugetgo-id-pattern's PackageId (B) in 55 behavioral library csproj). **None is predicted** | leg 4 reads them CHANGED | fixup step 5: the union's CNR measures; `GOLDEN_CLASS=gframe` STOPS on any non-G-frame hunk; COORD reads `gold-cs.patch` and relaunches with `GOLDEN_CLASS=any` (MS4b) |
| G2 | Corpus a converter seat reaches and no seat committed. **No row's notes record one** | leg E reads it union-attributable | fixup step 4: the emission check at the assembled head; `REGEN_ALLOW` composes to nothing, so any listed path STOPS the fixup for COORD (MS4) |
| META | A stale `stdlib-metadata.txt` (c2-native-array-view, i9-crosspkg and others touch corpus `package_info.cs`) | `TestStdLibMetadataInSync` fails in leg C | fixup PRERES reads the guard by name; a STALE asset is regenerated and rides (RULED at M) |
| H4 | **r-jwt-promoted-iface-repros adds three behavioral directories with Go and no C#** (PromotedIfaceFromLibEmbed, PromotedIfaceFromLibEmbedLib, PromotedIfaceTestEmbed: `go.mod` and `.go` only, read with git) | precheck's H4 arm REFUSES the union (PRERES dies); CNR transpiles them and lists the untracked output as CHANGED | **MS2: COORD's ruling before the fixup**; a follow-up merge if that is the route |
| H5 | The roster sentence: g-slog-roster-tc0 writes "No row opts back out" with 0 annotated rows | nothing | no NOTE predicted (the H5 arm reads "No" as 0 since N; MS3 is dropped) |
| T1 | (review round 1) **14 behavioral csproj the N seats ADD were cut before the two template seats** (TrimMode 0 in each at its own sha; the four libraries also keep `<PackageId>go.$(AssemblyName)</PackageId>`), 15 with c2-toml-test-host's | the union's CNR regenerates the 13 windows measures: step 5 died 'CNR moved a csproj', which `GOLDEN_CLASS=any` never reached; with `GOLDENS=skip` leg 4 reads CHANGED | **MS20: COORD's ruling** (`CSPROJ_TEMPLATE=accept` in the fixup, or a follow-up seat); NativeFieldPortAlias (linux-only) is the lanes' LX (Q12) |
| R1 | (review round 1) **Two seats REMOVE registered keys** (p1-hashset-module: `HashSet.go`; p2-n-cleanup: five guard tests in go2cs-src.projitems) | precheck's REG arm read every removal as 'base-keys-lost': a hard FAIL on every correct union (PRERES, step 6, PRE-1, the landing precheck) | FIXED in `tN-helpers.py` (`reg_arm`: seat-removed keys, `removed-key-back`) |

## 3. Files

| File | What |
|---|---|
| `tN-seats-draft.txt` | COORD's draft table, as written (27 rows + PENDING and the N notes). Not edited by the derive. |
| `tN-conflict-map.sh` | The pre-map; base and names changed. |
| `tN-assemble.sh` | Assembly; base, names and the literal-site list changed; review round 1: the header's stale "every seat CLEAN" premise, an `after <ref>` order check, the patch-id skip for a landed row. |
| `tN-follow.txt` | The ruled follow-up merges: EMPTY; MS2 may add one. |
| `tN-fixup.sh` | The fixup: PRE, PRERES, 0b PROSE, S1/optin, CORPUS, GOLDENS (no prediction), shape, commit. |
| `tN-regen-apply.py` | Copies the union converter's emission over listed corpus files (names only). |
| `tN-battery.sh` | The i7 battery: M's legs + **CC** (comparison-classifier, N6) + **PUB** (published output, N4); CNR's N derived (N2); NGF-j0's expectation (N5); EXEC 0 (N3); `T-rewrites.patch` saved for MS13 (N10). |
| `tN-helpers.py` | M's readers; EXEC_RULED and H3_TOKENS empty; **`cnrexpect`**, **`uflinux`**, **`testsrc-refresh`** new; review round 1: precheck's REG arm knows seat-removed keys (`reg_arm`), csprojdrift explains a seat-deleted csproj, **`csprojtemplate`** new (MS20). |
| `tN-emitcheck.sh`, `emitdrift.py` | Leg E; a per-file `.editorconfig` is a known class kept out of drift (N7; review round 1: it is converter emission by COORD's notes, carried by no row of the draft: Q16). |
| `tN-modules-legs.sh` | Leg MOD; base; the landed host seats read from master. |
| `tN-i7-sweeps.txt`, `tN-i9-shard.txt` | **93** and **132** rows, byte-identical to M's (and L's). |
| `tN-ng-parse.ps1` | L's ParseFile reader (header only). |
| `tN-i9-shard.sh`, `tN-i9-te.sh`, `tN-lane-brief-i9.md` | The i9 driver (writes the MS13 patch too), its TE reader, its brief. |
| `tN-linux-legs.sh`, `tN-lane-brief-linux.md` | The P1/P2 driver (LX new, UF carve-out, log/slog retired) and brief. |
| `COORD-LAUNCH-CHECKLIST.md` | From the seat table to master, with N's EXPECT lines and the signing check by KEY (N8). |
| `controls/` | M's two TE patches (unchanged) and N's note on the new readers' controls. |
| `tN-CHANGES.md` | Every change from M, by item, file and reason. |

TRAIN M's `COORD-RULINGS-r2.md` and `-r3.md` are not copied: they are records, cited in place (`trainM/`).

## 4. Manual steps (a check can name them; a person decides them)

- **MS1 (M1). The seat list at the cutoff; walk the literal sites when a row is added.** A row added, dropped, moved or
  re-pointed means: re-run the map (step 0) BEFORE the assembly reads the list (it refuses otherwise). The row order is
  the merge order; a stacked seat sits below its base. **Every shared path of a clean row that is not a registration
  file is READ by a person before the fixup** (precheck's BOTH arm then checks each merge's own tree line by line).
  **What is DERIVED at run time and needs no edit for a new row:** the converter tests the union adds (CB; LCn), the
  GolibTests classes (GT), the behavioral projects (legs 4, 5, LB), **CNR's N and its platform-exclusive set** (PRE-D,
  LX), the nugetgo test scripts (NGa), the execution-config rows (S; the lane drivers), `REGEN_ALLOW`, the map union,
  the host seats (now also from master's line). **What is still a literal: walk it when a row is added:**

  | File | Literal | What it is | Checked how |
  |---|---|---|---|
  | `tN-helpers.py` | `EXEC_RULED` | THE ruling: which rows carry an execution config (EMPTY at N) | precheck FAIL |
  | `tN-helpers.py` | `H3_TOKENS` | one identifier per seat that edits `golib/runtime/Goroutine.cs` (EMPTY: no N row does) | a row that edits the file with no entry FAILS precheck by name |
  | `tN-helpers.py` | `S1_NONE` | the four hand-written csproj with no LangVersion | precheck FAIL S1 |
  | `tN-fixup.sh` | `REGEN_G_SEAT`, `REGEN_G_PKGS` | where a named seat's uncommitted footprint may land (EMPTY at N) | a path outside STOPS the fixup |
  | `tN-battery.sh` | `CB_BASE` | six tree guards that exist at the base | a missing name is a finding |
  | `tN-battery.sh` | `GT_NEIGH`, `GT_TESTS` | M's hazard-H3 neighbours (kept: g-cctor-init-frame names frames beside them) | a missing class or method is a finding |
  | `tN-battery.sh` | `CHECKDEAD_GUARDS`, `NEIGH` | L's five checkdead guards, eight neighbours (regression) | a missing directory is a finding |
  | `tN-battery.sh` | `E_BISECT_KNOWN` | EMPTY at N | a name outside the derived converter seats is a finding |
  | `tN-battery.sh` | `X_ROWS`, `T_ROWS`, `HS_WANT`, `NPOST`, `PUB_MIN`, the `named` cases of leg S, `CRASH` | encoding/json (section 7 q5); the two T rows; the 3 GoDefaultGodebug files; the posts' package list (a NOTE only); PUB's floor 5 | stamped; a moved set is a finding |
  | `tN-battery.sh` | the measured minimums 24 / 68 / 26 / 62 / 1291 / 5 | CT, GN, TR, ST, FX, PUB floors | finding |
  | `tN-linux-legs.sh` | `GT_NEIGH`, `LB_FIXED`, `LX_KNOWN`, the rows of L4 / L5 / L6, `OS_ROOT_EXPECT` | the linux lanes' hazard-chosen sets; the six linux-only packages TRAIN M's CNR named | a missing one is a MOVER |
  | `tN-i9-shard.sh` | the `named` cases | test names read by name on four rows | stamped |
- **MS2 (M2). H4: r-jwt-promoted-iface-repros' three Go-only directories. COORD's ruling, BEFORE the fixup** (review
  round 1: ONE decision with row 23's `stack-on` note, section 7 Q1; the reviewer recommends route (b) if R can push by
  the cutoff, else (c)). Read
  with git: ee52639557 adds `PromotedIfaceFromLibEmbed/{go.mod,main.go}`, `PromotedIfaceFromLibEmbedLib/{go.mod,lib.go}`,
  `PromotedIfaceTestEmbed/{go.mod,lib.go,internal_test.go,external_test.go}`, no `.cs`, no csproj, no registration
  (R: "green only with the i9's seat"). precheck's H4 arm (PRERES, step 6, PRE-1) FAILS on them; CNR would transpile
  them and read the untracked output CHANGED; leg 5's runner would see unregistered Go. The routes M named, each a
  ruled FOLLOW-UP merge on a branch of its OWN, cut on the seated sha (never pushed onto the seated ref): (a) move the
  repros out of `src/tests/Behavioral` (a test-fixture home of R's choice: CNR, the runner and H4 then never see them),
  or (b) commit their emission and register them (csproj, go2cs.slnx, the four BehavioralTests files: R's seat, at the
  union's converter, after the i9's forwarders seat). Then `<its ref>:<seat sha>` in `tN-follow.txt`. Or (c) drop row 23
  to TRAIN O (it carries no converter change: 8 files, all under those three directories). CNR's derived N follows the
  tree whichever route (801 with the three counted, 798 without).
- **MS3 (M3). DROPPED: the H5 roster sentence.** M's prose patch fixed it; g-slog-roster-tc0 rewrites it to "No row opts
  back out" with 0 annotated rows, and the H5 arm now reads "No" as 0 (it read it as -1 before: a false NOTE). The arm
  stays.
- **MS4 (M4). Read the `REGEN_ALLOW` the fixup stamps.** At the draft list it composes to `/^$/`: NOTHING may be
  regenerated, so ANY union-attributable path the emission check lists stops the fixup BEFORE a byte is written, with
  the evidence kept (`/h/go2cs-tmp-coord/tNemitfix`): COORD reads the list, then relaunches with
  `REGEN_ALLOW=<pattern>` (minutes) or routes the path back to its seat. Candidates by the census: the converter seats
  that touch emission (r-shadowed-import-alias, c2-generic-map-chan, c2-multi-func-define, c2-named-composite-infer,
  r-module-driver-gomod-less, g-slice-view-note; c2-native-array-view committed its four corpus files and i9-crosspkg its
  100 in-seat, and r-module-driver-gomod-less's two src/core files are the hand-owned testing package). **MS4b:**
  `GOLDEN_CLASS=gframe` stops on the first moved golden: READ `gold-cs.patch` (which seat's line), then relaunch with
  `GOLDEN_CLASS=any`.
- **MS5 (M5). Review siblings and hand-owns.** `SIBLINGS=report` (RULED). M's run3 leg E read 7 / 5 / 3 standing
  siblings per target (windows / linux / darwin); read any the fixup lists as union-only. g-cctor-init-frame edits a
  hand-own (`runtime/managed_impl.cs`); i9-gomethodvalue-nilfunc `reflect/value_impl.cs`: HANDOWN must read 0 written.
- **MS6 (M6). DROPPED: c1-route-sysctl.** An M row; CB still stamps the darwin pull census line.
- **MS7 (M7). `SetegidBroadcastSeam` is never re-baselined whole.** Linux-exclusive; the windows CNR skips it. P1's LB
  AND the new LX leg read it; the expected residual is the go1.24 alias family only (LX stamps it KNOWN by content).
- **MS8 (M8). If the row order changes**, adjacent inserts in a registration file can become one conflict hunk. N's
  registration hazards are MS14.
- **MS9 (M9). Run the two TE control patches** through `teattr` and `hunkclass` once before the first battery
  (checklist section 2; unchanged readers, M's predictions).
- **MS9b (M9b). Run the precheck's and the LIVE gate's controls once before the first fixup** (floor 13):
  1. precheck on the assembled union, `head` mode. **PREDICTED while MS2 is unruled: rc 1, ONE FAIL line, H4, three
     directories**; ROSTER rows 225 == 225, linux annotations 223 == 223 (read with the arms' own regexes at 582de36d1b),
     `ok ROSTER execution annotations: none (the ruled set ...)`, PROOF 1 page (log.slog.md, G's blob), H3 the four
     master tokens at their base counts, no markers, **no NOTE**. Once MS2's route is in the tree: rc 0, notes 0.
     (Review round 1: this prediction holds only with the REG fix. M's REG arm read p1-hashset-module's and p2-n-cleanup's
     intended removals as `base-keys-lost` and FAILED every correct union. Now EXPECT `ok REG src/go2cs/go2cs-src.projitems:
     ... base-keys-lost=[] ... removed-key-back=[] seat-removed=[...HashSet.go, ...the five guard tests]`. The arithmetic's
     control ran on real blobs (section 8): a correct union reads lost 0 / back 0, a kept removed key reads `back`, a
     dropped base key reads `lost`, while M's formula would have read 6 lost.)
  2. The same command on a checkout whose HEAD is `8f46a9adae` (a child worktree, removed after). PREDICTED rc 1: one
     `FAIL BOTH <seat>: no first-parent merge` per row, `FAIL COUNT` / `FAIL REG` on the registration files the seats add
     to, `FAIL PROOF` (log.slog.md is master's blob), `FAIL ROSTER execution annotations: log/slog[release-tiered]`.
  3. The LIVE gate with a planted `tZ/.battery.lock`: rc 1 and one `LIVE lock ...tZ...` line; removed: rc 0, `none`.
- **MS9c (M9c). Run the seat table's controls once before the first assembly** (floor 13). From a per-run copy:
  `TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<planted copy> bash tN-assemble.sh` (nothing is created whatever the table says).
  `<n>` is the row count of the list as it stands at the cutoff (27 at the draft as cut; the re-cuts of MS21 and the toml
  row move it), and row numbers are read from what the table prints, never from this text:
  1. **A duplicated sha:** append `dup-control|a7873ee6c2|control` (c2-nugetgo-id-pattern's sha). EXPECT exit 2 with
     THREE lines naming row `<n+1>` dup-control: `origin tip none ... (no such branch on the remote ...)`, `the same sha as
     row <k> c2-nugetgo-id-pattern`, and `descends from row <j> r-module-license and no 'stack-on' note covers it (an
     UNDECLARED stack)` (review round 1: the derive's text named two of the three).
  2. **A stacked row above its base:** move `c2-nugetgo-id-pattern` above `r-module-license`. EXPECT exit 2 with
     `'stack-on r-module-license' names no EARLIER row` and `is an ANCESTOR of the earlier row`.
  3. **A row already on the base:** append `landed-control|932778788f|control` (M's H2 seat, an ancestor of 8f46a9adae).
     EXPECT exit 2 with `is already an ancestor of the base 8f46a9adae (a LANDED seat: drop the row)`. (Review round 1:
     the patch-id loop now skips such a row; the reviewer's run took about 10 minutes there, review round 1's planted run
     227 s for the whole check.)
  3b. (Review round 1) **An order note naming a LATER row:** append `; after p2-n-cleanup` to an early row's notes. EXPECT
     exit 2 with `'after p2-n-cleanup' names a LATER row`; an `after <earlier row>` note prints `ordered after row <k>
     ... (declared; order verified)`.
  (Until row 23 is ruled its `stack-on` problem prints in every planted run too.) Then the real list: **the derive and
  review round 1 read exit 2 with ONE problem (row 23, section 7 Q1)**; once ruled, EXPECT `TABLE OK: <n> rows ...
  rows-sha256=<h>`, `<h>` read from that line (a notes-only edit keeps the hash: the derive read `d62f0b3453e0` for the
  27 rows as cut, and the reviewer's scratch 28-row list with the toml row before p2-n-cleanup read `f01959f7f875`; both
  are history once a row moves).
  4. **The map refusal** (after the assembly, before the fixup): the list WITHOUT its last row (p2-n-cleanup), no
     `TABLE_ONLY`, no `MAP_UNION`: EXPECT `TABLE OK: <n-1> rows ...`, then exit 2 `ABORT: no map run is recorded ...`.
- **MS10 (M10). Darwin is a mac CI dispatch**, not a leg of any box here (c2-native-array-view's mac acceptance read
  PASS on both legs before seating; NativeFieldPortAlias itself is linux-only).
- **MS11 (M11). At landing, after merging master into the union:** `git diff --stat 8f46a9adae <that master sha>`, read
  whole; a path under `src/` other than `src/version.props` means the battery's verdicts do not transfer for the legs
  that read it (section 4b). Then `python -B tN-helpers.py precheck <tN> <that master sha> <run>/tN-logs/seats-effective.txt
  head` and `check-roster-format.ps1` under both editions with `EXEC_ROWS_EXPECT=0`, **BEFORE the bank step** (RULED at
  M). No script lands the train.
- **MS12 (M12). DROPPED: the log/slog reason prose.** It landed with M's fixup; G's slog seat rewrote the roster's
  comment and sentence. The `PROSE_PATCH` mechanism stays for a ruling that needs one.
- **MS13 (M13). THE REFRESH OF COMMITTED -tests SOURCES: a POST-BATTERY BANK STEP, not a leg (item N10).** M ruled the
  corpus-wide refresh a COORD item "fed by a windows re-emission after M lands". Its design:
  1. **Input = the WINDOWS rewrite patches N's own -tests runs saved, all at the SAME union head**: the i7 battery's
     `tN-logs/S-rewrites.patch` (restore_paths' `git diff --binary HEAD` of the S rows, full context) and
     `tN-logs/T-rewrites.patch` (NEW at N: the T legs' rewrites with context; `T-rewrites-U0.patch` stays for TE), and the
     i9's `tN-i9-logs/tracked-changes.patch` (NEW at N: its 132 rows' rewrites with context; the `-U0` twins are
     accepted, then `--unidiff-zero` is needed). A patch from another head (a `fixup-N` landed after a box's run) is NOT
     an input: that box re-reads first. The linux lanes' patches are NOT inputs (the committed corpus is the windows
     record; linux re-emission differs in ordinals and in its own test files).
  2. **Filter = committed TEST sources ONLY**: `tN-helpers.py testsrc-refresh <tN> --out <run>/tN-logs/testsrc-refresh.patch
     <patches>` keeps a file block only when its path is under `src/core`, not under `src/core/golib`, named `*_test.cs`,
     `package_test_info.cs` or `package_info_internal_test.cs`, tracked, and not hand-owned (no line-anchored
     `[module: GoManualConversion]` at HEAD). Everything else is REFUSED and listed by category, never written:
     production files (`package_init.cs` is the -tests hook's rewrite, not a target; `go2cs_test_host.cs`), proof pages
     (`docs/validation`: host-conditioned), csproj, review siblings, anything else; inside the class a block that creates,
     deletes, renames or binary-patches a file is REFUSED; two inputs that rewrite one file DIFFERENTLY are a CONFLICT
     (refused; two windows re-emissions disagree: read it). Per file it prints the hunk classes: G-FRAME and MAP (as TE
     reads them), ALIAS (fixup-2's using-alias lines), SLICE (the older `.slice` spelling of a range index), OTHER (read
     it). EXPECT `REFRESH-VERDICT ... refused=0 conflicts=0`. Smoke-read on M's run3 patches (section 8): 338 files,
     classes g-frame 486, map 80, alias 121, slice 293, other 143; 27 paths excluded by category.
  3. **Apply on the union after the battery** (and after MS11's precheck and the bank step), on a tree back at HEAD (the T
     legs' material is in `T-rewrites.patch`; restore it first): `git apply --check <out>` then `git apply <out>` (with
     `--unidiff-zero` only when the verdict says so), then `tN-helpers.py testsrc-refresh <tN> --check-worktree`: EXPECT
     rc 0 and `ONLY committed test sources changed` (any other changed or untracked path is REFUSED: restore it).
  4. **Commit it signed as its own commit**, `refresh: TRAIN N -- committed -tests sources from the windows re-emission
     (MS13)`, the explicit file list from `--check-worktree` (never `add -A`, floor 8), read by KEY (checklist section 4),
     BEFORE master is fast-forwarded. No leg needs a re-read: the refreshed bytes ARE the bytes the S / T legs and the i9's
     rows compiled at this head (state that, with the verdict line, in the commit message).
  The standing class until then: TE and TE-T keep reading G's frame class (no committed test source was regenerated at
  M) plus fixup-2's alias lines; P2's 35 runtime / runtime/pprof sources stale against fixup-2 are in the class.
- **MS14 (new). The registration files (assembly hazards, N:32).** go2cs.slnx and the four BehavioralTests files
  (`CompileTests`, `OutputComparisonTests`, `TargetComparisonTests`, `TranspileTests`) take inserts from EIGHT seats
  (r-shadowed-import-alias, g-using-static-alias-guards, c2-generic-map-chan, c2-multi-func-define,
  c2-named-composite-infer, i9-crosspkg-promoted-forwarders (4 projects), i9-gomethodvalue-nilfunc, g-cctor-init-frame;
  c2-native-array-view registers NativeFieldPortAlias in the four BehavioralTests files and, being linux-only, NOT in
  go2cs.slnx: by design, as the six linux-only projects at the base), **a NINTH once c2-toml-test-host is a row**
  (ElidedAnyCompositeElems: go2cs.slnx, go2cs-src.projitems and the four files; it read clean against all 27 rows in the
  reviewer's merge-tree pass). **KEEP EVERY SIDE'S INSERT.** The line-count invariant is precheck's COUNT arm (each
  file == base + every seat's OWN net inserts + the fixup's), the key union its REG arm (which, since review round 1,
  also knows the keys a seat REMOVES on purpose: `seat-removed=[...]`, and a removed key kept by a merge is
  `removed-key-back`), and the per-merge line check its BOTH arm. `src/go2cs/go2cs-src.projitems` takes many adds
  (c2-native-array-view, c2-s3b-nuget-map, c2-nugetgo-id-pattern, r-shadowed-import-alias, r-module-driver-gomod-less,
  c2-generic-map-chan, c2-multi-func-define, c2-named-composite-infer, p1-hashset-module, g-slice-view-note, and the toml
  row) AND removals (p2-n-cleanup's five guard tests, p1-hashset-module's HashSet.go): the merge-hazards adjacent-insert
  rule. **Review round 1 corrects this step: an insert beside a delete is NOT merged silently here, git REFUSES it**:
  r-module-driver-gomod-less inserts goModFile.go / goModFile_test.go immediately before the two h5 lines p2-n-cleanup
  deletes (`@@ -148,0 +149,2 @@` vs `@@ -149,2 +147,0 @@`): a CONFLICT whichever row merges second (MS21, item 3).
  **p2-n-cleanup is the LAST row** because it deletes files other rows may touch: the BOTH arm reads its merge against the
  whole union. **The append-only BOARD is a registration file too** (review round 1): two seats that each append a
  section to the end of `docs/phase4/BOARD-next-validation-candidates.md` (above its final raw-guard line) ALWAYS
  conflict (MS21, item 1): a BOARD entry rides one seat per train, or lands as a docs commit after the train.
- **MS15 (new). PUB's single-file hang context.** g-publish-keep fixes a published app that misbehaved under full trim
  (TrimMode=partial in the template). Single-file + ReadyToRun + partial trim HUNG 5 of 10 (then 10 of 10) at G's
  measurement (L:161-163); G's mitigation: the nine converter profiles set ReadyToRun false and the template turns R2R on
  only when not single-file; PUB's single-file arm is the standing guard (10 runs, 60 s). Its control is documented, not
  re-run (rc 1, `InterfaceAssertionMapKey (single-file) HUNG: 10 of 10 runs ...` at 2bb2e5f825's template). The hang's
  ROOT is COORD's own i7 item (L:163: reproduce at f0397cf250, stacks of a hung process): a PUB red reading HUNG is read
  against that first, and a NOT MEASURED publish against the leg's environment (pwsh with DOTNET_ROOT unset, dotnet10
  first on PATH: INFERRED, section 7 q7).
- **MS16 (new). c2-nugetgo-id-pattern stacks on r-module-license and merges with g-publish-keep with 0 conflicts (57
  files changed on both sides, L:140, L:162).** The table verifies the stack (dry run: `stacked on row 12 r-module-license
  (declared; ancestor verified)`). The 57 both-changed paths (the template and the 55 behavioral library csproj among
  them) are exactly what the BOTH arm reads at c2's merge: each side's lines kept, none duplicated.
- **MS17 (new). r-jwt-promoted-iface-repros after i9-crosspkg-promoted-forwarders (L:133).** ROW ORDER, not a git stack:
  ee52639557 is cut on e2008427b1 (M's first fixup), so its note's `stack-on` FAILS the table check (section 7 q1). Its
  repros are green only with the i9's seat in the tree; R's jwt/v5 re-run at the union is its acceptance; it is also MS2's
  H4 row.
- **MS18 (new). The PENDING rows (N:30) board only if pushed AND accepted by the cutoff, 10:00 Central**:
  r-test-host-init-order, r-ptrptr-anon-struct-lift (stack-on r-atlas-batch-repros d834821da0, which is not a row: it
  must be seated first or the stack-on fails the table), g-float-untyped-const-compare (the same base),
  p1-linux-only-goldens (likely the LX leg's remedy for any linux-only golden LX moves, and a candidate carrier of
  NativeFieldPortAlias's linux re-emitted csproj: Q12), p2 converter-warning-clears (the per-file `.editorconfig` entries
  the CONVERTER emits: item N7's class, Q16). **c2-toml-test-host 15838bfff5 is ACCEPTED** (L:172, 02:34: "add to
  tN-seats-draft AFTER the derive workflow ends, before p2-n-cleanup"): COORD adds the row, in the slot before
  p2-n-cleanup (cut on 8f46a9adae, 19 files: a behavioral project ElidedAnyCompositeElems, the four BehavioralTests
  files, go2cs.slnx, go2cs-src.projitems, converter changes in convCompositeLit.go and visitInterfaceType.go). It moves
  every typed figure of the draft: `<n>` = 28, the hash (`f01959f7f875` on the reviewer's scratch list), CNR's derived N
  +1, `NPOST` lacks it (a NOTE only), its csproj is a 15th of MS20's class. Each added row: MS1 (a new map, the literal
  sites), MS14 for its registrations.
- **MS19 (new). p1-hashset-module: the converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0`** and
  `src/go2cs/HashSet.go` is deleted. Every converter build of N (the emission check's two archived converters, the
  fixup, legs C / CB / PB / T, MOD, the lanes) resolves the module through the H: module cache (GOFLAGS empty; GOPROXY
  not off outside MOD). If the i7's cache does not hold it, the first build downloads it from the proxy: COORD's call
  whether to warm it first (`go mod download` in the tN worktree's `src/go2cs`, a network read) so no leg depends on the
  network. **Read 2026-10-03 (review round 1, exact paths only): the i7's cache (`H:\go\pkg\mod`) does NOT hold
  hashset v1.0.0** (x/mod v0.33.0 and x/tools v0.42.0 are there); `go env GOPROXY` is the default proxy, so the fixup's
  first converter build downloads it. A standalone `tN-modules-legs.sh` (GOPROXY=off) now refuses to start on a cold
  cache, naming the absent paths (derived from the union's `src/go2cs/go.mod`, exit 3); the lane briefs carry a warm step.
  **The HashSet compile break (MS21, item 4) is this step's twin:** p1-hashset-module deletes the type every unqualified
  `HashSet[` in package main named, and a row cut without it can still add one. Before the map, for every row, its ADDED
  `src/go2cs` Go lines must hold no unqualified use (the reviewer's scan; the review round's re-read found b9e2d5ffa1's two
  lines only):
  ```
  for r in $(grep -E '^[a-z0-9.-]+\|' tN-seats-draft.txt | cut -d'|' -f1,2); do sha=${r#*|}; mb=$(git merge-base 8f46a9adae $sha)
    n=$(git diff $mb $sha -- 'src/go2cs/*.go' | grep '^+' | grep -v '^+++' | grep -cE '(^|[^.A-Za-z_])(HashSet\[|NewHashSet\b)'); [ "$n" = 0 ] || echo "${r%|*} $n"; done
  # EXPECT no line once p1-hashset-module is a row (a comment that names the type reads here too: read each hit)
  ```
- **MS20 (review round 1). The template-class csproj: COORD's ruling before the fixup's step 5 (Q15).** Fourteen
  behavioral csproj the N seats add (NativeFieldPortAlias; UsingStaticNamespaceAlias + aliaslib;
  ShadowedStdlibImportAlias + errors; GenericDefinedMapChan; MultiValueFuncLiteralDefine; NamedCompositeGenericArg; the
  i9's four; NilFuncIfaceMethodValue; InitFrameNames), 15 with c2-toml-test-host's ElidedAnyCompositeElems, were cut
  before g-publish-keep's template (the ReadyToRun condition, the TrimMode comment and line: TrimMode count 0 in each at
  its own sha) and the four libraries before c2-nugetgo-id-pattern's (B) (they keep `<PackageId>go.$(AssemblyName)</PackageId>`).
  The union's CNR rewrites every csproj from the union template, so it moves the 13 windows measures; M's step 5 died
  `CNR moved a csproj (template drift is step 1's class ...)`, and step 1 repairs only LangVersion. N's step 5 now
  CLASSIFIES first (`tN-helpers.py csprojtemplate`): the template DELTA is derived from the seats' own edits of EXISTING
  behavioral csproj (base..HEAD, a line changed the same way in at least 10 files: G's 791, C2's 55); a moved csproj is
  in the class only when the union ADDED it and every hunk line is in the delta. Outside the class: die, as before.
  Inside it: `CSPROJ_TEMPLATE=stop` (the default) dies naming the class, and `CSPROJ_TEMPLATE=accept` (COORD's ruling)
  keeps CNR's regenerated csproj, stamps them, lists them in `tmpl-files.txt`, adds them to the commit's explicit file
  list and names them in the commit message as `(5b) TEMPLATE-CLASS csproj`. The other route: a follow-up seat that
  regenerates them before the fixup. NativeFieldPortAlias is linux-only: the windows CNR skips it, and LX reads it on P1
  (predicted +3/-1: Q12). Controls (section 8): positive per template seat, and three negatives (a planted line, a csproj
  the union did not add, the PackageId swap read against G's delta alone) each refuse.
- **MS21 (review round 1). THE DRAFT DOES NOT ASSEMBLE AS CUT: four routes, COORD's, BEFORE the map (Q13, Q14).**
  git merge-tree on the real 3-way bases (the reviewer ran all 351 row pairs; this round re-ran the four below and read
  the same markers). Each is a lane re-cut pushed before the cutoff, or a row that waits for TRAIN O; the assembler
  refuses any conflict (M's doctrine) and a follow-up merge cannot resolve one.
  1. **BOARD, row 9 g-trim-annotations (4526442d52) x row 4 c2-native-array-view (c27cf4198a).** Both append a section
     at the end of `docs/phase4/BOARD-next-validation-candidates.md` above its final raw-guard line (G: c2e7cef853's
     "the copying slice view"; C2: 745e0969c0's "native-root field views"); both cut on e2008427b1, so the real merge has
     the same base. The map would print `SEAT 9 g-trim-annotations: CONFLICT in: docs/phase4/BOARD-...`. The reviewer
     recommends (a): G re-cuts g-trim-annotations WITHOUT c2e7cef853, and that BOARD entry lands as a docs commit after N;
     or (b): G re-cuts it on c27cf4198a with `stack-on c2-native-array-view` (row 4 is earlier).
  2. **src/go2cs/visitAssignStmt.go, row 21 p1-hashset-module (a7a7a197dd) x row 19 c2-multi-func-define
     (1c182b5ca2)**: C2 rewrites the `if tupleResult || ...` condition (and a comment block above it); P1 rewrites the very
     next line, `leftExprs := HashSet[string]{}` -> `hashset.HashSet[string]{}`. Adjacent hunks: `SEAT 21 ...: CONFLICT`.
  3. **src/go2cs/go2cs-src.projitems, row 27 p2-n-cleanup (6235912709) x row 17 r-module-driver-gomod-less
     (537abad2a2)**: R inserts goModFile.go / goModFile_test.go right before the two h5 lines P2 deletes: `SEAT 27 ...:
     CONFLICT` whichever merges second. The reviewer recommends: P2 re-cuts p2-n-cleanup (or only its projitems commit) on
     537abad2a2, resolving 'keep the two goModFile lines, drop the two h5 lines', note `stack-on
     r-module-driver-gomod-less`; row 27 stays last, and precheck's COUNT then reads both seats' own nets without a double
     count (own-base is computed in row order).
  4. **A CLEAN merge that does not compile: row 15 g-using-static-alias-guards (b9e2d5ffa1) x row 21**: b9e2d5ffa1 adds
     `whiteboxBridgeDeclaredNames = HashSet[string]{}` and `whiteboxBridgeTypeNames = HashSet[string]{}` to
     `src/go2cs/usingStaticNamespaceAlias_test.go` (package main, lines 252-253; the base file has none), and row 21
     deletes `src/go2cs/HashSet.go`. The converter binary still builds (a test file), so the emission check passes; the
     fixup dies at PRERES (`TestStdLibMetadataInSync did not PASS ... a build failure`) and leg C would fail. No map can
     see it. The reviewer recommends folding it into item 2's route: P1 re-cuts p1-hashset-module on a local merge of
     b9e2d5ffa1 and 1c182b5ca2, qualifies the two lines (and the import), resolves the visitAssignStmt.go hunk, and notes
     `stack-on g-using-static-alias-guards stack-on c2-multi-func-define` (the table checks every stack-on token).
     Alternatives: a ruled follow-up cut on b9e2d5ffa1 listed in `tN-follow.txt` as `<ref>:b9e2d5ffa1` (it compiles only
     on the union), or p1-hashset-module waits for TRAIN O (owner priority applies: hashset is the first nugetgo mapping).
  After the routes land: re-read the table (MS9c), run the map (step 0: EXPECT every row clean), re-run MS19's scan.

### 4b. A fixup-N after the push: what COORD re-reads

M's table holds (a commit ON TOP, subject `fixup-2: TRAIN N`, then `fixup-3`, ...; the battery's PRE admits that chain
and nothing else single-parent). N adds two rows:

| The fixup-N touches | Re-read on the i7 | Lanes |
|---|---|---|
| `src/go2cs/**` non-test Go (the converter) | a fresh battery | i9 and linux: again; their MS13 patches are then STALE (re-read before the refresh) |
| `src/core/golib/**`, `src/gen/**` | 2b, GT x4, GN, CT, leg 5, H7 x3, MOD, PUB, then every S / T row: in practice a fresh battery | i9 and linux: again |
| `src/core/**` corpus `.cs` outside golib | E, H7 x3, the S or T row of that package and of the canaries, MOD if under `testing/` | the lane that sweeps that row |
| `src/core/**/*_test.cs`, `*.tests.csproj` | that row's S or T leg; TE | the lane that sweeps that row |
| `src/tests/Behavioral/**` goldens | leg 4 (CNR), `run-behavioral.ps1 --filter <project>` per project; PUB if one of its four | P1's LB, and LX when the project is linux-only |
| a registration file | precheck, SI, leg C | none |
| `src/go2cs/stdlib-metadata.txt` | CB | none |
| `src/_roster.ps1`, `docs/ValidatedTestPackages.md` | G1 / G2, ST51 / ST7, NV51 / NV7, precheck; a moved row on every box | the lane that sweeps a moved row |
| `src/go2cs/csproj-template.xml`, `src/go2cs/profiles/**` (N) | PUB, leg 4 (every csproj), E (csproj drift) | P1's LX, LB |
| `src/tools/**` (N) | NG*, CC | none |
| other `docs/**` | leg C | none |

Always: `python -B tN-helpers.py precheck <tN> 8f46a9adae <seats-effective.txt> head` at the new head.

## 5. Legs, the seats they gate, and expected readings

Walls are TRAIN M's run3 (`coord-scratch/tM/run3/tM-logs/SUMMARY.txt`), as a size, not N's budget. "derived" means
PRE-D or the leg computes it from the tree and stamps it.

| Leg | Gates | Expect | M wall / cap |
|---|---|---|---|
| PRE / PRE-D / PRE-1 / 1b / 1c | the union's shape (the seat merges + follow-ups + `fixup: TRAIN N` + any `fixup-N`); the derived lists; every seat | shape OK; PRE-D CNR `EXPECT N=801` at the draft list (785 + 16 measurable added; NativeFieldPortAlias added AND skipped) with its control (785 at 8f46a9adae); execution configs keep=[] drop=[log/slog], `EXEC_ROWS_EXPECT=0`; precheck rc 0 once MS2 is ruled (REG `seat-removed=[HashSet.go + five guard tests]`, `base-keys-lost=[]`: review round 1); outparity no gap; 0 module-path hosts, GoDefaultGodebug in the same 3 files; the CNR control at the base reads 785 (its fallback literal when M's SUMMARY is unreadable, stamped) | seconds |
| PRE-2 / 3 | coverage, canaries | 225 = 93 + 132; five canaries derived; the S list takes `encoding/json` and the derived TC0 row `log/slog` | seconds |
| C | every converter seat, repoguard (p1-hashset-module's module requirement: MS19) | ok throughout | 492 s / 40 m |
| **CC** (new, N6) | the comparison-classifier module | rc 0, `ok`, no FAIL | seconds / 120 s |
| CB | every test the union ADDS under `src/go2cs` (derived) + six base tree guards | N/N `--- PASS`, 0 SKIP/FAIL | 23 s / 30 m |
| FX, FXc1, FXc2 | regression; the guard's controls | `stale 0`; tracked >= 1291 (M's run3 stamp) and == current; both controls fire | about 40 s |
| E (+ bisect) | every footprint IN the tree | plants OK, 6 x rc 0, union-attributable **0**, csproj explained (g-publish-keep's and c2's csproj are seat edits; **p2-n-cleanup's GenericTests.csproj a seat delete**: review round 1, csprojdrift read it as OTHER and exited 1), HANDOWN 0, **editorconfig-written 0** (N7: no row carries the converter's per-file entries; Q16), RUNTIME-MAP 0; above 0: COORD names the arms (E_BISECT_KNOWN empty) | 2797 s |
| G1 / G2 | g-slog-roster-tc0 | pass x2; **0** rows with an execution config; check count REPORTED (M 1994) | 16 s |
| SY, SI / SIc | the projects N adds | clean; SIc fires six cycles | 16 s |
| ST51 / ST7, NV51 / NV7 / NVR | regression (g-slog-roster-tc0's comments in `_roster.ps1` and `run-validated-sweep.ps1`); c2-s3b-nuget-map edits `push-nuget.ps1` itself | 0 violations and checks >= 62, x2; pre-flight clean x2; NVR refused by name | 15 s |
| NGa / NGb / NGF-j0 | r-module-license, c2-nugetgo-id-pattern (8 paths under `src/tools`) | every `Test-*.ps1` `ran N, failed 0` x2 (N REPORTED; M 29 / 10); 0 parse errors x2 (NugetgoLicense.psm1 parsed); **J0: nugetgo.github.com.google.uuid in all four reads** (default PackageReference and no go. form, `-p`, `-getProperty`, the script default: N5) | seconds |
| 2b | the golib and gen seats | rc 0, errors 0, gen-load 0 | 1589 s |
| CT | regression | Failed 0, Skipped 0, Total >= 24 | 26 s / 10 m |
| GN | the gen seats (their own GenTests) | Failed 0, Total >= the derived floor (>= 68) | 11 s / 20 m |
| TR | r-module-driver-gomod-less (testing host) | Failed 0, Skipped 0, Total >= the derived floor (>= 26) | 28 s / 20 m |
| GT Debug + Release x3 | every golib seat | 0 failed, 0 NOT FOUND over the derived classes; each ADDED class at its floor | about 220 s each / 45 m |
| 4 | every golden under the union converter | NO REGRESSION (the template-class csproj are in the fixup under MS20's ruling, or a follow-up regenerated them); **N == the derived CNR_EXPECT_N** (801 at the draft list as cut, 798 if MS2 moves the repros out, +1 with the toml row); CNR's skip list == the derived one (the six + NativeFieldPortAlias) | 923 s |
| 5 + B: | the projects N adds (derived: 12; NativeFieldPortAlias not read here: linux-only), the goldens the fixup re-baselined, the literal guards | all pass; the main-alone pair exit 2 | 5380 s + about 30 m |
| **PUB** (new, N4) | g-publish-keep | rc 0 and `published-output gate: 5 of 5 published programs match go, none hung` (n >= 5); the leg's SDK 10.0.* | new: 5 to 8 min / 30 m |
| H7 x3 | linux, darwin, all | CS=0 x3 | about 725 s each |
| MOD | the module legs (regression; r-module-driver-gomod-less edits the driver and the testing host); XS, XM | M's verdicts: the controls MR4c / MR6c read rc != 0 by name (their converters carry the LANDED H2 seat's stream reader, read from master); XS errgroup 5, syncmap 3, singleflight **12**, semaphore KNOWN; XM modfile 323 ... tlog **17**, zip builds; F4 0; MOD-orphans 0 | 1745 s / 4 h |
| PB | regression | rc 0, cmp at its banked count (4), binlog SEEN then absent | 173 s |
| S | every row at ITS roster config (the i7 list is M's, byte-identical: N13) | each PASS at banked counts with a fresh record; **log/slog 199 + 17 at TC0, `tiered=False`** (a TestSetDefault red is read as the first-launch class first); no PASS line carries `[release-tiered]`; crypto/tls reads the BoGo host-limit disclosure as at M, `1340 = 4759 banked - 3419` (TestBogoSuite disclosed: capability present, the converted side over the host deadline) | about 2 h 50 m / 4 h per row |
| NR x4 | regression | 15 each, 0 deadlock lines on a fresh record | about 6 min |
| TE / HS / UF | the standing frame class + N's test-source moves; host censuses | TE a READING vs M's patch; HS 0 / the 3; UF 0 | seconds |
| T:runtime/pprof | regression; g-cctor-init-frame | **145 + 7**, BANK-ELIGIBLE, roster match; TestMemoryProfiler by name | 618 s / 30 m |
| TBS-W, T:runtime | regression | TBS-W rc 0; **10819 + 71**, roster match; `/panic` AND `/trap` DISCLOSED on a fresh record | 263 s + 6130 s / 150 m |
| TE-T / HS-T / UF-T, TE-i9, TL-WALL | as M; `T-rewrites.patch` saved for MS13 | readings | seconds |
| i9 shard | 132 rows | 132/132; log/slog `ENV-OK tiered=False`; UF 0; the MS13 patch posted; exit 0 | L: 3278 s |
| linux P1 | golib seats, rows, LX (new), LB, LM | see the brief; exit 0, or 4 with the known movers alone: os/exec (Q4) and LX's predicted `NativeFieldPortAlias.csproj` +3/-1 (Q12). LX checks its OWN restore only (review round 1: the whole-tree assert aborted at the rows' proof pages) | |
| linux P2 | runtime, pprof, TBS x10 | 10810 + 73; 147 + 7; 10/10 disclosed; exit 0 | M: runtime 6834 s |

NONZERO LEGS expected by design: FXc1, FXc2, SIc, NVR. **Nothing else** (PUB and CC are gated; PUB's control is not
re-run). The battery exits 6 otherwise. Findings: M's list, plus: PUB off its verdict or floor; PUB's SDK not 10.0.*;
CC without an `ok` line; NGF-j0 off any of its four reads; E's editorconfig-written above 0; CNR's skip list not the
derived one; PRE-D's CNR control (cnrexpect at the base != M's 785) or reconciliation off.

## 6. Deliberately NOT in the battery

| Not carried | Why |
|---|---|
| M's G GOTRACEBACK=system control, L's HOP and SPB, the C1 token probe, L's TE signatures | as at M (section 6 of M's README); g-slog-roster-tc0 edits `run-validated-sweep.ps1` comment lines only (read with git), so SPB stays out |
| The i9's uuid pack (NGE) | C2's S3b / ID-pattern seats are acceptance-read by NG and the J0 reads; a pack step needs a COORD-written GO |
| PUB's control arm (the single-file hang at 2bb2e5f825's template) | documented, not re-run (MS15); the hang root is COORD's i7 item |
| The hang-root repro and the full BoGo control | COORD's own i7 items after M (L:163, L:166), not train legs |
| crypto/tls on the i9 | the standing ban; the owner's limits experiment is separate |
| The MS13 refresh | a bank step after the battery, never a leg (N10) |
| A linux log/slog reading | retired with its pin (N3); the reviewer recommends keeping a plain row reading (section 7 Q3, owed) |
| Any darwin RUN | a mac CI dispatch (MS10) |

## 7. Open questions: OWED COORD RULINGS

1. **Row 23's note AND H4 (MS2): ONE decision** (review round 1; questions 1 and 2 of the derive). The note
   `stack-on i9-crosspkg-promoted-forwarders` fails the table (ee52639557 is cut on e2008427b1, a9cc98c1e3 is not its
   ancestor: re-read in review round 1, rc 2, one problem), and the row's three Go-only directories make PRERES die on
   H4. Editing the note to `after i9-crosspkg-promoted-forwarders` passes the table (the assembler now verifies an
   `after` note names an EARLIER row: review round 1) but leaves the fixup to die on H4, so it is never applied ALONE.
   H4's route (b) (commit the emission and register the projects at the union's converter) needs the i9's seat in R's
   tree, which would make the existing `stack-on` TRUE. **Reviewer's recommendation: (b) if R can push before the cutoff**
   (re-cut on a9cc98c1e3 with committed emission and registrations, the stack-on note kept and now true; row 23 becomes a
   registration seat for MS14 and moves CNR's N by 3), **otherwise (c): row 23 waits for TRAIN O.** (a) (move the repros
   out of the behavioral root) remains M's third route.
2. (merged into 1)
3. **The linux log/slog reading (N3).** COORD's 18:25 note retires the linux driver's log/slog leg with the pin; at N
   then NO lane reads log/slog on linux at the new default, and N is the train whose seat changes that row's config
   (g-slog-roster-tc0's commit measured linux at 8f46a9adae only; N adds golib seats on the frame and reflection path:
   g-cctor-init-frame, i9-gomethodvalue-nilfunc). **Reviewer's recommendation: keep a plain reading** in L4,
   `NAMED_NOW='TestSetDefault TestPanics TestCallDepth'; PASS_NOW="$NAMED_NOW"; rowleg log/slog 30m 240m; NAMED_NOW='';
   PASS_NOW=''` (EXPECT_V=roster reads 'linux: 199 + 17' at 582de36d1b, `tiered=False`; a few minutes on P1 at M): what
   the 18:25 note retired is the template's pin-deciding TC0 leg, whose purpose is done. Then restore the brief's L4 row
   and its section 4 control sentence. NOT applied (the leg stays retired until ruled).
4. **os/exec on P1's box (86 + 2 vs the roster's 87 + 1, Q31 host-conditional).** The driver raises it as a MOVER again
   (exit 4), so P1's exit can never be 0 and a different 86 + 2 cause would look identical. **Reviewer's recommendation:
   a stated exception keyed by NAME, not by count** (like `OS_ROOT_EXPECT`): accept exactly 86 + 2 only when the fresh
   record reads `TestExtraFiles go=pass cs=skip` (NAMED_NOW / named()) and no other divergence; anything else stays a
   MOVER; the brief says which box it applies to. NOT applied.
5. **X_ROWS=encoding/json** was M's ruling (two-box read for G's line attribution). Carried as regression (about 2 min);
   drop it by ruling.
6. **E_BISECT_KNOWN is empty**: if leg E reads above 0 at N, no arm runs by default (a finding names the derived
   converter seats). Rule a default if one is known by then.
7. **PUB's environment** (pwsh with DOTNET_ROOT unset, dotnet10 first on PATH) is INFERRED from the battery's other pwsh
   legs, not measured under this exact environment. The leg stamps the SDK it resolves; a NOT MEASURED publish is read
   as the instrument first.
8. **EXP_DESC / EXP_TH** for the fixup's optin check (TO BE SET or left reported).
9. **MS13**: confirm the refresh commit rides BEFORE master's fast-forward with no re-read (the argument: the refreshed
   bytes are what the legs compiled at that head), and that the linux lanes' re-emission stays out of it.
10. **Outside trainN (doctrine/template, not edited here):** the repo template `.claude/coord-scripts/templates/linux-legs.sh`
    lines 278-280 still carry the retired log/slog TC0 leg; `measurement-discipline` SKILL.md :40 / :99 cite the retired
    pin (L:135). A doctrine-batch item.
11. **MS19**: warm the i7's module cache with hashset v1.0.0 before the fixup, or let the first build fetch it (review
    round 1 read the cache: hashset v1.0.0 is ABSENT on 2026-10-03, so the fixup's first build WILL reach the proxy).
12. **LX's first reading: NativeFieldPortAlias.csproj** (review round 1 corrects this question's premise). g-publish-keep's
    tip 3ced87fd40 DID regenerate the six linux-only csproj (each +3/-1, like every other behavioral csproj: read with
    git), so L:137's "6 linux-only (WSL) pending" no longer holds. The one predictable LX mover is NativeFieldPortAlias.csproj
    (+3/-1: c2-native-array-view cut it on 8f46a9adae with `<PublishReadyToRun>true</PublishReadyToRun>` and no TrimMode;
    g-publish-keep cannot touch a file its base lacked, the windows fixup cannot regenerate a GoPlatformExclusive("linux")
    package, and c2-nugetgo-id-pattern's PackageId change reaches libraries only: all seven are Exe projects). The brief now
    PREDICTS it (still a MOVER). **Question: should the union carry NativeFieldPortAlias's linux re-emitted csproj, or LX
    stamp it a known mover?** Reviewer's recommendation: C2 adds the 3-line linux re-emission to c2-native-array-view, or
    p1-linux-only-goldens is seated carrying it, before the cutoff (LX's 0-hunk gate keeps its meaning); otherwise name
    the class in the brief's LX row and END line as known.
13. **MS21's four routes, before the map** (review round 1; the evidence is in MS21). (1) BOARD rows 4 / 9: reviewer
    recommends G re-cuts g-trim-annotations without c2e7cef853 (the BOARD entry lands as a docs commit after N), or (b) a
    re-cut stacked on c2-native-array-view. (2) + (4) visitAssignStmt.go and the HashSet compile break: reviewer
    recommends ONE P1 re-cut of p1-hashset-module on a local merge of b9e2d5ffa1 and 1c182b5ca2 (`stack-on
    g-using-static-alias-guards stack-on c2-multi-func-define`), else a ruled follow-up on b9e2d5ffa1, else TRAIN O (owner
    priority: hashset is the first nugetgo mapping). (3) projitems rows 17 / 27: reviewer recommends P2 re-cuts
    p2-n-cleanup (or its projitems commit) on 537abad2a2, `stack-on r-module-driver-gomod-less`, row 27 still last. Three
    lanes (G, P1, P2) must push before 10:00 for N to carry all of them.
14. **Re-cuts (M's doctrine) or a narrow ruled pre-resolution for N?** tN-assemble.sh merges each row `-S --no-ff` and
    exits 3 on any conflict; TRAIN G had a scoped path (`tG-assemble.sh`'s `resolve_from <rehearsal commit>` per
    seat|path). Reviewer's recommendation: re-cuts for the two converter-source conflicts and the HashSet break (the BOTH
    and COUNT arms then read clean merges), and take the BOARD entry out of the seat rather than build machinery; if COORD
    prefers machinery, scope it to append-only ledgers (keep both appends, asserted by the BOTH arm) and record it as a
    ruling. Not built here.
15. **MS20: the template-class csproj.** `CSPROJ_TEMPLATE=accept` in the fixup (the mechanism is built, stop by default;
    reviewer's recommendation) or a follow-up seat that regenerates the 13 (14 with the toml row) windows-measured csproj
    before the fixup.
16. **Item N7's premise: is P2's per-file `.editorconfig` converter emission?** COORD's notes say yes (L:83 "a compiler
    config entry the converter emits only where it knows the pattern", L:87, L:92, L:96), and review round 1 corrected the
    scripts' "hand-written, never emission" wording. At the 27-row list the class has no effect (no `.editorconfig` under
    src/core at the base or in any row; no `claude/p2-*warning*` ref on the remote, read 2026-10-03). Reviewer's
    recommendation: yes; if p2 converter-warning-clears seats at N, make `.editorconfig` ordinary emission in
    `emitdrift.py` (drift-compared, copied by `tN-regen-apply.py` under REGEN_ALLOW), drop the editorconfig-written die
    (fixup step 4) and finding (leg E), and have step 5 and leg 4 account for behavioral `.editorconfig` files the converter
    writes (step 5's 'CNR left UNTRACKED emission' die is mislabelled H4 for them). If it does not seat at N, the check stays
    as worded now. NOT applied.
17. **COORD's own table edits** (the derive and this round never edit `tN-seats-draft.txt`): add the c2-toml-test-host
    row before p2-n-cleanup (MS18); drop `(owes go2cs.slnx)` from row 22's note (a9cc98c1e3 already registers its four
    projects in go2cs.slnx; a notes-only edit keeps the hash); the routes of Q1 and Q13 re-point rows (new shas, a new
    hash, a new map).

## 8. What was checked, and what was not

- `bash -n`: all nine `.sh` pass. `python -B -m py_compile`: `tN-helpers.py`, `tN-regen-apply.py`, `emitdrift.py` pass.
  Every file is LF; `tN-ng-parse.ps1` is pure ASCII.
- **The assembler's TABLE_ONLY dry run** (02:36, `coord-scratch/tN/derive/tableonly/table-only.log`): rc 2, one problem
  (row 23's stack-on); row 13's stack verified; every remote tip at its seated sha. On a scratch variant with row 23's note
  as an order note (`seats-variant-after.txt`): `TABLE OK: 27 rows ... rows-sha256=d62f0b3453e0`, rc 0.
- **cnrexpect's control** at 8f46a9adae `--base aa0a07d5fd`: enumerated 791, skipped 6 (the six by name), n 785, added 7
  (M's seven): TRAIN M's run3 CNR line exactly.
- **testsrc-refresh smoke** on M's run3 `S-rewrites.patch` + `T-rewrites-U0.patch` (output to scratch, nothing applied):
  338 files, refused 0, conflicts 0, 27 excluded (19 production, 2 proof pages, 5 review siblings, 1 README).
- **The roster arms' predictions** at 582de36d1b against 8f46a9adae with the helper's own regexes: 225 / 225 rows, 223 /
  223 linux annotations unmoved; execution annotations log/slog -> none; the H5 sentence reads 0.
- Read with git (2026-10-03): each row's footprint (merge-base..sha); no row edits Goroutine.cs (H3_TOKENS empty); no
  `.editorconfig` under `src/core` at the base or in any row; the GoDefaultGodebug set is the same 3 files at five seat
  tips; NativeFieldPortAlias carries `[GoPlatformExclusive("linux")]` and is not in go2cs.slnx (like the six); the landed
  host seats' merges a92237f025 / 89fded79dc and H2's testConversion.go change (+31 / -4 against 02a0b44467); the roster
  guard prints `0 with an execution config` at 582de36d1b; `check-published-output.ps1`'s verdict line and NOT MEASURED /
  HUNG wording at 3ced87fd40; C2's J0 defaults at a7873ee6c2; comparison-classifier's go.mod (no requirements).
- **NOT run:** every script for real; the LX leg and the UF carve-out (no linux box here); PUB in its environment; the
  map; any precheck against a union (none exists).

**Review round 1 (2026-10-03; evidence under `coord-scratch/tN/derive/review1/`):**
- git merge-tree (old 3-way form, git 2.35) re-read the four MS21 items on their real bases: BOARD (base e2008427b1:
  one marker set, G's block vs C2's, above the raw guard), visitAssignStmt.go and go2cs-src.projitems (base 8f46a9adae:
  one marker set each, the hunks quoted in MS21); b9e2d5ffa1 x a7a7a197dd and 1c182b5ca2 x b9e2d5ffa1 merge clean. The
  unqualified-HashSet scan over every row's added `src/go2cs` Go lines finds b9e2d5ffa1 (2 lines) and nothing else.
  `git for-each-ref refs/coord/tN-map` is empty (no N map has run). Every remote tip still at its seated sha.
- **The REG arm** (refactored as `reg_arm`) was controlled on real blobs (`regarm_control.py`, the module's own
  function): over go2cs-src.projitems, 11 rows touch it and the seat-removed set is exactly HashSet.go + the five guard
  tests; a correct union reads lost 0 / back 0 (M's formula: 6 lost); a kept removed key reads `back` by name; a
  dropped base key reads `lost` by name.
- **csprojdrift**: at `8f46a9adae..6235912709` it read `UNEXPLAINED ... OTHER D GenericTests.csproj`, rc 1, before the
  fix; after it `seat-delete=1 ... TEMPLATE DRIFT ONLY`, rc 0. Negative: `8f46a9adae..2526a4efe2` (U is the deleting
  commit, so no commit below U explains it) reads `OTHER-deleted=1`, rc 1.
- **csprojtemplate**: a synthetic regeneration of UsingStaticNamespaceAlias.aliaslib.csproj (b9e2d5ffa1) read against
  each template seat's own delta: G's template lines vs head 3ced87fd40 (791 csproj, 1 removed / 3 added delta lines) rc
  0; the PackageId swap vs head a7873ee6c2 (55 csproj) rc 0; negatives rc 1 each: a planted line, the csproj absent from
  the added list, the PackageId swap read against G's delta alone. (A head holding BOTH deltas exists only on the union.)
- **TABLE_ONLY** on the real list (288 s, the box under load): rc 2, the same ONE problem (row 23). A planted list
  (row 23's note as `after i9-...`, an early row noted `after p2-n-cleanup`, a landed row appended): rc 2, exactly
  `'after p2-n-cleanup' names a LATER row`, `ordered after row 22 i9-crosspkg-promoted-forwarders (declared; order
  verified)` and the landed line, 227 s in all (the patch-id skip). MS9c control 1 printed its three dup-control lines.
- The i7's module cache, exact paths only: x/mod v0.33.0 and x/tools v0.42.0 present, **hashset v1.0.0 absent**.
- `bash -n` on every `.sh`, `compile()` on every `.py` (no `__pycache__` written; the derive's 148 KB `.pyc` was deleted).
