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

```csharp
internal static void Main() {
    var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
    var nums = new nint[]{10, 20, 30}.slice();
    var buf = new slice<byte>(4);
    fmt.Println(primes[0], nums[2], len(buf));
}
```

A **named** slice/array type (`type d [3]rune`, `type s []int`) lowers to a struct wrapping `array<T>`/`slice<T>`; its composite literal cannot use C# collection-initializer braces (the lowered struct has no `Add`), so it is constructed through the underlying-collection constructor: `d{0, 32, 0}` → `new d(new rune[]{0, 32, 0}.array())`.

The **empty** composite of such a type is its **zero value**, not a one-element literal: the generic named-composite `nil` filler (which gives a named *struct* composite its `new T(nil)` zero-value ctor argument) previously landed *inside* the element literal — `tmpBuf{}` (`type tmpBuf [32]byte`, runtime `string.go`'s `*buf = tmpBuf{}`) emitted `new tmpBuf(new byte[]{nil}.array())`, a `NilType` element in a `byte[]` (CS0029). An empty **array** composite now emits a zeroed *fixed-length* backing — `new tmpBuf(new byte[32].array())` — because Go's `[N]T{}` is a full-length zero array (an empty `{}` literal would produce a length-0 backing); an empty named-**slice** composite emits an empty non-nil backing — `pm{}` → `new pm(new uint32[]{}.slice())`. (Guarded by the `NamedArrayWrapper` extension — empty array composite read/written at full length, zeroing an existing wrapper through a pointer via `*buf = tb{}`, and an empty slice composite appended to; values vs Go. The nil-vs-empty *distinction* — `pm{} == nil` is `false` in Go — is a separate pre-existing golib model latent: the slice nil-compare conflates nil with empty-but-allocated.)

**Reslicing SHARES the backing array — golib `slice<T>` stores capacity.** A Go reslice is a view adjustment, never a copy: writes through `s[a:b]` are visible through `s` (and vice versa), `append` within capacity writes the shared backing in place, and only `append` beyond capacity reallocates and detaches. The emitted forms are the C# range indexer for 2-index expressions (`s[a:b]` → `s[a..b]`, `s[a:]` → `s[a..]`, `s[:b]` → `s[..b]`, `s[:]` → `s[..]`) and the golib `.slice(low, high, max)` extension for 3-index expressions (`s[a:b:c]` → `s.slice(a, b, c)`; a missing low is the `-1` sentinel). Both take bounds **relative to the view** (the source slice may itself sit at a non-zero offset in its backing array), default a missing high to `len(s)` — the range indexer resolves a from-end `Index` against the slice *length*, so `s[1..]` is Go's `s[1:]` even when `len < cap` — allow high up to `cap(s)`, and panic Go-style (`RuntimeErrorPanic.SliceBoundsOutOfRange`) when out of range. To represent a capacity-restricted 3-index view (`s[a:b:c]`, `c` below the backing array's end) without copying, `slice<T>` stores `m_capacity` as its own field rather than deriving it from the backing array's length; the `slice<T>(T[] array, nint low, nint high, nint max)` constructor builds such views. (Historically golib *copied* on any reslice that didn't span the whole backing array — a `base[2:5]` sub-slice was a detached array with lost aliasing — derived capacity from the backing end, and mis-measured `Available`/`Append` for non-zero-offset views. Guarded by the `SliceAliasing` behavioral test — `copy` into and element writes through a `low>0` reslice, reslice-of-reslice offset compounding, slice-of-array reslices, restricted-capacity 3-index writes, in-place vs reallocating `append`, all read back through the base and value-compared vs Go.)

**A `make` length/capacity/size-hint of a non-`int` integer type is cast to `nint`.** The golib allocating constructors all take `nint` — `slice<T>(nint length, nint capacity)`, `map<K,V>(nint capacity)`, `channel<T>(nint capacity)` — and C# does **not** implicitly convert a `uintptr`/`uint`/`uint32`/`uint64`/`int64` (C# `nuint`/`uint`/`ulong`/`long`) to `nint`. So `make([]byte, n/goarch.PtrSize)` with a `uintptr` length would leave `new slice<byte>(n / …)` with no applicable constructor, and overload resolution falls onto `slice<T>(T[])` — reported as `CS1503` ("cannot convert `nuint` to `byte[]`"); a map/chan with a `uintptr` hint is a direct `nuint`→`nint` CS1503. The converter casts each such length/capacity/hint argument to `nint`: `new slice<byte>((nint)(n / goarch.PtrSize))` (both args of `make([]byte, l, c)`), `new map<K,V>((nint)(hint))`, `new channel<T>((nint)(size))`. A plain `int` (`nint`) and an untyped-constant argument (`make([]byte, 4)` → a bare `4`) bind directly and are **left uncast** (no golden churn) — as are the widening `int8`/`int16`/`uint8`/`uint16` kinds. (Guarded by the `MakeSliceUintptrLen` behavioral test — `uintptr`/`uint`/`uint32`/`uint64` slice lengths, a `uintptr` len+cap, a `uintptr` map hint and chan size, and int/untyped controls, all `len`/`cap`/element values verified vs Go; runtime hits this in `mbitmap`'s `make([]byte, n/goarch.PtrSize)`.)

**Slicing a pointer-to-array.** Go lets a `*[N]T` be sliced directly — `p[lo:hi:max]`, `p[:]` — auto-dereferencing the array. The C# box `ж<array<T>>` has no slice/range members (its underlying `array<T>` does), so the converter dereferences first: `p[1:3:4]` → `(~p).slice(1, 3, 4)`, `p[:]` → `(~p)[..]`, `p[2:]` → `(~p)[2..]`. Without the deref the call binds to the box and fails (CS1929). The resulting slice shares the array's backing storage, matching Go. (The `(*[N]T)(ptr)[:n]` pointer-*cast* form is different — see *Pointer-cast slice* below.) A deref-aliased pointer **parameter** or **receiver** is the exception: it is emitted as the pointed-to *value*, not a box, so a `~` on it would deref a non-pointer (CS0023). When that value is a **named** array type — `b *pageBits` emitted `ref pageBits b`, where `pageBits` is `[N]uint64` — the wrapper has no slice/range members, so its underlying `array<T>` is reached via `.Value`: `b[:2]` → `b.Value[..2]`. When it is an **anonymous** array (`p *[N]T` → `ref array<T> p`) the value already *is* the `array<T>` and is sliced directly (`p[:]` → `p[..]`). Only a pointer-to-array **box** (a local, a field, a call result) gets the `~` deref. (Guarded by the `PointerArraySlice` behavioral test — local box, named-array receiver, and named-array parameter; runtime hits this in `select.go`'s `cas1[:ncases:ncases]` / `mprof.go`'s `stk[:n:n]` (locals) and `mpallocbits.go`'s `pageBits` receiver methods (`clear(b[:])`).)

An **untyped (type-inferred) composite literal** — the inner `{…}` of a `[][]rank{ key: {…} }`, which has no explicit type node — is emitted as a target-typed `new(…)` when its inferred type is a struct (the struct constructor takes the field values). When the inferred type is a **slice or array**, that form is wrong (`slice<rank>`/`array<rank>` have no element-list constructor → CS1729); the converter emits the element-array projection instead — `{rA, rB}` (inferred `[]rank`) → `new rank[]{rA, rB}.slice()`, and an inferred `[2]int` → `new nint[]{…}.array()`. When the inferred type is a **pointer-to-struct** — the `[]*T{ {…} }` shorthand for `&T{…}` — it is emitted as the boxed struct constructor `Ꮡ(new T(field: val, …))` (a bare `new(…)` would target the box `ж<T>`, whose constructor lacks the struct's fields → CS1739). When such an untyped slice/array literal is **keyed** (`{joiningL: stateBefore, …}` — the inner `{…}` of x/net/idna's `joinStates = [][numJoinTypes]joinState{stateStart: {…}, …}`), the element-array projection above cannot take Go's `key: value` syntax — `new joinState[]{ joiningL: stateBefore }` is a C# array initializer, which has no keyed element form (CS1003 ×62). The keyed case is routed to a golib `golib.SparseArray<T>` collection initializer instead — `new golib.SparseArray<joinState>{ [joiningL] = stateBefore, … }.array()` (`.slice()` for a slice element) — the same form the *typed* keyed slice/array path emits (see below); the `.array()`/`.slice()` `IEnumerable<T>` extension materializes the dense backing, and a defined-integer key takes the `[(int)key]` cast exactly as in the typed path. (Guarded by `UntypedNestedSliceComposite`; runtime/lockrank.go's `lockPartialOrder` is a `[][]lockRank` and runtime1.go's `dbgvars` is a `[]*dbgVar` of the positional forms, and x/net/idna's `joinStates` is the keyed form.)

An **indexed (keyed) slice/array literal** — `[]string{lockRankSysmon: "sysmon", …}` — is emitted as a golib `golib.SparseArray<T>` collection initializer (`[index] = value`). Its indexer takes a Go `int`. When an index key's Go type is a **defined integer type** whose underlying type does not implicitly widen to C# `int` (i.e. `int`/`int64`/`uint`/`uint32`/`uint64`/`uintptr`, as opposed to `int8`/`uint8`/`int16`/`uint16`/`int32`), the key is cast to `int` so it satisfies the indexer (CS1503 otherwise): `[lockRankSysmon]` (a `type lockRank int`) → `[(int)lockRankSysmon]`. A key that already widens (e.g. a `uint8`-backed `Kind`) is left uncast.

A keyed slice/array literal whose element type is a **non-empty INTERFACE** routes its elements
through the interface-cast element loop (each value wraps via `convertToInterfaceType`) instead of
`convExprList` — and that loop rendered a `KeyValueExpr` as a FLAT `key, value` pair, feeding the
SparseArray collection initializer one item at a time (`Add(key)` then `Add(value)`:
CS1950 + CS1503 ×21 pairs on go/internal/gccgoimporter's `lookupBuiltinType`,
`[...]types.Type{gccgoBuiltinINT8: types.Typ[types.Int8], …}` — untyped named-const keys in a
function-body literal, immediately indexed). The loop now emits the same `[key] = wrappedValue`
indexer form the non-interface path produces, with the key routed through `sparseArrayKey` (so a
defined-integer-type key keeps its `[(int)key]` cast alongside the interface-wrapped value); a
keyed MAP literal with an interface element type reaching the same loop takes the identical
indexer form. (Guarded by the `SparseArrayIfaceElem` behavioral test — a function-body sparse
literal with untyped named-const keys immediately indexed, the package-level form, and a
named-`uint`-keyed form, elements read back and output-compared vs Go.)

A keyed (sparse, constant-index) literal of a **named array-wrapper** type — internal/trace/oldtrace's `timedEventArgs{1: uint64(ev.StkID)}` where `type timedEventArgs [4]uint64` — backs onto the golib `array<T>(length)` (which has an indexer setter), not a raw C# array. The wrapper's constructor takes an `array<T>` (the positional path already produces one via `.array()`), and the keyed elements render as the `[i] = v` indexed initializer — valid on `new array<uint64>(4){[1] = v}` but *not* on `new uint64[]{[1] = v}` (CS0131, an array-initializer takes no indexed members). A **positional** literal of the same wrapper keeps the `new uint64[]{…}.array()` form (unchanged — no churn). (Guarded by `NamedArrayKeyedLiteral` — a `type args [4]uint64` with multi-keyed, single-keyed, and positional literals, element reads output-compared vs Go.)

A **generic** named array type carries its type parameters (and their constraints) onto the forward declaration, and its element type is emitted fully qualified in the `[GoType]` attribute so the generated array-backed partial — which lives in a file without this file's package-relative `using` aliases — can resolve it:

```go
type table[T any] [3]atomic.Pointer[T]
```
```csharp
[GoType("[3]sync.atomic_package.Pointer<T>")] partial struct table<T>
    where T : new();
```

