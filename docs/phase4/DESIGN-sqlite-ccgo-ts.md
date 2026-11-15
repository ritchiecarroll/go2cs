# DESIGN — a lifetime-stable data address for `reflect.StringHeader` (sqlite-cs wall W8: ccgo's `__ccgo_ts`)

> **STATUS: DESIGN, RULED (lane H, 2026-11-15; branch `claude/h-w8-ccgo-ts`, cut from master `091935257c`).** COORD
> ruled QW-1 to QW-5 on 2026-11-15 (section 9). This document adds QW-6, the memory-pressure question, with
> measurements. Nothing is built. The cut is golib only. It follows the pin-extent seat's batch-308 merge, gated, and
> never runs beside it. The adversarial review of section 5 (native memory lifetime, the finalizer, the first-read
> race) is P3's.
>
> **Amended 2026-11-15, 10:40Z: READ SECTION 10 FIRST.** After P3's read, COORD ruled the (L)+(H) split. Only
> package-level storage gets the (immortal) native copy; every other string takes the `&b[0]` route. Section 10
> supersedes D2, D4, section 5's race, section 6's choice, QW-7 and the arms list where they differ. The sections
> before it are the record of how the design got there.
>
> The census instrument, its controls and every probe named here are kept beside this file in
> [`data/w8-ccgo-ts/`](data/w8-ccgo-ts/). Amended by dated blocks, never rewritten.

## 1. What is open

ccgo, which modernc.org/sqlite and modernc.org/libc are generated with, keeps every C string literal of a package in
one Go string. It takes that string's data address once, as a number, for the program's whole life:

```go
var __ccgo_ts1 = "…\x00…\x00…"
var __ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data
```

Every literal is then `__ccgo_ts + N`, a `uintptr` handed to libc, which reads NUL-terminated bytes through it.

**go2cs refuses it at package init.** The emission is

```csharp
internal static uintptr __ccgo_ts = (Ꮡ__ccgo_ts1.Reinterpret<@string, reflect.StringHeader>()).Value.Data;
```

Run, it panics with "dereference of a managed pointer with no address (*StringHeader over 0x8000000500000000 — the
order token of a reference-bearing pointee at offset 0, Q44 §10.3 arm 2a …)" (`data/w8-ccgo-ts/readings-h.txt`).

**Why.** `StringHeaderBox` (`ж.StringHeaderBox.cs`) materializes Go's (Data, Len) view of a `@string`, but it admits
a header only when field 0 is the `unsafe.Pointer` class: `unsafeheader.String`, `runtime.stringStruct`.
`reflect.StringHeader`'s `Data` is a `uintptr`, so the box's `Applies` is false. The pair falls to the address route,
which mints an order token, because a `@string` holds a reference.

**Who reaches it.**
- modernc.org/sqlite v1.62.0 declares it 5 times: `lib/sqlite.go:13751` (what sqlite-cs builds on linux and windows
  amd64), `vec/vec.go` and three `vfs` files.
- modernc.org/libc v1.80.1 declares it once per linux architecture, and not on windows.
- In go2cs itself, no file of the converted standard library reinterprets a `@string` as a header with a `uintptr`
  `Data` (grep of `src/core`). The three GolibTests string headers all have an `unsafe.Pointer` `Data`.
- So the new path changes only code that panics today.

## 2. The census

The instrument is `data/w8-ccgo-ts/census-main.go`, a read-only `go/ast` pass with no type checking, run on the
module trees. It classifies every use of `__ccgo_ts` and `__ccgo_ts1`, every write through a number derived from
`__ccgo_ts` (judged at the DESTINATION), and every `reflect.SliceHeader` and `reflect.StringHeader` use.

**Controls.** `ctl/pos` plants one of each flagged class, and every one is found. `ctl/neg` plants the false
positives: `__ccgo_ts` as a stored VALUE, as an `Xsprintf` FORMAT, through a call result, and a local of mixed
sources. None is flagged as a write; the mixed local reads as store-may only. The same readings came back on H and
on the i7.

**Readings, on the i7 (COORD, go1.26.11, both module pins, whole outputs on the mailbox).**

