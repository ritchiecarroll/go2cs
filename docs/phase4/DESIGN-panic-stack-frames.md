# DESIGN — Go's panicking stack, as runtime.Callers sees it from a deferred call

> **Status: PROPOSED (2026-09-27, lane R).** A design record. Nothing is cut. COORD queued it after
> the recover-model seat (iii) as item B. P2 reviews the frame half (§3, §5).

> **Scope.** What `runtime.Callers` / `CallersFrames` report when they are called from a deferred
> call that a panic is running. The execution model does not change unless the chosen option says
> so. `runtime.Stack` is in scope only where it shares machinery.

## 1. What Go does

`gopanic` runs deferred calls ON the panicking goroutine's stack: the frames between the panic site
and the deferring function are still there. A `runtime.Callers(0, pcs)` from the deferred function
therefore reads, top down:

```
runtime.Callers
<the deferred function>
runtime.gopanic
[runtime.panicmem, runtime.sigpanic]   a hardware fault (nil dereference)
[runtime.panicdivide]                  an integer divide by zero
<panic site> ... <callers of the site> ... <the deferring function> ...
```

A panic raised INSIDE a deferred call stacks a second `gopanic` above the first
(TestCallersDoublePanic). A defer that is not open-coded (a defer in a loop) adds
`runtime.deferreturn` below `sigpanic` when the nil deferred func is called at exit
(TestCallersDeferNilFuncPanicWithLoop).

## 2. What the converted runtime does, measured

These readings are from `runtime.tests` built from claude/r-recover-model f8dfffc99a (windows, the
seat (iii) gate's binary):

| Test | wants (after Callers) | got |
|---|---|---|
| TestCallersPanic | func1, gopanic, f3, f2, f1, TestCallersPanic | func1 |
| TestCallersDoublePanic | func1.1, gopanic, func1, gopanic, TestCallersDoublePanic | func1.1, func1 |
| TestCallersNilPointerPanic | func1, gopanic, panicmem, sigpanic, TestCallersNilPointerPanic | func1 |
| TestCallersDivZeroPanic | func1, gopanic, panicdivide, TestCallersDivZeroPanic | func1 |
| TestCallersDeferNilFuncPanic | func1, gopanic, panicmem, sigpanic | func1 |
| …WithLoop | func1, gopanic, panicmem, sigpanic, deferreturn, …WithLoop | func1 |

(`testCallersEqual` drops the LAST frame it reads, so "got" is the live stack minus its bottom frame.)

The mechanism is the same in every row. Deferred calls run from `GoFrame.Run` in the emitted
`finally`, AFTER the CLR has unwound every frame between the panic site and the deferring function.
`captureCallers` (runtime/managed_impl.cs) walks the LIVE managed stack only. The unwound frames still
exist as a snapshot: `PanicException.PanicTrace`, captured once at the first catch. `runtime.Stack`
already appends that snapshot below the live frames (`renderStack`), but `Callers` does not, and
nothing anywhere emits `gopanic` or the runtime fault frames.

One more fact constrains any design. For a panic raised FROM a deferred call, `InheritThrowSite`
OVERWRITES `PanicTrace` with the handled panic's origin, which is correct for `runtime.Stack`'s single
traceback. The re-panic's OWN site is therefore not recorded anywhere, and that site is the `func1`
DoublePanic expects between its two `gopanic`s.

## 3. Options

### A. Run a panicking frame's defers inside the exception filter (P2's candidate shape)

The CLR's first pass runs catch filters with the faulting stack intact. If `GoFrame.IsPanic` ran the
frame's deferred calls there, a `Callers` from a defer would see the real frames natively. The
dispatcher's own frames stand in for `gopanic`, and only the fault frames would still need synthesis.

Costs and hazards, each checked against this runtime:
1. **A panic raised in a filter is SWALLOWED.** The CLR treats an exception thrown inside a filter as
   the filter evaluating to false, and the exception is lost. Go's replacement rule (a deferred call's
   panic replaces the one running, and the remaining defers still run: TestAbortedPanic,
   TestIssue43920, `defer panic(v)`) would need catch-and-park inside the filter, then a forced catch
   and a rethrow of the parked panic from the catch body. Any path that misses the park makes a Go
   panic VANISH, which is the worst failure mode available.
2. **Cleanup order inverts against non-Go `finally` blocks.** During the first pass, no inner
   `finally` has run yet. That includes GoFrame.Run's own save/restore finally, hand-owned
   `try/finally`, `lock` and `using` in golib and in `*_impl.cs`. An outer frame's Go defers would run
   BEFORE inner frames' C# cleanup. A defer that takes a lock an inner hand-owned frame still holds
   deadlocks when the lock is not reentrant (SemaphoreSlim-based). With `lock` it re-enters and sees
   half-updated state. Today's order (inner cleanup, then outer defers) is the only one that keeps
   hand-owned invariants.
3. **Recover's contract.** Recovery must stop the unwind at the deferring frame, so the filter
   returns true and the catch body returns normally. That fits the seat (iii) model, but only if every
   deferring frame's filter runs its defers exactly once, and the `finally` must then NOT run them
   again. That is a two-phase state machine in every frame.
