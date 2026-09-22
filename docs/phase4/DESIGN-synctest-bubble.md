# DESIGN — a managed `internal/synctest` bubble (sizing, go1.24.13)

**Status:** SIZING — a design record, no code. C1, 2026-09-22, read at the batch-7 stamp `3469154a95`
against the go1.24.13 GOROOT. Rules nothing; the seats below are COORD's to cut. Point-in-time —
amend with dated blocks.

**The question.** `net/http`'s 1.24.13 re-bank is blocked by the infrastructure-error rows that call
`internal/synctest.Run` (G's pass 3, `1e7709ac7a`), and `internal/synctest`'s own candidate row
(the ledger reads it twice: 29 run = 26 infrastructure-error + 3 fail, and DIVERGED 28 = 26 + 2) has the same root: every one of the five
pulls — `Run`, `Wait`, `acquire`, `release`, `inBubble` — is a `PartialStubGenerator` throw, because
Go pushes them from `runtime/synctest.go` and the push does not arrive.

**The answer, in one paragraph.** Nothing here is structural. The bubble is four golib-side seats —
(S1) bubble core + goroutine accounting, (S2) bubbled channels, (S3) the fake clock and fake timers,
(S4) the `internal/synctest` bodies — every one of which needs a .NET lane to gate, because the whole
of it is concurrency and none of it is visible to the Go-side suite. golib already owns the two
things the design leans on hardest: a single park seam that publishes every goroutine's wait reason
(`Goroutine.Park`), and an inheritance channel that flows from a `go` statement to the new goroutine
(the `AsyncLocal` the profile labels already ride). The one correctness subtlety is the WAKE side of
the accounting (§5, H2); everything else is plumbing.

## 1. What Go does (runtime/synctest.go and its consumers, read at the pin)

- **Membership.** `gp.syncGroup`, set by `Run` on the root and inherited by every goroutine that root
  starts (`proc.go:5143` `newg.syncGroup = callergp.syncGroup`), and by the goroutine that runs a
  bubbled timer's func (`time.go:1131-1138`).
- **Durable blocking.** The group counts `total` and `running`; a goroutine stops counting as running
  only when it parks with a reason in `isIdleInSynctest` (`runtime2.go:1179`): nil-channel send/recv,
  `select {}`, sleep, `Cond.Wait`, `WaitGroup.Wait`, coroutine switch, and the three `Synctest*`
  channel reasons. **Not** durable: `sync.Mutex`, semaphores, I/O, syscalls, and any channel created
  OUTSIDE the bubble.
- **Bubbled channels.** A channel made inside a bubble is tagged (`chan.go:116`); send/recv/select on
  it from outside panics (`send on synctest channel from outside bubble`, and the receive/select
  forms); a select whose every case is a bubbled channel parks as `SynctestSelect` (durable).
- **Fake time.** `time.Now` / `runtimeNano` return `sg.now` inside a bubble (`time.go:18-31`), which
  starts at 2000-01-01T00:00:00Z. Timers created inside are `isFake` and live on the bubble's own heap;
  touching one from outside panics (`reset of synctest timer from outside bubble`).
- **Run.** Starts `f` as a goroutine, then loops: fire due fake timers, park until the group is
  durably blocked, advance `now` to the next fake timer, repeat; when no timer remains, return — or
  panic `deadlock: all goroutines in bubble are blocked` if any member is still alive.
- **Wait.** Parks the caller until every OTHER member is durably blocked (`wait already in
  progress` on a second concurrent `Wait`; `goroutine is not in a bubble` outside one).
- **acquire / release / inBubble.** An activity count that keeps the group from idling, and a way to
  run `f` as a member of an existing bubble (`goroutine is already bubbled`).

## 2. What the blocked verdicts exercise

| Row | Tests | Needs |
|:--|:--|:--|
| `net/http` | `NewClientServerTest/synctest/{h1,h2,https1}`, `TransportRemovesConnsAfterBroken/{h1,h2}` | membership, bubbled channels (the fake net's locks are 1-buffered channels made inside the bubble and SELECTED on — durable by design), `Wait` (`runAsync`, and `fakeNetConn`'s autoWait before reads/after writes), `Run` — **S1 S2 S4** |
| `net/http` | `ServerShutdownStateNew/{h1,h2}`, `TransportIdleConnRacesRequest/{h1,h2unencrypted}`, `TransportRemovesConnsAfterIdle/{h1,h2}` | all of the above plus FAKE TIMERS: `time.Sleep` inside the bubble, `Shutdown`'s polling timer, the transport's `IdleConnTimeout` `AfterFunc` idle timers — **S1 S2 S3 S4** |
| `internal/synctest` | `Now`, `TimerReadBeforeDeadline`, `TimerReadAfterDeadline`, `TimerReset`, `TimeAfter`, `TimerFromOutsideBubble`, `TimerFromInsideBubble` | fake clock + fake timers + outside-bubble timer panics — **S3** (+S1 S4) |
| `internal/synctest` | `RunEmpty`, `SimpleWait`, `GoroutineWait`, `Wait`, `Mallocs`, `Cond`, `WaitGroup`, `DeadlockRoot`, `DeadlockChild` | membership, durable accounting (Cond, WaitGroup, channels, `select {}`), the deadlock panic — **S1 S2 S4** |
| `internal/synctest` | `ChannelFromOutsideBubble` | bubbled-channel panics — **S2** |
| `internal/synctest` | `IteratorPush`, `IteratorPull` | a coroutine switch counted durable (`iter.Pull`) — **S1** |
| `internal/synctest` | `ReflectFuncOf` | `reflect.FuncOf`/`StructOf` 100,000 times inside and outside a bubble — likely to fail on the converted `reflect`'s own StructOf capability (already pinned there), NOT on synctest |

⚠ G's `classes.txt` names **11** `net/http` leaves (3+2+2+2+2) and counts **12** infrastructure-error
rows; which verdict is the twelfth (an intermediate `synctest` node is the likely one) is NOT
reconciled here.

`TestMallocs` is not an allocation assert: it runs 100 bubbles, each a 101-goroutine chain — about
10,100 goroutine starts, which in golib's thread-per-goroutine model are 10,100 thread starts. Slow,
not impossible; it is the row's cost outlier.

## 3. golib today

**Already there, and load-bearing:**
- `Goroutine.Park(WaitReason)` — the ONE park seam, with the reason published per goroutine and an
  outermost-park `ParkTransition` hook the runtime installs. It already parks: channel send / receive
  / select (`channel.cs`), `select {}` as `SelectNoCases` (`channel.cs:832`), `Sleep`
  (`time_impl.cs`), `Cond.Wait` as `SyncCondWait` (`sync/runtime_impl.cs`), the semaphore
  (`RuntimeSemaphore`, reason-parameterised) and the mutexes.
- `Goroutine.Start` — a thread per goroutine whose `ExecutionContext` is captured at `Thread.Start`,
  so an `AsyncLocal` set on the creator is visible to the child (the profile-label mirror already
  relies on exactly this, seeded at `Enter`). Bubble membership rides the same channel.
- `time_impl.cs` — the clock (`runtimeNano`, `now`, `runtimeNow`), `Sleep`, and a timer heap with its
  own service thread (`newTimer` / `stopTimer` / `resetTimer`, synchronous timer channels).

**Gaps the bubble needs closed:**
1. **No bubble object** and no membership.
2. **Accounting is done by the WAKEE.** A parked goroutine is marked running again when ITS scope
   disposes — after its thread is rescheduled. Go's `goready` marks it runnable from the WAKER, at wake
   time. With wakee-side accounting the group can read "all durably blocked" in the window between a
   signal and the wakee's resumption, and `Wait` returns early or `Run` advances fake time early.
3. **Wrong or missing reasons for durable waits.** `WaitGroup.Wait` parks as `Semacquire`
   (`sync/waitgroup.cs:85`, and sync's `runtime_SemacquireWaitGroup` the same) — durable in Go (`SyncWaitGroupWait`), not in the table under its current reason. The
   coroutine handoff (`Coro.cs`) waits on two `SemaphoreSlim`s and does not park at all. golib's `WaitReason` has no `SyncWaitGroupWait`,
   `Coroutine`, `SynctestRun`, `SynctestWait`, `SynctestChanReceive`, `SynctestChanSend`, `SynctestSelect`.
4. **Channels are untagged**, so neither the outside-bubble panics nor the Synctest* reasons exist.
5. **Time is real**: the clock is `MonotonicClock` and every timer is on the one real-time heap.

## 4. The minimal faithful design — hooks and where each sits

| Hook | Where | What |
|:--|:--|:--|
| **H1 bubble core** | golib, new `runtime/SyncTestBubble.cs` | Go's `synctestGroup` field for field: a lock, `now`, a fake timer heap, `root`, `waiter`, `waiting`, `total` / `running` / `active`, and `maybeWake`. Membership: an `AsyncLocal<SyncTestBubble?>` plus a mirror on the `Goroutine`, seeded at `Enter` exactly as the labels are; `Register`/exit adjust `total`/`running`. |
| **H2 park + READY accounting** | golib `Goroutine.Park` / `ParkScope`, and every golib wake site | On park with a durable reason (the `isIdleInSynctest` table, verbatim), `running--` and `maybeWake`. On WAKE, the waker marks the wakee running before it signals (Go's `goready`), and the wakee's dispose is idempotent. The wake sites are finite and all in golib or hand-owned companions: `channel.cs` (send/recv/select/close), `RuntimeSemaphore.Release`, sync's notify list, the timer fire path, `Coro` resume. |
| **H3 reasons** | golib `WaitReason` (+ its Go-string table and `GoroutineParkAccountingTests`), sync's WaitGroup, `Coro.cs` | Add the seven reasons with Go's own strings; `WaitGroup.Wait` parks as `SyncWaitGroupWait`; the coroutine handoff parks as `Coroutine`. |
| **H4 bubbled channels** | golib `channel.cs` (`ChanCore`) | Tag at make from the creator's bubble; the three outside-bubble panics with Go's texts; park as the `Synctest*` reasons when every channel involved is bubbled. |
| **H5 fake time** | `time/time_impl.cs` (hand-owned) | `runtimeNano`/`now`/`runtimeNow` return the bubble's `now` for a member; `Sleep` in a bubble parks durable (`Sleep`) on a FAKE timer; `newTimer`/`AfterFunc` in a bubble go on the bubble's heap (`isFake`), never the service thread; a fake timer touched from outside panics; a timer func runs on a goroutine that joins the bubble. |
| **H6 the pulls** | a new hand-owned `internal/synctest/synctest_impl.cs` over golib (the package may reference nothing but golib and `unsafe`, the same no-cycle rule as `internal/sync`) | `Run` (the loop in §1, both panics, the nested-`Run` panic, the `asynctimerchan` refusal via the setting `time_impl.cs` already reads), `Wait`, `acquire`, `release`, `inBubble`. |

## 5. What it cannot model — and which of it is structural

- **Waits that bypass `Goroutine.Park`** (a BCL wait inside hand-owned code, a blocking P/Invoke) are
  invisible: the member stays "running", the group never idles, and `Wait`/`Run` HANG. That is the
  fail-SAFE direction — never a false "all blocked" — and it is **unbuilt, not structural**, for every
  site in the tree; only an unknown future BCL wait is structural, and it announces itself as a hang
  a diagnostic can name (on a deadline, list the members not parked).
- **Race-detector happens-before edges** (`racereleasemergeg` / `raceacquireg`): no race detector,
  nothing to model — **structural and harmless**.
- **Go's scheduler exactness** comes from the WAKE-side accounting (H2), not from the scheduler, so a
  thread-per-goroutine host reproduces it once H2 is built — **unbuilt**. This is the one place a
  lazy implementation passes most tests and flakes on the rest, so H2 carries its own arms.
- **Thread cost** (`TestMallocs`' ~10,100 goroutines) — a cost, not a divergence.

## 6. Size, order, and who can gate it

| Seat | Content | Gate |
|:--|:--|:--|
| **S1** | H1 + H2 + H3 — the bubble, membership, park/READY accounting at every wake site, the new reasons | GolibTests arms (membership inheritance; Wait returns only when every other member is durably parked; a woken member is counted running BEFORE its thread resumes; WaitGroup/Cond/select{}/coroutine each durable; a mutex wait NOT durable) + the GoroutineParkAccounting string table |
| **S2** | H4 — bubbled channels | GolibTests arms (the three panics by text; all-bubbled select durable, mixed select not) |
| **S3** | H5 — fake clock and timers | GolibTests arms + the `internal/synctest` row's seven time tests |
| **S4** | H6 — the five pulls | the `internal/synctest` row (29 verdicts) and `net/http`'s 12 |

Order S1 → S2 → S3 → S4; S4 is thin once S1–S3 exist. **Every gate needs .NET**: the whole design is
runtime behaviour the Go-side suite cannot see. So the seats belong on a hardware lane (R or i9) as
author. C1 can carry the READING for each seat (the Go source mapping, the arm list, a
Go-side emission read) and write a seat UNCOMPILED for an i7 compile loop, as it did for
`internal/sync`. But S1's wake-side accounting is concurrency code that should be iterated where it
can be run, not round-tripped.

**Predicted rows after S1–S4:** `internal/synctest` validates except `TestReflectFuncOf` if the
converted `reflect.StructOf` refuses (a `reflect` capability pin, not a synctest one) — so a candidate
at 28 or 29 of 29. `net/http`'s 12 infrastructure-error rows clear; the row's re-bank then rests on its
OTHER divergence (`TestRegisterErr//a`, the `runtime.Caller` registration-site class) and its parents.
