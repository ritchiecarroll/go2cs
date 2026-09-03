# Conversion Strategies

> **A high-level, example-driven tour of how `go2cs` maps each Go construct to C#.** This is the
> readable overview -- every section ends with a **Reference →** link into the exhaustive
> [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md), where the same topic
> is documented in full: every emitted form, edge case, phase-level fix, and the [behavioral test](Glossary.md#guard)
> that guards it. Read the summary for the *shape*; open the reference for the *why*.

The guiding goal is that the generated C# is both **behaviorally** and **visually** similar to the
original Go, so a Go developer can read the output and follow it. Two things the visible code does not
show in full make that possible: a hand-written runtime library, **`golib`** (`src/core/golib/`,
supplying `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `ж<T>`, `nil`, the builtins, …), and a set
of **[Roslyn](Glossary.md#roslyn) source generators** (`src/gen/go2cs-gen/`) that synthesize the Go semantics C# cannot spell
directly (interface satisfaction, receiver overloads, struct-embedding promotion, named-type operators).

> The C# snippets below are drawn from the actual converted standard library (`src/core/`,
> Go 1.23.12) wherever possible, paired with their original Go source. A few use small illustrative
> programs where that reads more clearly. Glyphs you will see throughout: **`ж<T>`** a heap "box"
> (pointer, read "zhe"), **`Ꮡ`** address-of, **`Δ`** a disambiguation rename (read "delta"),
> **`@string`** the Go string type, **`default!`** = `nil` in value position, and a handful of
> operator glyphs (`ᐸꟷ`/`ꟷᐳ` channel receive, `goǃ` goroutine, `ꟷ`/`ᐧ` comma-ok/type sentinels).

---

## Contents

- **Packages & project structure:** [Package Conversion](#package-conversion) ·
  [Variable Init Order](#package-level-variable-initialization-order) ·
  [Library versus Source](#compiled-library-versus-source-code)
- **Numbers, constants & nil:** [Constants](#constant-values) ·
  [Native/Narrow Integers](#native-and-narrow-integer-types) ·
  [Named Numeric Types](#named-numeric-types-and-constant-contexts) ·
  [Nil and Zero Values](#nil-and-zero-values) · [`any`](#empty-interface-any)
- **Assignment & scope:** [Multi-Assignment](#multi-assignment-and-evaluation-order) ·
  [Shadowing](#short-variable-redeclaration-shadowing) ·
  [Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)
- **Composite & named types:** [Slices and Arrays](#slices-and-arrays) ·
  [Strings](#strings-string-and-sstring) · [Maps and Channels](#maps-and-channels) ·
  [Generic Constraints](#generic-constraints) · [Type Aliasing](#type-aliasing)
- **Functions & control flow:** [Value-Receiver Delegates](#delegates-to-value-receiver-instances) ·
  [Defer/Panic/Recover](#defer--panic--recover) · [Expression Switch](#expression-switch-statements) ·
  [Type Switch](#type-switch-statements) · [Labels & Loop Variables](#labeled-control-flow-and-loop-variables)
- **Types & polymorphism:** [Struct Types](#struct-types) · [Struct Embedding](#struct-type-embedding) ·
  [Interfaces](#interfaces)
- **Pointers & memory:** [Pointers](#pointers) · [Implicit Dereferencing](#implicit-pointer-dereferencing)
- **The machinery:** [`go.golib`](#the-gogolib-support-namespace) · [Source Generators](#source-generators) ·
  [Manually-Converted Declarations](#manually-converted-declarations) · [Deterministic Output](#deterministic-output)

---

## At a glance

| Go | C# rendering | Machinery |
|---|---|---|
| `package foo` · top-level `func Bar()` | `partial class foo_package` · `static` methods (receiver methods → extension methods) | converter |
| `import "x/y"` | `using y = go.x.y_package;` + a `ProjectReference` | converter |
| `int` / `uint` / `uintptr` | `nint` / `nuint` / [`uintptr`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/uintptr.cs) (a distinct golib struct) | [BCL](Glossary.md#bcl) / golib |
| `int32`, `byte`, `rune`, `float64`, … | same-named C# aliases (`global using rune = System.Int32;`) | global usings |
| untyped constant | [`UntypedInt`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/UntypedInt.cs) / [`UntypedFloat`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/UntypedFloat.cs) / [`UntypedComplex`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/UntypedComplex.cs) wrapper | golib |
| `type Celsius float64` | [`[GoType("num:float64")]`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoTypeAttribute.cs) `partial struct Celsius` | `TypeGenerator` |
| `nil` | `nil` (golib [`NilType`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/NilType.cs)) or `default!` in value position | golib |
| `interface{}` / `any` | `any` (a global alias for `object`) | BCL |
| `[]T` slice · `[N]T` array | [`slice<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/slice.cs) · [`array<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/array.cs) | golib |
| `map[K]V` · `chan T` | [`map<K,V>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/map.cs) · [`channel<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/channel.cs) | golib |
| `string` | [`@string`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/string.cs) (heap) · [`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs) (non-escaping stack view) | golib |
| `v, ok := m[k]` (comma-ok) | [`var (v, ok) = m[k, ꟷ];`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/map.cs) | golib |
| `a, b = b, a` | `(a, b) = (b, a);` | C# tuples |
| `*T` · `&x` | [`ж<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) heap box · [`Ꮡx`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) address-of | golib |
| `type I interface{…}` | [`[GoType]`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoTypeAttribute.cs) `partial interface` + [generated implementing glue](https://github.com/ritchiecarroll/go2cs/blob/master/src/gen/go2cs-gen/ImplementGenerator.cs) | `ImplementGenerator` |
| struct embedding | [promoted field accessors + method forwarders](https://github.com/ritchiecarroll/go2cs/blob/master/src/gen/go2cs-gen/TypeGenerator.cs) | `TypeGenerator` |
| `func (t T) M()` / `func (t *T) M()` | [`[GoRecv]`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoRecvAttribute.cs)/`this` extension method + a [`ж<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) overload | `RecvGenerator` |
| `f := func(…){…}`, only ever called | a C# **local function** `ret f(…) {…}` — captures with no display class and no delegate | converter |
| `defer f()` · `panic(x)` · `recover()` | body INLINE in `try`/`catch`/`finally` beside a [`GoFrame`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoFrame.cs) local; `defer(f, ref ᒐ)`; [`throw panic(x)`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/PanicException.cs) | golib |
| `go f()` · `select {…}` | [`goǃ(…)`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs) · [`switch (select(…))`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs) | golib |
| `x.(T)` · `switch x.(type)` | [`x._<T>()`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs) · [`x.type()`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs) | golib / converter |
| generic `[T Constraint]` | `where T : <lifted interface(s)>` | golib / .NET |

---

## Package Conversion

A Go package becomes a C# **static partial class** named `<pkg>_package`, inside a root `go` namespace;
the import path's leading segments become the namespace. Go's package-level functions are `static`
methods; receiver methods are emitted as **extension methods** (decorated `[GoRecv]`). Using a *partial*
class lets the functions from a package's many files coalesce under one import. A program with `main`
converts to an executable project; imported packages convert to referenced library projects.

```go
import "unicode/utf8"
```
```csharp
using utf8 = go.unicode.utf8_package;    // one alias; per-file `package_info.cs` carries the global usings
```

Go initializes an imported package **before** the importer, whatever the import looks like. A converted
`init` is a .NET `[ModuleInitializer]`, which fires only at first use of something in *its own*
assembly — so an assembly the program has not touched yet has not initialized. The converter closes
that gap with a hook, once per assembly, ahead of the file's own `init`s. It is emitted for any import
whose package initializes something transitively; forcing an empty module constructor would be a
guaranteed no-op, so those are skipped.

```go
import (
    _ "image/png"    // registers the PNG decoder with image.Decode
    "log/slog"       // its init captures a value log's init installs
)
```
```csharp
// blank import: go.image.png_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using slog = go.log.slog_package;

[GoInit] internal static void initᴛᴛimportꓸimageꓸpng() { builtin.initPackage(typeof(go.image.png_package)); }
[GoInit] internal static void initᴛᴛimportꓸlogꓸslog() { builtin.initPackage(typeof(go.log.slog_package)); }
```

A blank import still emits no `using` (a `using _` alias would hijack C#'s discard) — it is a comment
plus its hook. NAMED imports went unforced until 2026-08-26, which is why `log/slog` — whose `init`
captures `log/internal.DefaultOutput`, a value **`log`'s** `init` installs — captured nil whenever a
program touched `slog` first, and then dereferenced it.

A converted **test** project gets one more hook for the same reason. Go runs every `init` in the
package under test — the production files' included — before the first test, but a `-tests` project
*references* the production assembly rather than recompiling it, so that `init` would otherwise wait
for the first touch of a production symbol. `package_test_info.cs` therefore forces the production
module directly; see
[the reference](ConversionStrategies-Reference.md#a--tests-production-reference-project-forces-the-package-under-tests-own-init).

A package whose emitted C# differs by platform keeps the differing files in per-`GOOS` subfolders, and its
`.csproj` compiles exactly one of them — `<Compile Include="$(GoTargetOS)/*.cs" />`, defaulting to
`windows`. Files identical on every platform stay flat, so this touches only the packages that genuinely
vary: **37** of 307. A package whose *imports* also differ by platform — **21** of them, `os` reaching
`internal/syscall/windows` on Windows and `internal/syscall/unix` elsewhere — states its common references
once and selects the rest the same way:

```xml
<ItemGroup Condition="'$(GoTargetOS)'=='windows'">
  <ProjectReference Include="$(go2csPath)core/internal/syscall/windows/internal.syscall.windows.csproj" />
</ItemGroup>
```

A **hand-owned** file needs the same treatment and cannot get it from the classifier, which compares
emissions and never sees a file nothing emits. Such a file belongs in exactly the platform builds its
*principal* takes part in — `<name>.cs` for an `<name>_impl.cs` companion, the `<name>.cs.auto` review
sibling for a `[module: GoManualConversion]` whole-file replacement — after which the ordinary rule applies:
shared by every platform means flat, a subset means one copy per folder. So
`runtime/lock_sema_impl.cs` lives in `runtime/windows/` **and** `runtime/darwin/` (Linux takes
`lock_futex.go`), while `os/proc_impl.cs` stays flat even though `proc.cs` is per-GOOS, because every
platform has one.

**Full detail:** [Reference → Package Conversion](ConversionStrategies-Reference.md#package-conversion) —
cross-package imports & assembly references, module-aware resolution, exported type aliases crossing
packages (the `ꓸ`-qualified `global using` round-trip), cross-package interface-satisfaction witnesses,
[imported-package initialization order](ConversionStrategies-Reference.md#an-import-forces-the-imported-packages-init-to-run),
build-tag/`GOOS`/`GOARCH` file selection,
[per-`GOOS` source folders](ConversionStrategies-Reference.md#per-goos-sources-layout-l3-and-gotargetos),
and the auto-generated `.slnx` solutions (the stdlib solution, and
the `-recurse` per-project solutions grouped into `src`/`pkg`/`core` folders).

---

## Package-Level Variable Initialization Order

Go initializes package vars in **dependency order** (resolved through function calls); C# static field
initializers run in an **undefined order across** a partial class's files. A var whose initializer
depends — directly, through a package function, or via a func literal — on a var in another file (or
declared later in the same file) is emitted as a bare field plus an init method beside it, and a
generated `package_init.cs` static constructor calls those methods in Go's `InitOrder`. C# runs all
field initializers before any static-ctor body, so the relocated initializers always see their
non-relocated dependencies ready. Everything else keeps the readable inline form.

```go
var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx") // modkernel32: another file
```
```csharp
internal static ж<LazyProc> procSetFilePointerEx;
internal static void initᴛprocSetFilePointerEx() { procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8); }
// package_init.cs: static syscall_package() { …; initᴛprocSetFilePointerEx(); … }
```

A **constant** can be a dependency too, but only the two forms that stay initialized fields rather
than [get-only properties](#constant-values) — a string const and a `GoBigConst` const. Go lists no
initialization order for constants at all, so that edge is one the conversion has to add itself.

**Full detail:** [Reference → Package-Level Variable Initialization Order](ConversionStrategies-Reference.md#package-level-variable-initialization-order) —
the three hazard shapes, transitive dependency analysis, moved-dependency closure, addressed globals,
[tuple-deconstructing specs relocated as one unit](ConversionStrategies-Reference.md#a-tuple-deconstructing-package-var-relocates-as-one-unit),
the [constant-dependency edge](ConversionStrategies-Reference.md#a-constant-emitted-as-an-initialized-field-is-an-initialization-dependency-too),
and the `PackageVarInitOrder` / `InitOrderTupleSpecs` behavioral guards.

---

## Compiled Library versus Source Code

Go compiles all source together (including the stdlib), which lets its compiler do whole-program escape
analysis. The go2cs converter **assumes values can escape to the heap** except in the
simplest-to-detect cases (see [Pointers](#pointers)) — a safe default that can cost an unnecessary heap
box, and the one that holds when converted packages are consumed as compiled libraries: the standard
library is published on NuGet as `go.<pkg>` / `go.lib` / `go.gen`, which fits how C# developers usually
consume dependencies, and a `-recurse=nuget` conversion references those packages directly.

**Full detail:** [Reference → Compiled Library versus Source Code](ConversionStrategies-Reference.md#compiled-library-versus-source-code).

---

## Constant Values

A **typed** Go constant emits with its concrete C# type. An **untyped** constant emits as a golib
`Untyped*` wrapper, so it adapts to whatever numeric type its use site needs — just like an untyped Go
constant taking its type from context. Numeric literal *formatting* is preserved where Go and C#
overlap (hex, binary, `_` separators), so bit masks and addresses stay recognizable.

```go
const MaxRetries = 3          // typed by use
const win = 100               // untyped
const mask = 0x4000           // formatting preserved
```
```csharp
public const nint MaxRetries = 3;
internal static UntypedInt win => 100;
internal static UntypedInt mask => 0x4000;   // not flattened to 16384
```

Whenever C# can say `const` it does. When it cannot — a `[GoType]` struct such as the `Untyped*`
wrappers, a named type, `uintptr`, or a complex is not a legal constant type — the declaration is a
get-only **property**, not a `static readonly` field. A Go constant has no initialization at all,
while C# runs static field *initializers* in class-textual order, so as a field a constant could be
read as its type's DEFAULT by any package-level variable declared ahead of it — silently. That is how
`compress/flate`'s Huffman decode table was allocated at length 0 instead of 512. (Two allocating
forms — `@string` and `GoBigConst` — stay fields on purpose; see the
[reference](ConversionStrategies-Reference.md#a-constant-c-cannot-declare-const-is-a-get-only-property-not-a-static-readonly-field).)

Float constant values emit **exactly**: the Go source literal verbatim when it is valid C#, else the
shortest round-trip form — never a shortened decimal. And a **function-local** untyped constant whose
every use resolves to one concrete type is **tightened** to that type — declared with C#'s `const`
where legal, with the now-redundant per-use casts dropped (one value-changing cast stays: the
sub-int32 shift width retype) — so the emitted code reads like the Go source (math `cbrt`; uses that
stay genuinely untyped, feed other constants, or participate in constant folding conservatively keep
the wrapper form):

```go
const (
    C = 5.42857142857142815906e-01 // 19/35 = 0x3FE15F15F15F15F1
    G = 3.57142857142857150787e-01 // 5/14  = 0x3FD6DB6DB6DB6DB7
)
s := C + r*t
t *= G + F/(s+E+D/s)
```
```csharp
const float64 C = 5.42857142857142815906e-01; // 19/35     = 0x3FE15F15F15F15F1
const float64 G = 3.57142857142857150787e-01; // 5/14      = 0x3FD6DB6DB6DB6DB7
var s = C + r * t;
t *= G + F / (s + E + D / s);
```

A **complex** constant emits a real complex value, built from its two halves by the same exact-float
rendering and recombined in the postfix `.i()` form written imaginary literals use — as a property,
because C# forbids `const` of a struct and both `complex128` (`System.Numerics.Complex`) and `complex64`
are structs:

```go
const cRational = 5.5 + 1.5i
const c64 complex64 = 1.5 + 2.5i
```
```csharp
internal static UntypedComplex cRational => /* 5.5 + 1.5i */ 5.5D + 1.5D.i();
internal static complex64 c64 => /* 1.5 + 2.5i */ 1.5F + 2.5F.i();
```

A native-sized constant whose value doesn't fit a C# `const` (e.g. `^uintptr(0)`) falls back to the
same property form with an `unchecked` cast. Note `uintptr` is a **distinct golib struct**, not an alias of
`System.UIntPtr` — Go treats `uint` and `uintptr` as different types, and the struct preserves that
identity.

**Full detail:** [Reference → Constant Values](ConversionStrategies-Reference.md#constant-values) — the
exact-float, complex-halves, and local-const tightening rules, the wrapper's value-conversion and
value-comparison contracts, the `unchecked` native-int cast rules, wide-unsigned named consts, and the
full `uintptr` conversion matrix.

---

## Native and Narrow Integer Types

Go's `int`/`uint` are platform-sized; C#'s are always 32-bit. C# 9's native integers `nint`/`nuint`
behave exactly like Go's, so `int` → `nint`, `uint` → `nuint` (and `uintptr` → the `uintptr` struct). The
fixed-width types keep readable same-named aliases (`int32`, `byte`, `rune`, …).

The one semantic gap is **narrow arithmetic**: Go evaluates `int8`/`uint8`/`int16`/`uint16` math at that
width with wrap-around, but C# promotes it to `int`. Where a narrow result flows into a narrow-typed slot,
the converter inserts a cast back — which both compiles and restores Go's wrapping:

```go
var a, b uint8 = 200, 100
take(a + b)         // Go: 44 (300 mod 256)
```
```csharp
uint8 a = 200, b = 100;
take((uint8)(a + b));   // wraps to 44, not 300
```

**Full detail:** [Reference → Native and Narrow Integer Types](ConversionStrategies-Reference.md#native-and-narrow-integer-types) —
narrow-arithmetic casts across argument/assignment/return contexts, wide-const overflow folding, signed
minima sign-folding, and the `Index`/`Range` `nint`→`int` caveat.

---

## Named Numeric Types and Constant Contexts

General untyped constant representation is covered in [Constant Values](#constant-values). This section
focuses on what happens after numeric context is known: Go defined types over numeric bases, constants that
must flow into those named or native-width types, and the casts needed to keep C# overload resolution and
operator binding aligned with Go.

A Go type over a numeric base — `type Celsius float64`, `type Duration int64` — becomes a `[GoType("num:…")]`
partial struct whose body (operators, comparisons, conversions to/from the underlying) the `TypeGenerator`
fills in. It's a distinct C# type that still behaves like its base, so method bodies read almost
line-for-line the same:

```go
type Duration int64                       // time/time.go
func (d Duration) Seconds() float64 {
    sec := d / Second
    nsec := d % Second
    return float64(sec) + float64(nsec)/1e9
}
```
```csharp
[GoType("num:int64")] partial struct Duration;      // time/time.cs
public static float64 Seconds(this Duration d) {
    var sec = d / ΔSecond;
    var nsec = d % ΔSecond;
    return (float64)(int64)sec + (float64)(int64)nsec / 1e9D;
}
```

The wrapper carries the full operator surface (integer underlyings also get `~`, shifts, and bitwise ops),
so `Word >> s` stays a `Word`. Converting *between* a named type and a non-underlying basic routes through
the underlying (`traceArg(procs)` → `(traceArg)(uint64)procs`), mirroring Go's numeric-conversion rules.
Unsigned unary minus lowers to `(T)0 - x` (C# forbids unary negation, i.e., `-` prefix, on unsigned).

**Full detail:** [Reference → Named Numeric Types and Constant Contexts](ConversionStrategies-Reference.md#named-numeric-types-and-constant-contexts) —
this is one of the deepest topics: `++/--` operators, to/from conversions, cross-assembly conversion
operators, named slice/array/map wrappers, `append` element casting, shift-width and bit-mask casts, and
the `&^=` bit-clear lowering.

---

## Nil and Zero Values

`nil` maps to the golib `NilType` value `nil` (from `go.builtin`), which defines comparison operators so
`x == nil` / `x != nil` work across slices, maps, channels, pointers, and interfaces — each defining what
"nil" means for it (a nil `map<K,V>` reads zero values, has `len` 0, ranges empty, panics on write). In
*value* position (a `return`, an assignment), `nil` is written **`default!`**:

```go
func Unwrap(err error) error {      // errors/wrap.go
    u, ok := err.(interface{ Unwrap() error })
    if !ok {
        return nil
    }
    return u.Unwrap()
}
```
```csharp
public static error Unwrap(error err) {     // errors/wrap.cs
    var (u, ok) = err._<Unwrap_type>(ᐧ);
    if (!ok) {
        return default!;
    }
    return u.Unwrap();
}
```

A nil→pointer **conversion** — Go's typed nil, `(*T)(nil)` — instead yields the pointer type's
**canonical typed nil instance** (`ж<T>.NilBox`), so the boxed value keeps its Go dynamic type:
`any((*T)(nil)) != nil`, `%T` prints `*T`, and the stdlib's descriptor idiom
`reflect.TypeOf((*T)(nil)).Elem()` resolves — a bare `null` erased all three:

```go
var errorType = reflectlite.TypeOf((*error)(nil)).Elem()   // errors/wrap.go
```
```csharp
internal static reflectliteꓸType errorType = reflectlite.TypeOf(((ж<error>)nil)).Elem();
```

Go draws no distinction between a nil it was handed that way and a pointer's zero value, so neither
does the conversion — a pointer entering interface space carries its static type **however it was
produced**. A non-empty interface gets that from its generated adapter; an `any` slot has no adapter,
so the box passes through `OrTypedNil()`, which substitutes the canonical instance for a plain null:

```go
var ip *int
fmt.Printf("%T %v\n", ip, any(ip) == nil)   // *int false
```
```csharp
ж<nint> ip = default!;
fmt.Printf("%T %v\n"u8, ip.OrTypedNil(), ((any)ip.OrTypedNil()) == default!);
```

Reflection reaches the same boundary from the other side. `reflect.Value.Interface()` is Go's
`packEface` — an interface built from a **type** and a **data word** — so a nil `*T` read out of a
slot packs as a non-nil `(type=*T, value=nil)` and a type assertion on it SUCCEEDS, dispatching the
method on the nil receiver. The bridge re-encodes a null pointer-kinded slot read as that same
canonical instance, which is what lets `encoding/gob` reach `big.Int.GobEncode`'s `if x == nil` arm
for a zero-filled `make([]*Int, 1)` element.

Detail (pointer-identity rules, adapter seeding, the structural-vs-dereference nil distinction, and
which slots the boundary covers):
[Canonical typed-nil pointer boxing](ConversionStrategies-Reference.md#canonical-typed-nil-pointer-boxing)
and [the reflection read path](ConversionStrategies-Reference.md#reflectvalueinterface-is-a-boundary-into-interface-space-so-it-packs-the-typed-nil-too).

Zero-value reference-backed values are null-safe: a `default!` `@string` reads as `""` rather than
throwing.

But `default!` is the Go zero value only for a type whose all-bits-zero form is already usable
storage, and a **fixed-size array is not**: golib's `array<T>` carries its length in the constructed
instance, and C# `default` runs neither a constructor nor a field initializer. So every declaration
with no initializer — a `var x T`, a package global, or a **named result** — climbs one shared
ladder: an unnamed `[N]T` constructs (`new(N)`), a struct with a promoted embed constructs
(`new(nil)`), a struct carrying a fixed array at any depth constructs (`new()`), and everything else
keeps `default!`.

```go
func (ip Addr) As16() (a16 [16]byte)                    // Go: sixteen zeroed bytes
```
```csharp
public static array<byte> /*a16*/ As16(this ΔAddr ip) {
    array<byte> a16 = new(16);                          // not default! — that is length 0
```

Getting this wrong stays silent until it isn't: `a16[:8]` on a length-0 array is a slice-bounds
fault, which golib now raises as Go's own recoverable `runtime error: slice bounds out of range
[:8] with capacity 0` panic rather than a .NET `ArgumentException` — a non-panic exception is
*contained* by a converted test host, which turns a crash into a hang.

Nilness is **representation** identity, not emptiness: `s == nil` on a slice is true exactly for the
nil slice (null backing array), so a non-nil empty (`[]byte{}`, `make([]T, 0)`, `s[len(s):]`) stays
observably non-nil, and nil survives reslicing (`nil[0:0]`) and no-op appends — the distinctions
Go programs (and the stdlib's own tests) rely on. The same holds for a DEFINED slice type
(`type S []int`): the generated wrapper's `== nil` delegates to the wrapped `slice<T>`'s own
`== nil` (representation nilness), so `S{}` is non-nil while the zero value is nil (named map/channel
wrappers were already correct — their backing compares by reference). It holds across a VARIADIC
parameter too, where the pack reaches the callee through a C# `params Span<T>` rather than through a
slice header: a zero-argument call materializes nil, a spread passes exactly the slice it was given,
and the two are told apart by the span's data reference — null for exactly the headers Go calls nil.

**Full detail:** [Reference → Nil and Zero Values](ConversionStrategies-Reference.md#nil-and-zero-values) —
null-safe zero values and pointer-to-interface assignment through selector fields; and
[Reference → Nil-vs-empty slice identity](ConversionStrategies-Reference.md#nil-vs-empty-slice-identity-s--nil-is-representation-nilness-not-emptiness)
— the full construction-identity enumeration.

---

## Empty Interface (`any`)

Every Go type satisfies `interface{}` (spelled `any`), which behaves like .NET's `object`, so the empty
interface maps directly to **`any`** (a global alias for `object`). `func(i interface{})` → `void f(any i)`;
`map[any]string` → `map<any, @string>`.

One wrinkle worth knowing: a Go string literal normally emits as a `"…"u8` `ReadOnlySpan<byte>`, which has
no conversion to `object`, and a plain C# `"…"` boxes a `System.String` where Go boxes `string`. So a
string literal materialized *as `any`* is boxed through `@string` at **every** interface position —
argument included, so `fmt.Println("x")` emits `fmt.Println((@string)"x"u8)` — preserving Go string
identity for a later `x.(string)`, `case string:`, or `==`. The `(@string)` cast is what makes the
combination legal (it converts the ROM span to a heap string, which boxes); the `u8` suffix is then free
and keeps the literal's bytes compile-time constant instead of transcoding them from UTF-16 on every
evaluation. A NAMED string constant needs nothing (it is already emitted as an `@string` member), which
is the exact mirror of the numeric rule below.

The numeric twin: Go materializes an untyped constant into an interface at its **default type** — untyped
int → `int` (go2cs `nint`), untyped rune → `rune` (`int32`), untyped float → `float64` — and every
observation of an interface value dispatches on the boxed CLR type. So an untyped constant boxed *as
`any`* is cast to that type at **every** interface position (`Ꮡv.Store((nint)(42))`,
`fmt.Sprintf("%s%c", d, (int32)(os.PathSeparator))`), else a later `x.(int)` — `x._<nint>()` — finds an
`Int32` and panics, a `case int:` falls through, and interface `==` reports unequal. Two renderings need
it: a bare int literal, which C# makes `System.Int32`; and anything referencing a **named untyped
constant** (`const fsize = 5`), which is a golib `UntypedInt`/`UntypedFloat` *wrapper struct* matching no
Go type at all — leaving it uncast made `fmt`'s dynamic-type dispatch fall back to reflection and print
`{6 %!d(bool=false)}` instead of `6` (go/token's `TestIssue57490`). The variadic `...any` slot and `any`
map keys were once carved out as "cosmetic"; both were real divergences the moment the box was compared
rather than printed (`encoding/base32`'s `testEqual(…, n, 0)` reported `n = 0, expected 0` UNEQUAL; a
`map[any]V` lookup by a real `int` value missed its literal-stored key), so neither is carved out now.

A related identity wrinkle: a deref-aliased pointer (a `*T` parameter or a pointer receiver) passed *as
`any`* renders the box `Ꮡp`, not the deref'd value alias `p` — Go boxes the *pointer*, and dropping the box
would store the pointed-to value, so a later `x.(*T)` would find a bare `T` and panic. This surfaced as
fmt's `sync.Pool` round-trip (`ppFree.Put(p)` then `Get().(*pp)`), which crashed every multi-call fmt
program before the fix.

**Full detail:** [Reference → Empty Interface (`any`)](ConversionStrategies-Reference.md#empty-interface-any) — the
`@string` and default-type boxing across argument, return, assignment, composite-literal, map-key, and
channel-send positions, and the box for a pointer value passed to an `any` argument.

---

## Multi-Assignment and Evaluation Order

Go evaluates every right-hand operand before assigning, which C# expresses with tuple deconstruction:

```go
x, y = y, x+y
```
```csharp
(x, y) = (y, x + y);
```

The deconstruction is **mandatory** whenever the targets alias, and that includes fields — regexp's
`inst.Out, inst.Arg = inst.Arg, inst.Out` must not shatter into two stores, or both fields end up holding
the original `Arg`:

```csharp
(inst.Value.Out, inst.Value.Arg) = (inst.Value.Arg, inst.Value.Out);
```

Go's **partial redeclaration** (`a, b := f()` where `a` already exists) reuses `a` and declares only the
new names, so the converter emits `var` per newly-declared element: `(frac, var e) = normalize(frac);`. A
blank element is a discard with no `var` (`_ = fi;`).

A multi-value **`return`** needs the same care from the opposite direction. Go orders a return's *calls*
and leaves its plain operands unordered against them; gc spills every call to a temporary first, so a plain
operand is read **after** them. A C# tuple literal reads strictly left to right, so where a later call can
write what an earlier operand reads, the converter emits gc's own rewrite:

```go
return o, o.unmarshalOIDText(oid)   // gc: the call runs, THEN o is read
```
```csharp
var ᴛ1 = o.unmarshalOIDText(oid);
return (o, ᴛ1);
```

**Full detail:** [Reference → Multi-Assignment and Evaluation Order](ConversionStrategies-Reference.md#multi-assignment-and-evaluation-order) —
per-element `var` mechanics, escaping/heap-boxed tuple elements, interface-converting deconstruction,
address-taken value locals (the `Ꮡ(value)` copy-vs-box distinction), and the return-operand spill's scope.

---

## Short Variable Redeclaration (Shadowing)

C# forbids a local from shadowing an enclosing local (CS0136). Where Go's `:=` legally shadows, the
converter **renames** the inner variable with a `Δ` suffix and rewrites its references, leaving the outer
one untouched (so its value is naturally preserved):

```go
func sumWithLenLocal(buf []int) int {
    total := 0
    len := len(buf)          // a local shadowing the builtin
    for i := 0; i < len; i++ { total += i }
    return total + len
}
```
```csharp
internal static nint sumWithLenLocal(slice<nint> buf) {
    nint total = 0;
    nint lenΔ1 = len(buf);           // renamed; the builtin call stays len(...)
    for (nint i = 0; i < lenΔ1; i++) { total += i; }
    return total + lenΔ1;
}
```

The mirror case — a local shadowing a package **global** — qualifies the *global* instead
(`runtime_package.Δtrace`), which a local can never shadow. Related renames cover type-vs-method name
collisions (`Δfoo` type vs `foo` method), closure parameters, and consts.

A collision rename is visible **across packages** — `time` declares both `const Second` and
`func (Time) Second() int`, so the const is `ΔSecond` and every consumer must spell it that way:

```go
d := 2 * time.Second                     // any Go program
```
```csharp
var d = 2 * time.ΔSecond;                // the const, not the Second() method group
```

The consumer derives that spelling from the **dependency's own declarations**, so it is the same whether or
not `time` happens to be converted in the same run — which is what makes a standalone `go2cs <dir>` (and
`-recurse`) conversion of such a program compile. It is likewise the same however the source *named* the
type: a renamed type reached through a **dot import** is a bare ident with no package qualifier to rewrite,
and it still resolves through the same imported alias (`Info{…}` and `types.Info{…}` both emit `typesꓸInfo`)
— see [Reference → A DOT-IMPORTED renamed type](ConversionStrategies-Reference.md#a-dot-imported-renamed-type-is-spelled-through-the-same-alias-as-the-qualified-reference).

**Full detail:** [Reference → Short Variable Redeclaration](ConversionStrategies-Reference.md#short-variable-redeclaration-shadowing) —
a large family: forward-collision detection at every block level, package-function shadowing, builtin-method
shadowing, box-name rules for renamed receivers/pointers, and nested-closure capture state.

---

## Multi-Result Values and Comma-Ok Forms

Go functions returning `(value, ok)` / `(value, error)` become ordinary C# value tuples, destructured at
the call site. The runtime's own comma-ok forms (map read, type assertion) use a discard **sentinel** to
select a second overload — `ꟷ` for indexers, `ᐧ` for assertions:

```go
func Atoi(s string) (int, error) {        // strconv/atoi.go
    i64, err := ParseInt(s, 10, 0)
    if nerr, ok := err.(*NumError); ok {
        nerr.Func = fnAtoi
    }
    return int(i64), err
}
```
```csharp
public static (nint, error) Atoi(@string s) {     // strconv/atoi.cs
    var (i64, err) = ParseInt(s, 10, 0);
    {
        var (nerr, ok) = err._<ж<NumError>>(ᐧ); if (ok) {
            nerr.Value.Func = fnAtoi;
        }
    }
    return ((nint)i64, err);
}
```

The single-value assertion `i.(T)` → `i._<T>()` panics on failure; the comma-ok `i._<T>(ᐧ)` returns safely.
An assertion to a *pointer* type renders the box type: `i.(*box)` → `i._<ж<box>>()`.

**Full detail:** [Reference → Multi-Result Values and Comma-Ok Forms](ConversionStrategies-Reference.md#multi-result-values-and-comma-ok-forms) —
package-level `var a, b = f()` component reads, variadic pointer-arg boxing, named-func-result signatures,
and variadic-closure `params` rebinding.

---

## Slices and Arrays

Go slices and arrays convert to golib `slice<T>` and `array<T>`. A composite literal builds a C# array and
projects it with `.slice()` / `.array()`; `make` uses a constructor:

```go
primes := [6]int{2, 3, 5, 7, 11, 13}   // array literal
nums := []int{10, 20, 30}              // slice literal
buf := make([]byte, 4)                 // make
```
```csharp
var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
var nums = new nint[]{10, 20, 30}.slice();
var buf = new slice<byte>(4);
```

A `[N]T` literal that writes fewer than `N` elements passes the declared length to the projection, because
Go zero-fills the remainder — `[8]byte{}` is eight zero bytes, not an empty array. A full literal keeps the
plain `.array()`, and a slice literal never pads:

```go
seed := [8]byte{1, 2}   // 1, 2, then six zeros
```
```csharp
var seed = new byte[]{1, 2}.array(8);
```

See [the reference](ConversionStrategies-Reference.md#a-fixed-array-composite-literal-carries-its-declared-length-arrayn)
for the keyed/`SparseArray` form and the nested-array gap.

`array<T>` carries its element type but not its LENGTH — C# has no const generic to hold the `N` of
`[N]T` — so wherever the length has to be recoverable at runtime it comes from the emitted code: a
value measures itself, and a struct field reads the dimension back out of the field initializer the
converter emits (`= new(32)`). A func **parameter** is the one position with neither, so it carries
the dimension as an attribute instead — which is what makes `reflect.TypeOf(f).In(0).Len()` answer
32 rather than 0, and `testing/quick` generate a real 32-byte array rather than an empty one:

```go
f1 := func(in [32]byte, sc Scalar) bool { … }
```
```csharp
var f1 = ([GoArrayDims(32)] array<byte> @in, Scalar sc) => { … };
```

See [the reference](ConversionStrategies-Reference.md) (*A func PARAMETER is the one position an
array's LENGTH cannot be recovered from*) for the delegate-instance read behind it.

A struct field takes the same attribute wherever its initializer cannot reach — that route recovers
an array the field IS, and nothing an array is BEHIND. The zero instance of a field declared
`*[3]float64` holds a nil pointer with no pointee to measure, and one declared
`map[[2]string][2]*float64` holds a nil map whose key and element types no entry could reveal. Both
are ordinary shapes at a **decode target**, which is exactly a struct nothing has populated yet, so
the two accessors get their cargo from the declaration:

```go
type T1 struct {                              // encoding/gob's TestEndToEnd
    Marr map[[2]string][2]*float64
    N    *[3]float64
}
```
```csharp
[GoType] partial struct T1 {
    [GoArrayDims(2), GoMapKeyDims(2)]
    public map<array<@string>, array<ж<float64>>> Marr;
    [GoArrayDims(3)]
    public ж<array<float64>> N;
}
```

`[GoArrayDims]` is what `Elem()` hands down and `[GoMapKeyDims]` what `Key()` does, so one stamp
covers any pointer depth (`***[3]int` carries the same `[GoArrayDims(3)]`, the cargo passing down
unshifted at every hop). A field that IS an array keeps its initializer, and a DEFINED array or map
type is not stamped — its managed form is a generated wrapper with no slot to carry cargo. See the
[field dims cargo](ConversionStrategies-Reference.md#a-struct-fields-type-only-array-dims--goarraydims--gomapkeydims-2026-08-20)
section of the reference.

`append`, `len`, `make`, and sub-slicing map to golib builtins/methods. A variadic `...T` parameter arrives
as `params ꓸꓸꓸT`, where `ꓸꓸꓸT` is a using alias for `Span<T>` whose identifier mirrors the Go name
(`...*RangeTable` → `ꓸꓸꓸжRangeTable`, `...unsafe.Pointer` → `ꓸꓸꓸunsafeꓸPointer`), falling back to an inline
`params Span<T>` for an element type that cannot form a legal alias identifier (a type parameter, or a
constructed type such as `[]byte`). At the top of the body, a variadic used only through `len`/`cap`,
indexing, or range binds to the allocation-free stack view `sslice<T>`; a value that may escape, grow,
or cross a closure/execution-wrapper boundary keeps the heap `slice<T>` fallback. On the CALL side, an
argument that Go reads as one element but C# could bind as the whole pack is cast to the element type —
a bare `nil`, and a `[]E`/`[N]E` passed without `...` (`f(a)` with `a []any` means a pack of ONE, since
spreading needs `a...`); both otherwise bind C#'s preferred *normal* form and silently lose an argument
or a level of nesting. See
[untyped constants boxed as `any`](ConversionStrategies-Reference.md#an-untyped-constant-boxed-as-any-boxes-at-gos-default-type)
in the reference. From the real stdlib:

```go
func Join(errs ...error) error {          // errors/join.go
    // ...
    e := &joinError{errs: make([]error, 0, n)}
    for _, err := range errs {
        if err != nil { e.errs = append(e.errs, err) }
    }
    return e
}
```
```csharp
public static error Join(params ꓸꓸꓸerror errsʗp) {     // errors/join.cs
    var errs = errsʗp.sslice();
    // ...
    var e = Ꮡ(new joinError(errs: new slice<error>(0, n)));
    foreach (var (_, err) in errs) {
        if (err != default!) { e.Value.errs = append((~e).errs, err); }
    }
    return new joinErrorжerror(e);
}
```

Arrays are Go **values**: every transfer copies the whole array. `array<T>` is a struct over a
shared `T[]`, so the converter appends a strongly-typed `.Clone()` wherever an array value is read
out of existing storage — assignment, range elements, composite-literal elements and struct fields,
returns, channel sends, `append` elements, and function parameters (cloned in the callee preamble).
Named array types clone the same way through their generated wrapper's own `Clone()`, and the copy
is **deep** for nested arrays (`[2][3]int` copies its inner arrays too, matching Go):

The range **expression** is one of those sites: Go evaluates it once, so `for i, v := range a` over an
array value iterates a COPY and a write to `a` inside the body is invisible to later iterations. It
gets its own member rather than `.Clone()`, because a snapshot cannot outlive its loop — that makes it
Go's inline, stack-resident copy, which costs zero allocations, so golib takes it from a pooled buffer
released when the loop ends. A range with no value variable (`for i := range a`), over a pointer to an
array, or over a slice copies nothing in Go, and neither does the emission.

```go
data := ints                          // an independent copy — writes to data never reach ints
for _, row := range m { row[0] = 9 }  // row is a per-iteration copy — m is never written
for i, v := range a {                 // v is read from a SNAPSHOT taken before the loop
    if i == 0 { a[1] = 91 }           // …so this write is invisible to the next iteration
}
```
```csharp
var data = ints.Clone();
foreach (var (_, vᴛ1) in m.ΔRangeSnapshot()) { var row = vᴛ1.Clone(); row[0] = 9; }
foreach (var (i, v) in a.ΔRangeSnapshot()) {
    if (i == 0) { a[1] = 91; }
}
```

A **struct** whose field is a fixed-size array carries the same shared `T[]` into a plain struct copy,
so it clones at exactly the same sites. The converter stamps the struct with the fields that need the
deep copy and go2cs-gen generates it, under a `Δ`-marked name so it cannot shadow a Go type's own
`Clone` method. crypto/sha256's `Sum` is the real case — it copies the digest so the caller can keep
writing, then destroys the copy finalizing it:

```go
d0 := *d                 // sha256.go — Go copies the [8]uint32 state and [64]byte block INLINE
hash := d0.checkSum()
```
```csharp
[GoType] partial struct digest { internal array<uint32> h = new(8); … }

// package_info.cs names the fields the copy must deep-copy, keeping the declaration Go-shaped
[GoValueClone("h", "x")] internal partial struct digest {}

ref var d0 = ref heap<digest>(out var Ꮡd0);
d0 = d.ΔClone();         // sha256.cs — without the clone, checkSum destroyed the CALLER's state
var hash = Ꮡd0.checkSum();
```

Go's two array-**pointer** conversions are the exception to that copying: each yields a *view* of
storage that already exists, so `array<T>` carries a `(low, length)` window and both emit an alias
rather than a snapshot. `(*[N]T)(s)` windows a slice; `(*[N]T)(unsafe.Pointer(p))` with `p` a `*T`
windows the storage `p` is an element of — internal/poll's console read buffer, filled by os's own
test through exactly this shape:

```go
d := (*[4]byte)(dst)                                        // image/png writer.go — shares dst's array
n = copy((*[10000]uint16)(unsafe.Pointer(buf))[:n:n], s16)  // os os_windows_test.go
```
```csharp
var d = Ꮡ(array<byte>.Alias(dst, 4));
n = copy((~array<uint16>.AliasPointer(Ꮡbuf, 10000)).slice(-1, n, n), s16);
```

A write through either has to reach the caller's buffer; against a copy it is discarded silently,
which is a wrong answer rather than a slow one. The value forms — `[4]byte(s)`, `*p` — still copy,
exactly as Go's do.

**Full detail:** [Reference → Slices and Arrays](ConversionStrategies-Reference.md#slices-and-arrays) —
named slice/array wrappers, pointer-to-array slicing, named-slice pointer reinterpretation, structural
composite rendering, array value-copy cloning (deep for nested arrays), the
[struct-carrying-arrays clone](ConversionStrategies-Reference.md#a-struct-carrying-array-fields-copies-through-its-generated-δclone),
[element-pointer array aliasing](ConversionStrategies-Reference.md#an-element-pointer-reinterpreted-as-an-array-pointer-aliases-the-elements-storage)
and slice-aliasing/write-through semantics.

---

## Strings (`@string` and `sstring`)

Go's `string` is represented by golib [`@string`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/string.cs):
an immutable byte string whose `len`, indexing, ranging, concatenation, and comparisons are byte-oriented
like Go's, not UTF-16-oriented like `System.String`. It also carries Go's string *header* — a backing array
plus an **offset and length** — so `s[i:j]` is an O(1) window over shared storage rather than a copy, which
is what keeps the ubiquitous `s = s[n:]` and `DecodeRuneInString(s[i:])` idioms linear instead of quadratic
([detail](ConversionStrategies-Reference.md#slicing-a-string-is-a-window-not-a-copy--string-carries-an-offset-and-a-length)).
Plain string literals usually render as
`"..."u8` `ReadOnlySpan<byte>` values, then target-type into `@string` only when a heap string is actually
needed. That keeps common literal-to-slice and literal-comparison forms allocation-free:

```go
var s string = "ready"
b := []byte("hi")
```
```csharp
@string s = "ready"u8;
var b = slice<byte>("hi"u8);
```

A string↔bytes conversion is a cast over the golib types: `string(b.buf[b.off:])` →
`(@string)(b.buf[(int)(b.off)..])`, and `[]byte(s)` → `slice<byte>(s)`. `[]rune(s)` decodes through the
Go string model rather than the CLR string model. Literals with raw byte escapes that cannot be expressed
faithfully as UTF-8 source (for example high `\xHH` bytes or greedy hex escapes) emit as byte-array-backed
`@string`, preserving Go's exact bytes.

A literal that materializes a **value** is **hoisted** to a package-scoped `private static readonly` field
declared immediately above the function that first uses it, so it costs at most one allocation per program
run instead of one per evaluation — Go's own RODATA cost model:

```go
func FormatBool(b bool) string {
	if b { return "true" }
	return "false"
}
```
```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string trueˢ = "true"u8;
private static readonly @string falseˢ = "false"u8;

public static @string FormatBool(bool b) {
    if (b) { return trueˢ; }
    return falseˢ;
}
```

The `ˢ` suffix marks a converter-synthesized name, like `ᴛ` for temporaries and `Δ` for renames. Hoisting
covers value-materializing contexts only — returns, assignments, `string` and `any` arguments, map keys,
named-string conversions — and deliberately skips the contexts where the inline literal is already free, or
where a name derived from the literal's content would read worse than the value itself: comparisons and
concatenations (golib compares and concatenates a `u8` span in place), `[]byte`/`[]rune` sources (the copy is
mandatory), format strings, composite-literal elements, `func init()` bodies, package-level initializers, and
literals whose slug carries no information. A literal used *only* in `any` slots is emitted pre-boxed, so
those sites allocate nothing at all. See the reference for the full inclusion/exclusion tables, the naming
rules, and the initialization-order guarantee.

Named string types are real wrapper structs (`type relationship string`), so the generated type keeps the
string surface: indexing, sub-slicing, `len`, comparisons, concatenation, constants, and method calls stay
on the named type instead of collapsing back to plain `@string`. Concatenation matters twice over: Go keeps
the named type across a `+`, so the wrapper carries its own `+` overloads (including against a `u8` span) —
without them C# falls back to `string.Concat`, handing back a `System.String` stripped of the type's
methods.

```go
type Token string
func (t Token) First() byte { return t[0] }
const done Token = "done"
next := done + "-next"    // still a Token
```
```csharp
[GoType("@string")] partial struct Token;
internal static readonly Token done = "done"u8;
public static byte First(this Token t) => t[0];
Token next = done + "-next"u8;
```

Most `string([]byte)` conversions must copy into `@string` — the price of Go's immutable-string guarantee.
Go's own compiler *elides* that copy when the resulting string does not escape and its source is not
modified while it is alive, letting the string alias the bytes in place. The converter recovers this common
fast path with a second string type,
[`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs): a
stack-only `readonly ref struct` that *views* a `ReadOnlySpan<byte>` with **no allocation**.

A provably-safe `string([]byte)` conversion emits `sstring`; anything that escapes stays `@string` (the implicit
`sstring`→`@string` conversion copies to the heap at that boundary, so correctness never depends on getting
the analysis right — only performance does).

Safety is enforced two ways. Because `sstring` is a `ref struct`, the .NET compiler forbids every way a
string could escape — a field, array, map, interface box, channel, closure, or a return past its data's
lifetime — so an over-reach is a **compile error, not a silent bug**. The one hazard the compiler cannot
see — the source slice mutated while the view is alive — the converter's escape analysis rules out. So
`sstring` appears only for a non-escaping conversion used in **read-only** positions: a comparison, a
`switch` tag, `len`/index, or a concatenation operand.

```go
if string(hdr[:4]) == wantMagic { … }        // compare a slice against a []byte-derived string
switch string(cmd) { case "get": …; case "put": … }
```
```csharp
if (((sstring)(hdr[..4])) == wantMagic) { … }   // mixed sstring/@string compare — no heap copy
var exprᴛ1 = ((sstring)cmd);                     // a string switch lowers to == comparisons
if (exprᴛ1 == "get"u8) { … } if (exprᴛ1 == "put"u8) { … }
```

Comparing an `sstring` against a `"…"u8` literal, an `@string`, or another view runs zero-allocation
directly over the backing spans — which is where the win shows: the eligible comparison idiom measures
~11–12× faster than the `@string` copy-and-compare. A repeated conversion in a loop is hoisted to a single
reused view; everything that escapes simply stays `@string`.

**Full detail:** [Reference → Strings (`@string` and `sstring`)](ConversionStrategies-Reference.md#strings-string-and-sstring) —
literal rendering, string↔`[]byte`/`[]rune` conversions, named-string wrapper behavior, high-`\x`-escape
byte arrays, the exact `sstring` eligibility predicate, comparison / `switch` / concatenation forms,
loop-invariant hoisting, and the `SStringElision` guard test.

---

## Maps and Channels

Go maps and channels convert to golib `map<K,V>` and `channel<T>`; `make` becomes a constructor, and
send/receive/`select` use runtime operators. Map reads honor Go's nil-map and comma-ok semantics:

```go
m := make(map[string]int)
c := make(chan int, 3)
u := make(chan int)             // unbuffered: rendezvous
unit, ok := unitMap[u]          // comma-ok read (time/format.go)
```
```csharp
var m = new map<@string, nint>();
var c = new channel<nint>(3);
var u = new channel<nint>(0);        // capacity 0 — real rendezvous semantics
var (unit, ok) = unitMap[u, ꟷ];      // two-value indexer via the ꟷ sentinel
```

A **nil key** is an ordinary key in Go wherever the key type can be nil (`map[any]V`, `map[error]V`,
`map[*T]V`), and it renders as `default!` — `m[nil] = "x"` → `m[default!] = xˢ`. `Dictionary<K,V>`
rejects a null key outright, so golib's backing store is a Dictionary subclass carrying a dedicated
nil-key slot that every map member routes to; the test that finds it is a JIT-time constant, so a
value-type key (`map[string]V`, `map[int]V`) compiles to exactly the code it did before and `PerfMap`
stays flat.

A **`range` body may mutate the map it is ranging over**, because Go's spec says it may: an entry
removed before it is reached is not produced, and an entry created during the range "may be produced
… or may be skipped". `Dictionary<K,V>`'s enumerator allows neither — a structural insert bumps its
version and the next `MoveNext` throws `InvalidOperationException` — so `map<K,V>` implements the
contract itself, walking a snapshot of the entries and re-reading each value on arrival. The emitted
code is an ordinary `foreach`; the fidelity lives in the runtime type. Overwrites and deletes never
threw (both are version-free since .NET Core 3.0), which is exactly why the insert case survived so
long: it is what hung `net/http`'s HTTP/2 server in `promoteUndeclaredTrailers`. A **NaN key** is the
one shape the arrival lookup cannot settle — it is equal to nothing, itself included, so the lookup
always misses and the entry would vanish from the range — so a miss is disambiguated with the store's
own comparer; `encoding/json`'s `TestMarshalTextFloatMap` is what reads that out. See the
[range-over-map](ConversionStrategies-Reference.md#a-range-body-may-mutate-the-map-it-is-ranging-over--the-enumerator-walks-a-key-snapshot)
section of the reference.

An **interface key compares by Go equality, not by wrapper identity**. Go compares interface values by
(dynamic type, dynamic value), and that one relation serves both `==` and map lookup. But a converted
interface value is presented through whichever generated adapter its current static interface calls for,
so asserting an `Object` to a narrower `dependency` hands back a *different* wrapper over the same
receiver box. A map keyed by an interface therefore installs golib's `GoEqualityComparer`, which projects
the same `builtin.AreEqual` that emitted `==` uses and hashes the unwrapped root; without it the asserted
value could not find its own entry, while `==` on that very pair still said `true`. That split is what
stopped `go/types` from type-checking anything — `initorder.dependencyGraph` is exactly this shape — and
it is scoped to interface/`any` keys, so concrete keys keep `EqualityComparer<K>.Default`'s fast path.
See the [interface map key](ConversionStrategies-Reference.md#an-interface-map-key-compares-by-go-equality-never-by-adapter-identity)
section of the reference.

A **`m[string(b)]` READ does not copy the key**, matching the Go compiler's own special case
(`runtime.slicebytetostringtmp`): a lookup hashes and compares its key but never retains it, so the
converter emits golib's `tmpstring(b)` — a transient `@string` windowing the slice's live bytes,
zero allocation. Everywhere the string escapes (a store `m[string(b)] = v`, `delete`, a return) the
copying conversion stays:

```go
if v := commonHeader[string(a)]; v != "" { return v, true }   // net/textproto reader.go
```
```csharp
@string v = commonHeader[tmpstring(a)]; if (v != ""u8) { return (v, true); }
```

golib's `channel<T>` is a faithful port of Go's runtime channel (hchan + selectgo): an unbuffered
send really waits for a receiver, `cap`/`len` report Go's values, a blocking `select` commits
exactly ONE case chosen uniformly at random among the ready ones, and close/panic semantics match
Go — see the [channel runtime](ConversionStrategies-Reference.md#real-channel-runtime--the-hchanselectgo-port-rendezvous-caplen-single-fire-uniform-random)
section of the reference. One channel has an owner that can take a value BACK: Go 1.23's synchronous
timer channel, where `Stop`/`Reset` guarantee that no tick from before the call can be received after
it — so `time.Timer.C` reports `len` and `cap` of 0 even while it holds a tick, and the pre-1.23
"drain the channel if `Stop` returned false" idiom is unnecessary. golib models it with Go's own
`hchan.timer` hook rather than a `time` special case.

A channel's **DIRECTION** is part of its Go type and is the one part `channel<T>` cannot express —
`chan T`, `chan<- T` and `<-chan T` all emit as one managed type, distinguished for the reader only
by a `/*<-*/` marker comment. So it rides on the VALUE and reaches `reflect` as descriptor cargo,
exactly the way a fixed-size array's length does, stamped at the three places a directional channel
value is born (a `make`, a struct field's zero, and `new`):

```go
ch := make(chan<- int)                       // text/template's TestIssue43065
type holder struct{ x chan<- string }
```
```csharp
var ch = new channel/*<-*/<nint>(0, GoChanDir.Send);
[GoType] partial struct holder {
    internal channel/*<-*/<@string> x = channel/*<-*/<@string>.SendOnly;
}
```

`reflect.Type.ChanDir()` and `String()` then answer `chan<- int` rather than the bidirectional type,
which is what lets `text/template`'s `walkRange` refuse a range over a send-only channel — and that
guard is why `reflect.Value.Recv`/`Send` are bridged in the same change: a working receive behind a
direction that always read bidirectional turns that refusal into an unbounded hang. A NARROWING
conversion (`var s chan<- int = ch`) and a DEFINED channel type are deliberately not stamped. See the
[chan direction cargo](ConversionStrategies-Reference.md#the-chan-direction-is-carried-by-the-value--descriptor-cargo-exactly-like-an-arrays-length-2026-08-20)
section of the reference.

A goroutine over a `select` — the concurrency core — lowers to `goǃ(...)` and a `switch` over `select(...)`,
with `ᐸꟷ` marking a receive-case and `ꟷᐳ` performing the receive. Every case's operands are hoisted
into select-scoped temps (`selᴛN`) emitted in strict source order and evaluated exactly once at
select entry — Go's evaluation rule — so the registration list names only temps: a receive case's
channel operand (used by both the registration and the winning case's guard), and a send case's whole
registration call, which only builds the case descriptor and so moves no send:

```go
go func() {                     // context/context.go
    select {
    case <-parent.Done():
        child.cancel(false, parent.Err(), Cause(parent))
    case <-child.Done():
    }
}()
```
```csharp
goǃ(() => {                     // context/context.cs
    var selᴛ2 = parent.Done();
    var selᴛ3 = child.Done();
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out _): {
        child.cancel(false, parent.Err(), Cause(parent));
        break;
    }
    case 1 when selᴛ3.ꟷᐳ(out _): { break; }}
});
```

With a `default:` clause the select becomes non-blocking: the same registrations feed `trySelect(…)`,
which commits at most ONE ready case (uniformly at random among the ready ones) and returns -1 when
none is — so the C# `default:` label runs exactly when Go's would. A full channel falls to the
`default:` exactly as in Go, and a send on a closed channel panics even though a default exists:

```go
select {                        // os/signal/signal.go
case c <- sig:
default:                        // send but do not block for it
}
```
```csharp
var selᴛ1 = c.ᐸꟷ(sig, ꓸꓸꓸ);            // os/signal/signal.cs
switch (trySelect(selᴛ1)) {
case 0: {
    break;
}
default: {
    break;
}}
```

**Full detail:** [Reference → Maps and Channels](ConversionStrategies-Reference.md#maps-and-channels) —
the nil map key's dedicated slot, the `m[string(b)]` no-copy read key (`tmpstring`), named map/channel
types, constrained map access through type parameters, the real channel runtime
(hchan + selectgo: rendezvous, cap/len, single-fire, uniform-random), and full `select` lowering
(terminating/empty clauses, escaping comm-clause bindings).

---

## Generic Constraints

A Go generic constraint becomes a C# `where` clause. Type-set constraints lift to the matching golib/.NET
interface (`[]T`→`ISlice<T>`, `[N]E`→`IArray<E>`, `map[K]V`→`IMap<K,V>`, `chan T`→`IChannel<T>`), and an
operator-bearing type set additionally lifts the `System.Numerics` operator interfaces so the body's
`+`/`<`/`==` compile. `comparable` emits no C# constraint beyond `new()` (no C# interface can admit
Go's full `==`-able set): Go's checker already validated every instantiation, and emitted equality on
a type-parameter operand routes through golib's `AreEqual`. A generic struct's own generated `Equals`
follows the same rule **per field** — fields whose type carries its own `==` (a `ж<T>` pointer, a
golib wrapper, another `[GoType]` struct) compare with `==`, and only genuine type-parameter fields
route through `AreEqual`.

```go
type Ordered interface {                  // cmp/cmp.go
    ~int | ~int8 | /* … */ | ~float64 | ~string
}
func Less[T Ordered](x, y T) bool {
    return (isNaN(x) && !isNaN(y)) || x < y
}
```
```csharp
[GoType("operators = Sum, Comparable, Ordered")]      // cmp/cmp.cs
partial interface Ordered<ΔT> { /* type set + derived operators, as comments */ }

public static bool Less<T>(T x, T y)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>,
              IComparisonOperators<T, T, bool>, new()
{
    return (isNaN(x) && !isNaN(y)) || x < y;
}
```

**Full detail:** [Reference → Generic Constraints](ConversionStrategies-Reference.md#generic-constraints) —
array-core `~[N]E` lifting, single-term pointer constraints (`[P *T]` → `ж<T>`), method-set interface
constraints and self-referential proxies, `comparable`, per-field generic-struct equality, unions
(`string | []byte`), and explicit type-argument handling.

---

## Type Aliasing

Go has two forms. A **type definition** (`type Celsius float64`) is a distinct type sharing an underlying;
because converted types are structs (no inheritance), the source generators emit the bridging (implicit
conversions to the underlying, interface implementations, receiver-method proxies). A **type alias
declaration** (`type P = *bool`) is true aliasing, emitted as a C# **global using**:

```go
type P = *bool
type table = map[string]int
```
```csharp
global using P = go.ж<bool>;
global using table = go.map<go.@string, nint>;
```

The RHS is namespace-rooted all the way down, unlike every other rendering the converter emits. C#
resolves a using alias's target *as if the compilation unit had no using directives*, which puts it
outside the file's `namespace go;` and outside the package class — so a nested `@string` would name
nothing there, and neither would a same-package `Header` (it is `go.main_package.Header`) or the `Func`
of a func-type alias (`System.Func`). The golib csproj-alias names go the other way and are substituted
rather than rooted, since `uint64` and friends stand for C# keywords: `type fe = [4]uint64` emits
`global using fe = go.array<ulong>;`.

**Full detail:** [Reference → Type Aliasing](ConversionStrategies-Reference.md#type-aliasing) — self-boxing
pointer conversions, the rooted-nesting RHS and its four qualifiers, keyword-safe RHS rendering,
`types.Unalias` at type-switched decision points, and same-package alias-target namespace qualification.

---

## Delegates to Value Receiver Instances

In Go a function is a value, and a **value-receiver method value** captures a *copy* of the receiver at the
moment it's taken — a subtlety that surprises non-Go programmers:

```go
d := data{name: "James"}
f1 := d.printName
f1()                 // "Name = James"
d.name = "Gretchen"
f1()                 // "Name = James" again — f1 bound a copy of d
```

To preserve this, the converter copies the receiver value into the delegate's capture (a snapshot taken at
assignment time), rather than capturing by reference. Method *expressions* (`(*T).M`), bound method values,
pointer-receiver method values, and conversions to named func types each have a tailored emission (a cast to
the concrete delegate, a box-bound method group, or `new NamedDelegate(...)`).

A **pointer**-receiver method value is the mirror image and needs the opposite treatment: `c.dec` is Go
shorthand for `(&c).dec`, so it must alias the receiver, not copy it. That implicit address-of heap-promotes
the local exactly like an explicit `&c` would — the escape analysis treats the two identically, and the
method group binds to the box:

```go
c := counter{n: 100}
applyInt(c.dec, 5, 7)        // (&c).dec — c.n is 88 afterwards
```
```csharp
ref var c = ref heap<counter>(out var Ꮡc);
c = new counter(n: 100);
applyInt(Ꮡc.dec, 5, 7);      // Ꮡc aliases c
```

That holds in **argument and assignment position alike** (`f = c.dec` emits the same `Ꮡc.dec` group), and in
assignment position it also means *no* receiver snapshot is taken — there is no copy to snapshot.

A direct call `c.dec()` is *not* a method value — it binds C#'s `this ref` extension receiver against the
variable and needs no box.

**Full detail:** [Reference → Delegates to Value Receiver Instances](ConversionStrategies-Reference.md#delegates-to-value-receiver-instances) —
method expressions (local & foreign), bound/interface/pointer/value-receiver method values, the go-statement
sibling, named and generic func-type conversions.

---

## Defer / Panic / Recover

A Go function that defers or recovers keeps its body exactly where Go put it: the statements are
emitted **inline** in the method, inside `try`/`catch`/`finally`, beside a `GoFrame` local that holds
this call's defer list. The `catch` parks a panic where `recover()` can read it; the `finally` drains
the deferred calls, which is Go's guarantee that they run on every exit path; and the frame is a
`ref struct`, so it lives in the stack frame and allocates nothing. A deferred call registers with
`defer(fn, args…, ref ᒐ)` — the arguments are captured there because Go evaluates them at the `defer`
statement. `panic(x)` lowers to `throw panic(x)`:

```go
func withLock(lk sync.Locker, fn func()) {   // database/sql/sql.go
    lk.Lock()
    defer lk.Unlock() // in case fn panics
    fn()
}
```
```csharp
internal static void withLock(sync.Locker lk, Action fn) {   // database/sql/sql.cs
    GoFrame ᒐ = default;
    try {
        lk.Lock();
        defer(lk.Unlock, ref ᒐ);
        // in case fn panics
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

A function with **named results** that deferred code mutates declares them ahead of the `try` and
returns them after the `finally`, because Go runs the deferred calls after the results are assigned
and before the caller sees them — which a `finally` cannot do to a value a `return` has already
evaluated. Every exit inside the `try` therefore leaves through a `goto`, which runs the `finally`
exactly as a return would.

An unrecovered panic — even in a goroutine — crashes the process exactly as in Go: golib's
`AppDomain.UnhandledException` backstop writes the `panic: …` report to stderr and exits with code 2.
The VALUE in that report follows Go's `preprintpanics` rule, so an `error` prints its `Error()` and a
`Stringer` its `String()` — `panic: open final.txt: code 13`, never the pointer's address — and the
substitution runs only on the printing path, so a recovered panic never calls either.

`recover()` is a static call reading the one thread-local slot the emitted `catch` parked the panic
in — which is what lets a deferred closure recover without holding any handle on the frame that
registered it. A re-`panic` is `throw panic(err)`:

```go
if err := recover(); err != nil {   // fmt/print.go
    // ...
    if p.panicking { panic(err) }
```
```csharp
var err = recover(); if (err != default!) {   // fmt/print.cs
    // ...
    if (p.panicking) { throw panic(err); }
```

A **traceback** taken while a panic is being handled (`runtime.Stack`, `debug.Stack`) reports what Go
reports. Go keeps the panicking frames on the stack until the panic completes; the CLR unwinds them
before a `finally`-based defer runs, so golib snapshots the panic's origin at the first catch — and a
re-`panic` inherits it, which is what keeps the origin visible through Go's
`defer func(){ panic(recover()) }()` idiom. The frames render in Go's shape
(`sync_test.onceFuncPanic()` over a tab-indented `file:line`), not the CLR's
`at go.sync_test_package.onceFuncPanic(…)`, because a traceback is observable output that programs
and tests read by package-qualified name.

The **programmatic** traceback — `runtime.Caller`, `runtime.Callers`, `Frames.Next` — walks the same
managed stack, filtered to the frames the *Go source* declares, so relative depths and `skip`
counting behave as in Go (go2cs's own adapter shells and generated forwarders are invisible, exactly
as Go's interface dispatch adds no frame). The one honest difference is `file`/`line`: they name the
**converted `.cs`** position, because that is the source the running program actually has.

**Full detail:** [Reference → Defer / Panic / Recover](ConversionStrategies-Reference.md#defer--panic--recover) —
the frame's emitted forms and why the body is not a lambda, the named-result `goto` exit, the
registration ladder, unrecovered-panic process exit (stderr + code 2), named-delegate/builtin callees,
value-returning goroutine wrapping, func-literal argument capture hoisting, the golib family-delegate
cast a VARIADIC deferred literal needs (a `params` lambda converts to no `Action<…>`), and box-bound
deferred pointer-receiver methods; plus
[Reference → `runtime.Stack` renders a GO-shaped traceback](ConversionStrategies-Reference.md#runtimestack-renders-a-go-shaped-traceback-and-recovers-the-panic-site).

---

## Expression Switch Statements

Go's expression `switch` (no automatic fall-through) usually lowers to `if / else if`, which handles cases
whose labels aren't C# compile-time constants (variables, `static readonly` consts, addresses). When every
label *is* a constant and there's no `fallthrough`, a real C# `switch` is used. A tag-less
`switch { case cond: }` lowers to a `switch` over the sentinel `ᐧ` with each arm a `when` guard:

```go
switch {                              // path/path.go
case path[r] == '/':
    r++
case path[r] == '.' && (r+1 == n || path[r+1] == '/'):
    r++
}
```
```csharp
switch (ᐧ) {                          // path/path.cs
case {} when path[r] is (rune)'/': {
    r++;
    break;
}
case {} when path[r] == (rune)'.' && (r + 1 == n || path[r + 1] == (rune)'/'): {
    r++;
    break;
}}
```

`fallthrough` expands to an if-chain with a fall flag and `goto`; a switch-targeting `break` inside an
if-else arm is wrapped in a one-shot `do { … } while (false)`. Because the chain emits clauses in
source order, a `default:` that Go places *before* some of its cases is guarded on a predicate
precomputed over **every** case label — never on the running match flag, which has not yet seen the
arms below it.

**Full detail:** [Reference → Expression Switch Statements](ConversionStrategies-Reference.md#expression-switch-statements) —
constant-vs-runtime label detection, `static readonly` tags, `fallthrough` + guarded-default returns,
non-trailing `default` clauses, and index/named-type case labels.

---

## Type Switch Statements

A Go type switch maps cleanly to C#'s type-pattern `switch`. The dynamic type comes from `.type()`, and
each `case T:` binds the value with a type pattern:

```go
func do(i interface{}) {
    switch v := i.(type) {
    case int:
        fmt.Printf("Twice %v is %v\n", v, v*2)
    case string:
        fmt.Printf("%q is %v bytes long\n", v, len(v))
    default:
        fmt.Printf("I don't know about type %T!\n", v)
    }
}
```
```csharp
internal static void @do(any i) {
    switch (i.type()) {
    case nint v: {
        fmt.Printf("Twice %v is %v\n"u8, v, v * 2);
        break;
    }
    case @string v: {
        fmt.Printf("%q is %v bytes long\n"u8, v, len(v));
        break;
    }
    default: {
        // ...
    }}
}
```

Cases that match an *anonymous interface* (`case interface{ Unwrap() error }:`) synthesize a named
`[GoType("dyn")]` interface and test it with `case {} Δx when Δx._<is_typeᴛ1>(out var x):`.

**Full detail:** [Reference → Type Switch Statements](ConversionStrategies-Reference.md#type-switch-statements) —
the tag-evaluates-once guarantee, default-arm binding, astral rune literals, and generic/embedded arms.

---

## Labeled Control Flow and Loop Variables

Go labels sit immediately before their statement; C# reproduces the behavior with a placed label and a
`goto`:

```go
Outer:
    for i := 0; i < n; i++ {
        for j := 0; j < m; j++ {
            if done { break Outer }
        }
    }
```
```csharp
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < m; j++) {
            if (done) { goto break_Outer; }
        }
    }
    break_Outer:;
```

**Full detail:** [Reference → Labeled Control Flow and Loop Variables](ConversionStrategies-Reference.md#labeled-control-flow-and-loop-variables) —
break vs continue label placement, labels on empty statements, and per-iteration loop-variable semantics
(Go 1.22).

---

## Struct Types

Go structs become C# `struct`s (stack-friendly; heap-boxed as `ж<T>` when they escape). The converter emits
a `[GoType]` partial struct with just the fields; the `TypeGenerator` synthesizes equality, `ISupportMake`,
and embedding promotion. Access modifiers follow Go's exported/unexported naming:

```go
type List struct {                    // container/list/list.go
    root Element
    len  int
}
```
```csharp
[GoType] partial struct List {        // container/list/list.cs
    internal Element root;
    internal nint len;
}
```

Inline/anonymous types are "lifted" out (a local `type x struct{…}` in `main` → `main_x`; an anonymous
struct → `settingsᴛ1`) from **any depth** of the declared type — `[]*struct{…}` and `map[K]*struct{…}`
lift exactly as a bare `struct{…}` does. The empty `struct{}` maps to the shared golib `EmptyStruct`,
and an empty `interface{}` field to `any` — neither is lifted.

**Full detail:** [Reference → Struct Types](ConversionStrategies-Reference.md#struct-types) — field-name
collisions in generated equality, combined field lines, local/anonymous-type lifting (and the recursive
descent that reaches an anonymous type through pointer/slice/map/channel composition), and recorded
implicit conversions between structurally-identical anon structs.

---

## Struct Type Embedding

Go uses embedding instead of inheritance. Since C# structs can't inherit, the `TypeGenerator` adds a field
for the embedded type and **promotes** its fields and methods — transitively through every level, and for
both value and pointer (`*T`) embeds:

```go
type reverse struct {                 // sort/sort.go
    Interface                         // embedded — Len/Less/Swap promoted
}
func (r reverse) Less(i, j int) bool {
    return r.Interface.Less(j, i)     // this method overrides the promoted Less
}
```
```csharp
[GoType] partial struct reverse {     // sort/sort.cs
    public Interface Interface;        // the embed becomes an explicitly-named field
}
internal static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}
```

The embed field is named by **Go**, not by the C# rendering of its type: Go calls the field of `struct{ *myInt }`
`myInt`, and that name — with its Go exportedness — is what the member, the generated constructor parameter and
the promotion accessor all carry. The two strings coincide for an ordinary embed; they diverge whenever the
converter renames the type, as it does when hoisting a function-local one to package scope.

An embed is an **inline field**, so a Go value copy copies it exactly as Go does — it was held in a shared
`ж<T>` box until 2026-08-14, which gave the embed reference semantics a C# struct assignment then aliased,
and that is the defect that made the converted `go/types` judge a type parameter not identical to itself.
Promoted-embed structs still construct through a generated constructor when the embedded type needs one of
its own (a fixed-size array at some depth). Cross-package embeds resolve through the compiled type's metadata, and pointer-receiver methods
promoted through a value embed are routed at the call site (`t.of(timeTimer.Ꮡtimer).modify(…)`) so writes
land on the real storage. Such a method is *also* emitted as a `ж<T>`-receiver extension on the outer type,
because that emitted set is what golib reads back at run time as the type's Go **method set** — so a
promotion the generator skips is not a missing shortcut but a Go method the type is then judged not to have,
and every duck-typed assertion against it quietly misses. A field promoted through a **pointer** embed takes the hop before its reference is
built, so its address is rooted where Go roots it — `f.pfd` for `type File struct{ *file }` *is* `f.file.pfd`,
one address whichever spelling reaches it:

```csharp
// os/File — the generated promoted field reference
internal static ж<FD> Ꮡpfd(ref File instance) => instance.@file.of(global::go.os_package.file.Ꮡpfd);
```

**Full detail:** [Reference → Struct Type Embedding](ConversionStrategies-Reference.md#struct-type-embedding) —
transitive/pointer promotion, the inline-field copy rule, zero-value construction, cross-package (metadata)
embeds, pointer-embed field identity, interface-adapter projection through embeds, and box-receiver primaries.

---

## Interfaces

Go interfaces are duck-typed. The converter emits each user interface as a `[GoType] partial interface`, and
the **`ImplementGenerator`** discovers which concrete types satisfy it and emits the implementing glue plus
implicit conversions — so assigning a concrete value to an interface variable is direct, no reflection:

```go
type Color interface {                // image/color/color.go
    RGBA() (r, g, b, a uint32)
}
type RGBA struct { R, G, B, A uint8 }
func (c RGBA) RGBA() (r, g, b, a uint32) { /* … */ return }
```
```csharp
[GoType] partial interface Color {    // image/color/color.cs
    (uint32 r, uint32 g, uint32 b, uint32 a) RGBA();
}
[GoType] partial struct ΔRGBA {       // Δ-renamed: the struct name collides with its RGBA() method
    public uint8 R, G, B, A;
}
public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ΔRGBA c) { /* … */ return (r, g, b, a); }
```

Each "concrete implements interface" pairing is recorded as `[assembly: GoImplement<ΔRGBA, Color>]` for the
generator to consume. The well-known built-ins (`error`, `fmt.Stringer`, …) are hand-written in golib but
implemented the same duck-typed way. A cross-package satisfaction is witnessed by the idiomatic
`var _ I = T{}` assertion in the type's own package.

A record is **exported**, so an importer reads it back and skips work it does not need. `image/png` writing
`d.palette[i] = color.RGBA{…}` emits exactly that — the bare struct into the `color.Color` slot — because
image/color's assembly already carries `ΔRGBA : Color`. Only when the declaring assembly *cannot* realize the
pair as a partial struct (a named FUNC type, which is a C# delegate — `net/http`'s `HandlerFunc`) does the
importer wrap the value in its own `<pkg>_<T>ᴠ<Iface>` adapter class.

A **pointer**-sourced record (`(Pointer = true)`) answers the same question for `*T`, and its answer is
simpler: that record *is* the declaring assembly's public `<T>ж<Iface>` adapter class, so the importer
references it rather than minting a local one. `text/template`'s `s.walk(value, t.Root)` emits
`new parse.ListNodeжNode(t.Root)` — reaching into `text/template/parse` — not a second adapter of its own:

```csharp
state.walk(value, new parse.ListNodeжNode(t.Root));   // text/template/exec.cs
```

Both halves of that decision — the one key spelling every record and cast site share, and the
partial-struct trust rule the VALUE form additionally needs — are in
[Reference → A foreign implement record is keyed in ONE spelling](ConversionStrategies-Reference.md#a-foreign-implement-record-is-keyed-in-one-spelling-and-a-value-one-is-trusted-only-for-a-partial-struct).

Beyond one bounded exception, a record is only written for a conversion the source **declares** — an
assignment, a call argument, a `var _ I = T{}` witness. It is not inferred in general, because a
compile-time inference cannot be complete: a dynamic type may live in a package converted **after** the
interface's own (io/fs is converted before os, so nothing in `fs` could record `os.dirFS`) — and an
interface *literal* (`x.(interface{ Len() int })`) can never be recorded at all. The exception is the case
where inference IS complete: when a type and an EXPORTED interface are declared in the **same** package,
both are in hand as that package converts, so the pairs it satisfies are recorded even with no cast
anywhere — `encoding/binary`'s `var BigEndian bigEndian` carries no `var _ ByteOrder = BigEndian`, and
without the record every consumer minted a `binary_bigEndianᴠByteOrder` wrapper that became a second
identity for the value (89 constructions across the stdlib). The **pointer** method set is recorded on the
same reasoning, with one extra gate: a `(Pointer = true)` record is consumed by NAMING the generated
adapter class, and that class is `public` only when both the type and the interface are exported, so an
unexported participant is excluded. Named FUNC types and generics are excluded from both forms — see
[Reference → A package records the pairs it SATISFIES](ConversionStrategies-Reference.md#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses). Structural satisfaction is resolved at RUN TIME instead: `TypeGenerator` emits **two runtime duck-typing
shells** beside every non-generic, non-constraint, non-empty interface — named or anonymous alike — found
through a `[GoInterfaceShell]` stamp: a delegate-bound generic shell for a pointer-sourced value (`ж<X>`) and
a reflective `object`-held shell for a value-sourced one (`os.dirFS`, a `[GoType("@string")]` struct). golib's
`AdapterBinder` picks the tier, owns all binding, and is **fail-soft** — a pair it cannot build MISSES, exactly
as Go answers. A declared record still wins first, as the ~1.1 ns nominal fast path; the shells answer
everything else. This is the ONLY duck-typing surface a converted interface has, and the only one it needs:
there are no per-interface conversion methods to reach reflectively (which Native AOT could not close) and no
converter-side structural guessing at named-interface pairs:

```csharp
[global::go.GoInterfaceShell(typeof(ΔSpeaker<>), typeof(ΔSpeakerᴛObj), "Speak")]
public partial interface Speaker { }
```

**Full detail:** [Reference → Interfaces](ConversionStrategies-Reference.md#interfaces) — a large topic:
the runtime shells and their AOT tiering, cross-package pointer/value adapters, unexported-sealing markers,
keyword-named method escaping, publicized unexported types, structural (C# inheritance) satisfaction, and
adapter accessibility.

---

## Pointers

Pointer conversions use the golib heap box **`ж<T>`** (read "zhe"). Taking an address uses **`Ꮡ`**; an
escaping local is allocated with `heap(...)`; a field/element address goes through `.of(Type.ᏑField)` /
`.at<T>(i)` / `Ꮡ(slice, i)`. A pointer parameter is deref-aliased with `ref var x = ref Ꮡx.Value`, and
writes through a pointer field use `.Value`:

```go
func (l *List) insert(e, at *Element) *Element {   // container/list/list.go
    e.prev = at
    e.next = at.next
    e.prev.next = e
    e.next.prev = e
    e.list = l
    l.len++
    return e
}
```
```csharp
internal static ж<Element> insert(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡat) {  // list.cs
    ref var l = ref Ꮡl.Value;
    ref var e = ref Ꮡe.Value;
    ref var at = ref Ꮡat.Value;
    e.prev = Ꮡat;
    e.next = at.next;
    e.prev.Value.next = Ꮡe;      // write through the pointer field
    e.next.Value.prev = Ꮡe;
    e.list = Ꮡl;
    l.len++;
    return Ꮡe;
}
```

The box's `Value` is the strict (nil-panicking) dereference; `ValueSlot` is its no-check twin; a
package-level global whose address is taken is backed by a real box so `&global` writes are observed. Using
`ж<T>` rather than C# `ref` sidesteps escape-analysis complications, at the cost of an occasional heap
allocation.

**Where no escape exists, the box does not either — the ref-lowering.** An *unexported package-level
function's* pointer parameter whose every use is a dereference (or a forward into another such position)
emits as a C# **`ref T` parameter**, and every call site passes a `ref` expression instead of minting a
box: `ref` reads as Go's `&`, the signature reads as Go's `*T`, and an address-taken local whose address
only feeds such positions reverts to a plain stack local (no `heap()` box, no pinnable slot). Any use the
classifier does not positively recognize — identity/nilness, escapes, `unsafe`, method calls on the
pointer, re-points, exported/func-value/linkname/hand-owned functions — keeps the boxed convention, and
`defer f(&x)` / `go f(&x)` sites stay boxed with the thunk deriving the ref at invoke time. A nil base at
a lowered field address panics eagerly via golib's zero-allocation `nonnil` (Go's timing); a nil pointer
argument still enters the callee and faults at first use:

```go
func p224Sub(out1, arg1, arg2 *p224MontgomeryDomainFieldElement)   // nistec/fiat/p224_fiat64.go
p224Sub(&e.x, &t1.x, &t2.x)
```
```csharp
internal static void p224Sub(ref p224MontgomeryDomainFieldElement out1, ref p224MontgomeryDomainFieldElement arg1, ref p224MontgomeryDomainFieldElement arg2)
p224Sub(ref nonnil(ref e).x, ref nonnil(ref t1).x, ref nonnil(ref t2).x);
```

Detail (the classification whitelist, the seven call-site emission rows and their boxed fallbacks, the
hoisted-temp rule for wrapper reinterprets, the locals reversion, the nil doctrine and the defer/go
carve-out): [A pointer parameter whose every use is a dereference is a `ref`
parameter](ConversionStrategies-Reference.md#a-pointer-parameter-whose-every-use-is-a-dereference-is-a-ref-parameter--the-ж-box-ref-lowering).

An **ENTRY alias** — the `ref` a pointer RECEIVER or pointer PARAMETER binds on the way in — must not use
`Value`. Go permits calling a method through a nil `*T`, and equally permits *passing* one: the body RUNS,
and the panic happens only where it dereferences the pointee. That is why `os`'s fifteen nil-tolerant
`*File` methods return `ErrInvalid` instead of panicking, and why `internal/concurrent`'s
`newIndirectNode(nil)` — which merely stores its argument — is not an error at all. So every entry alias
uses `DerefOrNull()`, which binds a *null ref* for a nil box: legal to hold, and it faults on first use,
so the panic is deferred to Go's own point rather than raised at entry (or, as a shared `default(T)`
slot would, lost entirely):

```go
func (f *File) Chdir() error {                  // os/file_posix.go
    if err := f.checkValid("chdir"); err != nil { return err }
    ...
}
```
```csharp
public static error Chdir(this ж<File> Ꮡf) {    // file.cs
    ref var f = ref Ꮡf.DerefOrNull();           // binds; a nil receiver does NOT throw here
    { var err = Ꮡf.checkValid(chdirˢ); if (err != default!) { return err; } }
    ...
}
```

A pointer PARAMETER binds identically — `ref var parent = ref Ꮡparent.DerefOrNull();` — and so does the
re-alias after a pointer is re-pointed, since a repoint is not a dereference either. The fault surfaces as
Go's own `runtime error: invalid memory address or nil pointer dereference`, and is recoverable. Detail
(both emission sites — the converter preamble and go2cs-gen's `ref`-receiver bridge — and the three
retired body analyses that used to admit nil parameters one shape at a time):
[A pointer RECEIVER's deref alias is nil-DEFERRING](ConversionStrategies-Reference.md#a-pointer-receivers-deref-alias-is-nil-deferring--the-panic-moves-to-the-body-it-does-not-vanish)
and [A pointer PARAMETER is nil-deferring for exactly the reason a receiver is](ConversionStrategies-Reference.md#a-pointer-parameter-is-nil-deferring-for-exactly-the-reason-a-receiver-is).

Pointer **equality is by address**, so `ж<T>.Equals` compares each referent shape by its real storage, not
by the box: a struct-field ref by (source object, field identity), and an element ref by (backing array,
absolute index). That last canonicalization is load-bearing — `Ꮡ(slice, i)` boxes the slice HEADER anew on
every call, so comparing the boxes made `&s[0] == &s[0]` *false*, and hashing them put two aliasing element
pointers in different `map[*T]` buckets. Reducing to the backing array plus `Low + index` makes every Go
alias of one element equal: `&s[1:][0] == &s[1]`, an in-capacity `append` result, and `&a[:][i] == &a[i]`.

Identity and nilness are **structural** — properties of the storage, never of the value stored there. A
standard heap box *is* its storage, so it compares and hashes by its own identity (two boxes over one
referent are two addresses, as `&c == &d` is false in Go), while two boxes aliasing the same native address
are one pointer. `IsNilPointer` answers "is this THE nil pointer" and drives every identity question; the
value-peeking `IsNull` survives only where reading the slot is the actual question, because a real address
whose pointee is nil (`&i` with `i == nil`) and a field/element reference box are both perfectly good
addresses.

The same reasoning answers **lifetime**: because the box is an expression temporary, anything asking
"when does this object die?" or "is this the same object?" asks the *referent*, exposed as
`ReferentObject` — the backing storage for an element ref, the **root** allocation for a (possibly
nested) field ref. `runtime.SetFinalizer(&buf[0], f)` finalizes `buf`'s allocation, as in Go, rather
than the throwaway `ж<byte>` the argument expression allocated; and `sync.Cond`'s copy detector, whose
Go implementation stores its own *address* (unsound on a moving collector), compares root-allocation
identity instead.

A pointer **reinterpret** — `(*U)(p)` between two types that share an underlying — names `p`'s own storage
in Go, so a write through the derived pointer is visible through `p`. The converter emits golib's aliasing
`p.Reinterpret<T, U>()` rather than boxing a converted copy. `flag`'s `newBoolValue` returning
`(*boolValue)(p)` is the shape that makes the difference visible: under the copy form a parsed flag never
reached the caller's variable.
And it answers **address stability**. Go's collector never moves a heap object, so
`uintptr(unsafe.Pointer(&x))` names an address that stays valid while native code uses it; the CLR's
does move them, so go2cs **pins** the root storage whenever a pointer's address is taken and holds it
for that pointer's lifetime — a heap box pins its own value slot, an element ref the backing array, a
field ref the allocation containing the field. That is also why a standard heap box keeps an unmanaged
pointee's value in a one-element array rather than in a field of the box: a class carrying references
cannot be pinned at all, so the value needs somewhere pinnable to live. Without this, every address
handed to a syscall was a *former* address — a collection during a blocking `ReadFile` moved the
byte-count box out from under the kernel, the count stayed zero, and `internal/poll` reported that as
a premature `io.EOF`.

**Full detail:** [Reference → Pointers](ConversionStrategies-Reference.md#pointers) — per-iteration
range-variable boxes, wide-index narrowing on element addresses, element/`unsafe.StringData` pointer
identity, pointer-typed globals & double-pointer walks, closure capture of boxed locals, `unsafe.Pointer`
conversions, and reinterpret casts.

---

## Implicit Pointer Dereferencing

Go auto-dereferences pointers on field access and method calls. The converter binds a `ref` local to the
box's value for a pointer parameter, so the body reads like Go:

```go
func PrintValPtr(ptr *int) {
    fmt.Printf("Value available at *ptr = %d\n", *ptr)
    *ptr++
}
```
```csharp
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.Value;
    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

A pointer *local* dereferences through its box on access — a read as `(~x).field`, a write as
`x.Value.field = …` (the assignable form). Promoted fields, nested LHS chains, `++`/`--`, and indexed
targets all thread the same assignment context so the write path stays assignable.

**Full detail:** [Reference → Implicit Pointer Dereferencing](ConversionStrategies-Reference.md#implicit-pointer-dereferencing) —
selector-base deref detection, nested LHS `.Value` chains, index-expression assignment targets, and
`*p.field` field-deref through parameters/receivers.

---

## The `go.golib` support namespace

golib's hand-written support types (`SparseArray<T>`, `PinnedBuffer`, `HashCode`, …) live in the
**`go.golib`** child namespace — deliberately *not* `go.<any Go package name>`, because a child namespace
visible from every referenced assembly would win simple-name lookup over an import alias (`go.runtime` would
shadow `using runtime = runtime_package;`, CS0576). The general form of that collision — a real
parent/child package pair — is handled by **Δ-renaming the import alias** (`using Δruntime = …`).

**Full detail:** [Reference → The go.golib support namespace](ConversionStrategies-Reference.md#the-gogolib-support-namespace) —
the collision reasoning and the transitive-closure alias-rename pre-pass (incl. foreign renamed-type alias
resolution).

---

## Source Generators

Several Go semantics can't be written directly in C#, so the converter emits compact attributed partial
declarations and lets Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every
converted project) synthesize the rest at compile time — keeping the visible code close to Go. The
principal generators:

- **`TypeGenerator`** (`[GoType]`) — struct members & equality; named numeric/slice/array/map/channel
  wrappers & operators; struct-embedding promotion.
- **`ImplementGenerator`** — finds concrete types satisfying each `[GoType] partial interface` and emits the
  implementation glue + implicit conversions.
- **`RecvGenerator`** (`[GoRecv]`) — emits the pointer/box (`ж<T>`) overload of each value-receiver method.
- **`ImplicitConvGenerator`** — the implicit operators letting a named type and its underlying interconvert.
- **`PartialStubGenerator`** — a throwing stub for any bodyless partial (asm/cgo) with no real
  implementation. (For cgo specifically the stubs are an interim state, not a dead end: the
  ratified [cgo interop plan](PLAN-cgo-interop.md) maps the `import "C"` ladder that replaces
  them with real P/Invoke-backed bindings.)

Common attributes: `[GoType]`, `[GoRecv]`, `[GoTag]`, `[GoPackage]`, and the test-only
`[GoTestMatchingConsoleOutput]`.

An inline `[GoType]` declaration is deliberately **bare** so it reads like the Go original — but a C# nested
type with no modifier is *private*, and a generator can't see the partial it is about to emit. So
`package_info.cs` carries a **`TypeAccessibility`** section inside the package class that pins each type's
real accessibility in source, one condensed line per type, ahead of generation:

```csharp
[GoType] partial interface Closer {          // io/io.cs — Go-shaped, no modifier
    error Close();
}
```
```csharp
public static partial class io_package {     // io/package_info.cs
    // <TypeAccessibility>
    internal partial struct discard {}
    public partial interface Closer {}
    // </TypeAccessibility>
}
```

**Full detail:** [Reference → Source Generators](ConversionStrategies-Reference.md#source-generators).

---

## Manually-Converted Declarations

A few Go declarations can't be faithfully auto-converted: their semantics depend on constructs the CLR
doesn't have — a managed pointer hidden inside an integer (runtime's `guintptr`/`puintptr`/`muintptr`, a
`uintptr` holding a `*g` the Go GC must not see), a two-word interface layout walked through
`unsafe.Pointer` (`reflect`), or a Go-runtime primitive with no managed equivalent (a scheduler
continuation, a sleeping semaphore). The rule: **managed reality beats raw reinterpretation** — hold the
`ж<T>` box or `any` directly instead of round-tripping through a `uintptr`/`unsafe.Pointer` the .NET GC
can't see, and reimplement the observable *contract* rather than the unportable *mechanism*.

Two mechanisms deliver it. Whole-file **`[module: GoManualConversion]`** makes the converter skip
emission for that file and redirect it to a non-compiled `<name>.cs.auto` review sibling, so a reconvert
can never touch the hand-owned `.cs`. A **type-level registry** instead skips only the listed
types/methods and points at a hand-written `*_impl.cs` companion beside the rest of the auto-converted
file.

`runtime.Gosched` is the smallest worked example. Go's body is a scheduler continuation that needs
`mcall`, a compiler intrinsic with no CLR equivalent, so `runtime/managed_impl.cs` implements the
*contract* — "yield the current thread" — instead of the mechanism:

```go
// Go — runtime/proc.go: the body is a scheduler continuation run on the system stack
func Gosched() { checkTimeouts(); mcall(gosched_m) }
```
```csharp
// C# — runtime/managed_impl.cs: the CONTRACT, on the managed scheduler
public static void Gosched() { Thread.Yield(); }
```

The same rule hand-owns `sync/atomic.Value` (stores the boxed `any` directly, `Volatile`/`Interlocked`
for the atomics), the reflection bridge (`reflect`/`internal/reflectlite` carry a boxed managed value
plus a synthetic descriptor stamped with the real `System.Type`), `sync.Pool`'s eface ring (a single
`any?` slot with `null` as the empty sentinel), `sync.Cond`'s copy detector (compares root-allocation
identity instead of a GC-unsound stored address),
[`internal/weak.Pointer`](ConversionStrategies-Reference.md#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one)
(a short `WeakReference` over the `ж<T>` box, with a `ConditionalWeakTable` standing in for the runtime's
canonical per-address weak handle so two weak pointers to one object still compare equal), `time`'s runtime timers (one dedicated thread servicing
a deadline-ordered heap on the Windows high-resolution timer), and the runtime's whole process-control
surface (`GC`, `GOMAXPROCS`, `Gosched`, `LockOSThread`, `Goexit`) as its contracts rather than its
scheduler-level mechanics. The GC **measurement** surface belongs to the same family and shows what
"realize, don't stub" costs and buys: `runtime.ReadMemStats` and `runtime/debug.ReadGCStats` now read
one snapshot from
[one gen2 pause recorder](ConversionStrategies-Reference.md#the-gc-measurement-surface--one-recorder-one-ring-one-snapshot),
so the per-cycle pause history, `LastGC`, `NumGC` and `HeapReleased` are measured facts rather than
zeros — while `Mallocs`/`Frees`/`HeapObjects` and `GCCPUFraction` stay zero, because the CLR's nearest
quantity means something else and a plausible-looking invented number is worse than a stated gap. The
same "realize, don't stub" instinct also ports an asm-backed architecture
layer for real wherever .NET exposes the same instructions the `.s` file issues — `hash/crc32`'s SSE4.2
`CRC32` and PCLMULQDQ folding.

**Asynchronous sockets are the deepest application of that rule, and it takes two hand-owns facing each
other.**

**Reinterpreting one array as an array of a different element type is the smallest member of the same
family, and it needs no OS at all.** `crypto/subtle` views a `[]byte` as `[]uintptr` to XOR a word at a
time; `golang.org/x/crypto/sha3` views its `[25]uint64` sponge state as `[200]byte` to absorb and
squeeze. Both are ordinary managed storage on both sides, and neither view exists in the managed model —
a `slice<T>`/`array<T>` is a window on a real `T[]`, and there is no `U[]` view over a `V[]`. Each file
is hand-owned and takes the same remedy: `MemoryMarshal.Cast`/`AsBytes` over the storage's own span,
which is a genuine *aliasing* view, so writes through it land. Where such a reinterpret is left
auto-converted the pointer is still a valid **address**, but dereferencing it reads the surrogate's
backing *reference* out of the pointed-at data — a fabricated reference whose first use is an access
violation. Those sites are the deliberate raw-metal fork: they compile, they are not expected to
produce Go's values, and each is hand-owned only when a suite reaches it.

**A Go pointer VARIABLE's address is the one thing the boundary cannot hand over, and that is a second
reason a syscall wrapper gets hand-owned.** The family above is about *layout* — a struct whose fields
sit at the wrong offsets. This one has no struct in it at all. A `**T` out-parameter (`&p` for a
`var p *T`) is, in Go, eight bytes of stack the kernel overwrites with an address; converted, it is a
`ж<ж<T>>` whose storage is a managed *object reference*, and golib's `ж<T>` → `uintptr` operator has
two answers for it, both wrong: `0` while the held pointer is still null — which tells Windows "no
output wanted", so the call succeeds and the caller reads back its own nil — and a live managed
address once it is not, which would have the kernel write raw bytes over a slot the collector reads as
a reference. Neither is fixable in the operator, because no single address is both kernel-writable as
eight raw bytes and managed-readable as a `ж<T>`; reconciling the two needs a *sync point*, and the
only code that knows when the raw word becomes a pointer again is the wrapper. So the remedy is a
native cell local to the call and a publish afterwards through `ValueSlot` — never `Value`, whose nil
guard would panic on the very write that fills the slot in. Same rule as the layout family for scope:
[fixed when a suite reaches it](ConversionStrategies-Reference.md#pointers), and verified at *value*
level, because the failure shape here is a quiet wrong answer rather than a crash.

Go's network poller is only half an API — the other half is called by the *scheduler*, from
`findrunnable` and sysmon — so wiring the converted runtime would initialize an IOCP and then block
forever. Instead, `internal/poll`'s ten `//go:linkname runtime_poll*` contracts are reimplemented on the
CLR's own completion machinery, and the overlapped WSA wrappers below them are displaced so each
operation owns a NATIVE control block for as long as the kernel holds it: `&o.o` is an interior field
inside a reference-bearing struct, so golib cannot pin it, and the OVERLAPPED is also the operation's
kernel-side identity (`CancelIoEx` matches by address). The two halves live in packages that cannot
reference each other, so the completion signal is pushed through a small platform-neutral rendezvous in
golib keyed by the descriptor — the one identity both sides independently hold. Everything above the
seam stays auto-converted, `execIO` and `FD` included:

```go
// Go — internal/poll/fd_poll_runtime.go: ten bodyless entry points into the runtime's poller
func runtime_pollWait(ctx uintptr, mode int) int
```
```csharp
// C# — internal/poll/windows/runtime_netpoll_impl.cs: the CONTRACT, on a Monitor and a Timer
internal static partial nint runtime_pollWait(uintptr ctx, nint mode)
{
    ManagedPollDesc? desc = descFor(ctx);

    if (desc is null)
        return pollErrClosing;

    return pollBlock(desc, modeState(desc, mode), ignoreErrors: false);
}
```

[Full detail](ConversionStrategies-Reference.md#the-managed-netpoller--the-ten-runtime_poll-contracts-on-nets-completion-machinery),
including [the submit seam's operation records, the golib rendezvous and the accept
handover](ConversionStrategies-Reference.md#the-overlapped-submit-seam--a-per-operation-record-owning-native-lifetime-and-a-golib-rendezvous).

**The Linux flavor answers the same ten contracts on epoll — and got there in two steps.** Linux's
`os` marks every opened file, pipe and socket pollable and asks the poller to arm it, so the file
surface had to work before anything else could. Step one made
`internal/poll/linux/runtime_netpoll_impl.cs` answer Go's own FALLBACK for every descriptor —
`pollOpen` returns `(0, EPERM)`, exactly what `epoll_ctl` says about a regular file, and `os.newFile`
drops back to blocking mode and carries on. Step two replaced that constant with the kernel: one
`epoll_create1(EPOLL_CLOEXEC)`, one background thread in `epoll_wait(-1)`, and the Windows flavor's
managed descriptor state machine copied verbatim, with `gopark`/`goready` becoming
`Monitor.Wait`/`PulseAll` on a per-descriptor gate. Registration is Go's edge-triggered
`EPOLLIN|EPOLLOUT|EPOLLRDHUP|EPOLLET`, which is sound because the CONSUMER only ever waits after the
kernel answered `EAGAIN`; `epoll_event.data` carries an opaque token rather than a pointer, so a
reused descriptor number cannot resurrect a retired one; and the 12-byte packed kernel record is a
native `Marshal` image through the keystone `syscall(2)` binding, never a `ж<T>` address. Files still
take the blocking path — now because the kernel refuses them, which is precisely Go's behavior — while
pipes, FIFOs, ttys and sockets are armed: deadlines are honored, `Close` unblocks a parked reader, and
`net.Listen`/`Dial` work.
[Full detail](ConversionStrategies-Reference.md#the-linux-flavors-poller--the-fallback-first-then-the-readiness-poller-epoll-one-drain-thread-and-the-windows-descriptor-state-machine).

**And the struct-passing class has Linux members too.** The converted `syscall.Stat_t` ends in a golib
`array<int64>` where the kernel's `struct stat` ends in three inline words, so it is not blittable and
the generated `Fstat`/`fstatat` handed the kernel its managed image — `os.Stat` on the Linux flavor
answered `IsDir() == false` for a real directory with a nil error, and every Glob/Walk built on it walked
nothing. The remedy is the same mirror-and-copy the Windows wrappers use
(`syscall/linux/zsyscall_linux_amd64_impl.cs`, displaced from the generated file under a linux-only
registry scope), and the same measurement found `rawSyscallNoError` — the bare-`SYSCALL` bottom of
`Getpid`/`Getuid`/… — still an announcing stub, now one body in `syscall_linux_impl.cs`.
[Full detail](ConversionStrategies-Reference.md#the-linux-struct-stat-mirror-and-the-noerror-raw-bottom--the-first-linux-members-of-the-struct-passing-class).

**The sockaddr family is mirrored on Linux too — as the socket poller's prerequisite.** The same
port alias and the same by-address `RawSockaddr*` structs that L10 retired on Windows live in
`syscall_linux.go`; `syscall/linux/sockaddr_linux_impl.cs` is L10 arm for arm (stack mirrors, one
encode and one decode, the generated address-taking `bind`/`connect` reused), under a shared
windows+linux registry scope. What it bought was honest and, at the time, small: a Linux socket was
still un-armable, so `net.Listen`/`Dial` merely reached `FD.Init` and returned `operation not
permitted` — the wall moved rather than fell, and the readiness poller above is what finished it. And `syscall.Mmap` on Linux returns a SNAPSHOT, because
golib's `unsafe.Slice` over a native pointer copies rather than aliases — a slice-model item, rooted
and routed.
[Full detail](ConversionStrategies-Reference.md#the-sockaddr-family-on-linux--l10s-mirror-arm-for-arm-as-the-socket-pollers-prerequisite-and-mmaps-slice-is-a-snapshot).

**On Linux the whole kernel boundary is one hand-own.** Go funnels every syscall through a single
assembly function, `internal/runtime/syscall.Syscall6`, so the managed corpus needs exactly one native
binding — glibc's `syscall(2)` — and the entire generated wrapper surface (open, read, write, stat,
getrlimit, the epoll helpers) lights up behind it:

```go
// Go — internal/runtime/syscall/syscall_linux.go: no body, no linkname, raw metal
func Syscall6(num, a1, a2, a3, a4, a5, a6 uintptr) (r1, r2, errno uintptr)
```
```csharp
// C# — internal/runtime/syscall/linux/syscall_linux_impl.cs
[LibraryImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static partial nint libc_syscall(nint number, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6);
```

Every native binding in the corpus is `[LibraryImport]` rather than `[DllImport]`, on both operating systems, for one reason: `[DllImport]` answers a signature it cannot marshal by silently marshalling a COPY, so a kernel writing through the pointer writes into a temporary the caller never reads — a wrong answer at run time. The source generator makes that a compile error instead, which turns the per-struct **layout** risk of routing Go's kernel boundary through managed structs into a build-time question. It costs `/unsafe` unconditionally (SYSLIB1062), and since the `.csproj` is regenerated on every transpile, a hand-owned file states that requirement itself with `[module: go.GoRequiresUnsafe]`, which the emission unions into `<AllowUnsafeBlocks>`.

The pointer half needs nothing extra: these wrappers pass addresses as `uintptr`, and golib's `ж<T>` →
`uintptr` operator pins the managed storage and yields a real address rather than a token, so the kernel
reads and writes through it. Go's second result `r2` is reproduced *exactly* rather than approximated —
the x86-64 syscall convention clobbers only `RCX`/`R11`, so the `RDX` the assembly reports is the `a3`
that went in. That, the SysV variadic question, and `errno` were each measured rather than assumed, and
the one case libc cannot distinguish is disclosed in the file.
[Full detail](ConversionStrategies-Reference.md#the-linux-syscall-bottom--one-libc-pinvoke-and-why-r2-is-exact-rather-than-approximate),
including the [scheduler brackets that are a faithful no-op](ConversionStrategies-Reference.md#the-scheduler-brackets-are-a-faithful-no-op-not-an-omission)
and [why `runtime.argslice` must be populated in the same change that forwards it](ConversionStrategies-Reference.md#runtimeargslice--forwarding-and-populating-are-one-change).
`os/signal`'s six runtime primitives are the sharpest case of that same rule: forwarding them needed an OS
edge (a real `SetConsoleCtrlHandler` feeding the *converted* `ctrlHandler`) and a genuinely blocking
`notetsleepg` before the pushed bodies could run at all — after which Go's own Windows semantics fall out
unaltered, including the ones that read like defects (`Ignore` does **not** suppress ^C on Windows, and
`Reset` leaves the ignored bit set).

**Full detail:** [Reference → Manually-Converted Declarations](ConversionStrategies-Reference.md#manually-converted-declarations) —
every hand-owned surface in full: the guintptr family, `sync/atomic.Value`, the reflection bridge,
whitelisted `//go:linkname` forwarders in both directions (a
[PULL](ConversionStrategies-Reference.md#a-cross-package-golinkname-pull-emits-a-forwarder-not-a-throwing-stub)
binds another package's symbol; a
[PUSH](ConversionStrategies-Reference.md#a-cross-package-golinkname-push-resolves-per-recorded-disposition--forwarder-or-announced-panic)
takes another package's body, or announces the pair it cannot honor),
[realizing an asm-backed arch layer with managed hardware intrinsics](ConversionStrategies-Reference.md#realizing-an-asm-backed-arch-layer-with-managed-hardware-intrinsics),
[realizing the runtime timer contract](ConversionStrategies-Reference.md#realizing-the-runtime-timer-contract-sleep--newtimer--stoptimer--resettimer),
[the runtime's process-control surface](ConversionStrategies-Reference.md#the-runtimes-process-control-surface-implement-the-contract-never-the-mechanism), and
[`sync.Pool`'s managed ring slot and thread-affine shard index](ConversionStrategies-Reference.md#syncpool--a-managed-reference-ring-slot-and-a-thread-affine-stand-in-for-the-p-pin).

---

## The standard library reproduces Go `-tags purego`

The converted standard library corpus reproduces **Go built with `-tags purego`**, not the default
`amd64`/`arm64` build. Go implements hot crypto/hash functions in `.s` assembly the transpiler cannot
convert (the Go file has only a bodyless declaration gated `… && !purego`), so a default build turns
them into throwing stubs that *compile* but can't *run*; `purego` selects the portable pure-Go
variants with real bodies. `-stdlib` and `-tests` apply `-tags purego` **by default** (an explicit
`-tags` replaces it, `-tags=` clears it) and print the effective tags at the start of each run —
a `-tests` run reconverts the package's production sources, so it must reproduce the same emission.
Every other conversion is tag-neutral. `purego` is a *convention*, not a language rule, so the default
set carries every portable-fallback tag the stdlib actually uses: `math/big` predates `purego` and
spells its own `math_big_pure_go`, gating `arith_decl_pure.go` (real pure-Go forwarders) against
`arith_decl.go`'s eight bodyless `arith_$GOARCH.s` declarations — without it every `big.Int`/`Float`/`Rat`
arithmetic path compiled clean and threw on first use. Asm-backed declarations split three ways: **purego-gated**
(the tag gives a real body — the common case, `crypto/sha256` et al.),
**GOARCH-gated with no purego escape** (hand-owned, e.g.
`internal/chacha8rand` and `hash/crc32` — whose `crc32_amd64.go` carries no build line at all, so
purego selects it too), and **genuinely raw-metal** (`[module: GoManualConversion]` compiling stub).
Hand-owning the second bucket need not mean stubbing: where .NET exposes the same instructions the
`.s` file issues, the arch layer can be ported for real — `hash/crc32` runs on `Sse42.Crc32` and
`Pclmulqdq` intrinsics. One accepted behavioral divergence from the default build: under purego,
`crypto/elliptic` P256 `Inverse` panics — **exactly as real Go does under `-tags purego`** (an upstream
gating inconsistency), so matching it is fidelity.

**Full detail:** [Reference → The standard-library conversion applies `-tags purego`](ConversionStrategies-Reference.md#the-standard-library-conversion-applies--tags-purego) —
the exposure decision and rejected alternatives, the three-bucket taxonomy, and the verified
`crypto/elliptic` divergence.

---

## Deterministic Output

Converter output is **byte-reproducible**: the same Go source with the same converter build produces
byte-identical C# every run — a guarantee the [goldens](Glossary.md#golden), the corpus build gate, and any
release tag all rest on. It's enforced by converting files sequentially in sorted-filename order, a
deterministic dependency-complete stdlib queue, and sorted emission of any set-backed output.

**Full detail:** [Reference → Deterministic Output](ConversionStrategies-Reference.md#deterministic-output).

---

*This summary tracks the [technical reference](ConversionStrategies-Reference.md) — when a conversion
decision changes the headline mapping of a construct, update the matching section here (with a real
example); record the full detail in the reference. See [`../CLAUDE.md`](../CLAUDE.md), "Record the
conversion decision."*
