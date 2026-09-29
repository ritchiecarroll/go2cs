# DESIGN — OQ-4: one CLR heap snapshot feeds Go's heap accounting

**Design only. Nothing here is cut; the increments are proposals with bars, and §7 lists the
questions a ruling must settle first.** Dated 2026-09-28 (C1); amended by dated blocks.

**Ruling basis.** Ledger 96edeb700a (2026-09-28): OQ-4's own trigger, "defer until a consumer
demands it" (`DESIGN-readmemstats-surface.md` ⟨OQ-4⟩, ratified 2026-08-21), HAS FIRED, so the
wiring is **owed**. `/gc/heap/live` is a real-world surface for metrics exporters. The work is a
design seat with a host-fatal risk (§3), scheduled after the objective banks, or sooner if it is
ready and green.

**What it retires.** runtime's `TestReadMetrics` "live bytes is 0", disclosed DEFERRED with this
record as its plan (`src/core/runtime/go2cs_test_disclosures.json`, 67efca7ef5). **What it does
not retire.** `TestReadMetricsConsistency`'s two halves: the mark CPU classes stay runtime-capability
under the 2026-09-27 00:35 ruling, and the scan bytes stay structural (§4.6).

## 1. The incoherence, measured

The managed host answers "how big is the heap" twice, from two stores that do not know about each
other:

- **`runtime.ReadMemStats`** is hand-owned (`managed_impl.cs`). It reads the CLR through golib's
  `GcPauseRecorder` (committed and heap-size samples taken once per gen2) and two live reads,
  `GC.GetTotalMemory(false)` and `GC.GetTotalAllocatedBytes(false)`.
- **Everything else** reads Go's own structures, which nothing writes:
  - runtime/metrics' compute closures (auto-converted);
  - Go's own `readmemstats_m`, reached through `ReadMemStatsSlow` and `ReadMetricsSlow`;
  - the pacer (`heapGoal`, `trigger`).
  The structures are `gcController.{heapInUse, heapFree, heapReleased, heapMarked, heapLive,
  totalAlloc, totalFree, mappedReady}`, `memstats.heapStats` (the consistent deltas),
  `memstats.numgc`, and the `*_sys` counters. **All zero.**

