# TRAIN N cleanup scout: synthesis at base 8f46a9adaef35c804d6eba62056e9844608301f3

What the synthesizer checked again at the base:
- Every DROP byte count, read with `git ls-tree -l`.
- The commits that introduced the ruled H5 files, read with `git log --diff-filter=A`.
- References to the member-bill files. One `git grep` outside `docs/phase4` for `h5-c1-2-fixture|apply-h5-c1-2-member-bill|h5MemberBillGuard|runtime2.pre-bill` found only the script itself, `src/go2cs/h5MemberBillGuard_test.go` and `src/go2cs/go2cs-src.projitems:149`. The runbook has no hit.
- Files the ignore rules would hide. `.gitignore` is the same at the base and at HEAD.

Scratch files are in `/h/go2cs-tmp-coord/coord-scratch/n-cleanup-scout/` (`base-files.txt`, `ignored-tracked.txt`).

## 1. DROP

Rows below have a verifier verdict of drop, and no live reference remains once the listed updates are made.

| # | Path(s) | What it was | Introduced | References to update (file:line) | Risk | Bytes |
|---|---|---|---|---|---|---|
| D1 | **H5 scaffolding (already ruled, plus one missing piece).** `src/h5-c1-2-fixture/runtime2.pre-bill.cs` (51,508); `src/apply-h5-c1-2-member-bill.sh` (64,813); `src/go2cs/h5MemberBillGuard_test.go` (11,213); `src/apply-h5-c1-1-rederives.sh` (40,313); **`src/go2cs/h5RederivePatchGuard_test.go` (7,483): NOT on the ruled list and must join it** | Appliers and fixture for the H5 hand-own re-derives of the 1.23.12-era tree, plus their guards. Each applier refuses a non-pre-H5c tree. Guard headers read "THE GUARD FOR src/apply-h5-c1-1-rederives.sh." / "THE GUARD FOR src/apply-h5-c1-2-member-bill.sh." | fixture f75b6fc032 2026-09-16; member-bill script and guard 29fc8388ee 2026-09-13; c1-1 applier and its guard ded03d4695 2026-09-13 | `src/go2cs/go2cs-src.projitems:149` (`<None Include="$(MSBuildThisFileDirectory)h5MemberBillGuard_test.go" />`) and `:150` (the h5RederivePatchGuard line). Delete both lines in the same commit. Optional dated in-stage note by the runbook owner at `docs/GoCorpusMigration.md:1076,1086,1087,1103`. The procedure needs no change because :1087 reads `git show "$P:src/apply-h5-c1-1-rederives.sh"` from `<patch-ref>` = ded03d469, which master history still reaches. Records stay: PATCH-h5-c1-1-*, PATCH-h5-c1-2-*, RESUME-SESSIONS.md, probes/c1-finalizer-iteration-index/apply.py. | Low as one commit. Medium if split, because three tests fail: script gone but guard kept fails TestH5RederivePatchSelfTest (bash 127 at :56); guard gone but projitems line kept fails TestProjitemsHasNoDanglingEntries (projitemsIntegrity_test.go:147); projitems line gone but file kept fails TestProjitemsRegistersEveryGoSource (:102). `safePushBash` (safePushGuard_test.go:128) stays; safePushGuard_test.go:61 and tokenDoorCensusGuard_test.go:45 still use it. | 175,330 |
| D2 | `src/utilities/GetPackageDeps/GetPackageDeps.go` (1,810), `src/utilities/GetPackageDeps/GetPackageDeps.exe` (2,728,960) | Scratch `go list -json` import-map program (main commented "// Example usage") with its prebuilt Windows binary | ba6fef6c91 2025-03-08 "Restructured build locations"; gofmt 1a005ff648 2026-07-12; moved d3223d252e 2026-08-06 | None. A case-insensitive whole-tree grep for `GetPackageDeps` finds only `src/utilities/go.mod:1`. `src/utilities/go.mod` is Q1. **Do not sweep `src/utilities`**: ReplaceInFiles (set-version.ps1:25) and UpdateTestTargets (go2cs.slnx:1074) are live. | none | 2,730,770 |
| D3 | `src/go2cs/launch.json` | 0-byte VS Code stub. It contradicts `.gitignore:30-33` ("launch.json in particular is a personal debug scratchpad ... does not belong in the repo") | 40a73d6c03 2025-05-03 | None. It is not in projitems, not a `//go:embed` asset, and not a staleness input (converterStaleness.go:284-297 walks only *.go). | none | 0 |
| D4 | `src/go2cs.png` | Package icon copy at the src/ top level, same blob (e36698d6) as `src/core/go2cs.png` and `src/go2cs/go2cs.png` | e696055436 2018-05-21; restamped a3fb171709 2026-08-08 | None resolve to it. golib.csproj:68 points at `src/core/go2cs.png`, go2cs-gen at `..\..\core\go2cs.png`, and the converter embeds `src/go2cs/go2cs.png` (embeddedTemplates.go:36, projitems:434). | low | 17,015 |
| D5 | `src/tests/Behavioral/clear-package-info.bat` (211), `src/tests/Behavioral/clear-generated.bat` (206) | Old cleanup scripts. The first runs `FOR /R . ... del` over every package_info.cs, and 791 tracked goldens sit under Behavioral, so it is a live hazard. The second duplicates the Generated sweep in `src/clean-bin.ps1:102`. | 9faaf9782b 2025-02-06 | None (zero hits for either name; no runner globs *.bat) | none | 417 |
| D6 | `src/tests/Behavioral/TypeSwitch/build.cmd` | bflat build against `..\..\..\gocore\golib\bin\Debug\net8.0\golib.dll`. Both that layout and that TFM are retired. | 010b5e623c / 1752b7aa11 2024-12-21 | Record only: `docs/phase4/CENSUS-tfm-inventory.md:403`, which calls it dead and a "Candidate for a cleanup chip". At most append a dated block; never edit. | none | 301 |
| D7 | `src/tests/Behavioral/ExprSwitch/ExprSwitchExtComments.go.test` (2,706), `src/tests/Behavioral/ExprSwitch/ExprSwitchMicro.go.test` (786) | Scratch switch programs renamed so Go ignores them. Not goldens. | 5081afe682 2018-08-05; 4102b61491 2024-10-28 | None (zero hits; every walk is `GetFiles(dir, "*.go")`) | none | 3,492 |
| D8 | `src/tests/Behavioral/mod-init-all.ps1` | "Maintenance one-off: re-initialize the Go module in every behavioral test project." Running it now is destructive in three ways. It rewrites bare-module go.mod files to `go2cs/<Name>`, which breaks NamedSliceChildPkg and IoLike (harness-gates.md:485). It deletes 793 curated csproj files. It rewrites the `go 1.23` directive in 757 go.mod files. | 5635b64a35 2025-03-16; ported ff5731e4ab 2026-08-08 | Only `docs/PLAN-linux-operation.md:372, :429, :1121` (port-census rows). Leave them as history or add a dated note. The finder called this a question because of the r47b hardening. The verifier answered that the hardening was the port-every-script sweep, not evidence of use. | low | 2,789 |
| D9 | `src/tests/GenericTests/GenericTests.csproj` (1,113), `Program.cs` (4,223), `orderedSlice.cs` (1,664) | Iteration-1 probe of the go2draft type-list generics design, which Go never shipped. It is in no .slnx and no runner, yet it has been hand-maintained anyway (8e9e1808ad 2026-09-08; 272afeef93 2026-09-29). | 6d00cfe317 2021-01-14 | `src/migrate-tfm.ps1:155` (remove the entry, or the census warns "file missing"). `src/tests/GolibTests/Sha3ReinterpretVectorTests.cs:31-32` and `docs/ConversionStrategies-Reference/pointers.md:822`: reword "follows GenericTests" to cite GolibTests' own converted-package reference (GolibTests.csproj:178 core/sort). Records stay: BOARD:9185, CENSUS-tfm-inventory.md:184. | low | 7,000 (the verifier's 6,999 is one byte low; ls-tree sums to 7,000) |
| D10 | `src/go2cs/convCallExpr.go:76-82` (`callFunIsUniverseBuiltin` plus its doc comment), `src/go2cs/convSelectorExpr.go:901-906` (`selectorBaseIsPackage` plus its doc comment) | Converter helpers whose last callers were removed: 9533b808c6 2026-08-07 (issue #33) and 024036df7b 2026-09-01 (CS1955). The helpers they wrap (`identIsUniverseBuiltin`, `selectorBasePackageObj`) keep other callers. | 6bf3917710 2026-07-20; 6ae8151e95 2026-08-02 | `docs/ConversionStrategies-Reference/shadowing.md:651` is already stale. It names `selectorBaseIsPackage` as "consulted by `aliasResolvedSelector`", but the caller is now `selectorBasePackageObj` (convSelectorExpr.go:833). Rename it in the same commit. | none (no emission change) | 747 |

Total for D1–D10 is 2,937,861 bytes at the tip. History keeps every blob, so clone size does not shrink; checkouts and worktrees do.

## 2. RELOCATE

No relocation survived verification. The two proposals are below so nobody raises them again without the evidence.

| Path(s) | Proposed destination | Proposer's why | Verifier verdict and the live use behind it | Edit list if COORD rules to move anyway | Risk | Bytes |
|---|---|---|---|---|---|---|
| `src/safe-push.sh`, `src/seat-duplication-census.sh`, `src/fleet-patchid-census.sh` + `src/go2cs/{safePushGuard,seatDuplicationGuard,fleetPatchIdCensusGuard}_test.go` | `.claude/coord-scripts/` | These are coordination tools, not product code, yet they sit at the src/ top level. | **Keep in place.** `fleetPatchIdCensusGuard_test.go:34` records the converter test package as a deliberate home, because every lane runs `go test ./...`. Moving them takes the self-tests out of the only harness every lane runs. | Guard script paths at safePushGuard_test.go:48, seatDuplicationGuard_test.go:38, fleetPatchIdCensusGuard_test.go:43; projitems :136, :321, :322; `.claude/skills/mailbox/SKILL.md:61, :66`; `.claude/coord-scripts/lanes/g-fetchable-check.sh:27`; `src/run-h10-dispatch.ps1:575`. `safePushBash` must stay reachable from tokenDoorCensusGuard_test.go:45. | medium | 82,286 |
| `src/tests/ElemAliasProbe/` | beside `docs/phase4/probes/` | It is in no solution, runner or CI. | **Keep** (see section 4). Living code comments cite its arms as evidence. | Every citation in section 4 would need its path changed | medium | 50,429 |

## 3. QUESTIONS FOR COORD

- **Q1. `src/utilities/go.mod` (33 B; go.mod files are on the never-drop list).** Question: may a go.mod whose only module is the dropped GetPackageDeps leave with D2?
  - Contents: `module GetPackageDeps` / `go 1.23.2`.
  - There is no go.work, and no go.mod at src/ or the repo root.
  - migrate-gorelease's `grep -F` sweep (:924) cannot match `1.23.2`.
  - Both verifiers say it should leave with D2. Yes adds it to the git rm list for S-A.
- **Q2. `src/seat-duplication-census.sh` (20,882) + guard (6,137) + `projitems:322`, decided as a PAIR with `src/fleet-patchid-census.sh` (15,661) + guard (4,468) + `projitems:136`.** Question: will TRAIN N's assembly, or any standing procedure, actually invoke seat-duplication-census.sh?
  - Nothing calls either script except its own self-test, which runs in every converter go test.
  - `.claude/skills/merge-hazards/SKILL.md:78-90` describes the seat census without naming it.
  - `g-fetchable-check.sh:118` names only the branch `claude/g-fleet-patchid-census`.
  - If yes: wire it in and keep both. If no: drop all four files and both projitems lines in one commit, in S-B, because that seat already edits projitems. Then the sibling prose at `fleetPatchIdCensusGuard_test.go:20` and `src/fleet-patchid-census.sh:4` goes with them, and merge-hazards SKILL.md:78-90 would describe a missing tool.
- **Q3. `src/token-door-census.sh` + `src/go2cs/tokenDoorCensusGuard_test.go` + `projitems:423` (16,252).** Question: is the token-door population a defect class someone will drive down, or a finished census?
  - The population still exists: six live wrappers, recorded in CENSUS-token-door-live-wrappers.md §4.
  - The guard's member control names CreateProcess (tokenDoorCensusGuard_test.go:62-63). The next hand-own of CreateProcess turns every converter go test red until the guard is re-anchored. This already happened once: 384dc371fb.
  - If it is a standing instrument: keep it, and optionally move the guard out of the converter package. If finished: drop all three together.
- **Q4. `src/tools/comparison-classifier/` (326,194; 296,159 of it is the runtime-panic fixture pair).** The verifiers split two question, one keep; none said drop. Question: is a reader of `go2cs_test_comparison.json` still wanted?
  - `docs/phase4/r-evidence/m1-module-proof-pages/PREDICTION.md:14` (2026-09-29) calls it "their one reader".
  - `.claude/skills/gate-forensics/SKILL.md:2101` relies on its host-crash short-circuit (classify.go:215-226).
  - Its JSON tags match `src/go2cs/testConversion.go:7077-7085`.
  - Nothing runs `classify_test.go`, because it is a separate module.
  - Options: keep and wire its `go test` into a lane; keep and trim the fixture; or retire it, after which SKILL.md:2101 describes a missing tool.
- **Q5. `src/_roster.ps1:391-503` (`Get-HopSkeletonRows`, 6,185 B).** Question: does 59db1d508e's 2026-09-29 retention ("that parser stays as the reader of the dated census record") still hold?
  - No caller exists.
  - If dropped, also update `_roster.ps1:510` and `:526`, which uses it as the shape contract.
  - Control for a drop: `check-roster-format.ps1` green, plus a `-Hop` dry read through `Get-HopPopulationRows`.
- **Q6. `.github/workflows/os-matrix.yml:64-71` (the `dotnet` input's `9.0.x` option) and `:300-304` (excluded area).** Question: has the condition "kept for as long as 1.23.1.x anchors ship" lapsed?
  - The last 1.23.1.x tag, nuget-1.23.1.7 (b0c73c8b9a, 2026-08-23), has its own workflow, with no `dotnet` input and a hard-coded `9.0.x`.
  - On master, choosing 9.0.x can only produce NETSDK1045.
  - Edits if dropped: `:65`, `:69-71`, `:300-304`. Also see the side finding about `src/migrate-tfm.ps1:93-97`.
- **Q7. `src/go2cs/HashSet.go`, 10 never-called methods.** The candidate said 11 but listed 10, and the 10 are verified. Question: does the P1 hashset seat land in TRAIN N?
  - That seat is `origin/claude/p1-hashset-module` a7a7a197dd; its commit 8c2f827430 deletes `src/go2cs/HashSet.go`.
  - If yes: the item disappears. Do not trim the file separately, because the trim would collide with P1.
  - If no: the 10 methods are a zero-reference drop.
- **Q8. `docs/images/go2cs-large.png` (42,233).** Question: does `https://git.io/vhT1d`, cited in the attribution at `docs/images/README:7`, resolve to this file?
  - No reference inside the tree.
  - docs/ is the public Pages site.
  - The owner can open the link to settle it. Lean keep.
- **Q9. `src/archived/StdLibBuild 2025-05-11/graphs/package_dependencies.png` (33,913,728; excluded area).** Question: drop the rendered PNG and keep `package_dependencies.dot` (83,537) and `build-dot.bat`, which regenerate it?
  - Correction to the finder: deleting it at the tip saves about 34 MB per checkout and worktree, which matters for the disk floor in safety floor #12. It does not shrink clones: blob 8d81fafd stays in history, and safety floor #9 bars rewriting history.

## 4. KEPT AFTER VERIFICATION

- `src/tests/ElemAliasProbe/`: its arms are the cited evidence in `convCallExpr.go:4871`, `convUnaryExpr.go:34`, `InheritedTypeTemplate.cs:234, :506` and `struct-embedding.md:280, :335, :347`. It is the re-run reproducer named in `INVESTIGATION-element-aliasing.md`, and it is unregistered by design.
- `src/go2cs.sln.DotSettings`: the owner's 6638540d24 (2026-07-23, a month after the .slnx switch) added `sslice` through the IDE, so ReSharper/Rider still writes this file as the team-shared layer.
- `src/utilities/ReplaceInFiles/{ReplaceInFiles.exe, GSF.Core.dll}`: `src/set-version.ps1:25` runs it, and set-version.ps1 is a solution item (`src/go2cs.slnx:1073`). Replacing it with pure PowerShell would need its own rewrite seat, with a control that winres.json comes out byte-identical.
- `src/safe-push.sh` (and its census siblings, pending Q2): it is the push procedure in `.claude/skills/mailbox/SKILL.md:61, :66`, and `g-fetchable-check.sh:27` depends on `--new`.
- The `release-tc0` execution vocabulary (`_roster.ps1:116-130, :1566-1568, :1589`): the frozen `docs/validation/1.23.12.3/ValidatedTestPackages.md:353` carries `execution: release-tc0`. The parser throws on unknown values (`_roster.ps1:313-318`), and `push-nuget.ps1:1337-1345` parses frozen rosters. There is also an explicit retention ruling at `_roster.ps1:128-129`.
- `src/utilities/UpdateTestTargets`: `src/go2cs.slnx:1074`.
- `src/run-h10-recon.ps1`: referenced by `docs/GoCorpusMigration.md` and `src/run-h10-dispatch.ps1`.
- `safePushBash` (`safePushGuard_test.go:128`): still used by `safePushGuard_test.go:61` and `tokenDoorCensusGuard_test.go:45` after the H5 guards go.
- `src/go2cs/profiles/*.pubxml` (9 files, tracked even though `.gitignore:180` ignores `*.pubxml`): embedded by `src/go2cs/embeddedTemplates.go:39` (`//go:embed profiles/*`).

Side findings, not drops; COORD may add any of them to a seat:
- `src/tests/ElemAliasProbe/ElemAliasProbe.csproj:5` points to "element-aliasing-investigation.md at the repository root". It should point to `docs/phase4/INVESTIGATION-element-aliasing.md`.
- `docs/ConversionStrategies-Reference/struct-embedding.md:330-335` says the arm8 door is "NOT closed", but `convCallExpr.go:4862-4872` records it closed by 9028f55ef6.
- `.claude/rules/corpus.md:398-402` claims robocopy was "the repository's last external Windows tool dependency". That is untrue while ReplaceInFiles.exe exists. This file is coordinator-owned.
- `src/migrate-tfm.ps1:93-97` hard-codes `default: 9.0.x`, which is stale for the next TFM hop.
- `H:\Projects\go2cs\grep.exe.stackdump` is untracked in the working tree. It is local cleanup, not a train item.

## 5. PROPOSED SEAT SPLIT

**S-A "drop extraneous items"** covers D2, D3, D4, D5, D6, D7 and D8, plus Q1 if ruled yes. No seat in this group touches a registered test, projitems, solution or visitor doc.

```
git rm -- src/go2cs/launch.json src/go2cs.png \
  src/utilities/GetPackageDeps/GetPackageDeps.go src/utilities/GetPackageDeps/GetPackageDeps.exe \
  src/tests/Behavioral/clear-package-info.bat src/tests/Behavioral/clear-generated.bat \
  src/tests/Behavioral/TypeSwitch/build.cmd \
  src/tests/Behavioral/ExprSwitch/ExprSwitchExtComments.go.test src/tests/Behavioral/ExprSwitch/ExprSwitchMicro.go.test \
  src/tests/Behavioral/mod-init-all.ps1
# + src/utilities/go.mod   (only on Q1 = yes)
```

Gates for S-A:
- `go test ./... -timeout <explicit>` in `src/go2cs`.
- `check-solution-integrity.ps1`, because the Behavioral tree is touched.
- A CNR, because files under `src/tests/Behavioral` move. It should come out byte-identical, since no runner reads these files. It can share one CNR with S-C.
- `git diff --cached --name-status --diff-filter=D` must list exactly the paths above, and `git status --porcelain | grep '^ D'` must be empty (safety floor #8).

**S-B "retire H5 scaffolding"** covers D1. It needs its own seat because it deletes two tests that run in every go test and edits projitems. If Q2 or Q3 are ruled drop, add them here, because they edit the same file and the same class of guard.

```
git rm -- src/h5-c1-2-fixture/runtime2.pre-bill.cs src/apply-h5-c1-2-member-bill.sh src/apply-h5-c1-1-rederives.sh \
  src/go2cs/h5MemberBillGuard_test.go src/go2cs/h5RederivePatchGuard_test.go
# edit: delete src/go2cs/go2cs-src.projitems lines 149 and 150 (same commit)
```

Gates for S-B:
- `go test ./... -timeout <explicit>` on a box with bash. This covers TestProjitemsHasNoDanglingEntries and TestProjitemsRegistersEveryGoSource.
- Safety floor #13 control: remove the applier without its guard, run `go test -run TestH5RederivePatchSelfTest`, confirm it goes red and names the script, then complete the removal.
- The same deletion-list checks as S-A.
- No CNR: these are test-only files, and no emission source changes.
- The runbook owner may add an in-stage dated note at `docs/GoCorpusMigration.md:1076-1103`.

**S-C "dead converter helpers"** covers D10 plus the `shadowing.md:651` rename. It needs its own seat because it changes converter source and a visitor-read doc. Gates: `go test ./... -timeout <explicit>`, which includes the docs Liquid test, and a CNR showing zero drift. Nothing is deleted with `git rm`; this seat edits lines only.

**S-D "retire GenericTests"** covers D9. It needs its own seat because it touches a visitor doc, a script table and a GolibTests comment.

```
git rm -- src/tests/GenericTests/GenericTests.csproj src/tests/GenericTests/Program.cs src/tests/GenericTests/orderedSlice.cs
# edits: src/migrate-tfm.ps1:155; src/tests/GolibTests/Sha3ReinterpretVectorTests.cs:31-32; docs/ConversionStrategies-Reference/pointers.md:822
```

Gates for S-D:
- `check-solution-integrity.ps1` (expected unchanged).
- `git grep -n GenericTests` at the seat head must return records only.
- Build GolibTests once, because a file in it was edited.
- `go test ./...` with a timeout, for the docs check.
- The same deletion-list checks.

Merge hazards:
- S-B, any Q2/Q3 drop, and the P1 hashset seat all edit `src/go2cs/go2cs-src.projitems`. P1's deletion of HashSet.go must also remove its entry, because the registration check runs in both directions. Run the merge-hazards adjacent-hunk check on that file.
- Q7 is settled by P1 seating, not by any of these seats.

## 6. COVERAGE

What each lens searched:
- **hop-scaffolding:** scripts at the src/ top level and their `*Guard_test.go` and projitems entries; `src/tools`; `src/utilities`.
- **orphans:** unreferenced scripts, projects and fixtures; non-golden files inside Behavioral project folders; `src/tests` projects in no .slnx; IDE files; src/ top-level assets; `docs/images`.
- **stray-and-output:** committed binaries, the largest blobs, empty files, misplaced top-level files.
- **retired-mechanisms:** uncalled converter functions (a token census of 1,961 declared names), `_roster.ps1` functions and vocabulary, CI workflow options.
- **docs-and-tools:** `src/utilities`, `src/tools`, site image assets.
- **Synthesizer:** a tracked-but-ignored census. 47 hits, all live: 38 are `.claude/**` force-adds (excluded area) and 9 are the embedded `src/go2cs/profiles/*.pubxml`.

What no lens reported a census of:
- dead templates or code in `src/gen/**`;
- `src/tests/Performance/**`;
- the other `src/tests` utility projects (BehavioralRunner, PerformanceRunner);
- Behavioral projects missing from solutions (left to `check-solution-integrity.ps1`, not re-run);
- a full reference census of every `src/*.ps1`/`*.bat`/`*.sh` (only the named ones and `run-h10-recon.ps1` were checked);
- stale or duplicate living `docs/*.md` pages outside the excluded records;
- dead golib API (excluded by doctrine).

`.claude/**`, `src/archived/**`, `src/core/**`, the nugetgo tooling and the j0 tooling were out of scope. Where they appear above, it is only as questions.