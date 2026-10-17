# Visual-parity review: where converted C# reads less like Go than it has to

Measured 2026-10-16 at master `d0dfcbf2f8` (clean tree). Suggestions only; nothing in any worktree was edited.

## Scope and method

- **Goldens:** I read about 45 behavioral projects closely or skimmed them as `.go` beside `.cs.target`.
  Among them: PointerToPointer, RangeStatements, DeferSimple, PanicRecover, SelectStatement, StructPromotion,
  InterfaceImplementation, GenericFuncDecl, GenericTypeDecl, TypeConversion, ExprSwitch, GoCallVariations,
  VariableCapture, IotaEnum, UnsafeOperations, StringByteSemantics, LabeledEmptyStmt,
  SwitchBreakContinueWrapper, MapCommaOk, ArrayPassByValue, GenericInterfaceConstraint, TypeAssert,
  MethodExpression, ForVariants, IfStatements, ParallelAssignmentHazard, AnonymousStructs,
  NestedEmbeddingPromotion, TypeSwitchMultiCase, SliceAliasing, ChannelCapLen, Constraints,
  GenericStructEquality, GenericTypeInstantiation, NamedSliceConversion, ClosureDefer, DeferLoopCapture,
  GoexitDefers, SelectSendDefault, InterfaceCasting, PointerReceiverRepoint, NamedMapNestedAssign,
  EmbeddedValuePointerMethod, StructPromotionWithInterface, RangeOverIntegerTypes.
- **Pattern counts** cover the whole corpus: all **872** `*.cs.target` goldens, plus the **15,432** converted
  `.cs` files under `src/core`. golib, `bin` and `obj` are excluded. A Python regex counter does the counting
  (in the session scratchpad, not in the repo). Counts are regex hits, so treat them as **±10% orders of
  magnitude**. Each count's regex is in the table where it matters.
- **Stdlib spot checks:** strings (and strings_test), encoding/binary, container/list, net/netip,
  strconv/itoa, time/format, weak tests, and sort (clean). sync/mutex is hand-owned, so it was not
  representative.
- **Why the current shapes exist:** I read `docs/ConversionStrategies.md` (closures, switch, strings, the
  glyph tables), `ConversionStrategies-Reference/pointers.md` and `expression-switch.md`, and the converter
  and golib sources each item cites.
- **Probes:** these ran in `H:\go2cs-tmp-coord\parity-probe` on .NET SDK 10.0.400 with C# 14
  (bin/obj deleted afterwards). Each claim below is marked **[probed]** or **[read only]**.

| Probe | Result |
|---|---|
| `err != nil` / `fn == nil` / `any != nil` against a NilType class that has `operator ==(object?, NilType)` | compiles with **no** CS0252/CS0253, results correct; a slice operand still binds its own `(slice, NilType)` operator |
| CollectionBuilder on a `readonly struct slice<T>`: `slice<int> s = [2,3,4]`, `f([1,2,3])`, nested `f([[1,2,3],[4,0,6],[7,8]])`, cast `(slice<int>)[9,8]`, `[]` | all compile and run; `[]` reaches the builder (non-nil) |
| IIFE helper `func(() => {…})()`, `func(() => 42)()`, `func((int x) => x+1)(3)` | work; direct `(() => {})()` is **CS0149** |
| C# 14 `extension(Shape) { static operator ==(Shape, Shape) }` on an interface | **compiles but is silently never called**: reference equality wins (a trap, see K1) |
| Interceptor replacing a call's result with a per-site cached static (legacy `InterceptsLocation(path,line,col)`, `<InterceptorsNamespaces>`) | works (CS9270 deprecation warning; a real generator would use `GetInterceptableLocation`) |
| `for (;;)` and `for (int i = 2; ; i++)` as the end of an int-returning method | no CS0161 |
| char literals: `int r='X'`, `nint n='a'`, `byteVar=='a'`, `byteVar-'0'`, `case 'X':` on an int switch | compile; `byte b='X'` and `case 'a':` on a **byte** switch are **CS0266** |
| `foreach ((i, v) in xs)` into existing locals | **CS8186** (the `iᴛ1` temps are load-bearing) |
| `d["e"]["f"] = 1` through a readonly-struct indexer rvalue | **CS1612** (`.Set(…)` is load-bearing) |

---

## Ranked suggestions

Ranking is readability gain (reach × how far the site is from Go) against effort and risk.
"Reach" is converted-core sites unless stated.

