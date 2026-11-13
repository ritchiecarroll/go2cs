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

### 7.6 Review revision (dated block, 2026-11-13, C1)

> **Status: REVISION of 7.1-7.5 after the Fable review** (COORD, inbox C1 20261113T003918Z). The review approved the
> ladder with changes and rejected I0 as scoped. 7.1-7.5 stand above as written; where this block contradicts them,
> this block governs. Code read at master `130cecb136`; every file cited below is unchanged since `6869ec2b54`. New
> evidence is in `docs/phase4/data/inlinearray-scout/`:
> - `probe-r2-Program.cs` and `probe-r2-output.txt`: the first probe plus the review's shapes, 0 checks failed. It
>   builds with `probe.csproj`.
> - `probe-await-Program.cs` and `probe-await-build.txt`: a project that MUST fail to build. Its error list is the
>   measurement.
> - `probe-eq-Program.cs` and `probe-eq-output.txt`: default equality, run twice in each call order.
> - `go-layout-main.go` and `go-layout-output.txt`: Go's own sizes and offsets for the probed shapes.
> - `census-r2-main.go`, with `census-r2-std.txt`, `census-r2-sqlite-lib.txt` and `census-r2-sqlite-and-lib.txt`.
>   The r2 outputs carry the distinct-lengths lines (item 8).
>
> Runtime: .NET SDK 10.0.403, runtime 10.0.14, linux-x64, Release. NO CODE until the owner rules the questions in
> 7.6.10.

#### 7.6.1 Item 1 (BLOCKER): admission is declaration-local and opt-in; an unanalysed face fails to compile

**The scout's predicate was wrong.** It was a whole-program usage fact, so a `_test.go` file, a later package or a
consumer could change a struct's layout between a shipping conversion and a `-tests` conversion. It also missed faces.
The r2 census counts the missed faces over std:

| face | fields | positive control |
|:--|--:|:--|
| implicit address: a pointer-receiver method called on `x.f` or `x.f[i]` | 29 | `runtime.timeHistogram.counts`, 3 sites. Go's `h.counts[i].Add(1)` is master's `Ꮡ(h.counts, …).Add(1)` at `src/core/runtime/histogram.cs:133` |
| an address or slice below the field (`x.f[i][:]`, `&x.f[i].g`) | 64 | none needed; counted by the same walk |
| a whole-value use (copy, argument, conversion, `==`, store, `range` with a value) | 118 fields, 225 sites | none needed; counted by the same walk |

- Of the scout's 306 admitted std fields, 6 have an implicit address and 2 more have a face below the field.
  - The 6: `internal/abi.RegArgs.ReturnIsPtr`, `reflect.abiDesc.inRegPtrs` and `.outRegPtrs`,
    `reflect.makeFuncCtxt.regPtrs`, `runtime.timeHistogram.counts` and `runtime.traceSchedResourceState.statusTraced`.
  - The 2: `image/jpeg.decoder.huff` and `image/jpeg.encoder.quant`.
  - So 306 was an over-count, and "298 remain" is no better: a census cannot list REFLECTION faces at all.
- In modernc.org/sqlite/lib every new column reads 0. Loading the parent package modernc.org/sqlite with it changes
  nothing but 2 more whole-struct conversion sites (10,819 against 10,817).

**Reflection is a face the census cannot see.** Measured: `FieldInfo.GetValue` on an inline field returns a COPY, so an
element write through it is lost. Through a reference-backed field, which is `array<T>`'s shape, the same write lands.
Writing at the field's byte offset inside the owner lands.

**The revised I0 rule.**
1. **Admission is a property of the DECLARATION.** A struct type qualifies if:
   - it is declared in a package that has OPTED IN;
   - every field is unmanaged once its arrays are inline;
   - it is within the caps (7.6.10, ruling 3) at every nesting level.

   How fields are used never enters the rule, so a shipping conversion and a `-tests` conversion cannot disagree.
2. **The opt-in is per package.** The first packages are ccgo-transpiled ones (modernc.org/sqlite/lib), and anonymous
   struct type expressions converted through `unsafe.Pointer` in those packages. std stays out until I2.
   - The opt-in is a conversion setting of the DECLARING package.
   - A consumer package sees the inline type through the declaring assembly's metadata, so it needs no setting of its
     own.
   - The setting's mechanism (a converter flag, or a property in the converted project) is COORD's to rule.
3. **An unanalysed face is a C# COMPILE ERROR, never a silent copy.** The I0 type exposes only an element indexer
   (`ref T this[int]`) and the constant length. It has:
   - no conversion to `array<T>`, `slice<T>` or `Span<T>`;
   - no slicing member;
   - no address member.

   **Measured constraint: an `[InlineArray]` type cannot be that type.** C# converts any `[InlineArray]` type to
   `Span<T>` implicitly, and a type cannot opt out of that. So the I0 type WRAPS the inline storage in a private field.
   In `probe-await-build.txt`:
   - `Span<int> s = p.a` over the `[InlineArray]` type compiles;
   - the same over a wrapper that exposes only the indexer and the length fails with CS0029.
4. **A whole-value read is an explicit copy the converter emits**, including before boxing to an interface. The copy
   is the C# struct copy, which is Go's semantics. std's 225 sites show this face is common where I0 will eventually
   reach; lib has none.
5. **Reflection goes by reference for an inline field, or refuses loudly.**
   - golib's reflect bridge reads and writes an inline field through (owner, Go field offset), the route measured to
     land above.
   - Where it cannot (a value reached only as a copy), it panics and names the field.
   - A `FieldInfo.GetValue` copy is never handed out as an addressable array.

#### 7.6.2 Item 2 (MAJOR): `[0]T` becomes an overlapped readonly alignment carrier

