# Empty Interface (`any`)
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#empty-interface-any)
In Go, every type satisfies the method-less interface `interface{}`, now spelled `any`. This operates fundamentally like .NET's `System.Object`, so the converter maps the Go empty interface to `any` (a global alias for `object`). For example, a Go `func(i interface{})` becomes `void f(any i)`, and a `map[any]string` becomes `map<any, @string>`.

## A named empty interface is an alias to `object`, whether it is spelled `any` or `interface{}`
A defined type over the empty interface holds every Go value, so it is emitted as a `global using` alias to
`object`, the same route as a defined type over any other named interface. That holds for both spellings,
`type I any` and the inline `type I interface{}`: the empty interface is `any`'s type set, whatever the RHS
syntax. The declaration keeps its Go name wherever the converted code uses it, and an uninhabited
descriptor carrier stamped `[GoLocalName]` keeps that name for reflection.

<!-- source: src/tests/Behavioral/NamedEmptyInterface/main.go:9 + :13 + :17 -->
```go
type I interface{}

func show(v I) { fmt.Printf("I: %T %v\n", v, v) }
…
func back(n int8) I { return n }
```
<!-- source: src/tests/Behavioral/NamedEmptyInterface/main.cs.target:1 + :8-9 + :16 + :24 -->
```csharp
global using I = object;
…
// Descriptor carrier for `I` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("I")] public interface Iᴅ { }
…
internal static void show(I v) {
…
internal static I back(int8 n) {
```

Emitted as a nested C# interface instead, it holds nothing, because no primitive or converted struct
implements an interface it was never declared on: every assignment, argument, return and element was CS0029
or CS1503. Only an inline interface with an EMPTY type set takes this route. An inline interface with methods
is a real nested C# interface, and a constraint interface (`interface{ ~int }`) is not the empty interface.
Importers reach the alias through the published type-alias entries
([Empty-interface targets](golib-namespace.md)).

Guarded by: `NamedEmptyInterface` (int8, int, string, a struct and nil assigned to, passed as, and returned as
a named `I` and an anonymous `interface{}` parameter; a type assertion, a type switch, comparison, and
`[]I` and `map[string]I`; each printed as `%T %v` against Go).

## A string literal in an `any` slot boxes through `@string` — as `(@string)"…"u8`
A Go string literal normally emits as a `"…"u8` `ReadOnlySpan<byte>` (which converts implicitly to `@string`). But a `ReadOnlySpan<byte>` has **no conversion to `object`**, so a string literal RETURNED (or returned as a tuple element) where the result type is the empty interface fails with CS0029 — testing's `func (f *chattyFlag) Get() any { return "test2json" }`. Such a result must box a golib `@string` (preserving Go string identity for a later `x.(string)` assertion), so `visitReturnStmt` renders the literal as `(@string)"…"u8` for an empty-interface result element:

```csharp
internal static any Get(this ref chattyFlag f) {
    if (f.json) {
        return (@string)"test2json"u8;   // NOT a BARE "test2json"u8 (CS0029)
    }
    return f.on;
}
```

**The cast and the `u8` suffix are independent decisions.** The cast is what the slot requires — it turns
the span into an `@string`, which boxes with Go's `string` dynamic type. The suffix then just decides where
the literal's *bytes* come from: `u8` makes them a compile-time constant in the assembly's data section,
while a bare C# `"…"` is a UTF-16 constant that `Encoding.UTF8.GetBytes` has to transcode **on every
evaluation** of the site (measured 2.1–2.4× for ASCII, 4.2× for non-ASCII). The two were coupled for a
while — the emitter derived "no `u8`" from "needs the cast" — and every `any` position paid the transcode.
They are now separate flags (`castToGoString`, `u8StringOK`), and every `any` slot takes the combined form.

The coupling had a second, load-bearing consumer: `convBinaryExpr` suppressed `u8` inside a string
**concatenation** whenever the enclosing slot had `u8` off, because two u8 operands fold (C# folds UTF-8
literal constants) into a single `ReadOnlySpan<byte>` that then has no boxing conversion — `print("\n" +
"\t")` in runtime's `newstack` diagnostics is CS1503. Splitting the flags would have silently re-enabled
`u8` there, a **compile break that CNR cannot surface** (the emitted text is legal-looking; only the corpus
build fails). Concat suppression therefore has its own signal, `BasicLitContext.spanTargetUnsupported`,
which says "this slot cannot hold a bare span" and is set by every span-hostile site — `any` positions,
ValueTuple elements, attribute (struct-tag) arguments, `panic`'s object parameter, a deferred call's
generic type-parameter slot — independently of how a STANDALONE literal renders there. It propagates into
nested operands, so `"a" + "b" + c` stays suppressed all the way down.

