## GAPS CHECKED

I ran about 25 git commands, all read-only against `8f46a9adaef35c804d6eba62056e9844608301f3`. My scratch files are in `/h/go2cs-tmp-coord/coord-scratch/n-cleanup-scout/`: `critic-lstree.txt`, `critic-nonexcl.txt` and `critic-nonbeh.txt`.

1. **IDE files and committed output.** I searched the tree for `.vscode/`, `.idea/`, `.vs/`, `*.user`, `*.suo`, `*.code-workspace`, logs, binlogs, `*.orig`, `*.rej`, `*.bak`, `*.tmp`, `*.trx`, `*.patch`, `*.diff`, `*.stackdump`, `bin/`, `obj/`, `TestResults/`, nupkg files and "- Copy"/".old"/"-backup" names, outside the excluded areas.
   - Only two files matched. `src/go2cs.sln.DotSettings` is already KEPT in the report.
   - The other is `src/go2cs/.vscode/settings.json` (353 B, a cSpell word list, added 87465f5f58 2025-01-12). It is tracked on purpose: `.gitignore:31` says "settings.json = the cSpell project dictionary", and `:34-35` reads `**/.vscode/*` then `!**/.vscode/settings.json`. Not a candidate.

2. **Second copies (a duplicate-blob census over the 1,167 files outside the excluded areas and Behavioral).** Every duplicate is intended:
   - the icon `75f1dc10`, 17 copies, one per Performance project;
   - `LICENSES/AGPL-3.0-only.txt` = `src/go2cs/LICENSE`;
   - `LICENSES/MIT.txt` = `src/gen/go2cs-gen/LICENSE`;
   - `e36698d6`, which is `docs/images/go2cs-small.png` = `src/go2cs.png` (D4) = `src/go2cs/go2cs.png`.
   - No helper module is copied between `src/tests` and `src/tools`, and no fixture has a second copy. Inside Behavioral the only duplicates are `X.cs` = `X.cs.target` golden pairs, which are out of scope.

3. **Top-level `src/` and `src/tests` files that section 6 says nobody censused.** I grepped `check-h6-completeness.ps1`, `h8-comparand.sh`, `handown-census.ps1`, `syscall-keepalive-census.ps1`, `run-h10-dispatch.ps1`, the four `src/tests/*.cs` files and `src/tests/sweep-oracle-rerun-selftest.ps1`.
   - **Live, referenced by the runbook as procedure for the next hop:**
     - `check-h6-completeness.ps1`: `docs/GoCorpusMigration.md:1361, :3772, :3939`
     - `handown-census.ps1`: `docs/GoCorpusMigration.md:1213, :1399`, plus `migrate-gorelease.ps1:539`
     - `h8-comparand.sh`: `docs/GoCorpusMigration.md:1688-1834`
     - `run-h10-dispatch.ps1`: `docs/GoCorpusMigration.md:2490, :2806`
     - Unlike the H5 appliers, none of these is pinned to a 1.23.12 tree. They take their GOROOTs as arguments.
   - **Live, run only by hand:** `syscall-keepalive-census.ps1` is cited by `src/core/syscall/windows/dll_windows.cs:32` as the guard you run when needed.
   - **Live, linked into projects:** the four `src/tests/*.cs` files.
     - `BehavioralRunner.csproj:31-33` links `ConverterBuildInputs.cs`, `PlatformExclusive.cs` and `BehavioralPackages.cs`.
     - `bestEffortTranspile_test.go:39` reads `BestEffortConversion.cs`.
     - `UpdateTestTargets/Program.cs:348` uses `BehavioralPackages`.
   - **Nothing runs `src/tests/sweep-oracle-rerun-selftest.ps1` (23,367 B).** No CI workflow, skill, rule, runbook or other script mentions it. Its only mention is a comment at `src/_roster.ps1:2231`. It is still maintained (0a4a336dd2 and 202a216b40, both 2026-09-30), so it is a live guard and not a drop. See C1.

