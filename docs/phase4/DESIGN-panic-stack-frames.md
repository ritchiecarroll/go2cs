# DESIGN — Go's panicking stack, as runtime.Callers sees it from a deferred call

> **Status: APPROVED as option B WITH P2's FOUR CHANGES (COORD, 2026-09-27).** Written by lane R and
> queued by COORD after the recover-model seat (iii) as item B. P2 reviewed the frame half: "OK with
> four changes, pairing required" (claude/mailbox e3a5725cff). Those changes are written into §3.B
> below, each marked **[P2-n]**, and the rule they replace is struck, not deleted. Nothing is cut yet.
> ORDER: seat (iii) lands, then P2's M3 (claude/p2-stack-roots) lands, then B is cut on the M3 base
> with P2 reviewing.

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

**Go's exact answer, MEASURED by P2** (linux amd64, go1.24.13, a go test probe, Callers from the
deferred call after recover). These are the cut's arm expectations:
- plain panic: Callers, dump, TestPlain.func1, `runtime.gopanic` panic.go:792, TestPlain,
  testing.tRunner testing.go:1792, runtime.goexit asm_amd64.s:1700
- nil deref: … func1, gopanic panic.go:792, `runtime.panicmem` panic.go:262, `runtime.sigpanic`
  signal_unix.go:925, TestNil (at the faulting line), tRunner, goexit
- divide: … func1, gopanic panic.go:792, `runtime.panicdivide` panic.go:241, TestDiv, tRunner, goexit
- defer in a loop plus a nil deferred func: … gopanic, panicmem, sigpanic, `runtime.deferreturn`
  panic.go:610, TestLoop, tRunner, goexit

Windows is NOT measured. By source, `sigpanic` there is signal_windows.go:401.

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

(`testCallersEqual` drops the LAST frame it reads, so "got" is the live stack minus its bottom frame.
**[P2-3]** P2's M3 changes this column: it appends `runtime.goexit` after the walk, so the test
function is no longer the dropped bottom frame. The cut's red arms are read on the M3 base, not
against this table.)

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
1. **A per-thread stack with one entry per deferring Run activation.** ~~Each `GoFrame.Run`
   activation that is running a panic pushes `(panic)` and pops on exit.~~
   **[P2-1] REQUIRED.** Every `GoFrame.Run` activation that runs deferred calls (its `m_count > 0`
   branch) pushes ONE entry holding that frame's OWN panic (`owned`, or null) and pops it on exit. It
   does not hold `handling ?? outer`.

   The panic-only form was positional and wrong in a reachable shape. A normal-return Run is on the
   stack too, for example when a panic's deferred call invokes a helper that defers and the helper's
   deferred call calls Callers:

   ```
   Callers, helper.func1, Run (normal), helper, func1, Run (panic), TestX
   ```

   Here the normal Run took the panic's entry, and the splice landed one sequence too high.

   A Run with `m_count == 0` runs no user code, so no Callers can sit above it, and it needs no entry.
   When a deferred call's panic REPLACES the handled one inside a Run (the TestAbortedPanic shape), that
   Run's entry is UPDATED to the new panic, so later defers in the same sequence splice what Go shows.

   The entry sits where seat (iii) already saves and restores `RecoverablePanic` and
   `HandledPanicValue`. Seat (iii)'s `RecoverablePanic` is the innermost entry's panic, so the stack
   can replace that slot.
2. **Each panic's OWN site.** Add `PanicException.SiteTrace`, which `InheritThrowSite` does NOT
   overwrite. `PanicTrace` keeps its inherited meaning for `runtime.Stack`.

   **[P2-2]** `SiteTrace` is the trace of the RAISED exception as caught by its owning frame,
   `new StackTrace(ex, true)` at the catch. It spans exactly the throw site to the catching frame, so
   the owner is its LAST Go frame. For a re-panic, the raised exception's own trace (throw site to
   Run's inner catch) yields `func1` between the two `gopanic`s, which is TestCallersDoublePanic's
   want. It is NEVER the inherited `PanicTrace`.

   Cost: one `StackTrace` capture per adoption. A first adoption shares the capture `PanicTrace`
   already takes, so only the re-panic path pays a second one.
3. **Each panic's KIND**, set where it is adopted:
   - `RuntimeErrorPanic.TryAsPanic` maps `NullReferenceException` and golib's
     `NilPointerDereference()` to fault (`panicmem`, `sigpanic`), and `DivideByZeroException` to
     `panicdivide`;
   - an explicit `panic(v)` gets none;
   - the index and slice checks map to Go's `goPanicIndex` family: named here, NOT claimed.
