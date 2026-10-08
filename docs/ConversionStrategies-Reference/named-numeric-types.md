# Named Numeric Types and Constant Contexts
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#named-numeric-types-and-constant-contexts)

General untyped constant representation is covered in [Constant Values](constants.md#constant-values). This section
records the places where constants and operators become difficult because a numeric context is already
known: named numeric wrappers, native-width targets, typed-element contexts such as `append`, shift and
bit-mask operands, and the casts needed to keep C# overload resolution aligned with Go.

This area is where Go's flexible numeric model meets C#'s stricter one, and it has a few moving parts worth calling out.

**Untyped constants.** As noted under [Constant Values](constants.md#constant-values), an untyped Go constant becomes a golib `UntypedInt`/`UntypedFloat`/`UntypedComplex`. These wrappers define implicit conversions to **and from** every numeric type so the value can slot into whatever context uses it, just like an untyped Go constant. The trade-off is that mixing an `UntypedInt` directly into heavily-typed arithmetic (e.g. `someUint64 * untypedConst`) can become ambiguous to C#'s overload resolution, since the wrapper is convertible in either direction. A *function-local* untyped constant whose every use resolves to one concrete basic type sidesteps the wrapper entirely — it is declared AT that type with the per-use casts dropped (see [Constant Values](constants.md#constant-values), *function-local untyped constants tighten*); the wrapper-cast machinery below applies to the remaining wrapper-emitted constants (package-level, mixed-context, and folding-participating locals).

One resolved instance: an argument to the **`min`/`max` builtins** that is a named untyped constant renders as its `UntypedInt` static, which golib's `min<T>(T, params ReadOnlySpan<T>)` overloads reject (CS1503 — params-span element binding does not apply the user-defined implicit conversion): runtime `min(n, maxObletBytes)` (`mgcmark.go`, `uintptr` sibling) and `min(debug.profstackdepth, maxProfStackDepth)` (`runtime1.go`, `int32`). The converter casts such an argument to the call's Go-resolved result type — `min(n, (uintptr)(maxObletBytes))` — and, once one argument is cast, every constant-valued sibling too (`min(big, limit, 500)` → `…, (uintptr)(limit), (uintptr)(500))` — a bare literal is a C# `int` and would break `T` inference against the cast type). Typed arguments and literal-only calls are unchanged. (Guarded by the `MinMaxBuiltin` extension — untyped consts typed by `uintptr`/`int32` siblings plus the mixed literal case, values vs Go.)

**Named numeric types.** A Go type definition over a numeric base — `type Celsius float64`, `type level int`, `type Flags uint` — is emitted as a partial struct carrying a `num:` `[GoType]` attribute, and the `TypeGenerator` source generator fills in the body:

```csharp
partial struct level /*num:nint*/;   // type level int
partial struct Flags /*num:nuint*/;   // type Flags uint
partial struct Celsius /*num:float64*/; // type Celsius float64
```

The generated struct wraps the underlying value and implements the comparison and arithmetic operators plus implicit conversions to/from the underlying type, so the named type is a distinct C# type that still behaves like its base.

**Increment / decrement.** Go allows `c++` / `c--` on a named integer (e.g. a `for c := chunkIdx(0); …; c++` loop counter). The generator therefore emits `operator ++`/`operator --` returning the **named type** — `operator ++(T value) => (T)(value.m_value + (U)1)` (U the underlying). Without a dedicated operator, C# `c++` falls back to the implicit conversion to the underlying and re-assigns the (promoted) result, which for a **native-int**-backed named type (`num:nuint`/`num:nint`) promotes to `ulong`/`long` and then cannot implicitly convert back to the named type (CS0266). The dedicated operators keep the result in the named type. (Guarded by the `NamedNumericIncDec` behavioral test — `++`/`--` on `uint`- and `int`-backed named types in loop counters; runtime uses this for `chunkIdx`/`arenaIdx`/`statDep` loop counters, ~7 CS0266.)

**Unsigned underlying types and unary minus.** Go permits unary minus on an unsigned value (it wraps: `-x == 0 - x`). C# does **not** allow the unary `-` operator on unsigned operands. So the generator's `IsUnsignedType` check *omits* the unary negation operator for unsigned underlying types (`uint8/16/32/64`, `byte`, `uintptr`, and the native `nuint`/`uint`). Go's unary minus on such a value is instead lowered by the converter to the equivalent subtraction-from-zero form:

```go
var b Flags = 2
_ = -b            // Go: unsigned unary minus (wraps)
```
```csharp
Flags b = 2;
_ = ((Flags)0 - b);   // C#: lowered to (T)0 - x
```

This keeps the generated numeric struct compilable (a `(T)(-value.m_value)` body over `nuint` is a CS0023 error) while preserving Go's wrap-around semantics. The same `(T)0 - x` lowering is used for unsigned unary minus on built-in unsigned values.

**Converting *to* a named numeric type.** The generated struct's implicit conversions are only between the named type and its *exact* underlying basic (`traceArg ↔ uint64`, `arenaIdx ↔ nuint`). So a Go conversion `traceArg(procs)` where `procs` is `int32`, or `arenaIdx(1 << b)` where the shift is `int`, has no matching operator — a plain `(traceArg)procs` is CS0030. The converter coerces the argument through the underlying type first, which is exactly Go's numeric-conversion semantics:

```go
var procs int32 = 5
a := traceArg(procs)   // type traceArg uint64
b := arenaIdx(1 << 4)  // type arenaIdx uint
```
```csharp
int32 procs = 5;
var a = ((traceArg)(uint64)procs);   // through the underlying uint64
var b = ((arenaIdx)(nuint)(1 << 4)); // through the underlying nuint
```

When the argument is *already* the underlying basic (`traceArg(u)` with `u uint64`), the existing single cast already binds, so no extra cast is inserted (no churn). (Guarded by the `NamedNumericConversion` behavioral test; runtime exercises this pervasively for `traceArg`, `arenaIdx`, `traceTime`, the `abi` offset types, etc.)

**Converting *from* a named numeric type.** The mirror direction has the same root: because the wrapper only converts between the named type and its *exact* underlying, a Go conversion *from* a named numeric *to a different basic numeric* — `uint64(nameOff)` where `type NameOff int32`, or `int(idx)` where `type idx uint` — has no matching operator (`(ulong)NameOff` / `(nint)idx` is CS0030). The converter routes it through the named type's underlying basic first — the named→underlying `[GoType]` operator followed by an ordinary numeric C# cast:

```go
var s NameOff = 7  // type NameOff int32
e := uint64(s)     // NameOff -> uint64
var i idx = 9      // type idx uint
f := int(i)        // idx -> int
```
```csharp
NameOff s = 7;
var e = ((uint64)(int32)s);  // through the underlying int32
idx i = 9;
nint f = ((nint)(nuint)i);   // through the underlying nuint
```

When the target basic *is* the named type's exact underlying (`int32(s)` for `NameOff`), the single operator already binds, so no extra cast is inserted (no churn). (Same `NamedNumericConversion` behavioral test; runtime hits this on the `abi` offset types `NameOff`/`TypeOff`/`TextOff` → `uint64`/`uintptr`, `taggedPointer`/`traceTime` → `int64`, etc.)

**Cast parenthesization (visual fidelity).** A Go conversion `T(x)` reads as a function call; the C# cast `(T)x` is the closest equivalent, so the converter keeps it minimally parenthesized to stay close to the source:

* **Basic-target conversions omit the outer wrap.** A conversion whose target is a *basic* C# type — `uint64(a)`, `int(k)`, `float64(b)`, and even `unsafe.Pointer(p)` (go/types models it as a `*types.Basic`) — emits `(uint64)a`, not `((uint64)a)`. The result of a basic-typed cast can never be the receiver of a postfix `.`/`[]`/invocation (Go basic types expose no callable members and the converter emits none on them), and the C# cast operator outranks every binary operator, so the bare form binds correctly in any surrounding context: `f((uint64)a)`, `return (uint64)a;`, `(uint64)a << n`, `(nint)(uint8)k < len(s)`, and `(nint)x.Load() + 5` (which parses as `((nint)(x.Load())) + 5` — postfix `.` binds before the cast). A **named**-type target keeps the defensive outer parens `((Named)x)` — its result *can* be member-accessed (`Named(x).Method()`), which is parent-context-dependent and not decidable at the conversion site. **`string` is the exception among basic types:** its C# representation is the member-accessible golib `@string` struct, so a `string(x)` conversion *is* a valid postfix receiver — the variadic-string spread `string(r)...` → `((@string)(rune)r).ꓸꓸꓸ`, an index `string(b)[i]` → `((@string)b)[i]`, or `len(string(b))`. Dropping the wrap there reparses the postfix against the cast's inner operand (`(@string)(rune)r.ꓸꓸꓸ` binds `.ꓸꓸꓸ` to `r`, CS1061), so a `string` target retains the outer parens like a named type. (`unsafe.Pointer` stays in the no-wrap set: although its C# `@unsafe.Pointer` is a struct, Go exposes no members on `unsafe.Pointer` and the converter never emits a postfix on such a conversion result.)
* **Identity conversions are not double-cast — EXCEPT a plain constant argument.** `arenaIdx(x)` where `x` is already `arenaIdx` is a Go no-op. This arises for an untyped-constant shift that adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types already types as `arenaIdx`, so the inner conversion has already emitted `(arenaIdx)((nuint)1 << bits)`), and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expression in a second `(arenaIdx)` cast just doubles it, so the converted argument is returned as-is. The exception: a plain **constant** argument (`Word(1)`) — go/types types the constant AS the target (identity), but the render is the bare literal, which under a binary operator resolves as `int` and degrades the whole expression (math/big's `mask := Word(1)<<s - 1`, CS0029). The named cast is re-imposed at the conversion site — `((Word)1) << (int)(s)) - 1` — which also made the older `:=`-declaration-only patch in visitAssignStmt redundant (it now sees the cast already present).

**Named-numeric wrappers carry the full INTEGER operator surface.** The generated `[GoType("num:…")]` wrapper defines `+ - * / % ++ --` returning the wrapper; integer underlyings additionally define `~`, the shifts `<< >>` (int count), and the binary bitwise `& | ^` — all returning the WRAPPER type, exactly Go's typing (`Word >> ŝ` IS a `Word`). Without them C# resolved compound expressions through the implicit-to-underlying conversion and the whole expression degraded to the raw numeric (math/big's Word arithmetic, CS0266 ×45). Floats/complex omit the integer-only operators.

**Named SLICE types keep the named type when sliced.** The generated slice wrapper's Range indexer and `Slice()` overloads return the wrapper (`nat[a:b]` IS a `nat` — a fresh wrapper sharing the same backing window), so a method call directly on a slice expression binds the named type's extensions (`u[s:].norm()` bound the raw `slice<Word>` instead, math/big CS1929 ×21). The explicit `ISlice<T>` implementations keep the raw slice type.

A conversion **between two named slice types** sharing an identical underlying (tar's `sparseElem(s[i*24:])`, both `[]byte`) hops through the shared underlying slice — `((sparseElem)(slice<byte>)(…))` — since the wrapper-returning slicing makes the argument the NAMED wrapper and a direct cast would chain two user-defined operators (CS0030). (Guarded by `SortArrayType`'s `Roster(byAge[0:2])`.) The same hop covers the map and array underlyings (net/mail's `textproto.MIMEHeader(h)`, where `Header` and `MIMEHeader` are both written over `map[string][]string`).

**…but NOT when one of the two was written directly over the other.** Go's `Underlying()` resolves through the whole declaration chain, so `type shuffledFS MapFS` — testing/fstest's own suite, over a `MapFS` that is itself `map[string]*MapFile` — passes the shared-underlying test above while being a completely different shape. The wrapper for a non-basic underlying keeps the **NAMED** base (`[GoType("global::go.testing.fstest_package.MapFS")]`, see `visitIdent`/`visitTypeSpec`), so `shuffledFS` declares exactly ONE conversion operator and it targets `MapFS`. Hopping through the raw map therefore *creates* the two-operator chain the hop exists to prevent — `shuffledFS`→`MapFS`→`map`, CS0030 — where the plain cast Go actually wrote binds in one step:

```go
f, err := MapFS(fsys).Open(name)          // fsys is shuffledFS
```
```csharp
var (f, err) = ((global::go.testing.fstest_package.MapFS)fsys).Open(name);
```

The written right-hand side is recovered from `packageTypeSpecRHS` (`writtenUnderlyingOperations.go`), the same per-package pre-pass the named-**numeric** hop already consults for its own version of this exception — with one difference: the numeric exception is gated to a CROSS-package base, because a same-package numeric chain resolves to the basic underlying (`[GoType("num:uintptr")]`, no named-base operator to bind). A composite underlying keeps the named base either way, so the composite exception needs no package gate. Both directions are covered (the arg written over the target, and the target written over the arg), and all three composite arms — map, slice and array. Unrecorded or cross-package declarations miss the lookup and keep the pre-existing route.

Measured on `testing/fstest`, whose whole 7-verdict suite sat behind this one CS0030: it now runs at **6 of 7**, the residual being `TestShuffledFS`'s runtime assertion that the returned `*shuffledFile` satisfies `fs.ReadDirFile` — the pointer-adapter identity class, unrelated. (Guarded by the `DefinedOverNamedComposite` behavioral test: a child package owning the named map/slice/array, defined types over each in both directions plus a same-package one, and the net/mail two-raw-map control in the same program so the hop is proven to still fire.)

**A constant or a basic value converted into a type written over another package's named numeric hops through that base.** logrus' `hooks/slog` declares `type Level logrus.Level` and writes `var _ slog.Leveler = Level(0)`. The wrapper keeps `logrus.Level` in its `[GoType]`, so its operators convert only from that base, and the direct cast `((Level)0)` would need two user-defined conversions (CS0030). The conversion now names the base between the two, one operator per cast:
```csharp
internal static slog.Leveler _ᴛ2ʗ = ((ΔLevel)(logrus.Level)0);
var l = ((Level)(levellib.Level)(uint32)n);   // a non-constant operand: basic, then base, then target
```
`foreignWrittenBase` reads the written base from the same `packageTypeSpecRHS` pre-pass, so the rule applies to a type the converted package declares itself. A typed const declaration already took this shape through its own path (windows registry's `CLASSES_ROOT = Key(syscall.HKEY_CLASSES_ROOT)` emits `unchecked((Key)(syscallꓸHandle)2147483648)`). (Guarded by the `ForeignDefinedConversion` behavioral test: a constant at package level and in an expression, a `uint32` operand, an `int` expression, and a type over `time.Duration`.)

> A defined type over a named COMPOSITE gets an inherited wrapper that does not expose the golib sequence surface, so `len(x)` directly on one is CS0315 against `builtin.len<TSeq>`. That is a separate, pre-existing gap — no corpus site asks for it, and the guard above deliberately measures through the base type instead.

(Guarded by `NamedNumericConversion`, `NamedNumericShiftConv`, `NamedTypeBitwiseConst`, `IotaEnum`, `FuncTypeParam`, and `CrossPkgUser`; the `string`-target exception is guarded by `StringConvPostfix` and `UnsafeOperations`; verified by the full behavioral suite — output comparisons confirm the precedence is unchanged.)

**Generated conversion operators between named numerics of *different* assemblies.** The two paragraphs above are the *converter's inline* casts. Separately, when the converter sees a conversion *between two named numeric types* it records a `[assembly: GoImplicitConv<…>]` and the `ImplicitConvGenerator` emits a user-defined `implicit operator` for it. The emitted body constructs one named type from the other's underlying value: `new Target((ValueType)src.Value)`.

**`ValueType` is a CAST TARGET, and it names the constructed type's BACKING PRIMITIVE (corrected
2026-08-08).** The template applies it to `src.Value` and feeds the result to the constructed type's
constructor, and that constructor takes the primitive — so `ValueType` must be `uint32`, `nint`,
`int64`, not the wrapper. It named the **constructed type itself** until this was rooted, making the
body a round-trip through that type's own conversion operators — `new WaitStatus((WaitStatus)src.Value)`
— which compiles only while a standard EXPLICIT conversion exists between the two primitives, because
a user-defined conversion admits just one standard conversion on its input. syscall's unix flavors are
where it finally bit: `WaitStatus` is backed by `uint32` and `Signal` by `int` (`nint`), and
`uint32`→`nint` is not a standard IMPLICIT conversion (a 32-bit unsigned value does not fit a 32-bit
native int), so its reverse is not a standard explicit one, the operator is not applicable, and the
cast is **CS0030**. Windows declares `WaitStatus` a struct, so the pair is never registered there and
no corpus build reached it. All 49 of the corpus's `ValueType` records carried the constructed type's
own name, so the form was never right — only never yet fatal, because the two compensating generator
overrides below cover most of the gap. One consumer had to move with it: the `uintptr` hop read
`ValueType` as the type to CONSTRUCT (`new {valueType}(…)`) and now constructs the LH type and casts
to `ValueType`, exactly like the default body. (Guarded by `implicitConvValueType_test.go` — an
end-to-end conversion of two named numerics with different underlyings, plus a unit sweep over every
basic kind a named numeric can carry.)

When both named types live in the **same** assembly the default body is fine (e.g. runtime's
`muintptr ↔ Δhex`), but when the operator must **construct a *foreign* named numeric** — one declared
in another C# assembly — two problems appear that only manifest cross-assembly:

* A direct cast to the foreign named type has no route. `(NameOff)src.Value` where `src.Value` is `ulong` and `NameOff` (`internal/abi`) is a *different assembly* is **CS0030** — C# does not select the foreign type's `int32`-based user conversion for a `ulong` source across the assembly boundary (the same cast to a *local* named type compiles). It must go **through the foreign type's underlying basic**: `new …NameOff((int)src.Value)`.
* The default host can be a phantom. The operator is hosted in `partial struct {sourceType}`; if that source is the *foreign* type (reached here via a local alias, e.g. runtime's `global using nameOff = abi.NameOff`, so the cross-package dot is hidden and the conversion records as `Inverted`), the `partial struct NameOff` declares a new *empty local* type rather than extending the foreign one — **CS1729** (no constructor). The operator is relocated into the **local** type instead.

So for a foreign *constructed* type the generator emits, fully-qualified and hosted in the local type:

```csharp
// runtime, dur↔hex style: foreign abi.NameOff constructed from local Δhex
partial struct Δhex {
    public static implicit operator global::go.@internal.abi_package.NameOff(global::go.runtime_package.Δhex src)
        => new global::go.@internal.abi_package.NameOff((int)src.Value); // through the underlying int32
}
```

The override fires **only** when the `new`-constructed side (the LH type: the *source* when the conversion is `Inverted`, else the *target*) is foreign; same-assembly operators are emitted byte-identically as before (no churn). Because the trigger is inherently cross-assembly, the behavioral-test harness (single-assembly, and unable to import a foreign named numeric — `internal/*` types are un-importable from a test module and the baseline stubs expose none) cannot host it; the guard is the **`core/runtime` build**, where `NameOff`/`TypeOff`/`TextOff` ↔ `Δhex` naturally occur (this fix cleared 3×CS0030 + 3×CS1729 there).

A **same-assembly** pair also needs the through-underlying routing when the two named numerics have **incompatible underlyings** — internal/trace's public `type Time int64` ↔ unexported `type timestamp uint64`, converted both ways (`Time(ev.Ts)` / `timestamp(ts)`). The default `new Time((ΔTime)src.Value)` casts `src.Value` (a `ulong`, since `timestamp` is `uint64`-backed) straight to the wrapper, which routes through the wrapper's `long`-based user conversion — but `ulong`→`long` is not an implicit C# conversion, so the cast is **CS0030**. (This is the *mixed-accessibility* case: `Time` is exported and `timestamp` is not, so the operator is already relocated into the less-accessible `timestamp` struct — orthogonal to the underlying.) The generator now, for a **local** numeric pair, casts through the constructed type's underlying C# keyword when the source underlying does **not** implicitly convert to it: `new Time((long)src.Value)`, `new timestamp((ulong)src.Value)`. The source/constructed underlyings are read from each side's `[GoType("num:X")]` tag (a sibling generator cannot see the generated `Value` property), and the implicit-convertibility test is the fixed C# numeric-conversion table over the fixed-width integer/float basics. Crucially this fires **only** on pairs the default cast could not compile (the default `(Wrapper)src.Value` succeeds *iff* that same source→underlying conversion is implicit), so every already-compiling conversion stays byte-identical — the full behavioral suite's [goldens](../Glossary.md#golden) are unchanged. `uintptr`-backed pairs keep the existing `nuint`-hop override; `int`/`uint` native-width wrappers are deliberately left to the default (their classification is version-sensitive and the failing corpus cases are fixed-width). (Guarded by the `NamedIntSignednessConv` behavioral test — a public `int64` ↔ unexported `uint64` named pair converted both ways, including a `^uint64(0)`→`int64` case whose `-1` result verifies the cast preserves the bit pattern exactly, output-compared vs Go; internal/trace's `timestamp`→`Time` inverse operator relies on it.)

A **cross-assembly** mixed-accessibility pair has no legal form at all, and is skipped. The relocation
above is the only remedy for a mixed pair — a C# user-defined conversion operator is necessarily
`public` **and** must be declared in one of its two operand types — and a *foreign* type cannot host
anything. Hosting in the local, more accessible side then exposes a type less accessible than the
operator: **CS0056** when the foreign side is the return type, **CS0057** when it is the parameter, so
neither direction is expressible. The shape is reachable only under the `-tests` white-box model, where
a package's own `_test.go` declares an EXPORTED defined type over an UNEXPORTED production one — `time`'s
`export_test.go` has `type RuleKind int` beside `zoneinfo.go`'s `type ruleKind int`, which become
`public RuleKind` in the test assembly and `internal ruleKind` in the referenced production assembly.
Nothing is lost by skipping: the converter renders such a conversion site as an explicit
through-underlying cast (`(RuleKind)(nint)r.kind`), which needs no operator at all. The local side's
accessibility comes from the GO export rule, as in the relocation above (at analysis time the `[GoType]`
partials are modifier-less); the FOREIGN side is read from metadata, where it is already final.

The recorded `GoImplicitConv` must also be able to **name the foreign type**. The recorded type name carries the foreign package's import qualifier — the DOT form `driver.IsolationLevel` for an unrenamed type, or a `ꓸ` global-using alias (`CrossPkgLibꓸGrade`) for a `Δ`-renamed one — but the attribute sits in `package_info.cs` at file scope and the generated operator lands in a `.g.cs`, neither of which carries the body files' import `using`s. A `Δ`-renamed foreign numeric resolves through its own `ꓸ` global using, but the **dot form needs a resolving `using driver = go.database.sql.driver_package;`** in `package_info.cs`'s `ImportedTypeAliases` block. The STRUCT-conversion branch of `checkForImplicitConversion` already drives that using by calling `recordConversionPackageUsing(argType)`/`(funcType)`, but the **aliased-NUMERIC branch omitted it** — so a cross-package named-numeric conversion (database/sql's `driver.IsolationLevel(opts.Isolation)`, where `sql.IsolationLevel` and `driver.IsolationLevel` are distinct named ints) left `driver` unresolved in both the attribute and the generated operator (CS0246). The numeric branch now records the same package usings. (Guarded by an extension to the `CrossPkgUser` cross-assembly test — a local `float64`-based named numeric converted to the *unrenamed* `CrossPkgLib.Celsius`, which renders in dot form and so needs the registered using; a `Δ`-renamed target like `CrossPkgLib.Grade` would have resolved via its alias and would not have caught the gap.)

The same underlying routing applies when an untyped-constant **shift** is re-typed to a named numeric. An untyped shift `1 << k` is re-typed to the type it assumes from context (so it can combine with typed operands); when that resolved type is a *named* numeric, the re-type must go through the underlying — `(arenaIdx)((nuint)1 << k)`, not a bare `(arenaIdx)(1 << k)` (CS0030). The shift's *width* is likewise decided by the underlying (a `nuint`/`uint64`-backed named type shifts the left operand in that width to avoid the `int`-overflow seen for `1 << 63`). Non-named shifts are unchanged. (Guarded by the `NamedNumericShiftConv` behavioral test — wide `uint`/`uint64`-backed and narrow `uint8`-backed named types; runtime hits this on `arenaIdx(1 << arenaBits)`.)

The unsigned named-numeric path above gets a width-cast operand, but a **signed** constant operator expression whose target is a plain builtin `int64` has no such cast, so C# would compute it in `int32` and overflow at compile time in checked mode (CS0220): `int64(1<<63 - 1)`, `var d int64 = 1<<40 + 7`, or `12345 * 1000000000 + 54321` passed to an `int64` parameter. Go evaluates each as a constant in its `int64` type. For a signed constant binary/shift expression whose folded value is **outside the C# `int32` range**, the converter emits the **folded 64-bit literal** (`9223372036854775807L`, `1099511627783L`, `12345000054321L`) instead of the operator form — correct, and self-contained. In-range constants are unchanged (they keep the readable `1 << k` form). (Guarded by the `UntypedConstArithmetic` behavioral test; runtime hits this in `mgcmark`/`netpoll`/`runtime1`.)

A signed fold whose resolved type is Go **`int`** (C# `nint`) additionally carries its own cast (2026-07-17): C# has no implicit `long`→`nint` conversion, so the bare `L` fold failed loudly at every **non-assignment** use — strings' SplitN test table puts `math.MaxInt / 4` in an `n int` struct field, and the composite-literal element emitted `2305843009213693951L` against the `nint` field (CS1503; the Phase-4 blocker-map row B7a, one site each in strings and bytes). The fold now emits `(nint)(2305843009213693951L)` — the *parenthesized* cast form the assignment path's `nativeIntConstCastType` already recognizes (`wholeExprIsCastOfType`), so assignments that previously received the whole-RHS wrap render **byte-identically** (the cast simply moves into the fold; `NativeIntWideConstAssign`'s `n = (nint)(144115188075855772L)` is unchanged). The value always fits — `nint` is 64-bit on all supported platforms — and the conversion is a runtime unchecked narrowing, never a C# constant expression, so no checked-context overflow arises. An *untyped*-int subtree keeps the bare `L` form (its enclosing context supplies the conversion), as does an `int64` target (`long` is already exact). (Guarded by the `NativeIntWideConstElement` behavioral test — composite-literal elements, call arguments, and a var initializer, values verified vs Go.)

The **in-range widened** sibling (2026-07-17; sort's test-suite conversion): a typed-`int` constant operator expression whose *value* fits int32 but whose emitted arithmetic an operand fold has widened to `long` — `maxswap: 1<<31 - 1` (sort_test `countOps`): the whole value (2147483647) is in range, so no whole-expression fold applies, but the *untyped* inner shift folds to a bare `2147483648L`, making the rendering `2147483648L - 1` — a C# `long` with no implicit conversion to the `nint` composite field (CS1503; assignments were equally unprotected, since `nativeIntConstCastType` also requires the whole value out of int32 range). `convBinaryExpr` now wraps such an expression in the same parenthesized cast at its own emission: `(nint)(2147483648L - 1)`, position-independent. The trigger is **shape-restricted** (`operandRendersWidenedFold`): an operand must be an *operator subtree* whose `overflowingConstLiteral` fold is non-empty — a named untyped-const *reference* of the same value renders as its `Untyped*` wrapper, which narrows itself at the use site (`maxInt - maxInt` stays unwrapped; wrapping it would churn green emissions). (Guarded by the `NativeIntWideConstElement` extension — the `1<<31 - 1` element, call argument, and assignment, plus the wrapper-operand control, values vs Go.)

**FLOAT literals in INTEGER contexts** render their integer form (2026-07-17; sort's test-suite conversion). A float literal *directly* typed integer by go/types has always folded (`math.Inf(1.0)` → `1` — the convBasicLit integer-form rule), but inside a constant operator expression the literal stays **untyped float** (go/types resolves the context on the outermost node only): search_test's tests table writes `{"descending 7", 1e9, …, 1e9 - 7}` against `n, i int` fields, and the element rendered `1e9D - 7` — a C# `double` against the `nint` field (CS1503). Integer contexts now **propagate** through `markUntypedConstContexts` exactly like float/complex ones, and a float literal whose propagated context is integer emits its exact integer form: `1000000000 - 7` — the arithmetic stays exact C# `int`, implicitly convertible everywhere. Two soundness gates: a non-integral literal (`1.5`) keeps its loud `D` form (`constant.ToInt` exactness), and **division does not propagate** an integer context — Go evaluates an untyped-float constant `/` in exact rational arithmetic, so a nested quotient may be transiently non-integral (`3.0 / 2 * 2` = 3) where folded operands would int-divide (`3/2*2` = 2), a silently wrong value; those trees keep the loud `double` rendering. (Guarded by the `NativeIntWideConstElement` extension — `1e9 - 7` and `5e8 * 2` elements and a `2e9 - 8` call argument, values vs Go.)

**UNSIGNED** constant expressions fold under a much narrower trigger (2026-07-03): every other unsigned shape already has a working mechanism — a *typed* unsigned shift gets the width-cast operand (`(uint64)1 << 40`), an int64-range untyped subtree is folded by the signed arm when recursion reaches it (`(281474976710655L) + arenaBaseOffset` in runtime `mranges`), and a named-const reference renders via its `Untyped*` wrapper (`(uintptr)m5 ^ 4` in runtime `hash64`). The one unfixable shape is an untyped constant **operator** subtree (a BinaryExpr) whose value exceeds **int64 entirely**: `1<<63` nested inside `(1 << 63) - 1` — go/types lands the uint64 conversion on the outermost constant node, so the inner shift stays untyped, no width cast reaches it, and C# computes it in int32. `int64((1 << 63) - 1 - (1<<63)%uint64(n))` (math/rand `Int63n`, CS0220) emits as `(int64)(9223372036854775807UL - (((uint64)1 << (int)(63))) % (uint64)n)`: the constant subtree folds to `UL`, the standalone *typed* shift keeps its readable width-cast form. Gated to plain-`uint64` underlying targets (`constExprHasBeyondInt64UntypedOperatorSubexpr`) — a native-width `uintptr` target would need a further cast the fold cannot safely synthesize, so that pre-existing caveat keeps its visible error. A first broader cut (any untyped subtree beyond int32, any unsigned target) regressed runtime's `hash64`/`mranges` by stealing exactly those already-working shapes — the narrow trigger is load-bearing. (Guarded by the `UntypedConstArithmetic` extension — the Int63n shape, value-compared vs Go.)

**`uintptr` was missing from the width-cast retype the paragraph above relies on** (2026-08-09). Everything there is conditioned on "a *typed* unsigned shift gets the width-cast operand (`(uint64)1 << 40`)", which is emitted by the shift retype in `convBinaryExpr` when `isWideShiftType` says the target does not promote to `int`. That predicate listed `uint32`/`uint64`/`int64`/`nuint` — and `uintptr` is the one wide unsigned type that does NOT render as a C# primitive: Go's `uint` becomes `nuint`, but Go's `uintptr` becomes golib's `uintptr` STRUCT. So it fell to the narrow arm, which casts the **result** — precisely the thing the function's own comment says does not help, because the shift has already happened in `int32`. `1 << (4 * goarch.PtrSize)` emitted `(uintptr)(1 << (int)(32))`, C# masked the count to five bits, and the value was **1**. golib's `uintptr` carries a native `nuint` and declares `operator <<(uintptr, int)` over it, so a cast OPERAND (`((uintptr)1 << (int)(32))`) shifts at 64 bits exactly as `nuint` does; `uintptr` simply joins the list. Whole-corpus A/B: **eight files, one mechanical family**, six of them already-correct sub-int32 values reshaped (`16 << 10`, `512 << 20`, `1 << 16`, `1 << 20`) and **two live wrong answers** — `runtime/internal/math`'s `MulUintptr` overflow fast path, whose guard read 1 instead of 2³² so every `uintptr` below `MaxUint32` "overflowed", and `runtime/mpagealloc_64bit.go`'s `1 << heapAddrBits`, which computed 2¹⁶ where Go computes 2⁴⁸. The neighbouring `1<<(UintptrSize/2) - 1` was always right, which is what hid this: there the shift is an INNER node still typed `untyped int`, so the signed fold above takes it whole. CNR is byte-identical across all 576 behavioral packages — no behavioral project had the shape until now. (Guarded by the extended `LargeUintptrConst` behavioral test, already the `MaxUintptr` pattern's home: a context-typed `1 << (4 * ptrSize)`, a literal-count `1 << 40`, and the composite-literal table row beside its always-correct `- 1` sibling, values vs `go run`; and by `runtime/internal/math`'s banked suite.)

**FLOAT** contexts need the same fold, and there the damage is **silent** rather than a compile error (2026-07-17). C# masks a shift count to the left operand's width (5 bits for `int`), so an integer-literal constant in a float context — where no arm above applies, because the constant's type is not an integer — evaluates in int32 and *quietly* yields the wrong number: `var hf float64 = 1 << 63` emitted `(1 << (int)(63))`, i.e. 63 & 31 = 31 → `int.MinValue`, and `hf / (1 << 60)` divided by 2^28 (60 & 31 = 28) instead of 2^60, printing 34359738368 where Go prints 8. Go evaluates the constant in exact arithmetic and converts the *result* to the float type, so the converter emits the Go-evaluated value as a float literal — `float64 hf = 9223372036854775808D`, `hf / (1152921504606846976D)`, `float32 sf = 1099511627776F` — which also carries the values `1<<63` puts beyond `int64`, where no `L`/`UL` fold could reach. Two gates keep the readable operator form everywhere it is already correct: the operands must be **all integer literals** (that is what makes C# evaluate in int32 — a float-literal operand like `1e18 * 10.0` already computes in `double`, and a named-const operand renders via its `Untyped*` wrapper), and the value must be **outside int32** (`1 << 10` computes identically in C# and is left alone). Unlike the int64 case, an inner shift is *not* rescued by recursion: Go promotes the operands of `1<<40 * 1.5` to a common kind, so the shift is recorded `untyped float` — invisible to the signed arm's integer test — and folds from its propagated context instead (see `markUntypedConstContexts` under [Constant Values](constants.md#constant-values)); left bare it masks to 256 and silently yields 0.375. The full-stdlib A/B footprint was exactly six lines, every one a live wrong-value bug: `math`'s `normalize` (`x * (1<<52)` off by 2^32), `cbrt` (2^54), `ldexp`'s denormal factor (`1.0/(1<<53)`), `pow`'s `1<<53`/`1<<63` branch guards, and **both** `math/rand` `Float64`s — v1 divided by `int.MinValue` and so returned *negative* numbers, v2 divided by 2^21 instead of 2^53. (`floatContextConstLiteral`, `convBinaryExpr.go`; guarded by the `UntypedConstArithmetic` extension — the `1<<63`/`1<<60` float64 and `1<<40` float32 folds, the `untyped float` nested shift, plus in-range and float-literal controls that must keep their operator form, values vs Go.)

**The same fold covers a complex128 context (2026-07-18).** `complex128` is float64-backed (`System.Numerics.Complex`), so an all-integer-literal shift whose *result* type is `complex128` — a slice/array element like `[]complex128{1 << 35, 1 << 240}` (`math/cmplx`'s `hugeIn` test inputs) — carries the identical int32-masking hazard: `1 << 35` emitted `(1 << (int)(35))`, which C# masks to 35 & 31 = 3 → **8** instead of 2^35, silently corrupting the complex value's real part (its imaginary part is 0, so it is not int-literal arithmetic). `floatContextConstLiteral` takes the constant's **real part** and folds it to a `D`-suffixed literal — `34359738368D`, and the 73-digit exact form of `1<<240` — which C# parses to the same float64 the Go constant rounds to (a power of two lands exactly; a mixed value like `1234567891234567 << 40` round-trips to the nearest double, matching Go). `complex64` is deliberately excluded: its float32 real part would overflow to a C# compile error for the beyond-float32 magnitudes this fold targets, and such constants do not arise. This cleared `math/cmplx`'s `TestTanHuge`, whose huge `Tan` inputs were being reduced to tiny masked values (8, 65536, 4096) — `Tan` then computed correctly on the *wrong* arguments. (Guarded by the `ComplexConstContext` behavioral test — `1<<35`/`1<<240`/`-1<<120`/`1234567891234567<<40` complex128 real parts, values vs Go.)

The same coercion is needed where the converter itself inserts a C# `(int)` cast on a named-numeric value — a **slice bound** (`summary[sc+1:ec]` with `sc`/`ec` of type `chunkIdx`), a **shift count** (`1 << (d % 64)` with `d` of type `statDep`), or the **length of an `unsafe.Pointer`-to-array slice** (`(*[N]T)(ptr)[:n]` → `new slice<T>(new ReadOnlySpan<T>(ptr, (int)n))`, since the `ReadOnlySpan<T>` constructor takes a C# `int` — see *Slicing a pointer-to-array*). A bare `(int)(sc + 1)` is CS0030 for the same reason, so the converter emits `(int)(nuint)(sc + 1)` / `(int)(nint)(d % 64)` — through the named type's underlying basic; a plain `nint`/`nuint` length is narrowed `(int)(n)`. Plain basic operands keep the bare `(int)(x)` form. (Guarded by the `NamedNumericIntCast` behavioral test; the Span length by `StdLibInternalAbi`.)

**Defined types over a struct — forwarded fields.** A Go type definition over a *struct* — `type winlibcall libcall` — makes the underlying struct's fields accessible on the named type (`w.fn`), without promoting its methods. The named type is emitted as `partial struct winlibcall /*libcall*/;` and the `TypeGenerator` wraps the underlying value (`private libcall m_value;`). For the underlying's fields to be reachable, the generator **forwards each as a ref-returning property** over `m_value`:
```csharp
private libcall m_value;                 // NOT readonly — see below
[UnscopedRef] public ref nuint fn => ref m_value.fn;
[UnscopedRef] public ref nuint n  => ref m_value.n;
// … args, r1, r2, err
```
The underlying struct is resolved with `GetStructDeclaration` (same package, or a *source*-referenced package), and its members come from `GetStructMembers`. Crucially `m_value` is **mutable** (not the wrapper's usual `readonly`), so a write through a pointer — `c.Value.fn = fn`, where `c` is a `ж<winlibcall>` and `c.Value` is `ref winlibcall` — reaches the real storage and persists. (The `readonly`→mutable choice is decoupled from the nullable-`m_value` form that only the lazily-allocated `array` backing needs.) Forwarding is skipped for a non-struct underlying (a named type over an interface or another named type) and for an underlying that contributes no fields, so those wrappers are unchanged. *Composite-literal construction* of such a type (`winlibcall{fn: x}`) is a separate, not-yet-handled case (the runtime accesses these only by field). (Guarded by the `NamedTypeOverStruct` behavioral test — write-through and read-back of forwarded fields through a pointer; runtime hits this on `winlibcall` over `libcall`, `syscall_windows.go`.)

**The forwarded member must be a VARIABLE, and the underlying may be METADATA-ONLY (2026-07-31).** Two independent defects in the paragraph above, both surfaced by `index/suffixarray`'s `suffixarray_test.go` — `type index Index`, where `Index` has an `ints`-typed field `sa` with `len`/`get` methods — and both fixed generally:

1. **A get/set property is not a variable.** In Go the selection *is* the underlying field, so `x.sa.len()` binds a receiver the converter emits `this ref` (every value-receiver method is a ref extension) and `&x.sa` / `x.sa.Push(…)` take its address. A get/set property yields a *value*, so all of those were **CS0206** ("a non ref-returning property or indexer may not be used as an out or ref value"). The forward is now a **ref-returning property**, which is a strict superset — `w.fn = v` still assigns (through the ref), and the variable-requiring uses now bind. `[UnscopedRef]` is what makes it legal at all: a struct member returning a ref to instance state is **CS8170** by default (the receiver could be a temporary), and the attribute states the ref's lifetime is the *receiver's* — exactly Go's guarantee, since the selection aliases the wrapper's own storage. C#'s ref-safety rules then reject at the call site precisely the cases Go also rejects (addressing a non-variable). Note the neighbouring array-view case below keeps its ensure-then-share-copy shape: its accessor must *materialize* a lazily-allocated backing first, which is a different problem than aliasing an existing field.

2. **A metadata-only underlying resolved to nothing.** `GetStructDeclaration` can only see a struct whose SOURCE is in this compilation or in a `CompilationReference`; a real MSBuild build hands a `<ProjectReference>` to the compiler as compiled *metadata*, so a defined type over a struct in **another package** forwarded no members at all and every selection on it was **CS1061**. `FindUnderlyingStructSymbol` now resolves the `[GoType("…")]` definition to its `INamedTypeSymbol` when the syntax walk misses — trying the name as written (`global::go.index.suffixarray_package.Index`, the fully-rooted form the `-tests` white-box bridge emits) and then `go.`-rooted (`time_package.Duration`, the package-alias-qualified form ordinary cross-package emission uses, is not a CLR name) — and `GetForeignStructMembers` enumerates it. Membership mirrors `StructTypeTemplate`'s metadata field scan: instance FIELDS plus the ref-returning, non-indexer PROPERTIES a referenced assembly's generated wrapper exposes for its embedded and promoted members. Visibility is decided by `Compilation.IsSymbolAccessibleWithin` rather than a public-only test, which is **Go's own rule projected into C#**: an exported field is `public` and always forwards, while an unexported one is `internal` and forwards only where C# can reach it — i.e. the friend (`InternalsVisibleTo`) test assembly, which is precisely the same-Go-package case where Go permits the selection. Ordinary cross-package wrappers over foreign structs whose fields are unexported (`type timeTime time.Time`) therefore forward nothing, exactly as Go allows nothing.

Both fixes were needed for one package: with only (2), the CS1061 wall collapsed to the board's originally-reported **CS0206** at two sites — a worked example of charter §9's root-cause layering (the first diagnostic moved rather than cleared). (Guarded by the `DefinedTypeOverForeignStruct` behavioral test — a `ptlike` sub-library supplies `Outer{Name string; In Inner}`, the parent declares `type alias ptlike.Outer` and reads a forwarded field, writes one, calls `Inner`'s value- and pointer-receiver methods *through* the forwarded field with the mutations observed afterwards, writes a nested element, converts back to the underlying, and reads the zero value — output-compared vs `go run`. It is the cross-assembly sibling of `NamedTypeOverStruct`, which covers the same-package case, and of `DefinedTypeOverPkgType`, which covers a cross-package defined type reached only by conversion, never by field.)

**Defined types over an array-backed defined type — the IArray view.** A second-level definition — `type pallocBits pageBits`, where `type pageBits [8]uint64` is itself an array-backed `[GoType]` wrapper — is `len()`'d and indexed directly in Go (runtime `mpallocbits.go`), which requires `IArray` on the **outer** wrapper (golib `len(IArray)`; CS1503 otherwise, and the named-over-array *indexing* sites in `mgcscavenge`/`proc`/`traceback` fail the same way). The generator detects this in the bare-name branch — the resolved underlying struct contributes no declared members but its own `[GoType]` definition is an array form (`[N]elem`) — and implements `IArray<elem>` on the wrapper as a **view** (`IArrayViewTypeTemplate`). Every member delegates through a private `view` accessor that first touches `m_value.Value` **on the mutable field** — materializing the underlying's *lazily-allocated* backing in the wrapper's own storage — and then returns a value copy sharing that heap `T[]`, so element refs land in the real storage. (Going through the plain copying `Value` property instead silently dropped writes on a zero-valued wrapper — the backing allocated on the copy — which is the historical `pallocBits` lost-writes trap, reproduced and pinned before the fix. A struct member cannot ref-return its own field — CS8170 — so the ensure-then-share-copy shape is the correct one; the `(pageBits)(b)` reinterpret conversions keep compiling and, once the backing exists, write through shared storage.) (Guarded by the `NamedArrayWrapper` behavioral test — `len`, index read/write, and a write via the `(*pageBits)(b)` reinterpret observed through the original, values vs Go; cleared runtime's 5 `pallocBits → IArray` CS1503 **plus a −3 CS0021 cascade** of named-over-array indexing, 86 → 74 with the `copy` overload below.)

**`copy` from a defined slice type.** `copy(dst, src)` where `src` is a *named* slice type — `type pMask []uint32`, runtime `proc.go`'s `copy(nidlepMask, idlepMask)` — cannot bind the generic `copy<T1,T2>(in slice<T1>, in slice<T2>)`: the wrapper implements `ISlice<uint32>` but *is not* a `slice<T2>`, and generic inference does not see user-defined conversions, so resolution fell onto `copy(slice<byte>, @string)` (CS1503 ×2 per call). golib adds `copy<T1, T2>(in slice<T1> dst, ISlice<T2> src)` — `T2` infers from the implemented interface — copying element-wise through the interface indexer with the same min-length/convert semantics; a genuine `slice<T>` source still binds the more-specific slice/slice overload, so existing calls are unchanged. (Guarded by the same `NamedArrayWrapper` test — `copy` count/values plus post-copy independence of source and destination, vs Go.)

The wrapper also forwards the underlying's **field-box accessors**. Taking the address of a wrapper's field — `&p.x` on a `*pinnerBits`, where `type pinnerBits gcBits` (runtime `pinner.go`) — emits the box-accessor form `Δp.of(pinnerBits.Ꮡx)`, whose owning type is the **wrapper**; without a forwarded accessor the static exists only on `gcBits` (CS0117). For every forwarded *field* (properties cannot be `ref`'d and get none, matching the plain-struct template) the generator emits the accessor as a **true ref through `m_value`** into the underlying struct's field: `public static ref uint8 Ꮡx(ref pinnerBits instance) => ref instance.m_value.x;` — a genuine ref chain into the wrapper's own storage, so a write through the resulting box persists (a copy here would silently drop writes — the trap that sank an earlier `pallocBits` forwarding attempt). Emitted only when members are forwarded, which is exactly when `m_value` is mutable. (Guarded by the `NamedTypeOverStruct` extension — `bump(&c.a)` writes through the wrapper's field address and the original observes it; cleared runtime `pinner.go`'s 3 CS0117, 89 → 86.)

**Untyped constants in a typed-element context (`append`).** Because an untyped constant renders as a bare C# `int`/`double` literal or an `Untyped*` wrapper, passing one as an `append` element to a typed slice trips C#'s overload resolution: `append<T>(ISlice, params T[])` infers `T` from the element while the `slice<T>` overloads infer `T` from the slice, so `append([]uint16, replacementChar)` (or `append(buf, 7, 8)`) would pick `slice<int>` and fail (CS0121 / CS0029). The converter therefore casts an untyped *numeric*-constant `append` element to the slice's element type, matching Go's implicit conversion:

```go
var a []uint16
a = append(a, replacementChar)   // replacementChar is an untyped const
a = append(a, 7, 8)
```
```csharp
slice<uint16> a = default!;
a = append(a, (uint16)(replacementChar));
a = append(a, (uint16)(7), (uint16)(8));
```

The same cast reaches an untyped numeric constant referenced through a **cross-package SELECTOR**.
`isUntypedNumericConstArg` had matched only a bare `*ast.Ident`, so `append([]byte, tabwriter.Escape)`
(go/printer's block builder — `tabwriter.Escape` is `const Escape = '\xff'`, rendered as a golib
`UntypedInt`) kept the ambiguity (CS0121 ×6). The gate now also inspects an `*ast.SelectorExpr`'s `Sel`
constant object, casting the element to the slice's element type: `append(block, (byte)(tabwriter.Escape))`.
That same selector gate also feeds the deferred method-value arg cast — `defer(Δsyscall.Seek,
Ꮡfd.Value.Sysfd, curoffset, (nint)(io.SeekStart), ref ᒐ)` (internal/poll `fd_windows.cs`) casts the
const to the parameter type rather than the default-type wrap — and the `regexp/syntax`
`unicode.MaxRune` append; both are equal-or-better and compile.
A same-package untyped const (a bare ident) is unchanged. (Guarded by the `CrossPkgUser` extension —
`append([]byte, CrossPkgLib.Sep)` (rune `':'`) and `append([]rune, CrossPkgLib.Precision)` (int `2`), both
cross-package untyped consts reached through a selector, output-compared vs Go; without the fix the appends
are CS0121.)

The parameter-type cast also reaches the **lambda form** of a go/defer call, not just the method-value
form. When the callee returns a value (or is a value-receiver method), `visitGoStmt`/`visitDeferStmt`
force the temp-param lambda `goǃ(ᴛ1 => f(ᴛ1), arg)` (see *A value-returning goroutine callee is wrapped
in a discarding lambda*); there the arg's C# type drives ᴛ1's inference, and the lambda body's `f(ᴛ1)`
then needs ᴛ1 to be `f`'s parameter type. An untyped numeric const otherwise took `convExprList`'s
DEFAULT-Go-type cast, so ᴛ1 inferred the default (`nint`) and `f(ᴛ1)` failed — hash/crc32's
`go MakeTable(Castagnoli)` (Castagnoli an untyped `uint32` poly) emitted `goǃ(ᴛ1 => MakeTable(ᴛ1),
(nint)Castagnoli)`, CS1503. `convCallExpr` now applies the parameter-type cast in the lambda form too, but
ONLY when the parameter differs from the const's default type (`untypedNumericConstArgDefaultType` vs the
param's underlying basic) — when they match, the existing default-cast path already yields the right type,
so overriding would only churn the golden (`(nint)x`→`(nint)(x)`). Proven zero-drift on the behavioral
corpus and the full stdlib reconvert (the fix fires only where a wider/other parameter demands it).
(Guarded by the `GoUntypedConstArg` behavioral test — `go compute(poly)` with a value-returning callee and
an untyped `uint32`-poly const, output-compared vs Go; without the fix the `goǃ` arg is `(nint)poly` and
the lambda body is CS1503.)

**A string-literal spread** — `append(b, "runtime error: "...)` (runtime `error.go`'s message builder) — renders the literal as a `"…"u8` `ReadOnlySpan<byte>`, which has no spread property (`.ꓸꓸꓸ` → CS1061). The spread emission wraps a direct string-literal source in the member-accessible `@string` — `append(b, ((@string)"runtime error: "u8).ꓸꓸꓸ)` — whose `ꓸꓸꓸ` returns the `Span<byte>` the `append<T>(slice<T>, params Span<T>)` overload binds; this is the same wrap the `string(r)...` conversion spread uses (above). A non-literal spread source (a slice, a `@string` variable) is unchanged. (Guarded by the `StringConvPostfix` extension — two literal spreads appended and value-compared vs Go.)

**A string-literal CONCAT as an object/interface vararg argument** — runtime `stack.go`'s newline+tab join in `print`'s diagnostics — needs the same u8 suppression the direct literal argument already gets, propagated INTO the `BinaryExpr`'s operands: both halves otherwise render as `"…"u8` spans, and a `ReadOnlySpan<byte>` cannot box to `object` (CS1503) nor be `+`-concatenated. The binary-expression conversion now honors an incoming `BasicLitContext.u8StringOK=false`, so the operands render as plain C# strings whose `+` and boxing are fine; the default context leaves every other path unchanged. (Guarded by the `StringConvPostfix` extension — a concat with an escape into an `fmt.Println` vararg plus a nested three-way concat, values vs Go.)

**A `[]byte("literal")` over a plain-text string literal** feeds the zero-allocation `u8` ROM span straight into the slice — `[]byte("hi")` → `slice<byte>("hi"u8)` — via golib's `slice<T>(ReadOnlySpan<T>)` factory (which copies the span into the slice's backing array), rather than routing the literal through a heap `@string` first (the older `slice<byte>((@string)"hi")` allocated an `@string` and then converted it to `byte[]`). Both the general `[]byte`/`[]rune` conversion path and the `u8` literal keep their existing forms elsewhere; only the specific plain-`[]byte`-literal case is retargeted, gated to exactly what `convBasicLit` renders as a `u8` span: a `[]rune` literal keeps `@string` (it needs `@string`'s rune decoding, not raw UTF-8 bytes); a high-`\xHH`-byte `[]byte` literal keeps the byte-array-backed `@string` (its bytes do not round-trip through `u8`); a NAMED byte-slice type (`type htmlSig []byte`) keeps its wrapper cast; and a string *variable* is already an `@string`. Not ambiguous with the array `slice<T>(T[])` builtin — a `u8` literal is a `ReadOnlySpan<byte>` (an exact match for the new overload), while an `@string` converts to `byte[]` but not to a span. (Guarded by the `StringLiteralSliceConversion` extension — plain-text, raw-backtick, and high-`\xHH`-byte `[]byte` literals plus `[]rune` and string-variable controls, output-compared vs Go; and confirmed across ~144 stdlib sites by the full reconvert.)

**The element-decoding rules key on the TYPE, not the spelling.** `byte` and `rune` are aliases of
`uint8` and `int32`, so `[]uint8("foo")` is the same conversion as `[]byte("foo")`. Keyed on the
spelling `[]byte`/`[]rune`, it missed every arm and emitted `slice<uint8>("foo")`, a System.String
with no conversion to the slice (CS1503, R's mapstructure reading). An unnamed slice target whose
element is the basic `uint8` or `int32` now takes the same routes in every position, and keeps the
source's spelling: `[]uint8("foo")` → `slice<uint8>("foo"u8)`, `[]int32("héllo")` →
`slice<int32>((@string)"héllo")`, `[]uint8("a" + "b")` → `slice<uint8>((@string)("a"u8 + "b"u8))`.
A defined element (`type Uint8 byte`) and an alias target (`type B = []byte`) keep their own routes.
(Guarded by `uint8SliceLiteralConversion_test.go` and the `Uint8SliceLiteralConversion` behavioral
test, output-compared vs Go.)

Typed arguments and already-explicitly-converted elements (`uint16(r)`) are left as-is.

Relatedly, when the shifted (left) operand of a shift is an untyped constant — `1 << k` — Go gives the whole shift the type it assumes from context (e.g. `uintptr` when compared with a `uintptr`), but the bare C# literal makes the result `int`, which then cannot compare or combine with the typed operand (CS0034). The shift result is cast to its resolved type:

```go
var u uintptr = 7
_ = u < 1<<8   // 1<<8 takes type uintptr
```
```csharp
uintptr u = 7;
_ = u < (uintptr)(1 << (int)(8));
```

The **narrow-width** flavor of the same shift-retype is a *behavioral* requirement, not just a compile fix: a sub-`int`-width left operand (`int8`/`uint8`/`int16`/`uint16`) promotes to `int` in a C# shift, so the shift computes at 32-bit width with no wraparound at the type's own width — where Go computes a shift in the operand's type. `byte(200) << 1` is 144 in Go (wraps at byte width) but 400 in the promoted C# `int` shift. The shift result is cast back to the **shift expression's resolved Go type**, so a var, *typed*-const, and *untyped*-const left operand all wrap alike (the untyped-const flavor was historically correct only via the wrapper retype above; typed left operands got no cast at all and produced the unwrapped value):

```go
var cb byte = 200
var k uint = 1
fmt.Println(cb << k)   // 144: wraps at byte width
```
```csharp
byte cb = 200;
nuint k = 1;
fmt.Println((byte)(cb << (int)(k)));
```

A whole-expression Go **constant** shift is skipped (Go constant arithmetic cannot overflow its type, and the wrap cast on a C# compile-time constant would even be rejected, CS0221), and **right** shifts take no cast (a narrow operand zero-/sign-extends into the `int`-width shift, so the result always fits the narrow width). A **named** narrow type routes through its underlying — `(nb)(byte)(n << (int)(k))` — since a `[GoType]` conversion accepts only its exact underlying, never C# `int`. `int32`-and-wider left operands already shift at their Go width in C# and keep their existing forms. (Guarded by the `NarrowShiftVarCount` behavioral test — byte/uint16/int8/int16 left shifts by variable counts that overflow the narrow width, across var, typed-const, untyped-const, and named-type left operands, plus right-shift controls, output-compared vs Go.)

A C# **compound shift-assignment** (`<<=`/`>>=`) requires the shift count to be `int`; the count's own (possibly unsigned/native-width) type is rejected — `s.allocCache >>= (nuint)x` is CS0019. So the count is cast to `int` (`s.allocCache >>= (int)x`). This applies whether the assignment target is a simple variable or a **selector/pointer-field** LHS (`s.allocCache`, a field reached through a `*mspan`) — both paths emit the same `(int)` count cast. (Guarded by the `ShiftPrecedenceUnsigned` behavioral test — simple-variable and struct-field shift-assigns with an unsigned count; runtime hits the field form in `malloc`/`mbitmap`'s `allocCache` bit walks.)

A related case is a **computed constant mask under a native-int bitwise operator**. `i & ((1 << shift) - 1)` or `i &^ (blockSize - 1)`, where `i` is a `uintptr`/`uint` (C# `nuint`/`uintptr`) and `shift`/`blockSize` are native-int constants: the mask is a Go compile-time constant, but because the native const is emitted as a get-only property (package scope) or a plain local (function scope), not a C# `const`, the *expression* is not a C# constant, so it renders as a bare `int` — and `nuint & int` is CS0019 (no common type, and no implicit constant conversion since the operand is non-constant). The converter casts such a computed-constant operand to the native result type — `(uintptr)i & (uintptr)((1 << (int)shift) - 1)`. A *small* bare literal (`x & 7`) is left alone (C#'s constant conversion fits it), but a **large** literal whose value exceeds the C# `int32` range (`uintptrMask & 0x00ffffffffff`) is emitted by `convBasicLit` with its own `(nint)`/unsigned cast — so it is no longer a bare `int` and `nuint & nint` is CS0019 too; such a literal operand is cast to the native result type the same way (`& (uintptr)(nint)1099511627775L`). A named untyped-const reference is handled by the wrapper cast below. There is also a `&^` (AND-NOT) twist: it is rendered `& ~y`, and `~` promotes its operand to `int`, so even a *small* constant operand (`p &^ 15` → `nuint & ~15` = `nuint & (int)-16`) is CS0019 — a negative `int` cannot convert to an unsigned native type, even as a constant. So a constant right operand of `&^` with a native-int result is also cast to the native type, `& ~(uintptr)15`, performing the complement in that width (a non-constant native operand, `p &^ mask`, already complements correctly and is left alone). (All guarded by the `NativeIntConstMask` behavioral test — computed mask, large-literal mask, and small-literal `&^`; runtime exercises this in arena/page mask arithmetic such as `arenaIndex`/`alignDown`, `mallocinit`'s `uintptrMask &`, and `os_windows`'s `ptr &^ 15` 16-byte align.)

Similarly, when a *named* untyped numeric constant (emitted as the `UntypedInt`/`UntypedFloat` wrapper) is an operand of arithmetic with a concrete numeric type, the wrapper's bidirectional implicit conversions can make the result resolve to the wrong type (`a * two32`, `uint64 * UntypedInt`, yields `int` — CS0029). The named-const operand is cast to the concrete operand's type (comparisons resolve through the implicit conversion, so only arithmetic is cast):

```go
const two32 = 1 << 32
var a uint64 = 100
_ = a*two32 + 3
```
```csharp
UntypedInt two32 = /* 1 << 32 */ 4294967296;
uint64 a = 100;
_ = a * (uint64)two32 + 3;
```

A constant too large for `int64`/`uint64` (or `float64`) is emitted as `GoBigConst` (=
`System.Numerics.BigInteger`), which has **no** implicit operator with the built-in numeric types.
Unlike an `UntypedInt`/`UntypedFloat` wrapper, that makes a bare reference a hard error in *every*
concrete numeric context, not merely a resolution hazard in arithmetic — so the cast belongs to the
**reference itself** (`bigIntegerConstMaterialization`), applied wherever go/types records a
concrete numeric type on it:

```go
const below1e23 = 99999999999999974834176
var ftoatests = []ftoaTest{{below1e23, 'e', 17, "9.99999999999999748e+22"}}
_ = x > Two129                     // Two129 = 1<<129
```
```csharp
internal static readonly GoBigConst below1e23 = /* 99999999999999974834176 */
    GoBigConst.Parse("99999999999999974834176");
internal static slice<ftoaTest> ftoatests = new ftoaTest[]{
    new((float64)below1e23, (rune)'e', 17, "9.99999999999999748e+22"u8)}.slice();
_ = x > (float64)Two129;
```

Comparison was originally the only casting consumer (in `convBinaryExpr`), which left composite-literal
elements, call arguments, typed `var` initializers, assignments, returns, and channel sends emitting
bare — twelve `BigInteger`→`double` CS1503s in `strconv`'s `ftoa_test.cs` alone. Moving the cast to the
reference serves all of them at once, and the comparison arm was dropped so it no longer double-casts
(`(float64)(float64)Two129`). Two properties make the context type reliable: go/types records the
**converted** type on the reference (inside `[]float64{…}` the recorded type is `float64`, not
untyped), and Go only admits a constant where its value is representable — so a BigInteger-backed
value's concrete context is necessarily float/complex, never a 64-bit integer that would overflow.
The reference is kept readable rather than folded to a literal, the same call
`foldedNamedFloatConstLiteral` makes for a bare reference. (Guarded by `BigUntypedConstComparison`,
extended from comparison-only to every position, with an in-range `UntypedInt` const as the
must-stay-uncast counter-control.)

### An INTEGER expression over a `GoBigConst` constant folds — it has no 64-bit form
The `(float64)Two129` cast above works because BigInteger converts to `double`. An **integer** target has
no such luck: `(uint64)mask` on a 128-bit BigInteger throws `System.OverflowException` at run time. That is
not a corner case — it is the shape of the Go standard library's whole-width byte-classification bitmap
idiom (`go/doc/comment`'s `isHost`/`isPath`/`isIdentASCII`/`importPathOK`, `net/textproto`'s
`validHeaderFieldByte`/`validHeaderValueByte`), where a 128-bit untyped `mask` is legal precisely because
Go requires only the FINAL value of a constant expression to be representable:

```go
const mask = 0 | (1<<26-1)<<'A' | (1<<26-1)<<'a' | (1<<10-1)<<'0' | 1<<'_' | /* … */ 1<<':'

return ((uint64(1)<<c)&(mask&(1<<64-1)) |
	(uint64(1)<<(c-64))&(mask>>64)) != 0
```

Both halves are `uint64`-valued constants, so both must emit as the go/types-recorded **folded value**.
`mask&(1<<64-1)` already did — its `1<<64-1` operand subtree exceeds int64, which
`constExprHasBeyondInt64UntypedOperatorSubexpr` recognizes. The sibling `mask>>64` has no such subtree: its
only unrepresentable operand is the *reference*, which the shift path retyped to the shift's resolved width
(`((uint64)mask).Rsh(64)`) and threw. `overflowingConstLiteral` therefore also folds on
`constExprHasBeyondUint64UntypedConstRef` — any PROPER subexpression that is a named untyped-const
reference fitting neither int64 nor uint64 (the `GoBigConst` emission, `isBigIntegerBackedConstRef`):

```csharp
GoBigConst mask = /* 0 | (1<<26-1)<<'A' | … */ GoBigConst.Parse("10633823862292363665388054147449749504");
return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (576284830442979328UL)) |
        (uint64)((((uint64)1).Lsh((uint64)((c - 64)))) & (576460746666278911UL)))) != 0;
```

Unlike the sibling overflow folds this one is **magnitude-independent**: the operator form does not merely
compute in the wrong width, it *throws*, so a folded value that fits int32 (`mask>>64` of `1<<70 | 1<<3` is
64) folds too. Scope: the unsigned arm covers `uint64`/`nuint`/`uintptr`; the signed arm is confined to the
64-bit-wide targets its `…L` / `(nint)(…L)` contract already covers (a narrower signed target keeps the
operator form, where the wrapper cast fails LOUDLY rather than silently computing the wrong value — no such
site exists in the stdlib corpus). The `mask` local itself stays emitted, unused, carrying the gofmt'd Go
constant as its comment: it is what makes the folded magic numbers readable back to the Go source (its
`BigInteger.Parse` hoists to a static field — see the next subsection).

Corpus footprint of the fold: exactly two files across the 302-package stdlib conversion
(`go/doc/comment/parse.cs`, `net/textproto/reader.cs`), both still compiling clean.
(Guarded by the `UntypedConstWideMask` behavioral test — the `isHost` mask, the `&^`-inverted
`validHeaderValueByte` mask, a small-valued high half, and a `uintptr`-target native-width mask, all
output-compared vs Go. Without the fold the `uintptr` arm is a hard CS0030 and the `uint64` arms throw.)

## A function-LOCAL `GoBigConst` hoists its parse to a `static readonly` field

A Go constant has no runtime existence — its value lives in the instruction stream — and `GoBigConst`
is the one C# constant projection with a real **per-evaluation** cost: `BigInteger.Parse` allocates its
bits array on every run. Emitted as a plain local, that parse re-ran on **every call** of the enclosing
function; `net/textproto`'s `validHeaderFieldByte` paid it 14 times per `canonicalMIMEHeaderKey` call
(560 B against Go's 0) inside `TestCommonHeaders`' want-ZERO `testing.AllocsPerRun` assert — and the
local was not even referenced, every use having been folded by the subsection above. An **int-kind**
function-local big constant therefore hoists its parse to one `private static readonly` field above the
function (the hoisted-string-literal pattern), and the local initializes from the field — a BigInteger
struct copy, which allocates nothing:

```go
func validHeaderFieldByte(c byte) bool {
	const mask = 0 | (1<<(10)-1)<<'0' | /* … */ 1<<'~'
	…
}
```
```csharp
// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ = GoBigConst.Parse("116972063611741436228934278030836105216");

internal static bool validHeaderFieldByte(byte c) {
    GoBigConst mask = /* 0 | (1<<(10)-1)<<'0' | … */
            maskᶜ;
    return …;
}
```

Field names are claimed package-wide (`<name>` + `HoistedConstMarker` `ᶜ` + ordinal on collision —
`reader.cs` declares `maskᶜ` and `maskᶜ1` for its two functions' masks), deterministic because files
convert sequentially; a `-tests` internal variant seeds from the production conversion's claims exactly
as lifted type names do (`productionHoistedConstOrdinals`). **Float/complex OVERFLOW constants keep the
per-call parse**: their exact string may be a rational (`"1/3"`) whose `Parse` throws, and a field
initializer would turn that per-call throw into a package-class `TypeInitializationException`.
Package-level big consts were already `static readonly` fields and are unchanged. (Guarded by
`UntypedConstWideMask` — four functions with local big-const masks, exercising the ordinal chain — and
by `net/textproto`'s validated `TestCommonHeaders`, whose want-zero assert is what surfaced the cost;
L11.)

## The `&^=` (bit-clear) compound assignment on a narrow type
C# has no `&^` (AND-NOT) operator, so Go's `a &^= b` expands to `a &= ~b`. The `~` complement always promotes its operand to `int`, and `int` is not implicitly convertible to a narrower or unsigned LHS type (`byte`/`ushort`/`uint`/`ulong`/`uintptr`/`nuint`) — so `flags &= ~b` is CS0266. The complemented value is therefore cast back to the LHS type, inside `unchecked` because for a *constant* operand `~b` folds to a negative `int` constant whose checked narrowing would overflow (CS0221):

```go
h.flags &^= hashWriting   // h.flags is uint8
```
```csharp
h.Value.flags &= unchecked((uint8)~hashWriting);
```

An LHS type that `int` widens to implicitly (`int`/`int32`/`int64`) needs no cast and stays `a &= ~b`. (Guarded by the `AndNotAssignNarrow` behavioral test, which exercises both an ident LHS and a struct-field LHS — they route through different assignment-emission paths.)

## A standalone `^x` on `uint8`/`uint16` truncates back to the operand's type

The same `int` promotion has a **silent-value** face, not just a CS0266 face. Go's `^x` has x's own
type, so on a sub-int UNSIGNED type the complement wraps to that width — `^uint16(5)` is `65530`. C#
promotes `byte`/`ushort` to `int` first, so bare `~x` is `-6`: identical in the low 16 bits, but every
**widening** use then carries the sign bits, and no cast is required to make it compile:

```go
w.writeBits(int32(^uint16(length)), 16)     // compress/flate, stored-block header
```
```csharp
w.writeBits((int32)((uint16)(~(uint16)length)), 16);
```

Without the inner truncation this wrote `-6`, and `writeBits`' `bits |= uint64(b) << nbits`
sign-extended it across the whole 64-bit accumulator — so every level-0 (`NoCompression`) DEFLATE
stream was garbage and the decoder rejected its own encoder's output with `flate: corrupt input
before offset 59`. It compiled clean and only `compress/zlib`'s `TestWriter`, which round-trips at
every level, caught it.

Only unsigned `uint8`/`uint16` need this. A **signed** narrow type is already value-correct (C#'s `~`
of a sign-extended operand equals the sign-extended Go result: `int32(^int16(5))` is `-6` in both
languages), and every type at least 32 bits wide (`uint`, `uint32`, `uint64`, `uintptr`, all signed
widths) keeps its own type under C#'s `~`. A NAMED type routes through its golib wrapper's operator.

A **CONSTANT** operand needs one more word. The truncation is then a C# *constant conversion*, and
those are checked at compile time no matter what the enclosing context says: `~(ushort)0` promotes to
the `int` `-1`, and `(ushort)(-1)` is a hard **CS0221** — however correct the runtime truncation would
be. The all-ones idiom is exactly that shape, and it is the bound `x/net/dnsmessage` compares each
section count against, seven times in one file:

```go
if len(m.Questions) > int(^uint16(0)) {     // vendor/golang.org/x/net/dns/dnsmessage
```
```csharp
if (len(m.Questions) > (nint)(unchecked((uint16)(~(uint16)0)))) {
```

So a Go-constant operand states `unchecked`; a variable operand does not, since C#'s default context
is already unchecked and the keyword would only add noise at flate's `^uint16(length)` sites.

Where the result is *immediately* narrowed back by the surrounding narrow-arithmetic cast the inner
truncation is redundant — `takeU8(^a)` renders `takeU8((uint8)((uint8)(~a)))`. That is accepted
cosmetic noise: the two forms are value-identical, and the alternative (deciding at the unary site
whether the enclosing context widens) trades a silent-corruption hole for readability. (Guarded by
the `AndNotAssignNarrow` behavioral test's widening cases — `int32(^uint16(x))`, `uint64(^uint8(x))`,
the `uint32`/`int16` no-op controls, the narrow round-trip, and the constant cases `int(^uint16(0))`,
`int(^uint8(0))` and `uint64(^seed)` over a typed `uint16` const.)

## Logical operators on a named boolean type cast through `bool`
A Go defined type whose underlying type is `bool` (`type boolVal bool`) is modeled as a `[GoType("bool")]` struct with an implicit `bool` conversion but no logical operators. Go's `!`, `&&`, and `||` on such a value yield that **same named type**, so `return !y` / `return x && y` in a function returning an interface the type implements (go/constant's `UnaryOp`/`BinaryOp`, returning the `Value` interface) still satisfies the interface. A bare `!y` / `x && y` in C# collapses to a plain `bool` — which cannot implicitly convert to the interface (CS0029), and `!` has no operator on the struct (CS0023). The converter casts each operand through `bool`, applies the operator, then casts the result back to the named type so it keeps satisfying the interface:

```go
case boolVal:
    return !y          // y is boolVal, result must be the Value interface
```
```csharp
case boolVal y: {
    return ((boolVal)(!(bool)y));
}
```

Binary `&&`/`||` take the parallel form `((boolVal)((bool)x && (bool)y))`. A predeclared-`bool` operand keeps the bare `!x` / `x && y` form (no golden churn). (Guarded by the `NamedBooleanLogic` behavioral test.)

## Casting a negative value to a non-keyword type parenthesizes the operand
C# parses `(T)-value` as a cast only when `T` is a keyword primitive (`int`, `long`, `nint`, `byte`, …). For a using-**alias** (`int64`=`long`, `uint64`=`ulong`, `rune`=`int`, …) or a `[GoType]` **named** type (`level`), `(int64)-1` / `(level)-1` is instead parsed as `type MINUS value` — CS0075 ("to cast a negative value, you must enclose the value in parentheses") and CS0119 ("'long' is a type, not valid in the given context"). So a cast whose operand leads with a unary `+`/`-` and whose target is not a C# keyword type parenthesizes the operand:

```go
lvl := level(-1)              // named conversion
mask := -1 << uint(bits)      // int64-typed wide shift
```
```csharp
var lvl = ((level)(-1));
var mask = ((int64)(-1) << (int)((nuint)bits));
```

Two emission sites carry it: the type-conversion cast (convCallExpr, `castOperandNeedsParens`) covers `level(-1)`/`int64(-1)`, and the wide-shift left-operand cast (convBinaryExpr) covers `-1 << bits` (a wide shift type does not promote to `int`, so its left operand is cast to that type). A keyword target (`(int)-1`, `(nint)-1`) and a non-negative operand keep the bare form (no golden churn). (Guarded by the `CastNegativeNamedType` and `ShiftNegativeWideConst` behavioral tests.)

## A cast's operand asks TWO questions: parse ambiguity AND precedence
The section above answers the *parse-ambiguity* question — does `(T)-1` read as a cast or as a subtraction? A cast's operand poses a second, independent question that `castOperandNeedsParens` (a leading-sign TEXT test) cannot see: a C# cast binds tighter than **every** binary operator, so an operand that renders as a top-level binary expression has the cast claim its **left operand alone**.

The named-numeric *identity-constant* arm (convCallExpr — reached when go/types gives a constant operand the target type, so the conversion looks like an identity) asked only the first question. Both symptoms below are the same emission:

```go
rf(3 / 2)                  // type rf float64 -- Go folds 3/2 as untyped INTEGER division: 1
renamedComplex64(3 + 4i)   // type renamedComplex64 complex64
```
```csharp
((rf)(3 / 2))                      // was: ((rf)3 / 2)
((renamedComplex64)(3F + 4F.i()))  // was: ((renamedComplex64)3F + 4F.i())
```

The first was **silently value-changing** and compiled cleanly: Go folds the constant expression in exact arbitrary precision *before* converting (untyped integer division gives `1`), whereas `((rf)3 / 2)` converts first and divides in the target's own float arithmetic (`1.5`). Every named int/float type hides the defect this way, because its `[GoType]` wrapper supplies an operator for the mis-bound first leg. A named **complex** type has no float→named-complex conversion at all, so there the same emission is a hard **CS0030** — which is how the class was found, holding `fmt`'s own test suite (`fmt_test.go`/`scan_test.go`'s `renamedComplex64`/`renamedComplex128` entries).

Keyed on the **AST**, not the rendered text: the operand's emission may be a call, a literal or a folded constant, and only the written expression says whether a binary operator is left exposed. A `ParenExpr` operand already renders wrapped, so the direct type test suffices. **Unary operands are deliberately excluded** — a cast and a unary operator share precedence and associate right, so `(T)~0` already means `(T)(~0)`; their only hazard is the sign ambiguity the section above covers. (Guarded by the `NamedConstConversionPrecedence` behavioral test, which is output-compared so the silent value divergence is caught, not merely the CS0030.)

## A stacked sign keeps its space: `- -a` never becomes `--a`
A unary `-` or `+` whose operand's emitted text starts with the SAME sign keeps a space between the two, which is also how gofmt writes it. C# reads `--` and `++` as single tokens, the decrement and increment operators, so the bare form changes the program: `--a` subtracts one from `a` and stores it, where Go's `- -a` negates twice and leaves `a` alone, and `--1` or `--k` over a constant does not compile.

<!-- source: src/tests/Behavioral/UnarySignStack/main.go:16-17 + :38 + :41 -->
```go
a := 5
fmt.Println(- -a, a)
…
fmt.Println(- - -a, + + +a, a)
fmt.Println(- -1, + +1, - -k, + +k)
```
<!-- source: src/tests/Behavioral/UnarySignStack/main.cs.target:14-15 + :30-31 -->
```csharp
nint a = 5;
fmt.Println(- -a, a);
…
fmt.Println(- - -a, + + +a, a);
fmt.Println((nint)(- -1), (nint)(+ +1), (nint)(- -k), (nint)(+ +k));
```

The test is on the operand's EMITTED text, so it covers every way a sign can arrive first: a nested unary, a negative literal, or a parenthesized operand whose parentheses the emission drops. Every other shape keeps the bare form: `-a` itself, and the mixed `-+a` and `+-a`, which C# already reads as two operators. The same hazard exists for any type, keyword primitive or named wrapper, since a generated named-numeric type declares `++` and `--` too.

Guarded by: `UnarySignStack` (int, int8, float64, a named float and a named int type, constants, parentheses, three signs deep, and an assignment; each line prints the operand after the expression, so a mutation shows in the output comparison).

## A named complex type emits only Go's complex operator set
The generated named-numeric wrapper (`go2cs-gen` `InheritedTypeTemplate`/`NumericTypeTemplate`) emits the operator surface of the *underlying kind*, and Go's complex kinds define only `==`/`!=`, `+`/`-`/`*`/`/`, unary `-`, and `++`/`--` — **no ordered comparisons and no `%`** (the Go spec limits `<`/`<=`/`>`/`>=` to ordered types and `%` to integers; C#'s `System.Numerics.Complex` and golib `complex64` have neither operator either). A `type C complex128` therefore gets no `<`/`<=`/`>`/`>=`/`%` operators and no `IComparisonOperators` interface declaration — emitting them was **CS0019 ×5 per type** (first hit: `testing/quick`'s `TestComplex64Alias`/`TestComplex128Alias`, which compile-blocked the whole quick test host). Integer named types keep the full set including `%`/bitwise/shifts, and float named types keep ordering (and C#'s native float `%`, inert for converted Go, stays). Same kind-gate shape as the pre-existing complement/shift gate (`GetComplementOperator`). Guarded by the `NamedNumericIncDec` behavioral test's named-complex block (`++`/`--`/arithmetic/equality on a `type cx complex128`).

---

[← Native and Narrow Integer Types](native-and-narrow-integers.md) · [Index](README.md) · [Floating-Point Formatting →](floating-point-formatting.md)
<!-- {% endraw %} -->
