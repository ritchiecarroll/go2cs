# DESIGN — M:N: the `LockOSThread` pin (N7), priced

> **STATUS: PROPOSED (lane G, 2026-11-20; branch `claude/g-mn-proto-b1`, cut from P4's `claude/p4-mn-byref`
> `e178ab8a1e` with master `1fa80494ee` merged in).** Design only: no code lands from this document until COORD
> rules it. It answers item 2 of the re-sized M:N plan: what `runtime.LockOSThread` means on the scheduler, what a
> pinned goroutine costs a carrier, and how the pin meets N5. Predictions and stop lines are in §6 and §7, written
> before any arm runs. Amended by dated blocks, never rewritten.
>
> **Revision 1 (section 10, 2026-11-20): P3's review folded in; COORD approved the asks of section 9 on it.**
> **Revision 2 (section 11, 2026-11-20): P3's re-read of section 10 folded in.** The build waits on the owner's M:N
> landing shape.

## 1. The gap

[DATA-mn-spike-2](DATA-mn-spike-2.md) N7: `runtime.LockOSThread` "still only counts". Row T8 promises "a dedicated
thread until `UnlockOSThread` or the goroutine's end", and M5 says "build N7's pin before any default-on ... a
pinned goroutine keeps a thread of its own, as on today's path". Until the pin exists, T1 (thread identity) has no
opt-out under the scheduler.

What golib has today, read on this branch:

| piece | where | what it does |
|---|---|---|
| the count | `Goroutine.NoteLockOSThread` / `NoteUnlockOSThread`, `LockedToThread` | golib's copy of Go's `m.lockedExt`, mirrored by runtime's hand-owned `LockOSThread` / `UnlockOSThread` (`managed_impl.cs`) |
| init lock keeps main | `MainThread.Run`; `Goroutine.RunMainTwin` refuses to multiplex a locked goroutine 1 | Go's "init functions run on the startup thread" rule |
| exit while locked | `GoroutineThreadPool.RegisterRetire` | a pooled thread whose goroutine ended locked is not reused |
| the scheduler | `GoScheduler` | none: a twin that locked and then parks resumes on whichever carrier takes its step |

So on the scheduler's path today a locked twin migrates at its next park. Off the scheduler (and for any goroutine
without a twin) every goroutine owns its thread, and the count is all Go's contract needs.

## 2. What Go does (proc.go, the contract to match)

- `LockOSThread` wires the calling goroutine to its current thread (`m.lockedg`, `g.lockedm`). Until the matching
  `UnlockOSThread` the goroutine always runs on that thread, and **no other goroutine runs on it**.
- When a locked goroutine parks, its thread does not look for work: `schedule` finds `mp.lockedg` and calls
  `stoplockedm`, which hands the P off (`handoffp`) and sleeps the thread on `m.park`.
- When it is readied, whichever thread finds it runnable calls `startlockedm`: it hands its own P to the locked
  thread and wakes it. The locked goroutine runs holding a P like any other.
- A goroutine that exits locked takes its thread with it (the thread is terminated, not reused).
- An init function's lock keeps `main` on the startup thread.
- Nesting is a count (`lockedExt`); the runtime's internal `lockedInt` is a second count golib does not see.

Cost in Go: a locked goroutine's wake is a thread wake, not a goroutine switch. That is the documented price of
the opt-out, and the reason Go code locks only where thread identity matters (thread-affine C libraries, GUI loops,
`setns`, per-thread credentials).

## 3. The design

**The pin is a carrier that stops serving the run queue.** A twin that is locked when its step ends keeps the
carrier that ran the step; the carrier waits for that goroutine's next step alone and runs it; when the goroutine
unlocks it returns to the pool; when the goroutine ends locked the carrier's thread ends with it.

### 3.1 Taking the pin (step end, no new hook at `LockOSThread`)

The lock needs no action when it is taken: the goroutine is on this carrier's thread already. The carrier reads
`LockedToThread` at the end of `RunStep`, after the unmount and **before** committing a pending park:

