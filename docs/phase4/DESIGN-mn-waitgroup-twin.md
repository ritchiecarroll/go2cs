# DESIGN — M:N: `sync.WaitGroup.Waitꞏtwin`, a hand-written twin for .5

COORD's seat of 2026-11-27 (docs/phase4/inbox/P4/20261127T194634Z-COORD.md), the owner's ruling quoted there: "Yes,
build in .5". G's design, written out here for G's review before any code.

## 1. Why

`Wait` lives in a hand-owned file of `sync` (waitgroup.cs), so its twin is hand-written too. (The generator does run
over sync in a twin build: the hand-owned mark is on sync's files, not on the assembly. It sees the hand twin and emits
none of its own: section 7 reads `refused has-twin=1` off sync's report. An earlier draft added "as for `Lockꞏtwin`";
the report does not read that way for `Lock`, see section 7.) A goroutine on the
M:N scheduler that calls `wg.Wait()` runs the plain `Wait`, which blocks its carrier thread on `WaitGroupState.Idle`
until the count reaches zero. Every such waiter holds a carrier: with K waiters and P procs, the scheduler starts
carriers past P to keep the Ps running. `Mutex.Lockꞏtwin` (mutex.cs) closed the same hole for a contended lock; this
does it for `Wait`.

## 2. The member

```
[EditorBrowsable(EditorBrowsableState.Never)]
public static GoTask Waitꞏtwin(this ж<WaitGroup> Ꮡwg)
```

In `partial class syncᴛWaitGroup`, beside `Wait`. Its shape matches `Wait(this ж<WaitGroup>)` exactly (same containing
type, static, same parameter display string), so `TwinGenerator.HasTwinMember` finds it and a consumer's call to
`wg.Wait()` becomes a park site.

**No `[GoParkWeak]`.** `Lockꞏtwin` is weak because an uncontended lock never parks and making every locker a twin would
cost more than it saves (N1). A `Wait` with a non-zero count always parks, and its callers are exactly the goroutines
that must give their carrier back, so a caller that parks only through `wg.Wait()` becomes a twin. That is the selection
change this seat makes, and the census in section 6 counts it.

Placed after `Go` in the file. The lines the seat adds ABOVE `Go` (three aliases, the waker's slot wake and two
comment lines) move its `go` statement from line 263 to 269, so the hand-kept `GoPositionMap` record is re-encoded for
the new line (the same Go line, 238) and the two conversion-strategy notes that cite it are re-cited (repoguard
`TestHandOwnedGoStatementsMapToGo` and `TestDocsProvenanceNotesResolve`, both red until then). The first draft of this
note said the record would not move; that was wrong.

## 3. The body

