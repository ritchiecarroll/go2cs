# Package Conversion

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#package-conversion)
Although a Go package more traditionally parallels a C# namespace, Go includes referenceable functions directly from within a package root, for example, the `Println` function in the `fmt` package is called like: `fmt.Println("Hello, world")`. For C#, only type declarations, e.g., `class`, `struct`, `enum`, etc., are allowed in a namespace; functions exist as part of a `class` or `struct`. Described from a C# perspective, all Go functions are [`static`](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members), i.e., the functions exist separately from an instance of a type. Go supports the notion of a receiver function which allows a function to be targeted to an instance of a type (paralleling the operation of a C# extension function), but this is still a static function.

As such, the conversion strategy for a Go package is to convert it into a static C# partial class, e.g.: `public static partial class fmt_package`. Using a partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`. The receiver functions are emitted as extension methods on that partial class (decorated with `[GoRecv]`, see [Source Generators](source-generators.md#source-generators)).

So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are simply converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`. Each package also emits a `package_info.cs` carrying a `[GoPackage]` assembly attribute plus the package-wide global `using` aliases (Go's built-in types, exported type aliases, etc.).

A consequence of converting a Go method to a C# **extension method** is that C# only discovers an extension method when its containing static class's *namespace* is in scope (via a `using <namespace>;` directive or the enclosing namespace) — a class **alias** such as `using atomic = go.@internal.runtime.atomic_package;` resolves the *type* (`atomic.Uint32`) but does **not** bring the class's extension methods into scope. This matters when a file calls a method on a value whose type comes from a multi-segment-path package (one that lands in a sub-namespace, e.g. `internal/runtime/atomic` → `go.@internal.runtime`): Go never requires importing a value's package merely to call a method on it, so such a file may emit no import — and hence no `using @internal.runtime;` — leaving the extension method invisible and the call mis-binding to a wrong (e.g. embedding-promoted) overload (CS1929). The converter therefore registers the namespace of **every cross-package method's defining package** as a file-local `using` at the call site, independent of the file's explicit imports. (Packages in the root `go` namespace — most top-level stdlib packages — need nothing extra, since same-namespace extension methods are always visible. This is a stdlib-structural concern that only surfaces under multi-segment package paths, so it is guarded by the Phase-3 `runtime` build rather than a single-package behavioral test.)

Go projects that contain a `main` function are converted into a standard C# executable project, i.e., `<OutputType>Exe</OutputType>`. The conversion process can reference and convert needed external projects as library projects, i.e., `<OutputType>Library</OutputType>`, per any encountered `import` statements. In this manner an executable with packages compiled as project-referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) can be produced.

An executable's **`<AssemblyName>` is the last element of its import path**, mirroring `go build`, which names a binary after the module/directory's final segment — so `module example.com/colordemo` produces `colordemo.exe`, not `example.com.colordemo.exe` (the full dotted project name). Only the `Exe` assembly name is shortened; the `.csproj` filename keeps the full dotted path (its identity in the solution and in `ProjectReference`s), and **library** assemblies keep the full dotted `<AssemblyName>` — their DLL and NuGet `PackageId` (`go.$(AssemblyName)`) must stay unique across the package graph (e.g. `github.com.fatih.color`).

## A project name is the package's FULL import path — a go-file-free container directory does not truncate it

The dotted project name above is the package's **import path**, every segment of it, joined with `.`
(`getProjectName`, `importOperations.go`). That is not a formatting preference: the name is the `.csproj`
filename, the library `<AssemblyName>`, the NuGet `PackageId`, and — minus its last segment — the C#
**namespace**. All four have to be unique across the package graph, and the import path is the only thing
about a package that is unique by construction.

For the standard library the path is read straight off `GOROOT/src`. For everything else the converter has
only a directory, so it recovers the import path the way the go command itself finds the main module:
**walk up to the nearest `go.mod`**, then join that module's declared path with the package's path relative
to it. The result is the import path by definition.

The walk used to stop early. Alongside `go.mod` and `main.go` it treated *the first ancestor directory
holding no `.go` files of its own* as a boundary and named the project after the leaf segment alone. Go
modules are made of such pure container directories — `internal/`, a proto grouping like `xds/core/`, the
`datatransfer/` above an `apiv1/`, a service tree's `endpoints/` parent — so the truncation was routine
rather than exotic, and it cost two things:

* **Duplicate project names.** `cloud.google.com/go/bigquery`'s `datatransfer/apiv1` and `storage/apiv1`
  both emitted `apiv1.csproj`. Visual Studio refuses to open a solution containing two projects of the
  same name and reports no reason — silently doing nothing from a file dialog, offering to forget the
  entry from the recent list — so **the entire generated `.slnx` failed to load** ([issue #35]). One
  reported `-recurse` conversion had **175 such projects across 49 colliding names** out of 1,727.
* **A collapsed namespace.** The truncated arm joins its segments with `.` *before* the separator split
  that builds the namespace, so the qualification never reached it: a truncated `internal/errors` landed
  on `go.errors_package` — the converted standard library's own class.

A go-file-free ancestor is therefore a **fallback, never a stop**: the walk records the leaf-relative name
it would have produced and keeps climbing. The fallback applies only when there is genuinely no module
root anywhere above — a GOPATH-style tree, which is the one case it was ever needed for. Because both the
declaring package and every importer derive the name from the same call, the two sides move together.

One consequence worth knowing when re-converting: recovering the full path **renames** every package that
was previously truncated (743 of the 1,727 above), so a re-conversion into an existing output root leaves
the old, wrongly-named `.csproj` files behind as orphans that a solution may still list. Convert into a
fresh output directory, or clear the old one first.

Guarded by `TestProjectNameSurvivesGoFileFreeContainerDir` and
`TestProjectNameFallsBackWhenNoModuleRoot` (the unit invariant, including the fallback arm) and
`TestRecurseGoFileFreeContainerDirsKeepDistinctProjectNames` (the whole `-recurse` conversion: distinct
`.csproj` files, a `.slnx` with no repeated project name, and matching qualification in the importer's
converted code). That last one also exercises `internal` as a **namespace** segment, which only the
recovered path produces — `go.example.com.app.@internal.web.api_package`, keyword-escaped on both the
declaration and reference sides.

## The repository's own `go2cs/` module marker is elided — on BOTH sides, by one rule

The behavioral corpus and the package-test fixtures declare `module go2cs/<Name>`, and that leading
segment is a repository marker rather than a namespace segment: `module go2cs/<Name>` and a bare
`module <Name>` emit the same `namespace go;` + `<Name>_package`, which is the only reason both
spellings can coexist in one tree (618 marked declarations against 46 bare ones). Eliding it is safe
by construction — a fetchable module path's first element must contain a dot, because it is a host
name, and `go2cs` has none, so the marker cannot swallow a real dependency's first segment.

A module path is nonetheless spelled **twice** by every conversion, and the elision has to reach both
spellings. `getProjectName` (`importOperations.go`) is the DECLARATION side: it decides the emitted
`namespace`, the project name, and the `<ProjectReference>` file name derived from it.
`convertImportPathToNamespace` (`visitImportSpec.go`) is the IMPORT side: it decides a
`using <alias> = …` target, a bare `using <namespace>;`, and the `typeof` of an init-forcing hook. The
marker was elided by the first and kept by the second, so every reference to a `go2cs/…` path named a
namespace that nothing emits, and such a package could not be imported at all:

```csharp
// value.cs — the DECLARATION                    // external_test.cs — the IMPORT
namespace go;                                    using harness = go2cs.convertedtestharness_package;
partial class convertedtestharness_package {     using static go2cs.convertedtestharness_package;
```

`error CS0234` ×2, which is why the end-to-end `-tests` fixture at
`src/tests/PackageTests/ConvertedTestHarness` never built from the day it was written — a `module
go2cs/convertedtestharness` whose own external test variant self-imports it. Both sides now route
through one function (`trimGo2CSModulePrefix`), the declaration side being the canonical one because
the committed corpus already rests on it and because the 46 bare declarations exist *only* to dodge
this asymmetry. A behavioral test that holds a nested sub-package is the measured case: under
`module IoLike` the parent imports `IoLike/FsLike`, which has no marker to disagree about, while the
same tree under `module go2cs/IoLike` would declare `go.IoLike.FsLike_package` and import
`go2cs.IoLike.FsLike_package`. Agreeing here retires that constraint rather than entrenching it —
it does not oblige any existing module to be respelled, and none is.

The elision is deliberately confined to a path a `go.mod` DECLARES. A path recovered from a directory
instead (`getImportPackageInfo`'s `build.Import` arm, whose canonical path is `GOROOT/src`- or
`GOPATH`-relative) is left alone: this is a module-path rule, and a directory that merely happens to
sit under a `go2cs` folder is not a module declaring that path.

Guarded by `TestModulePathNamespaceAgreesAcrossSides`
(`src/go2cs/moduleNamespaceAgreement_test.go`), which runs the marked single- and multi-segment cases
plus two controls — a bare module, which must reach the identical class, and an ordinary
`example.com/foo/bar`, which must keep every segment — through **both** derivations and requires one
answer. The guard pins the agreement rather than either side's output, because either side alone can
be self-consistently wrong.

## The emitted project PATH is budgeted — the file name compresses, the identity never does

Recovering the full import path made every project name correct, and on a deep dependency tree it made
them long. The import path is then spelled **twice** on disk — once by the directory, which mirrors it,
and once by the file name — so the deepest package of the conversion above emitted a **242-character**
project path before the user's output root even counted. Visual Studio refused to load it, and the same
solution failed all over again ([issue #35], second report).

Three separate limits are in play, and only one of them is negotiable:

| Limit | Value | Liftable? |
|---|---|---|
| Visual Studio's project loader | 260 characters (`MAX_PATH`) | **No.** Measured on a machine with `LongPathsEnabled=1`: a 259-character `.csproj` loads and builds, a 265-character one fails with *"The project file could not be loaded. Could not find a part of the path"* — naming a file that is demonstrably present and that `dotnet build` reads without complaint. |
| A single filename **component** | 255 characters | **No, ever.** No registry key, `\\?\` prefix or manifest touches it (measured: a 265-character *path* writes fine on that same machine, a 256-character *file name* does not). |
| Total path, for long-path-aware tools | 32,767 with the key, 260 without | Yes — `dotnet build` and Visual Studio's own `MSBuild.exe` both handled a 280-character artifact tree. |

So enabling long paths is not an answer, and neither is flattening the tree:
`pkg/<import-path>/<dotted>.csproj` and `pkg/<dotted>/<dotted>.csproj` measure **identically**, because a
separator costs exactly what a dot costs. The name cannot simply be shortened to the leaf either — that
is what collided in the first report, and `.slnx` has no display-name override to separate the file name
from the project name (measured: a `DisplayName` attribute does not help; Visual Studio still reports
*"Project name 'v3' already exists in the '/pkg/' solution folder"*). Spelling the path twice is the
floor unless one of the two spellings is **compressed**.

**The file name is what compresses; identity never does.** `projectFileBaseName` bounds the emitted
project path — `<tree-root>/<import-path>/<name>.csproj` — at **200 characters**, which leaves an output
root of up to 59 characters inside `MAX_PATH`. A name that fits is returned verbatim, which is every
package in the standard library (longest emitted path: 101) and every behavioral test (109), so the
committed corpus is untouched. A name that does not becomes `head~tail.hash8`: a readable head carrying
the module, a readable tail carrying the leaf package, and eight hex characters of SHA-256 over the full
canonical name. The C# namespace, the `<AssemblyName>` and therefore the NuGet `PackageId` all keep the
**full import path** — those are identity, and they stay globally unique and legible in a stack trace.
Replaying the reporter's real 1,724-package conversion: 1,724 distinct names, **11** compressed, longest
emitted path exactly 200.

The compression is a pure function of the canonical name, so it is **set-independent**, and that is the
point. The reference side derives a dependency's file name from the import path alone, knowing nothing
about the rest of the conversion. A shortest-unique-*suffix* scheme would have been shorter still, but
under it adding one dependency can rename unrelated projects — the same coupling the first report's fix
removed, one level up.

**Derived paths are redirected rather than budgeted.** `obj\`, `bin\` and the generator output
`EmitCompilerGeneratedFiles` writes *multiply* the import path instead of merely repeating it, so no name
budget reaches them: on the reporter's tree, 1,570 of 1,725 projects produced a generator path over
`MAX_PATH` and 43 produced a **file name** over the unliftable 255. A `-recurse` conversion therefore
emits a `Directory.Build.props` and `Directory.Build.targets` at the output root that move all three to
`<root>\.artifacts\{obj,bin,gen}\<token>\`, where the token is 12 hex characters of
`[MSBuild]::StableStringHash` over the project's root-relative directory. Two details are load-bearing:

* .NET's built-in `UseArtifactsOutput` was evaluated first and does **not** fit — it lays artifacts out
  under `$(ArtifactsPath)\obj\$(MSBuildProjectName)\`, and the project name *is* the long thing here.
  Measured at a 13-character output root, the deepest package still landed a 309-character artifact.
* `CompilerGeneratedFilesOutputPath` is set from the **`.targets`**, not the `.props` — the generated
  `.csproj` sets it in the project body, which beats any `.props` value — and with **no trailing
  separator**, because csc receives it as `/generatedfilesout:"<path>"` where a trailing backslash escapes
  the closing quote and the compiler is handed the directory as a file name (CS2021). Keeping this out of
  the csproj template is also what holds the standard library and behavioral corpus at zero movement.

Hint names are capped to 128 characters in the analyzer itself (`Common.GetValidFileName`), compressing to
`<head>-<hash16>.g.cs`. A hint name is only a label — Roslyn requires it to be unique within a generator
and nothing else — so the cap has no semantic consequence and the files it names are git-ignored. The
elision separator must come from the same allow-list `IsValidHintNameChar` enforces: a tilde there reaches
`AddSource` unscrubbed and throws, which surfaces as **CS8785, a warning**, leaving the generator to
contribute nothing and the build to fail on the type it should have emitted.

Guarded by `TestProjectFileBaseNameLeavesCorpusNamesAlone`, `…BoundsTheEmittedPath`,
`…BoundaryIsExact`, `…IsDeterministicAndSetIndependent`, `…KeepsDistinctNamesDistinct`, and
`TestIssue35ReplayCorpus` (which replays a real generated `.slnx` when `GO2CS_ISSUE35_CORPUS` points at
one), plus `TestRecurseDeepImportPathStaysInsideMaxPath` — a whole `-recurse` conversion over the
reporter's actual shape, asserting the compressed file name, the untouched `AssemblyName`, the tree-wide
path bound and the reference side's agreement.

[issue #35]: https://github.com/ritchiecarroll/go2cs/issues/35

## The recurse output root records its runtime root: the `$(go2csPath)` default

**A local-references `-recurse` conversion pins the runtime root it resolved against into the output
root's generated `Directory.Build.props`** ([issue #36]):

```xml
<PropertyGroup Condition="'$(go2csPath)' == ''">
  <go2csPath>C:/Users/<user>/go2cs-runtime/</go2csPath>
</PropertyGroup>
```
<!-- identifier scrubbed 2026-09-26 by security order: the profile path's account segment is now a placeholder -->

The `-go2cspath` command-line flag is a conversion-time input — it is where the converter reads each
imported package's `package_info.cs` from and what it resolves `$(go2csPath)core/…` references
against — but before this pin its value never reached the emitted build files. The MSBuild property
of the same name then had only the csproj template's fallback (`$(USERPROFILE)/go2cs/`, or
`$(SolutionDir)` under Debug), so converting into an isolated output root produced a solution whose
stdlib/runtime/analyzer references could not resolve unless the runtime happened to live at
`~/go2cs` or a deploy-core `Directory.Build.props` sat above the output. Three details:

* When the output root IS the runtime root (the one-positional form), the pin stays relative —
  `$(MSBuildThisFileDirectory)`, the deploy-core form — so the tree can move as a unit. An isolated
  output root pins the absolute resolved root (forward slashes and a trailing separator, per the
  section below); a re-conversion refreshes it.
* The `Condition` keeps it a default: a `go2csPath` environment variable, a `-p:go2csPath` build
  global, or a higher `Directory.Build.props` still wins.
* `-recurse=nuget` deliberately emits **no** pin — it has no `$(go2csPath)` references to resolve,
  and its props defaults `GoStdLibVersion` instead. A foreign `Directory.Build.props` at the output
  root (deploy-core's, or user-authored) is never clobbered, per the generated-file marker rule.

Guarded by the `go2csPath` assertions in `TestRecurseSyntheticModule` (absolute pin, isolated
output root), `TestRecurseBuildFilesRelativePinWhenRootsCoincide` (relative form), and
`TestRecurseNuGetReferences` (no pin under NuGet references).

[issue #36]: https://github.com/ritchiecarroll/go2cs/issues/36

## Path separators in emitted MSBuild files: forward slashes, on every host

**Every path the converter writes into a `.csproj`, `.slnx`, `.pubxml` or `Directory.Build.props` uses `/`, on every host.** There is no per-host emission and no host-conditional spelling: one converted corpus is correct on Windows, Linux and macOS.

```xml
<ProjectReference Include="$(go2csPath)core/fmt/fmt.csproj" />
<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" … />
<OutDir>bin/$(Configuration)/$(TargetFramework)/</OutDir>
<GoValidationProofFile>$(go2csPath)../docs/validation/$(GoStdLibVersion).$(GoBuildNumber)/fmt.md</GoValidationProofFile>
```

MSBuild accepts `/` in every path context on Windows, and normalizes `\` to `/` on Unix (`FileUtilities.MaybeAdjustFilePath`), so *both* spellings build on both hosts — the .NET SDK's own targets depend on the Unix direction. The reason to pick one is that two other consumers are **not** MSBuild:

1. **The converter's own path arithmetic.** The reference used to be composed by hand — replace every `/` with `\`, then `filepath.Join` a backslash-prefixed file name. On Windows `filepath.Clean` folded that back into a well-formed path; on Unix `filepath.Join` treats `\` as an ordinary filename character, so a Linux-hosted conversion emitted the malformed `$(go2csPath)core\fmt/\fmt.csproj` for *every* stdlib reference in *every* project — silent at emission, a restore failure later (F5, [`PLAN-linux-operation.md`](../PLAN-linux-operation.md) §A1.1). The composition is now `emittedProjectReference` (`importOperations.go`): `path.Join` (slash-only, host-independent) over a `filepath.ToSlash`'d directory. `writeProjectFile` and `writeTestProject` additionally `ToSlash` at the emission point, because a sibling reference made relative by `filepath.Rel` arrives OS-native.
2. **The harnesses that READ an emitted reference.** `BehavioralRunner.PreBuildSharedDeps` and `PerformanceRunner` parse `ProjectReference Include="…"` out of the csproj and resolve it with `Path.GetFullPath`, which on Linux does not split on `\` either.

**Consequence to expect when this changes: the sorted reference block can re-order.** References are `sort.Strings`-sorted, and `/` (0x2F) sorts *below* alphanumerics while `\` (0x5C) sorts *above* them, so a pair that differs at a separator boundary swaps. Across the whole 303-project stdlib the flip moved exactly one file's ordering — `net/http`, where `vendor/golang.org/x/net/http2/hpack` had sorted before `vendor/golang.org/x/net/http/httpguts` (`2` < `\`) and now sorts after it (`/` < `2`). Same set, different order; not a content change.

The hand-owned `core` files that are never re-emitted — `golib`, `testing`, `unsafe`, `internal/godebug` and `core/Directory.Build.props` among them — carry the form by hand. (At Go 1.23.12 the list also named `internal/concurrent` and `internal/weak`; their Go 1.24 successors `internal/sync` and `weak` convert ordinary files beside the hand-owned one, so both emit their own `.csproj`.) The one deliberate exception is the shared-project `<Import Project="..\go2cs\go2cs.projitems" Label="Shared" />` in `golib.csproj` and `go2cs-gen.csproj`: that is Visual Studio's own bookkeeping, VS round-trips its exact text, and MSBuild normalizes it on Unix regardless — so it stays backslashed, with a comment saying why.

Guarded by `TestEmbeddedCsprojTemplatesUseForwardSlashesOnly` and `TestValidationPackBlockUsesForwardSlashesOnly` (`csprojTemplate_test.go` — both templates are asserted to contain *no* backslash at all, so a future addition is covered without the guard enumerating it) and by `TestEmittedProjectReferenceIsHostIndependent` / `TestEmittedProjectReferenceForModuleCachePath` (`importOperations_test.go`).

## The validation-proof block follows the OUTPUT LOCATION, not the invocation mode

A converted stdlib package's `.csproj` carries a block that packs its versioned proof sheet as `VALIDATION.md` inside the nupkg, and `.csproj` files are **regenerated on every transpile** — so the block's emission condition is what decides whether a package keeps shipping its proof. The rule is a structural test on where the project file is being WRITTEN:

> emit the block for a `-stdlib` conversion, or for any conversion re-emitting a `.csproj` under the runtime root's `core/` tree.

It was originally scoped to the invocation MODE instead — `-stdlib`, later widened to `-stdlib` or `-tests` — and each narrowing left a door open that silently un-ships a package's proof sheet at the next `push-nuget`. The `-tests` door was the first (the standing "0 8" restore family: every Phase-4 pipeline run stripped the block from the package under test). The **single-package** door was the second and is closed here (2026-08-19): `go2cs <goroot-pkg-dir> <core-pkg-dir>` — the form a lane uses to regenerate ONE corpus package after a converter change — is neither `-stdlib` nor `-tests`, so it stripped the block too. That one is the harder of the two to catch, because only the `.csproj` moves and a lone `.csproj` diff in a reconvert reads as ordinary emission drift rather than as a loss.

Keying on the output location closes both doors at once and cannot be re-opened by adding a mode: `-recurse` writes under its own `src/`/`pkg/` trees, a behavioral fixture writes under `tests/Behavioral/`, and an end-user module writes wherever it was pointed — none of them satisfy "under `<go2csPath>/core/`", so all three keep their historical `.csproj` bytes. Guarded by `TestValidationPackBlockSurvivesTestsRewriteOfCorePackage`, which now pins the single-package form and its outside-`core` negative control alongside the `-tests` pair.

## A GOROOT-vendored reference is named for the package's ON-DISK path

**A standard-library reference takes its project file name from the directory it resolved to, not from the import path as written.** The two are the same string for every package in the standard library except one class — the GOROOT-**vendored** ones, imported as `golang.org/x/…` but existing on disk, and therefore as converted projects, only under `vendor/golang.org/x/…`.

```xml
<!-- crypto/ecdh imports `golang.org/x/crypto/chacha20` -->
<ProjectReference Include="$(go2csPath)core/vendor/golang.org/x/crypto/chacha20/vendor.golang.org.x.crypto.chacha20.csproj" />
```

The directory half was always right — it is rewritten from the resolved source dir — while the file name was composed from the import path, so the emitted reference named a real directory and a file in it that exists nowhere: `…/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj`. Deriving the name from the directory (`stdLibImportPathFromTargetDir`, applied in both stdlib arms of `importOperations.go`) is what makes the two halves agree *structurally* rather than coincidentally: `getProjectName` — the producer, which names the `.csproj` the vendored package actually emits — has always derived it from that same `GOROOT/src`-relative directory.

This is the third derivation in one family, and they must all resolve the vendored spelling or they disagree about which package is being named: the **namespace** (`resolveGorootVendoredPath`, under [Cross-package imports](#cross-package-imports-importing-another-package--assembly)), the **dependency-graph key** (`stdLibConverter`), and now the **project file name**.

Two consequences beyond the file name, because `PackageName` is not only a file name:

* It keys the embedded standard-library metadata, which records the vendored spelling (`##vendor.golang.org.x.crypto.chacha20`), so the unvendored name matched no section at all (asserted directly by the guard). Wherever that record is the source — a dependency with no `package_info.cs` on disk, i.e. a `-recurse=nuget` reference — the package's exported aliases and `GoImplement` records would have come back empty and silently fallen through to the derive-from-declarations path.
* It composes the imported-alias class path. The unvendored form yields `go.golang.org.x.crypto.chacha20_package`, which names a class that exists nowhere — the CS0234 family the namespace arm above exists to prevent. It was latent rather than active only because `loadImportedTypeAliases` dedupes on the *dependency's* `package_info.cs` path, which is the same file for both spellings, so whichever spelling was resolved first won and the other never applied its aliases.

**Where it surfaced, and what it did NOT do.** Only a `-tests` conversion could emit it: production emission resolves the vendored path upstream (`visitImportSpec`), while the test project's dependency list is the raw import set, which carries both spellings — so `crypto/ecdh`'s test project named the package twice, once correctly and once not. It is worth being precise about the damage, because a missing `<ProjectReference>` sounds fatal and is not: MSBuild degrades it to **warning MSB9008** and builds on (measured — the pre-fix `crypto.ecdh.tests.csproj` builds, 0 errors), and here the correct sibling reference supplied the assembly anyway. The real cost was downstream: the stale name was harvested into `go2cs-stdlib.slnx` as a phantom 308th project by the multi-platform merge's solution-recovery path (fixed on the solution side by `TestCollectConvertedProjectsIgnoresTestProjectReferences`; this is the emission half).

Guarded by `TestGorootVendoredReferenceNamesTheVendoredProject` (the vendored spelling, the metadata key, and the leaf package name), `TestStdLibImportPathFromTargetDir` (the recovery, including the non-core-rooted no-match that leaves the caller on the import path) and `TestStdLibReferenceUnchangedForUnvendoredPackage` (the no-op half — the whole corpus bar the `vendor/` tree, which is what makes a zero-movement [CNR](../Glossary.md#cnr) verdict meaningful rather than lucky), all in `importOperations_test.go`.

## An importer spells the package class from the package NAME — the standard library included

**The emitted class is `<packageName>_package`, so an importer's spelling has to come from the Go package name, never from the last segment of the import path.** The two agree for nearly every package, because a Go package is conventionally named for its directory — which is exactly why the places they disagree are so easy to miss.

```go
// crypto/x509/internal/macos/security.go
package macOS                                    // directory `macos`, package `macOS`
```

```csharp
// the declaration side has always followed the package name
namespace go.crypto.x509.@internal;
public static partial class macOS_package { … }

// so the importer must too — crypto/x509/darwin/root_darwin.cs
using macOS = go.crypto.x509.@internal.macOS_package;
```

`convertImportPathToNamespace` substitutes the import graph's authoritative package name for the path's last segment. It used to do that only for **non-stdlib** imports, reasoning that a stdlib package is named for its directory so stdlib references would stay byte-identical. That premise is true for every standard-library package but one, and the exception could not surface until darwin was built at all: `crypto/x509/internal/macos` is darwin-exclusive, so its importers emitted `macos_package` against a declared `macOS_package` for as long as the corpus was Windows-only. C# is case-sensitive, so the result is **CS0234** — and it reads like a missing project reference or an empty assembly, because the symbol genuinely exists nowhere.

Censused across `windows`, `linux` and `darwin`, the standard-library paths whose package name differs from their tail are exactly four:

| Import path | Package | Targets | Disposition |
|---|---|---|---|
| `crypto/x509/internal/macos` | `macOS` | darwin | the one that moves |
| `math/rand/v2` | `rand` | all | already correct via the `/vN` branch |
| `internal/trace/internal/testgen/go122` | `testkit` | all | nothing in the corpus imports it |
| `runtime/internal/wasitest` | `wasi` | all | nothing in the corpus imports it |

So trusting the import graph *everywhere* **keeps** the byte-identity the stdlib exclusion was asserting rather than merely asserting it — and [CNR](../Glossary.md#cnr) is what proves the claim instead of the comment.

The fix restructures rather than special-cases: **when the graph knows a package's name, that name is the class segment; the `/vN` directory convention remains the fallback for when it does not.** A narrower "substitute only when the two differ" test would have looked equivalent and quietly broken the exotic case the convention branch was written for — a package literally *named* `vN`, which would then be rewritten to its parent. Preferring the authoritative name over the convention wherever both are available is what keeps the two rules from fighting.

This is the same family as [the GOROOT-vendored reference](#a-goroot-vendored-reference-is-named-for-the-packages-on-disk-path) above: several independent derivations name one package, and they are correct only when they agree *structurally*. Guarded by `TestImportedPackageClassFollowsPackageName` (the rule, over a stdlib name/directory mismatch, an ordinary stdlib package, a `/vN` directory and a module dependency) and `TestMajorVersionFallbackAppliesWithoutGraphMetadata` (the fallback half), in `packageClassNaming_test.go`.

## Generated output path: `$(OutDir)` defers to `$(BaseOutputPath)`

Both project templates (`src/go2cs/csproj-template.xml` and the `-tests` host's `test-csproj-template.xml`) give the generated project a stable default output path:

```xml
<!-- Build outputs are copied to $(OutDir), so this default must also defer to an explicit
     $(BaseOutputPath); otherwise that redirection silently never takes effect. -->
<PropertyGroup Condition="'$(OutDir)'=='' AND '$(BaseOutputPath)'==''">
  <OutDir>bin/$(Configuration)/$(TargetFramework)/</OutDir>
</PropertyGroup>
```

The **`AND '$(BaseOutputPath)'==''` half is load-bearing** and was added 2026-07-26. MSBuild copies build outputs to `$(OutDir)`, and `$(OutDir)` is what the SDK *derives* from `$(BaseOutputPath)` late in the import order — so a template that pins `OutDir` in the project body outranks any `BaseOutputPath` an outer `Directory.Build.props` sets, and that redirection is discarded without a warning. The generated project would keep writing to `bin\<Config>\<tfm>\` while `OutputPath` reported the redirected location, which is exactly how the defect hides.

Three separate isolation intents were silently defeated by the unconditional pin, all found at once:

- **`-tests` host output** — `test-csproj-template.xml` sets `BaseOutputPath=bin/tests/` (alongside `obj/tests/`) precisely so the test project and the production project that *shares its directory* do not collide. The `obj/` half worked; the `bin/` half never did, so every converted test host was writing into its production package's output tree.
- **The Native AOT perf publishes** — `src/tests/Performance/Directory.Build.props` sets `BaseOutputPath=bin\aot-build\` under its `PerfAot` gate. With the pin, the AOT publish's *build* step wrote through `OutDir` into the JIT tree and overwrote the JIT binary with a self-contained, `IsDynamicCodeSupported=false` one — which the runner then measured and published as the "JIT" column. See `docs/phase4/DESIGN-iface-shell-caching.md` §10.
- **End-user layouts** — any converted project consumed under an artifacts/CI convention that sets `BaseOutputPath` was ignored the same way.

Two guards pin this. `TestCsprojTemplateEmitsWellFormedXml` / `TestTestCsprojTemplateEmitsWellFormedXml` (`src/go2cs/csprojTemplate_test.go`) substitute each template exactly as its emitter does and stream the result through `encoding/xml`, which enforces the same comment rules MSBuild's loader does — a malformed template breaks the entire corpus at compile time and is otherwise only visible ~450 s into a full behavioral run. On the measurement side, `PerformanceRunner` reads the JIT binary's `runtimeconfig.json` before timing anything and **fails the run** if it is self-contained or has dynamic code disabled.

## Per-GOOS sources: layout L3 and `$(GoTargetOS)`

Go selects its platform sources at *build* time — filename suffixes and `//go:build` constraints — so a
conversion does not merely target a platform, it **is** that platform. A converted package whose emission
varies across `GOOS` therefore keeps the varying files in per-GOOS subfolders, and the `.csproj` compiles
exactly one of them; files that are byte-identical on every platform stay flat. This is layout **L3**
([`phase4/DESIGN-multiplatform-corpus.md`](../phase4/DESIGN-multiplatform-corpus.md) §8, accepted 2026-08-08).
`internal/goos` is the first package to carry it:

```
src/core/internal/goos/goos.cs                    shared by windows, linux and darwin
src/core/internal/goos/package_info.cs            shared
src/core/internal/goos/windows/nonunix.cs         public const bool IsUnix = false;
src/core/internal/goos/windows/zgoos_windows.cs   public static readonly @string GOOS = @"windows"u8;
src/core/internal/goos/linux/unix.cs              public const bool IsUnix = true;
src/core/internal/goos/linux/zgoos_linux.cs       public static readonly @string GOOS = @"linux"u8;
src/core/internal/goos/darwin/…
```

Such a package's `.csproj` gains exactly two blocks — the selector, and the include that reads it:

```xml
<PropertyGroup Condition="'$(GoTargetOS)'==''">
  <GoTargetOS>windows</GoTargetOS>
</PropertyGroup>
…
  <Compile Remove="**/*.cs" />
  <Compile Include="*.cs" />
  <Compile Include="$(GoTargetOS)/*.cs" />
```

The include must follow `<Compile Remove="**/*.cs" />`, which would otherwise remove it — MSBuild evaluates
items in document order. The property may sit anywhere (properties are evaluated in an earlier pass) but is
declared above the item that reads it. `windows` is the default because the corpus that exists today is the
Windows emission, so a plain `dotnet build` reproduces the single-platform package this layout replaced:
verified byte-identical, and `-p:GoTargetOS=windows` produces the same assembly as the property absent.

**Only a package that varies gets the blocks.** The design measures the varying set at 37 of 304; the rest
emit the same C# on every platform and keep exactly the project file they always had.

**Which files are per-GOOS cannot be decided by one conversion.** The platform axis is a comparison of
several targets' *emissions*, and it is emphatically not derivable from Go file sets: four packages emit
different C# from identical Go source (constant folding, escape analysis, cross-file collision renaming,
dead-branch folding), and three emit identical C# from differing Go source. So the layout is produced by a
multi-target run — `go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64` — which
converts once per target into a seeded staging root, classifies every emitted artifact (shared / variant /
partial / exclusive), and merges the result into one tree.

A **single**-target conversion instead *honors* a layout the output tree already carries: if the package
directory holds `<goos>/<name>.cs`, that is where `<name>.cs` is written, and a package directory holding
any per-GOOS source folder gets the two blocks. That is what makes an ordinary seeded reconvert reproduce
an L3 package file for file, instead of laying a flat duplicate beside the copy the project is already
compiling — a duplicate-member break that would otherwise arrive silently. A per-GOOS folder is
distinguished from a nested package by the project file every converted package directory holds and a
source folder never does: `internal/syscall/windows` is a real package whose own directory name is a GOOS.

Guarded by `platformLayout_test.go` (`src/go2cs`), that negative case included.

**A HAND-OWNED file has a platform too, and the classifier cannot see it.** L3 decides placement by
comparing *emissions* — and a hand-owned file is never emitted, so it is never classified, so it silently
keeps whatever placement it had while the file it belongs to moves per-GOOS. That is invisible on Windows,
where the compile item set is identical either way, and it took the entire Linux corpus down at increment 3:
`runtime/lock_sema_impl.cs` supplements `lock_sema.cs`, which Go selects on Windows *and* macOS but never on
Linux, so the Linux build compiled the flat companion against a principal that was not in its build.

The rule is stated as a platform **set**, not as a folder: *a hand-owned file belongs in exactly the platform
builds its principal takes part in*, after which L3's ordinary placement rule applies unchanged — every
platform ⟹ flat, a subset ⟹ one copy per platform in the subset. Read literally as "inherit the principal's
folder", `os/proc_impl.cs` and `syscall/syscall_impl.cs` would be triplicated: their principals are per-GOOS
*variants* present on all three platforms, so three copies of one hand-written file would have to be
maintained in lockstep for no compile benefit. L3 duplicates only what cannot be shared.

A principal comes in two shapes, and both are already recorded in the tree by the emission itself:

| Hand-own | Principal | Why |
|:--|:--|:--|
| `<name>_impl.cs` | `<pkg>/<name>.cs` | a companion with no Go counterpart of its own; it *supplements* the converted file |
| `<name>.cs` carrying `[module: GoManualConversion]` | `<pkg>/<name>.cs.auto` | the conversion still runs and only its EMISSION diverts, to the review sibling — which is therefore emitted by exactly the platforms that compile the Go file this hand-own replaces |

The second binding is the one that catches a whole-file hand-own of a platform-exclusive Go file:
`syscall/dll_windows.cs` and `syscall/exec_windows.cs` replace Windows-**only** sources and now live in
`syscall/windows/`. The `.cs.auto` sibling moves with its `.cs` so the pair cannot separate — which is also
what a single-target reconvert already does, since `conversionDriver` routes both through
`platformLayoutPath`. A hand-own whose principal no target emitted (`runtime/managed_impl.cs`,
`internal/poll/runtime_sema_impl.cs` — go2cs machinery with no Go file behind it) has no placement evidence
and is left alone. Two existing copies of one hand-own that disagree are an **error**, never a first-wins
choice: a duplicated hand-own is hand-maintained in each folder, so propagating one over the other is how a
fix applied to a single flavor would disappear.

⚠ **A hand-owned FUNCTION is a different problem with no layout answer.** `manualConversionFuncs` is keyed by
NAME and is platform-blind, so an entry turns its Go declaration into a placeholder on *every* platform while
the implementation exists only where one was written — and `notetsleep_internal` is even four arguments in
`lock_sema.go` against two in `lock_futex.go`. Census and remedy in
[`phase4/DESIGN-multiplatform-corpus.md`](../phase4/DESIGN-multiplatform-corpus.md) §7.

Guarded by `platformHandOwn_test.go`, which walks the **real** `src/core` rather than a synthetic tree —
the next offender will be a file somebody adds by hand. Three structural rules: an `*_impl.cs` whose
principal is in some but not all of its package's per-GOOS folders must be in exactly those; a `.cs.auto`
lives beside the `.cs` it reviews; and a source carrying Go's own GOOS filename constraint (`*_windows.cs`,
`*_linux.cs`, `*_darwin.cs`, with an `_impl` suffix stripped first) is never flat in an L3 package. That
third rule exists because a static walk cannot find a *marked* hand-own's principal — it is what would have
caught `dll_windows.cs`/`exec_windows.cs` a whole increment earlier.

**References are conditioned by the same rule, one level up.** A package's *direct imports* can differ by
`GOOS` too — measured at **21** packages, `os` being the clearest: it imports `internal/syscall/windows` on
Windows and `internal/syscall/unix` on Linux and macOS. One flat reference list cannot say that, so the
references common to every platform stay unconditioned and only the differences are selected:

```xml
  <ItemGroup>
    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
    …the 16 references os has on every platform…
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='darwin'">
    <ProjectReference Include="$(go2csPath)core/internal/syscall/unix/internal.syscall.unix.csproj" />
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='windows'">
    <ProjectReference Include="$(go2csPath)core/internal/godebug/internal.godebug.csproj" />
    <ProjectReference Include="$(go2csPath)core/internal/syscall/windows/internal.syscall.windows.csproj" />
  </ItemGroup>
```

**A platform whose delta is empty still gets a group**, written self-closing
(`<ItemGroup Condition="'$(GoTargetOS)'=='linux'" />`). That is not noise, and it is the one detail the whole
mechanism rests on: the shared list is an *intersection*, and a single-target reconvert recovers the other
platforms' sets from the file it is about to overwrite. Forget that linux takes part and the next reconvert
computes the intersection over two platforms instead of three — promoting a Windows-only reference into the
shared list, where it would land in the Linux build. The empty group records membership so the axis survives.

Both producers — the multi-target merge and a single-target reconvert — go through one renderer, which is
what makes the reconvert reproduce the merge's bytes rather than something merely equivalent. When every
platform's imports agree again the block disappears and the project file returns to its plain form.

Two facts that are NOT references still have to be reconciled, because one `.csproj` serves every platform:
`<AllowUnsafeBlocks>` is emitted as the **union** across targets (it differs in `os/user` and `syscall`; the
property grants a capability rather than using one, so raising it is inert where unused), and every other
companion artifact — README, icons, `.cs.auto` — is taken from the **first target in `-platforms` order**,
the reference flavor, so the choice is deterministic instead of depending on which target happened to have
something to rewrite.

Guarded by `platformProject_test.go`, whose central test is the invariant itself: a single-target reconvert
of every platform must reproduce the merged project file byte for byte.

## Build-warning suppression: what the emitted `.csproj` silences, and what it deliberately does not

Both templates carry one suppression policy, and it is a policy rather than an accretion: every entry is a
diagnostic that is **structural to the Go emission model**, and every diagnostic that is *not* is left
visible on purpose. The full census, code by code, is [`docs/phase4/DESIGN-warning-suppression.md`](../phase4/DESIGN-warning-suppression.md).

```xml
<Nullable>annotations</Nullable>
<NoWarn>CS0162;CS0164;CS0282;CS0660;CS0661;CS1717;CS1718;CS8618;CS8860;CS8974;CS8981;IDE0060;IDE1006;CA2255</NoWarn>
```

The `NoWarn` entries each name an emission the converter re-creates on the very next conversion: Go type
names are lower-cased ASCII (`CS8981`, and `CS8860` for a Go type literally named `record`); a struct's
fields are split between its type file and `package_info.cs` (`CS0282`); Go comparison operators are emitted
on value types without `Equals`/`GetHashCode` (`CS0660`/`CS0661`); Go zero values leave non-nullable fields
uninitialized (`CS8618`); `if (raceenabled)` bodies and the `break;` appended after a `case` that already
`throw panic(…)`s are unreachable (`CS0162`), as are the synthetic `continue_<label>:`/`break_<label>:`
pairs emitted for every labeled Go statement (`CS0164`); the named-return store on the `goto ᒐdone` path
through a defer frame is a self-assignment (`CS1717`); `return f != f;` is Go's NaN idiom verbatim
(`CS1718`); a function value in a `map[string]any` looks like a forgotten call (`CS8974`); and Go `init()`
is modeled as a `[ModuleInitializer]`, which the library-hygiene analyzer objects to by design (`CA2255`).
`IDE0060`/`IDE1006` never fire at the command line — they exist for Visual Studio's live analysis, where
every `_`-shaped parameter and every Go identifier would otherwise be flagged.

**`Nullable` is `annotations`, not `enable`.** Go has no non-nullable pointer, interface, map, slice,
channel or func — every one of them is nil-able by construction — so C#'s nullable *flow analysis* is
asking a question the source language cannot pose, and the only way to satisfy it would be to annotate the
whole emitted corpus `?`, burying the Go shape the project exists to preserve. Nor is it protecting a
semantic go2cs wants: a converted program that dereferences a nil Go value *should* fault, because that is
Go's nil-pointer panic. `annotations` keeps `?` meaningful (golib's `ж<T>?`, `PanicException?`) and keeps
`default!` legal while turning the analysis off; `disable` would be wrong, because it makes every `?` in
the emitted code a fresh `CS8632`. One consequence is load-bearing: `CS8618` stays in `NoWarn` even under
`annotations`, because `go2cs-gen` emits `#nullable enable` at the top of each generated `.g.cs` and a
file-level directive beats the project property.

**The publish properties are scoped off `Library`.** `PublishReadyToRun`/`PublishTrimmed`/
`IncludeNativeLibrariesForSelfExtract`/`EnableCompressionInSingleFile` sit under
`Condition="'$(OutputType)'!='Library'"`, because the SDK turns `PublishTrimmed` into
`EnableTrimAnalyzer=true` **at build time** — so on a library, which is never published, that one line was
the sole source of every `IL####` warning in the corpus while buying nothing. A converted `main` package
still gets the analysis, and the trimmer re-runs over the whole closure at app publish, where it is
actionable. `AllowUnsafeBlocks` shares that group's history but **not** its condition: it is a compile
setting, and the converted stdlib is full of library packages that do not compile without it.

Codes deliberately left **visible** are the other half of the policy — `CS0219` (dead named-return locals,
the one static signal for a genuinely dropped assignment to a named result), `CS8778` (a live 32-bit
truncation in `nint`-typed `int64` constants), `CS0675`, `CS8500` (the managed-referent hazard the S1 fork
ruling is about — golib suppresses it locally, the corpus must not inherit it), `CS8826`, `CS0252`,
`CS0649`, `CS1522`. Each is a converter or golib defect wearing a warning's clothes; suppressing them would
delete the signal rather than fix the emission.

Hand-owned `.csproj` carry the policy by hand rather than by emission — `core/unsafe` and
`core/testing` (skip-listed packages), `core/internal/godebug` (whose only Go file is fully hand-owned,
so `unmarkedFileCount == 0` makes the driver `continue` before `writeProjectFile`), and `core/golib`; at
Go 1.23.12 `core/internal/weak` and `core/internal/concurrent` were two more, and their Go 1.24
successors `weak` and `internal/sync` emit their own. golib keeps a *shorter*, deliberately
different list: it is hand-written, it is the reflection/unsafe core, and its trim and nullable warnings
are a real to-do list with an owner rather than emission noise.

Three guards pin all of this in `src/go2cs/csprojTemplate_test.go`:
`TestBothCsprojTemplatesCarryTheSameSuppressionPolicy` asserts the two templates agree on `Nullable` and on
the `NoWarn` set *exactly* (so adding a code forces the design-doc update, and dropping one fails), and
`TestPublishPropertiesAreScopedOffLibrariesButAllowUnsafeBlocksIsNot` parses the rendered project and pins
both halves of the publish-group split — a regression that moved `AllowUnsafeBlocks` under the condition
would break the corpus in a way no warning count would reveal.

## The README has TWO emission points, because its Tests badge reads a page the run writes LAST

A converted package's README carries the four-badge line, and the Tests badge is composed at CONVERSION
time from `docs/validation/current/<dot-id>.md`. The run that WRITES that page is the compare at the END
of a `-tests` pipeline (`emitValidationProofPage`). Those two facts are an ordering, and the ordering is
the whole problem: within a single `-test-action all`, the README is always built from the proof page as
it stood BEFORE the run. A package whose counts are unchanged is fine; a package whose counts CHANGE —
every fresh bank, and every rebank that moves a number — emitted one run behind, so a fresh bank shipped
a README reading `Tests-not_yet_validated-orange` **beside its own green proof page**, and a bank owed one
extra conversion of its own package as paperwork.

Two formulations were on the table and they are **not** equivalent. Widening the emission GATE (which
package gets a README at all) is what `emitsPackageReadme` does, and it closes the unchanged-counts case;
it cannot reach an ordering. Closing the ordering needs a SECOND emission point:
`refreshPackageReadmeAfterProof` re-emits the README immediately after the compare writes the page.

**The hazard, and why the refresh is sourced from a record.** `-test-action build|run|compare` do not
convert: the converter's package globals (`packageDoc`, `packageSourceDir`) are empty on those paths. A
refresh that re-read them would render a DOC-LESS README over a corpus package — a destructive,
corpus-wide rewrite that reads as ordinary reconvert drift. So the refresh does not read them. The
conversion-time write records what it composed the README from (`packageReadmeEmission`, `readme.go`),
and the refresh runs from that record or not at all: **no conversion, no record, no write.** The hazard is
unrepresentable rather than avoided, and the record is taken INSIDE the `emitsPackageReadme` gate, so the
one decision about whether a package gets a README also decides whether one can be refreshed.

`writeReadmeFile` is idempotent (`needToWriteFile`), so a package whose counts did not move rewrites
nothing and a sweep stays byte-clean — this opens no standing-restore family. The corollary is worth
carrying: **a `README.md` moving during an operational sweep now means that package's validation counts
moved.** That is a finding to read, not dirt to classify.

Measured on `hash/adler32` with its proof page moved aside to simulate a fresh bank: one
`-test-action all` validates 2 of 2, writes the page, and emits the green `2/2` badge **in the same run**
— byte-identical to the committed README. A `-test-action compare` over a deliberately staled README leaves it
byte-for-byte untouched, and creates none where the conversion wrote none. (Guarded by
`TestPackageReadmeRefreshFollowsInProcessConversionNotRunMode`, the sibling of
`TestPackageReadmeEmissionFollowsPackageProvenanceNotRunMode` that pins the gate half.)

## Assembly metadata is one derivation chain, and the framework is hoisted to props

Every project in the tree — the two converter templates and the hand-written `golib` and `go2cs-gen` —
carries the same metadata block, in the same order, derived from the same two roots:

```xml
<Product>go2cs</Product>
<Description>$(AssemblyName) ($(TargetFramework) - $(Configuration))</Description>
<AssemblyTitle>$(Description)</AssemblyTitle>
<Authors>$(Product) Authors</Authors>
<Company>The $(Authors)</Company>
<Copyright>Copyright © 2018-2026 $(Company)</Copyright>
<RepositoryUrl>https://github.com/ritchiecarroll/go2cs</RepositoryUrl>
<RepositoryType>git</RepositoryType>
<ApplicationIcon>go2cs.ico</ApplicationIcon>
```

The order *is* the contract: the block reads top-down as a chain, so `Product` names the project, the two
description properties fall out of it, `Authors` falls out of `Product`, `Company` out of `Authors`, and
`Copyright` out of `Company`. There is exactly one place to edit a name, and no way for two projects to
disagree about one. A literal that happens to expand to the same string is still a defect — it is the copy
that goes stale.

The block had drifted in three directions at once before the guards existed. The two hand projects spelled
`Company` and `Copyright` as literals while the template spelled the year with a **printf verb** over
`time.Now().Year()`; the test-host template omitted `Authors` and `Copyright` entirely; and
`go2cs-gen.csproj` carried a **second, empty `<Description>`** below its real one — MSBuild keeps the last,
so the published `go.gen` package shipped with an empty description *and* an empty `AssemblyTitle`, with
nothing warning and nothing failing to build.

The `Copyright` year is now a **literal range**, not a verb. The verb made every emitted `.csproj` a
function of the wall clock: the same converter over the same sources with the same flags produced different
bytes on either side of New Year's Eve, so the first regeneration of each year reported the whole corpus as
drifted. Determinism is worth more here than an automatically-current year, which is a once-a-year edit.

`<TargetFramework>` is owned by **`src/Directory.Build.props`**, so a framework hop is one edit rather than
one per csproj family plus a whole-corpus regeneration. It survives in each project only as a **conditioned
fallback**:

```xml
<TargetFramework Condition="'$(TargetFramework)'==''">net10.0</TargetFramework>
```

Both halves are load-bearing. `Directory.Build.props` is imported *above* the project body, so where the
file is in scope it wins and the project's own line is inert; where it is not, the project still names a
framework and still builds. And it is genuinely not always in scope: `deploy-core.ps1` stages the corpus
under a root that **deliberately excludes** `core`'s props and writes its own, a `-recurse` conversion
writes generated code under an arbitrary output root, and a single-package conversion can land anywhere.
Unconditional in the project would mean the hop silently skips every emitted project; absent entirely would
mean those trees do not build at all. `go2cs-gen` keeps its `netstandard2.0` **unconditional** on purpose —
a Roslyn analyzer must not follow the hop — and its own value therefore wins over the props default.

MSBuild stops at the **first** `Directory.Build.props` found walking up, so the two nested ones
(`src/core`, `src/tests/Performance`) explicitly import the root via `GetPathOfFileAbove`. Any new nested
props file owes the same import or it silently shadows the root for everything beneath it.

Guarded by `csprojMetadata_test.go`, which states the contract over all four projects at once — the two
rendered templates and the two hand-written files read from disk — pinning the order as a subsequence, the
derived values exactly, the absence of a format verb, the conditioned framework, and (the regression guard
for the shipped defect) that no metadata property is set twice unconditionally.

## Cross-package imports (importing another package / assembly)

When a package imports another and uses its exported surface, the converter must agree, on both the **producer** side (converting the imported package) and the **consumer** side (resolving the `import`), on the imported package's C# `(namespace, class)` and emit a `ProjectReference` to its generated `.csproj`. The package class is `<packageName>_package` and the namespace is the root `go` plus the import path's leading segments, so the two sides line up when the Go package name equals the import path's last segment (the usual layout: `import "x/y/barlib"` → package `barlib` → `go.x.y.barlib_package`). The consumer emits `using barlib = …barlib_package;` and references members as `barlib.Thing`.

Resolving *where* an imported package lives is **module-aware**: the standard library is found under `GOROOT` and mapped to `$(go2csPath)core/<pkg>`, but a **local/user module** (reached via a `go.mod` `replace`, or simply co-located in the same module tree) is invisible to the legacy `go/build` GOPATH resolver. The converter therefore falls back to the dependency directory captured from the module-aware `go/packages` load, treats that package's converted output as **in-place** (co-located with its Go source), and emits a `ProjectReference` **relative to the referencing project** (e.g. `../barlib/barlib.csproj`) so the generated `.csproj` is portable. This is what makes "import a sibling package and use it" compile as separate assemblies.

**Exported type aliases cross packages.** A package-level Go type alias that is exported — `type Temperature = Celsius` — is recorded in that package's `package_info.cs` as an assembly attribute in its `<ExportedTypeAliases>` block:

```csharp
// <ExportedTypeAliases>
[assembly: GoTypeAlias("Temperature", "go.CrossPkgLib_package.Celsius")]
// </ExportedTypeAliases>
```

A consumer that imports the package and names `CrossPkgLib.Temperature` cannot use a C# member-access for it (C# has no namespace-level type alias). Instead, the converter parses the imported package's `package_info.cs`, reads its `[GoTypeAlias]` attributes, and emits a corresponding `global using` into the consumer's own `<ImportedTypeAliases>` block — keyed by a package-qualified name whose `.` separator is the extended-Unicode dot `ꓸ` (`ꓸ`, a valid C# identifier character), since `CrossPkgLib.Temperature` is not a legal C# identifier:

```csharp
// <ImportedTypeAliases>
global using CrossPkgLibꓸTemperature = go.CrossPkgLib_package.Celsius;
// </ImportedTypeAliases>
```

The consumer's converted code then refers to the alias as `CrossPkgLibꓸTemperature`. (This round-trip depends on the module-aware resolution above to locate the imported package's `package_info.cs`; a stdlib dependency is found under the `core` output tree, a local module via its `go/packages` directory.) Guarded by the `CrossPkgLib`/`CrossPkgUser` cross-package behavioral test pair.

**Imported stdlib alias metadata loads from the tree the assembly is COMPILED from — which since 2026-08-01 is the only tree there is.** *(Resolved; kept because the failure mode is instructive.)* The alias round-trip locates the imported package's `package_info.cs` under the `core` output tree. While the converted standard library lived in a SECOND tree, a `-tests` build compiled its stdlib dependencies from `go-src-converted` while `loadImportedTypeAliases` still read `package_info.cs` from the baseline `core` stub — and most stubs had **no** `package_info.cs` at all (`runtime` was impl-stubs only), so the alias map came back empty and a test's cross-package reference to a collision-renamed stdlib type rendered the RAW, undefined qualified name: `err.(runtime.Error)` → `runtime.Error` (CS0426) instead of `runtimeꓸError` → `runtime_package.ΔError` (math/bits' `Div` overflow/divide-zero panic asserts). The fix at the time derived the alias-load directory from the project-reference remap itself, so the two authorities could not drift. Both the remap and that derivation are now **deleted**: the converted stdlib lives at `$(go2csPath)core/<pkg>`, exactly where every resolver already pointed, so the alias-load path and the compile path are the same path by construction. The lesson outlives the machinery — *metadata must be read from the tree the code is compiled from, never from a parallel one that merely looks like it.*

## A cross-package type reference emits its `using <alias> = <namespace>;`

**A cross-package type reference emits its `using <alias> = <namespace>;` even when the file did not import the package under a usable name.** A foreign type renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type, `@unsafe.Pointer` for the `unsafe.Pointer` basic — which resolves only through a file-local alias (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias is normally generated from a *canonical* (unaliased) `import`, but a file can reference a foreign type with no such import through three routes: **type inference** — a *same-package* function returns a foreign type, so the caller infers a local of that type but never writes `pkg.` and need not import the package (runtime `preempt.go`: `fd := funcdata(f, i)`, where `funcdata` returns `unsafe.Pointer`); a **blank import** (`_ "pkg"`, side-effects-only — **no `using` is emitted for it at all**: the old `using _ = <ns>;` emission hijacked C#'s `_` DISCARD for the whole file, so a deconstruction discard (`(w, _) = w.ensure(…)`, runtime `tracetime.go`) bound the namespace alias instead (CS0118 + CS0029); the import is recorded as a comment, and a genuine type reference still gets its canonical alias from this machinery — e.g. `symtabinl.go`'s `_ "unsafe"` for `//go:linkname`); or an **aliased import** (`import u "unsafe"`, whose alias `u` differs from the canonical `pkg.Name()` prefix the type reference uses). All previously yielded CS0246. The converter now walks every emitted type (`collectTypePackages`, called from `getAliasQualifiedTypeName` — named types by `pkg.Path()`, an `unsafe.Pointer` basic by the pseudo-path `"unsafe"`, recursing through pointer/slice/array/map/chan/generic/func-signature so a `[]time.Duration` element registers too) and, at file close (`visitFile`), supplies the canonical `using <alias> = <namespace>;` for every referenced foreign package the file did not already import canonically. It is idempotent-safe — a canonical import records its path in `canonicalAliasImported`, so `visitFile` never re-emits (duplicates) it — and a non-canonical alias (`using u = unsafe_package;`) coexists with the added canonical one without conflict. It is also **collision-guarded**: the synthesized `using <alias> = <namespace>;` is skipped when its canonical `<alias>` was already bound to a *different* namespace by a real import — cryptobyte's `asn1.go` imports both `encoding_asn1 "encoding/asn1"` (referenced by type, so it reaches this loop) and the subpackage `.../cryptobyte/asn1` (unaliased → alias `asn1`), so synthesizing `using asn1 = encoding.asn1_package` would duplicate the subpackage's `using asn1` (CS1537). The real imports' emitted aliases are tracked per file (`importAliasesEmitted`); the parent stays reachable through its `encoding_asn1` alias, so skipping the canonical one is safe (a non-colliding canonical alias is still supplied — no churn). (The *separate* defect that the type reference itself renders `asn1.ObjectIdentifier` rather than the file's `encoding_asn1.ObjectIdentifier` — `getAliasQualifiedTypeName` uses the canonical alias, not the file's non-canonical one — is tracked independently.) This is the *type-reference* analog of the method-call `addMethodPackageNamespaceUsing`. (Guarded by `UnsafePointerInferredNoImport` — the `unsafe.Pointer` basic arm, scalar/composite/blank-import variants — and `InferredForeignTypeNoImport` — the generic named arm, an inferred `*strings.Reader` in an `fmt`-only consumer.)

**That supplied alias must carry the collision rename.** When the referenced package's using alias is `Δ`-renamed because a same-named CHILD namespace is visible from the import closure (`go.sync`, contributed by `sync/atomic`; `go.unicode`, by `unicode/utf8` — the same CS0576 collision that renames a *canonical* import's alias, above), `getAliasedTypeName` already renders the short-form type reference through the renamed qualifier (`Δsync.Mutex`, `Δunicode.Range16`). The `visitFile` supply loop, however, composed the alias from `packageUsingAlias` alone — the bare, unrenamed name — so it emitted `using sync = sync_package;` (or `using unicode = unicode_package;`) while the reference read `Δsync.Mutex`: the alias binds nothing (CS0246), and the bare alias would itself collide with the child namespace (CS0576). The supplied alias is now routed through `importQualifier` (`getSanitizedImport(importQualifier(alias))`, the same rename every canonical import applies), so the emitted `using Δsync = sync_package;` matches the reference. `importQualifier` is a no-op for any package whose alias is not renamed, so a non-colliding supplied alias stays byte-identical. The trigger is a file that reaches a renamed package's type through the supply route rather than a canonical import — overwhelmingly a **dot import** (`. "sync"` / `. "unicode"`), where the dot brings names in via `using static` yet the converter still qualifies the type, and no canonical `using <pkg> = …` is emitted to carry the rename. Production stdlib code essentially never dot-imports, so the defect stayed latent as an *unused* supplied alias (reflect's `value.go`/`makefunc.go` inferred `sync` without importing it — the `using sync` alias was never referenced, so the wrong spelling compiled); it surfaces in an EXTERNAL (`_test`) variant that dot-imports the package under test, which `unicode`'s `letter_test.go` does (`. "unicode"` + qualified `Range16`/`RangeTable`/`CaseRange`). Because every currently-compiling site had the alias *unused*, the change only ever flips an unused alias (compile-neutral) or fixes a broken one — no site that used `Δpkg.` while getting the bare supplied alias could have compiled. (Guarded by the `DotImportRenamedPackage` behavioral test — `. "sync"` with a `*Mutex` type reference forcing the qualified `Δsync.Mutex` position, output-compared vs Go; neutering the fix reproduces the reported `CS0246: 'Δsync' could not be found`.)

## System-colliding local type names are root-qualified in assembly attributes
A Go package can name one of its own exported types after a top-level C# `System` type — internal/profile's `ValueType`, go/ast's `Object`, bytes' `Buffer`. The `GoImplement`/`GoImplicitConv` assembly attributes generated in `package_info.cs` sit at **file scope**, before the `namespace` line, where both `using System;` (a csproj global using) and `using static go.<pkg>_package;` are active — so a bare `ValueType` is ambiguous between `System.ValueType` and the package type (CS0104). The emitter root-qualifies any bare, dotless type name matching a curated set of `System` top-level names at the package class:

```csharp
[assembly: GoImplement<go.@internal.profile_package.ValueType, message>]
[assembly: GoImplicitConv<go.@internal.profile_package.ValueType, ж<go.@internal.profile_package.ValueType>>(Indirect = true)]
```

Foreign types are always package-qualified already (dotted) and are left untouched; no non-colliding name changes, so every non-colliding attribute emits byte-identically. (Guarded by the `SystemCollidingTypeName` behavioral test.)

## Exported structs and interfaces cross packages

**Exported structs and interfaces cross packages.** An exported struct's fields and methods are reachable on the consumer side exactly as the producer emits them — `CrossPkgLib.Sensor{Name: …, Temp: …}` lowers to a C# constructor call and `s.Name` / `s.Hot()` to field/method access on the imported struct — because the struct and its `[GoRecv]` extension methods live in the (referenced) library assembly.

A cross-package **interface satisfaction** is subtler. Go is structurally typed, so a consumer may assign any value with the right method set to an interface; C# requires the *nominal* `partial struct T : I` implementation glue, which the [`ImplementGenerator`](source-generators.md#source-generators) can only add to `T` **in T's own assembly** (`isLocalImplType`). The converter records a `[assembly: GoImplement<T, I>]` for each concrete→interface conversion it *witnesses while converting T's package* — so for a consumer to use `Sensor` as `Labeled` across the assembly boundary, the satisfaction must be witnessed in the **library** that declares `Sensor`. The idiomatic Go interface-satisfaction assertion does exactly this:

```go
var _ Labeled = Sensor{}   // in CrossPkgLib — records GoImplement<Sensor, Labeled> in this assembly
```

With that, the library emits `[assembly: GoImplement<Sensor, Labeled>]`, `Sensor : Labeled` is realized in the library assembly, and a consumer's `var l CrossPkgLib.Labeled = s` / `CrossPkgLib.Describe(s)` compile as ordinary upcasts. (A library that returns the interface from a constructor — `func New(...) Labeled { return Sensor{…} }` — witnesses it the same way.) A type that satisfies an interface but is *never* used as it within its own package gets no nominal glue — proactively recording every local concrete→local interface structural match WAS tried (a declaration-site scan, 2026-07) and was RETIRED on 2026-07-25, because it could never be complete (it cannot see a dynamic type in a later-converted assembly) and paid for its incompleteness in speculative glue. Such a satisfaction is resolved at run time by the interface's duck-typing shells instead; a *declared* conversion still takes the nominal fast path. Also guarded by the `CrossPkgLib`/`CrossPkgUser` pair (Phase 3: struct field access + interface satisfaction).

## A DOTTED build tag is matched against the host toolchain's tool tags

**A DOTTED build tag (`goexperiment.X`, `amd64.vN`) is matched against the host toolchain's tool tags.** The converter re-checks each file's `//go:build` constraint after `go/packages` has already loaded it (to drop files for the wrong GOOS/GOARCH when converting cross-platform). Its evaluator only handled bare identifiers (`linux`, `amd64`), so a *dotted* tag parsed as a selector and fell through to `false`. That is wrong for an experiment enabled BY DEFAULT: `coverageredesign`, `regabiwrappers`, and `regabiargs` are in the host's `go/build` `ToolTags`, so `go/packages` loaded their `//go:build goexperiment.X` `_on.go` files — but the re-check then re-EXCLUDED them (the selector → `false`), dropping the package-level consts (`testing`'s `goexperiment.CoverageRedesign`, CS0117 ×4). The evaluator now resolves a dotted tag by membership in `build.Default.ToolTags` — so an enabled experiment's `_on.go` survives and a disabled one's `!goexperiment.X` `_off.go` survives, exactly the one `go/packages` chose. Blast radius is only `internal/goexperiment` (the sole stdlib package whose file selection flipped). **Guard owed** — the fix depends on the host toolchain's active tool tags, which the `go2cs/*` behavioral harness cannot express portably; validated by the reconvert A/B (only `internal/goexperiment` changes) and the [census](../Glossary.md#census) (the consts appear, `testing`'s CS0117 clear). (That tag lookup now lives in the single `matchTag` callback described next; it was a `*ast.SelectorExpr` case while the converter still parsed constraints itself.)

## Build constraints are parsed and evaluated by `go/build/constraint`

The re-check above needs a constraint parser, and for a long time the converter hand-rolled one: it lowercased the expression text and handed it to `parser.ParseExpr`, then walked the resulting `ast.Expr` itself. That parser is for **Go expressions**, and a build constraint is not one. A **Go release tag** is the case that breaks it — `go1.21` reads as the identifier `go1` followed by an illegal `.21` selector, so *every* constraint mentioning a release failed with `failed to parse build constraint: 1:4: expected 'EOF', found .21`. `conversionDriver` warns on that error and falls through to **including** the file, so the failure hid behind an accidentally-correct outcome for as long as the only constraints that hit it were bare `go1.N` gates (five `go-logr` files, surfaced by a `-recurse` probe of `go.opentelemetry.io/otel`). It stops being correct the moment a constraint mixes a release tag with a platform: `//go:build go1.21 && windows` converted for linux lost its platform half along with the rest of the expression.

Constraints are now parsed and evaluated by **`go/build/constraint`**, the package the toolchain itself uses, which retires the whole custom parse/eval layer rather than special-casing the dot:

* **Recognition** — `constraint.IsGoBuild` / `constraint.IsPlusBuild` decide what is a constraint line. Both require the comment at **column zero**, and only the file **header** is scanned (everything above the package clause, line comments only). The regex that preceded them matched any `//go:`-prefixed line *anywhere in the file*, so a `//go:build` quoted in documentation below the package clause gated the file it was describing.
* **Precedence** — a `//go:build` line wins outright; the legacy `// +build` lines apply only in its absence, ANDed across lines, with `,` meaning AND and a space meaning OR inside one. The old regex could not see `// +build` at all (no `//go:` prefix), so a legacy-only file — the norm in third-party modules predating Go 1.17, exactly what `-recurse` meets — converted as **unconstrained**.
* **Evaluation** — `Expr.Eval` drives one `matchTag` callback, so the boolean structure (`&&`, `||`, `!`, parentheses) is the stdlib's problem and go2cs owns only "is this one tag satisfied". Tags resolve in three layers: the `allowedPlatforms` map (GOOS, GOARCH, the derived `unix`/`posix`, the compiler tags, and every `-tags` value), then the Go release tags, then `build.Default.ToolTags` for dotted tags. Matching is **case-sensitive**, as the toolchain matches; the old evaluator lowercased the whole expression first, which quietly made a mixed-case `-tags MyTag` unsatisfiable — `SetTag` stored it verbatim but the lookup folded it.

**Release tags are asked of the go command, not of the compiled-in list.** This is the one place where "use `build.Default`" — right for `ToolTags` — is wrong. Under `GOTOOLCHAIN=auto` the go command re-execs a **newer** toolchain when the main module asks for one, so `go/packages` can be selecting files under Go 1.25 while the `build.Default.ReleaseTags` linked into a go2cs built with Go 1.23 stops at `go1.23`. The re-check would then call `go1.24` false where the loader called it true and drop the file — *and* the `!go1.24` sibling was already dropped upstream by the loader, leaving the package with **neither half** of a fallback pair. Over-exclusion is this pass's recurring failure mode (the `purego` seeding and the `goexperiment` branch above each exist to undo one) and it is the dangerous direction, because the loader has already applied the full constraint for the target platform — anything this pass subtracts is real code. So release tags come from `go env GOVERSION` run in the same directory `packages.Load` uses, expanded to `go1.1`…`go1.N`. An unreachable or unparseable toolchain falls back to the compiled-in list — never to an empty one, which would make every `go1.N` false.

That subprocess costs ~300 ms on Windows, so it is contained twice. Resolution is **lazy** — `matchTag` tests the `go1.N` *shape* before resolving anything, so a constraint naming no release tag never pays it; and the answer is then cached per **module root**, because GOTOOLCHAIN keys on the module. Both halves earn their place: the behavioral corpus is **569 separate modules**, so the cache alone would still spend 569 lookups answering a question not one of those packages asks (no behavioral constraint mentions a release tag), while `-stdlib` reaches the lookup exactly once — `sort` is the only standard-library package with a `go1.N` gate — and `GOROOT/src` carries one `go.mod` above all 302 packages anyway. Note this does not help the **type checker** go2cs links in, which is still whatever release compiled it; a module whose `go` directive exceeds that still needs go2cs rebuilt on a newer toolchain.

Guarded by `src/go2cs/buildConstraints_test.go` (release tags bare/negated/compound, the legacy `+build` grammar, extraction precedence, loader-toolchain resolution), each assertion verified to fail against the pre-fix converter.

## An import forces the imported package's `init` to run

Go guarantees an imported package is fully initialized **before** the importing package's own
initialization, for every form of import. A converted Go `init` becomes `[GoInit]`, which
`csproj-template.xml` aliases to .NET's `[ModuleInitializer]` — the right shape and a **weaker
guarantee**: a module constructor runs at first access to something in *its own* module, so an
assembly nothing in the program has touched yet has not initialized. The converter closes that gap by
emitting a hook that forces the imported package's module constructor.

**Blank imports were forced first, and for years they were all that was.** `import _ "image/png"`
imports a package purely for the side effects of its `init`, so it is by definition the case that
names nothing, and the observable form is a registry that stays empty. `image/gif`'s `writer_test.go`
blank-imports `image/png` so that png's `init` calls
`image.RegisterFormat` (`image/png/reader.cs`); with the import emitted as a comment alone that never
ran, `image.Decode` had no PNG entry, and `TestWriter` failed with
`../testdata/video-001.png image: unknown format` — the package's only failure, at 27 of 28. The same
shape gates every registration-by-blank-import consumer: `database/sql` drivers (`sql.Register`),
`net/http/pprof` (its `init` installs the `/debug/pprof` handlers), `image/png` and `image/jpeg` as
decoders for anything calling `image.Decode`, and `time/tzdata`.

**That reading was true of blank imports and said nothing about the others** (2026-08-26). A NAMED
import whose package is referenced only from a *function body* is equally untouched at
module-initialization time, and Go orders it identically. `log/slog` is the case that made the
difference observable: slog's `init` captures `log/internal.DefaultOutput`, which **`log`'s own
`init`** installs. Whichever of the two is touched first wins, so a test host that touches `slog`
first captured **nil** into `defaultHandler.output` — a value that is captured, never re-read, so
`log`'s later initialization could not repair it. `handler.cs:120` then dereferenced a nil func: on
the test's own thread that surfaced as one ordinary failure, and on a goroutine it escaped and killed
the host outright, costing every ordinally later test its verdict. It is a *correctness* fix rather
than a verdict fix — before it, any converted program that touched `log/slog` before `log` crashed.

**The trigger is "the imported package initializes something, transitively."** Two honest trigger
sets were available. Forcing **every** import matches Go exactly and costs a hook per import per
package, corpus-wide. Forcing every import whose module constructor is non-empty *transitively* is
observationally **equivalent** — running an empty module constructor is a guaranteed no-op, the same
reasoning the pseudo-package skip below already applies to `unsafe`/`builtin`/`C` — at a fraction of
the emission. The second is what the converter does.

The fact behind it is computed in process from the loaded package graph (`packageInitFacts.go`)
rather than published as metadata, and that is the one design decision in the change worth stating
plainly. A package **initializes** when it declares a `func init()`, or when go/types'
`Info.InitOrder` carries a package-level variable whose initialization expression is not a
compile-time constant — Go's own definition of what runs at init time, read off Go's own analysis
rather than re-derived. That answer is then closed transitively over the import graph, which is a
DAG, so it terminates; and only *reachability* matters, because forcing a package runs the forcing
hooks it carries in turn, so the runtime walks the graph one link at a time.

Computing it in process is possible because `go/packages` is loaded with `LoadAllSyntax`, which
carries **syntax and type information for every dependency**, not only for the package being
converted (measured on `log/slog`: 66 transitive dependencies, 66 with `TypesInfo`, 65 with `Syntax`
— the one without is `unsafe`, which the pseudo-package fence already answers). Three shapes make
that materially better than publishing the fact as a `package_info.cs` record: the
**hand-owned-by-consequence** packages (`internal/godebug`; at Go 1.23.12 also `internal/concurrent` and `internal/weak`)
never re-emit a `package_info.cs` at all, so a published record could never appear for them — and
`internal/godebug` has an `init`; a **hand-owned file** inside a converted package is not visited, so
its `init` would be invisible to a record scraped from emitted output, while Go's own graph still
sees it; and **layout L3** would need the record routed per `GOOS`, whereas the loader answers per
target for free, since which files a package is built from is exactly what decides whether it has an
`init`. The fact also stays deterministic from the Go sources alone, so the emission cannot vary with
the state of the output tree — the failure mode the `-go2cspath` empty-`<ImportedTypeAliases>` trap
is the standing example of.

An import path this conversion has **no loaded handle for** answers *yes*. Forcing a module
constructor that turns out to be empty is a guaranteed no-op; skipping one that is not loses Go's
ordering silently, and silently is how this defect lived in the corpus for months. The trigger fails
toward fidelity, never toward the smaller emission.

⚠ **A read-set heuristic cannot substitute.** The tempting narrow rule — "force only the imports whose
symbols the importer's own `init` references" — MISSES this exact case: slog's `init` reads
`log/internal.DefaultOutput`, but the package whose `init` WRITES it is `log`. The dependency that
must be forced is not the one the init statement names.

A blank import still emits **no `using`** — `using _ = <ns>;` would hijack C#'s `_` discard for the
whole file (CS0118 + CS0029 on any deconstruction discard) — so it stays a comment, and every import
form, blank included, gets the same generated module-initializer hook at the top of the importing
file's class body:

```go
import (
    _ "BlankImportSideEffects/jpeglike"
    _ "BlankImportSideEffects/pnglike"
    "BlankImportSideEffects/registry"
)
```
```csharp
// blank import: BlankImportSideEffects.jpeglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: BlankImportSideEffects.pnglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using registry = BlankImportSideEffects.registry_package;

partial class main_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸjpeglike() {
    builtin.initPackage(typeof(BlankImportSideEffects.jpeglike_package));
}

// …and the same hook for the NAMED import beside them, which Go orders identically.
[GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸregistry() {
    builtin.initPackage(typeof(BlankImportSideEffects.registry_package));
}
```

Four decisions make that emission what it is.

**The mechanism is `RuntimeHelpers.RunModuleConstructor`**, wrapped as golib's `builtin.initPackage(Type)`
so the emitted line stays readable and the mechanism lives in exactly one place. It is the explicit,
spec-defined way to run a module constructor, and the runtime guarantees a module constructor runs **at
most once** — so several importers of one package, or a package forced after it has already
loaded, are no-ops rather than repeated `init` calls. Measured under **Native AOT** as well as the JIT:
the call is AOT-safe, and under AOT the gap does not even arise (a single native image has no lazy
assembly load, so every linked module's initializers run at startup regardless) — the forced call is
simply redundant there. `typeof` also roots the package class for the trimmer, so a trimmed publish
keeps the assembly the program otherwise never names.

**The hook leads the class body**, ahead of the file's own `init` functions, because Go orders an
imported package's initialization before the importer's. Roslyn emits an assembly's module-initializer
calls in compilation file order and then declaration order within a file, so leading the file's
declarations is what that ordering buys — within one file it is exact. Across files of one package the
order is Roslyn's, and the conversion does not state it: an `init` in file B that depends on an import
only file A names is ordered by the compiler rather than by go2cs. That residual is the one part of
Go's rule the emission still approximates, and closing it would mean a single per-assembly driver that
forces every import in dependency order — a strictly larger change, and one no measured case needs.

**Exactly one hook per (assembly, imported package).** Go initializes a package once per program
however many files import it, and a .NET module constructor likewise runs once per assembly, so the
hook belongs to the first file that names the import; later files' imports of the same package
emit nothing. Since the hook covers named imports too, that overlap is now the rule rather than the
exception — most files of a package re-import what a sibling already forced. The hook's name is
derived from the import path (`image/png` →
`initᴛᴛimportꓸimageꓸpng`), which makes it unique by construction — two imports in one file
are two methods, and a shared generated name would be CS0111 — and stable across runs without a
counter or a file-name mangle. Path segments are reduced to C# identifier characters, so a module
path's dots and hyphens (`github.com/mattn/go-isatty`) cannot break the identifier. The doubled temp
marker keeps the name clear of the relocated-package-var method space (`initᴛ<varname>`, see
[Package-Level Variable Initialization Order](variable-initialization-order.md#package-level-variable-initialization-order)) and the
`import` word clear of the `-tests` package-init hook (`initᴛᴛtests`).

**The forcing `typeof` is root-qualified when the enclosing class shadows its leading segment.**
The hook body is the ONE place the converter spells a namespace-qualified path where C# *class*-member
lookup applies. Every other cross-package reference is emitted through a file-scoped `using` alias, and
a using **directive** resolves at namespace scope, where class members are not in play; the hook sits
inside the class body, and C# resolves the leading identifier of a namespace-or-type-name by searching
the enclosing type declarations **outward first**. A nested type sharing that identifier therefore
occludes the namespace for the whole class body — while the alias a few lines above keeps working, which
is what makes the failure read like a converter regression somewhere else entirely:

```csharp
using palette = image.color.palette_package;   // namespace scope — resolves

partial class image_internal_test_package {

[GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() {
    builtin.initPackage(typeof(image.color.palette_package));   // CS0426
}

[GoType] internal partial interface image : Image { … }         // ← occludes `image`
```

Go's own `image_test.go` declares a test-local `type image interface{…}`, and `package image` is free to:
importing `image/color/palette` binds `palette`, not `image`. The same shape is available to production
code — `type sync struct{}` beside `import "sync/atomic"` breaks identically — so the remedy covers the
class, not the instance: `writeImportInit` root-qualifies the target with `global::` (which restarts
lookup at the global namespace and so cannot be occluded at all) whenever `forcingTargetShadowed` finds a
package-level **type** named for the target's leading segment that emits into **this** class.

Both halves of that gate are narrow on purpose, and both directions cost something. Only a *type*
occludes: `typeof(a.b)` is a namespace-or-type-name, and that lookup considers types and namespaces
alone, so a same-named func or var is irrelevant. And only a type emitted into the *same* class occludes,
which is what keeps a production hook bare when the shadow lives in the test-variant class — the two are
sibling partial classes, and a `using static` import of one loses to the namespace's own members at
namespace scope — so the `-stdlib` and `-tests` emissions of the same production file stay identical.
Over-qualifying would be valid C# but would churn every hook in the corpus (2,147 sites across 691 files
at the time of the fix, 606 behavioral goldens among them); under-qualifying is a hard CS0426. An ordinal
census of the converted standard library finds **exactly one** shadow site — `image`'s — which is why the
guard costs the corpus nothing: every site it changes is a site that did not compile. Guarded by
`tests/Behavioral/ImportSegmentTypeShadow` (production scope, compiled and output-compared) and by
`importSegmentShadow_test.go` (the test-local half, the gate's four quadrants, and the rooting shapes).

**Go's pseudo-packages are skipped.** `unsafe` and `builtin` are compiler-provided and have no
initialization at all, and `C` is cgo. `import _ "unsafe"` is the `//go:linkname` ritual — 67 files of
the converted standard library — so forcing it would be a guaranteed no-op emitted 67 times. They are
answered before the loaded-handle lookup, which matters for `unsafe` specifically: it loads with type
information but **no syntax**, otherwise indistinguishable from a package the loader failed to give
this conversion, which fails open and would force it.

**Only the module constructor is forced — that is exactly the package's `init` functions.** A package's
own package-level variable initializers are C# static field initializers on the package class, which
the CLR still runs lazily at first access to that class, unchanged from every other import (and the
package's own `init` touching them is what triggers them). The residual case is a package whose
registration is a package-level `var _ = pkg.Register(…)` rather than an `init`; closing it means
additionally forcing the package class's type initializer, which is deliberately *not* done here
because it would eagerly run `runtime`'s 291 package-level initializers on behalf of
`runtime/metrics`'s linkname-only import for no measured benefit. No import in the converted
standard library registers that way.

Note the deliberate asymmetry with the trigger above, which counts a non-constant package-level
initializer as initialization. The FACT states what Go runs at init time; the HOOK runs what a .NET
module constructor can run. Making the fact narrower — "only a `func init()` counts" — would tighten
the emission today at the cost of encoding one property of the current emission model into a
Go-level question, so a later change that moved relocated initializers into the module constructor
would silently under-force. The looser fact costs a no-op hook for a package whose only
initialization is variables; it cannot cost a missed one.

The `-tests` emission carries all of this unchanged — the hook is written by the same import visitor,
so an import in a `_test.go` file (which is where `image/gif`'s blank import is) forces from the test
assembly. Under the **recompile** model the external variant's import of the *package under test*
binds a class compiled into that same assembly, so there is no separate module constructor and no
hook; under the two **production-reference** models there genuinely is one, and the ordinary
per-import hook forces it (`iter`'s `pull_test.cs` carries a real `initᴛᴛimportꓸiter()`). What that
does **not** cover is the subject of the next subsection.
Guarded by the `NamedImportInitOrder` behavioral test — four packages in the reduced `log/slog` shape,
where `store` holds a value and initializes nothing, `writer`'s `init` writes it, `reader`'s `init`
CAPTURES it, and `main` touches only `reader`, from a function body. Before the fix Go printed
`written-by-writer-init` and the converted C# printed the empty string. It also pins the
selectivity in the same run: `store` gets no hook, because it initializes nothing transitively. And by
the `BlankImportSideEffects` behavioral test — a `registry` package that two blank-imported
sibling packages fill from their `init`s, read back by an importer that never names either registrant,
with the importer's own `init` recording the count to prove the ordering and an unregistered name as
the negative control (without the hooks the program prints `count at init: 0` and three `missing:`
lines; with them it matches `go run` exactly) — plus four converter unit tests:
`TestImportInitName` and `TestNoInitPseudoPackages` lock the generated name's uniqueness and the
pseudo-package skip, while `TestPackageInitializesTransitively` and
`TestPackageInitializesTransitivelyFailsOpen` lock the fact itself against a loaded fixture module
covering each shape — no init at all, constant-only initializers, a `func init()`, a non-constant
variable initializer, and reaching an initializing package one and two hops away — plus the
unknown-path direction.

## A `-tests` production-reference project forces the package under test's own `init`

Go's contract for a test binary is stronger than "imports first": every `init` **in the package
under test** — the *production* files' included — has run before the first test does. The subsection
above closes the import half. This one closes the half the import machinery structurally cannot
reach.

Under the two production-**reference** test models (`reference` and `whitebox-reference`) the test
assembly is a separate module that *references* the production assembly of the **same** Go package.
`[GoInit]` is `[ModuleInitializer]`, which fires at first access to something in its *own* module,
so the production `init` ran at the first **touch of a production symbol** — which may be the second
test, or the tenth, or never. A test that only observes an `init` **side effect** therefore sees
nothing at all when it happens to run first.

An **external** test file (`package foo_test`) writes `import "foo"`, so the per-import hook already
forces it. An **internal** one (`package foo`) is *in* the package and imports nothing of it: there
is no import spec for `writeImportInit` to hang a hook on, and nothing else was forcing the module.

`net/http/pprof` is the shape that made it observable (2026-08-29). Its whole mux surface is
installed by its `init` (`http.HandleFunc("/debug/pprof/", Index)`, …), its test file is `package
pprof`, and `TestDeltaProfile` is the only test that goes through a real `httptest` server rather
than calling handlers directly. It got **404** whenever it ran before any test that touched a
production symbol — measured over eight shuffled runs that split four/four *exactly* on order, plus
the unshuffled pipeline (where `TestDeltaProfile` sorts first) making nine observations. Every
banked `-tests` row that observes an `init` side effect was order-**lucky** rather than proven safe,
which is precisely why the class stayed invisible: most suites happen to touch something first.

The remedy is one hook, seeded into `package_test_info.cs`'s anchor class by
`referenceModelTestPackageInfoSeed`, using the same `RunModuleConstructor` mechanism as the import
hooks:

```csharp
[GoPackage("pprof")]
public static partial class pprof_internal_test_package
{
    // <TypeAccessibility> … </TypeAccessibility>

    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.http.pprof_package));
    }
}
```

Four decisions, each mirroring one the import hooks already made.

**It is emitted unconditionally**, with no "does the production package initialize anything" gate.
The import hooks pay for that gate because they emit one hook *per import per package* corpus-wide;
this is **one hook per test project**, an empty module constructor is a guaranteed no-op, and the
runtime runs one at most once — so a gate could buy nothing and could only mis-answer.

**The recompile model gets nothing**, and structurally rather than by a check: it never seeds this
file. There the production sources *are* compile items of the test assembly, so their `[GoInit]`s
are already that module's own.

**The `typeof` target is `global::`-rooted unconditionally**, where the import hooks root only on a
detected shadow. The import hooks are conditional because over-qualifying would churn thousands of
corpus sites; here there is exactly one site per test project and it is new, so the collision-proof
spelling costs nothing and needs no `forcingTargetShadowed` analysis to stay correct.

**The residual ordering nuance is stated, not engineered around.** Roslyn orders a module's
initializers lexically and the tests csproj sorts its compile items by name, so a test file sorting
before `package_test_info.cs` has its own `init` run before this hook. That is the same guarantee the
converter already declines to make *across* files of one package, and it does not touch the property
the hook exists for: every module initializer runs before `Main`, hence before the first test.

**What it does and does not buy on the row that found it.** With `init` forced, pprof's
`TestDeltaProfile` goes `skip` → `infrastructure-error`, landing on the same
`pprof_mutexProfileInternal` stub that already fails `TestHandlers//debug/pprof/mutex`. The deferred
`init` was **masking a capability gap**; removing the mask changes the shape of the divergence, never
its existence. The row reads 6 of 15 either way. That is the general lesson as much as the specific
one — an ordering defect that hides a stub reads as a *skip*, which is the least alarming verdict
there is.

Guarded by `TestReferenceModelSeedForcesProductionInit`, which asserts the two properties that make
the hook work rather than its mere presence — that it carries `[GoInit]` (a plain method would never
run) and that its `typeof` is `global::`-rooted — over both the flat and the nested-namespace
whitebox shape.

## A NuGet-referenced standard library carries its exported metadata IN THE CONVERTER

Everything above — the imported-type-alias `global using` round-trip, and the foreign `GoImplement`
records that tell a consumer its dependency's own assembly already implements an interface — is learned
by **opening the referenced package's `package_info.cs` and scraping the `[assembly: …]` lines out of it**
(`loadImportedTypeAliases` / `loadPackageImplements`, `importOperations.go`). That file is converted
*source*, found at `$(go2csPath)core/<pkg>`, i.e. wherever `deploy-core` staged the standard library.

`-recurse=nuget` removes exactly that file. In NuGet mode the standard library is consumed as
pre-compiled `go.<pkg>` assemblies, so there is no converted-source tree on disk — and there never will
be, since skipping the source deployment is the whole point of the mode. The scrape then finds nothing
and the converter falls through to `foreignDerivedTypeAliases`, which recovers *some* aliases from the
dependency's own Go declarations and **no implement records at all**. Two things go wrong, both in the
CONSUMING package:

* its `<ImportedTypeAliases>` block loses the dependency's exported aliases
  (`global using syscallꓸHandle = go.syscall_package.ΔHandle`, `netꓸAddr`, `timeꓸLocation`, …); and
* not knowing that `syscall`'s assembly already implements `error` on `syscall.Errno`, it **re-declares
  the pair locally** — `[assembly: GoImplement<syscall_package.Errno, error>]` — and wraps the value at
  the cast site in a locally-generated adapter (`new syscall_Errnoᴠerror(e)`) instead of converting
  implicitly.

`golang.org/x/sys/windows` shows both at once: it Go-aliases `type Errno = syscall.Errno`, so the two
spellings `Errno` and `syscall_package.Errno` were each recorded, resolved to the same type, and made
`ImplementGenerator` emit the `syscall_Errnoᴠerror` adapter **twice** — CS0102 / CS0111 ×5 / CS8646.
That single duplicated adapter is what made the README's `fatih/color` walkthrough unbuildable under
`-recurse=nuget`.

**Note what is NOT the cause.** The `go2cs-gen` generators are not reference-kind sensitive: a real
MSBuild build hands a `<ProjectReference>` to the compiler as a `PortableExecutableReference` exactly
like a `<PackageReference>`, and the generators read foreign *type shape* from symbols either way (see
`FindTypeSymbol`). Both modes ran the same generator over the same kind of reference; the emitted C#
they were given differed, because the **converter's** cross-package knowledge differed.

**The conversion.** The exported metadata is a static, per-package property of the *published* assemblies,
so it travels with the converter. `internal/genstdlibmeta` captures the `<ExportedTypeAliases>` section and
every `GoImplement` record of every `src/core/**/package_info.cs` into
`src/go2cs/stdlib-metadata.txt`, which `stdlibMetadata.go` embeds via `//go:embed`. When a
stdlib dependency's `package_info.cs` is absent, the converter reads its recorded lines through the very
same parsers (`parseExportedTypeAliasLines` / `parseExportedPointerImplementLines` /
`parseExportedValueImplementLines`, refactored off the file read for this) and proceeds identically. The
asset is generated from the same tree `push-nuget.ps1` packs, so the embedded record and the published
assemblies are always one commit's output.

Reading the record is gated on `PackageInfo.PublishedStdLib` — `isStdLib && nugetRefs && !convertStdLib` —
and this gate is a **soundness precondition, not conservatism**. The record describes the converted
standard library at `src/core`; substituting it is only correct when that is what the build actually
references. Under `-recurse=nuget` that is guaranteed. A `$(go2csPath)` deploy root may hold an older or
partial staging whose exported surface differs, and the stdlib
self-conversion is *building* the assemblies being published. Both keep the derive-from-declarations
fallback. Because `-recurse=nuget` is off by default, no other conversion path changes — CNR is
byte-identical across the behavioral corpus.

The result is exact rather than approximate: converting the README's `colordemo` with `-recurse=nuget`
against an empty `$(go2csPath)` now emits **byte-identical** C# to the same app converted with
`-recurse` against a full `deploy-core stdlib` root, and the NuGet-referencing solution builds with 0
errors and runs with output matching `go run`. `syscall.Errno` is not a special case — a probe returning
six foreign concrete types as foreign interfaces recovered 14 imported aliases and dropped three spurious
re-declarations spanning both record forms (`io/fs`'s `PathError`→`error` **pointer** adapter, `sort`'s
`IntSlice`→`Interface` **value** implement, and `syscall.Errno`), leaving only the one record the
consumer legitimately owns (`os.File` → `io.Reader`), exactly as the source-referencing conversion does.

The guards live in `stdlibMetadata_test.go`. `TestStdLibMetadataAssetFileName` pins the generator's output name
against the `//go:embed` target (and its `package_info.cs` constant against the converter's), so the two
halves can never write and read different files. `TestStdLibMetadataInSync` regenerates the asset in-process from
`src/core` and fails on drift — a stale asset would hand `-recurse=nuget` the *previous*
standard library's records while the published assemblies carry the current ones, surfacing only as
downstream C# errors. `TestStdLibExportedMetadataReadsThroughPackageInfoParsers` pins that the embedded
lines feed the shared parsers (and that a `Pointer`-form record does not leak into the value implements).
`TestPublishedStdLibScope` pins the three-way gate above. `TestRecurseNuGetResolvesForeignImplements`
converts a module returning a `syscall.Errno` as an `error` with no converted stdlib on disk and asserts
the consumer records nothing and emits no local adapter — it fails on both assertions with the record
disabled.

**The record is per GOOS.** A package whose metadata varies by platform — layout L3, no flat
`package_info.cs` — keeps its reference (windows) flavor in the unqualified `##<name>` section and each other
flavor in its own `##<name>@<goos>` section; a package with a flat copy has one section, whatever per-GOOS
copies sit beside it, because the converter reads flat first on disk too. `stdLibExportedMetadata(name, goos)`
takes the conversion's target GOOS, reads the flavor section when one is recorded and falls back to the
unqualified one. The flavors matter because a NuGet consumer compiles against the flavor go.lib's
RID-selected compile asset (`buildTransitive/go.lib.targets`) selects: a linux conversion that read the
windows record would import `syscallꓸHandle = go.syscall_package.ΔHandle` (a windows-only type, CS0426) and
the windows flavor's Δ-renamed `Sockaddr` (CS0305 against the linux flavor's own).

The two halves must name the SAME platform, so `-recurse=nuget` writes that platform into the output
root's `Directory.Build.props` as a conditioned `GoCompileRuntimeIdentifier` default (windows → `win-x64`,
linux → `linux-x64`, otherwise unset), which `go.lib.targets` honors before `$(RuntimeIdentifier)` or the
build host. The compile flavor therefore follows the platform the tree was CONVERTED for, not the machine
doing the build, so a linux conversion built on windows, or a windows conversion built under WSL, pairs each
flavor's metadata with its own assembly. The dated account is in
`docs/phase4/DESIGN-multiplatform-corpus.md` §12's 2026-09-24 amendment.
Guards: `TestStdLibExportedMetadataSelectsTheTargetFlavor`, `TestStdLibExportedMetadataReadsAFlavorOnlyRecord`,
`TestStdLibMetadataCollectFlatWins`, `TestRecurseNuGetImportsTheTargetFlavorsAliases` (an emission arm) and
`TestRecurseNuGetPinsTheCompileRidToTheTarget` (the props emission).

**Residual limitation.** The asset is regenerated by `go generate .` from `src/go2cs`, so a change to the
converted standard library must be re-banked into `src/core` before it reaches
`-recurse=nuget` consumers. `TestStdLibMetadataInSync` makes that a test failure rather than a silent
mismatch, but it can only compare against the *committed* tree — it cannot detect that the committed tree
is itself older than the published packages.

## Standard-library solution file (`.slnx`)

A whole-standard-library run (`go2cs -stdlib`) also emits a Visual Studio solution — **`go2cs-stdlib.slnx`** — at the output root (`-go2cspath`), so the freshly converted stdlib is openable / buildable as **one unit** immediately after a run, rather than depending on a hand-maintained solution that drifts. It is the auto-generated counterpart of the committed `src/go2cs-stdlib.slnx`, and its XML mirrors the format of `src/go2cs.slnx` (a `<Configurations>` block plus `<Folder>`/`<Project>` entries, CRLF line endings, no BOM). It references:

* every converted stdlib project it emitted (`core/<pkg>/<projectName>.csproj`, grouped under a `/core/` folder),
* any per-package **test** projects (`*_test.csproj`, grouped under a `/tests/` folder — inert until Phase 4 emits them, and the folder is omitted entirely when there are none),
* the shared **`golib`** runtime (`core/golib/golib.csproj`), and
* the **`go2cs-gen`** source-generator/analyzer project (`gen/go2cs-gen/go2cs-gen.csproj`, under a `/generators/` folder).

The stdlib project list is gathered by walking the emitted `core/` output tree (so it also picks up future test projects with no code change), and every project is emitted in stable **alphabetical** order for deterministic output. All paths are **solution-relative** (forward slashes) so the generated solution is portable — no absolute, machine-specific paths. The `golib` and `go2cs-gen` references use the same `core\golib` / `gen\go2cs-gen` layout the converted `.csproj` files already assume via `$(go2csPath)` (which resolves to `$(SolutionDir)`), so the solution locates them wherever those csproj references already resolve. The file is only rewritten when its content changes, so repeated runs are a no-op.

## Recurse per-project solution file (`.slnx`)

A recursive end-user run (`go2cs -recurse`) instead emits **one `.slnx` next to every converted `.csproj`** (`ModuleConverter.generatePerProjectSolutions`), each over that anchor project plus its **transitive converted dependencies** + `golib` + the analyzer — no stdlib listed (the stdlib is *referenced* via `$(go2csPath)core`, pre-staged by `deploy-core`). Building the app's own per-project solution thus builds the app and its whole converted dependency closure in one `dotnet build`, without the ~300-project stdlib solution. The anchor project is marked the Visual Studio default startup project (`DefaultStartup="true"`).

Projects are grouped into **three top-level solution folders that mirror the `%GOPATH%` layout**, emitted in an **enforced, deliberately non-alphabetic order**:

* **`/src/`** — the project(s) being converted (the app's own main-module packages),
* **`/pkg/`** — their converted dependency packages (module-cache or `replace` third-party), then
* **`/core/`** — the go2cs runtime/generator projects (`golib`, `go2cs-gen`).

Each member is placed by **import path** (`isMainModulePackage` — the same rule that routes a package's output to `src\` vs. `pkg\`), so the solution folders agree with the on-disk parallel tree regardless of the `.slnx`-relative path shape. An **empty folder is omitted** — a dependency's own per-project solution has no `src` package — mirroring how the stdlib solution drops its `/tests/` folder when empty. Because the three folder names are unique leaves, no folder `Id` attribute is emitted (unlike the namespace-nested stdlib solution, whose duplicate leaves like `crypto`/`internal` require the hashed `folderID`). Paths are solution-relative forward slashes, CRLF line endings, no BOM; the file is rewritten only when its content changes. Rendered by `buildRecurseSolutionXML` (`solutionGenerator.go`), guarded by `TestBuildRecurseSolutionXML`, `TestBuildRecurseSolutionXMLSkipsEmptyFolders`, and the folder-order assertions in the `TestRecurseSyntheticModule` integration test.

## Moved sections

- <a id="an-upward-golinkname-var-alias-inverts-its-storage-instead-of-giving-up"></a>Moved to [An UPWARD `//go:linkname` var alias INVERTS its storage instead of giving up](manual-conversions/linkname-and-trampolines.md#an-upward-golinkname-var-alias-inverts-its-storage-instead-of-giving-up).
- <a id="a-foreign-implement-record-is-keyed-in-one-spelling-and-a-value-one-is-trusted-only-for-a-partial-struct"></a>Moved to [A foreign implement record is keyed in ONE spelling, and a VALUE one is trusted only for a partial struct](interfaces/records.md#a-foreign-implement-record-is-keyed-in-one-spelling-and-a-value-one-is-trusted-only-for-a-partial-struct).
- <a id="a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses"></a>Moved to [A package records the pairs it SATISFIES, not only the ones it witnesses](interfaces/records.md#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses).
- <a id="the-pointer-method-set-records-the-same-way-for-a-different-contract"></a>Moved to [The POINTER method set records the same way, for a different contract](interfaces/records.md#the-pointer-method-set-records-the-same-way-for-a-different-contract).
- <a id="a-doc-comment-link-resolves-to-a-fully-qualified-version-pinned-url"></a>Moved to [A doc-comment link resolves to a fully-qualified, version-pinned URL](comments.md#a-doc-comment-link-resolves-to-a-fully-qualified-version-pinned-url).
- <a id="a-sub-package-import-whose-leading-segment-is-a-package-alias-root-qualifies"></a>Moved to [A sub-package import whose leading segment is a package alias root-qualifies](golib-namespace.md#a-sub-package-import-whose-leading-segment-is-a-package-alias-root-qualifies).

---

[Index](README.md) · [Package-Level Variable Initialization Order →](variable-initialization-order.md)
