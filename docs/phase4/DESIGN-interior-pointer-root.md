# DESIGN: a pointer made from an address keeps its root, and a pointee that holds references is never read out of raw memory

> - **Status:** FOR REVIEW. No code is cut. Lane P5, 2026-11-14, by the owner's ruling of that date (COORD's
>   post to P5, 06:54Z): P5 owns the design for the interior `unsafe.Pointer` root, which covers cgo Phase 3
>   increment 3.1's two must-fix families and sqlite-cs's func store at a computed address. H implements 3.1 on it.
> - **Read and measured at:** master `314e1d8b60` (batch 302) for every measurement; H's
>   `claude/h-cgo-phase3-helpers` at `83fbf28560` for what that branch already closes. Go 1.26.10, one cloud
>   linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400), Release, `GoTargetOS=linux`.
> - **Modules:** modernc.org/sqlite v1.62.0 and modernc.org/libc v1.80.1, sqlite-cs's pins, read from the module
>   proxy. sqlite-cs's own tree was not read.
> - **Evidence:** `docs/phase4/data/interior-pointer-root/` (the program, the census tool, its per-target tables,
>   the stress program, the prototype patch). Its README says how to re-run each.
> - **Not seen:** the text of the narrow re-review of 3.1. Its findings are taken from COORD's summary of them
>   (06:52Z), and each cited site was read at the tree.

## Summary (for review)

**One rule, two halves.**

- **(a) A derived pointer keeps its root.** Every door that makes a pointer from an address hands on what the
  address was derived from: the box, and with the box its pin. Four doors do not today: the conversion
  `(ж<T>)(uintptr)(p)`, the `uintptr` arithmetic form, `Pointer.FromRef`, and the array windows.
- **(b) A pointee that holds managed references is never materialised out of raw memory.** A func read or stored
  through an address is its WORD (GoFuncCookie). Any other reference-holding pointee over raw memory is refused by
  name at the dereference.

**What is measured.**

- A 12-shape program: 3 shapes answer wrongly at master and at H's tip, the same in each of 3 runs (section 1).
- A throwaway prototype of half (a)'s typed door plus half (b)'s func store, applied by hand to the program's
  emission: 2 of the 3 wrong shapes turn right. The third (pointer identity) needs increment I5 (section 4.6).
- A census: 321 such conversions in the standard library across 5 targets, 32 outside `runtime` and
  `internal/runtime/maps`; 5,929 in sqlite with libc on linux/amd64 (section 3).
- No stdlib site is a measured failure. One reached site has the shape of batch 301's defect and read 0 wrong
  messages in 3 stress runs of 300,000 (section 1.3).

**What the fix costs.** One spelling changes on every converted line that converts an `unsafe.Pointer` to a typed
pointer: 916 occurrences in the committed corpus today, all flavours together. No Go line is added or removed. No
field is added to `ж<T>`, `slice<T>` or `unsafe.Pointer`. One field is added to one sealed box kind
(`NativeArrayBox`).

**Decisions asked of the review** are in section 8.

## 1. What is wrong, measured

### 1.1 The program

`data/interior-pointer-root/InteriorPointerShapes/main.go`. Each line is a pointer converted from an address
inside a Go value, then read, written, compared or held. `go run` prints 17 lines. The converted program is
built from master's own emission.

