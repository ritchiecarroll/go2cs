# DESIGN — a minimal managed execution tracer (Q28)

**Design only. Nothing here is cut, and the increments below are proposals with bars, not a plan of
record.** Dated 2026-09-04; amended by dated blocks.

This exists because `runtime/trace` was measured `0 of 2` and the row was ruled **unimplemented, not
untestable** (`RECON-runtime-trace-row.md`): both its tests are satisfiable by an implementation that
does not exist yet, which is expensive rather than impossible. This document sizes that
implementation against two acceptance bars, and states plainly what it does not buy.

## The two bars

**Bar A — the row's own suite.** `TestTraceStartStop` needs `trace.Start` to succeed, the buffer to
be **non-empty** after `Stop`, and **no writes after stop**. `TestTraceDoubleStart` needs a bare
`Stop()` to be harmless, the first `Start` to succeed and the **second to fail**. Neither test parses
the stream or inspects a single event. Bar A is therefore a *state machine plus any non-empty
output*, and it is important to say so, because clearing Bar A alone would be **laundering**: the two
verdicts would go green while nothing about the program had been observed. **Bar A is not proposed as
a stopping point.**

**Bar B — `go tool trace` opens a managed program's trace.** This is the bar that makes the work
honest, because the parser is an oracle we do not control: `internal/trace` rejects a stream that is
not self-consistent. Bar B is what turns "the tests pass" into "the trace means something".

## What the format actually demands

Read from the Go 1.23.12 sources rather than assumed:

- **Header** — `"go 1.%d trace\x00\x00\x00"`, so the literal `go 1.23 trace` followed by three NULs.
  The reader `Fscanf`s it and rejects an unknown version outright.
- **Batches** — every batch opens with an `EvEventBatch` (or `EvExperimentalBatch`) type byte; the
  reader errors with *expected batch event* on anything else. A batch is bound to a generation and an
  M, and carries its own timestamp and size.
- **Events** — the v2 (`go122`) set is large: goroutine lifecycle (`EvGoCreate`, `EvGoStart`,
  `EvGoStop`, `EvGoBlock`, `EvGoUnblock`, `EvGoDestroy`, `EvGoStatus`), processor scheduling
  (`EvProcStart`, `EvProcStop`, `EvProcSteal`, `EvProcStatus`), GC (`EvGCBegin`/`End`, sweep and
  mark-assist), heap (`EvHeapAlloc`, `EvHeapGoal`, `EvHeapObject*`), stacks and strings
  (`EvStack`, `EvStacks`, `EvString`, `EvStrings`), the user API (`EvUserTaskBegin`, `EvUserRegionBegin`,
  `EvUserLog`), and `EvFrequency` for the timer.

## What the managed runtime already has

This is the part that makes the design more than a wish, and it is why the row is *unimplemented*
rather than impossible. `golib/runtime/Goroutine.cs` already maintains a real registry:

| managed surface | what it gives the tracer |
|:--|:--|
| `s_live` (a concurrent map) + `Snapshot()` | the live goroutine set, ordered by id — the generation's `EvGoStatus` prologue |
| `Id` (internal, deliberately opaque to programs) | a stable goroutine id, which is exactly what the format keys on |
| `Current` (thread-local) + `Enter()` scope | create / start / destroy boundaries |
| `Park(WaitReason)` | block and unblock, with a **reason** already spelled in Go's own vocabulary |
| `WaitReason` (13 values) | `ChanSend`, `ChanReceive`, `Select`, `Semacquire`, `Sleep`, `SyncMutexLock`, `SyncRWMutexLock`, `SyncCondWait`, `IOWait` and their nil-channel variants — these map onto Go's block reasons directly |
| `MonotonicClock.Nanoseconds()` | the monotonic timebase `EvFrequency` describes |

So goroutine identity, lifecycle, park reasons and a monotonic clock all exist. **The tracer does not
need a scheduler to be invented; it needs the events it can already witness to be serialized.**

## What it can emit honestly, and what it cannot

**Honestly emittable** — every one of these is a fact the managed runtime observes today:
`EvGoCreate`, `EvGoStart`, `EvGoStop`, `EvGoDestroy`, `EvGoStatus`, `EvGoBlock` and `EvGoUnblock`
with a real reason, `EvFrequency`, and the `EvString`/`EvStacks` tables that name them.

