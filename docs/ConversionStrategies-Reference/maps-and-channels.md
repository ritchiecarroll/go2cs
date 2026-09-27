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

golib's `map<K,V>` used to hand out that enumerator directly, so every legal Go range-with-insert
became a runtime fault. It now implements Go's contract itself: the range takes a **snapshot of the
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
unspecified and deliberately randomized, so the position is free.

**One key shape makes the visit-time lookup the wrong instrument, and it is a real Go shape rather
than a curiosity.** A [NaN key is equal to nothing, itself included](#a-nan-map-key-is-never-equal-to-anything-itself-included),
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
`"Foo"` — a new key. The exception escaped the handler goroutine, the Phase-4 test host's
containment policy absorbed it as a test failure, the h2 stream was therefore never completed with
its trailers and `END_STREAM`, and the client blocked in `http2pipe.Read` forever. That was the
deterministic hang of `TestServerUndeclaredTrailers/h2`, and it stalled the whole `net/http` row —
the hang, not any divergence, is what left the rest of the suite unreached. Guarded from the Go side
by `tests/Behavioral/MapMutateDuringRange`, which covers insert, overwrite, delete, insert-with-delete,
a control that mutates a *different* map, and the NaN-key shapes; and at the golib level by
`tests/GolibTests/MapRangeMutationTests.cs`, which pins what the Go side cannot reach — the nil-key
entry's participation, and that a map *without* a nil key never yields a phantom entry.

## `m[string(b)]` — the map-READ key does not copy (`tmpstring`)

The Go compiler special-cases `m[string(b)]`: because a map lookup hashes and compares its key but
never retains it, the `[]byte`→`string` conversion's result provably does not outlive the index
expression, and the copy is skipped (`runtime.slicebytetostringtmp`). The converted C# paid that copy
on every probe — one backing `byte[]` per call — which is exactly the allocation
`net/textproto.TestCommonHeaders`' want-ZERO `testing.AllocsPerRun` assert measures over
`canonicalMIMEHeaderKey`'s common-header probe (L11). The converter now recognizes the same shape and
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
m[default!] = nilKeyˢ;
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
`ToString`). `range` yields the nil entry ahead of the buckets — Go's range order is unspecified and
deliberately randomized, so the position is free, and every map *without* a nil key stays on the
dictionary's own enumerator unwrapped.

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
small. Measured: `PerfMap` (`map[int]int`) is flat — 276.4 ms with the slot vs 271.8 ms without
(median of three 9-run sessions each), inside the 260–310 ms run-to-run band the *unchanged* build
spans on the same machine.

One consumer cannot see the slot and had to be threaded explicitly: `reflect.DeepEqual` walks the
backing `IDictionary` through a reflected field probe, which never yields a nil key, and a lone nil
entry does not necessarily show up in the `Len` comparison either (one extra ordinary key on the
other side hides it). `IMap` therefore exposes a non-generic `NilKeyEntry` — `(present, boxed value)`
— with a default implementation on `IMap<TKey, TValue>` that asks the comma-ok indexer, so the
generated named-map wrappers satisfy it with no go2cs-gen change; DeepEqual compares that entry
before the dictionary walk. (Guarded by `NilMapKey`: set/get/comma-ok/overwrite/delete/`len`/`range`/
`clear` on `map[any]string`, a nil-key composite literal, `map[error]int`, and the nil-key reads on
both a nil and an empty map, all output-compared vs `go run`. Before the fix the very first
`m[nil] = …` threw `ArgumentNullException`, which is how sync's `TestIssue40999` died as an
infrastructure error.)

