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
