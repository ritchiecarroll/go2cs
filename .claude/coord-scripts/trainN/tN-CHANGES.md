# TRAIN N script set: every change against TRAIN M's

DERIVED 2026-10-03 from `trainM/` (the record, untouched) by a workflow agent. Item numbers N1 to N14 are the
derivation's task items; "derive" marks a change no item named, made because a literal or a premise of M did not hold
for N's 27-row draft list (each with the reading that showed it). Manual steps are MSn (`tN-README.md` section 4).
Nothing was run for real; the readings cited are git reads, the helper controls and the TABLE_ONLY dry run
(`tN-README.md` section 8).

## 1. Inventory

| tN file | from | kind of change |
|---|---|---|
| `tN-seats-draft.txt` | COORD (01:55) | none (COORD's table, as written) |
| `tN-assemble.sh`, `tN-conflict-map.sh` | `tM-` same names | rename, base, header, literal-site list |
| `tN-fixup.sh` | `tM-fixup.sh` | rename, base, header, empty G-seat literals, no golden prediction, `.editorconfig` die |
| `tN-battery.sh` | `tM-battery.sh` | rename, base, baselines, header + leg comments, PUB, CC, CNR derivation, NGF-j0, EXEC 0, T patch |
| `tN-helpers.py` | `tM-helpers.py` | EXEC_RULED / H3_TOKENS empty, H5 'no', three new readers, labels |
| `tN-emitcheck.sh`, `emitdrift.py` | same | `.editorconfig` class |
| `tN-modules-legs.sh` | same | base, landed host seats |
| `tN-linux-legs.sh` | same | base, log/slog retired, LX, UF carve-out, landed host seats |
| `tN-i9-shard.sh`, `tN-i9-te.sh` | same | base, EXEC expectation, refresh patch |
| `tN-regen-apply.py`, `tN-ng-parse.ps1`, `tN-follow.txt`, `controls/` | same | names, headers, notes |
| `tN-i7-sweeps.txt`, `tN-i9-shard.txt` | same | none: byte-identical (sha256 40c6ec39...3724 and 057c78c1...cdf3d) |
| `tN-lane-brief-i9.md`, `tN-lane-brief-linux.md`, `COORD-LAUNCH-CHECKLIST.md`, `tN-README.md` | same | rewritten for N |
| `tN-CHANGES.md` | new | this table (N14); section 4 = review round 1 |
| `__pycache__/` | the derive's py_compile | DELETED in review round 1 (finding A7) |
| not copied: `COORD-RULINGS-r2.md`, `COORD-RULINGS-r3.md`, `tM-CHANGES.md`, `tM-seats-draft.txt` | | records of M, cited in place as `trainM/...` |

## 2. Changes

| Item | File | Change | Why |
|---|---|---|---|
| N1 | every script, `tN-follow.txt`, `controls/README.txt` | `tM-` -> `tN-`, `TRAIN M` -> `TRAIN N`, `trainM` -> `trainN`; `coord-scratch/tM` and `$X/tM` -> `tN`; worktree `/h/go2cs-tmp-coord/tN` (both slash forms, the `-p:go2csPath=H:/go2cs-tmp-coord/tN/` of leg TR included); emission scratch `tNemit*`; `refs/coord/tN-map/...`; union branch `claude/coord-trainN-union`; lock `coord-scratch/tN/.battery.lock`; `TM_LOCK_HELD` / `TM_EMIT_SCRATCH` -> `TN_*`; the LIVE gate's worktree fragment (`live`: `'tN'`) and timeout_stop's pattern | the task's renames; one mechanical pass (`coord-scratch/tN/derive/rename_tn.py`), then every survivor read by hand |
| N1 | every script | provenance pointers into M's record PROTECTED before the rename and restored as `trainM/tM-CHANGES.md section 9/10/11`, `trainM/COORD-RULINGS-r2/r3.md`, `coord-scratch/tM/wf/notes/...` | a blind rename would have pointed them at sections tN-CHANGES.md does not have |
| N1 | `tN-README.md`, scripts | `tM-README.md Mn` -> `tN-README.md MSn` (MSn carries Mn) | M's step names were referenced from the scripts |
| N1 | `tN-assemble.sh`, `tN-conflict-map.sh` | `BASE` = `8f46a9adaef35c804d6eba62056e9844608301f3` (TRAIN M's landed master) | the task's base |
| N1 | `tN-fixup.sh`, `tN-modules-legs.sh`, `tN-linux-legs.sh`, `tN-i9-shard.sh` | `BASE=8f46a9adae` | same |
| N1 | `tN-battery.sh` | `MASTER` default `8f46a9adae`; the PRE NOTE compares with `8f46a9adae` ("TRAIN M's landed master") | the precheck's master arm reads the base |
| N1 | `tN-battery.sh` | `L_SUMS`, `L_REWRITES`, `L_REWRITES_T` (TRAIN L run2) -> `M_SUMS`, `M_REWRITES`, `M_REWRITES_T` = `coord-scratch/tM/run3/tM-logs/{SUMMARY.txt,S-rewrites-U0.patch,T-rewrites-U0.patch}` | TL-WALL, the FX floor, the TE / TE-T baselines and the T legs' HOSTWALL compare with the previous train's record |
| N1 | `tN-battery.sh` | FX floor fallback literal 1281 -> 1291 (M's run3 stamp, read from `M_SUMS` first) | the measured minimum of the new baseline |
| N1 | `tN-helpers.py` | `wallcmp` prints `N=` / `M(min)=`; `hostwall` prints `M=` for the baseline record | labels of this run vs the previous train |
| N1 | `tN-modules-legs.sh`, `tN-linux-legs.sh` | (derive) H1/H2 host seats: when the seat list does not hold them, read from the BASE's first-parent merges by subject (`lsub`: `Merge claude/p2-*test-list*`, `*event-line*`; a92237f025 / 89fded79dc); the control converters' H2 patch is the LANDED seat's diff against the merge-base of its merge's parents (`H2MB`: +31 / -4 against 02a0b44467, read) | both host seats LANDED with M: read from the list alone they read ABSENT, singleflight would be NOT RUN, tlog KNOWN, and MR4c / MR6c would build without the stream reader (M's round-3 HIGH finding, again); merge-base with the base would be the seat itself, an empty diff |
| N2 | `tN-helpers.py` | NEW `cnrexpect <repo> [<ref>] [--base <ref>] [--goos] [--goarch]`: check-no-regression's own predicate over the tree (every behavioral directory holding a `*.go`, bin/obj excluded, minus line-anchored `[GoPlatformExclusive]` / `[GoArchExclusive]` this host cannot measure; one git grep) | CNR's N is DERIVED, never typed (the task); control: at 8f46a9adae it reads 791 / 6 / 785 and +7, TRAIN M's run3 CNR exactly |
| N2 | `tN-battery.sh` PRE-D (3b) | `CNR_EXPECT_N` defaults to cnrexpect at HEAD; cnrexpect at `$MASTER` must equal M's 785 read from `M_SUMS` (the reader's control, a finding otherwise); reconciliation = base N + measurable added - measurable removed, a finding when it differs; `NPOST` (the posts' 17 package directories, NativeFieldPortAlias included: the task's list lacked it, git shows it) stamped as a NOTE | "a reconciliation, not the expectation" |
| N2 | `tN-battery.sh` leg 4 | EXPECT = the derived N (stamped with its source); CNR's OWN skip list (its SKIPPED block) must equal the derived one (a finding otherwise); an N dir in the skip list that the tree declares exclusive is stated, not a finding | (derive) c2-native-array-view's NativeFieldPortAlias carries `[GoPlatformExclusive("linux")]` (read at c27cf4198a): M's "no new dir in the skip list" rule would have raised a false finding |
| N2 | `tN-battery.sh` PRE-D (3) continued | the go2cs.slnx agreement compares the added projects MINUS the platform-exclusive ones | (derive) linux-only projects are not registered in go2cs.slnx by design (the six at the base are not; NativeFieldPortAlias is not), a false dfind otherwise |
| N2 | `tN-battery.sh` leg 5 | a platform-exclusive added project leaves the isolated `B:` list, stamped | (derive) its `--filter` would match nothing on windows and exit 2 (a non-zero leg) |
| N2 | `tN-battery.sh` | `NDIRS` / `NSLNX` / `NGUARDS` (M's `M7DIRS`, `M7SLNX`, `MGUARDS`) | names that said M |
| N3 | `tN-helpers.py` | `EXEC_RULED = []`; precheck's message for an empty ruled set; `WORDNUM` gains `no` / `zero` | g-slog-roster-tc0 drops log/slog's release-tiered (0 execution rows); the roster's new sentence "No row opts back out" read as -1 and would have printed a false H5 NOTE (read with the arm's own regex at 582de36d1b) |
| N3 | `tN-battery.sh` | `EXEC_ROWS_EXPECT` defaults to 0 (from `execruled`); comments at X_ROWS, G1/G2, S (log/slog at TC0, `tiered=False`; net/http and internal/godebug no longer config rows) | same |
| N3 | `tN-fixup.sh` | the roster-guard comment and die message (0 rows, g-slog-roster-tc0) | same |
| N3 | `tN-linux-legs.sh` | the L4 `rowleg log/slog` line RETIRED (kept as a comment); the DERIVED stamp EXPECTs none at the union (log/slog at the base stays the reader's control: annotated at 8f46a9adae, read) | COORD 18:25: "retire linux-legs.sh:278-280 TC0 leg" (the repo template's lines; M's driver carried that leg as the tiered L4 row) |
| N3 | `tN-i9-shard.sh` | the PRE stamp EXPECTs log/slog at the base and NONE at the union; log/slog's named case expects `tiered=False` | same |
| N4 | `tN-battery.sh` | NEW leg PUB after leg 5's purge: `pwsh -NoProfile -File ./check-published-output.ps1` from `src/tests/Behavioral`, capped 30m, go pinned first and dotnet10 on PATH, DOTNET_ROOT unset (the pwsh legs' form); the SDK it resolves stamped (a finding off 10.0.*); EXPECT rc 0 and `<n> of <n> published programs match go, none hung` with n >= `PUB_MIN`=5; the documented control (rc 1, `InterfaceAssertionMapKey (single-file) HUNG: 10 of 10 ...` at 2bb2e5f825's template) stated, not re-run; restore + purge after | g-publish-keep 3ced87fd40's standing gate (script read at that sha: its verdict and NOT MEASURED / HUNG wording) |
| N5 | `tN-battery.sh` | NGF-j0: the default arm expects `nugetgo.github.com.google.uuid` and NOT the go. form; the `-p` arm kept; NEW `NGF-j0prop` (`-getProperty:J0UuidPackageId`) and the j0-consume.ps1 `PackageId` default read from the tree, both expecting nugetgo | c2-nugetgo-id-pattern a7873ee6c2 changes both defaults (read with git) |
| N6 | `tN-battery.sh` | NEW leg CC after leg C: `go test -count=1 ./... -timeout 120s` in `src/tools/comparison-classifier`; rc 0, an `ok` line, no FAIL | nothing else runs that module (go.mod: no requirements, go 1.23.12); `-count=1` added: leg C's no-cached-pass rule |
| N7 | `emitdrift.py` | a file named `.editorconfig` is kept out of written / drift; an optional seventh output lists any a conversion wrote (NEW / changed / same) | (premise CORRECTED in section 4, B4: COORD's notes rule the entries converter emission) P2's per-file entries are hand-written, never emission: a conversion that touched one would have read as union-attributable drift and been COPIED over the committed file by the fixup |
| N7 | `tN-emitcheck.sh` | passes the seventh output; `EDITORCONFIG derived` (the .editorconfig files M..U differ in), `EDITORCONFIG WRITTEN` lines and the count; the verdict line carries `editorconfig-written=<n>` | same; a known class, stated |
| N7 | `tN-battery.sh`, `tN-fixup.sh` | `editorconfig-written` above 0 is a finding (leg E) / a die (step 4) | same doctrine as HANDOWN |
| N8 | `COORD-LAUNCH-CHECKLIST.md` | the signature is read by KEY: `%GK` = 941694536F21BAFF, `%G?` G or U, on the fixup and on every union commit | %G? reads U for every COORD commit on this box (L:91) |
| N9a | `tN-linux-legs.sh` | NEW leg LX (P1, before LB): the windows-skipped, linux-native behavioral packages DERIVED with cnrexpect (checked against `LX_KNOWN`, the six run3's CNR named: MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName, WritevIovecSeam), transpiled in place as CNR does (`go2cs -go2cspath <src> <dir>`, deepest first); every tracked .cs / .csproj with a hunk and every untracked emission file is a MOVER by file; SetegidBroadcastSeam's alias-family residual KNOWN by content; diffs kept; restore and exact-path removal | windows CNR skips them; four went stale unseen. The task's guessed sixth (UnixAbstractSocketRoundTrip) is not one: the run3 line reads UnixAbstractAddrName and WritevIovecSeam. N adds a seventh, NativeFieldPortAlias |
| N9b | `tN-helpers.py` | NEW `uflinux <repo>`: untracked paths under src/core; an untracked `<pkg>/<name>_test.cs` whose `<name>_test.go` is a GOOS=linux test file and not a GOOS=windows one (go list TestGoFiles + XTestGoFiles) is carved out by name | the class IS real: P1 (18:58Z) read 17 under os/ and os/exec/ ("*_unix_test.cs and similar", the same at the control), P2 (00:45Z) 21 with runtime/pprof/rusage_test.cs |
| N9b | `tN-linux-legs.sh` | `ufread`: both UF sites (P1 after the rows; END for both lanes) carve that class out, stamp it, and count only the rest as a MOVER; `uf-<label>.txt` lists every path for exact-path cleanup | same |
| N10 | `tN-helpers.py` | NEW `testsrc-refresh <repo> --out <patch> <patch>...` and `--check-worktree`: the class (committed *_test.cs, package_test_info.cs, package_info_internal_test.cs under src/core, not golib, not hand-owned), everything else refused by category, shape refusals, conflicts between inputs, per-file hunk classes (G-FRAME, MAP, ALIAS, SLICE, OTHER) | MS13 as a POST-BATTERY BANK STEP; smoke-read on M's run3 patches (338 files, 0 refused, 0 conflicts, 27 excluded) |
| N10 | `tN-battery.sh` | the T legs also save `T-rewrites.patch` (`git diff --binary`, full context) | a full-context input for the refresh (S has restore_paths' `S-rewrites.patch`) |
| N10, N11 | `tN-i9-shard.sh`, `tN-lane-brief-i9.md` | the shard also writes `tracked-changes.patch` (with context) and stamps its lines and sha256; the brief asks for its path and sha256 in the post | the i9's 132 rows are half of the refresh |
| N10 | `tN-README.md` MS13, `COORD-LAUNCH-CHECKLIST.md` section 6 | the procedure: inputs at one head, filter, apply, `--check-worktree`, one signed commit before master's fast-forward | the design (not run) |
| N11 | `tN-lane-brief-i9.md` | the shard list carries over (132, sha unchanged); the crypto/tls and net/* ban stays; the refresh patch is posted | the task |
| N12 | `tN-README.md` section 4 | MS1 to MS13 carry M1 to M13 (MS3, MS6, MS12 DROPPED with the reason; MS2 now FIRES; MS13 is the refresh); NEW MS14 registration hazards (eight seats, keep every side's insert, COUNT / REG / BOTH; projitems adds + p2-n-cleanup's deletions; p2-n-cleanup LAST), MS15 PUB's single-file hang context, MS16 c2 stacks on r-module-license and merges with g-publish-keep (57 both-changed), MS17 row 23 after the i9's seat, MS18 the PENDING rows by the cutoff, MS19 the hashset module | the task |
| N13 | `tN-i7-sweeps.txt`, `tN-i9-shard.txt`, `tN-README.md` S row | carried byte-identical (no row moved between hosts); crypto/tls's expected reading stays the BoGo host-limit disclosure, 1340 = 4759 - 3419 (M's run3 line) | the task |
| N14 | `tN-CHANGES.md` | this file | the task |
| derive | `tN-helpers.py` | `H3_TOKENS = {}` | no N row edits Goroutine.cs (read: each row's merge-base..sha); M's entry would only have printed a NOTE |
| derive | `tN-helpers.py` | H4's comment: the arm FIRES at N (r-jwt-promoted-iface-repros' three Go-only directories); the precheck comment records N's roster prediction (225 / 225 rows, 223 / 223 linux annotations unmoved at 582de36d1b) | read with git and the arms' own regexes |
| derive | `tN-helpers.py` | `teattr`'s verdict: G-FRAME is the standing class until MS13; the baseline is TRAIN M's | the class did not go away at M |
| derive | `tN-battery.sh` | `E_BISECT_KNOWN=''` | M's two seats landed with M; a non-row name is a PRE-D finding |
| derive | `tN-fixup.sh` | `REGEN_G_SEAT=''`, `REGEN_G_PKGS=''`, both uses guarded for empty | M's g-godebug-pc-line is not a row; an empty pattern would have matched lines starting with `|` |
| derive | `tN-fixup.sh` | step 5's prediction is EMPTY (M's G census removed) and the scoring names every moved project; the class die says every N golden is expected to land there | no N golden is predicted; G's census predicts G's class only |
| derive | `tN-fixup.sh` | header rewritten (what N's fixup measures); PRERES's H4 die text; PROSEV default ("none is owed at N"); S1 messages (TRAIN M's master, N adds); the commit message's base sentence | N's facts |
| derive | `tN-battery.sh` | the header and LEG -> SEAT MAP rewritten for N's seats; leg comments (C, CB, FX, E, G1/G2, SI, ST, NV, NG, 2b, CT, GN, TR, GT, 4, 5, H7, MOD, S, NR, TE, TL-WALL) state which N seats each leg gates and N's expectation; M's guard provenance kept beside the code | the header is the leg map COORD reads |
| derive | `tN-battery.sh` NV | c2-s3b-nuget-map edits `src/push-nuget.ps1` itself: NV is that seat's i7 reading | read with git |
| derive | `tN-assemble.sh` | the literal-site list (LITS) is N's; the NEXT line says H4 fires at the draft list | N's literals |
| derive | `tN-follow.txt`, `controls/README.txt`, `tN-i9-te.sh`, `tN-regen-apply.py`, `tN-ng-parse.ps1` | headers and notes for N | names and facts |

## 3. Not mapped, and why

| M content | Status at N |
|---|---|
| `tM-README.md` M3, M6, M12 | DROPPED (MS3, MS6, MS12 say why) |
| M's `PROSE_PATCH` (`fix1/prose.patch`) | landed with M; the mechanism stays, no patch is owed |
| M's G-frame golden PREDICTION census in the fixup | removed (no N class to predict) |
| M's `REGEN_G_*` literals, `E_BISECT_KNOWN` names, `H3_TOKENS` entry, `EXEC_RULED` row | emptied (no N seat matches) |
| The repo template `.claude/coord-scripts/templates/linux-legs.sh` lines 278-280 and `measurement-discipline` SKILL.md :40 / :99 | outside `trainN/`: a doctrine-batch item (README section 7 q10) |
| `COORD-RULINGS-r2.md`, `COORD-RULINGS-r3.md` | records; cited in place |

## 4. Review round 1 (2026-10-03): the findings applied

A review of the derived set (lenses: assembly, battery, lanes) returned 31 findings. Each was MEASURED before it was
applied (`tN-README.md` section 8, "Review round 1"; evidence under `coord-scratch/tN/derive/review1/`). A finding whose
fix is a lane re-cut or a COORD ruling is NOT decided here: its planning is written down and the route is an owed
ruling (`tN-README.md` section 7, Q1, Q3, Q4, Q12 to Q17). `tN-seats-draft.txt` (COORD's, tracked) is not edited.
"F" numbers follow the review's order (A = assembly, B = battery, L = lanes).

| F | File | Change | Why |
|---|---|---|---|
| A1 | `tN-README.md` MS21 item 1, MS14, step 1; checklist section 0; `tN-assemble.sh` header | the BOARD conflict (row 9 x row 4) planned as a pre-map route; the BOARD named an append-only registration hazard | merge-tree, base e2008427b1: one marker set, G's block vs C2's, above the raw guard. The route (re-cut) is COORD's: Q13 |
| A2 | same, MS21 item 2 | the visitAssignStmt.go conflict (row 21 x row 19) | merge-tree, base 8f46a9adae: adjacent hunks (C2's condition, P1's `hashset.HashSet` line). Route: Q13 |
| A3 | same, MS21 item 3; MS14's sentence | the projitems conflict (row 27 x row 17); MS14 no longer says git merges an insert beside a delete "silently": it refuses it | merge-tree: `@@ -148,0 +149,2 @@` vs `@@ -149,2 +147,0 @@`, one marker set. Route: Q13 |
| A4 | same, MS21 item 4; MS19 (the scan command) | the clean-but-uncompilable merge (row 15 x row 21) planned; MS19 carries the unqualified-HashSet scan for every row | b9e2d5ffa1 lines 252-253 `HashSet[string]{}` in package main; base file 0 uses; a7a7a197dd deletes HashSet.go; scan over all rows: b9e2d5ffa1 alone. Route: Q13 |
| A5 | `tN-assemble.sh` header; README step 1 / step 3 / MS14; checklist section 1 | the stale "the pre-map reads every seat CLEAN" premise replaced by the review's prediction (rows 9, 21, 27); the map EXPECT reads "after the re-cuts" | `git for-each-ref refs/coord/tN-map` is empty: no N map ever ran |
| A6 | README header, MS9c, MS14, MS18, section 5; checklist section 0 | c2-toml-test-host recorded as ACCEPTED (its slot before p2-n-cleanup; COORD adds the row, Q17); MS9c's figures are `<n>`, `<n+1>`, `<n-1>` and a printed hash; the toml row is MS14's ninth registration seat, CNR +1, MS20's 15th csproj | L:172 (02:34); `git diff --stat 8f46a9adae 15838bfff5`: 19 files incl. go2cs.slnx, projitems, the four BehavioralTests files |
| A7 | `__pycache__/` (deleted); checklist section 1 | the derive's 148 KB `.pyc` removed; the force-add is preceded by `git add -f -n` (a dry-run list) and followed by `status --porcelain --ignored` | `.claude/` is excluded: plain `status --porcelain` lists nothing under trainN, so the old guard proved nothing; trainM tracks 0 `.pyc`. Python checks in this round use `compile()` (writes nothing) |
| A8 | `tN-assemble.sh` patch-id loop | a row that is an ancestor of the base is skipped (the table already refused it) | its merge-base is itself, so the loop read all of master since then (about 10 minutes for MS9c control 3); the planted run with a landed row took 227 s in all |
| A9 | `tN-assemble.sh` NEXT line, LITS | the NEXT line prints `$NSEATS`; LITS names `tN-fixup.sh REGEN_G_SEAT REGEN_G_PKGS (empty at N)` | the header says no seat count is written there; README MS1 lists that literal site |
| A10 | `tN-conflict-map.sh` comments | `8d7305053f` worded as M history; the 07:43 incident's ref restored to `refs/coord/tM-map/...` | the blanket rename made M's history read as N's |
| A11 | README MS9c control 1 | all three dup-control lines EXPECTed (tip none, same sha, undeclared stack) | re-run in this round: exactly those three plus row 23's, 4 problems, rc 2 |
| A12 | checklist section 0 | `row 23 (N:25)`; row 22's stale `(owes go2cs.slnx)` note listed as COORD's table edit (Q17) | the table check prints row 23; a9cc98c1e3 registers its four projects in go2cs.slnx (read with git) |
| A13 | `tN-assemble.sh` table check | an `after <ref>` note naming an EARLIER row prints `ordered after row k ... (declared; order verified)`; one naming a LATER row is a table problem; a hyphenated non-row token is a NOTE | the 'after' edit Q1 offers was read by no check. Planted run: both lines as designed |
| A14 | README section 7 Q1 (Q2 merged into it); MS2; checklist section 0 | carried as ONE owed ruling, with the reviewer's recommendation ((b) if R can push by the cutoff, else (c)); the `after` edit is never applied alone | coord-ruling |
| A15 | README section 7 Q14; `tN-assemble.sh` header | re-cuts vs a narrow ruled pre-resolution carried as an owed ruling; no machinery built | coord-ruling |
| B1 | `tN-helpers.py` precheck REG (new `reg_arm`); README MS9b.1, MS14, section 2 R1, section 5; checklist 2b; `tN-battery.sh` PRE-1 comment | keys a seat (or a fixup) removes on purpose are no longer `base-keys-lost`; a removed key still in the head with no re-add is `removed-key-back`; the ok line names `seat-removed=[...]`; a key a later stacked row removed is not `seat-keys-missing` | at M no seat removed a key; at N p1-hashset-module removes HashSet.go and p2-n-cleanup five guard tests: every correct union FAILED PRERES / step 6 / PRE-1 / the landing precheck. Control on real blobs: correct 0 / 0 (M's formula: 6 lost), a kept key reads back, a dropped base key reads lost |
| B2 | `tN-fixup.sh` step 5 (+ step 7, the commit message, the PRE stamp, `CSPROJ_TEMPLATE`); `tN-helpers.py` new `csprojtemplate`; README MS20, section 2 T1, env table; checklist section 3 | a csproj CNR moves is CLASSIFIED (the union template's delta, derived from the seats' own edits of existing behavioral csproj, in a csproj the union added) before any die; outside the class it dies as before; inside it STOPS unless `CSPROJ_TEMPLATE=accept`, which carries it (`tmpl-files.txt`, the explicit add list, `(5b)` in the message). The route (accept vs a follow-up seat) is COORD's: Q15 | 14 added csproj read TrimMode 0 at their own shas, the four libraries keep the go. PackageId; CNR rewrites csproj ("the transpile rewrites both"); `GOLDEN_CLASS=any` never reached the old die. Controls: positive per template seat, three negatives refuse |
| B3 | `tN-helpers.py` csprojdrift; `tN-battery.sh` leg E comment and header; README section 5 | a deleted csproj that a seat commit deleted is `seat-delete` (explained), listed as SEAT-EDIT/DELETE; `OTHER-deleted` otherwise | p2-n-cleanup deletes GenericTests.csproj: rc 1 before (UNEXPLAINED), rc 0 after; negative (no commit below U) rc 1 |
| B4 | `emitdrift.py`, `tN-emitcheck.sh`, `tN-fixup.sh` step 4 die, `tN-battery.sh` leg E | the wrong "hand-written, never emission" wording corrected (COORD's notes: converter emission, in P2's PENDING seat); the mechanism is unchanged and carried as an owed ruling, Q16 | L:83, :87, :92, :96; both branches of the reviewer's fix correct the wording; no `.editorconfig` under src/core at the base or in any row |
| B5 | `tN-battery.sh` PRE-D (3b) | the CNR control falls back to the literal 785 (TRAIN M's run3 line) when M's SUMMARY is unreadable, and stamps its source; it is never skipped | floor 13: an empty value passed the test silently |
| B6 | `tN-battery.sh` PRE-1 and SI comments, header leg map; `tN-emitcheck.sh` header; checklist section 5 | PRE-1 says no row carries an execution config; SI says 12 added, 11 in go2cs.slnx; the emitcheck header names EDCONF; one battery estimate (10 to 11 h) in both documents | stale comments |
| L1 | `tN-linux-legs.sh` LX and the LC stamp; brief LX row | LX snapshots the tracked state before its loop and requires the same state after its restore (not a clean tree); the carried-in changes are stamped by path; the LC stamp lists paths | the rows leave proof pages under docs/validation and LCf left one change at M (P1's M post): the whole-tree assert after LX would abort every healthy P1 run before LB / LM / END |
| L2 | README section 7 Q12 (premise corrected), MS18; brief LX row and END line | NativeFieldPortAlias.csproj +3/-1 PREDICTED (still a MOVER); the route carried as an owed ruling | the six base csproj were regenerated by g-publish-keep (3/1 each, read); NativeFieldPortAlias at c27cf4198a has the unconditioned ReadyToRun and no TrimMode |
| L3 | README section 7 Q3, section 6 | the reviewer's recommendation (keep a plain linux log/slog row reading) carried; the leg stays retired | coord-ruling |
| L4 | README section 7 Q4 | the reviewer's recommendation (a name-keyed TestExtraFiles exception) carried | coord-ruling |
| L5 | `tN-linux-legs.sh`, `tN-i9-shard.sh` | the abort text reads "the N fixup" | the grep looks for 'fixup: TRAIN N' |
| L6 | `tN-linux-legs.sh` GT comment | "TRAIN M run 1" restored in the dated 2026-10-02 note | blanket-rename residue |
| L7 | brief section 4 | the log/slog control sentence kept only as conditional on COORD restoring the row | the driver no longer reads log/slog |
| L8 | `tN-i9-shard.sh` (abort), `tN-linux-legs.sh` (MOVER), both briefs | the union's execution rows are compared with the RULED set read from its one site (`tN-helpers.py execruled`), never a typed "none" | the briefs asked lanes to stop on a non-empty list; the drivers only stamped it |
| L9 | `tN-modules-legs.sh` (before CM); both briefs; README MS19, Q11; checklist section 3 | the converter's DIRECT requirements, derived from the union's `src/go2cs/go.mod`, must be at their exact module-cache paths under GOPROXY=off, else exit 3 naming the absent paths and MS19; the briefs carry a warm step | read 2026-10-03: hashset v1.0.0 is absent from the i7's cache (x/mod and x/tools present) |
| L10 | (none) | DECLINED: the carved UF paths are not deleted at END | optional; the files are the on-disk evidence of the carve-out until COORD reads the post, `uf-END.txt` lists them, LEGS_CHECK_ONLY=1 names them and the brief states the exact-path cleanup |
