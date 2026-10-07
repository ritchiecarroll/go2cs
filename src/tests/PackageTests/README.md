# Converted package-test fixtures

`ConvertedTestHarness` is the end-to-end fixture for the opt-in converted Go test path (`-tests`).
It covers same-package access to an unexported declaration, an external `package p_test` with both
the named and the DOT self-import form (`. "go2cs/convertedtestharness"` — the shape the
`unicode/utf8` first-proof package requires), typed discovery, `TestMain`, duplicate parallel
subtests, cleanup, `TempDir`, and a `testdata` fixture read from the isolated working directory. (Invalid
test names like `Testlower` cannot live in the fixture — `go test`'s default vet `tests` analyzer
refuses to build them — so non-registration is guarded by the converter's `TestIsGoTestName`.)

From the repository root, build the converter (`go build -o bin/go2cs.exe .` in `src/go2cs`) and run:

```text
go2cs -tests -test-action all -go2cspath <repository>/src <repository>/src/tests/PackageTests/ConvertedTestHarness
```

The command converts production and test sources, builds and runs the generated C# host in an
isolated process, captures a clean `go test -json -count=1` baseline, and compares terminal
results by full Go test name. Artifacts (converted `.cs`, the `.tests.csproj`, the manifest, and
the comparison/results files) are regenerated in place and are gitignored.

⚠ **A `validated` result also writes two files OUTSIDE this directory, and they must be restored.**
`publishesRosterArtifacts` (`src/go2cs/validationProofPages.go`) admits any unfiltered `validated`
comparison, and it does not ask whether the converted package is a corpus package — so a successful
run of this fixture writes `docs/validation/current/go2cs.convertedtestharness.md` and adds an index
row to `docs/validation/index.md` pointing at `src/core/go2cs/convertedtestharness`, a package that
does not exist. Neither file is gitignored. Restore both after a run
(`git checkout -- docs/validation/index.md` and delete the untracked page), and read an unfiltered
`git status --porcelain` rather than a filtered one. The structural remedy is the predicate the
validation-pack block already uses for the same population — `rewriteOfCorePackage`, an
output-location test no fixture satisfies — but changing that gate risks silently un-publishing a
real roster row's proof page, so it is a ruling for the roster owner rather than a fixture-side fix.

# `ConsumerUsings`: the consumer usings gate

`ConsumerUsings` is the consumer fixture for the global usings go.lib's `buildTransitive/go.lib.targets`
gives every project that references a go.* or nugetgo.* package: `using go;` and
`using static go.builtin;`. Its `Program.cs` declares neither, so it compiles only if the package
supplies them. `test-consumer-usings.ps1` restores it from ONE feed into a fresh package cache and runs
two arms:

- a build and run with no usings in the source;
- a control that proves the arm can fail: with `-p:GoConsumerUsings=false` the build must fail with
  CS0246.

```text
pwsh src/tests/PackageTests/ConsumerUsings/test-consumer-usings.ps1 -Version <go.lib version> [-Source <feed dir or URL>]
```

Against a go.lib published before the usings existed, `-TargetsFile src/core/golib/buildTransitive/go.lib.targets`
imports the working-tree file explicitly. That is how the gate was read red-first, against the published
1.24.13.3: without the file the first arm FAILS with CS0246 and CS0103; with it, both arms PASS.

# `RidCompileAsset`: the RID-selected compile asset gate

`RidCompileAsset` is the consumer fixture for go.lib's `buildTransitive/go.lib.targets`. A
multi-flavour `go.*` package (layout L3) ships its reference flavour (Windows) in `lib/<tfm>/` and
each platform's flavour under `runtimes/<rid>/lib/<tfm>/`. NuGet resolves the COMPILE asset from `lib/`
whatever the RID is, so before the targets file a Linux consumer compiled against the Windows surface.
The fixture touches a type only the current platform's `go.syscall` flavour defines (`Rlimit` on
Linux, `DLLError` on Windows, `Kevent_t` on macOS, where `osx-x64` and `osx-arm64` ship the same darwin
build). The flavour is chosen from the RID's prefix (`win`, `linux`, `osx`), so an SDK that reports a
distro RID (`ubuntu.24.04-x64`, say) selects none and the control arm fails there. `test-rid-compile-asset.ps1` restores it from ONE feed into a fresh
package cache and runs four arms:

- a RID-less build and run;
- the same with `-r <host rid>`;
- a framework-dependent `dotnet publish -r <host rid>`, whose PUBLISHED app is then run;
- a control that proves the arm can fail. On Linux and macOS, `-p:GoRidCompileAssets=false` must fail
  with CS0426. On Windows, the reference platform, the control is the no-op proof: the `runtimes/win-x64`
  twin is byte-identical to `lib/`.

```text
pwsh src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1 -Version <go.* version> [-Source <feed dir or URL>]
```

Against a package set that predates the targets file, `-TargetsFile src/core/golib/buildTransitive/go.lib.targets`
imports the working-tree file explicitly. That is how the gate was read red-first, against the
published 1.24.13.1: on Linux both build arms FAIL with CS0426 and the control PASSES. With the file,
all four arms PASS on both platforms.

# `PublishSymbols`: the symbol files of a second single-file publish

`PublishSymbols` is the consumer fixture for go.lib's `buildTransitive/go.lib.symbols.targets`. A single-file
publish leaves each referenced assembly's `.pdb` loose beside the executable, but only the bundler names those
files as published, and a publish that finds its bundle up to date skips the bundler; the SDK's incremental
publish clean then deletes them. A frame in that assembly prints no file:line from then on. The go.* packages
ship no `.pdb`, so the fixture's frame lives in its OWN referenced library (`lib/`). `test-publish-symbols.ps1`
restores it from ONE feed into a fresh package cache and publishes it single-file and self-contained for the
host's RID into ONE folder, twice, with nothing changed between:

