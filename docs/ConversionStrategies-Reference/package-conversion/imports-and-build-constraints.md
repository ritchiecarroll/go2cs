# Package Conversion: Imports and Build Constraints

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers how one converted package reaches another: where an import resolves, the `using` aliases a file emits, how an import path becomes a namespace, and how build constraints select files.

## Cross-package imports

### Cross-package imports (importing another package / assembly)

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

### Exported structs and interfaces cross packages

**Exported structs and interfaces cross packages.** An exported struct's fields and methods are reachable on the consumer side exactly as the producer emits them — `CrossPkgLib.Sensor{Name: …, Temp: …}` lowers to a C# constructor call and `s.Name` / `s.Hot()` to field/method access on the imported struct — because the struct and its `[GoRecv]` extension methods live in the (referenced) library assembly.

A cross-package **interface satisfaction** is subtler. Go is structurally typed, so a consumer may assign any value with the right method set to an interface; C# requires the *nominal* `partial struct T : I` implementation glue, which the [`ImplementGenerator`](../source-generators.md#source-generators) can only add to `T` **in T's own assembly** (`isLocalImplType`). The converter records a `[assembly: GoImplement<T, I>]` for each concrete→interface conversion it *witnesses while converting T's package* — so for a consumer to use `Sensor` as `Labeled` across the assembly boundary, the satisfaction must be witnessed in the **library** that declares `Sensor`. The idiomatic Go interface-satisfaction assertion does exactly this:

```go
var _ Labeled = Sensor{}   // in CrossPkgLib — records GoImplement<Sensor, Labeled> in this assembly
```

With that, the library emits `[assembly: GoImplement<Sensor, Labeled>]`, `Sensor : Labeled` is realized in the library assembly, and a consumer's `var l CrossPkgLib.Labeled = s` / `CrossPkgLib.Describe(s)` compile as ordinary upcasts. (A library that returns the interface from a constructor — `func New(...) Labeled { return Sensor{…} }` — witnesses it the same way.) A type that satisfies an interface but is *never* used as it within its own package gets no nominal glue — proactively recording every local concrete→local interface structural match WAS tried (a declaration-site scan, 2026-07) and was RETIRED on 2026-07-25, because it could never be complete (it cannot see a dynamic type in a later-converted assembly) and paid for its incompleteness in speculative glue. Such a satisfaction is resolved at run time by the interface's duck-typing shells instead; a *declared* conversion still takes the nominal fast path. Also guarded by the `CrossPkgLib`/`CrossPkgUser` pair (Phase 3: struct field access + interface satisfaction).

### A cross-package type reference emits its `using <alias> = <namespace>;`

