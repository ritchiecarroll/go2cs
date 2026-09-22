# DESIGN — waker-side READY: the gopark/goready pair and the synctest bubble on one seam

**Status:** DESIGN, for COORD's ruling of S1. R, 2026-09-22, read at `3469154a95` (batch 7 stamp)
against the go1.24.13 GOROOT. Builds on C1's sizing, `DESIGN-synctest-bubble.md` at `740cc9f1fe`
(its S1–S4 and H1–H6 are used by name below and not restated). No code. Point-in-time: amend with
dated blocks.

**The question.** Two arcs meet at one seam. (a) The runtime row dies at `mcall(park_m)`: the
converted `gopark` has no managed answer below it (R's class reading, accepted at the lfnode seat).
(b) The synctest bubble needs a goroutine's return to "running" accounted when it is WOKEN, not when
it resumes (C1's H2, "the hard point"). Both are the same missing primitive: **golib has a park and
no ready.**

**The answer, in one paragraph.** Add a READY half to golib's park seam: a per-goroutine park state
that the WAKER moves from Parked to Readied, before it signals the primitive, through one call keyed
by the goroutine record (`Goroutine.Ready`). Then (a) is a thin hand-own: `gopark` parks the calling
goroutine on its own gate through that seam, and `ready`/`injectglist` resolve a `g` to its goroutine
by `goid` and call `Ready`. `mcall` stays a refusing stub, and nothing in the pair needs a table.
And (b)'s bubble counts a member idle only when it is parked durably AND its waker is accounted. Every
wait the seam cannot vouch for leaves the member counted RUNNING, so the bubble HANGS (loud, and a
diagnostic can name the member) and never reads "all blocked" early (a quiet lie). Nothing here is
structural. The cost is one ordering fix at every golib park site (§4), the one place C1's sizing
did not reach.

## 1. The seam today (read, not assumed)

- `Goroutine.Park(WaitReason)` (golib `runtime/Goroutine.cs`) is ACCOUNTING ONLY. It publishes the
  reason, invokes `ParkTransition(reason, true)` at the outermost scope, and its `ParkScope.Dispose`
  invokes `ParkTransition(reason, false)`, then restores the reason. Its own doc comment says:
  *"There is deliberately no goready side. The waker already signals the primitive, and the woken
  thread un-marks ITSELF when its scope disposes."* That sentence is the property this design changes.
- The runtime installs `ParkTransition` (`stubs_impl.cs`, Q61, 2026-09-05). Entering does
  `casgstatus(_Grunning → _Gwaiting)`; leaving does `_Gwaiting → _Grunning`, "Go's ready and execute
  collapsed". The `g` it moves is minted per goroutine thread (`getg`, a `[ThreadStatic] ж<g>`,
  `goid = Goroutine.Id`).
- golib keeps every live goroutine in `s_live`, a `ConcurrentDictionary<long, Goroutine>` keyed by
  `Id`. Ids are never reused.
- **Every golib park site publishes its waiter BEFORE it enters `Park`.** Channel send/receive
  (`channel.cs` ~458/543) enqueue, `Monitor.Exit`, then park. `select` (~878) enqueues all cases,
  `UnlockAll`, then parks, and its comment reasons about exactly this window ("a waker that claimed
  us between the unlock and this wait…"). `RuntimeSemaphore` enqueues under its bucket lock, leaves
  it, then parks. `runtime_notifyListWait` adds to the list under `lock(n)`, then parks. Go's order
  is the reverse: `gopark` marks the g `_Gwaiting` first, and only THEN does `unlockf` release the
  lock that makes the waiter visible, so a waker can only ever find a WAITING g.
- The wake sites are few, and each is one call:
  - channels: `Waiter.Wake()` (`channel.cs:241`, select-aware). It has 5 call sites: `Send`,
    `Recv`, `Close`, and a select's `TryCommitSendLocked` / `TryCommitRecvLocked`.
  - semaphore: `RuntimeSemaphore.Release` (`w?.Signal.Set()`, one site).
  - sync.Cond: `runtime_notifyListNotifyOne` / `NotifyAll` (`sync/runtime_impl.cs:158, :174`).
  - WaitGroup: `Idle.Set()` in `Add`. This is a `ManualResetEventSlim`, so **it cannot name whom it
    wakes**.
  - coroutines: `Coro.cs` hands off on two `SemaphoreSlim`s and **never enters `Park` at all**.
  - `time.Sleep` parks on a real deadline (`waitUntil`) and has **no waker**.
- `sync.WaitGroup.Wait` parks as `Semacquire`. Its comment says Go "has no wait reason of the latter
  name", which is **stale at 1.24**: `runtime2.cs:907` carries `waitReasonSyncWaitGroupWait = 24`
  ("sync.WaitGroup.Wait"), and Go 1.24 parks `Wait` with it.

## 2. The one invariant

> **A goroutine returns to RUNNING at the moment its WAKER decides to wake it, and the waker records
> that before it signals.**

The two ways to break it fail in opposite directions, which is what makes the design safe to build
incrementally:

| Failure | Effect on a bubble | Direction |
|:--|:--|:--|
| A durable park whose waker does NOT record READY | between the signal and the wakee's resumption, the group reads the wakee as still blocked → `Wait` returns early / `Run` advances fake time | **LIE** (quiet, flaky) |
| A park the seam does not count as durable at all | the member stays "running" → the group never idles | **HANG** (loud; a deadline can name the member) |

So the rule for S1 is: **a park may count toward idleness only if its site is DURABLE (Go's
`isIdleInSynctest` table, verbatim) AND WAKER-ACCOUNTED** (every waker of that primitive calls
`Ready`). The second property is declared at the park site, not inferred. A site that cannot declare
it stays in the HANG column. That is unbuilt work, never a wrong answer.

## 3. Mechanism

**Per-goroutine park state** on `Goroutine`: `Running → Parked → Readied → Running`, with one int
field and CAS transitions.

- `Park(reason, accounted)`: `Running → Parked`. It runs the existing `ParkTransition(reason, true)`
  and, in a bubble, `running--` iff durable and accounted.
- `Goroutine.Ready(Goroutine target)` is the new waker API, callable from ANY thread, including the
  timer service thread, which is not a goroutine. It does `Parked → Readied`, invokes a new
  `ReadyTransition` slot (the runtime's `casgstatus(_Gwaiting → _Grunnable)`, which is Go's `ready`
  and where Go adds the mutex wait time), and, in a bubble, `running++` under the bubble lock. The
  caller signals its primitive only after `Ready` returns.
- `ParkScope.Dispose`: `Readied → Running` runs the execute half (`_Grunnable → _Grunning`). A scope
  still `Parked` at dispose was ended by something that is not an accounted waker (a timeout, a real
  deadline, a non-accounted site), so it performs both halves itself, which is today's behaviour. So
  dispose is **idempotent over who readied it**, as C1 required.
- **`Ready` on a target that is not `Parked` PANICS BY NAME.** After the §4 reorder it is
  unreachable (Go throws `bad g->status in ready` for the same state), so a bookkeeping bug is loud,
  never a lost or double count.

**Q61 coexistence.** The collapsed "ready + execute at dispose" splits into its two Go halves: ready
at the waker, execute at the wakee. `/sync/mutex/wait/total:seconds` accumulates at the ready half
exactly as Go's `casgstatus` does. For a non-accounted park the sum is unchanged, because dispose
still does both. `GoParkTransitionProbe` / `GoNestedParkProbe` must read identically. That is an
S1 arm, not an assumption.

**Lock order.** channel/semaphore/notify lock → bubble lock, never the reverse. `Run`/`Wait` hold
only the bubble lock and never touch a primitive's lock. `Ready` takes only the bubble lock (for a
member) and CAS.

## 4. The ordering fix at every golib park site (the part C1's sizing did not reach)

Because each site publishes before parking (§1), an accounted waker can reach a goroutine that is
still `Running`. Two remedies are possible, and the design takes the FAITHFUL one:

- **Go's commit order (chosen).** Enter `Park` while still holding the lock that publishes the
  waiter, then release the lock, then wait. This is exactly `gopark`'s contract ("unlockf runs after
  the g is waiting"). The primitive's own lost-wakeup safety is untouched: the wait is still on the
  same semaphore, released by the same waker. The only thing that moves is the accounting's position
  relative to the unlock. `Park` under a primitive's lock is safe because `ParkTransition` is
  atomics on the goroutine's own `g` and takes no golib lock. That is an S1 read, with a guard.
- *(Rejected)* a "ready token" that `Ready` leaves on a `Running` target for `Park` to consume. It
  works, but it papers over an ordering that is simply wrong relative to Go, and it makes "Ready on a
  non-parked goroutine" legal, which removes the loud failure above.

| Site | Durable in Go? | Waker(s) | S1 action |
|:--|:--|:--|:--|
| chan send / recv (`channel.cs`) | only on a nil channel or a bubbled one (S2) | `Waiter.Wake` | reorder; `Waiter` carries its `Goroutine`; `Wake` → `Ready` then release |
| `select` (`channel.cs`) | `select {}`; all-bubbled cases (S2) | `Waiter.Wake` via the `SelectState` claim | reorder; `SelectState` carries the goroutine; the claimant readies |
| `select {}` / nil chan | yes | none (forever) | durable, no waker needed |
| `RuntimeSemaphore` (sync mutex/rwmutex, poll) | **no** | `Release` | reorder + `Ready` (correct Q61 timing); never counts idle |
| `sync.Cond.Wait` (notify list) | yes (`SyncCondWait`) | `NotifyOne` / `NotifyAll` | reorder into `lock(n)`; `NotifyWaiter` carries the goroutine; ready then set |
| `sync.WaitGroup.Wait` | yes (`SyncWaitGroupWait`) | `Add` → zero | a waiter SET in the wg state (the event cannot name whom it wakes); reason → `SyncWaitGroupWait`; `Add`-to-zero readies each |
| `Coro` switch | yes (`Coroutine`) | the peer's switch | park on the handoff; the switcher readies its peer before `Release` |
| `time.Sleep` | yes (`Sleep`) | none outside a bubble; the bubble's clock inside (S3) | outside: unchanged (dispose readies); inside: a fake timer whose fire readies |
| poll `IOWait` | no | netpoll | out of scope; stays non-durable |

## 5. The gopark/goready pair

**Displacements (registry, runtime, all three GOOS):** `gopark`, `ready`, `injectglist`. `goready`
stays converted (it is `systemstack(ready)` and `systemstack` is `fn()`), and so does
`goparkunlock` (it calls `gopark`). **`mcall` stays a refusing stub**: nothing in the pair calls it,
and a future caller reaching it still refuses by name.

- `gopark(unlockf, lock, reason, traceReason, traceskip)`: resolve the caller's goroutine (a thread
  with NO goroutine identity refuses by name, since nothing could ever ready it). Record the mapped
  reason on the `g`. Enter `Park` (accounted, durable per the table) on a per-goroutine gate. THEN
  call `unlockf(gp, lock)`, whose `false` aborts the park as Go's `park_m` does (`casgstatus` back,
  return). Then wait on the gate.
