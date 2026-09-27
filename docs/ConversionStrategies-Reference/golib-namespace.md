# The `go.golib` support namespace
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#the-gogolib-support-namespace)

golib's hand-written support types (`SparseArray<T>`, `PinnedBuffer`, `TypeExtensions`, `HashCode`, `FatalError`, …) live in the **`go.golib`** child namespace — deliberately NOT `go.<any Go package name>`. The namespace was originally `go.runtime`, which collides with the real `runtime` package: converted code imports runtime as `using runtime = runtime_package;` inside `namespace go`, and a child namespace `go.runtime` visible from any referenced assembly (golib is referenced by *every* project) wins simple-name lookup over the alias — CS0576 at every `runtime.X` use (surfaced by `iter`/`internal/weak` in wave 1). The same reasoning forbids `go.internal`, `go.sync`, etc.; `golib` is not a Go stdlib package name, so the child namespace can never collide with an import alias. Emitted code references these types via the child namespace (`new golib.SparseArray<T>{…}`), which resolves inside `namespace go` with no using directive.

The general form of this collision — a REAL parent/child package pair — is handled by **Δ-renaming the import alias**. A C# using alias declared inside a namespace conflicts with a same-named child namespace visible from ANY transitively referenced assembly (CS0576 at every use), and transitivity makes this common: `runtime.csproj` itself references `runtime/internal/math|sys` (namespace `go.runtime.@internal`), so *every* package importing `runtime` sees a `go.runtime` child namespace — `iter` and `internal/weak` surfaced it in wave 1 (`weak`, in namespace `go.@internal`, collides with `go.@internal.runtime` from `internal/runtime/*` instead). A pre-pass computes the package's transitive Go import closure (exactly mirroring MSBuild's transitive ProjectReference visibility), derives every child-namespace chain it contributes, and Δ-renames any import alias the current package's namespace would capture: `using Δruntime = runtime_package;` with uses `Δruntime.Goexit()` — the established collision marker. The rename propagates through one lookup to the using emission, package-qualifier identifiers, and cross-package type-name prefixes; a package with no collision emits byte-identically. (The behavioral corpus sees this on `io` — the real Go closure contains `os → io/fs`, hence `go.io` — captured in the `AnonymousInterfaces` golden as `Δio`.)

Three properties of the rename, established empirically (2026-07-16 review of the `Δmath` emissions; ruled working-as-designed): **(1) The trigger is per-file and, for a top-level parent package, fires only in packages emitting into `namespace go`.** The collision key is `<packageNS>.<alias>`, and usings are per-file — so `math` (whose own closure always contains `math/bits`, hence `go.math`) renders as `Δmath` in exactly the `namespace go` importers' math-importing files (strconv's ftoa/atof/eisel_lemire, fmt's scan, reflect's value, expvar, testing's benchmark), while every nested-namespace importer (`go.compress`, `go.crypto`, …) keeps the clean `using math = math_package;`: an alias declared inside the file-scoped nested namespace wins simple-name lookup before the outer `go.math` is consulted. That inner-scope exemption is why the clean form dominates the corpus. **(2) The baseline stub is lenient only by omission.** The clean alias compiles against `src/core` solely because the hand-owned stub csprojs omit the Go closure (`core/math` references just golib); in the design-target consumption — full conversion, NuGet packages, `-recurse` apps, all with transitive reference visibility — the clean alias is CS0576 at every use, and hoisting it to compilation-unit scope merely trades that for CS0234 (inside `namespace go` the child namespace shadows the alias). Output must be context-independent, so the conservative rename stands. **(3) The marker cannot be swapped onto "the colliding item".** That item is the child namespace itself — the import-path-mirroring namespace of `math/bits` et al., baked into separately-compiled referenced assemblies — so there is nothing local to rename, and renaming the namespace would break the path-mirroring invariant corpus-wide (the mechanism covers `Δruntime` ×48 files, `Δsync` ×38, `Δio` ×23, `Δsyscall`, `Δunicode`, …). The `MathFloatBits` and `GoNamespaceShadow` goldens pin the `Δmath` form.

