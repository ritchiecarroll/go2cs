# Package-Level Variable Initialization Order

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#package-level-variable-initialization-order)

Go initializes package-level variables in **dependency order** (spec: "within a package, package-level variable initialization proceeds stepwise, each step selecting the earliest variable … that has no dependencies on uninitialized variables"), where dependencies are resolved **through function calls and function literals** referenced by the initializer. The default conversion emits a package var as a C# static field with an initializer — but C# executes static field initializers in textual order **within** one file of the partial package class and in an **undefined** order **across** files. Three dependency shapes therefore break at runtime while compiling cleanly:

1. **Cross-file:** syscall's `var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")` (syscall_windows.go) reads `modkernel32` declared in zsyscall_windows.go — with the wrong file order the receiver box is nil (NullReference in the type initializer, which on windows crashes any program importing `os` at startup).
2. **Cross-file through a function:** syscall's `var Stdin = getStdHandle(STD_INPUT_HANDLE)` — the initializer calls a package *function* whose body reads zsyscall_windows.go's `procGetStdHandle`. Same failure, invisible to a direct-reference scan; dependencies must be resolved transitively through same-package function bodies (and func-literal bodies — an IIFE initializer executes at init time), exactly as Go's own analysis does.
3. **Same-file forward reference:** `var first = base + 1` declared above `var base = 41` — C# reads `base`'s zero value (silently wrong value, no crash).