- locked and parking: `goroutine.m_pin = this`, then `slot.Commit(...)` as today. The pin is set first so a wake
  racing the commit routes to this carrier (the commit's own "already woken" path calls `Ready`, which reads it).
- locked and the goroutine ended: the carrier retires (§3.4).
- not locked: today's path, unchanged.

The goroutine still unmounts and mounts around every park. The pin decides **which thread**, not whether golib's
per-goroutine state is swapped: the 38 ns swap keeps the tracer, `MainExit`'s marks and the execution context
exactly as they are on the unpinned path.

### 3.2 Waiting pinned (the carrier's loop)

A pinned carrier leaves the shared loop for a private one:

1. Before it waits: anything in its run-next slot is queued globally (as `EnterBlocked` does); it counts itself
   **blocked** (`s_blocked++`) and calls `EnsureCarrier` when work is queued. The P was released by the step's
   `finally` as for any step.
2. It waits on a private wake (a one-permit `SemaphoreSlim` and a step field) that only `Ready` for its goroutine
   fills. It never touches `s_runq` or `s_work`.
3. On the wake: `s_blocked--`, take a P (`s_p.Wait`, counted in `s_pWaiters` as today), `RunStep` the delivered
   step. Back to 3.1.

Counting the pinned carrier as blocked is the N5-shaped part of the design (§5): without it, P pinned goroutines
parked at once would leave no carrier for the run queue and the program would hang.

### 3.3 Waking a pinned goroutine (`Ready`)

`GoScheduler.Ready` reads `goroutine.m_pin` first: when set, the step goes to that carrier's private wake and
`Ready` returns. Go's `startlockedm` hands the waker's P to the locked thread and stops the waker; here the pinned
carrier takes a P from the shared pool instead, so the waker keeps running and one thread wake is saved.

Two further reads close the run-next slot to the pin:

- a waker whose mounted goroutine is locked does not use its own run-next slot (its carrier is about to pin, and
  the slot would wait behind a thread that serves one goroutine);
- a pinned target never enters anyone's run-next slot (the `m_pin` check comes before the slot).

### 3.4 Unpinning and ending

- `UnlockOSThread` to zero mid-step: nothing happens until the step ends. At the end `LockedToThread` is false, so
  `m_pin` is cleared, the park commits through the shared queue, and the carrier goes back to the shared loop (a
  surplus carrier then retires when idle, as today).
- The goroutine ends while locked: the carrier removes itself from `s_carriers`, fixes `s_blocked` if it counted
  itself, and returns from `Loop`, ending the thread. This matches Go's rule and `RegisterRetire`'s.
- `UnlockOSThread` with no lock outstanding does nothing (the count already refuses it).

### 3.5 What stays as it is

- Goroutines with no twin: they own a thread for life; the count is the whole contract (today's path).
- Plain blocks inside a pinned step (`EnterBlocked`): they already block this thread and hand the P off. The pin
  is kept by construction.
- The monitor's retake (a step inside a bracketed native call) and injection: a pinned step is a step like any
  other; both apply unchanged.
- An init function's lock: `RunMainTwin` already refuses to multiplex goroutine 1 (§1).

### 3.6 Size

golib only: `GoScheduler.cs` (the private loop, the `Ready` route, the step-end check; about 80-120 lines),
`Goroutine.Multiplexed.cs` (`m_pin`, about 5), readings (`pins`, `pinned-wakes`, `pinned-peak`), and GolibTests.
No converter, generator or public-surface change: `LockOSThread` keeps its signature, and `PublicSurface.golib.txt`
does not move (it is a stop line, §7).

## 4. Named limits

| # | limit | why it stays |
|---|---|---|
| L1 | a consumer's own C# `async` code resumes where the BCL puts it | T8's row already says so: the pin covers converted Go code only |
| L2 | a goroutine 1 that locks **after** it started still ends on the startup thread (`RunMainTwin` takes it back to exit) | it happens after `main` returned; nothing Go runs is thread-affine at that point. Named, not built |
| L3 | the runtime's internal `lockedInt` is not mirrored | golib sees `lockedExt` only. Converted runtime code that takes the internal lock on a twin is listed in the census of §6 (M6); none expected |
| L4 | a callback on a foreign thread has no goroutine to pin | N6/M7, the seam's gap; unchanged |
| L5 | an off-carrier resume of a pinned twin would run its code on a foreign thread | the `off-carrier-resumes` reading is zero unless something other than a twin or a park was awaited; M4 asserts it is zero for pinned goroutines |

## 5. How the pin meets N5

N5 is blocking the scheduler cannot see. The pin changes three things and leaves one alone.

1. **It adds a hazard, and the design closes it.** A pinned carrier waiting for its goroutine serves nothing else.
   If it counted as an ordinary carrier, P pinned goroutines parked at once would leave the run queue with no
   carrier: a hang of exactly N5's kind, made by the scheduler itself. §3.2 counts it blocked, so `EnsureCarrier`
   adds one. M3's arm with K = 2P pinned parked goroutines is the check.
2. **It makes a pinned goroutine's parks visible and cheap for the others.** Every park of a pinned goroutine
   releases its P and costs a thread, not a P: the same trade as today's thread path.
3. **It cannot make invisible blocking visible.** A locked goroutine inside an unbracketed native call holds its
   P exactly as an unlocked one does, and locked goroutines are the population most likely to be in native code.
   The retake and injection apply to it unchanged. The pin is not a policy for N5's invisible blocking and must not
   be sold as one; that policy is still item 5's design.
4. **It stays out of N5's injection cap.** A pinned carrier is a blocked carrier, not an injected one, so it does
   not spend the injection budget (`s_overBudget`).

## 6. Pricing: the arms

All on this box, JIT Release, the scheduler on (`GO2CS_MN=1`), each arm in its own process, 5 runs, medians with the
range. Controls first: an A/A pair of the base on each reading, so a delta is read against the box's own noise.

| arm | what | shape |
|---|---|---|
| M0 | A/A | the base (`9a515ef918`, this branch before the code) twice on M1's and M2's readings |
| M1 | unlocked cost | spike-2 §3.6's park readings (unbuffered ping-pong, never-park send/receive) on base vs pin build |
| M2 | pinned park/wake | the §3.6 ping-pong with one side locked; with both sides locked; against the scheduler-off thread path and the unlocked twin |
| M3 | thread and carrier census | K = 1, 16, 256 locked goroutines parked on channels while 10,000 unlocked twins ping-pong; K = 2P as the hang check; peak threads and carriers |
| M4 | identity (the gate) | 100,000 parks per shape (channel recv, select, `time.Sleep`, `sync.Mutex`, `WaitGroup`, a twin call's return, a timer): the managed thread id is constant per locked goroutine; no other goroutine is ever seen on a pinned thread; pinned off-carrier resumes = 0 |
| M5 | exit and nesting | lock twice, unlock once, park, check the pin holds; unlock to zero, park, check it is released; end locked, check the carrier thread ended and was not reused |
| M6 | census | every `LockOSThread` / `lockOSThread` site in the converted stdlib that a twin can reach (L3) |

M4 and M5 go into GolibTests (driving the scheduler directly, as the existing M:N tests do) and a behavioral
fixture `MnLockPin`. The gate is made to fail before it is trusted (floor 13): remove the `Ready` route and M4
must name it; let a pinned carrier take the run queue and the "no other goroutine" assertion must name it; then
restore and verify byte-identical.

## 7. Predictions and stop lines

Written before any arm runs. The base numbers are spike-2 §3.6 (twin ping-pong 1,422-1,550 ns, thread path
1,677-1,715 ns, mount + unmount 38 ns, never-park iteration 24.9-25.8 ns).

| # | prediction | stop line (stop, report, no tuning in the seat) |
|---|---|---|
| P1 | unlocked ping-pong and never-park iteration: no move outside M0's A/A band (the added cost is two field reads, under 1 ns) | either moves by more than 2% or past the A/A band, whichever is larger |
| P2 | one side locked: round trip 1.6-3.0 µs, within 1.8x of the thread path | over 2x the thread path (about 3.4 µs): the private wait needs a spin phase, which is a ruling, not a fix in the seat |
| P3 | both sides locked: 1.7-3.5 µs (two thread wakes per round trip, as the thread path) | over 2.5x the thread path |
| P4 | K locked parked goroutines: peak carriers = P + K (+0 to 2); peak threads move by K (+0 to 4) against K = 0 | peak carriers over P + K + 4 |
| P5 | K = 2P: the unlocked ping-pong completes, rate within 10% of K = 0 | any hang (a 60 s deadline) or a rate drop over 25% |
| P6 | M4: 0 thread changes per locked goroutine over 700,000 parks; 0 foreign goroutines on a pinned thread; 0 pinned off-carrier resumes | any non-zero |
| P7 | M5: all three hold; the ended-locked carrier's thread is gone and the carrier count drops by one | any miss |
| P8 | M6: 0 stdlib sites take the internal lock on a twin-reachable path | any site: L3 becomes a design item |
| P9 | no change to `PublicSurface.golib.txt`, the converter or the generator | any change |

The falsifier for the design as a whole: if P2 or P3 fails with the wait already spinning as `SemaphoreSlim` does,
the pin costs more than today's path, and M5's "as on today's path" is not met by a carrier pin. The alternative is
then to hand a locked twin to a plain thread goroutine for the rest of its lock (a one-way move off the scheduler),
priced as a second design.

## 8. Lane-days

| piece | lane-days |
|---|---|
| golib code (§3) and readings | 1 |
| GolibTests M4/M5, fixture `MnLockPin`, the made-to-fail checks | 0.5-1 |
| arms M0-M3 and M6, the record | 0.5-1 |
| **total** | **2-3**, as the plan's line |

Nothing here waits on another lane. Item 3 (the twin-set rule as text) can start on this ruling: the pin does not
change which signatures admit a twin.

## 9. Asked of COORD

1. Rule the design (§3) and its limits (§4) as the pin to build on `claude/g-mn-proto-b1`.
2. Accept §7's stop lines as written, in particular that a P2/P3 miss stops the seat for a ruling rather than for
   tuning.
3. Confirm L2 (goroutine 1 locked after start ends on the startup thread) is named, not built.

## 10. Revision 1, after P3's review (dated block, 2026-11-20)

P3 reviewed `b3dc12ddbb` read-only against `GoScheduler.cs` at that commit: BLOCK on one point, four SHOULDs, one
nit. COORD ruled all five into this document; on this revision the three asks of section 9 are approved
(section 3 with L1-L5 as the pin to build, the stop lines with finding 4's change, L2 named only). Building still
waits on the owner's M:N landing shape. Where this block and sections 3, 6 or 7 differ, this block holds.

### 10.1 BLOCK: the idle retire rule counted pinned carriers (amends 3.2)

A surplus shared carrier retires after 100 ms idle when `s_carriers.Count > s_procs`. A pinned carrier stays in
`s_carriers`, so with K >= P pinned and parked every shared carrier can retire, while `EnsureCarrier` (room is
`Count - s_blocked < s_procs`) adds none: a step enqueued in that window strands until some later `Enqueue`. P3's
case: P = 2, K = 2. A plain block shares the path today, but its thread comes back; a pin does not.

**Rule.** A shared carrier retires idle only when the UNBLOCKED count exceeds P: `Count - s_blocked > s_procs`, read
under `s_carriersLock`. Retiring and adding then read the same quantity. The read is a snapshot of an interlocked
counter. If a block ends just after it, a surplus carrier lives one more idle period. If a block begins just after
it, that carrier's own `EnterBlocked` (or the pinned wait's step 1) calls `EnsureCarrier` while work is queued.
Neither strands a step.

**Arm M3b.** K = 2P pinned parked goroutines and one unlocked ticker doing a short piece of work every 100 ms (run 1)
and every 250 ms (run 2), 30 s each. Predicted: every tick runs, each within 5 ms of its timer; carriers started
after the first second: at most 2. Stop: any tick later than 100 ms, or carriers started rising with the tick count
(a retire/add churn). Made to fail first: with the old rule restored, M3b must show the strand (a late tick).

### 10.2 The fourth step-end case (amends 3.1)

A step can end with the goroutine locked, no park pending and the goroutine not ended: the twin awaited something
that is neither a twin nor a park slot (L5's case), and its continuation went to that awaiter. If the carrier
pinned and waited, the resume would run off-carrier and the carrier would wait forever.

**Route it, in the builder.** Every await in a twin passes through golib's own builder (A1). When the awaiting
goroutine is locked and the awaiter is not golib's, the builder makes the await a park: it commits a park slot,
and the foreign awaiter's continuation wakes that slot instead of running the twin. The step then ends in case 1
(locked and parking), and the resume runs on the pinned carrier. Cost: one slot per foreign await under a lock.

**Backstop.** A step end that still reaches the fourth case is a golib defect: it throws by name ("golib: a locked
goroutine's step ended with no park pending"), as `ParkPending` does for a twin that parks twice in one step.
Made to fail: a unit that awaits `Task.Delay` under a lock must resume on the pinned thread; with the builder's
route neutered, the unit fails on the thread id, and with the backstop also removed it hangs and hits its deadline.

### 10.3 The order on pin and unpin, as an invariant (amends 3.4)

**Invariant: `m_pin` is written before `slot.Commit`, on pin AND on unpin.** On unpin it is cleared before the
commit. Cleared after, a wake between the two would go to a carrier that has left its private wait, and the
goroutine would never run. Made to fail deterministically: a unit whose slot is woken BEFORE the park commits
(Commit's "already woken" branch then calls `Ready` at once). With the clear moved after the commit, that unit must
hit its deadline and name the unpin. The same unit with the pin order reversed covers 3.1.

### 10.4 The bands (replaces P2 and P3 of section 7)

The mechanism estimates are built from spike-2 §3.6's parts (twin round trip 1,422-1,550 ns, thread path
1,677-1,715 ns, mount and unmount 38 ns, an uncontended P acquire of tens of ns):

| # | prediction (mechanism) | stop line |
|---|---|---|
| P2 | one side locked: one twin wake, one thread wake, one P acquire: **1.6-2.1 µs** | over 2x the thread path (about 3.4 µs) |
| P3 | both locked: the thread path plus two P acquires and two state swaps: **1.8-2.4 µs** | over 2.5x the thread path |

A reading inside the stop line but outside the band is a MISS to explain, not a stop.

### 10.5 The pinned wake under a saturated P set (new: P10 and arm M7)

Go hands the waker's P to the locked thread (`startlockedm`). Here the pinned carrier waits in `s_p.Wait` with every
other waiter. Reading `Monitor` for this revision shows the gap is wider than a wait for the next step end.
Injection runs only when the RUN QUEUE holds work (`queued = !s_runq.IsEmpty && s_idle == 0`). A pinned carrier
waiting for a P is not in the run queue, so with every P inside long CPU-bound steps nothing injects for it. Its
wake waits for one of those steps to end, however long that is. Go bounds the same case with its 10 ms preemption.

**Fix (part of the pin).** Injection counts a carrier waiting for a P as queued work, as the retake already does
(`s_pWaiters > 0`). A pinned wake then waits at most about the preemption threshold (10 ms) plus a monitor tick
(2 ms), Go's own order of bound. The injection cap (`s_maxInject`, P by default) still applies.

**Arm M7.** P unlocked twins in CPU-bound steps of 200 ms each (no parks), and one locked goroutine woken by a timer
every 10 ms for 10 s; wake-to-run latency per wake, p50 and p99. The same with the woken goroutine unlocked
(control). A run with the fix switched off is the made-to-fail check: its p99 must reach the step length.

| # | prediction | stop line |
|---|---|---|
| P10 | pinned p99 10-15 ms, within 1.5x of the unlocked control's p99; unsaturated (no CPU twins) p99 under 0.1 ms | pinned p99 over 25 ms, or over 2x the control's |

### 10.6 Nit: size

With 10.1-10.5 the golib change is about 130-180 lines (P3's estimate), not 80-120. The lane-days of section 8
stand: 2-3, the M3b and M7 arms inside the 0.5-1 for arms.

## 11. Revision 2, after P3's re-read of section 10 (dated block, 2026-11-20)

P3 re-read revision 1 (`5a795c18c2`): 10.1-10.4 and 10.6 CLEAR; 10.5 BLOCKED on one line, with one SHOULD, two
nits and a limit to name. COORD ruled all of them into this block. Where this block and section 10 differ, this
block holds.

### 11.1 BLOCK: the injection room counted pinned carriers (amends 10.5)

The injection adds its carrier only while `s_carriers.Count < s_procs + over`, and `Count` includes pinned carriers.
P3's case: P = 2, one pinned goroutine parked, both shared carriers in long steps, one unlocked goroutine queued.
The monitor injects (over = 1) and releases a P, `3 < 3` is false, and no carrier is added. No carrier is idle
(injection requires none), so the released P has no thread, and the queued goroutine waits for a step to end. One
pinned goroutine switched the 10 ms bound off for everyone.

**Rule.** The room reads the unblocked count, `Count - s_blocked < s_procs + over`, as 10.1's retire and
`EnsureCarrier` do. A census of `GoScheduler.cs` at this branch finds four reads of the carrier count or `s_blocked`
that decide a thread: `EnsureCarrier`'s room (already unblocked), the injection's room (this fix), the idle retire
(10.1), and `Enqueue`'s gate (`s_blocked > 0` calls `EnsureCarrier`). The last one is correct as it stands: while a
pinned goroutine lives, every `Enqueue` with no idle carrier takes `EnsureCarrier`'s lock. That is a cost to read in
M1, not a defect. The fourth `Count` read only keeps the peak reading.

**M7, third run.** The woken goroutine UNLOCKED, with K = 1 and K = P pinned goroutines parked. Its p50 and p99 must
match the control's (no pinned goroutines) within 1.25x; stop over 1.5x. Made to fail: the old room line restored,
K = 1 must show the starvation.

### 11.2 SHOULD: over-injection for P waiters (amends 10.5)

10.5 widened the trigger to `s_pWaiters > 0`, but the loop injects once per carrier past the threshold, not once per
waiting unit. One P waiter and P long steps give P injections: one is used, and P - 1 permits sit in `s_p` while
the steps run, so later steps start above P. Each injection may also add a carrier that has nothing queued, which
idles 100 ms and retires: a thread start per injection. Bounded (`s_maxInject`, and `ReleaseP` retires surplus), so
it is waste, not a hang. It applies to unpinned P waiters too (a goroutine coming back through `ExitBlocked`).

**Rule.** When the run queue is empty, a tick injects at most `s_pWaiters` permits (net of any already free in
`s_p`) and adds NO carrier: the waiters are threads already. With work queued, injection is as today.

**A budget the arm must respect, named.** A carrier injects once per step (`carrier.Injected` is reset when its next
step starts). So in a period where every carrier runs one long step, at most P injections happen, pinned or not;
a wake beyond that waits for a step to end. This is today's policy for unlocked goroutines too, N5's capped
injection, and not the pin's. M7 therefore wakes every 25 ms against 200 ms steps (8 wakes per step period, under
P = 12 on this box), and adds a fourth run at 5 ms wakes (40 per period) that READS the budget for both the pinned
and the control goroutine. That run is a reading, with no stop line. Its numbers belong to item 5 (N5's policy).

### 11.3 Nits

- **10.2's backstop** is raised while the goroutine is still mounted, as `RethrowTwinFatal` is, before the unmount,
  so golib's report names the goroutine.
- **P10 states p50 as well as p99** (replaces P10 of 10.5). With 11.2's rule, a wake waits for the next monitor tick
  that finds every running step past the threshold. With 200 ms steps that is most of the time, so:

| # | prediction | stop line |
|---|---|---|
| P10 | M7 at 25 ms wakes, saturated: pinned p50 1-3 ms, p99 10-13 ms, each within 1.5x of the unlocked control's; unsaturated (no CPU twins): p99 under 0.1 ms | pinned p99 over 25 ms, or over 2x the control's p99 |
| P11 | M7's third run: unlocked wake with K = 1 and K = P pinned parked: p50 and p99 within 1.25x of the control's | over 1.5x |

### 11.4 L5, widened: a lock taken off-carrier

P3's limit, named under L5. A goroutine that is NOT locked when it awaits something foreign resumes off-carrier
(10.2 routes only a locked goroutine's foreign await). A `LockOSThread` it then calls runs on a thread that is no
carrier, so nothing can pin it. Its next park is a park off its carrier, which `ParkPending` refuses by name today.
This is the existing off-carrier gap, not this design's, and M4's "pinned off-carrier resumes = 0" does not cover
it. A reading is added to M4: off-carrier `LockOSThread` calls, expected 0 in M4's shapes (none awaits a foreign
task). Closing the class means routing EVERY foreign await through a slot, as 10.2 does for a locked goroutine.
That would cost one slot per foreign await for every goroutine. It is named here as an option for item 5, and not
taken.

### 11.5 Size

The two lines of 11.1 and 11.2 do not move 10.6's 130-180 lines or section 8's 2-3 lane-days.

## 12. As cut: two additions, predictions first (dated block, 2026-11-20)

P3 CLEARED revision 2. COORD's GO for the build adds two items, written here before any code. The owner has since
moved M:N to .6; nothing in the pin changes for it.

### 12.1 The fifth read that decides a thread

`EnsureCarrier` returns at once while `s_idle > 0`. That read is safe only because a parked pinned carrier counts
in `s_blocked` and never in `s_idle`: it waits on its private wake, not on `s_work`, so it never takes the idle
count. **The cut keeps the pinned wait outside the idle count.** M4 gains an assertion: while K goroutines sit
pinned and parked, `s_idle` never exceeds the shared carriers' count (read under the carriers' lock each tick).

### 12.2 M1b: the Enqueue gate's cost, with a pin alive

M1 cannot see `Enqueue`'s gate (`s_blocked > 0` calls `EnsureCarrier`), because no pin is alive there. M1b: one
goroutine locked and parked for the whole run, all P busy (P - 1 CPU-bound twins beside the ping-pong pair), and
the unbuffered ping-pong against the same run with no pinned goroutine.

| # | prediction | stop line |
|---|---|---|
| P12 | M1b: ping-pong round trip with the pin alive within 1.05x of the run without it. Most wakes take the run-next slot, which never reaches `Enqueue`; the rest pay one `EnsureCarrier` lock with no idle carrier | over 1.25x: the cure is a lock-free unblocked count that `Enqueue` reads, calling `EnsureCarrier` only below P |

### 12.3 The order of the cut

1. golib: `m_pin`, the private wait, the `Ready` route and the run-next closure, the step-end cases (with the
   backstop raised while mounted), the builder route for a foreign await under a lock, the retire, room and
   injection rules, the readings.
2. GolibTests for M4 and M5, and the invariants of 10.3, each made to fail first.
3. The arms M0-M7, M1b and M3b on this box, and the record.

## 13. As built and as measured: two stop lines hit (dated block, 2026-11-20)

Built at `a84f835512` and `ebc75a5661` (MnLockPin's watchdog); measured with MnPinBench (src/tools/mnspike2) on this
box (12 logical processors, so P = 12; JIT, Release, `GO2CS_MN=1`), relay hold open. Three arm switches are added to
golib: `GO2CS_MN_INJECT_WAITERS=0`, `GO2CS_MN_RETIRE_RAW=1` and `GO2CS_MN_ROOM_RAW=1`. Each restores one line as
it was before the pin, and they exist only for the made-to-fail runs. One process per run, 5 runs for the
timings and 3 for the latencies. The base arm is the same program with golib built at `a231279143` swapped in
(golib-base `99cb5ff3`, golib-pin `13ba6de5`).

| # | reading | prediction | result |
|---|---|---|---|
| P1 | ping-pong, base vs pin, INTERLEAVED 8 + 8 | within the A/A band or 2% | MET: medians 1,042.7 vs 1,052.8 ns, +1.0% (base alone spreads 1,025 to 1,197). The block-ordered first read showed +7% (A/A 1,113.8 / 1,104.8, pin 1,192.5): drift between blocks, not the pin |
| P1 | never-park iteration | as above | MET: 24.9 / 24.8 base, 24.9 pin |
| P2 | one side locked | 1.6-2.1 µs; stop over 2x the thread path | **STOP**: 2,712.9 ns (2,692 to 2,725), 2.9x today's thread path of 935.1 ns (870 to 985). The thread path itself reads well under spike-2's 1,677-1,715 on this box now |
| P3 | both sides locked | 1.8-2.4 µs; stop over 2.5x | MISSED LOW, inside the line: 1,610.5 ns (1,578 to 1,683), 1.72x the thread path |
| P4 | peak carriers with K pinned parked | P + K (+0 to 2) | MET: 14, 29-30, 37-38, 269-270 at K = 1, 16, 24, 256; OS threads rise by K |
| P5 | K = 2P beside 5,000 pairs | no hang, rate within 10% of K = 0 | MET: 976,233 vs 990,400 trips/s (-1.4%). K = 256 reads -10.4% |
| P10 | M7, 12 CPU twins in 200 ms steps, a wake every 25 ms | pinned p50 1-3 ms, p99 10-13 ms, within 1.5x of control; stop: p99 over 25 ms or over 2x control | **STOP on the absolute line**: pinned p50 12.7 ms, p99 43.6 ms (38.1 to 57.5); the unlocked control p50 15.0, p99 59.9 (48.9 to 65.5). Pinned is 0.73x the control, so the relative line holds. With the waiter fix off: p99 599 ms, max 774 ms (the fix cuts the pinned tail 14x). Unsaturated: p99 0.216 ms (MISSED, predicted under 0.1; no line) |
| P11 | unlocked wake beside K = 1 and K = 12 pinned | within 1.25x of control | MET: K = 1 p50 11.6 ms, p99 52.4; K = 12 p50 16.2, p99 64.5 |
| P12 | M1b, ping-pong beside 11 CPU twins, a pin alive all run | within 1.05x of no pin | MET: 1,682.9 ns with the pin vs 1,989.4 without (the no-pin arm is noisy: 1,736 to 2,259) |
| M3b | K = 2P pinned parked, wakes every 100 ms / 250 ms for 30 s | every tick within 5 ms, no churn | MET: p99 0.548 / 0.361 ms, max 1.75 / 1.59 ms, carriers started 36 (12 + 24), flat. Made to fail: with the old retire rule the run STALLED to its 300 s deadline, a stranded queue |
| 11.1 red | the old injection-room line at K = 1 | must starve | **NOT SHOWN.** At 200 ms steps, and at 2 s steps with wakes every 25 ms and every 250 ms, the old line and the fixed one read alike (p99 about 1.75-1.9 s at 2 s steps, in BOTH arms). So unlocked wakes wait nearly a full step even inside the once-per-step budget: an injection-policy finding for item 5, present with or without the fix |
| M7, 4th | 5 ms wakes (a reading) | none | locked p50 16.1, p99 65.0 ms; control p50 15.4, p99 57.8 |

**Mechanism read for P2 (a hypothesis, not tuned):** a LOCKED waker may not use its run-next slot (3.3), so the
unlocked partner it wakes goes through the global queue and an idle carrier. Each round trip then costs two OS
thread wakes plus a queue hop and a P hand-off. With both sides locked each wake is one direct semaphore release,
which is why P3 is faster than P2.

**What the design says now:** a P2 miss stops the seat for a ruling (7). The options: (i) accept 2.7 µs per one-sided
locked round trip, paid only by code that locks; Go itself pays a thread hop for each locked wake. (ii) Price one cure
first: let a locked waker hand the goroutine it readies to a specific idle carrier's run-next slot, so that carrier
needs no queue hop. (iii) The falsifier's alternative design: move a locked twin to a plain thread goroutine for the
length of its lock. For P10, the absolute line assumed that injection bounds the wait near the preemption threshold.
The unlocked control (no pin anywhere, p99 60 ms) shows that today's policy does not bound it. Ruling asked: score P10
on its relative line, and move the absolute bound to item 5 (N5's policy), whose 2 s-step reading is above.

## 14. The cure for P2, priced as ruled (dated block, 2026-11-20)

COORD ruled section 13 as follows. P2: price ONE cure first, option (ii), and keep it if it brings P2 inside 2x,
else take (i). P3 is recorded as scored (MISSED LOW, inside its line). P10 is scored on its RELATIVE line (pinned
0.73x the control: MET), and the absolute bound moves to item 5 with the 2 s-step readings. The unsaturated p99 is
recorded as MISSED, with no line. 11.1's red gets one more shape.

**Diagnostic before the cure** (no code change): unlocked ping-pong with run-next OFF reads 2,898-3,045 ns, about
what one side locked reads (2,675-2,789). P2's cost was the cross-carrier wake through the global queue and the
shared work semaphore, not the pin.

**11.1's red, SHOWN** (COORD's shape: K = P = 12 pinned parked, 12 CPU twins in 4 s steps, one unlocked goroutine
woken each second, 12 s, 2 runs per arm). The old room line adds no carrier: peak carriers 24 (12 shared + 12
pinned), 4-6 wakes through, p50 4.0-6.0 s, p99 7.0 s. The fixed line reads peak carriers 32-34, 10-11 wakes, p50
1.0-2.0 s, p99 3.0 s.

**The cure (built):** idle carriers wait on their OWN semaphore and sit in an idle set under one lock; `s_idle` is
the set's size. `Enqueue` claims one idle carrier and releases its semaphore; this replaces the shared work
semaphore. A LOCKED waker hands the goroutine it readies straight to an idle carrier (the one that ran it last,
when idle) through that carrier's run-next slot. A carrier registers before it re-reads the run queue, and one
claimed as its wait times out takes the permit, so no wake is lost. Retire keeps the unblocked count (10.1). A
pinned carrier never enters the set (12.1); a reading tick checks it (`idle-set-violations`). 

| reading | prediction / guard | result |
|---|---|---|
| P2' one side locked | 1.6-1.8 µs; stop over 2x the same-session thread path | MET, INSIDE: 1,767.5 ns (1,743 to 2,754, one outlier) against a thread path of 932.9 ns, 1.89x. Before the cure 2,780.3 in the same session. **Kept**, as ruled |
| P3 both locked | inside 2.5x | 1,653.0 ns, 1.77x |
| P1 guard, unlocked ping-pong, interleaved | within 2% | 8 + 8: **+4.0% (a miss by the letter)**, 1,168.6 vs 1,123.9, ranges overlapping (1,123-1,251 vs 1,093-1,271). Resampled 16 + 16 at once: +1.6%, 1,073.7 vs 1,056.4 (1,041-1,226 vs 1,014-1,098). Reported for ruling |
| P1 guard, never-park | within 2% | 24.7 vs 24.9 ns |
| side reading, ping-pong with run-next off | falls to 1.6-2.0 µs | MET: 3,066 -> 1,892 ns |
| P5 guard, M3 K = 0 / K = 24 | within 10% of `9b8c9a0d2f`'s | 1,088,832 (+9.9%) / 1,046,514 trips/s (+7.2%), both faster |
| M3b guard (100 / 250 ms) | as the fixed arm | p99 0.197 / 0.280 ms, max 1.87 / 2.10 ms, carriers started 36 flat |
| 11.1 shape guard | as the fixed arm | peak carriers 33-34, 11 wakes, p50 1.02 s, p99 3.01 s |
| fifth read (12.1) | 0 | idle-set violations 0 in every run |
| MnLockPin | green 3/3 | green 3/3, output = plain, about 70,000 direct hand-offs per run |

### 14.1 Addendum: P2' re-scored, and the P1 guard re-run under a rule fixed first (dated block, 2026-11-20)

- **P2', corrected:** 1,767.5 ns is 1.89x the thread path, above the predicted 1.6-1.8 µs band and inside the 2x stop
  line: **MISSED HIGH on the prediction, MET on the line.** The cure is kept (COORD).
- **P1 guard, COORD's named re-run.** The rule was fixed before the run: an A/A pair (base vs base, interleaved,
  16 + 16), then base vs cure (interleaved, 32 + 32); MET if the cure's median is within max(2%, the A/A delta) of
  base. "Base" is the pre-cure pin build (golib `13ba6de5`); the cure is golib `b111c061`. Unlocked ping-pong, one
  process per run.

| sample | median (ns) | range |
|---|---|---|
| A/A base | 1,062.7 | 1,047.9 to 1,115.3 |
| A/A base, second copy | 1,058.0 | 1,037.8 to 1,114.8 |
| base | 1,063.7 | 1,033.2 to 1,127.4 |
| cure | 1,092.0 | 1,066.2 to 1,313.1 |

  The A/A delta is 0.44%, so the line is 2%. The cure reads **+2.66%: P1 MISSED** under the rule. The earlier readings
  stay beside it: 8 + 8 read +4.0% and 16 + 16 read +1.6%.
- **Candidates, not tested (no tuning without a ruling):** (a) COORD's: the idle carriers take a lock per 100 ms
  idle cycle (about 110 lock operations a second at P = 12, small against a million round trips a second). (b) The
  Enqueue fall-back after the run-next chain cap (128) now takes the idle-set lock. It is reached at most once per
  128 steps, so it would have to cost about 4 µs each time, which is implausible. (c) Code shape: `Ready` gained a
  branch with a lock-taking call on every wake path, which can change the JIT's inlining of `Ready` and its callers.
  20-30 ns a wake would explain +2.7%. That is the first candidate to test if a test is ruled.

### 14.2 Arm (c), one axis, outcome pre-ruled (dated block, 2026-11-20)

COORD's arm: the locked waker's hand-off moves out of `Ready` into a `[NoInlining]` static cold helper (`HandToIdle`),
and nothing else changes. The ruling, fixed before the run: if cure+(c) is within 2% of base, keep cure+(c) and P1 is
MET; otherwise keep the cure with its cost stated.

**The JIT check, posted before the run** (`DOTNET_JitDisasm`, Tier-1 `Ready`): pre-cure 274 bytes with `Enqueue`
inlined; cure 499 bytes, with `Enqueue` and `ClaimIdle` called out of line and two thread-static reads; cure+(c) 374
bytes, `Enqueue` inlined again and one `HandToIdle` call. `Commit` calls `Ready` in all three.

| sample (unlocked ping-pong, interleaved) | median (ns) | range |
|---|---|---|
| A/A base | 1,066.2 | 1,042.7 to 1,112.4 |
| A/A base, second copy | 1,060.3 | 1,034.5 to 1,119.8 |
| cure | 1,083.0 | 1,053.5 to 1,195.7 |
| cure+(c), paired with the cure | 1,084.2 | 1,050.5 to 1,156.4 |
| base | 1,066.6 | 1,044.1 to 1,125.1 |
| cure+(c), paired with base | 1,080.8 | 1,054.0 to 1,190.5 |

- A/A delta 0.56%, so the line is 2%. Base vs cure+(c): **+1.33%, inside: P1 MET; cure+(c) is KEPT**, as pre-ruled.
- Stated plainly: cure vs cure+(c) reads +0.1%, so the arm did NOT move the number. P1 passed this time because the
  cure's own cost read smaller in this session (+1.3% to 1.5%) than in 14.1's (+2.66%). That 14.1 reading stays
  visible as MISSED.
- P3's note, recorded as the named candidate for item 11 (park step thin), not tested here: the cost may sit in
  `Enqueue`'s `ClaimIdle` lock.

## 15. P3's MUST on the pin build: the shared-pool floor (dated block, 2026-11-20)

P3's one-pass read of `7cef357ae9` found the shared pool could fall to ZERO and strand the queue. (a) A goroutine that
ends locked retired its carrier with no replacement, even an ordinary shared carrier that never pinned. (b) A RUNNING
pinned carrier counted as shared capacity in the retire rule. P3's two SHOULDs: both rooms read the shared count,
and a standing reading of the floor.

**Red first, each named by MnLockPin's watchdog** (`MN_PIN_SCENARIO`, `GO2CS_MN_PROCS=4`, 10 s deadline):
- Scenario A, four goroutines that lock, compute 50 ms and end without parking, then one unlocked goroutine readied:
  `MNPIN-WATCHDOG arm=scenA`, rc 3, 2 runs of 2.
- Scenario B, K = P goroutines that lock, park once, compute 150 ms and end locked, with main readied by their
  sends: `arm=scenB`, rc 3, 3 runs of 3. A first form, where main waited in `WaitGroup.Wait`, stayed green. That
  wait is a plain block, which never queues main, so the scenario needs a twin park.

**The fix:** a pinned-carrier count, `s_pinnedCount`, kept under the carriers' lock at pin, unpin and retire.
`SharedUnblocked()` is the carriers that are neither pinned nor blocked: `Count - s_pinnedCount - (s_blocked -
s_pinnedWaiting)`. The idle retire, `EnsureCarrier`'s room and the injection room all read it (the RAW switches keep
the old raw count for the arms). `RefillShared()` keeps at least P shared carriers. It runs when a carrier pins, and
when a retiring thread leaves, inside the same lock, whatever the queue holds. `CheckIdleSet` reads the floor on every
reading tick (`shared-floor-violations`).

| reading | result |
|---|---|
| scenario A / B after the fix | green 3 of 3 each; 4 carriers retired and 4 replacements started (`carriers-started` 8); floor violations 0 |
| MnLockPin | green 3/3, output = plain, idle-set and floor violations 0 |
| M3 K = 24 / M3b 100 ms / the 11.1 shape | 962,181 to 1,019,784 trips/s / p99 0.370 ms, max 1.46 ms / peak carriers 34; floor violations 0 |
| **P1 guard, rule fixed first** | A/A 1,059.7 vs 1,060.7 ns (0.09%), so the line is 2%; base vs floor 1,085.8 vs 1,116.6 (32 + 32): **+2.84%, MISSED** |

**Mechanism read for the P1 miss (a hypothesis, untested):** unlocked ping-pong never pins, and the floor fix touches
only the pin, retire and room paths. So this reading most likely carries the cure's own cost, which has read +1.3%
to +2.7% across sessions (14.1, 14.2), and not anything the floor adds. The one-axis read that would separate them,
cure+(c) vs the floor fix, was not in the ruled protocol, and it is not run.

### 15.1 Rulings, and P1's attribution reading for the floor fix (dated block, 2026-11-20)

COORD ruled: the floor fix is KEPT whatever P1 reads (a hang fix, red first by name); P3 CLEARED it at `bfe38b8c5e`.
Section 15's +2.84% got ONE attribution reading, rule fixed first: A/A cure+(c) vs cure+(c) 16 + 16, then cure+(c)
(`b771dc6152`, golib `24f43ff1`) vs floor (`bfe38b8c5e`, golib `2534897f`) 32 + 32, interleaved.

| sample (unlocked ping-pong) | median (ns) | range |
|---|---|---|
| A/A cure+(c) | 1,105.5 | 1,060.8 to 1,273.8 |
| A/A cure+(c), second copy | 1,091.0 | 1,063.2 to 1,228.8 |
| cure+(c) | 1,086.6 | 1,058.0 to 1,162.1 |
| floor | 1,083.7 | 1,048.0 to 1,182.9 |

A/A 1.33%, so the line is 2%. The floor reads -0.27% against cure+(c): **the floor fix is cost-free on the unlocked
path. The cure's cost reads +1.3% to +2.8% across sessions** (14.1 +2.66%, 14.2 +1.33%, 15 +2.84%, each against the
pre-cure pin). It is stated here as the cure's cost, beside what the cure buys: P2 2.78 -> 1.77 µs, M3 7-10% faster,
and run-next-off wakes 3.07 -> 1.89 µs.

### 15.2 P3's SHOULD on the floor fix: pinned carriers blocked without waiting pinned (dated block, 2026-11-20)

P3 (CLEAR on `bfe38b8c5e`, one SHOULD): `SharedUnblocked` subtracted `s_blocked - s_pinnedWaiting` as the shared
blocked count. But `s_blocked` also counts a PINNED carrier that is blocked without waiting pinned: its goroutine in a
plain park (`EnterBlocked`), or retaken inside a native call. Locked goroutines in long blocks therefore
over-provisioned shared carriers. That errs toward threads, never a strand.

**Red first** (MnPinBench mode `pb`; predicted before the run: the old count goes toward P + 2K, and the line
is P + K + the injection budget = 12 + 24 + 12 = 48). K = 24 goroutines lock, park once (pinned), then sleep 3 s
through `time.Sleep`'s plain path (its twin is internal, 14), while 5,000 pairs keep work queued for 2 s. Before the
fix: peak carriers **61** in 3 runs of 3 (764,000 to 867,000 trips/s).

**The fix:** `s_pinnedBlocked` counts the pinned carriers in `s_blocked`, through a per-carrier flag set wherever that
carrier raises `s_blocked`: `WaitPinned`, `EnterBlocked`, and the monitor's retake. The retake sets the flag BEFORE
it publishes, so a step that releases the retake at once still lowers what it raised. The flag is read and cleared
where `s_blocked` falls (`ExitBlocked`, the end of `WaitPinned`, a step's release of its retake). `SharedUnblocked`
subtracts it in place of `s_pinnedWaiting`.

| reading | result |
|---|---|
| `pb`, K = 24, after the fix | peak carriers 37-38 (at most 48), 3 runs of 3; 954,382 to 1,062,990 trips/s; floor violations 0 |
| scenarios A / B, MnLockPin | green; output = plain; idle-set and floor violations 0 |
| GolibTests' M:N subset | 85 passed, 1 skipped |

Not exercised: the retake site's pinned counting. No arm here puts a locked goroutine in a bracketed native call. It
is built, and named as untested.
