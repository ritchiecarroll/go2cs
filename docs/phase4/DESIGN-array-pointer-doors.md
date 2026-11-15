# DESIGN: a slice or an element taken through `(*[N]T)(p)` aliases the memory p names

> - **Status:** FOR REVIEW, no longer a draft (G reviews; COORD's brief to P5 of 2026-11-14 18:06Z). No code is cut.
>   Written while batch 305 gated, on the cut it carries, and re-read against the landed master (next line).
> - **Re-read at the landed master, 2026-11-14 19:35Z:** master `c2919b8b16` (batch 305), merged into this branch at
>   `8a378c0ff1`. The merge base is the cut tip, so master adds only P4's S2-S4 and C2's inline pairs to the tree
>   this was measured on. Re-measured at the merged tree: the converter's emission of the program is byte-identical
>   to the cut's; the 24 arms and the 11 prototype arms give the same output and exit codes (arm M0's eight bytes
>   are an address and differ on every run); the corpus counts of section 3 are unchanged (`src/core` differs from
>   the cut in three files, none cited in this design). The census tables read Go source at the module pins and do
>   not depend on the tree. Nothing in the design changed. Everything section 9 lists as not shown is still not
>   shown.
> - **Read and measured at:** `claude/p5-interior-pointer-cut` `837a413fdc` (the interior-pointer cut, the seat in
>   batch 305). Go 1.26.11, one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0), Release, `GoTargetOS=linux`.
> - **Modules:** modernc.org/sqlite v1.62.0 and modernc.org/libc v1.80.1, sqlite-cs's pins, read from the module
>   cache. sqlite-cs's own tree and G's repro w5b2 were not read; arm C0 below reproduces w5b2's stack.
> - **Evidence:** `docs/phase4/data/array-pointer-doors/` (the program, Go's answers, the cut's answers, the
>   prototype, the predictions as written and scored, the census tool and its tables). Its README says how to
>   re-run each.
> - **Builds on:** `DESIGN-native-backed-slice.md` (the aliasing window), `DESIGN-pointer-provenance.md` (which
>   addresses are Go's), `DESIGN-native-array-view.md` section 4 (the floor called arm 5 here),
>   `DESIGN-native-backed-array-pointer.md` (the native array box), and the interior-pointer root design
>   (`claude/p5-interior-pointer-root` `5a21eeb512`), whose root rule this design applies and does not change.

## Summary (for review)

Go's `(*[N]T)(p)[lo:hi:max]` is a slice header over the memory p names: no copy, length `hi-lo`, capacity `max-lo`.
Its `(*[N]T)(p)[i]` is the element at `p + i*size`. The cut emits neither.

1. **A slice with a high bound is a copy.** Every write through it is lost, a view kept in a variable goes stale,
   the capacity of a two-index view reads the length, and the length is narrowed to `int` unchecked.
2. **Every other form goes through a box of the whole array.** Over C memory a NAMED array (libc's `RawMem`) reads
   the generated wrapper struct out of that memory, and the wrapper's first field is a managed reference. This is
   G's OverflowException when the memory is zero and the array is huge. When the array is small it is worse: no
   error, the data goes to a managed array, and a managed reference is stored into the C memory (arm M0). An
   UNNAMED array is refused by name (arm 5). That refusal is the dominant shape in sqlite: 384 of 433 sites on
   windows/amd64 and 784 of 898 on linux/amd64 are an index through a conversion.
3. **golib already has the aliasing window.** `slice<T>.OverNativeMemory` is what `unsafe.Slice` returns, with the
   provenance consult, the managed arm and the root anchors of the cut. A hand edit that routes the cut's span
   emission through `unsafe.Slice` makes the C-memory shapes, a field array and an interior Go address equal Go
   with no golib change (prototype arm, section 1.3). It does not fix a Go value of another type viewed as bytes.

The design is three doors on `unsafe.Pointer`, each defined by the Go form it stands for, and one floor.

| | Go form | door | what it closes |
|:--|:--|:--|:--|
| A | `(*A)(p)[lo:hi]`, `[lo:hi:max]`, and an open `[:]` of a named array | the view door | W5b, S3, S5; 34 sqlite sites on windows, 16 on linux |
| B | `(*A)(p)[i]`, `*(*A)(p)` read or stored | the element door, the array load and store | the named-array hole at these forms; 396 sqlite sites on windows, 879 on linux |
| C | the pointer itself kept (`raw := (*A)(p)`) | the array pointer door | libc's `goWideString` (2 sites), 1 unnamed site |
| floor | a named array over a bare address by any other route | refused by name at the conversion | arm M0's silent store, everywhere |

The four questions of the brief, answered in section 4: (1) the view door aliases (4.1, 4.2); (2) an open `[:]` of
an array past the span limit is exact under `copy`, where every real site is, and refused by name anywhere else
(4.3); (3) no long-length view is needed: lengths are run-time sizes and keep the Int32 ruling, capacities are
Go's (4.4); (4) the door asks the pointer what it carries, by the cut's rule: Go memory keeps its root, C memory is
bare (4.2).

The decisions asked are in section 8. The first is scope: the brief names door A; the census says door B is
eleven times its size in the program the brief serves.

## 1. What is wrong, measured

### 1.1 The program

`data/array-pointer-doors/NativeViewShapes/main.go` has 24 arms, one process per arm, so a fault in one does not
hide the next. Each arm is one shape taken from libc or sqlite, cited in a comment by file and line. Native memory
is an anonymous `syscall.Mmap`, which the converted runtime returns as a native-backed slice. Two mappings start
as `"XXXXXXXXXXXXXXXX"` and `"abcdefghijklmnop"`; the control arms (`C0 D0 F0 M0`) use a mapping that is still zero.

### 1.2 The cut against Go

Predictions were written before the converted program was built (`predictions.txt`, four sets, every line MET as
worded). "Lost" means the memory still reads its starting bytes.

| arm | shape | Go | the cut `837a413fdc` |
|:--|:--|:--|:--|
| A | `copy((*RawMem)(p)[:n:n], s)` | `5 "helloXXX"` | `5 "XXXXXXXX"`: lost |
| B | memcpy, both sides views | `6 "abcdefXX"` | lost |
| E | `b := (*RawMem)(p)[:n]`, written, re-sliced | `4 true "u\0\0\0\0XXX"` | `4 false "XXXXXXXX"`: lost, and cap is not Go's |
| G | a view over a Go array (`&buf[0]`) | `8 "errno 7\0"` | eight zero bytes: lost |
| H | a view passed to a function that writes | `4 "rrrrXXXX"` | lost |
| I | a view as a copy source | `6 "abcdef"` | equal |
| K | a view kept, the memory changed after | `"WXY!" "WXY!"` | `"abc!" "WXYZ"`: stale both ways |
| L | a `uintptr` constant bound | `16 "0123456789012345"` | lost |
| N | a `uint64` viewed as 8 bytes, written | `true` | `false`: lost |
| O | a byte-array field, a collection, a write | `"field\0"` | six zero bytes: lost |
| P | `unsafe.Add` into a Go slice, a collection, a write | `"....interior.."` | zeros: lost |
| C | `copy((*RawMem)(p)[:], wd)` | `4 "/tmpXXXX"` | dies in the wrapper's `get_Value` (reported as Go's nil dereference) |
| D | `(*RawMem)(p)[len(wd)] = 0` | `"XXXX\0XXX"` | dies the same way |
| F | `raw := (*RawMem)(p)`, `&raw[2]`, `raw[1] = 'Z'` | `25699 "aZcd"` | dies the same way |
| M | C and D over a 64-byte named array | `5 "small\0XX"` | dies the same way: the size is not the cause |
| C0 D0 F0 | C, D, F over zero memory | Go's answers | `OverflowException` from `array<T>..ctor(UInt64)` under `materializeValue`: G's w5b2 |
| M0 | M over zero memory | `5 "small\0\0\0"` | `5`, and the memory reads `e8 30 d0 .. 7f 00 00`: a managed reference |
| Q | `(*[16]byte)(&arr)[:]` over a `[4]uint32`, written | `[0 1 0 0]` | `index out of range [4] with length 0` |
| R S T | an unnamed array indexed, addressed, stored whole, over C memory | `7`, `9`, `"C\0XXXXXX"` | refused by name: arm 5 |
| J | `unsafe.Sizeof(RawMem{})` | `1125899906842623` | equal (a constant) |

Three results were not in the brief.

- **M0.** A pointer to a named array of any size, over C memory that happens to start with a zero word, runs
  without error and corrupts: `materializeValue` allocates a managed array, the copy lands there, and the holder's
  reference is written into the C memory, where the collector does not see it. Over memory that does not start
  with zero the same read takes eight bytes of data as a reference.
- **R, S, T.** The unnamed forms refuse loudly. That is the floor working, and it is the wall sqlite-cs meets
  next: section 3.
- **Q.** A whole view of an unnamed array over Go memory of another element type answers a slice of length 0.

### 1.3 The prototype

The same converted `main.cs` with ONE edit and no golib change: each span emission
`new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(X), LEN))` becomes
`@unsafe.Slice(@unsafe.Pointer.To<byte>(X), LEN)`. Predicted before it was built; every line MET.

| arms | prototype |
|:--|:--|
| A B H I K L | Go's answers. The native arm of `unsafe.Slice` aliases. |
| G O | Go's answers. The element-window arm aliases the Go array, also after `runtime.GC()` (O). |
| P | Go's answer after `runtime.GC()`. The native arm, with the root the cut's `unsafe.Add` kept. |
| E | the memory is Go's; the capacity line still reads `false` (`unsafe.Slice` answers cap = len). |
| N | still `false`. The address resolves to the pinned `uint64` box, so `unsafe.Slice` leaves its native arm, finds no element window, and takes its own snapshot fallback. |

So the window exists and the doors are missing, with one real gap in the window's arms (N), which 4.2 closes.

## 2. The mechanism, read at the tree

**The converter has two paths** (`convSliceExpr.go`).

- *The span path*: a slice of a pointer-to-array conversion with a high bound, inside a function. It reads the
  conversion's emitted TEXT, cuts the `(ж<...>)` prefix off, and emits a `ReadOnlySpan` over the raw pointer. The
  cast to a number drops what the pointer carried, the span constructor copies, and the bound is narrowed with
  `(int)(...)`. The converter knows the copy is wrong when the function writes through it: it prints
  `WARNING: a slice of a pointer-to-array cast at line N COPIES the memory it views` (six times for this program).
- *The box path*: everything else. The conversion is `@unsafe.Pointer.To<A>(p)` and the form is applied to the
  box: `.Value[..]`, `.Value[i]`, `.at<E>(i)`, `.slice()`.

**`To<A>(p)` over a bare address** is `ж<A>.FromNumber`. For `A = array<E>` its arm 5 refuses by name when the
address has no provenance record. For a named array, `A` is the generated wrapper struct, which is not
`array<>`-shaped, so the test (`s_isArrayShaped`) passes it and the fall-through mints `NativeBox<wrapper>`.
`NativeBox<T>.Value` is `Unsafe.AsRef<T>` at the address. The wrapper's first field is its lazy holder, a managed
reference. That is M, C0 and M0.

**What golib already has.**

- `slice<T>.OverNativeMemory(address, length, capacity, anchor)`: the aliasing window. Unmanaged `T` only, by a
  named panic at the door. Only the LENGTH is refused past the span limit (`GoSliceLimit`, the 2026-09-29 ruling);
  `m_capacity` is a native integer and is not bounded. Append within capacity writes in place.
- `unsafe.Slice`: the native arm (an address the registry does not know), the element-window arm (a pointer into
  a managed array aliases that array), a pointer-element arm, and a final snapshot for "a heap box, a struct
  field, or a reinterpreting pointer over a differently-typed array".
- `NativeSliceAnchors<T>`: ties a native window's (otherwise unused) array field to a root, so the root, its pin
  and its source live as long as any slice cut from the window. `NativeDerivation.Kept` decides what a derived box
  keeps: nothing for C memory alone, the root otherwise. Both are the cut's.
- `NativeArrayBox<E>`: a pointer to an unnamed array over native memory, minted with its length; `Value` refuses,
  the element door `at<E>(i)` and the box's slice members answer a native window.

**What a pointer can vouch for.** After the cut an `unsafe.Pointer` value carries its box when it was made from
one (`FromBox`, `FromPinnedBox`, `unsafe.Add` over either). A pointer made from a number carries nothing. Two
kinds of number look the same: a C address, and the transient address `FromRef` reads under `fixed` for one
statement. The cut's 4.3 left `FromRef` at the sites where the use is not provable at the node, and its guard
names them. Section 4.6 is about the second kind.

## 3. Census

`data/array-pointer-doors/avcensus` (go/packages and go/types) lists every conversion `(*A)(p)`, `p` an
`unsafe.Pointer` and `A` an array type, with the form that consumes it. Control: it reports the 27 conversions of
the program above (24 arms) with the right form on each. Tables: `census/tables.txt`.

| tag | sites | three-index | two-index | open | index | dereference | pointer kept |
|:--|--:|--:|--:|--:|--:|--:|--:|
| sqlite + libc, windows/amd64 | 433 | 26 | 7 | 1 | 384 | 12 | 3 |
| sqlite + libc, linux/amd64 | 898 | 12 | 2 | 2 | 784 | 95 | 3 |
| std, windows/amd64 | 125 | 18 | 7 | 11 | 37 | 6 | 46 |
| std, linux/amd64 | 117 | 6 | 5 | 7 | 37 | 6 | 56 |
| std, linux/arm64 | 118 | 6 | 5 | 7 | 37 | 6 | 57 |
| std, darwin/amd64 | 115 | 6 | 5 | 8 | 37 | 6 | 53 |
| std, darwin/arm64 | 113 | 6 | 5 | 8 | 37 | 6 | 51 |

What the tables say.

- **Slice forms in sqlite and libc are all `RawMem`.** Windows: 34 slice sites (26 three-index, 7 two-index, 1
  open), 4 index sites, 2 pointers kept; 40 in all, 2 of them in a libc test file. G's analysis counted 16
  libc views (1 whole, 13 bounded, 2 index stores); the tool finds 25 in libc's non-test files. The difference
  that matters is the 7 two-index sites, whose Go capacity is `1<<50 - 1 - lo`, and the 2 kept pointers, which
  are a third form.
- **Every open `[:]` of `RawMem` is a `copy` destination**: windows 1, linux/amd64 2 (and 2 more in
  `libc_unix.go`, by reading; that file is not in either target's build).
- **Views are read as well as written.** 7 of the windows sites are `copy` SOURCES (sqlite's `conn.go`), and 4
  are kept in a variable (`mem := (*libc.RawMem)(unsafe.Pointer(p))[:sz:sz]`, `vtab.go`).
- **One windows view is over Go memory**: `libc_windows.go:1657`, `&strerrorBuf[0]`.
- **The index form is the population.** 384 on windows, 784 on linux, nearly all unnamed arrays in ccgo's output
  (`(*[N]T)(__ccgo_ptr(p))[i]`; 380 of the 391 unnamed windows sites have that operand, a function that returns
  `unsafe.Pointer(p)` for a `uintptr`). 170 are stores, 73 reads, 92 bound to a variable, 43 arguments. No element
  type in either sqlite table holds a pointer.
- **In the standard library the index form is one file family**: 34 stores per target, all in `runtime`'s malloc
  files (`(*[2]uint64)(x)[0] = 0`), plus 2 address-of and 1 argument. The pointer kept (46 to 57 per target) is
  the standard library's main form, and it works today over Go memory.
- **The converted corpus has 13 span emissions in 6 files** (`internal/abi/type.cs` 2,
  `internal/syscall/windows/registry/windows/value.cs` 4, `runtime/heapdump.cs` 3, `runtime/iface.cs` 1,
  `runtime/mbitmap.cs` 1, `runtime/windows/os_windows.cs` 2), and 4 more in one `.cs.auto` sibling. Counted by
  `git grep` at the cut. The tool's 11 to 36 slice sites per target are more than that because some sit in
  hand-owned files, some have pointer elements (refused by the converter by name already) and the open forms
  take the box path.

NOT in the census: 32-bit targets (there `RawMem` is `[1<<31 - 1]byte`, which fits a native integer and is
still past the span limit, so nothing below changes by reading), and third-party modules other than these two.

## 4. The design

One rule: **a form applied to `(*A)(p)` is emitted as a door that receives `p` itself**, the `unsafe.Pointer`
value, with the array's length as a constant. The pointer is never cast to a number first, and no box of the
whole array is minted to serve a slice or an element. Names below are proposals; the implementation may respell
them, and none replaces a public member.

### 4.1 The view door (door A)

    (*A)(p)[lo:hi:max]   ->   @unsafe.Pointer.View<E>(p, lo, hi, max, N)
    (*A)(p)[lo:hi]       ->   @unsafe.Pointer.View<E>(p, lo, hi, N,   N)

`E` is the array's element type read from go/types (as `namedArrayViewElem` does today), `N` its length. The
bounds are passed at full width. The door checks `0 <= lo <= hi <= max <= N` with Go's slice-bounds panics, then
answers a `slice<E>` of length `hi-lo` and capacity `max-lo` over the memory at `p + lo*size`, by the arms of 4.2.
The same emission serves a named and an unnamed array, the forms with extra parentheses, and the explicit
dereference `(*(*A)(p))[lo:hi]`: the converter already recognizes those as one view
(`pointerArrayViewConversion`).

### 4.2 What the door answers, by what the pointer vouches for (question 4)

The door moves the pointer by `lo*size` with the cut's `unsafe.Add` (which keeps what the pointer carried), then:

| | the pointer | the view | lifetime |
|:--|:--|:--|:--|
| V1 | names an element of a managed array or slice of `E` | a managed window on that array (`unsafe.Slice`'s element-window arm) | the array is a managed object; nothing to pin |
| V2 | carries, or resolves to, Go storage of another type that holds no references and can be pinned; or is derived from such storage by arithmetic | a native window over the address, ANCHORED to the root | the root, its pin and its source live as long as any slice cut from the view |
| V3 | is a number the registry does not know | a bare native window | the memory's own, as in Go |
| V4 | names Go storage that cannot be held still and is not V1 | today's snapshot, unchanged | none; writes do not reach the source |

- V1 and V3 are `unsafe.Slice`'s arms as they stand, with Go's capacity instead of `cap = len`. For V1 the
  capacity cannot exceed the Go array the view starts in, so it is `min(max-lo, the array's remaining elements)`:
  a program that reaches past its own allocation is invalid in Go, and for `RawMem` over a Go array `N` is never
  the real size.
- V2 is new, and it is the N arm. `unsafe.Slice` refuses its native arm for an address the registry knows, for a
  reason written before the anchors existed: a native window over managed storage "retains nothing that holds
  the object still". An anchored window retains exactly that. P is the same mechanism already working (an
  interior address misses the registry, takes the native arm, and the anchor keeps the root): measured across a
  forced collection. V2 extends it to the root's own address. `unsafe.Slice` itself is NOT changed by this
  design; whether it should take V2 too is decision D6.
- V3 over C memory is bare, which is the root rule's other half: `NativeDerivation.Kept` answers null for C
  memory alone, so no anchor is allocated.
- V4 keeps the snapshot, because for reads it is exact and nothing better exists: a root that holds references
  cannot be pinned. The converter's warning stays for the sites that can only be V4 (4.6).
- An element type that holds references is refused by the converter by name today (`managedSliceViewRefusal`);
  unchanged. A pointer element takes `unsafe.Slice`'s pointer-words arm; unchanged. A type whose C# size is not
  its Go size is refused by `OverNativeMemory`; unchanged.

### 4.3 The open forms (question 2)

`(*A)(p)[:]` and `(*A)(p)[lo:]` have length `N - lo`. What Go USES of that length, at every real site, is
`copy`'s minimum with the other operand.

- **Directly under `copy`**, as destination or source:

      copy((*A)(p)[lo:], src)   ->   @unsafe.Pointer.CopyInto<E>(p, lo, N, src)
      copy(dst, (*A)(p)[lo:])   ->   @unsafe.Pointer.CopyFrom<E>(dst, p, lo, N)

  The helper copies `min(N - lo, len(other))` elements through a view of exactly that length and answers the
  count. This is Go's result for every `N`, not an approximation for large ones, and it evaluates `p` and the
  other operand once each, in Go's order. The converter already recognizes a slice as a direct `copy` argument
  for another view (`goImageCopyView.go`). C, C0 and M's first line take this form.
- **Anywhere else**, the open form is `View<E>(p, lo, N, N, N)`. For an array within the span limit that is a
  view of the whole array. For an array past it (`N - lo` over `Array.MaxLength`) the door refuses by name with
  the existing text of the ruling ("exceeds the maximum slice length of this runtime"), and the converter warns
  at conversion time, because `N` is a constant it can see. **There is no such site** in libc or sqlite at these
  versions. The refusal is the honest answer for one that appears: a Go slice of `1<<50` bytes has no
  representation here, and a clamped one would answer a wrong `len`.
- **Scope of door A's open form: named arrays only.** An unnamed open form (`(*[4]byte)(unsafe.Pointer(&x))[:]`,
  7 or 8 sites per target in the standard library, reflect's `nameOff` among them) keeps today's emission. It
  runs on banked rows today. Q shows the same emission answering length 0 for another pair of types; that is
  recorded as its own row (decision D4), not taken here.

### 4.4 Lengths and capacities (question 3)

- **No long-length view.** The length of every real view is a run-time size: `n`, `count`, `len(s)`, `size`. The
  door takes it at full width and refuses a length past the span limit by name, as `unsafe.Slice` does. Today's
  emission narrows it with `(int)(...)`, which by reading wraps (not run: `2^32 + 5` would read 5 elements).
  What stays refused is one `memcpy` or `read` of more than about 2 GiB through a single view. Lifting that
  needs either a long slice or a chunked copy inside the helpers; neither is proposed until a real program asks.
- **Capacity is Go's.** A two-index `(*RawMem)(p)[:count]` has capacity `1<<50 - 1`, and the window can carry it:
  only the length spans. E's capacity line then reads `true`. An `append` within capacity writes into the C
  memory in place, which is what Go does with that slice.
- **The array's length `N` is a bound, not a size.** It is passed as a constant and used for the bounds panics
  and for the open forms. Nothing of length `N` is ever allocated or spanned.

### 4.5 The element door, the array load and store, the pointer kept (doors B and C), and the floor

**Door B.** An index or a dereference applied directly to the conversion:

    (*A)(p)[i]          ->   @unsafe.Pointer.Element<E>(p, i, N).Value       read, store, compound, ++
    &(*A)(p)[i]         ->   @unsafe.Pointer.Element<E>(p, i, N)
    v := *(*A)(p)       ->   @unsafe.Pointer.LoadArray<E>(p, N)              a Go array value: a copy
    *(*A)(p) = v        ->   @unsafe.Pointer.StoreArray<E>(p, N, v)

`Element` checks `0 <= i < N` with Go's index panic and answers a pointer to `E`. When `p` names a managed array
of `E` it is that array's element box, the answer `at<E>(i)` gives today. Otherwise it is
`To<E>(unsafe.Add(p, i*size))`, which is Go's own definition of the form and is entirely the cut's machinery:
the root is kept, C memory is bare, and an `E` that holds references follows the pointee rule of the root design.
`LoadArray` and `StoreArray` copy `N` elements through `View<E>(p, 0, N, N, N)`; a named array wraps the result.
No box of the whole array exists at any point, so there is nothing to materialize and nothing to refuse.
Cost: one small box per access, the same as `*(*T)(unsafe.Pointer(p))` costs today.

**Door C.** The conversion whose value is kept, passed or returned:

    (*[N]E)(p)          ->   @unsafe.Pointer.ToArray<E>(p, N)
    (*A)(p), A named    ->   @unsafe.Pointer.ToArray<A, E>(p, N)

Over Go memory both answer what `To<...>` answers today. Over an address the registry does not know, the unnamed
form answers `NativeArrayBox<E>.Over(address, N, root)`, which exists, and the named form a sibling box kind for
the wrapper type: its `Value` refuses by name, and its element door answers native elements. Then `&raw[i]`
(emitted `raw.at<byte>(i)` today) works, which is all `goWideString` does. A store through a kept pointer to a
named array (`raw[1] = 'Z'`, emitted `raw.Value[1]`) is refused by name: zero real sites, and curing it means
changing how every index through a pointer to a named array is emitted, which is not proposed. The named box's
element window is cut at the span limit with the array's length as its capacity, so an index past the limit into
a `RawMem` pointer is refused by name; an index below it is exact.

**The floor.** In `FromNumber`, beside arm 5 and with the same provenance test: a `T` that is a generated array
wrapper (a struct that is an `IArray` and not `array<>`, decided once per `T`) over an address with no record is
refused by the same named panic. This is the route every non-converter caller takes (reflect, a hand-written
file, a module converted before door C). It cures nothing and makes M0 loud. It is separable and small, like arm
5 was, and it is proposed to land first (decision D2).

> **CORRECTED 2026-11-14 21:00Z (P5), from the floor seat's finding.** Two sentences above are wrong or
> incomplete, and are left as written so the review can see what changed.
>
> 1. *"Over Go memory both answer what `To<...>` answers today"* is wrong for the NAMED form. Measured at master
>    `c2919b8b16`: `To<A>` over Go memory of another type (a `[64]byte` field or variable, a `[8]uint64`, a
>    slice's first element, a struct's first field) makes a native box of the wrapper, loses the program's write
>    and stores a managed address into the Go storage, in 5 shapes of 7. Only the round trip at the pointer's
>    own type is right. Evidence: `data/named-array-floor/gomem/` on `claude/p5-named-array-floor`. So door C's
>    named form cannot delegate to `To` over Go memory either: `ToArray<A, E>` has to answer its own box of the
>    wrapper, sharing the Go array where `p` names a managed array of `E`, and the sibling box kind otherwise.
>    The reverse (a named array viewed as the unnamed one) answers length 0 today, as arm Q does.
> 2. *The floor* landed wider than this paragraph, by COORD's 19:58Z ruling: the provenance test at
>    `FromNumber` as written here, and a second line in the `NativeBox` constructor that refuses the wrapper
>    over ANY real address (not 0, not an order token), because an address with a record and the `Reinterpret`
>    conversion both reach the same store. Consequence for the doors: no door may make a `NativeBox` of a
>    wrapper. Door B never does (`Element`, `LoadArray` and `StoreArray` work in `E`). Door C's named form
>    must mint its own box kind for C memory and for Go memory alike.

### 4.6 A `FromRef` operand keeps the snapshot

`@unsafe.Pointer.FromRef(ref x)` is a number read under `fixed` for one statement. An aliasing window over that
number would outlive the statement that made the address true. So: **where the conversion's operand contains a
`FromRef` mint, the converter keeps today's span emission and today's warning.** In the corpus that is exactly
the two lines of `internal/abi/type.cs` (`addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), ...)`), which read
a method table and never write.

A `FromRef` number that reaches a door through a variable or a parameter is the provenance design's stated bound,
not a new one: `unsafe.Slice` already treats a number the registry does not know as native. What changes is the
window's lifetime for the views that were copies. The 11 corpus lines that would change are read one by one in
section 6 for this reason, and the guard of 4.7 keeps the rule.

### 4.7 Guards

- repoguard: no emitted view, element or array door has an operand containing `FromRef(`; the count of remaining
  span emissions in the corpus is pinned by file (2, both in `internal/abi/type.cs`), each named with its reason.
- The cut's native-mint census gains the doors' mints (the anchored window of V2, the named array box).
- GolibTests: the doors against `NativeMemory` and against pinned and unpinnable Go storage, each arm of 4.2 by
  name, the bounds panics, the span-limit refusal, the floor.
- One behavioral project from the program of 1.1, with per-OS native memory, so the emission is under a golden.

## 5. Acceptance: the arms

The program of 1.1, each arm against `go.out`, plus:

- **N under stress**: the V2 view of a `uint64`, a `uint32` array and a struct of scalars; 10,000 rounds of
  write, forced compacting collection, read through the variable; and the same with the view kept alive across
  the round while the variable goes out of use.
- **V1 capacity**: a three-index view of a Go array whose `max` passes the array's end.
- **Bounds**: `lo > hi`, `hi > max`, `max > N`, a negative bound, `i == N`, each with Go's panic text.
- **Span limit**: a length of `Array.MaxLength + 1` over C memory is refused by name; a two-index view with a
  small length over `RawMem` reports Go's capacity; `2^32 + 5` is refused, not read as 5.
- **The made-to-fail arm** (safety floor item 13): with V2's anchor removed, the stress arm must go red; with the
  `FromRef` rule removed, repoguard must name `internal/abi/type.cs`.
- **sqlite-cs**: G re-runs the demo past W5b; S3's and S5's sites are re-read at the emission.

## 6. Predictions, to be scored as worded

Each increment is predicted here and scored when it is cut. The corpus numbers are derived from `git grep` and the
tool at `837a413fdc` and are to be re-derived by the two-seeded three-target emission before any regen.

| | prediction | falsifier |
|:--|:--|:--|
| P1 | With door A: arms A B E G H I K L N O P and C C0 print Go's answers, E's capacity line included. M's first line (`copy` into the 64-byte named array) lands; M and M0 as a whole wait for door B. | any of them differing |
| P2 | With door B: D D0 M M0 R S T print Go's answers. | any of them differing |
| P3 | With door C: F and F0 print `25699` and `0` and then refuse the store `raw[1] = 'Z'` by name. | the address-of form failing; the store succeeding wrongly |
| P4 | With the floor alone, at the cut's emission: M0 is a named panic and no memory is written. | M0 printing anything |
| P5 | Door A's corpus footprint: 11 lines in 5 files change spelling (registry `value.cs` 4, `heapdump.cs` 3, `iface.cs` 1, `mbitmap.cs` 1, `os_windows.cs` 2); 5 of them are in flat files and move on every target, 6 are in `windows/` folders. `internal/abi/type.cs` does not move. No `package_info.cs` record moves. The `.cs.auto` sibling is named, not applied. | any other file; any of these not moving |
| P6 | Door B's corpus footprint is in `runtime`'s malloc files only: 34 stores, 2 address-of and 1 argument per target, plus the 6 dereference forms. | an index form outside `runtime`; a banked `runtime` row changing verdict |
| P7 | Door C's corpus footprint for named arrays: 4 lines in `runtime/mgcmark_greenteagc.cs`. The unnamed respelling is the 84 `To<array<...>>` lines in 28 files and is NOT proposed in the first cut (decision D5). | a named line elsewhere |
| P8 | Nothing a validated row reads changes its answer: the 11 lines of P5 read as follows. `value.cs` 4: the operand is `Ꮡ(data, 0)`, an element of a Go slice of another element type (V2, or V1 for the byte pair), written by the callee or read after a syscall filled it. `heapdump.cs` 2: direct `copy` sources, lifetime of the statement. `heapdump.cs` 1, `mbitmap.cs` 1: `sysAlloc` and `persistentalloc` memory, both WRITTEN through after the view is made, so today's copy loses those writes (latent). `iface.cs` 1: read-only. `os_windows.cs` 2: a `uintptr` parameter, read and written. | a banked windows or runtime row changing verdict under the sweep |

P8's two latent lost writes (`heapdump.go:743`, `mbitmap.go:1536`) are read from the Go source, not run.

## 7. Stop lines

1. **V2 under stress.** If the stress arm is wrong even once in 10,000 rounds, V2 is withdrawn: a Go value of
   another type keeps the snapshot (N stays red, named), and the design says so. Doors A, B and C over C memory
   and V1 do not depend on V2.
2. **Footprint.** A two-seeded emission that moves a file P5 to P7 do not name stops the regen until the line is
   attributed.
3. **A `FromRef` number at a door.** If the guard of 4.7 finds an aliasing door whose operand is derived from
   `FromRef` in the corpus beyond the two named lines, that site keeps the snapshot by name before anything lands.
4. **Banked rows.** The windows flavour of `os` and `internal/syscall/windows/registry`, `runtime`, and the five
   largest reflect consumers by verdict count are swept at the merge result. A changed verdict stops the landing.
5. **Public surface.** New members only. The binary-compat guard stays clean without a suppression.
6. **The element door's cost** is not a stop line. If sqlite-cs measures it as dominant, a ref-returning form for
   the read and the store is a follow-up with its own lifetime reading.

## 8. Options not taken, and the decisions asked

Not taken:

- **A native backing for `array<T>`** (`DESIGN-native-array-view.md` section 3). It would make `.Value` of a
  native array pointer real, and it changes the representation every array in the corpus uses. The doors need no
  array value at all.
- **A distinct "tail" type for the open form** with its own `copy` overloads. It removes the converter's look at
  `copy`, and `x := (*A)(p)[:]` would then infer the wrong type for `x`.
- **Clamping an open view to the span limit.** `len` would be wrong, silently.
- **Routing the span path to `unsafe.Slice` as is** (the prototype). It leaves N red, has no capacity, and keeps
  the narrowing at the call site.

Decisions asked (my recommendation first):

- **D1, scope.** Design all three doors here; cut door A and the floor first (the brief), door B second, door C
  last. Reason: door B is 384 of 433 sqlite sites on windows and is refused by name today, so G's demo meets it
  directly after W5b; it is one small door on the cut's own machinery.
- **D2, the floor first.** Land the named-array floor with door A or ahead of it. It turns a silent store of a
  managed reference into C memory into a named panic, and it depends on nothing else.
- **D3, an open view past the limit outside `copy`.** A run-time refusal by name and a conversion-time warning
  (recommended), or a conversion-time error. No site exists either way.
- **D4, unnamed open forms.** Left on today's emission, with Q recorded as a BOARD row (recommended), or taken
  into door A, which moves reflect's `nameOff` lines on a banked row.
- **D5, door C for unnamed arrays.** Not in the first cut (recommended): the form works over Go memory today, its
  footprint is 84 lines in 28 files, and sqlite has one such site.
- **D6, `unsafe.Slice` and V2.** Leave `unsafe.Slice` as it is (recommended); V2 is the view door's arm. Taking
  it there too changes validated behavior and deserves its own measurement.
- **D7, the review.** The design touches the safety boundary in one place that is new, V2 (an aliasing window
  over pinned Go storage of another type, kept alive by an anchor), and names one existing hole (M0). COORD's
  second review is asked for those two, not for the doors over C memory.

## 9. What this does not show

- Nothing ran on windows or darwin. The census covers them by type-checking; the program's native memory is
  `syscall.Mmap`.
- The doors do not exist. The prototype is one hand edit that uses `unsafe.Slice`; V2, the capacity, the open
  forms, the element door and the floor are designed and predicted, not run.
- V2's lifetime claim rests on reading `NativeSliceAnchors` and on arm P, not on a stress run.
- Master before the cut was not measured. The named-array read reaches the same conversion body through the old
  cast spelling, so the hole is expected there; that is a reading.
- P8's reading of the 11 corpus lines is from the Go source and the emitted text, not from a run.
- No cost was measured: the element door's box per access, the anchor's allocation per V2 view.
- sqlite-cs's tree, its walls S3 and S5 as G lists them, and G's 16-view census were not read. The census here is
  the tool's, at sqlite-cs's module pins.

## 10. Amendment 2026-11-14 22:40Z (P5): the rulings on doors A and B, the census by arm, the floor as reworked

COORD's rulings of 21:20Z (items 3 to 6, with G's agreement to the design at `27ac1088e8`), the follow-up review
of the floor of 22:22Z, and the forward of 22:17Z. Nothing above is rewritten; where this section and an earlier
one differ, this one is later. Evidence: `data/array-pointer-doors/amend-1114/`.

<!-- Provenance. Rulings: claude/mailbox docs/phase4/inbox/P5/20261114T212040Z-COORD.md (doors),
     20261114T222234Z-COORD.md (review of 4c8ee1cbd2, three nits), 20261114T221736Z-COORD.md (two pointer casts).
     The floor: branch claude/p5-named-array-floor, tip 4c8ee1cbd2, DESIGN-native-array-view.md second block.
     The probe: built 2026-11-14 22:27Z to 22:31Z on linux/amd64 against golib at 4c8ee1cbd2, converter of
     c2919b8b16. The caller counts: grep over the 96 files `go list` selects for windows/amd64, sqlite v1.62.0. -->

### 10.1 The floor is at the read (corrects the 21:00Z block of 4.5)

The 21:00Z correction says the floor has "a second line in the `NativeBox` constructor". After the review that is
no longer so. The named-array floor is ONE line, in `NativeBox<T>.ValueAddress`, which `Value` and `ValueSlot`
both go through. The constructor refuses nothing, and the number conversion has no named-array line.

- `(*RawMem)(cptr)` over C memory is therefore refused at the first READ of the array, not at the conversion.
  Until it is read the pointer is an address: its number, its equality and a cast back to another pointer type
  all work, as they did at master.
- A nil pointer to a named array is not refused by the floor (its test is `address != null`). Its read reaches
  address 0 and the CLR's fault becomes Go's nil dereference, as for every nil native box. Not new.
- The rule for the doors is unchanged: no door makes a native box of a wrapper and reads it. Door C's named
  kind is a sibling box whose `Value` refuses by name.
- The `array<>` test is spelled in three places (`ж.NativeBox.cs:174`, `ж.cs:329` and `ж.cs:966`). Door A's cut
  touches both files and ties them with a comment at each naming the others.

### 10.2 Ruling 3: arm W, the twin of w5b2

w5b2 is Go memory: a `RawMem` view over a local `[16]byte`. Three statements join the acceptance kit as arm W
(`amend-1114/w/`, Go's answers in `go.out`):

    var buf [16]byte
    copy((*RawMem)(unsafe.Pointer(&buf[0]))[:], "name\x00")       5 "name\x00\x00"       CopyInto over V1
    copy(out[:], (*RawMem)(unsafe.Pointer(&buf[0]))[:])            4 "name"               CopyFrom over V1
    copy((*RawMem)(unsafe.Pointer(&buf[0]))[12:], "wxyz")          4 "\x00\x00wxyz"       CopyInto, lo = 12

W sits beside C0 in P1's list. It is expected red at the floor's tip with the floor's refusal, as arms gb and gg
of the floor's kit are (a named array over the address of an element of Go memory). W itself has not been
converted yet.

### 10.3 Ruling 6: CopyInto and CopyFrom over V1 stop at the Go array

Over V1 the count is `min(N - lo, len(other), the array's remaining elements from the pointer's element)`.
When the first two allow more than the third, the door refuses BY NAME before it copies anything:

    go2cs: copy through (*[N]E)(p)[lo:] would pass the end of the Go array p points into (K elements remain)

Go has no answer for that program (it writes or reads past its own allocation), so the arm that holds it is a
GolibTests arm and not a kit arm: a 20-byte source into arm W's 16-byte array, and a 32-byte destination from
it. The message is asserted, and the array is asserted unchanged after the refused write. The view form's
capacity rule in 4.2 is the same bound and is unchanged.

### 10.4 Ruling 4: the census by arm, and V2 has a real site

The split is `amend-1114/by-arm.txt`. A site's arm is decided at run time by what its operand names, so the
split is a reading of each site's source.

| population | V3 | V1 | a `uintptr` parameter: the caller's arm | lines |
|:--|--:|--:|--:|--:|
| sqlite + libc, windows/amd64 | 16 | 1 | 17 | 34 |
| sqlite + libc, linux/amd64 | 12 | 0 | 4 | 16 |

G's reading of the sites holds: sqlite's own 12 views take C memory, and libc's one view of Go memory at the
site (`Xstrerror`) is V1. The 17 parameter lines are where it does not close. They sit in 15 functions of the C
ABI (`Xmemcpy`, `Xmemmove`, `Xread`, `Xfread`, `Xsnprintf` and others), and what they view is whatever number
the caller passes.

**Not confirmed: one production site needs V2.** sqlite's generated code on windows/amd64 calls `Xmemcpy` 412
times and `Xmemmove` 42 times. Three calls pass the address of a Go package variable as the DESTINATION:

- `sqlite_windows.go:5802`, in `Xsqlite3_randomness`: `memcpy(&sqlite3Prng, &chacha20_init, 16)`, the first
  seeding of sqlite's random number generator, which sqlite reads for temporary file names, journal and WAL
  salts and random rowids.
- `_sqlite3PrngSaveState` and `_sqlite3PrngRestoreState` (test control): the whole 132-byte state, both ways.

The variable is `struct { Fs [16]uint32; Fout [64]uint8; Fn uint8 }`. The converter emits a struct of this shape
with an inline layout (the probe's twin: 132 bytes, fields at 0, 64 and 128, no reference), so it is V2's case: Go storage of another type that holds no
references and can be pinned. Thirteen more calls pass a Go variable as the SOURCE; a snapshot is exact for
those. The counts are a lower bound (an address that arrives through a variable or arithmetic is not counted).

Measured on the same shape (`amend-1114/prng/`, libc's `Xmemcpy` body with six callers):

| arm | destination, source | Go | converted | prototype |
|:--|:--|:--|:--|:--|
| pa | the 132-byte struct, from a `[4]uint32` | `0x61707865 0x3320646e ...` | zeros | zeros |
| pb | the struct, from the struct | `7 9 3` | `0 0 0` | `0 0 0` |
| pc | a struct of scalars, from a `[4]uint32` | `0x61707865 ...` | zeros | zeros |
| pd | a `[8]byte`, from a `[8]uint8`, each named by the array's own number | `[217 213 5 ...]` | zeros | zeros |
| pe | a `[4]uint32`, from a `[8]uint8` | `0xf905d5d9 0xd763a120` | zeros | zeros |
| pf | a `[16]byte`, from a `[4]uint32` | `[101 120 112 ...]` | zeros | zeros |

The prototype is `unsafe.Slice` as it stands, which for a number the registry knows takes its snapshot. That is
what door A answers for Go storage of another type when V2 is absent. So with door A cut as V1, V3 and V4 only,
arms pa, pb, pc and pe stay lost, silently, and so does the seeding at `:5802` on the windows flavour.

Two more things the probe shows:

- **V1 must take the array's own number.** pd and pf name a byte array by `uintptr(unsafe.Pointer(&buf))`, the
  array and not its element 0, and the prototype misses the element window. Door A's V1 is defined on what the
  number resolves to: a box of `array<E>` at offset 0 is that array's element 0, and a number inside the array
  is the element at that offset. pd joins P1 as a V1 arm; pf is V1 for its destination.
- **std needs no V2.** None of P5's 11 lines writes through a view of Go storage of another type
  (`by-arm.txt` section 4). P8 calls the two `os_windows` lines "read and written"; read at the source they are
  read only.

**What follows for the cut.** By the ruling's own terms ("deferred to .5 unless a real site appears") V2 is not
deferred by this section; the decision is COORD's. The order proposed keeps both outcomes open:

1. Door A is cut as V1, V3, V4 first: its commits, its arms (A B E G H I K L O P, C C0, W, pd, pf), its
   footprint.
2. V2 is the LAST commits of the same seat: the anchored window, arm N, arms pa pb pc pe, the stress arm of
   section 5 and its made-to-fail twin. Stop line 1 is unchanged: one wrong round in 10,000 withdraws it.
3. If V2 is deferred or withdrawn, the seat ends at step 1 and N, pa, pb, pc and pe stay red and NAMED, with a
   BOARD row that names `sqlite_windows.go:5802`.

**D8, asked: what a lost write says when V2 is absent.** `Xmemcpy`'s views are bounded forms, which 4.1 emits
as `View` and then `copy`. The door hands out a snapshot and cannot know it will be written, so without V2 the
write at `:5802` stays as silent as it is today (the converter's warning names the line at conversion time, and
nothing at run time). Two answers:

- (a) Leave it: the BOARD row of step 3 names the site, and V2 is the cure.
- (b) Emit a bounded view that is `copy`'s direct destination through `CopyInto` with its bounds (15 of the
  windows flavour's 34 lines have that context), so the door sees the write and refuses a snapshot destination
  by name. One more emission shape and one more refusal.

Recommended: (a) if V2 stays in the seat, because the snapshot destinations that remain are roots that hold
references, which the converter already warns about; (b) only if V2 is deferred, so that `:5802` fails by name
on the windows flavour and not silently. No std production line is a copy destination through a view (P5's 11
lines are bound views and copy sources), so neither answer moves a banked row's production side. Three std TEST
lines are (`os_windows_test.go:243`, `:340`, `:730`: `uint16` views of `&buf.PathBuffer[0]`, a one-element array
at the end of a struct, and of a typed pointer). Their arm is NOT classified here; they are named so that the
cut's `-tests` census reads them before (b) could reach a banked windows row of `os`.

### 10.5 Ruling 5: door B's first cut, the V3 fast path

The element door of 4.5 costs one box per access, and the index form is in hot code (G's count on windows/amd64:
`_chacha_block` 97 sites, `_balance_nonroot` 31, `_datetimeFunc` 28). The first cut of door B therefore has two
spellings:

    (*A)(p)[i]   read, store, compound, ++     ->   @unsafe.Pointer.ElementRef<E>(p, i, N)        a `ref E`
    &(*A)(p)[i]                                 ->   @unsafe.Pointer.Element<E>(p, i, N)           the box, as in 4.5

`ElementRef` checks `0 <= i < N` with Go's index panic, then:

- **V3** (the number resolves to nothing): `ref Unsafe.AsRef<E>((void*)(p + i*size))`. No allocation, and no
  lifetime question, because C memory has none.
- **V1, V2, V4 and a pointer that carries a root**: the box of 4.5, and `ref box.Value`. The box is what keeps
  the root, so it is not removed here.

An `E` that holds references is refused as in 4.5; nothing about the pointee rule changes.

**Acceptance, added.** An allocation-count arm on an insert-shaped loop: 100,000 stores and reads through
`(*[1<<20]uint32)(unsafe.Pointer(c))[i]` over C memory, measured with `GC.GetAllocatedBytesForCurrentThread`
around the loop, after one warm-up pass. **Stop line 7 (it replaces stop line 6, which left the cost to a
follow-up): more than 0 bytes per element for V3 stops door B.**
The control is the same loop over a Go `[]uint32` through `&s[0]` (V1), which is expected to allocate and
shows the instrument can read a non-zero.

**For whoever cuts N3** (G's caveat): a `ref` return cannot live across an `await` (CS8178). `ElementRef`'s
result is used inside one expression, and the spill the N3 twin introduces must never split that expression.
If a site cannot be kept in one expression, it takes `Element(...).Value`.

### 10.6 The two pointer casts COORD forwarded (22:17Z): both keep their root

Read at `4c8ee1cbd2`; nothing was run.

- `runtime/mprof_impl.cs:194`, `(ж<bucket>)(uintptr)(allnext.Load())`. The number is the one
  `FromPinnedBox(nb)` registered when the previous bucket was stored. `bucket` holds references, so the number is
  an order token; the number conversion resolves it and, the pointee types being equal, answers the bucket's
  own box (its first arm). The result IS the root. The box is reachable throughout: the atomic holds the
  pointer that carries it until the store four lines below, and the hash chain holds it for good.
- `unsafe/unsafe.cs:1452`, `(ж<array<T>>)(uintptr)Pointer.FromPinnedBox(field)`. The cast to a number drops what
  the pointer carried, but the conversion's last line keeps what the number RESOLVES to
  (`NativeDerivation.KeptResolved`), and the registry resolves a pinned address to its box. So the native box
  keeps the field's box, which keeps the pin. If the field's box were collected between the two casts (only
  the caller's reference holds it there), the number would not resolve and arm 5 would refuse by name, since
  the target is always `array<T>`: a refusal, never a box that keeps nothing. The 11 corpus callers pass a
  field's box of a struct box that is live across the call (`pp.ᏑPort` 8, `sa.ᏑPort` 2, `mp.of(m.Ꮡcheaprand)` 1).

Neither is a BOARD row. Door A's cut respells `:1452` as `Pointer.To<array<T>>(Pointer.FromPinnedBox(field))`,
which keeps the root by what the pointer carries and closes the window by construction (one line, hand-owned,
GolibTests arm beside it).

### 10.7 What this section does not show

- The doors still do not exist: every "door A answers" above is the design, and the prototype is `unsafe.Slice`.
- That V2 as designed makes pa, pb, pc and pe print Go's answers is predicted, not run.
- The caller counts are for package `modernc.org/sqlite/lib` on windows/amd64 only, and are a lower bound.
- What the linux flavour does with `memcpy(&sqlite3Prng, ...)` (its libc memcpy is not a view form) was not
  measured.
- 10.6 is a reading. The window at `unsafe.cs:1452` was not produced.
- sqlite-cs's own tree was not built; `:5802` is read from the module, not observed failing in the demo.

## 11. Amendment 2026-11-15 01:30Z (P5): door A as cut (V1, V3, V4), its scorings, and what it found

Door A is cut on `claude/p5-array-door-a`, ten commits on `31a22d3e7c`, tip `a37a986860` before this block.
Nothing above is rewritten; where this section and an earlier one differ, this one is later. V2 is not in these
ten commits. It is the last commits of the seat, as ruled, and gets its own block. Evidence:
`data/array-pointer-doors/door-a-1115/` and `data/array-pointer-doors/scope-probe/`.

<!-- Provenance. Rulings: claude/mailbox docs/phase4/inbox/P5/20261114T223917Z-COORD.md (V1/V3/V4 first, V2 last;
     stop line 1; D8 = a), 20261114T234748Z-COORD.md (the hand-own respell is eight lines), 20261115T001446Z-COORD.md
     (the failed behavioral prediction scored; the rule line), 20261115T004631Z-COORD.md (the project-file remedy),
     20261115T011951Z-COORD.md (the lost write at a named array's address: its own seat), 20261115T012026Z-COORD.md
     (the scoring accepted; the nil-length row is door C's). My posts: inbox/COORD/20261114T230543Z-P5.md,
     20261114T234701Z-P5.md, 20261115T001409Z-P5.md, 20261115T003530Z-P5.md, 20261115T004525Z-P5.md,
     20261115T011848Z-P5.md, 20261115T011939Z-P5.md. Box: one cloud linux/amd64 VM, Go 1.26.11, .NET SDK 10.0,
     Release, GoTargetOS=linux unless a line says otherwise. Converters: base = the batch-305 converter, cut = the
     converter built at a0c43a357b. -->

### 11.1 What landed

| commit | what |
|:--|:--|
| `27cc695f78` | golib: `View` (three overloads), `CopyInto` (slice source, string source), `CopyFrom` on `@unsafe.Pointer`; 20 GolibTests arms |
| `a993b432a0` | `ArrayPointer<T>.Of` hands the pointer to the typed door (10.6); the three `array<>` predicates tied by comments (10.1) |
| `cab7b3a2d1` | converter: the emission of 4.1 and 4.3, and the `FromRef` rule of 4.6 |
| `5d6eac352a` | scope fix 1: the same-element alias conversion keeps `array<T>.AliasPointer` |
| `c24cb00630` | the behavioral project `ArrayPointerViewDoors` (25 lines, linux-exclusive); two goldens re-baselined |
| `a0c43a357b` | scope fix 2: the door replaces the copying span and nothing else |
| `53f22a53ce` | the rule line in the converter rules; the 25-shape probe stored |
| `2c91fe4748` | five hand-owned `runtime` files declare their own unsafe code; a guard (11.5) |
| `961d63313b` | the hand-owned registry `value.cs` takes the door at its four sites, eight lines |
| `a37a986860` | the regen, 13 lines in 4 files of `runtime`; the repoguard guard of 4.7 |

The emitted forms, as the converter writes them:

    (*A)(p)[lo:hi]              @unsafe.Pointer.View<E>(p, lo, hi, N)
    (*A)(p)[lo:hi:max]          @unsafe.Pointer.View<E>(p, lo, hi, max, N)
    (*A)(p)[lo:]                @unsafe.Pointer.View<E>(p, lo, N)              A a named array
    copy((*A)(p)[lo:], src)     @unsafe.Pointer.CopyInto<E>(p, lo, N, src)     A a named array
    copy(dst, (*A)(p)[lo:])     @unsafe.Pointer.CopyFrom<E>(dst, p, lo, N)     A a named array

### 11.2 Where the cut departs from sections 4 and 10

1. **Three `View` overloads, with `long` bounds, where 4.1 has one.** Go's panic text differs by form (two
   indices, three indices, the open form), and one door could not tell which the program wrote. The two-index
   form passes no `max`; the door uses `N`.
2. **The pointer is resolved first and `lo` is applied per arm.** 4.2 moves the pointer by `lo*size` and then
   classifies it. Resolving first lets V1 answer a window by element index and lets a pointer to the array by
   its own number (pd, pf of 10.4) reach V1.
3. **A view past the end of the Go array the pointer resolved to is refused by name.** 4.2 clamps the capacity
   to the array's remaining elements. The cut clamps the capacity and refuses a LENGTH past the end, because Go
   has no answer for that program. The copy doors refuse the same way and copy nothing (ruling 6, 10.3).
4. **`CopyInto` refuses a snapshot destination by name.** A copy into a snapshot is a write nobody can read.
   The door sees this write because the open form under `copy` is its own emission. A bounded view under
   `copy` still hands out a snapshot and cannot know it will be written: D8, ruled (a).
5. **A snapshot's capacity is its length** (V4). Go's capacity would let an `append` write into memory the
   snapshot does not have.
6. **A pointer made by `unsafe.Add` from Go storage takes the anchored native window.** It carries its root, so
   the window keeps it. This is arm P's mechanism, already in the interior-pointer cut; the door reaches it.
7. **The pointer-to-array arm reads `ж<array<E>>` only.** A box of a named array wrapper takes the snapshot until
   V2 lands.
8. **The conversion-time note "COPIES the memory it views" no longer fires at a door site.** Until V2 lands, a
   write through a view of Go storage of another type takes the snapshot and is lost without a note. That is the
   state of pa, pb, pc and pe in 10.4, and it is what V2 is for.
9. **The repoguard guard of 4.7 was made to fail by text**, not by a converter with the `FromRef` rule removed:
   one of `internal/abi/type.cs`'s two spans rewritten as a door over its `FromRef` mint; the guard named
   `type.cs:230` and the row's count. The converter test `arrayPointerViewDoor_test.go` is where the rule itself
   was switched off and the test went red.

### 11.3 Two scope defects, both mine, both found before any corpus emission

The door's hook sits ahead of every route that renders a pointer-to-array conversion whole, and it took a
sliced conversion away from three of them.

- **The same-element alias** (`(*[N]T)(unsafe.Pointer(p))`, `p` a `*T`): found by reading where
  `os_windows_test.go:243`, `:340` and `:730` land. It would have moved `reparse_windows.cs` :65 :89,
  `runtime/iface.cs` :248, the three test lines and two behavioral files. Fixed in `5d6eac352a`. So those three
  test lines keep the alias route, and 10.4's open question about their arm is closed: they never reach a door.
- **Identity, two array types of one shape, and the native field view**: the no-regression re-transpile at
  `c24cb00630` moved one golden I had predicted would not move (`PointerCastSliceRange`, an identity
  conversion). A probe of 25 sliced shapes under both converters then read 22 changed, 14 of them not the
  copying span (identity 9, one shape 3, field view 2). The field view would have been wrong and not only
  different: its helper resolves the field's Go offset for a native root. Fixed in `a0c43a357b` with one
  predicate, `arrayPointerConversionKeepsItsRoute`, built from the predicates the four routes themselves use.
  After it: 8 changed, each the copying span; 17 byte-identical.

The lesson is a rule line in `.claude/rules/converter.md`: a hook ahead of existing routes is scoped by
converting every sibling shape with the base converter and the cut before the first commit, and it declines
through each route's own predicate.

### 11.4 Predictions, scored as worded

| | prediction | score |
|:--|:--|:--|
| P5 (section 6) | 11 lines in 5 files | **Corrected twice BEFORE any emission ran, then measured.** First (23:05Z): registry `value.cs` is a hand-own, so the converter writes its `.cs.auto`; 7 lines in 4 files. Second (23:47Z): a function whose only spans become doors loses `unsafe` on its declaration line; **13 lines in 4 files, `value.cs.auto` 8**. MET as corrected, on linux/amd64, windows/amd64, darwin/amd64 and linux/arm64: `heapdump.cs` 5, `iface.cs` 2, `mbitmap.cs` 2 on every target, `windows/os_windows.cs` 4 on windows. No other `.cs`, 0 `GoPositionMap` lines, `internal/abi/type.cs` and the alias sites unmoved, the base arm reproduces the committed file. |
| P5's added falsifier "a .csproj moving" (23:47Z) | none moves | **FIRED.** `runtime/runtime.csproj` moved `AllowUnsafeBlocks` true to false on linux/amd64, windows/amd64 and linux/arm64. See 11.5. |
| behavioral (23:47Z) | the full re-transpile moves no file beyond the two re-baselined goldens | **MISSED**: one file, the second scope defect of 11.3. After the fix: NO REGRESSION across 1067 packages (MET, 00:35Z). |
| P9 | W's three lines, pd and pf print Go's answers; pa pb pc pe still print zeros | **MET** (`door-a-1115/kits/`). |
| P11 | ruling 6's refusals carry the remaining count and copy nothing | **MET** by the GolibTests arm `TheCopyDoorsRefusePastTheEndOfAGoArrayByNameAndCopyNothing`. |
| P1 | arms A B E G H I K L N O P and C C0 print Go's answers; M's first line lands | **MET without N**: A B C E G H I K L O P C0 print Go's answers; M reaches its index store and stops there at the floor's refusal (door B). **N prints false**: it is V2's arm and is scored with V2. |
| P8 | no banked row changes verdict | **NOT SCORED.** No banked row was swept at this tip. It is stop line 4, read at the merge result. |
| P2, P3, P6, P7, P10, P12, P13 | doors B and C, V2, D8 (b) | not in this cut |

Arms D F D0 F0 M0 R S T are doors B and C and stop at named panics, as before the cut. Q answers length 0, the
floor's row.

P8's reading of the registry lines is corrected: `value.cs` :315 and :348 are a byte view over `[]uint16`
storage, so they are V2's case and not V1. All four sites read through the view and none writes, so the
snapshot answers them until V2 lands.

### 11.5 The project-file miss and its remedy

`<AllowUnsafeBlocks>` is the converter's own emission holding an unsafe function, OR a
`[module: go.GoRequiresUnsafe]` declaration in the package. The six functions the doors rewrote were the only
converted unsafe code in `runtime`, and five hand-owned files there used unsafe code with no declaration:
`memmove_impl.cs`, `linux/mem_linux_impl.cs`, `linux/stubs2_impl.cs`, `windows/signal_windows_impl.cs`,
`darwin/libccall_impl.cs`. They compiled only because converted code beside them happened to be unsafe.

I predicted "no .csproj moves" from the lines the change rewrites, and never counted what those lines were
holding up for the rest of the package. Posted alone before anything was applied; remedy accepted (00:46Z).

- `2c91fe4748`: the declaration on the five files; repoguard `TestHandOwnedUnsafeIsDeclared` and its planted
  control; the rule in `.claude/rules/corpus.md`. Census: 43 hand-owned production files use unsafe code in 7
  packages; `runtime` was the only package without a declaration.
- The arm, at `961d63313b` with the cut converter: a seeded `-stdlib runtime` reconvert leaves `runtime.csproj`
  byte-identical to the seed on all four targets. `runtime` builds at Release, `GoTargetOS=linux`, with the
  regen applied: 0 errors.
- Negative control: the same build with `AllowUnsafeBlocks` false is red, 2 errors, both CS0227, at
  `linux/mem_linux_impl.cs(123)` and `linux/stubs2_impl.cs(66)`.

Two statements of my 00:45Z post were wrong and are corrected here and in the 01:19Z post. darwin/amd64's
project file does NOT move: a named pointer type there (`machVMRegionInfo`) keeps the package unsafe by itself,
so a three-target merge would have kept `true` in the committed file, and the hole was real for a per-hunk
apply and for any single-flavour regen. And the first CS0227 is not at `memmove_impl.cs`: its unsafe code is in
blocks inside bodies, which the compiler reports only after the declaration errors.

Named, not applied: `runtime`'s `.cs.auto` siblings `mcleanup`, `mfinal` and `runtime2` differ from the seed
under the reconvert, and the committed registry `value.cs.auto` differs from the base converter's emission.
Both are stale review siblings and were so before this seat.

### 11.6 Two findings that are not door A's

- **The nil array pointer's length is door C's, not door A's.** The floor's BOARD row of 2026-11-14 ("a nil
  array pointer made from a number carries no length") says "cure in door A". Door A emits views only. The kept
  pointer `(*[N]T)(p)` is still `@unsafe.Pointer.To<array<T>>(p)`, and the cure needs `N` handed over at that
  conversion: door C's unnamed form (P7, D5: 84 lines in 28 files). Corrected by a BOARD append of this date;
  the row stays OPEN, owner P5, and is .5 with door C unless the sqlite-cs demo reaches it (ruled 01:20Z).
- **A write at the address of a named array that has not been used yet is lost.** Found while rerunning the
  kits; it is on master and independent of the doors. `type small [8]byte; var local small;
  *(*byte)(unsafe.Pointer(&local)) = 41` prints 41 in Go and 0 converted, in five shapes, with both controls
  right (`door-a-1115/lazy/`). The box's two pin paths box a copy of the wrapper, and the lazy backing is
  allocated on the copy. BOARD row of this date; ruled into .4 as its own golib seat directly after this one
  (01:19Z).

### 11.7 What this section does not show

- Nothing ran on windows or darwin. Their footprints are emissions on linux under the target's `GOOS`/`GOARCH`;
  the registry package was BUILT for the windows target, and no windows row was run.
- No banked row was swept at this tip (stop line 4). The windows flavour of `os` and of the registry package,
  `runtime`, and the reflect consumers are read at the merge result.
- V2 is not in these commits. Until it is, pa pb pc pe and N are red, and `sqlite_windows.go:5802` loses its
  write as 10.4 describes.
- The repoguard door guard was made to fail by text (11.2, item 9).
- No cost was measured: a view's allocation, the anchor per native window.
- `src/go2cs.slnx`, the binary-compat guard and the full converter suite were not yet run at this tip when this
  block was written. They are run before the announce, and their readings go in the announce and in V2's block.
- The sqlite-cs demo was not re-run past W5b. That is G's arm (section 5).

## 12. Amendment 2026-11-15 (P5): V2, the anchored window over Go storage of another type

The last commits of door A's seat, as ruled (22:39Z): the eleven commits before them stand without these two.
Code: `a7490fcee7`. Stop line 1 was not hit.

### 12.1 What V2 is, as cut

A view of a Go value that is NOT an array of the element type, where something holds the value still, is a
native window over the value's own bytes, anchored on the box that owns the pin.

- **Who holds it.** One new internal interface, `IHeldStorage.HeldBytesFrom(address)`, which every pointer box
  implements: the bytes this box holds still from `address` to the end of the storage it pinned, or 0. Three
  facts must hold or the answer is 0: the box owns a pin; the box is pinned AT that address (the registry's own
  validate-on-read, `IsPinnedAt`); the pinned object is a managed array whose end is known.
- **Who is asked.** For a number: the box the registry resolves it to. For a pointer that carries its own box
  (`unsafe.Pointer(&x)`): that box, and its number is taken there, which is the pin moment.
- **What is cut.** `slice<E>.OverNativeMemory(address + lo*size, count, min(capacity, remaining), anchor)`, the
  anchor being `NativeSliceAnchors<E>.For(holder)`. The anchor keeps the box; the box's pin is released by its
  finalizer once the box is unreachable. So the storage stays where it is for as long as any slice cut from the
  view lives.
- **Where it ends.** At the end of the pinned storage. A view or a copy that would pass it is refused by name
  ("would pass the end of the Go value p points to (K elements of it remain)"), as V1 refuses past its array.
  A view at a FIELD of a struct has room to the end of the struct, the allocation Go's pointer rules allow.
- **What is left for V4.** A value nothing holds still: one that holds a reference. Its pointer's number is an
  order token and never an address, so it is not viewed as memory; the copy reads through the box.

`unsafe.Slice` is not changed (decision D6). Public surface: no new public member; `IHeldStorage` is internal.

### 12.2 Acceptance

| | arm | reading |
|:--|:--|:--|
| stress (section 5, stop line 1) | `TenThousandRoundsOfWriteCollectReadKeepEveryViewOnItsValue`: 10,000 rounds; a `uint64`, a `uint32` array and structs of scalars; write through the views, force a compacting collection, read through the variables and the views; second mode, the variables go out of use and the views alone keep them | right in 10,000 of 10,000 rounds, 1 min 28 s; the unheld control array moved in 9,971 rounds, so the rounds could move a value |
| made to fail (floor item 13) | `WithoutItsAnchorTheSameWindowDoesNotKeepTheValue`: the same window cut with no anchor | the value is collected. And with the door's anchor replaced by null, the stress arm is red at round 0 ("a view did not keep the value it views"); restored byte-identical |
| P10 | arm N, and pa pb pc pe of 10.4 | see 12.3 |
| GolibTests, whole suite, Release, linux | | 2787 passed, 0 failed, 49 skipped |

### 12.3 P10 scored

P10 as worded (sixth set, written 2026-11-14 22:40Z): with V2 added, pa, pb, pc, pe and N print Go's answers,
and the stress arm is right in 10,000 of 10,000 rounds. **MET.** The two kits were rebuilt against the tree at
`a7490fcee7` and run one arm per process; their emission is the one of 11.4, since V2 changes no converter line.
Outputs: `data/array-pointer-doors/door-a-1115/v2/`.

| arm | before V2 (`a37a986860`) | with V2 | Go |
|:--|:--|:--|:--|
| N (NativeViewShapes) | `false` | `true` | `true` |
| pa (prng) | `0x0 0x0 0x0 0x0` | `0x61707865 0x3320646e 0x79622d32 0x6b206574` | the same |
| pb | `0 0 0` | `7 9 3` | the same |
| pc | `0x0 0x0 0x0` | `0x61707865 0x3320646e 0x6b20657479622d32` | the same |
| pe | `0x0 0x0` | `0xf905d5d9 0xd763a120` | the same |

`prng`'s six lines are now Go's six lines (`diff` against `amend-1114/prng/go.out`: empty). The other 23 arms of
NativeViewShapes print what they printed before V2: the two outputs differ in N's line alone. With N, P1 of 11.4
is met for every arm it names.

The seven arms of `door-a-1115/lazy/` print the same with V2 as without it: a named array whose address is
taken before its first use still loses the write, because the box V2 asks is the box of that row.

### 12.4 What stays a snapshot or a lost write after V2 (D8, ruled (a): a BOARD row)

- A Go value that holds a reference: a copy. `CopyInto` refuses it as a destination by name; a bounded view
  under `copy` writes the copy and the write is lost, with no note at conversion time since the doors (11.2,
  item 8). No site of this kind is known in libc, sqlite or std.
- An operand that holds a `FromRef` mint: the copying span, two lines of `internal/abi/type.cs`, read only.
- A named array whose address is taken before its first use: the BOARD's row of 2026-11-15, its own seat. V2's
  named-array arm writes an element first, so it does not meet that row.
- `sqlite_windows.go:5802` (10.4) is V2's case and is cured by it on the shape measured here. It was not
  observed in the sqlite-cs demo itself.

### 12.5 What this section does not show

- Nothing ran on windows or darwin. V2 rests on the CLR's pinned handle and on the registry, which are the same
  code on every target, and that is a reading.
- The stress arm forces collections on one thread. No arm takes a view on one goroutine while another moves or
  drops the variable.
- No cost was measured: one anchor object per V2 view, and a pin that lives as long as the view.
- The `prng` program is libc's `memcpy` shape. sqlite's own generated code was not converted or run here.

## 13. Amendment 2026-11-15 (P5): the review of V2, its three fixes, and what the first one found

COORD's review of V2 (`7765cbe0f8..1fa8b60636`, posted 03:01Z) returned SHIP-WITH-FIXES: two fixes before door A
seats, one keep-alive, three BOARD rows. This section is what was changed for it, on top of the merge of master at
batch 307 (`38ccbfd93e`). Code: `d133673c65`.

### 13.1 Fix 1: storage a Go array no longer has

**The gap.** A Go array's box pins the array's elements once (`pinnedArrayData`, `m_pin ??=`) and answers that
address from then on. A whole assignment gives the array new elements: the converter emits `a = b` as
`a = b.Clone()` and `a = [4]uint32{…}` as a fresh array, and both replace the backing the `array<T>` holds. The
pin still holds the old elements, and the registry still resolves the old address to the box. `HeldBytesFrom`
compared the address with the pin's and nothing else, so V2 cut its window over the old elements: reads were of
what the variable no longer is, and every write was lost without a word. The replacement is real (the review
asked): `ArrayValueCopySites` and the kit below show the emission.

**The fix.** The box answers for storage only when the pinned elements are the array's own now
(`ReferenceEquals` of the pinned object and the array's `Source`); else it holds nothing, the door falls to V4,
and what V4 does is known: a view is a copy and a copy door refuses to write by name. For a pin-only hold (a
scalar, a struct, a field or an element) the pinned object must contain the box's own slot, which is a check:
nothing replaces that storage.

**Red first.** `AViewAtTheNumberOfAGoArrayAssignedSinceIsNoWindowOnItsOldElements`: red before the fix (the view's
capacity was 16, a window on the old elements), green with it (capacity 4, a copy; `CopyInto` at the number and at
an address inside the array both refused by name).

**What the fix does not cure, and what the measurement found.** The fix stops the door from writing where the
variable is not. It does not make the address name the variable again: the pin is still of the old elements. The
kit `data/array-pointer-doors/door-a-1115/reassign/` takes an array's address, assigns the array whole, then
writes or reads through the address, ten ways. Against the tree with the fix, eight print something other than
Go:

| arm | Go | converted |
|:--|:--|:--|
| local, write through `*(*uint32)(p)` | `41` | `1` |
| the same by number | `42` | `1` |
| assigned from another variable | `43 9` | `9 9` |
| a named array | `44` | `9` |
| a package variable, by number | `45` | `5` |
| a struct field | `46` | `1` |
| a read through the address | `7` | `0` |
| `(*[4]uint32)(p)[0] = 47` (the same element type) | `47` | `47` |
| a byte view (door A, V2) after the assignment | `48` | `1` |
| control: never assigned whole | `49` | `49` |

Seven of these do not pass through a view door at all (`@unsafe.Pointer.To<T>(p).Value`), so they are not door A's:
by reading, they are on master (the pin code is not changed by this seat). The run was not repeated on master.

The cause is wider than pins. `data/array-pointer-doors/door-a-1115/reassign-plain/` has no `unsafe` in it: a
slice of an array, a pointer to an element, and a slice of a field or of a package variable, each taken before a
whole assignment of the array. Seven of its nine lines differ from Go:

| arm | Go | converted |
|:--|:--|:--|
| `s := a[:]; a = [4]int{1,2,3,4}; s[0]` | `1` | `0` |
| then `s[1] = 20; a[1]` | `20` | `2` |
| `p := &b[0]; b = …; *p = 30; b[0], *p` | `30 30` | `1 30` |
| `q := &c; c = …; q[0] = 40; c[0]` | `40` | `40` |
| `set(&c)` | `50` | `50` |
| `t := d[:]; d = other; t[0] = 60; d[0], other[0]` | `60 9` | `9 9` |
| a field, `u := h.arr[:]; h.arr = …; u[0] = 70` | `70` | `1` |
| a package variable | `80` | `1` |
| assigned through `*r = …` with a slice taken before | `90` | `1` |

In Go an array variable is its storage: an assignment copies elements into it, and everything that names the
storage (a slice of it, a pointer to an element, its address as a number) still names the variable. Here an
array variable holds a reference to a backing, and an assignment swaps the backing. This is its own wall, posted
with its size on 2026-11-15 and recorded on the BOARD; it is not worked in this seat.

### 13.2 Fix 2: an address inside a Go value

**The gap.** `(*[1<<30]byte)(unsafe.Add(unsafe.Pointer(&v), 4))[:n]`. The address is 4 bytes inside `v`'s pinned
storage, and the registry knows a box by the address it was pinned at, so the address resolves to nothing and the
door took V3, the arm for C memory. V3 keeps the pointer's root, so the storage stayed put, and it bounds the view
by Go's capacity, N less the low bound: 1 << 30 bytes over a value of 16. A write past the value went into
whatever the managed heap held next. V3 is part of this seat, so this was not on master.

**The fix.** Before V3, the door asks whether the pointer was derived from a Go value
(`NativeDerivation.HeldSource`): from what the pointer keeps (`Kept`), a chain of derivations is walked to its
first managed box, as the order token of a derived box walks it (`NativeDerivation.Token`), and through a view as
well: an element of a native-backed slice, or a native array window, that keeps a root was cut over that root
(`INativeViewAnchor`). That box answers for the storage it holds still from its first byte to its end
(`IHeldStorage.TryHeldExtent`, with fix 1's check). Then:

| the pointer | the door |
|:--|:--|
| derived from a Go value that holds its storage, address inside it or at its end | V2 at that address: a window to the end of the storage, anchored on what the pointer keeps; past the end is refused by name ("would pass the end of the Go value p points to (K elements of it remain)") |
| the same, address before the storage or past its end | refused by name ("p is outside the Go value it was derived from") |
| derived from a Go value that holds nothing still (it cannot be pinned, or its storage was replaced, 13.1) | V4: a view is a copy read at the address, and a copy door refuses to write by name |
| not derived from a managed box: C memory, or a number that carries nothing | V3, as before |

**What the last row can still do.** A pointer made from a number held in a variable carries nothing
(`unsafe.Pointer(n)`; Go's own rules call that conversion invalid when n was an address of Go memory), and C
memory has no end known here. For both, the view has Go's capacity and a write past the real memory is the
program's, as it is in Go. Arithmetic written in one expression, `unsafe.Pointer(uintptr(p) + n)`, is emitted as
`unsafe.Add(p, n)` and carries p's root, so it is bounded. The pin-extent index (H's seat) exists in cgo
processes only and is not used.

**Red first.** Four arms, each red before the fix on its first assertion, which is of the capacity, so the red run
wrote nothing out of bounds: `AViewAtAnAddressInsideAGoValueEndsWhereTheValueEnds` (the review's shape over a
struct of scalars: capacity 1 << 30 before, 12 after), `…InsideAGoArrayOfAnotherElementEndsWhereTheArrayEnds`
(the array-data pin), `AViewAtAnAddressPastOrBeforeAGoValueIsRefusedByName`, and
`AViewAtAnElementOfAViewOfAGoValueEndsWhereTheValueEnds` (an element of a view of the value, found while
writing the fix: the same overrun through `&view[1]`). One control, green before and after:
`AViewAtAnAddressInsideNativeMemoryKeepsGosCapacity` (C memory keeps Go's capacity, through `unsafe.Add` and
through an element of a view).

### 13.3 Fix 3, the BOARD rows, and the readings

`CopyInto` (both forms) and `CopyFrom` keep `p` alive until the elements are copied (`GC.KeepAlive(p)` after
`copy`). The review's items 4 to 6 are three BOARD rows of 2026-11-15: the value's own Go and C# sizes are not
compared (only the element's), no alignment check, no two-thread arm.

Readings, at the code commit on one cloud linux/amd64 VM, Release, `GoTargetOS=linux`:

| | reading |
|:--|:--|
| `ArrayPointerViewDoorTests` | 36 of 36 (30 before; five of the six new arms red before the fixes, the C-memory control green) |
| GolibTests, whole suite | 2793 passed, 0 failed, 49 skipped (2787 before the six arms) |
| the stress arm and its control (12.2) | in the suite above, green; the made-to-fail of 12.2 was not repeated: `HeldWindow` and the anchor are not changed |
| NativeViewShapes (24 arms), `prng` (6), `lazy` (7), rebuilt against the fixed tree | each output byte-identical to the one of 12.3 |
| the behavioral program `ArrayPointerViewDoors`, built by path | 25 lines, equal to `go run` |
| repoguard (mint census, public surface, provenance) | green; the mint census counts are unchanged: `Window` one site, `HeldWindow` one site |

The gates at the merge result (the seeded `-stdlib runtime` reconvert on four targets, CNR, the by-path builds
and the converter suite) are scored in the announce of this commit, not here.

### 13.4 What this section does not show

- Nothing here ran on windows or darwin before the hosted runs.
- The interior-address arms take one derivation step, or two through a view. A longer chain is the same walk, and
  that is a reading. The walk stops after 64 hops and then answers as for C memory.
- A field reference over a struct in C memory, and a root that is not a pointer box, are answered as C memory
  (V3). No arm reaches the door with one.
- The stale case of 13.1 is checked per view. A view cut BEFORE the assignment is a window on the old elements
  and stays one: nothing tells it the array has other storage now.

## 14. Amendment 2026-11-15 (P5): the lost write at a named array's address, option 1, and two lines owed to section 13

This is the finding of 11.6 ("a write at the address of a named array that has not been used yet is lost"),
ruled into .4 as its own golib seat directly after door A (01:19Z). The doors emit nothing at these sites; the
section is here because this record named the finding and its kit. Code: `ddba5cc1e7`.

### 14.1 What was wrong, and the change

A named array, `type small [8]byte`, is a wrapper struct go2cs-gen writes. The wrapper holds one field and
fills it with the array's backing on first use. A pointer box's two pin paths (its conversion to a number and
to a raw pointer) read the box's value as an `IArray`, which for a value type is a boxed COPY of the wrapper,
and `pinnedArrayData` pinned that copy's `Source`. On a wrapper nothing had used, reading `Source` filled a
backing of the copy's own. The address handed out was of an array the variable never reads; the variable made
its own backing on its next use, and the write was lost without a panic.

The element pointer `&p[i]` has had a gate for this class since August (`arrayView`, `publishArrayBacking`):
fill on the boxed copy, then copy the wrapper back over the box's real storage, under the box's lock.
`pinnedArrayData` now goes through that gate before it pins, when the pointee is a type whose backing is filled
on first use (`s_publishArrayBacking`) and the box holds no pin yet. `publishArrayBacking` takes the storage as
a `ref T` parameter: the element door passes `Value`, which panics on a nil pointer as Go's `&p[i]` does, and
the pin passes `ValueSlot`, after the operators' own nil test. The pin's check (`AnyElementArrayViewCheck`)
accepts a view of any element, where the element door asks for one element type. golib only: no public surface,
no converter change, no emitted line moves.

### 14.2 Red first, and the readings

`NamedArrayPinPublishTests` has one arm per shape, each as the converter spells it, and each reads the variable
after the write, never the pointer the write used: a local, a package variable by number, a struct field,
`new(small)` by number, libc's memcpy destination, the raw-pointer conversion, and the number taken twice. Seven
were red before the change and the two controls green (a named array used before its address is taken; an
unnamed array), as predicted before the run. All nine are green with it.

The kit `door-a-1115/lazy/` is now the behavioral project `NamedArrayAddressWrite`, output compared with
`go run`. It imports `fmt` and `unsafe` only, so it is not exclusive to a platform.

| reading (linux/amd64, Release) | at `996ce6c9bf` | with the change |
|:--|:--|:--|
| `NamedArrayPinPublishTests`, nine arms | 7 red, 2 green | 9 green |
| the door, floor and publish classes together | not read as one set | 58 of 58 |
| whole GolibTests | 2793 passed, 0 failed, 49 skipped | 2802 passed, 0 failed, 49 skipped |
| `NamedArrayAddressWrite` against `go run` | la, lb, lc, ld and le differ (`door-a-1115/lazy/converted.out`) | seven lines equal (`converted-option1.out`) |

Nothing here ran on windows or darwin before the hosted runs. The local gates (by-path builds, the repository
guards, the converter suite, CNR) are scored in the announce of this commit, not here.

### 14.3 What option 1 leaves

- The gate's copy-back is a plain write under the box's lock, and the wrapper's own getter publishes without
  that lock. A goroutine that first uses the variable directly at the moment another takes its address for the
  first time can leave the two on different backings. The element door has the same window. Option 2 closes it
  (an interlocked publish of the wrapper's one holder field into the slot, in the element gate and in the pin,
  with a two-thread stress arm): the seat after door B.
- A field or element reference is a box minted per take. Two takes of one field's address on two goroutines hold
  two locks. Each publish is still of the same storage, and the second finds the first's backing there unless
  the two interleave inside the window above.
- A named array ASSIGNED whole after its address was taken keeps its old pin (13.1). That wall is routed to
  another lane (ruled 03:46Z), with the kits of 13.1.

### 14.4 Two lines owed to section 13

- 13.4 says the walk from a derived pointer to its Go value "stops after 64 hops and then answers as for C
  memory". Said in full, as the review asked (04:16Z), and as measured the same day: each `unsafe.Add` over a
  pointer that carries Go storage adds one hop, so a pointer advanced step by step reaches the cap. At an
  interior address of the value the view is bounded by the value through 61 steps and takes Go's capacity (N
  less the low bound, nothing bounding it by the Go value) from 63. At the value's own first address the
  registry answers and the bound holds at any depth. The same chain keeps 55 bytes per step for as long as the
  last pointer lives, and a comparison of such a pointer recurses once per step: at 1,000,000 steps the
  process ends in a stack overflow. Those two are on master since batch 305. The finding is its own BOARD row
  of this date with its reach and its remedy, ruled into .4 as the seat cut from this one (04:39Z); the probe
  and its readings are `door-a-1115/chain/`.
- The linux solution build read red at door A's earlier tip (72 error lines in the 10 windows-native behavioral
  members) and reads the same at master (COORD's control run, 03:58Z). It is a first reading of that leg and is
  not door A's; the windows solution build is the gate.

## 15. Amendment 2026-11-15 (P5): a pointer advanced step by step stays one hop from its Go value

This is the finding of 14.4 and of the BOARD row of this date ("a pointer advanced step by step with
`unsafe.Add` over a Go value keeps every step"), ruled into .4 as its own golib seat, cut from the lost-write
seat and before door B (04:39Z). The doors emit nothing here; the section is in this record because door A's
bound by the Go value (13.2) is one of the three things the chain broke. Code: `fcc2a35455`.

### 15.1 What was wrong, and the change

`unsafe.Add` over a pointer that carries Go storage mints a native box at the new address, and that box retained
the box the pointer carried (`NativeDerivation.Kept`). The pointer a second `unsafe.Add` receives carries the
first one's box, so `for ... { p = unsafe.Add(p, 1) }` built a chain one box per step. Three things read the
chain. The collector kept every box (55 bytes a step). `NativeDerivation.Token`, which a comparison and a hash
ask, made one call per box and overflowed the stack at 1,000,000 steps. `NativeDerivation.HeldSource`, which a
view door asks for the Go value's end, stops after 64 boxes, so a view at the walked address took Go's capacity
and lost the value's end from 63 steps.

The change is in what a derivation retains. A native box that owns no pin holds its source and nothing else:
the source is what keeps the storage where the box's address says it is. A box derived from such a box now
retains that box's SOURCE in its place (`INativeRooted.NativeRetainedInItsPlace`, answered by `NativeBox<T>`;
`Kept` passes over such boxes in a loop). Every step of the walk is then one box from the Go value, and the box
of the step before is garbage as soon as its pointer is. `Token` is a loop that sums the distances; it was a
call per box. The 64-box bounds in `HeldSource` and in the dereference census stay as guards.

A box is NOT passed over when it owns a pin (its pin is released when it is collected, so it must be kept),
when its number is an order token or 0 (there is no address to measure a distance from, and token arithmetic
has its own route, which mints no box), or when it is any other box kind. The rule sits in `Kept`, so it covers
every derivation that asks there: `unsafe.Add` in both forms, a typed conversion of a native pointer, a field
of a struct in native memory at a known Go offset, `unsafe.Slice`, and the view doors' anchors.

golib only (`ж.NativeDerivation.cs`, `ж.NativeBox.cs`, `ж.FieldRefBox.cs`). The interface is internal and the new
member has a default body. No public surface, no instance state added to a box, no converter change, no emitted
line moves.

### 15.2 The order token is the same at every step

Asked at 04:39Z: does the order token stay identical however the address was reached? It does, before and
after. A derived box's token is its source's token plus the distance from the source's address to its own.
Along a chain that is the head's token plus the sum of the steps, and the sum is the distance from the head to
the last address, which is what one box retaining the head answers directly. The three ways a walk can end (a
managed box with an address, a managed box whose number is a token, anything else) each give the same number
either way.

The arm that holds it walks 1,000 steps over a `[1024]byte` and asserts at EVERY step that the walked pointer's
token is the token of `&b[k]`, is the token of `unsafe.Add(p, k)` minted in one step, that the walked pointer
`==` both, hashes with both, and `!=` where it began. It is green at the parent commit and green with the
change: the identity did not move.

### 15.3 Red first, and the readings

`UnsafeAddChainTests` has 16 rows. Two of them ended the test host at the parent commit (a stack overflow in
`Token`), so they were run alone there.

| reading (linux/amd64, Release) | at `70abf5dd5a` | with the change |
|:--|:--|:--|
| what the pointer's box retains after 2, 10 and 1,000 steps | the box of the step before (3 red) | the element box the walk began at |
| the box of step 1, after 100 steps and a collection | alive (red) | collected |
| `==` after 1,000,000 steps | stack overflow, the host ends | equal, one hash, unequal to the next address |
| a chain of 1,000,000 boxes built link by link, compared | stack overflow, the host ends (104,681 `Token` frames) | equal; its token is the token of the place; `Kept` answers its head |
| view capacity one byte inside a 16-byte Go value, after 63, 65, 2,001 and 10,001 steps | 1,073,741,824 (4 red) | 15 |
| a typed walk of two steps from a view that owns its pin | retains the first step's box (red) | retains the view |
| controls: 1 step; 15.2's arm; capacity after 1 and 61 steps; C memory | 5 green | 5 green |
| `UnsafeAddChainTests`, 16 rows | 5 green, 9 red, 2 end the host | 16 green |
| `UnsafeAddCarryTests`, 5 arms (batch 305's) | 5 green | 5 green |
| whole GolibTests | 2802 passed, 0 failed, 49 skipped | 2818 passed, 0 failed, 49 skipped |

The probe of 14.4, run again with the change (`door-a-1115/chain/probe-after.out`):

| reading | before | with the change |
|:--|--:|--:|
| bytes kept after 10,000 steps over a Go value | 554,336 | none (the reading is -64) |
| bytes kept after 1,000,000 steps | 55,440,880 | none (the reading is -64) |
| a loop of 10,000 steps with one `!=` per step | 659 ms | 23 ms |
| the same, 20,000 steps | 2,094 ms | 13 ms |
| `==` after 100,000 steps | 6.6 ms | 13 microseconds |
| `==` after 1,000,000 steps | did not return | 23 microseconds |

The two loop timings were taken while another battery ran on the same box. They show that the cost no longer
grows with the square of the steps; they are not a benchmark. Over C memory the same loops read 2 ms and 4 ms.

Nothing here ran on windows or darwin before the hosted runs. The local gates are scored in the announce of this
commit, not here.

### 15.4 What this leaves

- A chain through a box kind that is not passed over. Read from the code, not measured: a field's address taken
  through a typed view whose Go layout is not known is a field reference over the view, and a pointer made from
  it carries that reference. A loop of the form `p = unsafe.Pointer(&(*T)(p).f)` over such a type would add two
  objects per step. Where the layout is known the field is a native box at the field's Go offset, which asks
  `Kept` and stays one box from the Go value. No site of the first form is known.
- The two bounds of 64 stay. No mint builds a chain now, and a chain built link by link is passed over by `Kept`
  before `HeldSource` is asked. The dereference census walks what a box retains directly and would stop at 64 on
  a hand-built chain; it is a census, not a door.
- The two cases of 13.4 that no bound reaches are as they were: C memory, and a number that carries nothing.
- A view door's window, and a slice from `unsafe.Slice`, are now anchored on the Go value's own box where they
  were anchored on the last step's box. Each keeps the value's storage pinned as long as before, since the last
  step's box kept nothing else.