**PRINTING a nil-key map is a second such consumer, one layer down.** `fmt` orders map keys through
`internal/fmtsort`, which walks the map with `reflect.Value.MapRange` and compares the key Values —
and the reflection bridge typed each entry from its BOXED OBJECT, which for the nil key is `null`, so
`Key()` handed back the **invalid zero Value**. `fmtsort.compare` cannot compare that at all: it reads
`aVal.Type()` first thing and falls through to `panic("bad type in compare: " + aType.String())` on a
nil type, so printing *any* map carrying a nil key died inside `fmt`. Go's rule is the slot rule the
bridge already applies to struct fields and slice elements — **a map entry Value is typed by the map's
DECLARED key/value type**, so `map[any]V` hands out Kind Interface keys whatever the dynamic value is,
and a nil key or value is a VALID nil Value of that type. `MapIter` now carries the map's key and value
types (plus the map Value's read-only bits) and `Key()`/`Value()` build through `makeTypedValue`, which
also makes `fmtsort`'s nil-compares-low rule reachable for the first time — a nil key sorts first,
exactly as Go prints it. (Guarded two ways: `NilMapKey` gains the printing shapes — `map[any]int` with
a nil key through both `Println` and `%v`, a nil-only map, `map[error]int`, `map[*int]int` — and the
new `ReflectMapRangeNilKey` drives the bridge directly over `map[any]int`, a named `map[any]int`,
`map[*int]string`, `map[error]int`, a concrete `map[string]int`, a slice-valued map and a nil
interface VALUE, printing only order-independent facts because Go's map iteration order is
unspecified. Pre-fix it reports `any: 0 0 1 5 3` — zero interface-kind keys, zero nil keys, ONE
invalid key, and a value sum of 5 instead of 6 because the nil entry was skipped — against Go's
`any: 3 1 0 6 3`.)

## An INTERFACE map key compares by Go equality, never by adapter identity

Go compares interface values by (dynamic type, dynamic value), and that **one** relation serves both
`==` and map-key lookup: a `map[Iface]V` finds an entry under exactly the values `==` calls equal. In
the conversion the two had diverged. Emitted `==`/`!=` route through golib's
[`builtin.AreEqual`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/builtin.cs),
which unwraps the three generated adapter tiers (`IInterfaceAdapter`, `IжAdapter`, `IValueAdapter`)
before comparing — but `map<K,V>`'s backing `Dictionary<TKey,TValue>` used the **default** comparer and
compared the *wrappers*.

That gap is observable because an interface value's wrapper is not stable. The same Go dynamic value is
presented through whichever adapter the static interface currently holding it calls for, so asserting an
`Object` to a narrower `dependency` yields a **different** wrapper object over the same receiver box.
Under the default comparer the asserted value could no longer find its own entry in the map it came out
of — while `==` on the very same pair still answered `true`, because `AreEqual` unwrapped. Equal but
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

This is `go/types`' own `initorder.dependencyGraph`, and it is why the converted type checker could not
type-check **any** source: the missed lookup returned a nil `ж<declInfo>`, and `~` on it nil-panicked
one frame later — surfacing through `check.cs:430`, `handleBailout`'s faithful re-panic of Go's own
`default: panic(p)` arm, which is a bailout frame and not the fault site.

The fix is in golib and centralizes on the relation that already existed:
[`GoEqualityComparer`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoEqualityComparer.cs)
projects `AreEqual` as an `IEqualityComparer<TKey>`, hashing the **unwrapped root** so the hash stays
consistent with it — the same rule the compile-time `ImplementGenerator` adapters already applied
(`m_box.GetHashCode()`), which the runtime shells never had. It is installed only for key types that can
actually carry an adapter (`typeof(TKey).IsInterface`, or `any`); a concrete key — `@string`, an
integer, a converted struct — is never wrapped and keeps `EqualityComparer<TKey>.Default`'s
devirtualized path, the test being a JIT-time constant per instantiation. Restating the relation inside
each generated shell was rejected for the reason the defect illustrates: `AreEqual` is golib's single
definition of Go equality, and a second copy per shell class is exactly the drift that produced this.
(Guarded by the `InterfaceAssertionMapKey` behavioral test — pointer- and value-receiver implementors,
an `Object` that is not a `dependency`, lookup after narrowing, lookup after re-widening, and the
`Object(d) != obj` identity probe that pinned the equal-but-unfindable split.)

The same relation gained its last carrier on 2026-08-19 (the crypto/tls regression): golib's own
`error<T>` — the hand-written generic shell for `error`, the one shell go2cs-gen does not emit — had
never joined the `IInterfaceAdapter` unwrap protocol its generated siblings define, so `AreEqual`
could see through every carrier EXCEPT it and two independently minted carriers of the same error
value compared by reference. The shape needs two minters for one value, which the white-box test
model makes routine: crypto/tls's production code never casts `AlertError` to `error` (it only boxes
it into `any`), so fmt's `%w` assert resolved the runtime shell, while the test assembly's
`errors.Is` target arrived as its own generated `ᴠ` value adapter — and
`errors.Is(err, AlertError(alertBadCertificate))` answered false for the very alert `quicError` had
wrapped (`TestQUICHandshakeError`). `error<T>` now carries the identical member every generated
shell does: the `ж` box when pointer-backed (Go pointer-identity equality), the wrapped value
otherwise. (Guarded by `GolibTests.ErrorShellCarrierEqualityTests` — two shells over one value, the
pointer-identity flavor, and the protocol membership itself.)

## A NaN map key is never equal to anything, itself included

BCL `Double.Equals` reports NaN equal to NaN, deliberately, so that a NaN stored in a collection can
be found again. Go applies `==` unchanged, so a NaN key is equal to nothing: `m[NaN] = 1` twice stores
TWO entries, and neither can ever be read back or deleted. `fmt`'s own `TestSprintf` reads the
difference out of `%v` of a map — `map[NaN:1]` against Go's `map[NaN:1 NaN:1]`.

`GoEqualityComparer.ForKeys` therefore supplies a per-representation, non-boxing comparer for the four
float representations, whose entire implementation is `==` — because C#'s float `==` IS the IEEE
relation Go's map applies. The hash stays the type's own, and a NaN that hashes consistently while
comparing unequal builds exactly the same-bucket/never-equal chain Go's map builds for it. Scoped to
the raw representations: a NAMED float type's wrapper, and a struct or array that CONTAINS a float,
still compare through their generated equality and inherit the BCL rule. No measured consumer reaches
those, and covering them would mean routing every struct-keyed map through the reflective relation.