4. **The walk.** While walking the live stack top down, each `GoFrame.Run` frame met is paired with
   the next entry of (1). Every deferring Run has one, per [P2-1], so the pairing is exact. If the
   entry is a panic:
   - emit `runtime.gopanic`, then the kind frames, then the panic's `SiteTrace` Go-source frames up to
     (not including) the frame that owns the sequence;
   - then resume the live walk.

   A null entry (a normal-return Run) contributes nothing.

   **[P2-2] The owner frame is CHECKED, not assumed.** Drop `SiteTrace`'s last Go frame only after
   asserting its `MethodBase` equals the next live Go frame of the walk. On a mismatch, splice NOTHING
   and answer today's live stack. A missing splice is a known divergence; a wrong one is a silent lie.

   **[P2-4] Run frames are matched by `MethodBase` IDENTITY** (a cached handle for `GoFrame.Run`),
   never by name.

   **[P2-3] Spliced frames go through the SAME skip and capacity path as live frames** in M3's
   reshaped `captureCallers` loop: skip first, then the capacity check, then store. Go's skip counts
   `gopanic`, and a full buffer ends the walk from the top.

   **This REQUIRES `GoFrame.Run` to be `[MethodImpl(MethodImplOptions.NoInlining)]`.** It carries no
   attribute today. Its exception handling kept older JITs from inlining it, but that is a JIT
   heuristic and not a contract. An inlined Run leaves no frame to pair, and every splice silently
   shifts by one sequence. This is the same rule captureCallers already states for its own entry
   points.
5. **The fault frames.** `runtime.gopanic` and friends are interned as caller records like any
   other, so `CallersFrames` resolves them and a PC does round-trip arithmetic inside its span.

   **[P2-3]** They are interned with M3's `internRootFrame(function, file, line)`, which interns a
   record no live call site backs, so every walk answers one PC per Go call site. Files are spelled
   GOROOT-relative, as the other stdlib records are: `runtime/panic.go`. Go has one call site per
   spliced frame per GOOS (`gopanic` always reports panic.go:792), so today's key, the function, is
   enough. If B ever models `sigpanic`'s two paths (signal_unix.go 915 and 925), the line joins the key.

   **[P2-4] Named, not folded:** `runtime.gopanic` is also a real converted method. If a live
   `gopanic` frame is ever walked (Go code calling it directly), it interns a SECOND record for the
   function with a different Entry. That is harmless to the six rows, and the cut names it at the
   site.

What the tests would read:
- **DoublePanic:** splicing twice (inner Run → panic 2's site `func1`; outer Run → panic 1's site)
  yields `func1.1, gopanic, func1, gopanic, TestCallersDoublePanic`.
- **The nil-defer thunk** (seat iii item C) is adopted with the fault kind and its site is the Run
  frame, so it yields `gopanic, panicmem, sigpanic`.
- **`deferreturn` (the loop form) is NOT reproduced.** It needs the open-coded vs frame-record
  distinction, which only the converter knows. Named as a residual. A one-bit converter hint could
  close it later.

  **[P2]** That hint must be Go's OPEN-CODING RULE, not merely "a defer in a loop": no defer in a
  loop, at most 8 defers, and returns × defers at most 15.
- **Not covered, accepted [P2]:** CPU-profile stacks sampled inside a deferred call. The sampler walks
  threads through EventPipe, not captureCallers, so they will not show `gopanic`. The heap and block
  recorders go through `callers()` and WILL show it, as Go's do.

What B does NOT change is the observable order of deferred calls against non-Go cleanup, because
that order is already the model. Its cost falls on `Callers`, which already builds a file-info
`StackTrace`.

~~Plus one push and pop per panic sequence (none on the no-panic path).~~ **[P2-1]** Plus one push
and pop per DEFERRING RETURN, beside the existing `HandledPanicValue` save/restore. That branch already
pays a thread-static read and write on the no-panic path. The cut re-reads seat (iii)'s defer cost
benchmarks with it.

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

- **Red-first** golib arms for the splice shapes, with expectations taken from P2's measured Go lists
  (§1). They must include:
  - [P2-1]'s helper-defers shape (a normal Run above a panic Run);
  - an AbortedPanic-shaped entry update;
  - [P2-2]'s mismatch arm (the owner check fails, so there is no splice).

  Arms are read on the M3 base. Then runtime `TestCallersPanic`, `DoublePanic`, `NilPointerPanic`,
  `DivZeroPanic`, `DeferNilFuncPanic` → pass on windows and linux. `…WithLoop` stays a named divergence
  (`deferreturn`).
- **Cost:** seat (iii)'s PerfDefer benchmarks (BenchmarkDefer/10/Many/PanicRecover bodies), before and
  after, per [P2-1]'s restated cost.
- **No regression:** every `runtime` Callers/Caller/Stack test by name (TestCallers, the io flatten
  depth tests: no panic, so the walk is unchanged), runtime/pprof, runtime/debug's
  TestSetCrashOutput, GolibTests in both flavours, and the full behavioral suite.
- **P2** reviews the cut. P2 agrees with NoInlining on `GoFrame.Run`, in the same seat.
