# Constant Values
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#constant-values)
Go constants hold arbitrary-precision literals with expression support, and assignment of a constant to a variable happens at compile time. The converter preserves the constant value (and, in a comment, the original expression). A *typed* Go constant is emitted with its concrete C# type, e.g.:

```csharp
public const nint MaxRetries = 3;
```

An *untyped* Go constant is emitted using a golib "untyped" wrapper type — `UntypedInt`, `UntypedFloat`, or `UntypedComplex` — so it can hold a value that does not fit a single primitive and can implicitly adapt to whatever numeric type its use site requires (mirroring how an untyped Go constant takes its type from context). A `[GoType]` struct is not a legal C# constant type, so the declaration is a get-only **property** rather than a field (see the next subsection for why):

```csharp
internal static UntypedInt win => 100;
public static UntypedInt N => /* 11 + 1 */ 12;
```

## A constant C# cannot declare `const` is a get-only PROPERTY, not a `static readonly` field

A Go constant has **no initialization**: it is a compile-time value usable from anywhere in the
package regardless of declaration order. Whenever C# *can* say `const` (a primitive-typed const) that
property is preserved for free. It cannot for the wrapper types above, nor for a named type, a
`uintptr`, or a complex — and a `static readonly` **field** reintroduces exactly the initialization
Go does not have. C# runs static field initializers in class-**textual** order (across a partial
class, in `<Compile>`-item order), so a package-level variable declared *ahead* of the constant read
it as the type's DEFAULT — silently, with no diagnostic:

```go
var fixedHuffmanDecoder huffmanDecoder    // compress/flate/inflate.go — declared FIRST
...
const huffmanNumChunks = 1 << huffmanChunkBits   // …the const it transitively needs comes LATER
type huffmanDecoder struct { chunks [huffmanNumChunks]uint32 }
```

As a field, `huffmanNumChunks` was still `0` when that variable's `new huffmanDecoder()` ran its
`chunks = new(huffmanNumChunks)` field initializer, so the decode table was allocated at length **0**
where Go says 512: `init` filled nothing (its `for off < len(h.chunks)` loop never ran) and every
later `chunks[i]` read panicked with `index out of range [281] with length 0`. The same trap zeroed
`maxNumLit` for `fixedLiteralEncoding`'s initializer *across files*, emptying the fixed Huffman code
table. Both are silent-correctness defects that compiled clean, and both took down every dependent
package (`compress/gzip`, `compress/zlib`).

A get-only property carries no initialization at all, so declaration order cannot be observed and the
JIT folds the literal at each use — Go's semantics exactly:

```csharp
internal static UntypedInt huffmanNumChunks => /* 1 << huffmanChunkBits */ 512;
public static ΔKind Uintptr => 12;                        // named-type const
internal static uintptr MaxUintptr => unchecked((uintptr)18446744073709551615);
```

**RESIDUE — the two ALLOCATING const forms stay `static readonly` fields**: `@string` (whose `u8`
literal the string-literal arc deliberately hoists to a single allocation) and `GoBigConst` (a
`BigInteger.Parse`). A property would rebuild their value on every read. Neither can serve as an
array length, so neither reproduces the failure class above — but a package-level variable
initializer that reads one *before* its declaration point still would, so the residue is handed to
the [initialization-order pass](variable-initialization-order.md#a-constant-emitted-as-an-initialized-field-is-an-initialization-dependency-too),
which draws a relocation edge for exactly these two forms and nothing else.

The two rules divide the problem cleanly and neither is redundant: the property form takes every
numeric, named-numeric, `uintptr`, complex and untyped constant *out* of the ordering problem, and
the relocation edge orders the string/`GoBigConst` residue that has to stay in it. Guarded together
by the `PackageVarInitOrder` behavioral test (cross-file constants consumed by earlier-declared
vars — directly, through a struct's fixed-array field initializer, and through a constant-folded
initializer over a named-string const).

**Wrapper conversions are VALUE conversions in every direction.** `UntypedInt` stores its payload as
`int64` bits (so a `ulong`-range literal like `9223372036854775808` round-trips through the same 8 bytes),
and its float/complex operators originally *bit-reinterpreted* that payload — `var fl float64 = m` (m an
untyped `3`) produced `1.5e-323`, the denormal double whose bit pattern is 3, instead of `3`. The
float32/float64/complex64/complex128 operators now convert by **value**, like every integer-direction
operator always did. Because the payload can be unsigned-flavored (a beyond-int64 `ulong` literal such as
`1 << 63` uses the `uint64` constructor — bits identical to a signed `-9223372036854775808`), the wrapper
carries a payload-kind discriminator so a float conversion keeps the uint64 magnitude and `ToString()`
prints it unsigned. (Guarded by the `UntypedIntFloatContexts` behavioral test — one local const used in
both int and float contexts, the `1<<63` unsigned payload, complex contexts via `real`/`imag`, and a
negative payload, values verified vs Go.)

**COMPARISON is a value comparison too, across payload kinds.** The `(payload, unsigned-flavour)` pair
denotes a mathematical integer, and `<`/`<=`/`>`/`>=`/equality must order over that VALUE — but they
compared the raw `int64` bits, so a `uint64` at or above 2^63 (which reads as a NEGATIVE `int64`)
answered `u < someSmallConst` **TRUE**. That is a silent wrong answer for the whole
`if fastSmalls && i < nSmalls` idiom the standard library uses: `strconv.FormatUint(9223372036854775808, 10)`
took the small-integer fast path, `small()` truncated the negative index, and the result was `"0"`
(`TestUitoa`); the varlen sibling threw on the truncated slice bound (`TestFormatUintVarlen`). The
operators now route through one `Compare` helper: same flavour compares at that flavour, and mixed
flavours put a NEGATIVE signed payload below every unsigned payload (otherwise both are non-negative
and the unsigned reading of each is exact). Only a payload at or above 2^63 changes answer — every
signed-only or in-`int64`-range comparison is bit-for-bit what it was. (Guarded by the
`BigUntypedConstComparison` behavioral test's wrapper arm — `0`/`999`/`1000`/`1001`/`1<<62`/`1<<63`/
`1<<64-1` against a wrapper const across all six relations, the signed counter-cases, and the inlined
`FormatUint` fast-path predicate, output-compared vs Go; the test FAILS `[Output]` without the fix.)

**The wrapper carries 64-bit shift operators so a still-wrapped untyped shift keeps Go's width.** `UntypedInt` defines `<<` and `>>` (an `int` count) returning `UntypedInt`, shifting the `int64` payload. Without them, a wrapper-typed untyped constant shifted by a **non-constant** count bound through the implicit `UntypedInt → int` conversion — a 32-bit `int` shift, which both masks the count to its low 5 bits and truncates the payload to 32 bits — whereas Go shifts at the type the untyped operand assumes from context (typically the enclosing `uint64`). `math.Frexp` corrupted on exactly this: `x |= uint64((-1 + bias) << shift)` (`bias` a package-level `UntypedInt` const, so the compound `-1 + bias` stays `UntypedInt`; `shift` a runtime `int`, up to 52) emitted `((-1 + bias) << (int)(shift))` and computed `1022 << (52 & 31)` = `1022 << 20` instead of `1022 << 52`, scrambling the exponent field of the assembled `float64`. The operators keep the shift a 64-bit `long << int` returning `UntypedInt`, so it composes with the surrounding `(uint64)` conversion and reproduces Go's value. Note this fires only for a **compound** untyped operand (`bias - 1`): a bare-identifier operand (`bias << n`) is cast to the shift's context result type at the use site first, so it never reaches the wrapper operator. (Guarded by the `UntypedIntWideShift` behavioral test — package-level `UntypedInt` consts `bias`/`bits`, the compound `bias - 1` left-shifted and `bits - 1` right-shifted by runtime counts 52/40/33 (all > 31, so a 32-bit narrowing would mask them), values verified vs Go.)

**Float constant values are emitted exactly.** The emitted value of a float-kind constant declaration is never `go/constant`'s `Value.String()` — that is a *shortened* human-readable form (~6 significant digits), and using it silently truncated the **compiled** value while the exact literal survived only in the `/* … */` comment (math `cbrt`'s `C = /* 5.42857142857142815906e-01 */ 0.542857`). The emission prefers the Go **source literal verbatim** when it is also valid C# syntax — decimal floats including exponents and `_` digit separators overlap C# exactly, and a unary-minus form (`-7.05306122448979611050e-01`) carries its sign — which also elides the now-redundant original-expression comment (the emitted value *is* the original). When no single valid literal exists — a folded constant expression (`19.0 / 35.0`), or a Go-only literal form (hex float `0x1p-2`, trailing-dot `5.`) — the value emits as the **shortest round-trip** form (`strconv.FormatFloat 'g'/-1`) of the constant converted at the declaration's width (bitSize 32 for a `float32`-typed const, so the `f`-suffixed single parses with the same one rounding Go applies; 64 otherwise):

```go
const (
    C              = 5.42857142857142815906e-01 // 19/35 = 0x3FE15F15F15F15F1
    D              = -7.05306122448979611050e-01
    folded         = 19.0 / 35.0
)
```
```csharp
internal static readonly UntypedFloat C = 5.42857142857142815906e-01;
internal static readonly UntypedFloat D = -7.05306122448979611050e-01;
internal static readonly UntypedFloat folded = /* 19.0 / 35.0 */ 0.5428571428571428;
```

A beyond-float64 value still routes to the `GoBigConst` (BigInteger) overflow path unchanged. (Guarded by the `UntypedConstDefine` behavioral test — package-level and function-local high-precision consts printed and compared against Go, which fails with the truncated `0.542857` emission; `SortArrayType` additionally locks the verbatim forms `1.0f`/`3.14e100`.)

