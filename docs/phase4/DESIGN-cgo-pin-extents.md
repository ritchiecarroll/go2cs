# DESIGN — a pin-extent index for the cgo void-pointer door (cgo Phase 3, the number residue of 3.1)

> **STATUS: PROPOSED DESIGN, ruled in outline (lane H, 2026-11-14; branch `claude/h-cgo-pin-registry`, cut from
> master `c2919b8b16`).** No code lands from this document, and nothing in it is built. Targets `.5`. It is ordered
> after batch 306 (which lands 3.1) and before 3.2's design step.
>
> **It SUPERSEDES section 17 of [`DESIGN-cgo-phase3-marshaling.md`](DESIGN-cgo-phase3-marshaling.md)**, the golib
> seat proposal written 2026-11-12/13 on `claude/h-cgo-pin-registry-proposal` (`d686a65645`). That branch never
> landed and stays as the record. Section 9 below scores its predictions.
>
> COORD ruled the outline on 2026-11-14 (section 10). The M:N section is co-owned with lane G, whose answer to QG-1
> is folded into section 6. Amended by dated blocks, never rewritten.
>
> **Provenance notes (2026-11-14): checked by hand, not by the guard.** `docs/phase4/` is outside
> `TestDocsProvenanceNotesResolve` by design, because it holds point-in-time records. So a green `TestDocs` says
> nothing about the notes in this file. All 15 were checked at `c2919b8b16` by a scratch script that applies the
> guard's own rule: the cited line, whitespace collapsed, must contain the quote. 15 of 15 resolve. A negative
> control (one quote deliberately altered) was named by the script, and was not caught by `TestDocs`, which is how
> the gap was found.

## 1. What is open

3.1's door hands C a number only when it is nil or C memory. It tells Go memory from C memory, for a bare number, by
an EXACT lookup in the pin and token registry. <!-- src/core/golib/runtime/GoCgoArgs.cs:285 (`private static bool IsKnownGoAddress(nuint address) =>`) -->
Two classes of number get past that lookup today, and two GolibTests arms hold them open as disclosed:

- **Four exact numbers whose pin was never registered.** A wider view pinned over an element, and a native box over
  a pinned managed buffer, each handed over through the bare mint and as its number held in a variable.
  <!-- src/tests/GolibTests/GoCgoArgsTests.Phase3.cs:156 (`public void APointerOverManagedStorageCrossesAVoidPointerOnlyAsTheFourDisclosedNumbers()`) -->
- **Every interior number.** A registered address plus an offset is not a key, for every kind of pin.
  <!-- src/tests/GolibTests/GoCgoArgsTests.Phase3.cs:254 (`public void ANumberBuiltByArithmeticOverAGoPointerCrossesAsDisclosed()`) -->

