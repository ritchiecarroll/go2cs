# Layout-divergent views: census (P2, 2026-10-16)

Measure-only record for the one layout design COORD ruled on 2026-10-16 (VIEW-OVER-BYTES plus
TAGGED-POINTER, owned by G). Taken on linux, uid 0, Microsoft .NET 10.0.12 / SDK 10.0.112, go1.26.9,
at hop tip be402cb16c.

## What is counted

`main.go` loads `std` with tests for one GOOS and lists every site where Go code views memory as a
struct `T` whose go2cs C# layout can differ from Go's layout. A struct qualifies when it holds,
directly or through a nested struct:

- a reference field: an array (`array<T>` holds a `T[]`), a pointer, an `unsafe.Pointer`, a string,
  slice, map, chan, func or interface; or
- an embedded field (go2cs-gen declares the embed storage in the generated partial, after the
  converter's fields; G's TAGGED-POINTER reading).

Each row has a kind:

| Kind | Go shape | What golib mints today |
|---|---|---|
| BYTES | `(*T)(unsafe.Pointer(&b[i]))` over bytes | `NativeBox<T>` over the pinned byte array (`ж.ElemRefBox.cs:378`) |
| UINTPTR | `(*T)(unsafe.Pointer(u))`, `u` a uintptr | `NativeBox<T>` over the address, or the box a token resolves to |
| UPTR | `(*T)(p)`, `p` an unsafe.Pointer value (unsafe.Add, sysAlloc, a field) | whatever `p` holds |
| RETYPE | `(*T)(unsafe.Pointer(q))`, `q` a `*U` of another type | the aliasing, projection and prefix routes in `ж.PointerExtensions.cs`, else the address route |
| CALLEE | `uintptr(unsafe.Pointer(&v))` handed to a call | the funnel or keystone marshal, else the box's address or token |

Columns: kind, std or test, reasons, file:line:col, package, enclosing func, T, the qualifying fields as name(kind)
(first six).

## Totals

| GOOS | BYTES | UINTPTR | UPTR | RETYPE | CALLEE | all |
|---|---|---|---|---|---|---|
| windows | 19 + 3 test | 30 | 192 + 35 test | 265 + 12 test | 115 + 8 test | 679 |
| linux | 5 | 22 | 182 + 34 test | 263 + 13 test | 80 + 4 test | 603 |
| darwin | 13 | 22 | 180 + 34 test | 272 + 12 test | 87 + 4 test | 624 |

Runtime and reflect hold most rows (windows: runtime 23 of 30 UINTPTR, 134 of 192 UPTR, 110 of 265
RETYPE). The rows are NOT filtered for hand-owned functions, reflect's descriptor projections, or
paths the managed runtime never reaches; a row is a site where the question arises, not a defect.

## The CLR layout of a reference-bearing struct

`clr-offsets/` is a plain .NET 10 console program, without go2cs-gen, that declares four census
structs with an `array<T>`-shaped field (a `T[]` plus two ints) and prints CLR field offsets:

| Struct | Field | Go offset | CLR offset |
|---|---|---|---|
| MountPointReparseBuffer | SubstituteNameOffset, PathBuffer | 0, 8 | 0, 8 |
| SymbolicLinkReparseBuffer | Flags, PathBuffer | 8, 12 | 0, 16 |
| RawSockaddrInet4 | Len, Port, Addr, Zero | 0, 2, 4, 8 | 2, 0, 8, 24 |
| runtime `_func` | funcID, `_` pad, nfuncdata | 40, 42, 43 | 40, 48, 42 |

A struct that holds a reference gets the CLR's automatic layout, which orders fields by size. So the
divergence does not need an embed: SymbolicLinkReparseBuffer has none, and its field order matches
Go's, yet Flags sits at CLR offset 0. MountPointReparseBuffer matches only because its four scalars
are the same size and come first.

## Reading of the os failure (createMountPoint)

`buf := (*MountPointReparseBuffer)(unsafe.Pointer(&byteblob[0]))` mints a `NativeBox<T>` over the
pinned blob (`ж.PointerExtensions.cs:250`, `ж.ElemRefBox.cs:378`). Its `Value` is
`Unsafe.AsRef<T>(address)` (`ж.NativeBox.cs:120`), the CLR layout. `&buf.PathBuffer[0]` is a
`FieldRefBox<array<uint16>>` over that box; `ж.cs:360` asks `TryGetNativeArrayView` first, but
`FieldRefBox` does not override it, so the view reads `Value` and takes blob bytes 8..23 as an
`array<uint16>` header. A zeroed blob reads as a null array of length 0, which is the observed
"index out of range [0] with length 0". A blob the kernel filled would read as a fabricated
reference. `FieldRefBox`'s own Go-offset address (`ж.FieldRefBox.cs:255`) covers one hop off a native
root and is used only by `builtin.NativeFieldArrayPointer`.

## How NativeStructMarshal's layout could serve a view

`NativeStructMarshal.classify` already decides, per struct type, whether every field has a Go-layout
image: it reads Go offsets (`GoFieldOffsets`), recurses into nested structs, admits fixed arrays of
scalars, and names the first field that has no image. The deep form adds the pointer rules (0 is nil,
an unchanged word keeps its box, any other word is a `NativeBox<E>` over that address). A view can use
that classification in two ways:

1. Field-wise, without a copy: the per-field Go offsets give a field view its address (root plus Go
   offset, every hop), a scalar leaf is read and written in place there, and an array of scalar leaves
   becomes a native window at that address. This serves `&p.f`, `p.f[i]` and `uintptr(unsafe.Pointer(&p.f))`.
2. Whole-value: `p.Value` (the emission's `buf.Value.X = v`, and a method's `ref` receiver) needs a
   `ref T` in CLR layout, which the bytes cannot provide. `decode` can fill a managed copy on each
   access and `encode` can write back the bytes the copy changed. A write followed by a direct read of
   the bytes, with no access to the view in between, would miss that write.

The codec reads and writes through reflection (`FieldInfo` per field), which suits cold boundary code
and not the runtime's hot readers.