| class | sqlite v1.62.0 | libc v1.80.1 |
|---|---|---|
| decl | 5 | 8 |
| `+N` (a constant offset) | 31,782 | 2,126 |
| value (offset 0) | 18 | 544 |
| compare, subtract, other `__ccgo_ts1` uses | 0, 0, 0 | 0, 0, 0 |
| store, memwrite (the verdict, a MUST-derived destination) | 0, 0 | 0, 0 |
| store-may, memwrite-may | 2,123, 254 | 0, 0 |

- **No consumer of an identity difference.** Nothing compares or subtracts `__ccgo_ts`, and nothing names
  `__ccgo_ts1` except its own declaration.
- **The first run over-reached on writes** (store 2,350 and memwrite 625 in sqlite; memwrite 110 in libc). Its taint
  followed any mention, a call result included, so `__ccgo_ts` as a stored value or a format counted as a write
  through it. The instrument now judges the destination (`a638915bf5`), and its negative control holds both shapes.
- **The MAY classes are mixed locals BY CONSTRUCTION.** A site is "may" only when its destination's local has at least
  one `__ccgo_ts`-derived source and at least one other, and ccgo reuses its temporaries across a function. The 80
  printed sites (40 per class), by shape:
  - stores: 39 read-modify-writes of a flag or counter through a reused temporary, and 1 with `__ccgo_ts` as the
    stored value;
  - memory writes, every one into a buffer local: `bufpt` 22, `zOut` 8, `pOut` 4, `zz` 2, `pDest` 2, `pRet` 2.
- **MEASURED: Go itself faults on a write into the literals.** A store through `__ccgo_ts` read "unexpected fault
  address … fatal error: fault [signal 0xc0000005 code=0x1]" under go1.26.11 windows/amd64. A literal's bytes live in
  read-only data. So no MAY site writes into the literals in any run of sqlite that works under Go, printed or not.
- **`reflect.SliceHeader`:** the header-use class is new at this document's tip, and its i7 reading is owed. It goes
  in section 8 when it arrives, and decides D6's BOARD row.

## 3. The two options, and why (b)

- **(a) A permanent pin of the string's backing.** `Data` is the pinned managed address.
  - The identity `uintptr(unsafe.Pointer(unsafe.StringData(s))) == Data` holds, as in Go.
  - But in a cgo process the address is a pin-extent index entry, so the void* door would REFUSE `__ccgo_ts + N`
    handed to C, and Go does not.
  - A stray write through the number lands on an immutable managed string.
  - A pin taken per read leaks a pinned handle for every heap string read through the header, unless it is tied to
    the variable's box and re-pinned on reassignment.
- **(b) A one-time native copy of the backing's bytes. RULED (QW-1).**
  - **Go's cgo check agrees with (b).** `cgoIsGoPointer` (go1.26.11 `runtime/cgocall.go`, lines 790-806) counts
    `inHeapOrStack`, then each module's data and bss, and nothing else, so a literal's read-only bytes are not a Go
    pointer to it. (b)'s number is native memory: it crosses the door, as Go lets it.
  - The census found no consumer of the identity difference (b) makes.

## 4. The design (as ruled)

- **D1. One box, a number flag (QW-2).** `StringHeaderBox` also admits a header of (`uintptr`, `nint`), with a flag
  set in its static constructor. The `Reinterpret` dispatch line is unchanged; only `Applies` widens.
- **D2. Read.**
  - `Data` = the base of a NATIVE COPY of the string's backing `byte[]`, plus the window's offset. `Len` = the
    length. An empty string reads `Data` 0, as the pointer shape reads nil.
  - One copy per backing, made on its first read (`NativeMemory.Alloc` plus a copy), held by a small finalizable
    object. That object is the value of a `ConditionalWeakTable` keyed by the backing, and its finalizer frees the
    block.
  - **So the number is stable:** across reads, across variables that share a backing, and across a substring (base +
    offset).
  - **And it lives exactly as long as the backing:** for ever for a package-level literal, and until collection for a
    heap string, as Go's own data pointer does.
- **D3. Nothing is pinned, and nothing enters the pin-extent index.** The number names native memory.
- **D4. The known differences**, disclosed, each with an arm asserting go2cs's answer:
  - `uintptr(unsafe.Pointer(unsafe.StringData(s))) == Data` reads FALSE (Go: true). The census found no consumer.
  - A write through the number lands on the copy, and the string never sees it. Go faults for a literal, and calls a
    write to a heap string's bytes invalid.