**The scout's probe used the wrong shape.** It stood in `X int32` for S22's real tail. Re-probed with S22's real shape,
`struct{ FforceAlignment [0]int16; Fa [201]int8; F__ccgo_pad2 [1]byte }`. In Go it is size 202 and alignment 2, and
nested after a byte (`struct{ B byte; S S22 }`) it sits at offset 2 with outer size 204.

| CLR form of S22 | S22 size | S nested after a byte | outer size |
|:--|--:|--:|--:|
| sequential, `[0]int16` dropped | 202 | 1 | 203 |
| explicit at Go's offsets with `Size = 202`, no carrier | 202 | **1** | **203** |
| explicit, `[0]int16` carried as `[FieldOffset(0)] readonly short` | 202 | 2 | 204 |

- **Explicit layout alone does not fix it.** Go's offsets and Go's Size give Go's size and offsets for S22 itself, but
  not Go's alignment, so a CLR container still places S22 at offset 1. Only the carrier restores the alignment.
  - The carrier makes `[2]S22` 404 bytes, Go's size, and the struct stays unmanaged.
- **"Explicit offsets transitively"** (the review's alternative) would fix only the containers go2cs emits. A `T[]`,
  a `ж<T>` payload or a generic container that go2cs does not lay out would still use the CLR's alignment of 1.
- **The carrier is a primitive of the element's ALIGNMENT, not the element itself.**
  - `struct{ A byte; _ [0][2]int64 }` is 16 bytes in Go. Carrying the element (`[2]int64` at offset 8) makes it 24 in
    the CLR; carrying a `long` makes it 16.
  - The same rule gives Go's trailing padding: syscall's `InotifyEvent` (`Name [0]uint8` last) is 20 in Go and 16
    without a carrier. A `byte` carrier at offset 16, rounded to alignment 4, is 20 (derived, not probed).
  - A mid-struct carrier overlapping the next field (`struct{ A byte; Z [0]int32; B byte }`, B at 4, size 8) also
    reads Go's layout.
- **The carrier SHARES bytes with real fields.** After `Fa[0] = 5` and `Fa[1] = 6`, the readonly carrier reads 1541.
  The fix fits the existing zero-size-field arc (`src/go2cs/zeroSizeFieldLayout.go`, the readonly rule and its
  accessor), and Go never reads a `[0]T`. But equality, hashing, the image encoder and reflect's field walk must all
  SKIP it. `ClrLayoutMismatch` already skips zero-size fields (`src/core/golib/GoReflect.TypeLayout.cs:514`).
- **The population, from the r2 census:**
  - **lib's 72 whole-converted struct types:** 35 carry an alignment-forcing `[0]T`. Without carriers, 12 lay out
    differently from Go:
    - 10 in alignment, so they are wrong whenever nested. S22 is one, and so are `TSrcList`, `TExprList`, `TIdList`,
      `TWalIterator` and `TYYMINORTYPE`.
    - 2 in size: `TyyParser` reads 1,032 against Go's 1,232, and `Tfts5yyParser` 2,032 against 2,432.
  - **Over every lib struct type expression:** 93 of 148 carry one, 39 differ and 37 differ in alignment.
  - **std's 264 declaration-local candidates:** 2 carry one, and 5 differ, all in size from trailing zero-size
    fields.

#### 7.6.3 Item 3 (MAJOR): the layout gate must be able to fail

**Today's gate can pass without checking anything.** `GoReflect.ClrLayoutMismatch` answers null ("no mismatch") in
three cases where the layout is unproven:
- when `GoFieldOffsets` is null (`src/core/golib/GoReflect.TypeLayout.cs:495`);
- when the struct holds a bool or a char at any depth, because `Marshal.OffsetOf` then reads the marshaled layout (:503);
- when the CLR offset is unreadable (:530).

The review cited :494, :503 and :527; those are the lines as they read at `130cecb136`.

It also never compares size or alignment.

**The S22 measurement shows why offsets and size are not enough.** S22 without a carrier has Go's size (202) and
Go's offsets (0 and 201) and is still wrong. Only ALIGNMENT shows the defect.

**Required, for the I0 cut:**
- **The gate asserts four things for every admitted type at EVERY nesting level:**
  - `GoFieldOffsets` is non-null;
  - each CLR field offset equals Go's;
  - `Unsafe.SizeOf` equals `GoSizeOf`;
  - the CLR alignment equals `GoAlignOf`. The CLR alignment is measured as the offset of `T` in a generic
    `struct{ byte; T }`; the composed-family probe shows generic sequential offsets are honored for unmanaged
    elements.
- **The gate reads MANAGED offsets.** For an inline field `Marshal.OffsetOf` happens to agree: 201, 2, 204 and 64 in
  the probe. But a bool anywhere sends it to the marshaled layout. So the CLR offsets come from `Unsafe.ByteOffset`
  over field references, in a per-type table go2cs-gen emits, which needs no reflection and works under AOT. The
  population this matters for: lib's 72 hold no bool; std's 264 candidates include 44 that do.
- **The deliberately broken control is S22 WITHOUT its carrier** (size and offsets right, alignment wrong). The gate
  must name it before any admitted type's green counts. A second control is a type whose `GoFieldOffsets` is null,
  which must fail, never pass.
- **`GoFields` and `ArrayDims` learn the inline lengths first.** Today an array field's Go size comes from the
  `[GoArrayDims]` stamp beside `array<T>`. An I0 field carries its length in its TYPE, so `TryGoSizeOf` must read it
  from there, or the offsets stay null and the gate checks nothing again.

#### 7.6.4 Item 4 (MAJOR): a length golib lacks is refused, unless the owner picks the composed family

**The scout's fallback is withdrawn.** "A length golib does not ship stays `array<T>`" made a public field's TYPE
depend on the golib version. Two options remain for the owner:

**(a) Named family with refusal.**
- golib ships `go.array<N><T>` for a fixed set of lengths, and converted code reads `array201<int8> Fa`.
- A length outside the set is a convert-time REFUSAL naming the length and the field. Adding a length is a golib
  tier-2 change, and never changes an existing field's type.
- **Cost:** the owner principle "nothing is left out for being unreached" is not met for a module whose length golib
  lacks. That module's structs refuse until a golib release adds the length.

**(b) Composed family.**
- golib ships only power-of-two blocks, and any N is their composition. 201 = 128 + 64 + 8 + 1 is
  `Cat<Cat<Arr128<T>, Arr64<T>>, Cat<Arr8<T>, Arr1<T>>>`, built from golib types alone. So two assemblies' `[201]T`
  are one C# type with no golib growth.
- **Measured contiguous** for `sbyte`, `long`, a padded 16-byte struct and `string`: the blocks sit at
  0/128/192/200 elements, and the total size is 201 elements in each case.
- **Readability** is recovered by a generated `global using` alias per (N, element) that the assembly uses
  (`array201_int8`). An alias cannot be generic, so generic code over `[N]T` shows the composed spelling.
- **Two consequences:**
  - `Cat<A, B>` is not itself an `[InlineArray]`, so it has no language Span conversion, which item 1 requires.
  - Its indexer dispatches BY BLOCK (`i < A.Length ? First[i] : Second[i - A.Length]`) rather than by flat
    arithmetic. The `string` case laid out in order here, but the CLR gives no layout guarantee for reference-bearing
    structs, and dispatching by block holds under any layout.
- **NOT MEASURED:** the indexer's cost through a composed type.

**Recommendation: (b).** It is the only option that converts every length with no golib release in the loop, and the
alias keeps the source readable.

#### 7.6.5 Item 5 (MAJOR): equality is generated; the default is order-dependent and WRONG, not only throwing

The default equality of an inline array, and of a struct holding one, measured:

| call | result |
|:--|:--|
| `Equals` on the inline array | `NotSupportedException` ("Calling built-in Equals() or GetHashCode() on type marked as InlineArray is invalid.") |
| `GetHashCode` on the inline array | the same exception |
| `Equals` on a holder, as the FIRST equality use in the process | `NotSupportedException`, and again on a second call |
| `Equals` on a holder after a `Dictionary` lookup has run first | **True**, for two holders whose inline arrays DIFFER (`A[2]` 0 against 7) |
| `Dictionary<Holder, int>` lookup, in either order | **True/True**: an unequal key is found |
| `GetHashCode` on a holder | returns, the same value for both unequal holders |

The wrong answers reproduced in both runs of `probe-eq-output.txt`. The mechanism is NOT ROOTED: some runtime state the
first path leaves behind changes what the second path does. For Go this is a silent wrong `==` and a silent wrong map
key, so equality cannot be left to the default.

**Required:**
- **The family implements equality itself:** `IGoEquatable`, `IEquatable`, `Equals(object)` and `GetHashCode`. The
  probe's `Arr4Eq<T>` does this with a span compare.
- **Every holder gets generated equality**, used for Go `==` and for map keys. A holder that keeps the default but
  holds an equality-implementing member answered correctly here (False for unequal holders). That runs through
  reflection field by field, and given the row above, I0 does not rely on it.
- **Equality skips the `[0]T` carriers** (7.6.2).

#### 7.6.6 Item 6 (MINOR): the M:N collision is CS8178 on a compound assignment

`probe-await-build.txt`, for an inline array in an async method:

| form | result |
|:--|:--|
| `p.a[0] += await F()`, `p` a local | CS8178 (the element reference cannot be preserved across `await`) |
| the same on a class field (a heap owner) | CS8178 |
| `ref int e = ref p.a[0]; await F(); e = 1;` | CS9217 |
| `Span<int> s = p.a; await F(); s[0] = 1;` | CS4007 |
| `p.a[0] = await F()` | compiles, and runs correctly (`probe-r2-output.txt`) |
| `p.a[await F()] += 1` | compiles |

The scout's 7.5 named CS4007 for the general case; the review's CS8178 is the one an ordinary Go statement reaches.

**The twin emitter's rule:**
- A compound assignment whose right side contains an `await` is split. The index operands are spilled to temps in Go's
  order, then `var t = await …; p.a[i] += t;`.
- Measured: the split form compiles and gives `a[1] == 6` from 5 + 1.
- Go's spec leaves the order between reading `p.a[i]` and calling `F()` unspecified. Reading after the call is the
  order the split produces.

#### 7.6.7 Item 7 (MINOR): I2's backing, shown across a real move

The scout printed offset 0 and did not show that the owner MOVED. Re-probed:
- the owner is a fresh gen-0 box allocated between two blocks of garbage, then put through three compacting
  collections;
- the owner went from generation 0 to 2, and its address moved by 37,324,912 bytes in the committed run (the distance differs run to run);
- **negative control:** a PINNED box of the same type, through the same collections, moved by 0, so the address read
  is real;
- through the moved owner, (owner, offset) read 42 and wrote 43, and the write is seen through the box. Offset 0 is
  correct: it is `Fa`'s offset inside the box's data, and `Fa` is the first field;
- **negative control:** boxing a COPY of a by-value local at slicing time and writing through (that box, offset) leaves
  the local at 42. The write is lost.

**Rules for I2:**
- The owner is never a box minted at slicing time. A sliced local is boxed at its declaration, on the
  address-taken-local path, or it is not sliced.
- For a struct held by a native box (`NativeBox`), the slice's backing is the existing native base
  (`src/core/golib/slice.cs:253`) plus the field's Go offset, not an owner. NOT MEASURED; it composes with
  DESIGN-native-backed-slice.
- **I3 (managed elements) is (owner, offset) only, never layout-equal.** Agreed with the review: a reference-bearing
  struct has no Go layout in the CLR to be equal to.

#### 7.6.8 Item 8 (MINOR): the census record, re-derived

- **Distinct lengths:** the r2 outputs carry the lines, so the scout's figures now reproduce from committed files.
  - std: 306 admitted fields over 46 lengths, and 80 lengths over all array fields.
  - lib: 103 admitted fields over 32 lengths.
  - Under 7.6.1 these are the old predicate's figures, kept for the record. Admission is now per declaration.
- **"Would admit all 72" is now derived, not assumed.** Each type is walked through every field at every depth.
  - lib: all 72 whole-converted types hold an array, are all-unmanaged once inline, and are within the caps. None holds
    a bool. 35 need a `[0]T` carrier, and 12 would be wrong without one (7.6.2).
  - Over every lib struct type expression, 148 hold an array and all 148 qualify under 7.6.1's rule.
  - std, for comparison: of its 66 whole-converted types, 40 are all-unmanaged once inline and 38 are within the caps.
    The scout's table gave 66 without making that split.

#### 7.6.9 Item 9 (MINOR): the cgo mirrors, and a by-value cap

- **The cgo mirrors already emit inline arrays.** `src/go2cs/cgoMirrors.go:344` writes
  `[InlineArray(N)] internal struct <name> { private <elem> m_element; }`, one non-generic internal helper per
  (N, element) per assembly. The two families serve different purposes:
  - a mirror never crosses an assembly boundary and holds C's layout;
  - the I0 family is Go's type, shared by every assembly.

  Under (b) the mirrors move onto the composed family in the I0 cut, so one family remains. Under (a) a mirror needs
  any N, so the mirrors keep their helpers, and this page records the two families and why.
- **A by-value cap.** Go grows a goroutine's stack; a CLR thread's stack is fixed (C46 reserved 1 GB for the main
  goroutine only).
  - A by-value local or parameter of an I0 type larger than a cap is heap-boxed by the converter, on the existing
    address-taken-local path, so the frame holds a reference.
  - The proposed cap is 1 KiB per local or parameter. It is an owner ruling with the others.
  - NOT MEASURED: the stack size of a non-main goroutine's thread, and recursion depth under that cap.

#### 7.6.10 The ladder as revised, and the owner rulings

- **I0. Layout-equal structs in opted-in packages.**
  - Admission is 7.6.1's declaration-local rule.
  - Emission: the wrapper type with an indexer only, `[0]T` as an alignment carrier, and generated equality.
  - It is gated by 7.6.3's four-way check with its broken control.
  - Its first target is lib's 72 whole-converted types, S22 among them.
  - The rest of 7.5's I0 predictions stand, except std, which is out until I2.
- **I1.** Unchanged, except for the await rule: per 7.6.6, the twin emitter splits compound assignments, where 7.5
  said it re-reads elements.
- **I2.** As in 7.5, with 7.6.7's rules. The slice-width or pinning cost is still posted and ruled before any cut.
  std's opt-in comes here.
- **I3.** (owner, offset) only.

**Owner rulings needed before I0 (COORD has put the first three to the owner):**
1. **Home and family:**
   - (a) a named golib family with convert-time refusal of an unshipped length; or
   - (b) the composed family from golib blocks, with a per-assembly alias for each (N, element).
   - Recommended: (b).
2. **The visible name:** `array201<int8>` under (a), or the alias `array201_int8` under (b).
3. **The caps:** 4 KiB per array and 64 KiB per struct (as 7.5), plus 1 KiB per by-value local or parameter (7.6.9).
4. **For COORD: the opt-in mechanism** (a converter flag, or a property in the converted project) and the first
   package list: modernc.org/sqlite/lib.

### 7.7 I0 finalized under the owner's rulings, with the family's measured cost (dated block, 2026-11-13, C1)

> **Status: DESIGN, I0 FINAL pending the two questions in 7.7.5.** The owner ruled on 7.6.10 (COORD, inbox C1
> 20261113T010503Z), and COORD ordered a cost measurement before any code. This block records the rulings, the
> measurement, I0 as it will be cut, its predictions and its stop lines. The evidence is in
> `docs/phase4/data/inlinearray-scout/family/`: its README holds every reading and the sources. The census under the
> ruled caps is `census-r3-*`. NO CODE: COORD routes the build to a lane with disk, and C1 reviews it.

#### 7.7.1 The rulings

1. **Family (a).** golib's build GENERATES `array1<T>` … `arrayN<T>` up to the per-array cap, generic over the
   element. A length past the cap is refused at convert time, by name, with no fallback. Converted code reads
   `internal array201<int8> Fa;`.
2. **Namespace.** The family lives in its own namespace, marked `EditorBrowsable(Never)`, and converted projects
   reach it through the global using the converter already writes. Proposed name: **`go.arrays`**.
3. **Caps, FIXED for now:**
   - 4 KiB per array and 4 KiB per struct, until the null-fault distance is measured on linux and darwin;
   - over 1 KiB held by value goes to the heap.
   - Making them configurable comes later, within proven bounds.
4. **Opt-in** (COORD): `-inline-arrays=<import-path patterns>`, empty by default. The first list is
   modernc.org/sqlite/lib alone, and std stays out until I2.

#### 7.7.2 The measurement: "they only grow by usage"

**The setup.** The family was generated at three sizes into a standalone library:
- **one:** length 201;
- **dense:** 1-256 plus the census lengths over 256, 264 lengths;
- **full:** 1-4096, every length the 4 KiB cap admits for a 1-byte element.

Each length is one public wrapper (indexer, `Length`, Go equality, no Span) over a private `[InlineArray(N)]`
storage. The baseline is the published `golib.dll` of go.lib 1.26.10.2: 1,164,800 bytes. golib itself cannot be
built on this box, so the family was measured as its own assembly. Its size and load cost are what it would add.

| | one | dense (264) | full (4096) |
|:--|--:|--:|--:|
| library size, `where T : unmanaged` | 6,656 | 149,504 | 2,616,320 (2.2 x golib.dll) |
| library build, no incremental | 1.2 s | 1.6 s | 8.5 s |
| JIT: first use of `array201<sbyte>` (assembly, type, JIT), median of 30 | 1.29 ms | 1.42 ms | 2.74 ms |
| JIT: RSS, median of 30 | 24.8 MB | 25.0 MB | 26.5 MB |
| JIT: process wall, median of 30 | 29.7 ms | 29.0 ms | 29.8 ms |
| Native AOT, go2cs's `TrimMode=partial` (and the SDK's full trim) | 1,325,056 | 1,325,056 | 1,325,056 |
| Native AOT, the library ROOTED, unconstrained `T` | 1,572,400 | 7,986,736 | link failed |
| Native AOT, the library ROOTED, `where T : unmanaged` | not run | 1,584,688 | 2,301,488 |

