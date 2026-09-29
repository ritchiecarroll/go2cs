# The ReadMemStats measurement surface — real pause facts, a truthful HeapReleased, and the discriminator that routes `math/big`

> **STATUS: DESIGN — RATIFIED (coordinator, 2026-08-21). All six §9 open questions are ruled AS
> RECOMMENDED**, including ⟨OQ-2⟩'s deliberate departure from Ruling B's literal "cumulative
> decrease" wording — the adversarial pass (§8.1) proved that phrasing produces a monotone field
> that is right exactly where it is tested and wrong everywhere else, and the commissioning text's
> PRINCIPLE (a truthful managed form, never fabrication) is exactly what the high-water formulation
> serves. §8.2's landing precondition is BINDING: `ReadMemStats` stays allocation-free, guarded by
> the named GolibTests guard, because `net/textproto`'s banked allocation-bracket test would
> otherwise move. Implementation proceeds per §7's staged landing (S0/S1 measurement first); §8 is
> the charter-§7 adversarial pass against this document's own first draft.
>
> **S2 and S3 are IMPLEMENTED and recorded in [§7.2](#72-s2s3--implemented-and-measured-2026-08-21)**
> (2026-08-21, same machine). The recorder is `src/core/golib/runtime/GcPauseRecorder.cs`; the two
> surfaces read one snapshot from it. Headlines: `TestReadGCStats` **CLOSES** — all nine assertions,
> on mechanism — so `runtime/debug` moves **2 → 3 of 9**; `ReadMemStats` is **allocation-free at
> ZERO** (288 B → 0.0 B/call, 0 B across all 200 bracketed windows), and the named guard is pinned
> there; `HeapReleased`, `LastGC`, `PauseNs`/`PauseEnd`, `PauseTotalNs` and `NumForcedGC` are all
> real. `TestFreeOSMemory` does **not** close, and §7.2.3 is the measurement that says why: its
> assertion 1 passes, its assertion 2 fails because the 32 MiB object is never reclaimed while the
> test method's own frame is live — reproduced in plain hand-written C#, with `HeapReleased`
> exonerated by the same probe releasing the full 33,689,600 B for the same object allocated behind a
> call that has returned. That is an object-lifetime divergence, not a measurement-surface one, and
> §7.1.10's "a Debug build" attribution is narrower than the phenomenon.
>
> **S0 and S1 are RUN and recorded in [§7.1](#71-measurements--s0-and-s1-run-2026-08-21)**
> (2026-08-21, i7-5820K, .NET 9.0.19, `GolibTests` 191/191). Headlines: the §5.4 discriminator reads
> **T = P**, so `math/big`'s 224-verdict row is **root (1)** and routes to the ж-box arc — ⟨OQ-3⟩
> does not fire; the recorder's per-gen2 overhead is **below the noise floor**; `TestFreeOSMemory`'s
> magnitude assertion **PASSES** under §4.1's formulation, so ⟨OQ-6⟩'s contingency does not arise;
> and **`ReadMemStats` is not allocation-free today** (288 B/call), so §8.2's precondition is work
> S2/S3 must do rather than a property to preserve. §7.1.9 lists the four places the measurements
> refine this document's own text — including the `GetGCMemoryInfo` overload §3.2 leaves unnamed,
> whose default is wrong.
>
> **Commissioned by Ruling B** (coordinator, 2026-08-20,
> [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md) — *"RULING x2 …
> `runtime-capability` is minted"*): `HeapReleased` and the per-GC pause history were **REFUSED** as
> disclosures because *"both have truthful managed forms (the cumulative `TotalCommittedBytes`
> decrease; a gen2-callback recorder capturing real pause facts). They are priced together as the
> ReadMemStats measurement-surface design — one arc, one recorder, both facts from it — and queued,
> not disclosed."* The measurement pass of the same day added the third consumer: *"The discriminator
> is cheap and it sits inside the ReadMemStats measurement-surface design Ruling B already
> commissioned."*
>
> **Companions:** `src/core/runtime/managed_impl.cs` (`ReadMemStats`, `GC`, `readMetricsManaged` —
> the file this arc edits), `src/core/runtime/debug/stubs_impl.cs` (`readGCStats`, `freeOSMemory`),
> `src/core/runtime/mstats.cs` (the `MemStats` declaration and its ring conventions),
> [`DESIGN-allocation-counting.md`](DESIGN-allocation-counting.md) (the precedent this design copies:
> a stated coverage boundary plus a cross-check that refuses to report a number the boundary
> invalidates), [`DESIGN-cooperative-scheduler.md`](DESIGN-cooperative-scheduler.md) §11 OQ8 (the
> ratified `GOMAXPROCS` position §5 depends on), [`DESIGN-zh-box-reduction.md`](DESIGN-zh-box-reduction.md)
> §3.6 (the arc `math/big`'s row routes to under one of §5's two outcomes), and the charter's §5 gate
> table.
>
> **Written against the corpus at `f77f825ec` (2026-08-21), Go 1.23.1, net9.0.** Every figure carries
> its date and provenance, or is named UNMEASURED.

---

## 1. The bill — what this surface answers today, and what asks for more

### 1.1 The surface, field by field

`runtime.ReadMemStats` is hand-owned (`managed_impl.cs`, `[module: GoManualConversion]`). It is not a
stub: it fills the fields the CLR genuinely measures and leaves the rest zero, and the file says so in
its header. Read as a census:

| `MemStats` field | Source today | Truthful? |
|:--|:--|:--|
| `Alloc`, `HeapAlloc`, `HeapInuse` | `GC.GetTotalMemory(forceFullCollection: false)` | yes, approximate by the API's own contract |
| `TotalAlloc` | `GC.GetTotalAllocatedBytes(precise: false)` | yes — process-cumulative, exactly like Go's |
| `Sys`, `HeapSys` | `GCMemoryInfo.TotalCommittedBytes` | yes |
| `HeapIdle` | `committed − live`, floored at 0 | yes, as a derivation |
| `NextGC` | `GCMemoryInfo.HeapSizeBytes` | yes |
| `PauseTotalNs` | `GC.GetTotalPauseDuration()` | yes — but **all generations**, see §2 |
| `NumGC` | `GC.CollectionCount(GC.MaxGeneration)` | yes — gen2 collections |
| `EnableGC` | `true` | yes |
| `HeapReleased` | **zero** | honest gap — this arc's §4 |
| `LastGC`, `PauseNs[256]`, `PauseEnd[256]` | **zero** | honest gap — this arc's §3 |
| `NumForcedGC`, `GCCPUFraction`, `DebugGC` | **zero** | see §4.3 |
| `Mallocs`, `Frees`, `Lookups`, `HeapObjects`, `BySize[61]`, the `Stack`/`MSpan`/`MCache`/`BuckHash`/`GC`/`Other` `Sys` breakdown | **zero** | allocator-internal bookkeeping the CLR does not keep; r56d measured that no in-process object count exists at all (see `DESIGN-allocation-counting.md` §1) |

The sibling surface, `runtime/debug.readGCStats` (`stubs_impl.cs`), fills the packed layout
`[n pauses][n pause ends][lastGC][numGC][totalPause]` with **n = 0**, `lastGC = 0`, and the same two
real aggregates. Its header states the refusal in the terms this design inherits: *"the CLR keeps no
comparable pause/end-time log, and fabricating one would be worse than reporting none."*

### 1.2 The three consumers that ask for more, with their exact asserts

**`runtime/debug` `TestReadGCStats`** (measured 2026-08-19, lane `claude/runtime-debug`: package at
2 of 9, `NumGC` reading 6) is a **self-consistency** check between the two surfaces. Nine assertions;
today exactly two fail, and both are length checks:

| # | assertion | today |
|:--|:--|:--|
| 1 | `stats.NumGC == mstats.NumGC` | passes (both read `CollectionCount`) |
| 2 | `stats.PauseTotal == mstats.PauseTotalNs` | passes (both read `GetTotalPauseDuration`) |
| 3 | `stats.LastGC.UnixNano() == mstats.LastGC` | passes vacuously (both 0) |
| 4 | `len(stats.Pause) == min(NumGC, 256)` | **FAILS** — 0 vs 6 |
| 5 | `stats.Pause[i] == mstats.PauseNs[off]`, walking the ring backwards | not reached (else-branch) |
| 6–7 | `PauseQuantiles` endpoints and monotonicity | pass vacuously (all zero) |
| 8 | `len(stats.PauseEnd) == min(NumGC, 256)` | **FAILS, `t.Fatalf`** |
| 9 | `stats.PauseEnd[i].UnixNano() == mstats.PauseEnd[off]` | not reached |

Note what that shape forbids. The *cheap* way to make 4 and 8 pass is to report `NumGC = 0` on both
surfaces — the lengths then agree at zero and the test goes green. That is refused here for the same
reason Ruling B's anti-laundering clause refuses a one-byte heap dump: it would **destroy a fact the
CLR genuinely measures** in order to satisfy an assert. The row closes by supplying the missing half,
or it does not close.

**`runtime/debug` `TestFreeOSMemory`** allocates a 32 MiB `[]byte`, drops it, calls `FreeOSMemory()`,
and makes two assertions:

* `after.HeapReleased > before.HeapReleased` — today fires as `no memory released: 0 -> 0`;
* `after.HeapReleased − before.HeapReleased >= bigBytes − slack`, with `slack = 16 MiB` and no extra
  page-size slack on a 4 KiB-page host, i.e. **≥ 16 MiB must actually be handed back**.

The second assertion is a property of the *platform*, not of the field's definition, and this design
does not assume it holds (§4.5, ⟨OQ-6⟩).

**`math/big` `TestMulUnbalanced`** reads `MemStats.TotalAlloc` around one `nat.mul` inside a
`GOMAXPROCS(1)` window and bounds the delta at 10× the input size. §5 is that row in full.

### 1.3 What already consumes `ReadMemStats` and therefore constrains any change

Censused over `src/core` at `f77f825ec`. Every row below is on the validated roster, so each is a
banked-row blast-radius item for this arc:

| Consumer | What it reads | Verdict exposure |
|:--|:--|:--|
| `net/textproto` `TestReadMIMEHeaderAllocations` | `TotalAlloc` delta across 200 windows, asserts `< 32,768 B` per header read | **direct** — a banked passing row that measures across `ReadMemStats` itself |
| `image/gif` `TestDecodeMemoryConsumption` | `HeapAlloc` delta, asserts `< 30 MiB` | **direct** |
| `expvar` | publishes the whole struct as `/debug/vars` JSON (`memstats()` → `ReadMemStats` → `ΔClone`) | indirect — its banked tests assert the handler, not the struct's contents |
| `sync` | `PauseNs[(NumGC+255)%256]` and `NumGC` deltas | **none** — both sites are `Benchmark`s, uniformly Phase-4D-deferred |
| `runtime/metrics` | nothing — see §6.3, it does **not** read this surface | none today |

---

## 2. The one definition this design rests on

**A Go GC cycle is a CLR gen2 collection.** Everything else follows from applying that identity
uniformly.

Go has one heap generation, so every Go cycle is a full cycle and `sum(PauseNs over all cycles) ==
PauseTotalNs` holds exactly. The CLR has three, so the mapping has to choose, and `NumGC` already
chose: it counts `GC.CollectionCount(GC.MaxGeneration)`. That convention is load-bearing and this
design keeps it, because the alternative — counting ephemeral collections as Go cycles — would report
a `NumGC` that no Go program's intuition survives (gen0 collections are frequent and are not
stop-the-world full cycles).

Applied uniformly, the identity says:

* the pause ring records **gen2** collections only;
* `LastGC` is the end time of the last **gen2** collection;
* `PauseTotalNs` becomes the ring's running sum, i.e. **gen2 pause time**, which is *smaller* than
  today's all-generation `GC.GetTotalPauseDuration()`. That is a change to an answer the surface
  already gives, so it is ⟨OQ-5⟩ rather than a silent consequence.

The alternative — keeping `PauseTotalNs` at `GetTotalPauseDuration()` and letting it exceed the
ring's sum — leaves the surface internally inconsistent for no gain: `TestReadGCStats` compares the
two *surfaces* to each other, not the sum to the total, so nothing forces it either way. One
definition, applied everywhere, is the durable choice.

---

## 3. The pause-history recorder

### 3.1 Mechanism space, priced

| Mechanism | What it gives | Price | Verdict |
|:--|:--|:--|:--|
| **Resurrecting finalizable sentinel** ("gen2 callback": an object nothing strongly references, whose finalizer does the work and calls `GC.ReRegisterForFinalize(this)`) | a synchronous-enough wake once per collection that condemns its generation; after its first promotion that generation is gen2 | one small object, one finalizer run per gen2 GC | **recommended** |
| `GC.RegisterForFullGCNotification` + `WaitForFullGCApproach`/`Complete` | approach/complete notifications for blocking gen2 | requires concurrent/background GC to be **off** — a process-wide GC configuration change forced on every converted program — plus a dedicated polling thread | **refused**: the configuration price is out of all proportion |
| EventPipe / in-process `EventListener` on the GC keyword | rich per-GC events including pause detail | r56d measured (net9.0/9.0.18, x64) that runtime events reach an in-process listener **asynchronously**, ~117 ms after the fact; plus a listener thread | **refused as the primary**, retained as the §3.6 fallback — but note it *re-opens* the exact race the arc is buying (§3.4) |
| Poll `GC.GetGCMemoryInfo` from each `ReadMemStats` call | no extra machinery at all | `GetGCMemoryInfo` reports only the **last** GC of a kind, so any collection between two reads is lost — the ring's slots are indexed by cycle number, so a lost cycle is a hole, not a rounding error | **refused**: it cannot fill a ring truthfully |

### 3.2 The design

A single `GcPauseRecorder` (golib, or `runtime`'s hand-own — see §7), holding:

* `ulong[256] pauseNs`, `ulong[256] pauseEndUnixNs` — the ring, fixed storage, allocated once;
* `long observed` — the number of gen2 collections this recorder has **seen**;
* `long lastGcEndUnixNs`, `ulong pauseTotalNs` — the running aggregates;
* `long committedHighWater` — §4's high-water mark, sampled at the same points;
* a plain lock guarding the six.

`Observe()` is the one write path, and it is **idempotent per collection**:

1. read `GC.CollectionCount(GC.MaxGeneration)`; if it has not advanced past `observed`, return —
   this is also what filters the sentinel's first one or two callbacks, which fire on ephemeral
   collections before the sentinel is promoted;
2. read the collection's facts from `GC.GetGCMemoryInfo(…)` — `PauseDurations` summed for the pause,
   `DateTime.UtcNow` for the end time (the CLR publishes no per-GC wall-clock end stamp; the
   recorder's own read is the honest approximation and is documented as such);
3. write `pauseNs[observed % 256]` and `pauseEndUnixNs[observed % 256]`, then `observed++`.

That ordering is Go's own, verbatim: `gcMarkTermination` writes `pause_ns[numgc%256]` *then*
increments `numgc`, which is why `MemStats`' doc comment can say *"the most recent pause is at
`PauseNs[(NumGC+255)%256]"`. Writing the ring the same way is what makes `ReadGCStats`' backwards walk
(`off := (NumGC + 255) % 256`, decrementing) line up **by construction** rather than by agreement.

**`NumGC` is then the recorder's `observed`, not `CollectionCount`.** This is the design's central
move and it is worth stating plainly: the two surfaces (`ReadMemStats` and `readGCStats`) read one
snapshot taken under one lock, so assertions 1–9 of §1.2 hold because there is no second source to
disagree with. The cost is that `NumGC` can lag the true gen2 count by at most one collection, for at
most the finalizer's scheduling latency. **Understating is the safe direction** — a pause the recorder
did not observe is not invented — and §3.4 closes the lag at the one boundary where Go's tests depend
on it.

`readGCStats` fills the packed buffer from the same snapshot: `n = min(observed, 256)` pauses
most-recent-first, then `n` end times most-recent-first, then `lastGC`, `numGC`, `pauseTotal`, and
returns the buffer re-sliced to `2n+3` — which is exactly what `ReadGCStats`' `n := len(stats.Pause)-3;
n /= 2` arithmetic expects, and what the current n=0 implementation already honors at length 3.

### 3.3 The activation model — the always-on cost, priced

Ruling B's reservation is verbatim: *"it is an always-on recorder every converted program pays for."*
Three models, priced:

**(a) Always on, armed from `runtime`'s package initializer** — recommended.
*Cost:* one sentinel object (~24 B) permanently cycling through the finalization queue; one finalizer
invocation, one `GetGCMemoryInfo` call and two array writes **per gen2 collection**; the ring's two
fixed 2 KiB arrays for the process lifetime. Nothing per allocation, nothing per `ReadMemStats` call
that is not already paid.
*Compare the accepted precedent:* `AllocationCounter` charges a static-bool read and a
never-taken branch at **every golib allocation site**, measured at 11–15 % on the tightest allocation
loops when enabled and within noise when disabled (r58a, 2026-08-09, desktop net9.0 Release —
`DESIGN-allocation-counting.md` §8). This recorder's cost is O(1) per **gen2 collection**, a rate
lower by many orders of magnitude. The worry is real in shape and small in size — but its size is
UNMEASURED, and §3.6 makes measuring it a landing precondition rather than a claim.

**(b) First `ReadMemStats` arms it** — rejected, on correctness rather than cost.
Arming late means `observed` starts at zero after N collections have already happened. Either the
surface then reports `NumGC = 0` when the CLR has really collected N times — destroying a fact it
answers truthfully today, exactly the laundering §1.2 refuses — or it seeds `observed` from
`CollectionCount` and claims a ring it cannot fill, which breaks assertion 4 in the opposite
direction. Lazy arming makes the surface *less* true in order to save a cost nobody has measured.

**(c) Test-host only** (arm from `TestHost.Run`, beside `AllocationCounter.Enable()`) — rejected,
though it is the cheapest and has a real precedent.
It would make the runtime surface answer **differently under test than in production**: a converted
application asking for GC pause history gets zeros, and the only programs for which the feature works
are the ones proving it works. That is a worse property than a per-gen2-collection cost, and it is the
one shape a measurement surface must never have. (The precedent does not transfer: `AllocationCounter`
is host-gated because its cost is per-allocation *and* its only consumer is `testing.AllocsPerRun`,
which exists only under a host. `ReadMemStats` has a production consumer today — `expvar`'s
`/debug/vars`.)

**Recommendation: (a), with a `GO2CS_GC_PAUSE_HISTORY=0` opt-out** for anyone who *measures* a
problem — an escape hatch, not a configuration knob, documented as such. ⟨OQ-1⟩.

### 3.4 Finalizer-timing skew, and the specific mitigation

The skew is real: the sentinel's finalizer runs after the collection completes, so a `ReadMemStats`
landing in that gap sees `observed = k−1` while the CLR has completed `k` gen2 collections.

**No assert in `runtime/debug`'s suite is exposed to it**, because every one of §1.2's nine
assertions compares the two surfaces to *each other* and both read the same `observed`. There is no
assert anywhere in the consuming set that compares `NumGC` to an independently-known collection count.

The one place the lag would bite is the boundary Go's tests actually use: `runtime.GC()` is documented
to complete a full cycle, and Go's tests rely on the world being quiet when it returns. The mitigation
is exact and small:

> **`runtime.GC()` and `debug.freeOSMemory()` drain the recorder before returning.**

Both already end in a blocking, compacting, forced gen2 `GC.Collect`, and `runtime.GC()` already calls
`GC.WaitForPendingFinalizers()` between its two collects *for this very class of reason* ("callers …
rely on finalizers having RUN by the time it returns"). The change is to end each with
`GcPauseRecorder.Drain()`, which is `GC.WaitForPendingFinalizers()` followed by a direct `Observe()`.
Because `Observe()` is idempotent per collection (§3.2 step 1), the direct call cannot double-record
whatever the finalizer already recorded, and the finalizer cannot double-record what the drain took.
After `runtime.GC()` returns, `NumGC` is current — which is precisely the state `TestReadGCStats`
reads it in.

### 3.5 What the asserts consume, restated as an acceptance list

With the recorder in place and both surfaces reading one snapshot:

* assertions 1, 2, 3 hold by construction (one source);
* 4 and 8 hold by construction (`len == min(observed, 256)` on both sides);
* 5 and 9 hold by construction (one ring, one indexing convention);
* 6 and 7 are computed by `ReadGCStats` itself from the values it was handed and hold for any
  non-degenerate data.

`TestReadGCStats` therefore closes on **mechanism**, not on tuning — which is the property that makes
it worth building rather than disclosing.

### 3.6 Cost and correctness measurements this arc OWES before it lands

Nothing in §3.3 is measured yet. The arc delivers, on a named machine and date, in the shape of
`DESIGN-allocation-counting.md` §8:

1. **Recorder overhead per gen2 collection** — an allocation-churn benchmark forcing N gen2
   collections, recorder armed vs disarmed, wall time and `GetTotalPauseDuration` compared.
2. **`ReadMemStats` must remain ALLOCATION-FREE.** This is a landing precondition, not a nicety —
   §8's cost lens explains why. The guard: a `GolibTests` case that calls `ReadMemStats` in a loop and
   asserts `GC.GetAllocatedBytesForCurrentThread()` moved **zero** across the loop. (The read path is
   designed to satisfy it: `array<T>` is a `readonly struct` over a backing array the `MemStats`
   constructor already allocated, and `ref T this[index]` writes into it, so filling up to 256 slots
   allocates nothing; `GetGCMemoryInfo` returns a struct. Designed-to and proven-to are different
   things, and only the second one lands.)
3. **Sentinel liveness** — a probe proving the sentinel keeps firing after promotion (i.e. that
   `observed` tracks `CollectionCount(MaxGeneration)` over a long run, lagging by at most 1), with the
   negative control that a strongly-referenced sentinel never fires at all.
4. **`GetGCMemoryInfo` fidelity** — that `PauseDurations` for the gen2 GC is what the recorder thinks
   it is, and which `GCKind` overload answers for a background gen2 collection. Everything §3.2 says
   about that API is read from its documented surface and is **unverified against the live runtime**.

If (3) or (4) fails, the EventPipe fallback of §3.1 is the retreat — at the stated price that its
asynchrony re-opens the lag §3.4 closes.

---

## 4. `HeapReleased`, and the fields around it

### 4.1 The formulation — and a departure from the ruling's wording

Ruling B names *"the cumulative `TotalCommittedBytes` decrease"*. Taken literally that is a **monotone
counter**: accumulate `max(0, previousCommitted − currentCommitted)` at each sample and never decrease.
It satisfies `TestFreeOSMemory`. It also contradicts the field it implements.

Go documents `HeapReleased` as *"bytes of physical memory returned to the OS. This counts heap memory
from idle spans that was returned to the OS and **has not yet been reacquired** for the heap."* It is
a **current** quantity and it goes **down** when the heap grows back. A monotone lifetime total is a
different quantity wearing the same name — and in a long-running program it drifts arbitrarily far
above the truth.

The formulation that is both truthful and sufficient:

> **`HeapReleased = max(0, committedHighWater − currentCommitted)`**, where `committedHighWater` is the
> running maximum of `GCMemoryInfo.TotalCommittedBytes` and `currentCommitted` is its latest sample.

Committed bytes are bytes the process holds from the OS. Bytes committed at the high-water mark and
not committed now are bytes returned to the OS and not yet reacquired — Go's definition, read from a
real measurement, decreasing exactly when Go's decreases. It satisfies `TestFreeOSMemory`'s first
assertion for the same reason the monotone form does, and it is right for the reason the monotone form
is not.

Because this **departs from the commissioning ruling's literal text**, it is ⟨OQ-2⟩ rather than a
lane decision.

Two honesty notes that belong in the code comment, not just here:

* `GCMemoryInfo.TotalCommittedBytes` is a **snapshot as of the last GC**, not a live figure. So
  `HeapReleased` is fresh exactly when a GC has just run — which is the case at every point
  `TestFreeOSMemory` reads it, since `FreeOSMemory` collects. Elsewhere it is as stale as the last
  collection, and the header says so.
* The high-water mark is sampled at every `ReadMemStats` call **and** at every recorder `Observe()`,
  so a program that never calls `ReadMemStats` still tracks it correctly through the recorder, and a
  program that calls it in a tight loop pays one comparison.

### 4.2 Which fields inherit the same derivation honestly

Once the recorder exists, four more fields can be answered without inventing anything:

| Field | Derivation | Note |
|:--|:--|:--|
| `LastGC` | recorder's `lastGcEndUnixNs` | the recorder's own read of the clock at `Observe()`; the CLR publishes no per-GC end stamp, so the approximation is named |
| `PauseNs`, `PauseEnd` | the ring | §3 |
| `PauseTotalNs` | the ring's running sum (gen2) | replaces `GetTotalPauseDuration()`; ⟨OQ-5⟩ |
| `NumForcedGC` | a counter `runtime.GC()` and `debug.FreeOSMemory()` increment on completion | Go's field is *"GC cycles that were forced by the application calling the GC function"* — the managed model can count exactly that, and it is a fact about the **program**, not about the collector |

### 4.3 What stays zero — and the rule that decides

> **A field is answered only when a managed measurement means the same thing the Go field means.
> Where the CLR measures something adjacent-but-different, the field stays zero and the
> documentation names the adjacent quantity and states why it was refused.**

That rule is `DESIGN-allocation-counting.md`'s discipline generalized from one counter to a whole
struct: a stated boundary beats an unstated approximation, and *"a plausible-looking invented number"*
is the failure mode both documents exist to prevent. Applied:

* **`Mallocs` / `Frees` / `HeapObjects` / `BySize` / `Lookups` — stay zero.** r56d established by
  measurement, not assertion, that the CLR publishes **no in-process object count** at all. The
  adjacent quantity is bytes; bytes are not counts. (`HeapObjects = Mallocs − Frees` stays consistent
  at zero.)
* **`GCCPUFraction` — stays zero.** The adjacent quantity is `GCMemoryInfo.PauseTimePercentage`, which
  is *pause time as a share of wall time since the last GC*. Go's field is *GC's share of this
  program's available **CPU** time since the program started*. Different numerator, different
  denominator, different window. Deriving one from the other would produce a number in the right
  range and of the wrong kind — the textbook case this rule exists to refuse.
* **The `Stack`/`MSpan`/`MCache`/`BuckHash`/`GC`/`Other` `Sys` breakdown — stays zero.** These are
  Go allocator arenas. The CLR has no corresponding partition to report.
* **`DebugGC` — stays false.** Go's own field is unused.

### 4.4 The Go invariants the managed struct does not satisfy — stated, never repaired

`MemStats`' doc comments assert relations between fields. Two of them do not hold here, one already
and one newly:

* **`Sys == StackSys + MSpanSys + … + OtherSys`.** Already false today: `Sys` is committed bytes while
  every breakdown term is zero. Repairing it would mean inventing the breakdown. Refused.
* **`HeapIdle >= HeapReleased`** — newly at risk. `HeapIdle` is instantaneous (`committed − live`)
  while `HeapReleased` is a difference against a historical high-water mark; after a large release the
  second can exceed the first. **Clamping `HeapReleased` to `HeapIdle` is refused**: it would make a
  measured number smaller to satisfy a relation the model does not have.

Both belong in `managed_impl.cs`'s header, so a reader of this struct learns which invariants survived
the port from the surface itself and not by discovering it in a debugger.

### 4.5 What this arc does NOT promise about `TestFreeOSMemory`

The first assertion (`after > before`) closes if the CLR releases **anything**. The second requires
**≥ 16 MiB** to come back from a 32 MiB LOH object after an aggressive compacting collect. Whether
`TotalCommittedBytes` actually falls by that much is a property of the CLR's segment/decommit policy
and is **UNMEASURED**. It is cheap to measure — a ~20-line probe — and the implementation arc measures
it **before** claiming the row, not after. ⟨OQ-6⟩ carries the contingency.

---

## 5. The `math/big` discriminator

### 5.1 The row, and what is actually in dispute

```go
func TestMulUnbalanced(t *testing.T) {
    defer runtime.GOMAXPROCS(runtime.GOMAXPROCS(1))
    x := rndNat(50000); y := rndNat(40)
    allocSize := allocBytes(func() { nat(nil).mul(x, y) })   // a TotalAlloc delta
    inputSize := uint64(len(x)+len(y)) * _S                  // (50000+40)*8 = 400,320 B
    if ratio := allocSize / uint64(inputSize); ratio > 10 { t.Errorf(...) }
}
```

Measured on the converted side — three independent byte readings, and a fourth run that reproduced the
row:

| measurement | `allocSize` | provenance |
|:--|--:|:--|
| r57a | 20,487,200 B | 2026-08-09 |
| r58b | 20,487,208 B | ж-box A-stage gate |
| ж-box A3 pinned | 20,499,128 B | 2026-08-13, recorded as "+0.06 %, unmoved" |
| measurement pass | ratio 51, `224/226` reproduced exactly | 2026-08-20, an independent pipeline run on an unmodified converter |

Ratio 51 against a bound of 10. Two candidate roots, named by the measurement pass and deliberately
not separated there: **(1)** the converted `nat.mul` genuinely allocates ~5× what Go's does;
**(2)** `GetTotalAllocatedBytes(precise: false)` is process-wide and unsynchronized, so the window
catches other threads' allocations where Go's `GOMAXPROCS(1)` window catches almost none.

**The evidence already in hand favors (1), and should be stated before any new measurement is taken.**
The spread across those three runs — taken on different trees, in different sessions, on at least two
different machines — is **0.058 %**. Cross-thread pollution is stochastic: background-thread
allocation caught inside a millisecond-scale window would vary run to run, not agree to six parts in
ten thousand. Determinism at that level is the fingerprint of the code under test allocating the same
bytes every time. Further, 20.5 MB is three orders of magnitude above the CLR's per-thread allocation
context quantum, so the `precise: false` **lag** cannot account for it either — imprecision bounds the
error at roughly one allocation context per thread, not at 20 MB.

That is a strong prior, not a finding. The measurement below is the finding.

### 5.2 Is a per-window, per-thread read sound? — the affinity analysis

The brief's proposed instrument is `GC.GetAllocatedBytesForCurrentThread()` on the test goroutine's
thread. Its soundness rests on goroutine-to-thread affinity, and in this tree that affinity is
**total**:

* `Goroutine.Start` gives **each goroutine its own dedicated thread** for its whole life
  (`golib/runtime/Goroutine.cs`: *"a go2cs goroutine is, and remains, a thread for its whole life"* —
  the CLR offers no stack switching, so there is no migration to worry about). The scheduler design's
  OQ1 ratified exactly this executor (2026-08-13).
* The converted test host runs each test body on its own thread (`testing/TestExecution.cs`).
* `testing.AllocsPerRun` already relies on the same property and says so: the thread scoping *"stands
  in for the `runtime.GOMAXPROCS(1)` pinning Go's `AllocsPerRun` uses to keep other goroutines out of
  its measurement."*
* `nat.mul` spawns nothing, so the whole measured window runs on one thread.

So a per-thread read over this window is **sound as a measurement**.

> **Doc-drift finding, recorded not fixed** (this lane is docs-only and does not own those files):
> both `testing.AllocsPerRun`'s remarks (`src/core/testing/testing.cs`) and
> `DESIGN-allocation-counting.md` §3 still say *"converted goroutines share the thread pool"*. That
> was true before the dedicated-thread executor landed and is now stale — in the safe direction (both
> texts understate the affinity they rely on), but stale in exactly the sentence a future reader would
> cite when judging §5.2. One-line fix, owed to whichever lane next touches either file.

### 5.3 Why `TotalAlloc` is nevertheless NOT redefined

Sound as a measurement is not the same as correct as a **field**. `MemStats.TotalAlloc` is documented,
in Go and in `mstats.cs`, as *cumulative bytes allocated for heap objects* — a **process** quantity.
Answering it per-thread would mean:

* two goroutines reading `TotalAlloc` see unrelated, non-comparable numbers, and neither is the
  program's total;
* `expvar`'s `/debug/vars` publishes a thread-local figure under a process-wide name — a lie to a
  production consumer, delivered over HTTP;
* the `net/textproto` banked row's assert silently changes meaning.

Trading a true process number for a convenient thread number to make one test pass is the shortcut
the charter's §2 names. **`TotalAlloc` stays process-wide and cumulative.** Nor does it become
`precise: true` by default: precision costs a walk of every thread's allocation context on a call that
`net/textproto` makes 400 times inside a measured region, and §5.1's arithmetic shows imprecision
cannot explain a 20 MB reading anyway. If a later measurement shows the lag matters for some other
consumer, precision is revisited then, with that measurement in hand.

The discriminator is therefore **an experiment, not a field change** — which is also why it can be run
before anything ships.

### 5.4 The measurement, made runnable

**Location: `src/tests/GolibTests`**, which already binds converted packages for exactly this kind of
otherwise-unreachable guard (`core/vendor/golang.org/x/crypto/sha3` for the sha3 reinterpret fix,
`core/testing` for the host's output cap). Add a `core/math/big` project reference and one test class.

`nat` is package-internal, so the probe drives the same code path through the public surface:
`new(big.Int).Mul(x, y)` with `x` at 50,000 words (3,200,000 bits) and `y` at 40 words reaches
`nat.mul` with the same unbalanced shape. Three numbers around one window, on the test's own thread:

| symbol | instrument | what it means |
|:--|:--|:--|
| **P** | `GC.GetTotalAllocatedBytes(precise: true)` delta | process-wide, exact — what `TotalAlloc` would report with the lag removed |
| **T** | `GC.GetAllocatedBytesForCurrentThread()` delta | this thread only, exact, pollution-free |
| **C** | `AllocationCounter.CurrentThreadCount` delta | golib **objects** on this thread — already thread-scoped, already instrumented, needs only `AllocationCounter.Enable()` |

**Decision rule, fixed in advance:**

* **`T ≈ P` (within a few %) → root (1).** The window's allocation is the code under test's own. The
  row is the allocation model, and it routes to the **ж-box reduction arc**, which already names it:
  *"`math/big` `TestMulUnbalanced` … may move; measured, not promised"* (`DESIGN-zh-box-reduction.md`
  §3.6). `C` then localizes the residual to golib sites, and `T / 400,320` says directly whether the
  row could ever clear the 10× bound.
* **`T ≪ P` → root (2).** The process-wide read is catching other threads, and the test's premise —
  a `GOMAXPROCS(1)`-quiet window — does not hold in the managed model. See §5.5.

The probe is a normal `GolibTests` case: it runs under the existing gate, needs no pipeline, no
corpus regen and no converter change, and it is reproducible from a clone. It should report all three
numbers in its assertion message whatever it concludes, so the record is the numbers and not the
verdict.

### 5.5 Where root (2) would route — and why it is still not a disclosure

If pollution is the root, the honest fix is **not** at `TotalAlloc` (§5.3) but at the divergence that
lets other goroutines run inside a `GOMAXPROCS(1)` window. That divergence is stated and ratified:
`GOMAXPROCS` is a remembered value that does not cap parallelism, and the scheduler design's **OQ8**
kept it that way on the explicit grounds that *"a runnable-limiter honoring GOMAXPROCS is M2-shaped
machinery **with no consuming suite**"* (ratified 2026-08-13).

Root (2) would supply the first consuming suite. The recommendation is therefore to **reopen OQ8 on
that evidence**, not to mint a disclosure: `runtime-capability`'s own admission test asks *"does a
truthful managed implementation of the asserted behavior exist at any cost?"* — and for a runnable
limiter the answer is plainly yes. ⟨OQ-3⟩ carries it, because reopening a ratified scheduler decision
is a coordinator act.

---

## 6. Consumers and acceptance

### 6.1 `runtime/debug`

`TestReadGCStats` closes on §3's mechanism. `TestFreeOSMemory`'s first assertion closes on §4; its
second is measured before it is claimed (§4.5). `WriteHeapDump`'s three rows are **not touched** by
this arc — they are Ruling B's `runtime-capability` disclosure, pinned AS FAILING by that class's own
anti-laundering clause. `TestStack` is the position-map arc. This arc buys **two** of the nine
verdicts and no more; the package banks when the position map lands and the disclosure is written.

> **MEASURED (S2/S3, 2026-08-21): it buys ONE, and the second is blocked outside this arc.**
> `TestReadGCStats` closed exactly as predicted — all nine assertions, on mechanism (2 → **3 of 9**).
> `TestFreeOSMemory` did not: its assertion 1 PASSES (`0 -> 3,039,232`) and its assertion 2 fails,
> because the 32 MiB object is never reclaimed while the test's own frame is live (§7.2.3). Nothing in
> this arc can move it — `HeapReleased` reports 33,689,600 B for the same object when it is genuinely
> unreachable — so the row belongs to whichever arc owns converted-program object lifetime, and
> `runtime/debug` now needs the position map, the `WriteHeapDump` disclosure, `TestSetCrashOutput`
> **and** that lifetime question before it banks.

### 6.2 `math/big`

No verdict is bought directly. What lands is the **discriminator** (§5.4) and therefore the routing:
one outcome sends the row to an arc that already exists, the other reopens a ratified scheduler
question. Either way the row stops being *"two candidate roots, not separated."* If root (1) holds and
the ж-box arc later moves the number below the bound, `math/big` costs **one** manifest entry
(`TestNewIntAllocs`, textbook `alloc-count-semantics`) for 224 verdicts.

### 6.3 `runtime/metrics` — the census the board asked for

The board flagged this as *"a NEW measurement surface every `ReadMemStats` consumer inherits,
`runtime/metrics` included."* **Censused at `f77f825ec`, the two surfaces are already fully decoupled,
and `runtime/metrics` inherits nothing automatically:**

* `metrics.Read` is hand-owned (`runtime/metrics/sample.cs`) and crosses into
  `runtime.readMetricsManaged` (`managed_impl.cs`) — names in, `(kind, scalar, pointer)` out.
* That crossing preserves `readMetricsLocked`'s batch semantics and then runs the **auto-converted**
  compute closures over `statAggregate`, which read Go's own `memstats`, `gcController` and
  `consistentHeapStats` — *not* `ReadMemStats`.
* `consistentHeapStats.read` is hand-owned to return the **zero delta**, because *"nothing ever writes
  a `heapStatsDelta` — the CLR allocator does not populate Go's allocator bookkeeping."*

Consequences, precisely:

* `/memory/classes/heap/released:bytes` computes from `heapStats.released` and therefore reads **0**,
  and will keep reading 0 after `MemStats.HeapReleased` becomes real.
* `/gc/pauses:seconds` writes from `sched.stwTotalTimeGC`, a runtime histogram nothing populates.
* `/gc/cycles/total:gc-cycles` likewise reads Go's own counters, not `NumGC`.
* The banked `runtime/metrics` row is **`TestNames` + `TestDocs`** — the description table's
  sorted-name/regexp contract and its doc.go agreement, plus a `Read` round trip that computes a
  **kind** for every published metric. No value is asserted. **This arc cannot move that row.**

So there is no inherited breakage. There *is* an inherited **incoherence**: after this arc, go2cs
answers "how many bytes did this process return to the OS" with a real number on one surface and zero
on another, where Go documents the two as the same quantity. Wiring the metrics closures to the
recorder is a larger change (it converts auto-converted compute closures into hand-owns, on a banked
package, for no consuming test), so the recommendation is **record the divergence in both headers and
on the board, and defer the wiring until a consumer demands it**. Because "one measurement, one source
of truth" is a shared-architecture principle rather than a local call, it is ⟨OQ-4⟩.

### 6.4 Banked-row blast radius, and the gates

This arc changes `src/core/runtime`'s hand-owns and possibly golib. **No converter change**, so no CNR
is owed (charter §5 binds CNR to converter changes). The gate list:

| Gate | Why |
|:--|:--|
| `GolibTests` | the new probes of §3.6 and §5.4 live here |
| Full `go2cs.slnx` Debug build | mandatory after any golib/runtime API change — the only gate that compiles the non-generated members |
| Full behavioral suite | golib is linked by everything |
| Filtered validated sweep — **`net/textproto`, `image/gif`, `expvar`, `sync`, `time`, `runtime/metrics`, `math/big`, `runtime/debug`** | the §1.3 census: every banked row that reads this surface, plus the two target packages. Derive the list again at gate time from a fresh `git grep` for `ReadMemStats`/`MemStats` under `src/core` rather than reusing this table — the crypto/tls merge lesson (2026-08-19) is that a canary set is derived, never remembered |
| Post-merge filtered re-sweep at the merge RESULT | banking-merge rule, same lesson |

---

## 7. Staged landing

| Stage | Content | Gate |
|:--|:--|:--|
| **S0** | The §5.4 discriminator alone — a `GolibTests` case, a `core/math/big` project reference, zero production change. Publishes P/T/C and routes `math/big`. | `GolibTests` |
| **S1** | The §3.6 measurement probes: sentinel liveness, `GetGCMemoryInfo` fidelity, recorder overhead, `ReadMemStats`-is-allocation-free. Still zero production change. | `GolibTests` |
| **S2** | The recorder + `NumGC`/`LastGC`/`PauseNs`/`PauseEnd`/`PauseTotalNs` + the `GC()`/`freeOSMemory()` drain + `readGCStats` filling `2n+3`. | full list, §6.4 |
| **S3** | `HeapReleased` + `NumForcedGC` + the §4.4 invariant statement in the header. | full list, §6.4 |

S0 and S1 are pure measurement and can land — or simply be run and reported — before any ruling on
⟨OQ-1⟩/⟨OQ-2⟩. That is deliberate: the two questions a coordinator has to rule are both cheaper to
rule with S1's numbers in hand, and S0 is worth running whatever happens to the rest of this arc.

---

## 7.1 Measurements — S0 and S1, run 2026-08-21

> **S0 and S1 are DONE.** Every figure below is measured on **the i7-5820K coordinator machine**
> (6C/12T, 31.9 GB, Windows 11 10.0.26100), **.NET SDK 9.0.317 / runtime .NET 9.0.19**, workstation
> **concurrent** GC (`IsServerGC=False`, `LatencyMode=Interactive`), `GolibTests` built **Debug**, on
> the corpus at **`aaacb1e40`**, Go 1.23.1. Gate: **`GolibTests` 191/191** (34 s), the charter-§5 gate
> for this change class. No production file changes; no CNR is owed (no converter change).
>
> Nothing here is self-ruled. Four readings **refine or contradict text in this document** and are
> called out as such in §7.1.9.

### 7.1.1 The instrument

Two files, both `GolibTests` cases, both reproducible from a clone with
`dotnet test src/tests/GolibTests/GolibTests.csproj -c Debug --filter <name> --logger "console;verbosity=detailed"`:

| File | Stage | Contents |
|:--|:--|:--|
| `src/tests/GolibTests/MulUnbalancedDiscriminatorTests.cs` | S0 | the §5.4 discriminator and the window's wall-clock context |
| `src/tests/GolibTests/GcMeasurementSurfaceProbes.cs` | S1 | §3.2's recorder transcribed as a test-local prototype, plus the four §3.6 probes, the natural-pressure and background-GC probes, and the §4.5 ⟨OQ-6⟩ probe |

`GolibTests.csproj` gains two project references — `core/math/big` (S0's operand path) and
`core/runtime` (the allocation guard calls the real `ReadMemStats`, not a model of it) — on the
precedent the file already carries for `sha3` and `testing`.

Every probe **reports**; the only assertions are the ones that would make a reading vacuous if they
failed (an instrument that answered nothing, a mechanism that never fired), plus the one real guard
of §7.1.4.

### 7.1.2 S0 — the §5.4 discriminator: **root (1)**, decisively

One `Int.Mul` of a 50,000-word by a 40-word operand (`inputSize` = 400,320 B, Go's own figure), six
windows, three instruments:

| round | **P** process-wide, exact | **T** this thread, exact | **C** golib objects | T/P | T / inputSize |
|:--|--:|--:|--:|--:|--:|
| 0 (cold) | 20,527,408 B | 20,527,368 B | 10,001 | 100.00 % | 51.3× |
| 1 | 20,520,824 B | 20,520,824 B | 9,995 | 100.00 % | 51.3× |
| 2 | 20,520,864 B | 20,520,824 B | 9,995 | 100.00 % | 51.3× |
| 3 | 20,520,864 B | 20,520,824 B | 9,995 | 100.00 % | 51.3× |
| 4 | 20,520,824 B | 20,520,824 B | 9,995 | 100.00 % | 51.3× |
| 5 | 20,520,864 B | 20,520,824 B | 9,995 | 100.00 % | 51.3× |

**The decision rule of §5.4, applied as written: `T ≈ P` — indeed `T = P` to within 40 bytes — so the
root is (1).** The window's allocation is the code under test's own. `math/big`'s 224-verdict row is
an **allocation-model** row and it routes to the **ж-box reduction arc**, which already names it
(`DESIGN-zh-box-reduction.md` §3.6). It is **not** a `GOMAXPROCS`/pollution row, so ⟨OQ-3⟩ does not
fire and scheduler OQ8 stays closed.

Three things make that conclusion solid rather than merely arithmetic:

* **The probe reproduces the row.** Cold `P` = 20,527,408 B against the row's banked `allocSize`
  readings of 20,487,200 B (r57a) and 20,499,128 B (ж-box A3) — a 0.20 % spread across two machines,
  three trees and a deterministic-vs-`rndNat` operand. The probe is measuring the row.
* **The window is long.** 456–522 ms per `Mul` (Debug). Cross-thread pollution had half a second per
  window to appear in `P` and did not.
* **When pollution *did* appear, it appeared exactly where §5.1 predicted.** One window in twelve read
  `P` = 22,443,168 B (+1.92 MB) while `T` held at 20,520,824 B — the stochastic signature moving the
  process-wide instrument and never the per-thread one. That is the design's own argument, observed.

**The multiplier against Go, measured on this machine** (`go1.23.1`, the same operand shape through
`new(big.Int).Mul`, `TotalAlloc` bracket): Go allocates **403,488 B, ratio 1.01×**. The converted path
allocates **50.9× what Go's does**, not the "~5×" §5.1 offers as the shape of root (1). Go does not sit
near the 10× bound — it sits at 1× — so the row needs the converted path to shed **≈ 80 % of its
allocation** to clear the bound, not a trim. `C` = 9,995 golib objects for 20.5 MB (≈ 2,053 B/object)
localizes the residual to golib backing-store sites and is the number the ж-box arc starts from.

### 7.1.3 §3.6 item 1 — recorder overhead per gen2 collection: **below the noise floor**

3 rounds × 60 forced gen2 collections with real allocation churn between each, a freshly-armed
recorder against a disarmed control, alternated so machine drift cancels. Four independent runs:

| run | disarmed | armed | overhead |
|:--|--:|--:|--:|
| 1 | 1.284 ms/collection | 1.279 ms | **−0.005 ms (−0.4 %)** |
| 2 | 1.254 ms | 1.262 ms | **+0.008 ms (+0.6 %)** |
| 3 | 1.298 ms | 1.287 ms | **−0.011 ms (−0.9 %)** |
| 4 | 1.637 ms | 1.581 ms | **−0.056 ms (−3.4 %)** |

The sign changes run to run, so the recorder's per-collection cost is **not resolvable against a
1.25–1.64 ms gen2 collection**. `GetTotalPauseDuration` per collection moves the same way and by the
same margin. For scale, the precedent §3.3 cites — `AllocationCounter` at 11–15 % on the tightest
allocation loops — is a per-*allocation* charge; this one is per-*gen2-collection* and is unmeasurable
there. **Ruling B's "an always-on recorder every converted program pays for" is a real worry whose
measured size is zero**, which is what ⟨OQ-1⟩'s always-on recommendation was ratified on the
expectation of.

### 7.1.4 §3.6 item 2 — the allocation guard: **`ReadMemStats` is NOT allocation-free today**

| measurement | reading |
|:--|:--|
| `ReadMemStats` per call, `GetAllocatedBytesForCurrentThread` over 2,000 calls | **288.0 B/call**, exactly, every run |
| 200 empty bracketed windows (two back-to-back `ReadMemStats`, nothing between), `TotalAlloc` delta | **244.7 / 285.7 / 326.7 B per window** (three runs), **8,200 B worst single window**, 6–8 of 200 windows non-zero |
| the worst window as a share of `net/textproto`'s 32,768 B per-iteration budget | **25.02 %** |

The root is one BCL object: `GCMemoryInfo` is a struct, but `GC.GetGCMemoryInfo()` allocates a fresh
`GCMemoryInfoData` **class** behind it on every call. §3.6 item 2's parenthetical reads that surface
as allocation-free and it is not.

Two consequences, and the second is the sharp one:

* **§8.2's landing precondition is already violated by the code as it stands.** S2/S3 do not
  *preserve* an allocation-free read path, they have to *create* one — which the recorder makes easy,
  since it already samples `TotalCommittedBytes` at every `Observe()` and can hand `ReadMemStats` the
  figure instead of each read fetching its own.
* **The cost is currently *masked*, not absent, and the mask is lumpy.** `TotalAlloc` reads
  `GetTotalAllocatedBytes(precise: false)`, which does not move until a thread's allocation context is
  exhausted, so most windows read 0 and roughly one in thirty absorbs a whole ~8 KB quantum. A
  `net/textproto`-shaped measurement is therefore already charged **a quarter of its budget** in its
  worst iteration, today, by the brackets alone. Anything the read path adds raises both the mean and
  the lumpiness.

**The guard lands with this stage**, `GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation`,
pinned at a **320 B ceiling** — today's 288 B plus headroom for a runtime that grows the struct. It is
a ceiling, not the target: **zero stays the S2/S3 precondition and this constant is the instrument
that gets tightened to it.** Until then the guard catches the regression §8.2 is actually afraid of —
a ring copied into a fresh array per read (2 KiB) or a snapshot object per call. A failure reporting
~288–320 B is a toolchain hop, not a go2cs regression; re-measure rather than re-diagnose.

### 7.1.5 §3.6 item 3 — sentinel liveness: **the mechanism works, and the lag bound has a boundary**

| regime | gen2 collections | sentinel firings | recorder `observed` | lag |
|:--|--:|--:|--:|--:|
| **negative control** — sentinel strongly referenced | 6 | **0** | 0 | — |
| promotion | 1 forced collection to first firing | 4 in 4 | — | — |
| **drained** — force, then `Drain()`, 40 cycles | 44 | 44 | **44** | **≤ 1 before the drain, 0 after** |
| **natural pressure** — 4.0 s, 28,819,062,784 B allocated, nothing forced | 54 | 54 | **54 (100.0 %)** | **0** |
| **artificial burst** — 8 forced collections back to back, no drain | 8 | — | **+2** | 8, and 6 after one drain |
| **artificial burst, sustained** — 3 × 60 forced collections with churn, no drain | 180 | — | 32–46 | **observation rate 17.8–25.6 %** |

The mechanism is sound: the resurrecting sentinel fires once per gen2 collection, promotion costs one
collection, and a strongly-referenced sentinel never fires at all — so the live readings are evidence
of the resurrection and not of some other wake-up.

**§3.2/§3.4's "lag by at most one collection" holds in both regimes a program actually meets** — the
drained one (which is what `runtime.GC()` and `debug.freeOSMemory()` produce, by §3.4's own
mitigation) and the natural one (which is what a program under load produces; 54 collections in four
seconds, every one seen). It fails only where gen2 collections are **forced back to back with no
drain**, because the finalizer thread never gets between two of them and `Observe()` advances by at
most one per call. That is a harness pattern, and the one harness pattern that matters —
`runtime.GC()` in a loop — is drained by construction.

So the boundary is: **`NumGC` is exact wherever the program either forces collections (drained) or
lets the runtime pace them (kept up with); it understates only under externally-forced back-to-back
gen2 collections.** Understating stays the safe direction, and no assert in §1.2's set is exposed
either way, because all nine compare the two surfaces to each other.

### 7.1.6 §3.6 item 4 — `GetGCMemoryInfo` fidelity: **confirmed, with one overload finding**

* **Pause fidelity is exact.** Across one forced blocking gen2: `GetTotalPauseDuration` delta =
  1,639,000 ns; `sum(FullBlocking.PauseDurations)` = 1,639,000 ns; **ratio 1.000**. Every run agrees to
  the last nanosecond. What §3.2 assumes about this API is true.
* **`PauseDurations.Length` is always 2.** For a blocking collection the second entry is 0; summing is
  therefore right and costs nothing.
* **§8.3's background-GC claim is CONFIRMED.** A background gen2, induced with allocation pressure:
  `index=101, generation=2, concurrent=True, PauseDurations=[2,000 ns, 149,000 ns]` — **two non-zero
  entries**, exactly as §8.3 states unverified. The ring's single number is their sum, and the header
  must say so.
* **`GC.Collect(2, Forced, blocking: false)` does NOT produce a background collection.** It advanced
  the gen2 index with `Concurrent=False` while `GCKind.Background` stayed at index 0. Only real
  allocation pressure produces one. (Recorded because it is the obvious way to try to induce one, and
  it does not work.)
* **⚠ THE OVERLOAD FINDING. §3.2 step 2 says "read the collection's facts from
  `GC.GetGCMemoryInfo(…)`" without naming an overload, and the default — `GCKind.Any` — is WRONG.**
  Measured: after a forced gen2 (`Any`: index 86, generation 2) followed by a single forced gen0
  collection, `Any` reports **index 87, generation 0**, while `FullBlocking` still reports index 86,
  generation 2. `Any` means "the latest collection of any kind", and ephemeral collections are
  frequent — so a recorder reading `Any` would write an **ephemeral collection's pause into a gen2
  ring slot** whenever one lands between the collection and the finalizer's `Observe()`. Neither
  single overload covers both cases (`FullBlocking` misses a background gen2, `Background` misses a
  blocking one). The rule the implementation needs: **read `Any` and accept it only when
  `info.Generation == GC.MaxGeneration`; otherwise take whichever of `FullBlocking` / `Background`
  carries the higher `Index`.** S2 owes that; this is a correction to §3.2, not a lane decision about
  it.

### 7.1.7 §4.5 / ⟨OQ-6⟩ — `TestFreeOSMemory`'s magnitude assertion: **it passes; the contingency is not needed**

Go's sequence, step for step, four times (the fourth harsher: two aggressive passes plus an explicit
LOH `CompactOnce`), with `HeapReleased` computed under §4.1's high-water formulation:

| round | committed before → after | `HeapReleased` delta | assertion 1 (`after > before`) | assertion 2 (`≥ 16,777,216 B`) |
|:--|:--|--:|:--|:--|
| 0 | 36,048,896 → 2,359,296 B | **33,689,600 B** | PASSES | **PASSES** |
| 1 | 35,987,456 → 2,359,296 B | **33,628,160 B** | PASSES | **PASSES** |
| 2 | 36,052,992 → 2,363,392 B | **33,689,600 B** | PASSES | **PASSES** |
| 3 (harsher) | 35,991,552 → 2,363,392 B | **33,628,160 B** | PASSES | **PASSES** |

The CLR returns **100.4 % of the 32 MiB dropped** — committed falls back to the settled baseline
(2.36 MB) every time — against a requirement of 16 MiB. Each round carries its own control: live bytes
(`GetTotalMemory(forceFullCollection: true)`) return to baseline, so the reading is about decommit
policy and not about a missed collection.

**`TestFreeOSMemory` therefore closes on BOTH assertions once `HeapReleased` is real.** ⟨OQ-6⟩'s
contingency does not arise, and the ruling's decision not to pre-rule it cost nothing.

### 7.1.8 ⟨OQ-2⟩ — the two formulations, measured rather than argued

After the four release/reacquire cycles above, with 32 MiB reacquired and live:

| formulation | `HeapReleased` |
|:--|--:|
| **§4.1 high-water**, `max(0, committedHighWater − currentCommitted)` | **0 B** — falls back to zero exactly as Go's documented field does when the heap reacquires |
| **Ruling B's literal** cumulative `TotalCommittedBytes` decrease | **134,635,520 B** — cannot fall, and now overstates by the entire amount |

The literal form drifts by **≈ 33.6 MB per release cycle** and is already off by 134 MB after four
cycles of a test that runs in 37 ms. §8.1's finding — "right exactly where it is tested and wrong
everywhere else" — is not a rhetorical point; it is this table. The ratified departure is the correct
one.

### 7.1.9 What these measurements change

Four readings refine or contradict text in this document. None contradicts a **ruling**; all four are
recorded for the coordinator rather than acted on by the measuring lane.

1. **§3.6 item 2's premise is wrong** — `ReadMemStats` allocates 288 B/call today (§7.1.4). §8.2's
   precondition becomes *work S2/S3 must do*, not a property to preserve. The guard is landed and
   pinned; the tightening to zero is future work.
2. **§5.1's "~5×" understates by an order of magnitude** — the measured multiplier against Go on this
   machine is **50.9×** (§7.1.2), because Go sits at 1.01× and not near the bound. The row needs ≈80 %
   of the converted path's allocation removed, which is a materially bigger ask than the sentence
   implies. Future work, owned by the ж-box arc.
3. **§3.2/§3.4's "at most one collection" needs its boundary stated** — exact under drained and under
   natural pacing, 17.8–25.6 % observation under externally-forced back-to-back collections (§7.1.5).
   The design's conclusion survives; the claim needs the qualifier.
4. **§3.2 step 2's unnamed `GetGCMemoryInfo` overload must be named** — the default is wrong and would
   write ephemeral pauses into gen2 slots (§7.1.6). S2 owes the `Generation == MaxGeneration` check.

Unchanged and now evidenced: ⟨OQ-1⟩ (overhead unmeasurable, §7.1.3), ⟨OQ-2⟩ (§7.1.8), ⟨OQ-3⟩ (does not
fire — root is (1), §7.1.2), ⟨OQ-6⟩ (contingency not needed, §7.1.7).

### 7.1.10 A measurement trap this stage paid for

The §4.5 probe first reported that **nothing** was released — 65,536 B against 32 MiB, in every round,
including the harsher variant — which reads exactly like a CLR decommit-policy finding and would have
sent ⟨OQ-6⟩ to a contingency it does not need. It was an artifact of the harness, not of the runtime:
**`GolibTests` builds Debug, and a Debug build reports every stack slot — including the temporary
holding `new byte[32 MiB]` on its way into the field — as live for the whole enclosing method.**
Clearing the field in the same frame therefore does not make the object unreachable, no collection
reclaims it, and the probe measures its own lifetime bug. Live bytes did not move across a forced
blocking compacting `Aggressive` collect (34,445,360 → 34,445,736 B); with the allocation moved behind
a call that has returned, they fall by the full 32 MiB and both assertions pass.

Go's own test has the shape the fix restores — `big = make(...)` then `big = nil` — because the Go
compiler does not extend a temporary's lifetime that way. **Any managed probe that drops a reference
and then measures must allocate in a frame that has already exited**, and should carry the
live-bytes control the probe now carries, which turns the artifact into a printed "THE READING ABOVE
IS INVALID" instead of a plausible number.

> **⚠ REFINED BY S2/S3 (2026-08-21): "a Debug build" is narrower than the phenomenon.** The same
> three-way probe — allocate INLINE into a static field vs. allocate BEHIND a call that has returned —
> was re-run under `-p:Optimize=true`, and again under `-p:Optimize=true` with
> `DOTNET_TieredCompilation=0`. All three configurations read **identically**: the inline shape retains
> the object (live 35,064,608 → 35,070,752 B; `HeapReleased` delta 5.48 MB against the 16 MiB the
> assertion needs), the behind-a-call shape reclaims it (live 35,062,624 → 1,520,144 B;
> `HeapReleased` delta **33,689,600 B**, §7.1.7's figure to the byte). Both a plain `byte[]` and a
> `slice<byte>` behave the same way, so no go2cs machinery is involved. This is therefore a property
> of the managed frame rather than of Debug codegen, and "build the test host Release" is not the
> remedy anyone should reach for. It is what blocks `TestFreeOSMemory`'s second assertion in the
> CONVERTED suite (§7.2.3), where the allocating frame IS the test's own and cannot be restructured.

---

## 7.2 S2/S3 — implemented and measured (2026-08-21)

> Same machine, same toolchain as §7.1: **i7-5820K** (6C/12T, 31.9 GB, Windows 11 10.0.26100), **.NET
> SDK 9.0.317 / runtime 9.0.19**, workstation concurrent GC, Go 1.23.1. Base commit `3659b9cdc`. No
> converter change, so no CNR is owed (charter §5 binds CNR to converter changes).

### 7.2.1 What landed, and where

| Design section | Landed as |
|:--|:--|
| §3.1–§3.3 the recorder, the sentinel mechanism, the always-on activation model | **`src/core/golib/runtime/GcPauseRecorder.cs`** (new) — the ring, `Arm`/`Observe`/`Drain`, the `GO2CS_GC_PAUSE_HISTORY=0` escape hatch, and `GcPauseSnapshot` |
| §3.2 write ordering, §7.1.6/refinement 4 the `Generation == MaxGeneration` overload rule | `GcPauseRecorder.Observe` / `TryReadGen2Info` |
| §3.3(a) armed from the package initializer | `runtime/managed_impl.cs` `ᴛArmGcPauseRecorder` **and** `runtime/debug/stubs_impl.cs` `ᴛArmGcPauseRecorder` — either assembly can be the first a program touches, and `Arm` is idempotent |
| §3.4 the `GC()` / `freeOSMemory()` drain | `runtime/managed_impl.cs` `GC()`; `runtime/debug/stubs_impl.cs` `freeOSMemory()` |
| §2/⟨OQ-5⟩ `NumGC`, `LastGC`, `PauseNs`, `PauseEnd`, `PauseTotalNs` | `runtime/managed_impl.cs` `ReadMemStats` |
| §3.2 `readGCStats` filling `2n+3`, most recent first | `runtime/debug/stubs_impl.cs` `readGCStats` |
| §4.1/⟨OQ-2⟩ `HeapReleased` high-water, with the two honesty notes as code comments | `GcPauseRecorder.SnapshotLocked` / `SampleCommitted` |
| §4.2 `NumForcedGC` | `GcPauseRecorder.NoteForcedGC`, called by both forced-cycle entry points |
| §4.3 the zero-rule fields, §4.4 the two unsatisfied invariants | stated in `managed_impl.cs`'s header; asserted by `GcPauseHistorySurfaceTests.RefusedFieldsStayZero` |
| §6.3/⟨OQ-4⟩ the `runtime/metrics` incoherence | recorded in `managed_impl.cs` above `readMetricsManaged`; **nothing in `runtime/metrics` was touched** |
| §3.6 item 2 / §8.2 the allocation guard | `GolibTests.GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation`, ceiling **tightened 320 → 0** |
| §3.5's acceptance list, as a repo-local guard | `src/tests/GolibTests/GcPauseHistorySurfaceTests.cs` (new, 5 cases) + a `core/runtime/debug` project reference |

### 7.2.2 `runtime/debug`: **2 → 3 of 9**, and `TestReadGCStats` closes on mechanism

Measured through the pipeline (`go2cs -tests -test-action all -test-timeout 15m`) on the same tree,
before and after:

| Test | Go | C# before | C# after |
|:--|:--|:--|:--|
| `TestReadGCStats` | pass | **fail** | **PASS** |
| `TestFreeOSMemory` | pass | fail | fail (§7.2.3) |
| `TestSetGCPercent` | skip | skip | skip |
| `TestSetMaxThreadsOvf` | pass | pass | pass |
| `TestSetCrashOutput` | pass | fail | fail — not this arc |
| `TestStack` | pass | fail | fail — the position-map arc |
| `TestWriteHeapDump{Nonempty,Finalizers,TypeName}` | pass | fail | fail — Ruling B's `runtime-capability` disclosure, pinned AS FAILING |

All nine of §1.2's assertions hold for `TestReadGCStats`, including the ones that were never reached
before (5, 9) and the ones that passed vacuously (3, 6, 7). Nothing was tuned to make them: both
surfaces read one snapshot under one lock, and the ring's write ordering is the inverse of
`ReadGCStats`' backwards walk. §8.3's one genuine race — a gen2 collection landing between
`ReadGCStats` and `ReadMemStats`, which Go's test closes with `SetGCPercent(-1)` and this tree cannot —
did not fire in the measured runs; it is not claimed to be impossible.

### 7.2.3 `TestFreeOSMemory`: assertion 1 closes, assertion 2 is blocked OUTSIDE this arc

The converted row reports `less than 16777216 released: 0 -> 3039232`. Assertion 1 (`after > before`)
**passes**. Assertion 2 fails, and the cause is not `HeapReleased`.

A three-way probe in plain hand-written C# (GolibTests, i.e. no converted code and no go2cs machinery
in the allocation path) settles it — same 32 MiB object, same `FreeOSMemory`, three frame shapes:

| shape | live before → after | `HeapReleased` delta | verdict |
|:--|:--|--:|:--|
| allocate INLINE in the measuring frame, `slice<byte>` (the converted test's exact shape) | 35,064,608 → 35,070,752 B | 5,484,544 B | object **not reclaimed** |
| allocate INLINE in the measuring frame, plain `byte[]` | 35,061,560 → 35,073,536 B | 61,440 B | object **not reclaimed** |
| allocate BEHIND A CALL that has returned | 35,062,624 → **1,520,144 B** | **33,689,600 B** | object **reclaimed** |

The third row is §7.1.7's figure to the byte, so `HeapReleased` is exactly right and the CLR does hand
back 100 % of the 32 MiB — when the object is genuinely unreachable. It is not, in the converted test,
because the test method's own frame keeps it alive. Re-running all three under `-p:Optimize=true`, and
again under `-p:Optimize=true` with `DOTNET_TieredCompilation=0`, changes **nothing** — so §7.1.10's
"a Debug build" attribution is narrower than the phenomenon, and building the test host Release is not
the remedy.

**This is a converted-program object-lifetime divergence, not a measurement-surface one**, and it is
general: a Go program that drops its last reference to a large object and measures memory *in the same
function* sees it retained under go2cs, where Go's compiler does not extend the temporary's lifetime.
It is recorded here and routed out of this arc; nothing in §3 or §4 can move it.

### 7.2.4 The allocation guard, at ZERO

§8.2's landing precondition, which S1 measured as already violated (288 B/call), is now met:

| measurement | S1 (2026-08-21, pre-implementation) | S2/S3 |
|:--|--:|--:|
| `ReadMemStats` per call, `GetAllocatedBytesForCurrentThread` over 2,000 calls | 288.0 B | **0.0 B** |
| 200 empty bracketed windows, `TotalAlloc` delta, mean | 244.7 / 285.7 / 326.7 B | **0.0 B** |
| worst single window | 8,200 B | **0 B** |
| windows non-zero, of 200 | 6–8 | **0** |
| worst window as a share of `net/textproto`'s 32,768 B budget | 25.02 % | **0.000 %** |

The remedy is §7.1.4's own: `GC.GetGCMemoryInfo()` allocates a `GCMemoryInfoData` box per call, so it
left the read path entirely — `ReadMemStats` reuses the committed/heap-size figures the recorder
already samples once per gen2 collection, and the ring is copied into the caller's already-allocated
`array<T>` backing. The direct read survives only as a fallback for the two cases that have no recorder
sample: before the first observed collection, and under `GO2CS_GC_PAUSE_HISTORY=0` — where it restores
the pre-recorder behavior exactly, which is the point of an escape hatch. The cost is freshness:
`TotalCommittedBytes` was a snapshot as of the last GC of any kind and is now as of the last observed
**gen2** collection. It is therefore fresh at every point a Go test reads it, because `runtime.GC()` and
`debug.FreeOSMemory()` both drain the recorder before returning.

### 7.2.5 Gates

| Gate | Result |
|:--|:--|
| `GolibTests` (full) | **196/196** — 191 at S1 plus the five new `GcPauseHistorySurfaceTests` cases; the allocation guard passes at its new **0 B** ceiling |
| `dotnet build src/go2cs.slnx -c Debug --no-incremental -m -p:UseSharedCompilation=false` | **0 errors**, 339 warnings, **3,546 s** (722 registered projects) |
| Behavioral suite (`run-behavioral.ps1`, all four phases) | **PASS** — 603/603 Transpile, Compile and Target; 577/577 Output (26 skipped, no `package main`); **6,551.7 s**. Zero golden drift, as expected from a change that touches no converter code |
| Validated sweep — the §1.3/§6.4 census re-derived at gate time by `git grep` over `src/core`, kept to the rows that are actually BANKED | `net/textproto` **26**, `image/gif` **28**, `expvar` **11**, `sync` **44**, `time` **159**, `runtime/metrics` **2** — every one at its exact banked count (plus `runtime/internal/math` 1 and `runtime/internal/sys` 4, swept incidentally by the `time` substring filter). `math/big`, `runtime/pprof` and `runtime/debug` are the census's other consumers and are **not on the roster**, so they carry no banked count to protect; `runtime/debug` is measured through the pipeline instead |
| `runtime/debug` pipeline | 2 → **3 of 9**, run twice with the same verdicts (§8.3's race did not fire in either) |
| CNR | **not owed** — no converter change |

Two figures worth carrying: the full-solution build is now **~3,550 s** on this machine class, against
CLAUDE.md's 2026-08-07 row of 1,432 s at 573 projects (the tree is at 722), and the behavioral suite is
**~6,550 s** against a 2,820–4,131 s row at 555 packages (it is at 603). Both rows are stale rather
than the runs being slow.

Post-gate `git status` carried the four sweep dirt classes CLAUDE.md catalogues and nothing else:
CRLF phantoms (`expvar_test.cs`, `net/textproto/reader{,_test}.cs`, `runtime/metrics/doc.cs`), the
`-tests`-closure production re-flips (`runtime/metrics/description.cs`'s `global::go.*` root escape;
`time/package_init.cs`'s `initᴛᴛtests()` hook), a `.cs.auto` review sibling (`time/tick.cs.auto`), and
`time`'s banked test sources drifting against a converter improvement that predates this arc
(anonymous-`dyn`-struct dedupe — `time_test.cs`, `package_test_info.cs`; no converter code was
touched here, so it cannot be this arc's). All restored; nothing banked.

---

## 8. Adversarial self-review (charter §7)

Three lenses, run against this document's own first draft. Each records what the attack **found** and
what the design does about it.

### 8.1 Correctness — can any field lie?

**Found: yes — the commissioning ruling's own phrasing produces a lying field.** A *"cumulative
`TotalCommittedBytes` decrease"* is monotone; Go's `HeapReleased` is a current quantity that falls when
the heap reacquires memory. Implemented literally, the field would pass `TestFreeOSMemory` and then
drift arbitrarily far above the truth in any long-running program — a number that is right exactly
where it is tested and wrong everywhere else, which is the worst shape a measurement can have.

*Answer:* §4.1's high-water formulation, which decreases exactly when Go's decreases, satisfies the
same test for a better reason, and is escalated as ⟨OQ-2⟩ precisely because it departs from the
ruling's wording.

Two secondary findings from the same lens, both answered by stating rather than repairing:
`Sys == Σ(breakdown)` is already false and `HeapIdle >= HeapReleased` becomes at-risk (§4.4) — and
the temptation in each case is to invent a breakdown or clamp a measured number, which §4.3's rule
forbids. Third: `NumGC` from the recorder **understates** by up to one collection (§3.4). Understating
is the safe direction and is closed at the one boundary that matters; the alternative — seeding
`observed` from `CollectionCount` and padding the ring — would fabricate pause entries, and is
refused.

### 8.2 Cost — what does every converted program pay?

**Found: the always-on worry is aimed at the wrong cost, and the real cost is elsewhere and sharper.**
The recorder's own price is O(1) per **gen2 collection** — a rate orders of magnitude below the
per-allocation branch the corpus already accepted for `AllocationCounter` at 11–15 % when enabled
(r58a). But `ReadMemStats` itself gets more expensive, and one banked row measures **across
`ReadMemStats` calls**: `net/textproto`'s `TestReadMIMEHeaderAllocations` brackets each header read
between two `ReadMemStats` calls and asserts under 32,768 B per iteration. Any allocation `ReadMemStats`
performs after capturing `TotalAlloc` in the first call — or before capturing it in the second —
lands **inside** the measured window and is charged to `ReadMIMEHeader`. A recorder that copied its
ring into a fresh array per read, or a snapshot object allocated per call, would move a banked,
currently-passing row and it would look like a `net/textproto` regression.

*Answer:* **allocation-free reads are a landing precondition, not an aspiration** — §3.6 item 2, with
a `GolibTests` guard that asserts `GetAllocatedBytesForCurrentThread()` does not move across a
`ReadMemStats` loop. The design supports it (fixed ring storage, `array<T>`'s `ref` indexer writing
into the caller's already-allocated backing, `GetGCMemoryInfo` returning a struct) but the guard is
what makes it true. A related discipline falls out: the ring copy writes only the `min(observed, 256)`
slots that exist, leaving the rest zero exactly as Go does, so the per-read cost is proportional to
observed collections and capped.

Third cost, named for completeness: the sentinel keeps one entry permanently cycling through the
finalization queue. Small, unmeasured, and §3.6 item 1 measures it before landing.

### 8.3 Flakiness — which asserts become timing-dependent?

**Found: the design makes eight of nine assertions flake-proof by construction and leaves exactly one
genuine race — and the race exists because the knob Go's test uses to close it is inert here.**
`TestReadGCStats` calls `ReadGCStats(&stats)` and then `runtime.ReadMemStats(&mstats)` and compares
values across the two calls, on the stated assumption *"no GC during ReadGCStats"*. Go earns that
assumption with `defer SetGCPercent(SetGCPercent(-1))` at the top of the test — GC is **off** for its
duration. In this tree `setGCPercent` is a remembered value with no effect on collection (documented
honestly in `stubs_impl.cs`), so a gen2 collection landing between the two calls would move `observed`
and split the two reads.

*Answer, in three parts.* (i) The window is microseconds and is entered immediately after
`runtime.GC()` has forced two full compacting collections, so the heap is as far from the next gen2
trigger as it ever gets — the probability is low but non-zero and is **not** claimed to be zero.
(ii) The snapshot is taken under one lock over fixed storage, so a torn or half-updated read is
impossible; only a genuine collection between the two *calls* can split them. (iii) The tempting
"fix" — making `SetGCPercent(-1)` enter `GC.TryStartNoGCRegion` — is **refused**: the
save-and-restore idiom `defer SetGCPercent(SetGCPercent(-1))` appears throughout the corpus's test
suites, so that change would put arbitrary test bodies into a no-GC region (and would throw when the
budget cannot be reserved), a corpus-wide behavioral change bought for one assertion. If the row does
prove flaky, the response is to **measure the flake rate over N runs and record it** — never to
disclose a race this arc introduced.

Two further flakiness findings: `TestFreeOSMemory`'s magnitude assertion depends on a CLR decommit
policy nobody has measured (§4.5, ⟨OQ-6⟩); and a **background** gen2 collection reports two entries in
`PauseDurations` rather than one stop-the-world pause, so the ring's single number is their sum — a
stated approximation, recorded in the header, not a hidden one.

---

## 9. Open questions

Each carries this lane's recommendation. None is self-ruled.

* **⟨OQ-1⟩ — Recorder activation model** (§3.3). *Recommendation:* **always on, armed from `runtime`'s
  package initializer**, with a `GO2CS_GC_PAUSE_HISTORY=0` opt-out and the §3.6 overhead measurement
  as a landing precondition. First-read arming is rejected on correctness (it makes `NumGC` less true
  than today's); test-host-only is rejected because a measurement surface must not answer differently
  under test than in production, and this one has a production consumer (`expvar`).
* **⟨OQ-2⟩ — `HeapReleased` formulation** (§4.1). *Recommendation:* **`max(0, committedHighWater −
  currentCommitted)`**, not the ruling's literal monotone cumulative decrease. It matches Go's
  documented semantics (it can go down), satisfies the same test, and needs ratification only because
  it departs from the commissioning text.
* **⟨OQ-3⟩ — If the discriminator finds root (2)** (§5.5). *Recommendation:* **reopen scheduler OQ8**
  — a `GOMAXPROCS` runnable limiter was deferred for want of a consuming suite and would then have
  one — rather than mint a disclosure. `runtime-capability` refuses the row by its own admission test,
  and `TotalAlloc` must not be redefined to make a test pass (§5.3).
* **⟨OQ-4⟩ — One measurement, one source of truth?** (§6.3). Should `runtime/metrics`'
  `/memory/classes/heap/released:bytes`, `/gc/pauses:seconds` and `/gc/cycles/total:gc-cycles` be
  rewired to the recorder in this arc? *Recommendation:* **no — record the divergence in both headers
  and on the board, defer the wiring until a consumer demands it.** It converts auto-converted compute
  closures into hand-owns on a banked package for no consuming test. The cost of deferring is a stated
  incoherence, which is cheaper than an unstated one.
  > ⚠ **Amended 2026-09-28 (ledger 96edeb700a): the trigger has FIRED.** runtime's `TestReadMetrics`
  > (`0 < /gc/heap/live:bytes <= HeapSys`) is the consuming test, and metrics exporters the real-world
  > one, so the wiring is owed. Its "live bytes is 0" line is disclosed DEFERRED until then. The design:
  > [`DESIGN-oq4-heap-accounting-snapshot.md`](DESIGN-oq4-heap-accounting-snapshot.md).
* **⟨OQ-5⟩ — `PauseTotalNs` redefinition** (§2, §4.2). *Recommendation:* **the ring's gen2 running
  sum**, replacing today's all-generation `GC.GetTotalPauseDuration()`. It follows from the one
  definition of a Go cycle; it makes an existing answer smaller; nothing banked reads it (`sync`'s
  only sites are benchmarks).
* **⟨OQ-6⟩ — Contingency if `HeapReleased` becomes truthful and `TestFreeOSMemory`'s ≥ 16 MiB
  assertion still fails** (§4.5). *Recommendation:* **do not pre-rule.** The probe is ~20 lines and
  runs in S1; rule on the number. Pre-ruling would be inventing a class for a case that may not exist.