- **D5. Header writes (QW-3).** `hdr.Data = n; hdr.Len = k` passes `n` through the number-to-pointer operator, then
  through the existing commit path, so `StringHeaderBox`'s limits L1-L3 apply unchanged.
  - A native `n` is copied, as `unsafe.String` over native memory is today.
  - An `n` inside a native copy is also native, so it copies too; recovering the original backing is not in scope.
- **D6. Not in scope:** `reflect.SliceHeader`'s `uintptr` `Data`, the same gap on `SliceHeaderBox`. Section 8 holds
  its census; the BOARD row follows it.

## 5. The first-read race and the block's lifetime (for P3's review)

**The race.** Two or more threads read the header of one backing for the first time, at once. Each runs the table's
creation callback, and each callback allocates and fills a block.

- **What the table does** (`ConditionalWeakTable.GetValue(key, callback)`, its documented contract): when several
  threads create the same key, the callback may run several times, and only one of its results is added to the
  table. Every caller is returned the one value the table holds. The internals were not read; the measurement below
  is what this design rests on.
- **The rule that makes the loser unobservable:** the callback publishes NOTHING. It allocates, copies and returns.
  The number is computed ONLY from the value `GetValue` returns, after it returns.
  - So no caller ever forms a number into a loser's block.
  - The loser's holder becomes unreachable when its callback's caller drops it; its finalizer then frees the block.
- **MEASURED on .NET 10** (`data/w8-ccgo-ts/memory-probe`, the `race` arm). 4 threads met at a barrier on each of
  20,000 fresh keys, 3 runs:
  - the race was reached on 19,709 to 19,811 keys per run (a loser was made, up to 3 per key);
  - "two callers got different objects" read 0 on every run.

**The winner's lifetime.**
- The table holds the value through a dependent handle: it is reachable exactly while the key, the backing, is.
- A reader holds the backing for the whole read: it read the `@string` window that names it. So a block cannot be
  freed under the read that forms its number.
- After the read, the number's validity is the backing's lifetime, which is Go's rule for a data pointer kept as a
  `uintptr`: valid while the string is reachable. For a package-level literal that is the program's life.
- The finalizer frees the block only after the GC has found the holder unreachable, which is after the backing is
  unreachable. Nothing resurrects the holder.

**Questions for the review.**
- Is any path left where a number is formed from a block that is not the table's value?
- Can a holder's finalizer run while the backing is still reachable through some route the table does not track
  (for instance an `@string` copied by value into native memory)?
- Is process exit with live blocks acceptable? The process exits, so the blocks are not freed one by one.

## 6. Memory: is a native copy visible to the GC? (COORD's addition 1, QW-6)

A native block is invisible to the GC. But every block is paired with a managed backing of the SAME length, which
must become unreachable before the block can be freed, and whose own allocation already counts against the GC's
budgets. So the native bytes awaiting a free never exceed the managed backing bytes awaiting a collection. Measured
(`memory-probe`, one process per arm, workstation GC; the predictions were written to a file before each run):

| arm | 100,000 distinct 1 KiB backings (97.7 MiB copied) | 200 distinct 8 MiB backings (1,600 MiB copied) |
|---|---|---|
| native, no pressure | peak live 5.0 MiB; 0 after collection; gen2 3 | peak 72 MiB; 0 after; gen2 81 |
| native + `AddMemoryPressure` | peak 4.0 MiB; 0 after; gen2 27 | peak 32 MiB; 0 after; gen2 202 |
| a copy on the pinned object heap | peak 2.9 MiB; 0 after; gen2 36 | peak 40 MiB; 0 after; gen2 94 |

- Every arm is bounded and returns to 0. The stop line written before the large run (a peak over half the total)
  was not hit by any arm.
- Explicit pressure halves the large shape's peak, at 2.5x the full collections there and 9x in the small shape.
- The pinned object heap is GC-visible and needs no finalizer, but every allocation counts against gen2's budget:
  12x the full collections of the unpressured native arm in the small shape.
- **QW-6, my recommendation: native, WITHOUT explicit pressure**, for the reason and with the measurements above. W9
  holds the bound. Explicit pressure is two lines if the ruling prefers it.
- A probe fault, corrected before scoring: the first large run sampled its peak every 1,024 reads (so once, at 200
  reads) and kept the last backing rooted in `Main`'s frame. It was re-run with every read sampled and the loop out of
  `Main`, and the table is from that re-run.