| # | Item | Class | Reach (core) | Effort | Risk |
|---|---|---|---|---|---|
| 1 | **N1** nil comparisons render `nil`, not `default!` | converter | ~17.9K comparisons (≥11.1K `err`) | S | low (stage 1) |
| 2 | **P1** pointer-field selectors `(~p).f` / `p.Value.f` → `p.f` via the designed extension properties | gen + converter | ~25.3K | M | med |
| 3 | **R5** un-aliased pointer **params** keep their Go name (`Ꮡt.Errorf` → `t.Errorf`) | converter | ~10.3K params / ~27K uses | M | low-med |
| 4 | **C1** rune literals drop `(rune)` where C# already takes a char | converter | ~9.7K casts, most eligible | S-M | low |
| 5 | **B1** drop ≥32-bit bitwise re-casts, keep precedence parens | converter | ~3.3K wraps in ~530 files | M | med |
| 6 | **Z1** `T{}` emits `new T()` instead of `new T(nil)` | converter | ~1.7K | S | low |
| 7 | **E1** `} else if (…) {` on one line; space before `{` | converter | ~1.1K splits + ~4.5K `){` | S | low |
| 8 | **B2** redundant parens: `((T)x)` in arg/assign/return; `(T)(5)` | converter | ~2.1K + ~5.5K | S | low |
| 9 | **L1** slice literals as collection expressions | golib + converter | ~9.4K simple (+ nested) | M | med |
| 10 | **H1** hoisted string literals back at the site via interceptors | gen + converter (+golib) | ~12.1K decls / ~30.4K uses | L | med-high |
| 11 | **P2** field address `Ꮡb.of(Buffer.Ꮡoff)` → `Ꮡb.Ꮡoff` | gen + converter | ~8.3K | S (after P1) | low |
| 12 | **D1** `default!` → `default` | converter | ~62K | S | none (churn) |
| 13 | **H2** fold `heap<T>(out …); x = rhs;` into `heap(rhs, out …)` | converter | ~1.9K pairs | S-M | low-med |
| 14 | **N2** named-numeric → any basic numeric in one cast | gen | ~850 double casts | S-M | med |
| 15 | **S1** non-constant tag switch as a C# pattern `switch` | converter | ~1.6K `exprᴛ` temps (114 do/while) | M | med |
| 16 | **F1** IIFE `((Action)(() => {…}))()` → `func(() => {…})()` | golib + converter | ~92 (58 goldens) | S | low |
| 17 | Minor: `WithDirection`, generic constraint repetition | golib / converter | 92 / small | S / L | low / med |

**Recommended first batch:** N1 (stage 1), Z1, E1, C1, and B2 (B2 bundled with the queued shift-count
cleanup). All five are converter-only, S to S-M, and IL-identical or semantically trivial. Between them they
remove roughly 35-40K visibly non-Go tokens. **P1** is the largest single win. It belongs right beside the
motivating extension-property work, not in a separate batch (see the P1 notes).

---

## 1. N1: nil comparisons render `nil`, not `default!`

- **Pattern.** Go `net/netip/netip.go:135` `if err != nil {` → C# `net/netip/netip.cs:149`
  `if (err != default!) {`.
  - Go `PanicRecover.go:13` `if r := recover(); r != nil {` → `PanicRecover.cs.target:26`
    `var r = recover(); if (r != default!) {`.
  - Pointers already read `found == nil` (IpAdapterAddresses main.cs.target:149). Interfaces, funcs, slices,
    maps and chans read `default!`.
- **Why it happens.** `convIdent.go:55-74` renders universe `nil` as golib `nil` only in "pointer context" and
  as `default!` everywhere else.
- **Proposed shape.** In `==` / `!=` where one side is universe nil, render `nil` whenever the other operand's
  type has a NilType operator.
  - Stage 1: interface and func operands. golib **already has** `operator ==/!=(object?, NilType)`
    (`golib/NilType.cs:150-170`), so no golib change is needed. Converter only.
  - Stage 2: slice, map and chan, after the proof below.
- **Parity.**
  - Interface: `err != default!` is C# reference inequality against null. `err != nil` binds
    `(object?, NilType)` = `obj is not null`. The predicate is identical.
  - Typed nil pointer inside an interface: the value is a non-null adapter or box, so both forms answer
    non-nil, as Go does.
  - Funcs: `Delegate.op_Equality(d, null)` and `is null` agree (NilType does not convert to Delegate, so the
    object operator is the only candidate).
  - **Not identical / exclude:**
    - Type-parameter operands. `T == nil` would box and bind the object operator. The converter already
      special-cases these (`ISlice.IsNil`).
    - Slices, until proven: `s == default!` binds `slice<T> == slice<T>` (header identity incl.
      `m_nativeBase`, `golib/slice.cs:1188-1195`), while `s == nil` binds `IsNil` (`m_array is null`,
      `slice.cs:224`). These can disagree for a **native-backed** slice whose managed array is null. Prove
      `IsNil ⇔ header == default` for every construction kind first, and do the same for map
      (`a.Equals(b)` vs `m_map is null`) and channel.
  - **Assignments and returns stay `default!`.** C# forbids user-defined conversions to interfaces, so
    `return (n, nil)` cannot type-check. For pointers, `nil` converts to the canonical `NilBox`, whereas
    `default!` is a C# null. Switching those is a representation change (it interacts with `?.` uses such as
    `data?.Value` in TypeConversion.cs.target:32), so it is out of scope here.
- **Cost.** A static readonly field load plus a one-line operator the JIT inlines to a null test, so the same
  machine code as today. No allocation. AOT-neutral.
- **Reach.** `[!=]= default!`: 14,648 + 3,246 core (283 + 228 goldens). By name alone, `err [!=]= default!`
  is 9,669 + 1,465.
- **Effort.** S. **Risk:** low for stage 1. **Probe:** done [probed]. The cheap confirmation is a converter
  change plus a corpus reconvert: the build is the proof, since an operand without an operator fails to
  compile.