`resultParamIsInterface` excludes the empty interface (`andNotEmptyInterface`), so the interface-conversion arm never fires for `any`; the per-element context sets `castToGoString` on (and leaves `u8StringOK` on) instead. Only string basic-literals consult those flags, so a non-string `any` result is unaffected. Also corrects a latent semantic bug in the multi-result form (`return "<no value>", true` from a `(any, bool)` result rendered a raw C# string, which would fail a Go `x.(string)` assertion). Guarded by `InterfaceCasting`.

The same boxing applies to an **assignment** whose target's static type is the empty interface — a plain
local (`arg = "<nil>"`, go/types format.go's sprintf over an `any` range variable, CS0029), a
selector/index target (`h.value = "field"`), and a mixed-statement reassignment all render the literal
`(@string)"…"u8`. `visitAssignStmt` threads the same `castToGoString`-on literal context
into each RHS conversion site when `lhsIsEmptyInterface` reports the target is `any` (the NON-empty
interface wrap stays with `convertExprToInterfaceType`, which the empty interface deliberately bypasses).
(Guarded by the `AnyStringLitAssign` behavioral test — an `any` local, an `any`-typed range variable, and
an `any` struct field each assigned a string literal, then type-switched on `string`, output-compared vs Go.)
The same boxing applies to every **composite-literal** position whose declared slot type is the empty
interface — the interface-wrap machinery deliberately bypasses `any` there too, so a string-literal
element otherwise renders either as the u8 span (no conversion to the generated `object` slot —
CS1503/CS0029) or as a bare C# string (compiles, but boxes a `System.String`, so a later Go
`x.(string)` assertion or `case string:` fails at runtime):

```go
type pair struct { label string; value any }
p  := pair{"tag", "val"}          // positional field
n  := &node{inner: "hi"}          // keyed field (typed, elided, and pointer-elided forms alike)
m  := map[string]any{"k": "mv"}   // map value
mk := map[any]int{"ky": 7}        // map key
s  := []any{"a", "b"}             // slice/array element
sp := [3]any{1: "sp"}             // sparse-array element
```

```csharp
var p  = new pair("tag"u8, (@string)"val"u8);        // NOT bare "val" (wrong boxed identity)
var n  = Ꮡ(new node(inner: (@string)"hi"u8));        // NOT a BARE "hi"u8 (CS1503)
var m  = new map<@string, any>{["k"u8] = (@string)"mv"u8};
var mk = new map<any, nint>{[(@string)"ky"u8] = 7};
var s  = new any[]{(@string)"a", (@string)"b"}.slice();
var sp = new array<any>(3){[1] = (@string)"sp"u8};
```

(`p`'s FIRST element is the plain `string` field — a positional struct element in a `string` slot now
renders `u8` too, matching what the keyed and elided forms already emitted; the slice/array `any`
ELEMENT form is the one position still on the bare-cast rendering.)

Keyed elements resolve their target slot in `convKeyValueExpr` (struct field via `info.Uses`; map/sparse
element and map key via the threaded composite type) and take the same `castToGoString`-on literal
context; positional struct fields and slice/array elements flip the per-element flags
(`useGoStringArg` on, `u8StringArgOK` left on) that `convExprList` feeds each element's literal context. A TYPE-PARAMETER slot is excluded even though its underlying constraint is an
interface (`isEmptyInterfaceTarget`) — a `~string`-constrained field takes the literal directly. Only
string basic-literals are affected; every non-`any` slot keeps its exact prior form. (Guarded by the
`AnyStringLitComposite` behavioral test — all the shapes above, each read back through a `string`
type-switch to prove runtime identity, output-compared vs Go.)

The same boxing applies to a **channel send** whose element type is the empty interface — both the
statement form and the select-case registration form. The send value previously converted with no
target-type context at all, so the literal's default `"…"u8` span failed against the channel's
`in object` send parameter (CS1503):

```go
ch := make(chan any, 1)
ch <- "text"                    // statement send
select { case ch <- "sel": … }  // select-case send (registration form)
```

```csharp
ch.ᐸꟷ((@string)"text");                              // NOT "text"u8 (CS1503)
switch (select(ch.ᐸꟷ((@string)"sel", ꓸꓸꓸ))) { … }    // registration form takes the same box
```

Both send positions route through a shared `convSendValueExpr` (`visitSendStmt.go`), which resolves
the channel's ELEMENT type and applies the same `isEmptyInterfaceTarget`/`isStringBasicLit` gate as
the assignment and composite-literal positions (a type-parameter element is excluded; only string
basic-literals are affected). The same helper also activates the NON-empty interface element wrap —
see [Maps and Channels](maps-and-channels.md#maps-and-channels). (Guarded by the `AnyStringLitChanSend` behavioral test —
statement and select-case sends read back through a `string` type-switch and an `x.(string)`
assertion to prove runtime identity, output-compared vs Go.)

An explicit conversion through an interface type LITERAL — `interface{}("something")`, as testify's
`objx` writes it — takes the same box. The conversion arm wrapped the operand in a cast to the literal's
C# type and nothing else, so a string constant arrived as its u8 span: `(any)("something"u8)` (CS0030,
no conversion from `ReadOnlySpan<byte>` to `object`). The arm now boxes a string CONSTANT through
`@string` first, `(any)((@string)("something"u8))`, whether the operand is a literal or a named constant
and whether the type is written bare or parenthesized (`(interface{})("x")`). Only a constant of the BASIC string type is affected: a constant of a
NAMED string type keeps its own wrapper type, so its dynamic type stays `main.sname` rather than
collapsing to `string`, and a string variable was never a span. The `any(…)` spelling takes a different
route and is unchanged. (Guarded by the `IfaceLiteralStringConversion` behavioral output test — a
literal, a parenthesized interface type, a named constant, a named-type constant, a string variable
and an `any(…)` control, each printed with `%v %T` against Go.)

