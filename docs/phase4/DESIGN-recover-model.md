# DESIGN — Go's recover(): the panic chain and direct-call eligibility

> **Status: PROPOSED (2026-09-27, lane R).** A written design, owed before any cut by COORD's ruling
> on the runtime-claims seat. Nothing here is implemented. §6 is the gate the cut owes, and §7 names
> what the model still does NOT reproduce.

> **Scope.** What `recover()` returns, and when a panic continues past a deferred sequence. This is
> the golib panic core (`GoFrame`, `GoFuncRoot`, `builtin.recover`). The converter and the emitted
> frame shape do not change.

## 1. What Go does (runtime/panic.go, `gorecover` / `gopanic`)

Two rules. golib reproduces neither today.

1. **Eligibility is DIRECT.** `recover()` returns the panic value only when it is called directly by
   a deferred function that the panic sequence itself invoked (`argp == p.argp`). Examples that
   return nil:
   - a recover in a defer that runs because some OTHER function returned normally, even when that
     function is itself a deferred call run by the panic;
   - a recover in a helper that the deferred function calls.
2. **Panics form a CHAIN.** A panic raised inside a deferred call while another panic is running
   pushes onto the chain.
   - If the NEW panic is recovered and its deferred call then returns normally, the OLD panic is
     still in flight, unrecovered and recoverable.
   - If the new panic is recovered in the SAME frame whose defers the old panic was running, the
     old panic is ABORTED (TestAbortedPanic).

## 2. What golib does, measured

The panic state is ONE thread-local slot, `GoFuncRoot.CapturedPanicValue`:
- the emitted catch (`GoFrame.Capture`) parks the panic in it;
- `recover()` returns whatever is there and clears it;
- `GoFrame.Run`'s tail re-raises only while the slot is non-null.

`runtime.tests` was rebuilt from the runtime-claims tip (3fae75de81, windows) and run over all 14
tests in `defer_test.go`: **11 PASS, 3 FAIL.**

| Test | Reads | Rule broken | golib mechanism |
|---|---|---|---|
| `TestRecoverMatching` | `wanted nil recover, got panic1` | 1 (direct) | The slot is per THREAD, so a recover in a normal-return defer, nested inside a panic-run deferred call, reads and CLEARS the outer panic. |
| `TestIssue43920` | `have <nil>, want 1` | 2 (chain) | A deferred call raised panic 3, which OVERWROTE panic 1 in the slot. Recovering 3 cleared it, so the outer frame's recover saw nil. |
| `TestIssue43921` | `have <nil>, want 1` | 2 (chain) | Panic 4 was captured over panic 1 inside a deferred call, and recovering 4 cleared the slot. The inner frame's `Run` tail then found it null and SWALLOWED panic 1, so the function returned normally. |

The eleven passes must stay passes:
- `TestAbortedPanic`, the abort half of rule 2;
- `TestIssue43941`, a 14-step chain;
- `TestDeferWithRepeatedRepanics`, `TestIssue37688`, `TestDisappearingDefer`, the open- and
  non-open-coded defer mixes, and the rest.

## 3. The model

The idea is to track the panic a deferred call may recover, per deferred SEQUENCE, instead of
tracking "the last panic seen on this thread". Three changes, all in golib.

**(a) `PanicException.Recovered`**, a per-instance flag.
- `recover()` sets it.
- `GoFrame.Capture` (a catch adopting an in-flight panic) CLEARS it. A panic arriving at a catch is
  unrecovered by definition, so a hand-owned rethrow of an instance that was once recovered can
  never make a later frame swallow it.

**(b) `GoFuncRoot.RecoverablePanic`**, a thread-local: the panic whose deferred sequence is running
right now, or null.
- `GoFrame.Run` sets it ONCE at sequence start to `handling` (the claimed panic, or null for a
  normal-return sequence).
- It updates it when the loop adopts a panic raised by a deferred call (the existing catch arm,
  which already replaces `handling`).
- It RESTORES the outer value in the existing `finally`, the one that already save/restores
  `HandledPanicValue`.

`recover()` becomes:

```csharp
PanicException? p = GoFuncRoot.RecoverablePanic;
if (p is null || p.Recovered) return null;
p.Recovered = true;
return p.State;
```

**(c) `Run`'s tail re-raises `handling` when `!handling.Recovered`**, replacing the check
`owned is not null && CapturedPanicValue is not null`. `owned` still decides what the frame claims
at entry. The foreign-unwind correction (`m_foreignRethrow`) is unchanged.

