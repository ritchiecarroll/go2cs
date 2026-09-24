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

# `RidCompileAsset`: the RID-selected compile asset gate

`RidCompileAsset` is the consumer fixture for go.lib's `buildTransitive/go.lib.targets`. A
multi-flavour `go.*` package (layout L3) ships its reference flavour (Windows) in `lib/<tfm>/` and
each platform's flavour under `runtimes/<rid>/lib/<tfm>/`. NuGet resolves the COMPILE asset from `lib/`
whatever the RID is, so before the targets file a Linux consumer compiled against the Windows surface.
The fixture touches a type only the current platform's `go.syscall` flavour defines (`Rlimit` on
Linux, `DLLError` on Windows). `test-rid-compile-asset.ps1` restores it from ONE feed into a fresh
package cache and runs four arms:

- a RID-less build and run;
- the same with `-r <host rid>`;
- a framework-dependent `dotnet publish -r <host rid>`, whose PUBLISHED app is then run;
- a control that proves the arm can fail. On Linux, `-p:GoRidCompileAssets=false` must fail with
  CS0426. On Windows, the reference platform, the control is the no-op proof: the `runtimes/win-x64`
  twin is byte-identical to `lib/`.

```text
pwsh src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1 -Version <go.* version> [-Source <feed dir or URL>]
```

Against a package set that predates the targets file, `-TargetsFile src/core/golib/buildTransitive/go.lib.targets`
imports the working-tree file explicitly. That is how the gate was read red-first, against the
published 1.24.13.1: on Linux both build arms FAIL with CS0426 and the control PASSES. With the file,
all four arms PASS on both platforms.