## An untyped constant boxed as `any` boxes at Go's DEFAULT TYPE

The numeric twin of the `@string` boxing above. Go materializes an untyped constant into an interface
at its **default type**: untyped int → `int` (go2cs `nint`, an `IntPtr`), untyped rune → `rune`
(`int32`), untyped float → `float64`. The boxed CLR type must match, because every observation of an
interface value dispatches on it: `x.(int)` (emitted `x._<nint>()`) panics on a boxed `Int32`
(`interface conversion: interface {} is int, not int` — both sides print "int", but one is `Int32`,
one is `nint`); a `case int:` type switch falls through; golib `AreEqual` bails early on
`leftType != right.GetType()`; and `fmt`'s `printArg` type-switch drops to its reflection fallback.
Two renderings need a cast, for different reasons:

- A bare int **literal** (or literal-only arithmetic) in the int32 range: `convBasicLit` renders it as
  a plain C# integer literal, which is `System.Int32`. (An int constant *outside* int32 range already
  renders `(nint)…L`, and a rune literal renders `(rune)'A'` / a float literal `2.5D` — all already the
  default CLR type, so those need nothing.)
- Any expression referencing a **named untyped constant** (`const fsize = 5`), which `visitValueSpec`
  emits as a golib `UntypedInt`/`UntypedFloat` **wrapper struct**, never a CLR number — `fsize + 1`
  evaluates through the wrapper's operator overloads and boxes the STRUCT, which matches *no* Go type
  at all. This holds at every magnitude and for every wrapped kind, which is why the int32-range test
  applies to the literal rendering only.

The cast applies at every empty-interface position — call argument (variadic `...any` included),
var-spec, assignment, `return`, channel send, slice/array element, keyed struct-field, map value, map
KEY (composite and index alike), and an explicit `any(...)` conversion:

```go
fmt.Sprintf("%s.v%d.%d", GOARCH, 8, i)   // variadic ...any argument
fmt.Sprintf("%s%c%03d", d, os.PathSeparator, seq)  // named untyped RUNE const under %c
v.Store(42)                  // non-variadic any argument (atomic.Value.Store)
var a any = 7                // var-spec
b = 8                        // reassignment
func r() any { return 42 }   // return
ch <- 3                      // channel send (chan any)
_ = []any{5}                 // slice/array element
_ = map[string]any{"k": 9}   // map value
_ = map[any]string{12: "x"}  // map key (and the matching m[12] lookup)
_ = holder{v: 3}             // keyed struct field
_ = any(7).(int)             // explicit conversion to any
```

