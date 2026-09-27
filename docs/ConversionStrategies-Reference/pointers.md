<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->
# Pointers

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

The `at<T>(index)` element-address accessor takes a `nint` index. Go permits **any** integer type as an array/slice index and converts it to `int` for the access, but C# has no implicit `nuint`/`uint`/`ulong`→`nint` conversion, so a non-`int` index is narrowed explicitly to match Go's index-to-int conversion (CS1503 otherwise). An `int` index, or an untyped int constant (which renders as a plain int literal), is emitted as-is:

```csharp
var pi = Ꮡa.at<nint>((nint)(i));        // &a[i]      where i is a uintptr
var pe = Ꮡa.at<nint>((nint)(g % 2));    // &a[g%2]    where g is a uint (g%2 widens to long in C#)
```

This is the element-address analogue of the indexed-literal key cast (`SparseArray<T>`, above) and the `IBinaryInteger<T>` width-agnostic length params on `unsafe.Add`/`Slice`/`String`. (Guarded by the `ArrayWideIndexAddress` behavioral test.)

The address of a **slice** element uses the call form `Ꮡ(slice, index)` (golib overloads `Ꮡ<T>(IArray<T>, int)` and `(…, nint)`) rather than `at<T>`. Go `int` (→ `nint`) and the small integer types that implicitly widen to `int` bind directly, but an unsigned-32-or-wider or 64-bit index (`uint`/`uint32`/`uint64`/`uintptr`/`int64`) binds neither overload, so it is cast to `int`: `Ꮡ(s, (int)(i))`. Only those wide/unsigned types are cast — an `int`/`nint` or small-int index is emitted as-is to avoid churn. (Mirrors the runtime's `&datap.pclntable[funcoff]` / `&filetab[fileoff]`, indexed by `uint32` offsets. Guarded by the `ElementAddressUnsignedIndex` behavioral test.)

The `Ꮡ(slice, index)` form applies to **any slice-typed base expression**, not just a named slice variable — a method-**call** result (`&b.stk()[0]`, runtime `mprof.go`; `&StringByteSlice(s)[0]`, `syscall`), a builtin/`make` result, an `unsafe.Slice(…)` result (`reflect`), or a **slice-expression** base (`&x[0:cap(x)][cap(x)-1]`, `math/big`). Such bases have no bare identifier, so they previously fell out of the (identifier-gated) slice arm into the *array* branch — a slice's type name also starts with `[` — whose naive fallback textually prefixed `Ꮡ` onto the postfix chain: `Ꮡb.stk().at<uintptr>(0)` binds as `(Ꮡb).stk()…`, referencing a box that does not exist (CS0103), or copy-boxed the slice header (a lost-write latent). The element address of the returned slice **view** reaches the shared backing array per Go aliasing, so a write through the pointer is visible via the original storage. (Guarded by the `NestedFieldElementAddr` extension — `&st.stk()[0]` through a pointer local, write-through vs Go.)

The same `(int)` narrowing (the shared `castWideIntegerToInt` helper) applies to the bounds of a **3-index (full) slice** `s[low:high:max]`, which lowers to the golib `.slice(nint low, nint high, nint max)` method: a `uintptr`/`uint`/`uint32`/`uint64`/`int64` bound is cast — `stk[:b.nstk:b.nstk]` (b.nstk a `uintptr`) → `stk.slice(-1, (int)(b.nstk), (int)(b.nstk))`. Go's own slice bounds are `int`, so the narrowing matches Go. A plain `int`/small-int bound is left uncast. (The 2-index range forms `s[lo:hi]` narrow through `getRangeIndexer` for the C# `[..]` range operator; only the 3-index `.slice()` form needed this.) (Guarded by the `Slice3IndexWideBound` behavioral test — `uintptr`/`uint`/`uint64` full-slice bounds on an array and a slice + an int control, values verified vs Go; runtime hits this in `mprof`'s `stk[:b.nstk:b.nstk]`.)