Why each failing test turns green:
- **`TestRecoverMatching`**: the nested defer runs in the deferred function's OWN normal-return
  sequence, where `RecoverablePanic` is null, so its recover reads nil. That sequence's `finally`
  restores panic1, and the outer defer recovers it.
- **`TestIssue43920`**: panic 3 is adopted by the normal-return sequence of the deferred function
  D2 and recovered there. D2's `finally` restores panic 1 as the outer sequence's recoverable panic,
  and D1 recovers it. Panics 4 and 5 run the same way one level down, and panic 4 is aborted exactly
  as in Go.
- **`TestIssue43921`**: panic 4 is recovered in Dx's own sequence. The inner frame's tail finds
  `handling` = panic 1 unrecovered and re-raises it, and the outer frame recovers 1.
- **`TestAbortedPanic` (stays green)**: panic2 REPLACES `handling` inside the same loop, so after it
  is recovered the next defer's recover reads a recovered panic and returns nil. Panic1 is aborted
  and the tail does not re-raise.

**`CapturedPanicValue` keeps its other job**, the catch→finally handshake (`Capture` / `ClaimPanic`),
and stops being what `recover()` reads. `InFlightPanic` is the traceback's view,
`HandledPanicValue ?? CapturedPanicValue`. It reads the slot directly, not through `recover()`.

Today a recover CLEARS the slot, so the fallback half stops seeing the panic. Under this design a
recover no longer clears it. The cut therefore owes one line: `recover()` still clears
`CapturedPanicValue` when it recovers the panic that slot holds. That keeps the traceback view
byte-identical and changes only what `recover()` returns.

## 4. Cost

The per-deferred-call path gains nothing:
- one thread-local write at sequence start, skipped when the value is already null, which is the
  common no-panic case;
- one restore in a `finally` that already exists;
- one field write per `recover()`.

The hot case is a mutex-unlock defer in a sequence that is not panicking. It pays one `ThreadLocal`
read. The cut measures that with runtime's own `BenchmarkDefer` / `BenchmarkDefer10` /
`BenchmarkDeferMany` (runtime_test.go) before and after, matched on load and warmth. It reports the
absolute ns/op with its box and tree named.

## 5. Surface census

Measured at 3fae75de81 by name across `src/core` and `src/gen`. Every reader and writer of the slot
family (`CapturedPanic*`, `HandledPanic*`, `UnclaimedPanic`, `ClaimPanic`, `InFlightPanic`) is in:
- `golib/GoFrame.cs` (6)
- `golib/GoFuncRoot.cs` (13)
- `golib/PanicException.cs` (1)
- `runtime/managed_impl.cs` (1, the `InFlightPanic` read)

`builtin.recover` is the only reader of the slot's value. No converted file names any of these; the
converter emits only `GoFrame.IsPanic` / `Capture` / `Run` and `recover()`.

## 6. The gate the cut owes

- **Red-first:** golib unit arms for the three failing shapes and the abort shape, written against
  golib alone and red at base where they claim to be. Then runtime `defer_test.go`, all 14 by name:
  14/14 on windows and linux.
- **No regression:** GolibTests in both flavours; the full behavioral suite (`DeferPanicArg`, the
  recover and re-panic tests); CNR need not run, since no converter change is made.
- **Rows** whose suites lean on the panic core:
  - `sync` (OnceFunc / OnceValue replay a panic with `panic(recover())` from a deferred call);
  - `testing`-adjacent rows;
  - `encoding/json` and `text/template` (both recover internally to unwind parsers);
  - `net/http` (TestInterruptWithPanic).

  All are swept on windows, and linux takes the three largest.
- **Cost:** the §4 reading.

## 7. What the model does NOT reproduce — stated, not hidden

- **Helper-called recover.** A `recover()` in a helper that the deferred function calls still
  recovers here, because `RecoverablePanic` is a property of the sequence, not of the call depth.
  Go returns nil. Reproducing it needs call-depth identity:
  - Every function would have to know whether it was the delegate `Run` invoked, which is a
    converter prologue in every function containing `recover()`.
  - It also needs `defer f()`'s wrapper lambda to hand that identity to `f`.

  No test in `defer_test.go` exercises it. Code that DEPENDS on a helper's recover returning nil is
  rare, and code that works only because the helper's recover succeeded is a bug in Go. So the gap
  can only make a converted program recover where Go would crash. **Stated as a known divergence; a
  follow-up only if a measured row needs it.**
- **`runtime.Goexit` interplay** is unchanged: `GoexitException` is not a `PanicException`, so it
  never becomes recoverable.
