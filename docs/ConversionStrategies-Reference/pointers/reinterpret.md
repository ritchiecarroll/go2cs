# Pointers: Reinterpreting a Pointer

[Reference index](../README.md) · [Pointers](../pointers.md) · [Summary of this topic](../../ConversionStrategies.md#pointers)

This page covers pointer conversions that reinterpret storage: a pointer to a defined type over the same underlying type, and array or slice views over a slice or an element pointer.

## Reinterpreting to a defined type

### Reinterpreting a pointer to a defined type with identical underlying — `(*Base)(p)`
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

#### These three arms now ALIAS instead of boxing a copy (2026-08-03, r38-gob)

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

### A reinterpret of a MANAGED pointer aliases the box — it never round-trips through the address
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
[`docs/phase4/FINDING-managed-box-uintptr-lifetime.md`](../../phase4/FINDING-managed-box-uintptr-lifetime.md).

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
[*`abi.Type`'s SPECIALIZATIONS are synthesized, not downcast*](../reflection/types.md#abitypes-specializations-are-synthesized-not-downcast--structtype--arraytype)).

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

## Array and slice views

### The Go 1.17 pointer form aliases the slice's backing store

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

### An element pointer reinterpreted as an array pointer ALIASES the element's storage
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

### A pointer-cast slice with a LOW bound offsets the span
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

---

[← Pointers](../pointers.md) · [Index](../README.md)
