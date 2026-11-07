# DESIGN — a Go-layout byte copy over a reference-bearing struct (the reinterpret seam)

> **State: PROPOSED sizing (G, 2026-11-07), design only, held. Nothing lands before 1.26.10.3.** COORD's order: size
> what golib needs for a read and write byte view over a reference-bearing box's Go layout, what it costs, which other
> sites take the same shape, and the refusal it must keep for a view that crosses a reference field. It is owed by
> `syscall`'s darwin row, which stays unbanked at 1.26.10.3 on it (`PLAN-rebank-1-26-10-3.md` section 4.1, RULED: no
> deferral, a real gap). Read at master `d07c7600cb`.

## 1. The shape, and why it fails today

`syscall` `TestGetdirentries` (`getdirentries_test.go:67`) parses each record by copying its bytes into a local:

```go
copy((*[unsafe.Sizeof(dirent)]byte)(unsafe.Pointer(&dirent))[:], data)
```

`syscall.Dirent` is POINTER-FREE in Go: integers plus `Name [1024]int8` and `Pad_cgo_0 [3]byte`. Its C# form is
REFERENCE-BEARING, because each Go fixed array is golib's `array<T>`, a struct over a `T[]`. So `&dirent` has no
address, and its number is an order token.

**A probe of the same shape** (a struct with `Name [16]int8`, converted at master): Go prints `32 104 [0 0 0 0]`. The
converter emits two forms:
- `(*[N]byte)(unsafe.Pointer(&r))[:]` becomes
  `((ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡr))).slice()`. The `uintptr` operator resolves the token
  to `Ꮡr`, a reference-bearing box. Arm 2a (`ж.cs`, `IsOrderTokenAt`) admits the conversion as a native box over the
  token, and the `.slice()` dereference panics by name: "dereference of a managed pointer with no address
  (*array`1 over <token> ... arm 2a)", BOARD 2026-10-26, P2 (a).
- `unsafe.Slice((*byte)(unsafe.Pointer(&r)), n)` becomes `@unsafe.Slice(Ꮡr.Reinterpret<rec, byte>(), n)`. Same
  storage question, through `PointerExtensions.Reinterpret`.

## 2. The options

**(A) A live byte view whose bytes ARE the struct's storage. Impossible.** The C# struct is not laid out as Go's: a
fixed array is a reference to a separate heap array. A `slice<byte>` handed out is a `byte[]` that the converted code
then indexes and copies directly, so neither side can see the other's writes. No box can keep a retained byte slice and
the struct coherent.

**(B) A runtime image with a write-back hook.**
- The view materializes a Go-layout `byte[]` image when it is made.
- golib's `copy` builtin recognizes a destination backed by a registered image and decodes it back into the struct
  after copying.
- **No converter change**, but rejected: it is coherent only for the copy idiom. An image written through an index
  (`b[3] = x`) or held across a later struct write is silently stale. A SILENT wrong answer is the one outcome the
  pointer model refuses everywhere else (arm 2a exists to turn one into a panic).

**(C) RECOMMENDED: the copy idiom, recognized by the converter and carried out by golib, refused loudly everywhere
else.**
- The two idioms Go code uses are a byte copy INTO a struct and a byte copy OUT of one, each made and consumed in one
  expression:
  - `copy(<byte view of &x>[lo:hi], src)`;
  - `copy(dst, <byte view of &x>[lo:hi])`.
  - Either view form counts: `(*[N]byte)(unsafe.Pointer(&x))` or `unsafe.Slice((*byte)(unsafe.Pointer(&x)), n)`.
- Because the view never escapes, a copy-through has exactly Go's semantics: encode, copy, decode.
- Every OTHER use of such a view (retained, indexed, passed on) keeps today's loud refusal, unchanged.

## 3. What (C) needs

**golib, about 60 lines.** Two helpers on `unsafe`'s seam, beside `Slice`, built on what `NativeStructMarshal` already
does for the darwin libc funnel ("a 1048-byte Go-layout copy" of this same `Dirent`):

```csharp
// copy(<byte view of &x>[lo:hi], src): returns the count copied, as Go's copy does.
public static nint CopyIntoGoLayout<T>(ж<T> x, nint lo, nint hi, slice<byte> src);
// copy(dst, <byte view of &x>[lo:hi])
public static nint CopyFromGoLayout<T>(slice<byte> dst, ж<T> x, nint lo, nint hi);
```

Each one:
1. Classifies `T` with the marshaller's `shapeOf` (`NativeStructMarshal.cs`). It needs `Marshallable` and its Go
   `Size`.
2. **Refuses by name** if the shape names a `PointerField` (section 4).
3. Encodes the box's value into a stack or pooled buffer of Go's size (`encode`, by Go's own field order and offsets).
4. Copies `min(len, hi - lo)` bytes in the asked direction.
5. For INTO only, decodes the whole buffer back into the box (`decode`). This read-modify-write keeps the bytes
   outside `[lo, hi)` unchanged, as Go's partial copy does (darwin's copies are `reclen` long, shorter than the
   struct).

`encode` and `decode` are private today; they become internal, and nothing else changes in the marshaller.

**The converter, about 80 lines plus tests.** A recognizer in the `copy` builtin's emission:
- **Matches** an argument that is a slice of a byte-array conversion of `unsafe.Pointer(&x)`, or `unsafe.Slice` of
  `(*byte)(unsafe.Pointer(&x))`.
- **Requires** the element type to be `byte` or `uint8` (an `int8` view is the same element size and could join).
- **Emits** the helper.
- **Leaves alone** any operand that is not such a view, or whose `x` is reference-free in C#. The existing aliasing
  already serves those (the port alias, the `[N]byte` views over scalar fields the census shows).
- **Gates on the C# type:** the recognizer emits the helper only where the converter's type model says `x`'s C# type
  holds a reference. Elsewhere the emission is byte-identical to today, which the corpus diff must show.

## 4. The refusal it must keep

**A view across a Go POINTER field is refused, loudly, by name.** This is not the boundary the CLR draws:
- `Dirent` is reference-bearing in C# but pointer-free in Go, and it is ADMITTED.
- A struct with a `string`, slice, map, pointer, interface, chan, func or `unsafe.Pointer` field is pointer-bearing in
  Go. A byte copy over its pointer words is a garbage-collector-visible pun that a managed runtime cannot make.

The marshaller already draws exactly this line. `classify` returns `PointerField` for the first such field, and its
syscall refusal reads "<field> holds a managed reference (a pointer, string, slice, ...)". The helpers refuse with
the same naming: "go2cs: Go-layout byte copy over <T> crosses pointer field <F>".

**Range refinement, not in this cut:** a copy whose `[lo, hi)` stops before the first pointer word could be admitted
by checking the field offset. No site in the census needs it.

Every non-idiom use of a byte view over a reference-bearing box keeps arm 2a's refusal. Today's behaviour does not
move for any site this seam does not take.

## 5. Census: who takes this shape

Two greps over go1.26.10's `src`, excluding `cmd` and `testdata`:
- `(*[N]byte|int8|uint8)(unsafe.Pointer(`: 59 sites;
- `unsafe.Slice((*byte|uint8|int8)(unsafe.Pointer(&`: 11 sites.

Plus `src/tests/Behavioral` (9 code sites, all over reference-free or native storage).

| site | shape | C# storage | today | this seam |
|:--|:--|:--|:--|:--|
| `syscall/getdirentries_test.go:67` | copy INTO `Dirent` (whole) | reference-bearing, Go pointer-free | **panics (arm 2a)** | **TAKEN**: the owed row |
| `syscall/syscall_darwin.go:311` | copy OUT of `Dirent` (`reclen` bytes) | the same | hand-owned: `getdirentries_darwin_impl.cs` copies native records directly | would take it, so the hand-own's buffer could be retired. Not proposed |
| `os/dir_darwin.go:77` | a RETAINED `[1024]byte` view over `Dirent.Name` (`[1024]int8`), then parsed | an element-type pun of a fixed array | hand-owned (`os/darwin/dir_darwin_impl.cs`) | NOT taken: a retained view, so (C) refuses it by design |
| 30 `(*[2]byte)(unsafe.Pointer(&…Port))` sites in `syscall`, `net`, `internal/poll`; `(*[4]byte)(&nameOff)` in `reflect`, `runtime` and `internal/reflectlite`; `(*[4|8]byte)(&value)` in registry; `routebsd`; `x/net/lif` | byte views over SCALAR fields or locals | reference-free | working: the roster banks these rows | untouched: the recognizer gates on a reference-bearing `x` |
| `runtime` (`lock_spinbit`, `mbitmap`, `signal_*`, `vdso_linux`, `string.go`, `os_linux`), `crypto/internal/boring` | views over NATIVE addresses or native pointers | native | working | untouched |
| `simd/archsimd`, registry `&v[0]` and the `syscall` `&pp.Path[0]` `unsafe.Slice` sites | views over a slice's or array's ELEMENTS | element runs | working (element-run views) | untouched |
| `encoding/binary/binary_test.go:1154` | `unsafe.Slice` over an `int`'s bytes | reference-free | working | untouched |

**The population is one live site,** the `syscall` test. One more production site and one retained-view site are
already hand-owned around the shape. Converted third-party modules are the wider audience (`golang.org/x/sys/unix`
byte-views `Utsname` fields, for one). Their array-field puns are the retained shape, which this seam refuses; they
need the element-pun design, which is separate.

## 6. Cost, gates and the value it buys

| piece | size |
|:--|:--|
| golib helpers, plus `encode`/`decode` made internal | about 60 lines |
| converter recognizer in the `copy` emission | about 80 lines |
| GolibTests | INTO (whole and partial); OUT (partial); a pointer-bearing struct refused by name; a retained view still refused (arm 2a unchanged) |
| converter tests | both view forms recognized; a reference-free `x` byte-identical; a non-copy use left alone |
| behavioral test | the probe above, Go's output `32 104 [0 0 0 0]` |

**Gates:**
- go test, GolibTests and the behavioral suite;
- a corpus reconvert diff that touches only `syscall`'s darwin test files, with every other `copy` emission
  byte-identical;
- CI on the three OSes;
- the darwin `syscall` leg reading `TestGetdirentries` `/n=10` and `/n=1000` matched.

**Value:**
- `syscall` banks darwin, provided its two subtests and their parent were its only residue (ledger 2026-11-05: the
  Getfsstat fix moved 14 verdicts and left exactly these).
- A seam the module conversions can reuse for whole-struct byte copies.

**Not bought:** the retained-view and element-pun shapes, which keep their loud refusal until their own design.
