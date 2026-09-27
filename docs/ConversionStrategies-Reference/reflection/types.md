# Reflection: Types

[Reference index](../README.md) · [Reflection](README.md) · [Summary of this topic](../../ConversionStrategies.md#reflection-reflect)

This page covers the type side of the [reflection bridge](README.md): how a `reflect.Type` answers names, method tables, fields, type relations and identity, and how `ArrayOf`, `StructOf` and `SliceOf` build types at run time.

## Names and strings

### `reflect.Type.Name()` — a DEFINED type HAS a name even when its underlying type is a composite

Go's rule is about DEFINEDNESS, not shape: `Name()` reports the type's name within its package for
any defined type and `""` only for a type that was never defined — `[]int`, `map[string]int`, `*T`,
`chan int`, `interface {}`, `struct { … }`. `type testSET []int` is defined, so its `Name()` is
`"testSET"` even though its underlying type is a slice.

The bridge had that backwards. Go's own `rtype.Name()` gates on the descriptor's `TFlagNamed` bit
(`abi.Type.HasName()`), which a **synthesized** `abi.Type` never carries, so the hand-owned
`rtype.Name()` substituted a shape test — `GoReflect.ElementType(st) is not null`, i.e. "does this
type have an element type?". That is true of a defined container exactly as it is of an unnamed one,
so every `type S []T` / `[N]T` / `map[K]V` / `chan T` / `*T` in the corpus reported no name at all.

The tell was already in the same descriptor: `PkgPath()` reads the SAME managed nesting and answered
`"main"` for those types while `Name()` answered `""` — a pair Go's own model cannot produce, since
a type with a package path is by definition a defined type.

The visible symptom was one byte. `encoding/asn1`'s `getUniversalType` distinguishes a SET from a
SEQUENCE on the type's name and nothing else:

```go
if strings.HasSuffix(t.Name(), "SET") {
    return false, TagSet, true, true
}
return false, TagSequence, true, true
```

so `Marshal(testSET([]int{10}))` produced `300302010a` where Go writes `310302010a` — `0x30`
SEQUENCE for `0x31` SET, with no error, no panic and no other divergence anywhere in the encoding.

The gate is now `GoReflect.HasGoName`, the managed stand-in for `TFlagNamed`. It mirrors
`GoTypeName` ARM FOR ARM, because `Name()` IS that method's output with the package qualifier
trimmed — the two disagreeing would let a type report a name it does not have, or hide one it does.
False for exactly the arms that render Go structurally: the raw golib containers matched by open
generic definition (`slice<>`/`array<>`/`map<,>`/`channel<>`/`ж<>`), `object` (`interface {}`),
`EmptyStruct` (`struct {}`), an anonymous-struct lift (`[GoType("dyn")]` without a `[GoLocalName]`,
which would make it a named function-local type), and the pointer-sourced adapter that stands for
`*T`. True everywhere else — including the predeclared scalars, since Go's `int` IS a named type.

The distinction the fix turns on is that a DEFINED container is not a golib container: the converter
emits it as its own wrapper type that merely IMPLEMENTS the container interface, which is why the
open-generic-definition test separates the two where an element-type probe cannot:

```go
type intSET []int
type byteArray [4]byte
type stringMap map[string]int
type intChan chan int
type intPtr *int
```
```csharp
[GoType("[]nint")] partial struct intSET;
[GoType("[4]byte")] partial struct byteArray;
[GoType("map[@string, nint]")] partial struct stringMap;
[GoType("chan nint")] partial struct intChan;
[GoType("ж<nint>")] partial class intPtr;
```

Three further answers change with it, all in the same direction and none of them a value Go can
produce: `interface {}`, `struct {}` and a lifted anonymous struct used to return their STRUCTURAL
spelling from `Name()` (there is no dot to trim, so the whole string came back) and now correctly
return `""`. `String()` was never affected — it has no such gate and rendered all of these correctly
throughout, which is why the defect stayed invisible to `%T` and to the `ReflectliteTypeName` guard.

(Guarded by the `ReflectStructTagCopy` behavioral test, which pairs each of the five named shapes
with its unnamed control and re-runs asn1's own `HasSuffix(Name(), "SET")` decision. Measured on
`encoding/asn1`'s converted suite: **37 of 38**, up from 35 — it closes `TestMarshal` #37 and also
`TestCertificate`, whose "sequence tag mismatch" and empty RDN name had been left unattributed on
the board and are the same root, since its `RDNSequence` is a `[]RelativeDistinguishedNameSET`.)

**The follow-on this section recorded is now CLOSED.** It read: `abi.Type.HasName()` itself is still
`false` for every synthesized descriptor, so `internal/reflectlite.rtype.Name()` — the ordinary
converted Go body, which gates on it — answers `""` for EVERY type; and populating the bit would
ALSO change `directlyAssignable`'s `T.HasName() && V.HasName()` short-circuit, "a corpus-wide
assignability change, not a naming one". That reading was exactly right, and the assignability
change is precisely why the bit was worth carrying. `synthesizeDescriptor` now stamps `TFlagNamed`
from the same `GoReflect.HasGoName` gate this section installed, so the descriptor bit and the name
a `Type` reports come from ONE predicate, and `reflectlite`'s `Name()` becomes truthful with it. See
[*Go's ASSIGNABILITY rule, and the identity walk underneath
it*](#gos-assignability-rule-and-the-identity-walk-underneath-it).

### The type NAME is a descriptor read too — `reflectlite`'s `rtype.String`

**The type NAME is a descriptor read too — `reflectlite`'s `rtype.String` (2026-08-02).** The same
class as the specialization reads above, at its quietest. Go's `rtype.String()` is
`t.nameOff(t.Str).Name()`: `Str` is a **name offset** into the linker-built name blob, resolved by
pointer arithmetic from the descriptor's own address. A synthesized descriptor has no blob and no
`Str`, so the mini-bridge's `String()` answered `""` for every type — and answered it **silently**,
because `""` is a legal name for an unnamed Go type, so nothing panicked and no read faulted; the
empty string simply propagated into whatever the caller was building. `context`'s `stringify`
fallback (the arm for a key type with no `String()` method) printed
`context.Background.WithValue(, c1k1)` where Go prints
`context.Background.WithValue(context_test.key1, c1k1)`. That is the failure mode worth recording:
a descriptor read that cannot be honored does not always throw — this one degraded to a
plausible-looking empty field, and only a differential caught it.

`reflect`'s own `rtype.String` has been hand-owned over `GoReflect.GoTypeName` since Phase 1; this
is the identical answer for the mini-bridge (`internal/reflectlite/type_impl.cs`, registered as
`manualConversionFuncs["internal/reflectlite"]["rtype.String"]`), so the full bridge and the mini
bridge cannot disagree about what a type is called. Array dims ride along exactly as on the
`reflect` side — a descriptor that knows its length renders Go's `[N]T` rather than `[]T`. The
managed nesting supplies the package qualifier: `key1` is declared in class
`context_test_package`, stamped `[GoPackage("context_test")]`, so `GoQualifiedName` recovers
`context_test.key1` — including for a `-tests` external test package, whose Go-visible package name
is the stamp's authority rather than the class name.

**Blast radius, measured rather than assumed:** within `reflectlite` only three call sites reach
`String()` — `stringify`/`contextName` (the fix), and the `assignTo` and `elem()` **panic
messages**, which merely become legible. No comparison logic consumes it: `rtype.Name` still
answers `""` for every type because it gates on `HasName()`, which reads the `TFlagNamed` bit that
`synthesizeDescriptor` never sets, and the `haveIdenticalType`/`directlyAssignable` chain that
would consume `Name()` is dead behind the hand-owned `Implements`/`AssignableTo`. `errors` — the
mini-bridge's other consumer — never reaches `String()` at all (`Comparable`/`Kind`/`Implements`/
`AssignableTo`/`Elem`/`Set`/`IsNil` only), and re-validates unchanged at 61/61. Demonstrated
consumer: `context`'s `TestValues`, 36/38 → **37/38** (the remaining failure is `TestAllocs`, the
measured alloc-count disclosure). `rtype.Name` is the recorded next gap of this shape, deliberately
NOT fixed without a consumer that demonstrates it.