| # | Shape | Go | master | H's tip | prototype |
|:--|:--|:--|:--|:--|:--|
| 1 | a func stored at `&tbl + 8` (vfs/patches64.go's line), then the slot compared with the func's word, then called | landed, equal, 5 | landed, **not equal, panic by name** | **as master** | right |
| 2, 3 | a word stored at `&tbl + 16` by `uintptr` math, at `+24` by `unsafe.Add` | 42, 43 | right | right | right |
| 4, 5 | a read and a write inside a local struct | 2, 30 | right | right | right |
| 6 | a slice element's interior, written and read | right | right | right | right |
| 7 | an array field's interior | right | right | right | right |
| 8 | the interior pointer is the ONLY reference to its base; read, collect, write, collect, read | 2, then 22 | 2, then **0** | **as master** | right |
| 10, 11 | the base stays referenced; the interior pointer is held across collections | right | right | right | right |
| 12 | a base that holds a string; the interior pointer is the only reference | 66 | right | right | right |
| 9 | `(*int64)(unsafe.Add(unsafe.Pointer(&p), 8)) == &p.y` | true | **false** | **false** | **false** |

- Shape 1 at master: the slot holds the delegate's raw object reference, a heap address the collector neither
  tracks nor updates. It is not 0 and it is not the func's word. The panic is `go2cs: 0x7fe1bdcfcac8 is not a func
  word -- no func value was read as this number`.
- Shape 8 at master: the write of 22 went to where the base no longer is. That is a write into memory the
  collector has reused.
- Shape 12 is right because a base that holds references has no address: its number is an order token, and the
  token arithmetic route (`GoReflect.ResolveInteriorAlias`) answers an alias that holds the base.

### 1.2 What "the store lands in a read copy" was

G's BOARD row says of w2 at the base: "the store lands in the read view the reinterpret hands out, and the slot
stays 0". That is the OFFSET 0 form before G's seat: `Reinterpret<uintptr, F>` answers
`new StandardBox<TDst>(fn)` (ж.PointerExtensions.cs, the `FuncCookieReinterpret` arm), a fresh box, so the store
lands in it. G's seat fixed offset 0 in the converter. At a COMPUTED address the path is different (1.1, shape 1):
the conversion mints a native box of a delegate type, and the store writes a reference into raw memory.

### 1.3 The reached site, stressed

`syscall.UnixRights` and `UnixCredentials` write each value through `Cmsghdr.data`, which the corpus emits as
`(@unsafe.Pointer)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref h) + …)` (syscall/linux/sockcmsg_unix.cs:28, and
the darwin twin). `FromRef` is a `fixed` address for one statement: no pin and no root. `h` is a managed view of a
fresh byte slice, which nothing pins. A collection that moves the slice between the address and the store loses
the write and corrupts the heap.

`data/interior-pointer-root/CmsgStress/main.go`: 64 descriptors, 300,000 messages, 3 goroutines allocating and
forcing collections. **3 runs, 0 wrong messages each.** The window is a few instructions wide. This is recorded as
LATENT. It is not evidence that the site is safe.

### 1.4 H's tip

The same emission built against `83fbf28560` (0 build errors), 3 runs: the output differs from Go on the same
three shapes as at master, 1, 8 and 9, and on nothing else, in each run. H's branch keeps the root through
`unsafe.Add`, a field reference and a reinterpret, but the emission still casts the result to `uintptr` before it
reaches `ж<T>`, so what `Add` carried is dropped there. Prediction P0 in section 6 was written before this was
read.

## 2. The mechanism, read at the tree

### 2.1 What a pointer made from a number can hold

A native box (`NativeBox<T>`) is an address. It can hold two more things: a PIN (`m_pin`), which keeps managed
storage where the address says, and a RETAINED SOURCE (`m_retainedSource`), which keeps a box alive. A pin lives on
the box whose address was taken (`ж<T>.EnsureStableAddress`), and a finalizer releases it when that box dies. So a
derived pointer that holds neither is valid only while something else keeps the first box alive.

`unsafe.Pointer` can carry its box too (`Pointer.RetainedSource`), and at master `unsafe.Add` over a carrying
pointer hands it on (`NativeDerivation.Moved`).

### 2.2 The three converter doors that drop it

| Door | Emitted for | What is lost |
|:--|:--|:--|
| `(ж<T>)(uintptr)(p)` (convCallExpr.go:964 and :985 at H's tip) | every `(*T)(p)` whose operand is an `unsafe.Pointer` that is not a recognised reinterpret | `p`'s retained source, at the `uintptr` cast. The number alone reaches `ж<T>`'s operator (ж.cs:976). An exact registered address still resolves; an interior one mints `new NativeBox<T>(number)` (ж.cs:1199). |
| `(@unsafe.Pointer)((uintptr)p + n)` | `unsafe.Pointer(uintptr(p) + n)` | the same, one step earlier |
| `@unsafe.Pointer.FromRef(ref x)` (convCallExpr.go:1054 and :3653) | `unsafe.Pointer(t)` where `t` is a receiver or a deref-aliased parameter | there is nothing to lose: no pin was taken and no box is carried |

Corpus counts at master, tracked non-test `.cs` under `src/core`, all flavour folders together
(`data/interior-pointer-root/corpus-counts.py`, its output beside it): 916 occurrences of the first door outside
golib and unsafe (runtime 680, internal 135, reflect 58, syscall 31, net 8, crypto 3, time 1), 12 of them directly
over `@unsafe.Add(`; 41 `FromRef` calls (runtime 22, syscall 4, internal/sync 4, internal/runtime 4, reflect 3,
internal/abi 3, sync 1). Seven lines add an offset to a `FromRef` number: four are `runtime`'s `malloc.cs:2575`,
one per flavour, and three are outside runtime, internal/abi/abi.cs:103 and the two `sockcmsg_unix.cs:28`.

> **Corrected before this posted.** The census post of 07:11Z said 900. That count came from a greedy pattern
> that reads two casts on one line as one, over a file list that dropped the 17 golib files with a non-ASCII
> name. The script matches the type argument by bracket depth: 919 in all, of which 3 are golib's and unsafe's
> own (2 comments and one line inside `ж.cs`).

### 2.3 Every native mint at H's tip

Read with `git grep` at `83fbf28560` for `new NativeBox<`, `NativeArrayBox<…>.Over(`, `OpenArray(` and
`OverNativeMemory(` under `src/core/golib` and `src/core/unsafe`.

| Site | Keeps today | Under this design |
|:--|:--|:--|
| ж.cs:1199, the number operator | the box an EXACT address resolves to (`KeptResolved`) | unchanged for a bare number; the typed door (4.1) passes what the pointer carried |
| ж.FieldRefBox.cs:319, a field of a native struct | `Kept(m_source)` | unchanged |
| ж.FieldRefBox.cs:332, an ARRAY field of a native struct (`OpenArray`) | **nothing** | carries `Kept(m_source)` (4.4) |
| NativeArrayOpeners.cs:61, builtin.cs:2869, `NativeArrayBox.Over` | **nothing**: the kind has no slot | gains a retained source (4.4) |
| ж.NativeArrayBox.cs:98, its element view | **no anchor** | anchored on the box's retained source (4.4) |
| ж.PointerExtensions.cs:201, reinterpret of a native box | `Kept(box)` | unchanged |
| ж.ElemRefBox.cs:457 | pin and `this` | unchanged |
| unsafe.cs:953, typed `unsafe.Add`; ж.NativeDerivation.cs:51, `Add` on a pointer | `Kept` | unchanged |
| unsafe.cs:1059, `unsafe.Slice` over a native pointer | anchor from `Kept(ptr)` | unchanged; its native test is 4.5 |
| builtin.cs:2516, 2545, 2633, 2663, an element of a native slice | the slice's anchor | unchanged |
| slice.cs:159 with ж.HeaderSliceBox.cs:123 and ж.SliceHeaderBox.cs:249, a slice header rebased onto an address | **no anchor** | 4.5: anchored when the address resolves to Go memory |
| GoTypeOps.cs:101 `NewNativeBox(address)` | nothing | NOT CLASSIFIED here: each caller is read at the seat and listed in the guard (4.7) |
| runtime/GoCgoArgs.cs:103; ж.NativePointerArrayBox.cs:290; ж.PointerTokens.cs:576, 582, 586 | nothing | C memory or a token by construction; listed with that reason in the guard (4.7) |

### 2.4 The pointee half

`NativeBox<T>.Value` and `ValueSlot` are `ref Unsafe.AsRef<T>(address)` (ж.NativeBox.cs:135, 138). For a `T` that
holds a managed reference, that reads raw bytes as a reference and writes a reference into memory the collector
does not scan. Three members are known:

| Member | Where | Today |
|:--|:--|:--|
| a func stored at a computed address | sqlite vfs/patches64.go:18-26, nine stores on every 64-bit target, in `init` | shape 1: a raw reference in the slot, then a panic at the first call through it |
| a func read from memory | runtime/panic.go:969 (a defer slot), runtime/mcleanup.go:203 | C1's census left both "on today's emission"; not measured here |
| a Go type whose C# form holds a reference | libc pthread_musl.go:136, 177, 190: C memory read as `*sync.Mutex`. Go's Mutex is two words of numbers; the hand-owned C# Mutex holds a `SemaphoreSlim` reference (sync/mutex.cs:70) | a managed reference fabricated from 8 bytes of C memory, at thread create, join and exit on linux |

The third member is why the test is on the C# type (`RuntimeHelpers.IsReferenceOrContainsReferences<T>`) and not
on the Go type: Go's own answer for `sync.Mutex` is "holds no pointer".

### 2.5 Is it one mechanism?

Half. The two families of the 3.1 review, shape 8 and shape 9 are one defect: a derived pointer keeps no root.
The func store goes through the same door, and keeping the root would not fix it: with the root kept, the store
still writes a reference where Go writes one word. It needs half (b) as well.

## 3. Census

**The tool.** `data/interior-pointer-root/ipcensus/main.go`, on go/packages and go/types. A site is a conversion
`(*T)(e)` where `e` is an `unsafe.Pointer` made by arithmetic: `unsafe.Add`; `unsafe.Pointer(u)` with `+` or `-`
inside `u`; a call to a function that returns one of those over its parameter or receiver (found to a fixpoint: 43
helpers in std on linux/amd64, `runtime.add`, `reflect.add`, `abi.addChecked`, `(*Cmsghdr).data` among them); or a
local assigned from one. Each site is classified by use (read, write, held) and by whether `T` holds pointers in
Go's sense. `CGO_ENABLED=0`, test files excluded.

**Standard library, 5 targets** (linux and darwin on amd64 and arm64, windows/amd64; 0 load errors each; 305 to
313 sites per target, 321 in the union):

| | sites | read | write | held | pointee holds pointers |
|:--|--:|--:|--:|--:|--:|
| `runtime` (197) and `internal/runtime/maps` (92) | 289 | 90 | 70 | 129 | 89 |
| everything else | 32 | 9 | 8 | 15 | 13 |

The 32, by package: reflect 11, internal/abi 7, syscall 5, internal/runtime/gc/scan 3, internal/syscall/execenv 2,
unique 1, sync 1, internal/syscall/windows 1, internal/reflectlite 1. The table is
`data/interior-pointer-root/std-outside-runtime.txt`.

**The 15 held sites outside runtime and maps, each read.** The deterministic failure (shape 8) needs a base that
only the interior pointer keeps alive.

| Site | Base | Verdict |
|:--|:--|:--|
| internal/abi abi.go:102 | the receiver's bitmap; the pointer is read in the next statement | `FromRef` plus offset, used at once: half (a), 4.3 |
| internal/abi type.go:241, 248 (`Methods`, `ExportedMethods`) | type data | emitted as `FromRef`, then a COPY through `new ReadOnlySpan` (type.cs:230, 243): a read at once from an unpinned address |
| internal/abi type.go:525, 536 (`InSlice`, `OutSlice`) | type data | hand-owned (type_impl.cs:700, 704); not emitted |
| internal/abi type.go:597, 603; internal/reflectlite type.go:151 | a name's bytes, type data | held by the type |
| reflect makefunc.go:172 | `args.Ptrs`, a local the frame holds | used in the loop body |
| reflect type.go:1207 | `staticuint64s` | static |
| reflect type.go:1760 | a module's type section | static |
| internal/syscall/execenv execenv_windows.go:44; syscall env_windows.go:93 | the environment block the OS returned | C memory |
| sync pool.go:302; unique clone.go:23 | | hand-owned files (`.cs.auto` siblings exist) |

None is shape 8. `runtime` and `internal/runtime/maps` are counted and NOT classified by reach: golib's own map
serves the map builtin, and most of `runtime` is displaced. That is a reading of the tree's structure, not a
measurement of each of the 289.

**sqlite v1.62.0 with libc v1.80.1:** 5,929 sites on linux/amd64 (35 packages loaded, 6 with sites), 5,630 on
windows/amd64 (23 loaded, 5 with sites). The per-package table is `data/interior-pointer-root/sqlite-summary.txt`. Nearly all are ccgo memory
held as numbers, which this design does not change. Over a Go variable's own address: the nine vfs func stores,
and a few locals used in the same expression (`&sIdxIter.Fu` in lib). Held: 9, all over ccgo numbers.

**Limits of the census.** It finds arithmetic it can see in one function or through a helper. A pointer stored in
a field and converted elsewhere is not a site. It counts Go source, not emission: a site in a displaced function
is counted.

## 4. The design

### 4.1 The typed door (half a)

**golib and unsafe.** One new static method on `unsafe.Pointer`, `[EditorBrowsable(Never)]`, one line in
`PublicSurface.golib.txt`:

```csharp
public static ж<T> To<T>(Pointer? p);    // Go's (*T)(p)
public static ж<T> To<T>(uintptr number); // the same for an operand that is already a number: today's operator
```

`To<T>(Pointer)` answers, in order:

1. nil for a nil pointer;
2. what `(ж<T>)(uintptr)p` answers today, whenever that is NOT a native box with nothing (an alias the registry
   resolved, a token route, a refusal, a native box that already retains);
3. otherwise the native box at that address RETAINING `NativeDerivation.Kept(p.RetainedSource)`, when that is not
   null;
4. otherwise today's native box (C memory, or a pointer that carries nothing).

It mints ONE box, as today: the number operator's body moves into a shared internal method that takes the carried
source, and the public operator calls it with null. No new field, no new box kind.

**The converter.** The two `(…)(uintptr)(%s)` sites emit `@unsafe.Pointer.To<T>(expr)`. Nothing else in the
conversion's routing changes: the reinterpret routes above it still answer first.

### 4.2 The arithmetic form (half a)

`unsafe.Pointer(uintptr(P) + n)`, in one expression, is Go's own valid form (unsafe's rule 3) and `go vet`'s
`unsafeptr` predicate. The converter recognises exactly it: a conversion to `unsafe.Pointer` whose operand is a
`uintptr` sum or difference with exactly one term that is a conversion from an `unsafe.Pointer` `P`. It emits
`@unsafe.Add(P', n)`, which carries. Anything else stays a number, and Go says of it what this design says: the
number does not keep its referent.

### 4.3 `FromRef` leaves the emission (half a)

Both converter sites emit `FromRef(ref x)` where a box is in scope under another name (a method's `Ꮡh` beside
`ref var h`). They emit `@unsafe.Pointer.FromPinnedBox(Ꮡh)` instead. With 4.2 this closes `Cmsghdr.data` and
abi.cs:103 by the rule. `FromRef` stays in unsafe's surface for hand-owned callers; the count of converted files
that call it becomes 0, and a repoguard arm holds it there.

- NOT READ: whether every emitted `FromRef` site has its box in scope. The seat reads the 41 before the change. A
  site with no box is stop line S3.
- One of the seven offset lines is `runtime`'s `(*notInHeap).add` (malloc.cs:2575 in each flavour). Its base is
  not-in-heap memory by its Go type, so the box in scope is a native one. `FromPinnedBox` over a native box must
  answer that box's own address and take no pin. The seat reads that arm before the change; an `add` that pins or
  refuses there is stop line S1.

### 4.4 The windows (half a)

`NativeArrayBox<T>` gains `private readonly object? m_retainedSource`, passed through `Over` and `OpenArray`.
`FieldRefBox.NativeArrayWindow` passes `NativeDerivation.Kept(m_source)`. The box's element view is cut with
`NativeSliceAnchors<T>.For(m_retainedSource)`, H's existing anchor. Cost: 8 bytes on each native array box and on
no other box, because the field is on the sealed leaf kind.

### 4.5 The SHOULD items

- **unsafe.cs:1057, `unsafe.Slice`'s native test.** It decides "genuinely native" by a registry MISS. With 4.1 a
  pointer over Go memory carries its root, so the test asks `Kept(ptr)` FIRST and the registry second. An interior
  address of Go memory then takes the anchored route, not the C-memory one.
- **The slice-header rebase** (`ReflectOverNativeMemory`): the header's data word is a number. When it resolves to
  a live box, the slice is anchored on it; otherwise it is C memory as today.
- **Finding 8's reflect pair** (`Value.UnsafePointer`, `Value.UnsafeAddr`): both hand out a registered number, so
  both come back through 4.1's step 2. They join the arms (section 5); no code is predicted for them.
- **The C-memory control.** Every arm in section 5 has a twin over C memory that must still pass. A refused
  C-memory twin is stop line S1.

### 4.6 Identity (half a, its own increment)

Shape 9 stays false under 4.1: a native box at `&p + 8` and the field alias `&p.y` are two kinds over one
location, and equality's native arm says a native pointer never equals a managed one. The tree already has the
remedy for a base with no address: `GoReflect.ResolveInteriorAlias(base, offset, T)` answers the field or element
alias when the base's Go layout has a `T` exactly there. Increment I5 applies it in `To<T>` step 3, when the
carried root is a managed box and the offset is the number minus the root's registered address. An alias holds its
root by construction and does not depend on the pin.

It is separate because it puts a layout walk on a conversion path. It lands only if its census (the walk counted
per banked row) reads 0 inside every banked allocation-asserted body.

### 4.7 The guard that keeps the rule

A repoguard arm lists every native mint site of 2.3 by file and enclosing member, each with its disposition
(`carries`, or `C memory: <reason>`). A new `new NativeBox<`, `.Over(`, `OpenArray(` or `OverNativeMemory(` call
that is not in the list fails the arm by name. Red first: one planted mint.

### 4.8 The pointee rule (half b)

- **B1, converter.** `*(*F)(e) = fn` with `F` a func type and `e` ANY `unsafe.Pointer` expression emits the word
  store G's seat emits for `e = &x`: `@unsafe.Pointer.To<uintptr>(e).Value = (uintptr)GoFuncCookie.Of(fn)`. This
  widens class w2's source and nothing else.
- **B0, census before any refusal.** A counter at `NativeBox<T>`'s dereference for a reference-holding `T`, by
  `T` and by whether the box retains a source, off by default (the shape of `Q44RegistryCensus`). It is read over
  the banked canaries of section 5.
- **B2, golib, only after B0.** For a delegate `T`, `Value` reads and writes the WORD through GoFuncCookie, and
  `ValueSlot` is refused by name. For any other reference-holding `T` with 0 banked arrivals in B0, the
  dereference is refused by name: `go2cs: *T read from raw memory: T holds managed references`. A `T` with banked
  arrivals keeps today's behaviour and is NAMED in the record with its count. B2 does not land on a guess.
- **libc's Mutex** (2.4) is a member of B2's second class. Under B2 sqlite-cs is refused by name at the first libc
  thread create, join or exit on linux, not at init. Today the same lines fabricate a reference. The remedy is a
  C-layout mutex for libc (a table keyed by address, in a libc hand-own). It is a SEPARATE item for the sqlite-cs
  queue and is not designed here.

### 4.9 The cgo doors

No door changes. `GoCgoArgs.StructPointer`, `HelperPointer` and the void pointer's door already refuse a native
box that retains a source ("not C memory alone"). They let the two families through because the boxes arrive with
nothing. With 4.1 and 4.4 they arrive with their root.

## 5. Acceptance: the arms

| Arm | Where | Red at | Green by |
|:--|:--|:--|:--|
| H's red arms for the interior conversion | H's ref (`claude/h-cgo-31-review2-red`, when it exists) | H's tip | 4.1, 4.2 |
| H's red arms for the array-field windows and the slices over them | the same ref | H's tip | 4.4, 4.5 |
| `InteriorPointerShapes`, behavioral, Output-compared | registered by the seat | master and H's tip: shapes 1, 8 and 9 differ, 4 lines of 17 (1.1) | shapes 1 and 8 by 4.1, 4.2 and B1; shape 9 by I5 |
| `InteriorPointerRootTests` (GolibTests): a derived pointer outlives its base across a forced collection, for the door, the arithmetic form, a receiver, an array window, a rebased slice header; each with a C-memory twin | new | master and H's tip | 4.1 to 4.5 |
| `FuncWordShapes` gains the computed-address store | existing behavioral | master (not built there: P4) | B1 |
| the mint guard | repoguard | a planted mint | 4.7 |

Names are proposed; H's are H's. Coordination of names goes through COORD.

**Canaries, read before and after, nothing re-baselined:** `runtime`, `os`, `syscall`, `reflect`,
`internal/runtime/maps`, `crypto/tls`, `net` (it reaches `UnixRights`), the five largest reflect consumers by
verdict count, and every banked allocation assert.

## 6. Predictions, to be scored as worded

- **P0 (written before 1.4 was read).** At H's tip the program's master emission still differs from Go on lines
  1, 8 and 9: the cast drops what `Add` carried. **HIT** (1.4: those three shapes and no other, 3 runs). "Lines"
  here and in P1 are the program's shape numbers, the first field of each output line; shape 1 prints two lines.
- **P1.** With 4.1, 4.2 and B1 the converted `InteriorPointerShapes` differs from Go on line 9 ONLY. With I5 it
  equals Go.
- **P2, corpus.** The two-seeded `-stdlib` diff, per target: every changed line holds the old cast, an
  arithmetic form of 4.2, or a `FromRef`; 0 `GoPositionMap` lines; 0 hand-own clobbered. The count per target
  equals the count of those three spellings in the BASE arm's emission, taken by `corpus-counts.py`'s matcher before the change.
- **P3, B1 in the stdlib.** 0 changed lines: C1's census found 0 w2 sites in std.
- **P4, sqlite.** The vfs package converted by the seat's converter emits 9 word stores for patches64.go and 3
  for patches.go:122-124 (already G's form).
- **P5.** No banked canary row moves and no banked allocation assert moves. `To<T>` mints the one box the cast
  mints.
- **P6.** GolibTests' total rises by the new arms. An existing row moves only if the red commit named it.
- **P7.** H's arms: every one red at H's tip is green, and every C-memory twin still passes.
- **P8, B0.** The census reads 0 arrivals for a non-delegate reference-holding `T` over a box that retains
  nothing, across the canaries. NOT KNOWN: reflect's `*(*unsafe.Pointer)(add(…))` sites (8 in value.go) may arrive
  here. If they do, P8 is MISSED and B2's second class does not land for that `T`.

## 7. Stop lines

- **S1.** A C-memory twin is refused, or an arm that passes at H's tip fails.
- **S2.** A corpus line changes that is not one of P2's three spellings, or a `GoPositionMap` line moves.
- **S3.** An emitted `FromRef` site has no box in scope. Stop and post the site.
- **S4.** A banked canary row or a banked allocation assert moves. Nothing is re-baselined.
- **S5.** The fix needs a field on `ж<T>`, `slice<T>` or `unsafe.Pointer`, a second new box kind, or a go2cs-gen
  change.
- **S6.** B0 reads a banked arrival for a class B2 would refuse. B2 does not land for it.
- **S7.** I5's walk is counted inside a banked allocation-asserted body. I5 does not land; shape 9 is disclosed.
- **S8.** A derived form the arms can build still crosses a cgo door after the fix. It is not disclosed.
- **S9.** `InteriorPointerShapes` passes at the red commit. The arm is then not measuring the defect.

## 8. Options not taken, and the decisions asked

**Resolve by range** (find the pinned box whose storage contains the number, inside the number operator). It
changes 0 corpus bytes. Not taken:

- it puts a lookup on every number-to-pointer conversion that misses, and sqlite's 5,929 sites do that on C
  memory by design;
- it answers from a weak registry, so it retains only what something else already keeps alive at that instant;
- it makes a stored `uintptr` keep working, which Go itself declares invalid.

**Register every derived address.** Not taken: one registry entry per `Add`, on paths the allocation asserts
cover.

**Decisions.**

1. The typed door changes one spelling on 916 corpus occurrences and on every behavioral golden that holds
   the cast. Does that enter THIS release, or does the release take the arms and the latent `Cmsghdr.data` close
   (4.2 and 4.3 alone, a few lines of footprint) and the door follows?
2. I5 (identity) now, or disclosed until its census is read?
3. The split. Proposed: H cuts 4.4, 4.5 and its own arms on its branch; P5 cuts 4.1 to 4.3, 4.7, B1, B0 and the
   program on H's tip; B2 and I5 follow their censuses.
4. The libc Mutex item: who takes it in the sqlite-cs queue.

## 9. What this does not show

- Nothing ran on windows or darwin. The census covers them; the program and the stress ran on linux/amd64.
- Native AOT was not built.
- The prototype touched 2 files and the program's emission by hand. It is evidence that the rule answers shapes 1
  and 8, not that the cut is sound.
- H's red arms were not run here.
- `runtime` and `internal/runtime/maps` (289 sites) were not classified by reach.
