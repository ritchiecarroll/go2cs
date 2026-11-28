# DESIGN — `sync.WaitGroup`: a reused WaitGroup panics where Go panics, instead of hanging

COORD's seat of 2026-11-27 (docs/phase4/inbox/P4/20261127T234312Z-COORD.md), agreed with G. The finding is the BOARD
row of 2026-11-27 ("a plain sync.WaitGroup.Wait HANGS where Go panics"). Written out here for G's read before any code.

**Stacked on the Wait twin** (branch claude/p4-wg-twin at 6204ef9c12f, DESIGN-mn-waitgroup-twin.md): both seats
rewrite the waiter list, `Add`'s zero arm and the wait itself, so this one branches from the twin's tip and lands after
it.

## 1. What is wrong

A plain waiter sleeps on `WaitGroupState.Idle`, one `ManualResetEventSlim` for the WaitGroup's whole life. `Add` sets it
at zero and resets it at any non-zero count. A waiter that the zero-reaching `Add` has signalled re-checks the event
when its thread resumes. An `Add` that raises the count before then resets the event under it, and the waiter sleeps
through its own generation: it returns only when a later generation drains, or never.

Go (sync/waitgroup.go at 1.26.11, lines 206 to 213): after the semaphore returns, `Wait` re-reads the state, and a
non-zero state is `panic("sync: WaitGroup is reused before previous Wait has returned")`. The program is misusing the
WaitGroup and Go says so by name.

Measured (the BOARD row; linux/amd64, Release): one goroutine in `Wait` on a count of 1, the main thread calls `Done()`
then `Add(1)` at once. golib: the waiter had not returned 2 s later in 100 of 100 rounds. Go 1.26.11, the same
sequence: the panic in 3 of 3 runs.

The twin (`Waitꞏtwin`) does not sleep there, because its slot keeps the wake. It does not panic either: it returns
as if nothing were wrong. So the twin is immune to the hang and still differs from Go.

## 2. The change

Two parts, both in the hand-owned src/core/sync/waitgroup.cs.

**(a) Each zero-reaching generation releases its own waiters.** `Idle` goes. `WaitGroupState` gains
`long Generation`, guarded by `Gate`. A plain waiter, under the hold of `Gate` in which it registered, records the
generation and waits on the lock itself:

```
long generation = s.Generation;
using (Goroutine.Park(reason)) {
    while (s.Generation == generation) {
        Monitor.Wait(s.Gate);
    }
    reused = s.Counter != 0 || s.Parked.Count != 0 || s.Bubbled;   // Go's wg.state.Load() != 0
    Monitor.Exit(s.Gate);
}
```

`Add` at zero, under `Gate`, after it has readied every waiter and woken every twin slot: `s.Generation++` and
`Monitor.PulseAll(s.Gate)`. Nothing resets anything: the generation only counts up, and a waiter tests it under the
lock that writes it, so an `Add` from zero back up cannot put a released waiter back to sleep. `Add`'s non-zero arm
loses its `Idle.Reset()` and does nothing in its place.

`Monitor.Wait` releases `Gate` and re-takes it on wake, so the registration, the park entry and the wait keep Go's
commit order (the waiter is published to `Add` before it can miss a wake), as the plain `Wait` has today.

`Gate` is released BEFORE the park scope is disposed. Leaving a plain park on the M:N scheduler re-takes a P, which can
block; holding `Gate` across that would let the goroutines that hold the Ps and want `Gate` deadlock the waiter.

