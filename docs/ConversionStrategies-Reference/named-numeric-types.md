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
[GoType("num:nint")]  partial struct level;   // type level int
[GoType("num:nuint")] partial struct Flags;   // type Flags uint
[GoType("num:float64")] partial struct Celsius; // type Celsius float64
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

(Guarded by `NamedNumericConversion`, `NamedNumericShiftConv`, `NamedTypeBitwiseConst`, `IotaEnum`, `FuncTypeParam`, and `CrossPkgUser`; the `string`-target exception is guarded by `StringConvPostfix` and `UnsafeOperations`; verified by the full behavioral suite — output comparisons confirm the precedence is unchanged.)

The same underlying routing applies when an untyped-constant **shift** is re-typed to a named numeric. An untyped shift `1 << k` is re-typed to the type it assumes from context (so it can combine with typed operands); when that resolved type is a *named* numeric, the re-type must go through the underlying — `(arenaIdx)((nuint)1 << k)`, not a bare `(arenaIdx)(1 << k)` (CS0030). The shift's *width* is likewise decided by the underlying (a `nuint`/`uint64`-backed named type shifts the left operand in that width to avoid the `int`-overflow seen for `1 << 63`). Non-named shifts are unchanged. (Guarded by the `NamedNumericShiftConv` behavioral test — wide `uint`/`uint64`-backed and narrow `uint8`-backed named types; runtime hits this on `arenaIdx(1 << arenaBits)`.)

The same coercion is needed where the converter itself inserts a C# `(int)` cast on a named-numeric value — a **slice bound** (`summary[sc+1:ec]` with `sc`/`ec` of type `chunkIdx`), a **shift count** (`1 << (d % 64)` with `d` of type `statDep`), or the **length of an `unsafe.Pointer`-to-array slice** (`(*[N]T)(ptr)[:n]` → `new slice<T>(new ReadOnlySpan<T>(ptr, (int)n))`, since the `ReadOnlySpan<T>` constructor takes a C# `int` — see *Slicing a pointer-to-array*). A bare `(int)(sc + 1)` is CS0030 for the same reason, so the converter emits `(int)(nuint)(sc + 1)` / `(int)(nint)(d % 64)` — through the named type's underlying basic; a plain `nint`/`nuint` length is narrowed `(int)(n)`. Plain basic operands keep the bare `(int)(x)` form. (Guarded by the `NamedNumericIntCast` behavioral test; the Span length by `StdLibInternalAbi`.)

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