<a id="foreign-renamed-types-reference-the-recorded-imported-type-alias"></a>Moved to [Foreign renamed types reference the recorded imported-type alias](shadowing.md#foreign-renamed-types-reference-the-recorded-imported-type-alias).

<a id="a-foreign-packages-collision-rename-is-derived-from-that-package-not-from-the-conversion-run"></a>Moved to [A foreign package's collision rename is derived from that package, not from the conversion run](shadowing.md#a-foreign-packages-collision-rename-is-derived-from-that-package-not-from-the-conversion-run).

<a id="a-foreign-packages-re-exported-type-alias-is-derived-from-that-package-too"></a>Moved to [A foreign package's re-exported type ALIAS is derived from that package too](type-aliasing.md#a-foreign-packages-re-exported-type-alias-is-derived-from-that-package-too).

<a id="a-dot-imported-renamed-type-is-spelled-through-the-same-alias-as-the-qualified-reference"></a>Moved to [A DOT-IMPORTED renamed type is spelled through the same alias as the qualified reference](shadowing.md#a-dot-imported-renamed-type-is-spelled-through-the-same-alias-as-the-qualified-reference).

<a id="the-reflection-bridge-answers-a-read-where-the-answer-exists--four-members-that-did-not-2026-08-19"></a>Moved to [The reflection bridge answers a read where the answer EXISTS — four members that did not (2026-08-19)](reflection/values.md#the-reflection-bridge-answers-a-read-where-the-answer-exists--four-members-that-did-not-2026-08-19).

<a id="a-typed-nil-keeps-its-type-across-both-interface-space-boundaries"></a>Moved to [A typed nil keeps its type across BOTH interface-space boundaries](reflection/values.md#a-typed-nil-keeps-its-type-across-both-interface-space-boundaries).

## Package aliases shadowed by method names

- **Package aliases shadowed by method names** qualify through the `_package` class
  (`sort_package.Sort(…)` — flate's `byLiteral.sort` bound the method group, CS0119).
  Guarded by `SortArrayType` (`PeopleByAge.sort`).
  Same-package tests also participate in that shadow set even during an **ordinary production
  conversion**. Under recompile fallback an in-package `_test.go` declaration lands in the SAME C#
  package class and can shadow a production alias; under white-box reference it lives in a bridge,
  but external-test references still need one stable production spelling — yet go/packages'
  production package omits test files. hash/maphash's `smhasher_test.go`
  declares `func (k *bytesKey) bits() int` while `maphash_purego.go` imports `math/bits`, so
  `bits.Mul64(a, b)` binds the method group in the test assembly (CS0119, plus CS8130 on both
  deconstructed results). Every production package conversion therefore performs a cheap,
  build-constraint-aware directory scan of its in-package `_test.go` function/method names and
  folds them into `packageFuncMethodNames`; it loads and type-checks no test dependency graph.
  External-package tests are excluded because they emit into another C# package class. This makes
  production output mode-stable: ordinary and `-tests` conversion both emit
  `math.bits_package.Mul64(a, b)`. When a test-only declaration is specifically responsible, the
  statement also explains the otherwise surprising spelling:

  ```csharp
  // Fully qualified to avoid alias shadowing by the same-package test declaration "bits".
  var (hi, lo) = math.bits_package.Mul64(a, b);
  ```

  This remains a REFERENCE-spelling change only — never a symbol rename, so production names stay
  pinned (see `testMethodRenames`). On entry to `processTestConversion` the sibling list is cleared:
  the in-package variant sees the declaration in its own typed universe, while the external
  variant's declarations live in a different C# class. Guarded by
  `TestSiblingTestDeclaratorsContributeAliasShadow`, including build-excluded and external-test
  negatives plus the exact explanatory comment; hash/maphash's normal-vs-`-tests` production file
  is also byte-identical.
  A **Δ-renamed foreign CONST reached through that fallback** must still substitute the
  renamed member: the composed lookup key (`time_package.Second`) misses the alias map
  (keyed on the plain package name, `time.Second`), so `getAliasedTypeName` retries with
  the `PackageSuffix` stripped and, on a CONST hit, keeps the `_package` qualifier while
  substituting the alias — `time_package.ΔSecond` (crypto/tls's `Config.time` method ×
  time's `Second` const-vs-`Time.Second()` collision; the raw name bound the
  `Second(this Time)` extension method group, CS0019 ×2). Gated to consts: const entries
  exist only for collision-renamed members, while type entries cover every exported type,
  whose raw `_package`-qualified renders already bind. Guarded by
  `ShadowedImportConstLib`/`ShadowedImportConstUser` (the lib Δ-renames `Peak` for its own
  `Meter.Peak` collision; the user's `gauge.ShadowedImportConstLib` method shadows the
  import and `Span(2) * ShadowedImportConstLib.Peak` reaches the renamed const through the
  fallback, output-compared vs Go).

<a id="converted-programs-write-utf-8-stdout--the-ambient-console-code-page-never-reaches-the-bytes"></a>Moved to [Converted programs write UTF-8 stdout — the ambient console code page never reaches the bytes](manual-conversions/linkname-and-trampolines.md#converted-programs-write-utf-8-stdout--the-ambient-console-code-page-never-reaches-the-bytes).

## Generated code global::-qualifies root-namespace references
Inside a package whose namespace nests a same-named segment (go/build/constraint emits into
`namespace go.go.build`), C# binds a generated reference's leading `go` RELATIVELY to `go.go`
(CS0234). The generators qualify every type-reference position via `GlobalQualify`
(Common.cs); generated signatures also carry parameter REF KINDS (`in slice<byte>`) and a
canned `System.IFormattable` impl where the interface inherits it (the hand-finished io
stub's dyn machinery).

The **converter** faces the same `go.go` shadowing in the import `using` directives it emits for a
`go/*` package (go/token lands in `namespace go.go`, imports `sync`/`unicode` sub-namespaces): a
rooted `using atomic = go.sync.atomic_package;` / `using go.sync;` binds its leading `go` to the
enclosing `go.go` namespace, resolving `go.sync` to the nonexistent `go.go.sync` (CS0234). `rootQualifyIfAmbiguous` routes its rooting returns through `rootQualified`, which emits `global::go.`
instead of a bare `go.` when the package's namespace second segment is itself `go`:

```csharp
using atomic = global::go.sync.atomic_package;
using global::go.sync;
```

The shadowing is NOT limited to `go/*` packages themselves: any package with a `go/*` package
anywhere in its transitive import CLOSURE compiles with `namespace go.go` in scope (its referenced
assembly makes `go.go` a member of namespace `go`), and C#'s inner-to-outer lookup then binds the
bare leading `go` of a rooted using target to that member from EVERY namespace nested under the
root — internal/fuzz (imports go/ast) emitted `using bits = go.math.bits_package;` inside
`namespace go.@internal`, resolving to the nonexistent `go.go.math` (CS0234 ×16, plus the same
shape in net/rpc's `Δhttp` alias and testing/internal/testdeps). `rootQualified` therefore also
emits `global::go.` when `packageChildNamespaces` carries the `go.go` key (populated from the
transitive import closure by `computeImportAliasRenames`' pre-pass). A package with no `go/*`
anywhere in its closure — every package that was compiling before, and all pre-existing behavioral
tests — keeps the bare `go.` prefix, so there is no golden churn. Cleared go/token, go/doc/comment,
go/build/constraint (own-namespace branch); internal/fuzz's 18 CS0234 and net/rpc's latent pair
(closure branch). Guarded by the `GoNamespaceShadow` behavioral test, which covers BOTH branches
through a nested local module literally named `go/nsshadow` (emitting `namespace go.go`, the shape
a single-file behavioral test cannot express): the nested lib imports `math` + `math/rand` so its
own rooted using exercises the own-namespace branch, and the importing `main` package (namespace
`go`, with `go.go` in its closure) exercises the closure branch.

**Under `-tests` the shadow gate spans BOTH compilation halves, and the directly-composed using
targets must go through it too.** The gate had two holes that only a test conversion can expose,
and math/rand/v2 (whose `regress_test.go` imports `go/format`) hit both — 13 of the package's 22
compile errors:

1. *The closure was computed per PACKAGE, not per ASSEMBLY.* A `-tests` run recompiles the
   package's PRODUCTION sources into the test assembly, so that assembly's reference closure is the
   UNION of the production and `_test.go` closures. The production conversion pass saw only its own
   half, never learned `go.go` was in scope, and emitted bare `using bits = go.math.bits_package;`
   into a compilation that did contain `go.go`. `collectSiblingTestClosure` now runs a
   metadata-only (`NeedName|NeedImports|NeedDeps`) load of the test variants before the production
   conversion and records their transitive import paths in `siblingClosureImportPaths`, which
   `computeImportAliasRenames` folds into the closure it walks — so every consumer of the namespace
   maps (the shadow gate, `rootQualifyIfAmbiguous`, `isStrippedGoPathPackageRef`) describes the
   assembly rather than the package. The set is empty for every non-`-tests` conversion, so no
   other output moves.
2. *Targets composed straight from `packageNamespace` bypassed `rootQualified` entirely.* Both the
   package-under-test anchor (`visitImportSpec`'s `isPackageUnderTest` branch, which REPLACES the
   `rootQualifyIfAmbiguous`-derived target with `<packageNamespace>.<pkg>_package`) and the test
   host's `using go.testing_runtime;` were bare, which is why one emitted file could show a
   correctly-qualified `using iotest = global::go.testing.iotest_package;` beside a broken
   `using static go.math.rand.rand_package;`. `globalQualifyRooted` applies the same gate to an
   ALREADY-rooted path and both sites now route through it. It is idempotent and a no-op with no
   shadow, so unshadowed packages emit byte-identically.

Both holes fire for ANY package whose test closure reaches a `go/*` package, and a `regress_test.go`
importing `go/format` is a common stdlib idiom — this is not a v2 quirk. Guarded by
`TestGlobalQualifyRootedForcesGlobalUnderRootShadow` and `TestSiblingClosureContributesRootShadow`
(`src/go2cs/rootShadowQualification_test.go`); the behavioral corpus cannot cover them because it
never runs `-tests` and no behavioral package imports a `go/*` package.

## A sub-package import whose leading segment is a package alias root-qualifies
When a package imports both a parent package and its sub-package — testing/fstest importing `io` **and** `io/fs` — the converter emits `using io = io_package;` (a **type** alias for the io package class) and, for io/fs, a **relative** namespace target `io.fs_package`. In C# the leading `io` segment of `io.fs_package` binds to that type alias, so `io.fs_package[.FS]` resolves to the nonexistent nested type `io_package.fs_package[.FS]` — CS0426 (the `using fs = …` alias line, the embedded `fs.FS` getter, and the generated `TypeGenerator` copies). The converter records every direct-import using-alias identifier bound in the package and prefixes `go.` onto any multi-segment relative namespace/type whose leading segment is one of them, so the segment resolves as the child **namespace** it names:

```go
import (
    "io"
    "io/fs"
)
type fsOnly struct{ fs.FS }
```
```csharp
using fs = go.io.fs_package;
public go.io.fs_package.FS FS;
```

The unqualified `io.fs_package.FS` is retained as the `promotedInterfaceImplementations` map **key** (which feeds alias-less generator files where the relative form resolves). This complements the existing alias-vs-child-namespace Δ-rename, which only catches `<currentNS>.<alias>` collisions. (Recurs for any parent+sub-package import pair; a behavioral guard is owed — the io/fs embedded-interface pattern needs a parent+sub-package pair absent from the core baseline stdlib.)

The same collision detection must see GOROOT-VENDORED namespaces. `visitImportSpec` resolves a GOROOT package's `golang.org/x/…` import to its on-disk `vendor/…` path (and namespace) when the importing file lives under GOROOT, but `computeImportAliasRenames` built `packageChildNamespaces` from the raw `imp.Path()` — so a vendored sub-namespace like `go.vendor.golang.org.x.text.unicode` was absent from the map. `rootQualifyIfAmbiguous` then could not see that a stdlib alias's leading segment collides with it: bidirule (at `vendor/golang.org/x/text/secure/bidirule`, importing both `unicode/utf8` and the vendored `golang.org/x/text/unicode/bidi`) emitted `using utf8 = unicode.utf8_package;`, whose `unicode` bound to the in-scope vendored `unicode` namespace rather than stdlib `go.unicode` (CS0234). `computeImportAliasRenames` now applies `resolveGorootVendoredPath` to each closure path when the package lives under GOROOT — matching the emission — so the vendored namespaces populate the map and the alias root-qualifies to `go.unicode.utf8_package`. Gated on GOROOT so a user module's own `golang.org/x` dependency is untouched; **guard owed** (the fix fires only for a GOROOT-vendored package, which the behavioral harness — never under GOROOT — cannot express; validated by the bidirule reconvert [A/B](../Glossary.md#ab)).

---

[← Labeled Control Flow and Loop Variables](labels-and-loop-variables.md) · [Index](README.md) · [Source Generators →](source-generators.md)
<!-- {% endraw %} -->
