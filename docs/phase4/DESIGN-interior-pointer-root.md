# DESIGN: a pointer made from an address keeps its root, and a pointee that holds references is never read out of raw memory

> - **Status:** FOR REVIEW. No code is cut. Lane P5, 2026-11-14, by the owner's ruling of that date (COORD's
>   post to P5, 06:54Z): P5 owns the design for the interior `unsafe.Pointer` root, which covers cgo Phase 3
>   increment 3.1's two must-fix families and sqlite-cs's func store at a computed address. H implements 3.1 on it.
> - **Amended 2026-11-14 (section 10):** the review's six must-fixes are answered, the four decisions are
>   recorded as ruled, and H's arms are named. Where section 10 and sections 3 to 8 differ, section 10 holds.
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

## 10. Amendment, 2026-11-14: the review, the rulings and H's arms

**Source.** COORD's review of 83d2baafea (inbox P5, 2026-11-14 07:34Z; verdict APPROVE-WITH-FIXES, six must-fixes),
COORD's post of H's arm names (07:36Z) and of H's first red arm (07:45Z). Every claim of the review was read at H's
tip 83fbf28560 and at master 314e1d8b60 before it was answered. Counts are from
`data/interior-pointer-root/review-counts.py`, run at master; its output is `review-counts.txt` beside it. Where
this section and sections 3 to 8 differ, this section holds.

### 10.1 Must-fix 1: a pointer from `FromBox` keeps its root and the root can move

**Read.** `FromBox` (unsafe.cs:500) ends `fixed (T* ptr = &box.ValueSlot) return new Pointer((uintptr)ptr, box);`.
The number is an address read for one statement and the box is retained; nothing is pinned.
`NativeDerivation.Kept` and `Moved` retain and do not pin. The review is right, and section 3's verdict on reflect
makefunc.go:172 ("a local the frame holds") is withdrawn: it answered lifetime and not movement.

**Measured by H (COORD, 07:45Z).** `ATypedPointerConvertedFromAnInteriorUnsafePointerKeepsItsStorageAlive` at H's
red tip b642ac3682: red 3 of 3 runs. After a compacting collection the typed pointer is a native box with no source,
the array is alive and has moved (about 8 MB), and the pointer still reads the old bytes at the old address.

**The rule.** Pinning in step 3 alone is not enough: the number was read before the pin, so a collection between the
mint and the derivation leaves a stale number, and a later pin holds the root still at a different address. So:
**a pointer whose carried source is a managed box takes its base from the box, never from its stored number.** A
pointer straight from `FromBox`, `FromBoxObject` or `FromPinnedBox` names its source at offset 0 by construction.
`NativeDerivation.Moved` (under `unsafe.Add`) and `To<T>` step 3, given such a pointer, read the source's own number
through the box's number operator (ж.cs:1214), which pins pinnable storage, registers it and answers the address
that now holds. That number is the base. A pointer that was already moved carries the native box `Moved` made: its
address came from a pinned base, and its retained root holds the pin (the handle is a field of the root box). It is
used as it is. `FromBox` itself does not change, so a pointer that is never derived from costs what it costs today.

| The root's storage | The base | `To<T>` at an offset |
|:--|:--|:--|
| pinnable: a reference-free pointee, or the data of a reference-free array | the pinned address | offset 0 is step 2 (the registry answers the root); otherwise a native box at base plus offset that retains the root |
| holds a reference (`StorageKind` None) | the box's order token | token arithmetic, which the number operator answers today: the field or element alias when Go's layout has a `T` exactly there, a refusal by name otherwise (`resolveInterior`, ж.cs:1204). No address is formed |
| unpinnable (`EnsureStableAddress` pins only when `PinnableStorage` is not null) | not stable | the alias walk first. With no alias, today's box, COUNTED by B0's counter as its own class. A refusal follows that census, as B2's does |

**Counts at master.** 295 `FromBox` calls in 74 files outside golib and unsafe (converted tests and review siblings
included). 2 lines derive from one in the same expression (reflect/makefunc.cs:130,
runtime/mgcmark_greenteagc.cs:817). A pointer that passes through a variable first is not visible to a text count,
which is why the rule is in golib and not in the emission.

**The guard (4.7) gains a column.** Every `new Pointer(number, source)` mint is listed with "the number is the
source's own address: yes or no". A mint that answers no over a movable source is stop line S10.