## Named map types and constrained map access

A defined map type — `type Grades map[string]int` — emits the `[GoType("map[K, V]")] partial struct` forward declaration (completing the long-standing `visitMapType` stub), implemented by go2cs-gen's Map template: full forwarding of `IMap<K, V>` (including the two-value comma-ok indexer), `IDictionary<K, V>`, enumeration, and the `ISupportMake` factory through the wrapped `map<K, V>`. Its composite literal wraps the concrete map literal in the named constructor — `new Grades(new map<@string, nint>{["a"u8] = 1})` — mirroring named arrays/slices (a direct indexer-initializer would target a default wrapper with no backing dictionary; the old emission produced Go-style `key: value` inside C# braces — CS1513). Comma-ok indexing works through a **constrained map type parameter** too: `v, ok := m[k]` where `M ~map[K]V` detects the map CORE of the constraint (both at the assignment's tuple gate and in the index emission) and routes the same `m[k, ꟷ]` two-value indexer, which lives on `IMap<K, V>` itself. The **nil comparison** `m == nil` — Go's only legal map comparison, maps.Clone's nil-preserve guard — emits the `IMap.IsNil` property (`if (m.IsNil)`; backing-store null, distinct from an allocated empty map — no operator exists on a type parameter, CS8761), and `delete(m, k)` on a constrained map binds a golib `delete(IMap<K, V>, K)` overload (key/value types infer from the interface conversion). (Guarded by the `GenericTypeInference` extension `EqualMaps` — a maps.Equal clone over a named map type through the constraint, comma-ok + comparable-erased equality, values vs Go.)
For source-generated named-map wrappers, the generator parses the `[GoType("map[K, V]")]` payload at the top-level comma, not every comma in the string. This matters for function-valued maps: `type opTable map[CrossPkgLib.Ticks]func(int, int) int` emits `map<global::go.CrossPkgLib_package.Ticks, Func<nint, nint, nint>>`, preserving the full delegate as the value type. Any source-file alias used inside the `[GoType]` payload is resolved through Roslyn and rewritten to its fully-qualified target before the template emits `IMap<K, V>`, `IDictionary<K, V>`, and `ICollection<KeyValuePair<K, V>>`; generated files therefore do not depend on file-local package aliases such as `using token = ...`. (Guarded by `NamedMapCrossPkgKey`.)

The named arm must also **carry the map type into the key/value slot emission**, exactly as the
unnamed arm does. Every `MapSource` slot rule in `convKeyValueExpr` is gated on that type: a pointer
KEY or VALUE boxes to `Ꮡx` rather than aliasing a deref'd value into a `ж<T>` slot (CS0029), an `any`
key or value slot re-renders a string literal as `(@string)"…"u8` instead of the bare `u8` span (which
has no conversion to an `object` slot — CS1503), an untyped-constant `any` key boxes at Go's default
type so the store and every lookup agree on the boxed type, and an array-typed key clones into the
map. Left nil, a *named* map type silently opted out of all of them, so `type namedAny map[any]int`
with `namedAny{nil: 1, "b": 2}` emitted `["b"u8] = 2` and failed to compile while the identical
**unnamed** `map[any]int{nil: 1, "b": 2}` one line above emitted `[(@string)"b"u8] = 2`. (Guarded by
the `DeepEqual` behavioral test's named-map block, which is written over `map[any]int` precisely so
the `any`-slot rule is exercised through a named type.)

A bare **`make(Grades)` with no size argument** defaults the size to 0 — emitting `new Grades(0)` — so the wrapper's allocating `(nint size)` constructor runs and the backing dictionary is created. The generated wrapper struct has that `(nint size)` constructor but no *parameterless* one, so a plain `new Grades()` would be `default(Grades)` — a **nil** map (null backing store, so `m == nil` is true and a write panics), whereas Go's `make` returns a **non-nil empty** map (`m == nil` false, writes succeed). The default is applied only to `*types.Named` defined types: the unnamed `map<K, V>` builtin already allocates in its own parameterless constructor and stays `new map<K, V>()`, and a type *alias* (`type M = map[int]int`) resolves to that builtin rather than a wrapper — so neither drifts (`make` emission in `convCallExpr.go`, right beside the named-channel default below). This mirrors the named-channel unbuffered default (`make(closeWaiter)` → `new closeWaiter(1)`); a sized `make(Grades, n)` (already `new Grades(n)`) and the `Grades{}` composite literal are non-nil already. (Guarded by `NamedMapMakeNonNil` — `make` with and without a size, a plain nil `var`, and a composite literal, each `== nil`-compared and output-compared vs Go.)

