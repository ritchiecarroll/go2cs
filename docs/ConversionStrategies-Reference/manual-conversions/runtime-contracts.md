# Manually-Converted Declarations: Runtime Contracts

[Reference index](../README.md) · [Manually-Converted Declarations](../manual-conversions.md) · [Summary of this topic](../../ConversionStrategies.md#manually-converted-declarations)

This page covers the runtime and sync contracts golib implements in managed code: process control, timers, the GC measurement surface, weak and pinned pointers, and the synchronization primitives.

## Process control

### The runtime's PROCESS-CONTROL surface: implement the CONTRACT, never the mechanism

`runtime`'s public control API — `GC`, `GOMAXPROCS`, `Gosched`, `Stack`, `ReadMemStats`,
`LockOSThread`/`UnlockOSThread` — converts faithfully and **compiles**, then dies on its first call.
Each body drives machinery that has no managed counterpart: `GC()` → `gcStart` → `acquirem` →
`getg()`; `GOMAXPROCS(n)` → `stopTheWorldGC` → `semacquire` → `getg()`; `Gosched()` →
`mcall(gosched_m)`. `getg` and `mcall` are Go **compiler intrinsics** — a register read and a stack
switch — so the [`PartialStubGenerator`](../source-generators.md#source-generators) fills them with a throw, and the throw
lands wherever the caller ran. On a goroutine (a managed thread) that is an unhandled exception and
**terminates the process**: sync's suite lost 28 of its 51 tests to one such throw.

The ruling is the one sync's `Mutex`/`notifyList` established: **where a Go mechanism has no managed
counterpart but its public contract does, reimplement the CONTRACT at the API boundary and never
emulate the mechanism.** Synthesizing a plausible `g`/`m` so the converted scheduler can walk it buys
nothing — the code underneath still wants a real run queue, a real heap and real stacks. The seven
declarations above are dropped from emission by the type-level registry
(`manualConversionFuncs["runtime"]`) and answered in a hand-owned `runtime/managed_impl.cs`;
everything below them stays auto-converted and simply becomes unreachable.

| Go API | Managed realization | Divergence |
|---|---|---|
| `Gosched()` | `Thread.Yield()` | none — same "offer the rest of the slice, then continue" contract |
| `GOMAXPROCS(n)` | remembered value, defaults to `Environment.ProcessorCount`; `n < 1` queries | does **not** cap parallelism — the CLR schedules goroutine threads. The universal save/restore idiom `defer runtime.GOMAXPROCS(runtime.GOMAXPROCS(n))` is exactly right |
| `GC()` | blocking compacting collect → `WaitForPendingFinalizers` → collect → **`GcPauseRecorder.Drain()`** | none observable; the second pass reclaims what finalizers released, which is the state a completed Go cycle leaves. The drain is what makes `NumGC` current on return |
| `Stack(buf, all)` | managed `StackTrace` text into `buf` | cannot show frames that already unwound, and `all` reports only the calling thread (see below) |
| `ReadMemStats(m)` | `GC.GetTotalMemory`/`GetTotalAllocatedBytes` + one `GcPauseRecorder` snapshot | allocator-internal fields (`Mallocs`/`Frees`/`HeapObjects`/`BySize`) and `GCCPUFraction` stay **zero** rather than invented; the pause history, `LastGC`, `PauseTotalNs`, `NumGC`, `NumForcedGC` and `HeapReleased` are **real** (see below) |
| `LockOSThread`/`UnlockOSThread` | no-ops | no-ops **by construction**: a goroutine already *is* a managed thread, so the guarantee they exist to provide holds unconditionally |

`Stack`'s divergence is worth stating precisely because it looks like a bug: Go keeps the panicking
frames alive until the panic completes, so `debug.Stack()` called from a deferred function *during* a
panic shows the panicking stack; a CLR exception pops those frames before the `finally` that runs the
defers, so the managed trace shows the deferred frame's stack instead (sync's
`TestOnceFuncPanicTraceback`).

**`runtime/debug`'s knobs are the same ruling one package over.** `runtime/debug/stubs.go` declares
`setGCPercent`/`setMemoryLimit`/`setMaxStack`/`setMaxThreads`/`setPanicOnFault`/`readGCStats`/
`freeOSMemory` with no body ("Implemented in package runtime"), bound by the Go linker to a runtime
function carrying the matching `//go:linkname … runtime/debug.<name>` **PUSH**. go2cs has no
cross-assembly linker, so each became a throwing stub — `debug.SetGCPercent`, the first line of
sync's `TestPool`. Forwarding to the converted runtime bodies would not help (they take the mheap
lock on the system stack and wait on a mark cycle), so `runtime/debug/stubs_impl.cs` answers the
contracts: the tuning knobs keep Go's documented GET/SET semantics (remember, return the previous
value, negative = query where Go says so) and have no effect on collection; `freeOSMemory` is a real
compacting collect; `setPanicOnFault` is `[ThreadStatic]` because it is per-goroutine in Go;
`readGCStats` reports a **real** per-pause history in the exact packed layout `ReadGCStats` unpacks.
`modinfo`/`WriteHeapDump`/`SetTraceback`/`runtime_setCrashFD` are inert, matching a binary built
without module or heap-dump support.

**Two assembly primitives DO have exact managed forms** (`runtime/stubs_impl.cs`).
`systemstack(fn)` is `fn()` — Go's own contract already says that a caller already on a system stack
"calls fn directly and returns", and in the managed model there is one stack per goroutine and no g0
to switch to, so that is the only branch. `procyield(n)` is `Thread.SpinWait((int)n)`. Everything
else in `runtime/stubs.go` deliberately keeps throwing, `getg`/`mcall` included: a loud, locatable
failure beats quietly operating on a fabricated goroutine descriptor.