**Not honestly emittable, and this is the boundary the design refuses to blur:**

- **Per-P scheduling.** There are no Ps. The workable model is **one P per OS thread** — the CLR
  thread the goroutine is running on — which makes `EvProcStart`/`EvProcStop` a *description of the
  managed scheduler*, not of Go's. That is a modelling choice and must be labelled as one wherever
  the trace is read. `EvProcSteal` has no managed counterpart at all and should never be emitted.
- **GC phases.** The CLR's collector is not Go's. `EvGCBegin`/`EvGCEnd` and the sweep and
  mark-assist events describe a collector that is not running; emitting them would be fabrication.
  `GcPauseRecorder` can witness *pauses*, which is a different and smaller claim.
- **Syscall boundaries.** `EvGoSyscallBegin`/`End` mark a goroutine leaving Go's scheduler for the
  kernel. The managed runtime does not distinguish those transitions.
- **Heap events.** `EvHeapAlloc`, `EvHeapObject*` and `EvHeapGoal` describe Go's allocator.
- **Stacks.** `EvStack` wants Go frames. CLR stacks can be captured, but the mapping is a separate
  problem and an empty stack table is the honest first answer.

## Increments, in the ruled order

**C-1 — the state machine and the header.** `StartTrace` stops answering the
tracing-not-supported error and instead arms a tracer: an enabled flag with a compare-and-swap so the
second `Start` fails, a writer, the header, one `EvFrequency`, and an empty batch. `StopTrace`
disarms and flushes, and writes nothing afterwards. **Bar A clears here — and this increment must NOT
land alone**, for the laundering reason above; it lands with C-2 or not at all. Footprint: the two
existing hand-owns (`runtime/{windows,linux}/trace_impl.cs`) gain bodies rather than refusals; one
new golib file for the writer; no converter change.

**C-2 — goroutine events.** The registry's create/start/park/unblock/destroy points emit into the
active batch, with `Snapshot()` supplying the generation prologue so every goroutine referenced later
has a prior `EvGoStatus`. This is where the trace starts describing the program. Footprint: hooks at
the `Goroutine.Enter`/`Park` boundaries, the string and stack tables, and the batch writer; the
registry surface is **shared with Q27's goroutine profile**, and the two should agree on one snapshot
primitive rather than growing two.

**C-3 — `go tool trace` readability.** Bar B. Generation framing, batch sizing, and the ordering
rules the parser enforces; the P model labelled as one-per-thread. The acceptance test is external
and unforgiving: the tool opens the file, or it does not.

## What this design does NOT buy

- **It does not make `runtime/trace` a validated row on C-1 alone.** Bar A is too weak to be a
  finish line, and the design says so on purpose.
- **It does not give Go's scheduler.** Anything a reader infers about Ps, stealing, syscalls or GC
  from a managed trace is an artifact of the model, not a measurement of the program.
- **It does not give stacks** in its first three increments.
- **It does not settle `runtime/pprof`.** That row's CPU-profile class is a neighbour, not the same
  question, and it is classified on its own evidence.

## The falsifier that would change the row's disposition

If, in building C-1 through C-3, the v2 format proves **impossible to emit self-consistently from a
managed scheduler** — concretely, if the parser cannot accept a stream whose P model is one-per-thread
and whose GC, syscall and heap event classes are absent — then the row is not merely expensive, and
that finding is the evidence that would justify putting an owner-ruled fourth exclusion class to the
owner. **Until that is measured, it is not claimed:** today's honest statement is that the row is
unimplemented, and this document is the size of implementing it.

---

## AMENDED 2026-09-27 (G) — re-read at the go1.24.13 pin, and the shape of C-1/C-2/C-3 as cut

COORD's order of 2026-09-27: re-read against Go 1.24.13, amend, post, then cut C-1 and C-2 together as
one seat, then C-3 (Bar B), red first at each step. Everything below was read from the pin's GOROOT and
the format facts were EXERCISED, not only read: a hand-built minimal trace parses under the pin's
`go tool trace -d=parsed`, and two negative controls (no frequency batch; an event before any P) are
refused with the parser's own error and exit 1.