**A FUNCTION-BODY float const referencing a named untyped-float const folds the same way (2026-07-18).** The exact-emission above is a *declaration* rule; the identical double-rounding hazard exists for a compile-time float constant computed in a FUNCTION body that references a named untyped-float const (`math.Pi`, `math.Ln10` — each emitted as a golib `UntypedFloat` wrapper already rounded to float64). Left as runtime C# arithmetic, `float64(100000 * Pi)` becomes `(float64)(100000D * Pi)` and rounds a SECOND time — 314159.2653589793, a ULP below Go's single arbitrary-precision fold 314159.26535897935 (`math`'s `TestLargeCos` was fed a −1-ULP argument; the trig algorithm itself is bit-exact), and `1 / Ln10` in `Log(x) * (1/Ln10)` the same. `foldedNamedFloatConstLiteral` emits the Go-folded value as a `/* <expr> */ <literal>` at the RESOLVED float width, at two sites: a `float64(…)`/`float32(…)` type CONVERSION (`convCallExpr`, restricted to a *basic* float target so a named float type keeps its `[GoType]` wrapper path) and a computed-const OPERAND being cast to a concrete float in a binary expression (`convBinaryExprCore`, reusing the sibling's resolved type). The width is honored exactly — a float32 target rounds the exact constant STRAIGHT to float32 (`exactFloatText`'s `constant.Float32Val`), never through a float64 intermediate, which would double-round *differently* than Go's single round-to-float32. Gated to a COMPUTED float const that references a named untyped const: a bare named-const reference already renders as a single-rounded wrapper, and a pure-literal float const (`1.5 * 2.0`, no named ref) computes exactly in C# double and keeps its readable operator form. Cleared `math`'s `TestLargeCos/Sin/Tan/Sincos` + `TestLog10` (68 → 73 / 77) by folding 34 sites across the sin/tan/atan/erf/jN/lgamma/pow families; the emission is a no-op on the behavioral corpus (nothing there exercises the pattern). (Guarded by the `NamedConstFloatFold` behavioral test — a `float64(100000*myPi)` conversion, its float32 counterpart, and a `1/myLn10` typed-float64 operand, output-compared vs Go.)

