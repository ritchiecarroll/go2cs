# RECON — IDE usability pass, VS Code + VS 2026 (2026-11-26, R-LAPTOP)

> **State: RECORD (2026-11-26).** The IDE half of the .NET 11 / IDE usability pass (COORD dispatch 20261126T114814Z),
> run on R-LAPTOP by a fresh lane-R session with computer use, report only. Routed by COORD 20261126T170118Z:
> finding 1 to P6's S5; findings 2, 3, 4, 7, 8, 9 to P1; finding 5 to P2; findings 6 and 10 recorded only (IDE behaviour).
> The 41 screenshots and the two saved VS build logs it cites (`ide-shots/`) are NOT in the repo: they stay on R-LAPTOP
> under `C:\go2cs-tmp\r7-gates\ide-shots\`. Profile paths are written `%USERPROFILE%`. Amended with dated blocks, never rewritten.

## Setup

- Tree: `C:\go2cs-tmp\r-ide`, worktree at origin/master **d6805be2e8367f2a3a26ac71cb225d2a13754120** (train 14; train 15 had not landed at fetch time 05:5x local).
- go2cs.exe built from the worktree with go1.26.11 (`env-12611.sh`).
- Gate: VS Installer (`setup.exe` 95840/682516 + msiexec) ran 05:39–06:14; pass started after `vswhere` reported `isComplete=True` and the installer exited (owner confirmed "Install finished").
- VS Community 2026 **18.11.12402.416 → 18.12.12412.261** (updated during the gate). SSMS 22 updated too.
- VS Code 1.138.0. C# extensions at start: C# Dev Kit 3.20.199, C# 2.140.9, .NET Install Tool 3.1.0 — **globally disabled on this box**; enabled for the pass (owner-approved) and auto-updated on enable to **C# Dev Kit 3.40.215, C# 2.160.4, .NET Install Tool 3.2.0**.

> **SHA note (added after the pass):** train 15 landed while the pass was running (origin/master → `51f4d4b37a90bf69c3180430a6025bd03f891d56`, a fast-forward carrying P1's restore cure). The whole pass was already under way, so it **finished on `d6805be2e8`**, as R instructed. A read-only diff `d6805be2e8..51f4d4b37a`: P1's cure (`68d912305a`, `1936aab3db`, `473e3de289`) changes the `go2cs -recurse` local-refs build files (`generateRecurseBuildFiles`: recorded target platform, twins/roll-forward imports). It does **not** change the csproj templates' `go2csPath`/`$(SolutionDir)` logic, and `testConversion.go` gains only one seed call. So finding 1 (the converted `-tests` project fails in the IDEs) should still hold at 51f4d4b37a: that's from reading the code, not a re-run. `src/Directory.Build.targets` (+25 lines) and `golib.csproj`/`GolibTests.csproj` also changed, so the repo-solution build and warning numbers may shift; not re-measured.

## .NET SDKs visible to the IDEs

The VS 18.12 update **removed SDK 10.0.404 and installed 11.0.100**. Machine PATH has only `C:\Program Files\dotnet\`:

```
dotnet --list-sdks (C:\Program Files\dotnet)
9.0.321 [C:\Program Files\dotnet\sdk]
11.0.100 [C:\Program Files\dotnet\sdk]
```
(`sdk\10.0.402` dir remains but is not listed — leftover.) Side-by-side, not on the GUI PATH: `%USERPROFILE%\dotnet10` = 10.0.400.
Runtimes: Microsoft.NETCore.App 8.0.34, 9.0.23, 10.0.15, 11.0.0.

So **only the .NET 11 SDK was visible to both IDEs** — arranged by the VS update itself, nothing uninstalled. Repo `global.json` = `10.0.400` + `rollForward: latestMajor`.

## Converted test project (greet)

`C:\go2cs-tmp\r-ide-greet\greet` (go.mod `example.com/greet`, `greet.go` + `greet_test.go`, `go test` ok), converted with
`go2cs -tests -test-action convert -go2cspath C:\go2cs-tmp\r-ide\src C:\go2cs-tmp\r-ide-greet\greet C:\go2cs-tmp\r-ide-greet\out` → rc 0, one warning:
`WARNING: Package license is unspecified for C:\go2cs-tmp\r-ide-greet\out\example.com.greet.csproj (reported once per module); add a LICENSE beside the project or pass -license with an SPDX expression before packing.`
Output: `example.com.greet.csproj` (Library) + `example.com.greet.tests.csproj` (Exe test host), no `.sln`.

## VS Code 1.138.0 + C# Dev Kit 3.40.215 / C# 2.160.4

How opened: **File > Open Folder…** via the native dialog, clicks only (Up button + double-click; the dialog's address bar and typing are blocked under click tier). Build had no clickable Dev Kit path (right-click only), so builds used **Terminal > Run Task… > dotnet > "dotnet: build"** (Dev Kit's task provider). Screenshots: `C:\go2cs-tmp\r7-gates\ide-shots\vsc-*.jpg`.

Box-specific noise (not go2cs, but what this user saw): VS Code restored a stale window ("The editor could not be opened because the file was not found"); Todo-Tree "Failed to find vscode-ripgrep"; "Failed to install Github Copilot upgrade extension for .NET upgrade"; Go extension "Invalid settings: setting option "vulncheck""; C/C++ "The language server crashed. Restarting…" + "Client cpptools: connection to server is erroring" (after the solution build); Copilot survey toast; "Deprecated extensions detected".

### VS Code — repo root (`C:\go2cs-tmp\r-ide`, what a new user opens after clone)

| Step | Result |
|---|---|
| Open | **Restricted Mode** banner: "Restricted Mode is intended for safe code browsing. Trust this folder to enable all features." (Workspace Trust; trusted). C# tooling was globally disabled on this box (enabled for the pass). On enable, the **C# extension auto-opened `src\archived\go2cs-examples.sln`** (`[solution/open] … Loading c:\go2cs-tmp\r-ide\src\archived\go2cs-examples.sln`) and restored the archived net5.0 examples. Then Dev Kit 3.40 (new "Workspace Dev Mode", announcement page "ATTENTION: Big changes are here in C# Dev Kit!") found **no solution at the root** and synthesized `workspaceStorage\…\ms-dotnettools.csdevkit\r-ide.sln` with **2008 projects** (every csproj in the tree, archived + tools + tests). |
| SDK | No prompt. Dev Kit log: `global.json found … Workspace folder "c:\go2cs-tmp\r-ide" requires SDK version: 11.0.100` — global.json 10.0.400 + latestMajor resolved to 11.0.100 silently. |
| Restore | **FAILED** — Problems: "Failed to restore NuGet packages for the solution." (`r-ide.sln`, NuGet). Cause in NuGet log: `src\tests\CSharpConsumer\CSharpConsumer.csproj(21,5): error : ConsumerConvertedRoot is not set: build this project through run-csharp-consumer.ps1, which converts the modules it references.` plus **77 × NETSDK1138** "The target framework 'net5.0' is out of support…" (archived examples). 82.8 s. |
| Build / Run / Test | Not attempted at the root (moved to `src` — see below). |

### VS Code — `src\go2cs.slnx` (folder `src` opened; status-bar "Open Solution" picker)

| Step | Result |
|---|---|
| Open | Folder `src` (two root .slnx) → C# Project Details shows an **Open Solution** button; picker "Choose solution to open" lists **`archived\go2cs-examples.sln` first and pre-selected**, then `go2cs-stdlib.slnx`, `go2cs.slnx`. Choosing go2cs.slnx **writes an untracked `src/.vscode/settings.json`** (`"dotnet.defaultSolution": "go2cs.slnx"`) — Source Control shows 1 change, branch shows `d6805be2*`. 1323 projects loaded (status "Projects: 1323"). The C# language server separately loads non-solution projects under the folder and toasts "There were problems loading project MnGoOrder.csproj" / "maps-with-pointers2.csproj"; C# log errors for `src\tests\PackageTests\*` ("The following PackageReference item(s) do not have a version specified: go.lib…") and `tools\mnspike2\MnLockPin` ("Skipping project …MnLockPin\gen\go2cs-gen\go2cs-gen.csproj because it was not found"). |
| Restore | **OK**, 226 s. **4 × NU1903** "Package 'Newtonsoft.Json' 9.0.1 has a known high severity vulnerability, https://github.com/advisories/GHSA-5crp-9r3c-p9vr" in PerformanceRunnerTests, GolibTests, GenTests, ChannelTests (Problems ⚠4). |
| Build | Terminal > Run Build Task → **"No build task to run found. Configure Build Task…"** (Dev Kit contributes no default build task). Via Run Task > dotnet > "dotnet: build": `dotnet build …\go2cs.slnx /p:Configuration=Debug "/p:Platform=Any CPU"` → **Build succeeded, 1873 Warning(s), 0 Error(s), 00:15:02.53**. Problems 1K+. Samples: CS9113 `core\testing\TestReporter.cs` "Parameter 'verbose' is unread."; IL2026/IL2111 (System.Delegate.CreateDelegate) across Behavioral `main.cs`; CS7022 BehavioralRunner `Program.cs` "The entry point of the program is global code; ignoring 'Runner.Main(string[])' entry point."; CS8600 BehavioralTests; CS0108 "hides inherited member" (Behavioral CrossPkgUser, EmbeddedInterfaceMergedMethod); CS8625 in `MnGenericContended\Generated\…chandir.g.cs`. |
| Run | Run > Run Without Debugging → "Select debugger" → ".NET 5+ and .NET Core" → **nothing happens** (no file open). Run and Debug > More C# options… lists launch configs ("C#: Launch Startup Project", PerformanceRunnerTests, GenTests, every Behavioral program…). Picking **C#: IotaEnum** → extension host unresponsive ~15 s, then toast/log **"Unable to determine debug settings for project 'c:\go2cs-tmp\r-ide\src\tests\Behavioral\IotaEnum\IotaEnum.csproj'"**; nothing ran. |
| Test | Test Explorer: "C# Dev Kit - Test Explorer" with BehavioralTests, ChannelTests, GenTests, GolibTests, PerformanceRunnerTests (net10.0), **plus a "Go" group** — the Go extension lists every Behavioral case directory as a Go test package. Ran **GolibTests**: builds first (`dotnet build go2cs.slnx /target:tests\library\GolibTests`), then **Total 3332, Passed 3231, Failed 0, Skipped 101, SUCCEEDED, 1217.68 s**. Counter froze at 126–147/3219 for ~6 min (host idle, 0.17 CPU-s/5 s) before resuming; header counter ended "3123/3123" vs summary total 3332. Single-project build replaced the 1873-warning Problems list with 4. BehavioralTests not run (multi-hour; drives conversions). |

### VS Code — `src\go2cs-stdlib.slnx` (status-bar solution switcher)

| Step | Result |
|---|---|
| Open | Status-bar "Solution: go2cs.slnx" → same picker (archived sln first) → go2cs-stdlib.slnx. |
| Restore | **OK**, 47 s, no warnings/errors. |
| Build | Run Task > dotnet: build → **Build succeeded, 0 Warning(s), 0 Error(s), 00:05:42.92** (ran after the go2cs.slnx build, so already-compiled projects re-emit nothing). |
| Run | No runnable app in the solution. |
| Test | No C# test projects (Test Explorer shows only the Go group). |

### VS Code — converted test project (`C:\go2cs-tmp\r-ide-greet\out`)

| Step | Result |
|---|---|
| Open | Restricted Mode banner again (new folder; trusted). No .sln → **C# Workspace Dev Mode**, Projects: 2 (example.com.greet, example.com.greet.tests). Immediately ⚠ **MSB9008 "The referenced project gen/go2cs-gen/go2cs-gen.csproj does not exist."** (example.com.greet.csproj). |
| Restore | Implicit; no restore error surfaced beyond MSB9008. |
| Build | Run Task > dotnet: build → **FAILED, 8 Error(s), 0.65 s**. Problems 16 errors / 3 warnings, all in the **production** project: CS0246 `fmt_package`, `string` (greet.cs); CS0234 "The type or namespace name 'builtin' does not exist in the namespace 'go'" (GlobalUsings.g.cs); CS0246 `GoPositionMapAttribute`/`GoPositionMap`/`GoPackageAttribute`/`GoPackage` (package_info.cs); MSB9008 for `gen/go2cs-gen/go2cs-gen.csproj` **and for `%USERPROFILE%\AppData\Roaming\Code\User\workspaceStorage\c1efe8…\ms-dotnettools.csdevkit\gen\go2cs-gen\go2cs-gen.csproj`**. Root cause (from the emitted csproj): production `example.com.greet.csproj` sets `go2csPath` = `$(SolutionDir)` in Debug with **no fallback pin** (the tests csproj has `$(MSBuildThisFileDirectory)../../r-ide/src/`); with no solution `$(SolutionDir)` is empty (plain build) or Dev Kit's workspaceStorage dir (synthesized solution), so every `$(go2csPath)core/...` reference vanishes. |
| Run | Run and Debug > More C# options… > **C#: example.com.greet.tests** → builds the tests csproj directly (recompiles the core closure from `r-ide\src\core` via the tests-csproj pin, 4:10, re-emits CS9113) → same 8 errors → dialog **"Build Failures — 'c:\go2cs-tmp\r-ide-greet\out\example.com.greet.tests.csproj' failed to build. Would you like to continue and run the last successful build?"** (answered No). |
| Test | Test Explorer: no C# tests (expected — the host is an Exe); only the Go extension's `out › greet_test.go` node (the copied Go test file). |

## Visual Studio Community 2026 18.12.12412.261

How opened: start window **"Open a project or solution"** and **File > Open > Project/Solution…**, native dialog, clicks only (tree pane → C: → double-click folders → select file → Open). Screenshots: `ide-shots\vs-*.jpg`; full build logs saved via File > Save Output As: `ide-shots\vs-go2cs-slnx-rebuild-output.txt`, `ide-shots\vs-go2cs-stdlib-rebuild-output.txt`.

Box-specific noise: post-update "What's new — Visual Studio has been updated" document on first open; the start window's recents list `C:\Projects\go2cs\src\go2cs.slnx` (main checkout, not used).

### VS — `src\go2cs.slnx`

| Step | Result |
|---|---|
| Open | Loads 1323 projects ("Loading project N of 1323") in ~1.5 min, no dialog. **No .slnx save prompt on open or on File > Close Solution** (the expected shared-projitems prompt did not appear in 18.12). **Default startup project is `go.arrays`** (a stdlib Class Library). Git Changes initially "Git version control isn't being used yet", then picks up the worktree ("Detached at d6805be2", 1 change = VS Code's `src/.vscode/settings.json`). |
| SDK | No prompt (only SDK 11.0.100 visible; global.json 10.0.400 + latestMajor). |
| Restore | Implicit with the build, no errors; **4 × NU1903** (Newtonsoft.Json 9.0.1, high severity) in PerformanceRunnerTests, GolibTests, GenTests, ChannelTests. |
| Build | Build > **Rebuild Solution**: "Rebuild All: **1321 succeeded, 0 failed, 0 skipped**", **21:03.196 min**. Error List **0 Errors / 1867 Warnings / 0 Messages**. Census of the saved log (1869 warning lines): CS8632 ×1076 (GolibTests, "The annotation for nullable reference types should only be used in code within a '#nullable' annotations context"), IL2026 ×183 + IL2111 ×183 + IL2091 ×42 (Behavioral, CreateDelegate / `go.builtin.new<T>()`), CS0649 ×181 (GolibTests 177), CS8981 ×119 ("The type name 'rune' only contains lower-cased ascii characters", GolibTests/ChannelTests GlobalUsings), CS8778 ×46, CS8602 ×18, NU1903 ×4, CS8600 ×4, CS7022 ×2 (BehavioralRunner, PerformanceRunner), CS8973 ×2, CS0108 ×2, and singles CS9113 (`core\testing\TestReporter.cs` — **the only warning in `core`**), CS1685 ("The predefined type 'RuntimeHelpers' is defined in multiple assemblies … using definition from 'go2cs-gen, Version=1.26.11.1'"), CS8625, CS8604, CS0168, CA1416 (GolibTests RuntimeNumCPUWindowsTests), GO2CS0011 (Behavioral RefusalDowngradeProxy, intentional `-warn`). An immediate Build Solution afterwards rebuilt 3 projects (internal.goexperiment, GolibTests, MethodValueReassignCapture), 1318 up-to-date, 36.9 s. |
| Run | Start Without Debugging with the default startup → dialog **"A project with an Output Type of Class Library cannot be started directly. In order to debug this project, add an executable project to this solution which references the library project. Set the executable project as the startup project."** After picking **AliasImport** in the startup dropdown: ran (console window via VsDebugConsole; contents not visible — Terminal access was declined in computer use), Diagnostic Tools/Monitoring attached. |
| Test | Test Explorer first says "Build your solution to discover all available tests. Click "Run All" to build, discover, and run all tests in your solution." (even right after the Rebuild). After a Build: **"Test discovery finished: 8331 Tests found in 10.3 sec"** — BehavioralTests 4292, ChannelTests 24, GenTests 729, GolibTests 3222, PerformanceRunnerTests 64. Ran **ChannelTests: 24 Passed, 0 Failed, 0 Skipped, 9.1 s**. (GolibTests already run in VS Code; BehavioralTests not run — multi-hour, drives conversions.) |

### VS — `src\go2cs-stdlib.slnx`

| Step | Result |
|---|---|
| Open | File > Close Solution (no prompt) → start window → Open a project or solution → go2cs-stdlib.slnx. Loads, startup again `go.arrays`. Test Explorer keeps showing the previous solution's tests/results. |
| Restore | Implicit; NuGet "Restored …" lines, no errors/warnings. |
| Build | Rebuild Solution: "Rebuild All: **364 succeeded, 0 failed, 0 skipped**", **15:55.611 min**, **2 warnings**: **MSB3061 "Unable to delete file "C:\go2cs-tmp\r-ide\src\gen\go2cs-gen\bin\Debug\netstandard2.0\go2cs-gen.dll". The process cannot access the file … because it is being used by another process"** (the generator DLL is held by the running IDE, so Rebuild's clean cannot delete it); CS9113 `core\testing\TestReporter.cs`. |
| Run | No runnable project (default startup go.arrays → same Class Library dialog). |
| Test | Test Explorer: **"System.NullReferenceException: Object reference not set to an instance of an object."** (1 Error) after the switch from go2cs.slnx, during the rebuild; no tests. |

### VS — converted test project (`C:\go2cs-tmp\r-ide-greet\out\example.com.greet.tests.csproj`)

| Step | Result |
|---|---|
| Open | File > Open > Project/Solution → the tests csproj (no .sln exists). Opens with a temporary solution; startup project = example.com.greet.tests. Git Changes "0 Repositories". |
| Restore / Build | Build Solution → **Build failed**: **4 × NU1104** "Unable to find project 'C:\go2cs-tmp\r-ide-greet\out\core\testing\testing.csproj'. Check that the project reference is valid and that the project file exists." (also golib, context, io) + **MSB9008** "The referenced project C:\go2cs-tmp\r-ide-greet\out/core/context/context.csproj does not exist." Here **even the tests csproj breaks**: in VS `$(SolutionDir)` = the project folder, so its `'$(Configuration)'=='Debug' and '$(SolutionDir)'!=''` branch sets `go2csPath` = `out\` and the `../../r-ide/src/` fallback pin is never reached. |
| Run | Start Without Debugging → **"There were build errors. Would you like to continue and run the last successful build?"** (No). |
| Test | Test Explorer: "Build failed" / "Build your solution to discover all available tests." — nothing discovered (expected: Exe host). |
| Close | File > Exit → **"Save changes to the following items? example.com.greet.tests.slnx*"** (VS's temporary solution; Don't Save). This is the only save prompt seen in the whole pass. |

## Summary of findings (go2cs-actionable first)

1. **Converted `-tests` output does not build in either IDE when opened directly** (no solution). Production csproj: Debug `go2csPath = $(SolutionDir)` with no fallback → VS Code: CS0246/CS0234/MSB9008 (8 errors); VS: tests csproj also resolves `go2csPath = $(SolutionDir) = out\` → NU1104 ×4. The tests csproj's own `../../<root>/src/` pin only helps when `SolutionDir` is empty. CLI-side `-test-action build` presumably passes `go2csPath` explicitly (not verified here).
2. **Repo root opened in VS Code** (no root .sln): C# Dev Kit 3.40 synthesizes a 2008-project workspace solution (archived + tools + PackageTests + CSharpConsumer) and restore **fails** on `CSharpConsumer.csproj`'s deliberate `ConsumerConvertedRoot is not set` error, with 77 × NETSDK1138 from archived net5.0 examples. The C# extension alone auto-opens `src\archived\go2cs-examples.sln`.
3. **Solution picker lists `archived\go2cs-examples.sln` first and pre-selected** (VS Code, folder `src`).
4. **VS default startup project is `go.arrays`** (Class Library) for both solutions → "cannot be started directly" on F5/Ctrl+F5.
5. **Warnings**: go2cs.slnx ~1870 warnings, essentially all in test projects (GolibTests CS8632 ×1076 dominates; Behavioral IL2026/IL2111). Converted stdlib is clean except CS9113 in hand-owned `core\testing\TestReporter.cs`. 4 × NU1903 Newtonsoft.Json 9.0.1 (high severity advisory) in the four MSTest projects.
6. **VS Rebuild of go2cs-stdlib.slnx: MSB3061 cannot delete `go2cs-gen.dll`** while the IDE holds it.
7. VS Code Run of a Behavioral program via Dev Kit launch configs: **"Unable to determine debug settings for project …IotaEnum.csproj"**, nothing runs; Run Without Debugging with no file open does nothing; no default build task ("No build task to run found").
8. VS Code writes an untracked `src/.vscode/settings.json` (`dotnet.defaultSolution`) — `.vscode/` is not gitignored.
9. Go extension (golang.go) lists every Behavioral case dir, and the converted output's copied `greet_test.go`, in Test Explorer.
10. VS Test Explorer: NullReferenceException after switching solutions; requires a build after load before discovery.
11. **No SDK prompt anywhere** with only .NET 11.0.100 visible; no `.slnx` save prompt for the repo solutions (only for VS's temp solution around a bare csproj).

Not verified / not done: BehavioralTests in either IDE (multi-hour, runs conversions); GolibTests in VS (run in VS Code only); converted-program console output in VS (console window masked).

## Changes to this box made by the pass (for the owner)

- VS Code: C#, C# Dev Kit, .NET Install Tool were globally disabled; enabled for the pass (owner-approved), auto-updated to 2.160.4 / 3.40.215 / 3.2.0 (old versions remain on disk). C# and C# Dev Kit are **disabled again**. **.NET Install Tool is left ENABLED**: VS Code refused to disable it alone ("'GitHub Copilot upgrade' and 'GitHub Copilot modernization for .NET' extensions depend on this") and disabling those two as well would go beyond the original state — owner's call.
- VS Code Workspace Trust now lists `C:\go2cs-tmp\r-ide` and `C:\go2cs-tmp\r-ide-greet\out` (paths deleted at cleanup).
- VS / VS Code recent lists include the go2cs-tmp paths.
- `%USERPROFILE%\source\repos\Output-Build.txt` was written twice by Save Output As, copied here and deleted.
- Leftover MSBuild/VBCSCompiler nodes from the IDE builds stopped by PID; IDEs closed.