**The format at the pin.**
- Header: the 1.24.13 runtime writes `go 1.23 trace\x00\x00\x00` (runtime/trace.go:823); the parser
  accepts 22 and 23 through the same go122 spec (internal/trace/version/version.go:20-40). The header
  this design named stands.
- Batch: `EvEventBatch` (1), uvarint gen, uvarint M, uvarint base timestamp, uvarint payload length,
  payload of at most 64 KiB (go122/event.go:508). Events are `[type, uvarint dt, args…]`, dt measured
  from the previous event in the same batch.
- Generation: detected by the batch's gen field alone. EXACTLY ONE frequency batch per generation
  (`[8, uvarint ticksPerSecond]`, alone in its batch); strings and stacks batches are optional (ID 0
  is the empty string / no stack); there is no end marker, EOF ends the trace; a later generation must
  re-state every G and P. So the tracer writes ONE generation for the whole Start..Stop window.
- Events used (numbers at go122/event.go): ProcStatus 13, ProcStop 11, GoStatus 25, GoCreate 14,
  GoStart 16, GoBlock 20, GoUnblock 21, GoDestroy 17; String 5 inside a strings batch (4); Frequency 8.
  Status enums: P Running 1 / Idle 2; G Runnable 1 / Running 2 / Waiting 4.
- Ordering (internal/trace/order.go): GoCreate, GoBlock and GoDestroy need an M holding a P (ProcStatus
  first, else "expected a proc but didn't have one"); GoStart needs the G Runnable, `g_seq` = previous
  + 1 and an M with a P and no G; GoUnblock needs the G Waiting and seq + 1, and no context; a G's seq
  counts only GoStart and GoUnblock.
- runtime/trace's tests at the pin (trace_test.go:18, :38): TestTraceStartStop (non-empty output; no
  bytes after Stop over 100 ms) and TestTraceDoubleStart (the second Start errors; a double Stop is
  harmless). Neither parses the output, so Bar A is unchanged and still not a stopping point.
- The Bar B oracle is `go tool trace -d=parsed FILE` (cmd/trace/main.go:232-238): the full
  NewReader/ReadEvent path, exit 0 at EOF, exit 1 with the error on stderr. NOT the default HTTP mode,
  which logs "able to proceed" and carries on.

**What the pin says about the managed surface.**
- The registry (golib/runtime/Goroutine.cs) has all five points: create is `StartWithCreator`
  (registered on the CREATING thread), start is `Adopt` on the new thread, destroy is `Scope.Dispose`,
  block is `Park` (outermost scope only) and unblock is `Ready` on the waker's side. `ParkTransition`
  and `ReadyTransition` are single-slot hooks the runtime already occupies (runtime/stubs_impl.cs:287),
  so the tracer does NOT take them: it gets direct calls at the five points behind one volatile
  `enabled` read, which is its whole cost while tracing is off.
