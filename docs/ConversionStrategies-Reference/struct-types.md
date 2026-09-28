# Struct Types

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#struct-types)
Go structs are converted to C# `struct` types and used on the stack to optimize memory use and reduce GC pressure; when an instance must escape the stack it is wrapped in a heap box, [`ж<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) (see [Pointers](pointers.md#pointers)). Rather than spell out the whole struct body, the converter emits a partial struct carrying a `[GoType]` attribute, and the `TypeGenerator` source generator synthesizes the members (equality, `ISupportMake`, embedding promotion, etc.):

```csharp
[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}
```

The generator also chooses the access modifier from the Go name (exported → `public`, unexported → `internal`), except where the converter emits an explicit modifier — for instance, an unexported type used as the type of an *exported* field is published as `public` to satisfy C# accessibility (the converter emits `public partial struct …` and the generator honors that explicit modifier).

The synthesized value-equality body compares the struct's fields against a parameter named `other` (`public bool Equals(Person other) => this.Name == other.Name && this.Age == other.Age;`). Each comparison's **left operand is qualified with `this.`** so a Go field whose name happens to collide with the parameter — a field literally named `other` — still binds field-to-field. Without the qualification a `type holder struct { mark int; other int }` would emit `other == other.other`, where the left `other` resolves to the *parameter* (a `holder`) rather than the field (an `int`), failing to compile with CS0019 (`==` cannot be applied to `holder` and `int`). `GetHashCode`/`ToString` reference the same field names but have no colliding parameter, so they need no qualification. (Guarded by the `StructFieldNamedOther` behavioral test.)

A combined Go field declaration — `x, y int` — emits a single combined C# line (`internal nint x, y;`) so the output mirrors the Go source's line grouping. The combined form is only used when every name in the group shares the same emitted type and access modifier and none needs per-name special handling; otherwise the converter falls back to one line per name. The fallback applies when any of these hold: a blank field `_` (renamed per occurrence — `_`, `__`, …), a name equal to the enclosing struct type (renamed with the `Δ` collision marker), a per-field array initializer (` = new(N)`), or a mix of exported and unexported names in the same group (`X, y int` → `public nint X;` / `internal nint y;`). Field comments and tags attach to the whole Go field, so they never diverge within a group.

C# does not allow inline or intra-function type definitions, so these are "lifted" out of the function. A **named** local type is lifted with its enclosing function's name as a prefix to avoid collisions — a `type x struct{…}` declared in `main` becomes `main_x`. An **anonymous** struct (or an anonymous struct used as a field/value) is lifted to a synthesized name with a `ᴛ`*N* suffix and marked dynamic, e.g. `[GoType("dyn")] partial struct settingsᴛ1`. Struct "definitions" that match structurally remain usable interchangeably (the generator and implicit conversions handle this). A reference to a lifted type as a bare identifier is renamed to the lifted name, and so is its use as a **slice or array element type** — `[]entry` (where `entry` is a local type) emits `slice<process_entry>`, not the short `slice<entry>` (which is unresolved at package scope → CS0246). The element is resolved through the same lift registry as the bare-identifier and anonymous-struct cases. (Guarded by the `LocalTypeSliceElement` behavioral test, covering both the slice and fixed-array forms; runtime hit this on `printDebugLog`'s `[]readState` and `traceAdvance`'s `[]untracedG`.)

**The `dyn` marker is also the type's RUN-TIME identity, and it must not leak the synthesized name.** An unnamed Go struct has no name to report, so `reflect.Type.String()` and `%T` render it STRUCTURALLY — `struct { X int; y int }`, and `struct {}` for golib's `EmptyStruct`. Because a lift gives the type a synthesized C# name (`settingsᴛ1`), that name would otherwise be what reflection reports; `[GoType("dyn")]` is exactly what distinguishes a lift from an ordinary declared struct, whose Go name IS its own. golib's `GoReflect.TypeNaming` therefore renders a `dyn`-marked value type from the `GoFields` projection — the same field table `NumField`/`Field` and the value side read, so a type's reported name and the fields it hands out cannot disagree — following Go's format exactly: an embedded field contributes its type alone, a tagged field appends the `strconv.Quote`d tag. Before it, `go/ast`'s `TestPrint` reported `ast_internal_test.typeᴛ1` where Go prints `struct { X int; y int }`, and `internal/platform`'s decode error named `[]platform_test.listEntry` rather than Go's structural spelling. (Landed 2026-08-09 with `go/ast`'s 9/9 bank; see [`docs/phase4/DESIGN-reflection-bridge.md`](../phase4/DESIGN-reflection-bridge.md).)

A **map whose VALUE type is an anonymous struct** is lifted the same way. A package-level `var m = map[K]struct{…}{…}` — crypto/internal/hpke's `SupportedKEMs` (`map[uint16]struct{ curve ecdh.Curve; hash crypto.Hash; nSecret uint16 }`) and `SupportedAEADs` — names its value struct through the lift so the map type reads `map<uint16, SupportedKEMsᴛ1>`; without it, `getAliasQualifiedTypeName`'s map arm stringified the value as raw Go `struct{…}` syntax straight into the C# map signature (`map<uint16, struct{ curve ecdh.Curve; … }>`) — which C# cannot parse (a CS1519/CS1003 syntax cascade). `extractStructType` already lifts a slice/array **element** struct (its `ArrayType` arm) but has no map arm, so a dedicated `extractMapValueStructType` lifts the map **VALUE** struct at the package-level value-spec composite-literal site. The keyed element literals stay the target-typed `new(…)` constructor form — `[0x0020] = new(ecdh.X25519(), crypto.SHA256, 32)` — which binds to the lifted struct's generated constructor; a func-typed value field (SupportedAEADs' `aead func([]byte) (cipher.AEAD, error)`) lifts to a `Func<…>` field that a method-group or func-value element still fills. Both the declaration type (`getCSharpTypeName` → the map arm) and the literal's own type render (`convMapType` → `getExpressionTypeName`) resolve the value through the shared `liftedTypeMap` (the lift runs before the initializer is converted). (Guarded by the `MapAnonStructValue` behavioral test — a package-level map with an anonymous-struct value type, including a func-typed field, constructed and read back by key, output-compared vs Go.)