### An UNNAMED func type renders STRUCTURALLY, exactly as an unnamed struct does

`GoReflect.TypeNaming` had no delegate handling at all, so a Go func value printed the CLR delegate
family standing in for it — `fmt`'s own `TestSprintf` reads it back:

```
(Action`1)(0x26d47ab)          // was
(func(*testing.T))(0xPTR)      // Go
```

A func type is named or unnamed by the same test every other type uses, and for converted code that
test is exact: a Go DEFINED func type is emitted as its own `delegate` nested in the declaring
`<pkg>_package` class (`http.HandlerFunc`), while an unnamed one lands on a BCL/golib delegate family
(`Action<ж<T>>`, `Func<…>`, the variadic `Funcꓸꓸꓸ`/`Actionꓸꓸꓸ`, or C#'s natural delegate type) that
no converted package declares. So `GoTypeName` renders the second group from `TryFuncShape` in Go's
own format — `func(` inputs `)`, then nothing / ` T` / ` (T, U)` for zero, one and several results,
with a variadic tail written `...T` — and `HasGoName` answers `false` for it, arm for arm, which is
what keeps `Name()` and the descriptor's `TFlagNamed` agreeing with the string.

**The variadic tail is recognized by SHAPE, not by the delegate family's name.** The golib families
carry a name marker and the detection used to rely on it, but a declared `func(string, ...int)` used
as a method group in an `any` position acquires C#'s NATURAL delegate type instead, whose name carries
no marker — so it reported non-variadic and `In(1)` handed back a raw `Span<int>`, rendering
``func(string, Span`1)``. A `Span<T>` parameter cannot arise any other way in converted code (Go has
no such type, and the converter emits one only for a variadic tail), so testing for it subsumes the
name test rather than widening it.

Residual, stated rather than hidden: a defined **methodless** func type has no managed identity — the
converter renders it inline as its base delegate family — so it is indistinguishable from an unnamed
one here and reports the unnamed answer. That is the same "describe the type the bridge can actually
build a descriptor for" rule `ChanDir` settles on. A defined func type carrying a method does get its
own delegate and keeps its name. Guarded by `ReflectBridgeClosure` (six unnamed shapes and one named,
against `go run`) and `GolibTests.GoReflectBridgeClosureTests`.

## Method tables

### The method COUNT is a descriptor read too — `rtype.NumMethod`

**The method COUNT is a descriptor read too — `rtype.NumMethod`, the gate on json's Unmarshaler
discovery (2026-08-02).** The same silent-degradation class as the NAME read above. Go's
`rtype.NumMethod` counts `uncommon()` method tables — trailing descriptor allocations the linker
lays out after the `abi.Type`, which a synthesized descriptor never populates — so it answered
**0 for every concrete type**, and answered it silently, because 0 is the correct count for most
types and nothing downstream faults on it. The consequence hid one hop away: `encoding/json`'s
`indirect()` only ATTEMPTS its `Unmarshaler`/`TextUnmarshaler` interface assert behind
`v.Type().NumMethod() > 0`, so no custom `UnmarshalJSON`/`UnmarshalText` was **ever** dispatched —
every `json.Unmarshal` into `time.Time` fell through to the raw-struct path and died with
`json: cannot unmarshal string into Go value of type time.Time` (time's `TestTimeJSON`; in
`TestUnmarshalInvalidTimes` the miss inverted the failure — `{}` decoded *silently* where
`Time.UnmarshalJSON` rejects it). The marshal side never had the problem: `newTypeEncoder` gates on
`Implements`, hand-owned since the type-relation increment.

Severed at the same semantic boundary as every read of this class: the hand-owned `rtype.NumMethod`
(`reflect/value_impl.cs`) answers over golib `GoReflect.GoMethodCount` →
`TypeExtensions.GoMethodSetCount`, which counts over `GetGoMethodSetCandidates` — the **same
candidate source** the structural probe (`StructurallyImplements`) and the duck-typing shell binder
resolve through, so the NumMethod gate and the interface assert behind it can never disagree about
a method set (a count from any other source could answer 0 for a set the assert would bind, and the
gate would silently re-skip the dispatch this fixes). Candidates are deduplicated by **projected Go
name** — one Go pointer-receiver method reaches the registry in two emitted shapes (the
RecvGenerator's `ж<X>` overload and the original `[GoRecv]` `this ref X` extension) — and
exported-ness is judged Go's way (first rune uppercase) on the projection, after the same leading
collision-marker strip `GoMethodNameMatches` applies. Go's kind split is preserved: an interface
type counts ALL its methods (`GetInterfaceMethodNames`, instance members only — the golib static
`As<T>` helpers stay invisible), the empty interface (`object`) counts 0, a concrete type counts
exported only, with `ж<X>` seeing X's value- AND pointer-receiver methods and a plain X only the
value-receiver ones; an adapter shell answers as the Go dynamic type it stands for, mirroring the
`KindOf`/`ElementType` unwrap (R10). The count is memoized per (element, pointer-ness) and cleared
on assembly load with the candidate cache that feeds it.

One root, four symptom shapes — the guard (`tests/Behavioral/JsonUnmarshalerDispatch`) locks all
four against `go run`: unmarshal into `&t` directly (the Pointer-kind gate), whole-value dispatch
of a non-string JSON value (the error path), a user-declared named type with a pointer-receiver
`UnmarshalJSON` (dispatch is not stdlib-specific), and unmarshal into struct FIELDS (the
`Name() != "" && CanAddr()` → `Addr()` route through the field-alias box). Demonstrated consumer:
time's `TestTimeJSON` and `TestUnmarshalInvalidTimes`. `rtype.Method(i)` stays auto and still reads
the same absent tables — the recorded next gap of this shape: a `NumMethod() > 0` gate now lets a
method-ENUMERATION loop (`for i := range t.NumMethod() { t.Method(i) }`) get further than before,
and the first consumer that walks one demonstrates it.

### The count and the WALK are ONE increment — `Type.Method(i)`, `Value.Method(i)`, `MethodByName`

**…and the count and the WALK are ONE increment — `Type.Method(i)`, `Value.Method(i)`,
`MethodByName` (2026-08-03).** The paragraph above shipped alone and was **reverted**: the
recorded successor gap arrived one session later, on the very next all-package sweep. `math/rand`
and `math/rand/v2`'s `TestRegress` enumerate every generator method — `typ.Method(i).Name`,
`rv.Method(i)`, `mv.Type()`'s `NumIn`/`In`, `mv.Call(args)` — against a golden output table, and
both went from validated to `panic: reflect: Method index out of range`. The lesson is general and
worth stating as a rule: **a truthful count is a PROMISE that the table behind it can be indexed.**
While `NumMethod` answered 0 the enumeration loops were unreachable and their auto `Method(i)`
(which reads the same absent `uncommon()` tables) could not be observed; making the count truthful
is exactly what made them reachable. A descriptor read and the gate in front of it belong in one
increment.

The whole table is now ONE list — `TypeExtensions.GetGoMethodSetEntries` — and `NumMethod` is its
`.Count`, so a size and an order can no longer be derived separately and disagree. It is built over
`GetGoMethodSetCandidates`, the same candidate source `StructurallyImplements` and `AdapterBinder`
resolve through, then: deduplicated by projected Go name (keeping the shape a delegate can bind —
a `[GoRecv] this ref X` receiver cannot be a `Func<>` parameter, and the RecvGenerator's `ж<X>`
overload always sits beside it), exported-only for a concrete type, and **sorted ORDINALLY by Go
method name**, which is Go's own method-table order (verified against `go run`: a promoted embedded
method sorts *in place*, it is not appended).