**Address of an element of an array *field* reached through a pointer or boxed struct.** When the array being indexed is a field of a heap-boxed value — `&mp.future[i]` where `mp` is a `*memRecord`, or `&g.future[i]` where `g` is an address-taken global — the *array field's* address goes through the box-field accessor first, then the element index: `Ꮡmp.of(memRecord.Ꮡfuture).at<cycle>(i)` (pointer parameter), `mp.of(...)` (pointer local), `Ꮡg.of(rec.Ꮡfuture).at<cycle>(i)` (boxed global). A naive `Ꮡ` prefix on the field read (`Ꮡ(~mp).future`) instead binds `.future` to the box value `Ꮡ(~mp)` (a `ж<memRecord>`, which has no `future` member) → CS1061. This requires a matching golib detail: `ж<T>.at<TElem>(index)` resolves the array through the `Value` property, **not** the raw `m_val` field — for a field-reference pointer produced by `of(...)`, `m_val` is an empty default and the real array lives behind `Value` (the same resolution `of(...)` itself uses). Reading `m_val` would miss the array → null-deref at runtime even though the C# compiled. (`array<T>` is a readonly struct over a shared backing `T[]`, so the value `Value` yields still aliases the real elements; writes through the returned element pointer land.) (Guarded by the `PointerFieldArrayElementAddress` behavioral test — pointer parameter and pointer local both taking `&p.future[i]` and mutating through it.)

The RECEIVER's own array field is the one case that does **not** take this route: a Go pointer receiver renders as `this ref T recv`, which has no box companion, so `of(...)` would name a box that does not exist (`Ꮡr.of(RegArgs.ᏑInts)`, CS0103). It uses the element-aliasing two-arg `Ꮡ(recv.field, (int)(i))` instead — correct because copying an `array<T>` wrapper shares its backing `T[]`. See *Element address of an ARRAY FIELD of the receiver* under Slices and Arrays for the write-dropping bug this replaced (`compress/flate` losing all LZ77 matching).

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
[A pointer's REFERENT, not its box, answers every lifetime and identity question](#a-pointers-referent-not-its-box-answers-every-lifetime-and-identity-question).

## A pointer's nilness and identity are STRUCTURAL — the `IsNull` / `IsNilPointer` split

`ж<T>` answers two different questions that a single predicate used to conflate, and the conflation was a defect *class*: `IsNull` was `m_isNull || m_val is null`, so it reported **true** for three unrelated things —

1. **THE nil pointer** (`m_isNull`: nil-constructed, or the canonical `NilBox`) — the only one that is actually nil;
2. **a real address whose reference-typed pointee is legitimately nil** — `&i` with `i == nil`, `new(any)`, a closure-captured `p *T` local (`ж<ж<T>>`). In Go these are ordinary non-nil addresses, and `*p` yields the nil *value* rather than panicking;
3. **a struct-field or array-element reference box** over a reference-typed `T` — `&s.next`, `&elems[i]` — whose storage lives in the referenced struct/array, leaving `m_val` an unused `default` that reads as null for any reference type. A perfectly valid address, misread as nil purely because of where its storage is.

`IsNilPointer` (`m_isNull`) is the STRUCTURAL predicate, and everything about pointer **identity** keys off it — equality, `GetHashCode`, `PointerOrderToken`, the reflection bridge's `IsNil`/`Elem`, and the dereference guard on `operator ~`. `IsNull` keeps only case 2 (case 3 is now excluded structurally, alongside the native-address exclusion added earlier for the same reason), and is consulted only where peeking at the value is the actual question: the strict `Value` getter, `PinnedBuffer`, and the `uintptr`/`void*` address conversions — where a reference-typed pointee has no reportable address at all, so `0`/`null` is the only answer the managed model can give, and for the value-typed pointees that actually cross into native code the two predicates coincide (`unsafe.Pointer`'s pointee is `uintptr`, so every `IsNull` in that class *is* the structural question).

Fixed consumers: `operator ~` (both the `ж<T>` and the `IPointer<T>` interface form) guard on the structural predicate and read `ValueSlot`, so `*p` on a real address whose pointee is nil yields nil — and a *field-reference* deref reads the field instead of throwing; `DerefOrNil`/`IsNilStandardPointer` likewise, so the pointer-walk re-alias hands back the **real** slot (a write through it persists) rather than the throwaway; and the reflection bridge's interface-routed slot read (`GoReflect.readSlotViaInterface`) and `deepValueEqual`'s cycle-detection `identityRoot` ask `INilPointer.IsNilPointer` instead of the value-peeking property — the latter previously dropped `&i`-shaped values out of cycle detection entirely.

**Two identity rules changed with it.** (a) A standard heap box's identity was formerly *derived from the value it held* whenever `T` was a reference type — two distinct boxes wrapping one referent compared **equal**. That reported `&c == &d` **true** for two distinct `*int` variables holding the same pointer, collapsed `map[**int]V{&c: …, &d: …}` into a **single** entry, and made a pointer's **hash mutate when its pointee was assigned**, so a key inserted while its pointee was nil could never be found again (`m[q]` read back the zero value while `len(m)` still said 1). A pointer's identity is its storage: a standard box *is* the storage, so it hashes and compares by its own identity, and `&x == &x` holds because an addressed Go variable is heap-boxed **once**. (b) Conversely, two boxes **aliasing the same native address** (`m_nativeAddr`) are now the same pointer — a `uintptr` round-trip mints a fresh box each time, and Go requires `(*T)(unsafe.Pointer(p)) == (*T)(unsafe.Pointer(p))`.

