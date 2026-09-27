# Native and Narrow Integer Types

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#native-and-narrow-integer-types)

In Go the `int` and `uint` types are sized according to the platform build target, i.e., 32-bit or 64-bit. C#'s `int`/`uint` are always 32-bit and `long`/`ulong` are always 64-bit. As of C# 9.0, native-sized integer types exist that behave exactly like their Go counterparts: [`nint` and `nuint`](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9#performance-and-interop). The converter maps Go `int` → `nint` and Go `uint` → `nuint`; `uintptr` also maps to `nuint`. The fixed-width Go types (`int8/16/32/64`, `uint8/16/32/64`, `byte`, `rune`) are kept as readable C# aliases of the same name (e.g. `global using uint16 = System.UInt16;`).

**Narrow-integer arithmetic.** A subtle semantic gap: Go evaluates arithmetic on a sub-`int`-width integer (`int8`/`uint8`/`int16`/`uint16`) at that operand's own width, with overflow **wrapping** — `var a, b uint8 = 200, 100; a + b` is `44` (300 mod 256). C#, however, **promotes** arithmetic on `byte`/`sbyte`/`short`/`ushort` to `int`, so `a + b` is `300` and is *not* implicitly assignable back to the narrow type. Where a narrow-arithmetic result is used in a context that requires the narrow type — e.g. passed to a narrow-typed parameter — the converter emits an explicit cast back to that type, which both compiles (the implicit `int`→narrow conversion is rejected, CS1503) and restores Go's wrapping:

```csharp
takeU8((uint8)(a + b));   // Go take(a + b), a/b uint8 → 44 (wraps), not 300
takeU8((uint8)(~a));      // Go take(^a) → 55
```

The same cast applies in the **assignment** context — a narrow-arithmetic value assigned to a narrow variable, array/slice element, or struct field (`y := a + b; y = y + 1; arr[0] = a + b; bx.b = a + b`) — and in the **declaration** context — a typed-var initializer (`var z uint8 = a + b`). All emit `(uint8)(a + b)` for the same two reasons. (A double cast is avoided when another path already narrowed the RHS, e.g. a bitwise op with an untyped constant emits its own `(byte)(b | 128)`.)

The cast is applied only when the value's Go type already matches the target (parameter / LHS / declared type), so Go accepts it without a conversion, and only for an arithmetic (binary/unary) expression — a bare identifier is already the narrow type. (Guarded by the `NarrowArithmeticArg` behavioral test, which verifies the wrapped values match Go across all four contexts. Wider integer types — `int32`/`uint32` and up — are not promoted by C# and need no cast.)

A redundant-cast guard on this decision — skip the cast when the converted RHS is *already* a full narrowing (`(byte)(b | 128)`) — must distinguish a WHOLE-expression cast from one that only converts the FIRST operand. `buf[i] = byte(e/100) + '0'` (runtime `print.go`) emits the RHS `(byte)(e / 100) + (rune)'0'`, which *starts* with `(byte)(` but only casts `e/100`; the binary result is still `int` (the `(rune)'0'` promotes it), so the narrowing cast is still required (CS0266). The guard therefore checks that the cast-paren's matching close is at the very end of the RHS (a parenthesis-balance walk that skips `(`/`)` inside char/string literals), not merely that the RHS begins with `(byte)(`. (Guarded by the `NarrowByteArithFirstOperandCast` behavioral test — including a wrapping case; cleared 3 runtime CS0266 in `print.go`'s exponent formatting.)

The **per-argument** cast path (`convExprList`, which applies `castArgToType` — e.g. an `append` element cast to the slice's element type) carries the same redundant-cast guard and shares the same whole-expression test. It previously used a bare `strings.HasPrefix` check, which the narrow-shift result cast above newly exposed: `append(s, uint16(v[0])<<8 + uint16(v[1]))` (`vendor/golang.org/x/text`'s BMP-string decoding, `crypto/x509`) emits `(uint16)(…<<8) + (uint16)v[1]`, whose leading `(uint16)(` covers only the shifted first operand while the sum still promotes to `int` — so the prefix test wrongly skipped the element cast that the whole expression still needs. Both sites now call the same balance-walk helper, so a first-operand-only cast is never mistaken for a whole-expression narrowing.

The same narrowing applies to a **`return` of narrow-integer arithmetic**. `func lowerASCII(c byte) byte { return c + ('a'-'A') }` (runtime `env_posix`) returns `byte + int` (the untyped char constant promotes to `int`) → CS0266 against the `byte` result type. The cast was applied on the assignment and value-spec paths but not the return path; it is now applied in `visitReturnStmt` when the function's result type at that position is narrow and the returned expression is binary/unary arithmetic (reusing the same gate — a bare identifier, a call, or an already-whole-expr-narrowed return is untouched, and a non-narrow result type is unaffected). (Guarded by the `NarrowByteArithReturn` behavioral test — a per-branch return plus a wrapping case; cleared the `env_posix.lowerASCII` CS0266.)

