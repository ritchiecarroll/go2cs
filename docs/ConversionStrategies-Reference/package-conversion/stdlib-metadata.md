# Package Conversion: Standard-Library Metadata

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers how a package that references the standard library as a NuGet package still has the exported metadata the converter reads from a dependency.

## NuGet-referenced standard library

### A NuGet-referenced standard library carries its exported metadata IN THE CONVERTER

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

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
