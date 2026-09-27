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

## A global addressed only by the package's own `_test.go` is still heap-boxed
A Go pointer to a package-level var aliases that var's real storage, which in C# means the global
must be backed by a heap box (see [Pointers](#pointers)); `packageAddressedGlobals` decides that by
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

## Combined field-element address `base.at(field, i)`

The address of an element of an array/slice FIELD of a boxed value — `&x.c[i]` where `c` is an
array field, or the implicit address taken to call a pointer-receiver method `x.c[i].inc()` — was
rendered as a two-step chain `Ꮡx.of(counters.Ꮡc).at<atomic.Int32>(i)`: `of(field)` takes the field's
address (a `ж<array<E>>`), then `at<E>(i)` takes the element's. The explicit `<E>` is needed because
golib's standalone `at<TElem>(nint)` is generic in an element type unrelated to the pointer's `T`, so
it cannot be inferred. golib adds combined overloads `ж<T>.at<TElem>(FieldRefFunc<…array<TElem>…>, nint
index)` (one per field-accessor shape and array/slice kind, each forwarding to `of(field).at<TElem>(i)`)
whose `TElem` IS inferred from the field accessor's return type. The converter then collapses the chain
to `Ꮡx.at(counters.Ꮡc, i)` — dropping both the `.of(` step and the `<E>` type argument. It rewrites the
recursively-built field address `base.of(Type.Ꮡfield)` by retargeting its trailing `.of(field)` to
`.at(field, i)`, only when the field segment is parenthesis-free (a plain `Type.Ꮡfield` accessor, so the
final `)` provably matches the last `.of(`); any other shape falls back to the explicit chained form.
The combined overload is behaviorally identical to the chain (it literally forwards to it). (Guarded by
`ArrayOfCrossPackageType`, `IndexedElementDirectBoxMethod` and `PointerFieldArrayElementAddress` — all
output-compared; the `.inc()`/`bump()` element writes verify runtime equivalence.)

The routing gate sees through **nested value fields to the chain root**. `&pp.wbBuf.buf[0]` (runtime
`mwbbuf.go`) roots at the pointer `pp` through the *value* field `wbBuf`; the original gate checked
pointer-ness only one level up (`pp.wbBuf`, a struct), fell to a naive `Ꮡ` prefix (`Ꮡpp.wbBuf…` — CS1061
on the box), and the same failure hit the closure-captured variant (`&mp.trace.buf[gen%2]`, `trace.go`).
The gate now walks intermediate selectors to the root, so any pointer-rooted (or heap-boxed) chain routes
through the recursive `&field` machinery — `pp.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)` — which already
rendered multi-hop of-chains. A **nested-index** base — `&cache.entries[ck][i]` (2-D array via a pointer,
`symtab.go`) — is an `IndexExpr`, not a selector, so it gets its own arm: recursively take the inner
element's address (`cache.at(pcvalueCache.Ꮡentries, ck)`) and chain the outer `.at<T>(i)` onto it — the
gate also accepts a HEAP-BOXED value root (`&grid.cells[1][2]` on an address-escaping local), fixing that
shape too. An unboxed value-rooted chain keeps the prior naive form (corpus byte-identical). *Known
remaining gap (pre-existing): an intermediate `IndexExpr` inside the selector chain —
`&ptr.items[i].buf[j]`, an array-of-structs hop — defeats the root walk (both arms only step through
selectors) and keeps the CS1061 naive form; the recursive machinery likely has the pieces when a runtime
site demands it.* (Guarded by
the `NestedFieldElementAddr` behavioral test — all three runtime shapes with write-through vs Go; note a
ZERO-VALUED struct's array-field backing is null in the C# emulation — a separate pre-existing latent —
so the test initializes its arrays.)

**Element address of a by-value ARRAY PARAMETER.** Array parameters are cloned by value in the
function preamble (`value = value.Clone();`, Go's array-copy semantics) but are never
escape-analyzed, so they have no heap box — the naive element-address form would name a box that
does not exist (`Ꮡvalue.at<byte>(0)`, CS0103 — syscall `SetsockoptInet4Addr`, `&value[0]` on
`value [4]byte`). The converter boxes a **copy of the wrapper struct** instead:
`Ꮡ(value).at<byte>(0)`. `array<T>` wraps a `T[]` reference, so the copied wrapper SHARES element
storage with the cloned parameter — element reads and writes through the pointer stay behaviorally
correct. (One accepted edge, no stdlib hit: reassigning the *whole* array param after taking an
element address leaves the pointer on the older backing array.) (Guarded by `DeferTypelessReturns`'
`first` — element address of a `[4]byte` parameter, value vs Go.)

**Element address of a POINTER-to-array — `&t[i]` where `t` is `*[N]E`.** Go auto-derefs the index
(`(*t)[i]`), so the element lives in the pointed-to array on the heap; `t` already IS the `ж<[N]E>`
box. The converter emits `t.at<E>(i)` — ж's `at` materializes the array's lazy backing on the REAL
storage and then returns an element pointer over the shared backing. (How `at` reaches that lazy
getter has changed twice and matters: a reflection-built constrained delegate first — fatal under
Native AOT, `d5c0c9c10` — then an unsynchronized box-touch-copy-back, and since 2026-08-30 a
**per-box atomic publish**; see *The array-backing publish is atomic per box* below.) The base is
rendered in POINTER context so it yields the box: a deref-aliased pointer
PARAMETER gives `Ꮡt` (the parameter is `ж<[N]E> Ꮡt`, deref-aliased to `ref var t = ref Ꮡt.Value` in
the prologue), while a box-valued LOCAL from `new([N]E)` gives the plain `t`. Previously this shape
fell through every array/slice branch (the base's type is a `*types.Pointer`, not an array or slice)
to the generic `Ꮡ(t.Value[i])` **copy** form, which boxes a snapshot of the element and silently
drops any write made through the returned pointer. This is exactly hash/crc32's
`slicingMakeTable`/`simpleMakeTable`: `simplePopulateTable(poly, &t[0])` populated a throwaway copy,
leaving every CRC table all-zeros (checksums degenerated to `~0`-with-shifts — `TestGolden`,
`TestSlicing`). The same latent write-through-a-copy bug lurked corpus-wide wherever `&ptr[i]` on a
pointer-to-array was written through — `crypto/internal/nistec` (`p224GG[i].SetBytes(…)` in static
init), `internal/bisect` and `runtime` (`atomic.Store*(&arr[i], …)`) — all now alias correctly. (A
pointer-to-SLICE cannot reach here: Go does not auto-deref `*[]E` for indexing; it is written
`(*t)[i]`, a `StarExpr` the slice branch already aliases.) (Guarded by `PointerToArrayElementAddress`
— write-through `&g[j]` on a `*grid` local, value vs Go.)

**Element address of a SLICE FIELD of the receiver — `&b.lines[i]`.** The address of a slice element
uses one of two golib forms: the **element-aliasing** two-arg `Ꮡ(x, i)` (→ `new ж<T>(IArray, index)`,
whose `ValueSlot` returns `ref backingArray[index]`, so writes land in the shared backing array), or the
**copy-boxing** `Ꮡ(x[i])` (→ `Ꮡ(in T)`, which boxes a *copy* of the element value). For a slice the copy
form is only sound when nothing is written back through the pointer. Inside a pointer-receiver method the
converter had a `refRecv` fast-path that chose the copy form for a "receiver reference to a slice" — but
its detection keyed off `getIdentifier(indexExpr.X)`, which walks the selector chain to its **root**
identifier. So a slice *field* of the receiver — `&b.lines[i]`, whose base `b.lines` roots at the
receiver `b` — matched the fast-path too, and emitted the copy form. text/tabwriter's `terminateCell`
does `line := &b.lines[len-1]; *line = append(*line, cell)`: the `append` grew a *copy* of the row's
slice header and wrote the new length into the boxed copy, never back into `b.lines`, so every line
stayed length 0 and **all formatted output came out empty** (only the newlines survived). The fix
restricts the copy form to the case the receiver is *directly* the slice (`indexExpr.X` is the bare
receiver identifier); any slice base that is a field, call result, or other non-identifier expression
uses the element-aliasing `Ꮡ(x, i)` form — which is correct for a slice in all cases, since a slice value
always shares its backing array. This also corrected a benign read-only site (`NamedFuncTypeStructuralField`'s
`s.by(&s.items[j], &s.items[i])` comparison) from copy to alias. (Guarded by
`SliceFieldElementAddress` — append-through-pointer into a `[][]int` field of a pointer receiver plus an
in-place element mutate, value vs Go; validated end-to-end by `text/tabwriter`'s test suite.) The ARRAY
branch carried the identical defect and is narrowed the same way — next.

**Element address of an ARRAY FIELD of the receiver — `&d.hashHead[h]`.** The array branch had its own
`refRecv` fast-path with the same root-identifier detection, and so the same bug: an array *field* of the
receiver roots at the receiver and took the copy-boxing `Ꮡ(d.hashHead[h])`, whose `Ꮡ(in T)` overload
heap-boxes a copy of the **element**. `compress/flate`'s `deflate()` is the canonical victim — it does
`hh := &d.hashHead[hash&hashMask]; … *hh = uint32(d.index + d.hashOffset)` to maintain the chained hash
table. Every head write landed in a throwaway box, so `hashHead` stayed all-zero, `d.chainHead` was always
`0`, and the `d.chainHead-d.hashOffset >= minIndex` guard (`0-1 >= 0`) meant **`findMatch` was never
called at all**. Levels 2–9 therefore emitted LITERALS ONLY: still-valid deflate streams roughly the size
of `HuffmanOnly` output. Since `png.BestCompression` maps to flate level 9, a 256×256 PNG encoded to
**134,644 bytes instead of Go's 36,760** — pixel-identical on decode, ~3.7× weaker compression, with
`NoCompression`, `HuffmanOnly` and `BestSpeed` (whose `deflatefast.go` encoder writes `e.table[…]`
directly and never takes an element address) all byte-exact, which is what localized it. The fix mirrors
the slice branch: the copy form is kept only when the receiver is *directly* the array (`indexExpr.X` is
the bare receiver identifier); an array field of the receiver uses the element-aliasing two-arg
`Ꮡ(d.hashHead, (int)(…))`.

Note the array field deliberately does **not** route through the `.of(field)`/`.at<T>(i)` box machinery
described above, even though that machinery exists for array fields. Its trigger is `baseIsPointer` — the
*Go* receiver type is `*T` — but a Go pointer receiver renders as `this ref T recv`, which has **no** box
companion, so it emitted `Ꮡr.of(RegArgs.ᏑInts)` for `internal/abi`'s `&r.Ints[reg]` → CS0103. The two-arg
form needs no box and aliases correctly regardless: `array<T>` is a readonly struct wrapping an eagerly
allocated `T[]`, so evaluating the field copies only the wrapper while the copy shares element storage —
the same reasoning the array-*parameter* case above relies on. (That reasoning holds for `array<T>` and
**not** for a field whose type is a NAMED array, whose wrapper allocates its backing lazily; such a
field is projected through `.Value` first — see *The element address of a VIRGIN named array must
materialize through the receiver*. No corpus site currently has that shape; the gate is there because
the shape is legal Go, not because something was found broken.) Seventeen corpus files corrected, several
of them silently broken in the same write-dropping way: `runtime`'s `&r.statusTraced[gen%3]`,
`&h.counts[…]` and `&m.stats[gen]` performed `.CompareAndSwap`/`.Store`/`.Add` **on a copy**;
`crypto/internal/edwards25519` built its lookup tables via `(&v.points[i]).FromP3(…)` into copies;
`image/jpeg` wrote Huffman/quantization tables through `&d.huff[tc][th]` and `&d.quant[…]`. (Guarded by
`RecvArrayFieldElementAddress` — chained-hash write-through with an unsigned index, plus a nested
`&h.pairs[i][j]`, value vs Go; the flate ratio itself is verified by deflating fixed buffers at every
level and byte-comparing the sizes against `go run`.)

## The address of a FIELD of a slice or array element aliases the element

Go's `&s[i].f` is a pointer *into* the backing storage: a write through it changes `s[i]`. The
`&`-machinery builds such an address in two steps — the element's address, then a field reference on
it — and the first step has to be the **element-aliasing** form the index branch already renders for
`&s[i]` itself (`Ꮡ(s, i)` for a slice, `Ꮡarr.at<E>(i)` / `p.at<E>(i)` for an array or a
pointer-to-array). The arm's last-resort fallback instead renders `Ꮡ(<value>)`, a box over a **copy**
of the element, and a field ref rooted there aliases the copy: every write through the pointer is
dropped while every read still looks right, so the container simply never changes.

```go
p_A_Other := &p.Inst[pc].Out        // regexp/onepass.go, onePassCopy
*p_B_Alt = *p_A_Other               // patches the compiled program in place
```

```csharp
var p_A_Other = Ꮡ((~p).Inst, pc).of(onePassInst.ᏑOut);          // aliases the element
// NOT: Ꮡ((~p).Inst[pc]).of(onePassInst.ᏑOut)                   // a box over a COPY — write lost
```

This is the same write-dropping class the slice, array and pointer-to-array index branches each call
out by name (`text/tabwriter`'s empty lines, `compress/flate` emitting literals only at levels 2–9,
`hash/crc32`'s all-zero slicing tables), reached through a **field of the element** rather than
through the element itself. The predicate is `exprIsIndexableElement`: slice, array, or
pointer-to-array only. A map is excluded because Go does not permit `&m[k]` at all, so an index over
one can never legitimately reach the `&`-machinery, and admitting it would mask a front-end error as
a plausible emission; a generic instantiation shares `*ast.IndexExpr`'s shape but types as a
signature or a named type and falls out without a special case. The recursion is ordered *before* the
heap-boxed branch, which already recursed identically for an `IndexExpr` base, so a boxed base
reaches the same emission either way and no existing site moves.

**Why it surfaced when it did.** The PROMOTED case was masked for as long as `go2cs-gen` held an
embed in a shared `ж<T>` box (see *An embedded struct is an INLINE field, so a value copy copies it*
above): the embed's reference semantics meant a copied element still pointed at the origin's embedded
storage, so `Ꮡ(elem).of(T.ᏑPromoted)` reached the real element **by accident**. Making the embed an
inline field was correct and removed that accident, which is what exposed this — `regexp`'s
`onePassCopy` stopped patching, and `TestCompileOnePass` reported `isOnePass=false` for
`^(?:(?:a+)*)$` and `^(?:(?:(?:a*)+))$`. That commit fixed the sibling arm (a promoted
pointer-receiver **call** descending a copy box); this is the address-of-**field** arm of the same
defect. An ordinary, non-embedded field of an element was never masked and was broken all along.

**The base the recursion newly exposed: a pointer RECEIVER over a named array.** `&t[i].field` where
`t` is `*semTable` (`type semTable [4]struct{…}`, runtime's `semtable.rootFor`) now reaches the
index arm's pointer-to-array branch, which renders `t.at<E>(i)` on the assumption that a
pointer-to-array base yields a `ж<[N]E>` box. A Go pointer receiver does not: it renders as
`this ref T recv`, which has no box companion, so `recv.at<E>(i)` names a member the value does not
have (CS1061). It needs none — a named fixed-array type is generated as `IArray<E>` over a shared
backing `E[]`, so the two-arg element-aliasing overload aliases on the wrapper itself. **But that
wrapper's backing is allocated LAZILY, and the two-arg overload takes its target BY VALUE, so on a
still-virgin wrapper the backing materialized on the call site's boxing temp and the receiver's own
storage was never written** — see *The element address of a VIRGIN named array must materialize
through the receiver* below, which is why the emission carries `.Value`:

```csharp
[GoRecv] internal static ж<semaRoot> rootFor(this ref semTable t, nint i) {
    return Ꮡ(t.Value, i).of(semTableᴛ1.Ꮡroot);     // was: Ꮡ(t.Value[i]).of(…) — a COPY
}
```

That is exactly the treatment the receiver's array FIELD already gets in the same arm (see *Element
address of an ARRAY FIELD of the receiver* under Slices and Arrays), for the same reason. A
deref-aliased pointer PARAMETER and a box-valued LOCAL both DO have a box and keep `.at<E>(i)`.

The base has to be the receiver **identifier itself**, not merely rooted at it — the same
object-identity-versus-root-identifier rule the slice and array branches state, inverted.
`getIdentifier` walks a selector chain to its root, so `&p.chunks[l1][l2]` (runtime's
`pageAlloc.chunkOf`) and `&u.inlTree[uf.index]` (`symtabinl`) both report the receiver as their root
while their actual base is a pointer-to-array FIELD — a genuine `ж<[N]E>` rvalue that does have a box
and must keep `.at<E>(i)`. Routing those through the two-arg overload hands it a `ж<array<E>>` where
it wants an `IArray<E>`, which does not bind. Neither shape has a behavioral test, and the corpus is
what caught them: a `-stdlib` reconvert of the affected packages moved both files, and reverting them
is what the identifier restriction does.

Guarded by the **`SliceElementFieldAddress`** behavioral test — the deliberate mirror of
`SliceFieldElementAddress` (that one is `&(slice field)[i]`, this one is `&(slice[i]).field`) —
covering an ordinary field of a slice local and of an array local, a promoted field of a slice field
reached through a pointer, and `onePassCopy`'s own idioms: two pointers into one element swapped and
then written through, and a cross-element `*dst = *src`. The pointer-receiver-over-named-array
sub-case above is guarded by **`NamedArrayAnonElement`**'s Compile and golden phases, and — since
the lazy-backing gap below was closed — behaviorally by **`NamedArrayWrapper`**'s element-address
probe on a virgin wrapper. (`NamedArrayAnonElement`'s own `main` still deliberately never indexes
the array; that note said zero-valuing a named fixed-size array "does not yet materialize its
backing on the value itself", which is exactly the gap the next section closes.)

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

## The THREE deref accessors of `ж<T>` — when each is needed, and how the converter picks

Establishing a local `ref` over a heap box (`ref var p = ref Ꮡp.<accessor>`) looks like one
operation but encodes different answers to one question: **is this access the Go DEREFERENCE, and
what does Go say happens on nil at exactly this point?** Consolidated here because the members
landed across separate arcs (their individual sections, linked below, carry the full derivations);
this is the map.

| Accessor | On nil | The Go semantics it encodes | How the converter KNOWS |
|:--|:--|:--|:--|
| `.Value` | **panics immediately** (Go's message, even on bind) | this access IS the deref, and Go panics here — the ordinary pointer USE site (`*p`, `~Ꮡp`, a read through the box) | the DEFAULT everywhere except a pointer's ENTRY alias; no special case applies |
| `.ValueSlot` | **no check** — the slot as-is | a read of the HELD value, never a deref: when the pointee is itself reference-like, `*p` legally yields nil (`*(&err)` of a nil `error` panics in neither language), so `.Value`'s null check would fire SPURIOUSLY on a legally-held null. Identical to `.Value`'s slot in every non-throwing case. Also where nil is structurally impossible (a freshly `make`-allocated box, `heap(out …)`) and in the reflection bridge's field paths. | by the POINTEE'S TYPE or by CONSTRUCTION — a box-of-pointer LOCAL, a named-result box, the bridge's field walk. NOT at a pointer's entry alias (see below) |
| `.DerefOrNull()` | **defers** — binds `Unsafe.NullRef<T>`, faults with Go's panic on first USE | Go defers the panic to the body's own deref point: passing a nil `*T` to a function, or calling a method through one, is legal; the body RUNS, a side effect before the deref must happen, and the panic lands where Go's would — after it, or never (delegated `checkValid`-style guards). | STRUCTURALLY — EVERY direct-ж pointer ENTRY alias, RECEIVER and PARAMETER alike, unconditionally (no analysis, because the accessor is faithful whether or not the body guards), plus the pointer-reassignment re-alias and go2cs-gen's `ReceiverMethodTemplate` bridge; see *A nil RECEIVER is nil-deferring, not nil-safe* and *A pointer PARAMETER is nil-deferring for exactly the reason a receiver is* |

Why three and not one: the ENTRY alias and the USE site are different questions, and `.Value`
answers the second. `.ValueSlot` is different in KIND rather than in timing — it marks accesses
that were never dereferences in Go's semantics at all, which no nil-policy accessor can express —
but it is not selected at an entry alias, where nothing can know whether the body will dereference
and the nil-policy question is the only one being asked.

**There used to be a fourth, `.DerefOrNil()` — a nil-SAFE accessor handing back a shared
`default(T)` slot — and its retirement (2026-08-02) is what collapsed the set.** It was admitted
by a body ANALYSIS: a pointer param the body nil-compares, one passed the untyped `nil` at a
same-package call site, or one whose first mentioning statement re-points it without dereferencing
(`l = l.get()` normalization). Wherever that analysis was RIGHT the silent zero was unobservable;
wherever it was wrong — and it could never be complete, because a body's guard may be DELEGATED to
a callee it merely hands the pointer to — a deref Go says must panic instead read a silent zero.
Unifying every pointer entry alias on `.DerefOrNull()` made the analysis unnecessary in the first
place, so the accessor, the three analyses that fed it (`collectNilSafePtrParams`,
`reassignedBeforeDerefParamName`, and the package-wide nil-argument pre-pass) and their vestigial
receiver arms were deleted together — 382 net lines of converter. The golib method survives with
its own unit coverage, but converted code no longer emits it.

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
- <a id="a-colliding-pointer-adapter-name-qualifies-its-foreign-interface-side"></a>Moved to [A colliding pointer-adapter name qualifies its FOREIGN interface side](interfaces/adapters.md#a-colliding-pointer-adapter-name-qualifies-its-foreign-interface-side).
- <a id="the-club-41-mop-up-batch-flagflatebinarysyntax-roots"></a>Moved to [The club-41 mop-up batch (flag/flate/binary/syntax roots)](methods-and-receivers.md#the-club-41-mop-up-batch-flagflatebinarysyntax-roots).
- <a id="publicization-decides-what-a-types-modifier-is-the-test-bridge-arm-only-decides-where"></a>Moved to [Publicization decides WHAT a type's modifier is; the test-bridge arm only decides WHERE](type-accessibility.md#publicization-decides-what-a-types-modifier-is-the-test-bridge-arm-only-decides-where).
- <a id="a-white-box-productionproduction-pointer-pair-is-already-implemented--do-not-record-it-again"></a>Moved to [A white-box PRODUCTION↔PRODUCTION pointer pair is already implemented — do not record it again](interfaces/records.md#a-white-box-productionproduction-pointer-pair-is-already-implemented--do-not-record-it-again).
- <a id="a-goimplement-record-is-gated-on-the-method-set-actually-satisfying-the-interface"></a>Moved to [A GoImplement record is gated on the method set actually satisfying the interface](interfaces/records.md#a-goimplement-record-is-gated-on-the-method-set-actually-satisfying-the-interface).
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
