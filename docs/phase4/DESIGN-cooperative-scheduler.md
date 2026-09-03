# The cooperative scheduler — owning goroutine capacity instead of renting the ThreadPool's heuristics

> **STATUS: RATIFIED AND LANDED (coordinator; chartered 2026-08-13 at `0b8287f07` — all seventeen
> OQs ruled, recommendations ratified — formerly PROPOSED 2026-08-12, lane scheduler-design).
> SCHED-S1 LANDED at `4f06d78ae` (2026-08-13): goroutines get their own threads — the runtime owns
> capacity, and the pool floor retires.
> **SCHED-S3 LANDED 2026-09-03 (dated amendment): park ACCOUNTING, §5.3 adopted verbatim.** golib's
> `Goroutine` carries a `WaitReason` (Go's own `waitReasonStrings`, only the members a go2cs park site
> can set), `Goroutine.Park(reason)` is the §5.3 scope, and §6 rows 1-7 plus **row 9** (the netpoll
> `Monitor.Wait`, whose `IO wait` the netpoll arc was left to adopt at its option) wrap it — protocols
> untouched, one volatile store each way, no allocation. `runtime.Stack(all: true)` stops ignoring
> `all`: it enumerates the registry in goid order with a truthful `goroutine N [<reason>]:` header per
> goroutine. What did NOT land, and is still Stage B: any OTHER goroutine's FRAMES — those blocks
> carry a one-line placeholder, because the CLR has no supported cross-thread stack walk and the
> capture-at-park-time route needs the ruled synthetic-PC registry to symbolize it. Also not adopted:
> §6 row 10 (the testing parallel gate — accounting on a host gate with no consumer), and rows 11-12,
> which the table already rules `never`. `GoroutineState` becomes DERIVED from the reason rather than
> stored, so "parked with no reason" and "running with a reason" cease to be representable. The
> `NumGoroutine` half of the S3 row landed earlier, with the Coro arc.
> Guards: behavioral `GoroutineWaitState`, `GolibTests.GoroutineParkAccountingTests`.** Consequence for reading this document (dated amendment
> 2026-08-29, from the owner's staleness report): §1 and §2 describe the PRE-LANDING launch path —
> `ThreadPool.QueueUserWorkItem` and the `max(4×cores, 256)` min-thread floor — as the measured
> bill this design was written against; both are RETIRED. The current source is
> `Goroutine.cs:202-209` (`new Thread(() => Run(body), s_stackReserve)`, one dedicated thread per
> goroutine; `Goroutine.cs:26` records the history), and `builtin.cs:73` deliberately carries NO
> `SetMinThreads` floor any more. The 28.7-minute singleflight ladder in §1 is the HISTORICAL
> pathology that motivated the fix, not a live one — any plan citing this document's §1/§2 in the
> present tense (the trap `PLAN-cgo-interop.md`'s blocking-call section fell into) must re-derive
> against current source. Commissioned by the singleflight
> convergence measurement (branch `claude/singleflight-convergence`, board append at `e6f331cd2`):
> *"the row is ultimately a scheduler-arc row, and any floor is a bridge across it, not a fix for
> it."* Companions: `src/core/golib/builtin.cs` (the thread floor and its own self-indictment),
> `src/core/golib/runtime/Goroutine.cs` (the goroutine root this design rebuilds beneath),
> `src/core/golib/channel.cs`, `src/core/sync/runtime_impl.cs`,
> `src/core/internal/poll/runtime_sema_impl.cs`, `src/core/time/time_impl.cs` (the park sites),
> [`DESIGN-channels.md`](DESIGN-channels.md) §4 (the ratified pool-thread divergence this design
> exists to retire), [`DESIGN-goexit.md`](DESIGN-goexit.md) (the goroutine root's history, and the
> Option-A registry this design supplies), and `DESIGN-netpoll-managed-poller.md` (branch
> `claude/netpoll-design`, `0cfed1364`) §3.2/§8 — the adjacent arc whose shared boundary §7
> settles. Written against the corpus at `0133b6aa7` (2026-08-12), Go 1.23.1.

---

## 1. The bill, measured — the singleflight ladder is witness #1

`internal/singleflight` sits one row from a bank: 4 of 5 verdicts match and
`TestDoAndForgetUnsharedRace` consumes the 30-minute package deadline verdict-less
([`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md), "`internal/singleflight`
— 4 of 5"). The convergence question the board deliberately left open was spent on 2026-08-12
(branch `claude/singleflight-convergence`, laptop lane, solo, go1.23.1 — an instrumented run whose
aftermath was fully reverted). **It converges: iteration 20 (d=524s), test elapsed 1720.8s, package
wall ≈ 1725s — a 75-second margin under the 30-minute deadline.** `go test` runs the same package in
**0.040s** (the race test itself 0.01s; Go converges on its FIRST iteration). The gap is ~10⁵, and
`shared` was 0 at the converging iteration: the 28.7 minutes is all scheduling, none of it
singleflight.

The test (`singleflight_test.go:145`) launches n=1000 goroutines per iteration, each calling `g.Do`
on one key whose function sleeps `d`; if `calls != 1` — the goroutines did not all arrive inside
`g.Do` before the first call completed — it doubles `d` and retries. The measured census:

| iter | d | calls | pool start→end | wait |
|--:|--:|--:|:--|--:|
| 1-3 | 1-4ms | 20, 8, 6 | 12→258 | ~0.02s |
| 4-10 | 8-512ms | 4-9 | 258→258, flat | ~4×d |
| 11-15 | 1-16.4s | 4-7 | 258→354 (~+0.9/s) | ~3-4×d |
| 16 | 32.8s | 4 | 354→**162** | 98s |
| 17 | 65.5s | 3 | 162→221 | 197s |
| 18 | 131s | 2 | 221→567 | 262s (=2×d) |
| 19 | 262s | 2 | 567→**75** | 524s (=2×d) |
| 20 | 524s | **1** | **75→1002** | 524s (=1×d) |

Three mechanisms own the table, and the branch's headline names what none of them is: **the ladder
is pool heuristics, not parking latency.**

1. **The goroutines that miss the window never parked slowly; they never STARTED.** `spawn_s=0.00`
   every iteration — queueing 1000 work items is instant — and `calls ≈ ceil(1000 / live pool)`:
   the queue drains in waves of pool size, each post-wave batch dispatching only after the previous
   call completed, which by construction mints a fresh call. `wait ≈ calls × d` throughout, so no
   value of `d` helps while the pool is small. The tail sits in the pool QUEUE, not in `wg.Wait`.
2. **Iterations 1-10 are pinned at golib's own floor.** `Goroutine.Start` is
   `ThreadPool.QueueUserWorkItem` (`golib/runtime/Goroutine.cs:64`); the min-thread floor of
   max(4×cores, 256) (`golib/builtin.cs:77-78`) is why the pool leaps 12→258 in three iterations
   and then sits exactly there. Below the floor, creation is on demand; above it, only the
   starvation gate injects (~0.9-1.8 threads/s, and an iteration must hold starvation ≥ ~1s
   continuously to trip it — growth begins only at `d ≥ 1s`).
3. **Idle-thread retirement FIGHTS the injection, so capacity cannot accumulate across
   iterations.** Iteration 16 ends 192 threads below its start; iteration 19 ends at 75: once `d`
   exceeds the pool's ~20s idle timeout, every thread left idle through the final wave's sleep is
   culled. Convergence arrives only when a SINGLE `d` is long enough for in-sleep injection alone
   to field all 1000 — iteration 20 starts at 75 live and injects ~930 during one 524s sleep.

The remedy fork the append prices: a `$longTimeouts` floor of 60m banks the row as measured but is
*"the first deadline asked to cover a heuristic rather than work"* — the finish is a race against
injection-vs-retirement with 2× penalty steps, one slipped rung moves 28.7 min to ~55 min, and the
floor adds ~29-55 min to every full sweep for one row. *"The durable path is the one golib already
names"* — and that path is this document.

### 1.1 The rest of the bill — standing divergences and forecast walls, not yet suite rows

The board was scanned for further rows attributable to goroutine parking/scheduling. Singleflight
is the only **measured** suite row. The rest of the bill is standing divergence notes and walls
that other arcs have already hit and routed around — each one a consumer of exactly the machinery
this design proposes:

| Witness | What it says today |
|---|---|
| `golib/builtin.cs:68-78` | The floor calls itself *"a mitigation, not a scheduler: programs parking thousands of goroutines remain out of reach until a cooperative scheduler exists (documented divergence)"*. n=1000 sits exactly on that line; the §1 table is the divergence's first quantified witness. |
| [`DESIGN-channels.md`](DESIGN-channels.md) §4, §5(3) | *"Blocked goroutines hold pool threads… Programs with thousands of blocked goroutines remain out of reach until a cooperative scheduler exists (explicitly out of scope here)"* — ratified at the wave3 integration as the standing position on goroutine scheduling. This design is the successor that position was waiting for. |
| [`DESIGN-channels.md`](DESIGN-channels.md) §4 (deadlock) | *"a genuinely deadlocked all-real-channel program now parks forever"* where Go prints `fatal error: all goroutines are asleep - deadlock!` and exits 2. Detection needs a goroutine registry plus park accounting — §5.4's follow-up. |
| `DESIGN-netpoll-managed-poller.md` §3.2 (branch) | The deep wall behind through-the-runtime netpoll wiring: *"there is no g to park, no P to hand a ready g to, and no sysmon to pump `netpoll(delta)`… Emulating that means writing a scheduler."* That arc cut around the wall at the ten-contract boundary; §7 settles which side of the seam this design owns. |
| [`DESIGN-goexit.md`](DESIGN-goexit.md) §3/§5 | Faithful main-goroutine Goexit (Option A) is deferred on *"a live-goroutine registry"*; *"the `OnGoroutine` flag is the hook a live-goroutine registry would build on."* §5.1 is that registry. |
| `runtime/lock_managed_impl.cs:35-38` | *"getg() is a Go compiler intrinsic with no managed realization yet (a `[ThreadStatic]` g/m model is the future root that unlocks it)"* — the hand-owned runtime lock protocol is already waiting on the identity object §5.1 mints. |
| `runtime/debug.cs:44-47` | `runtime.NumGoroutine()` returns `gcount()`, read from the converted-dead scheduler's bookkeeping — nothing ever populates it. A registry gives it a real answer. |
| `testing/TestExecution.cs:34-41, 119-124` | The test host already made this design's move for its OWN threads: each test gets a dedicated 256MB-reserve thread *"rather than a pool work item"* because *"dozens-to-hundreds of parked thread-pool threads would starve the pool (injection is ~1 thread/s) — stalling both the suite and any converted goroutines the tests spawn."* The host dodged the ladder per-test; goroutines are still in it. |

Scanned and deliberately **not** claimed: `ForVariants` (board, DELIBERATE-SKIP) is Go-side
scheduling nondeterminism, not parking cost; `hash/maphash`'s 15-min-vs-7.6s is compute on a slow
host (CLAUDE.md's own attribution); the `unique` regression is unattributed and owned by its bisect
lane. This design's bill is the singleflight ladder plus the table above — nothing else is asserted.

## 2. What exists today — the goroutine machinery census

**The launch path.** Go's `go f(a, b)` emits `goǃ(f, a, b)` — 34 arity overloads
(`golib/builtin.GoroutineLaunchers.cs`), every one a one-line dispatch into `Goroutine.Start`
(`golib/runtime/Goroutine.cs:62-65`), which is `ThreadPool.QueueUserWorkItem(_ => Run(body))`. The
root policy lives once, in `Run` (`Goroutine.cs:92-121`): mark the thread as a goroutine, run the
body, swallow `GoexitException` (the r16 Goexit shape), offer non-panic escapes to a host
containment policy, and let a `PanicException` reach golib's AppDomain backstop (report Go-style,
exit 2). The r16 funnel — *"the root's policy is stated exactly once"* — is what makes this design
cheap: **the executor is one method's body.**

**Goroutine state is thread-affine, everywhere.** The body of a goroutine runs start-to-finish on
one thread, synchronously, and golib's per-goroutine state rides thread-local storage on that fact:

| State | Storage | Where |
|---|---|---|
| "am I a goroutine" (the Goexit gate) | `[ThreadStatic] t_onGoroutine` | `Goroutine.cs:24-25` |
| captured/handled panic (`recover()`, tracebacks) | `ThreadLocal<PanicException>` ×2 | `GoFuncRoot.cs:20-26` |
| defer frames | `GoFrame` — a **`ref struct`** in the caller's own stack frame | `GoFrame.cs:54` |
| `sync.Pool` shard id (`procPin`) | `[ThreadStatic] t_procId` | `sync/runtime_impl.cs:272-289` |
| high-resolution sleep timer | `[ThreadStatic] t_highResTimer` | `time/time_impl.cs:916-920` |
| allocation counting (`AllocsPerRun` disclosures) | `GC.GetAllocatedBytesForCurrentThread` + `[ThreadStatic]` | `golib/AllocationCounter.cs:72-101` |
| test attribution (which test started this goroutine) | `AsyncLocal<TestExecution>` flowing via ExecutionContext capture | `testing/TestExecution.cs:64-67` |

The last row is the one that flows ACROSS the launch rather than living on one side of it:
`QueueUserWorkItem` captures the ExecutionContext, which is how a failure inside a goroutine is
attributed to the test that spawned it. Any replacement executor must preserve that flow (§5.2's
invariant list; `Thread.Start` captures the ExecutionContext the same way).

**The park census.** Every blocking Go primitive is realized as a synchronous block of the calling
thread on a managed wait primitive — correct in isolation, and the protocols are battle-hardened
(the wave3 adversarial rounds, the ticketed notify list, the seeded semaphore). What no layer owns
is the CAPACITY consequence: each park holds a **shared-pool** thread, and the pool's replacement
heuristics are the §1 ladder. The full inventory is §6; the shape of it: channels and select park
on per-waiter `SemaphoreSlim(0,1)` (`channel.cs:150, :125`), the sync and internal/poll runtime
semaphores on per-waiter `ManualResetEventSlim` FIFO queues (`sync/runtime_impl.cs`,
`internal/poll/runtime_sema_impl.cs`), `time.Sleep` on a per-thread high-resolution waitable timer
(`time_impl.cs:454-471`), and the timer engine itself is already a **dedicated background thread**
(`time_impl.cs:684`) — as are the test host's per-test tRunner goroutines
(`TestExecution.cs:119-130`). Dedicated threads for things that park is not a new idea in this
tree; it is the established dodge, applied twice, that has not yet reached the one place every
`go` statement lands.

**The public runtime surface.** `GOMAXPROCS` is a remembered value that does not cap parallelism
(`runtime/managed_impl.cs:27-31, 85-98` — *"a goroutine is a managed thread and the CLR schedules
it"*, an honest divergence stated once); `Gosched()` is `Thread.Yield()` (`managed_impl.cs:100-108`);
`NumGoroutine()` answers from dead bookkeeping (§1.1). Nothing in this design changes the first
two; the third becomes real.

## 3. Goals and non-goals — what "cooperative" means here

Go's scheduler is an M:N multiplexer: goroutines are stackful coroutines, `gopark` switches a g off
its M, `goready` queues it to a P, and the runtime owns every blocking point. None of that
transplants: the CLR offers no stack switching (§4's M4/M5 price the two escapes), so **a go2cs
goroutine is, and remains, a thread for its whole life.** "Cooperative" here therefore does not
mean coroutines or yield points. It means the two things Go's runtime does that the current model
delegates to a foreign heuristic:

- **The runtime, not the host pool, owns execution capacity.** A parked goroutine must never delay
  a not-yet-started one. Go delivers this by multiplexing; a thread-affine runtime delivers it by
  giving every goroutine its own schedulable unit and letting the OS preempt — capacity equals
  demand by construction, and the §1 ladder's three mechanisms (queue waves, floor, injection-vs-
  retirement) structurally cannot occur because there is no queue, no floor, and no inference.
- **Blocking is declared, not hidden.** Go's parks all funnel through `gopark(reason)`; the managed
  mirror is a park ACCOUNTING contract (§5.3) wrapped around the existing wait primitives, so the
  runtime can answer "how many goroutines exist, how many are parked, and why" — the fact every
  §1.1 consumer (deadlock fatal, NumGoroutine, Goexit A, netpoll accounting, getg) needs.

**Goals.**

1. Kill the ladder: goroutine launch-to-running latency independent of how many goroutines are
   parked. Acceptance gate: `TestDoAndForgetUnsharedRace` converges in seconds, not rungs (§9).
2. Goroutine identity as a first-class runtime object with a live registry — the `[ThreadStatic]`
   g-model `lock_managed_impl.cs` forecasts, the registry `DESIGN-goexit.md` Option A waits on.
3. The `gopark`/`goready` contract as ONE seam for park accounting, adopted by the in-tree park
   families without relocating their proven wait protocols.
4. **Zero converter change, zero emission change.** Everything lands behind `Goroutine.Start` and
   golib/hand-own internals; CNR stays byte-identical; no golden re-baselines beyond the new
   guard's own project.
5. Preserve every thread-affinity invariant in §2's table, by construction.

**Non-goals.**

- **No Go-scale goroutine counts.** OS threads bound this design at roughly 10⁴ concurrent
  goroutines (§4's M1 pricing); 10⁵-10⁶ stays out of reach and MOVES the documented divergence
  from "thousands park badly" to "tens of thousands park fine; beyond that is out of scope." The
  only known path past the bound is async lowering, respected-and-rejected in §8.
- **No run queues, no GOMAXPROCS parallelism capping, no preemption machinery.** The OS scheduler
  is the dispatcher; `GOMAXPROCS` keeps its documented get/set-only divergence; `Gosched` stays
  `Thread.Yield`.
- **No scheduler-facing netpoll surface.** `netpoll(delta)`/`netpollBreak`/`netpollready` stay
  unimplemented and unpumped; the netpoll arc's ten-contract cut stands (§7).
- **No performance parity claims against Go.** Goroutine creation will remain ~2-3 orders costlier
  than Go's `newproc` (§4). Correctness-first per Phase-4 doctrine; the perf suite gates only
  against regressing today's C# numbers.
- **What stays on the ThreadPool: everything that is not a goroutine.** BCL internals, `Task`
  continuations (the test host's `TaskCompletionSource` plumbing), finalizers, any
  `System.Threading.Timer` callbacks in hand-owns. The pool returns to being .NET's utility, with
  no Go parking on it — which is what lets the floor retire (§5.2).

## 4. The mechanism space, priced honestly

**M0 — status quo plus bridges (floors, per-package deadlines).** Priced by the board append
itself: a 60m `$longTimeouts` floor banks singleflight as measured, but it is a deadline covering a
heuristic, quantized at 2× per slipped rung, and it taxes every full sweep ~29-55 min for one row.
The min-thread floor's structural limits are §1's mechanisms 2 and 3: it pre-warms injection only
up to its own value, it is process-global (it distorts the host, the timer callbacks, and every
non-goroutine pool user), and retirement unwinds whatever injection wins. Bridges stay available;
none of them is the fix.

**M1 — a dedicated thread per goroutine (RECOMMENDED).** `Goroutine.Start` creates a background
thread per launch instead of queueing a work item.

- *What it buys.* The ladder dies by construction: all 1000 singleflight goroutines are RUNNING
  within the spawn window, so `calls` collapses to ~1 as soon as `d` exceeds that window —
  convergence in low seconds (estimate; the S2 measurement is the number). Zero contract
  migration: every existing park primitive already blocks a thread correctly, and under M1 that
  thread is nobody else's capacity. Every §2 thread-affinity invariant holds by construction —
  the body still owns one thread for its whole life. The two in-tree precedents (test threads,
  timer thread) become the general rule instead of local dodges.
- *What it costs.* Estimates to be measured at S1, stated as such: thread creation on Windows is
  on the order of 10-100 µs (vs ~1 µs to queue a work item, ~10²-10³ ns for Go's `newproc`) — the
  singleflight shape spawns 20×1000 threads ≈ 1-2 s of total creation overhead against a test that
  currently takes 28.7 minutes. A live thread costs tens of KB of committed memory (TEB + kernel
  stack) plus its stack RESERVE, which is address space, not memory (the test host already
  reserves 256MB per test thread on this reasoning, `TestExecution.cs:57-62`). 10⁴ concurrent
  goroutines ≈ hundreds of MB commit and a workable OS scheduler load; 10⁵ is marginal; 10⁶ is
  not real — that is the §3 bound.
- *What it cannot deliver.* Go-scale counts (above), sub-µs `go` statements, and Go's
  GOMAXPROCS-bounded CPU discipline: N CPU-bound goroutines are N runnable threads the OS
  time-slices, where Go would run GOMAXPROCS and queue the rest. That is a *continuation* of the
  existing documented divergence (`managed_impl.cs:28-31`), not a new one — the pool never
  enforced GOMAXPROCS either — but oversubscription pressure at high runnable counts is real and
  is the one workload class where M2 would beat M1.

**M2 — an owned pooled executor with park-driven injection.** Go's own M-spawning shape: a worker
pool whose size the runtime adjusts the instant a worker parks (the §5.3 contract is the signal —
no inference, no 1s starvation gate, no idle culling of needed capacity).

- *What it buys over M1.* Thread reuse for spawn-heavy programs (amortizes creation), a bounded
  idle-thread count, and a natural home for future policy.
- *What it costs.* The machinery M1 doesn't need: worker lifecycle, a run queue, an idle-retirement
  policy — a reimplementation of the pool minus its heuristics. And it carries a structural hole:
  a block the contract does NOT see (any BCL wait or blocking P/Invoke inside converted code — the
  10 ms blocking `ReadFile` in `os` is already real) stalls a worker invisibly, resurrecting a
  smaller ladder. Go plugs the same hole with sysmon retaking Ps from long syscalls — i.e., a
  watchdog, more machinery still. Under the singleflight shape M2 needs a thread per parked
  goroutine anyway (1000 parked = 1000 threads); it converges identically to M1 with strictly more
  code between here and there.
- *Verdict.* Not first. If S1's sweeps or perf gates measure real thread-churn cost, the remedy is
  a small **thread cache** on M1 (park finished goroutine threads briefly and reuse them — Go's own
  M reuse), which is additive, not a rewrite. M2 stays on the books as the shape that cache grows
  into if policy is ever actually needed.

**M3 — a custom `TaskScheduler`.** Rejected. A TaskScheduler chooses which thread runs a QUEUED
task; it has no hook at the moment a RUNNING task blocks — the one fact this problem is about. Any
blocking signal would come from the §5.3 contract anyway, at which point the TaskScheduler is Task
ceremony on the launch path plus a second exception-propagation model (unobserved-task semantics)
fighting the goroutine root's carefully preserved fatal path (`GoroutinePanicExitCode`'s contract).

**M4 — `gopark`/`goready` instrumentation alone, staying on the shared pool.** Rejected as a fix:
accounting without owned capacity leaves the ladder intact, because the pool does not consult our
counters. Kept as the design's second half (§5.3) — the contract is what §1.1's consumers need,
and under an owned executor it is nearly free.

**M5 — async lowering.** The only mechanism that delivers Go-scale counts on the CLR, and the one
that collides with a stated project principle. Priced in §8, where the collision is treated as an
explicit open question rather than assumed away.

*External evidence, briefly.* The CLR's own green-threads experiment (2023) was built and shelved —
the platform's official answer to blocking at scale is async/await, and stack switching is not on
offer. Java's Loom shows thread-shaped user code CAN scale when the VM owns parking, but it bought
that with VM-level continuations the CLR does not have. The honest menu on .NET is exactly two
items: threads (M1/M2) or async (M5). go2cs's charter — Go-like behavior, no implicit async,
visually similar output — picks threads; M1 is threads with the least machinery.

## 5. The design — identity, executor, park accounting

Three pieces, in dependency order. All of it golib and hand-owned runtime files; none of it
converter, emission, or corpus surface.

### 5.1 Goroutine identity and the registry

`Goroutine` (golib/runtime) grows from a static policy holder into a per-goroutine identity
object — the `[ThreadStatic]` g-model `lock_managed_impl.cs` forecasts:

```
sealed class Goroutine {
    long   Id;              // monotonic; internal only — Go deliberately hides goroutine ids,
                            //   so NO converted-code API exposes this (thread name + debugger only)
    GoroutineState State;   // Running | Parked(reason) — written only by its own thread / park scope
}
static [ThreadStatic] Goroutine? t_current;   // the getg() root; null on non-goroutine threads
static registry: live set + Interlocked count (main goroutine registered at golib module init)
```

`OnGoroutine` becomes `t_current is not null` (the r16 gate keeps its exact semantics — `Enter()`
scopes still mint/restore an identity for host-created goroutine threads, i.e. the test host's
tRunner threads, `TestExecution.cs:130`). `NumGoroutine()` becomes the registry count. The main
goroutine registers at init so counts and the future deadlock detector see it; host threads that
never run Go code never appear.

### 5.2 The executor — dedicated threads behind `Goroutine.Start`

`Goroutine.Start(body)` becomes:

```
Thread t = new(() => Run(body), GoroutineStackReserve) {
    IsBackground = true,
    Name = $"goroutine-{id}",
};
t.Start();
```

`Run` is UNCHANGED — the Goexit catch, the containment filter, the panic fatal path all stay
exactly as r16 built them. The 34 `goǃ` overloads are untouched; no emitted call site changes; CNR
stays byte-identical. In the same stage, the min-thread floor in `builtin.cs:68-78` RETIRES: its
stated reason ("goroutines are synchronous ThreadPool work items") stops being true, and the pool
returns to .NET duty at .NET defaults.

**Invariants that MUST hold across the swap, each with its standing witness:**

| Invariant | Why | Witness |
|---|---|---|
| ExecutionContext flows into the goroutine | test attribution rides `AsyncLocal` (`TestExecution.cs:64-67`); `Thread.Start` captures EC exactly as `QueueUserWorkItem` did | converted-test host attribution (any sweep failure attributes to the right test) |
| `IsBackground = true` | when `main` returns the process exits regardless of live goroutines — Go's exit semantics; a foreground thread would hold the process open | every behavioral program whose `main` exits with goroutines parked |
| body runs start-to-finish on ONE thread | every §2 thread-affinity row | `GoexitDefers`, sync's banked 41, `SyncTimerChannel` |
| unrecovered goroutine panic = process death, exit 2, Go-shaped stderr | Go fidelity; the containment filter must keep matching only what it matched | `GoroutinePanicExitCode` |
| `GoexitException` swallowed at the root only | r16 contract | `GoexitDefers`, `TestOnceFuncGoexit` |
| a goroutine thread that finishes stops looking like one | registry entry retired in `Run`'s exit path (the analog of r16's restore-the-flag rule) | `NumGoroutine` (S3) returning to baseline |

**Stack reserve.** Go stacks grow to ~1GB; a .NET thread's stack is a fixed reservation whose
overflow is uncatchable. The test host answered this with a 256MB reserve — address space, not
memory; pages commit on demand (`TestExecution.cs:57-62`, with io/fs's legitimate 10,001-frame
recursion as the motivating witness). The same reasoning transfers: goroutine bodies are arbitrary
Go code. Recommendation: the same 256MB reserve, one shared constant, env-overridable
(`GO2CS_GOROUTINE_STACK`) for hosts where VA is constrained. 10⁴ goroutines × 256MB ≈ 2.5TB of a
64-bit process's 128TB VA — the reserve is not the scaling bound; live-thread count is. (**OQ2**.)

### 5.3 `gopark`/`goready` — the accounting contract, not a wait relocation

Go's `gopark` does three jobs: publish "parked (reason)", switch the g off the M, run a commit
callback under the publication protocol. Under a thread-affine executor the middle job does not
exist and the third stays inside each primitive's own (already adversarially hardened) protocol.
What remains — and what every §1.1 consumer needs — is the first job. So the managed contract is
deliberately small:

```
using (Goroutine.Park(WaitReason.ChanReceive))   // no-op when t_current is null (host threads)
{
    parked.Park.Wait();                           // the EXISTING primitive, untouched
}
```

`Park` flips `t_current.State` to Parked(reason) and a global parked-count; `Dispose` flips it
back after the wake. There is deliberately **no goready side**: the waker already signals the
primitive (`Waiter.Wake`, `Semrelease`, timer fire), and the woken thread un-marks itself. This is
the cheapest contract that makes "how many goroutines are parked, and why" a runtime fact, and it
does NOT relocate any wait protocol — no lost-wakeup re-verification is owed, the wave3 park/claim
paths stay byte-for-byte, and adopting it is a mechanical wrap at each §6 site.

What the contract does not attempt (and Go's does): the commit-callback publication protocol
(each primitive keeps its own park-under-lock discipline), and wake-to-run handoff latency
accounting. If a future consumer needs a real `goready` hook (netpoll integration under M2, run
queues), it extends this contract; nothing lands speculatively.

### 5.4 What each consumer gets (none of these land in this arc — they unblock)

- **Deadlock detection** (`fatal error: all goroutines are asleep - deadlock!`, exit 2): registry
  count == parked count, with a conservative disarm — any armed timer, any in-flight poll wait,
  any host-marked goroutine present ⇒ never fire. Its own follow-up arc with its own gates (**OQ7**).
- **`runtime.NumGoroutine`**: the registry count, displacing the dead `gcount()` read via the
  established `manualConversionFuncs` mechanism (`managed_impl.cs` precedent).
- **Goexit Option A** (`DESIGN-goexit.md` §3): main-goroutine Goexit waits on the registry
  draining, then dies with Go's fatal text. The gate it needs exists once the registry does.
- **`lock_managed_impl.cs`'s deferred bookkeeping**: `getg()`-shaped code gets its
  `[ThreadStatic]` root; the m.locks/preempt lines return when something needs them.
- **The netpoll arc**: §7.

## 6. The park inventory — every site, today's primitive, its Go counterpart, its stage

The complete census of places converted or hand-owned code blocks a thread on a Go-semantic wait.
"Adopts" means wrapping in §5.3's scope — accounting only, protocol untouched.

| # | go2cs park site | Primitive today | Go's counterpart (converted-dead site) | Adopts |
|---|---|---|---|---|
| 1 | channel send/recv — `golib/channel.cs:388-389, :471-472` (park after `Monitor.Exit`) | per-waiter `SemaphoreSlim(0,1)` (`Waiter.Park`, `:150`) | `gopark(chanparkcommit…)` — `runtime/chan.cs:278, :652` | S3 |
| 2 | blocked `select` — `golib/channel.cs:802` | shared `SelectState.Park` (`:125`) | `gopark(selparkcommit…)` — `runtime/windows/select.cs:322` | S3 |
| 3 | channel timed/uncancelable helper wait — `golib/channel.cs:1514-1521` | `Thread.Sleep` / `WaitHandle.WaitOne` | (same family as 1) | S3 |
| 4 | `sync` runtime semaphore (Mutex/RWMutex/WaitGroup) — `sync/runtime_impl.cs:83` | per-waiter `ManualResetEventSlim`, FIFO + direct handoff | `semacquire1` → `goparkunlock` — `runtime/sema.cs` via `runtime/windows/proc.cs:420` | S3 |
| 5 | `sync.Cond` ticketed notify list — `sync/runtime_impl.cs:188` | per-waiter `ManualResetEventSlim` | `notifyListWait` → `goparkunlock` | S3 |
| 6 | `internal/poll` fdMutex semaphore — `internal/poll/runtime_sema_impl.cs:50+` | per-waiter `ManualResetEventSlim`, FIFO | same sema family | S3 |
| 7 | `time.Sleep` — `time/time_impl.cs:454-471` → `waitUntil` | per-thread high-res waitable timer (`:916-920`) | `gopark(resetForSleep…)` — `runtime/time.cs:277` | S3 |
| 8 | timer/ticker channel delivery | parks via rows 1-2 (`sendTime` is a non-blocking send; the service thread never parks-as-goroutine, `time_impl.cs:684`) | runtime timer heap | covered by 1-2 |
| 9 | netpoll `ManagedPollDesc` waits (proposed, branch) | `Monitor.Wait` per that design's §4.1 | `gopark(netpollblockcommit…)` — `runtime/netpoll.cs:605` | optional, netpoll arc's call (§7) |
| 10 | testing parallel gate — `TestExecution.cs:75` | `ManualResetEventSlim` on an already-dedicated thread | tRunner's channel recv | optional, accounting only |
| 11 | runtime note/mutex hand-own — `runtime/lock_managed_impl.cs` | SpinWait escalation | `notesleep`/`futexsleep` — parks **Ms, not Gs** | never — M-level, below the goroutine model, exactly as in Go |
| 12 | GC/finalizer background goroutines — `runtime/mfinal.cs:213`, `mgc.cs:1209` | converted-dead; the CLR owns GC and finalizers | `gopark(finalizercommit…)` etc. | never |

`runtime.Gosched` is a yield, not a park — it stays `Thread.Yield()` and never enters this table.

## 7. The netpoll boundary — which side owns what

The netpoll design (branch `claude/netpoll-design`) hit this design's subject from the other side:
its §3.2 names `gopark`/`goready`/timers/g-CAS protocols as the deep wall behind wiring
`internal/poll` through the converted runtime, and §3.2's decisive point — nothing under the CLR
would ever pump `netpoll(delta)`, because the thread Go dedicates to draining the poller IS the
scheduler — is the reason it cut at the ten-contract boundary and parks callers on a Monitor
inside `ManagedPollDesc` instead. The two arcs share that boundary; here is the proposed ownership,
stated so neither doc's claims drift:

- **This arc owns:** goroutine identity, the registry, the park-accounting contract (§5.3), and —
  already owned today — the timer engine (`time_impl.cs`). These live in golib and the hand-owned
  runtime layer; they are Go-semantics-generic (contrast the netpoll doc's OQ6, which correctly
  keeps a Windows IOCP poller OUT of golib).
- **The netpoll arc owns:** the ten `runtime_poll*` contracts, `ManagedPollDesc`, its deadline
  machinery, and its own Monitor waits. Those waits are ordinary thread blocks — exactly as
  legitimate under this design's executor as a channel park, and row 9 of §6 is theirs to adopt
  (accounting only) whenever it becomes useful. **Neither arc depends on the other's landing
  order**: the poller works unchanged on either executor, and this scheduler needs nothing from
  the poller.
- **Nobody resurrects the runtime's netpoll.** `netpoll(delta)`, `netpollBreak`, `netpollready`,
  `netpollAnyWaiters` stay converted-and-dead under both designs. If a far-future arc ever wants
  through-the-runtime wiring, it consumes §5.3's contract as extended at that time — and that arc,
  not this one, prices it.

The one sentence the netpoll doc should eventually carry back (a one-line amendment when it next
revises): its §3.2 "no g to park" premise stays TRUE under this design — there is still no
multiplexer — but "parking a goroutine" becomes an accounted, first-class runtime event rather
than an anonymous thread block, which is all its row-9 adoption would consume.

## 8. The no-implicit-async principle — respected, and what respecting it costs

CLAUDE.md states the conversion principle this design must either respect or argue against:
*"Generated C# intentionally targets Go-like behavior first (no implicit async), and Go-like
appearance second."* Async lowering — transpiling Go functions to `async` methods so every park
becomes an `await` and goroutines become tasks multiplexed M:N on the pool — is the one mechanism
that would deliver Go-scale goroutine counts on the CLR. Priced against this codebase, not in the
abstract:

1. **Every signature changes.** `func f() (int, error)` becomes `Task<(nint, error)>`-shaped;
   every call site grows `await`. The visible output stops reading like the Go original — the
   project's first-listed goal — at every function boundary, not in hidden machinery.
2. **The defer/recover emission rebuilds from scratch.** `GoFrame` is a `ref struct`
   (`GoFrame.cs:54`), which the C# language forbids from living across an `await`; the entire
   try/catch/finally + `GoFrame` emission (the r41 arc that retired the GoFunc execution-context
   ladder) would need a third generation. `recover()` reads `ThreadLocal` slots
   (`GoFuncRoot.cs:20-26`) that stop being coherent the first time a body hops threads at an
   await; every row of §2's thread-affinity table migrates to `AsyncLocal` — whose copy-on-write
   flow semantics are NOT drop-in for mutable-slot patterns (a write after an await inside a
   deferred closure lands in a context copy).
3. **The ж model fights it.** Pins and interior addresses held across an await cross GC
   safepoints on foreign threads; the pipe-EOF class of defect (`DESIGN-channels.md` §4 epilogue)
   gets a new, wider window.
4. **Syscalls stay synchronous anyway.** Every P/Invoke (`ReadFile`, `WSARecv`, the dispatcher)
   blocks a real thread; async lowering virtualizes only MANAGED waits, so the syscall-heavy
   suites keep holding threads and the boundary becomes sync-over-async — the worst of both.
5. **The blast radius is the whole project.** Every `visit*`/`conv*` converter file, every golden,
   every corpus package re-baselines; the hardened closure-emission and GoFrame arcs are
   invalidated wholesale — the exact shape the nothing-throwaway principle exists to forbid, spent
   to benefit programs (10⁵+ goroutines) no corpus suite runs.
6. **The tax is universal.** State-machine transformation costs on the ~99% of converted calls
   that never park, paid so the <1% that do can multiplex.

**Position: respect the principle.** This design takes threads, accepts the ~10⁴ bound as the
documented divergence's new, smaller shape (§3), and records async lowering as the only known path
past that bound — an arc that would require its own design, its own measured justification (a real
consumer needing 10⁵ goroutines), and an explicit coordinator amendment of the conversion
principle first. **OQ5** asks the coordinator to ratify exactly that recording, so the question is
settled by ruling rather than by this document's assumption.

## 9. Blast radius and gates

**What changes (S1):** golib only — `Goroutine.cs` (identity + executor), `builtin.cs` (floor
retirement), one new behavioral guard project. No converter `.go` files, no `projitems` entry, no
emission change, no corpus regen, no hand-own census motion (golib is not marker territory).
S3 adds mechanical park-scope wraps inside `golib/channel.cs` and three already-marked `_impl.cs`
files (census count unchanged — the files are already marked) plus the `NumGoroutine`
displacement (one `manualConversionFuncs` data entry; its regen is that stage's A/B footprint).

**Gates, per the standing rules for the golib change class:**

- **Full behavioral suite** (`run-behavioral.ps1`, budget from CLAUDE.md's table top) — the
  goroutine-semantics guards are the point: `GoroutinePanicExitCode`, `GoexitDefers`,
  `ChannelRendezvous`, the select family, `SyncTimerChannel`, `LocalTimeZone`.
- **CNR** — expect byte-identical except the new guard project's own files (S1) / the enumerated
  `NumGoroutine` displacement (S3), named in the commit.
- **`dotnet build src/go2cs.slnx` once before banking** — the QuickTest lesson; golib API surface
  moves (floor retirement, new members).
- **`go2cs-stdlib.slnx` build** — all 307 projects reference golib.
- **Full validated sweep** (110-package roster), coordinator-run solo per the standing rule. The
  whole roster is the gate for this change class; the closest watches are `sync` (41/10, the
  concurrency crown), `time`, `os`, `io`, `context`-shaped suites. ⚠ Priced expectation: goroutines
  now START truly concurrently instead of in pool-sized waves, which is MORE Go-like — any new
  failure this surfaces is a real latent race being seen for the first time, to be classified and
  rooted, not a cue to revert the executor reflexively.
- **Performance suite, solo** — `PerfChannel` and `PerfStartup` are the sensitive rows
  (thread-creation on the launch path; floor retirement at init). `--no-aot` for iteration; the
  full AOT run (hours on the current coordinator hardware) at bank.
- **New behavioral guard `GoroutineParkStorm`** — ~1,000 goroutines park across a WaitGroup and a
  channel rendezvous storm, then release; deterministic output, output-compared vs `go run`. Under
  today's model this shape cannot finish inside the runner's 30s run budget (it IS §1's ladder);
  under S1 it finishes in well under a second. Lands WITH S1 per repo practice — it is the
  red-today witness that the gate guards something.

**The acceptance gate (this arc's reason):** the instrumented singleflight convergence re-run —
method inherited verbatim from the convergence branch (split `-tests` actions, per-iteration print
into the staged copy, measure, revert everything). Bar: **convergence in seconds — wall for the
race test ≤ 120 s** (expected: low single-digit seconds; the generous bar keeps the gate a gate,
not a benchmark). **The bank that follows (S2):** `internal/singleflight` **5/5** through
`go2cs -tests -test-action all` inside the DEFAULT 30-minute deadline with real margin — **no
`$longTimeouts` entry** — then the validated-package commit ritual (test sources + proof page +
roster row), superseding the board append's 60m-floor option before anyone lands it (**OQ9**).

**Risk register, priced:**

| Risk | Assessment |
|---|---|
| A roster suite spawns goroutines faster than threads create | Watch sweep wall-times at S1; the thread-cache follow-up (§4 M2 verdict) is the lever, additive not structural. No roster suite is known to spawn at that rate; sync's hammer tests spawn tens. |
| Latent races surface under genuine concurrency | Expected and desirable (see sweep gate note); Go runs these suites genuinely concurrent too. |
| A goroutine leak now leaks threads | Equivalent today: a leaked parked goroutine holds a pool thread forever. Same leak, better diagnostics (named threads, registry count). |
| Native AOT | `Thread` is fully AOT-supported; the perf suite's AOT publish gate proves it on the real closure. |
| Floor retirement regresses a non-goroutine pool consumer | None parks (Task continuations, timer callbacks); if the suite says otherwise, restoring one line is a revert, not a redesign. |
| Alloc-count disclosures move | Spawn cost lands on the spawner (one `Thread` + one closure vs one closure today); `AllocsPerRun` bodies in the banked set do not spawn. Verified by the sweep, not argued. |

**Docs owed with S1** (the step-7 rule): `ConversionStrategies-Reference.md:6063-6066` ("A parked
operation holds its ThreadPool thread (goroutines are pool work items)…") and `:6971` ("`goǃ`
queues the body on the bare thread pool…") both describe the executor this design replaces —
update both in the landing commit, plus the goroutines section's headline if the summary doc
carries one.

## 10. Staged landing

| Stage | Contents | Gate |
|---|---|---|
| **S0** | This document; coordinator rulings on §11 | — |
| **S1** | Registry + identity (§5.1), dedicated-thread executor (§5.2), floor retirement, `GoroutineParkStorm` guard, the two reference-doc updates | Full suite + CNR + both `.slnx` builds + full sweep + perf (solo) + the instrumented convergence measurement (acceptance gate) |
| **S2** | `internal/singleflight` validated 5/5 inside the default deadline; bank per the validated-package ritual; board row flipped; roster +1 | The bank IS the gate |
| **S3** | Park-scope adoption at §6 rows 1-7 (mechanical wraps, protocols untouched); `NumGoroutine` over the registry | Full suite + sweep again (park-path adjacency is the highest-scrutiny class even for wraps); CNR footprint = the one displacement regen |
| **S4+** | Each its own charter, unblocked not promised: deadlock fatal (OQ7), Goexit Option A, netpoll park-scope adoption (netpoll arc's), thread cache if S1/S2 measurements demand it | per-arc |

S1 and S2 are one lane's natural span (S2 is a measurement of S1). S3 is separable and safe to
defer — nothing in S1/S2 depends on it; it exists for the §1.1 consumers.

## 11. Open questions — RULED (coordinator, 2026-08-13)

> **All nine recommendations are RATIFIED as written.** The arc is chartered: dedicated thread per
> goroutine behind the r16 funnel, the 256MB shared reserve, floor retirement in the executor-swap
> commit, park accounting wrapped around the existing hardened primitives, and the §7 netpoll
> ownership split. Two annotations: (1) OQ5's out-of-scope recording for async lowering is
> ratified WITH a named reopening path — .NET 11's runtime-async transform (observed in the C# 15
> preview notes, 2026-08-13) is the watch-item that would qualify as "a future arc with a real
> consumer" under §8's principle-amendment route; (2) OQ9 resolves in the ratifying direction —
> no `$longTimeouts` bridge was landed, the arc is chartered promptly, S2 banks singleflight
> inside the default deadline. Each item below retains its original recommendation text as the
> record of what was ratified and why.

- **OQ1 — Executor** (§4): dedicated thread per goroutine (recommended) vs owned pooled executor
  with park-driven injection (rejected as first move: same thread count where it matters, strictly
  more machinery, plus the un-instrumented-block hole) vs accounting-only on the shared pool
  (rejected: the ladder survives). Ruling this rules the arc.
- **OQ2 — Stack reserve** (§5.2): 256MB reserve matching the test host's constant, shared,
  env-overridable (recommended) vs a smaller default (cheaper VA, re-imports the uncatchable
  overflow risk for deep-recursing goroutines the host already paid to remove).
- **OQ3 — Floor retirement timing** (§5.2): same commit as the executor swap (recommended — its
  stated premise is deleted in that commit) vs a separate follow-up stage (safer-looking, but it
  leaves a process-global distortion running for no stated reason, and the revert is one line
  either way).
- **OQ4 — Park-contract shape** (§5.3): accounting scope wrapped around existing primitives
  (recommended; zero protocol relocation, zero re-verification debt) vs relocating waits into a
  central gopark that owns the block (Go-shaped, but it re-opens every adversarially-hardened
  park/claim protocol for no consumer that needs it).
- **OQ5 — The async question** (§8): ratify recording async lowering as out of scope for goroutine
  scheduling, with the ~10⁴-goroutine OS-thread bound accepted as the documented divergence's new
  shape, revisitable only by a future arc with a real consumer and an explicit principle
  amendment. (Recommended. The alternative — leaving it formally open — costs nothing today but
  invites re-litigating §8 at every scheduler-adjacent lane.)
- **OQ6 — The netpoll seam** (§7): confirm the ownership split — this arc owns identity/registry/
  park accounting/timers; the netpoll arc owns the ten contracts and its Monitor waits, adopting
  row 9 accounting at its own option; no landing-order dependency either way.
- **OQ7 — Deadlock detection** (§5.4): charter as its own follow-up arc once the registry exists
  (recommended), never as a rider on S1-S3 — the conservative-disarm design wants its own
  adversarial pass (false-positive fatal is worse than today's silent park-forever).
- **OQ8 — `Gosched`/`GOMAXPROCS`** (§2, §3): both unchanged (recommended) — `Thread.Yield` and the
  get/set-only divergence respectively; a runnable-limiter honoring GOMAXPROCS is M2-shaped
  machinery with no consuming suite.
- **OQ9 — The singleflight bridge** (§1, §9): if this arc is chartered promptly, do NOT land the
  60m `$longTimeouts` floor the board append offers — S2 banks the row inside the default deadline
  and the bridge would be dead code with a sweep tax. If the arc is deferred past the next sweep
  cycle, the bridge is the board's call to make on its own schedule.
