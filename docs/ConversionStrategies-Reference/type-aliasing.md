# Type Aliasing

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#type-aliasing)
Go supports two kinds of [type aliasing](https://go101.org/article/type-system-overview.html#type-definition): a "type definition" and a "type alias declaration".

## Type Definitions
For a Go "type definition" the new type is a distinct type that shares an [underlying type](https://go101.org/article/type-system-overview.html#underlying-type) with its base. Because converted types are structs (no inheritance), the converter relies on the source generators (see [Source Generators](source-generators.md#source-generators)) to emit the bridging needed for these to be used interchangeably while remaining distinct: implicit conversion operators down to the underlying type (via `ImplicitConvGenerator` / `TypeGenerator`), and, where the base is a built-in like `slice`, the relevant interface (`ISlice<T>`, etc.) implementation. A named type also supports the extension methods (receiver functions) of its underlying types, which the generators surface as proxy/overload methods.

When a pointer conversion `(*Target)(srcPtr)` bridges two structurally-identical structs, the converter records an **indirect** (boxing) implicit conversion `Source → ж<Target>` and `ImplicitConvGenerator` emits `implicit operator ж<Target>(Source src) => Ꮡ(new Target(<members>))`. For a **self-boxing** conversion — `Source` and `Target` are the *same* struct (`mspan → ж<mspan>`), which arises from a self-referential struct's recursive sub-struct conversions — that member-by-member reconstruction is both unnecessary and wrong: a pointer field whose target ctor parameter is itself a `ж<…>` was deref'd (`src.f?.Value ?? default!`), and a value cannot bind a pointer parameter (CS1503). The generator detects self-boxing (the boxed element type equals the source) and emits `Ꮡ(src)` instead — boxing a copy of the whole struct directly, identical in effect for a pointer-free struct and correct for one with pointer fields. (Validated by the green baseline build, which regenerates every `.g.cs`, plus the `TypeConversion` behavioral test for the non-self-boxing form; runtime exercised self-boxing for `mspan`, `g`, `stackScanState`, `hmap`, etc.)

## Type Alias Declarations
For a Go "type alias declaration" the alias matches C# aliasing implemented with the `using` keyword. Since the alias may be exported and referenced across files, the converter emits a **global** `using` (C# 10's [Global Using Directive](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-10.0/GlobalUsingDirective.md)) into the package's generated aliases. For example:

```go
type P = *bool
type M = map[int]int
type table = map[string]int
```
```csharp
global using P = go.ж<bool>;
global using M = go.map<nint, nint>;
global using table = go.map<go.@string, nint>;
```

**A `global using` RHS renders csproj-alias numerics as C# keywords.** C# resolves a using directive's target *without reference to other using directives* — aliases are invisible to one another — so the golib csproj-level aliases (`uint64`, `float64`, `any`, …) that resolve everywhere else in the compilation are CS0246 inside `global using X = …;`. The alias-declaration emission (only) rewrites those names to their using-safe keyword/BCL equivalents: fiat's `type p224UntypedFieldElement = [4]uint64` emits `global using p224UntypedFieldElement = go.array<ulong>;`. Body code keeps the Go-visual alias names; already-safe names (`byte`, `bool`, `nint`, `go.@string`) are untouched, and the rewrite deliberately skips dot-qualified names so a package type sharing a builtin name is left alone. (Guarded by the `AliasStructComposite` extension `words` — an alias to `[4]uint64` used as a parameter type, output-compared.)

**An alias to an unnamed array/slice resolves through `types.Unalias` at type-switched decision points.** An alias is a `*types.Alias` (Go 1.22+) — neither the AST's `ast.ArrayType` nor the resolved `*types.Array` — so an emission that type-switches on the syntax node or the unresolved type misses it. Three sites resolve through `types.Unalias`: (1) the **`range` operand dispatch** — `for _, e := range w` on an alias-typed array previously matched no arm and emitted the *whole loop* as a C# comment, a silent behavioral hole; it now emits the normal `foreach (var (_, e) in w)`. (2) the **composite-literal dispatch** — `words{10, 20, 30, 40}` emits the same element-array projection the unnamed literal uses, `new uint64[]{10, 20, 30, 40}.array()`; the alias name renders as an Ident (not an `ast.ArrayType`), so it cannot take the composite-initializer bracket rewrite, and keeping it produced a C# collection initializer on the alias (`new words{…}` — CS1061, `array<T>` has no `Add`). (3) **`var` declarations** — a local or package-level `var w words` allocates the fixed-size backing (`words z = new(4);` / `internal static words gw = new(4);`) instead of `default!`/uninitialized, whose null backing array throws NRE on the first element write. An alias to a *named* type is unaffected: unaliasing lands on `*types.Named` and the existing wrapper-struct arms apply, with the alias name preserved. (Guarded by the `AliasStructComposite` extensions — alias-typed range loop, composite literal, and local + global `var` with element writes, values vs Go.)

**A same-package alias TARGET carries the package's FULL namespace, not just its class.** An exported alias whose target is *lifted* into the package class — `type CorpusEntry = struct{…}` lifts its anonymous struct to a nested `CorpusEntryᴛ1` (and the same for an alias to a same-package named type) — must qualify that target with the package's whole namespace. For a package in a **nested** namespace (`internal/fuzz` → namespace `go.@internal`, class `fuzz_package`; `net/http` → `go.net` / `http_package`) the lifted type lives at `go.@internal.fuzz_package.CorpusEntryᴛ1`. Building the qualifier from the bare `<pkg>_package` class alone dropped the intervening namespace segment, so the emitted `global using CorpusEntry = go.fuzz_package.CorpusEntryᴛ1;` — and the matching `[assembly: GoTypeAlias("CorpusEntry", "go.fuzz_package.CorpusEntryᴛ1")]` that every consumer replays verbatim through its `<ImportedTypeAliases>` block — named a namespace that does not exist → CS0234 at the using-alias line and at every use (internal/fuzz's `CorpusEntry`, ×60). The qualifier is now taken from the same `packageNamespace` that emits the `namespace …;` declaration (minus the root, plus the class), so the target and the declaration always agree: `go.@internal.fuzz_package.CorpusEntryᴛ1`. A **top-level** package's namespace is exactly the root (`go`), leaving no intervening segment, so its target stays `go.<pkg>_package.…` — the emission is byte-for-byte unchanged there. (Guarded by the `NestedAliasUser` behavioral test — a top-level `package main` that imports its own nested `inner` subpackage, whose C# namespace is `go.NestedAliasUser`; `inner` exports an anon-struct alias `Entry`, and both `inner`'s own `global using` and the consumer's imported `global using innerꓸEntry` resolve to `go.NestedAliasUser.inner_package.Entryᴛ1`, values vs Go.)

**The whole RHS is namespace-ROOTED, at every nesting depth — the alias resolves at COMPILATION scope.** The two paragraphs above each close one hole in this wall; this is the wall. C# resolves a using alias's target *as if the immediately containing compilation unit had no using directives*, which puts it **outside** the file's `namespace go;`, **outside** the emitted `<pkg>_package` class, and with none of the csproj-level golib aliases in effect. Every other rendering in the converter elides the root namespace from nested names, because every other rendering lands *inside* `namespace go` — where the elision is legal and is what makes the emitted C# read like Go. Only the outermost name was rooted here, so each type ARGUMENT under it named nothing: `type names = []string` emitted `global using names = go.slice<@string>;`, CS0246 on `@string`. The same held for `slice`, `error`, `complex64` and `ж`; for a same-package `Header` (which is `go.main_package.Header` at that scope); for a cross-package `io_package.Reader` (`go.io_package.Reader`); and for the BCL `Func`/`Action` of a func-type alias, since `System` is not in scope there either. Nine aliases produced seventeen CS0246.

The alias emission therefore renders in a **rooted-nesting** mode, in which the target *and* everything it nests carry full qualification:

```go
type Header struct{ Name string }

type nested = map[string][]Header
type fn = func(string) int
type rdrs = []io.Reader
```
```csharp
global using nested = go.map<go.@string, go.slice<go.main_package.Header>>;
global using fn = System.Func<go.@string, nint>;
global using rdrs = go.slice<go.io_package.Reader>;
```

Four qualifiers are in play and they are not interchangeable: golib types root to `go.`, the BCL delegates to `System.`, golib's variadic delegate FAMILY (`Actionꓸꓸꓸ`/`Funcꓸꓸꓸ`) back to `go.`, and a same-package name to `go.<ns>.<pkg>_package.` — the mechanism of the paragraph above, applied now at every depth rather than to the target alone (which is why the target no longer needs a prefix computed for it separately). The csproj-alias names are the exception that proves the rule: `uint64`, `float64`, `any` and friends are not members of `go` at all, so they are **substituted** with the keyword or BCL type they stand for rather than rooted — the first paragraph's rewrite, moved inward. An alias whose target is ITSELF an alias resolves through `types.Unalias` before rendering, since a C# using alias may not name another using alias.

This is a **user-code** defect rather than a corpus one, and the census says so precisely for the type-ARGUMENT arm: the whole converted standard library declares exactly four package-level aliases with type arguments (fiat's `p224`/`p256`/`p384`/`p521`, each `[4]uint64`), and all four take a C# keyword as their argument, so that arm moves nothing. An end-user package that aliases a slice, map, channel or func type — the ordinary `type Handlers = map[string]Handler` — hit it on the first build, and a `-recurse` module conversion hit it over a third-party type.

The **substitution** arm is the one with corpus sites, and they were live **CS0234** nobody had seen. A csproj-alias name reaching the alias RHS as the WHOLE target was rooted rather than substituted — `go.int32`, which the compiler rejects with "the type or namespace name `int32` does not exist in the namespace `go`", since `int32` is a `<Using Alias=…>` for `System.Int32` and not a member of `go` at all. `getUsingAliasSafeTypeName` could not catch it because that sweep deliberately skips dot-qualified names, exactly so a package type sharing a builtin name is left alone. Six sites carried it, all cgo `_C_*` typedefs in **darwin-exclusive** files (`os/user/darwin/cgo_lookup_syscall.cs`, `net/darwin/cgo_unix_syscall.cs`), which is why they stayed latent: the default `$(GoTargetOS)` is `windows`, so nothing compiles them. `type _C_int = int32` now emits `global using _C_int = int;`, and the neighbours that were already right are unmoved — `_C_char = byte` (a C# keyword) and `_C_size_t = go.uintptr` (`uintptr` IS a real golib struct, so rooting it is correct). (Guarded by the `PackageAliasRootedTypeArgs` behavioral test — twenty-five package-level aliases covering golib element types, keyword element types that must NOT be rooted, same-package named types at one and two levels of nesting, a lifted anonymous struct and interface, a cross-package interface, both directional channel forms, both delegate spellings, and an alias to an alias, output-compared vs Go. Also pinned by `TestRecurseChannelOfHyphenatedModulePath`, whose assertion recorded the unrooted cross-package form until this landed.)

**Rooting is IDEMPOTENT: an already-`global::`-rooted target is not rooted again.** The rooted mode
above prefixes the root namespace onto every name it renders, and one caller hands it names that are
already rooted — a **white-box test conversion**, whose test-alias qualifiers build a reference to a
production declaration with an explicit `global::` (`global::go.net.netip_package.uint128`). Prefixed
a second time that becomes `go.global::go.net.netip_package.uint128`, which is **CS7000** "unexpected
use of an aliased name": `global::` is the root, so anything in front of it is by construction not a
name. `net/netip`'s `export_test.go` re-exports two unexported production types this way
(`type Uint128 = uint128`, `type AddrDetail = addrDetail`) and both of its alias lines failed to
parse, taking all 266 of the package's verdicts with them. The renderer now returns a `global::`
target unchanged, stated at the renderer rather than at the one caller, because every future caller
wants the same answer. (Guarded by `mixedKeyedComposite_test.go`'s
`TestRootedUsingAliasKeepsGlobalQualifier`, which asserts the rooted *and* unrooted renders both
leave such a name alone.)

## A collision-renamed alias chain resolves to its concrete target

**A collision-renamed alias chain resolves to its concrete target.** When an exported type's name *collides* with a method name it is `Δ`-renamed (see [Type-vs-Method Name Collisions](shadowing.md#type-vs-method-name-collisions)); and when that type is *also* an empty interface — `type Token any` colliding with a `Token()` method, encoding/json's shape — the producer's `package_info.cs` carries a **two-hop chain**. The collision analysis records `Token → ΔToken`, and `visitTypeSpec` (which renders an empty-interface target as `object`) records the renamed declaration `ΔToken → object`:

```csharp
[assembly: GoTypeAlias("Token", "ΔToken")]
[assembly: GoTypeAlias("ΔToken", "object")]
```

A consumer that resolves only the FIRST hop and then qualifies the intermediate `Δ`-name as a package member emits `global using jsonꓸToken = go.encoding.json_package.ΔToken;` — but `ΔToken` is an assembly-scoped `global using`, **not** a namespace member of `json_package`, so it is CS0426 (encoding/json's `Token` consumed by html/template, internal/coverage/cfile, expvar, log/slog, internal/fuzz, …). The imported-alias loader (`loadImportedTypeAliases`) therefore follows the chain within the producer's OWN exported aliases to its **concrete** target, emitting `global using jsonꓸToken = object;`. A chain whose final target is a real `Δ`-renamed member (a delegate/struct such as `ΔFilter`, which is *not* itself an exported alias) stops there and stays package-qualified, unchanged. Guarded by the `CrossPkgLib`/`CrossPkgUser` pair — an empty-interface `Token` colliding with a `Sensor.Token()` method, named as a `var` type in the consumer and its boxed value read back, output-compared vs Go.

## A same-named cross-package alias target is fully qualified

**A same-named cross-package alias target is fully qualified.** Two *different* packages can share a Go package name — html/template and text/template are both `package template`. When such a package aliases the other's type — html/template's `type FuncMap = template.FuncMap`, whose target lives in text/template — the alias RHS must name the target's OWN `(namespace, class)`: `go.text.template_package.FuncMap`. `getFullyQualifiedTypeName` had gated its cross-package branch on the package *name* (`pkg.Name() != packageName`), so a same-named foreign type read as *same-package* and fell through to the `t.String()` path, whose cross-package slash-strip drops BOTH the `text` path segment AND the `_package` class — emitting `global using FuncMap = go.template.FuncMap;` (CS0234; `template` is not a namespace of `go`). The check now compares package **identity** (`pkg != v.pkg`), matching `getAliasQualifiedTypeName` and `collectCrossPackagePaths`, so the branch fires and the target fully qualifies. (A code-*body* reference already rendered correctly — `getAliasQualifiedTypeName` keyed on identity — so only the `global using` alias RHS was wrong.) Guarded by the `CrossPkgSameNameAlias` behavioral test: a `package atomic` that aliases the same-named `sync/atomic`'s `Int32` (`type Int32 = atomic.Int32`), whose `global using` RHS must render `go.sync.atomic_package.Int32`, not the dropped-segment `go.atomic.Int32`.

## A foreign package's re-exported type ALIAS is derived from that package too

The sibling class, and the one that hits real end-user code hardest. `os` declares
`type FileMode = fs.FileMode` (likewise `FileInfo`, `DirEntry`, `PathError`), and a re-export takes
`visitTypeSpec`'s **using-alias** arm: the converted `os` emits an assembly-scoped
`global using FileMode = go.io.fs_package.FileMode;` and publishes
`[assembly: GoTypeAlias("FileMode", "go.io.fs_package.FileMode")]`. The re-export is therefore a *using
alias inside os's assembly*, **never a member of `os_package`** — so a consumer converted without that
artifact emits `os.PathError` and gets `CS0426: the type name 'PathError' does not exist in the type
'os_package'`. Exactly the run-composition dependence of the collision renames, one metadata class over.

`foreignTypeAliases.go` derives these under the same invariant — what a dependency publishes is a function
of that package's own declarations — from its `go/types` scope, plus its syntax for the one distinction only
a declaration's RHS carries. Two declarations take the using-alias route and are reproduced:

| Dependency declares | Published entry | Consumer emits |
|---|---|---|
| `type FileMode = fs.FileMode` | `("FileMode", "go.io.fs_package.FileMode")` | `osꓸFileMode` |
| `type Kind = abi.Kind` (and `abi` Δ-renames `Kind`) | `("Kind", "go.@internal.abi_package.ΔKind")` | `reflectliteꓸKind` |
| `type PublicKey any` (a DEFINED type over the empty interface) | `("PublicKey", "object")` | `object` |
| `type Reader io.Reader` (a DEFINED type over a named interface) | `("Reader", "go.io_package.Reader")` | `pkgꓸReader` |

Three details make the reproduction exact rather than approximate:

* **The alias TARGET carries the target package's OWN collision rename.** `internal/reflectlite`'s
  `type Kind = abi.Kind` publishes `go.@internal.abi_package.ΔKind`, because `internal/abi` Δ-renames `Kind`
  against `(*Type).Kind()`. In a full run that Δ arrives from `abi`'s parsed `package_info.cs` (the
  `importedTypeAliases` consult in `convertToCSFullTypeName`'s default arm); the derivation recomputes it
  with the same foreign-package-aware `packageHasMethodNamed` test the collision lane uses, so the two
  sources agree.
* **A Go type ALIAS is exempt from the collision lane.** `performNameCollisionAnalysis` records only
  *defined* types (`!typeSpec.Assign.IsValid()`), so an alias name that *also* names a method is **not**
  Δ-renamed — `reflectlite` declares both `type Kind = abi.Kind` and `(*rtype).Kind()` and still publishes
  the plain source name `Kind`. The two derivations split on exactly that line: a colliding *defined* type
  belongs to the collision lane (which owns the `Token`/`ΔToken`/`object` two-hop), a colliding *alias* to
  this one. Getting that boundary wrong either double-publishes one source name with two targets or drops
  `reflectlite`'s entry entirely.
* **An empty-interface target is `object`, imported BARE.** `type PublicKey any` is not an alias at all but
  a defined type over the empty interface, which has exactly that interface's method set and so takes the
  using-alias arm too. Its target is the C# keyword, not a package member (`isCSharpBuiltinTypeName`) —
  `crypto`'s `PublicKey`/`PrivateKey`/`DecrypterOpts`, `plugin`'s `Symbol`, `database/sql/driver`'s `Value`.

**Deliberately not derived** (a wrong target is worse than none — a missing entry leaves the reference
exactly as it converts today, a wrong one names a type that does not exist): a composite or basic RHS
(`type Table = map[string]int`, whose rendering runs the whole `convertToCSFullTypeName` lowering) and an
alias-to-an-alias chain; an **anonymous** struct/interface RHS, which is *lifted* under a generated name
only a conversion assigns (`internal/fuzz`'s `type CorpusEntry = struct{…}` → `CorpusEntryᴛ1`); a generic
target; a methodless named **func** type, rendered inline as its base delegate with no named type to point at
(the same omission `typeCollisionAliases` and `writePackageInfoFile` make); and a target that is **itself**
emitted as a using alias by its own package, which would need a second hop this derivation does not follow.
Each declines by shape, from the dependency's own declarations, so the decision is stable across runs.

Same use-gating as the collision renames: a derived alias's `global using` reaches the consumer's
`package_info.cs` only once an emitted reference has resolved through it. Evidence, taken with the fix
neutered and restored: a standalone `os.FileMode`/`os.FileInfo`/`os.PathError` + `fs.WalkDir` program
converted with `go2cs <dir>` against an output root holding **no** converted stdlib failed with the CS0426
above and now compiles and runs with output byte-identical to `go run .`; single-package `-stdlib crypto/ecdh`
reconverts `ecdh.cs` byte-identically to the committed full-run corpus where before it emitted the
nonexistent `crypto.PublicKey`; and a whole-stdlib reconvert is byte-for-byte unchanged, the derivation
running only where the loader previously did nothing at all.

**Still open — the `GoImplement` pairs** (`loadPackageImplements`), and *honestly* so rather than pending:
they are recorded at CONVERSION time from the cast and witness sites a dependency's own bodies contain, so
which adapter classes its assembly actually carries is a product of its **emission**, not of its
declarations. There is nothing sound to compute from `go/types`: an over-approximation (every exported type
× every exported interface) would name adapters that do not exist — CS0246, strictly worse than the present
behavior, where the consumer records and emits its own local adapter (`io_SectionReaderжReader` instead of the
provider's `io.SectionReaderжReader`), which compiles and behaves identically and only duplicates the class.
The class retires with the runtime interface shells rather than with a derivation: once a concrete-to-interface
conversion goes through a runtime-constructed shell instead of a compile-time adapter, there is no per-pair
record left to be missing.

(Guarded by `foreignTypeAliases_test.go`: a three-package fixture — a consumer, the `dep` whose re-exports
are under test, and the `other` it re-exports from — carrying one declaration of every published shape and
every declined one, asserting the derivation, its independence from run-accumulated state, and the end-to-end
render with no `package_info.cs` present.)

## An aliased import's imported type ALIAS renders as its `global using` name

**An imported type ALIAS through an aliased import renders as its `global using` name.** A `global using` alias is not a member of the package class, so `import pl "PALib"` with `pl.B2{V: 1}`, where `B2` is an exported alias, cannot render `new pl.B2(…)` (CS0426). The alias table is keyed by the package's declared name, and `aliasResolvedSelector` looks a published, non-const type alias up under that name, rendering `new PALibꓸB2(…)` as the canonical import does. Every other member keeps the file's alias (`new pl.Box(…)`). (Guarded by the `AliasImport` behavioral test's `aliased.go`.)

## The ALIAS kind takes the lift too

**The ALIAS kind takes the lift too — and for a different reason.** A local declaration that emits a
`using` ALIAS rather than a nested type — a real `type X = Y`, or a defined type over a *named*
interface such as `type X any` — was the last local type-declaration kind not taking the hoist. It
needs no member-level redirection (an alias is emitted at file scope either way), but it needs the
NAME, because the alias it writes is a `global using`: scoped to the whole **compilation**, not to
the file, let alone the function. Two functions declaring `type testFnc any` therefore claimed one
alias name — `CS1537 the using alias 'testFnc' appeared previously in this namespace` — whether they
sat in one file or in two of the same compilation. `archive/tar`'s suite is the shape: `testFnc` is
declared in `writer_test.go`'s `TestWriter` **and** `TestFileWriter`, and again in `reader_test.go`'s
`TestFileReader`, with `fileMaker` alongside it; three diagnostics held all **97** of that package's
verdicts. The naming half of `liftLocalTypeDecl` is now the shared `liftLocalTypeDeclName`, and the
alias branch calls it when `v.inFunction`, emitting `global using TestWriter_testFnc = object;`.

The reference mapping is registered under a **guard**, `liftedTypeDeclaredBy`: only a `*types.Named`
or `*types.Alias` whose own `Obj` **is** this declaration qualifies. A wrong key here renames every
reference to an unrelated type — `type X = Header` inside a function binds the declaration's object
to the *existing* `Header`, and (without materialized aliases) `type X = int` binds it to plain
`int`, so keying the lift on either would rewrite every `Header`, or every `int`, in the file.
Anything that does not qualify registers nothing and renders exactly as before. A function-local
declaration is also no longer published in `exportedTypeAliases`: it is not part of the package's
exported surface whatever its Go name looks like, and after the lift the name a consumer would
import does not exist. **Zero production-corpus impact by construction** — an AST scan of the Go
1.23.1 sources finds *no* function-local alias-or-defined-over-interface declaration in any compiled
stdlib file (all 50 hits are `internal/types/testdata`, which is never built), which is why only two
test suites ever met it. (Guarded by the `LocalTypeAliasScope` behavioral test — the same local
names declared in two functions of one file and again in a second file of the same package, plus a
real `type hdr = Header` alias whose target is used bare alongside it; the unfixed converter emits
five duplicate `global using` lines.)

**Known residual, a different one, in the same emission line:** an alias whose target is an *unnamed
composite* renders its type ARGUMENTS unrooted — `type names = []string` emits `global using names =
go.slice<@string>;`, where only the outermost name is rooted and `@string`, a nested `slice`,
`error`, `complex64`, a same-package `Header` and a foreign `io_package.Reader` all arrive bare and
do not resolve at compilation scope (`CS0246`). This is **package-level**, not function-local, and
predates the lift above; `getUsingAliasSafeTypeName` exists for exactly this class of problem
(a using-alias RHS is resolved without reference to other using directives) but rewrites only the
csproj-level golib name aliases, never the rooting. No converted stdlib package declares such an
alias, so the corpus has never reached it; a converted user module would.

## Generic Type Aliases

A Go 1.24 generic alias (`type A[T any] = Box[T]`) cannot be a C# `using` alias: a `using` directive cannot declare type parameters (`using A<T> = Box<T>;` is CS1002), and a closed alias cannot take type arguments at a use (CS0307). A Go alias *is* its target (identity, method set, assignability), so the converter renders the target wherever the alias is named:

```go
type P[K comparable, V any] = Pair[K, V]
type StrPair[V any] = Pair[string, V]
type IntMap[V any] = map[int]V

func swap[T comparable](p P[T, T]) P[T, T] { return P[T, T]{Key: p.Val, Val: p.Key} }
m := IntMap[string]{1: "one"}
```
```csharp
// type P[K comparable, V any] = Pair[K, V]
// type StrPair[V any] = Pair[string, V]
// type IntMap[V any] = map[int]V

internal static Pair<T, T> swap<T>(Pair<T, T> p) {
    return new Pair<T, T>(Key: p.Val, Val: p.Key);
}
var m = new map<nint, @string>{[1] = "one"u8};
```

- **Declaration.** The alias emits a one-line comment holding its Go text, and nothing else: no `global using` and no `GoTypeAlias` record. A record would reach every importer as a `global using` over an unbound `go.T`, breaking packages that never name the alias.
- **Uses.** `genericAliasTarget` (typeNameResolution.go) answers `types.Unalias(t)` for a `*types.Alias` with type arguments or type parameters. `getAliasQualifiedTypeName` and `getFullyQualifiedTypeName` call it first, and a composite literal whose type syntax names a generic alias renders the resolved target instead of the syntax. An alias of an alias resolves all the way (`SP[bool]` → `Pair<@string, bool>`), and a cross-package target takes the file's import alias (`ga.Box<nint>`).
- **Plain aliases are untouched.** An alias without type parameters, including one over an instantiated generic (`type IntBox = Box[int]`), keeps its `global using`, its record and its name at every use.
- **Not supported:** a generic alias whose target is an anonymous struct or interface type (`type Cell[T any] = struct{ Row, Col T }`). The converter reports it with a conversion warning and does not yet convert it: the declaration emits only the comment, and a use does not compile. Its target is a different anonymous type per instantiation, and lifting it as a generic type of its own would make `Cell[int]` a different type from `struct{ Row, Col int }`.

Guarded by the `GenericTypeAlias` behavioral test (with `GenericTypeAliasLib` and `GenericTypeAliasBystander`, a consumer that never names an alias) and by `TestGenericTypeAliasRendersItsTarget` and `TestPlainTypeAliasKeepsItsName`.

**A func-type alias target is built from its signature.** `type DurFn = func(time.Duration) int` emits `global using DurFn = System.Func<go.time_package.Duration, nint>;`, and a variadic `type Var = func(...int) int` emits `global using Var = go.Funcꓸꓸꓸ<nint, nint>;`. `usingAliasDelegateType` renders each element in its rooted form and roots the delegate family at `System.` (or `go.` for the variadic family). Named results keep their names in the tuple (`(nint n, bool ok)`). An importer reads the record's `System.`-rooted target as it is, never qualifying it into the declaring package's class. (Guarded by the `AliasImport` behavioral test, with `AliasImportLib`.)

---

[← Generic Constraints](generic-constraints.md) · [Index](README.md) · [Delegates to Value Receiver Instances →](value-receiver-delegates.md)