<a id="an-integer-expression-over-a-gobigconst-constant-folds--it-has-no-64-bit-form"></a>Moved to [An INTEGER expression over a `GoBigConst` constant folds — it has no 64-bit form](constants.md#an-integer-expression-over-a-gobigconst-constant-folds--it-has-no-64-bit-form).

<a id="a-function-local-gobigconst-hoists-its-parse-to-a-static-readonly-field"></a>Moved to [A function-LOCAL `GoBigConst` hoists its parse to a `static readonly` field](constants.md#a-function-local-gobigconst-hoists-its-parse-to-a-static-readonly-field).

<a id="the--bit-clear-compound-assignment-on-a-narrow-type"></a>Moved to [The `&^=` (bit-clear) compound assignment on a narrow type](native-and-narrow-integers.md#the--bit-clear-compound-assignment-on-a-narrow-type).

<a id="a-standalone-x-on-uint8uint16-truncates-back-to-the-operands-type"></a>Moved to [A standalone `^x` on `uint8`/`uint16` truncates back to the operand's type](native-and-narrow-integers.md#a-standalone-x-on-uint8uint16-truncates-back-to-the-operands-type).

<a id="logical-operators-on-a-named-boolean-type-cast-through-bool"></a>Moved to [Logical operators on a named boolean type cast through `bool`](struct-types.md#logical-operators-on-a-named-boolean-type-cast-through-bool).

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

## A named complex type emits only Go's complex operator set
The generated named-numeric wrapper (`go2cs-gen` `InheritedTypeTemplate`/`NumericTypeTemplate`) emits the operator surface of the *underlying kind*, and Go's complex kinds define only `==`/`!=`, `+`/`-`/`*`/`/`, unary `-`, and `++`/`--` — **no ordered comparisons and no `%`** (the Go spec limits `<`/`<=`/`>`/`>=` to ordered types and `%` to integers; C#'s `System.Numerics.Complex` and golib `complex64` have neither operator either). A `type C complex128` therefore gets no `<`/`<=`/`>`/`>=`/`%` operators and no `IComparisonOperators` interface declaration — emitting them was **CS0019 ×5 per type** (first hit: `testing/quick`'s `TestComplex64Alias`/`TestComplex128Alias`, which compile-blocked the whole quick test host). Integer named types keep the full set including `%`/bitwise/shifts, and float named types keep ordering (and C#'s native float `%`, inert for converted Go, stays). Same kind-gate shape as the pre-existing complement/shift gate (`GetComplementOperator`). Guarded by the `NamedNumericIncDec` behavioral test's named-complex block (`++`/`--`/arithmetic/equality on a `type cx complex128`).

## An integer named-numeric wrapper implements the integer operator interfaces

A `[GoType num:]` wrapper (`type stringID uint64`) already declared the *common* numeric operator
interfaces so it could serve a `cmp.Ordered`-shaped constraint (`IAddition`/`ISubtraction`/
`IMultiply`/`IDivision`/`IEquality`/`IComparison`/`IIncrement`/`IDecrementOperators`), but the
*integer-only* three — `IModulusOperators`, `IBitwiseOperators`, `IShiftOperators<T, int, T>` —
were deliberately left off because their operators (`%`, `&|^~`, `<<`, `>>`) are kind-gated. That
left a named integer type unable to satisfy a converter-emitted `~integer` operator constraint:
internal/trace's `type dataTable[EI ~uint64, E any]` instantiated with `type stringID uint64` was
CS0315 ×48 on exactly those three interfaces. The `NumericTypeTemplate` operators already exist
(same kind-gate), so `InheritedTypeTemplate` now also *declares* the three integer interfaces for an
integer underlying (float/complex keep only the common set). `IShiftOperators` additionally requires
`operator >>>` (unsigned right shift) — added to the integer operator block; Go emits no `>>>`, but
the member is needed to satisfy the interface. Cleared internal/trace's 48 CS0315 (49→1, the residual
being the unrelated ΔLabel CS0542). Guarded by `NamedNumericOperatorConstraint` (a generic
`mix[K ~uint64 | ~int32]` applying modulus/bitwise/both-shifts on the type parameter, instantiated
with a named `uint64` and a named `int32`, values vs Go). Corpus-verified against math/big (Word),
archive/tar, and time (Duration).

## A named-numeric wrapper is `IComparable<T>` as well as ordered by operators

Ordering has two surfaces in .NET and the wrapper only carried one. `IComparisonOperators<T,T,bool>`
(above) serves a constraint lifted from `cmp.Ordered`; `IComparable<T>` is what the BCL's own
ordering binds — `Array`/`List.Sort`, `SortedSet<T>`, `Comparer<T>.Default` — and, decisively for
converted code, what golib's N-argument `min`/`max` are constrained on. (The two-argument forms take
`IComparisonOperators`, because a *type parameter* constrained by `cmp.Ordered` has no
`IComparable<T>` conversion; the `params ReadOnlySpan<T>` forms cannot, since a span element must
compare through a member, not an operator.) So a named numeric bound `min(a, b)` and failed
`min(a, b, c, d)`: `min(a-got, got-a, a-got+q, got-a+q)` over `crypto/internal/mlkem768`'s
`type fieldElement uint16` was CS0315, "no boxing conversion from `fieldElement` to
`System.IComparable<fieldElement>`". `InheritedTypeTemplate` now declares `IComparable<T>` on the
**same kind-gate** as `IComparisonOperators` — every numeric kind except complex, which Go orders no
more than C# does — and `NumericTypeTemplate` emits its single member inside the same gated block:

```csharp
public int CompareTo(fieldElement other) => m_value.CompareTo(other.m_value);
```

Forwarding to the *underlying* value's `CompareTo`, rather than writing the comparison out of the
wrapper's own `<`/`>`, is what keeps a named float on the BCL total order (NaN below everything) —
which is what makes `min` yield NaN when any argument is NaN, as Go's does. Every underlying a
`[GoType num:]` wrapper can name satisfies it: the aliases are BCL primitives, `uintptr` is a golib
struct that declares `IComparable<uintptr>` itself, and a wrapper over another wrapper picks up the
member this template gives it. The wrapper was already `IEquatable<T>`; this makes it ordered too,
matching the golib `uintptr` and `@string` structs, which are both. (Guarded by extensions to the
`MinMaxBuiltin` behavioral test — `min`/`max` at two and four arguments over named unsigned,
floating and signed underlyings, values vs Go; the pre-fix generator is CS0315 ×10 across the three
kinds.)

## Integer wrappers carry the UntypedInt bridge

- **Integer wrappers carry the UntypedInt bridge** (`(token)(endBlockMarker)` — C# never
  chains two user conversions, CS0030). Guarded by `SortArrayType` (`levelToken`).

## Short declarations keep the named-numeric cast

- **Short declarations keep the named-numeric cast** (`p := printFlags(0)` re-imposes
  `((printFlags)0)`, CS1503)

---

[← Native and Narrow Integer Types](native-and-narrow-integers.md) · [Index](README.md) · [Floating-Point Formatting →](floating-point-formatting.md)
<!-- {% endraw %} -->