1. **Off a carrier** (`!GoScheduler.OnCarrier`): call the plain `Wait()` and return a completed task. The scheduler off,
   a foreign thread, a goroutine that runs on a thread of its own: all take the path they take today. `OnCarrier` reads
   the MOUNT, not the lock: a goroutine locked to its thread while it is mounted on a carrier is on a carrier and takes
   the twin path. (An earlier draft listed "a goroutine locked to its thread" among the off-carrier cases; P3's NIT.)
2. **On a carrier**, under `s.Gate`, in `Wait`'s own order:
   - read the caller's bubble and `associated` exactly as `Wait` does;
   - **zero count**: return a completed task, after the Go 1.25 disassociation (`Parked` empty, `Bubbled`, `associated`
     → clear both). Unchanged from `Wait`: the same lines, run under the same lock;
   - **non-zero count**: `synctestDurable = s.Bubbled && associated`; register the goroutine AND a new `GoParkSlot` in
     the waiter list; enter `Goroutine.ParkTwin(synctestDurable ? SynctestWaitGroupWait : SyncWaitGroupWait)` while
     still holding the lock (the lock that publishes the waiter to `Add`); release the lock; `await slot`; dispose the
     park scope. If the scheduler refuses the park, the waiter takes its entry back off the list and ends its scope
     before the refusal reaches its caller (section 10.1). The same reasons as `Wait`, so a bubble's `IsIdle` counts a twinned waiter as durably blocked exactly as
     it counts a plain one, and a traceback prints the same wait reason.

The front is not async: the off-carrier path and the zero-count return take no state machine and allocate nothing, as
`Lockꞏtwin` splits its non-async front from an async slow path. The `await` runs in that slow path, outside the lock:
`Gate` is released before the first await, on the entering thread.

**No profiling branch.** Plain `Wait` charges no block profile (unlike `Lock`, whose twin falls back to plain `Lock`
when the block or mutex profile is on), so the twin has nothing to keep equal to it there.

**ONE hold of `Gate`** (P3's MUST). The front reads the count, takes the zero-count branch or registers the waiter, and
enters `ParkTwin`, all under a single `lock (s.Gate)`; the async slow method takes no lock and only awaits the slot. The
count is read under the lock that `Add` writes it under, so an `Add` that reaches zero either runs before the read (the
twin returns at once) or after the waiter is registered (it finds the waiter and wakes its slot). The design that reads
the count, releases the lock and registers later is the lost wake P3 described; it is not the one built. Row R1c races
a `Done` against the twin's entry.

## 4. The waker: `Add` at zero

`WaitGroupState.Parked` becomes a list of `(Goroutine? Goroutine, GoParkSlot? Slot)` pairs: a plain waiter registers a
null slot, a twin its own. At zero, under `Gate`, for each entry: `Goroutine.Ready(goroutine)`, then `slot?.Wake()`;
then `Parked.Clear()` and `Idle.Set()`, as now. Ready before wake is the channel waiter's order (channel.cs,
`Waiter.Wake`): the goroutine is marked runnable before its continuation can be queued. `GoParkSlot` remembers a wake
that arrives before the await commits, so a waker that runs between the lock release and the `await` is not lost.

`Parked.Count` keeps its meaning everywhere it is read (the bubble "busy" argument to `wgAddBubble`, the zero-count
disassociation, the reset of `Bubbled`), so the synctest arms are unchanged.

## 5. Red first, then the rows

| row | where | red at master because |
|---|---|---|
| R1 carriers | GolibTests, a `GO2CS_MN=1` child (GoroutineRunTwinTests' pattern: the scheduler is read once per process). K = 4P twinned goroutines (`goǃtwin`) wait on one WaitGroup. The reading is the scheduler's `handoffs` counter (a carrier that blocks in a plain park hands its P off first) and its carriers started, taken ONLY once all K waiters are parked with the WaitGroup wait reason; that gate has a deadline that fails the row by name, so a read taken before the waiters park cannot pass. TWIN arm (run first, before any carrier past P exists): the body awaits `Waitꞏtwin()`, 0 handoffs and 0 carriers started, and every waiter completes. CONTROL arm: the body calls the plain `Wait()`, handoffs at least K (the hole, measured; the instrument's positive control) | the twin arm does not compile at master (no member) |
| R1b mixed | The same child (P3's SHOULD): K twinned waiters and one goroutine in the plain `Wait` on one WaitGroup; one `Done` finishes all K+1 within the deadline, so a defect in either half of the (goroutine, slot) waker fails by name | no member |
| R1c race | The same child: 20,000 rounds of one twinned waiter against a `Done`. Each round records whether the twin's task was incomplete at return (the `Done` came after the registration) or complete (it came first); the delay before the `Done` follows the last outcome, so the rounds settle at the crossing on any host. Both counts are printed and both must be above zero. A waiter that slept through the `Done` stops the row at its deadline, naming the round (section 10.2) | no member |
| R1d refused park | The same child: a twin body keeps a park on its own slot pending and calls `Waitꞏtwin` in the same step, so the scheduler refuses the twin's park after it registered. The refusal reaches the caller, the goroutine's wait reason is cleared, and the WaitGroup's next `Done` readies nobody (section 10.1) | red at a6959f3f53f: the wait reason stays `SyncWaitGroupWait` |
| R2 zero count | GolibTests, in-process (off a carrier): a zero-count `Waitꞏtwin` returns a completed task; a non-zero one off a carrier blocks on the plain path and completes after `Done` | no member |
| R3 bubble | GolibTests: the zero-count return disassociates a WaitGroup bubbled to the caller's bubble (the 1.25 rule), on the twin as on `Wait` | no member |
| R3b durable | GolibTests, the same child: K twinned waiters inside a synctest bubble, on a WaitGroup added there. `synctest.Wait` returns (the bubble is durably idle), every waiter is parked with `SynctestWaitGroupWait`, and none handed a P off. A goroutine inside a bubble CAN be multiplexed: the test host offers no twin to a synctest test, but `Goroutine.StartTwin` (Goroutine.Multiplexed.cs:77) takes the creator's bubble, so a `goǃtwin` inside one starts a multiplexed member | no member |
| R4 behavioral | `MnWaitGroup`, `[GoTestMatchingConsoleOutput]`, compared with `go run`: K goroutines park in `wg.Wait()` behind a gate, a releaser drains the count, all K report; deterministic output. Under the runner's `--mn` the stderr READING line carries peak-carriers (read, not compared) | Output equal at master too (the plain Wait is correct, only costly); the READING is the red |
| R5 selection | the twin report (`go2cs.twins.report.g.cs`) of R4's `--mn` build lists the function that parks ONLY through `wg.Wait()` as twinned | at master that function is not a candidate |
| R5b selection, standing | GenTests `WaitGroupTwinSelectionTests`: over a model of go.sync's two members under their real names, a function whose only park is `wg.Wait()` gets a twin that awaits `Waitꞏtwin()`; with the member marked weak, and with it absent, the function stays plain (section 10.4) | no row |
| R6 surface | StdlibTwinSurfaceTests: `syncᴛWaitGroup.Waitꞏtwin` frozen (public static, returns GoTask, EditorBrowsable Never, a twin member); the list of public twins in go.sync and go.time gains it | no member |

## 6. Frozen surface, gates and the census

- **Surface.** `Waitꞏtwin` is frozen surface of go.sync, as `Lockꞏtwin` is: R6, plus a dated amendment block in
  FREEZE-mn-surface.md quoting the owner. PublicSurface.golib.txt `[mn-frozen]` lists golib only, so it does not move.
- **Gates.** GolibTests Release and Debug, unfiltered, `-p:GoTargetOS=linux`; converter `go test` (repoguard
  included); CNR; the behavioral row filtered, plain and `--mn`.
- **sync's twin is unconditional.** the twin is hand-written, so sync always carries `Waitꞏtwin`, with or without
  `GoStdlibTwins`, so an opted-in program on a PLAIN stdlib gains it too. R4's `--mn` build shows that.
- **Census.** The stdlib built with `-p:GoStdlibTwins=true` at the base and at the cut; the report's `T` rows diffed.
  Prediction: the newly twinned set is the converted callers that park only through a WaitGroup wait, at most 10
  functions, in net/http, net/http/httptest, net/rpc, internal/singleflight and go/internal/srcimporter (the converted
  non-test files that call it). No function leaves the twinned set. Over 20: a design read before the seat goes on.

## 7. Readings (linux/amd64, 4 Ps, K = 16, Release)

- **Red at master:** the rows do not compile, five CS1061 on `Waitꞏtwin` and nothing else.
- **R1, five child runs:** twin arm `handoffs=0 carriers-started=0` in 5 of 5; bubble arm the same, every waiter
  parked with `SynctestWaitGroupWait`; control arm `handoffs=16` in 5 of 5 (its carriers started read 0 to 8, because
  carriers the cold start injected are still there to block: the handoff count is the reading, not that one).
- **A warm-up round precedes the read.** Cold, the twin arm read 10 to 17 carriers started with 0 handoffs. After one
  round of K waiters the arm reads 0. The cause this bullet first gave (the monitor injects a carrier past a long first
  step) was inferred and is mostly wrong: section 10.3 has the counters.
- **Neuter:** `Waitꞏtwin` made to take the plain `Wait` on a carrier reds the twin arm by name ("16 handed their P off
  for 16 waiters"); restored byte-identical.
- **R4:** `MnWaitGroup` passes all four phases plain and under `--mn`, Output compared with `go run`.
- **R5:** the `--mn` build's twin report reads `candidates=9 twinned=4` with `T main.cs|14|waitAll|sites=1`. With the
  member hidden (renamed, then restored byte-identical) it reads `twinned=3 go-only=1` and has no `waitAll` row.
- **Census** (the stdlib built `-c Release -p:GoStdlibTwins=true -p:GoTargetOS=linux`, twice in one tree: at the cut, and
  at the cut with master's `waitgroup.cs` swapped in, so the member is the one axis; the file restored byte-identical).
  358 twin reports at both arms, summing `candidates=32219 twinned=10518` at both. The `T` rows, keyed by file and
  name, are the SAME SET: no function is newly twinned and none leaves. Eight functions that were already twins gain
  one await site each, their `wg.Wait()`: `parseFiles` (go/internal/srcimporter), `Do` (internal/singleflight), `Close`
  and `Shutdown` (net/http server.cs), `Close` (net/http/httptest server.cs), `ServeCodec` (net/rpc), `testFutureTimeout`
  and `testConcurrentMethods` (vendor/golang.org/x/net/nettest). The `await-sites` lines sum to +8. srcimporter's
  `go-only` reads 1 at the base and 0 at the cut (the report does not name the function; `parseFiles` is the only row
  that moved there). No `outside-twins calls` count moved outside sync, so no converted stdlib `wg.Wait()` sits in a
  function without a twin.
- **The prediction of section 6, scored.** The bound held: 0 newly twinned against "at most 10", and none leave. The
  mechanism it named did NOT show in the stdlib: no converted caller parks only through a WaitGroup wait, every one
  was a twin already through another park site. So in the stdlib this seat changes 8 sites inside existing twins from
  a call that blocks the carrier to a park, and the selection change is shown by R5 alone (`waitAll`). The package
  list missed one of six: nettest, with 2 of the 8 functions.
- **sync's own report (G's check).** At the cut it carries `refused has-twin=1 blocked=0`, a line the base arm does not
  have, with `candidates=125 twinned=6` at both arms and no `T` row for WaitGroup's `Wait`: the generator sees the hand
  twin and emits none. `Lock` does not read the same way: neither arm counts it under `has-twin`. The generator's match
  compares parameter types as written (`TwinGenerator.HasTwinMember`), the plain `Lock` takes `ref Mutex` and
  `Lockꞏtwin` takes the box, so that pair does not match; `Wait` and `Waitꞏtwin` both take the box. No report has a `T`
  row for `Lock` either, so it gets no generated twin, for a reason the report does not name. The other sync lines that
  moved: `outside-twins calls` 331 to 332 (the twin's off-carrier call of the plain `Wait`) and `Go`'s row, line 260 to
  266.

## 8. Reviews (2026-11-27, relayed by COORD)

**P3:** APPROVE WITH CHANGES. MUST: one hold of `Gate` or a re-read (section 3 states the one hold; row R1c). SHOULD:
the mixed-waiter row (R1b). P3 also confirmed that a bubble goroutine can be multiplexed, so R3b is owed.

**G:** APPROVED, with two required changes, both folded above: R1 proves its own premise (the read is gated on all K parked,
with a failing deadline), and R3b answers the bubble question. Also folded: the profiling sentence, the
allocation-free front, and the unconditional-twin census line. A pre-existing defect G named in the plain `Wait` (an
`Add` that takes the count from 0 back up can `Reset` the event under a woken waiter that has not yet re-checked it,
so the waiter sleeps through its generation; Go panics there with "sync: WaitGroup is reused before previous Wait has
returned") goes to the BOARD with this seat, measured first (golib hung 100 of 100 rounds; Go panicked 3 of 3), with no fix in
this cut. The twin is immune to the sleep (its slot keeps the wake), and panics as Go does since the reuse seat
(DESIGN-waitgroup-reuse.md, 2026-11-28; G's ruling, COORD 20261128T015335Z). Since that seat the plain `Wait` has no
event: it sleeps on `Gate` until a generation counter moves, so where sections 1, 4 and 10 of this note name `Idle`
they describe the tree as it was at this seat.

## 9. Not changed

`Wait`'s plain body, `Add`'s counter and panics, the bubble fatals, `Go`, and every caller that is not on a carrier.

## 10. After P3's code read (2026-11-28)

P3 read the code at a6959f3f53f: CLEAR WITH NITS, four SHOULDs and three NITs. G approved the seat for the train and asked
for the cold round as a READING line. Everything below is folded on top of the posted tip. Readings: linux/amd64, 4 Ps,
K = 16, Release.

### 10.1 A refused park (P3 SHOULD 1; P3's design SHOULD 3, not answered before)

The twin registers its waiter and enters its park scope under `Gate`, then awaits its slot outside the lock. The
scheduler can refuse that await: `GoScheduler.ParkPending` throws when the step has already recorded a park.

Measured at a6959f3f53f, before any change:

- **The compiler runs no `finally` of the method once an await has thrown out of the awaiter's `OnCompleted`.** The
  state machine has marked itself suspended by then, and every `finally` body is guarded by that mark. A probe method
  (`try { await slot; } catch { ... throw; } finally { ... }`) whose park was refused read `catch=True finally=False`. So
  the twin's `finally { scope.Dispose(); }` did not run: the scope stayed open AND the entry stayed in `Parked`. (P3
  read the scope as disposed; the entry half of the finding is as P3 wrote it.)
- **The damage, two forms.** With a first WaitGroup's park pending and a second WaitGroup's park refused, the second's
  `Done` marks the goroutine readied while it waits on the first, and the first's `Done` then throws `golib: Ready of
  goroutine N, which is already readied (sync.WaitGroup.Wait)` out of `Add`, before `Parked.Clear` and `Idle.Set`. With
  a park on a plain slot pending, the goroutine keeps the wait reason `SyncWaitGroupWait` while it waits elsewhere.
- **A `lock` written inside the catch does not release.** Its exit is a `finally`, guarded the same way. The first form
  of the fix removed the entry under `lock (s.Gate)` in the catch; the child then stopped in the next `Done`, waiting
  for `Gate` (traced to that line).

The fix: the catch calls `waitRefused`, a method that is NOT async, which removes the waiter's entry under `Gate` and
ends the park scope; then the catch rethrows. The normal path ends the scope after the await. No `finally` is left in
the method.

Row R1d holds it. Plants, each restored (the file byte-identical by sha256 after the last one):

| plant | the row reads |
|---|---|
| the entry is kept | `golib: Ready of goroutine N, which is not parked`, thrown out of the `Done` (P3's scenario) |
| the scope is kept open | `Expected:<Zero>. Actual:<SyncWaitGroupWait>. the refused park's scope ended` |

NOT in this seat: the compiler rule is general. Five other hand-written twins await a park slot: `Lockꞏtwin`
(sync/mutex.cs, inside two `finally` blocks: the park scope and `leave`), three sites in golib's channel.cs and one in
time/time_impl.cs. None is read here beyond `Lockꞏtwin`'s shape; reported to COORD for G.

### 10.2 The race row (P3 SHOULD 2)

The row showed nothing about WHERE its `Done` landed: a spin of at most 255 iterations can leave every round on one
side. It now counts the rounds whose task was incomplete at return, moves the delay toward the crossing after each
round, prints the split and requires both sides. Nine runs of 20,000 rounds: `done-first` 10573 to 10625, `twin-first`
9375 to 9427.

Shown red by a plant: the twin made to read the count under one hold of `Gate` and register under a second (the bubble
arms kept, so the earlier rows pass). First run: `round 143: the waiter slept through the Done that reached zero`.
Restored byte-identical.

### 10.3 The cold round (P3 SHOULD 3, G's READING line)

The warm-up round is printed as `WGTWIN READING twin-cold ...`, reported and not asserted. Both carrier lines now carry
the monitor's counters for the round. Eleven cold rounds, one per child process (`handoffs` and `retakes` read 0 in
every one):

| injections | sensed | carriers-started |
|---|---|---|
| 1 | 3 | 7 |
| 2 | 8 | 13 |
| 1 | 8 | 11 |
| 2 | 7 | 12 |
| 4 | 3 | 10 |
| 2 | 9 | 14 |
| 2 | 9 | 14 |
| 2 | 9 | 14 |
| 1 | 6 | 10 |
| 2 | 11 | 16 |
| 2 | 6 | 11 |

- No waiter hands a P off, cold or warm: the cold cost is not waiters holding carriers.
- carriers-started is below P + injections + sensed in 11 of 11: one below in 10, two below in 1. So the count is the
  pool's own P carriers, made at the first twin start (`GoScheduler.Start`), plus carriers the monitor started.
- The monitor's share is mostly `sensed`, not `injections`: a carrier in a step whose thread sat in a managed wait for
  two monitor ticks with work queued, whose P the monitor takes back. The design's first account (injection past a
  long step) and P3's by-read account both named injection.
- NOT measured: what those carriers wait on. Inferred by read: before its park, a waiter's step reaches one managed
  wait, `Gate`. Cold, the first holder compiles under it while the others wait. A warm round reads 0 for all three
  counters, which fits.

The twin round asserts `handoffs == 0` in every round, and `carriers-started == 0` only in a round where the three
monitor counters read 0; a round the monitor acted in is printed and repeated, three at most, and if all three were
such rounds the line says the carrier count was not asserted. The carrier count is read before the counters (the
monitor counts first and starts the carrier after). Eleven warm rounds: every counter and the carrier count read 0 on
the first attempt.

### 10.4 The selection, standing (P3 SHOULD 4)

R5 was a one-time reading of the twin report. R5b is the standing row. It is a model, not go.sync itself (GenTests
references no converted package): the real member's shape and its missing weak mark are frozen by R6, and R5b holds
what the generator does with a member of that shape. Its two controls (weak, absent) read the other outcome.

### 10.5 NITs

- 5: the control arm waits for the hand-off count to reach K before it reads it (the reason is published before the
  hand-off is counted).
- 6: section 3, item 1 corrected.
- 7: the durable reason shown red by a plant (the twin's reason made non-durable): `synctest.Wait never returned: the
  bubble did not count the twinned waiters as durably blocked`. Restored byte-identical.
- Also: the rows' counter reader matched `handoffs` inside `idle-handoffs` by pattern. It read the right one because
  that one comes first on the line; the pattern now matches a whole name.
