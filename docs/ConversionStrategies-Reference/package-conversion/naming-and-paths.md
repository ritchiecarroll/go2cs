# Package Conversion: Names and Paths

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers how a converted package's project is named and where it is written, and how an importer spells the package class.

## Project names and paths

### A project name is the package's FULL import path — a go-file-free container directory does not truncate it

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

### The repository's own `go2cs/` module marker is elided — on BOTH sides, by one rule

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

### The emitted project PATH is budgeted — the file name compresses, the identity never does

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

### A GOROOT-vendored reference is named for the package's ON-DISK path

**A standard-library reference takes its project file name from the directory it resolved to, not from the import path as written.** The two are the same string for every package in the standard library except one class — the GOROOT-**vendored** ones, imported as `golang.org/x/…` but existing on disk, and therefore as converted projects, only under `vendor/golang.org/x/…`.

```xml
<!-- crypto/ecdh imports `golang.org/x/crypto/chacha20` -->
<ProjectReference Include="$(go2csPath)core/vendor/golang.org/x/crypto/chacha20/vendor.golang.org.x.crypto.chacha20.csproj" />
```

The directory half was always right — it is rewritten from the resolved source dir — while the file name was composed from the import path, so the emitted reference named a real directory and a file in it that exists nowhere: `…/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj`. Deriving the name from the directory (`stdLibImportPathFromTargetDir`, applied in both stdlib arms of `importOperations.go`) is what makes the two halves agree *structurally* rather than coincidentally: `getProjectName` — the producer, which names the `.csproj` the vendored package actually emits — has always derived it from that same `GOROOT/src`-relative directory.

This is the third derivation in one family, and they must all resolve the vendored spelling or they disagree about which package is being named: the **namespace** (`resolveGorootVendoredPath`, under [Cross-package imports](imports-and-build-constraints.md#cross-package-imports-importing-another-package--assembly)), the **dependency-graph key** (`stdLibConverter`), and now the **project file name**.

Two consequences beyond the file name, because `PackageName` is not only a file name:

* It keys the embedded standard-library metadata, which records the vendored spelling (`##vendor.golang.org.x.crypto.chacha20`), so the unvendored name matched no section at all (asserted directly by the guard). Wherever that record is the source — a dependency with no `package_info.cs` on disk, i.e. a `-recurse=nuget` reference — the package's exported aliases and `GoImplement` records would have come back empty and silently fallen through to the derive-from-declarations path.
* It composes the imported-alias class path. The unvendored form yields `go.golang.org.x.crypto.chacha20_package`, which names a class that exists nowhere — the CS0234 family the namespace arm above exists to prevent. It was latent rather than active only because `loadImportedTypeAliases` dedupes on the *dependency's* `package_info.cs` path, which is the same file for both spellings, so whichever spelling was resolved first won and the other never applied its aliases.

**Where it surfaced, and what it did NOT do.** Only a `-tests` conversion could emit it: production emission resolves the vendored path upstream (`visitImportSpec`), while the test project's dependency list is the raw import set, which carries both spellings — so `crypto/ecdh`'s test project named the package twice, once correctly and once not. It is worth being precise about the damage, because a missing `<ProjectReference>` sounds fatal and is not: MSBuild degrades it to **warning MSB9008** and builds on (measured — the pre-fix `crypto.ecdh.tests.csproj` builds, 0 errors), and here the correct sibling reference supplied the assembly anyway. The real cost was downstream: the stale name was harvested into `go2cs-stdlib.slnx` as a phantom 308th project by the multi-platform merge's solution-recovery path (fixed on the solution side by `TestCollectConvertedProjectsIgnoresTestProjectReferences`; this is the emission half).

Guarded by `TestGorootVendoredReferenceNamesTheVendoredProject` (the vendored spelling, the metadata key, and the leaf package name), `TestStdLibImportPathFromTargetDir` (the recovery, including the non-core-rooted no-match that leaves the caller on the import path) and `TestStdLibReferenceUnchangedForUnvendoredPackage` (the no-op half — the whole corpus bar the `vendor/` tree, which is what makes a zero-movement [CNR](../../Glossary.md#cnr) verdict meaningful rather than lucky), all in `importOperations_test.go`.

## Package class names

### An importer spells the package class from the package NAME — the standard library included

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

So trusting the import graph *everywhere* **keeps** the byte-identity the stdlib exclusion was asserting rather than merely asserting it — and [CNR](../../Glossary.md#cnr) is what proves the claim instead of the comment.

The fix restructures rather than special-cases: **when the graph knows a package's name, that name is the class segment; the `/vN` directory convention remains the fallback for when it does not.** A narrower "substitute only when the two differ" test would have looked equivalent and quietly broken the exotic case the convention branch was written for — a package literally *named* `vN`, which would then be rewritten to its parent. Preferring the authoritative name over the convention wherever both are available is what keeps the two rules from fighting.

This is the same family as [the GOROOT-vendored reference](#a-goroot-vendored-reference-is-named-for-the-packages-on-disk-path) above: several independent derivations name one package, and they are correct only when they agree *structurally*. Guarded by `TestImportedPackageClassFollowsPackageName` (the rule, over a stdlib name/directory mismatch, an ordinary stdlib package, a `/vN` directory and a module dependency) and `TestMajorVersionFallbackAppliesWithoutGraphMetadata` (the fallback half), in `packageClassNaming_test.go`.

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
