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

**Amended 2026-11-16 (P5, COORD's ask of 08:38Z): the window is bounded, not zero.** A full-suite GolibTests
Release run read 5,784 bytes once in the 100,000-iteration window (H's W8 re-gate at master 2c1ce10b25). That is
far below one object per access (a box per access is at least 24 B x 200,000 = 4.8 MB), so it is a cost paid once,
and the instrument counts every byte the thread allocates, including what a tier change costs when the JIT moves
the loop inside the window. Measured on linux/amd64 at 2c1ce10b25: the loop's unoptimized body allocates 24 B per
call, so with on-stack replacement held past the window (`DOTNET_TC_OnStackReplacement_InitialCounter=50000`)
or off, the zero form read 24 and failed. CPU load (six busy processes) and three in-process allocating threads
did not reproduce the red in 15 probe runs. The arm now measures 1,000,000 iterations (2,000,000 accesses) and
asserts fewer than 65,536 bytes: eleven times the one-time cost seen, and below one 24-byte object per 700
accesses, so a per-access cost (at least 48 MB) still fails it. The control now asserts at least 24 bytes per
access. Readings: green 2 of 2 in each of four arms (plain, on-stack replacement late, off, tiering off), 3 of 3
under CPU load; the negative control (the store through the boxing door `Element<E>(...).Value` on the native
pointer) read 112,000,000 bytes and failed.

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

## 16. Amendment 2026-11-15 (P5): door B as cut: the element and array doors, the type's length, the range window

Door B is cut on `claude/p5-array-door-b`, from the chain seat's tip `04660ffaa6`. Code: `5244cfbe2b`. Regen:
`ef92f09825`. Behavioral goldens: `cf55010966`. Nothing above is rewritten; where this section and 4.5 or 10.5 differ, this one is later. Evidence:
`data/array-pointer-doors/door-b-1115/`.

<!-- Provenance. Rulings: claude/mailbox docs/phase4/inbox/P5/20261115T060752Z-COORD.md (F4 into this seat, red
     first, with cap and range checked; F1 its own small seat after option 2; F2 stays with door C, its census
     taken; F3, L1, L2 BOARD rows), 20261115T061431Z-COORD.md (F5 into this seat, cured with door A's window; the
     nistec sites stated; the call inside the pointer form folded if a few lines, else a row),
     20261115T062219Z-COORD.md (C6, the named whole door, into this seat; the byte-view line read). My posts:
     inbox/COORD/20261115T060046Z-P5.md (first scores and plan), 20261115T060725Z-P5.md (the findings on shapes the
     cut does not move), 20261115T061407Z-P5.md (F4 scored, F5 found), 20261115T063050Z-P5.md (C6, the byte view,
     F5 built). R's design: claude/r-reassign-census fa457b5424, section 8. Box: one cloud linux/amd64 VM, Go
     1.26.11, .NET SDK 10.0, Release, GoTargetOS=linux unless a line says otherwise. Converters: base = the
     converter built at 04660ffaa6, cut = the converter built at the code commit. -->

### 16.1 What landed

The emitted forms, as the converter writes them. `p` is an `unsafe.Pointer`, `A` an array type named or not,
`E` its element, `N` its length.

    (*A)(p)[i]                        @unsafe.Pointer.ElementRef<E>(p, i, N)         a `ref E`: read, store, compound, ++
    &(*A)(p)[i]                       @unsafe.Pointer.Element<E>(p, i, N)            the pointer to the element
    *(*A)(p), read for its value      @unsafe.Pointer.LoadArray<E>(p, N)             `(A)` in front when A is named
    *(*A)(p) = v                      @unsafe.Pointer.StoreArray<E>(p, N, v);
    len, cap of *(*A)(p) or (*A)(p)   N                                              where Go evaluates nothing
    for i := range *(*A)(p)           range(N)                                       where Go evaluates nothing
    for _, v := range (*A)(p)         range(@unsafe.Pointer.View<E>(p, 0, N, N))     door A's window

**golib** (`src/core/unsafe/unsafe.cs`, 138 lines added). Each door does Go's steps in Go's order: the nil check,
the index against `N` with Go's index panic, then the element. What the element is follows the four answers the
view doors already give, for a window of one element at `i`. `Window` was split so that the doors share its
classification (`Aliased`), and door A's 36 GolibTests arms are green over the split.

| what `p` points to | `Element`, `ElementRef` | `LoadArray` | `StoreArray` |
|:--|:--|:--|:--|
| V1: a Go array or slice of `E` (an element of it, or the array by its own number) | the array's own element box; an index past the end of the Go array is refused by name | N elements copied out | N elements copied in |
| V3: C memory, or a number that carries nothing | a native box at the address; `ElementRef` makes no box (10.5) | copied out | copied in |
| V2: a Go value of another type its pointer can hold still | a native box at the element's address inside the value, anchored on the box that holds the value still; past the value's end is refused by name | copied out | copied in |
| V4: a Go value nothing can hold still | Go's definition of the form, `(*E)(unsafe.Add(p, i*size))`, as the pointer conversion answers it | an exact copy | refused by name: the write would land in a copy |

`StoreArray` writes zeros from where a short source ends: the zero value of an array that was never given
storage has no elements, and Go's has `N` zeros. An `E` that holds references is refused by name by every door,
as it is by the view doors; the converter does not emit a door for one.

**The converter** (`src/go2cs/arrayPointerElementDoor.go`, and one call each in `convIndexExpr.go`,
`convStarExpr.go`, `convUnaryExpr.go`, `visitAssignStmt.go`, `convCallExpr.go`, `visitRangeStmt.go`). The doors
decline through door A's predicate, `arrayPointerViewOfBase`: the operand is an `unsafe.Pointer`, the element
holds no references, the conversion is none of the four routes that render it whole (11.3), and the operand is
no `FromRef` mint. A dereference is the load door only where Go reads it for its value. That is decided once
per file by a pass before conversion (`collectArrayPointerLoads`, called beside the escape analysis), so the
store form, the address-of form and the operand of a folded `len` never reach the load door. The value the load
door answers is its own copy and is not cloned again (`arrayCloneOperations.go`).

A named array takes every door. Its index and element forms are the unnamed ones, since `E` and `N` are the
type's. Its whole load is cast to the named type and its whole store passes the named value, which converts by
itself.

**Guards.** The converter test `TestArrayPointerElementDoorReplacesTheWholeArrayBox`, 35 arms.
`ArrayPointerElementDoorTests` in GolibTests, 20 arms. The behavioral project `ArrayPointerElementDoors`, 38
lines against Go, linux-exclusive like door A's (it maps native pages). Repoguard's
`TestArrayPointerViewDoors` reads the four new doors with the same `FromRef` rule as the view doors.

### 16.2 Where the cut departs from 4.5 and 10.5

1. **`LoadArray` and `StoreArray` are not `View<E>(p, 0, N, N, N)` copies.** 4.5 has them copy through the
   public view door. They call the shared classification directly, so that a store into a Go value nothing can
   alias is refused by name where the view door would hand out a snapshot and lose the write, and so that a
   refusal names the form the program wrote (`*(*[4]byte)(p) = v`).
2. **A Go array the pointer resolved to bounds the index.** 4.5 answers `at<E>(i)` for a managed array of `E`.
   The cut does, and refuses by name an index that is inside `N` and past the end of that array, as door A
   refuses a view past it (11.2, item 3). Go would read or write whatever lies behind the array.
3. **The length forms, the range window and the named whole forms are in this seat** by the three rulings in
   the provenance block. 4.5 has none of the first two, and gives the named whole forms to the wrapper of the
   result; the cut casts the load and passes the store.
4. **A named array's index, whole load and whole store take the door over any memory.** 4.5's floor refuses a
   named array over memory that is no array of its type. The forms the doors take never ask the floor now: they
   read and write elements of `E`. What still reaches the floor is the conversion KEPT (door C).

### 16.3 The length is the type's (F4), and the conversion ranged for its values is a window (F5)

Found by running the scope probe end to end against Go, on shapes the first cut did not move. Both were silent
wrong answers at the base converter, and both are older than the doors.

| Go | at the base converter | at the cut |
|:--|:--|:--|
| `len(*(*[4]byte)(p))`, `cap(...)` | `len(~To<array<byte>>(p))`: 64 over a `[64]byte`, 0 over a `uint64`, a nil-dereference panic over nil (Go answers 4 each time) | `4` |
| `len((*[4]byte)(p))`, `cap(...)` | `len(To<array<byte>>(p).Value)`: the same answers | `4` |
| `len(*(*small)(p))`, `small` a named array | the named floor's panic over other memory | its `N` |
| `for i := range *(*[4]byte)(p)`, `for range ...` | `range(len(~To<...>(p)))`: 64 rounds | `range(4)` |
| `for _, b := range *(*[4]byte)(p)` | a snapshot of the 64-element box | `range(LoadArray<byte>(p, 4))` |
| `len(*(*[4]byte)(get(p)))` | the box route | `len(LoadArray<byte>(get(p), 4))`: Go evaluates this one, and the call runs once |
| `for _, b := range (*[4]byte)(p)` | `range((To<array<byte>>(p)).Value)`: 64 rounds over a `[64]byte`, none over a `uint64` | `range(@unsafe.Pointer.View<byte>(p, 0, 4, 4))` |

The fold reads Go's own rule. A call the type checker folded to a constant evaluated nothing; a conversion is
no call, and a constant call (`unsafe.Sizeof`) is none either. The converter's older test for "the operand
holds a call or a receive" counts `(*[4]byte)(p)` and `unsafe.Pointer(x)` as calls, which is why its older fold
for a pointer to an array never reached these forms. That test is unchanged, because other arms read it; the
exact one is beside the door (`exprHoldsCallOrReceive`). The fold is the type's answer, so it holds for every
operand and every element, managed and named included.

The conversion ranged for its values is not a load. Go reads each element through the pointer as the loop
reaches it, so a store in the body is seen by a later round. That is door A's window. It is the closed form
`[0:N]`, because door A keeps the open form of an unnamed array on its older route (decision D4). The form
ranged for its index only already emits a range of `N` rounds over the pointer and is right.

**Census.** `len` or `cap` of a dereferenced or converted array pointer: 0 sites in the standard library with
its tests, in modernc.org/libc v1.80.1 and in modernc.org/sqlite v1.62.0. The pointer form ranged for values:
2 sites, both in `crypto/internal/fips140/nistec` (`p256.go:575`, `p256_asm.go:335`), both
`range (*[N][8]byte)(ptr)`. Their element is itself an array, which is a managed element, so door A's predicate
declines them and they keep their route. Both sit in a branch only a big-endian target takes (`cpu.BigEndian`,
`s390x`). The behavioral program `RangePointerArrayConversion` holds both forms: its first loop is the window,
and its second, over `(*[2][2]int)(q)`, is the kept route. Both print Go's lines.

### 16.4 Red first, and the readings

| reading (linux/amd64, Release) | before | with the change |
|:--|:--|:--|
| converter test, the 18 forms of 16.1's first four rows, the hooks switched off | 18 red | green; the hooks restored byte-identical (sha256) |
| converter test, F4's 5 arms, F5's 1, the named whole door's 2, each before its change | red, the older arms green in the same run | 35 of 35 green |
| GolibTests, the 20 door arms, with two planted regressions in one build | the fast path taken out: only the allocation arm red; `StoreArray` not clearing past a short source: only its arm red; 18 green | 20 green, door A's 36 green; `unsafe.cs` restored byte-identical (sha256) |
| stop line 7 (10.5): 100,000 stores and reads through `ElementRef` over C memory, after a warm-up pass | with the fast path out: 22,400,000 bytes, 224 a round | 0 bytes; the control (a Go array through its element's box) reads above 0 |
| `ArrayPointerElementDoors`, 38 lines | the base emission panics at its first line (`cannot view native memory as array<...>`) | 38 of 38 equal Go, the index panics' texts included |
| scope probe, 55 sibling shapes, base against cut | | 40 lines change, one for one, and 15 shapes keep their emission: 21 `ElementRef`, 3 `Element`, 9 `LoadArray`, 5 `StoreArray`, 1 line that is both (a whole store of a whole load), 1 folded `len`. Every base line of the 40 is the box of the whole array over an `unsafe.Pointer`: 35 unnamed, 5 named (`door-b-1115/scope-probe/`) |
| length probe, the shapes of 16.3, built and run | its 5 printed lines all differ from Go's, then a nil-dereference panic where Go prints 3 more | byte-identical to Go, 8 lines (`door-b-1115/lenprobe/`) |
| R's line `(*[16]byte)(v)[:4][0] = 48` in three operand forms, a named whole, an alias whole | | 6 of 6 equal Go (16.6) |
| whole GolibTests | 2818 passed, 0 failed, 49 skipped | 2838 passed, 0 failed, 49 skipped |
| the behavioral corpus re-transpiled (`check-no-regression`) | | five programs move, 12 lines, each a form the doors take; each prints Go's output from the new emission (16.5) |

Nothing here ran on windows or darwin before the hosted runs. The local gates are scored in the announce of this
commit, not here.

### 16.5 Predictions, scored as worded

| | prediction | score |
|:--|:--|:--|
| P2 | With door B: D D0 M M0 R S T print Go's answers. | **MET.** Each of the seven prints Go's answer at the cut, one arm per process (`door-b-1115/kits/NativeViewShapes.converted.out`). Of the 24 arms, 21 now equal Go. F and F0 stop at the named floor (a named array's conversion kept in a variable: door C) and Q answers length 0 (the floor's row), as they did at door A's cut. |
| P6 | Door B's corpus footprint is in `runtime`'s malloc files only: 34 stores, 2 address-of and 1 argument per target, plus the 6 dereference forms. Falsifiers: an index form outside `runtime`; a banked `runtime` row changing verdict. | **MISSED as worded; the index-form falsifier did not fire.** Four targets, the same on each (`door-b-1115/footprint/`): 39 lines in 6 converted files, no `GoPositionMap` line, no line of another kind, no `.csproj`. The 34 stores: MET, in the malloc files (the platform's `malloc.cs` 2, `malloc_generated.cs` 30, `malloc_stubs.cs` 2). The 2 address-of: MET in count, in `runtime/lock_spinbit.cs`, which is no malloc file. The 1 argument: MISSED, 0 lines. It is `sync/pool.go:93`; its element holds a reference, so the door declines it, and the corpus `sync/pool.cs` is hand-owned. The 6 dereference forms: 4 emitted, as 3 load lines (`internal/runtime/maps/runtime_faststr.cs` 2, `runtime/alg.cs` 1; 6 `LoadArray` calls) and 1 `StoreArray` line in `runtime/mfinal.cs.auto`, the review sibling of a hand-owned file, named and not applied. Of the other 2, `reflect/value.go:1622` is hand-owned (`reflect/value_impl.cs`) and `internal/runtime/maps/map_test.go:666` is a test file, which a `-stdlib` emission does not write (16.8). "In the malloc files only" is therefore wrong on three files, one of them outside `runtime`; the form outside `runtime` is a load, not an index. The second falsifier did not fire either: no `runtime` row was run locally, and the one banked row with a moved file, `internal/runtime/maps`, keeps its verdict (16.8). |
| P12 | The allocation arm: 0 bytes per element over C memory for 100,000 stores and reads after one warm-up pass; the V1 control reads more than 0. | **MET.** `AnIndexedLoopOverNativeMemoryAllocatesNothing` reads 0 bytes; its control reads above 0. Stop line 7 did not fire. |
| F4, F5, the named whole door (06:14Z, 06:30Z) | 0 corpus lines | **MET.** No added line of the four-target footprint holds a folded length, a range window or a named cast: the 3 load lines are unnamed arrays read for their value, and the `.cs.auto` line is an unnamed whole store. |
| the behavioral corpus | none made | Five programs moved and I had predicted none and no number: `ArrayCastDerefClone` 2 lines, `ExplicitDerefConversionStore` 4, `RangePointerArrayConversion` 1, `SwitchPointerSentinelCase` 2, `UnsafePointerReinterpret` 3. Each was built from the new emission and its stdout, stderr and exit code equal `go run`'s. Their goldens are re-baselined in `cf55010966`. |
| the hook's scope (06:00Z) | every changed line of the scope probe is the whole-array box over an `unsafe.Pointer` with a scalar element | **MET** at the final cut: 40 changed lines, each the whole-array box over an `unsafe.Pointer` with a scalar element at the base converter. The 06:00Z reading had 36 changed lines; the length forms and the named whole forms ruled in afterwards added the rest. |

### 16.6 For R's cut, and the byte-view line R reported

Both cuts edit `visitAssignStmt.go`. Door B's hook there takes `*(*A)(x) = v` only where `x` is an
`unsafe.Pointer` and the element holds no references, and it runs before the general path. Every other pointer
(a `*[N]T` variable, a field, a managed element) falls through to the assignment R's cut emits. Whichever cut
lands second merges the other.

The one named site R's census found in sqlite, `libc_musl.go:1495`
(`*(*uuid.Uuid_t)(unsafe.Pointer(dst)) = *(*uuid.Uuid_t)(unsafe.Pointer(src))`), is not a named array to the
type checker: on linux/amd64 `uuid.Uuid_t` is an alias of `[16]uint8`. The door emits it as
`StoreArray<uint8>((@unsafe.Pointer)dst, 16, LoadArray<uint8>((@unsafe.Pointer)src, 16))`. The behavioral
program holds that exact line and a truly named array beside it.

R's reassign kit reported `(*[16]byte)(v)[:4][0] = 48` emitted as a copying span, so the write was lost. That
is the defect door A exists for, and R's tip was cut from a master without door A. The converter at
`04660ffaa6` and this cut both emit `@unsafe.Pointer.View<byte>(v, 0, 4, 16)[0] = 48;` (for a pointer variable,
for a number converted in place, and through `FromPinnedBox` for `unsafe.Pointer(&g)`), and each prints 48 as
Go does (`door-b-1115/rprobe/`).

### 16.7 What the cut leaves, each a BOARD row of this date

- **F1, the native field view indexed directly** (`(*[2]byte)(unsafe.Pointer(&h.port))[1]`, `h` a Go struct):
  emits `(@unsafe.ArrayPointer<byte>.Of(h.Ꮡport, 2)).Value[1]` and panics "index out of range [1] with length
  0". It is one of the four routes, so the door declines it. 0 corpus sites: the 11 field-view sites bind a
  local and index through the route's in-place path. Ruled its own small .4 seat after option 2.
- **F2, an index through a named array over memory that holds an unnamed array of the same shape**
  (`(*small)(unsafe.Pointer(a))[1]`, `a` a `*[64]byte`): the route for two array types of one shape,
  `a.Reinterpret<array<byte>, small>()`, and the named floor's panic by name. Loud. Census from the stored tables
  (`census/`): 0 sites. An index or a dereference whose operand is `unsafe.Pointer(q)`, `q` a typed pointer, has 2
  sites per standard-library target, both in `runtime/lock_spinbit.go` over a `*uintptr`, which the door takes,
  and none in sqlite or libc. None has an array behind `q`. Door C's named form
  is its cure. Its whole-load and whole-store half is closed by this cut.
- **F3, an index with a `FromRef` operand** (`(*[12]byte)(unsafe.Pointer(h))[0]` in a pointer-receiver method):
  keeps the box and panics "index out of range [0] with length 0". Declined by design. 0 corpus lines match
  `To<array<...>>(...FromRef(`. It is the receiver-box question, not an array question.
- **L1, an element that is itself an array** (`(*[2][2]int32)(p)[1][0]`): a managed element, so it keeps the box
  route, which answers length 0 over memory that is no Go array of that type. 0 census sites as an index or a
  dereference.
- **L2, an element that holds a reference** (`(*[2]unsafe.Pointer)(p)[1]`, and its whole store): refused by
  name by the pointee rule. By design. `sync/pool.go:93` is of this form in Go; the corpus file is hand-owned
  and does not carry it.
- **A call inside the pointer form under `len`** (`len((*[4]byte)(get(p)))`): keeps
  `len(To<...>(get(p)).Value)`. Go answers 4 after one call and no dereference. golib has no door for
  "evaluate, then answer N", so it is not a few lines. 0 sites.

### 16.8 What this section does not show

- Nothing ran on windows or darwin locally. The hosted set is dispatched at the announce.
- The sqlite-cs demo was not built or run at this cut. The index form is its dominant shape (10.5's counts),
  and what this section measures is the door, a program of the demo's shapes, and the standard library.
- No banked row was swept at this tip (stop line 4 is read at the merge result). The one banked row with a site of this cut, `internal/runtime/maps` (its test `map_test.go:666`, a named whole load), was run through `-tests` on linux/amd64 at the regen commit: 19 matched and 108 disclosed, as banked. Its line emits `(TestMapIndirect_big)@unsafe.Pointer.LoadArray<byte>(got, 256)`. The committed converted test sources of that package are not refreshed here: they already differ from the converter's emission by 59 lines of the interior-pointer respelling (batch 305), which that regen did not apply to test sources either, so the base arm does not reproduce them.
- `ElementRef` answers a `ref`, which cannot live across an `await` (10.5's note for N3). No emitted site
  splits the expression today; nothing here tests one that would.
- The V3 fast path decides "C memory" by the pointer keeping nothing and the registry not knowing the number.
  A number that named Go storage which has since moved or died is outside every door, as 13.4 says.

### 16.9 Amendment 2026-11-15 (P5): an anonymous struct element is lifted before a door names it

G found it at master `0c1d295ea8` (batch 309), on windows and on linux: the conversion of the sqlite-cs demo ends
with exit code 1, "2 unresolved dynamic type(s) were emitted as raw Go source". Both types are the element of an
array that C compiled to Go keeps in a function's frame block, which Go source declares once, as a blank
placeholder, and then reaches only through conversions:

```go
var _ /* aSub at bp+0 */ [13]struct {
	FnList int32
	FaList uintptr
}

(*[13]struct {
	FnList int32
	FaList uintptr
})(unsafe.Pointer(bp))[iSub].FaList = aMerge
```

**The cause.** An anonymous struct written in a conversion's type is lifted to a C# type where the conversion is
rendered (`liftConversionTargetAnonStruct`, called from the call arm of `convCallExpr`). The doors render from the
index, the slice, the dereference or the range, so a conversion a door takes is never rendered. Every door of 308
and 309 names its element in one function, `arrayPointerViewOfBase`, and it named this one with no lift behind it:
the deferred name resolved to nothing and the emission carried the Go struct text. Before batch 309 the index form
rendered the conversion, which made the lift. Door A (batch 308) has had the same hole for its slice and copy
forms; the sqlite sources index, so it first showed with door B.

**The change.** `arrayPointerViewOfBase` makes that lift itself, after the door's declines and before it names the
element, under the name the call arm gives it. A door that declines changes nothing. The lift reuses a C# type the
function already has for the same Go type: the blank placeholder's (`subLists__ᴛ1`) or a literal's bound to a
variable (`bound_v`), so a whole store of such a value assigns. With neither, the type is `<function>_type`. No
golib change.

**The other naming sites, and the predicate.** A site is in the class when a door names a type the conversion
alone writes. The doors have one naming site, and it is the one changed. The field view (18.2) names an element at
two sites, both behind its own rule that the element is a numeric basic type, so it cannot name a struct; a
conversion over a field's address whose element is an anonymous struct is the element door's, and the converter
test has that arm. A named array's name (the whole doors) is a declared type. The census
(`data/array-pointer-doors/anon-elem-1115/`) reads the standard library, production and test files, on four
targets for a conversion to a pointer to an array whose element holds an anonymous struct: 0 sites, with any
operand, beside 118 to 128 `unsafe.Pointer` conversions to a pointer to an array per target, and 13 sites in the
behavioral program planted as the positive.

| reading (linux/amd64, Release) | before | with the change |
|:--|:--|:--|
| converter test `TestArrayPointerDoorLiftsAnAnonymousStructElement`: six functions, one for each door form and one over a field's address | red: 6 unresolved types, one for each function | green: 0 unresolved, no Go struct text, each door over a lifted type, one declaration for each |
| `AnonStructArrayPointerDoor`, 13 lines: the two sqlite shapes, and a literal bound first | master's converter exits 1 with 2 unresolved types | converts, builds, 13 of 13 equal Go |

**What this leaves.** The open slice `[lo:]` of an unnamed array stays on its older route (decision D4, and the
BOARD row of 2026-11-14, "an array viewed over another array type answers length 0"). Over an anonymous struct
element that route converts, as before this change, and at run time it is that row: `[1:]` over six words panics
"slice bounds out of range [1:0]". The program's slice arm is written closed (`[1:2]`) for that reason. Nothing
here ran on windows or darwin, and the sqlite-cs conversion itself is G's to re-run at this tip. The local gates are
scored in the announce of this commit, not here.

### 16.10 Amendment 2026-11-16 (P5): the open slice of an unnamed array takes the doors (decision D4, taken)

Section 4.3 kept door A's open form for a named array, and section 8 put the unnamed one to the owner as decision
D4, recommending a BOARD row. That row is the 2026-11-14 row "an array viewed over another array type answers
length 0" (its first item, shape Q). COORD's brief of 2026-11-16 00:57Z asked for it in .4, and its 01:10Z
reading approved the plan below as written.

**What D4 left.** The view door declined `(*[N]T)(p)[lo:]` when the array was unnamed, and the copy door took the
open form only for a named one. The unnamed form kept the older route, `@unsafe.Pointer.To<array<E>>(p).slice(lo)`:
the box of the whole array, read as a value. Over memory that is a Go array of that very type it is right. Over
anything else it answers length 0, so a store panics where Go slices, and as `copy`'s destination over the bytes of
a number (reflect's `nameOff` copy) the box read itself ends the process (an access violation in the constructor
of `slice<byte>`, which no `recover` sees).

**The change.** Both `named` conditions go (`arrayPointerViewDoor.go`): the unnamed open form is the view door's
`View<E>(p, lo, N)` wherever it stands, and `CopyInto` / `CopyFrom` where it is a direct argument of `copy`. The
open form over a field's address stays the field view's (`ArrayPointer<E>.Of(h.Ꮡf, N).slice()`), and the converter
test keeps that as its control. No golib change.

**The census.** Over the door census of 11-14: the sqlite closure (modernc.org/sqlite v1.62.0, libc v1.80.1)
holds 0 unnamed open forms; its three open forms are libc's `RawMem`, named, door A's already. The standard
library holds 7 (linux), 8 (darwin) and 11 (windows); in the corpus they are the seven lines below, and the other
windows sites take a typed pointer, which is no door operand.

| reading (linux/amd64, Release) | at W17's tip | with the change |
|:--|:--|:--|
| converter test, three new positives (open view, copy into, copy from) and the field-address control | the three red: the old route | green; the control unchanged |
| `OpenSliceUnnamedArray`, 7 arms under a recover each (the probe) | A and B panic where Go slices; C ends the process (exit 134) | 7 of 7 equal Go |
| footprint, two seeded `-stdlib` roots per target | | linux/amd64 and arm64 4 files, darwin/amd64 3, windows/amd64 5 files, 6 lines; 0 unresolved markers |

The moved lines: the `nameOff` copy in `reflect/type.cs`, `internal/reflectlite/type.cs` and `runtime/type.cs`
(every target) is now `CopyInto<byte>(FromPinnedBox(ᏑnameOff), 0, 4, ...)`; `runtime/linux/os_linux.cs` reads
`startupRand` through `View<byte>(val, 0, 16)`; `runtime/windows/os_windows.cs` reads the environment block
through `View<uint16>(strings, 0, 16777216)`; and the registry's `value.cs.auto` review sibling takes
`View<byte>(..., 0, 4)` and `(..., 0, 8)` in `SetDWordValue` and `SetQWordValue`. The sibling was also stale since
batch 308 in four other sites (the span copy with the `unsafe` modifier); by COORD's 01:46Z note it is taken whole
to this tip's emission, 8 lines more, in its own commit.

**What the first gate battery found, two reds of this seat, each fixed on the branch.** (1) check-no-regression
moved one behavioral program, `GoImageCopyThroughView`: its open-form copies through a byte view of a struct that
holds an array (darwin's `Dirent`) had taken the Go-image view (`copy(@unsafe.GoImage(Ꮡd).slice(), data)`), and
with D4 the copy door, which is asked before the arguments are converted, took them as `CopyInto` over the box.
Such a box has no address for a door, so the copy door now declines a slice the Go-image route has marked, through
that route's own mark (`goImageCopySlices`), and the converter test carries it as CONTROL 10, red without the
decline. The corpus holds no Go-image site, so the footprint above stands. (2) `TestCommentParityCensus` rose 350
to 352 on the comment above the `nameOff` copy in reflect and internal/reflectlite: the comment is where Go has it,
but the census asked the C# line for Go's `copy` and the door spells it `CopyInto`. The census now reads a copy
door's line as carrying `copy` (as it already reads `new slice<T>` for `make`), with an anchor fixture arm, red
without the change, and the corpus census is back at 350.

**A candidate, not this seat.** The registry's `value.cs` is hand-owned because of this form: its own header says
the generated `(*[N]byte)(unsafe.Pointer(&value))[:]` read the value's bytes as a managed array and failed
`TestValues` with a nil dereference. With door A serving it, the review sibling
now emits the view, so retiring that hand-own is a candidate for a later seat, measured on its own.

**What this leaves.** The 11-14 row's other shape, `ge` (a named array bound to a local and then indexed as an unnamed
one), is the kept-local class of door C and stays .5. A nil array pointer made from a number still carries no
length (the 2026-11-14 row after it). The windows reading of the banked rows the moved lines touch (reflect,
runtime, internal/reflectlite, internal/abi) is COORD's to place; the local gates and the hosted set are scored in
the announce of this seat.

### 16.11 Amendment 2026-11-16 (P5): the doors read inline admission for the element (W19)

G found, on 2026-11-16, that door B still stepped aside in the sqlite-cs demo for arrays whose element is a ccgo
union, so the old copying box remained and G's demo carried a stand-in for those reads (W12b's: 11 reads on
windows, 18 reads and 1 store on linux). COORD's brief of 02:52Z asked for the size first; its 03:30Z reading
approved the plan below as written.

**The root.** Each door names its element in `arrayPointerViewOfBase` and requires it to be unmanaged in C#. It
asked `csManagedField(E, 0)`, the plain reading, where every Go array is golib's `array<T>`, a reference. A ccgo
union, `struct{ Fi [0]int32; Fu [0]uint32; Ff float32 }`, holds a `[0]T` carrier for each member but the one that
has storage, so under that reading it is managed and the door declined. The old route,
`To<array<E>>(p).Value[i]`, reads the whole array as a value and answers length 0 over memory that is no Go array
of that type, which libc's heap is not. But a struct ADMITTED to inline arrays (inlineArrayAdmission.go) holds a
`[0]T` as a readonly primitive marked `GoAlignmentCarrier` and a nonzero array as a `go.arrays` member, and
zeroSizeFieldLayout.go already reads such a struct with `csManagedFieldInline`. The door never asked admission.
COORD's question, whether a `[0]T` carrier is a managed field for the door's purpose: not once the struct is
admitted, and yes when it is not (then it really is an `array<T>`). The carrier classification (C2's B2 side) is
right as it stands; no change there.

**The change.** The element rule asks admission: admitted, `csManagedFieldInline`; otherwise `csManagedField` as
before. No golib change. The door's own `(*[N]E)(p)` converts the element whole, which is itself an admission
seed (condition 1 of the admission rule), so a union the doors meet is admitted unless a cap, a face, a foreign
closure or the pair rule keeps it out. The standard library is never admitted, so no corpus line moves by
construction.

**The census** (the doors' operand rule over modernc.org/sqlite v1.62.0 and libc v1.80.1; the tool and its output
are in the kit): elements managed under the plain reading and unmanaged under the inline one. linux/amd64 19 sites:
TExpr 7 and TRtreeCoord 4 (every array a `[0]T` carrier), Taddress 4 and the sockaddr union
`struct{ Fsin6 [0]Tsockaddr_in6; Fsin Tsockaddr_in; F__ccgo_pad2 [12]byte }` 4 (a nonzero array among them).
windows/amd64 11: TExpr 7 and TRtreeCoord 4. That is G's list exactly.

| reading (linux/amd64, Release) | at D4's tip | with the change |
|:--|:--|:--|
| converter test: read, store, nested union, nonzero inline array; control a union a cap keeps out | the four red: the box route; the control unchanged | green; the control unchanged |
| `UnionElementArrayDoor`, 7 arms under a recover each | 6 panic (length 0, and slice bounds [1:0]); arm F, a Go array of the same type, equal | 7 of 7 equal Go |
| footprint, two seeded `-stdlib` roots per target | | 0 corpus files on linux/amd64, windows/amd64, darwin/amd64 and linux/arm64; 0 unresolved markers |

**The control, and a premise that did not hold.** The plan's control was "the same union never converted whole,
so not admitted". The first run showed that premise false: the door's `(*[N]E)(p)` is the whole conversion, so
that union was admitted and took the door. The control is now a union the 4096-element cap keeps out (GO2CS2009),
whose `[0]T` stays an `array<T>`: it keeps the box route at both converters, which is the reading the not-admitted
arm of the rule must give.

**What this leaves.** Whether each of G's 19 types is admitted in its own package is read where it is converted:
G's sqlite-cs demo with no W12b stand-in, both demos, windows and linux, is the proof. A type a face or the pair
rule keeps out would keep the box route and name itself in that package's GO2CS2009 warnings.

## 17. Amendment 2026-11-15 (P5): the publish of a named array's holder is a compare-and-swap (option 2)

This is the first item of 14.3, ruled the seat after door B (01:19Z) and accepted as planned (08:54Z). golib
only: no public surface, no converter change, no emitted line moves. Code: `1ecae62963`.

### 17.1 What was wrong, and the change

The wrapper go2cs-gen writes for `type small [8]byte` holds one field, a holder, and its own getter fills that
field on first use with an interlocked compare-and-swap. That half was already right. The plain write was
golib's: `publishArrayBacking`, the gate of the element door `&p[i]` and, since section 14, of the two pin
paths, filled a boxed copy of the wrapper and then copied the whole wrapper back over the box's storage. It did
so under the box's lock, and the lock covers the publishes of that one box. The getter takes no lock. A
goroutine that first used the variable directly, between the gate's read of the storage and its write, had the
holder it had just installed replaced by the copy's, and every element it had written through its own holder
was lost without a panic. Go has one array.

The gate now stores the copy's holder into the storage with a compare-and-swap against the empty slot. A
publish that finds another holder there reads the storage again and publishes that one; the copy's own backing
was reachable from nowhere else. The swap is taken where the wrapper is exactly one reference word
(`s_arrayHolderIsOneWord`: the size of a pointer, and holding a reference), which both wrapper shapes that reach
the gate are: the named array, and the array-view wrapper over one (`type pallocBits pageBits`). A wrapper of
any other shape would keep the plain write. None exists, and a GolibTests arm reads the shape of every such
wrapper in the converted `runtime` so that a change of the generator's layout is a red test and not a silent
return to the plain write.

A third site made the same plain write and was found by reading, not by a test: reflect's pointer conversion
between two names of one array type (`GoReflect.TryConvertPointer`) touched the length of a copy and wrote the
copy back. It now reads the pointee through the same gate, by one internal member on the box
(`IGoReflectBox.ReadBoxSlotPublished`).

### 17.2 A correction to 14.3

14.3 says: "A field or element reference is a box minted per take. Two takes of one field's address on two
goroutines hold two locks." That is wrong for fields. A field's address is one cached box for each pair of
parent box and field (the field-view cache), so two takes share one lock, and two publishes through it never
overlapped. It is right for elements: `at` mints a box per take, and two element references of one element of
an array of named arrays hold two locks. I predicted the two-field-references arm red before its first run and
it read 0 lost of 200,000. The arm stays as a control, and an arm for two element references was added; it
read 78,243. The text of 14.3 stands as written, with this correction beside it.

### 17.3 Red first, and the readings

`NamedArrayHolderPublishRaceTests` runs two sides on two threads released together, once for each of 200,000
fresh arrays, and reads both writes back through the variable. A host that cannot run the two sides inside each
other's window proves nothing, so every arm first asks a control: the old publish written out by hand in the
test file, against a direct first use. Where the control loses no write the arms report Inconclusive, not
green. "Lost" counts the arrays where one side's write was not read back.

| arm (linux/amd64, Release, 4 processors) | at `8536236dd6` | with the change |
|:--|:--|:--|
| control: the old publish by hand against a direct first use | 81,465 lost | 62,831 lost (it must lose) |
| a direct first use against the element door | 47,334 lost | 0 |
| a direct first use against the pin by number | 15,878 lost | 0 |
| a direct first use against the raw pointer | 32,698 lost | 0 |
| two element references of one element | 78,243 lost | 0 |
| a direct first use against reflect's pointer conversion | 6,735 lost | 0 |
| two field references of one field (a control, 17.2) | 0 lost | 0 |
| each door alone still publishes | green | green |
| two pins by number of one array on two threads, named and unnamed | (arm added with the change; 0 and 0 with the plain write) | 0 and 0 |
| a publish after a whole assignment follows the storage | (arm added with the change) | green |
| every named array the generator wrote is one reference word | (arm added with the change) | green: 8 wrappers in the converted `runtime` |
| whole GolibTests | 2838 passed, 0 failed, 49 skipped | 2849 passed, 0 failed, 49 skipped |

Planted regressions, each restored byte-identical by hash. The publish forced onto the plain write reds exactly
the five racing arms (44,698, 46,668, 42,426, 77,670 and 204 lost, in the table's order) and leaves the other
six green. Reflect's site put back as it was, with the gate fixed, reds the reflect arm alone (6,320 lost).

The local gates (by-path builds, the repository guards, the converter suite, CNR) are scored in the announce of
this commit, not here. Nothing here ran on windows or darwin before the hosted runs.

### 17.4 What this leaves

- A named array ASSIGNED whole after its address was taken keeps its old pin (13.1). That is another lane's cut
  (ruled 03:46Z) and is not touched here. The publish does follow a whole assignment (the arm of 17.3): the gate compares
  the storage's backing with the one it last published, and a holder already in the storage is the one it
  adopts.
- The BOARD row "no arm takes views of one Go value on two threads" (door A's review) named this seat for its
  arm. The two-pin arm covers the pin of an array taken on two threads and reads green. A view of a Go value
  that is not an array, taken on two threads, still has no arm.
- The stress arms race hand-written copies of the generator's wrapper, as every GolibTests class of this
  record does. What ties them to the generator is the shape arm of 17.1 and the behavioral program
  `NamedArrayAddressWrite`, which runs the generated wrapper on one thread.
- The readings are of one host with four processors. An arm that reads 0 lost of 200,000 beside a control that
  loses tens of thousands is evidence that the window is closed, not a proof; the argument that it is closed is
  the compare-and-swap in 17.1.

## 18. Amendment 2026-11-15 (P5): the field view over a Go struct (F1 of 16.7, and what its probe found)

16.7 left F1 as "the native field view indexed directly ... panics index out of range [1] with length 0",
loud, 0 sites, a small seat. A probe of every sibling shape, run before any fix, read a wider defect, and COORD
ruled the seat in three parts (09:36Z): A, the direct index; B, the route for a Go struct; C, the forms that
read the whole array, as a commit on top. This section is A and B. Code: `d3d9786321`.

### 18.1 What the probe read

`(*[N]T)(unsafe.Pointer(&h.f))`, f not a T, is emitted `@unsafe.ArrayPointer<T>.Of(field, N)` since the native
field-view door (docs/phase4/DESIGN-native-array-view.md). The helper answered golib's native door for a NATIVE
root and the pointer conversion to the array type for every other root. The probe
(`data/array-pointer-doors/field-view-1115/probe/`:
15 forms over each of four roots and 4 over a fifth, 64 lines, Go's output against the emission at
`516586c4a9`, linux/amd64 Release):

| root | at `516586c4a9` |
|:--|:--|
| pages from `mmap`, and a struct laid over a Go byte slice (15 forms each) | 8 answer in place. 7 are refused by name: every form the converter spelled with `.Value` (the index where the view is written: read, store, compound, word; the whole load direct and through a local; a range by value) |
| a Go struct with no reference in it, on the heap and as a package variable (15 each) | `len` is right, a folded constant. 9 panic "index out of range with length 0", 4 panic "nil pointer dereference", and 1 is silent: a range by value over a local answers 0x0 where Go answers 0xf1 |
| a Go struct that holds a reference (4) | refused by name at the conversion, as "cannot view native memory" |

So the row's panic was the Go root's, and over a Go root the forms through a local failed as well. The
conversion answered a native box of `array<T>` over the field, and its `Value` read the field's bytes as the
three fields of `array<T>` itself: a managed reference made out of content, the class the floor's own comment
in `ж<T>.FromNumber` names. "Length 0" is what sat after the probe's struct.

One line of the prediction written before the run MISSED: the struct laid over a Go byte slice was predicted
wrong in every form, and it behaves as the native root. Its pointer is a native box over the pinned slice, so
golib's native door answers for it.

### 18.2 The change

**A, the converter** (`convIndexExpr.go`, `nativeFieldArrayView.go`). The view indexed where it is written,
`(*[N]T)(unsafe.Pointer(&p.f))[i]`, and its explicit dereference, `(*(*[N]T)(unsafe.Pointer(&p.f)))[i]`, emit
`@unsafe.ArrayPointer<T>.Of(field, N).ElementRef(i)`, as a local bound by the view does. They emitted
`(...Of(field, N)).Value[i]`.

**B, the unsafe package** (`src/core/unsafe/unsafe.cs`, hand-owned; no golib change, no public surface).
`ArrayPointer<T>.Of` answers `Pointer.FieldView<T>` where the native door declines. It resolves the pinned
pointer to the field as the view doors do (`Aliased`, 4.2) for a window of N elements at the field, and
answers a native array pointer over that window:

- V2, a Go value its box holds still: over the bytes the field's root holds, anchored on the box that holds
  them. The view keeps the root, and it ends where the root's storage ends (13.2's held extent): a view longer
  than its field reads the fields after it, as Go does, and one that would pass the end of the root is refused
  by name.
- V3 and V1 are answered as the doors answer them; neither is a field of a Go struct of another type.
- V4, a root with no address: refused by name, "the field is in a Go value that has no address (a struct that
  holds references has none)". It was refused as native memory.

So the helper answers a native array pointer for every root that has an address, and the converter's forms
over it are one set: `ElementRef`, `at`, `slice`.

### 18.3 Red first, and the readings

`FieldArrayViewOverGoStructTests`, 8 arms, at `516586c4a9` with the arms added: 8 of 8 red, each for its own
reason. Four panic with a length that is no view's; two are null references (the word view, the slice); one
fails "no panic" where `Value` answered a header; one reads the refusal that says native memory. With the
change: 8 of 8 green. The arms: the index in place and a store through it; a `uint64` field as two `uint32`;
the slice and the element address; `Value` refused by name; the held extent (a view of 12 bytes from a field at
offset 4 of a 16-byte struct reads its last byte, and 13 is refused by name); a struct that is an element of a
slice; the view as the only reference across three forced compacting collections; the refusal for a struct
that holds a reference.

One older arm changed. `NativeFieldArrayViewTests` held the helper equal to the pointer conversion over a
reference-bearing root, "box for box or panic for panic"; it read red with the change, as it should, and now
asserts the refusal's new words.

The converter's arms (`TestNativeFieldArrayViewIndexedDirectlyReadsThroughElementRef`): red at the base
converter on every form, green with A. Its control, a same-element view indexed where it is written, keeps
`array<T>.AliasPointer`.

The probe again, 64 lines against Go's:

| emission | unsafe package | lines equal to Go's |
|:--|:--|:--|
| base | base | 8 |
| base | cut | 12 |
| cut | cut | 48 |

Predicted for the cut before it ran: 48 equal, the 12 whole-array forms (3 on each of the four roots with an
address) refused by name, and the 4 forms over the reference-bearing struct refused in the new words. MET as
worded. The counts of the two upper rows are low partly because a refused store changes every line after it;
the row that carries the claim is the third, where no refused form writes.

The behavioral program `FieldViewOverGoStruct` runs the forms of A and B over a heap struct, a package
variable, an element of a slice of structs and a struct laid over a Go byte slice: 62 lines, equal to Go's.
The converter's emission of the probe moves 6 lines between the two binaries, each a direct index.

Whole GolibTests, linux/amd64 Release: 2857 passed, 0 failed, 49 skipped (2849 passed, 0 failed, 49 skipped at `516586c4a9`, so the
eight new arms and nothing else).

Footprint, two seeded standard-library emissions on each of the four targets with the base and the cut
converter: 0 corpus files on linux/amd64, windows/amd64, darwin/amd64 and linux/arm64, as predicted. Each pair of roots differs in 2 files, the run's own dated report and progress log (`reports/conversion_report.html`, `stdlib_conversion_progress.txt`). The 11 emitted field-view sites all bind a local, which A does not touch.

### 18.4 Where the sites are

A typed census with the converter's own predicate over the sqlite demo's module closure on linux/amd64 (12
module packages, 310 files; `field-view-1115/census/`): 17 field-address array views, all in
`golang.org/x/sys/unix`'s sockaddr code, all bound to a local. 12 are the field view's shape; 4 are two
selections deep and 1 is a same-element view. sqlite and libc have none. Every one of the 17 is over a struct
whose converted form holds references: a fixed array, its own or an embedded raw sockaddr's. So are the 8
sites in `syscall`'s own `recvfrom` code, which is displaced by a hand-owned file, and the 1 in `runtime`'s
`cheaprand`, on the 32-bit path. That is 26 sites, and a Go struct with no reference in it, the class B cures,
has none in these trees.

The 26 are refused by name before this cut and after it. Each views exactly its field (by reading the lines),
and a view that fits inside its field needs no address: it is a managed reinterpret of the field's own
storage. That is a new box kind in golib, ruled the next seat after C (09:54Z), with its design written first.

### 18.5 The shapes beside the field view (probe 2)

The field view is one field selected from a pointer-typed identifier. A second probe
(`data/array-pointer-doors/field-view-1115/probe2/`, 25 lines) ran the shapes that are not: a field two
selections deep (`&o.inner.port`), a field of a struct variable and of a package variable selected directly,
and the two-selection shape over a struct that holds a reference, which is `x/sys/unix`'s
(`&sa.raw.Port`, syscall_linux.go:394). The converter emits all of them identically at the base and at the
cut: this cut does not touch them. 12 of the 25 lines equal Go's, and 3 of the other 13 only follow from a
store refused on an earlier line.

| form | a Go struct with no reference in it | a Go struct that holds a reference |
|:--|:--|:--|
| the index where the view is written: a read, a store, the address of an element | right, 7 of 7 (and `len`): door B's element door | right, 4 of 4: the element door's fourth arm, Go's own `(*E)(unsafe.Add(p, i*size))`, reads and writes inside the field, and needs no address |
| the view bound to a local, then indexed | 5 of 5 panic: "nil pointer dereference" or "index out of range with length 0" | 2 of 2 refused by name, as native memory |
| a range by value over the local | silent: 0x0 where Go answers 0x46 | not run |
| the open slice `[:]` where the view is written | panics, a nil dereference (the open form of an unnamed array, which door A leaves: decision D4) | refused by name |

Two things follow.

- **The conversion kept in a local, over a Go value of another type that holds no reference, is the class of
  18.1 for every shape the field view does not take.** Decision D5 says of door C's unnamed form that "the form
  works over Go memory today". That holds where the memory is a Go array of that type. It does not hold here.
  The census of 18.4 counts 0 field-address views over a struct with no reference; a kept view of an address
  that is not a field's (`&x`, x a number) was not counted by it. BOARD row of this date, with its size.
- **Over a struct that holds a reference, the index where it is written already answers.** So the element
  access the 26 sites of 18.4 need is in the tree: an array pointer whose element i is the pointer conversion at
  offset i times the element size. What the next seat adds is the pointer that carries it, the bound that keeps
  it inside the field, and the field view's shape widened to a field two selections deep, which 4 of the 17
  census sites are.

### 18.6 What this leaves

- **C, the forms that read the whole array through the view** (`*p`, a range that reads the values,
  `(*p)[i]` through a local): refused by name on every root, where over a Go struct they were a panic or the
  silent range. A commit on top of this one.
- **The refusal's words over a Go struct.** The view's `Value` refuses as "native-backed array at 0x...: a
  *[N]T over native memory has no array<T> to dereference". Over a Go struct the memory is the struct's, held
  still. The text is golib's and is not changed here.
- **A field of a struct that holds a reference:** the 26 sites of 18.4. Refused by name; the next seat.
- **The shapes of 18.5 over a struct with no reference**, kept in a local: a panic, and one silent form. 0
  sites in the trees read. Sized on the BOARD; the field shapes go with the next seat's widened view, and the
  rest is door C's unnamed form.
- Nothing here ran on windows or darwin before the hosted runs. The sqlite-cs demo was not built at this cut;
  the census says it has no site of this shape outside `x/sys/unix`.

### 18.7 Amendment 2026-11-15 (P5): part C, the whole array through the view

The view has no `array<T>` for `Value` to return, so every form that needs all N elements goes through the
elements. `ArrayPointer<T>` gains two helpers (`src/core/unsafe/unsafe.cs`, hand-owned), and the converter
names them:

| Go, p a field view | emitted | what it does |
|:--|:--|:--|
| `v := *p`, and `*p` as a returned value, an argument, an operand of `==`, an element or field value of a composite literal, a sent value | `@unsafe.ArrayPointer<T>.Load(p)` | a new Go array, the N elements copied |
| `*p = v` | `@unsafe.ArrayPointer<T>.Store(p, v);` | N element stores; a value with no storage stores zeros |
| `for _, x := range p` | `range(p.slice())` | each element read when its round comes, as Go reads through the pointer |
| `for _, x := range *p` | `range(...Load(p))` | Go copies the array first, and so does the load |
| `(*p)[i]` | `p.ElementRef(i)` | the view's own element, no copy |
| `(*p)[lo:hi]` | `p.slice(lo, hi)` | the view's own window |
| `len(*p)`, `for i := range *p` | `N` | Go reads no memory for either |

p is the conversion where it is written or a local bound by it and never re-pointed, the same test A uses.

**The value positions are a list of their own.** `packageFieldViewLoads` is door B's list
(`packageArrayPointerLoads`) and three positions more: an element or field value of a composite literal, a
sent value, and any of them inside parentheses. Door B's list can leave a position out because its dereference
has a general emission to fall back on. The view has none: a value position the list misses is a refusal at
run time. The composite literal is not a corner. It is how `x/sys/windows` writes the form
(`RawSockaddrBth{BtAddr: *(*[8]byte)(unsafe.Pointer(&sa.BtAddr)), ...}`), found by the census of 18.8.

**The scope probe** (`field-view-1115/probe3/`: 32 functions, every sliced, dereferenced, loaded, stored and
ranged form of a view local and of the conversion where it is written, with five controls), converted by the
A+B converter and by the cut. 22 lines differ, every one a whole-array form of a field view: 6 slices of the
dereference (4 through a local, 2 where the conversion is written), 1 index of the dereference, 1 `len`, 9
lines of loads (a tuple of two, a typed declaration, an element of a slice literal, a returned value, two
field values of a struct literal, a field value inside parentheses, a sent value, a value inside
parentheses), 2 stores, 1 tuple store, 1 range over the pointer and 1 index-only range over the dereference.
The five controls (a local pointed at something else afterwards, a field two selections deep, a package
variable, a pointer to a real array, a slice) are byte-identical. Run: Go prints 11 lines. The cut's emission
prints the first 10 equal to Go's and stops at the tuple store, refused by name.

**One position stays refused by name:** the left side of a tuple assignment, `*p, *q = *q, *p`. The emission
is `(p.Value, q.Value) = (Load(q), Load(p))`, and `Value` refuses. 0 sites in the trees read. A BOARD row
carries it, with its size.

**Red first.** The silent arm reads red by its value: at `516586c4a9` the range by value over the kept view
ran no rounds and answered 0x0 where Go answers 0xf1 (`probe/base.out`); after A and B the form is refused by
name (`probe/cut.out`); with C it equals Go's. The converter's arms
(`TestNativeFieldArrayViewWholeArrayGoesThroughTheHelpers`): at the A+B converter all seven positive arms are
red, 19 failed assertions, and the control, a local pointed at something else, is green; with C all are
green. The golib helpers are new, so their seven arms in `FieldArrayViewOverGoStructTests` cannot be red at
the base: they are read green, and the converter's arms and the behavioral program carry the red.

**Readings**, linux/amd64 Release, at `73319097cd`.

| gate | reading |
|:--|:--|
| the behavioral program `FieldViewOverGoStruct`, with part C's forms added | 114 lines, equal to Go's |
| whole GolibTests | 2864 passed, 0 failed, 49 skipped, 2913 (parts A and B: 2857 passed, so the seven new arms and nothing else) |
| GolibTests by path | 25 of 25 |
| `go vet` of the converter | clean |
| `go test ./...` at the code commit | 10 packages ok, 1 red: `internal/repoguard`, on 14 cites in the docs that part C's own edits moved (below) |
| `go test ./...` with this docs commit | read after the commit and posted with the announce |
| check-no-regression | NO REGRESSION, byte-identical across all 1076 behavioral packages (12 platform-exclusive skipped by name) |
| footprint, two seeded standard-library emissions per target, base the converter of parts A and B | 0 corpus files on linux/amd64, windows/amd64, darwin/amd64 and linux/arm64. Each pair of roots differs in 2 files, the run's own dated report and progress log |

**Not predicted: the docs guards.** Part C's converter edits moved lines that 12 provenance notes cite
(`convSliceExpr.go`, `visitRangeStmt.go`, `convCallExpr.go`; 7 in `ConversionStrategies.md`, 5 in
`labels-and-loop-variables.md` by the guard's count of cited spans), and its additions to the behavioral
program moved two samples that `pointers.md` cites. `TestDocsProvenanceNotesResolve` and
`TestDocsSamplesHoldTheirLines` named all 14, and the two tests that plant a stale note into the same page
failed with them. The cites are re-pointed in this docs commit and nothing else in those pages changes.

**One gate run was mine to repeat.** The first check-no-regression run was killed by a cleanup I ran in the
same tree while it was running: it removed build output, the converter binary the check had built among it,
and the check stopped with the binary's path not found. No tracked file was touched. The reading in the table
is the second run, alone on the box. A build-output purge in a tree leaves `src/go2cs/bin` alone and waits
for a running check.

**Predictions** (`field-view-1115/probe3/prediction.txt`, written before the probe ran), scored as worded:

1. Probe 3: the first 8 lines equal to Go's, the run stops at the tuple store, refused by name. **The
   mechanism MET, the specifics MISSED as worded.** The prediction says Go prints 13 lines; it prints 11. And
   after writing it I moved the tuple store's call to the end of the probe so that every other line is
   reached, so the run prints 10 equal lines and stops at the 11th, with the refusal the prediction names.
2. The behavioral program, 114 lines, equal to Go's on every line: **MET.**
3. The emission of the same program by the converter of parts A and B, against their tree, stops at its first
   whole-array form, refused by name: it prints 14 lines equal to Go's and stops at the 15th, `heap whole load`, with the view's refusal by name. **MET.**
4. Footprint 0 corpus files on four targets: **MET.**
5. Whole GolibTests 2864 passed, 0 failed, 49 skipped, 2913: **MET**, each number.
6. check-no-regression, NO REGRESSION at 1076 packages: NO REGRESSION, byte-identical across all 1076 behavioral packages (12 platform-exclusive skipped by name). **MET.**
7. The converter's arms red on all seven positive arms at the converter of parts A and B and green with C,
   the control green on both: **MET** (19 failed assertions at the base).

No prediction covered the docs guards; that reading is above, on its own.

**What this leaves.** The tuple store above. The refusal's words, which still say "native memory" over a Go
struct (18.6). The 26 sites of 18.4 and the sites of 18.8 are over a struct that holds a reference: C's
helpers answer for them once the next seat's box exists, and not before.

### 18.8 Amendment 2026-11-15 (P5): the census over the TypeScript closure and the nine nugetgo modules

Ruled with the box kind (COORD 09:54Z). The instrument is 18.4's typed census, taking its target from the
environment and printing two more columns: what the field is, and whether the view's N elements fit inside
the field at the target's sizes (`field-view-1115/census2/`). Go 1.26.11, `GOWORK=off`, cgo off. The
TypeScript tree is private: this record holds counts and the `golang.org/x/sys` site rows, and no line of it.

| closure | own packages | linux/amd64 | linux/arm64 | darwin/amd64 | windows/amd64 |
|:--|--:|--:|--:|--:|--:|
| TypeScript (the tsc module at `ba29a56299`, 104 roots) | 0 | 17 | 17 | 4 | 7 |
| the nine nugetgo modules (18 roots) | 0 | 17 | 17 | 4 | 0 |

Every site is in `golang.org/x/sys`: `unix` (17 on linux, 4 on darwin) and `windows` (7). It arrives through
tsc's native-path and file-watch packages and `x/term`, and for the nine through gojq's command line
(go-isatty). Whether any site is on an executed path was not measured; by reading, all are in the
socket-address encoders and decoders.

- **The field** is a number in every site but one, and the view fits inside its field in all.
- **The root** is a struct whose converted form holds references, in all: the class refused by name today.
- **The use** is a local bound to the view and indexed, in all but three.
- **The shape**, on linux: 12 the field view's, 4 two selections deep, 1 a same-element view.

**Three on windows that the sqlite count does not hold:** `x/sys/windows`' `SockaddrBth.sockaddr`
(syscall_windows.go:1051-1053) loads the whole array through the view where it is written, as field values of
a struct literal. Part C emits them as loads. Two are over a number field. The third is over a GUID field, a
struct whose converted form holds an array: its 16 bytes are not contiguous in managed memory, so no
reinterpret of the field's storage serves it. That one is door C's class, a layout copy, and the box kind is
not sized as if it covered it.

So the population the next seat serves is "a number field viewed as its bytes", inside its field. The design
block for that box (`ReinterpretedArrayBox<E>` over the field's own box) was approved 11:17Z with three arms
added: identity against Go, the pointer's number, and a compacting collection between a store and a read.

## 19. Amendment 2026-11-15 (P5): the field view over a struct that holds a reference (the box kind of 18.4)

Ruled the next seat after C (COORD 09:54Z), its design block approved as written with three arms added (11:17Z:
identity against Go, the number, a move between a store and a read). This section is the record of the cut,
and 19.6 says where the cut departs from the approved block.

### 19.1 The class, and the answer

`(*[N]E)(unsafe.Pointer(&x.f))`, f not an E, where x's converted struct holds a reference: a fixed array (an
`array<T>` is a managed array), a string, a pointer. Such a struct cannot be pinned, so nothing holds its
bytes still, no native array pointer can be made over the field (V2 of 18.2 needs an address that stays), and
the view was refused by name (V4). It is the class of all 26 sites of 18.4 and of every site the census of
18.8 found: `x/sys`'s sockaddr code, which writes a port as two bytes and an address as four.

In every one of those sites the view fits inside its field. A view that fits inside its field needs no
address. golib already answers, for every pointer box, "my pointee's storage as bytes", recomputed from the
live box on each access (`IReinterpretSource.RawSlot`, the read `PointerExtensions.Reinterpret` takes for one
value). The new box kind is the same read for N elements:

| | |
|:--|:--|
| the kind | `ReinterpretedArrayBox<E> : ж<array<E>>`, internal and sealed (`src/core/golib/ж.ReinterpretedArrayBox.cs`) |
| it holds | the FIELD's own pointer, the `x.Ꮡf` the converter already passes, and N. No bytes, no pin, no copy |
| element i | byte `i x size(E)` of the field's slot, read as E. Index checked against N, with Go's panic |
| it is made | in `ArrayPointer<E>.Of`'s last arm (`Pointer.FieldView`, V4), when the field's type holds no reference, E holds none, and `N x size(E) <= size(f)` |
| `Value` | refuses by name: there is no `array<E>`. The refusal names the forms that are served |

A read sees the field as it is at that moment and a store writes it. So no copy exists to lose a write; the
field's own accesses (`x.f = v`) and the view's see each other at once, in both directions; and the collector
may move the struct between any two accesses, because the slot is found again from the live box each time.
Byte order is the host's, as Go's is. No state is added that a race could tear.

The forms the converter emits over the view are the ones parts A and C already emit, unchanged:

| Go | emitted (unchanged) | over this kind |
|:--|:--|:--|
| `p[i]`, a read, a store, `+=`, `++` | `p.ElementRef(i)` | the element, in place (a new arm in the three `ElementRef` overloads) |
| `v := *p` | `@unsafe.ArrayPointer<E>.Load(p)` | N element reads into a new Go array (a new arm) |
| `*p = v` | `@unsafe.ArrayPointer<E>.Store(p, v);` | N element stores (a new arm) |
| `len(p)`, `for i := range p` | `N` | no memory read |
| `p == q`, `p != nil`, `unsafe.Pointer(p)` | as for any pointer | 19.3 |
| `p[lo:hi]`, `&p[i]`, `for _, v := range p` | `p.slice(...)`, `p.at<E>(i)`, `range(p.slice())` | refused by name (19.4, 19.6) |

**Public surface: none added or changed.** `ArrayPointer<T>.Of` keeps its signature (tier 2); the kind and its
interface are internal; `Pointer.FieldView` is internal and gains a parameter. No instance state is added to
`ж<T>`, so no existing box grows. No generic virtual method is added (`GenericVirtualMethodGuardTests`).

### 19.2 The box has no storage of its own, so three doors ask it first

`ж<T>`'s two number operators (`uintptr` and `void*`) read the box's slot to find a fixed array's data before
they ask what kind of storage it is. This kind's slot refuses. So a non-generic interface,
`IPointerNumberForwarder`, is tested in both operators after the nil and zerobase arms and before the slot is
read, and answers the number of the field's pointer, minted by that pointer's own conversion.
`unsafe.Pointer.FromBox`, the mint for `unsafe.Pointer(p)`, takes the address of the slot the same way and
has the same test: the number is the field's, and the view rides along as the referent.

The test is one interface check on a path that already makes three virtual calls. `NativeAddress` is not
overloaded for this: `Reinterpret` reads a native box's number as a real address.

### 19.3 Identity and the number (the first two arms of 11:17Z)

**Identity.** The view's order token, allocation, referent, equality and hash are the field pointer's. Two
views of one field are one Go pointer; views of two fields of one x are not; views of x.f and y.f are not; a
view is never nil. `unsafe.Pointer` compares two pointers that each resolve a referent by the referent's order
token, so `unsafe.Pointer(p) == unsafe.Pointer(&x.f)` is true, as in Go, and stays true across a move, and the
two hash alike (a map keyed on them finds its key). Asserted in
`ReinterpretedFieldArrayViewTests.TwoViewsAreEqualExactlyWhenTheirFieldsAre` and
`TheViewAsAnUnsafePointerIsTheFieldsPointer`, and by the behavioral program's `address:` line.

**The number, and a finding against the ruling's wording.** 11:17Z asked that `uintptr(unsafe.Pointer(p))`
follow "the existing rule for a no-address pointee (the order token)", stable across two reads and equal to
the field box's own number, so nothing new is minted. The cut forwards the field pointer's own number, so the
last two hold by construction: nothing is minted or registered in the view's name, and the two are equal. But
that number is NOT an order token. The STRUCT has no address and its pointer's number is a token; a
reference-free FIELD of it is `PointerStorage.Unpinnable`, a real address that nothing holds still
(`ж.FieldRefBox.cs`, the Sysfd repair: tokenising such a field made the kernel refuse every Windows TCP
dial). So `uintptr(unsafe.Pointer(p))`, like `uintptr(unsafe.Pointer(&x.f))` before this cut, is the field's
address at the moment it is read. Two reads agree when no collection moved the struct between them, and the
unit arm reads them inside a no-GC region (`TheViewsNumberIsTheFieldsOwn`); it asserts the token equality
separately. Go's rule for a `uintptr` held across statements is the same: it is a number, not a reference.
This is the standing pin-unheld hole of an unpinnable field, older than this cut and not widened by it.

### 19.4 What is refused by name

| form | refusal | sites |
|:--|:--|:--|
| a view longer than its field, `N x size(E) > size(f)` | at the conversion: "the view is 4 bytes and the field holds 2; the field is in a Go value that has no address (a struct that holds references has none), so the fields after it have no address to read" | 0 |
| a field that is a struct whose converted form holds a reference | at the conversion: it is not contiguous bytes | 1, the Bluetooth GUID of 18.8: door C's layout copy |
| `p.Value`, and so `p[lo:hi]`, `p[:]`, `&p[i]`, `for _, v := range p` | "has no array<T> to dereference: its elements are the field's own bytes. Index it (p[i]), load it (*p) or store it (*p = v) ..." | 0 |

The first row is a real difference from a struct with no reference in it, where the same view is legal and
reads on into the following fields inside the root's storage (V2). The reference page states it.

The second row is not an array field. A field that is itself an array of numbers is a managed array of its
own, which can be held still: `ArrayPointer<E>.Of` serves a view of it from the arms part B wrote, on every
root, and does not reach this kind (19.7, the array-field probe).

### 19.5 Red first, and the readings

**Red.** The 16 arms of `ReinterpretedFieldArrayViewTests`, built against part C's golib and unsafe package
(`73319097cd`, the cut's four source files set aside): 14 failed. 12 threw the refusal by name where the arm
expects a value ("the field is in a Go value that has no address ..."), and the two refusal arms failed on the
refusal's words. The 2 that passed are the control (the struct has no address: its pointer is a token, its
field unpinnable) and the nil field pointer, which was nil before. The 25 arms of the two existing classes
passed, among them the two that asserted the old refusal; those two are rewritten by this cut.

**Green**, linux/amd64 Release, at the cut.
The seat was merged with batch 308 (`b2214e00e4`) before it was gated whole, so each reading names its tree:
"the second commit" is `3512595d9a` on part C before the merge, "merged" is that commit merged with part C's
merged tip (`e245e697be`).

| gate | reading |
|:--|:--|
| `ReinterpretedFieldArrayViewTests` | 16 of 16; the three field-view classes together 41 of 41 |
| GolibTests, the second commit | 2929, 0 failed (2886 passed, 43 skipped) |
| GolibTests, merged | 2935, 0 failed (2886 passed, 49 skipped): merged part C's 2919 and the 16 new arms |
| GenTests, merged | 530 of 530 |
| the behavioral program `FieldViewOverReferenceStruct` | its 18 lines equal Go's (the runner's four phases pass at the seat's last tree) |
| check-no-regression, merged | 1089 packages transpiled, NO REGRESSION across 1077, the same 12 platform-exclusive skipped |
| the converter's `go test ./...` | rc 0, 11 packages ok |
| the footprint, merged, four targets | one corpus file, on the two linux targets (the second prediction, below) |

The two GolibTests rows differ in passed and skipped by 6 with the same 16 arms in both. The first ran with the
pinned Go toolchain on its path and the second without it: six arms that run the Go toolchain as their oracle
ran and passed in the first and report inconclusive in the second. Read by comparing the two result files
test by test.

**The move** (the third arm of 11:17Z): `AStoreIsReadAfterTheCollectorMovesTheStruct` runs 200 rounds. Each
allocates garbage in front of a fresh struct, stores through two views made before the collection (two
element stores and one whole store), forces a blocking compacting collection, then reads through the views
and through the field, and stores again. The witness that a move happened is the field's own number: it must
change in some rounds, or the arm reports Inconclusive rather than green. The behavioral program does the
same with `runtime.GC()` between a store and a read.

**Predictions** (`reinterpret-box-1115/prediction.txt`, written after the field-view arms read green and before
the rest was measured), scored as worded:
| | prediction | reading | |
|:--|:--|:--|:--|
| 1 | GolibTests at the cut: part C's 2864 passed + 16 = 2880 passed, 0 failed, 49 skipped, 2929 | 2929 and 0 failed at the second commit; +16 at both trees. Passed and skipped read 2886 and 43, the six toolchain arms of 19.5 | MET on the total, the +16 and 0 failed; the passed and skipped numbers as worded MISSED by those 6 |
| 2 | as first worded: 0 corpus files for both commits | the second commit moves one | would have MISSED by one file; corrected before any footprint ran, both wordings kept |
| 2' | as corrected: `syscall/linux/lsf_linux.cs` alone, 3 lines, no position-map line, on linux/amd64 and linux/arm64; 0 corpus files on windows/amd64 and darwin/amd64 | exactly that file on the two linux targets, 3 lines, the two cut arms identical, the base arm equal to the committed file; 0 corpus files on the other two; 0 unresolved lift markers on all four | MET |
| 3 | behavioral +1 program, its 14 lines equal to Go's; check-no-regression NO REGRESSION at 1077 | +1 program; it holds 18 lines since the second commit added four, all equal to Go's; NO REGRESSION across 1077 | MET; the line count is the first commit's |
| 4 | the program converted by part C's converter and run against part C's tree is refused by name at its first line | no line printed; the refusal by name at its first view (`prediction4.out`, `prediction4.err.txt`) | MET |

The footprint is two-seeded: the base arm is merged part C's converter, the cut arm this seat's, both converting
into roots seeded from the merged tree. On every target two more files differ between the arms,
`src/reports/conversion_report.html` and `src/stdlib_conversion_progress.txt`: they are each run's own report and
carry its times. They are not corpus files and are named here so that the count of differing files (3 on linux,
2 on windows and darwin) is not read as a miss.

One GolibTests run is not in the table. The first whole run, at the first commit's uncommitted tree with another
gate running on the machine, read 2879 passed and 1 failed: `ASpinningThreadsCpuTimeAdvancesWithItsWallTime`, a
timing arm of the CPU clock that this cut does not touch. It passed in the two runs above.

**Amended 2026-11-16 (P5, COORD's ask of 13:11Z): that arm assumed a whole core, and no longer does.** It bounded
the spinner's CPU time by 0.8 x its wall time, so it read the machine's load: with one competitor pinned to the
same core it failed 10 of 10 runs at 41-48% of wall on a linux box, and hosted osx-arm64 read 52%. It now bounds
the CPU time by the spinner's own work: a fixed count of equal chunks, priced at the fastest chunk's wall time
(about 0.1 ms there, well under a scheduler slice), since contention only lengthens a chunk. Green 10 of 10 with
0, 1 and 3 pinned competitors; a seam reading 0 reds it, and a seam reading wall time reds its sleeper twin.

The open-slice probe of 19.7 carried its own two predictions (`reinterpret-box-1115/probe-open/prediction.txt`,
written before the probe was converted): the three open roots end the process and print no line; the three
workaround forms print Go's lines. Both MET.

### 19.6 Where the cut departs from the approved design block

Two lines of the block (posted 11:13Z, approved 11:17Z) are not delivered as worded. Each was posted to COORD
on its own.

- **"range over the pointer takes the element per round."** It is refused by name. Part C emits
  `range(p.slice())` for a range that reads the values, and a slice header needs storage it can name, which
  this value does not have. Serving it needs an emitted form of its own (an enumerator over the element door),
  and a snapshot would be wrong: Go reads each element when its round comes. 0 sites in every tree read. BOARD
  row of this date, with its size. The index-only range reads no memory and is served.
- **"`uintptr(unsafe.Pointer(p))` follows the existing rule for a no-address pointee (the order token)."** It
  is the field's own number, which is an address (19.3).

### 19.7 The converter: the field view's shape is a chain of field selections off a variable
The second commit of the seat (`3512595d9a`), ruled to ride with the box kind, probe first (COORD 11:17Z).

**The shape.** The hook (`nativeFieldArrayViewOf`, `src/go2cs/nativeFieldArrayView.go`) took ONE field selected
from a pointer-typed identifier, `&p.f`. It now takes one or more field selections, each of a field declared in
the type it is selected from, off any variable: `&p.f`, `&sa.raw.Port`, `&s.f` for a struct variable,
`&global.f` for a package variable. That is every shape whose address the converter already emits as the
field's own pointer (`p.Ꮡf`, `sa.Ꮡraw.ᏑPort`, `Ꮡs.Ꮡf`), which is what `ArrayPointer<T>.Of` takes. A promoted
field (a selection that passes through an embedded field), a root that is an index, a call or a dereference
written out, and a package-qualified name keep the pointer conversion, 18.5's route. The emission of a view
that was already a field view does not change.

**Probe first** (`reinterpret-box-1115/probe/`, 36 lines of Go output, both emissions built against the first
commit's tree). The converter of part C prints 12 lines equal to Go's; the widened one prints 32, and 49
emitted lines differ, each a field view through such a chain. The other 4:

| lines | form | reading at the widened converter |
|:--|:--|:--|
| 2 | `p[:]` and `&p[i]` where the conversion is written, two selections deep, over a struct that holds a reference | refused by name (19.4) |
| 2 | controls left on the earlier route: a promoted field, and a field of a slice element, kept in a local | "index out of range [1] with length 0", as at part C |

**One form changes from an answer to a refusal by name.** `&(*[2]byte)(unsafe.Pointer(&sa.raw.port))[1]`, the
element's address where the conversion is written, two selections deep, over a struct that holds a reference.
At part C it took door B's element door and printed Go's answer, through a box at an address nothing holds
still. A probe of that route (`reinterpret-box-1115/probe-unpinnable/`): over 200 rounds with a forced
compacting collection the struct moved in 200, and the kept pointer read stale in 200 (index 1) and 196
(index 0). That is the UNPINNABLE class of DESIGN-interior-pointer-root 10.1. One selection deep the form was
already a field view and already refused. 0 sites in every tree read.

**An array field viewed as an array of another element type** (`reinterpret-box-1115/probe-arr/`, 7 lines):
`(*[8]byte)(unsafe.Pointer(&d.name))`, name a `[8]int8`, which is how `os` reads a directory entry's name on
darwin (dir_darwin.go:77). The widening moves this form too, for the roots it admits, so it was read before
the commit was made, one arm per process:

| arm | part C's converter, part C's tree | the widened converter |
|:--|:--|:--|
| pointer root, `[:]` and a kept local (2) | Go's lines (already a field view) | Go's lines |
| struct variable and two-selection chain, a kept local (2) | "index out of range with length 0" | Go's lines |
| struct variable, package variable, two-selection chain, `[:]` (3) | **the process ends**: `AccessViolationException` in `slice<byte>`'s constructor, under `array<byte>.Slice` | Go's lines |

The three that end the process are not a panic a `recover` can take. The pointer conversion reads the field's
pointer as a pointer to an `array<byte>`, and the slice reads an array header from what is behind it. An array
field is a managed array of its own, which can be held still, so `ArrayPointer<T>.Of` serves it from the
arms part B wrote (the field's root holds the bytes), not from the new box. The behavioral program holds one
line for it (`array field of a variable`).

**What remains on that route:** the same `[:]` over a promoted field, over a field of a slice element and over
a field of a call's result. Read after the merge with batch 308 (`reinterpret-box-1115/probe-open/`,
predictions written first, one arm per process, each arm under a `recover`): all three end the process with
the same exception and the `recover` does not run; the three forms that reach the field from a variable or a
pointer (the embedded field written out; the element or the call's result bound to a pointer variable first)
print Go's lines. All six as predicted. Ruled for .5 (COORD 13:44Z) and disclosed with its workaround in
`docs/Limitations.md`; BOARD row of this date, with its size.

**The standard library was never typed-censused, and it holds one site the widening moves.** 18.4's
instrument reads a module's closure and skips packages that have no module, so given `std` it reads nothing:
18.4's standard-library count (8 in `syscall`, 1 in `runtime`) was made by reading. Run with that filter
flipped (`reinterpret-box-1115/census-std/`, Go 1.26.11, cgo off), the field-address array
views of another element type are:

| target | sites | the field view's shape at part C | two selections deep | a struct variable |
|:--|--:|--:|--:|--:|
| linux/amd64, linux/arm64 | 13 | 10 | 2 | 1 |
| darwin/amd64 | 15 | 12 | 2 | 1 |
| windows/amd64 | 12 | 10 | 2 | 0 |

On linux the typed run agrees with 18.4's reading (syscall_linux.go:527, 539, 670, 678; syscall_unix.go:338,
351, 365, 378; rand.go:245) and adds one site. Every root is a struct that holds a reference. Three of each target's first column are
`runtime/mgcmark_greenteagc.go`'s views to a NAMED array type (`*gc.ObjMask`): the instrument's predicate
counts them and the converter's does not (it asks for an unnamed array type), so they are not field views at
either commit. The two-selection sites are `syscall`'s own port encoders (`&sa.raw.Port`:
syscall_linux.go:527, 539; syscall_bsd.go:167, 180; syscall_windows.go:865, 884), each in a function that a
hand-owned file displaces on its target, so no emitted file holds them. The darwin struct-variable site is
`os`'s `readdir`, hand-converted on darwin. That leaves one emitted site the widening moves:

```go
// syscall/lsf_linux.go:32, LsfSocket
var lsall SockaddrLinklayer
p := (*[2]byte)(unsafe.Pointer(&lsall.Protocol))
p[0] = byte(proto >> 8)
p[1] = byte(proto)
```

`SockaddrLinklayer` holds an array and a raw sockaddr. The committed emission takes the pointer conversion
and indexes through `Value` (`src/core/syscall/linux/lsf_linux.cs:30`), the kept-local class of 18.5; the
widened converter emits `ArrayPointer<byte>.Of(Ꮡlsall.ᏑProtocol, 2)` and `ElementRef`, which the first commit
serves. It is a 27th site, not one of the 26. The footprint prediction for this commit was written as 0
corpus files and corrected to this one file before any footprint ran; both wordings are in
`prediction.txt`. The emitted file is updated by its own commit, from the footprint's cut root.

**Arms.** `TestNativeFieldArrayViewTakesAChainOfFieldSelections` (`nativeFieldArrayView_test.go`): a nested
field, a chain three selections deep, a struct variable and a package variable, each asserted to emit `Of` and
`ElementRef` and neither the pointer conversion nor `Value`; red at part C's converter on 12 assertions.
Three controls (a promoted field, a slice element's field, a call's result) assert the earlier route. The
behavioral program `FieldViewOverReferenceStruct` gains four lines: the port written and read two selections
deep, the index where the conversion is written two selections deep, a struct variable and a package variable,
and an array field of a struct variable.

### 19.8 What this leaves

- The range by value, the slice and the element's address over this kind: refused by name, 0 sites (19.4).
- A field that holds a reference (the Bluetooth GUID, 1 site in `x/sys/windows`): door C's class.
- `reflect` over such a pointer reads and stores the whole array through the same element door
  (`IGoReflectBox`), as for the native kinds; no arm here drives it through `reflect` itself.
- A view made by a form the field view's shape does not take (a promoted field; a root that is an index, a
  call or a dereference) keeps its earlier route: kept in a local it panics, and its open slice over an array
  field ends the process (19.7). 0 emitted sites in the standard library. Ruled for .5 (COORD 13:44Z); disclosed
  with its workaround in `docs/Limitations.md`; BOARD row of this date, with its size.
- Nothing here ran on windows or darwin before the hosted runs.

## 20. Amendment 2026-11-20 (P5): door C's unnamed form, kept in a local (decision D5, taken)

Section 8 left door C's unnamed form out of the first cut (D5) because "the form works over Go memory today ... and
sqlite has one such site". That site is wall C of the sqlite-cs run: modernc.org/sqlite's page cache binding
(`pagecache_trampolines.go`) writes two words into memory it took from `libc.Xcalloc`:

    f := (*[2]uintptr)(unsafe.Pointer(stub))
    f[0], f[1] = uintptr(page.Buf()), uintptr(page.Extra())

SQLite's own code then reads the same 16 bytes as a `sqlite3_pcache_page`. Go relies on three things: the stores land
in place in that memory, a raw read of the address sees them, and no managed copy of the array is made. The emission
before this cut was `@unsafe.Pointer.To<array<uintptr>>(...)` with `f.Value[0]` and `f.Value[1]`. `array<E>` carries
no N, so over an address no Go value owns the number conversion can build no window, and arm 5 refuses by name
(`DESIGN-native-array-view.md` 1.1 is why it must). Nine of sqlite's own tests stop there (3 in the pcache tests, 6 at
the root).

### 20.1 The change

**Converter** (`keptArrayDoor.go`). A conversion `(*[N]E)(unsafe.Pointer(x))` whose value is kept (bound, passed or
returned), of an UNNAMED array whose element holds no reference in go2cs's model, is emitted

    (*[N]E)(p)        ->   @unsafe.Pointer.ToArray<E>(p, N)
    f[i], (*f)[i]     ->   f.ElementRef(i)      f a local bound once by the door, never re-pointed, address never taken

It declines, keeping `To<array<E>>(p)`, wherever another route already renders the conversion
(`arrayPointerConversionKeepsItsRoute`: identity, a managed source, an alias source, the field view of section 18),
on a span view, on a `FromRef` operand (4.6) and on an element that holds a reference. A named array is not taken
(4.5's sibling box kind; 0 sites in the population of 20.3). `len(f)` stays the constant N, as for every array pointer.
An index through any other pointer to such a box (a parameter, a field, a re-pointed local) still reads `.Value[i]` and
refuses by name: loud, not wrong.

**golib.** `@unsafe.Pointer.ToArray<E>(p, n)` is `To<array<E>>(p)` with a length. It runs the same preamble (nil, and a
pointer whose source is itself the array pointer) and then `ж<array<E>>.FromNumber`'s own arm order, through a new
overload that takes the length and an opener; the old two-argument form forwards with none, so every other caller is
unchanged. The opener replaces exactly two answers, the two where `To` had no N:

| Arm | `To<array<E>>(p)` | `ToArray<E>(p, N)` |
|:--|:--|:--|
| 5: a number no Go value owns (C memory) | refused by name | an N-element native window over the address |
| 5: an address derived from a Go value that the registry does not know | refused by name | a window only if the value holds its storage still and all N elements lie inside it; otherwise refused as by `To` |
| 2b: a pinned real address of another pointee | a `NativeBox<array<E>>` whose `Value` reads element bytes as an array header | a window only over bytes that box holds still, all N elements inside them; otherwise `To`'s box |

Every other arm answers what `To` answers: nil, the same-type alias (the source box itself), the order token and a
tagged token (no window: a token names no user memory), token arithmetic (arm 3, refused by the same text), and an
element that holds a reference (the opener is null, so it is `To` exactly). The window is
`builtin.NativeArrayPointer<E>` (`NativeArrayBox<E>.Over`), whose `Value` refuses by name and whose `ElementRef` is
bounds-checked against N with Go's index panic. A window over Go storage is cut by the view doors' own rule (V2,
section 4.2): only over bytes a box holds still, anchored on that box (`IHeldStorage`; for an address derived from a Go
value, `NativeDerivation.HeldSource`), so the value stays alive and stays where the window says it is. `slice(...)` and
`at<E>(i)` over the box take the native view first; every form through `.Value` refuses by name, as it did at arm 5.

The opener is a cached static lambda per element type (`KeptArrayOpener<E>.Open`), so a conversion allocates no
delegate. The window is bounded by N, as Go's is: it is not checked against the size of the memory behind the
address, which Go does not check either.

### 20.2 What it costs the refusal's safety

Arm 5 is unchanged for every caller that does not pass N; only the door turns "an unknown address" from a refusal into
a window. An address derived from a Go value stays loud unless the value holds its storage still (20.3 shows why). The
one population that moves from loud to silent is a managed address that carries nothing and that the registry does not
know, a `FromRef` number that escaped its statement through a variable: the provenance design's stated bound, which
`unsafe.Slice` already accepts (4.6). Still held: no `array<E>` is ever made from bytes, an element holding a reference
is refused as before, an index is bounded by N, and the number 0 is nil. Lifetime is Go's: C memory freed under a live
window is a use after free on both sides.

One bound moves from loud to silent with it. The held-storage test runs once, at the conversion, so a window kept past a
whole assignment to its value still addresses the value's old elements: with `var a [8]uint64`,
`f := (*[16]uint32)(unsafe.Pointer(&a)); a = b; f[0] = 1` writes into the elements `a` had before the assignment
(`a = b` is emitted `a = b.Clone()`, the case `ж.cs` names where the pin is checked), and the write is lost without a
panic. The base was wrong there too, since To's box read the element bytes as an array header, so this is no
regression; it is the window's limit, read from the code and not measured. A conversion taken after the assignment
answers the new elements.

*Amendment 2026-11-21: measured, the limit above holds for none of 22 whole-assignment shapes once two converter defects found by the measurement are fixed; one shape remains (a return into a named result under a defer); see section 21.*

### 20.3 Red first, and the readings

GolibTests `KeptArrayDoorTests`, 11 arms: the C-memory tuple store (refused through `To`, written in place through the
door and read back raw); index N is Go's bounds panic with nothing written; nil; the same-type alias; arm 2b; an
interior address of a heap struct whose source is dropped, the window keeping it alive across a collection (made to fail
with the root forced null); token arithmetic refused by the same text as `To`; the order token answering `To`'s box
kind and its refusal; an element that holds a reference answering `To`'s refusal exactly; a field nothing holds still
answering `To`'s refusal; and a window past the end of a Go value answering `To`'s answer.

The door's two guards (the order-token test and the tagged-token test) were each removed in turn and both together,
and the door was moved ahead of arm 3:

| Variant | Result |
|:--|:--|
| order-token guard removed | 9 of 9 green |
| tagged-token guard removed | 9 of 9 green |
| both removed | the order-token arm red |
| door ahead of arm 3, guards removed | the token-arithmetic arm and the order-token arm red |

So the two guards are redundant with each other (either alone holds the order-token arm), and the arm order is what
keeps token arithmetic refused. Restored sha-identical. The full GolibTests suite (linux, 3075 tests at the V2 commit)
is green at Debug and Release.

**A window over storage nothing holds still: found by the scope probe, fixed before the push.** The first cut opened its
window at arm 5 for any address the registry did not know. Every array-pointer fixture of the converter suite, converted
by the base and the cut converters, moved three shapes beyond the sqlite site: a conversion ranged for its index alone, a
conversion stored into a field or element, and `(*[2]byte)(unsafe.Pointer(&e.Port))` through a promoted field, an
element or a call result (the shapes 19.8 leaves). Built and run, the third wrote through its window after a collection
and the write was lost: the field belongs to a struct that holds a reference, nothing can pin it, and the window
addressed where the struct had been. The base refused it by name at the conversion. The opener now applies V2, and the
same program answers the base's refusal; the ranged form and a held field view print what native Go prints. Made to
fail: with the held-extent test removed, the two new arms go red and the other nine stay green; restored sha-identical.

**Corpus footprint**, two seeded `-stdlib` roots (windows/amd64, linux/amd64, darwin/amd64, linux/arm64): 12 files, all
under `runtime/`, 36 lines out and 36 in: 22 conversions respelled and 18 index sites to `ElementRef`. No `.csproj`, no
`package_info.cs`, no `GoPositionMap` line. The prediction named 23 conversions; `runtime/symtabinl.cs:73` is not
respelled, because `inlinedCall` carries `_ [3]byte`, an `array<byte>` field, so its element holds a reference and the
door declines it. The decline is correct and the prediction's census was wrong. Every one of the 22 sites is in runtime
code that is converted but not run (`DESIGN-native-array-view.md` 1.5): signal handling, vdso and auxv setup, the
write-barrier buffer, inline unwinding, and `alg`'s complex-number equality. The behavioral sources emit no door: all 31
behavioral packages whose Go source holds a `(*[N]T)(...)` conversion convert byte-identically with and without the cut.

<!-- P5, 2026-11-20. Rulings: COORD 085021Z (the amended design; P3 MUST 1-2, SHOULD 3-5 taken), 092645Z (P3 CLEAR on the
     design; SHOULD 4 owed in full before the merge: the census over the behavioral suite and the banked rows that load the
     runtime, prediction first). Tree base 91024cab0c (batch 320). Commits: f609052c37 (the door, converter and golib, 9
     arms; the liveness arm made to fail with the root forced null in ж.cs and restored sha-identical), b6b0816469 (the
     census instrument: per-process file, start block, samples), f075e0e3e9 (the regen of the 12 runtime files),
     12347fb964 (the V2 rule in the opener, two arms, the converter test expectations and the moved citations; the
     scope probe's 12 fixture modules, 3 changed, read in full). Footprint:
     base converter 1143 s, cut 914 s, GOMEMLIMIT=9GiB, one at a time (the first attempt was OOM-killed at 12.6 GB beside a
     GolibTests build). Per-file index sites: signal_amd64 (linux-amd64, darwin) 4 each, signal_darwin_amd64 3,
     signal_unix (linux, darwin) 1 each, alg 1, traceback (amd64, linux-arm64) 2 each. vdso_linux and mgc assign to an
     existing variable or field, so their index sites keep .Value[i]. Excluded by name: the zsyscall argv and malloc l2
     lines and plugin.cs:96 (a pointer or unsafe.Pointer element), sync/pool.cs.auto:100, mfinal.cs:329 (a whole-file
     hand-own whose .cs.auto already holds StoreArray). The committed bytes of the 12 files equal the pre-change emission
     (CR-stripped compare), so the regen carries no foreign drift. GolibTests linux: Debug 2997 passed / 76 skipped / 0
     failed, Release 3022 / 51 / 0, of 3073; the 6 toolchain tests that skip without go on PATH pass with it at both. -->

### 20.4 SHOULD 4: the census, and the sqlite reading

**Prediction, posted before any run:** zero arrivals at the door's native arm from the behavioral suite and from the
`runtime`, `reflect`, `sync` and `os` hosts, and arrivals with arm 5 above zero and arm 2b at zero from the sqlite root
and pcache hosts, whose nine divergences (six root, three pcache) were predicted to pass. The prediction also said a
process that never arrives writes no file. That was wrong about the instrument, not about the door: the census's start
block writes a `total=0` file for every golib process that has the variable set, so the reading is the sum of the
totals, and the file count is the number of processes the census saw.

| Population (linux/amd64, `CGO_ENABLED=0`) | Result | Processes | Arrivals |
|---|---|---:|---:|
| Behavioral suite, all four phases, 12 slices | 1009 of 1009 measurable pass (Output: 983 compared, 26 not compared) | 993 | 0 |
| `runtime` -tests | PASS 11094 | 18 | 0 |
| `reflect` -tests | PASS 445 | 1 | 0 |
| `sync` -tests | PASS 48 | 10 | 0 |
| `os` -tests | 3831 + 2 (the banked 3840 + 2 is the hosted runner's reading; 3831 + 2 is the other hosts', as the roster names) | 16 | 0 |
| sqlite module (positive control) | root's six and pcache's three divergences pass; pcache validates 19 of 19 | 9 | 1281 (pcache 1280, root 1), all arm 5 |

No banked row arrived, so there is no number to read for a mapped address. Every sampled sqlite number lies in the
`0x7f..` range where libc's allocator maps its blocks; the hosts had exited, so they were not read against
`/proc/<pid>/maps`.

The sqlite root host still dies in flight on `TestSEHTruncatedShm` (an access violation in `Xmemcpy` under
`walIndexTryHdr`, which reads a shared-memory file the test has truncated). The run before the cut died there in the same
way, so the tests after it in the root host are unmeasured in both runs. It is not this seat's wall.

<!-- P5, 2026-11-20, at 12347fb964 (CNR rc 0: 1122 behavioral packages byte-identical, 12 platform-exclusive skipped;
     converter go test rc 0; GolibTests linux Debug 3005 passed / 70 skipped / 0 failed, Release 3030 / 45 / 0, of 3075).
     Behavioral: run-behavioral.ps1 --slice i/12 --phase all, bin/obj purged between slices, census
     GO2CS_KEPT_ARRAY_CENSUS_FILE=<dir>/k-{pid}.txt; 993 files, every one total=0. CNR transpiles 1134 packages; the runner
     enumerates 1021 projects (1009 measurable, 12 platform-exclusive), so 113 packages CNR reads are not runner
     projects. -tests: run-validated-sweep.ps1 -Filter PKG -Exact, one row at a time, core bin/obj purged before each; the sweep's test-side emission drift and untracked
     test files restored and removed afterwards, `git status --porcelain | grep '^ D'` empty. The os row's 9-verdict
     shortfall is the host: ValidatedTestPackages.md's 2026-11-10 note records 3831 + 2 on the non-hosted hosts against
     3840 + 2 on the hosted linux-x64 runner. sqlite: the C1 union tree (ea2aa14497) with the cut merged locally
     (de3a62fad8, no file touched by both sides), converter rebuilt, `-tests -recurse -test-action all` over the module,
     3547 s; the run before the cut is the same union without it. go2cs.slnx (linux, Debug): the whole solution ran the
     disk out (1 GB left, MSB4017 on a full device) and also reds on SockaddrRoundTrip, a windows-only project the linux
     flavour cannot compile; its 105 members outside tests/Behavioral, built through a solution filter, compile with
     0 errors, and the behavioral members were compiled by the census run above. -->


## 21. Amendment 2026-11-21 (P5): a whole assignment copies into the window's storage; one shape remains

20.2 says a window kept past a whole assignment to its value addresses the old elements, because `a = b` is emitted
`a = b.Clone()`. For a whole assignment in statement position that emission had already gone when door C landed: since
the in-place reassign (DESIGN-array-reassign-in-place.md on claude/r-reassign-census, landed as 5953f454d0), a whole
assignment to an existing array location is emitted `a.Assign(b)`, and to a struct holding an array `h.AssignFrom(...)`,
and both copy into the storage the location already has. The window is cut over that storage, so it sees the new
elements and a write through it lands in them. 20.2 was read from the code and not measured, and it misread that
emission.

It was right for the paths the in-place reassign did not reach. Measured (21.1), two converter defects and one
remaining shape:

- **A tuple assignment in a for clause** (`for a, n = b, 0; ...`, and the same in the post clause) still emitted
  `(a, n) = (b.Clone(), 0)`: the tuple's in-place copy needed statement position (visitAssignStmt.go, "a for-loop
  clause has neither"). Fixed in this seat (bb687e3e32): in the init clause the `Assign` calls follow as a C# expression
  list, `for ((var aᴛ1, n) = (b.Clone(), 0), a.Assign(aᴛ1); ...)`, with a non-identifier target captured before the
  loop; in the post clause, where C# allows no declaration, the temporary is declared in the hoist buffer, which lowers
  the post to the end of the body behind the existing continue label. The standard library holds no such clause (21.2).
- **An escaping named array result in a function with a defer** was boxed as `heap<array<T>>(out ...)`, which is
  `default(array<T>)`: no storage. That is the "destination with none" this section first called unreachable through
  the door. It is reachable: a window cut over the result threw at the cut, and, door or not, every index through the
  result panicked with length 0. Fixed in this seat (901eb8fbf2): the result gets `heap(new array<T>(N), out ...)`, as a
  local does. The committed corpus holds no instance of the bare form.
- **What remains: `return v` into a named result, in a function with a defer**, is emitted `a = v` before the deferred
  calls run, so it replaces the result's storage. A window, a slice (`s := a[:]`) or an element pointer (`&a[1]`) taken
  earlier and written by a deferred call misses the returned value: Go returns `7 2` where the conversion returns `1 2`.
  This is not door C's; it reaches plain slices too, and it is the in-place reassign's missing return path. Recorded in
  Limitations.md ("A few quiet ones") and KnownIssues.md, with the workaround (assign the result, `a = v`, then a bare
  `return`, which takes `a.Assign(v)`; measured green). The standard library has no instance (21.2). Fixing it means
  the defer-mode return in visitReturnStmt taking `Assign`/`AssignFrom`, with tuple temporaries for a multi-result
  return; sized at about one lane-day with its corpus footprint, so it is posted for scheduling rather than cut here.

A map element (`m[k] = b`) keeps the clone by rule: a Go map value is not addressable, so no window can be cut over
it.

### 21.1 The guard, and its controls

Behavioral `ArrayViewWholeAssign`, 30 lines, each taking a window (or, for a named result, an element through the result's
address), assigning the whole value and printing the value's first element and the window's element 2, compared to Go:
a variable from a variable, from a literal, from a call, a tuple, a swap, an array field, a whole struct (the field
view), an element of an array of arrays, the outer array, `*p`, a package variable, an element of a slice of structs;
the for init, for post and for-clause field tuples; if-init and switch-init tuples; a multi-value right side
(`a, err = f()`); a range that assigns (`for _, a = range arrs`); a struct inside a struct; `*ph = holder{...}`; a named
array type; an index through an escaping named result in a function with a defer; a select receive and a comma-ok
receive (`a, ok = <-ch`); a single assignment in a for init and in a for post (`for a = b; ...`, `...; a = b`, the
one-operand path); a labelled `continue outer` from an inner loop into a loop whose post was lowered; and an index
through an escaping named result of a struct holding an array, and of an alias of an array type (`type A = [8]uint64`),
each in a function with a defer. All four phases pass.

Controls, each a scratch build, the converter source restored sha-identical:

- In-place assignment off. The edit: the first statement of `valueAssignInPlace` (arrayCloneOperations.go) made
  `if true { return false }`. The twelve statement-position lines red, each printing the old element and 0, which is
  exactly the loss 20.2 described (run on the first twelve, before the other shapes existed).
- The converter before bb687e3e32: the three for-clause lines red (`for cl  : 101 0 1`, `for post: 171 0 2`,
  `for fld : 181 0 1`), the other seven new lines green, as predicted before the run.
- The converter before 901eb8fbf2: the named-result line panics, `index out of range [1] with length 0`.
- The converter at 10a8a1b325, before the alias fold (P3's review, COORD 144923Z): the alias-of-array result line
  panics the same way, since `type A = [8]uint64` is a `*types.Alias` and missed the array test; the other four new
  lines (both single assignments, the labelled continue, the struct result) green, all five as predicted before the run.
  The fold unaliases the result type first.

### 21.2 The standard library census

A go/packages pass over `std` with test files, at windows/amd64, linux/amd64, darwin/amd64 and linux/arm64: a for
clause whose tuple assigns an array or a struct holding one, 0 (positive control: the guard reads 3 of 3); a function
with a defer, a named result of such a type and an explicit return, 4 (net goLookupIPCNAMEOrder, internal/trace
MakeEvent and ReadEvent, log/slog Value.Resolve), none taking the address, a slice or an element address of the result
(positive control: the probes read 3 of 3). The predicate does not see an alias taken through a field of a struct
result or through a pointer-receiver method call; the four were read and take neither.

## 22. Amendment 2026-11-21 (P5): the return into a named result copies in place; the remaining shape is fixed

21's last bullet left one shape: `return v` into a named result, in a function with a defer, was emitted `a = v` before
the deferred calls ran, so it replaced the result's storage, and a slice, an element pointer or a window taken from the
result earlier and written by a deferred call missed the returned value (Go `7 2`, conversion `1 2`). COORD scheduled
the fix as its own seat (134304Z). It is fixed, and the Limitations and KnownIssues entries are removed.

### 22.1 The change

- **The defer-mode return** (visitReturnStmt.go) asks `valueAssignMethod` of each named result's type, the predicate
  the in-place reassign uses for `=`. A single result that owns array storage is emitted `a.Assign(v)` (an array) or
  `h.AssignFrom(v)` (a struct holding one), without the clone: the call copies the elements into the result's own
  storage. In a tuple, each such operand becomes a temporary in the deconstruction and its copy follows the statement,
  left to right, as in the tuple path of visitAssignStmt: `(var aᴛ1, err) = (mk(), default); a.Assign(aᴛ1);`. A result
  forwarded from a call (`return pair()`) takes the same form, `(var aᴛ1, err) = pair(); a.Assign(aᴛ1);`. A result
  reached through its box inside a function literal is `Ꮡa.Value.Assign(...)`. Results with no value-assign method, and
  every return outside defer mode, are emitted as before. In a tuple the clone of a variable operand is kept; it is
  redundant before the copy, and correct.
- **A function literal's escaping named array result** was declared `heap<array<T>>(out var Ꮡa)` outside the wrapper
  (iifeOperations.go, namedReturnDeclLines), which is `default(array<T>)`: no storage, so every index through the result
  panicked with length 0. It now takes `heap(new array<T>(N), out var Ꮡa)`, the literal twin of the 901eb8fbf2
  declaration in visitFuncDecl, with the alias unwrapped as in the 21 fold. A literal whose result is not boxed takes the
  frame form's plain local and was never affected by this half.

### 22.2 The guard, red first

`ArrayViewWholeAssign` gains 14 lines (44 in all), each a named result written by a deferred call through something
taken from it before an explicit return: a slice, an element pointer, a window, a struct result's array field, a named
array type, a tuple `return mk(), nil`, the second of two results with an early return, a recovered panic, a return of
a variable, a function literal's result (plain and captured), a literal whose result's address is taken (an index
through it, and `return [8]uint64{1, 2}` written through the pointer), and a forwarded call `return pair()`. Go output
checked first.

- The converter at d7df22c615 (door C with its fold): 11 lines lose the deferred write (`ret slice: 1 2` against Go's
  `7 2`, and the same loss on each of the other ten), and the address-taken literal panics `index out of range [1] with
  length 0`, which stops the program there, so the last two lines are not reached. The forwarded call is red by its
  emission (`(a, err) = pair();`), read and not run. Predictions P-RD-3, P-RD-5 and P-RD-6 were
  written before each run; P-RD-3 missed the plain literal in kind (it lost the write, it did not die, because a literal
  whose result is not boxed takes the frame form's local), which is why the address-taken pair was added.
- The fixed converter: 44 of 44 lines equal Go; the golden re-baselined from that emission and byte-equal to it.
- check-no-regression moved one more behavioral file, not predicted: ZeroValueArrayNamedResult, whose `withDefer`
  ends `return a4, nil` under a defer and now emits `(var a4ᴛ1, err) = (a4.Clone(), default); a4.Assign(a4ᴛ1);`. Its
  golden is re-baselined and all four phases pass, output equal to Go. No other behavioral file moves.

### 22.3 The corpus footprint

Seeded `-stdlib -comments` reconverts, base converter d7df22c615 against the fix, one target per run (windows/amd64,
linux/amd64, darwin/amd64, linux/arm64), both arms at one root path, seeded from this branch's `src/core` with
`version.props`. On every target the same two files differ, by the same 27 lines: internal/trace event.cs (the 11
returns of MakeEvent) and reader.cs (the 16 of Reader.ReadEvent), each line `(e, err) = (...)` becoming
`(var eᴛN, err) = (...); e.AssignFrom(eᴛN);` on the same line. No line is added or removed, no position-map line moves,
and no project file or README differs; the bare `heap<array<..>>(out var` count is 5 in both arms (all `:=` locals, none
a literal's result). The hunks are applied verbatim in this seat and internal/trace builds clean.

Of the four functions 21.2 named, net goLookupIPCNAMEOrder and log/slog Value.Resolve do not move: the first has no
defer of its own (the census predicate saw one inside a function literal), and the second's result type, Value, holds
only a zero-length array and has no value-assign method (its return is emitted `rv = v;` with no clone). Both were
predicted 0 before the run (P-RD-7). The prediction missed the trace counts by one per file: it counted the
`goto ᒐdone` returns and not each function's terminal return. Event's arrays are copied into the result's storage now;
nothing in either function takes an alias of the result, so the banked internal/trace row should read as before. It
was not re-run in this seat; the union sweep reads it.

### 22.4 Amendment 2026-11-21 (P5): P3's review, three guard rows and one clause

P3 cleared the seat (2026-11-21 17:41Z) with two SHOULD rows and one NIT. COORD ruled one commit on the seat:
- **Three guard rows** in ArrayViewWholeAssign, each compared to Go. None reds at this tip, so none needed a fix:
  - `retCrossed`, `func f() (a, b [8]uint64)` ending `return b, a`: the temps take both operands before either result
    is stored, so the results swap. Go and C# print `7 2 8 1`.
  - `retPanicAfter`, `return [8]uint64{1, 2}, boom()` under a recovering defer: the panic comes before any result is
    stored, so `a` stays zero and only the deferred write reaches it. Go and C# print `0 5 <nil>`.
  - `retNamedTuple`, a `named` array result in a tuple fed an unnamed array: the temp takes the operand's type and
    `a.Assign(aᴛ1)` compiles. Go and C# print `7 2 <nil>`.
  The guard is now 47 lines, all equal to Go, and its golden is re-baselined by `UpdateTestTargets --only`.
- **The internal/trace banked row** (SHOULD 2) is COORD's to name in the batch's gate list; it was not re-run here.
- **NIT, `visitReturnStmt.go`.** The tuple form names its temps from `namedReturnNames` by target index, so the
  in-place condition now also requires `len(v.namedReturnNames) == len(assignTargets)`. Where they differ, the return
  keeps the plain assignment instead of indexing past the names.

## 23. Amendment 2026-11-28 (P5): an element that holds a pointer word, over C memory (design note, before code)

> Amended by 23.8 (P3's review, 2026-11-28): the pointer-field accessors already on master replace items 1 and 5, the
> read rule is the accessor's, no write reaches the window's indexer, and the class is the `[GoCgoStruct]` marker's.

COORD asked (050241Z) whether door B can serve a C-memory struct element whose fields include an `unsafe.Pointer` or a
pointer member, read and write, with the pointer minted as C memory on read, as H's go-sqlite3 slice does for one record,
and what it costs. The shape is go-sqlite3 v1.14.53's row read (sqlite3.go:3105):
`(*[(math.MaxInt32-1)/unsafe.Sizeof(C.sqlite3_go_col{})]C.sqlite3_go_col)(unsafe.Pointer(rc.colvals))[:n:n]`. 16.1 refuses
"an element that holds a reference" by name, at every door. This section is a design note: no code is cut.

**Answer.** Not as door B stands, and not by widening it: no `ref E` can alias C bytes when `E` holds a reference. Yes by
a different element: a native box at the element's address whose word members are read and written at the access, with
no managed copy kept. The read half is mostly in golib already. The write half needs one converter emission. The whole
thing is one seat of about a day and a half, stacked on H's slice, with a corpus footprint of zero by construction (23.6).

<!-- Provenance. Ask: claude/mailbox docs/phase4/inbox/P5/20261128T050241Z-COORD.md. H's slice: claude/h-cgo-sqlite-slice
     bc383e49c9, docs/phase4/DESIGN-cgo-sqlite-marshalling.md 2 (shape A) and 6 (rulings, COORD 031803Z). Readings: one
     cloud linux/amd64 VM, Go 1.26.11, gcc, .NET SDK 10.0, Release; H's converter built from bc383e49c9's src/go2cs; the p3
     fixture of testdata/cgo/p3 with one arm added (slicecolsview), converted -cgo against libgo2csprobe built from H's
     probe.c. golib read at bc383e49c9: ж.cs FromNumber, ж.NativeBox.cs FieldRef and ReadGoValue, ж.ShadowArrayBox.cs,
     slice.cs (native mode), runtime/GoCgoArgs.cs. -->

### 23.1 The readings

Go prints the same three lines for both arms below: `1 10 0.5 alpha`, `2 20 1 beta`, `3 30 1.5 gamma`.

- **slicecolsc**, H's red arm, is door C's shape and not door B's: `cols := (*[3]C.struct_go2cs_probe_col)(C.malloc(3 *
  size))`, then `C.go2cs_probe_col_fill(&cols[0], 3)` and a read of `cols[i].typ`, `.i64`, `.f64`, `.ptr` and `.n`. It is
  emitted `@unsafe.Pointer.To<array<C.struct_go2cs_probe_col>>(C.malloc(3 * size))` and dies there: "cannot view native
  memory as array<struct_go2cs_probe_col>: the address has no managed element storage behind it" (ж.cs, FromNumber's
  array floor). The read loop is emitted `cols.Value[i].typ`.
- **slicecolsview** is go-sqlite3's own shape, added to the fixture for this note. A typed pointer to C memory is filled
  by C, cut as the open slice of a huge array, and read through `col := &colvals[i]`. It is emitted
  `array<C.struct_go2cs_probe_col>.AliasPointer(p, 53687091).slice(0, 3, 3)` (door A's open slice, 16.10), then
  `Ꮡ(colvals, i)`. The call before it crosses: `C.go2cs_probe_col_fill(p, 3)` passes the native box's address. It dies at
  the slice: ShadowArrayBox refuses a `*[N]T` over memory the Go runtime did not allocate, for an element type that holds
  references.
- **Door B's own form**, `(*A)(p)[i]` over such an element, is declined at conversion by `arrayPointerViewOfBase`
  (16.1), so it is emitted as before the doors. The refusal is the same refusal at three doors.
- **slicecolsgo** (the same records in a Go array) is also red at H's tip: "pointer handed to C.go2cs_probe_col_fill is an
  element of a Go array or slice ... refuses the call". That is H's own arm and H's defect, not a door's. It is named here
  so that nobody reads it as one.

go-sqlite3 uses the view **read-only**. C fills `rc.colvals` (`_sqlite3_column_values` or `_sqlite3_step_values`) before
the view is cut, and the loop reads `col.typ`, `col.i64`, `col.f64`, `col.ptr` and `col.n` (sqlite3.go:3103-3175). The
records Go writes are `cargs`, a `[]C.sqlite3_go_col` in Go memory (`marshalFusedArgs`, `a := &cargs[i]`), which H's
image door already carries. No Go store lands in a C-memory `sqlite3_go_col` anywhere in the package.

### 23.2 Why no `ref E`

`C.struct_go2cs_probe_col` is `{ int typ; long long i64; double f64; const void *ptr; int n; }`, 40 bytes. Go lays it out as C
does (cmd/cgo's contract: `unsafe.Sizeof(C.struct_X{})` is C's `sizeof`, with explicit padding where the alignments
differ; go-sqlite3 mallocs `n * unsafe.Sizeof(...)` on it). The converted C# struct holds `@unsafe.Pointer ptr`, and golib's
`unsafe.Pointer` is a number plus `object? m_retainedSource`. So the struct holds a managed reference, the CLR gives it
auto layout, and its managed size and field order are not C's. A `ref E` into the C bytes would read the reference out of
C's `i64` or `ptr` word. That is the class-1 layout root (golib-gen rules), and why every door refuses it.

### 23.3 What golib already has

- **The element as a native box.** `ж<E>.FromNumber` over C memory mints `NativeBox<E>`. Its `FieldRef<TF>` reads a field
  at its **Go offset** in the native bytes, so `col.typ`, `col.i64`, `col.f64` and `col.n` through `&colvals[i]` are refs
  into C memory today, live in both directions. A field whose type holds a reference is refused there by name ("a managed
  reference where Go stores the value inline"). That refusal is the only reason `col.ptr` cannot be read.
- **A value decode.** `NativeBox<E>.ReadGoValue` (B2) decodes a reference-bearing struct out of the bytes at Go's offsets
  into a fresh `E`. It decodes array members only and refuses any other reference by name, "since memory holds no image
  of one".
- **The word rules.** H's mirror helpers (`ᴍin`/`ᴍout`, emitted per struct in `<os>/main.cgo.cs`) apply cgo's rules to a
  pointer word: `GoCgoArgs.VoidPointerMember` encodes nil as 0, a native pointer as its address, and refuses a Go pointer
  with cgo's own message. Coming back, any non-zero word is C memory, minted `new @unsafe.Pointer((uintptr)w)`, and a
  typed member goes through `PointerMemberBack`. COORD's OQ-2 ruling (031803Z) holds for C memory as it does for a copy
  back: by cgo's rules, a pointer word in C memory is never a Go pointer.

So a pointer word **does** have an image in C memory: the address it holds. B2's "no image" is right for a string, a
slice, a map, an interface or a func, and wrong for a pointer word.

### 23.4 The design

**The class.** A struct type declared by cmd/cgo (`_Ctype_struct_*`, or a typedef of one) whose references are all
pointer words: typed pointers and `unsafe.Pointer`, directly or in nested structs and arrays of such words. This is the
class H's mirror already carries a pointer member for. A cgo struct holding anything else stays refused by name.

**Read (golib).**
1. `NativeBox<E>.FieldRef<TF>` gains a word arm. A pointer-word `TF` at a native root is decoded from the word at its Go
   offset on each access: 0 is nil, and any other value is a native pointer that carries nothing (the rule of 23.3). The
   decoded value lives in a slot on the box, and the accessor returns a ref to that slot. Each access decodes again, so a
   read after C rewrites the word sees the new value.
2. `NativeStructMarshal.DecodeValue` decodes pointer words with the same rule, so a whole-element read (`v := s[i]`,
   `*p` read for its value, door B's load) is a fresh, exact `E`.
3. **Doors A, B and C**, at their V3 arm (C memory, an address no Go value owns), answer a **native struct window** for
   an `E` in the class, instead of the shadow or the array floor:
   - `Element<E>(p, i, N)`, `&s[i]` and the box `at<E>(i)` are a `NativeBox<E>` at `p + i * sizeof(E)`, with Go's
     size as the stride. Field access goes through 1, and the box's address crosses to C as it does today.
   - The window's indexer cannot hand out a `ref E` into C bytes. It returns a ref to a fresh decode (2), good for a
     read.
   - The window covers `[lo, max)` of the view, never `N`: 53,687,091 is a bound, as it is for every door.

**Write (converter, then golib).** A box cannot observe a write made through a ref it has already handed out (golib-gen
rules, native boundary class 2: "the remedy for a pointer-typed destination is a converter EMISSION change"). So:
4. A store to a scalar member reached through an index of a class `E` (`s[i].n = 5`, `cols.Value[i].n = 5`, `(*A)(p)[i].n =
   5`, compound and `++` included) is emitted through the element's box (`Ꮡ(s, i).n = 5`), whose field ref is live (1).
   Over Go memory this is the same storage, one element box later.
5. A store to a pointer-word member of a class `E`, through any box or index, is emitted as one explicit store call
   naming the field. A managed box assigns. A native box encodes: nil is 0, a native pointer is its address, and a Go
   pointer is refused by name. Go forbids a Go pointer in C memory (the cgocheck2 experiment reports it), and C bytes cannot hold a
   managed reference.
6. A whole-element store of a class `E` into C memory (`s[i] = v`, `*p = v`) stays refused by name: `NativeBox<E>.Value`
   already refuses the whole-value ref for a reference-bearing struct. Encoding it from H's per-member rules is a
   follow-up row if a consumer appears. go-sqlite3 has none.

**Coherence.** None is needed, and that is the reason for this shape. Every scalar access is a ref into the C bytes, every
pointer-word read decodes at the access, and every pointer-word store encodes at the store. No managed copy outlives an
expression, so nothing has to be flushed before a cgo call or refreshed after one, and a `C.free` leaves nothing behind
that a later call would read.

### 23.5 The shapes not taken

- **A decoded shadow window** (ShadowArrayBox with a codec over foreign memory). It needs a flush of Go's writes before
  every cgo call and a refresh after every call. It needs an invalidation on `C.free`, or a refresh reads freed memory.
  A read through one alias after a write through another, with no call in between, is stale. ShadowArrayBox works because
  only Go writes the runtime's own blocks, and C writes these.
- **A snapshot, read-only by contract.** It is the smallest change and right for go-sqlite3 alone. But a write through it
  is lost silently, and nothing can detect the write (the same class-2 rule), so it would ship a silent lost write.
- **Per-type mirror codecs in golib** (a static-abstract interface the converter implements on each `_Ctype_struct_*ᴍ`).
  It is not needed: since cmd/cgo gives Go's layout as C's, the Go-offset shape golib already computes is the C layout.
  The mirror's `CheckLayoutᴍ` stays the cross-check, and a guard arm compares the two offsets for the fixture structs.

### 23.6 Cost

| Part | Where | Size (estimate) |
|:--|:--|:--|
| word arm of `FieldRef`, word decode in `DecodeValue` | golib `ж.NativeBox.cs`, `NativeStructMarshal` | ~70 lines |
| store call: assign on the managed kinds, encode on `NativeBox` | golib `ж.cs` and the box kinds | ~80 lines, virtual with an assigning default; the binary-compat check owed |
| native struct window at the V3 arm of doors A, B and C | golib `unsafe.cs`, `slice.cs` (a store kind beside the native mode) | ~120 lines |
| admission of the class in `arrayPointerViewOfBase`; 4 and 5 | converter `arrayPointerElementDoor.go`, `convIndexExpr.go`, `visitAssignStmt.go` | ~150 lines |
| guards | GolibTests ~25 arms; converter test arms; p3 e2e arms (slicecolsc, slicecolsview, a scalar store, a pointer-word store, a Go pointer stored and refused, a read after C rewrites a word) | ~1 day with the above |

- **Corpus footprint: zero by construction.** The class is cmd/cgo's types, and the corpus is emitted at
  `CGO_ENABLED=0`, where none exists. The two-seeded diff on four targets is still owed, as the proof of that.
- **Run-time cost** is on cgo code only. One element box per indexed store of a class `E` (4). One decode, plus one
  pointer object for a non-nil word, per pointer-word read. go-sqlite3 reads `col.ptr` once per text or blob column.
- **Order.** It stacks on H's slice (the `void *` member class, `VoidPointerMember`) and lands after it.
- **Limits.**
  - Generic code converted without rule 4, such as a `slices` function instantiated on a class `E` and handed a C-memory
    slice, stores through the indexer's fresh decode and loses the write. It is the one silent shape left. go-sqlite3 has
    no instance; the guard names it.
  - Pointer arithmetic over the window (`unsafe.Add` inside an element) reads C bytes, as Go does; that is not a door
    question.

### 23.7 For COORD

**Recommended:** one seat on this design, red first, with the six p3 arms of 23.6. It stacks on H's slice and is sized
from the table. If the seat is to be smaller, cut the read half alone (1 to 3, and the admission): go-sqlite3 needs only
that. But rule 5 must then be cut as a refusal by name, because a pointer-word store through a native box would
otherwise land in the read slot and be lost silently. P3 reads this note.

### 23.8 Amendment 2026-11-28 (P5): P3's review, and the design as amended

P3 read 23.1 to 23.7 at 1913dae537 (APPROVE WITH CHANGES, 072002Z): three MUST-FIX and two SHOULD-FIX findings. Each was
checked at master 4a56f17a3a and holds. COORD had approved the design in principle (071332Z), with the code to start
on P3's CLEAR. Where this subsection and 23.3 to 23.7 differ, this one is later. Nothing above is rewritten.

<!-- Provenance. P3's verdict: claude/mailbox docs/phase4/inbox/COORD/20261128T072002Z-P3.md (copy to P5 at 072022Z).
     Checked at master 4a56f17a3a: src/go2cs/cgoDeclarations.go cgoStructMarker (:295-335, gated on cgoRouteActive, a
     direct pointer or unsafe.Pointer field only); src/core/golib/ж.CgoFields.cs cgoPointerField (get and set) and
     cgoWordField; src/core/golib/CgoPointerWords.cs ReadWord and WriteWord; src/core/unsafe/unsafe.cs LoadWord and
     StoreWord (W1, W2). The p3 fixture as converted by H's converter carries [global::go.golib.GoCgoStruct] on
     struct_go2cs_probe_col, _cursor and _entry (package_info.cs). -->

1. **The pointer-field accessors are already on master** (DESIGN-cgo-phase3-marshaling 18.9). This is finding 1; 23.3
   missed them. The converter stamps `[GoCgoStruct]` on a cmd/cgo struct with a pointer or `unsafe.Pointer` field, on
   the -cgo route only (`cgoStructMarker`). go2cs-gen gives each such field a get and set property on the box in place
   of the ref one: `cgoPointerField` for a typed pointer, and `cgoWordField` with the word doors `LoadWord` and
   `StoreWord` for an `unsafe.Pointer`. So `col.ptr` through `col := &colvals[i]` never reaches `FieldRef`, and the read
   and the store through a `NativeBox<E>` over C memory work today (GolibTests `CgoPointerFieldTests`). The sentence in
   23.3 that `FieldRef`'s refusal is "the only reason `col.ptr` cannot be read" does not hold for a marked struct.
   **Amended:** item 1 (the `FieldRef` word arm) is dropped. Item 5's store call is dropped: a pointer-word store
   through an index is rule 4's emission, `Ꮡ(s, i).ptr = v`, which binds the existing setter.
2. **One read rule, the accessor's** (finding 2). 23.3 and item 2 took "any non-zero word is C memory", from the
   copy-back. The accessor's read is stricter. 0 is nil. A word that names Go memory (the registry, a live pin, a
   tagged token) is refused by name, since go2cs keeps no record of a Go pointer stored in C bytes. Any other word is
   C memory (`CgoPointerWords.ReadWord`). The copy-back's rule differs because it knows the word it copied in.
   **Amended:** item 2's word decode calls the accessor's reader, its classification included, so `colvals[i].ptr` and
   `col.ptr` answer alike. The stores already agree.
3. **No write may reach the window's indexer** (finding 3). Items 3 and 6 contradicted each other: `s[i] = v` is emitted
   through the indexer, so it would land in the fresh decode and be lost silently, as would a swap, `s[i].inner.x = 1`,
   `s[i].arr[j] = x`, any emission passing `ref s[i]`, and golib's own copy into the window, `clear`, and `append`
   within capacity. **Amended:**
   - The native struct window's indexer **refuses by name**. It hands out no ref.
   - The converter emits **every** access whose path starts at an index of a class `E` through the element box. That
     covers a read by value (the box's value read, which is item 2's decode), a field read, a field store, a
     whole-element store, compound assignment, `++`, and a swap. This replaces rule 4.
   - Each such access lands on a live field ref, on the accessor, or on a refusal by name. A whole-element store
     reaches `NativeBox<E>.Value`'s refusal (rule 6). An array member reaches `FieldRef`'s refusal of a managed
     reference.
   - golib's own writers over a native struct window (copy into it, `clear`, `append` within capacity) refuse by name.
   - Generic code converted without the rule (the one silent shape of 23.6) now reaches the indexer, so it refuses by
     name. **No silent shape remains.**
   Over Go memory the box route reads and writes the same storage, so the window's refusal binds only C memory.
4. **The class is the marker's** (finding 4). 23.4's class was wider. The marker counts only a direct pointer or
   `unsafe.Pointer` field of a cmd/cgo struct. A nested struct value's field, an array of pointers, a defined pointer
   typedef, and a Go type defined over a C struct are not stamped, and none of them has an accessor. **Amended:** the
   seat's class is `[GoCgoStruct]`. The doors refuse the rest by name, as today.
5. **`DecodeValue`'s word arm is keyed on the marker** (finding 5). Without the key, an unmarked Go struct with a pointer
   field, read by value over foreign bytes (transpiled C), would decode native pointers where it is refused today.
   **Amended:** the word arm runs for a `[GoCgoStruct]` type only.

**The amended seat.**
- golib: the word arm of `DecodeValue` (marked types, the accessor's reader); the native struct window at the V3 arm of
  doors A, B and C, with a refusing indexer; and refusals in golib's copy, `clear` and `append` over the window. About
  200 lines.
- converter: admission of `[GoCgoStruct]` element types in `arrayPointerViewOfBase`, and the box route for every access
  starting at an index of such a type (amended rule 4). About 180 lines.
- Every predicate is the marker's, which is gated on `cgoRouteActive`. The standard library converts off the route, so
  the corpus footprint is zero by construction; the two-seeded four-target diff is still owed as the proof. The box
  route also applies to Go-memory slices of a marked type, so cgo fixture goldens move. The seat's prediction names
  them before the run.
- Still about a day and a half, stacked on H's slice tip once H posts it SEAT READY, behind the cgo arm's rerun and the
  D7 burst (COORD 071332Z).

**Arms against `go run`** (the owner's rule: each shape let through, and each refusal, including the near-misses of
findings 3 to 5). Through the doors over C memory:
- **Let through:** slicecolsc and slicecolsview as read; a scalar-member store through an index; a pointer-word store
  through an index; a read after C rewrites a word; a whole-element read by value.
- **Refused by name:** a Go pointer stored into a pointer word; a word naming Go memory, read through an index and
  through the box, both refused alike; a whole-element store; a swap of two elements; an array member's element store;
  a generic `slices` call that writes, reaching the indexer; `copy` into the window, `clear`, and `append` within
  capacity; an element type outside the marker (a nested struct's pointer field, an array of pointers); an unmarked Go
  struct with a pointer field read by value over foreign bytes.
- Each refusal's Go answer is noted beside its arm: where Go itself accepts the shape, the arm records the refusal as
  a narrowing, with its BOARD row.