The no-family control (`sbyte[]`) is 1,325,072 bytes under Native AOT.

**What the owner's assumption gets right and wrong.**
- **Native AOT, as go2cs publishes it: TRUE.** Unused lengths cost 0 bytes. The three binaries are the same size and
  differ only in 40 bytes of build identifiers.
  - A control proves the publish could have shown a cost. Under `TrimMode=partial`, ILC 10.0.14 does NOT root a
    referenced non-trimmable library: an unreferenced non-generic method in it is not compiled (0 map entries).
  - With `TrimmerRootAssembly` it is compiled: 5 entries, and 65,536 bytes over the rooted one-length library.
- **The library: FALSE.** The DLL carries every definition. The full family is 2.6 MB, and a JIT process that touches
  it pays about 1.45 ms more on first use and about 1.8 MB more RSS. The process wall medians did not separate at
  this resolution.
- **A configuration that ROOTS the library: FALSE, by a wide margin, unless `T` is constrained.**
  - Rooted, every length compiles over `System.__Canon`, at about 140 bytes per ELEMENT (array1024 144,596 bytes,
    array4096 574,302). That is 6.4 MB for the dense set.
  - For the full set the object file was 1.2 GB and the link ran out of this box's disk. That is a box limit, not a
    measured ceiling.
  - `where T : unmanaged` removes every `__Canon` instantiation, and the rooted full family is then 2,301,488 bytes.
  - golib's own guidance names a trimmer root as the alternative to `TrimMode=partial` (`src/core/golib/GoFieldMetadata.cs:83`), so
    a rooted golib is a supported configuration, and the constraint is required.
