# Maps and Channels

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#maps-and-channels)
Go maps and channels convert to the golib [`map<K,V>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/map.cs) and [`channel<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/channel.cs) structures. `make` becomes a constructor; channel send/receive use the runtime operators:

```go
m := make(map[string]int)
c := make(chan int, 3)
```
```csharp
var m = new map<@string, nint>();
var c = new channel<nint>(3);
```

Map reads honor Go's nil-map and comma-ok semantics (see [Nil and Zero Values](nil-and-zero-values.md#nil-and-zero-values) and [Multi-Result Values and Comma-Ok Forms](multi-result-and-comma-ok.md#multi-result-values-and-comma-ok-forms)).

## A `range` body may MUTATE the map it is ranging over — the enumerator walks a KEY SNAPSHOT

Go's spec permits the body of a `range` to add to and delete from the very map being ranged:

> "If a map entry that has not yet been reached is removed during iteration, the corresponding
> iteration value will not be produced. If a map entry is created during iteration, that entry may
> be produced during the iteration or may be skipped."

.NET's `Dictionary<TKey, TValue>` enumerator permits neither reading. A structural **add** bumps its
internal version and the next `MoveNext` throws
`InvalidOperationException: Collection was modified; enumeration operation may not execute`. Two
adjacent mutations do *not* throw, which is exactly what made this so easy to miss: since .NET Core
3.0 an **overwrite** of an existing key and a **`Remove`** are both version-free. Only the insert
bites — and only when the inserted key is genuinely new.

