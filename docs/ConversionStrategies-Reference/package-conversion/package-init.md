# Package Conversion: Package Initialization

[Reference index](../README.md) · [Package Conversion](../package-conversion.md) · [Summary of this topic](../../ConversionStrategies.md#package-conversion)

This page covers how a converted package's `init` is made to run: an import forces the imported package's initialization, and a `-tests` project forces the package under test's own.

## Import initialization

### An import forces the imported package's `init` to run

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
[Package-Level Variable Initialization Order](../variable-initialization-order.md#package-level-variable-initialization-order)) and the
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

## Test projects

### A `-tests` production-reference project forces the package under test's own `init`

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

---

[← Package Conversion](../package-conversion.md) · [Index](../README.md)