## 2. P1: pointer-field selectors through the designed extension properties (materially wider reach than the motivating case)

- **Pattern.**
  - Go `GoCallVariations.go:37` `acc.total` → `GoCallVariations.cs.target:51` `(~acc).total`.
  - Go `StructPromotion.go:84` `l.tag = "x"` → `StructPromotion.cs.target:82` `l.Value.tag = "x"u8;`.
  - Go `ParallelAssignmentHazard/main.go:39` `e.out, e.arg = e.arg, e.out` → `main.cs.target:29`
    `(e.Value.@out, e.Value.arg) = (e.Value.arg, e.Value.@out);`.
  - In container/list, every `e.prev.Value.next = …` (`container/list/list.cs`, insert/remove/move).
- **Angle.** The motivating `buf.of(T.ᏑF).Value = x` shape is only **214** core sites.
  - The same per-struct ref-returning extension properties, declared on `ж<T>`, also serve Go's ordinary
    implicit dereference.
  - That is `(~p).f` reads (**15,762** core, 512 goldens) and `p.Value.f` reads and writes (**9,573** core,
    269 goldens).
  - The proposal is to route those through the same generated members, so the site is `p.f`, as in Go.
- **Shape.** Gen (the motivating design) plus converter (emit `p.f` for a Go selector through a pointer to
  a struct).
- **Parity.**
  - Nil: `~p` checks `IsNilPointer` and `p.Value` checks `IsNull`. For a struct T these are the same mark
    (`ж.StandardBox.cs:102-123`). A C# null `p` throws NRE in all three forms.
  - Aliasing: the property returns `ref` to the real field, which is Go's semantics. Today `(~p)` copies the
    **whole struct** (`ж.cs:781-787` returns T by value) and then reads one field. The ref form is the same
    value single-threaded, and it is more faithful when a chained call mutates through the field.
  - **Not identical / must guard:**
    - Go field names that collide with `ж<T>` instance members: `Value`, `ValueSlot`, `IsNull`,
      `IsNilPointer`, `IsNative`, `NativeAddress`, `Direction`/`Cargo` on other carriers, `of`, `at`,
      `Equals`, `GetHashCode`, `ToString`, `GetType`.
    - **Instance members silently win** over extension members. A field named `Value` would make `p.Value = x`
      overwrite the *whole struct*.
    - So the converter needs a collision list that falls back to today's spelling, plus a guard test with a
      `Value` field.
  - Stack frames: mark the getters `[MethodImpl(AggressiveInlining)]`, so runtime.Caller-sensitive guards
    (CallerLine*, RuntimeCallerFrames) see no extra frame.
- **Cost.** Compile-time sugar plus an inlined static `ref` getter. It **removes** a struct copy per
  `(~p).f` read, which is a perf win for large structs.
- **Effort.** M (the generator work is already planned; the converter routing and collision fallback are
  new). **Risk:** medium (collisions). **Probe:** the extension-property mechanics belong to the known design;
  the collision behaviour is a two-line probe.

## 3. R5: un-aliased pointer parameters keep their Go name

- **Pattern.** Go `strings/strings_test.go:312` `t.Errorf("IndexRune(%q, %d) = %v; want %v", …)` → C#
  `strings/strings_test.cs:401` `Ꮡt.Errorf("IndexRune(%q, %d) = %v; want %v"u8, …)`.
- **Why.** A pointer parameter is always named `Ꮡname`, so that a ref-lowered deref alias
  `ref var name = ref Ꮡname.DerefOrNull();` can take the Go name.
  - When no alias is emitted, which is the common case, every body use carries the `Ꮡ` that Go does not have.
  - Pointer **locals** already keep Go names (`var acc = Ꮡ(new accum(nil))`), so params and locals are
    inconsistent.
- **Shape.** Converter: name the parameter `t` when the function emits no deref alias for it.
  **Receivers** (`this ж<T> Ꮡl`) stay as they are, because `ReceiverMethodTemplate.cs:69` derives the box name
  from them.
- **Parity.** A pure rename, with identical IL. The existing shadow and collision renaming covers a nested C#
  scope reusing the name (CS0136 is a compile error, never silent).
- **Cost.** None.
- **Reach.** 14,286 non-receiver `ж<…> Ꮡx` params, 10,295 with no alias, ~27.0K body uses. `Ꮡt.(Error|Errorf|Fatal…)`
  alone is 17,797. (The params regex misses nested generic param types, so this is a lower bound.)
- **Effort.** M (every converter site that spells the box name must consult "alias emitted?").
  **Risk:** low-med.
- **Owner decision:** this changes the reading convention, from "Ꮡ = box" to "Ꮡ = box only when a deref
  alias also exists".

## 4. C1: rune literals drop `(rune)` where C# already accepts a char

- **Pattern.**
  - Go `time/format.go:916` `'0' <= c && c <= '9'` → `time/format.cs:968`
    `(rune)'0' <= c && c <= (rune)'9'`.
  - Go `net/netip/netip.go:118` `case '.':` → `netip.cs:130` `case (rune)'.':`.
  - Go `strings_test.go:286` `{"", 'a', -1}` → `strings_test.cs:343` `new(""u8, (rune)'a', -1)`.
  - Doubled: `encoding/json/indent.cs:74` `(byte)((rune)'\\')`.
