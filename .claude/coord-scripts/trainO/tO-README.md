# TRAIN O battery: draft summary

**DERIVED 2026-10-03 (19:55 to 21:30 Central) from TRAIN N's script set** (`hnd/.claude/coord-scripts/trainN/`, frozen
while N's battery runs: never edited) by a workflow agent, against N's battery as it stood at the derive
(`coord-scratch/tN/bat1`, launched 12:06, RUNNING), the O pre-map (`trainO/tO-premap.md`, 19:50, read on N's union
59ee0d21bf as a stand-in base) and COORD's notes (`trainL/tL-seats-draft.txt`, 2026-10-03 from 04:00 on, plus the
mailbox to 20:11). **Nothing here was run for real.** The commands run: git reads (`fetch` through `/h/Projects/go2cs`,
`ls-remote`, `show`, `log`, `diff`, `merge-base`), `bash -n` on every `.sh`, `compile()` on every `.py`, and two pwsh
probes for the PUB leg's environment (section 8). N's worktree `/h/go2cs-tmp-coord/tN` was not touched. Every change
against N is in `tO-CHANGES.md` (O1 to O14, D1, D2), the verification in `tO-DERIVE-REPORT.md`.

**Naming.** N's manual steps MS1 to MS21 keep their numbers where they are still procedure (MSn carries N's MSn); O's new
ones are **MS22** (a NEW per-file .editorconfig in a behavioral project) and **MS23** (the release-smoke CI dispatch).
Seat notes: **O:n** = `trainO/tO-seats-draft.txt` line n; **L:n** = `trainL/tL-seats-draft.txt` line n; **PM** = the
pre-map, `tO-premap.md`.

Union: `/h/go2cs-tmp-coord/tO`, branch `claude/coord-trainO-union`, base **TRAIN N's LANDED master** (N's union
59ee0d21bf + fixup-2 `r-ps51-selfdesc-count` 1ddbac142d + the bank step + the MS13 refresh). **It does not exist at the
derive, so every script takes it as a REQUIRED variable** (`BASE` in the assembler, the map, the fixup, the module legs
and the two lane drivers; `MASTER` in the battery) and no script of this set writes a base sha (O1). One signed merge per
row of the seat list, in row order. **The list is the 35-row draft** (rows cut on THREE bases, 8f46, 59ee and e200, and
three on N seats; tips read by ls-remote 21:24 Central, every tip at its seated sha). Its rows hash is
**`rows-sha256=676e5ba3ec40`** AS CUT (review round 1: r-docs-refresh at its new tip cdb9eb8a23 and
i9-generator-skip-records added below F; the 34-row derive list read `7391e9e7058d`); six rows owe a re-cut or a new ref
(MS21) and MS24 proposes one more row, so the hash moves before the map. REVIEW ROUND 1 is `tO-CHANGES.md` section 5.

## 1. The order of work

