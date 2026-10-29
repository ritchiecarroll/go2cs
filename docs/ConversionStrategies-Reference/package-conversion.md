# Package Conversion

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#package-conversion)
Although a Go package more traditionally parallels a C# namespace, Go includes referenceable functions directly from within a package root, for example, the `Println` function in the `fmt` package is called like: `fmt.Println("Hello, world")`. For C#, only type declarations, e.g., `class`, `struct`, `enum`, etc., are allowed in a namespace; functions exist as part of a `class` or `struct`. Described from a C# perspective, all Go functions are [`static`](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members), i.e., the functions exist separately from an instance of a type. Go supports the notion of a receiver function which allows a function to be targeted to an instance of a type (paralleling the operation of a C# extension function), but this is still a static function.

As such, the conversion strategy for a Go package is to convert it into a static C# partial class, e.g.: `public static partial class fmt_package`. Using a partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = fmt_package;` (spelled relative to the enclosing `go` namespace). The receiver functions are emitted as extension methods on that partial class (a pointer receiver is `this ref T`, see [Source Generators](source-generators.md#source-generators)).

So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are simply converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = unicode.utf8_package;`. Each package also emits a `package_info.cs` carrying the `[GoPackage]`-attributed package class declaration plus the package-wide global `using` aliases (Go's built-in types, exported type aliases, etc.).

A consequence of converting a Go method to a C# **extension method** is that C# only discovers an extension method when
its containing static class's *namespace* is in scope (via a `using <namespace>;` directive or the enclosing namespace)
— a class **alias** such as `using atomic = go.@internal.runtime.atomic_package;` resolves the *type* (`atomic.Uint32`)
but does **not** bring the class's extension methods into scope. This matters when a file calls a method on a value
whose type comes from a multi-segment-path package (one that lands in a sub-namespace, e.g. `internal/runtime/atomic` →
`go.@internal.runtime`): Go never requires importing a value's package merely to call a method on it, so such a file may
emit no import — and hence no `using @internal.runtime;` — leaving the extension method invisible and the call
mis-binding to a wrong (e.g. embedding-promoted) overload (CS1929). The converter therefore registers the namespace of **every
cross-package method's defining package** as a file-local `using` at the call site, independent of the file's explicit
imports. (Packages in the root `go` namespace — most top-level stdlib packages — need nothing extra, since
same-namespace extension methods are always visible. This is a stdlib-structural concern that only surfaces under
multi-segment package paths, so it is guarded by building the converted `runtime` rather than by a single-package
behavioral test.)

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

The walk does not stop at *the first ancestor directory holding no `.go` files of its own*. Go modules
are made of such pure container directories — `internal/`, a proto grouping like `xds/core/`, the
`datatransfer/` above an `apiv1/`, a service tree's `endpoints/` parent — so naming a project after the
leaf segment below one would truncate routinely, and it would cost two things:

* **Duplicate project names.** `cloud.google.com/go/bigquery`'s `datatransfer/apiv1` and `storage/apiv1`
  would both emit `apiv1.csproj`. Visual Studio refuses to open a solution containing two projects of the
  same name and reports no reason — silently doing nothing from a file dialog, offering to forget the
  entry from the recent list — so **the entire generated `.slnx` fails to load** ([issue #35]). <!-- The
  issue's reported -recurse conversion had 175 such projects across 49 colliding names out of 1,727. -->
* **A collapsed namespace.** A truncated name joins its segments with `.` *before* the separator split
  that builds the namespace, so the qualification never reaches it: a truncated `internal/errors` would
  land on `go.errors_package` — the converted standard library's own class.

A go-file-free ancestor is therefore a **fallback, never a stop**: the walk records the leaf-relative name
it would have produced and keeps climbing. The fallback applies only when there is genuinely no module
root anywhere above — a GOPATH-style tree, the one case that needs it. Because both the
declaring package and every importer derive the name from the same call, the two sides move together.

When re-converting a tree that an older go2cs converted with truncated names, the full path
**renames** those packages, so a re-conversion into an existing output root leaves the old,
wrongly-named `.csproj` files behind as orphans that a solution may still list. Convert into a
fresh output directory, or clear the old one first. <!-- history: until the issue #35 fix the walk stopped at
the first go-file-free ancestor and named the project after the leaf; the fix renamed 743 of the 1,727
packages in the reported conversion. -->

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
spellings can coexist in one tree (most behavioral modules carry the marker; a minority are bare). <!-- measured at the
change: 618 marked declarations against 46 bare ones; not re-measured by any guard -->
Eliding it is safe
by construction — a fetchable module path's first element must contain a dot, because it is a host
name, and `go2cs` has none, so the marker cannot swallow a real dependency's first segment.

A module path is nonetheless spelled **twice** by every conversion, and the elision has to reach both
spellings. `getProjectName` (`importOperations.go`) is the DECLARATION side: it decides the emitted
`namespace`, the project name, and the `<ProjectReference>` file name derived from it.
`convertImportPathToNamespace` (`visitImportSpec.go`) is the IMPORT side: it decides a
`using <alias> = …` target, a bare `using <namespace>;`, and the `typeof` of an init-forcing hook. If
the declaration side elided the marker and the import side kept it, every reference to a `go2cs/…` path
would name a namespace that nothing emits, and such a package could not be imported at all:

<!-- illustration: not converter output -->
```csharp
// value.cs — the DECLARATION                    // external_test.cs — the IMPORT
namespace go;                                    using harness = go2cs.convertedtestharness_package;
partial class convertedtestharness_package {     using static go2cs.convertedtestharness_package;
```

`error CS0234` ×2. The end-to-end `-tests` fixture at `src/tests/PackageTests/ConvertedTestHarness`
is that shape — a `module go2cs/convertedtestharness` whose own external test variant self-imports
it. Both sides therefore route through one function (`trimGo2CSModulePrefix`), with the declaration
side's spelling as the canonical one. A behavioral test that holds a nested sub-package shows why both
spellings must agree: under `module IoLike` the parent imports `IoLike/FsLike`, which has no marker to
disagree about, while the same tree under `module go2cs/IoLike` would declare
`go.IoLike.FsLike_package` and, without the shared rule, import `go2cs.IoLike.FsLike_package`. Neither
spelling of a module has to change. <!-- history: the import side kept the marker until both sides moved to
trimGo2CSModulePrefix; the ConvertedTestHarness fixture never built before that, and the bare module
declarations in the behavioral corpus exist only to dodge the old asymmetry. -->

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
| Visual Studio's project loader | 260 characters (`MAX_PATH`) | **No.** Measured on a Windows machine with `LongPathsEnabled=1`: a 259-character `.csproj` loads and builds, a 265-character one fails with *"The project file could not be loaded. Could not find a part of the path"* — naming a file that is demonstrably present and that `dotnet build` reads without complaint. |
| A single filename **component** | 255 characters | **No, ever.** No registry key, `\\?\` prefix or manifest touches it (measured on Windows: a 265-character *path* writes fine on that same machine, a 256-character *file name* does not). |
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
package in the standard library and every behavioral test (both far inside the budget), so the
committed corpus is untouched. <!-- measured at the change: longest emitted stdlib path 101, longest
behavioral 109 (also in TestProjectFileBaseNameLeavesCorpusNamesAlone's comment; the test pins sample names,
not the maxima) --> A name that does not becomes `head~tail.hash8`: a readable head carrying
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
against — and the pin is how its value reaches the emitted build files. Without it the MSBuild
property of the same name would have only the csproj template's fallback (`$(USERPROFILE)/go2cs/`, or
`$(SolutionDir)` under Debug), so a solution converted into an isolated output root could not resolve
its stdlib/runtime/analyzer references unless the runtime happened to live at `~/go2cs` or a
deploy-core `Directory.Build.props` sat above the output. Three details:

* When the output root IS the runtime root (the one-positional form), the pin stays relative —
  `$(MSBuildThisFileDirectory)`, the deploy-core form — so the tree can move as a unit. An isolated
  output root pins the absolute resolved root (forward slashes and a trailing separator, per the
  section below); a re-conversion refreshes it.
* The `Condition` keeps it a default: a `go2csPath` environment variable, a `-p:go2csPath` build
  global, or a higher `Directory.Build.props` still wins.
* `-recurse=nuget` deliberately emits **no** pin — it has no `$(go2csPath)` references to resolve,
  and its props defaults `GoStdLibVersion` instead: the Go minor of the converting toolchain with a
  float (`<GoStdLibVersion>1.26.*</GoStdLibVersion>` for a Go 1.26 conversion, `goStdLibVersionFloat`), so
  restore takes the newest published `go.*` release of that minor. Set `GoStdLibVersion` (in that file, a higher
  `Directory.Build.props` or a `-p:` global) to pin one exactly. A foreign `Directory.Build.props` at the output
  root (deploy-core's, or user-authored) is never clobbered, per the generated-file marker rule.

Guarded by the `go2csPath` assertions in `TestRecurseSyntheticModule` (absolute pin, isolated
output root), `TestRecurseBuildFilesRelativePinWhenRootsCoincide` (relative form), and
`TestRecurseNuGetReferences` (no pin under NuGet references).

[issue #36]: https://github.com/ritchiecarroll/go2cs/issues/36

## Path separators in emitted MSBuild files: forward slashes, on every host

**Every path the converter writes into a `.csproj`, `.slnx`, `.pubxml` or `Directory.Build.props` uses `/`, on every host**, with one generated exception: the `-recurse` output root's artifact redirection (`.artifacts\obj\<token>\` and its `bin`/`gen` siblings, in that root's `Directory.Build.props`/`.targets`, `moduleConverter.go`) is still written with `\`, which MSBuild normalizes on Unix. There is no per-host emission and no host-conditional spelling: one converted corpus is correct on Windows, Linux and macOS.

```xml
<ProjectReference Include="$(go2csPath)core/fmt/fmt.csproj" />
<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" … />
<OutDir>bin/$(Configuration)/$(TargetFramework)/</OutDir>
<GoValidationProofFile>$(go2csPath)../docs/validation/$(GoStdLibVersion).$(GoBuildNumber)/fmt.md</GoValidationProofFile>
```

MSBuild accepts `/` in every path context on Windows, and normalizes `\` to `/` on Unix (`FileUtilities.MaybeAdjustFilePath`), so *both* spellings build on both hosts — the .NET SDK's own targets depend on the Unix direction. The reason to pick one is that two other consumers are **not** MSBuild:

1. **The converter's own path arithmetic.** On Unix `filepath.Join` treats `\` as an ordinary filename character, so a
   reference composed with backslashes (`$(go2csPath)core\fmt/\fmt.csproj`) is malformed on a Linux host for *every*
   stdlib reference in *every* project — silent at emission, a restore failure later. The composition is
   `emittedProjectReference` (`importOperations.go`): `path.Join` (slash-only, host-independent) over a
   `filepath.ToSlash`'d directory. <!-- history: the reference was once composed by replacing every / with \ and joining
   a backslash-prefixed file name, which Windows' filepath.Clean repaired and Linux did not (F5,
   docs/PLAN-linux-operation.md §A1.1). --> `writeProjectFile` and `writeTestProject` additionally `ToSlash` at the
   emission point, because a sibling reference made relative by `filepath.Rel` arrives OS-native.
2. **The harnesses that READ an emitted reference.** `BehavioralRunner.PreBuildSharedDeps` and `PerformanceRunner` parse `ProjectReference Include="…"` out of the csproj and resolve it with `Path.GetFullPath`, which on Linux does not split on `\` either.

**The separator decides the sort order of the reference block.** References are `sort.Strings`-sorted, and `/` (0x2F)
sorts *below* alphanumerics while `\` (0x5C) sorts *above* them, so a pair that differs at a separator boundary sorts
differently under the two spellings: `vendor/golang.org/x/net/http/httpguts` sorts before
`vendor/golang.org/x/net/http2/hpack` (`/` < `2`), where the backslashed spelling put it after (`2` < `\`). <!--
measured at the change over the 303-project stdlib: the slash flip reordered exactly one file, net/http's reference
block; same set, different order. -->

The hand-owned `core` files that are never re-emitted — `golib`, `testing`, `unsafe`, `internal/godebug` and
`core/Directory.Build.props` among them — carry the form by hand. <!-- history: at Go 1.23.12 the list also named
internal/concurrent and internal/weak; their Go 1.24 successors internal/sync and weak convert ordinary files beside the
hand-owned one, so both emit their own .csproj. --> The one deliberate exception is the shared-project
`<Import Project="..\go2cs\go2cs.projitems" Label="Shared" />` in `golib.csproj` and `go2cs-gen.csproj`: that is Visual
Studio's own bookkeeping, VS round-trips its exact text, and MSBuild normalizes it on Unix regardless — so it stays
backslashed, with a comment saying why.

Guarded by `TestEmbeddedCsprojTemplatesUseForwardSlashesOnly` and `TestValidationPackBlockUsesForwardSlashesOnly` (`csprojTemplate_test.go` — both templates are asserted to contain *no* backslash at all, so a future addition is covered without the guard enumerating it) and by `TestEmittedProjectReferenceIsHostIndependent` / `TestEmittedProjectReferenceForModuleCachePath` (`importOperations_test.go`).

## The validation-proof block follows the OUTPUT LOCATION, not the invocation mode

A converted stdlib package's `.csproj` carries a block that packs its versioned proof sheet as `VALIDATION.md` inside the nupkg, and `.csproj` files are **regenerated on every transpile** — so the block's emission condition is what decides whether a package keeps shipping its proof. The rule is a structural test on where the project file is being WRITTEN:

> emit the block for a `-stdlib` conversion, or for any conversion re-emitting a `.csproj` under the runtime root's `core/` tree.

A rule keyed on the invocation MODE would leave a door open for every mode it does not name, and each open door silently
un-ships a package's proof sheet at the next `push-nuget`: a `-tests` run rewrites the package under test's `.csproj`,
and a single-package conversion, `go2cs <goroot-pkg-dir> <core-pkg-dir>` — the form that regenerates ONE corpus package
after a converter change — is neither `-stdlib` nor `-tests`. The single-package case is the harder one to catch,
because only the `.csproj` moves and a lone `.csproj` diff in a reconvert reads as ordinary emission drift rather than
as a loss. <!-- history: the block was first scoped to -stdlib, then to -stdlib or -tests; the -tests door stripped the
block from every package under test, and the single-package door was closed 2026-08-19. -->

Keying on the output location closes every such door and cannot be re-opened by adding a mode: `-recurse` writes under
its own `src/`/`pkg/` trees, a behavioral fixture writes under `tests/Behavioral/`, and an end-user module writes
wherever it was pointed — none of them satisfy "under `<go2csPath>/core/`", so none of them gets the block. Guarded by
`TestValidationPackBlockSurvivesTestsRewriteOfCorePackage`, which pins the single-package form and its outside-`core`
negative control alongside the `-tests` pair.

## A GOROOT-vendored reference is named for the package's ON-DISK path

**A standard-library reference takes its project file name from the directory it resolved to, not from the import path as written.** The two are the same string for every package in the standard library except one class — the GOROOT-**vendored** ones, imported as `golang.org/x/…` but existing on disk, and therefore as converted projects, only under `vendor/golang.org/x/…`.

```xml
<!-- crypto/ecdh imports `golang.org/x/crypto/chacha20` -->
<ProjectReference Include="$(go2csPath)core/vendor/golang.org/x/crypto/chacha20/vendor.golang.org.x.crypto.chacha20.csproj" />
```

The directory half is rewritten from the resolved source dir; a file name composed from the import path would name a
real directory and a file in it that exists nowhere:
`…/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj`. Deriving the name from the directory
(`stdLibImportPathFromTargetDir`, applied in both stdlib arms of `importOperations.go`) is what makes the two halves
agree *structurally* rather than coincidentally: `getProjectName` — the producer, which names the `.csproj` the vendored
package actually emits — derives it from that same `GOROOT/src`-relative directory.

This is the third derivation in one family, and they must all resolve the vendored spelling or they disagree about which
package is being named: the **namespace** (`resolveGorootVendoredPath`, under
[Cross-package imports](#cross-package-imports-importing-another-package--assembly)), the **dependency-graph key**
(`stdLibConverter`), and the **project file name**.

Two consequences beyond the file name, because `PackageName` is not only a file name:

* It keys the embedded standard-library metadata, which records the vendored spelling
  (`##vendor.golang.org.x.crypto.chacha20`), so an unvendored name matches no section at all (asserted directly by the
  guard). Wherever that record is the source — a dependency with no `package_info.cs` on disk, i.e. a `-recurse=nuget`
  reference — the package's exported aliases and `GoImplement` records would come back empty and silently fall through
  to the derive-from-declarations path.
