# Package-Level Variable Initialization Order

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#package-level-variable-initialization-order)

Go initializes package-level variables in **dependency order** (spec: "within a package, package-level variable initialization proceeds stepwise, each step selecting the earliest variable … that has no dependencies on uninitialized variables"), where dependencies are resolved **through function calls and function literals** referenced by the initializer. The default conversion emits a package var as a C# static field with an initializer — but C# executes static field initializers in textual order **within** one file of the partial package class and in an **undefined** order **across** files. Three dependency shapes therefore break at runtime while compiling cleanly (the Phase-3 → Phase-4 distinction in miniature):

1. **Cross-file:** syscall's `var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")` (syscall_windows.go) reads `modkernel32` declared in zsyscall_windows.go — with the wrong file order the receiver box is nil (NullReference in the type initializer, the first Phase-4 crash of any program importing `os`).
2. **Cross-file through a function:** syscall's `var Stdin = getStdHandle(STD_INPUT_HANDLE)` — the initializer calls a package *function* whose body reads zsyscall_windows.go's `procGetStdHandle`. Same failure, invisible to a direct-reference scan; dependencies must be resolved transitively through same-package function bodies (and func-literal bodies — an IIFE initializer executes at init time), exactly as Go's own analysis does.
3. **Same-file forward reference:** `var first = base + 1` declared above `var base = 41` — C# reads `base`'s zero value (silently wrong value, no crash).

**The conversion:** `collectMovedInitVars` (converter: `initOrderOperations.go`) walks `types.Info.InitOrder` — Go's authoritative dependency-sorted initializer list — resolving each initializer's same-package var dependencies transitively through package function/method bodies. An initializer moves when a dependency is cross-file, a same-file forward reference, or **itself moved** (a moved var is only assigned in the ctor, so its dependents can no longer stay field initializers regardless of layout). A moved var is emitted as a **bare field** plus a tiny **init method beside it in its home file** (so the rendered expression keeps that file's using aliases), and a generated `package_init.cs` supplies the package class's **static constructor**, calling the methods in InitOrder:

```go
// syscall_windows.go
var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")
```

```csharp
// syscall_windows.cs
internal static ж<LazyProc> procSetFilePointerEx;
internal static void initᴛprocSetFilePointerEx() { procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8); }

// package_init.cs (generated)
partial class syscall_package {
    static syscall_package() {
        initᴛprocSetFilePointerEx();
        // … every relocated initializer, in types.Info.InitOrder …
    }
}
```

This is correct by C#'s own initialization guarantees: **all** static field initializers (every partial-class file) run **before** the static-constructor body, so every non-relocated dependency is already initialized when the ctor runs; the ctor then applies the relocated initializers in Go's order. Vars with no order hazard (the overwhelming majority — only 25 of the 302 stdlib packages relocate anything) keep their readable inline form. Cross-**package** order needs no handling: accessing another package's static field triggers that type's initialization first (.NET guarantees), matching Go's imported-packages-first rule. Adding an explicit static ctor also removes `beforefieldinit` from the package class, giving it *precise* initialization semantics.

Notes: a **blank** (`_`) initializer never relocates (its value is unreadable, so its order is immaterial — it still runs as a field initializer for its side effect); the `initᴛ` method name composes the TempVarMarker so it cannot collide with any Go identifier; an **addressed** global relocates as a default-valued heap box whose ctor assignment writes through the ref property into the same box; a **tuple-deconstructing** spec relocates as one unit (see the next subsection). The one remaining warn-and-stay-inline fallback is a moved PLAIN var whose initializer carries a multi-value hoisted inner call (`globalDeclHoist` — the `template.Must(template.New(…).Parse(…))` spread shape); the whole-corpus census found zero flagged occurrences of it. Guarded by the `PackageVarInitOrder` behavioral test (all three hazard shapes plus IIFE and moved-dependency closure, output-compared vs Go).

## A TUPLE-deconstructing package var relocates as ONE unit

A package-level `var a, b = f()` — one multi-value call deconstructed across the names — is a single initialization step in Go: `types.Info.InitOrder` carries **one entry for the whole spec** with every name in its `Lhs`, so `collectMovedInitVars` flags all of a spec's non-blank names together under one shared ordinal, and the emission registers **one `initᴛ` method per spec** at that ordinal — `writeOrderedInitCalls` needs no new bookkeeping. Until 2026-08-11 this path *refused* to relocate ("unsupported for tuple specs", warn and leave inline), on a "no stdlib occurrence" premise the census falsified: `crypto/internal/edwards25519`'s `var identity, _ = new(Point).SetBytes(…)` and `var generator, _ = …` reach `feOne` and `d` — declared *later* in the same file — through the package's own `(*Point).SetBytes`, so the inline field initializers ran first, `identity` read a null `feOne`, `field.Subtract` null-dereferenced, and the package cctor threw before any test ran (the corpus's only whole-package casualty: 0 of 55 verdicts, restored to 52 of 55 by the relocation — the three residuals are separate pre-existing roots). Census: exactly two production occurrences on Windows (both edwards25519) and two latent on darwin (`os`'s `executable_darwin.go`); full detail in [`docs/phase4/FINDING-init-order-tuple-specs.md`](../phase4/FINDING-init-order-tuple-specs.md).

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

An **all-blank** spec (`var _, _ = f()`) never relocates — blanks are excluded from the moved set, so it keeps the inline emission where the first blank's field initializer carries the call for its side effect. An **addressed** name relocates as the default-valued heap box with the method assignment writing through the ref property, exactly like the plain path. A blank in the **middle** of a spec simply drops out of the assignment list (the golden's `var head, _, tail = makeTrio()` assigns `.Item1` and `.Item3`). (Guarded by the `InitOrderTupleSpecs` behavioral test — both sub-shapes, the mid-spec blank, a plain var chained onto a moved tuple var, an addressed moved tuple var, and an order-safe inline control, output-compared vs Go — and by the converter unit test `TestPackageTupleVarSpecInitOrderRelocation`, which additionally pins that the refusal warning no longer fires and that an order-safe spec keeps the inline holder emission.)