| Step | Command (from a FRESH per-run copy of this folder + the seat list) | Wall |
|---|---|---|
| 0 | N lands. `BASE=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master \| cut -f1)` (read, never typed; floor 15), then the checklist's step-0 ASSERTS (review round 1: N's `refresh: TRAIN N` and fixup-2 inside BASE; the assembler and the map also refuse a BASE with a train-assembly commit above it on origin/master's first-parent line unless `BASE_BEHIND_OK=1`). Re-run the pre-map at BASE (PM section 7), then MS9c's table controls and the real list: `BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 bash tO-assemble.sh` (EXPECT `TABLE OK`), then `BASE=$BASE SEATS=<the list> bash tO-conflict-map.sh`: the map keeps a fully clean union under `refs/coord/tO-map/rows-<hash>`; **the assembly REFUSES a list no map has read** | minutes |
| 1 | The manual steps of section 4 that come BEFORE the map, the assembly or the fixup: **MS21 (the six owed re-cuts / new refs; single-parent routes preferred: precheck's MULTI-BASE arm is new and unrun)**, MS1 (walk the literal sites), MS2 (H4: the trio boards as a set), MS14 (registration files), MS19 (hashset: the i7 cache, the HashSet scan), MS20 (the template-class csproj ruling), MS22 (the NEW .editorconfig ruling) | |
| 2 | `BASE=$BASE bash tO-assemble.sh` (fetch; `ls-remote` for the seat tips; signing preflight; the table, now with O2's two checks; the map refusal; the merges; shape, ORDER and MAP TREE). Writes `coord-scratch/tO/assemble-maptree.txt` | minutes |
| 3 | Ruled follow-up merges, signed `--no-ff`, each listed in `tO-follow.txt`, each on a branch of its OWN: **none planned** (every O route is a re-cut) | |
| 4 | `BASE=$BASE EXPECT_HEAD=<the assembled head> bash tO-fixup.sh` (REGEN_ALLOW composes from the four footprint rows; `CSPROJ_TEMPLATE=accept` and `EDITORCONFIG_NEW=accept` only by COORD's rulings, MS20 / MS22) | INFERRED about 1 h 30 m |
| 5 | **Push the union; GO to the i9 (`tO-lane-brief-i9.md`) and to P1/P2 (`tO-lane-brief-linux.md`)** with `UNION`, `FIXUP` and **`BASE`** (the drivers require it), before the battery, so the lanes run beside it. **MS23: dispatch the os-matrix release-smoke run at the union** (CI; it runs beside everything) | |
| 6 | `MASTER=$BASE EXPECT_HEAD=<fixup sha, 10 chars> bash tO-battery.sh`, from the fixup's run folder or a copy holding the SAME `tO-seats-draft.txt` and `tO-follow.txt` | INFERRED 11 to 12 h (N's bat1 reached the sweeps' end at 19:50 from a 12:06 launch, T legs after; O adds WE + WEc about 1 h, IDC seconds, 6 more projects in leg 5, and PUB actually runs); default deadline launch + 14 h |
| 6b | Only if a red needs a tree change AFTER the push (floor 9): `fixup-2: TRAIN O` ON TOP (`FIXUP_N=2`). Then section 4b | |
| 7 | Landing: MS23's release-smoke verdict read; merge master into the union (MS11), the landing precheck BEFORE the bank step, the bank step, **MS13 the refresh of committed -tests sources**, then master fast-forwarded | |

Per-run copy (floor 4): copy the whole folder, and `hnd/.claude/coord-scripts/trainO/tO-seats-draft.txt` into it.
Every script refuses a launch from the worktree, from `hnd`, from any `wf/draft*` folder or from the main checkout.
The fixup, the battery, a standalone `tO-modules-legs.sh` and a hand-launched `tO-emitcheck.sh` take one shared lock
(`coord-scratch/tO/.battery.lock`) and, before it, read the LIVE gate (`tO-helpers.py live` reads every train's
`coord-scratch/t*/.battery.lock`, so an N battery still running refuses O's fixup; `LIVE_ACK=1` runs past a hit by
COORD's explicit call). The battery refuses a run folder that already holds a `SUMMARY.txt`.

Exit statuses (unchanged from N). Battery: 0 clean; **6** a leg outside the four expected-non-zero controls (FXc1,
FXc2, SIc, NVR) is non-zero, or a FINDING exists; 2 abort; 3 a leg's disk or build abort; **5** a wall cap fired (PUB
30m, WE 150m, WEc 45m among them), or a process is still running from the module out root after MOD: the lock is KEPT;
9 the deadline. Assembly: 2 a table problem or no map for the list; 3 a merge that failed; 4 a shape, order or MAP TREE
miss. Module legs: 4 on any FAIL verdict. Linux driver: **4** on any MOVER or freshness fail. i9 shard: **4** on a failed
row or a `SOFT:` line.

Environment variables (N's, with O's values and the new rows):

| Script | Variable | Default | Meaning |
|---|---|---|---|
| assemble, map, fixup, modules, i9, linux | `BASE` | **required** (O1) | TRAIN N's landed master; must resolve, be on origin/master (assemble, map) and on the union's first-parent line (lanes) |
| battery | `MASTER` | **required** (O1) | the same base; must sit on the union's first-parent line |
| battery | `N_RUN` | `coord-scratch/tN/bat1` (O4) | TRAIN N's battery run folder: its SUMMARY, S / T patches and host records are the previous-train baselines (FX floor, CNR control, TE / TE-T, HOSTWALL, TL-WALL), stamped in PRE |
| map | `SEATS` | COORD's live `hnd` list | the list the map reads |
| assemble | `TABLE_ONLY` | unset | `1` stops after the seat-table check: nothing created, nothing merged (MS9c) |
| assemble | `TIP_MOVED_OK` | unset | refs whose REMOTE tip descends from the seated sha, acknowledged by COORD |
| assemble | `MAP_UNION` | derived | normally unset: the union `tO-conflict-map.sh` kept for the same rows |
| assemble, map | `BASE_BEHIND_OK` | unset | (review round 1, F2-1) `1` runs past a BASE that has a train-assembly commit (`Merge claude/... into TRAIN X`, `fixup...: TRAIN X`, `refresh: TRAIN X`) above it on origin/master's first-parent line: COORD's explicit call, stated (the default refuses a stale base such as N's union 59ee0d21bf) |
| assemble, fixup | `SIGN_PROBE` | `1` | `0` skips the real probe signature |
| fixup, battery, modules, emitcheck | `LIVE_ACK` | unset | `1` runs past the LIVE gate; stamped |
| fixup | `EXPECT_HEAD` | required | the assembled union head; with `FIXUP_N` >= 2, the pushed head |
| fixup | `FIXUP_N` | `1` | `k+1` = a fixup ON TOP of the k already at the head; subject `fixup-N: TRAIN O` |
| fixup | `PROSE_PATCH` | unset | a comment-and-prose patch; none is owed at O |
| fixup | `REGEN` | `apply` | `skip` leaves the corpus alone (COORD's explicit call) |
| fixup | `REGEN_ALLOW` | composed from the seat list | at the draft: **the 32 paths of four rows' `CORPUS FOOTPRINT` notes** (xsys-libc 1, B1 1, p2 19 incl. 11 `.editorconfig`, fm-record 11), stamped in PRE (MS4) |
| fixup | `SIBLINGS` | `report` | `refresh` also refreshes the `*.cs.auto` siblings only the union arm rewrites (p2 commits `sync/poolqueue.cs.auto`) |
| fixup | `GOLDENS` | `regen` | `skip` leaves stale goldens for leg 4 to report |
| fixup | `GOLDEN_CLASS` | `gframe` | `any` re-baselines a moved golden that carries another line kind, after COORD has read it (MS4b) |
| fixup | `CSPROJ_TEMPLATE` | `stop` | `accept` carries the 18 csproj the union added that its CNR regenerated with the union template's delta alone (MS20; the class is now also derived from the template file: D2) |
| fixup | `EDITORCONFIG_NEW` | `stop` | (O9, MS22) `accept` carries a NEW per-file `.editorconfig` the union CNR writes in a behavioral project no seat gave one |
| fixup | `EXP_DESC`, `EXP_TH` | unset | optin.py check's two counts; unset = reported (TO BE SET BY COORD) |
| fixup, battery | `EXEC_ROWS_EXPECT` | the count of `EXEC_RULED` | **0** (as at N) |
| fixup, battery | `SEATS_EXPECTED` | unset | optional second derivation of the seat count |
| battery | `EXPECT_HEAD` | required | the fixup commit, or the newest `fixup-N` on top of it |
| battery, modules | `DEADLINE` | launch + 14 h | an ABSOLUTE local time `'YYYY-MM-DD HH:MM'` |
| battery | `CNR_EXPECT_N` | **derived** | PRE-D derives it with `cnrexpect` at HEAD and stamps its reconciliation (PREDICTED 819 at the draft) |
| battery | `E_BISECT_SEATS` | `E_BISECT_KNOWN` = `r-funclit-interface-target g-method-value-fm-record` | the rows bisected if leg E reads above 0 (their footprints are the fixup's to regenerate). ROW NAMES: if G's R5 re-cut comes back under a new ref name, pass `E_BISECT_SEATS='r-funclit-interface-target <that ref>'` at launch (review round 1, F2-5; else PRE-D's FINDING, exit 6) |
| battery | `WE_FLAVOURS` | `linux,windows,darwin` | (O8) leg WE's flavours |
| battery, modules | `REALMOD_TEST_TIMEOUT` | `2m` | the real-module legs' `-test-timeout` |
| battery | `REALMOD` | `1` | `0` skips XS/XM by COORD's explicit call |
| linux P2 | `RT_CHILD`, `RT_OUTER` | `210m`, `450m` | the full runtime row's deadlines |
| battery | `PRECHECK_MODE` | `abort` | `warn` runs past a failed PRE assert, stamped |
| battery | `I9_PATCH`, `I9_BASELINE` | unset | the i9's TE patch; its TRAIN N patch as the baseline (`coord-scratch/tN/i9-patches/tN-tracked-changes-U0.patch`) |
| battery | `SWEEP_ROW_CAP` | `4h` | outer wall cap per sweep row |
| emitcheck | `TO_LOCK_HELD` | unset | set to `1` by the fixup and the battery |

## 2. Why the union owes a fixup

| | Fact | What breaks without it | Where |
|---|---|---|---|
| G1 | Goldens a seat x seat interaction moves. **More likely than at N**: 22 rows were cut on 8f46 / e200 and their goldens predate N's converter seats; MethodValueReceiverSnapshot is composed by B1 (main.cs + target), fm-record (package_info.cs) and r2r-restore (csproj), and no single seat's CNR saw all three (PM section 4); c2-gen-kind-name-collision's generator ran no CNR at its seat | leg 4 reads them CHANGED | fixup step 5: the union's CNR measures; `GOLDEN_CLASS=gframe` STOPS on any non-G-frame hunk; COORD reads `gold-cs.patch` and relaunches with `GOLDEN_CLASS=any` (MS4b) |
| G2 | Corpus the four footprint rows carry from OTHER converters (p2's e200, fm-record's 8f46) or not at all (B1's os_windows.cs) | leg E reads it union-attributable | fixup step 4: REGEN_ALLOW composes from exactly those rows' notes; anything else STOPS for COORD (MS4) |
| T1 | The 18 behavioral csproj the O rows add carry the pre-N template (no TrimMode lines; read with git, every one Exe) | step 5 dies 'CNR moved a csproj', and N's reader would have classed all 18 OUTSIDE the template class (at O the seats' own csproj edits hold only r2r-restore's ReadyToRun line) | **D2**: `csprojtemplate` admits an added line the HEAD template holds literally and a removed line the template held at that csproj's cut; `CSPROJ_TEMPLATE=accept` is COORD's ruling (Q12) |
| E1 | `.editorconfig` is CONVERTER EMISSION (p2's warningEntries.go) | N's emitdrift kept it out of drift and N's fixup DIED on any written one | **O9**: ordinary emission in emitdrift / emitcheck / regen-apply; REGEN_ALLOW admits p2's 11; step 5 carries a moved behavioral one like a golden and NAMES a new one (`EDITORCONFIG_NEW`, MS22) |
| E2 | (review round 1) The converter DELETES its own `.editorconfig` when a package's model comes out empty; at O, PREDICTED (inferred from source): p2's `vendor/golang.org/x/sys/cpu/.editorconfig` on darwin, because c1-darwin-xsys-libc initializes the two fields its one CS0649 section covers | a deletion writes nothing: leg E read 0, step 4 copied nothing, the stale file stayed committed and WE arm B reads it STALE on darwin | emitdrift.py now lists a committed `.editorconfig` a conversion deleted (`DELETED`); leg E reads it union-attributable and step 4 REFUSES it by name (the fixup never applies a deletion: floor 8). The remedy is a ROW (MS24) |
| D1 | A2's red commit b9da3e6a2d registers A1's `CheckPtrToAnonStructPtr` (3 lines x 3 files), the same hunk A1 adds (old start 1577 in each, read with git); git keeps one copy | precheck's COUNT arm reads expect = head + 3 and FAILS a correct union (PRERES, step 6, PRE-1, the landing) | **D1**: the COUNT arm credits a -U0 hunk the row and the union side of its merge hold identically (git's own rule), stamped `(identical-insert credit -3)` |
| O3 | A re-cut on a LOCAL MERGE of two of {BASE, earlier rows} (PM's two-parent routes for R4 and R5) has several best common ancestors | N's precheck took one (`merge-base` without `--all`) and over-counted the other's registrations: N's Q20 | **O3**: `merge-base --all`; a MULTI-BASE row's own change is synthesized from all its bases (COUNT and REG), stamped `COUNTBASE`; the assembler's table NOTEs such a row. Unrun: single-parent re-cuts remain the recommendation |
| META | fm-record and p2 change package_info.cs GoPositionMap lines without stdlib-metadata.txt | none expected (the asset carries aliases and GoImplement records only) | fixup PRERES reads `TestStdLibMetadataInSync` by name; a STALE asset is regenerated and rides |
| H4 | r-atlas-batch-repros adds two Go-only directories | precheck's H4 arm refuses the union | none at the draft: A2 and A1 each commit one directory's emission (the trio boards as a set, as ruled 04:33) |
| R1 | p1-hashset-module removes HashSet.go from go2cs-src.projitems | nothing (N's `reg_arm` knows seat-removed keys) | PREDICTED `seat-removed=[p1-hashset-module:...HashSet.go]` |
| DARWIN | The two C1 banks add 22 darwin annotations to the roster that N's bank step also edits | a darwin annotation a 3-way drops reads nowhere (the ROSTER arms compared columns and linux annotations only) | **O6**: precheck's DARWIN arm expects the base's annotations + each row's own darwin edits, row by row |

## 3. Files

| File | What |
|---|---|
| `tO-seats-draft.txt` | The proposed O table (35 rows: pre-map order v2 + review round 1; slots and PENDING as comments). DRAFT for COORD; the step-1 candidate list is kept as `tO-seats-candidates-step1.txt`. |
| `tO-premap.md`, `tO-premap.sh`, `tO-regcheck.py`, `premap-run1/` | The worktree-free pre-map (this workflow's step 2), not edited here. |
| `tO-conflict-map.sh` | The map; base REQUIRED; refuses a row carrying another train's assembly commits (O2). |
| `tO-assemble.sh` | Assembly; base REQUIRED and on origin/master; O2's two table checks; the literal-site list. |
| `tO-follow.txt` | The ruled follow-up merges: EMPTY. |
| `tO-fixup.sh` | The fixup: base REQUIRED; REGEN_ALLOW admits `.editorconfig`; no editorconfig die; step 5's `.editorconfig` classes and `EDITORCONFIG_NEW`; O's texts. |
| `tO-regen-apply.py` | Copies the union converter's emission over listed corpus files (names; the `.editorconfig` note). |
| `tO-battery.sh` | The i7 battery: N's legs + **IDC** (O7) + **WE / WEc** (O8); MASTER REQUIRED; N_RUN baselines; PUB fixed (O5) and PUB_MIN 6 (O4); leg E's editorconfig a reading (O9); the O leg map. |
| `tO-helpers.py` | N's readers; precheck: MULTI-BASE (O3), identical-insert credit (D1), DARWIN arm (O6); csprojtemplate: the template file's delta (D2); labels. |
| `tO-emitcheck.sh`, `emitdrift.py` | Leg E; `.editorconfig` ordinary emission (O9). The standing-drift classifier (N's FIXUP STOP 1) is kept unchanged with its hermetic control. |
| `tO-modules-legs.sh` | Leg MOD; BASE REQUIRED (the battery passes it). |
| `tO-i7-sweeps.txt`, `tO-i9-shard.txt` | **93** and **132** rows, byte-identical to N's (sha256 40c6ec39...3724 and 057c78c1...cdf3d, `cmp`). |
| `tO-ng-parse.ps1` | L's ParseFile reader (header only). |
| `tO-i9-shard.sh`, `tO-i9-te.sh`, `tO-lane-brief-i9.md` | The i9 driver (BASE required; the execrows control at a derived ref: O10), its TE reader, its brief. |
| `tO-linux-legs.sh`, `tO-lane-brief-linux.md` | The P1/P2 driver (BASE required; O10) and brief. |
| `COORD-LAUNCH-CHECKLIST.md` | From the seat table to master, with O's EXPECT lines, MS23 and the signing check by KEY. |
| `controls/` | M's two TE patches (unchanged) and the controls note (M-history pointers restored: O13). |
| `tO-CHANGES.md`, `tO-DERIVE-REPORT.md` | Every change from N; the verification. |

## 4. Manual steps (a check can name them; a person decides them)

- **MS1. The seat list; walk the literal sites when a row is added.** A row added, dropped, moved or re-pointed means:
  re-run the map BEFORE the assembly reads the list. **DERIVED at run time**: the converter tests the union adds (CB,
  LCn), the GolibTests classes (GT), the behavioral projects (legs 4, 5, LB), CNR's N and its platform-exclusive set, the
  nugetgo test scripts (NGa), the execution-config rows, `REGEN_ALLOW`, the map union, the host seats, the darwin
  annotation expectation (O6). **Still a literal: walk it when a row is added:**

  | File | Literal | What it is |
  |---|---|---|
  | `tO-helpers.py` | `EXEC_RULED` (empty), `H3_TOKENS` (empty), `S1_NONE` | the ruling; Goroutine.cs editors; hand csproj |
  | `tO-fixup.sh` | `REGEN_G_SEAT`, `REGEN_G_PKGS` (empty) | a named seat's uncommitted footprint |
  | `tO-battery.sh` | `CB_BASE`, `GT_NEIGH`, `GT_TESTS`, `CHECKDEAD_GUARDS`, `NEIGH`, `E_BISECT_KNOWN`, `X_ROWS`, `T_ROWS`, `HS_WANT`, `NPOST` (the 18), `PUB_MIN` (6), `IDC_MIN` (217), `WE_FLAVOURS`, the floors 24 / 68 / 26 / 62 / 1291 | as at N, plus O's |
  | `tO-linux-legs.sh` | `GT_NEIGH`, `LB_FIXED`, `LX_KNOWN`, the L4 / L5 / L6 rows, `OS_ROOT_EXPECT` | the lanes' hazard sets |
  | `tO-i9-shard.sh` | the `named` cases | test names read by name |
- **MS2. H4.** The trio r-atlas-batch-repros + g-float-untyped-const-compare + r-ptrptr-anon-struct-lift boards as a SET
  (ruled 04:33): either completing row alone, or the base row alone, leaves a Go-only directory and PRERES refuses.
  r-jwt-promoted-iface-repros is not a row (PENDING, Q9); if it is ruled in, it is a re-cut with committed emission and
  registrations (N's route (b)), never a follow-up.
- **MS3. DROPPED** (as at N): the roster sentence reads 'No row opts back out' with 0 annotated rows.
- **MS4. Read the `REGEN_ALLOW` the fixup stamps.** At the draft it admits exactly the four footprint rows' 32 paths
  (`src/core/<path>`; `.editorconfig` admitted since O). Any OTHER union-attributable path stops the fixup before a byte
  is written, with the evidence kept (`/h/go2cs-tmp-coord/tOemitfix`): COORD reads the list, then relaunches with
  `REGEN_ALLOW=<pattern>` or routes the path back to its seat. Three classes regen-apply REFUSES BY RULE, whatever
  `REGEN_ALLOW` says, which COORD should expect could appear (review round 1 names each one's route, F2-6):
  1. **a NEW emitted path** (rule 2), e.g. the union converter writing an `.editorconfig` for a package under `src/core`
     with none committed. `REGEN_ALLOW` cannot carry it (rule 2 refuses before the allow set matters; `EDITORCONFIG_NEW`
     covers behavioral project directories only). Route: a seat or a NEW ref commits the file (a row, as MS24's), or
     COORD rules a rule-2 exemption for paths ending `/.editorconfig` (e.g. `SRC_EDITORCONFIG_NEW=accept`, mirroring
     `EDITORCONFIG_NEW`, stamped and listed in the commit message): NOT IMPLEMENTED, a script change by that ruling;
  2. **a per-target difference of one path** (rule 4): the three targets share an `.editorconfig`, each run replacing
     only its own GOOS's facts. Route: never hand-merged; the seat that moved the facts re-cuts with the union's file;
  3. **a committed `.editorconfig` a conversion DELETED** (review round 1: emitdrift.py's `DELETED` class, refused by
     name): the fixup never applies a deletion (floor 8). Route: a row that commits the deletion (MS24; PREDICTED for
     `vendor/golang.org/x/sys/cpu/.editorconfig`).
  **MS4b:** `GOLDEN_CLASS=gframe` stops on the first moved golden: READ
  `gold-cs.patch`, then relaunch with `GOLDEN_CLASS=any`.
- **MS5. Review siblings and hand-owns.** `SIBLINGS=report` (RULED). HANDOWN must read 0 written: managed_impl.cs (p2,
  names, fm-record), value_impl.cs (B6, B7, K, names) and ж.PointerTokens.cs are hand-owns O rows edit.
- **MS6. DROPPED** (an M row).
- **MS7. `SetegidBroadcastSeam` is never re-baselined whole.** Linux-exclusive; P1's LB and LX read it. At O its golden
  target is P1's own (`p1-linux-only-goldens-targets`), so the residual exception should read no line.
- **MS8. If the row order changes**, adjacent inserts in a registration file can become one conflict hunk (MS14).
- **MS9. Run the two TE control patches** through `teattr` and `hunkclass` once before the first battery (checklist 2).
- **MS9b. Run the precheck's and the LIVE gate's controls once before the first fixup** (floor 13). PREDICTED at the
  assembled draft list (after MS21's re-cuts):
  1. precheck on the assembled union, `head` mode: rc 0, no NOTE; `ok COUNT` x6, with
     `r-ptrptr-anon-struct-lift:+N(identical-insert credit -3)` on Compile, Target and TranspileTests (D1's positive
     reading); `ok REG ... seat-removed=[p1-hashset-module:...HashSet.go]`; `ok ROSTER rows base=225 merged=225 ... none`;
     `ok ROSTER linux annotations ... none`; **`ok ROSTER darwin annotations base=<BASE's> merged=<+22> expect=<+22> (the
     base + 22 row-own edit(s): c1-darwin-pilot-bank, c1-darwin-wave2-bank) mismatched: none`**; `ok H4 ... 0`; a
     `COUNTBASE multi-base row` line ONLY for a row cut on a local merge, and (review round 1, F2-7) a `NOTE MULTI-BASE
     row <name>` line when such a row's diff touches the roster, a proof page or Goroutine.cs (the DARWIN, PROOF and H3
     arms read it unsynthesized: read that row's lines by hand). None is expected on the single-parent routes.
  2. The same command on a child worktree whose HEAD is BASE: PREDICTED rc 1 with one `FAIL BOTH <seat>: no first-parent
     merge` per row, FAIL COUNT / REG on the registration files, and **`FAIL ROSTER darwin annotations ... mismatched:`
     naming the 22** (the DARWIN arm's negative control). No credit is given there (no merges): D1's negative.
  3. The LIVE gate with a planted `tZ/.battery.lock`: rc 1 and one `LIVE lock ...tZ...` line; removed: rc 0, `none`.
  4. (D2) `csprojtemplate` is run by the fixup itself; its PREDICTED verdict at the draft is `files=<the 18> template-only=<the
     18> other=0`, with one `template delta at the cut ...` line per distinct cut naming the TrimMode comment and line.
- **MS9c. Run the seat table's controls once before the first assembly** (floor 13), from a per-run copy with
  `BASE=$BASE TABLE_ONLY=1 SIGN_PROBE=0 SEATS=<planted copy>` (nothing is created whatever the table says):
  1. a duplicated sha (append `dup-control|b95dd8d6e6|control`): three lines (no remote; the same sha as
     r-ptrptr-anon-struct-lift; an undeclared stack on r-atlas-batch-repros);
  2. a stacked row above its base (move c1-darwin-wave2-bank above c1-darwin-pilot-bank): `names no EARLIER row` and `is an
     ANCESTOR of the earlier row`;
  3. a landed row (append `landed-control|f6ce1a0ab7|control`, N's row 28, an ancestor of BASE): `already an ancestor of the
     base`; 3b. an `after` naming a later row;
  4. **(O2's positive control) `BASE=8f46a9adae BASE_BEHIND_OK=1`** (M's master, an ancestor of origin/master, so it passes
     the landed check; since review round 1 the BEHIND check would stop it first, hence `BASE_BEHIND_OK=1`, whose NOTE line
     is part of the reading): EXPECT one `carries <n> train-assembly commit(s) the base ... lacks` line for each of the seven rows cut on
     N's union 59ee (r-docs-refresh, coord-census-dotted-handle, c1-macos-flavors, c1-darwin-xsys-libc,
     c1-release-smoke-all-os, c2-nuget-followups, g-single-file-r2r-restore). Stated: the three rows cut on N SEATS
     (c2-defer-receiver-copy, i9-promoted-method-expr, p1-linux-only-goldens-targets) carry N's lane commits, not train
     commits, and are NOT caught by O2 on a wrong base; the base being on origin/master is their guard.
  5. **(review round 1, F2-1: the BEHIND check's controls)** `BASE=59ee0d21bf` (N's union, on origin/master once N lands,
     with N's fixup-2 / bank / MS13 commits above it) WITHOUT `BASE_BEHIND_OK`: EXPECT `ABORT: BASE 59ee0d21bf is BEHIND a
     landed train ... train-assembly commit(s) (... fixup-2: TRAIN N ... refresh: TRAIN N ...)`, rc 2, from the assembler
     AND from `tO-conflict-map.sh` (nothing created). Measured 2026-10-03 on today's master with M's equivalent:
     `BASE=e2008427b1` (M's union below its fixup-2) reads 1 hit (`8f46a9adae fixup-2: TRAIN M`); `BASE=8f46a9adae` and
     `BASE=origin/master` read 0.
  Then the real list: EXPECT `TABLE OK: <n> rows ... rows-sha256=<h>` (read `<h>` from the line).
- **MS10. Darwin is a mac CI dispatch**, not a leg of any box here; so is the release gate (MS23).
- **MS11. At landing, after merging master into the union:** `git diff --stat $BASE <that master sha>`, read whole;
  then `python -B tO-helpers.py precheck <tO> <that master sha> <run>/tO-logs/seats-effective.txt head` and
  `check-roster-format.ps1` under both editions with `EXEC_ROWS_EXPECT=0`, **BEFORE the bank step**.
- **MS12. DROPPED** (no prose owed).
- **MS13. THE REFRESH OF COMMITTED -tests SOURCES: a POST-BATTERY BANK STEP, as at N** (`tO-helpers.py testsrc-refresh`,
  unchanged): inputs = the WINDOWS rewrite patches of O's own runs at ONE union head (`tO-logs/S-rewrites.patch`,
  `tO-logs/T-rewrites.patch`, the i9's `tracked-changes.patch`); filter = committed test sources only; apply on the
  union after the battery, `--check-worktree`, one signed commit `refresh: TRAIN O -- ...` before master's fast-forward.
  At O it is expected SMALL (N's refresh is in the base): what O's converter rows move in committed test sources.
- **MS14. The registration files.** go2cs.slnx and the four BehavioralTests files take inserts from 16 rows (18
  projects); go2cs-src.projitems from 13 rows plus hashset's removal. KEEP EVERY SIDE'S INSERT. The line-count invariant
  is precheck's COUNT arm (with D1's credit and O3's synthesis), the key union its REG arm, the per-merge line check its
  BOTH arm. The pre-map's chain C read every registration file at expect on 59ee except Compile / Target / Transpile at
  -3 (D1, now credited). The append-only BOARD is a registration file too: O's three BOARD sections land as ONE docs
  row (R2).
- **MS15. PUB's context.** The single-file hang's ROOT was the golib lock inversion N fixed (g-typecache-lock-inversion);
  g-single-file-r2r-restore re-enables single-file R2R (0/10 hung on N's union); PUB_MIN 6 is its seat's count. The PUB
  leg itself never ran at N (rc=150, O5): O's is its first real reading on a union.
- **MS16, MS17, MS18. DROPPED** (N's rows).
- **MS19. p1-hashset-module: the converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0`.** Warm the i7's cache
  before the fixup (`( cd /h/go2cs-tmp-coord/tO/src/go2cs && go mod download )`, then `git -C /h/go2cs-tmp-coord/tO status
  --porcelain | grep -vc '^??'` EXPECT 0). The HashSet scan over every row's ADDED `src/go2cs` Go lines must print
  nothing once the hashset re-cut is a row (as cut it prints `c1-darwin-xsys-libc 2`):
  ```
  for r in $(grep -E '^[a-z0-9.-]+\|' tO-seats-draft.txt | cut -d'|' -f1,2); do sha=${r#*|}; mb=$(git merge-base $BASE $sha)
    n=$(git diff $mb $sha -- 'src/go2cs/*.go' | grep '^+' | grep -v '^+++' | grep -cE '(^|[^.A-Za-z_])(HashSet\[|NewHashSet\b)'); [ "$n" = 0 ] || echo "${r%|*} $n"; done
  ```
- **MS20. The template-class csproj: COORD's ruling before the fixup's step 5 (Q12).** 18 csproj (all Exe, TrimMode count
  0 at their own shas: read with git). `CSPROJ_TEMPLATE=accept` carries them (N ruled accept for its 16).
- **MS21. THE DRAFT DOES NOT ASSEMBLE AS CUT: six routes, BEFORE the map** (PM section 5; each a lane's re-cut or a new
  ref; the assembler refuses any conflict). **Prefer SINGLE-PARENT re-cuts**: a re-cut stacked on ONE earlier row (or on
  BASE / 59ee alone) has a unique merge base; a re-cut on a LOCAL MERGE of two of them is readable only through O3's new,
  unrun MULTI-BASE arm (N's Q20).
  1. **R2 (A2, BOARD)**: a NEW ref at **b967f9fa7f** (A2's BOARD-free parent; alone-clean and chain-clean in PM); G or COORD
     names it (e.g. `claude/g-float-untyped-const-compare-r2`; a new ref pushes, reads back, then announces). Its BOARD
     section rides the BOARD docs row. (Review round 1, F1-5: G's owed g-untyped-float-operators, ruled 20:37 'stack-on
     A2 0c4397a069', belongs on THIS new ref, not on 0c4397a069: a stack on A2's BOARD-only tip brings the BOARD conflict
     back and a second copy of A2's 30-line section; C2's planned yaml.v3 seat stacked on G's inherits it: Q15.)
  2. **R2 (the BOARD docs row)**: COORD cuts ONE docs row on BASE appending G's c2e7cef853 section (29 lines), A2's (30)
     and B2's (2) above the BOARD's final endraw (the slot in the draft). Guard: one raw, one endraw, endraw final, zero
     bare openers.
  3. **R3 (B2)**: R re-cuts r-typeswitch-collapsed-func-case stacked on c2-uint8-literal-conv 3bf2ff1c32 ALONE, without
     the 2-line BOARD row, projitems keep-both (sorted); its notes then declare `stack-on c2-uint8-literal-conv`. Assert
     projitems = base + 1 + 1.
  4. **R4 (c2-nuget-followups, docs/README.md)**: single-parent route: C2 re-cuts on r-docs-refresh's tip **cdb9eb8a23**
     ALONE (review round 1, F1-1: R's tip moved from 84fcc6373d at 20:39, the owner's macOS line; a re-cut on 84fcc6373d
     would fail the table once row 1 seats cdb9eb8a23, its declared stack naming a row whose sha is not its ancestor; tell
     C2 BEFORE it cuts, and r-docs-refresh is FROZEN from then), re-applying its `-nuget-map` rows onto R's text. Its
     line-314 sentence ('By default a `-recurse=nuget` run also asks the nugetgo.net registry ...; `-nuget-map off` opts
     out') STAYS IN THE RE-CUT (review round 1, F1-4: the derive's 'later docs row cut on the O pre-union' had no slot,
     and a ref cut on a local assembly carries map merges no table check admits), placed as its own paragraph or inside
     its `-nuget-map` rows with at least ONE unchanged line between it and line 315 (p2's insertion point): measured with
     `git merge-file` on the README blobs, a separate paragraph below the blank line at 316 merges clean with p2 (base
     e2008427b1). Its notes then declare its stack on r-docs-refresh (the `stack-on` token naming r-docs-refresh at
     cdb9eb8a23); the premap (chain) reads it before the map. The pre-map's two-parent route (a local merge of
     cdb9eb8a23 + 18264b3359) works only if COORD accepts O3 unrun (Q18).
  5. **R5 (G's pair)**: names-r2 on 59ee0d21bf (textually BASE for these paths) or BASE, keeping N's init-frame naming AND
     -fm / main.main in one function; fm-record-r2 stacked on names-r2 ALONE, leaving `encoding/json/package_info.cs` OUT
     of the seat (p2's line 70 and fm-record's line 71 are adjacent): the fixup regenerates it under fm-record's footprint
     (the expected reading: both lines moved). The footprint note is re-measured at the re-cut.
  6. **R6 (hashset)**: P1 re-cuts ONCE stacked on c1-darwin-xsys-libc 9e68c4a176 (single parent), converting every
     unqualified use (BASE's 304 lines in 46 files, xsys-libc's two), the LAST converter row. P1 (20:00): "hashset O re-cut
     waits for N to land".
  After the routes land: re-run `tO-premap.sh` (chain) with the new shas, MS9c, the map, MS19's scan. Each re-cut row's
  notes declare its stack by the `stack-on` token (B2 on c2-uint8-literal-conv, C2 on r-docs-refresh, fm-record on G's
  names re-cut, hashset on c1-darwin-xsys-libc): without it the table reads an UNDECLARED stack. If G's pair comes back
  under NEW ref names, the battery takes `E_BISECT_SEATS='r-funclit-interface-target <fm-record's ref>'` (checklist 5).
- **MS22 (O, E1). A NEW per-file `.editorconfig` in a behavioral project: COORD's ruling before the fixup's step 5.** If
  the union CNR writes one for a project no seat gave one (a project cut before p2's converter whose Go source holds one
  of the three facts), step 5 STOPS naming it (`edconf-new.txt`); `EDITORCONFIG_NEW=accept` carries it as an added file
  (listed `(5c)` in the commit message). NOT MEASURABLE before the union exists; 0 is the likely reading.
- **MS23 (O). THE RELEASE GATE IS A CI RUN, NOT AN i7 LEG.** O carries the 1.24.13.4 release's two seats
  (c1-macos-flavors, c1-release-smoke-all-os). The i7 battery exercises only their converter test (CB, C), golib's
  buildTransitive targets through every build (2b, GT, H7) and `push-nuget.ps1`'s pre-flight (NV51 / NV7); **nothing on
  the i7 runs `release-smoke.ps1`, the per-flavour pack or the walkthrough arm D**. So COORD dispatches, once the union
  is pushed and before landing:
  ```
  gh workflow run os-matrix.yml --ref claude/coord-trainO-union -f goos=windows -f stage=release-smoke
  gh run list --workflow os-matrix.yml --branch claude/coord-trainO-union --limit 1     # the run id; then: gh run watch <id>
  ```
  EXPECT (C1's proof runs 37144621021 / 37155599211 at their tips): the pack job green (344 packages, 0 failed); A, B, C
  PASS on all four RIDs (win-x64, linux-x64, osx-x64, osx-arm64); **D (the README walkthrough) GATING and PASS on all
  four** (it needs c1-darwin-xsys-libc in the tree: an order dependency). A red holds the landing; the release itself
  (1.24.13.4) is AFTER O and is not this step.
- **MS24 (review round 1, F1-2). p2's `vendor/golang.org/x/sys/cpu/.editorconfig` is STALE at the union: a deletion ROW,
  COORD's ruling (Q21), BEFORE the map.** INFERRED from source, no document had it: p2 commits that file with ONE section,
  `[/darwin/syscall_darwin_x86_gc.cs]` CS0649, for the two `libc_*_trampoline_addr` fields; c1-darwin-xsys-libc gives
  both an initializer through a new branch of `visitValueSpec.go` (its line 495) that sits BEFORE the chain's final else,
  where p2 calls `recordUnassignedFieldCandidate`. So the union converter derives no CS0649 fact for that file on darwin,
  the package's model comes out empty, and `writeWarningEntries` deletes the file ("an empty model deletes the
  converter's file"). Neither seat could see it (p2 measured at a merge of 8f46a9adae; xsys-libc was cut on 59ee without
  p2). Without a remedy the union keeps the file, WE arm B ("an entry for a file under `<goos>/` must warn on THAT
  flavour") FAILS on darwin (`STALE core/vendor/golang.org/x/sys/cpu [/darwin/syscall_darwin_x86_gc.cs] CS0649`), and
  section 5's WE EXPECT is false. The fixup cannot carry a deletion (step 7 dies on any tracked deletion, floor 8;
  `tO-regen-apply.py` only copies). Review round 1 makes it VISIBLE (emitdrift.py's `DELETED` class: leg E reads it
  union-attributable, step 4 REFUSES it by name, the reading line is `EDITORCONFIG DELETED`) and proposes the remedy:
  1. **Confirm** after N's battery (one conversion, one box, floor 1): in a seeded scratch root at a LOCAL merge of
     9e68c4a176 + 18264b3359 (git archive of the merge's `src/core` + `src/go2cs`), convert x/sys/cpu for darwin alone
     (`-platforms darwin/amd64`, the output root as the second positional, floor 3) and read whether
     `src/core/vendor/golang.org/x/sys/cpu/.editorconfig` is gone. Gone = the prediction holds.
  2. **The row** (RECOMMENDED): COORD cuts `claude/coord-o-xsys-cpu-editorconfig` on p2's tip 18264b3359, ONE signed
     commit `git rm src/core/vendor/golang.org/x/sys/cpu/.editorconfig`, pushes, reads back, announces; the row sits
     directly below p2 (the SLOT in the draft), its notes declaring its stack on p2-converter-warning-clears (the token)
     and its order below c1-darwin-xsys-libc (the `after` token, an earlier row). p2's footprint note keeps the path (the
     union then emits nothing there: no drift). Alternatives: P2 appends the same commit to its own ref (a tip move:
     TIP_MOVED_OK or a re-seat), or a ruled follow-up on p2's seated sha (`tO-follow.txt`).
  3. Then WE's EXPECT holds (`PASS`, stale 0), leg E reads 0, and the fixup's step 4 lists nothing for that path. The
     durable path beyond this (the fixup applying a ruled converter deletion itself) would carve an exception to floor 8:
     not drafted.
- **MS25 (review round 1, F1-3). value_impl.cs: the four writers' union reading (COORD's ruling, Q23).** B6, B7, K and
  names edit hand-owned `src/core/reflect/value_impl.cs` and NO row was cut on all the earlier ones (B7 on 8f46 without
  B6, ruled 11:08; K on B7 without B6; names on 8f46 without any; names' R5 re-cut on 59ee/BASE brings none in). Measured
  (review round 1, `git merge-file`, the merge base of each step = the row vs the union so far): 59ee -> B6 -> B7 -> K ->
  names reads 0 conflicts, 4154 -> 4187 -> 4198 -> 4210 -> 4216 lines, the +62 = each seat's own net (33 + 11 + 12 + 6):
  nothing duplicated or dropped; the hunks are disjoint (B6 at 44; B7 167/1518/1808; K 1484, 1784-1815; names 1226; N
  330/830). Two SEMANTIC pairs no gate ran together: **B6 x K** (Value equality as datum identity vs nil and typed-nil map
  keys) and **B6 x names** (func-kind Value equality, delegate target + method, vs the method-value pointer token,
  `delegateMethodToken`). Proposed ruling (a): NO re-cut; read the merged value_impl.cs WHOLE at the union (merge-hazards:
  two representations of one fact), and gate on: ReflectValueMapKeyIdentity, ReflectTypedNilStore, ReflectNilMapKey and
  MethodValueFuncNames (leg 5, each isolated); the reflect, fmt and text/template S rows and the i9's encoding/gob,
  encoding/xml, encoding/json rows; mapstructure 128/128 and yaml.v3 re-read at the union by the atlas lane (B6 alone read
  mapstructure 127/128 on the 10:07 ledger line; B7 read 128/128 at its full local merge; C2's yaml.v3 read 44/46 at a
  local merge with B7 + K, the 2 = the untyped-constant wrap class, not reflect). Ruling (b): re-cuts (names on the three,
  K on B6 + B7), which the measurement does not call for.

### 4b. A fixup-N after the push: what COORD re-reads

N's table holds; O adds three rows and widens one:

| The fixup-N touches | Re-read on the i7 | Lanes |
|---|---|---|
| `src/go2cs/**` non-test Go (the converter) | a fresh battery | i9 and linux: again; MS13 patches STALE |
| `src/core/golib/**`, `src/gen/**` | 2b, GT x4, GN, CT, leg 5, H7 x3, WE, MOD, PUB, then every S / T row | i9 and linux: again |
| `src/core/**` corpus `.cs` or `.editorconfig` outside golib | E, H7 x3, WE (O), the S or T row of that package and of the canaries | the lane that sweeps that row |
| `src/core/**/*_test.cs`, `*.tests.csproj` | that row's S or T leg; TE | the lane that sweeps that row |
| `src/tests/Behavioral/**` goldens | leg 4 (CNR), `run-behavioral.ps1 --filter <project>` per project; PUB if one of its projects | P1's LB, and LX when linux-only |
| a registration file | precheck, SI, leg C | none |
| `src/go2cs/stdlib-metadata.txt` | CB | none |
| `src/_roster.ps1`, `docs/ValidatedTestPackages.md` | G1 / G2, ST51 / ST7, NV51 / NV7, precheck (ROSTER x3 incl. DARWIN) | a moved row on every box |
| `src/go2cs/csproj-template.xml`, `src/go2cs/profiles/**` | PUB, leg 4, E | P1's LX, LB |
| `src/tools/**` | NG*, CC | none |
| `.claude/coord-scripts/coord-identifier-census.sh` (O) | IDC | none |
| `.github/workflows/os-matrix.yml`, `src/tests/PackageTests/**`, `src/push-nuget.ps1` (O) | NV51 / NV7 | **MS23 again** (the release-smoke CI run) |
| other `docs/**` | leg C | none |

Always: `python -B tO-helpers.py precheck <tO> $BASE <seats-effective.txt> head` at the new head.

## 5. Legs, the rows they gate, and expected readings

Walls are TRAIN N's (`coord-scratch/tN/bat1/tN-logs/SUMMARY.txt`) or M's where N's had not run, as a size. "derived"
means PRE-D or the leg computes it and stamps it. The PRE-D predictions are for the 35-row draft AFTER MS21's re-cuts (MS24's row, if ruled, adds no project, test or registration).

| Leg | Gates | Expect | N wall / cap |
|---|---|---|---|
| PRE / PRE-D / PRE-1 / 1b / 1c | the union's shape; MASTER on the first-parent line (O1); the derived lists; every seat | shape OK; **PRE-D CNR `EXPECT N=819`** (801 + 18 measurable added, none platform-exclusive) with its control (801 at BASE from N_SUMS; the fallback literal 801 is stamped when used); behavioral: 18 added, slnx agrees; execution configs keep=[] drop=[]; precheck rc 0 (MS9b.1); outparity no gap; 0 module-path hosts, GoDefaultGodebug in the same 3 files | seconds |
| PRE-2 / 3 | coverage, canaries | 225 = 93 + 132; five canaries derived (N read crypto/cipher, runtime, crypto/tls, net/http, go/types); X_ROWS encoding/json joins the S list | seconds |
| C | every converter row (20), repoguard; hashset's module requirement (MS19) | ok throughout | 488 s / 40 m |
| CC | regression | rc 0, `ok`, no FAIL | 3 s / 120 s |
| CB | every test the union ADDS under `src/go2cs` (derived; about 29 named by the rows as cut) + six base tree guards | N/N `--- PASS`, 0 SKIP/FAIL | 26 s / 30 m |
| FX, FXc1, FXc2 | regression; the guard's controls | `stale 0`; tracked >= 1291 (N's stamp) and == current; both controls fire | about 70 s |
| E (+ bisect) | every footprint IN the tree (the four rows) | plants OK, 6 x rc 0, union-attributable **0**, standing drift stamped (N's log/syslog csproj class: FIXUP STOP 1), csproj explained (r2r-restore's are seat edits), HANDOWN 0, RUNTIME-MAP 0; `.editorconfig` a READING, and `editorconfig-deleted=0` (review round 1: a committed one a conversion DELETED is union-attributable; with MS24's row in the tree none is predicted); above 0: the two E_BISECT_KNOWN arms run (or E_BISECT_SEATS) | M: 2797 s |
| G1 / G2 | the two C1 banks (the 5.1 reading they owe), the roster | pass x2; **0** rows with an execution config; check count REPORTED (it moves with 22 darwin annotations; N read 1994) | 16 s |
| SY, **IDC** (O7), SI / SIc | symbol sync; coord-census-dotted-handle; the projects O adds | clean; IDC rc 0 `SELF-TEST PASSED` fail=0 pass >= 217; SI clean, SIc six cycles | seconds |
| ST51 / ST7, NV51 / NV7 / NVR | regression; NV = c1-macos-flavors' and c1-release-smoke-all-os' push-nuget.ps1 | 0 violations and checks >= 62, x2; pre-flight clean x2; NVR refused by name | 15 s |
| NGa / NGb / NGF-j0 | regression (no O row touches src/tools) | identity `ran 43, failed 0` x2; **selfdescription `ran 16, failed 0` under BOTH editions** (fixup-2 in BASE; N read 16/4 under 5.1); 0 parse errors x2; J0 nugetgo in all four reads | seconds |
| 2b | the golib and gen rows (six golib; four gen: F, i9-generator-skip-records, B3, B6, each measured on 8f46's generator, before N's template changes) | rc 0, errors 0, gen-load 0 (GO2CS0003, an ERROR since i9-generator-skip-records, would read here) | M: 1589 s |
| CT | regression | Failed 0, Skipped 0, Total >= 24 | 26 s / 10 m |
| GN | B3 (KindNamedUnderlyingTests), F (BadImplementRecordTests), i9-generator-skip-records (GeneratorSkipRecordTests, TypeGeneratorSkipTests; 74/74 at its seat) | Failed 0, Total >= the derived floor | 11 s / 20 m |
| TR | regression (testing host) | Failed 0, Skipped 0, Total >= the derived floor (>= 26) | 28 s / 20 m |
| GT Debug + Release x3 | every golib row; UntypedIntFloatOperandTests (added), CgoDynamicImportResolutionTests (changed): derived | 0 failed, 0 NOT FOUND | about 220 s each / 45 m |
| 4 | every golden under the union converter | NO REGRESSION; **N == the derived CNR_EXPECT_N** (819 predicted); CNR's skip list == the derived one (N's seven) | N: about 15 min |
| 5 + B: | the 18 projects O adds (each isolated), the goldens the fixup re-baselined, the literal guards; the four value_impl.cs writers B6 x B7 x K x names read together for the first time (MS25: ReflectValueMapKeyIdentity, ReflectTypedNilStore, ReflectNilMapKey, MethodValueFuncNames) | all pass; the main-alone pair exit 2 | N: about 1 h 45 m incl. isolated |
| **PUB** (O4, O5) | g-publish-keep's gate; r2r-restore's single-file R2R arm | rc 0 and `published-output gate: 6 of 6 published programs match go, none hung` (n >= 6); SDK 10.0.*; pwsh started under the pinned-go PATH | N: rc 150 in 2 s (instrument) / 30 m |
| H7 x3 | linux, darwin, all (xsys-libc's darwin file; the regenerated corpus) | CS=0 x3 | about 750 s each |
| **WE / WEc** (O8) | p2's per-file warning entries vs the compiler, three flavours; its planted control | WE rc 0, final `PASS` (stale 0, missing 0; UNMEASURED named) **only with MS24's deletion row in the tree**; without it PREDICTED FAIL on darwin: `STALE core/vendor/golang.org/x/sys/cpu [/darwin/syscall_darwin_x86_gc.cs] CS0649: under darwin/ but does not warn on darwin` (review round 1, inferred); WEc rc 0 `CONTROL CAUGHT`; plant gone | INFERRED about 1 h / 150 m + 45 m |
| MOD | the -tests -recurse driver (regression; c1-tests-corpus-arch, B4, J edit -tests conversion); XS, XM | N's verdicts: singleflight 12, tlog 17, semaphore KNOWN; F4 0; MOD-orphans 0 | N: about 28 min / 4 h |
| PB | regression | rc 0, cmp at its banked count, binlog SEEN then absent | about 3 min |
| S | every row at ITS roster config (none annotated); the canaries; X_ROWS encoding/json (two footprint rows) | each PASS at banked counts with a fresh record; crypto/tls reads the BoGo host-limit disclosure as at N | N: about 2 h 50 m / 4 h per row |
| NR x4 | regression | 15 each, 0 deadlock lines | about 6 min |
| TE / HS / UF | O's test-source moves vs N's patch; host censuses | TE a READING (G-FRAME hunks now = sources N's refresh missed); HS 0 / the 3; UF 0 | seconds |
| T:runtime/pprof | regression; frame names (-fm, main.main) | **145 + 7**, BANK-ELIGIBLE, roster match; TestMemoryProfiler by name | M: 618 s / 30 m |
| TBS-W, T:runtime | regression; p2's runtime1.cs and pragma, fm-record's runtime position maps | TBS-W rc 0; **10819 + 71**, roster match; `/panic` AND `/trap` DISCLOSED | M: 263 s + 6130 s / 150 m |
| TE-T / HS-T / UF-T, TE-i9, TL-WALL | as N; baselines N's | readings | seconds |
| i9 shard | 132 rows | 132/132; UF 0; the MS13 patch posted; exit 0 | N: 58 min |
| linux P1 | golib rows, rows, LX, LB, LM | exit 0, or 4 with the known movers alone: os/exec (86 + 2) and LX's predicted `NativeFieldPortAlias.csproj` +2/-0; LB Target pass on P1's three linux-only goldens | |
| linux P2 | runtime, pprof, TBS x10 | 10810 + 73; 147 + 7; 10/10 disclosed; exit 0 | N: 15 legs, 0 movers |
| **release-smoke (CI)** | c1-macos-flavors, c1-release-smoke-all-os, c1-darwin-xsys-libc | MS23: pack green; A-D PASS on all four RIDs | CI |

NONZERO LEGS expected by design: FXc1, FXc2, SIc, NVR. **Nothing else** (WEc is expected rc 0). Findings: N's list,
plus: IDC off its verdict or floor; WE not PASS; WEc not CAUGHT or its plant left; PUB off its verdict or below 6.

## 6. Deliberately NOT in the battery

| Not carried | Why |
|---|---|
| The release-smoke stage (pack, four RIDs, walkthrough D) | a CI run by construction (macOS runners, the per-flavour pack): MS23, COORD's manual step before landing |
| M's G GOTRACEBACK=system control, L's HOP and SPB, the C1 token probe, L's TE signatures | as at N |
| The i9's uuid pack (NGE) | needs a COORD-written GO |
| PUB's control arm (the single-file hang at 2bb2e5f825's template) | documented, not re-run (MS15) |
| crypto/tls on the i9 | the standing ban |
| The MS13 refresh | a bank step after the battery, never a leg |
| A linux log/slog reading | retired at N (N's Q3 recommendation to keep a plain reading was never ruled) |
| Any darwin RUN | a mac CI dispatch (MS10) |

## 7. Open questions: OWED COORD RULINGS (O's; N's are `trainN/tN-README.md` section 7)

1. **c1-release-smoke-all-os 5cda875206** (the remote tip, announced + pushed 23:30Z at COORD's 18:28 ask; COORD 19:56
   "tip MOVES") in place of the accepted 13d0f7eb22: the table seats the REMOTE TIP; confirm the acceptance. The row
   stays after c1-darwin-xsys-libc (its `after` note is verified by the table).
2. **BOARD (R2)**: the MS21 precedent (A2 at b967f9fa7f as a NEW ref, B2 re-cut without its row, ONE BOARD docs row on
   BASE), or the ruled append-append pre-resolution (all four writers are pure appends: PM R2). Who names A2's new ref?
3. **G's pair (R5)**: re-cut on 59ee or BASE, and the single-parent fm-record route (encoding/json/package_info.cs left to
   the fixup's regeneration) vs the pre-map's two-parent route (needs O3, unrun).
4. **encoding/json/package_info.cs order**: p2 first (the draft) and fm-record regenerated; confirm.
5. **B2 x B5 projitems (R3)**: B2's re-cut stacked on B5 ALONE (single parent), keep-both.
6. **docs/README.md (R4)**: C2's single-parent re-cut on r-docs-refresh's NEW tip cdb9eb8a23 with the line-314 sentence
   KEPT in the re-cut, placed non-adjacent to p2's line 315 (review round 1: measured clean; the derive's 'later docs row
   cut on the O pre-union' had no slot and cannot pass the table), or the two-parent route under O3. Tell C2 the base
   before it cuts.
7. **p1-hashset-module (R6)**: P1 re-cuts after N lands, stacked on c1-darwin-xsys-libc; confirm P1 owns it (P1 is
   unblocked: 19:59 post).
8. **P1's targets ref `claude/p1-linux-only-goldens-targets` d745548f77** (NEW since the pre-map; announced 19:59, read
   back 20:00; 3 files, each `main.cs.target` the same blob as its `main.cs`): accept it as an O row (the draft seats it).
9. **r-jwt-promoted-iface-repros**: still PENDING (Go-only as cut; PromotedIfaceTestEmbed has no main): a `-tests`
   fixture instead, or drop it (the i9's four behavioral modules are in BASE)?
10. **p2-converter-warning-clears**, cut on e200: accept the union CNR, leg E, WE / WEc and the S rows of its footprint as
    its gate, with `.editorconfig` ordinary emission (O9)?
11. **B1's os_windows.cs**: admitted by the composed REGEN_ALLOW (the draft: the row's note carries a CORPUS FOOTPRINT), or
    R adds it on a new ref?
12. **CSPROJ_TEMPLATE=accept** for the 18 (D2's reader classifies them; N ruled accept for its 16).
13. **The trio** (r-atlas-batch-repros + A2 + A1) boards as a set with A3; keep d834821da0 as an explicit row (the draft
    does).
14. **Docs-only items**: r-design-named-func-type-identity (a slot, unruled), c2-sibling-pkgname-design (after N), G's
    c2e7cef853 BOARD section (in the BOARD docs row: COORD cuts it?). Review round 1: B2's parked BOARD row cites
    `DESIGN-named-func-type-identity.md`, which only b061154615 carries (59ee0d21bf does not; a code span, so site-check
    cannot see it): seat the design note WITH the BOARD docs row (RECOMMENDED; COORD ruled option A from it at 06:59), or
    reword B2's row.
15. **g-untyped-float-operators**: COORD ruled at 20:37 ('stack-on A2 0c4397a069'; O only if pushed before O freezes at
    N's landing, else P); no ref at 21:24. Review round 1 recommends AMENDING the base to A2's BOARD-free new ref
    (b967f9fa7f under its new name), never 0c4397a069 (A2's BOARD-only tip: it brings back the BOARD conflict with N's
    745e0969c0 and a second copy of A2's 30-line section). Tell G, and C2 through it (C2's yaml.v3 seat, 21:02, stacks on
    G's). If it lands in O: a slot directly below A2; the BOARD docs row's A2 section drops 'open, routed to TRAIN O'.
16. **Gates by seat**: c2-gen-kind-name-collision's are the battery's C, GN, 2b, 4 and 5 (golib-gen.md); the C1 banks'
    Windows PowerShell 5.1 roster reading is G2 + fixup PRERES. Confirm both.
17. **coord-census-dotted-handle 2b6213d821** (COORD's own row, ledger 20:08 'a TRAIN O row'): seated second, leg IDC its
    gate. Confirm (the inputs had listed the census admit as TRAIN P).
18. **NEW (O3)**: accept precheck's MULTI-BASE synthesis for a two-parent re-cut, or require single-parent re-cuts only
    (the recommendation, since O3 is unrun)? Either way the table NOTEs such a row.
19. **NEW (O5, an N finding)**: N's battery PUB leg read `rc=150 :: NO VERDICT LINE` (FINDING, 15:49, bat1): pwsh could not
    start with dotnet10 first on PATH (reproduced 20:12). The gate was never measured on N's union. Re-run it by hand on N's
    union before N lands (the O5 invocation), or accept G's seat-level 6/6 as N's reading?
20. **NEW (MS22)**: `EDITORCONFIG_NEW` default stop (the draft), or accept by ruling ahead of the fixup?
21. **NEW (review round 1, MS24)**: p2's `vendor/golang.org/x/sys/cpu/.editorconfig` is stale at the union (inferred:
    xsys-libc initializes the fields its one CS0649 section covers; the union converter deletes the file on darwin; WE
    arm B FAILS on darwin otherwise). Rule the remedy: COORD's deletion row cut on p2's tip, directly below p2
    (RECOMMENDED), P2's tip move, or a ruled follow-up; and the confirming one-package darwin conversion after N's battery.
22. **NEW (review round 1, F1-1)**: r-docs-refresh **cdb9eb8a23** (COORD's 21:02 line, in the 15:42 form that seated
    84fcc6373d): the table seats it; confirm, and FREEZE r-docs-refresh from C2's R4 cut on.
23. **NEW (review round 1, MS25)**: value_impl.cs: ruling (a) no re-cut, the merged file read whole at the union and the
    named union gates (B6 x K, B6 x names), or (b) re-cuts.
24. **NEW (review round 1)**: **i9-generator-skip-records fbacba8faf** (O ACCEPTED 21:11, after the review's inputs):
    seated directly below F with its stack on F declared; its generator claims (generated trees 0 diff, GenTests 74/74)
    were read on 8f46's generator, before N's template changes: the battery's 2b, GN, leg 5 and H7 are its union gates.
    Confirm the row and that no other gate is owed.

## 8. What was checked, and what was not

- `bash -n`: every `.sh` of trainO passes; `compile()`: `tO-helpers.py`, `tO-regen-apply.py`, `emitdrift.py` pass (no
  `__pycache__` written). A grep for an apostrophe inside `${VAR:?...}` (a quote bash -n accepts when a later quote closes
  it) reads none; the derive met and fixed two such lines (tO-CHANGES.md section 4).
- **The table's tokens** (a grep simulating the assembler's extraction over `tO-seats-draft.txt`): six `stack-on` tokens,
  each naming an EARLIER row that is its ancestor (git, 20:17, over every row pair: exactly the six declared descents, no
  undeclared one); one `after` token (c1-darwin-xsys-libc, earlier); four `CORPUS FOOTPRINT` groups composing 32 paths.
  All 34 remote tips equal their seated shas (ls-remote 20:17).
- **PUB's environment (O5)**, two probes on this box (20:12, no build): pwsh with `go pin + dotnet10 + PATH` and
  DOTNET_ROOT unset: rc 150, 'You must install or update .NET', exactly N's log; with the pinned-go PATH: rc 0, 7.4.6; the O5
  invocation shape with a stub `check-published-output.ps1` (prints the dotnet it sees, exits 7): `dotnet10\dotnet.exe`, SDK
  10.0.400, rc 7 propagated.
- Read with git: the darwin banks change only the roster (linux text byte-identical, 22 rows); no O row touches
  Goroutine.cs, a proof page, src/tools, GoDefaultGodebug or a committed test host; the 18 new behavioral csproj are Exe with
  TrimMode 0; the template delta 8f46a9adae..4b30906aea is the TrimMode pair + the PackageId marker; r2r-restore reaches the
  six linux-only csproj (+1/-1) and not NativeFieldPortAlias's; the footprint files of p2 / fm-record / xsys-libc / B1 are
  generated (no line-anchored `[module: GoManualConversion]`); P1's three targets are the same blobs as their main.cs;
  `git log -S` derives the execrows control ref (g-slog-roster-tc0's roster commit 2d46eba8f0, whose parent annotates
  log/slog); A1's and A2's PtrToAnonStructPtr registration hunks share old start 1577 / 1580 in the three files.
- **NOT run:** every script for real; no precheck, csprojtemplate or credit arm on any union (none exists); WE, IDC, the
  release-smoke run; the pre-map at BASE.
- **Review round 1 (2026-10-03 21:24-21:55 Central; `tO-CHANGES.md` section 5).** ls-remote 21:24: all 35 rows' tips at
  their seated shas (r-docs-refresh at cdb9eb8a23). A read-only simulation of the assembler's table check at the stand-in
  base 59ee0d21bf (no fetch, no worktree): 35 rows, `rows-sha256=676e5ba3ec40`, 0 problems; seven `stack-on` tokens (the
  six + i9-generator-skip-records on F), each an earlier ancestor; one `after`; no landed row, no train commit carried, no
  multi-base row; 540 patch-ids (rows + N's commits over each row's base), none under two shas; the CORPUS FOOTPRINT
  composition still 32 paths (1 + 1 + 19 + 11). `git merge-file` chains (each step's base = the row vs the union so far):
  value_impl.cs 0 conflicts 4154 -> 4216; TypeGenerator.cs (i9-generator-skip-records, B3, B6) 0 conflicts 869 -> 905;
  ImplementGenerator.cs (N + F + i9-generator-skip-records) 0 conflicts; docs/README.md: cdb9eb8a23 then p2 clean, then
  69ccd65ec6 the premap's 2 hunks; C2's sentence as a separate paragraph below line 316 clean with p2. The BEHIND
  predicate measured on today's master (M's equivalent: e2008427b1 1 hit, 8f46a9adae 0). emitdrift.py's DELETED class and
  tO-regen-apply.py's refusal run on synthetic trees in the session scratchpad (deleted file listed; unchanged file not;
  refused by name; the allow-nothing control refuses it too). `bash -n` on every `.sh`, `compile()` on every `.py`
  (tO-premap.sh and tO-regcheck.py included), no `__pycache__`; every file LF (tO-regcheck.py converted).