Handing out that enumerator directly would make every legal Go range-with-insert a runtime fault,
so golib's `map<K,V>` implements Go's contract itself: the range takes a **snapshot of the
entries** and re-reads each value at the moment it is visited
([`map.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/map.cs),
`enumerateStore`). That lands every clause of the spec —

* an entry **removed** before it is reached fails the visit-time lookup and is not produced, which
  is the half Go *guarantees*;
* an entry **created** during the range is absent from the snapshot and so is never produced, which
  is the "or may be skipped" half Go leaves free;
* a value **overwritten** during the range is produced at its current value, which is what Go's own
  range reads out of the bucket when it arrives there;
* every pre-existing entry is still produced **exactly once**, so a body that inserts cannot be
  re-entered for a key it has already handled.

The nil-key entry (see [The NIL map key](#the-nil-map-key)) is produced first; Go's range order is
unspecified and deliberately randomized, so the position is free. The snapshot walk itself starts at
a random entry and wraps, as Go's iterator starts at a random slot, so two ranges over one map can
differ in order.

**One key shape makes the visit-time lookup the wrong instrument, and it is a real Go shape rather
than a curiosity.** A [NaN key is equal to nothing, itself included](manual-conversions.md#a-nan-map-key-is-never-equal-to-anything-itself-included),
so `m[NaN] = v` twice stores *two* entries and neither can ever be read back **or deleted**. For such
a key the lookup always misses, so a re-read on arrival silently drops every NaN entry from every
range — a worse defect than the one this machinery exists to fix, because nothing raises. So a miss
is disambiguated with **the store's own comparer**: if the key is not even equal to itself, no lookup
can match it and no `delete` can remove it, so the snapshotted entry is produced. Using the
dictionary's comparer settles "unretrievable" by exactly the relation whose failure is being
interpreted, rather than by a hardcoded list of float types — a custom comparer gets the same
treatment for free. The one operation that *does* remove such an entry is `clear`, which empties the
store outright, so a now-empty store suppresses it.

`encoding/json` reads this out immediately, and loudly: `mapEncoder` sizes
`sv = make([]reflectWithString, v.Len())` and fills it **by index** from `MapRange`, so a range that
yields fewer entries than `len()` leaves zero `reflect.Value`s in the tail and panics inside
`stringEncoder`'s `v.Type()`. That is `TestMarshalTextFloatMap`, and it is the reason the shape is
guarded at both layers.

```go
// Legal Go: the body inserts a new key into the map it is ranging.
for k, v := range m {
    if len(k) == 1 {
        m[k+"!"] = v * 10
    }
}
```
```csharp
foreach (var (k, v) in m) {
    if (len(k) == 1) {
        m[k + "!"u8] = v * 10;
    }
}
```

The emission is an ordinary `foreach` — the fidelity lives in the runtime type, not in the emitted
shape, so nothing about the converted code advertises the difference.

The cost is one `KeyValuePair[]` per non-empty range where there was none, and the self-equality test
only ever runs on the miss path. That is a deliberate trade: this is the construct's *semantics*, and
go2cs converts behavior first. If a range ever measures hot enough to care, the snapshot is the one
thing to pool; the shape above does not change.

This is not an exotic corner. `net/http`'s HTTP/2 server hits it in `promoteUndeclaredTrailers`,
which ranges the handler's header map and writes each promoted `"Trailer:Foo"` entry back under
`"Foo"` — a new key. With the .NET enumerator handed out directly, the exception escapes the
handler goroutine, the converted test host's containment policy absorbs it as a test failure, the h2
stream is never completed with its trailers and `END_STREAM`, and the client blocks in
`http2pipe.Read` forever: a deterministic hang of `TestServerUndeclaredTrailers/h2` that leaves the
rest of the `net/http` suite unreached. Guarded from the Go side
by `tests/Behavioral/MapMutateDuringRange`, which covers insert, overwrite, delete, insert-with-delete,
a control that mutates a *different* map, and the NaN-key shapes; and at the golib level by
`tests/GolibTests/MapRangeMutationTests.cs`, which pins what the Go side cannot reach — the nil-key
entry's participation, and that a map *without* a nil key never yields a phantom entry.

## `m[string(b)]` — the map-READ key does not copy (`tmpstring`)

The Go compiler special-cases `m[string(b)]`: because a map lookup hashes and compares its key but
never retains it, the `[]byte`→`string` conversion's result provably does not outlive the index
expression, and the copy is skipped (`runtime.slicebytetostringtmp`). A copying conversion would
pay one backing `byte[]` per probe, which is exactly the allocation
`net/textproto.TestCommonHeaders`' want-ZERO `testing.AllocsPerRun` assert measures over
`canonicalMIMEHeaderKey`'s common-header probe. The converter recognizes the same shape and
emits golib's `tmpstring(b)` — a TRANSIENT `@string` windowing the slice's live backing through
`@string.TransientAliasOf`, zero allocation:

```go
if v := commonHeader[string(a)]; v != "" { return v, true }
v, ok := m[string(b)]
```
```csharp
@string v = commonHeader[tmpstring(a)]; if (v != ""u8) { return (v, true); }
var (v, ok) = m[tmpstring(b), ꟷ];
```

The scope is deliberately EXACTLY the shape whose safety Go's own optimization proves
(`mapReadTmpStringKey`, `convIndexExpr.go`): a map index in **rvalue** position — plain or comma-ok —
whose key type is the PREDECLARED `string` and whose key expression is a conversion to predeclared
`string` over a plain `[]byte` (element exactly basic `uint8`). Everywhere the string ESCAPES the
copying conversion stays: an assignment target (`m[string(b)] = v` stores the key — emitted
`m[((@string)b)] = v`), `delete(m, string(b))`, the function's own `return string(a)` paths, a
named-string key type, a named-over-byte element. Compound assignments and `++` mark the index an
assignment target, so they keep the copy for both their read and write halves. (Guarded by the
`MapStringBytesLookup` behavioral test — hit/miss/comma-ok probes through a mutating slice, a
sub-slice operand, and the store-then-mutate case proving the STORED key copied — and by
`GolibTests.AllocationCounterTests.TmpStringMapProbeChargesNothing`, which pins the zero charge in
both units.)

## The NIL map key

Go's map accepts **nil as a key** whenever the key type can be nil — `map[any]V`, `map[error]V`,
`map[*T]V`, a named-interface key — and that entry is an ordinary entry: it reads, comma-oks,
overwrites, deletes, counts toward `len`, is visited by `range`, is dropped by `clear`, appears in a
composite literal, and copies through `maps.Clone`. The converter renders it as `default!`, so the
Go and C# sides line up member for member:

```go
m := make(map[any]string)
m[nil] = "nil-key"
v, ok := m[nil]
delete(m, nil)
lit := map[any]int{nil: 1, "b": 2}
```
```csharp
var m = new map<any, @string>();
m[default!] = "nil-key";
var (v, ok) = m[default!, ꟷ];
delete(m, default!);
var lit = new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2};
```

golib's `map<TKey, TValue>` wraps a `Dictionary<TKey, TValue>`, which **rejects a null key** with
`ArgumentNullException` *before* its comparer is ever consulted — there is no comparer to teach. So
the nil entry gets a slot of its own: the backing store is a private `Dictionary<TKey, TValue>`
**subclass** carrying `HasNilKey` + `NilKeyValue`, and every member of the map surface routes a null
key to that slot (indexer get/set, `Set`, comma-ok, `TryGetValue`, `ContainsKey`, `Add`, `Remove`,
`Clear`, `Count`, both enumerators, `Keys`/`Values`, the copy constructor behind `CloneMap`, and
`ToString`). `range` yields the nil entry ahead of the snapshot walk (see the range section above) —
Go's range order is unspecified and deliberately randomized, so the position is free.

Two design points are load-bearing. First, the slot lives on the **store, not on the struct**: a Go
map is a reference type, so every copy of a `map<K,V>` value must observe the same nil entry, and a
field on the `readonly struct` would make a write through one copy invisible through another.
Deriving from `Dictionary` (rather than wrapping it) also keeps the struct exactly one reference wide
— no extra allocation, no widened value — and leaves every existing Dictionary interop path (the
implicit conversions, the `ICollection<T>` casts, the reflection bridge's backing-field probe)
binding as before. Second, `map<K,V>` is golib's hottest type, so the nil test is
`!typeof(TKey).IsValueType && (object?)key is null`: `typeof(TKey).IsValueType` is a **JIT-time
constant**, so for a value-type key — `map[string]V`, `map[int]V`, the overwhelmingly common shape —
the test and every branch it guards fold away and those instantiations compile to exactly the code
they had before nil keys existed. Only a reference-typed key pays a null check, and the slot
operations themselves sit behind `[MethodImpl(MethodImplOptions.NoInlining)]` so the hot members stay
small.<!-- Measured at the time (machine and platform not recorded): 276.4 ms with
the slot vs 271.8 ms without, median of three 9-run sessions each, inside a 260–310 ms run-to-run band.
The prose sentence "PerfMap is flat" was removed at the 2026-10-26 docs review: a performance result is
stated only with a recorded platform. -->

One consumer cannot see the slot and had to be threaded explicitly: `reflect.DeepEqual` walks the
backing `IDictionary` through a reflected field probe, which never yields a nil key, and a lone nil
entry does not necessarily show up in the `Len` comparison either (one extra ordinary key on the
other side hides it). `IMap` therefore exposes a non-generic `NilKeyEntry` — `(present, boxed value)`
— with a default implementation on `IMap<TKey, TValue>` that asks the comma-ok indexer, so the
generated named-map wrappers satisfy it with no go2cs-gen change; DeepEqual compares that entry
before the dictionary walk. (Guarded by `NilMapKey`: set/get/comma-ok/overwrite/delete/`len`/`range`/
`clear` on `map[any]string`, a nil-key composite literal, `map[error]int`, and the nil-key reads on
both a nil and an empty map, all output-compared vs `go run`. Without the slot the very first
`m[nil] = …` throws `ArgumentNullException`; sync's `TestIssue40999` is a Go test with that shape.)

**PRINTING a nil-key map is a second such consumer, one layer down.** `fmt` orders map keys through
`internal/fmtsort`, which walks the map with `reflect.Value.MapRange` and compares the key Values.
Typed from its BOXED OBJECT, a nil key is `null`, so `Key()` would hand back the **invalid zero
Value**, and `fmtsort.compare` cannot compare that at all: it reads `aVal.Type()` first thing and falls
through to `panic("bad type in compare: " + aType.String())` on a nil type, so printing *any* map
carrying a nil key would die inside `fmt`. Go's rule is the slot rule the bridge also applies to
struct fields and slice elements — **a map entry Value is typed by the map's
DECLARED key/value type**, so `map[any]V` hands out Kind Interface keys whatever the dynamic value is,
and a nil key or value is a VALID nil Value of that type. `MapIter` carries the map's key and value
types (plus the map Value's read-only bits) and `Key()`/`Value()` build through `makeTypedValue`, which
is also what makes `fmtsort`'s nil-compares-low rule reachable — a nil key sorts first, exactly as Go
prints it. (Guarded two ways: `NilMapKey` carries the printing shapes — `map[any]int` with
a nil key through both `Println` and `%v`, a nil-only map, `map[error]int`, `map[*int]int` — and
`ReflectMapRangeNilKey` drives the bridge directly over `map[any]int`, a named `map[any]int`,
`map[*int]string`, `map[error]int`, a concrete `map[string]int`, a slice-valued map and a nil
interface VALUE, printing only order-independent facts because Go's map iteration order is
unspecified. Typing entries from the boxed object reports `any: 0 0 1 5 3` — zero interface-kind
keys, zero nil keys, ONE invalid key, and a value sum of 5 instead of 6 because the nil entry is
skipped — against Go's `any: 3 1 0 6 3`.)

## An INTERFACE map key compares by Go equality, never by adapter identity

Go compares interface values by (dynamic type, dynamic value), and that **one** relation serves both
`==` and map-key lookup: a `map[Iface]V` finds an entry under exactly the values `==` calls equal.
Emitted `==`/`!=` route through golib's
[`builtin.AreEqual`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs),
which unwraps the three generated adapter tiers (`IInterfaceAdapter`, `IжAdapter`, `IValueAdapter`)
before comparing, so `map<K,V>`'s backing `Dictionary<TKey,TValue>` must not use the **default**
comparer, which would compare the *wrappers*.

That gap is observable because an interface value's wrapper is not stable. The same Go dynamic value is
presented through whichever adapter the static interface currently holding it calls for, so asserting an
`Object` to a narrower `dependency` yields a **different** wrapper object over the same receiver box.
Under the default comparer the asserted value could not find its own entry in the map it came out
of — while `==` on the very same pair still answers `true`, because `AreEqual` unwraps. Equal but
unfindable:

```go
M := make(map[dependency]*graphNode)
for obj := range objMap {                       // objMap is map[Object]*declInfo
    if obj, _ := obj.(dependency); obj != nil {
        M[obj] = &graphNode{obj: obj}
    }
}
for obj, n := range M {
    for d := range objMap[obj].deps { /* ... */ }   // every key of M IS a key of objMap
}
```
```csharp
foreach (var (obj, n) in M) {
    foreach (var (d, _) in (~objMap[obj]).deps) { /* ... */ }
}
```

This is `go/types`' own `initorder.dependencyGraph`: with the default comparer the converted type
checker could not type-check **any** source, because the missed lookup returns a nil `ж<declInfo>`, and
`~` on it nil-panics one frame later — surfacing through `check.cs`'s `handleBailout`, a faithful
re-panic of Go's own `default: panic(p)` arm, which is a bailout frame and not the fault site.

golib centralizes on the one relation:
[`GoEqualityComparer`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoEqualityComparer.cs)
projects `AreEqual` as an `IEqualityComparer<TKey>`, hashing the **unwrapped root** so the hash stays
consistent with it — the same rule the compile-time `ImplementGenerator` adapters apply
(`m_box.GetHashCode()`). It is installed only for key types that can
actually carry an adapter (`typeof(TKey).IsInterface`, or `any`); a concrete key — `@string`, an
integer, a converted struct — is never wrapped and keeps `EqualityComparer<TKey>.Default`'s
devirtualized path, the test being a JIT-time constant per instantiation. The relation is not restated
inside each generated shell: `AreEqual` is golib's single definition of Go equality, and a second copy
per shell class is exactly the drift that splits `==` from lookup.
(Guarded by the `InterfaceAssertionMapKey` behavioral test — pointer- and value-receiver implementors,
an `Object` that is not a `dependency`, lookup after narrowing, lookup after re-widening, and the
`Object(d) != obj` identity probe that pinned the equal-but-unfindable split.)

golib's own `error<T>` — the hand-written generic shell for `error`, the one shell go2cs-gen does not
emit — joins the same `IInterfaceAdapter` unwrap protocol its generated siblings define, carrying the
identical member every generated shell does: the `ж` box when pointer-backed (Go pointer-identity
equality), the wrapped value otherwise. Outside the protocol, two independently minted carriers of the
same error value would compare by reference. The shape needs two minters for one value, which the
white-box test model makes routine: crypto/tls's production code never casts `AlertError` to `error`
(it only boxes it into `any`), so fmt's `%w` assert resolves the runtime shell, while the test
assembly's `errors.Is` target arrives as its own generated `ᴠ` value adapter, and
`errors.Is(err, AlertError(alertBadCertificate))` must still answer true for the alert `quicError`
wrapped (`TestQUICHandshakeError`).<!-- history: error<T> joined the IInterfaceAdapter protocol on 2026-08-19
(the crypto/tls regression); until then errors.Is answered false in TestQUICHandshakeError. --> (Guarded by
`GolibTests.ErrorShellCarrierEqualityTests` — two shells over one value, the
pointer-identity flavor, and the protocol membership itself.)

## Named map types and constrained map access

A defined map type — `type Grades map[string]int` — emits the `partial struct … /*map[K, V]*/` forward declaration, implemented by go2cs-gen's Map template: full forwarding of `IMap<K, V>` (including the two-value comma-ok indexer), `IDictionary<K, V>`, enumeration, and the `ISupportMake` factory through the wrapped `map<K, V>`. Its composite literal wraps the concrete map literal in the named constructor — `new Grades(new map<@string, nint>{["a"] = 1})` — mirroring named arrays/slices (a direct indexer-initializer would target a default wrapper with no backing dictionary).<!-- history: visitMapType was a stub before this; the old emission produced Go-style `key: value` inside C# braces (CS1513). --> Comma-ok indexing works through a **constrained map type parameter** too: `v, ok := m[k]` where `M ~map[K]V` detects the map CORE of the constraint (both at the assignment's tuple gate and in the index emission) and routes the same `m[k, ꟷ]` two-value indexer, which lives on `IMap<K, V>` itself. The **nil comparison** `m == nil` — Go's only legal map comparison, maps.Clone's nil-preserve guard — emits the `IMap.IsNil` property (`if (m.IsNil)`; backing-store null, distinct from an allocated empty map — no operator exists on a type parameter, CS8761), and `delete(m, k)` on a constrained map binds a golib `delete(IMap<K, V>, K)` overload (key/value types infer from the interface conversion). (Guarded by the `GenericTypeInference` extension `EqualMaps` — a maps.Equal clone over a named map type through the constraint, comma-ok + comparable-erased equality, values vs Go.)
For source-generated named-map wrappers, the generator parses the `/*map[K, V]*/` payload at the top-level comma, not every comma in the string. This matters for function-valued maps: `type opTable map[CrossPkgLib.Ticks]func(int, int) int` emits `map<global::go.CrossPkgLib_package.Ticks, Func<nint, nint, nint>>`, preserving the full delegate as the value type. Any source-file alias used inside the definition comment is resolved through Roslyn and rewritten to its fully-qualified target before the template emits `IMap<K, V>`, `IDictionary<K, V>`, and `ICollection<KeyValuePair<K, V>>`; generated files therefore do not depend on file-local package aliases such as `using token = ...`. (Guarded by `NamedMapCrossPkgKey`.)

The named arm must also **carry the map type into the key/value slot emission**, exactly as the
unnamed arm does. Every `MapSource` slot rule in `convKeyValueExpr` is gated on that type: a pointer
KEY or VALUE boxes to `Ꮡx` rather than aliasing a deref'd value into a `ж<T>` slot (CS0029), an `any`
key or value slot re-renders a string literal as `(@string)"…"u8` instead of the bare `u8` span (which
has no conversion to an `object` slot — CS1503), an untyped-constant `any` key boxes at Go's default
type so the store and every lookup agree on the boxed type, and an array-typed key clones into the
map. Left nil, a *named* map type would silently opt out of all of them, so `type namedAny map[any]int`
with `namedAny{nil: 1, "b": 2}` would emit `["b"u8] = 2` and fail to compile while the identical
**unnamed** `map[any]int{nil: 1, "b": 2}` emits `[(@string)"b"u8] = 2`. (Guarded by
the `DeepEqual` behavioral test's named-map block, which is written over `map[any]int` precisely so
the `any`-slot rule is exercised through a named type.)

A bare **`make(Grades)` with no size argument** defaults the size to 0 — emitting `new Grades(0)` — so the wrapper's allocating `(nint size)` constructor runs and the backing dictionary is created. The generated wrapper struct has that `(nint size)` constructor but no *parameterless* one, so a plain `new Grades()` would be `default(Grades)` — a **nil** map (null backing store, so `m == nil` is true and a write panics), whereas Go's `make` returns a **non-nil empty** map (`m == nil` false, writes succeed). The default is applied only to `*types.Named` defined types: the unnamed `map<K, V>` builtin already allocates in its own parameterless constructor and stays `new map<K, V>()`, and a type *alias* (`type M = map[int]int`) resolves to that builtin rather than a wrapper — so neither drifts (`make` emission in `convCallExpr.go`, right beside the named-channel default below). This mirrors the named-channel unbuffered default (`make(closeWaiter)` → `new closeWaiter(0)`); a sized `make(Grades, n)` (already `new Grades(n)`) and the `Grades{}` composite literal are non-nil already. (Guarded by `NamedMapMakeNonNil` — `make` with and without a size, a plain nil `var`, and a composite literal, each `== nil`-compared and output-compared vs Go.)

Two conventions for the definition comment's text coexist, and the generator's alias substitution must tell them
apart. The map/channel emitters write dotted types in **source-alias form** (`CrossPkgLib.Ticks`,
via `getAliasQualifiedTypeName`), which the substitution above resolves; the slice/array element and
defined-over-selector emitters write the **namespace-qualified form** (`io.fs_package.FileInfo`,
via `getFullyQualifiedTypeName`), which roots through the `go` namespace and must pass through untouched.
The telltale is the segment after the leading identifier: a real alias maps to a package *class*,
so its next segment is a type name — a `_package`-suffixed next segment means the leading
identifier is a namespace segment that merely *collides* with a file alias. net/http's fs.go
aliases `io` while declaring `type fileInfoDirs []fs.FileInfo` → `[]io.fs_package.FileInfo`;
substituting the `io.` would produce the nonexistent `go.io_package.fs_package.FileInfo` (CS0426).<!-- at the fix:
CS0426 x48 in net/http. -->
The substitution skips exactly those occurrences (a negative lookahead on `_package.`). On the
converter side, the namespace-qualified form must lead with the **canonical** qualifier, never a
file-local Δ collision-rename: a consumer whose own namespace has a same-named child imports under
`using ΔIoLike = IoLike_package;`, but `[]ΔIoLike.FsLike_package.Info` resolves nowhere in the
alias-free `.g.cs` — `canonicalizeQualifierRename` reverts a leading import-rename segment
(mirroring the visitTypeSpec global-using-target rule). (Guarded by `NamedSliceChildPkg` — a
nested-namespace consumer package importing both `IoLike` and `IoLike/FsLike`, with a named slice
of the subpackage's type used across the assembly boundary.)

A map indexed by a **non-empty interface key** converts a concrete key expression through the same interface-adapter path used by assignments and call arguments. For example, `seen[item] = "kept"` where `seen` is `map[Node]string` and `item` is `*Item` emits `seen[new ItemжNode(item)] = "kept"`; the comma-ok read emits the same adapter for the key, `seen[new ItemжNode(item), ꟷ]`. This records the pointer implementation (`GoImplement<Item, Node>(Pointer = true)`) and keeps dictionary lookup semantics aligned with Go's interface key identity. Empty-interface map keys keep their existing literal handling (`map[any]...` turns string literals into Go strings rather than UTF-8 spans), and pointer-typed map keys keep the direct pointer-box path. (Guarded by `InterfaceMapKeyPointer`.)

A **pointer-keyed** map indexed by the method's **receiver** supplies the receiver's box as the key, exactly like the deref-aliased pointer-parameter case: `t.m[c]` inside `func (c *conn) …` emits `t.m[Ꮡc]` (plain read, write, and the comma-ok `t.m[Ꮡc, ꟷ]` alike) — net/http transport.go's idle-connection bookkeeping (`t.idleLRU.m[pc]`) needs the box where the deref-aliased VALUE would not bind `ж<persistConn>` (CS1503 plus the `(v, ok)` deconstruction cascade). The box exists only on a **direct-ж** method, so the receiver-as-map-key body shape itself *promotes* the method to direct-ж (`bodyUsesReceiverAsPointerValue` has an `IndexExpr` case, gated on a pointer-KEYED map operand) — a method whose only pointer-use of its receiver is the map key still gets `this ж<conn> Ꮡc`. A pointer LOCAL is the key itself (no `Ꮡ`), and `delete(t.m, c)` boxes through the ordinary pointer-argument rule once the method is direct-ж. (Guarded by `PtrKeyMapReceiverLookup` — pure-shape promotion, plain read/write, comma-ok, and delete through two distinct receiver identities, values vs Go.)

A value sent into a channel of **non-empty interface element type** converts through the same
interface-adapter path used by assignments and call arguments; the test for interface-ness is made on
the channel's ELEMENT type, not on the channel type (whose underlying is `*types.Chan`).<!-- history: the send
emission once tested the CHANNEL type itself for interface-ness, so no conversion ever fired. --> A value
implementation sends bare while recording the implement pair for the
generator (`vs.ᐸꟷ(new dog(name: "rex"))` with `[assembly: GoImplement<dog, speaker>]`); a pointer
implementation wraps the box in its generated pointer adapter:

```go
ps := make(chan speaker, 1)
c := &cat{name: "tom"}
ps <- c
```

```csharp
var ps = new channel<speaker>(1);
var c = Ꮡ(new cat(name: "tom"));
ps.ᐸꟷ(new catжspeaker(c));   // records GoImplement<cat, speaker>(Pointer = true)
```

A pointer-typed send value renders as its box (parity with the argument-position rule in
`convExprList`), and a type-parameter element (`chan T` in generic code) keeps the bare emission.
The string-literal empty-interface arm of the same helper is described under
[Empty Interface (`any`)](empty-interface.md#empty-interface-any). (Guarded by `AnyStringLitChanSend` — a value impl and a pointer
impl sent through a `chan speaker`, method-dispatched on receive, output-compared vs Go.)

## Named channel types

A defined channel type — `type closeWaiter chan struct{}` (net/http's h2 bundle) — emits the
`partial struct … /*chan T*/` forward declaration,<!-- history: visitChanType was a stub before this; the corpus
had no GoType("chan …"), CS0246 at every use. -->
implemented by go2cs-gen's Channel template: the wrapper holds a `channel<T>` and forwards its full
surface — the Go-visual send/receive members (`ᐸꟷ`, `ꟷᐳ`, including the select-registration
`ᐸꟷ(v, ꓸꓸꓸ)`/`Sending`/`Receiving` forms), the comma-ok `Receive(ꟷ)`/`Received` pair, `IChannel`'s
object-typed members, enumeration for `range`, the `ISupportMake` factory, and a `(nint size)`
constructor so `make(closeWaiter)` emits `new closeWaiter(0)` — a REAL unbuffered channel (the make
path resolves the chan through `Underlying()`, giving named channels the same unbuffered default as
plain `chan T`; the wrapper constructor forwards the size unclamped, so named channels can be
unbuffered — see [Real channel runtime](#real-channel-runtime--the-hchanselectgo-port-rendezvous-caplen-single-fire-uniform-random)):

```go
type closeWaiter chan struct{}
func (cw *closeWaiter) Init() { *cw = make(closeWaiter) }
func (cw closeWaiter) Close() { close(cw) }
func (cw closeWaiter) Wait()  { <-cw }
```

<!-- source: src/tests/Behavioral/NamedChannelType/main.cs.target:8-20 -->
```csharp
partial struct closeWaiter /*chan EmptyStruct*/;

internal static void Init(this ref closeWaiter cw) {
    cw = new closeWaiter(0);
}

internal static void Close(this closeWaiter cw) {
    close<EmptyStruct>(cw);
}

internal static void Wait(this closeWaiter cw) {
    ᐸꟷ<EmptyStruct>(cw);
}
```

Two deliberate wrinkles. **Free-function channel ops name the element type explicitly** —
`ᐸꟷ<EmptyStruct>(cw)`, `close<EmptyStruct>(cw)`: golib's `ᐸꟷ<T>(channel<T>)`/`close<T>(in
channel<T>)` reach the wrapper only through its user-defined conversion to `channel<T>`, which C#
generic inference never considers (CS0411); the explicit type argument lets the conversion apply at
the argument instead (`namedChanElemTypeArg`, applied at the unary-receive, select-registration and
`close` emission sites — a plain `chan T` operand is byte-identical; a package that ALSO
declares a `close` method keeps the `builtin.` shadow qualification of the general builtin path,
so net/http emits `builtin.close<EmptyStruct>(cw)`). **The wrapper's `Close` is an
explicit `IChannel` implementation only**: Go code commonly defines its OWN `Close()` method on a
named channel type (the closeWaiter shape above), and a public instance `Close` would shadow that
method's extension form at every call site; `close(ch)` routes through the golib free function, so
no public surface is lost. (Guarded by `NamedChannelType` — the closeWaiter trio plus a buffered
`type intQueue chan int` exercising make/send/len/cap/receive/comma-ok/close/range/select, output
vs Go.)

## A GENERIC defined map, channel or slice keeps its type parameters

A generic defined map or channel type declares its type parameters, and their constraints, on the
forward declaration AND on the accessibility line, exactly as a generic defined array or slice
already did. `type Set[T comparable] map[T]void` (the hashset module's exported shape) emits:

<!-- source: src/tests/Behavioral/GenericDefinedMapChan/GenericDefinedMapChan.cs.target:11-12, 49-51 -->
```csharp
partial struct Set<T> /*map[T, @void]*/;

public static void Reset<T>(this ref Set<T> s) {
    s = new Set<T>(0);
}
```

with `public partial struct Set<T> {}` in `package_info.cs`. Without the parameter list the
declaration would be a NON-generic `partial struct Set` over an unbound `T`, so every member go2cs-gen
generates for it would be CS0246, every use (`Set<nint>`) CS0308, and the accessibility line would
declare a second, empty public type. On the generator side the Slice, Map and Channel templates take
the base template's `ConstructorName` for their capacity/length constructor, because the type's full
name (`public Set<T>(nint size)`) does not parse as a constructor; a non-generic shell's constructor is
unaffected. Go's standard library declares no generic defined map or channel type, so a third-party
module (the hashset module) is where the shape appears. (Guarded by
`GenericDefinedMapChan` — a generic map over an unexported named empty struct and over `struct{}`,
a two-parameter map, a channel and a slice, each with value and pointer receivers and two
instantiations from `main`, output vs Go — plus `genericDefinedMapChan_test.go` on the declarations
and `GenericInheritedShellConstructorTests` on the generated constructors.)
<!-- history: before this both lines dropped the parameter list, and the behavioral corpus declared no such type, so
nothing exercised either half until a third-party module did. -->

## A function-LOCAL named type declaration hoists to member level (slice/map/channel/array/pointer)

C# forbids a type declaration inside a method body, so a `type X []T` / `type X map[K]V` /
`type X chan T` / `type X [N]T` declared **inside a function** cannot emit its `partial
struct X;` forward declaration in place — the following statements would then parse as MEMBER
declarations (`CS1519 Invalid token 'foreach' in a member declaration`, `CS1513 } expected`, the
map form's `CS8124`). A local `type X struct{…}` already hoists: `visitStructType`/`visitIdent`/
`visitInterfaceType` each redirect the declaration into `currentFuncPrefix` (emitted at member level
ahead of the method), rename it with the enclosing-function prefix (`ExampleChunk_People`), and
register the lifted name in `liftedTypeMap` so every reference resolves to it. The shared helper
`liftLocalTypeDecl` (`visitTypeSpec.go`) applies that same hoist to the array/slice, map, and channel
emitters (slices' `example_test` and maps' `maps_test` declare such types): at package scope it is a
no-op (target stays `v.targetFile`, `finish()` does nothing), and inside a function it prefixes the name, registers
the lift, redirects to a member-level builder,
and flushes into `currentFuncPrefix`. A local **slice/array of a local element type** also needs the
element resolved to its lifted name: `visitArrayType`'s simple-identifier fast path (which keeps the
written name so `[3]rune` stays `rune`) is skipped when the element is itself a lifted local type
(`!v.liftedTypeExists`), routing it through `getFullyQualifiedTypeName`, which resolves `liftedTypeMap` — so
`type People []Person` (Person a local struct) emits `partial struct ExampleChunk_People
/*[]ExampleChunk_Person*/;`, not the raw `[]Person`. (Guarded by the `LocalNamedTypeDecls`
behavioral test — a function-local named slice-of-local-struct, map, channel, and fixed-size array,
each constructed/ranged/indexed in the body and output-compared vs Go; without the hoist four
`partial struct …;` declarations land in the method body.)

Two completions of the same rule, both demonstrated by `encoding/gob`'s test suite:

* **The POINTER kind hoists too.** `type X *T` takes `liftLocalTypeDecl` like the other kinds; left in
  the body, its `partial class X /*ж<…>*/;` is `CS1525 Invalid expression term 'partial'` (gob's
  `codec_test.go` `type Rec ***Rec`) and takes the rest of the function with it. The lift is taken
  **before** `convStarExpr` renders the pointer text so a self-referential declaration resolves its
  own name through `liftedTypeMap`.
* **A SELF-REFERENTIAL local type re-resolves its element after the hoist.** The array/map/channel
  emitters resolve the element/key/value name *before* the declaration's own hoist registers its
  lifted name, so `type recursiveSlice []recursiveSlice` / `type recursiveMap
  map[string]recursiveMap` (gob's `encoder_test.go`) would emit `/*[]recursiveSlice*/` on a
  member-level `TestRecursiveSliceType_recursiveSlice` — a name that does not exist after the lift,
  `CS0246` inside the generated slice/map partial. Each emitter therefore re-resolves its element through
  `liftedTypeMap` **when the hoist actually renamed the declaration**; a package-level declaration
  never renames, so its emission is untouched (`main_recursiveSlice /*[]main_recursiveSlice*/` in
  the `LocalNamedTypeDecls` golden).

**The ALIAS kind takes the lift too — and for a different reason.** A local declaration that emits a
`using` ALIAS rather than a nested type — a real `type X = Y`, or a defined type over a *named*
interface such as `type X any` — takes the hoist as well. It needs no member-level redirection (an alias is emitted at
file scope either way), but it needs the
NAME, because the alias it writes is a `global using`: scoped to the whole **compilation**, not to
the file, let alone the function. Two functions declaring `type testFnc any` would therefore claim one
alias name — `CS1537 the using alias 'testFnc' appeared previously in this namespace` — whether they
sit in one file or in two of the same compilation. `archive/tar`'s suite is the shape: `testFnc` is
declared in `writer_test.go`'s `TestWriter` **and** `TestFileWriter`, and again in `reader_test.go`'s
`TestFileReader`, with `fileMaker` alongside it.<!-- three diagnostics held every one of the package's 97 verdicts at
the time. --> The naming half of `liftLocalTypeDecl` is the shared `liftLocalTypeDeclName`, and the
alias branch calls it when `v.inFunction`, emitting `global using TestWriter_testFnc = object;`.

The reference mapping is registered under a **guard**, `liftedTypeDeclaredBy`: only a `*types.Named`
or `*types.Alias` whose own `Obj` **is** this declaration qualifies. A wrong key here renames every
reference to an unrelated type — `type X = Header` inside a function binds the declaration's object
to the *existing* `Header`, and (without materialized aliases) `type X = int` binds it to plain
`int`, so keying the lift on either would rewrite every `Header`, or every `int`, in the file.
Anything that does not qualify registers nothing and keeps its ordinary rendering. A function-local
declaration is also not published in `exportedTypeAliases`: it is not part of the package's
exported surface whatever its Go name looks like, and after the lift the name a consumer would
import does not exist. **No production-corpus impact by construction** — an AST scan of the Go
1.26.10 standard library finds *no* function-local alias or defined-over-a-named-type declaration in
any production source file outside `testdata` (and `cmd`), so only test suites meet the shape.
<!-- history: the original scan was of the Go 1.23.1 sources: 50 hits, all internal/types/testdata; two test suites met
the shape. Re-scanned for Go 1.26.10 on 2026-10-25 (go/parser over GOROOT/src, non-_test.go files, testdata, vendor
and cmd skipped): 70 function-local type declarations, none an alias and none defined over an Ident or selector. -->
(Guarded by the `LocalTypeAliasScope` behavioral test — the same local
names declared in two functions of one file and again in a second file of the same package, plus a
real `type hdr = Header` alias whose target is used bare alongside it; without the lift five
`global using` lines duplicate.)

**A package-level alias of an *unnamed composite* roots every type ARGUMENT, not only the outermost
name.** A using-alias RHS is resolved at compilation scope, without reference to other using
directives, so `type names = []string` emits `global using names = go.slice<go.@string>;`, and a
nested `slice`, `error`, `complex64`, a same-package `Header` (`go.main_package.Header`) and a foreign
`io_package.Reader` (`go.io_package.Reader`) are each rooted the same way; a C#-keyword element
(`byte`, `long`) stays the keyword. (Guarded by `PackageAliasRootedTypeArgs`.)
<!-- history: this paragraph recorded an open residual (only the outermost name rooted, CS0246 on every nested
argument); the PackageAliasRootedTypeArgs golden pins the rooted form at 3e3f18925a, e.g.
src/tests/Behavioral/PackageAliasRootedTypeArgs/main.cs.target:1 `global using names = go.slice<go.@string>;`. -->

A *conversion expression* to a hoisted local named **pointer** type (`NodePtr(&Node{V: 9})`, with
`type NodePtr *Node` declared in function `main`) names the lifted type, `new main_NodePtr(Ꮡ(new Node(V: 9)))`,
as the composite kinds do (a local `Tally(m)` renders `(main_Tally)m`). The `LocalNamedTypeDecls` guard
uses the assignment form `var np NodePtr = &Node{V: 9}`, so the conversion form has no guard of its own.
<!-- history: this paragraph recorded a known residual, the conversion emitting the pre-hoist name new NodePtr(…)
(CS0246); a probe converted at 3e3f18925a on linux/amd64 emits new main_NodePtr(Ꮡ(new Node(V: 9))) and
(main_Tally)m. -->

## An embedded field's NAME is the UNQUALIFIED type name (dot-imported embeds)

An embedded struct field's name is, per the Go spec, the *unqualified* type name. A cross-package
embed written as a selector (`struct{ io.Writer }`) already stripped its qualifier for the field
name; a **dot-imported** embed (`import . "io"` then embedded `ReaderFrom`) reaches the emitter as a
bare `*ast.Ident`, yet `getAliasQualifiedTypeName` renders it package-qualified — and, once the package is a
collision-rename, as `Δio.ReaderFrom`. Kept in the field name (`internal io_package.ReaderFrom Δio.ReaderFrom;`),
that qualifier's dot is a C# syntax error (`CS1003 '(' expected` / `CS1026 ') expected'`), so
`visitStructType` strips to the last segment whenever the resolved embedded-type name
carries a qualifier (covering both the selector and dot-imported-ident forms; a same-package embed
has no dot), yielding `public io_package.ReaderFrom ReaderFrom;`. io's own test suite has the shape
(`io_test.go`'s `import . "io"`), and its converted test file carries both the field and the
`using Δio = io_package;` alias.
<!-- history: this paragraph said the io test suite remained blocked by import . "io" using-alias resolution issues (the
Δio namespace emitted but never aliased); at 3e3f18925a src/core/io/io_test.cs:19 `using Δio = io_package;` and :26
`/*embed*/ public io_package.ReaderFrom ReaderFrom;`, and io is a validated row. -->

## Select statement lowering (terminating and empty clauses)

A `select` lowers to a C# `switch` over a golib runtime call that commits exactly ONE case and returns its ordinal: the blocking form `switch (select(ᐸꟷ(a, ꓸꓸꓸ), …))` (selectgo — commits a uniformly-random ready case or parks), and the default form `switch (trySelect(…))` (the same poll pass, returning -1 so the C# `default:` label runs when no case is ready). Receive cases keep a `case N when selᴛN.ꟷᐳ(out v):` guard that consumes the committed value; send cases are performed by the runtime commit and get a bare `case N:` label. Every case's operands are hoisted into select-scoped temps (`var selᴛN = …;`) emitted in strict source order and evaluated exactly once at select entry — a receive case's channel operand (used by BOTH the registration and the guard) and a send case's whole registration call, so the registration list names only temps (see [Every case's operands are hoisted](#every-cases-operands-are-hoisted--evaluated-exactly-once-in-source-order-at-select-entry)). The registration calls (`ᐸꟷ(ch, ꓸꓸꓸ)` receive, `ch.ᐸꟷ(v, ꓸꓸꓸ)` send) return `SelectOp` case descriptors and `select(params SelectOp[])` runs a faithful selectgo (see [Real channel runtime](#real-channel-runtime--the-hchanselectgo-port-rendezvous-caplen-single-fire-uniform-random)): it commits exactly ONE ready case — chosen uniformly at random — or parks until one becomes ready. A committed receive's value crosses to the winning case's unchanged guard (`case N when ch.ꟷᐳ(out v):`) through a per-thread pending-frame stack the guard pops (a stack, so a select nested in the guard's target expression cannot destroy the outer commit — see the same section), so the emitted select text needs no part of its own for the hand-off. Two structural completions (io pipe.go's `read`):

* **An EMPTY clause body still needs its jump.** C# requires every switch section to end in a jump statement (CS8070 on a final `default:`, CS0163 otherwise). The flag that suppresses the trailing `break;` after a terminal `return` resets per *clause*, not per statement (an empty body has no statement to reset it), so a bare Go `default:` after a returning clause emits `default: { break; }`.
* **A terminating blocking select gets an unreachable trailing `return default!;`.** Go's spec makes a select with no `default:` whose every comm-clause body ends in a terminating statement itself terminating, so a value-returning function may end with it. The lowered form's guarded `case N when <recv>:` labels cannot prove exhaustiveness to C# (CS0161). Mirroring the switch guarded-terminal-default rule, the emission appends `return default!;` after the closing brace — gated on: no default, every clause terminating (`isTerminatingStmtList`, conservative), no select-targeting `break`, a value-returning signature, and not named-return-defer mode (void wrapper).

The golib non-blocking receive underpinning the default-form guards distinguishes the two "no value" cases per Go semantics: a **closed** empty channel is receive-ready with the zero value; an **open** empty channel reports not-ready, so the `default:` is taken. (Guarded by the `SelectStatement` extensions `firstMsg` — terminal blocking select in a value-returning func — and `poll` — empty `default:` after a returning case, polled both before and after `close`.)

## A NIL channel is never ready — and asking must not throw

golib models a channel as a **struct** holding a reference to its core, so the nil channel is that
struct's ZERO value: the core reference is null. Go gives a nil channel well-defined behavior — it is
never closed, a receive or send on it blocks forever, and in a `select` with a `default` the nil case
is simply not chosen — so every probe must *report* "not ready" rather than dereference the absent
core: `IsClosed` is `false`, `Length` and `Capacity` are 0, and the non-blocking send and receive
report not-ready.
<!-- src/core/golib/channel.cs:1610 `public bool IsClosed => m_core is not null && m_core.Closed;`.
history: the pre-hchan channel null-checked its backing fields in SendIsReady / ReceiveIsReady / Receiving, but
IsClosed did not, so asking whether a nil channel was closed threw a NullReferenceException; that crashed every
child process os/exec launched through a background context and was the last blocker on math/rand's
TestDefaultRace. -->

This is not an exotic shape. os/exec's `Start` runs

```go
if c.ctx != nil {
	select {
	case <-c.ctx.Done():
		return c.ctx.Err()
	default:
	}
}
```

and `context.Background().Done()` **is** a nil channel, so a probe that dereferenced it would crash
every child process launched through a background context. With `IsClosed` reporting `false` for a
nil channel, the non-blocking receive falls through to "not ready" and the `default:` clause runs,
matching Go. (Guarded by `NilChannelSelectDefault`:
nil receive and comma-ok receive taking the default, `len`/`cap` of a nil channel, a real channel
behaving normally alongside, and a mixed select where the nil case must never win over a ready real
case.)

## The default form routes through trySelect — send cases are unguarded in both forms

Per-case try-operation guards in a `switch (ᐧ)` (`case ᐧ when ch.ꟷᐳ(out v):` /
`case ᐧ when ch.ᐸꟷ(v, ꟷ):`) would be single-fire by construction (C# evaluates the ordered guards until
the first true), but their ready-case choice is the **case order**, never uniform-random — unfixable
against an ordered C# `switch`. The default form therefore routes through golib's non-blocking
`trySelect(…)`: the same registrations as the blocking form, the same selectgo poll pass (distinct
cores locked in Id order, Fisher-Yates pollorder, exactly one commit under the held locks), no
parking, and -1 — the default sentinel matched by the C# `default:` label — when no case is ready:
<!-- history: the default form was first lowered as switch (ᐧ) with an unguarded case ᐧ:, which ran unconditionally and
silently dropped os/signal's process send; the per-case try-operation guards were the interim fix before trySelect. -->

```go
select {
case c <- sig:
default:   // send but do not block for it
}
```

<!-- source: src/core/os/signal/signal.cs:268-275 -->
```csharp
var selᴛ1 = c.ᐸꟷ(sig, ꓸꓸꓸ);
switch (trySelect(selᴛ1)) {
case 0: {
    break;
}
default: {
    break;
}}
```

Send cases get a bare `case N:` label in BOTH forms — the runtime call performed the winning send
itself, so a guard would either send the value a second time or fail and silently skip the chosen
clause body. Receive cases keep their `case N when ch.ꟷᐳ(out v):` guard, which consumes the
committed value from the runtime's per-thread pending-frame stack. Go's remaining rules live in the
runtime: a **closed** channel's send case panics (Go panics even when a `default:` exists — the
poll pass checks `closed` before readiness, so a closed FULL channel panics rather than taking the
default), and a **nil** channel's case is never ready, so it is never chosen. golib's non-blocking
`Sent`/`ᐸꟷ(v, ꟷ)`/`TrySend` surface remains for direct non-blocking sends (and the `-uco=false`
named-method mode), delegating to the same single runtime send implementation.

A pointer-element channel needs two more rules. net/rpc's `func (call *Call) done()` sends
`call.Done <- call`: (1) the capture-mode pre-pass counts a send-value position, so
`bodyUsesReceiverAsPointerValue` recognizes a `SendStmt` whose value is the pointer receiver and
promotes the method to direct-ж, giving it a receiver box to hand out; and (2) `convSendValueExpr`
applies the pointer ident context for every pointer element, not only *interface* elements, so a
deref-aliased pointer (a pointer parameter, or the receiver) renders as its box and binds the
`in ж<T>` send parameter (rendered as its value it is CS1503). Both forms of send route through
`convSendValueExpr`, so the statement form `ch <- recv` follows the same rule.

(Guarded by `SelectSendDefault`: full buffered taking the default then the same select succeeding
once drained, free-capacity buffered delivering the value, unbuffered with a waiting receiver, nil,
closed-panics-through-the-default, one-ready-among-several, a send and a receive case with neither
ready, exactly-one-send when several are ready, and a no-default select still blocking.)

## Every case's operands are hoisted — evaluated exactly once, in SOURCE ORDER, at select entry

Go's spec evaluates, for every case in the statement, a receive operation's channel operand and a
send statement's channel AND right-hand-side expressions exactly once, in source order, upon
entering the select. Leaving an operand inline in the `select(…)`/`trySelect(…)` registration
argument list breaks that in two distinct ways.

**Evaluated twice.** A receive case's operand appears in the registration call AND again as the
winning guard's receiver, and C# reads a struct method call's receiver AFTER evaluating its
arguments, so even a bare identifier can change under the guard (the out-target expression runs
first). A non-referentially-stable operand — `case <-time.After(d):` (net/http/pprof),
`case <-fresh():`, or an identifier the out-target reassigns — re-evaluates to a DIFFERENT channel:
the runtime's pending-frame core match then (correctly) refuses delivery, and the factory's side
effect runs twice.

**Evaluated out of order.** C# evaluates the registration arguments in argument order, i.e. AFTER
every hoisted temp. A send case left inline therefore had its channel operand and value expression
observed after a *later* receive case's operand: a select whose FIRST case was a send observed
`[recv-chan, send-chan, send-val]` where Go's order is `[send-chan, send-val, recv-chan]`.

The converter therefore hoists EVERY case's operands into select-scoped temps, emitted in strict
source order, leaving the registration list naming only temps. Uniformly, with no stability
analysis — `channel<T>` struct copies share one core, so the temp preserves identity, and the hoist
IS Go's up-front-once evaluation model:

```go
select {
case v := <-fresh():
    ...
}
```

```csharp
var selᴛ1 = fresh();
switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
case 0 when selᴛ1.ꟷᐳ(out var v): {
    ...
    break;
}}
```

A SEND case hoists its **whole registration call** rather than two separate operand temps
(`SelectSendRecvMix`, a send case textually first on a full channel plus a receive case on the
same channel):

```go
select {
case ch <- 8:
    fmt.Println("send fired on full channel (wrong)")
case took = <-ch:
}
```

<!-- source: src/tests/Behavioral/SelectSendRecvMix/main.cs:33-42 -->
```csharp
var selᴛ3 = ch.ᐸꟷ(8, ꓸꓸꓸ);
var selᴛ4 = ch;
switch (select(selᴛ3, ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
case 0: {
    fmt.Println("send fired on full channel (wrong)");
    break;
}
case 1 when selᴛ4.ꟷᐳ(out took): {
    break;
}}
```

That is both legal and stronger than two operand temps. `Sending`/`ᐸꟷ(v, ꟷ)` only BUILDS a
`SelectOp` descriptor — golib's `Sending` is `return new SelectOp(m_core, isSend: true, sendValue:
value);` — and the communication is performed later by the runtime commit inside
`select`/`trySelect`, so moving the call ahead of the switch moves no send. The call evaluates its
receiver then its argument, i.e. channel operand then value expression, contiguously and in source
order: exactly Go's rule. And the value expression keeps its ORIGINAL argument position, so every
implicit conversion the `in T` parameter applies — untyped-constant narrowing to the element type,
interface-adapter wrap, `@string`/`nint` boxing, array clone (see the send-value rules above) — is
preserved by construction, with no new type inference anywhere. A separate value temp would have to
re-render the element type to declare itself, and `var` inference is provably wrong there:
`case bch <- 200:` on a `chan byte` becomes `var t = 200;` — an `int`, which does not convert to
`byte` at the call (CS1503) — and any divergence in a hand-rendered element type would SILENTLY
change the conversion instead. The whole-call hoist also leaves the `ж<T>`-pointer element case
(net/rpc's `call.Done <- call`) unaffected by construction.

A send case's winning label stays a bare `case N:` — the runtime commit performed the send, so it
carries no guard and nothing re-evaluates.

(Guarded by `SelectOperandOnceEval` for the once-only property — ready and parked call-expression
operands with printed call counters, the reassigned-identifier out-target, and the default form;
an inline operand FailFasts it on the pending-frame core-match assert.
And by `SelectOperandSourceOrder` for the ordering property — a send case textually first with all
three operand expressions logging their fixed source positions: the blocking form with the receive
winning and with the send winning, a default-form select interleaving send/receive/send with nothing
ready (including an untyped `200` into a `chan byte` and a value boxed into a `chan any`), and the
already-correct receive-first direction as a regression anchor. Every select there is deterministic
by construction — exactly one case can ever be ready — so the uniform-random commit never affects
the output. Inline send operands print `3:recv-chan 1:send-chan 2:send-val` where Go prints
`1:send-chan 2:send-val 3:recv-chan`.)

## Known exposure: marker-shaped USER identifiers can collide with synthetic names

The converter's synthetic-name markers — `ᴛ` (TempVarMarker, U+1D1B: `selᴛ1`, `tupleᴛ2`,
`elemᴛ0`, `iᴛ1`, `initᴛ<name>`, lifted-type `<name>ᴛ1`), `ʗ` (CapturedVarMarker), `Δ`
(ShadowVarMarker), and the rest of the Symbols.cs family — are exotic Unicode LETTERS, legal in
Go identifiers. A Go program that itself declares an identifier matching a generated shape (e.g.
`selᴛ1` used in a `select`, emitting `var selᴛ1 = selᴛ1;`) collides with the synthetic name —
loudly, at C# compile time (CS0128/CS0102), never silently. A general Δ-rename of user
identifiers matching the numbered-temp shape was attempted at the sanitizer choke point
(`getCoreSanitizedIdentifier`) and REJECTED: that choke point also renders the converter's own
synthetic names (loop temps `iᴛ1`, lifted anonymous/named-value types `main_MyBoolᴛ1`,
cross-file anon-struct names), so the blanket rule Δ-renamed synthetic names too and churned
non-select goldens; distinguishing user from synthetic identifiers requires threading origin
through many naming call sites — deliberate sprawl for a trigger that demands typing U+1D1B in
Go source. Accepted as a documented family-wide exposure: the failure mode is a compile error
naming the colliding identifier, and the workaround is renaming the pathological identifier in
the Go source.

## Real channel runtime — the hchan/selectgo port (rendezvous, cap/len, single-fire, uniform-random)

golib's `channel<T>` is written over a faithful port of Go's runtime machinery, which gives it
unbuffered rendezvous, a `make(chan T)` distinct from `make(chan T, 1)`, a blocking select that
performs exactly one send case, and a uniform-random choice among ready cases:
<!-- history: these were four long-standing channel-semantics gaps, closed together by the rewrite
(docs/phase4/DESIGN-channels.md; rendezvous and the select rework landed as one unit because staging rendezvous first
regressed the legacy Sending path). -->

* **`ChanCore<T>` is the `hchan` analog**: a Monitor lock, a circular `T[]` buffer (null when
  `dataqsiz == 0`), `sendx`/`recvx`/`qcount`, `closed`, intrusive `recvq`/`sendq` parked-waiter
  queues, and a monotonic `Id` (the total lock order for select). The `channel<T>` struct holds only
  a reference to its core, so the zero value is the NIL channel and struct copies share one channel.
  `chansend`/`chanrecv`/`closechan` follow Go's routines exactly, including the buffered-full
  parked-sender head-take/tail-enqueue rotation and drain-before-zero comma-ok (a closed channel
  yields its remaining buffered values with `ok == true` first, then `(zero, false)`).
* **`make(chan T)` emits `new channel<T>(0)`** — capacity 0 is a real rendezvous channel; `cap()` is
  `dataqsiz` and `len()` is `qcount`, so `make(chan T)` vs `make(chan T, 1)` are distinct
  (the make default in `convCallExpr.go` covers plain and named channels; the gen Channel template's
  wrapper constructor forwards the size unclamped). A parked operation blocks its goroutine's
  own dedicated thread (`Goroutine.Start`), so parking costs nobody else's capacity and a program
  can park thousands of goroutines at once — the shape `GoroutineParkStorm` guards. The thread is
  dedicated for the goroutine's life and reused after it (`GoroutineThreadPool`, as Go reuses an M),
  because creating a thread is costly and does not get faster with more creators; a goroutine that
  finds no idle thread still gets a new one, so reuse changes what a `go` statement costs, never
  how many can run.
  <!-- src/core/golib/runtime/GoroutineThreadPool.cs:21 `costs on the order of 100 µs to create and hand-shake`
  (platform not recorded; the same comment measures 6-9.5k threads a second on a 16-thread laptop).
  history: golib once queued goroutines on the shared ThreadPool and raised its min-thread floor to
  max(256, 4 × processor count); both retired with the dedicated-thread executor
  (docs/phase4/DESIGN-cooperative-scheduler.md). -->
* **Blocking select is a selectgo port behind the unchanged emitted text.** The registration
  methods (`Receiving`, `Sending`, `ᐸꟷ(ch, ꓸꓸꓸ)`, `ch.ᐸꟷ(v, ꓸꓸꓸ)`) return type-erased `SelectOp`
  descriptors — invisible to overload resolution at every emitted call site — and
  `select(params SelectOp[])` partitions out nil channels (never registered), locks the distinct
  cores in `Id` order, scans a Fisher-Yates-shuffled poll order, and **commits exactly one ready op
  under the held locks** (uniform-random single-fire); otherwise it parks one
  `SelectState`-linked waiter per case, where a single `winner` CAS is the single-fire authority
  every waker — plain send, plain receive, another select's commit, AND close — must win before
  touching a waiter. Publish-before-signal ordering and park-outside-the-lock discipline throughout.
* **The committed receive value crosses to the guard via a per-thread pending-frame STACK** (not a
  single slot): `select`/`trySelect` push a frame (channel core, value, ok) on a receive commit,
  and the winning guard (`Received`/`ꟷᐳ`) pops exactly the frame whose core matches its own
  channel. A stack because the guard's out-argument TARGET expression is evaluated BEFORE the
  guard call, and legal Go can run another select there (`case a[f()] = <-ch:` where `f()`
  selects) — the inner select pushes and pops its own frames, so the outer commit survives; a
  single slot was destroyed by the inner select's entry (outer value lost, or the next buffered
  value stolen). Only receive commits push frames; a
  send-case win touches nothing (a select may have send and receive cases on the SAME channel, and
  clearing would destroy an outer frame mid-nest). Known residual: a panic unwinding between
  commit and consume strands a frame — unbounded under a repeated panic-in-target/recover loop,
  an accepted benign memory residual. The stack must never be CAPPED: live depth is dynamic, not
  textual — one textual select whose out-target expression recurses holds one live frame per
  recursion level, so a depth cap silently drops live outer frames (the `DeepSelectRecursion`
  guard, 100 levels, fails a depth-64 cap). Frames are never MIS-consumed (every
  consume matches the top frame by channel core); a strand stacked above a live frame makes the
  outer select fire zero cases — the committed value is abandoned exactly as the panic abandoned
  the communication, never delivered wrongly. Debug-only depth warnings, never a process-killing
  assert. With no matching frame the same guards are non-blocking probes, unchanged.
* **A channel may have an OWNING TIMER** (Go's `hchan.timer`), the hook Go 1.23's synchronous timer
  channel needs: `IChannelTimer` is installed by `channel<T>.AttachTimer` before the timer is armed,
  `Capacity`/`Length` report 0 while the owner answers `HidesBuffer` (Go's `chanlen`/`chancap`
  timer-channel branch), and `DrainBuffer()` — Go's `runtime.timerchandrain` — empties the buffer
  without servicing parked waiters, so the owner can REVOKE a value the channel already accepted.
  It is the only sanctioned way to un-send, and only sound for a channel whose producer owns it
  exclusively; `IsUnbuffered` keeps reporting the physical shape. Full semantics under
  [Realizing the runtime TIMER contract](manual-conversions.md#realizing-the-runtime-timer-contract-sleep--newtimer--stoptimer--resettimer).
* **Close/panic semantics are Go's**: send on closed panics (even from within a select, and even
  when a `default:` exists); close of closed and close of nil panic; a parked select-send woken by
  close panics on its own thread; parked receivers (plain and select) wake with `(zero, false)`;
  range-over-channel terminates on closed-and-drained; `len`/`cap` of nil are 0. A boxed
  `IChannel`'s nil comparison is representation nilness (`channel is null`) — a
  `{ Length: 0, Capacity: 0 }` pattern would misclassify a live empty unbuffered channel as nil.

(Guarded by `ChannelRendezvous` — cap/len 0, not-ready probes with no counterpart, rendezvous
round-trip, ping-pong alternation; `ChannelCapLen` — buffered fill/wrap/drain, nil/unbuffered
len/cap, comma-ok drain-after-close; `SelectSingleFire` — exactly one delivery among multiple ready
send cases, 100-iteration volume guard; `SelectSendRecvMix` — send+recv cases on the same channel,
one-commit-per-select; `SelectRandomFairness` — both branches of a two-ready select taken over 200
iterations; `CloseWakesBlocked` — close waking parked receivers/senders/selects in both directions
plus the whole panic family; `NilChannelInSelect` — nil cases never ready beside live ready and
parked cases; plus `NamedChannelType`'s unbuffered named-channel rendezvous and the rest of the
select/channel behavioral tests.)

## An escaping comm-clause binding receives into a temp and heap-boxes at clause entry

A `case result := <-ch:` whose bound variable's address is taken in the clause body — internal/fuzz
`coordinatorLoop`'s `c.crashMinimizing = &result` and `writeToCorpus(&result.entry, …)` — escapes to
the heap, so the body's address-of emission references the `Ꮡresult` box companion (an escaping `:=`
local's form). A plain `out var result` in the comm-clause label would leave `Ꮡresult` undeclared
(CS0103), and the `when` guard's `out var` slot cannot declare a ref local, so `selectCommBinding`
(`visitSelectStmt.go`) receives into a uniquely-numbered temp and opens the clause body with the
entry-time box pattern proven by the escaping-parameter preamble:

<!-- source: src/core/internal/fuzz/fuzz.cs:268-269 -->
```csharp
case 2 when selᴛ3.ꟷᐳ(out var resultᴛ1): {
    ref var result = ref heap(resultᴛ1, out var Ꮡresult);
```

The gate is `identHasHeapBox` — the exact predicate the body's `&name` emission uses — so the box is
declared iff it is referenced; alias/box names mirror `convertToHeapTypeDecl` (sanitized analyzed name
for the value alias, raw analyzed name behind `Ꮡ` for the box, matching `boxBaseName`). Both bindings
of the `(val, ok)` form are checked. A non-escaping binding keeps the direct `out var <name>` form
(preserving the shadow-rename render, e.g. `out var errΔ5`), and an ASSIGN-mode rebind of an existing
boxed local already writes through its ref alias.<!-- at the fix: the full-stdlib A/B footprint was exactly
internal/fuzz/fuzz.cs, and the CS0103 x2 were the last own-errors keeping internal/fuzz red. --> (Guarded by
`SelectEscapeBinding` — escaping binding written through both
directions, escaping `(val, ok)` binding with a field address through the box, and a mixed
escaping/plain select, output-compared vs Go; without the temp it fails with the
CS0103 `Ꮡres` class. A clause taking ONLY a field address (`&res.value`, no whole-var `&res`) still
copy-boxes — the known assignment-position escape-analysis gap, out of scope here.)

---

[← Strings (`@string` and `sstring`)](strings.md) · [Index](README.md) · [Generic Constraints →](generic-constraints.md)