## Test-variant (`-tests`) initializers relocate too — through the erasable static-ctor hook

The same pass runs in the `-tests` conversion driver (the three-drivers rule): a `_test.go` package-level var whose initializer reads a var declared later in the file, in another file, or in the production package cross-file has exactly the same hazard — internal/fmtsort's `compareTests` reads `chans`/`ints` declared 170 lines below it, so every test died in the class initializer on the default slice; encoding/gob's `basicTypes` table reads production `type.go` vars cross-file. The relocated **emission** is unchanged (bare field + `initᴛ<name>` method beside the declaration); only the **ordered-constructor site** differs by variant, because a C# class gets ONE static constructor across all partial parts:

* **External variant** (`<pkg>_test` — its own `<pkg>_test_package` class): a generated `package_init_external_test.cs` supplies the class's static constructor directly, exactly like `package_init.cs`.
* **Internal variant** (test files join the production `<pkg>_package` class): when the production `package_init.cs` exists it already owns the single static-ctor slot, so under `-tests` its constructor ends with a call to an **erasable classic partial method** — `static partial void initᴛᴛtests();` — and the test conversion emits `package_init_internal_test.cs` implementing it with the test-side relocations. Unimplemented (the production compile set excludes `*_test.cs`), C# erases both the declaration and the call, so the production assembly is untouched at runtime. Test relocations therefore run after every production relocation — semantically safe, since a production initializer can never depend on a test var. When the production package relocated nothing, the internal test variant claims the static ctor itself.

Two hard-won rules ride along: the hook name doubles the TempVarMarker (`init` + `ᴛᴛ` + `tests`, `Symbols.PackageTestInitHookMethod`) so no relocated var's `initᴛ<name>` method can collide; and go2cs-gen's **PartialStubGenerator must never stub the hook** — its whole design is C# erasure when unimplemented, and the generator's `NotImplementedException` stub for "asm/cgo" bodyless partials detonated encoding/gob's static ctor for every consumer of the production assembly (go/token's serialize path) until the generator learned to skip it (guarded by `PartialStubGeneratorTests.StubsAsmPartialsButNeverTheTestInitHook`). The hook emission is `-tests`-gated, so a plain `-stdlib`/behavioral conversion is byte-identical — like the IP-4 csproj test-artifact exclusions, the production-file difference is intended `-tests` output, not drift. The relocated test files themselves ride the banked suites (fmtsort's cctor is the operational guard). The hoisted-literal interplay keeps its conservative setting (`initOrderRelocated=false` for the real test emission run): suppressing test-file hoists in initializer-reachable functions is a pure allocation pessimization, never a correctness risk, and flipping it would drift every banked `*_test.cs`.

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

**Where the edge came from, and what it kept.** The defect that exposed it was `regexp/syntax`'s `opNames` — a 19-entry table index-keyed by the `Op` constants declared in a later file, every key read as zero, the whole table collapsing to one slot so `Parse("a").Dump()` printed the numeric fallback `op3{a}` where Go prints `lit{a}`. `Op` is a named `uint8`, so the property rule now removes *that* hazard at the source, and an over-broad predicate naming every non-`const` form would draw an edge for a shape that no longer has one. Scoped to the residue, the edge's corpus footprint is **two packages**: `internal/buildcfg` relocates five initializers (`GOARCH`, `GOOS`, `GO386`, `GO_LDSO`, `Version` — each `envOr("…", default…)` over a string const in `zbootstrap.go`) and `net/http` relocates `timeFormats`. Both were latent zero-value const reads that compiled cleanly.

Guarded by the `PackageVarInitOrder` behavioral test, which carries both halves: neutering the predicate collapses `pipeLabel` onto an empty `@string` (stdout mismatch), and neutering the property rule allocates a fixed-array field at length 0 and panics on the first indexed write (exit code 2). Neither is redundant.

---

[← Package Conversion](package-conversion.md) · [Index](README.md) · [Compiled Library versus Source Code →](compiled-library-vs-source.md)
