# Package Conversion: Project Files

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers the files a conversion emits around the C# sources: the `.csproj` and its shared props, the package README, and the solution files.

## The project file

### The recurse output root records its runtime root: the `$(go2csPath)` default

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

### Path separators in emitted MSBuild files: forward slashes, on every host

**Every path the converter writes into a `.csproj`, `.slnx`, `.pubxml` or `Directory.Build.props` uses `/`, on every host.** There is no per-host emission and no host-conditional spelling: one converted corpus is correct on Windows, Linux and macOS.

```xml
<ProjectReference Include="$(go2csPath)core/fmt/fmt.csproj" />
<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" … />
<OutDir>bin/$(Configuration)/$(TargetFramework)/</OutDir>
<GoValidationProofFile>$(go2csPath)../docs/validation/$(GoStdLibVersion).$(GoBuildNumber)/fmt.md</GoValidationProofFile>
```

MSBuild accepts `/` in every path context on Windows, and normalizes `\` to `/` on Unix (`FileUtilities.MaybeAdjustFilePath`), so *both* spellings build on both hosts — the .NET SDK's own targets depend on the Unix direction. The reason to pick one is that two other consumers are **not** MSBuild:

1. **The converter's own path arithmetic.** The reference used to be composed by hand — replace every `/` with `\`, then `filepath.Join` a backslash-prefixed file name. On Windows `filepath.Clean` folded that back into a well-formed path; on Unix `filepath.Join` treats `\` as an ordinary filename character, so a Linux-hosted conversion emitted the malformed `$(go2csPath)core\fmt/\fmt.csproj` for *every* stdlib reference in *every* project — silent at emission, a restore failure later (F5, [`PLAN-linux-operation.md`](../../PLAN-linux-operation.md) §A1.1). The composition is now `emittedProjectReference` (`importOperations.go`): `path.Join` (slash-only, host-independent) over a `filepath.ToSlash`'d directory. `writeProjectFile` and `writeTestProject` additionally `ToSlash` at the emission point, because a sibling reference made relative by `filepath.Rel` arrives OS-native.
2. **The harnesses that READ an emitted reference.** `BehavioralRunner.PreBuildSharedDeps` and `PerformanceRunner` parse `ProjectReference Include="…"` out of the csproj and resolve it with `Path.GetFullPath`, which on Linux does not split on `\` either.

**Consequence to expect when this changes: the sorted reference block can re-order.** References are `sort.Strings`-sorted, and `/` (0x2F) sorts *below* alphanumerics while `\` (0x5C) sorts *above* them, so a pair that differs at a separator boundary swaps. Across the whole 303-project stdlib the flip moved exactly one file's ordering — `net/http`, where `vendor/golang.org/x/net/http2/hpack` had sorted before `vendor/golang.org/x/net/http/httpguts` (`2` < `\`) and now sorts after it (`/` < `2`). Same set, different order; not a content change.

The hand-owned `core` files that are never re-emitted — `golib`, `testing`, `unsafe`, `internal/godebug` and `core/Directory.Build.props` among them — carry the form by hand. (At Go 1.23.12 the list also named `internal/concurrent` and `internal/weak`; their Go 1.24 successors `internal/sync` and `weak` convert ordinary files beside the hand-owned one, so both emit their own `.csproj`.) The one deliberate exception is the shared-project `<Import Project="..\go2cs\go2cs.projitems" Label="Shared" />` in `golib.csproj` and `go2cs-gen.csproj`: that is Visual Studio's own bookkeeping, VS round-trips its exact text, and MSBuild normalizes it on Unix regardless — so it stays backslashed, with a comment saying why.

Guarded by `TestEmbeddedCsprojTemplatesUseForwardSlashesOnly` and `TestValidationPackBlockUsesForwardSlashesOnly` (`csprojTemplate_test.go` — both templates are asserted to contain *no* backslash at all, so a future addition is covered without the guard enumerating it) and by `TestEmittedProjectReferenceIsHostIndependent` / `TestEmittedProjectReferenceForModuleCachePath` (`importOperations_test.go`).

### The validation-proof block follows the OUTPUT LOCATION, not the invocation mode

A converted stdlib package's `.csproj` carries a block that packs its versioned proof sheet as `VALIDATION.md` inside the nupkg, and `.csproj` files are **regenerated on every transpile** — so the block's emission condition is what decides whether a package keeps shipping its proof. The rule is a structural test on where the project file is being WRITTEN:

> emit the block for a `-stdlib` conversion, or for any conversion re-emitting a `.csproj` under the runtime root's `core/` tree.

It was originally scoped to the invocation MODE instead — `-stdlib`, later widened to `-stdlib` or `-tests` — and each narrowing left a door open that silently un-ships a package's proof sheet at the next `push-nuget`. The `-tests` door was the first (the standing "0 8" restore family: every Phase-4 pipeline run stripped the block from the package under test). The **single-package** door was the second and is closed here (2026-08-19): `go2cs <goroot-pkg-dir> <core-pkg-dir>` — the form a lane uses to regenerate ONE corpus package after a converter change — is neither `-stdlib` nor `-tests`, so it stripped the block too. That one is the harder of the two to catch, because only the `.csproj` moves and a lone `.csproj` diff in a reconvert reads as ordinary emission drift rather than as a loss.

Keying on the output location closes both doors at once and cannot be re-opened by adding a mode: `-recurse` writes under its own `src/`/`pkg/` trees, a behavioral fixture writes under `tests/Behavioral/`, and an end-user module writes wherever it was pointed — none of them satisfy "under `<go2csPath>/core/`", so all three keep their historical `.csproj` bytes. Guarded by `TestValidationPackBlockSurvivesTestsRewriteOfCorePackage`, which now pins the single-package form and its outside-`core` negative control alongside the `-tests` pair.

### Generated output path: `$(OutDir)` defers to `$(BaseOutputPath)`

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

### Build-warning suppression: what the emitted `.csproj` silences, and what it deliberately does not

Both templates carry one suppression policy, and it is a policy rather than an accretion: every entry is a
diagnostic that is **structural to the Go emission model**, and every diagnostic that is *not* is left
visible on purpose. The full census, code by code, is [`docs/phase4/DESIGN-warning-suppression.md`](../../phase4/DESIGN-warning-suppression.md).

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

### Assembly metadata is one derivation chain, and the framework is hoisted to props

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

## The README

### The README has TWO emission points, because its Tests badge reads a page the run writes LAST

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

## Solution files

### Standard-library solution file (`.slnx`)

A whole-standard-library run (`go2cs -stdlib`) also emits a Visual Studio solution — **`go2cs-stdlib.slnx`** — at the output root (`-go2cspath`), so the freshly converted stdlib is openable / buildable as **one unit** immediately after a run, rather than depending on a hand-maintained solution that drifts. It is the auto-generated counterpart of the committed `src/go2cs-stdlib.slnx`, and its XML mirrors the format of `src/go2cs.slnx` (a `<Configurations>` block plus `<Folder>`/`<Project>` entries, CRLF line endings, no BOM). It references:

* every converted stdlib project it emitted (`core/<pkg>/<projectName>.csproj`, grouped under a `/core/` folder),
* any per-package **test** projects (`*_test.csproj`, grouped under a `/tests/` folder — inert until Phase 4 emits them, and the folder is omitted entirely when there are none),
* the shared **`golib`** runtime (`core/golib/golib.csproj`), and
* the **`go2cs-gen`** source-generator/analyzer project (`gen/go2cs-gen/go2cs-gen.csproj`, under a `/generators/` folder).

The stdlib project list is gathered by walking the emitted `core/` output tree (so it also picks up future test projects with no code change), and every project is emitted in stable **alphabetical** order for deterministic output. All paths are **solution-relative** (forward slashes) so the generated solution is portable — no absolute, machine-specific paths. The `golib` and `go2cs-gen` references use the same `core\golib` / `gen\go2cs-gen` layout the converted `.csproj` files already assume via `$(go2csPath)` (which resolves to `$(SolutionDir)`), so the solution locates them wherever those csproj references already resolve. The file is only rewritten when its content changes, so repeated runs are a no-op.

### Recurse per-project solution file (`.slnx`)

A recursive end-user run (`go2cs -recurse`) instead emits **one `.slnx` next to every converted `.csproj`** (`ModuleConverter.generatePerProjectSolutions`), each over that anchor project plus its **transitive converted dependencies** + `golib` + the analyzer — no stdlib listed (the stdlib is *referenced* via `$(go2csPath)core`, pre-staged by `deploy-core`). Building the app's own per-project solution thus builds the app and its whole converted dependency closure in one `dotnet build`, without the ~300-project stdlib solution. The anchor project is marked the Visual Studio default startup project (`DefaultStartup="true"`).

Projects are grouped into **three top-level solution folders that mirror the `%GOPATH%` layout**, emitted in an **enforced, deliberately non-alphabetic order**:

* **`/src/`** — the project(s) being converted (the app's own main-module packages),
* **`/pkg/`** — their converted dependency packages (module-cache or `replace` third-party), then
* **`/core/`** — the go2cs runtime/generator projects (`golib`, `go2cs-gen`).

Each member is placed by **import path** (`isMainModulePackage` — the same rule that routes a package's output to `src\` vs. `pkg\`), so the solution folders agree with the on-disk parallel tree regardless of the `.slnx`-relative path shape. An **empty folder is omitted** — a dependency's own per-project solution has no `src` package — mirroring how the stdlib solution drops its `/tests/` folder when empty. Because the three folder names are unique leaves, no folder `Id` attribute is emitted (unlike the namespace-nested stdlib solution, whose duplicate leaves like `crypto`/`internal` require the hashed `folderID`). Paths are solution-relative forward slashes, CRLF line endings, no BOM; the file is rewritten only when its content changes. Rendered by `buildRecurseSolutionXML` (`solutionGenerator.go`), guarded by `TestBuildRecurseSolutionXML`, `TestBuildRecurseSolutionXMLSkipsEmptyFolders`, and the folder-order assertions in the `TestRecurseSyntheticModule` integration test.

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