**A cross-package type reference emits its `using <alias> = <namespace>;` even when the file did not import the package under a usable name.** A foreign type renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type, `@unsafe.Pointer` for the `unsafe.Pointer` basic — which resolves only through a file-local alias (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias is normally generated from a *canonical* (unaliased) `import`, but a file can reference a foreign type with no such import through three routes: **type inference** — a *same-package* function returns a foreign type, so the caller infers a local of that type but never writes `pkg.` and need not import the package (runtime `preempt.go`: `fd := funcdata(f, i)`, where `funcdata` returns `unsafe.Pointer`); a **blank import** (`_ "pkg"`, side-effects-only — **no `using` is emitted for it at all**: the old `using _ = <ns>;` emission hijacked C#'s `_` DISCARD for the whole file, so a deconstruction discard (`(w, _) = w.ensure(…)`, runtime `tracetime.go`) bound the namespace alias instead (CS0118 + CS0029); the import is recorded as a comment, and a genuine type reference still gets its canonical alias from this machinery — e.g. `symtabinl.go`'s `_ "unsafe"` for `//go:linkname`); or an **aliased import** (`import u "unsafe"`, whose alias `u` differs from the canonical `pkg.Name()` prefix the type reference uses). All previously yielded CS0246. The converter now walks every emitted type (`collectTypePackages`, called from `getAliasQualifiedTypeName` — named types by `pkg.Path()`, an `unsafe.Pointer` basic by the pseudo-path `"unsafe"`, recursing through pointer/slice/array/map/chan/generic/func-signature so a `[]time.Duration` element registers too) and, at file close (`visitFile`), supplies the canonical `using <alias> = <namespace>;` for every referenced foreign package the file did not already import canonically. It is idempotent-safe — a canonical import records its path in `canonicalAliasImported`, so `visitFile` never re-emits (duplicates) it — and a non-canonical alias (`using u = unsafe_package;`) coexists with the added canonical one without conflict. It is also **collision-guarded**: the synthesized `using <alias> = <namespace>;` is skipped when its canonical `<alias>` was already bound to a *different* namespace by a real import — cryptobyte's `asn1.go` imports both `encoding_asn1 "encoding/asn1"` (referenced by type, so it reaches this loop) and the subpackage `.../cryptobyte/asn1` (unaliased → alias `asn1`), so synthesizing `using asn1 = encoding.asn1_package` would duplicate the subpackage's `using asn1` (CS1537). The real imports' emitted aliases are tracked per file (`importAliasesEmitted`); the parent stays reachable through its `encoding_asn1` alias, so skipping the canonical one is safe (a non-colliding canonical alias is still supplied — no churn). (The *separate* defect that the type reference itself renders `asn1.ObjectIdentifier` rather than the file's `encoding_asn1.ObjectIdentifier` — `getAliasQualifiedTypeName` uses the canonical alias, not the file's non-canonical one — is tracked independently.) This is the *type-reference* analog of the method-call `addMethodPackageNamespaceUsing`. (Guarded by `UnsafePointerInferredNoImport` — the `unsafe.Pointer` basic arm, scalar/composite/blank-import variants — and `InferredForeignTypeNoImport` — the generic named arm, an inferred `*strings.Reader` in an `fmt`-only consumer.)

**That supplied alias must carry the collision rename.** When the referenced package's using alias is `Δ`-renamed because a same-named CHILD namespace is visible from the import closure (`go.sync`, contributed by `sync/atomic`; `go.unicode`, by `unicode/utf8` — the same CS0576 collision that renames a *canonical* import's alias, above), `getAliasedTypeName` already renders the short-form type reference through the renamed qualifier (`Δsync.Mutex`, `Δunicode.Range16`). The `visitFile` supply loop, however, composed the alias from `packageUsingAlias` alone — the bare, unrenamed name — so it emitted `using sync = sync_package;` (or `using unicode = unicode_package;`) while the reference read `Δsync.Mutex`: the alias binds nothing (CS0246), and the bare alias would itself collide with the child namespace (CS0576). The supplied alias is now routed through `importQualifier` (`getSanitizedImport(importQualifier(alias))`, the same rename every canonical import applies), so the emitted `using Δsync = sync_package;` matches the reference. `importQualifier` is a no-op for any package whose alias is not renamed, so a non-colliding supplied alias stays byte-identical. The trigger is a file that reaches a renamed package's type through the supply route rather than a canonical import — overwhelmingly a **dot import** (`. "sync"` / `. "unicode"`), where the dot brings names in via `using static` yet the converter still qualifies the type, and no canonical `using <pkg> = …` is emitted to carry the rename. Production stdlib code essentially never dot-imports, so the defect stayed latent as an *unused* supplied alias (reflect's `value.go`/`makefunc.go` inferred `sync` without importing it — the `using sync` alias was never referenced, so the wrong spelling compiled); it surfaces in an EXTERNAL (`_test`) variant that dot-imports the package under test, which `unicode`'s `letter_test.go` does (`. "unicode"` + qualified `Range16`/`RangeTable`/`CaseRange`). Because every currently-compiling site had the alias *unused*, the change only ever flips an unused alias (compile-neutral) or fixes a broken one — no site that used `Δpkg.` while getting the bare supplied alias could have compiled. (Guarded by the `DotImportRenamedPackage` behavioral test — `. "sync"` with a `*Mutex` type reference forcing the qualified `Δsync.Mutex` position, output-compared vs Go; neutering the fix reproduces the reported `CS0246: 'Δsync' could not be found`.)

