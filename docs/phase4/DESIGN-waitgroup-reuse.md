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

## 8. After G's read: the answers, the cost reading, and one addition

**G (COORD 20261128T015335Z): APPROVED, with one required reading.** Confirmed: no lost pulse (the generation is
re-tested under `Gate` around `Monitor.Wait`), the lock order, that the twin never shares `Generation`, and the bubble
flag (U4). The answers to section 7: (1) the counter on `Gate`; (2) yes, the twin panics too, and the twin note and the
BOARD row now say "immune to the sleep; panics as Go does since this seat"; (3) the position limit of section 6 is
accepted as named.

**The required reading: the default-mode cost of the plain `Wait`.** linux/amd64, 4 processors, Release, `GO2CS_MN`
unset. A generation is `Add(1)`, the waiters in the plain `Wait`, `Done()`. Each number is the median of 5 repetitions
in one process, in ns per generation; every run is listed. "Before" is the Wait twin's tip 6204ef9c12f with the same
harness on the same box. The harness is `ChildWaitGroupCostReading` in GolibTests WaitGroupReuseTests.cs, which does
nothing unless `GO2CS_TEST_WGREUSE_COST=1`.

Where `Done` follows the registration at once:

| shape | before | section 2 as written | with the spin (the cut) |
|---|---|---|---|
| 1 waiter, 100,000 generations, the goroutines kept | 2312 1880 1897 2049 1966 2133 | 18856 18679 | 1851 1807 1951 2147 1968 1883 |
| 1 waiter, 100,000 generations, a goroutine per generation | 8963 8396 9071 8401 7761 8401 | 30131 31002 | 8579 8884 9334 9038 8458 8420 |
| 64 waiters on one group, 10,000 generations | 296819 290370 246285 256576 248324 259973 | 1994641 2110708 | 252200 271183 243642 272299 249945 247588 |

Where every waiter is asleep before `Done` (the test holds 1 ms before `Done`; the 1 ms is subtracted), run in both
orders:

| shape | before | with the spin (the cut) | the spin, sleeping on a second lock (not adopted) |
|---|---|---|---|
| 1 waiter, 5,000 generations | 87300 87348 81738 81733 | 91021 88234 84177 79914 | 78618 85055 79624 83242 |
| 64 waiters, 1,000 generations | 1819257 1778908 1809697 1809166 | 1991009 1986897 1987510 1841767 | 1776361 1859044 1914829 1937388 |

**Section 2 as written was 7 to 9 times slower** where `Done` follows closely. `ManualResetEventSlim.Wait` spins before
it sleeps (its `SpinCount` read 35 on this box; it is 1 on a single processor), and in those shapes the `Done` arrives
inside the spin. `Monitor.Wait(Gate)` does not spin, so every generation paid a thread sleep and wake and then a queue
for `Gate`.

**The addition: the same spin, written out.** After the waiter has registered and left `Gate`, and before it takes
`Gate` again to sleep:

```
int spins = Environment.ProcessorCount == 1 ? 1 : 35;       // ManualResetEventSlim's own defaults
SpinWait spinner = default;

while (spinner.Count < spins && Volatile.Read(ref s.Generation) == generation) {
    spinner.SpinOnce(sleep1Threshold: -1);
}

Monitor.Enter(s.Gate, ref locked);

while (s.Generation == generation) {
    Monitor.Wait(s.Gate);
}
```

The spin's read is a hint. The test that decides is still the one under `Gate` before every `Monitor.Wait`, so the
no-lost-pulse argument G confirmed is unchanged. With it the three close shapes read level with before (means -5, +3
and -4 percent, inside the spread of the runs), and so does one sleeping waiter (+1.5 percent).

**`Add` advances the generation and pulses only when a plain waiter was registered** (an entry whose slot is null). A
WaitGroup whose waiters are all twins takes no pulse.

**The recorded cost: 64 sleeping waiters read 8 percent slower** (mean 1,951,796 against 1,804,257 ns; the four runs
are +1 to +12 percent against the before runs). That is about 2.3 us per waiter per generation, against a wake that
costs about 28 us per waiter in both versions. By read it is the cost section 2 named: each woken waiter re-takes
`Gate`, one after another, where the event's waiters queued on the event's private lock; here they queue behind the
next generation's `Add` and registrations. The other side is the event, which is no longer allocated for a WaitGroup.

Tried and not adopted: `Generation` written and tested under a lock of its own, taken inside `Gate` by `Add` only, with
Go's re-read under `Gate` alone. Four runs read -2 to +9 percent against before (mean +4): this box cannot separate it
from the `Gate` form, and it would move the confirmed argument onto a second lock. Not built: the three parts of the
state kept in one atomic word, as Go's state is, which would take the re-read off `Gate` at the price of a write at
every site that changes the state.

**Red first, as run.** At the twin's tip: U1 red in round 1 (still parked 2000 ms after the `Done`), U2 red (7 of 8
waiters returned normally, 1 still parked), U4 red (returned normally), U6 red (16 of 16 twins returned normally), U8
red in both modes (`a waiter panicked: false`; the plain run took 10.8 s through the program's fallback). U3 and U5
green, as section 4 predicted. U7 was not reached at that commit: it follows U6 in the same child, and the child stops
at its first failed row. With the fix U1 prints `hang rounds=100 returned=0 panicked=100` and U6 prints
`panicked=16 handoffs=0`.

**The three neuters, as run (each restored byte-identical by sha256; 20 of 20 rows green after the restore).**
(1) `Add` from zero back up steps the generation back: U1 red in round 1 and U2 red (0 of 8 ended), both by their
deadlines, as section 4 said. Also red, and not named in section 4: U3 (0 of 8 ended: the second `Done` finds no waiter
listed, so it does not step the generation forward again) and U7 (the plain waiter still parked). U6 green. (2)
`reused` forced false in the plain path: U2 red by name ("waiter 0 panicked: it returned normally"), U4 red by name,
U7 red by name ("the plain waiter panicked: it returned normally"), U6 green, and U1 reads `returned=100 panicked=0`.
(3) The same in the twin's read: U6 red by name ("16 of 16 returned normally"), U2 and every in-process row green.