golib-only change — no emitted-code difference. (Guarded two ways, because the converter routes every reference-typed-pointee deref through `.ValueSlot` — verified with a 6-shape probe including generics — so `operator ~` is unreachable from converted Go and only golib-internal, hand-owned and reflection-bridge code takes it. The Go-expressible half is in the `PointerToNilPointerIdentity` behavioral output test: distinct-variable identity, `map[**int]` two-key distinctness, hash stability across a pointee assignment, and reference-typed field-reference deref + map keying — pre-fix `&c == &d` printed `true`, the two-key map held **1** entry, and the stable-key lookup read back empty. The unreachable half is in `GolibTests.PointerNilPredicateTests`, which drives `operator ~` (both forms), `DerefOrNil`, `ReadPointerSlot` through a hand-written stand-in for a generated named-pointer wrapper, and native-alias identity — 7 of its 10 assertions fail pre-fix.)

## A pointer's REFERENT, not its box, answers every lifetime and identity question

A `ж<T>` is a *pointer*, and go2cs mints them freely: `Ꮡ(s, i)` allocates a fresh box on every call,
and `Ꮡx.of(T.Ꮡfield)` allocates one per field access. The box is therefore an **expression
temporary** whose lifetime says nothing about the storage it names. Anything that asks a question
about the *object* — when does it die, is this the same object — must ask it of the referent.
`INilPointer.ReferentObject` (golib `ж.cs`) is that projection, and `ж<T>` resolves it the way
`Equals` already resolves pointer identity:

| Pointer shape | `ReferentObject` |
|---|---|
| array/slice element (`&s[i]`) | the canonical backing storage (`CanonicalElement` — the `T[]`, never a per-call header/view) |
| struct field (`&x.f`), including a nested `of()` chain | the **root** allocation, resolved recursively through the per-call intermediate boxes |
| standard heap box (`&x`, `new(T)`), whatever the pointee's type | the box itself — it *is* the allocation |
| native alias (a `uintptr` round-trip) | the box itself — the address it wraps names no *managed* allocation, so there is nothing GC-keyed to resolve to (the one place this projection and `Equals`, which identifies such boxes by that address, part company) |

Two consumers depend on it, and both were broken without it:

* **`runtime.SetFinalizer`** keyed its `ConditionalWeakTable` registration on the boxed `obj`. Go
  attaches a finalizer to the *object* a pointer points at — `runtime.SetFinalizer(&buf[0], f)`
  finalizes `buf`'s allocation — so keying on the throwaway `ж<byte>` the argument expression
  allocated registered against a lifetime nothing in the program shared: the finalizer became due
  the moment the box died (or, under a JIT that roots the whole frame, could never become due at
  all). It now keys on the referent, so the registration tracks exactly the allocation Go would
  finalize, and two boxes for the same address correctly share one registration (Go's "finalizer
  already set").
* **`sync.Cond`'s `copyChecker`**, below.

**`Ꮡ(IArray<T>, index)` takes its target BY VALUE, deliberately.** It used to be `in IArray<T>`.
`in` on an *interface* parameter elides no copy — it is already one reference — but it forces the
caller's boxing temp (a `slice<T>`/`array<T>` header is a struct, so every call boxes one) to be
**address-exposed**, and an address-exposed slot is not lifetime-tracked: the JIT reports it live for
the whole enclosing method. One `Ꮡ(s, i)` therefore pinned `s`'s backing array to the caller's frame
until that method *returned*, in fully optimized code. Measured with a `WeakReference` probe against
a `DOTNET_TieredCompilation=0` build: the `in` form leaks the array, the by-value form releases it.

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

<a id="a-pointer-reflect-handed-out-as-an-unsafepointer-must-convert-back--the-order-token-is-remembered-not-redefined"></a>Moved to [A pointer `reflect` handed out as an `unsafe.Pointer` must convert BACK — the order token is remembered, not redefined](reflection/values.md#a-pointer-reflect-handed-out-as-an-unsafepointer-must-convert-back--the-order-token-is-remembered-not-redefined).

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
Guarded by `NamedPointerReinterpret` (`tail`/`consume`).

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

## The Go 1.17 pointer form aliases the slice's backing store

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

## Nested dereferences parenthesize before the outer `.Value`
A deref whose operand is ITSELF a deref renders with the prefix `~` form, on which a naked postfix `.Value` mis-binds (postfix beats unary: `~X.Value` is `~(X.Value)`). The outer deref wraps the inner one -- reflect `MapOf`'s `**(**mapType)(unsafe.Pointer(&imap))`:
```csharp
var back = (~(ж<ж<array<int64>>>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡip).Value))).Value;
```
Guarded by `PointerCastSliceRange` (compile-shape).

## A range over a pointer-typed type conversion parenthesizes before the deref
Ranging over a pointer to an array implicitly dereferences it — the converter appends `.Value` to the
range expression. When the range expression is itself a pointer-typed TYPE CONVERSION it renders as a C#
cast (`(ж<array<byte>>)(uintptr)(p)`, crypto/internal/nistec's p256 init over
`(*[43*32*2*4][8]byte)(*p256PrecomputedPtr)`). A cast binds LOWER than member access, so a bare append
`(ж<…>)(p).Value` parses as `(ж<…>)((p).Value)` — the deref lands on the operand, not the cast result
(CS1579 "no GetEnumerator" on the box type, CS8130). `visitRangeStmt` now wraps the range expression in
parentheses — `((ж<…>)(p)).Value` — whenever the pointer-unwrap deref is active and `rangeStmt.X` is a
`*ast.CallExpr` whose `Fun` is a type expression (`info.Types[Fun].IsType()`, which catches the
unsafe.Pointer conversions `isTypeConversion` deliberately excludes). Byte-identical corpus-wide (the
pattern only occurs on a pointer-producing conversion in range position, which never compiled before).
Guarded by `RangePointerArrayConversion` (transpile+compile+target only — the exact cast shape needs an
`unsafe.Pointer` source, whose runtime round-trip golib does not reproduce, so it is not output-compared).

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
[*`abi.Type`'s SPECIALIZATIONS are synthesized, not downcast*](reflection/types.md#abitypes-specializations-are-synthesized-not-downcast--structtype--arraytype)).

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

## Moved sections

- <a id="a-func-literal-that-is-only-ever-called-emits-as-a-c-local-function"></a>Moved to [A func literal that is only ever CALLED emits as a C# LOCAL FUNCTION](functions-and-closures.md#a-func-literal-that-is-only-ever-called-emits-as-a-c-local-function).
- <a id="function-literals-returning-unsafepointer-state-their-return-type"></a>Moved to [Function literals returning `unsafe.Pointer` state their return type](functions-and-closures.md#function-literals-returning-unsafepointer-state-their-return-type).
- <a id="interface-returning-literals-with-distinct-arm-types-state-their-return-type-too"></a>Moved to [Interface-returning literals with distinct arm types state their return type too](functions-and-closures.md#interface-returning-literals-with-distinct-arm-types-state-their-return-type-too).
- <a id="multi-value-literals-with-no-fully-typed-arm-state-their-return-type--named-results-included"></a>Moved to [Multi-value literals with no fully-typed arm state their return type — named results included](functions-and-closures.md#multi-value-literals-with-no-fully-typed-arm-state-their-return-type--named-results-included).
- <a id="string-returning-literals-in-assignment-position-state-their-return-type"></a>Moved to [String-returning literals in assignment position state their return type](functions-and-closures.md#string-returning-literals-in-assignment-position-state-their-return-type).
- <a id="numeric-returning-literals-with-untyped-constant-arms-state-their-return-type"></a>Moved to [Numeric-returning literals with untyped-constant arms state their return type](functions-and-closures.md#numeric-returning-literals-with-untyped-constant-arms-state-their-return-type).
- <a id="a-literal-in-generic-result-inference-position-states-its-return-type"></a>Moved to [A literal in GENERIC-RESULT inference position states its return type](functions-and-closures.md#a-literal-in-generic-result-inference-position-states-its-return-type).
- <a id="a-returned-func-literal-is-typeless-in-c"></a>Moved to [A returned FUNC LITERAL is typeless in C#](functions-and-closures.md#a-returned-func-literal-is-typeless-in-c).
- <a id="capturing-the-address-of-a-heap-boxed-local-in-a-closure"></a>Moved to [Capturing the address of a heap-boxed local in a closure](functions-and-closures.md#capturing-the-address-of-a-heap-boxed-local-in-a-closure).
- <a id="a-capture-that-is-written-after-the-capture-point-routes-to-shared-storage-not-a-snapshot"></a>Moved to [A capture that is WRITTEN after the capture point routes to shared storage, not a snapshot](functions-and-closures.md#a-capture-that-is-written-after-the-capture-point-routes-to-shared-storage-not-a-snapshot).
- <a id="a-write-that-encloses-the-literal-counts-as-written-after-capture--the-self-recursive-closure"></a>Moved to [A write that ENCLOSES the literal counts as written-after-capture — the self-recursive closure](functions-and-closures.md#a-write-that-encloses-the-literal-counts-as-written-after-capture--the-self-recursive-closure).
- <a id="a-nested-closures-capture-snapshot-reads-the-enclosing-closures-snapshot"></a>Moved to [A nested closure's capture snapshot reads the enclosing closure's snapshot](functions-and-closures.md#a-nested-closures-capture-snapshot-reads-the-enclosing-closures-snapshot).
- <a id="a-variable-declared-inside-a-closure-is-not-captured-by-it"></a>Moved to [A variable DECLARED INSIDE a closure is not captured BY it](functions-and-closures.md#a-variable-declared-inside-a-closure-is-not-captured-by-it).
- <a id="capture-mode-methods-called-through-a-value-field-of-the-receiver"></a>Moved to [Capture-mode methods called through a value field of the receiver](methods-and-receivers.md#capture-mode-methods-called-through-a-value-field-of-the-receiver).
- <a id="a-direct-ж-method-on-a-value-field-chain-boxes-through-the--machinery"></a>Moved to [A direct-ж method on a value field-chain boxes through the &-machinery](methods-and-receivers.md#a-direct-ж-method-on-a-value-field-chain-boxes-through-the--machinery).
- <a id="field-address-of-a-collision-renamed-heap-boxed-local-uses-the-raw-box-name"></a>Moved to [Field address of a collision-renamed heap-boxed local uses the raw box name](methods-and-receivers.md#field-address-of-a-collision-renamed-heap-boxed-local-uses-the-raw-box-name).
- <a id="a-capture-mode-method-on-a-shadow-renamed-heap-boxed-local-uses-the-rendered-box-name"></a>Moved to [A capture-mode method on a shadow-renamed heap-boxed local uses the rendered box name](methods-and-receivers.md#a-capture-mode-method-on-a-shadow-renamed-heap-boxed-local-uses-the-rendered-box-name).
- <a id="a-colliding-pointer-adapter-name-qualifies-its-foreign-interface-side"></a>Moved to [A colliding pointer-adapter name qualifies its FOREIGN interface side](interfaces.md#a-colliding-pointer-adapter-name-qualifies-its-foreign-interface-side).
- <a id="the-club-41-mop-up-batch-flagflatebinarysyntax-roots"></a>Moved to [The club-41 mop-up batch (flag/flate/binary/syntax roots)](methods-and-receivers.md#the-club-41-mop-up-batch-flagflatebinarysyntax-roots).
- <a id="publicization-decides-what-a-types-modifier-is-the-test-bridge-arm-only-decides-where"></a>Moved to [Publicization decides WHAT a type's modifier is; the test-bridge arm only decides WHERE](type-accessibility.md#publicization-decides-what-a-types-modifier-is-the-test-bridge-arm-only-decides-where).
- <a id="a-white-box-productionproduction-pointer-pair-is-already-implemented--do-not-record-it-again"></a>Moved to [A white-box PRODUCTION↔PRODUCTION pointer pair is already implemented — do not record it again](interfaces.md#a-white-box-productionproduction-pointer-pair-is-already-implemented--do-not-record-it-again).
- <a id="a-goimplement-record-is-gated-on-the-method-set-actually-satisfying-the-interface"></a>Moved to [A GoImplement record is gated on the method set actually satisfying the interface](interfaces.md#a-goimplement-record-is-gated-on-the-method-set-actually-satisfying-the-interface).
- <a id="promoted-methods-through-a-foreign-pointer-embed-forward-via-the-embedded-box"></a>Moved to [Promoted methods through a FOREIGN pointer embed forward via the embedded box](struct-embedding.md#promoted-methods-through-a-foreign-pointer-embed-forward-via-the-embedded-box).
- <a id="promoted-methods-through-embedded-interface-fields-route-per-member-to-the-declaring-field"></a>Moved to [Promoted methods through embedded INTERFACE fields route per-member to the declaring field](struct-embedding.md#promoted-methods-through-embedded-interface-fields-route-per-member-to-the-declaring-field).
- <a id="slice-to-array-the-value-form-copies-the-pointer-form-aliases"></a>Moved to [Slice-to-array: the VALUE form copies, the POINTER form ALIASES](slices-and-arrays.md#slice-to-array-the-value-form-copies-the-pointer-form-aliases).

---

[← Interfaces](interfaces.md) · [Index](README.md) · [Implicit Pointer Dereferencing →](implicit-dereferencing.md)
<!-- {% endraw %} -->