### System-colliding local type names are root-qualified in assembly attributes
A Go package can name one of its own exported types after a top-level C# `System` type — internal/profile's `ValueType`, go/ast's `Object`, bytes' `Buffer`. The `GoImplement`/`GoImplicitConv` assembly attributes generated in `package_info.cs` sit at **file scope**, before the `namespace` line, where both `using System;` (a csproj global using) and `using static go.<pkg>_package;` are active — so a bare `ValueType` is ambiguous between `System.ValueType` and the package type (CS0104). The emitter root-qualifies any bare, dotless type name matching a curated set of `System` top-level names at the package class:

```csharp
[assembly: GoImplement<go.@internal.profile_package.ValueType, message>]
[assembly: GoImplicitConv<go.@internal.profile_package.ValueType, ж<go.@internal.profile_package.ValueType>>(Indirect = true)]
```

Foreign types are always package-qualified already (dotted) and are left untouched; no non-colliding name changes, so every non-colliding attribute emits byte-identically. (Guarded by the `SystemCollidingTypeName` behavioral test.)

## Import paths

### Major-version import directories
A `/vN` import path segment (math/rand/v2) hosts a package named for the PARENT segment, so the
emitted class follows the package NAME: consumers reference `go.math.rand.rand_package`, and the
namespace is `go.math.rand` — never the path-derived `v2_package` / `go.math.rand.v2`. Go's own
convention (the directory is a version marker, not the package identifier) means the package name
equals the second-to-last path segment, and every place the converter derives a class/namespace/
alias from a `/vN` import path must honor it. There are **four** such derivations, reached by
different renderers, and each needed the convention applied at its own site:

1. **`using`-alias + namespace emission** — `convertImportPathToNamespace` (`visitImportSpec.go`)
   rewrites the last path part to the parent segment via `majorVersionSegmentRegex`, so the file's
   `using rand = go.math.rand.rand_package;` and the package's own `namespace go.math.rand` agree.
2. **`t.String()`-based FQ type rendering** — `getAliasQualifiedTypeName` / `getFullyQualifiedTypeName` (`main.go`) build a
   foreign type's name from the type graph's path-qualified string, whose last segment slash-strip
   assumes the path tail IS the package qualifier. For a `/vN` tail it left the version behind
   (`math/rand/v2.Rand` → `v2.Rand`), which the alias-prepend then doubled into `rand.v2.Rand`
   (`v2` read as a member of class `rand_package` — CS0426). Both renderers now reduce the foreign
   import-PATH qualifier to the package NAME before the slash-strip. `getFullyQualifiedTypeName` also composes
   `pkg.Path()+"_package"` directly for the qualified base name — routed through `packageClassPath`,
   which swaps a `/vN` tail for the Go package name.
3. **Cross-package reference metadata** — `PackageInfo.RootPackageName` (`importOperations.go`) is the
   code-facing qualifier that keys imported-alias loading and the foreign-implement records that cast
   sites reference (`GoImplement<…rand_package.PCG, …rand_package.Source>(Pointer = true)`). It was the
   path's last segment (`v2`); `rootPackageNameFromPathParts` now returns the parent segment for a
   `/vN` tail. `PackageName` stays path-formed — it also names the referenced `.csproj`, which IS
   `math.rand.v2.csproj`.
4. **Imported type-alias TARGET class** — `loadImportedTypeAliases` (`importOperations.go`) qualifies
   an imported alias's target as `go.<PackageName>_package.<Type>`; the class path is `PackageName`
   with its final segment replaced by `RootPackageName`, so a `/vN` producer's exported aliases
   resolve to `rand_package`, not `v2_package`.