**The arm.** `InteriorPointerRootTests.AFromBoxDerivedPointerSurvivesACompactingCollection`, with H's compaction
recipe taken exactly (collect and finalize three times, large-object compaction once, a forced blocking compacting
generation-2 collection twice, the freed space refilled with 0xEE). The collection is placed in three positions:
between `FromBox` and `Add`, between `Add` and the conversion, between the conversion and the write. Each position
asserts the ADDRESS (the typed pointer's address equals the storage's current address plus the offset, H's
diagnostic) and then the value. A run in which the storage did not move is INCONCLUSIVE, never passed. Twins: the
same over `FromPinnedBox`, and over C memory.

### 10.2 Must-fix 2: 4.3 changes the number, not only the root

**Read.** `FromPinnedBox` is `new Pointer((uintptr)box, box)` (unsafe.cs:560), and the number operator answers the
box's order token when the storage holds a reference (ж.cs:1269-1280). The review is right.

**The population.** 42 code lines call `FromRef` outside golib and unsafe: 37 converted, 1 in a converted test
(reflect/all_test.cs:8425), 1 hand-owned (runtime/mfinal.cs:376), 3 in a review sibling (sync/waitgroup.cs.auto).
Section 2.2's 41 counted non-test files and included 3 comment lines. All 37 converted lines carry the `(uintptr)`
cast of convCallExpr.go:3653; the bare emission at convCallExpr.go:1054 has 0 converted lines. All 37 have their
box as a parameter of the enclosing member (read by script), so 4.3's "NOT READ" is now read and S3 does not fire
at master.

**The 37 lines are 25 Go sites, classified by the C# pointee.** Go's own "holds no pointer" is not the test:
`_func` and `spanInlineMarkBits` hold `array<byte>` fields, which are references in C#.

| C# pointee | Sites | Number under `FromPinnedBox` |
|:--|:--|:--|
| reference-free: 15 sites, 22 lines | internal/abi type.cs:230, 243 (`UncommonType`); internal/runtime/maps group.cs:132, 134, 142, 145 (`ctrlGroup`); internal/sync mutex.cs:67, 94, 189, 201 (`Mutex`); runtime malloc.cs:2575 (`notInHeap`, 4 flavours); stack.cs:1424 (`stackObjectRecord`, 4 flavours); mgcmark_greenteagc.cs:106, 107 (`spanScanOwnership`); syscall sockcmsg_unix.cs:28 (`Cmsghdr`, 2 flavours) | the pinned address, when the box is a standard box |
| an array: 1 site | internal/abi abi.cs:103 (`IntArgRegBitmap`, an `IArray<uint8>`) | the pinned address of the element data. `FromRef` answers the address of the wrapper struct, whose first word is a reference, so today's `+ i/8` reads the reference's bytes |
| holds a reference: 9 sites, 14 lines | reflect type.cs:559, 565, 571 (`rtype`); runtime mheap.cs:730 (`mheap`, 4 flavours); select.cs:567 (`hchan`, 3 flavours); symtab.cs:240 (`_func`); mgcmark_greenteagc.cs:146 (`spanInlineMarkBits`); synctest.cs:191 (`synctestBubble`); time.cs:1280 (`timer`) | the box's order token |

The table is the static half. The box kind decides too: a field or element reference whose pointee is reference-free
inside a container that holds references can answer its token (ж.FieldRefBox.cs:417-434). No count of that is made.

**What the number is used for, under a token.**

- Identity, order or an opaque word: select.cs:567 (the lock order), synctest.cs:191, time.cs:1280 (a race context),
  mheap.cs:730 (handed to `recordspan`, which converts it back: the token resolves to the box, where `FromRef`'s
  number resolves to nothing). A token is right here, and it is stable where `FromRef`'s number is not.
- Compared with a module's address range: symtab.cs:240, and stack.cs:1424 in the reference-free class. The number
  is in no module either way.
- Handed on as an `unsafe.Pointer`: reflect type.cs:559-571 (`resolveNameOff`, `resolveTypeOff`, `resolveTextOff`)
  and mgcmark_greenteagc.cs:146 (`memclrNoHeapPointers`). NOT READ: what each callee does with the number.