```csharp
fmt.Sprintf("%s.v%d.%d"u8, GOARCH, (nint)(8), i);
fmt.Sprintf("%s%c%03d"u8, d, (int32)(os.PathSeparator), seq);
Ꮡv.Store((nint)(42));
any a = (nint)(7);
b = (nint)(8);
internal static any r() { return (nint)(42); }
ch.ᐸꟷ((nint)(3));
_ = new any[]{(nint)(5)}.slice();
_ = new map<@string, any>{["k"u8] = (nint)(9)};
_ = new map<any, @string>{[(nint)(12)] = "x"u8};   // and m[(nint)(12)]
_ = new holder(v: (nint)(3));
_ = ((any)(nint)(7))._<nint>();
```

`untypedConstBoxCast` (`convCallExpr.go`) drives the decision and returns the C# cast type (or none).
The constant's **kind** comes from `info.Types[arg]` — the type go/types has already DEFAULTED for the
interface slot — so a literal (`42`), a unary (`-5`), a binary (`1 + 2`), and a named untyped const are
all classified by one rule. Whether the rendering is a wrapper struct comes from
`exprRendersUntypedConstWrapper`, which walks the expression for an `*ast.Ident` resolving (via
`Info.Uses`) to a `*types.Const` whose **OWN** declared type is `UntypedInt`/`UntypedRune`/
`UntypedFloat` — `info.Types[arg]` cannot answer this, since it reports plain `int` for a literal and a
named untyped const alike. A defined-type-over-int constant (`type MyInt int`) is excluded (its box is
the `[GoType]` wrapper, asserted as `MyInt`). Call arguments reuse the per-argument `castArgToType`
plumbing; the other positions wrap through `boxUntypedConstAsDefaultType`.

**Deliberate exclusions and known residues:**

- A **type-parameter parameter** constrained by `any` (`func f[T any](v T)`) reads as an empty
  interface here too, but its instantiation binds the argument to the concrete `T` (int → the `nint`
  parameter), where a bare int literal already converts implicitly. Every gate uses
  `isEmptyInterfaceTarget` (which excludes type parameters), unlike the u8-span→`@string` case, where
  a `K=string` parameter genuinely needs the cast to bind.
- An untyped **COMPLEX** constant is out of scope: a named one renders as golib `GoBigConst` (a
  `BigInteger.Parse` of the literal text — `visitValueSpec`'s `writeUntypedConst` path, with its own
  standing TODO), a separate pre-existing gap that a `(complex128)` cast would not close. A complex
  *literal* renders `1D + 2D.i()` and already boxes as `complex128`.
- Untyped **bool** constants need nothing: `true`/`false` render as C# `bool`, already the Go type.
  Untyped **string** constants DO need the cast, but under the mirror-image rule — see below.
- A cast onto an expression that already renders at the default type — a **typed** constant
  (`const seqFirst int = iota` → `const nint`), or a local untyped const that `visitValueSpec`
  tightened to a concrete C# type (`const float64 derived = 7`) — is a harmless no-op the predicate
  does not attempt to suppress. It cannot know the declaration's chosen C# spelling from the argument
  alone, and an extra cast is cosmetic noise where a missing one is a runtime divergence.

**A variadic `...any` slot used to be carved out for literals**, on the theory that a boxed `Int32`
formats identically to `nint` under `%d`/`%v` so the cast was redundant noise on the most common call
pattern. That is wrong wherever the boxed value is **compared** rather than printed:
`encoding/base32`'s `testEqual("Read after EOF, n = %d, expected %d", n, 0)` boxed `n` as `nint` and `0`
as `Int32`, `AreEqual` compared the dynamic types first, and the assert fired with a message that reads
as an equality (`n = 0, expected 0`). It is also wrong for the `%!`-verb path, where the full-conversion
`fmt` names the argument via `reflect.TypeOf(arg).String()` and a boxed `Int32` reports `"int32"`
instead of Go's `"int"`. The carve-out is gone; the corpus-wide footprint of removing it is **two
lines in two files** (`go/token/position.cs`, `internal/buildcfg/cfg.cs` — Go's own stdlib almost never
passes a bare int literal into a `...any` slot), plus two `(int32)(os.PathSeparator)` lines in
`testing/testing.cs` from the rune-kind arm, which is a genuine `%c` fix for the Phase-4 test host.