The **empty struct `struct{}` is never lifted** — it maps to the shared golib `EmptyStruct`, so a `struct{}{}` composite literal emits `new EmptyStruct()` and a `map[K]struct{}` ("set") emits `map<K, EmptyStruct>`. Lifting an empty struct would be doubly wrong: it has no fields to model, and the lift mis-attributes its name and identity. When the `struct{}{}` is the value assigned to a map element (`seen[k] = struct{}{}`), the enclosing assignment passes the **LHS ident** (`seen`) into the struct-conversion context to name the lift — so the empty struct was being lifted to `<func>_seen` *and registered under `seen`'s own type, the map* `map[K]struct{}`, in the lifted-type registry. That poisoned every later reference to that map type: the function parameter `seen map[K]struct{}` rendered as the phantom struct instead of `map<K, EmptyStruct>`, and its comma-ok deconstruction (`(_, ok) = seen[k]`) and two-arg indexer vanished (CS8130/CS0021), while real-map call sites mismatched (CS1503). `convStructType` now short-circuits an empty struct to `EmptyStruct` before any lift, mirroring the `!isEmptyStruct` guard that `extractStructType` already applies everywhere else. (Guarded by the `EmptyStructMapSet` behavioral test; runtime hit this on `typesEqual`'s `seen map[_typePair]struct{}` parameter.)

An **empty `interface{}` field is never lifted** either — it maps to `any`, exactly as a bare `interface{}` type does. When `visitStructType` lifts an anonymous struct it walks its fields and lifts any *anonymous interface* field to its own named `[GoType("dyn")]` interface. Those three inline lift sites (a plain `interface{}` field, a `*interface{}` field, and a `[]interface{}`/`[N]interface{}` element) type-asserted `*ast.InterfaceType` directly, diverging from `extractInterfaceType` — the canonical lift gate, which already excludes empty interfaces. So encoding/json's slice-encoder cycle memo — `ptr := struct{ ptr interface{}; len int }{v.UnsafePointer(), v.Len()}` — lifted its `ptr interface{}` field to a named empty **marker** interface `encode_ptr_ptr`. A named empty interface is implemented by nothing, so constructing the struct from the boxed `uintptr` failed (`cannot convert from 'uintptr' to '…encode_ptr_ptr'`, CS1503). The three sites now carry the same `!isEmptyInterface` guard, so an empty-interface field falls through to the normal field-type conversion and renders `any` (`*interface{}` → `ж<any>`, `[]interface{}` → `slice<any>`). (Guarded by the `AnonymousStructs` extension `cycleMemo` — an in-function anonymous struct with an `interface{}` field constructed from a pointer, read back through the field, and used as a map key, output-compared vs Go; fails CS1503 without the guard. Part of greening encoding/json.)

**A returned anonymous-struct composite literal records its implicit conversion AFTER it is lifted.** Two structurally-identical anonymous structs are the *same* Go type but the converter lifts each occurrence to a *distinct* C# name, so a conversion between them must be bridged by a recorded `[assembly: GoImplicitConv<…>]` (the `ImplicitConvGenerator` emits the operators). A closure whose **result type** is an anonymous struct that `return`s a **composite literal** of the identical anonymous struct — `mk := func(…) struct{ptr any; len int} { return struct{ptr any; len int}{p, n} }` — lifts the closure-result type (`…_func_R0`) and the composite-literal type (`…_type`) separately, and `visitReturnStmt`'s `checkForDynamicStructs` records the conversion between them. Each side's C# name is resolved through the per-file lifted-type registry (`liftedTypeMap`), but a function-local composite literal is only *added* to that registry **during its own `convExpr`**. The recording therefore had to move to run **after** the result expression is converted: reading the arg's type earlier found it unlifted and stringified it as raw Go `struct{…}` text — an invalid C# generic argument in the emitted attribute (`[assembly: GoImplicitConv<struct{ptr interface{}; len int}, …_func_R0>]`, CS1031 "Type expected"). Recording after the lift resolves both sides to their lifted names (`[assembly: GoImplicitConv<…_type, …_func_R0>]`). The `dynamicCast` template `checkForDynamicStructs` may return is applied to the already-converted result expression identically either way, so the reorder is otherwise output-neutral (the full-stdlib A/B reconvert is byte-identical). This is latent for the current stdlib (encoding/json builds its identical memo struct once into a variable, avoiding a second same-shape lift). (Guarded by the `ClosureReturnAnonStruct` behavioral test.)

## Lifted anonymous structs embedding an interface
archive/tar's ReadFrom-hiding shape — `io.Copy(struct{ io.Writer }{tw}, r)` — exercises four
coupled rules: a SELECTOR embed's interface check resolves the **Sel** (`Writer`), not the
package ident (`io`), so the cross-package interface embed emits as a plain interface FIELD
(the promoted-struct property form made the generator construct the interface — CS0144); the
composite literal routes the element through the interface conversion **at render**
(`interfaceTypes[i]`, not just the record-only call); a receiver placed into an
INTERFACE-typed composite field triggers **direct-ж** (Go's interface holds the `*T`, so the
pointer adapter wraps the box `Ꮡfr`); and the generator emits ONE value-form impl per
(struct, interface) pair, folding a Promoted duplicate in (CS0111). ARGUMENT-position values
of a mismatched delegate type wrap in the named delegate's constructor exactly like
composite-literal fields (generic delegate params stay native — unsubstituted type params
cannot render). Guarded by `AnonymousInterfaces` (`tally`/`fill`, `byteRepeat`, and the
named-array `quad`/`frame` Range slice).

The same interface-field record+route also fires for a **named non-struct** element that
implements the field's interface. The gate above triggered only when the element's underlying is a
struct (or the field is embedded), so a named scalar with a method set — hpack's
`DecodingError{InvalidIndexError(idx)}`, where `type InvalidIndexError int` has an `Error()` method
satisfying the `error` field — recorded no `GoImplement<InvalidIndexError, error>` and passed the
value bare to the interface-typed constructor parameter (which surfaces as `NilType`, CS1503). The
gate now also fires when a named, non-struct, **non-interface** element type `types.Implements` the
field's interface — mirroring the call-argument path, which routes any argument into an interface
parameter with no struct-only restriction. Recording the `GoImplement` is what clears the error (the
generated implementation makes the scalar implement the interface, so the bare pass then converts
implicitly); the render routing (`interfaceTypes[eltIndex]`) is set too, matching the struct case. An
interface-typed element is excluded — it is already the interface and needs no adapter. (Guarded by
the `InterfaceFieldNamedScalar` behavioral test — a named `int` into an `error` field and a named
`string` into a local interface field, positional and keyed forms, output-compared vs Go.)