**Function-local untyped constants TIGHTEN to their single concrete use type.** A per-function analysis pass (`performUntypedConstAnalysis`) resolves every use of each function-local untyped numeric constant through go/types: when ALL uses record the SAME concrete basic type — and none participates in constant folding — the declaration emits at that type (with C#'s `const` keyword where legal: the primitive aliases; native-int/`uintptr` values fall to the existing `static readonly`/`unchecked` demotions), and every cast the wrapper made necessary (the bitwise/arith operand casts, the `append`/deferred-call element casts, the 32-bit-and-wider shift retype) is skipped as redundant. One cast is **kept because it is value-changing, not wrapper-driven**: a tightened const of a *sub-int32* type (`int8`/`int16`/`uint8`/`uint16`) as the LEFT operand of a non-constant shift keeps the width retype — C# promotes a narrow shifted operand to `int`, so without `(byte)(cb << (int)(k))` Go's wraparound at the declared width is lost (`const cb = 200; b + cb<<k` → Go wraps `200<<1` to 144, byte width, result 145; the promoted C# shift computes 400 → 401). The emitted code otherwise reads like the Go source — math `cbrt`:

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

The guards are deliberately conservative — any doubt keeps today's `Untyped*` wrapper form:

- **Function-local only** (package-level constants keep the wrapper: they are visible across functions whose contexts may differ, including from other files of the package).
- **Untyped integer/rune/float/COMPLEX**, excluding any value on the `GoBigConst` (BigInteger) path. (Untyped complex was excluded while `complex128` had no non-`const` emission; a representable complex const now emits `static readonly` like `uintptr`, and the wrapper it replaces is actively *wrong* for it — see *A complex constant emits a complex VALUE* below. A complex use type tightens only an untyped-COMPLEX const: the converter never *widens* an untyped integer const to `complex128` because one use happens to be complex-typed.)
- **Every use must record a concrete numeric basic type** in go/types' `Info.Types`, and all uses must agree on ONE basic kind. go/types records the implicit-conversion target for an untyped operand (`float64` for `C + x` with `x float64`), so a use that stays untyped (another constant's initializer), resolves to a NAMED type or type parameter, or records a non-numeric type (`string(c)`) disqualifies — as do mixed concrete types (`float64` and `float32` uses of one constant).
- **No use may participate in constant folding**: an ancestor expression carrying a folded constant value (`uint64(B1) << 32` — cbrt's B1/B2 stay `UntypedInt`) disqualifies. Go folds untyped constant expressions at arbitrary precision; re-expressing an operand at a concrete C# type could change the folded result (or re-fold it in C#'s checked int32 arithmetic).
- The exact value is re-checked representable in the resolved type (belt-and-braces — go/types already validated each use's conversion).

A tightened constant composes with the exact-float emission above (the cbrt literals round-trip to their documented bit patterns, e.g. `C` ↔ `0x3FE15F15F15F15F1`), with the `iota` initializer (a position-0 `= iota` tightened to nint emits golib's constant bare — see the bare-iota rule below; any other tightened type keeps the folded value with the `/* iota */` comment), and with a float-KIND value under an INTEGER tightened type — `const infinity = 1e6` (go/printer; `1e6` lexes as a float literal) used only in int contexts emits the integer form `const nint infinity = 1000000;`, since a C# `1e6` double literal has no implicit conversion to nint and the tightening pass guaranteed integral representability. (Guarded by the `UntypedConstDefine` behavioral test's `tightenGuards` — single-type/append/defer/shift-operand uses tighten, mixed-type/const-feeding/folding uses keep the wrapper, and the narrow byte/int16/uint16 shifted consts keep the width retype (145, not 401), all output-compared vs Go — and by `BitwiseUntypedConst`, whose local `signBit = 1 << 63` now emits `const uint64` with the `(uint64)` operand casts dropped; `ConstShadowsParam` locks the shadow-rename interplay, its folded `int64(ns)` uses staying untightened.)

## A complex constant emits a complex VALUE, rendered from its two exact halves

A COMPLEX-kind constant is emitted as a real complex value built from its real and imaginary parts, each rendered by the same exact-float machinery a float const uses (`exactFloatText`) and recombined in the postfix `.i()` form `convBasicLit` already emits for written imaginary literals. Because `complex128` is `System.Numerics.Complex` and `complex64` is a golib struct — and C# forbids `const` of a library struct (CS0283) — the declaration is `static readonly`, the same demotion `uintptr` takes:

```go
const (
    cRational   = 5.5 + 1.5i
    cNegImag    = 2.25 - 0.75i
    cPureImag   = 3i
    cWideEnough = 1.5e308 + 1.0e307i
    cFolded     = (1 + 2i) * (3 + 4i)
)
const c64 complex64 = 1.5 + 2.5i
```
```csharp
internal static readonly UntypedComplex cRational = /* 5.5 + 1.5i */ 5.5D + 1.5D.i();
internal static readonly UntypedComplex cNegImag = /* 2.25 - 0.75i */ 2.25D + -0.75D.i();
internal static readonly UntypedComplex cPureImag = /* 3i */ 3D.i();
internal static readonly UntypedComplex cWideEnough = /* 1.5e308 + 1.0e307i */ 1.5e+308D + 1e+307D.i();
internal static readonly UntypedComplex cFolded = /* (1 + 2i) * (3 + 4i) */ -5D + 10D.i();

internal static readonly complex64 c64 = /* 1.5 + 2.5i */ 1.5F + 2.5F.i();
```

The receiver's `F`/`D` suffix selects the golib `i()` overload (`i(this float)` → `complex64`, `i(this double)` → `complex128`) exactly as a written literal's does, and each half's implicit float→complex conversion closes the `+`. A ZERO real part renders as the bare imaginary literal (`3D.i()` — the Go source form of `3i`); a NEGATIVE imaginary part composes as written, because member invocation binds tighter than unary minus, so `2.25D + -0.75D.i()` is 2.25 + −(0.75·i).

**Why the halves are tested individually.** Representability was previously decided by handing `go/constant`'s `Value.ExactString()` to `strconv.ParseComplex`. That text is the parenthesized RATIONAL form — `5.5+1.5i` is `(11/2 + 3/2i)` — which is neither C# syntax nor a form `ParseComplex` accepts (its grammar is Go *literal* syntax: no parentheses, no spaces around the sign, no `p/q`). The test could therefore never succeed: **every** complex constant, however ordinary, was classified as beyond-`complex128` and emitted through the `GoBigConst` arm — whose `BigInteger.Parse` cannot represent a complex at all. `strconv`'s `atoc_test.go` (`const want = 1.5e308 + 1.0e307i`, a value that fits `complex128` with room to spare) failed to compile on `c != want` (CS0019, `Complex` vs `GoBigConst`). Each half is now rendered and range-tested on its own at the declaration's element width; only a genuinely unrepresentable value keeps the `GoBigConst` arm, and that emission now **warns**, because it is knowingly lossy.

A FUNCTION-LOCAL untyped complex const additionally tightens to its single concrete use type (the tightening pass above), which the wrapper form cannot substitute for: `UntypedComplex` converts implicitly to *and* from `complex128`, so comparing a wrapper-typed const against a `complex128` is AMBIGUOUS (CS0034) — `atoc_test`'s `TestParseComplexIncorrectBitSize` is exactly that shape, and it emits `complex128 want = …`. (Guarded by the `ComplexConstContext` behavioral test — rational halves, a negative imaginary part, a pure imaginary, the beyond-1e308-real strconv shape, a folded complex expression, a `complex64`-typed const, and a function-local const, all output-compared vs Go.)

**A const initialized by exactly the builtin `iota` emits golib's constant bare when it can express the value.** golib's builtin declares `public const nint iota = 0` (`golib/builtin.cs`), so the initializer emits as bare `iota` — instead of the folded comment form `/* iota */ 0` — only when BOTH halves of that declaration match: the folded group-position value is `0` (position 0 of the Go const group) AND the emitted C# type accepts golib's `nint` constant, i.e. the emitted type *is* `nint` (an explicit Go `int` type, or a function-local untyped const tightened to it) or the `UntypedInt` wrapper (implicit from nint). Everything else keeps the folded form: a LATER group position folds to a value golib's constant cannot express (`x = iota` at position 1 emits `/* iota */ 1` — on the `UntypedInt` path a bare `iota` there would even compile, silently at the WRONG value), and any other emitted type — named wrappers (`ΔKind Invalid = /* iota */ 0`), other widths (`int64`) — keeps `/* iota */ N` rather than casting golib's nint. (No current emission path casts an in-range position-0 const, so no `(T)iota` form exists; should one ever require a cast anyway, the cast would wrap `iota` rather than the folded value.) The identifier must resolve to the *universe* `iota` — a user-shadowed `iota` keeps the folded value. From compress/flate's `huffmanBlock` states:

```go
const (
    stateInit = iota // Zero value must be stateInit
    stateDict
)
```
```csharp
const nint stateInit = iota; // Zero value must be stateInit
const nint stateDict = 1;
```

(Guarded by the `IotaEnum` behavioral test — position-0 bare iota at explicit `int` and via local tightening, the `int64` mismatch both explicit and tightened, and later positions on both the nint and `UntypedInt` paths; the untyped later-position `rawOne = iota` case also locks the value fix — the prior emission referenced golib's `iota` (0) for a position-1 constant (1) — and the named-wrapper enum stays folded; all output-compared vs Go.)

A Go untyped *float* constant defaults to `float64`, so its C# literal carries the double suffix `D` — not `F` — regardless of whether the value happens to fit in `float32`. (Emitting `F` whenever the value fit would make `z := 1.0` a `float`, breaking later `float64` arithmetic with CS0266.) A literal in an explicit `float32` context keeps `F`:

```go
z := 1.0           // untyped float -> float64
var f float32 = 2.5 // float32 context
```
```csharp
var z = 1.0D;
float32 f = 2.5F;
```

The `F`-vs-`D` decision needs one more step **inside a constant expression**: go/types resolves the contextual type on the *outermost* constant expression only (its `updateExprType` deliberately never descends into constant operands — they never materialize at runtime in Go), so the inner literals of `var b float32 = -3.5`, of `complex(2.5, -3.5)` in a `complex64` context, or of `-(1.5 + 2.0)` stay recorded `untyped float` and would fall back to the `D` default — emitting `-3.5D` where C# needs `-3.5F` (no implicit double→float32/complex64 conversion: CS0266/CS0019). Since the emitted C# preserves the operand structure, the converter re-propagates the resolved type down the constant shapes go/types dropped it from — parens, unary `+`/`-`, arithmetic binary operands, and the `complex`/`real`/`imag`/`min`/`max` builtin arguments, each mapping the context appropriately (a `complex64` result makes `complex(…)`'s arguments `float32`; a `float32` result makes `real(…)`'s argument `complex64`) — via `markUntypedConstContexts` (`untypedConstOperations.go`), which the literal emitter consults when the literal's own recorded type is untyped:

```go
var c64 complex64 = complex(2.5, -3.5)
var c64b complex64 = 2.5 - 3.5i
var a, b float32 = 2.5, -3.5
```
```csharp
complex64 c64 = complex(2.5F, -3.5F);
complex64 c64b = 2.5F - 3.5F.i();
float32 a = 2.5F;
float32 b = -3.5F;
```

An **imaginary literal** is emitted in POSTFIX form — Go `3.5i` becomes `3.5D.i()`, the closest C# rendering of the Go literal — via golib extension methods on the suffixed real literal. The receiver suffix drives the overload choice: `…F.i()` (`i(this float)`) returns a `complex64` and `…D.i()` (`i(this double)`) a `complex128`, so the suffix follows the literal's resolved complex type per the same propagated context — replacing the earlier fits-in-float32 heuristic, which routed `0.1i` in a `complex128` context through `complex64` and silently lost precision (`(double)(0.1f) != 0.1`). The postfix form exists because a bare `i(…)` call is poisoned by Go's single most common identifier: a local or parameter named `i` in scope binds the bare call instead of the using-static golib helper (encoding/gob `encComplex`'s `i *encInstr` parameter, `c != 0+0i` → CS0149; C# block-scope rules make even a later-declared local poison an earlier bare call, CS0135/CS0844). Member access cannot bind a local, needs zero scope analysis, and — since the F/D suffix is emitted unconditionally — the receiver always lexes as a real literal (`0D.i()`, `.25D.i()`, `1e2D.i()` all parse; a suffixless `3.i()` would not). Member invocation also binds tighter than unary minus, so `-3.5D.i()` is `-(3.5i)` exactly as in Go, down to the negative-zero real part Go prints as `(-0-3.5i)`. The prior solution was the class-qualified `builtin.i(3.5D)` — equally shadow-immune, replaced for readability; the static call form remains valid on the `this`-modified overloads. (Guarded by the `UntypedConstFloatContext` behavioral test — the shapes above plus nested parens, quotient operands, `min`/`max`, `real`/`imag` round-trips, and a named-`float32` context, values verified vs Go — by `ComplexImaginaryShadow` — the gob-shaped shadowing parameter — and by `ComplexFormat` — complex printing round-trips.)

**A golib companion to the above: `UntypedFloat`'s conversions to a complex type are EXPLICIT, not implicit.** An untyped float constant emits as a golib `UntypedFloat` (see the untyped-wrapper section), and multiplying it into complex arithmetic — Go's `1i * math.Pi`, which the imaginary rule renders `1D.i() * math.Pi` (`Complex * UntypedFloat`) — bound **ambiguously** while `UntypedFloat` converted *implicitly* to both `float64` and `complex128`: the complex operand could bind as `Complex * double` (untyped → double) OR as `UntypedFloat * UntypedFloat` (the complex → `UntypedFloat` implicit conversion), and C# prefers neither (CS0034). The fix keeps `UntypedFloat`'s float↔complex relationship explicit in **both** directions (`UntypedFloat.cs`), so such arithmetic resolves cleanly to `Complex * double` — mathematically identical, since the untyped operand is a real value. This is compile-time only (an untyped float already converts implicitly to its natural `float64`; the widening to complex is the rarer direction, where the converter emits an explicit cast). Adding `Complex`-typed operators to `UntypedFloat` instead was rejected — it *reintroduces* ambiguity (`UntypedFloat / int` then matches both `UntypedFloat op UntypedFloat` and `Complex op UntypedFloat` via `int`'s dual conversions). This unblocked `math/cmplx`'s example build. (Guarded by the same `UntypedConstFloatContext` test's `1i * gPi` / `gPi * 2i` / `1i + gPi` lines — a package-level untyped const, so the operand emits as `UntypedFloat`; the pre-fix golib fails these at compile with CS0034.)

**An untyped INT literal resolved to a floating type takes the same F/D suffix (2026-07-18).** The propagation above marks the *integer* argument `0` of `complex(0, gHalfPi)` with the `complex128` element context (`float64`), but `convBasicLit`'s int-literal arm formerly ignored that mark and emitted a bare `0` (a C# `int`). golib's `complex` builtin has both a `complex(float32, float32) → complex64` and a `complex(float64, float64) → complex128` overload, and C# rates the `int → float` argument conversion *better* than `int → double` — so `complex(0, gHalfPi)` (where `gHalfPi` is a package-level untyped float, implicitly convertible to either width) bound the **complex64** overload and recomputed `gHalfPi` at float32: `math/cmplx`'s `Atanh` of an infinite input returned `float32(π/2)` = 1.5707963705062866 where Go's `complex(0, math.Pi/2)` is float64 π/2 = 1.5707963267948966. The int-literal arm now renders a context-resolved literal at its float type (`complex(0D, gHalfPi)` → the `float64` overload), exactly as the float and imaginary arms above already do; an int has no fractional part, so its exact digits plus the `F`/`D` suffix suffice. A `complex64` context is unaffected — the float32 overload is the intended one there. The *directly*-typed float case (a `[]float64{74, …}` element) is the companion rule immediately below. One operand kind is deliberately **excluded**: an integer-kind operand of a `/` is left unmarked, because Go integer-divides untyped-int operands even in a float context (`7 / 2` is `3`, then `3.0` — not `3.5`), so pushing a float type onto it would silently switch C# to float division. A float-kind operand of the same `/` (`math.Pi / 2`, already a float division) keeps the context and its `D` suffix; the exclusion is per-operand (`isIntegerKindConstExpr` in the QUO arm of `propagateUntypedConstContext`), matching the exact-rational reasoning that already bars an *integer* context from crossing `/`. This cleared `math/cmplx`'s `TestAtanh`. (Guarded by the `ComplexConstContext` behavioral test — `complex(0, gHalfPi)`'s imaginary part as float64 π/2, plus `7 / 2 == 3` and `complex(7/2, 0)`'s integer-quotient real part, vs Go.)

**The mirror case: an IMAGINARY literal resolved to a REAL float type emits its real part, NOT `.i()` (2026-07-24).** Go permits an untyped complex constant whose imaginary part is ZERO (`0i`, value 0) to convert to a `float` parameter: `complex(math.NaN(), 0i)` — internal/fmtsort's `sort_test.go` `complex128`-key map — where `complex()`'s second parameter is `float64`. go/types records the literal's type as that `float64` (the converted type), and its REAL part (0) is what must be emitted. `convBasicLit`'s `token.IMAG` arm formerly emitted `.i()` unconditionally, producing `0D.i()` — a `System.Numerics.Complex` where the golib `complex(double, double)` overload wants a `double`, so C# reported CS1503 on BOTH arguments (`double`→`float` on arg 1, `Complex`→`float` on arg 2, as it fell back to the `complex(float, float)` candidate). The arm now checks the RESOLVED type: when it is a float (`types.IsFloat`), the real part is rendered as a plain `D`/`F`-suffixed literal (`0D`) exactly as the float and int arms do; only a complex resolved type takes the `.i()` form. Only `0i` can reach this arm — a nonzero imaginary constant is not representable as a real float, so go/types never records one with a float type — but the emission is driven off the resolved type, not that fact. This was the sole compile blocker for internal/fmtsort. (Guarded by the `resolveBuildTags`/`convBasicLit` converter unit coverage and the `ComplexConstContext` behavioral test's `complex(_, 0i)` real-context line, values vs Go.)

**A DIRECTLY float-typed int literal takes the suffix too — and an overflowing one MUST (2026-07-20).** The rule above keys off the *propagated* context (`untypedConstContexts`), which the propagation walk records only for the constant shapes it descends — parens, unary sign, arithmetic/shift operands, and the `complex`/`min`/`max`/`real`/`imag` builtin arguments. A great many int literals reach a float type by a route the walk never touches: go/types types them **directly** as a non-untyped float — a `float64`/`float32` **composite-literal element**, a typed **const**, a **function argument**, a **return** value, or an **assignment** RHS. For those, `info.Types[lit].Type` is already `float64`/`float32` (not `untyped int`), so no context mark exists and the int-literal arm formerly fell through to the ordinary integer emitter — a **bare** C# integer literal. For a small value that merely bound an int-typed overload where a float one was meant (the same latent hazard as the propagated case); for a **large** value it was a hard compile error. strconv's `ftoa_test.go` puts `123456789123456789123456789` in a `float float64` struct field; the bare 27-digit literal overflows **every** C# integral type — `error CS1021` "Integral constant is too large" — which was the sole blocker stopping the whole `strconv` test host from compiling in Phase 4. `intLiteralFloatKind` (`convBasicLit.go`) now consults **both** routes: the literal's *directly-resolved* non-untyped float/complex type first (a named type over `float64` resolves through `Underlying` to its `float64` kind, matching the FLOAT arm), then the propagated context. Either way the int-literal arm emits the same `F`/`D`-suffixed literal. The Go **source digits are preserved verbatim** whenever they also form a valid C# real literal (the visually-similar goal) — `123456789123456789123456789D` is a valid C# `double` literal that rounds to the *same* float64 the bare form overflowed on, so the emitted digits still read like the source. A radix-prefixed or legacy-leading-zero form cannot survive a pasted suffix (`0x10D` is the C# *hex integer* 269), so those re-render as the folded constant's exact **decimal** digits — the reuse of `isValidCSharpRealLiteral` (which already rejects those forms for the FLOAT arm) gates the choice. This also makes the previously-inconsistent element pair consistent: `[]float64{74, -784}` now emits `74D, -784D` (the negated sibling already rendered `-784D` via the propagated route). Value-identical throughout; the behavioral A/B footprint was 15 projects, every changed line an integer-form literal in a float/complex slot gaining a `D`/`F`. (Guarded by the `IntFormFloatConst` behavioral test — an overflowing `123456789123456789123456789` and a small `33909` in a `float64` field/var/return, a `float32` field, and a decimal-form control that stays byte-identical, output-compared vs `go run`; unfixed, the overflowing literal fails to compile with CS1021.)

**Go-only float literal forms re-render as decimal.** The suffix decisions above choose *what type* a literal is; independently, its *text* must be a form C# can parse. Both literal arms emit the Go **source text verbatim** whenever C# shares the form — the same visually-similar goal that keeps `0x4000` from flattening to `16384`, so `1.5e-3` stays `1.5e-3D` — but two Go float forms have no C# spelling and must re-render as the shortest round-trip decimal (`strconv.FormatFloat 'g'/-1`) of the go/types-folded constant:

| Go | Was emitted | Now |
|---|---|---|
| `0x1p-2` | `0x1p-2D` — CS1002, C# has no hex-float syntax | `0.25D` |
| `2.` / `1.e2` | `2.D` / `1.e2D` — C# requires digits after the point | `2D` / `100D` |
| `0x10i` | `builtin.i(0x10D)` — **silently 269** | `builtin.i(16D)` |

The imaginary row is the dangerous one: `0x10D` is a *valid* C# hex integer literal, so the pasted-on suffix changed the value with no diagnostic rather than failing to compile. An imaginary literal's mantissa is matched against `constant.Imag` of its folded (complex) value, never the whole constant.

The re-render rounds the **exact** constant straight to the literal's resolved width (`constant.Float32Val` for a `float32`/`complex64` context), never float64-then-narrow: `var j float32 = 0x1.0000010000000000001p0` is 1 + 2⁻²⁴ + a residue, so double rounding lands exactly halfway and ties-to-even *down* to `1`, while Go's single rounding sees the residue and rounds up to `1.0000001`. These forms are absent from the non-test stdlib corpus but routine in Go's own `_test.go` files (the math tests) and in user code. The shared predicate (`isValidCSharpRealLiteral`) is the same one the constant-declaration path above uses; it also rejects the Go-only *integer*-mantissa radix forms an imaginary literal can carry — octal `0o123i`, binary `0b101i`, and legacy leading-zero `0123i`/`0_123i` (octal-flavored source C# would re-read as decimal) — which re-render as their exact decimal value (`83D.i()`, `5D.i()`, `123D.i()`; guarded by the exotic-mantissa cases in `ComplexFormat`). (Guarded by the `GoOnlyFloatLiteralForms` behavioral test — hex floats, trailing-dot and `1.e2` forms in `float64`/`float32` contexts, hex-float/hex-integer/trailing-dot imaginary literals in `complex128`/`complex64` contexts, the double-rounding case above, and decimal controls proving verbatim round-trip; values verified vs Go.)

A native-sized integer constant (`nint`/`nuint`, including the `uintptr` alias) whose value does not fit a C# constant of that type — e.g. `const MaxUintptr = ^uintptr(0)` (= `0xFFFFFFFFFFFFFFFF`), a `ulong` literal that needs a *non-constant* `nuint` conversion — cannot be a C# `const` (CS0133/CS0266). It is emitted as `static readonly` with an `unchecked` cast instead (small native-int consts like `const nint iota = 0` stay `const`):

```csharp
public static readonly uintptr MaxUintptr = /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);
```

The same `unchecked` cast is emitted for a **named** constant declared over a *wide unsigned* underlying whose folded value overflows int32 — `const unknownClass = ^Class(0)` (x/text/unicode/bidi, `type Class uint`) and `const _m = ^Word(0)` (go/constant via math/big, `type Word uintptr`) both fold to the all-ones literal `18446744073709551615` (a C# `ulong`), which has no implicit conversion to the `[GoType]` wrapper struct (CS0266). The native-int-const detection, which previously fired only for a `uintptr` underlying, now also fires for `uint`/`uint64` underlyings, so the const emits `unchecked((Class)18446744073709551615)`. A **small** named const stays uncast (`const c = Class(5)` → `Class(5)`, an ordinary in-range constant conversion) — the cast is added only when the value is out of int32 range, so no other named-const emission churns. (Guarded by the `NamedNumericConstCast` behavioral test — a beyond-int32 `^Named(0)` over `uint` and over `uint64` plus a small in-range control, values verified vs Go; shared root, cleared go/constant and bidi one error each.)

**Numeric literal formatting is preserved** wherever Go and C# syntax overlap: hex (`0x4000`), binary (`0b1011`), and decimal literals — including `_` digit separators — emit with their original source text (`0x4000` never flattens to `16384`), keeping bit masks and addresses recognizable; required `U`/`UL`/`L` suffixes and casts compose with the preserved text (`0xFFFFFFFFU`). Go-only forms re-render as decimal: `0o…` octal has no C# syntax, and a legacy leading-zero octal (`0755`) would silently re-bind as decimal 755 in C#.

## Constant folds and narrowing casts

### A beyond-MaxInt64 integer literal in a `uint64` context emits `UL`

**A beyond-MaxInt64 integer literal in a `uint64` context emits a plain `UL` literal.** The emitter classifies an INT literal by parsed range, and a value above int64 (representable only unsigned — the `-Inf` bit pattern `0xFFF0000000000000`, `^uint64(0)`) previously *always* emitted `(nuint)0x…UL`. That prefix is the bridge needed when the literal's resolved type is Go `uint`/`uintptr` (C# `nuint` — a bare `ulong` literal has no implicit conversion to it, CS0266, while the non-constant unchecked `(nuint)` conversion compiles), but in a `uint64` context it is spurious: semantically wrong for a 64-bit target type and value-truncating on a 32-bit platform — `math.Float64frombits(0xFFF0000000000000)` emitted `Δmath.Float64frombits((nuint)0xFFF0000000000000UL)` while the int64-range `0x7FF0000000000000` emitted clean (the signed branch already consulted the resolved type). The emitter now checks the literal's resolved *underlying* type: `uint64` — including a named type over `uint64`, whose `[GoType]` wrapper converts implicitly from `ulong` — takes the plain `0xFFF0000000000000UL`; native-width unsigned targets keep the `(nuint)` cast. This also cleans the same pattern from stdlib constant tables on the next regen (crypto/sha512's K, crypto/des masks, nistec field elements). (Guarded by the `MathFloatBits` behavioral test — ±Inf bit patterns as `uint64` arguments plus var-decl, comparison-operand, and binary-mask contexts, values verified vs Go; the `BitwiseUntypedConst`/`NamedIntSignednessConv`/`ShiftPrecedenceUnsigned` goldens re-baselined to the cast-free form, and `LargeUintptrConst` pins the native-width path unchanged.)

### A constant expression whose SUBexpression overflows the target narrows once

**A constant expression whose SUBexpression overflows the target type narrows once at the whole expression.** Go evaluates constant arithmetic in **arbitrary precision** and requires only the FINAL value to be representable in the target type — a subexpression is free to overflow it, so `[]int32{1<<31 - 1}` is legal Go even though the inner shift is 2147483648. C# has no such rule: it would compute the operators in `int` and overflow at compile time (CS0220), which is why an out-of-int32-range constant subexpression FOLDS to a C# `long` literal (`2147483648L`) in the first place. That fold widens the WHOLE element rendering to `long`, and `long` converts implicitly to none of the narrower integer targets — so the emission must narrow back exactly once:

| Go | Was emitted | Now |
|---|---|---|
| `[]int32{1<<31 - 2}` | `2147483648L - 2` — CS0266 | `(int32)(2147483648L - 2)` |
| `[]uint32{1<<32 - 1}` | `4294967296L - 1` — CS0266 | `(uint32)(4294967296L - 1)` |
| `[]uintptr{1<<40 + 1}` | `1099511627776L + 1` — CS0266 | `(uintptr)(1099511627776L + 1)` |
| `bits & (1<<52 - 1)` (uint64) | `bits & (4503599627370496L - 1)` — CS0019 | `bits & ((uint64)(4503599627370496L - 1))` |

The narrowing applies to every integer target EXCEPT `int64`, whose C# `long` already *is* the widened width, and it fires at the emission itself — in the parenthesized `(type)(…)` form `wholeExprIsCastOfType` recognizes — so it reaches composite elements, arguments, and comparison operands, not only the assignment position the sibling `nativeIntConstCastType` covers (which keeps handling the out-of-int32-range values folded whole; the cast strings match, so neither re-wraps the other). Two scope restrictions carry over from that sibling: at least one OPERAND must itself fold to a `long` literal — a bare `1 << 40` (both operands small) emits as a 32-bit `1 << (int)(40)` whose count C# MASKS to 8, and casting *that* would convert a loud error into a silently wrong value — and the whole value must be int64-exact, so a `uintptr` past int64 range keeps its visible error rather than being masked. Requiring a folded operand also guarantees the *non*-folded operands compute exactly, since a shift is only left unfolded when its value fits int32, which bounds its count below 32. A subexpression that stays inside int32 therefore keeps its readable operator form (`1<<20 + 1` → `(1 << (int)(20)) + 1`), and a whole value already past int32 still folds outright (`1<<63 - 1` → `9223372036854775807L`).

Corpus effect: this repaired latent `ulong`-versus-`long` mismatches across crypto/aes, crypto/cipher, database/sql/driver, math/big, net/http, runtime, strconv, sync, and vendored chacha20poly1305, and made math/rand's `Int31n` compute in `uint32` exactly as Go does (it previously computed the same value in `long`). The `1<<31 - 1` / `1<<63 - 1` idiom is pervasive in Go's own `_test.go` files, where the shape is a hard compile blocker. (Guarded by the `ConstSubexprOverflow` behavioral test — int32/int16/uint32/uint64/uintptr/int elements, the int64 no-cast case, in-range controls, and assignment/explicit-conversion/argument positions, values verified vs Go.)

### The narrowing root can be a UNARY node

**The narrowing root can be a UNARY node, and the widening fold can be arbitrarily deep (2026-07-25).** Two shapes escaped the rule above because it only ever looked at a `*ast.BinaryExpr` and only at that node's two *direct* operands.

1. **A negated widened constant roots at a unary node.** go/types types only the ROOT of a constant operator expression and leaves its operands untyped (`updateExprType` stops descending once the node it is retyping is itself constant), so `[]int32{-(1<<31 - 1)}` records `untyped int` on the inner `1<<31 - 1` and `int32` only on the negation. There is no typed *binary* anywhere in the tree to hang the cast on, so nothing narrowed and the element emitted a bare `-(2147483648L - 1)`. strconv's `atoi_test` parseInt32 table — `{"-2147483647", -(1<<31 - 1), nil}` against an `int32` struct field — is exactly this (CS1503). `widenedConstExprCastType` now accepts a unary root too and `convUnaryExpr` applies the cast at its own emission, mirroring `convBinaryExpr`: `(int32)(-(2147483648L - 1))`. `^` takes the same treatment (`(int32)(~(2147483648L - 1))`) and an `int` target narrows to `nint`; the non-constant unary operators (`&x`, `<-ch`, `!b`) are excluded by the existing constant-value and integer-kind guards.
2. **The fold need not be a DIRECT operand.** The operator form is emitted over the operand *renderings*, so a subtree that does not fold itself still renders `long` when one of *its* operands does: in `[]int32{(1<<31 - 1) - 1}` the root's operands are `(1<<31 - 1)` (value in range, unfolded) and `1`, and only the grandchild shift folds — likewise `[]int32{1<<40>>20 - 1}`. `operandRendersWidenedFold` now descends the whole constant subtree instead of testing one level. Descent cannot over-report: because go/types leaves the operands of a constant operator expression untyped, no interior node ever carries a narrowing cast of its own, so exactly one cast is emitted at the root — the guard test pins `(int32)((2147483648L - 1) - 1)`, not a doubled form.

The unary root is also taught to the explicit-conversion path (`int32(-(1<<31 - 1))`), which returns the operand's own cast rather than doubling it — the same `wholeExprIsCastOfType` check that path already applied to a binary operand. (Guarded by the `ConstSubexprOverflow` extension — negated/`^`/deep-fold elements for `int32`, `int`, and an in-range `int16` control, plus a typed assignment, an explicit conversion, a struct-field table entry and two call arguments, values vs Go; counter-proven against the pre-fix converter, whose emission fails to compile with ten CS0266/CS1503 on exactly those positions.)

### A subexpression past INT64 under a NATIVE-WIDTH unsigned target folds

**A subexpression past INT64 under a NATIVE-WIDTH unsigned target folds with its own `(nuint)` cast (2026-07-20).** The narrowing above needs the whole value to be int64-exact, and the *fold* that produces its widened operand originally ran only under a plain `uint64` target — a native-width target (`uint`/`uintptr`, and any named type over them) was left with its visible error, since `nuint` has no implicit conversion from `ulong` and the fold could not name the target. Go's arbitrary-precision rule makes this shape ordinary in numeric code: math/big's `nat{0, 0, 1 + 1<<(_W-1), _M ^ (1 << (_W - 1))}` (`int_test.go`'s `TestQuoStepD6`, where `Word` is a named type over `uintptr` and `_W` is 64) has an inner `1 << 63` of 9223372036854775808 — past int64 entirely, so no signed `long` fold can carry it — while each element's own value is representable in `Word`. Left alone, C# computed the element in int32: `1 + (1 << (int)(63))` against a `Word` element (CS0029), and `(nuint)_M ^ (1 << (int)(63))` mixing `nuint` with `int` (CS0019).

The fold now covers `uint64` **and** both native-width spellings — Go `uint` renders as `nuint`, Go `uintptr` as golib's distinct `uintptr` struct — and carries the narrowing itself for the native-width pair, in the same parenthesized form `wholeExprIsCastOfType` recognizes as the `(nint)(…)` arm on the signed side:

| Go | Was emitted | Now |
|---|---|---|
| `[]Word{1 + 1<<63}` (`Word uintptr`) | `1 + (1 << (int)(63))` — CS0029 | `(nuint)(9223372036854775809UL)` |
| `[]uintptr{_M ^ (1 << 63)}` | `(uintptr)_M ^ ((1 << (int)(63)))` — CS0019 | `(nuint)(9223372036854775807UL)` |
| `[]uint{1 + 1<<63}` | `1 + (1 << (int)(63))` — CS0029 | `(nuint)(9223372036854775809UL)` |
| `[]uint64{1 + 1<<63}` | `9223372036854775809UL` | *unchanged* |

The cast is spelled `nuint` for both native-width targets rather than naming the target: it is the primitive C# native unsigned type, and it converts implicitly to golib's `uintptr` struct and to a `[GoType]` wrapper over `uintptr` alike — so one spelling covers `uint`, `uintptr`, and named types over either, with no target-name synthesis. The `uint64` emission is untouched (it already had an implicit conversion from `ulong`), so this is zero-churn on the existing corpus — CNR is byte-identical across all 434 behavioral projects. (Guarded by the same `ConstSubexprOverflow` behavioral test, extended with a named-`uintptr` `Word` type plus plain `uintptr`/`uint`/`uint64` elements of the beyond-int64 shape, values verified vs Go.)

See [Named Numeric Types and Constant Contexts](named-numeric-types.md#named-numeric-types-and-constant-contexts) for how these interact with native-int and named numeric types. See also [example](https://github.com/ritchiecarroll/go2cs/tree/master/src/archived/Examples/Manual%20Tour%20of%20Go%20Conversions/basics/numeric-constants).

### A signed constant expression outside `int32` emits the folded 64-bit literal

The unsigned named-numeric path above gets a width-cast operand, but a **signed** constant operator expression whose target is a plain builtin `int64` has no such cast, so C# would compute it in `int32` and overflow at compile time in checked mode (CS0220): `int64(1<<63 - 1)`, `var d int64 = 1<<40 + 7`, or `12345 * 1000000000 + 54321` passed to an `int64` parameter. Go evaluates each as a constant in its `int64` type. For a signed constant binary/shift expression whose folded value is **outside the C# `int32` range**, the converter emits the **folded 64-bit literal** (`9223372036854775807L`, `1099511627783L`, `12345000054321L`) instead of the operator form — correct, and self-contained. In-range constants are unchanged (they keep the readable `1 << k` form). (Guarded by the `UntypedConstArithmetic` behavioral test; runtime hits this in `mgcmark`/`netpoll`/`runtime1`.)

A signed fold whose resolved type is Go **`int`** (C# `nint`) additionally carries its own cast (2026-07-17): C# has no implicit `long`→`nint` conversion, so the bare `L` fold failed loudly at every **non-assignment** use — strings' SplitN test table puts `math.MaxInt / 4` in an `n int` struct field, and the composite-literal element emitted `2305843009213693951L` against the `nint` field (CS1503; the Phase-4 blocker-map row B7a, one site each in strings and bytes). The fold now emits `(nint)(2305843009213693951L)` — the *parenthesized* cast form the assignment path's `nativeIntConstCastType` already recognizes (`wholeExprIsCastOfType`), so assignments that previously received the whole-RHS wrap render **byte-identically** (the cast simply moves into the fold; `NativeIntWideConstAssign`'s `n = (nint)(144115188075855772L)` is unchanged). The value always fits — `nint` is 64-bit on all supported platforms — and the conversion is a runtime unchecked narrowing, never a C# constant expression, so no checked-context overflow arises. An *untyped*-int subtree keeps the bare `L` form (its enclosing context supplies the conversion), as does an `int64` target (`long` is already exact). (Guarded by the `NativeIntWideConstElement` behavioral test — composite-literal elements, call arguments, and a var initializer, values verified vs Go.)

The **in-range widened** sibling (2026-07-17; sort's test-suite conversion): a typed-`int` constant operator expression whose *value* fits int32 but whose emitted arithmetic an operand fold has widened to `long` — `maxswap: 1<<31 - 1` (sort_test `countOps`): the whole value (2147483647) is in range, so no whole-expression fold applies, but the *untyped* inner shift folds to a bare `2147483648L`, making the rendering `2147483648L - 1` — a C# `long` with no implicit conversion to the `nint` composite field (CS1503; assignments were equally unprotected, since `nativeIntConstCastType` also requires the whole value out of int32 range). `convBinaryExpr` now wraps such an expression in the same parenthesized cast at its own emission: `(nint)(2147483648L - 1)`, position-independent. The trigger is **shape-restricted** (`operandRendersWidenedFold`): an operand must be an *operator subtree* whose `overflowingConstLiteral` fold is non-empty — a named untyped-const *reference* of the same value renders as its `Untyped*` wrapper, which narrows itself at the use site (`maxInt - maxInt` stays unwrapped; wrapping it would churn green emissions). (Guarded by the `NativeIntWideConstElement` extension — the `1<<31 - 1` element, call argument, and assignment, plus the wrapper-operand control, values vs Go.)

**FLOAT literals in INTEGER contexts** render their integer form (2026-07-17; sort's test-suite conversion). A float literal *directly* typed integer by go/types has always folded (`math.Inf(1.0)` → `1` — the convBasicLit integer-form rule), but inside a constant operator expression the literal stays **untyped float** (go/types resolves the context on the outermost node only): search_test's tests table writes `{"descending 7", 1e9, …, 1e9 - 7}` against `n, i int` fields, and the element rendered `1e9D - 7` — a C# `double` against the `nint` field (CS1503). Integer contexts now **propagate** through `markUntypedConstContexts` exactly like float/complex ones, and a float literal whose propagated context is integer emits its exact integer form: `1000000000 - 7` — the arithmetic stays exact C# `int`, implicitly convertible everywhere. Two soundness gates: a non-integral literal (`1.5`) keeps its loud `D` form (`constant.ToInt` exactness), and **division does not propagate** an integer context — Go evaluates an untyped-float constant `/` in exact rational arithmetic, so a nested quotient may be transiently non-integral (`3.0 / 2 * 2` = 3) where folded operands would int-divide (`3/2*2` = 2), a silently wrong value; those trees keep the loud `double` rendering. (Guarded by the `NativeIntWideConstElement` extension — `1e9 - 7` and `5e8 * 2` elements and a `2e9 - 8` call argument, values vs Go.)

**UNSIGNED** constant expressions fold under a much narrower trigger (2026-07-03): every other unsigned shape already has a working mechanism — a *typed* unsigned shift gets the width-cast operand (`(uint64)1 << 40`), an int64-range untyped subtree is folded by the signed arm when recursion reaches it (`(281474976710655L) + arenaBaseOffset` in runtime `mranges`), and a named-const reference renders via its `Untyped*` wrapper (`(uintptr)m5 ^ 4` in runtime `hash64`). The one unfixable shape is an untyped constant **operator** subtree (a BinaryExpr) whose value exceeds **int64 entirely**: `1<<63` nested inside `(1 << 63) - 1` — go/types lands the uint64 conversion on the outermost constant node, so the inner shift stays untyped, no width cast reaches it, and C# computes it in int32. `int64((1 << 63) - 1 - (1<<63)%uint64(n))` (math/rand `Int63n`, CS0220) emits as `(int64)(9223372036854775807UL - (((uint64)1 << (int)(63))) % (uint64)n)`: the constant subtree folds to `UL`, the standalone *typed* shift keeps its readable width-cast form. Gated to plain-`uint64` underlying targets (`constExprHasBeyondInt64UntypedOperatorSubexpr`) — a native-width `uintptr` target would need a further cast the fold cannot safely synthesize, so that pre-existing caveat keeps its visible error. A first broader cut (any untyped subtree beyond int32, any unsigned target) regressed runtime's `hash64`/`mranges` by stealing exactly those already-working shapes — the narrow trigger is load-bearing. (Guarded by the `UntypedConstArithmetic` extension — the Int63n shape, value-compared vs Go.)

**`uintptr` was missing from the width-cast retype the paragraph above relies on** (2026-08-09). Everything there is conditioned on "a *typed* unsigned shift gets the width-cast operand (`(uint64)1 << 40`)", which is emitted by the shift retype in `convBinaryExpr` when `isWideShiftType` says the target does not promote to `int`. That predicate listed `uint32`/`uint64`/`int64`/`nuint` — and `uintptr` is the one wide unsigned type that does NOT render as a C# primitive: Go's `uint` becomes `nuint`, but Go's `uintptr` becomes golib's `uintptr` STRUCT. So it fell to the narrow arm, which casts the **result** — precisely the thing the function's own comment says does not help, because the shift has already happened in `int32`. `1 << (4 * goarch.PtrSize)` emitted `(uintptr)(1 << (int)(32))`, C# masked the count to five bits, and the value was **1**. golib's `uintptr` carries a native `nuint` and declares `operator <<(uintptr, int)` over it, so a cast OPERAND (`((uintptr)1 << (int)(32))`) shifts at 64 bits exactly as `nuint` does; `uintptr` simply joins the list. Whole-corpus A/B: **eight files, one mechanical family**, six of them already-correct sub-int32 values reshaped (`16 << 10`, `512 << 20`, `1 << 16`, `1 << 20`) and **two live wrong answers** — `runtime/internal/math`'s `MulUintptr` overflow fast path, whose guard read 1 instead of 2³² so every `uintptr` below `MaxUint32` "overflowed", and `runtime/mpagealloc_64bit.go`'s `1 << heapAddrBits`, which computed 2¹⁶ where Go computes 2⁴⁸. The neighbouring `1<<(UintptrSize/2) - 1` was always right, which is what hid this: there the shift is an INNER node still typed `untyped int`, so the signed fold above takes it whole. CNR is byte-identical across all 576 behavioral packages — no behavioral project had the shape until now. (Guarded by the extended `LargeUintptrConst` behavioral test, already the `MaxUintptr` pattern's home: a context-typed `1 << (4 * ptrSize)`, a literal-count `1 << 40`, and the composite-literal table row beside its always-correct `- 1` sibling, values vs `go run`; and by `runtime/internal/math`'s banked suite.)

**FLOAT** contexts need the same fold, and there the damage is **silent** rather than a compile error (2026-07-17). C# masks a shift count to the left operand's width (5 bits for `int`), so an integer-literal constant in a float context — where no arm above applies, because the constant's type is not an integer — evaluates in int32 and *quietly* yields the wrong number: `var hf float64 = 1 << 63` emitted `(1 << (int)(63))`, i.e. 63 & 31 = 31 → `int.MinValue`, and `hf / (1 << 60)` divided by 2^28 (60 & 31 = 28) instead of 2^60, printing 34359738368 where Go prints 8. Go evaluates the constant in exact arithmetic and converts the *result* to the float type, so the converter emits the Go-evaluated value as a float literal — `float64 hf = 9223372036854775808D`, `hf / (1152921504606846976D)`, `float32 sf = 1099511627776F` — which also carries the values `1<<63` puts beyond `int64`, where no `L`/`UL` fold could reach. Two gates keep the readable operator form everywhere it is already correct: the operands must be **all integer literals** (that is what makes C# evaluate in int32 — a float-literal operand like `1e18 * 10.0` already computes in `double`, and a named-const operand renders via its `Untyped*` wrapper), and the value must be **outside int32** (`1 << 10` computes identically in C# and is left alone). Unlike the int64 case, an inner shift is *not* rescued by recursion: Go promotes the operands of `1<<40 * 1.5` to a common kind, so the shift is recorded `untyped float` — invisible to the signed arm's integer test — and folds from its propagated context instead (see `markUntypedConstContexts` under [Constant Values](#constant-values)); left bare it masks to 256 and silently yields 0.375. The full-stdlib A/B footprint was exactly six lines, every one a live wrong-value bug: `math`'s `normalize` (`x * (1<<52)` off by 2^32), `cbrt` (2^54), `ldexp`'s denormal factor (`1.0/(1<<53)`), `pow`'s `1<<53`/`1<<63` branch guards, and **both** `math/rand` `Float64`s — v1 divided by `int.MinValue` and so returned *negative* numbers, v2 divided by 2^21 instead of 2^53. (`floatContextConstLiteral`, `convBinaryExpr.go`; guarded by the `UntypedConstArithmetic` extension — the `1<<63`/`1<<60` float64 and `1<<40` float32 folds, the `untyped float` nested shift, plus in-range and float-literal controls that must keep their operator form, values vs Go.)

**The same fold covers a complex128 context (2026-07-18).** `complex128` is float64-backed (`System.Numerics.Complex`), so an all-integer-literal shift whose *result* type is `complex128` — a slice/array element like `[]complex128{1 << 35, 1 << 240}` (`math/cmplx`'s `hugeIn` test inputs) — carries the identical int32-masking hazard: `1 << 35` emitted `(1 << (int)(35))`, which C# masks to 35 & 31 = 3 → **8** instead of 2^35, silently corrupting the complex value's real part (its imaginary part is 0, so it is not int-literal arithmetic). `floatContextConstLiteral` takes the constant's **real part** and folds it to a `D`-suffixed literal — `34359738368D`, and the 73-digit exact form of `1<<240` — which C# parses to the same float64 the Go constant rounds to (a power of two lands exactly; a mixed value like `1234567891234567 << 40` round-trips to the nearest double, matching Go). `complex64` is deliberately excluded: its float32 real part would overflow to a C# compile error for the beyond-float32 magnitudes this fold targets, and such constants do not arise. This cleared `math/cmplx`'s `TestTanHuge`, whose huge `Tan` inputs were being reduced to tiny masked values (8, 65536, 4096) — `Tan` then computed correctly on the *wrong* arguments. (Guarded by the `ComplexConstContext` behavioral test — `1<<35`/`1<<240`/`-1<<120`/`1234567891234567<<40` complex128 real parts, values vs Go.)

### A computed constant assigned to a native-width integer that overflows int32

A related **wide** case: a computed *constant* arithmetic expression assigned to a **native-width integer** (`uintptr`/`uint`/`int` → C# `nuint`/`nint`) whose folded value overflows int32. `pattern = 1<<maxBits - 1` (runtime `mbitmap`, `maxBits` = 57) is a `uintptr` constant, but the converter folds the untyped sub-shift `1<<maxBits` to a **signed** C# `long` literal (`144115188075855872L`, since it exceeds int32 and the untyped operand is treated as signed), so the whole RHS is `long` — which has no implicit conversion to the native target (CS0266). A `UL`/`(nuint)` suffix would not help (`ulong`→`nuint` is also an explicit conversion). The converter wraps the whole RHS in the native target's cast: `pattern = (uintptr)(144115188075855872L - 1)`. This fires **only** when the constant fits int64 but is out of int32 range — exactly the signed-`long` fold range. A value that overflows *int64* (a large unsigned `uintptr` like `1<<63 + 1<<62`) is deliberately left alone: its sub-shift already mis-emits (a `1<<63` int-shift), so casting it would convert a visible compile error into a silent wrong value — that is a separate defect to fix on its own, not to mask. (Guarded by the `NativeIntWideConstAssign` behavioral test — `uintptr`/`uint`/`int` targets with int64-range constants, values verified vs Go; cleared the `mbitmap` CS0266, the last one in `runtime`.)

### The signed integer minima sign-fold at the unary level

**The signed integer minima sign-fold at the unary level.** Go folds `-literal` into one constant, but the emitter classifies the POSITIVE operand literal alone, and both signed minima's magnitudes overflow their own type: `[]int32{-2147483648}` (internal/fuzz mutator's `interesting32`) saw 2147483648 > MaxInt32 and emitted `-(nint)2147483648L`, which has no implicit conversion back to an int32 slot (CS0266); the int64 minimum's operand 9223372036854775808 does not even parse as int64, routing through the unsigned branch to `-(nuint)9223372036854775808UL` — and C# defines no unary minus on `nuint` at all (CS0023). `convUnaryExpr`'s `token.SUB` handling now mirrors its FLOAT arm: for an INT literal operand it classifies the range on the **unary expression's resolved (sign-folded) constant**. The exact int32 minimum in an int32-typed context emits the plain negated literal `-2147483648` — C# special-cases the negated decimal int-min as an `int` constant, **by value**, so `_` digit separators survive (`-2_147_483_648` compiles, proven by the guard) — and the exact int64 minimum emits `-9223372036854775808L` (the matching `long` special case), wrapped as `((nint)(-9223372036854775808L))` in a Go-`int` context where `long` has no implicit conversion. Decimal source formatting is preserved per the literal-formatting rule; hex/binary re-render as decimal (C# has no signed special case for those forms — `-0x80000000` binds as a `long`-typed expression). Everything else keeps the default path: in-int32 operands never had a problem, and a folded int32-min in a WIDER context (`var x int64 = -2147483648`, or boxed to `any` where Go-`int` must stay `nint`) keeps the implicitly-convertible `-(nint)…L` form — the full-stdlib A/B footprint was exactly the one mutator.cs line. (Guarded by the `IntMinLiterals` behavioral test — int32-min plain and underscored in `[]int32`, int64-min in `[]int64`, the nint-min `:=` form, between-minima and non-minimal negative controls, and min-value comparisons, values vs Go; the pre-fix converter fails it CS0266 ×2 + CS0023 ×2.)

### A beyond-int32 integer constant takes the width of its RESOLVED type

**A beyond-int32 integer constant takes the width of the type it RESOLVED to, not the untyped default (2026-08-08).** A Go integer constant outside the C# `int32` range cannot be written bare in a native-width slot — `long` has no implicit conversion to `nint` — so the converter wrapped every one of them in `(nint)…L`. That is right only when the constant really is a Go `int`. When it resolved to `int64` the cast is the wrong type: `int64` **is** C# `long`, so the digits alone denote it exactly, and the `(nint)` both truncates on a 32-bit target and reads nothing like the Go source. The compiler says so — `CS8778`, "constant value may overflow `nint` at runtime" — and 607 of the corpus's 620 such warnings were one table, `math/rand`'s `rngCooked [607]int64`:

```csharp
// before — every element carries the untyped-int DEFAULT type
internal static array<int64> rngCooked = new int64[]{
    -(nint)4181792142133755926L, -(nint)4576982950128230565L, (nint)1395769623340756751L, …

// after — the element type the Go source declares, and the Go source's own digits
internal static array<int64> rngCooked = new int64[]{
    -4181792142133755926L, -4576982950128230565L, 1395769623340756751L, …
```

The cause is a deliberate go/types behavior that stays invisible until you look for it: `updateExprType0` short-circuits with *"if x is a constant, the operands were constants"* and does **not** descend into the operands of a constant expression, because in Go they never materialize at runtime. So in `[...]int64{-4181792142133755926, 1395769623340756751}` the NEGATED element records `untyped int` while its positive sibling records `int64` — purely because one is wrapped in a unary minus. Every element of `rngCooked` is negative, which is why that whole table lost its element type while positive-only tables elsewhere kept theirs. `convBasicLit` therefore resolves the literal's integer type from **two** routes, exactly as it already does for the float `F`/`D` suffix: the type go/types recorded directly, and failing that the contextual type `markUntypedConstContexts` propagated (which already pushes an integer context through unary `+`/`-`/`^` and arithmetic operands). An `int64` resolution emits the bare `…L`; everything else keeps `nint` — it must, because an `any` slot has to box a Go `int` as `nint` so a later `x.(int)` succeeds — but as `unchecked((nint)…L)`, which is what makes a beyond-int32 **constant** conversion legal without the warning (`nint` is 64-bit on every platform go2cs targets, so the value is exact). The same `unchecked` covers the other two emitters of a native-width constant: `convBinaryExpr`'s constant FOLD (`unchecked((nint)(4611686018427387903L))` — `bufio`'s `maxInt/2`) and `csNintLiteral`'s array LENGTH (`unchecked((nint)140737488355327)` — runtime's `(*[maxAlloc/2 - 1]byte)` casts). The fold keeps its parenthesized `(T)(…)` body: `wholeExprIsCastOfType`, the redundancy guard 17 call sites share, now peels an `unchecked(` wrapper first, so enclosing paths still recognize the cast and do not re-wrap it into `(nint)(unchecked((nint)(…)))`. (Guarded by `nativeIntConstWidth_test.go` — the negated `int64` element and its positive sibling, the `int` var and the `any` slot both taking `unchecked`, the fold NOT double-wrapped, an in-range constant untouched, plus a unit test pinning the recognizer's peel. Corpus effect: `CS8778` 620 → **0**.)

### A NARROW-UNSIGNED target folds a constant only when nothing else can make it compile

`uint32(1<<32 - 1)`, `*_C_pw_uidp(&sp) = 1<<32 - 2`. Go evaluates an untyped constant expression
at arbitrary precision and requires only the RESULT to fit the target; C# evaluates the operands
themselves, and an operand past `uint32` forces the literal path to emit a bare `long` — which has
no implicit conversion to `uint`/`ushort`/`byte`, so the assignment fails **CS0266** even though
the value fits exactly. The fix folds the whole expression and casts the result:

```csharp
//  Go:   *_C_pw_uidp(&sp) = 1<<32 - 2          (_C_uid_t = uint32)
_C_pw_uidp(Ꮡsp).Value = unchecked((uint32)(4294967294UL));
```

**The FIVE conditions, and why each exists.** This arm is deliberately the narrowest in the fold
family, because every widening of it damaged readable emission somewhere else in the corpus. It
applies only when ALL of:

1. **the target is unsigned and narrower than `uint64`** — the wider arms already handle their own;
2. **the target is a plain basic type or an ALIAS to one**, never a NAMED type — `basic` at that
   point is the UNDERLYING type, so `io/fs.FileMode` (a named `uint32`) arrives looking plain, and
   folding it erased the type on every mode expression in the corpus
   (`(fs.FileMode)(ModeDevice | ModeCharDevice)` → a bare `unchecked((uint32)(69206016UL))`). A
   named target keeps the arm below, which carries its type in the fold; an alias has no distinct
   C# type to lose;
3. **no NAMED CONSTANT is referenced** — those render through their `Untyped*` wrappers, which is
   how every wider arm preserves them, and folding replaced `math/bits`' `x>>1 & (m0 & m)` with
   `unchecked((uint32)(1431655765UL))`: arithmetic no reader can trace back to `m0`;
4. **an UNTYPED subexpression exceeds `uint32`** — a typed conversion carries its own width and
   emits correctly unaided, so `runtime`'s `^uint32(0)/8 + 1` never needed the fold, and counting
   it flattened the entire `class_to_divmagic` table into 68 casts;
5. **the threshold is `uint32`, not the target's own width** — `(1<<16) - 1` exceeds a `uint16` but
   the emission already carries an explicit `(ushort)` cast and compiles (measured), so
   `regexp/syntax`'s `Range16` keeps its source form; only a bare `long` has nothing to rescue it.

**How the conditions were found — the method, not just the result.** Each was exposed by a
three-target corpus regeneration, never by the two packages the fix was aimed at: the local darwin
build was green at every step. The site count fell **754 → 46 → 12 → 10 → 2** across six
regenerations, and the two survivors are exactly the expressions that cannot compile otherwise.
The lesson generalizes past this arm: *a converter change is measured against the corpus, not
against the file that motivated it* — a fold that looks obviously correct at its motivating site
can rewrite hundreds of unrelated ones, and only a full regeneration shows it.

Guarded by the `ConstSubexprOverflow` behavioral test, which already covered the construct
(`u32 := []uint32{1<<32 - 1}`); its golden re-baselined to the folded form with the Output phase
passing unchanged — the value never moved, only the spelling.

### A folded constant of a NAMED type carries its type in the fold

`overflowingConstLiteral` materializes a compile-time integer constant whose value falls outside the C#
`int32` range, because C# would otherwise evaluate the operator expression in `int32` and overflow
(CS0220). It read the constant's type through `Underlying()`, so a constant of a *defined* type folded
to a bare basic literal and the Go type was simply lost:

```go
d := 8 * time.Hour
secondsEastOfUTC := int((8 * time.Hour).Seconds())
```

```csharp
var d = 28800000000000L;                            // a C# long, not a Duration
nint secondsEastOfUTC = (nint)(28800000000000L).Seconds();   // CS1929 — long has no Seconds
```

The compile error is the loud half; the silent half is `d`, which is now a `long` and prints as its
digit count where a `Duration` prints `8h0m0s`. The fold now carries the named type in the same
parenthesized `(T)(…)` shape the native-int arm uses — `(time.Duration)(28800000000000L)` — which
`wholeExprIsCastOfType` already recognizes, so enclosing paths do not re-wrap it. The `[GoType]` wrapper
converts implicitly from its underlying, so the cast is always legal, and Go's own parentheses around a
method-call receiver keep the postfix `.M()` binding to the cast rather than to the literal. Only
constants outside `int32` reach this arm at all, so the corpus footprint is the handful of computed
`time.Duration`-class constants above that magnitude. (Guarded by the `PackageNameShadowing` behavioral
test, case 4.)

## A COMPLEX constant expression must be FOLDED — .NET's mixed operators are not Go's arithmetic

The float sibling of this rule (see *Named Numeric Types and Constant Contexts*) folds a constant
expression only where a named untyped const forces it, on the stated ground that a pure-literal float
constant "computes exactly in C# double, so its readable operator form is kept". That ground does not
hold for COMPLEX, and the counter-example is ordinary: .NET's mixed real/complex operators are not
Go's constant arithmetic and are not even IEEE. `Complex.operator -(double, Complex)` computes the
imaginary part as `-right.Imaginary` rather than `left.Imaginary - right.Imaginary`, so

```csharp
1230000D - 0D.i()      // imaginary -0
```

where Go — which folds the untyped expression in exact rational arithmetic, and an exact zero has no
sign — yields `+0`. `fmt`'s `TestSprintf` reads the sign back out: `%#12.5g` of `1230000 - 0i` printed
`-0.0000i` against Go's `+0.0000i`.

So the rule is stated the way Go states it: a complex-kind constant operator expression HAS an exact
value and is emitted as that value, through the same `exactComplexConstString` a complex const
DECLARATION already uses. Keyed on `constant.Complex` — go/constant normalizes a real-valued result to
an Int/Float kind, so a real-only expression in a complex context is left to the float and integer
folds above it. Deliberately no `/* <gofmt> */` annotation, unlike the float fold: the folded text
re-renders in the same `re + imi` shape `convBasicLit` already emits, so the common case (`3 + 4i` →
`3F + 4F.i()`) is byte-identical to the operator rendering it replaces, and only the cases where the
two genuinely disagree move.

## `string()` of an untyped constant reference hops through the default type
`string(utf8.RuneError)` renders the argument as its cross-package `static readonly` Untyped* wrapper, from which `@string` has no conversion (CS0030). The conversion hops through the constant's DEFAULT Go type first -- exactly Go's conversion semantics; a plain literal is already a C# constant and keeps its direct form:
```csharp
fmt.Println("a" + ((@string)(rune)CrossPkgLib.Sep) + "b");
```
Guarded by `CrossPkgUser` (`string(CrossPkgLib.Sep)`).

## A `:=` from a named untyped constant materializes the default type
`codepoint := unicode.ReplacementChar` must not declare with `var`: the constant renders as its
`static readonly` Untyped* wrapper (`UntypedInt`/`UntypedFloat`/`UntypedComplex`), so `var` binds the
LOCAL to the wrapper type instead of Go's inferred default type, and a later Go conversion like
`string(codepoint)` fails (CS0030 — no `UntypedInt`→`@string` form; go/types conversions.go). The
declaration materializes the Go-inferred default type instead — exactly Go's `:=` typing:
```csharp
rune codepoint = replacementChar;    // NOT `var codepoint = …` (binds UntypedInt)
float64 factor = scale;
```
The gate is an Ident/Selector RHS resolving to a `*types.Const` of untyped NUMERIC kind (int is already
routed to the explicit `nint` form, and string consts to the explicit string path); literals and computed
constant expressions render as plain C# literals and keep `var`. Applies in both the single-declaration
and the mixed-statement paths. (Guarded by the `UntypedConstDefine` behavioral test — untyped rune and
float package constants `:=`-bound then converted/multiplied, output-compared vs Go.)

## A computed untyped float constant materializes at its destination's float width
A named untyped constant can still need its `Untyped*` wrapper because another use demands a different
type. When that name participates in a computed float constant, C# must not evaluate the expression
through the wrapper's arithmetic operators:
```go
const repetitions = 100000
var loopBound int = repetitions
mean := .5 * repetitions
var quarterMean float64 = .25 * repetitions
```
The wrapper form makes C# overload resolution prefer `UntypedInt.operator*`, converting `.5` or `.25`
to an integer and truncating it to zero before the result reaches the float local. The converter instead
folds the exact Go constant expression once at the destination's resolved `float32` or `float64` width.
An explicitly typed destination is visible on the expression itself; for a new `:=` local, go/types keeps
the RHS untyped and records the default type on the declared identifier, so the declaration edge supplies
that width. Both paths reuse the same named-constant fold and leave bare references, non-float constants,
and expressions without a wrapper-emitted named constant unchanged. Guarded by `UntypedConstDefine`;
`hash/maphash`'s 100,000-sample SMHasher avalanche bounds are the corpus witness.

## `complex()` over a NAMED untyped constant pins the element width
golib's `complex` builtin is overloaded on element width — `complex(float32, float32) => complex64`,
`complex(float64, float64) => complex128` — and `UntypedFloat` converts implicitly to **both**. C#
then applies its better-conversion-target rule, which prefers the **narrower** target (`float32`
converts to `float64`, not the reverse), so a `complex128` the Go checker typed as such was silently
constructed at float32 width:

```go
const maxFloat32 = 3.40282346638528859811704183484516925440e+38
over := complex(maxFloat32*2, maxFloat32*2)     // complex128, 6.805646932770577e+38
```
```csharp
var over = complex((float64)(maxFloat32 * 2D), (float64)(maxFloat32 * 2D));
```

Without the casts `over` is `(+Inf+Infi)` — and `encoding/gob`'s `TestOverflow` then found nothing
out of complex64's range to reject, because `float32FromBits` accepts +Inf at either width. A
LITERAL argument never had the problem: the untyped-const analysis records the call's element type
as the argument's context and `convBasicLit` renders the `F`/`D` suffix from it (`complex(1.5D,
2.5D)`). A **named** untyped const (`Δmath.MaxFloat32`), or a constant expression over one, renders
as the `UntypedFloat` symbol and cannot carry a width — so exactly those calls pin their untyped
arguments explicitly, at the element width Go's own typing gives the call
(`complexCallElementType`, resolving an untyped-complex-constant call through its recorded context
and Go's `complex128` default). A MIXED call needs nothing and gets nothing: `complex(g, half)` with
`g` a `float64` was always unambiguous, since `float64` has no implicit conversion to `float32`.

**The rule cannot be expressed from golib's side, and the attempt is instructive.** Naming the
untyped pair explicitly (`complex(UntypedFloat, UntypedFloat) => complex128`) makes every MIXED call
ambiguous — `complex(0D, gHalfPi)` has the float64 overload better on the first operand and the
untyped one better on the second, so neither wins (CS0121). Completing all four width pairings does
not rescue it either: `UntypedFloat` converts implicitly in **both directions** with `float32` and
`float64`, so for an operand that is neither — `complex(7/2, 0D)`, an `int` — no candidate is
strictly better and the ambiguity simply moves. Overload resolution has no way to say "prefer the
width the *call* was typed at"; only the emitter knows that.

Corpus footprint: **zero**. The trigger is a named-untyped-const operand, and no `complex()` call in
the standard library (math/cmplx included) has one — every corpus site is either width-pinned
literals or has a typed operand. Guarded by the extended `ComplexConstContext` (the overflow pair,
its float32-range question, a named-untyped-const pair in both a complex128 and an explicit
complex64 context, and the mixed call that must stay unchanged; neuter-proven — with the arm removed
the guard's `over` prints `(+Inf+Infi)` and `over-fits-float32 true` where Go says `false`).

## A constant too large for `int64`/`uint64` is emitted as `GoBigConst`

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

---

[← Compiled Library versus Source Code](compiled-library-vs-source.md) · [Index](README.md) · [Native and Narrow Integer Types →](native-and-narrow-integers.md)
<!-- {% endraw %} -->
