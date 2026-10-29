# Slices and Arrays
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#slices-and-arrays)
Go slices and arrays are converted to the golib [`slice<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/slice.cs) and [`array<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/array.cs) structures. A `make`-style allocation uses a constructor; a composite literal builds a C# array and projects it with the `.slice()` / `.array()` extension:

```go
package main

import "fmt"

func main() {
    primes := [6]int{2, 3, 5, 7, 11, 13}   // array literal
    nums := []int{10, 20, 30}              // slice literal
    buf := make([]byte, 4)                 // make
    fmt.Println(primes[0], nums[2], len(buf))
}
```

converts to:

<!-- illustration: not converter output -->
```csharp
internal static void Main() {
    var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
    var nums = new nint[]{10, 20, 30}.slice();
    var buf = new slice<byte>(4);
    fmt.Println(primes[0], nums[2], len(buf));
}
```

A **named** slice/array type (`type d [3]rune`, `type s []int`) lowers to a struct wrapping `array<T>`/`slice<T>`; its composite literal cannot use C# collection-initializer braces (the lowered struct has no `Add`), so it is constructed through the underlying-collection constructor: `d{0, 32, 0}` → `new d(new rune[]{0, 32, 0}.array())`.

The **empty** composite of such a type is its **zero value**, not a one-element literal: the generic named-composite
`nil` filler (a named non-struct composite's NilType constructor argument; a named *struct* reads `new T()`) does not apply inside an
element literal, where it would put a `NilType` element in a `byte[]` (CS0029). An empty **array** composite emits a
zeroed *fixed-length* backing — `tmpBuf{}` (`type tmpBuf [32]byte`, runtime `string.go`'s `*buf = tmpBuf{}`) →
`new tmpBuf(new byte[32].array())` — because Go's `[N]T{}` is a full-length zero array (an empty `{}` literal would
produce a length-0 backing); an empty named-**slice** composite emits an empty non-nil backing — `pm{}` →
`new pm(new uint32[]{}.slice())`, so `pm{} == nil` is `false`, as in Go
([Nil-vs-empty slice identity](#nil-vs-empty-slice-identity-s--nil-is-representation-nilness-not-emptiness)). (Guarded
by the `NamedArrayWrapper` extension — empty array composite read/written at full length, zeroing an existing wrapper
through a pointer via `*buf = tb{}`, and an empty slice composite appended to; values vs Go.) <!-- history: the nil
filler once landed inside the element literal, `new tmpBuf(new byte[]{nil}.array())`. -->

**Reslicing SHARES the backing array — golib `slice<T>` stores capacity.** A Go reslice is a view adjustment, never a
copy: writes through `s[a:b]` are visible through `s` (and vice versa), `append` within capacity writes the shared
backing in place, and only `append` beyond capacity reallocates and detaches. The emitted forms are the C# range indexer
for 2-index expressions (`s[a:b]` → `s[a..b]`, `s[a:]` → `s[a..]`, `s[:b]` → `s[..b]`, `s[:]` → `s[..]`) and the golib
`.slice(low, high, max)` extension for 3-index expressions (`s[a:b:c]` → `s.slice(a, b, c)`; a missing low is the `-1`
sentinel). Both take bounds **relative to the view** (the source slice may itself sit at a non-zero offset in its
backing array), default a missing high to `len(s)` — the range indexer resolves a from-end `Index` against the slice *length*,
so `s[1..]` is Go's `s[1:]` even when `len < cap` — allow high up to `cap(s)`, and panic Go-style
(`RuntimeErrorPanic.SliceBoundsOutOfRange`) when out of range. To represent a capacity-restricted 3-index view
(`s[a:b:c]`, `c` below the backing array's end) without copying, `slice<T>` stores `m_capacity` as its own field rather
than deriving it from the backing array's length; the `slice<T>(T[] array, nint low, nint high, nint max)` constructor
builds such views. <!-- history: golib once copied on any reslice that did not span the whole backing array, derived
capacity from the backing end, and mis-measured Available/Append for non-zero-offset views. --> (Guarded by the
`SliceAliasing` behavioral test — `copy` into and element writes through a `low>0` reslice, reslice-of-reslice offset
compounding, slice-of-array reslices, restricted-capacity 3-index writes, in-place vs reallocating `append`, all read
back through the base and value-compared vs Go.)

**A `make` length/capacity/size-hint of a non-`int` integer type is cast to `nint`.** The golib allocating constructors all take `nint` — `slice<T>(nint length, nint capacity)`, `map<K,V>(nint capacity)`, `channel<T>(nint capacity)` — and C# does **not** implicitly convert a `uintptr`/`uint`/`uint32`/`uint64`/`int64` (C# `nuint`/`uint`/`ulong`/`long`) to `nint`. So `make([]byte, n/goarch.PtrSize)` with a `uintptr` length would leave `new slice<byte>(n / …)` with no applicable constructor, and overload resolution falls onto `slice<T>(T[])` — reported as `CS1503` ("cannot convert `nuint` to `byte[]`"); a map/chan with a `uintptr` hint is a direct `nuint`→`nint` CS1503. The converter casts each such length/capacity/hint argument to `nint`: `new slice<byte>((nint)(n / goarch.PtrSize))` (both args of `make([]byte, l, c)`), `new map<K,V>((nint)(hint))`, `new channel<T>((nint)(size))`. A plain `int` (`nint`) and an untyped-constant argument (`make([]byte, 4)` → a bare `4`) bind directly and are **left uncast** (no golden churn) — as are the widening `int8`/`int16`/`uint8`/`uint16` kinds. (Guarded by the `MakeSliceUintptrLen` behavioral test — `uintptr`/`uint`/`uint32`/`uint64` slice lengths, a `uintptr` len+cap, a `uintptr` map hint and chan size, and int/untyped controls, all `len`/`cap`/element values verified vs Go; runtime hits this in `mbitmap`'s `make([]byte, n/goarch.PtrSize)`.)

**Slicing a pointer-to-array.** Go lets a `*[N]T` be sliced directly — `p[lo:hi:max]`, `p[:]` — auto-dereferencing the
array. A box over an anonymous array, `ж<array<T>>`, is sliced through golib's `slice` extension overloads on the box,
every form a call: `p[1:3:4]` → `p.slice(1, 3, 4)`, `p[:]` → `p.slice()`, `p[2:]` → `p.slice(2)`. Each overload
dereferences a managed array and slices a box over native memory through its element window, and the resulting slice shares the array's backing storage, matching Go. A box over a **named** array type
dereferences through `.Value` and slices the wrapper: `g[1:3]` → `g.Value[1..3]`. (The `(*[N]T)(ptr)[:n]`
pointer-*cast* form is different — see *Pointer-cast slice* below.) A deref-aliased pointer **parameter** or
**receiver** is emitted as the pointed-to *value*, not a box. When that value is a **named** array type — `b *pageBits`
emitted `ref pageBits b`, where `pageBits` is `[N]uint64` — its underlying `array<T>` is reached via `.Value`:
`b[:2]` → `b.Value[..2]`. When it is an **anonymous** array (`p *[N]T` → `ref array<T> p`) the value already *is* the
`array<T>` and is sliced directly (`p[:]` → `p[..]`). (Guarded by the `PointerArraySlice` behavioral test — local box,
named-array receiver, and named-array parameter; runtime hits this in `select.go`'s `cas1[:ncases:ncases]` /
`mprof.go`'s `stk[:n:n]` (locals) and `mpallocbits.go`'s `pageBits` receiver methods (`clear(b[:])`).) An index-only
`for i := range p` over a `*[N]T` reads no element: a box passes the type's length beside the pointer (`range(p, N)`),
so a nil `p` ranges N times as in Go, and a deref-aliased pointer parameter or receiver ranges the count itself
(`range(N)`) — see
[An index-only range over a slice or array reads no element](labels-and-loop-variables.md#an-index-only-range-over-a-slice-or-array-reads-no-element).

**Named-slice pointer reinterpret (`(*[][]byte)(buf)` with `buf *Buffers`).** Go converts a pointer-to-named-slice to a pointer to its underlying slice type freely — net's `fd.pfd.Writev((*[][]byte)(buf))`, where poll's `Writev` *reslices the header through the pointer* (`consume` advances `*v`), and the caller must observe it. The C# boxes `ж<Buffers>` and `ж<slice<slice<byte>>>` are unrelated generic instantiations (CS0030), so the conversion emits a **field view over the wrapper's own backing slice**:

<!-- source: src/core/net/windows/fd_windows.cs:250 -->
```csharp
    var (n, err) = Ꮡfd.of(netFD.Ꮡpfd).Writev(Ꮡbuf.of(Buffers.Ꮡm_value));
```

Two generator pieces make the view real aliasing: a named-slice wrapper's `m_value` is **mutable** (`ReadOnlyValue = false` — a readonly field would force a defensive copy and lose header writes), and `ISliceTypeTemplate` emits the field-ref accessor `internal static ref slice<T> Ꮡm_value(ref Buffers instance)` that `ж<T>.of()` projects through. Claimed narrowly: pointer→pointer, source pointee a NAMED type whose underlying is a slice identical to the (unnamed) target pointee. (Guarded by the `SortArrayType` extension `consumeOne` — a `(*[]Person)(&crew)` reinterpret whose reslice through the view shrinks the original `Roster`, runtime-verified against Go.)

The **reverse** direction — an underlying-slice pointer to a NAMED-slice pointer, `(*Buffer)(&b)` with `type Buffer []byte` (log/slog/internal/buffer's `sync.Pool.New`) — is **asymmetric**, because the projection above cannot run backwards: a named-wrapper box *contains* the underlying slice (project it out), but a bare-slice box does **not** contain a wrapper to project. It is emitted as golib's **storage reinterpret** instead — `Ꮡb.Reinterpret<slice<byte>, Buffer>()` — which re-views the *same slot* as the wrapper type rather than constructing anything: a generated named-slice wrapper is a single-field struct over the slice header, exactly the layout correspondence `ReinterpretAliasesStorage` recognizes, so the managed alias arm engages and the derived pointer ALIASES the addressed slice. A bare `(ж<Buffer>)(Ꮡ(b))` cast is CS0030 (unrelated instantiations). The source comes two ways and both render in BOX form (the `isPointer` ident context): an **address-of** arg (`&b`, `&h.field`) and an **existing pointer** arg (cryptobyte's `(*String)(out)` with `out *[]byte`). Claimed narrowly by the mirror gate: pointer→pointer, target pointee a NAMED type whose underlying is a slice identical to the (unnamed) source pointee.

A wrapper box built over a copy — `Ꮡ(new Buffer(b))` — would be right only if the reinterpret were used through the
returned pointer alone. That is false wherever the conversion exists precisely to write BACK, which is the shape's
dominant corpus use, and the writes would go nowhere:

| Site | What a copy would cost |
|:--|:--|
| `log/slog` `commonHandler.withAttrs` — `(*buffer.Buffer)(&h2.preformattedAttrs)` | Every attribute `WithAttrs` pre-formatted would be dropped while the handler still advanced `groupPrefix`/`nOpenGroups`, so the JSON it emitted afterwards would be unbalanced (four `testing/slogtest` cases: `WithAttrs`, `multi-With`, `empty-group-record`, `resolve-WithAttrs`). |
| `crypto/tls` `readUint{8,16,24}LengthPrefixed` — `(*cryptobyte.String)(out)` with `out *[]byte` | An out-PARAMETER whose whole purpose is the write-back: the caller's slice would never receive the length-prefixed field. |
| `crypto/tls` `parseECHConfigList` — `(*cryptobyte.String)(&ec.PublicKey)` | A struct FIELD that would stay empty after a successful parse. |
| `vendor/…/cryptobyte` `ReadASN1Bytes` — `(*String)(out)` | Same out-parameter shape, same loss. |

The aliasing form also needs no `ref var @out = ref Ꮡout.DerefOrNull()` local for an out-parameter source. <!-- history:
the form was once a wrapper box over a copy; the correction's corpus A/B touched log/slog/handler.cs,
log/slog/internal/buffer/buffer.cs, crypto/tls/ech.cs, crypto/tls/handshake_messages.cs and
vendor/golang.org/x/crypto/cryptobyte/asn1.cs (measured at the landing: 5 files, 8 sites). --> (Guarded by the
`NamedSlicePointerReinterpret` behavioral test — a direct `(*Buf)(&b)` **read back through `b`**, the closure-returned
`func() *Buf { … return (*Buf)(&s) }` shape, a pointer-**parameter** `fillVia(out *[]byte)` read back through `out`, and
a struct-**field** `(*Buf)(&h.preformatted)` appended across a reallocating growth and then truncated through a freshly
derived pointer — output-compared vs Go.)

**Pointer-cast slice (`(*[N]T)(ptr)[:n]`).** A Go conversion that casts an `unsafe.Pointer` to a pointer-to-array and
slices it produces a `[]T` over the pointed-to memory. It is emitted as the golib **`slice<T>`** — the C# representation
of *every* `[]T` — built from a `ReadOnlySpan<T>` over the raw pointer:
`new slice<T>(new ReadOnlySpan<T>((T*)ptr, (int)n))`. (A bare `Span<T>` would not do: it does **not** range as
`(index, element)` tuples — `for i := range s` → CS8130 — and has no `Ꮡ(s, i)` element-address — CS0411; `slice<T>`
supports both, since it is `IArray<T>`.) The `ReadOnlySpan<T>` constructor takes a C# `int`, so a Go `int`/`uint` length
(`nint`/`nuint`) is narrowed via `getRangeIndexer` (through the underlying for a named numeric); an int literal is left
as-is. The slice **copies** the pointed-to memory (`ReadOnlySpan.ToArray()`), which is self-consistent for code that
only uses the resulting slice (e.g. runtime's `os_windows` ranges an unsafe `[]byte` read-only). Since this is always the `(*[N]T)(ptr)` unsafe-cast
form, it is memory-layout-dependent code whose raw values flow through the `unsafe.Pointer`=`nuint` round-trip (a
transient `fixed` address → not GC-stable), so the runtime values are not the contract — only compilable, rangeable,
element-addressable C#. No behavioral test emits this Span form: the `PointerCastSliceRange` test's source is already a `*[4]int`, so its
`(*[4]int)(unsafe.Pointer(&arr))[:4]` is an identity cast that the converter collapses to the array box and slices
directly (`Ꮡarr.slice(0, 4)`), and that test guards the index range, value range and `&s[i]` element-address over the
result. The standard-library sites of the Span form are runtime's `os_windows`, `heapdump` and `mbitmap`. The length
narrowing is covered by `StdLibInternalAbi`.

An **untyped (type-inferred) composite literal** — the inner `{…}` of a `[][]rank{ key: {…} }`, which has no explicit type node — is emitted as a target-typed `new(…)` when its inferred type is a struct (the struct constructor takes the field values). When the inferred type is a **slice or array**, that form is wrong (`slice<rank>`/`array<rank>` have no element-list constructor → CS1729); the converter emits the element-array projection instead — `{rA, rB}` (inferred `[]rank`) → `new rank[]{rA, rB}.slice()`, and an inferred `[2]int` → `new nint[]{…}.array()`. When the inferred type is a **pointer-to-struct** — the `[]*T{ {…} }` shorthand for `&T{…}` — it is emitted as the boxed struct constructor `Ꮡ(new T(field: val, …))` (a bare `new(…)` would target the box `ж<T>`, whose constructor lacks the struct's fields → CS1739). When such an untyped slice/array literal is **keyed** (`{joiningL: stateBefore, …}` — the inner `{…}` of x/net/idna's `joinStates = [][numJoinTypes]joinState{stateStart: {…}, …}`), the element-array projection above cannot take Go's `key: value` syntax — `new joinState[]{ joiningL: stateBefore }` is a C# array initializer, which has no keyed element form (CS1003 ×62). The keyed case is routed to a golib `golib.SparseArray<T>` collection initializer instead — `new golib.SparseArray<joinState>{ [joiningL] = stateBefore, … }.array()` (`.slice()` for a slice element) — the same form the *typed* keyed slice/array path emits (see below); the `.array()`/`.slice()` `IEnumerable<T>` extension materializes the dense backing, and a defined-integer key takes the `[(int)key]` cast exactly as in the typed path. (Guarded by `UntypedNestedSliceComposite`; runtime/lockrank.go's `lockPartialOrder` is a `[][]lockRank` and runtime1.go's `dbgvars` is a `[]*dbgVar` of the positional forms, and x/net/idna's `joinStates` is the keyed form.)

An **indexed (keyed) slice/array literal** — `[]string{lockRankSysmon: "sysmon", …}` — is emitted as a golib `golib.SparseArray<T>` collection initializer (`[index] = value`). Its indexer takes a Go `int`. When an index key's Go type is a **defined integer type** whose underlying type does not implicitly widen to C# `int` (i.e. `int`/`int64`/`uint`/`uint32`/`uint64`/`uintptr`, as opposed to `int8`/`uint8`/`int16`/`uint16`/`int32`), the key is cast to `int` so it satisfies the indexer (CS1503 otherwise): `[lockRankSysmon]` (a `type lockRank int`) → `[(int)lockRankSysmon]`. A key that already widens (e.g. a `uint8`-backed `Kind`) is left uncast.

A keyed slice/array literal whose element type is a **non-empty INTERFACE** routes its elements
through the interface-cast element loop (each value wraps via `convertToInterfaceType`) instead of
`convExprList`. Rendered as a FLAT `key, value` pair, a `KeyValueExpr` would feed the SparseArray
collection initializer one item at a time (`Add(key)` then `Add(value)`:
CS1950 + CS1503 ×21 pairs on go/internal/gccgoimporter's `lookupBuiltinType`,
`[...]types.Type{gccgoBuiltinINT8: types.Typ[types.Int8], …}` — untyped named-const keys in a
function-body literal, immediately indexed). The loop therefore emits the same `[key] = wrappedValue`
indexer form the non-interface path produces, with the key routed through `sparseArrayKey` (so a
defined-integer-type key keeps its `[(int)key]` cast alongside the interface-wrapped value); a
keyed MAP literal with an interface element type reaching the same loop takes the identical
indexer form. (Guarded by the `SparseArrayIfaceElem` behavioral test — a function-body sparse
literal with untyped named-const keys immediately indexed, the package-level form, and a
named-`uint`-keyed form, elements read back and output-compared vs Go.)

A keyed (sparse, constant-index) literal of a **named array-wrapper** type — internal/trace's
`timedEventArgs{1: uint64(ev.StkID)}` (`tracev1.go`) <!-- `oldtrace.go` before Go 1.26 --> where `type timedEventArgs [4]uint64`
— backs onto the golib `array<T>(length)` (which has an indexer setter), not a raw C# array. The wrapper's constructor
takes an `array<T>` (the positional path already produces one via `.array()`), and the keyed elements render as the
`[i] = v` indexed initializer — valid on `new array<uint64>(4){[1] = v}` but *not* on `new uint64[]{[1] = v}` (CS0131,
an array-initializer takes no indexed members). A **positional** literal of the same wrapper keeps the
`new uint64[]{…}.array()` form. (Guarded by `NamedArrayKeyedLiteral` — a `type args [4]uint64` with multi-keyed,
single-keyed, and positional literals, element reads output-compared vs Go.)

A **generic** named array type carries its type parameters (and their constraints) onto the forward declaration, and its element type is emitted fully qualified in the definition comment so the generated array-backed partial — which lives in a file without this file's package-relative `using` aliases — can resolve it:

```go
type table[T any] [3]atomic.Pointer[T]
```
<!-- illustration: not converter output -->
```csharp
partial struct table<T> /*[3]sync.atomic_package.Pointer<T>*/
    where T : new();
```

An **anonymous array/slice field whose element type lives in a multi-segment-path package** —
`cpuLogWrite [2]atomic.Pointer[profBuf]`, `children [4]atomic.UnsafePointer` (atomic = `internal/runtime/atomic`) —
keeps its `array<…>` wrapper. The field's type name is built structurally from the `[N]`/`[]` marker plus the
recursively resolved element, *not* from the type's package-qualified string: that string
(`[2]internal/runtime/atomic.Pointer[…]`) goes through a cross-package last-segment strip that would also remove the
leading `[2]`, collapsing the field to the bare element type (`atomic.Pointer<…> = new(2)`) whose array `new(2)`
initializer then mis-binds the element constructor (CS1503). With the structural rendering the field stays
`array<atomic.Pointer<profBuf>> = new(2)`. An array of a current-package or basic-typed element needs none of this (its
string has no foreign path to strip). (Guarded by the `ArrayOfCrossPackageType` behavioral test — `[3]atomic.Int32` /
`[2]atomic.Uint64` fields; runtime's `trace`/`traceMap` structs hold these.)

### A struct's array fields get their fixed length from a generated parameterless constructor

A Go `[N]T` array FIELD has a zero value of N zero elements — never nil. The converter emits the field
with a length initializer — `internal array<atomic.Int32> c = new(3);` — but a C# struct field
initializer only runs when an **explicitly declared** parameterless constructor is invoked; the implicit
struct constructor that `new counters()` would otherwise use zeroes every field and SKIPS initializers,
leaving the array's backing `T[]` null (an NPE on the first index or `len`). The `TypeGenerator`
therefore emits an explicit parameterless constructor for every struct, so `new S()` runs the field
initializers and each array field gets its `new(N)` backing. (C# 11 auto-defaults any field without an
initializer; a slice/map/chan field — which has no `new(N)` initializer — stays its nil zero value,
matching Go.) The **NilType constructor preserves the initializers too**: re-assigning
`this.field = default!` to every plain member would run *after* the field initializers and null an
array field's fresh `new(N)` backing, so a struct built by `new S(nil)` would NRE on the first index. The
NilType and parameterless constructor bodies (`AppendZeroValueInitializers`) assign only what C#'s
implicit zeroing would leave *broken*: the promoted-embed boxes (see
[Struct Type Embedding](struct-embedding.md#struct-type-embedding)), and — see next paragraph — any plain struct-typed field
whose own type needs construction; everything else is left to the field initializers plus C# 11
auto-default. This is generator-only, so it shows in the `.g.cs` output rather than in a golden;
`ArrayOfCrossPackageType` checks it as an **output-compared** test (`len(x.c)` / `len(x.d)` print `3 2`).

A plain (non-embed) struct-typed FIELD whose type *itself* needs construction is the recursive case:
`default(T)` gives such a field a `default(FieldType)`, whose nested promoted-embed box or fixed-array
backing is null — so the first touch NREs even though `T`'s own boxes were constructed. This is fmt's
`pp{ … fmt fmt … }` where `fmt` embeds `fmtFlags` (a ctor-allocated box): left alone, `newPrinter`'s
`@new<pp>()` would run `pp()`'s ctor and leave `fmt` as `default(fmt)` with a null box, and
`p.fmt.init(&p.buf)` → `clearflags` would NRE on the first `fmt.Println`. `AppendZeroValueInitializers`
therefore also emits `this.f = new FieldType(nil);` for each such field, and because that runs
`FieldType`'s own NilType constructor (which recursively constructs *its* needy fields), a single level of
construction fixes every depth. "Needs construction" (`StructTypeNeedsConstruction`) is: has a promoted
embed, a fixed-array field, or a nested struct field that needs construction; a reference field
(pointer/interface/delegate) keeps its correct nil zero value.

**The resolution must be by SYMBOL, not by syntax.** A `<ProjectReference>` reaches the compiler as a
**`PortableExecutableReference`** — compiled metadata with no syntax trees — so in any real MSBuild build a
search for a `StructDeclarationSyntax` cannot resolve *any* cross-package field type, and nothing in the
consuming package would construct it. When such a type carries a fixed array at any depth, `default` leaves
that `array<T>`'s backing null (golib's deliberate zero-value discriminator), so `len` and `range` silently
measure **zero** and the first index or pin throws — `GCHandle.AddrOfPinnedObject`'s
`InvalidOperationException: Handle is not initialized` (see golib `ж.cs`, `pinnedArrayData`).
`math/rand/v2`'s `ChaCha8` is the standard-library case: its `internal chacha8rand.State state;` needs
`State`'s `buf = new(32)` / `seed = new(4)`, where Go's `new(ChaCha8)` yields 32 real zeroed words. (Only
`CompilationReference`s — in-memory Roslyn compilations, the shape unit tests use — carry syntax.)
<!-- history: StructTypeNeedsConstruction once answered only for structs it found syntax for, on the reasoning
that "its own package constructs it"; ChaCha8's state went unconstructed until the symbol path. -->

`GetStructDeclaration` is therefore backed by `Compilation.FindTypeSymbol`, which resolves a
fully-qualified display name to an `INamedTypeSymbol` through `GetTypeByMetadataName` (stripping `global::`
and verbatim `@`, rendering type arguments as arity suffixes, and trying each namespace-vs-nested-type split
of the dotted name since a display string spells both `.`). The metadata walk applies the same three
triggers over `GetMembers()` — a ref-returning property is a promoted embed, a `go.array<T>`-typed field is
a fixed array, a struct-typed field recurses — with the same cycle guard. On the metadata path the
`public T(NilType)` constructor that `new T(nil)` needs is **checked rather than assumed** (metadata is
fully compiled, so the generated constructor is really there): a hand-written golib struct or any other
referenced type without one returns `false` and correctly keeps its `default`. Scalars, pointers, slices,
maps and interfaces are still left `default`, because `default` **is** their Go zero value — over-
constructing would add an allocation to every instantiation for no semantic gain. (Guarded by the
`CrossPackageArrayZeroValue` output-compared test — a `Holder` whose field type lives in the `bufpkg`
sibling library sub-project, so the reference is genuinely metadata; a same-project field type resolves by
syntax and would pass even unfixed. Against the unfixed generator the test panics with
`index out of range [2] with length 0`.)

**The field-wise constructor closes the same gap for a PARTIAL composite literal.** The *parameterized*
constructor (`GenerateConstructor`, used by a composite literal that sets some fields —
`&Holder{tag: "lit"}` → `new Holder(tag: "lit")`) cannot take `T f = default!` and assign `this.f = f;`
unconditionally for a needy struct member: an OMITTED argument would arrive as the broken `default(T)` and
overwrite the field — the NilType path's breakage, for a **same-package** field type too. `io.pipe` is
the standard-library case, whose `onceError rerr, werr` value fields each embed `sync.Mutex` via the
promotion box: `io.Pipe()` builds the pipe with `new pipe(wrCh: …, rdCh: …, done: …)`, omitting
`rerr`/`werr`, so both boxes would be null and the first `Store`/`Load` `Lock()` would NRE on the pipe's
writer goroutine, crashing every `io.Pipe` consumer. A fixed-array member has the analogous
`if (f.Source is not null)` guard (its `= new(N)` field initializer supplies the omitted zero); the
needy-struct member has no field initializer to fall back on, so it must be *constructed*.
`GenerateConstructor` emits the needy value-struct member's parameter as **nullable** — `onceError? rerr = default!` —
making an omitted argument a genuine
`null` sentinel that `default(onceError)` (a real struct value with a null box) could never be — and its
body reconstructs only when omitted: `this.rerr = rerr ?? new onceError(nil);`, exactly mirroring the
pointer-embed `?? new ж<T>(nil)` handling for a promoted embed. A caller-SUPPLIED value is used as-is (no
extra allocation, unchanged reference semantics — the struct copy shares the same embed box); an omitted
one gets `T`'s own NilType ctor, which recursively constructs *its* needy members. The predicate
`IsNeedyValueStructMember` reuses `StructTypeNeedsConstruction` (member is not a promoted embed, not a
reference, not a fixed array, and its struct type needs construction), so every *ordinary* member keeps
the plain `T f = default!` parameter and assignment. The NilType, parameterless and field-wise
constructors are all covered, so this reaches
`new(T)`/`@new<T>()`/`T{}`/`&T{…}` (empty **and** partial composite literals). (Guarded by the
`PromotedEmbedZeroValueField` output-compared test — a `slotBox{id: 3}` partial literal omitting a
`holder` value field that embeds a promoted `counter`, whose promoted `inc()` is then called on the
omitted field; without the nullable parameter it NREs with `Object reference not set to an instance of an
object`.)

**A MIXED-VISIBILITY struct takes one arm more: the PUBLIC field-subset constructor, which does not
name the needy member at all.** The paragraph above fixes the member's *parameter* — but a struct with
any unexported field gets **two** field-wise constructors (`Constructors`): a `public` one over
`PublicStructMembers` and an `internal` one over all of them, the public subset carrying
`OverloadResolutionPriority(-1)` so a same-assembly named-args call binds the full overload. An
unexported needy member is therefore absent from the public subset ctor's parameter list *and* its
body, so the `?? new T(nil)` reconstruction cannot apply to it and the field would be left at
`default(T)` — the exact state `AppendZeroValueInitializers` exists to prevent. Only a literal in
**another package** reaches that constructor, because every same-package literal binds the internal
all-fields ctor. `syscall.SockaddrUnix` is the standard-library case: `&SockaddrUnix{Name: path}` from
`net` leaving `raw` default would make `raw.Path`'s `[108]int8` backing zero-length, and `sockaddr()`'s
own `if n > len(sa.raw.Path)` guard would return `EINVAL` *before `bind` ever reached the kernel*,
failing every AF_UNIX listen/dial on Windows with "invalid argument" (`net`'s `TestModeSocket` and
`TestUnixConnLocalWindows`). Because the error is Go's own **invented** `EINVAL` (`APPLICATION_ERROR`-based,
message "invalid argument") rather than the kernel's `WSAEINVAL` (10022, "An invalid argument was
supplied."), such a failure reads like a rejected sockaddr rather than a missing zero value.
`GenerateConstructor` reuses `AppendZeroValueInitializers` over the members the ctor does **not** name, giving them the
same zero-value construction the parameterless ctor gives them; the all-fields internal ctor omits
nothing, so nothing changes there, and a struct with no unexported members emits no subset ctor at
all. (Guarded by the `CrossPkgLiteralNestedField` output-compared test — an `addrlib` sibling library
supplies `Addr{Name string; raw rawAddr}` in `SockaddrUnix`'s exact shape plus an `Embedder` whose
unexported member is a promoted **embed**, both built from the parent package by composite literal;
it reads the nested fixed array's length, runs a guard-then-fill `Encode()` mirroring `sockaddr()`,
reads bytes back out, and checks that an over-long name is still rejected — so it separates "the array
is right" from "the guard always fails". Without this arm the capacity reads `0` and `Encode` answers
`0 false`.)

A bare **`var x T`** zero-value declaration (no initializer) calls none of those, so the *converter*
closes the remaining gap on its side: when `T` needs construction it emits `T x = new();` — the generated
parameterless constructor, which runs the same field initializers + `AppendZeroValueInitializers` — rather
than `T x = default!;`, which would leave an array field's backing null (an NRE on the first index/`len`). The
converter mirrors `StructTypeNeedsConstruction` with the Go-side `structZeroValueNeedsConstruction` (promoted
embed / fixed-array field / nested needy struct, recursively; a reference field keeps its correct nil zero
value). A promoted-embed `var` keeps its existing `new(nil)` (the NilType ctor) and a scalar-only struct
keeps `default!`, so only genuinely-needy structs take `new()` (`PublicizedFieldType`'s `var cr CaseRange`,
with its `[3]rune` `Delta` field, is one). A needy struct
**global** likewise gets `new()` in place of the bare `static T x;`. This `var`/global path is guarded by the
`ZeroValueStructVar` output-compared test (`var z holder` with a `[8]int` field, a nested `wrapper`, and a
scalar-only `point` control). Guarded by
the `NestedPromotedEmbedInit` output-compared test (a `printer` holding a `formatter` field that embeds
`flags` and holds a `[3]byte`, reached via both `new(printer)` and `&printer{}`, its promoted fields and
array written and printed against Go).

### The zero-value ladder is one ladder, and a NAMED RESULT climbs it too

The `var x T` path above and the named-result prologue are the same question asked at two syntactic
sites — *what does a declaration with no initializer put in the slot?* — and both take the same answer.
A named result declared `T name = default!;` at function entry would give a `[N]T` result **length 0**
and an array-bearing struct result a null backing, so both shapes emit their construction:

<!-- illustration: not converter output -->
```csharp
// src/core/net/netip/netip.cs — func (ip Addr) As16() (a16 [16]byte)
array<byte> a16 = new(16);
byteorder.BEPutUint64(a16[..8], ip.addr.hi);   // with default!: length 0, a slice-bounds panic

// src/core/crypto/tls/common.cs — func (c *Config) ticketKeyFromBytes(b [32]byte) (key ticketKey)
ticketKey key = new();                          // runs `aesKey = new(16)`, `hmacKey = new(16)`
copy(key.aesKey[..], hashed[(int)(legacyTicketKeyNameLen)..]);   // with default!: copies 0 bytes
```
<!-- history: both were shipped with `default!` and measured live in crypto/tls ("aes: invalid key size 0"). -->

Both come from a single shared helper, `zeroValueInitializer`, which the three named-result declaration sites
(`visitFuncDecl`'s plain and blank-slot prologues, `iifeOperations.namedReturnDeclLines` for a
function literal and the deferred-named-return lowering) and the `var`/global paths all read. Its
rungs, in order: an **unnamed** fixed-size array → `new(N)` plus `arrayZeroValueArgs`' element
factory; a promoted-embed struct → `new(nil)`; a struct carrying a fixed array at any depth →
`new()`; everything else → `default!`. A **named** array type is deliberately excluded — go2cs-gen's
array wrapper allocates its backing lazily from its own known size, so its `default` is already
usable — and a scalar-only result still stays `default!`, which is what keeps the change confined
to types whose Go zero value genuinely is not all-bits-zero.

Guarded by the `ZeroValueArrayNamedResult` output-compared test, which pins all five emission sites
against `go run`: an `As16`-shaped `[16]byte` result written through a slice of itself, a
`ticketKey`-shaped struct result filled by `copy`, a nested value-struct result, a result declared
by the deferred-named-return lowering, and a function literal's named result — plus a scalar-only
control proving the ladder does not over-fire.

Two sites of the same class are knowingly **not** changed, because the corpus does not exercise
either and an unexercised emission change is an unmeasured one: the tuple element a *blank* named
result contributes to an explicit return (`visitReturnStmt`), and the zero-results `return default!;`
that closes a value-returning function's recovered-panic catch arm (`visitFuncDecl`/`convFuncLit`).
Censused at zero — no `return default!;` in the corpus sits in a function whose return type mentions
`array<`, and GOROOT declares no blank named result of array type. A third gap, a **map miss** on an
array-valued map, is closed: the read site supplies the zeroed `[N]T` (see *A map READ of a
shape-carrying element supplies the zero from the CALL SITE* below).

#### A slice-bounds fault is a PANIC, not an ArgumentException

`slice<T>`'s windowing constructors raise `RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max,
capacity)` for an out-of-bounds window, with the Go runtime's message shapes, and `array<T>`'s
slice→array conversion raises `RuntimeErrorPanic.ArrayConversionLength` with Go's text. A .NET
`ArgumentException`, `ArgumentOutOfRangeException` or `IndexOutOfRangeException` would not be a
`PanicException`, so it would be invisible to `recover()` and — the expensive part — would satisfy
`Goroutine.CanContain`, so a converted test host would *contain* it and record it on the
`TestExecution` instead of failing. When the dying goroutine is the one another goroutine is waiting
on, the record never flushes and the package deadline burns with no output at all, where Go fails
loudly in milliseconds. The netip case above reproduces Go's message exactly: `a16[:8]` on a length-0
array prints `runtime error: slice bounds out of range [:8] with capacity 0`. <!-- history: the six throws
were plain .NET exceptions until this fix; 17 of crypto/tls's 53 measured divergences presented that way
(10 as an infrastructure-error line, 7 as a silent hang). -->

### Prefer the file-local package alias over the fully-qualified `_package` name

A cross-package named type has two C# spellings: the **fully-qualified** form `sync.atomic_package.Int32` (the namespace-rooted class, from `getFullyQualifiedTypeName`) and the **file-local alias** form `atomic.Int32` (the `using atomic = sync.atomic_package;` shorthand, from `getAliasQualifiedTypeName`). For *visual* fidelity — the converted C# should read like the Go original, which writes `atomic.Int32` — body emission prefers the alias. But the alias is only resolvable where the `using` is in scope, so the choice is made per emission site by `getScopeCheckedTypeName`, which returns the alias form **only when every cross-package type referenced by the type is imported in the current file** (checked against the per-file `importQueue`), and otherwise falls back to the fully-qualified form.

The fallback matters: a Go file may *index* an atomic-typed array field of a struct — `&x.c[i]` → `…at<E>(i)` — without ever naming the element type `E`, so it carries no `import "sync/atomic"` and no `using atomic`. There the element type must stay fully-qualified (it resolves inside `namespace go;` with no alias) or the file fails CS0246. When the package *is* imported (the common case, and every behavioral test of this), the prettier alias is used.

`getScopeCheckedTypeName` is applied at the body-emission sites that land in the current source file:

* **named struct-field declarations** — `internal atomic.Int64 total;` (not `sync.atomic_package.Int64`);
* **heap-box allocations** — `ref var n = ref heap(new atomic.Int32(), out var Ꮡn);`;
* **element-address `at<T>`** — `…at<atomic.Int32>(0)`.

It is **not** used for forms consumed by the source generators in alias-less generated files, which must stay fully-qualified: the `/*…*/` definition comment (e.g. `/*sync.atomic_package.Uint32*/`, `/*[3]sync.atomic_package.Pointer<T>*/`), the `global using` type-alias declarations, and the promoted-interface/embedded-field registration keys. (Embedded fields keep the full form for their promoted accessors; only the named-field branch uses the display name. Struct-embedding promotion across packages re-derives member types from the Roslyn semantic model, not from the field's emitted text, so aliasing the field declaration is safe.) Guarded by `ArrayOfCrossPackageType`, `AtomicValues`, `FuncTypeParam`, `GenericAtomicPointerField`, `GlobalAtomicDefer`, `GlobalAtomicFieldMethod`, and `StructPromotionWithInterface`/`StructPointerPromotionWithInterface`.

### Combined field-element address `base.at(field, i)`

The address of an element of an array/slice FIELD of a boxed value — `&x.c[i]` where `c` is an
array field, or the implicit address taken to call a pointer-receiver method `x.c[i].inc()` — is a
two-step chain at heart, `Ꮡx.of(counters.Ꮡc).at<atomic.Int32>(i)`: `of(field)` takes the field's
address (a `ж<array<E>>`), then `at<E>(i)` takes the element's. The explicit `<E>` is needed because
golib's standalone `at<TElem>(nint)` is generic in an element type unrelated to the pointer's `T`, so
it cannot be inferred. golib has combined overloads `ж<T>.at<TElem>(FieldRefFunc<…array<TElem>…>, nint
index)` (one per field-accessor shape and array/slice kind, each forwarding to `of(field).at<TElem>(i)`)
whose `TElem` IS inferred from the field accessor's return type. The converter collapses the chain
to `Ꮡx.at(counters.Ꮡc, i)` — dropping both the `.of(` step and the `<E>` type argument. It rewrites the
recursively-built field address `base.of(Type.Ꮡfield)` by retargeting its trailing `.of(field)` to
`.at(field, i)`, only when the field segment is parenthesis-free (a plain `Type.Ꮡfield` accessor, so the
final `)` provably matches the last `.of(`); any other shape falls back to the explicit chained form.
The combined overload is behaviorally identical to the chain (it literally forwards to it). (Guarded by
`ArrayOfCrossPackageType`, `IndexedElementDirectBoxMethod` and `PointerFieldArrayElementAddress` — all
output-compared; the `.inc()`/`bump()` element writes verify runtime equivalence.)

The routing gate sees through **nested value fields to the chain root**. `&pp.wbBuf.buf[0]` (runtime
`mwbbuf.go`) roots at the pointer `pp` through the *value* field `wbBuf`; a gate that checked
pointer-ness only one level up (`pp.wbBuf`, a struct) would fall to a naive `Ꮡ` prefix (`Ꮡpp.wbBuf…` —
CS1061 on the box), as would the closure-captured variant (`&mp.trace.buf[gen%2]`, `trace.go`).
The gate walks intermediate selectors to the root, so any pointer-rooted (or heap-boxed) chain routes
through the recursive `&field` machinery — `Ꮡpp.of(runtime_package.Δp.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)` — which already
rendered multi-hop of-chains. A **nested-index** base — `&cache.entries[ck][i]` (2-D array via a pointer,
`symtab.go`) — is an `IndexExpr`, not a selector, so it gets its own arm: recursively take the inner
element's address (`cache.at(pcvalueCache.Ꮡentries, (ulong)(ck))`) and chain the outer `.at<T>(i)` onto it — the
gate also accepts a HEAP-BOXED value root (`&grid.cells[1][2]` on an address-escaping local). An unboxed
value-rooted chain keeps the plain `Ꮡ` form. *Known gap: an intermediate `IndexExpr` inside the selector
chain — `&ptr.items[i].buf[j]`, an array-of-structs hop — is not walked by either arm (both step only
through selectors).* (Guarded by the `NestedFieldElementAddr` behavioral test — all three runtime shapes
with write-through vs Go.)

**Element address of a by-value ARRAY PARAMETER.** An array parameter whose element address is
taken (syscall `SetsockoptInet4Addr`, `&value[0]` on `value [4]byte`) is heap-boxed at entry, its Go
by-value clone folded into the box init, and the element address is taken through that box:
`ref var value = ref heap(valueʗp.Clone(), out var Ꮡvalue);` then `Ꮡvalue.at<byte>(0)` (see
[An address-taken VALUE PARAMETER heap-boxes too](multi-assignment.md#an-address-taken-value-parameter-heap-boxes-too)).
An array base that owns no box keeps the older fallback, which boxes a **copy of the wrapper
struct**, `Ꮡ(value).at<byte>(0)`: `array<T>` wraps a `T[]` reference, so the copied wrapper SHARES
element storage with the original, and element reads and writes through the pointer stay correct.
(Guarded by `DeferTypelessReturns`' `first` — element address of a `[4]byte` parameter, value vs Go.)

**Element address of a POINTER-to-array — `&t[i]` where `t` is `*[N]E`.** Go auto-derefs the index
(`(*t)[i]`), so the element lives in the pointed-to array on the heap; `t` already IS the `ж<[N]E>`
box. The converter emits `t.at<E>(i)` — ж's `at` materializes the array's lazy backing on the REAL
storage and then returns an element pointer over the shared backing. `at` reaches that lazy getter
through a **per-box atomic publish** (see *The array-backing publish is atomic per box* below).
<!-- history: first a reflection-built constrained delegate (fatal under Native AOT, d5c0c9c10), then an
unsynchronized box-touch-copy-back, and since 2026-08-30 the per-box atomic publish. --> The base is
rendered in POINTER context so it yields the box: a deref-aliased pointer
PARAMETER gives `Ꮡt` (the parameter is `ж<[N]E> Ꮡt`, deref-aliased to `ref var t = ref Ꮡt.Value` in
the prologue), while a box-valued LOCAL from `new([N]E)` gives the plain `t`. The generic `Ꮡ(t.Value[i])`
**copy** form would box a snapshot of the element and silently drop any write made through the returned
pointer — hash/crc32's `simplePopulateTable(poly, &t[0])` would populate a throwaway copy and leave every
CRC table all-zeros. The other write-through sites are `crypto/internal/nistec` (`p224GG[i].SetBytes(…)`
in static init), `internal/bisect` and `runtime` (`atomic.Store*(&arr[i], …)`). <!-- history: this shape
once fell through every array/slice branch to the copy form; hash/crc32's TestGolden and TestSlicing
failed with ~0-with-shifts checksums. --> (A
pointer-to-SLICE cannot reach here: Go does not auto-deref `*[]E` for indexing; it is written
`(*t)[i]`, a `StarExpr` the slice branch already aliases.) (Guarded by `PointerToArrayElementAddress`
— write-through `&g[j]` on a `*grid` local, value vs Go.)

**Element address of a SLICE FIELD of the receiver — `&b.lines[i]`.** The address of a slice element
uses one of two golib forms: the **element-aliasing** two-arg `Ꮡ(x, i)` (→ `new ж<T>(IArray, index)`,
whose `ValueSlot` returns `ref backingArray[index]`, so writes land in the shared backing array), or the
**copy-boxing** `Ꮡ(x[i])` (→ `Ꮡ(in T)`, which boxes a *copy* of the element value). For a slice the copy
form is only sound when nothing is written back through the pointer. Inside a pointer-receiver method the
`refRecv` fast-path keeps the copy form only when the receiver is *directly* the slice (`indexExpr.X` is
the bare receiver identifier); any slice base that is a field, call result, or other non-identifier
expression uses the element-aliasing `Ꮡ(x, i)` form — which is correct for a slice in all cases, since a
slice value always shares its backing array. A slice *field* of the receiver — `&b.lines[i]`, whose base
`b.lines` roots at the receiver `b` — must not take the copy: text/tabwriter's `terminateCell` does
`line := &b.lines[len-1]; *line = append(*line, cell)`, and through a copy the `append` would grow a
*copy* of the row's slice header, never `b.lines`, so **all formatted output would come out empty**.
`NamedFuncTypeStructuralField`'s read-only `s.by(&s.items[j], &s.items[i])` takes the alias form too.
<!-- history: the fast-path once keyed off getIdentifier(indexExpr.X), which walks to the root identifier,
so receiver fields took the copy form and text/tabwriter emitted empty output. --> (Guarded by
`SliceFieldElementAddress` — append-through-pointer into a `[][]int` field of a pointer receiver plus an
in-place element mutate, value vs Go; validated end-to-end by `text/tabwriter`'s test suite.) The ARRAY
branch is narrowed the same way — next.

**Element address of an ARRAY FIELD of the receiver — `&d.hashHead[h]`.** The array branch's `refRecv`
fast-path follows the slice branch's rule: the copy form is kept only when the receiver is *directly* the
array (`indexExpr.X` is the bare receiver identifier); an array field of the receiver uses the
element-aliasing two-arg `Ꮡ(d.hashHead, (hash & (uint32)hashMask))`. The copy-boxing `Ꮡ(d.hashHead[h])`
would heap-box a copy of the **element**: `compress/flate`'s `deflate()` maintains its chained hash table
through `hh := &d.hashHead[hash&hashMask]; … *hh = uint32(d.index + d.hashOffset)`, and with every head
write landing in a throwaway box `findMatch` would never be called, so levels 2–9 would emit literals only
— valid deflate streams with far weaker compression. <!-- history: that was the shipped behavior before the
narrowing; a 256x256 PNG at png.BestCompression (flate level 9) encoded to 134,644 bytes
instead of Go's 36,760, pixel-identical on decode, while NoCompression, HuffmanOnly and BestSpeed (whose
deflatefast.go encoder never takes an element address) stayed byte-exact (platform not recorded). -->

Note the array field deliberately does **not** route through the `.of(field)`/`.at<T>(i)` box machinery
described above, even though that machinery exists for array fields. Its trigger is `baseIsPointer` — the
*Go* receiver type is `*T` — but a Go pointer receiver renders as `this ref T recv`, which has **no** box
companion, so it would emit `Ꮡr.of(RegArgs.ᏑInts)` for `internal/abi`'s `&r.Ints[reg]` → CS0103. The two-arg
form needs no box and aliases correctly regardless: `array<T>` is a readonly struct wrapping an eagerly
allocated `T[]`, so evaluating the field copies only the wrapper while the copy shares element storage —
the same reasoning the array-*parameter* case above relies on. (That reasoning holds for `array<T>` and
**not** for a field whose type is a NAMED array, whose wrapper allocates its backing lazily; such a
field is projected through `.Value` first — see *The element address of a VIRGIN named array must
materialize through the receiver*. No corpus site has that shape; the gate is there because the shape is
legal Go.) The same write-through sites appear across the standard library: `runtime`'s
`&r.statusTraced[gen%3]`, `&h.counts[…]` and `&m.stats[gen]` (`.CompareAndSwap`/`.Store`/`.Add`),
`crypto/internal/edwards25519`'s lookup tables built via `(&v.points[i]).FromP3(…)`, and `image/jpeg`'s
Huffman/quantization tables written through `&d.huff[tc][th]` and `&d.quant[…]`. <!-- measured at the
landing: the narrowing corrected seventeen corpus files, several silently write-dropping. --> (Guarded by
`RecvArrayFieldElementAddress` — chained-hash write-through with an unsigned index, plus a nested
`&h.pairs[i][j]`, value vs Go; the flate ratio itself is verified by deflating fixed buffers at every
level and byte-comparing the sizes against `go run`.)

**An array element's address is one box per element when the element has field views.** `&a[i]` on an
array (`Ꮡ(a, i)`) is golib's element-reference box over the backing array and absolute index. For an
element type whose fields have been addressed through a box (`&a[i].x`, a field view), golib keeps ONE
persistent box per (backing, index) for the backing's life, so a second `&a[i]` finds the same box and the
field views cached on it, rather than minting both again (`ElemRefBox<T>.OfArrayElement`, scoped by
`BoxShape<T>.Slotted`). The boxes sit in a weak table keyed on the backing, allocated in 64-slot chunks on
the first address taken inside each, so a backing nobody references is collected with its boxes. Every
other element type takes a fresh box per expression. Equality is (backing, index) either way, so Go's
`&a[i] == &a[i]` holds; only the number of objects differs. (Guarded by GolibTests' `ElemBoxPersistTests`.)
<!-- landed 2026-10 for crypto/internal/fips140/nistec's p256Table.Select, which takes &table[i] sixteen times
a lookup and then the x, y and z views of each (P-256 R3). -->

## Array ASSIGNMENT copies the whole array (`.Clone()` on the RHS)

Go array assignment copies the array — `data := ints` yields independent storage — but the emitted
`array<T>` is a struct over a shared `T[]` backing store, so a plain C# struct copy **aliases**: a
write through the copy would be visible through the source (sort's `TestReverseSortIntSlice` does
`data := ints; data1 := ints` and sorts one each way). The converter therefore appends golib's
strongly-typed `array<T>.Clone()` to an assignment RHS that copies an array **out of existing
storage** (see `cloneValueCopy` in `visitAssignStmt.go`):

```go
d := garr        // → var d = garr.Clone();
var e = garr     // → array<nint> e = garr.Clone();
e = src          // → e = src.Clone();
f := h.arr       // selector RHS      → var f = h.arr.Clone();
row := m[1]      // index RHS         → var row = m[1].Clone();
g := *q          // deref RHS         → var g = q.Value.Clone();
x, y = y, x      // tuple swap        → (x, y) = (y.Clone(), x.Clone());
```

Only existing storage takes the clone — an ident, selector, index, or deref RHS reads a value some
other name can still reach; a composite literal, call result, or conversion is freshly constructed
and stays bare. The shape/type gate is the shared `exprReadsValueNeedingClone`
(`arrayCloneOperations.go`), which tests the UNDERLYING type — so direct, alias-declared, and NAMED
array types all clone (the named wrapper via its strongly-typed `Clone()`, next section), and an
interface-typed LHS (`var x any = arr`) boxes the clone. (Guarded by the `ArrayPassByValue`
extension — all seven assignment shapes above, written-through and read back against the source —
and `ArrayValueCopySites`' `namedAssignCopies` — named `:=`/`var`/`any`-boxed forms, values vs Go.)
The other copy sites of the same defect class — range elements, composite-literal elements,
returns, channel sends, append elements — are covered by the follow-up section below.

## Array VALUE-COPY at every transfer site (range, composite, return, send, append) — DEEP for nested arrays

Go copies the whole array at **every** value transfer, not just assignment and parameter passing —
but the emitted `array<T>` (and the generated named-array wrapper) is a struct over a shared `T[]`
backing store, so every plain C# struct copy ALIASES. The converter appends the strongly-typed
`.Clone()` wherever an array value is read **out of existing storage** (an ident, selector, index,
or pointer-deref — `exprReadsValueNeedingClone` in `arrayCloneOperations.go`; a composite
literal or call result is freshly constructed and needs none):

```go
for _, row := range m { row[0] = 99 }  // → foreach (var (_, vᴛ1) in m.ΔRangeSnapshot()) { var row = vᴛ1.Clone(); … }
for i, row = range m { … }             // pre-existing vars → row = vᴛ1.Clone(); inside the body
m := [2][3]int{a, b}                   // → new array<nint>[]{a.Clone(), b.Clone()}.array()
s1 := holder{arr: a}                   // keyed struct field  → new holder(arr: a.Clone())
mv := map[string][3]int{"x": a}        // map value           → ["x"] = a.Clone()
mk := map[[2]int]string{k: "kv"}       // map KEY             → [k.Clone()] = "kv"
lst := []any{b}                        // interface boxing    → new any[]{b.Clone()}.slice()
return h.arr                           // return              → return h.arr.Clone();
ch <- a                                // channel send        → ch.ᐸꟷ(a.Clone())
s = append(s, a)                       // append element      → s = append(s, a.Clone())
```

The range emission routes an array-valued key/value through the same iterate-a-temp mechanism a
reassigned range var uses (a C# foreach variable cannot be redeclared from itself); a map range KEY
of array type clones the same way. A range over an array VALUE iterates golib's `ΔRangeSnapshot()`,
a copy of the array for the loop (Go copies the range expression once), so a write to `m` inside the
body does not change later iterations (guarded by `ArrayRangeSnapshot`). The RECEIVE side of a channel needs no twin —
the send stored an
unaliased element and a buffered element is dequeued exactly once.

Three deeper repairs make the single `.Clone()` correct everywhere:

- **`array<T>.Clone()` is DEEP for nested arrays** (golib `array.cs`): Go's `[2][3]int` copy copies
  the inner arrays too, which a shallow `T[]` clone would leave shared (`m.Clone()` then
  `m[0][0] = 99` would write the source's inner array). An element that is itself an array wrapper
  (anything implementing `IArray`) is re-cloned through its `ICloneable` surface, which returns the
  properly-wrapped clone so the unbox
  recurses through any nesting depth; the `typeof` gate keeps flat element types on the single
  shallow copy.
- **NAMED array types get a strongly-typed `Clone()`** (`IArrayTypeTemplate` /
  `IArrayViewTypeTemplate`): with only the object-returning `ICloneable` form, named-array copies could
  not be expressed. `public Row Clone() => new Row(Value.Clone());` (and
  the view-wrapper equivalent through its underlying wrapper) lets every site above — plus the
  function-parameter preamble, func-literal parameters, and array-typed VALUE RECEIVERS — clone
  named and alias-declared arrays exactly like direct ones (`typeIsArrayValue` tests the underlying
  type).
- **`array<T>` equality/hash is structural per-ELEMENT** (golib `array.cs`): Go arrays are
  comparable values, so a `map[[2]int]V` key must be found again by an equal array with different
  backing. `GetHashCode` hashes the elements, not the backing reference (which every structurally
  equal key would miss), and the `Equals` overloads pass element-typed comparers, not container-typed
  ones (`EqualityComparer<T[]>`, which `IStructuralEquatable` would call per boxed element and throw on);
  both recurse through nested arrays.

**The suffix must be WRAPPED on a `~`-prefixed rendering.** A deref whose operand is a pointer CAST
(`*(*T)(p)`, `convStarExpr`'s casted-pointer-deref path) renders with the PREFIX `~` operator, and C#
postfix binds tighter than unary — so a naked `.Clone()` re-binds onto the cast's inner operand
instead of the dereferenced array. reflect's `InterfaceData` is the standard-library case (hand-owned
in `value_impl.cs`; the `ArrayCastDerefClone` golden holds the emission):

```go
return *(*[2]uintptr)(v.ptr)   // reflect/value.go
```
<!-- illustration: not converter output -->
```csharp
return ~(ж<array<uintptr>>)(uintptr)(v.ptr).Clone();    // WRONG — .Clone() reads v.ptr (CS1061)
return (~(ж<array<uintptr>>)(uintptr)(v.ptr)).Clone();  // emitted — clone the dereferenced array
```

Every clone-append site therefore routes its rendering through `appendValueClone`
(`arrayCloneOperations.go`), which wraps only when the rendering starts with the deref operator —
the same precedence guard `convStarExpr` already applies when IT appends the postfix `.Value` to a
cast/deref rendering. Every other shape `exprReadsValueNeedingClone` admits (ident, selector,
index, and the postfix `.Value` deref form) is already a C# primary expression and takes the suffix
bare. <!-- history: the corpus-wide A/B footprint of the wrap was exactly the one reflect line, whose CS1061
had blocked the whole converted stdlib through fmt -> reflect. -->

(All guarded by the `ArrayValueCopySites` behavioral test — one output-compared section per site
class, including multidimensional deep-copy through range and parameter passing — plus
`ArrayCastDerefClone`, which guards the wrapped cast-deref form above. That guard is
**output-compared**: its TYPED-pointer half (`*(*T)(p)` where `p` is already `*T`) RUNS, because the
identity reinterpret does not route through the raw-address `uintptr` bridge (see *A SAME-TYPE
reinterpret … collapses to the pointer itself*), so the clone's copy semantics are proven by VALUE
— mutating the returned array must leave the pointed-to original untouched, and the lvalue form
must write through. Its `unsafe.Pointer` half stays compile-shape only, with the results
deliberately discarded: reconstructing an array through an `unsafe.Pointer` round trip reads raw
memory and cannot reproduce Go's values under the managed model.)

**Known gaps:** (1) golib-internal
element-wise transfers of nested-array elements (`copy(dst, src)`, spread `append(dst, src...)`)
copy element structs without re-cloning; (2) an array-typed map KEY at an index-STORE (`mk[k] = v`
stores `k` uncloned — only the composite-literal key form clones); (3) a named↔underlying array
CONVERSION (`[4]int(named)`) hands the wrapper's backing through the implicit operator uncloned;
(4) a fixed-size ARRAY reached only through an EMBEDDED struct member keeps a shared backing. An
embed is an INLINE field (see
[An embedded struct is an INLINE field, so a value copy copies it](struct-embedding.md#an-embedded-struct-is-an-inline-field-so-a-value-copy-copies-it)),
so a struct copy copies the embed itself, but the clone walk in the section below skips embeds.

## A STRUCT carrying array fields copies through its generated `ΔClone()`

The aliasing above is not confined to array-typed variables: a struct whose FIELD is a fixed-size
array has exactly the same problem one level up, because the plain C# struct copy carries the
field's `array<T>` header — and therefore its shared `T[]` — into the copy. crypto/sha256's `Sum` is
the canonical case; it copies the digest *precisely* so it can finalize the copy while the caller
keeps writing the original:

```go
type digest struct{ h [8]uint32; x [chunk]byte; nx int; len uint64; is224 bool }

func (d *digest) Sum(in []byte) []byte {
	d0 := *d                 // Go copies h and x INLINE
	hash := d0.checkSum()    // …then destroys d0's state finalizing it
	return append(in, hash[:]...)
}
```

With a copy sharing `h` and `x`, `checkSum` would destroy the CALLER's running state: every second
`Sum` on one hash would return a different digest. <!-- history: sha1/sha256/sha512 and cryptotest.TestHash's
SumAppend/ResetState/OutOfBoundsRead/StatefulWrite subtests failed this way before the struct clone;
TestGolden's write-half, Sum, write-rest sequence produced the hash of the empty string. -->

The converter treats such a struct exactly like an array. `typeNeedsValueClone`
(`arrayCloneOperations.go`) is the widened gate — a fixed-size array, **or** a struct carrying one
in a field, directly or through another such struct — so every site the array machinery already
covered (assignment/var-decl RHS, composite-literal element and keyed field, map key and value,
return, channel send, `append` element, range key/value, function/func-literal parameter and value
receiver) clones a struct too. The struct declaration is stamped with the fields that need it, and
go2cs-gen turns the stamp into the deep copy:

<!-- source: src/core/crypto/internal/fips140/sha256/sha256.cs:52-58, 210, 213-215 -->
```csharp
partial struct Digest {
    internal array<uint32> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal bool is224; // mark if this digest is SHA-224
}
…
public static slice<byte> Sum(this ref Digest d, slice<byte> @in) {
…
    ref var d0 = ref heap<Digest>(out var Ꮡd0);
    d0 = d.ΔClone();
    var hash = Ꮡd0.checkSum();
```

A plain `d0 = d;` would share the two arrays between the copies.

<!-- illustration: not converter output -->
```csharp
// generated (go2cs-gen StructTypeTemplate)
internal partial struct digest : IGoValueClone
{
    public digest ΔClone()
    {
        digest copy = this;
        copy.h = h.ΔClone();
        copy.x = x.ΔClone();
        return copy;
    }

    object ICloneable.Clone() => ΔClone();
}
```

Four details make this correct and collision-free:

- **The CONVERTER decides which fields clone, not the generator.** Only the converter has the Go
  type information, and it must agree with itself at the copy sites; `[GoValueClone("h", "x")]`
  (golib `GoValueCloneAttribute`) is that single source of truth. A defined type over such a struct
  (`type IpMaskString IpAddressString`, whose underlying holds a `[16]byte`) is emitted as go2cs-gen's
  inherited wrapper, so it is stamped `[GoValueClone("Value")]` and its clone forwards to that one
  member — without it, `syscall.IpAddrString`'s own clone had no `ΔClone` to call (CS1061).
- **The stamp is written on the `package_info.cs` accessibility record**, not on the declaration
  above — `[GoValueClone("h", "x")] internal partial struct digest {}` — so the converted source
  keeps the shape of the Go struct it came from. `TypeGenerator` reads it off any part of the
  partial type, which is also what lets a hand-owned conversion keep stamping it inline. See
  [Extended attributes](source-generators.md#extended-attributes-what-stays-on-the-declaration-and-what-moves).
- **The method is NOT named `Clone`.** A Go type may declare its own `Clone` method, which converts
  to an EXTENSION method on the package class — and an instance member of the same name silently
  SHADOWS it. Vendored `x/crypto/sha3`'s `func (d *state) Clone() ShakeHash` is the real case: its
  recv-overload forwarder bound to the generated `state Clone()` and failed CS0029. The name is
  `Symbols.ValueCloneMethod` (`ΔClone`), reusing the same `Δ` collision-avoidance marker the
  promoted-accessor rename uses. Copy SITES for plain arrays keep golib's public `Clone()`; only the
  generated bodies use the uniform name, which `array<T>` and the named-array/array-view wrappers
  alias to their own `Clone()` so one call form covers every clone-needing field type.
- **`array<T>.Clone()` recurses through the marker.** As it re-clones an element that is itself an
  array (`[2][3]int`), it clones an element that is one of these structs (`[2]digest`) through
  `IGoValueClone`/`ICloneable`.
- **EMBEDDED members are never listed and never cloned.** go2cs-gen holds an embed as an INLINE
  field, so the plain C# struct copy already copies it, as Go does. What the skip still costs is gap
  (4) above: an array reached only through an embed is not seen, so a struct that needs cloning only
  because of such an array is not stamped, and that array's backing stays shared.

A BLANK or unnamed parameter is skipped: it is emitted under a synthetic name and can never be
referenced, so there is nothing for the copy to protect — and the preamble would otherwise be
written against the empty analyzed name (` = .ΔClone();`, CS1525 ×2 in `log/slog`'s benchmark
`Handle(disabledHandler, context.Context, slog.Record)`, every parameter of which is blank). The
array-typed arm skips a blank parameter the same way.

(Guarded by the `StructArrayFieldValueCopy` behavioral test — one output-compared line per site
class: pointer-deref copy, ident copy, selector copy, composite-literal element, nested-struct copy,
by-value parameter, returned field, array and slice index, range value, map value, value receiver.
Validated end-to-end by crypto/sha1, crypto/sha256, crypto/sha512 and bufio, whose only residue is
the `alloc-profile` disclosure the extra managed allocations force.)

## Nil-vs-empty slice identity (`s == nil` is representation nilness, not emptiness)
Go distinguishes a **nil** slice (nil backing pointer) from a **non-nil empty** slice (a real backing
pointer with zero length), and programs observe the difference through `s == nil`, `reflect.DeepEqual`,
and marshaling — bytes' `TestTrim`/`TestTrimFunc`/`TestClone` assert it directly (`TrimRight` of a
non-empty slice trims *in place* and stays non-nil; `Clone` of a non-nil input must return non-nil;
the `[]byte{}` *want*-side literals must not classify as nil). golib `slice<T>` carries the
distinction in its representation — the backing `m_array` field is `null` exactly for the nil slice —
and every observation and construction path keeps it:

- **`operator ==(slice<T>, NilType)`** tests `m_array is null` (`IsNil`) — representation nilness — so
  a zero-length zero-capacity view (`[]byte{}`, `[]byte("")`, `x[len(x):]`) is not nil.
- **`operator ==(slice<T>, slice<T>)`** is Go slice-**header identity** (same backing array reference
  and native base, offset, length, capacity). Go forbids comparing two slices, and the nil comparison
  `s == nil` binds the `NilType` operator above, so no converted code binds this one. (Neither did the
  earlier spelling `s == default!`: `NilType` converts implicitly to `slice<T>`, so C# prefers the
  `NilType` operand for a `default` literal. Against the default header `(null, 0, 0, 0)` identity is the
  nil test anyway, which GolibTests `NilHeaderEqualityTests` checks for every construction kind.) Structural equality is on the `Equals` overloads for C#-side
  collection use, and the generated named-type wrappers bind `Equals`, not this operator.
- **`Reslice`** (every `s[a:b]`/`s[a:b:c]`) returns `default` for a nil source, never a fresh empty
  array: reslicing nil is legal only within zero bounds, and Go's `nil[0:0]` **is** the nil slice (the
  result shares the nil backing pointer).
- **`Append`** with **zero elements** returns the source unchanged. Go's `append(s)` (and
  `append(s, empty...)`) returns `s` itself — no growth is needed, so the same header comes back: nil
  stays nil, and bytes.Clone's `append([]byte{}, b...)` with empty `b` returns the non-nil literal.
  (`builtin.widen`, the generic pointer-instantiation projection, likewise projects only a nil source
  to nil, not any empty one.)
<!-- history: operator ==(slice, NilType) once tested Length == 0 && Capacity == 0; operator ==(slice, slice)
was structural content equality; Reslice laundered a nil backing into a fresh empty array (m_array ?? []); and a
zero-element Append allocated a fresh empty slice for a nil source. -->

The full identity enumeration golib maintains (invariant: **nil ⟺ `m_array is null`**):

| Construction | Go identity | golib path |
|---|---|---|
| `var s []T`, struct/element zero values | nil | `default(slice<T>)` — null backing |
| `[]T(nil)`, nil literal in slice context | nil | `T[]`-taking ctors map null → `default` |
| `nil[0:0]`, `nil[0:0:0]` | nil | `Reslice`/bounded ctors preserve the null backing |
| `append(nilSlice)` — nothing to add | nil | `Append` returns the source header unchanged |
| `[]T{}` composite literal | non-nil empty | `new T[]{}.slice()` — real empty array |
| `[]byte("")` / `[]rune("")` / conversions of empty strings | non-nil empty | span/`@string` paths materialize a real array |
| `make([]T, 0)` | non-nil empty | parameterless ctor / `Make` — real empty array |
| `s[a:a]`, `s[len(s):]` of non-nil `s` (even cap 0) | non-nil empty | `Reslice` shares the real backing array |
| `append(emptySlice)` — nothing to add | that same non-nil empty | `Append` identity return |
| `append(s, elems...)` with elements | non-nil | in-place or reallocated — always a real array |
| `f()` — zero-argument variadic call | nil | empty `params` pack → `Span<T>.slice()` answers `default` |
| `f(nilSlice...)` — spread of nil | nil | `ꓸꓸꓸ`/`ToSpan` of a null backing → same path |
| `f([]T{}...)`, `f(x[:0]...)` — spread of non-nil empty | non-nil empty | span carries a real reference → the copy path |

The last three rows rest on one fact. Copied through `ReadOnlySpan<T>.ToArray()`, a zero-argument
variadic call's pack would be a non-nil empty where Go passes nil, because `Array.Empty<T>()` is a real
(hence non-nil) backing array. Nil-ness crosses the C# `params` boundary intact once you read the right
property: **a span's data reference is null exactly when Go's slice header's data pointer is nil**,
which is the same invariant `slice<T>` already keeps one level up (`m_array is null` ⟺ nil).
`SliceExtensions.slice<T>(this Span<T>, …)` answers `default` for an empty pack whose reference
is null and takes the copy path otherwise, which separates all three variadic shapes above. Roslyn's
choice of `default(Span<T>)` for an empty pack is not language-guaranteed, but the degradation if it
ever changes is to a real reference (hence non-nil), never to a nil where Go
has storage; the spread half depends on golib's own `ToSpan`, not on the compiler. One residual, from
`ToSpan` rather than from the rule: a zero-size element type (`[]struct{}`) spans as `Span<T>.Empty`
when empty, so an empty non-nil slice of a zero-size type reads as nil across a spread.

(Guarded by the `SliceNilVsEmpty` behavioral test — every row of the table probed with `s == nil`,
`len`, and `cap` against `go run`; `resliceTailCapZero` discriminates the operator fix, `nilReslice`
the `Reslice` fix, `appendNilNothing` the `Append` fix, and `packZeroArgs`/`spreadNil` versus
`spreadEmptyLiteral`/`spreadMakeZero`/`spreadResliceTailCapZero` the variadic rows, with
`variadicDerived` checking that a pack's nil-ness survives the reslice, no-op append, and
re-spread a callee typically performs before observing it. `NilSliceConversion` continues to guard
the `[]T(nil)` conversion row.)

## `append` grows a slice exactly as Go's `growslice` does

`cap()` after an append is observable, so golib computes the capacity a grown slice gets the way Go's
`runtime.growslice` does, for every element type:

```go
var b []byte
b = append(b, 1)
fmt.Println(cap(b)) // 8: the allocator's smallest size class
```

1. **`nextslicecap`.** Double the old capacity below 256 elements. From 256 on, step by
   `(newcap + 3*256) / 4`, a smooth move toward ×1.25. A need past double the old capacity takes the need
   itself.
2. **`roundupsize`.** Multiply by the element's Go size and round up to the allocator's size class
   (`GoSizeClasses.RoundUpSize`), then divide back. A pointer-bearing object larger than
   `goarch.PtrSize * goarch.PtrBits` bytes (512 on amd64, the target the standard library is converted
   for) carries an 8-byte malloc header inside its class, so a `[]string` steps 32, 71, 143. A request
   past 32 KiB rounds up to whole 8 KiB pages.

The element's Go size and whether it holds pointers come from golib's type layout (`GoReflect.TryGoSizeOf`,
`GoPtrBytesOf`), read once per element type (`GoGrowthFacts<T>`). An **unnamed array element** is the
exception: `[][2]uint64` and `[][3]uint64` are both `slice<array<ulong>>`, because a managed `array<T>` carries
its length in the value, not the type. So each growth reads the element's dims off a value it appends (a
nested `[2][3]byte` through its first element), and its size and pointer facts from those dims; no answer is
cached per C# type. A zero-length array is zero-size in Go, so its slice grows to exactly its new length, as
`[]struct{}` does. `reflect.Value.Grow` has no appended value, so it reads an element of the slice, or the
length the slice recorded when it was created. A growth with neither keeps step 1 alone, and
`GoSizeClasses.UnroundedElementTypes` records the type. A nil
slice grows through the same rule, so its first backing is a size class (`append([]byte(nil), 1)` has cap 8,
not 1). `reflect.Value.Grow` reaches the same function, as Go's
`reflect_growslice` does. `internal/bytealg.MakeNoZero` (strings.Builder's buffer) reads the same table.

The table and constants are Go's (GOROOT/src/internal/runtime/gc). The repoguard test
`TestGolibSizeClassesMatchTheToolchain` fails when they drift from the pinned toolchain, and proves that
golib's smallest-class-at-least-as-large search equals Go's two lookup tables at every small size.
GolibTests' `GoGrowSliceTests` replays twenty-six growth sequences recorded from Go itself
(`testdata/growslice`, under `-gcflags='-N -l'`), and `TestGoGrowSliceExpectationsMatchTheToolchain`
re-runs that probe.

**The stack buffer is modeled separately.** Go 1.25's stack buffer of a non-escaping append, and Go
1.26's move to the heap of a buffered slice that escapes once (its growth a size class at a time inside
the buffer when its capacity is observed), are the compiler's decisions after inlining; the converter
reads them from the compiler and golib reproduces them: see the next section.

## A non-escaping append can take Go's 32-byte stack buffer

Since Go 1.25 the compiler gives an append whose result does not escape a 32-byte backing store in the
frame of the function that appends. At a variable's first qualifying append, when the slice must grow, is
empty and the new length is at most K = 32 / the element's size, the slice becomes that buffer, with
capacity K:

<!-- source: src/tests/Behavioral/StackAppendBackingStore/main.go:13-20 -->
```go
func loopCaps() {
	var x []int64
	for i := 0; i < 5; i++ {
		x = append(x, int64(i))
		fmt.Print(cap(x), " ")
	}
	fmt.Println()
}
```

Go prints `4 4 4 4 8`, where growslice alone gives `1 2 4 4 8`. A flag in the frame records that the
buffer is taken, so a function takes it at most once per call: a slice reset to nil and refilled grows as
growslice does. An element of zero size or over 32 bytes, a spread `append(x, s...)`, a slice that is not
empty, and a variable whose address is taken never take it.

Which appends qualify is the compiler's decision, made after inlining and escape analysis, so the converter
reads it from the compiler instead of re-deriving it (`src/go2cs/stackAppendHints.go`). For each package
it runs `go build`, or `go test -c` for test files, with `-gcflags='-m -d=variablemakehash=vy'` for
windows/amd64, linux/amd64 and darwin/amd64, whatever the conversion's own target. It hints a site when:

- the site's own copy does not escape, or the variable is one whose escape Go moves to the heap (below);
- the backing-store check logged the variable's declaration, which it does not for a variable whose
  address is taken;
- every copy inlined elsewhere in the package that can be attributed to the site agrees, and none that
  cannot be attributed exists (a callee with more than one append, inlined, leaves all of them unhinted);
- every target that compiles the site picks it as the variable's first qualifying append, and none refuses
  it.

The function that appends declares one `stackAppend<T>` per hinted variable at its entry, and the site
passes it to golib's `append`:

<!-- source: src/tests/Behavioral/StackAppendBackingStore/main.cs.target:8-17 -->
```csharp
internal static partial void loopCaps() {
    var xᴷ = new stackAppend<int64>();

    slice<int64> x = default;
    for (nint i = 0; i < 5; i++) {
        x = append(xᴷ, x, (int64)i);
        fmt.Print(cap(x), " ");
    }
    fmt.Println();
}
```

`ᴷ` (U+1D37, a modifier-letter capital K) names the buffer after its variable. Two hinted variables of one
function can share a name in sibling scopes, as strconv's `unquote` has two `buf`s in separate cases. Their
buffers are declared together at the entry, so the second takes `Δ` and an ordinal, as a second declaration of
a Go name does: `bufᴷ` and `bufᴷΔ1`. A function literal's own buffer may repeat a name its enclosing function
uses, because C# lets a lambda's local shadow an enclosing one. The overload takes the
buffer as an `in` parameter, which C# passes by reference to the caller's local with no keyword at the
call, and sets the flag through that reference. That is sound only when the argument is a plain local of
the calling method or lambda that no lambda captures, so the go2cs-gen analyzer GO2CS0007 makes any other
argument a build error. The public overload is a small `AggressiveInlining` head: it only checks whether
the append may take the buffer, or has a moving buffer to track, and otherwise calls the ordinary
`slice<T>.Append`. The take itself is the `NoInlining` `appendBuffered`, so both of its returns reach the
caller through one result slot that the site's next call overwrites. Inlined, the store-taking return
would build its slice in a stack temp the JIT reports for the caller's whole frame, keeping the first
buffer reachable after the slice outgrows it, where Go's dead stack buffer is not (runtime's
`TestGCTestIsReachable` reads the difference). `move2heap`, below, is `NoInlining` for the same reason. A variable
declared inside a function literal gets its buffer at the top of the
lambda's body, one per call as in Go. An outer variable appended inside a function literal gets none:
the literal captures it by reference, so Go lowers that append in place.

Taking the buffer charges no allocation to golib's allocation counter, because Go's buffer is on the stack.

**A variable that escapes once is moved to the heap there.** Go 1.26 also gives the buffer to a variable that
escapes, when its only escape is one statement outside its loops: a `return` of it (or a bare `return` of it as a
named result), an assignment of it (`*p = s`, `h.s = s`), or a `range` over it (cmd/compile/internal/slice). The
compiler inserts `s = move2heap(s)` ahead of that statement, so `var r []byte; for ... { r = append(r, b) };
return r` appends into the buffer and allocates once, at the return, for the length's size class: `byteSlice(9)`
in runtime's slice_test.go makes one allocation with capacity 16, where growslice alone makes two. The same pass
logs that statement's position under `-d=variablemakehash` (a `return` at its keyword, an assignment at its
token, a `range` at its `range` keyword), so the harvest reads the move from the log. Such a variable's site
qualifies even though `-m` says its append escapes, and the statement is converted with the move ahead of it:
`r = move2heap(rᴷ, r);` and then `return r;`, and the variable's buffer is declared `new stackAppend<T>(moves: true)`. A slice that grew out of the buffer is returned as it is, and one
still in it is copied to a counted heap array. A range over a slice that never escapes is a move too: Go copies
the slice out of the buffer before the loop. When several such variables escape in one statement, as in
`return a, w`, each is moved, one line per variable ahead of it.

When the variable's capacity is observed, by `cap(s)`, `s = s[i:j]`, `s = []T{...}` or passing `s` to a call,
the buffer is declared `new stackAppend<T>(moves: true, capacityUsed: true)`. Then every append passes it, and a growth that
fits the buffer takes the next size class of the new length instead of doubling (Go's `growsliceBuf`). The move
keeps the capacity (`moveSlice`), where otherwise it is the length's size class (`moveSliceNoCap`). runtime's
`byteCapSlice(17)` returns capacity 24 after one allocation. GolibTests' `StackAppendMove2heapTests` replay
runtime's TestAppendByteInLoop, TestAppendPtrInLoop and TestAppendByteCapInLoop tables row for row, and the
converter's `stackAppendMove2heap_test.go` holds the emission of each shape.
GolibTests' `StackAppendTests` hold the runtime rule, and the `StackAppendBackingStore` behavioral test
compares nineteen arms with Go.

**Conversion time.** The harvest compiles the package, so it adds to conversion time; a package with no
candidate append runs no harvest. With an empty Go build cache, it compiles the package and everything it
imports for all three targets. The build cache keeps those compiles, so a later conversion pays far less,
and an edited package recompiles only itself. A `main`
package costs more on every conversion, because each of its three harvests also links a binary. Within one
converter run each harvest runs once per package, target, toolchain and build configuration, whatever
the number of targets being emitted.

**What is not hinted.** A copy of an append inlined into another package is invisible to that package's
own harvest, so the site follows its own package's copies alone. A hinted site inside a function body that its
package converts by hand (a `*_impl.cs` companion) reaches only the converted placeholder, so the hand-written
body grows as growslice does. The standard library's sites of this kind are all in reflect: the parameter and
result lists `rtype.Method` builds, `FuncOf`'s argument list, and `StructOf`'s method set and type string.
<!-- The five, one hint each: GOROOT/src/reflect/type.go:660 `in = append(in, t)` and :666 `out = append(out, toRType(ret))` (rtype.Method), :1957 `args = append(args, t)` (FuncOf), :2335 `methods = append(methods, abi.Method{` and :2423 `repr = append(repr, ';')` (StructOf). Counted by COORD's std legs at e14cff064f, which emitted 5 hints fewer than predicted per target for this reason. --> A conversion whose toolchain predates
Go 1.25 gets no harvest output and so no hints, which is Go's own behavior before 1.25.

## A slice of a ZERO-SIZE element type carries no storage — `make([]struct{}, math.MaxInt)` allocates nothing

Go's `struct{}` has size 0, so `mallocgc(0, …)` returns the address of the runtime's global
`zerobase` and charges no malloc: `makeslice` multiplies the element size by the capacity and compares
*that* against `maxAlloc`, which for a zero-size element is 0 whatever the length. `make([]struct{}, n)`
therefore succeeds for every `n` up to `math.MaxInt`, and Go's own library leans on it — `slices.Concat`,
`slices.Repeat` and their tests use `[]struct{}` precisely BECAUSE it exercises the length arithmetic at
`MaxInt` without touching memory.

A real backing array for the same expression would panic `makeslice: len out of range` at
`Array.MaxLength` — a ceiling Go does not have here, because Go has nothing to allocate — and
`slices.TestRepeat` and `slices.TestConcat_too_large` would die on their OWN `make([]struct{}, MaxInt)`
before reaching the function under test.

**The predicate is a SHAPE question, not a size question.** `Unsafe.SizeOf<T>()` cannot answer it: C#
gives an empty struct one byte, so every zero-size Go type measures 1. What survives the conversion
faithfully is the FIELD SET — a Go struct is zero-size exactly when it has no fields of nonzero size, and
the emitted C# struct carries the same fields — so `GoZeroSizeFacts<T>` asks that instead, recursively,
with "no instance fields at all" as the base case (golib's `EmptyStruct` for an anonymous `struct{}`, and
every `partial struct noCopy { }` the converter emits for a named one). The answer is a
`static readonly` per closed `T`, so every gate written against it folds at JIT time and no ordinary
element type pays for the branch.

Under it, a zero-size slice's `m_array` is a **non-null placeholder** — one shared single-element array,
golib's `zerobase` — so `s == nil` keeps the representation-nilness rule above (a `make`d one is never
nil, the zero header still is), while every window bound is checked against `m_length`/`m_capacity`
alone. Each operation Go answers from length arithmetic does the same here: `make` skips the allocation
and the `Array.MaxLength` ceiling (Go's `len < 0` rule stays), `Reslice` rebuilds the header directly,
`append` bumps the length and grows the capacity to exactly the new length, as Go's zero-size
`growslice` does, `copy` returns `min(len(dst), len(src))`
and moves nothing (Go's `memmove` of `n * 0` bytes), `clear` is complete before it starts, indexing
answers the ONE shared element (Go computes `&s[i]` as `data + i*0`, so every index names the same
address), and `range` is a counted loop.

**A slice-typed spread operand travels AS THE SLICE IT IS.** `Span<T>.Length` is `int32` while a Go
slice length is `int`, so a spread projected to a span (`ꓸꓸꓸ`, golib's `Span<T>` projection of a slice)
could not carry `slices.Grow`'s `append(s[:cap(s)], make([]E, n)...)` to `n == MaxInt`. The emission
therefore passes a slice-typed operand whole to golib's `appendꓸꓸꓸ`, whose name carries the spread:
<!-- history: until 2026-08-27 a spread emitted as append(dst, src.ꓸꓸꓸ), projecting every operand to a span. -->

```go
hello.cipherSuites = append(hello.cipherSuites, defaultCipherSuitesTLS13...)
s2 := append(s[:i], v...)        // in a generic body over S ~[]E
```
<!-- illustration: adapted, owed a real excerpt -->
```csharp
hello.Value.cipherSuites = appendꓸꓸꓸ((~hello).cipherSuites, defaultCipherSuitesTLS13);
var s2 = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), v);
```

golib has an `appendꓸꓸꓸ` overload for each kind of operand. A `slice<T>` operand binds the exact
`appendꓸꓸꓸ<T>(slice<T>, in slice<T>)` and is not boxed; a named slice binds it too, through its
implicit conversion to `slice<T>`, which is a view of the same window. An `sslice<T>` variadic pack
binds `appendꓸꓸꓸ<T>(slice<T>, in sslice<T>)` and is read as the span it already is, with no copy to
reach the call. The `ISlice<T>` overload remains for other operands. Its interface parameter boxes a
struct operand, which is why a `slice<T>` has its own exact overload. The constrained form,
`appendꓸꓸꓸ<S, E>`, takes `ISlice<E>`, with an `sslice<E>` twin.
<!-- src/core/golib/builtin.cs:542 `public static slice<T> appendꓸꓸꓸ<T>(slice<T> slice, in slice<T> elems)`; :555 `public static slice<T> appendꓸꓸꓸ<T>(slice<T> slice, in sslice<T> elems)`; :529 `public static slice<T> appendꓸꓸꓸ<T>(slice<T> slice, ISlice<T> elems)`; :1446 `public static S appendꓸꓸꓸ<S, T>(S s, ISlice<T> items)`; :1454 `public static S appendꓸꓸꓸ<S, T>(S s, in sslice<T> items)`. -->
<!-- The exact slice<T> overload landed in golib 17785ec85 (2026-10-22). Before it a slice<T> operand bound the ISlice<T> overload and was boxed: 56 B per call that AllocationCounter never sees; Go 1.26's bytes TestReplace read it as 56 objects per run for `append([]byte(nil), s...)`, which allocates nothing in Go (builtin.cs's doc comment on that overload). -->

Inside golib the window's own span serves every copy a span can express (managed backings are
int-bounded by `T[]`, so allocation counts and in-place/grow behavior are byte-identical to the span
core), zero-size windows route by pure length arithmetic BEFORE any span form (which is what lets
`TestConcat_too_large`'s `make([]struct{}, math.MaxInt)` fakes flow through `Concat`'s Grow chain
allocation-free, a grown capacity being exactly the new length), and an `ISlice<T>` operand that is
not a `slice<T>` (a named-slice wrapper in a constrained body) lends its own `ꓸꓸꓸ` projection — one
interface call, inside the core. Two shapes are deliberate: the constrained form takes BOTH type
arguments explicitly (`appendꓸꓸꓸ<S, E>`), because a constraint surface does not participate in C#
inference; and the name is DISTINCT from `append` by design — sharing the overload set re-entered
the C#14 params/betterness thicket (measured CS0121 against the constrained span twin), and the
converter alone mints these calls.<!-- Census at the landing: 401 corpus sites across 151 files routed. --> String
spreads keep the span route (their spread is a byte projection, not a slice). Emission:
`convCallExpr`'s append classification (`spreadArgAsSlice`/`appendTypeArgs`) + `convExprList`'s
spread arm; golib: `builtin.appendꓸꓸꓸ` over `slice<T>.AppendWindow` (the exact `slice<T>` form) and
`slice<T>.Append(in slice<T>, ISlice<T>)` (the interface form, which sends a `slice<T>` to the same window
arm). Go's own `slices` tests depend on it.<!-- At the landing: slices validated 119 matched · 3 disclosed. -->

**Append-of-make grows in place without the make.** <!-- landed 2026-09-24, REC-C §B --> Go's compiler recognises
`append(x, make([]T, n)...)` (walk's `isAppendOfMake`, "extendslice") and extends `x` by `n` zeroed elements
without allocating the `make`, which is why `slices.Grow` says it "allocates only once". The emission
renders exactly that operand as the length-only `makeꓸꓸꓸ<T>(n)`, and nothing else in the call changes:

<!-- source: src/core/slices/slices.cs:473 -->
```csharp
s = subslice<S, E>(appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, cap(s)), makeꓸꓸꓸ<E>(n)), 0, len(s));
```

golib's `makeꓸꓸꓸ<T>` validates the length as `make` does (same panic), and `appendꓸꓸꓸ` over it calls
`slice<T>.AppendZeroed`. That mirrors `Append`'s arms: the zero-size arm, so `TestConcat_too_large`'s
`MaxInt` fakes still allocate nothing; nil; in place, zeroing the shared backing's new elements, which a
stale write beyond `len` would otherwise leak; and growth through `GrowCapacity`, Go's `growslice`. So no `cap()`
moves. Refused, each keeping the make-then-append emission:
- a capacity argument (Go does not recognise it either);
- an element whose zero value must be constructed (`arrayElemFactory`);
- fixed-array dimension cargo;
- a `make` bound to a name.

The stdlib footprint is a one-operand swap at each site.<!-- Stdlib footprint at `fa18863b94`: 19 lines in 16 files on windows, 18 in 15 on linux and darwin. --> (Guarded by:
- the `AppendOfMake` behavioral test;
- the converter's `TestAppendOfMakeEmitsTheLengthOnlyOperandOnlyForGosExtendSliceShape`, controlled both
  ways;
- GolibTests' `AppendOfMakeTests`.)

Known and deliberate divergence: Go's OTHER zero-size shape is the zero-length array (`[0]T`, and
`[N]struct{}`). go2cs emits a Go array as `array<T>`, whose backing is a managed reference field, so such
a type classifies as NON-zero-size and keeps the allocating path — the honest answer for the
representation as it stands, since claiming zero-size for a type whose C# shape genuinely carries a
reference would put a wrong element ref in front of every consumer.

(Guarded by `GolibTests.ZeroSizeSliceSemanticsTests`, which pins every operation at `MaxInt` scale plus
the positive controls — a negative `make` length still panics, and an ordinary element type keeps its
allocation ceiling — because a fix that got any ONE operation wrong would still let a package row go
green while leaving the storage-free path unsound for the next consumer.)

**Named slice/map/channel wrappers (defined types).** The distinction extends to DEFINED types
(`type S []int`, generated as go2cs-gen `InheritedTypeTemplate` wrappers). The comparison Go permits on
a named slice/map/channel is `x == nil`; the converter renders it as `s == nil` (before parity seat #24,
`s == default!` in value context), and — verified against the emitted C# — **both spellings bind the
wrapper's `operator ==(S, NilType)` overload**, not the same-type `operator ==(S, S)` (reverting the
latter has no observable effect; reverting the former flips the result). For a **slice** wrapper that
overload delegates to `slice<T>`'s own `== NilType` — REPRESENTATION nilness (null backing array) — via
`value.m_value == nil`, so `IntSlice{} == nil` is false while the zero value stays nil. It cannot use
`value.Equals(default(S))`: the slice wrapper's `Equals` (from `ISliceTypeTemplate`'s `Equals(ISlice<T>?)`)
is structural *content* equality, which would classify an empty non-nil named slice (`S{}`, `make(S, 0)`,
an `s[len(s):]` tail) as nil. The other nil-comparable kinds: **map** and **channel** wrappers use
`Equals(default)` — they declare no structural `Equals`, so it falls back to reference identity through
the backing field
(`map<K,V>.Equals` is `ReferenceEquals(m_map, …)`; `channel<T>` compares its queue by reference).
Array/numeric/string/struct/`any` wrappers are not nil-comparable in Go (and the pointer wrapper
already uses a reference-identity `Equals` override), so they keep the structural default; the same-type
`operator ==(S, S)` is likewise left untouched (Go forbids comparing two slices, so no converted code
reaches it). This lives in go2cs-gen, so the `.cs.target` goldens do not show it; the behavioral run
and a corpus build do. <!-- history: the slice wrapper's overload emitted value.Equals(default(S)) until this
fix, gated on the full behavioral suite (four phases) plus a full corpus build. -->

`NilType`'s `operator ==(ISlice?, NilType)` is `slice is null` (representation nilness).
`slice<T>.Source` (and every wrapper's `IArray.Source`) materializes a DETACHED copy
(`ToSpan().ToArray()`) and is never null, so a `{ Length: 0, Capacity: 0, Source: null }` pattern could
never match. <!-- history: the operator once carried that dead arm. --> No converted `s == nil` routes through
this interface operator: a concrete `slice<T>` binds its own `== NilType`, and interface/`any`
comparands bind `NilType`'s `object` arm.

(The named-wrapper rows are guarded by the `NamedSliceNilVsEmpty` behavioral test — named
slice/map/channel zero value (nil) vs empty literal (non-nil), plus the slice `resliceTailCapZero` and
`nilReslice` discriminators, output-compared vs `go run`; it fails if the wrapper's `== nil` regresses
to structural equality.)

## A composite literal omitting a fixed-array field keeps the zeroed backing

A Go struct's fixed-array field is emitted with a field initializer carrying its Go length
(`badCharSkip [256]int` → `internal array<nint> badCharSkip = new(256);`), and C# runs field
initializers in every explicitly declared constructor — but an argument the composite literal OMITS
arrives at the generated **parameterized** constructor as the zero value `default!`, whose backing `T[]`
is null, and assigning it would null the initializer's backing, so the field's first walk would NRE
(strings' Boyer-Moore `stringFinder{pattern: …, goodSuffixSkip: …}` never sets `badCharSkip`). The
generated constructor therefore guards exactly the fixed-array members: <!-- history: an unguarded assignment
was the TestFinderCreation/TestFinderNext blocker, Phase-4 row R8. -->

<!-- illustration: not converter output -->
```csharp
if (badCharSkip.Source is not null) this.badCharSkip = badCharSkip;
```

`array<T>.Source` intentionally returns the RAW backing reference (null discriminates a
never-constructed zero value), and keeping the initializer for a zero-value argument is precisely
Go's semantics — the zero `[N]T` IS the zeroed backing the initializer produced. A constructed
argument assigns as given (see the copy-semantics sections above for the literal-argument clone).
golib `array<T>` reads are also **null-safe**: a bare `default(array<T>)` (a zero value no constructor
ever touched) enumerates/compares/prints as an EMPTY array and panics Go-style on any index, instead of
throwing NRE — mirroring `@string`'s null-safe zero value. The empty view is an approximation: the
declared length only exists where a constructor or initializer ran, which is why the converter
constructs every zero value that carries a fixed array — a `var z holder` local is `holder z = new();`
([A struct's array fields get their fixed length](#a-structs-array-fields-get-their-fixed-length-from-a-generated-parameterless-constructor)),
and `make([]S, n)` builds its elements ([`make([]E, n)` constructs its
ELEMENTS](#makee-n-constructs-its-elements-by-the-same-rule)).
(Guarded by the
`ZeroValueArrayField` behavioral test — the literal-omission shape ranged/indexed/printed vs Go,
plus an explicit-argument control.)

## A fixed-array composite literal carries its DECLARED length (`.array(N)`)

A `[N]T{…}` literal is **N** long however many elements it writes — Go zero-fills the rest, so
`[8]byte{}` is eight zero bytes and `[8]byte{1, 2}` is `1, 2` followed by six zeros. The literal
renders as a C# element array projected through golib's `.array()` extension, and that element
array holds only the elements actually written, so a bare projection would produce an array as long as
the LITERAL rather than as long as the TYPE: `[8]byte{}` would be length **0**, compile cleanly and then
panic on first use (`index out of range [7] with length 0` — math/rand/v2 `chacha8`'s `Seed`). The
projection therefore takes the declared length:

```go
a := [8]byte{}          // eight zero bytes
b := [8]byte{1, 2}      // 1, 2, then six zeros
c := [3]byte{1, 2, 3}   // already full
```

<!-- illustration: not converter output -->
```csharp
var a = new byte[]{}.array(8);
var b = new byte[]{1, 2}.array(8);
var c = new byte[]{1, 2, 3}.array();      // full literal keeps the plain projection
```

Only a **short** literal takes the length argument. A full literal — and every `[...]T{…}`
ellipsis literal, whose length *is* its element count — already yields the right length and keeps
the plain `.array()` form. A **slice** literal is genuinely as long as its elements (`[]byte{}` IS
empty) and never pads; its `.slice()` projection takes no length. golib's `array<T>(T[] source, int length)` constructor
does the zero-filled copy,
which is deliberately distinct from the `array(slice<T>, nint)` slice-to-array *conversion* ctor
(there a short source is a Go panic; here it is the normal case).

The **indexed/keyed** form carries the length too. A keyed literal whose indices all fold to
constants renders as `new array<T>(N){[i] = v}`. Constant-key detection is tracked separately from
the maximum index, so a literal whose only key *is* 0 (`[8]byte{0: 9}`) is still constant-keyed rather
than falling to the `SparseArray` projection, whose extent is `max index + 1`, not `N`; and the
`SparseArray` projection — still used for a key that is constant but not a literal
(a `const` identifier), which `SparseArrayIfaceElem`'s `[kLast]shape` registry exercises — also
carries the declared length. (Guarded by the `ArrayLiteralDeclaredLength` behavioral test: empty,
partial, full, ellipsis, keyed, zero-keyed, named, aliased, package-level, non-byte element types,
a tail write proving the backing is really N long, and a `[]byte{}` slice control, output-compared
vs `go run`.)

**…and the padding itself needs the element factory when the ELEMENT's zero value must be
constructed.** The length argument sizes the OUTER dimension and fills it with `default(T)`, which is
not usable storage for an unnamed nested fixed array (`[2][3]uint8` emits `array<array<uint8>>`, and
the inner length lives only in the Go type) or for a struct whose fixed-array field initializer runs
only inside a declared constructor. Without a factory `[2][3]uint8{}` would be 2 long with two
**zero-length** elements, disagreeing with the DECLARED form `var x [2][3]uint8`, which routes through
the zero-value construction ladder — visible through `reflect`, where a constructed
`ArrayOf(2, TypeOf([3]uint8{}))` would compare unequal to the literal-built value. <!-- closed
2026-08-26 -->

The padding carries `arrayZeroValueArgs`' element factory — that same ladder's own renderer,
reused rather than restated, so the literal and the declaration cannot drift apart. It recurses, so
depth composes:

```go
n := [2][3]uint8{}
d := [2][3][4]uint8{}
c := [2]cell{}          // cell has a [4]uint8 field
m := [2]named{}         // type named [6]byte
```
<!-- illustration: not converter output -->
```csharp
var n = new array<uint8>[]{}.array(2, () => new(3));
var d = new array<array<uint8>>[]{}.array(2, () => new(3, () => new(4)));
var c = new cell[]{}.array(2, () => new());
var m = new named[]{}.array(2);           // NAMED element: its wrapper allocates its own backing
```

All **three** routes to the padding carry it: the positional projection above (golib's
`array<T>(T[], int, Func<T>)` extension), the constant-keyed indexed form
(`new array<array<uint8>>(4, () => new(3)){[1] = …}`, through `array<T>`'s existing
`(nint, Func<T>)` constructor), and the `SparseArray` projection for a key that is constant but not
a literal. The sparse one is not simply the first with a different receiver: a sparse literal's zero
values are its **gaps**, which can sit anywhere rather than only in a tail, and enumerating a
`SparseArray` renders a gap as `default!` — indistinguishable afterwards from an element the literal
genuinely wrote. So that overload asks the sparse array which indices were SET
(`SparseArray<T>.TryGetItem`) and constructs the rest. An element whose `default(T)` is already the
Go zero value — every scalar, and every NAMED array element, whose generated wrapper allocates its
backing lazily from its own known size — renders the bare length. (Guarded by the same
`ArrayLiteralDeclaredLength` test, extended with empty/partial/full nested, three-deep, a needy-struct
element, a named-element counter-case, package-level, and both indexed forms.) <!-- failing-first measured
as `nested empty 2 0 0` against Go's `2 3 3`, followed by the panic, and `keyed nested 4 0 3 0 3` against
Go's `4 3 3 3 3`. -->

## An ELIDED element carries the same construction as the spelling it elides

Go lets a composite literal **elide** its element's own type, and in two directions: the element's
literal type (`[][2][3]int{{}}` is `[][2][3]int{[2][3]int{}}`) and, when the element type is a
pointer, the `&T` as well (`[]*[4]byte{{}}` is `[]*[4]byte{&[4]byte{}}`). Elision is pure surface
syntax — the two spellings are the same value — so the emission for the elided form must be the
emission for the written one. <!-- two places diverged until 2026-09-04 -->

**An elided `&T` whose pointee is not a struct.** The elided path renders a STRUCT pointee through the
boxed constructor `Ꮡ(new T(…))`. A generic target-typed `new(…)` for an ARRAY, SLICE or MAP pointee
would target golib's `ж<T>`, which is `abstract` (`CS0144`), so each pointee renders through the arm
that renders its shape, with the address taken around it:

```go
pa  := []*[4]byte{{}}                  // == []*[4]byte{&[4]byte{}}
psl := []*[]int{{}}
pm  := []*map[string]int{{}}
mp  := map[string]*[2]int{"a": {}}     // map VALUE, same elision
```

<!-- illustration: not converter output -->
```csharp
var pa  = new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice();
var psl = new ж<slice<nint>>[]{Ꮡ(new nint[]{}.slice())}.slice();
var pm  = new ж<map<@string, nint>>[]{Ꮡ(new map<@string, nint>{})}.slice();
var mp  = new map<@string, ж<array<nint>>>{["a"] = Ꮡ(new nint[]{}.array(2))};
```

`pa`'s emission is **byte-identical** to what the explicitly written `[]*[4]byte{&[4]byte{}}`
produces, which is the property that matters. A **struct** pointee (`[]*S{{}}`) takes the boxed
constructor.

A **named** non-struct pointee (`type nb [4]byte; []*nb{{}}`) is the one shape these arms cannot
render themselves, and it is **closed by routing rather than by copying**. Its value is built by the
generated wrapper constructor, which lives in the TYPED path's named-composite machinery (empty vs
keyed vs positional, array vs slice vs map, alias vs named) — so the elided literal is handed to
that renderer with its pointee supplied as the resolved type, and the address is taken around the
result exactly as the struct arm takes it:

```go
pna  := []*nb{{}}                 // type nb [4]byte
pnsl := []*nsl{{}}                // type nsl []int
pnmp := []*nmp{{}}                // type nmp map[string]int
```

<!-- illustration: adapted, owed a real excerpt -->
```csharp
var pna  = new ж<nb>[]{Ꮡ(new nb(new byte[4].array()))}.slice();
var pnsl = new ж<nsl>[]{Ꮡ(new nsl(new nint[]{}.slice()))}.slice();
var pnmp = new ж<nmp>[]{Ꮡ(new nmp(new map<@string, nint>{}))}.slice();
```

Each is byte-identical to the explicit `&nb{}` / `&nsl{}` / `&nmp{}` spelling beside it in the
guard, which is the assertion — elision is surface syntax, so one Go value may not have two
emissions.

The **wrap** that suggests itself here is measured wrong, and that measurement is why the routing
exists: the same pointee *without* the `&` (`[]nb{{}}`) already emits the structural projection
`new nb[]{new byte[]{}.array(4)}`, which binds the named slot through the generated implicit
conversion — but that conversion is between **values**, not between **boxes**, so
`Ꮡ(new byte[]{}.array(4))` is a `ж<array<byte>>` and cannot bind a `ж<nb>` slot.

A named type over a *nested* fixed array (`type nn [2][3]int`) needs this routing AND the element
factory its wrapper's empty literal carries (see *A NAMED array type's EMPTY literal is its zero value*
below); the guard pins `[]*nn{{}}` beside `&nn{}` as the row that requires both. <!-- history: before the
wrapper fix all three spellings (elided, &nn{}, nn{}) printed `2 0 [[] []]` against Go's
`2 3 [[0 0 0] [0 0 0]]`, the elided one agreeing with the explicit one throughout. -->

**An elided fixed-array element whose own element must be constructed.** The elided array arm
carries the element factory beside the declared length, as the TYPED path does: without it
`[][2][3]int{{}}` would size its outer dimension to 2 and leave both rows **zero length**. Both elided
array arms — positional and the `SparseArray` keyed one — route through `arrayLengthArgs` and
`arrayElemFactory`, the same single renderer the typed path and the zero-value ladder use:

```go
nested      := [][2][3]int{{}}
nestedShort := [][2][3]int{{{1, 2, 3}}}
nestedKeyed := [][2][3]int{{1: {7, 8, 9}}}
structElem  := [][2]withArray{{}}         // withArray has a [2][3]int field
```

<!-- illustration: adapted, owed a real excerpt -->
```csharp
var nested      = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.slice(), 2, 3);
var nestedShort = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3))}.slice(), 2, 3);
var nestedKeyed = GoReflect.WithElemDims(new array<array<nint>>[]{new golib.SparseArray<array<nint>>{[1] = new nint[]{7, 8, 9}.array()}.array(2, () => new(3))}.slice(), 2, 3);
var structElem  = GoReflect.WithElemDims(new array<withArray>[]{new withArray[]{}.array(2, () => new())}.slice(), 2);
```

(`GoReflect.WithElemDims` records the element's array dimensions on a new slice of arrays, which a
managed `array<T>` does not carry in its type; `reflect` reads them back where the slice has no
element to measure. `make` of such a slice takes it too, below.)

The sharpest witness is `[2][2][3]int{{}}`, where both spellings meet in ONE expression: its outer
literal is written and takes the typed padding `.array(2, () => new(2, () => new(3)))`, and its single
elided element must take the same factory, or one Go value has two lengths in the same line. A full
literal, a slice literal, and an element whose `default(T)` is already the
Go zero value (every scalar, and every named array element) take no factory.

(Both guarded by the `CompositeLiteralElements` behavioral test, output-compared against `go run`
over the elided and explicit spelling of each shape plus writes through the constructed storage.)
<!-- failing-first, measured separately: the pointer half was nine CS0144s at the Compile phase; the array half
compiled clean and failed at Output, `nested: 1 2 0 [[] []]` against Go's `1 2 3 [[0 0 0] [0 0 0]]`, then an
index-out-of-range panic on the write. -->

## A NAMED array type's EMPTY literal is its zero value — and its element may need constructing

A named type over a fixed array (`type nn [2][3]int`) lowers to a go2cs-gen wrapper around
`array<E>`, and an EMPTY composite of it (`nn{}`) is the type's zero value rather than a length-0
literal. The wrapper's renderer has a shortcut for exactly that case — `new nn(new array<nint>[2].array())`
— because `new T[N]` is already the zero-filled declared length and restating it would merely emit
`new byte[6].array(6)`. That reasoning holds for every element whose `default(T)` is already the Go
zero value, and fails for the two shapes that must be CONSTRUCTED: `new T[N]` is N copies of
`default(T)`, which is *sized but unusable storage* — the same
`default(T)`-is-not-usable-storage problem the two sections above solve, one level up and reached
through the wrapper instead of the projection: `nn{}` would have an outer length of 2 whose rows are
**length zero**, and a named array over a struct carrying a fixed-array field (runtime's `semTable`
shape, `[251]struct{root semaRoot; pad [40]byte}`) would panic on the first inner index. The empty
shortcut therefore consults `arrayElemFactory` and, when the element needs one, backs the wrapper with
the element-factory `array<T>` constructor — the same form the wrapper's KEYED branch uses, through
`arrayLengthArgs`, the one place that argument list is spelled:

```go
nnLit := nn{}                 // type nn [2][3]int
nnPtr := &nn{}
nsLit := ns{}                 // type ns [2]withArray — withArray has a [2][3]int field
nbLit := nb{}                 // type nb [4]byte — element needs nothing
```

<!-- source: src/tests/Behavioral/CompositeLiteralElements/main.cs:128, 131, 138, 147 -->
```csharp
var nnLit = new nn(new array<array<nint>>(2, () => new(3)));
…
var nnPtr = Ꮡ(new nn(new array<array<nint>>(2, () => new(3))));
…
var nsLit = new ns(new array<withArray>(2, () => new()));
…
var nbLit = new nb(new byte[4].array());
```

`nbLit` keeps the shortcut: its element needs nothing constructed.

`arrayLengthArgs`' own comment names the renderers that must carry the factory, "the named-array
wrapper's" among them, and the wrapper's empty, short and keyed literals all do. Every named array whose
element is a scalar or another named type keeps the shortcut.

**The zero value reached WITHOUT a literal is a different layer.** `var x nn`, `new(nn)` and
`default(nn)` never call the wrapper's constructor; they reach go2cs-gen's lazy backing, which constructs
its elements by its own rule — see the next section. <!-- history: this layer stayed wrong for a while after
the literal fix; of 86 named array types then, 5 had a needy element: runtime.semTable,
crypto/internal/boring/bcache.cacheTable, crypto/internal/nistec's p256Table and p256AffineTable, and a
ten-deep [2]^10 *int in runtime's GC test. -->

(Guarded by the `CompositeLiteralElements` behavioral test: the plain and explicit-pointer spellings
of both shapes, a write through the constructed storage, the elided spelling as the cross-renderer
control, and the populated, short, keyed and plain-element literals as the must-not-change controls.
The predicate has THREE arms and each has a row, because a control only tests the axis it varies: an
element needing construction (`nn`, `ns`), a plain scalar element (`nb`), and a NAMED array element
(`type ni [3]int; type no [2]ni`) — which keeps the shortcut and is still correct, because its own
wrapper allocates a backing of its own known size. That last arm is what pins `arrayElemFactory`'s
named-element exclusion from this side.) <!-- failing-first measured: the pre-fix converter reported FAIL
[Target,Output], `nnLit: 2 0 [[] []]`, `nnPtr: 2 0 [[] []]`, then an index-out-of-range panic on the write,
exit code 2 against Go's 0. -->

## An array or slice literal may MIX positional and keyed elements

Go's "all elements keyed, or none" rule is a **struct**-literal rule. An array or slice literal may
mix the two freely, and the positional elements take the indices Go computes for them: the first
element is index 0, a keyed element sets the index to its (constant) key, and each following
positional element continues from there. So the literal below is **sixteen** bytes long, not three:

```go
ip := []byte{0xfe, 0x80, 15: 0x01}   // 0: 0xfe, 1: 0x80, 15: 0x01 — length 16
a  := [8]int{1, 2, 5: 9, 10}         // 0: 1, 1: 2, 5: 9, 6: 10 — length 8, its declared one
```

Detecting a keyed literal from `Elts[0]` alone (the struct rule) would send a mixed literal down the
plain positional emission while its keyed elements still render through the key/value arm, whose sparse
form wants an assignment target that does not exist in an expression position (CS1525). <!-- history: that
was the emission before this fix: `new byte[]{0xfe, 0x80, <nil>[15] = 0x01}.slice()`. -->

A mixed literal is normalized to an all-keyed one carrying Go's own indices, which lets the
sparse-array machinery render it unchanged — and keeps the LENGTH, which is the part a wrong emission
would get silently wrong rather than loudly:

<!-- illustration: not converter output -->
```csharp
new slice<byte>(16){[0] = 0xfe, [1] = 0x80, [15] = 0x01}
new array<nint>(8){[0] = 1, [1] = 2, [5] = 9, [6] = 10}
```

An all-positional or already-all-keyed literal is untouched by construction; a literal whose keys do
not fold to constants keeps its plain emission, because an index the converter cannot compute is one it
must not invent. Guarded by `mixedKeyedComposite_test.go`, which converts both mixed forms plus the two
unmixed controls. (`net/netip`'s `TestAddrFromSlice`/`TestAsSlice` write IPv4-in-IPv6 addresses this
way; no production site in the converted standard library does.)

## A fixed-size array constructs its ELEMENTS when `default(T)` is not usable storage

`new array<T>(N)` fills its backing with `default(T)`, which is the correct Go zero value only when
`default(T)` is itself well formed. For a NESTED fixed array it is not: `[2][4]byte` emits
`array<array<byte>>`, and the inner length `4` lives only in the Go type — `array<T>` has nowhere to
carry it, so golib cannot recover it from `T`. Every element would keep a null backing, so `len(x[1])`
would report 0 where Go says 4, and the first indexed write would panic (`index out of range [2] with
length 0`) — silently, in code that compiles clean. The same holds for an element whose own zero value
needs construction: `default(T)` skips the generated constructor that runs a struct's
fixed-array field initializers and allocates its embed boxes.

Only the converter knows the element's shape, so it supplies an element factory to a golib
`array(int length, Func<T> elementFactory)` constructor:

```go
var x [2][4]byte           // len(x), len(x[1]) => 2 4
var deep [2][3][4]byte
var se [2]inner            // type inner struct { b [3]byte }
```
<!-- source: src/tests/Behavioral/NestedFixedArrays/main.cs:24, 31, 41 -->
```csharp
array<array<byte>> x = new(2, () => new(4));
…
array<array<array<byte>>> deep = new(2, () => new(3, () => new(4)));
…
array<inner> se = new(2, () => new());
```

The factory nests to any depth, and each element gets its OWN storage rather than one shared inner
array. It is emitted from every fixed-array zero-value site — local `var`, package-level `var`
(including the addressed-global `ж<>` box form), the type-ALIAS-to-array spelling, a struct's
field initializer (`internal array<array<nint>> entries = new(2, () => new(3));`), the
**heap-boxed** (address-taken) local, and the **`new([N]T)`** builtin:

```go
var leafCounts [maxBitsLimit][maxBitsLimit]int32   // addressed: copy(leafCounts[i][:i], …)
f.bits = new([maxNumLit + maxNumDist]int)
```
<!-- illustration: adapted, owed a real excerpt -->
```csharp
ref var leafCounts = ref heap(new array<array<int32>>(16, () => new(16)), out var ᏑleafCounts);
f.bits = Ꮡ(new array<nint>(316));
```

Those last two are separate emission paths. The heap-boxed declaration (`convertToHeapTypeDecl`) is
`compress/flate`'s Huffman-coder shape — `bitCounts`' `leafCounts [16][16]int32` is boxed because
`copy(leafCounts[i][:i], …)` slices an element, and its `leafCounts[level][level] = 2` needs real inner
rows. For `new([N]T)`, golib's `@new<T>()` builds the zero value through the parameterless constructor,
where `array<T>()` has no length at all, so the converter spells the length (`f.bits` is 316 long, as in
Go). A NAMED array type keeps the zero-value `@new<row>()` form for the same reason a named element needs
no factory. <!-- history: both paths once skipped arrayZeroValueArgs; leafCounts panicked with an
index-out-of-range error, taking compress/gzip and compress/zlib down, and f.bits came back length 0. -->

A NAMED array element needs no factory and is deliberately left alone: `type row [4]byte` generates
a wrapper that allocates its backing lazily from its own known size (go2cs-gen's
`m_value ??= new row(4)`), so `array<row> nr = new(2);` is already correct. Elements whose
`default(T)` is a valid zero value (scalars, pointers, slices, maps) likewise keep the bare
`new(N)`.

This mirrors go2cs-gen's `AppendZeroValueInitializers`/`NeedsConstruction`, which does the same for
struct FIELDS. (Guarded by the `NestedFixedArrays` behavioral test: inner `len`,
writes read back through inner arrays, per-element storage independence, three-level nesting,
struct/named-array elements, and the global paths, all compared against `go run`.)

## A NAMED array's ZERO VALUE constructs its elements through the wrapper's lazy backing

The section above closes every site the CONVERTER can spell. A named array type has a whole class of
sites it cannot: the wrapper allocates its backing lazily, so `default(nn)` — with no declaration for
the converter to rewrite — is a complete Go zero value that go2cs-gen materializes on first touch as
`new array<E>(N)`. That is the same `default(E)` fill one layer down, wrong for the same two element
shapes if left bare.

Measured against `go run` with that bare backing, `type nn [2][3]int` and `type ns [2]wa` (`wa`
carrying a fixed-array field — runtime's `semTable` shape) diverge at **seven distinct site kinds**,
and the arithmetic is what identifies the single cause:

| site | Go | bare backing |
|---|---|---|
| `var d nn` · `new(nn)` · a struct FIELD · a named RESULT · a map read | `2 3 [[0 0 0] [0 0 0]]` | `2 0 [[] []]` |
| `var aa [2]nn` · `make([]nn, 2)` | `2 2 3 …` | `2 2 0 …` |
| `d[1][2] = 9` | `[[0 0 0] [0 0 9]]` | `panic: index out of range [2] with length 0` |

The `2 2 0` rows are the diagnosis, not a detail: in an `array<nn>` the outer and middle dimensions
are **correct**, because the wrapper heals its own length on first touch. Only the innermost — the
one the lazy backing fills — is wrong. So all of these funnel through ONE emission, and narrowing
`zeroValueInitializer`'s named-array carve-out would reach only the two sites that have a declaration
statement (`var` and the named-result prologue), leaving five.

The construction is therefore at that one emission, and it is split across the two halves by **which half
holds the fact**:

- **go2cs-gen** answers for a STRUCT element, reusing the `NeedsConstruction` predicate it
  applies to a struct FIELD (`static`, taking its context and cache explicitly, so there is one
  definition). It owns the half the converter could not supply anyway: a
  cross-ASSEMBLY element resolves by metadata symbol rather than syntax. The rendering is the same
  NilType construction a needy field takes — `new array<wa>(2, static () => new wa(nil))`.

  The element name has to be asked in **both spellings it can arrive in**. A struct FIELD reaches the predicate already
  `global::go.`-rooted,
  because `GetStructMembers` produces rooted names; a `/*[N]E*/` descriptor's element is
  *package-alias-qualified* (`sync.atomic_package.Pointer<…>`), which is not a CLR name at all —
  every converted package class lives under the `go` namespace. Asked in that spelling alone, every
  cross-assembly element would answer false, **silently**: the wrapper would keep the bare-length
  backing. `FindUnderlyingStructSymbol` already documents
  and performs the retry for the same reason one hop over, so the lookup goes through it. The
  corpus's own control shows the symbol path is sound — `math/rand/v2`'s `ChaCha8`
  constructs its cross-package `chacha8rand.State` field from a rooted name — and a three-arm probe
  (cross-package non-generic, cross-package generic closed over a concrete type, and a generic named
  array whose element closes over its OWN type parameters) pins the axis as *cross-assembly* rather than
  *generic*: all three construct, byte-identical to `go run`.
- **The converter** answers for a nested UNNAMED array element, because nothing downstream can. The
  descriptor is `[2]array<nint>` — the inner `3` is gone — and an `array<T>`'s length is INSTANCE
  state, so a site with no instance cannot recover it. It stamps
  `/*[2][3]*/ partial struct nn /*[2]array<nint>*/;` and gen builds the factory from
  everything after the first dimension: `new array<array<nint>>(2, static () => new(3))`. This is the
  existing `GoArrayDims` cargo (same attribute, same outermost-first meaning as on a parameter or a
  field) reached one hop earlier — at construction rather than at description — with its
  `AttributeUsage` widened to `Struct`.

The discriminator is exact rather than approximate: `goArrayDims` walks unnamed arrays only, so it
returns 2+ dimensions **precisely** when the element is one. A plain element (`[4]byte`), a struct
element (`[2]wa`) and a NAMED element (`[2]ni` over `type ni [3]int`) each return one dimension and
take no stamp — and the named element must not, since its own wrapper allocates its own backing by
this very route. A dimension exceeding `int` yields no factory rather than a wrong one: Go's
pointer-to-unbounded-array idiom (`*[1<<50 - 1]byte`) puts such a length in the type system
deliberately and no such array is allocatable.

In the converted standard library (counted from the emission, not a GOROOT `go/packages` walk —
`-stdlib` defaults to `-tags purego`, so the asm-path types such a walk counts are absent from the
emission) no named fixed-array wrapper has a nested-array element<!-- 59 wrappers at the cut, corpus
footprint zero -->, and only one has a needy element: `runtime.semTable`, whose lifted anonymous-struct
element carries `pad = new(40)`. It is answered by gen, needs no stamp, and its generated backing is
`new array<semTableᴛ1>(251, static () => new semTableᴛ1(nil))`.

`bcache.cacheTable` looks like a second and is **not** one, which is worth stating because a
text-keyed census says otherwise: its element `atomic.Pointer<T>` carries exactly one fixed-array
field and that field is Go's **blank identifier** (`internal array<ж<T>> _ = new(0)`, the
"mention `*T` to disallow conversion" trick). A `_` field is unreadable in Go, so its zero value can
never be observed, and go2cs-gen's predicate skips it on both its syntax and its symbol path.
Declining to construct there is correct, not a gap — and the value would have been identical anyway,
since the initializer is `new(0)`.

So in the standard library the stamp has no site and the generator's half reaches one; the rule serves
end-user Go converted through `-recurse`, and the next such type.

(Guarded by the `NamedArrayZeroValue` behavioral test: both shapes × both spellings (`var`, `new(T)`)
× the seven site kinds, the write arm, and the two controls that pin the boundary from the other side
— a plain element and a NAMED element, both already correct and both required to stay on the bare
`new array<E>(N)` backing. The sibling `CompositeLiteralElements` guards the same element predicate
reached through a composite LITERAL; this one guards the zero value, which is where every one of the
standard library's needy named arrays is actually built.)

## A NAMED array compared with the unnamed array it is written over casts the unnamed side

Go lets a value of a named array type be compared with a value of the unnamed array type it is
declared over, because the unnamed operand is assignable to the named type. In C# the wrapper
(`type Hash [4]byte` lowers to a go2cs-gen struct over `array<byte>`) converts IMPLICITLY in both
directions, so `Hash == Hash` and `array<byte> == array<byte>` both apply and the comparison is
ambiguous (CS0034, x/mod's sumdb/tlog `h != sha256.Sum256(nil)`). The emission casts the unnamed
operand to the named type, which is the comparison Go makes, in either operand order and for `==`
and `!=` alike:

```go
var h Hash                       // type Hash [4]byte
if h != sum(nil) { … }           // func sum([]byte) [4]byte
```

<!-- illustration: not converter output -->
```csharp
if (h != ((Hash)(sum(default!)))) { … }
```

Only a type written DIRECTLY over an unnamed array takes the cast (`writtenRHSIsUnnamedArray`): a
type written over another named array converts to that type instead, and an unknown declaration
keeps the bare emission. Not covered: a public wrapper over a non-public element type, whose
conversion operators the generator omits (see `OmitUnderlyingConversionOperators`), so neither the
bare comparison nor the cast compiles there; no instance is known. Guarded by the
`NamedArrayVsUnnamedCompare` behavioral test.

## `make([]E, n)` constructs its ELEMENTS by the same rule

`slice<T>`'s length constructor fills its backing with `default(T)` exactly as `array<T>`'s does, so
`make([][hashSize]int, n)` as a bare `new slice<array<nint>>(n)` would produce n **zero-length** arrays,
because the inner length lives only in the Go type (hash/maphash's `avalancheTest1` indexes them at once;
`image/draw`'s Floyd-Steinberg `quantErrorCurr`/`Next` rows and `x/text/transform`'s chain buffers have
the same shape).

`make` threads the **same** `arrayElemFactory` the fixed-array zero-value sites use into a golib
`slice(nint length, Func<T> elementFactory, nint capacity = -1, nint low = 0)` constructor — one
rule, one predicate, both containers:

```go
grid := make([][4]int32, 3)
capped := make([][2]int, 1, 3)
```
<!-- source: src/tests/Behavioral/NestedFixedArrays/main.cs.target:99, 105 -->
```csharp
var grid = GoReflect.WithElemDims(new slice<array<int32>>(3, () => new(4)), 4);
…
var capped = GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(2), 3), 2);
```

The result records its element dimensions through `GoReflect.WithElemDims`, as a slice-of-arrays
literal does (above).

The factory fills the **whole backing**, not just the first `length` elements: Go zeroes the entire
allocation, so the capacity beyond the length is already valid storage once a re-slice or `append`
exposes it. As with `array<T>`, a NAMED array element (`type row [4]byte`) and every element whose
`default(T)` is a valid zero value keep the plain length constructor. The composite-literal path
(`[][4]int{{…}}`) takes the same factory
([An ELIDED element carries the same construction](#an-elided-element-carries-the-same-construction-as-the-spelling-it-elides)).
(Guarded by `NestedFixedArrays`, extended with the
`make` length and length+capacity forms, a struct element needing construction, and a named-array
element control, output-compared vs `go run`.)

**A DEFINED slice type routes the factory through its underlying `slice<E>`.** <!-- 2026-07-27 --> `make`'s
target is not always `slice<E>`: for `type SortedMap []KeyValue` the target is the go2cs-gen wrapper,
which declares `SortedMap(nint length, nint capacity = -1, nint low = 0)` and **no element-factory
overload**, so a lambda passed to it would bind to `nint` (`CS1660: Cannot convert lambda expression to
type 'nint'`, at `internal/fmtsort`'s `make(SortedMap, 0, n)`). The factory-filled backing is therefore built as the
underlying `slice<E>` — bit-for-bit the value the unnamed form produces — and handed to the wrapper's
`T(slice<E> value)` constructor, which the generator always emits:

```go
sorted := make(SortedMap, 0, n)           // internal/fmtsort sort.go
```
<!-- source: src/core/internal/fmtsort/sort.cs:57 -->
```csharp
var sorted = new SortedMap(new slice<KeyValue>(0, () => new(), n));
```

<!-- history: this was a live defect: -tests regenerates production .cs on every run, so a broken sort.cs left
by one package's run broke every later package downstream of fmt (the sweep read 41 pass / 20 fail at the
time). It was the fourth zero-value-construction instance in a fourth emission path. -->

## `clear` rebuilds each element through golib's `GoZero` — the RUN-TIME half of zero-value construction

`clear` is the run-time half of the same rule. Assigning `default!` to every element would replace each
element of a `[][4]int32` with a **length-zero** array, where Go's `clear` leaves four zeroed `int32`s
(`image/draw`'s Floyd–Steinberg dither calls `clear(quantErrorNext)` once per scan line and then indexes
the next row). The same holds one level in for a struct element carrying a fixed-array field: `default!`
skips the generated parameterless constructor that runs its field initializers.

The converter cannot help here — a `clear` call site has no Go type shape to thread a factory
through, and the C# type `array<E>` is the same for every N — so the shape recovery lives in golib, as
a single entry point:

<!-- illustration: not converter output -->
```csharp
public static T GoZero<T>(T template)   // builtin.cs
```

`GoZero` returns the Go zero value of `T`, consulting `template` **only for run-time shape**, and
resolves one of three answers from a per-closed-`T` static (`ZeroFacts<T>`, the same JIT-folding
shape as `AssertFacts<T>`), so the overwhelmingly common case compiles to a constant `default`:

| `T` | zero |
|:--|:--|
| any reference type, or a value type golib owns (`@string`, `slice<T>`, `map<K,V>`, the numerics) | `default` |
| `array<E>` — implements the golib marker `IGoZeroShaped` | `GoZeroLike()`: a new array of the template's LENGTH, elements zeroed recursively so `[2][3]int32` keeps its inner lengths |
| a converted Go struct (a `partial struct` + a generated parameterless constructor) | that constructor — exactly what the converter emits for `var x T`, and `default` for a plain struct |

All three slice-shaped `clear` overloads (`slice<T>`, `Span<T>`, and the constrained `ISlice<T>`)
route through one `Span<T>` body that keeps the vectorized `Span.Clear()` whenever
`default(T)` is already the Go zero value:

```go
q := make([][4]int32, 3)
q[1][2] = 7
clear(q)
len(q[1]) // 4, not 0
```

The converted-struct-plus-constructor rule is deliberately broad rather than a per-shape enumeration
(fixed-array field, promoted embed, …): calling a converted struct's own zero-value constructor is
always correct, so a FUTURE field shape that needs construction is covered without a new rule. That
generality is the whole point — this is the run-time counterpart of the
converter's `arrayZeroValueArgs` and go2cs-gen's `AppendZeroValueInitializers`, which build a zero
value where the shape is known statically; `GoZero` recovers it from a value that already carries
it, which is what a built-in is handed. (Guarded by the `ClearBuiltinShadow` behavioral test,
extended with `clear` over an array-element slice, a struct-with-array-field slice, and an
array-of-arrays element, each written to after the clear and output-compared vs `go run`.)

## A map READ of a shape-carrying element supplies the zero from the CALL SITE

A map read is the last zero-value path. Go's read of an ABSENT key — every read of a nil map
included — yields the element type's zero value, and for `[N]T` that zero is N zeroed elements. A
plain indexer answers `default(TValue)`, and `default(array<T>)` has **length zero**, so the first
index into a missed entry would panic `index out of range [0] with length 0` where Go reads a zero:

```go
// html/escape.go — entity2 is map[string][2]rune
if x := entity2[string(entityName)]; x[0] != 0 {
```

That is not an edge path. A miss is the NORMAL outcome for any `&…` run that is not a two-rune
entity, so `html`'s `TestUnescape` would die on ordinary input.

The shape cannot come from the map. It is a property of the Go map TYPE, and neither
`map<TKey, TValue>` nor a `default` (nil) one carries it — reading it off an existing entry would
answer only for a POPULATED map and guess for an empty or nil one. The READ SITE always knows it
statically, so the same ladder every declaration site uses (`zeroValueInitializer` /
`arrayZeroValueArgs`) is threaded into a golib indexer overload that invokes the factory **only on a
miss**; the emitted lambda is non-capturing, so it is cached and a HIT costs nothing:

```go
x := entity2["notthere"]              // len 2, both runes 0
n := nested["zzz"]                    // map[string][2][3]int — inner lengths survive too
z, ok := entity2["alsomissing"]       // comma-ok form
v := nilMap[7]                        // quadMap is map[int][4]byte, nil — len 4
```
<!-- source: src/tests/Behavioral/MapArrayValueZero/MapArrayValueZero.cs.target:12, 18, 28, 39 -->
```csharp
var x = entity2["notthere", () => new array<rune>(2)].Clone();
…
var (z, ok) = entity2["alsomissing", () => new array<rune>(2), ꟷ];
…
var n = nested["zzz", () => new array<array<nint>>(2, () => new(3))].Clone();
…
var v = nilMap[7, () => new array<byte>(4)].Clone();
```

All three map surfaces answer it, so this is one rule rather than three: `map<TKey, TValue>` declares
the two overloads, go2cs-gen's `IMapTypeTemplate` forwards them for a NAMED map type, and
`IMap<TKey, TValue>` carries them as default members for a map-cored type parameter. Two exclusions
keep the emission unchanged where it is already right — a NAMED array element (its wrapper allocates
its backing lazily from its own known size, the same exclusion `arrayElemFactory` documents), and an
assignment TARGET, which carries a value and needs no zero. In the converted standard library the one
such read is the `entity2` read in `html/escape.cs`.<!-- measured at the
landing: one line, the read now at html/escape.cs:160 (`entity2[tmpstring(entityName), () => new array<rune>(2)]`) -->

(Guarded by the `MapArrayValueZero` behavioral test: plain and comma-ok reads, hit and miss, a
nested element, a named map type read both nil and empty, an unnamed map read both nil and empty,
and a store-then-read control, all output-compared vs `go run`.) <!-- failing-first: transpiled with the
pre-fix converter the same program panicked with index out of range [0] with length 0, the html signature. -->

## A named slice wrapper's non-generic `ISlice.Append` is an EXPLICIT implementation

The generated wrapper for `type S []E` implements both halves of the golib slice surface, and both
declare an `Append`: the typed `ISlice<E>.Append(E[])` and the non-generic `ISlice.Append(object[])`.
Both **public** would be fine for every `E` except one — with `E = any`, `object[]` and `E[]` are the
SAME parameter list, so the wrapper would carry two public methods differing only in return type:
**CS0111**. A `[]any` named slice is a table-driven-test idiom (`fmt`'s `type SE []any`, `archive/tar`'s
`type fileOps []any`), which is why production code rarely meets it. <!-- history: ISliceTypeTemplate once
emitted both public; the CS0111 held fmt's (63 verdicts) and archive/tar's (97) whole test suites. -->

The non-generic overload is explicit —

<!-- source: src/gen/go2cs-gen/Templates/InheritedType/ISliceTypeTemplate.cs:88 -->
```csharp
ISlice? ISlice.Append(object[] elems) => ((ISlice)m_value).Append(elems);
```

— which is what golib's own `slice<T>` has always declared (`ISlice ISlice.Append(object[] elems)`
beside `ISlice<T> ISlice<T>.Append(params T[] elems)`), so the wrapper matches the type it wraps. The reasoning is the
same one the template already applies to `GetEnumerator` in the
subsection above: this is the boxing, interface-typed path, taken only when a consumer asks for the
interface, and the public surface is the typed overload. Converted code never calls it by name —
Go's `append` emits golib's `append` builtin, which reaches `slice<T>.Append` statically.

<!-- measured after the change: both suites cleared this root and stopped on unrelated ones (fmt on five:
CS1955, CS0030 on renamed complex types, CS1729/CS0103/CS0034 around Scan_type; archive/tar on a duplicate
global using alias). -->
Guarded by the `NamedAnySliceType` behavioral test —
both suites' declarations verbatim, spread into a variadic `...any`, appended to, indexed, ranged,
sub-sliced, spread into a second named `[]any`, and compared against `nil`, output-compared vs
`go run`.

## Composite types render structurally (`[]*T` keeps the pointer)
A slice/array type is rendered structurally in every type-name path: the `[N]`/`[]` marker plus the recursively resolved
element, never from the `go/types` string form. The string form is path-qualified (`[]*internal/abi.Type`), and the
cross-package last-segment strip would eat everything before the slash *including the pointer marker*, silently dropping
the `ж<>` (reflect's `[]*abi.Type` fields would compile against the WRONG element type). The recursion also resolves
lifted anonymous elements and cross-package generic elements:
```go
ptrs := vals.([]*atomic.Int32)
```
<!-- source: src/tests/Behavioral/ArrayOfCrossPackageType/main.cs.target:18 -->
```csharp
var ptrs = vals._<slice<ж<atomic.Int32>>>();
```
Guarded by `ArrayOfCrossPackageType` (the type assert and a `var` declaration).

A **SAME-PACKAGE instantiated generic** is rendered structurally for the same reason — the name plus each type argument
recursively resolved, never from the `go/types` string. A *cross-package* generic takes the structural path too
(`getAliasQualifiedTypeName`/`getFullyQualifiedTypeName` both special-case `pkg != v.pkg`). A generic whose OWN type is
local while a type ARGUMENT is cross-package must not use the `t.String()` form either: in `curve[*repro/sub.Item]` the
slash-strip would eat everything before the `/` — **including the `curve[` header** — collapsing the wrapper
(crypto/elliptic's `var p224 = &nistCurve[*nistec.P224Point]{…}` and its `p256`, `p384` and `p521` siblings are the
standard-library cases). Both `getAliasQualifiedTypeName` (the var-type path) and `getFullyQualifiedTypeName` (the
struct-embed field path) render a same-package generic as `Name[args…]` with each arg via the same function, so the
arguments carry their short, slash-free package-qualified names and the header survives:
`*Holder[*CrossPkgLib.Sensor]` → `ж<Holder<ж<CrossPkgLib.Sensor>>>`. (In crypto/elliptic the type parameter is
constrained by the generic interface `nistPoint[Point]`, so the argument is its adapter and the var is
`ж<nistCurve<P224PointжnistPoint>>`.) <!-- history: at Go 1.24 crypto/elliptic also embedded the instance,
`p256Curve struct { nistCurve[*nistec.P256Point] }`; from Go 1.25 it is `var p256 = &nistCurve[*nistec.P256Point]{…}`.
Before this rule crypto/elliptic emitted `ж<nistec.P224Point>>` / `ref go.nistec.P256Point> …`, a CS1519/CS1526 cascade
of about 137 errors across elliptic/ecdh/mlkem768 (Go 1.24's crypto/internal/mlkem768, at Go 1.26
crypto/internal/fips140/mlkem); the rule was byte-identical across the behavioral corpus. --> (Guarded by the
`CrossPkgUser` extension — a same-package `Holder[*CrossPkgLib.Sensor]` as a var type AND a struct embed, field
read/write vs Go.)

## Appending to an interface-typed slice casts the element
A value appended to a `[]Iface` slice whose type is not already the interface -- a pointer rendering as the `*T`-to-interface adapter ctor, or a raw struct value -- leaves both golib `append` overloads applicable (`append<T>(ISlice, params T[])` infers the concrete/adapter type; `append<T>(slice<T>, params Span<T>)` infers the interface -- CS0121). The converter casts such elements to the element interface type:
<!-- source: src/tests/Behavioral/InterfaceCasting/InterfaceCasting.cs.target:161-162 -->
```csharp
    pack = append(packᴷ, pack, (Animal)(new CatжAnimal(Ꮡ(new Cat()))));
    pack = append(pack, (Animal)(new Dog()));
```
(`packᴷ` is the variable's Go stack buffer, which its first append passes —
see
[A non-escaping append can take Go's 32-byte stack buffer](#a-non-escaping-append-can-take-gos-32-byte-stack-buffer).)
An already-interface-typed element stays bare. The **empty interface** (`any`) element type is affected identically and takes the same cast — `append(args[:len(args):len(args)], c.output)` with `args []any` and `c.output []byte` infers `T=[]byte` on the `ISlice` overload but `T=any` on the `slice<T>` overload (testing's `flushToParent`, CS0121), and appending a scalar (`append(anys, 5)`) is the same shape; the differing element is cast to `any` so both overloads agree, as in the `AppendUntypedConst` golden:
<!-- source: src/tests/Behavioral/AppendUntypedConst/main.cs.target:36 -->
```csharp
anys = append(anys.slice(0, len(anys), len(anys)), (any)(data));
```
Guarded by `InterfaceCasting` (non-empty interface) and `AppendUntypedConst` (the empty-interface `[]byte`-into-`[]any` and scalar-into-`[]any` cases).

## A `nil` element appended to a slice casts to the element type
A bare `nil` appended as a single element -- `append(b.lines, nil)` on `[][]cell` -- renders `nil` as `default!`, which
C# overload resolution would bind to `append`'s `params` parameter as the **whole null array** (a non-expanded params
call), appending **ZERO** elements rather than one nil element. That would silently no-op the grow: text/tabwriter's
`addLine` would never extend `b.lines`, and every `terminateCell` indexing `b.lines[len(b.lines)-1]` would panic with
index `[-1]`. The converter therefore casts any untyped-nil element to the slice's element type, forcing single-element
binding — the interface (`error`) and named-composite branches above for their own reasons, and an **unnamed** nillable
element type (slice/map/pointer/chan/func) for this one:
<!-- source: src/tests/Behavioral/AppendNilSliceElement/main.cs.target:14, 19, 23 -->
```csharp
    lines = append(lines, (slice<nint>)(default));
    …
    maps = append(mapsᴷ, maps, (map<@string, nint>)(default));
    …
    ptrs = append(ptrsᴷ, ptrs, (ж<nint>)(nil));
```
(The pointer element's `nil` renders in pointer context. `mapsᴷ` and `ptrsᴷ` are the variables' Go stack
buffers, passed by a variable's first append that may take one —
see
[A non-escaping append can take Go's 32-byte stack buffer](#a-non-escaping-append-can-take-gos-32-byte-stack-buffer).)
A nil element is only ever valid when the element type is nillable, so the cast target always exists. Spread appends (`append(dst, src...)`) are excluded (the existing `Ellipsis.IsValid()` guard). Guarded by the `AppendNilSliceElement` behavioral test (slice/map/pointer element types, output-compared vs Go).

---

[← Multi-Result Values and Comma-Ok Forms](multi-result-and-comma-ok.md) · [Index](README.md) · [Strings (`@string` and `sstring`) →](strings.md)
<!-- {% endraw %} -->