The convention is that a package literally named `vN` would instead need the type-graph name; the
stdlib has none, so the regex/parent-segment rule holds corpus-wide. Guarded by the `VersionedImport`
behavioral test — a `main` importing a sibling `vlib/v2` module (`package vlib`) that mirrors
math/rand/v2's shape: a struct field `ж<vlib.Rand>` (renderer #2), a `*PCG → Source` pointer cast
recorded as `go.vlib.vlib_package.PCG` (#3/#4), output-compared vs `go run` across all four phases.
This is what unblocks sort as Phase 4's second validated package (its test suite imports math/rand/v2).

### A C# keyword inside a dotted import-path element
A Go import-path element may itself contain **dots** — a module host (`gopkg.in`, `example.com`,
`golang.org`) or a versioned tail (`yaml.v3`) — and every one of those dots is a **namespace
separator** in the emitted C#. So `gopkg.in/yaml.v3` does not render two namespace levels, it renders
four, and each is a separate C# identifier that has to be keyword-escaped on its own.

Two sanitizers render namespace text, and until 2026-08-07 only one of them knew that. The
**declaration** side (`getProjectName` → `getCoreSanitizedIdentifier`) has always split an element on
its dots, so the dependency's own file correctly opens `namespace go.gopkg.@in;`. Every **consumer**
emission — the import's `using yaml = …;` alias, the enclosing-namespace `using gopkg.…;` an unaliased
import adds, the child-namespace map that decides root qualification, and the string-path type
renderer — goes through `convertImportPathToNamespace`, which sanitized each `/`-split element with
`getSanitizedImport`, measuring it **whole**. Whole, `gopkg.in` is not a C# keyword, so it passed
through bare and the importer emitted

```csharp
using yaml = gopkg.in.yaml_package;   // CS1001/CS1002/CS1022 — `in` is a keyword
using gopkg.in;
```

against a producer that had named itself `go.gopkg.@in`. The dependency compiled; nothing that
imported it could. (Issue #33: `gopkg.in/yaml.v3` converts, then does not build.)

The fix is one function, both sides: **`getSanitizedImport` splits on dots too**, escaping each level
independently — exactly what the declaration side does. The recursion stays inside `getSanitizedImport`
rather than deferring to `getCoreSanitizedIdentifier`, because callers append the `_package` class
suffix to the final element before calling and the core sanitizer `Δ`-prefixes anything ending in
`_package`; that swap would emit `Δyaml_package`, a class no producer declares. Escaping is idempotent
(an already-`@`-marked part returns unchanged), so re-sanitizing a rendered namespace is stable.

The behavior change is exactly "a dotted input with a keyword sub-token is now escaped": a string
containing a dot could never equal a keyword, so the old whole-string test never fired for one, and
hyphen/tilde replacement is per-part identical either way. Emission-neutral for both corpora, and
measured so — the behavioral corpus has no dotted module path at all, and the standard library's only
dotted element is `golang.org` (the GOROOT-vendored tree), whose `golang`/`org` are not keywords:
[CNR](../../Glossary.md#cnr) byte-identical across 572 packages, and a seeded full reconvert byte-identical
across 5,179 `.cs`/`.csproj`/`README.md` plus the generated `go2cs-stdlib.slnx`.

Guarded at both altitudes: `TestGetSanitizedImportKeywordSegments` (`sanitization_test.go` — several
keywords in several positions, the two sanitizers asserted to agree on a segment, the `_package`
suffix asserted NOT to be `Δ`-prefixed, and idempotence) and `TestRecurseKeywordNamespaceSegment`
(`moduleConverter_integration_test.go` — network-free, an unaliased import of a `gopkg.in`-shaped
dependency, asserting the producer's declaration and both consumer emissions name the same namespace,
then sweeping every `using` in the file against the converter's own `keywords` set so a keyword the
fixture never exercises is covered by the same assertion). Both neuter-proven: restoring the
whole-string measurement reproduces the reporter's emitted text verbatim.

### The import-path rewrite rewrites only the PATH, not the constructor in front of it
The string-path type renderer (`convertToCSFullTypeName`) peels a Go type expression one constructor
at a time — `<-chan `, `chan `, `chan<- `, `*`, `[]`, `[N]`, `map[K]`, `func(…)` — recursing on what
is left. Its **import-path rewrite runs first**, before any of those branches, because a
package-qualified element has to become a C# namespace before the name means anything. Until
2026-08-08 that rewrite measured the path from index 0 of the whole string, constructor included.

`convertImportPathToNamespace` maps a **hyphen to an underscore**, because a Go path element may
legally contain one (`mongo-driver`, `go-isatty`). Handed the constructor as well, it rewrote the
`-` of `<-chan` too. The declaration in `go.mongodb.org/mongo-driver/x/mongo/driver/session`

```go
type Pool struct { descChan <-chan description.Topology }
```

renders fully-qualified as `<-chan go.mongodb.org/mongo_driver/mongo/description_package.Topology`,
and came back as `<_chan go.mongodb.org.mongo_driver.…`. No channel branch recognizes `<_chan`, so it
fell through to the **array** branch, which slices past the `>` that a `[N]` length closes — and with
no `>` in the string at all, `strings.Index` returned -1 and the slice was `typeName[0:]`. The
renderer re-entered on the IDENTICAL string, without bound: `fatal error: stack overflow`, taking a
1,726-package `-recurse` run down at package 1456 (issue #33's third report).

A **single-segment** path never had a slash to enter the rewrite, so `<-chan time_package.Time` was
always correct. That is the whole reason the standard library — which is nothing but single- and
multi-segment *stdlib* paths, none of them hyphenated behind a `<-chan` — never saw this, and only a
module dependency could.

The fix is `importPathStart`: find where the path begins by scanning **backward** from the candidate
region to the first byte no import path may contain, and rewrite only from there. `-` cannot be that
delimiter (it would split `mongo-driver` mid-path), but every constructor the renderer emits ends in
one that can — a space (`<-chan `, `chan `, `chan<- `), `*`, `]` (`[]`, `[2]`, `map[K]`) or `(`
(`func(`). `<-chan ` then survives for its own branch, which recurses on the bare qualified element
exactly as it always has, and `descChan` emits
`/*<-*/channel<go.mongodb.org.mongo_driver.mongo.description_package.Topology>` against the
`using description = global::go.go.mongodb.org.….description_package;` the same file writes.

Two subtleties the first cut got wrong, both caught by [CNR](../../Glossary.md#cnr):

- **A non-ASCII byte is a path byte.** Every delimiter this scan looks for is a constructor
  character and all of them are ASCII, but the converter's own synthetic markers are not (`ᴛ`, `ж`,
  `Ꮡ`, `ꓸ`, `Δ`). Treating a multi-byte rune as a delimiter stranded the scan *inside* the type
  name, freezing the path in front of it: `go.main_package/entryᴛ1` for
  `go.main_package.entryᴛ1`. The `PublicizedInterfaceAnonAlias` and `NestedAliasUser` lifted-alias
  goldens are what surfaced it.
- **The scan stops at the generic-argument bracket.** Past it lie type ARGUMENTS whose `,`, space
  and `]` are not constructor text and would strand the scan at the tail of the string.

**Known residual.** When the OUTERMOST constructor is `[]`, its leading `[` is read as the start of a
generic argument list and truncates the path scan to nothing, so `[]<-chan <module path>.T` is still
mangled. Fixing it means no longer treating a *leading* `[` as a generic bracket, which re-routes
every `[]<pkg>/<sub>.T` in the corpus through the other branch (a `_package`-suffix change) — a
corpus-wide emission change that does not belong in a crash fix. It no longer crashes, which is the
part that mattered: see below.

**The crash-proofing is separate from the rendering fix, and is the part that generalizes.** The
array branch now requires the `>` it slices past. A Go stack overflow is a **fatal** runtime error,
not a panic, so the conversion driver's per-file `recover` could not contain it and one unrenderable
type killed the whole run instead of its own package. Bounded, an unrecognized shape is reported by
name on stderr (`Cannot render a C# type name for the unrecognized type expression "…"`) and the
package still converts. Every other branch consumes at least one byte before recursing, so bounding
this one bounds the renderer.

Guarded at both altitudes. `typeNameResolution_test.go` pins the renderer: `TestImportPathStart`
(each constructor, the marker runes, and the paths that must NOT move),
`TestConvertToCSFullTypeNameConstructedModulePaths` (the reported field in all three channel
directions, `*`/`[]`/`[N]`/`map[K]`/nested, plus the single-segment and bare-path cases that must stay
byte-identical, plus the residual above pinned as a decision), and `TestUnclosedBracketTerminates`,
which runs in a **child process** with a 4 MB stack because the condition it guards is unrecoverable
in-process. `TestRecurseChannelOfHyphenatedModulePath`
(`moduleConverter_integration_test.go`) pins that a real declaration of the reported shape reaches
that renderer through an actual `-recurse`, over a network-free fixture whose module path mirrors the
report's — hyphenated first segment, multi-segment tail. Its fixture carries a type **alias** to the
channel alongside the struct field, deliberately: a field DECLARATION emits the readable file-local
alias (`description.Topology`), so the fully-qualified render is computed but never written and a
test reading only the field cannot tell a correct render from a mangled one — while an exported alias
writes the fully-qualified string verbatim into both `main.cs` and the `[GoTypeAlias]` record.

Neuter-proven three ways: both reverted reproduces the reported `fatal error: stack overflow`; the
bound alone reverted fails the child test in 0.02 s; the rewrite alone reverted (bound in place)
makes the converter print the warning, exit 0, and still write every `.cs` — the crash-proofing
demonstrated independently of the rendering — while the integration test fails on the emitted
`global using TopoChan = go.<_chan example.com.mongo_driver.…;`.

### A non-canonically-aliased import renders foreign types via the file's alias
A file that imports a package under an **explicit alias that differs from the canonical package
name** must render that package's types through the alias, not the canonical name. cryptobyte's
`asn1.go` imports `encoding/asn1` as `encoding_asn1` — because the sibling vendored subpackage
`.../cryptobyte/asn1` already claims the canonical `asn1` — so a `*asn1.BitString` parameter must
emit `ж<encoding_asn1.BitString>`. `getAliasQualifiedTypeName` had rendered the canonical `asn1.BitString`
(`importQualifier(pkg.Name())`), which the file's `using asn1 = …cryptobyte.asn1_package` resolves
to the *subpackage* (no `BitString`) — CS0426, and the RecvGenerator faithfully propagated the
wrong qualifier into its `.g.cs`. A `types.Type` carries no source alias, so a per-file
`importPathAliases` map (import path → the alias the file's `using` bound) is threaded into
`getAliasQualifiedTypeName`; a foreign type whose import path the file aliased renders through that alias. Only
**explicitly-aliased** imports populate the map — unaliased / blank / dot / Δ-collision-renamed
imports are absent and keep the `importQualifier(pkg.Name())` fallback, so nothing else changes
(value references were already correct — they come from the AST import name via `convIdent`; only
type references, sourced from `types.Type`, lost the alias). Cleared cryptobyte's CS0426 (which had
masked deeper `Builder.add`/`slice.Value` roots, now banked). GUARD OWED — the shape needs two
packages whose names collide so one import is forced non-canonical, not expressible in the
single-library behavioral corpus.

## Build constraints

### A DOTTED build tag is matched against the host toolchain's tool tags

**A DOTTED build tag (`goexperiment.X`, `amd64.vN`) is matched against the host toolchain's tool tags.** The converter re-checks each file's `//go:build` constraint after `go/packages` has already loaded it (to drop files for the wrong GOOS/GOARCH when converting cross-platform). Its evaluator only handled bare identifiers (`linux`, `amd64`), so a *dotted* tag parsed as a selector and fell through to `false`. That is wrong for an experiment enabled BY DEFAULT: `coverageredesign`, `regabiwrappers`, and `regabiargs` are in the host's `go/build` `ToolTags`, so `go/packages` loaded their `//go:build goexperiment.X` `_on.go` files — but the re-check then re-EXCLUDED them (the selector → `false`), dropping the package-level consts (`testing`'s `goexperiment.CoverageRedesign`, CS0117 ×4). The evaluator now resolves a dotted tag by membership in `build.Default.ToolTags` — so an enabled experiment's `_on.go` survives and a disabled one's `!goexperiment.X` `_off.go` survives, exactly the one `go/packages` chose. Blast radius is only `internal/goexperiment` (the sole stdlib package whose file selection flipped). **Guard owed** — the fix depends on the host toolchain's active tool tags, which the `go2cs/*` behavioral harness cannot express portably; validated by the reconvert A/B (only `internal/goexperiment` changes) and the [census](../../Glossary.md#census) (the consts appear, `testing`'s CS0117 clear). (That tag lookup now lives in the single `matchTag` callback described next; it was a `*ast.SelectorExpr` case while the converter still parsed constraints itself.)

### Build constraints are parsed and evaluated by `go/build/constraint`

The re-check above needs a constraint parser, and for a long time the converter hand-rolled one: it lowercased the expression text and handed it to `parser.ParseExpr`, then walked the resulting `ast.Expr` itself. That parser is for **Go expressions**, and a build constraint is not one. A **Go release tag** is the case that breaks it — `go1.21` reads as the identifier `go1` followed by an illegal `.21` selector, so *every* constraint mentioning a release failed with `failed to parse build constraint: 1:4: expected 'EOF', found .21`. `conversionDriver` warns on that error and falls through to **including** the file, so the failure hid behind an accidentally-correct outcome for as long as the only constraints that hit it were bare `go1.N` gates (five `go-logr` files, surfaced by a `-recurse` probe of `go.opentelemetry.io/otel`). It stops being correct the moment a constraint mixes a release tag with a platform: `//go:build go1.21 && windows` converted for linux lost its platform half along with the rest of the expression.

Constraints are now parsed and evaluated by **`go/build/constraint`**, the package the toolchain itself uses, which retires the whole custom parse/eval layer rather than special-casing the dot:

* **Recognition** — `constraint.IsGoBuild` / `constraint.IsPlusBuild` decide what is a constraint line. Both require the comment at **column zero**, and only the file **header** is scanned (everything above the package clause, line comments only). The regex that preceded them matched any `//go:`-prefixed line *anywhere in the file*, so a `//go:build` quoted in documentation below the package clause gated the file it was describing.
* **Precedence** — a `//go:build` line wins outright; the legacy `// +build` lines apply only in its absence, ANDed across lines, with `,` meaning AND and a space meaning OR inside one. The old regex could not see `// +build` at all (no `//go:` prefix), so a legacy-only file — the norm in third-party modules predating Go 1.17, exactly what `-recurse` meets — converted as **unconstrained**.
* **Evaluation** — `Expr.Eval` drives one `matchTag` callback, so the boolean structure (`&&`, `||`, `!`, parentheses) is the stdlib's problem and go2cs owns only "is this one tag satisfied". Tags resolve in three layers: the `allowedPlatforms` map (GOOS, GOARCH, the derived `unix`/`posix`, the compiler tags, and every `-tags` value), then the Go release tags, then `build.Default.ToolTags` for dotted tags. Matching is **case-sensitive**, as the toolchain matches; the old evaluator lowercased the whole expression first, which quietly made a mixed-case `-tags MyTag` unsatisfiable — `SetTag` stored it verbatim but the lookup folded it.

**Release tags are asked of the go command, not of the compiled-in list.** This is the one place where "use `build.Default`" — right for `ToolTags` — is wrong. Under `GOTOOLCHAIN=auto` the go command re-execs a **newer** toolchain when the main module asks for one, so `go/packages` can be selecting files under Go 1.25 while the `build.Default.ReleaseTags` linked into a go2cs built with Go 1.23 stops at `go1.23`. The re-check would then call `go1.24` false where the loader called it true and drop the file — *and* the `!go1.24` sibling was already dropped upstream by the loader, leaving the package with **neither half** of a fallback pair. Over-exclusion is this pass's recurring failure mode (the `purego` seeding and the `goexperiment` branch above each exist to undo one) and it is the dangerous direction, because the loader has already applied the full constraint for the target platform — anything this pass subtracts is real code. So release tags come from `go env GOVERSION` run in the same directory `packages.Load` uses, expanded to `go1.1`…`go1.N`. An unreachable or unparseable toolchain falls back to the compiled-in list — never to an empty one, which would make every `go1.N` false.

That subprocess costs ~300 ms on Windows, so it is contained twice. Resolution is **lazy** — `matchTag` tests the `go1.N` *shape* before resolving anything, so a constraint naming no release tag never pays it; and the answer is then cached per **module root**, because GOTOOLCHAIN keys on the module. Both halves earn their place: the behavioral corpus is **569 separate modules**, so the cache alone would still spend 569 lookups answering a question not one of those packages asks (no behavioral constraint mentions a release tag), while `-stdlib` reaches the lookup exactly once — `sort` is the only standard-library package with a `go1.N` gate — and `GOROOT/src` carries one `go.mod` above all 302 packages anyway. Note this does not help the **type checker** go2cs links in, which is still whatever release compiled it; a module whose `go` directive exceeds that still needs go2cs rebuilt on a newer toolchain.

Guarded by `src/go2cs/buildConstraints_test.go` (release tags bare/negated/compound, the legacy `+build` grammar, extraction precedence, loader-toolchain resolution), each assertion verified to fail against the pre-fix converter.

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
