# .NET 11 SDK usability: a converted program, and the repository, on a machine with only .NET 11

Measured 2026-11-20 on i9 (windows/amd64) with an isolated .NET 11.0.100 SDK (runtime 11.0.0 only), unzipped from
Microsoft's archive with its SHA-512 checked against the release metadata. `DOTNET_ROOT` and `PATH` named that folder
alone, with the machine's dotnet off `PATH`, and the CLI home, NuGet cache and tool path were fresh. Nothing was
installed machine-wide. Owner item, COORD rulings 2026-11-20 07:24Z (the fix) and 11:36Z (this note).

## A converted program

| Step | On .NET 11 alone |
|---|---|
| `dotnet tool install go2cs` (1.26.10.3, then the 1.26.11.1 converter) | installs and runs (a native executable per RID) |
| `go2cs -recurse=nuget` | converts; the output has no `global.json`, targets `net10.0`, sets no `RollForward` |
| `dotnet build` | builds, 0 warnings / 0 errors; the `net10.0` targeting, runtime, host and crossgen packs restore from nuget.org |
| `dotnet run`, or the built exe | **fails at launch**: "You must install or update .NET to run this application", framework `Microsoft.NETCore.App` 10.0.0 not found (exit 150) |
| `dotnet publish` with the emitted profile | publishes self-contained, carrying runtime 10.0.x, and runs with no .NET 10 anywhere |

`global.json` plays no part: the converter writes none, and the SDK is chosen once, from the folder the command
starts in. A build that compiles a go2cs clone's projects (`-p:go2csPath=<clone>/src`, about 64 projects for `fmt` and
`strings`) succeeds on SDK 11 when started outside the clone.

**The fix** is go.lib's `buildTransitive/go.lib.targets` giving an executable `RollForward=Major` (THE ROLL-FORWARD
block). Major rolls forward only when the requested major is absent: a host trace shows the same exe bound to 10.0.15
where .NET 10 is installed, and to 11.0.0 on the 11-only root. Gate: `src/tests/PackageTests/RollForward`. The README
walkthrough (`fatih/color`, `go2cs -recurse=nuget`, build, run) on .NET 11 alone: with the published go.lib 1.26.11.1
the run exits 150; with the working-tree targets the runtimeconfig reads `Major` and the output is byte-identical to
`go run`.

**What the default reaches** (P3's review, 2026-11-20 12:51Z; each case is an arm of the gate, and each clause of the
condition was neutered on a copy of the file and turned exactly its own arm red):

- an `Exe` and a `WinExe` (a converted program inside a Windows GUI host fails at launch the same way without it);
- a TEST project too: a converted test host is an executable (`src/go2cs/test-csproj-template.xml` sets `OutputType`
  Exe), so a converted `-tests` project that restores the go.* packages runs its tests on .NET 11 when 10 is absent.
  The validation of record (the roster, the banked counts) is taken on .NET 10; a run on a newer major is not a
  validation of it;
- NOT a library (it chooses no runtime), NOT a project that sets `RollForward` itself, and NOT a project that brings a
  `runtimeconfig.template.json`. With both a template that set `rollForward` and the property, the SDK wrote
  `rollForward` twice into `runtimeconfig.json` (Major, then the template's value), so a template turns the default off;
- a self-contained publish carries its own runtime: the same warnings (none) with and without the file.

A user who wants the refusal back sets `RollForward` to `Disable` or `LatestMinor` in the project (docs/README.md,
Requirements).

## The repository on .NET 11 alone (contributor side): measured, not changed

`/global.json` pins `10.0.400` with `rollForward: latestFeature`, and `latestFeature` never crosses a major, so every
`dotnet` command started inside a clone is rejected on a machine with only SDK 11: "A compatible .NET SDK was not
found. Requested SDK version: 10.0.400" (exit 155).

GolibTests, built Release with SDK 10 and run by SDK 11 from outside the clone with `DOTNET_ROLL_FORWARD=Major` (its
test host on runtime 11.0.0), against the same build on runtime 10.0.15:

| Runtime | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| 10.0.15 | 3011 | 2953 | 0 | 58 |
| 11.0.0 (rolled forward) | 3011 | 2948 | 5 | 58 |

The skip sets are identical by name. All five failures are one cause: the test starts a child `dotnet test` or
`dotnet build` from inside the clone, and the child is refused by `global.json` exactly as above
(`AManagedExceptionEscapingAGoroutineIsCopiedToTheCrashOutput`, `AnImmutableSettingKeepsTheValueAnEarlierSetenvCached`,
`TheFirstUpdateParsesTheStartupEnvironment`, `AProcessThatLinksNoCgoPackageHasNoIndexUntilItRegistersAndThenBackfillsTheExactRegistry`,
`ABlockEventReturnsWhenEveryAllocationIsSampled`). No golib test failed on runtime 11 for a reason of its own.

Options for a contributor with only SDK 11, for a later ruling: `rollForward: latestMajor` would admit SDK 11 inside the
clone (the full repository, its `go test` and the sweep under SDK 11 are UNMEASURED); or keep the pin, which OQ-H2 ruled
tolerant inside the major and `csprojMetadata_test.go` holds, and state the .NET 10 SDK requirement for contributors.