An **anonymous array/slice field whose element type lives in a multi-segment-path package** — `cpuLogWrite [2]atomic.Pointer[profBuf]`, `children [4]atomic.UnsafePointer` (atomic = `internal/runtime/atomic`) — keeps its `array<…>` wrapper. The field's type name is built structurally from the `[N]`/`[]` marker plus the recursively resolved element, *not* from the type's package-qualified string: that string (`[2]internal/runtime/atomic.Pointer[…]`) goes through a cross-package last-segment strip that would also remove the leading `[2]`, collapsing the field to the bare element type (`atomic.Pointer<…> = new(2)`) whose array `new(2)` initializer then mis-binds the element constructor (CS1503). With the structural rendering the field stays `array<atomic.Pointer<profBuf>> = new(2)`. An array of a current-package or basic-typed element was unaffected (its string has no foreign path to strip). (Guarded by the `ArrayOfCrossPackageType` behavioral test — `[3]atomic.Int32` / `[2]atomic.Uint64` fields; runtime's `trace`/`traceMap` structs hold these.)

<a id="a-structs-array-fields-get-their-fixed-length-from-a-generated-parameterless-constructor"></a>Moved to [A struct's array fields get their fixed length from a generated parameterless constructor](nil-and-zero-values.md#a-structs-array-fields-get-their-fixed-length-from-a-generated-parameterless-constructor).

<a id="the-zero-value-ladder-is-one-ladder-and-a-named-result-climbs-it-too"></a>Moved to [The zero-value ladder is one ladder, and a NAMED RESULT climbs it too](nil-and-zero-values.md#the-zero-value-ladder-is-one-ladder-and-a-named-result-climbs-it-too).

#### A slice-bounds fault is a PANIC, not an ArgumentException

The same investigation named why this defect was so expensive to find. `slice<T>`'s two windowing
constructors threw a plain `ArgumentException`/`ArgumentOutOfRangeException` for an out-of-bounds
window, and `array<T>`'s slice→array conversion threw `IndexOutOfRangeException`. Neither is a
`PanicException`, so neither is visible to `recover()` and — the expensive part — both satisfy
`Goroutine.CanContain`, which means a converted test host *contains* them and records them on the
`TestExecution` instead of failing. When the dying goroutine is the one another goroutine is waiting
on, the record never flushes and the package deadline burns with no output at all. 17 of
`crypto/tls`'s 53 measured divergences presented that way — 10 as an infrastructure-error line, 7 as
a silent hang — where Go would have failed loudly in milliseconds.

All six throws now raise `RuntimeErrorPanic.SliceBoundsOutOfRange(low, high, max, capacity)`, whose
message shapes already mirror the Go runtime's, and the conversion-length check raises the new
`RuntimeErrorPanic.ArrayConversionLength` (same text as before — it was already Go's — now as a
recoverable panic). The netip case reproduces Go's message exactly: `a16[:8]` on a length-0 array
prints `runtime error: slice bounds out of range [:8] with capacity 0`.

### Prefer the file-local package alias over the fully-qualified `_package` name

A cross-package named type has two C# spellings: the **fully-qualified** form `sync.atomic_package.Int32` (the namespace-rooted class, from `getFullyQualifiedTypeName`) and the **file-local alias** form `atomic.Int32` (the `using atomic = sync.atomic_package;` shorthand, from `getAliasQualifiedTypeName`). For *visual* fidelity — the converted C# should read like the Go original, which writes `atomic.Int32` — body emission prefers the alias. But the alias is only resolvable where the `using` is in scope, so the choice is made per emission site by `getScopeCheckedTypeName`, which returns the alias form **only when every cross-package type referenced by the type is imported in the current file** (checked against the per-file `importQueue`), and otherwise falls back to the fully-qualified form.

The fallback matters: a Go file may *index* an atomic-typed array field of a struct — `&x.c[i]` → `…at<E>(i)` — without ever naming the element type `E`, so it carries no `import "sync/atomic"` and no `using atomic`. There the element type must stay fully-qualified (it resolves inside `namespace go;` with no alias) or the file fails CS0246. When the package *is* imported (the common case, and every behavioral test of this), the prettier alias is used.

`getScopeCheckedTypeName` is applied at the body-emission sites that land in the current source file:

* **named struct-field declarations** — `internal atomic.Int64 total;` (was `sync.atomic_package.Int64`);
* **heap-box allocations** — `ref var n = ref heap(new atomic.Int32(), out var Ꮡn);`;
* **element-address `at<T>`** — `…at<atomic.Int32>(0)`.

It is **not** used for forms consumed by the source generators in alias-less generated files, which must stay fully-qualified: the `[GoType("…")]` attribute string (e.g. `[GoType("sync.atomic_package.Uint32")]`, `[GoType("[3]sync.atomic_package.Pointer<T>")]`), the `global using` type-alias declarations, and the promoted-interface/embedded-field registration keys. (Embedded fields keep the full form for their promoted accessors; only the named-field branch uses the display name. Struct-embedding promotion across packages re-derives member types from the Roslyn semantic model, not from the field's emitted text, so aliasing the field declaration is safe.) Guarded by `ArrayOfCrossPackageType`, `AtomicValues`, `FuncTypeParam`, `GenericAtomicPointerField`, `GlobalAtomicDefer`, `GlobalAtomicFieldMethod`, and `StructPromotionWithInterface`/`StructPointerPromotionWithInterface`.

<a id="combined-field-element-address-baseatfield-i"></a>Moved to [Combined field-element address `base.at(field, i)`](pointers.md#combined-field-element-address-baseatfield-i).

## Array ASSIGNMENT copies the whole array (`.Clone()` on the RHS)

Go array assignment copies the array — `data := ints` yields independent storage — but the emitted
`array<T>` is a struct over a shared `T[]` backing store, so a plain C# struct copy **aliases**: a
write through the copy was visible through the source. The first operational hit was sort's
`TestReverseSortIntSlice` (`data := ints; data1 := ints` left ONE store sorted ascending then
descending, so the ascending/descending mirror check failed ×7 — misdiagnosed at first as an
embed-override dispatch defect; the dispatch was correct). The converter now appends golib's
strongly-typed `array<T>.Clone()` to an assignment RHS that copies an array **out of existing
storage** (see `cloneArrayValueCopy` in `visitAssignStmt.go`):

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
and stays bare. The shape/type gate is the shared `exprReadsArrayValueFromStorage`
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
or pointer-deref — `exprReadsArrayValueFromStorage` in `arrayCloneOperations.go`; a composite
literal or call result is freshly constructed and needs none):

```go
for _, row := range m { row[0] = 99 }  // → foreach (var (_, vᴛ1) in m) { var row = vᴛ1.Clone(); … }
for i, row = range m { … }             // pre-existing vars → row = vᴛ1.Clone(); inside the body
m := [2][3]int{a, b}                   // → new array<nint>[]{a.Clone(), b.Clone()}.array()
s1 := holder{arr: a}                   // keyed struct field  → new holder(arr: a.Clone())
mv := map[string][3]int{"x": a}        // map value           → ["x"u8] = a.Clone()
mk := map[[2]int]string{k: "kv"}       // map KEY             → [k.Clone()] = "kv"u8
lst := []any{b}                        // interface boxing    → new any[]{b.Clone()}.slice()
return h.arr                           // return              → return h.arr.Clone();
ch <- a                                // channel send        → ch.ᐸꟷ(a.Clone())
s = append(s, a)                       // append element      → s = append(s, a.Clone())
```

The range emission routes an array-valued key/value through the same iterate-a-temp mechanism a
reassigned range var uses (a C# foreach variable cannot be redeclared from itself); a map range KEY
of array type clones the same way. The RECEIVE side of a channel needs no twin — the send stored an
unaliased element and a buffered element is dequeued exactly once.

Three deeper repairs make the single `.Clone()` correct everywhere:

- **`array<T>.Clone()` is DEEP for nested arrays** (golib `array.cs`): Go's `[2][3]int` copy copies
  the inner arrays too, but the shallow `T[]` clone left every nested backing shared — so even the
  cloned sites under-copied at depth ≥ 2 (`m.Clone()` then `m[0][0] = 99` wrote the source's inner
  array). An element that is itself an array wrapper (anything implementing `IArray`) is re-cloned
  through its `ICloneable` surface, which now returns the properly-wrapped clone so the unbox
  recurses through any nesting depth; the `typeof` gate keeps flat element types on the single
  shallow copy.
- **NAMED array types get a strongly-typed `Clone()`** (`IArrayTypeTemplate` /
  `IArrayViewTypeTemplate`): the wrapper's only clone was the object-returning `ICloneable` form, so
  named-array copies could not be expressed. `public Row Clone() => new Row(Value.Clone());` (and
  the view-wrapper equivalent through its underlying wrapper) lets every site above — plus the
  function-parameter preamble, func-literal parameters, and array-typed VALUE RECEIVERS — clone
  named and alias-declared arrays exactly like direct ones (`typeIsArrayValue` tests the underlying
  type, widening the old direct-`*types.Array` preamble gate).
- **`array<T>` equality/hash is structural per-ELEMENT** (golib `array.cs`): Go arrays are
  comparable values, so a `map[[2]int]V` key must be found again by an equal array with different
  backing. `GetHashCode` hashed the backing reference (every structural-equal key missed), and the
  `Equals` overloads passed container-typed comparers (`EqualityComparer<T[]>`) where
  `IStructuralEquatable` calls them per boxed element — throwing on the first equal-length
  distinct-backing comparison. Element-typed comparers fix both and recurse through nested arrays.

**The suffix must be WRAPPED on a `~`-prefixed rendering.** A deref whose operand is a pointer CAST
(`*(*T)(p)`, `convStarExpr`'s casted-pointer-deref path) renders with the PREFIX `~` operator, and C#
postfix binds tighter than unary — so a naked `.Clone()` re-binds onto the cast's inner operand
instead of the dereferenced array. reflect's `InterfaceData` is the real-world case:

```go
return *(*[2]uintptr)(v.ptr)   // reflect/value.go
```
```csharp
return ~(ж<array<uintptr>>)(uintptr)(v.ptr).Clone();    // WRONG — .Clone() reads v.ptr (CS1061)
return (~(ж<array<uintptr>>)(uintptr)(v.ptr)).Clone();  // emitted — clone the dereferenced array
```

Every clone-append site therefore routes its rendering through `appendArrayValueClone`
(`arrayCloneOperations.go`), which wraps only when the rendering starts with the deref operator —
the same precedence guard `convStarExpr` already applies when IT appends the postfix `.Value` to a
cast/deref rendering. Every other shape `exprReadsArrayValueFromStorage` admits (ident, selector,
index, and the postfix `.Value` deref form) is already a C# primary expression, so the change is
byte-neutral wherever the suffix was correct — the corpus-wide A/B footprint was exactly this one
reflect line, whose CS1061 had blocked the whole converted stdlib through `fmt`→`reflect`.

(All guarded by the `ArrayValueCopySites` behavioral test — one output-compared section per site
class, including multidimensional deep-copy through range and parameter passing — plus
`ArrayCastDerefClone`, which guards the wrapped cast-deref form above. That guard is now
**output-compared**: its TYPED-pointer half (`*(*T)(p)` where `p` is already `*T`) RUNS once the
identity reinterpret stops routing through the raw-address `uintptr` bridge (see *A SAME-TYPE
reinterpret … collapses to the pointer itself*), so the clone's copy semantics are proven by VALUE
— mutating the returned array must leave the pointed-to original untouched, and the lvalue form
must write through. Its `unsafe.Pointer` half stays compile-shape only, with the results
deliberately discarded: reconstructing an array through an `unsafe.Pointer` round trip reads raw
memory and cannot reproduce Go's values under the managed model.)

**Known remaining gaps (documented, not yet emitted):** (1) golib-internal
element-wise transfers of nested-array elements (`copy(dst, src)`, spread `append(dst, src...)`)
copy element structs without re-cloning; (2) an array-typed map KEY at an index-STORE (`mk[k] = v`
stores `k` uncloned — only the composite-literal key form clones); (3) a named↔underlying array
CONVERSION (`[4]int(named)`) hands the wrapper's backing through the implicit operator uncloned;
(4) an EMBEDDED struct member is held as a `ж<T>` box, so a struct copy shares the embed outright
(`b := a; b.n = 99` writes through to `a.n`) — a defect of the embed model, wider than arrays and
untouched by the section below.

## `range` over an ARRAY VALUE iterates a COPY — the snapshot is the range EXPRESSION's `.Clone()`

Go evaluates a range expression **once** before the loop, so `for i, v := range a` over an array VALUE iterates a copy: a write to the container inside the body is invisible to every later iteration. The emitted `array<T>` (and the generated named-array wrapper) is a struct over a shared `T[]` backing, so the plain operand ALIASES the container — the emission read the writes back, diverging from `go run` on every such loop:

```go
a := [4]int{1, 2, 3, 4}
for i, v := range a {
    if i == 0 { a[1], a[2], a[3] = 91, 92, 93 }
    fmt.Println(i, v)                 // Go: 1 2 3 4        emitted (before): 1 91 92 93
}
```

The range expression is simply the array value-copy site nobody had emitted (see *Array VALUE-COPY at every transfer site* above — `range` was listed there for the iteration VARIABLE, never for the operand). It now takes a copy of its own:

```csharp
foreach (var (i, v) in a.ΔRangeSnapshot()) { … }        // array value  — snapshot, Go's copy
foreach (var (i, v) in h.arr.ΔRangeSnapshot()) { … }    // struct field — likewise a value
foreach (var (i, v) in r.ΔRangeSnapshot()) { … }        // named array  — the wrapper forwards it
```

**Why it is not the `.Clone()` every other transfer site takes, and this is the load-bearing part.**
Semantically it could be, and it was first. But a Go array copy lives INLINE — on the stack when the
destination is a local — so Go charges it **zero mallocs and zero `TotalAlloc`**, and a range snapshot
is the one array copy that provably cannot outlive its statement. `Clone()` mints a counted managed
array through `AllocationCounter`, which is right for a copy that DOES outlive the statement (an
assignment, a return, a field, a channel send) and wrong for one that cannot: golib's counter is
documented as the structural mirror of `runtime.MemStats.Mallocs`, so charging what Go does not
makes the mirror wrong by construction, and every `testing.AllocsPerRun` assertion around a range
over an array value would disagree with Go's own number. The byte meter is stricter still —
`runtime.ReadMemStats`'s `TotalAlloc` maps to `GC.GetTotalAllocatedBytes`, which no counter can hide
from — so the copy has to genuinely not allocate. `array<T>.ΔRangeSnapshot()` returns a `RangeSnapshot`
struct whose enumerator rents from `ArrayPool<T>.Shared` and returns the buffer in `Dispose`, which
C#'s `foreach` calls in a `finally`; steady state is zero managed allocations on both meters. Three
residuals are named rather than hidden: the first rent of a size class allocates once per process, an
array beyond the pool's largest bucket allocates per rent (as it would in Go, which also moves an
array that size off the stack), and an element type needing a DEEP copy still allocates per element,
because a nested `array<T>`'s backing is a heap object in this model.

**A SLICE element is never re-copied, and getting that wrong was a real crash.** `array<T>.Clone()`
re-clones elements that are themselves array wrappers, and `ISlice<T>` derives from `IArray<T>` — so a
named-slice element passed that test, while the generated wrapper's `Clone()` forwards to the
underlying `slice<T>`'s `ICloneable.Clone()`, which hands back a boxed `slice<T>`: the element cast is
then `slice<int>` → `ΔBits` and throws `InvalidCastException`. Latent until something first cloned an
array whose element is a slice; the range snapshot was that first caller, and math/big's
`bitsList` (`[...]Bits`, `type Bits []int`) is the corpus site — `TestFloatAdd` and `TestFloatMul` died
on it. Semantically the exclusion is required anyway: Go's array-of-slices copy copies HEADERS and
shares every backing store, which the shallow element copy already did. The `ArrayRangeSnapshot`
guard pins both halves — replacing a whole element through the original is invisible to the loop
(a fresh header), while a write THROUGH a shared backing is visible.

Scoped exactly as gc's own rule is (`cmd/compile/internal/walk/order.go`'s `rangeStmt`), so three shapes stay UNCOPIED because Go copies nothing there either — and each is a control in the guard:

* **No value iteration variable.** With at most one iteration variable and a constant length, Go does not evaluate the range expression at all, so `for i := range a` reads the LIVE array through the index. A blank value (`for i, _ := range a`) is the same case.
* **A POINTER to an array.** `for i, v := range p` shares the pointee; the emission keeps the bare `p.Value`.
* **A slice.** Its copy is the header, which the struct assignment already is.

`exprReadsValueNeedingClone` narrows the rest: only a read out of EXISTING storage (ident, selector, index, deref) can alias — a composite literal, call result, or conversion is freshly constructed and reachable by no other name, and a return already clones on its own way out.

With the operand snapshotted, `array<T>`'s enumerator reads LIVE storage and needs no capture point of its own, which is what finally let it shed the iterator method: `GetEnumerator()` returns the nested `array<T>.Enumerator` STRUCT, and go2cs-gen's `IArrayTypeTemplate` / `IArrayViewTypeTemplate` forward that struct (with the explicit `IEnumerable<(nint, T)>` member beside it) so named array types range as cheaply. Measured with `GC.GetAllocatedBytesForCurrentThread` over 1,000 loops: **72 B/loop → 0 B/loop** on the pattern path, and 103 → 79 B/loop on the boxing interface path, which now boxes a struct instead of driving a state machine. Snapshotting inside the enumerator instead would have been wrong in both directions — it would allocate on every loop AND copy for the two shapes above where Go shares.

Guarded two ways: `ArrayRangeSnapshot` (behavioral, output-compared against `go run`) mutates the container mid-loop across the array value, named-array, struct-field, nested-array, array-of-named-slices, `=`-form, mutable-range-var and aliased-element shapes, with the pointer, slice and index-only arms as controls that must NOT copy; `ArrayRangeAllocationTests` (`GolibTests`) asserts the enumerator's zero bytes with the boxing interface path as its nonzero control, the snapshot's zero on BOTH meters (object count and CLR bytes) with `Clone()` as the counted control that makes those zeros mean something, and the array-of-slices copy that must share its backing rather than throw.

## `for range` over a slice allocates NOTHING — `slice<T>.GetEnumerator()` returns a struct

`for i, v := range s` emits `foreach (var (i, v) in s)`, and Go's range over a slice allocates nothing at all. C# matches that only if the enumerator stays off the heap, which is entirely a question of what `GetEnumerator` **returns**: `foreach` binds `GetEnumerator` by **pattern** — the concrete return type, ahead of and independently of any interface — so a struct return is enumerated in place, while an interface return is a heap object per loop *entry*.

`slice<T>.GetEnumerator()` returned `IEnumerator<(nint, T)>` from an ITERATOR method (`yield return`), which is the worst of both: the compiler-generated state machine is one allocation and the inner `SliceEnumerator` class it drove is a second. Measured at **136 bytes per loop entry**, corpus-wide — every ranged loop in every converted package, paid whether the loop body allocated or not. It is invisible in output and in timings at small scale, and unmissable in a Go test that asserts an allocation count: `time.TestUnmarshalTextAllocations` runs `parseRFC3339`, whose `parseUint` closure ranges its argument once per field.

The return type is now the concrete nested `slice<T>.Enumerator` struct (the shape `List<T>.Enumerator` uses, and the one golib's own `sslice<T>` already had). Two contracts had to move with it:

* `slice<T>` reaches `IEnumerable<(nint, T)>` through `ISlice<T>` → `IArray<T>`, which the old public method satisfied implicitly. The interface member is now an **explicit** implementation returning the same struct boxed — so LINQ, an interface-typed local, and anything holding the slice as `IEnumerable<(nint, T)>` behave exactly as before, at exactly the cost they already paid. Only the pattern path is free.
* go2cs-gen's `ISliceTypeTemplate` (every `type S []E` named-slice wrapper) forwarded the interface. It now forwards `global::go.slice<E>.Enumerator` and carries the same explicit interface member, so a named slice type ranges as cheaply as the `slice<E>` it wraps — otherwise every `for range` over a named slice would have kept the box.

`array<T>.GetEnumerator()` was the identical shape and was deliberately left alone here, because the copy Go's array range takes had to be placed first; it is settled in the section below.

Guarded by `SliceRangeAllocationTests` in `GolibTests`, which asserts **zero** bytes via `GC.GetAllocatedBytesForCurrentThread` across 1,000 loops (whole slice, sub-window with window-relative indices, and the nil slice), plus the interface-path equivalence. It is a measured guard on purpose: restoring the interface return type still compiles and still produces correct output — it just allocates again — so only bytes can catch the regression. Neutering to the interface return reports 48 B/loop; restoring the original iterator body reports exactly 136 B/loop.

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

With the copy sharing `h` and `x`, `checkSum` destroyed the CALLER's running state: every second
`Sum` on one hash returned a different digest, and `TestGolden`'s write-half → `Sum` → write-rest
sequence produced the hash of the empty string. sha1/sha256/sha512 all failed the same way, as did
`cryptotest.TestHash`'s SumAppend/ResetState/OutOfBoundsRead/StatefulWrite subtests.

The converter now treats such a struct exactly like an array. `typeNeedsValueClone`
(`arrayCloneOperations.go`) is the widened gate — a fixed-size array, **or** a struct carrying one
in a field, directly or through another such struct — so every site the array machinery already
covered (assignment/var-decl RHS, composite-literal element and keyed field, map key and value,
return, channel send, `append` element, range key/value, function/func-literal parameter and value
receiver) clones a struct too. The struct declaration is stamped with the fields that need it, and
go2cs-gen turns the stamp into the deep copy:

```csharp
[GoType] partial struct digest {
    internal array<uint32> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal bool is224;
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    ref var d0 = ref heap<digest>(out var Ꮡd0);
    d0 = d.ΔClone();                 // was `d0 = d;` — the arrays were shared
    var hash = Ꮡd0.checkSum();
    …
}
```

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
- **`array<T>.Clone()` recurses through the new marker.** It already re-cloned an element that is
  itself an array (`[2][3]int`); an element that is one of these structs (`[2]digest`) now clones the
  same way, through `IGoValueClone`/`ICloneable`.
- **EMBEDDED members are never listed and never cloned.** go2cs-gen holds an embed in a `ж<T>` box
  whose member accessor writes THROUGH the box, so assigning one in a clone would corrupt the
  source. Embedded-struct copy aliasing is the separate, pre-existing gap (4) above; this change
  neither fixes nor worsens it, and a struct that needs cloning only because of an embed is not
  stamped.

A BLANK or unnamed parameter is skipped: it is emitted under a synthetic name and can never be
referenced, so there is nothing for the copy to protect — and the preamble would otherwise be
written against the empty analyzed name (` = .ΔClone();`, CS1525 ×2 in `log/slog`'s benchmark
`Handle(disabledHandler, context.Context, slog.Record)`, every parameter of which is blank). The
array-typed arm had the same latent hole; no blank array parameter happened to exist in the corpus.

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
but the observation and two construction paths used to lose it:

- **`operator ==(slice<T>, NilType)`** tested `Length == 0 && Capacity == 0`, misclassifying every
  zero-length zero-capacity view (`[]byte{}`, `[]byte("")`, `x[len(x):]`) as nil. It now tests
  `m_array is null` — representation nilness.
- **`operator ==(slice<T>, slice<T>)`** was structural content equality. Go forbids comparing two
  slices, so the *only* converted code binding this operator is the nil comparison `s == nil`, which
  renders as `s == default!` (the nil literal renders `default!` in value contexts). It is now Go
  slice-**header identity** (same backing array reference, offset, length, capacity), which against
  the default header `(null, 0, 0, 0)` is exactly the nil test. Structural equality remains on the
  `Equals` overloads for C#-side collection use, and the generated named-type wrappers bind `Equals`
  (not this operator), so their behavior is unchanged.
- **`Reslice`** (every `s[a:b]`/`s[a:b:c]`) laundered a nil backing into a fresh empty array
  (`m_array ?? []`). Reslicing nil is legal only within zero bounds, and Go's `nil[0:0]` **is** the
  nil slice (the result shares the nil backing pointer) — it now returns `default` for a nil source.
- **`Append`** with **zero elements** allocated a fresh empty slice for a nil source. Go's
  `append(s)` (and `append(s, empty...)`) returns `s` itself — no growth is needed, so the same
  header comes back: nil stays nil, and bytes.Clone's `append([]byte{}, b...)` with empty `b`
  returns the non-nil literal. `Append` now returns the source unchanged when there is nothing to
  add. (`builtin.widen`, the generic pointer-instantiation projection, likewise now projects only a
  nil source to nil instead of any empty one.)

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

The last three rows close what this section used to record as a **known adjacent gap**: a
zero-argument variadic call materialized a non-nil empty where Go passes nil, because the pack's
`Span<T>` was copied through `ReadOnlySpan<T>.ToArray()`, whose `Array.Empty<T>()` is a real (hence
non-nil) backing array. Nil-ness crosses the C# `params` boundary intact once you read the right
property: **a span's data reference is null exactly when Go's slice header's data pointer is nil**,
which is the same invariant `slice<T>` already keeps one level up (`m_array is null` ⟺ nil).
`SliceExtensions.slice<T>(this Span<T>, …)` now answers `default` for an empty pack whose reference
is null and takes the copy path otherwise, which separates all three variadic shapes above. Roslyn's
choice of `default(Span<T>)` for an empty pack is not language-guaranteed, but the degradation if it
ever changes is to the previous behavior (a real reference, hence non-nil), never to a nil where Go
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

## A slice of a ZERO-SIZE element type carries no storage — `make([]struct{}, math.MaxInt)` allocates nothing

Go's `struct{}` has size 0, so `mallocgc(0, …)` returns the address of the runtime's global
`zerobase` and charges no malloc: `makeslice` multiplies the element size by the capacity and compares
*that* against `maxAlloc`, which for a zero-size element is 0 whatever the length. `make([]struct{}, n)`
therefore succeeds for every `n` up to `math.MaxInt`, and Go's own library leans on it — `slices.Concat`,
`slices.Repeat` and their tests use `[]struct{}` precisely BECAUSE it exercises the length arithmetic at
`MaxInt` without touching memory.

golib allocated a real backing array for the same expression and panicked `makeslice: len out of range`
at `Array.MaxLength` — a ceiling Go does not have here, because Go has nothing to allocate. Both
`slices.TestRepeat` and `slices.TestConcat_too_large` died on their OWN `make([]struct{}, MaxInt)` before
reaching the function under test.

**The predicate is a SHAPE question, not a size question.** `Unsafe.SizeOf<T>()` cannot answer it: C#
gives an empty struct one byte, so every zero-size Go type measures 1. What survives the conversion
faithfully is the FIELD SET — a Go struct is zero-size exactly when it has no fields of nonzero size, and
the emitted C# struct carries the same fields — so `GoZeroSizeFacts<T>` asks that instead, recursively,
with "no instance fields at all" as the base case (golib's `EmptyStruct` for an anonymous `struct{}`, and
every `[GoType] partial struct noCopy { }` the converter emits for a named one). The answer is a
`static readonly` per closed `T`, so every gate written against it folds at JIT time and no ordinary
element type pays for the branch.

Under it, a zero-size slice's `m_array` is a **non-null placeholder** — one shared single-element array,
golib's `zerobase` — so `s == nil` keeps the representation-nilness rule above (a `make`d one is never
nil, the zero header still is), while every window bound is checked against `m_length`/`m_capacity`
alone. Each operation Go answers from length arithmetic does the same here: `make` skips the allocation
and the `Array.MaxLength` ceiling (Go's `len < 0` rule stays), `Reslice` rebuilds the header directly,
`append` bumps the length and applies the identical growth rule, `copy` returns `min(len(dst), len(src))`
and moves nothing (Go's `memmove` of `n * 0` bytes), `clear` is complete before it starts, indexing
answers the ONE shared element (Go computes `&s[i]` as `data + i*0`, so every index names the same
address), and `range` is a counted loop.

**One honest ceiling remained, and the slice-shaped-spread arc retired it (2026-08-27, ruled
post-B2):** `Span<T>.Length` is `int32` while a Go slice length is `int`, and a VARIADIC SPREAD used
to cross exactly that boundary — `append(dst, src...)` emitted `append(dst, src.ꓸꓸꓸ)`, and `ꓸꓸꓸ` is a
`Span<T>`, so `slices.Grow`'s `append(s[:cap(s)], make([]E, n)...)` could not reach `n == MaxInt`.
The priced remedy landed as priced: a slice-typed spread operand now travels **AS THE SLICE IT IS** —
the emission routes it to golib's `ISlice<T>`-taking forms, whose name carries the spread the way the
operand property did:

```go
hello.cipherSuites = append(hello.cipherSuites, defaultCipherSuitesTLS13...)
s2 := append(s[:i], v...)        // in a generic body over S ~[]E
```
```csharp
hello.Value.cipherSuites = appendꓸꓸꓸ((~hello).cipherSuites, defaultCipherSuitesTLS13);
var s2 = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), v);
```

Inside golib the window's own span still serves every copy a span can express (managed backings are
int-bounded by `T[]`, so allocation counts and in-place/grow behavior are byte-identical to the span
core), zero-size windows route by pure length arithmetic BEFORE any span form (which is what lets
`TestConcat_too_large`'s `make([]struct{}, math.MaxInt)` fakes flow through `Concat`'s Grow chain
allocation-free, with the growth rule's even-rounding guarded against wrapping past `MaxInt`), and a
named-slice wrapper arriving through the boxed interface lends its own `ꓸꓸꓸ` projection — one
interface call, inside the core. Two shapes are deliberate: the constrained form takes BOTH type
arguments explicitly (`appendꓸꓸꓸ<S, E>`), because a constraint surface does not participate in C#
inference; and the name is DISTINCT from `append` by design — sharing the overload set re-entered
the C#14 params/betterness thicket (measured CS0121 against the constrained span twin), and the
converter alone mints these calls. Census at the landing: 401 corpus sites across 151 files routed;
string spreads keep the span route (their spread is a byte projection, not a slice). Emission:
`convCallExpr`'s append classification (`spreadArgAsSlice`/`appendTypeArgs`) + `convExprList`'s
spread arm; golib: `builtin.appendꓸꓸꓸ` ×2 over `slice<T>.Append(in slice<T>, ISlice<T>)`. The row
this unlocked: `slices` validates 119 matched · 3 disclosed the moment the arc lands.

**Append-of-make grows in place without the make (2026-09-24, REC-C §B).** Go's compiler recognises
`append(x, make([]T, n)...)` (walk's `isAppendOfMake`, "extendslice") and extends `x` by `n` zeroed elements
without allocating the `make`, which is why `slices.Grow` says it "allocates only once". The emission now
renders exactly that operand as the length-only `makeꓸꓸꓸ<T>(n)`, and nothing else in the call changes:

```csharp
s = subslice<S, E>(appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, cap(s)), makeꓸꓸꓸ<E>(n)), 0, len(s));
```

golib's `makeꓸꓸꓸ<T>` validates the length as `make` does (same panic), and `appendꓸꓸꓸ` over it calls
`slice<T>.AppendZeroed`. That mirrors `Append`'s arms: the zero-size arm, so `TestConcat_too_large`'s
`MaxInt` fakes still allocate nothing; nil; in place, zeroing the shared backing's new elements, which a
stale write beyond `len` would otherwise leak; and growth through `CalculateNewCapacity`. So no `cap()`
moves. Refused, each keeping the make-then-append emission:
- a capacity argument (Go does not recognise it either);
- an element whose zero value must be constructed (`arrayElemFactory`);
- fixed-array dimension cargo;
- a `make` bound to a name.

Stdlib footprint at `fa18863b94`: 19 lines in 16 files on windows, 18 in 15 on linux and darwin, each a
one-operand swap. (Guarded by:
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
a named slice/map/channel is `x == nil`; the converter renders it as `s == default!` (nil literal in
value context) or `s == nil` (pointer context), and — verified against the emitted C# — **both bind the
wrapper's `operator ==(S, NilType)` overload**, not the same-type `operator ==(S, S)` (reverting the
latter has no observable effect; reverting the former flips the result). That overload emitted
`value.Equals(default(S))`, and the **slice** wrapper's `Equals` (from `ISliceTypeTemplate`'s
`Equals(ISlice<T>?)`) is structural *content* equality, so an empty non-nil named slice (`S{}`,
`make(S, 0)`, an `s[len(s):]` tail) was misclassified as nil. It now delegates to `slice<T>`'s own
`== NilType` — REPRESENTATION nilness (null backing array, R13) — via `value.m_value == nil`, so
`IntSlice{} == nil` is false while the zero value stays nil. Audit of the other nil-comparable kinds:
**map** and **channel** wrappers were already correct and are unchanged — they declare no structural
`Equals`, so `Equals(default)` falls back to reference identity through the backing field
(`map<K,V>.Equals` is `ReferenceEquals(m_map, …)`; `channel<T>` compares its queue by reference).
Array/numeric/string/struct/`any` wrappers are not nil-comparable in Go (and the pointer wrapper
already uses a reference-identity `Equals` override), so they keep the structural default; the same-type
`operator ==(S, S)` is likewise left untouched (Go forbids comparing two slices, so no converted code
reaches it). Because this is a compile-time (go2cs-gen) change it leaves TRANSPILER output — the
`.cs.target` goldens — byte-identical, so it is gated on the FULL behavioral suite (four phases)
**plus** a full corpus build rather than CNR alone.

Separately, `NilType`'s `operator ==(ISlice?, NilType)` dropped its historical
`{ Length: 0, Capacity: 0, Source: null }` arm: `slice<T>.Source` (and every wrapper's
`IArray.Source`) materializes a DETACHED copy (`ToSpan().ToArray()`) and is never null, so the
property pattern could never match — the expression already reduced to `slice is null`
(representation nilness), which is what it now states plainly. No converted `s == nil` routes through
this interface operator: a concrete `slice<T>` binds its own `== NilType`, and interface/`any`
comparands bind `NilType`'s `object` arm.

(The named-wrapper rows are guarded by the `NamedSliceNilVsEmpty` behavioral test — named
slice/map/channel zero value (nil) vs empty literal (non-nil), plus the slice `resliceTailCapZero` and
`nilReslice` discriminators, output-compared vs `go run`; it fails if the wrapper's `== nil` regresses
to structural equality.)

## A composite literal omitting a fixed-array field keeps the zeroed backing

A Go struct's fixed-array field is emitted with a field initializer carrying its Go length
(`badCharSkip [256]int` → `internal array<nint> badCharSkip = new(256);`), and C# runs field
initializers in every explicitly declared constructor — but the generated **parameterized**
constructor then assigned every member from its argument, and an argument the composite literal
OMITS arrives as the zero value `default!`, whose backing `T[]` is null. The assignment nulled the
initializer's backing, so the field's first walk NREd (strings' Boyer-Moore
`stringFinder{pattern: …, goodSuffixSkip: …}` never sets `badCharSkip` — the
`TestFinderCreation`/`TestFinderNext` operational blocker, Phase-4 row R8). The generated
constructor now guards exactly the fixed-array members:

```csharp
if (badCharSkip.Source is not null) this.badCharSkip = badCharSkip;
```

`array<T>.Source` intentionally returns the RAW backing reference (null discriminates a
never-constructed zero value), and keeping the initializer for a zero-value argument is precisely
Go's semantics — the zero `[N]T` IS the zeroed backing the initializer produced. A constructed
argument assigns as before (see the copy-semantics gaps above for the literal-argument clone).
Separately, golib `array<T>` reads are now **null-safe**: a bare `default(array<T>)` (a zero value
no constructor ever touched) enumerates/compares/prints as an EMPTY array and panics Go-style on
any index, instead of throwing NRE — mirroring `@string`'s null-safe zero value. The empty view is
a disclosed approximation: the declared length only exists where a constructor or initializer ran,
so a `holder z = default!;` zero-var local still reads its array field at length 0, not N (a known
converter gap, chipped separately; the `make([]S, n)` half of it is now closed — see *`make([]E, n)`
constructs its ELEMENTS by the same rule* below). (Guarded by the
`ZeroValueArrayField` behavioral test — the literal-omission shape ranged/indexed/printed vs Go,
plus an explicit-argument control.)

## A fixed-array composite literal carries its DECLARED length (`.array(N)`)

A `[N]T{…}` literal is **N** long however many elements it writes — Go zero-fills the rest, so
`[8]byte{}` is eight zero bytes and `[8]byte{1, 2}` is `1, 2` followed by six zeros. The literal
renders as a C# element array projected through golib's `.array()` extension, and that element
array holds only the elements actually written, so the projection produced an array as long as the
LITERAL rather than as long as the TYPE. `[8]byte{}` became length **0**: it compiled cleanly and
then panicked on first use (`index out of range [7] with length 0` — math/rand/v2 `chacha8`'s
`Seed`, whose `[8]byte{}` never held a byte). The projection now takes the declared length:

```go
a := [8]byte{}          // eight zero bytes
b := [8]byte{1, 2}      // 1, 2, then six zeros
c := [3]byte{1, 2, 3}   // already full
```

```csharp
var a = new byte[]{}.array(8);
var b = new byte[]{1, 2}.array(8);
var c = new byte[]{1, 2, 3}.array();      // full literal keeps the plain projection
```

Only a **short** literal takes the length argument. A full literal — and every `[...]T{…}`
ellipsis literal, whose length *is* its element count — already yields the right length and keeps
the plain `.array()` form, so existing goldens for those are unchanged. A **slice** literal is
genuinely as long as its elements (`[]byte{}` IS empty) and never pads; its `.slice()` projection
is untouched. golib's `array<T>(T[] source, int length)` constructor does the zero-filled copy,
which is deliberately distinct from the `array(slice<T>, nint)` slice-to-array *conversion* ctor
(there a short source is a Go panic; here it is the normal case).

The same dropped length reached the **indexed/keyed** form by a second route. A keyed literal
whose indices all fold to constants renders as `new array<T>(N){[i] = v}`, which was already
correct — but the scan used `0` as its "no constant keys" sentinel, so a literal whose only key
*is* 0 (`[8]byte{0: 9}`) read as unresolved and fell to the `SparseArray` projection, whose extent
is `max index + 1`, not `N`. Constant-key detection is now tracked separately from the maximum
index, and the `SparseArray` projection — still used for a key that is constant but not a literal
(a `const` identifier), which `SparseArrayIfaceElem`'s `[kLast]shape` registry exercises — also
carries the declared length. (Guarded by the `ArrayLiteralDeclaredLength` behavioral test: empty,
partial, full, ellipsis, keyed, zero-keyed, named, aliased, package-level, non-byte element types,
a tail write proving the backing is really N long, and a `[]byte{}` slice control, output-compared
vs `go run`; the pre-fix converter exits with the index-out-of-range panic.)

**…and the padding itself needs the element factory when the ELEMENT's zero value must be
constructed** (closed 2026-08-26). The length argument sizes the OUTER dimension and fills it with
`default(T)`, which is not usable storage for an unnamed nested fixed array (`[2][3]uint8` emits
`array<array<uint8>>`, and the inner length lives only in the Go type) or for a struct whose
fixed-array field initializer runs only inside a declared constructor. So `[2][3]uint8{}` came out
2 long with two **zero-length** elements — `len(x[0])` reported 0 where Go says 3, and the first
indexed write into one panicked — while the DECLARED form `var x [2][3]uint8` was correct all along,
because it routes through the zero-value construction ladder instead. Two spellings of one Go type
disagreeing is how it surfaced: through `reflect`, where a constructed `ArrayOf(2, TypeOf([3]uint8{}))`
compared unequal to the literal-built value and `TypeOf(lit).Elem().Len()` answered 0.

The padding now carries `arrayZeroValueArgs`' element factory — that same ladder's own renderer,
reused rather than restated, so the literal and the declaration cannot drift apart. It recurses, so
depth composes:

```go
n := [2][3]uint8{}
d := [2][3][4]uint8{}
c := [2]cell{}          // cell has a [4]uint8 field
m := [2]named{}         // type named [6]byte
```
```csharp
var n = new array<uint8>[]{}.array(2, () => new(3));
var d = new array<array<uint8>>[]{}.array(2, () => new(3, () => new(4)));
var c = new cell[]{}.array(2, () => new());
var m = new named[]{}.array(2);           // NAMED element: its wrapper allocates its own backing
```

All **three** routes to the padding carry it: the positional projection above (golib's new
`array<T>(T[], int, Func<T>)` extension), the constant-keyed indexed form
(`new array<array<uint8>>(4, () => new(3)){[1] = …}`, through `array<T>`'s existing
`(nint, Func<T>)` constructor), and the `SparseArray` projection for a key that is constant but not
a literal. The sparse one is not simply the first with a different receiver: a sparse literal's zero
values are its **gaps**, which can sit anywhere rather than only in a tail, and enumerating a
`SparseArray` renders a gap as `default!` — indistinguishable afterwards from an element the literal
genuinely wrote. So that overload asks the sparse array which indices were SET
(`SparseArray<T>.TryGetItem`) and constructs the rest. An element whose `default(T)` is already the
Go zero value — every scalar, and every NAMED array element, whose generated wrapper allocates its
backing lazily from its own known size — renders the bare length exactly as before, so no existing
golden moves. (Guarded by the same `ArrayLiteralDeclaredLength` test, extended with empty/partial/full
nested, three-deep, a needy-struct element, a named-element counter-case, package-level, and both
indexed forms; failing-first measured as `nested empty 2 0 0` against Go's `2 3 3`, followed by the
panic, and `keyed nested 4 0 3 0 3` against Go's `4 3 3 3 3`.)

## Slice-to-array: the VALUE form copies, the POINTER form ALIASES
Go has two slice-to-array conversions and they are different conversions, so each gets its own
golib entry. Both panic Go-style on a short slice.
- The Go 1.20 **value** form `[4]byte(slice)` emits `new array<byte>(s, 4)` — the COPY
  constructor `array<T>(slice<T>, nint)`. Go's conversion copies, so this is exactly faithful
  (netip `AddrFromSlice`, CS1955).

## An ELIDED element carries the same construction as the spelling it elides

Go lets a composite literal **elide** its element's own type, and in two directions: the element's
literal type (`[][2][3]int{{}}` is `[][2][3]int{[2][3]int{}}`) and, when the element type is a
pointer, the `&T` as well (`[]*[4]byte{{}}` is `[]*[4]byte{&[4]byte{}}`). Elision is pure surface
syntax — the two spellings are the same value — so the emission for the elided form must be the
emission for the written one. It was not, in two places, and each failed in a different way
(closed 2026-09-04).

**An elided `&T` whose pointee is not a struct.** The elided path rendered the STRUCT pointee
through the boxed constructor `Ꮡ(new T(…))`; an ARRAY, SLICE or MAP pointee matched nothing and
fell to the generic struct-constructor fallback, which emits a target-typed `new(…)` — against
golib's `ж<T>`, which is `abstract`. That is `CS0144`, so the shape did not compile at all, while
the explicit `&[4]byte{}` spelling of the same value converted and compiled. Each pointee now
renders through the arm that already renders its shape, with the address taken around it:

```go
pa  := []*[4]byte{{}}                  // == []*[4]byte{&[4]byte{}}
psl := []*[]int{{}}
pm  := []*map[string]int{{}}
mp  := map[string]*[2]int{"a": {}}     // map VALUE, same elision
```

```csharp
var pa  = new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice();
var psl = new ж<slice<nint>>[]{Ꮡ(new nint[]{}.slice())}.slice();
var pm  = new ж<map<@string, nint>>[]{Ꮡ(new map<@string, nint>{})}.slice();
var mp  = new map<@string, ж<array<nint>>>{["a"u8] = Ꮡ(new nint[]{}.array(2))};
```

`pa`'s emission is now **byte-identical** to what the explicitly written `[]*[4]byte{&[4]byte{}}`
produces, which is the property the fix is really asserting. A **struct** pointee (`[]*S{{}}`) was
correct all along and is untouched.

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

```csharp
var pna  = new ж<nb>[]{Ꮡ(new nb(new byte[4].array()))}.slice();
var pnsl = new ж<nsl>[]{Ꮡ(new nsl(new nint[]{}.slice()))}.slice();
var pnmp = new ж<nmp>[]{Ꮡ(new nmp(new map<@string, nint>{}))}.slice();
```

Each is byte-identical to the explicit `&nb{}` / `&nsl{}` / `&nmp{}` spelling beside it in the
guard, which is the assertion — elision is surface syntax, so one Go value may not have two
emissions. Before the routing every one of these emitted a bare `new()` against the abstract
`ж<T>` (CS0144), so the shape did not compile at all.

The **wrap** that suggests itself here is measured wrong, and that measurement is why the routing
exists: the same pointee *without* the `&` (`[]nb{{}}`) already emits the structural projection
`new nb[]{new byte[]{}.array(4)}`, which binds the named slot through the generated implicit
conversion — but that conversion is between **values**, not between **boxes**, so
`Ꮡ(new byte[]{}.array(4))` is a `ж<array<byte>>` and cannot bind a `ж<nb>` slot.

One neighbouring shape was **not** closed by this routing, and it is worth keeping the pairing
visible because neither change is sufficient alone: a named type over a *nested* fixed array
(`type nn [2][3]int`) was wrong in **all three** spellings — elided, explicit `&nn{}`, and the
plain declared `nn{}` all printed `2 0 [[] []]` against Go's `2 3 [[0 0 0] [0 0 0]]` — because
the named-array wrapper's empty-literal shortcut emitted `new nn(new array<nint>[2].array())`
with no element factory. That is the `default(T)`-is-not-usable-storage family inside the
wrapper; it predated this routing and was independent of it, and the routing's own property held
over it throughout (the elided spelling agreed with the explicit one exactly, both being wrong).
It was recorded rather than pinned at the time, because a golden for a known-wrong value is worse
than none. **It is closed now** — see *A NAMED array type's EMPTY literal is its zero value* below
— and the guard pins `[]*nn{{}}` beside `&nn{}` as the row that requires BOTH changes: the elided
spelling must be routed to the typed renderer, and that renderer must construct the element.

**An elided fixed-array element whose own element must be constructed.** The elided array arm
carried the declared length for a short literal but not the element factory beside it — the same
`default(T)`-is-not-usable-storage defect the section above closes for the TYPED path, one
spelling over. So `[][2][3]int{{}}` compiled, sized its outer dimension to 2, and left both rows
**zero length**: `len(x[0][0])` answered 0 where Go says 3, `reflect` measured `[2][0]`, and the
first indexed write panicked. Both elided array arms — positional and the `SparseArray` keyed one —
now route through `arrayLengthArgs` and `arrayElemFactory`, the same single renderer the typed
path and the zero-value ladder use:

```go
nested      := [][2][3]int{{}}
nestedShort := [][2][3]int{{{1, 2, 3}}}
nestedKeyed := [][2][3]int{{1: {7, 8, 9}}}
structElem  := [][2]withArray{{}}         // withArray has a [2][3]int field
```

```csharp
var nested      = new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.slice();
var nestedShort = new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3))}.slice();
var nestedKeyed = new array<array<nint>>[]{new golib.SparseArray<array<nint>>{[1] = new nint[]{7, 8, 9}.array()}.array(2, () => new(3))}.slice();
var structElem  = new array<withArray>[]{new withArray[]{}.array(2, () => new())}.slice();
```

The sharpest witness is `[2][2][3]int{{}}`, where both spellings meet in ONE expression: its outer
literal is written, so it took the typed padding `.array(2, () => new(2, () => new(3)))` and was
correct, while its single elided element took `.array(2)` and was not — one Go value, two lengths,
in the same line. A full literal, a slice literal, and an element whose `default(T)` is already the
Go zero value (every scalar, and every named array element) render exactly as before.

(Both guarded by the `CompositeLiteralElements` behavioral test, output-compared against `go run`
over the elided and explicit spelling of each shape plus writes through the constructed storage.
Failing-first measured separately, because neither defect's control subsumes the other's: the
pointer half is nine `CS0144`s at the Compile phase, and the array half **compiles clean** and
fails only at Output — `nested: 1 2 0 [[] []]` against Go's `1 2 3 [[0 0 0] [0 0 0]]`, then
`panic: runtime error: index out of range [2] with length 0` on the write.)

## A NAMED array type's EMPTY literal is its zero value — and its element may need constructing

A named type over a fixed array (`type nn [2][3]int`) lowers to a go2cs-gen wrapper around
`array<E>`, and an EMPTY composite of it (`nn{}`) is the type's zero value rather than a length-0
literal. The wrapper's renderer has a shortcut for exactly that case — `new nn(new array<nint>[2].array())`
— because `new T[N]` is already the zero-filled declared length and restating it would merely emit
`new byte[6].array(6)`. That reasoning holds for every element whose `default(T)` is already the Go
zero value, and fails for the two shapes that must be CONSTRUCTED: `new T[N]` is N copies of
`default(T)`, which is *sized but unusable storage* — the same
`default(T)`-is-not-usable-storage defect the two sections above close, one level up and reached
through the wrapper instead of the projection.

So `nn{}` produced an outer length of 2 whose rows were **length zero** — `2 0 [[] []]` against Go's
`2 3 [[0 0 0] [0 0 0]]` — and a named array over a struct carrying a fixed-array field (runtime's
`semTable` shape, `[251]struct{root semaRoot; pad [40]byte}`) panicked on the first inner index. The
empty shortcut now consults `arrayElemFactory` and, when the element needs one, backs the wrapper
with the element-factory `array<T>` constructor — the same form the wrapper's own KEYED branch
already used, through `arrayLengthArgs`, the one place that argument list is spelled:

```go
nnLit := nn{}                 // type nn [2][3]int
nnPtr := &nn{}
nsLit := ns{}                 // type ns [2]withArray — withArray has a [2][3]int field
nbLit := nb{}                 // type nb [4]byte — element needs nothing
```

```csharp
var nnLit = new nn(new array<array<nint>>(2, () => new(3)));
var nnPtr = Ꮡ(new nn(new array<array<nint>>(2, () => new(3))));
var nsLit = new ns(new array<withArray>(2, () => new()));
var nbLit = new nb(new byte[4].array());          // shortcut PRESERVED — nothing to construct
```

`arrayLengthArgs`' own comment names four renderers that must carry the factory, "the named-array
wrapper's" among them; before this that was true only of the wrapper's keyed path, so the empty
shortcut was precisely the fifth caller the comment was written to prevent. The wrapper's short and
keyed literals already carried it and are unchanged, as is every named array whose element is a
scalar or another named type.

**The zero value reached WITHOUT a literal is a DIFFERENT layer and is deliberately still wrong.**
`var x nn`, `new(nn)` and `default(nn)` never call the wrapper's constructor: the converter's
`zeroValueInitializer` routes a named array to `default!` on the stated ground that the wrapper
allocates its backing lazily from its own known size (the same exclusion `arrayElemFactory`
documents), and go2cs-gen's lazy backing is `new array<E>(N)` with no factory and no way to recover
an inner Go length. That is where the standard library actually builds these — of **86** named array
types (production and `_test.go`), **5** have an element whose zero value needs construction, and all
five are reached that way: `runtime.semTable` (`var semtable semTable`),
`crypto/internal/boring/bcache.cacheTable` (`new(cacheTable[K, V])`), `crypto/internal/nistec`'s
`p256Table` and `p256AffineTable`, and a ten-deep `[2]^10 *int` in runtime's own GC test (`new(T)`),
which only a `-tests`-dimension census can see. Closing it needs the factory to reach a position with
no template and no type syntax — a golib, go2cs-gen or `zeroValueInitializer` change with its own
gates — so it is recorded here and at the guard rather than pinned, since a golden for a known-wrong
value is worse than no golden.

(Guarded by the `CompositeLiteralElements` behavioral test: the plain and explicit-pointer spellings
of both shapes, a write through the constructed storage, the elided spelling as the cross-renderer
control, and the populated, short, keyed and plain-element literals as the must-not-change controls.
The predicate has THREE arms and each has a row, because a control only tests the axis it varies: an
element needing construction (`nn`, `ns`), a plain scalar element (`nb`), and a NAMED array element
(`type ni [3]int; type no [2]ni`) — which keeps the shortcut and is still correct, because its own
wrapper allocates a backing of its own known size. That last arm is what pins `arrayElemFactory`'s
named-element exclusion from this side.
Failing-first measured: at the pre-fix converter the same program reports `FAIL [Target,Output]` —
`nnLit: 2 0 [[] []]`, `nnPtr: 2 0 [[] []]`, then `panic: runtime error: index out of range [2] with
length 0` on the write, exit code 2 against Go's 0.)

## An array or slice literal may MIX positional and keyed elements

Go's "all elements keyed, or none" rule is a **struct**-literal rule. An array or slice literal may
mix the two freely, and the positional elements take the indices Go computes for them: the first
element is index 0, a keyed element sets the index to its (constant) key, and each following
positional element continues from there. So the literal below is **sixteen** bytes long, not three:

```go
ip := []byte{0xfe, 0x80, 15: 0x01}   // 0: 0xfe, 1: 0x80, 15: 0x01 — length 16
a  := [8]int{1, 2, 5: 9, 10}         // 0: 1, 1: 2, 5: 9, 6: 10 — length 8, its declared one
```

The converter's keyed-literal detection read `Elts[0]` alone (its own comment cited the struct rule
as the justification), so a mixed literal took the plain positional emission while its keyed
elements still rendered through the key/value arm — whose sparse form wants an assignment target
that does not exist in an expression position:

```csharp
// before — CS1525, invalid expression term '<'
new byte[]{0xfe, 0x80, <nil>[15] = 0x01}.slice()
```

A mixed literal is now normalized to an all-keyed one carrying Go's own indices, which lets the
existing sparse-array machinery render it unchanged — and recovers the LENGTH, which is the part a
wrong emission gets silently wrong rather than loudly:

```csharp
new slice<byte>(16){[0] = 0xfe, [1] = 0x80, [15] = 0x01}
new array<nint>(8){[0] = 1, [1] = 2, [5] = 9, [6] = 10}
```

An all-positional or already-all-keyed literal is untouched by construction, so the corpus is
byte-identical across the change; a literal whose keys do not fold to constants is left exactly as
it was, because an index the converter cannot compute is one it must not invent. Guarded by
`mixedKeyedComposite_test.go`, which converts both mixed forms plus the two unmixed controls.
(Found by the Phase-4 measurement of `net/netip`, whose `TestAddrFromSlice`/`TestAsSlice` write
IPv4-in-IPv6 addresses this way — zero production sites in the converted standard library, which is
why a shape this ordinary survived to be found by a test conversion.)

## A fixed-size array constructs its ELEMENTS when `default(T)` is not usable storage

`new array<T>(N)` fills its backing with `default(T)`, which is the correct Go zero value only when
`default(T)` is itself well formed. For a NESTED fixed array it is not: `[2][4]byte` emits
`array<array<byte>>`, and the inner length `4` lives only in the Go type — `array<T>` has nowhere to
carry it, so golib cannot recover it from `T`. Every element kept a null backing, so `len(x[1])`
reported 0 where Go says 4, and the first indexed write panicked (`index out of range [2] with
length 0`) — a silent-correctness defect that compiled clean. The same held for an element whose own
zero value needs construction: `default(T)` skips the generated constructor that runs a struct's
fixed-array field initializers and allocates its embed boxes.

Only the converter knows the element's shape, so it supplies an element factory to a golib
`array(int length, Func<T> elementFactory)` constructor:

```go
var x [2][4]byte           // len(x), len(x[1]) => 2 4
var deep [2][3][4]byte
var se [2]inner            // type inner struct { b [3]byte }
```
```csharp
array<array<byte>> x = new(2, () => new(4));
array<array<array<byte>>> deep = new(2, () => new(3, () => new(4)));
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
```csharp
ref var leafCounts = ref heap(new array<array<int32>>(16, () => new(16)), out var ᏑleafCounts);
f.bits = Ꮡ(new array<nint>(316));
```

Those last two were the same silent-correctness defect one layer down. The heap-boxed declaration is
a THIRD emission path (`convertToHeapTypeDecl`, a string path that never consulted
`arrayZeroValueArgs`), and it is exactly the shape `compress/flate`'s Huffman coder uses —
`bitCounts`' `leafCounts [16][16]int32` is boxed because `copy(leafCounts[i][:i], …)` slices an
element, and its first `leafCounts[level][level] = 2` panicked with `index out of range [1] with
length 0`, taking `compress/gzip` and `compress/zlib` down with it. `new([N]T)` is a FOURTH: golib's
`@new<T>()` builds the zero value through the parameterless constructor, where `array<T>()` has no
length at all, so `f.bits` came back length 0 (Go: 316). A NAMED array type keeps the zero-value
`@new<row>()` form for the same reason a named element needs no factory.

A NAMED array element needs no factory and is deliberately left alone: `type row [4]byte` generates
a wrapper that allocates its backing lazily from its own known size (go2cs-gen's
`m_value ??= new row(4)`), so `array<row> nr = new(2);` is already correct. Elements whose
`default(T)` is a valid zero value (scalars, pointers, slices, maps) likewise keep the bare
`new(N)`, which keeps the A/B footprint to genuinely nested shapes.

This mirrors go2cs-gen's `AppendZeroValueInitializers`/`NeedsConstruction`, which does the same for
struct FIELDS, and narrows the zero-value gap disclosed above — a `default!` zero-var local still
reads an array field at length 0. (Guarded by the `NestedFixedArrays` behavioral test: inner `len`,
writes read back through inner arrays, per-element storage independence, three-level nesting,
struct/named-array elements, and the global paths, all compared against `go run`.)

## A NAMED array's ZERO VALUE constructs its elements through the wrapper's lazy backing

The section above closes every site the CONVERTER can spell. A named array type has a whole class of
sites it cannot: the wrapper allocates its backing lazily, so `default(nn)` — with no declaration for
the converter to rewrite — is a complete Go zero value that go2cs-gen materializes on first touch as
`new array<E>(N)`. That is the same `default(E)` fill one layer down, and it was wrong for the same
two element shapes.

Measured against `go run`, `type nn [2][3]int` and `type ns [2]wa` (`wa` carrying a fixed-array
field — runtime's `semTable` shape) diverged at **seven distinct site kinds**, and the arithmetic is
what identifies the single cause:

| site | Go | before |
|---|---|---|
| `var d nn` · `new(nn)` · a struct FIELD · a named RESULT · a map read | `2 3 [[0 0 0] [0 0 0]]` | `2 0 [[] []]` |
| `var aa [2]nn` · `make([]nn, 2)` | `2 2 3 …` | `2 2 0 …` |
| `d[1][2] = 9` | `[[0 0 0] [0 0 9]]` | `panic: index out of range [2] with length 0` |

The `2 2 0` rows are the diagnosis, not a detail: in an `array<nn>` the outer and middle dimensions
were **already correct**, because the wrapper heals its own length on first touch. Only the innermost
— the one the lazy backing fills — was wrong. So all of these funnel through ONE emission, and
narrowing `zeroValueInitializer`'s named-array carve-out would have reached only the two sites that
have a declaration statement (`var` and the named-result prologue), leaving five.

The fix is therefore at that one emission, and it is split across the two halves by **which half
holds the fact**:

- **go2cs-gen** answers for a STRUCT element, reusing the `NeedsConstruction` predicate it already
  applies to a struct FIELD (now `static`, taking its context and cache explicitly, so there is one
  definition rather than a second copy). It owns the half the converter could not supply anyway: a
  cross-ASSEMBLY element resolves by metadata symbol rather than syntax. The rendering is the same
  NilType construction a needy field takes — `new array<wa>(2, static () => new wa(nil))`.

  The element name has to be asked in **both spellings it can arrive in**, and this cost a measured
  defect before it was found. A struct FIELD reaches the predicate already `global::go.`-rooted,
  because `GetStructMembers` produces rooted names; a `[GoType("[N]E")]` descriptor's element is
  *package-alias-qualified* (`sync.atomic_package.Pointer<…>`), which is not a CLR name at all —
  every converted package class lives under the `go` namespace. Asked in that spelling alone, every
  cross-assembly element answered false, and answered it **silently**: the wrapper simply kept the
  bare-length backing this change exists to remove. `FindUnderlyingStructSymbol` already documents
  and performs the retry for the same reason one hop over, so the lookup goes through it. The
  corpus's own control says the symbol path was never at fault — `math/rand/v2`'s `ChaCha8`
  constructs its cross-package `chacha8rand.State` field today, from a rooted name — and a
  three-arm probe (cross-package non-generic, cross-package generic closed over a concrete type, and
  a generic named array whose element closes over its OWN type parameters) is what pinned the axis
  as *cross-assembly* rather than *generic*: all three declined before, all three construct now, all
  three byte-identical to `go run`.
- **The converter** answers for a nested UNNAMED array element, because nothing downstream can. The
  descriptor is `[2]array<nint>` — the inner `3` is gone — and an `array<T>`'s length is INSTANCE
  state, so a site with no instance cannot recover it. It stamps
  `[GoType("[2]array<nint>")] [GoArrayDims(2, 3)] partial struct nn;` and gen builds the factory from
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

Corpus footprint at the cut: **zero**. A census of the emitted corpus (not a GOROOT `go/packages`
walk — `-stdlib` defaults to `-tags purego`, so the asm-path types such a walk counts are absent from
the emission) finds **59** named fixed-array wrappers, **none** with a nested-array element, and
exactly **one** with a needy element: `runtime.semTable`, whose lifted anonymous-struct element
carries `pad = new(40)`. It is answered by gen, needs no stamp, and its generated backing is now
`new array<semTableᴛ1>(251, static () => new semTableᴛ1(nil))`.

`bcache.cacheTable` looks like a second and is **not** one, which is worth stating because a
text-keyed census says otherwise: its element `atomic.Pointer<T>` carries exactly one fixed-array
field and that field is Go's **blank identifier** (`internal array<ж<T>> _ = new(0)`, the
"mention `*T` to disallow conversion" trick). A `_` field is unreadable in Go, so its zero value can
never be observed, and go2cs-gen's predicate skips it on both its syntax and its symbol path.
Declining to construct there is correct, not a gap — and the value would have been identical anyway,
since the initializer is `new(0)`.

So the stamp changes no emitted file today and the gen half moves exactly one corpus site. This is an
end-user-Go correctness fix reached through `-recurse`, plus the guarantee that the next such type
converts correctly.

(Guarded by the `NamedArrayZeroValue` behavioral test: both shapes × both spellings (`var`, `new(T)`)
× the seven site kinds, the write arm, and the two controls that pin the boundary from the other side
— a plain element and a NAMED element, both already correct and both required to stay on the bare
`new array<E>(N)` backing. The sibling `CompositeLiteralElements` guards the same element predicate
reached through a composite LITERAL; this one guards the zero value, which is where every one of the
standard library's needy named arrays is actually built.)

## The array-backing publish is atomic per box

`ж<T>.at<Telem>(i)` has to reach a go2cs-gen named fixed-size array wrapper's LAZY backing, and
`ж<T>` is deliberately **unconstrained** in `T`, so golib cannot call an interface member on
`ref Value` without boxing a copy. The sequence it used was box the wrapper, touch `Source` so the
backing materializes on that copy, copy the whole wrapper back over the real storage — correct
single-threaded (that copy-back IS `47ddd5a50`'s fix for the same lost write) and **lossy with two
threads**, because it is an unsynchronized read-modify-write of shared state. Two threads reaching a
still-lazy wrapper each allocated their own backing; the second copy-back discarded the first along
with every element already written into it, and the element pointers already handed out kept naming
the orphan. Because the wrapper is several words wide, the half-done copy-back could also be
*observed*, surfacing as a spurious `IndexOutOfRangeException` out of `at`'s bounds check rather
than as a lost write.

`crypto/internal/boring/bcache`'s concurrent section is the measured victim: entries lost in ~28% of
runs, and always in the first ~15 of 102,100 — the fingerprint of a bounded start-up window rather
than of a broken CAS or a GC interaction (both A/B-eliminated).

The publish is now gated per **box**, which is the only durable unit available: the by-value copy
cannot be one, and constraining `T` is not on the table — the constrained-CALL route was torn out in
`d5c0c9c10` for killing every Native AOT binary at type-init. `m_publishedArrayBacking` serves two
jobs at once: `null` is the once-only gate (every thread serializes through `lock (this)`, which is
exactly the cold-start window the race lives in), and a *different* backing is the reassignment
detector, so no stale ready-flag can hand out a pointer into a private copy after `*p` is assigned a
fresh zero wrapper. The fast path is lock-free — one acquire read, one type test, one reference
compare.

The publish path is also narrowed to the shapes that actually *are* lazy, which fixed a second,
separate defect the old unconditional probe carried. golib's own `array<T>`/`slice<T>` and every
named-slice wrapper hold their backing in a field, so there is nothing to publish — and
**`slice<T>.Source` is defined to return a DETACHED COPY**, so the old code allocated and threw away
a full copy of the backing on *every* element take through a `ж<slice<T>>`. Measured (isolated
processes, median of three): slice `.at()` **215.09 → 29.00 ns/op**, array `23.66 → 21.84`, named
wrapper `28.51 → 26.22`. Every shape got faster; the fix removes an allocation from the hot path
rather than adding a lock to it.

> **Doctrine: a lazy-initialization fix is not finished until the publish is atomic.** `47ddd5a50`
> correctly diagnosed "the allocation landed on the copy and the real storage stayed virgin" and
> added the copy-back. The single-threaded repair of a lost-write defect is exactly the shape that
> leaves a concurrency residue behind.

**Not closed by this**, because no golib-side gate can be: the generated `Value => m_value ??= …`
getter is itself a read-modify-write, so two threads first-touching the *same struct instance* by
ref still race (`ref semTable semtable => ref Ꮡsemtable.Value; semtable[i] = x`). Closing that needs
an atomic publish inside the generated getter (go2cs-gen). Measured unchanged at ~95% of trials
(ElemAliasProbe `arm7`) — closed separately, see *The named-array wrapper publishes its lazy backing
atomically* below.

## The element address of a VIRGIN named array must materialize through the receiver

The arm above hands `&t[i]` to golib's by-value `Ꮡ<T>(IArray<T> target, int index)`, which was
reasoned sound because "a named fixed-array type is generated as `IArray<E>` over a shared backing
`E[]`". That is true of golib's own `array<E>` — an eagerly-allocated readonly struct, where a copy
shares the storage — and **false of the go2cs-gen wrapper**, whose backing is allocated on first
touch:

```csharp
private array<E>? m_value;
public  array<E>  Value => m_value ??= new array<E>(N);
```

The overload takes its target by value, so the CALL SITE boxes the wrapper and golib only ever sees
that private copy. Over a still-zero wrapper the `??=` therefore ran on the boxing temp, the
receiver's storage stayed virgin, and **every element pointer named a fresh throwaway array — every
write through it silently lost, single-threaded, no concurrency required.** `runtime`'s `rootFor` is
the only access path to `semtable`, so nothing ever materialized the shared table: each call handed
back a pointer into its own private 251-entry array of zero `semaRoot`s. (Latent only because
`sync`'s Mutex/RWMutex/WaitGroup are hand-owned on `SemaphoreSlim` and never reach
`runtime.semacquire`.)

The emission projects through the wrapper's own `Value` getter first:

```csharp
Ꮡ(t.Value, i)      // was: Ꮡ(t, i)
```

`Value` is a **mutating struct member**, so invoking it on the `ref` receiver (or on a field of one)
runs the `??=` against the REAL storage, and the `array<E>` it returns shares that backing — so the
element box aliases the receiver. Both wrapper flavors carry it: a direct-array RHS
(`type Mont [4]uint64`) exposes `Value : array<E>`, and a named RHS (`type pallocBits pageBits`)
yields the view wrapper whose `Value` is that named type, itself an `IArray<E>` over the same
storage. An UNNAMED `[N]E` base renders as golib `array<E>`, has no `Value` member and needs none,
so the projection is gated on the base being a named type over an array
(`lazyArrayBackingProjection`, `convUnaryExpr.go`) and every other site is unchanged — a seeded
whole-corpus reconvert, diffed emission-against-emission, moves **exactly one file**:
`runtime/sema.cs`.

The `.at<E>(i)` route would also be correct (it publishes through the box — see golib's
`arrayView`/`publishArrayBacking`), but it is unavailable here for the same reason this arm exists
at all: a `[GoRecv] ref` receiver has no `ж<>` box.

Guarded by **`NamedArrayWrapper`**'s `slots`/`slot` probe — a pointer-receiver method returning
`&s[i]` on a virgin wrapper, written through and read back. Verified as a real gate rather than a
green that cannot go red: at the previous emission it reports `stdout mismatch C# vs Go`.

**A different door, measured and NOT closed by this.** `runtime/mpallocbits.cs`'s
`Ꮡ((pageBits)(b))` binds golib's standard-box `Ꮡ<T>(in T)` over a value produced by the generated
by-value conversion operator (`implicit operator pageBits(pallocBits value) => value.view`), which
materializes on the operator's own parameter copy. First-touch writes through it are lost; once
anything else materializes `b`, every copy shares the backing and writes land (measured both ways —
ElemAliasProbe `arm8`). It needs its own increment.

## The named-array wrapper publishes its lazy backing atomically

The third door of the same family, and the one neither of the others can reach: the generated
wrapper's **own** `Value` getter, reached by a plain `ref` with no golib on the path at all —
`internal static ref semTable semtable => ref Ꮡsemtable.Value;` and then `semtable[i] = x`. A per-box
publish gate in `ж<T>.at()` never sees it (there is no `at()` call), and the receiver projection above
never sees it either (there is no `Ꮡ`). What it meets is `m_value ??= new array<E>(N)`, a
read-modify-write of shared mutable state: two threads that first-touch the same zero-valued wrapper
each allocate a backing and the second store **orphans the first**, together with every element
pointer already derived from it. Silent — no fault, no exception — and confined to a start-up window
measured in microseconds. Measured at **872 of 900** concurrent first-touch trials (ElemAliasProbe
`arm7`, 24 threads × 300 trials × 3 batches).

The publish becomes an interlocked CAS. Every racing thread allocates, exactly one wins the slot, and
the losers discard their allocation *before* anything can derive an element address from it — which is
what makes it correct rather than merely narrower:

```csharp
private global::System.Runtime.CompilerServices.StrongBox<array<uint64>>? m_value;   // was: array<uint64>?

public array<uint64> Value
{
    get
    {
        global::System.Runtime.CompilerServices.StrongBox<array<uint64>>? value = m_value;

        if (value is null)
        {
            var created = new global::System.Runtime.CompilerServices.StrongBox<array<uint64>>(new array<uint64>(256));
            value = global::System.Threading.Interlocked.CompareExchange(ref m_value, created, null) ?? created;
        }

        return value.Value;
    }
}
```

**Why the slot had to change shape at all.** An interlocked publish needs ONE machine word. `array<E>`
is a 3-field readonly struct (backing plus the `Alias` window's low/length), so `array<E>?` is 24
bytes — it can neither be CAS'd nor even *read* without tearing while another thread writes it. The
narrower one-word alternative, holding the bare `E[]`, does not preserve the value: a
constructor-supplied array may be an alias **window** (`array<E>.Alias`, Go's `(*[N]E)(s)`) whose
`Source` is wider than the array, and flattening it to its backing would silently widen the named
array and shift its origin. The holder carries the whole `array<E>`, so nothing is lost. It is
`StrongBox<array<E>>` and not plain `object` because an `object` slot makes every warm read an
`unbox.any` — a type-check helper *call* in the hot loop; measured on the element-address path over 64
cold tables, `1.97 → 4.13 ns/op` for `object` against `1.97 → 2.45` for the typed holder.

The residual cost is one dependent load and the probe reports it honestly (`arm9`, both emissions in
one process): the raw `Value` getter gets **faster** (`1.03 → 0.87 ns/op` — the wrapper struct shrank
from 24 bytes to 8, so every Go by-value array copy moved with it), the element path over ONE
long-lived table — what the corpus's named arrays actually are, and where the JIT hoists the
loop-invariant getter — sits between `−1%` and `+12%` run to run, and the pathological shape of 64
separate non-resident tables costs `+17…25%`.

**The consequence that had to be measured, not reasoned.** A Go fixed-size array is COMPARABLE and
legal as a map key. With no overrides a C# struct inherits `ValueType.Equals`/`GetHashCode`, and both
read the single `m_value` field — now a *reference*. So two distinct wrappers over equal content began
comparing unequal and hashing differently, missing each other in a map and in `reflect.DeepEqual`:
precisely the silent wrong answer this door exists to remove, traded for a different one. The `==`
operator hid it completely, because `EqualityExpression` binds the wrapper's own
`Equals(IArray<E>)` at COMPILE time and that was structural all along. The Array kind therefore emits
both overrides, delegating to `array<E>`'s element-wise pair so neither depends on the slot's shape
any more:

```csharp
public override bool Equals(object? obj) => obj is Table other && Value.Equals(other.Value);

public override int GetHashCode() => Value.GetHashCode();
```

One golib companion follows for the same reason: `GoReflect.TryUnwrapWrapperValue` reads `m_value` by
reflection to hand callers the wrapper's underlying value, so it unwraps the holder's extra level (no
converted or golib type is ever an `IStrongBox`).

Guarded by **`NamedArrayWrapper`**'s map-key probe — two separately built equal keys, a re-store
through the second, a third distinct key, and the same for the VIRGIN zero array whose backing neither
side has materialized. Verified as a real gate rather than a green that cannot go red: with the two
overrides suppressed and nothing else changed, it reports `stdout mismatch C# vs Go`.

**Still not closed, by construction:** a materialization that happens on a by-value COPY of the
wrapper publishes to the copy's field, so the `arm8` `Ꮡ((pageBits)(b))` door above is untouched. The
emission-vs-emission blast radius is nil — a generator change alters no committed `.cs`, and the
suite's Transpile and Target phases stay byte-identical across it.

## `make([]E, n)` constructs its ELEMENTS by the same rule

`slice<T>`'s length constructor fills its backing with `default(T)` exactly as `array<T>`'s does, so
the identical silent-correctness defect reached `make`. `make([][hashSize]int, n)` emitted
`new slice<array<nint>>(n)` and produced n **zero-length** arrays, because the inner length lives
only in the Go type — so hash/maphash's `avalancheTest1` panicked on its first `g[j] += …`
(`index out of range [0] with length 0`), and `image/draw`'s Floyd-Steinberg `quantErrorCurr`/`Next`
rows and `x/text/transform`'s chain buffers carried the same latent defect unexercised.

`make` now threads the **same** `arrayElemFactory` the fixed-array zero-value sites use into a golib
`slice(nint length, Func<T> elementFactory, nint capacity = -1, nint low = 0)` constructor — one
rule, one predicate, both containers:

```go
grid := make([][hashSize]int, n)          // hash/maphash smhasher_test.go
q := make([][4]int32, r.Dx()+2, cap)      // image/draw
```
```csharp
var grid = new slice<array<nint>>(n, () => new(64));
var q = new slice<array<int32>>(r.Dx() + 2, () => new(4), cap);
```

The factory fills the **whole backing**, not just the first `length` elements: Go zeroes the entire
allocation, so the capacity beyond the length is already valid storage once a re-slice or `append`
exposes it. As with `array<T>`, a NAMED array element (`type row [4]byte`) and every element whose
`default(T)` is a valid zero value keep the plain length constructor, so the A/B footprint stays on
genuinely nested shapes. The composite-literal path (`[][4]int{{…}}`) is still open, chipped
separately with the array-literal case above. (Guarded by `NestedFixedArrays`, extended with the
`make` length and length+capacity forms, a struct element needing construction, and a named-array
element control, output-compared vs `go run`.)

**A DEFINED slice type routes the factory through its underlying `slice<E>`** (2026-07-27). `make`'s
target is not always `slice<E>`: for `type SortedMap []KeyValue` the target is the go2cs-gen wrapper,
which declares `SortedMap(nint length, nint capacity = -1, nint low = 0)` and **no element-factory
overload**, so the lambda bound to `nint` — `CS1660: Cannot convert lambda expression to type 'nint'`
at `internal/fmtsort`'s `make(SortedMap, 0, n)`. The factory-filled backing is therefore built as the
underlying `slice<E>` — bit-for-bit the value the unnamed form produces — and handed to the wrapper's
`T(slice<E> value)` constructor, which the generator always emits:

```go
sorted := make(SortedMap, 0, n)           // internal/fmtsort sort.go
```
```csharp
var sorted = new SortedMap(new slice<KeyValue>(0, () => new(), n));
```

This was a **live** defect, not a latent one, and its blast radius is worth recording: `-tests`
regenerates production `.cs` on every run, so once the run for one banked package left a broken
`sort.cs` on disk, every later package downstream of `fmt` failed to build too — a validated sweep
read as 41 pass / 20 fail from this single root. It is the fourth instance of the zero-value-
construction class in a fourth emission path, which is the standing argument for centralizing that
construction rather than patching sites.

## `make([]T, len[, cap])` out-of-range panics are RECOVERABLE, with Go's messages

Go's `makeslice` panics recoverably for a negative or over-allocatable length/capacity — the
recovered value's text is `runtime error: makeslice: len out of range` (or `cap`; probed vs
`go run` — the recovered value is a `runtime.errorString`). golib's make path (the
`slice<T>(nint length, nint capacity, nint low)` constructor) raised
`ArgumentOutOfRangeException`/`OverflowException` for the same inputs — .NET exceptions
`recover()` cannot catch, so a deferred recover never ran and the process died. The constructor
now validates first and throws `RuntimeErrorPanic.MakeSliceLenOutOfRange()` /
`MakeSliceCapOutOfRange()` (recoverable `PanicException`s carrying Go's message text), using
`Array.MaxLength` as .NET's `maxAlloc` equivalent. The same validation class applies to the
hand-owned `internal/bytealg.MakeNoZero` (`bytealg_impl.cs`) — Go's runtime implementation of it
panics `len out of range` before allocating, and strings/bytes `TestRepeatCatchesOverflow`
recovers that panic and matches on `"out of range"` (Phase-4 row R6; `strings.Repeat` of a
near-`maxInt` product reaches `MakeNoZero` after passing Repeat's own overflow pre-checks).
Like the established golib runtime-panic convention, the panic STATE is the message string, not
an `error` value — a recovering type switch takes Go's `case error:` arm only in Go; both sides
converge on the same `err.Error()` text through the `fmt.Errorf("%s", v)` default arm. (Guarded
by the `MakeSlicePanicRange` behavioral test — in-range, negative, huge-length, and huge-capacity
`make` under `recover()`, messages compared vs Go.)

## `clear` rebuilds each element through golib's `GoZero` — the RUN-TIME half of zero-value construction

The fifth instance of that class landed in golib rather than the converter, and it is the one that
made the centralization real. `builtin.clear(slice<T>)` assigned `default!` to every element, so
`clear(q)` over a `[][4]int32` replaced each element with a **length-zero** array — Go's
`clear` leaves four zeroed `int32`s. `image/draw`'s Floyd–Steinberg dither calls
`clear(quantErrorNext)` once per scan line, and the next row's `quantErrorNext[x][0]` panicked with
`index out of range [0] with length 0`. The same held one level in for a struct element carrying a
fixed-array field: `default!` skips the generated parameterless constructor that runs its field
initializers.

The converter cannot help here — a `clear` call site has no Go type shape to thread a factory
through, and the C# type `array<E>` is the same for every N. Patching the site would have been the
fifth per-site repair of one defect, so the shape recovery moved into golib and became a single
entry point:

```csharp
public static T GoZero<T>(T template)   // builtin.cs
```

`GoZero` returns the Go zero value of `T`, consulting `template` **only for run-time shape**, and
resolves one of three answers from a per-closed-`T` static (`ZeroFacts<T>`, the same JIT-folding
shape as `AssertFacts<T>`), so the overwhelmingly common case compiles to a constant `default`:

| `T` | zero |
|:--|:--|
| any reference type, or a value type golib owns (`@string`, `slice<T>`, `map<K,V>`, the numerics) | `default` |
| `array<E>` — implements the new golib marker `IGoZeroShaped` | `GoZeroLike()`: a new array of the template's LENGTH, elements zeroed recursively so `[2][3]int32` keeps its inner lengths |
| a converted Go struct (`[GoType]` + a generated parameterless constructor) | that constructor — exactly what the converter emits for `var x T`, and `default` for a plain struct |

All three slice-shaped `clear` overloads (`slice<T>`, `Span<T>`, and the constrained `ISlice<T>`)
route through one `Span<T>` body that keeps the vectorized `Span.Clear()` whenever
`default(T)` is already the Go zero value:

```go
q := make([][4]int32, 3)
q[1][2] = 7
clear(q)
len(q[1]) // 4, not 0
```

The `[GoType]`-plus-constructor rule is deliberately broad rather than a per-shape enumeration
(fixed-array field, promoted embed, …): calling a converted struct's own zero-value constructor is
always correct, so a FUTURE field shape that needs construction is covered without re-opening the
class a sixth time. That generality is the whole point — this is the run-time counterpart of the
converter's `arrayZeroValueArgs` and go2cs-gen's `AppendZeroValueInitializers`, which build a zero
value where the shape is known statically; `GoZero` recovers it from a value that already carries
it, which is what a built-in is handed. (Guarded by the `ClearBuiltinShadow` behavioral test,
extended with `clear` over an array-element slice, a struct-with-array-field slice, and an
array-of-arrays element, each written to after the clear and output-compared vs `go run`.)

<a id="a-map-read-of-a-shape-carrying-element-supplies-the-zero-from-the-call-site"></a>Moved to [A map READ of a shape-carrying element supplies the zero from the CALL SITE](maps-and-channels.md#a-map-read-of-a-shape-carrying-element-supplies-the-zero-from-the-call-site).

## A named slice wrapper's non-generic `ISlice.Append` is an EXPLICIT implementation

The generated wrapper for `type S []E` implements both halves of the golib slice surface, and both
declare an `Append`: the typed `ISlice<E>.Append(E[])` and the non-generic `ISlice.Append(object[])`.
`ISliceTypeTemplate` emitted both **public**, which is fine for every `E` except one — with
`E = any`, `object[]` and `E[]` are the SAME parameter list, so the wrapper carried two public
methods differing only in return type: **CS0111**. That is a single duplicate-member emission, and
it held two whole converted test suites, `fmt`'s `type SE []any` (63 verdicts) and `archive/tar`'s
`type fileOps []any` (97) — a `[]any` named slice is a table-driven-test idiom, which is why the
production corpus never met it.

The non-generic overload is now explicit —

```csharp
ISlice? ISlice.Append(object[] elems) => ((ISlice)m_value).Append(elems);
```

— which is what golib's own `slice<T>` has always declared (`ISlice ISlice.Append(object[] elems)`
beside `ISlice<T> ISlice<T>.Append(params T[] elems)`), so the wrapper now matches the type it
wraps. The reasoning is the same one the template already applies to `GetEnumerator` in the
subsection above: this is the boxing, interface-typed path, taken only when a consumer asks for the
interface, and the public surface is the typed overload. Converted code never calls it by name —
Go's `append` emits golib's `append` builtin, which reaches `slice<T>.Append` statically.

Measured after: both suites clear this root and stop on unrelated ones — `fmt` on five
(CS1955 `map` used as a method, CS0030 on renamed complex types, CS1729/CS0103/CS0034 around
`Scan_type`), `archive/tar` on the duplicate `global using` alias its board row records as closed
and which is in fact still live. Neither banks. Guarded by the `NamedAnySliceType` behavioral test —
both suites' declarations verbatim, spread into a variadic `...any`, appended to, indexed, ranged,
sub-sliced, spread into a second named `[]any`, and compared against `nil`, output-compared vs
`go run`.

## Every generated named-slice wrapper also implements the non-generic `IArray` surface

Every generated named-slice wrapper also implements the non-generic `IArray` surface explicitly. The public typed `Source` remains `T[]` for the concrete wrapper, but the interface member is emitted as `Array IArray.Source => ((IArray)m_value).Source!;`, matching golib's `IArray.Source` contract and keeping `len(IArray)`, element-address helpers, and interface-typed builtins bound to the wrapper. Pointer elements use the same form, e.g. `type queue []*item` emits `ISlice<ж<item>>` plus the explicit `Array IArray.Source` member. (Guarded by `NamedSlicePointerElements`.)

## Composite types render structurally (`[]*T` keeps the pointer)
A slice/array type is rendered structurally in every type-name path: the `[N]`/`[]` marker plus the recursively resolved element, never from the `go/types` string form. The string form is path-qualified (`[]*internal/abi.Type`), and the cross-package last-segment strip would eat everything before the slash *including the pointer marker*, silently dropping the `ж<>` (reflect's `[]*abi.Type` fields compiled against the WRONG element type). The recursion also resolves lifted anonymous elements and cross-package generic elements:
```go
ptrs := vals.([]*atomic.Int32)
```
```csharp
var ptrs = vals._<slice<ж<atomic.Int32>>>();
```
Guarded by `ArrayOfCrossPackageType` (the type assert and a `var` declaration).

## Appending to an interface-typed slice casts the element
A value appended to a `[]Iface` slice whose type is not already the interface -- a pointer rendering as the `*T`-to-interface adapter ctor, or a raw struct value -- leaves both golib `append` overloads applicable (`append<T>(ISlice, params T[])` infers the concrete/adapter type; `append<T>(slice<T>, params Span<T>)` infers the interface -- CS0121). The converter casts such elements to the element interface type:
```csharp
pack = append(pack, (Animal)(new CatжAnimal(Ꮡ(new Cat(nil)))));
pack = append(pack, (Animal)(new Dog(nil)));
```
An already-interface-typed element stays bare. The **empty interface** (`any`) element type is affected identically and takes the same cast — `append(args[:len(args):len(args)], c.output)` with `args []any` and `c.output []byte` infers `T=[]byte` on the `ISlice` overload but `T=any` on the `slice<T>` overload (testing's `flushToParent`, CS0121), and appending a scalar (`append(anys, 5)`) is the same shape; the differing element is cast to `any` so both overloads agree:
```csharp
args = append(args.slice(-1, len(args), len(args)), (any)(c.output));
```
Guarded by `InterfaceCasting` (non-empty interface) and `AppendUntypedConst` (the empty-interface `[]byte`-into-`[]any` and scalar-into-`[]any` cases).

## A `nil` element appended to a slice casts to the element type
A bare `nil` appended as a single element -- `append(b.lines, nil)` on `[][]cell` -- renders `nil` as `default!`, which C# overload resolution binds to `append`'s `params` parameter as the **whole null array** (a non-expanded params call), appending **ZERO** elements rather than one nil element. This silently no-ops the grow: text/tabwriter's `addLine` never extended `b.lines`, so every `terminateCell` indexing `b.lines[len(b.lines)-1]` panicked with index `[-1]`. The interface (`error`) and named-composite branches above already cast a nil element (its untyped-nil type differs from the element type), but an **unnamed** nillable element type -- slice/map/pointer/chan/func -- matched no branch and emitted bare. The converter now casts any untyped-nil element to the slice's element type, forcing single-element binding:
```csharp
lines = append(lines, (slice<nint>)(default!));       // []int   element
maps  = append(maps,  (map<@string, nint>)(default!)); // map     element
ptrs  = append(ptrs,  (ж<nint>)(nil));                 // pointer element (nil renders in pointer context)
```
A nil element is only ever valid when the element type is nillable, so the cast target always exists. Spread appends (`append(dst, src...)`) are excluded (the existing `Ellipsis.IsValid()` guard). Guarded by the `AppendNilSliceElement` behavioral test (slice/map/pointer element types, output-compared vs Go).

---

[← Multi-Result Values and Comma-Ok Forms](multi-result-and-comma-ok.md) · [Index](README.md) · [Strings (`@string` and `sstring`) →](strings.md)
<!-- {% endraw %} -->