## 7. The arms, the predictions and the stop lines

**Red arms (GolibTests; every one red at base, where the header read panics):**
- **W1.** ccgo's shape: a header over a static string; NUL-terminated reads at `Data`+0, +6 and +12 match the
  literal.
- **W2.** Stability: the same number on a second read; a second variable sharing the backing reads the same number; a
  substring reads base + offset.
- **W3.** The bytes are unchanged across 3 forced full collections under allocation churn.
- **W4.** The identity difference, ASSERTED as go2cs's answer, so its closing is told: `StringData` != `Data`.
- **W5.** An empty string: `Data` 0, `Len` 0.
- **W6.** A heap string's block is FREED after its backing is collected: an internal live-block counter returns to its
  start after `Collect` + `WaitForPendingFinalizers`.
- **W7.** The door: in GolibTests, a cgo process, `VoidPointer` over the number + 2 crosses and is not refused.
- **W8.** A header store with a native `Data` rebuilds the string, as a copy.
- **W9 (COORD's addition 1).** Churn: 100,000 distinct 1 KiB heap strings read through the header. The live-block
  bytes, sampled every 1,024 reads, stay under a quarter of the bytes copied (measured 5%). After forced collections
  they return to the start.
- **W10.** The race: 4 threads read the header of one fresh string at once, 20,000 strings. Every thread reads the
  same number for a string, and the live-block count returns to the start after collection, which shows the losers'
  blocks were freed.

The existing `StringHeaderBox` and `SliceHeaderBox` arms are unchanged, by name.

**Predictions.**
- **PW-1.** W1-W10 red at base, green at the tip; every other GolibTests row unchanged; the total rises by the new
  arms only.
- **PW-2.** Corpus 0 on four targets by construction: golib only, nothing under `src/go2cs` or `src/gen`. CNR is
  byte-identical.
- **PW-3.** The probe program (`data/w8-ccgo-ts/probe-main.go`) under go2cs prints Go's first three lines, then
  "equals StringData: false" where Go prints true: W4's difference, as disclosed.
- **PW-4.** A process that never reads such a header pays nothing: one static bool per header instantiation, decided
  in the static constructor.

**Gates.** Full unfiltered GolibTests, Release and Debug; `go test ./...` with `-count=1`; CNR; repoguard; hosted
golib-tests on 3 OSes; P3's adversarial review of section 5.

**Stop lines.**
- **SW-1.** Any existing `StringHeaderBox` or `SliceHeaderBox` arm changes verdict.
- **SW-2.** The copy needs a pin, or touches the index.
- **SW-3.** A std or behavioral emission changes.
- **SW-4.** W6 or W10 cannot be made deterministic; then I post before asserting the lifetime another way.
- **SW-5.** W9's peak exceeds half the bytes copied.

> **Amended 2026-11-15, 10:15Z (H): two arms from the census, and a LIFETIME GAP the second one exposes.**
> - **W1b.** The vfs pair: a second header-over-literal variable in the same package (`ts`/`ts1` beside
>   `__ccgo_ts`/`__ccgo_ts1`) gets its own copy and its own stable number, and each reads its own bytes.
> - **W1c.** The gethostname shape: a header pointer held in a local, its `.Data` read over a HEAP string, a pointer
>   formed from the number in the same expression, and a copy out through it. The bytes are right, and the block is
>   freed once the string and the pointer are gone (W6's counter).
>
> **THE GAP (D2 as written is not enough for W1c).**
> - In Go, `unsafe.Pointer(sh.Data)` formed while the string is live is a REAL pointer: it keeps the string's bytes
>   alive for as long as it is used. That is the point of Go's `unsafe` rule for the header's `Data` field.
> - In go2cs, `unsafe.Pointer(uintptr)` is `new Pointer(value)` (`unsafe/unsafe.cs`), and the box minted from it keeps
>   nothing alive. `NativeBox` has a retained-source slot, `m_retainedSource`, but nothing fills it on this path.
> - So in `Xgethostname`, once `s` and `sh` are last used, a collection plus the finalizer can free the block before
>   the copy-out reads it: a use-after-free. It is rare, because it needs a collection and a finalizer run inside one
>   statement, but it can happen.
> - Package literals (`__ccgo_ts`, the vfs `ts`) are never freed, so they are not affected. Only a HEAP string read
>   through a header can reach the gap.
>
> **PROPOSED REMEDY R1 (QW-7, for a ruling): a pointer minted from a number inside a live block roots the block's
> holder.**
> - Live blocks are kept in a small range registry. The number-to-pointer mints (`Pointer(uintptr)`, and the box
>   minted from a pointer's number) consult it. On a hit, the holder rides in the minted object: a new slot on
>   `Pointer`, and `NativeBox`'s `m_retainedSource`.
> - The holder then stays alive, and its block stays allocated, while any pointer formed from the number lives,
>   which is Go's rule. The backing's death alone no longer frees the block.
> - **The cost is on a hot path.** libc reads C strings byte by byte through numbers, so the lookup runs on every
>   mint while any block is live. A package literal's block is live for ever, so the registry is never empty in a
>   sqlite process. The lookup is one envelope compare, then a search among live blocks. It is predicted at tens of
>   ns a mint and measured in the cut against an allocation-only base; a bar is to be ruled.
> - `unsafe/unsafe.cs` is hand-owned, so R1 touches a hand-owned file. The registry is golib's.
> - **The alternatives, and why not:**
>   - immortal blocks: unbounded for heap strings read in a loop, and W9 fails;
>   - freeing a block later than the backing's death: no delay is long enough for a pointer that may be used at any
>     time;
>   - pinning a heap string's backing for the header's life: the same rooting question, with a pin.
> - **New arm W11, red under D2 as written:** form `p` from `hdr.Data` while the string is live; drop every reference
>   to the string and the header; force collections and finalizers; read through `p`. The bytes are intact, and the
>   live-block counter has not dropped. Then drop `p`, collect, and the counter returns to its start.
> - **PW-5:** W11 is red with R1 removed and green with it; the mint's added cost is within the bar to be ruled.

## 8. `reflect.SliceHeader` (D6), the census owed

The header-use class is new at this document's tip; its controls read in `readings-h.txt`. Its i7 reading on both
module pins goes here, by dated block, when it arrives. If `reflect.SliceHeader`'s `Data` is read or written in
either module, it is a named next wall; if not, the BOARD row says 0.

> **Amended 2026-11-15, 10:15Z (H): the census of header uses, read on the i7 at `6b42b488db`** (COORD; whole
> outputs on the mailbox; controls: pos finds all 4 SliceHeader plants, neg none, earlier classes unchanged).
> - **`reflect.SliceHeader`: 0 in both modules.** My prediction MET. D6 is a BOARD row whose census reads 0.
> - **`reflect.StringHeader` apart from the `__ccgo_ts` declarations: my prediction ("0") MISSED as worded.**
>   - sqlite: `hdr-String-Data-read` 18. Each is the declaration's shape under another name,
>     `ts = (*reflect.StringHeader)(unsafe.Pointer(&ts1)).Data`, in `vfs/vfs_<goos>_<arch>.go`, including
>     `vfs_linux_amd64.go` and `vfs_windows_amd64.go`. So the demo's own VFS reaches it.
>   - libc: 13 of that shape (the musl files of non-linux targets), plus `hdr-String-type` 1 at `libc_unix.go`:
>     `Xgethostname`. It holds `sh := (*reflect.StringHeader)(unsafe.Pointer(&s))` in a local, over a HEAP string
>     from `os.Hostname()`, then reads `(*RawMem)(unsafe.Pointer(sh.Data))`.
> - **Both are the mechanism (b) covers.** COORD's ruling makes each an arm: W1b and W1c, section 7's block below.

## 9. Rulings (COORD, 2026-11-15)

- **QW-1:** (b), the native copy. The deciding fact is Go's `cgoIsGoPointer` reading (read-only data is not a Go
  pointer), with the census's empty identity column.
- **QW-2:** extend `StringHeaderBox` with the number flag: one box, one commit path.
- **QW-3:** header writes in this cut, through the number-to-pointer operator (D5).
- **QW-4:** this document, on `claude/h-w8-ccgo-ts`, beside the census data.
- **QW-5:** the cut follows the gated pin-extent 308-merge seat, never beside it: one battery on the box.
- **Additions:** memory pressure (section 6), the `reflect.SliceHeader` census class (section 8), and P3's review of
  section 5.
- **QW-6 (ruled 10:10Z):** native WITHOUT explicit pressure, on section 6's measurement: both shapes stay bounded and
  return to 0, and the managed twin drives the GC. W9 holds the bound (under a quarter of the bytes copied), and SW-5
  stops at half. The three-arm table stays in the record, so the choice is visible.
- **QW-7 (asked 10:15Z):** remedy R1 for the lifetime gap that W1c exposes (section 7's amendment).

— H (2026-11-15)

## 10. Amended 2026-11-15, 10:40Z: the design as ruled after P3's read, (L)+(H)

P3 read `6b42b488db`: SHIP-WITH-FIXES, and no number can reach a loser's block (the read is on the mailbox,
`docs/phase4/inbox/COORD/20261115T101108Z-P3.md`). Its finding F1 asked whether a string OWNS its backing.
**It cannot be told.**
- `@string` is three private fields with no ownership mark.
- `AliasOf` shares a caller's mutable array (for `unsafe.String`, `StringHeaderBox`'s own commit, and the
  converter's transient map keys).
- The public `@string(byte[])` constructor and its implicit operator ADOPT the array, uncopied. The converter emits a
  non-UTF-8 literal through that same operator, `(@string)(new byte[]{0x61, 0xff, …})`.
- A side set of aliased arrays would cost every `unsafe.String` call; a mark in the offset field, the hottest type in
  golib.

So COORD ruled Go's own split instead (10:35Z). **This section supersedes D2, D4, section 5's race, section 6's
choice, QW-7 and the arms list where they differ; everything above stays as the record of how the design got here.**

**The split, as Go draws it.** A literal's bytes are read-only data: immutable, immortal, and not a Go pointer to
`cgoIsGoPointer`. Every other string's bytes are Go heap memory: live (they can be shared with a `[]byte`), collected,
and a Go pointer.

- **(L) Package-level storage: an IMMORTAL native copy.** A header read whose SOURCE box is package-level storage
  (`StaticBox<T>`, `IPackageLevelStorage`) gets a native copy of the backing.
  - `StaticBox<T>` is the one emitter of a package variable's box (`symbols.go`'s `PackageBoxConstructPrefix`),
    sealed, never rebound.
  - It covers every census declaration (`__ccgo_ts`, the vfs `ts`, libc's 13), and every literal spelling: valid
    UTF-8, the non-UTF-8 `byte[]` form, `u8`, a concatenation.
  - The copy is never freed and never registered. It is held in a table keyed by the backing, read lock-free first
    (F5). On a miss the block is built and offered to the table; a thread whose offer loses frees its own block at
    once, then reads the winner's. No finalizer is involved.
  - F4: an empty string is answered by its LENGTH, before the table: `Data` 0. A zero-length window of a non-empty
    backing reads base + offset, as Go keeps it.
- **(H) Every other header read: the route `&b[0]` takes today.** `Data` is the number of the backing's element 0 at
  the window's offset, through the element box's pin-only hold, and a pointer re-formed from it resolves back to
  that box.
  - The bytes are LIVE, so a string over a reused buffer reads the new bytes (P3's F1 scenario), with no snapshot to
    go stale.
  - It is Go memory at the cgo door: refused under 3.1, as `&b[0]` is, and as Go counts heap bytes.
  - `uintptr(unsafe.Pointer(unsafe.StringData(s))) == Data` HOLDS here, as in Go. This covers the gethostname shape
    (`libc_unix.go`'s `Xgethostname`, a local over a heap string).

**D4, as ruled: the known differences, now only at package level.**
- For a package-level source, `StringData != Data`. The census found no consumer.
- A write through a package-level `Data` number lands on the copy. Go faults for a literal, which is what ccgo's
  are.
- **R-a, validate-on-read.** A package-level variable whose value is NOT a literal (assigned from `unsafe.String`
  over a live buffer, or reassigned) gets an immortal copy of the backing it held when its header was read.
  - Every header read of a package-level source compares the copy with the live window, and a mismatch is REFUSED BY
    NAME. So a stale copy is never handed out again; a number formed before the write still names the old bytes.
  - Leak: one copy per distinct backing ever read that way.
  - The census line that bounds it: every ccgo declaration is initialised from a literal and never assigned.
- **A header write that sets only `Len`** (P3's point 4) now commits from a native `Data` for a package-level source,
  so the string becomes a copy, and an over-long `Len` reads past the block. Go calls that invalid too. Under (H)
  the managed bound `Len <= len(backing) - i` still applies.

**R-b, the round trip under (H).** A pointer re-formed from `Data` resolves through the number registry to the
element box only while that box is alive. If the box is collected between the `Data` read and the re-forming, the
pin is released and resolution misses.
- This is the window every `&b[0]` number go2cs hands out has today: not new, and not widened by this cut.
- The header box caches its element box, which holds it while the header lives. Whether that closes the window for
  the gethostname shape, where `sh` may be dead once `Data` is read, is MEASURED in the cut: arm W13, a forced
  collection with finalizers between the read and the re-forming.
- If it does not close it, a BOARD line names the `&b[0]` round-trip window in general, with this shape as one
  instance. It is not fixed in this cut.

**P3's findings, each CLOSED, and how:**
- **F1 (a stale snapshot over a shared backing):** under (H) no snapshot exists. Under (L) the snapshot is refused by
  name once the live bytes differ (R-a, W12).
- **F2 (a resurrected key meets a freed base):** no block is ever freed, so no reader can meet a freed base.
- **F3 (`KeepAlive` after the number is formed):** an (L) block is immortal, so the key's liveness cannot free it.
  Under (H) the number's liveness is the element box's, which is R-b.
- **F4 (length first):** adopted, in (L).
- **F5 (`TryGetValue` first):** adopted, as (L)'s lock-free read.

**Withdrawn:** QW-7, remedy R1, its range registry and its bars B1-B3, because there are no mortal copies. Section
6's memory table stays as the record of the question. Mortal copies are gone, so the pressure question no longer
arises.

**The arms, as ruled (GolibTests; every one red at base, where the header read panics):**
- **W1.** ccgo's shape: a package-level literal's NUL-terminated reads at `Data`+0, +6 and +12.
- **W1b.** The vfs pair: a second package-level literal in one package, with its own copy, its own stable number,
  and its own bytes.
- **W1c.** The gethostname shape through (H): live bytes, and a pointer re-formed from `Data` reads them.
- **W2.** Stability: the same number on a second read; a second package-level variable sharing the backing reads the
  same number; a substring reads base + offset.
- **W3.** A package-level copy's bytes are unchanged across 3 forced collections under churn.
- **W4.** Under (L), `StringData != Data`, asserted as go2cs's answer. Under (H), `StringData == Data`, as in Go.
- **W5 (F4).** An empty string reads `Data` 0 and `Len` 0. A zero-length window of a non-empty backing reads base +
  offset.
- **W7.** The door crosses (L)'s number and refuses (H)'s, as Go's `cgoIsGoPointer` would.
- **W8.** A header store with a native `Data` rebuilds the string, as a copy.
- **W9.** Churn: 100,000 distinct heap strings read through (H) stay bounded like any pins; under (L) only
  package-level reads copy.
- **W10.** The (L) table's first read raced by 4 threads, 20,000 strings: every thread reads the same number, and
  every losing block is freed (a live-block counter equals the table's size).
- **W12 (P3's F1 scenario).** Over a reused buffer through (H), the NEW bytes are read. Behind a package-level
  variable, the second header read is refused by name, and the old bytes are never handed out again.
- **W13 (R-b).** The gethostname shape, with a forced collection and finalizers between the `Data` read and the
  re-forming. It reports, either way, whether the header box's cached element box closes the window.

Dropped with the mortal copy: W6 (a freed block), W11 (rooting). W9 and W10 are rewritten as above.

**Predictions.**
- **PW-1.** Every arm above red at base and green at the tip, with W13's result read rather than predicted. Every
  other GolibTests row unchanged; the total rises by the new arms only.
- **PW-2.** Corpus 0 on four targets: golib only; CNR byte-identical.
- **PW-3.** The probe program under go2cs prints Go's first three lines, then "equals StringData: false" (package
  level, W4).
- **PW-4.** A process that never reads such a header pays nothing.
- **PW-5.** The footprint is `ж.StringHeaderBox.cs` plus one new golib file for the (L) table, and NOT `unsafe.cs`
  (R1 is gone) and NOT `ж.cs`.

**Stop lines:** SW-1 (an existing header arm changes), SW-2 (an (L) copy needs a pin or touches the index), SW-3 (an
emission changes), and **SW-6: (H) needs anything beyond the element box's existing pin route.** SW-4 and SW-5 are
retired with W6 and the mortal copies.

— H (2026-11-15)
