# DESIGN (stub) -- REC-A: storage for a Go array VALUE (the zh-box record's class 4)

> **Status: STUB, 2026-09-23.** Minted by the H10 relabel ruling (ledger 2026-09-23 03:37, X(2) and
> O1) so that the fixed-array family's `deferred` entries cite a record that names a stage which
> removes the counted allocation. **Owner: C1. Full design: phase-4D kickoff.** Nothing is cut
> against this stub; every line number below was read at `bb54ff0920` (the batch-8b stamp) unless it
> says otherwise, and every figure is the i7 reading run's (`claude/coord-h10-readings` ac9f8251ee,
> Release with tiering off). Feasibility is stated UNMEASURED where it is not measured.
>
> **Fixed up 2026-09-23** per COORD's ACCEPT-WITH-FIXES (ledger 3942e083ad; list
> `docs/phase4/briefs/h10-relabel-fixup.md` at claude/coord-handover 83e4f16ea6, items 2 and 4): the
> md5/sha LOCAL arrays moved to REC-B (COORD's ruling 2), a managed-element arm and a literal-face
> stage added, and §5's shares re-derived from the emission.

## 0. The class

A Go array value `[N]T` -- a struct field, a local, a by-value copy or return, or a composite-literal
temporary -- lives inline in its owner's storage in Go and costs no allocation of its own. go2cs emits
it as golib `array<T>`, a struct over a heap `T[]` (`src/core/golib/array.cs`, the `NewArray`/`CopyOf`
constructors at :68-123), so every array VALUE costs one counted object when it is created and one
more each time it is copied. `DESIGN-zh-box-reduction.md` §6 calls this **class 4** and names
`[InlineArray]` storage as the mechanism that deletes it (:519-521). net10.0 has value-typed fixed
storage, so the class is **expensive, not impossible**: O1 ruled it `deferred`, never `structural`.

## 1. Faces

| face | what allocates | read at |
|:--|:--|:--|
| **field** | the `= new(N)` initializer of a `[N]T` field, run by every explicit constructor | `chunkedReader.buf` (net/http/internal/chunked.cs:40); `digest.s`/`.x` (crypto/md5/md5.cs:39-40); `slog.Record.front` (log/slog/record.cs:38) |
| **by-value copy / return** | `ΔClone()` / `.Clone()` of a struct or array value | md5 `d0 = d.ΔClone()` (md5.cs:168); `return digest.Clone()` (md5.cs:198) |
| **composite-literal temp** | a literal array materialised per evaluation | md5 `var tmp = new byte[]{0x80}.array(72)` (md5.cs:184) -- a `new byte[]` the C# compiler emits and golib's counter does NOT see (`AllocationCounter.cs`, *Coverage*), then `.array(72)`, which is counted |

Local fixed arrays (`var a [65]byte`) are REC-B's local face (`DESIGN-nonescaping-locals.md`), not
this record's: their question is frame lifetime, not representation. That includes the digests'
locals -- `tmp` and `digest` in each `checkSum` (md5.cs:184/:193, sha1.cs:180/:196,
fips140/sha256/sha256.cs:209/:225, fips140/sha512/sha512.cs:283/:302), the block functions' `w`
(sha1block.cs:19, sha256block.cs:82, sha512block.cs:98) and the package `Sum*` functions' `sum`
(crypto/sha256/sha256.cs:61/:73, crypto/sha512/sha512.cs:80 and siblings) -- per COORD's ruling 2
(O1: "REC-B for local arrays"). The `.Clone()` that copies such a local OUT stays this record's copy
face.

## 2. Members (by entry name)

`crypto/md5` TestAllocations; `crypto/sha1` TestAllocations; `crypto/sha256` TestAllocations;
`crypto/sha512` TestAllocations; `net/http/internal` TestChunkReaderAllocs; `crypto/ed25519`
TestAllocations (its Scalar/fiat arrays and SHA-512 digest copies); `crypto/internal/fips140test`
TestEdwards25519Allocations, TestNISTECAllocations/P224, /P256, /P384, /P521 (the fiat field-element
arrays); `log/slog` TestAlloc/* (F3, `Record.front`, through the managed-element arm of §3). Every
one is a MIXTURE and names its other families in its own entry.

## 3. Stages that REMOVE the counted allocation

**First increments (no representation change):**

1. **`ΔClone`/`.Clone()` elision.** A by-value copy whose source is dead after the copy, or whose
   destination is never written, needs no deep copy. md5's `d0 = d.ΔClone()` (md5.cs:168) is copied
   so the caller can keep writing `d`; the copy itself is the Go semantics, but the SECOND array
   allocation per field (`.Clone()` of the returned digest at :198, a value the caller copies again)
   is not. *Removes:* one counted object per elided copy. *Precondition:* a converter-side liveness
   read of the copy's source and destination within one function; refused when either escapes.
2. **`@new`-then-overwrite elision.** `var d0 = @new<Digest>();` followed by `d0.Value = d.ΔClone();`
   (read at crypto/internal/fips140/sha512/sha512.cs:272-273) allocates a zero `Digest` -- including
   its two array fields `h` and `x` (sha512.cs:60-61) -- and immediately overwrites it. Boxing the
   clone directly removes the zero value's arrays. *Removes:* two counted objects per `Sum`.
   *Precondition:* the overwrite is the box's first use.

3. **The literal face: element-wise initialisation.** `var tmp = new byte[]{0x80}.array(72)` (md5.cs:184)
   builds a C# array from the literal -- a `new byte[]` the counter does not see -- and then copies it
   into the 72-byte `array<T>`. Emit the literal's elements into the destination instead
   (`array<byte> tmp = new(72); tmp[0] = 0x80;`). *Removes:* the uncounted literal array, which md5
   needs because it retires only at ZERO BYTES; the counted destination is REC-B's local.
   *Precondition:* the literal's element count does not exceed the destination's length.

**The representation stage (the one that deletes the class):** an `[InlineArray(N)]`-backed storage
for `[N]T` of an unmanaged `T` and small `N`, so a field or copy is inline and a copy is the C# struct
copy, exactly as Go's. *Removes:* every field-face and copy-face object for the admitted types.
*Preconditions:*

- **The shared `slice<T>`-backing question.** `slice<T>` requires a `T[]` backing (zh-box §6,
  :520-522), so an array that is ever sliced (`e.x[:]`, `cr.buf[:2]` at chunked.cs:114) cannot take
  inline storage until slice<T> can window something other than a `T[]`. The options, none chosen
  here: a third backing kind (an owner object plus an offset), or lowering the slice to a `Span<T>`
  once its non-escape is proven (shared with REC-B).
- **Q74-5** (`DESIGN-value-field-representation.md` §6): `array<T>`'s window (`m_low`/`m_length`)
  exists so `(*[N]T)(s)` can alias existing storage. An inline field and an aliasing view are two
  representations of one Go type, and each increment says which sites get which.
- **zh-box §6's 64 KB null-byref precondition** (:522-526, S-F6): inline layouts create the field
  offsets at which a null-byref dereference becomes an unmappable `AccessViolationException`; every
  null-deferring byref path is re-audited against the 64 KB null partition first.
- **The struct-growth byte cost.** A representation change that makes a struct BIGGER states it as a
  per-row formula in its own commit. Q74's precedent: `pallocData` goes from two 16-byte references to
  two 64-byte inline blocks, +96 B per instance (`DESIGN-value-field-representation.md` §4).
- `[GoValueClone]` decisions for the admitted structs become dead and are removed with them (Q74-4).

**The managed-element arm.** `[InlineArray]` accepts reference-containing elements, so a `[N]T` of a
MANAGED `T` is admitted when the array is never sliced, never address-taken as a whole, never pinned and
never viewed over native memory: the slice-backing and window questions then do not arise, while the
64 KB null-byref audit and the struct-growth cost still apply. `slog.Record.front` is `array<Attr>` (log/slog/record.cs:38) over a managed `Attr` (a `@string`
and a `Value` carrying an `any`), and every use of it is an element index or `len` (record.cs:85, :106,
:111, :145-146). *Removes:* F3, one counted object per `NewRecord`.

## 4. Refusals

An array that is sliced, address-taken as a whole (`&a`), converted to or from `unsafe.Pointer`, or
viewed over native memory stays `array<T>` until the backing question is answered for its face; a
managed element type is admitted only by the managed-element arm's four conditions. A copy whose source or destination escapes keeps its clone.

## 5. Predictions (per entry; counted objects per run, i7 readings)

| entry | today | after this record's stages alone |
|:--|--:|:--|
| crypto/md5 TestAllocations | 6 | 3: REC-A takes the copy face (ΔClone's 2 arrays at md5.cs:168, `.Clone()` at :198); left are REC-B's 2 locals (`tmp`, `digest`) and B′'s `heap<digest>` box (:167). The literal stage removes the uncounted `new byte[]` |
| crypto/sha1 TestAllocations | 7 | 4: REC-A takes the copy face (ΔClone 2 + `.Clone()` 1); left are REC-B's 3 locals (`tmp`, `digest`, the block's `w`) and B′'s box |
| crypto/sha256 TestAllocations | 46 | 24: REC-A takes 22 -- the 8 field arrays of the four `@new<Digest>` and the copy face (ΔClone 8, `.Clone()` 4, `sum.Clone()` 2); left are REC-B's 14 locals (`tmp` 4, `digest` 4, `w` 4, `sum` 2), 4 class-3b `@new<Digest>`, 4 B′ boxes and 2 slices; then BYTES on the 2 uncounted shells per run |
| crypto/sha512 TestAllocations | 106 | 46: REC-A takes 60 -- 32 field arrays (16 of `New*`'s `@new`, 16 zero fields of `Sum`'s `d0`, which increment 2 removes) and the copy face (ΔClone 16, `.Clone()` 8, `sum.Clone()` 4); left are REC-B's 28 locals, 16 class-3b and 2 slices; then BYTES on the 4 uncounted shells per run |
| net/http/internal TestChunkReaderAllocs | 2 | 1 (Go's own allocation); the retiring commit states the pass rests on the counter's coverage boundary (the shells at chunked.cs:33/:114 are uncounted) |
| the curve family and ed25519 | 75 to 17,086; 9,503 | UNMEASURED: not decomposed; the share this record removes is measured at the census gate |
| log/slog TestAlloc/* | per entry | one fewer per enabled call (F3, the managed-element arm) |

## 6. Gates

Census first: the population of array VALUE sites per face, measured before any increment, with
three positive controls that must appear in it -- the md5 digest copy (md5.cs:168), `chunkedReader.buf`
(chunked.cs:40) and sha512's `d0` (sha512.cs:272-273). Each increment then reads the members' rows
before and after at Release with tiering off, and a want-0 member retires only at ZERO BYTES
(testing.cs:743).

## 7. SCOUT: `[InlineArray]` storage for a Go array value, against sqlite-cs S22 (dated block, 2026-11-13, C1)

> **Status: SCOUT, design only.** COORD's owner-prompted order of 2026-11-13 (inbox C1 20261113T000333Z). Read at master
> `6869ec2b54`. The figures are from a runtime probe (.NET 10, Release, linux-x64) and a go/types census, both kept in
> `docs/phase4/data/inlinearray-scout/` with their outputs. No converter, generator or golib code is changed by this
> block. What is not measured says NOT MEASURED once.

### 7.1 Can an `[InlineArray]` field make a Go struct's CLR layout equal its Go layout?

**Yes, for a struct whose fields are all unmanaged once its arrays are inline.** Measured with the probe
(`probe-Program.cs`), using S22's own shape, `struct{ FforceAlignment [0]int16; Fa [201]int8; X int32 }`. Go lays it
out as `Fa` at 0, `X` at 204, size 208.

| Check | Result |
|:--|:--|
| a generic per-length helper, `[InlineArray(201)] struct Arr201<T> { T e; }` | loads; `Unsafe.SizeOf<Arr201<sbyte>>()` is 201 |
| the struct with `Arr201<sbyte> Fa`, sequential layout | `X` at 204, size 208: Go's offsets; blittable (`IsReferenceOrContainsReferences` false) |
| the same under `[StructLayout(Explicit, Size = 208)]` at Go's offsets | size 208; `Marshal.SizeOf` 208 |
| a `ref` view over native bytes (`Unsafe.AsRef<T>(ptr)`) | reads `Fa[199]` and `X` in place, and a write lands in native memory, both with no copy |
| a `Span<T>` from the inline field (C# 12) | aliases the native bytes |
| a struct copy | copies the inline array: Go's value semantics, with no `ΔClone` |
| a managed element (`Arr4<string>`) | accepted |
| length 0 | refused by the type loader: "InlineArrayAttribute requires that the length argument is greater than 0" |
| length 16,777,216 (bytes), and 131,073 `long`s | both load; this probe found no size cap |

The runtime's limits, as measured:
- The element may be any type, managed included.
- The helper may be generic over its element.
- The LENGTH is a constant in the attribute, so **every distinct N needs its own type.**
- Length 0 is refused. A Go `[0]T` therefore cannot be an inline array. It is a zero-size field, which the converter
  already lays out under `[StructLayout(LayoutKind.Explicit)]` at Go's offsets for unmanaged structs
  (`src/go2cs/zeroSizeFieldLayout.go:30`). S22's `FforceAlignment` only forces alignment; an explicit layout with Go's
  `Size` reproduces it, and `GoReflect.ClrLayoutMismatch` already skips zero-size fields
  (`src/core/golib/GoReflect.TypeLayout.cs:486`).
- The probe found no size cap. The practical cap is a different one: a value-type local lives on the stack, and a large
  array there is a stack-depth risk that Go avoids by escape analysis. §7.5 caps the first increment by size.

**What the visible C# reads like.** The per-N type is a real cost, and it is stated here, not hidden.

```csharp
// Go:   Fa [201]int8
internal array201<int8> Fa;      // today: internal array<int8> Fa = new(201);
```

- `x.Fa[i]` is an element `ref`, a copy `y := x.Fa` is a struct copy, and `len(x.Fa)` is a constant.
- The name `array201<int8>` keeps both of Go's facts in the C# reading: the length and the element type.
- The cgo mirrors already emit this kind of type, per (N, element) and non-generic (`cgoMirrors.go:344`). A generic
  helper per N is enough here.

**Type identity across assemblies decides where the helper lives.** Go has one `[201]int8`. A C# type has one
defining assembly. So:
- A helper generated in each assembly would make two packages' `[201]int8` different C# types. It also cannot be the
  type of a public field (CS0052) unless it is public, and two public copies collide in namespace `go`.
- **The identity-preserving home is golib**: one shared, hidden (tier 2) family, `go.array<N><T>`, for the lengths
  converted code uses.
- The census bounds that family: std's admitted fields (§7.4) use **46 distinct lengths**, and modernc.org/sqlite/lib's
  use **32**.
- A length golib does not ship stays `array<T>` until the family grows, which happens release by release, under the
  public-surface page's tier-2 rule. A third-party module can need a length std does not, so golib's set cannot be
  closed. That is the cost to rule on.

### 7.2 The slice-backing question, narrowed

`slice<T>` has two backings today: a `T[]`, and since DESIGN-native-backed-slice a native base address
(`src/core/golib/slice.cs:253`, `m_nativeBase`). An inline array inside a heap object is neither.

**A third backing is feasible: (owner object, byte offset).** The probe stores `(owner, Unsafe.ByteOffset(...))` for
the inline field of a struct held in a class box, forces three compacting gen-2 collections, then reads and writes
through `Unsafe.AddByteOffset(ref <owner's data>, offset)`. Both read and write are correct, and the write is seen
through the box. So `s := x.Fa[:]`, followed by a write through `s`, is seen in `x.Fa`: Go's aliasing.

**Held by value or behind a box:**
- **Behind a box** (`ж<T>`, a struct field of a class, an element of a `T[]`): the owner is that heap object. A slice,
  an element pointer (`&x.Fa[i]`) and a whole-array pointer (`&x.Fa`) all window it. Aliasing is Go's.
- **Held by value in a LOCAL or a parameter**: there is no heap owner, so there is nothing to window. A `ref` to stack
  memory cannot be stored in a `slice<T>` field. In Go, slicing makes the array addressable, and Go moves an escaping
  one to the heap. The converter must do the same: a local whose inline array is sliced or address-taken is boxed,
  which is the address-taken-local path that already exists. This collides with REC-B, whose question is when a local
  may STAY on the frame.
- **A COPY** (`y := x`) has its own inline storage, so a slice taken from `x` before the copy keeps aliasing `x`, not
  `y`. That is Go's semantics, and the copy is now a plain struct copy.
- **Cost, NOT MEASURED:** a third backing adds one reference field to every `slice<T>`, making it wider on every path in
  the corpus. The alternative reuses the native backing over a pinned owner. That is C45 option A's lazy box-owned pin
  (`DESIGN-unsafe-header-views.md`): no wider slice, but pinned objects fragment the heap. Either needs its own
  measurement before a cut.

**Q74-5, the window question** (`array<T>`'s `m_low`/`m_length`, which exists so `(*[N]T)(s)` can alias storage): an
inline field and an aliasing view are two representations of one Go type. With a golib family, `(*[201]int8)(p)` over
native memory is simply a native box of `array201<sbyte>`, with no window. A window remains only for an array pointer
derived from a MANAGED slice, which keeps `array<T>`.

### 7.3 S22: read native memory as such a struct, with no copy, and write back

S22 is `var _sqlite3DigitPairs = *(*struct{ FforceAlignment [0]int16; Fa [201]int8; F__ccgo_pad2 [1]byte })(unsafe.Pointer(&struct{ f [201]int8; _ [1]byte }{...}))`
(modernc.org/sqlite v1.62.0, line 1765 of its `lib/sqlite_g_00000000000d89cf.go`, which builds for linux, darwin,
windows and the other 64-bit targets). Both structs are 202 bytes in Go.

- **Today**: `Fa` is `array<sbyte>`, a managed reference, so the native box's whole-struct read is refused by name
  (`src/core/golib/runtime/RuntimeErrorPanic.cs:192`). The refusal is correct: reading Go's bytes into a CLR reference
  field would produce a garbage reference.
- **With inline storage**:
  - Both structs are unmanaged and laid out at Go's offsets, so `ClrLayoutMismatch` answers null and the whole-struct
    read is admitted.
  - The source literal is a managed box. The conversion is the existing `Reinterpret` path, and its size guard
    (`SizeOf(dst) <= SizeOf(src)`, 202 <= 202) admits it.
  - The `*` copies the struct, exactly as Go's `*` does, with no copy beyond Go's own.
  - A WRITE through the converted pointer lands in the source, so aliasing is Go's.
- **Phase 3's image-by-copy** (DESIGN-reinterpret-seam.md's `GoImage`: encode the Go-layout image into a scratch buffer,
  copy, decode):
  - S22 is a one-shot read at init, so the image is CORRECT for it.
  - It costs one encode and one copy per access.
  - Its writes reach the source only if copied back.
  - It is the only route for a struct that keeps a managed field (a string, a pointer), because such a struct cannot
    have Go's layout in the CLR at all.
- **The two are complementary, not rivals:**
  - Inline storage makes the all-unmanaged structs LAYOUT-EQUAL, so they need no image.
  - Images remain for reference-bearing structs.
  - The image encoder must learn inline fields: they are bytes, so the change is small.
- For sqlite-cs specifically, the census (7.4) shows every array field in lib is unmanaged. So inline storage would
  admit all 72 array-holding struct types that lib converts through `unsafe.Pointer`, not S22's one.

### 7.4 The census

go/types, linux/amd64, with test files for std. The tool is `census-main.go`; the outputs are `census-std.txt` and
`census-sqlite-lib.txt`. A field is counted once, keyed by its declaring position, because a package and its test
variant type-check the same declaration twice.

| | std (Go 1.26.10, 968 packages with test variants) | modernc.org/sqlite/lib v1.62.0 |
|:--|--:|--:|
| struct types (named or alias declarations) | 4,224 | 812 |
| ... with an array field | 377 | 147 |
| array fields (struct type expressions included) | 680 (82 only in an anonymous struct) | 229 (96) |
| zero-length (alignment only) | 30 | 126 |
| unmanaged element / managed element | 574 / 106 | 229 / 0 |
| fields ever sliced | 221 | 0 |
| whole address taken (`&x.f`) | 45 | 0 |
| element address taken (`&x.f[i]`) | 56 | 0 |
| converted through `unsafe.Pointer` (`&x.f`, `&x.f[i]`) | 23 | 0 |
| any of REC-A's refusals (sliced, whole address, `unsafe`) | 264 | 0 |
| ADMITTED to a first increment (unmanaged, nonzero, none of the four uses) | 306, over 46 distinct lengths | 103, over 32 distinct lengths |
| array-holding struct types converted to or from `unsafe.Pointer` WHOLE | 66 distinct, at 196 sites | 72 distinct, at 10,817 sites |

**Controls.** The census finds REC-A's own positive controls:
- `net/http/internal.chunkedReader.buf` sliced 2;
- `crypto/md5.digest.x` sliced 6;
- `crypto/internal/fips140/sha512.Digest.x` sliced 5;
- `log/slog.Record.front` with none of the four uses, which is the managed-element arm's condition.

**What the two populations say:**
- std's arrays are mostly reached as VALUES and SLICES. Its 264 refused fields wait on the backing (7.2).
- ccgo's arrays are never sliced or addressed as Go arrays at all. lib reaches memory through `uintptr` arithmetic and
  reads whole STRUCTS through `unsafe.Pointer`, 10,817 times. So for the transpiled-C class, layout equality of the
  struct is the whole question, and the slice backing is not one.

**The lengths.**
- std's admitted fields span 46 lengths, including `[16384]`, `[1048576]` and a `[1<<42]`-class type that no value
  storage could hold. The size cap of 7.5 removes those.
- lib's span 32 lengths, the largest 512.

**NOT MEASURED:**
- sqlite-cs's CONVERTED tree, d5f60ed. This session was refused read access to that repository, so the lib figures
  are from the Go source go2cs converts, at the module's latest version, which may differ from sqlite-cs's pin.
- How many of lib's 10,817 conversions are whole-struct DEREFERENCES rather than field accesses. The census counts
  conversions; a native box already reads fields at Go offsets.
- The per-row byte cost of growing each admitted struct.

### 7.5 Recommendation: an increment ladder

**I0. Layout-equal native structs** (S22's face first).
- **Scope:**
  - **The struct qualifies:** every field is unmanaged once its arrays are inline, it is no larger than 64 KiB, and
    each array field is no larger than 4 KiB. Every field offset therefore stays under the 64 KiB null partition, and
    zh-box §6's null-byref audit has nothing new to clear.
  - **Each array field qualifies:** it is never sliced, never address-taken whole or by element, and never converted
    through `unsafe.Pointer` on its own.
- **Emission:**
  - The array field becomes golib's `array<N><T>`.
  - The struct takes Go's offsets under the existing explicit layout.
  - Zero-length fields stay zero-size fields.
- **Predictions:**
  - lib's 72 whole-converted struct types read layout-equal: `ClrLayoutMismatch` null for each, checked by a GolibTests
    reflection walk over a converted lib.
  - S22's refusal is gone at start-up, and the next wall is S1 (function pointers), TS's stated order.
  - std: up to 306 admitted fields before the size cap, and class 4's FIELD-face objects for them go to zero.
  - REC-A's members do NOT move at I0: md5, sha1, sha256 and sha512 slice their digest arrays (the census controls),
    so they wait for I2.
- **Stop lines:**
  - A struct with any managed field stays as it is.
  - A length golib does not ship stays `array<T>`.
  - A refused face found after emission is a converter bug and stops the increment.
- **Collides with:**
  - H's Phase 3 image class: it leaves images to the reference-bearing structs, and its encoder learns inline fields.
  - The zh-box record: class 4's field face, counted per member.

**I1. The value and copy face.**
- **What changes:** `ΔClone`/`.Clone()` become struct copies for I0's structs, deleting class 4's copy face.
- **Collides with M:N:** an async twin's state machine may hold an inline array as a field. But an element `ref`, or the
  `Span` conversion, may not live across an `await` (CS4007), so the twin emitter must re-read an element after each
  `await` rather than hold a `ref`.
- **Prediction:** REC-A §3's first increments (clone elision, `@new`-then-overwrite) shrink to nothing for I0's
  structs.

**I2. The sliced and address-taken face.**
- **What it needs:** the third `slice<T>` backing (owner, offset), or C45 option A's pinned owner, measured first; the
  element and whole pointer views over (owner, offset); and locals with a sliced inline array boxed (REC-B).
- **What it admits:** std's 264 refused fields, and REC-A's members (md5, sha1, sha256, sha512 and the curve family).
- **Stop line:** the slice-width or pinning cost is posted and ruled before any cut.

**I3. The managed-element arm** (REC-A §3), once I2's views exist, for `slog.Record.front` and its kin.

**Owner rulings needed before I0:**
1. golib as the home of the `array<N><T>` family, with its growth rule: a length outside it stays `array<T>`.
2. The visible name, `array201<int8>`, against the alternatives: a name ending in a glyph, or keeping `array<int8>` in
   source with the length in an attribute. The latter is possible only once I2's backing exists.
3. The I0 size caps (4 KiB per array, 64 KiB per struct).