- **A side observation, NOT CHASED.** golib's comments say `TrimMode=partial` keeps golib "whole" (`src/core/golib/GoFieldMetadata.cs:34`).
  For Native AOT, the control above shows that partial does not root a referenced non-trimmable assembly's
  unreferenced code. Whether that matters to golib (its reflection reads METADATA, which `--scanreflection` may keep)
  is not measured here. It is reported to COORD, not acted on.

#### 7.7.3 The shape proposed

- **The FULL family, 1-4096, `where T : unmanaged`.**
  - The constraint matches I0, which admits only unmanaged elements, and it is what keeps a rooted publish small.
  - I3's managed elements will need a separate decision when I3 comes. The constraint keeps that decision out of I0.
  - A length is refused only past the cap, which the ruling refuses anyway. So the family itself refuses NOTHING
    extra, which keeps the owner's "nothing is left out".
- **In its own assembly, `go.arrays.dll`, shipped in the go.lib package and built by golib's build.**
  - The CLR loads an assembly on first use. So a program that never opts in pays nothing: not the 2.6 MB of
    metadata, the 1.45 ms or the 1.8 MB. (Standard CLR behaviour; NOT MEASURED here.)
  - Inside `golib.dll` every go2cs program would carry it, because golib is always loaded. Whether the load cost is
    the same inside a larger assembly is NOT MEASURED.
  - Identity across assemblies is unaffected: there is still exactly one defining assembly.