A **method value is an ordinary bound delegate**, and that is the design's whole economy. Go
carries `v.Method(i)` as the receiver's own Value plus a `flagMethod` bit with the index packed
into the flag, then rebuilds the signature (`typeSlow`) and re-resolves the receiver
(`methodReceiver`) on every use — all descriptor reads. The bridge instead BINDS the receiver into
a managed delegate at `Method(i)` time, so the result is a Kind-Func Value and everything
downstream is reuse rather than new surface: `mv.Type()` is the ordinary canonical Type of a
delegate, `NumIn`/`In`/`NumOut`/`Out` are the existing `TryFuncShape` readers, and `mv.Call(args)`
is the existing `Value.Call` **unchanged** — with the receiver already absent from the signature,
which is precisely Go's method-value contract. `Type.Method(i).Func` is the same delegate UNBOUND
(receiver first), Go's contract for the type side, and `Value.MethodByName` needs no hand-own at
all: it composes the hand-owned `rtype.MethodByName` and `Value.Method`.

Binding is **expression-compiled, and that is not a preference**: `Delegate.CreateDelegate(type,
firstArgument, method)` cannot close over a VALUE-type first argument (measured — `ArgumentException`)
and Go value receivers are by-value structs, so the closed-delegate form fails for every
value-receiver method, `time.Time`'s entire method set included. Each `MethodInfo` is compiled once
into a `Func<object?, Delegate>` factory (a nested lambda — the outer takes the receiver, the inner
IS the bound Go-signature delegate), so a bind costs a closure allocation, not a compile. A
value-receiver method reached through `*X` is handed a COPY of the pointee, Go's rule.

**A `this object` extension method is golib plumbing, never a Go method** — and filtering it is a
correctness fix, not tidiness. `GetGoMethodSetCandidates`' assignability safety net (there for
promotion and base relationships) is satisfied by EVERY type when the receiver is `object`, so
`TypeExtensions.TryCastAsInteger(this object, out ulong)` was entering every type's method table —
and entering it **nondeterministically**, because the candidate scan is redone whenever a late
assembly load clears the caches: the same binary reported `NumMethod` **4 or 6 for the same type**
depending only on which assemblies had loaded by the first read (it reproduced under the behavioral
runner's redirected stdout and not under a console). That also means the shipped-then-reverted
count was wrong in a way nothing could observe. It is filtered in the method-TABLE builder rather
than in the shared candidate source, whose admission rule the duck-typing assert and the shell
binder also read: a Go METHOD SET is a stricter question than "could this extension method dispatch
on this value?".

Guard: `tests/Behavioral/ReflectMethodTableWalk` locks the surface against `go run` — count/walk
agreement and sorted order, a value type's set excluding pointer-receiver methods, a bound
pointer-receiver call MUTATING the receiver across calls, a value-receiver method bound through a
pointer, a no-result method, `MethodByName` round-tripping to the same index (and both absent-name
forms), the unbound `Method.Func` called receiver-first, promoted-embed ordering, and an interface
table whose method value dispatches to the dynamic value. Demonstrated consumers: `math/rand`
**43/43** and `math/rand/v2` **36/36** re-validate at their exact banked counts with `TestRegress`
now genuinely walking (the converted bridge reports `*rand.Rand NumMethod: 16` in Go's order and
`Intn(1000000000) = 526058514`, matching `go run` — where before the pair it reported 0 and the
test passed VACUOUSLY, executing none of its 320 golden comparisons); `time` goes 146 → **148**
pass of 159 as the two increment-6 JSON rows re-land. Recorded next gaps of this shape: `MakeFunc`,
variadic `Call`/`CallSlice`, and `reflectlite`'s `rtype.Name`.

## Fields and tags

### `StructField.Tag` is a REAL read — the converter has always emitted the tag, nothing had ever read it

The converter emits a tagged field's Go struct tag verbatim at the declaration:

```go
NamedCurveOID asn1.ObjectIdentifier `asn1:"optional,explicit,tag:0"`
```
```csharp
[GoTag(@"asn1:""optional,explicit,tag:0""")]
public asn1.ObjectIdentifier NamedCurveOID;
```

`GoTagAttribute` aliases `System.ComponentModel.DescriptionAttribute`, so the text survives into
metadata. It has done so since tags were first emitted — and until 2026-08-09 **nothing in the
corpus read it**: golib's Go-field projection (`GoReflect.GoFieldInfo`) carried no tag, and the
reflection bridge's `rtype.Field` left `StructField.Tag` at its zero value. Every converted
struct therefore reported as UNTAGGED, and every tag-driven decoder in the standard library —
`encoding/json`, `encoding/xml`, `encoding/asn1` — saw a type with no tags at all.

The failure that surfaced it is subtle rather than loud, which is the point: `encoding/asn1`
omits an `optional` field whose value `DeepEqual`s its zero, so with the tag invisible
`crypto/x509`'s `marshalECPrivateKeyWithOID(k, nil)` MARSHALLED the nil `NamedCurveOID` instead
of omitting it, and `makeObjectIdentifier` rejected the empty arc list — `asn1: structure
error: invalid object identifier`, the whole of `crypto/ecdsa`'s `TestEqual`. Nothing about the
message points at reflection.

`GoFieldInfo` gains `Tag`, read from the declaration's attribute (the promoted-embed arm reads
it off the backing box field, since Go allows a tag on an embedded field); `rtype.Field`
surfaces it as `StructTag`. `Offset`, `PkgPath` and `Anonymous` stay **unpopulated**: no
truthful read backs them here, and a descriptor field whose read cannot be honored must not be
made to look truthful. (Guarded by the `ReflectStructTagCopy` behavioral test — raw tag text,
`Tag.Get` for two keys, `Tag.Lookup`'s absent-vs-empty distinction, and untagged fields
answering `""`, output-compared vs Go.)

### EXPORTEDNESS is a descriptor read too — `StructField.PkgPath`

**EXPORTEDNESS is a descriptor read too, and the value side had been right about it all along —
`StructField.PkgPath` (2026-08-11).** `reflect.StructField.IsExported()` is nothing but
`f.PkgPath == ""`, and Go fills `PkgPath` with the declaring package's import path for an
unexported field (`type.go`: `if !p.Name.IsExported() { f.PkgPath = t.PkgPath.Name() }`). The
hand-owned `rtype.Field(i)` left it unset — the field's own comment said so, on the reading that no
truthful read backed it — so `IsExported()` answered **true for every field of every converted
struct**. Silent, like every member of this family: `""` is the correct `PkgPath` for the exported
fields that are most fields, so nothing faulted and nothing looked wrong.

The consequence is a **guard that can never fire**. `encoding/asn1` opens both its struct arms —
`parseField` and `makeField` — with

```go
for i := 0; i < structType.NumField(); i++ {
    if !structType.Field(i).IsExported() {
        return StructuralError{"struct contains unexported fields"}
    }
}
```