- **Shape.** Converter: emit the bare char literal when go/types records the literal's final type as
  `rune`/`int32`, `uint16`, `uint32`, `int64`, `uint64`, `int`, `uint`, `uintptr`(?) (C# has implicit
  char → ushort/int/uint/long/ulong/nint/nuint), or when the literal is an operand of a comparison or
  arithmetic operator. Emit `(byte)'x'` (not `(rune)`) for byte targets.
- **Parity.** Identical constant values; comparisons and arithmetic promote both forms to int.
  - **Keep the cast for:**
    - byte, int8 and int16 targets, and case labels on a byte switch (CS0266 [probed]);
    - `any`/interface boxing (`fmt.Println('a')` must box int32, not char);
    - generic inference and overload resolution (e.g. `(@string)(rune)ch`);
    - `var r = 'a'` (it would become char);
    - runes above 0xFFFF (no char literal exists);
    - named types and `uintptr`, if golib's struct lacks a char conversion.
- **Cost.** Identical IL.
- **Reach.** `\(rune\)'` = 9,735 core in 490 files (163 goldens). The sample suggests most are comparisons,
  switch labels and rune/int parameters.
- **Effort.** S-M. **Risk:** low (a wrong choice is a compile error, except in the `any` and overload cases,
  which the rule excludes). **Probe:** char contexts [probed].

## 5. B1: drop same-width bitwise re-casts for ≥32-bit types; keep only precedence parens

- **Pattern.** Go `encoding/binary/binary.go:88`
  `return uint32(b[0]) | uint32(b[1])<<8 | uint32(b[2])<<16 | uint32(b[3])<<24`
  → `binary.cs:92`
  `return (uint32)((uint32)((uint32)((uint32)b[0] | ((uint32)b[1] << (int)(8))) | ((uint32)b[2] << (int)(16))) | ((uint32)b[3] << (int)(24)));`
- **Why.** `convBinaryExpr.go` (bitwise arm, final `return fmt.Sprintf("(%s)(%s)", …)` near line 2069)
  wraps **every** `& | ^ &^` result in `(T)(…)`.
  - That wrap is needed for sub-int types (C# promotes them to int; see
    `narrowArithmeticOperations.go:17-60`).
  - For `int32/uint32/int64/uint64/nint/nuint`, C# already yields the Go type.
- **Shape.** Converter: skip the wrap when the result is one of those types and both operand renderings are
  already that C# type (typed non-constant operands, or bare literals). Emit parentheses only where Go and C#
  precedence differ.
  - Go puts `|`/`^` with `+` and `&`/`&^` with `*`, all above comparisons.
  - C# puts `& ^ |` *below* `==`.
  - So `(a | b) == c` keeps its parens, while `x | y << 8 | z` needs none.
  - After the queued shift-count cleanup, the example reads
    `(uint32)b[0] | (uint32)b[1] << 8 | (uint32)b[2] << 16 | (uint32)b[3] << 24`, token for token like Go.
- **Parity.** Same values (the casts are identity conversions). **Not identical** if a paren is lost where
  precedence differs, so this needs a guard test mixing `|`/`&`/`^` with `+`, `==` and `<<`.
  - Keep the cast where an operand is an `UntypedInt`/`UntypedFloat` wrapper or a named numeric (the existing
    operand-cast arms).
- **Cost.** Identical IL.
- **Reach.** A balanced-paren scan of `(T)(…)` with a top-level bitwise operator finds, in core, uint64 1,393,
  uint32 1,083, nint 543, int32 130, nuint 122 and int64 43, about 3.3K in ~530 files. byte/uint16 (≈1.1K)
  stay. It is concentrated in crypto, hash, encoding and math/bits, which Go readers read closely.
- **Effort.** M (a precedence-aware paren decision). **Risk:** medium. **Probe:** a precedence-matrix guard
  red-then-green, then the corpus reconvert.