- **The cheaper alternative, if the owner wants golib's package smaller.**
  - Dense 1-256 plus the census lengths: 149,504 bytes.
  - Refusals: 0 over lib.
  - For std, which stays out until I2, every length the old predicate admitted is either in the set or over the cap.
    std's full list of field lengths was not checked against the set.
  - A third-party module with a length in 257-4096 outside the set is refused by name until a golib release adds it.
  - Adding a length turns a refusal into a conversion and never changes an existing field's type.

#### 7.7.4 I0, as it will be cut

1. **The converter.**
   - **Option.** `-inline-arrays=<import-path patterns>`, empty by default. It is recorded with the conversion, so a
     shipping conversion and a `-tests` conversion read the same list.
   - **Admission.** In a matching package, a struct DECLARATION is admitted when:
     - every field is unmanaged once its arrays are inline;
     - every array is no larger than 4,096 bytes and no longer than 4,096 elements;
     - every struct at every level is no larger than 4,096 bytes.

     A struct that fails any rule stays exactly as today. That depends only on the declaration and the ruled caps,
     never on golib's version.
   - **Emission:**
     - an admitted `[N]T` field becomes `go.arrays.array<N><T>`;
     - a `[0]T` becomes a readonly alignment carrier at its Go offset (7.6.2), under the existing explicit layout;
     - a whole-value read is an explicit copy;
     - a compound assignment with an `await` on its right side is split (7.6.6).
   - **The by-value cap.** A local or parameter of an admitted type over 1 KiB is boxed on the address-taken-local
     path. In lib that is 3 of the 72 whole-converted types, and 4 of the 148 declarations.
   - **Refusals** are convert-time errors naming the field and the cap.
