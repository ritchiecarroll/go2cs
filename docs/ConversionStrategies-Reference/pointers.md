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

## Sub-pages

| Page | Covers |
|:--|:--|
| [Identity and nil](pointers/identity-and-nil.md) | storage canonicalization, the IsNull and IsNilPointer split, the referent and its lifetime, nil-deferring receivers and parameters, the normalization idiom, where a deref-aliased pointer renders its box |
| [Ref lowering](pointers/ref-lowering.md) | the zero-allocation dereference, ref-parameter lowering and its argument rows |
| [Reinterpreting a pointer](pointers/reinterpret.md) | reinterpreting to a defined type, the slice-to-array pointer form, element pointers as array pointers, pointer-cast slices |

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
- <a id="pointer-equality-canonicalizes-the-storage-not-the-referent--slicearray-element-identity"></a>Moved to [Pointer equality canonicalizes the STORAGE, not the referent — slice/array element identity](pointers/identity-and-nil.md#pointer-equality-canonicalizes-the-storage-not-the-referent--slicearray-element-identity).
- <a id="a-pointers-nilness-and-identity-are-structural--the-isnull--isnilpointer-split"></a>Moved to [A pointer's nilness and identity are STRUCTURAL — the `IsNull` / `IsNilPointer` split](pointers/identity-and-nil.md#a-pointers-nilness-and-identity-are-structural--the-isnull--isnilpointer-split).
- <a id="a-pointers-referent-not-its-box-answers-every-lifetime-and-identity-question"></a>Moved to [A pointer's REFERENT, not its box, answers every lifetime and identity question](pointers/identity-and-nil.md#a-pointers-referent-not-its-box-answers-every-lifetime-and-identity-question).
- <a id="a-pointer-parameter-used-only-through-its-box-gets-no-deref-value-alias"></a>Moved to [A pointer parameter used only through its box gets no deref VALUE alias](pointers/identity-and-nil.md#a-pointer-parameter-used-only-through-its-box-gets-no-deref-value-alias).
- <a id="a-pointer-receiver-compared-to-nil-compares-its-box-not-its-derefd-value"></a>Moved to [A pointer RECEIVER compared to `nil` compares its box, not its deref'd value](pointers/identity-and-nil.md#a-pointer-receiver-compared-to-nil-compares-its-box-not-its-derefd-value).
- <a id="a-pointer-receivers-deref-alias-is-nil-deferring--the-panic-moves-to-the-body-it-does-not-vanish"></a>Moved to [A pointer RECEIVER's deref alias is nil-DEFERRING — the panic moves to the body, it does not vanish](pointers/identity-and-nil.md#a-pointer-receivers-deref-alias-is-nil-deferring--the-panic-moves-to-the-body-it-does-not-vanish).
- <a id="a-pointer-parameter-is-nil-deferring-for-exactly-the-reason-a-receiver-is"></a>Moved to [A pointer PARAMETER is nil-deferring for exactly the reason a receiver is](pointers/identity-and-nil.md#a-pointer-parameter-is-nil-deferring-for-exactly-the-reason-a-receiver-is).
- <a id="a-receiver-or-parameter-re-pointed-before-first-use--the-normalization-idiom"></a>Moved to [A receiver or parameter RE-POINTED before first use — the normalization idiom](pointers/identity-and-nil.md#a-receiver-or-parameter-re-pointed-before-first-use--the-normalization-idiom).
- <a id="a-reference-type-pointee-pointer-parameter-uses-the-nil-check-free-valueslot-deref-alias"></a>Moved to [A reference-type-pointee pointer parameter uses the nil-check-free `.ValueSlot` deref alias](pointers/identity-and-nil.md#a-reference-type-pointee-pointer-parameter-uses-the-nil-check-free-valueslot-deref-alias).
- <a id="unsafepointerp-on-a-pointer-parameter-renders-the-box-never-a-deref"></a>Moved to [`unsafe.Pointer(p)` on a pointer PARAMETER renders the box, never a deref](pointers/identity-and-nil.md#unsafepointerp-on-a-pointer-parameter-renders-the-box-never-a-deref).
- <a id="a-pointer-element-composite-literal-takes-the-box-for-a-deref-aliased-ident"></a>Moved to [A pointer-element composite literal takes the box for a deref-aliased ident](pointers/identity-and-nil.md#a-pointer-element-composite-literal-takes-the-box-for-a-deref-aliased-ident).
- <a id="a-pointer-value-passed-to-an-any-argument-takes-the-box"></a>Moved to [A pointer value passed to an `any` argument takes the box](pointers/identity-and-nil.md#a-pointer-value-passed-to-an-any-argument-takes-the-box).
- <a id="reading-a-pointer-and-taking-a-field-pointer-allocate-nothing--the-two-costs-hidden-inside-жt"></a>Moved to [Reading a pointer and taking a field pointer allocate NOTHING — the two costs hidden inside `ж<T>`](pointers/ref-lowering.md#reading-a-pointer-and-taking-a-field-pointer-allocate-nothing--the-two-costs-hidden-inside-жt).
- <a id="a-pointer-parameter-whose-every-use-is-a-dereference-is-a-ref-parameter--the-ж-box-ref-lowering"></a>Moved to [A pointer parameter whose every use is a dereference is a `ref` parameter — the ж-box ref-lowering](pointers/ref-lowering.md#a-pointer-parameter-whose-every-use-is-a-dereference-is-a-ref-parameter--the-ж-box-ref-lowering).
- <a id="the-lowered-emission-row-by-row--the-seven-argument-shapes-in-emitted-code"></a>Moved to [The lowered emission, row by row — the seven argument shapes in emitted code](pointers/ref-lowering.md#the-lowered-emission-row-by-row--the-seven-argument-shapes-in-emitted-code).
- <a id="reinterpreting-a-pointer-to-a-defined-type-with-identical-underlying--basep"></a>Moved to [Reinterpreting a pointer to a defined type with identical underlying — `(*Base)(p)`](pointers/reinterpret.md#reinterpreting-a-pointer-to-a-defined-type-with-identical-underlying--basep).
- <a id="these-three-arms-now-alias-instead-of-boxing-a-copy-2026-08-03-r38-gob"></a>Moved to [These three arms now ALIAS instead of boxing a copy (2026-08-03, r38-gob)](pointers/reinterpret.md#these-three-arms-now-alias-instead-of-boxing-a-copy-2026-08-03-r38-gob).
- <a id="a-reinterpret-of-a-managed-pointer-aliases-the-box--it-never-round-trips-through-the-address"></a>Moved to [A reinterpret of a MANAGED pointer aliases the box — it never round-trips through the address](pointers/reinterpret.md#a-reinterpret-of-a-managed-pointer-aliases-the-box--it-never-round-trips-through-the-address).
- <a id="an-element-pointer-reinterpreted-as-an-array-pointer-aliases-the-elements-storage"></a>Moved to [An element pointer reinterpreted as an array pointer ALIASES the element's storage](pointers/reinterpret.md#an-element-pointer-reinterpreted-as-an-array-pointer-aliases-the-elements-storage).
- <a id="a-pointer-cast-slice-with-a-low-bound-offsets-the-span"></a>Moved to [A pointer-cast slice with a LOW bound offsets the span](pointers/reinterpret.md#a-pointer-cast-slice-with-a-low-bound-offsets-the-span).
- <a id="unsafealignof--unsafeoffsetof-name-a-type-resolved-through-gotypes"></a>Moved to [`unsafe.Alignof` / `unsafe.Offsetof` name a TYPE, resolved through `go/types`](unsafe-and-native-memory.md#unsafealignof--unsafeoffsetof-name-a-type-resolved-through-gotypes).
- <a id="unsafesizeof--alignof--offsetof-fold-to-a-constant-at-expression-sites"></a>Moved to [`unsafe.Sizeof` / `Alignof` / `Offsetof` FOLD to a constant at expression sites](unsafe-and-native-memory.md#unsafesizeof--alignof--offsetof-fold-to-a-constant-at-expression-sites).
- <a id="the-run-time-unsafesizeof-answers-through-gos-layout-rule-not-the-clrs-marshaller"></a>Moved to [The run-time `unsafe.Sizeof` answers through Go's layout rule, not the CLR's marshaller](unsafe-and-native-memory.md#the-run-time-unsafesizeof-answers-through-gos-layout-rule-not-the-clrs-marshaller).
- <a id="converting-a-go-pointer-to-unsafepointer"></a>Moved to [Converting a Go pointer to `unsafe.Pointer`](unsafe-and-native-memory.md#converting-a-go-pointer-to-unsafepointer).
- <a id="an-opaque-struct-conversion-mints-a-recoverable-token--syscallpointerunsafepointerp-keeps-its-referent"></a>Moved to [An OPAQUE `*struct{}` conversion mints a recoverable token — `syscall.Pointer(unsafe.Pointer(p))` keeps its referent](unsafe-and-native-memory.md#an-opaque-struct-conversion-mints-a-recoverable-token--syscallpointerunsafepointerp-keeps-its-referent).
- <a id="pointer-display-never-dereferences-out-of-range-unsafestringdata-is-nil"></a>Moved to [Pointer DISPLAY never dereferences out-of-range; `unsafe.StringData("")` is nil](unsafe-and-native-memory.md#pointer-display-never-dereferences-out-of-range-unsafestringdata-is-nil).
- <a id="a-numeric-value-pun-read-is-a-bitcast--uint64unsafepointerf-boxes-nothing"></a>Moved to [A numeric value pun READ is a bitcast — `*(*uint64)(unsafe.Pointer(&f))` boxes nothing](unsafe-and-native-memory.md#a-numeric-value-pun-read-is-a-bitcast--uint64unsafepointerf-boxes-nothing).
- <a id="the-pointer-word-read--unsafepointerunsafepointerx-emits-a-carrying-pointer-never-a-byte-pun"></a>Moved to [The pointer-word read — `*(*unsafe.Pointer)(unsafe.Pointer(&x))` emits a CARRYING pointer, never a byte pun](unsafe-and-native-memory.md#the-pointer-word-read--unsafepointerunsafepointerx-emits-a-carrying-pointer-never-a-byte-pun).
- <a id="pointer-derived-funnel-arguments-and-bridged-unsafepointer-arguments-keep-their-box-alive-across-the-call--the-statement-scoped-gckeepalive-drain"></a>Moved to [Pointer-derived funnel arguments and bridged `unsafe.Pointer` arguments keep their box alive across the call — the statement-scoped `GC.KeepAlive` drain](unsafe-and-native-memory.md#pointer-derived-funnel-arguments-and-bridged-unsafepointer-arguments-keep-their-box-alive-across-the-call--the-statement-scoped-gckeepalive-drain).
- <a id="an-address-of-managed-storage-that-outlives-its-statement-must-carry-a-pin"></a>Moved to [An address of MANAGED storage that outlives its statement must carry a PIN](unsafe-and-native-memory.md#an-address-of-managed-storage-that-outlives-its-statement-must-carry-a-pin).
- <a id="a-reinterpreted-raw-address-aliases-native-memory-instead-of-boxing-a-copy"></a>Moved to [A reinterpreted raw address ALIASES native memory instead of boxing a copy](unsafe-and-native-memory.md#a-reinterpreted-raw-address-aliases-native-memory-instead-of-boxing-a-copy).
- <a id="unsafeslice-over-managed-element-storage-aliases-it"></a>Moved to [`unsafe.Slice` over MANAGED element storage ALIASES it](unsafe-and-native-memory.md#unsafeslice-over-managed-element-storage-aliases-it).
- <a id="a-pointer-reflect-handed-out-as-an-unsafepointer-must-convert-back--the-order-token-is-remembered-not-redefined"></a>Moved to [A pointer `reflect` handed out as an `unsafe.Pointer` must convert BACK — the order token is remembered, not redefined](reflection/values.md#a-pointer-reflect-handed-out-as-an-unsafepointer-must-convert-back--the-order-token-is-remembered-not-redefined).

---

[← Interfaces](interfaces.md) · [Index](README.md) · [Implicit Pointer Dereferencing →](implicit-dereferencing.md)
<!-- {% endraw %} -->