The same narrowing applies to a narrow-arithmetic **comparison operand** (2026-07-18). A narrow (`int8`/`uint8`/`int16`/`uint16`) NON-constant arithmetic/complement result compared *directly* — with no narrow destination to force the cast — keeps its C# `int`-promoted value and so compares WRONG: `int8(MaxInt8) + 1 != MinInt8` evaluates `128 != -128` (true) where Go wraps at int8 width to `-128 != -128` (false). `convBinaryExprCore` now wraps each comparison operand (`==` `!=` `<` `<=` `>` `>=`) that is a narrow non-constant arithmetic expression at its own width — `(int8)(v + 1) != MinInt8` — through the same `narrowArithmeticCastTypeFor` helper (`narrowComparisonOperand`), gated to NON-constant operands: a Go constant cannot overflow its type, and a wrap cast on a C# compile-time constant expression is CS0221 (the shift-retype path guards identically). Unlike the destination contexts, this had no compile symptom — the code compiled and only the *value* of the comparison was wrong — so it surfaced only when `math`'s `TestMaxInt`/`TestMaxUint` ran. (Guarded by the `NarrowArithmeticArg` behavioral test's comparison cases `a+b == 44` / `c+d == -56` / `e+f != 4464` / `^a > 50`, output-compared vs Go.)