**Naming a lift that has no name source — `new(struct{ SomeIface })`.** Every dyn-lift derives its
C# type name from context (the declared var, the struct field, the parameter name). A package-level
`var reserved = new(struct{ types.Type })` (go/internal/gccgoimporter's singleton) had NO source:
the initializer is a CallExpr (the composite-literal up-front lift didn't fire), so the declaration
type fell to the raw `t.String()` mangle (`ж<types.Type}>`), and the lift arrived late from the
call-argument path under builtin `new`'s UNNAMED parameter — an EMPTY lift name, declaring
`partial struct  {` and registering `""` for every reference (`@new<>()`,
`[assembly: GoImplement<, …>]`, `new жΔType(…)` — a whole-package syntax cascade). Two-part fix:
`visitValueSpec` lifts a `new(struct{…})` initializer's struct UP FRONT under the var's name
(mirroring the composite-literal and hpke map-value lifts), so the declaration, the `@new<…>` type
argument, the `GoImplement` recordings, and the pointer-adapter names all resolve through
`liftedTypeMap`:
```csharp
[GoType("dyn")] partial struct reservedᴛ1 {
    public global::go.go.types_package.ΔType Type;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();
p.typeList[n] = new reservedᴛ1жΔType(reserved);
```
and `visitStructType` itself falls back to the generic `"type"` when a lift arrives with an empty
name (the FUNCTION-LOCAL `x := new(struct{…})` form still reaches it through the unnamed-parameter
path → `main_type`), so no caller can produce an unnamed type declaration. (Guarded by the
`NewAnonStructIfaceEmbed` behavioral test — the package-level singleton converted to its embedded
interface through the lifted type's pointer adapter, the embedded field filled and called through
the promotion, plus the function-local form — output-compared vs Go.)

## An anonymous struct lifts from ANY depth of its declared type
The lift only happens if the converter can *find* the `struct{…}` literal in the declaration it is
converting, and the probe that found it used to look exactly one level down: through a pointer, or
through a slice/array element, or (a later addition) a map value — never through a composition of
those. So `[]struct{…}` lifted and `[]*struct{…}` did not, and net's

```go
var ipStringTests = []*struct {
	in  IP     // see RFC 791 and RFC 4291
	str string // see RFC 791, RFC 4291 and RFC 5952
	byt []byte
	error
}{ … }
```

emitted the *raw Go type text* into the C# declaration — `slice<ж<struct{in net.IP; str string; byt
[]byte; error}>>` — which is not C# at all: **CS1031 `Type expected`**, followed by a 90-error
syntax cascade that hid every real diagnostic in the file behind it. The shape had been invisible
because a composed occurrence still resolved *if some other declaration in the package happened to
register the identical signature first*; `error` embedded here makes the signature unique, so
nothing did.

The probe is now a recursive descent over the type-composing syntax — pointer, array/slice element,
`...T`, parenthesization, map value then key, channel element — so an anonymous struct (and, by the
same helper, an anonymous interface) is found wherever it sits. The `AnonStructComposedTypes` golden
shows the same shape lifting and its elements constructing normally:

```csharp
[GoType("dyn")] partial struct ptrElemsᴛ1 {
    internal nint @in;
    internal @string str;
    internal error error;
}
internal static slice<ж<ptrElemsᴛ1>> ptrElems = new ж<ptrElemsᴛ1>[]{
    Ꮡ(new ptrElemsᴛ1(1, "one"u8, default!)),
    Ꮡ(new ptrElemsᴛ1(2, "two"u8, default!))
}.slice();
```

This strictly widens what lifts: anything that lifted before still lifts, under the same name. The
walk is deliberately **first-match**, because each lifting caller can name only one anonymous type
per declaration — a type expression carrying two *distinct* anonymous literals (`map[struct{…}]struct{…}`)
lifts the value's and leaves the key's. That residual used to apply to every composed shape; it is
now confined to that one. The map-value case had its own one-off probe, which the recursion subsumes
and which was deleted with it.

Scope, measured by a whole-standard-library A/B reconvert against a converter built from the previous
commit — **not** by a source scan, which got this wrong. A grep for the shape found it only in
`_test.go` files and would have concluded the production corpus was untouched; the A/B found
`encoding/gob/type.cs`, because `bootstrapType("_reserved1", (*struct{ r7 int })(nil))` reaches its
anonymous struct through a *parenthesized pointer conversion* — `(…)` then `*` then the literal — a
composition the scan's pattern never looked for. (Charter §9's false-alarm rule, from the other
direction: a zero-hit scan is only as good as its positive controls, and this one had a control for
`[]*struct{…}` and none for `(*struct{…})`.) The corpus footprint is exactly those seven `tReservedN`
declarations, and the change there is a **naming improvement, not a behavior change**: the lift now
happens at the call argument, where it takes the parameter's name (`eᴛ1`…`eᴛ7`), instead of arriving
late from `convStarExpr`'s fallback as the generic `Δtype`/`Δtypeᴛ1`…. Declarations, uses and the
`package_info.cs` accessibility block all move together. (Guarded by the
`AnonStructComposedTypes` behavioral test: a slice of
pointer-to-anonymous-struct with an embedded `error`, a map to pointer-to-anonymous-struct, and a
slice of slice of anonymous struct, read and written through and output-compared vs Go.)

**The struct-FIELD arm was the same probe, and it now shares the same descent.** `visitStructType`
kept its *own* hand-written peel — a chain of `field.Type.(*ast.StarExpr)` / `.(*ast.StructType)` /
`.(*ast.InterfaceType)` / `.(*ast.ArrayType)` arms, each looking exactly one level down — so a struct
field declared `[N]struct{…}` lifted while `[N]*struct{…}`, `[]*struct{…}`, `map[K]struct{…}` and
`chan struct{…}` fell through to the same raw-Go-text emission. That the map case had already been
patched in as its own arm rather than as a rule is the shape a point-repair leaves behind, and it is
what marked this as the next site. The arm now calls `extractStructType` / `extractInterfaceType`,
the identical helpers every other lift site uses:

```go
type Composed struct {
	Ptrs  [2]*struct{ Size uint32 }
	ByKey map[string]struct{ Count int }
}
```
```csharp
[GoType("dyn")] partial struct Composed_Ptrs  { public uint32 Size; }
[GoType("dyn")] partial struct Composed_ByKey { public nint Count; }

[GoType] partial struct Composed {                     // package_info.cs records [GoValueClone("Ptrs")]
    public array<ж<Composed_Ptrs>> Ptrs = new(2);
    public map<@string, Composed_ByKey> ByKey;          // was: map<@string, struct{Count int}>
}
```

Two properties keep the shared helper faithful to what the arm did before. The lift name stays
`<struct>_<field>`, which is well-defined for every shape because a field type carrying an anonymous
literal always *names* the field — the Go spec makes an embedded field a type name, never a literal.
And **sub-struct tracking** (`subStructTypes`, which feeds `addImplicitSubStructConversions`) still
records only the two shapes it ever recorded — the field *is* the anonymous struct, or a pointer
straight to it — because that map describes the field's own declared type; a struct reached through a
slice/array/map/channel element is not the field's type and never was tracked.

Measured by the same whole-standard-library A/B, the widening has **no corpus consumer today**: no
converted package declares a composed anonymous-struct field. What the A/B *did* change is four files
in two packages, all one incidental canonicalization — the shared helpers exclude the **empty**
`struct{}`/`interface{}`, and the old field arm did not:

```csharp
- [GoType("dyn")] partial struct Func_opaque { }          // …and NamedArg__NamedFieldsRequired, Out__…
- [GoType] partial struct Func { internal Func_opaque opaque; }
+ [GoType] partial struct Func { internal EmptyStruct opaque; // unexported field to disallow conversions
```

Go's `opaque struct{}` is `struct{}`, and golib's `EmptyStruct` is what every other site already maps
it to — so `runtime.Func`, `database/sql`'s `NamedArg` and `Out` stop minting a private empty type
apiece, three `[GoType("dyn")]` declarations and their `package_info.cs` entries disappear, and the Go
trailing comment lands back where Go writes it. Nothing referenced the removed names (verified across
the whole reconverted corpus, with the baseline emission as the positive control), and the 302-package
corpus builds with 0 errors. (Guarded by the `AnonStructArrayElement` behavioral test, extended: a
`[2]*struct{…}` field, a `[]*struct{…}` field, a `map[K]struct{…}` field and a `map[K]interface{…}`
field, each read back through its lifted type, alongside the pre-existing one-level `[N]struct{…}`
control and the parenthesized `(*struct{ r7 int })(nil)` conversion. Its A/B reproduces the defect
directly: against the previous binary the three struct fields emit raw `struct{…}` text. The composed
fields are read at their ZERO values on purpose — *constructing* a value of an anonymous struct type
lifts a second, function-scoped name for the same Go type, and a container of it has no implicit
conversion to bridge the two. That is the recorded cross-context anonymous-lift identity split, which
applies equally to the one-level shape and is a separate increment.)

## An INITIALIZED var lifts its explicit anonymous declared type too — and a blank name lifts from the GO identifier

`visitValueSpec` lifts a var whose DECLARED type is an anonymous struct/interface literal, but
until 2026-08-09 only on the BODYLESS arm (`var x struct{…}`). Give the same var an
**initializer** and nothing lifted it, so the raw Go text landed in both the declaration type
and the value adapter's class name — and its braces close the C# member, making every following
declaration in the file read as a namespace-level one:

```go
// crypto/ecdh's test half opens with the documented-interface witness idiom:
var _ interface{ Equal(x crypto.PublicKey) bool } = &ecdh.PublicKey{}
```
```csharp
// before — CS1519/CS1002 at the site, then CS0106 on every remaining member, CS1022 at EOF:
internal static interface{Equal(x crypto.PublicKey) bool} _ᴛ1ʗ =
    new ecdhꓸPublicKeyжinterface{Equal(x crypto.PublicKey) bool}(Ꮡ(new ecdhꓸPublicKey(nil)));
// after:
[GoType("dyn")] partial interface _ᴛ1 { bool Equal(cryptoꓸPublicKey x); }
internal static _ᴛ1 _ᴛ1ʗ = new ecdh.ΔPublicKeyж_ᴛ1(Ꮡ(new ecdhꓸPublicKey(nil)));
```

The initialized arm now performs the bodyless arm's lift (both the struct and the interface
twin). Ordering is not a constraint: the adapter name is minted EARLIER in the same iteration by
`convertToInterfaceType`, but as a deferred `«DYNTYPE:…»` marker, so a lift registered afterwards
still resolves it at the file-visit barrier.

The lift is named from the **GO** identifier, not from `csIDName`. For an ordinary name the two
agree (`csIDName` is that name sanitized, and `getUniqueLiftedTypeName` re-sanitizes its
argument), but a BLANK `_` var's `csIDName` is a synthesized temp (`_ᴛ1ʗ`) that exists in no Go
scope — so `getUniqueLiftedTypeName`'s `typeExists` check cannot see it and hands the type the
field's own name back, giving one class a nested type and a field both called `_ᴛ1ʗ` (CS0102).
Passing `_` finds the blank var among the package's defs and bumps the type to `_ᴛ1`, distinct
by construction. (Guarded by the `AnonInterfaceVarWitness` behavioral test — two blank witnesses
over different anonymous interfaces, a NAMED anonymous-interface var that is then called through
its adapter, an anonymous-struct declared type, and a local interface value of the witness type,
output-compared vs Go; and by `crypto/ecdh`'s banked 47-verdict suite, which is where it was
found.)

## Every type-name render resolves a lifted anonymous struct cross-file

The registry/marker resolution above initially covered only two dedicated call sites
(`dynamicStructTypeName`'s `ж.of(…)` address-of-field form and `convertToInterfaceType`), while
the GENERAL type-name renderers — `getAliasQualifiedTypeName`/`getFullyQualifiedTypeName`, which every other emission
path reaches (heap-box declarations, casts, generic arguments…) — still fell through to raw
`t.String()` Go text on a `liftedTypeMap` miss. So ranging over a package-level anonymous-struct
slice declared in a SIBLING file, with the loop variable escaping to a heap box, stringified the
element type into the box declaration: bytes' `compareTests` (`[]struct{a, b []byte; i int}`,
declared in compare_test.go, ranged from the earlier-sorted bytes_test.go) emitted
`ref var tt = ref heap(new struct{a <>byte; b <>byte; i int}(), …)` — CS1526 plus a ~170-error
parser cascade that blocked all of bytes (Phase-4 blocker B8).

Both renderers now resolve a NON-EMPTY anonymous struct/interface through
`deferredDynamicTypeName` before the `t.String()` fall-through: the shared
`packageDynamicTypeNames` registry (the declaring file may already have been visited — file
visits run in deterministic sorted-file order), else the deferred `«DYNTYPE:…»` marker. The
empty `struct{}`/`interface{}` are excluded — their raw signatures intentionally map to
`EmptyStruct`/`any` downstream. The marker payload is now the HEX-ENCODED signature rather than
the raw text: these general render paths flow through string transformation passes
(`convertToCSTypeName` rewrites every `[`/`]` to `<`/`>`, alias handling splits on `.`) that
would corrupt an embedded raw signature before the post-barrier resolution could match it back
to the registry; hex digits pass through every transform untouched, and the encoding is a pure
function of the signature so equal signatures still render the identical (comparable) string.
Emitted form:

```csharp
// zvars.cs (declaring file, visited AFTER the reference):
[GoType("dyn")] partial struct compareTestsᴛ1 { … }
internal static slice<compareTestsᴛ1> compareTests = …;
// main.cs (cross-file range + heap box):
foreach (var (_, vᴛ1) in compareTests) {
    ref var tt = ref heap(new compareTestsᴛ1(), out var Ꮡtt);
    …
}
```

Guarded by `AnonStructCrossFile` (`zvars.go` declares `compareTests` and sorts after `main.go`,
forcing the marker path; `avars.go` declares `sizeTests` and sorts before it, taking the direct
registry hit — main.go ranges over both with `&tt`/`&st` forcing the heap box, output-compared vs
Go).

## A lifted type name is unique across the PACKAGE, and the `-tests` variant inherits production's

Resolution (above) is one half; **naming** is the other. Every lifted type — an anonymous
struct/interface, or a function-local declaration hoisted out of its body — is emitted as a
**nested type of the single `<pkg>_package` partial class**, so its name has to be unique across
the whole package. The uniquing set was per-FILE, which is a scope narrower than the emission
target: two sibling files whose lifts reach for the same generated name each believed the name
free and both declared it.

Both spellings a lift can start from are exposed to this. An anonymous type with no name of its
own falls back to the generic `type` (rendered `Δtype`, then `Δtypeᴛ1`, `Δtypeᴛ2`, … per
collision), and a function-local declaration is prefixed with the **method name only** — which
sibling files legitimately share, since Go allows one `probe` method per receiver type.
encoding/gob hit both at once: production `type.cs` and the internal-variant `encoder_test.cs`
each lifted a differently-shaped `struct{…}` to `Δtype`/`Δtypeᴛ1`, and the class then carried two
definitions of each — CS0579 on the doubled `[GoType]` attribute plus CS0111/CS0557 on every
member `go2cs-gen`'s `TypeGenerator` emitted for the duplicate (32 errors, the whole package
blocked). Note the failure is **not** avoided when the two anonymous structs happen to be
structurally identical: the second `[GoType("dyn")]` is still a duplicate attribute.

The claim set is therefore package-scoped (`packageLiftedTypeNames`, reset per package/variant),
with two deliberate exemptions:

- A `[module: GoManualConversion]` file **does not claim**. Its emission is redirected to a
  non-compiled `.cs.auto` review sibling, so a claim there would push a real file's type name to a
  higher ordinal for a declaration that never compiles. Those visitors keep the per-file set alone.
- The `-tests` **INTERNAL** variant is pre-seeded with the names the production conversion claimed
  (`productionLiftedTypeNames`). That variant emits its `_test.go` files into the production
  package class while the production `.cs` on disk are **not** regenerated, so those names are
  immutable and the test-side lift is the side that moves — the same production-pinned rule
  `testMethodRenames` applies to declarators and the Tier-C hoist seed applies to literal fields.
  The seed is the production run's live claim set, captured in `convertTestVariants` before the
  first variant's `resetPackageState` (production conversion runs moments earlier in the same
  process). The **EXTERNAL** variant is not seeded: its `<pkg>_test_package` is a separate class
  and may reuse every production name freely.

```csharp
// type.cs (production, pinned):        encoder_test.cs (internal variant, steps around):
[GoType("dyn")] partial struct Δtype {  [GoType("dyn")] partial struct Δtypeᴛ7 {
    internal nint r7;                       internal nint A;
}                                       }
```

Residual: two package-level anonymous structs that are structurally IDENTICAL but declared in
different files still lift to two distinct C# types (one Go type split in two) rather than
sharing one. That combination cannot compile today either — it is the CS0579 case above — so
nothing regressed; unifying them needs the second declaration's *emission* suppressed, not just
its name reused.

Guarded by `AnonStructCrossFile`'s `bvars.go`/`yvars.go` (both manifestations, straddling
`main.go` so file order is exercised in both directions) and, for the `-tests` seed,
`TestTestVariantPinsProductionLiftedTypeNames`.

<a id="a-global-addressed-only-by-the-packages-own-_testgo-is-still-heap-boxed"></a>Moved to [A global addressed only by the package's own `_test.go` is still heap-boxed](pointers.md#a-global-addressed-only-by-the-packages-own-_testgo-is-still-heap-boxed).

<a id="astral-rune-literals"></a>Moved to [Astral rune literals](strings.md#astral-rune-literals).

<a id="type-switch-default-arm-binds-the-interface-value"></a>Moved to [Type-switch default arm binds the interface value](type-switch.md#type-switch-default-arm-binds-the-interface-value).

<a id="the-type-switch-tag-evaluates-exactly-once"></a>Moved to [The type-switch tag evaluates exactly once](type-switch.md#the-type-switch-tag-evaluates-exactly-once).

<a id="generated-code-global-qualifies-root-namespace-references"></a>Moved to [Generated code global::-qualifies root-namespace references](golib-namespace.md#generated-code-global-qualifies-root-namespace-references).

<a id="a-goimplement-records-adapter-key-is-canonical-not-textual"></a>Moved to [A GoImplement record's adapter key is canonical, not textual](interfaces/records.md#a-goimplement-records-adapter-key-is-canonical-not-textual).

<a id="a-test-projects-references-cover-unrooted-alias-targets-single--and-multi-segment"></a>Moved to [A test project's references cover UNROOTED alias targets (single- AND multi-segment)](test-conversion.md#a-test-projects-references-cover-unrooted-alias-targets-single--and-multi-segment).

<a id="generic-embedded-fields"></a>Moved to [Generic embedded fields](struct-embedding.md#generic-embedded-fields).

<a id="a-func-literal-in-an-any-slot-states-its-go-result-type-explicitly"></a>Moved to [A func literal in an `any` slot states its Go result type explicitly](functions-and-closures.md#a-func-literal-in-an-any-slot-states-its-go-result-type-explicitly).

## Lifted function-local types: anonymous structs dedupe, named types carry [GoLocalName]
C# forbids type declarations in method bodies, so the converter lifts function-local types to
package scope under a function-prefixed name. Two Go type-identity rules ride the lift:

- **Structurally identical anonymous struct types are ONE Go type.** Repeated textual
  occurrences (`new(struct{ A Struct })` four times in encoding/binary's TestSizeStructCache)
  must lift to a SINGLE C# type — per-occurrence lifts split `reflect.Type` identity per
  occurrence, so binary's `structSize` cache gained four entries where Go adds one. Lifted
  anonymous structs dedupe by structural signature within a scope — a function, and, since
  2026-08-18, PACKAGE level within a file: two package vars over one written anonymous struct
  (internal/reflectlite's `assignableTests`/`implementsTests`, reflect's own
  `funcLookupCache`/`structLookupCache`) are one Go type, and splitting them made the C# types
  un-unifiable where Go unifies freely — `append(assignableTests, implementsTests...)` could not
  type (CS9244 + CS8130 on the range deconstruction). The scope discriminator is explicit
  (function name, or "" at package level) so the scope-keyed MAP never dedupes across scopes, and
  NAMED declarations keep per-declaration identity and never dedupe. Cross-scope unification is
  instead the ADOPTION path's job: a lift **adopts a PACKAGE-LEVEL lift of the same anonymous
  type** rather than minting a second one (so a function-local literal of a package-lifted
  anonymous struct reuses the package's type — Go's anonymous-struct identity is scopeless, and
  assigning the local to a package var is legal Go needing one C# type — the coordinator ruling
  at the local-iface-cast × escape-box-copy merge): `encoding/xml`'s
  `read_test.go` declares `type Child struct{ G struct{ I int } }` — lifted `Child_G` — and then
  writes the same anonymous type as a composite literal inside a function, and Go assigns one to the
  other (CS1503 ×6 while they were two C# structs). The package-level registry decides it, keyed by
  the full `types.String()` **including field tags**, which is exactly what Go's struct identity
  compares; reuse is one-directional, so no package-level lift is ever renamed. The residuals:
  cross-FILE splits (both mechanisms are file-ordered — the registry needs the package-level
  declaration already visited, which declaration order guarantees within one file and nothing
  guarantees across files), and the adoption path's ordering generally. (Guarded by
  `TestPackageLevelAnonStructDedup`; corpus footprint of the package-level extension measured at
  exactly one site, reflect's lookup-cache pair, by seeded whole-stdlib reconvert.)
- **A lifted local NAMED type carries its original Go name** via the golib `[GoLocalName]`
  attribute — a SEPARATE attribute, never a `[GoType]` definition token (the TypeGenerator
  matches that slot by exact string and throws on unknown forms). The reflection bridge's
  naming (`GoReflect.GoQualifiedName` → `Type.String()`, `%T`) prefers it, so a local type
  prints Go's `*binary.Person`, never the lifted `*binary.TestNoFixedSize_Person`
  (TestNoFixedSize asserts the exact error text). Being read off the runtime `Type` is also what
  makes the stamp movable, so it is written on the `package_info.cs` accessibility record and the
  lifted declaration reads as the plain lift it is
  ([Extended attributes](source-generators.md#extended-attributes-what-stays-on-the-declaration-and-what-moves)):

```go
func TestNoFixedSize(t *testing.T) {
	type Person struct { … }
```
```csharp
[GoType("dyn")] partial struct TestNoFixedSize_Person {

// package_info.cs
[GoLocalName("Person")] public partial struct TestNoFixedSize_Person {}
```
Guarded by the `LiftedLocalTypes` behavioral test (single lifted declaration for repeated
anonymous occurrences + `[GoLocalName]` pinned in the golden); operationally by
encoding/binary's banked suite.

## A function-LOCAL named type declaration hoists to member level (slice/map/channel/array/pointer)

C# forbids a type declaration inside a method body, so a `type X []T` / `type X map[K]V` /
`type X chan T` / `type X [N]T` declared **inside a function** cannot emit its `[GoType(…)] partial
struct X;` forward declaration in place — the following statements would then parse as MEMBER
declarations (`CS1519 Invalid token 'foreach' in a member declaration`, `CS1513 } expected`, the
map form's `CS8124`). A local `type X struct{…}` already hoists: `visitStructType`/`visitIdent`/
`visitInterfaceType` each redirect the declaration into `currentFuncPrefix` (emitted at member level
ahead of the method), rename it with the enclosing-function prefix (`ExampleChunk_People`), and
register the lifted name in `liftedTypeMap` so every reference resolves to it. The array/slice, map,
and channel emitters did **not** — they wrote the forward declaration straight into the method body
(the reported slices `example_test`/maps `maps_test` defect). The shared helper `liftLocalTypeDecl`
(`visitTypeSpec.go`) now applies that same hoist to all three: at package scope it is a no-op
(target stays `v.targetFile`, `finish()` does nothing, so production emission is byte-identical),
and inside a function it prefixes the name, registers the lift, redirects to a member-level builder,
and flushes into `currentFuncPrefix`. A local **slice/array of a local element type** also needs the
element resolved to its lifted name: `visitArrayType`'s simple-identifier fast path (which keeps the
written name so `[3]rune` stays `rune`) is skipped when the element is itself a lifted local type
(`!v.liftedTypeExists`), routing it through `getFullyQualifiedTypeName`, which resolves `liftedTypeMap` — so
`type People []Person` (Person a local struct) emits `[GoType("[]ExampleChunk_Person")] partial
struct ExampleChunk_People;`, not the raw `[]Person`. (Guarded by the `LocalNamedTypeDecls`
behavioral test — a function-local named slice-of-local-struct, map, channel, and fixed-size array,
each constructed/ranged/indexed in the body and output-compared vs Go; the unfixed converter leaks
four `partial struct …;` declarations into the method body.)

Two completions of the same rule, both demonstrated by `encoding/gob`'s test suite:

* **The POINTER kind hoists too.** `type X *T` was the one forward-declaration kind still writing
  its `[GoType("ж<…>")] partial class X;` straight into the body — gob's `codec_test.go`
  `type Rec ***Rec` produced `CS1525 Invalid expression term 'partial'` and took the rest of the
  function with it. It now takes `liftLocalTypeDecl` like the other kinds, and the lift is taken
  **before** `convStarExpr` renders the pointer text so a self-referential declaration resolves its
  own name through `liftedTypeMap`.
* **A SELF-REFERENTIAL local type re-resolves its element after the hoist.** The array/map/channel
  emitters resolved the element/key/value name *before* the declaration's own hoist registered its
  lifted name, so `type recursiveSlice []recursiveSlice` / `type recursiveMap
  map[string]recursiveMap` (gob's `encoder_test.go`) emitted `[GoType("[]recursiveSlice")]` on a
  member-level `TestRecursiveSliceType_recursiveSlice` — a name that no longer exists, `CS0246`
  inside the generated slice/map partial. Each emitter now re-resolves its element through
  `liftedTypeMap` **when the hoist actually renamed the declaration**; a package-level declaration
  never renames, so its emission is untouched (verified byte-identical across the whole behavioral
  corpus and the 302-package stdlib).

**Known residual:** a *conversion expression* to a hoisted local named **pointer** type
(`NodePtr(&Node{V: 9})`, with `type NodePtr *Node` declared in the function) still emits the
pre-hoist source name (`new NodePtr(…)`, `CS0246`). The composite kinds do not have this — a local
`Tally(m)` correctly renders `((main_Tally)m)` — so the gap is specific to the named-pointer
conversion arm's target-name resolution. It was previously masked by the hard syntax error above and
has no consumer among the measured packages (gob only *declares* `Rec` and takes its address); the
`LocalNamedTypeDecls` guard therefore uses the assignment form `var np NodePtr = &Node{V: 9}`.

## A lift inside a PACKAGE-LEVEL func literal flushes at package scope, seeded by the declaration
A func literal's body is function scope, and `convFuncLit` sets `inFunction` for it accordingly —
but that flag does **not** say there is an enclosing function DECLARATION. `currentFuncName` and
`currentFuncPrefix` (the lift's name prefix and its declaration sink) are allocated together by
`visitFuncDecl`, so for a literal in a package-level initializer they held whatever the *previous*
function declaration in the file left behind. Every lift site keys on `lifted && inFunction` and
then writes into that prefix, so a type lifted there was named after an unrelated function and
written into a buffer already flushed:

```go
var readers = []struct {
	name string
	f    func(string) io.Reader
}{
	{"ReaderOnly", func(s string) io.Reader {
		return struct{ io.Reader }{strings.NewReader(s)}   // fmt/scan_test.go
	}},
}
```

The declaration **vanished**, leaving only its use site — `new Scan_type(…)` named after the
preceding `Scan…` function, with no such type declared anywhere: **CS1729** (no one-argument
constructor), plus **CS0103**/**CS0034** in the `ImplementGenerator` wrapper generated for the
phantom type from its `[assembly: GoImplement]` record. With **no** preceding function declaration
the buffer was nil rather than stale and the converter **panicked** (nil receiver inside
`strings.Builder.copyCheck`); that panic is recovered per file, so the whole FILE was skipped with
only a `visit file error` warning. One root — which symptom appeared depended solely on
declaration order within the file.

A package-level literal now gets its **own** sink, flushed at package scope, and takes its name
seed from the declaration being initialized (`packageInitLiftName`, set by `visitValueSpec`):

```csharp
[GoType("dyn")] partial struct readersᴛ1 { … }   // the OUTER anonymous struct (unchanged)

[GoType("dyn")] partial struct readers_type {    // the lift from inside the func literal
    public io_package.Reader Reader;
}
```

Package scope is where a lifted type belongs anyway — it is exactly where the sibling
package-level lift (`readersᴛ1`) already goes — and the flush lands before the var's own field
because a package-level initializer is converted to a string first and written afterwards.
Seeding from the declaration is what keeps the name unique per var, as `readersᴛ1` already is.
(Guarded by the `PackageVarFuncLitTypeLift` behavioral test, whose two files cover BOTH symptoms:
`main.go` places a function declaration before the var — the dropped-declaration form — and
`varfirst.go` declares the var first — the panic form.)

<a id="a-methodless-named-func-type-renders-as-its-base-delegate"></a>Moved to [A methodless named func type renders as its base delegate](functions-and-closures.md#a-methodless-named-func-type-renders-as-its-base-delegate).

<a id="named-delegate-types-wrap-mismatched-initializers"></a>Moved to [Named delegate types wrap mismatched initializers](functions-and-closures.md#named-delegate-types-wrap-mismatched-initializers).

<a id="a-named-delegate-value-passed-to-a-structural-func-parameter-re-wraps"></a>Moved to [A named delegate value passed to a structural func parameter re-wraps](functions-and-closures.md#a-named-delegate-value-passed-to-a-structural-func-parameter-re-wraps).

<a id="func-typed-fields-with-a-cross-package-slash-path-type-render-structurally"></a>Moved to [Func-typed fields with a cross-package (slash-path) type render structurally](functions-and-closures.md#func-typed-fields-with-a-cross-package-slash-path-type-render-structurally).

<a id="a-variadic-func-type-lowers-to-the-golib-actionꓸꓸꓸfuncꓸꓸꓸ-delegates"></a>Moved to [A variadic func type lowers to the golib `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` delegates](functions-and-closures.md#a-variadic-func-type-lowers-to-the-golib-actionꓸꓸꓸfuncꓸꓸꓸ-delegates).

<a id="reflectvaluecall-over-a-variadic-func-value-is-typed-dispatch--no-reflective-invoke-can-carry-the-tail"></a>Moved to [`reflect.Value.Call` over a variadic func value is TYPED dispatch — no reflective invoke can carry the tail](reflection/values.md#reflectvaluecall-over-a-variadic-func-value-is-typed-dispatch--no-reflective-invoke-can-carry-the-tail).

<a id="reflectmakefunc-is-valuecalls-exact-inverse--a-compiled-delegate-over-the-descriptors-carried-systemtype-2026-08-29"></a>Moved to [`reflect.MakeFunc` is `Value.Call`'s exact inverse — a compiled delegate over the descriptor's carried System.Type (2026-08-29)](reflection/values.md#reflectmakefunc-is-valuecalls-exact-inverse--a-compiled-delegate-over-the-descriptors-carried-systemtype-2026-08-29).

<a id="major-version-import-directories"></a>Moved to [Major-version import directories](package-conversion/imports-and-build-constraints.md#major-version-import-directories).

<a id="a-c-keyword-inside-a-dotted-import-path-element"></a>Moved to [A C# keyword inside a dotted import-path element](package-conversion/imports-and-build-constraints.md#a-c-keyword-inside-a-dotted-import-path-element).

<a id="the-import-path-rewrite-rewrites-only-the-path-not-the-constructor-in-front-of-it"></a>Moved to [The import-path rewrite rewrites only the PATH, not the constructor in front of it](package-conversion/imports-and-build-constraints.md#the-import-path-rewrite-rewrites-only-the-path-not-the-constructor-in-front-of-it).

<a id="a-non-canonically-aliased-import-renders-foreign-types-via-the-files-alias"></a>Moved to [A non-canonically-aliased import renders foreign types via the file's alias](package-conversion/imports-and-build-constraints.md#a-non-canonically-aliased-import-renders-foreign-types-via-the-files-alias).

## A ONE-FIELD struct's positional `nil` literal names its field constructor
The universe `nil` renders in a value context as the typeless `default!`, which takes its type from
whatever it is assigned or returned into. A constructor ARGUMENT is the one position where nothing
supplies that type, and a generated struct partial offers exactly two one-argument constructors: the
nil constructor `T(NilType)` and the field constructor `T(F field = default!)`. `default!` converts
to both, so a one-field struct's positional literal carrying `nil` is `CS0121 — the call is
ambiguous`:
```csharp
new TestWriter_testClose(default!)          // ambiguous: T(NilType) vs T(error)
new TestWriter_testClose((error)default!)   // names the field constructor, and only it
```
The argument now carries the field's type, via the same per-element `castArgToType` plumbing the
narrow-integer and `any`-field element casts use. **Only** a one-field struct can reach this: Go
requires a positional composite literal to list every field in order, so at any other arity the call
already differs from `T(NilType)` in argument count — and only `nil` can, because every other element
renders with a type of its own. A POINTER field is excluded and deliberately unchanged: there the
literal renders golib's `nil`, whose type `NilType` is an *exact* match for `T(NilType)` and so beats
the field constructor's user-defined conversion without ambiguity, producing the zero struct, which
is the correct value. `archive/tar`'s `testClose{nil}` is the reported shape (×9, and the last wall
in front of that package's 97 verdicts); `database/sql`'s `stubDriverStmt{nil}` is the same root.

## Func-field callees drive argument treatment

- **Func-field callees drive argument treatment**: `getFunctionSignature` resolves a
  FUNC-typed field's signature (`d.fill(d, b)` — the receiver arg renders as the box for a
  `ж<T>` slot, CS1503).

## Defined-over-named-struct composites wrap the underlying

- **Defined-over-named-struct composites wrap the underlying** (`decoder{order: o}` →
  `new decoder(new coder(order: o))`, CS1739). Guarded by `NamedPointerReinterpret` (`view{}`).

## Defined types over a struct — forwarded fields

**Defined types over a struct — forwarded fields.** A Go type definition over a *struct* — `type winlibcall libcall` — makes the underlying struct's fields accessible on the named type (`w.fn`), without promoting its methods. The named type is emitted as `[GoType("libcall")] partial struct winlibcall;` and the `TypeGenerator` wraps the underlying value (`private libcall m_value;`). For the underlying's fields to be reachable, the generator **forwards each as a ref-returning property** over `m_value`:
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

## The wrapper also forwards the underlying's field-box accessors

The wrapper also forwards the underlying's **field-box accessors**. Taking the address of a wrapper's field — `&p.x` on a `*pinnerBits`, where `type pinnerBits gcBits` (runtime `pinner.go`) — emits the box-accessor form `Δp.of(pinnerBits.Ꮡx)`, whose owning type is the **wrapper**; without a forwarded accessor the static exists only on `gcBits` (CS0117). For every forwarded *field* (properties cannot be `ref`'d and get none, matching the plain-struct template) the generator emits the accessor as a **true ref through `m_value`** into the underlying struct's field: `public static ref uint8 Ꮡx(ref pinnerBits instance) => ref instance.m_value.x;` — a genuine ref chain into the wrapper's own storage, so a write through the resulting box persists (a copy here would silently drop writes — the trap that sank an earlier `pallocBits` forwarding attempt). Emitted only when members are forwarded, which is exactly when `m_value` is mutable. (Guarded by the `NamedTypeOverStruct` extension — `bump(&c.a)` writes through the wrapper's field address and the original observes it; cleared runtime `pinner.go`'s 3 CS0117, 89 → 86.)

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

## Generic struct equality is decided per FIELD, not per type parameter
A generic `[GoType]` struct's synthesized `Equals` (see [Struct Types](#struct-types)) was gated on
the struct's TYPE PARAMETERS: unless every parameter carried an `IEqualityOperators`-implementing
constraint (and, stricter still, every *constraint* of every parameter implemented it), the whole
struct's `Equals` body was the constant **`false /* missing equality constraints */`**. Since a
`comparable` parameter deliberately emits no C# constraint (previous subsection),
essentially every generic struct in the corpus — all 22 generic `[GoType]` declarations at the time
of the fix — carried a constant-false `Equals`, breaking equality that never depended on the
parameter at all. `unique.Handle[T]`'s only field is `*T` (`ж<T>`), whose pointer-identity `==` is
valid for **every** T, yet no two handles ever compared equal — directly contradicting the type's
documented contract ("two handles compare equal exactly if the values used to create them would");
`internal/weak.Pointer[T]`'s only field does not mention T at all. Even
`internal/trace.dataTable[EI, E]` — whose `EI` explicitly lists `IEqualityOperators` — failed the
every-constraint quantifier because `IAdditionOperators` and its siblings do not *themselves*
implement the equality interface.

The gate now decides **per member** (`GetEqualityFallbackMembers` in
`StructDeclarationSyntaxExtensions`): a member whose type supports `==` independent of the
unconstrained parameters keeps the same `this.f == other.f` compare a non-generic struct emits, and
only a member whose type IS an unconstrained type parameter falls back to golib's
**`AreEqual`** — the identical routing the converter emits for Go `==` on any type-parameter
operand, giving `EqualityComparer<T>.Default` speed on value types while preserving IEEE float
semantics (raw `EqualityComparer` reports NaN equal to itself, inverting Go — see the
floating-point note above) and typed-null/runtime-type semantics for reference and interface
instantiations. Real emissions (from the converted stdlib's generated sources):

```csharp
// unique.Handle<T> — ж<T> has pointer-identity == for every T:
public bool Equals(Handle<T> other) =>
    this.value == other.value;

// database/sql.Null<T> — mixed: the T field routes through AreEqual, the rest keep ==:
public bool Equals(Null<T> other) =>
    global::go.builtin.AreEqual(this.V, other.V) &&
    this.Valid == other.Valid;

// net/http.mapping<K, V> — golib slice/map fields carry their own ==, so no member falls back:
public bool Equals(mapping<K, V> other) =>
    this.s == other.s &&
    this.m == other.m;
```

The member classifier asks only "does `==` COMPILE for this member type": a type parameter
qualifies through ANY `IEqualityOperators`-implementing constraint (matching C# operator
resolution, not the whole-struct gate's every-constraint test); reference types (classes incl.
`ж<T>` and `unsafe.Pointer`, interfaces, arrays, delegates), enums, pointers, and built-in value
types always qualify; a `[GoType]` struct qualifies by its **attribute** — both struct templates
emit a same-type `operator ==` unconditionally, and for a struct of the *same compilation* the
attribute is the only visible evidence, because that operator does not exist yet while the
generator runs; any other value type qualifies only by actually declaring a same-type
`op_Equality`. Structs that passed the old whole-struct gate, and every non-generic struct, emit
byte-identical bodies to before — the fallback set is computed only when the gate fails.
`GetHashCode` needs no matching change (`golib.HashCode.Combine` always compiled and hashes
consistently with both compare forms). (Guarded by the `GenericStructEquality` behavioral test —
the `Handle` pointer-identity shape, the plain-T fallback shape, a T-independent-field struct, the
`Null`-shaped mix, a nested generic struct field, and a generic struct as a map key, all
output-compared vs Go.)

---

[← Type Switch Statements](type-switch.md) · [Index](README.md) · [Struct Type Embedding →](struct-embedding.md)