2. **golib (or `go.arrays.dll`, 7.7.5).**
   - The generated family: `EditorBrowsable(Never)`, `where T : unmanaged`, no Span, slice or address face.
   - Equality through `IGoEquatable`, which skips nothing (a family member holds no carrier).
   - GoReflect learns that a `go.arrays` type is a Go array of N: `GoFields`, `ArrayDims` and `TryGoSizeOf`.
   - Reflection reads and writes an inline field through (owner, Go offset), or panics naming the field.
3. **go2cs-gen.**
   - Generated equality for every holder of an admitted array, skipping carriers.
   - A per-type managed field-offset table (`Unsafe.ByteOffset`) for the gate.
4. **The gate.**
   - It runs for every admitted type at every level, and checks four things: `GoFieldOffsets` non-null, offsets
     equal, `Unsafe.SizeOf == GoSizeOf`, and alignment equal.
   - **Broken control:** S22 without its carrier must be NAMED. A second control: a type with null offsets fails.
   - **The default-equality trap** of 7.6.5 is a GolibTests case: a holder's `==` and map key must answer unequal
     for unequal arrays in both call orders.

**Predictions** (scored against the cut, as worded):
- **P1. lib converts with no refusal.** With `-inline-arrays=modernc.org/sqlite/lib`:
  - all 148 of lib's array-holding struct types (named, and anonymous struct type expressions) are admitted, and 0
    are refused;
  - the 103 nonzero array fields become `array<N><T>` over 32 lengths;
  - the 126 zero-length fields become carriers.
- **P2. The gate passes lib's 72 whole-converted types and fails the right controls.**
  - All 72 pass all four checks.
  - With carriers removed, 12 fail: 10 on alignment, and `TyyParser` and `Tfts5yyParser` on size.
  - The S22 control is named.
- **P3. S22 stops refusing.** S22's whole-struct read through the native box is admitted (`ClrLayoutMismatch` null
  with non-null offsets), and sqlite-cs's next start-up wall is S1, the function pointers.
- **P4. Nothing changes for anyone who does not opt in.** With the option empty, a two-seeded three-target `-stdlib`
  reconvert reads ZERO changed files, and the behavioral goldens do not move.
- **P5. AOT is unchanged.** A go2cs Native AOT hello-world's size with the family shipped equals its size without it,
  apart from build identifiers.
- **P6. The rooted cost holds.** A rooted Native AOT publish of the full constrained family stays within 2.4 MB
  (measured 2,301,488).