4. **Reach.** The filter needs the frame (`IsPanic(ex, out p, ref ᒐ)`). That is a converter change in
   EVERY function that defers, so a corpus-wide footprint, plus a matching Goexit path (GoexitException
   runs defers from the `finally` today).
5. **Panic-path cost.** It is small per panic, but user code running during exception dispatch is
   hostile to debuggers and to the CLR's own assumptions (stack overflow in a filter, nested dispatch).

### B. Splice Go's logical panicking stack into Callers (recommended)

Keep the execution model. Make `captureCallers` present what Go's unwinder would report:
1. **A per-thread stack of running panic sequences.** Each `GoFrame.Run` activation that is running a
   panic pushes `(panic)` and pops on exit. This sits where seat (iii) already saves and restores
   `RecoverablePanic`, and it can replace that slot, since the innermost entry IS the recoverable
   panic.
2. **Each panic's OWN site.** Add `PanicException.SiteTrace`, snapshotted at adoption, which
   `InheritThrowSite` does NOT overwrite. `PanicTrace` keeps its inherited meaning for `runtime.Stack`.
   Cost: one more `StackTrace` capture, on the re-panic path only. A first adoption reuses the same
   capture.
3. **Each panic's KIND**, set where it is adopted:
   - `RuntimeErrorPanic.TryAsPanic` maps `NullReferenceException` and golib's
     `NilPointerDereference()` to fault (`panicmem`, `sigpanic`), and `DivideByZeroException` to
     `panicdivide`;
   - an explicit `panic(v)` gets none;
   - the index and slice checks map to Go's `goPanicIndex` family: named here, NOT claimed.
4. **The walk.** While walking the live stack top down, each `GoFrame.Run` frame met is paired with
   the next entry of (1). If the entry is a panic, emit `runtime.gopanic`, then the kind frames, then
   the panic's `SiteTrace` Go-source frames up to (not including) the frame that owns the sequence,
   then resume the live walk. The owning frame is the next live Go-source frame, so it is not
   duplicated. Normal-return Run frames contribute nothing.

   **This REQUIRES `GoFrame.Run` to be `[MethodImpl(MethodImplOptions.NoInlining)]`.** It carries no
   attribute today. Its exception handling kept older JITs from inlining it, but that is a JIT
   heuristic and not a contract. An inlined Run leaves no frame to pair, and every splice silently
   shifts by one sequence. This is the same rule captureCallers already states for its own entry
   points.
5. **The fault frames.** `runtime.gopanic` and friends are interned as caller records like any
   other, so `CallersFrames` resolves them and a PC does round-trip arithmetic inside its span.

What the tests would read:
- **DoublePanic:** splicing twice (inner Run → panic 2's site `func1`; outer Run → panic 1's site)
  yields `func1.1, gopanic, func1, gopanic, TestCallersDoublePanic`.
- **The nil-defer thunk** (seat iii item C) is adopted with the fault kind and its site is the Run
  frame, so it yields `gopanic, panicmem, sigpanic`.
- **`deferreturn` (the loop form) is NOT reproduced.** It needs the open-coded vs frame-record
  distinction, which only the converter knows (a defer in a loop). Named as a residual, and a
  one-bit converter hint could close it later.

What B does NOT change is the observable order of deferred calls against non-Go cleanup, because
that order is already the model. Its cost falls only on `Callers`, which already builds a
file-info `StackTrace`, plus one push and pop per panic sequence (none on the no-panic path).

### C. Structural disclosure

Disclose the six rows as structural and cut nothing. The frames are observable only through
`Callers`/`Stack`/pprof from inside a panic's deferred call. That is cheap and honest, but B's reach
is small (golib `GoFrame`/`PanicException`/`RuntimeErrorPanic`, and runtime `captureCallers`), so it
buys five of the six rows for little.

## 4. Recommendation

B. A's one advantage, native frames, is bought with a swallowed-panic hazard, an inverted cleanup
order and a corpus-wide emission change. B reproduces the same frames for `Callers` where they are
read, keeps every execution-order property the recover seat just established, and costs nothing when
no panic is running. C remains the fallback for any row B cannot reach (`deferreturn`), stated per row.

## 5. Gate a cut would owe

- **Red-first** golib arms for the splice shapes, plus runtime `TestCallersPanic`, `DoublePanic`,
  `NilPointerPanic`, `DivZeroPanic`, `DeferNilFuncPanic` → pass on windows and linux. `…WithLoop`
  stays a named divergence (`deferreturn`).
- **No regression:** every `runtime` Callers/Caller/Stack test by name (TestCallers, the io flatten
  depth tests: no panic, so the walk is unchanged), runtime/pprof, runtime/debug's
  TestSetCrashOutput, GolibTests in both flavours, and the full behavioral suite.
- **P2** reviews §3.B.4, the pairing of Run frames to sequence entries, against the inlining and
  NoInlining rules captureCallers already states.