so `Marshal(unexported{X: 5, y: 1})` returned a **nil** error where Go returns that structural
error, and `Unmarshal` ran straight past the refusal into `parseField(val.Field(i), …)` on the
unexported field, where `SetInt` reached `mustBeAssignable` and panicked
(`TestUnexportedStructField`). Note what that panic proves: the two halves of the read-only model
had degraded **independently**. `Value.Field` already stamped `flagStickyRO` for an unexported
field — from `GoReflect.GoFields`, the same projection the type side walks — so `CanSet()`/
`CanInterface()` were correct and the write was correctly refused; it was only the TYPE-side
descriptor that had no answer, which is why a package that *probes* settability got no warning
while a package that simply *writes* got a clean Go-shaped panic. `PkgPath` now derives from the
same projection's `Exported` bit plus `GoReflect.GoPackagePath` (the package identity the managed
nesting carries, already `rtype.PkgPath`'s source), so a probe of the type and a write through the
value cannot disagree about a field.

Two neighbouring `StructField` members stay unpopulated, for two different reasons worth keeping
apart. **`Offset`** is the r39d rule — a descriptor field whose read cannot be honored must not be
populated to look truthful: a Go byte offset exists to be added to a data pointer, and managed
storage has no such pointer. (`abi.StructType` does populate `Offset`, and correctly: its consumers
— `unique`'s clone sequencer, `internal/reflectlite` — read it as layout metadata, never as an
address to walk.) **`Anonymous`** is the opposite case: it IS knowable, since an embedded field
arrives through golib's promoted-embed box hop, but no measured consumer demands it and the
recorded next gap of that shape is larger — go2cs-gen emits the promoted-embed backing box AFTER
the declared fields, so `struct{X; y; Inner; inner; Ptr}` walks as `X, y, Ptr, Inner, inner` here
where Go walks it in declaration order. Field ORDER and `Anonymous` want one increment together,
with a consumer that demonstrates them.

Guard: `tests/Behavioral/ReflectUnexportedFieldFlags`, byte-identical to `go run` — the indexed
walk's `IsExported`/`PkgPath`/`Tag`, `FieldByName` carrying the same flags (including the blank
field, which Go also reports unexported), a field-for-field assertion that the type side and the
value side AGREE (`v.Field(i).CanSet() == t.Field(i).IsExported()`), and the consumer shape itself:
a decoder that probes before writing must be able to refuse with a returned error rather than a
panic. Demonstrated consumer: `encoding/asn1`'s `TestUnexportedStructField`.

### An EMBEDDED field is an embed to `reflect`, tag included — and Go's whole embedding contract reads it

`reflect.StructField.Anonymous` is not a cosmetic flag. It is the signal every Go encoder uses to
decide whether a field's own fields are *flattened* into the enclosing object: `encoding/json`'s
`typeFields`, and the same walk in `encoding/xml`, `encoding/gob` and `text/template`, treat a field
as an embed exactly when `Anonymous` is set and no name tag overrides it.

The bridge left it unpopulated, on the recorded ground that no consumer demanded it. One does.
Reported `false`, every embed became an ORDINARY field named after its type:

```
{"S1":{"X":2},"S2":{"X":4}}      // where Go writes {}
{"S":"B","BugA":{"S":"A"}}       // where Go writes {"S":"B"}
json: unknown field "Level1b"    // where Go reports "extra"
```

It is a real READ, not a reconstruction: the converter emits an embed as a partial property over a
marker-prefixed backing box, and golib's field projection records that shape as
`GoFieldInfo.Embedded` — the same flag `reflect`'s struct-identity walk already compares (Go's
`haveIdenticalUnderlyingType` ends each field with `tf.Embedded() != vf.Embedded()`).

**An embed's TAG lives at a different declaration site, and that is the second half.** The converter
stamps `[GoTag]` on the emitted partial PROPERTY:

```csharp
[GoTag(@"json:""e,omitempty""")]
public partial ref ж<Embed0b> Embed0b { get; }
```

while `go2cs-gen` mints the backing field the property returns a ref to — *without* carrying the
attribute across:

```csharp
private global::go.ж<…Embed0b> ʗEmbed0b;    // no [GoTag]
```

The projection reads FIELDS, so every embedded field came back untagged — silently, because `""` is
the right answer for most embeds. `GoReflect`'s embed arm now asks the PROPERTY first (the
declaration) and keeps the field as a fallback, so a tagged embed reports its tag with no generator
change: `encoding/json`'s `TestMarshalEmbeds` emitted `"Embed0b":{…}` where Go emits `"e":{…}`, and
marshalled the `json:"-"` embed it must omit entirely.

**Recorded, not fixed: field ORDER.** `go2cs-gen` emits the promoted-embed backing field in a
generated partial, i.e. AFTER the declaring part's plain fields, so `Host{X; y; Inner; inner; Ptr}`
projects as `X, y, Ptr, Inner, inner` where Go walks it in declaration order. No measured consumer
observes it — `encoding/json`'s dominance rules are decided by DEPTH and tag, never by declaration
order, and its one order-sensitive test (`TestMarshalEmbeds`) declares its single plain field first,
so the two orders coincide. The shape that will expose it is a struct interleaving plain and embedded
fields whose *key order* is compared, and the remedy is declaration-order cargo (the way array dims
are carried), not a re-sort in the projection. Guarded by the `ReflectBridgeClosure` behavioral test,
which looks its fields up BY NAME for exactly this reason — a guard that asserted the current order
would pin the gap as a contract.

## Type relations and identity

### The reflect TYPE-RELATION mirrors + Convert