**The `any` map KEY used to be excluded too**, because golib's `map` uses the default `Dictionary`
comparer (no numeric normalization — `nint(6) != Int32(6)`) and leaving *both* the composite key and a
literal index uncast kept `map[any]int{6:1}[6]` round-tripping. That self-consistency only held for
literal-vs-literal: a lookup by a real `int` VALUE (`m[n]`, necessarily boxed `nint` — the only form Go
can even distinguish) MISSED. `convIndexExpr` now applies the same cast to an untyped-constant index of
an `any`-keyed map, so store and lookup agree on `nint` and both forms hit — while `m[int32(6)]`
correctly misses, as Go requires.

(Guarded by the `UntypedIntInterfaceBox` behavioral test — each position read back through an
`x.(int)` assertion or an `int`/`int32` type switch, output-compared vs Go — and by
`AnyBoxedUntypedConst`, which pins the whole default-type class in one program: variadic and
non-variadic slots, named/literal/rune/float/beyond-int32 constants, `[]any`/`map[any]`/`map[K]any`/
struct-field/chan-send/return/explicit-conversion positions, the `map[any]` store↔lookup round-trip
plus its `int32` miss, and the dynamic types a type switch reports — output-compared vs `go run`. The
pre-fix converter diverges on 13 of its lines.)

