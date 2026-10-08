# Struct Types

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#struct-types)
Go structs are converted to C# `struct` types and used on the stack to optimize memory use and reduce GC pressure; when an instance must escape the stack it is wrapped in a heap box, [`ж<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) (see [Pointers](pointers.md#pointers)). Rather than spell out the whole struct body, the converter emits a partial struct carrying a `[GoType]` attribute, and the `TypeGenerator` source generator synthesizes the members (equality, `ISupportMake`, embedding promotion, etc.):

<!-- source: src/tests/Behavioral/AnonymousStructs/AnonymousStructs.cs.target:7-10 -->
```csharp
partial struct Person {
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

## A global addressed only by the package's own `_test.go` is still heap-boxed
A Go pointer to a package-level var aliases that var's real storage, which in C# means the global
must be backed by a heap box (see [Pointers](pointers.md#pointers)); `packageAddressedGlobals` decides that by
scanning the package for `&g`. But `go/packages` excludes `_test.go` from a production package, so
an address taken *only* by the package's own in-package test half is invisible at the declaration.
path/filepath is the canonical case — `path.go` declares `var lstat = os.Lstat // for testing` and
`export_test.go` declares `var LstatP = &lstat`, the whole point being that a test can swap the
implementation the production `Walk` calls. The production emission left `lstat` a plain field, and
the test variant's `Ꮡlstat` named a box nothing declared: **CS0103**.

The converter now scans the build-selected in-package `_test.go` files for the identifiers they take
the address of and folds them into the addressed-global set, so the production declaration carries
the box:

```csharp
internal static ж<Func<@string, (fs.FileInfo, error)>> Ꮡlstat = new(os.Lstat);
internal static ref Func<@string, (fs.FileInfo, error)> lstat => ref Ꮡlstat.ValueSlot;  // for testing
```

Three properties make this the right shape rather than a `-tests`-only patch:

- **It runs in ordinary conversion too**, exactly as `siblingTestFuncMethodNames` does for reference
  spelling, so a package's production storage shape is **mode-stable** — an `-stdlib` reconvert and a
  `-tests` run emit the same bytes. Conditioning it on `-tests` would make the banked corpus flip
  between the two.
- **The scan is a cheap direct directory read, not a second type-check** — no test dependency graph is
  loaded. It is therefore name-based, and the production pass resolves each candidate against the real
  package scope, dropping anything that is not a package-level var (a type, a func, an import
  qualifier, a name that exists only in the test file).
- **It errs toward recording nothing.** Names bound anywhere inside the enclosing top-level
  declaration — receiver, parameters, results, `:=`, `var`/`const`/`type`, range and type-switch
  bindings — are excluded, so `&counter` on a local that shadows a global does not box the global.
  Under-recording restores today's loud CS0103; over-recording would silently box a global no pointer
  aliases.

Only **build-selected** test files are scanned (`go/build`'s `MatchFile`, with the run's `GOOS`/
`GOARCH` and `-tags`), so the boxed set is a property of the build configuration exactly as the
converted production sources themselves are: `path_windows_test.go` contributes on Windows and
`path_unix_test.go` does not. That is the same rule `siblingTestFuncMethodNames` already follows, and
it is the correct answer — a global no *selected* file addresses needs no box in that configuration.

Measured across the whole standard library by an A/B reconvert: **13 globals in 13 files**, and every
single one is a Go *"for testing"* hook — `path/filepath` and `os`'s `lstat`, `os`'s
`testingForceReadDirLstat` and `allowReadDirFileID`, `runtime`'s `readRandomFailed`, `useAeshash`,
`doubleCheckReadMemStats`, `casgstatusAlwaysTrack`, `forcegcperiod` and `timeBeginPeriodRetValue`,
`reflect`'s `callGC` (whose own comment reads *"for testing; see TestCallMethodJump and
TestCallArgLive"*), `internal/poll`'s `logInitFD`, `net/http`'s `maxWriteWaitBeforeConnReuse` and
`testHookEnterRoundTrip`, and `time`'s `usPacific`. No false positives, which is what the
bind-aware exclusion buys — and the same set is forward work, since `os`, `runtime`, `reflect`,
`net/http`, `internal/poll` and `time` all need those hooks to alias real storage before their own
suites can pass.

External (`package foo_test`) test files are deliberately not scanned: they reach the package only
through its exported surface, and `&otherpkg.Var` from *any* other package is a separate, still-open
gap — `collectAddressedGlobals` only ever scans the package under conversion. (Guarded by the
`SiblingTestAddressedGlobal` behavioral test, whose `export_test.go` addresses a bare global, a
global through a field selector, and a global from a function body, against negatives for a
test-file-local declarator and a shadowing local. It is the first behavioral project to carry a
`_test.go`; the corpus harness skips `_test.go` when pairing sources with `.cs` goldens, since a
production transpile never emits one.)

## Astral rune literals
A quoted rune literal beyond the BMP (`'\U0001D504'`) cannot be a C# char literal — it emits
the code point (`(rune)0x1D504`); BMP literals keep their source text verbatim (html's entity
table, CS1012 ×133). Guarded by `StringConvPostfix` (`glyphs`).

## Type-switch default arm binds the interface value
The default clause binds the guard to the ORIGINAL guarded expression (`var x = err;`), whose
static type is the interface — the switch-operand form (`err.type()`) is object and cannot
flow back out (`default: return x`, go/build/constraint's pushNot, CS0266).

## The type-switch tag evaluates exactly once
Go evaluates the TypeSwitchGuard's operand exactly once, but the default-arm and multi-type
re-binds above textually re-emit the tag expression, so a tag containing a **call or channel
receive** evaluated once at dispatch and again at each matched re-bind arm —
`switch p := recover().(type)` re-called `recover()` (which returns nil the second time,
silently losing the recovered value in a `case nil, *bailout:`-style arm that reads `p`;
go/types handleBailout), and a `switch v := (<-ch).(type)` re-received. Such a tag is now
HOISTED into a one-time temporary, and both the dispatch operand and every re-bind read it:

```csharp
var switchᴛ1 = next(x);
switch (switchᴛ1.type()) {
case @string _:
case bool _: {
    var v = switchᴛ1;      // re-bind reads the temp — next() ran exactly once
    …
default: {
    var v = switchᴛ1;
```

The hoist is deliberately GATED — only a tag containing a call (conversions hoist
conservatively; the temp is merely unneeded) or a receive, and only when some arm actually
re-binds (a bound default, or a multi-type clause with a non-blank ident) — so every pure-tag
type switch keeps its direct, byte-identical emission. The temp name comes from the
per-package `getGlobalTempVarName` counter (`switchᴛN`), so nested and sibling hoists never
collide. Single-type concrete labels and the `when`-guard interface labels bind from the
dispatch operand's pattern variable and never re-evaluate the tag regardless. (Guarded by the
`TypeSwitchImpureTag` behavioral test — a counting-function tag whose per-switch eval count is
printed and output-compared vs Go [the pre-fix emission provably prints `calls: 7` for Go's
`calls: 4`], a `recover()` tag in a deferred multi-type switch, and a channel-receive tag that
would deadlock on re-receive.)