**(b) Go's re-read, and Go's panic.** `reused` above is Go's `wg.state.Load() != 0` over the same three parts of the
state: the counter (high word), the waiter count (`Parked.Count`, the low bits) and the bubble flag (`Bubbled`). `Add`
at zero with waiters clears all three under `Gate` (it already does: Go's `wg.state.Store(0)`), so a waiter of that
generation reads them non-zero only if a later `Add` ran before its read. Then, outside the lock and after the park
scope:

```
if (reused) {
    throw panic("sync: WaitGroup is reused before previous Wait has returned");
}
```

The twin gets the same read at the same point: after `await slot` and the scope's dispose, it takes `Gate`, reads the
three parts, releases, and throws the same panic. One read under one hold, as in the plain path. The read sits on the
normal path of `waitParkedAsync`, after the await, and a helper that is not async does it. (In that state machine a
`lock` does not release once an await has thrown out of the awaiter's `OnCompleted`: the twin note's section 10.1. On
the normal path it would release; the helper keeps the method free of the question.) A refused park rethrows before
the read and reads nothing.

**Why a generation on `Gate` and not one event per generation.** An event per generation also meets G's shape (a
waiter captures its generation's event under `Gate`, `Add` at zero sets it and drops it, nothing is ever reset). It
costs an allocation for every generation that has a blocking plain waiter, and a second object beside the lock that
already publishes the waiter. The counter allocates nothing and its whole correctness argument is "tested under the
lock that writes it". Its cost: `PulseAll` wakes every thread waiting on `Gate`, a later generation's waiters
included (each re-tests its own generation and waits again), and each released waiter re-takes `Gate` once, one after
another. The re-read of part (b) needs that hold in either shape. I recommend the counter. If G prefers the event,
sections 3 to 6 stand unchanged.

## 3. What the race looks like after the change

The reuse is a race in Go too: a woken waiter that reads the state before the new `Add` returns normally. Both
outcomes stay possible here, decided by who takes `Gate` first. What goes is the third outcome Go does not have: the
waiter that neither returns nor panics.

A waiter woken into a state that went up and came back to zero before its read returns normally, in Go and here.

## 4. Red first, then the rows

| row | where | red at the twin's tip because |
|---|---|---|
| U1 the hang | GolibTests, in-process. The BOARD measurement as a row: one goroutine in the plain `Wait` on a count of 1; once it is parked with the WaitGroup reason the test calls `Done()` then `Add(1)` at once; 100 rounds, a fresh WaitGroup each. Every round's waiter must END before its deadline, by returning or by the panic of section 2 (both are Go's outcomes). The row prints how many ended each way | the waiter of round 1 is still parked at its deadline (measured 100 of 100) |
| U2 the panic, exact | GolibTests, in-process. K plain waiters parked on one count. The test holds `Gate` (by reflection, as BubbledWaitGroupTests reaches `wgAddBubble`) across `Done()` and `Add(1)`, so no waiter can read the state in between. All K end with a PanicException whose text is Go's, byte for byte | the waiters are still parked at the deadline |
| U3 no panic at zero | GolibTests. As U2 with `Done()`, `Add(1)`, `Done()` under the hold: the state is zero at every waiter's read, all K return normally. Go does the same | green at the tip too; it bounds the fix (a check that read "was there ever a later Add" would red it) |
| U4 the flag | GolibTests, inside a synctest bubble. As U2 with `Done()` then `Add(0)` under the hold: the counter is 0 and the bubble flag is set, so Go's state is non-zero. The waiter panics | the waiter returns normally |
| U5 two generations, used correctly | GolibTests. K waiters, drained, all returned; then `Add(1)`, K more waiters, drained. No panic, every waiter returns, on one WaitGroup | green at the tip too; it guards the generation swap |
| U6 the twin | GolibTests, the `GO2CS_MN=1` child of WaitGroupTwinTests. K twinned waiters, the U2 sequence. Each awaited twin throws the panic, and none handed a P off | the twins return normally |
| U7 mixed | The same child: K twinned waiters and one plain waiter in the U2 sequence. All K+1 panic | the twins return, the plain waiter is still parked |
| U8 behavioral | `WaitGroupReuse`, `[GoTestMatchingConsoleOutput]`, compared with `go run`. 20 rounds, a fresh WaitGroup each: 8 goroutines call `wg.Wait()` under a deferred `recover` and report what they recovered; main sleeps 10 ms, calls `wg.Done()` then `wg.Add(1)`, collects the 8 reports, then `wg.Done()`. It prints the number of reports (160), whether any report was the panic (true), and the panic's text. Under the runner's `--mn` the waiter is a twin, so the same row reads the twin | Output differs: no waiter ever panics, so the second line reads false and the third is empty |

U8 prints "any", not a count: in Go a waiter that another P steals during the release loop reads zero and returns, so
the number that panic is not fixed. One round losing the race on every waiter needs main descheduled between two
adjacent calls for longer than 8 wake-ups; 20 independent rounds all doing so is not a rate worth a number.

**Neuters (floor 13), each restored byte-identical.** (1) `Add`'s non-zero arm made to bump the generation back
(the old reset, in the new state): U1 and U2 red by their deadlines. (2) `reused` forced false in the plain path: U2
and U4 red by name, U6 green. (3) the same in the twin: U6 red by name, U2 green.

## 5. Gates

GolibTests Release and Debug, unfiltered, `-p:GoTargetOS=linux`; converter `go test` (repoguard included); CNR; the
behavioral row filtered, plain and `--mn`; `MnWaitGroup` again, plain and `--mn` (its three generations reuse one
WaitGroup correctly, and must stay silent). No twin census: no member is added and no caller's selection changes.
No frozen surface moves: `WaitGroupState` is internal.

## 6. Not changed, and two named limits

- `Add`'s counter, its negative-count panic and the bubble fatals; the zero-count return of `Wait` and of the twin;
  `Go`; the waker's order (ready, then wake).
- **Go's two `Add`-side misuse panics stay unreachable** ("sync: WaitGroup misuse: Add called concurrently with
  Wait", waitgroup.go:121 and :132). Both fire in the window between Go's atomic add that reaches zero and its
  `Store(0)`. Here that window is inside one hold of `Gate`, so no other `Add` can observe it. The same misuse then
  shows at the waiter's read, which is the other place Go reports it. Not in this cut.
- **The frame's position stays the C# one.** "At Go's point" is met as the point in the body: after the wait returns,
  the re-read, then the panic. A traceback through the hand-owned `Wait` prints waitgroup.cs and its line, not
  waitgroup.go:213. The hand-kept `GoPositionMap` record may map `go` statements only: repoguard
  `TestHandOwnedGoStatementsMapToGo` rule (c), from COORD's ruling of 2026-10-19 part 2, refuses any other mapped
  line. Mapping the two throw lines to Go line 213 would need that ruling changed, so it is a question for COORD and
  not a line in this seat.

## 7. Questions for G

1. The counter on `Gate` (section 2), or an event per generation?
2. The twin panics too (section 2b). The twin seat's note and BOARD row say "immune"; that stays true of the sleep
   only, and this note says so.
3. The position limit of section 6: accepted, or is the ruling to be asked?