**The conversion:** `collectMovedInitVars` (converter: `initOrderOperations.go`) walks `types.Info.InitOrder` — Go's authoritative dependency-sorted initializer list — resolving each initializer's same-package var dependencies transitively through package function/method bodies. An initializer moves when a dependency is cross-file, a same-file forward reference, or **itself moved** (a moved var is only assigned in the ctor, so its dependents can no longer stay field initializers regardless of layout). A moved var is emitted as a **bare field** plus a tiny **init method beside it in its home file** (so the rendered expression keeps that file's using aliases), and a generated `package_init.cs` supplies the package class's **static constructor**, calling the methods in InitOrder:

```go
// syscall_windows.go
var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")
```

In `syscall_windows.cs` the variable is a bare field beside the method that sets it:

<!-- source: src/core/syscall/windows/syscall_windows.cs:561-562 -->
```csharp
internal static ж<LazyProc> procSetFilePointerEx;
internal static void initᴛprocSetFilePointerEx() { procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8); }
```

The generated `package_init.cs` calls every relocated initializer from the class's static constructor,
in `types.Info.InitOrder`:

<!-- source: src/core/syscall/windows/package_init.cs:9-12 -->
```csharp
partial class syscall_package {
    static syscall_package() {
        initᴛprocSetFilePointerEx();
        initᴛStdin();
```

This is correct by C#'s own initialization guarantees: **all** static field initializers (every partial-class file) run **before** the static-constructor body, so every non-relocated dependency is already initialized when the ctor runs; the ctor then applies the relocated initializers in Go's order. Vars with no order hazard (the overwhelming majority — only a small minority of standard-library packages relocate anything) keep their readable inline form.<!-- At the time of writing: 25 of the 302 stdlib packages relocated anything. --> Cross-**package** order needs no handling: accessing another package's static field triggers that type's initialization first (.NET guarantees), matching Go's imported-packages-first rule. Adding an explicit static ctor also removes `beforefieldinit` from the package class, giving it *precise* initialization semantics.

Notes: a **blank** (`_`) initializer never relocates (its value is unreadable, so its order is immaterial — it still runs as a field initializer for its side effect); the `initᴛ` method name composes the TempVarMarker so it cannot collide with any Go identifier; an **addressed** global relocates as a default-valued heap box whose ctor assignment writes through the ref property into the same box; a **tuple-deconstructing** spec relocates as one unit (see the next subsection). A moved plain var whose initializer spreads a multi-value inner call (the `template.Must(template.New(…).Parse(…))` shape) relocates too: the spill that the inline form writes as a hidden static tuple field becomes a local statement of its `initᴛ` method (`var (ᴛ1, ᴛ2) = pair(); spread = must(ᴛ1, ᴛ2);`), so the call waits for the same dependencies the var does.<!-- The spill: src/go2cs/visitValueSpec.go:771 (`// A multi-value inner call spread in the initializer`) and :782 (`v.globalDeclHoistStatements = moved`); src/go2cs/convExprList.go:285 (`globalFieldHoist = !v.globalDeclHoistStatements`). Probe at 3e3f18925a: `var spread = must(pair())` above `var base = 41` emits `internal static void initᴛspread() {` with the deconstruction inside it, and no warning. --> Guarded by the `PackageVarInitOrder` behavioral test (all three hazard shapes plus IIFE and moved-dependency closure, output-compared vs Go).

## A TUPLE-deconstructing package var relocates as ONE unit

A package-level `var a, b = f()` — one multi-value call deconstructed across the names — is a single initialization step in Go: `types.Info.InitOrder` carries **one entry for the whole spec** with every name in its `Lhs`, so `collectMovedInitVars` flags all of a spec's non-blank names together under one shared ordinal, and the emission registers **one `initᴛ` method per spec** at that ordinal — `writeOrderedInitCalls` needs no new bookkeeping. `crypto/internal/edwards25519`'s `var identity, _ = new(Point).SetBytes(…)` and `var generator, _ = …` show why it must: they reach `feOne` and `d` — declared *later* in the same file — through the package's own `(*Point).SetBytes`, so inline field initializers would run first, `identity` would read a null `feOne`, `field.Subtract` would null-dereference, and the package's static constructor would throw before any test ran. The standard-library occurrences are edwards25519's (all targets) and `os`'s `executable_darwin.go` (darwin).<!-- full detail in [`docs/phase4/FINDING-init-order-tuple-specs.md`](../phase4/FINDING-init-order-tuple-specs.md) (internal record, unlinked 2026-10-26, OQ-1) --><!-- Relocation landed 2026-08-11. At the time: edwards25519 went from 0 of 55 verdicts to 52 of 55 (three residuals separate pre-existing roots); census two production occurrences on windows (both edwards25519) and two latent on darwin. -->

Two emission sub-shapes (`writeMovedPackageTupleVarSpec`, `visitValueSpec.go`), both turning every name into a **bare field**:

**One non-blank name** (the edwards25519 shape): the method assigns its component directly from the once-run call; blank siblings keep their uninitialized `_ᴛNʗ` fields — the call now runs in the ctor, so no blank ever carries it (from the `InitOrderTupleSpecs` golden, `var single, _ = makeGreeting()`):

```csharp
internal static @string single;
internal static error _ᴛ1ʗ;
internal static void initᴛsingle() { single = makeGreeting().Item1; }
```

**Two or more non-blank names** (darwin `os`'s `var initCwd, initCwdErr = Getwd()` shape; golden: `var cwd, cwdErr = fakeGetwd()`): the method evaluates the call **once into a method-local** and assigns each non-blank component from it — the inline path's hidden static tuple holder is unnecessary, because the method body itself sequences the call before its reads. The local reuses the holder's minted `tupleᴛNʗ` name shape so it cannot collide with anything the rendered call expression references:

```csharp
internal static @string cwd;
internal static error cwdErr;
internal static void initᴛcwd() { var tupleᴛ1ʗ = fakeGetwd(); cwd = tupleᴛ1ʗ.Item1; cwdErr = tupleᴛ1ʗ.Item2; }
```

An **all-blank** spec (`var _, _ = f()`) never relocates — blanks are excluded from the moved set, so it keeps the inline emission where the first blank's field initializer carries the call for its side effect. An **addressed** name relocates as the default-valued heap box with the method assignment writing through the ref property, exactly like the plain path. A blank in the **middle** of a spec simply drops out of the assignment list (the golden's `var head, _, tail = makeTrio()` assigns `.Item1` and `.Item3`). (Guarded by the `InitOrderTupleSpecs` behavioral test — both sub-shapes, the mid-spec blank, a plain var chained onto a moved tuple var, an addressed moved tuple var, and an order-safe inline control, output-compared vs Go — and by the converter unit test `TestPackageTupleVarSpecInitOrderRelocation`, which additionally pins that no refusal warning fires and that an order-safe spec keeps the inline holder emission.)

## Test-variant (`-tests`) initializers relocate too — through the erasable static-ctor hook

The same pass runs in the `-tests` conversion driver (the three-drivers rule): a `_test.go` package-level var whose initializer reads a var declared later in the file, in another file, or in the production package cross-file has exactly the same hazard — internal/fmtsort's `compareTests` reads `chans`/`ints` declared 170 lines below it, so every test died in the class initializer on the default slice; encoding/gob's `basicTypes` table reads production `type.go` vars cross-file. The relocated **emission** is unchanged (bare field + `initᴛ<name>` method beside the declaration); only the **ordered-constructor site** differs by variant, because a C# class gets ONE static constructor across all partial parts:

* **External variant** (`<pkg>_test` — its own `<pkg>_test_package` class): a generated `package_init_external_test.cs` supplies the class's static constructor directly, exactly like `package_init.cs`.
* **Internal variant** (test files join the production `<pkg>_package` class): when the production `package_init.cs` exists it already owns the single static-ctor slot, so under `-tests` its constructor ends with a call to an **erasable classic partial method** — `static partial void initᴛᴛtests();` — and the test conversion emits `package_init_internal_test.cs` implementing it with the test-side relocations. Unimplemented (the production compile set excludes `*_test.cs`), C# erases both the declaration and the call, so the production assembly is untouched at runtime. Test relocations therefore run after every production relocation — semantically safe, since a production initializer can never depend on a test var. When the production package relocated nothing, the internal test variant claims the static ctor itself.

Two hard-won rules ride along: the hook name doubles the TempVarMarker (`init` + `ᴛᴛ` + `tests`, `PackageTestInitHookMethod` in `symbols.go`) so no relocated var's `initᴛ<name>` method can collide; and go2cs-gen's **PartialStubGenerator must never stub the hook** — its whole design is C# erasure when unimplemented, and the generator's `NotImplementedException` stub for "asm/cgo" bodyless partials would detonate encoding/gob's static ctor for every consumer of the production assembly (go/token's serialize path), so the generator skips it (guarded by `PartialStubGeneratorTests.StubsAsmPartialsButNeverTheTestInitHook`). The hook emission is `-tests`-gated, so a plain `-stdlib`/behavioral conversion is byte-identical — like the test-artifact exclusions `-tests` writes into the csproj, the production-file difference is intended `-tests` output, not drift. The relocated test files themselves run in the validated test suites (fmtsort's cctor is the operational guard).<!-- "IP-4" named the csproj test-artifact exclusion increment in the fleet record. --> The hoisted-literal interplay keeps its conservative setting (`initOrderRelocated=false` for the real test emission run): suppressing test-file hoists in initializer-reachable functions is a pure allocation pessimization, never a correctness risk, and flipping it would change every converted `*_test.cs`.

## A CONSTANT emitted as an initialized FIELD is an initialization dependency too

Go constants are compile-time values with no initialization order at all, so `types.Info.InitOrder` never mentions them — and for almost every constant that stays true in C#, because a constant C# cannot declare `const` is emitted as a **get-only property** rather than a field (see [Constant Values](constants.md#a-constant-c-cannot-declare-const-is-a-get-only-property-not-a-static-readonly-field)). A property re-evaluates at each read, so it can never be observed at its zero value, whatever the declaration order.

**Two forms cannot be properties**, because re-evaluating them would rebuild an allocation on every read: a **string** constant (`@string`, or a `/*@string*/` wrapper such as `const labelPipe label = "pipe"`, whose `u8` literal the string-literal arc deliberately hoists to a single allocation) and a **GoBigConst** constant (a `BigInteger.Parse`). Those two stay `static readonly` **fields with initializers**, and therefore carry exactly the cross-part field-initializer ordering hazard a package *var* has. The relocation analysis has to supply that dependency edge itself, because Go's own analysis has no reason to model it:

```go
// server.go — a plain string const, so a `static readonly @string` FIELD
const TimeFormat = "Mon, 02 Jan 2006 15:04:05 GMT"
```
```go
// header.go — sorts BEFORE server.go in the compile set
var timeFormats = []string{TimeFormat, time.RFC850, time.ANSIC}
```

Read before its initializer runs, `TimeFormat` is the empty `@string`, and `net/http`'s date parsing silently loses its primary format. `collectRefs` records package-scope `*types.Const` references whose emission is an initialized field (`constEmittedAsInitializedField`, keyed off the constant's **value kind** — string always; int, float and complex only where the magnitude escapes to `GoBigConst`), resolved transitively through function bodies on the same memoized graph the var closure uses. A const dependency forces the move under the same two rules a var dependency does: declared in a different file, or later in the same file. A const is of course never itself *relocatable*, so only its declaration site matters.

