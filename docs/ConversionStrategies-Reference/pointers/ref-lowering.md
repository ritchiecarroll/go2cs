# Pointers: Ref Lowering

[Reference index](../README.md) · [Pointers](../pointers.md) · [Summary of this topic](../../ConversionStrategies.md#pointers)

This page covers what reading through a `ж<T>` box costs, and when a pointer parameter is lowered to a C# `ref` parameter instead of a box.

## Zero-allocation dereference

### Reading a pointer and taking a field pointer allocate NOTHING — the two costs hidden inside `ж<T>`

Go's `*p` and `&x.f` are free. Both allocated in go2cs, silently — the code was correct, it merely paid — and the bill was visible only where something counted it. `os.TestWriteStringAlloc` bounds `f.WriteString(s)` at **zero** allocations; the measured cost was over nine thousand bytes per call (**9,184** through the test pipeline, **9,208** under the standalone probe, which writes to its own file rather than the host's `t.TempDir()` one), and a byte-exact decomposition of the probe's number (markers around every frame of `WriteString → File.Write → poll.FD.Write → syscall.Write`, arithmetic closing to the byte) put **5,728 of it — 62 %** in these two places, not in the defer machinery that was the standing suspicion (the frame for that shape is 192 bytes, near 2 %).

**1. `IsNull` boxed the whole pointee on every dereference — 4,760 bytes (52 %).** `Value`'s standard-box branch guards on `IsNull`, whose last term is the value-peeking `m_val is null` (case 2 of the split above — a real address whose reference-typed pointee is legitimately nil). On an **unconstrained** type parameter `is null` compiles to `box !T; ldnull; ceq`, so a term that is constant-false for every struct `T` still allocated *and memcpy'd* a full copy of the pointee, on every read. A pointer to a large record paid its own size per dereference: `os.file` is 592 bytes, and the write path walks eight `of()` links, each bottoming out in one of these. The term is now guarded by a per-`T` `s_valueCanBeNull` (`!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null`), computed from the type rather than by boxing `default(T)`, so type initialization allocates nothing either. The guard also let the peek read the RIGHT storage: a `T` containing no references keeps its value in the pinnable `m_slot` and leaves `m_val` the unused default, so `m_val is null` answered for the wrong slot and every `ж<Nullable<T>>` reported nil whatever it held — unreachable from converted code (Go has no `Nullable`), and wrong, so it is corrected alongside.

**2. `of(…)` minted the untyped accessor wrapper per CALL — 968 bytes (11 %).** `of<TElem>` stores an `object`-taking wrapper around the typed field accessor. The wrapper closes over nothing but that accessor, so it is a pure function OF it — and the accessor is a static method group, which the compiler already caches to a singleton. Minting the wrapper per call therefore bought a fresh display class plus a fresh delegate (88 bytes) for a value identical every time, on every `&x.field` in the corpus. It is now memoized per accessor in a `ConditionalWeakTable`; the keys are weak, so an accessor that is genuinely per-call leaves no permanent entry. Pointer equality is unaffected — it compares the field IDENTITY token (the original accessor), which is what made the per-call wrapper tolerable in the first place.

Together these take `os.File.WriteString` from **9,208 to 3,168 bytes per call (−65.6 %)** — probe and pipeline agreeing to the byte afterwards, the test now printing `expected 0 allocs for File.WriteString, got 3168` — and the same two costs were being paid by every pointer read and every field address in every converted package. The row still does not reach zero — the remainder is the `ж<T>` boxes themselves (1,488 B, of which 608 is one `ж<FD>` whose inline `m_val` slot a field reference never uses), the syscall seam's `unsafe.Pointer`/`heap` boxes (784 B), the defer machinery (192 B — the display class and delegate of each capturing defer) and the `unsafe.StringData` pin (136 B) — inherent to the current pointer and defer models rather than waste inside them. The arc for those is recorded in [`docs/phase4/BOARD-next-validation-candidates.md`](../../phase4/BOARD-next-validation-candidates.md).

golib-only change — no emitted-code difference. (Guarded by `GolibTests.PointerDereferenceAllocationTests`: four measured-byte assertions plus a semantics pair. With the fixes neutered they report 528 B/deref for a 512-byte pointee, 288 B/deref for a reference-bearing one, 32 B/deref through a field-pointer chain, and 200-vs-112 B/call for `of(…)` against a bare box of the same type — the last stated as a COMPARISON rather than a byte count so it survives any future change to `ж<T>`'s layout.)

## Ref-parameter lowering

### A pointer parameter whose every use is a dereference is a `ref` parameter — the ж-box ref-lowering

The emitted-form rule (stage A2 of [`docs/phase4/DESIGN-zh-box-reduction.md`](../../phase4/DESIGN-zh-box-reduction.md), rulings §10.1/§10.3/§10.4): an **unexported package-level function's** pointer parameter whose every body use is a dereference (`*p`, `p.f`, `p[i]`, `range p`), a derived address feeding another lowered position (`&p.f` → a lowered argument), or a forward into another lowered position, emits as a C# **`ref T` parameter** instead of the boxed `ж<T>` — and every call site passes a `ref` expression instead of minting or carrying a box. A `ref T` argument is an alias into the caller's storage the GC tracks and updates, so no pinning, no box, no allocation, and writes through it land in the caller's storage by construction. The signature reads as Go's `*T`, `ref` reads as Go's `&`, and the entry deref preamble disappears because the parameter *is* the alias:

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

### The lowered emission, row by row — the seven argument shapes in emitted code

The seven rows of [`DESIGN-zh-box-reduction.md`](../../phase4/DESIGN-zh-box-reduction.md) §3.3, each with its emitted form and the golden that pins it. Every snippet is verbatim from a committed `.cs.target`, quoted through [`EXEMPLARS-a2-ref-lowering.md`](../../phase4/EXEMPLARS-a2-ref-lowering.md) — which carries the before/after pair and the history for each; only the *current* form is stated here.

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

---

[← Pointers](../pointers.md) · [Index](../README.md)