- Cast to a raw C# pointer: internal/abi type.cs:230, 243, the converter's span form
  `new ReadOnlySpan<E>((E*)(uintptr)(p), n)`. That form has 19 code lines outside golib and unsafe: 9 converted (the
  two above; runtime heapdump.cs:61, 71, 762; iface.cs:243; mbitmap.cs:1638; windows/os_windows.cs:582, 616), 6
  hand-owned, 4 in a review sibling. `UncommonType` is reference-free, so over a standard box the number stays an
  address; the read past the struct is wrong today and stays wrong (no method table follows an `UncommonType` in
  managed memory). Over a box that answers a token it would be the wild read the review describes.
  runtime/iface.cs:243 feeds the same form from `FromPinnedBox` at master.

**4.3 as amended.** An emitted `FromRef` site changes to `FromPinnedBox` only when every use of its number is one of:
the typed door or `unsafe.Add` (carried, 10.1), or identity, order or an opaque word. A site whose number reaches a
raw C# pointer, a span constructor or a callee the seat has not read KEEPS `FromRef` and is listed by name in the
repoguard arm, which therefore holds a list and not a count of 0. At master the list starts as internal/abi
type.cs:230 and 243, and reflect type.cs:559-571 and mgcmark_greenteagc.cs:146 until their callees are read.

**New stop line S10** is in 10.8.

### 10.3 Must-fix 3: the cast goes with the callee

**Read.** convCallExpr.go:3653 is `fmt.Sprintf("(uintptr)@unsafe.Pointer.FromRef(ref %s)", …)`. With only the callee
swapped, `@unsafe.Add((uintptr)FromPinnedBox(Ꮡg), i)` hands `Add` a number and the source is gone before the
derivation. The seat emits `@unsafe.Pointer.FromPinnedBox(Ꮡg)` with no cast. Where Go's own text converts to
`uintptr` (`uintptr(unsafe.Pointer(c))`), the outer cast is that conversion's and stays. P2's matcher counts the cast
as part of the `FromRef` spelling.

### 10.4 Must-fix 4: B1 when the slot is a managed func variable; lifetime

**Read.** `GoFuncCookie.s_delegateOf` is a `Dictionary<nuint, WeakReference<Delegate>>` (GoFuncCookie.cs:74, 149); a
static function's row is held strongly (:50, :132). The review is right on both points.

**B1 as amended.** The converter does not emit `To<uintptr>(e).Value = cookie`. It emits one golib call,
`@unsafe.Pointer.StoreFunc(e, fn)` (`[EditorBrowsable(Never)]`, one surface line), which decides at run time what `e`
names:

1. a managed func variable (the carried source is a box whose pointee is a delegate type, at offset 0): the func
   itself is stored, through the slot access `StoreThrough` already uses (unsafe.cs:600). This is class w1's result.
   A delegate type the slot cannot hold is refused by name;