**An untyped `nil` in a VARIADIC slot states its element type for a different reason — arity, not dynamic type.** The constants above are cast so the *box* matches Go's; a bare `nil` is cast so the argument *exists at all*. C# prefers a call's **normal** form over its **expanded** one whenever the argument converts to the params ARRAY, and a typeless `default!` converts to `any[]` exactly as readily as to `any` — so `exec(t, db, "INSERT|t|id=10,name=?", nil)` (database/sql's `sql_test.go`) emitted `exec(…, insertTId10Nameˢ, default!)` against `params ꓸꓸꓸany argsʗp` and bound it as a **null array**. The callee saw `len(args) == 0` where Go passes one nil element (a bare `nil` is always ONE variadic element; passing the slice itself requires `nil...`), and the fake driver answered `sql: expected 1 arguments, got 0`. That is the significant part: it is a **silent behavioral divergence, not a compile error** — the emission is perfectly valid C# that means something else — which is why the variadic position needs the cast while an ordinary parameter does not (a non-variadic slot has only one form to bind, so `f(nil)` there is already unambiguous and stays bare). The fix reuses the same `castArgToType` plumbing: every trailing argument of an expanded variadic call that is the predeclared `nil` renders `(any)(default!)` — at the parameter's ELEMENT type, which `getParameterType` already yields for the variadic slot — and the nil need not be first, so the whole tail is checked. A SPREAD call (`f(args...)`) is excluded: it passes the slice whole, so there is no expansion to disambiguate and `describe(none...)` correctly yields length 0. (Guarded by the `VariadicSlotInterfaces` extension — arity read back for `nil` alone, leading, trailing and repeated, in both an `...any` and a named-interface `...Shape` slot, against no-argument, typed-value and `nil...`-spread controls, output-compared vs `go run`.)

**A SLICE or ARRAY of the ELEMENT type, passed as the SOLE argument of a variadic slot, states its element type for the SAME arity reason — and it is a C# 14 regression, not a standing defect (2026-08-24).** Go spreads a variadic argument only on an explicit `a...`; a bare `a` is one value, even when its type is exactly `[]E`. `jsValEscaper(a)` with `a []any` against `func jsValEscaper(args ...any)` (html/template `js_test.go`, whose whole table nests each case as `[]any{x}` and expects one more level of array wrapping) therefore means a pack of length one. The emission is `jsValEscaper(a)` with `a` a `slice<any>` against `params ꓸꓸꓸany argsʗp`, and `using ꓸꓸꓸany = Span<any>` — so whether Go's meaning survives depends entirely on whether C# finds the callee applicable in its NORMAL form. Under C# 13 it did not: reaching `Span<any>` from `slice<any>` needed golib's `implicit operator T[]` (slice.cs) followed by array→span, which was itself a **user-defined** operator on `Span<T>`, and C# never composes two user-defined conversions. Only the expanded form was applicable, so the slice arrived as one element and the corpus was correct **by accident**. C# 14 made array→span and string→span **standard** implicit conversions; a user-defined conversion admits one standard conversion on each side, so `slice<any>` → `any[]` → `Span<any>` became implicit, the normal form became applicable, and C# prefers the normal form — the slice silently became the entire argument list. Exactly one level of nesting disappears, never zero and never two: `"[42]"` renders as `" 42 "`, `"[[42,\"foo\",null]]"` as `"[42,\"foo\",null]"`. The remedy is the sibling rule's cast at the same `castArgToType` plumbing — `jsValEscaper((any)(a))` — which removes the normal form from consideration (nothing converts `any` to `Span<any>`) and restores the expanded one. Three deliberate narrowings keep the footprint at the mechanism: a SPREAD call is excluded (it passes the slice whole, and `anys...` really is the pack); a tail of two or more arguments is excluded (the normal form is only applicable at arity one); and a NAMED slice/array type is excluded, because its only route to a span is wrapper→`slice<T>`→`T[]`→span — two user-defined conversions, which C# 14 still does not compose. The cast is always legal where the call already compiled: binding the expanded form at all required an implicit conversion from the argument to the element type, so making it explicit cannot fail where the implicit one succeeded. (Guarded by the `VariadicSlotInterfaces` extension — a `[]any` and a `[2]any` passed whole, their `...` spread controls, a nil-slice pair separating "passed whole" from "spread", a two-argument tail control, and a named-slice control pinning the exclusion; read back both as ARITY and as rendered OUTPUT, output-compared vs `go run`. The pre-fix converter diverges on 6 of its lines under C# 14 and none under C# 13.)

**An untyped STRING constant takes the same treatment, under the MIRROR-IMAGE shape rule (2026-07-25).**
Go's default type for an untyped string constant is `string` — golib `@string` — and here it is the
LITERAL that boxes wrong: `convBasicLit` renders a string literal as a plain C# `"seed"`
(a `System.String`) or, where the position allows it, a `"seed"u8` `ReadOnlySpan<byte>` (a **ref
struct**, which cannot box at all — CS0029). A NAMED string constant needs nothing whether it is typed
or untyped, because it is emitted as an `@string` member — there is no `UntypedString` wrapper struct —
and its concatenations evaluate through `@string`'s own operators. So the string arm of
`untypedConstBoxCast` keys off the literal-only SHAPE (`constExprIsStringLiteralConcat`: string
`BasicLit`s joined by `+` through parens), exactly INVERTING the `exprRendersUntypedConstWrapper` test
the numeric arms apply.

The defect was that the coverage was **partial and therefore self-inconsistent**: the positions that
already boxed through `@string` did so via a `BasicLitContext` flag (`castToGoString`, set by
`anyBoxedStringLitContext` and its siblings, which also suppresses the `u8` form) — struct field, slice
element, map value, channel send, `return`, reassignment — while a **call argument**, a **var-spec**,
an `any`-keyed map **index lookup**, and an explicit `any("…")` conversion left the literal bare. So
`box{v: "seed"}` stored an `@string` while `eq(b.v, "seed")` passed a `System.String`, and Go's
`true`/`true` came back C# `false`/`false` — silently, with `%T` still printing `string` on both sides.
A literal CONCATENATION (`"se" + "ed"`) was uncovered at *every* position, because the flag only reaches
a `BasicLit`, and where the `u8` form survived (`new box(v: "se"u8 + "ed"u8)`) the emission did not even
compile.

Both mechanisms are kept, each doing what it is good at. The literal context still produces the tighter
`(@string)"seed"` at the positions that carry it, and the four missing positions were given it
(`convExprList` via a new `isStringBasicLit` branch beside `markAnyFieldLits`' identical either/or,
`visitValueSpec`'s `isAnyType` context, and `convIndexExpr`'s `any`-key context). `untypedConstBoxCast`
is the general net underneath — it catches the concatenations and any position that lacks the flag — and
all its application sites now route through `applyUntypedConstBoxCast`, which skips a rendering that
already leads with the cast so the two can never double up:

```go
box{v: "seed"}; eq(b.v, "seed")   // Go: true      var v any = "x"      m[any] lookup by "seed"
```
```csharp
new box(v: (@string)"seed");  eq(b.v, (@string)"seed");   // now true
any v = (@string)"x";         m[(@string)"seed"];
new any[]{(@string)("se" + "ed")};                        // the concat shape, previously bare
```

The cost is real and accepted, on the same reasoning that removed the variadic int carve-out: **every
string literal in a `...any` slot now carries the cast**, so `fmt.Println("x")` emits
`fmt.Println((@string)"x")`. That is 866 lines across 150 behavioral projects — uniform, mechanical, and
individually inspected. The alternative is a carve-out that leaves `x.(string)`, `case string:`, `==`,
and `%T` silently wrong on exactly the values a Go program is most likely to compare.

(Guarded by the `AnyBoxedUntypedConst` extension — literal, concatenation, named-untyped and named-typed
string constants at the variadic, non-variadic, var-spec, `[]any`, `map[any]` key store *and* lookup,
`map[K]any` value, keyed and positional struct-field, channel-send, `return`, explicit-conversion and
type-assertion positions, plus the dynamic type a `case string:` switch reports — output-compared vs
`go run`. The pre-fix converter leaves twelve of those renderings bare and emits four that do not compile
at all — CS0029/CS0030/CS1503 on the `"…"u8` span reaching an `object` slot.)

**The same cast applies to an interface `==`/`!=` comparison against an untyped `int` constant.** Go
compares an interface against a concrete value by its dynamic type *and* value, which the converter
lowers to golib's reflective `AreEqual` (`convBinaryExpr`'s interface-comparison branch — the
`iface == concrete` / `iface == iface` / `iface == ptr` cases). `AreEqual(object, object)` bails early
on `leftType != right.GetType()`, so a comparison operand's boxed *runtime* type must match, exactly as
a stored-then-asserted value's does. A bare C# int literal boxes as `System.Int32`, so `e.Value != 1`
against an `any` field holding a boxed Go `int` (`nint`/`IntPtr`) — container/list's
`TestIssue6349`, emitted `!AreEqual((~e).Value, 1)` — saw `IntPtr != Int32`, reported the values
UNEQUAL, and fired the test's error even though the value round-tripped as `1`. The interface-comparison
branch now casts the concrete constant operand to its default type (`!AreEqual((~e).Value, (nint)(1))`),
reusing the same `untypedConstBoxCast` predicate the boxing positions above key off — so the literal
boxes as Go's `int` dynamic type and the runtime-type guard passes. The cast is confined to the
`AreEqual` lowering (interface-vs-concrete / interface / pointer), so a bare int compared against a
*concrete* `int` — which lowers to C#'s native `==`, never `AreEqual` — stays bare (no noise). The
predicate yields nothing for the interface operand itself and for any non-constant operand, so only the
genuine boxed-constant-mismatch site is touched. (Guarded by the
`InterfaceUntypedIntCompare` behavioral test — an `any`-field-holding boxed int compared `==`/`!=`
against an int literal, negative-literal and literal-on-the-left forms, output-compared vs Go; the
pre-fix converter emits the bare literal and mis-reports every comparison unequal.)

