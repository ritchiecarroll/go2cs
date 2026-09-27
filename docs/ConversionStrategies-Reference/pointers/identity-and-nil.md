# Pointers: Identity and Nil

[Reference index](../README.md) · [Pointers](../pointers.md) · [Summary of this topic](../../ConversionStrategies.md#pointers)

This page covers what a pointer's identity and nilness mean in the `ж<T>` model, and how a nil receiver or pointer parameter defers its panic to the first real dereference.

## Pointer identity

### Pointer equality canonicalizes the STORAGE, not the referent — slice/array element identity

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

Canonicalizing `array<T>` to its backing is sound precisely because Go's by-value array COPY is emitted as golib's `.Clone()` (see [Slices and Arrays](../slices-and-arrays.md#slices-and-arrays)), giving the copy its own backing — two distinct Go arrays can never canonicalize to the same storage. Like the `PinnedBuffer` precedent, the change only ever *adds* true results for same-storage-same-index pairs: distinct backings still compare unequal (`&z[0] != &s[0]`), distinct indices still compare unequal (`&s[0] != &s[1]`), and the struct-field and heap-box arms are untouched. This is a golib-only change — no emitted-code difference.

(Guarded by the `SlicePointerIdentity` behavioral output test — self identity, distinctness, re-slice and re-slice-of-re-slice aliasing, the in-capacity `append` result, array-vs-slice-over-that-array, struct elements, a write through an element pointer observed through both views, and `map[*int]` store/overwrite/lookup/miss including a lookup keyed through a *different* window — vs `go run`. Counter-proven: pre-fix **every** identity assertion printed `false`, both map lookups returned empty, and `len(m)` grew from 2 to 3 on the overwrite.)

The same "a box is a temporary, the storage is the object" reasoning answers **lifetime** questions —
when the referent dies, and whether two boxes name the same allocation — which `runtime.SetFinalizer`
and `sync.Cond`'s copy detector both depend on, and which is also why `Ꮡ(IArray<T>, index)` must take
its target **by value**: see
[A pointer's REFERENT, not its box, answers every lifetime and identity question](#a-pointers-referent-not-its-box-answers-every-lifetime-and-identity-question).

### A pointer's nilness and identity are STRUCTURAL — the `IsNull` / `IsNilPointer` split

`ж<T>` answers two different questions that a single predicate used to conflate, and the conflation was a defect *class*: `IsNull` was `m_isNull || m_val is null`, so it reported **true** for three unrelated things —

1. **THE nil pointer** (`m_isNull`: nil-constructed, or the canonical `NilBox`) — the only one that is actually nil;
2. **a real address whose reference-typed pointee is legitimately nil** — `&i` with `i == nil`, `new(any)`, a closure-captured `p *T` local (`ж<ж<T>>`). In Go these are ordinary non-nil addresses, and `*p` yields the nil *value* rather than panicking;
3. **a struct-field or array-element reference box** over a reference-typed `T` — `&s.next`, `&elems[i]` — whose storage lives in the referenced struct/array, leaving `m_val` an unused `default` that reads as null for any reference type. A perfectly valid address, misread as nil purely because of where its storage is.

`IsNilPointer` (`m_isNull`) is the STRUCTURAL predicate, and everything about pointer **identity** keys off it — equality, `GetHashCode`, `PointerOrderToken`, the reflection bridge's `IsNil`/`Elem`, and the dereference guard on `operator ~`. `IsNull` keeps only case 2 (case 3 is now excluded structurally, alongside the native-address exclusion added earlier for the same reason), and is consulted only where peeking at the value is the actual question: the strict `Value` getter, `PinnedBuffer`, and the `uintptr`/`void*` address conversions — where a reference-typed pointee has no reportable address at all, so `0`/`null` is the only answer the managed model can give, and for the value-typed pointees that actually cross into native code the two predicates coincide (`unsafe.Pointer`'s pointee is `uintptr`, so every `IsNull` in that class *is* the structural question).

Fixed consumers: `operator ~` (both the `ж<T>` and the `IPointer<T>` interface form) guard on the structural predicate and read `ValueSlot`, so `*p` on a real address whose pointee is nil yields nil — and a *field-reference* deref reads the field instead of throwing; `DerefOrNil`/`IsNilStandardPointer` likewise, so the pointer-walk re-alias hands back the **real** slot (a write through it persists) rather than the throwaway; and the reflection bridge's interface-routed slot read (`GoReflect.readSlotViaInterface`) and `deepValueEqual`'s cycle-detection `identityRoot` ask `INilPointer.IsNilPointer` instead of the value-peeking property — the latter previously dropped `&i`-shaped values out of cycle detection entirely.

**Two identity rules changed with it.** (a) A standard heap box's identity was formerly *derived from the value it held* whenever `T` was a reference type — two distinct boxes wrapping one referent compared **equal**. That reported `&c == &d` **true** for two distinct `*int` variables holding the same pointer, collapsed `map[**int]V{&c: …, &d: …}` into a **single** entry, and made a pointer's **hash mutate when its pointee was assigned**, so a key inserted while its pointee was nil could never be found again (`m[q]` read back the zero value while `len(m)` still said 1). A pointer's identity is its storage: a standard box *is* the storage, so it hashes and compares by its own identity, and `&x == &x` holds because an addressed Go variable is heap-boxed **once**. (b) Conversely, two boxes **aliasing the same native address** (`m_nativeAddr`) are now the same pointer — a `uintptr` round-trip mints a fresh box each time, and Go requires `(*T)(unsafe.Pointer(p)) == (*T)(unsafe.Pointer(p))`.

golib-only change — no emitted-code difference. (Guarded two ways, because the converter routes every reference-typed-pointee deref through `.ValueSlot` — verified with a 6-shape probe including generics — so `operator ~` is unreachable from converted Go and only golib-internal, hand-owned and reflection-bridge code takes it. The Go-expressible half is in the `PointerToNilPointerIdentity` behavioral output test: distinct-variable identity, `map[**int]` two-key distinctness, hash stability across a pointee assignment, and reference-typed field-reference deref + map keying — pre-fix `&c == &d` printed `true`, the two-key map held **1** entry, and the stable-key lookup read back empty. The unreachable half is in `GolibTests.PointerNilPredicateTests`, which drives `operator ~` (both forms), `DerefOrNil`, `ReadPointerSlot` through a hand-written stand-in for a generated named-pointer wrapper, and native-alias identity — 7 of its 10 assertions fail pre-fix.)

### A pointer's REFERENT, not its box, answers every lifetime and identity question

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

## Nil receivers and parameters

### A pointer parameter used only through its box gets no deref VALUE alias
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

### A pointer RECEIVER compared to `nil` compares its box, not its deref'd value
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

### A pointer RECEIVER's deref alias is nil-DEFERRING — the panic moves to the body, it does not vanish
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

### A pointer PARAMETER is nil-deferring for exactly the reason a receiver is
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

### A receiver or parameter RE-POINTED before first use — the normalization idiom
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

### A reference-type-pointee pointer parameter uses the nil-check-free `.ValueSlot` deref alias

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

## Where a pointer renders its box

### `unsafe.Pointer(p)` on a pointer PARAMETER renders the box, never a deref
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

### A pointer-element composite literal takes the box for a deref-aliased ident

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

### A pointer value passed to an `any` argument takes the box
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
[A pointer crossing into an interface carries its static type](../nil-and-zero-values.md#a-pointer-crossing-into-an-interface-carries-its-static-type-however-the-pointer-was-produced),
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

[← Pointers](../pointers.md) · [Index](../README.md)
