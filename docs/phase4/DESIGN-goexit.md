# DESIGN — `runtime.Goexit`: the managed unwind shape

> **Status: IMPLEMENTED (r16).** Ruled by the user 2026-07-26: land §2, take **option C** for the
> main-goroutine case. Built as specified — see [§5 As built](#5-as-built-r16) for the exact shape,
> the two places it differs from the sketch, and the measurements. `TestOnceFuncGoexit` moved
> capability-excluded → **pass**.

## 1. What Go specifies

`runtime.Goexit` terminates **the calling goroutine only**:

1. All of that goroutine's **deferred calls run**, in the usual order.
2. `recover()` inside those defers returns **nil** — Goexit is *not* a panic, and a defer cannot
   cancel it.
3. Other goroutines are unaffected.
4. **From the main goroutine:** `main` terminates without returning, and the *program continues*
   running the other goroutines; if they all exit, the runtime crashes with a fatal
   "no goroutines" error.

Property 2 is the load-bearing subtlety: any implementation on `PanicException` would make
`recover()` observe and swallow a Goexit, which real Go code (sync's `OnceFunc` panic-replay
machinery, `testing`'s `FailNow`) explicitly distinguishes.

## 2. The sketched shape (from the r14 arc — cheap, fits existing machinery)

A dedicated **`GoexitException`** in golib:

- **Not a `PanicException`** — `recover()`'s implementation already keys on `PanicException`, so
  during Goexit unwinding it returns nil with **zero changes** to the recover path. Property 2
  falls out by construction.
- **Unwinds through `GoFunc.HandleFinally`** — the existing defer machinery runs deferred calls
  during the unwind exactly as it does for a panic. Property 1 falls out of the machinery that
  already exists.
- **Swallowed at the goroutine root** — the `GoFunc`/goroutine-thread entry catches
  `GoexitException` specifically and ends the thread silently (a `PanicException` reaching the same
  point remains the fatal-crash path, unchanged). Property 3 falls out of thread isolation.
- The **testing host's** per-test root treats a `GoexitException` escaping a test function the way
  Go's `tRunner` does: the test goroutine ended without completing — which is precisely `FailNow`'s
  contract, so this shape is also the future path for converted `testing.T.Fatal/FailNow` fidelity.

Estimated footprint: one golib exception type, one catch clause at the goroutine root, one in the
test host, and `runtime.Goexit`'s hand-owned body (`throw new GoexitException()`). No converter
change; no emission change.

## 3. The open question — main-goroutine Goexit

The one part with no obviously-right managed shape. Options:

| # | Shape | Fidelity | Cost/risk |
|---|---|---|---|
| **A** | Main thread's root catch **waits for all live goroutine threads**, then exits with Go's fatal "no goroutines" error text and code | Faithful to the spec, including the crash-when-all-exit | Needs a live-goroutine registry (golib's goroutine tracking already counts for the deadlock detector — verify reuse); some care with non-goroutine threads (timers, finalizers) |
| **B** | Main-goroutine Goexit = process exit | Unfaithful (kills goroutines Go would keep running) | Trivial, silently wrong for the exact programs that use it |
| **C** | Implement the goroutine-side shape now; **keep main-goroutine Goexit capability-gated** | Faithful where implemented; honest where not | Zero — the gate exists today; main-goroutine Goexit is vanishingly rare outside runtime tests |

## 4. Recommendation

**Land §2 now; take Option C for the main-goroutine case.** The goroutine-side shape is cheap,
semantically exact on all three properties, unblocks `TestOnceFuncGoexit`, removes a whole
host-crash class, and opens the door to `FailNow` fidelity. Option A can graduate from C later as
its own small arc if a real consumer appears — the capability gate makes the deferral visible
rather than silent. Option B is rejected outright (silently wrong).

**Verification plan when ruled:** behavioral guard `GoexitDefers` (a goroutine calls Goexit; its
defers observably run; `recover()` in those defers prints nil; main continues; output-compared vs
`go run`); sync re-run with `TestOnceFuncGoexit` expected to move capability-excluded → pass; full
suite + 44-package sweep (golib change class); the main-goroutine case stays gated with a
capability-gate positive control.

## 5. As built (r16)

The estimate held: one golib exception type, one goroutine root, one clause in the test host, one
hand-owned `runtime.Goexit`. No converter emission change; no golden re-baseline.

| Piece | Where |
|---|---|
| `GoexitException` — not a `PanicException` | `src/core/golib/GoexitException.cs` |
| The goroutine ROOT (`Start`/`Run`, the Goexit catch, `OnGoroutine`) | `src/core/golib/runtime/Goroutine.cs` |
| All 18 `goǃ` arity overloads funnel into it | `src/core/golib/builtin.cs` |
| `runtime.Goexit`'s hand-owned body + the main-goroutine gate | `src/go-src-converted/runtime/managed_impl.cs`, `src/core/runtime/runtime.cs` |
| Converter: `Goexit` hand-owned; static capability entry removed | `src/go2cs/manualTypeOperations.go`, `src/go2cs/testConversion.go` |
| Per-test root treats an escaping Goexit as `tRunner` does | `src/core/testing/TestHost.cs` |
| Behavioral guard | `src/tests/Behavioral/GoexitDefers/` |

**Confirmed as designed:** `recover()` needed **zero** changes. `GoFunc.Execute`'s catch filter is
`when (RuntimeErrorPanic.TryAsPanic(ex, out var panic))` and `HandleRecover` returns
`CapturedPanic.Value?.State` — a `GoexitException` fails the filter, nothing is captured, and
`recover()` yields nil. `HandleFinally` runs from a `finally`, so the defers pop during the unwind;
`HandleFinally`'s trailing "rethrow the captured panic" never fires because nothing was captured.

**Two things the sketch did not say, both forced by the managed model:**

1. **The goroutine root had to be *created*, not just extended.** There were 18 `builtin.goǃ`
   overloads, each queuing its own work item — no single root to add a catch to. They now all
   dispatch through `Goroutine.Start`, which is the *reason* the policy can be stated once (and the
   place the r16 test-host containment hooks in). Net cost: one extra delegate allocation per
   goroutine.
2. **Option C needs a goroutine-vs-main *runtime* answer, so the static capability entry had to go.**
   Leaving `runtime.Goexit` in `unsupportedRuntimeCapabilities` would keep `TestOnceFuncGoexit`
   excluded, and the gate cannot distinguish the two cases anyway — a call graph says nothing about
   which goroutine will run a function. The gate therefore moved to where the distinction exists:
   `Goroutine.OnGoroutine`, a `[ThreadStatic]` the root sets and **restores** (goroutines run on
   pooled threads, so a finished goroutine must stop looking like one). The test host marks its own
   per-test threads with `Goroutine.Enter()` — in Go a test body *is* a goroutine (`tRunner`).
   `unsupportedRuntimeCapabilities` is now empty but intact, with
   `TestUnsupportedRuntimeCapabilityGate` as its positive control.

**Measured:**

- `GoexitDefers` behavioral guard: C# stdout byte-identical to `go run .`, all 4 phases pass.
- `GoroutinePanicExitCode` still passes — the root catches `GoexitException` only, so a goroutine
  panic keeps its fatal path (stderr + exit 2).
- Main-goroutine gate positive control (scratch converted program calling `runtime.Goexit()` from
  `main`): prints the `NotSupportedException` naming this document and exits **2** — Go exits 2 there
  too, with `fatal error: no goroutines (main called runtime.Goexit) - deadlock!`.
- sync: `TestOnceFuncGoexit` **pass** (was capability-excluded). Package split 22 pass / 14 fail /
  1 infrastructure-error of 51, with 14 tests still unmeasured because `TestPoolChain` panics
  (`interface conversion: interface {} is unsafe.Pointer, not int`) on a goroutine — a converted-code
  defect, and a *panic*, so it correctly keeps the fatal path rather than being contained.

**Option A (faithful main-goroutine Goexit) remains available** and unblocked: the honest gate makes
the deferral visible, and the `OnGoroutine` flag is the hook a live-goroutine registry would build on.

> **Amended 2026-10-25 (lane C2):** option A has a PROPOSED design, [Goexit from the main goroutine](../DESIGN-goexit-main.md),
> ruled for the first .2 seat. Until it lands, option C above is the shape golib ships.
