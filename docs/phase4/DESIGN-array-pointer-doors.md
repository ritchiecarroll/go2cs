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