**Stop lines:**
- **S1.** Any corpus or golden change with the option empty.
- **S2.** The gate passes S22 without its carrier, or passes a type with null offsets.
- **S3.** The family loses `where T : unmanaged`, or a rooted AOT publish of it exceeds P6.
- **S4.** An unanalysed face compiles to a silent copy instead of a C# error.
- **S5.** A holder reaches the default equality.
- **S6.** A cap is raised before the null-fault distance is measured on linux and darwin (the owner's ruling).

#### 7.7.5 Two questions before the cut

1. **For the owner, through COORD: is `go.arrays.dll` beside `golib.dll` in the go.lib package acceptable?** The
   ruling reads "golib's own build generates", and a second assembly from that build keeps the ruling's name and
   namespace. If not, the family goes into `golib.dll`, and every program carries 2.6 MB more there.
2. **For COORD: the side observation in 7.7.2** (`TrimMode=partial` does not root golib under Native AOT on ILC
   10.0.14), as a BOARD line or a separate check.

> **Amended 2026-11-13 (C1), on COORD's relay of the owner's rulings** (inbox C1 20261113T012200Z):
> - **7.7.5 question 1 is RULED.** The family is `go.arrays.dll`, its own assembly in the go.lib package, built by
>   golib's build. It holds 1-4096 lengths, `where T : unmanaged`, in namespace `go.arrays`.
> - **7.7 is ACCEPTED as the I0 cut**, with its six predictions and six stop lines as written.
> - **Implementation goes to C2; C1 reviews** for probe parity, the carriers, the gate's broken control and the
>   generated equality.
> - **Prediction P7, added before the cut and measured in it:** a program that does not opt in never loads
>   `go.arrays.dll`. The measurement is a module-load trace of a non-opted program, which must show no load.
> - **7.7.5 question 2** is a BOARD line, "under Native AOT, ILC 10.0.14's `TrimMode=partial` does not compile a
>   referenced non-trimmable library's unreferenced code", routed by COORD.

#### 7.7.6 Can admission be auto-detected? (dated block, 2026-11-13, C1)

> **The owner's ask, through COORD** (inbox C1 20261113T034712Z). Instead of the `-inline-arrays` opt-in, could I0
> admit a struct type when its OWN declaring package, in its NON-TEST files, converts a value of it whole through
> `unsafe.Pointer`, with the option kept as an override? This block is a read-only census of that rule, and a
> recommendation.
>
> **Evidence:** `docs/phase4/data/inlinearray-scout/auto/`. `census-r4-auto.go` is the tool's `-auto` mode.
> - `auto-sqlite-lib-libc.txt` covers modernc.org/sqlite v1.62.0 (`lib` and the `sqlite` package) and every package
>   of modernc.org/libc v1.80.1.
> - `auto-std.txt` covers std, Go 1.26.10, with test files.
> - Both are linux/amd64 under the ruled caps (4 KiB per array and per struct).
>
> C2's I0-lib parts A to C do not depend on this; only part D does.

**What the census counts.**
- **Sites.** Each `(*X)(p)` with `p` an `unsafe.Pointer`, or `unsafe.Pointer(e)` with `e` a `*X`.
- **What a site needs inline.** The types that must be inline for `X` to be layout-equal: every struct type
  reachable from `X` BY VALUE (X itself, array elements, struct fields, at any depth) that holds an array.
- **Where each type is converted.** Each such type is recorded against the package that converts it, split into test
  and non-test files.
- **Anonymous struct types** (ccgo writes many of its types as aliases of them) are keyed by their type string. They
  count as in-package when a package that spells them converts them; go2cs lifts each one into the package that
  writes it.

**1. lib's 72, and the closure.**
- **The 72 reconcile exactly.** lib's 72 whole-converted types are the conversion TARGETS that hold an array
  directly. Every one is converted in lib's own non-test files.
- **The rule must take the CLOSURE**, not only the targets:
  - 25 more targets hold their arrays only in nested structs;
  - 23 types are reached only by nesting.

  Inline storage for a target is layout-equal only if those are inline too. Keyed by type, lib's auto set is
  **118 types**, all converted in lib's own non-test files.
- **Converted only in another package's non-test files: 0** in lib, sqlite and libc. **Only in test files: 0.**

**2. Does auto admit anything the opt-in would not?** No. Over every package read, auto admits 0 types outside the
opt-in's population. Auto is a SUBSET of the opt-in:

| package | the opt-in admits | auto admits of them | the difference |
|:--|--:|--:|:--|
| modernc.org/sqlite/lib | 130 | 118 | 12 types never converted whole, nor nested in a type that is |
| modernc.org/libc | 175 | 98 | 77 |
| libc's subpackages (`libc/netdb`, `libc/signal`, `libc/sys/...`, 19 packages) | 113 | 1 | 112: they declare C types for transpiled programs, which convert them |
| std, for I2 | 189 | 53 | 136 |

- **The 130 is keyed by type.** 7.7's P1 counted lib's 148 by struct-expression POSITION, where identical anonymous
  structs count once per spelling.
- **libc reads 99 admitted** by the auto tally and 98 by this comparison. One anonymous type is attributed to
  different packages by the two passes; NOT RESOLVED.
- **Over the caps:** 5 libc types are converted whole in libc but exceed the caps, so they stay as today.
  - Two are crypt_blowfish's `BF` key tables: `[4][256]TBF_word`, over 4 KiB.
  - Two are musl logarithm data tables: `tab [128]`, plus their polynomial arrays.
  - One is a netlink buffer, `[8192]Tuint8_t`.

**3. libc and std, for I2.**
- **libc:** 104 types need inline storage; all are converted in libc's own non-test files; 99 are admitted and 5
  are over the caps.
- **std**, over its 12 declaring packages: 110 need inline storage.
  - 96 are converted in their own non-test files: 53 admitted, 40 managed, 3 over the caps.
  - **6 are converted only in another package:** runtime converts `internal/abi.ITab`, `.InterfaceSwitchCache`,
    `.TypeAssertCache`, `internal/chacha8rand.State` and `internal/cpu.CacheLinePad`, and sync converts
    `sync/atomic.Pointer[poolChainElt]`.
  - 8 are converted only in tests.
  - So std's runtime-internal packages are where the own-package rule misses. std stays out of I0 under either
    trigger.

**4. Soundness: what the rule cannot see, and whether a miss is loud.**
- **A type the rule misses keeps today's emission exactly** (`array<T>` fields). So a miss reproduces today's
  behaviour, never a new layout. Native memory read as such a struct is refused by name, which is S22's refusal
  (`src/core/golib/runtime/RuntimeErrorPanic.cs:192`). A managed pointer round trip has no layout question.
- **Generic conversions** (a target holding a type parameter, so the instantiation chooses the type):
  - lib, sqlite and libc have 1, `sqlite.go:1223`'s `cFuncPointer[T]`. It reads a function value as a `uintptr`,
    which is S1's wall, and it holds no array at any instantiation.
  - std has 84 in non-test files: `atomic.Pointer[T]`, `abi.Escape` and kin. They round-trip a pointer that was
    always a `*T`, which needs no layout equality.
  - A generic reinterpretation of an array-holding struct would be missed, and would meet today's behaviour, loudly
    for native memory.
- **Function values:** not a blind spot. A conversion is an expression in some function body, and the census reads
  every body in the packages it loads. A func value moves where the CALL happens, not where the conversion is
  spelled. So a conversion in a func literal is read in its own package, and one in another package's function is
  counted as converted in that package.
- **`reflect.NewAt`:** 0 calls in all three corpora.
- **The one NEW hazard is a MIXED reinterpret chain**: `(*T)(unsafe.Pointer(&x))` where one side is admitted and
  the other is not. Inline bytes read as a struct that still holds an `array<T>` reference is not a case today's
  paths were built for.
  - **lib, sqlite and libc:** 1,863 chains, 0 mixed.
    - 1 has both sides admitted: S22's.
    - In 914 only one side holds an array (a struct read from or written as bytes or scalars). The struct side is
      admitted in 908 of those; the other 6 are libc's over-cap tables, which stay as today.
  - **std:** 460 chains, **7 mixed**, all in the runtime's GC work buffers: `workbuf` against `stackWorkBuf` and
    `stackObjectBuf`, where one side holds pointers.
  - The hazard is NOT specific to auto. 7.7's declaration-local opt-in splits those pairs the same way. So I0 needs a
    pair rule under either trigger.

**Recommendation: AUTO, with `-inline-arrays` as an override, on four conditions.**
1. **Admission is the CLOSURE.** It covers every struct reachable by value from a type the declaring package's
   non-test files convert whole, and each type must pass 7.7's declaration-local rules and caps. Admitting only the
   conversion targets would admit S22 and leave 48 of lib's 118 types behind it.
2. **A pair rule.** A reinterpret chain whose two sides disagree on admission is a convert-time error naming the
   chain and both types. It is never emitted, so a mixed pair cannot reach run time. lib reads 0 such chains, so the
   rule costs lib nothing.
3. **The decision is stamped in `package_info` (Q4).** A `-tests` conversion reads the stamp; it never re-derives the
   decision from `_test.go` files. The census shows why: std has 8 types converted only in tests, which must not
   change a shipping layout.
4. **The override covers what auto cannot see.**
   - `-inline-arrays=<patterns>` forces packages ON with the opt-in's full declaration-local population. That is
     libc's subpackages (112 types that only their consumers convert), and any package whose reinterpretation is
     generic.
   - A `-` form turns auto off for a package.
   - std stays out at I0 whatever the option says.

**What this changes in 7.7, if ruled:**
- **P1 becomes:** with auto on and no override, lib admits **118** types (type-keyed) with 0 refusals, S22 among
  them. With `-inline-arrays=modernc.org/sqlite/lib`, it admits the 130 of the opt-in population.
- **A new stop line, S7:** a mixed reinterpret chain is emitted instead of refused.
- **Its P2 control:** the S22 shape with only one side admitted must be refused at convert time, by name.

> **Amended 2026-11-13 (C1), recording two COORD rulings:**
> - **7.7.6 is RULED as recommended** (inbox C1 20261113T040024Z). Admission is AUTO-DETECTED on its four
>   conditions:
>   - the closure;
>   - the pair rule, as a convert-time error, `GO2CS2010`;
>   - the `package_info` stamp;
>   - `-inline-arrays` as an override, with a `-` form that turns auto off.
>
>   std stays out at I0. Part D's admission is reviewed against census-r4: lib 118 under auto, 130 with the
>   override.
> - **P6 is scored AS WORDED, and it is MISSED** (inbox C1 20261113T052540Z): C2 measured a rooted publish of
>   17,793,904 bytes against the 2.4 MB bound. The prediction stays as written. What it meant: the 2.4 MB was my
>   standalone probe's TOTAL, a 1,325,056-byte app plus a family INCREMENT of 976,432 bytes (7.7.2). The shipped
>   family needs golib, about 16.9 MB of every go2cs Native AOT binary, which the bound never included. Read as the
>   increment, C2's rooted reading adds 860,160 bytes, under the 976,432 measured in 7.7.2. That reading is recorded
>   beside the score, not in place of it.

> **Amended 2026-11-13 (C2), part D: admission and its record** (COORD's rulings of 04:00Z, 05:20Z and 06:19Z):
> - **Admission is automatic; the option overrides it.** A struct is admitted when its own package's non-test files
>   convert it whole through `unsafe.Pointer`, together with every struct it holds by value, plain ones included (C1's
>   review, N4). `-inline-arrays=<patterns>` forces a package's whole declaration-local population of 7.7.4 on, and
>   `-<pattern>` turns the automatic rule off. std is never admitted. Converter: `inlineArrayAdmission.go`.
> - **7.7.1 and 7.7.4 amended: a cap is a WARNING, not a refusal.** A type the rule selects that a cap keeps out
>   converts exactly as today with `GO2CS2009`, naming the type, the field and the cap. libc's over-cap types convert
>   with one warning per selected seed (4). The convert-time ERROR is `GO2CS2010`: a reinterpret chain
>   `(*T)(unsafe.Pointer(e))` whose two array-holding sides disagree. `-warn` cannot downgrade it.
> - **P1 as measured** (66d9772a86, linux/amd64, modernc.org/sqlite v1.62.0, libc v1.80.1): lib admits 158 types
>   (120 holding arrays) automatically and 185 (144) forced; libc 123 (99) with 4 `GO2CS2009`; the sqlite package
>   0; `GO2CS2010` 0 everywhere; fmt `%v` and reflect uses reaching an admitted type 0. Against its prediction
>   (mailbox 921b72106) 5 lines MET and 3 MISSED as worded: 118 and 130 were C1's combined lib+libc run, which files an
>   anonymous struct both packages spell under libc (C1's tool over lib alone reads 120 and 144); libc's 5 counted
>   types where the warning counts seeds (the two BF tables are one seed).
> - **P4 restated:** with the option empty, a two-seeded three-target std reconvert reads 0 files, and the behavioral
>   corpus moves ONLY in FieldAccessReinterpret, GoImageCopyThroughView and PointerFieldSelectors, the automatic rule's
>   whole behavioral footprint; every other project reads 0.
> - **The record.** `package_info.cs` carries an `<InlineArrays>` section, only when there is a record: one
>   `[assembly: go.golib.GoInlineArrayType("<Go name>")]` per admitted named type, and
>   `[assembly: go.golib.GoInlineArrays("forced" | "off")]` when the rule was not the default. A `-tests` conversion
>   applies the recorded rule to the production files and refuses a record they do not reproduce. A reinterpret chain
>   to another converted package's named struct is judged by that package's record; one reaching another package's
>   anonymous struct, or a package whose record cannot be read, is counted, not judged. Converter:
>   `inlineArrayStamp.go`.
> - **OWED (deferred by COORD, 06:19Z):** laying out a struct of ANOTHER package that an admitted type holds by value,
>   from that package's own record. Until then such a seed stays out with `GO2CS2009`; lib and libc hold 0.
> - **Faces (COORD 06:50Z, ruling (a)).** The automatic rule also keeps a seed out when an array field in its closure is
>   used, in the package's non-test files, in a way an inline array cannot express: sliced, its address or an element's
>   address taken, or used as a whole value other than boxed into an interface. Index, `len`/`cap`, `range`,
>   `unsafe.Offsetof`/`Sizeof`/`Alignof` and boxing stay admissible. A copy between two admitted fields is also kept out at I0
>   (0 sites in lib and in the behavioral corpus), so the rule does not depend on which other types are admitted. Forced
>   mode does not consult faces. Measured: lib 158/120 and forced 185/144, libc 123/99 with 4 `GO2CS2009`, sqlite 0, all
>   unchanged; behavioral, FieldAccessReinterpret keeps `reparse` only (`record` slices `Path`, and `tail` was admitted only
>   through `record`), GoImageCopyThroughView keeps `Dirent` and `Pair`, PointerFieldSelectors keeps `wire`. Converter:
>   `inlineArrayFaces.go`.
