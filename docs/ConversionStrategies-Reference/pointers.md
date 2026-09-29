# Pointers
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#pointers)
Pointer conversions use the golib heap box [`ж<T>`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/%D0%B6.cs) (read "zhe"). Taking the address of a value uses the address-of operator `Ꮡ` (e.g. `Ꮡx`); an escaping local is allocated via `heap(...)`, and addresses of a struct field or array element are taken through `.of(Type.ᏑField)` / `.at<T>(index)`.

The box's value accessors follow one naming scheme (unified 2026-07-02; the checked accessor was previously `val`): **`Value`** is the strict dereference (`ref`-returning; panics on a nil pointer, as Go does), **`ValueSlot`** is its no-check twin (the identical real slot — for reads/writes of a held value that may legally be nil), and **`DerefOrNull()`** is the null-box-tolerant extension every pointer ENTRY alias binds through (an *extension method* is the only ref-returning form C# permits on a possibly-null receiver; it returns a NULL ref when nil, so the alias binds and the panic lands at the body's own deref). The same `Value` name is used by the generated named-type wrappers for their underlying-value accessor and by the golib `uintptr` struct for its raw word — converted code has exactly one spelling for "the value behind this thing". A Go struct **field** named `val` still emits as `.val` (it is the user's identifier, not the accessor):

```csharp
ref var a = ref heap(new array<@string>(2), out var Ꮡa);  // escaping local
var p = Ꮡa.at<@string>(0);                                 // &a[0]
var pField = Ꮡsettings.of(settingsᴛ1.ᏑRetries);            // &settings.Retries
```

**A heap-boxed *range variable*** needs the box allocated **per iteration**. When a `for i := range s` (or `for _, f := range s`) variable has its address taken, it escapes — but the foreach already declares that name, so a single `ref var i = ref heap(…)` before the loop would clash (CS0136). The converter iterates a *temp* and, inside the body, allocates a fresh box each pass and copies the temp into it:

```go
for i := range s {
    p := &i      // i escapes
    use(p)
}
```
```csharp
foreach (var (iᴛ1, _) in s) {
    ref var i = ref heap(new nint(), out var Ꮡi);   // a FRESH box each iteration
    i = iᴛ1;
    var p = Ꮡi;
    use(p);
}
```

The per-iteration box is required for Go 1.22 loop-variable semantics: each iteration's variable is distinct, so a stored `&i` must point to a different box each pass (`for i := range s { ptrs = append(ptrs, &i) }` yields `0 1 2`, not `2 2 2`). A non-escaping companion variable still declares directly in the foreach. (Guarded by the `RangeVarHeapBox` behavioral test — both a within-iteration `&i` and the stored-pointer distinctness case; runtime exercises it in `for i := range stackpool` and `for _, f := range s.Fields`.) A heap-boxed `for i := …; cond; post` **clause** variable takes the same per-iteration box through its carrier rewrite — see [*For-clause variables are per-iteration*](labels-and-loop-variables.md#for-clause-variables-are-per-iteration-go-122-loop-variable-semantics).

The `at<T>(index)` element-address accessor has `int`, `nint` and `ulong` overloads, and each checks the index at its FULL value against the length before anything narrows it, as Go does (goPanicIndex / goPanicIndexU): an out-of-range index panics with Go's `index out of range [i] with length n`, recoverable. A wide **unsigned** index (`uint`/`uint32`/`uint64`/`uintptr`) takes `(ulong)` and binds the `ulong` overload, so an index at or above 2^63 keeps its value. (A `(nint)` cast read it as negative and reported `[-N]`.) It is a cast rather than bare because the C# expression can be wider than its Go type: `g % 2` for a `uint` g is a C# `long`. Any other non-`int` integer index (`int64`, or a small kind) takes `(nint)`, which loses nothing. An `int` index, or an untyped int constant (which renders as a plain int literal), is emitted as-is:

```csharp
var pi = Ꮡa.at<nint>((ulong)(i));       // &a[i]      where i is a uintptr
var pe = Ꮡa.at<nint>((ulong)(g % 2));   // &a[g%2]    where g is a uint (g%2 widens to long in C#)
```

The field forms `Ꮡp.at(S.Ꮡa, i)` (an array field reached through a pointer) carry the same three overloads. This is the element-address analogue of the indexed-literal key cast (`SparseArray<T>`, above) and the `IBinaryInteger<T>` width-agnostic length params on `unsafe.Add`/`Slice`/`String`. (Guarded by the `ArrayWideIndexAddress` and `PointerElementAtUnsignedIndex` behavioral tests.)

A **string** indexed by a wide/unsigned integer keeps its full value too. An `@string` *variable*'s indexers (`int`, `nint`, `ulong`) each bounds-check before any narrowing, so an unsigned index is emitted bare onto `this[ulong]` and an `int64` takes `(nint)`. A string LITERAL renders as a `ReadOnlySpan<byte>` (`"…"u8`) whose indexer takes `int` and throws a CLR exception `recover()` cannot see, so a NON-constant index on a literal renders as golib's `LiteralByteAt(<literal>, <index>)`, which applies the same check and allocates nothing: runtime `heapdump.go`'s `"0123456789abcdef"[pc&15]`. A constant index is in range by Go's own compile-time check and keeps `"…"u8[i]`. (Guarded by the `ArrayWideIndexAddress` extension, literal and variable string bases with `uintptr`/`uint64` indexes, and by `UnsignedIndexPastIntRange` and `SliceBoundsEscapeRoutes`.)

The address of a **slice** element uses the call form `Ꮡ(slice, index)` (golib overloads `Ꮡ<T>(IArray<T>|slice<T>|array<T>, int|nint|ulong)`) rather than `at<T>`. Every overload checks the index at its full value against the slice's LENGTH, so `&s[5]` on a slice of length 3 and capacity 10 panics as Go does. An unsigned index (`uint`/`uint32`/`uint64`/`uintptr`) is emitted bare onto the `ulong` overload and an `int64` takes `(nint)`. Go `int` (→ `nint`) and the small integer types that implicitly widen to `int` bind directly. (Mirrors the runtime's `&datap.pclntable[funcoff]` / `&filetab[fileoff]`, indexed by `uint32` offsets. Guarded by the `ElementAddressUnsignedIndex` and `SliceBoundsEscapeRoutes` behavioral tests.)

The `Ꮡ(slice, index)` form applies to **any slice-typed base expression**, not just a named slice variable — a method-**call** result (`&b.stk()[0]`, runtime `mprof.go`; `&StringByteSlice(s)[0]`, `syscall`), a builtin/`make` result, an `unsafe.Slice(…)` result (`reflect`), or a **slice-expression** base (`&x[0:cap(x)][cap(x)-1]`, `math/big`). Such bases have no bare identifier, so they previously fell out of the (identifier-gated) slice arm into the *array* branch — a slice's type name also starts with `[` — whose naive fallback textually prefixed `Ꮡ` onto the postfix chain: `Ꮡb.stk().at<uintptr>(0)` binds as `(Ꮡb).stk()…`, referencing a box that does not exist (CS0103), or copy-boxed the slice header (a lost-write latent). The element address of the returned slice **view** reaches the shared backing array per Go aliasing, so a write through the pointer is visible via the original storage. (Guarded by the `NestedFieldElementAddr` extension — `&st.stk()[0]` through a pointer local, write-through vs Go.)

The bounds of a **3-index (full) slice** `s[low:high:max]` lower to the golib `.slice(nint low, nint high, nint max)` method, and a `uintptr`/`uint`/`uint32`/`uint64`/`int64` bound takes `(nint)` (the `castWideIntegerToNint` helper): `stk[:b.nstk:b.nstk]` (b.nstk a `uintptr`) → `stk.slice(-1, (nint)(b.nstk), (nint)(b.nstk))`. A plain `int`/small-int bound is left uncast. `(nint)` rather than `(int)`: an `(int)` cast truncated `s[0:w:w]` at w = 1<<32+5 to `len 5 cap 5`. The three bounds can mix kinds, so they take one nint overload rather than unsigned ones, and an unsigned bound at or above 2^63 panics with a signed bound in its text, a stated residual. (The 2-index range forms `s[lo:hi]` narrow through `getRangeIndexer` for the C# `[..]` range operator; only the 3-index `.slice()` form needed this.) (Guarded by the `Slice3IndexWideBound` behavioral test — `uintptr`/`uint`/`uint64` full-slice bounds on an array and a slice + an int control, values verified vs Go; runtime hits this in `mprof`'s `stk[:b.nstk:b.nstk]`.)

**Address of an element of an array *field* reached through a pointer or boxed struct.** When the array being indexed is a field of a heap-boxed value — `&mp.future[i]` where `mp` is a `*memRecord`, or `&g.future[i]` where `g` is an address-taken global — the *array field's* address goes through the box-field accessor first, then the element index: `Ꮡmp.of(memRecord.Ꮡfuture).at<cycle>(i)` (pointer parameter), `mp.of(...)` (pointer local), `Ꮡg.of(rec.Ꮡfuture).at<cycle>(i)` (boxed global). A naive `Ꮡ` prefix on the field read (`Ꮡ(~mp).future`) instead binds `.future` to the box value `Ꮡ(~mp)` (a `ж<memRecord>`, which has no `future` member) → CS1061. This requires a matching golib detail: `ж<T>.at<TElem>(index)` resolves the array through the `Value` property, **not** the raw `m_val` field — for a field-reference pointer produced by `of(...)`, `m_val` is an empty default and the real array lives behind `Value` (the same resolution `of(...)` itself uses). Reading `m_val` would miss the array → null-deref at runtime even though the C# compiled. (`array<T>` is a readonly struct over a shared backing `T[]`, so the value `Value` yields still aliases the real elements; writes through the returned element pointer land.) (Guarded by the `PointerFieldArrayElementAddress` behavioral test — pointer parameter and pointer local both taking `&p.future[i]` and mutating through it.)

The RECEIVER's own array field is the one case that does **not** take this route: a Go pointer receiver renders as `this ref T recv`, which has no box companion, so `of(...)` would name a box that does not exist (`Ꮡr.of(RegArgs.ᏑInts)`, CS0103). It uses the element-aliasing two-arg `Ꮡ(recv.field, (int)(i))` instead — correct because copying an `array<T>` wrapper shares its backing `T[]`. See *Element address of an ARRAY FIELD of the receiver* under Slices and Arrays for the write-dropping bug this replaced (`compress/flate` losing all LZ77 matching).

## A colliding pointer-adapter name qualifies its FOREIGN interface side

A pointer-interface adapter class is named `[<pkg>_]<structSimple>ж<ifaceSimple>`. The STRUCT side is
package-qualified when foreign (`bytes_ReaderжReader`), which keeps two same-named foreign structs
adapting to one interface apart. The INTERFACE side had no such treatment — it composed from its bare
last-dot segment — so the mirror-image case collided: ONE struct cast to TWO interfaces whose simple
names match composes one class name twice (`CS0102`, `CS0111` per member, `CS8646`).

`compress/flate` is the case that surfaced it. It declares its own `Reader` (`io.Reader` +
`io.ByteReader`), and its tests hand a `*bufio.Reader` and a `*bytes.Reader` to `NewReader`, which
casts to both that and `io.Reader` — so `bufio_ReaderжReader` and `bytes_ReaderжReader` were each
emitted twice and the package could not build its test host at all. The rule is
**collision-conditional**: only within a group of records composing the same name does the interface
side take a package qualifier (`bufio_Readerжio_Reader`), and the LOCAL member of a group keeps the
bare name (at most one member can be local, so that stays unambiguous and preserves the Go-like short
form). Qualifying unconditionally was measured and rejected — 644 distinct adapter names across 3,688
construction sites would churn. The entire 302-package production corpus contains **no** collisions,
so the rule is byte-neutral there by construction; it takes a test closure's extra casts to make one.

Grouping keys on the whole composed name, struct side included. `compress/gzip` records both
`<Reader, io.Reader>` and `<bufio.Reader, flate.Reader>` — two records whose interfaces share a simple
name but whose struct sides differ, composing `ReaderжReader` and `bufio_ReaderжReader`. Keying on the
interface name alone would call that a collision and rename a validated package's adapters for nothing.

The converter and the generator must agree on every name, and neither may guess, so both read the same
authority: the final `[assembly: GoImplement<…>(Pointer = true)]` lines. That set is not known while
cast sites are being rendered — it is settled only after the whole package is visited and
`writePackageInfoFile` has applied its alias-covered skip and its interface-inheritance prune — so a
cast emits a deferred marker (mirroring the DYNTYPE marker of the anonymous-struct barrier) that
`resolveAdapterNameMarkers` rewrites once the records are final. The marker's payload is hex-encoded
for the same reason DYNTYPE's is: a rendered type name passes through string transformation passes
before reaching the file. Only the INTERFACE side is ever rewritten — the struct side is emitted
verbatim, because it is the reference's *path*, not just a name fragment: rewriting it turned
`new os.FileжWriter(f)` (namespace `os`, adapter class `FileжWriter`, generated in os's own assembly)
into a bare `FileжWriter` that resolves nowhere. (Guarded by `AdapterNameInterfaceCollision` — a local
`Reader` and `io.Reader` reached from one `*src`, verified by reverting the fix: `CS0102` + `CS8646`
on `srcжReader`. Unblocked `compress/flate`'s Phase-4 test host.)

The same `Value`-not-`m_val` rule applies to the **dereference operator** `~`. A value read through a pointer — `(~c).field`, the form the converter emits for `c.field` where `c` is a `*T` — must resolve through `Value`. For a *field-reference* pointer (`c := &b.w` → `Ꮡb.of(box.Ꮡw)`) or an array-element pointer, the real storage lives behind `Value` and `m_val` is an empty default, so `operator ~` returning `m_val` would read a **zero-valued copy** (`(~c).a` → `0`) — it compiles but is silently wrong. `ж<T>.operator ~` therefore returns `value.Value` (which resolves struct-field / array-element references and, for a standard pointer, is exactly `m_val`), matching the `IPointer<T>.operator ~` that already did. This surfaced when a defined-type-over-struct's forwarded fields were read back through a `*wrapper`, but it is general to any `*x.field` value read. (Guarded by the `NamedTypeOverStruct` behavioral test's read-back path.)

The `at<E>(i)` **element type `E` is rendered fully-qualified** — `at<sync.atomic_package.Int32>`, not the file-local alias `at<atomic.Int32>`. A namespace-rooted type resolves inside `namespace go;` without any `using <pkg>` alias, whereas the alias form needs the file to import that package. A file can index a cross-package-typed array field of a struct without ever naming the element type (so Go requires no import, and the converter emits no `using atomic`), which would leave the alias unresolved (CS0246, e.g. runtime's `tracecpu.go` indexing `trace.cpuLogWrite`). A current-package or basic element renders identically either way, so this is churn-free. (Guarded by the `ArrayOfCrossPackageType` behavioral test's `&x.c[i]` element-address case.)

Using `ж<T>` rather than the C# `ref` keyword avoids the escape-analysis complications of passing a `ref` into code that expects a heap-allocated pointer. This is a simplification that can cost an unnecessary heap allocation when an address is taken; a future escape-analysis pass could keep such values on the stack when it is provably safe, similar to how [Go does this](https://golang.org/doc/faq#stack_or_heap) at compile time.

> Note: a package-level global whose address is taken is backed by a real heap box so that writes through `&global` (and `&global.field`) are observed, rather than mutating a copy.

## Pointer equality canonicalizes the STORAGE, not the referent — slice/array element identity

Go compares pointers by **address**: `unsafe.StringData(s) == unsafe.StringData(t)` is true whenever both strings share the same backing data (a header copy `t := s`, or `strings.Map`'s identity fast path returning `s` unchanged — strings' `TestMap` asserts exactly that). `ж<T>.Equals` already models address identity per referent shape — struct-field refs compare (source, field-identity), array-index refs compare (backing, index), heap boxes compare wrapped-object identity — but the array-index arm compared the `IArray` *instance*, and `@string.buffer` materializes a **fresh `PinnedBuffer` view per access**, so two `StringData` results over the very same bytes never compared equal ("unexpected copy during identity map"). The array-index arm (and the matching `GetHashCode`) now canonicalizes a `PinnedBuffer` to the object its `GCHandle` pins (`PinnedTarget`, normally the string's backing `byte[]`) before the reference comparison, so equal addresses compare equal while everything previously-equal stays equal — the canonicalization only ever *adds* true results for same-storage-same-index pairs, and distinct-but-equal arrays still compare unequal (Go pointer semantics, never value comparison). `strings.Map`'s fast path needed no change at all: it already returned `s`, sharing the backing array through the `@string` struct copy — only the identity *comparison* was blind. (Guarded by the `StringDataIdentity` behavioral output test — header-copy identity true, repeated-call identity true, a runtime copy false, content equality unaffected; before the fix the two identity cases printed `false`.)

**The same class covers ORDINARY slice and array element pointers, which were the larger miss (2026-07-24).** `Ꮡ(target, index)` takes an `IArray<T>`, an INTERFACE, so passing a `slice<T>` — a HEADER (backing array + low + len + cap) over storage it merely *shares* — **boxes the header struct afresh on every call**. The array-index arm compared those boxes, so `&s[0] == &s[0]` was **false**: Go's most basic pointer identity, violated for every slice element. The blast radius is wider than comparison alone, because `GetHashCode` canonicalized the same way: two aliasing element pointers landed in different `map[*T]` buckets, so `m[&s[2]] = "two"` then `m[&s[2]] = "TWO"` *added a second entry* instead of overwriting, and `m[&s[2]]` read back the zero value. `array<T>` (and the generated named-array wrappers) had the identical problem, reached through `ж<T>.at` (`&a[i]` on a boxed array), which likewise boxes a copy of the wrapper struct.

`CanonicalStorage(IArray)` is now `CanonicalElement(IArray, index)`, returning the **actual storage object plus the ABSOLUTE element index within it** — the referent as stored is never the storage itself:

| Referent | Canonical storage | Index |
|---|---|---|
| `PinnedBuffer` (per-access view) | the object its `GCHandle` pins (`PinnedTarget`) | unchanged |
| `slice<T>` | its backing `T[]` (`m_array`) | `Low + index` |
| a named slice type | the backing of the `slice<T>` its full-window interface sub-slice hands back | `Low + index` |
| `array<T>`, named-array wrapper | the raw backing via non-generic `IArray.Source` | unchanged |
| any foreign `IArray` | the referent itself (prior behavior) | unchanged |

Folding the window offset into the index is what makes every Go alias of one element compare equal, not just the same-header case: a re-slice (`&s[1:][0] == &s[1]`), a re-slice of a re-slice, an in-capacity `append` result (`&append(s[:0:1], 9)[0] == &s[0]`), and a slice over an array (`&a[:][i] == &a[i]`) all reduce to the same `(T[], absolute index)` pair. A named slice type wraps a `slice<T>` it does not expose, so it is unwrapped via `view.Slice(0, view.Length)` — the same trick `slice<T>`'s `ISlice<T>` constructor uses; `Source` cannot serve, because a slice header's `Source` deliberately materializes a **detached copy** (only `array<T>`'s is raw).

Canonicalizing `array<T>` to its backing is sound precisely because Go's by-value array COPY is emitted as golib's `.Clone()` (see [Slices and Arrays](slices-and-arrays.md#slices-and-arrays)), giving the copy its own backing — two distinct Go arrays can never canonicalize to the same storage. Like the `PinnedBuffer` precedent, the change only ever *adds* true results for same-storage-same-index pairs: distinct backings still compare unequal (`&z[0] != &s[0]`), distinct indices still compare unequal (`&s[0] != &s[1]`), and the struct-field and heap-box arms are untouched. This is a golib-only change — no emitted-code difference.

(Guarded by the `SlicePointerIdentity` behavioral output test — self identity, distinctness, re-slice and re-slice-of-re-slice aliasing, the in-capacity `append` result, array-vs-slice-over-that-array, struct elements, a write through an element pointer observed through both views, and `map[*int]` store/overwrite/lookup/miss including a lookup keyed through a *different* window — vs `go run`. Counter-proven: pre-fix **every** identity assertion printed `false`, both map lookups returned empty, and `len(m)` grew from 2 to 3 on the overwrite.)

The same "a box is a temporary, the storage is the object" reasoning answers **lifetime** questions —
when the referent dies, and whether two boxes name the same allocation — which `runtime.SetFinalizer`
and `sync.Cond`'s copy detector both depend on, and which is also why `Ꮡ(IArray<T>, index)` must take
its target **by value**: see
[A pointer's REFERENT, not its box, answers every lifetime and identity question](manual-conversions.md#a-pointers-referent-not-its-box-answers-every-lifetime-and-identity-question).

## A pointer's nilness and identity are STRUCTURAL — the `IsNull` / `IsNilPointer` split

`ж<T>` answers two different questions that a single predicate used to conflate, and the conflation was a defect *class*: `IsNull` was `m_isNull || m_val is null`, so it reported **true** for three unrelated things —

1. **THE nil pointer** (`m_isNull`: nil-constructed, or the canonical `NilBox`) — the only one that is actually nil;
2. **a real address whose reference-typed pointee is legitimately nil** — `&i` with `i == nil`, `new(any)`, a closure-captured `p *T` local (`ж<ж<T>>`). In Go these are ordinary non-nil addresses, and `*p` yields the nil *value* rather than panicking;
3. **a struct-field or array-element reference box** over a reference-typed `T` — `&s.next`, `&elems[i]` — whose storage lives in the referenced struct/array, leaving `m_val` an unused `default` that reads as null for any reference type. A perfectly valid address, misread as nil purely because of where its storage is.

`IsNilPointer` (`m_isNull`) is the STRUCTURAL predicate, and everything about pointer **identity** keys off it — equality, `GetHashCode`, `PointerOrderToken`, the reflection bridge's `IsNil`/`Elem`, and the dereference guard on `operator ~`. `IsNull` keeps only case 2 (case 3 is now excluded structurally, alongside the native-address exclusion added earlier for the same reason), and is consulted only where peeking at the value is the actual question: the strict `Value` getter, `PinnedBuffer`, and the `uintptr`/`void*` address conversions — where a reference-typed pointee has no reportable address at all, so `0`/`null` is the only answer the managed model can give, and for the value-typed pointees that actually cross into native code the two predicates coincide (`unsafe.Pointer`'s pointee is `uintptr`, so every `IsNull` in that class *is* the structural question).

Fixed consumers: `operator ~` (both the `ж<T>` and the `IPointer<T>` interface form) guard on the structural predicate and read `ValueSlot`, so `*p` on a real address whose pointee is nil yields nil — and a *field-reference* deref reads the field instead of throwing; `DerefOrNil`/`IsNilStandardPointer` likewise, so the pointer-walk re-alias hands back the **real** slot (a write through it persists) rather than the throwaway; and the reflection bridge's interface-routed slot read (`GoReflect.readSlotViaInterface`) and `deepValueEqual`'s cycle-detection `identityRoot` ask `INilPointer.IsNilPointer` instead of the value-peeking property — the latter previously dropped `&i`-shaped values out of cycle detection entirely.

**Two identity rules changed with it.** (a) A standard heap box's identity was formerly *derived from the value it held* whenever `T` was a reference type — two distinct boxes wrapping one referent compared **equal**. That reported `&c == &d` **true** for two distinct `*int` variables holding the same pointer, collapsed `map[**int]V{&c: …, &d: …}` into a **single** entry, and made a pointer's **hash mutate when its pointee was assigned**, so a key inserted while its pointee was nil could never be found again (`m[q]` read back the zero value while `len(m)` still said 1). A pointer's identity is its storage: a standard box *is* the storage, so it hashes and compares by its own identity, and `&x == &x` holds because an addressed Go variable is heap-boxed **once**. (b) Conversely, two boxes **aliasing the same native address** (`m_nativeAddr`) are now the same pointer — a `uintptr` round-trip mints a fresh box each time, and Go requires `(*T)(unsafe.Pointer(p)) == (*T)(unsafe.Pointer(p))`.

golib-only change — no emitted-code difference. (Guarded two ways, because the converter routes every reference-typed-pointee deref through `.ValueSlot` — verified with a 6-shape probe including generics — so `operator ~` is unreachable from converted Go and only golib-internal, hand-owned and reflection-bridge code takes it. The Go-expressible half is in the `PointerToNilPointerIdentity` behavioral output test: distinct-variable identity, `map[**int]` two-key distinctness, hash stability across a pointee assignment, and reference-typed field-reference deref + map keying — pre-fix `&c == &d` printed `true`, the two-key map held **1** entry, and the stable-key lookup read back empty. The unreachable half is in `GolibTests.PointerNilPredicateTests`, which drives `operator ~` (both forms), `DerefOrNil`, `ReadPointerSlot` through a hand-written stand-in for a generated named-pointer wrapper, and native-alias identity — 7 of its 10 assertions fail pre-fix.)

## Reading a pointer and taking a field pointer allocate NOTHING — the two costs hidden inside `ж<T>`

Go's `*p` and `&x.f` are free. Both allocated in go2cs, silently — the code was correct, it merely paid — and the bill was visible only where something counted it. `os.TestWriteStringAlloc` bounds `f.WriteString(s)` at **zero** allocations; the measured cost was over nine thousand bytes per call (**9,184** through the test pipeline, **9,208** under the standalone probe, which writes to its own file rather than the host's `t.TempDir()` one), and a byte-exact decomposition of the probe's number (markers around every frame of `WriteString → File.Write → poll.FD.Write → syscall.Write`, arithmetic closing to the byte) put **5,728 of it — 62 %** in these two places, not in the defer machinery that was the standing suspicion (the frame for that shape is 192 bytes, near 2 %).

**1. `IsNull` boxed the whole pointee on every dereference — 4,760 bytes (52 %).** `Value`'s standard-box branch guards on `IsNull`, whose last term is the value-peeking `m_val is null` (case 2 of the split above — a real address whose reference-typed pointee is legitimately nil). On an **unconstrained** type parameter `is null` compiles to `box !T; ldnull; ceq`, so a term that is constant-false for every struct `T` still allocated *and memcpy'd* a full copy of the pointee, on every read. A pointer to a large record paid its own size per dereference: `os.file` is 592 bytes, and the write path walks eight `of()` links, each bottoming out in one of these. The term is now guarded by a per-`T` `s_valueCanBeNull` (`!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null`), computed from the type rather than by boxing `default(T)`, so type initialization allocates nothing either. The guard also let the peek read the RIGHT storage: a `T` containing no references keeps its value in the pinnable `m_slot` and leaves `m_val` the unused default, so `m_val is null` answered for the wrong slot and every `ж<Nullable<T>>` reported nil whatever it held — unreachable from converted code (Go has no `Nullable`), and wrong, so it is corrected alongside.

**2. `of(…)` minted the untyped accessor wrapper per CALL — 968 bytes (11 %).** `of<TElem>` stores an `object`-taking wrapper around the typed field accessor. The wrapper closes over nothing but that accessor, so it is a pure function OF it — and the accessor is a static method group, which the compiler already caches to a singleton. Minting the wrapper per call therefore bought a fresh display class plus a fresh delegate (88 bytes) for a value identical every time, on every `&x.field` in the corpus. It is now memoized per accessor in a `ConditionalWeakTable`; the keys are weak, so an accessor that is genuinely per-call leaves no permanent entry. Pointer equality is unaffected — it compares the field IDENTITY token (the original accessor), which is what made the per-call wrapper tolerable in the first place.

Together these take `os.File.WriteString` from **9,208 to 3,168 bytes per call (−65.6 %)** — probe and pipeline agreeing to the byte afterwards, the test now printing `expected 0 allocs for File.WriteString, got 3168` — and the same two costs were being paid by every pointer read and every field address in every converted package. The row still does not reach zero — the remainder is the `ж<T>` boxes themselves (1,488 B, of which 608 is one `ж<FD>` whose inline `m_val` slot a field reference never uses), the syscall seam's `unsafe.Pointer`/`heap` boxes (784 B), the defer machinery (192 B — the display class and delegate of each capturing defer) and the `unsafe.StringData` pin (136 B) — inherent to the current pointer and defer models rather than waste inside them. The arc for those is recorded in [`docs/phase4/BOARD-next-validation-candidates.md`](../phase4/BOARD-next-validation-candidates.md).

golib-only change — no emitted-code difference. (Guarded by `GolibTests.PointerDereferenceAllocationTests`: four measured-byte assertions plus a semantics pair. With the fixes neutered they report 528 B/deref for a 512-byte pointee, 288 B/deref for a reference-bearing one, 32 B/deref through a field-pointer chain, and 200-vs-112 B/call for `of(…)` against a bare box of the same type — the last stated as a COMPARISON rather than a byte count so it survives any future change to `ж<T>`'s layout.)

## A pointer parameter whose every use is a dereference is a `ref` parameter — the ж-box ref-lowering

The emitted-form rule (stage A2 of [`docs/phase4/DESIGN-zh-box-reduction.md`](../phase4/DESIGN-zh-box-reduction.md), rulings §10.1/§10.3/§10.4): an **unexported package-level function's** pointer parameter whose every body use is a dereference (`*p`, `p.f`, `p[i]`, `range p`), a derived address feeding another lowered position (`&p.f` → a lowered argument), or a forward into another lowered position, emits as a C# **`ref T` parameter** instead of the boxed `ж<T>` — and every call site passes a `ref` expression instead of minting or carrying a box. A `ref T` argument is an alias into the caller's storage the GC tracks and updates, so no pinning, no box, no allocation, and writes through it land in the caller's storage by construction. The signature reads as Go's `*T`, `ref` reads as Go's `&`, and the entry deref preamble disappears because the parameter *is* the alias:

```go
func p224Sub(out1, arg1, arg2 *p224MontgomeryDomainFieldElement) { ... }
p224Sub(&e.x, &t1.x, &t2.x)
```

```csharp
internal static void p224Sub(ref p224MontgomeryDomainFieldElement out1, ref p224MontgomeryDomainFieldElement arg1, ref p224MontgomeryDomainFieldElement arg2) { ... }
p224Sub(ref nonnil(ref e).x, ref nonnil(ref t1).x, ref nonnil(ref t2).x);
```

**What disqualifies (the whitelist argument — any use the classifier does not positively recognize keeps the box):** pointer identity or nilness (`p == nil`, map keys), escapes (returned, stored, captured by a closure or a defer/go argument frame), representation observations (`unsafe.Pointer(p)`, `uintptr(p)`, interface conversions, a method call ON `p`), re-pointing (`p = q`), and function-identity escapes (exported [Phase A], func-value uses, `//go:linkname` registry membership, named pointer types, bodiless assembly stubs, declaration in — or a curated call from — a `[module: GoManualConversion]` hand-owned file). Blank/unnamed pointer parameters are never candidates (no uses, nothing to gain, and the boxed path owns the synthesized-name conventions). The fixed point is two-sided: a call site whose argument shape has no `ref` emission row (including the tuple-splat `f(g())` form) strips the position rather than dead-ending emission.

**The call-site emission rows** (each self-checks and falls back to today's boxed emission wrapped `.DerefOrNull()` — total over classifier-admitted shapes without coverage ever being a soundness premise):

| Go argument | lowered emission |
|:--|:--|
| `&e.x` / `&p[i]` (base is a pointer's deref alias or a lowered `ref` param) | `ref nonnil(ref e).x` / `ref nonnil(ref p)[i]` |
| `&x.f` / `&s[i]` (value-rooted base: local, value param, global, slice) | `ref x.f` / `ref s[i]` — `nonnil` elided, the base cannot be null |
| `&x` (address-taken local/param/result — reverted or kept-box) | `ref x` (the plain local, or the entry ref alias into the surviving box — same storage either way) |
| a pointer variable/field/deref/assert (carries a box) | `ref (q).DerefOrNull()` — reads the box at CALL time, so a re-pointed pointer is never stale |
| a lowered `ref` parameter forwarded | `ref p` — it already is the ref |
| `(*T2)(&v.x)` (the named-array-wrapper reinterpret — §10.3's hoisted-temp rule) | `var ᴛ1 = nonnil(ref v).x.Value;` … `ref ᴛ1` — the wrapper's `Value` yields its `array<T>` header, a copy whose `T[]` backing is SHARED, so element writes flow through and whole-header writes are lost in both emissions equally (byte-parity with the old `Ꮡ((Ꮡv.of(…)).Value.Value)` form). Go requires identical underlying types for pointer conversions, so the wrapper family closes under `.Value` reads and single user-defined conversions; anything else (e.g. a named-SLICE reinterpret) keeps the boxed fallback |
| `&T{…}` composite literal | `var ᴛ1 = new T(…);` … `ref ᴛ1` — observationally identical to a distinct heap box, since a lowered callee can never compare, store, escape or convert the address |
| the literal `nil` | `ref ((ж<T>)default!).DerefOrNull()` — binds the null ref; the callee's first use faults with Go's panic |

**Address-taken locals revert for free.** A local (or value parameter, or named result) whose EVERY address-connected use feeds a lowered position — directly, outside defer/go, outside any closure — loses its `heap()` box entirely: the declaration reverts to a plain local, removing **two** counted objects per unmanaged local (the box and its eager pinnable slot). Any surviving box use (a stored address, a closure crossing, a pointer-receiver method) keeps the box, and the lowered sites alias the same storage through the entry ref alias. The reversion also collapses the per-iteration loop-variable boxing scaffold where the loop var's address only feeds lowered positions (`ForVariants`).

**The nil doctrine (ruling §10.4).** Go panics eagerly at `&e.x` when `e` is nil — before the callee is entered. A naive null byref would instead let the callee run side effects Go never runs and let a callee `recover` catch a panic it can never catch (the design review's S-F1 third behavior). Lowered field/element address formation over a *nullable* base (a pointer's deref alias — null exactly when the pointer is nil) is therefore eagerly checked by golib's `nonnil(ref e)` — one branch, zero allocation, throwing the exact panic `ж<T>.Value` raises — and elided where the base provably cannot be null (a value local/parameter/result, an addressed global's ref property). A plain nil pointer ARGUMENT (`f(q)` with nil `q`) still enters the callee and faults at first use, exactly as Go. Measured gc subtlety recorded with the guard: Go evaluates sibling function CALLS among the arguments in lexical order *before* non-call operands like `&e.x`, so "later arguments unevaluated" holds only for non-call operands.

**`defer f(&x)` / `go f(&x)` are boxed sites, categorically.** The defer/go machinery stores eagerly-evaluated argument values in a frame, and a managed `ref` cannot be stored there (the compiling alternative — a copy-box — silently loses writes: the panel's 0-vs-7 refutation). The eager arguments keep the boxed emission, the statement always takes the temp-param lambda form (a `ref`-parameter method group cannot convert to `Action<…>`), and the thunk derives each ref at invoke time: `defer(ᴛ1 => setErr(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ);` — preserving Go's defer-time argument evaluation. An address flowing to a lowered position under defer/go keeps its box (the locals carve-out), and an address-carrying use of a candidate's OWN parameter inside a defer/go argument frame vetoes that parameter (the X2-defer-arg mirror).

**Determinism across emissions:** classification reads only the production package's own files — never `_test.go` — so the `-stdlib` and `-tests` emissions of production sources agree by construction (a white-box `export_test.go` func-value alias cannot un-lower what `-stdlib` lowered; unit-guarded).

Landed measured effect on the flagship: `crypto/internal/nistec/fiat` transpiles with **zero** `heap(` sites and **zero** `.of(` sites (was 158 address-taken locals and 56 field-ref argument feeds), per the design's §3.6 projection. (Guarded by the `RefLoweredParams` behavioral test — write-through, forwarding chains, the mixed kept/reverted local, the defer/go carve-out in all three observable directions, the X5 func-value exclusion — and `RefLoweredNilTiming` — the eager-panic differential in three nil spellings plus the deferred-fault half, all output-compared against `go run`. The classifier and its fixed point are unit-guarded in `refLoweringAnalysis_test.go`; the corpus-wide census instrument is `-ref-census`.)

## The lowered emission, row by row — the seven argument shapes in emitted code

The seven rows of [`DESIGN-zh-box-reduction.md`](../phase4/DESIGN-zh-box-reduction.md) §3.3, each with its emitted form and the golden that pins it. Every snippet is verbatim from a committed `.cs.target`, quoted through [`EXEMPLARS-a2-ref-lowering.md`](../phase4/EXEMPLARS-a2-ref-lowering.md) — which carries the before/after pair and the history for each; only the *current* form is stated here.

| # | Go argument | boxed emission | lowered emission | golden |
|:-:|:--|:--|:--|:--|
| 1 | `&e.x` — field of a deref'd parameter or receiver (a **nullable** base) | `Ꮡe.of(T.Ꮡx)` — 1 box | `ref nonnil(ref e).x` | `RefLoweredParams`, `GenericReceiverFieldAddress` |
| 2 | `&x` — an address-taken local, value parameter or named result | `Ꮡx`, the `heap()` box minted at the declaration | `ref x` — the plain local; the box and its eager `T[1]` slot are gone | `ForVariants` |
| 3 | a pointer variable/field/deref/assert `q` — it carries a box | `q` | `ref (q).DerefOrNull()` — read at CALL time, so a re-pointed pointer is never stale | `PointerParamNilWalk`, `PointerFieldArrayElementAddress` |
| 4 | `&s[i]` / `&x.f` over a **value-rooted** base (local, value param, global, slice) | `Ꮡ(s, i)` — 1 box + 1 interface temp; `Ꮡx.of(T.Ꮡf)` for the field form | `ref s[i]` / `ref x.f` — `nonnil` elided, the base provably cannot be null | `AddressOfParamWrite`, `PointerFieldArrayElementAddress` |
| 5 | `(*T2)(&v.x)` — a pointer conversion over a `[GoType]` named-**array** wrapper | `Ꮡ((Ꮡv.of(…)).Value.Value)` — 2 boxes | hoisted temp: `var ᴛ1 = v.x.Value;` … `ref ᴛ1` | `NamedArrayWrapper` |
| 6 | a non-variable pointer expression — `&T{…}`, `new(T)`, any call result | `Ꮡ(new T(…))` / carries the returned box | hoisted temp, same shape as row 5: `var ᴛ1 = new T(…);` … `ref ᴛ1` | `RefLoweredParams` |
| 7 | the literal `nil` | `default!` | `ref ((ж<T>)default!).DerefOrNull()` — binds the null ref; the callee faults at first use | `GuardedNilPointerParamDeref` |

A lowered parameter forwarded into another lowered position is `ref p` — it already is the ref. Rows 5–7 share one justification: a lowered callee can never compare, store, escape or convert the address, so a caller-side temporary is observationally identical to a distinct heap box.

**Row 1 — the parameter *is* the alias, and it survives generic instantiation** (`GenericReceiverFieldAddress`; the callee's `ж<T> Ꮡp` box and its `DerefOrNull()` preamble are gone, and the caller's 128-byte-per-evaluation field box becomes free):

```csharp
internal static void setT<T>(ref T p, T val) {
    p = val;
}

public static void Set<T>(this ж<Box<T>> Ꮡb, T val) {
    ref var b = ref Ꮡb.DerefOrNull();

    setT(ref nonnil(ref b).v, val);
}
```

**Row 2 — an address-taken local comes home from the heap** (`ForVariants`; two counted objects per unmanaged local removed, and the per-iteration boxing scaffold of a labeled loop collapses to one plain loop variable):

```csharp
nint i = 0;
while (i < 10) {
    f(ref i);
    i++;
}
internal static void f(ref nint y) {
    fmt.Print(y);
}
```

**Row 3 — the callee lowers, the call site unwraps** (`PointerFieldArrayElementAddress`; `c` comes from `.at(…)` indexing and so still carries a box — each function makes its own deal and the convention change composes across the boundary):

```csharp
internal static void bump(ref cycle c) {
    c.n++;
}
internal static void viaParam(ж<rec> Ꮡp, nint i) {
    var c = Ꮡp.at(rec.Ꮡfuture, i);
    bump(ref (c).DerefOrNull());
}
```

The same row, dereferenced per call rather than bound once, is what keeps a **reassigned** pointer honest (`PointerParamNilWalk`, whose walk loop emits `advance(ref (Ꮡp).DerefOrNull())`); note also what does *not* lower there — a pointer escaping through a **return** keeps its box identity, so `advance`'s `(ж<node>, nint)` result is unchanged.

**Row 5 — two boxes become one temp** (`NamedArrayWrapper`; the wrapper's `Value` yields an `array<T>` header whose `T[]` backing is SHARED, so element writes flow through and whole-header writes are lost in *both* emissions equally — byte-parity, not a new behavior. Type-gated by `refConvPairingSupported` to the identical-underlying-array family; a string or numeric wrapper's value is a plain copy and keeps its identity box end to end):

```csharp
scal sm = new();
var ᴛ1 = sm.s.Value;
fromBytes(ref ᴛ1, 7);
var ᴛ2 = (nonMont)((sm.s).Value);
@double(ref sm.s, ref ᴛ2);
```

**Row 7 — a lowered parameter still accepts Go's `nil`** (`GuardedNilPointerParamDeref`; the synthesized argument binds a null box and defers the fault to the first actual use inside the callee, which is Go's "a nil pointer only panics when dereferenced" timing. `RefLoweredNilTiming` pins it against `go run`):

```csharp
internal static nint digits(nint @base, ref nint invalid) {
    ...
}
nint c2 = digits(10, ref ((ж<nint>)default!).DerefOrNull());
```

**The counter-examples are guarded beside the lowered ones** (`RefLoweredParams`), so the boundary is itself under test: a parameter compared to `nil` keeps its box (its *identity* is observed); one used as a func value keeps it (a method group cannot close over a `ref`); a `defer`/`go` site keeps it and derives the ref at invoke time (`defer(ᴛ1 => bump(ref ᴛ1.DerefOrNull()), Ꮡresult, ref ᒐ);`); in-lambda call sites are uniformly boxed-fallback wrapped `.DerefOrNull()`; string/numeric wrapper reinterprets sit outside row 5's family; and a blank or unnamed pointer parameter is never a candidate.

## Pointer-typed globals and double-pointer walks (`&head`, `*pp`, `ValueSlot`)
A package-level global of **pointer type** whose address is taken — `var head *node` with `pp := &head` — is heap-boxed like any addressed global, yielding a **double box**: `ж<ж<node>> Ꮡhead`. Three rules make the classic linked-list walk (`for pp := &head; *pp != nil; pp = &(*pp).next { … *pp = n }`) faithful:

1. **One star is ONE deref.** `*pp` on a `**T` yields a `*T` — a single `.Value`/`.ValueSlot` hop, never two. (An older arm added an extra `.Value` per pointer *depth*, double-dereferencing every single-star of a double-pointer field — runtime `mheap.go`'s `specialsIter` walk failed CS0029 in both assignment directions.) A genuine `**pp` is two nested `StarExpr`s, each contributing its own hop. Likewise a *field read through an explicit single star* on a `**T` — `(*outer.ptr).Value` — keeps the base pointer-typed after one star, so normal pointer-base field handling supplies the remaining auto-deref: `(~(outer.ptr.Value)).Value`.
2. **A deref whose *result* is still reference-like reads `ValueSlot`, not `Value`.** Go's `*pp` may legally yield nil (`*pp != nil` is the loop condition); only *dereferencing* that nil panics. golib's strict `Value` accessor nil-checks the slot, so a deref (or boxed-global property) producing a pointer/slice/map/chan/func/interface value routes through `ж<T>.ValueSlot` — the **identical real slot with no nil check** — and reads *and writes* both persist: `pp.ValueSlot = n` lands in the original global storage. A deref producing a plain **value** keeps the strict `Value` (a nil `*node` deref must panic, as in Go). The boxed global's ref-property follows the same split: `internal static ref ж<node> head => ref Ꮡhead.ValueSlot;` for the pointer-typed global, `=> ref Ꮡg.Value;` for a value-typed one.
3. **`&global` on an addressed global is the identity box, never a copy.** `&allm` (where `var allm *m` is boxed) emits `Ꮡallm` — the existing box — not `Ꮡ(allm)`, which would heap-allocate a *copy* and silently disconnect writes. And `&(*pprev).alllink` (address of a field behind one explicit star) peels the star and goes through the field-box accessor: `pprev.Value.of(m.Ꮡalllink)`.

The full emitted walk:

```csharp
internal static ж<ж<node>> Ꮡhead = new(default(ж<node>));
internal static ref ж<node> head => ref Ꮡhead.ValueSlot;

for (var pp = Ꮡhead; pp.ValueSlot != nil; pp = (pp.ValueSlot).of(node.Ꮡnext)) {
    if ((~(pp.ValueSlot)).val == v) {
        pp.ValueSlot = (pp.ValueSlot).Value.next;   // *pp = (*pp).next — write lands in real storage
        ...
```

This is exactly the runtime's `allm`/`itabTable` shape (`for pprev := &allm; *pprev != nil; pprev = &(*pprev).alllink`). (Guarded by the `GlobalPointerWalk` behavioral test — ordered insertion, head/middle removal, and a method call through the pointer global, all via `**node` writes, output-compared against Go.)

## Capturing the address of a heap-boxed local in a closure
A local whose address is taken (`&m`) is heap-boxed: the converter emits `ref var m = ref heap(new T(), out var Ꮡm)`, where `Ꮡm` is the box and `m` is a `ref`-local alias of `Ꮡm.Value`. When a **function literal captures such a local and takes its address inside the closure**, the variable must be referenced through the box, not snapshot-copied. A C# `ref`-local cannot be captured by a lambda (CS8175), and the older snapshot capture (`var mʗ1 = m;`) is wrong twice over: it copies the *value* out of the box (so writes through the captured `&m` are lost), and the copy declaration is a statement that has nowhere valid to land when the literal sits in an expression position — e.g. a func literal passed as a **call argument** (`run(func(){ use(&m) })`) or a local initializer (`f := func(){ use(&m) }`).

The fix: a heap-boxed local whose address is taken inside a lambda is marked *box-ref* and the snapshot is suppressed. The box `Ꮡm` is a plain local (a capturable reference), so the C# closure captures it by reference — matching Go's capture-by-reference semantics. Inside the closure the converter then renders every form through the box:

```csharp
ref var m = ref heap(new box(), out var Ꮡm);
run(() => {
    set(Ꮡm);                       // &m  → Ꮡm
    Ꮡm.Value.y = Ꮡm.Value.x + 1;       // value use of m → Ꮡm.Value
});
// &m.field (value struct field) → Ꮡm.of(box.Ꮡfield)
```

This also covers `&m.field` (a value-struct field address inside the closure: `Ꮡm.of(box.Ꮡfield)`). The detection is scoped to the bare `&m` and value-struct `&m.field` forms (the ones with a box-ref emission form); an element address `&m[i]` keeps the existing snapshot path. The behavioral test `FuncLitArgCapture` guards the call-argument, value-use, field-address, and initializer cases.

## A capture that is WRITTEN after the capture point routes to shared storage, not a snapshot
Go closures share the ONE variable with the enclosing function. The value snapshot the converter uses for captured structs/arrays/slices/maps/chans (`var tʗ1 = t;` hoisted before the lambda, in-lambda references renamed to `tʗ1`) is therefore only *observationally* correct while **neither side writes the variable after the snapshot point**. Once anything does, the snapshot silently diverges — the program compiles and runs, with wrong values:

- a closure's writes land in a divorced copy: `bump := func() { t.total += 100 }` never affects `t` (probe-proven: Go 106, snapshot 6);
- body writes after the lambda's creation are invisible to a read-only closure: `get := func() int { return t.total }; t.total += 10` reads the stale copy (Go 15, snapshot 5);
- a **deferred** literal observes the variable's registration-time value instead of Go's final value (`defer func(){ fmt.Println(t.total) }(); t.total = 42` — Go 42, snapshot 5);
- two closures over one variable each get their own copy, so a writer and a reader stop communicating entirely.

The converter now detects **written-after-capture** per variable during analysis (`varShareFacts`, one cached scan of the enclosing declaration) and routes such captures to shared storage. Writes counted, conservatively by syntax: an assignment or `++`/`--` whose target roots at the variable's own storage (`t = …`, `t.f.g = …`, array `a[i] = …` — but *not* through a deref, an implicit pointer deref `p.f = …`, or a slice/map element, which a snapshot copy shares anyway); a pointer-receiver method call on the variable held as a value (Go's implicit `&t`); a `for t = range` clause; an explicit `&t` anywhere or an uncalled pointer-receiver method value (an **alias** — later writes through it are syntactically invisible, so it counts at any position, e.g. `p := &t; get := func(){…}; p.total = 50`); and any of these inside any func literal (the literal may run at any time). A plain body write counts only if positioned after a referencing literal or sharing a `for`/`range` loop with one (a later iteration's write follows an earlier iteration's creation).

The routing, by variable shape:

```csharp
// Heap-boxed variable (escaping struct local, aliased int, …) → by-box (boxRefVars):
ref var t = ref heap<Tally>(out var Ꮡt);
t = new Tally(5, "s");
var bump = () => {
    Ꮡt.Value.total += 100;      // value use → Ꮡt.Value: writes the ONE box the body reads
};

// Unboxed variable (value parameter; slice/map/chan local, whose copy diverges on
// reassignment) → NATIVE C# capture — no snapshot, no rename; the display class
// shares the local exactly as Go shares the variable:
internal static void probeB1(Tally t) {
    var bump = () => {
        t.total += 100;          // captures the parameter itself
    };
    bump();
    t.total++;                   // 106, matching Go
```

This applies only to genuine **closure-body** references (a func literal's body, directly or as a go/defer statement's literal callee). A go/defer statement's non-literal callee/receiver expression and its call arguments keep their statement-time evaluation — `defer fmt.Println(t.total)` still prints the registration-time value, which IS Go's argument semantics. Read-only-after-capture variables keep the snapshot (observationally identical, zero churn — the vast majority of stdlib captures). A **loop-statement-defined** variable (for-init or range clause) also keeps it: its per-lambda snapshot approximates Go 1.22's per-iteration variable, which shared routing would break. A literal's own parameters/results are not captures and are excluded. (The remaining known gap, deliberately out of scope here: a `for`-init variable captured by closures diverges from Go 1.22 per-iteration semantics — the C# `for` control variable is shared across iterations — tracked as its own defect.)

The native route makes the emitted C# read exactly like the Go for the parameter case; the by-box route reuses the box-ref machinery above (including `ValueSlot` for inherently-heap locals: a captured slice that the closure reassigns emits `Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, …)` against a materialized `heap<slice<T>>` box). (Guarded by the `ClosureWriteVisibility` behavioral test — 19 probes: boxed/plain × local/param × closure-writes/body-writes-after × plain/defer/go/IIFE/loop-created contexts, plus slice/map reassignment, alias writes, two-closure sharing, and the read-only/defer-argument/range controls that must KEEP snapshot semantics.)

**A NAMED RESULT routed to shared storage declares its box too.** The `defer func(){ hook(written, err) }()` idiom is exactly the written-after-capture shape above with the captured variable being a *named result* — Go's deferred closure must observe the FINAL named-result values. When the escape analysis marks such a result (an interface-typed result is blanket-marked the first time it is reused on a mixed `v, err := …` define; a value-type one when `&x` is taken), the render sites duly go through the box (`Ꮡerr.ValueSlot` inside the deferred literal) — but the named-result declaration prologue emitted only the plain `error err = default!;`, leaving `Ꮡerr` undeclared (CS0103 — internal/poll `SendFile`'s deferred `TestHookDidSendFile`, the single error skip-cascading ~80 os-dependent packages). A box-backed named result (`identHasHeapBox`, the same gate plain locals use) now declares the box, in three shapes:

- **No defer frame** (plain function, or a closure writing the result): the full escaping-local form at the declaration site — `ref var err = ref heap<error>(out var Ꮡerr);` — body and bare returns keep reading the plain alias, nested closures read/write `Ꮡerr.ValueSlot`. A value-type result with `&x` gets `ref var x = ref heap(new nint(), out var Ꮡx);`, making the write through `&x` visible to the bare return (previously that shape was also CS0103).
- **namedReturnDeferMode** (a function whose results are declared before the frame's `try`): the decls sit OUTSIDE the `try`, and the deferred closures that read them are lambdas, which cannot capture a `ref` local (CS8175) — so the outside line creates only the box (`heap<error>(out var Ꮡerr);`), the `try` re-derives the value alias inside (`ref var err = ref Ꮡerr.ValueSlot;`, exactly like a deref'd pointer parameter's `ref var fd = ref Ꮡfd.Value;`), and the post-`finally` return reads through the box: `ᒐdone: return (written, Ꮡerr.ValueSlot);` (internal/poll `sendfile_windows.cs`).
- **Func-literal sibling** (a literal with named results + defer + post-capture writes): same split, except the literal's body is itself a lambda conversion, so every in-`try` use already renders through the box — including the explicit-return rewrite's assignment targets (`(var v, Ꮡe.ValueSlot) = pair(n);`) — and the literal's trailing `return (w, Ꮡe.ValueSlot);` reads the box.

The box-read accessor follows the box-ref rule above: `.ValueSlot` for an inherently-heap result (reading the held reference is not a dereference), `.Value` for a value-type box. Results NOT escape-marked are untouched — `written` in the same defer stays a plain local captured natively by the C# closure, which already observes the final value. (Guarded by the `NamedResultDeferCapture` behavioral test — value + error named results logged by a deferred closure with post-capture writes and bare returns, the `&x` value-result, the func-literal sibling, and a non-defer closure write; output-compared vs Go, proving the deferred observation of FINAL values. Stdlib footprint: 12 functions across 10 files — internal/poll, net/http, go/parser, crypto/tls, internal/fuzz, debug/buildinfo, both go importers, net/textproto.)

**A PARAMETER routed to shared storage declares its box too** — the third position of the same family (plain locals, named results, parameters). A parameter can be escape-marked without any capture-mode method call: a body-top-level mixed `:=` REDECLARES the parameter object (the spec's redeclaration rule includes the parameter lists when the block is the function body), so the define walker escape-analyzes it — and an interface-typed one is blanket-marked. When such a parameter is also captured by a closure and written after the capture point, the routing above sends it by-box (`Ꮡctx.ValueSlot` inside the lambda) — but the parameter prologue only boxed for the capture-mode (direct-ж) trigger, leaving the box undeclared (CS0103): database/sql `beginDC`'s `ctx` (redeclared by `ctx, cancel := context.WithCancel(ctx)` after `withLock`'s closure captured it) and go/types `nify`'s `x, y` (swapped by `x, y = y, x` and redeclared by `xorig, x := x, Unalias(x)` after the trace defer captured them). `paramNeedsHeapBox` (and its func-literal analogue `funcLitHeapBoxParamIdents`) now also fires for a box-ref-routed parameter, emitting the exact capture-mode form — the signature takes the incoming value as `ctxʗp` and the preamble declares `ref var ctx = ref heap(ctxʗp, out var Ꮡctx);` (inside the frame's `try` when the function has a frame, where the box is an ordinary capturable local). Body statements keep reading/writing the plain ref alias — the redeclare emits `(ctx, var cancel) = …` against it — so both sides hit the ONE box, and a deferred observer sees Go's FINAL values. The check rides the declaring-ident lookups, so a box-ref'd value RECEIVER (never `ʗp`-renamed by the signature paths) can never take the param form. (Guarded by the `WrittenCaptureParam` behavioral test — the beginDC redeclare shape, the nify deferred-observer shape (named result + defer frame), a closure-write read back by the body, the func-literal sibling, and an inherently-heap slice param; all output-compared vs Go. Stdlib footprint: exactly `database/sql/sql.cs` + `go/types/unify.cs`.)

## A write that ENCLOSES the literal counts as written-after-capture — the self-recursive closure

The write scan above compares *positions*: a body write counts when it sits after a referencing literal, or shares a loop with one. Go's standard recursive-closure idiom defeats a pure position test, because the write **starts before** the literal it contains:

```go
var check func(uint32, []bool) bool
check = func(pc uint32, m []bool) (ok bool) {
	…
	ok = check(inst.Out, m) && check(inst.Arg, m)   // recurses through the variable
	…
}
```

The assignment statement's position is that of `check` on its left, which precedes the literal on its right — yet the RHS is evaluated *first*, so the store to `check` unambiguously happens after the literal exists. Scored as read-only-after-capture, the capture took the snapshot path (`var checkʗ1 = check;` hoisted **above** the assignment) and every recursive call invoked the still-**null** delegate: a `NullReferenceException` on the first recursion. In regexp's `makeOnePass` that is the entire ambiguity check, so `^.$` — and most of the package — became uncompilable. The scan now also counts a write whose syntactic extent *contains* a referencing literal (`w.pos < lit.pos < w.end`), which routes the capture to shared storage; `check` escapes, so it takes the by-box form and the recursion resolves against the box the assignment fills:

```csharp
ref var check = ref heap<Func<uint32, slice<bool>, bool>>(out var Ꮡcheck);
check = (uint32 pc, slice<bool> mΔ1) => {
    …
    ok = Ꮡcheck.ValueSlot((~inst).Out, mΔ1) && Ꮡcheck.ValueSlot((~inst).Arg, mΔ1);
    …
};
```

The same edge covers **mutually** recursive closures (`even`/`odd`, each literal enclosed by the write to its own name while reading the other), and it generalizes beyond closures: any write that evaluates a referencing literal as part of itself — `t.mutate(func(){ use(t) })` — now counts. (Guarded by the `ClosureWriteVisibility` probes Q1/Q2 — a self-recursive sum and a mutually recursive parity pair; the pre-fix converter compiles both and nil-derefs at the first recursive call.)

## A nested closure's capture snapshot reads the enclosing closure's snapshot
When a heap-boxed **ref-local is used by VALUE** (its address is not taken) and captured by NESTED closures, it is not box-ref'd — it is snapshot-copied: the converter declares `var mʗ1 = m;` before the closure and the closure uses `mʗ1`, so the uncapturable `ref`-local `m` is never referenced inside the lambda. The snapshot chain must be threaded through each level. A capture generated for an **inner** closure that lands inside an **outer** closure's body must read the outer closure's snapshot, not the enclosing method's ref-local — the shape testing/fuzz.go's `run` closure has, capturing `fn := reflect.ValueOf(ff)` (a heap-boxed `reflect.Value`) and spawning `go tRunner(t, func(t){ … fn.Call(args) })` from inside itself, where the method-level `fn` is a ref-local uncapturable inside a closure (CS8175). The guard's own shape emits it:

```csharp
ref var p = ref heap<payload>(out var Ꮡp);
p = new payload(vals: new nint[]{1, 2, 3, 4}.slice());
var @out = new channel<nint>(1);
var outʗ1 = @out;
var pʗ1 = p;                   // outer's snapshot (before the outer closure)
void outer() {
    var outʗ2 = outʗ1;
    var pʗ2 = pʗ1;             // the goroutine's snapshot reads outer's pʗ1, NOT p
    goǃ(() => {
        outʗ2.ᐸꟷ(pʗ2.sum());
    });
}
```

`generateCaptureDeclarations` finds the RHS by walking the conversion stack outward past pass-through levels (a `go`/`defer` statement's own `enterLambdaConversion`, which carries an empty rename map) to the first enclosing lambda that renamed the variable. It skips the capture's OWN owner state — `pendingCaptures` is shared across a function's lambdas, so an outer lambda's snapshot can be generated while converting an inner func-literal argument (`go dnsWaitGroupDone(ch, func(){})`, net/lookup.go), leaving the owner's state on the stack with a rename equal to the name being declared; adopting it would emit a self-reference `var fʗ1 = fʗ1;` (CS0841). Byte-identical corpus-wide except where a nested closure re-captures a heap-boxed local. Guarded by `FuncLitArgCapture` (a heap-boxed struct re-captured in an inner goroutine — CS8175 without the fix — and the `go f(x, func(){})` self-reference shape) and by `DeferValueFieldPtrReceiver` (a defer inside a lambda).

A **pointer (or other inherently-heap) local** captured by a closure that takes its address needs the box too, but reaches it by a different route. A local of an *inherently heap-allocated* type — a pointer, slice, map, channel, interface, or func — is already a reference, so it normally gets **no** heap box (the `convertToHeapTypeDecl` path returns nothing for such types). But when one is captured by a closure that takes its address (`mToFlush := &node{…}; run(func(){ prev := &mToFlush; … *prev = mToFlush.next })`), the closure needs a *shared* box so writes through `&mToFlush` inside it reach the outer function's storage. The converter detects this as the same *box-ref* mark used above (an inherently-heap local whose address is taken inside a lambda), and for a box-ref local it now emits the heap box even though the type is inherently heap — `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` — so the box `ᏑmToFlush` (a `ж<ж<node>>`, i.e. a `**node`) exists for the closure to reference. Without it the closure emitted `ᏑmToFlush` for `&mToFlush` against a never-declared box (CS0103); a same-function `&ptr` with **no** closure still takes the `Ꮡ(ptr)` copy form (a copy is fine there — no shared storage is needed), so that case is unchanged.

Reading such a box needs care, because for a box-of-pointer the held value can legitimately be nil while the box itself is a real allocation. `Ꮡm` here is a `ж<ж<node>>` (a `**node`), so `Ꮡm.Value` reads the *held pointer value* — not a dereference of `Ꮡm` — and in Go reading `*(&p)` when `p` is a nil `*T`/slice/map yields the nil value, with no dereference and no panic. The strict `ж<T>.Value` getter (which panics on a null stored value by design, so a genuine `*p` on a nil pointer still throws) would wrongly panic on that read. So the converter emits the golib `ж<T>.ValueSlot` accessor for these box-of-pointer reads — identical to `.Value` but without the nil-pointer-dereference check, returning the *real* slot so reads and writes both persist (and unlike the retired `DerefOrNil`, which yielded a throwaway slot for a genuinely-nil box). `ValueSlot` is selected here for a box-ref **local** of inherently-heap type; a deref'd pointer *parameter* reaches the same slot through `DerefOrNull()`, whose non-nil path IS `ValueSlot` — see *The THREE deref accessors of `ж<T>`*. The `heap(out …)` / `heap(target, out …)` helpers likewise return `ref pointer.ValueSlot`: a freshly allocated box is structurally non-nil, so the getter's nil check there is always spurious (identical to `.Value` for a value-type box; it just avoids a spurious panic when establishing the `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` alias). A genuine dereference of the held pointer (the second `.Value` in `ᏑmToFlush.ValueSlot.Value.v`) stays strict and still panics on nil — preserving Go's "panic ⇒ panic" semantics, and complementing the deliberate strict-`.Value` design at every genuine USE site. (Guarded by the `ClosureCapturedPointerAddress` behavioral test — a closure that takes the address of a captured pointer local, walks a linked list by reassigning *through* that address and mutating each node, with the outer function observing both the reassignment-to-nil and the persisted mutations, proving the box is shared rather than copied. Mirrors runtime's `trace.go` `mToFlush := allm; systemstack(func(){ prev := &mToFlush; … mToFlush = mToFlush.next })`, ~4 CS0103.)

A pointer-receiver method called **through a FIELD of such a boxed pointer local**, inside the closure, field-refs through the **held pointer**, not the box. The receiver of `c.flushGen.Store(…)` (runtime `mcache.go`'s `allocmcache`, inside `systemstack`) is taken via the &-machinery, and inside a lambda the box-ref address form substitutes the capturable box for the uncapturable ref-local alias. For a *value*-struct local (box `ж<T>`) and a deref'd pointer *parameter* (box `ж<T>` — the Go pointer itself) the bare box is the correct `.of()` receiver — but a boxed pointer LOCAL's box is `ж<ж<T>>`, one level above the `ж<T>` the field accessor projects from, and feeding it to `.of` fails inference (CS0411 — the one error that skip-cascaded ~237 packages behind `runtime`). Such a base declines the bare-box form and falls through to the pointer-variable field arm, whose ident render reads the box the same way every other in-lambda value use does: `Ꮡc.ValueSlot.of(mcache.ᏑflushGen).Store(…)` — `.ValueSlot` because reading the held pointer out of the box must not nil-check (the dereference happens in `.of`, preserving panic semantics), and because that slot IS what the enclosing `ref var c = ref heap<ж<mcache>>(out var Ꮡc)` alias reads. When such a local is **named after its own type** (`gauge := newGauge()`), the accessor's owning-type name additionally qualifies with the package class (`Ꮡgauge.ValueSlot.of(main_package.gauge.Ꮡv)`): the enclosing `ж<gauge>`-declared local stays visible inside the lambda, so the bare type name binds the uncapturable ref-local (CS8175) with no identical-simple-name fallback — the declared type differs from the type name. (Guarded by the `ClosurePtrLocalFieldMethod` behavioral test — the `allocmcache` shape: a pointer local written inside a closure and immediately method-called through a value field, read back after the closure, plus the named-after-type variant; output-compared vs Go, proving the write-through and the field-method call both bind the one shared box.)

A **deref'd pointer parameter or pointer receiver** captured by a closure is box-ref'd the same way, even when only its *value* is used inside the closure (not its address). Such a parameter is emitted as the box `ж<T> Ꮡp` with `ref var p = ref Ꮡp.DerefOrNull()`, and the `ref`-local alias cannot be captured (CS8175). Inside the closure a value use becomes `Ꮡp.Value.field` and an address use `Ꮡp`, so the closure captures the box by reference — matching Go capturing the pointer. (Guarded by the behavioral test `PointerParamCapturedInClosure`; the runtime captures `*maptype` / `*m` parameters this way pervasively.)

A pointer **receiver** captured by a closure needs an extra step the parameter case does not: the box `Ꮡp` only exists if the method is emitted **direct-ж** (the box passed *as* the receiver, `this ж<T> Ꮡp`). A normal pointer-receiver method is `[GoRecv] this ref T p` (a value-ref receiver, with the `ж<T>` companion generated separately), which has no box for the closure to reference. So "the receiver is referenced inside a function literal" is a **direct-ж trigger** — a fourth one alongside taking a field's address (`&p.field`), returning the receiver (`return p`), and using the receiver as a bare pointer value (`p.next = p`, `p != q`). Mirrors runtime's `func (p *_panic) nextFrame() { systemstack(func(){ … p.lr … }) }`. A closure parameter that shadows the receiver name resolves to a distinct object, so it does not falsely trigger the promotion. (Guarded by the `ReceiverCapturedInClosure` behavioral test — receiver captured by an immediately-invoked closure that reads/writes through it, by one that takes a field's address, and by one that is *returned* so the box must outlive the call.)

Once a method is direct-ж, its receiver is the box `Ꮡc`, but the deref'd value alias `ref var c = ref Ꮡc.DerefOrNull()` is what most uses see. When such a receiver is passed **whole** as a pointer argument — `stackcache_clear(c)` in `func (c *mcache) prepareForSweep()` — the argument must be the box `Ꮡc`, not the value alias `c` (a value cannot bind a `ж<mcache>` parameter → CS1503). A deref-aliased pointer *parameter* is already handled (it is an `identIsParameter`), but a direct-ж *receiver* is not a parameter, so the call-argument conversion recognizes it explicitly and emits the box. (Guarded by the `DirectBoxReceiverPassedWhole` behavioral test.)

The receiver placed whole into a **composite-literal element** whose field is a pointer — `func (f *_func) funcInfo() funcInfo { …; return funcInfo{f, mod} }` (runtime `symtab.go`; `funcInfo`'s first field is the embedded `*_func`) — needs the same box, and is itself a **direct-ж promotion trigger** (`bodyUsesReceiverAsPointerValue`'s composite arm): a boxless `[GoRecv] ref` receiver has no `Ꮡf` to place in the field (CS1503). Once promoted, the composite renders the box through the existing pointer-field element machinery: `new ΔfuncInfo(Ꮡf, mod)`. Both positional and keyed elements trigger, gated on the **field's declared type being a Go pointer** (resolved positionally or by key from the composite's struct type — the element expression's own type is always `*T` for a pointer receiver): a receiver placed into an *interface*-typed field also typechecks in Go, but that emission compiles today, and promoting for it would re-route every such method stdlib-wide (the field gate trims the first-cut 73-file audit to 68 — the shape is genuinely pervasive: go/types' Checker methods, net/textproto's dotReader{r: r}, zstd readers — every audited site the same signature+box re-routing) — its pointer-identity semantics are logged as a separate question. (Guarded by the `DirectBoxReceiverPassedWhole` extension — positional + keyed composites, identity verified by writing through the wrapped pointer and reading the original.)

The same composite arm also fires when the receiver is stored **as an element of a SLICE or ARRAY literal whose element type is a pointer** — `func (s *UserTaskSummary) Descendents() []*UserTaskSummary { descendents := []*UserTaskSummary{s}; … }` (internal/trace `summary.go`). Without promotion the boxless `[GoRecv] ref` receiver renders the value alias `s` into a `ж<T>[]` slot (CS0029); once promoted direct-ж, the element renders the box: `new ж<UserTaskSummary>[]{Ꮡs}.slice()` (and `[2]*T{s, other}` → `new ж<T>[]{Ꮡs, Ꮡother}.array()`). Gated on the **slice/array element type being a pointer** (the `*types.Slice`/`*types.Array` arms of `bodyUsesReceiverAsPointerValue`), mirroring the struct-field pointer gate. (Guarded by the `ReceiverPointerValue` extension — the receiver stored into a `[]*ring` and a `[2]*ring` literal, `chain[0]` identity verified by mutating through the stored pointer and reading back through the receiver.)

The same pointer-element boxing must also fire for an **ELIDED (type-inferred) nested composite** — the inner `{c}` of `[][]*Certificate{{c}}` (crypto/x509 `Verify`). The inner literal has no `Type` node; its inferred element type is `*Certificate`, and its sole element `c` is the deref-aliased `*Certificate` receiver. The typed composite path boxes a bare pointer-typed ident element (`argTypeIsPtr`), but the untyped-elided slice/array path rendered its elements with a **nil** context, so that treatment never ran and `c` emitted the value alias into a `ж<Certificate>[]` array (CS0029). The elided path now supplies a context that boxes a bare pointer-typed ident when the element type is a pointer — `new ж<Certificate>[]{Ꮡc}.slice()` — returning nil (unchanged nil-context rendering) when the element type is not a pointer or no element is a bare pointer ident, so non-pointer elided literals stay byte-identical. (Guarded by the `ElidedNestedPtrComposite` behavioral test — `[][]*Node{{n}}` where `n` is a pointer receiver.)

A **MAP** composite literal whose value or key type is a pointer boxes its element the same way — but through `convKeyValueExpr` (the `[key] = value` form), not the slice/array element loop above. `map[K]*T{k: c}` where `c` is a deref'd pointer parameter renders the value alias `c` into a `ж<T>` map slot (CS0029); the map-source branch of `convKeyValueExpr` now sets the `isPointer` ident context for the VALUE when the map's declared element type is a pointer, so a bare-ident pointer value emits its box `Ꮡc` — `new map<@string, ж<node>>{["a"u8] = Ꮡa}`. A pointer-KEY map (`map[*T]V{c: 1}`) boxes the key the same way (`new map<ж<node>, nint>{[Ꮡa] = 1}` — the `ж<T>` dictionary key matches by box identity). Gated on the map's declared **element/key type being a pointer** (not an interface — an interface-valued map still routes through the interface conversion) *and* the element expr's own type being a pointer, so a value already rendered as a box (`&x`, a pointer local) is unaffected. (Guarded by the `MapPointerElementLiteral` behavioral test — a pointer-value map and a pointer-key map built from `*node` parameters, aliasing verified by mutating through a stored value and looking up by pointer-key identity.)

**Reassigning a pointer parameter to a new pointer.** A `*T` parameter that walks memory by reassignment — `bits = addb(bits, n)` (a `*byte` step in the runtime's bitmap scanners) or `p = p.next` (a list walk) — cannot write through its value alias: `ref var bits = ref Ꮡbits.Value` makes `bits` the pointed-to *value*, and a pointer RHS (`ж<byte>`) does not fit it (CS0266/CS0029). The reassignment instead repoints the **box** and re-aliases the value var — `Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.Value;` — reusing the same box-reassignment path that handles a direct-ж receiver's `r = r.prev` (the RHS already emits the box form). (Guarded by the `PointerParamWalk` behavioral test, a circular-list walk that reassigns the parameter and reads the pointed-to value each step.) Reassigning a *pointer local* (not a parameter) is unaffected — a local already holds the box.

**Reassigning the RECEIVER is itself a direct-ж trigger.** Go's pointer receiver is an ordinary local, so a method may repoint it to walk a structure — `func (s *fakeStmt) QueryContext(…) { for { …; if s.next == nil { break }; s = s.next } }` (database/sql's `fakedb_test.go`) — and the rebind is local to the callee. The emission is the receiver twin of the parameter case directly above (`Ꮡs = s.next; s = ref Ꮡs.DerefOrNull();`), and `visitAssignStmt`'s arm for it has existed as long as the parameter's — but it is reachable only through the box `Ꮡs`, which exists only when the method is emitted **direct-ж**. That made the repoint a trigger the pre-pass did not have: a method whose receiver is repointed but which returns nothing, compares nothing to nil and takes no field address matched none of the other nine predicates, kept the boxless `[GoRecv] this ref T s`, and the assignment put a `ж<T>` into a `ref T` (CS0029). `bodyReassignsReceiver` closes it, matching by **object identity** so an inner `:=` that shadows the receiver's name — a different variable — does not promote the method. The trigger is invisible on the production corpus by construction: every stdlib method that repoints its receiver is *also* carried by a neighbouring predicate (`container/ring`'s `Move` and `go/types`' `LookupParent` return it; `math/big`'s `fmtX`, `net/http`'s `addBytes` and `time`'s two `(*Location)` lookups likewise already emit `this ж<T> Ꮡx`), which is why the gap first surfaced in a test file — and why adding the trigger moves no production emission. (Guarded by the `PointerReceiverRepoint` behavioral test — three methods that repoint the receiver to walk a list, one summing, one advancing a bounded number of steps and reading a field afterward, one *writing* through the receiver at every step so a stale alias would scale the head repeatedly; all deliberately free of every other trigger, with the caller's head pointer proven unmoved by the callee's rebind, output-compared vs Go.)

**A box accessor's qualifier names the class that DECLARES the type, which under `-tests` can be the bridge.** The accessor `Type.Ꮡfield` in `receiver.of(Type.Ꮡfield)` package-qualifies whenever a bare name could be shadowed (see *Field address of a collision-renamed heap-boxed local*), and the qualifier used to be the production `<pkg>_package` unconditionally. For a type an internal `_test.go` declares that class is the wrong one — the white-box emission unit is the bridge (`<pkg>_internal_test_package`), so the reference resolves to nothing (CS0117). `database/sql`'s `fakedb_test.go` is the corpus instance and it is forced into the qualifying path: `type table struct { mu sync.Mutex; … }` sits beside `func (db *fakeDB) table(string) (*table, bool)`, so the *type* is Δ-renamed, and Δ-renamed always qualifies — every one of the six `t.mu.Lock()`/`Unlock()` sites emitted `sql_package.Δtable.Ꮡmu`. The qualifier now resolves through `packageScopeClassName`, the same helper that already draws the production/bridge line for package-level *value* references, so both halves stay addressable from the one bridge file. Production conversions are unaffected by construction — outside `-tests` there is no class override and the helper returns the production class. (Guarded by the `TestTestVariantBoxAccessorNamesBridgeDeclaringClass` converter test, which asserts both directions: the bridge class appears and the production class does not.)

**Reassigning a captured pointer parameter inside a closure.** The repoint-and-re-alias above (`Ꮡp = …; p = ref Ꮡp.Value;`) rebinds a `ref`-local. Inside a CLOSURE that captured the parameter that is illegal: the re-aliased value var is an ENCLOSING `ref`-local, and C# forbids referencing an outer `ref` local inside a lambda (CS8175 — crypto/x509 `buildChains`'s `considerCandidate` closure does `if sigChecks == nil { sigChecks = new(int) }` on the captured `*int` parameter). The box reassignment `Ꮡp = …` is legal (it writes the captured box field, hoisted to a closure field), so only the ref-local refresh is dropped inside a lambda:
```csharp
if (ᏑsigChecks == nil) {
    ᏑsigChecks = @new<nint>();          // was: … ; sigChecks = ref ᏑsigChecks.DerefOrNull();  (CS8175)
}
ᏑsigChecks.Value++;
```
Every in-lambda and post-lambda dereference of a repointed captured pointer routes through the box `Ꮡp.Value`, so the now-stale value alias is never read — an accepted modeling gap (like the nil-terminated walk's), not a miscompile. The suppression is sound because no LEGITIMATE re-alias ever occurs inside a lambda: a lambda's OWN pointer parameter is passed as the box `ж<T>` (never deref-aliased), and a heap-boxed value local is written THROUGH its box (`Ꮡb.Value = …`, never box-repointed). Guarded by `ClosureReassignsPtrParam` (a closure that reassigns a captured `*int` parameter; a non-nil runtime argument keeps the reassignment branch unreached so output stays deterministic).

The same repoint-and-re-alias applies when the parameter is reassigned **from a tuple** — `(left, x, idx) = binarySearchTree(x, idx, n/2)` (runtime `mgcstack.go`) or `pp, _ = pidleget(0)` (`proc.go`). The box-reassignment triggers matched the RHS **element-wise**, so a tuple *deconstruction* (one call RHS, several LHS) never fired them — the ж<T> tuple component was assigned into the deref'd value alias (CS0029) — and element 0's raw expression type is the whole `*types.Tuple` (never a pointer), so even a first-position pointer element missed. The per-element RHS type now comes from the call's result tuple, and the emitted form is the single-assign form verbatim: `(left, Ꮡx, idx) = binarySearchTree(Ꮡx, idx, n / 2); x = ref Ꮡx.DerefOrNull();` — the same nil-deferring re-alias every repoint takes (`(Ꮡpp, _) = pidleget(0); pp = ref Ꮡpp.DerefOrNull();`). The triggers are gated to a **reassigned** element: a `:=`-declared pointer element binds the tuple's ж<T> component into a fresh pointer local — which *is* the box — directly, and an inner `:=` local shadowing a parameter's name must not repoint the parameter's box (crypto/x509's `c, _, err := …cert(i)`). (Guarded by the `PointerParamNilWalk` extension — a nil-compared tuple-reassign walk plus a reassign-then-mutate-through probe, values vs Go.)

**Assigning `nil` to the parameter itself is a box repoint, not a write to the pointee.** Both triggers above gate on the RHS being *pointer-typed*, and the untyped `nil` literal has no type of its own — so `p = nil` missed them, rendered against the deref'd **value** alias, and emitted `p = default!`, which **zeroes the pointed-to struct** while leaving the box `Ꮡp` non-nil. The caller's `!= nil` then still passed and it walked a wiped-out object. This is regexp's `makeOnePass`, whose `p = nil` (the "not one-pass after all" bail-out) handed `compileOnePass` an `onePassProg` with an **emptied `Inst` slice** instead of a nil pointer — an index-out-of-range in `cleanupOnePass` on every pattern the one-pass analysis rejected, which is most of them. A nil RHS is now treated as pointer-valued whenever the corresponding target is pointer-typed, so it takes the ordinary repoint-and-re-alias form (nil-DEFERRING, as every repoint is):

```go
// regexp/onepass.go — makeOnePass
if !check(pc, m) { p = nil; break }
…
if p != nil { for i := range p.Inst { p.Inst[i].Rune = onePassRunes[i] } }
```
```csharp
if (!check(pc, m)) {
    Ꮡp = default!; p = ref Ꮡp.DerefOrNull();  // the POINTER goes nil; the pointee is untouched
    break;
}
…
if (Ꮡp != nil) { foreach (var (i, _) in p.Inst) { p.Inst[i].Rune = onePassRunes[i]; } }
```

A pointer **local** assigned nil is unaffected — a local already *is* the box, so `p = default!` is correct there. (Guarded by the `PointerParamNilWalk` extension `dropIfShort` — nils the parameter, returns it, and the caller then proves the original node's value survived; the pre-fix converter compiles it and reports a non-nil result with a zeroed pointee.)

**Nil-terminated walk.** A pointer-parameter walk that stops at a nil terminator — `func sumList(p *node) int { for p != nil { total += p.val; p = p.next } }` — needs two extra pieces, *modeled together*:

1. **Compare the box, not the value alias.** The loop guard `p != nil` must emit `Ꮡp != nil` (the box). Each binary operand's pointer context is otherwise taken from the *other* operand's pointer-ness, and `nil` is not a pointer type — so the param would convert in value form (`p != nil`, comparing a `node` struct value, the wrong thing). The converter forces the box form for a deref'd pointer *parameter* in a `==`/`!=` comparison. This is safe only for a parameter: a pointer *local* is already the box, and forcing it would emit a non-existent `Ꮡlocal`.
2. **Nil-deferring re-alias.** On the final step `p.next` is nil, so `Ꮡp = p.next` repoints the box to nil; re-aliasing through the plain `Ꮡp.Value` getter would then throw a nil-pointer dereference before the guard is re-checked. The deref/re-alias instead routes through the golib `ж<T>` extension `Ꮡp.DerefOrNull()`, which binds `Unsafe.NullRef<T>` when the box is nil — legal to HOLD, faulting only on USE — rather than throwing at the bind. The entry alias uses it too, so an empty-list call (`sumList(nil)`) binds without faulting at entry.

```csharp
internal static nint sumList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();
    nint total = 0;
    while (Ꮡp != nil) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return total;
}
```

`DerefOrNull()` is **not** a substitute for a genuine dereference: reading or writing `*p` on a nil pointer (`~Ꮡp` / `Ꮡp.Value`) still panics, preserving Go semantics — and so does a read THROUGH the bound null ref, which is the whole point. Neither piece needs a predicate any more: piece 1 (the box-form comparison) is a property of the expression, and piece 2 is what EVERY pointer entry alias and repoint now emits, because a repoint is not a dereference in Go and a nil argument is not an error in Go (see *A pointer PARAMETER is nil-deferring for exactly the reason a receiver is*). Historically both were gated on the body nil-COMPARING the parameter, which covered the reassigned walk above and a nil-testing body invoked with a literal-nil argument (`defer closeIt(nil, 3)` → `p == nil`) — at the cost of a shared `default(T)` slot that let an *unguarded* deref of an actually-nil argument read a silent zero where Go panics. The unconditional accessor keeps the walk working and drops the trade. (Guarded by the `PointerParamNilWalk` behavioral test — a nil-terminated sum, a mutate-through-the-parameter pass, and an empty-list call — plus `DeferTypelessReturns`' deferred nil-argument call. `PointerParamWalk` covers the never-nil circular walk.)

A **package-level global** referenced inside a closure is *not* captured at all — it is a C# static, accessed live. A value snapshot (`var gʗ1 = g`) would copy the struct (so `&gʗ1` has no box → CS0103, and writes through the global from inside the closure would be lost) and is semantically wrong, since Go reads/writes the live global. For an address-taken (heap-boxed) global the closure references the static box `Ꮡg` directly — a method call routes as `Ꮡg.method()` and a field address as `Ꮡg.of(T.Ꮡfield)`. (Guarded by `GlobalCapturedInClosure`; the runtime does this in every `systemstack(func(){ … mheap_ … })`.)

## A func literal that is only ever CALLED emits as a C# LOCAL FUNCTION
A C# lambda that captures anything allocates **two** heap objects every time the lambda expression is evaluated: a display class holding the captured variables, and a delegate bound to it. That is charged per call of the *enclosing* function, whether or not the closure is ever invoked — 88 bytes for the two-word case, measured. Go allocates neither when its escape analysis proves the closure does not outlive the frame, which is why `time`'s `TestUnmarshalTextAllocations` asserts `want 0 allocs`, and why `parseRFC3339`'s `parseUint := func(…)` was 88 of that row's 216.

A `name := func(…){…}` whose variable is **only ever the callee of a call** is therefore emitted as a C# *local function* instead:
```csharp
//  Go:   ok := true
//        parseUint := func(s bytes, min, max int) (x int) { … ok = false … return x }
var ok = true;
nint /*x*/ parseUint(bytes sΔ1, nint minΔ1, nint max) {
    nint x = default!;
    …
    ok = false;          // the SAME `ok` — both sites are rewritten to one struct-closure field
    …
    return x;
}
nint year = parseUint(s[0..4], 0, 9999);
```
Roslyn compiles a local function that is never converted to a delegate with a **by-ref struct closure**: the captured variables move into a struct that lives in the enclosing frame and is passed as a hidden `ref` parameter. There is still exactly one storage location per captured variable — the enclosing method's own uses are rewritten to the same field — so sharing, write-visibility and the capture-snapshot machinery are all unchanged. Only the heap objects are gone. The result type is rendered by the same helper `visitFuncDecl` uses, so a named Go result keeps its `/*x*/` comment and a local function reads exactly like a declared one; a single-return literal keeps the expression-bodied collapse (`byte num2(slice<byte> bΔ1) => …;`).

**The "only ever called" proof is what keeps that compilation available**, not a convenience: converting a local function to a delegate anywhere makes Roslyn fall back to a heap display class, and a local function has no value form to give a store, a return, an argument or a comparison in the first place. Every reference other than the declaring occurrence must be a call callee — which also subsumes reassignment (`f = …` is a non-call use of `f`) and address-taking, so the emitted name can never be required as a first-class value. Three further gates: the statement must be a `:=` **define** with one LHS ident and one RHS literal (a mixed `f, err := …` re-use records the name in `Uses`, not `Defs`, and binds no fresh object); it must be in statement position, since a local function is a declaration and cannot sit in a `for`/`if`/`switch` init clause; and the enclosing function declaration must be known (a literal inside a package-level `var` initializer is left alone).

A literal that **defers or recovers** is no bar: its frame is an ordinary local of the local function, declared in the local function's own body like any other, so the whole shape stays allocation-free —

```csharp
nint /*r*/ guard(nint n) {
    nint r = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    r = -1;
                }
            }
        }, ref ᒐ);
        if (n < 0) {
            throw panic("negative");
        }
        r = n * 2; goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}
```

Go's two-step recursion idiom (`var f func(int) int; f = func(int) int {…}`) is an ASSIGN, not a DEFINE, so it is not this shape at all and keeps the lambda — correctly, since the recursive reference reads `f` as a value. (Guarded by the `LocalFunctionEmission` behavioral test: the `parseUint` shape with a named result and a mutated capture, the expression-bodied collapse, a struct-and-array capture mutated through the local function, two nested levels, and the deferring/recovering literal above — plus four negative controls, one per disqualifying reason: value use, reassignment, the recursion two-step, and argument position. The golden pins the emitted form; the stdout comparison against `go run` pins the capture semantics.)

## A variable DECLARED INSIDE a closure is not captured BY it
The escape analysis heap-boxes a local when something *outside* its frame can reach its storage; a closure is one such route, because the emitted C# serves the shared variable through a `ж<T>` box. The closure arm of that analysis matched on any mention of the object lexically inside a function literal's body — and for a variable declared *there*, that mention is its own declaration. So a literal's own local was treated as if the literal closed over it:
```csharp
//  Go:   testing.AllocsPerRun(100, func() { var t Time; t.UnmarshalText(in) })
Δtesting.AllocsPerRun(100, () => {
    ref var tΔ1 = ref heap(new Δtime.Time(), out var ᏑtΔ1);   // ← 128 B, and ᏑtΔ1 is never used
    tΔ1.UnmarshalText(inʗ1);
});
```
The box `ᏑtΔ1` is **never referenced anywhere in the emitted body** — `UnmarshalText` is a `this ref Time` extension, which binds the variable directly — while the identical two statements written outside a closure emitted a plain `Δtime.Time tΔ1 = default!;`. The arm now skips an object whose declaration position lies inside the literal, and the emission is the plain local. That was the other 128 of `time`'s 216.

The narrowing direction of an escape rule is the dangerous one — an under-box drops writes silently — so the proof is stated rather than assumed. Go scoping puts a literal's own local out of reach of every other frame, so there is nothing for a shared box to make visible; and every route by which such a local can *still* genuinely escape is decided by an arm that walks the **whole enclosing function body**, literal bodies included: `&x` / `&x.f` / `&x[i]` (the address-of arm), a pointer argument (the call arm), a `go`/`defer` use (their own arms), a capture-mode method call, and a pointer-receiver **method value** — Go's `(&x).M` written without the `&`. None of them is lost. The skip also keeps descending rather than stopping, so a literal **nested** inside the skipped one — which does close over the variable — still gets its own turn through the arm and still marks the escape.

(Guarded by the `ClosureLocalNoHeapBox` behavioral test. Five of its eight probes are the boxes that must SURVIVE, one per escape route, and each writes through the escaping alias and reads the value back so an over-narrowed rule prints a wrong number rather than merely emitting a different shape; the two positive probes are the pointer-receiver-method and copy-only shapes that now emit plain locals. Its N3 probe is the nesting case, and it is also the interaction test with the local-function rule above: the nested literal is emitted as a local function and captures the surviving box.)

## Capture-mode methods called through a value field of the receiver
A pointer-receiver method that takes the address of one of its own fields (`func (c *Counter) Add(d int32) int32 { return bump(&c.n, d) }`) is *capture-mode*: it is emitted with the heap box **as** its receiver (`this ж<Counter> Ꮡc`) so `&c.n` can field-reference the real storage as `Ꮡc.of(Counter.Ꮡn)`. When another struct embeds such a type as a **value field** and drives it through that field — `func (f *Flag) Incr() int32 { return f.c.Add(1) }` — the call needs a `ж<Counter>` aliasing the real `f.c`. The enclosing method is therefore itself promoted to capture-mode (direct-ж), and `f.c.Add(1)` is emitted as `(&f.c).Add(1)`:
```csharp
public static int32 Incr(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.Value;
    return Ꮡf.of(Flag.Ꮡc).Add(1);   // f.c.Add(1) — nested field-address box
}
```
The nested `Ꮡf.of(Flag.Ꮡc).of(Counter.Ꮡn)` chain resolves each level through `ж<T>.Value` (which honors a parent that is itself a field/array reference), so writes land on the real embedded field rather than a copy. A plain (non-capture) value method called through the same field — `f.c.Get()` — is left as a normal `f.c.Get()` value call.

This field-address routing applies only to **value** fields. When the field is itself a **pointer** — e.g. cpuProfile's `log *profBuf`, accessed as `cpuprof.log` where `cpuprof` is a heap-boxed global — its C# value is *already* a `ж<profBuf>` box, so a direct-ж method binds to it directly: `cpuprof.log.close()`. Taking the field's address (`Ꮡcpuprof.of(cpuProfile.Ꮡlog)`) would double-box to `ж<ж<profBuf>>` (CS1929). The heap-boxed-receiver routing recognizes that a field selector or indexed element whose own type is a Go pointer is already a box and skips the `&`-machinery for it. This discriminates a pointer *field* of a boxed global (already a box) from a deref'd pointer *parameter* (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`): the latter is a bare identifier, not a selector/index, so it is correctly still routed through `Ꮡs`. The same exclusion applies when the pointer field is reached through a pointer **local** rather than a boxed global — `s := sl.mspan; s.gcmarkBits.bytep(…)` where `s` is a `*mspan` local — which otherwise routed through the pointer-local-field address path (`s.of(mspan.ᏑgcmarkBits)`); the field value `(~s).gcmarkBits` is already the `ж<gcBits>`. (Guarded by the `PointerFieldOfBoxedGlobal` behavioral test, covering both the boxed-global `cpuprof.log.write`/`.close` form and the pointer-local `s.log.push` form; runtime exercises both pervasively, e.g. `mspan.sweep`.)

The same applies when the value field belongs to a **package global** rather than a receiver — `ctrl.total.Add(5)` where `var ctrl controller` and `total` is an atomic field. The method's box address goes through the field-address machinery, `Ꮡctrl.of(controller.Ꮡtotal).Add(5)`, not a bare `Ꮡ` prefix on `ctrl.total` (which would bind to the box variable `Ꮡctrl`, whose value type has no `total` member → CS1061). This is the form runtime uses pervasively for `gcController`, `sched`, `memstats`, etc. The **method call itself** triggers heap-boxing the global: when a pointer-receiver method is called on a (possibly nested) value field of a package value global, the escape pass marks that global address-taken so its box exists — the call site needs the box even when the global is never explicitly `&`-addressed elsewhere. This is gated on the method being ж-only (a pointer receiver): a same-package method known to be capture-mode, **or** any pointer-receiver method whose package's capture-mode set is not locally available — the latter covers cross-package atomic methods (`func (x *Uint32) Store`), which are likewise emitted with only a box receiver, so a plain value/ref of the field cannot bind them (CS1929). The walk to the global root bails at any pointer hop (a field reached through a pointer already has a real address and is handled by the pointer-local / receiver paths), so a receiver/parameter field such as `f.c` is never disturbed. (Guarded by the `AtomicValues` behavioral test's global-atomic-field case; runtime exercises this for `prof.signalLock`, `trace.seqlock`, `scavenge.gcPercentGoal`, etc.)

It also applies when the receiver is an **indexed element** of such a field — `trace.stackTab[i].dump()` (boxed global) — where the element's address goes through the box-field accessor: `Ꮡ(trace.stackTab, i).dump()` for a slice field, or `Ꮡtrace.of(T.ᏑstackTab).at<E>(i).dump()` for an array field. The same routing covers an indexed element of an array/slice reached through a **pointer** — `bh.Value[i].Load()`, where `bh` is a pointer and the element is an atomic value — emitted `bh.of(T.Ꮡval).at<E>(i).Load()`. This is gated on the called method being **direct-ж** (a box receiver): an ordinary `[GoRecv] ref` method binds to an addressable element directly, so it is left as `container[i].method()` and only a direct-ж method (which truly needs the box) is routed — avoiding needless churn on the common case. (Guarded by the `IndexedElementDirectBoxMethod` behavioral test — a direct-ж method on an array-element-through-a-pointer-parameter, with mutation persistence verified; runtime hits this on `mprof`'s `bh.Value[i].Load()`/`.StoreNoWB()`.)

A capture-mode method called on a **value local of an inherently-heap type** — a named *slice*/*map*/*chan* — also forces the box, which `identHasHeapBox` otherwise refuses. An inherently-heap type is already a reference, so a var of it is normally *not* boxed even when it "escapes" (the escape pass marks every inherently-heap local escaping and returns early). But a capture-mode pointer-receiver method — internal/trace/internal/oldtrace's `orderEventList` (a named `[]orderEvent`) with heap.Interface `Push`/`Pop` that forward the receiver to `heapUp(h, …)`/`heapDown(h, …)` — is emitted with a `ж<orderEventList>` receiver, so a plain value cannot bind it (CS1929 — `var frontier orderEventList; frontier.Push(…)`). The escape pass therefore records the capture-mode reason **in that inherently-heap early-return branch** (the only place these vars are seen, before the general address-of scan), and `identHasHeapBox` honors it — emitting `ref var frontier = ref heap<orderEventList>(out var Ꮡfrontier)` so the calls route `Ꮡfrontier.Push(…)`/`Ꮡfrontier.Pop()` through the box. A named slice/map/chan with no capture-mode method called on it stays unboxed (already a reference — no churn). (Guarded by the `NamedSliceCaptureMethod` behavioral test — a named-slice value local with `*stack` `push`/`pop` that forward the receiver to helpers, mutated and read through the same box, output-compared vs Go.)

A capture-mode method called on a **value PARAMETER** boxes the parameter **at entry** — go/format's `format(…, cfg printer.Config)` calling `cfg.Fprint(&buf, fset, file)`, where `(*printer.Config).Fprint` is transitively direct-ж (its body calls the defer/recover-wrapped `fprint` on its own receiver), so its only emitted receiver form is the box `ж<Config>` and the raw value parameter cannot bind it (CS1929 ×2). Parameters are deliberately **never** fed through the full escape analysis, so the escape pass runs only narrow, named parameter checks (`markCaptureModeBoxedParams`) rather than the general escape walk — this one being `bodyCallsCaptureModeMethodOn`, the same predicate the local-var arms use. (The companion check, `objectAddressTaken`, was added later — see *An address-taken VALUE PARAMETER heap-boxes too* above; before it, a plain `&param` did use the `Ꮡ(value)` copy-box.) For a marked param the **signature renames the incoming value to the `ʗp` form** (the variadic-prologue rename convention) and the parameter preamble declares the boxed alias:

```csharp
internal static (slice<byte>, error) format(…, printer.Config cfgʗp) {
    ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);
    …
    cfg.Indent = indent + indentAdj;        // body writes hit the boxed storage…
    var err = Ꮡcfg.Fprint(…);               // …the same storage the callee mutates through the receiver
```

Entry-time boxing is the load-bearing choice: Go auto-addresses the parameter (`cfg.Fprint(…)` ≡ `(&cfg).Fprint(…)`), so a body write **before** the call (`cfg.Indent = …`) must be seen by the callee, and the callee's writes through the receiver pointer must be seen by the rest of the body — while the **caller's** argument stays untouched (by-value parameter). A call-site `Ꮡ(cfg)` copy-box compiles but silently drops the callee's writes for the rest of the function. An ARRAY param folds its Go by-value clone into the box init (`ref var b = ref heap(bʗp.Clone(), out var Ꮡb);` — the plain `b = b.Clone();` preamble line is skipped), and an inherently-heap-typed param records the capture-mode box reason exactly like the value-local arm above. Beyond this trigger and the address-taken one, a param that leaks into `identEscapesHeap` some other way — a mixed `data, pc, line := …` define re-uses the param object, so the define walker escape-analyzes it (debug/gosym's `slice`) — keeps its historical unboxed emission (`paramNeedsHeapBox` re-verifies the predicate against the declaring ident). Whole-stdlib reconvert diff: exactly go/format's `internal.cs` changed, nothing else. (Guarded by the `CaptureModeValueParam` behavioral test — a defer-promoted direct-ж method plus a transitively-promoted one called on a value parameter, with a pre-call write observed by the callee, callee writes read back after, and the caller's copy proven untouched, output-compared vs Go — and by the `CaptureModeValueParamLib`/`CaptureModeValueParamUser` cross-package pair mirroring the format→printer shape: a foreign `Config` value param, `Fprint` → defer/recover `fprint` transitive promotion, trace accumulation across two calls proving write-visibility through the foreign `ж<Config>` extension.)

When the same function **also contains a func literal or defer that references the boxed parameter**, the in-lambda references must route **through the box** — the capture analysis marks such a param box-ref (the same arm family as a deref'd pointer parameter, whose `ref var p = ref Ꮡp.Value` alias shares the exact shape). The boxed param's Go name is a `ref`-local alias, which a C# lambda cannot capture (CS8175), and the general capture-snapshot fallback (`var tʗ1 = t;` before the lambda) compiles but **divorces the closure from the boxed storage** Go shares between the closure and the direct-ж callee: a closure read misses the callee's writes through the receiver pointer, a closure write is invisible to the callee, and a deferred closure observes entry-time values instead of return-time state. With the box-ref mark, a closure read emits `var get = () => Ꮡt.Value.total;`, a closure write `Ꮡt.Value.total += 100;`, and a deferred observer `defer(() => { (result, log) = (Ꮡt.Value.total, Ꮡt.Value.log); }, ref ᒐ);` — the box `Ꮡt` is a plain `ж<T>` local, captured by reference, so every reference (body, closure, callee) hits the one boxed storage, matching Go's one-parameter-variable semantics. A **deferred direct-ж method value on the param itself** (`defer t.Add(n)`) needed no change — it already routes through the box (`defer(Ꮡt.Add, n, ref ᒐ)`), binding the receiver address at defer time exactly like Go. Whole-stdlib reconvert diff: **zero files** — no stdlib function composes a capture-mode-boxed param with a closure today, so the composition is user-code-facing and was guard-discovered. (Guarded by the `CaptureModeParamClosure` behavioral test — four compositions with write-visibility checks in both directions: a closure read that must see the callee's later write, a closure write the callee must observe (and vice versa), a deferred closure reading return-time state, and a deferred method value whose writes a sibling deferred observer reads; each output-compared vs Go, with the caller's copy proven untouched. Under the pre-fix snapshot emission all four compiled and produced wrong values.)

Entry-time boxing extends to a **function literal's own value parameter** — the original coverage walked only `*ast.FuncDecl` params, so `f := func(t Tally, m int) {…; t.Add(m); …}` rendered the raw `Tally` value against `Add`'s only `ж<Tally>` receiver form (CS1929). The escape pass marks literal params with the same one-narrow-predicate check as declaration params (walking `FuncLit` nodes **before** the define walk, so a mixed `t, y := …` re-use cannot pre-empt the verdict; a leaked-but-not-capture-mode param keeps its historical unboxed emission via the same declaring-ident re-verification). The literal's signature takes the incoming value under the `ʗp` name and its **first block statement** is the boxed re-declaration — the exact preamble form, injected before the single-return collapse (which it thereby suppresses, correctly keeping the body a block):
```csharp
var f = (Tally tʗp, nint m) => {
    ref var t = ref heap(tʗp, out var Ꮡt);
    t.total++;                    // body writes hit the boxed storage…
    Ꮡt.Add(m);                    // …the same storage the callee mutates
    return (t.total, t.log);
};
```
This applies uniformly to every literal form: an assigned literal, a call argument, a `defer func(t Tally) {…}(x)` / `go …` argument-passing target (each deferred/goroutine run boxes its own copy at entry), and — unlike the variadic prologue, which excludes them — an **IIFE**, whose names-only parameter list emits the `ʗp` name so the rebinding composes with the delegate cast. A literal with both a variadic tail and a boxed param stacks the two `ʗp` prologues (variadic slice first, matching the declaration preamble order). A **nested closure** over the literal's boxed param takes the box-ref route (never a value snapshot, which compiled but orphaned the callee's writes — `var tʗ1 = t; tʗ1.Add(9)` lost both directions of write-visibility), while the literal's **own body** keeps the plain ref-alias renders above (`t.total++`, not `Ꮡt.Value.total++`): a box-ref var whose declaring literal is the lambda currently being converted renders plain, since its box and alias are locals of that very lambda — only genuinely nested lambdas read through the box. Whole-stdlib reconvert diff: **zero files** — no stdlib literal calls a capture-mode method on its own value param today, so this is user-code-facing and guard-discovered. (Guarded by the `CaptureModeFuncLitParam` behavioral test — assigned, IIFE, deferred-argument, nested-closure, and variadic-composition shapes, each with write-visibility checked in both directions and the caller's copy proven untouched, output-compared vs Go.)

And it applies when the field belongs to a **pointer local** — `h.s.inc()` where `h` is a `*holder` local and `inc` has a pointer receiver. A pointer local holds the box `ж<holder>` directly, so the value `~` dereference of the field (`(~h).s`) is an rvalue; the `[GoRecv]` method needs an addressable receiver (CS1510 on the generated `ref`). The field's box address is taken instead — `h.of(holder.Ꮡs).inc()` — binding the `ж` overload. (A pointer *parameter* is deref-aliased to a value, so `p.s.inc()` already works without this and is left alone. This is the form runtime uses for `(*c).gp.set(…)` / `.cas(…)` in coro.)

Finally, the same rvalue problem occurs when the field belongs to a pointer reached through *another field* — `o.h.wait.add(…)` where `o.h` is a `*holder` field and `wait` is a value (atomic) field. `o.h` dereferences to an rvalue, so `(~o.h).wait` is not addressable. The receiver is routed through the box-field accessor `o.h.of(holder.Ꮡwait)`, which aliases the **real** field storage — *not* a `Ꮡ(value)` copy, which compiles but silently boxes a copy so the atomic write is lost (a behavioral bug, not a compile error). Both the explicit address form (`&o.h.wait`) and a pointer-receiver method call on the field are routed this way. This is deliberately scoped to a base that is itself a *field selector*: a bare-ident base is the method's own receiver or a deref'd pointer *parameter* (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&` would emit `Ꮡf.of(…)` but a value-ref receiver has no `Ꮡf` box) or a pointer *local* (handled above). (Guarded by the `AtomicFieldThroughPointer` behavioral test — a mutate-then-read proves the real field is updated, not a copy; runtime exercises this for atomic fields reached through pointer chains such as `sgp.g.selectDone.CompareAndSwap` and `gp.m.mLockProfile.recordLock`.)

The base may also be a pointer **rvalue** — a pointer-returning **call** (`getg().schedlink.set(…)`, `q.tail.ptr().schedlink.set(…)`, `Δp.chunkOf(ci).scavenged.setRange(…)`, `getg().m.p.ptr().wbBuf.get2()`) or a pointer **element index** (`batch[i].schedlink.set(…)`). Go auto-derefs the pointer to reach the value field, so the converter renders the read as `(~rvalue).field`; the `~` deref is an rvalue, so a pointer-receiver method on it cannot bind (`CS1510` on the generated `ref`). Unlike a deref-aliased *parameter* (whose box is `Ꮡp`) or a *field* deref (handled above), the call/index value **already is** the `ж<T>` box, so the receiver is materialized straight through it via the box-field accessor — `getg().of(g.Ꮡschedlink).set(…)`, `batch[i].of(g.Ꮡschedlink).set(…)` — never a `Ꮡ(value)` copy (which would lose the write). The routing is scoped to a base that is **not** an ident and **not** a field selector (those are the param/receiver/local/field cases above) and is **not a type conversion**: a conversion `(*T)(p)` renders as a C# *cast* (`(ж<T>)(uintptr)(…)`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner operand, so a pointer-reinterpret keeps its existing `Ꮡ(…)` form (the runtime-unsafe S1 territory). (Guarded by the `PointerRvalueFieldReceiver` behavioral test — a pointer-receiver method on a value field reached through a returning call, a method-call chain, and a pointer-element index, each with write-through verified; runtime exercises this for `guintptr.set` via `getg()`/`batch[i]`/`q.tail.ptr()`, `pallocData.setRange` via `chunkOf`, and `wbBuf.get2`/`discard` via `getg().m.p.ptr()`.)

**A TYPE-ASSERTION base is a pointer rvalue too (2026-07-31).** The shape list that admits a base into
that box-field routing enumerates ident / selector / call / index / star, and a type assertion is none
of them — so `&c.(*UDPConn).conn` (net `udpsock_test`, reaching the promoted `conn.Write` through an
asserted `PacketConn`) dropped to the `Ꮡ(value)` copy-box fallback and named a `.conn` member that
`ж<UDPConn>` does not have (**CS1061**; had it bound, it would have written into a copy). The list is
about C# **precedence**, not about which node kinds have happened to come up: a base whose rendering is
postfix chains `.of(…)` cleanly, and a type assertion always renders as the postfix `c._<ж<UDPConn>>()`,
which *is* the box. Only the type-CONVERSION `CallExpr` stays excluded, for the cast-precedence reason
stated above. `exprIsValueFieldOfPointerRvalue`, the sibling predicate that decides the *routing*,
already accepted a type assertion through its default arm — so the two now agree rather than one
routing a shape the other could not render. This is the address-of copy-boxing family's next uncovered
**base** shape; the pattern of that family is that each fix covers one base shape, so the next
uncovered one is worth looking for rather than waiting for. (Guarded by the
`PointerRvalueFieldReceiver` extension — `iface.(*node).s.set(55)`, with the write read back through
the original pointer.)

It has a **second consumer, in an already-banked package**, found by the validated sweep rather than
predicted: `compress/flate`'s `flate_test.go` reaches `dict.availWrite()` through an asserted
`*decompressor`, and the old emission copy-boxed it —

```csharp
Ꮡ((~r._<ж<…decompressor>>()).dict).availWrite()                      // copy
r._<ж<…decompressor>>().of(…decompressor.Ꮡdict).availWrite()         // the real field
```

— which `flate` survived only because `availWrite` is a **read**. That is this family's signature
exactly: the copy gives the right answer until someone writes through it, and the documented
"faithful for reads" caveat is a latent wrong answer with a timer on it. `compress/flate` re-validates
at its banked 64/64 with the corrected emission.

The bare-ident-base exclusion above holds **only for `[GoRecv] ref` methods** (which bind on the addressable value alias directly). A **direct-ж** (box-receiver) method — `func (s *scavengeIndex) find(…)` and the like, emitted with a `ж<T>` receiver — needs the *box*, so calling it on a value field-chain rooted at a deref-aliased pointer **parameter or (direct-ж) receiver** is `CS1929`: `Δp.scav.index.find(force)` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock.Load()` (root `mp`, a `*m` parameter), `h.userArena.readyList.remove(s)`. These are routed through the box-field accessor too — `Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).find(force)` — never a `Ꮡ(value)` copy (which would lose an atomic write). The `&`-machinery recurses through the value field-chain to the param/receiver box: `&Δp.scav.index` builds `Ꮡp.of(…).of(…)`, where the box base is the **raw** parameter name (`Ꮡp`, not the shadow-renamed `ᏑΔp` — a deref param `p`→`Δp` is `ref var Δp = ref Ꮡp.Value`, box `Ꮡp`). The routing is gated to direct-ж so a `[GoRecv] ref` method on the same chain keeps binding directly (no churn); a receiver root additionally requires the *enclosing* method to be direct-ж (only then does its receiver box `Ꮡrecv` exist). (Guarded by the `FieldChainBoxReceiver` behavioral test — a direct-ж method on a value field-chain rooted at a pointer parameter and at a direct-ж receiver, both with write-through verified; runtime exercises this pervasively for `scavengeIndex`/`mSpanList`/`timers` methods and `m.trace` atomic fields.)

For the **receiver-root** case, the enclosing method only *becomes* direct-ж through the capture-mode pre-pass's transitive fixpoint: a pointer-receiver method that calls a direct-ж method on a value field-chain of its own receiver — `func (p *pageAlloc) free(…) { … p.scav.index.free(…) }` — is promoted to direct-ж so its receiver box `Ꮡp` exists for the routing above. This detection walks the **full** value field-chain `recvName.f1.…fn.method` (every hop a value, non-pointer field), not just one level: `p.scav.index.free(…)` roots `free` at the receiver `p` through two value fields (`scav`→`index`). A one-level chain (`b.u.Load()` on an embedded atomic) was already detected; the multi-level walk generalizes it. A pointer field anywhere in the chain stops the walk — that subexpression is already a box and roots the call elsewhere (the pointer-field paths above), so it must not trigger promotion. The promotion is transitive: once `pageAlloc.free` is direct-ж, its caller `func (h *mheap) freeSpanLocked(…) { … h.pages.free(…) }` is in turn promoted (now calling a direct-ж method on `h.pages`), and so on up the call graph until a root holding the value through a real box/pointer. (The multi-level receiver-root promotion is covered by the `FieldChainBoxReceiver` test's `deep.bumpDeep` case — `d.mid.c.inc()`, a direct-ж `inc` on a two-level value field-chain of a receiver with no other direct-ж trigger, write-through verified; runtime exercises it on `pageAlloc.free`/`freeSpanLocked`.)

## `unsafe.Alignof` / `unsafe.Offsetof` name a TYPE, resolved through `go/types`
Go defines `Sizeof`, `Alignof` and `Offsetof` against the **static type** of their operand — and never
evaluates that operand (all three are compile-time constants for any type of non-variable size). golib
matches that shape: `Alignof(Type, string? fieldName = null)` and `Offsetof(Type structType, string
fieldName)` take a `System.Type`, so the converter has to turn the Go operand into a type argument.

It now does that from `go/types`: **`unsafe.Alignof(x)` emits `@unsafe.Alignof(typeof(T))`** for `T` the
C# rendering of the operand's static type — one rule for *every* operand shape, because Go's
`Alignof(s.f)` is the required alignment of the **field's own type**, which is exactly what golib's
two-argument overload resolves to anyway. **`unsafe.Offsetof(s.f)` emits `@unsafe.Offsetof(typeof(S),
"f")`**, where `S` comes from `types.Selection.Recv()` with any implicit pointer dereference stripped,
and a *promoted* field is walked down its embedding chain so the offset is measured against the struct
that declares it (Go's rule: relative to the immediately enclosing struct). The field name is the
emitted identifier with any keyword escape removed, since reflection sees `@out` as `out`.

The shape was previously derived by splitting the **converted C# text** on `.` and reading the pieces as
if they were a Go field selector — one part meant `x`, two meant `s.f`, anything else warned and fell
through to an emission that cannot compile. That mistakes any dotted *rendering* for a selector and
corrupts every operand that is not literally an identifier or a one-level selection: a conversion
operand renders with a leading cast, so `unsafe.Alignof(uint32(0))` became `(uint32)0.GetType()`, which
C# parses as `(uint32)(0.GetType())` — `CS0030: Cannot convert type 'System.Type' to 'uint'`, and the
sole build blocker on `crypto/md5` (its `benchmarkSize` alignment probe); a `ж` dereference `Ꮡx.Value`
read as struct `Ꮡx` with field `Value`; and a two-level `cpu.X86.HasAVX` was rejected outright.
`.GetType()` was also the wrong instrument on its own terms — it reports the **dynamic** type of a boxed
or interface-typed operand where Go uses the static one, and it evaluates the operand, which Go does
not. (`unsafe.Sizeof` was unaffected: it emitted the generic `@unsafe.Sizeof(x)`, whose type argument C#
infers.) Guarded by the `UnsafeOperations` behavioral
test, extended with a conversion operand, an index operand, a selector through a pointer, a two-level
selector, and a field whose name is a C# keyword; all output-compared vs `go run`.

**This shape is now the FALLBACK, not the normal path** — expression sites fold to the constant (next
section), and only a variable-size operand still reaches the `typeof(T)` emission. The promoted-field
rule stated above is also *wrong about Go*, and the fold supersedes it: Go measures a promoted field
against the **operand** struct, not against the struct that declares it.

## `unsafe.Sizeof` / `Alignof` / `Offsetof` FOLD to a constant at expression sites
Go computes all three at compile time from the operand's **static type**, never evaluates the operand,
and yields a **typed `uintptr` constant** for any operand type of non-variable size. *Declaration*
sites have always emitted that constant — `internal static uintptr offsetX86HasAVX => /*
unsafe.Offsetof(cpu.X86.HasAVX) */ 66;` in `runtime/cpuflags.cs`. **Expression sites now emit the same
form**, so one Go construct has one behavior:

```go
var hdr Header32
data := make([]byte, unsafe.Sizeof(hdr))
// …
f.Type = Type(bo.Uint16(data[unsafe.Offsetof(hdr.Type):]))
```
```csharp
var data = new slice<byte>((nint)(/* unsafe.Sizeof(hdr) */ (uintptr)52));
// …
f.Value.Type = ((Type)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Type) */ (uintptr)16)..]));
```

The value comes from `go/types`, which folds against the `types.Sizes` for the **loaded target
`GOARCH`** — the Go compiler's own layout rules — so the emitted number is what the *Go* program
computes, not a measurement of the emitted C#. The literal keeps its `uintptr` type because Go's
constant is typed: a bare number would let `uadd := unsafe.Sizeof(*t)` infer C# `int`, and an `int`
*variable* has no implicit conversion back to `nuint` (`internal/abi`'s `FuncType.InSlice` hands it to
a `uintptr` parameter — CS1503). The cast is inert everywhere else: C#'s constant-expression conversion
would have bound a bare literal anyway, and a cast binds tighter than every binary operator, so no site
needs extra parentheses.

Three things this fixes, beyond removing a reflection call from a construct Go settles at compile time:

- **A latent throw.** golib's `Sizeof` rides `Marshal.SizeOf<T>`, which **throws** for a non-blittable
  `T` — and a converted Go struct is non-blittable as soon as it holds a `slice<T>`, `@string`,
  interface, or `ж<T>` field. `debug/elf`'s `Header32` holds an `array<byte>` (a `byte[]` inside), so
  its `unsafe.Sizeof(hdr)` answered **56** where Go says 52 (a 4-byte-too-long header read), and the
  reader's own `Section`/`Prog` — embedded struct + `io.ReaderAt` + `ж<SectionReader>` —
  **throw** `ArgumentException: … cannot be marshaled as an unmanaged structure`. `runtime.mapiterinit`'s
  `unsafe.Sizeof(hiter{})/goarch.PtrSize != 12` guard sits in front of every map range, and `hiter`
  holds four `ж<…>` fields.
- **Semantic fidelity.** A reflection answer measures the *CLR-marshalled* layout, which differs from
  Go's whenever golib's field representation differs — `string` is 16 bytes in Go/amd64, `@string` is a
  managed struct. `debug/elf` uses these values as **on-disk format offsets**; they must be Go's.
- **Promoted fields.** Go measures `unsafe.Offsetof(e.count)` against the **operand** struct: with
  `count` at 8 inside an embedded `Padded` that itself sits at 8, the answer is **16**. The reflection
  form could see only one hop of the embedding chain; `go/types` folds the whole path.

A **variable-size** operand still emits the run-time form (with a converter warning naming the site),
because Go itself does not fold it: since Go 1.18 the operand may be **type-parameter-typed**, and the
call is then not a constant. Four such sites exist in the stdlib — `slices.Compact`
(`unsafe.Sizeof(a[0])` on `S ~[]E`), `internal/saferio` (`unsafe.Sizeof(v)` on `E`), and
`runtime/minmax` (×2) — and they are why golib's `@unsafe` run-time forms are retained rather than
deleted.

Measured over the full stdlib (seeded A/B reconvert, Go 1.23.1, `windows/amd64`): **262 expression
sites folded across 61 files in 16 packages** — `runtime` 129, `debug/elf` 70, `syscall` 17,
`internal/poll` 13, then a long tail; declaration sites byte-identical. Design record:
[`docs/phase4/DESIGN-unsafe-constant-folding.md`](../phase4/DESIGN-unsafe-constant-folding.md). Guarded by
`UnsafeOperations`, extended with all three builtins in call-argument, arithmetic, comparison,
assignment and compound-assignment, and `make`-size positions, over structs whose Go layout is
padding-, embedding- and array-sensitive (and non-blittable once converted); output-compared vs
`go run`.

## The run-time `unsafe.Sizeof` answers through Go's layout rule, not the CLR's marshaller

The folding arc above removed the reflection call from every site Go itself settles at compile time,
and named what was left: the handful of operands whose type is a **type parameter**, which Go's own
spec calls variable-size and does not fold either. Those kept riding `Marshal.SizeOf<T>` — and there
the latent throw the folding arc had just designed around was not latent at all, because a type
parameter binds at run time to exactly the shapes `Marshal.SizeOf` refuses: a generic type
(`ж<Section>`, `slice<T>`) raises *"The specified Type must not be a generic type"*, and a struct
holding a managed reference raises *"cannot be marshaled as an unmanaged structure"*.

Three packages died on it at once, all through the same one line — `internal/saferio.SliceCap[E]`,
which asks `unsafe.Sizeof(*new(E))` only to decide how large a chunk it may pre-allocate:
`debug/macho` (E = the `Load` **interface**), `internal/xcoff` (E = `ж<Section>`), and
`go/internal/gccgoimporter` through `debug/elf` (E = `ΔSection`, a struct over an embedded header, an
`io.ReaderAt` and a `ж<SectionReader>`).

So the run-time form now answers through **`GoReflect.GoSizeOf`** — the same Go-layout walk the
reflection bridge already stamps into a descriptor's `Size_`, and the same one `reflect.Type.Size()`
reads:

```csharp
public static uintptr Sizeof<T>(T x) {
    nint size = GoReflect.GoSizeOf(typeof(T), GoReflect.ArrayDimsOfValue(x));
    return size >= 0 ? (uintptr)size : (uintptr)Marshal.SizeOf<T>();
}
```

`typeof(T)` is Go's rule verbatim — `Sizeof` is defined against the operand's **static** type, and the
converter's inferred type argument is that type. Dims come from the live value because `array<T>`
carries its Go length in the instance, not the type. `Marshal.SizeOf` stays as the fallback for the
shapes `GoSizeOf` declines (`-1`: an array whose length nothing can reveal, a struct holding such a
field), so no operand that resolved before stops resolving.

This makes the answer *correct* as well as non-throwing, which matters beyond the three packages:
`Marshal.SizeOf` reports a **bool** as 4 bytes where Go says 1, so any struct containing one was
already measured wrong — silently, at the sites the folding arc could not reach. A Go size now has
one definition in the runtime rather than two (the unification `golib/GoReflect.TypeLayout.cs` had
recorded as deferred pending a named consumer). Corpus reach is small by construction: **7 run-time
call sites**, against 283 folded ones.

## Converting a Go pointer to `unsafe.Pointer`
`unsafe.Pointer` is the golib class `unsafe_package.Pointer : ж<uintptr>` (a numeric address wrapper). A `uintptr`/`unsafe.Pointer` argument converts through the implicit `uintptr ↔ Pointer` operators, but a **Go pointer** argument (`*T`, emitted as the managed box `ж<T>`) has no such conversion — a plain cast `(@unsafe.Pointer)(ж<T>)` is `CS0030` (when `T` is unrelated to `uintptr`) or a runtime `InvalidCastException` (the base→derived downcast `(@unsafe.Pointer)(ж<uintptr>)` compiles but the object is a plain `ж<uintptr>`, not a `Pointer`). So `unsafe.Pointer(ptr)` for a pointer `ptr` is emitted through the referent-**retaining** mint (2026-08-27, the I5 ruling — it was `@unsafe.Pointer.FromRef(ref (box).Value)` before that, which flattened the pointer to a transient number in a fresh box):
```go
func (u *UnsafePointer) Load() unsafe.Pointer { return Loadp(unsafe.Pointer(&u.value)) }
```
```csharp
public static @unsafe.Pointer Load(this ж<UnsafePointer> Ꮡu) {
    ref var u = ref Ꮡu.Value;
    return (uintptr)Loadp(@unsafe.Pointer.FromBox(Ꮡu.of(UnsafePointer.Ꮡvalue)));
}
```
> `FromBox` carries the same transient numeric address the `FromRef` form produced (still **not GC-stable** — the caveat of every `unsafe.Pointer`-as-`uintptr` use) **and the source box itself** (`Pointer.RetainedSource`, the `NativeBox` §4 retention pattern). The retained referent is what makes the bare-`unsafe.Pointer` atomic primitives real: `StorepNoWB`/`Loadp` were silently non-functional — `*(*unsafe.Pointer)(ptr) = val` wrote the argument box's own `uintptr` slot, never the memory it names (`internal/runtime/atomic`'s `TestStorepNoWB`, the 14/15) — and now recover the referent (`StoreThrough`/`LoadThrough`, dispatching through golib's internal `IUntypedSlotAccess` seam on `ж<T>`) and reach the very slot the pointer names. A pointer with no recoverable referent panics **by name** instead of losing the write. A nil box mints the 0 address rather than `FromRef`'s nil-deref panic — Go's `unsafe.Pointer(nil)`, matching the pointer-parameter emission's existing behavior. The `*unsafe.Pointer` siblings (`Casp1`/`storePointer`/`casPointer`) were always correct by signature — they carry an aliasing `ж<unsafe.Pointer>`. A pointer **receiver** keeps `FromRef` (a `this ref T` receiver has no box to retain) — the one non-retaining mint, a recorded residual, loud at the primitives if ever reached. (The reinterpret pattern `*(*U)(unsafe.Pointer(&x))` is handled separately and is not affected. Guarded by `GolibTests`' `UnsafePointerRetentionTests`, failing-first against the `FromRef` emission, plus the banked `internal/runtime/atomic` suite itself.)

**A NIL pointer converts to address 0, not a throw.** golib's `ж<T> → uintptr` (and `ж<T> → void*`) operator takes the pointed-to storage's address via a `fixed` block — but a **nil** box has no storage to pin, so `&value.Value` dereferences it and throws. Go's `uintptr(unsafe.Pointer(nil))` is simply **0**, and the syscall wrappers pass nil pointers exactly this way: `syscall.Write` hands `writeFile` a nil `*Overlapped` for a synchronous write, then passes `uintptr(unsafe.Pointer(overlapped))` (= 0) to the `SyscallN` trampoline. The operators now return `0`/`null` for a nil box before pinning — so any converted `os.Stdout.Write` (hence `fmt.Println`) whose stdout is a pipe reaches the OS `WriteFile` and prints, instead of crashing on the nil-`overlapped` argument. (Guarded by the `NilPointerUintptr` behavioral **output** test — `uintptr(unsafe.Pointer(nilPtr)) == 0` and a non-nil control, vs Go.)

**A PACKAGE-SCOPE `uintptr(unsafe.Pointer(...))` must not crash the converter.** The `unsafe.Pointer` conversion path has a special case that rewrites `unsafe.Pointer(arg)` into the ref-based extension call `(uintptr)@unsafe.Pointer.FromRef(ref arg)` when the *enclosing function is a pointer-receiver method* whose single argument aliases the receiver (pointer-receiver methods are emitted as ref-based extension functions, so the pointer must be reconstructed from a `ref`). That test read `v.currentFuncSignature.Recv()` **unconditionally** — but a **package-level** `var` initializer is converted with no enclosing function, so `currentFuncSignature` is `nil` and the receiver probe nil-panicked *during conversion* (`go/types.(*Signature).Recv`). The special case can never apply at package scope — there is no receiver — so the fix guards it with `v.currentFuncSignature != nil` (the same idiom `convUnaryExpr` and `captureModeOperations` already use), which falls through to the ordinary emission: `var gPtr uintptr = uintptr(unsafe.Pointer(&global))` → `(uintptr)Ꮡglobal` (it read `(uintptr)new @unsafe.Pointer(Ꮡglobal)` until the dead-wrapper peephole below), identical to the in-function non-receiver form the corpus already produces. This is what blocked `cmp`'s Phase-4 validation: `cmp_test.go` declares `var nonnilptr uintptr = uintptr(unsafe.Pointer(&negzero))` and `var nilptr uintptr = uintptr(unsafe.Pointer(nil))` at package scope, and the converter crashed before emitting a line.

**A null `Pointer` *reference* (from `unsafe.Pointer(nil)`) also converts to 0.** Distinct from the nil-*box* case above: the untyped `nil` literal in `unsafe.Pointer(nil)` renders as `(@unsafe.Pointer)default!`, and `default` of the reference type `Pointer` is a C# **null**, not a `ж<T>` box. golib's `Pointer → uintptr` operator then dereferenced `value.Value` and threw `NullReferenceException` — even though `Pointer`'s own `==`/`!=` operators already treat a null reference as nil (`value?.IsNull ?? true`). The `uintptr` and `void*` conversion operators now honor that same null-tolerance (`value is null ? 0/null : value.Value`), so `uintptr(unsafe.Pointer(nil))` yields 0 whether the nil arrives as a nil box or a null `Pointer` reference. (Both the package-scope crash and this null-reference conversion are guarded by the extended `NilPointerUintptr` behavioral **output** test — package-level `var gPtr = uintptr(unsafe.Pointer(&global))` (non-zero) and `var gNil = uintptr(unsafe.Pointer(nil))` (0), vs Go; the pre-fix converter panics on the package-scope declaration and, once past that, the pre-fix golib NREs on `gNil`.)

**A pointer to a Go fixed array resolves to the array's DATA, pinned across the FFI call.** The other half of that `fixed`-block operator is wrong for a `ж<array<T>>` (`unsafe.Pointer(&arr)` where `arr` is a Go `[N]T`): `&value.Value` is the address of the golib `array<T>` **struct wrapper** — which holds the backing `T[]` as a reference field, an offset, and a length — *not* the address of the array data, and the `fixed` releases it before the operator even returns. A native syscall handed that address writes over the wrapper's fields (clobbering the `T[]` reference), so a later `buf[i]` reads through a corrupted array and faults. This is exactly the go-isatty MSYS/cygwin-pipe probe: `IsCygwinTerminal` fills a `[262]uint16` with a `FILE_NAME_INFO` via `GetFileInformationByHandleEx(…, uintptr(unsafe.Pointer(&buf)), …)`, then reads `l := *(*uint32)(unsafe.Pointer(&buf))` (the `FileNameLength`) and slices `buf[2 : 2+l/2]` — the garbage `l` drove `array<uint16>.get_Item(Range)` off the end (`AccessViolationException`). The converted **fatih/color** sample went **empty on a pipe** because of it: fatih/color's `NoColor` probe evaluates `!isatty.IsTerminal(fd) && !isatty.IsCygwinTerminal(fd)`, so only a *pipe* (where `IsTerminal` is false, unlike a console, and `GetFileType` is `FILE_TYPE_PIPE`, unlike a file) reaches the faulting FFI call — file-redirect and real-console output were fine, matching the observed matrix. The operators now special-case a value that is a Go fixed array — an `IArray` that is **not** an `ISlice` (a `slice<T>`'s `&s` is its header, exactly as in Go, so slices stay on the value-slot path) — and return the pinned address of element 0 of the backing `T[]`, via a `PinnedBuffer` (a `GCHandle.Alloc(…, Pinned)`) cached on the box. The pin lives for the box's lifetime — so the syscall write lands in the real backing array and every managed read afterward (the `l` reinterpret and the `buf[2:]` slice) observes it — and is released when the box is collected (the `PinnedBuffer` finalizer frees the handle). This is a golib-only change (no emitted-code difference); the array-buffer-to-syscall pattern that previously faulted now runs, while non-array pointers keep the existing transient `fixed`-address behavior byte-for-byte. (Guarded by the `FixedArrayBufferPointer` behavioral **output** test — the `*(*uint32)(unsafe.Pointer(&buf))` read-back idiom, the array still readable through its own indexer afterward, and address-stability across repeated conversions, vs Go; end-to-end, the converted fatih/color sample now prints byte-identically to `go run` through a pipe.)

**EVERY managed address handed to native code is pinned for the pointer's lifetime — a `fixed` block cannot outlive its own statement (2026-08-03, r38-os-fin).** The entry above pinned the ONE case that had been proven to corrupt memory; the general case was left with the transient `fixed` address, under a soundness note in `syscall/dll_windows.cs` that called the window between capture and the trampoline's `calli` "short and allocation-free". It is neither, for a BLOCKING syscall: the window stays open for as long as the kernel takes. `os`'s `TestPipeEOF` parks in `ReadFile` on a pipe for 10 ms per read while the rest of a parallel suite allocates around it, and a gen0 collection in that window moves both the `*uint32` byte-count box `syscall.Read` passes and the caller's read buffer — measured directly, a `heap(new uint32(), out var Ꮡdone)` box and a `Ꮡ(buf, 0)` element pointer BOTH report a different address after one forced collection. The kernel then writes to neither: `done` stays 0, `syscall.Read` returns `(0, nil)`, and `internal/poll`'s `FD.eofError` turns that into a premature `io.EOF`. That is the whole of the row characterized as *"`bufio.Reader.ReadBytes` over a converted `os.Pipe` returns a premature `io.EOF`, and only under parallel load"*, and it explains its measured shape exactly — monotone in the parallelism level (0 of 4 at `-parallel 1`, 1 of 3 at 4, 5 of 5 at 8, 100% at the default), because more threads means more allocation means more collections inside the same 10 ms window; independent of finalizers, which a control had already ruled out. The buffer's half of the same defect writes 4 KB into freed heap, which is the moving-site `ExecutionEngineException` recorded beside it. **golib now pins before it reads an address**: `ж<T>`'s `uintptr`/`void*` operators call `EnsureStableAddress`, which takes a lifetime `GCHandle` on the ROOT storage the pointer names — a standard heap box pins its own value slot, an element reference pins the canonical backing array, a field reference recurses to the allocation that contains the field — on exactly the terms `pinnedArrayData` already used for the fixed-array case, and released when the box is collected. The enabling change is that a standard heap box's value STORAGE is now a one-element array for a `T` that contains no references (`ж<T>.m_slot`): a box is a class with reference fields and `GCHandle` refuses to pin anything that contains pointers, so the value had nowhere pinnable to live. It is allocated EAGERLY and never migrated — `heap<T>(out ж<T>)` hands the caller a `ref` alias before any address is taken (`ref var done = ref heap(new uint32(), out var Ꮡdone)`), so moving the storage on first address-take would leave that alias on the abandoned copy, which is this very bug one level down. A `T` that DOES carry references gets no slot and keeps the transient address: its C# layout is not a native layout either, so no syscall can meaningfully be handed its address — the change is additive, and `RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is a JIT constant, so neither the branch nor the allocation costs such a box anything. This also makes Go's unsafe.Pointer RULE 3 (pointer arithmetic through `uintptr`) sound, which it silently was not. golib-only — no emitted-code difference. (Guarded by `src/tests/GolibTests/NativeAddressStabilityTests.cs`, a neutered-fix control across all four box kinds plus the reference-bearing negative case: with `EnsureStableAddress` removed every address assertion fails on the first forced collection. Operationally, `os`'s residual went from 13 rows to 3 in one change — `TestPipeEOF` and the whole child-stdout family, whose empty child output was the same premature EOF read through `exec`'s pipe.)

**A STRUCT handed to the kernel by address must be blittable — otherwise the wrapper is hand-owned.** The previous entry fixes a pointer to a fixed ARRAY; this is the same problem one level up, for a whole struct, and it has no golib-level answer. A generated syscall wrapper passes `uintptr(unsafe.Pointer(&s))` and the kernel writes the NATIVE record at that address. That is safe only when the converted struct's C# layout matches the native one — which it does for a scalar/handle struct (`SecurityAttributes` is two `uint32`s and a `uintptr`, so `CreatePipe`, and therefore `os.Pipe`, works through the ordinary converted wrapper), and never when a field is a golib `array<T>` (Go's inline `[N]T`) or a `ж<T>` (Go's pointer field): both are MANAGED REFERENCES occupying one word where Windows expects inline bytes or a raw address. The kernel then writes the native-sized record over a smaller managed object — corrupting the heap past its end and leaving fabricated object references in the reference-typed fields. **It does not fail at the call.** It fails at the next read of one of those fields, arbitrarily far away and with a diagnostic that names the wrong code: `syscall.GetTimeZoneInformation` writes 172 bytes of `TIME_ZONE_INFORMATION` (two inline `WCHAR[32]` name buffers) over a ~64-byte `Timezoneinformation`, and the crash surfaces as an `ACCESS_VIOLATION` inside `slice<ushort>..ctor` on `zoneinfo_windows.go`'s next `syscall.UTF16ToString(z.StandardName[:])` — so every converted program calling `time.Now().Weekday()` / `Location()` / `Local` on Windows died, in `time`, with no mention of `syscall`. The remedy is per-wrapper hand-ownership, not a converter or golib change: a `manualConversionFuncs` entry turns the generated wrapper into a placeholder, and a `*_impl.cs` companion supplies a blittable `[StructLayout(LayoutKind.Sequential)]` mirror (`fixed` buffers for the inline arrays, so they stay inline), a direct `[DllImport]`, and an explicit field-for-field copy back into the converted struct at the boundary — see `src/core/syscall/zsyscall_windows_impl.cs`, and `exec_windows.cs`'s `StartProcess`/`_STARTUPINFOEXW` for the first instance. **Verify at VALUE level, never at fault level:** a mirror with the wrong offsets returns garbage without crashing, so "it no longer faults" proves nothing. (Guarded by the `LocalTimeZone` behavioral **output** test, which compares the zone ABBREVIATION — which comes from the name buffers — the offset in seconds — from `Bias`/`StandardBias`/`DaylightBias` — and a fixed instant rendered through the local zone — which selects between them via `StandardDate`/`DaylightDate` — against `go run`.) A census of `src/core/syscall` finds 32 non-blittable converted structs and eleven wrappers passing one by address (an earlier count of ten collapsed the `findFirstFile1`/`findNextFile1` pair into a single row); the ones not yet fixed are latent and board-rowed rather than fixed speculatively, since each needs its own value-level verification. **`src/core/syscall` is not the class's boundary, and the class runs in BOTH directions.** `internal/syscall/windows` holds six more wrappers of the same shape, and where the kernel merely READS the record go2cs hands it the diagnosis above inverts: nothing is written over the managed object, so there is no delayed corruption — instead the native reader picks each field out of the C# storage at the NATIVE offset, and because the CLR auto-layouts a struct containing references (grouping them first) an ordinary integer field ends up under a pointer field. `internal/syscall/windows.SHARE_INFO_2` is 48 managed bytes against 56 native ones, and netapi32 reads `shi2_path` at offset 40 — which in C# is `MaxUses` (1) followed by `CurrentUses` (0) — so it dereferences the pointer value `1` and the process dies AT the call with `0xC0000005`; the same reordering makes `shi2_passwd` an 8-byte over-read past the end of the record. This shape is loud and immediate rather than distant, which makes it easy to misread as a regression in whatever changed most recently. Note also what is NOT the defect: a managed reference standing in for an `LPWSTR` is survivable on its own — it is a readable address, and a control that keeps the native field ORDER returns `ERROR_INVALID_NAME` instead of faulting — so the reordering, not the reference, is what a remedy must answer. `os`'s own `readdir` is the worked precedent for the read direction (`src/core/os/windows/dir_windows_impl.cs` walks the kernel's buffer at native offsets rather than reinterpreting it as the managed surrogate). The write direction has no answer yet where the wrapper receives an opaque `*byte`: `Reinterpret` correctly declines to alias a reference-bearing struct as `byte`, and the address route it falls back to has already discarded the managed identity that a field-for-field copy would need.

**Second and third members of the class: `findFirstFile1` / `findNextFile1` (2026-08-01).** The same seam over a bigger record — `WIN32_FIND_DATAW` is 592 bytes with `cFileName[260]` and `cAlternateFileName[14]` INLINE (520 and 28 bytes of storage), where the converted `win32finddata1` carries two one-word `array<uint16>` references — and the first member a real test suite actually *reached*: `path/filepath.EvalSymlinks` → `toNorm` → `normBase` asks `FindFirstFile` for the on-disk spelling of every path element, so the whole `EvalSymlinks` family took the C# test host down mid-run, which silently under-reports every verdict after it as an empty result. Both faces of the corruption appeared from that one package — an `IndexOutOfRangeException` inside `PinnedBuffer` where the clobbered reference still resolved to something (`normBase`'s `UTF16ToString(data.FileName[:])`), and an outright `ACCESS_VIOLATION` in `slice<ushort>..ctor` where it did not (`copyFindData`'s `src.FileName[..]`). Only the two `*1` wrappers are hand-owned: Go itself puts the native-layout boundary exactly there — `syscall_windows.go`'s `FindFirstFile` allocates a `win32finddata1`, calls the wrapper, then `copyFindData`s out — so the public `FindFirstFile`/`FindNextFile` and `copyFindData` above them are pure Go logic and convert faithfully. Two details generalize to the next member of the class: the *name* argument is pinned with a `fixed` block wrapped around the call rather than handed golib's TRANSIENT `ж`→`uintptr` address (`Value` on an element box resolves to a ref INTO the caller's backing array, so `fixed` pins that array and the whole NUL-terminated name stays contiguous behind the pointer for the call); and both inline buffers are copied WHOLE, NULs included, because Go reads them as `UTF16ToString(data.FileName[:])` — which stops at the first NUL — while the SAME `win32finddata1` is reused across every `FindNextFile` of an enumeration, so a copy that stopped at the terminator would leave the previous entry's runes behind it. `Reserved0` (offset 36) is copied verbatim but deliberately not asserted: Windows documents it as the reparse-point tag only when `FileAttributes` carries `FILE_ATTRIBUTE_REPARSE_POINT` and UNDEFINED otherwise, so a reparse-free tree has no stable value to compare — its offset is instead pinned from both sides by the verified `FileSizeLow` (32) and `FileName` (44), which leave it and `Reserved1` the only eight bytes between them. (Guarded by the `FindFirstFileData` behavioral **output** test, which builds a purpose-made tree and compares, per entry, the long name in both ASCII and non-ASCII — `FileName` at 44 — the 8.3 short name — `AlternateFileName` at 564 — the directory bit — `FileAttributes` at 0 — the byte size — `FileSizeLow` at 32 — and a DISTINCT fixed instant stamped per entry — `LastWriteTime` at 20 — plus `ERROR_FILE_NOT_FOUND` for a missing file, the `ERROR_NO_MORE_FILES` end of an enumeration, and `filepath.EvalSymlinks` restoring canonical case through the whole `path/filepath` path, all against `go run`.)

**Fourth member, and the first carrying TWO defects: the sockaddr family (2026-08-11, lane L10).** `net.Listen` on Windows is what forced this one, and it never even reached the seam — it died one layer earlier. Go writes the port in network byte order through a two-byte alias over the raw struct's port field, `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))`, and the auto conversion of that rebuilds an `array<byte>` from a raw address, which materializes `default(array<byte>)` — a LENGTH-ZERO array — so `p[0]` panicked with `index out of range [0] with length 0` (golib `array.cs:280` via `syscall_windows.cs:881`). An `array<T>` is a managed container with its own header, not two inline bytes, so no address reinterpret can ever produce one; the encoders are hand-owned and write the field arithmetically instead, leaving `raw` in exactly the state Go leaves it. Behind that sat the ordinary form of this class: `RawSockaddrInet4`'s `Addr [4]byte` / `Zero [8]uint8` are `array<byte>` managed references, so `unsafe.Pointer(&sa.raw)` names a ~24-byte object with object references where Windows wants a 16-byte `sockaddr_in` with the octets inline — the case golib's own `ж.cs` describes when it explains why a reference-bearing pointee gets no pinnable storage ("such a value's C# layout is not a native layout either, so no syscall can meaningfully be handed its address"). **Two departures from the precedent above are worth cribbing.** The mirror is a LOCAL at each call site rather than a field the way `Timezoneinformation`'s is: a sockaddr's native image is needed for the duration of one call, and a stack buffer is trivially stable for exactly that long, where a managed field's address would need a pin whose lifetime nothing owns. And no new `[DllImport]`/`[LibraryImport]` is declared at all — golib models `unsafe.Pointer` as a box over a plain address (`unsafe`'s `Pointer : ж<uintptr>`, whose `uintptr` operator returns the stored address), so the package's OWN generated `bind`/`connect`/`connectEx` wrappers already accept any address and were never the broken part; handing them the mirror's address reuses their errno handling verbatim and keeps the hand-owned surface to the layout translation, which is the only thing that was wrong. `Getsockname`/`Getpeername` are the exception, and for a precise reason: their generated wrappers take a typed `ж<RawSockaddrAny>` rather than an address, so those two call the package's `Syscall` trampoline directly, mirroring the generated wrappers' error handling. (Guarded by the `SockaddrRoundTrip` behavioral **output** test — `socket`/`bind`/`getsockname`/`listen`/`connect`/`getpeername` on loopback over both IPv4 and IPv6 — which prints kernel-derived values and cross-checks them rather than checking for absence of a fault: the client's `getpeername` must equal the listener's bound address field for field, closing encode → kernel → decode → encode → kernel → decode. Ephemeral ports are never printed, only whether the two ends agree about them, so the output is host-independent.)

**The DECODE joined the family three days later, and only because a CONVERTER capability landed first (2026-08-14, netpoll lane S2b).** `(*RawSockaddrAny).Sockaddr` carries the same port alias as the encoders and panics identically, but L10 left it auto-converted on measurement rather than on effort — its body held the only `ΔSockaddr` casts in the package, so displacing it dropped the three `GoImplement<…>(Pointer = true)` records and `net` minted duplicates (the trap written up in the next paragraph). Once `recordSamePackageImplements` began recording the POINTER method set the records stopped depending on that body, the hand-own became available, and `net`'s ACCEPT path is what wanted it: it decodes the `GetAcceptExSockaddrs` output through this one method, the single route to a `Sockaddr` that the hand-owned `Getsockname`/`Getpeername` do not cover. **Its shape differs from every other member of this class, and that is the transferable part.** The others translate a MANAGED struct into a native image to hand the kernel; this one runs in the opposite direction with no kernel call in it at all — it FLATTENS the managed `RawSockaddrAny` back into the 116-byte native image its fields are a transcription of (`Family` at 0, `Addr.Data` covering 2..15, `Pad` covering 16..115) and hands that to `readNativeSockaddr`, the same decode `Getsockname`/`Getpeername` already route through. Two things force the flatten rather than a field-by-field read: the auto body's `Reinterpret<RawSockaddrAny, RawSockaddrInet4>` cannot alias one reference-bearing struct as another (their managed layouts share no offsets at all), and a `sockaddr_in6`'s 16-byte address spans offsets 8..23 — `Addr.Data[6..13]` **and** `Pad[0..7]` — so a decode written against the managed fields has to know that boundary anyway. Reusing the one native decode keeps that knowledge in a single place. (Guarded by four new lines in `SockaddrRoundTrip` driving the method directly on hand-built `RawSockaddrAny` values: IPv4, an IPv6 case whose address deliberately crosses the `Data`/`Pad` boundary, an AF_UNIX name, and an unknown family that must answer `EAFNOSUPPORT`. The Go side writes those fields BY NATIVE OFFSET, so the test states the layout it depends on instead of assuming it.)

**An ARRAY of native records adds a stride and a size that is both an INPUT and an OUTPUT — `WSAEnumProtocols` / `WSAStartup` (2026-08-16).** Every earlier member of this class passes ONE struct; `WSAEnumProtocols` passes an array of them, and the two things that follow from that are the transferable part. **(1) The size is a byte count the CALLER computed, and it is already native.** Go writes `len := uint32(unsafe.Sizeof(buf))` over its `[32]WSAProtocolInfo` and the converter answers with Go's own arithmetic — `20096`, i.e. 32 × 628, correct as a native size and four fifths of it past the end of the ~3.8 KB managed array the same call hands over (`WSAPROTOCOL_INFOW` ends in `szProtocol[256]` INLINE and nests a `GUID`'s `[8]byte` and a `WSAPROTOCOLCHAIN`'s `[7]uint32`, three inline arrays the conversion collapses to one-word references, so the managed record is ~120 bytes). This is `Process32First`'s dwSize rule — *the size a native call is told must be the NATIVE size* — but it arrives from the caller rather than from the wrapper, so the mirror does not compute it: it SIZES ITS OWN STAGING BLOCK FROM IT, which makes the two agree by construction and needs no cross-check. **(2) The same field is an OUTPUT, and must pass back UNTRANSLATED.** On `WSAENOBUFS` Windows rewrites it with the size the catalog needs, in NATIVE strides, and the Go caller compares that against its own `unsafe.Sizeof` arithmetic — so a boundary that helpfully "corrected" it to managed sizes would be the defect, not the fix. Both directions are native-side arithmetic; the mirror is only a layout translator and touches neither. The record COUNT Windows returns is likewise a count of native records, so the copy-back walks the staging block at `sizeof(NativeWSAProtocolInfoW)` and steps the caller's pointer with `unsafe.Add`, which resolves through the backing array (the mechanism the netpoll `stageBuffers` already relies on for `&o.bufs[0]`). **A third property is new and worth naming: the address was not merely wrong, it was TRANSIENT.** `Ꮡbuf.at<WSAProtocolInfo>(0)` cannot be pinned at all — `PinnedBuffer.PinOnly` refuses a reference-bearing element type — so unlike `Timezoneinformation`, where a stable-but-mislaid-out address was handed over, here no address the box can produce survives the call. That closes the door on any remedy short of a native staging block; there is nothing to pin.

**⚠ And a struct-passing defect can be SILENT because nothing READS the out-struct — `WSAStartup` is the demonstration.** It is the same class over `WSADATA` (408 bytes native, `szDescription[257]` and `szSystemStatus[129]` inline, against a ~40-byte managed `WSAData` whose CLR auto-layout groups its two `array<byte>` references first), it runs inside `internal/poll`'s `InitWSA` once per process that imports `net`, and it has been writing ~408 bytes over that ~40-byte object since the corpus first dialled a socket. Nothing noticed, for one reason only: `InitWSA` discards the `WSAData` it passes. The moment a program READS `data.Description` the corruption surfaces as an `ACCESS_VIOLATION` in `slice<byte>..ctor` — the exact `GetTimeZoneInformation` signature — and it does so BEFORE the `WSAEnumProtocols` on the next line is reached, which is why the two were fixed together and why the guard could not have reproduced the chipped defect without also fixing this one. The lesson generalizes past Winsock: **a passing suite is not evidence that a wrapper of this shape is sound, and neither is the absence of a fault — only a read of the out-struct's far fields is.** (Both guarded by the `WsaProtocolInfo` behavioral **output** test, which asserts only host-INVARIANT properties so Go and the conversion must agree on any machine: the negotiated 2.2 version words and a printable NUL-terminated `Description` for `WSAStartup`; and, because the query names `IPPROTO_TCP` alone, `SocketType == SOCK_STREAM`, `Protocol == IPPROTO_TCP` and an INET address family for EVERY returned entry — three fields that sit past the nested `GUID` and `WSAPROTOCOLCHAIN` whose inline arrays the managed layout collapses, so a record read at managed strides cannot agree with them by luck — plus a printable `ProtocolName` pinning the far end of the record 512 bytes further on, a required size that is a whole number of native records, and the `XP1_IFS_HANDLES` verdict itself, which is what `useSetFileCompletionNotificationModes` → `FD.skipSyncNotif` is derived from.)

**A LINKED output cannot be handed back at all — `GetAddrInfoW` / `FreeAddrInfoW` (2026-08-16).** Every earlier member of this class copies ONE record, or an array of them, into storage the CALLER already owns. `GetAddrInfoW` returns a chain the KERNEL owns: native `ADDRINFOW` records (48 bytes on x64 — four ints, a `size_t`, and three raw pointers) linked by `ai_next`, each pointing at a `sockaddr` whose type is selected by `ai_family`. The wrapper is defective in the established way at both ends — the converted `AddrinfoW` holds `Canonname`, `Addr` and `Next` as MANAGED REFERENCES, so the hints Windows reads are garbage and the `*ADDRINFOW` it writes lands in a reference slot, which is the `Fatal error. 0xC0000005` inside `Syscall6` that crypto/tls's `TestVerifyHostname` measured — but **fixing the call alone would not have been enough, and that is the transferable part.** `net`'s consumer reads the sockaddr THROUGH the result (`(*syscall.RawSockaddrInet4)(unsafe.Pointer(result.Addr))`), and `RawSockaddrInet4`'s `Addr [4]byte` / `Zero [8]uint8` are golib `array<T>` values — a backing-array REFERENCE plus bounds — so reading that struct out of a native `sockaddr_in` fabricates two managed references from address and port bytes, which is the fabricated-reference deref the sha3 arc named and which has no general fix. So the hand-own transcribes the WHOLE chain into managed `ж<AddrinfoW>` boxes and the sockaddr with it, typed by `ai_family` exactly as the native `ai_addr` is, and frees the native chain EAGERLY at the copy — which makes `FreeAddrInfoW` a hand-owned no-op, because nothing native escapes the call and handing a managed object's address to `ws2_32`'s real free would release memory it does not own. Two properties are worth carrying past Winsock. **(1) A managed pointer CAN cross an `unsafe.Pointer` field, through the token table.** `AddrinfoW.Addr` is Go's `syscall.Pointer` (`type Pointer *struct{}`), and the consumer projects it to a scalar and converts the scalar back to a typed pointer — precisely the round trip golib's `ManagedPointerTokens` exists to make work — so the sockaddr box is registered under its own pointer-order token and the field carries that token. Until now the only minter of those tokens was the hand-owned reflection bridge; this is the second, and the shape is the one the table was written for rather than a new use of it. The token must be wrapped in a `Pointer` built over a NATIVE-address `ж<EmptyStruct>`, because the generated named-pointer wrapper's `uintptr` conversion takes the address of the storage its box addresses — which for a native box IS the number handed in, so the token survives the field round trip unchanged. **(2) The token table is WEAK by design, so the strong reference is the hand-own's business.** A `ConditionalWeakTable` keyed on the record box holds each sockaddr for exactly as long as its record is reachable, which is the Go lifetime; without it the box behind a live token could be collected and the consumer's cast would silently fall back to a native-address box over a token — a wild read where the defect had been a loud one. (Guarded by the `LookupServicePort` behavioral **output** test, which asks `net.LookupPort` for `tcp`/`udp`/`tcp4`/`tcp6` services: that is the one reach into this pair needing neither DNS nor a network, and it exercises the hints mirror, the chain copy, the token handoff and BOTH sockaddr flavors — a byte-order slip prints a swapped port rather than failing, so the comparison is at value level. Proven failing-first: with the hand-own removed the guard reports `exit code mismatch: C# -1073741819 vs Go 0`, i.e. the access violation itself.)

**⚠ A `**T` OUT-parameter arrives at the kernel as NULL, silently — a SECOND class beside the layout one, censused at 13 wrappers (2026-08-16).** `ж<T> → uintptr` answers 0 for a box whose `IsNull` is true, and `IsNull` is the VALUE-PEEKING question: for `ж<ж<T>>` — a heap-boxed POINTER, which is what `&p` is when `p` is a nil `*T` — the held reference is null before the call, so the operator reports the box as nil and returns 0. That is deliberate and correct for the case it was written for (`syscall.Write` hands `writeFile` a nil `*Overlapped`, and `uintptr(unsafe.Pointer(nil))` is 0 in Go), and it is silently wrong for the shape every out-parameter takes: the wrapper tells the kernel "I do not want this output", the kernel obliges, the call SUCCEEDS, and the caller reads back the nil it started with. The measured instance is `crypto/x509`'s Windows system verifier: `createStoreContext` calls `CertAddCertificateContextToStore(handle, leafCtx, CERT_STORE_ADD_ALWAYS, &storeCtx)` — whose `ppStoreContext` is documented OPTIONAL — gets `err == nil`, returns a nil `storeCtx`, and `systemVerify`'s next line panics on `(*storeCtx).Store` with a Go-shaped nil-pointer dereference that names golib's `~` operator and not the wrapper three frames back (crypto/tls's `TestQUICHandshakeError`). Note that the 0 is accidentally the SAFER outcome: had the address been passed, the kernel would have written a native pointer into a slot the collector reads as an object reference. So the remedy is the same per-wrapper hand-own as the layout class — receive into a native slot, then build a native-address `ж<T>` from it — never a change to the operator. A census of the emitted wrappers finds **13** of this shape: `CertAddCertificateContextToStore`, `CertGetCertificateChain`, `ConvertSidToStringSid`, `ConvertStringSidToSid`, `DnsQuery`, `_DnsQuery`, `GetFullPathName`, `getQueuedCompletionStatus`, `GetQueuedCompletionStatus` in `syscall`, plus `CreateEnvironmentBlock`, `NetGetJoinInformation`, `NetUserGetInfo`, `NetUserGetLocalGroups` in `internal/syscall/windows` — and two already hand-owned for other reasons (`GetAddrInfoW`, `GetAcceptExSockaddrs`). They are board-rowed rather than fixed speculatively, on the same do-it-when-a-suite-reaches-it rule as the layout census.

**The out-parameter class is CLOSED at the boundary, because the operator has TWO wrong answers and neither is fixable (2026-08-17, lane `claude/x509-cryptoapi`).** The entry above is right about the mechanism and stops one measurement short. Asked directly, golib answers `(uintptr)` for a `ж<ж<T>>` **0 while the held pointer is null** — the silent "no output wanted" above — and **a live MANAGED address the moment it is not**, which is the worse of the two: a slot the collector reads as an object reference, handed to a kernel that writes eight raw bytes into it, silent until the next collection. (`EnsureStableAddress` does not even pin such a slot: `GCHandle` pins only reference-free storage, so the address is transient as well as wrong.) That pair settles the layer question by measurement rather than preference: **no single address is both kernel-writable as eight raw bytes and managed-readable as a `ж<T>`**, so there is nothing the operator could return that would serve the boundary. Reconciling the two representations needs a SYNC POINT — the moment the raw word becomes a pointer box — and that moment is "after the call returns", which only the wrapper knows. So `ж.cs` is unchanged, and the remedy is one shape applied per wrapper: a native cell local to the call, its address handed over, and a publish afterwards through **`ValueSlot`, never `Value`** (the nil guard on `Value` value-peeks, so it would panic on the very write that fills the slot in — the same reason `GetAcceptExSockaddrs` writes its two `**RawSockaddrAny` results that way). A ZERO report publishes the nil pointer with no special case, because `ж<T>`'s native constructor already treats address 0 as nil (Go's `(*T)(unsafe.Pointer(uintptr(0))) == nil`), which is what lets each wrapper write one unconditional assignment for both the written and unwritten cases. A nil slot is a real caller, not a defensive branch — `crypto/x509`'s `createStoreContext` passes literal nil for every intermediate it adds. Five of the thirteen are taken in `src/core/syscall/windows/zsyscall_windows_ptrout_impl.cs` (the `ConvertSidToStringSid`/`ConvertStringSidToSid` round trip, `NetGetJoinInformation` over a third DLL with a different free routine, and the two crypt32 members `crypto/x509` reaches); the other eight are left for stated reasons, chief among them that `DnsQuery`'s `**DNSRecord` pointee is a linked native chain whose converted record holds managed references, so publishing the address alone would replace a silent nil with a fabricated-reference landmine. ⚠ Correction to the census above: `NetGetJoinInformation` and `NetUserGetInfo` live in **`syscall`**, not `internal/syscall/windows` — the split is 11 + 2. **Verify at VALUE level, exactly as for the layout class:** the guard (`PointerOutParameter` behavioral output test) round-trips four well-known SIDs string→`*SID`→string, requires a malformed SID to still be REJECTED, and WALKS `NetGetJoinInformation`'s returned buffer to its NUL rather than testing it for nil. Proven failing-first, and the failure is not a crash — with the cell address neutered the program exits 0 and prints `stable: true false`, two empty strings agreeing with each other and with nothing else, plus a plausible-looking `ERROR_INVALID_PARAMETER` from advapi32 (whose out-parameter, unlike `ppStoreContext`, is REQUIRED). The golib contract itself is pinned from the other side by `GolibTests`' `AddressOfAPointerToPointerIsNotAKernelWritableSlot` and `PublishingANativeAddressThroughValueSlotIsTheBoundaryRemedy`, so a later reader cannot "fix" the operator into the dangerous answer. ⚠ **Fixing this does NOT make `crypto/x509`'s system verifier work** — it reaches the next wall, which is the LAYOUT class again: `CertChainPara` is passed by address while holding `ж<ж<byte>> UsageIdentifiers` and `ж<Filetime> CacheResync`, and `systemVerify` writes the NATIVE `Size` (80) into a much smaller managed object, so `CertGetCertificateChain` blocks inside the kernel with every field past the first read at the wrong offset (stack-proven with `dotnet-stack`). See the board entry for the full remaining CryptoAPI census and for why the read-back half cannot take the `GetAddrInfoW` treatment — `CertVerifyCertificateChainPolicy` and `CertFreeCertificateChain` both need the ORIGINAL native pointer back, so any remedy needs a dual identity rather than a transcription.

**⚠ A hand-own removes its body's EMISSION, and with it any `[assembly: GoImplement]` its body witnessed — check before listing a function in `manualConversionFuncs`.** This is a general trap the sockaddr work surfaced, not a sockaddr detail. Every `GoImplement` record the converter writes comes from a CAST it converted, so a function whose body holds the only casts of a type to an interface is also the only thing recording that pair. `RawSockaddrAny.Sockaddr` is exactly that function for all three `Sockaddr` types, and hand-owning it dropped the three `GoImplement<…, ΔSockaddr>(Pointer = true)` records from `syscall`'s `package_info.cs`. The consequence is not a build failure, which is what makes it dangerous: a MEASURED reconvert of `net` against the shortened `package_info` showed `net` quietly minting its own `syscall_SockaddrInet4жΔSockaddr` adapters instead of using `syscall`'s — the SECOND-IDENTITY regression `samePackageImplements.go` exists to prevent, where reflect and fmt see the wrapper in place of the value's own type and a direct-boxed value compares unequal to an adapter-wrapped one. Declaring the records in the `*_impl.cs` does NOT fix it: a dependent package's converter run reads `package_info.cs`, not the manual file. Recording them from the method set is the real answer, and it has since LANDED — `recordSamePackageImplements` now covers the POINTER method set as well as the value one, so these three pairs are recorded from `types.Implements(*T, Sockaddr)` and no longer depend on any body being converted. Proven by re-running exactly the probe that measured the regression: with `RawSockaddrAny.Sockaddr` suppressed through `manualConversionFuncs`, `syscall`'s `package_info.cs` still carries all three `(Pointer = true)` records and a reconvert of `net` still references `syscall.SockaddrInet4жΔSockaddr` rather than minting its own. **The general trap is narrower now but not gone**, and the check is unchanged for the cases the recorder's gates exclude — a pair whose interface or target is UNEXPORTED, a named FUNC target, a generic, or a promotion deeper than one embed hop is still witnessed only by its casts, as is any pair whose interface is declared in a DIFFERENT package (the recorder is same-package only, so a body holding the only `*T → io.Reader` cast is exactly as load-bearing as before). The cheap check before hand-owning anything: grep the body for interface conversions, and if it has any, reconvert one DEPENDENT package and diff.

**The wall BEHIND this one (recorded 2026-08-11 so it is not rediscovered).** Fixing the sockaddr seam does not by itself make `net` work, and the board's expectation that it would was formed while the panic masked what follows. With `bind` succeeding, `net.Listen` walks on to `internal/poll`'s `pollDesc.init` and stops at `runtime_pollServerInit` — one of ten bodyless `//go:linkname` netpoll entry points that the `PartialStubGenerator` emits as "external (assembly or cgo) function is not implemented". The counterpart exists in the converted runtime (`runtime/netpoll.cs` carries `poll_runtime_pollServerInit`), but nothing wires the linkname across assemblies — and wiring it would not be enough either, because the Windows implementation bottoms out in `netpollinit` → `stdcall2(_CreateIoCompletionPort)` → `asmstdcall`, itself a stub. So this is a second, independent seam whose honest remedy is the managed-API-boundary pattern already used for `sync`'s Mutex and `runtime`'s traceback surface — hand-own the ten `runtime_poll*` contracts against .NET's own completion-port machinery rather than emulating Go's poller — and it is a design arc, not a wrapper repair. **RESOLVED for the listener lifecycle 2026-08-13** (design ruled, arc S1): the ten contracts are hand-owned in `internal/poll/windows/runtime_netpoll_impl.cs` and `net.Listen` now completes against a real kernel — see *The managed netpoller* below. The prediction in this paragraph held exactly, including that the deep wall is the scheduler rather than `asmstdcall`; what it did NOT anticipate is that making `pollWait` wake up is only half the arc, because the overlapped submissions `execIO` issues cannot yet reach the kernel safely (the OVERLAPPED lifetime seam, S2).

**The same fork one step further out: REINTERPRETING an OS byte buffer as a Go struct (`os.readReparseLink`, 2026-08-02).** The entries above are about a struct handed TO the kernel; this is the mirror — a byte slice the kernel FILLED, reinterpreted as a Go struct whose trailing inline array stands in for variable-length data written after it. `internal/syscall/windows`'s `SymbolicLinkReparseBuffer` / `MountPointReparseBuffer` both end in `PathBuffer [1]uint16`, and `Path()` reads the real name through `(*[0xffff]uint16)(unsafe.Pointer(&rb.PathBuffer[0]))[n1:n2]`. In the conversion that field is a golib `array<uint16>` — an 8-byte MANAGED REFERENCE where the OS wrote 2+ bytes of inline UTF-16 — so golib's `PointerExtensions.Reinterpret` correctly declines to alias managed storage for a reference-bearing struct and falls back to the raw-address route; `&rb.PathBuffer[0]` then resolves an object reference synthesized out of path bytes and faults with an `ACCESS_VIOLATION` inside `array<uint16>.get_Item`. That KILLED the C# test host mid-run at `os`'s `TestReadlink`, at test 50 of 178, which is the same silent under-reporting shape `findFirstFile1` produced for `path/filepath`. No converter or golib change can rescue it — a managed array reference can never be laid out like an inline OS array — so it takes the `dir_windows_impl.cs` remedy rather than the blittable-mirror one: `manualConversionFuncs` turns `os.readReparseLink` into a placeholder and `src/core/os/file_windows_impl.cs` decodes the record straight out of the byte slice at its documented offsets (`REPARSE_DATA_BUFFER` header 8 bytes; then `SubstituteNameOffset`/`Length` at +0/+2, `PathBuffer` at +12 for the symlink shape and +8 for the mount point), with every read an ordinary bounds-checked slice index — no pointer, no pinning, no `unsafe` block. `openSymlink` and `normaliseLinkPath` stay auto: they pass scalars, handles and strings. ⚠ `syscall.Readlink` carries the identical defect over its own private `reparseDataBuffer` / `symbolicLinkReparseBuffer` / `mountPointReparseBuffer` copies; it is LATENT (nothing in the validated corpus reaches it) and is recorded rather than fixed speculatively, per the do-it-when-a-suite-reaches-it rule above.

**The same fork, in the idiom Go used before `unsafe.Slice` existed: `(*[N]T)(unsafe.Pointer(p))[:]` over a NATIVE pointer (2026-08-17, lane `claude/x509-cryptoapi`).** This one is worth naming separately because the Go source looks utterly ordinary — it is the standard pre-1.17 way to read a NUL-terminated `*uint16` the OS handed back, and Go's own standard library still writes it. golib's `array<T>.AliasPointer` answers it correctly **when the pointer has managed element storage behind it**: it windows the real backing array, clamped, so an overrun is a Go-style index panic. When the pointer is a genuinely NATIVE address there is no `T[]` to window, and it falls back to `(ж<array<T>>)(uintptr)element` — a native-address box over `array<T>`, which is *itself* reference-bearing. Dereferencing that fabricates a `T[]` reference out of address bytes, and the slice constructor built from it dies with `Fatal error. System.AccessViolationException at go.slice'1[UInt16]..ctor` — measured, from a guard's first draft reading `NetGetJoinInformation`'s result. The fallback is not a defect in `AliasPointer`, which has nothing better available; the emitted form simply has no correct realization for a native operand, exactly as with the sockaddr decode. **Live corpus sites over genuinely native pointers: `net/windows/lookup_windows.cs` lines 395, 465 and 500** (the DNS answer walk — SRV `Target`, NS `Host`, TXT `StringArray`, all `*uint16`/`**uint16` written by `DnsQuery`) and **`reflect/type.cs:1887`** (`t.t.GCData`). The `syscall` and `internal/syscall/windows` uses of `AliasPointer` are all over MANAGED element pointers (`Ꮡ(tv, 0)`, `Ꮡ(rb.PathBuffer, 0)`) and take the safe route. Note where the DNS three sit: immediately behind `DnsQuery`, itself an unfixed member of the out-parameter class — so a `net` DNS arc owns two walls stacked on one call, and reaching a real `*DNSRecord` would only bring the walk into range of this one. The remedy where a suite reaches it is the established one: transcribe at the boundary (as `GetAddrInfoW` does for `ADDRINFOW`) rather than aliasing native memory as a managed array.

**The same fork at its largest, and the first member whose defect is in NO wrapper: `net.adapterAddresses` and the `IP_ADAPTER_ADDRESSES` chain (2026-08-17).** Every entry above names a wrapper. This one does not, and that is the point worth carrying: `internal/syscall/windows`'s `GetAdaptersAddresses` is handed a byte buffer and fills it, which is exactly what a byte buffer is for — the wrapper is correct and stays auto-converted. The defect is entirely in the CALLER, `net`'s own `adapterAddresses`, which walks the filled buffer as a linked record (`for aa := (*windows.IpAdapterAddresses)(unsafe.Pointer(&b[0])); aa != nil; aa = aa.Next`). `IpAdapterAddresses` is the corpus's most reference-dense converted struct — nine `ж<T>` fields, an `array<byte>` `PhysicalAddress` and an `array<uint32>` `ZoneIndices` where the native record has raw pointers and inline storage — so golib declines to alias the byte run as it and the reinterpret falls to a native-address box, after which the loop's OWN nil test fabricates a managed reference out of adapter bytes: `ACCESS_VIOLATION` in `ж<IpAdapterAddresses>.op_Equality`, killing the process. It was the wall standing behind the `GetAddrInfoW` fix — `dnsReadConfig` is `getSystemDNSConfig`'s only source of DNS servers on Windows — so until it landed no converted program on Windows could resolve a host name at all, and it also backs `net.Interfaces`, `net.InterfaceAddrs` and `Interface.MulticastAddrs`. The remedy is `GetAddrInfoW`'s, one structure size up: `manualConversionFuncs` turns `net.adapterAddresses` into a placeholder, and `src/core/net/windows/interface_windows_impl.cs` holds the buffer in NATIVE memory that never escapes the function, transcribes the whole chain into managed `ж<T>` boxes, and frees the native buffer eagerly in a `finally`. Three things generalize past this record. **(1) The output is a chain of chains.** Each adapter record carries SIX nested linked lists (unicast, anycast, multicast, DNS-server, prefix, WINS-server and gateway — five of which share one native shape), and every consumer reaches THROUGH a record into them, so a top-level copy alone would have moved the fabrication one hop out instead of removing it. **(2) Whether a managed pointer needs a `ManagedPointerTokens` handoff is decided by the GO DECLARATION, not by the data.** `AddrinfoW.Addr` is an untyped `syscall.Pointer` that the consumer casts by hand, so it needs a token; `SocketAddress.Sockaddr` is a TYPED `*syscall.RawSockaddrAny`, which converts to a `ж<syscall.RawSockaddrAny>` field carrying a managed box directly — no `unsafe.Pointer` round trip to survive, and therefore no token, no weak table and no anchor. Reaching for the token machinery here would have been ceremony. **(3) The transcription writes the image an EXISTING hand-own reads.** The consumers call `.Sockaddr()` on that field, which is `syscall`'s hand-owned decode — it flattens the managed `RawSockaddrAny` back to its 116-byte native image and decodes that — so the sockaddr transcription is the same inverse-flattening `syscall`'s own `GetAcceptExSockaddrs` performs for the accept path (Family at 0, `Addr.Data` covering 2..15, `Pad` covering 16..115), duplicated rather than shared because exposing it would put a non-Go symbol on a published package's public surface. Finally, the copy is FAITHFUL rather than minimal: `Length`, `AdapterName`, `DnsSuffix`, `Description`, `Flags`, `ZoneIndices`, `FirstPrefix`, the link speeds and the WINS list are carried although no consumer reads them today, because this record is the public shape behind `net.Interfaces` and a declared field left nil would be a SILENT divergence for the next consumer rather than a loud one. (Guarded by the `IpAdapterAddresses` behavioral **output** test. Its assertions are derived facts both sides compute over the same live host — the host's own adapter set is never printed, since it varies by machine and can change between the two runs — and they pin named native offsets: the loopback MTU is exactly `0xffffffff` reported as `-1`, `FriendlyName` at +72 must be non-empty, printable, unique and round-trip through `InterfaceByName`, `PhysicalAddressLength` at +88 must stay within the inline `[8]byte` at +80, and `127.0.0.1/8` plus `::1/128` are end-to-end proofs of both sockaddr flavors including the `OnLinkPrefixLength` read 56 bytes into a unicast entry. Proven failing-first: with the auto body restored the guard reports `exit code mismatch: C# -1073741819 vs Go 0`.)

**The same fork, with NO kernel anywhere in it: reinterpreting one MANAGED array as an array of another element type (`sha3.xorIn`/`copyOut`, 2026-08-16).** The two entries above both involve the OS, which makes it easy to file this whole family under "syscall marshalling". It is not about the OS at all. Go's sponge implementation views its own `[25]uint64` state as raw bytes —

```go
ab := (*[25 * 64 / 8]byte)(unsafe.Pointer(&d.a))
subtle.XORBytes(ab[:], ab[:], buf)   // absorb
copy(b, ab[:])                        // squeeze
```

— purely managed storage on both sides. A `byte[]` view over a `uint64[]` has no managed spelling (the same fact that made `crypto/subtle`'s `words()` — a `uintptr[]` view over a `byte[]` — hand-owned), so `pointerReinterpretManagedSource` excludes pointer-to-ARRAY targets and the site keeps the raw-address route: `(ж<array<byte>>)(uintptr)(new unsafe.Pointer(Ꮡd.of(state.Ꮡa)))`. That box is fine as an *address*; the defect is what happens when it is **dereferenced**. `~ab` reads an `array<byte>` STRUCT — a backing-store REFERENCE plus bounds — out of the keccak state's own DATA, i.e. fabricates a managed reference, and the first use of it is an `AccessViolationException` inside `slice<byte>`'s constructor that kills the process. **`crypto/tls` reached it on every TLS 1.3 ClientHello** (`mlkem768.NewKeyFromSeed → kemKeyGen → sha3.Sum512`), where it claimed 10 of the package's 17 remaining divergences.

**The emission is not wrong** -- there is no correct alternative, so the site is hand-owned (`src/core/vendor/golang.org/x/crypto/sha3/xor.cs`, `[module: GoManualConversion]`), and the remedy is `crypto/subtle`'s: `MemoryMarshal.AsBytes` over the array's own span is a genuine ALIASING view of the same backing storage, so the absorb's XOR lands in the real state and the squeeze reads it. Go's `cpu.IsBigEndian` branch is left exactly as converted; only the reinterpreting branch changes. (Guarded by `GolibTests.Sha3ReinterpretVectorTests` -- FIPS-202 vectors plus lengths straddling SHA3-256's 136-byte rate and an unaligned sub-slice, checked against the OS SHA-3 implementation; the vendored package has no `_test.go` in GOROOT and is not importable from a behavioral test, and an MSTest tier binding a converted package follows `GenericTests`, which references `core/sort`. Neutering the fix is a RUN control: the auto-converted `xor.cs` does not merely fail the vectors, it KILLS the test host with the AccessViolation above.) *(At Go 1.24.13 `golang.org/x/crypto/sha3` is no longer vendored and this file is gone; the standard library's Keccak lives in `crypto/internal/fips140/sha3`, whose hand-owned `keccakf.cs`/`keccakf_impl.cs` take the reverse view — the `[200]byte` state as `[25]uint64` — by the same `MemoryMarshal.Cast` remedy. The MSTest guard's disposition is recorded at `src/tests/GolibTests/GolibTests.csproj:193-196`.)*

**The same fork one RANK up — a NESTED array view, and why the native-array-view arc does not reach it (`internal/chacha8rand.setup`/`block_generic`, 2026-08-25).** sha3's is a `[200]byte` view of a `[25]uint64`: same rank, different element type. chacha8rand opens one `[32]uint64` allocation as `(*[16][4]uint32)(unsafe.Pointer(buf))` and that again as `(*[16][2]uint64)(unsafe.Pointer(b32))` — different element type **and** different RANK. The failure is the mild end of the fabrication: the buffer is ZEROED at the first call, so the `array<array<uint32>>` struct read out of it has a null backing, i.e. **length zero**, and the first index panics `index out of range [0] with length 0` rather than access-violating. `TestBlockGeneric` is the whole of the package's 1-of-4 gap, and the panic site — golib's `array<T>.get_Item` — is the same one `html`'s map-miss produced from a completely unrelated root, which is worth stating plainly: **`get_Item` is the DETECTOR of an unshaped array, never the defect**; two correlated crashes at one golib line were two different producers.

**This one does not fall to `DESIGN-native-array-view.md`** (RATIFIED, §3 emission work HELD pending the provenance amendment) even when that arc lands, and the reason generalizes: a native-backed `array<T>` can hold blittable elements in raw bytes, and `array<uint32>` is not blittable — a `[16][4]uint32` view needs sixteen `array<uint32>` STRUCTS, which is exactly the thing that cannot live in the buffer. So the package is ROUTED AROUND rather than waiting on the arc: `manualConversionFuncs["internal/chacha8rand"]` suppresses both functions and `chacha8_impl.cs` provides them, taking `MemoryMarshal.Cast<uint64, uint>` over the array's own span — the sha3/subtle remedy, one rank up. `block` was already hand-owned there (Go implements it in ASSEMBLY on amd64/arm64) in a scratch-and-pack shape, and the new `block_generic` computes IN PLACE through the aliasing view using the package's own auto-converted `qr`, so `TestBlockGeneric` still compares two implementations that share no code — what it newly proves is that the aliasing view WRITES THROUGH. Measured: `internal/chacha8rand` **3 → 4 of 4, `"status": "validated"`, 0 disclosed**. The seam's kernel-free reproduction is preserved directly in golib by `GolibTests.ArrayShapeReinterpretTests`, which measures the raw route's length-zero answer and pins the span view's write-through in both directions — so routing the package around costs the arc nothing, and the witness can no longer be fixed away by a conversion change.

**The same fork with a LOUD failure instead of a silent one — a reinterpret to a LARGER struct, and an assert that kills the process (`reflect.rtype.FieldByIndex`, 2026-08-19).** Every entry above turns on a fabricated managed reference; this one is decided a step earlier, by SIZE, and it is worth stating separately because the remedy is cheaper and the diagnosis is misleading in a new way. Go's `(*structType)(unsafe.Pointer(t))` re-views an `*rtype` as the struct-kind specialization that begins with the same `abi.Type`. `ReinterpretAliasesStorage` requires the destination to FIT in the source, and `structType` is strictly LARGER than `rtype` — it carries `PkgPath` and `Fields` past the embedded `Type` — so the alias arm never engages and the pair takes the raw-address route. The derived box therefore names storage that is not a `structType`, and the descent `Ꮡt.of(structType.ᏑType)` yields an `abi.Type` with no managed cargo: `sysType` is null. The very first statement of the auto body, `toType(&t.Type)`, then trips `canonType`'s synthType-was-bypassed assertion — a `System.Diagnostics.Debug.Assert`, which in a Debug build **terminates the process** (`0x80131623`).

That failure mode is the part worth carrying. It is not a fault at a later read, and it is not a test failure: the host DIES, so every remaining test in the file reports no verdict at all. `encoding/xml` reaches it from `getTypeInfo → addFieldInfo` on any `Unmarshal` of a struct with a promoted field, and **15 of its verdicts came back EMPTY rather than failed** — which reads like a missing or mis-run suite rather than one dead call, and is the same silent under-reporting shape `findFirstFile1` and `readReparseLink` produced. A comment in `value_impl.cs` asserted this branch was dead ("synthType always stamps sysType… every canonType caller feeds a synthType box or nil"); it was not, and an assert that documents itself as unreachable is exactly the one to distrust.

The remedy is smaller than the blittable-mirror and byte-buffer answers above, because nothing native is involved and no layout has to be transcribed: the reinterpret exists only to reach a descriptor the receiver ALREADY holds. `manualConversionFuncs` turns `rtype.FieldByIndex` into a placeholder and `value_impl.cs` seeds from `common()` — the rtype's own synthType-stamped `abi.Type`, which IS the value Go's `&t.Type` names after the reinterpret — so no `structType` is ever synthesized. Go's index walk is kept verbatim, and each hop goes through the already-hand-owned `rtype.Field`, so the promoted-field projection stays the one `GoReflect` provides. Measured: `encoding/xml` **369 → 384 of 386**, canaries `fmt` 63/63 and `encoding/json` 491/491 unmoved. **The general lesson for this family: when the reinterpret's target is a struct-kind SPECIALIZATION that only extends the source, prefer reaching the shared prefix directly over re-viewing the whole record** — the specialization's extra fields are exactly what makes the alias unrepresentable, and they are usually not what the code wanted.

⚠ **What did NOT land, and why it is recorded rather than shipped.** The failure MODE is the expensive half: an `AccessViolationException` cannot be recovered, carries no diagnostic, and names whichever consumer first touched the fabricated reference rather than the reinterpret that built the box. A refusal was therefore built and measured -- `ж<T>` raising a contained panic instead of reading. **Two counter-examples, both found by gates rather than by reasoning, sank it.** (1) `RuntimeHelpers.IsReferenceOrContainsReferences<T>()` alone is too wide: `time.syncTimer`'s `~Ꮡc.Reinterpret<channel<Time>, unsafe.Pointer>()` lands on the same address route because `unsafe.Pointer` is a CLASS, and reading it yields the real channel object -- type-CONFUSED rather than fabricated, which is the managed-referent model the corpus is built on. Refusing it took down `time.NewTimer` and every `crypto/tls` test that opens a pipe. Adding a `typeof(T).IsValueType` term fixed that. (2) The narrowed form still regressed `ArrayCastDerefClone`, the behavioral guard for this very fork: its `*(*Row)(unsafe.Pointer(&r))` over a ZERO-valued array reads an `array<nint>` whose fabricated backing reference is **null**, which `array<T>`'s null-safe zero value absorbs -- so the site produced garbage harmlessly, which is exactly the "compiles and does not crash" bar the S1 ruling sets for raw-metal stubs, and the refusal turned it into `exit 2`. **The distinction the remedy actually needs is not the pointee's TYPE but whether the fabricated reference comes out NULL** -- and that cannot be tested without first materializing a `T` with a wild reference in a stack slot, which is itself unsafe. So the class keeps the AccessViolation for now; the 43 corpus files carrying the shape (26 of them under `runtime`, plus `internal/syscall/windows/registry`'s `SetDWordValue`/`SetQWordValue` and `reflect`/`runtime`'s `name.pkgPath`) are each fixed when a suite reaches them, and the null-check sketch is the starting point for whoever revisits it.

**Atomic pointer ops on a MANAGED pointer field read/write the reference, not a `uintptr`.** The lock-free-cache idiom `atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))` / `atomic.StorePointer(…, unsafe.Pointer(v))` — where `x.field` has type `*T` and so holds a `ж<T>` reference — cannot go through the literal conversion: `new @unsafe.Pointer(v)` round-trips the managed reference through its (transient) address, and `(ж<@unsafe.Pointer>)(uintptr)(FromRef(ref …field))` dereferences raw memory, losing GC identity (it NRE'd on the very first read — x/sys/windows's `LazyDLL`/`LazyProc` proc caches at package-init). `convCallExpr.managedAtomicPointerIdiom` recognizes the idiom (the callee is `sync/atomic.LoadPointer`/`StorePointer` and the argument is `(*unsafe.Pointer)(unsafe.Pointer(&Z))` with `Z` of pointer type) and emits golib's managed-referent overloads on the **field box** directly: `atomic.LoadPointer(Ꮡx.of(T.Ꮡfield))` → `ж<ж<T>>` → `Volatile.Read` returning `ж<T>`, and `atomic.StorePointer(Ꮡx.of(T.Ꮡfield), v)` → `Volatile.Write` of the plain `ж<T>` (the stored value unwrapped from its `unsafe.Pointer(…)` conversion). The overloads are additive — a `ж<ж<T>>` argument never matches the existing `ж<@unsafe.Pointer>` (`= ж<Pointer>`) signature, so ordinary `unsafe.Pointer` atomics are untouched. The load stays `unsafe.Pointer`-typed to Go, so a caller's `== nil` still renders `(uintptr)… == nil`; the `ж<T> → uintptr` operator (above) yields 0 for a nil box, so the nil test is correct with no change to the surrounding emission. Blast radius is only the packages using the idiom (x/sys/windows and a handful of stdlib sites), each a pure re-shaping to the managed overload; CNR byte-identical across the behavioral corpus. (Guarded by the `ManagedAtomicPointer` behavioral **output** test — a `*proc`-field lock-free cache initialized once and re-read, vs Go; it NRE'd before the fix.)

The `ref` the helper takes depends on how the pointer argument **renders**. A genuine box — an address-of expression, a local pointer variable, a pointer field, a call result — is the `ж<T>` object, so the ref goes through its boxed value: `FromRef(ref (box).Value)`. But a **deref-aliased** pointer — a pointer *parameter* or pointer *receiver*, which the body renders as the pointed-to value alias (`ref var p = ref Ꮡp.Value`) — is not a box; `.Value` on it is `CS1061` (`nuint` has no `Value` — runtime `select.go` `unsafe.Pointer(pc0)` and `heapdump.go` `unsafe.Pointer(pstk)`, both `*uintptr` parameters). The alias is itself a ref-local into the boxed storage, so the converter takes its ref directly: `FromRef(ref p)`. Detection reuses `exprIsDerefAliasedPointer` (the same discriminator the pointer-reinterpret block uses). This also let the `guintptr`/`muintptr` receiver family (`runtime2.go` `(*uintptr)(unsafe.Pointer(gp))` inside `guintptr.cas`) compile — previously `ref (gp).Value` bound the `[GoType]` wrapper's `Value` *property* (CS0206); the CAS it feeds (`atomic.Casuintptr`) is a `partial` asm stub, so the copy-box semantics match the established reinterpret precedent (compile-milestone bar; the faithful managed-referent `ж<T>` model for those types remains a separate effort). (The bare `unsafe.Pointer(p)` pin stays exercised across the stdlib — runtime `select.go`/`heapdump.go`, and `runtime2.go`'s genuine `*guintptr`→`*uintptr` reinterpret here, whose differing element types keep it off the identity path. The `UnsafePointerParamPin` behavioral **output** test now guards the same-type **identity collapse** of the `(*uintptr)(unsafe.Pointer(p))` shape it originally used — see *A SAME-TYPE reinterpret … collapses to the pointer itself* above — where the whole conversion elides to the box; a `(*byte)(unsafe.Pointer(&value))`-style DIFFERENT-type reinterpret still pins through `FromRef`.)

**Returning an `unsafe.Pointer` parameter whole is a plain value return.** The return path boxes a *pointer parameter* returned whole (`return p` → `return Ꮡp` — the value alias cannot bind the pointer result), and the pointer-result check counts the `UnsafePointer` basic as a pointer. But an `unsafe.Pointer` parameter renders as a plain **value** param (`@unsafe.Pointer zero`) with *no* box, so the prefix referenced a nonexistent `Ꮡzero`/`Ꮡv`/`Ꮡfd` (CS0103 — runtime `map.go` `mapaccess1_fat`/`mapaccess2_fat`'s `return zero`, `mem_windows.go`, and `panic.go` `readvarintUnsafe`'s tuple return). The box form now applies only when the returned parameter's own type is a **genuine `*T`** (deref-aliased, so `Ꮡp` exists); an `unsafe.Pointer` param returns as-is. (Guarded by the `UnsafePointerParamPin` extension — the whole-return, tuple-return, and genuine-`*T`-control shapes, values vs Go; cleared 4 runtime CS0103, 63 → 59.)

The **reverse** direction — reinterpreting a raw address *as* a pointer, `(*T)(p)` where `p` is an `unsafe.Pointer` (or `uintptr`) — is the reinterpret pattern referenced above. Its result is the pointer type `ж<T>`. A plain `(ж<T>)p` cast is `CS0030`: because `unsafe.Pointer` is `Pointer : ж<uintptr>`, reaching `ж<T>` needs the two chained user-defined conversions `Pointer → uintptr → ж<T>`, and C# performs at most one user-defined conversion in a cast. The converter routes explicitly through `uintptr` — `(ж<T>)(uintptr)(p)` — which reads the `T` at `p`'s address via golib's `explicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => Value`, the address the pointer holds). The deref `*((*unsafe.Pointer)(k))` then adds `.Value`: `((ж<@unsafe.Pointer>)(uintptr)(k)).Value` — Go's read of the `unsafe.Pointer` stored at `k`. This is the identical routing the *dereference* path (`(*int)(p)` inside `*(...)`) already used via its `isPointerCast` flag; the fix extends it to the two shapes that did **not** set that flag: a bare call **argument** `atomicwb((*unsafe.Pointer)(ptr), new)` (runtime `atomic_pointer.go`) and an **extra-paren** deref `*((*unsafe.Pointer)(k))` (runtime `map.go`'s indirect key — `convStarExpr`'s dereference branch sees a `ParenExpr`, not the `CallExpr`, so it never marks the cast). Gated to a **pointer-result** conversion whose **argument** is a raw address (`unsafe.Pointer`/`uintptr` basic); the pointer-to-*named*-type value conversion `(*Base)(defPtr)` (below) has a `*T` argument, is handled earlier, and is not affected. Like every reinterpret through the `uintptr` round-trip, the golib operator reads/boxes a **copy** from a `fixed` address, so this is memory-layout-dependent code whose runtime values are **not the contract** — golib's own `map<K,V>` is what actually runs; the converted `runtime/map.go` only needs to compile. (Guarded by the `UnsafePointerReinterpret` behavioral **Compile + Target** test — both the extra-paren deref and the bare-argument shapes; cleared all 21 `unsafe.Pointer → ж<unsafe.Pointer>` CS0030 in `runtime`, 137 → 114.)

**A SAME-TYPE reinterpret `(*T)(unsafe.Pointer(p))` where `p` is already `*T` collapses to the pointer itself.** Converting a `*T` to `unsafe.Pointer` and back to the *same* `*T` is a no-op identity in Go — the language spec makes `(*Builder)(abi.NoEscape(unsafe.Pointer(b)))` exactly `b.addr = b` (strings.Builder's copy-by-value guard; the type's own TODO says to revert it to that once escape analysis improves). The `uintptr` round-trip above is **wrong** for this shape: golib's `ж<T>(uintptr)` DEREFERENCES-and-COPIES, so `b.addr` became a fresh box over a *copy* of the receiver, never reference-equal to it — and the guard's own `b.addr != b` self-check FALSE-PANICKED on the second call to any `strings.Builder` method (a `Grow` then a `WriteString`, or `strings.Join`'s repeated `WriteString`), surfacing as `panic: strings: illegal use of non-zero Builder copied by value` in the converted **fatih/color** `-recurse` sample the moment color was enabled. `convCallExpr.pointerReinterpretIdentitySource` intercepts this exact shape at the top of the conversion path — a `(*T)(…)` whose source, after peeling an optional escape-analysis identity wrapper (`abi.NoEscape` or a package-local `noescape`, matched by name **and** `unsafe.Pointer→unsafe.Pointer` signature), is `unsafe.Pointer(p)` with `p` of the *identical* pointer type `*T` — and emits `p`'s **box** directly (in the `isPointer` context, so a deref-aliased receiver/param renders `Ꮡb`, not its value alias `b`):
```csharp
internal static void copyCheck(this ж<Builder> Ꮡb) {
    ref var b = ref Ꮡb.Value;
    if (b.addr == nil) {
        b.addr = Ꮡb;                       // was (ж<Builder>)(uintptr)(abi.NoEscape((uintptr)@unsafe.Pointer.FromRef(ref b)))
    } else if (b.addr != Ꮡb) {
        throw panic("strings: illegal use of non-zero Builder copied by value");
    }
}
```
This preserves pointer identity AND shared storage (a write through the reinterpreted pointer now flows back, unlike the copy). A **different** element type is a genuine reinterpret and keeps the `uintptr` round-trip; the interception is `(*T)`-target- and same-element-type-gated (`types.Identical(srcElem, targetElem)`), so it fires ONLY for the identity. Across the 302-package stdlib it rewrites exactly **8** latently-miscompiled sites (`strings.Builder.copyCheck`, `internal/reflectlite`, `internal/syscall/windows/registry`, `os`, `syscall`, and three `runtime` sites) to the cleaner, correct box form — CNR byte-identical everywhere else; the bare `unsafe.Pointer(p)` pin (61 files) and the genuine-reinterpret round-trip (130 files) both remain and stay compile-guarded by the full build. (Guarded by the `PointerReinterpretIdentity` behavioral **output** test — a Builder-style `copyCheck` self-reference called repeatedly must NOT panic, and a genuine copy-by-value MUST still be caught, vs Go; it panicked before the fix — plus the identity-collapse arms of `UnsafePointerParamPin` (param/receiver/field), `PointerSelectorDeref`, and `PointerCastSliceRange`.)

**The identity also collapses when the source pointer is reached DIRECTLY — `*(*T)(p)` / `(*T)(p)` with `p` already `*T`.** This is the same no-op, minus the `unsafe.Pointer` hop: Go's way of re-reading a pointer at a fixed type. It was **not** recognised, and the deref path made it worse than the round-trip above. `convStarExpr`'s casted-pointer-deref branch sets `isPointerCast`, and the conversion renderer took that flag *alone* as licence to emit the raw-address bridge `(ж<T>)(uintptr)(p)`. But `isPointerCast` means only "this conversion is the operand of a deref" — it says nothing about the source being an **address**, and the bridge is only ever correct for one that is. A typed Go pointer is a managed **box**, and a deref-aliased pointer parameter renders as that box's *value alias*, so the `(uintptr)` leg had no conversion at all:

```csharp
// Go:  func derefStruct(p *Pt) Pt { return *(*Pt)(p) }
internal static Pt derefStruct(ж<Pt> Ꮡp) {
    ref var p = ref Ꮡp.Value;
    return ~(ж<Pt>)(uintptr)(p);           // CS0030: cannot convert 'Pt' to 'uintptr'
}
```
```csharp
internal static Pt derefStruct(ж<Pt> Ꮡp) {
    return ~Ꮡp;                            // the box, dereferenced in place
}
```

`pointerReinterpretIdentitySource` now accepts either source form — the direct pointer, or one unwrapped from `unsafe.Pointer(p)` — so the identity is intercepted before the bridge is ever considered. Emitting the box is correct for all three uses at once: a **value read** copies (`~Ꮡp`, plus the array `.Clone()` where the element is an array), an **lvalue write** lands on the real storage (`(Ꮡp).Value = …`) rather than on the round-trip's copy, and **pointer identity** is preserved. Note the recognition must stay pinned to a genuine `(*T)(…)` **conversion** — its `Fun` must denote a type. Matching on argument type alone collapses any one-argument *call* that takes and returns the same pointer type, silently deleting it (`advance(a)` → `a`, `Ꮡp.Swap(Ꮡa)` → `Ꮡa`); CNR caught exactly that across 13 behavioral projects. The corpus-wide A/B footprint is **two lines in one file** — `time.NewTimer`/`AfterFunc`'s `(*Timer)(newTimer(…))`, where `newTimer` already returns `*Timer`, shed a redundant identity cast — because the CS0030 shape needs a pointer *parameter*, which the stdlib's own reinterprets never use; the defect bites converted end-user code and behavioral guards. (Guarded by the `TypedPointerCastDeref` behavioral **output** test — struct, named-numeric, via-`unsafe`, non-deref, lvalue, and local-pointer shapes, plus the one-argument-call over-match control — and by the strengthened `ArrayCastDerefClone`; both verified to FAIL with the fix neutered, with that exact CS0030.)

**Still routed through the bridge (a known gap):** a typed-pointer source whose element type *differs* but shares an underlying — Go permits `(*T)(p)` there — is only partly covered by the named↔named / named↔basic / named↔array re-box routes below. A tag-differing struct pair (`types.Identical` counts tags, Go's conversion rule does not), an unnamed-array ↔ named-array pair, and a named ↔ unnamed struct pair all fall through to the raw-address bridge and mis-render. None occurs in the stdlib corpus, and narrowing the bridge gate without a correct box→box route for them merely trades one broken form (`(ж<Row>)(uintptr)(p)`) for another (`(ж<Row>)p`), so the gate is left as-is and the shapes are recorded here.

A deref whose **starred inner is a func type** (or any non-identifier type) — `*(*func())(add(…))`, runtime `panic.go`'s deferred-slot read `return *(*func())(add(p.slotsPtr, i*…)), true` — misses the identifier-gated cast-deref branch and falls to the default deref path, which must **wrap the cast before `.Value`**: C# postfix binds tighter than a cast, so a naked `.Value` re-binds onto the cast's *inner* operand (`(ж<Action>)(uintptr)(add(…)).Value` reads the inner `@unsafe.Pointer`'s `uintptr` — CS0029 `ж<Action>`→`Action` in the tuple return). The default deref now wraps any type-conversion operand: `(((ж<Action>)(uintptr)(add(…))).Value, true)`. This is the fourth instance of the cast-precedence/extra-paren family, and **indexing** a reinterpret result directly is the fifth: `(*[2]uint64)(x)[0] = 0` (runtime `malloc.go`) appended the pointer-to-array auto-deref `.Value` and the index to the cast render — `(ж<array<uint64>>)(uintptr)(x).Value[0]` read the inner `@unsafe.Pointer`'s `uintptr` and indexed a `nuint` (CS0021); the index emission now wraps a type-conversion base the same way: `((ж<array<uint64>>)(uintptr)(x)).Value[0]`. (Guarded by the `UnsafePointerReinterpret` extensions — the func-type deref in a tuple return and the indexed reinterpret write/read.)

The unsafe builtins `unsafe.Add`, `unsafe.Slice`, and `unsafe.String` accept a length/offset of **any integer type** (Go's `IntegerType` constraint, which includes `uintptr`/`uint`). golib's implementations therefore take a generic `IBinaryInteger` length, truncated to the `int` offset — so `unsafe.Slice(p, uintptrLen)` binds without an explicit cast (a plain `nint` parameter rejected a `uintptr`/`uint` argument with CS1503). (Guarded by `UnsafeBuiltinIntegerLen`.)

Passing an `unsafe.Pointer` **argument to an `unsafe.Pointer` parameter** keeps the `@unsafe.Pointer` struct value — `add(p, x)`, not `add(p.Value, x)`. The struct is an exact match for the parameter. (Guarded by `UnsafePointerArgPassing`.)

**Array-backed defined types reinterpret through storage-sharing `Value` refs, not value copies.** The fiat field-arithmetic shape (crypto/internal/edwards25519 `scalar.go`) reinterprets `&s.s` (a `fiatScalarMontgomeryDomainFieldElement`, written directly over `[4]uint64`) as `(*[4]uint64)` — and as its *sibling* `(*fiatScalarNonMontgomeryDomainFieldElement)` — then **writes element-wise through the reinterpreted pointer** (`fiatScalarFromBytes` parses INTO `&s.s` on a virgin receiver). Neither the copy-boxing named↔named route (each `[GoType("[N]elem")]` wrapper converts only to `array<E>`; a sibling cast needs two chained user conversions — CS0030) nor a plain `ж<>` cast (distinct instantiations) works, and any copy-based route would materialize the wrapper's **lazy** backing on a temp and orphan every write. The emission derefs through the ref-returning `ж<T>.Value` and invokes the wrapper's `Value` property in place — `Ꮡ((Ꮡs.of(Scalar.Ꮡs)).Value.Value)` (underlying-array form) / `Ꮡ((nonMont)((…).Value.Value))` (sibling form, one implicit conversion from `array<E>`) — materializing the backing on the ORIGINAL storage and boxing an `array<E>` struct that shares its `T[]`: element reads and writes flow through. Gating consults the type's **written RHS** (a new per-package pre-pass records each `TypeSpec`'s declared right-hand side, which `Named.Underlying()`'s full resolution loses): only types written *directly* over an unnamed array take this route, so chain-defined view wrappers (`type pallocBits pageBits`) keep the existing copy-box route byte-identically; the same written-RHS gate lets `isTypeConversion` claim the pointer-to-type-literal target `(*[4]uint64)(…)` (no `types.Object` exists for a composite type) without disturbing the pointer-cast slice form (`(*[1<<20]Method)(p)[:n:n]`, internal/abi). Caveat (documented, no stdlib site): a *whole-value* write through the reinterpreted box (`*p = q`) rebinds only the boxed struct. (Guarded by the `NamedArrayWrapper` extensions — a virgin-field write through the underlying reinterpret, a sibling reinterpret aliasing the same storage read-during-write, and a heap-boxed local, all output-compared vs Go.)

**The `uintptr → ж<T>` raw-address reinterpret operator is `explicit` by design.** It boxes a **copy** of the value read at an arbitrary address (the runtime-unsafe reinterpret seam) — never something to happen silently, and every converter-emitted reinterpret already uses explicit cast syntax (`(ж<T>)(uintptr)(p)`). As an *implicit* conversion it also poisoned overload resolution: a `uintptr` argument converted to **both** an `@unsafe.Pointer` parameter (via the numeric `uintptr ↔ Pointer` operators, which stay implicit) and any `ж<T>` parameter, so a **free function and a same-named pointer-receiver method** — runtime's `func add(p unsafe.Pointer, x uintptr)` (stubs.go) vs `func (p *notInHeap) add(bytes uintptr)` (malloc.go), both emitted as static `add` overloads in the package class — were ambiguous (CS0121) at every free-call site whose argument is a **pin of a boxless receiver**: inside a `[GoRecv] ref` method, `unsafe.Pointer(b)` emits the `uintptr`-typed `(uintptr)@unsafe.Pointer.FromRef(ref b)` (runtime `map.go` `b.keys()`/`b.overflow()`/`b.setoverflow()`, `mprof.go`'s stack-record walkers — 6 sites). With the operator explicit, the `uintptr` argument binds only the `@unsafe.Pointer` overload. The reverse `ж<T> → uintptr` (box → address) operator remains implicit — producing a number is not a silent deref. (Guarded by the `FuncVsMethodOverload` behavioral **output** test — the free `add` + direct-ж method `add` overload pair with the boxless-receiver pin call shape, plus both method-call forms, values vs Go; cleared all 6 runtime CS0121, 59 → 53.)

**A cross-package type reference emits its `using <alias> = <namespace>;` even when the file did not import the package under a usable name.** A foreign type renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type, `@unsafe.Pointer` for the `unsafe.Pointer` basic — which resolves only through a file-local alias (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias is normally generated from a *canonical* (unaliased) `import`, but a file can reference a foreign type with no such import through three routes: **type inference** — a *same-package* function returns a foreign type, so the caller infers a local of that type but never writes `pkg.` and need not import the package (runtime `preempt.go`: `fd := funcdata(f, i)`, where `funcdata` returns `unsafe.Pointer`); a **blank import** (`_ "pkg"`, side-effects-only — **no `using` is emitted for it at all**: the old `using _ = <ns>;` emission hijacked C#'s `_` DISCARD for the whole file, so a deconstruction discard (`(w, _) = w.ensure(…)`, runtime `tracetime.go`) bound the namespace alias instead (CS0118 + CS0029); the import is recorded as a comment, and a genuine type reference still gets its canonical alias from this machinery — e.g. `symtabinl.go`'s `_ "unsafe"` for `//go:linkname`); or an **aliased import** (`import u "unsafe"`, whose alias `u` differs from the canonical `pkg.Name()` prefix the type reference uses). All previously yielded CS0246. The converter now walks every emitted type (`collectTypePackages`, called from `getAliasQualifiedTypeName` — named types by `pkg.Path()`, an `unsafe.Pointer` basic by the pseudo-path `"unsafe"`, recursing through pointer/slice/array/map/chan/generic/func-signature so a `[]time.Duration` element registers too) and, at file close (`visitFile`), supplies the canonical `using <alias> = <namespace>;` for every referenced foreign package the file did not already import canonically. It is idempotent-safe — a canonical import records its path in `canonicalAliasImported`, so `visitFile` never re-emits (duplicates) it — and a non-canonical alias (`using u = unsafe_package;`) coexists with the added canonical one without conflict. It is also **collision-guarded**: the synthesized `using <alias> = <namespace>;` is skipped when its canonical `<alias>` was already bound to a *different* namespace by a real import — cryptobyte's `asn1.go` imports both `encoding_asn1 "encoding/asn1"` (referenced by type, so it reaches this loop) and the subpackage `.../cryptobyte/asn1` (unaliased → alias `asn1`), so synthesizing `using asn1 = encoding.asn1_package` would duplicate the subpackage's `using asn1` (CS1537). The real imports' emitted aliases are tracked per file (`importAliasesEmitted`); the parent stays reachable through its `encoding_asn1` alias, so skipping the canonical one is safe (a non-colliding canonical alias is still supplied — no churn). (The *separate* defect that the type reference itself renders `asn1.ObjectIdentifier` rather than the file's `encoding_asn1.ObjectIdentifier` — `getAliasQualifiedTypeName` uses the canonical alias, not the file's non-canonical one — is tracked independently.) This is the *type-reference* analog of the method-call `addMethodPackageNamespaceUsing`. (Guarded by `UnsafePointerInferredNoImport` — the `unsafe.Pointer` basic arm, scalar/composite/blank-import variants — and `InferredForeignTypeNoImport` — the generic named arm, an inferred `*strings.Reader` in an `fmt`-only consumer.)

**That supplied alias must carry the collision rename.** When the referenced package's using alias is `Δ`-renamed because a same-named CHILD namespace is visible from the import closure (`go.sync`, contributed by `sync/atomic`; `go.unicode`, by `unicode/utf8` — the same CS0576 collision that renames a *canonical* import's alias, above), `getAliasedTypeName` already renders the short-form type reference through the renamed qualifier (`Δsync.Mutex`, `Δunicode.Range16`). The `visitFile` supply loop, however, composed the alias from `packageUsingAlias` alone — the bare, unrenamed name — so it emitted `using sync = sync_package;` (or `using unicode = unicode_package;`) while the reference read `Δsync.Mutex`: the alias binds nothing (CS0246), and the bare alias would itself collide with the child namespace (CS0576). The supplied alias is now routed through `importQualifier` (`getSanitizedImport(importQualifier(alias))`, the same rename every canonical import applies), so the emitted `using Δsync = sync_package;` matches the reference. `importQualifier` is a no-op for any package whose alias is not renamed, so a non-colliding supplied alias stays byte-identical. The trigger is a file that reaches a renamed package's type through the supply route rather than a canonical import — overwhelmingly a **dot import** (`. "sync"` / `. "unicode"`), where the dot brings names in via `using static` yet the converter still qualifies the type, and no canonical `using <pkg> = …` is emitted to carry the rename. Production stdlib code essentially never dot-imports, so the defect stayed latent as an *unused* supplied alias (reflect's `value.go`/`makefunc.go` inferred `sync` without importing it — the `using sync` alias was never referenced, so the wrong spelling compiled); it surfaces in an EXTERNAL (`_test`) variant that dot-imports the package under test, which `unicode`'s `letter_test.go` does (`. "unicode"` + qualified `Range16`/`RangeTable`/`CaseRange`). Because every currently-compiling site had the alias *unused*, the change only ever flips an unused alias (compile-neutral) or fixes a broken one — no site that used `Δpkg.` while getting the bare supplied alias could have compiled. (Guarded by the `DotImportRenamedPackage` behavioral test — `. "sync"` with a `*Mutex` type reference forcing the qualified `Δsync.Mutex` position, output-compared vs Go; neutering the fix reproduces the reported `CS0246: 'Δsync' could not be found`.)

**`uintptr(unsafe.Pointer(x))` builds no `Pointer` object — the operand converts straight to `uintptr` (2026-08-03, r39c).** Go's most common syscall idiom converted to `(uintptr)new @unsafe.Pointer(x)`, and that object was provably dead. golib's `Pointer` is a `ж<uintptr>` whose only value-taking constructor takes a `uintptr`, so the operand is ALREADY converted — by `implicit operator uintptr(ж<T>)`, the very operator the enclosing cast would use — before the wrapper exists; the wrapper stores that finished number in its own one-element slot, and the cast reads it straight back out. The round-trip is the identity, exactly: `uintptr(Pointer)` returns `value.IsNull ? 0 : value.Value`, and the constructor marks the box nil precisely when the address is 0 (`base(value, value == 0)`, with `ж<uintptr>`'s value-peeking `IsNull` arm unable to fire for a value-typed pointee). So the wrapper is no longer emitted:

```go
r1, _, e1 := Syscall(procConvertSidToStringSidW.Addr(), 2, uintptr(unsafe.Pointer(sid)), uintptr(unsafe.Pointer(&stringSid)), 0)
```
```csharp
var (r1, _, e1) = Syscall(procConvertSidToStringSidW.Addr(), 2, (uintptr)Ꮡsid, (uintptr)ᏑstringSid, 0);
```

**The pin is what makes this safe to elide, and it is not the wrapper's.** Since r38 every managed address handed to native code is pinned for the POINTER's lifetime (*EVERY managed address handed to native code is pinned…*, above) — but `EnsureStableAddress` / `pinnedArrayData` set `m_pin` on the **operand** box, released when the operand is collected. The `Pointer` wrapper owns no pin, holds no reference to the operand, and tracks no lifetime; its `ж<uintptr>` slot holds a number. Eliding it therefore cannot shorten any pin, move any address, or change any value — a strictly-dead allocation, which is why this is a peephole and not a semantic change.

The rule is keyed at the **wrapper's own emission site** (`unsafePointerBoxEmission`, marked per-CallExpr by `markDeadUnsafePointerBox` from the enclosing conversion), so it is self-limiting: the `unsafe.Pointer` arms that render a raw address by other means — `@unsafe.Pointer.FromRef(ref x)` for a deref-aliased pointer receiver, `((@unsafe.Pointer)(uintptr)v)` for a named uintptr/pointer operand, and `(@unsafe.Pointer)default!` for the literal `nil` — never consult the mark and are byte-identical. The enclosing conversion must be one that renders a `(uintptr)` cast around the operand, which is the basic `uintptr(…)` target and the named-over-uintptr target that hops through its underlying (`Handle(…)`, `syscall/security_windows.cs`'s `LocalFree` defers). An operand that binds looser than a cast is parenthesized (`unsafe.Pointer(uintptr(p) + off)`); an address-of operand is not, because `&x` renders as the primary box form `Ꮡx` / `Ꮡ(…)` / `Ꮡs.at<T>(i)`.

Corpus footprint: **494 sites across 45 files** in a seeded two-temp-root A/B whole-stdlib reconvert — 249 in `syscall/zsyscall_windows.cs` alone, 65 in `internal/syscall/windows/zsyscall_windows.cs`, 25 each in `runtime/heapdump.cs` and `runtime/os_windows.cs` — plus 10 across four behavioral goldens; every changed line in the A/B is reproduced exactly by the transformation and none is anything else. Measured with a `GC.GetAllocatedBytesForCurrentThread` probe over 2,000 calls: `syscall.Write` **1,072 → 544 B/op** (−49.3 %) and `os.File.WriteString` **9,208 → 8,680 B/op**, i.e. 528 bytes — three wrapper objects — off every zsyscall wrapper call chain. (The hand-owned `crypto/subtle/xor_generic.cs` keeps its three sites, as it must: the converter regenerates it into the `xor_generic.cs.auto` review sibling instead of over it.) (Guarded by the `UintptrUnsafePointerIdiom` behavioral **output** test — address identity across two takes of one global, a pointer parameter, the nil pointer's 0, an array-element stride, an existing `unsafe.Pointer` value round-trip, a dereferenced pointer-to-pointer operand, and a write read back through the same pointer, all vs `go run` — plus the re-baselined `NilPointerUintptr`, `NilPointerParamUnsafePointer`, `FixedArrayBufferPointer` and `UnsafeOperations` goldens, which diverge the moment the peephole is neutered.)

**⚠ CORRECTED 2026-08-23: the RECEIVE half below is misattributed — it is not this family.** The send half stands exactly as written. The read panic was traced to `(ж<array<T>>)(uintptr)` reinterpreting native bytes as a managed array reference (`array<T>` holds a `T[]`, so a native-backed box fabricates a reference and dereferences it) — measured in GolibTests with no kernel involved, 61 corpus sites, 35 in `runtime`. The argument used to attribute it here — that the identical round-trip works in the WRITE direction because TCP exercises it — was false: `sockaddrInet4ToRaw`'s only callers are the `WriteMsg` family, which nothing on the roster reaches, so that direction was UNEXERCISED rather than working.

**Sixth member — the DATAGRAM pair, and the first where the SEND and the RECEIVE need different remedies (2026-08-23, lane R).** `internal/syscall/windows`' `WSASendtoInet4/6` were bodyless linkname partials, so every UDP send on Windows threw the PartialStubGenerator's stub. Filling them exposed both halves of the family at once.

The SEND is the family's ordinary shape and takes the family's ordinary remedy: the dead generated body passed `Ꮡto.sockaddr()` — a pointer INTO A MANAGED BOX — straight to `WSASendTo`, so the hand-own writes the address image into a `stackalloc` buffer through `syscall`'s `GoWriteNativeSockaddrInet4/6` seam first. One definition of the layout, shared with the Linux half (`internal/syscall/unix/linux/net_linux_impl.cs`), so neither can drift.

What is new is the SUBMIT. A Windows datagram send is an *overlapped* operation, and the three helpers it needs (`operationFor`/`Rearm`/`stageBuffers`) are private to `syscall`'s WSA hand-own while the declarations they serve live in `internal/syscall/windows`. Publishing them would put a Go-shaped seam on `syscall`'s public surface, which that file's own header exists to refuse. The resolution — ratified as netpoll design §4.7 — extends golib's platform-neutral `GoAsyncIO` by two primitives stated without naming Winsock:

```csharp
public static nuint RearmOperation(nuint descriptor, object waiterKey, nint mode);   // the native OVERLAPPED, prepared for a new submit
public static nuint StageOperationBuffer(object waiterKey, int byteCount);            // N bytes whose lifetime is the operation's
```

The caller writes the `WSABUF` bytes into the staged memory itself, exactly as it writes the sockaddr image. `syscall`'s own helpers become in-package callers of the same two primitives, so there is ONE record store rather than two that must agree — which is the property that makes this an extension rather than a parallel mechanism.

**The RECEIVE is the same family and a DIFFERENT remedy, and the distinction is worth carrying.** With the send working, `ReadFromInet4` panics `index out of range [0] with length 0` inside `rawToSockaddrInet4`. Do not attribute that to the `(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(...))` round-trip it panics in: the WRITE direction (`sockaddrInet4ToRaw`) runs the identical round-trip and works, and TCP exercises it. The fault is the box — `@new<syscall.RawSockaddrAny>()`, managed arrays inside, handed to `WSARecvFrom` by address. Per the AV-vs-panic triage rule (golib's `array<T>` indexer is bounds-checked), a clean bounds panic means the array is EMPTY: nothing was materialised at that address. Because a received address arrives *asynchronously*, the stack-buffer remedy does not apply — the native staging buffer must be decoded at HARVEST time, which is the decode-side problem `AcceptEx`'s output buffer also has. Send and receive are therefore separate increments, and the guard (`UdpLoopbackRoundTrip`, Linux-proven) waits for the second.


## Pointer-derived funnel arguments and bridged `unsafe.Pointer` arguments keep their box alive across the call — the statement-scoped `GC.KeepAlive` drain

Go's `//go:uintptrkeepalive` contract (cmd/compile's `escape.rewriteArgument`, `cmd/compile/internal/escape/call.go`): an argument that is a `uintptr(<operand of type unsafe.Pointer>)` conversion to a syscall funnel keeps the pointed-to object alive for the duration of the call — the inline `uintptr(unsafe.Pointer(&x))`, the two-step `_p0 = unsafe.Pointer(&p[0]) … uintptr(_p0)` that mksyscall emits for every `[]byte` argument, and a pointer-typed variable passed as `uintptr(unsafe.Pointer(p))` alike. The funnel set is package-qualified through go/types (`syscallFunnelCall`, `syscallKeepAliveAnalysis.go`): `syscall.Syscall/Syscall6/9/12/15/18/SyscallN/RawSyscall/RawSyscall6`, `internal/runtime/syscall.Syscall6`, darwin's lowercase libc trampolines `syscall`, `syscall6`, `syscall6X`, `syscallX`, `syscallPtr`, `rawSyscall`, `rawSyscall6` (runtime linknames declared by package `syscall`), and `crypto/x509/internal/macos`'s own `syscall` (corefoundation.go). In C# the argument is a NUMBER over a pinned box, and nothing references the box once the argument is evaluated, so a collection during the call retires the pin the kernel is reading through. Emitted form: each pointer-derived argument is captured into a `ᴋN` temp declared before the statement, the call takes `(uintptr)ᴋN`, and `System.GC.KeepAlive(ᴋN);` follows the statement (`convSyscallFunnelCall`, drained by `visitStmt`):

```go
func recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error) {
	var _p0 unsafe.Pointer
	if len(p) > 0 {
		_p0 = unsafe.Pointer(&p[0])
	} else {
		_p0 = unsafe.Pointer(&_zero)
	}
	r0, _, e1 := syscall6(abi.FuncPCABI0(libc_recvfrom_trampoline), uintptr(fd), uintptr(_p0), uintptr(len(p)), uintptr(flags), uintptr(unsafe.Pointer(from)), uintptr(unsafe.Pointer(fromlen)))
	n = int(r0)
	if e1 != 0 {
		err = errnoErr(e1)
	}
	return
}
```

```csharp
internal static (nint n, error err) recvfrom(nint fd, slice<byte> p, nint flags, ж<RawSockaddrAny> Ꮡfrom, ж<_Socklen> Ꮡfromlen) {
    nint n = default!;
    error err = default!;

    @unsafe.Pointer _p0 = default!;
    if (len(p) > 0){
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var ᴋ16 = _p0;
    var ᴋ17 = Ꮡfrom;
    var ᴋ18 = Ꮡfromlen;
        var (r0, _, e1) = syscall6(abi.FuncPCABI0(libc_recvfrom_trampoline), (uintptr)fd, (uintptr)ᴋ16, (uintptr)len(p), (uintptr)flags, (uintptr)ᴋ17, (uintptr)ᴋ18);
    System.GC.KeepAlive(ᴋ16);
    System.GC.KeepAlive(ᴋ17);
    System.GC.KeepAlive(ᴋ18);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}
```

The MANAGED-callee member of the same class (measured 2026-09-05 on runtime's `TestSmhasherWindowed`): the converter bridges every value-consumed call whose RESULT is `unsafe.Pointer` with `(uintptr)` (`convCallExpr`), so `memhash32(noescape(unsafe.Pointer(&i)), seed)` hands the callee a number with the Pointer's retained source stripped, and the frame-minted box `Ꮡi` has no reference left after `FromPinnedBox` returns. A bridged `unsafe.Pointer`-returning CALL argument (not a conversion, not a builtin) over `unsafe.Pointer(&x)` of a frame-minted local — a parameter re-minted as a box included — names `Ꮡx` for a `System.GC.KeepAlive(Ꮡx)` after the statement (`bridgedWrapperKeepAliveBoxes`); a `return f(…)` hoists the call into a `ᴛN` temp so the KeepAlive sits between the call and the return:

```go
func int32Hash(i uint32, seed uintptr) uintptr {
	return memhash32(noescape(unsafe.Pointer(&i)), seed)
}
```

```csharp
internal static uintptr int32Hash(uint32 iʗp, uintptr seed) {
    ref var i = ref heap(iʗp, out var Ꮡi);

    var ᴛ2 = memhash32((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡi)), seed);
    System.GC.KeepAlive(Ꮡi);
    return ᴛ2;
}
```

Boundaries, each guarded in `syscallFunnelSet_test.go`: a BARE `f(unsafe.Pointer(&x))` argument is not bridged (the Pointer retains its box through the call) and records nothing; a package-level variable's box is a static field and records nothing; an outer CONVERSION (`n := uintptr(noescape(unsafe.Pointer(&i)))`, runtime's `stdcallN` / `mstart0` shape) records nothing, because a stored number is outside the class — a KeepAlive after the store would hold the box exactly as far as the store. The drain is per STATEMENT: a box named inside an `if` condition is kept alive by the first statement after the call (inside the body or after the `if` — a use on any path after the call keeps the local live at it); a `for` INIT/POST clause, emitted inside the C# header where no statement can follow, is refused by name (`rejectForClauseKeepAlive`), as a deferred or spawned call is (the contract is statement-scoped and a `defer` runs at unwind); a function literal converts its body against an EMPTY pending list and restores the enclosing statement's afterwards, so a box the enclosing call named before its literal argument was converted is never drained inside the lambda. `src/syscall-keepalive-census.ps1` walks the converted emission: every captured temp has exactly one KeepAlive (arm 1), hand-own pointer arguments are held across their call (arm 2), and no raw pointer-derived funnel argument remains (arm 3 — RED 104 on the pre-cut darwin emission, which had never been through the funnel path).

## A numeric value pun READ is a bitcast — `*(*uint64)(unsafe.Pointer(&f))` boxes nothing

Go's `math.Float64bits` is one line, `return *(*uint64)(unsafe.Pointer(&f))`, and Go compiles it to a
register move. The general pointer-reinterpret emission (this section's neighbours) had to treat it like
any other address-of:
- `f` was heap-boxed because its address is taken;
- it was read back through `~Ꮡf.Reinterpret<float64, uint64>()`.

That is **three counted objects per call** where Go has none: the box, its pinnable slot, and the
reinterpreting field reference. GolibTests' `ValuePunBitcastTests` measures the old body at exactly 3.
Every caller paid it, including encoding/binary, strconv's float formatting, log/slog's float
`Value`s and gob.

**A pun that is only READ, between two same-size predeclared sized numerics** (int8…int64, uint8…uint64,
float32, float64), now renders as golib's value bitcast (`Unsafe.BitCast`). Its `&x` no longer counts as
address-taken, so `x` stays an ordinary parameter or local when that was its only address use:

```go
func Float64bits(f float64) uint64 { return *(*uint64)(unsafe.Pointer(&f)) }
```
```csharp
public static uint64 Float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}
```

**The shapes that keep the aliasing reinterpret, deliberately:**
- a pun that is WRITTEN, e.g. runtime/minmax.go's `*(*uint32)(unsafe.Pointer(&x)) |= …`, whose write
  must land in `x`;
- a pun whose result is addressed;
- a named type;
- `int`/`uint`/`uintptr`, whose width is the target's;
- `bool`, complex and every composite.

A pun read whose `x` is ALSO addressed elsewhere still bitcasts, and its box stays for that other use.
The recognition is a per-package pre-pass (`valuePunOperations.go`), and escape analysis skips the `&x`
it consumes. The `//go:cgo_unsafe_args` lift is the precedent for both.

**Stdlib footprint at `fa18863b94`:** all 12 production pun reads, identical on windows, linux and
darwin:
- math ×4;
- runtime's `float64bits`/`float64frombits` and its two histogram infinities;
- reflect's two float32 register moves;
- internal/runtime/atomic's `Float64.Load`/`Store`.

That is 5 files plus their position maps (9 per target), each target −49/+29, and the census is in
`docs/phase4/probes/c2-value-pun-census/`.

**Guarded by:**
- the `ValuePunBits` behavioral test (an accepted read, an address kept elsewhere, and a write), and
  `UnsafeOperations`, whose golden moves on its own three pun reads;
- the converter's `TestValuePunReadRendersAsABitcastWithoutABox`, controlled both ways;
- GolibTests' `ValuePunBitcastTests`.

## The pointer-word read — `*(*unsafe.Pointer)(unsafe.Pointer(&x))` emits a CARRYING pointer, never a byte pun

Go's idiom for reading the pointer word stored at some location — `time.syncTimer`'s
`*(*unsafe.Pointer)(unsafe.Pointer(&c))` reading a channel's header word, `runtime/stack.go`'s
`*(*unsafe.Pointer)(&pp)` re-typing a `uintptr` — has ONE destination the managed storage
reinterpret can never serve: `unsafe.Pointer` itself is a CLASS in the surrogate model, so golib's
alias gate refuses it, and the pre-2026-08-25 fallback deref-copied whatever bits sat in the
source's first reference-sized slot INTO a `Pointer` reference. A reference materialized from bytes
is a CLR type-safety break: junk dispatch on a quiet heap, an `AccessViolationException` when the
punned bits land unmapped — measured live on every `NewTimer` (the `asynctimerchan=2` witness,
2026-08-24: two pipeline runs died in `Pointer.op_Implicit`; three structurally-faithful quiet-heap
repros passed, the byte-view census's latent-with-live-trigger shape exactly).

`reinterpretManagedEmission` (convCallExpr.go) therefore special-cases a target pointee of the
`unsafe.Pointer` basic and emits the word the two ways the managed model can carry one:

```go
u := uintptr(0xC0FFEE)
q := *(*unsafe.Pointer)(unsafe.Pointer(&u))   // the word IS the number

c := make(chan int, 1)
p := *(*unsafe.Pointer)(unsafe.Pointer(&c))   // the word is a REFERENCE
```

```csharp
@unsafe.Pointer q = ~Ꮡ(new @unsafe.Pointer(~Ꮡu));           // exact fidelity: Pointer(value)
@unsafe.Pointer p = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));   // the source BOX's pin token
```

The uintptr arm is exact Go semantics. The managed arm carries the source box's
`ManagedPointerTokens` pin token — non-nil for a live value, stable for the box's lifetime, and
provenance-resolvable back to the storage it names. One corner is knowingly inexact and stated at
the emission: a nil channel's word is 0 in Go while a box token is non-zero; the one stdlib
consumer (`newTimer`) reads only the nil-bit and recomputes it from the GODEBUG setting, and no
stdlib site passes a nil channel through this shape.

**Deliberately NO golib-side guard backs this up**, and the history is the load-bearing part: a
runtime refusal (return a zero-holding box for any reference-typed destination) was tried first and
the full behavioral suite failed `PointerCastSliceRange` — the `ж<T> → ж<U>` DOUBLE-pointer pun
(reflect `MapOf`'s `**(**mapType)(unsafe.Pointer(&imap))`) reads one ж instantiation's reference
slot as another and works precisely because the generic layouts coincide, a distinction golib
cannot draw generically and the converter draws exactly. Guarded by
`tests/Behavioral/UnsafePointerWordRead` (both arms vs `go run`, token stability across re-reads);
the corpus regen moved all 11 members of the shape (`sleep.cs` + ten runtime sites) and nothing
else. The 10 runtime sites are dormant raw-metal; a site that WRITES through the derived pointer
stores into a detached box rather than corrupting punned memory, and real write-through semantics
belong to the provenance arc.

## A pointer `reflect` handed out as an `unsafe.Pointer` must convert BACK — the order token is remembered, not redefined

A Go pointer to managed storage has no machine address to report, so every projection of one to a scalar answers with a stable **order token** instead: `INilPointer.PointerOrderToken`, whose own remarks say plainly that tokens "are order keys, never an identity substitute". `reflect.Value.Pointer` and `reflect.Value.UnsafePointer` both project through it (`reflect/value_impl.cs`'s `reflectPointerToken`), and that contract is exactly right for the consumers it was written for — `fmt`'s `%p`, and `internal/fmtsort`'s ordering of pointer-keyed map entries, which compares two tokens arithmetically.

It is not sufficient for the other direction Go permits: converting the scalar back to a pointer and dereferencing it. `go/types`' own test suite does precisely that to reach the unexported `Config._Trace` field:

```go
func boolFieldAddr(conf *Config, name string) *bool {
	v := reflect.Indirect(reflect.ValueOf(conf))
	return (*bool)(v.FieldByName(name).Addr().UnsafePointer())
}

*boolFieldAddr(&conf, "_Trace") = manual && testing.Verbose()   // check_test.go:166
```

which converts to

```csharp
internal static ж<bool> boolFieldAddr(ж<types.Config> Ꮡconf, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(Ꮡconf.OrTypedNil()));
    return (ж<bool>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}
```

`ж<T>`'s `explicit operator ж<T>(uintptr)` builds a **native-address** box over whatever number it is handed (`m_nativeAddr`), so the `.Value` store writes a `bool` at the numeric value of an order token — an access violation when that page is not mapped, and silent heap corruption when it is. It killed `go/types`' converted test host outright at the first test that reaches the idiom, `TestCheck/blank.go`, with 542 verdicts behind it; the fault arrives as a bare `System.AccessViolationException` in `testFilesImpl` with no frame below it, because the faulting store is inlined at the call site.

The information needed to do better is never actually lost. `reflect.Value.Addr` surfaces the **real aliasing box** — an addressable Value already carries `addrBox`, minted by `GoReflect.FieldAliasBox` — and only the projection to a scalar discards it. So golib's `ManagedPointerTokens` (`golib/ж.PointerTokens.cs`) remembers the association the projection drops: the token that was handed out, and the box it named. `reflectPointerToken` registers on the way out, and the `uintptr → ж<T>` operator consults the table **first**, so a token that came from there recovers its own box and aliases the original storage exactly as Go's pointer would. Everything else keeps the pre-existing native-address route unchanged.

Two properties are deliberate. **The token VALUE does not change.** The obvious alternative — minting self-identifying handles from a reserved numeric range — would also change what `%p` prints and what order pointer-keyed maps print in, because those read the very same token; carrying the association out of band instead leaves every existing observable byte-identical, and costs one dictionary probe on a conversion that was already allocating an object. **Entries are weak, and verified on resolve**: the table must never be the reason a box stays alive, and a resolve re-derives the box's current token and requires equality, so a stale entry cannot answer. The type-descriptor path (`typeDescriptorOrderToken`, which packs type NAMES so fmtsort orders them lexically) returns before registration and is untouched — those tokens are shared by every descriptor with the same name and are not identities to recover.

Corpus footprint of the idiom: in all of GOROOT it is `go/types`' `check_test.go` (bool and string), its `cmd/compile/internal/types2` twin, and four descriptor-walking sites in `reflect/type.go`; `sync/atomic`'s two uses only test alignment (`p&7 != 0`) and never dereference. (Guarded by the `ReflectFieldAddrWrite` behavioral **output** test — writes to a bool, a string and an int field through `Addr().UnsafePointer()`, each read back through the ORIGINAL struct to prove aliasing rather than a detached copy, two derivations of one field compared for pointer equality, and the untouched neighbours asserted unchanged, all vs `go run`. It faults with an access violation on pre-fix golib.)

## An OPAQUE `*struct{}` conversion mints a recoverable token — `syscall.Pointer(unsafe.Pointer(p))` keeps its referent

Windows type definitions use `type Pointer *struct{}` for a "pointer to one of many types" field — `CERT_CHAIN_POLICY_PARA.pvExtraPolicyPara`, `WSAMsg.Name` — and Go populates it with `T(unsafe.Pointer(p))` over whatever record the API expects. The emission used to be the numeric chain `(T)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer(p))`, which projects p's box to a scalar at the `@unsafe.Pointer` constructor (the class holds only a number — `Pointer : ж<uintptr>`). For a pointee **carrying managed references** that scalar is a transient GC-heap address with no recoverable box behind it: golib's `uintptr` operator pins only reference-free storage, so the number that reaches the boundary is neither stable nor resolvable, and the referent's liveness ends at the JIT's discretion.

The measured victim was crypto/x509's `checkChainSSLServerPolicy` — the mint-site problem the CryptoAPI chain arc left open by name. Its `SSLExtraCertChainPolicyPara` holds `ServerName *uint16`, a managed reference, so the SSL policy parameter crossed into `CertVerifyCertificateChainPolicy` as a wrong-layout transient address: `Fatal error 0xC0000005` inside `Syscall6`, the last crypt32 member standing between `crypto/tls` and a roster row.

The conversion now emits golib's referent-preserving mint whenever the target's underlying type is `*struct{}` and the source is `unsafe.Pointer(p)` over a Go pointer whose box the emitter has in hand:

```go
sslPara := &syscall.SSLExtraCertChainPolicyPara{ AuthType: syscall.AUTHTYPE_SERVER, ServerName: servernamep }
para := &syscall.CertChainPolicyPara{ ExtraPolicyPara: (syscall.Pointer)(unsafe.Pointer(sslPara)) }
```

```csharp
var sslPara = Ꮡ(new syscall.SSLExtraCertChainPolicyPara(AuthType: syscall.AUTHTYPE_SERVER, ServerName: servernamep));
var para = Ꮡ(new syscall.CertChainPolicyPara(ExtraPolicyPara: ((syscall.Pointer)ManagedPointerTokens.MintOpaque(sslPara))));
```

`ManagedPointerTokens.MintOpaque` (`golib/ж.PointerTokens.cs`) keeps the numeric route **byte for byte** for every pointee that route already answered exactly — nil is 0, a native-backed box is its real address, a reference-free pointee pins and reports stable storage — and diverges only for the reference-bearing class, where three things happen at the mint: the scalar becomes the box's own `PointerOrderToken`; the token is `Register`ed so a boundary wrapper recovers the box with `Resolve` (the same round trip the reflect projection above and the ADDRINFOW hand-own's sockaddr fields take — this is the table's third minter); and the minted `ж<EmptyStruct>` holds the referent reachable for its own lifetime through a `ConditionalWeakTable`, so the opaque pointer keeps its pointee alive exactly as the Go pointer it stands for would — the referent is otherwise reachable only through a local the JIT is free to retire before the syscall that consumes the token.

The consuming side is `syscall`'s hand-owned `CertVerifyCertificateChainPolicy` (`zsyscall_windows_certchain_impl.cs`): a scalar the table recognizes as an `SSLExtraCertChainPolicyPara` box is transcribed — server name and all — into native mirrors for exactly the duration of the call; a scalar it does not recognize passes through unchanged, which is both the nil pointer and any genuinely native (or pinned reference-free) address, for which pass-through is the correct native call.

Scope is deliberate on three edges. The target must be `*struct{}`-underlying: such a type names an opaque pointer **by construction** (there is nothing to dereference), so no reader can depend on the scalar being a dereferenceable address; wider named-pointer targets keep their existing routes. The source must be the `unsafe.Pointer(p)` call form — a stored `unsafe.Pointer` variable or a `uintptr` has already lost its referent and keeps the numeric chain. And the emission-level fix, rather than a hand-own of the one x509 function, is what covers every author of the Go shape — `internal/poll`'s six `WSAMsg.Name` mints (whose `RawSockaddrAny` referents now stay alive for the message's lifetime, ready for the WSA-msg wrappers when that arc lands), converted test suites, and user code. (Guarded by the `SystemCertVerify` behavioral **output** test's policy rows, which drive the same mint from test code against a trusted-by-waiver chain: with `CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG` set, a MATCHING server name answers `0` and a MISMATCHED one answers `CERT_E_CN_NO_MATCH` — an answer crypt32 can only give if the name crossed the boundary intact — and with nothing waived the same chain answers `CERT_E_UNTRUSTEDROOT`. Pre-fix, the first policy call dies with the access violation above.)

## Pointer DISPLAY never dereferences out-of-range; `unsafe.StringData("")` is nil

Printing a pointer (`ж<T>.ToString()` → `PrintPointer`, the stub-fmt fallback for `%v`/`%p` of a
pointer) only needs an address-like `0x…` token, but the printer read `ptr.Value` to derive one —
and an array/slice-ELEMENT reference can legally sit outside its backing store's valid range (the
zero index of an EMPTY pinned buffer, or one-past-the-end pointer arithmetic), where that read
throws `IndexOutOfRangeException` and kills the host (strings' `TestClone`, Phase-4 row R9).
`PrintPointer` now checks an element reference's index against its backing store first and prints
the BACKING STORE's identity when the element is unreadable — stable per pointer box, never a
throw. Relatedly, `unsafe.StringData` of an EMPTY string now returns **nil**: Go documents the
empty-string result as unspecified-may-be-nil, its runtime returns nil (probed — so distinct empty
strings' data pointers compare EQUAL, which `TestClone` asserts), and golib's
pin-a-fresh-buffer-per-call implementation could never satisfy that identity. Addresses differ run
to run, so behavioral coverage checks printed SHAPE and nil-identity (`UnsafePointerPrint`); the
out-of-range print itself has no Go-parity spelling from converted code today (the
`unsafe.Add`-through-`unsafe.Pointer` seam loses the element box), so that property is guarded at
the golib level by `GolibTests.PointerPrintTests` — a golib UNIT-test project (beside
`ChannelTests` under `/tests/library/`) for runtime properties no Go↔C# output comparison can
reach.

**`unsafe.String(ptr, 0)` reads nothing, so it must not pin `ptr` either.** That same empty-buffer
element reference reaches the string builtin, by a route the standard library takes constantly on
Windows: `syscall.UTF16ToString` truncates its argument at the first NUL and returns
`unsafe.String(unsafe.SliceData(buf), len(buf))`, so an **all-NUL** `[N]uint16` — every unset
`WCHAR` field of a Win32 record (`WIN32_FIND_DATAW.cAlternateFileName` on a volume with 8.3 name
generation disabled, `MIB_IFROW.wszName`, `PROCESSENTRY32.szExeFile`, `STARTUPINFO.lpDesktop`) —
arrives as a zero-length `buf`. `SliceData` is documented to return a **non-nil** pointer to an
unspecified address for a non-nil slice of capacity 0 (only a *nil* slice yields nil), which this
model materializes as an index-0 box into a zero-length backing array; `unsafe.String` then pinned
that referent (`fixed (byte* p = &ptr.Value)`) to build the string and threw
`IndexOutOfRangeException` where Go returns `""`. The zero-length case now returns the empty string
**before** the pointer is touched, which is precisely Go's rule: a run-time panic occurs only when
`ptr` is nil *and* `len` is not zero, so a length of zero dereferences nothing whatever the pointer
is. `SliceData` is deliberately left alone — Go specifies the non-nil-pointer-to-unspecified-address
result for `cap == 0`, so returning nil there to dodge the deref would trade a throw for a wrong
answer. `unsafe.Slice(ptr, 0)` needs no matching guard: its element-window route
(`TryGetElementWindow`) already yields an empty aliasing window over a zero-length backing without a
deref. (Guarded by the `UnsafeStringEmpty` behavioral **output** test — the all-NUL `[14]`/`[260]`
buffers, terminated/unterminated/leading-NUL controls that keep truncation honest, and the
`SliceData`/`StringData` zero-length matrix with non-zero-length positive controls, compared vs
`go run`; verified to throw with the guard removed.)

**`unsafe.SliceData` is an INTERIOR POINTER, not a pin.** Go defines it as `&slice[:1][0]`, so the
faithful model is the array-element reference `Ꮡ(s, 0)` — the exact box the converter emits for
`&s[0]`. It was instead a pinned-buffer box over `slice.buffer`, and that was wrong three ways at
once:

| The pin | The consequence |
|:--|:--|
| `GCHandle.Alloc(…, Pinned)` refuses storage whose element type carries a managed reference | `SliceData` over **any** such slice threw `ArgumentException: Object contains references`. `log/slog`'s `GroupValue` is the corpus witness — `groupptr(unsafe.SliceData(as))` over `[]Attr`, rebuilt by `unsafe.Slice` in `Value.group()` — and it infrastructure-errored every grouping path in the package (5 `testing/slogtest` rows). |
| The pin covered the whole backing array from index 0, ignoring the slice's LOW bound | `SliceData(s[2:])` addressed element 0 rather than element 2, and did not compare equal to `&s[2]` as Go's pointer identity requires. |
| `PinnedBuffer` implements `IArray<byte>` alone | `ж<T>.Value`'s `array is IArray<T>` test failed for every element type but `byte`, so the derived pointer was undereferenceable — `InvalidOperationException` instead of the element. |

Pinning was never what `SliceData` *means*. An address is needed only when the pointer is converted
to `uintptr`/`void*`, and `ж` already pins on demand at exactly those conversions
(`EnsureStableAddress`), declining gracefully for storage that cannot be held still. The element
reference additionally makes the round trip **alias** rather than snapshot — `unsafe.Slice`'s
`TryGetElementWindow` arm rebuilds a window over the original backing, so a write through the
rebuilt slice reaches the source, which is Go's semantics. `StringData` keeps its pin: a `@string`
is a `byte[]`, always pinnable, and its emptiness identity is the property `TestClone` asserts.
(Guarded by the `UnsafeSliceDataAliasing` behavioral **output** test — a `[]struct{string;int}`
round-tripped through `SliceData`/`Slice` and written through, `SliceData(s) == &s[0]` identity, a
re-sliced source's low bound, an `[]int64` and a `[]*T` deref, and the plain `[]byte` control —
plus `UnsafeStringEmpty`, which pins the zero-length behavior above unchanged.)

## Reinterpreting a pointer to a defined type with identical underlying — `(*Base)(p)`
A Go conversion `(*Base)(p)` where `p` is a `*Def` and `Base`/`Def` share an *identical underlying* type (one is a defined type over the other, e.g. `type pinnerBits gcBits`, or both over the same type) reinterprets the pointer. C# has no conversion between the two distinct generic instantiations `ж<Def>` and `ж<Base>`; only the `[GoType]` wrapper's **value** conversion `Def ↔ Base` exists. So the converter performs the reinterpret on the value and re-boxes it:
```go
func (s *mspan) newPinnerBits() *pinnerBits { return (*pinnerBits)(newMarkBits(s.nelems * 2)) }   // newMarkBits returns *gcBits
```
```csharp
internal static ж<pinnerBits> newPinnerBits(this ref mspan s) {
    return Ꮡ((pinnerBits)(~newMarkBits(((uintptr)s.nelems) * 2)));   // deref the ж<gcBits> box, value-convert, re-box
}
```
The argument is **dereferenced first** (`~box`) when it renders as a genuine pointer box — a call result, a local box, or a pointer field — because the value conversion operates on the underlying value, not on `ж<Def>` (a plain `(pinnerBits)(ж<gcBits>)` is `CS0030`). A deref-aliased pointer **parameter/receiver** already renders as the pointed-to value (`Δp`, not a box), so it value-converts directly with no `~` — the original `(*atomic.Uint32)(p)` receiver case (runtime/mprof `goroutineProfileStateHolder`). Both forms box a **copy** (`Ꮡ`): the shared underlying is the wrapped value, and a defined-over-struct wrapper holds it in a `readonly` field, so there is no write-through to lose; this matches the long-standing copy semantics of this branch (the runtime intrinsics behind these are assembly stubs). Both ships stay in managed `ж<>` land — no raw-address round-trip. (Guarded by `NamedPointerReinterpret`.)

The **third direction** — a pointer to a BASIC type reinterpreted to a defined type over that
basic — takes the same value-convert-and-re-box route: fmt's `(*stringReader)(&str)` (`type
stringReader string`) emits `Ꮡ((stringReader)(str))` — the address-of collapses with the value
deref, restricted to this arm so the long-guarded emissions stay byte-identical. Writes through
the box hit the copy, which is faithful for the pattern (the source string is never re-read).
Guarded by `NamedPointerReinterpret` (`tail`/`consume`). The **deferring-receiver rule** is a
sibling of these box-form decisions: a method that defers or recovers at FUNCTION level and also
references its receiver takes the direct-ж receiver (`this ж<T> Ꮡx`) rather than `this ref T`,
whose deref alias then emits inside the frame's `try` (`bodyWrappedInDeferContext`; fmt `ss.Token`,
guarded by `DeferCallOrder` `acc.add`). The direct-ж form is the alloc-free, race-free one, and it
is also what a deferred closure needs, since a lambda cannot capture a `ref` local.

The same block also covers a **named-numeric pointer reinterpreted to its underlying *basic* type** — `(*uint64)(head)` where `head` is a `*lfstack` (`type lfstack uint64`). This is the runtime's atomic-on-a-named-integer pattern: `atomic.Load64((*uint64)(head))` / `atomic.Cas64((*uint64)(head), …)` on the named atomic types **`lfstack`** (uint64, `lfstack.go`), **`sweepClass`** (uint32, `mgcsweep.go`), **`profAtomic`** (uint64, `profbuf.go`), and **`sysMemStat`** (uint64, `mstats.go`). `ж<lfstack>` and `ж<uint64>` are distinct generic instantiations with no conversion (`CS0030`); the reinterpret condition is generalized from *Named↔Named* to also fire when the **result** elem is a **basic** type whose underlying equals a **named** argument elem's (`namedToBasic`), and again for the reverse (`basicToNamed`).

### These three arms now ALIAS instead of boxing a copy (2026-08-03, r38-gob)

Everything above described the emission as *value-convert-and-re-box*, and justified the copy each time
the shape came up — "no write-through to lose", "the intrinsics are asm stubs", "the source string is
never re-read". **That justification was a property of the call sites the arm happened to have, not of
the conversion**, and Go's `(*U)(p)` says the opposite: the derived pointer names **p's own storage**, so
a write through it is visible through `p`. `encoding/gob` is where the difference stopped being
theoretical. `Gobber.GobDecode` decodes straight back through a reinterpret used as a **call argument** —

```go
type Gobber int
func (g *Gobber) GobDecode(data []byte) error {
	_, err := fmt.Sscanf(string(data), "VALUE=%d", (*int)(g))   // writes THROUGH the reinterpret
	return err
}
```

— which is neither a deref context nor a raw-address source, so it took this arm and emitted
`fmt.Sscanf(…, Ꮡ((nint)(g)))`: `Sscanf`'s write landed in a throwaway box, `g` never changed, and the
decoder returned 0 for 23. All three arms now route through golib's **aliasing** reinterpret —
`reinterpretManagedEmission`, the same emission this file already produced for the identical conversion
in a deref or raw-address context — so the arm's own gate is the only thing that changed:

```csharp
public static error GobDecode(this ж<Gobber> Ꮡg, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%d"u8, Ꮡg.Reinterpret<Gobber, nint>());
    return err;
}
```

`PointerExtensions.Reinterpret` is where the "can the managed model express this alias?" decision already
lives (`ReinterpretAliasesStorage`), so the converter delegates rather than re-deciding; it reports false
for the two shapes that must keep the re-box — an IDENTITY conversion (intercepted upstream by
`pointerReinterpretIdentitySource`) and an **array** pointee, whose lazily-materialized backing store a
storage reinterpret bypasses — so the chain-defined `type pallocBits pageBits` pair the written-RHS gate
deliberately leaves on this arm falls through unchanged. A useful second-order effect: the entry deref
alias these functions carried (`ref var head = ref Ꮡhead.DerefOrNull();`) becomes dead, because the body
now names only the box, so the alias-liveness scan drops it.

**Whole-stdlib A/B (converter-vs-converter, both sides seeded per ritual 1a): 14 files, 41 hunks, every
one this substitution and nothing else** — and the census is what shows the copy was never harmless.
Beyond the runtime's asm-stub atomics (`lfstack`, `sweepClass`, `profAtomic`, `sysMemStat`,
`pinnerBits`), it silently broke real write-through in **`flag`** (every `newBoolValue`/`newIntValue`/…
returns `(*boolValue)(p)`, so a parsed flag never reached the user's variable), **`crypto/tls`**
(`clientShares.ReadUint16((*uint16)(&ks.group))` parsed key-share groups and signature schemes into a
copy), **`crypto/cipher`** (`(*cbcEncrypter)(newCBC(b, iv))`, whose IV mutates per block),
**`image/png`** (`(*encoder)(buffer)` over a pooled `EncoderBuffer`), **`go/types`**
(`(*term)(t)`), and **`crypto/internal/boring/bbig`**. The reconverted corpus builds **304/304, 0
errors**. (Guarded by the extended `NamedNumericPointerReinterpret` behavioral **output** test: the read
path it always covered, plus write-back through an argument-position reinterpret — the gob shape — a
named→named struct reinterpret held in a local and written through a field selector, and a basic→named
`(*namedString)(&s)`; verified to FAIL on stdout with the fix neutered.)

## The club-41 mop-up batch (flag/flate/binary/syntax roots)
Nine coupled rules from the shallow-stack campaign:
- **Named func types implementing interfaces** (flag's `funcValue`): a delegate cannot be a
  partial struct — the generator routes Delegate records to the VALUE adapter
  (`new funcValueᴠValue(v)`), whose Go methods are package extensions binding on the wrapped
  copy; non-struct record kinds SKIP rather than throw (a throw kills the package's entire
  generator run). Guarded by `FirstClassFunctions` (`handler.tag`).
- **Ref receivers never take box renders**: a `[GoRecv] ref` receiver has NO box — the
  escape-heap and lambda-capture convIdent arms fall through to the value alias (flate init's
  `d.fill = (*compressor).fillStore` emitted a nonexistent `Ꮡd`, CS0103). Guarded by
  `FirstClassFunctions` (`worker`).
- **Func-field callees drive argument treatment**: `getFunctionSignature` resolves a
  FUNC-typed field's signature (`d.fill(d, b)` — the receiver arg renders as the box for a
  `ж<T>` slot, CS1503).
- **Integer wrappers carry the UntypedInt bridge** (`(token)(endBlockMarker)` — C# never
  chains two user conversions, CS0030). Guarded by `SortArrayType` (`levelToken`).
- **Package aliases shadowed by method names** qualify through the `_package` class
  (`sort_package.Sort(…)` — flate's `byLiteral.sort` bound the method group, CS0119).
  Guarded by `SortArrayType` (`PeopleByAge.sort`).
  Same-package tests also participate in that shadow set even during an **ordinary production
  conversion**. Under recompile fallback an in-package `_test.go` declaration lands in the SAME C#
  package class and can shadow a production alias; under white-box reference it lives in a bridge,
  but external-test references still need one stable production spelling — yet go/packages'
  production package omits test files. hash/maphash's `smhasher_test.go`
  declares `func (k *bytesKey) bits() int` while `maphash_purego.go` imports `math/bits`, so
  `bits.Mul64(a, b)` binds the method group in the test assembly (CS0119, plus CS8130 on both
  deconstructed results). Every production package conversion therefore performs a cheap,
  build-constraint-aware directory scan of its in-package `_test.go` function/method names and
  folds them into `packageFuncMethodNames`; it loads and type-checks no test dependency graph.
  External-package tests are excluded because they emit into another C# package class. This makes
  production output mode-stable: ordinary and `-tests` conversion both emit
  `math.bits_package.Mul64(a, b)`. When a test-only declaration is specifically responsible, the
  statement also explains the otherwise surprising spelling:

  ```csharp
  // Fully qualified to avoid alias shadowing by the same-package test declaration "bits".
  var (hi, lo) = math.bits_package.Mul64(a, b);
  ```

  This remains a REFERENCE-spelling change only — never a symbol rename, so production names stay
  pinned (see `testMethodRenames`). On entry to `processTestConversion` the sibling list is cleared:
  the in-package variant sees the declaration in its own typed universe, while the external
  variant's declarations live in a different C# class. Guarded by
  `TestSiblingTestDeclaratorsContributeAliasShadow`, including build-excluded and external-test
  negatives plus the exact explanatory comment; hash/maphash's normal-vs-`-tests` production file
  is also byte-identical.
  A **Δ-renamed foreign CONST reached through that fallback** must still substitute the
  renamed member: the composed lookup key (`time_package.Second`) misses the alias map
  (keyed on the plain package name, `time.Second`), so `getAliasedTypeName` retries with
  the `PackageSuffix` stripped and, on a CONST hit, keeps the `_package` qualifier while
  substituting the alias — `time_package.ΔSecond` (crypto/tls's `Config.time` method ×
  time's `Second` const-vs-`Time.Second()` collision; the raw name bound the
  `Second(this Time)` extension method group, CS0019 ×2). Gated to consts: const entries
  exist only for collision-renamed members, while type entries cover every exported type,
  whose raw `_package`-qualified renders already bind. Guarded by
  `ShadowedImportConstLib`/`ShadowedImportConstUser` (the lib Δ-renames `Peak` for its own
  `Meter.Peak` collision; the user's `gauge.ShadowedImportConstLib` method shadows the
  import and `Span(2) * ShadowedImportConstLib.Peak` reaches the renamed const through the
  fallback, output-compared vs Go).
- **Blank params synthesize names when the body discards** (`_ = b[7]` bound the blank
  `littleEndian` receiver, CS0029) — encoding/binary's bounds-check hints. Guarded by
  `TypeSwitch` (`marker.tag`).
- **Defined-over-named-struct composites wrap the underlying** (`decoder{order: o}` →
  `new decoder(new coder(order: o))`, CS1739). Guarded by `NamedPointerReinterpret` (`view{}`).
- **Labeled switches declare their break target** (`break_BigSwitch:;` after the switch —
  both switch visitors now mirror visitForStmt, CS0159). Guarded by `SwitchBreakInCase`
  (`pick`).
- **Short declarations keep the named-numeric cast** (`p := printFlags(0)` re-imposes
  `((printFlags)0)`, CS1503) and **empty-interface switch tags compare via AreEqual**
  (`switch err := recover(); err { case ErrLarge: }`, CS0019).

## Slice-to-array: the VALUE form copies, the POINTER form ALIASES
Go has two slice-to-array conversions and they are different conversions, so each gets its own
golib entry. Both panic Go-style on a short slice.
- The Go 1.20 **value** form `[4]byte(slice)` emits `new array<byte>(s, 4)` — the COPY
  constructor `array<T>(slice<T>, nint)`. Go's conversion copies, so this is exactly faithful
  (netip `AddrFromSlice`, CS1955).
- The Go 1.17 **pointer** form `(*[4]byte)(slice)` emits `Ꮡ(array<byte>.Alias(s, 4))` — an
  ALIASING window over the slice's own backing store. The Go spec is explicit that here "the
  slice and array share their underlying array", so the array is a *view*, and `array<T>` carries
  a `(low, length)` window for exactly this one construction; every other construction spans its
  whole backing, where the window is inert.

**This was a copy until 2026-07-31, and the copy was a silent wrong answer, not a performance
detail** — every write through the pointer was discarded. `image/png`'s encoder is the corpus
witness: its `cbTCA8` row loop converts each four-byte destination window and writes the
un-premultiplied pixel through it,

```go
d := (*[4]byte)(dst)
s := (*[4]byte)(src)
…
d[0] = uint8((uint32(s[0]) * m / a) >> 8)
```

so against a copy *every* pixel write went nowhere and any RGBA image that was not fully opaque
encoded as an all-zero image. It surfaced as `TestWriteRGBA`'s "50/50 Transparent/Opaque RGBA"
and "RGBA with variable alpha" subtests, and the two subtests that did pass passed by luck: the
opaque one takes the `cbTC8` path entirely, and the fully-transparent one wants all-zero output,
which is what a lost write also produces.

The window is where the aliasing is, not merely the sharing: the png loop converts `cr[0][1:]`
and re-slices it forward, so the array's element 0 is not element 0 of the backing store. Three
consequences follow and all three are Go's:
- `array<T>.Clone()` — Go's by-value array copy — materializes the *window*, so a copy is a full,
  offset-free array again;
- `p[:]` bounds its capacity at the array's end (`cap` 4, not the rest of the slice);
- `&(*[4]byte)(s)[0] == &s[0]`: `ж<T>.CanonicalElement` resolves an element pointer taken through
  a window to the same absolute element as one taken through the slice.

Guarded by `SliceToArrayPointerAlias` (offset write-through, read-back through the pointer,
value-conversion copy, deref copy, `p[:]` aliasing and `len`/`cap`, element identity, and the
png-shaped windowed loop) and by `NamedPointerReinterpret` (`sliceToArray`). The POINTER-sourced
sibling `(*[N]T)(unsafe.Pointer(p))` is
[below](#an-element-pointer-reinterpreted-as-an-array-pointer-aliases-the-elements-storage).

## An element pointer reinterpreted as an array pointer ALIASES the element's storage
`(*[N]T)(unsafe.Pointer(p))` with `p` a `*T` is the same conversion reached from a POINTER instead
of a slice, and it emits the same kind of window — `array<T>.AliasPointer(p, N)`, which yields the
`ж<array<T>>` directly. It is what Go's stdlib reaches for whenever a `*T` really names a run of
elements: os's `TestReadStdin` fake fills internal/poll's read buffer with
`copy((*[10000]uint16)(unsafe.Pointer(buf))[:n:n], s16)`, and `syscall.Readlink` decodes a reparse
record through `(*[0xffff]uint16)(unsafe.Pointer(&data.PathBuffer[0]))[:n:n]`.

Before this the shape took the raw-address route (`(ж<array<T>>)(uintptr)(…)`), which cannot
express it in either of its two forms. Dereferenced, that box reads an `array<T>` STRUCT — a
backing-store reference plus bounds — out of the pointed-at *data*, i.e. a fabricated managed
reference. Sliced, the `isPointerCast` fusion in `convSliceExpr` catches it first and produces a
`slice<T>` COPY of the memory, so every write through it is discarded: all 462 of `TestReadStdin`'s
subtests read back zeros (`have [0 0 0…] want [abc…]`).

Three rules bound the arm, and each is a real limit of the managed model rather than a convenience:
- **Same element type only.** `array<T>.AliasPointer` windows the pointer's own `T[]` backing; a
  `T[]` view over differently-typed storage does not exist in C#. `(*[2]byte)(unsafe.Pointer(&x.Port))`
  over a `uint16` keeps the address route — that is the raw-metal fork, unchanged.
- **The window is decided at RUNTIME.** Only a pointer that actually addresses managed array/slice
  storage has a backing to window; a heap box, a struct field or a native address falls back to the
  identical address route inside golib, so nothing that worked before changes shape.
- **`N` is CLAMPED to the storage that exists.** Go's `N` in this idiom is a promise, not a length —
  it is spelled `10000` / `1<<16` / `0xffff` over buffers a few elements long and the result is
  always immediately re-sliced to the real count. Honoring it literally would put the window's own
  bounds past its backing store, where a full-slice expression is an exception rather than a window.
  So `len(*p)` reports the addressable extent, not Go's `N`, in the huge-`N` case; an overrun is
  then a Go-style index panic instead of the silent corruption the same overrun is in Go. (Where `N`
  fits — the ordinary `(*[4]byte)(unsafe.Pointer(&s[i]))` — `len` is exactly Go's.)

One latent defect in the same family fell out with it: `SliceExtensions.slice(this array<T>, …)`
sliced the RAW backing store, ignoring `m_low`/`m_length`, so explicit bounds over ANY window —
`Alias`'s as well as `AliasPointer`'s — addressed the source's elements rather than the array's
(`p[1:3]` of a window over `buf[1:]` yielded `buf[1:3]`, not `buf[2:4]`). The Range indexer already
resolved through the window; the explicit-bounds path now does too, which is the path the
`[:n:n]` idiom takes.

Guarded by `ArrayPointerElementAlias` (write-through at element 0 and at an offset, the huge-`N`
`[:n:n]` copy shape, indexed write-through, read-back through the pointer, deref copy, `p[:]` and
`p[1:3]` aliasing with `len`/`cap`, element identity, a Go fixed-array source, and a re-sliced
view).

## A direct-ж method on a value field-chain boxes through the &-machinery
A direct-ж (box-receiver) method called on a field of a plain VALUE param — netip's
`ip.addr.halves()`, where Go auto-addresses `&ip.addr` — routes the receiver through the
&-machinery: `Ꮡ(ip).of(ΔAddr.Ꮡaddr).halves()`. This boxes a COPY, which is faithful because
the enclosing Go value param is itself a copy: writes through the method could only ever reach
the local copy in Go too. (Pointer-rooted chains and indexed elements take their own
long-standing arms; this is the remaining value-rooted case.) Guarded by
`StructPointerPromotionWithInterface` (`rig`/`probeRig`).

## Field address of a collision-renamed heap-boxed local uses the raw box name
A heap box always keeps the RAW Go identifier (`ref var Δslice = ref heap<T>(out var <box>slice)`), so taking the address of a FIELD of a collision-renamed boxed local routes through `boxBaseName` -- the raw-name box, never the Δ-renamed alias (CS0103; reflect `SliceOf`'s `&slice.Type`). This matches the whole-value `&p` form and the renamed receiver/parameter boxes:
```csharp
internal static void bump(ж<nint> Ꮡnp) {
```
Guarded by `CollisionRenamedLocalBox` (`bump(&p.n)` on the renamed local `p`).

## A capture-mode method on a shadow-renamed heap-boxed local uses the rendered box name
A capture-mode method — one that escapes its receiver's address, e.g. `cryptobyte.Builder.AddASN1`, which hands `&b` to a callback — called on a heap-boxed VALUE local routes through the receiver box: `var b Builder; b.AddASN1(…)` → `Ꮡb.AddASN1(…)`. Unlike a deref-aliased pointer *parameter* (whose box keeps the RAW name, `Ꮡp`), a heap-boxed value LOCAL keeps its box under the RENDERED name — an escaping local is `ref var b = ref heap(new T(), out var Ꮡb)`, so when the local is SHADOW-renamed its box takes the renamed name. crypto/x509 `marshalCertificate`'s inner `serialiseConstraints` closure declares `var b cryptobyte.Builder`, renamed `bΔ1` to dodge the enclosing method's own `var b` declared LATER (a C# lambda cannot re-declare an enclosing-scope local, CS0136); its box is `ᏑbΔ1`. Emitting the raw-name box `Ꮡb` there both mis-references the outer `b`'s box (declared later in the method → CS0841/CS0103) and, where a same-named outer box does resolve, calls the method on the wrong operand — go/types `conversions.go` called `x.convertibleTo` on the receiver box `Ꮡx` instead of the inner operand box `ᏑxΔ2`:
```csharp
ref var bΔ1 = ref heap(new cryptobyte.Builder(), out var ᏑbΔ1);
…
ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ2) => { … });   // was Ꮡb (CS0841)
```
The receiver-box render resolves the box base through `boxBaseName` with the lambda capture-remap DISABLED, so it yields: the shadow-rendered *declaring* name (`bΔ1`) for an escaping local; the raw name (`Ꮡp`) for a pointer parameter; and — critically — the *declaring* name for a variable CAPTURED by the closure, not its value-snapshot capture name. A heap-boxed local captured by a closure has its box captured directly (`Ꮡonce` in sync `OnceFunc`'s returned closure), so the capture-remapped `Ꮡonceʗ1` (a non-existent box) must not appear. Guarded by `ShadowedHeapBoxReceiver` (an inner closure's `var b` capture-mode method, shadow-renamed against an outer same-named `var b` declared later).

## Nested dereferences parenthesize before the outer `.Value`
A deref whose operand is ITSELF a deref renders with the prefix `~` form, on which a naked postfix `.Value` mis-binds (postfix beats unary: `~X.Value` is `~(X.Value)`). The outer deref wraps the inner one -- reflect `MapOf`'s `**(**mapType)(unsafe.Pointer(&imap))`:
```csharp
var back = (~(ж<ж<array<int64>>>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡip).Value))).Value;
```
Guarded by `PointerCastSliceRange` (compile-shape).

## Function literals returning `unsafe.Pointer` state their return type
A literal with a single `unsafe.Pointer` result can mix return arms of DIFFERENT C# types (reflect `deepEqual`'s `ptrval`: `(uintptr)v.pointer()` on one arm, the raw `v.ptr` on the other), which defeats C# lambda return-type inference (CS8917). The emitted lambda states its return type explicitly; each arm then converts implicitly through the golib operators:
```csharp
var pick = @unsafe.Pointer (bool u) => {
```
Guarded by `PointerCastSliceRange` (compile-shape).

## Interface-returning literals with distinct arm types state their return type too
The same inference gap hits an interface result whose arms return DIFFERENT concrete types — net ipsock.go's `inetaddr := func(ip IPAddr) Addr` returns three pointer-adapter classes (`TCPAddrжΔAddr` / `UDPAddrжΔAddr` / `IPAddrжΔAddr`), which share only the interface (CS8917). When a single non-empty-interface result's return arms carry two or more distinct types, the lambda states the return type explicitly (`Addr (IPAddr ip) => …`); each arm then converts implicitly. Single-typed literals keep the inferred form (zero churn). (Guarded by the `InterfaceCasting` extension `makeAnimal` — an adapter arm plus a value arm, runtime-verified.)

**And the opposite end of the same gap: arms that are ALL untyped `nil`.** `client := func(*TCPConn)
error { <-serverDone; return nil }` (net `net_test`) renders its only arm as `default!`, which carries
no natural type at all — so where the rule above has too many candidate types, this has none, and it
is the same CS8917. The interface arm now also states the return type when the literal is in
assignment position, has a return, and **every** single-result arm is untyped nil. Drift-free by
construction: an all-`default!` arm set never had an inferable natural type, so every site the rule
touches is a site that did not compile. It is the single-result twin of the multi-result
`!hasFullyTypedArm` rule immediately below, and it stays out of argument/return position for the same
reason that one does — those literals are target-typed by their delegate, so nothing is inferred.
(Guarded by the `FuncLitStringConcatReturn` extension: an all-`nil` literal in both `:=` and `var`
form, plus a mixed-arm control that must KEEP inferring.)

## Multi-value literals with no fully-typed arm state their return type — named results included
The single-result inference gaps above generalize to any MULTI-result literal where EVERY return arm carries a typeless element — `return nil, nil, nil, nil, err` on the error arms and `return dnsNames, ips, emails, uriDomains, nil` on the success arm (crypto/x509 `parseNameConstraintsExtension`'s `getValues := func(subtrees) (dnsNames []string, ips []*net.IPNet, emails, uriDomains []string, err error)`). A C# tuple literal with any untyped element has no natural type, so no arm fixes the lambda's return type and delegate-type inference fails (CS8917). The lambda states its tuple return type explicitly, and each `nil` then takes its target element type:
```csharp
var getValues = (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains, error err) (cryptobyte.String subtrees) => { … };
```
NAMED results are now included (they were previously excluded): the trigger — a multi-result literal with a return arm but NO fully-typed arm — is identical whether the results are named or not. A bare `return` (which returns the named results) never matches the result arity, so it neither marks has-return nor a false fully-typed arm; a named literal that DOES have a fully-typed explicit arm keeps inferred typing (no return-type prefix, no churn). Guarded by `NamedResultLambdaInfer` (a five-result named-result closure whose error arms return `nil,nil,err` and success arm `e,o,nil`).

## String-returning literals in assignment position state their return type
A literal with a single Go `string` result can mix return arms of DIFFERENT C# types even though every arm is a Go string: a bare string literal is a `"…"u8` `ReadOnlySpan<byte>`, a literal+variable concat binds golib's `operator +(@string, @string)` (so it is `@string` regardless of u8 suppression), and a call into a hand-written stub can return C# `string` (the baseline `fmt.Sprintf` does). `@string` and `string` convert implicitly in BOTH directions, so a lambda mixing those arms has no unique best common type and its delegate type is not inferable — CS8917 on `pick := func(v any) string {…}` whose `case string:` arm returns `"string:" + t` alongside `fmt.Sprintf` arms. In assignment position (`var pick = …`, where C# must infer the delegate type), the lambda states its return type explicitly and each arm then converts to `@string` in place:
```csharp
var pick = @string (any v) => {
```
Argument/return/composite-element literals are target-typed by their receiving delegate type (no inference to fail — and an explicit return type could only add an identity-match constraint against stub delegate types), so they keep the plain form; the Go `var` declaration form emits an explicit delegate type (`Func<@string, bool, @string> pad = …`) and is likewise immune. Gated to the basic string kind — a named string type would need its own conversions. Guarded by `FuncLitStringConcatReturn` (`:=` literals mixing concat, u8-literal, and stub-`Sprintf` arms — including a type-switch body and a right-side literal concat — plus the `var` form; runtime-verified).

The TUPLE-ELEMENT sibling: in a MULTI-result literal, a bare string literal element is worse than typeless — it is *wrongly* typed. Inside a tuple the literal emits as a bare C# `string` (u8 spans cannot be tuple elements), so an arm with no `nil` and no string variable — internal/fuzz `fuzzOnce`'s `return dur, coverageSnapshot, ""` (`func(entry CorpusEntry) (dur time.Duration, cov []byte, errMsg string)`) — counted as "fully typed" in the multi-result scan above and suppressed the explicit tuple return type, letting inference *succeed with the wrong element type*: the destructured `errMsg` was C# `string`, which has no `!=` against a `"…"u8` span (CS0019 rather than CS8917). A basic-string constant literal element whose declared result element is Go `string` now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var fuzzOnce = (time.Duration dur, slice<byte> cov, @string errMsg) (CorpusEntry entry) => { … };
```
and each `""` converts to `@string` in place via target typing. Same assignment-position gate; a literal whose string elements are all variables keeps inferred typing (the full-stdlib A/B footprint was exactly internal/fuzz worker.cs plus two latent-identity repairs, internal/coverage/decodecounter `sget` and net ipsock.go `addrErr`, both re-proven green). Guarded by the `FuncLitStringConcatReturn` extensions `fuzzish` (named results, `!= ""` on every destructured element) and `sget` (unnamed `(string, error)`); the pre-fix converter fails them with exactly CS0019 ×4.

The NUMERIC sibling: an untyped numeric constant literal element against a differently-SIZED declared result element is wrongly typed the same way. The literal emits bare, so the arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT literal C# `double` — where the Go result is e.g. `int64`: net/http ServeContent's `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` had no `nil`/string-literal element on its error arms, counted "fully typed", and inferred `Func<(int, error errSeeker)>` — rejected at the `serveContent(…, sizeFunc, …)` call because delegate types are invariant (CS1662/CS0029/CS1503, and the leaked `errSeeker` element name rides the inferred tuple). Such an element now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var sizeFunc = (int64, error) () => { …; return (0, errSeeker); };
```
A declared element the literal's natural type already matches (`int32` for an INT literal, `float64` for a FLOAT literal) infers correctly and stays inferred, and Go `int` (C# `nint`) is deliberately exempt — `return 0, err` against `(int, error)` results is pervasive and green today (the element converts implicitly at every use site), so marking it would churn stdlib-wide for no observed defect, the same reasoning that keeps `lambdaConstReturnCastType` away from signed single results. A SUB-negated literal (`return -1, …`) is unwrapped and marked the same way. (Full-stdlib A/B footprint: net/http fs.cs `sizeFunc` — the target — plus three latent same-shape repairs, internal/coverage/decodecounter `rdu32` ×3 and net/http h2_bundle `allocatePromisedID` (both `(uint32, error)`) and internal/zstd `fetchHuff` (`(uint16, error)`). Guarded by the `FuncLitNumericTupleReturn` behavioral test — the sizeFunc shape and a float64 shape both PASSED to typed function parameters, the `-1` arm, and the int/float64-identical controls that must keep inferred typing; the pre-fix converter fails it with exactly the fs.cs trio CS1662/CS0029/CS1503 ×2.)

That Go-`int` exemption is **narrowed, not absolute**: it holds only while every numeric arm at the declared-`int` position is a literal. When such a position carries BOTH a bare-`0` arm (naturally C# `int`) AND a **non-literal** Go-`int` arm (`i + 1` → C# `nint`) — and the other tuple slots on the non-literal arms are typeless (`default!`), leaving a single naturally-typed literal arm to drive inference — C# infers the delegate's first element as `int`, so the `nint` arm then fails to convert (CS0029/CS1662) and the assignment-inferred delegate is rejected at the invariant use site (CS0407). This is bufio `ExampleScanner_*`'s `onComma := func(…) (advance int, token []byte, err error) { …; return i+1, data[:i], nil; …; return 0, data, ErrFinalToken; }`. The multi-result scan therefore additionally records, per declared-`int` position, whether an int-LITERAL arm and a non-literal `nint`-expression arm both occur; when they do, the explicit `(nint, …)` return type is forced (`(nint advance, slice<byte> token, error err) (…) => …`) and each arm converts in place. The all-literal `return 0, err` shape has no non-literal arm, so it keeps its inferred emission untouched — the corpus is undisturbed (behavioral CNR byte-identical across all 451 projects). (Guarded by the `FuncLitNumericTupleReturn` extension — the `mixedIntArms`/`onComma` shape assigned and passed to a typed parameter, alongside the unchanged all-literal `intControl`; the pre-fix converter fails it with CS0029/CS1662/CS1503.)

## Numeric-returning literals with untyped-constant arms state their return type
The SINGLE-result numeric sibling of the string arm above (2026-07-17; the Phase-4 blocker-map row B7b — strings ×3, bytes ×2): a literal with a declared numeric result whose return arm references a **named untyped constant** — strings/bytes TestMap's `maxRune := func(rune) rune { return unicode.MaxRune }`. The const reference emits as a golib `Untyped*` wrapper reference (`Δunicode.MaxRune`, an `UntypedInt` static), and the wrapper's implicit conversions run in **both** directions with every numeric type. So in natural-inference position an all-const arm set infers the wrapper delegate — `var maxRune = (rune r) => Δunicode.MaxRune;` is `Func<rune, UntypedInt>`, rejected at the invariant-delegate `Map(maxRune, …)` call (CS1503) — and a mixed const/typed arm set (TestMap's `encode`, mixing `unicode.MaxRune`/`utf8.RuneSelf` with the `rune` parameter) has no unique best common type at all (CS8917). When any top-level return arm is a bare named untyped-const reference, the lambda states the declared return type explicitly and each arm converts in place:
```csharp
var maxFn = rune (rune _) => maxRune;
```
Same gates as the string arm: assignment position only (argument/return/composite-element literals are target-typed — no inference to fail), and a BASIC numeric result (a named numeric type would need a second user conversion the wrapper cannot chain — the `lambdaConstReturnCastType` named-type rationale).

**Literal-only arm sets are NOT automatically safe** (corrected 2026-08-15, the `crypto/tls` lane). This section used to end "literal-only arm sets stay inferred (no churn): an int literal is already C# `int`". They *are* concretely typed — but a BARE int literal is C# `int`, which is the declared type only when the Go result is `int32`. `crypto/tls` `TestCipherSuites`' comparator is the counterexample:
```go
isBetter := func(a, b uint16) int { …; return -1; …; return +1; …; return 0 }
…
if !slices.IsSortedFunc(prefOrder, isBetter) { … }
```
Every arm rendered as C# `int` against a Go `int` (C# `nint`) result, so the inferred delegate was `Func<ushort, ushort, int>`. Every *call* of the variable accepted it (`int` converts to `nint`); the delegate-VALUED use did not, delegate types being invariant — `CS1503: cannot convert from 'System.Func<ushort, ushort, int>' to 'System.Func<ushort, ushort, nint>'`, one of the package's four build errors. So a third arm joins: when **every** top-level single-result return arm is an INT literal and the declared basic result is an integer type other than `int32`, the declared type is stated:
```csharp
var isBetter = nint (uint16 a, uint16 b) => { …; return -1; …; return +1; …; return 0; };
```
**The gate is keyed to what the converter EMITS, not to the literal's Go-side natural type.** Two wider cuts were written and each measured to over-apply before this one:

| shape | emitted arm | C# infers | prefix? |
|:--|:--|:--|:--:|
| `func(…) int { return -1 }` | `-1` (bare) | `int` — wrong | **yes** |
| `func(bool) int64 { return 9 }` | `9` (bare) | `int` — wrong | **yes** |
| `func(bool) int32 { return 100 }` | `100` (bare) | `int32` — right | no |
| `func() float32 { return 0.5 }` | `0.5F` | `float` — right | no |
| `func() float64 { return 3 }` | `3D` | `double` — right | no |

Only a declared **integer** width leaves the literal bare: a floating result carries its width into the literal, and `int32`/`rune` *is* the bare literal's own C# type. Three further things bound the rule. Any arm the predicate cannot classify suppresses it — every non-INT-literal expression is *assumed* to carry the declared type — so mixed arm sets keep their present emission, as does a bare `return` against named results. A literal bound to a name that is only ever CALLED never reaches this code at all: `localFunctionDefine` has already emitted it as a C# local function carrying an explicit result type, so the arm can only fire where the delegate type is genuinely observable. And both literal SIGNS are stripped (`numericBasicLit` now unwraps unary `+` as well as unary `-`): Go writes an explicitly-positive literal precisely where it pairs with a negative one, which is the comparator shape this arm exists for, and treating `+1` as a non-literal blinded the predicate to half its own arm set — it did, on the first cut, where the fix silently emitted nothing at all. Not covered, and with no measured instance: a MIXED arm set whose declared type is NARROWER than the type C# picks (`func(…) uint16` with one `0` arm and one `ushort` arm, where `ushort` widens to `int`); that needs the natural C# type of an arbitrary expression, which the predicate deliberately does not attempt. (All five table rows sit side by side in the `FuncLitUntypedConstReturn` behavioral test, so the split stays pinned to the emission rather than to this table.)

A constant operator **expression** arm containing a named untyped constant counts the same as the bare reference (2026-07-17; the B7b gap — bytes TestMap's `invalidRune := func(r rune) rune { return utf8.MaxRune + 1 }` was the one remaining bytes build error): the operator result keeps the wrapper type, so the inferred delegate was `Func<int, UntypedInt>` against Map's `Func<int, int>` parameter (CS1503). The arm test (`returnArmKeepsUntypedWrapper`) walks paren/unary/binary trees for an untyped-named-const leaf, **except** when a constant fold (`overflowingConstLiteral` / `floatContextConstLiteral`) rewrites the whole arm to a plain literal — that emission is concretely typed and needs no prefix. All other gates unchanged. (Guarded by the `FuncLitUntypedConstReturn` behavioral test — the single-arm CS1503 shape, the mixed-arm CS8917 shape, an `int64` result with a beyond-int32 const arm, the const-expression arm (`return maxRune + 1`), plus literal-only and argument-position controls that must keep the plain form; output-compared vs Go.)

## A literal in GENERIC-RESULT inference position states its return type
The arms above all describe **natural-inference** position — a literal assigned to a `var`, where no
delegate target exists. A literal passed as an ARGUMENT is normally target-typed by its parameter and
needs no prefix, and the earlier rules say exactly that. There is one argument shape where that is
false: the callee is **generic** and the parameter's declared signature returns a **type parameter**
— `sync.OnceValue[T any](f func() T)`, `sync.OnceValues[T1, T2 any](f func() (T1, T2))`. There the
parameter type is not yet a concrete delegate; C# must infer the type argument **from the lambda's
own return expressions**, so the Go result type go/types already resolved is ignored. Two shapes then
break (both live in sync's `oncefunc_test.go`):

- **No arm yields a C# type at all** — a body terminated by `panic` (`func() any { calls++; panic("x") }`
  emits a statement lambda whose only exit is a `throw`), or one whose sole arm is an untyped `nil`
  (`return default!`). Inference has nothing to work from: CS0411 ×4.
- **An arm's NATURAL C# type differs from the declared Go result** — `func() int { return 42 }` is
  naturally `Func<int>`, where Go's `int` is `nint`: the wrong delegate is inferred and the
  declaration it initializes rejects it (CS0029).

A func literal in that position now states its declared result type, which fixes the type argument to
exactly Go's for every arm shape (so, unlike the natural-inference arms, no arm inspection is needed):

```go
var onceValue = sync.OnceValue(func() int { return 42 })
f := sync.OnceValue(func() any { calls++; panic("x") })
g := sync.OnceValues(func() (any, any) { buf[0] = 1; return nil, nil })
```
```csharp
internal static Func<nint> onceValue = Δsync.OnceValue(nint () => 42);
var f = Δsync.OnceValue(any () => { Ꮡcalls.Value++; throw panic("x"); });
var g = Δsync.OnceValues((any, any) () => { bufʗ3[0] = 1; return (default!, default!); });
```

The gate is the **result** position specifically. A type parameter appearing only in the func-typed
parameter's own PARAMETER list — the `slices.SortFunc(x, func(a, b E) int)` shape — is inferred from
the lambda's already-typed parameters and stays unprefixed; marking those would churn every such call
site in the corpus for no defect. Full-stdlib footprint: two files, both package-level
`sync.OnceValue` initializers (`internal/sysinfo`'s `CPUName`, `internal/syscall/windows`'s
`SupportUnixSocket`/`SupportTCPInitialRTONoSYNRetransmissions`). (Guarded by
`GenericResultLambdaInfer` — the `nint`/`any`/panic-terminated/two-result shapes, a concrete
multi-result instantiation, and the parameter-position negative control, output-compared vs Go.)

## A returned FUNC LITERAL is typeless in C#

Every arm above asks the same question — *does this return expression carry a natural C# type?* — and
each was written against the Go-side shapes seen so far: an untyped `nil`, a bare constant, an untyped
const wrapper. A Go **function literal** is a fourth shape, and it is typeless for a reason none of
those tests notice: it is fully typed in Go, and it renders as a bare C# lambda, which has no natural
type at all.

A function's own returns need no help — a declared C# result type target-types them, so a method whose
body sits in a frame simply returns the literal (`context`'s `afterFuncContext.AfterFunc`, which
returns a `Func<bool>` literal from a defer-holding body). The one site that does need help is **a
literal returned from inside another literal.** `lambdaConstReturnCastType` already casts a bare
integer literal returned inside a lambda for exactly this reason (CS8917). Its sibling
`lambdaFuncLitReturnCastType` names the declared result type of a returned func literal, under the same
gates — inside a lambda conversion only (a named function's returned literal is target-typed by its
declared C# return type), and only when the declared result is a NAMED func type, whose emitted
delegate name is what a cast needs:

```go
mergeCancel := func(ctx, cancelCtx Context) (Context, CancelFunc) {
    …
    return ctx, func() { stop(); cancel(Canceled) }
}
```
```csharp
(context.Context, Action) mergeCancel(context.Context ctx, context.Context cancelCtx) {
    …
    return (ctx, (Action)(() => {
        stopʗ1();
        cancelʗ3(context.Canceled);
    }));
}
```

(`mergeCancel` is itself only ever called, so it takes the local-function emission above — still a
lambda conversion, which is what the gate tests.) Without the cast the tuple has no natural type, so
neither does the enclosing conversion — CS8917 on the declaration and CS8130 at every deconstruction
of its result. Naming the type is also the more faithful rendering: Go's declared result there *is*
`CancelFunc`, a methodless func type, which renders inline as its base delegate `Action`. An UNNAMED `func() bool` result would need the synthesized
`Func<…>`/`Action` spelling and has no corpus site today, so it is deliberately left.

## Publicization decides WHAT a type's modifier is; the test-bridge arm only decides WHERE

`visitTypeSpec` writes a `[GoType]` declaration's access modifier from one of two sources, and they
answer different questions. `packagePublicizedTypes` answers *what* the modifier must be — an
unexported type reached by an exported field, var, or callable signature has to be `public` or C#
rejects the referrer (CS0050/CS0051/CS0052). `testInlineTypeAccess` answers *where* it is written: a
white-box bridge type carries its modifier inline rather than through `package_info.cs`'s
`<TypeAccessibility>` section, because its metadata anchor can be a different test class.

Asking the inline arm FIRST made it answer both — from the name alone — so a publicized bridge type
stayed `internal`. `context`'s internal test file declares `type testingT interface{…}` and the
exported `func XTestParentFinishesChild(t testingT)` that `x_test.go` calls; the publicization pre-pass
records `testingT` (it runs over the test-augmented package, so the exported `*types.Func` arm fires),
but the emission ignored it: `internal partial interface testingT` under a `public` method, CS0051 ×4.
Publicization now outranks, and the inline arm supplies the DEFAULT:

```csharp
[GoType] public partial interface testingT {   // was: internal
```

This is why `signatureReferencesUnexportedProductionType` (which downgrades an exported *test-file*
function whose signature names an unexported PRODUCTION type) is correctly restricted to production
types: a test-declared type is meant to be handled by publicization, and now is.

## A white-box PRODUCTION↔PRODUCTION pointer pair is already implemented — do not record it again

Under the `whitebox-reference` test model the internal bridge is the SAME Go package as production, so
a `*prodT → prodIface` cast inside an internal `_test.go` reads as local and records its own
`[assembly: GoImplement<T, Iface>(Pointer = true)]`. Production, though, is a **referenced assembly**
that already generated that adapter from its own record — and `InternalsVisibleTo <assembly>.tests`
makes even an unexported adapter class reachable. The duplicate record makes go2cs-gen emit a SECOND
adapter under a test anchor, and that copy resolves its forwarding members in the TEST class's scope:
`context`'s `contains(pc.children, cc)` converts `*cancelCtx`/`*timerCtx` to `canceler` for a map key,
and the duplicate bound `Done` to the unrelated `afterFuncContext.Done` extension (CS1929) while
emitting `cancel` with an **empty body** — a silently degraded override, not merely a build error.

The fix suppresses only the RECORD, which is what makes it small: `resolveAdapterNameMarkers` resolves
a pair that reached no record to the unqualified name it would have had, and that name is production's
own adapter, so the cast site repoints with no other change.

```csharp
!contains((~pc).children, new global::go.context_package.cancelCtxжcanceler(cc))   // production's, not a copy
```

It is gated on production ACTUALLY carrying the pair — its `package_info.cs` is loaded by
`convertTestVariant` into `importedPointerImplements` — never assumed: a pair only the test converts
still needs its local record. Reaching that set also required `canonicalRecordIfaceName` to strip a
leading `global::`, which names no package and never appears in a parsed record; the deliberate
non-collapse it documents is untouched, since a genuinely foreign pair still keys as
`net.http_package.ΔHandler` against the record's `http_package.ΔHandler`. This is the POINTER twin of
the value arm's `whiteboxProductionTarget` carve-out — note the pointer target arrives as a
`*types.Pointer`, so the shared `whiteboxProductionTarget` flag (computed from the unwrapped VALUE
form) is structurally false there and the check must unwrap and ask directly.

## A GoImplement record is gated on the method set actually satisfying the interface
Every `[assembly: GoImplement<T, Iface>]` record makes the `ImplementGenerator` emit implementation glue whose members forward to T's like-named methods — so a record whose Go method set does NOT satisfy the interface generates a forwarder to a method that does not exist. The corpus case: net/http's `err = http2GoAwayError{LastStreamID: …, ErrCode: cc.goAway.ErrCode, …}` — the keyed composite's sparse-array `ident` context leaks the `error`-typed LHS onto each FIELD value, and the `ErrCode` field's value recorded `GoImplement<http2ErrCode, error>` even though `http2ErrCode` has only `String()`/`stringToken()` (its generated `Error() => this.Error()` was CS1929). `convertToInterfaceType` now folds a `types.Implements` check over the recorded form's method set (T for a value record, `*T` for a `ж<T>` record) into `recordableBase`, which gates both the record and the matching adapter-wrapping emissions. A conversion the Go checker admitted always passes the check, so the gate can only drop pairs a caller composed from mismatched types; a type-param-carrying target skips the check (`types.Implements` is undefined for uninstantiated generics, and the open-generic conversion emission must stay). The full-stdlib A/B for this change is exactly one removed line — the false `http2ErrCode` record. (Guarded by the NEGATIVE `KeyedLiteralIfaceAssign` behavioral test: a keyed literal assigned to an `error` variable whose field-value type has `String()` but no `Error()` — a reintroduced record fails the compile phase.)

## Promoted methods through a FOREIGN pointer embed forward via the embedded box
A struct embedding a FOREIGN pointer (`net/http`'s `http2timeTimer struct { *time.Timer }`, `net/http/internal`'s `FlushAfterChunkWriter struct { *bufio.Writer }`, `bufio.ReadWriter { *Reader; *Writer }`) promotes the embed's pointer-receiver methods, but those land as ж-extensions visible to the consuming assembly only through METADATA — direct-ж primaries (`Reset(this ж<Timer> …)`) and the public `RecvGenerator` twins of `[GoRecv]` methods (`Write(this ж<Writer> …)`). The `ImplementGenerator`'s syntax-tree hop scan saw none of them, so the value-form partial deref'd to the value (`this.Timer.Value.Reset(d)` — CS1929, the extension receiver strands) and the FOREIGN-struct pointer adapter fell back to the struct itself (`m_box.Value.Write(p)` — CS1061, `ReadWriter` declares nothing). Both arms now probe the embed's type SYMBOL: the single-hop paths union the foreign element's metadata box methods into the hop's box-method set (`this.Timer.Reset(d)`; `m_box.Value.Writer` in the local pointer arm), and the foreign-struct pointer arm routes each member still on the plain fallback through the UNIQUE pointer embed whose metadata box methods declare it (`m_box.Value.Writer.Write(p)`; Go's depth-one promotion ambiguity rules make the unique-embed requirement faithful). An embed package class outside extension-lookup reach (not the emitting namespace, the shared root `go`, or an enclosing segment) forwards through its package-class static with the box as the receiver argument, mirroring the foreign-extension arm. (Guarded by the `ForeignPtrEmbedIfaceLib`/`ForeignPtrEmbedIfaceUser` pair — a local struct embedding a foreign pointer adapted by value AND by pointer, plus a foreign two-pointer-embed struct adapted by pointer, output-compared vs Go.)

## Promoted methods through embedded INTERFACE fields route per-member to the declaring field
A struct whose embeds are INTERFACE fields (httputil's `dumpConn struct { io.Writer; io.Reader }` adapted to `net.Conn`) satisfies interface members through the fields' method sets. The pointer adapter's embedded-interface-field arm was gated to a SINGLE field, so a multi-field struct got no forwarding at all (`m_box.Read(…)` — CS1061). The arm now resolves per member: each still-unbound interface member forwards through the UNIQUE embedded field whose interface declares it (`m_box.Value.Reader.Read(p)` / `m_box.Value.Writer.Write(p)`); a member declared by several fields is left unbound (Go's promotion ambiguity rules reject it unless the struct overrides, and a struct override is already resolved earlier). The single-field behavior is unchanged (zip's `nopCloser`, slogtest's Δ-renamed `Handler` field). (Guarded by the `IfaceFieldEmbedAdapter` behavioral test — a two-interface-field struct adapted by pointer to a third interface needing members from both fields plus one declared on the struct, output-compared vs Go.)

## A pointer parameter used only through its box gets no deref VALUE alias
Every named pointer parameter is emitted as its box `ж<T> Ꮡp` with an entry-time value alias
`ref var p = ref Ꮡp.Value` (so a value use `p.field` reads through `p`). But a parameter that the body
touches **only** through its box — `unsafe.Pointer(p)` → `new @unsafe.Pointer(Ꮡp)`, `p == nil` →
`Ꮡp == nil`, or passing `p` on as a `*T` argument → `Ꮡp` — never references that value alias, so the
alias is a dead local that nonetheless **dereferences the box** at function entry. When the argument is
nil this NREs even though Go never touches the pointee: `syscall.writeFile(…, overlapped *Overlapped)`,
called with a nil `overlapped` and using it only as `unsafe.Pointer(overlapped)`, crashed at
`ref var overlapped = ref Ꮡoverlapped.Value` — the failure of any converted `fmt.Println` whose stdout is
a pipe (`syscall.Write` → `writeFile`). The converter already skips the alias for an **unnamed/blank**
pointer param (never referenced); this extends it to a **named** param whose value alias is likewise
unreferenced. After the body is converted, `bodyReferencesIdentAsValue` scans the emitted body text for
the value-alias name as a **standalone identifier** — the address marker `Ꮡ` is a Unicode *letter*, so
the box form `Ꮡp` is excluded by the preceding-letter boundary, while a genuine value use always emits
the bare name and matches. The scan only ever ADDS spurious matches (a field selector `x.p`, a string, a
comment), which keep the alias, so a live alias is never dropped (no CS0103) — it removes strictly unused
locals. Because a param with no alias leaves `implicitPointers` empty (which otherwise triggers the
signature rebuild that renames pointer params to the box `Ꮡ<name>` the body references), a
`skippedDeadPointerAlias` flag forces that rebuild. Full-stdlib/behavioral blast radius is broad (72
behavioral files — many functions forward a pointer param onward as a pointer), all pure alias removals.
This shares its root with the receiver case below (an eager deref alias NREs on a nil pointer); the
converse **live**-alias nil case — invoking a pointer method/function that *does* dereference a nil
receiver/param — still NREs at the alias, awaiting the nil-safe `DerefOrNil` extension. (Guarded by the
`DeadPointerParamAlias` behavioral test — a pointer param used only in `p == nil`, one forwarded as a
pointer, and one dereferenced (alias kept), each called with nil and non-nil, output-compared vs Go; the
nil calls NRE'd before the fix.)

⚠ **Keeping a dead alias is not free, so the scan excludes the one spurious match that is systematic: a
C# NAMED-ARGUMENT LABEL** (`isNamedArgumentLabel`). The converter renders a Go composite literal's field
keys as named arguments to the fieldwise constructor go2cs-gen generates, and Go's own idiom is to name
the field after the value it is initialized from — so a pointer parameter whose name matches one of the
literal's FIELDS matched the whole-word scan on the label alone. internal/concurrent's
`newIndirectNode(parent *indirect) { return &indirect{node: …, parent: parent} }` never dereferences
`parent` (the C# passes the box, `parent: Ꮡparent`), yet `parent:` kept the alias alive;
`ref var parent = ref Ꮡparent.Value` then deref'd the box at ENTRY, and the ROOT node — created as
`newIndirectNode(nil)`, a legitimately nil parent — threw inside `NewHashTrieMap`, taking down `unique`'s
package initializer, `net/netip`'s, and every dependent (found through encoding/gob's `TestNetIP`). A
label is the identifier immediately followed by ONE `:` — never `::` — whose preceding non-space
character opens or continues an argument list; every other colon form the converter emits is excluded by
that (a `case X:` arm and a goto label are preceded by a keyword or a statement boundary, an interpolated
format specifier `{x:F2}` by a brace, and a conditional's `cond ? a : b` spaces its colon). A parameter
genuinely used elsewhere still matches there, so this discards the label occurrence and never the scan.
Whole-stdlib A/B (two seeded reconverts on the two binaries, 302 packages): **39 files**, every diff hunk
the removal of one dead `ref var` line and nothing else, and the reconverted corpus builds 304/304 with
**0 errors** — the removed aliases are all genuinely unreferenced. (Guarded by the same
`NilPointerParamUnsafePointer` behavioral test, extended with the composite-literal shape and a
dereferencing positive control whose alias must survive; the exclusion's own boundaries — `::`, a
`case X:` arm, a spaced conditional colon, an interpolated format specifier, and a label that does not
mask a real use elsewhere — are pinned by the `TestBodyReferencesIdentAsValueIgnoresNamedArgumentLabels`
converter unit test, since a behavioral program cannot put most of them in that position.)

## A pointer RECEIVER compared to `nil` compares its box, not its deref'd value
A method with a pointer receiver — `func (f *File) checkValid() error { if f == nil { … } }` — is emitted
as `checkValid(this ж<File> Ꮡf, …)` with the body alias `ref var f = ref Ꮡf.Value`. Go's `f == nil` is a
**pointer** comparison (nil-pointer check — and Go legitimately calls methods on nil-pointer receivers), so
it must compare the box `Ꮡf == nil`, not the deref'd struct value `f`. Emitted in value form, `f == nil`
binds the generated `File.operator==(File, NilType)` — which compares against `default(File)` and, for a
**promoted-embed** struct (`File` embeds `*file`), dereferences that zero value's null embed box → a
`NullReferenceException` (the first crash of a converted `fmt.Println`, via `os.Stdout.Write` →
`checkValid`); even for a plain struct it is the wrong answer (`&box{}`, a non-nil pointer to a zero struct,
compares *equal* to `default(box)` → `true` where Go gives `false`). The converter already forced the box
form for a deref'd pointer **parameter** (`isDerefdPointerParamIdent`, driving the `==`/`!=` operand
context in `convBinaryExpr`); the receiver is deliberately not a "parameter" in that model
(`paramNames` excludes the receiver), so it needs its own recognizer, `isDerefdPointerReceiverIdent`
(object-identity match via `isPointerReceiver`/`identResolvesToReceiver`, so a local shadowing the receiver
name keeps its own render). It is scoped to the comparison operands only — unlike a pointer parameter the
receiver is **not** folded into `nilSafePtrParamNames`, so its deref-alias form is unchanged (only the
receiver's `==`/`!=` operand switches to the box); `convIdent` renders `Ꮡf` for the receiver through its
existing direct-ж-receiver arm. Like a deref'd pointer parameter, the box form applies to **every**
`==`/`!=` comparison, not just against nil: a pointer receiver can only be `==`-compared to another
pointer or to nil, so the box is always the correct (pointer-identity) operand — `func (b *Reader)
Reset(r io.Reader) { if b == r … }` (bufio) becomes `AreEqual(Ꮡb, r)`, comparing the pointer to the
interface's held pointer, where the pre-fix `AreEqual(b, r)` compared a *struct value* to the interface
(never equal — a latent recursion-guard bug the fix also closes). The full-stdlib A/B is small and
mechanical — 161 receiver operands across 77 files gain a `Ꮡ` box (155 nil-checks, 6 pointer-identity),
every changed line adding only the box.
(Guarded by the `PointerReceiverNilCompare`
behavioral test — a plain-struct `*box` where `&box{}` must compare `!= nil`, `== nil`/`!= nil` receiver
methods, and a promoted-embed `*embedder` whose pre-fix value comparison NRE'd, output-compared vs Go; and
by the re-baselined `RingPointerMethods` whose `r != nil` receiver walk now renders `Ꮡr != nil`.)

## A pointer RECEIVER's deref alias is nil-DEFERRING — the panic moves to the body, it does not vanish
Go legitimately **calls** methods through a nil pointer: the method RUNS, and the nil-pointer panic
happens only where the body actually dereferences the pointee. The emitted entry alias
`ref var b = ref Ꮡb.Value;` instead dereferenced at ENTRY, so every nil-tolerant method panicked
before it began — whichever way its guard is spelled:

| Guard shape | Go | pre-fix C# |
|:--|:--|:--|
| INLINE — `func (b *Buffer) String() string { if b == nil { return "<nil>" } … }` (bytes `TestNil`) | returns `"<nil>"` | entry panic |
| DELEGATED — `func (f *File) Chdir() error { if err := f.checkValid("chdir"); … }`, where `checkValid` is what asks `f == nil` (os `TestNilFileMethods`, all fifteen `*File` methods) | returns `ErrInvalid` | entry panic |
| NONE, but a side effect first — `fmt.Println(…)` then `f.name` | prints, THEN panics | entry panic, nothing printed |

The first row was closed in 2026-07 by folding a **nil-COMPARED** receiver into `nilSafePtrParamNames`
so its alias took the nil-SAFE `DerefOrNil()`. That could not close the other two, and widening it to
every receiver was the wrong instrument: `DerefOrNil()` hands back a shared throwaway `default(T)`
slot, so an unguarded deref reads a **silent zero where Go panics** — acceptable only where the
converted guard provably excludes the read (the nil-terminated pointer walk it exists for), never as a
corpus-wide default. That widening was measured and reverted.

The instrument that works is a **nil-DEFERRING** accessor, `DerefOrNull()`, which binds
`Unsafe.NullRef<T>()` for a nil box. A null ref is legal to HOLD and to pass on as `ref T`; it faults
on USE. So the panic is not discarded and not moved earlier — it lands exactly where Go's does:

```csharp
public static (nint n, error err) Read(this ж<File> Ꮡf, slice<byte> b) {
    ref var f = ref Ꮡf.DerefOrNull();          // binds; never throws
    {
        var errΔ1 = Ꮡf.checkValid(readˢ);      // through the BOX — Go's guard, reached
        if (errΔ1 != default!) { return (0, errΔ1); }
    }
    (n, var e) = Ꮡf.read(b);
    return (n, f.wrapErr(readˢ, e));           // the first real deref — panics here, as Go does
}
```

The first field read, field write, or whole-struct copy through that ref raises
`NullReferenceException`, which `RuntimeErrorPanic.TryAsPanic` already maps to Go's own
`runtime error: invalid memory address or nil pointer dereference` — recoverable and printed
verbatim. Measured, not assumed: a synthetic struct whose field sits 200 KB past the null page still
faults as a clean `NullReferenceException` (the JIT emits an explicit check rather than relying on the
64 KB guard page), and a converted Go struct cannot reach even that offset — Go's inline `[N]T` becomes
a golib `array<T>`, an 8-byte managed reference — so there is no null-page cliff to fall off.

**Where it is emitted.** The entry alias exists in **two** places and both take the accessor, or the
fix is half-done:
- the converter's own preamble for a direct-ж receiver (`visitFuncDecl`, unconditionally for
  `i == 0 && Recv != nil && directBoxReceiver`; plus a repointed receiver's re-alias in
  `visitAssignStmt`, so the two halves of one alias cannot disagree);
- **go2cs-gen's `ReceiverMethodTemplate`**, the bridge that reaches a `ref T`-receiver method through a
  box. Left eager, that bridge panicked one call frame *earlier* than Go — the `ValAnnounce` arm of the
  guard below caught exactly this and was the reason the generator half was found at all.

Because the accessor is unconditional there is no predicate to get wrong, and
`isComparedDirectBoxReceiverIdent` — the 2026-07 receiver-specific arm of `collectNilSafePtrParams` —
is subsumed and deleted. A non-nil receiver is unaffected: `DerefOrNull()` routes it to `ValueSlot`,
the identical real slot, which additionally subsumes the `isInherentlyHeapAllocatedType` →
`.ValueSlot` receiver arm above (same slot, and now the genuinely-nil case is handled too rather than
silently read). The scan that remained after this fix covered pointer PARAMETERS only — and the
section below is why it does not exist at all any more.

(Guarded by the `NilReceiverMethods` behavioral test, output-compared vs `go run`: a delegated
`checkValid`-style guard that must return the error, an unconditional deref that must panic with Go's
message, a side effect that must be observed BEFORE that panic, and a non-nil receiver read through
both emission shapes — each panic case run through both the direct-ж preamble and the generated
`ref`-receiver bridge. `PointerReceiverNilCompare` and bytes' `TestNil` continue to cover the inline
guard.)

## A pointer PARAMETER is nil-deferring for exactly the reason a receiver is
Go's nil rule does not distinguish the two. Passing a nil `*T` to a function is as legal as calling a
method through one: the body RUNS, and the nil-pointer panic happens only where the body actually
dereferences the pointee. So the entry alias for a pointer PARAMETER takes the same
`DerefOrNull()` the receiver does, unconditionally, with no admitting analysis — because the accessor
is faithful whether or not the body guards.

Getting here took three arms of a body ANALYSIS, each one a real fix for a real crash and each one
provably incomplete, which is the argument for retiring all of them at once:

| Arm | The shape it admitted | Why it could not be the answer |
|:--|:--|:--|
| nil-COMPARED param (`collectNilSafePtrParams`) | `for p != nil { …; p = p.next }`; `if p == nil { … }` | a body that never spells the comparison is not a body that cannot take nil |
| nil-ARGUMENT call site (`collectNilArgPtrParams`) | `digits(ch, 10, nil)` — text/scanner's optional out-param | SAME-package call sites only; the converter sees one package at a time, so a cross-package nil argument stayed broken |
| RE-POINTED before first read (`reassignedBeforeDerefParamName`) | `l = l.get()` — Go's nil-receiver NORMALIZATION idiom (`time.Location.lookup`, whose entry fault cost seven of `time`'s verdicts) | narrow by construction: first top-level statement only, and every use inside it non-dereferencing |

The shape none of them could reach is the one that broke `net`. `internal/concurrent`'s
`newIndirectNode(parent *indirect[K, V])` STORES its parameter and never dereferences it, and
`net`'s package initializer reaches it through `netip` → `unique` as `newIndirectNode(nil)` — a
perfectly ordinary nil argument, from another package, with no comparison anywhere in the callee. The
eager `ref var parent = ref Ꮡparent.Value;` faulted at entry, inside a static constructor, so every
`net` test failed before the first one ran (`net` compiled with 0 errors and scored **0/138**).

Nothing analyzable distinguishes that callee from one that derefs immediately. What distinguishes them
is what the BODY does, at the point it does it — which is precisely what the nil-deferring accessor
defers to:

```go
// internal/concurrent/hashtriemap.go
root: newIndirectNode[K, V](nil),        // NewHashTrieMap's initializer, reached from net's .cctor
...
func newIndirectNode[K, V comparable](parent *indirect[K, V]) *indirect[K, V] {
	return &indirect[K, V]{node: node[K, V]{isEntry: false}, parent: parent}
}
```
```csharp
internal static ж<Δindirect<K, V>> newIndirectNode<K, V>(ж<Δindirect<K, V>> Ꮡparent)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var parent = ref Ꮡparent.DerefOrNull();   // was: ref Ꮡparent.Value — NRE in net's .cctor

    return Ꮡ(new Δindirect<K, V>(node: new node<K, V>(isEntry: false), parent: Ꮡparent));
}
```

**What the accessor costs.** For a NON-nil pointer, nothing observable: `DerefOrNull()` routes to
`ValueSlot`, the identical real slot, so every read, every write-through and every re-alias behaves
exactly as `.Value` did. For a nil pointer it binds `Unsafe.NullRef<T>`, which is legal to hold and
faults on USE — so an unguarded deref still raises Go's
`runtime error: invalid memory address or nil pointer dereference` (via
`RuntimeErrorPanic.TryAsPanic`), recoverable, at the body's own deref point, AFTER any side effect the
body performed first. The retired nil-SAFE accessor could not say that: its shared `default(T)` slot
read a silent zero where Go panics, which is why it was only ever admissible under a proof.

**One shape, everywhere a pointer is aliased.** The unification also takes the pointer-reassignment
re-alias in `visitAssignStmt` (single-assign, tuple-deconstruction, and the `p = nil` repoint alike):
a repoint is not a dereference in Go, so re-aliasing must not fault, and the two halves of one alias
must not disagree about whether nil is legal to hold. And it takes the `isInherentlyHeapAllocatedType`
→ `.ValueSlot` arm at THIS site, which is the one place the unification goes beyond swapping an
accessor. `.ValueSlot` remains type-selected everywhere it belongs — a box-of-pointer LOCAL, a
named-result box, `heap(out …)`, the reflection bridge's field paths — but at an entry alias it was
selected AFTER the nil-safe analysis, i.e. the old code already ranked nil-ability above pointee-kind.
Keeping it ranked first under an unconditional nil policy would have inverted that order and handed 9
corpus aliases across 8 files (`internal/weak`'s `ptr`, runtime `mbitmap`'s `header`, dwarf's
`fixups`, …) a NEW entry-time fault on exactly the nil arguments the previous converter tolerated —
measured, and the reason the arm is not selected here.

**Measured footprint** (seeded whole-stdlib A/B reconvert, base vs fixed, 2026-08-02): **457 files,
2,551 lines, 2,555 accessor sites — 100% a single shape**, the accessor token at a
`= ref <box>.<accessor>;` position, with zero unclassified lines and no file changing its line count:

| Transition | Sites |
|:--|--:|
| `.Value` → `.DerefOrNull()` | 2,079 |
| `.DerefOrNil()` → `.DerefOrNull()` | 413 |
| `.ValueSlot` → `.DerefOrNull()` | 59 |

Corpus-wide the entry/re-alias census moves from `Value` 2,849 / `ValueSlot` 81 / `DerefOrNil` 420 /
`DerefOrNull` 2,030 to `Value` 766 / `ValueSlot` 22 / `DerefOrNil` 0 / `DerefOrNull` 4,585 — the
remaining `.Value` are genuine USE sites, where Go does panic. (Seven `DerefOrNil()` sites survive in
committed `*_test.cs` under `container/ring`, `go/token`, `index/suffixarray` and `testing/quick`: a
`-stdlib` reconvert does not re-emit banked test sources, so they level at each package's next
`-tests` run.) The behavioral corpus re-baselines in the same one shape — 71 `.cs`, 138 lines, 69
goldens, every added line an accessor swap. The converter loses 382 net lines: the accessor constant,
three analyses, their helpers, the package-wide pre-pass and the visitor state behind them.

(Guarded by the `NilPointerParamMethods` behavioral test, output-compared vs `go run`: a nil argument
through a DELEGATED guard that must return the error, a nil argument STORED and never dereferenced —
the `newIndirectNode` shape — an unconditional deref that must panic with Go's message, a side effect
that must be observed BEFORE that panic, a nil-terminated walk, a `*error` parameter that legally
HOLDS nil, the normalization idiom, and every non-nil argument unchanged. Neuter-proven: restoring the
eager param arm diverges from Go on 42 output lines. `GuardedNilPointerParamDeref`,
`PointerParamNilWalk`, `NilReceiverNormalization` and `PointerToInterfaceParamDeref` continue to cover
the shapes the retired arms were built for.)

## A receiver or parameter RE-POINTED before first use — the normalization idiom
Go's other nil idiom does not test the pointer at all: it *normalizes* it, by re-pointing through a
helper that does the testing. `time`'s `Location.lookup` is the canonical case — `Time{}` carries a nil
`*Location` meaning UTC, and `get` maps nil to `&utcLoc`:

```go
func (l *Location) lookup(sec int64) (name string, offset int, start, end int64, isDST bool) {
	l = l.get()
	if len(l.zone) == 0 { … }
```

No comparison predicate can see this — `lookup` never writes `l == nil` — so under the eager alias the
receiver **faulted before its first statement**, where Go returns UTC. That one site cost seven of
`time`'s test verdicts (`TestDefaultLoc`, `TestSecondsToUTC`, `TestNanosecondsToUTC`, `TestParse`,
`TestTimeGob`, `TestTimeIsDST`, `TestZoneBounds`), every one of them dying at entry.

It was closed first by a dedicated predicate (`reassignedBeforeDerefParamName`: the FIRST top-level
body statement that mentions the pointer must be an `=` that re-points it, with every use of it inside
that statement leaving the pointee untouched — a pointer-receiver method call like `l.get()`, a nil
comparison, or the bare pointer as an argument; a field selection, a value-receiver method, or a `*p`
disqualified it). That predicate is gone: the unconditional nil-deferring alias covers the idiom
without needing to recognize it, and covers the variants it could not admit (a normalization on the
SECOND statement, or one nested in an `if`). Both halves now read the same way:

```csharp
internal static (@string name, nint offset, int64 start, int64 end, bool isDST) lookup(this ж<ΔLocation> Ꮡl, int64 sec) {
    …
    ref var l = ref Ꮡl.DerefOrNull();          // binds for Time{}'s nil loc; no fault at entry
    Ꮡl = Ꮡl.get(); l = ref Ꮡl.DerefOrNull();   // the repoint is not a deref either
```

One subtlety the predicate era got right and this must keep: a normalizer that CAN return nil
(`GenericFuncCall`'s `renew` — `p = escape(p); *p += 10`, where `escape` is identity) must still panic
at the `*p`, and it does — the re-alias binds a null ref and the dereference through it faults with
Go's message, exactly where Go's does. (Guarded by the `NilReceiverNormalization` behavioral test —
normalize-then-read, normalize-then-*write* through the re-aliased receiver with a read-back proving
the real slot was addressed, the same shape on a nil-legal pointer *parameter*, and a control that
reads BEFORE normalizing and still panics through a nil receiver exactly as Go does.)
## A reinterpreted raw address ALIASES native memory instead of boxing a copy
`(ж<T>)(uintptr)` is the reinterpret seam: it turns a raw address back into a pointer. It used to
box a **copy** of the pointed-at value —

```csharp
public static unsafe explicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value.Value);
```

— which silently discarded the address. That is fine only for an immediate single read. It makes
three things impossible: pointer arithmetic (there is no address left to advance), observing writes
that native code makes afterward, and — worst — handing the pointer **back** to the OS, which then
operates on the address of a *managed box field* instead of the native block.

`syscall.Environ` does all three. It walks the `GetEnvironmentStringsW` block and frees it:

```go
envp, e := GetEnvironmentStrings()
defer FreeEnvironmentStrings(envp)
for *envp != 0 { … end = unsafe.Add(end, size) … }
```

Converted, the walk scanned the GC heap and the deferred `FreeEnvironmentStringsW` asked Windows to
free GC memory — an outright `STATUS_HEAP_CORRUPTION` (0xC0000374) process kill. `os.Environ()`
alone reproduced it, so nothing that reads the environment could run.

`ж<T>` now has a fourth reference kind alongside the standard value, struct-field and array-element
refs: a box that **aliases a native address**. `Value`/`ValueSlot` read that memory through
`Unsafe.AsRef`, `IsNull` is address-based (address 0 is the nil pointer, matching Go's
`(*T)(unsafe.Pointer(uintptr(0))) == nil`), and the `uintptr`/`void*` operators round-trip the
address **exactly** — which is what Go's `uintptr(unsafe.Pointer(p))` guarantees. `unsafe.Add`,
`unsafe.Slice` and `unsafe.String` honor the kind.

`unsafe.Add` also gained an `unsafe.Pointer` overload. Go's `unsafe.Add` is **byte** arithmetic and
its argument is always an `unsafe.Pointer`, but golib models `unsafe.Pointer` as `ж<uintptr>` whose
*value* is the address — so the generic `ж<T>` overload, which resolves a managed array-element
reference, found none and returned a **nil** pointer. Stepping through a native block dereferenced
address 0 on the very first step.

**Known limit:** `unsafe.Slice`/`unsafe.String` over a native address *snapshot* the memory into a
managed buffer rather than aliasing it the way Go does, so writes through the result do not reach the
native memory. That is sufficient for reading a block a syscall returned (the `Environ` shape) and is
where the seam still differs from Go.

## An address of MANAGED storage that outlives its statement must carry a PIN

The native-address box above is exactly right when the address IS native memory — there is nothing
behind it the collector could move. It is a **dangling pointer** when the address points into managed
storage, and the reinterpret fallback produces precisely that: where `(*U)(unsafe.Pointer(p))` cannot
alias `p`'s storage in the managed model, golib names it by address instead
(`PointerExtensions.Reinterpret` → `(ж<U>)(uintptr)box`), and the `uintptr` operator's

```csharp
fixed (void* ptr = &value.Value)
    return (uintptr)ptr;
```

pins for **that statement** and no longer. The derived pointer outlives it. Once a collection moves
the storage, reads through the pointer return whatever now occupies the old address — and **writes
land in whatever now owns it**. The second is the one that matters: it is not a wrong value, it is
silent heap corruption, and the crash surfaces later, somewhere unrelated. The corpus's clearest
instance of the shape is `os_windows_test.go`'s `createMountPoint`, whose four `uint16` field stores
go through a `[]byte` scratch buffer addressed as a reparse record:

```go
byteblob := make([]byte, buflen)
buf = (*windows.MountPointReparseBuffer)(unsafe.Pointer(&byteblob[0]))
buf.SubstituteNameOffset = target.substituteName.offset   // … and three more
```

```csharp
var byteblob = new slice<byte>(buflen);
buf = Ꮡ(byteblob, 0).Reinterpret<byte, windows.MountPointReparseBuffer>();
buf.Value.SubstituteNameOffset = target.substituteName.offset;
```

**The rule: the pin's lifetime is the DERIVED POINTER's, not the address-taking statement's.** The
fallback now asks the source box for a pinned address (`ж<T>.TryPinnedReinterpret`), and the derived
box OWNS that pin — a `PinnedBuffer.PinOnly` handle held in the box's `m_pin` field and released by
its finalizer when the box is collected. This is the same field and the same idiom as the fixed-array
syscall-buffer pin (`pinnedArrayData`, above); the two uses are disjoint, since a native box never
takes the lazy one.

Only an **array/slice-element** reference can be pinned, and that is not a shortcut — it is the only
reference kind whose storage is an object the runtime can be asked to hold still. A native alias has
nothing managed behind it; a nil box has no storage; and the storage of a standard heap box and of a
struct-field reference alike is a field of a `ж<T>`, which holds delegates and a nullable tuple and so
is never blittable — `GCHandle` refuses to pin it. Those kinds keep the pre-existing address route, so
the change is strictly additive: where a pin cannot be taken, behavior is what it was, never something
newly wrong. That is also what keeps `reflect`'s prefix-downcast idiom
(`(*structType)(unsafe.Pointer(t))` over a `*abi.Type`, structurally unrepresentable and deliberately
on the address route) working unchanged — a blanket "fail loudly" was never available.

The pin is **cross-checked before it is trusted**. It is taken on the backing store
`CanonicalElement` names, which proves something only if the referent really lives inside that object,
so the address reached through the box's own value slot must be the same byte as the address of that
backing's element; a view whose `Source` is a detached copy fails the test and gets no pin.

Guarded by `tests/Behavioral/ReinterpretPinLifetime`, which is deterministic in both directions — it
writes through the derived pointer before and after enough allocation churn to move and recycle the
buffer, and reads back through both the derived pointer and the original slice. Pre-fix C# printed
`read: false true true` / `write: false false false false false false` on 5 of 5 runs where Go prints
all `true`; post-fix it matches Go on 8 of 8. Its sibling `ReinterpretPointerLifetime` guards the
other half of the same contract — the ALIASING route, for reinterprets the managed model *can*
represent.

**What this is NOT evidence of.** `os`'s test host was separately observed dying with an
`ExecutionEngineException` whose crash site moved between runs, and `createMountPoint` was the
standing suspect. A pre-fix control run of the whole `os` suite at `af5df9e16` (golib stashed back to
base, the rebuilt `golib.dll` verified to lack the fix) reproduced no such crash, and two complete
post-fix runs bracket its agreeing count from both sides, so that attribution is retracted — see the
`os` section of [`docs/phase4/BOARD-next-validation-candidates.md`](../phase4/BOARD-next-validation-candidates.md).
The pin defect is real and deterministic on its own evidence; it simply was not shown to be that
host-killer.

**Not fixed by this, and a different class:** a destination struct holding a managed reference where
Go has an inline array (`PathBuffer [1]uint16` → `array<uint16>`) still fabricates an object reference
out of whatever bytes sit at that offset when the field is read. Pinning makes those bytes the *real*
buffer's rather than recycled memory, but a fabricated reference is a CLR type-safety break either
way. That is the raw-metal-on-non-native-types fork (`os.readReparseLink`'s remedy is a hand-owned
decode; `os_windows_test.go`'s `createMountPoint` is test code that cannot be hand-owned).

## `unsafe.Slice` over MANAGED element storage ALIASES it

The snapshot above is the right answer for a native address and the wrong one for the far commoner
shape: `unsafe.Slice(&s[i], n)`, where the pointer addresses an element of a managed slice or array.
Go's result shares that storage, so writes through the rebuilt slice must land in the original
backing — and the snapshot silently swallowed every one of them. crypto/subtle is the case that
exposed it: `XORBytes` hands `xorBytes` bare pointers, which rebuilds its three slices and writes the
whole result through `dst`, so **`XORBytes` wrote nothing at all** (its test matrix compared `dst`
against its untouched `0xdd` fill).

`ж<T>` answers with the window when it has real managed element storage
(`TryGetElementWindow`): the referent is reduced through the same `CanonicalElement` mapping pointer
equality uses — so a pointer taken through a re-sliced view addresses the same absolute element Go's
would — and the result is a `slice<T>` over that backing with `len == cap == n`, exactly Go's shape.
A heap box, a struct-field ref, or a REINTERPRETING pointer (a `(*U)(unsafe.Pointer(&b[0]))` over a
differently-typed array) has no such storage and keeps the snapshot; a `T[]` view over another
element type does not exist in the managed model.

That last exclusion is why `crypto/subtle/xor_generic.cs` is **hand-owned**
(`[module: GoManualConversion]`). Its word-at-a-time loop reinterprets the byte slices as
`[]uintptr` —

```go
func words(x []byte) []uintptr {
	return unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), uintptr(len(x))/wordSize)
}
```

— which the converted form can only snapshot, so for every length that is a multiple of 8 the XOR
went to a detached buffer and `dst` stayed untouched, while other lengths landed only their trailing
`n % 8` bytes. The hand-owned file does the same reinterpret the managed way,
`MemoryMarshal.Cast<byte, ulong>` over the slices' own spans — a genuine aliasing view, so the word
writes land in place. It keeps Go's word-at-a-time behavior (and with it the performance contract
crypto/cipher's CTR and GCM modes depend on) and drops only Go's `supportsUnaligned`/`aligned` gate,
which exists for architectures whose unaligned word loads fault. (Validated by crypto/subtle's own
suite: 7/7, no disclosures, over the full 1..1024 × 8 × 8 × 8 alignment matrix.)


## A reinterpret of a MANAGED pointer aliases the box — it never round-trips through the address
The section above is about a pointer whose source genuinely *is* an address. The mirror case is
`(*U)(unsafe.Pointer(p))` where `p` is an ordinary Go pointer `*T` — the shape `reflect` uses to view
one struct as another (`toRType`: `(*rtype)(unsafe.Pointer(t))`). Both pointee types are managed, so
there is no native memory anywhere in the expression, yet the emission routed through the raw-address
seam anyway:

```csharp
return (ж<view>)(uintptr)(new @unsafe.Pointer(Ꮡh));   // was
return Ꮡh.Reinterpret<view>();                        // is
```

The old form is **not merely indirect, it is unsound**. golib's `implicit operator uintptr(ж<T>)`
ends in `fixed (void* ptr = &value.Value) return (uintptr)ptr;` — `fixed` pins only for the duration
of its own statement. The address escapes it, and `(ж<U>)(uintptr)` then builds a *native-backed* box
holding no reference to the source. The derived pointer therefore neither keeps its pointee alive nor
survives the collector moving it. Consumed immediately it happens to work; **retained**, it dangles,
and the read comes back silently wrong once another allocation reuses the address.

That is exactly what `reflect` does — `canonType` caches the reinterpreted `rtype` for process
lifetime — so after enough heap churn `TypeOf(x).Kind()` began reporting `Invalid` mid-process, which
inverted `fmt.Sprint`'s "space only between two non-strings" rule corpus-wide. See
[`docs/phase4/FINDING-managed-box-uintptr-lifetime.md`](../phase4/FINDING-managed-box-uintptr-lifetime.md).

The golib extension `Reinterpret<T, TDst>()` decides by **provenance first**:

| Source pointer | Result |
|---|---|
| A nil box, or a plain `null` reference (both are Go's nil pointer) | `ж<TDst>.NilBox` — Go's `(*U)(unsafe.Pointer((*T)(nil))) == nil` |
| Aliases a NATIVE address (`m_nativeAddr` — a Win32 API's returned block) | the same address; the interop contract above is untouched |
| Owns MANAGED storage, and the reinterpret is representable (below) | a box aliasing that storage, through ж's existing struct-field-ref kind |
| Anything else | the pre-existing address route |

The managed arm recomputes its `ref` from a live object reference on every access
(`Unsafe.As<T, TDst>(ref …ValueSlot)`), so it is GC-safe and needs no pin. It composes through the
field-ref and array-element reference kinds — a reinterpret of `&s.f` or `&a[i]` aliases the real
storage rather than a copy — and two reinterprets of one box compare equal, as Go requires (ж
equality compares the source object plus the accessor, and the accessor is a static method).

It is an **extension method on `ж<T>?`**, which is what lets a `null` source be tolerated at all: a
zero-valued pointer field is a plain `null`, and an instance call on it throws where Go yields nil.
That is why the emission carries both type arguments.

**Why the managed arm is gated.** Go's rule for `(*TDst)(unsafe.Pointer(p))` is that `TDst` is no
larger than `T` and the two share an equivalent layout — but that rule cannot be inherited, because a
go2cs surrogate's C# layout is not its Go layout: a Go `[2]byte` is 2 bytes while `array<byte>` is a
single reference to a backing store, a Go `string` is 16 bytes while `@string` is 8, a Go `[]byte` is
24 while `slice<byte>` is 32. A **valid** Go reinterpret can therefore become an oversized
`Unsafe.As` that reads past the value slot into the box's own private fields and materializes a
*fabricated managed reference* — a CLR type-safety break, strictly worse than the contained
wrong-read the address route gives. So the alias is taken only where it is demonstrably safe: both
pointees value types; the destination fits inside the source; and either neither type contains
managed references, or the two are layout-compatible in the senses the converter generates (the same
type, a single-field wrapper over the other — Go's struct-embedding idiom and the generated
named-type wrappers — or an identical recursive field-type sequence). Everything else falls back to
the address route, so the change is additive: where it does not apply, behavior is exactly what it
was.

This mirrors **Go's own rule** on the other axis too: a pointer obtained through `unsafe.Pointer` is a
real reference the collector tracks, while a `uintptr` is a number that does not keep its referent
alive — so an arithmetic-derived source (`(*U)(unsafe.Pointer(uintptr(p) + off))`) keeps the address
route.

What this deliberately does **not** cover: Go's prefix-downcast idiom, where the runtime allocates a
larger struct, hands out a pointer to its embedded header, and casts back
(`(*structType)(unsafe.Pointer(t))` with `t` a `*abi.Type`). In Go the larger allocation is really
there; in the managed model a `ж<abi.Type>` holds only an `abi.Type`, so there is nothing behind it
to downcast to. Those sites keep the address route and remain the raw-metal class — which is why the
two of them that converted code actually *reaches*, `abi.Type.StructType()` and `ArrayType()`, are
hand-owned and SYNTHESIZED instead (see
[*`abi.Type`'s SPECIALIZATIONS are synthesized, not downcast*](manual-conversions.md#abitypes-specializations-are-synthesized-not-downcast--structtype--arraytype)).

Emission detail: the peeling is shared with the identity-reinterpret elision
(`pointerConversionSource` — it unwraps an optional `abi.NoEscape`/`noescape` wrapper and an
`unsafe.Pointer(p)` *conversion*, but never a function that merely returns `unsafe.Pointer`, e.g.
`mallocgc`). Identical element types stay the identity elision described above; differing element
types are the genuine reinterpret. The interception sits at the **two points that emit the address
route** — the conversion path and the regular-call path, since `(*U)(unsafe.Pointer(…))`
mis-classifies as a non-conversion and reaches only the latter — deliberately *not* upstream with the
identity elision: the re-box routes above render their own conversions correctly, and diverting them
breaks named ARRAY wrappers, whose lazily-materialized backing store a storage reinterpret bypasses.

**A pointer-to-ARRAY target is excluded at the CONVERTER, not at the golib gate.** For
`(*[N]T)(unsafe.Pointer(p))` the interception can only ever lose. golib never takes the managed arm
for it — `array<U>` is an 8-byte struct holding a backing-store *reference*, so it fails the size
gate against any smaller pointee and the reference gate against any numeric one, and `Reinterpret`
falls straight through to the address route it was meant to replace. But the address route's **text**
is not inert: the slice-of-pointer-cast fusion in `convSliceExpr` keys on a leading `(ж<…>`
(`isPointerCast`) to lower `(*[N]T)(ptr)[:n]` into a `slice<T>` over a `ReadOnlySpan<T>` of the
pointed-to memory — the only correct lowering of that idiom, since an `array<T>` can neither view
native memory nor be punned out of a scalar's bytes. Emitting `Reinterpret` defeats the match and
leaves `(~box).slice(…)` over an `array<T>` whose backing reference was read out of the **pointee's
data**: a fabricated managed reference, i.e. an `AccessViolationException` that kills the process
rather than the contained wrong read the address route gives. Measured end to end on
`internal/syscall/windows/registry.GetStringValue` — the read behind `time.initLocalFromTZI` and
`mime.initMimeWindows`, so essentially every Windows program that formats a local time or looks up a
MIME type: same probe, `Windows 10 Pro` before and a hard fault after. `pointerReinterpretManagedSource`
therefore returns nil for an array-underlying target, restoring the previous emission exactly, fused
or not. (`reparse_windows.path()` → `os.Readlink`, registry `Get`/`SetStringValue` ×4, `os/user`, and
`reflect.gcSlice` are on that route; 17 further corpus sites were already on the address route and
are unchanged.)

(Guarded by the `ReinterpretPointerLifetime` behavioral output test — an aliasing case, plus a
lifetime case whose reinterpreted pointer is the only surviving reference across heavy allocation
churn; before the fix it printed `lifetime: true false false` against Go's `true true true`. The
gate's fallback is guarded by `FixedArrayBufferPointer`, whose fixed-array pin must survive, and the
array-target exclusion by `PointerCastSliceReinterpret`, an output test over the NON-IDENTITY
pointer-cast slice that `PointerCastSliceRange` explicitly defers to "the stdlib exercises that
shape" — which is exactly how the fabrication reached the corpus untested.)

## A pointer-cast slice with a LOW bound offsets the span
`(*[N]T)(ptr)[lo:hi]` lowers to a `slice<T>` over a `ReadOnlySpan<T>` of the pointed-to memory (see
the fusion above). The span was always built from element 0 with length `hi`, dropping the low bound
entirely — so the result held the **wrong elements** whenever `lo` was non-nil, and was right only
when `lo` happened to be 0. Go's expression is the elements `lo..hi`, so the span must start at
element `lo` and run `hi - lo`:

```go
return syscall.UTF16ToString((*[0xffff]uint16)(unsafe.Pointer(&rb.PathBuffer[0]))[n1:n2:n2])
```
```csharp
return syscall.UTF16ToString(new slice<uint16>(new ReadOnlySpan<uint16>(
    (uint16*)(uintptr)(new @unsafe.Pointer(Ꮡ(rb.PathBuffer[0]))) + (int)(n1), (int)(n2) - (int)(n1))));
```

A pointer cast binds tighter than `+`, so the offset lands on the typed pointer with no extra
parentheses. Two live consequences before the fix: `internal/syscall/windows`'s
`(*symbolicLinkReparseBuffer).path()` slices `[n1:n2:n2]` to skip the print name, so `os.Readlink`
returned the reparse buffer from offset 0 instead of the substitute name; and `internal/abi`'s
`FuncType.OutSlice()` slices `[InCount : InCount+outCount]`, so it returned the **in**-parameters
followed by the out-parameters and `reflect.Type.Out(i)` indexed the wrong half (`reflect.gcSlice`
`[begin:end:end]` likewise read the GC bitmap from the wrong start). (Guarded by the non-zero-low
arms of `PointerCastSliceReinterpret` — the reparse `[n1:n2:n2]` shape and a byte reinterpret of a
wider element sliced `[3:7]`, both wrong before and matching Go now. `StdLibInternalAbi`'s golden
re-baselines to the corrected `OutSlice`; its own stdout does not depend on the offset, which is why
it stayed green through the defect.)

## `unsafe.Pointer(p)` on a pointer PARAMETER renders the box, never a deref
A pointer parameter is emitted as the box `ж<T> Ꮡp` plus a deref'd VALUE alias
(`ref var p = ref Ꮡp.Value`). Taking its address through that alias —
`@unsafe.Pointer.FromRef(ref p)` — forces the alias to be materialized, so the entry-time deref
raises a nil-pointer panic for a **nil** argument, even though Go never touches the pointee and
`uintptr(unsafe.Pointer(nil))` is defined to be `0`.

Nil out-pointers are idiomatic in the syscall wrappers. `DuplicateHandle` takes `lpTargetHandle
*Handle` as nil together with `DUPLICATE_CLOSE_SOURCE` to close a handle *without* receiving a
duplicate, and `syscall.StartProcess` does exactly that in a deferred call — so spawning any child
process panicked there.

The address now comes from the **box**: `new @unsafe.Pointer(Ꮡp)`. golib's
`implicit operator uintptr(ж<T>)` already yields precisely the address Go wants — `0` for a nil box,
the aliased address for a native pointer (above), the pinned storage otherwise. Rendering the box
also removes the bare value reference from the body, so an otherwise-unused alias is dropped as dead
by the scan described under *A pointer parameter used only through its box gets no deref VALUE alias*
and the entry deref disappears with it. Parameters whose pointee is a basic or struct type already
took this form; a named-numeric pointee (`*Handle`) and a pointer-to-pointer (`**uint16`) did not.

A pointer **receiver** keeps the `FromRef` form — a `this ref T` receiver has no box to address.
Guarded by `NilPointerParamUnsafePointer` (nil and non-nil arguments across all three pointee shapes,
plus a case where the value alias stays genuinely live so the box rendering must not drop it).

## A reference-type-pointee pointer parameter uses the nil-check-free `.ValueSlot` deref alias

The entry deref-alias for a pointer parameter is `ref var p = ref Ꮡp.Value`. The `.Value` getter
throws `NilPointerDereference` when the box reports `IsNull` — for a MANAGED box, `m_val is null`.
That is correct when the pointee is a VALUE type (a null `m_val` means a genuinely nil pointer). But
when the POINTEE is itself a reference type — `*error`, `*[]T`, `*map[K]V`, `**T`, `*func(…)`,
`*chan T` — the box holds the reference VALUE directly, and that value is legitimately null when it
is the zero value (a nil interface/slice/map). The pointer is still a valid, non-nil box (`Ꮡ(err)`),
so establishing the entry ALIAS is a read of the held value, not a dereference of the box: in Go,
`*(&err)` of a nil `error` yields nil, no panic. `.Value`'s `IsNull` check misfires on `m_val is
null` and panics spuriously at function entry — text/scanner's `digits(…, invalid *bool)` and, for a
reference pointee, text/tabwriter's `handlePanic(err *error)` (deferred from `Write`, whose `err` is
a nil named-return interface) crashed with a nil-pointer panic before `recover()` even ran. The fix:
when the pointee `isInherentlyHeapAllocatedType`, emit the nil-check-free `.ValueSlot` accessor
(`ref var err = ref Ꮡerr.ValueSlot`), mirroring `namedResultBoxAccessor` — a named result of the same
type already reads this way. `.ValueSlot` returns the same real `m_val` slot as `.Value` in every
non-throwing case, so write-through and non-null reads are byte-behaviorally identical; only the
spurious-panic case changes. Corpus-wide the swap touches 49 stdlib files + 10 behavioral goldens, all
value-preserving (full behavioral suite Output 0-fail). (Guarded by the `PointerToInterfaceParamDeref`
behavioral test
— a `*error` parameter read through inside a deferred recover/re-panic where the pointee is nil at
address-of time; before the fix the entry alias NREs, after it prints the re-panic message,
output-compared vs `go run`.) ⚠ This fixes only the spurious CRASH. A SEPARATE latent defect remains:
a non-heap-promoted address-taken named return — `Ꮡ(err)` boxes a COPY — so `*err = …` in the
deferred handler writes the copy while `return err` reads the original; text/tabwriter's tests need
that heap-promotion of address-taken named returns before they fully validate.

The value-type nilable case — a genuinely nil `*rune`/`*bool`/`*int` optional-out-param, deref'd only
under a body VALUE guard — was handled for a fortnight by a companion **call-site nil-argument**
detection, and is now subsumed by the unconditional nil-deferring entry alias (see *A pointer
PARAMETER is nil-deferring for exactly the reason a receiver is*). The problem it solved is worth
keeping on the record, because it is the cleanest demonstration of why an entry-alias policy cannot be
an analysis: `collectNilSafePtrParams` scanned only the body for `param == nil`/`!= nil`, so
text/scanner's `digits(ch0 rune, base int, invalid *rune)` — whose sole deref `*invalid == 0` sits
behind `ch >= max` (never `invalid != nil`) and which is called `digits(ch, 10, nil)` — kept the
strict `.Value` entry hoist and NRE'd at entry, where Go never dereferences (`ch >= max` is false on
the nil-call path). The remedy was a package-wide pre-pass (`collectNilArgPtrParams`) recording, per
`*types.Func`, the pointer-parameter positions ever passed the untyped `nil` at a call site — which
worked, but only for SAME-package call sites, because the converter processes one package at a time.
A parameter passed nil solely from another package stayed strict and stayed broken; that residual is
what `.DerefOrNull()` closes structurally, and the pre-pass was deleted with the rest of the analysis.
(Still guarded by the `GuardedNilPointerParamDeref` behavioral test — a `*int` out-param deref'd under
an `i >= base` guard, called once with a real pointer and once with nil; NREs at the entry hoist under
either predecessor, matches `go run` now without one.)

## A pointer-element composite literal takes the box for a deref-aliased ident

A bare identifier element of a pointer-element composite literal (`[]*CommentGroup{c}`) renders
the pointer VALUE — the box `Ꮡc` — not the deref'd receiver ref-local `c`. Every named pointer
parameter is deref-aliased in C# (`ref var c = ref Ꮡc.Value`), and the bare name is the value
alias; the array element type is `ж<CommentGroup>`, so the alias form was CS0029 (go/ast's
`CommentMap.addComment` — the sibling `append(list, Ꮡc)` already took the box through the
call-argument pointer arm). The routing mirrors the struct-field pointer arm: the element index
is marked `argTypeIsPtr`, which convExprList turns into the pointer ident context:
```csharp
list = new ж<CommentGroup>[]{Ꮡc}.slice();
```
Gated to bare idents of pointer type — keyed elements (maps) and address-of/composite elements
manage their own pointer rendering. Guarded by the `PointerParamWalk` extension `collect` (the
literal arm and the append arm, aliasing proven by a post-collect write through the original).

## A pointer value passed to an `any` argument takes the box
A deref-aliased pointer passed WHOLE (as an argument, not `p.field`) to an EMPTY-interface (`any`)
parameter renders the pointer VALUE — the box `Ꮡp` — not the deref'd value alias `p`. Go boxes the
*pointer* into the interface, so dropping the box stores the pointed-to VALUE and loses pointer
identity: a later `x.(*T)` assertion (rendered `._<ж<T>>()`) then finds a bare `T` and panics
("interface conversion: … is T, not *T"). This is fmt's own `sync.Pool` round-trip —
`func (p *pp) free() { … ppFree.Put(p) }` (Put's parameter is `any`) feeding `newPrinter`'s
`ppFree.Get().(*pp)` — which crashed the SECOND time through the pool, blocking every multi-call fmt
program. Both a pointer RECEIVER and a plain `*T` PARAMETER take the box:
```go
func (p *pp) free()  { poolPut(p) }   // p is *pp (pointer receiver); poolPut(x any)
func keep(q *pp)     { poolPut(q) }   // a plain *T parameter, same shape
```
```csharp
internal static void free(this ж<pp> Ꮡp) {
    ref var p = ref Ꮡp.Value;
    …
    poolPut(Ꮡp.OrTypedNil());          // NOT poolPut(p) — a pp VALUE loses pointer identity
}
internal static void keep(ж<pp> Ꮡq) {
    poolPut(Ꮡq.OrTypedNil());
}
```
(The `OrTypedNil()` suffix is the other half of the same boundary — see
[A pointer crossing into an interface carries its static type](nil-and-zero-values.md#a-pointer-crossing-into-an-interface-carries-its-static-type-however-the-pointer-was-produced),
which generalized this arm from the call-argument slot to every empty-interface slot.)
This mirrors the composite-literal element arm above: the argument index is marked `argTypeIsPtr`,
which convExprList turns into the pointer ident context, so `convIdent` emits the parameter box
(`Ꮡp`) or the current method's direct-ж receiver box. It fires ONLY for the empty interface — a
NON-empty interface already routes the pointer through its `*T`→interface adapter (`interfaceTypes`),
and the two arms are mutually exclusive. A pointer LOCAL is excluded (it already holds its box
directly — the bare name IS the box), an `unsafe.Pointer` argument is excluded (not a `*types.Pointer`),
and the treatment fans out across a variadic `...any`. The receiver form reaches through a closure
too — `Ꮡs.Value.d.note(Ꮡs)` for `s.d.note(s)` inside a nested lambda (the database/sql `(*Stmt)`
shape). Guarded by `PointerValueToInterfaceArg` (a minimal sync.Pool-shaped free list round-tripping
a `*pp` via both a pointer receiver and a pointer param, each `.(*pp)`-asserted after the `any` hop —
the 2nd pool Get panicked before the fix) and the `NestedLambdaReceiverField` receiver-in-closure case.

---

[← Interfaces](interfaces.md) · [Index](README.md) · [Implicit Pointer Dereferencing →](implicit-dereferencing.md)
<!-- {% endraw %} -->