**The reflect TYPE-RELATION mirrors + Convert (Phase-3 continuation, 2026-07-26).** Go's descriptor
model reaches its type relations by **descriptor specialization**: when `Kind() == Interface` the
`*abi.Type` IS an `interfaceType` allocation, so `implements()` does
`Reinterpret<abi.Type, interfaceType>` and walks `.Methods`; `ptrTo` builds a `ptrType` prototype
through an eface reinterpret; `FieldByName` reinterprets to `structType` and walks `.Fields`. Behind
a **synthesized** descriptor none of that layout exists — the reinterpret produces a struct whose
promoted-embed box is default, and the first read throws from `ж.ValueSlot` ("Cannot get reference
to value…", the encoding/gob type-initializer crash). Reinterpret-specialization is therefore a
class of descriptor reads that can never be honored behind the bridge; each surface severs at its
semantic boundary onto the SAME golib machinery emitted asserts use: `rtype.Implements` /
`rtype.AssignableTo` over `GoReflect.GoImplements` (mirroring the reflectlite increment-1 forms),
`PointerTo` synthesizing the managed `ж<T>` pointer type (canonical via `toType`),
`rtype.FieldByName` over the shared `GoFields` projection (top-level names; the embedded-field
depth search is deferred with a named consumer — a promoted name answers Go's not-found path),
and `Value.Convert` over `GoReflect.TryConvertTo` — THE convertibility relation (the recorded
R-13 remedy), severing the `cvtInt → makeInt → unsafe_New` stub chain (R-14; internal/fmtsort's
package-level `ct()` table). `Value.Cap`/`Value.SetLen` join over the golib container interfaces
(gob's `decodeSlice` probes `Cap() < n` then re-lengths the header — SetLen writes the re-windowed
slice back through the aliased box), and the hand-owned `rtype.Field` stamps the single-hop
`StructField.Index` — an empty Index made the auto `FieldByIndex` return the struct ITSELF, so
gob's `encodeStruct` walked every wireType field as the whole struct. Demonstrated consumers:
encoding/gob's init + Encoder/Decoder engines (a struct round-trips end-to-end), go/token's
`TestSerialization` (FileSet through gob, 31/31), internal/fmtsort (3/3). Registered in
`manualConversionFuncs["reflect"]`; the banked fmtsort/go-token suites are the operational guards.

### Go's ASSIGNABILITY rule, and the identity walk underneath it

`reflect.Type.AssignableTo` was hand-owned as **identity on the carried `System.Type`, or
interface-implements**. Distinct Go types are distinct managed types, so identity gets named-type
distinctness for free — but it is strictly narrower than Go's rule, which also admits a value whose
type has the same UNDERLYING type as the destination provided **at least one of the two is not a
defined type**:

```go
type userDefinedBytes []byte
var u userDefinedBytes
u = []byte{1, 2, 3}   // legal: []byte is undefined, userDefinedBytes is defined
```

`database/sql`'s `TestUserDefinedBytes` is the measured consumer, and its symptom is a data bug
rather than an error. `convertAssignRows` tries two arms in order:

```csharp
if (sv.IsValid() && sv.Type().AssignableTo(dv.Type())) {
    case slice<byte> b: dv.Set(reflect.ValueOf(bytes.Clone(b)));   // arm 1 — CLONES
}
if (dv.Kind() == sv.Kind() && sv.Type().ConvertibleTo(dv.Type())) {
    dv.Set(sv.Convert(dv.Type()));                                 // arm 2 — SHARES the array
}
```

Go takes arm 1 and copies. The identity rule rejected the pair, so the converted run fell through to
arm 2 and handed the caller a **view over the driver's own array** — the test's own words, "got
potentially dirty driver memory".

**The hand-own is retired.** Go's body runs: `directlyAssignable(uu.t, t.t) || implements(uu.t, t.t)`.
It could not run before, because three things it stands on had no answer — and each had to land in
the same change, or the fix would have traded one wrong answer for a wider one.

**(1) `HasName()` had to become truthful FIRST.** `directlyAssignable`'s first gate is
`if T.HasName() && V.HasName() || T.Kind() != V.Kind() { return false }`. With the bit never set,
that gate passed for every pair — so retiring the hand-own without it would have called two DISTINCT
defined types over one underlying type assignable, which Go rejects:

```go
type myBytes []byte
type myOtherBytes []byte
// Go: myBytes is NOT assignable to myOtherBytes — both are defined.
```

**(2) `implements` — the FREE function — had to be bridged, not just the method.** `rtype.Implements`
was already hand-owned over `GoReflect.GoImplements`, but Go's own `directlyAssignable`,
`AssignableTo`, `convertOp` and `Value.assignTo` all route through the free `implements(T, V)`, whose
auto form reinterprets the descriptor as an `interfaceType` and reads `.Methods` off a promoted-embed
box that is default behind a synthesized descriptor — it **throws** for any non-empty interface. It
now answers from the same `GoReflect.GoImplements` probe the emitted `_<T>` asserts use, so a method
set cannot be answered one way by a type assertion and another by reflection.

**(3) `haveIdenticalUnderlyingType`'s downcast arms had to be fixed WITH it.** This is THE seat of
Go's type-identity relation — `ConvertibleTo` reaches it through `convertOp`, `AssignableTo` through
`directlyAssignable`. Five of its eight arms already worked: the scalar arm needs nothing, and
Array/Map/Pointer/Slice recurse through the `Elem()`/`Key()`/`Len()` that `internal/abi` synthesizes
(previous section). The **struct**, **func** and **interface** arms reached their operands by the
prefix-downcast idiom instead — and they did not fail loudly. They read ZERO of everything and
returned **true**:

| Arm | What it read | What it therefore reported |
|---|---|---|
| Struct | `len(structType.Fields)` → 0 for both operands, so the field loop never ran | any two structs IDENTICAL — including different field TYPES, a renamed field, and a different field COUNT |
| Func | `funcType.InCount`/`OutCount` → 0 for both | any two func types IDENTICAL |
| Interface | `len(interfaceType.Methods)` → 0 for both, which is Go's own "both empty ⇒ identical" | any two interface types IDENTICAL |

A false positive in an identity relation is the most dangerous shape this bridge produces, because
every caller reads it as permission. It was already live through `ConvertibleTo`; routing
`AssignableTo` through the same walk would have widened it to assignment.

All three arms are now bridged in `reflect`'s `value_impl.cs`, and the struct arm sits at the
**`reflect`** level on purpose rather than in `internal/abi`. abi's synthesized `StructType()`
deliberately leaves `StructField.Name` the zero `ΔName` — a `ΔName` is a pointer into the linker's
name blob and every reader walks it with raw-address arithmetic — so the field NAMES and TAGS Go's
identity walk compares are not there to be had one layer down. `reflect` already owns the named-field
projection (`rtype.Field`, over `GoReflect.GoFields`), and the walk reads that SAME projection, so
the fields a type hands out and the fields its identity is decided by cannot disagree. Every clause
Go compares is compared: field count, the struct's `PkgPath` (set when the struct holds an unexported
field), and per field the name, the type, the **tag when `cmpTags`** — the single place assignability
and convertibility diverge — the offset, and **embeddedness**, for which `GoReflect.GoFieldInfo`
gained an `Embedded` flag, since `struct{T}` and `struct{T T}` agree on everything else (an embed's
Go field name IS its type name).

Two residuals are stated rather than hidden. The **interface** arm proves "methodless" only for
`object` (Go's `any`), so a DEFINED empty interface with a managed type of its own is answered *not
identical* — the conservative direction, since a false negative degrades a caller to "this needs a
conversion" while a false positive hands it a silent wrong assignment. And a **defined methodless
func type has no managed identity at all**: the converter renders it inline as its base delegate, so
`type myFunc func(int) bool` and `func(int) bool` are one managed type, and the named/unnamed pairs
every other kind can assert cannot be produced for funcs.

### `abi.Type`'s SPECIALIZATIONS are synthesized, not downcast — `StructType()` / `ArrayType()`

Go's `(*structType)(unsafe.Pointer(t))` is the **prefix-downcast** idiom: the linker really allocated a
`structType` and handed out a pointer to its embedded `Type` header, so casting back reaches the
sub-record. The section on `Reinterpret` above names this as the one case the managed arm deliberately
does not cover — nothing sits behind a `ж<abi.Type>` but an `abi.Type` — and these are the two sites
where converted code took that cast anyway.

The failure is not the contained wrong read the address route usually gives. `Reinterpret` correctly
**refuses** to alias managed storage for a reference-bearing pair (aliasing would fabricate object
references), so it fell through to the raw address and read `ΔStructType`'s fields out of the memory
that follows the descriptor's value slot. Probed on `abi.TypeFor[testStringStruct]()`:

```
Fields.Length   8830452760576     <- an address fragment read as a slice length
Fields.Capacity 16                <- the descriptor's OWN Size_, bleeding through the shifted view
```

`m_array` happened to land on a real heap object, so indexing it threw `IndexOutOfRangeException`
rather than access-violating — a caught CLR type-safety break. That is **six of `unique`'s nineteen
rows**, thrown on the first iteration of `unique.buildStructCloneSeq`, and `internal/reflectlite`'s
`NumField`/`Len` read the same garbage.

Both specializations are therefore hand-owned in `internal/abi/type_impl.cs` (registered in
`manualConversionFuncs` as `Type.StructType` / `Type.ArrayType`, so the converter emits a placeholder
comment for the Go bodies) and **synthesized from the descriptor's carried `System.Type`**, exactly as
the descriptor itself is:

| Field | Answer |
|---|---|
| `StructType.Fields[i].Typ` | `synthType` of the projected Go field type, dims-stamped from the declaring zero instance |
| `StructType.Fields[i].Offset` | the field's **Go** (amd64) byte offset |
| `ArrayType.Len` / `.Elem` / `.Slice` | the descriptor's carried array dims, the element descriptor, and `[]T`'s |

The offsets are Go's numbers, not the CLR's — a Go `string` is 16 bytes where `@string` is an 8-byte
reference — and they come from the SAME walk that stamps a descriptor's `Size_` and `Align_`
(`GoReflect.GoFieldOffsets`, `GoSizeOf` and `GoAlignOf` all read one memoized `structLayoutOf` pass),
so a field's `Offset`, its struct's `Size_` and its `Align_` cannot disagree. `unique`'s `cloneSeq` values are the demonstrated consumer:
`struct{ z float64; b string }` → offsets `[8]`, `[2]struct{ a string }` → `[0 16]`, `[3]string` →
`[0 16 32]`, each matching `go test` exactly.

Two things are deliberately **not** invented, following the r39d rule that a descriptor field whose
read cannot be honored must not be populated to look truthful. A descriptor with no `System.Type`, or
a struct holding a field whose Go size is unknowable (one unknown size makes every later offset a
guess), answers Go's **nil** — which every Go caller already tests. And `StructField.Name` /
`StructType.PkgPath` stay the zero `ΔName`: a `ΔName` is a pointer into the linker's name blob and
every reader of one walks it with `addChecked` raw-address arithmetic, the same route that produced
the garbage above, whereas Go's own `ΔName.Name()` answers `""` for a nil `Bytes` — so the zero value
is a state the format *defines* rather than a fabrication. A named field descriptor already comes
from `reflect`'s hand-owned `rtype.Field` over `GoReflect.GoFields`, and no converted caller of
`abi.StructType` reads a field name.

**`Elem()` and `Key()` are the same idiom one level in, and they are synthesized too.** `Elem()`
downcasts the header to the `sliceType`/`arrayType`/`chanType`/`mapType`/`ptrType` behind it and reads
that record's `Elem` field; `Key()` does it for a `mapType`. They inherited the defect exactly, and
answered **nil** for every slice, array, chan, map and pointer descriptor in the corpus.

Nil is what made this one *fatal* rather than merely wrong. The specializations above return nil for
an unknowable layout and every Go caller tests it — but nothing tests `Elem()`. `reflect`'s
`haveIdenticalType` recurses straight into `nameFor(t)`, which reads the descriptor's carried
`System.Type` and nil-dereferences, so the whole of `ConvertibleTo`/`AssignableTo` died for any
operand that was not a scalar. Both are now hand-owned (`Type.Elem` / `Type.Key` in
`manualConversionFuncs`) over `GoReflect.ElementType` / `KeyType` — the **same** golib resolution
`reflect`'s own hand-owned `rtype.Elem` / `rtype.Key` use one layer up, so the descriptor layer and
the `reflect` layer cannot disagree about what an element type is. The dims cargo threads by the same
rule `rtype.Elem` applies: an array descriptor's element takes the **tail** of `[outer]…[inner]`,
while a pointer's dims are the pointee's already and pass through unshifted. Kinds with no element
still answer Go's nil, which is Go's own answer for them.

Same defect class, **still open** and deliberately not chased here: `MapType()`, `FuncType()`,
`InterfaceType()` and the free `Len()` reinterpret the same way — each awaiting a measured consumer,
since a synthesized `ΔMapType` would have to populate runtime-map fields (`Hasher`, `KeySize`, the
indirect-key/elem flags) that have no managed answer at all.

Guarded by `GolibTests.GoStructLayoutTests` (Go offsets and sizes for the exact shapes `unique`'s
`TestMakeCloneSeq` exercises, plus alignment padding — removing the per-field alignment rounding fails
`FieldOffsets_ApplyGoAlignmentPadding`), and measured by `unique`'s own suite: **1 → 4 of 19 matched**,
with all six `IndexOutOfRangeException` rows gone and the three `TestHandle` ones moved on to the
`internal/weak` linkname root behind them. `Elem()`/`Key()` are guarded by the
`ReflectConvertAssignable` behavioral test — `ConvertibleTo`/`AssignableTo` across all six
element-bearing kinds, named and unnamed, compared line for line against `go run` — with the golib
resolution they stand on pinned by `GolibTests.PointerNilPredicateTests`'
`ElementAndKeyResolveForEveryKindTheDescriptorMustServe`.

### A managed REFERENCE is a Go pointer, not a Go struct — the reflection bridge's descent rule

`GoReflect.KindOf` classifies a managed `Type` to a Go `reflect.Kind`, and exactly one of those kinds
means *look inside*: `Struct`. `GoSizeOf`, `GoAlignOf` and `IsComparable` all stop at every other
kind, because that is Go's own layout rule — a pointer, slice, map, chan, interface or func field is
a fixed-size header (8/24/8/8/16/8 bytes on amd64) whatever it refers to, and only struct and array
fields recurse. Go can afford that rule because a Go struct containing itself by value is a type Go
itself rejects, so the descent is finite by the source language's own definition.

`KindOf`'s fallback answered `Struct` for any managed **reference** type it did not otherwise
recognize, and that broke the rule in the one direction that matters. The converter emits every Go
struct as a C# **value** type — the entire converted corpus carries seven `[GoType] partial class`
declarations and all seven are named-POINTER types (`type P *T`), classified `Pointer` structurally
before the fallback is reached — so a reference type arriving there is never a Go struct at all. It
is an opaque managed handle: the backing object a hand-owned shim holds in place of Go's own
representation. Answering `Struct` sent the walks into the CLR's own private fields and from there
into the BCL object graph, which has no rule against cycles:

```
Named -> Mutex -> SemaphoreSlim -> TaskNode -> TaskNode -> ...
```

`sync.Mutex` is the corpus entry point — hand-owned on a lazily-created `SemaphoreSlim` because Go's
runtime sleeping semaphore has no managed form — and `SemaphoreSlim`'s async wait queue is a linked
list, so `TaskNode.Next` is a `TaskNode` and the descent never ends. `go/types`' `TestSizeof` asks
`reflect` for the size of `Named`, which holds a `sync.Mutex`, and the run died there with a
`StackOverflowException`: uncatchable, so it took the whole process and reported the 44 tests
alphabetically after it as **absent** verdicts rather than as one failure.

The rule is therefore stated positively: **a managed reference is one pointer word wide and is never
descended into.** `KindOf` reports `Pointer` for it, which is both finite and Go's own answer —
`sync.Mutex` is 8 bytes in Go, and the shim that stands in for it is 8 bytes here, which is why
`go/types`' `Named` computes to Go's exact 112. Termination now rests on a rule that holds in the
target language rather than the source one: only `Struct` and `Array` recurse, `Struct` is answered
for value types alone, and C# forbids a value type from containing itself transitively (CS0523).

Two consequences worth carrying:

- **A legal self-referential Go type still gets a real size, not a bail-out.** `type node struct {
  value string; next *node }` terminates at the pointer and answers offsets `[0 16]`, size 24. A
  cycle *detector* would have answered "unknown" here; the classification answers correctly, which
  is why the fix is the classification and not a guard.
- **The depth cap underneath is a safety net, not the algorithm.** `structLayoutOf` refuses to
  recurse past 128 levels and answers "size unknown" (the r39d rule — a descriptor field that cannot
  be read truthfully stays unpopulated). No legal graph can reach it; tripping it would mean the
  classification is wrong again, and the point is that the next such defect costs a wrong number
  instead of a dead process and a run's worth of unmeasurable verdicts.

Guarded by `GolibTests.GoStructLayoutTests` — `ManagedReferenceField_IsOneWord_NotAStructToDescendInto`,
`CyclicManagedReferenceGraph_Terminates` and `SelfReferentialThroughPointer_IsFiniteAndCorrect`, the
first two of which are guards against a stack overflow no assertion can catch, so reaching the
assert at all is the guard. Measured by `go/types`: 513 verdicts with 44 absent, then **557 of 557
agreeing with `go test`**.

### `MapType().Hasher` and `Key.Equal` cannot be honored at all

**Rooted and deliberately NOT landed: `MapType().Hasher` and `Key.Equal` cannot be honored at all.**
The remaining `unique`/`net` wall is a map descriptor whose `Hasher`/`Key`/`Elem` are unpopulated, so
`concurrent.NewHashTrieMap`'s delegate construction fails on the first field it touches. Populating
them looks like the same shape as the reads above and **is not**, because the contract differs in
kind: `Hasher(unsafe.Pointer, uintptr) uintptr` must hash *the value at an address*, and the address
the call site produces cannot name a managed value. Measured, three ways: two boxes holding equal
`@string` values necessarily have **different** addresses (so an address-derived hash can never make
`unique.Make("hello")` agree with itself — the package's entire purpose); a box whose pointee
contains a reference has no pinnable slot and its address **moved across a forced GC**; and the
`unsafe.Pointer` handed to the delegate carries no link back to its source box by construction, its
constructor taking a `uintptr`. The key and elem *types* ARE recoverable from the descriptor's
carried `System.Type` — but populating those alone would be actively worse than the present failure:
`Key.Equal` is the comparability SIGNAL (a pointer-identity compare, not a value compare), so a
half-populated descriptor turns a loud construction failure into a map that silently mislays every
key. That is the increment-6 lesson inverted — **a descriptor field whose read cannot be honored must
not be populated to look truthful** — and it is why this row is handed on rooted rather than half
landed. The remedy is one layer down and outside this arc's files: `internal/concurrent.HashTrieMap`
is a managed-referent raw-metal case whose *contract* (a concurrent map over comparable `K`) the CLR
answers natively while its *mechanism* (hash the bytes at an address) it cannot, so it wants a
hand-owned `_impl.cs` on the `sync.Mutex` precedent.

### `new(T)`'s other half: a POINTER descriptor carries its POINTEE's array dims

**Its other half is descriptor cargo.** A POINTER descriptor carries its POINTEE's array dims
unshifted — the rule `Elem()` already applies when it hands them down — but nothing populated it, so
`reflect.TypeOf(new([3]int)).Elem()` described a dimension-LESS array and `reflect.New` of it
allocated a zero-length one, giving the fresh value a different Type from the one it mirrors.
`abi.TypeOf` measures it now, through `GoReflect.PointeeArrayDims`.

## Types built at run time

### `reflect.ArrayOf` composes a descriptor; it does not reconstruct a linker record

`ArrayOf(n, elem)` builds an array TYPE at run time, for a type no declaration in the program
produced. Go's own body cannot be converted usefully, and it does not degrade: before it assembles
its `arrayType` record (`Str`/`Hash`/`GCData`/`PtrBytes`/`Equal`, plus a `SliceOf` for the record's
`Slice` field) it looks the type up **by name** through `typesByString` → `typelinks()`, the
linker-built type table, which has no managed form and is a `NotImplementedException` stub. So every
call threw whatever it was asked for — `encoding/gob`'s `TestIgnoreDepthLimit` reports it as an
`infrastructure-error` rather than a failure — and the throw says nothing about the request: it is
the reconstruction of a **linker** record, which the managed bridge never needs.

golib's `array<T>` **is** the array type. The one part of a Go array type the managed emission
cannot hold is its LENGTH (C# has no const generic parameter for the `4` in `[4]byte`), and that is
exactly what the descriptor's **dims cargo** already carries for every declared array. So the whole
construction is the `(managed type, dims)` pair `abi.TypeOf` reaches from a live `[n]T` value:

```csharp
// reflect/value_impl.cs — the hand-own, beside its sibling constructor PointerTo
public static ΔType ArrayOf(nint length, ΔType elem) {
    if (length < 0) {
        throw panic("reflect: negative length passed to ArrayOf");
    }
    System.Type? st = sysTypeOfReflectType(elem);
    ...
    nint[]? elemDims = arrayDimsOfReflectType(elem);
    nint[] dims = new nint[1 + (elemDims is null ? 0 : elemDims.Length)];
    dims[0] = length;
    elemDims?.CopyTo(dims, 1);
    ...
    return toType(abi.synthType(typeof(array<>).MakeGenericType(st), dims));
}
```

**Interning is what makes it a round trip rather than a look-alike.** `canonType` keys the `ΔType`
wrapper on the managed type PLUS the dims rendering, so `ArrayOf(3, TypeOf(byte))` and
`TypeOf([3]byte{})` are the SAME canonical `reflect.Type` **by identity** — and `Len`/`Elem`/`Size`/
`Align`/`String`/`New`/`Zero` then agree because they read one descriptor, not because each was
separately made to agree.

**The dims COMPOSE**, and that is not a nested-array special case. The slot means *what `Elem()`
hands down*: an array consumes the head and passes the tail, while a pointer's and a map's dims pass
through unshifted. So `[n][3]byte` and `[n]*[3]int` are both spelled `[n, 3]`, and each accessor
takes back its own share. Repeated composition is therefore free, which is the shape `gob`'s
depth-limit test builds 101 deep.

What an array has **no slot** to hand down is a channel's DIRECTION or a map KEY's dims:
`abi.Type.Elem` descends those through a POINTER only, so `[n]chan<- T` describes `[n]chan T` here.
That is the cargo model's shape rather than this function's — a DECLARED `[n]chan<- T` reads back
exactly the same way — so it is recorded, not worked around (the r39d rule).

Guarded by the **`ReflectArrayOf`** behavioral test, whose every row is that identity claim. The
registry entry is `manualConversionFuncs["reflect"]["ArrayOf"]` (`manualTypeOperations.go`), which is
what turns the auto body into the placeholder the hand-own fills. Implementing `ArrayOf` alone does
**not** flip `gob`'s last verdict — `TestIgnoreDepthLimit` wraps its 101-deep array in a
`reflect.StructOf`, which is runtime struct synthesis over `System.Reflection.Emit` and a feature arc
of its own.

⚠ The guard's nested rows compare against a declared **variable**, not an empty composite literal,
and the difference is load-bearing. `var x [2][3]uint8` emits as `new(2, () => new(3))` — the inner
dimension is in the initializer, which is the source the bridge recovers a declared array's length
from. The empty literal `[2][3]uint8{}` emits as `new array<uint8>[]{}.array(2)`, whose two elements
are `default(array<uint8>)`, i.e. length ZERO — so the inner dimension is dropped and
`reflect.TypeOf(lit).Elem().Len()` answers `0` where Go answers `3`. That is a converter EMISSION
gap, older than and independent of this hand-own (it needs no reflection to reach), and it is
recorded here rather than papered over.

### `reflect.StructOf` MINTS a CLR value type, and then changes nothing else

`StructOf(fields)` is `ArrayOf`'s sibling one order of magnitude up. `PointerTo` and `ArrayOf` hand
`MakeGenericType` an **existing** managed type, because `ж<T>` and `array<T>` *are* the Go type; a
struct has no generic container to instantiate, so `StructOf` is the one caller that asks for a Go
type nothing declared and a real CLR **value type has to be minted** for it — with
`System.Reflection.Emit`, in golib's `GoStructSynthesis`.

The auto body dies where `ArrayOf`'s does, and one stub earlier than expected: measured, the first
throw is `addReflectOff` (from `runtimeStructField` → `resolveReflectType`), not `typelinks`. That is
the point rather than a detail — **everything past the validation loop is Go's runtime reconstructing
linker output** (`structTypeFixedN` prototypes, GC-program construction, `resolveReflectName` into
the linker's name blob, `unsafe_New`), so which stub is reached first is incidental. Both of
`reflect`'s *own* callers of `StructOf` are themselves such reconstructions — a fake struct
describing a func's argument frame (`initFuncTypes`), and `struct{S structType; U uncommonType; M
[n]Method}` to obtain an rtype followed in memory by a method array — so a hand-own owes them
nothing.

**What makes the mechanism honest is that nothing downstream is new.** Once the CLR type exists,
`abi.synthType` describes it exactly as it describes a converted struct, and `GoFields`,
`structLayoutOf`/`GoFieldOffsets`, `structFieldOf`, `FieldAliasBox`, `ZeroValueOf`,
`haveIdenticalUnderlyingType`, `GoTypeName` and `canonType` all run **unmodified** — not one of them
asks where a `System.Type` came from. A descriptor-only synthetic type would instead have grown a
second path in about ten places, and a green row would then prove the second path rather than the
bridge.

The mint carries five things, and each answers exactly one downstream reader:

| Emitted | Read by | Why it cannot be dropped |
|:--|:--|:--|
| `[GoType("dyn")]` on the type | `HasGoName`, `GoTypeName` | a `StructOf` result is a Go ANONYMOUS struct: `Name()` must be `""` and `String()` must render structurally |
| a **parameterless constructor** seeding every array-kinded field | `GoReflect.FieldArrayDims` | an array field's Go LENGTH |
| `[GoTag("…")]` on a field | `goTagOf` | `StructField.Tag` |
| `[GoArrayDims]` / `[GoMapKeyDims]` on a field | `FieldStampedDims` / `FieldMapKeyDims` | the pointer-hop and map hops |
| a `ʗ`-prefixed CLR field name | `collectGoFields` | `StructField.Anonymous` |

**The constructor is the piece that is easy to get backwards, so it is worth stating flatly.**
`collectGoFields` reads an *array* field's dims from a cached **zero instance** —
`Activator.CreateInstance(declaringType)` — because in converted code the converter emits the length
as a field **initializer** (`= new(4)`) that the generated parameterless constructor runs. The
`[GoArrayDims]` stamp is *not* that route: it exists for the pointer and map-element hops, where a
zero instance holds a nil pointer or an empty map and has nothing to measure. A `TypeBuilder` struct
has no field initializers, so **without an emitted constructor every synthesized array field would
report length 0** — silently, `0` being a legal Go length, and mis-sizing the struct as well, since
`structLayoutOf` sizes an array field from the same vector. Both routes are therefore emitted; they
cover disjoint cases, under exactly the rule `fieldCargoDims` applies to declared fields.

**Interning is the contract, not an optimization.** `encoding/gob` keys
`map[reflect.Type]gobType` and `enc.sent map[reflect.Type]typeId` on the result, so a fresh
descriptor per call makes every recursion a cache miss and every mutually recursive type an infinite
regress. Two properties of the shape key are load-bearing:

- it cannot be built from `System.Type`s. `[1]int` and `[2]int` are ONE `array<nint>` and
  `chan<- T` and `chan T` are ONE `channel<T>` — length and direction live only as descriptor cargo —
  so each field contributes its `abi.descriptorDimsKey` rendering, **reused rather than restated**,
  so a shape key and the descriptor it stands for cannot separate the same two types differently;
- the intern holds the **mint**, under a lock rather than a `ConcurrentDictionary` factory.
  `GetOrAdd` runs its factory concurrently and discards the losers' work, but the work here is
  `DefineType`, and a duplicate type name **throws** (measured on a bare probe: 3 of 4 racing threads
  failed).

**`PkgPath` nests.** `GoPackagePath(t)` is `GoPackageClassPath(t.DeclaringType)`, which reads the
declaring class's namespace plus its name with `_package` trimmed — so a struct with an unexported
field is minted **inside** a synthesized container class `<pkg>_package` in namespace
`go.<parent-path>`: `go.encoding.gob_package` for `encoding/gob`. The obvious spelling
`go.encoding.gob.gob_package` is measurably wrong and yields `"encoding/gob/gob"`. The container is
minted only when a field actually carries a `PkgPath`, so the common all-exported case pays nothing.

**Narrowings this hand-own ships with, recorded so they are met knowingly:**

1. **Interning is `StructOf`-local.** A converter-lifted anonymous struct of the same shape is a
   different CLR type, so `StructOf(f) == TypeOf(struct{F int}{})` is `false` here and `true` in Go —
   the same class as the cross-context anonymous-lift identity split. `haveIdenticalUnderlyingType`
   still answers `true` for the pair, so `AssignableTo`, `ConvertibleTo`, `Convert` and assignment
   all behave; only `==` on the `Type` splits. The one shape exempt is `struct{}`, whose managed form
   golib already declares (`EmptyStruct` *is* Go's empty struct), so the degenerate call reaches the
   type a declaration produces.
2. **A directional-channel field keeps its identity but not its description.** The direction is in
   the shape key, so `struct{C chan<- T}` and `struct{C chan T}` are distinct types; the minted field
   is a plain `channel<T>`, so `Field(i).Type.ChanDir()` answers bidirectional. Carrying it is one
   more seeded field in a constructor that already exists.
3. **Embedded fields with methods panic**, matching Go's own documented gap (*"StructOf currently
   does not support promoted methods of embedded fields"*) — and using Go's exact message wherever
   Go's own condition matches.
4. **The type's own `PkgPath()` and its `StructField.PkgPath` are answered by different rules.**
   `structFieldOf` uses `f.Exported ? "" : GoPackagePath(st)` and is exactly right; `rtype.PkgPath()`
   reads the same call with no `HasGoName` gate, so a synthesized struct that needed a container
   reports a package path for the TYPE where Go answers `""` for an unnamed type. That is
   pre-existing and corpus-wide (every converter-lifted anonymous struct behaves the same way), so it
   is pinned rather than changed here.

Guarded by the **`ReflectStructOf`** behavioral test — the only gate that checks the answer against
Go rather than against our own expectation — and by `GolibTests`' `GoStructSynthesisTests`, which
pins the three mechanisms that fail *silently*: the constructor, the shape key, and the `ʗ` prefix.
The last of those is asserted on `.Anonymous` and never on `Type.String()` on purpose: an embedded
field and a same-named regular field render **identically**, so a `String()`-based check could not go
red on the defect it exists to catch. The registry entry is
`manualConversionFuncs["reflect"]["StructOf"]`.

### `reflect.SliceOf` is the same one-liner as `PointerTo`

`SliceOf(elem)` dies in the same `typesByString` → `typelinks()` lookup and needs nothing but the
generic instantiation `typeof(slice<>).MakeGenericType(elem)` — the `PointerTo` shape exactly. The
only decision it carries is what dims to hand the descriptor, and the answer is **none**: a declared
`[]T` descriptor carries `null`, because `abi.TypeOf` measures dims for an ARRAY value and a POINTER's
pointee only. Passing the element's dims through would break the identity that makes the constructed
and the declared type one `reflect.Type`, and would not help either — `rtype.Elem`'s non-pointer,
non-map arm consumes the head of the dims vector, so a one-element vector hands down nothing. So
`SliceOf(ArrayOf(3, byte))` describes `[][3]byte` with the element's length unknown, which is exactly
what `TypeOf([][3]byte{})` reads back today. That residual belongs to the cargo model — a slice type
has no dims slot — not to this constructor.

⚠ Two PRE-EXISTING residuals meet here and neither belongs to this constructor, so the guard
asserts identity and deliberately does not print the name. go2cs renders a dims-less `array<T>` as
`[]T` by design (`GoReflect.TypeNaming`: *"length is not carried on the managed type"*), so
`fmt.Println(reflect.TypeOf([][3]uint8{}))` already prints `[][]uint8` at master, and
`reflect.TypeOf([][3]uint8{}).Elem() == reflect.TypeOf([3]uint8{})` is already `false` — both with no
reflection constructor in sight. One root: a slice type has no dims slot, so nothing survives the
`Elem()` hop. Widening the cargo there is an arc of its own.

---

[← Reflection](README.md) · [Index](../README.md) · [Values →](values.md)