## Generated code global::-qualifies root-namespace references
Inside a package whose namespace nests a same-named segment (go/build/constraint emits into
`namespace go.go.build`), C# binds a generated reference's leading `go` RELATIVELY to `go.go`
(CS0234). The generators qualify every type-reference position via `GlobalQualify`
(Common.cs); generated signatures also carry parameter REF KINDS (`in slice<byte>`) and a
canned `System.IFormattable` impl where the interface inherits it (the hand-finished io
stub's dyn machinery).

The **converter** faces the same `go.go` shadowing in the import `using` directives it emits for a
`go/*` package (go/token lands in `namespace go.go`, imports `sync`/`unicode` sub-namespaces): a
rooted `using atomic = go.sync.atomic_package;` / `using go.sync;` binds its leading `go` to the
enclosing `go.go` namespace, resolving `go.sync` to the nonexistent `go.go.sync` (CS0234). `rootQualifyIfAmbiguous` routes its rooting returns through `rootQualified`, which emits `global::go.`
instead of a bare `go.` when the package's namespace second segment is itself `go`:

```csharp
using atomic = global::go.sync.atomic_package;
using global::go.sync;
```

The shadowing is NOT limited to `go/*` packages themselves: any package with a `go/*` package
anywhere in its transitive import CLOSURE compiles with `namespace go.go` in scope (its referenced
assembly makes `go.go` a member of namespace `go`), and C#'s inner-to-outer lookup then binds the
bare leading `go` of a rooted using target to that member from EVERY namespace nested under the
root — internal/fuzz (imports go/ast) emitted `using bits = go.math.bits_package;` inside
`namespace go.@internal`, resolving to the nonexistent `go.go.math` (CS0234 ×16, plus the same
shape in net/rpc's `Δhttp` alias and testing/internal/testdeps). `rootQualified` therefore also
emits `global::go.` when `packageChildNamespaces` carries the `go.go` key (populated from the
transitive import closure by `computeImportAliasRenames`' pre-pass). A package with no `go/*`
anywhere in its closure — every package that was compiling before, and all pre-existing behavioral
tests — keeps the bare `go.` prefix, so there is no golden churn. Cleared go/token, go/doc/comment,
go/build/constraint (own-namespace branch); internal/fuzz's 18 CS0234 and net/rpc's latent pair
(closure branch). Guarded by the `GoNamespaceShadow` behavioral test, which covers BOTH branches
through a nested local module literally named `go/nsshadow` (emitting `namespace go.go`, the shape
a single-file behavioral test cannot express): the nested lib imports `math` + `math/rand` so its
own rooted using exercises the own-namespace branch, and the importing `main` package (namespace
`go`, with `go.go` in its closure) exercises the closure branch.

**Under `-tests` the shadow gate spans BOTH compilation halves, and the directly-composed using
targets must go through it too.** The gate had two holes that only a test conversion can expose,
and math/rand/v2 (whose `regress_test.go` imports `go/format`) hit both — 13 of the package's 22
compile errors:

1. *The closure was computed per PACKAGE, not per ASSEMBLY.* A `-tests` run recompiles the
   package's PRODUCTION sources into the test assembly, so that assembly's reference closure is the
   UNION of the production and `_test.go` closures. The production conversion pass saw only its own
   half, never learned `go.go` was in scope, and emitted bare `using bits = go.math.bits_package;`
   into a compilation that did contain `go.go`. `collectSiblingTestClosure` now runs a
   metadata-only (`NeedName|NeedImports|NeedDeps`) load of the test variants before the production
   conversion and records their transitive import paths in `siblingClosureImportPaths`, which
   `computeImportAliasRenames` folds into the closure it walks — so every consumer of the namespace
   maps (the shadow gate, `rootQualifyIfAmbiguous`, `isStrippedGoPathPackageRef`) describes the
   assembly rather than the package. The set is empty for every non-`-tests` conversion, so no
   other output moves.
2. *Targets composed straight from `packageNamespace` bypassed `rootQualified` entirely.* Both the
   package-under-test anchor (`visitImportSpec`'s `isPackageUnderTest` branch, which REPLACES the
   `rootQualifyIfAmbiguous`-derived target with `<packageNamespace>.<pkg>_package`) and the test
   host's `using go.testing_runtime;` were bare, which is why one emitted file could show a
   correctly-qualified `using iotest = global::go.testing.iotest_package;` beside a broken
   `using static go.math.rand.rand_package;`. `globalQualifyRooted` applies the same gate to an
   ALREADY-rooted path and both sites now route through it. It is idempotent and a no-op with no
   shadow, so unshadowed packages emit byte-identically.

Both holes fire for ANY package whose test closure reaches a `go/*` package, and a `regress_test.go`
importing `go/format` is a common stdlib idiom — this is not a v2 quirk. Guarded by
`TestGlobalQualifyRootedForcesGlobalUnderRootShadow` and `TestSiblingClosureContributesRootShadow`
(`src/go2cs/rootShadowQualification_test.go`); the behavioral corpus cannot cover them because it
never runs `-tests` and no behavioral package imports a `go/*` package.

## A GoImplement record's adapter key is canonical, not textual
`interfaceImplementations` is keyed by RENDERED type name, so one resolved pair recorded under two
spellings is two records — and go2cs-gen turns two records into two definitions of the SAME adapter
type. The interface side arrives class-relative when PARSED from a `package_info.cs`
(`rand_package.Source`, via `loadPackageImplements`) and fully namespace-qualified when rendered at
a CAST SITE (`go.math.rand.rand_package.Source`). `canonicalRecordIfaceName` stripped only the root
prefix, so the two keyed differently, the foreign-adapter existence proof missed, and the pair was
re-recorded under the second spelling.

Under `-tests` this is routine rather than exotic: the EXTERNAL (`package <name>_test`) variant
reaches the package under test through its import path, so it renders that package's types
qualified, while the seeded production metadata carries them short. math/rand/v2 emitted both
`[assembly: GoImplement<PCG, Source>(Pointer = true)]` and
`[assembly: GoImplement<go.math.rand.rand_package.PCG, go.math.rand.rand_package.Source>(Pointer = true)]`,
and `ImplementGenerator`'s `GetUniqueHintName` silently uniquified the duplicate FILE name — so the
duplicate TYPE reached the compiler as CS0102 + CS0111 ×5 + CS8646 on `rand_package.PCGжSource`.
(`math/rand` escapes only by luck: its one self-qualified record targets a different interface than
any short record.)

The adapter's identity is exactly `<class>_package.<Type>` — the pair `ImplementGenerator` composes
its class name from — so the record key collapses a longer chain to its `<pkg>_package` tail, leaving
a nested type reference (`x.y_package.Outer.Inner`) untouched. The EMISSION side is normalized to
match: `stripLocalTypeQualifier` rewrites a reference naming one of THIS package's own types through
the package's fully-qualified class back to the bare local form the attribute file's
`using static <ns>.<pkg>_package;` resolves, so the two spellings collapse in the emitting
`HashSet`. Guarded by `TestStripLocalTypeQualifier`.

*(Superseded in the details, 2026-08-02: `canonicalRecordIfaceName` is retired. Both record sets now
compose one key through `implementRecordKey` / `canonicalImplementRecordIfaceName` — same collapse
rule, now shared rather than duplicated. See* A foreign implement record is keyed in ONE spelling,
and a VALUE one is trusted only for a partial struct.*)*

⚠ **The collapse only reaches records the CURRENT run rendered — a stale spelling already on disk
slips past it, because `package_info_external_test.cs` / `package_test_info.cs` are MERGE-PRESERVING** (see
the anchor-routing note above). The merge reads each existing attribute line VERBATIM into the
emitting `HashSet`, so a record persisted by an OLDER converter — before `stripLocalTypeQualifier`
reduced it — arrives under the pre-collapse spelling and never meets the fresh, already-collapsed
one. `container/heap` (banked at package #8, before the collapse landed) committed
`[assembly: GoImplement<IntHeap, go.container.heap_package.Interface>(Pointer = true)]`; a fresh
`-tests` run of a NESTED package-under-test now renders that same pair as the bare
`[assembly: GoImplement<IntHeap, Interface>(Pointer = true)]` (the qualified `go.container.heap_package.`
prefix gets `rootQualifySubNamespaceTypeRefs`-rooted then stripped, whereas a TOP-LEVEL package's
`sort_package.Interface` is never rooted so it is never stripped and stays byte-stable). The two
spellings both survived the merge → `GetUniqueHintName` uniquified the second `.g.cs` → a duplicate
`IntHeapжInterface` reached the compiler (CS0102 + CS0111 + CS8646). `writePackageInfoFile` now runs
every merged-in `[assembly: GoImplement<…>]` line through the SAME `qualifyLocalTypeRef` pipeline the
fresh render applies, so a stale record collapses into the canonical one instead of duplicating it —
the whole-line pass is safe because the pipeline only rewrites package-qualified name tokens (bare
flag keywords `Pointer`/`Promoted` and the `assembly`/`GoImplement` scaffolding are untouched), and it
is scoped to `GoImplement` lines specifically so it cannot rewrite a `GoImplicitConv` attribute's
`ValueType = "…"` keyword (`ValueType` is a System-colliding name the rooter would otherwise qualify).
Because whole-package conversions (`-stdlib`, every behavioral test) write with `mergeExisting=false`
they never take this path, so the corpus and behavioral goldens are byte-identical. Guarded by
`TestMergedStaleGoImplementSpellingCollapses`.

**A type ALIAS is the third spelling, and it is resolved at the SOURCE.** The two collapses above
reconcile spellings of one type *after* they are rendered. An alias cannot be reconciled that way:
`type Expr = ast.Expr` is a name for a type that already has a name, and go2cs-gen composes the
adapter class from the *resolved symbol*, never from the record's text — so a cast site that
composes the class name from the alias spelling names a class the generator never emits (CS0246),
and the pair is additionally recorded twice, once per spelling. `convertToInterfaceType` therefore
resolves BOTH operands through `types.Unalias` before composing anything, which is where the
function already reached ad hoc at five later points.

The defect long predates the case that exposed it: any alias whose name differs from its target's
mismatched the same way, and a *package-level* `type E = ast.Expr` would have done it just as well.
It stayed invisible because the only aliases the corpus reached were spelled exactly like their
targets, so the composed name happened to be right. `go/types`' `rangeStmt` declares
`type Expr = ast.Expr` **function-locally**, and once function-local type declarations began taking
the enclosing-function lift (`rangeStmt_Expr`, so two functions never claim one compilation-scoped
`global using`), `check.errorf(lhs[i], …)` started composing `ast_rangeStmt_Exprᴠpositioner` against
the generator's `ast_Exprᴠpositioner` — 557 verdicts behind two lines. With the resolution in place
the aliased and unaliased cast sites in that same function land on one adapter and one record.
(Guarded by the `LocalTypeAliasScope` extension: a function-local `type S = fmt.Stringer` converted
to a local `namer` beside the same conversion written through `fmt.Stringer` directly, so a
spelling-composed name shows up as both a second `ᴠ` class and a duplicate `GoImplement` record.)

## A test project's references cover UNROOTED alias targets (single- AND multi-segment)
A `-tests` project sets `DisableTransitiveProjectReferences`, so its references are the
direct-import closure plus whatever `aliasReferenceImports` recovers by scanning the emitted `using`
aliases for namespace tokens. The scan matched only the ROOTED token (`go.hash_package`), but a
SINGLE-SEGMENT package emits its alias UNROOTED — `using hash = hash_package;` inside
`namespace go.math.rand`, where C#'s outward lookup finds the class in the enclosing root namespace
with no qualifier. math/rand/v2's `chacha8_test.cs` needs `hash` purely because `sha256.New()`
RETURNS `hash.Hash`, so the package appears in no import list and only this scan could have found
it: the reference went missing and the build failed CS0246 on `hash_package`. The scan now also
carries a bare token per single-segment package, matched on a SEGMENT boundary (`target == token` or
`target` starts with `token + "."`) — a substring test would let `hash_package` match
`go.hash.maphash_package` and pull in a package nothing references. Guarded by
`TestAliasReferenceImportsMatchesUnrootedSingleSegmentAlias` and
`TestAliasReferenceImportsDoesNotMatchAcrossSegmentBoundaries`.

A **MULTI-segment** package hits the identical gap when the test's enclosing namespace SHADOWS the
root `go`. From `namespace go.math` the alias for os/exec is emitted ROOTED — `using exec =
go.os.exec_package;` (math/rand's `default_test.cs`, caught by the `HasSuffix(target, token)` arm) —
but from a namespace whose first segment re-binds `go`, the alias is emitted UNROOTED and relies on
C# outward lookup: go/doc/comment's `std_test.cs` (in `namespace go.go.doc`) and internal/abi's
`abi_test.cs` (in `namespace go.@internal`) both emit `using exec = os.exec_package;`, again purely
because `testenv.Command(…)` RETURNS `*exec.Cmd` so os/exec appears in no import list. The rooted
token (`go.os.exec_package`) is now ALSO matched when it ends with the unrooted target after a
segment boundary — `HasSuffix(token, "." + target)`, so `os.exec_package` matches
`go.os.exec_package` while the leading `.` anchor keeps `os.exec_package` from matching an unrelated
`go.notos.exec_package`. This was the single shared root cause blocking both internal/abi and
go/doc/comment (CS0246 on the `os` namespace). Guarded by
`TestAliasReferenceImportsMatchesUnrootedMultiSegmentAlias` and
`TestAliasReferenceImportsUnrootedTailAnchoredOnSegmentBoundary`.

**An emitted CONVERSION RECORD names packages no import list and no alias mentions.** A `using` alias
is not the only line in the test metadata that must BIND: go2cs-gen realizes every
`[assembly: GoImplement<…>]` / `[assembly: GoImplicitConv<…>]` record into a generated adapter,
partial or operator, so both generic arguments have to resolve at the attribute itself. The converter
records an interface pair from a type's *use*, and that use can be entirely implicit —
os/signal's test does `cmd.Stdout = &buf`, whose os/exec field type is `io.Writer`, so
`package_test_info.cs` carries

```csharp
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
```

while `io` appears in no import list of the production package or its tests, and in no alias. Under
`DisableTransitiveProjectReferences` that is CS0246 on `io_package` at the attribute line, plus a
cascading go2cs-gen **CS8785** (`ImplementGenerator failed … second generic type argument must be an
interface`) once the unbound interface degrades to an error type — the generator then contributes
nothing and the whole package's adapters vanish. The scan therefore also reads the record lines,
extracting each type reference's **package-class qualifier** — everything up to and including the
first segment ending in `_package` (`io_package`, `go.io.fs_package`, `go.@internal.abi_package`).
That is deliberately the qualifier, not the whole type reference: it has exactly the shape a `using`
alias TARGET has, so the *same* three token-match arms above decide both, with no second matcher to
keep in sync. Only the record's generic argument list is scanned (first `<` to last `>`, so a nested
`ж<…>` argument is covered whole) — an attribute's `(Pointer = true)` / `(ValueType = "…")` payload is
metadata, and the `ValueType` is a string, not a reference. Additive as before, and the manifest's
dependency list stays import-derived. This is what lets **os/signal** validate (its `TestCtrlBreak`,
1/1 vs `go test`). Guarded by `TestAliasReferenceImportsMatchesConversionRecordPackages`,
`TestAliasReferenceImportsMatchesConversionRecordQualifierShapes` and
`TestAliasReferenceImportsIgnoresConversionRecordAttributePayload`.

**Referencing a `go/*`-package TYPE loses a root segment because the path's own `go` collides with the root namespace.** A `go/ast` type reference renders correctly as `go.go.ast_package.X` (root `go` + the path's `go.ast` → namespace `go.go`, class `ast_package`), but `convertToCSTypeName` then strips the *leading* `go.` as a redundant root (bodies live inside `namespace go`), leaving `go.ast_package.X` — namespace `go`, which has no `ast_package` (CS0234/CS0426 in the go/* consumers go/doc, go/printer, go/internal/typeparams, whose GoImplement attributes and `using` aliases both carry the stripped form). The two rooting helpers now recognise this: `isStrippedGoPathPackageRef` splits the ref at its first `_package` class segment and tests the *namespace* portion against `packageChildNamespaces` (the current package's rooted import-closure namespaces): the ref is stripped iff that namespace is NOT already a real rooted namespace but *becomes* one when the root `go.` is prepended. This is a **membership** test, not a string-shape test, so it recognises a stripped go/*-package ref at any depth — `go.ast_package` (ns `go`✗ → `go.go`✓), `go.build.constraint_package` (ns `go.build`✗ → `go.go.build`✓, three-segment `go/build/constraint`), `go.doc.comment_package` (ns `go.doc`✗ → `go.go.doc`✓) — while leaving a genuinely-rooted ref alone (`go.io.fs_package` — ns `go.io` is already real). (The earlier two-segment string heuristic — "the class segment sits immediately after `go.`" — recognised only the depth-one `go.ast_package` shape and silently missed the three-segment `go/build/constraint` and `go/doc/comment` sub-package refs, which are string-indistinguishable from a correctly-rooted `go.io.fs_package`; the membership test is what disambiguates them.) `rootQualifySubNamespaceTypeRefs` (the assembly-scope GoImplement/GoImplicitConv attributes) re-roots the stripped form to a bare `go.go.ast_package`; `rootQualifyIfAmbiguous` (the in-namespace `using` aliases) re-roots to `global::go.go.ast_package` — always `global::`, because a bare `go.go.<pkg>_package` re-binds its leading `go` to the nearest enclosing `go` from *any* importer (a go/*-package's own `go.go.*` namespace, and equally `internal/pkgbits` at `go.internal.pkgbits` resolving the second `go` inside `go.go`, CS0234). This un-blocks the whole go/* chain at the rooting level (go/doc's own-errors 17 → 1); each go/* package still needs its remaining per-package residuals (e.g. a methodless-func-type's `[GoTypeAlias]` still names an inline-rendered `ΔFilter`) to fully compile. The depth-one shape is now guarded by `GoNamespaceShadow` (its `go/nsshadow` nested module's import renders through `isStrippedGoPathPackageRef` → `using nsshadow = global::go.go.nsshadow_package;`); the multi-segment sub-package depth (`go/build/constraint`) remains census-verified only — the A/B reconvert-diff showed only the four `go/build/constraint`- and `go/doc/comment`-importing packages, go/build, go/doc, go/parser, go/printer, gaining the corrected double-`go` rooting, with the depth-one `go.go.ast_package` refs unchanged and zero collateral.

**BCL names in generator templates are global::-qualified too — a Go type can shadow any bare BCL
name.** The generated partials sit inside the package class, where every Go type in the package is
a sibling member that wins name lookup over `System.*`: internal/trace/traceviewer declares
`type Range struct`, so the named-string wrapper's sub-slice indexer `this[Range range]` bound the
Go `Range` instead of `System.Range` (CS1503 inside its own `ViewType.g.cs`). This is a *class* of
collisions, not one bug — any package declaring a type named `Range`, `Index`, `Type`, `Span`, … is
exposed — so the audit qualified every BCL reference the TypeGenerator templates emit:
`global::System.Range` (string/slice/array indexers), `global::System.Span<T>`/`ReadOnlySpan<byte>`,
the `IEnumerator`/`IEnumerable` members, `ICloneable`, `IEquatable` and the `System.Numerics`
operator interfaces on numeric wrappers, `System.Type`/`Reflection.MethodInfo`/`Activator`/
`NotImplementedException`/`[DebuggerNonUserCode]` in the dynamic-interface machinery, and the
`GeneratedCode` attribute stamped on every generated declaration (`Common.cs`, shared by all
generators). golib names (`slice<T>`, `NilType`, `IChannel`, …) stay bare — they live in the `go`
namespace the generated code owns. Converter-emitted visible code is not part of this rule (it
renders BCL names by the file-scoped conventions above). (Guarded by `BclTypeNameShadow` —
a package declaring `type Range struct` alongside a named string type and a named slice type, both
sub-sliced with the Go `Range`'s fields as bounds, output vs Go.)

## Generic embedded fields
A GENERIC embed (`entry[K,V]` embedding `node[K,V]`, internal/concurrent) arrives in the AST as
an `IndexExpr`/`IndexListExpr` over the base type; the anonymous-field walk unwraps it (plain,
pointer, and selector forms) and the member emits under the **base name** with type arguments
stripped **before** the selector dot-strip — the arguments may contain qualified types whose
dots otherwise win the LastIndex (`*concurrent.HashTrieMap[T, weak.Pointer[T]]` misnamed its
member `Pointer` instead of `HashTrieMap`). The TypeGenerator's promoted accessors carry the
type parameters on the instance param (`ref Δentry<K, V> instance`) and strip them from the
member access (`instance.node.isEntry`). A promoted method call through a raw ж **box local**
hops `X.Value` ahead of the cross-package pointer-embed hop
(`m.Value.HashTrieMap.Value.Load(value)`, unique). BANKED: unqualified promoted METHOD calls
through a generic embed (`w.show()`) — receiver wrappers resolve the embedded type by exact
name; qualified calls work. Guarded by `GenericStructFields` (`wrapped[T]`/`tag[T]`) and
`CrossPkgUser` (`holder[T]` embedding `*CrossPkgLib.Cache[T]`).

## A func literal in an `any` slot states its Go result type explicitly
A function literal converted into a real **empty-interface** parameter has no delegate target
type, so C# natural-types it from its return arms — `func(x int) int { return 0 }` inferred
`Func<nint, int>` (the literal `0` is C# `int`, i.e. Go `int32`), and the natural type becomes
the value's runtime dynamic type, which reflection then classifies: `func(int) int` and
`func(int) int32` collapsed to ONE managed type, so `quick.CheckEqual` saw equal func types
where Go's differ (testing/quick's TestFailure #3). The emission states the declared Go result
type explicitly:
```go
CheckEqual(func(x int) int { return 0 }, func(x int) int32 { return 0 }, nil)
```
```csharp
CheckEqual(nint (nint x) => 0, int (nint x) => 0, default!);
```
Scoped to single-result literals in `any` slots (`CallExprContext.emptyInterfaceArgs` →
`LambdaContext.untypedInterfaceTarget` → convFuncLit's explicit-return-type mechanism);
target-typed positions are untouched — their delegate supplies the type, and an explicit return
type there could only add identity-match constraints against hand-written stub delegate types.
Multi-result `any`-slot literals kept natural tuple typing until html/template supplied the
consumer that caveat was waiting for (see below). Guarded
by the `LiftedLocalTypes` behavioral test; operationally by testing/quick's banked suite.

**The same slot is reached through a KEYED COMPOSITE, and there the loss is total rather than
merely imprecise.** The argument position above was the first consumer; a `map[K]any` value, an
`any` struct field and a sparse-`[N]any` element are the same empty-interface slot arrived at
through `convKeyValueExpr` instead of `convExprList`, and they were not marked. For a literal
with a reachable `return` the natural type is at least a func type of the right arity, so the
defect only narrowed a result type. For a literal whose body **never completes normally** there
is no return statement to infer from at all, so C# infers `Action` and the Go result type is
gone outright:
```go
FuncMap{"die": func() bool { panic("die") }}   // text/template exec_test
```
```csharp
["die"u8] = bool () => { throw panic("die"); }  // was: () => { throw panic("die"); }
```
The reflection bridge then reports `NumOut() == 0` — truthfully, because the datum is missing
from the emission, not from the bridge — and `text/template`'s own `goodFunc` rejects a function
Go accepts ("function die has 0 return values; should be 1 or 2"), panicking as the FuncMap is
registered and taking **16 of that package's 52 verdicts** with it. The mark is applied where the
value's declared slot is already resolved, so all three keyed forms are covered by one predicate;
a slot with a CONCRETE func type (`map[string]func() bool`) has a delegate target and is
deliberately left exactly as it was. Guarded by `untypedInterfaceFuncLit_test.go`
(`TestUntypedInterfaceFuncLitResultType` — the panic-only literal, a normal-return literal, an
`any` struct field, the MULTI-result arm, and the concrete-slot control), each arm proven
failing-first independently.

**The MULTI-result arm has the same owner from the opposite end.** The single-result rule above
was scoped for want of a demonstrated consumer; `html/template`'s escape_test is one. Its
`FuncMap{"pred": func(a ...any) (any, error) {…}}` renders every arm as a C# tuple carrying a
typeless element — `return (i - 1, default!)` and `return (default!, fmt.Errorf(…))` — so where
the panic-only literal has NO arm to infer from, this has arms that contribute nothing. Neither
fixes a delegate type, and inference fails outright (CS8917, then CS1662/CS8716 on each return).
The declared result tuple is stated explicitly through `generateResultSignature`, the same helper
the generic-inference arm already used:
```csharp
["pred"u8] = (any, error) (params ꓸꓸꓸany aʗp) => { … }
```

## A `:=`-bound func literal states its declared result type when its body cannot supply it
A function literal bound with `:=` emits as `var f = (…) => …`, so C# takes the delegate type from
the body's return arms. Two body shapes cannot supply the declared Go result list, and the literal
is then rejected where it is passed as its declared func type (CS1503):

- **No return statement at all.** A literal with results whose body only panics (x/sync
  singleflight's `TestPanicDo`) has nothing to infer from, so C# infers `Action`.
- **A forwarded multi-value call that converts.** `return os.Open(name)` against a declared
  `(io.ReadCloser, error)` is emitted as a tuple of converted temps, whose natural type is the
  adapter class `(os_FileжReadCloser, error)` (x/mod's sumdb/dirhash).

Both state the declared result type, through the same explicit-return-type mechanism as the arms
above:
```go
fn := func() (interface{}, error) { panic("boom") }
open := func(name string) (io.ReadCloser, error) { return os.Open(name) }
```
```csharp
var fn = (any, error) () => {
    throw panic("boom");
};
var open = (Δio.ReadCloser, error) (@string name) => {
    var (ᴛ1, ᴛ2) = os.Open(name);
    return (new os_FileжReadCloser(ᴛ1), ᴛ2);
};
```
The forwarded-call test is the predicate `visitReturnStmt` uses to decide the element-wise
conversion (`forwardedReturnNeedsConversion`), so the type is stated exactly when the return emits
adapter wrapping. A SINGLE-result literal returning one concrete adapter (`return
new slog.JSONHandlerжΔHandler(…)` against `slog.Handler`) is left unprefixed: delegate covariance
converts `Func<…, Adapter>` to `Func<…, Handler>`, and testing/slogtest's banked suite compiles that
shape. Guarded by the `PanicOnlyFuncLiteralVar` and `FuncLiteralDeclaredResultIface` behavioral
tests.

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

In the INTERNAL white-box test bridge (the `_test.go` files of the package under test) no metadata file receives
the accessibility records: `recordTypeAccessibility` hands the attributes back and the declaration carries them
inline. The struct lift always wrote its stamp there. The interface lift discarded the returned attributes, so a
function-local interface in an internal test file had no `[GoLocalName]` at all, and go2cs-gen, which finds the
struct field embedding an interface by that stamp, forwarded a promoted method to `recvᴛ.<Func>_<name>.F()`
(CS0120/CS1061, BurntSushi/toml's `TestEncodeAnonymousNoStructField`). The interface declaration now places them
the same way:

```csharp
[GoType("dyn")] [GoLocalName("Inner")] internal partial interface TestLocalEmbeddedInterface_Inner {
```

Outside the bridge nothing comes back and the record stays in `package_info.cs`, so production emission and the
external test variant are unchanged. Guarded by `internalTestLocalInterfaceStamp_test.go`.

**A local type constructed by name is named by its lift.** A conversion to a function-local named pointer from a
non-nil pointer, `type sPtr *s; dps := sPtr(ps)` (testify's `assertions_test.go`), is emitted as a constructor call.
The constructor names the lifted type, `new topLevel_sPtr(ps)`, the same name a conversion from `nil` already used
(`((fromNil_sPtr)nil)`), and never the Go spelling `sPtr`, which names no C# type (CS0246). The function containing
the type does not change the rule: a type declared inside a closure is named by its lift too. A package-level type
is unaffected, because its lifted name and its Go name are the same. Guarded by the `LocalNamedPointerConversion`
behavioral test.

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

## A methodless named func type renders as its base delegate

Go treats a named func type as freely interconvertible with its underlying `func(...)` when the
type has **no methods** — the name is purely documentary. `type releaseConn func(error)`
(database/sql) and `type CancelFunc func()` (context) are assigned to and from anonymous
`func(...)` values without conversion: `grabConn` returns `releaseConn`, `queryDC` takes
`func(error)`, and Go passes one to the other. Emitting the named type as a *distinct* C#
delegate (`ΔreleaseConn`) broke this — the base `Action<error>` its underlying renders to has no
implicit conversion to it (CS1503/CS0029), and the mismatch even excluded the `ж`-receiver
overload of methods taking such a param, so `db.pingDC(...)` on a boxed `*DB` failed with CS1929.

A **non-generic** named func type with **no methods** is therefore rendered AS its base C#
delegate (`Action`/`Func<…>`) everywhere it is referenced (`getAliasQualifiedTypeName`/`getFullyQualifiedTypeName` return
the underlying signature), and its declaration is skipped (`visitFuncType` emits only a marker
comment). Every named↔underlying conversion becomes identity, exactly as Go models it:
```csharp
// type releaseConn is a methodless func type — rendered inline as its base delegate
internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<ΔConn> Ꮡc, context.Context _) { … }
internal static error queryDC(this ref DB db, …, Action<error> release, …) { … }
```
Three exclusions keep the collapse sound — a type is left as a named delegate if any holds:
- it **has methods** (its method set is meaningful — the `FirstClassFunctions`/`hashFunc` wrap case
  below still applies);
- it is **generic** (it is referenced as `Seq<V>`, and the type parameter must stay in scope — see
  the generic-`Seq` range-over-func case);
- its **signature references another named func type, including itself**. A *self-referential* func
  type — `type stateFn func(*machine) stateFn` (a Go state machine, `NamedFuncTypeStateMachine`) —
  has no finite base-delegate form (`Func<M, Func<M, …>>` is infinite); and a reference to another
  named func type (`strategy func(score) action`) would leave that name undefined after collapse.
  Only the *leaves* of the func-type reference graph collapse; a referencing type stays named and
  renders the collapsed leaf inside its own signature.

Because the collapse applies at both the declaration and every reference, and to foreign types too
(context's `CancelFunc` collapses in context's own conversion, so database/sql sees `Action`),
consistency holds across packages. One position needed a companion fix: a variadic `...Option`
element is package-class-qualified (`main_package.Option`) for a package-local named type, which
would mangle a collapsed delegate to `main_package.Action` (CS0426) — `variadicElementType` now
skips the qualifier when the element collapsed. Cleared 13 of database/sql's 17 errors (the whole
named-func family + the CS1929 it masked). Guarded by `MethodlessFuncType` (a function returning
the named type, one taking the anonymous underlying, a struct field, and a tuple-deconstruction
seam across the two); regression-checked against the self-referential (`NamedFuncTypeStateMachine`,
unchanged), nested-reference (`FirstClassFunctions`), and variadic-param (`PublicizedFuncTypeParam`)
cases.

**A collapsed methodless func type must NOT export a `[GoTypeAlias]`.** When such a type is *also*
collision-renamed — `type Filter func(...)` alongside a method `Filter` (go/ast's `Filter` vs
`(CommentMap).Filter`; the `ReservedTypeMethodCollision` shape) — the rename records an exported
`[assembly: GoTypeAlias("Filter", "ΔFilter")]` so consumers can name the renamed type. But because
the type collapses to its base delegate, **no `<pkg>_package.ΔFilter` type is ever emitted** — so a
consumer that loads the alias generates `global using astꓸFilter = go.go.ast_package.ΔFilter;`
naming a nonexistent type (go/doc referencing `ast.Filter`, CS0426). `visitFuncType` now records
each collapsed methodless func type's name in `packageInlineFuncTypeNames`, and the exported-type-
alias emission skips any alias whose key *or* value matches (the collision path stores the alias
under the renamed value `ΔFilter`, the plain path under the raw name) — so the alias is never
exported and the consumer renders `ast.Filter` inline as `Func<nint, bool>` through the normal
collapse. (Guarded by the `CrossPkgUser` extension — a cross-package `CrossPkgLib.Sift` methodless
func type colliding with a `Sift` method, named as a var type and rendered inline, output vs Go; and
by `ReservedTypeMethodCollision` whose `[GoTypeAlias]` is now correctly absent.)

When such a collapsed delegate's signature carries a parameter whose type lives in a **sub-package**
(an import path with a slash), the `Func<…>`/`Action<…>` rendering must qualify that type as the
package **class**, not the namespace. The collapsed signature is produced from the Go signature's
`t.String()`, which keeps the canonical import PATH inline — `func(*sync/atomic.Int32) int32`,
`func(string, io/fs.DirEntry, error) error` (path/filepath's `WalkDirFunc`) — losing the file's import
alias. `convertToCSFullTypeName` converted the whole slash-bearing string as one import path, dotting
the type straight into the namespace: `sync.atomic.Int32` / `io.fs.DirEntry` — CS0234, since `atomic`
is not a namespace of `go.sync` (the type lives in class `atomic_package`). It now splits the trailing
`.TypeName` off at the first `.` after the last path `/`, converts the package path with the class
suffix, and re-appends: `sync.atomic_package.Int32`, `io.fs_package.DirEntry`. The suffix is only added
when the path segment does not already carry it — some callers (a recorded `[GoType]` underlying,
`sync/atomic_package.Uint32`) hand a pre-suffixed path, which would otherwise double to
`atomic_package_package` (a `DefinedTypeOverPkgType` regression, caught and gated). The behavioral
corpus is byte-identical except the intended change, and an A/B reconvert of net+go/types (same package
set) is byte-identical — only the func-type-subpackage-param shape moves. (Guarded by the
`SubpackageFuncTypeParam` behavioral test — a methodless `applyFunc func(*atomic.Int32) int32` whose
collapsed delegate carries the `sync/atomic` sub-package parameter, output-compared vs Go; the same
shape drives path/filepath's `WalkDir`/`Walk` referencing `io/fs.DirEntry`/`FileInfo`.)

A collapsed func type's **parameter list must not be double-converted**. `convertToCSFullTypeName`'s
`func(` handler split the parameter string with `extractTypes`, then re-ran `convertToCSTypeName` over each
result — but `extractTypes` already renders a NAMED parameter in C# form (it strips the Go name and converts
the type). Re-feeding an already-C# `map<@string, ж<Object>>` through the `map<` arm's `splitMapKeyValue`
mis-parsed it into `map<@string, ж<Object>, >` — a spurious trailing empty type arg (CS1031 "Type expected",
go/ast's `NewPackage` taking `type Importer func(imports map[string]*Object, path string) (…)`). The fix makes
`extractTypes` **always** return C#-form (the bare-type/unnamed branch now converts in place too, matching the
named branch), and the caller trusts that output directly instead of a second pass. This is byte-identical
everywhere except named-parameter func types — bare-type func types (`func(int, string)`) were already
converted once and stay so, just at the `extractTypes` site rather than the caller. (Guarded by the
`NamedFuncTypeMapParam` behavioral test — `type Importer func(imports map[string]*Node, path string) (pkg
*Node, err error)` used as a function parameter, output-compared vs Go; CNR byte-identical across the corpus,
and an A/B reconvert of go/ast shows only that one collapsed-delegate parameter shape moving.)

**A collapsed methodless named func type's DELEGATE TYPE renders through `iifeDelegateType`, not the string
path.** The double-conversion fix above kept the collapsed delegate on `convertToCSFullTypeName`'s `func(`
string handler — but that string domain naively slash→dots a cross-package element's import PATH. go/doc
passes `simpleImporter` to `ast.NewPackage` (whose `importer` is `ast.Importer`, a methodless func type), so
the converter wraps the method group in the collapsed base delegate
`new Func<map<@string, ж<go.ast.Object>>, @string, (ж<go.ast.Object> pkg, error err)>(simpleImporter)` —
`go/ast.Object` mangled to `go.ast.Object` (no `_package` class, no file alias), so `ast` is not a namespace
of `go` (CS0234 ×2), and the resulting error-typed delegate then fails the method-group→delegate conversion
(CS0123). `getCSharpTypeName` now routes a methodless named func type through the SAME structural
`iifeDelegateType` path an ANONYMOUS signature already takes (that path exists precisely because the string
path mangles slash-bearing package paths), naming each element via `aliasedElementTypeName` — so the
cross-package `ast.Object` keeps its `ast` alias (and a Δ-renamed foreign element its recorded `ꓸ`-alias):
`new Func<map<@string, ж<ast.Object>>, @string, (ж<ast.Object>, error)>(simpleImporter)`. The only visible
change for a SAME-package/single-segment element is that a multi-result signature's delegate type drops its
Go result-tuple element NAMES (`(ж<Node> pkg, error err)` → `(ж<Node>, error)`), matching how anonymous
signatures already render — cosmetic, both compile. An A/B full-stdlib reconvert moves **11 files, all
equal-or-better**: the go/doc mangle fixed, plus `go/parser`/`go/scanner` (`go.token_package.ΔPosition` →
`tokenꓸPosition`), `go/internal/gccgoimporter` (a malformed `(io.ReadCloser>, error)` → valid),
`internal/trace/traceviewer` (`net.http_package.Request` → `http.Request`), and `path/filepath`
(`io.fs_package.DirEntry` → `fs.DirEntry`) all cleaned up, with `bufio`/`go/ast`/`nettest` only dropping
cosmetic tuple names; CNR touches only three existing goldens (`NamedFuncTypeMapParam`,
`SubpackageFuncTypeParam`, `FirstClassFunctions`), all the same pattern. (Guarded by the `CrossPkgUser`
extension — a package-level `simpleResolve` passed as a METHOD GROUP to `CrossPkgLib.Resolve`, whose
`Resolver` is a methodless func type naming the cross-package `*CrossPkgLib.Node`, so the wrapped delegate
renders `ж<CrossPkgLib.Node>` via the alias, output-compared vs Go. The single-segment producer compiles
either way, so the byte-golden — unnamed vs Go-named result tuple — is what guards the routing; the exact
slash-bearing CS0234/CS0123 needs a multi-segment producer like go/ast, verified by the go/doc source A/B.
go/doc's own remaining block is the SHARED generated-adapter forwarding of go/ast's unexported interface
marker methods — a separate root.)

A companion root cleared path/filepath fully: a **cross-package type ALIAS whose target lives in yet
another package** — `os.FileInfo = fs.FileInfo` (os/types.go, target in `io/fs`) — is emitted as an
assembly-scoped `global using FileInfo = go.io.fs_package.FileInfo;` in **os's own** conversion, never as
a member of the os package's C# class, so a cross-package reference `os_package.FileInfo` does not resolve
(CS0426, path/filepath's `lstat = os.Lstat` func value). `getAliasQualifiedTypeName` now renders such an alias as its
**target** — `os.FileInfo` → `fs.FileInfo` (→ `io.fs_package.FileInfo` via the file's `fs` using). Gated
to a **different-package target**: an alias to a SAME-package type (`CrossPkgLib.Temperature = Celsius`)
already resolves through the existing `ꓸ` global-using alias (`CrossPkgLibꓸTemperature`) and is left
untouched — narrowing here reverted a `CrossPkgUser` churn the blanket form caused. CNR byte-identical;
an A/B of os+io/ioutil (same package set) shows only that one intended resolution (io/ioutil's `ReadDir`
sort lambda moved `osꓸFileInfo` → `fs.FileInfo`, matching the file's other `fs.FileInfo` refs — still
compiles). **GUARD OWED** — the shape needs three packages (B declares `Y`, A aliases `type X = B.Y`, C
references `A.X`), which neither the single-package baseline nor the 2-package `CrossPkg` harness
expresses; validated by the `core/path/filepath` build (1→0) + io/ioutil build.)

**A func type renders structurally in EVERY type-name path — the signature never stringifies.**
`getAliasQualifiedTypeName` now carries a `*types.Signature` arm (`signatureTypeName`, beside `iifeDelegateType`)
mirroring the slice/map/chan composite arms: Go syntax — `func(name type, …) results` — with every
parameter/result type resolved **recursively**, so a cross-package element keeps the file's short
import alias exactly like the neighboring map/slice fields. Previously only *some* positions routed
through the structural `iifeDelegateType` (var declarations; variadic or slash-bearing struct
fields); every other position — a struct field of a **named** methodless func type (go/importer's
`importer gccgoimporter.Importer`), a MAP field's func **value** type (net/http's `TLSNextProto`
maps), a same-package named func field (traceviewer's `f MutatorUtilFunc`) — reached
`convertToCSFullTypeName` as `t.String()` text with import PATHS inline, and the slash heuristics
mangled those one of **three ways** depending on the string's shape: the whole-string
path-conversion arm fires when no dot-after-slash precedes the first `[` (a leading `map[` bracket),
naively dotting every path — `ж<go.types.Package>`, `ж<crypto.tls.Conn>` (no `_package` class, and
under a `go.go`-nested namespace the leading segment binds the child namespace — CS0234) — while the
split-at-dot arm mangles a mid-signature path to a classed-but-unrooted form
(`@internal.trace_package.UtilFlags`, traceviewer mmu.cs). With the structural arm the string
reaching the parser is slash-free (`func(*Server, *tls.Conn, Handler)`) and each element converts
through the normal alias route. Result NAMES are preserved, so a named multi-result field keeps its
named C# tuple (the display-path advantage the old struct-field routing existed to protect); a
same-package/builtin signature renders byte-identically to the old `t.String()` path (zero churn —
CNR confirmed across all 331 behavioral projects). A variadic tail renders `...elem` (which the old
path's `..`-strip reduced to the unparseable `.elem`) and lowers through the parser to the golib
`ꓸꓸꓸ` delegate family (next paragraph). One side effect: element recursion passes through
`getAliasQualifiedTypeName`'s foreign-ALIAS arm, so a signature naming a cross-package alias (`os.FileInfo` →
`io/fs.FileInfo`) now registers the **target's** package for a file-local using — a few stdlib files
gain a benign `using fs = …;` alias line (`collectTypePackages`' Named case does not match a
`*types.Alias`, so the old path never registered it). Whole-stdlib A/B footprint: 20 files — the
go/importer field fixed, `ж<tls.Conn>` in net/http server/transport/h2_bundle (field + composite
literals), traceviewer's `Func<trace.UtilFlags, (slice<slice<trace.MutatorUtil>>, error)>`,
go/scanner's `err` field moving to the canonical `tokenꓸPosition` alias (the old
`go.token_package.ΔPosition` resolved only by go.go-namespace luck), the variadic type-assert
target below, one comment-alignment shift, and the benign using-line additions. Cleared the
IMP-2/HTTP-3 CS0234 cluster (net/http ×8 + go/importer ×6 + traceviewer). (Guarded by the
`SynthesizedDelegateChildPkg` behavioral test — a nested CHILD subpackage (slash-bearing import
path) whose `*inner.Record` rides a named methodless func-type field with a nested-tuple lookup
param AND a `map[string]func(*inner.Record, string)` field, both invoked at runtime vs Go.)

**The func-type string parser splits parameters at TOP-LEVEL commas only — and a variadic tail
lowers to the `ꓸꓸꓸ` delegate family.** `extractTypes` split the parameter list with a naive
`strings.Split(signature, ",")`, so a nested func param returning a TUPLE — `lookup func(string)
(io.ReadCloser, error)` (go/internal/gccgoimporter's `Importer`, surfacing as go/importer's
`gccgoimports.importer` field) — shredded at the tuple's interior comma, unbalancing the assembled
delegate: `Func<@string, (io.ReadCloser>, error)` (the inner `>` closes before the tuple's second
element — a 6-error syntax cascade, IMP-1). `splitTopLevelParams` tracks `<>`/`()`/`[]`/`{}` depth
(with the channel-arrow `<-` guard `splitMapKeyValue` already carries) and splits only at depth 0.
On top of that, a variadic tail (`...elem`, from the structural render above) converts its ELEMENT
type in `extractTypes` and carries an ellipsis-family marker that the `func(` assembler hoists into
the delegate FAMILY name — `Actionꓸꓸꓸ<@string, any>` — mirroring `iifeDelegateType`'s lowering
exactly. That fixed the variadic func type as a type-ASSERTION target as a rider:
`.(func(string, ...any))` (net/http transport.go's `tLogKey` logger) previously emitted the
unparseable `._<Action<@string, .any>>(ᐧ)` and now renders `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`.
(Guarded by the `FuncFieldNestedTupleParam` behavioral test — builtin-typed struct fields with
nested-func-returning-tuple params in both the anonymous and named-collapse forms plus a
named-tuple-result sibling, all invoked at runtime vs Go.)

A **type ASSERTION** whose target is a methodless func type must assert against the **collapsed
delegate**, not the (never-emitted) name. `ci.(Compressor)` where
`type Compressor func(io.Writer) (io.WriteCloser, error)` (archive/zip's compressor/decompressor
registries) rendered `ci._<Compressor>()` — `convTypeAssertExpr` converts the target via `convExpr`,
which emits the bare ident, and after collapse `Compressor` is undefined (CS0246). When the asserted
target is a methodless named func type, the assertion now renders its `getCSharpTypeName` (the collapsed
`Func<…>`): `ci._<Func<io.Writer, (io.WriteCloser, error)>>()` — matching how the stored value was
emitted (a collapsed delegate). Other assertion targets are unchanged. (Guarded by the
`MethodlessFuncTypeAssert` behavioral test — `i.(Compressor)` on a matching and a non-matching dynamic
type, output-compared vs Go; CNR byte-identical and an A/B of archive/zip shows only the two intended
`_<Compressor>`/`_<Decompressor>` → `_<Func<…>>` lines.)

An **UNINITIALIZED local `var` of a methodless named func type** renders its declared type through the
same structural path. `visitValueSpec`'s no-initializer branch computed the type from
`convertToCSTypeName(getAliasQualifiedTypeName(...))` (the string path) and only re-routed a bare *anonymous*
`*types.Signature` through `getCSharpTypeName`; a methodless NAMED func type is a `*types.Named`, so it kept
the string render — and that render mangles a slash-bearing cross-package element. go/parser's `parseDecl`
declares `var f parseSpecFunction`
(`type parseSpecFunction func(doc *ast.CommentGroup, keyword token.Token, iota int) ast.Spec`), which
emitted `Func<ж<go.ast.CommentGroup>, go.token.Token, nint, go.ast_package.Spec> f = default!;` — the
`go.ast`/`go.token` elements re-root to the nonexistent `go.go.ast`/`go.go.token` (CS0234), and the
declared delegate then mismatched the lambdas assigned to `f` and the `parseGenDecl(keyword, f)` parameter,
which render the SAME Go types structurally as `ast.CommentGroup`/`token.Token` (CS1661/CS1678/CS1503 — 12
errors, all this one declaration). The no-initializer branch now routes a func-typed var (anonymous
signature OR methodless named func, via `methodlessNamedFuncSignature`) through `getCSharpTypeName` →
`iifeDelegateType`, whose `aliasedElementTypeName` keeps each element's `pkg.Type` alias:
`Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f = default!;`. This precedence matches
`getCSharpTypeName`'s own — the func render wins over the foreign-alias route (which for a methodless named
func would point at the SKIPPED delegate declaration); a non-func foreign-renamed local keeps its alias
unchanged. An A/B full-stdlib reconvert moves exactly one file (go/parser/parser.cs), greening go.parser
outright. (Guarded by the `MethodlessFuncType` extension — an uninitialized `var find lookup` where
`type lookup func(string) (path string, ok bool)`; the byte-golden captures the structural render
`Func<@string, (@string, bool)>` — dropping the Go result NAMES the string path keeps — output-compared vs
Go. As with the delegate-routing sibling above, a single-segment/same-package producer compiles either way,
so the unnamed-vs-Go-named result tuple is what guards the routing; the exact slash-bearing CS0234 needs a
multi-segment producer like go/ast, verified by the go/parser source A/B.)

## Named delegate types wrap mismatched initializers
A NAMED func-type field initialized with a value of a DIFFERENT delegate type has no implicit
C# conversion: internal/concurrent's `keyHash: mapType.Hasher` feeds a `hashFunc` field from a
`Func<…>` field. The composite-literal walk resolves each element's field BY NAME (keyed-aware)
and wraps mismatched delegate values in the target delegate's constructor —
`keyHash: new hashFunc((~mapType).Hasher)` (the wrap splits a C# named-argument label first).
FuncLit and nil initializers stay bare. Guarded by `FirstClassFunctions`
(`handler`/`provider`/`registry`).

## A named delegate value passed to a structural func parameter re-wraps
The MIRROR of the argument-position named-delegate wrap: a **structural** (written-anonymous) func
parameter receiving a value of a **named** delegate type — net/http h2_bundle's
`sc.scheduleHandler(…, handler)`, where `handler` is `HandlerFunc` and the parameter is
`func(ResponseWriter, *Request)` (CS1503). Go converts named→structural implicitly; C# needs the
same delegate re-wrap, targeting the synthesized structural delegate:

```go
type Handler func(int, string) string   // has a method → distinct C# delegate
func invoke(f func(int, string) string, n int, s string) string { return f(n, s) }
var h Handler = describe
invoke(h, 1, "a")
```
```csharp
invoke(new Func<nint, @string, @string>(h), 1, "a"u8);
```

Two argument shapes render named and take the wrap: a value whose **Go type** is a named func type
(with methods), and a `:=` local **declared from a method group**, which the declaration emission
types with the matching package named delegate (`HandlerFunc handler = Ꮡsc.Value.handler.ServeHTTP;`
— the bare-function-value `:=` rule above) even though go/types keeps it structural — the exact
h2_bundle shape. A **methodless** named func type already *renders* as the structural delegate
(`methodlessNamedFuncSignature` collapses it — same C# type), so it stays bare; method groups and
func literals themselves convert natively. A generic structural parameter (unsubstituted type
params) also stays native. (Guarded by the `NamedDelegateStructuralParam` behavioral test —
named-with-method and method-group-declared locals wrapped, methodless/method-group/func-literal
controls bare, values vs Go.)

The same mirror applies to a **composite-literal FIELD** (2026-07-17; sort's test-suite
conversion): the composite walk previously wrapped only the named-field ← different-delegate
direction, so GOROOT sort example_keys_test's `planetSorter{planets: planets, by: by}` — a `By`
value (named, with a `Sort` method) initializing the written structural field `by func(p1, p2
*Planet) bool` — emitted the bare `by: by` against the `Func<ж<Planet>, ж<Planet>, bool>`
constructor parameter (CS1503; the Phase-4 blocker-map row B10b). The structural-field arm now
applies the identical named-rendering test and wrap: `by: new Func<ж<Planet>, ж<Planet>,
bool>(by)`. Method groups, func literals, and nil stay bare, and generic fields stay native, as
at call sites. (Guarded by the `NamedFuncTypeStructuralField` behavioral test — the By-with-method
sorter pattern wrapped, a method-group field initializer control bare, values vs Go.)

## Func-typed fields with a cross-package (slash-path) type render structurally
A func-typed struct field whose signature names a type from a **multi-segment** import path —
testing/quick's `Config.Values func([]reflect.Value, *rand.Rand)`, where `rand` is `math/rand` —
must render as a structural `Action`/`Func<…>` delegate via `getCSharpTypeName`, not through the
string display path. The display path stringifies the signature as `func([]reflect.Value,
*math/rand.Rand)` and splits the slash-bearing import path on `/`, emitting the dotted
`math.rand.Rand`; but `math` aliases to `math_package`, so `math.rand` resolves to the nonexistent
`math_package.rand` (CS0426). The structural renderer recurses per signature element and qualifies
each named type by its package **name**:

```go
type Config struct {
    Values func([]reflect.Value, *rand.Rand)   // rand is math/rand
}
```
```csharp
public Action<slice<reflectꓸValue>, ж<rand.Rand>> Values;
```

The re-routing is gated on the signature string containing `/` **or the signature being variadic**:
the string path cannot render a variadic signature at all — `getAliasQualifiedTypeName`'s `..` strip reduces the
ellipsis of go/build's `JoinPath func(elem ...string) string` (Context, build.go:84) to `.string`,
emitting the unparseable `Func<.@string, @string>` (CS1031 + CS1003 ×2, all three go.build errors),
and even unstripped it has no variadic lowering. Structurally such a field renders the golib
variadic delegate family (`public Funcꓸꓸꓸ<@string, @string> JoinPath;` — see the variadic-lowering
section below), which loose-arg, empty and spread calls through the field all bind. Every other
func field keeps the display path: `func(string) (importPath string, ok bool)` preserves its named
tuple elements that the structural renderer drops. (Guarded by the `FuncTypeParam` behavioral
test's `runner.gen` field, and by `VariadicFuncFields` — a struct with variadic func-typed fields
assigned from a named func and func literals, called loose/empty/spread — for the variadic arm.)

## A variadic func type lowers to the golib `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` delegates

A **variadic function TYPE used as a value** — a parameter, variable, struct field, or collapsed
methodless named type such as go/types' `reportf func(format string, args ...interface{})` — used
to have three mutually incompatible lowerings: the delegate type rendered `Action<@string,
slice<any>>` (no `params` — the BCL `Action` cannot express one), a variadic func LITERAL emitted
the named-function convention `(@string format, params ꓸꓸꓸany argsʗp) => …` (CS1661/CS1678
against that `Action`), and calls through the value passed loose Go-style args as if `params`
existed (`reportf("…"u8, (~f).typ)` — CS1503; `reportf("empty type set"u8)` — CS7036).

The lowering now targets a golib delegate family carrying a real C# 13 `params Span<T>` tail
(`src/core/golib/variadic.cs`; fixed-arity prefixes up to eight mirror the BCL Action/Func family,
and the `ꓸꓸꓸ` suffix reads as Go's `...`):

```csharp
public delegate void Actionꓸꓸꓸ<T1, TArg>(T1 arg1, params Span<TArg> args);
public delegate TResult Funcꓸꓸꓸ<T1, TArg, out TResult>(T1 arg1, params Span<TArg> args);
```

`iifeDelegateType` — the single structural lowering every `getCSharpTypeName(*types.Signature)` and
collapsed methodless named func type routes through — names the family when `sig.Variadic()` and
passes the variadic **element** type as the last type argument. Everything else then agrees with
**zero changes** to the other emissions, because the parameter types match by identity
(`ꓸꓸꓸT` *is* `Span<T>`):

- the named-function convention (`internal static @string gather(@string prefix, params ꓸꓸꓸnint
  valsʗp)`) converts as a method group — `apply(gather)` stays bare;
- a variadic func literal (`(@string prefix, params ꓸꓸꓸnint valsʗp) => …`, C# 13 params lambda)
  converts natively — go/types' `comparable(typ, true, default!, (@string format, params ꓸꓸꓸany
  argsʗp) => {…})` now binds its `Actionꓸꓸꓸ<@string, any>` parameter;
- calls through the value pass loose args or an empty tail via C# `params` expansion, and a Go
  spread (`f(nums...)`) binds the slice's `.ꓸꓸꓸ` Span in normal form;
- a C# consumer calls a transpiled printf-style callback naturally (`ctx.Logf("…", a, b)`) — the
  library use case that ruled out the pack-into-a-`slice<T>` alternative.

A `:=`-declared variadic func literal is untouched: it keeps C#'s natural (params-capable) lambda
type under `var` (the `VariadicClosureSpread` shape). One deliberate residue: `defer`/`goǃ` of a
call **through a variadic func value** would need to capture the `Span` tail, which a ref struct
cannot be — pack into a slice at such a site. ⚠ **That residue now has its one demonstrated
consumer, and it is a TEST file** (found 2026-08-19, lane `claude/variadic-call`): the census of
the whole Go 1.23 tree finds exactly ONE `defer`/`go` of a variadic func literal —
`html/template/examplefiles_test.go:90`, `defer func(dirs ...string){…}(dir1, dir2)` — which emits
`defer((params ꓸꓸꓸstring dirsʗp) => {…}, dir1, dir2, ref ᒐ)` and fails inference against
`builtin.defer<T1,T2>(Action<T1,T2>, T1, T2, ref GoFrame)`: **CS0411**. That is why the claim used
to read "no stdlib occurrence" — the original A/B was over PRODUCTION sources, and the shape lives
only in a `_test.go`, so nothing before the Phase-4 `-tests` pipeline could see it. It is one of the
two roots now blocking `html/template`'s 243 verdicts. Full-stdlib
A/B footprint: go/types predicates.cs/expr.cs plus every file that renders a variadic func type
structurally (inspected file-by-file at introduction). (Guarded by `VariadicFuncValues` — a named
func AND a func literal satisfying a variadic func-typed param, loose/empty/spread calls through
it, and a nil-compared variadic func-typed var — output-compared vs Go.)

**A type-ASSERTION target routes through the same structural lowering.** `convTypeAssertExpr` rendered
the asserted type by converting the TYPE EXPRESSION through the string-based type-name path, which
skips the variadic lowering above — net/http transport.go's
`cw.(func(string, ...any))` emitted `._<Action<@string, .any>>(ᐧ)` with a literal `.any` (CS1001, the
`...` mangled instead of lowered). An anonymous-signature assert target now renders through
`getCSharpTypeName` → `iifeDelegateType`, exactly like the collapsed methodless NAMED func target already
did: `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`. Non-variadic signatures render identically on both paths, so the
only full-stdlib delta is the transport.cs site. (Guarded by `VariadicFuncTypeAssert` — a positive
variadic assert invoked through the asserted value, a negative assert on a non-func value, and a
non-variadic anonymous func assert, output-compared vs Go.)

**…and the BOXING side needs the matching cast, or the two can never meet (2026-08-20).** Rendering
the assert target through `iifeDelegateType` fixes the *reading* half; the *writing* half is where the
value acquires a dynamic type, and for a variadic func that type is C#'s, not Go's. C# gives a method
group or lambda at an untyped destination a **natural function type**: for a non-variadic signature
that is `Func<…>`/`Action<…>` — go2cs's own lowering, so the two already agree and nothing is emitted
— but a `params` signature has no BCL delegate, so C# **synthesizes** one and the box carries
`<>f__AnonymousDelegate0` forever. html/template's `funcMap` is `map[string]any` of `func(...any)
string` escapers assigned as method groups, and its own `TestRedundantFuncs` reads them back with
`funcMap[n].(func(...any) string)`: `interface conversion: interface {} is <>f__AnonymousDelegate0,
not go.Funcꓸꓸꓸ<object, @string>`. The assert was right, the box was wrong, and both were emitted by
the same converter.

So a variadic func entering EMPTY-INTERFACE space is cast to its Go func type at the boundary —
`((Funcꓸꓸꓸ<any, @string>)(attrEscaper))` — which is the same carry-your-Go-type rule the pointer box
and the untyped-constant box already apply at that same finite set of slots, and it lives with them in
`typedNilInterfaceBoxing.go`. Both sides now name the type through `getCSharpTypeName` →
`iifeDelegateType`, one renderer, so they cannot drift. The cast is a no-op wherever the value already
has that type (a typed var, a call result), so it widens nothing; a NON-empty interface target needs
nothing either, since a bare func type has no methods and satisfies no other Go interface. (Guarded by
the extension to `VariadicFuncTypeAssert` — a variadic func literal direct to `any`, a variadic method
group as a `map[string]any` element, through a plain assignment, and as an `[]any{…}` element, each
asserted back; plus a NON-variadic literal direct to `any` as the control that must keep matching
without a cast. Neutering the cast prints `no match` on all four and leaves the control passing.)

**A variadic METHOD VALUE was the one shape in the family still frozen at fixed arity (2026-08-26).**
`errorf := t.Errorf` — go/types' and `slices`' own idiom, `errorf = t.Logf` one statement later, then
loose Go-style calls — has TWO emissions, and both dropped the variadic tail. A bound method value
forwards through a lambda carrying the method's own parameters, and that lambda rendered the tail as
the plain `slice<T>` the signature *stores* rather than the `params ꓸꓸꓸT` convention every declared
variadic function uses: `(@string p1, slice<any> p2) => Ꮡt.Errorf(p1, p2)`. Every call through the
value was then an arity error — `errorf("…", n)` CS1503 on a bare `n` against `slice<any>`,
`errorf("…", a, b)` CS1593 "does not take 3 arguments", `errorf("…")` CS7036 — which is the same
family the lambda's explicit parameters were introduced to fix, one level in. The tail now renders
through `variadicParamType`, the same routine the named-function convention uses (a file-local
`using ꓸꓸꓸT = Span<…>;` alias where one is mintable, inline `Span<T>` otherwise), so the forwarded
argument binds the receiving `params ꓸꓸꓸany` parameter directly and the call inside the lambda is
unchanged.

The DECLARATION is the second half, and it is not optional. A `params` lambda has no BCL delegate, so
`var` gives it a **synthesized** natural type — which binds that lambda and gives C# no reason to hand
the same type to the second lambda the reassignment installs. `visitAssignStmt`'s method-group branch
therefore names golib's variadic delegate family when the signature is variadic and no package named
func type matches — `Actionꓸꓸꓸ<@string, any> emit = (@string p1, params ꓸꓸꓸany p2) => …` — reusing
`iifeDelegateType`, the same lowering `getCSharpTypeName` already gives every func type used as a
value, so there is exactly one spelling of this type in the emission. Non-variadic method values keep
`var`, unchanged. (Guarded by the `VariadicFuncValues` extension — a pointer receiver's variadic
method bound by `:=`, conditionally reassigned to a second variadic method, then called with loose
args, an empty tail and a spread; it fails on the pre-change converter with CS1503 + CS1593 + CS7036,
which is exactly the `slices` `TestGrow`/`TestConcat` error set.)

A/B footprint: this is the half of the arc that moves anything outside its own guard, and it moves
two lines. CNR at 645 behavioral packages reports `DeferCallOrder` and `GoCallVariations`, both
`f1 := fmt.Println` — a variadic PACKAGE function bound as a method value, which was the same
`var`-inferred synthesized delegate and is now `Funcꓸꓸꓸ<any, (nint, error)>`. Both still compile and
still match `go run`. The whole converted standard library re-emits byte-identically (4,173 artifacts,
0 changed), because the rule fires on nothing else: a method value whose signature is not variadic
never reaches it.

## `reflect.Value.Call` over a variadic func value is TYPED dispatch — no reflective invoke can carry the tail

The `params Span<T>` tail above is what makes a converted variadic callable and readable from Go
AND from C#. It also puts the value permanently out of reach of every reflective invoke path:
`Span<T>` is a **ref struct**, and `Delegate.DynamicInvoke` and `MethodInfo.Invoke` both marshal
their arguments through an `object?[]` a ref struct cannot enter. `System.Linq.Expressions`
refuses one outright as well, so the method-value binder's `Expression.Lambda` approach
(GoReflect.MethodSets.cs) does not generalize either. `reflect.Value.Call` therefore threw
`NotImplementedException` for every variadic func value — which is 13 of `text/template`'s 52
verdicts, since its whole `FuncMap` feature calls user functions exactly that way.

The call is made in **typed code** instead (`GoReflect.InvokeVariadic`, GoReflect.TypeLayout.cs).
One small generic trampoline per family arity — eighteen, the closed set golib's variadic.cs
declares — is closed over the delegate's own parameter types by `MakeGenericMethod` and cached as
an ordinary delegate, the `elementBoxViaAt` idiom GoReflect.FieldAccess.cs already uses:

```csharp
private static object? callVariadicFunc1<T1, TArg, TResult>(Delegate d, object?[] a, Array t)
{ return ((Funcꓸꓸꓸ<T1, TArg, TResult>)d)((T1)a[0]!, new Span<TArg>((TArg[])t)); }
```

Inside the trampoline the tail is a `TArg[]` and its conversion to `Span<TArg>` is ordinary, so
nothing is boxed and the tail ALIASES the array rather than copying it. Two consequences worth
stating: a panic inside the callee propagates natively (a direct call wraps nothing in a
`TargetInvocationException`, unlike the fixed-arity `DynamicInvoke` path beside it), and a fixed
prefix beyond the family's eight throws a named `NotImplementedException` rather than mis-indexing.

**The delegate being called is not always the family type, and the rebind is what makes that
total.** A variadic func literal in an `any` slot — a `map[string]any` FuncMap value, the exact
`text/template` shape — takes C#'s NATURAL delegate type instead, the same identity difference
`TryFuncShape` had to stop reading off the type NAME. Those rebind onto the family by RETARGETING
through `Invoke` (`Delegate.CreateDelegate(familyType, del, "Invoke")`), never by re-binding the
original's own target and method: a delegate the BRIDGE itself built is expression-compiled — a
variadic method value from `Value.Method` is exactly that — and a compiled lambda's `Method` is not
a runtime `MethodInfo`, which `CreateDelegate` rejects with "MethodInfo must be a runtime MethodInfo
object". Retargeting also carries a multicast invocation list intact. The family's type arguments
are built FROM the delegate's own `Invoke` signature, so the two agree by construction.

Go's `Call` contract shapes the arity rule too: `Call` itself builds the tail slice (`CallSlice` is
the form that takes it pre-built), so the last `In()` is the tail SLICE, every argument from that
position on is assignable to its ELEMENT, and there is no upper bound — only a lower one of
`NumIn()-1`. (Guarded two ways: behavioral `ReflectVariadicCall` output-compares eleven shapes
against `go run` — declared func, empty tail, no fixed params, `...any`, multi-return, no-result,
two fixed params, a variadic METHOD value, and three `map[string]any` literals — while
`GoReflectBridgeClosureTests` pins the three delegate identities, the tail's aliasing, the refusal
of a non-variadic delegate, and every family arity of both families, which are golib-only shapes no
Go program can construct. The arity row matters because only 0, 1 and 2 fixed parameters have a
consumer in the corpus today: 3 through 8 would otherwise be discovered by whichever package
reached them first.)

## `reflect.MakeFunc` is `Value.Call`'s exact inverse — a compiled delegate over the descriptor's carried System.Type (2026-08-29)

Go's `MakeFunc` is runtime machinery end to end: it reinterprets the descriptor into a `funcType`
sub-record, asks `funcLayout` for a stack map, and pairs an assembly stub (`makeFuncStub`) with a
closure context the scheduler calls back through. None of that exists behind a managed-backed
descriptor — `abi.synthType` mints every one as a plain `heap<Type>` box with the CLR
`System.Type` as cargo, so the `Reinterpret<abi.Type, funcType>()` recovers a **zero box** and
`funcLayout` panics `reflect: funcLayout of non-func type <nil>`. First operational hit:
`net/http/httptrace`'s `compose`, which walks `ClientTrace`'s func-typed fields and MakeFuncs a
composed hook for every pair both traces set.

The hand-owned form (`reflect/makefunc_impl.cs`, displaced via the `manualConversionFuncs`
registry) runs the marshalling that `Value.Call` runs, in the opposite direction. Where `Call`
marshals a `slice<Value>` into a delegate's `DynamicInvoke`, `MakeFunc` builds a delegate of
**exactly** the descriptor's carried delegate type whose invocation boxes its CLR arguments,
types each one by the func's STATIC parameter type (`makeTypedValue` — an interface-typed
parameter reports Kind Interface, a nil pointer is a VALID typed-nil Value, and a `[N]byte`
parameter carries the descriptor's `funcParamDims` cargo, the one route a fixed array parameter's
length reaches reflect at all), runs `fn`, and marshals the result Values back out under the SAME
assignability renderer Call's arguments use (`marshalIntoSlot` — one rule for both directions).
A Go multi-return packs into the delegate's own declared `ValueTuple`. The delegate itself comes
from golib's `GoReflect.MakeGoFuncDelegate` — expression-compiled once per delegate type into a
factory (outer lambda takes the `Func<object?[], object?>` invoker, inner IS the typed delegate),
the same memoization rule the method-value binder follows — so the result is callable DIRECTLY as
a typed Go func (`t.DNSStart(info)`), through `Value.Call`, and through composition with itself.

The returned Value rides `typ`'s **own** descriptor box rather than a fresh `synthType`, so the
dims cargo survives and `Type()` interns back to the caller's wrapper: `MakeFunc(t, fn).Type() == t`
by identity. A VARIADIC func type is a loud `NotImplementedException`, not a wrong delegate: its
tail is `params Span<T>`, a byref-like type no expression tree can carry — the route that exists is
the reverse of `InvokeVariadic`'s typed family trampolines above, unbuilt for want of a
demonstrated consumer, exactly as `Value.CallSlice` records. `makeMethodValue`'s identical
`funcLayout` read deliberately stays auto: it is reachable only through `flagMethod`, which the
bridge never sets (`Value.Method` binds the receiver into an ordinary delegate instead). With
MakeFunc live, `reflect/iter.cs`'s rangefunc `Seq`/`Seq2` funcs gain their real implementation
path too. (Guarded by behavioral `ReflectMakeFunc`: the docs swap example invoked directly, the
httptrace compose shape, multi-return, canonical `Type()` identity, `Call` over a made func, an
interface-typed parameter, a typed-nil pointer argument, and a `[4]byte` parameter whose `Len()`
proves the dims cargo threads through. Banked consumer: `net/http/httptrace` 2|0.)

## Major-version import directories
A `/vN` import path segment (math/rand/v2) hosts a package named for the PARENT segment, so the
emitted class follows the package NAME: consumers reference `go.math.rand.rand_package`, and the
namespace is `go.math.rand` — never the path-derived `v2_package` / `go.math.rand.v2`. Go's own
convention (the directory is a version marker, not the package identifier) means the package name
equals the second-to-last path segment, and every place the converter derives a class/namespace/
alias from a `/vN` import path must honor it. There are **four** such derivations, reached by
different renderers, and each needed the convention applied at its own site:

1. **`using`-alias + namespace emission** — `convertImportPathToNamespace` (`visitImportSpec.go`)
   rewrites the last path part to the parent segment via `majorVersionSegmentRegex`, so the file's
   `using rand = go.math.rand.rand_package;` and the package's own `namespace go.math.rand` agree.
2. **`t.String()`-based FQ type rendering** — `getAliasQualifiedTypeName` / `getFullyQualifiedTypeName` (`main.go`) build a
   foreign type's name from the type graph's path-qualified string, whose last segment slash-strip
   assumes the path tail IS the package qualifier. For a `/vN` tail it left the version behind
   (`math/rand/v2.Rand` → `v2.Rand`), which the alias-prepend then doubled into `rand.v2.Rand`
   (`v2` read as a member of class `rand_package` — CS0426). Both renderers now reduce the foreign
   import-PATH qualifier to the package NAME before the slash-strip. `getFullyQualifiedTypeName` also composes
   `pkg.Path()+"_package"` directly for the qualified base name — routed through `packageClassPath`,
   which swaps a `/vN` tail for the Go package name.
3. **Cross-package reference metadata** — `PackageInfo.RootPackageName` (`importOperations.go`) is the
   code-facing qualifier that keys imported-alias loading and the foreign-implement records that cast
   sites reference (`GoImplement<…rand_package.PCG, …rand_package.Source>(Pointer = true)`). It was the
   path's last segment (`v2`); `rootPackageNameFromPathParts` now returns the parent segment for a
   `/vN` tail. `PackageName` stays path-formed — it also names the referenced `.csproj`, which IS
   `math.rand.v2.csproj`.
4. **Imported type-alias TARGET class** — `loadImportedTypeAliases` (`importOperations.go`) qualifies
   an imported alias's target as `go.<PackageName>_package.<Type>`; the class path is `PackageName`
   with its final segment replaced by `RootPackageName`, so a `/vN` producer's exported aliases
   resolve to `rand_package`, not `v2_package`.

The convention is that a package literally named `vN` would instead need the type-graph name; the
stdlib has none, so the regex/parent-segment rule holds corpus-wide. Guarded by the `VersionedImport`
behavioral test — a `main` importing a sibling `vlib/v2` module (`package vlib`) that mirrors
math/rand/v2's shape: a struct field `ж<vlib.Rand>` (renderer #2), a `*PCG → Source` pointer cast
recorded as `go.vlib.vlib_package.PCG` (#3/#4), output-compared vs `go run` across all four phases.
This is what unblocks sort as Phase 4's second validated package (its test suite imports math/rand/v2).

## A C# keyword inside a dotted import-path element
A Go import-path element may itself contain **dots** — a module host (`gopkg.in`, `example.com`,
`golang.org`) or a versioned tail (`yaml.v3`) — and every one of those dots is a **namespace
separator** in the emitted C#. So `gopkg.in/yaml.v3` does not render two namespace levels, it renders
four, and each is a separate C# identifier that has to be keyword-escaped on its own.

Two sanitizers render namespace text, and until 2026-08-07 only one of them knew that. The
**declaration** side (`getProjectName` → `getCoreSanitizedIdentifier`) has always split an element on
its dots, so the dependency's own file correctly opens `namespace go.gopkg.@in;`. Every **consumer**
emission — the import's `using yaml = …;` alias, the enclosing-namespace `using gopkg.…;` an unaliased
import adds, the child-namespace map that decides root qualification, and the string-path type
renderer — goes through `convertImportPathToNamespace`, which sanitized each `/`-split element with
`getSanitizedImport`, measuring it **whole**. Whole, `gopkg.in` is not a C# keyword, so it passed
through bare and the importer emitted

```csharp
using yaml = gopkg.in.yaml_package;   // CS1001/CS1002/CS1022 — `in` is a keyword
using gopkg.in;
```

against a producer that had named itself `go.gopkg.@in`. The dependency compiled; nothing that
imported it could. (Issue #33: `gopkg.in/yaml.v3` converts, then does not build.)

The fix is one function, both sides: **`getSanitizedImport` splits on dots too**, escaping each level
independently — exactly what the declaration side does. The recursion stays inside `getSanitizedImport`
rather than deferring to `getCoreSanitizedIdentifier`, because callers append the `_package` class
suffix to the final element before calling and the core sanitizer `Δ`-prefixes anything ending in
`_package`; that swap would emit `Δyaml_package`, a class no producer declares. Escaping is idempotent
(an already-`@`-marked part returns unchanged), so re-sanitizing a rendered namespace is stable.

The behavior change is exactly "a dotted input with a keyword sub-token is now escaped": a string
containing a dot could never equal a keyword, so the old whole-string test never fired for one, and
hyphen/tilde replacement is per-part identical either way. Emission-neutral for both corpora, and
measured so — the behavioral corpus has no dotted module path at all, and the standard library's only
dotted element is `golang.org` (the GOROOT-vendored tree), whose `golang`/`org` are not keywords:
[CNR](../Glossary.md#cnr) byte-identical across 572 packages, and a seeded full reconvert byte-identical
across 5,179 `.cs`/`.csproj`/`README.md` plus the generated `go2cs-stdlib.slnx`.

Guarded at both altitudes: `TestGetSanitizedImportKeywordSegments` (`sanitization_test.go` — several
keywords in several positions, the two sanitizers asserted to agree on a segment, the `_package`
suffix asserted NOT to be `Δ`-prefixed, and idempotence) and `TestRecurseKeywordNamespaceSegment`
(`moduleConverter_integration_test.go` — network-free, an unaliased import of a `gopkg.in`-shaped
dependency, asserting the producer's declaration and both consumer emissions name the same namespace,
then sweeping every `using` in the file against the converter's own `keywords` set so a keyword the
fixture never exercises is covered by the same assertion). Both neuter-proven: restoring the
whole-string measurement reproduces the reporter's emitted text verbatim.

## The import-path rewrite rewrites only the PATH, not the constructor in front of it
The string-path type renderer (`convertToCSFullTypeName`) peels a Go type expression one constructor
at a time — `<-chan `, `chan `, `chan<- `, `*`, `[]`, `[N]`, `map[K]`, `func(…)` — recursing on what
is left. Its **import-path rewrite runs first**, before any of those branches, because a
package-qualified element has to become a C# namespace before the name means anything. Until
2026-08-08 that rewrite measured the path from index 0 of the whole string, constructor included.

`convertImportPathToNamespace` maps a **hyphen to an underscore**, because a Go path element may
legally contain one (`mongo-driver`, `go-isatty`). Handed the constructor as well, it rewrote the
`-` of `<-chan` too. The declaration in `go.mongodb.org/mongo-driver/x/mongo/driver/session`

```go
type Pool struct { descChan <-chan description.Topology }
```

renders fully-qualified as `<-chan go.mongodb.org/mongo_driver/mongo/description_package.Topology`,
and came back as `<_chan go.mongodb.org.mongo_driver.…`. No channel branch recognizes `<_chan`, so it
fell through to the **array** branch, which slices past the `>` that a `[N]` length closes — and with
no `>` in the string at all, `strings.Index` returned -1 and the slice was `typeName[0:]`. The
renderer re-entered on the IDENTICAL string, without bound: `fatal error: stack overflow`, taking a
1,726-package `-recurse` run down at package 1456 (issue #33's third report).

A **single-segment** path never had a slash to enter the rewrite, so `<-chan time_package.Time` was
always correct. That is the whole reason the standard library — which is nothing but single- and
multi-segment *stdlib* paths, none of them hyphenated behind a `<-chan` — never saw this, and only a
module dependency could.

The fix is `importPathStart`: find where the path begins by scanning **backward** from the candidate
region to the first byte no import path may contain, and rewrite only from there. `-` cannot be that
delimiter (it would split `mongo-driver` mid-path), but every constructor the renderer emits ends in
one that can — a space (`<-chan `, `chan `, `chan<- `), `*`, `]` (`[]`, `[2]`, `map[K]`) or `(`
(`func(`). `<-chan ` then survives for its own branch, which recurses on the bare qualified element
exactly as it always has, and `descChan` emits
`/*<-*/channel<go.mongodb.org.mongo_driver.mongo.description_package.Topology>` against the
`using description = global::go.go.mongodb.org.….description_package;` the same file writes.

Two subtleties the first cut got wrong, both caught by [CNR](../Glossary.md#cnr):

- **A non-ASCII byte is a path byte.** Every delimiter this scan looks for is a constructor
  character and all of them are ASCII, but the converter's own synthetic markers are not (`ᴛ`, `ж`,
  `Ꮡ`, `ꓸ`, `Δ`). Treating a multi-byte rune as a delimiter stranded the scan *inside* the type
  name, freezing the path in front of it: `go.main_package/entryᴛ1` for
  `go.main_package.entryᴛ1`. The `PublicizedInterfaceAnonAlias` and `NestedAliasUser` lifted-alias
  goldens are what surfaced it.
- **The scan stops at the generic-argument bracket.** Past it lie type ARGUMENTS whose `,`, space
  and `]` are not constructor text and would strand the scan at the tail of the string.

**Known residual.** When the OUTERMOST constructor is `[]`, its leading `[` is read as the start of a
generic argument list and truncates the path scan to nothing, so `[]<-chan <module path>.T` is still
mangled. Fixing it means no longer treating a *leading* `[` as a generic bracket, which re-routes
every `[]<pkg>/<sub>.T` in the corpus through the other branch (a `_package`-suffix change) — a
corpus-wide emission change that does not belong in a crash fix. It no longer crashes, which is the
part that mattered: see below.

**The crash-proofing is separate from the rendering fix, and is the part that generalizes.** The
array branch now requires the `>` it slices past. A Go stack overflow is a **fatal** runtime error,
not a panic, so the conversion driver's per-file `recover` could not contain it and one unrenderable
type killed the whole run instead of its own package. Bounded, an unrecognized shape is reported by
name on stderr (`Cannot render a C# type name for the unrecognized type expression "…"`) and the
package still converts. Every other branch consumes at least one byte before recursing, so bounding
this one bounds the renderer.

Guarded at both altitudes. `typeNameResolution_test.go` pins the renderer: `TestImportPathStart`
(each constructor, the marker runes, and the paths that must NOT move),
`TestConvertToCSFullTypeNameConstructedModulePaths` (the reported field in all three channel
directions, `*`/`[]`/`[N]`/`map[K]`/nested, plus the single-segment and bare-path cases that must stay
byte-identical, plus the residual above pinned as a decision), and `TestUnclosedBracketTerminates`,
which runs in a **child process** with a 4 MB stack because the condition it guards is unrecoverable
in-process. `TestRecurseChannelOfHyphenatedModulePath`
(`moduleConverter_integration_test.go`) pins that a real declaration of the reported shape reaches
that renderer through an actual `-recurse`, over a network-free fixture whose module path mirrors the
report's — hyphenated first segment, multi-segment tail. Its fixture carries a type **alias** to the
channel alongside the struct field, deliberately: a field DECLARATION emits the readable file-local
alias (`description.Topology`), so the fully-qualified render is computed but never written and a
test reading only the field cannot tell a correct render from a mangled one — while an exported alias
writes the fully-qualified string verbatim into both `main.cs` and the `[GoTypeAlias]` record.

Neuter-proven three ways: both reverted reproduces the reported `fatal error: stack overflow`; the
bound alone reverted fails the child test in 0.02 s; the rewrite alone reverted (bound in place)
makes the converter print the warning, exit 0, and still write every `.cs` — the crash-proofing
demonstrated independently of the rendering — while the integration test fails on the emitted
`global using TopoChan = go.<_chan example.com.mongo_driver.…;`.

## A non-canonically-aliased import renders foreign types via the file's alias
A file that imports a package under an **explicit alias that differs from the canonical package
name** must render that package's types through the alias, not the canonical name. cryptobyte's
`asn1.go` imports `encoding/asn1` as `encoding_asn1` — because the sibling vendored subpackage
`.../cryptobyte/asn1` already claims the canonical `asn1` — so a `*asn1.BitString` parameter must
emit `ж<encoding_asn1.BitString>`. `getAliasQualifiedTypeName` had rendered the canonical `asn1.BitString`
(`importQualifier(pkg.Name())`), which the file's `using asn1 = …cryptobyte.asn1_package` resolves
to the *subpackage* (no `BitString`) — CS0426, and the RecvGenerator faithfully propagated the
wrong qualifier into its `.g.cs`. A `types.Type` carries no source alias, so a per-file
`importPathAliases` map (import path → the alias the file's `using` bound) is threaded into
`getAliasQualifiedTypeName`; a foreign type whose import path the file aliased renders through that alias. Only
**explicitly-aliased** imports populate the map — unaliased / blank / dot / Δ-collision-renamed
imports are absent and keep the `importQualifier(pkg.Name())` fallback, so nothing else changes
(value references were already correct — they come from the AST import name via `convIdent`; only
type references, sourced from `types.Type`, lost the alias). Cleared cryptobyte's CS0426 (which had
masked deeper `Builder.add`/`slice.Value` roots, now banked). GUARD OWED — the shape needs two
packages whose names collide so one import is forced non-canonical, not expressible in the
single-library behavioral corpus.

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

**An imported type ALIAS through an aliased import renders as its `global using` name.** A `global using` alias is not a member of the package class, so `import pl "PALib"` with `pl.B2{V: 1}`, where `B2` is an exported alias, cannot render `new pl.B2(…)` (CS0426). The alias table is keyed by the package's declared name, and `aliasResolvedSelector` looks a published, non-const type alias up under that name, rendering `new PALibꓸB2(…)` as the canonical import does. Every other member keeps the file's alias (`new pl.Box(…)`). (Guarded by the `AliasImport` behavioral test's `aliased.go`.)

---

[← Type Switch Statements](type-switch.md) · [Index](README.md) · [Struct Type Embedding →](struct-embedding.md)