The emission side needed the matching half. Relocation lived only in the **non-constant** initializer arm (`tv.Value == nil`), yet a Go-**constant**-valued initializer is just as order-sensitive in C#: Go folds the value at compile time, but the conversion deliberately keeps the *source expression* for readability, and that expression still reads the field:

```go
// registry.go
type label string
const labelPipe label = "pipe"
```
```go
// entries.go — compiled first; the initializer folds to the constant "pipe!" in Go
var pipeLabel = string(labelPipe) + "!"
```
```csharp
// entries.cs — relocated, because the rendered expression READS the labelPipe field
internal static @string pipeLabel;
internal static void initᴛpipeLabel() { pipeLabel = ((@string)labelPipe) + "!"u8; }
```

**Where the edge came from, and what it kept.** The defect that exposed it was `regexp/syntax`'s `opNames` — a 19-entry table index-keyed by the `Op` constants declared in a later file, every key read as zero, the whole table collapsing to one slot so `Parse("a").Dump()` printed the numeric fallback `op3{a}` where Go prints `lit{a}`. `Op` is a named `uint8`, so the property rule removes *that* hazard at the source, and an over-broad predicate naming every non-`const` form would draw an edge for a shape that no longer has one. Scoped to the residue, the edge relocates `internal/buildcfg`'s initializers that read a string const in `zbootstrap.go` (`GOARCH`, `GOOS` and `GO386`, each `envOr("…", default…)`, and `GO_LDSO` and `Version`, each a plain read) and `net/http`'s `timeFormats`.<!-- At 3e3f18925a: src/core/internal/buildcfg/cfg.cs:30 (`internal static void initᴛGOARCH() { GOARCH = envOr("GOARCH"u8, defaultGOARCH); }`), :66 (`internal static void initᴛGO_LDSO() { GO_LDSO = defaultGO_LDSO; }`), :72 (`internal static void initᴛVersion() { Version = version; }`); src/core/net/http/header.cs:125 (`internal static void initᴛtimeFormats() { timeFormats = new @string[]{`). --> Both were latent zero-value const reads that compiled cleanly.