## An ELIDED inner literal of `any` elements boxes them as the typed literal does

The two rules above apply to every element of a slice or array literal whose element type is an empty
interface, including an inner literal whose type is ELIDED: the `{1, 2}` and `{"a", "b"}` of a
`[][2]any{{1, 2}, {"a", "b"}}`. An elided inner literal goes through a smaller renderer than the typed one, and that
renderer used to hand its elements a context with neither rule, so a string literal stayed a `u8` span, which has no
conversion to `object` (CS0029, BurntSushi/toml's encode test), and an untyped constant boxed as C# `int`, so a later
`x.(int)` read false where Go reads true. `elidedElemContext` now gives such a literal the typed path's element
marking (`markEmptyInterfaceElems`, shared by both renderers), so the two spellings of one Go type emit alike:

```csharp
var a = GoReflect.WithElemDims(new array<any>[]{new any[]{(nint)(1), (nint)(2)}.array(), new any[]{(@string)"a"u8, (@string)"b"u8}.array()}.slice(), 2);
```

A pointer, func or `unsafe.Pointer` element takes its box the same way. An elided literal of any other element type,
and an elided literal with nothing to mark, keep their exact rendering. The type-aware census of the shape read 0
sites in the converted standard library (production on three targets, tests on two) and 0 across 757 behavioral
modules. Guarded by `elidedAnyCompositeElems_test.go` and the `ElidedAnyCompositeElems` behavioral test, which fails
on the pre-change converter with CS0029 x4.

## Comparing two interfaces of one UNCOMPARABLE dynamic type panics, as Go does

Go decides an interface `==` in three steps, and only the third can panic:

1. a **nil** operand makes it a nil test — `m == nil` is false for an interface holding a map, and
   never panics;
2. a **dynamic-type mismatch** answers false — `m == 5` never panics either;
3. only once both operands carry the **same** dynamic type does the runtime run that type's equal
   algorithm — and an uncomparable type has none, so it panics
   `runtime error: comparing uncomparable type T`.

`builtin.AreEqual(object?, object?)` implemented the first two and answered step 3 quietly with a
`bool`, for every shape but one: a **map**, **slice** or **func** held in an interface, and a
**struct or array that transitively contains one**. The single covered shape — two adapters over a
nil named-func delegate — was a special case of exactly this rule, and now routes through the same
mint (`RuntimeErrorPanic.ComparingUncomparableType`) rather than restating the message.

The gate sits **after** both nil legs and the dynamic-type check, which is what makes it safe against
the ~1,300 emitted `AreEqual` call sites: it is reached only where Go itself would have run the equal
algorithm and found none, and the converter emits `AreEqual` only for a comparison Go's own type
checker admitted. The panic is a recoverable runtime error, so `recover()` observes it exactly as in
Go.

The message spells the dynamic type as Go spells it, which takes two things the managed type alone
cannot supply:

| Go value in an `any` | reported type |
|---|---|
| `map[string]int{}` | `map[string]int` |
| `[]int{1}` | `[]int` |
| `func(){}` | `func()` |
| `withSlice{1, []int{2}}` | `main.withSlice` — the STRUCT, not its field |
| `myMap{}` (`type myMap map[string]int`) | `main.myMap` — its OWN name |
| `[1][]int{{1}}` | `[1][]int` — the LENGTH is part of the type |

The length comes off the live operand (`GoReflect.ArrayDimsOfValue`), because a managed `array<T>`
does not carry it in its `Type` — only the value knows. Comparability itself is **not** restated
here: it delegates to `GoReflect.IsComparable`, already the signal the reflection bridge populates
`abi.Type.Equal` from, so `==` and `reflect.Type.Comparable` answer from one definition. The verdict
is immutable per type and cached in a `ConcurrentDictionary<Type, bool>`, since `==` is a hot path
(roughly every `err == io.EOF` in the corpus) and an uncached walk would re-reflect over a struct's
fields on every comparison.

**A struct's interface-typed FIELD recurses correctly** — `TypeGenerator` already compares such
fields through `AreEqual` (see *the interface field an emitted `Equals` must route through
`AreEqual`*), so `struct{ V any }` holding a map panics naming `map[string]int`, the inner type, just
as Go does.

**Stated residual — an ARRAY that reaches an uncomparable value through an interface.** Measured
against go1.23.12: `[1]any{map…}`, `[1]any{[1]any{map…}}` and `[1]withAny{…}` (a struct with an `any`
field) all panic in Go and all answer quietly here. Two distinct mechanisms sit behind that, and
neither is the gate above:

- a SELF-comparison never reaches an element at all — `array<T>`'s structural equality
  short-circuits on backing-store reference identity, so `a == a` is true before any element is
  examined. This is visible in the `[1]withAny{…}` row, whose per-element comparer *would* have
  panicked (the struct routes its `any` field through `AreEqual`), and did not;
- for genuinely distinct arrays, `array<T>` compares elements with `EqualityComparer<T>.Default`
  rather than Go's relation, so an `[N]any` never consults `AreEqual` for its elements.

Closing either means changing `array<T>`'s equality, which also decides `GetHashCode`, array-typed
map keys and `DeepEqual` — and the element-comparer half is the same hole
`GoEqualityComparer.ForKeys<T>()` closed for map keys, whose doc records a deliberate decision to
leave float-containing arrays on the BCL rule. Left as a measured residual with no known consumer
rather than covered speculatively (the r39d rule).

(Guarded by the `UncomparableEquality` behavioral test — every panicking shape with its message
asserted verbatim, every nil and type-mismatch non-trigger, and the comparable positive controls,
output-compared vs `go run`.)

## A parenthesized asserted type is rendered without its parentheses

Go lets the asserted type carry parentheses: objx writes `v.data.((Map))`. They are syntax only, so the
converter renders the type unwrapped, `v.data._<Map>()` and `v.data._<Map>(ᐧ)` for the comma-ok form, because a
C# type argument cannot be parenthesized (`_<(Map)>()` is CS1525). A parenthesized pointer type takes the same
route: `p.((*point))` becomes `p._<ж<point>>(ᐧ)`. (Guarded by the `ParenthesizedTypeAssertion` behavioral test.)

---

[← Nil and Zero Values](nil-and-zero-values.md) · [Index](README.md) · [Multi-Assignment and Evaluation Order →](multi-assignment.md)
<!-- {% endraw %} -->
