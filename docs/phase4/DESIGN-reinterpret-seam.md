# DESIGN: the reinterpret seam — `copy` through a Go-layout image of a reference-bearing struct

C1, 2026-11-09. COORD ruling (A), with the view-type shape preferred (owner tenet: the most Go-like visible line).
Branch `claude/c1-reinterpret-seam`, cut from master `fb885891b`.

## The defect

Go's serialization idiom views a record's bytes and copies through the view:

```go
var dirent syscall.Dirent
copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data) // getdirentries_test.go:67
```

The converter emits it as:

```csharp
copy(((ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡdirent))).slice(), data);
```

`Dirent`'s `Name [1024]int8` is a managed `array<sbyte>`, so the box has no pinnable storage. The `(uintptr)`
operator hands out the box's ORDER TOKEN, and `(ж<array<byte>>)` mints a `NativeBox` over it. The first dereference
meets arm 2a: "dereference of a managed pointer with no address". Go's own run of a 30-line repro prints
`7 32 3 abc` and `7 32 3 97 1048`; the C# panics.

## The census (type-aware, go/packages over `std` with tests)

A site is `(*[N]E)(unsafe.Pointer(X))`, E a scalar, X's pointee a REFERENCE-BEARING struct in go2cs's model.

| target | prod | test |
|---|---|---|
| linux/amd64 | 0 | 0 |
| windows/amd64 | 0 | 0 |
| darwin/amd64 | 0 | 1 (`syscall/getdirentries_test.go:67`, `syscall.Dirent` -> `*[1048]byte`) |
| darwin/arm64 | 0 | 1 (the same site) |

The known site found by the tool is the positive control. A widened second derivation (any scalar element, any
reference-bearing pointee) adds only array-to-array views: runtime `alg.go:413`, `mgcmark_greenteagc.go` x3, sha3
`keccakf.go:56`, os `dir_darwin.go:77`. Their pointees are arrays of scalars, whose `T[]` element storage pins and
has a real address, and their rows are banked and pass, so they are NOT this class. The committed production corpus
emits `(ж<array<X>>)(uintptr)(…)` at 79 sites, and none has a reference-bearing struct pointee.

## Why golib alone cannot answer it

- `slice<T>` has exactly two backings: a managed `T[]` or a native base. Dirent's Go-layout bytes live in TWO
  managed objects, the box (its scalars) and `Name`'s `sbyte[]`, so no `slice<byte>` can alias them.
- A box cannot observe a write performed through a view it has already handed out
  (`.claude/rules/golib-gen.md`). A golib hook at the operator runs when the view is TAKEN, not when `copy`
  STORES. A view over a Go-layout snapshot reads right and drops every write: option (B), refused.

## The shape: a Go-layout image view that `copy` accepts

The converter keeps `copy(` at the site and replaces only the view's conversion:

```csharp
copy(@unsafe.GoImage(Ꮡdirent).slice(), data);
```

- `@unsafe.GoImage(ж<S>)` (hand-owned `unsafe` package) returns golib's `GoImage<S>`, a readonly struct naming
  the box and a byte window, initially `[0, sizeof(S))`. It mirrors the slicing surface `ж<array<byte>>` offers
  the converter's slice emission (`.slice()`, `.slice(lo)`, `.slice(lo, hi)`, `.slice(lo, hi, max)` and a
  `Range` indexer), with Go's bounds panics. Slicing returns a narrower `GoImage<S>` and touches nothing.
- `copy` gains overloads, beside the existing ones, for the view as either argument:
  `copy(GoImage<S> dst, slice<byte> src)`, `copy(slice<byte> dst, GoImage<S> src)` and
  `copy(GoImage<S> dst, @string src)`. Each one:
  - ENCODES the struct's Go-layout image into a zeroed scratch buffer of Go's size;
  - copies Go's `min(len, len)` bytes into or out of the window;
  - for a destination view, DECODES the image back into the box.
  Go's semantics for the statement follow: the count copy returns; a partial final record (the test's own comment:
  "cap(buf) < Sizeof(Dirent)"); padding bytes read as zero; and bytes outside the window keep the struct's values.
- `slice<byte>` and its hot paths are untouched. `GoImage<S>` implements no `ISlice`, so the new overloads cannot
  capture or perturb any existing `copy` call.

### The per-type plan: no GC-heap allocation per call

`NativeStructMarshal`'s existing `encode`/`decode` box every value through reflection (one box per array element),
so the image path does not reuse them. A plan per `S` is built once and cached in a static generic class:

- a flat list of segments in Go field order: a SCALAR leaf (Go offset, size, CLR offset inside `S`), and an ARRAY
  of scalar leaves (Go offset, element size, length, CLR offset of the `array<E>` field);
- the CLR offsets are measured once on a zero instance, through `__makeref` within a `fixed` over its raw data (the
  same trick golib's `PrintPointer` uses);
- an array segment reads and writes its elements through `array<E>.ToSpan()`, with a closed set of concrete
  accessors over Go's scalar kinds. No `MakeGenericType`, because a Native AOT image has no code for an
  instantiation nothing made statically (NativeStructMarshal's Leaf note, seat #18).

Per call: `ref S` through the box's `ValueSlot`, then an image in a `stackalloc` buffer up to a cap (1 KiB is not
enough for Dirent's 1048, so the cap is 4 KiB) or an `ArrayPool<byte>` rental above it. Encode, copy, decode:
raw byte moves, no allocation.

### Refused by name (no image exists)

- A struct holding a pointer, string, slice, map, chan, func, interface or unsafe.Pointer field, anywhere inside:
  an identity problem, not a layout one, as the libc funnel refuses it.
- A shape the plan cannot prove: an embed's box hop, an array whose length the type does not carry, a leaf whose
  managed size is not Go's.

### Scope, stated

- The converter recognizes `(*[N]E)(unsafe.Pointer(P))`, sliced, as a DIRECT argument of `copy`, where:
  - E is `byte`/`uint8`;
  - N equals `unsafe.Sizeof` of P's pointee;
  - the pointee is a reference-bearing struct.
  That is the census member's shape exactly.
- An ESCAPING view (`b := (*[N]byte)(unsafe.Pointer(&s))`), another element type, or another N keeps today's
  arm-2a refusal by name. No corpus member reaches those; they are named limits, not silent gaps.

## Predictions and gates

- Red-first on linux:
  - a GolibTests arm over a Dirent-shaped local (write-through, read-back, a partial window, padding zero, the
    pointer-field refusal);
  - a behavioral project, the repro, against Go's output.
- Two-seeded footprint on all three targets: exactly ONE file moves, the darwin syscall `-tests` emission of
  `getdirentries_test.go`. Production is byte-identical, and none of the 79 `(ж<array<X>>)(uintptr)` sites changes.
- CNR for the new behavioral project only.
- Gates:
  - `go test ./...` in `src/go2cs`, `TestLicensing` included;
  - repoguard;
  - GolibTests Release (TieredCompilation=0) and Debug.
- Then the darwin syscall read on both RIDs, to bank. PREDICTION: TestGetdirentries passes on both, and syscall
  validates darwin unless something else in the row moved since .3.