The hand-owned `consistentHeapStats.read` returns the zero delta ("nothing ever writes a
heapStatsDelta"). Go's `unsafeRead`, which `readmemstats_m` uses, is converted and merges
`m.stats[0..2]`. Those are zero too.

| Surface | Reads | Value today |
|:--|:--|:--|
| `MemStats.HeapAlloc` / `HeapSys` / `HeapReleased` | the CLR | real |
| `/gc/heap/live:bytes` | `gcController.heapMarked` | 0 |
| `/memory/classes/heap/{objects,unused,free,released}` | `heapStats` deltas | 0 |
| `/gc/heap/allocs:bytes`, `/gc/heap/frees:bytes` | `heapStats` deltas | 0 |
| `/gc/cycles/total:gc-cycles` | `memstats.numgc` | 0 (`MemStats.NumGC` is real) |
| `/gc/heap/goal:bytes` | `gcController.heapGoal()` | the heap minimum over a zero `heapMarked` since M1 5dd6bce97b, derived from `commit`'s arithmetic, not measured (`MemStats.NextGC` is the CLR heap size) |

An exporter scraping runtime/metrics therefore reports an empty heap. The consumers that FIRED the
trigger:
- runtime's `TestReadMetrics` (`0 < live <= HeapSys`);
- every metrics exporter.

## 2. Why the line is not separable

`TestReadMetrics` bounds `live <= mstats.HeapSys`. `ReadMetricsSlow`'s `mstats` comes from
`readmemstats_m`, where `HeapSys = heapInUse + heapFree + heapReleased`, also 0. Writing
`heapMarked` alone turns "live bytes is 0" into "live bytes: N > heap sys: 0". Feeding `HeapSys`
means feeding the heap accounting, and §3 is why that must be done coherently or not at all.

## 3. The invariants, and why a miss is host-fatal

runtime's test binary enables `doubleCheckReadMemStats` for the WHOLE binary, in `gc_test.go`'s
`init`. `readmemstats_m` then THROWS, inside the stopped world, unless all of these hold at the
instant it reads:

1. `gcController.heapInUse == consStats.inHeap`
2. `gcController.heapReleased == consStats.released`
3. `heapInUse + heapFree == consStats.committed - inStacks - inWorkBufs - inPtrScalarBits`
4. `gcController.totalAlloc == Σ allocs` (largeAlloc + smallAllocCount × class size)
5. `gcController.totalFree == Σ frees`
6. `gcController.mappedReady == totalMapped - consStats.released`

A throw is host-fatal: `TestReadMemStats`, `TestReadMetrics` and every later test in the runtime
row is lost. So the design's first property is **equality by construction**. Every fed field is
DERIVED from one snapshot through the same arithmetic Go uses, never measured twice.

The second property is **no torn read**. The stop-the-world contract (`manual-conversions.md`) keeps
worldsema's exclusion but does NOT suspend other goroutines. A metrics reader feeding the structures
while `readmemstats_m` reads them under a stopped world would tear equalities 1–6 as surely as a
wrong mapping.

## 4. The design

### 4.1 Go's own structures are the store

The feed writes `memstats.heapStats.stats[0]` (absolute values; generations 1 and 2 stay zero), the
`gcController` fields of §3, and `memstats.numgc` / `numforcedgc` (§5.1).

The hand-owned `consistentHeapStats.read` becomes `unsafeRead`'s merge, so the metrics path and
`readmemstats_m` read the SAME fields. Everything downstream stays auto-converted and derives by
Go's own arithmetic:
- the compute closures;
- `heapStatsAggregate` and `sysStatsAggregate`;
- `readmemstats_m`;
- the pacer.

### 4.2 The snapshot, and what feeds each field

One snapshot per read, allocation-free. Most of it is golib's recorder sample from the last
observed gen2, extended in I1:
- committed `C`;
- heap size `H`;
- fragmentation `F`;
- the live bytes that gen2 left, `L₂`;
- released `R` (the recorder's `HeapReleased`, which `ReadMemStats` already reports).

Two figures are read live at the feed, and neither allocates:
- total allocated `A`, from `GC.GetTotalAllocatedBytes(false)`;
- current live `L`, from `GC.GetTotalMemory(false)`.

| Go field | Fed from | Freshness |
|:--|:--|:--|
| `consStats.largeAlloc`, `gcController.totalAlloc` | `A` | live (Go's is live too) |
| `consStats.largeFree`, `gcController.totalFree` | `A − L` | live |
| `consStats.inHeap`, `gcController.heapInUse` | `max(H, L)` | last gen2, raised to live |
| `consStats.committed` | `max(C, inHeap)`; released memory is NOT in it (invariant 3 sums in-use and free only) | last gen2 |
| `consStats.released`, `gcController.heapReleased` | `R` | last gen2 |
| `gcController.heapFree` | `committed − inHeap` | derived |
| `gcController.mappedReady` | `totalMapped − R` | derived (invariant 6) |
| `gcController.heapMarked` | `L₂` | last gen2 (Go's is last GC too) |
| `gcController.heapLive` | `L` | live |
| all allocation and free COUNTS | 0 | honest zero (§4.6) |

`inObjects = A − (A − L) = L`, so `/memory/classes/heap/objects` is the live heap and `unused =
inHeap − L` is fragmentation. That agrees with `MemStats.HeapAlloc = L` by construction. The `max`
clamps keep every derived byte count non-negative when the heap has grown since the last gen2. That
is the one place the mapping chooses rather than measures, and Q3 asks for it to be ruled.

### 4.3 Derivation order

The feed computes `inHeap`, `committed` and `released` first, then derives `heapInUse = inHeap`,
`heapFree = committed − inHeap`, `mappedReady = heapInUse + heapFree` (the `*_sys` terms are 0),
`totalAlloc` and `totalFree` from the SAME `A` and `L` it writes into the deltas. Equalities 1–6
then hold because each side is literally the same number. The arm in §6 checks all six on the fed
state WITHOUT the throw, so a regression reads red rather than fatal.

### 4.4 The writer holds worldsema

Every feed runs holding worldsema. `readmemstats_m`'s callers already hold it, because they stop
the world. `readMetricsManaged` (runtime/metrics.Read) takes it with `semacquire`, recording no
pause sample, since Go's `metrics.Read` stops no world. It takes it BEFORE `metricsLock`, the order
`ReadMetricsSlow` uses (stopTheWorld, then metricsLock), so the two cannot invert, and holds it
through the read. That rules out a torn read with no new lock, and the cost is one semaphore per
`metrics.Read`, contended only by stop-the-world callers.

### 4.5 Allocation-free

`ReadMemStats` is held to 0 B per call (`GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation`,
because net/textproto's banked `TestReadMIMEHeaderAllocations` brackets it). The feed copies
`ulong`s out of the recorder's last sample and makes two non-allocating CLR reads. Nothing new is
boxed; the pause-histogram precedent (`StwPauses`' cached word boxes) applies if an atomic field
needs a box.

### 4.6 What stays zero, by rule

A field is answered only when a managed measurement means the same thing
(`DESIGN-readmemstats-surface.md`'s rule). The following stay 0 and are named, not invented:
- **allocation and free COUNTS** (so `/gc/heap/allocs-by-size` stays empty and `HeapObjects` 0);
- **scan bytes** (the CLR does not split scannable from pointer-free bytes: A5's structural half);
- **the `*_sys` breakdown** (stacks, spans, mcache, buckhash, gc misc, other);
- **the mark CPU classes** (A5's runtime-capability half).

## 5. Couplings the wiring arms

### 5.1 Cycles and pauses

Feeding `memstats.numgc` (the recorder's gen2 count) makes `/gc/cycles/total` agree with
`MemStats.NumGC`. It also ARMS `TestReadMetricsConsistency`'s check that `/gc/pauses:seconds`
holds at least `2 × numGC` samples. That check passes vacuously today, at 0 cycles. Under the
stop-the-world contract only `runtime.GC` records a GC-class pause, one per call; automatic gen2s
record none. Two ways through:

- **P-a (recommended):** the feed records each newly observed gen2's CLR pauses from the recorder's
  ring into `stwTotalTimeGC`, and `runtime.GC` records Go's TWO stopped phases (a `stwGCSweepTerm`
  pair before its `stwGCMarkTerm` pair).
  - A background gen2 has two CLR pauses and meets `2 × numGC` itself.
  - A forced cycle carries runtime.GC's two pairs; its one blocking CLR pause must then NOT be
    recorded as well, or it is double-counted. So the feed skips a gen2 the recorder marks forced.
  - Only an AUTOMATIC blocking gen2 (one pause) can still fall short. That is rare in a test and
    named, not padded.
- **P-b:** leave `numgc` at 0. `/gc/cycles/total` then stays incoherent with `MemStats.NumGC`: the
  OQ-4 divergence, kept for cycles.

### 5.2 The heap goal

`/gc/heap/goal` is `heapGoal()` over the fed `heapMarked` and GOGC (since M1). `MemStats.NextGC` is
the CLR's heap size. Q5 asks whether `ReadMemStats` should report `heapGoal()`, one answer to one
question, or keep the CLR figure, which is what the CLR will actually collect at.

## 6. Increments, each red first

- **I1: the recorder's sample.** `GcPauseRecorder` records `F`, `L₂` (`HeapSizeBytes −
  FragmentedBytes` of the gen2's `GCMemoryInfo`) and the forced flag per gen2, beside `C` and `H`.
  - Red: a GolibTests arm reads a zero `L₂` after `runtime.GC()`.
  - Gate: the ReadMemStats allocation guard stays 0.
- **I2: the feed and the reader.** Covers §4.1–§4.4.
  - Red: runtime's `TestReadMetrics` "live bytes is 0".
  - Also red: a probe arm computing equalities 1–6 on the fed state, with no throw, reading
    unequal.
  - Retires the deferred entry.
  - Gates: the runtime row (`TestReadMemStats` is the canary: it must stay PASS with the double
    check on; `TestReadMetrics` must go PASS).
  - Gates: every banked row that reads `ReadMemStats` or runtime/metrics, DERIVED at gate time by
    `git grep`, never remembered; `runtime/metrics`; GolibTests in both flavours.
- **I3: cycles and pauses** (§5.1, if P-a).
  - Red: `TestReadMetricsConsistency`'s pause check fed with a nonzero `numgc` and no pauses.
  - Gate: `TestSchedPauseMetrics`' runtime.GC subtest now sees two GC samples per call and still
    passes (it asserts an increase).
- **I4: `ReadMemStats` alignment**, only if Q5 is ruled that way.

No converter change is expected. `consistentHeapStats.read` is already a registered hand-own, so
the footprint is 0 and CNR is owed only if a registry row moves.

## 7. Open questions for the ruling

1. **Scope.** Is this whole design (I1–I3) the owed wiring, with I4 on Q5's answer?
2. **Counts.** Zero COUNTS beside real BYTES leaves `/gc/heap/allocs:objects` at 0 while
   `/gc/heap/allocs:bytes` is real. Accept (the honest-zero rule), or refuse the bytes until counts
   exist?
3. **The clamps.** `inHeap = max(H, L)` and `committed = max(C, inHeap)`: mixed freshness chosen
   for non-negativity. Accept, or feed everything as of the last gen2? The latter is simpler but
   freezes `/gc/heap/allocs:bytes` between gen2s, which Go's is not.
4. **P-a or P-b** (§5.1).
5. **The heap goal** (§5.2): should `MemStats.NextGC` read `heapGoal()`?
6. **The writer lock** (§4.4): `metrics.Read` briefly takes worldsema. Accept the contention with
   stop-the-world callers, or name another exclusion?

-- C1, 2026-09-28