4. **Other areas I checked; all live:**
   - `src/tests/ChannelTests` is in `go2cs.slnx:1046` and `migrate-tfm.ps1:153`.
   - `src/tests/PackageTests` is referenced by `csprojTemplate_test.go:140` and its README.
   - The `src/tests/Performance` tree is uniform. Its one odd file, `PerfTlsHandshake/package_init.cs`, belongs to a registered project (`go2cs.slnx:1070`).
   - `src/go2cs/internal/gen*`: `//go:generate` at `stdlibMetadata.go:19` and `symbols.go:19`; genpopulation via `check-roster-format.ps1:908` and `run-validated-sweep.ps1:256`.
   - `src/go2cs/testdata/validationproof` is read by `validationProofPages_test.go:20`.
   - `src/tour` (25 files) is cited by `docs/README.md:45` and `.gitattributes`.
   - Every image in `docs/images` and `docs/css/octicon.css` is referenced: `docs/README.md:45, :150, :366` and `docs/_layouts/default.html:26`.
   - `docs/ConversionStrategies-Reference.md` (132 KB) is a deliberate anchor-redirect page for the split reference (ac5b309125). Keep.

## DROP ROWS SPOT-CHECKED

| Row | Verdict | Evidence |
|---|---|---|
| **D2** GetPackageDeps | **Upheld**, no missed reference | A case-insensitive grep of the whole tree, including `.claude/**` and `docs/phase4/**`, for `GetPackageDeps` finds only `src/utilities/go.mod:1` (`module GetPackageDeps`). |
| **D5** `clear-package-info.bat` / `clear-generated.bat` | **Upheld**, no missed reference | A whole-tree grep for both names returns nothing. No `*.bat` glob appears anywhere under `src/`, `.github/` or `.gitattributes`. I read both files at the base: they recurse `del /F /Q` over every `package_info.cs`, and `rmdir /S /Q` over every `Generated` folder, from the current directory down. The hazard is as the report describes. |
| **D7** `ExprSwitchExtComments.go.test` / `ExprSwitchMicro.go.test` | **Upheld**, no missed reference | A whole-tree grep for both names and for `\.go\.test` returns nothing. No `*.test` or `*.go.*` glob appears in `src/` or `.github/`. |
| **D4** `src/go2cs.png` (extra check) | **Upheld** | Every relative `go2cs.png` reference resolves somewhere else. The only `../go2cs.png` is `src/core/golib/golib.csproj:68`, which means `src/core/go2cs.png`. Each of the 2,297 bare `go2cs.png` hits in csproj files resolves to that project's own folder. No path resolves to `src/go2cs.png`. |

## ADDITIONAL CANDIDATES

None of my checks found a new DROP. One question and two minor notes:

- **C1 (question for COORD, not a drop): `src/tests/sweep-oracle-rerun-selftest.ps1`.**
  - Size: 23,367 B.
  - Introduced: 77a0882cc4, 2026-09-02, "harness(sweep): a row whose ONLY divergences are Go=fail/C#=pass is re-run once before it fails".
  - What it does, in its own words: it "Guards the sweep's ORACLE-ONLY failure rule". It dot-sources `../_roster.ps1` and checks `Test-OracleOnlyFailure` / `Get-ResultsTailText` against fabricated records.
  - References: only `src/_roster.ps1:2231`, which is a comment.
  - Removal breaks nothing, because no test, CI workflow, `.slnx`, projitems or other script invokes it.
  - **That is exactly the risk.** It is a gate guard that has to be remembered, and it was still being extended on 2026-09-30. Suggest wiring it into the CI workflow (`os-matrix.yml`) or the `_roster.ps1` edit procedure, the same way the report frames Q4 for comparison-classifier. Do not drop it.
- **Side note: stale pointer.** Item 14 in `docs/CleanupBacklog.md:183` points at "ConversionStrategies-Reference.md ~line 10734". That page is now an anchor redirect, so the pointer no longer lands. This is an edit for whoever owns CleanupBacklog, not a drop.
- **Side note: `src/tests/PackageTests/ConvertedTestHarness`.** Item 11, still open in `docs/CleanupBacklog.md:135`, says it "does not build". `package-conversion.md:86` and `moduleNamespaceAgreement_test.go:32` describe that failure as history. I did not check its status further. COORD may want to confirm whether item 11 should be struck.

Coverage I did not reach: dead code inside `src/gen/**` templates (I only checked the file list), and the tour's `pipeline_test.go` vs `pipeline_v2_test.go`, where I looked at the names only.