- a green arm: both publishes leave `PublishSymbolsLib.pdb` beside the host and the frame reads `Where.cs:<line>`;
- a control that proves the arm can fail: with `-p:GoKeepSymbolsLoose=false` the second publish loses the file
  and the frame reads `:0`.

```text
pwsh src/tests/PackageTests/PublishSymbols/test-publish-symbols.ps1 -Version <go.lib version> [-Source <feed dir or URL>] [-FallbackFolder <package folder for the runtime pack>]
```

Read red-first against a go.lib packed from master 40a1f839c5 (2026-10-05, win-x64): the green arm's second
publish kept only `PublishSymbols.pdb` and printed `:0`, and the control passed. The same happens with no go.lib
at all: it is the SDK's behaviour for any single-file app with a referenced library.

`test-publish-symbols-aot.ps1` is the Native AOT control: the target is meant to be inert under `PublishAot`, so the
fixture published with `PublishAot=true` and `PublishSingleFile=true`, once with the target on and once with
`-p:GoKeepSymbolsLoose=false`, must leave identical publish folders (and the AOT executable must run). It needs the
platform's native toolchain. Both scripts take `go.*` from `-Source` only and everything else from nuget.org, so a
self-contained publish can restore the SDK's runtime and ILCompiler packs against a pack rehearsal's local feed.

```text
pwsh src/tests/PackageTests/PublishSymbols/test-publish-symbols-aot.ps1 -Version <go.lib version> [-Source <feed dir or URL>] [-TargetsFile <go.lib.targets>]
```

# `PackageSymbols`: a consumer's std frames resolve to their Go file:line

`PackageSymbols` is the consumer fixture for the go.* packages' OWN symbol files. Its program prints the first
`go.sort` frame above a `sort.Slice` comparator as the converted runtime resolves it (`runtime.Caller`), which
reads the frame's file:line from `go.sort`'s `.pdb` beside the application: `slice.go:<n>` or `zsortfunc.go:<n>`
with the symbols, `none` without. `test-package-symbols.ps1` restores it into a fresh package cache, `go.*` from the
feed under test ONLY and everything else (the runtime and ILCompiler packs a publish needs) from nuget.org:

- RUN: `dotnet run` resolves the frame, and every dependency assembly in the build output has its `.pdb` beside it
  with the matching debug id (the RID-specific packages' assemblies included);
- PUBLISH: a single-file, self-contained publish for the host's RID, twice into one folder, resolves it both times;
- FDD: a framework-dependent publish that is not single-file resolves it, the `.pdb` matching as in RUN;
- OFF: with `-p:GoCopyPackageSymbols=false` the output is today's (`none`, no dependency `.pdb`): the off switch,
  and the proof the arms above can fail;
- AOT (`-Aot` only; needs the ILCompiler packages): a Native AOT publish succeeds and its `.pdb` list is printed.

```text
pwsh src/tests/PackageTests/PackageSymbols/test-package-symbols.ps1 -Version <go.* version> [-Source <feed dir or URL>] [-FallbackFolder <package folder for the runtime pack>] [-Aot]
```

Read red-first (2026-10-06, win-x64) against a local pack in the published 1.24.13.4 shape (no `.pdb` in any
package): RUN, PUBLISH and FDD failed (`none`; 31 dependency assemblies, 0 with a `.pdb`), OFF passed.

# `release-smoke.ps1`: consume a feed the way a user does

`release-smoke.ps1 -Feed <dir> -Converter <go2cs> -WorkRoot <dir>` runs four arms against ONE local
feed (a `push-nuget.ps1 -VersionSuffix` rehearsal's merged output), each restoring into a fresh cache
with `go.*` mapped to the feed alone: (A) `RidCompileAsset` above; (B) a generated stdlib program
(file I/O, plus every `sort` form whose converted body once called itself: `IntSlice`, `Float64Slice` and
`StringSlice` `.Sort()` and the bare `sort.Sort(sort.StringSlice(v))`) converted with `go2cs -recurse=nuget`,
built, run, and its stdout compared byte for byte with `go run`; a crash that prints "Stack overflow" is named
as one in the verdict. That red reading is the tiered-JIT shape (the arm runs a Debug build with tiered
compilation on): under Release with `DOTNET_TieredCompilation=0` the same self-call loops forever instead, so a
TIMEOUT on this arm is the same defect in its other shape;
(C) `Behavioral/StatLayoutTruth`, the same way; (D) the README walkthrough (`fatih/color`), the same
way, gating only with `-GateWalkthrough` and MEASURED without it; the `release-smoke` stage passes it on every
leg; (E) `PublishSymbols` above, the guard and its AOT control against the same feed, MEASURED and never gating
until it has read green on two trains; (F) `PackageSymbols` above with `-Aot`, the packages' own symbol files, MEASURED
and never gating on the same terms, its frame lines and its AOT `.pdb` list carried into the verdict. It exits 0 when the gating arms pass. The `release-smoke` stage of
`.github/workflows/os-matrix.yml` packs the feed on Windows and runs this on all four shipped RIDs.
`-Feed https://api.nuget.org/v3/index.json -Version <go.* version>` runs the same arms against a published
release instead (a URL feed requires `-Version`; nuget.org is then the one source); the stage's `published_version`
input does this on every leg, with no pack.
