# DESIGN — M:N: `sync.WaitGroup.Waitꞏtwin`, a hand-written twin for .5

COORD's seat of 2026-11-27 (docs/phase4/inbox/P4/20261127T194634Z-COORD.md), the owner's ruling quoted there: "Yes,
build in .5". G's design, written out here for G's review before any code.

## 1. Why

`sync` is hand-owned, so TwinGenerator never runs over it (TwinGenerator.cs, `IsHandOwnedAssembly`). A goroutine on the
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

Placed AFTER `Go` in the file, so the `go` statement in `Go` keeps its line and the hand-kept `GoPositionMap` record
(repoguard `TestHandOwnedGoStatementsMapToGo`) does not move.

## 3. The body

1. **Off a carrier** (`!GoScheduler.OnCarrier`): call the plain `Wait()` and return a completed task. The scheduler off,
   a foreign thread, a goroutine locked to its thread: all take the path they take today.
2. **On a carrier**, under `s.Gate`, in `Wait`'s own order:
   - read the caller's bubble and `associated` exactly as `Wait` does;
   - **zero count**: return a completed task, after the Go 1.25 disassociation (`Parked` empty, `Bubbled`, `associated`
     → clear both). Unchanged from `Wait`: the same lines, run under the same lock;
   - **non-zero count**: `synctestDurable = s.Bubbled && associated`; register the goroutine AND a new `GoParkSlot` in
     the waiter list; enter `Goroutine.ParkTwin(synctestDurable ? SynctestWaitGroupWait : SyncWaitGroupWait)` while
     still holding the lock (the lock that publishes the waiter to `Add`); release the lock; `await slot`; dispose the
     park scope. The same reasons as `Wait`, so a bubble's `IsIdle` counts a twinned waiter as durably blocked exactly as
     it counts a plain one, and a traceback prints the same wait reason.

The count is read under the lock that `Add` writes it under, so an `Add` that reaches zero either runs before the read
(the twin returns at once) or after the waiter is registered (it finds the waiter).

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
| R1 carriers | GolibTests, a `GO2CS_MN=1` child (GoroutineRunTwinTests' pattern: the scheduler is read once per process). K = 4P twinned goroutines (`goǃtwin`) wait on one WaitGroup; main counts the carriers started while all K are parked, then calls `Done`. CONTROL arm: the twin body calls the plain `Wait()` — carriers started rise past P (the hole, measured). TWIN arm: the body awaits `Waitꞏtwin()` — carriers started do not move, and every waiter completes | the twin arm does not compile at master (no member); the control arm is the reading of the hole and stays as the instrument's positive control |
| R2 zero count | GolibTests, in-process (off a carrier): a zero-count `Waitꞏtwin` returns a completed task; a non-zero one off a carrier blocks on the plain path and completes after `Done` | no member |
| R3 bubble | GolibTests: the zero-count return disassociates a WaitGroup bubbled to the caller's bubble (the 1.25 rule), on the twin as on `Wait` | no member |
| R4 behavioral | `MnWaitGroup`, `[GoTestMatchingConsoleOutput]`, compared with `go run`: K goroutines park in `wg.Wait()` behind a gate, a releaser drains the count, all K report; deterministic output. Under the runner's `--mn` the stderr READING line carries peak-carriers (read, not compared) | Output equal at master too (the plain Wait is correct, only costly); the READING is the red |
| R5 selection | the twin report (`go2cs.twins.report.g.cs`) of R4's `--mn` build lists the function that parks ONLY through `wg.Wait()` as twinned | at master that function is not a candidate |
| R6 surface | StdlibTwinSurfaceTests: `syncᴛWaitGroup.Waitꞏtwin` frozen (public static, returns GoTask, EditorBrowsable Never, a twin member); the list of public twins in go.sync and go.time gains it | no member |

## 6. Frozen surface, gates and the census

- **Surface.** `Waitꞏtwin` is frozen surface of go.sync, as `Lockꞏtwin` is: R6, plus a dated amendment block in
  FREEZE-mn-surface.md quoting the owner. PublicSurface.golib.txt `[mn-frozen]` lists golib only, so it does not move.
- **Gates.** GolibTests Release and Debug, unfiltered, `-p:GoTargetOS=linux`; converter `go test` (repoguard
  included); CNR; the behavioral row filtered, plain and `--mn`.
- **Census.** The stdlib built with `-p:GoStdlibTwins=true` at the base and at the cut; the report's `T` rows diffed.
  Prediction: the newly twinned set is the converted callers that park only through a WaitGroup wait, at most 10
  functions, in net/http, net/http/httptest, net/rpc, internal/singleflight and go/internal/srcimporter (the converted
  non-test files that call it). No function leaves the twinned set. Over 20: a design read before the seat goes on.

## 7. Not changed

`Wait`'s plain body, `Add`'s counter and panics, the bubble fatals, `Go`, and every caller that is not on a carrier.