Guarded by the `PackageVarInitOrder` behavioral test, which carries both halves: neutering the predicate collapses `pipeLabel` onto an empty `@string` (stdout mismatch), and neutering the property rule allocates a fixed-array field at length 0 and panics on the first indexed write (exit code 2). Neither is redundant.

<a id="a-listed-var-is-built-on-its-first-read-never-in-the-static-constructor"></a>
## A pure-data var is built on its first read, never in the static constructor

Go's linker writes a table of constants and function values into the binary as static data, so a Go program pays
nothing for it at startup. As a C# field initializer, or as an `initᴛ` method that `package_init.cs` calls, the same
table would be built, and the code that builds it compiled, the first time any member of its package is used. So a
var whose value is pure data is built on its first read instead.

**The shape.** The converter writes the var's declaration exactly as it would write an eager one, on one line, inside
a holder class named `lazyᴛ<name>` and marked `[GoLazy]`, with the var's Go comment after it. .NET runs the holder's
static field initializer the first time the holder is used, which is the var's first read, and runs it once:

<!-- source: GOROOT/src/runtime/malloc_tables_generated.go:8-522 -->
```go
var mallocScanTable = [513]func(size uintptr, typ *_type, needzero bool) unsafe.Pointer{
	mallocPanic,
	mallocgcSmallScanNoHeaderSC1,
…
}
```
<!-- source: src/core/runtime/malloc_tables_generated.cs:9-523 -->
```csharp
[GoLazy] partial class lazyᴛmallocScanTable { internal static array<Func<uintptr, ж<_type>, bool, @unsafe.Pointer>> mallocScanTable = new Func<uintptr, ж<_type>, bool, @unsafe.Pointer>[]{
    mallocPanic,
    mallocgcSmallScanNoHeaderSC1,
…
}.array(); }
```