**A runtime shift count that can reach the operand width uses Go-semantics helpers (2026-07-18).** Go zeroes a shift whose count reaches or exceeds the operand's bit-width — `x >> n` / `x << n` with `n >= width` is `0` (a SIGNED right shift sign-extends: `0` for a non-negative value, `-1` for a negative one). C#'s native `>>`/`<<` instead **mask** the count (`n & 63` for a 64-bit operand, `n & 31` for 32-bit, and sub-`int` operands promote to `int` and mask by 31), so a native shift by a runtime count silently yields the wrong value once the count can reach the width — `math.FMA`'s double-word funnel shifts (`u2 >> (64 - n)` at `n == 0`: Go `>>64`=0, C# `>>0`=`u2`) and `math.RoundToEven`'s `>> e` (`e` up to 1024 for a NaN) both corrupted. The converter keeps the native shift ONLY when the count is PROVABLY in `[0, width)`: **R1** a constant in range (the majority of shifts — every `x >> 5`; a constant `>= width` routes to the guard, which returns 0); **R2** a syntactic mask `y & M` with constant `M <= width-1`; **R3** a modulo `y % M` with constant `M <= width`. Everything else — a bare variable count (`x >> s`, which genuinely can exceed width, as RoundToEven's `e` proves, so it cannot be trusted) or an arithmetic count (`64 - n`, `shift - e`) — routes through golib's `GoShift.Rsh`/`Lsh` extension methods, `x.Rsh(n)` / `x.Lsh(n)` (the naming echoing Go's `math/big.Int.Rsh`/`Lsh`): one guarded shift per operand width returning 0 (or sign-extending) at `n >= width`. The count is taken as a wide `uint64` so a computed count like `64 - n` that unsigned-wraps to a huge value is compared at FULL magnitude BEFORE narrowing to the `int` shift amount — truncating to `int` first would defeat the guard. A named-`[GoType]`-wrapper left operand keeps its generated-operator path, as does a compound untyped-const `UntypedInt` left operand (its own `operator<<`, the `UntypedIntWideShift` subject) — but the WRAPPER OPERATOR now carries the guard itself (2026-08-09), because leaving it native left that entire family with the masked answer and no other layer covers it. `NumericTypeTemplate`'s `operator <<` / `operator >>` emit `value.m_value.Lsh((uint64)shift)` / `.Rsh(…)` in place of the native `value.m_value << shift`, so a named integer type shifts with Go's semantics wherever the converter's own provability analysis cannot reach. What motivated it was not a wrong number but a **hang**: `math/big`'s `lehmerSimulate` reads `a2 = B.abs[n-2] >> (_W - h)` on `Word` (`type Word uint`), and for a normalized operand `h == 0`, so the count is exactly 64 — Go yields 0, C# yielded the word itself. The corrupted Lehmer cosequences make `GCD`'s `for len(B.abs) > 1` loop stop converging: an INFINITE LOOP inside `math/big`, reached from `crypto/elliptic`'s generic `CurveParams` path, so `elliptic.P256().Params().Double(Gx, Gy)` never returned. That is `crypto/ecdsa`'s `TestINDCCA/P256/Generic`, carried on the board as a 20-minute timeout open between "performance gap or hang"; the fixed path runs in 0.31 s against Go's 0.66 s, so it was never slowness. It is value-dependent, which is why it hid so long — a garbage `a1`/`a2` that fails Collins' stopping condition immediately costs only a Euclidean step, so equal-width operand pairs pass and only pairs that make the condition iterate corrupt anything. `operator >>>` stays native: Go has no unsigned-right-shift operator, so nothing converted ever calls it. Measured footprint: of ~3,556 corpus shifts, ~80% (constant, named-const, and masked/modulo counts) stay native and byte-identical; only the ~20% of unprovable variable/arithmetic counts become `.Rsh`/`.Lsh` — the SOUND floor without value-range analysis (a lightweight loop-bound/range recognition could shrink it later). This replaces the narrow-shift result retype for a runtime count (`(byte)(cb << (int)(k))` → `cb.Lsh(k)`, the `Lsh(byte)` overload doing both the width wrap and the `k>=8`→0). (Guarded by the `GoShiftSemantics` behavioral test — 64/32-bit unsigned shifts by runtime counts 0/1/63/64/65/200, signed sign-extension, and R2/R3 masked/modulo counts staying native, output-compared vs Go; cleared `math`'s `TestFMA` + `TestRoundToEven`, whose funnel shifts now emit guarded automatically. Extended 2026-08-09 with the NAMED forms — `type word uint` / `halfword uint32` / `signedword int64` shifted by the same runtime counts, including the `w >> (W - h)` shape `lehmerSimulate` writes, which is the wrapper-operator route rather than the guard route.)

**Constant-literal return inside a lambda with an unsigned result (delegate-type inference, CS8917).** A Go closure assigned to a local — `casePC := func(casi int) uintptr { if pcs == nil { return 0 }; return pcs[casi] }` (runtime `select.go`) — is emitted as `var casePC = (nint casi) => { … };`, whose delegate type C# must **infer from the return-expression types**. The literal `return 0` is typed `int`; `return pcs[casi]` is typed `nuint` (`uintptr`). C#'s best-common-type algorithm uses the expression types (not constant convertibility), and `int` has no common type with `nuint`/`uint`/`ulong` (there is no implicit `int`→unsigned conversion for a non-constant), so the `var` assignment fails with CS8917 ("no best type found for the lambda"). The converter casts the literal to the result type so both returns share it: `return (uintptr)(0)`. Gated tightly to avoid churn and new errors: only **inside a lambda body** (`conversionInLambda` — a *named* func's `return 0` to a `nuint` result compiles as an ordinary constant conversion and needs no cast), only for a bare **integer literal** (the sole shape that trips the `int`-vs-unsigned inference gap — `byte`/`uint16` widen to `int`, and the signed/`nint`/`long` kinds share a common type with `int`, so those never hit CS8917), and only when the result is a **basic** `uint`/`uint32`/`uint64`/`uintptr` (a *named* type over an unsigned kind is left alone — `(gclinkptr)(0)` would only compile if that type defined an int conversion, so casting it could introduce a new error). Runs after the narrow-arithmetic return cast, with which it is disjoint (that handles binary/unary arithmetic on sub-`int` types; this handles a bare literal to a wide unsigned type). (Guarded by the `ClosureMixedReturnUnsigned` behavioral test — `uintptr`/`uint64`/`uint32`/`uint` mixed-return closures plus a signed control that stays uncast, values verified vs Go; cleared the `select.go` `casePC` CS8917.)

> One sticking point: not all C# indexing constructs accept a `nint`. Explicit indexers support `nint`, but [implicit index support](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support) (the `Index`/`Range` syntax) currently only works with `int`, so range-operation indices are cast to `int` where needed. (The earlier strategy of compiling to `long`/`ulong`, or of custom `@int`/`@uint` structs selected by a `TARGET32BIT` directive, has been superseded by `nint`/`nuint`.)

## `uintptr` is a DISTINCT golib struct

**`uintptr` is a DISTINCT golib struct** (`golib/uintptr.cs`), not an alias of `System.UIntPtr`: Go's `uint` and `uintptr` are distinct types (both may appear in one type switch; `%T` reports them differently; conversion between them is explicit), and the historical alias erased that identity — type switches collided (CS8120), `%T` lied, and overloads could not distinguish them. The struct holds a single public mutable `nuint Value` field (PascalCase — it is public so `Interlocked`/`Volatile` seams can target the inner storage; the intrinsics cannot take a ref to a user struct) and carries the full operator surface so `uintptr`-typed expressions KEEP the type. The conversion matrix is empirically tuned to C#'s user-defined-conversion candidate rules (encompassing counts only STANDARD conversions, so nothing ever chains two user-defined operators; a PARTIAL outbound operator set is unstable — undeclared targets see multiple viable std-hop candidates, CS0457): implicit both ways with `nuint` plus implicit from smaller unsigned/`char`/`UntypedInt`; explicit inbound from signed types and `uint64`; the FULL exact outbound matrix (all integer widths + `float32`/`float64` + unsafe `void*`). Knock-ons handled with it: `const uintptr` is illegal C# (user struct) so every uintptr const emits `static readonly`; a uintptr-typed switch tag/label can never be a constant/relational pattern (CS9135) so those switches use the if-else `==` form; wrappers over uintptr (`[GoType("num:uintptr")]`) gain generated `nuint`/`UntypedInt` bridges; generic-math-constrained golib helpers (`unsafe.Add/Slice/String`) gain non-generic `uintptr` overloads; and the manual managed-referent types declare direct `uintptr` bridges (token out, panic-on-nonzero in).

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

---

[← Constant Values](constants.md) · [Index](README.md) · [Named Numeric Types and Constant Contexts →](named-numeric-types.md)