**`internal/runtime/atomic` is the NATIVE half of the S1 fork and gets a real conversion, not a
stub** (`atomic_impl.cs`). Its ~40 declarations are all `.s` files, but they are plain memory atomics
over native scalars, and the CLR has an exact equivalent: `Xadd*`→`Interlocked.Add` (both return the
NEW value), `Xchg*`→`Interlocked.Exchange` and `And32`/`Or32`/`And64`/`Or64`→`Interlocked.And`/`Or`
(all return the OLD value), `Cas*`→`CompareExchange`, `Store*`/`StoreRel*`→`Volatile.Write` (a
release store in both models). Two widths have no intrinsic and take a shared latch instead: 8-bit
(`And8`/`Or8` — the CLR has no byte-width `Interlocked`) and the `unsafe.Pointer` family
(`Casp1`/`StorepNoWB`/`storePointer`/`casPointer` — golib models `unsafe.Pointer` as a class wrapping
a `uintptr`, so a CAS must compare the wrapped NUMBER and swap the REFERENCE, which no single
intrinsic expresses); `Xadduintptr`/`Anduintptr`/`Oruintptr` ride a CAS loop because `Interlocked`
offers `Exchange`/`CompareExchange` for `nuint` but not `Add`/`And`/`Or`. Leaving this package
stubbed poisons everything built on it — `runtime.SetMutexProfileFraction`, an otherwise perfectly
faithful conversion, died on `Store64` (sync's `TestMutex`).

#### `runtime.Goexit` unwinds the goroutine with a `GoexitException`

**`runtime.Goexit` unwinds the goroutine with a `GoexitException`, and every one of Go's three
properties falls out of machinery that already existed.** Go specifies that Goexit ends the calling
goroutine only: its deferred calls all run, `recover()` inside them returns **nil** (a Goexit is not a
panic, and a defer cannot cancel it), and other goroutines are untouched. The managed form is a golib
`GoexitException` that is deliberately **not** a `PanicException` — `recover()`'s implementation keys
on `PanicException` (the frame's `GoFrame.IsPanic` filter, via `RuntimeErrorPanic.TryAsPanic`), so it
is blind to this type BY CONSTRUCTION and the recover path needs **zero** special handling. The defers
still run because `GoFrame.Run()` sits in a `finally`, draining the defer list during the unwind
exactly as it does for a panic, across frames. Every `go` statement dispatches through one **goroutine root**
(`golib.Goroutine.Start` → `Run`, the single site all 18 `builtin.goǃ` arity overloads funnel into),
which catches `GoexitException` and ends that thread silently; a `PanicException` reaching the same
point is deliberately NOT caught and keeps its Go-faithful fatal path (stderr report, exit 2 — guarded
by the `GoroutinePanicExitCode` behavioral test). `runtime.Goexit`'s body is hand-owned in the runtime
package's `managed_impl.cs` (`manualConversionFuncs`), since the converted body drives Go's own
`_panic` record and stack unwinder (`getcallerpc`/`nextDefer`/`goexit1` — all assembly). Guarded by
the `GoexitDefers` behavioral test (defers run across frames, `recover()` sees nil, other goroutines
still run, main continues) and by sync's `TestOnceFuncGoexit`, which this unblocked.

**Goexit from the MAIN goroutine stays gated — at runtime, not statically.** There, Go ends `main`
without returning while the *program continues* running its other goroutines, crashing with
"no goroutines" once they all exit; that needs a live-goroutine registry and a main-thread parking
protocol the managed model does not have (`docs/phase4/DESIGN-goexit.md` option C). The distinction is
not statically decidable — a function's call graph says nothing about which goroutine will run it — so
`Goroutine.OnGoroutine` (a `[ThreadStatic]` the root sets and restores, because goroutines run on
pooled threads) answers it at the call, and the main-goroutine case throws a loud
`NotSupportedException` naming the design doc rather than silently doing something else. Consequently
`unsupportedRuntimeCapabilities` (`testConversion.go`) is now **empty**: the mechanism remains — a test
whose transitive closure reaches a listed symbol converts as `unsupported`, disclosed by name, the
same gate an unsupported `testing.*` member uses — with `TestUnsupportedRuntimeCapabilityGate` as its
positive control so an empty list cannot masquerade as a working lookup. Add an entry only for
something *provably* unavailable, never merely unimplemented, and scan every validated package for the
symbol first: gating one **removes** tests from a banked package's run set, the mirror of the widening
trap.

### `iter.Pull`'s coro — a symmetric handoff between two threads, and the goroutine count that had never been wired

**The narrowest cut yet through a scheduler primitive, and the one where the converted code stayed the specification.** `iter.Pull`/`Pull2` are built on two `//go:linkname` entries into the runtime, `newcoro` and `coroswitch` (`iter/iter.go:213–217`). Go describes a coro as *"a special channel that always has a goroutine blocked on it"*: `coroswitch(c)` makes the caller the blocked party and starts the party that was blocked, so control alternates and the two contexts are never runnable at once. `coroswitch_m` implements that by swapping stacks and ending in `gogo`.

**Everything else in `iter` converts faithfully, and that is why the hand-own is four methods rather than a package.** The yield closure, the `yieldNext` handshake, the `done` latch, and the deferred `recover` that turns a panic *or* a `runtime.Goexit` inside the sequence function into a `panicValue` the pulling side re-raises are ordinary Go, and `iter.cs` reproduces them line for line — panic texts (`iter.Pull: next called again before yield`) included. Only the control TRANSFER has no managed counterpart, so the seam is drawn exactly there. The converter already emits the two linkname declarations as bodyless `partial`s, so **no `manualConversionFuncs` entry is needed for them** — there is no Go body to displace — and `core/iter/iter_impl.cs` simply supplies the implementing halves. (Without it the `PartialStubGenerator` fills both with throwing stubs, which is why `iter.Pull` raised `NotImplementedException` on first use.)

The transfer itself is golib's (`go.golib.Coro`), not `iter`'s, because it is a runtime capability: Go declares it in `runtime/coro.go`, and the corpus carries the mechanically converted, permanently dead counterpart at `runtime/coro.cs`, whose body bottoms out in the `mcall`/`getg`/`newproc1`/`gogo` stubs. Four decisions are worth cribbing:

- **A thread and two one-permit semaphores, because the callee is arbitrary converted Go.** The sequence function needs a real stack, which under the CLR means a real thread — the same conclusion `Goroutine` reaches for goroutines. Capacity **one** is the point: a permit is a TURN, not a count, so a second release before the peer consumes the first would mean both sides were runnable, and `SemaphoreSlim` throws rather than letting that pass. The alternatives (an iterator state machine, a `Task`) both require the CALLEE to be written for them.
- **The coro goroutine is a real goroutine, registered before `newcoro` returns.** Go's `newcoro` creates the g synchronously and `NumGoroutine` counts it from that moment — `iter`'s own tests assert one extra goroutine on the statement *after* `Pull` returns. A thread left to register on its own time makes that count race, so `Coro.Start` waits for the handshake. **The exit side is the mirror and matters as much**: the identity is retired BEFORE the caller is released, so a puller can never observe a count that still includes a coro which has finished.
- **Panics and Goexit cross by not being special.** The body runs under `Goroutine.Run` — the same root every `go` statement uses — so a `GoexitException` ends the coro goroutine after its defers have run, a host containment policy still contains an infrastructure failure to one test, and an unrecovered panic keeps Go's fatal path. Nothing is re-implemented. What the caller sees is whatever the body recorded in the closure both sides share, which is Go's own mechanism rather than an emulation of one. The release sits in a `finally` so an escaping panic **crashes rather than hangs**: Go's outcome is process death either way, but a peer parked on a permit nobody will release wedges the run instead of reporting.
- **The token is keyed, not widened.** `iter` declares `type coro struct{}` — an empty struct whose only job is to be a token the two functions agree on. There is nowhere in it to put a rendezvous, and widening it would both diverge from Go's field set and change how `GoZeroSizeFacts` classifies it. A `ConditionalWeakTable` keyed on the `ж<coro>` box leaves the converted type exactly as Go declares it (`new(coro)` mints a fresh `StandardBox` per call, and boxes compare by IDENTITY — the property `sync`'s semaphore table already relies on), and the entry cannot outlive the token.

**`runtime.NumGoroutine` was wired in the same change, and it had never answered truthfully.** Its Go body is `gcount()`, which derives the live count by SUBTRACTION over scheduler state the managed model never populates — `allglen`, less `sched.gFree.n`, less `sched.ngsys`, less each P's `gFree.n`. Every term was zero, and `gcount`'s own `if n < 1 { n = 1 }` floor then turned the nonsense into a plausible-looking constant: **`NumGoroutine()` returned 1 for every program, forever** — exactly the shape of wrong that survives unnoticed, because a single-goroutine program's answer really is 1. golib's `Goroutine` registry had maintained the true count all along (it is what the SIGQUIT dump already printed), so this is a WIRING rather than an approximation: a `manualConversionFuncs` entry displaces the auto body and `managed_impl.cs` returns `Goroutine.Count`. Go's staleness caveat carries over unchanged and for the same reason; `gcount`'s floor does not, since the registry cannot report fewer than the caller's own goroutine.

Guarded on both sides: `iter`'s converted suite validates **28/28** against `go test` (goroutine accounting, double-next/double-yield, panic-through on `next` and on `stop`, Goexit across the boundary, immediate stop), and the `IterPullRendezvous` behavioral test pins the rendezvous semantics against `go run` in the corpus gate — deliberately printing no goroutine counts, since a count is stable in Go only under the stabilization loop that suite uses, and a flaky stdout comparison would fire across the whole corpus rather than in one package.

## Timers

### Realizing the runtime TIMER contract (`Sleep` / `newTimer` / `stopTimer` / `resetTimer`)

`time`'s four timer entry points have no Go body — they are `//go:linkname`'d into `runtime/time.go` — so the converter emits them as bodyless `partial`s that the [`PartialStubGenerator`](../source-generators.md#source-generators) fills with `NotImplementedException`. A stub throw on a timer path is uniquely destructive: it lands on whichever goroutine touched the timer, and an unrecovered panic in *any* goroutine terminates the process, so every package that so much as called `time.Sleep` once was unreachable. `time_impl.cs` supplies the four bodies (the same supplemental-companion mechanism as `math_impl.cs` and the clock reads above it), and the whole model is a single `_impl.cs` region — no converter or golib change.

**Service model: one heap, one thread — Go's own pre-per-P design.** Go keeps timers in per-P heaps run by whichever P first notices a deadline; the managed model is one deadline-ordered heap serviced by one dedicated background thread, the shape Go itself used before per-P timers (the old runtime `timerproc`). One thread is sufficient *and* ordering-faithful for a specific reason: the only two callbacks package `time` ever installs are `sendTime` (a **non-blocking** channel send) and `goFunc` (which only starts a goroutine), so no timer callback can occupy the service thread and delay a later deadline. Callbacks are collected under the lock and invoked after releasing it, in deadline order.

**Precision: the same OS object Go uses.** `System.Threading.Timer` is *not* the mechanism, because its resolution is the Windows timer tick — measured at ~15 ms on the development host, which would make a 1 ms Go timer fire 15× late and let two timers less than a tick apart fire **out of order**. Both `Sleep` and the service thread instead wait on a Windows high-resolution waitable timer (`CreateWaitableTimerExW` + `CREATE_WAITABLE_TIMER_HIGH_RESOLUTION`), which is precisely what the Go runtime creates for its own sub-tick sleeps (`runtime/os_windows.go` `createHighResTimer`/`usleep`), cached **per thread** exactly as Go caches it per M. The wait is driven off the same monotonic source `runtimeNano()` reads, so timer deadlines stay coherent with `Now()`/`Since`/`Sub`; a truncation in the 100 ns due-time unit can only wake early, and the deadline loop re-waits the remainder, preserving Go's "at least `d`" guarantee. Where no high-resolution timer exists the fallback waits coarsely to within a millisecond and spins the remainder — correct, but tick-quantized, which is the whole reason the high-resolution path is preferred. The interop is `[LibraryImport]` over `nint` and `ref long`, and this file is where the migration to it cost the most: the source generator marshals no `SafeHandle` and guesses at no `bool`, so the handle's reference count is now taken visibly in `Arm` and both `bool`s carry an explicit `UnmanagedType.Bool`. It also needs `/unsafe`, which `time`'s converted emission does not, so `time_impl.cs` carries a [`[module: go.GoRequiresUnsafe]`](mechanism.md#a-hand-owned-file-can-declare-that-it-needs-unsafe) declaration — the mechanism that exists because the csproj is regenerated on every transpile. See [Every P/Invoke is source-generated](mechanism.md#every-pinvoke-is-source-generated).

**Hidden state keyed by BOX IDENTITY.** Go's `runtime.timeTimer` carries the timer state in fields *after* the two `time` can see — sleep.go's "extra fields after the channel, reserved for the runtime and inaccessible to users". The managed equivalent hangs a `runtimeTimer` record (`when`/`period`/`f`/`arg`/`gen`) off the `ж<Timer>` box's **reference identity** through a `ConditionalWeakTable`: `stopTimer`/`resetTimer` are always handed the very box `newTimer` returned. Weak-keyed, so an unreferenced `Timer` stays collectible (Go 1.23 recovers unreferenced timers) — while an *armed* timer's state is independently kept alive by the service heap, which is what makes a bare `time.After(d)`, whose `Timer` is dropped on the spot and only the channel kept, still fire.

**The Stop/Reset contract and the fire race.** Both report `when > 0` — Go's `timer.stop`/`timer.modify` compute `pending` exactly that way — so *pending* means armed and not yet fired, and `when` doubles as the arm marker (0 = stopped, or a one-shot that fired). Every change to `when` bumps a generation counter, and heap entries carry the generation they were queued with; a `Stop` or `Reset` therefore cancels an already-queued firing **without** removing it from the heap (Go leaves the stale entry too, marked `timerModified`/`timerZombie`). One lock guards the heap *and* every timer field — Go's finer-grained per-timer + per-P scheme exists to scale across Ps, and with a single heap there is one lock and hence no lock-ordering hazard — so a `Stop`/`Reset` racing the firing callback can neither double-fire nor lose a re-arm: either the service thread already took the callback (and `when` is 0, so `pending` correctly reports false), or the generation bump invalidates its queued entry and the callback never runs. A `Ticker` re-arms by **whole periods** past a late firing (`next = when + period*(1 + delay/period)`), which keeps the tick *phase* aligned to the original schedule instead of drifting; combined with `sendTime`'s non-blocking send onto the cap-1 channel, a receiver too slow to keep up therefore **loses** ticks rather than seeing them queue — Go's documented "adjust the time interval or drop ticks to make up for slow receivers".

**ONE firing per timer per pass — the service pass reads the clock exactly once.** `serviceTimers` samples `now` once and threads that single value through the whole drain. That is the invariant, not an optimization, and Go does the same for the same reason: the scheduler samples the clock in `timers.check` and hands it down through `timers.run(now)` to `timer.unlockAndRun(now)`, never re-reading it inside a pass. The consequence is a theorem rather than a heuristic — *within one service pass every timer fires at most once*. A one-shot leaves the heap with `when` cleared; a periodic timer is re-armed to `next = when + period*(1 + delay/period)` with `delay = now - when >= 0`, and writing `delay = q*period + r` for `0 <= r < period` gives `next = when + period*(1 + q) = now + (period - r)`, which `r < period` makes **strictly greater than `now`** — so the re-peek always takes the "not yet due" branch and the drain ends. It holds for every period, down to 1 ns. Re-reading the clock per iteration broke the theorem and was a real defect (recorded r36, fixed r39): the advanced `when` lands one nanosecond ahead, a freshly read `now` has already passed it, and the same ticker fires again — for as long as consecutive reads of the ~100 ns monotonic source keep advancing. The burst is invisible while nobody is receiving (the non-blocking send onto a cap-1 channel drops all but one) but `time`'s own `TestChan` *is* receiving: the two stale values an async ticker is allowed became three or more, and `noTick` reported "extra tick" in **all three** `asynctimerchan` modes — which the then-standing asynchronous-timer-channel divergence (now implemented, below), scoped to the sync mode, never explained. The invariant does not rate-limit: a pass fires each due timer once and then waits until the new head deadline, which for a fast ticker is already past, so the wait returns at once and the next pass fires it again — exactly as Go's scheduler calls `check` again with a fresh `now`. The bound is on re-firing *within* a pass, which is what "drop ticks to make up for slow receivers" means. Nor can it delay anything: `next` depends on `now` only through the non-decreasing floor `delay/period`, so hoisting the read can only make the pass's `deadline` smaller or equal, and `waitUntil` recomputes `remaining` from a fresh clock — no timer can wake later than the per-iteration version would have woken it, and when `delay < period` (the common case) the deadline is identical either way. A timer coming due *during* a pass waits for the next one and that is not a delay either, because the head of the heap is the minimum `when` among live entries, so the deadline is already past by the end of the drain and the next pass starts at once. Two adjacent places are deliberately **not** Go, and the fidelity claim should not be read past them: Go's `check` releases the timer-set lock around *each* callback and re-validates the head between them, so a `Stop` landing mid-pass cancels the callbacks after it, where this drain commits the whole batch under one lock hold and then runs it; and Go keeps one heap entry per *timer* (repositioned in place, zombies swept) where this keeps one per *arm*, reclaiming a dead entry only when it reaches the head. Both predate the single clock sample and are narrowed by it, since a frozen `now` commits a smaller batch. A standing constraint follows from the same arithmetic: at the instant a ticker is stopped or reset at most **two** of its ticks can exist — one buffered, one committed but not yet sent. In *synchronous* mode both are now revoked outright (the drain takes the first, the `seq` check the second — see the next entry), so the guarantee no longer rests on that count; in `asynctimerchan={1,2}` it still does, and two is exactly what `drainAsync` drains, so the margin there is zero and any later change that lets two ticks for one timer be committed before their callbacks run re-breaks `noTick` in those modes without touching `time_impl.cs`.

**`tick.cs` is hand-owned, and revertibly so.** Go builds a `Ticker` by reinterpreting the `*Timer` the runtime returned — `(*Ticker)(unsafe.Pointer(newTimer(…)))`, plus the mirror-image casts in `Ticker.Stop`/`Reset` — because "Ticker and Timer have the same layout". Converted literally those three reinterprets **compile but cannot work**: each is a managed-box `uintptr` round-trip whose address escapes its `fixed` pin, and nothing references the `ж<Timer>` `newTimer` produced, so the ticker's storage is collected at the next GC (the retained-pointer worst case of the corpus-wide hazard in `docs/phase4/FINDING-managed-box-uintptr-lifetime.md`). Pinning is not available either — `Ticker` holds a `channel<Time>`, a managed reference, and `GCHandle.Alloc(…, Pinned)` rejects a type containing references — so the raw-address model has no sound form here. The file (marked `[module: go.GoManualConversion]`, whole-file) builds the `Ticker` directly and addresses its timer by box identity, which is the standing managed-referent ruling at the top of this section; every doc comment and the struct declaration are the converted output verbatim, and the file should be **deleted** in favor of its `tick.cs.auto` sibling the moment a general managed-reinterpret capability lands.

**Synchronous timer channels — the property, and the two mechanisms that keep it (#37196).** Go 1.23 made a chan-based `Timer`/`Ticker` channel *synchronous*, and the name describes a guarantee rather than a plumbing change: the channel is still created as `make(chan Time, 1)` — a tick can be produced with nobody receiving — and what changed is that its owner may take a tick **back**. The whole proposal is one sentence: *a `Stop` or `Reset` prevents any tick generated before the call from being received after it.* Equivalently, every value a receive on `t.C` observes was committed **after** the most recent `Stop`/`Reset`, so there are no stale values and the pre-1.23 "drain `t.C` when `Stop` returned false" idiom is unnecessary. Two consequences are directly observable: `Stop`/`Reset` report `pending == true` when they **revoke** an unreceived tick, not only when the timer was still armed; and `len(t.C)`/`cap(t.C)` are **0**, because a buffered value the next `Stop` may confiscate must not be advertised.

The guarantee needs three mechanisms — two of Go's, at the same two layers, plus one this model needs because it fires eagerly where Go fires lazily. **(1) Send lock + sequence, in `time_impl.cs`** — Go's `timer.sendLock`/`timer.seq`. A firing is decided under the heap lock but must be delivered with it released, so a `Stop`/`Reset` can land in between; the service pass therefore only *offers* a tick. It captures `seq` when it commits the firing (exactly where Go's `unlockAndRun` copies `t.seq` under `t.mu`), then takes that timer's `sendLock` and re-checks — a mismatch means a `Stop`/`Reset` intervened and the offer is **abandoned** (Go expresses the same thing by replacing `f` with a no-op). `Stop`/`Reset` bump `seq` while holding `sendLock`, so every offer is wholly before them or wholly after; it cannot straddle. `seq` is deliberately *not* the heap generation `gen`: `gen` is bumped by a firing as well, which is what makes it the heap's liveness token, and a delivery check has to survive its own firing. **(2) Buffer drain, in golib** — `channel<T>.DrainBuffer()`, Go's `runtime.timerchandrain`: the `seq` check stops offers not yet sent, and a tick *already in the buffer* is revoked by emptying it. Both `Stop` and `Reset` drain, and either reports `pending` true when it discarded something. **(3) The `offered` flag, with no Go counterpart.** Mechanisms 1 and 2 revoke correctly but cannot between them *answer* correctly: in the very window mechanism 1 exists to cover, the tick is in neither place a `Stop` looks — `when` is already 0 and the buffer is still empty — so `Stop` would revoke the tick and then report that there had been none. Go never reaches that state, because a sync-mode chan timer nobody is receiving from is not in a heap at all (`timer.needsAdd` requires `t.blocked > 0`) and therefore never fires; eager firing is what opens the window, so eager firing is what has to close it. `offered` is set under the heap lock where the firing is committed, cleared under `sendLock` where it resolves, and read by `Stop`/`Reset`, which hold both. Together the three cover every place a tick can be: scheduled, committed-but-unsent, or buffered. A tick handed **directly** to a parked receiver is none of them — but that receive already committed, and it committed while the sender held `sendLock`, hence strictly before the `Stop` waiting on that lock could return; the guarantee is about ticks received *after* the call, so a direct hand-off falls inside it rather than being an exception.

The channel side is a general hook, not a `time` special case: golib gains `IChannelTimer` (Go's `hchan.timer`), a channel installs its owner with `AttachTimer` before the timer is armed, and `channel<T>.Capacity`/`Length` return 0 while the owner answers `HidesBuffer` — asked **live**, because `GODEBUG=asynctimerchan` selects the model at every observation, exactly as `runtime.chanlen`/`chancap` re-read the setting. `IsUnbuffered` deliberately still reports the *physical* shape (a timer channel's send does not rendezvous), since Go exposes only `cap()`.

Two places are deliberately **not** Go. Upstream `Stop` drains after releasing `sendLock` and `Reset` drains before releasing it; this holds `sendLock` across the drain in **both**, because Go can afford the looser order only thanks to lazy heaping (a sync-mode chan timer is in a heap only while a receiver is blocked on it, so nothing can produce a tick between `Stop`'s unlock and its drain) and this model's service thread is always live and always eager — a `Reset` racing another goroutine's `Stop` could otherwise have its fresh tick drained by the `Stop`. And Go fires a sync-mode chan timer **lazily** (`runtime.maybeRunChan`, at receive time) where this fires eagerly from the service heap; that is what mechanism 3 exists for, and *with* it `Stop`'s answer is the same either way — but eager firing still costs Go 1.23's early GC of unreferenced timers, since an armed timer stays reachable from the service heap. `GODEBUG=asynctimerchan` selects the model as upstream: `0` (default) synchronous, `1` pre-1.23 asynchronous (package `time`'s own `syncTimer` withholds the channel, so none is ever registered), `2` asynchronous semantics over a registered channel — and modes 1 and 2 run the pre-existing model unchanged, every branch being gated on the live setting. Measured on `time`'s own `TestChan`: all six subtests (`asynctimerchan={0,1,2}` × Timer/Ticker) pass, where before only `{1,2}` did. Guarded end-to-end by the `SyncTimerChannel` behavioral project, whose stdout is byte-compared against `go run`: `Stop`/`Reset` `pending` after a fire, no stale tick, `len`/`cap` 0, `AfterFunc` untouched, and three things a single-shot test cannot see: 200 `Reset`-to-imminent timers that must still deliver (the counter-property that keeps the drain honest), 600 ticker `Stop`/`Reset`-vs-firing races that must revoke exactly nothing, and two 600-timer batches armed against ONE absolute deadline and stopped/reset at that instant, every one of which must report `pending`. The last is what exposed mechanism 3's absence, and it only samples the window because the batch and the caller's sleep share a deadline — give each timer its own relative duration and the caller wakes milliseconds after the last tick has already landed, so the window is never entered and a broken implementation passes.

**Reach.** Timers gate roughly 35 stdlib suites. `encoding/base64`'s `TestDecoderIssue3577` and four of `internal/singleflight`'s five tests flip to passing on this alone; `singleflight`'s fifth needs 1000 *simultaneously parked* goroutines, which was the separately-documented cooperative-scheduler limitation in golib's `goǃ` (a goroutine was a ThreadPool work item and held its thread while parked, so the 256-thread floor bounded how many could be parked at once), not a timer issue. The dedicated-thread executor retired that bound (`docs/phase4/DESIGN-cooperative-scheduler.md`).

## Memory and the GC

### The GC measurement surface — one recorder, one ring, one snapshot

The recorder is `golib/runtime/GcPauseRecorder.cs`, landed 2026-08-21; the design and measurements are in
[`docs/phase4/DESIGN-readmemstats-surface.md`](../../phase4/DESIGN-readmemstats-surface.md). `ReadMemStats`
and `readGCStats` used to answer independently, and both left the per-cycle facts zero — which made
`runtime/debug`'s `TestReadGCStats` fail on two *length* assertions comparing the two surfaces to each
other. The cheap way to make those pass is to report `NumGC = 0` on both; that is refused, because it
would destroy a fact the CLR genuinely measures in order to satisfy an assert. Instead one definition
is applied uniformly — **a Go GC cycle is a CLR gen2 collection**, which is what `NumGC` already meant —
and one recorder supplies the missing half:

* the mechanism is a **resurrecting finalizable sentinel**: an object nothing strongly references,
  whose finalizer records the collection and then calls `GC.ReRegisterForFinalize(this)`, so it wakes
  once per gen2 collection. `GC.RegisterForFullGCNotification` was refused (it requires background GC
  off, process-wide), an in-process EventPipe listener was refused (events arrive ~117 ms late), and
  polling from each read was refused (it loses every collection between two reads, which is a hole in
  a ring whose slots are indexed by cycle number);
* the ring is written in **Go's own order** — slot `observed % 256`, *then* the counter — so
  `MemStats`' documented "the most recent pause is at `PauseNs[(NumGC+255)%256]`" and `ReadGCStats`'
  backwards walk line up by construction rather than by agreement;
* **`NumGC` is the recorder's count, not `CollectionCount`**, so the two surfaces cannot disagree. It
  can lag the true gen2 count by at most one collection, for at most the finalizer's scheduling
  latency — understating, never inventing — and `runtime.GC()`/`debug.FreeOSMemory()` **drain** the
  recorder before returning, which closes the lag at the one boundary Go's tests read it across;
* **`HeapReleased = max(0, committedHighWater − currentCommitted)`** over `TotalCommittedBytes`. A
  *cumulative* decrease would be monotone; Go documents the field as a current quantity that falls
  when the heap reacquires, and the monotone form was measured drifting ~33.6 MB per release cycle;
* **`ReadMemStats` is allocation-free**, and that is a landing precondition rather than a nicety:
  `net/textproto`'s banked `TestReadMIMEHeaderAllocations` brackets each header read between two
  `ReadMemStats` calls. `GC.GetGCMemoryInfo()` allocates a `GCMemoryInfoData` box per call (288 B
  measured), so the committed/heap-size figures come from the recorder's own per-gen2 sample and the
  ring is copied into the caller's already-allocated `array<T>` backing. Guarded at **zero** by
  `GolibTests.GcMeasurementSurfaceProbes.ReadMemStatsPerCallAllocation`;
* always on, armed from `runtime`'s (and `runtime/debug`'s) module initializer, with a
  `GO2CS_GC_PAUSE_HISTORY=0` escape hatch that restores the pre-recorder answers exactly. Measured
  cost: one finalizer run and one `GetGCMemoryInfo` call per gen2 collection — below the noise floor
  of a 1.25–1.64 ms collection.

Guarded by `GolibTests.GcPauseHistorySurfaceTests` (the two surfaces held against each other,
`HeapReleased` across a `FreeOSMemory`, `NumForcedGC`, and the refused-fields-stay-zero rule).

### `internal/weak.Pointer` — the CLR already has weak references, so the runtime handle becomes one

> **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 `internal/weak` it was written
> against).** The package is the public `weak`, and `Strong` became `Value`; the hand-own is
> [`src/core/weak/pointer.cs`](../../../src/core/weak/pointer.cs), on the same `WeakReference` design. It is
> no longer its package's only Go file (`doc.go` converts), so `weak` re-emits its `.csproj`,
> `package_info.cs` and `README.md`, and the layer beneath it is `internal/sync.HashTrieMap`.

`internal/weak` is `unique`'s liveness model, one layer below `internal/concurrent.HashTrieMap` and in
front of the same consumers. The package's entire body is two `//go:linkname` declarations, and both
pushed bodies live in `runtime/mheap.go`:

```go
func Make[T any](ptr *T) Pointer[T] {
	ptr = abi.Escape(ptr)                                   // force the pointee onto the heap
	var u unsafe.Pointer
	if ptr != nil {
		u = runtime_registerWeakPointer(unsafe.Pointer(ptr))
	}
	runtime.KeepAlive(ptr)
	return Pointer[T]{u}
}

func (p Pointer[T]) Strong() *T { return (*T)(runtime_makeStrongFromWeak(p.u)) }
```

`registerWeakPointer` → `getOrAddWeakHandle` → `getWeakHandle` → `spanOfHeap` walks `mheap_` span
metadata to find or hang a `specialWeakHandle` off the span, and `makeStrongFromWeak` loads a word out of
that handle and **re-derives an object pointer from the address**. The managed model populates no span
metadata, and *"what object lives at this address?"* is a question the CLR does not answer at all — so the
pair is registered UNHONORABLE in `linknamePushTargets` and each half announces itself by name (see
[*A cross-package `//go:linkname` PUSH resolves per recorded disposition*](linkname-and-trampolines.md#a-cross-package-golinkname-push-resolves-per-recorded-disposition--forwarder-or-announced-panic)).
The announcement is what pointed at this hand-own; this is what it was pointing at.

**The ruling is the `sync.Mutex` / `internal/concurrent.HashTrieMap` precedent, and it fits better here
than anywhere it has been applied before, because the CLR has first-class weak references of its own.**
`src/core/internal/weak/pointer.cs` carries `[module: go.GoManualConversion]` and contains no span walk.
The contract translates clause for clause:

| Go's contract | Managed mechanism |
|:--|:--|
| `Make(ptr)` never fails; `Strong()` yields the ORIGINAL pointer while the referent is reachable, and nil once the collector has identified it unreachable — **before** a finalizer can resurrect it | `WeakReference<ж<T>>` over the `ж<T>` box, **SHORT** (`trackResurrection: false`); Go's handle likewise clears ahead of finalization |
| A weak pointer does not keep its referent alive | Nothing on the `Pointer<T>` → `handle<T>` → referent path is a strong reference |
| Weak handles are **unique and canonical per byte offset into an object**, so weak pointers made from pointers that compare equal compare equal — and pointers to different offsets within one object do not | A `ConditionalWeakTable` keyed on the referent ALLOCATION whose value is a `ConcurrentDictionary` keyed on the GO POINTER. `ж<T>`'s own `Equals`/`GetHashCode` ARE Go's pointer identity, including "two fields of one struct are different addresses" |
| Equality is retained after the referent is reclaimed | `Pointer<T>` holds the handle STRONGLY, so the handle outlives the referent and keeps answering — it just answers nil forever after |
| A weak pointer made after a resurrection is NEWLY UNIQUE | The table entry dies with the referent (that is what a `ConditionalWeakTable` key is), so a later `Make` mints a fresh handle |
| `abi.Escape(ptr)` — force the pointee out of the frame | Nothing to force: a `ж<T>` IS a heap allocation from construction, whatever its pointee's type |
| `runtime.KeepAlive(ptr)` | `GC.KeepAlive` — load-bearing, not decorative: the referent is reachable from `Make`'s frame only through the argument, and every use of it is finished before the return |

**Why the canonical table does not pin what it indexes — the one subtle claim.** A
`ConditionalWeakTable` is an EPHEMERON: its value is kept alive only while the KEY is independently
reachable, and edges *from* the value *to* the key do not count as reachability. The key is
`ж<T>.ReferentObject` — the same lifetime question `runtime.SetFinalizer` already keys on (`mfinal.cs`),
answered the same way: an element ref resolves to its backing storage, a field ref to the containing
allocation, and a standard heap box to itself. The value holds the `ж<T>` boxes strongly, as dictionary
keys, which is deliberate — for a field or element pointer the box is a per-expression view that would
otherwise die long before the struct does, and `Strong()` must keep returning it; the ephemeron makes
that safe. The handle holds only a `WeakReference`. Composing the three, **a box is reachable exactly
when its referent is**, so one plain `WeakReference` tracks the REFERENT's liveness for every pointer
shape, not merely the standard-box shape.

One shape is deliberately not modelled, and it is Go's error case too: a box that ALIASES A NATIVE
ADDRESS names unmanaged storage the collector does not own, so its managed reachability is not the Go
question. Go answers by faulting (`throw("getWeakHandle on invalid pointer")` — a non-heap address has no
span); here it would observe an eventual nil rather than a fabricated pointer, the safe direction.
Nothing in the converted corpus takes a weak pointer to one.

**A second, independent defect this closes.** `Pointer[T]` is written out rather than left to `[GoType]`,
because the generated struct equality is field-wise `==` **guarded on every type parameter carrying an
`IEqualityOperators` constraint** (`TypeGenerator`'s `hasEqualityOperators` → `AllGenericTypesHaveConstraint`),
and Go's `Pointer[T any]` carries none — so the emitted body was literally
`Equals(other) => false /* missing equality constraints */`. Two weak pointers to one object NEVER
compared equal, contradicting the type's own doc comment and silently defeating `unique.Make`'s
`m.CompareAndDelete(value, wp)`, which could therefore never match and never evict a dead entry.
Equality is the *whole reason* the runtime canonicalizes the handle, so it is hand-written here — and as
`IEquatable<Pointer<T>>`, which the generated form does not implement, so `EqualityComparer<Pointer<T>>.Default`
reaches it without boxing on every lookup. ⚠ **The gate itself is over-conservative and the defect is
corpus-wide**: it disqualifies a struct when ANY type parameter lacks the constraint, even when no
field's type mentions that parameter. `unique.Handle[T]` is the other confirmed victim — its single field
is a `ж<T>`, which defines `==` for every `T`, yet its generated `Equals` is `false` too, so
`unique.Handle` values never compare equal either. That is a generator fix rather than a hand-own, and is
left to its own arc.

**Guarding measurement.** `internal/weak`'s own suite now links and runs
(`go2cs -tests -test-action all "<GOROOT>/src/internal/weak" src/core/internal/weak`): **`TestPointerEquality`
PASSES against `go test`** — the canonicalization clause, the hardest one, validated end to end.
`TestPointer` and `TestPointerFinalizer` do not, and the reason is the roster's already-named
**`codegen-liveness`** class rather than the weak model: both hold the referent in a live C# local (`bt`)
across the `runtime.GC()` that is supposed to kill it, where Go's per-safepoint liveness maps drop it at
its last use. (Neither is *disclosable* — `TestPointerFinalizer` does not fail an assertion, it blocks
forever on `<-done` waiting for a finalizer that a still-rooted object can never queue — so
`internal/weak` does not bank.) A dedicated probe separates the two — a referent
created and dropped inside a `[MethodImpl(NoInlining)]` helper is reported collected, and only a weak
pointer that had `Strong()` called on it *earlier in the same frame* stays alive:

```
PASS  Strong() is nil once the referent is unreachable (never probed)
FAIL  Strong() is nil once the referent is unreachable (probed first)
```

with a self-keyed `ConditionalWeakTable` control and the two-level table control both collecting, so the
ephemeron reasoning above is confirmed rather than assumed. `unique` reads the same way from the other
side: every `TestHandle` subtest that gets far enough reports **only** `v0 != v1` (the `[GoType]` equality
gate above) and never `v0.Value() != v1.Value()` — i.e. both `Make` calls interned the *same* `ж<T>`, which
is exactly what canonical weak handles plus `LoadOrStore` are for.

`pointer.go` is this package's only Go file, so marking it makes `internal/weak` **fully hand-owned**: the
driver `continue`s on `unmarkedFileCount == 0` and stops re-emitting `internal.weak.csproj`,
`package_info.cs` and `README.md`, and no `pointer.cs.auto` review sibling is produced — the position
`internal/godebug` and `internal/concurrent` are already in. The marker census moves **39 → 40**.

### `runtime.Pinner` — a pin BIT keyed by the referent allocation, and Go's two-level cgo walk over managed values

**What the seam is.** Go's `Pinner` makes one promise with two observables. The promise — an object
is "not moved or freed until `Unpin`" — exists so an address can be handed to non-GC-aware code, and
the ADDRESS half is already unconditional in golib: an address is only ever minted by the `ж<T>`
`uintptr`/`void*` conversions, which pin the storage for the box's whole life, and a reachable box is
never freed. So the first hand-own made `Pin`/`Unpin` no-ops — right about the half no test measures
and wrong about the two halves every test does: the **pin bit** (`isPinned`, read by the cgo argument
check) and the **lifetime hold** (a pinned object stays alive until `Unpin`). Meanwhile the converted
`isPinned` nil-dereferenced in `spanOf` (`mheap_.arenas` is never allocated) and the converted
`cgoCheckPointer` returned silently at `debug.cgocheck == 0` — Go's default of 1 is set by
`parsedebugvars` on the `schedinit` path the managed host never runs, the same silently-unreached
init as `internal/cpu`'s feature flags.

**The emitted form.** Five bodies are displaced through `manualConversionFuncs["runtime"]` —
`Pinner.Pin`, `Pinner.Unpin`, `isPinned`, `pinnerGetPinCounter`, `cgoCheckPointer` — into one flat
marked companion, `core/runtime/pinner_impl.cs`; `setPinned`, `unpin`, `pinnerGetPtr`,
`cgoCheckArg` and `cgoCheckUnknownPointer` stay converted and dead behind them (the `mfinal.cs`
"vestigial machinery" precedent). The pin is a COUNT in a `ConditionalWeakTable` keyed by the
pointer's `INilPointer.ReferentObject` — a standard box is its own referent, an element reference's
is its canonical backing, a field reference's is its source allocation — which is Go's per-object
span index one level down: pinning `&sl[0]` pins the whole backing, so `isPinned(&sl[1])` and the
slice header's array word both read pinned, and pinning an interface CELL does not pin the pointer it
holds. The table is weak; the hold is the pinner's own list of referents (Go's `refs`, one level
down), a partial-part field on the converted `pinner` struct so it rides with the box through
resurrection into the leak finalizer, which goes through the hand-owned `SetFinalizer` bridge and
calls the converted `pinnerLeakPanic` variable so a test's swap is observed. No CLR pin is taken, no
byte lands on `ж<T>` (a GolibTests arm asserts the box's field set), and nothing is written into
`ManagedPointerTokens` — that record is weak, per-projection and about the address-take, and
`TestPinnerSimple` takes `unsafe.Pointer(p)` (which registers) BEFORE asserting `!IsPinned`; the
record is only READ, through `Resolve`, for a bare number.

**The cgo check.** `cgoCheckPointer(ptr, arg)` is Go's rule verbatim: every Go pointer word at
level 1 must be pinned, every Go pointer word at level 2 must be pinned, level 3 is not inspected
(`cgoCheckArg` walks the argument's pointee by its `GoType` structure; `cgoCheckUnknownPointer`
reads a pointee's words without descending). A `NativeBox`, a native-backed slice and a number
nothing resolves are not Go pointers; a channel, a map and a closure are Go pointers to unpinnable
heap objects and always fail; a string literal's bytes are a heap `byte[]` here where Go's are
RODATA — the one divergence the runtime suite reaches, disclosed as `runtime-capability` on
`TestPinnerConstStringData`'s exact signature. `GODEBUG=cgocheck=0` disables the check, read once.

**One converter shape tolerated.** `internal/fmtsort`'s test init passes
`reflect.ValueOf(ch).UnsafePointer()` to `Pin`, and the converter wraps that `unsafe.Pointer`-typed
call result in `(uintptr)` on its way into the `any` parameter — so `Pin` accepts a `uintptr` as the
projected form of a pointer, resolving it through the record and no-op'ing on a miss. Routed to the
Q49 bridge class; the accommodation retires with the converter fix. Design, per-row classification
and the prediction on record: `docs/phase4/DESIGN-runtime-pinner.md`; guards: GolibTests
`RuntimePinnerTests` (every "passes" arm followed by the same check with the pin removed, which must
go red), and runtime's own `pinner_test.go` through the `-tests` pipeline.

## Synchronization

### `sync.Pool` — a managed-reference ring slot, and a thread-affine stand-in for the P pin

`sync.Pool` is the third shape of the same wall, and the most instructive: **the raw-metal type is not
a pointer-in-an-integer, it is the `any` itself.** Go's `poolDequeue` is a lock-free ring of `eface`
slots — the two-word `{type, value}` form of an interface — and its whole ownership protocol hangs on
the TYPE word: a slot is empty **iff** `typ == nil`, and a consumer publishes "done with this slot" by
atomically storing nil into `typ` alone, leaving `val` to be overwritten later. Under the CLR an `any`
is ONE reference, so the literal conversion reinterprets the two-word struct as an `any`
(`Unsafe.As`) and the `typ` word ends up doing double duty as both the type tag and the value. Two
failures follow immediately, and both were observed: a stored value read back through the
reinterpretation surfaces as its own type word (`panic: interface conversion: interface {} is
unsafe.Pointer, not int`), and the empty-slot sentinel — a nil `unsafe.Pointer` — is indistinguishable
from a *stored value of that type*, so `pushHead` and `popTail` disagree about who owns a slot and the
ring corrupts or wedges. `TestPoolChain` took the whole test host down with the panic above, which cost
the 14 tests that sort after it alphabetically.

The fork is confined to the **slot representation** and keeps every other line of Go's algorithm —
the packed `head`/`tail`, the fullness test, the CAS protocol, the single-producer/multi-consumer
contract, and the entire `poolChain` half:

```csharp
// eface is Go's two-word {type, value} representation of an `any`. Under the CLR an `any` IS a single
// managed reference, so the slot holds that reference directly.
[GoType] partial struct eface {
    internal any? val;
}
```

`null` is the empty-slot sentinel — a state no stored value can forge — and a private singleton stands
in for Go's typed-nil `dequeueNil(nil)` marker (a slot holding a *nil interface value* must still read
as occupied). That also collapses Go's two-step release into one: with a single word there is nothing
to tear, so **one** `Volatile.Write(ref slot.val, null)` both clears the value and hands the slot back
to the producer, where Go needs a value store followed by a publishing `atomic.StorePointer` on `typ`.
`TestPoolDequeue` proves the release protocol end to end — 2·10⁶ items through a **fixed** 16-element
ring, with the head/tail seeded 500 short of wrapping.

`Pool` itself is a whole-file hand-own for a different reason: its `[P]poolLocal` shard block is
reached by **pointer arithmetic** through an `unsafe.Pointer` (`indexLocal`), which is meaningless when
the block is a managed array, and `procPin` — the thing that gives each shard's dequeue its *single*
producer — has no P to pin to. Go's algorithm survives intact (private slot → the shard's shared chain
→ steal from other shards' tails → the victim cache; `poolCleanup` ageing local → victim → dropped);
three pieces are replaced:

| Go mechanism | Managed realization | Divergence |
|---|---|---|
| `[P]poolLocal` block + `indexLocal` pointer arithmetic | a `poolLocal[]` held directly, one heap object per shard | the cache-line pad against false sharing is gone — separate objects, nothing to pad |
| `procPin()` → P id, preemption off | **thread-affine** index: a thread draws a sticky id once, in arrival order, folded into the current `GOMAXPROCS`; `procUnpin` has nothing to undo | threads outnumber shards, so two threads CAN share one shard — the one thing a real pin rules out. Closed on Pool's side: the private slot is claimed/taken with a single `Interlocked` step, and the shard's shared-chain HEAD (single-producer by contract) is serialized by a per-shard producer gate. Stealing (`popTail`) stays lock-free, as designed |
| `poolCleanup` at the start of every GC cycle, world stopped | registered with the runtime exactly as Go registers it (`runtime_registerPoolCleanup` → `runtime.GC()` invokes the hook, mirroring `gcStart` → `clearpools`) | narrower trigger: **requested** collections age the pool, automatic CLR collections do not, so a program that never calls `runtime.GC()` retains its cached items longer than Go's would. And the swap runs on the caller's thread, not under STW, so a `Put` racing it can land in a shard that just became a victim and age one cycle early. Both sit inside Pool's contract — *any item may be removed at any time* — so they cost a cache hit, never correctness |

The registration path is worth noting because it is a **general** cross-assembly constraint, not a Pool
detail: sync reaches the runtime through `//go:linkname runtime_registerPoolCleanup`, whose target
`sync_runtime_registerPoolCleanup` the exported-ness rule makes `internal` to the runtime assembly — and
[a linkname forwarder cannot bind an internal target across assemblies](linkname-and-trampolines.md#a-cross-package-golinkname-pull-emits-a-forwarder-not-a-throwing-stub).
A one-line `public` shim in `runtime/managed_impl.cs` is the crossing point, the same remedy
`blockUntilEmptyFinalizerQueue` already uses in `mfinal.cs`.

**What the arc could NOT satisfy, stated precisely.** `TestPoolGC` asserts that after draining a Pool
and collecting, at least `N-1` of `N` finalizers have run — a budget of exactly **one** straggler still
reachable "on stack or elsewhere". A drained Pool here holds nothing (proven with a `WeakReference`
probe: run the drain in its own frame and **zero** of 100 survive), but the test's own frame spends the
budget twice under an **unoptimized** build: the fill loop's `v` keeps the last item, and the JIT's
MinOpts codegen keeps the *discarded* return value of the final `Get()` reachable from the calling
frame for the rest of the method. That second straggler reproduces with no Pool in sight — a factory
whose result is discarded in a loop leaves its last object alive in Debug — and the test passes in
`Release`, where the JIT's liveness is precise. So it is a codegen-liveness divergence of the
Debug-configured CLR, not a Pool defect; the honest classification is the disclosed-divergence class,
not a contortion of the drain order to make one assert land. That class is now
[named and pinned](../test-host-and-disclosures.md#codegen-liveness--a-frame-holds-what-go-has-already-dropped) as
`codegen-liveness`, with `TestPoolGC`'s straggler count pinned exactly so a real Pool retention
regression cannot hide behind the disclosure.

### `sync.Cond`'s copy detector, on reference identity rather than an address

Go's `copyChecker` is a `uintptr` that stores **its own address** and compares:

```go
if uintptr(*c) != uintptr(unsafe.Pointer(c)) &&
   !atomic.CompareAndSwapUintptr((*uintptr)(c), 0, uintptr(unsafe.Pointer(c))) &&
   uintptr(*c) != uintptr(unsafe.Pointer(c)) {
	panic("sync.Cond is copied")
}
```

Neither half survives conversion, and the two failure modes point in opposite directions:

* Storing an **address** is unsound on a moving collector. The GC relocates the box holding the
  `Cond`, so a compaction between two `Wait` calls would leave a stale word in a `Cond` nobody
  copied — a *spuriously panicking* condition variable, which is far worse than no check.
* The auto body is **inert**. The address-of-self operand converts to
  `@unsafe.Pointer.FromRef(ref c)` while the CAS destination converts to `Ꮡ((uintptr)(c))`, which
  boxes a **copy** of the value — so the compare-and-swap writes to a throwaway box, the checker is
  never initialized, and `check()` never panics however often the `Cond` is copied. `TestCondCopy`
  failed with `got <nil>, expect sync.Cond is copied`.

`cond_impl.cs` (registered as `manualConversionFuncs["sync"]["copyChecker.check"]`, so only this one
method is hand-owned — the `copyChecker` type and every `Cond` method stay auto) asks the same
question against the invariant the CLR does guarantee: the checker stores a token derived from its
`ReferentObject`'s GC-stable identity hash. Every `Cond` method reaches its checker as
`Ꮡc.of(Cond.Ꮡchecker)`, whose referent is the *root* allocation holding the `Cond` — the same object
on every access, including for a `Cond` embedded in a larger struct, which is exactly why the
referent projection must recurse. A struct copy carries the ORIGINATING allocation's token into a new
one, which is what `check()` sees.

Two deliberate simplifications, both in the safe direction:

* **No compare-and-swap.** Go needs one because concurrent first-users race to publish the word;
  here every racer computes the *same* token for the same allocation, so an aligned native-word store
  cannot publish a value any racer would disagree with.
* **Identity hashes are not unique**, so a copy between two colliding allocations goes unreported —
  a missed detection, never a false alarm. Likewise, two elements of one backing array both resolve
  to that array, so a copy between them is invisible (an element pointer's identity is the storage
  plus an index the checker has no room for). Recorded, not worked around: Go's own check is
  documented as best-effort.

### `internal/concurrent.HashTrieMap` — a managed map where Go seeds itself from `MapType().Hasher`

> **At Go 1.24.13 (dated amendment; the section below is the Go 1.23 surface it was written against).**
> The package moved to `internal/sync`, and the hand-own is now
> [`src/core/internal/sync/hashtriemap.cs`](../../../src/core/internal/sync/hashtriemap.cs), whose header
> records the 1.24.13 surface: `NewHashTrieMap` is gone and the zero map is seeded by `init`/`initSlow`
> on first touch (still from `abi.TypeOf(m).MapType()`'s `Hasher`); `V` widened to `any`, so
> `CompareAndSwap` and `CompareAndDelete` panic up front for a non-comparable `V`; `keyEqual` is gone;
> seven methods were added (`Clear`, `CompareAndSwap`, `Delete`, `LoadAndDelete`, `Range`, `Store`,
> `Swap`); and `internal/sync` is no longer fully hand-owned, because `mutex.go` and `runtime.go`
> convert. At 1.24 the same map also backs every `sync.Map` by default (`goexperiment.synchashtriemap`).

`internal/concurrent` is the whole of `unique`'s storage, and `unique` is `net/netip`'s address interner —
so this one type sits in front of `unique`'s entire suite, `net`'s last package-initializer root and
`encoding/gob`'s `TestNetIP`. Go 1.23's implementation is a lock-free hash-trie, and **every bit of its
behavior comes from one runtime descriptor read**:

```go
func NewHashTrieMap[K, V comparable]() *HashTrieMap[K, V] {
	var m map[K]V
	mapType := abi.TypeOf(m).MapType()
	ht := &HashTrieMap[K, V]{
		root: newIndirectNode[K, V](nil), keyHash: mapType.Hasher,
		keyEqual: mapType.Key.Equal, valEqual: mapType.Elem.Equal,
		seed: uintptr(rand.Uint64()),
	}
	return ht
}
```

`Hasher` is a raw function pointer into the hashing machinery the compiler emits for `map[K]V`;
`Key.Equal`/`Elem.Equal` are its matching bit-compare thunks. All three take `unsafe.Pointer`s and mean
*"hash / compare the bytes AT this address"* — and **the managed reflection bridge cannot honor that
contract**. An address in the CLR names no value: two boxes holding equal strings sit at different
addresses, and a pointee containing references moves across a GC. An address-derived hash would therefore
stop `unique.Make("hello")` agreeing with itself, which is the exact inverse of the package's purpose.

Populating `Hasher` anyway — with anything plausible — is barred by the **inverse of the atomic rule**: a
descriptor field whose read cannot be honored must stay EMPTY, because a half-populated descriptor converts
a loud construction failure into a map that is silently wrong. (Reflection increment 8 rooted the row there
and reported it *not landable in the bridge*.) So the literal conversion compiles and can never run:
`NewHashTrieMap` threw inside the package initializer of every `unique` consumer, taking `net/netip` and
every dependent with it.

**The ruling (2026-08-03) is the `sync` precedent applied one level up: hand-own the whole file, and keep
the SEMANTICS rather than the mechanism.** `sync`'s Mutex/RWMutex/WaitGroup are reimplemented on
`SemaphoreSlim`/monitors because Go's sleeping semaphore is co-designed with the state machine and cannot be
emulated; here the coupling is to the descriptor surface instead of to the scheduler, but the fork is the
same one — the raw-metal arm of the S1 fork (see
[`Baseline-vs-FullConversion.md`](../../../src/archived/Baseline-vs-FullConversion.md)). `src/core/internal/concurrent/hashtriemap.cs`
carries `[module: go.GoManualConversion]` and contains **no trie at all**. The exported API and its
concurrency contract are preserved exactly; the store is a `ConcurrentDictionary`, whose guarantees line up
member for member:

| Go member | Managed mechanism | Semantic note |
|:--|:--|:--|
| `NewHashTrieMap[K, V]()` | `Ꮡ(new HashTrieMap<K, V>(store: new mapStore<K, V>()))` | the store is a CLASS, so a by-value copy of the struct shares one map — exactly what Go's `root *indirect[K,V]` pointer gives |
| `(*HashTrieMap).Load(key)` | `TryGetValue`; miss returns `(*new(V), false)` as `@new<V>().ValueSlot` | `[GoRecv]`, so the RecvGenerator still mints the `ж<…>` overload `unique` binds |
| `(*HashTrieMap).LoadOrStore(key, value)` | `TryGetValue` → `TryAdd` retry loop | exactly one caller of a racing set observes `loaded == false`; `GetOrAdd` is a single call but cannot report WHICH outcome occurred, and `unique.Make` depends on that answer |
| `(*HashTrieMap).CompareAndDelete(key, old)` | `ContainsKey` gate → `TryRemove(KeyValuePair)` | the pair overload is an atomic compare-and-remove under `EqualityComparer<V>.Default`; the gate reproduces Go's order (a missing key returns false *without* comparing values) |
| `(*HashTrieMap).All()` | closure over the store's enumerator | ConcurrentDictionary's enumeration is **weakly consistent** — never throws on concurrent mutation, visits each live key once, promises no order — which is Go's documented contract verbatim, and is what lets `unique`'s cleanup pass `CompareAndDelete` while it walks |
| zero `HashTrieMap` | `storeOf` lazily installs the store (`Interlocked.CompareExchange`) | Go 1.23's zero value is unusable (nil `root`/`keyHash`); nothing depends on that panic, and the same `gateOf` idiom `sync.Mutex` uses removes a whole class of null dereference |
| `keyHash` + `keyEqual` | `EqualityComparer<K>.Default` | see below — verified to BE Go's `==` for every key shape the corpus interns |
| `valEqual` | `EqualityComparer<V>.Default`, guarded by `mustBeComparable` | Go's value comparison panics for an INTERFACE `V` holding an uncomparable dynamic type (`V comparable` admits `any` since Go 1.20, moving the check to run time); the guard mirrors that panic instead of letting the comparer answer a question Go refuses to. Inert for a non-interface `V`, resolved once per instantiation |

**The equality/hash bridge is the correctness question, and it was measured, not reasoned.** Go hashes and
compares keys by K's own `==`; the managed implementation uses `EqualityComparer<K>.Default`. For every key
shape the converted corpus actually interns these agree:

* **`ж<T>`** (`unique`'s own `map[*abi.Type]any`) implements `IEquatable<ж<T>>` as pointer IDENTITY with a
  matching identity hash, and `abi.TypeFor<T>()` interns one descriptor box per `System.Type` — so one Go
  type always presents one key, and a second `TypeFor` call finds the first call's entry.
* **A `[GoType]` struct** — `net/netip`'s `addrDetail{isV6 bool; zoneV6 string}`, the shape `unique`
  actually interns — carries a generated field-wise `Equals` over `==` plus a `HashCode.Combine` of the
  same fields, which is Go's struct `==` exactly. It does **not** implement `IEquatable<T>`, so
  `EqualityComparer<T>.Default` routes through the `object` override; that lands on the same comparison, at
  the cost of one box per lookup.
* **`@string`** compares and hashes by CONTENT, as Go's string `==` does — verified with two keys built
  from distinct backing storage.

**Two further walls sit BEHIND this one**, both uncovered by making `unique` reachable for the first time
and both outside this file:

1. ~~**A cross-assembly `//go:linkname` PUSH never links.**~~ **CLOSED** — see
   [*A cross-package `//go:linkname` PUSH resolves per recorded disposition*](linkname-and-trampolines.md#a-cross-package-golinkname-push-resolves-per-recorded-disposition--forwarder-or-announced-panic)
   above. The forwarder machinery handled the PULL direction only (a bodyless declaration naming another
   package's symbol); `runtime` pushes the other way —
   `//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup`
   (`mgc.go`), `//go:linkname internal_weak_runtime_registerWeakPointer internal/weak.runtime_registerWeakPointer`
   (`mheap.go`) — and the *consuming* package's bodyless declaration was left for the
   [`PartialStubGenerator`](../source-generators.md#source-generators) to fill with `NotImplementedException`. `unique`'s
   registration now FORWARDS to runtime's converted body; `internal/weak`'s two halves stay unlinked **by
   ruling** (the pushed bodies walk `mheap_` span metadata) and announce the linkname pair rather than
   fabricate one. The remedy they name — a hand-owned managed weak reference — has since landed; see
   [*`internal/weak.Pointer`*](#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one)
   below.
2. **`abi.TypeFor<T>()` is silently WRONG for an interface `T`.** Its non-interface branch returns an
   interned descriptor; the interface branch is `TypeOf((*T)(nil)).Elem()`, and `Type.Elem()` for
   `Kind == Pointer` reinterprets the descriptor as a `PtrType` (`Ꮡt.Reinterpret<Type, PtrType>()`) and
   reads `.Elem` — which under the managed layout lands on the descriptor's `Equal` field. `TypeFor<any>()`
   and `TypeFor<error>()` therefore return a `System.Func<unsafe.Pointer, unsafe.Pointer, bool>`, not a
   `ж<abi.Type>` at all. Shared generics let that object be *stored* into
   `ConcurrentDictionary<ж<abi.Type>, any>` without a cast check, and the first real key comparison then
   dispatches `IEquatable<ж<abi.Type>>.Equals` on a delegate → `EntryPointNotFoundException`. The old trie
   never dispatched anything on a key's runtime type (it compared raw addresses through `keyEqual`), which
   is why a corpus-wide bridge defect could hide behind it. **Not hardened against here on purpose** —
   tolerating a type-unsafe key would be the same "plausible but fake" move the inverse-atomic rule
   forbids; the loud failure is the correct behavior and the fix belongs in `abi`.

**Guarding measurement.** `encoding/gob` holds at **95 of 106** and `TestNetIP`'s root moves from
`NewHashTrieMap` → `ArgumentException: Delegate to an instance method cannot have null 'this'` to the
linkname stub above (no row regresses; `TestNetIP` is the only gob row whose closure reaches `unique` at
all). `unique` itself goes **0 → 1 of 19** and, more usefully, stops being a one-root wall: its 15 identical
`TypeInitializationException` rows resolve into five distinct downstream roots (the two linkname pushes, a
`GCHandle: Object contains references` on `abi.Escape`, an `IndexOutOfRangeException` in `makeCloneSeq`'s
`slice<T>` enumeration, and the `TypeFor` hole above). The hand-owned file is also its package's only Go
file, which makes `internal/concurrent` fully hand-owned — see
[`Baseline-vs-FullConversion.md`](../../../src/archived/Baseline-vs-FullConversion.md) for what that does to the package's
`.csproj`/`package_info.cs`/`README.md`, and for the seeded-reconvert proof in both directions.

---

[← Manually-Converted Declarations](../manual-conversions.md) · [Index](../README.md)