go2cs-gen's `LazyHolderGenerator` completes each holder. It gives the package class a `ref` accessor under the var's
Go name, with the field's accessibility, so every use site reads and writes the var as it would the field:

```csharp
internal static ref array<Func<uintptr, ж<_type>, bool, @unsafe.Pointer>> mallocScanTable => ref lazyᴛmallocScanTable.mallocScanTable;
```

A holder must hold exactly one static field, named for the holder, or for a var whose address is taken its box,
`Ꮡ<name>` (below). The generator refuses any other holder by name (GO2CS0009) and generates nothing for it. `collectMovedInitVars` skips a lazy var, so it gets no `initᴛ` method and
the package's static constructor never builds it.
<!-- The holder line: src/go2cs/lazyPackageVarOperations.go:576 (`func (v *Visitor) writeLazyPackageVar(access, csTypeName, csIDName, valExpr string, snapshot bool, writeTrailing func()) {`),
     its name :606 (`return "lazy" + TempVarMarker + strings.TrimPrefix(csIDName, "@")`), and for a var whose address is taken :593 (`func (v *Visitor) writeLazyAddressedPackageVar(`). The accessor is generated, so it is shown uncited:
     src/gen/go2cs-gen/LazyHolderGenerator.cs:134 (`source.Append($"{memberIndent}{Accessibility(field)} static ref {type} {fieldName} => ref {holderName}.{fieldName};\r\n");`),
     pinned by src/tests/GenTests/LazyHolderGeneratorTests.cs:165 (`"internal static ref Box _Lu => ref lazyᴛ_Lu._Lu;",`);
     a readonly box's by-value accessor src/gen/go2cs-gen/LazyHolderGenerator.cs:132 (`source.Append($"{memberIndent}{Accessibility(field)} static {type} {fieldName} => {holderName}.{fieldName};\r\n");`).
     The marker: src/core/golib/GoLazyAttribute.cs. Kept out of the ordered ctor:
     src/go2cs/initOrderOperations.go:286 (`if len(initializer.Lhs) == 1 && isLazyPackageVar(initializer.Lhs[0]) {`). -->

**Which initializers are eligible.** Building the value later must not change it, and building it must do nothing
anyone could see. `lazyInitializerRefusal` accepts:

* constants, types, package functions and func literals (a func value's body does not run when it is made);
* composite literals and `&T{…}` allocations of eligible parts (a struct literal's field key names a field and is
  not a read);
* conversions and operators over eligible operands (a conversion applies a type, so it is not a call);
* reads of same-package vars that are themselves eligible and never assigned or addressed, and slices of such a
  var with constant bounds (slicing aliases, so the slice is the same whenever it is taken).
  <!-- src/go2cs/lazyPackageVarOperations.go:189 (`if mutated[dep] || packageAddressedGlobals[dep] {`) -->

Everything else keeps the var eager: a call, a read of an imported var (its package may change it first) or of a
field, an index or a dereference (each copies a value as it is at that moment), a method value (it binds its
receiver when it runs), `&` of storage, a channel receive and a type assertion.

**Which vars stay eager whatever their initializer.** `collectLazyPackageVars` also keeps eager:

* a var declared with other names from one value;
* a var whose address only a pointer-receiver method value takes (`f := X.M`): the converter keeps no heap box
  for it, so the holder would carry no box for the pointer to name;
  <!-- src/go2cs/lazyPackageVarOperations.go:164 (`candidate.reason = "its address is taken where the converter keeps no box for it"`) -->
* a var whose declaration is hand-owned, and every var that reads it: the converter does not shape a declaration it
  does not emit;
* a var that reads a var the package or its internal tests assign or take the address of;
* a var that copies another whose storage a slice aliases, since a write through the slice would change the copy;
* a var declared in a `_test.go` file.
  <!-- src/go2cs/lazyPackageVarOperations.go:121 (`if strings.HasSuffix(fset.Position(obj.Pos()).Filename, "_test.go") {`) -->

**Which eligible vars are built on their first read.** An eligible var gets a holder when its initializer has at
least 20 syntax nodes (a bool or a short string costs less built eagerly than a class of its own), when it reads a
var that has one (an eager reader would build that var at startup anyway), or when it is exported and an initializer
built on a first read reads it. The decision is a fixpoint, since a var can gain a holder through a dep or a reader
that only just gained one. `unicode`'s `Upper = _Lu` is one short line, and it has a holder because `_Lu` has one.

**A var whose address is taken carries its box.** go2cs backs such a var (`&X`, or a pointer-receiver or capture-mode
call on it) with a heap box, `ᏑX`, and a `ref` property `X` over it. Built on its first read, the var keeps both: the
holder holds the box field exactly as the eager declaration writes it, `[GoLazy] partial class lazyᴛX { internal
static readonly ж<T> ᏑX = new StaticBox<T>(…); }`, the `ref` property `internal static ref T X => ref ((StaticBox<T>)ᏑX).ValueSlot;` follows it
unchanged, and the generator adds the box accessor `ж<T> ᏑX => lazyᴛX.ᏑX`. The box field is readonly, which a
writable `ref` cannot return, so the accessor returns the box itself, a reference. A use site does not change: `&X`
reads `ᏑX` and `X` reads through it, the first of either builds the box, and the address is the same before and after
the first read. Such a var is never read by another initializer built on a first read, so its holder never needs an
initial value (the generator refuses `[GoLazy(Initial = true)]` on a box holder). `io`'s `blackHolePool` and the
`sync.Pool`s of `encoding/gob`, `go/printer`, `log/slog` and `net/http`, `nistec`'s precomputed table and the `idna`
and `norm` sparse tries are built this way.

**An exported var keeps its initial value for the vars built from it.** Go builds every initializer when the program
starts, so an importer that later assigns `unicode.Lu` does not change `unicode.Categories`. The converted
`Categories` is built on its first read, which may come after such an assignment. So `Lu`'s holder is marked
`[GoLazy(Initial = true)]`, and the generator adds `internal static readonly T initial;` to it, set in the holder's
static constructor, which C# runs after the field initializer. Every initializer built on a first read reads that
field instead of the var:

<!-- source: src/core/unicode/tables.cs:10-23 -->
```csharp
[GoLazy] partial class lazyᴛCategories { public static map<@string, ж<RangeTable>> Categories = new map<@string, ж<RangeTable>>{
    ["C"u8] = lazyᴛC.initial,
…
    ["Lu"u8] = lazyᴛLu.initial,
```

An exported var such an initializer reads always has a holder of its own, so the snapshot has somewhere to live; a
reader of an exported var with a constant initializer, which has no holder, stays eager.
<!-- the snapshot: src/gen/go2cs-gen/LazyHolderGenerator.cs:137 (`source.Append($"{memberIndent}partial class {holderName} `, which adds `internal static readonly T initial` and sets it in the holder's static constructor);
     the marker argument: src/go2cs/lazyPackageVarOperations.go:580 (`marker = "GoLazy(Initial = true)"`). The importer's
     assignment is the LazyPackageVars behavioral test's case: tables.Heavy, assigned before tables.ByName is first read. -->

**runtime's malloc tables are always built on their first read.** The converter lists runtime's two malloc dispatch
tables, `mallocScanTable` and `mallocNoScanTable`, as vars that get a holder whatever their size. Their only reader is
`mallocgc`'s size-specialized branch, behind the constant `sizeSpecializedMallocEnabled`, which is false because the
`SizeSpecializedMalloc` experiment is off, so no path reads them at all. A listed var that breaks a rule is refused
by name: the converter stops on that file with `lazyPackageVars entry <name> cannot be built on its first read:` and
the reason. An initializer whose conversion would spill a hoisted holder beside the var is refused the same way.

**A package with nothing left to order has no `package_init.cs`.** When every var a package would move to the ordered
static constructor has a holder instead, the conversion writes no `package_init.cs` for it and removes a generated one
an earlier conversion wrote. Under `-platforms`, the merge removes a package's generated `package_init.cs` that no
target emitted. A hand-written `package_init.cs` is never removed.

Guarded by the converter tests `TestLazyPackageVarIsBuiltOnFirstRead`, `TestLazyPackageVarPredicateChoosesAndShapes`,
`TestLazyPackageVarLeavesHandOwnedEager`, `TestLazyPackageVarBuildsAPointerAddressedPoolInItsBox`,
`TestLazyPackageVarCarriesItsBox`, `TestLazyPackageVarLeavesManualVarEager` and `TestLazyPackageVarRefusesByName`,
all in `lazyPackageVar_test.go`, and
`TestMergeRetiresAPackageInitNoTargetEmitted`; by GenTests' `LazyHolderGeneratorTests`; by GolibTests'
`MallocDispatchTablesTests`, which checks that the runtime's static constructor does not build either table and that
a first read finds each one populated, entry for entry, as Go's `mkmalloc` writes it; and by the `LazyPackageVars`
behavioral test, whose output matches Go's: init order, an importer's assignment before a first read, one pointer
per allocation, func literal values, and an address taken before a var's first read that is its address after it.
<!-- The predicate: src/go2cs/lazyPackageVarOperations.go:97 (`func collectLazyPackageVars(fset *token.FileSet, pkg *types.Package, info *types.Info, syntax []*ast.File) {`),
     :297 (`func lazyInitializerRefusal(info *types.Info, scope *types.Scope, expr ast.Expr, deps *[]*types.Var) string {`),
     the floor :48 (`const lazyPackageVarNodeFloor = 20`), the eager cases :154 (`candidate.reason = "its declaration is hand-owned"`),
     :164 (`candidate.reason = "its address is taken where the converter keeps no box for it"`),
     :195 (`candidate.reason = fmt.Sprintf("it copies the var %s, whose storage a slice aliases", dep.Name())`);
     the list :38 (`var lazyPackageVars = map[string]map[string]bool{`). The refusals: src/go2cs/visitValueSpec.go:727
     (`cannot be built on its first read: %s`), :740 (`its initializer spills a hoisted holder`). The reader's gate, at
     go1.26.10: GOROOT/src/runtime/malloc.go:1039 (`const sizeSpecializedMallocEnabled = goexperiment.SizeSpecializedMalloc`).
     package_init.cs: src/go2cs/initOrderOperations.go:504 (`func retireStalePackageInitFile(outputDir string) error {`),
     src/go2cs/platformEmit.go:1261 (`func retireUnemittedPackageInitFiles(coreDir string, targets []string, emissions []*platformEmission, emitted map[string]bool) (int, error) {`).
     Guards: src/go2cs/lazyPackageVar_test.go:158 (`func TestLazyPackageVarIsBuiltOnFirstRead(t *testing.T) {`), :502
     (`func TestLazyPackageVarRefusesByName(t *testing.T) {`), :412 (`func TestLazyPackageVarCarriesItsBox(t *testing.T) {`);
     src/tests/GolibTests/MallocDispatchTablesTests.cs:82 (`public void TheRuntimeStaticConstructorNoLongerBuildsThem()`).
     Why the two malloc tables are listed (COORD's ruling (A), 2026-10-22, per the list's own comment): built eagerly they
     were 2 x 17.1K IL JIT-compiled at every startup; no platform is recorded for that figure, so it stays here. The node
     floor's derivation (97.5% of the eligible initializer mass in std at a third of the vars) is the design note's census,
     per the constant's own comment. -->

---

[← Package Conversion](package-conversion.md) · [Index](README.md) · [Compiled Library versus Source Code →](compiled-library-vs-source.md)