- `ready(gp, traceskip, next)`: `s_live[gp.goid]` → `Goroutine.Ready(target)` → release the target's
  gate. A missing entry (an exited goroutine) or a target not `Parked` throws Go's own `bad g->status
  in ready`. There is no runq, no P, no `wakep`: the target is its own thread, so readying it IS
  running it.
- `injectglist(glist)`: for each `g` on the list, the same as `ready`. This is Go's batch form, and
  it is what `scavengerState.wake` uses. It is NOT `goready`, which is why a pair of only
  gopark/goready would miss the one caller the row reaches first.

**The mapping: lifetime and ownership.** The runtime owns the `g` (one per goroutine thread,
thread-static, never reused). golib owns the `Goroutine` (`s_live` from start to exit, ids never
reused). The link is `goid == Id`, read each time, so the pair owns NO table, nothing outlives either
side, and there is no ABA: a stale `g` names an id `s_live` no longer holds, and that is the loud
throw above. The per-goroutine gate is a field on `Goroutine` (one `SemaphoreSlim(0,1)`, allocated at
the first `gopark`, not per goroutine), so its lifetime is the goroutine's.

**Converted callers the runtime row reaches (windows flavour: 34 call sites in 14 files):**
- **Today: exactly one.** All three `mcall` roots in the lfnode row (r-lf2, 2026-09-22) are one
  event: `TestScavenger` → `Scavenger.Start` → the harness goroutine's `scavengerState.park` →
  `goparkunlock` → `gopark` → `mcall`. Its wake is `scavengerState.wake` → `injectglist`, and
  `BlockUntilParked` polls `s.scavenger.parked` under `lock2`.
- **Next, once the host survives TestScavenger:** runtime's own converted `sema.cs` (`semacquire1` →
  `goparkunlock`; `semrelease1` → `readyWithTime` → `goready`), reached by `metrics_test.go:1261`'s
  `runtime.Semacquire` / `runtime.Semrelease1` subtest.
- The rest (the runtime's own `chan.cs`/`select.cs`, which golib replaces for user code; `netpoll`;
  `mgc*`; `mfinal`; `trace`; `synctest.cs`) are listed for the census arm and are not claimed
  reached. The arm re-derives this list at the cut from the row, not from this record.

**What the pair cannot model:** Go's scheduler state (runq position, `next`, P hand-off, `wakep`,
`traceskip`) has no managed counterpart and nothing reads it. `_Grunnable` becomes a real,
observable interval for the first time (between the waker's ready and the wakee's resumption), which
is MORE faithful than today, and the probes must show it.

## 6. The bubble on this seam (C1's S1, restated only where the seam changes it)

- Membership (H1) as C1 wrote it: an `AsyncLocal` inherited at `Goroutine.Start`, mirrored on the
  `Goroutine`.
- **Idle is computed, not guessed:** `running == 0` over members whose park is durable AND accounted.
  `Wait`/`Run` read it under the bubble lock. `Ready` increments it under the same lock before the
  waker's signal, so the window in C1's §5 cannot open.
- A fake-timer fire (S3) is an accounted waker like any other: `Run` advances `now`, and each due
  timer's delivery readies its receiver before `Run` re-checks idleness.

## 7. What hangs instead of lying, and what is structural

- **HANG (fail-safe, unbuilt):** any wait that bypasses `Park` (a BCL wait in hand-owned code, a
  blocking P/Invoke) and any park declared non-accounted. The member counts running, so `Run`/`Wait`
  never return. S1 adds the diagnostic: on a deadline, list the members not parked, with their
  reasons.
- **Never a LIE by construction:** idleness requires `Parked` + durable + accounted, and every
  transition out of `Parked` goes through `Ready` (waker) or dispose (wakee), both under the bubble
  lock for a member.
- **Structural and harmless:** the race detector's happens-before edges (no race detector).
- **Structural, stated:** `mcall` itself, and any future converted caller of it other than via
  `gopark`, has no managed answer and refuses by name.

## 8. Proposed cut order, and the arms (for COORD to rule)

Three seats where C1 has one S1, because the first two are independently landable and each has its
own row evidence:

| Seat | Content | Red-first arms | Row evidence |
|:--|:--|:--|:--|
| **S1a** the ready seam | park state + `Ready` + `ReadyTransition`; the §4 reorder at every site; `Waiter`/`SelectState`/`NotifyWaiter` carry their goroutine; WaitGroup waiter set + `SyncWaitGroupWait`; Coro parks as `Coroutine`; the Q61 split | a woken goroutine reads `Readied`/`_Grunnable` BEFORE its thread resumes (a gate holds the wakee's resumption); `Ready` on a non-parked target panics by name; each site's waker readies (chan, select, Cond one/all, WaitGroup, Coro); a semaphore wait still reads non-durable; the Q61 probes and the mutex-wait metric unchanged | behavioral FULL (a golib concurrency change) + GolibTests; `sync`, `internal/poll`, `iter` rows unchanged |
| **S1b** the pair | registry `gopark`/`ready`/`injectglist` ×3 GOOS; the per-goroutine gate | `gopark` + `injectglist` round trip on a golib goroutine; `unlockf` false aborts; an exited `g` throws Go's text; a non-goroutine thread refuses | **runtime row**: `TestScavenger` stops killing the host; the death moves or clears; `mcall` roots 3 → 0 |
| **S1c** the bubble core | H1 + the idle computation + the new synctest reasons + the not-parked diagnostic | C1's S1 arms, plus: the window arm (a signal followed by an immediate `Wait` never returns before the wakee is `Readied`), repeated N times on this box | none alone (S2–S4 make the rows move) |

**Predictions, stated to be scored:** S1a moves no banked row. S1b clears TestScavenger's death, and
the row's next death (if any) is named, not predicted. S1c moves no row until S4.

## 9. Open questions for the ruling

1. Accept the §4 reorder (Go's commit order) over the ready-token alternative?
2. S1 as three seats (a/b/c) rather than one?
3. The WaitGroup fix changes a hand-owned file's reason AND adds a waiter set. It is ruled here as
   S1a scope because it is a durable park that must be accounted; say if you want it split out.

---

## Amendment 2026-09-22 (evening) — the owner's ruling, five additions, and what S1a measured

**Ruling (owner, via COORD).** H10 closes with `net/http` moved to the candidates. This arc lands
AFTER the hop, as one train on master; nothing of it merges into `claude/version-go1.24.13`. The
seat order stands: S1a → S1a2 (WaitGroup's waiter set and 1.24 reason, and Coro parking, split out of
S1a) → S1c → S2 → S3 → S4. S1b (the runtime pair) is free to follow S1a, since runtime's row is also
post-hop. §4's open question 1 is ruled: **Go's commit order**, with a contention-stress arm beside
every reordered site.

**Five additions from an owner-invoked feasibility workflow (COORD's relay; neither record covered
them).** R spot-checked (1), (2) and (4) against the tree at `3469154a95`, and all three hold:

1. **An S-HOST seat.** `TestTransportIdleConnRacesRequest/h2unencrypted` calls `t.Skip` INSIDE the
   bubble, and the hand-owned testing host refuses `SkipNow`/`FailNow` off the test's own thread
   (`TestExecution.cs`, `TryEnsureOwner` at the `FailNow` / `SkipNow` / `Parallel` / `Setenv` sites).
   That is an undisclosable infrastructure error. Go does not apply that owner rule to a bubble root
   started from the test. *Checked: the four call sites exist as stated.*
2. **Creator-side spawn counting.** Go counts a new goroutine into the bubble at the `go` statement
   (and when a timer fires). golib registers it later, at `Run` on the CHILD's thread
   (`StartWithCreator` → `new Thread(() => Run(...))`), so between the `go` and the child's first
   instruction the bubble can read idle: `Wait` returns early, fake time advances early (`runAsync`;
   `IdleConnRacesRequest`). S1c must count the child in `Start`, on the creator's thread, and
   discount it if the thread never starts. *Checked: registration is child-side.*
3. **Nil select cases** are excluded from "every channel is bubbled", as Go does. Otherwise
   `Server.Shutdown`'s select on `context.Background().Done()` (a nil channel) keeps the bubble busy.
   S2.
4. **The lazily started timer service thread** inherits the `ExecutionContext` of whoever arms the
   FIRST timer (`time_impl.cs`: `s_timerThread = new Thread(serviceTimers)`, no `SuppressFlow`). With
   bubble membership in an `AsyncLocal`, the engine thread would become a bubble member for its whole
   life. Start it under `ExecutionContext.SuppressFlow()`. S3 (and S1c, which introduces the
   `AsyncLocal`). *Checked: no flow suppression today.*
5. **S3 needs a callback slot on the golib bubble** so `Run` can reach `time`'s private timer heap
   across the assembly boundary (the `ParkTransition` / `ReadyTransition` pattern: golib owns the
   slot, the owning assembly installs it).

The workflow also notes that the Coro half of S1a2 is not reached by `net/http`.

**What S1a measured (branch `claude/r-s1a-ready-seam`, `68fb51c5c`):**
- **Park had its own ordering hole.** `Park` published the golib reason BEFORE the runtime
  transition. Under R2 (Ready wired, old publish-before-park order), a racing waker found the
  goroutine "parked" in golib while its g was still `_Grunning`, and only the runtime's check caught
  it ("status is 2, not _Gwaiting"). `Park` now runs the transition first and publishes the reason
  second (the mirror of dispose's order), and R2 then fails at golib's own invariant ("Ready of
  goroutine N, which is not parked") on all four stress arms. §3's "Ready on a non-Parked target
  panics by name" is therefore a golib-layer property, not only a runtime one.
- **The g → goroutine link for `ReadyTransition` is a descriptor on the record.** `Ready` runs on the
  waker's thread and must reach the TARGET's `g`, while `getg`'s cache is thread-static, so the
  runtime publishes its `g` as `Goroutine.RuntimeDescriptor` when it mints one. This is the same link
  S1b's `ready` / `injectglist` need in the other direction (they already hold the `g` and look up
  the goroutine by `goid`).
- **The readied bit rides in `m_waitReason`**, not in a second field, keeping that field's
  documented one-field invariant ("readied" can only sit beside a nonzero reason). A nested dispose
  carries the bit into the scope it restores.