* It composes the imported-alias class path. The unvendored form yields `go.golang.org.x.crypto.chacha20_package`,
  which names a class that exists nowhere — the CS0234 family the namespace arm above exists to prevent.
  `loadImportedTypeAliases` dedupes on the *dependency's* `package_info.cs` path, which is the same file for both
  spellings, so a mixed pair would not fail loudly: whichever spelling resolved first would win and the other would
  never apply its aliases.

**Where the rule matters most.** Production emission resolves the vendored path upstream (`visitImportSpec`), while a
`-tests` project's dependency list is the raw import set, which carries both spellings (`crypto/ecdh`'s test project
imports the package under both). A wrong `<ProjectReference>` there would not break the build — MSBuild degrades a
missing one to **warning MSB9008** and builds on — but the stale name would be harvested into `go2cs-stdlib.slnx` as a
phantom extra project by the multi-platform merge's solution-recovery path, which
`TestCollectConvertedProjectsIgnoresTestProjectReferences` guards on the solution side. <!-- history: before
stdLibImportPathFromTargetDir, crypto/ecdh's test project named chacha20 twice, once with the unvendored file name; the
pre-fix crypto.ecdh.tests.csproj built with 0 errors and the phantom project reached go2cs-stdlib.slnx. -->

Guarded by `TestGorootVendoredReferenceNamesTheVendoredProject` (the vendored spelling, the metadata key, and the leaf package name), `TestStdLibImportPathFromTargetDir` (the recovery, including the non-core-rooted no-match that leaves the caller on the import path) and `TestStdLibReferenceUnchangedForUnvendoredPackage` (the no-op half — the whole corpus bar the `vendor/` tree, which is what makes a zero-movement verdict from the behavioral regression check meaningful rather than lucky)<!-- [CNR](../Glossary.md#cnr) -->, all in `importOperations_test.go`.

## An importer spells the package class from the package NAME — the standard library included

**The emitted class is `<packageName>_package`, so an importer's spelling has to come from the Go package name, never from the last segment of the import path.** The two agree for nearly every package, because a Go package is conventionally named for its directory — which is exactly why the places they disagree are so easy to miss.

```go
// crypto/internal/entropy/v1.0.0/entropy.go
package entropy                                  // directory `v1.0.0`, package `entropy`
```

<!-- illustration: not converter output -->
```csharp
// the declaration side follows the package name — crypto/internal/entropy/v1.0.0/entropy.cs
namespace go.crypto.@internal.entropy.v1_0_0;
partial class entropy_package { … }

// so the importer must too — crypto/internal/fips140/drbg/entropy_fips140.cs
using entropy = go.crypto.@internal.entropy.v1_0_0.entropy_package;
```

`convertImportPathToNamespace` substitutes the import graph's authoritative package name for the path's last segment,
for standard-library and module imports alike. An importer that spelled the class from the path instead would name
`v1_0_0_package` against a declared `entropy_package`; a case-only difference fails the same way, because C# is
case-sensitive. The result is **CS0234** — and it reads like a missing project reference or an empty assembly, because
the symbol genuinely exists nowhere. <!-- history: the substitution once applied to non-stdlib imports only. Through Go
1.24 crypto/x509/internal/macos was `package macOS`, darwin-exclusive, and its importers emitted macos_package against a
declared macOS_package (CS0234) until darwin was first built; Go 1.26 renamed the package macos. -->

Censused across `windows`, `linux` and `darwin` at Go 1.26.10 (`go list std`, 2026-10-22), the standard-library paths whose package name differs from their tail are:

| Import path | Package | Targets | Disposition |
|---|---|---|---|
| `math/rand/v2` | `rand` | all | already correct via the `/vN` branch |
| `crypto/internal/entropy/v1.0.0` | `entropy` | all | keeps its directory segment (`go.crypto.@internal.entropy.v1_0_0`) |
| `crypto/internal/fips140deps` | `fipsdeps` | all | keeps its directory segment; no std production package imports it |
| `crypto/internal/fips140test` | `fipstest` | all | keeps its directory segment; test-only |
| `internal/runtime/wasitest` | `wasi` | all | keeps its directory segment; test-only |

<!-- earlier censuses: four rows at Go 1.23 (crypto/x509/internal/macos -> macOS on darwin, the case-only row that
     moved; math/rand/v2; internal/trace/internal/testgen/go122 -> testkit; runtime/internal/wasitest -> wasi), and
     Go 1.24 added the two fips140 rows (re-censused 2026-10-04). Go 1.26 renamed macOS to macos, dropped go122's
     testkit, moved wasitest under internal/runtime and added crypto/internal/entropy/v1.0.0. -->

For every other standard-library package the name and the directory agree, so trusting the import graph *everywhere*
changes no standard-library spelling. The "keeps its directory segment" rows are the next section's rule.

The rule is structural rather than a special case: **when the graph knows a package's name, that name is the class
segment; the `/vN` directory convention is the fallback for when it does not.** A narrower "substitute only when the two
differ" test would look equivalent and quietly break the exotic case the convention branch was written for — a package
literally *named* `vN`, which would then be rewritten to its parent. Preferring the authoritative name over the
convention wherever both are available is what keeps the two rules from fighting.

This is the same family as [the GOROOT-vendored reference](#a-goroot-vendored-reference-is-named-for-the-packages-on-disk-path) above: several independent derivations name one package, and they are correct only when they agree *structurally*. Guarded by `TestImportedPackageClassFollowsPackageName` (the rule, over a stdlib name/directory mismatch, an ordinary stdlib package, a `/vN` directory and a module dependency) and `TestMajorVersionFallbackAppliesWithoutGraphMetadata` (the fallback half), in `packageClassNaming_test.go`.

## A package named differently from its directory keeps the directory in its NAMESPACE

**When a package's name differs from the last segment of its import path, the namespace keeps the whole import path and the class is `<name>_package` beneath it.** Two directories under one parent may declare the same name, and Go keeps them apart by their import paths:

```go
// github.com/google/go-cmp/cmp/internal/teststructs/foo1   and   …/teststructs/foo2
package foo                                      // both directories, one package name
```

<!-- illustration: not converter output -->
```csharp
// with the parent namespace alone: one fully qualified type for both, CS0433 in cmp's tests, which reference both
namespace go.github.com.google.go_cmp.cmp.@internal.teststructs;
partial class foo_package { … }

// as emitted: each keeps its directory
namespace go.github.com.google.go_cmp.cmp.@internal.teststructs.foo1;
partial class foo_package { … }

// and an importer names the directory in its alias; the code using it still reads foo1.X, as in Go
using foo1 = go.github.com.google.go_cmp.cmp.@internal.teststructs.foo1.foo_package;
```

Producer and consumer each derive the spelling from the import path and the package name alone (`packageKeepsDirectorySegment`), so there is no registry and no directory scan. A scheme that disambiguated only on an actual collision would need one: the consumer cannot tell from an import path that a same-named sibling exists. Spelling the directory into the CLASS name instead (`fooꓸfoo1_package`) was declined because every path that parses `*_package` would have to learn the compound form.

Three differences keep today's spelling, because none of them can collide:

| Difference | Example | Why it cannot collide |
|---|---|---|
| a `/vN` tail | `math/rand/v2` → `rand` | the major-version convention: the directory hosts the parent-named package |
| a `name.vN` tail | `gopkg.in/yaml.v3` → `yaml` | the same convention spelled as a suffix |
| case only | a directory `macos` holding `package macOS` | Go's module rules forbid two paths in one module that differ only by case |

`package main` never moves (it is its own executable and is never referenced), and an external test package is decided by the package it tests (`foo_test` → `foo`), so it shares its production package's namespace. Every other difference moves, a hyphenated module directory included: `github.com/mattn/go-isatty` (`package isatty`) is `go.github.com.mattn.go_isatty.isatty_package`.

A persisted `package_info.cs` is copied through verbatim outside its marker sections, so its own `namespace` declaration
and the template's `using static <namespace>.<class>;` line are converged on the current namespace
(`convergePackageNamespace`); otherwise a package that started keeping its directory would carry the old namespace in
that one file. <!-- Ruled 2026-10-04: option 1 of docs/phase4/DESIGN-sibling-package-name-collision.md. The case-only
row's standard-library instance, crypto/x509/internal/macos (package macOS), held through Go 1.24; Go 1.26 renamed the
package macos. --> <!-- The design is [the sibling-package design note](../phase4/DESIGN-sibling-package-name-collision.md) (internal record, unlinked 2026-10-26, OQ-1). -->
Guarded by `siblingPackageNamespace_test.go` (two siblings sharing a name, the four exempt shapes, the convergence) and
the behavioral `SiblingPackageNames` test.

## Generated output path: `$(OutDir)` defers to `$(BaseOutputPath)`

Both project templates (`src/go2cs/csproj-template.xml` and the `-tests` host's `test-csproj-template.xml`) give the generated project a stable default output path:

```xml
<!-- Build outputs are copied to $(OutDir), so this default must also defer to an explicit
     $(BaseOutputPath); otherwise that redirection silently never takes effect. -->
<PropertyGroup Condition="'$(OutDir)'=='' AND '$(BaseOutputPath)'==''">
  <OutDir>bin/$(Configuration)/$(TargetFramework)/</OutDir>
</PropertyGroup>
```

The **`AND '$(BaseOutputPath)'==''` half is load-bearing**. <!-- added 2026-07-26 --> MSBuild copies build outputs to
`$(OutDir)`, and `$(OutDir)` is what the SDK *derives* from `$(BaseOutputPath)` late in the import order — so a template
that pins `OutDir` in the project body outranks any `BaseOutputPath` an outer `Directory.Build.props` sets, and that
redirection is discarded without a warning. The generated project would keep writing to `bin\<Config>\<tfm>\` while
`OutputPath` reported the redirected location, which is exactly how the defect hides.

An unconditional pin would silently defeat three separate isolation intents:

- **`-tests` host output** — `test-csproj-template.xml` sets `BaseOutputPath=bin/tests/` (alongside `obj/tests/`)
  precisely so the test project and the production project that *shares its directory* do not collide. Without the
  condition only the `obj/` half would take effect, and every converted test host would write into its production
  package's output tree.
- **The Native AOT perf publishes** — `src/tests/Performance/Directory.Build.props` sets
  `BaseOutputPath=bin\aot-build\` under its `PerfAot` gate. With an unconditional pin, the AOT publish's *build* step
  would write through `OutDir` into the JIT tree and overwrite the JIT binary with a self-contained,
  `IsDynamicCodeSupported=false` one, which the runner would then measure as the "JIT" column. <!-- It did, before the
  condition: see docs/phase4/DESIGN-iface-shell-caching.md §10. -->
- **End-user layouts** — any converted project consumed under an artifacts/CI convention that sets `BaseOutputPath`
  would be ignored the same way.

Two guards pin this. `TestCsprojTemplateEmitsWellFormedXml` / `TestTestCsprojTemplateEmitsWellFormedXml`
(`src/go2cs/csprojTemplate_test.go`) substitute each template exactly as its emitter does and stream the result through
`encoding/xml`, which enforces the same comment rules MSBuild's loader does — a malformed template breaks the entire
corpus at compile time and is otherwise only visible deep into a full behavioral run. <!-- about 450 s in, at the
change; platform not recorded --> On the measurement side, `PerformanceRunner` reads the JIT binary's
`runtimeconfig.json` before timing anything and **fails the run** if it is self-contained or has dynamic code disabled.

## Per-GOOS sources: layout L3 and `$(GoTargetOS)`

Go selects its platform sources at *build* time — filename suffixes and `//go:build` constraints — so a
conversion does not merely target a platform, it **is** that platform. A converted package whose emission
varies across `GOOS` therefore keeps the varying files in per-GOOS subfolders, and the `.csproj` compiles
exactly one of them; files that are byte-identical on every platform stay flat. This is layout **L3**.
<!-- [`phase4/DESIGN-multiplatform-corpus.md`](../phase4/DESIGN-multiplatform-corpus.md) §8 (internal record, unlinked 2026-10-26, OQ-1) --> <!-- accepted 2026-08-08 -->
`internal/goos` carries it:

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

<!-- source: src/core/internal/goos/internal.goos.csproj:145-147 -->
```xml
<PropertyGroup Condition="'$(GoTargetOS)'==''">
  <GoTargetOS>windows</GoTargetOS>
</PropertyGroup>
```

```xml
  <Compile Remove="**/*.cs" />
  …
  <Compile Include="package_info.cs" Condition="Exists('package_info.cs')" />
  <Compile Include="$(GoTargetOS)/package_info.cs" Condition="Exists('$(GoTargetOS)/package_info.cs')" />
  <Compile Include="*.cs" Exclude="package_info.cs" />
  <Compile Include="$(GoTargetOS)/*.cs" Exclude="$(GoTargetOS)/package_info.cs" />
```

(The `package_info.cs` items compile that file first — a flat package carries only the root one; see
[An import forces the imported package's `init` to run](#an-import-forces-the-imported-packages-init-to-run).)
The include must follow `<Compile Remove="**/*.cs" />`, which would otherwise remove it — MSBuild evaluates
items in document order. The property may sit anywhere (properties are evaluated in an earlier pass) but is
declared above the item that reads it. `windows` is the default because Windows is the corpus's reference
target (the first target of the `-platforms` run below), so a plain `dotnet build` builds the Windows flavor
and `-p:GoTargetOS=windows` produces the same assembly as the property absent; `-p:GoTargetOS=linux` or
`-p:GoTargetOS=darwin` selects another flavor. <!-- history: when L3 landed, the plain build was verified
byte-identical to the single-platform Windows package the layout replaced. -->

**Only a package that varies gets the blocks.** The design measures the varying set as a small minority of
the standard library; the rest <!-- the design's measurement: 37 of 304 --> emit the same C# on every platform and keep
a plain project file.

**Which files are per-GOOS cannot be decided by one conversion.** The platform axis is a comparison of
several targets' *emissions*, and it is emphatically not derivable from Go file sets: some packages emit
different C# from identical Go source (constant folding, escape analysis, cross-file collision renaming,
dead-branch folding), and others emit identical C# from differing Go source. <!-- the design's census: four and three
--> So the layout is produced by a
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
where the compile item set is identical either way, and it breaks the whole Linux build:
`runtime/lock_sema_impl.cs` supplements `lock_sema.cs`, which Go selects on Windows *and* macOS but never on
Linux, so a flat companion compiles in the Linux build against a principal that is not in it. <!-- history:
this took the entire Linux corpus down at L3 increment 3. -->

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
`syscall/dll_windows.cs` and `syscall/exec_windows.cs` replace Windows-**only** sources and live in
`syscall/windows/`. The `.cs.auto` sibling moves with its `.cs` so the pair cannot separate — which is also
what a single-target reconvert already does, since `conversionDriver` routes both through
`platformLayoutPath`. A hand-own whose principal no target emitted (`runtime/managed_impl.cs`,
`internal/poll/runtime_sema_impl.cs` — go2cs machinery with no Go file behind it) has no placement evidence
and is left alone. Two existing copies of one hand-own that disagree are an **error**, never a first-wins
choice: a duplicated hand-own is hand-maintained in each folder, so propagating one over the other is how a
fix applied to a single flavor would disappear.

**A hand-owned FUNCTION carries its platforms in the registry.** `manualConversionFuncs` is keyed by NAME,
and a Go name is not unique across platforms: Go selects one of several files declaring the same function by
build constraint. So every entry carries a GOOS scope (`goosScope`, `manualTypeOperations.go`): the empty
scope means every target, and a scoped entry turns only those targets' declaration into a placeholder.
`os.(*File).readdir` is scoped to windows and darwin, whose flavors hand OS memory to a Go struct, while
Linux's `dir_unix.go` flavor converts as ordinary Go. A scope does not express a per-platform signature:
`notetsleep_internal` is four parameters in `lock_sema.go` and two in `lock_futex.go`, so it is one
every-target entry, and each flavor's own `*_impl.cs` declares its own signature, which L3 routes per GOOS.
<!-- Background in [`phase4/DESIGN-multiplatform-corpus.md`](../phase4/DESIGN-multiplatform-corpus.md) §7 (internal record, unlinked 2026-10-26, OQ-1). -->

Guarded by `platformHandOwn_test.go`, which walks the **real** `src/core` rather than a synthetic tree —
the next offender will be a file somebody adds by hand. Three structural rules: an `*_impl.cs` whose
principal is in some but not all of its package's per-GOOS folders must be in exactly those; a `.cs.auto`
lives beside the `.cs` it reviews; and a source carrying Go's own GOOS filename constraint (`*_windows.cs`,
`*_linux.cs`, `*_darwin.cs`, with an `_impl` suffix stripped first) is never flat in an L3 package. That
third rule exists because a static walk cannot find a *marked* hand-own's principal — it is what catches a
whole-file hand-own such as `dll_windows.cs` or `exec_windows.cs` left flat.

**References are conditioned by the same rule, one level up.** A package's *direct imports* can differ by
`GOOS` too — `os` is the clearest: it imports `internal/syscall/windows` on
Windows and `internal/syscall/unix` on Linux and macOS. <!-- measured at the change: 21 packages --> One flat reference
list cannot say that, so the
references common to every platform stay unconditioned and only the differences are selected:

```xml
  <ItemGroup>
    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
    …the references os has on every platform…
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='darwin'">
    <ProjectReference Include="$(go2csPath)core/internal/syscall/unix/internal.syscall.unix.csproj" />
  </ItemGroup>

  <ItemGroup Condition="'$(GoTargetOS)'=='linux'">
    <ProjectReference Include="$(go2csPath)core/internal/byteorder/internal.byteorder.csproj" />
    <ProjectReference Include="$(go2csPath)core/internal/goarch/internal.goarch.csproj" />
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

**The merge reads only the targets that converted the package.** Each target converts into a full copy of
the seeded tree, so a target that does not build a package still holds the seed's copy of its project file.
Reading that copy would make the seed the base of a package only darwin or linux converts, and would take the
seed's reference list for that target's import set, conditioning references by a GOOS that never builds the
package. So `mergePlatformProjectFile` reads the converting targets alone, and reads every target that holds
the file only when none converted it (a hand-owned package). A project file that carries a per-GOOS block is
re-merged even when every target holds an identical copy (`carriesConvertedPlatformAxis`): a single-target
reconvert keeps the other platforms' groups from the file it overwrites, so identical copies prove nothing,
and the converted-only read drops a group no converting target confirms (`internal/syscall/windows`, which
selects Go files on Windows alone, carries no per-GOOS reference groups). Guarded by `platformProjectConverted_test.go`
(`TestMergeTakesProjectFilesFromTargetsThatConvertedThePackage`,
`TestMergeDropsAPerGOOSAxisNoConvertingTargetConfirms`).

Two facts that are NOT references still have to be reconciled, because one `.csproj` serves every platform:
`<AllowUnsafeBlocks>` is emitted as the **union** across targets (it differs in `os/user` and `syscall`; the
property grants a capability rather than using one, so raising it is inert where unused), and every other
companion artifact — README, icons, `.cs.auto` — is taken from the **first target in `-platforms` order that
converted the package**, the reference flavor first, so the choice is deterministic instead of depending on
which target happened to have something to rewrite. A review sibling in a per-GOOS folder comes from that
GOOS's target, and a companion of a package no target converted from the first target that holds it.

Guarded by `platformProject_test.go`, whose central test is the invariant itself: a single-target reconvert
of every platform must reproduce the merged project file byte for byte.

## Build-warning suppression: what the emitted `.csproj` silences, and what it deliberately does not

Both templates carry one suppression policy, and it is a policy rather than an accretion: every entry is a
diagnostic that is **structural to the Go emission model**, and every diagnostic that is *not* is left
visible on purpose.
<!-- The full census, code by code, is [`docs/phase4/DESIGN-warning-suppression.md`](../phase4/DESIGN-warning-suppression.md) (internal record, unlinked 2026-10-26, OQ-1). -->

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
`EnableTrimAnalyzer=true` **at build time** — so on a library, which is never published, that one line
would be the sole source of every `IL####` warning in the corpus while buying nothing. A converted `main`
package still gets the analysis, and the trimmer re-runs over the whole closure at app publish, where it is
actionable. `PublishReadyToRun` applies to `dotnet publish` alone, so a published converted program starts
from precompiled code while `dotnet run` and `dotnet build` JIT it as usual. `AllowUnsafeBlocks` sits beside
that group but does **not** share its condition: it is a compile setting, and the converted stdlib is full
of library packages that do not compile without it.

Codes deliberately left **visible** are the other half of the policy — `CS0219` (dead named-return locals,
the one static signal for a genuinely dropped assignment to a named result), `CS8778` (a live 32-bit
truncation in `nint`-typed `int64` constants), `CS0675`, `CS8500` (the managed-referent hazard — golib
suppresses it locally, the corpus must not inherit it), `CS8826`, `CS0252`,
`CS0649`, `CS1522`. Each is a converter or golib defect wearing a warning's clothes; suppressing them would
delete the signal rather than fix the emission.

Hand-owned `.csproj` carry the policy by hand rather than by emission — `core/unsafe` and
`core/testing` (skip-listed packages), `core/internal/godebug` (whose only Go file is fully hand-owned,
so `unmarkedFileCount == 0` makes the driver `continue` before `writeProjectFile`), and `core/golib`.
<!-- history: at Go 1.23.12 core/internal/weak and core/internal/concurrent were two more; their Go 1.24
successors weak and internal/sync emit their own. --> golib keeps a *shorter*, deliberately
different list: it is hand-written, it is the reflection/unsafe core, and its trim and nullable warnings
are a real to-do list with an owner rather than emission noise.

Two guards pin all of this in `src/go2cs/csprojTemplate_test.go`:
`TestBothCsprojTemplatesCarryTheSameSuppressionPolicy` asserts the two templates agree on `Nullable` and on
the `NoWarn` set *exactly* (so adding a code forces the design-doc update, and dropping one fails), and
`TestPublishPropertiesAreScopedOffLibrariesButAllowUnsafeBlocksIsNot` parses the rendered project and pins
both halves of the publish-group split — a regression that moved `AllowUnsafeBlocks` under the condition
would break the corpus in a way no warning count would reveal.

## A doc-comment link resolves to a fully-qualified, version-pinned URL

A converted package's `README.md` is its package-level Go doc comment rendered to Markdown, and a Go doc
comment can link. Left to `go/doc/comment`'s defaults, those links come out **site-root-relative**:
`[io.Reader]` renders as `[io.Reader](/io#Reader)`, because `Printer.DocLinkBaseURL` defaults to empty and
`DocLink.DefaultURL` then composes a path from the site root. That is exactly right for pkg.go.dev, which
serves the documentation at its own root, and exactly wrong everywhere this README is actually read:
GitHub resolves `/io#Reader` against `github.com`, Pages/Jekyll against the site root, and nuget.org
against `nuget.org`. The link is dead in all three.

The emitter therefore installs its own `Printer.DocLinkURL` (`renderPackageDoc` in `readme.go`, resolver in
`readmeDocLinks.go`). A standard-library target pins the Go release that produced the conversion —
`https://pkg.go.dev/io@go1.26.10#Reader` — which is the same rule, and the same honesty doctrine, the Docs
badge beside it already follows.

**Completeness is structural here, not a judgement call, because the grammar is closed.**
`go/doc/comment`'s `Text` interface has exactly four implementations — `Plain`, `Italic`, `*Link`,
`*DocLink` — and only two carry a URL:

* **`*Link` URLs are absolute by construction.** Both of the parser's two link sources require a scheme:
  `parseLink` rejects a `[text]: url` definition whose url has no `isScheme(...)://`, and `autoURL` rejects
  inline text on the same test (the accepted schemes are `file`, `ftp`, `gopher`, `http`, `https`,
  `mailto`, `nntp`). A `*Link` therefore *cannot* reach the emitter with a relative URL, and passes through
  untouched — which is also what the "already-absolute URLs are left alone" rule asks for.
* **`*DocLink` is the sole relative-URL producer**, and its own documentation enumerates the exhaustive set
  of five field combinations. `resolveDocLinkURL` answers all five.

| `DocLink` fields | Emitted URL |
|---|---|
| `ImportPath` | `https://pkg.go.dev/io@go1.26.10` |
| `ImportPath`, `Name` | `https://pkg.go.dev/io@go1.26.10#Reader` |
| `ImportPath`, `Recv`, `Name` | `https://pkg.go.dev/io@go1.26.10#Writer.Write` |
| `Name` | `https://pkg.go.dev/<current>@go1.26.10#Name` |
| `Recv`, `Name` | `https://pkg.go.dev/<current>@go1.26.10#Recv.Name` |

The two same-package forms do not occur — the converter leaves `Parser.LookupSym` nil, so `[NewInt]`
stays literal text rather than becoming a link (which is why the corpus is full of escaped `\[Int]`,
`\[Encoder]`, `\[Decode]`: those are not dead links, they are not links at all, and pkg.go.dev shows an
unresolvable name the same way). Answering them anyway is what makes the resolver total against the
*grammar* rather than against today's census, so enabling `LookupSym` later needs no second pass here.

**An external module path is pinned only when the distribution actually pinned it.** A path whose first
element carries a dot is a module, not a std package, and cannot be pinned to a Go release — it is not a Go
release artifact. When GOROOT vendors that exact package, `src/vendor/modules.txt` records the snapshot the
conversion read and the URL states it (`golang.org/x/sys@v0.39.0/cpu#X86`). When it does not —
`golang.org/x/sys/windows` is referenced by std doc comments but is **not** among the x/sys packages GOROOT
vendors — the URL is emitted fully qualified but **unversioned** rather than borrowing the pin from the
module's other vendored packages. A fabricated pin is worse than an unpinned link: the unpinned one still
resolves on all three surfaces, which is the entire defect being fixed. Same degradation the Source·Go
badge makes for the same reason — an unresolvable pin costs precision, never correctness.

A corpus census at the change found relative links of the three `ImportPath` forms only, with no bare-fragment
`#Name` — exactly the distribution the grammar predicts with `LookupSym` nil — alongside many already-absolute
targets that pass through unchanged. <!-- census at the change: 99 relative link occurrences across 38 of 307
emitted READMEs (40 package-only /pkg, 57 /pkg#Name, 2 /pkg#Recv.Name, 0 bare-fragment #Name), against 1,899
already-absolute targets -->

Guarded by `readmeDocLinks_test.go`, which enumerates the five combinations rather than sampling them and
fails on any target that still begins at the site root, plus an end-to-end case over real godoc markup that
asserts both halves of the contract — every doc link qualified, every absolute link untouched.

**Markdown link syntax in a doc comment is not a doc link.** `go/doc/comment` does not support it, so a
comment that writes `[text](url)` renders as an escaped `\[text]` followed by the URL auto-linked in parentheses,
exactly as
pkg.go.dev shows it: the converter reproduces the upstream text faithfully. <!-- history: through Go 1.24
src/core/image/README.md rendered that shape for the Go Security Policy link, from upstream image/image.go's
Markdown link; Go 1.26's image/image.go uses a doc-comment link definition and the README renders an ordinary
link. -->

## The README has TWO emission points, because its Tests badge reads a page the run writes LAST

A converted package's README carries the four-badge line, and the Tests badge is composed at CONVERSION
time from `docs/validation/current/<dot-id>.md`. The run that WRITES that page is the compare at the END
of a `-tests` pipeline (`emitValidationProofPage`). Those two facts are an ordering, and the ordering is
the whole problem: within a single `-test-action all`, the README is always built from the proof page as
it stood BEFORE the run. A package whose counts are unchanged is fine; a package whose counts CHANGE
in that run would be emitted one run behind — a newly validated package would ship a README reading
`Tests-not_yet_validated-orange` **beside its own green proof page**, and need one extra conversion of its
own package to catch up.

Two mechanisms are involved, and they are **not** equivalent. Widening the emission GATE (which
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
nothing and a repeated run stays byte-clean. The corollary is worth carrying: **a `README.md` that moves
during a `-tests` run means that package's validation counts moved.** That is a finding to read, not
emission drift.

Measured on `hash/adler32` with its proof page moved aside to simulate a first validation: one
`-test-action all` validates 2 of 2, writes the page, and emits the green `2/2` badge **in the same run**
— byte-identical to the committed README. A `-test-action compare` over a deliberately staled README leaves it
byte-for-byte untouched, and creates none where the conversion wrote none. (Guarded by
`TestPackageReadmeRefreshFollowsInProcessConversionNotRunMode`, the sibling of
`TestPackageReadmeEmissionFollowsPackageProvenanceNotRunMode` that pins the gate half.)

## Assembly metadata is one derivation chain, and the framework is hoisted to props

Every project in the tree — the two converter templates and the hand-written `golib` and `go2cs-gen` —
carries the same metadata block, in the same order, derived from the same two roots (the two hand-written
projects prefix `Description` with prose of their own, their nuget.org copy, and end with the shared tail):

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

A property set twice is a silent defect: MSBuild keeps the last value, so a second, empty `<Description>`
below the real one ships a package with an empty description *and* an empty `AssemblyTitle`, with nothing
warning and nothing failing to build. <!-- history: before the guards the block had drifted three ways. The
two hand projects spelled Company and Copyright as literals while the template spelled the year with a printf
verb over time.Now().Year(); the test-host template omitted Authors and Copyright; and go2cs-gen.csproj carried a
second, empty Description, so the published go.gen package shipped with an empty description and title. -->

The `Copyright` year is a **literal range**, not a format verb over the current year. A verb would make every
emitted `.csproj` a function of the wall clock: the same converter over the same sources with the same flags
would produce different bytes on either side of New Year's Eve, so the first regeneration of each year would
report the whole corpus as drifted. Determinism is worth more here than an automatically-current year, which
is a once-a-year edit.

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

<!-- source: src/tests/Behavioral/CrossPkgLib/package_info.cs:29, 33, 36 -->
```csharp
// <ExportedTypeAliases>
…
[assembly: GoTypeAlias("Temperature", "go.CrossPkgLib_package.Celsius")]
…
// </ExportedTypeAliases>
```

A consumer that imports the package and names `CrossPkgLib.Temperature` cannot use a C# member-access for it (C# has no namespace-level type alias). Instead, the converter parses the imported package's `package_info.cs`, reads its `[GoTypeAlias]` attributes, and, for each alias an emitted reference resolves through, emits a corresponding `global using` into the consumer's own `<ImportedTypeAliases>` block — keyed by a package-qualified name whose `.` separator is the extended-Unicode dot `ꓸ` (`ꓸ`, a valid C# identifier character), since `CrossPkgLib.Temperature` is not a legal C# identifier:

<!-- source: src/tests/Behavioral/CrossPkgBox/package_info.cs:12, 16, 19 -->
```csharp
// <ImportedTypeAliases>
…
global using CrossPkgLibꓸTemperature = go.CrossPkgLib_package.Celsius;
…
// </ImportedTypeAliases>
```

The consumer's converted code then refers to the alias as `CrossPkgLibꓸTemperature`. (This round-trip depends on the module-aware resolution above to locate the imported package's `package_info.cs`; a stdlib dependency is found under the `core` output tree, a local module via its `go/packages` directory.) Guarded by the `CrossPkgLib`/`CrossPkgUser` cross-package behavioral test pair.

**A collision-renamed alias chain resolves to its concrete target.** When an exported type's name *collides* with a method name it is `Δ`-renamed (see [Type-vs-Method Name Collisions](shadowing.md#type-vs-method-name-collisions)); and when that type is *also* an empty interface — `type Token any` colliding with a `Token()` method, encoding/json's shape — the producer's `package_info.cs` carries a **two-hop chain**. The collision analysis records `Token → ΔToken`, and `visitTypeSpec` (which renders an empty-interface target as `object`) records the renamed declaration `ΔToken → object`:

<!-- source: src/core/encoding/json/package_info.cs:35-36 -->
```csharp
[assembly: GoTypeAlias("Token", "ΔToken")]
[assembly: GoTypeAlias("ΔToken", "object")]
```

A consumer that resolves only the FIRST hop and then qualifies the intermediate `Δ`-name as a package member emits `global using jsonꓸToken = go.encoding.json_package.ΔToken;` — but `ΔToken` is an assembly-scoped `global using`, **not** a namespace member of `json_package`, so it is CS0426 (encoding/json's `Token` consumed by html/template, internal/coverage/cfile, expvar, log/slog, internal/fuzz, …). The imported-alias loader (`loadImportedTypeAliases`) therefore follows the chain within the producer's OWN exported aliases to its **concrete** target, emitting `global using jsonꓸToken = object;`. A chain whose final target is a real `Δ`-renamed member (a delegate/struct such as `ΔFilter`, which is *not* itself an exported alias) stops there and stays package-qualified, unchanged. Guarded by the `CrossPkgLib`/`CrossPkgUser` pair — an empty-interface `Token` colliding with a `Sensor.Token()` method, named as a `var` type in the consumer and its boxed value read back, output-compared vs Go.

**Imported stdlib alias metadata loads from the tree the assembly is COMPILED from.** The alias round-trip reads the
imported package's `package_info.cs` at `$(go2csPath)core/<pkg>`, which is also where every stdlib `<ProjectReference>`
compiles from, so the alias-load path and the compile path are one path by construction. Read from anywhere else, a
cross-package reference to a collision-renamed stdlib type would render the raw, undefined qualified name:
`err.(runtime.Error)` as `runtime.Error` (CS0426) instead of `runtimeꓸError` → `runtime_package.ΔError`. *Metadata must
be read from the tree the code is compiled from, never from a parallel one that merely looks like it.* <!-- history:
while the converted standard library lived in a second tree (go-src-converted, until 2026-08-01), a -tests build
compiled stdlib dependencies from it while loadImportedTypeAliases read package_info.cs from the baseline core stub,
most of which had none, so math/bits' Div overflow and divide-by-zero panic asserts rendered runtime.Error raw. A
project-reference remap and an alias-load derivation from it bridged the two trees; both were deleted with the second
tree. -->

**A same-named cross-package alias target is fully qualified.** Two *different* packages can share a Go package name —
html/template and text/template are both `package template`. When such a package aliases the other's type —
html/template's `type FuncMap = template.FuncMap`, whose target lives in text/template — the alias RHS must name the
target's OWN `(namespace, class)`: `go.text.template_package.FuncMap`. `getFullyQualifiedTypeName` decides its
cross-package branch by package **identity** (`pkg != v.pkg`), matching `getAliasQualifiedTypeName` and
`collectCrossPackagePaths`, so the branch fires and the target fully qualifies. Decided by package *name*, a same-named
foreign type would read as *same-package* and fall through to the `t.String()` path, whose cross-package slash-strip
drops BOTH the `text` path segment AND the `_package` class — `global using FuncMap = go.template.FuncMap;` (CS0234;
`template` is not a namespace of `go`). <!-- history: the branch was gated on pkg.Name() != packageName until this fix;
code-body references were already right, only the global using alias RHS was wrong. --> Guarded by the
`CrossPkgSameNameAlias` behavioral test: a `package atomic` that aliases the same-named `sync/atomic`'s `Int32`
(`type Int32 = atomic.Int32`), whose `global using` RHS must render `go.sync.atomic_package.Int32`, not the
dropped-segment `go.atomic.Int32`.

**A `//go:linkname` VARIABLE pull becomes a forwarding property to the (publicized) remote.** Go's
`//go:linkname local pkgpath.remote` on a bodyless package var aliases `local` to another package's `remote` — SAME
storage, resolved by the linker. math/bits' `//go:linkname overflowError runtime.overflowError` (its `Div` panics with
`overflowError`, a `runtime.Error`) is the standard library's case: emitted as a plain field it would stay null, and the
converted `Div` would panic with `null`. Go 1.23 requires the *definition* side to authorize the pull with a
one-argument handle (`runtime/linkname.go`'s bare `//go:linkname overflowError`); the authorization is puller-AGNOSTIC,
so the faithful C# emission of a handle-marked var is **`public`** (a puller in a separate assembly must reach it) — a
purely local decision each package makes from its own directives, no cross-package coordination. The pulling var then
emits as a **forwarding property** to the fully-qualified remote (resolves in `namespace go;` without a using), and the
remote's package is queued for a project reference — the definition side (`LinknameVarPullLib`, one-arg handle), then
the two-arg pull (`LinknameVarPull`):

<!-- source: src/tests/Behavioral/LinknameVarPullLib/provider.cs.target:7 -->
```csharp
public static @string secret = "pulled across the linkname";
```
<!-- source: src/tests/Behavioral/LinknameVarPull/main.cs.target:9 -->
```csharp
internal static @string secret { get => go.LinknameVarPullLib_package.secret; set => go.LinknameVarPullLib_package.secret = value; }
```

A pull whose direct reference would close a project cycle is instead reached **late-bound**, by name, through
golib's `LinknameVar<T>` — the same storage, with no `ProjectReference`. math/bits is that case at Go 1.26,
because `runtime` imports `math/bits`:

<!-- source: src/core/runtime/panic.cs:344 -->
```csharp
public static error overflowError = ((error)((errorString)(@string)"integer overflow"u8));
```
<!-- source: src/core/math/bits/bits_errors.cs:12 -->
```csharp
internal static error overflowError { [global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields, "go.runtime_package", "runtime")] get => global::go.LinknameVar<error>.Get("go.runtime_package, runtime", "overflowError"); set => global::go.LinknameVar<error>.Set("go.runtime_package, runtime", "overflowError", value); }
```

Three safety gates keep the emission compilable, each narrowing direct forwarding to what C# can express: (1) a handle var is publicized only when its **type is itself publicly accessible** — runtime's `sched` (`schedt`), `writeBarrier` (anon struct), `lastmoduledatap` (`*moduledata`) have unexported types and stay `internal`, since a public member cannot expose a less-accessible type (CS0052/CS0053) and such a var could not be pulled cross-assembly anyway; (2) a pull whose forwarding reference would form a **project-reference cycle** — math/bits pulling `runtime.overflowError` at Go 1.26, where runtime imports math/bits — is reached late-bound through `LinknameVar<T>` as above (Go's link-time linkname has no package cycle; a C# project reference cannot be circular), and a pull of the package's own symbol keeps its plain field; (3) an **address-taken** pull — reflect's `//go:linkname zeroVal runtime.zeroVal` with `&zeroVal[0]` — keeps its addressed-global heap box, because a property has no address (`ᏑzeroVal` would be CS0103). The definition-side handle collection (`collectLinknameHandles`, a package-wide pre-pass like `collectPublicizedTypes`) and the pull recognition live in `linknameOperations.go`. (Guarded by the `LinknameVarPull`/`LinknameVarPullLib` behavioral test pair — a consumer package pulling an unexported handle var from a separate provider assembly, its value printed and output-compared vs `go run`; the late-bound arm by `TestUpwardLinknamePullIsReachedLateBound` (`linknameLateBound_test.go`), which converts a real cyclic pull and reads the emitted source and project file, and golib's `LinknameVarTests`. This is what keeps math/bits' `Div` panic tests from panicking with `null`.)

**Gate (2) asks its question three ways, because the cheapest oracle is not always available.** Reading the `-stdlib`
convert-set graph alone and answering "no cycle" whenever there is no graph — *every* single-package and every `-tests`
conversion, as a batch driver alone builds one — would give one variable two answers with no diagnostic: converting
`runtime` under `-stdlib` would suppress the `CanUseLongPaths` pull, and converting the SAME package under `-tests`
would emit it, where the resulting `runtime -> internal/syscall/windows` reference closes six project cycles (MSB4006)
through Go's own `internal/syscall/windows -> syscall -> runtime`. <!-- history: that shortcut was the converter's
behavior until this fix (W1). --> The assumption behind such a shortcut — that one package alone cannot form a
cross-package cycle — is true of every reference the converter emits *except* this one: all the others descend from an
`import`, and Go's import graph is acyclic by construction, whereas **a linkname edge is the one reference the converter
emits that Go's own graph does not contain**. `linknamePullWouldCycle` answers from, in cost order, the convert-set
graph when a batch driver built one; the current package's own transitive import closure (if this package already
reaches the target, the target cannot reach back); otherwise a memoized `packages.Load` of the pull TARGET, walked for
the current package. A question that *cannot* be answered is answered "yes" and says so on stderr — an unanswerable
cycle question must not be answered "no", because "no" emits a reference that may not compile at all, while "yes" takes
the late-bound arm, which needs no reference. (Guarded by the `TestLinknamePullCycleGuard…` family in
`linknameCycleGuard_test.go`.)

## An UPWARD `//go:linkname` var alias INVERTS its storage instead of giving up

Gate (2) above keeps a cyclic pull compilable, but "compilable" is the whole of what it achieves: the two declarations become two unrelated fields, which is silently *not* what Go's directive says. A `//go:linkname` var alias is a **link-time identity** — `runtime.canUseLongPaths` and `internal/syscall/windows.CanUseLongPaths` are one word of memory, arranged with no import in either direction. C# has no link-time identity, so one assembly must hold the field and the other must reach it through a member reference, which is a **compile-time** edge and must be acyclic. Every aliased pair therefore forces one question:

> **Which side holds the storage?**

The project graph answers it: **storage goes in whichever package the other one already depends on.** `varLinknamePull` always puts it on the right of the two-argument directive, which is correct for a DOWNWARD pull (`math/bits → runtime`) and forms a cycle for an upward one. For the upward case the converter **inverts** rather than degrades — `runtime` keeps the storage (where Go's own write already is) and the isw declaration becomes the forwarding property:

<!-- illustration: not converter output -->
```csharp
// runtime (storage side — publicized by packageVarAccess's alias arm, not by a handle):
public static bool canUseLongPaths;
// internal/syscall/windows (forwarding side, under Go's one-arg handle):
public static bool CanUseLongPaths { get => go.runtime_package.canUseLongPaths; set => go.runtime_package.canUseLongPaths = value; }
```

Inverting costs **zero** new project references here — `isw → runtime` already exists — where the un-inverted direction costs six cycles. That asymmetry is not luck: it is the same fact stated twice, since the side that is already depended upon is by definition the side no new edge is needed to reach.

**It needs a curated registry**, `linknameVarAliasTargets`, for the identical reason `linknamePushTargets` does: converting `internal/syscall/windows`, the converter cannot see runtime's directive. A package is converted from its own syntax, and its dependencies contribute *types, not comments* — so from isw's side a var under a one-arg handle is indistinguishable from any other opened var, and nothing in it names `runtime`. The row records the missing half as a judgment; `linknameVarAliasStorage` is **derived** from it (the `linknamePushSources` pattern) so the publicize arm and the registry cannot drift. Go's authorization is still required — the forwarding side must carry its one-arg handle, so a row that outlives Go's directive fails closed to a plain field rather than inventing an alias — and gate (3) is inherited on the target side, because an address-taken forwarding property would name a `Ꮡ` box that does not exist.

**Forwarding and populating are one change** — the `GetSystemDirectory` rule again. `canUseLongPaths` is written only by `initLongPathSupport`, called only from `osinit`, which the converter emits already marked not-run and whose body bottoms out in `asmstdcall`; so the alias alone would have faithfully forwarded a permanent `false`. A naive "set it true" would be worse than the gap: `os.fixLongPath` would stop adding the `\\?\` prefix on a host where the PEB `IsLongPathAwareProcess` bit was *not* actually set, producing paths that silently fail. The flag is therefore tied to the **outcome**: golib's `InitializeWindowsLongPaths` reads the PEB bit back after writing it and records that observation in `WindowsLongPathsEnabled`, and the hand-owned `runtime/windows/os_windows_impl.cs` copies it into `canUseLongPaths` from a `[ModuleInitializer]` — the same slot, file and pattern as that file's existing `ᴛInitSysDirectory`. (Guarded by `TestRecurseLinknameVarAlias` for the four emission arms, `TestLinknameVarAliasRegistryMatchesGoSource` for both halves of each row against GOROOT, and the `LongPathRoundTrip` behavioral test for the semantics — a >MAX_PATH path round-tripped through `os` and output-compared vs `go run`.)<!-- Design and the corrected root: [`docs/phase4/DESIGN-linkname-push-cycles.md`](../phase4/DESIGN-linkname-push-cycles.md) (internal record, unlinked 2026-10-26, OQ-1). -->

**Exported structs and interfaces cross packages.** An exported struct's fields and methods are reachable on the consumer side exactly as the producer emits them — `CrossPkgLib.Sensor{Name: …, Temp: …}` lowers to a C# constructor call and `s.Name` / `s.Hot()` to field/method access on the imported struct — because the struct and its `this ref` extension methods live in the (referenced) library assembly.

A cross-package **interface satisfaction** is subtler. Go is structurally typed, so a consumer may assign any value with the right method set to an interface; C# requires the *nominal* `partial struct T : I` implementation glue, which the [`ImplementGenerator`](source-generators.md#source-generators) can only add to `T` **in T's own assembly** (`isLocalImplType`). The converter records a `[assembly: GoImplement<T, I>]` for each concrete→interface conversion it *witnesses while converting T's package* — so for a consumer to use `Sensor` as `Labeled` across the assembly boundary, the satisfaction must be witnessed in the **library** that declares `Sensor`. The idiomatic Go interface-satisfaction assertion does exactly this:

```go
var _ Labeled = Sensor{}   // in CrossPkgLib — records GoImplement<Sensor, Labeled> in this assembly
```

With that, the library emits `[assembly: GoImplement<Sensor, Labeled>]`, `Sensor : Labeled` is realized in the library
assembly, and a consumer's `var l CrossPkgLib.Labeled = s` / `CrossPkgLib.Describe(s)` compile as ordinary upcasts. (A
library that returns the interface from a constructor — `func New(...) Labeled { return Sensor{…} }` — witnesses it the
same way.) A type that satisfies one of its own package's **exported** interfaces is recorded even when no cast in the
package witnesses it, within the gates of
[A package records the pairs it SATISFIES](#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses). A
satisfaction no record can cover — an unexported interface, or a dynamic type in an assembly converted after the
interface's own — is resolved at run time by golib's `AdapterBinder` through the interface's duck-typing shells
([Interfaces](interfaces.md#every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert));
a *declared* conversion still takes the nominal fast path. Also guarded by the `CrossPkgLib`/`CrossPkgUser` pair (struct
field access + interface satisfaction). <!-- history: a declaration-site scan that recorded every local
concrete-to-local-interface match was tried in 2026-07 and retired on 2026-07-25 as incomplete and speculative; the
same-package recorder below records exported-interface pairs under five gates. -->

### A sub-package import whose leading segment is a package alias root-qualifies
When a package imports both a parent package and its sub-package — testing/fstest importing `io` **and** `io/fs` — the converter emits `using io = io_package;` (a **type** alias for the io package class) and, for io/fs, a **relative** namespace target `io.fs_package`. In C# the leading `io` segment of `io.fs_package` binds to that type alias, so `io.fs_package[.FS]` resolves to the nonexistent nested type `io_package.fs_package[.FS]` — CS0426 (the `using fs = …` alias line, the embedded `fs.FS` field, and the generated `TypeGenerator` copies). The converter records every direct-import using-alias identifier bound in the package and prefixes `go.` onto any multi-segment relative namespace/type whose leading segment is one of them, so the segment resolves as the child **namespace** it names:

```go
import (
    "io"
    "io/fs"
)
type fsOnly struct{ fs.FS }
```
<!-- source: src/core/testing/fstest/mapfs.cs:7, 222 -->
```csharp
using fs = go.io.fs_package;
…
/*embed*/ public go.io.fs_package.FS FS;
```

The unqualified `io.fs_package.FS` is retained as the `promotedInterfaceImplementations` map **key** (which feeds
alias-less generator files where the relative form resolves). This complements the existing alias-vs-child-namespace
Δ-rename, which only catches `<currentNS>.<alias>` collisions. (It recurs for any parent+sub-package import pair;
`testing/fstest`'s conversion is the standard-library instance, and no single-package behavioral test covers it.)

The same collision detection must see GOROOT-VENDORED namespaces. `visitImportSpec` resolves a GOROOT package's
`golang.org/x/…` import to its on-disk `vendor/…` path (and namespace) when the importing file lives under GOROOT, so
`computeImportAliasRenames` applies `resolveGorootVendoredPath` to each closure path when the package lives under GOROOT
— matching the emission — and builds `packageChildNamespaces` from the vendored paths. Built from the raw `imp.Path()`,
the map would lack a vendored sub-namespace like `go.vendor.golang.org.x.text.unicode`, and `rootQualifyIfAmbiguous`
could not see that a stdlib alias's leading segment collides with it: bidirule (at
`vendor/golang.org/x/text/secure/bidirule`, importing both `unicode/utf8` and the vendored
`golang.org/x/text/unicode/bidi`) would emit `using utf8 = unicode.utf8_package;`, whose `unicode` binds to the in-scope
vendored `unicode` namespace rather than stdlib `go.unicode` (CS0234). With the vendored namespaces in the map the alias
root-qualifies to `go.unicode.utf8_package`. Gated on GOROOT so a user module's own `golang.org/x` dependency is
untouched. No unit or behavioral test covers it: the rule fires only for a GOROOT-vendored package, which the behavioral
harness — never under GOROOT — cannot express; the bidirule reconvert, compared before and after the change, validated
it. <!-- [A/B](../Glossary.md#ab) -->

**A DOTTED build tag (`goexperiment.X`, `amd64.vN`) is matched against the host toolchain's tool tags.** The converter
re-checks each file's `//go:build` constraint after `go/packages` has already loaded it (to drop files for the wrong
GOOS/GOARCH when converting cross-platform). The evaluator resolves a dotted tag by membership in
`build.Default.ToolTags`, in the single `matchTag` callback described next — so an enabled experiment's `_on.go`
survives and a disabled one's `!goexperiment.X` `_off.go` survives, exactly the one `go/packages` chose. An experiment
enabled BY DEFAULT is the case that needs it: its tag is in the host's `go/build` `ToolTags`, so `go/packages` loads its
`//go:build goexperiment.X` `_on.go` file, and a re-check that read a dotted tag as `false` would re-EXCLUDE it,
dropping the package-level consts its importers read (`goexperiment.GreenTeaGC` at Go 1.26; at Go 1.24 it was
`testing`'s `goexperiment.CoverageRedesign`, CS0117 ×4). Only `internal/goexperiment` selects files this way. No test
covers the dotted-tag arm: it depends on the host toolchain's active tool tags, which the `go2cs/*` behavioral harness
cannot express portably; the reconvert A/B (only `internal/goexperiment` changes) and the
[census](../Glossary.md#census) validated it. <!-- history: the evaluator first handled bare identifiers only, so a
dotted tag parsed as a selector and fell through to false; coverageredesign, regabiwrappers and regabiargs were the
default experiments it dropped (Go 1.24). -->

## Build constraints are parsed and evaluated by `go/build/constraint`

The re-check above parses and evaluates constraints with **`go/build/constraint`**, the package the toolchain itself
uses. A constraint is not a **Go expression**: a **Go release tag** such as `go1.21` would read to `parser.ParseExpr` as
the identifier `go1` followed by an illegal `.21` selector, and `//go:build go1.21 && windows` converted for linux would
lose its platform half along with the rest of the expression. <!-- history: the converter hand-rolled a parser over
parser.ParseExpr (lowercased) until this change; every release-tag constraint failed with
`failed to parse build constraint: 1:4: expected 'EOF', found .21`, and conversionDriver's fall-through to including the
file hid it for bare go1.N gates (five go-logr files, found by a -recurse probe of go.opentelemetry.io/otel). -->

* **Recognition** — `constraint.IsGoBuild` / `constraint.IsPlusBuild` decide what is a constraint line. Both require
  the comment at **column zero**, and only the file **header** is scanned (everything above the package clause, line
  comments only), so a `//go:build` quoted in documentation below the package clause does not gate the file it
  describes.
* **Precedence** — a `//go:build` line wins outright; the legacy `// +build` lines apply only in its absence, ANDed
  across lines, with `,` meaning AND and a space meaning OR inside one. A legacy-only file — the norm in third-party
  modules predating Go 1.17, exactly what `-recurse` meets — is therefore constrained as Go constrains it.
* **Evaluation** — `Expr.Eval` drives one `matchTag` callback, so the boolean structure (`&&`, `||`, `!`, parentheses)
  is the stdlib's problem and go2cs owns only "is this one tag satisfied". Tags resolve in three layers: the
  `allowedPlatforms` map (GOOS, GOARCH, the derived `unix`/`posix`, the compiler tags, and every `-tags` value), then
  the Go release tags, then `build.Default.ToolTags` for dotted tags. Matching is **case-sensitive**, as the toolchain
  matches, so a mixed-case `-tags MyTag` is satisfiable.

**Release tags are asked of the go command, not of the compiled-in list.** This is the one place where "use
`build.Default`" — right for `ToolTags` — is wrong. Under `GOTOOLCHAIN=auto` the go command re-execs a **newer**
toolchain when the main module asks for one, so `go/packages` can be selecting files under a newer Go release than the
one whose `build.Default.ReleaseTags` is linked into go2cs (a go2cs built with Go 1.26 stops at `go1.26`). The re-check
would then call the newer release's tag false where the loader called it true and drop the file — *and* its `!go1.N`
sibling was already dropped upstream by the loader, leaving the package with **neither half** of a fallback pair.
Over-exclusion is this pass's recurring failure mode (the `purego` seeding and the `goexperiment` branch above each
exist to undo one) and it is the dangerous direction, because the loader has already applied the full constraint for the
target platform — anything this pass subtracts is real code. So release tags come from `go env GOVERSION` run in the
same directory `packages.Load` uses, expanded to `go1.1`…`go1.N`. An unreachable or unparseable toolchain falls back to
the compiled-in list — never to an empty one, which would make every `go1.N` false.

That subprocess costs ~300 ms on Windows, so it is contained twice. Resolution is **lazy** — `matchTag` tests the `go1.N` *shape* before resolving anything, so a constraint naming no release tag never pays it; and the answer is then cached per **module root**, because GOTOOLCHAIN keys on the module. Both halves earn their place: every behavioral test is its own module, so the cache alone would still spend one lookup per module answering a question not one of those packages asks (no behavioral constraint mentions a release tag), while `-stdlib` reaches the lookup only for the rare standard-library file with a `go1.N` gate, and `GOROOT/src` carries one `go.mod` above every standard-library package anyway. <!-- measured at the change: 569 behavioral modules; sort the only std package with a go1.N gate; 302 std packages --> Note this does not help the **type checker** go2cs links in, which is still whatever release compiled it; a module whose `go` directive exceeds that still needs go2cs rebuilt on a newer toolchain.

Guarded by `src/go2cs/buildConstraints_test.go` (release tags bare/negated/compound, the legacy `+build` grammar,
extraction precedence, loader-toolchain resolution).

## Default build tags: `purego` for the standard library, `safe` for modules

A build tag decides which Go files a build compiles, so the tags a conversion applies decide what the converted code is. go2cs applies two by default, each for one thing converted code cannot do.

* **`purego` for the standard library** (with `math_big_pure_go`, the same switch under math/big's older spelling). Go binds its hottest crypto, hash and big-number routines to `.s` assembly, and C# has no way to run assembly. These tags select Go's own portable implementations instead. They apply to `-stdlib` and to every `-tests` run. [The standard-library conversion applies `-tags purego`](purego.md#the-standard-library-conversion-applies--tags-purego) covers them in full.
* **`safe` for modules.** Some libraries do pointer arithmetic into the runtime's private layout: testify's internal copy of spew adds a computed byte offset to a `reflect.Value` to reach its private flag word. Converted code keeps those values in .NET's own layout, so golib refuses that arithmetic with a panic rather than reading the wrong memory. Such libraries ship a fallback for builds tagged `safe`, and a `-recurse` conversion adds the tag so the fallback is what converts. The cost is the fallback's own: spew cannot print unexported fields, so testify's assertion messages show less of a struct than a default Go build would. The standard library uses the tag nowhere, so it changes nothing there. `-module-safe-tag=false` turns it off.

**The Go baseline uses the same tags.** When `-tests` validates a package, the `go test` run that produces Go's verdicts is given exactly the tags the conversion used (`oracleTestArgs` reads the one resolved set). Both sides therefore compile the same files, and a verdict compares like with like. A module's proof pages and its `MODULE.md` state the tags their verdicts were read under.

**`appengine` is not set**, although some libraries treat it as a second name for "avoid unsafe". It also changes behavior that has nothing to do with unsafe: logrus, for example, switches its terminal check on it. Adding it would change what a converted module does, not only which fallback it takes.

**An explicit `-tags` replaces every default**, `-tags=` included, so a caller can always choose the exact set.

Guarded by `TestResolveBuildTags` (the default for each mode, the switch, and the explicit override), `TestDefaultModuleBuildTagsContent` (exactly `safe`, never `appengine`), `TestOracleTestArgsCarryResolvedBuildTags`, and the proof-page tests for the build-tags sentence.

## An import forces the imported package's `init` to run

Go guarantees an imported package is fully initialized **before** the importing package's own
initialization, for every form of import. A converted Go `init` becomes `[GoInit]`, which
`csproj-template.xml` aliases to .NET's `[ModuleInitializer]` — the right shape and a **weaker
guarantee**: a module constructor runs at first access to something in *its own* module, so an
assembly nothing in the program has touched yet has not initialized. The converter closes that gap by
emitting a hook that forces the imported package's module constructor.

**Blank imports are the obvious case.** `import _ "image/png"`
imports a package purely for the side effects of its `init`, so it is by definition the case that
names nothing, and the observable form is a registry that stays empty. `image/gif`'s `writer_test.go`
blank-imports `image/png` so that png's `init` calls
`image.RegisterFormat` (`image/png/reader.cs`); with the import emitted as a comment alone that init would
never run, `image.Decode` would have no PNG entry, and `TestWriter` would fail with
`../testdata/video-001.png image: unknown format`. <!-- history: it did, as image/gif's only failure at 27 of
28, before blank imports were forced. --> The same
shape gates every registration-by-blank-import consumer: `database/sql` drivers (`sql.Register`),
`net/http/pprof` (its `init` installs the `/debug/pprof` handlers), `image/png` and `image/jpeg` as
decoders for anything calling `image.Decode`, and `time/tzdata`.

**Named imports need it too.** A NAMED
import whose package is referenced only from a *function body* is equally untouched at
module-initialization time, and Go orders it identically. `log/slog` is the case that makes the
difference observable: slog's `init` captures `log/internal.DefaultOutput`, which **`log`'s own
`init`** installs. Without the hook, whichever of the two is touched first wins, so a program that
touches `slog` first would capture **nil** into `defaultHandler.output` — a value that is captured, never
re-read, so `log`'s later initialization could not repair it — and slog's handler would then dereference a
nil func: any converted program that touched `log/slog` before `log` would crash. <!-- history: forcing was
extended from blank to named imports on 2026-08-26, after log/slog's test host crashed this way. -->

**The trigger is "the imported package initializes something, transitively."** Two trigger sets are
sound. Forcing **every** import matches Go exactly and costs a hook per import per
package, corpus-wide. Forcing every import whose module constructor is non-empty *transitively* is
observationally **equivalent** — running an empty module constructor is a guaranteed no-op, the same
reasoning the pseudo-package skip below already applies to `unsafe`/`builtin`/`C` — at a fraction of
the emission. The second is what the converter does.

The fact behind it is computed in process from the loaded package graph (`packageInitFacts.go`)
rather than published as metadata, and that is the one design decision worth stating
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
**hand-owned-by-consequence** packages (`internal/godebug`)
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
ordering silently, and silently is exactly how such a gap would hide. The trigger fails
toward fidelity, never toward the smaller emission.

⚠ **A read-set heuristic cannot substitute.** The tempting narrow rule — "force only the imports whose
symbols the importer's own `init` references" — MISSES this exact case: slog's `init` reads
`log/internal.DefaultOutput`, but the package whose `init` WRITES it is `log`. The dependency that
must be forced is not the one the init statement names.

A blank import still emits **no `using`** — `using _ = <ns>;` would hijack C#'s `_` discard for the
whole file (CS0118 + CS0029 on any deconstruction discard) — so it stays a comment in the importing
file, and every import form, blank included, gets the same generated module-initializer hook, written
once per package into the `<ImportInitializers>` section of its `package_info.cs` class body:

```go
import (
    _ "BlankImportSideEffects/jpeglike"
    _ "BlankImportSideEffects/pnglike"
    "BlankImportSideEffects/registry"
)
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/main.cs.target:4-6 -->
```csharp
// blank import: BlankImportSideEffects.jpeglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: BlankImportSideEffects.pnglike_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using registry = BlankImportSideEffects.registry_package;
```
<!-- source: src/tests/Behavioral/BlankImportSideEffects/package_info.cs:76-81 -->
```csharp
// <ImportInitializers>
[GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸjpeglike() => builtin.initPackage(typeof(BlankImportSideEffects.jpeglike_package));
[GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸpnglike() => builtin.initPackage(typeof(BlankImportSideEffects.pnglike_package));
[GoInit] internal static void initᴛᴛimportꓸBlankImportSideEffectsꓸregistry() => builtin.initPackage(typeof(BlankImportSideEffects.registry_package));
[GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
// </ImportInitializers>
```

The hooks live in the metadata file rather than in each importing file's class body, so that converted
code reads like the Go it came from (`importInitSection.go`).
<!-- design: [`phase4/DESIGN-import-hook-relocation.md`](../phase4/DESIGN-import-hook-relocation.md) (internal record, unlinked 2026-10-26, OQ-1) --> <!-- history: until 2026-09-01 the hooks sat at the top of each importing file's class
body, where they ran before that file's own inits only. -->

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

**The hooks precede every `init` of the package**, because Go orders an imported package's
initialization before the importer's. Roslyn emits an assembly's module-initializer calls in
compilation item order and then declaration order within a file, and every generated project compiles
`package_info.cs` as its FIRST item (its `package_info.cs` `<Compile Include>` items come ahead of the
`*.cs` glob), so all of a package's import hooks run before any of its own `init` functions,
deterministically and across files. The module initializers go2cs-gen writes for its run-time registries only queue each
registration
for golib to run later, so where they fall in this order changes nothing
([Run-time registrations wait for the first lookup that misses](source-generators.md#run-time-registrations-wait-for-the-first-lookup-that-misses)).

**Exactly one hook per (assembly, imported package).** Go initializes a package once per program
however many files import it, and a .NET module constructor likewise runs once per assembly, so the
package's imports are collected across all its files (hand-owned ones included) and each forced
import is one line of the manifest, sorted by the rendered line. The hook's name is
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
inside the package class body, and C# resolves the leading identifier of a namespace-or-type-name by searching
the enclosing type declarations **outward first**. A nested type sharing that identifier therefore
occludes the namespace for the whole class body — while the alias a few lines above keeps working, which
is what makes the failure read like a converter regression somewhere else entirely:

<!-- illustration: not converter output -->
```csharp
using palette = image.color.palette_package;   // namespace scope — resolves

partial class image_internal_test_package {

[GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() => builtin.initPackage(typeof(image.color.palette_package));   // CS0426

internal partial interface image : Image { … }         // ← occludes `image`
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
Over-qualifying would be valid C# but would churn every hook in the corpus; under-qualifying is a hard
CS0426. A census of the converted standard library found the shadow only in `image`'s test variant, which
is why the guard costs the corpus nothing: every site it changes is a site that did not compile. <!--
measured at the fix: 2,147 hook sites across 691 files, 606 behavioral goldens among them; exactly one shadow
site. Since the relocation the internal test variant's hooks land in the test class
(image/package_info_internal_test.cs, package_test_info.cs), both global::-rooted. --> Guarded by
`tests/Behavioral/ImportSegmentTypeShadow` (production scope, compiled and output-compared) and by
`importSegmentShadow_test.go` (the test-local half, the gate's four quadrants, and the rooting shapes).

**Go's pseudo-packages are skipped.** `unsafe` and `builtin` are compiler-provided and have no
initialization at all, and `C` is cgo. `import _ "unsafe"` is the `//go:linkname` ritual, common across
the converted standard library, so forcing it would be a guaranteed no-op emitted for every one of them. <!-- measured
at the change: 67 files -->
They are
answered before the loaded-handle lookup, which matters for `unsafe` specifically: it loads with type
information but **no syntax**, otherwise indistinguishable from a package the loader failed to give
this conversion, which fails open and would force it.

**Only the module constructor is forced — that is exactly the package's `init` functions.** A package's
own package-level variable initializers run in the package class's type initializer (field initializers,
and the ordered static constructor for the vars that must follow Go's initialization order), which the CLR
runs at first access to that class, unchanged from every other import (and the package's own `init`
touching them is what triggers them). A pure-data var go2cs builds lazily is built on its own first read
instead
([A pure-data var is built on its first read](variable-initialization-order.md#a-pure-data-var-is-built-on-its-first-read-never-in-the-static-constructor)).
The residual case is a package whose
registration is a package-level `var _ = pkg.Register(…)` rather than an `init`; closing it means
additionally forcing the package class's type initializer, which is deliberately *not* done here
because it would eagerly run every one of `runtime`'s package-level initializers on behalf of
`runtime/metrics`'s linkname-only import for no measured benefit. No import in the converted
standard library registers that way.

Note the deliberate asymmetry with the trigger above, which counts a non-constant package-level
initializer as initialization. The FACT states what Go runs at init time; the HOOK runs what a .NET
module constructor can run. Making the fact narrower — "only a `func init()` counts" — would tighten
the emission today at the cost of encoding one property of the current emission model into a
Go-level question, so a later change that moved relocated initializers into the module constructor
would silently under-force. The looser fact costs a no-op hook for a package whose only
initialization is variables; it cannot cost a missed one.

The `-tests` emission carries all of this unchanged — the hook is collected by the same import visitor
and written into the test variant's metadata file, so an import in a `_test.go` file (which is where
`image/gif`'s blank import is) forces from the test assembly. Under the **recompile** model the external variant's
import of the *package under test*
binds a class compiled into that same assembly, so there is no separate module constructor and no
hook; under the two **production-reference** models there genuinely is one, and the ordinary
per-import hook forces it (`iter`'s `package_test_info.cs` carries a real `initᴛᴛimportꓸiter()`). What that
does **not** cover is the subject of the next subsection.
Guarded by the `NamedImportInitOrder` behavioral test — four packages in the reduced `log/slog` shape,
where `store` holds a value and initializes nothing, `writer`'s `init` writes it, `reader`'s `init`
CAPTURES it, and `main` touches only `reader`, from a function body. Go prints
`written-by-writer-init`; without the hooks the converted C# would print the empty string. It also pins the
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
so without a hook the production `init` runs at the first **touch of a production symbol** — which may be the second
test, or the tenth, or never. A test that only observes an `init` **side effect** therefore sees
nothing at all when it happens to run first.

An **external** test file (`package foo_test`) writes `import "foo"`, so the per-import hook already
forces it. An **internal** one (`package foo`) is *in* the package and imports nothing of it: there
is no import spec for `writeImportInit` to hang a hook on, and nothing else forces the module.

`net/http/pprof` is the shape that makes it observable. Its whole mux surface is
installed by its `init` (`http.HandleFunc("/debug/pprof/", Index)`, …), its test file is `package
pprof`, and `TestDeltaProfile` is the only test that goes through a real `httptest` server rather
than calling handlers directly, so without the hook it gets **404** whenever it runs before any test
that touches a production symbol. A suite that happens to touch something first passes by luck of
order, which is what makes the gap easy to miss. <!-- history: found 2026-08-29; measured over eight
shuffled runs that split four/four exactly on order, plus the unshuffled pipeline (where TestDeltaProfile
sorts first), nine observations. With init forced, TestDeltaProfile went skip -> infrastructure-error on the
pprof_mutexProfileInternal stub that already fails TestHandlers//debug/pprof/mutex; the row read 6 of 15
either way. -->

The remedy is one hook, seeded into `package_test_info.cs`'s anchor class by
`referenceModelTestPackageInfoSeed`, using the same `RunModuleConstructor` mechanism as the import
hooks:

<!-- source: src/core/net/http/pprof/package_test_info.cs:61-63, 97-104 -->
```csharp
[GoPackage("pprof")]
public static partial class pprof_internal_test_package
{
    …
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
detected shadow. The import hooks are conditional because over-qualifying would churn every hook in the
corpus; here there is exactly one site per test project and it is new, so the collision-proof
spelling costs nothing and needs no `forcingTargetShadowed` analysis to stay correct.

**The residual ordering nuance is stated, not engineered around.** Roslyn orders a module's
initializers lexically and the tests csproj sorts its compile items by name, so a test file sorting
before `package_test_info.cs` has its own `init` run before this hook. That is the same guarantee the
converter already declines to make *across* files of one package, and it does not touch the property
the hook exists for: every module initializer runs before `Main`, hence before the first test.

**A deferred `init` can mask a capability gap.** An ordering defect that hides a stub reads as a
*skip*, the least alarming verdict there is; forcing the `init` changes the shape of such a divergence,
never its existence.

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
be, since skipping the source deployment is the whole point of the mode. A scrape alone would then find
nothing and fall through to `foreignDerivedTypeAliases`, which recovers *some* aliases from the
dependency's own Go declarations and **no implement records at all**. Two things would go wrong, both in
the CONSUMING package:

* its `<ImportedTypeAliases>` block would lose the dependency's exported aliases
  (`global using syscallꓸHandle = go.syscall_package.ΔHandle`, `netꓸAddr`, `timeꓸLocation`, …); and
* not knowing that `syscall`'s assembly already implements `error` on `syscall.Errno`, it would
  **re-declare the pair locally** — `[assembly: GoImplement<syscall_package.Errno, error>]` — and wrap the
  value at the cast site in a locally-generated adapter (`new syscall_Errnoᴠerror(e)`) instead of
  converting implicitly.

`golang.org/x/sys/windows` shows both at once: it Go-aliases `type Errno = syscall.Errno`, so the two
spellings `Errno` and `syscall_package.Errno` would each be recorded, resolve to the same type, and make
`ImplementGenerator` emit the `syscall_Errnoᴠerror` adapter **twice** — CS0102 / CS0111 ×5 / CS8646 —
which is enough to make the README's `fatih/color` walkthrough unbuildable under `-recurse=nuget`.

**Note what is NOT the cause.** The `go2cs-gen` generators are not reference-kind sensitive: a real
MSBuild build hands a `<ProjectReference>` to the compiler as a `PortableExecutableReference` exactly
like a `<PackageReference>`, and the generators read foreign *type shape* from symbols either way (see
`FindTypeSymbol`). Both modes run the same generator over the same kind of reference; what can differ is the emitted C#
they are given, because that comes from the **converter's** cross-package knowledge.

**The remedy.** The exported metadata is a static, per-package property of the *published* assemblies,
so it travels with the converter. `internal/genstdlibmeta` captures the `<ExportedTypeAliases>` section and
every `GoImplement` record of every `src/core/**/package_info.cs` into
`src/go2cs/stdlib-metadata.txt`, which `stdlibMetadata.go` embeds via `//go:embed`. When a
stdlib dependency's `package_info.cs` is absent, the converter reads its recorded lines through the very
same parsers (`parseExportedTypeAliasLines` / `parseExportedPointerImplementLines` /
`parseExportedValueImplementLines`, which take lines rather than a file) and proceeds identically. The
asset is generated from the same tree `push-nuget.ps1` packs, so the embedded record and the published
assemblies are always one commit's output.

Reading the record is gated on `PackageInfo.PublishedStdLib` — `isStdLib && nugetRefs && !convertStdLib` —
and this gate is a **soundness precondition, not conservatism**. The record describes the converted
standard library at `src/core`; substituting it is only correct when that is what the build actually
references. Under `-recurse=nuget` that is guaranteed. A `$(go2csPath)` deploy root may hold an older or
partial staging whose exported surface differs, and the stdlib
self-conversion is *building* the assemblies being published. Both keep the derive-from-declarations
fallback. Because `-recurse=nuget` is off by default, no other conversion path changes — every behavioral test
re-converts to its committed output byte for byte.

The result is exact rather than approximate: converting the README's `colordemo` with `-recurse=nuget`
against an empty `$(go2csPath)` emits **byte-identical** C# to the same app converted with
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
linux → `linux-x64`, darwin → `osx-x64` or `osx-arm64` by the target's arch, otherwise unset;
`compileRuntimeIdentifierForTarget`), which `go.lib.targets` honors before `$(RuntimeIdentifier)` or the
build host. The compile flavor therefore follows the platform the tree was CONVERTED for, not the machine
doing the build, so a linux conversion built on windows, or a windows conversion built under WSL, pairs each
flavor's metadata with its own assembly. Background: `docs/phase4/DESIGN-multiplatform-corpus.md` §12.
<!-- the 2026-09-24 amendment of that section -->
Guards: `TestStdLibExportedMetadataSelectsTheTargetFlavor`, `TestStdLibExportedMetadataReadsAFlavorOnlyRecord`,
`TestStdLibMetadataCollectFlatWins`, `TestRecurseNuGetImportsTheTargetFlavorsAliases` (an emission arm) and
`TestRecurseNuGetPinsTheCompileRidToTheTarget` (the props emission).

**Residual limitation.** The asset is regenerated by `go generate .` from `src/go2cs`, so a change to the
converted standard library must be committed into `src/core` before it reaches
`-recurse=nuget` consumers. `TestStdLibMetadataInSync` makes that a test failure rather than a silent
mismatch, but it can only compare against the *committed* tree — it cannot detect that the committed tree
is itself older than the published packages.

## A foreign implement record is keyed in ONE spelling, and a VALUE one is trusted only for a partial struct

The record scraped above answers one question at a cast site: *does the dependency's own assembly
already implement this pair?* If it does, the bare value converts implicitly and a local
`<pkg>_<T>ᴠ<Iface>` value adapter is dead machinery. Getting the answer wrong in either direction is
expensive, so both halves — the KEY and the TRUST — are stated precisely here.

**The key.** `implementRecordKey` composes `<declaring package>|<C# simple type>|<pkg>_package.<Iface>`
and is called by BOTH sides of BOTH record sets: `loadPackageImplementLines`, over records parsed from a
dependency's `package_info.cs`, and the value arms *and* the foreign-pointer arm of
`convertToInterfaceType`, over a cast being converted. That it is one function is the whole point —
composed independently, the two sides would use different alphabets and agree only when the
dependency's import path is a single segment:

| dependency | load side | use side | |
|:--|:--|:--|:--|
| `io` | <code>io&#124;noBody&#124;io&#95;package.ReadCloser</code> | <code>io&#124;noBody&#124;io&#95;package.ReadCloser</code> | match |
| `encoding/binary` | <code>binary&#124;bigEndian&#124;binary&#95;package.ByteOrder</code> | <code>binary&#124;bigEndian&#124;encoding.binary&#95;package.ByteOrder</code> | **miss** |
| `image/color` | <code>color&#124;ΔRGBA&#124;color&#95;package.Color</code> | <code>color&#124;RGBA&#124;image.color&#95;package.Color</code> | **miss** |
| `text/template/parse` (ptr) | <code>parse&#124;ListNode&#124;parse&#95;package.Node</code> | <code>parse&#124;ListNode&#124;text.template.parse&#95;package.Node</code> | **miss** |
| `go/types` (ptr) | <code>types&#124;TypeName&#124;go.types&#95;package.Object</code> | <code>types&#124;TypeName&#124;types&#95;package.Object</code> | **miss** |
| `image` (ptr) | <code>image&#124;ΔRGBA&#124;image&#95;package.Image</code> | <code>image&#124;RGBA&#124;image&#95;package.Image</code> | **miss** |

Two divergences, and the second is easy to miss because it only shows on a collision-renamed type.
(1) The INTERFACE side: a parsed record names the recording package's own interface BARE and a foreign
one whole (`go.image.color_package.Color`), while a cast site always renders the full namespace chain.
`canonicalImplementRecordIfaceName` drops everything ahead of the `<pkg>_package` segment, so both reduce
to `color_package.Color`; a member path under the class (`y_package.Outer.Inner`) survives intact. The
package CLASS must stay — the simple name alone collides, and image's `Paletted`→`image.Image` record
must not satisfy a `Paletted`→`draw.Image` cast. Note that neither side is reliably the *longer* one, so
a "strip the chain" heuristic would not do: `go/types` records its OWN `Object` fully qualified where the
cast site renders it short, while `text/template/parse` records its own `Node` bare where the cast site
renders it whole. Which spelling a file produces depends on its own using/alias context — which is
exactly why a canonical form, and not either side's raw text, is the key. (2) The TYPE side: a record
carries the EMITTED C# name, so image/color's `RGBA` (collision-renamed against its own `RGBA()` method)
is `ΔRGBA` there, and a use side naming the GO type would miss it, so both sides reduce to the emitted name.

The DECLARING-package component is what keeps a record honest. A package may record a value pair for a
type declared in a THIRD assembly — `image` re-declared all of image/color's models — and go2cs-gen
realizes that as a local adapter class, not as the type implementing the interface. The use side names
the TARGET's package, so such a record can never satisfy a cast (`image|Alpha|…` against
`color|Alpha|…`).

**The trust.** A record says the declaring assembly implements the pair; it does not say HOW.
`ImplementGenerator` makes every named Go type a `partial struct T : Iface` that really does implement
it — struct, slice (`partial struct Palette /*[]Color*/`), map, channel, numeric
(`partial struct ΔSignal /*num:nint*/`) — with exactly one exception: a named FUNC type arrives
as a C# **delegate**, which cannot be a partial struct, so its `TypeKind.Delegate` arm emits an adapter
CLASS in the declaring assembly instead. `valueRecordRealizesAsPartialStruct` gates on the target's Go
underlying being a non-`*types.Signature`, at the use site where `go/types` can still see it. Without
that gate a consumer hands a bare delegate to an interface slot — CS0029 for net/http's
`HandlerFunc` → `ΔHandler` in `expvar`, `net/http/cgi` and three more.

**The POINTER set shares the key and needs no trust gate.** `[assembly: GoImplement<T, Iface>(Pointer =
true)]` is not "the declaring assembly implements this somehow" — it is exactly the shape
`ImplementGenerator` realizes as the public adapter class `<T>ж<Iface>`, so the record's existence *is*
the answer and there is nothing further to ask. (The delegate hazard that forces
`valueRecordRealizesAsPartialStruct` on the value side cannot arise: a pointer record already means the
adapter route was taken.) Matching a foreign record does suppress a LOCAL record, but whether the consumer
needs that local record is a *realization* question, not a *key* question, and on the pointer side the
answer is always no. <!-- history: an earlier ruling kept this set's key un-collapsed for that reason. -->

One consequence comes with the key, and only the collision-renamed types reach it: a foreign
type whose name is Δ-renamed resolves through a whole-TYPE `global using` alias (`imageꓸRGBA =
go.image_package.ΔRGBA`), which is a single identifier rather than a path — and the adapter is a MEMBER
of the declaring package's class, so composing onto the alias names nothing (`imageꓸRGBAжImage`, CS0246
×11 across five packages). The foreign-adapter arm therefore rebuilds a dotless base as the file's
package qualifier plus the type's EMITTED simple name — `image.ΔRGBAжImage`, which is exactly what the
declaring assembly's generator composed (`image/image.cs` reads `new ΔRGBAжImage(…)` for its own casts).

Every repointed construction is the same edit — `new <pkg>_<T>ж<Iface>(x)` becomes `new <pkg>.<T>ж<Iface>(x)`,
the declaring package's own adapter.
<!-- measured at the change (not re-measured by any guard):
**Pointer footprint,** from a whole-stdlib A/B with both roots seeded (304/304 converted per side):
31 files, **66 constructions**, every one the same edit — `new <pkg>_<T>ж<Iface>(x)` becomes
`new <pkg>.<T>ж<Iface>(x)`, the declaring package's own adapter — plus the 37 `(Pointer = true)` records
that existed only to generate those local classes. Zero additions anywhere and the total adapter-
construction census is unchanged at 4348, so it is a one-for-one redirection, not a removal. By
declaring package: `text/template/parse`→`Node` 33, `go/types`→`Object`/`ΔType` 20, `image`→`Image` 4,
`net/http`→`RoundTripper`/`ΔHandler` 4, `net/url`→`error` 2, `net/textproto`→`error` 1,
`go/internal/srcimporter`→`types.Importer` 1, `go/build/constraint`→`Expr` 1. `go2cs-stdlib.slnx`
builds 0 errors on the overlaid tree. -->

The pointer form's symptom is milder than the value form's. The generated adapter's `Equals` compares
`IжAdapter.Box` by reference, so a redundant local adapter and the declaring assembly's own one still
compare equal and still alias the same object, with no observable divergence. What is wrong
is duplication plus a **non-deterministic dynamic type**: each adapter's module initializer calls
`AdapterRegistry.Register(typeof(ж<T>), typeof(Iface), …)`, which is first-wins, so which assembly's
class a type-assert re-wraps into depends on assembly load order. The value form's second-identity
failure (`image/png`'s `%v`, below) is the same defect one degree worse.

Every suppressed construction is the same edit — `new <pkg>_<T>ᴠ<Iface>(x)` becomes `x`. A pair the declaring
package satisfies but never casts has no record to match, so the local adapter stays its only realization;
the next section closes that.
<!-- measured at the change (not re-measured by any guard):
**Footprint,** from a whole-stdlib A/B with both roots seeded (302/302 converted per side): 13 files,
every changed line the same edit — `new <pkg>_<T>ᴠ<Iface>(x)` becomes `x` — plus the 16
`[assembly: GoImplement]` records that existed only to generate those adapters. **497 constructions**
go away (472 of them in `image/color/palette`'s two palette literals); the rest of the corpus adapter
census is identical count for count, `HandlerFuncᴠΔHandler` included. Two survivors are instructive
because they are NOT this defect: `color.Palette`→`color.Model` (5) and `encoding/binary`'s
`bigEndian`/`littleEndian`→`ByteOrder` (79) have no record to match at all — neither package ever
converts that pair itself, so nothing writes the record and the local adapter is the only realization.
A pair a package satisfies but never records is a separate root, closed by the section below. -->

This is not merely a wasted allocation. The adapter is a **second identity** for one Go value: `reflect`
and `fmt` see the wrapper where the Value's own type says the wrapped struct, which is how it surfaced —
`image/png`'s `diff` printing `%v` of a `color.Color` died with `System.ArgumentException: Field 'R' … is
not a field on the target object which is of type 'go.image_package+color_NRGBAᴠColor'`. (Guarded by the
`ForeignValueImplementSuppression` behavioral test — a sibling package at a multi-segment path that
converts its own values, a collision-renamed implementer, a second implementer, and a named FUNC type as
the live negative: the converter emits the func's adapter alone. `ValueAdapterDynamicType`'s sibling
never converts its pairs, and the declaring side records pairs it merely satisfies (next section), so its
assertions prove the bare value. <!-- history: without the key fix the ForeignValueImplementSuppression shape
emitted five adapters; ValueAdapterDynamicType kept four real adapters until the same-package recorder landed. --> The
pointer form
is guarded by `ForeignPointerImplementSuppression`, whose sibling `tone` self-converts a
collision-renamed `*Tone` and an ordinary `*Plain` — both must reference tone's own adapters — against
a live negative that must keep minting its own: `shade.Level`, an interface with the same SIMPLE name as
`tone.Level` (`*Lone`, a pair `tone` satisfies but never casts, is covered by the pointer-set recorder
below). Unit-guarded by
`TestImplementRecordKeyBothCompositionsAgree`,
`TestImplementRecordKeyKeepsPackageClassDiscrimination` and `TestValueRecordRealizesAsPartialStruct`.)

## A package records the pairs it SATISFIES, not only the ones it witnesses

Every `[assembly: GoImplement<T, Iface>]` the converter writes comes from a **cast it converted** —
`convertToInterfaceType` records the pair it just emitted. Go satisfies an interface **structurally**,
so a package can implement one of its own interfaces completely and never write a conversion:
`encoding/binary` declares `type bigEndian struct{}` with the whole `ByteOrder` method set and exports
`var BigEndian bigEndian`, with no `var _ ByteOrder = BigEndian` anywhere. No cast, no record — so
without more `binary_package.bigEndian` would be a partial struct that does **not** implement `ByteOrder`,
and every consumer would mint its own `binary_bigEndianᴠByteOrder` adapter. This is the one place where
*the declaring assembly implements this pair* is TRUE in Go and FALSE in the emitted C#.

`recordSamePackageImplements` (`samePackageImplements.go`, called from `processConversion` after
the file visits and before `writePackageInfoFile`) walks the package scope and records the VALUE-form
pairs the package satisfies. `encoding/binary`'s metadata carries:

<!-- source: src/core/encoding/binary/package_info.cs:43-50 -->
```csharp
// <InterfaceImplementations>
[assembly: GoImplement<bigEndian, AppendByteOrder>]
[assembly: GoImplement<bigEndian, ByteOrder>]
[assembly: GoImplement<littleEndian, AppendByteOrder>]
[assembly: GoImplement<littleEndian, ByteOrder>]
[assembly: GoImplement<nativeEndian, AppendByteOrder>]
[assembly: GoImplement<nativeEndian, ByteOrder>]
// </InterfaceImplementations>
```

and every consumer hands over the bare value, with no record or adapter of its own — `debug/dwarf`'s
`d.Value.order = binary.BigEndian`, and `crypto/x509`'s `crypto.SignerOpts signerOpts = hashFunc`.

It records **through** `convertToInterfaceType` with an EMPTY expression — the record-only probe path
`convCompositeLit` / `convTypeAssertExpr` / `visitValueSpec` already use, since every emission arm is
gated on `exprResult != ""`. That is the whole design: a synthesized pair is composed, keyed and pruned
exactly as a real cast would compose, key and prune it, so no second naming path can drift from the cast
site's — the divergence the FOREIGN lookup's shared key exists to prevent. Scope names arrive sorted, so
the record order is deterministic across runs.

**Five gates bound it, and each one is load-bearing.**

* **The interface is EXPORTED.** A record is a CROSS-ASSEMBLY contract — it exists so another
  assembly's cast can drop its local adapter — and no other assembly can name an unexported interface,
  so a record for one could never be consulted. The package's own casts already record what it needs
  internally.
* **The target's underlying is NOT a `*types.Signature`.** A named FUNC type is a C# delegate, which
  cannot be a partial struct, so `ImplementGenerator` emits an adapter CLASS for it; a consumer trusting
  THAT record hands a bare delegate to an interface slot (CS0029 — net/http's `HandlerFunc` → `ΔHandler`).
  This is the declaring-side half of `valueRecordRealizesAsPartialStruct` above.
* **Neither side is GENERIC.** A type argument cannot appear in an assembly-attribute type argument
  (CS0246) — the same exclusion `convertToInterfaceType`'s `targetIsOpenGeneric` makes.
* **Both sides are declared in a file this run CONVERTS.** A package scope holds every file's
  declarations, including build-constraint-excluded ones, and a record naming a type no emitted file
  declares is CS0246.
* **Every interface method is REALIZABLE by the generator** — it resolves on the type itself or
  through at most ONE embedded field (`types.LookupFieldOrMethod` index length ≤ 2).
  `ImplementGenerator` forwards a promoted member through a single embed hop and says so ("Go's
  promotion ambiguity rules make multi-embed satisfaction rare; extend when needed"), so a deeper
  promotion emits a forwarder through the WRONG hop: `CrossPkgUser`'s `rig` embeds
  `CrossPkgLib.Device`, which embeds `Sensor`, where `Label` lives, and
  `CrossPkgLib_package.Label(this.Device)` is CS1503 for want of `this.Device.Sensor`. Promotion
  through an embedded INTERFACE is the common shape this still admits (`sort`'s `reverse` embeds
  `Interface`; `debug/macho`'s segment types embed `LoadBytes`). The bound is deliberately
  CONSERVATIVE rather than a model of the generator's exact reach — it withholds a few stdlib
  records (such as `net`'s `tcpConnWithoutReadFrom`/`tcpConnWithoutWriteTo`→`Conn`, whose `*TCPConn` hop the
  generator's `embedHopDeepPaths` arm can in fact follow), none of which has a consumer, and
  withholding a speculative record is always safe: the consumer keeps its local adapter.

The gates bind only the SPECULATIVE recorder. A pair the source actually casts is DEMANDED and still
records at its cast site — promotion depth and all — so none of this narrows what a cast records.

The **POINTER** method set — `types.Implements(*T, Iface)`, the far larger set, whose records are
adapter-class *existence* signals with a different trust rule — records under its own rules; see the next
section.

<!-- measured at the change (not re-measured by any guard):
**Footprint,** from a whole-stdlib A/B with both roots seeded (302/302 converted per side, 3690 files
compared CRLF-normalized): **68 files**, split evenly between metadata and code. **33 records** appear
across sixteen declaring packages; **31** go away — three dropped by the existing interface-inheritance
prune because a newly recorded pair subsumes one a cast had recorded (`flag`'s `textValue`→`Value` under
`Getter`, and `net`/`runtime`'s `errorString`→`error` under their own `ΔError`, which embeds it), and
twenty-eight consumer-local foreign records that existed only to generate an adapter. **89 adapter
constructions** disappear across 34 files — exactly the census the previous section predicted, pair for
pair: `binary_bigEndianᴠByteOrder` 43, `binary_littleEndianᴠByteOrder` 36, `color_PaletteᴠModel` 5,
`crypto_HashᴠSignerOpts` 5. Every changed consumer line is the same edit, the adapter construction
unwrapped to its argument; the full stdlib solution builds with 0 errors. -->

Many of these records have no consumer — they are the rule stating what Go already says
(`sort`'s `reverse`→`Interface`, `image`'s `Rectangle`→`RGBA64Image`, `debug/macho`'s five `Load`
implementers, `io`'s `discard`→`StringWriter`), and they cost one assembly attribute each. Notably
ABSENT is `net/http`'s `HandlerFunc`→`ΔHandler`: the delegate gate holds on the corpus instance that
motivated it.

Guarded by the `SamePackageImplementNoWitness` behavioral test — a sibling `ledger` package that
declares an exported interface and value implementers and never converts one to the other, with its
negatives live rather than asserted (a named FUNC type, an unexported interface, a generic; the
pointer-only implementer is a positive under the next section). The converter mints one adapter there,
the delegate negative's (`ledger_MeterᴠMetric`). `CrossPkgLib`/`CrossPkgUser` and
`ValueAdapterDynamicType` carry the same shape and pass the bare value. The realizability gate
is guarded by COMPILE rather than by a golden, and by the corpus case that found it: `CrossPkgUser`'s
`rig` is the depth-2 promotion, so dropping the gate puts a `GoImplement<rig, Labeled>` back and the suite
goes red on CS1503 in the generated forwarder.

## The POINTER method set records the same way, for a different contract

The section above covers the VALUE half of "the declaring assembly implements this pair is TRUE in Go and
FALSE in the emitted C#"; this one covers the POINTER half.

**Why it is not just "the same rule with a bigger set."** A value record and a pointer record are consumed
differently, and the difference decides both the gates and the failure mode. A VALUE record licenses an
IMPLICIT conversion: the declaring assembly's `partial struct T : Iface` means a consumer hands over the
bare value and names nothing. A POINTER record is an adapter-class EXISTENCE signal — `Pointer = true` is
exactly the shape `ImplementGenerator` realizes as `<T>ж<Iface>` — and the consumer CONSUMES it by NAME,
emitting `new pkg.TжIface(x)` where it would otherwise mint its own `pkg_TжIface`.

**Why cast-site sourcing is not good enough.** A record sourced only from casts lives or dies with the
ONE body that happens to witness it. `syscall`'s three `Sockaddr{Inet4,Inet6,Unix} → Sockaddr` pairs are
witnessed by exactly one method body, `(*RawSockaddrAny).Sockaddr`, so hand-owning that single function
would silently drop all three `(Pointer = true)` records, and a reconvert of `net` would mint
`syscall_SockaddrInet4жΔSockaddr` beside `syscall`'s own. Nothing would fail to compile. The pointer form's
symptom is milder than the value form's `%v`-over-the-wrapper crash — both adapters wrap the same box and
compare equal — but it is a NON-DETERMINISTIC dynamic type: each adapter's module initializer calls
`AdapterRegistry.Register` first-wins, so which class a type assert re-wraps into follows assembly load
order. Sourcing the record from the METHOD SET makes the pair independent of which bodies a run converts,
which is the root fix rather than a rule about what may be hand-owned.

`recordSamePackageImplements` therefore asks BOTH
questions of every candidate and records both forms. The pointer set is a SUPERSET of the value set, so a
value-satisfied pair is recorded twice, and that is deliberate: Go's `T` and `*T` are two dynamic types,
realized as the partial struct and the adapter respectively, and dropping the pointer record for such a
pair leaves a consumer's `var i Iface = &t` with nothing to reference.

**The gates are the same five, plus one.** The added gate is the trust rule made mechanical:

* **BOTH sides are EXPORTED** (`pointerRecordIsPubliclyRealizable`). `ImplementGenerator` scopes the
  adapter class `public` only when the struct and the interface are each public, `internal` otherwise. A
  record is a cross-assembly contract, and this form's contract is *"this class exists and you may name
  it"* — so a record naming an unexported participant advertises a class no consumer can reference
  (CS0122), an existence signal that is a lie. The value form needs only the interface gate because its
  contract is realized by a conversion that names nothing, which is why the two rules differ here and only
  here.

The realizability gate is not merely re-asked of `*T` — it is TIGHTENED, and that is the second place
the two forms genuinely differ. `generatorCanForwardPointerMethodSet` requires every interface method to
resolve **DIRECTLY** on the type (index length 1), admitting no promotion at all, where the value bound
admits one embed hop. A partial struct's explicit implementation resolves a promoted member the way the
converter's own call sites do; the ж adapter does not. Its promoted-member arms are keyed on embedded
**POINTER** fields (`GetEmbeddedPointerHopNames`), and with exactly one such field the single-hop arm
takes every unbound member *unconditionally* — which the generator says outright, and is right to, because
for a DEMANDED record that member's promotion is what type-checked the cast. For a SPECULATIVE record it
is not: the member's true source may be a different embed entirely.

`StructPointerPromotionWithInterface`'s `MyCustomError` is the corpus instance. It embeds BOTH the
`Abser` interface and `*MyError`; `Abs` is promoted from the INTERFACE, but the adapter's lone pointer
embed is `*MyError`, so a speculative record would make the generated forwarder bind `Abs` against
`MyError` — where the only candidate in scope is `time.Abs(Duration)`: **CS1929**, in a generated file,
naming `time` from a test about struct promotion. **Depth is not the discriminator** (that
promotion is index length 2, which the value bound admits); the KIND of hop is, and modelling the
generator's exact hop selection inside the converter would duplicate its internals in a second place —
the very drift this recorder's design exists to prevent. So the bound is conservative in the same spirit
as the value one and safe in the same way: withholding a speculative record leaves the consumer with its
local adapter. A pair the source actually CASTS is untouched and keeps the full promotion
support the generator was built for, which is what that behavioral test guards. A named FUNC type is
excluded before either question is asked.

**One relaxation: an interface with an UNEXPORTED method.** Go lets only the declaring package implement such
an interface, so a consumer's locally minted ж adapter cannot even name the member it would forward and
answers a zero value instead. For that interface the record is not an optimization but the only correct
realization, so the pointer form admits the value form's one-hop bound there
(`interfaceHasUnexportedMethod`, `samePackageImplements.go`). `text/template/parse`'s `*BranchNode`→`Node`
(whose `Type`/`Position` are promoted from embedded `NodeType`/`Pos`) is the corpus instance: without the
record, html/template's escaper would mint an adapter whose `tree()` returns null and crash three packages
away.

<!-- measured at the change (not re-measured by any guard):
**Footprint,** from a whole-stdlib A/B with both roots seeded (304/304 converted per side; marker gate
54 marked files / 43 `*_impl.cs` companions / **0 violations** on both roots): **75 files** — 35
`package_info.cs` and 40 code — with **0** `.csproj` and **0** `README.md` moved. **184 records appear**
across 22 declaring packages (`go/ast` 96, `image` 17, `io` 14, `image/color` 12, `database/sql` 8,
`net` 7, `math/rand/v2` 4, `sort` 3, …) and **117 go away**, every one a consumer-local duplicate the
declaring assembly now owns: `go/parser` 49, `go/types` 28, `go/doc` 8, `go/printer` 5 (all of them
`go/ast` node types), `net/http` 4, the five `debug/*` + `internal/xcoff` readers' `io.SectionReader`
pairs, `net/http/httputil`'s `io.Pipe{Reader,Writer}`, and one each for `sync.Mutex`→`Locker`,
`image/color.RGBA64`→`Color` and `parse.BranchNode`→`Node`. Net corpus movement is **+67** pointer
records (1,071 → 1,138), not the 548 the deferral's raw pair count suggested — most of that set was
already recorded from cast sites, and the gates take the rest. -->

Every repointed adapter construction is the same edit — `new pkg_TжIface(x)` becomes `new pkg.TжIface(x)`: a
site names the declaring assembly's one adapter rather than a locally minted duplicate.
<!-- measured at the change (not re-measured by any guard):
**318 adapter constructions** are repointed across 40 files, every changed line the same edit —
`new pkg_TжIface(x)` becomes `new pkg.TжIface(x)` — which is the second-identity elimination measured:
318 sites that used to name a locally minted duplicate now name the declaring assembly's one adapter.
There is no third family; a classifier over the whole diff reports **zero** unclassified added lines. -->

Guarded by `SamePackageImplementNoWitness`, whose `*Tally → Metric` pair is positive
(the consumer references `ledger.TallyжMetric` instead of minting `ledger_TallyжMetric`) and which
carries the negative this gate needs — `tick`, an UNEXPORTED target whose pointer set implements the
exported interface, kept live through `ledger.Count` and absent from `ledger`'s metadata. Also guarded by
`ForeignPointerImplementSuppression`, where `Lone` — a pair `tone` satisfies and never casts — is that
test's proof that a record needs no witnessing cast, while its `shade.Level` negative (a
same-SIMPLE-named interface in another package) keeps its local adapter. Unit-guarded by
`TestPointerRecordIsPubliclyRealizable`,
`TestRealizabilityBoundsDifferByForm` and `TestPointerMethodSetSubsumesValueMethodSet`
(`samePackageImplements_test.go`).

**Hand-owning a witnessing body is therefore safe.** Measured with `(*RawSockaddrAny).Sockaddr` suppressed through
`manualConversionFuncs`, `syscall` still records all three `(Pointer = true)` Sockaddr pairs, and `net`'s
construction sites still reference `syscall`'s own adapter (`new syscall.SockaddrInet4жΔSockaddr(…)`).
<!-- history: the L10 probe (on windows/amd64, net's six construction sites), re-run with the suppression on scratch
builds of each converter into seeded roots:
the pre-increment converter dropped all three records and net minted syscall_SockaddrInet4жΔSockaddr locally;
the post-increment converter kept all three and net referenced syscall's own adapter. -->

## Standard-library solution file (`.slnx`)

A whole-standard-library run (`go2cs -stdlib`) also emits a Visual Studio solution — **`go2cs-stdlib.slnx`** — at the output root (`-go2cspath`), so the freshly converted stdlib is openable / buildable as **one unit** immediately after a run, rather than depending on a hand-maintained solution that drifts. It is the auto-generated counterpart of the committed `src/go2cs-stdlib.slnx`, and its XML mirrors the format of `src/go2cs.slnx` (a `<Configurations>` block plus `<Folder>`/`<Project>` entries, CRLF line endings, no BOM). It references:

* every converted stdlib project it emitted (`core/<pkg>/<projectName>.csproj`), each nested under a solution folder named by its full Go import path (`/crypto/aes/`), with every intermediate folder declared explicitly and a deterministic `Id` hashed from the full folder path, because Go namespaces guarantee duplicate leaves (`/crypto/` and `/vendor/golang.org/x/crypto/`);
* the shared **`golib`** runtime (`core/golib/golib.csproj`) and the **`go2cs-gen`** source-generator/analyzer project (`gen/go2cs-gen/go2cs-gen.csproj`), at the solution root with no folder, plus the CPU-profiler companion when present.

Per-package **test** projects (`<pkg>.tests.csproj`) are deliberately left OUT: their inputs are pipeline-staged, so a solution containing one does not build from a clean tree (`collectConvertedProjects`, `solutionGenerator.go`).

The stdlib project list is gathered by walking the emitted `core/` output tree, and every project is emitted in stable **alphabetical** order for deterministic output. All paths are **solution-relative** (forward slashes) so the generated solution is portable — no absolute, machine-specific paths. The `golib` and `go2cs-gen` references use the same `core/golib` / `gen/go2cs-gen` layout the converted `.csproj` files already assume via `$(go2csPath)` (which resolves to `$(SolutionDir)`), so the solution locates them wherever those csproj references already resolve. The file is only rewritten when its content changes, so repeated runs are a no-op.

## Recurse per-project solution file (`.slnx`)

A recursive end-user run (`go2cs -recurse`) instead emits **one `.slnx` next to every converted `.csproj`** (`ModuleConverter.generatePerProjectSolutions`), each over that anchor project plus its **transitive converted dependencies** + `golib` + the analyzer — no stdlib listed (the stdlib is *referenced* via `$(go2csPath)core`, pre-staged by `deploy-core`). Building the app's own per-project solution thus builds the app and its whole converted dependency closure in one `dotnet build`, without the whole-stdlib solution. The anchor project is marked the Visual Studio default startup project (`DefaultStartup="true"`).

Projects are grouped into **three top-level solution folders that mirror the `%GOPATH%` layout**, emitted in an **enforced, deliberately non-alphabetic order**:

* **`/src/`** — the project(s) being converted (the app's own main-module packages),
* **`/pkg/`** — their converted dependency packages (module-cache or `replace` third-party), then
* **`/core/`** — the go2cs runtime/generator projects (`golib`, `go2cs-gen`).

Each member is placed by **import path** (`isMainModulePackage` — the same rule that routes a package's output to `src\` vs. `pkg\`), so the solution folders agree with the on-disk parallel tree regardless of the `.slnx`-relative path shape. An **empty folder is omitted** — a dependency's own per-project solution has no `src` package — so no folder is ever declared empty. Because the three folder names are unique leaves, no folder `Id` attribute is emitted (unlike the namespace-nested stdlib solution, whose duplicate leaves like `crypto`/`internal` require the hashed `folderID`). Paths are solution-relative forward slashes, CRLF line endings, no BOM; the file is rewritten only when its content changes. Rendered by `buildRecurseSolutionXML` (`solutionGenerator.go`), guarded by `TestBuildRecurseSolutionXML`, `TestBuildRecurseSolutionXMLSkipsEmptyFolders`, and the folder-order assertions in the `TestRecurseSyntheticModule` integration test.

---

[Index](README.md) · [Package-Level Variable Initialization Order →](variable-initialization-order.md)
