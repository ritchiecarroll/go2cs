# DESIGN — M:N: the `LockOSThread` pin (N7), priced

> **STATUS: PROPOSED (lane G, 2026-11-20; branch `claude/g-mn-proto-b1`, cut from P4's `claude/p4-mn-byref`
> `e178ab8a1e` with master `1fa80494ee` merged in).** Design only: no code lands from this document until COORD
> rules it. It answers item 2 of the re-sized M:N plan: what `runtime.LockOSThread` means on the scheduler, what a
> pinned goroutine costs a carrier, and how the pin meets N5. Predictions and stop lines are in §6 and §7, written
> before any arm runs. Amended by dated blocks, never rewritten.

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