Two `[GoType]` payload conventions coexist, and the generator's alias substitution must tell them
apart. The map/channel emitters write dotted types in **source-alias form** (`CrossPkgLib.Ticks`,
via `getAliasQualifiedTypeName`), which the substitution above resolves; the slice/array element and
defined-over-selector emitters write the **namespace-qualified form** (`io.fs_package.FileInfo`,
via `getFullyQualifiedTypeName`), which roots through the `go` namespace and must pass through untouched.
The telltale is the segment after the leading identifier: a real alias maps to a package *class*,
so its next segment is a type name — a `_package`-suffixed next segment means the leading
identifier is a namespace segment that merely *collides* with a file alias. net/http's fs.go
aliases `io` while declaring `type fileInfoDirs []fs.FileInfo` → `[]io.fs_package.FileInfo`;
substituting the `io.` produced the nonexistent `go.io_package.fs_package.FileInfo` (CS0426 ×48).
The substitution skips exactly those occurrences (a negative lookahead on `_package.`). On the
converter side, the namespace-qualified form must lead with the **canonical** qualifier, never a
file-local Δ collision-rename: a consumer whose own namespace has a same-named child imports under
`using ΔIoLike = IoLike_package;`, but `[]ΔIoLike.FsLike_package.Info` resolves nowhere in the
alias-free `.g.cs` — `canonicalizeQualifierRename` reverts a leading import-rename segment
(mirroring the visitTypeSpec global-using-target rule). (Guarded by `NamedSliceChildPkg` — a
nested-namespace consumer package importing both `IoLike` and `IoLike/FsLike`, with a named slice
of the subpackage's type used across the assembly boundary.)

A map indexed by a **non-empty interface key** converts a concrete key expression through the same interface-adapter path used by assignments and call arguments. For example, `seen[item] = "kept"` where `seen` is `map[Node]string` and `item` is `*Item` emits `seen[new ItemжNode(item)] = "kept"u8`; the comma-ok read emits the same adapter for the key, `seen[new ItemжNode(item), ꟷ]`. This records the pointer implementation (`GoImplement<Item, Node>(Pointer = true)`) and keeps dictionary lookup semantics aligned with Go's interface key identity. Empty-interface map keys keep their existing literal handling (`map[any]...` turns string literals into Go strings rather than UTF-8 spans), and pointer-typed map keys keep the direct pointer-box path. (Guarded by `InterfaceMapKeyPointer`.)

A **pointer-keyed** map indexed by the method's **receiver** supplies the receiver's box as the key, exactly like the deref-aliased pointer-parameter case: `t.m[c]` inside `func (c *conn) …` emits `t.m[Ꮡc]` (plain read, write, and the comma-ok `t.m[Ꮡc, ꟷ]` alike) — net/http transport.go's idle-connection bookkeeping (`t.idleLRU.m[pc]`) passed the deref-aliased VALUE where `ж<persistConn>` was expected (CS1503 plus the `(v, ok)` deconstruction cascade). The box exists only on a **direct-ж** method, so the receiver-as-map-key body shape itself now *promotes* the method to direct-ж (`bodyUsesReceiverAsPointerValue` gained an `IndexExpr` case, gated on a pointer-KEYED map operand) — a method whose only pointer-use of its receiver is the map key still gets `this ж<conn> Ꮡc`. A pointer LOCAL is unchanged (it *is* the key — no `Ꮡ`), and `delete(t.m, c)` boxes through the ordinary pointer-argument rule once the method is direct-ж. (Guarded by `PtrKeyMapReceiverLookup` — pure-shape promotion, plain read/write, comma-ok, and delete through two distinct receiver identities, values vs Go.)

A value sent into a channel of **non-empty interface element type** converts through the same
interface-adapter path used by assignments and call arguments — the send emission previously tested
the CHANNEL type itself for interface-ness (never true, its underlying is `*types.Chan`), so no
conversion ever fired. A value implementation sends bare while recording the implement pair for the
generator (`vs.ᐸꟷ(new dog(name: "rex"u8))` with `[assembly: GoImplement<dog, speaker>]`); a pointer
implementation wraps the box in its generated pointer adapter:

```go
ps := make(chan speaker, 1)
c := &cat{name: "tom"}
ps <- c
```

```csharp
var ps = new channel<speaker>(1);
var c = Ꮡ(new cat(name: "tom"u8));
ps.ᐸꟷ(new catжspeaker(c));   // records GoImplement<cat, speaker>(Pointer = true)
```

A pointer-typed send value renders as its box (parity with the argument-position rule in
`convExprList`), and a type-parameter element (`chan T` in generic code) keeps the bare emission.
The string-literal empty-interface arm of the same helper is described under
[Empty Interface (`any`)](empty-interface.md#empty-interface-any). (Guarded by `AnyStringLitChanSend` — a value impl and a pointer
impl sent through a `chan speaker`, method-dispatched on receive, output-compared vs Go.)

## Named channel types

A defined channel type — `type closeWaiter chan struct{}` (net/http's h2 bundle) — emits the
`[GoType("chan T")] partial struct` forward declaration (completing the long-standing
`visitChanType` stub; the whole corpus previously had NO `GoType("chan …")` — CS0246 at every use),
implemented by go2cs-gen's Channel template: the wrapper holds a `channel<T>` and forwards its full
surface — the Go-visual send/receive members (`ᐸꟷ`, `ꟷᐳ`, including the select-registration
`ᐸꟷ(v, ꓸꓸꓸ)`/`Sending`/`Receiving` forms), the comma-ok `Receive(ꟷ)`/`Received` pair, `IChannel`'s
object-typed members, enumeration for `range`, the `ISupportMake` factory, and a `(nint size)`
constructor so `make(closeWaiter)` emits `new closeWaiter(0)` — a REAL unbuffered channel (the make
path resolves the chan through `Underlying()`, giving named channels the same unbuffered default as
plain `chan T`; the wrapper constructor forwards the size unclamped, so named channels can be
unbuffered — see the channel-runtime section below):

```go
type closeWaiter chan struct{}
func (cw *closeWaiter) Init() { *cw = make(closeWaiter) }
func (cw closeWaiter) Close() { close(cw) }
func (cw closeWaiter) Wait()  { <-cw }
```

```csharp
[GoType("chan EmptyStruct")] partial struct closeWaiter;

[GoRecv] internal static void Init(this ref closeWaiter cw) {
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

**The ALIAS kind takes the lift too — and for a different reason.** A local declaration that emits a
`using` ALIAS rather than a nested type — a real `type X = Y`, or a defined type over a *named*
interface such as `type X any` — was the last local type-declaration kind not taking the hoist. It
needs no member-level redirection (an alias is emitted at file scope either way), but it needs the
NAME, because the alias it writes is a `global using`: scoped to the whole **compilation**, not to
the file, let alone the function. Two functions declaring `type testFnc any` therefore claimed one
alias name — `CS1537 the using alias 'testFnc' appeared previously in this namespace` — whether they
sat in one file or in two of the same compilation. `archive/tar`'s suite is the shape: `testFnc` is
declared in `writer_test.go`'s `TestWriter` **and** `TestFileWriter`, and again in `reader_test.go`'s
`TestFileReader`, with `fileMaker` alongside it; three diagnostics held all **97** of that package's
verdicts. The naming half of `liftLocalTypeDecl` is now the shared `liftLocalTypeDeclName`, and the
alias branch calls it when `v.inFunction`, emitting `global using TestWriter_testFnc = object;`.

The reference mapping is registered under a **guard**, `liftedTypeDeclaredBy`: only a `*types.Named`
or `*types.Alias` whose own `Obj` **is** this declaration qualifies. A wrong key here renames every
reference to an unrelated type — `type X = Header` inside a function binds the declaration's object
to the *existing* `Header`, and (without materialized aliases) `type X = int` binds it to plain
`int`, so keying the lift on either would rewrite every `Header`, or every `int`, in the file.
Anything that does not qualify registers nothing and renders exactly as before. A function-local
declaration is also no longer published in `exportedTypeAliases`: it is not part of the package's
exported surface whatever its Go name looks like, and after the lift the name a consumer would
import does not exist. **Zero production-corpus impact by construction** — an AST scan of the Go
1.23.1 sources finds *no* function-local alias-or-defined-over-interface declaration in any compiled
stdlib file (all 50 hits are `internal/types/testdata`, which is never built), which is why only two
test suites ever met it. (Guarded by the `LocalTypeAliasScope` behavioral test — the same local
names declared in two functions of one file and again in a second file of the same package, plus a
real `type hdr = Header` alias whose target is used bare alongside it; the unfixed converter emits
five duplicate `global using` lines.)

**Known residual, a different one, in the same emission line:** an alias whose target is an *unnamed
composite* renders its type ARGUMENTS unrooted — `type names = []string` emits `global using names =
go.slice<@string>;`, where only the outermost name is rooted and `@string`, a nested `slice`,
`error`, `complex64`, a same-package `Header` and a foreign `io_package.Reader` all arrive bare and
do not resolve at compilation scope (`CS0246`). This is **package-level**, not function-local, and
predates the lift above; `getUsingAliasSafeTypeName` exists for exactly this class of problem
(a using-alias RHS is resolved without reference to other using directives) but rewrites only the
csproj-level golib name aliases, never the rooting. No converted stdlib package declares such an
alias, so the corpus has never reached it; a converted user module would.

**Known residual:** a *conversion expression* to a hoisted local named **pointer** type
(`NodePtr(&Node{V: 9})`, with `type NodePtr *Node` declared in the function) still emits the
pre-hoist source name (`new NodePtr(…)`, `CS0246`). The composite kinds do not have this — a local
`Tally(m)` correctly renders `((main_Tally)m)` — so the gap is specific to the named-pointer
conversion arm's target-name resolution. It was previously masked by the hard syntax error above and
has no consumer among the measured packages (gob only *declares* `Rec` and takes its address); the
`LocalNamedTypeDecls` guard therefore uses the assignment form `var np NodePtr = &Node{V: 9}`.

## An embedded field's NAME is the UNQUALIFIED type name (dot-imported embeds)

An embedded struct field's name is, per the Go spec, the *unqualified* type name. A cross-package
embed written as a selector (`struct{ io.Writer }`) already stripped its qualifier for the field
name; a **dot-imported** embed (`import . "io"` then embedded `ReaderFrom`) reaches the emitter as a
bare `*ast.Ident`, yet `getAliasQualifiedTypeName` still renders it package-qualified — and, once the package is a
collision-rename, as `Δio.ReaderFrom`. Gating the qualifier-strip on the *selector* form left that
qualifier in the field name (`internal io_package.ReaderFrom Δio.ReaderFrom;`), whose embedded dot is
a C# syntax error (`CS1003 '(' expected` / `CS1026 ') expected'` — the reported io `io_test`
defect). `visitStructType` now strips to the last segment whenever the resolved embedded-type name
carries a qualifier (covering both the selector and dot-imported-ident forms; a same-package embed
has no dot, so it is a byte-identical no-op), yielding the correct `public io_package.ReaderFrom
ReaderFrom;`. (This is one root among several in the io test suite, which remains blocked by separate
`import . "io"` using-alias resolution issues — the `Δio` namespace is emitted but never aliased.)

## Select statement lowering (terminating and empty clauses)

A `select` lowers to a C# `switch` over a golib runtime call that commits exactly ONE case and returns its ordinal: the blocking form `switch (select(ᐸꟷ(a, ꓸꓸꓸ), …))` (selectgo — commits a uniformly-random ready case or parks), and the default form `switch (trySelect(…))` (the same poll pass, returning -1 so the C# `default:` label runs when no case is ready). Receive cases keep a `case N when selᴛN.ꟷᐳ(out v):` guard that consumes the committed value; send cases are performed by the runtime commit and get a bare `case N:` label. Every case's operands are hoisted into select-scoped temps (`var selᴛN = …;`) emitted in strict source order and evaluated exactly once at select entry — a receive case's channel operand (used by BOTH the registration and the guard) and a send case's whole registration call, so the registration list names only temps (see the operand-evaluation section below). The registration calls (`ᐸꟷ(ch, ꓸꓸꓸ)` receive, `ch.ᐸꟷ(v, ꓸꓸꓸ)` send) return `SelectOp` case descriptors and `select(params SelectOp[])` runs a faithful selectgo (see the channel-runtime section below): it commits exactly ONE ready case — chosen uniformly at random — or parks until one becomes ready. A committed receive's value crosses to the winning case's unchanged guard (`case N when ch.ꟷᐳ(out v):`) through a per-thread pending-frame stack the guard pops (a stack, so a select nested in the guard's target expression cannot destroy the outer commit — see the channel-runtime section), so the emitted select text is identical to the pre-redesign form. Two structural completions (io pipe.go's `read`):

* **An EMPTY clause body still needs its jump.** C# requires every switch section to end in a jump statement (CS8070 on a final `default:`, CS0163 otherwise); the emitted `break;` was suppressed when the *previous* clause ended in a terminal `return` (the was-return flag is reset per statement, and an empty body has none). The flag resets per *clause* now — a bare Go `default:` emits `default: { break; }`.
* **A terminating blocking select gets an unreachable trailing `return default!;`.** Go's spec makes a select with no `default:` whose every comm-clause body ends in a terminating statement itself terminating, so a value-returning function may end with it. The lowered form's guarded `case N when <recv>:` labels cannot prove exhaustiveness to C# (CS0161). Mirroring the switch guarded-terminal-default rule, the emission appends `return default!;` after the closing brace — gated on: no default, every clause terminating (`isTerminatingStmtList`, conservative), no select-targeting `break`, a value-returning signature, and not named-return-defer mode (void wrapper).

The golib non-blocking receive underpinning the default-form guards distinguishes the two "no value" cases per Go semantics: a **closed** empty channel is receive-ready with the zero value; an **open** empty channel reports not-ready, so the `default:` is taken. (Guarded by the `SelectStatement` extensions `firstMsg` — terminal blocking select in a value-returning func — and `poll` — empty `default:` after a returning case, polled both before and after `close`.)

## A NIL channel is never ready — and asking must not throw

golib models a channel as a **struct**, so the nil channel is that struct's ZERO value: every field
is null. Go gives a nil channel well-defined behavior — it is never closed, a receive or send on it
blocks forever, and in a `select` with a `default` the nil case is simply not chosen — so the
readiness probes must *report* "not ready" rather than dereference the absent state. Most of them
already did (`SendIsReady` / `ReceiveIsReady` / `Receiving` all null-check their backing fields);
`IsClosed` did not, so merely asking whether a nil channel was closed threw a
`NullReferenceException`.

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

and `context.Background().Done()` **is** a nil channel, so every child process launched through a
background context crashed in the probe — the last blocker on math/rand's `TestDefaultRace`.
`IsClosed` now reports `false` for a nil channel, which makes the non-blocking receive fall through
to "not ready" and the `default:` clause run, matching Go. (Guarded by `NilChannelSelectDefault`:
nil receive and comma-ok receive taking the default, `len`/`cap` of a nil channel, a real channel
behaving normally alongside, and a mixed select where the nil case must never win over a ready real
case.)

## The default form routes through trySelect — send cases are unguarded in both forms

The `default:` form was originally lowered as `switch (ᐧ)` with per-case try-operation guards
(`case ᐧ when ch.ꟷᐳ(out v):` / `case ᐧ when ch.ᐸꟷ(v, ꟷ):` — the interim fix for the dropped-send
defect os/signal's `process` exposed, where an unguarded `case ᐧ:` ran unconditionally and silently
dropped the value). That shape is single-fire by construction (C# evaluates the ordered guards until
the first true) but its ready-case choice is the **case order**, never uniform-random — provably
unfixable against an ordered C# `switch`. The default form now routes through golib's non-blocking
`trySelect(…)`: the same registrations as the blocking form, the same selectgo poll pass (distinct
cores locked in Id order, Fisher-Yates pollorder, exactly one commit under the held locks), no
parking, and -1 — the default sentinel matched by the C# `default:` label — when no case is ready:

```go
select {
case c <- sig:
default:   // send but do not block for it
}
```

```csharp
switch (trySelect(c.ᐸꟷ(sig, ꓸꓸꓸ))) {
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

A pointer-element channel forced two further root fixes, both pre-existing and both previously
unreachable because the dropped send never compiled the value expression. net/rpc's
`func (call *Call) done()` sends `call.Done <- call`: (1) the capture-mode pre-pass had no
send-value position, so the method was never promoted to direct-ж and had no receiver box to hand
out — `bodyUsesReceiverAsPointerValue` now recognizes a `SendStmt` whose value is the pointer
receiver; and (2) `convSendValueExpr` applied the pointer ident context only for *interface*
elements, so a deref-aliased pointer (a pointer parameter, or the receiver) rendered as its value
and could not bind the `in ж<T>` send parameter (CS1503). Both forms of send route through
`convSendValueExpr`, so the statement form `ch <- recv` — broken in exactly the same way — is fixed
by the same change.

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
`case bch <- 200:` on a `chan byte` becomes `var t = 200;` — an `int`, which no longer converts to
`byte` at the call (CS1503) — and any divergence in a hand-rendered element type would SILENTLY
change the conversion instead. The whole-call hoist also leaves the `ж<T>`-pointer element case
(net/rpc's `call.Done <- call`) unaffected by construction.

A send case's winning label stays a bare `case N:` — the runtime commit performed the send, so it
carries no guard and nothing re-evaluates.

(Guarded by `SelectOperandOnceEval` for the once-only property — ready and parked call-expression
operands with printed call counters, the reassigned-identifier out-target, and the default form;
counter-proven against the pre-fix emission, which FailFasts on the pending-frame core-match assert.
And by `SelectOperandSourceOrder` for the ordering property — a send case textually first with all
three operand expressions logging their fixed source positions: the blocking form with the receive
winning and with the send winning, a default-form select interleaving send/receive/send with nothing
ready (including an untyped `200` into a `chan byte` and a value boxed into a `chan any`), and the
already-correct receive-first direction as a regression anchor. Every select there is deterministic
by construction — exactly one case can ever be ready — so the uniform-random commit never affects
the output. Counter-proven against the pre-fix converter, which prints
`3:recv-chan 1:send-chan 2:send-val` where Go prints `1:send-chan 2:send-val 3:recv-chan`.)

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

The four long-standing channel-semantics gaps (no unbuffered rendezvous; `make(chan T)` conflated
with `make(chan T, 1)`; a blocking select performing EVERY send case; first-match instead of
uniform-random ready choice) were closed together by rewriting golib `channel<T>` over a faithful
port of Go's runtime machinery (`docs/phase4/DESIGN-channels.md` — the blessed synthesized design;
rendezvous and the select rework land as ONE unit because staging rendezvous first regresses the
legacy `Sending` path):

* **`ChanCore<T>` is the `hchan` analog**: a Monitor lock, a circular `T[]` buffer (null when
  `dataqsiz == 0`), `sendx`/`recvx`/`qcount`, `closed`, intrusive `recvq`/`sendq` parked-waiter
  queues, and a monotonic `Id` (the total lock order for select). The `channel<T>` struct holds only
  a reference to its core, so the zero value is the NIL channel and struct copies share one channel.
  `chansend`/`chanrecv`/`closechan` follow Go's routines exactly, including the buffered-full
  parked-sender head-take/tail-enqueue rotation and drain-before-zero comma-ok (a closed channel
  yields its remaining buffered values with `ok == true` first, then `(zero, false)`).
* **`make(chan T)` emits `new channel<T>(0)`** — capacity 0 is a real rendezvous channel; `cap()` is
  `dataqsiz` and `len()` is `qcount`, so `make(chan T)` vs `make(chan T, 1)` are finally distinct
  (the make default in `convCallExpr.go` covers plain and named channels; the gen Channel template's
  wrapper constructor no longer clamps `size < 1` to 1). A parked operation blocks its goroutine's
  own dedicated thread (`Goroutine.Start`), so parking costs nobody else's capacity and a program
  can park thousands of goroutines at once — the shape `GoroutineParkStorm` guards. golib used to
  queue goroutines on the shared ThreadPool and raise its min-thread floor to
  `max(256, 4 × processor count)` to compensate; both the floor and its premise retired with the
  dedicated-thread executor (`docs/phase4/DESIGN-cooperative-scheduler.md`).
* **Blocking select is a selectgo port behind the unchanged emitted text.** The registration
  methods (`Receiving`, `Sending`, `ᐸꟷ(ch, ꓸꓸꓸ)`, `ch.ᐸꟷ(v, ꓸꓸꓸ)`) return type-erased `SelectOp`
  descriptors — invisible to overload resolution at every emitted call site — and
  `select(params SelectOp[])` partitions out nil channels (never registered), locks the distinct
  cores in `Id` order, scans a Fisher-Yates-shuffled poll order, and **commits exactly one ready op
  under the held locks** (uniform-random single-fire, gaps 3+4); otherwise it parks one
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
  value stolen — found by the adversarial verification round). Only receive commits push frames; a
  send-case win touches nothing (a select may have send and receive cases on the SAME channel, and
  clearing would destroy an outer frame mid-nest). Known residual: a panic unwinding between
  commit and consume strands a frame — unbounded under a repeated panic-in-target/recover loop,
  an accepted benign memory residual. The stack must never be CAPPED: live depth is dynamic, not
  textual — one textual select whose out-target expression recurses holds one live frame per
  recursion level, so a depth cap silently drops live outer frames (the `DeepSelectRecursion`
  guard, 100 levels, falsified an attempted depth-64 cap). Frames are never MIS-consumed (every
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
  `IChannel`'s nil comparison is representation nilness (`channel is null`) — the old
  `{ Length: 0, Capacity: 0 }` pattern would misclassify a live empty unbuffered channel as nil.

(Guarded by `ChannelRendezvous` — cap/len 0, not-ready probes with no counterpart, rendezvous
round-trip, ping-pong alternation; `ChannelCapLen` — buffered fill/wrap/drain, nil/unbuffered
len/cap, comma-ok drain-after-close; `SelectSingleFire` — exactly one delivery among multiple ready
send cases, 100-iteration volume guard; `SelectSendRecvMix` — send+recv cases on the same channel,
one-commit-per-select; `SelectRandomFairness` — both branches of a two-ready select taken over 200
iterations; `CloseWakesBlocked` — close waking parked receivers/senders/selects in both directions
plus the whole panic family; `NilChannelInSelect` — nil cases never ready beside live ready and
parked cases; plus the extended `NamedChannelType` unbuffered named-channel rendezvous and the
pre-existing select/channel suite.)

## An escaping comm-clause binding receives into a temp and heap-boxes at clause entry

A `case result := <-ch:` whose bound variable's address is taken in the clause body — internal/fuzz
`coordinatorLoop`'s `c.crashMinimizing = &result` and `writeToCorpus(&result.entry, …)` — escapes to
the heap, so the body's address-of emission references the `Ꮡresult` box companion (an escaping `:=`
local's form). The comm-clause label emitted only a plain `out var result`, never a box, leaving
`Ꮡresult` undeclared (CS0103 ×2, the last own-errors keeping internal.fuzz red after its CS0234s
cleared). The `when` guard's `out var` slot cannot declare a ref local, so `selectCommBinding`
(`visitSelectStmt.go`) receives into a uniquely-numbered temp and opens the clause body with the
entry-time box pattern proven by the escaping-parameter preamble:

```csharp
case 2 when (~c).resultC.ꟷᐳ(out var resultᴛ1): {
    ref var result = ref heap(resultᴛ1, out var Ꮡresult);
```

The gate is `identHasHeapBox` — the exact predicate the body's `&name` emission uses — so the box is
declared iff it is referenced; alias/box names mirror `convertToHeapTypeDecl` (sanitized analyzed name
for the value alias, raw analyzed name behind `Ꮡ` for the box, matching `boxBaseName`). Both bindings
of the `(val, ok)` form are checked. A non-escaping binding keeps the direct `out var <name>` form
(preserving the shadow-rename render, e.g. `out var errΔ5`), and an ASSIGN-mode rebind of an existing
boxed local already writes through its ref alias — the full-stdlib A/B footprint was exactly
internal/fuzz/fuzz.cs. (Guarded by `SelectEscapeBinding` — escaping binding written through both
directions, escaping `(val, ok)` binding with a field address through the box, and a mixed
escaping/plain select, output-compared vs Go; the pre-fix converter fails it with exactly the
CS0103 `Ꮡres` class. A clause taking ONLY a field address (`&res.value`, no whole-var `&res`) still
copy-boxes — the known assignment-position escape-analysis gap, out of scope here.)

---

[← Strings (`@string` and `sstring`)](strings.md) · [Index](README.md) · [Generic Constraints →](generic-constraints.md)