A box that is handed over carries its own answer, and is refused by it. That includes a box derived from another
pointer, which since 3.1 retains its root (P5's retained-root rule, `NativeDerivation`). So the residue is a BARE
NUMBER only.

## 2. What was measured before designing

A scratch GolibTests probe, never committed, Release, at `c2919b8b16` on H. A figure written `a / b` is two readings
at the same tree, in run order. The probe's source and outputs are kept in H's scratch until this seat banks.

### 2.1 How the exact registry misses the four (M1)

| Box | Its number | Registered? | Its pin | Number's place in the pin |
|---|---|---|---|---|
| a wider view pinned over an element | the element's address | no (table delta 0); `Resolve` MISSES | a pin-only hold (`Length` 0) over the WHOLE 16-byte backing array | base +4 |
| a native box over a pinned managed buffer | the buffer's address | no (table delta 0); `Resolve` MISSES | a `PinnedBuffer` of 32 bytes | base +0 |

- Both boxes are native boxes that HOLD a pin. <!-- src/core/golib/ж.NativeBox.cs:219 (`internal bool PinsManagedStorage => m_pin is not null;`) -->
  Their number leaves through the exit's native arm, which hands out the address and registers nothing.
  <!-- src/core/golib/ж.cs:1233 (`if (value is not null && value.NativeAddress != 0)`) -->
- The pinned reinterpret pins the backing array, not the element. <!-- src/core/golib/ж.ElemRefBox.cs:450 (`PinnedBuffer? pin = PinnedBuffer.PinOnly(backing);`) -->
- For both, the pin's base and its last byte cross too. Every byte of both pins is open, not only the number handed out.
- **Section 17.2's open claim, settled.** Registering the native box's own number from outside (the public
  `RegisterPinned(number, box)`) makes the exact number refused. It validates through the order-token arm, with no new
  arm: a native box's token is its address. <!-- src/core/golib/ж.PointerTokens.cs:683 (`if (CurrentToken(box) == token)`) -->
  But number+1 still crosses. Registering boxes closes the four exact numbers and nothing else.
- **The interior gap on ordinary pins.** A value slot's number +4 and +8 cross. A fixed array's data +4 and +28
  cross. The exact numbers are refused. One past the end (+32) crosses, which is correct.

### 2.2 What the door costs today (M2)

| | Time | Managed bytes |
|---|---|---|
| an exact `Resolve` that misses | 30.9 / 30.1 ns | 0 |
| the whole door on a C number, the `unsafe.Pointer` mint included | 102.4 / 76.8 ns | 96 B |

### 2.3 Three index shapes, measured

**X-SCAN, section 17.3's walk of the live registry on an exact miss (M3).**

| Live entries | 16 | 500 | 5000 |
|---|---|---|---|
| per walk | 2.10 / 1.23 µs | 31.4 / 31.7 µs | 160 / 147 µs |
| managed bytes per walk | 64 B (the enumerator) | 64 B | 64 B |

Every legitimate C crossing misses the exact lookup, because C memory is never registered. So the walk would land on
EVERY C call: 12x to 2000x the whole door. **Rejected.**

**X-COW, one sorted copy-on-write array of extents (M4, one reading).**

| Live extents | 16 | 500 | 5000 |
|---|---|---|---|
| lookup | 5.5 ns, 0 B | 18.4 ns, 0 B | 36.0 ns, 0 B |
| pin + unpin | 575 ns, 624 B | 9.1 µs, 16 KB | 78 µs, 160 KB |

The lookup is fine. The pin path grows with the population. **Rejected.**

**X-PAGE, a page-keyed index (MX).** The bar, written before this arm was run:
- a pin path passes if pin plus unpin is flat in population (at most 2x from 16 to 5000) and at most 1 µs and 400 B;
- a lookup passes at no more than 2x today's exact miss, so at most 60 ns, flat.

By that bar M4 fails at every population, its 16 row on bytes.

Two shapes were measured. **COW** keeps, per 4 KiB page, an immutable array of extents, replaced on each pin or unpin.
**LIST** keeps, per page, a lock-free pushed list: unpin marks an entry dead, and a writer-side compaction runs under a
per-page lock. The reader never locks.

Each shape was fed two layouts:
- **SPARSE:** M4's synthetic layout, about one extent per page.
- **DENSE:** the real addresses of N live pinned 16-byte managed arrays, which the heap packs about 60 to 100 to a
  page. This is the layout a cgo program that pins in a loop produces.

Every entry holds one short weak `GCHandle` to its owner (section 3, D3). Three readings, plus one with the
populations reversed (R):

| LIST | 16 | 500 | 5000 |
|---|---|---|---|
| SPARSE lookup, a hit | 50.7 / 36.9 / 38.8, R 12.7 ns | 27.8 / 18.6 / 19.6, R 21.6 ns | 64.8 / 51.3 / 46.6, R 76.2 ns |
| DENSE lookup, a hit | 25.8 / 30.5 / 29.7, R 21.7 ns | **147 / 122 / 127, R 123 ns** | **183 / 158 / 151, R 162 ns** |
| lookup, a C number (a miss) | 4.0 to 9.4 ns in every steady row | | |
| pin + unpin, an occupied page (both layouts) | 90 to 189 ns, 56 B | 91 to 116 ns, 56 B | 102 to 194 ns, 56 B |
| pin + unpin, a fresh page (both layouts) | 140 to 262 ns, 144 B | 145 to 185 ns, 144 B | 146 to 259 ns, 144 B |
| a 64 KiB pin (16 or 17 pages) | 1.9 to 3.0 µs, 2608 B | | |

| COW | 16 | 500 | 5000 |
|---|---|---|---|
| SPARSE pin + unpin | 177 to 589 ns, 204 B | 158 to 200 ns, 202 B | 212 to 506 ns, 208 B |
| DENSE pin + unpin | 233 to 283 ns, **820 to 864 B** | 611 to 929 ns, **3.8 to 4.6 KB** | 672 to 971 ns, **4.8 to 5.0 KB** |
| DENSE lookup, a hit | 18 to 22 ns | 49 to 80 ns | 53 to 107 ns |

The weak handle alone costs 26.2 ns per allocate-and-free, and 0 managed bytes. A `GCHandle` lives in the runtime's
handle table, so the managed byte counter cannot see it. Its slot is NOT MEASURED, not zero.

**The verdict.** LIST's pin path PASSES on both layouts. COW's FAILS DENSE on bytes. No shape passes both bars
everywhere:
- LIST's lookup misses 60 ns on DENSE from 500 up;
- it straddles 60 ns on SPARSE at 5000 (46.6 to 76.2 ns);
- COW's DENSE lookup straddles it too.

By COORD's condition, which is the pin path, the index is **X-PAGE-LIST**. The DENSE lookup cost is DISCLOSED, not
passed. Section 5 says who pays it.

**The order confound, measured.** In the forward order the 16-pin rows ran first and read high and noisy. A
reversed-order arm, predicted before it ran, moved the high readings to whichever population ran first. Run last, the
16 rows fell: SPARSE COW hit 8.4 ns, its pin 177 ns, SPARSE LIST hit 12.7 ns. So the first-run excess is warm-up, not
population. This is also why the first lookup row of every run reads about 22 ns on a C number where the steady rows
read 4 to 9.

### 2.4 Go's own answer

Go asks the same question with a span lookup: `cgoIsGoPointer` answers through `inHeapOrStack`, which finds the
address's span by its arena index (go1.26.11 `runtime/cgocall.go`, lines 790-797). A page-keyed index is that
mechanism, over the managed storage golib pins.

## 3. The design

**D1. Register PINS, not boxes.** Every `PinnedBuffer` that holds Go storage still enters the index when it pins, and
leaves it when it releases (`Dispose`, or its finalizer). <!-- src/core/golib/runtime/PinnedBuffer.cs:43 (`~PinnedBuffer()`) -->
- **The extent** is the pinned object's data: its base, which is the pin's address, <!-- src/core/golib/runtime/PinnedBuffer.cs:96 (`public unsafe byte* Pointer => (byte*)m_handle.AddrOfPinnedObject();`) -->
  plus the array's length times its element size.
- **The element size** is taken at the generic site that pins:
  - the value slot's pin-only hold, <!-- src/core/golib/ж.cs:716 (`m_pin = PinnedBuffer.PinOnly(storage);`) -->
  - a fixed array's data, <!-- src/core/golib/ж.cs:785 (`m_pin ??= new PinnedBuffer(arr.Source, arr.Length);`) -->
  - and the pinned reinterpret, which pins the whole backing array (section 2.1).
  - A byte buffer constructed directly is byte-sized, which covers the hand-owned callers (`unsafe`, and two of
    windows' syscall files).
- **What it covers:** the four exact numbers and the interior of every kind of pin, in one mechanism. A pin-only hold
  answers its OBJECT's extent, not its own zero length.
- **What it leaves alone:** the exact registry (`s_table`), `Resolve` and the number-to-pointer operator. So section
  17's PA-3 risk does not exist here: no conversion answers differently.

**D2. The door.** `IsKnownGoAddress` gains one step: on an exact miss, ask the index. Nothing else in the door
changes.
- **A native box minted from a bare number** is asked with P5's `NativeRootAddress`, the native root's own address,
  through the one predicate it already calls. <!-- src/core/golib/runtime/GoCgoArgs.cs:297 (`!native.PinsManagedStorage && !IsKnownGoAddress(native.NativeAddress)`) -->
  <!-- src/core/golib/ж.NativeBox.cs:198 (`nuint INativeRooted.NativeRootAddress => NativeAddress;`) -->
- **A derived box** keeps P5's retained-root refusal. The index is not asked about a box that carries its answer, so
  P5's machinery is reused, not duplicated.
- **The index answers end-exclusive.**

**D3. The index: X-PAGE-LIST.**
- **Shape.** A concurrent dictionary from a 4 KiB page number to that page's list of extents.
  - Each extent is ONE entry object shared by every page it covers. It holds the extent's base, its end, a dead
    flag, and one **short weak `GCHandle` to its `PinnedBuffer`**.
  - The reader takes no lock. A writer locks only the page it changes.
- **The weak handle is a requirement, not an optimization (G's QG-1 answer, section 6).**
  - The index must not root a buffer, because the buffer leaves the index from its own finalizer.
  - A short weak handle's target is cleared BEFORE the finalizer runs. So a reader can never take a buffer whose
    finalizer might unpin it while a native call is using it. A raw address range with no handle could.
- **The door's rule.** It resolves the entry's handle to a STRONG reference, and keeps that reference alive
  (`GC.KeepAlive`) from before `GoNativeCall.Enter` to after `Exit`.
  - Under 3.1 the door only REFUSES. There, a hit decides a refusal, and an entry found dead reads as a released pin
    (row 7).
  - The strong reference becomes load-bearing at 3.2, the first time a pinned Go buffer is allowed to cross. R8 is
    its arm.
- **The named lever, not measured.** A finer cell than a 4 KiB page cuts the DENSE in-page scan, and makes a large
  pin write more cells. It carries its own prediction at seat time. It is not a decision here.

**D4. Who keeps the index: G-CGO, with backfill.**
- **Switch-on.** The index exists only in a process that links a cgo package. It is switched on in
  `GoCgoLibraries.Register`, the module initializer every `-cgo` package already emits, so no converter change is
  needed. <!-- src/go2cs/cgoLibraryRegistration.go:86 (`[global::System.Runtime.CompilerServices.ModuleInitializer]`) -->
- **No banked row pays.** A `-cgo` conversion is one package and is not combinable with `-stdlib`, `-tests` or
  `-recurse`, so no banked row is a cgo process. <!-- src/go2cs/main.go:305 (`Not combinable with -stdlib, -tests or -recurse`) -->
  0 bytes land on any banked row by construction, so section 17.6's allocation-meter census is not needed.
- **Backfill.** When the index switches on, it backfills from the exact registry's live pinned registrants.
- **THE EARLY-PIN RESIDUAL, disclosed as row 11 of section 4.** A pin made before the first cgo module loads, which is
  not in the exact registry, is not in the index. That is a pinned reinterpret or a hand-owned buffer whose number
  was never taken through the exit. If its number is handed to C later, it crosses. R5 is its arm. The alternative
  (G-ALL: the index always on) closes the residual, but charges every pin on every row. It would need the census
  first.

## 4. What it closes

One row per source of a number over Go memory. Rows 1-9 are section 17.4's and 17.11's, re-scored.

| # | Source of the number | Exact | Interior | After this seat |
|---|---|---|---|---|
| 1 | a pinned value slot, through the exit | refused today | crosses today | **both refused** (D1, D2) |
| 2 | a fixed array's pinned data, through the exit | refused today | crosses today | **both refused** |
| 3 | a wider view pinned over an element | crosses today (2 of the four) | crosses today | **both refused**, over the whole backing array |
| 4 | a native box over a pinned managed buffer | crosses today (2 of the four) | crosses today | **both refused** |
| 5 | a pointee that holds references (an order token) | refused by the tag | refused by the tag | unchanged |
| 6 | a pinned buffer's address read directly by hand-owned code | crosses today | crosses today | **both refused** (the pin is registered at construction); not reachable from a cgo package |
| 7 | a number whose pin was RELEASED | crosses | crosses | NOT closed: the entry has left the index by design. 3.2's first item |
| 8 | a number minted from a reference with no pin (`FromRef`) | crosses | crosses | NOT closed: no pin, so no extent. The converter's (the BOARD row routed to P2) |
| 9 | the result of a call whose result type is `unsafe.Pointer` | | | closed before this seat, by its own seat (section 17.11) |
| 10 | reserved by section 17.11 for a source the narrow re-review finds | | | |
| 11 | **an early pin under G-CGO**: made before the first cgo module loads, and not in the exact registry | crosses | crosses | **NOT closed, disclosed** (D4). R5 |

After this seat the disclosed gap is rows 7, 8 and 11. `docs/Limitations.md` and the release notes are reworded in
the same seat to say exactly that.

## 5. Who pays, and when

| Who | When | Cost (H, section 2) |
|---|---|---|
| every legitimate C call through the door | an exact miss, then an index miss (no page) | the exact miss as today, plus 4 to 9 ns steady |
| a cgo program, at each pin and its release | D1's insert and remove | 90 to 262 ns and 56 to 144 managed B per one-page pin, plus a weak handle (26.2 ns, its slot not measured); a 64 KiB pin, 1.9 to 3.0 µs and 2608 B |
| **a Go address that reaches the door: a HIT** | the in-page scan | **up to 122 to 183 ns on DENSE pages**, 13 to 76 ns on SPARSE |
| every process that links no cgo package | never | 0: the index is never created |

**The DENSE hit figure is not every C call's cost.** A hit means a Go address reached the door. Under 3.1 that is
the REFUSAL path, an error, so no successful call pays it. From 3.2, a permitted crossing of a pinned Go buffer pays
it once per call. Every legitimate C call pays the miss: 4 to 9 ns in steady rows, 4 to 23 ns counting first-run rows.

## 6. How it composes with M:N's native-call seam (with lane G)

[`DESIGN-native-call-seam.md`](DESIGN-native-call-seam.md) is co-owned with G, and its M1-M8 are the assumptions read
here.

- **The door runs before `GoNativeCall.Enter`, while the goroutine holds its P.** The index read takes no lock and
  never blocks, so it cannot stall a P (SB-2).
- **Writes happen at pin moments, which are P-holding Go code, with one exception: removal on the finalizer thread.**
  G's answer to QG-1, read at `c2919b8b16`:
  - **What runs there.** The CLR finalizer thread runs exactly six golib or runtime finalizers: `PinnedBuffer` (this
    removal); `GoFinalizerSentinel` and `GoCleanupSentinel`, which only enqueue to `GoFinalizerQueue`, whose runner
    is a separate background thread; `BoringCaches` and `GcPauseRecorder` (bookkeeping); and `memProfileSentinel`,
    converted profiler code under a plain C# lock.
  - **No re-entry.** None of them disposes a `PinnedBuffer` or reads the index, and the CLR runs finalizers one at a
    time. So a removal there is never re-entered. It only races with `Dispose` removals on other threads.
  - **The rule.** The writer takes a .NET lock or a CAS, NEVER a Go runtime lock: a Go runtime lock is the one thing
    that could ask for a P or `getg()`. SB-4 holds.
  - **Under M:N nothing changes on that thread.** Carrier injection targets goroutine threads only, and the R2
    twins exist only for converted Go functions under a goroutine. golib's finalizers are never twinned.
  - **`memProfileSentinel` is named here so that M:N never gives it a Go lock.**
- **The reader hazard is the one G named, and D3 answers it.** The index cannot root a buffer that leaves on its
  finalizer. So the entry holds a short weak handle, and the door holds a strong reference from before `Enter` to
  after `Exit`. Under M1 and M2 the frame that holds it cannot migrate during the native call.
- **3.2's pin scope (`#cgo noescape`) is the index's write side.** A pin that spans `Enter..Exit` stays in the index
  for exactly that span.
- **Callbacks (M7) are unaffected.** They convert numbers back through the number-to-pointer operator, which this
  seat does not touch. So the foreign-thread gap section 6.3 of the seam design records does not widen.

## 7. The red arms that prove it (to be written red-first in the seat)

- **R1.** The four-disclosed arm is rewritten to assert 0 crossings. It is red at the base: 4 cross.
- **R2.** The arithmetic arm is rewritten to assert refusal for every pin kind: value slot +4; array data +28; the
  wider view's pin base and base+k; buffer +31. Each beside its exact number. Red at the base.
- **R3.** C-memory controls cross: a `malloc` block's exact number and an interior number, and one past a Go
  extent's end. SB-1 guards this arm.
- **R4.** After a pin is released (`Dispose`), the index no longer answers for its extent. C memory placed at that
  address later crosses: there is no stale refusal.
- **R5.** Activation: a pin made before G-CGO switches on, and in the exact registry, is refused after it, through
  the backfill. A pinned reinterpret made before activation crosses: row 11, asserted as disclosed so that its
  closing is told.
- **R6.** A process that links no cgo package never creates the index: 0 managed bytes around a pin loop, read with
  the census off. SB-3 guards this arm.
- **R7.** Pin and unpin on N threads while the door asks, on live pins only: no live pin's number crosses, and no C
  number is refused.
- **R8 (G's hazard; it bites at 3.2).** A pinned buffer whose last user reference is dropped while a native call
  that received its address is still running. The door's strong reference keeps the buffer, and so its pin, until
  `Exit`: the bytes are unchanged after a forced collection inside the call. Under 3.1 the arm asserts the
  refusal, and that the index entry still resolves after the collection.

## 8. The seat it proposes

**Predictions, to be scored as worded at seat time.**

- **PB-1.** R1 and R2 are red at the base and green at the tip. R3-R8 are green at the tip.
- **PB-2.** GolibTests' total rises by the new arms only. The two rewritten arms change assertion and keep their
  names. No other row moves, and the registry's growth guard and the reinterpret-source retention rows are included
  in that.
- **PB-3.** 0 corpus bytes on four targets (golib only, no converter change). `check-no-regression` is
  byte-identical, and no behavioral golden moves.
- **PB-4.** The `os` row and `crypto/internal/nistec`'s wall time are inside their recorded spread. They are read
  twice at each tree, alone on the box. No banked allocation assert moves.
- **PB-5.** The door on a C number costs no more than base + 15 ns: one page-dictionary miss.
- **PB-6.** Every arm of both end-to-end cgo programs reads as at the base, on linux, windows and darwin (both
  architectures).
- **Most likely red that is not the mechanism:** R7's timing under a loaded box, or R8's forced collection on a
  host where the pin outlives the reference for a reason unrelated to the door.

**Gates.**
- The full, unfiltered GolibTests.
- The reflect bridge's five largest banked consumers by verdict count, derived at gate time, plus the cost canary,
  `crypto/tls` and `os`.
- The full `go test ./...` and `check-no-regression`.
- The full behavioral suite once at the tip.
- The two-seeded corpus comparison on four targets, predicted 0.
- Hosted `cgo-e2e` on every OS.
- An adversarial review at the tip. This is collector-safety work.

**Stop lines (as ruled).**
- **SB-1.** Any C-memory control is refused.
- **SB-2.** The door's read needs a lock.
- **SB-3.** Under G-CGO, a process that links no cgo package allocates for the index.
- **SB-4.** The finalizer's removal needs a P or goroutine identity. QG-1 answers that it does not; the line stays,
  as the guard on that answer.
- **SB-5.** The exact registry, `Resolve` or the number-to-pointer operator must change.

## 9. Section 17's predictions, scored

Section 17 was never built, so most of its predictions were never run. Those that this measurement reached:

- **17.2's open claim** (a pin-holding native box's entry validates with no new arm): **CONFIRMED** (section 2.1).
- **17.7 PA-7** (the walk under 0.5 µs at 16 live entries, about 10 µs at 500): **MISSED**, by 2.5x to 4.2x and
  3.1x (section 2.3). It was a derived figure stated so that a measurement could miss it, and one did. The walk is
  rejected on a reason PA-7 never addressed: it falls on every C call, not only on Go addresses.
- **17.5's cost table:** **SUPERSEDED.** Every figure in it was derived, and section 2 replaces them with
  measurements.
- **17.3 QA-1 to QA-3** (1-EXIT, 2-BOX, 3-SCAN): superseded by QB-1 to QB-3 below. 1-EXIT closes the four exact
  numbers and nothing interior (section 2.1). 3-SCAN is rejected by M3.
- **PA-1 to PA-6 and PA-8:** not run. Their successors are PB-1 to PB-6.

## 10. Decisions, as ruled by COORD (2026-11-14)

- **QB-1. Register pins** (D1), not boxes at the exit: ruled.
- **QB-2. X-PAGE**: ruled, on condition that it was measured. It was (section 2.3), and it stands as X-PAGE-LIST by
  the pin-path condition. The DENSE lookup is disclosed.
- **QB-3. G-CGO with backfill**: ruled. The early-pin residual is its own row (row 11), with R5 as its arm.
- **QB-4. This document**, superseding section 17 and scoring its predictions: ruled. The ruling includes a pointer
  line in the marshaling design's owed list. That list is in 3.1's record, which lands with batch 306, so the
  pointer line is owed as a commit on this branch after 306 lands.
- **QB-5. Order**: after 306, before 3.2's design.
- **QG-1** (G): answered (section 6). SB-4 holds.

— H (2026-11-14)