- An alternative implementation worth a probe: a post-conversion Roslyn simplifier pass (IDE0004 "remove
  unnecessary cast", IDE0047 "remove unnecessary parentheses"). It is semantically checked, but it is a new
  toolchain step in the converter pipeline [read only].

## 6. Z1: an empty struct literal emits `new T()`

- **Pattern.**
  - Go `StructPromotion.go:70` `record := Record{}` → `StructPromotion.cs.target:68`
    `var record = new Record(nil);`.
  - Go `PointerToPointer.go:48` `b := Buffer{}` → `.cs.target:46` `b = new Buffer(nil);`.
- **Why it can change.** `StructTypeTemplate.cs:2079-2096` generates the `(NilType _)` ctor **and** an
  explicit parameterless ctor with the **same body** (`AppendZeroValueInitializers`). The converter already
  emits `new T()` for zero values (`heap(new T1(), …)`, UnsafeOperations.cs.target:88).
- **Shape.** Converter: struct composite literals with no elements emit `new T()`. Only **struct** types:
  `InheritedTypeTemplate`'s `(NilType)` ctor for named slice/map types means *nil*, and Go `IntSlice{}` is
  non-nil.
- **Parity.** Identical: same initializer, and overload resolution prefers the parameterless ctor over the
  all-optional field ctor.
- **Cost.** Marginally cheaper (no NilType argument).
- **Reach.** `new …(nil)` 1,704 core / 271 goldens (some are non-struct and excluded).
- **Effort.** S. **Risk:** low. **Probe:** golden diff plus build.

## 7. E1: `} else if (…) {` on one line, and a space before `{`

- **Pattern.** Go `strings/strings.go:1099` `} else if n < 0 || m < n {` → `strings/strings.cs:1187-1190`:
  ```
  nint m = Count(s, old); if (m == 0){
      ...
  } else 
  if (n < 0 || m < n) {
  ```
  The same appears in `netip.cs` parseIPv4Fields.
- **Why.** `visitIfStmt.go:76-90`:
  - It writes `" else"` and then recurses. The recursive `visitIfStmt` starts with `v.newline` (line 37), so
    the else-if lands on its own line.
  - `strings.TrimSpace(body)` strips the space before `{` whenever an else follows.
  - The position sentinel (line 86) only needs the `if` line to map to the else-if's Go line, and in Go that
    is the same line as `} else`.
- **Shape.** Converter: emit `} else if (cond) {` on one line and keep `) {`.
- **Parity.** None affected. Guard: the line-map guards (CallerLine*, LineMarker*, RuntimeCallerFrames).
- **Reach.** `} else \n if (` 1,114 core; `){` at line end 4,505 core.
- **Effort.** S. **Risk:** low.

## 8. B2: redundant parentheses around conversions and cast literals

- **Pattern.**
  - Go `NamedSliceConversion/main.go:34` `count([]CaseRange(special))` → `main.cs.target:30`
    `count(((slice<CaseRange>)special))`.
  - Go `StringByteSemantics/main.go:37` `string(b)` → `main.cs.target:30` `((@string)b)`.
  - Go `SliceAliasing/main.go:57` `append(w, 500)` → `main.cs.target:41` `(uint32)(500)`.
  - `strconv/itoa.cs` `u = ((uint64)0 - u);`.
  - `encoding/binary/binary.cs:79` `(byte)((v >> (int)(8)))` (Go `binary.go:76` `byte(v >> 8)`).
- **Shape.** Converter: wrap a conversion in outer parens only when it is the receiver of `.`, `[]`, `->`,
  `++/--`, or an operand of a binary operator. A cast of a non-negative literal needs no inner parens;
  `(T)(-5)` keeps them (for non-keyword T, `(T)-5` parses as subtraction).
- **Parity.** Syntax only. **Cost:** none.
- **Reach.**
  - Argument position: 1,382 core.
  - Assignment: 533 core.
  - Return: 186 core.
  - `(T)(literal)`: 5,550 core / 593 goldens.
- **Effort.** S. **Risk:** low. Bundle it with the queued shift-count cleanup (same code paths, one corpus
  churn).

## 9. L1: slice literals as collection expressions

- **Pattern.** Go `LabeledEmptyStmt.go:57` `nestedGotoEnd([][]int{{1, 2, 3}, {4, 0, 6}, {7, 8}})` →
  `.cs.target:53`
  `nestedGotoEnd(new slice<nint>[]{new nint[]{1, 2, 3}.slice(), new nint[]{4, 0, 6}.slice(), new nint[]{7, 8}.slice()}.slice())`.
- **Shape.**
  - golib: `[CollectionBuilder(typeof(slice), nameof(Create))]` on `slice<T>`, with
    `Create(ReadOnlySpan<T>)` returning a **non-nil** slice even when empty.
  - Converter: in target-typed positions (non-overloaded argument, typed local, field, return, element of a
    collection expression) emit `[…]`, giving `nestedGotoEnd([[1, 2, 3], [4, 0, 6], [7, 8]])`.
    Go elides the inner types exactly the same way.
  - A local declares its type: `slice<nint> nums = [2, 3, 4];` (RangeStatements.cs.target:20 today:
    `var nums = new nint[]{2, 3, 4}.slice();`).
  - Test tables become `slice<T> tests = [ new(…), … ];`.
- **Parity.** Element conversions are the same implicit conversions the array initializer uses.
  - `[]` must stay **non-nil** (Go `[]int{}` ≠ nil). The builder controls this [probed].
  - **Keep today's form for:**
    - fixed-length arrays (`[N]T{…}` pads to N; a collection expression would not);
    - keyed or sparse literals (`SparseArray`);
    - `var x = …` without a stated type;
    - generic calls whose inference needs the element type;
    - overloaded callees (collection expressions convert to arrays, spans and lists, so the risk is
      ambiguity or a different overload; CS0121 is the visible case).
- **Cost.** Primitive constants come from an RVA span, others from a stack InlineArray. Then `Create` copies
  into one new `T[]`. Today's code is one array allocation plus an RVA memcpy. Allocation count is the same,
  plus one copy for non-primitive elements. Confirm in the perf suite.
- **Reach.** Single-level `new T[]{…}.slice()` 9,385 core / 627 goldens, plus nested and `{}` (587).
- **Effort.** M. **Risk:** medium (overload sensitivity). **Probe:** mechanics [probed]; corpus impact
  unmeasured.

## 10. H1: string literals back at the call site via interceptors (owner decision)

- **Pattern.** Go `VariableCapture.go:10` `fmt.Println("Name =", d.name)` → `VariableCapture.cs.target:12`
  `private static readonly object nameˢ = (@string)"Name ="u8;` plus `:15` `fmt.Println(nameˢ, d.name);`.
  ExprSwitch.cs.target:36-78 is a 40-line hoist wall ahead of one function.
- **Why it exists.** It is deliberate and load-bearing for cost (`hoistedLiteralOperations.go:21-38`;
  DESIGN-string-literal-allocation). A u8 → @string materialization copies, and boxing to `any` allocates.
  The pass also has to relocate package-var initializers for init order (§4.4).
- **Shape.**
  - Converter: emit `fmt.Println(ˢ("Name ="), d.name)`, where golib's `ˢ(string)` is an allocating fallback.
  - Generator: for every `ˢ(…)` invocation, emit an interceptor (`GetInterceptableLocation`). It returns a
    per-literal cached `static readonly object` (or `@string`) built from `"Name ="u8`. One method per unique
    literal, N `[InterceptsLocation]` attributes.
  - golib.targets: add `<InterceptorsNamespaces>`.
  - Literals that are not valid UTF-8 keep today's hoisted form.
- **Parity.** Identical bytes and identical one-time materialization. Initialization moves into a generated
  class's static init (or lazily), which **removes** the init-order hazard and could retire
  `packageHoistLitReaders`/relocation. Literal identity is per assembly instead of per package, which nothing
  in Go observes except `unsafe.StringData` equality, which Go itself does not guarantee.
- **Cost.**
  - Runtime: an inlined static field read, the same as today.
  - Build: a generator pass over every invocation in big packages.
  - Feature status: interceptors are honored by SDK 10.0.400 [probed with the legacy attribute], but they are
    still documented for first-party and opt-in use. That platform dependency is the decision.
- **Reach.** `static readonly … ˢ = ` 12,103 core / 4,254 goldens; `\w+ˢ` uses 30,387 core / 8,827 goldens.
- **Effort.** L. **Risk:** medium-high.

## 11. P2: field address through a generated property

- **Pattern.**
  - Go `PointerToPointer.go:49` `PrintValPtr(&b.off)` → `.cs.target:47` `PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));`.
  - Go `l.lazyInit(); … &l.root` → `container/list/list.cs` `Ꮡl.of(List.Ꮡroot)`.
- **Shape.** Gen: beside P1's `ref F f`, emit `ж<F> Ꮡf => p.of(T.Ꮡf)` on `ж<T>`. The converter emits
  `Ꮡb.Ꮡoff`, which drops the type-name repetition Go does not have.
- **Parity.** Same call, same cached view (`ж.cs:217-230`). **Cost:** an inlined forwarder.
- **Reach.** `.of(T.Ꮡf)` not followed by `.Value`: 8,270 core.
- **Effort.** S once P1 lands. **Risk:** low.
- **Owner decision:** the spelling (`Ꮡb.Ꮡoff` vs keeping `of`).

## 12. D1: `default!` → `default`

- **Pattern.** Go `MapCommaOk/main.go:35` `var w int` → `main.cs.target:28` `nint w = default!;`. The same
  shape covers every zero-valued declaration and nil-valued return or argument.
- **Why it can go.** Converted projects compile with `<Nullable>annotations</Nullable>` (e.g.
  `strings.csproj:21`; `csprojTemplate_test.go:360` pins the policy). Nullable **warnings are off**, so `!`
  is inert everywhere.
- **Parity and cost.** Identical IL. A source consumer that turns nullable warnings on would see warnings,
  never errors.
- **Reach.** 62,349 core / 1,418 goldens.
- **Effort.** S. **Risk:** none, but it is a corpus-wide churn. Do it in the same reconvert as N1.

## 13. H2: fold the heap declaration and its initializer

- **Pattern.**
  - Go `AnonymousStructs.go:39` `a := 10` → `.cs.target:36-37`
    `ref var a = ref heap<nint>(out var Ꮡa);` / `a = 10;`.
  - Go `GenericStructEquality` `x, y := 5, 5` → `ref var x = ref heap<nint>(out var Ꮡx); x = 5;`.
- **Shape.** Converter: for a single-statement `x := rhs` / `var x = rhs` whose box declaration is not
  hoisted, emit `ref var a = ref heap(10, out var Ꮡa);`. The `heap(in T, out ж<T>)` overload already exists
  (`builtin.cs:2791`).
- **Parity.** rhs is evaluated before the box exists, in both forms. rhs cannot name `Ꮡx`: Go scoping forbids
  it, and so does C# (compile error). **Unverified:** I did not find the reason the split exists. Check the
  anonymous-struct lift order (`visitAssignStmt.go:975-1010`) and multi-value forms before cutting.
- **Cost.** Cheaper: it skips `GoZero<T>()` construction followed by an overwrite, which allocates for needy
  structs.
- **Reach.** Adjacent declare-then-assign pairs: 1,406 + 517 core / 281 goldens.
- **Effort.** S-M. **Risk:** low-med.

## 14. N2: a named numeric converts to any basic numeric in one cast

- **Pattern.**
  - Go `IotaEnum.go:56` `if int(k) < len(kindNames) {` → `.cs.target:53` `if ((nint)(uint8)k < len(kindNames)) {`.
  - `runtime/metrics_test.cs:598` `(float64)(int64)latencies[…]`.
- **Shape.** Gen: for `/*num:U*/` named numerics, emit explicit operators to every basic numeric type. The
  body is `(target)value.m_value`.
- **Parity.** A Go conversion of a named numeric is the conversion of its underlying value, which is exactly
  the current two-step. **Risk:** adding operators can make a previously valid cast ambiguous (CS0457) or
  change which user-defined conversion binds in generic or overloaded code. Probe with the full set of
  numeric targets.
- **Reach.** ~850 core double casts.
- **Effort.** S-M. **Risk:** medium.

## 15. S1: a non-constant tag switch as a C# pattern switch

- **Pattern.** Go `SwitchBreakContinueWrapper/main.go:20-27` (`switch words[i] { case "skip": … break … continue …`)
  → `.cs.target:35-51`. That is `var exprᴛ1 = words[i]; if (exprᴛ1 == "skip"u8) { do { … break; … goto continueᴛ1; } while (false); }` with a `continueᴛ1:;` label.
- **Shape.** Converter: `switch (words[i]) { case var ᴛ when ᴛ == "skip"u8: { … } }`.
  - It mirrors the expressionless form the converter already emits (`switch (ᐧ) { case {} when … }`,
    ExprSwitch.cs.target:133-142).
  - A C# switch is a native `break` target, and `continue` passes through it to the loop. So the
    `do { } while (false)` wrapper and the `goto continueᴛN` label both disappear, and the tag temp is gone.
  - Keep the if-chain for `fallthrough` switches (`goto case` needs constants).
- **Parity.** The tag is evaluated once in both forms. When clauses run in lexical order, and `default` runs
  last regardless of position, matching Go. Labels with side effects evaluate in order and only until the
  first match, as in Go. Guard: the existing ExprSwitch, SwitchDefaultOrder and SwitchBreak* goldens.
- **Cost.** Equivalent (C# lowers it to the same comparisons).
- **Reach.** `var exprᴛ = ` 1,575 core (not all are switches); `} while (false);` 114; `goto continueᴛ` 4.
- **Effort.** M. **Risk:** medium.
- **Owner decision:** taste. Per-line, `case var ᴛ when ᴛ == L:` is wordier than `if (exprᴛ1 == L)`, but the
  block shape matches Go.

## 16. F1: IIFE via a golib `func` identity

- **Pattern.** Go `PanicRecover.go:43` `func() { defer …; f() }()` → `.cs.target:65` `((Action)(() => { … }))();`.
- **Shape.** golib: `public static Action func(Action f) => f;` plus `Func<…>`/`Action<…>` arities.
  - The converter emits `func(() => { … })()`.
  - For a value-returning literal, emit the result type explicitly, as `func<nint>(() => …)()` or C# 10
    `func(nint () => …)()`. A natural-type lambda `() => 0` would be `Func<int>` and change `%T`.
  - `func` cannot collide with a converted identifier: it is a Go keyword.
- **Parity.** An identity method. Same delegate, same capture. **Cost:** inlined identity; delegate allocation
  unchanged.
- **Reach.** ~92 core / 58 goldens (`}))(`).
- **Effort.** S. **Risk:** low. [probed]

## 17. Minor

- `ch.WithDirection(GoChanDir.Recv)` (SelectStatement.cs.target:81,86 for Go `go filter(ch, ch1, prime)`,
  `.go:78`). It exists for reflection, since direction is stamped on the value (`channel.cs:1320-1346`).
  - A shorter golib spelling such as `ch.Recv()` / `ch.Send()` is S. Hiding it entirely would need distinct
    directional types: L.
  - 92 core sites.
- Generic type constraints repeated on every method (GenericTypeInstantiation.cs.target:
  `where T : /* ~int | ~string */ IAdditionOperators<…>, …` on Push, Pop and every other method).
  - C# 14 `extension<T>(ref Stack<T> s) where T : … { … }` blocks state the constraint once per block, as Go
    states it once per type.
  - It is a large emission and generator change for modest gain. Park it.

---

## Examined and kept: these shapes are load-bearing

- **`AreEqual(a, b)` for interface `==`** (InterfaceImplementation.cs.target:35, 90; 2,510 core).
  - C# `==` on interfaces is reference equality.
  - **[probed]** A C# 14 extension `operator ==` on an interface compiles but is **never selected**, because
    predefined reference equality wins silently. Do not try this route; it would be a silent false parity.
- **Interface adapters** (`new FrogжAnimal(f)`, `.cs.target:32`). C# forbids user-defined conversions to
  interfaces, and C# 14 has no extension conversions.
- **`foreach (var (iᴛ1, vᴛ1) in …) { i = iᴛ1; … }`** for `for i, num = range` (RangeStatements.cs.target:27-29).
  CS8186 **[probed]**.
- **`d["e"u8].Set("f"u8, 1)`** for `d["e"]["f"] = 1` (NamedMapNestedAssign). CS1612 **[probed]**, even for a
  readonly struct.
- **`rem(i, prime)` / `div`** (78 / 42 core). C# `int.MinValue % -1` throws; Go yields 0.
- **`{ var r = recover(); if (r != …) { … } }` blocks.** C# `if`-condition pattern variables leak to the
  enclosing scope, so the brace block keeps Go's if-init scoping. The one-line `init; if` already mirrors Go.
- **Labeled `break`/`continue` → `goto break_x` / `continue_x:;`** (ForVariants). C# has no labeled
  break/continue.
- **`ʗ` capture snapshots.** C# cannot capture `ref` locals. The snapshot keeps the enclosing body's accesses
  off the display class (pointers.md:319). It only applies when nothing writes after capture.
- **`params ꓸꓸꓸT xsʗp` + `.sslice()`.** This is stack-only variadics. A C# 13 `params slice<T>` would
  heap-allocate per call.
- **`s.slice(lo, hi)` vs `s[lo..hi]`.** `System.Range`/`Index` are `int`, while Go indices are 64-bit `int`.
  Ranges are already used where the bounds are int constants.
- **`"…"u8` everywhere.** It is the zero-cost UTF-8 literal. A C# `string` would transcode per use.
- **`GoFrame ᒐ` try/catch-when/finally** (DeferSimple.cs.target:13-20). Moving `Capture` into the filter would
  run it in the first pass, before inner `finally` blocks. `IsPanic` also maps CLR exceptions to Go panics, so
  a typed `catch` cannot replace it.
- **`throw panic(x)`** (reachability), **array `.Clone()`** (Go value-copies arrays), **`ΔRangeSnapshot()`**,
  **`OrTypedNil()`** (typed nil into `any`), **`(uint64)0 - u`** for unsigned `-u` (C# rejects unary minus
  on ulong), and **`Δ` collision renames**.
- **String-tag switches as if-chains.** u8 literals are not C# constants. S1 changes the *shape*, not this
  fact.

## Not re-proposed (already known)

- Shift-count casts (the post-hop queue). B1 and B2 should ride with it.
- Face-lift markers and GoStr.
- `PLAN-marker-comment-parity.md` (on master).
- Uniform `/*embed*/` fields.
- The motivating struct-field extension properties. P1 and P2 only extend their reach.
- ASCII glyph spellings. `ᐧ` was left alone for that reason, though `while (ᐧ)` → `for (;;)` (934 + 90 core;
  Go `for {` / `for i := 2; ; i++`) is an S, zero-risk, probe-proven change **[probed: no CS0161]**, if the
  owner considers it outside the glyph item.

## Spotted in passing (not readability)

- **`TypeConversion.cs.target:32`** emits `ж<Person> person = ((ж<Person>)(data?.Value ?? default!));` for
  Go `(*Person)(data)`.
  - That converts the **pointee value** and re-boxes it. If it does, pointer identity and aliasing with
    `*data` are lost, which is a behavior question worth a guard.
  - [read only; I did not trace the conversion operator.]
- `fallthrough` is a `ThreadLocal<bool>` (`builtin.cs:57-60, 209`). Every `fallthrough = true` and every
  test pays a ThreadLocal access, where a function-local bool would do.
- SwitchBreakContinueWrapper.cs.target:68 compares `words[i] == "stop"`, a C# `string` literal (not `u8`),
  inside the do-while wrapper. That transcodes per evaluation, and it is inconsistent with the `"skip"u8`
  emitted beside it. 118 core `== "…")` sites.

## Decisions for the owner

1. **H1:** take a dependency on interceptors (opt-in compiler feature) to put string literals back at the
   site and retire hoist and relocation machinery. The other option is to accept the hoist walls.
2. **R5:** the naming convention. `Ꮡ` would mark a parameter only when a deref alias also exists.
3. **P2 spelling** `Ꮡb.Ꮡoff`, and whether to schedule **P1** with the motivating extension properties, which
   I recommend.
4. **D1:** spend one corpus-wide churn (62K sites) on the inert `!`. If yes, fold it into N1's reconvert.
5. **L1:** collection expressions only in nested and argument positions (lowest risk), or also typed locals.
6. **S1:** the switch-shape preference.
7. Whether `for (;;)` counts as part of the rainy-day glyph item.

## Reproducibility notes

- The counts come from regexes over the master working tree at `d0dfcbf2f8`:
  - "core" = `src/core/**/*.cs` minus golib/bin/obj (15,432 files);
  - "goldens" = `src/tests/Behavioral/*/*.cs.target` (872).
- The key regexes are quoted inline: `[!=]= default!`, `\(~[\wᏑ@]+\)\.[\w@]+(?!\w|\()`,
  `\b[\wᏑ@]+\.Value\.[\w@]+(?!\w|\()`, `\(rune\)'`, `\.of\([\w.]+\.Ꮡ[\w@]+\)`,
  `static readonly [^=]*ˢ\d* = `, `\bnew [\w<>@, ]+\(nil\)`, `\} else \n\s*if \(`, `\)\{\s*$`,
  `new [\w@<>, ]+\[\]\{[^{}]*\}\.slice\(\)`.
- The pointer-parameter and bitwise counts used small balanced-scan scripts, so they are lower bounds.
- Probe sources: `H:\go2cs-tmp-coord\parity-probe\{Program.cs, Intercept.cs, probe.csproj}`. The negative
  probes were transient files, recorded in the table above.