2. a word: C memory, or reference-free managed storage (10.1's first row): the word store,
   `To<uintptr>(e).Value = (uintptr)GoFuncCookie.Of(fn)`;
3. the inside of a value that holds references (10.1's second row): the alias walk. A func field takes the func, a
   `uintptr` field takes the word, anything else is refused by name.

No arm writes 8 bytes over a reference.

**Lifetime, stated.** A static function's word is valid for the life of the process. A closure's word is valid while
something else holds the closure: stored only in raw memory, it can be collected, and its word then resolves to
nothing and is refused by name at the call. Go has the same rule for a func stored in memory its collector does not
scan. Read-back and equality are those of class w2 and of S1.

**Arms.** `FuncWordShapes` gains three rows: the store through a pointer that names a func variable at run time
(the variable is then called), the store into a word slot (read back, compared, called), and a closure's word after
its closure is dropped and a collection is forced (the named refusal). NOT READ: whether sqlite's nine stores are
all of static functions; the seat reads vfs/patches64.go:18-26 for it.

### 10.5 Must-fix 5: B0's reach, and B2 against the refusal that already exists

**Read.** `NativeBox<T>.ValueAddress` (ж.NativeBox.cs:124) already refuses by name
(`RuntimeErrorPanic.NativeStructLayoutMismatch`) a STRUCT `T` whose layout `GoReflect.ClrLayoutMismatch` proves is
not Go's, and a field that is a managed reference is such a proof (GoReflect.TypeLayout.cs:488). Section 4.8 did not
mention it. `ClrLayoutMismatch` answers null, and so refuses nothing, for a `T` that is not a struct and for a struct
whose Go layout it cannot read.

**B2, redefined.** B2 adds nothing where `ClrLayoutMismatch` already refuses. Its population is exactly
`IsReferenceOrContainsReferences<T>` AND `ClrLayoutMismatch(T) is null`, in two classes:

- B2a, `T` is itself a reference: a delegate, a pointer box, a string, a slice, an interface, an `unsafe.Pointer`;
- B2b, a struct that holds a reference and whose Go layout is unknown. libc's Mutex over the hand-owned C#
  `sync.Mutex` is the known member.

**B0, widened.** The counter sits in `ValueAddress` after the layout check, so it counts exactly B2's population, by
`T`, by class, and by whether the box retains a source; and 10.1's unpinnable class beside it. It is read over EVERY
banked row, on every flavour the roster banks (windows, linux/amd64, linux/arm64, and darwin where rows are
banked), by one sweep per flavour with the counter's variable set. P5 lands the counter and reads linux/amd64.
The other flavours are read by the lanes that hold those hosts, dispatched by COORD. B2 lands for a class only when
every flavour's reading is in and is 0 for it. The windows evidence that the linux/amd64 canaries cannot stand in:
net/windows/interface_windows_impl.cs:262 casts an OS buffer to `ж<windows.IpAdapterAddresses>`, a struct with
pointer fields.

### 10.6 Must-fix 6: H's ref, and S8 replaced by named arms

**The precondition is met.** `claude/h-cgo-31-review2-red` is on origin at b642ac3682 (read by `git ls-remote`,
2026-11-14): 8a8a52ff1e (unit) and b642ac3682 (end to end), on H's 83fbf28560. COORD's reading of it (07:47Z):
GoCgoArgsTests 34 tests, 28 passed, 6 failed, the 17 earlier rows all passing. P5 did not run it.

**The acceptance list, from H's ref.** Red, to turn green: U1, U2, U3, U4, U5 and C2. To stay green: U6, C1, K0 and
the 17 earlier rows. End to end: the seven `go*` arms read red and turn green, the six `c*` twins and the 40
existing arms do not move.

**What H's readings change in this design.**

- **Family 1 is "an interior address of a box's own storage".** U1 read 15 crossings, not 20: `&x[1]` over a local
  does not cross, because the element box's own address is registered and the conversion finds it in the registry
  (K0's answer at offset 0). An element box is not interior to itself. Sections 2.5 and 4.1 are to be read with
  that definition: step 2 of the door already answers an element's or a field's own address, and step 3 is for an
  address inside a box's storage that no box of its own names.
- U3 read 34 crossings, per view (a slice and a Go array): the reinterpret of `&h.name` 5, `unsafe.Add` 2 (untyped),
  `&s[1]` 5, `SliceData` 5. `&h.name[1]` crosses no door and loses its view, which is U4.
- U5 is a lifetime fault, not a crossing: the doors hold, and the view is collected while the slice of pointers is
  alive.
- U6 is green at H's tip: the reflect pair is refused at both void doors today. 4.5 predicted no code for it.
- **C2 is red: an over-refusal.** `&h.name[1]` over C memory is refused at the void door as a pointer into Go
  memory. S1 names a refused C-memory twin, so 4.4 (H's half) must turn C2 green, and P5's half must not refuse it
  by another route.

**S8 is replaced** by one named cgo-door arm per family, H's, each handing every derived form to every door it
reaches and asserting 0 crossings:

| Family | Arm (GoCgoArgsTests) | Forms | Doors or end-to-end rows |
|:--|:--|:--|:--|
| 1, the interior conversion | `ATypedPointerConvertedFromAnInteriorUnsafePointerIsRefusedAtEveryDoor` (U1) | `unsafe.Add` over `&b[0]`; `&h.n` of a view; `&x[1]` of a local; through a user function | `StructPointer`, `HelperPointer`, `VoidPointer` of both mints, `HelperVoidPointer`; end to end `goaddtyped`, `goaddfill`, `gofieldtyped` |
| 2, the array-field window | `AnArrayFieldOfAViewOverGoMemoryIsRefusedAtEveryDoor` (U3) | a reinterpret of `&h.name`; `unsafe.Add` over it; `h.name[:]` with `&s[1]` and `SliceData`; `&h.name[1]`; each over a slice and over a Go array | end to end `goviewnamestring`, `goviewnameadd`, `goviewnameslice`, `goviewnamedata` |

**The rest of H's arms, for section 5's table:**
U2 `ATypedPointerConvertedFromAnInteriorUnsafePointerKeepsItsStorageAlive` (plain Go, a compacting collection);
U4 `AnArrayFieldOfADroppedViewKeepsTheViewAndItsPinAlive`;
U5 `AnUnsafeSliceOfPointersOverAViewKeepsTheViewAndIsRefusedAtEveryDoor` (4.5);
U6 `TheReflectPairOverADerivedPointerIsRefusedAtTheVoidDoors`.
The controls for S1: `TheReviewsShapesOverCMemoryStillCross`,
`AnElementOfAnArrayFieldOfACStructCrossesTheVoidDoorAsItsAddress`,
`ATypedPointerConvertedFromAnUnsafePointerAtOffsetZeroIsRefused`, and the end-to-end twins `caddtyped`,
`cfieldtyped`, `cviewnamestring`, `cviewnameadd`, `cviewnameslice`, `cviewnamedata`.

**P5's own arms** are in `InteriorPointerRootTests`: the rebased slice header (`ReflectOverNativeMemory`), which is
not on H's ref, and 10.1's `FromBox` arm. Every arm there uses H's compaction recipe and the address assertion; a
test that collects without compacting can read green over stale bytes.

### 10.7 The SHOULD items

- **4.2, nested sums.** The recognised operand is a tree of `+` and `-` over `uintptr` in which exactly one leaf is
  a conversion from an `unsafe.Pointer` `P` and that leaf is never on the right of a `-`. It emits
  `@unsafe.Add(P', rest)`, where `rest` is the same tree with the leaf removed. `Cmsghdr.data` has two addends and
  becomes one `Add`. A mask is not a sum: mgcmark_greenteagc.cs:106 (`&^ 0b11`) stays a number and is named in the
  record as a residue.
- **`unsafe.Add(nil, n)`.** golib answers nil (unsafe.cs:924); Go answers the number `n`. With 4.2,
  `unsafe.Pointer(uintptr(p) + n)` over a nil `p` would change from `n` to nil. The seat changes `Add`'s nil arm to
  answer the number with no source, as Go does: one line, one arm red first (`AddOverANilPointerAnswersTheOffset`).
  It also changes every existing `Add` over a nil pointer with a non-zero offset from nil to that number; a canary
  that moves for it is S4. COORD may rule the other way (keep nil and disclose it).
- **Hand-owned files keep the old cast.** 34 hand-owned files hold it: 61 casts in the parenthesised spelling and 25
  in the unparenthesised one (`(ж<T>)(uintptr)x`, as sync/atomic/type.cs:137 and 159). The per-file list is the last
  block of `review-counts.txt`. The seat does not edit them. 4.7's guard covers golib's mints only, so the list is a
  second repoguard arm: a hand-owned file that GAINS a cast fails it by name. Each listed file is read once in the
  seat for a cast over a derived pointer; one found is posted, not fixed in passing.
- **`GoTypeOps.NewNativeBox` (GoTypeOps.cs:101).** Its caller NativeStructMarshal.Deep.cs:517 reaches it for a
  pointer word that native code left and that names nothing inside any Go copy the marshaller made (the copies are
  tested first, :505-514). That is C memory by construction: disposition `C memory: a pointer word native code
  returned`, listed in 4.7's guard with that reason.
- **A `uintptr` held across statements** is disclosed on the user page: docs/Limitations.md, "A few quiet ones", in
  this commit, with the identity difference of I5 beside it (ruling 2).
- **Performance.** `To<T>` mints one box (4.1 says so; the prototype's two mints are the prototype's). 4.3 puts the
  number operator on receivers that took a bare `fixed` before: one pin handle per box and one registry write per
  call. The seat TIMES it, beside allocations: `ctrlGroup.get` and `set` in a loop, and the banked
  `internal/runtime/maps` row's wall time, before and after, three runs each on one host. internal/sync's four
  sites sit under `race.Enabled`, a constant false (internal/race/norace.cs:13), so they never run and are not
  timed. The threshold is stop line S11.

### 10.8 Predictions and stop lines, added or changed

- **P9.** `AFromBoxDerivedPointerSurvivesACompactingCollection` is red at H's tip in all three positions on the
  address assertion, and green with 10.1. Its `FromPinnedBox` twin is red at H's tip only through the cast (no
  source), and green with 4.1.
- **P10.** B1's `StoreFunc` changes 0 stdlib lines (P3 stands) and the vfs package's 9 stores take it (P4's count
  stands; the spelling is `StoreFunc`, not the word store).
- **P2, restated.** The `FromRef` spelling includes its `(uintptr)` cast (10.3), and the count of changed `FromRef`
  lines is the 37 less the lines 10.2 keeps by name.
- **S8** is withdrawn and replaced by the two named arms of 10.6: either arm reading a crossing after the fix is a
  stop, and is not disclosed.
- **S10, new.** A mint whose number is not its source's own address over a movable source (10.1); or a 4.3 site
  whose number can be a token and reaches a raw C# pointer, a span constructor or a native memory primitive (10.2).
  The site is not changed. It is posted.
- **S11, new.** `ctrlGroup.get` or `set` slows by more than 25%, or the `internal/runtime/maps` row's wall time rises
  by more than 10%, on the same host. 4.3 does not land for the receivers measured; the numbers are posted.
- **S3** stands for any site the seat's own emission adds on another target or in a converted test.

### 10.9 The four decisions, as ruled (COORD, 2026-11-14 07:34Z; the owner may overrule)

1. **The door enters this release (1.26.11.1), as its own seat, after must-fixes 1 to 3.** It moves the corpus, so
   it queues in the corpus order right after R's hop and ahead of the visual seats. Fallback if S1, S2 or S4 trips:
   4.2, 4.3, 4.4 and the arms ship, and family 1 is disclosed on the user page.
2. **I5 (identity) is disclosed now** (docs/Limitations.md, this commit) and lands after its census.
3. **The split.** H cuts 4.4, 4.5 and its arms. P5 cuts 4.1 to 4.3, 4.7, B1, B0 and the program, on H's tip. B2 and
   I5 follow their censuses.
4. **The libc Mutex item goes to P2**, after its S8 seat. It is a separate BOARD row of this date.

### 10.10 What this amendment does not show

- No code is cut and nothing in this section was run by P5. H's readings are quoted from COORD's posts.
- The callees of 10.2's handed-on sites and sqlite's nine stores are named as NOT READ.
- The box-kind half of 10.2 (a field or element reference that answers a token) is not counted.
- `review-counts.py` reads text. A `FromRef` or a cast built across two lines is not counted.

### 10.11 4.5's first item, reworded for a pointer element (COORD, 2026-11-14 07:51Z)

4.5 says an interior address of Go memory "takes the anchored route". That is right for an element that holds no
reference and impossible for a POINTER element: `slice<T>.OverNativeMemory` refuses an element that holds a
reference (slice.cs:309-311), and `unsafe.Slice` already sends a pointer element down its own branch
(unsafe.cs:1053-1057, G's words route, which reads the n words into a managed array). So, in H's cut of 4.5:

- an element that holds no reference: as 4.5 says, `Kept(ptr)` is asked first and the slice is anchored on it;
- a pointer element: the branch keeps G's words route, and the fresh managed array it makes is tied to `Kept(ptr)`
  through the table `NativeSliceAnchors` already has (ж.NativeDerivation.cs:72). No field is added.

H adds two arms on its ref for this before cutting: a red arm for the rebased slice header, and a guard arm that an
element of a native slice anchored to a Go view stays refused at the void door. With the first, the header-rebase
arm of 10.6 is no longer P5's alone: P5's `InteriorPointerRootTests` keeps its own plain-Go row for it and does not
duplicate H's door row.

### 10.12 As cut: three rulings taken during the converter cut (COORD, 2026-11-14)

Dated lines, so the design and the cut agree. Nothing above is rewritten.

- **4.2 takes the LEFTMOST pointer leaf only** (ruled 09:36Z). 10.7 allows the leaf anywhere in the sum but on the
  right of a `-`. The cut recognises a chain of `+` and `-` whose FIRST term is `uintptr(P)`, which is `go vet`'s
  rule (unsafeptr's `isSafeArith`), and renders everything after that term as the offset. Read at the cut: no corpus
  site has the pointer anywhere else in a sum. Every `(@unsafe.Pointer)(sum)` the cut does not take, 72 lines, is a
  sum over a number held in a variable or a field. A rounded pointer (`uintptr(P) &^ m`), which vet also admits,
  stays a number, as 10.7 says of the mask.
- **4.3 is the node rule** (ruled (a), 08:40Z). 4.3 and 10.2 say `FromRef` leaves the emission. As cut, the
  receiver's box is emitted only at the three uses the converter can read at the conversion itself: the first
  argument of `unsafe.Add`, the `P` of a 4.2 form, and the operand of Go's own `uintptr(...)`. Every other use (a
  call argument, a return, a store) keeps `FromRef`. 13 sites per target are kept, each named in 4.7's guard with the
  reason "use not provable at the node". P2 is scored a MISS as worded, with that list beside it. The use walk that
  would retire the kept sites is a BOARD row for after the release.
- **4.7's kept list and the regen** (ruled 09:36Z). The guard arm reads the committed corpus, which holds the old
  lines until the regen. The list names the 13 kept sites with the ruled reason and names the sites the converter no
  longer emits as "leaves at the regen", so the arm is green before the regen and after it.

### 10.13 As cut: B0 as it shipped, 10.5 corrected, B2 moved after the release (COORD, 2026-11-14 12:20Z)

Dated lines. Nothing above is rewritten; where a line here disagrees with 10.5, this line is the one in force.

- **B0 as cut** (cut branch 65d9a4a990). The counter is `NativeBoxDereferenceCensus`
  (src/core/golib/NativeBoxDereferenceCensus.cs), called from `NativeBox<T>.ValueAddress` after the layout refusal
  and after `DereferenceableAddress`, so a refused dereference is not counted. It is off unless
  `GO2CS_NATIVEBOX_CENSUS` is set, and then writes one file per process (`GO2CS_NATIVEBOX_CENSUS_FILE`, `{pid}` in
  the name). Off, it costs one static null test per dereference; it refuses nothing and adds no public surface. The
  reader and its controls are docs/phase4/probes/p5-nativebox-census/.
- **10.5's known member is wrong.** `sync.Mutex`, the hand-owned C# struct, holds one managed reference at a known Go
  offset, so `ClrLayoutMismatch` already refuses it by name and it is NOT in B2b. Measured in the cut: GolibTests arm
  `TheHandOwnedMutexIsRefusedByLayoutAndIsNotInThePopulation`. libc's Mutex over it is therefore not a B2b arrival
  on a whole-value read; which read shape libc takes is P2's question (COORD, same post).
- **B2b is narrower than 10.5 says.** A struct with a string field, or with any reference field whose Go offset is
  known, is refused by the layout check. B2b is left with a Go struct value that holds a reference only in a field
  the layout check reads as zero Go bytes or cannot place: an unstamped `array<T>` field is the measured case.
- **An array value arrives as B2a.** `array<T>` holds its elements by reference in its CLR form and is not a Go
  struct value, so the counter classes it B2a, not "itself a reference" in 10.5's sense. Each row carries `kind=`
  for that reason. Read over GolibTests: 2 of B2a over `array<array<uint32>>`, source retained.
- **A third class beside B2.** UNPINNABLE: the box is derived, through any number of hops, from a managed box whose
  storage cannot be pinned (10.1's class). It is counted whatever the pointee is.
- **RULED: B2 and the every-banked-row reading move to after 1.26.11.1.** B2 (the refusal by name) does not land in
  this seat. B0's reading over every banked row on four flavours, which 10.5 makes B2's precondition, is not run
  before the release, and no hosted workflow option is added for it now. The counter ships in this seat, inert, as
  the instrument that later reading needs. In this seat's gates the counter is read over the canary rows on
  linux/amd64 only; an UNPINNABLE arrival in a canary is posted on its own. BOARD row: the cut branch.
- **What this leaves not shown.** Until that reading, nothing shows that B2's population is empty on windows, on
  darwin or on linux/arm64, and nothing refuses a B2 dereference by name. 10.5's windows evidence
  (net/windows/interface_windows_impl.cs:262) stands unread.


### 10.14 As cut: S11 as measured and the miss in its wording, the rooted member of 4.5, the canary reading (COORD, 2026-11-14 15:39Z)

Dated lines. Nothing above is rewritten; where a line here disagrees with 10.7, 10.8 or 4.5, this line is the one in
force.

- **S11 as measured** (linux/amd64, one host, one axis: the base converter against the cut converter over the same
  golib; Release). A loop of 20,000,000 calls, median of 5 interleaved runs, base to cut:

  | receiver | `set` | `get` |
  |---|---|---|
  | `new(ctrlGroup)` | 2101 ms to 1992 (-5%) | 1848 to 1967 (+6%) |
  | `(*ctrlGroup)(g.data)`, made once | 1903 to 2040 (+7%) | 1807 to 2015 (+12%) |
  | `g.ctrls().set` and `.get`, made at each call (the package's own callers) | 2447 to 2871 (+17%) | 2445 to 2922 (+20%) |
  | `&held.ctrls`, a field of a struct | 2115 to 3742 (+77%) | 1895 to 3685 (+95%) |

  In the shape internal/runtime/maps calls them, `get` and `set` are inside the 25% line. On a field receiver, a shape
  no corpus caller of `ctrlGroup` makes, they are over it: the number conversion of a field box costs about twice
  that of a box that owns its storage. What the conversion does per call on a field box is read in the BOARD row
  below; which part of that the difference is, is NOT MEASURED.
- **The row arm of S11 could not fire, and that is a miss in the stop line, mine.** The banked
  `internal/runtime/maps` row never reaches `ctrlGroup.get` or `set`: with both made to throw, the row's verdict set
  is identical and the throw is never printed (the tests outside the 19 matched stop earlier, at `newarray`'s refusal
  or at a dereference with no address, in both arms alike). And the sweep's wall time for the row cannot carry a 10%
  line: three readings at one emission read 218 s, 243 s and 196 s, all but about 2 s of it the publish. The host's
  own execution, 10 interleaved runs per arm, read 1.998 s and 2.005 s (+0.4%). The arm was written without checking
  that the row runs the code it was meant to time. A stop line names a measurement that has been seen to move with
  the thing it guards, or it says that it has not.
- **RULED (a): 4.3 lands as cut for `ctrlGroup.get` and `set`.** The field-receiver cost is a BOARD row for after
  1.26.11.1 (the cut branch), with what the conversion does per call on a field box and a candidate remedy. That
  row also carries the read 4.3 still owes: which of the other changed receiver sites are called on a field box in
  code that runs.
- **4.5's rooted member is a second member, not a parameter.** 4.5 as cut (86247389c0) added `object? root = null`
  to the published `IGoReflectSlice.ReflectOverNativeMemory`. The binary-compat guard read that as the published
  member removed and an abstract member added (CP0002, CP0006; baseline go.lib 1.26.10.3). As fixed on the cut branch
  (c79e1e106b): the three-argument member stays as published, and the member that takes the root is a second one
  with a default body forwarding to it. `ISlice<T>` implements both. No suppression.
- **The canary reading 10.13 asked for.** The counter over 14 rows on linux/amd64 (syscall, internal/runtime/maps,
  unique, sync/atomic, reflect, errors, encoding/json, encoding/xml, go/types, go/internal/gcimporter, net, os,
  crypto/tls, runtime): no UNPINNABLE, no B2a and no B2b arrival. Dereferences counted: sync/atomic 144,000,000,
  runtime 3,465,564, reflect 3,987, net 3,075, os 412, syscall 52, the rest 0. In crypto/tls, 4 of 3,432 processes
  were killed at the BoGo runner's deadline and are not measured past their start. The other three flavours stay
  unread, as 10.13 says.
- **The regen, after the go1.26.11 hop.** The two-seeded footprint was re-read on four targets on the merged base
  (3ff376f868) and each target's score equals its score before the hop: converted .cs lines 641 (linux/amd64), 634
  (windows), 628 (darwin), 642 (linux/arm64). Applied by the hunk rule to 160 files, 938 lines out and 910 in
  (837a413fdc on the cut branch); the
  13 kept FromRef sites of 10.12 stay at every target and no other old form is left. The .cs.auto review siblings
  (12 files across the targets) are named and not applied.
- **What the regen showed that 6 and 10 did not plan for: three instruments read the old SPELLING.** The
  native-boundary census and the native-root field-view census matched the cast `(ж<T>)(uintptr)` alone, and 4.1
  respells every converted site they count. The first went red on its declared rows; the second fell from 13 to 5
  and stayed green. Both read both spellings now. The comment census rose by 9 on five lines whose blank lines did
  not move, because it pairs a Go line with its C# line by identifier and 4.2 and 4.3 no longer spell `Pointer` or
  `uintptr` there; its baseline is raised by those 9 (COORD ruled (a), 17:04Z) and the census's anchor is a seat for
  its owner after the release. BOARD row: the cut branch. A cut that respells
  an emitted form searches the instruments for the old form before its regen, and says what it found.
