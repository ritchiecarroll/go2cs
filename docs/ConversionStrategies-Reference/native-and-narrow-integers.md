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

A related **wide** case: a computed *constant* arithmetic expression assigned to a **native-width integer** (`uintptr`/`uint`/`int` → C# `nuint`/`nint`) whose folded value overflows int32. `pattern = 1<<maxBits - 1` (runtime `mbitmap`, `maxBits` = 57) is a `uintptr` constant, but the converter folds the untyped sub-shift `1<<maxBits` to a **signed** C# `long` literal (`144115188075855872L`, since it exceeds int32 and the untyped operand is treated as signed), so the whole RHS is `long` — which has no implicit conversion to the native target (CS0266). A `UL`/`(nuint)` suffix would not help (`ulong`→`nuint` is also an explicit conversion). The converter wraps the whole RHS in the native target's cast: `pattern = (uintptr)(144115188075855872L - 1)`. This fires **only** when the constant fits int64 but is out of int32 range — exactly the signed-`long` fold range. A value that overflows *int64* (a large unsigned `uintptr` like `1<<63 + 1<<62`) is deliberately left alone: its sub-shift already mis-emits (a `1<<63` int-shift), so casting it would convert a visible compile error into a silent wrong value — that is a separate defect to fix on its own, not to mask. (Guarded by the `NativeIntWideConstAssign` behavioral test — `uintptr`/`uint`/`int` targets with int64-range constants, values verified vs Go; cleared the `mbitmap` CS0266, the last one in `runtime`.)

**A runtime shift count that can reach the operand width uses Go-semantics helpers (2026-07-18).** Go zeroes a shift whose count reaches or exceeds the operand's bit-width — `x >> n` / `x << n` with `n >= width` is `0` (a SIGNED right shift sign-extends: `0` for a non-negative value, `-1` for a negative one). C#'s native `>>`/`<<` instead **mask** the count (`n & 63` for a 64-bit operand, `n & 31` for 32-bit, and sub-`int` operands promote to `int` and mask by 31), so a native shift by a runtime count silently yields the wrong value once the count can reach the width — `math.FMA`'s double-word funnel shifts (`u2 >> (64 - n)` at `n == 0`: Go `>>64`=0, C# `>>0`=`u2`) and `math.RoundToEven`'s `>> e` (`e` up to 1024 for a NaN) both corrupted. The converter keeps the native shift ONLY when the count is PROVABLY in `[0, width)`: **R1** a constant in range (the majority of shifts — every `x >> 5`; a constant `>= width` routes to the guard, which returns 0); **R2** a syntactic mask `y & M` with constant `M <= width-1`; **R3** a modulo `y % M` with constant `M <= width`. Everything else — a bare variable count (`x >> s`, which genuinely can exceed width, as RoundToEven's `e` proves, so it cannot be trusted) or an arithmetic count (`64 - n`, `shift - e`) — routes through golib's `GoShift.Rsh`/`Lsh` extension methods, `x.Rsh(n)` / `x.Lsh(n)` (the naming echoing Go's `math/big.Int.Rsh`/`Lsh`): one guarded shift per operand width returning 0 (or sign-extending) at `n >= width`. The count is taken as a wide `uint64` so a computed count like `64 - n` that unsigned-wraps to a huge value is compared at FULL magnitude BEFORE narrowing to the `int` shift amount — truncating to `int` first would defeat the guard. A named-`[GoType]`-wrapper left operand keeps its generated-operator path, as does a compound untyped-const `UntypedInt` left operand (its own `operator<<`, the `UntypedIntWideShift` subject) — but the WRAPPER OPERATOR now carries the guard itself (2026-08-09), because leaving it native left that entire family with the masked answer and no other layer covers it. `NumericTypeTemplate`'s `operator <<` / `operator >>` emit `value.m_value.Lsh((uint64)shift)` / `.Rsh(…)` in place of the native `value.m_value << shift`, so a named integer type shifts with Go's semantics wherever the converter's own provability analysis cannot reach. What motivated it was not a wrong number but a **hang**: `math/big`'s `lehmerSimulate` reads `a2 = B.abs[n-2] >> (_W - h)` on `Word` (`type Word uint`), and for a normalized operand `h == 0`, so the count is exactly 64 — Go yields 0, C# yielded the word itself. The corrupted Lehmer cosequences make `GCD`'s `for len(B.abs) > 1` loop stop converging: an INFINITE LOOP inside `math/big`, reached from `crypto/elliptic`'s generic `CurveParams` path, so `elliptic.P256().Params().Double(Gx, Gy)` never returned. That is `crypto/ecdsa`'s `TestINDCCA/P256/Generic`, carried on the board as a 20-minute timeout open between "performance gap or hang"; the fixed path runs in 0.31 s against Go's 0.66 s, so it was never slowness. It is value-dependent, which is why it hid so long — a garbage `a1`/`a2` that fails Collins' stopping condition immediately costs only a Euclidean step, so equal-width operand pairs pass and only pairs that make the condition iterate corrupt anything. `operator >>>` stays native: Go has no unsigned-right-shift operator, so nothing converted ever calls it. Measured footprint: of ~3,556 corpus shifts, ~80% (constant, named-const, and masked/modulo counts) stay native and byte-identical; only the ~20% of unprovable variable/arithmetic counts become `.Rsh`/`.Lsh` — the SOUND floor without value-range analysis (a lightweight loop-bound/range recognition could shrink it later). This replaces the narrow-shift result retype for a runtime count (`(byte)(cb << (int)(k))` → `cb.Lsh(k)`, the `Lsh(byte)` overload doing both the width wrap and the `k>=8`→0). (Guarded by the `GoShiftSemantics` behavioral test — 64/32-bit unsigned shifts by runtime counts 0/1/63/64/65/200, signed sign-extension, and R2/R3 masked/modulo counts staying native, output-compared vs Go; cleared `math`'s `TestFMA` + `TestRoundToEven`, whose funnel shifts now emit guarded automatically. Extended 2026-08-09 with the NAMED forms — `type word uint` / `halfword uint32` / `signedword int64` shifted by the same runtime counts, including the `w >> (W - h)` shape `lehmerSimulate` writes, which is the wrapper-operator route rather than the guard route.)

**The signed integer minima sign-fold at the unary level.** Go folds `-literal` into one constant, but the emitter classifies the POSITIVE operand literal alone, and both signed minima's magnitudes overflow their own type: `[]int32{-2147483648}` (internal/fuzz mutator's `interesting32`) saw 2147483648 > MaxInt32 and emitted `-(nint)2147483648L`, which has no implicit conversion back to an int32 slot (CS0266); the int64 minimum's operand 9223372036854775808 does not even parse as int64, routing through the unsigned branch to `-(nuint)9223372036854775808UL` — and C# defines no unary minus on `nuint` at all (CS0023). `convUnaryExpr`'s `token.SUB` handling now mirrors its FLOAT arm: for an INT literal operand it classifies the range on the **unary expression's resolved (sign-folded) constant**. The exact int32 minimum in an int32-typed context emits the plain negated literal `-2147483648` — C# special-cases the negated decimal int-min as an `int` constant, **by value**, so `_` digit separators survive (`-2_147_483_648` compiles, proven by the guard) — and the exact int64 minimum emits `-9223372036854775808L` (the matching `long` special case), wrapped as `((nint)(-9223372036854775808L))` in a Go-`int` context where `long` has no implicit conversion. Decimal source formatting is preserved per the literal-formatting rule; hex/binary re-render as decimal (C# has no signed special case for those forms — `-0x80000000` binds as a `long`-typed expression). Everything else keeps the default path: in-int32 operands never had a problem, and a folded int32-min in a WIDER context (`var x int64 = -2147483648`, or boxed to `any` where Go-`int` must stay `nint`) keeps the implicitly-convertible `-(nint)…L` form — the full-stdlib A/B footprint was exactly the one mutator.cs line. (Guarded by the `IntMinLiterals` behavioral test — int32-min plain and underscored in `[]int32`, int64-min in `[]int64`, the nint-min `:=` form, between-minima and non-minimal negative controls, and min-value comparisons, values vs Go; the pre-fix converter fails it CS0266 ×2 + CS0023 ×2.)

**Constant-literal return inside a lambda with an unsigned result (delegate-type inference, CS8917).** A Go closure assigned to a local — `casePC := func(casi int) uintptr { if pcs == nil { return 0 }; return pcs[casi] }` (runtime `select.go`) — is emitted as `var casePC = (nint casi) => { … };`, whose delegate type C# must **infer from the return-expression types**. The literal `return 0` is typed `int`; `return pcs[casi]` is typed `nuint` (`uintptr`). C#'s best-common-type algorithm uses the expression types (not constant convertibility), and `int` has no common type with `nuint`/`uint`/`ulong` (there is no implicit `int`→unsigned conversion for a non-constant), so the `var` assignment fails with CS8917 ("no best type found for the lambda"). The converter casts the literal to the result type so both returns share it: `return (uintptr)(0)`. Gated tightly to avoid churn and new errors: only **inside a lambda body** (`conversionInLambda` — a *named* func's `return 0` to a `nuint` result compiles as an ordinary constant conversion and needs no cast), only for a bare **integer literal** (the sole shape that trips the `int`-vs-unsigned inference gap — `byte`/`uint16` widen to `int`, and the signed/`nint`/`long` kinds share a common type with `int`, so those never hit CS8917), and only when the result is a **basic** `uint`/`uint32`/`uint64`/`uintptr` (a *named* type over an unsigned kind is left alone — `(gclinkptr)(0)` would only compile if that type defined an int conversion, so casting it could introduce a new error). Runs after the narrow-arithmetic return cast, with which it is disjoint (that handles binary/unary arithmetic on sub-`int` types; this handles a bare literal to a wide unsigned type). (Guarded by the `ClosureMixedReturnUnsigned` behavioral test — `uintptr`/`uint64`/`uint32`/`uint` mixed-return closures plus a signed control that stays uncast, values verified vs Go; cleared the `select.go` `casePC` CS8917.)

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

> One sticking point: not all C# indexing constructs accept a `nint`. Explicit indexers support `nint`, but [implicit index support](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support) (the `Index`/`Range` syntax) currently only works with `int`, so range-operation indices are cast to `int` where needed. (The earlier strategy of compiling to `long`/`ulong`, or of custom `@int`/`@uint` structs selected by a `TARGET32BIT` directive, has been superseded by `nint`/`nuint`.)

## Narrow arithmetic narrows itself where its consumer is not wrap-invariant

A non-constant `int8`, `uint8`, `int16` or `uint16` arithmetic result carries its own cast back to its type
wherever the value that reaches its consumer would differ from Go's. The casts above cover a typed narrow
destination; this rule covers every other consumer: an `any` or interface argument, a generic argument, a
widening or float conversion, an index or slice bound, a shift count, a `/`, `%` or `>>` operand, a switch tag
or case value, a map key, a channel send, a map-literal value, a keyed array element and a parenthesized
operand. In these examples `a` is an `int8` holding 100 and `u` is a `uint8` holding 200, so Go's `a + a` is
-56 and `u + u` is 144.

<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.go:63 -->
```go
var x any = a + a
```
<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.cs.target:116 -->
```csharp
any x = (int8)(a + a);
```

<!-- source: src/tests/Behavioral/NarrowArithmeticCompileSinks/main.go:22 -->
```go
mk := map[uint8]string{144: "wrapped"}
fmt.Println("map key:", mk[u+u])
```
<!-- source: src/tests/Behavioral/NarrowArithmeticCompileSinks/main.cs.target:29 -->
```csharp
var mk = new map<uint8, @string>{[144] = "wrapped"u8};
fmt.Println(mapKeyˢ, mk[(uint8)(u + u)]);
```

`mapKeyˢ` is the hoisted `"map key:"` literal (`private static readonly object mapKeyˢ = (@string)"map key:"u8;`).

**Why.** C# evaluates `a + a` as the `int` 200 where Go wraps to -56. Deferring the wrap is exact only while
the consumer reads the low bits and something above it narrows the final result. Anywhere else the
unwrapped value is observable: an interface boxes `200` as an `int32`, so `%T` and a type switch see the
wrong type; `int(u+u)` reads 400; `s[u+u]` panics; a switch takes the wrong case; and a map key, range bound,
channel send, map-literal value or keyed array element does not compile.

**One decision, before emission.** A per-file pre-pass (`markNarrowArithmeticContexts`, beside
`markUntypedConstContexts`) classifies each narrow arithmetic expression by its consumer, seen through
parentheses, and `convBinaryExpr` / `convUnaryExpr` emit the cast on the expression itself:

- **Wrap-invariant** consumers add nothing: a same-width `+ - * & | ^ &^` (and its compound assignment,
  `x += e`), the left operand of a native `<<`, a unary `- ^ +`, or a conversion to an integer no wider than
  the operand. Each reads only the low bits, and the result above it is narrowed in turn. A unary `+` or
  signed `^` therefore passes its operand's risk up: `^(a + b)` counts as a result that can leave the range.
- **Value** consumers read the whole value but not its C# type: a widening or float conversion, an index, a
  slice bound, a shift count (including the right side of `<<=` / `>>=`), a `/`, `%` or native `>>` operand
  (and the right side of `/=` / `%=`), a switch tag or case value. Only a result that can leave the narrow range in C# takes the cast: `+`, `-`, `*`, a
  unary `-`, and a signed `/` (`int8(-128) / -1` is 128 in C#).
- **Typed** consumers take the value at its Go type: every other consumer, including a comparison, an
  interface or generic argument, a map key (a type parameter with a map core included), a switch case
  value or tag compared through an interface, the receiver of a guarded shift (below), and every typed
  destination. Here every narrow result takes the cast,
  because a result that is only int-TYPED (`>>`, `%`, an unsigned `/`, a signed unary `^`, a unary `+`) still
  boxes, infers and binds as `int32`.

**A guarded shift's receiver.** A shift whose count is not provably below the width renders as golib's
`receiver.Rsh(n)` or `receiver.Lsh(n)`, and the receiver's C# type picks the overload. An int-promoted
receiver would bind the `int32` one, which neither wraps nor narrows, so the receiver is cast and
parenthesized: `(a + a) >> n` becomes `((int8)(a + a)).Rsh(n)`. The narrow overload then returns Go's type
and width, so a guarded shift needs no cast of its own.

<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.go:166 -->
```go
var x int8 = (a + a) >> n
```
<!-- source: src/tests/Behavioral/NarrowArithmeticSinks/main.cs.target:205 -->
```csharp
int8 x = (int8)(((int8)(a + a)).Rsh(n));
```

At a typed destination of the identical Go type the cast takes the destination's own spelling (`byte` for a
`[]byte` element fed `uint8` arithmetic), so the destination casts described above see a whole-expression cast
of their own type and add nothing: their emission does not change. A value consumer whose destination has
the identical type (the divisor of `x /= e`, a parameter of a named func type) takes its spelling too. For the same reason the cast is always
written `(T)(…)`, even around a rendering that is already parenthesized. A parenthesized Go operand drops its
own parentheses once its content is a cast: `(a+a)/2` becomes `(int8)((int8)(a + a) / 2)` when the quotient
itself reaches a typed consumer. The one exception is a guarded shift's receiver, which is parenthesized
again because a cast binds looser than the `.Rsh(n)` member access.

**Not covered here.** A named narrow type (`type T uint8`) is excluded: its `[GoType]` wrapper operators already
cast back. So are the operators whose emission already narrows its whole result: `& | ^ &^`, a native
`<<`, a guarded `>>` or `<<`, and an unsigned unary `^`. So is a constant expression, which cannot overflow its type in Go (and whose cast would be
CS0221). A named interface declared inline, `type I interface{}`, rejects every basic value, narrow or not,
because it emits as a C# interface; `type I any` emits as `object` and is covered.

Guarded by: `NarrowArithmeticSinks` (the runtime consumers, one recovered arm per class, each red before the
rule), `NarrowArithmeticCompileSinks` (the consumers that did not compile before the rule),
`NarrowArithmeticArg` (the typed destinations, unchanged).
<!-- Ruled 2026-09-27 (ledger 15:18, mailbox 8d62b909a1) from C2's sizing (inbox COORD 20260927T201231Z-C2:
     345 at-risk GOROOT sites across windows+linux+darwin, prod + tests). Before the rule an interface sink
     printed `200 int32` for Go's `-56 int8`; NarrowArithmeticSinks' 25 arms were all red at master
     1aebd6a885 and NarrowArithmeticCompileSinks failed CS1503 x3 / CS0266 x3. The inline empty-interface
     rejection is independent of arithmetic (a plain int8 value fails CS0029 the same way at master). -->

## A signed division or remainder by a variable divisor calls golib's `quo` or `rem`

An unnamed `int`, `int32` or `int64` (so also `rune` and C#'s `nint`) division or remainder whose divisor is
not a constant, and not `len` or `cap`, becomes golib's `quo(a, b)` or `rem(a, b)`; `x /= b` and `x %= b`
become `x = quo(x, b)` and `x = rem(x, b)`. A constant -1 divisor folds when converting: `a / -1` is
`unchecked(-a)` and `a % -1` is zero. Every other divisor keeps C#'s operator.

<!-- source: src/tests/Behavioral/MinIntDivide/main.go:59 -->
```go
arm("int64", func() string { return fmt.Sprint(i64/int64(m1), " ", i64%int64(m1)) })
```
<!-- source: src/tests/Behavioral/MinIntDivide/main.cs.target:73 -->
```csharp
arm(int64ˢ, () => fmt.Sprint(quo(i64, (int64)m1), (@string)" "u8, rem(i64, (int64)m1)));
```

Here `i64` holds `math.MinInt64`, `m1` is an `int` holding -1, `arm` runs the function under `recover` and
prints its result under the label, and `int64ˢ` is the hoisted `"int64"` literal.

**Why.** Go's spec wraps the one overflowing signed quotient, the most negative value divided by -1, to that
same value, and makes any value modulo -1 zero, with no panic. .NET throws `OverflowException` for both at
32 and 64 bits. That is not a Go panic: `recover()` cannot see it, and the program dies. Division by zero is
unaffected: it still throws `DivideByZeroException`, which golib reports as Go's integer divide-by-zero
panic.

**Where the check lives.** Go itself checks for -1 at run time only where the divisor is a variable, so that
is the only place the helper appears:

- `quo` and `rem` (golib's `builtin.cs`) have `nint`, `int32` and `int64`
  overloads: `b == -1 ? unchecked(-a) : a / b` and `b == -1 ? 0 : a % b`. Their XML documentation says why,
  so a hover over a call explains it.
- A NAMED integer type keeps `a / b`: go2cs-gen's `NumericTypeTemplate` gives a wrapper over `int32`, `int64`,
  `nint` or `rune` the same -1 arm inside its own `/` and `%` operators.
- A narrower signed type needs nothing: C# promotes it to `int`, where `-128 / -1` is 128, and the
  narrow-arithmetic rule above casts the result back to Go's wrapped value.
- A `len` or `cap` divisor is never negative, and a constant divisor is known when converting.

**When a Go name would bind first.** C# resolves a bare `quo` or `rem` to a member of the package class
before the `using static go.builtin` import, and a C# local is in scope in its whole block, including its
own initializer. So the helper is written `builtin.quo` or `builtin.rem` when the package declares a
function, method, variable or constant of that name (`go/constant` declares its own `quo`), or when the
enclosing function declares a variable of that name anywhere (`if rem := n % size; rem != 0` in
`crypto/internal/fips140/ecdsa`):

<!-- source: src/tests/Behavioral/MinIntDivideShadow/main.cs.target:19 -->
```csharp
fmt.Println(builtin.quo(i32, (int32)m1), builtin.rem(i32, (int32)m1));
```

**Limits.** A compound `x /= b` whose target has a side effect when read (an index by a call, say) keeps
C#'s `/=`: it cannot be read twice. No standard-library site has that shape.

Guarded by: `MinIntDivide` (int, int32, int64, rune, named int64 and int32, the narrow types, compound
assignment, the constant -1 fold, ordinary divisors, a local named `rem`, and division by zero, each arm
under `recover`), `MinIntDivideShadow` (a package declaring its own `quo` and `rem`), and GolibTests'
`SignedDivisionTests` (the helpers, and a control proving the plain operators throw).
<!-- Owner ruling 2026-09-27 (ledger 15:42, mailbox 446950ea43), from C2's sizing (inbox COORD
     20260927T193256Z-C2): at master every int, int32, int64 and named-over-int64 arm died with
     OverflowException (exit 2), unseen by recover(). Measured cost 0-3% on a 4M-element micro-benchmark. -->

---

[← Constant Values](constants.md) · [Index](README.md) · [Named Numeric Types and Constant Contexts →](named-numeric-types.md)