- There is no M and no P: each goroutine owns its OS thread for life.
- `runtime.ReadTrace` is still the converted body (systemstack + gopark on the tracer's own reader),
  unreachable today, and runtime/trace.Start's reader goroutine loops on it. It joins StartTrace and
  StopTrace as a hand-own (manualConversionFuncs, goosWindowsLinux) -- a converter registry row, with
  its two-seeded footprint, in the C-1+C-2 seat. This design did not name it; the pin's sources do.

**The model, as cut.**
- M = the goroutine's OS thread; ONE P per M (P id = M id), declared by ProcStatus(Running) before that
  M's first event. A goroutine's events are written on its own M; GoUnblock is written on the WAKER's M,
  which is the one event that needs no context.
- Statuses are LAZY per goroutine, as Go's own runtime states them (statusWasTraced): a goroutine's
  first event on its own thread is preceded by GoStatus(Running); an Unblock target not yet stated is
  preceded by GoStatus(Waiting). The Snapshot() PROLOGUE states every goroutine that is PARKED when
  tracing starts as Waiting, so a goroutine that never wakes inside the window is still in the trace,
  which is what Go's generation start gives. Running goroutines are stated lazily on their own thread,
  the only place that can put them on an M.
- ONE snapshot primitive: `Goroutine.Snapshot()`. `ProfileSnapshot()` (Q27, landed) is already its
  user-goroutine projection; the tracer reads `Snapshot()` directly, because system goroutines park and
  are unblocked too. Proposed to P2 through its inbox.
- Every tracer event takes one global lock while tracing is on, and its timestamp is taken under that
  lock, so events are globally ordered and each M's batch is monotonic. Per-M buffers flush as batches
  of at most 64 KiB. The cost exists only while a trace runs.
- GoBlock's reason string is Go's TRACE vocabulary, not the traceback one (`WaitReasons.Text` spells
  "sync.Mutex.Lock"; the trace spells "sync"). The mapping is read from the pin's gopark call sites and
  runtime/traceruntime.go's traceBlockReasonStrings: chan receive/send -> "chan receive"/"chan send";
  the nil-channel waits and `select {}` -> "forever"; select -> "select"; semacquire, Mutex, RWMutex
  and WaitGroup -> "sync"; Cond.Wait -> "sync.(*Cond).Wait"; sleep -> "sleep"; IO wait -> "network";
  the synctest waits -> "synctest"; anything else -> "unspecified". Strings go in a strings batch at
  flush.
- Frequency is `Stopwatch.Frequency`, and timestamps are Stopwatch ticks.
- Stacks: none (stack ID 0 everywhere), unchanged.
- NEVER emitted: ProcSteal, GC, heap, syscall, CPU samples, and the user API's tasks, regions and logs.
- Coroutine bodies (iter.Pull) are goroutines -- Coro.cs runs each through Goroutine.Run, which mints
  the identity -- and hand off with Park(Coroutine)/Ready. Go traces a coroutine switch with its switch
  events; this model states it as a block ("unspecified") and an unblock, which is a model difference
  and is stated, not hidden. A pooled thread can carry successive bodies, each a GoStart/GoDestroy pair
  on that M's one P.

**Increments, amended.**
- C-1 + C-2, ONE seat (as ruled): the state machine (a CAS so the second Start fails), the header, the
  frequency and strings batches, lazy statuses, the Snapshot prologue, create/start/block/unblock/
  destroy, and the ReadTrace hand-own. Red first: runtime/trace's two tests fail today on "tracing is
  not supported"; they pass here, and a GolibTests arm pins the state machine and the stream's framing.
- C-3 (Bar B): a GolibTests arm writes a managed program's trace (create, a channel block and unblock,
  exit) and runs the toolchain's own parser on it (`go tool trace -d=parsed`), which must exit 0 and
  show the expected transitions. Red first on a deliberately broken stream (the frequency batch
  withheld), so the arm is shown able to fail. Inconclusive only where no Go toolchain resolves; every
  fleet box has one.

**Consequences.** runtime/trace leaves E4 by arithmetic the moment its two tests match on an honest
tracer, and net/http/pprof's trace subtest moves with it (COORD: that row banks only behind a real
tracer). Darwin keeps its converted StartTrace (it has no hand-own); out of scope and named.

-- G, 2026-09-27

> **Amendment, 2026-09-28 (lane `claude/c1-q5-trace-start-pair`, coordinator-recorded).** "Darwin
> keeps its converted StartTrace" is superseded. C1's stop-the-world seat displaced darwin's
> StartTrace/StopTrace, and under layout L3 one `trace_impl.cs` body cannot both refuse and run the
> tracer. So the merge of that seat into this design made the managed tracer the content of all
> three `<goos>/trace_impl.cs` copies, byte-identical, and darwin now drives `ExecutionTracer` as
> windows and linux do. That forced the `ReadTrace` registration from `goosWindowsLinux` to `goosAny`.
> Its measured footprint is darwin only: `trace.cs` and the re-encoded `package_info.cs` map. After
> it, darwin's `trace.cs` is blob-identical to linux's. The same lane brackets
> `ExecutionTracer.Start()` with `stopTheWorld(stwStartTrace)`/`startTheWorld`, answering a refused
> start before the stop, as Go does. `StopTrace` takes no pair, since Go's `traceAdvance` stops no
> world. The design above is otherwise unchanged; darwin's tracer runs untested until the darwin run
> layer exists.
