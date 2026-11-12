# DATA: the M:N spike, phase 2 (R5b)

> **STATUS: RECORD IN PROGRESS (lane G, opened 2026-11-11; branch `claude/g-mn-spike2-b6`, off master `742a3dbb24`).**
> Phase 2 of the spike that [DATA-mn-spike](DATA-mn-spike.md) reports. It answers the owner's three questions about
> ruling R2 of [DESIGN-mn-scheduler](DESIGN-mn-scheduler.md) (hidden async twins, option O6, with O7 placement):
> is it viable, what does it cost, and **what does a C# consumer see change**, against the held fallback R3
> (visible async, option O4). Spike code lives under `src/tools/mnspike2` on `claude/g-mn-spike2-*` branches and
> never lands. Sections are added as phases report; each is a dated block and none is rewritten.
> Host for local readings: lane G's box (Windows x64); go1.26.10; .NET SDK 10.0.400, runtime 10.0.11.

## 1. B6: what a C# consumer sees change (2026-11-11, first reading)

This section comes first because the owner asked for it first. It is written before B1 to B5 run, so each row
says how it is known:

- **MEASURED**: a reading in this record or in phase 1's;
- **READ**: read from the code or the published doc at master `742a3dbb24`, with the place named;
- **DESIGN**: follows from the ruled design and is not built yet; the phase that will prove it is named;
- **OPEN**: not decided; the phase that decides it is named.

"Consumer" means C# code that references converted packages and golib: it calls exported Go functions,
implements Go interfaces by hand, starts goroutines with `goǃ`, and uses `channel<T>`. The tiers are those of
`docs/PLAN-public-surface-stability.md` (ruled 2026-11-11): tier 1 frozen, tier 2 hidden plumbing, tier 3 no promise.

### 1.1 The short answer

Under **R2** no tier 1 surface changes: no Go-declared signature, no type mapping, no channel member, no
exception type. The consumer-visible changes are (a) hidden, default-bodied additions, and (b) threading
behaviour, of which thread identity is the one a caller can trip over (row T1).

Under **R3** tier 1 changes by construction: at least **1,370 exported functions in 116 non-internal standard
library packages** take a task-shaped signature, every caller of them awaits, and every hand-written
implementer of an affected Go interface breaks. The threading changes of R2 apply as well, and one of them is
stronger (T1).

### 1.2 Signatures and types

| # | Surface | Today | Under R2 (hidden twins) | Under R3 (visible async) | How known |
|---|---|---|---|---|---|
| S1 | Exported Go functions and methods | synchronous, Go's shape | **unchanged** | at least 1,370 exported functions in 116 non-internal std packages become task-shaped and must be awaited: `fmt.Println`, `fmt.Sprintf`, `io.Copy`, `io.ReadAll`, `os.ReadFile`, `json.Marshal`, `sort.Sort`, `(*os.File).Write`, `(*sync.WaitGroup).Wait` among them | MEASURED (phase 1's can-park census, windows, upper bound, long parks only: 5,223 names, of which 1,061 internal or vendor, 1,370 exported and public; linux: 5,266, 1,087, 1,391 in 117 packages; counted by `src/tools/mnspike2/b6/exported-canpark.py`, whose control names five functions that must be in the set and four that must not). A visible-async converter must treat the upper bound as the set, because the decision is static |
| S2 | Go interfaces a consumer implements (`class CollectingWriter : io.Writer`) | abstract members only | each can-park method gains ONE hidden member with a default body that runs the synchronous method. An existing implementer compiles and runs unchanged, as a class and as a struct, and a package built before twins keeps working | the Go-named member itself changes shape (`Write` returns a task), so **every hand implementer breaks**; this cannot meet OQ5 | MEASURED for R2 (§1.6, the K2 probe, JIT and Native AOT). READ for R3: `(*os.File).Write` is in the can-park set, so `io.Writer.Write` must be task-shaped |
| S3 | The frozen type mapping (named tuples, `ж<T>` / `ref T`, `nint`, `@string`) | as documented | **unchanged**. Twins box internally where C# forbids a `ref` across an await | `ref` and span parameters cannot sit on an async method, so the visible mapping changes for those functions | MEASURED (phase 1: in the can-park set, 1,323 `ref` parameters, 204 span parameters, 1,992 `ref` locals) |
| S4 | New public members | none | one hidden twin per can-park function, on the package class or type, named with a marker no Go name can carry, `[EditorBrowsable(Never)]`, tier 2. They must be public, because one package's twin calls another's. Reflection lists them | none added; existing ones change | DESIGN (B2 builds the generator). Known .NET behaviour, not measured here: the attribute hides a member for assembly references and not for a project reference in the same solution |
| S5 | Channels: `Send`, `Receive`, `TrySend`, `TryReceive`, `Close`, `foreach`, `ReceiveAsync`, `SendAsync` | golib, hand-written | **unchanged**, members and meaning. From a C# thread `Send` and `Receive` still block that thread; the awaitable pair still takes no thread | unchanged members | READ (`src/core/golib/channel.cs`; the consumer doc, "Goroutines and channels") |
| S6 | `PanicException`, `GoErrorException`, `GoexitException` | frozen | **unchanged** | unchanged | READ |
| S7 | Older packages and versions | n/a | a package built before twins runs against a newer golib: its interface calls reach the default body and its goroutines take the 1:1 fallback. No flag day | a flag day: every package is re-published with new signatures; a break-floor move | MEASURED for the interface half (§1.6, the "old converted" arm). DESIGN for the fallback (B3) |

### 1.3 Threading a C# caller can observe

| # | Behaviour | Today | Under R2 | Under R3 | How known |
|---|---|---|---|---|---|
| **T1** | **Thread identity across a park** | a goroutine is one OS thread from start to end; the consumer doc says so | see §1.4. In one sentence: a C# method body never changes thread, and two calls into C# code from the same converted goroutine may run on different threads | as R2, **and** the consumer's own method body resumes on another thread after each `await` of a Go call unless a synchronization context brings it back | DESIGN, tier 3 by ruling. B3 measures it |
| T2 | A synchronous call from a plain C# thread (Main, the thread pool, a UI thread) | runs on the caller's thread and blocks it at a park | **unchanged**: the caller gets the visible synchronous body | must be awaited. Blocking on the task from a UI thread is the classic deadlock; constructors, property getters and `lock` bodies cannot await | DESIGN (O6: a synchronous caller never enters a twin) |
| T3 | The published sentence "Each goroutine runs on its own thread for as long as it runs" | true | false for a goroutine whose entry function is converted code; still true for `goǃ(() => ...)` with a C# lambda, which has no twin and stays 1:1 | false | READ (`docs/ConsumingGoFromCSharp.md`, "Goroutines and channels"; `builtin.GoroutineLaunchers.cs`: `goǃ` takes plain delegates). The doc edit is owed with the landing, not the spike |
| T4 | C# code that blocks while a converted goroutine is calling it (a callback, a hand `io.Writer`, a P/Invoke of the consumer's own) | blocks that goroutine's thread only | blocks a carrier. The scheduler takes the carrier's slot back after a threshold and runs the other goroutines on another thread: latency for others up to the threshold, one more thread, **never a deadlock** | same | DESIGN (O6's fallback, "a missing twin costs a thread, never correctness"). B3 measures the latency |
| T5 | `runtime.GOMAXPROCS` | a remembered number; every runnable goroutine runs in parallel on its own thread | real: at most that many goroutines run converted code at once, as in Go | same | READ (`src/core/runtime/managed_impl.cs`, `GOMAXPROCS`). DESIGN for the new meaning |
| T6 | Thread count and the per-process thread cap | one thread per live goroutine; a goroutine-heavy program dies at 2,028 (osx-arm64) or 3,052 (osx-x64) | parked goroutines hold no thread: 100,000 parked on 3 carriers in 0.11 s on osx-arm64 | same | MEASURED (phase 1, with hand twins). B3 repeats it with generated twins |
| T7 | Per-thread ambient state seen from converted code: thread name, priority, culture set on the thread, `AsyncLocal<T>` values | the goroutine's own thread | the carrier's, unless golib flows an execution context per goroutine | .NET's own async rules | **OPEN**: B1 decides whether a goroutine carries an execution context, with a cost reading |
| T8 | `runtime.LockOSThread` | bookkeeping only; trivially true | real: the goroutine moves to a dedicated thread and stays there until `UnlockOSThread` or its end | cannot help a C# `async` caller, whose continuations .NET schedules | READ (`managed_impl.cs`, `LockOSThread`: it counts and calls the converted `dolockOSThread`). DESIGN for the pin (O7); B1 builds it, B3 measures it |

### 1.4 T1 in full: thread identity, and what a caller does about it

Under R2 only generated twins contain an `await`, and only converted Go code has twins. That gives three cases.

1. **Inside one C# method body the thread never changes.** C# source is never rewritten. If code below a C# frame
   reaches a park (a callback that calls `ch.Receive()`, say), that park blocks the thread, exactly as today
   (row T4). So thread-statics, a `lock`, `Marshal.GetLastPInvokeError()` read after a P/Invoke, and a
   thread-affine handle used within one method all behave as they do now.
2. **Between two calls into C# code from the same converted goroutine, the thread may differ.** A handler that a
   converted server loop calls once per request, a hand-written `io.Writer` whose `Write` is called twice, a
   comparison function passed to converted code: each call may arrive on a different carrier. State kept in a
   `[ThreadStatic]` field or a `ThreadLocal<T>` between calls, a window handle, a COM apartment object, or any
   native handle bound to its creating thread is no longer safe to keep across calls.
3. **A goroutine started from C# with a C# lambda keeps its thread**, because its entry has no twin (row T3).
   This is behaviour, not a promise: tier 3.

What a caller can do, in order of preference:

- keep thread-bound state inside one call (case 1 guarantees it);
- call `runtime.LockOSThread()` at the top of the goroutine that needs one thread for its life. This is Go's own
  answer, and the surface plan already names it as the documented opt-out. It costs that goroutine a dedicated thread;
- keep state that must follow a goroutine on an object the goroutine passes along, not on the thread.

Under R3 case 1 is lost: the consumer's own `async` method resumes wherever the awaited Go call completes.
`LockOSThread` cannot restore it, because .NET, not golib, schedules a C# continuation.

### 1.5 Errors, tools, size and speed

| # | Surface | Under R2 | Under R3 | How known |
|---|---|---|---|---|
| E1 | An unrecovered panic in a goroutine ends the process; a `try`/`catch` around `goǃ` cannot see it | **unchanged** | unchanged | READ (the consumer doc). B3 proves it under twins |
| E2 | `defer`, `recover`, `Goexit` semantics | unchanged by design: the panic and defer state now held per thread (`GoThreadState`) is installed on a carrier when a goroutine mounts it and taken off when it parks | a third frame generation (design §6) | DESIGN. B1 builds the swap, B3 runs the defer and recover shapes |
| E3 | Stack traces a consumer logs from a panic | frames of a twinned function carry the twin's hidden name and async state-machine frames, and cover the stack since the last resume | every can-park frame is an async frame | DESIGN. B3 captures a real trace for the record |
| E4 | Debugging converted code | a breakpoint in the visible `.cs` binds inside the twin through `#line`; stepping over a park can change the thread shown; thread views show carriers, not goroutines | ordinary async debugging | MEASURED by PDB proxy (phase 1, G5: 4 of 4 lines bind); the manual check is still owed |
| E5 | Package and publish size, build time | every package with can-park functions carries a second generated body for them (at most a third of functions) | no second body | B4 measures it |
| E6 | Call cost | a call that never reaches a twin is untouched. On twin paths: 952 ns per park and resume round trip, +609 ns per func-literal twin registration, and a struct called through a constrained generic boxes to reach a default body | every can-park function pays the task machinery on every call, parked or not, `fmt.Sprintf` included | MEASURED (phase 1) for the three figures; B4 extends it. The boxing is known .NET behaviour, not measured here |
| E7 | Start-up in a host application | carriers start at the first twinned goroutine | same | B4 measures it |

### 1.6 The K2 probe: a hidden twin on a public interface

Stop line K2 asked whether an interface twin can be added without breaking a C# implementer. Owner ruling OQ5
(2026-11-11) now requires the answer to be a default-implemented member or a separate hidden interface.

`src/tools/mnspike2/dimprobe` (commit `840ef902a5`) takes `io.Writer`'s shape. The generated half of the partial
interface adds `ValueTask<(nint n, object? err)> Writeꓽasync(byte[] p) => new(Write(p));`. Four implementers: a
hand class, a hand struct, a "converted" struct with its own async twin, and a struct standing for a package built
before twins. Each is called through the interface, through a generic constrained to it, and through a delegate.

| Reading | JIT | Native AOT (win-x64) |
|---|---|---|
| the 12 calls reach the expected body, and the member is absent from the implementing class | 13 of 13 | 13 of 13 |
| bytes allocated by the default body over 100,000 calls | 24 | 0 |
| red control (expect the default body where the type's own twin must run) | exactly 3 fail, exit 1 | exactly 3 fail, exit 1 |

**K2 does not fire, and the first form OQ5 allows is sound**, so the hidden-interface form was not measured.
Not covered: a Visual Basic or F# implementer, an explicit interface implementation, and a consumer compiled
against the old interface assembly then run against the new one (adding a default member is binary-compatible
by .NET's design; not measured here).

### 1.7 What this section still owes

T1 case 2 and T4 observed in a running program (B3); the `LockOSThread` pin (B1, B3); T7's ruling (B1); E2 under
generated twins (B3); a real E3 trace (B3); E5 to E7 (B4); the count of exported INTERFACES R3 would change;
and the same lists for a real module's closure (B5, the TypeScript compiler's Go port).

### 1.8 The native-call seam, read as it stands

[DESIGN-native-call-seam](DESIGN-native-call-seam.md) and `src/core/golib/runtime/GoNativeCall.cs` were read at master
`742a3dbb24` and are **not changed by the spike**.

- The seam's state already lives on the goroutine (`m_nativeDepth`, odd while inside a native call), not on the
  thread. B1 therefore needs nothing from the seam to start: the scheduler's monitor reads a carrier's current
  goroutine, sees an odd depth, and takes the slot back (the design's M4, the `MayBlock` path).
- An eager hand-off for the `Blocks` hint would need a scheduler hook inside `Enter` and `Exit`. That is a
  request to the seam's owner after the spike, not a spike change.
- The seam's stated gap M7 (a callback arriving on a foreign thread runs with no goroutine identity) matters
  more under M:N, where identity decides which state is mounted. It stays a named gap in the spike.

## 2. B0: the shape set and its baselines on today's runtime (2026-11-11)

Written before any spike code touches the runtime. It fixes what B3 re-runs under generated twins, and what
today's runtime (one OS thread per goroutine) reads on the same programs. Branch `claude/g-mn-spike2-b0`, on
`6afb937753`. It adds one Behavioral test and one sampler, and changes no converter, generator or golib file.

### 2.1 The shape set

Eighteen existing Behavioral programs, picked because each parks or unwinds a goroutine in a way P4 names:

| shape | programs |
|---|---|
| channel rendezvous and close | ChannelRendezvous, CloseWakesBlocked, ChannelReceiveFromClosed |
| select (fairness, default, send and receive mixed, nil channel, recursion) | SelectStatement, SelectRandomFairness, SelectSendDefault, SelectSendRecvMix, NilChannelInSelect, DeepSelectRecursion |
| timers and sleep | SyncTimerChannel |
| Mutex, WaitGroup, many parked goroutines | GoroutineWaitState, GoroutineParkStorm |
| panic, recover and Goexit in a goroutine | GoexitDefers, GoroutinePanicExitCode, GoroutinePanicAfterDeferredDone, PanicRecover, GoroutineRecoveringWhenMainReturns, CrashGoroutineCreatedBy |

One new Behavioral program, `MnMixedStorm`, is the nineteenth. n goroutines are alive at once, and each parks
four ways before it ends: a select over two channels, `time.Sleep`, a contended `sync.Mutex`, and an unbuffered
send that main receives. Half start from a named method (`go s.worker(i)`) and half from a func literal that
calls it, because the two take different routes to a hidden twin. Its output is deterministic by construction
(ids and tokens are summed, never printed in arrival order). n is 100,000 unless `MN_STORM_N` names another
count; `MN_STORM_HOLD_MS` holds the goroutines parked so a sampler can read the process. It is a real
Behavioral test (registered, golden minted), so a hosted leg reaches it with
`stage=behavioral-smoke filter=MnMixedStorm`.

At its default size the test is **red on today's runtime by design**, on every host. That is the point of it,
and it is why this branch never lands.

### 2.2 Today's runtime on the shape set (Windows x64, local)

Each of the eighteen ran alone through `run-behavioral.ps1 --filter <name>` (all four phases, Release):
**18 of 18 PASS**. Seventeen were compared with Go on exit code and standard output. `SelectStatement` carries
no `[GoTestMatchingConsoleOutput]`, so its Output phase reports `skip 1`: for that program the baseline is
"transpiles, matches its golden, compiles", and B3 can claim no more for it than that.

### 2.3 MnMixedStorm on today's runtime (Windows x64, local)

Read with `src/tools/mnspike2/Measure-Shape.ps1`: it starts one program, samples OS threads and private bytes
every 250 ms, and hashes the standard output with carriage returns removed. `MN_STORM_HOLD_MS=3000` in every
row, so the wall time includes a 3 s hold. "Same output" means the converted build's output hash equals the Go
build's for that n.

| n | build | verdict | wall | peak OS threads | peak private | output |
|---|---|---|---|---|---|---|
| 1,000 | Go | EXITED 0 | 3.3 s | 9 | 22 MB | 4 lines |
| 1,000 | converted | EXITED 0 | 3.8 s | 1,011 | 90 MB | same as Go |
| 10,000 | Go | EXITED 0 | 3.3 s | 14 | 101 MB | 4 lines |
| 10,000 | converted | EXITED 0 | 6.3 s | 10,012 | 634 MB | same as Go |
| 100,000 | Go | EXITED 0 | 3.8 s | 15 | 890 MB | 4 lines |
| 100,000 | converted | EXITED 0 | 115.1 s | 100,008 | 5,962 MB | same as Go |

- The sampler is proven both ways by the table itself: the Go build reads 9 to 15 threads where the converted
  build reads n plus 8 to 12.
- The conversion is right at every n: where the process survives, today's runtime prints Go's output. What is
  red is the cost. Each parked goroutine holds a thread, about 60 KB of private memory each, and the
  100,000 run takes 111 s longer than Go's.
- Through the Behavioral runner at the default n the test is **red on Windows too**: Transpile, Compile and
  Target pass, and Output reports `timeout 1` ("timed out after 30s (C# side)"), because the run needs 115 s
  against the runner's 30 s budget. With `MN_STORM_N=1000` the same runner passes all four phases.

### 2.4 What B0 could not read

`GoroutineParkStorm` (1,000 parked goroutines, no hold) ends in 0.9 s, and a 250 ms sampler caught 400 and 328
threads in two runs, not the roughly 1,010 that must exist at its gate. A peak thread count from this sampler is
a lower bound unless the program holds. `MnMixedStorm` has the hold for that reason, and B1 adds a reading
printed by the runtime itself (carriers and OS threads), which does not depend on a sampler's timing.

The hosted readings (osx-arm64, osx-x64, linux-x64, linux-arm64) follow in a dated block below once the legs
have run on this branch.

### 2.5 Hosted readings on today's runtime (2026-11-11, branch `claude/g-mn-spike2-b0` at `b6165f507b`)

Dispatched as `os-matrix.yml`, `stage=behavioral-smoke`, `filter=MnMixedStorm`, default n (100,000). Transpile,
Compile and Target pass on every leg that ran; the Output phase is where each one is red.

| leg | run | Output phase | runner wall |
|---|---|---|---|
| linux-x64 | 43062197012 | FAIL: `exit code mismatch: C# 2 vs Go 0`, C# stderr `System.OutOfMemoryException` | 213.7 s |
| osx-arm64 | 43062191279 | FAIL: the same line | 337.3 s |
| osx-x64 | 43062191279 | FAIL: the same line | 1,103.2 s |
| linux-arm64 | not read | the workflow refuses the behavioral stages on the arm leg until seat S3 lands | |

- Every hosted leg dies the same way: thread creation fails, the process reports `OutOfMemoryException` and exits 2
  where Go exits 0. Windows (2.3) is the only host where the 100,000 run survives, and it needs 115 s.
- The failure line carries no thread count, so these legs confirm the mode and not the cap. The counts on file
  (2,028 on osx-arm64, 3,052 on osx-x64) come from the phase-1 record (`DATA-mn-spike.md`), not from this run.
- The first dispatch failed at `plan` on both workflows and read nothing: the darwin one was sent with an `arch`
  input it does not take, and the linux one asked for the arm leg. The runs above are the re-dispatch.
- B3 re-runs the three legs that ran on a ref carrying generated twins. Green there is P4.

## 3. B1: the scheduler in golib behind a switch, on a hand-twinned program (2026-11-11)

B1 asks whether golib can carry O6 and O7 at all, before any generator exists: a scheduler, the park paths
wired to it, and one program whose twins are written by hand the way B2's generator would write them. Branch
`claude/g-mn-spike2-b1`, on `b6165f507b`. Windows x64, 12 logical processors, .NET SDK 10.0.400 (runtime
10.0.11), go1.26.10. Spike code: it stays on the branch.

### 3.1 What was built

- `golib/runtime/GoScheduler.cs` (564 lines): a global run queue of goroutine steps, P tokens (one per
  processor), carrier threads (1 MB stacks) that take a P to run one step, a per-carrier run-next slot, hand-off
  of the P when a mounted goroutine blocks its thread, and a monitor thread that injects a P beside a step that
  runs long. `GoParkSlot` is where a twin waits: its continuation is published by the carrier after the
  goroutine is unmounted, so a waker cannot make a goroutine runnable while it is still mounted.
- `golib/runtime/Goroutine.Multiplexed.cs` (228 lines): `StartTwin`, the twin's root (the thread root's
  try/catch ladder with awaits), and `Mount`/`Unmount`, which exchange the goroutine-logical state golib keeps
  in thread-statics (`GoThreadState`, pending select frames, the last adopted panic, the Goexit flag, the
  `GoFrame` activation and thread token) between the goroutine and the carrier.
- Twin entry points beside the blocking ones: `selectꞏtwin`, `channel<T>.Sendꞏtwin`, `Receiveꞏtwin` (both
  forms), `time.Sleepꞏtwin`, `sync.Mutex.Lockꞏtwin`. Ten existing golib and stdlib files change by 355 added
  and 10 removed lines; 237 of the additions are the channel twins. One test file gains one row: the carrier's
  thread-static in the thread-state census (3.4).
- `GoFrame` loses nothing a converted body can see: `defer(…, ref ᒐ)`, the converted `catch … when
  (GoFrame.IsPanic…)` and `finally { ᒐ.Run(); }` compile unchanged inside an `async ValueTask` method.
- `runtime.getg` cached its descriptor per thread; under carriers that hands one goroutine another's
  descriptor. It now re-mints when the cached owner is not the mounted goroutine.
- `src/tools/mnspike2/MnStormTwin`: the converted `MnMixedStorm` with `workerꞏtwin` written by hand (the same
  statements in the same order, each parking call replaced by the callee's twin, awaited; the receiver stays the
  box and the `ref` local is derived again after each await), a `goǃtwin` that takes the body and the twin, and
  two micro programs (`MN_MODE=ping`, `MN_MODE=leaf`).

With `GO2CS_MN` unset, `StartTwin` is `Goroutine.Start` and nothing in the scheduler runs.

### 3.2 Switches and the runtime's own reading

| switch | meaning |
|---|---|
| `GO2CS_MN=1` | goroutines started with a twin are multiplexed |
| `GO2CS_MN_READING=1` | print the scheduler's counters at exit (one `READING go-scheduler` line on stderr) |
| `GO2CS_MN_PROCS` | P count (default: the processor count) |
| `GO2CS_MN_PREEMPT_MS` | how long a step runs before a P is injected beside it (default 10; 0 = off, an arm) |
| `GO2CS_MN_MAXINJECT` | injected Ps live at once (default: the P count) |
| `GO2CS_MN_RUNNEXT=0` | run-next slot off (an arm) |
| `GO2CS_MN_NOGETGFIX=1` | the `getg` fix off (the red proof) |
| `GO2CS_MN_MOUNTBENCH=1` | time `Mount` + `Unmount` at exit |

The reading line carries peak carriers, carriers started, peak OS threads, steps, parks, hand-offs, injections,
run-next steps and steals, steps over 10 ms and the longest step. It answers 2.4: a 250 ms sampler read 24
threads on an arm whose scheduler counted 226.

### 3.3 MnMixedStorm under hand-written twins

Every reading in section 3 is from the tree as committed unless it says otherwise. Same sampler and hold
(3,000 ms) as 2.3; the last three columns are the scheduler's own. Every row exits 0 and
its output hash equals the Go build's for that n.

| n | build | wall | peak OS threads | peak private | carriers (peak) | steps | steps over 10 ms |
|---|---|---|---|---|---|---|---|
| 1,000 | Go | 3.3 s | 9 | 22 MB | | | |
| 1,000 | thread path (switch off) | 3.8 s | 1,011 | 92 MB | | | |
| 1,000 | twins | 3.8 s | 24 | 29 MB | 12 | 3,711 | 12 |
| 10,000 | Go | 3.3 s | 14 | 101 MB | | | |
| 10,000 | thread path (2.3) | 6.3 s | 10,012 | 634 MB | | | |
| 10,000 | twins | 4.0 s | 28 | 75 MB | 18 | 39,981 | 92 |
| 100,000 | Go | 3.8 s | 15 | 890 MB | | | |
| 100,000 | thread path (2.3) | 115.1 s | 100,008 | 5,962 MB | | | |
| 100,000 | twins | 6.9 s | 43 | 614 MB | 24 | 450,411 | 794 |

- At 100,000 goroutines the twin build holds 43 OS threads where the thread path holds 100,008, finishes in
  6.9 s against 115.1 s, and uses less private memory than the Go build (614 MB against 890 MB). It is 3.1 s
  behind Go on wall time.
- No arm in B1 lost a wakeup or printed a wrong line.
- About 0.2% to 0.3% of steps run past 10 ms (the longest 110 ms). A twin cannot be preempted, so this is the latency
  cost of a run-to-park step under load. Injection does not move it: an earlier build with injection off read
  756 such steps, the longest 106 ms.

### 3.4 Red controls

- **Switch off** is the thread path: 1,011 threads at n = 1,000 in the same build (row two above).
- **GolibTests with the switch off** (the whole suite, Release, on the B1 tree; the base was not re-run): the
  first run read 2 failed, 2,528 passed, 49 skipped of 2,579. Both failures are source censuses naming the
  spike's own new lines, not behaviour: `NoUncountedBackingAllocations` (the select twin allocated its waiter
  array past the allocation counter) and `EveryThreadStaticInTheTree_IsClassified` (the carrier's
  thread-static). With the array counted as the blocking select counts its own, and the thread-static listed
  as a kept thread resource, the second run reads 0 failed, 2,530 passed, 49 skipped.
- **`getg` fix off** (`GO2CS_MN_NOGETGFIX=1`, n = 1,000): exit 2, `panic: runtime: park transition leaving
  select on goroutine 13 whose status is 2, not _Gwaiting (4) or _Grunnable (1)`. The trace names
  `main.MoveNext()` and `created by main.goǃtwin`: that is how a panic trace reads under hand twins today
  (finding 8).
- **The first build** read 489 OS threads at 100,000: the monitor started a thread for every long step. The
  injection budget in 3.2 (and a carrier count bounded at twice the P count) is the fix; 43 threads on the tree
  as committed.

### 3.5 A missing twin

The design's rule is that a missing twin costs a thread, never correctness. Two arms, both on the twin build:

| arm | n | wall | peak OS threads (scheduler's count) | carriers (peak) | hand-offs | output |
|---|---|---|---|---|---|---|
| the whole goroutine has no twin (`MN_STORM_NOTWIN=1`) | 1,000 | 3.8 s | 1,012 | 1,000 | 3,985 | same as Go |
| one callee has no twin (`MN_STORM_SYNCSLEEP=1`: the 1 ms sleep) | 1,000 | 3.8 s | 226 | 206 | 1,000 | same as Go |
| the same | 10,000 | 4.3 s | 540 | 517 | 10,000 | same as Go |
| the same | 100,000 | 7.8 s | 792 | 772 | 100,000 | same as Go |

- Correctness holds in every arm, and in the sync-sleep arms hand-offs equal the untwinned sleeps exactly.
- The thread cost is **per concurrently blocked untwinned call**, not per goroutine and not per call site. A
  goroutine with no twin at all is one thread for as long as it lives (row one equals the thread path). One
  untwinned 1 ms sleep in 100,000 goroutines costs about 800 threads at its peak, because that many sleeps
  overlap. A long-blocking untwinned call on a hot path would put one thread back under every goroutine parked
  in it, which on macOS is the cap again. Twin coverage of the calls that park for long is what the cap needs,
  and B5's census has to count those.
- This table is on the second hand-off rule. The first one added a carrier at every hand-off that found none
  idle, and read 1,869 threads for 1,000 untwinned goroutines and 10,015 for the sync-sleep arm at 10,000.
  Go's rule (add one only when work is queued and fewer than P carriers are unblocked) reads the rows above.

### 3.6 Micro-readings

| reading | twin | thread path |
|---|---|---|
| unbuffered ping-pong, one round trip (two parks, two wakes) | 1,422 to 1,550 ns (slot off: 3,334 to 3,419) | 1,677 to 1,715 ns |
| buffered send + receive that never parks, per iteration | 24.9 to 25.8 ns | 27.1 to 28.7 ns |
| allocation over 2,000,000 such iterations | 40 bytes | 40 bytes |
| `Mount` + `Unmount` | 38.4 to 38.6 ns | |

- The no-park path costs a twin nothing measurable and allocates nothing: an operation that completes returns
  a completed `ValueTask`. That the twin reads about 3 ns under the body is not explained.
- The parking path is where the cost is. Through the global queue a wake crosses carriers and costs twice the
  thread path, whose park completes inside its spin on this box. With the run-next slot the twin is under the
  thread path, at about 700 ns per step (two steps per round trip). The same program built by Go reads 425 to
  432 ns per round trip (232 ns at `GOMAXPROCS=1`), so the twin is 3.3 to 3.6 times Go here. The step is
  unoptimised (a semaphore take and release for the P, `Mount` and `Unmount`, a timestamp and a GC-pause read
  per step, park accounting); B4 prices it.

### 3.7 Predictions and verdicts

Written to the prediction file before each measurement.

| | prediction | verdict |
|---|---|---|
| B1-1 | switch off: GolibTests passes as before | FALSE on the first run (2 failed: two source censuses named the spike's new lines); 0 failed of 2,579 once both were answered |
| B1-2 | 1,000 goroutines with no twin: n + 8 to 12 threads | FALSE first (1,869), TRUE after the hand-off rule (1,012) |
| B1-3 | twinned storm at 100,000: at most 60 threads, 10 s, 400 to 1,500 MB | FALSE first (489 threads), TRUE after the injection budget (43, 6.9 s, 614 MB) |
| B1-4 | ping-pong: twin 700 to 1,800 ns, thread path 5 to 25 µs | FALSE both halves (3,334 to 3,419 without the slot; 1,677 to 1,715) |
| B1-5 | no-park path: no allocation, at most +10 ns | TRUE |
| B1-6 | a twin calling one untwinned parking callee prints Go's output; threads grow with concurrent parks | TRUE |
| B1-7 | no lost wakeup, no wrong output | TRUE in every arm run |
| B1-8 | the `getg` fix has a red | TRUE |
| B1-9 | 1 MB carrier stacks: no stack failure | TRUE so far |
| B1-10 | `GoFrame` change compiles | golib, runtime, time, sync and the storm compile; the full stdlib was not built |
| B1-11 | `Mount` + `Unmount` 20 to 60 ns | TRUE (38.4 to 38.6) |
| B1-12 | a contended `Lock` twin hops through the thread pool; few extra threads | consistent (about 19 threads beyond the carriers), not isolated |
| B1-13 | no injection arm: output unchanged | TRUE |
| B1-14 | run-next slot: ping-pong 300 to 900 ns | FALSE on the range (1,422 to 1,550), TRUE that it beats the thread path |
| B1-15 | hand-off rule: the no-twin arm reads n + 12, and the sync-sleep arm is unchanged | first half TRUE; second FALSE (its carriers fell from 1,011 to 206) |

### 3.8 Findings

1. **Injection needs a budget.** Twins cannot be preempted, so a long step can only be answered by more
   parallelism, and unbounded that is a thread per long step (489). A real build needs a bounded policy.
2. **Hand-off needs Go's rule.** A carrier per hand-off cost up to two threads per untwinned goroutine.
3. **The cost of a missing twin is per concurrently blocked call** (3.5). This is the line that decides how
   much twin coverage the macOS cap needs.
4. **Wakes need a run-next slot**, and a step still costs about 700 ns, against about 215 ns in Go (3.6).
5. **Hand-off covers golib's own parks only.** `Goroutine.EnterBlocked` and `GoNativeCall` (H's native-call
   seam) do not hand off a P in the spike: a goroutine blocked in native code holds its carrier's P. That is a
   B3 item. B1 needed nothing from H's seam and changed nothing in it.
6. **Per-thread caches keyed on "this thread is this goroutine" break** (`getg` was one). The thread-statics
   other code registers through `GoroutineThreadState.Register` (the memory profile's recording flag, the
   escape markers, the CPU sampler's draining flag, `runtime/debug`'s panic-on-fault flag, the runtime's
   held-lock list, `sync`'s proc id) do not move with the goroutine in the spike, and must in a real build.
7. **`ref` and `in` receivers cannot be twin parameters**, and a `ref` local does not survive an await. The
   hand twin takes the box and derives the `ref` again after each await; B2 has to emit that.
8. **A twin's panic trace names the state machine** (`main.MoveNext()`), and a parked twin has no thread id in
   a goroutine dump. Both are visible to a consumer who reads traces (1.5), and B2 and B3 owe the answers.
9. **The BCL runs continuations inline**, so a chain of twins completing synchronously deepens the carrier's
   stack. 1 MB carriers have not failed, and a real build probably wants its own task type and builder.
10. **Not twinned in B1:** `sync.WaitGroup.Wait`, `sync.Cond`, `RuntimeSemaphore`, the profiling arms. The
    `Lock` twin waits on the mutex's gate through the thread pool; the `Sleep` twin allocates a timer and a slot
    per sleep.
11. **`goǃ` positions are already keys**: the `[GoBoundedGoroutine]` interceptor records in `package_info.cs`
    key on them (a copied `package_info.cs` fails with GO2CS0010). B2's routing of `goǃ` to a twin has to
    compose with that interceptor.
12. **golib's source censuses caught the spike on its first full run** (3.4): an allocation past the counter
    and an unclassified thread-static. Those are the gates a real build meets, and they fire on new golib code
    whether or not the switch is on. Run GolibTests early in B2.
13. **At 100,000 goroutines the twin build uses less private memory than Go** (614 MB against 890 MB). Not
    explained; B4 reads where each build's memory goes.

### 3.9 What B1 did not measure, and instrument notes

- Nothing here ran on a hosted leg: no macOS or linux reading under twins exists yet. That is B3, on generated
  twins.
- The full stdlib was not built against the `GoFrame` change.
- The thread-pool share of the twin build's threads (B1-12) was not isolated.
- Twice the first arm after a build read about 3.5 s long on wall time (7.3 and 7.5 s where the repeat reads
  3.8 to 4.0 s), once with the switch off and once with it on. On the build as committed it did not (3.8 s).
  Every wall time above follows a discarded warm-up arm.
- A peak from the 250 ms sampler is a lower bound for a burst; burst arms quote the scheduler's own peak.

### 3.10 After the first repoguard run: the spike's golib members move to their own files (2026-11-11)

Sections 3.1 to 3.9 describe the tree at `1b8cc2658c`, and every reading in them was taken on that build.
The docs guard was then run on that commit for the first time and was red:
`TestDocsProvenanceNotesResolve` reported 39 provenance notes (of 934 checked) that no longer pointed at the
line they describe, and the two tests that plant a stale note on top of a clean tree failed with it. The
notes cite `src/core/golib/<file>.cs:<line>` with the text of that line. The spike had inserted lines above
cited lines in four files: `builtin.cs` (7 lines), `GoFrame.cs` (9), `runtime/Goroutine.cs` (7, above its
last cited line, 1436) and `channel.cs` (190, above one cited line).

The commit on top of `1b8cc2658c` changes no behaviour and moves the insertions out from under the notes:

- `selectꞏtwin` is in a new `golib/builtin.Twins.cs` (21 lines); `builtin.cs` is the base file again.
- `GoFrame.SwapMounted` is in a new `golib/GoFrame.Mounted.cs` (19 lines); `GoFrame.cs` differs from the
  base in one line (the declaration, now `public partial struct GoFrame`, no longer a `ref struct`).
- `Goroutine.Park` and the hand-off test are in `runtime/Goroutine.Multiplexed.cs` (now 238 lines);
  `Goroutine.cs` changes line for line at 61, 698 and 740, and otherwise only after line 1642.
- The channel twins stay in `channel.cs`, which needs them beside the private queue. Its one note,
  in `docs/ConversionStrategies-Reference/maps-and-channels.md`, is re-cited from line 1624 to line 1814,
  where the quoted line now is.

This replaces the file count in 3.1: nine existing golib and stdlib files change by 332 added and 10 removed
lines (237 of the additions are the channel twins), and four files are new (842 lines).

The arms of 3.3 and 3.5 were run again on the relocated build (same box, same sampler and hold). Every row
exits 0 with Go's output hash for its n.

| arm | n | wall | sampler peak threads | peak private | carriers | scheduler peak OS threads | steps | hand-offs | injections | steps over 10 ms | longest step |
|---|---|---|---|---|---|---|---|---|---|---|---|
| twins | 1,000 | 3.8 s | 24 | 30 MB | 12 | 28 | 3,947 | 0 | 0 | 12 | 13 ms |
| twins | 10,000 | 4.0 s | 28 | 76 MB | 18 | 31 | 39,988 | 0 | 36 | 87 | 30 ms |
| twins | 100,000 | 6.9 s | 35 | 609 MB | 24 | 36 | 405,804 | 0 | 445 | 923 | 111 ms |
| no twin | 1,000 | 3.8 s | 1,012 | 87 MB | 1,000 | 1,012 | 1,000 | 3,975 | 0 | 0 | 1 ms |
| one untwinned sleep | 10,000 | 4.3 s | 332 | 109 MB | 528 | 550 | 30,243 | 10,000 | 36 | 65 | 42 ms |
| switch off | 1,000 | 3.9 s | 1,011 | 92 MB | | | | | | | |

These agree with 3.3 and 3.5 inside the spread those sections already show. The 100,000 arm peaks at 36 OS
threads here against 43 before: the peak is the 12 processors' carriers plus however many P injections
overlap, so it moves from run to run, and "36 to 43" is the reading. The first arm after this build read
7.2 s on wall time where its repeat reads 3.8 s: the first-arm effect of 3.9 is back (three builds of four),
so the warm-up arm stays in every measurement.

GolibTests with the switch unset, on the relocated tree: 0 failed / 2,530 passed / 49 skipped of 2,579 (363 s).

Finding 14 (continuing 3.8). **golib is cited by line.** Any twin that is added to an existing golib file
moves the lines below it, and the documents cite those lines. A real change of this kind either keeps its
additions in files of their own (partial types make that cheap, as here) or re-cites every note
under it in the same commit. The first is the rule B2 follows; the channel twins are the case that cannot
follow it, and their re-cite is one note today.

The converter's full test (`go test ./...`) was then run on the relocated commit and was red on one test:
`TestByRefReceiversFollowTheReceiverRule` names `sync/mutex.cs:250`, the `Lock` twin, a hand-written
`this ref` receiver that says neither `[GoRecv]` nor `[GoCopyBound]`. The twin now carries `[GoRecv]`, as
`Lock` does. The storm builds and reads as before (twins at 1,000: 24 threads, 3.8 s; at 100,000: 43
threads, 7.0 s, 610 MB; switch off at 1,000: 1,011 threads; exit 0 and Go's hashes), and the test passes.
This is the fourth guard that read the spike's own lines (findings 12 and 14): a hand-written twin in a
hand-owned file is a receiver like any other. Whether the mark's generated pointer-receiver overload is the
box-taking form that finding 7 asks for was not read; B2 reads it.

## 4. B2: a twin generator in go2cs-gen (2026-11-11)

B2 asks whether a generator can write the twins B1 wrote by hand, from the converted code as it stands, and
what the owner's ruling on interfaces (OQ5: a default-bodied member or a separate hidden interface, never an
abstract member on the Go-named interface) costs in each form. Branch `claude/g-mn-spike2-b2`, on
`3e21aef0d8`. Windows x64, 12 logical processors, .NET SDK 10.0.400 (runtime 10.0.11, ILCompiler 10.0.11),
go1.26.10. Spike code: it stays on the branch.

### 4.1 What was built

- `go2cs-gen/TwinGenerator*.cs` (five files, 1,517 lines), off unless the project's `GoTwins` property is true.
  It runs every sibling generator of its own assembly in a nested driver, so it binds the members those
  generators add (field access, receivers, interface implementations); the siblings therefore run twice in a
  twinned project.
- Candidates are the static methods with bodies. A candidate is refused before any analysis when it is an
  instance method, takes its receiver or a parameter by reference, returns by reference, mentions a ref-like
  type, is unsafe, is an iterator, is already async-shaped, already has a twin, or is named by the refuse file
  (4.6). The rest go through a fixpoint: a function parks when it calls a twin (golib's, another assembly's by
  name, or one of its own) or holds a `go` site whose target parks.
- A twin is the original body's text between `#line` directives (errors, breakpoints and stack traces name the
  visible file and line), `async ValueTask` or `ValueTask<T>`, named `<name>ꞏtwin`, in `<file>.twins.g.cs`.
  Each parking call becomes `await <callee>ꞏtwin(…)`; a `go` site whose target has a twin becomes a twin start;
  a `go` literal becomes a pair of lambdas over the same captures; a `ref` local derived from a box is derived
  again after every awaiting statement.
- The program entry: a generated `GoTwinEntry` class whose `Main` runs `Mainꞏtwin` on the scheduler when
  `GO2CS_MN=1` and `Main` otherwise.
- Interface twins in two shapes (4.4), with forwarders on the implementers the converter records.
- A report file per project (`go2cs.twins.report.g.cs`): candidates, twins, await sites, refusals by reason,
  and the candidates a refusal stopped from parking ("blocked").
- golib: `builtin.Twins.cs` gains the name-uniform aliases the generator calls, the twin starters and the entry
  runner (77 lines); `channel.cs` gains the send twin's operator form (7 lines); `GoScheduler.Monitor` is
  corrected (4.5).
- `Directory.Build.targets` gains the per-project switches of 4.2 (35 lines). The pointer-adapter template
  gains one keyword, `partial`, emitted only when twins are on (4.7).
- `src/tests/Behavioral/MnIfaceTwin`: a converted Go program whose interface `Parker` has six implementers:
  `gate` (a pointer receiver emitted in box form; a mutex and a sleep), `napper` (a value receiver; a sleep),
  `fixed` and `late` (no park; `late` reaches the interface through a dynamic assertion), `relay` (a pointer
  receiver emitted as a `this ref` extension; a sleep), and `HandParker`, a C# class written by hand in
  `hand.cs` the way the consumer document teaches. `hand.cs` also holds a class that has the method and does
  not name the interface. The fixture is not registered in the solution or in the behavioral runner; its
  `main.cs.target` is its `main.cs`, which a re-conversion of `main.go` reproduces byte for byte.

No visible converted file changes in any build of B2: `git status` over the storm's and the fixture's
non-generated files is empty with twins on.

### 4.2 Build properties

| property | meaning |
|---|---|
| `GoTwins` | the project gets twins (set in a project file) |
| `GoTwinsFor=<names>` | the named projects get twins; the command-line form (a `GoTwins` on the command line is global and reaches every referenced project) |
| `GoTwinsGo=false` | `go` sites are left as thread starts (an arm) |
| `GoTwinsIface=hidden\|default`, `GoTwinsForIface` | the interface twin shape; the second is the command-line form |
| `GoTwinsIfaceAll`, `GoTwinsForIfaceAll` | twin every member of every Go interface of the project, not only those with a parking implementer in it |
| `GoTwinsRefuse=<file>` | rows `path\|line\|CSxxxx`; a candidate whose span holds the line is refused (4.6) |
| `GoAotFor=<names>` | publish the named projects with Native AOT; a `PublishAot` on the command line reaches the netstandard2.0 generator project and fails it |

### 4.3 The converted MnMixedStorm under generated twins

The storm of 2.3, converted and untouched, built with `GoTwinsFor=MnMixedStorm`: the generator twins `worker`
(5 await sites) and `Main` (4), turns both `go` sites into twin starts, and refuses nothing. Same sampler and
hold as 3.3. Every row exits 0 and its output hash equals the Go build's for that n.

| n | build | wall | peak OS threads | peak private |
|---|---|---|---|---|
| 1,000 | thread path (switch off) | 3.8 s | 1,011 | 89 MB |
| 1,000 | hand twins (3.3) | 3.8 s | 24 | 29 MB |
| 1,000 | generated twins | 3.8 s | 25 | 30 MB |
| 10,000 | hand twins (3.3) | 4.0 s | 28 | 75 MB |
| 10,000 | generated twins | 4.0 s | 28 | 76 MB |
| 100,000 | hand twins (3.3) | 6.9 s | 43 | 614 MB |
| 100,000 | generated twins | 6.9 s | 36 | 620 MB |

The generated twins read as the hand twins did. A second read after the interface work and the correction of
4.5 gave 25, 33 and 36 threads, 6.8 s and 617 MB at 100,000, with the same output hashes and an identical
generated tree.

The same project published as a Native AOT image under generated twins (`GoAotFor=MnMixedStorm`; publish
1,290 s, 0 errors, 194,819,072 bytes), one run per arm, same sampler:

| n | path | wall | peak OS threads | peak private | output hash |
|---|---|---|---|---|---|
| 1,000 | thread path (switch off) | 3.5 s | 1,009 | 120 MB | Go's |
| 1,000 | generated twins | 3.6 s | 25 | 72 MB | Go's |
| 10,000 | generated twins | 3.8 s | 26 | 130 MB | Go's |
| 100,000 | generated twins | 7.9 s | 34 | 632 MB | Go's |

The thread counts and the output carry to the image. Two things differ from the JIT build and neither was
explained: the image holds more private memory at the small arms (72 MB against 30 at 1,000), and the one run
at 100,000 took 7.9 s against 6.9 and 6.8. The image starts faster: a no-hold run of 1,000 exits in 0.4 s
against 4.1 to 4.6 s.

### 4.4 Interface twins: the two shapes OQ5 allows

A call through a Go interface parks when the implementer behind it parks, so the interface needs a way to reach
the implementer's twin. Both ruled shapes were built and measured on the same fixture.

- **Hidden.** The Go-named interface gains a nested public interface `Parkerꞏtwin` (hidden from editors) that
  carries `Parkꞏtwin`, and a static `Parkꞏcall(recv, args)` that tests `recv is Parkerꞏtwin` and otherwise wraps
  the plain call. Implementers with a twin gain the base-list entry and a forwarder. The Go-named interface's
  own member list does not change.
- **Default.** The Go-named partial interface gains `Parkꞏtwin` with a default body that wraps the plain call.
  Implementers with a twin gain an explicit implementation.

The fixture prints, per implementer, the sum of 100 calls through the interface, one from each of 100
goroutines, each holding 20 ms, then the reflected method count of `Parker`. With `MN_IFACE_HAND=1` it adds the
hand-written implementer (which answers 5, so 500) and the C# side's own `typeof(Parker).GetMethods()` count.
The "1,000 parked" rows run one implementer with 1,000 goroutines holding 3 s.

| reading | twins off | hidden, JIT | hidden, AOT | default, JIT | default, AOT |
|---|---|---|---|---|---|
| build errors, hand.cs unchanged | 0 | 0 | 0 | 0 | 0 |
| output against Go's, switch off and on | equal | equal | equal | one row differs | one row differs |
| `reflect` method count of `Parker` (Go prints 1) | 1 | 1 | 1 | 2 | 2 |
| C# `GetMethods()` count | 1 | 1 | 1 | 2 | 2 |
| hand implementer through the interface | 500 | 500 | 500 | 500 | 500 |
| 1,000 parked in `gate`, peak OS threads | | 30 | 24 | 28 | 23 |
| 1,000 parked in `napper` | | 24 | 22 | 24 | 23 |
| 1,000 parked in `relay` (by-ref receiver, no twin) | | 1,013 | 1,013 | 1,013 | 1,011 |
| 1,000 parked in the hand implementer | | 1,013 | 1,011 | 1,013 | 1,011 |
| Native AOT publish, wall | 1,312 s | | 1,298 s | | 1,289 s |
| Native AOT image, bytes | 195,318,272 | | 195,352,576 | | 195,352,576 |
| trim warning lines | 767 | | 767 | | 767 |

The AOT `relay` and hand rows were read with `GO2CS_MN_RUNNEXT=0` on images that still carried the defect of
4.5; the corrected image is read in 4.5.

- **K2 does not fire in either shape.** A hand-written C# class that implements the Go interface compiles
  unchanged and answers correctly under JIT and under Native AOT, with twins off, hidden or default.
- **The hidden shape is invisible to both reflections.** Output equals Go's in every arm and the method count
  stays 1 on the Go side and on the C# side. Its visible cost is one public nested interface per twinned Go
  interface, and one base-list entry on each implementer that has a twin.
- **The default shape works and shows.** It builds and runs under JIT and AOT, and the Go-named interface now
  has two members: `reflect`'s `NumMethod` prints 2 where Go prints 1, and a C# caller's `GetMethods()` reads 2.
  The Go side can be filtered in golib's method-set code; the C# side cannot.
- **An implementer without a twin costs a thread per parked call, never a wrong answer.** The by-ref-receiver
  implementer and the hand-written one both run the plain method on the carrier, which hands its P off: 1,000
  parked calls hold about 1,000 threads, as today. For a hand-written implementer this is the documented rule a
  C# author needs; for by-ref receivers it is the generator's largest refusal class (4.6).
- **Twins cost 34,304 bytes of a 195 MB AOT image** on this fixture and no trim warning. The 22 minutes of ILC
  are the rooted `go.*` closure under partial trimming, with or without twins.
- The duck-typed C# class does not satisfy the assertion with twins off either, so the fixture has no reading
  on late binding.

### 4.5 A scheduler defect in B1, found by the AOT arm

`GoScheduler.Monitor`'s run-next steal walk held the carriers lock and queued each stolen goroutine inside the
`foreach`. Queuing can add a carrier, the lock is reentrant, and the add appends to the list under its own
enumerator: `InvalidOperationException: Collection was modified`, exit 2. The pushed B1 tip (`3e21aef0d8`)
carries it.

- Red: the 1,000-parked `relay` and hand arms died 5 of 5 on both AOT images. The same `relay` arm ran 5 of 5
  clean under JIT (run-next steals 0, 0, 1, 0, 0), so no B1 arm met it: it is timing, not AOT.
- Correction: the walk collects the steals under the lock and queues them after it.
- Green: a hidden-shape image published from the corrected golib (1,306 s, the same 195,352,576 bytes) ran
  both arms 5 of 5 with run-next on: exit 0, no exception, 1,011 threads, 1,003 hand-offs, and 4 or 5 run-next
  steals in every run, so the walk that threw is the walk that ran. Output equals Go's at the same size.
- The throw needs a steal whose queuing starts a carrier while the walk has entries left. The JIT build stole
  0 or 1 times a run in this arm and the AOT image 4 or 5; why the image steals more was not read.

### 4.6 The standard library: scouting and the refuse loop

The generator was run over the 55 converted standard-library packages the fixture's build reaches, reports
summed. This is a scout of the generator's reach, not a run: no std twin is called by any measured arm.

| reading | count |
|---|---|
| candidates (static methods with bodies) | 5,333 |
| twins written | 372 |
| await sites | 446 |
| refused before analysis | 904 |
| refused candidates that would otherwise park ("blocked") | 49 |

- Twins by package: `syscall` 199 of 470 candidates, `runtime` 145 of 2,935, `io` 14 of 54,
  `internal/godebug` 5, `internal/bisect` 3, `sync` 3 of 90, `internal/testlog` 2, `reflect` 1 of 414. The
  other 47 packages get none.
- Refusals by reason: by-ref receiver 499, by-ref parameter 155, unsafe 132, instance method 62, ref-like type
  49, explicit implementation 4, iterator 2, already has a twin 1.
- Blocked by reason: unsafe 39, by-ref receiver 6, by-ref parameter 3, ref-like 1; `syscall` 43 and `runtime` 6.
  The by-ref refusals are the largest class and block 9 functions; the unsafe ones block 39, all but one in
  `syscall`.
- Parking spreads through two edges that mean little under Go: `syscall` parks through `LazyProc` (a mutex on
  first use), and `runtime` through `gcStart` and `mallocgc`. A twin census that follows those edges overstates
  what parks in practice; B5 measures the share on a real program and whether those edges want a cut.

**The refuse loop.** A twin is a copy of the body with awaits in it, and C# rejects some copies that the
pre-refusals cannot see from the syntax: a `ref` local or a returned reference held across an await. The loop
builds the project, reads the compiler's errors in twin files (the `#line` directives make each error name the
visible file and line), writes one `path|line|CSxxxx` row per error into the refuse file, and builds again; the
generator refuses the candidate whose span holds the line. It ends when a build has no error.

| package | candidates | twins, round 1 | rows | twins at the fixpoint | rounds | build wall |
|---|---|---|---|---|---|---|
| `io` | 54 | 14 | 0 | 14 | 1 | 111 s |
| `sync` | 90 | 3 | 0 | 3 | 1 | 113 s |
| `syscall` | 470 | 199 | 9 | 195 | 2 | 124 s, 117 s |
| `runtime` | 2,935 | 145 | 0 | 145 | 1 | 195 s |

- `syscall`'s nine errors are two shapes in four functions: CS9217 six times in `StartProcess` (a `ref` local
  the generator could not derive again after an await; its report counts one such local), and CS8178 three
  times, once in each standard-handle initialiser (an assignment through a property that returns a reference,
  with an awaited right-hand side).
  Refusing the four costs no other twin: 199 becomes 195.
- No round ended on an error outside a twin file, and none failed without a compiler error.
- The four packages hold 361 of the 372 scouted twins. The other 11 built in the scout without an error.
- Red control: a refuse file seeded with one row inside a twinned `io` function dropped `io` from 14 twins to
  12 (the function and its one caller) and the report named the candidate and the reason given in the row.
- The loop is a build-time cost paid once per package and release, not per build: its output is a file of rows.
  In a product it would run where the converter runs, and the rows would ship beside the converted code.

### 4.7 Guards and red controls

| guard | reading |
|---|---|
| generator tests, first run with the entry class | 495 of 496: the static-class census named the generated entry (`TwinGenerator.Emit`, a `static class`). The entry became a sealed class; 496 of 496 |
| generator tests, adapter template with an unconditional `partial` | 484 of 496: twelve tests that find the adapter by its `sealed class` spelling |
| generator tests, `partial` only under twins | 496 of 496, no test edited |
| golib tests, corrected monitor and the new twin members | 0 failed, 2,530 passed, 49 skipped |
| refuse loop, seeded row (4.6) | red as designed: 14 twins to 12, candidate named |
| provenance guard | red on a real line move (last row); it named the file, the line and the quoted text |
| scheduler defect (4.5) | red 5 of 5 on two arms and two images before the correction; green 5 of 5 on both arms after it |
| twins-off output | the fixture's adapters read `sealed class` again; `runtime`, `sync`, `io` and `syscall` rebuilt with twins off after the loop hold no twin name and no twin directory |
| twins-on output across the template change | the fixture's generated tree is identical by hash before and after |
| converter suite and repository guards (`go test ./...`) | first run: the converter suite and every other package ok (1,076 s); the repository guard red on 1 of 934 provenance notes, because the five lines added to ImplementGenerator moved a line the generator reference cites. Re-cited at its new line; the guard re-run with this record in place is green |

Not run: the solution-wide Debug build that a landing generator change owes, and a `-tests` row. Nothing here
lands; the twins-off path is covered by the generator tests and the rows above.

### 4.8 Predictions and verdicts

Every line was written before its run; the dated blocks are in the lane's prediction file.

| id | prediction | reading | verdict |
|---|---|---|---|
| B2-P1 | the converted storm under generated twins reads inside the hand twins' spread, Go's hashes | 25 / 28 / 36 threads, 3.8 / 4.0 / 6.9 s, hashes equal | HIT |
| B2-P2 | no visible file changes; twins off leaves the generated tree as it was | both hold | HIT |
| B2-P3 | the generator twins exactly `worker` (5 await sites) and `Main` (4), refuses nothing | as predicted | HIT |
| B2-P4 | both `go` sites become twin starts; the literal becomes a lambda pair; `Wait` stays a hand-off | as predicted (3 hand-offs) | HIT |
| B2-P5 | a default-bodied twin on a generated interface part runs under JIT and AOT, hand implementer unchanged | holds | HIT |
| B2-P6 | storm build wall, twins on at most +20% | +0.6% on medians of three; the project is too small to show a cost | HIT, uninformative |
| B2-W1 | the same pair on `runtime`: +50% to +110% | 189.5 s against 99.4 s, +91% | HIT |
| B2-W2 | `runtime` assembly +3% to +8% | +248,320 bytes on 3,851,264, +6.4% | HIT |
| B2-P7 | the refuse loop ends in at most 3 rounds | 1, 1, 2, 1 | HIT |
| B2-I1 | hidden shape: 0 errors JIT and AOT, Go's output, hand implementer unchanged | holds | HIT |
| B2-I2 | default shape: 0 errors JIT and AOT, hand implementer unchanged; K2 fires in neither | holds | HIT |
| B2-I3 | method count 1 under hidden; under default 2 on the CLR side and (65%) 2 on the Go side | 1 and 1; 2 and 2 | HIT |
| B2-I4 | box-form receiver through the adapter, value receiver through its part, by-ref receiver refused | as predicted | HIT |
| B2-I5 | 1,000 parked: twinned implementers at most 60 threads, untwinned 900 or more, Go's output | 22 to 30; 1,011 to 1,013 | HIT |
| B2-I6 | a `partial` adapter leaves the generator tests green (0 to 2 assertions to respell) | 12 failed; the keyword is now emitted only under twins, 496 of 496, no test edited | MISS |
| B2-I7 | the storm's readings do not move after the interface work | same hashes, same generated tree | HIT |
| B2-A1 | twins-off AOT publish of the fixture under 6 minutes | 1,312 s | MISS |
| B2-A2 | each shape publishes within 1.25x of the twins-off wall | 1,298 s and 1,289 s | HIT |
| B2-A3 | (first form) the default shape stalls in ILC | it does not; the first run was stopped before a control existed | MISS |
| B2-R1 | `syscall` reaches a fixpoint in 2 rounds, round 1 records exactly the 9 scouted rows | holds | HIT |
| B2-R2 | the rows refuse 3 to 5 candidates; 187 to 196 twins remain | 4; 195 | HIT |
| B2-R3 | `runtime`, `io`, `sync` end in round 1 with 0 rows | holds | HIT |
| B2-R4 | no round ends stuck or failed without a compiler error | none | HIT |
| B2-R5 | a seeded row drops `io`'s twins by at least 1 and is named in the report | 14 to 12, named | HIT |
| B2-G1 | with the corrected monitor the two arms that died run 5 of 5 clean with run-next on | 5 of 5 each, 1,011 threads, 4 or 5 steals a run, Go's output | HIT |
| B2-S1 | the storm publishes as a Native AOT image under generated twins, 1,250 to 1,400 s | rc 0, 0 errors, 1,290 s | HIT |
| B2-S2 | the image prints Go's output in every arm; threads about 1,010 off, at most 40 / 45 / 50 on | hashes equal; 1,009; 25 / 26 / 34 | HIT |
| B2-S3 | at 100,000 the image runs in at most 6.9 s and 400 to 650 MB; no arm ends on an exception | 7.9 s (one run); 632 MB; none | MISS on the wall |

Four misses, and what each one was. S3's wall was a line drawn at the JIT build's reading with one run behind
it; the image was a second slower at 100,000 and B4 owes the repeated pair. I6 was a change I made to every build's output where the plan said twins
only; the tests that keep the adapter's spelling caught it. A1 was a guess about ILC with no reading behind it.
A3 was a verdict drawn from an arm with no control, corrected by running the control.

The two cost lines from B1 that B2 can speak to: IL size is inside its bar on `runtime` (+6.4% against
"+35% on net/http"); generator time is outside it (+91% against "+50% on net/http"). B4 prices both on
`net/http`, and the first lever on time is to stop running the sibling generators twice.

### 4.9 Findings

The generator.

1. **The twins can be generated from the converted code as it stands.** No visible file changed, the storm's
   generated twins read as the hand twins did, and the fixture prints Go's output. The "class defer frame" of
   the plan was not needed: the twin keeps the original's defer frame and runs it in the same `finally`.
2. **`#line` carries.** A compiler error in a twin names the visible file and line, which is what makes the
   refuse loop possible; a breakpoint in the visible file binds both copies. Twin files disable warnings, so a
   twin's diagnostics never reach a build log unless they are errors.
3. **A deferred call runs in its plain form.** A deferred unlock is right as it is. A deferred call that parks
   blocks its carrier, which hands its P off: a thread for the length of the park, never a wrong answer.
4. **golib still has plain-only blockers.** `WaitGroup.Wait`, `sync.Cond`, the runtime semaphore and `foreach`
   over a channel have no twin. Each costs a hand-off where it blocks (the storm's one `Wait` reads as 3
   hand-offs). They are golib work of the same kind as B1's.
5. **A `go` site in a plain copy starts a thread.** Only twin copies route `go` to the scheduler. A goroutine
   started from code that has no twin (a refused function, a hand-written C# caller) is a thread, as today.
6. **Call sites that an interceptor rewrites are not rewritten in the twin copy.** No measured shape depends
   on one. B3 has to census them before any claim about the std closure.
7. **Twins bypass the converter's whitespace pass** (they are text copies), and one cosmetic case remains: a
   re-derivation after a switch section that ends in `break` is unreachable code, silent under the disabled
   warnings.
8. **The by-ref classes are the largest refusal and a small loss.** By-ref receivers and parameters are 654 of
   904 refusals and block 9 functions in this closure; unsafe bodies block 39, nearly all in `syscall`, where
   the park the census sees is `LazyProc`'s first-use mutex (4.6). The ref-local
   census reads 13,011 declarations in the corpus with 99.7% in the shapes the generator re-derives; that
   census has no negative control yet.
9. **Hand-written twins are seen by the sibling generators.** A twin written by hand in a converted file is
   picked up by the method-set, receiver and embedding generators as an ordinary method. Generated twins are
   not, because the siblings never see them. For `Mutex`'s `Lock` twin this answers 3.10's open read: the
   receiver mark does generate the box-taking overload a twin can call, and the same pass writes the twin's
   name into the type's generated method-set table, where Go-side reflection could find it (not run). B1's
   hand twins stay where they are; no new ones should be added, and a landing design generates them all.
10. **The sibling generators run twice in a twinned project**, once in the compiler and once in the twin
    generator's nested driver. The storm project is too small to show the cost (4.8, B2-P6); the pair on
    `runtime` is in 4.8.

Interfaces.

11. **Both OQ5 shapes work under JIT and Native AOT, and a hand-written implementer compiles unchanged under
    both.** The hidden shape is the one that keeps both reflections and Go's output; the default shape needs a
    golib filter for `reflect` and still shows a second member to C# reflection.
12. **The "auto" policy is per assembly.** It twins an interface member when the same project holds a parking
    implementer, and cannot see an implementer in another assembly. `GoTwinsIfaceAll` twins every member; its
    cost on a real closure is B5's to measure.
13. **The converter already records a dynamic assertion** (`x.(Parker)` on `late`) as an implementation, so the
    forwarders cover it.
14. **The pointer adapter has to be `partial` under twins, and only under twins.** The first version changed
    the keyword in every build and twelve generator tests caught it; a change to non-twin output is a K4-class
    slip and the guard that catches it exists.

Consumer-visible, to fold into B6's list.

15. A twin has its original's accessibility: a public Go function gains a public `<name>ꞏtwin` beside it,
    hidden from editors, visible to reflection and to a determined caller.
16. Hidden shape: one public nested interface per twinned Go interface, one base-list entry per twinned
    implementer. Default shape: one more member on the Go-named interface.
17. A C# class that implements a Go interface keeps working and costs a thread per parked call through it. A
    C# author who wants the cheap path would implement the nested twin interface by hand; that is new, optional
    surface, and no arm here wrote one.
18. The program entry becomes a generated class when the main package is twinned; `Main` stays and still runs
    the thread path when the switch is off.

The scheduler and the tooling.

19. **B1's monitor could throw under load** (4.5). Found by an arm B1 did not have; fixed on this branch.
20. `GoTwins` and `PublishAot` given on the command line are global properties and reach every referenced
    project; the per-project forms (`GoTwinsFor`, `GoAotFor`) exist for that reason.
21. A Native AOT publish of a converted program is about 22 minutes of ILC on this box with or without twins,
    and leaves self-contained host files in the build output that stop a later framework-dependent run of the
    same project until the output directory is removed.

### 4.10 What B2 did not measure

- No std twin was run. The scout and the loop prove that the generated std twins compile, not that they behave;
  B3 runs shapes whose parks are in std (`io.Pipe`, `sync` beyond the mutex, `time`).
- Cross-assembly routing ran only against B1's hand twins in golib, `sync` and `time`: a twin calls
  `<callee>ꞏtwin` in another assembly when that assembly holds one. No arm built two generator-twinned
  assemblies and called across them at run time.
- macOS and Linux. Every reading is Windows x64; the hosted legs are B3.
- The native seam, cgo and `LockOSThread` under generated twins (B3, read-only against the seam).
- Generator time and IL size on a large package beyond the one pair in 4.8; B4 prices them on `net/http`.
- The TypeScript closure (B5).
- Instance methods (62 refused, none blocked here) and generic functions were not given their own arm.

### 4.11 Ruling on the interface twin shape (2026-11-11, after B2 was posted)

The coordinator ruled the OQ5 shape on the readings of 4.4: **the hidden twin interface**.

- A default-bodied member makes a Go-named interface report 2 methods to `reflect`'s `NumMethod` and to C#
  `GetMethods()` where Go has 1. That is a change to what a consumer observes on a tier-1 surface, and the hidden
  shape does not make it.
- The guard the public-surface plan names (no Go-named interface gains an abstract member) fits the hidden shape
  as it stands: the Go-named interface gains no member at all.
- The default shape stays in the spike's generator as a measured arm (`GoTwinsIface=default`) and is not carried
  forward. From B3 on, every build that twins interfaces uses the hidden shape (`GoTwinsIface=hidden`).
- The build-wall cost of 4.8 (+91% on `runtime`, outside B1's bar) is noted by the coordinator; the nested-driver
  lever is priced in B4.

## 5. B3: the shapes under generated twins (2026-11-11)

Branch `claude/g-mn-spike2-b3`, on B2's `d221a1182a`. B3 asks whether the programs that park in Go's ways print
Go's output when the generator of section 4 writes their twins, on this box and on the hosted runners, and what a
goroutine blocked outside golib (a native call, a park in an untwinned package) costs. The coordinator asked for a
lean phase: no run beyond what these questions need.

### 5.1 What was run

- **The 19 programs**: the 18 shapes of B0 and MnMixedStorm, each through `run-behavioral.ps1` with
  `GoTwinsFor=<name>` in the environment and `GO2CS_MN=1`. The runner compares stdout and the exit code with Go's.
  The control that twins were on is each program's twin report; a direct run of the built program gives the
  scheduler's reading.
- **MnBlockedIO**, a new Behavioral project (registered beside MnMixedStorm; golden and Go reference taken the
  same way). K goroutines sit in a read that cannot finish until main writes: first on OS pipes (the read blocks
  its thread inside a native call), then on `io.Pipe` (the read parks inside package `io`). While they are blocked,
  50 pairs of goroutines pass a counter over channels 100 times each; only then does main feed the readers.
  Sizes come from the environment (`MN_IO_KIND`, `MN_IO_K`, `MN_IO_ROUNDS`, `MN_IO_PAIRS`); the defaults (K 8) are
  what the behavioral suite runs.
- **Hosted**: the `behavioral-smoke` stage of `os-matrix.yml` on this branch builds the filtered program with
  generated twins and then runs it directly beside Go's build of the same source, printing `READING mn-direct`
  lines (commit `c1494b9d84`). One dispatch per host family, `filter=MnMixedStorm`.

### 5.2 The 19 programs (Windows x64, 12 processors)

| program | twins / candidates | await sites | runner | peak OS threads | parks | hand-offs |
|---|---|---|---|---|---|---|
| MnMixedStorm | 2 / 8 | 9 | PASS | 43 | 406,484 | 3 |
| ChannelRendezvous | 1 / 2 | 8 | PASS | 23 | 8 | 1 |
| CloseWakesBlocked | 1 / 3 | 14 | PASS | 23 | 4 | 1 |
| ChannelReceiveFromClosed | 1 / 2 | 4 | **FAIL**, then PASS (5.3) | 23 | 0 | 1 |
| SelectStatement | 11 / 15 | 27 | PASS (not compared) | 24 | 171 | 148 |
| SelectRandomFairness | 1 / 2 | 4 | PASS | 23 | 0 | 1 |
| SelectSendDefault | 8 / 13 | 27 | PASS | 24 | 6 | 1 |
| SelectSendRecvMix | 1 / 2 | 7 | PASS | 23 | 0 | 1 |
| NilChannelInSelect | 1 / 2 | 6 | PASS | 23 | 1 | 1 |
| DeepSelectRecursion | 3 / 4 | 5 | PASS | 23 | 0 | 1 |
| SyncTimerChannel | 1 / 4 | 11 | PASS | 25 | 738 | 4 |
| GoroutineWaitState | 2 / 9 | 12 | PASS | 27 | 2,004 | 2 |
| GoroutineParkStorm | 1 / 3 | 2 | PASS | **175** | 1 | 2 |
| GoexitDefers | 2 / 4 | 7 | PASS | 23 | 2 | 1 |
| GoroutinePanicExitCode | 1 / 2 | 1 | PASS (exit 2) | 23 | 1 | 1 |
| GoroutinePanicAfterDeferredDone | 1 / 4 | 0 | PASS (exit 2) | 23 | 0 | 2 |
| PanicRecover | 0 / 6 | 0 | PASS | no reading | | |
| GoroutineRecoveringWhenMainReturns | 1 / 5 | 0 | PASS | 24 | 0 | 4 |
| CrashGoroutineCreatedBy | 2 / 13 | 2 | **FAIL** (5.3) | 24 | 0 | 26 |

First battery: **17 of 19 PASS** by the runner's verdict. After the two generator-side fixes of 5.3: **18 of 19**,
and the last red is one token of one trace line. The runner compares 18 of the 19 with Go on stdout and exit code;
SelectStatement is not compared (2.2), so the claim for it is that it builds with twins, matches its golden and
its direct run exits 0. Compared with Go, the count is 17 of 18. `Main` has a twin in 18 of the 19 (PanicRecover parks nowhere, so it has no twin and never
starts the scheduler). One hand-off is the base line of every program with a main twin: the main thread waits for
the main twin outside the scheduler.

**Confirmation battery (20:04 to 20:23 CST, the generator, golib and runtime as committed):** the 19 programs and
MnBlockedIO again in one run. 19 PASS and 1 FAIL, the FAIL being CrashGoroutineCreatedBy on the goroutine id of
5.3 (`created by main.child in goroutine 3`). Twin and await-site counts equal the table's; the reports read
`ref-spills=1` for ChannelReceiveFromClosed and `ref-spills=0 ref-blocked=0` for the other 18. Two cautions on
that run: SelectStatement's Output phase reads `skip 1`, as above; and MnBlockedIO's build was up to
date, so its twin report was not rewritten. The control used for it is the binary: it names `Mainꞏtwin` and
`ioReaderꞏtwin`, and PanicRecover's binary (no twin) names neither.

Two rows are levers for B4 and not defects:

- **GoroutineParkStorm reads 175 OS threads**: its goroutines start from a `go` literal (it takes an argument
  and defers) that the spike's generator left on the launcher (the report reads `go-other=1 closure-parks=2`).
  Both parks of that body (a `sync.WaitGroup` wait and an unbuffered send) therefore hold a thread. The reach of the
  generator over `go` literals, and a twin for `sync`'s waits (5.6 shows that step for `io`), are what lower it.
- **SelectStatement reads 148 hand-offs in 171 parks**: every `go` site is a twin start (11), but the report
  counts 6 park calls in the 4 functions that got no twin and 1 inside a closure, and a park through one of those
  is a thread park that hands its P off. Output is Go's either way; the cost is threads and context switches.
  Why those 4 functions were refused was not chased.

### 5.3 The two reds

**ChannelReceiveFromClosed: CS4007 in the Main twin.** The statement is `fmt.Printf("%d "u8, <-c)`. Its twin
awaited the receive in place, after the u8 literal (a ref struct) had been evaluated, and the compiler refuses a
ref struct that lives across an await. The generator now applies **rule S (spill)**: when a ref struct value is
evaluated before an awaited call inside one statement, and everything evaluated earlier is inert (constants, u8
literals, static method names, calls already moved), the awaited call moves ahead of its statement into a
`var ᴛtwinN = await …;` on the statement's own line. Anywhere else the call is left as a blocking call, which
hands its P off as a thread park does. The twin report counts both (`ref-spills=`, `ref-blocked=`). After the
change the program passes with `ref-spills=1 ref-blocked=0`; the generated line reads
`var ᴛtwin1 = await ᐸꟷꞏtwin(c); fmt.Printf("%d "u8, ᴛtwin1);`. No other program of the 19 has a spill or a
blocked site, and the visible `.cs` is untouched (K4 holds).

**CrashGoroutineCreatedBy: one trace line.** Go prints `created by main.child in goroutine 1`. The first run
printed `created by main.MoveNext in goroutine 3`. Two separate causes:

1. *The name.* A twin runs as its state machine's `MoveNext`, on a compiler-generated nested type named
   `<childꞏtwin>d__N`. The runtime's frame namer (`goFrameName` in `runtime/managed_impl.cs`) now maps such a
   frame to the function the twin was generated from, and drops the IL offset (it belongs to the state machine).
   After the change the line reads `created by main.child in goroutine 3`. golib has a second reader that derives
   Go names from methods (`GoSyntheticPC.GoNameOf`); no program of this battery reaches it through a twin, so it
   is left as it was and needs the same mapping in a real build.
2. *The goroutine id.* The main twin is not goroutine 1. `RunMainꞏtwin` registers the twin as a new goroutine
   (id 3 here) while goroutine 1 stays the waiting main thread. That is the item B1 recorded as unsettled,
   together with "a `Goexit` on main waits for ever" (3.9): the registry's main entry is read by the exit path,
   by the deadlock detector and by `Goexit`, so making the twin goroutine 1 changes all three and was not done
   under a lean B3. **It stays open and is priced in the final answer.** It is not a K1 stop: the id is a registry
   number golib assigns, and the twin shape does not prevent golib assigning 1 to the main twin.

### 5.4 The storm on hosted runners (generated twins, `GO2CS_MN=1`, ref `c1494b9d84`)

Every output equals Go's. Walls in seconds; threads are the scheduler's own `peak-os-threads`.

| host | Go, 1,000 / 10,000 / 100,000 | C# on threads, 1,000 | twins 1,000 | twins 10,000 | twins 100,000 |
|---|---|---|---|---|---|
| linux-x64 (4 P) | 3.1 / 3.1 / 3.4 s, 6 threads | 3.9 s, 1,011 threads | 3.6 s, 22 | 4.2 s, 23 | 7.3 s, 29 |
| osx-arm64 (3 P) | 3.4 / 3.2 / 3.4 s, 4 to 5 | 7.2 s, 1,012 | 4.1 s, 20 | 8.5 s, 26 | 29.6 s, 30 |
| osx-x64 (4 P) | 3.2 / 3.2 / 3.7 s, 5 | 5.9 s, 1,012 | 4.7 s, 24 | 5.6 s, 34 | 18.1 s, 33 |

B0 read OutOfMemory on all three hosts at 100,000 goroutines on the thread path. Under generated twins the
program finishes on all three with at most 34 OS threads. The wall at 100,000 is 2.1 times Go's on linux-x64, 4.9
times on osx-x64 and **8.7 times on osx-arm64** (the 3-processor runner; 54 injections). The storm holds each
goroutine for a fixed time, so Go's wall is flat and ours grows with the per-goroutine cost; B4 prices that cost.

### 5.5 A goroutine blocked in a native call (MnBlockedIO, `MN_IO_KIND=os`; Windows x64, 12 P)

B1's scheduler hands a P off only for golib's own parks. A goroutine blocked inside a native call holds its
carrier and its P, and the monitor's answer (a P injected beside a long step) is capped at the P count. Go's
sysmon retakes the P of a goroutine it finds in a syscall. The spike adds that retake to the scheduler's monitor:
a carrier whose mounted goroutine reads an odd native-call depth (the seam's own field) on two ticks in a row,
while work waits, has its P taken back outside the injection budget; the carrier settles the account when the
step parks or ends. All of it is in `GoScheduler.cs` behind `GO2CS_MN=1`; `GO2CS_MN_RETAKE=0` turns it off (the
red arm). **The native-call seam's files are not edited.** What a real build would ask of the seam went to the
coordinator before this change (5.9, item 4).

| K readers | Go | scheduler as B2 left it (retake off) | with the retake | C# on threads |
|---|---|---|---|---|
| 8 | 0.1 s | 0.7 s, Go's output, 31 threads | 0.7 s, Go's output, 26 threads, 0 retakes | |
| 40 | 0.1 s | **hangs** (60 s bound); output stops after the first line | 0.7 s, Go's output, 59 threads, 36 retakes | |
| 1,000 | 0.3 s | **hangs** (60 s bound); no output | 3.4 s, Go's output, 1,017 threads, 994 retakes | 0.9 s |

- **The gap is real and it is a hang, not a slow-down**: with 40 natively blocked goroutines on 12 P (more than
  the carriers the injection budget allows), every runnable goroutine waits for ever. Without the retake, hidden
  twins would turn a program that runs today into one that stalls.
- **The retake restores Go's behaviour, not Go's cost.** Each natively blocked goroutine still holds one OS thread
  (1,017 at K 1,000, the thread path's own count). And because the monitor finds the call only by polling, each
  retake waits two ticks: 974 injections, a longest step of 2.7 s, and a wall 3.8 times the thread path's on this
  arm. A hand-off made at the seam's entry (requests 1 and 2 of 5.9, item 4) removes the wait.
- **Reach**: only the Windows syscall funnel is bracketed by the seam today. On linux and darwin `os.File.Read`
  is not, so the retake cannot see it there, and this arm was not run hosted. Coverage of the seam is what M:N
  rests on for native calls.

### 5.6 A park inside an untwinned package, and the same package twinned (`MN_IO_KIND=io`)

`io.Pipe`'s read parks on channels and a mutex inside package `io`. With `io` built as it is today, the fixture's
own twins end at the call into `io`, and every blocked reader holds a thread. Building `io` with generated twins
(`GoTwinsFor` naming both projects) takes one build and no refuse row: 14 of its 54 candidates get twins, the
pipe's `read`, `write`, `Read` and `Write` among them.

| `io` | K | wall | Go's output | peak OS threads | parks | hand-offs |
|---|---|---|---|---|---|---|
| as today | 1,000 | 0.8 s | yes | 1,012 | 10,071 | 9,002 |
| twinned | 8 | 1.2 s | yes | 23 | 10,122 | 2 |
| twinned | 40 | 0.6 s | yes | 24 | 10,296 | 2 |
| twinned | 1,000 | 0.7 s | yes | **35** | 16,083 | 2 |

The fixture's twin report gains one await site in `ioReader` (the `Read` in the other assembly) and one in `Main`
(the `Write`): a twin in a referenced package is found and awaited by B2's rules as they stand. Output is Go's
either way, so a missing twin in a dependency costs threads and never correctness; that is the property that lets
packages gain twins one at a time. After the arm `io` was rebuilt without twins and the 1,012-thread reading came
back.

### 5.7 Census: what an interceptor would have served in the twin copies

An interceptor is keyed on the position of the call it was written for and does not reach the generator's copy.

- **String literals in an object slot**: the twin files of the 19 programs hold **123** such literals in 15
  programs, each given its Go type by the generator's cast (B2). Controls: MnBlockedIO's twins hold 9, the count
  of its `fmt.Println` literals read from the Go source; ChannelReceiveFromClosed (u8 formats only) and
  PanicRecover (no twin) hold 0. None of the 18 compared programs prints differently from Go for a `System.String` handed
  where Go has a `string`.
- **`[GoBoundedGoroutine]` records**: 26 across 11 of the 19 programs. A `go` site the generator routes to a twin
  start does not use the launcher the record intercepts; a site left on the launcher inside a twin copy runs
  without its record. No output differs, and what the record saves was not priced here.

### 5.8 Predictions and verdicts

Every line was written before its run; the dated blocks are in the lane's prediction file.

| id | prediction | reading | verdict |
|---|---|---|---|
| B3-S1 | at least 15 of 19 PASS; all 19: 45% | 17 of 19, then 18 after two generator-side fixes (one of the 18 is not compared with Go, 5.2) | HIT; the 45% line MISS |
| B3-S2 | `Main` twinned in at least 16; at least 3 programs keep an untwinned park; no stdout differs for it | 18; 8 programs above the base line of 1 hand-off; none differs | HIT |
| B3-C1 | at least 30 literals in object slots across at least 12 programs; 0 to 3 bounded-goroutine records | 123 in 15; 26 records in 11 | HIT; the record count MISS (predicted without reading a `package_info.cs`) |
| B3-C2 | 60% that a shape differs from Go for a `System.String` in a twin | none differs; the cast covers these 19 | MISS |
| B3-H1 | PASS on three hosts; at most 60 threads; walls at most 30 / 30 / 90 s | PASS; 29 / 30 / 33 threads; 7.3 / 29.6 / 18.1 s | HIT (osx-arm64 by 0.4 s) |
| B3-N1 | as it stands K 8 passes and K 40 hangs; with a retake K 40 and K 1,000 print Go's output within K + 40 threads | as predicted; 59 and 1,017 threads | HIT |
| B3-X1 | Go's output either way; 60% that the pipe's read and write both get twins and the run stays under 60 threads | both twinned; 35 threads against 1,012 | HIT |

Stop lines: **K1 not fired** (no shape whose twin cannot keep Go's behaviour; the one red token is a registry
number, 5.3). **K4 not fired** (no visible `.cs` changes; both generator changes are in twin text). K3 is
B4's.

### 5.9 Findings

1. **R2 holds over the shapes B0 chose**: 17 of the 18 programs compared with Go print its output and exit code
   under generated twins on the M:N scheduler (the 19th, SelectStatement, is not compared and runs to exit 0), the storm finishes on three hosted hosts where the thread path ran out of memory, and
   the one red is one token with a known cause.
2. **Twins needed two rules B2 did not have**, both outside the visible emission: the spill of an awaited call
   past a live ref struct (rule S), and the mapping of a state machine's frame back to its Go function name.
   Each was found by one program of the battery; a wider corpus will find more of this kind, and each is local to
   the generator or the runtime's frame namer.
3. **Two additions to the consumer-visible list of section 1** (folded into the public-surface plan with the
   final answer):
   - *.NET-side stack frames.* Wherever a twin runs, `Exception.StackTrace`, a debugger's call stack, a profiler
     and a crash dump show `<nameꞏtwin>d__N.MoveNext` and the async method builder's frames where they show the
     Go-named method today. Go-format tracebacks are mapped back (5.3); .NET-format ones cannot be.
   - *Goroutine ids under a main twin.* Until the open item of 5.3 is settled, main's work runs on a goroutine
     whose id is not 1, and a trace names it as the parent of the goroutines main starts.
4. **Native calls need the seam** (posted to the coordinator before the retake was written; nothing changes in
   the seam during the spike). A real build asks: (1) `Enter` passes its hint to the scheduler, so a call marked
   as blocking hands its P off at once; (2) `Exit` tells the scheduler the call is over, so a retaken goroutine
   takes a P or queues before it runs managed code again; (3) coverage is the contract, since an unbracketed
   native call is invisible to the retake and falls back to the capped injection; (4) a callback on a foreign
   thread still has no goroutine identity (the design's gap M7), which matters more under M:N.
5. **The cost lever is how far the twins reach**, not the twin mechanism: the pipe's 1,012 threads are parks
   inside an untwinned `io`, and `io` twinned reads 35; GoroutineParkStorm's 175 are parks in a `go` literal the
   generator does not twin yet. B4 prices the standard library's share (build wall and size) against this.
6. **The slowest hosted reading is 8.7 times Go's wall** (osx-arm64, 100,000 goroutines, 3 P). It is inside the
   bar set before the run and far from Go; B4's per-goroutine cost table says where it goes.

### 5.10 What B3 did not measure

- **The main twin as goroutine 1** (5.3) and the `Goexit`-on-main wait of 3.9: open, priced in the final answer.
- **golib's second name reader** (`GoSyntheticPC.GoNameOf`) under a twin frame: no program here reaches it.
- **Go's own thread count on the native arm**: the Go reference was read for output and wall only.
- **The native arm on linux and darwin**: the seam does not bracket `os.File.Read` there, so there is nothing for
  the retake to see.
- **Native AOT for B3's additions** (rule S, the retake, the frame-name mapping): B2's AOT arm was not repeated.
- **Why 4 of SelectStatement's 15 candidates got no twin**, and what a `[GoBoundedGoroutine]` record saves at a
  site whose twin copy runs without it.
- **Any standard-library package other than `io` under twins at run time**: B2 built four; B3 ran one.

### 5.11 Gates (Windows x64, dotnet SDK 10.0.400 / runtime 10.0.11, go1.26.10; M:N switches unset)

Run on the tree as committed with this section's code (rule S, the retake, the frame names, MnBlockedIO), one
after the other, 20:24 to 20:48 CST.

| gate | reading |
|---|---|
| GenTests | 0 failed, 496 passed of 496 (71 s) |
| GolibTests | 0 failed, 2,534 passed, 45 skipped of 2,579 (414 s) |
| `go test ./...` in `src/go2cs` (the converter suite and the repo guards) | 11 packages ok, 0 failed (968 s) |
| solution integrity (`check-solution-integrity.ps1`) | **FAILED on one row**: `MnIfaceTwin` is not registered in `go2cs.slnx` |

- **The integrity red is B2's, not B3's.** `MnIfaceTwin` is the interface-twin fixture of section 4 (it carries a
  hand-written file and is built by name); B2 did not run this gate, so B3 is the first to read it. MnBlockedIO,
  the project this phase adds, is registered and is not named; the project graph reads 0 cycles across 363
  projects on all four targets. The fixture is left unregistered: the branch never lands, and registering a
  program with a hand file would put it under the suite's golden rules.
- GolibTests read 2,530 passed and 49 skipped in B2. The 4 tests that moved from skipped to passed were not
  identified.
- The repo guards ran once more after this section was placed. That reading is in the phase's DONE post, since
  writing it here would change the file they read.
- The behavioral suite as a whole was not run: the 20 programs of 5.2 ran one by one through the same runner.

## 6. B4: what twins cost (2026-11-11)

Branch `claude/g-mn-spike2-b4`, on B3's `cde9999305`. B4 is the cost table of the plan: what the twin path adds
to a build, to an assembly, to a call that does not park, to start-up, and to the parts of the runtime it touches;
and whether the build-wall cost B2 measured (+91% on `runtime`) can be removed. The coordinator asked for a lean
phase and for the lever on the nested sibling run.

The branch's first commit (`31cdbb875b`) registers `MnIfaceTwin` in `go2cs.slnx`, which closes the integrity red
of 5.11.

### 6.1 What was built

- **Three ways for the twin generator to see its siblings' output** (`TwinGenerator.Siblings.cs`), chosen per
  project by the build property `GoTwinsSiblings`:
  - *nested* (unset; the form of sections 4 and 5): the 14 sibling generators run a second time, inside a
    generator driver the twin generator starts.
  - *memo*: every sibling's file passes through one helper (`AddGeneratedSource`); under this property the helper
    also keeps the text, keyed by the compilation, and the twin generator parses what was kept. A sibling that has
    not run yet is run nested.
  - *last*: the same memo, with the twin work done under a second generator type, `ZTwinGenerator`, whose name
    sorts after every sibling's. Nothing runs twice. The twin files then land in that type's directory.
- **A report line for the control** (always written): `// siblings=<form> sibling-sources=N memo-sources=N
  nested-generators=N`. Under `GoTwinsTiming=true` a second line gives the generator's own stopwatch:
  `// timing sibling-ms=N analysis-ms=N emit-ms=N`. The times stay out of the report otherwise, since they differ
  from build to build.
- **MnCost** (`src/tools/mnspike2/MnCost`): one Go program, converted in place, with six modes picked by
  `MN_COST_MODE`: a call from one function that can park into another that never does; a `sync.Mutex` pair; a
  deferred closure; `io.CopyBuffer` in 512-byte chunks between two user types that do next to nothing; the same
  copy into a SHA-256 hash; and a hold that sleeps, for the idle thread count. Each loop carries a channel
  receive under a condition that is never true, so the generator gives it a twin and the loop pays what a twin
  costs without parking once. `main` calls each loop itself, so the main twin awaits the loop's twin. Each mode
  prints five readings of nanoseconds and allocated bytes per operation; the first is not read (the JIT is still
  tiering).

Both new properties are compiler-visible and are read from the environment as well as from the command line.

### 6.2 The lever on the nested sibling run (`runtime`, Release, rebuild of the project alone, Windows x64, 12 P)

Each run alternates the arms; the first off arm of a run reads 15 to 40 s above the later ones (a cold compiler
server), so the pairs below are read from the second arm on, or as the difference inside one pair.

| form | walls, twins on (s) | off arm beside it (s) | sibling run (s) | analysis (s) | emit (s) | sibling sources | assembly (bytes) |
|---|---|---|---|---|---|---|---|
| off | | 92.5, 98.3, 91.9, 85.8, 92.8, 88.7 | | | | | 3,851,264 |
| nested | 204.2, 184.3 | 92.5, 98.3 | 95.3, 84.2 | 2.7, 2.5 | 0.09, 0.06 | 1,101 | 4,100,096 |
| memo, as first written | 101.1, 96.0 | 92.5, 98.3 | 0.13, 0.13 | 2.1, 2.1 | 0.15, 0.04 | **684** | **4,099,584** |
| memo with the late siblings nested | 209.3, 171.8 | 91.9, 85.8 | 93.5, 73.7 | 3.0, 2.4 | 0.09, 0.06 | 1,101 (684 kept, 1 generator nested) | 4,100,096 |
| last | 119.6, 98.6 | 113.5, 92.8 | 0.49, 0.49 | 2.7, 2.3 | 0.10, 0.06 | 1,101 (all kept) | 4,100,096 |

- **The nested run is the whole added wall.** 84 to 95 s of the 86 to 92 s that twins add are the siblings'
  second run; the twin generator's own analysis is 2.5 to 2.7 s and its emission under 0.1 s.
- **The memo as first written was wrong, and the control said so.** The compiler runs one assembly's generators in
  the order of their full type names. `TypeGenerator` sorts after `TwinGenerator`, so its 417 files were not in
  the memo yet: 684 sources against 1,101. The twin generator then bound against a compilation without them: 648
  calls inside twin bodies and 2,858 outside did not bind, one twin lost an await (`runFinalizers` in
  `mfinal.cs` called `mallocgc` without its twin; 176 await sites against 177), and the assembly came out 512
  bytes smaller. Its wall is no reading.
- **Running only the late sibling nested is correct and gains nothing**: the one late generator is the one that
  costs (below).
- **The last form is correct and removes the cost.** All 1,101 sources come from the memo, no generator runs
  twice, the 18 twin files are byte-identical to the nested form's (the report compared without its `siblings`
  and `timing` lines), and the assembly has the same size. Paired with the off arm just before each: **+6.1 s and
  +5.8 s (+5.4% and +6.2%)**; against the last off arm of the run, +11%. B2's reading for the same project was
  +91%.
- **What a landing form would need**: the twin generator has to run after every generator whose output it reads.
  The spike gets that from a type name; a product would state the order (one pipeline that ends with the twin
  step, or an incremental generator fed by the others' outputs) and would not keep sources in a static table.
  Nothing a consumer sees changes with the form: the twin files and the assembly are the same bytes.

**One generator is most of this project's build, twins or not.** The compiler's own report for a twins-off rebuild
of `runtime` (95 s wall): generators 85.8 s in all, of which `go2cs.TypeGenerator` **80.6 s (93%)**,
`EscapeRegistryGenerator` 2.5 s, `ImplementGenerator` 1.2 s, `ImplicitConvGenerator` 0.7 s, `RecvGenerator` 0.3 s,
every other under 0.1 s. It writes 417 files there, about 190 ms each. This is outside M:N: it is today's product,
and it is why the nested run cost what it did. The spike did not look for the reason.

### 6.3 `net/http` and `strconv`: build wall and assembly size (Release, rebuild of the project alone, Windows x64)

`net/http` was built with hidden interface twins and the last form; its closure (191 projects) was built first
with twins off. The validate-and-refuse loop of 4.3 reached its fixpoint in round 2: round 1 gave 8 compiler
errors (1 CS8178, 7 CS9217, all a `ref` kept across an await), the 8 rows name 4 functions and the twin
count goes from 262 to 255, round 2 built clean.

| project | arm | wall (s) | generator stopwatch: siblings / analysis / emit (s) | assembly (bytes) | twins |
|---|---|---|---|---|---|
| `net/http` | off | 42.6, 42.9 | | 2,728,448 | |
| `net/http` | last | 45.7 | 0.44 / 1.42 / 0.06 | 3,119,616 | 255 of 1,209 candidates, 504 await sites, 11 files |
| `net/http` | nested | 75.0 | 29.4 / 1.55 / 0.06 | 3,119,616 | the same 11 files, byte for byte |
| `strconv` | off | 2.53, 2.44 (the first, cold: 2.97) | | 349,184 | |
| `strconv` | last | 2.57, 2.70 | 0.001 / 0.03 / 0 | 349,184 | 0 of 57 candidates; the report file only |

- **`net/http` pays +3.0 s (+7%) in the last form and +32.2 s (+76%) nested.** The nested run's 29.4 s is again
  the siblings' whole second run: the compiler's report for the off build gives the generators 30.0 s of the
  42.6 s, `TypeGenerator` 13.6 s and `ImplementGenerator` 12.9 s of it.
- **The assembly grows by 391,168 bytes (+14.3%)** for 255 twins, their state machines, and the hidden interface
  twins (12 interface members twinned, 24 forwarders).
- **What the generator refused in `net/http`**: 359 candidates for a by-ref receiver (32 of them reach a park), 27
  for a by-ref parameter (4), 2 for a ref-like type, and the 4 functions the build rows named. The 39 that reach a
  park and get no twin are the cases where a goroutine would hold its carrier; B5 counts them across the corpus.
- **`strconv` is the control: nothing in it can park.** The generator finds no twin and writes its report and
  nothing else; the assembly has the same size; the rebuild reads +0.04 and +0.25 s beside the off arm before it,
  which is inside that arm's own spread. A package without a park pays the analysis (26 ms) and nothing more.

### 6.4 What a twin costs on a path that never parks (MnCost, JIT, Release, Windows x64, 12 P)

Each cell is the median of 8 readings (2 processes, repetitions 1 to 4; repetition 0 is not read), with the
range where it is wider than 5%. "Plain" is the build without twins, which is today's product; the twins build
with the scheduler off reads the same as plain in every mode (1.88, 230, 25.0, 19.5 ns), so one column stands
for both. Go's own build of the same source is beside it for scale.

| shape, per operation | Go | plain | twins, `GO2CS_MN=1` | twin adds |
|---|---|---|---|---|
| a call into a function that can park and does not | 1.7 ns, 0 B | 1.88 ns, 0 B | 7.88 ns, 0 B | **+6.0 ns** |
| `sync.Mutex` `Lock` and `Unlock`, uncontended | 3.8 ns, 0 B | 230 ns, 64 B | 27.9 ns, 0 B | **−202 ns, −64 B** |
| a call into a function that defers a closure | 4.6 ns, 0 B | 24.7 ns, 88 B | 43.8 ns (36.4 to 47.7), 88 B | **+19 ns**, no allocation added |
| `io.CopyBuffer`, one 512-byte chunk, reader and writer that do next to nothing (io twinned) | 6.2 to 7.9 ns | 20.0 ns, 0 B | 58.7 to 59.9 ns, 0 B | **+39 ns, 3.0x** |
| the same copy into SHA-256 (io twinned) | 254 ns | 4,429 ns | 4,471 ns | **+42 ns, 1.01x** |

The control for every row is the JIT's own list of compiled methods (`DOTNET_JitDisasmSummary`): in the twins
build the mode's twin is compiled under `GO2CS_MN=1` (4 to 5 methods with the state machine) and no twin of it
is compiled with the scheduler off; the plain build compiles none.

- **A twin call that completes without parking costs 6 ns** over the plain call: a `ValueTask` built, returned
  and awaited, with no allocation. This is the price of every call from one twin into another.
- **The mutex row is not a cost; it shows a slow path in today's `Lock`.** With profiling off the plain `Lock`
  enters the park bracket before it tries the lock (`using (Goroutine.Park(...)) { gate.Wait(); }`), which is
  where the 200 ns and the 64 bytes go. The hand twin of B1 tries the free lock first (`gate.Wait(0)`) and only
  then waits. The same first step in the plain `Lock` would give today's product the 28 ns reading with no M:N
  at all. Routed to the coordinator; not changed here.
- **A defer inside a twin adds 13 ns beyond the call's 6**, and no allocation. The 88 bytes a deferred closure
  costs today are the same on both paths (Go: 0).
- **An interface call made inside a twin costs about 20 ns when the implementer has no twin.** The hidden twin
  interface of 4.11 tests the receiver for the twin interface; a type that never parks does not implement it, so
  the call falls to the plain method and its result is wrapped in a `ValueTask` and awaited. The copy loop makes
  two such calls a chunk. The case where the implementer has a twin was not timed alone.
- **K3 ("a real program over 2x slower on its non-parking path").** The floor kernel reads 3.0x: its chunk does
  20 ns of work and the twin path adds 39. It is the worst a copy loop can be, and it is not a program: with a
  hash behind the same loop the adder is the same 42 ns and the ratio is 1.01x. A loop goes over 2x only where
  the work between two interface calls made in a twin is under about 20 ns. **I read K3 as not fired, and the
  floor reading as the number to carry**: 6 ns a twin call, 20 ns an interface call from a twin. The ruling is
  the coordinator's.
- **The first run of this table timed nothing.** Its `main` passed each loop to a timing helper as a func value;
  a func value holds the function's plain body (the generator twins no func literal), so every arm ran plain
  code. The reading said so before the code did: the twins row equalled the plain row to 0.1 ns. The second form
  calls the loops from `main` itself and carries the control above. The consequence for a consumer is in 6.9.

### 6.5 Start-up and idle threads (MnCost, twins build, JIT, Windows x64)

The program with no mode prints one line and ends; 20 processes with the scheduler off and 20 with it on,
alternating. The hold mode sleeps 3 s in `main` and is sampled for its thread count.

| reading | scheduler off | scheduler on | difference |
|---|---|---|---|
| process wall, median of 20 (ms) | 601.4 | 617.7 | +16 |
| process wall, minimum of 20 (ms) | 585.9 | 597.0 | +11 |
| paired difference, median of 20 pairs (ms) | | | +12 (range −51 to +178) |
| peak threads while `main` sleeps | 15 | 28 | +13 |
| peak private memory while `main` sleeps (MB) | 23 | 23 | 0 |

- **Start-up costs about 11 to 16 ms, on a 600 ms process.** That is the scheduler's 12 carriers and its monitor
  starting, and the main twin's first mount. The spread between pairs is wider than the difference, so the sign is
  safe and the size is good to a few milliseconds. The 600 ms is today's start of a converted program under the
  JIT and is not the spike's.
- **The scheduler holds 13 more threads at rest**: one carrier per P (12 here) and the monitor. They are parked,
  and they replace the thread a goroutine would otherwise hold, so the count stops growing where the thread path
  keeps adding one per parked goroutine (29 to 33 threads for 100,000 goroutines in 5.4).

### 6.6 The rows read from the tree (no run)

Line counts are `git diff --shortstat` of B3's tip against the branch point on master (`742a3dbb24`); B4's own
additions are the lever of 6.1 and the cost fixture.

| part | what the spike changed | reading |
|---|---|---|
| converter (`src/go2cs`) | nothing | 0 files, 0 lines. No emitted `.cs` and no golden differs for twins; K4 does not fire |
| golib | the scheduler, the goroutine's mounted state, the frame's mounted half, the parking twins of the channel and builtin operations | 10 files, +1,242 −9 |
| go2cs-gen | the twin generator | 8 files, +1,675 −1 (before B4's lever) |
| hand-owned standard library | `runtime` (the hand-off around a blocking stub, the main twin's wait), `sync` (the `Mutex.Lock` twin), `time` (the `Sleep` twin) | 4 files, +99 −2 |
| fixtures and tools | `src/tools/mnspike2`, the Behavioral programs | 7 files +831; 27 files +2,231 |

- **defer, recover and the frame model.** A twin keeps the converted body's `defer` and `recover` calls as they
  are written; what changes is where the frame's state lives while the goroutine is not on a thread. The spike
  moves it at mount and unmount (38.4 to 38.6 ns for the pair, 3.6); the cost inside a function that does not
  park is the defer row of 6.4. No emitted call shape changes.
- **The native-call seam.** `GoNativeCall.cs` is byte-identical to master's on this branch: the spike adds no
  call into the seam and changes none. The scheduler reads the seam's depth counter in one place (the monitor of
  5.5, which takes a carrier back from a goroutine that sits in a native call). Where a call is not bracketed
  (linux and darwin `os.File.Read` today, 5.10) the scheduler cannot see it; that costs a carrier for the length
  of the call, not correctness.
- **cgo.** The corpus is emitted with `CGO_ENABLED=0` on every platform, so no converted package holds a cgo
  call today and M:N adds nothing there. A cgo call, if the corpus ever carries one, is a native call and would
  ride the same seam: bracketed, it gives its carrier back; unbracketed, it holds one.
- **Thread-static state (B4-T1).** The census in GolibTests lists 34 fields, 22 of them in golib. The spike moves
  8 golib fields with the goroutine at mount. 2 more golib fields are goroutine-scoped and are not moved (the
  allocation tally and the `Gosched` backoff streak), so 10 of 22 is the count a landing form would carry
  (predicted 8 to 14). **Outside golib, 7 of 12 fields are goroutine-scoped and the spike does not move them**:
  `getg`'s cache and its owner, the held runtime locks and their count, the pending lock-profile flag,
  `debug.SetPanicOnFault`, and the sleep timer. A goroutine that parks in a twin and resumes on another carrier
  reads the other carrier's value for these. None of the 20 programs of section 5 reaches one across a park; a
  landing form moves them or keys them by goroutine. `sync.Pool`'s shard id is per P in Go and stays with the
  carrier.

### 6.7 The storm under Native AOT and JIT, memory per goroutine, and the ping at one P (Windows x64, 12 P)

MnMixedStorm with generated twins and `GO2CS_MN=1`, 100,000 goroutines, hold 3 s; the two arms alternate, 5 runs
each. All ten runs print the output Go prints.

| arm | walls (s) | median (s) | peak threads | peak private memory (MB) |
|---|---|---|---|---|
| Native AOT (B2's image, built before B3's rule S and retake) | 7.8, 7.5, 8.1, 7.5, 7.2 | 7.5 | 33 to 38 | 628 to 633 |
| JIT (B3's final build) | 7.3, 7.2, 6.8, 7.0, 7.3 | 7.2 | 36 to 40 | 612 to 618 |
| Go's own build | 3.8, 3.8 | 3.8 | 14 to 15 | 883, 889 |

| memory per goroutine | peak private at 10,000 (MB) | at 100,000 (MB) | per goroutine |
|---|---|---|---|
| Go | 101, 101 | 883, 889 | 8.9 KB |
| twins, JIT | 76, 83 | 612 to 618 | 6.1 KB |

| ping, `GO2CS_MN=1`, 200,000 round trips (hand-twin fixture of B1) | ns a round trip | steps on the run-next slot |
|---|---|---|
| 12 P, before | 1,397, 1,445, 1,399 | 396,899 to 396,900 of 400,002 |
| `GO2CS_MN_PROCS=1` | 1,710, 1,705, 2,115 | 396,899 to 396,900 of 400,002 |
| 12 P, after | 1,508, 1,397, 1,427 | 396,900 of 400,002 |

- **Native AOT reads 0.3 s behind the JIT on this program** and holds the same threads and memory. The image is
  B2's, so the pair differs in vintage as well as in compiler; a twin program publishes and runs under Native AOT
  at the size B2 recorded, and this section adds only that it is not slower in kind.
- **A parked goroutine costs 6.1 KB under twins and 8.9 KB in Go.** Go's figure is its 8 KB starting stack on
  Windows; the twin's is the state machines of the frames that are live at the park, the goroutine record, and
  the channel waiter. This is the whole of finding 13 of section 3.
- **The ping does not get faster on one P; it gets slower by about 300 ns.** The run-next counter says why: in
  both arms 396,900 of the 400,002 steps took the run-next slot, so at 12 P the round trip already stays on one
  carrier and there is no wake across carriers to remove. The 700 ns of a step is the step itself (park, ready,
  resume). Go reads 425 to 432 ns for the round trip (3.6), and today's thread path 1,677 to 1,715. Where inside
  the step the time goes was not measured.

### 6.8 Predictions and verdicts

Every line was written before its run; the dated blocks are in the lane's prediction file. P5 is the plan's cost
bar.

| id | prediction | reading | verdict |
|---|---|---|---|
| B4-G1 | of the wall twins add to `runtime`, the nested sibling run is 40 to 60% and the twin generator's own analysis 25 to 45% | the nested run is all of it (84 to 95 s of 86 to 92 s); analysis 2.5 to 2.7 s | MISS, both halves |
| B4-G2 | the lever brings `runtime` from +91% to +35..55%; 50% inside P5's +50% | +5.4% and +6.2% (last form) | MISS on the good side; inside P5 |
| hybrid (written 21:29) | the memo with the late siblings nested: the right twins, wall +15..40% | the right twins; +100% and +128% (the one late generator is the one that costs) | controls HIT, cost MISS |
| last form (written 21:42) | the right twins at +3..11% | byte-identical twin files; +5.4% and +6.2% | HIT |
| B4-G3 | `net/http`: generator time +40..80% before the lever, +20..45% after; assembly +5..15% | +76%; +7%; +14.3% | HIT; MISS on the good side; HIT |
| B4-G3 | `strconv`: 0 twins, +0 bytes; wall +5..25% after the lever | 0 of 57; +0 bytes; +0.04 and +0.25 s, inside the off arm's spread | HIT; HIT; the wall line is not resolved |
| B4-C1 | a twin call that completes synchronously: +5..25 ns | +6.0 ns, 0 B | HIT |
| B4-C2 | `Mutex` pair in a twin, `sync` as built: +0..3 ns | −202 ns, −64 B (6.4: the plain `Lock`'s slow path) | MISS on the good side |
| B4-C3 | defer in a twin: +20..80 ns; 55% no added allocation | +19 ns; none added | MISS by 1 ns on the good side; HIT |
| B4-K3 | the copy kernel through twinned `io`: at most 1.3 times | 3.0 times on the floor kernel; 1.01 times with a hash behind it | MISS on the bad side (kernel); the hashed stream's own line (at most 1.05) HIT |
| B4-S1 | start-up +2..10 ms; the scheduler adds 9 to 13 threads | +11 to +16 ms; +13 | MISS, high by 1 to 6 ms; HIT |
| B4-A1 | AOT median 7.0 to 8.2 s, JIT 6.3 to 7.5 s; 70% that AOT is slower by 0.3 to 1.2 s | 7.5; 7.2; 0.3 | HIT (the difference at the low edge) |
| B4-M1 | per goroutine: Go 8.0 to 9.0 KB, twins 5 to 7 KB | 8.9; 6.1 | HIT |
| B4-P1 | the ping at one P reads 150 to 400 ns | 1,705 to 2,115 ns, above the 12 P reading | MISS, in the other direction |
| B4-T1 | 8 to 14 of golib's thread-static fields belong to the goroutine | 10 of 22 | HIT |

| P5 line | reading | verdict |
|---|---|---|
| a twin call completing synchronously +5..25 ns | +6.0 ns | HIT |
| uncontended `Mutex.Lock` in a twin at most +30 ns | faster than the plain form | HIT |
| defer in a twin +40..80 ns and 1 allocation | +19 ns, none | MISS on the good side |
| start-up at most +5 ms and +3 threads | +11 to +16 ms; +13 threads | MISS, both (the thread line was called a miss beforehand) |
| IL at most +35% on `net/http`, 0 on `strconv` | +14.3%; 0 | HIT |
| generator time at most +50% on `net/http` | nested +76%; last +7% | nested MISS; last HIT |
| 8 to 14 of golib's thread-static fields move to the goroutine | 10 of 22 | HIT |

Stop lines: **K3 read, and in my reading not fired** (6.4: no program reads over 2 times; the floor kernel of a
copy loop does, and the number it gives is carried). The ruling is the coordinator's. **K4 not fired** (the
converter is untouched, 6.6). K1 and K2 were settled in sections 4 and 5.

### 6.9 Findings

1. **The build-wall cost of B2 is gone in the last form**: `runtime` +5.4% and +6.2% where B2 read +91%,
   `net/http` +7% where the nested form reads +76%, a package without a park +0 within the spread. The twin files
   and the assembly are the same bytes in every form. A landing form states the generator order instead of
   getting it from a type name.
2. **What a twin costs where nothing parks**: 6 ns a call from one twin into another, about 20 ns an interface
   call made in a twin on an implementer that has no twin, 13 ns more for a defer, no allocation in any of them.
   A loop pays over 2 times only where the work between two such interface calls is under about 20 ns.
3. **A func value holds the plain body, so a park behind one holds its carrier.** The generator twins named
   functions and methods; a func literal and any function passed as a value run the plain body
   under either build. A park reached that way inside a twin is the untwinned-dependency case of 5.6: Go's
   output, and a carrier held for the length of the park. `net/http`'s report counts 26 parks inside func
   literals and 69 calls through a func value in its twins, and a handler registered as a func value is the
   common shape of that package. This is the widest gap in the generator's reach, wider than the by-ref
   refusals of 6.3; what closing it asks of a func value's representation belongs to the final answer.
4. **The assembly grows by 14% on the heaviest parking package measured** and by nothing on a package without a
   park.
5. **Start-up costs 11 to 16 ms and 13 threads at rest** on 12 P (one carrier per P and the monitor).
6. **A parked goroutine is 6.1 KB against Go's 8.9 KB**, and Native AOT runs the storm within 0.3 s of the JIT.
7. **The park step is 700 ns and is not the wake between carriers** (6.7). Go's round trip is about a third of
   the twin's. For a program that parks all the time this, and not the twin call, is the cost that matters; it
   is the scheduler's, and the spike did not profile it.
8. **Seven thread-static fields outside golib are goroutine-scoped and are not moved** (6.6). A landing form
   moves them or keys them by goroutine.
9. **Four readings are about today's product and not about M:N** (routed to the coordinator, nothing changed
   here): `TypeGenerator` is 93% of the generator time on `runtime` and, with `ImplementGenerator`, 88% on
   `net/http`; the plain `sync.Mutex.Lock` has no try-first step and reads 230 ns and 64 bytes where the twin
   reads 28 ns and none (Go: 3.8 ns); a deferred closure costs 24.7 ns and 88 bytes (Go: 4.6 ns, none); the
   converted SHA-256 reads 4,430 ns for 512 bytes where Go, with the CPU's instructions, reads 254.

### 6.10 What B4 did not measure

- **An interface call in a twin whose implementer has a twin**: not timed alone (6.4 times the fallback).
- **The cost table on linux and darwin, and under Native AOT**: the micro rows are Windows x64 under the JIT;
  Native AOT was read for the storm only, on B2's image.
- **Where the 700 ns of a park step goes**, and any contended path (a contended `Mutex`, a select with several
  ready cases).
- **`net/http` with its dependencies twinned, and at run time**: its 191 dependencies were built with twins
  off and the package was built, not run. The prediction named `sync` and `io` twinned beside it.
- **The whole standard library's build wall under the last form**: three projects were timed.
- **The generator inside an editor**, where it runs as the source changes; only command-line rebuilds were
  timed.
- **Start-up under Native AOT.**
- **Why `TypeGenerator` costs about 190 ms a file.**

### 6.11 Gates (Windows x64, dotnet SDK 10.0.400 / runtime 10.0.11, go1.26.10; M:N switches unset)

Run on the tree as committed with this section's code (`e41abb2081`: the sibling forms and MnCost), one after the
other, 22:31 to 23:02 CST.

| gate | reading |
|---|---|
| GenTests | 0 failed, 496 passed of 496 (75 s) |
| GolibTests | 0 failed, 2,534 passed, 45 skipped of 2,579 (688 s) |
| `go test ./...` in `src/go2cs` (the converter suite and the repo guards) | 11 packages ok, 0 failed (1,073 s) |
| solution integrity (`check-solution-integrity.ps1`) | OK: all 1,034 behavioral projects are registered in `go2cs.slnx` |

- **The integrity gate is green**: the branch's first commit registers `MnIfaceTwin`, the one row 5.11 read red.
- GenTests instantiate every generator type, `ZTwinGenerator` among them; it returns at once in a project
  without `GoTwins`.
- The repo guards ran once more after this section was placed. That reading is in the phase's DONE post, since
  writing it here would change the file they read.
- The behavioral suite as a whole was not run; no file under it changes in this phase.

## 7. B5: how much of a real program can park (2026-11-11)

Branch `claude/g-mn-spike2-b5`, on B4's `fcdcdf76fc`. B5 is the plan's closure question: on a large real program,
what share of the functions would the twin rule mark, and what does the generator refuse. The program is the
TypeScript compiler's Go port, the one the fleet already converts. The coordinator asked for a lean phase: measure
what the verdict needs, no more. So B5 is a census of Go source and adds no code to the tree; the run under twins
that the plan allowed "if coverage allows" was not attempted (7.12 says why).

The branch's first commit (`85989b9947`) is not B5's measurement: it writes the three product findings of 6.9,
item 9, as BOARD rows, as the coordinator routed them.

### 7.1 What was read, and with what

- **The program.** `microsoft/TypeScript` at commit `ba29a56299`, module `github.com/microsoft/TypeScript/tsc`,
  with the TS session's `cmd/checkdemo` entry (parse, bind and type-check a project). `go list -deps` gives its
  closure as 200 packages. The census counts named functions and methods with bodies (no closures, no synthetic
  wrappers) outside the runtime family: **16,923 functions in 173 packages**, of which the TS module holds 9,395
  in 48 packages, third-party modules 1,824 in 18, and the standard library 5,704 in 107. `GOOS=windows`,
  go1.26.10.
- **The tool.** Phase 1's `canpark` (`src/tools/mnspike/canpark` at `b328b55d08`): go/ssa and a VTA call graph;
  a function can park when it holds a park point of Go's (a channel operation, `select`, `time.Sleep`, a
  `WaitGroup` or `Cond` wait, the poller's wait, and, unless `-long-only`, the contention path of a `Mutex`,
  `RWMutex` or the file-descriptor lock) or calls a function that can. The runtime family neither parks nor
  propagates.
- **Two flags added for B5**, 31 added lines, kept in the lane's scratch and on no branch. `-no-func-values`: an edge
  whose call site has no static callee and is not an interface invoke does not propagate (in the marking loop and
  in the witness search). `-cut a,b`: a function named in the list is never marked, so no caller is marked
  through it.
- **Three reaches.** *Upper*: every VTA edge (static calls, interface invokes, calls of a func value). *Middle*:
  static calls and interface invokes, which is the reach of the generator of section 4, where a func value holds
  the plain body (6.9, item 3). *Lower*: static calls only.
- **What the middle arm is and is not.** It is an estimate of the generator's marking, not a bound on it. VTA
  resolves an interface call to the implementers that can reach that site; the generator twins an interface member
  when its own project holds a parking implementer (4.4, 4.9 item 12), which is coarser inside a project and blind
  across projects. And the census reads Go's park points, the leaves a complete twin set would have; the spike's
  own leaves are fewer (channels, `select`, `time.Sleep`, `Mutex.Lock`, 3.1), so the generator as built would
  mark less than any arm here.
- **Site tallies** (closures, `go` statements, interface and func-value call sites) are printed by the tool for
  the whole closure and are not split by module.

### 7.2 The arms

Can-park functions and their share of the functions counted, by reach, leaf set and cut.

| arm | whole closure (16,923) | TS module (9,395) | third-party (1,824) | standard library (5,704) |
|---|---|---|---|---|
| upper, long parks only | 7,106 (42.0%) | 5,096 (54.2%) | 1,023 (56.1%) | 987 (17.3%) |
| upper, lock leaves too | 7,182 (42.4%) | 5,126 (54.6%) | 1,024 (56.1%) | 1,032 (18.1%) |
| upper, long only, the two `fmt` cuts of 7.4 | 6,099 (36.0%) | 4,320 (46.0%) | | |
| **middle, long only** | 4,867 (28.8%) | **3,981 (42.4%)** | 216 (11.8%) | 670 (11.7%) |
| middle, lock leaves too | 5,042 (29.8%) | 4,075 (43.4%) | 217 (11.9%) | 750 (13.1%) |
| middle, long only, cut 1 | 3,264 (19.3%) | 2,579 (27.5%) | | |
| **middle, long only, cuts 1 and 2** | 809 (4.8%) | **181 (1.9%)** | | |
| middle, lock leaves too, cuts 1 and 2 | 3,332 (19.7%) | 2,598 (27.7%) | | |
| lower, long only | 311 (1.8%) | 11 (0.1%) | 39 (2.1%) | 261 (4.6%) |
| lower, lock leaves too | 2,652 (15.7%) | 2,229 (23.7%) | 40 (2.2%) | 383 (6.7%) |
| lower, lock leaves too, `(*ast.Node).EagerJSDoc` cut | 2,444 (14.4%) | 2,021 (21.5%) | | |

- **Over half of the TS module can park by the upper rule (54.2%)**, and 42.4% in the generator's reach.
- **Three edges carry almost all of it.** With the lock leaves dropped and two `fmt` edges cut, the generator's
  reach marks 181 functions, 1.9% of the module. The lock leaves alone put 23.7% of the module on a path of
  static calls; cut 1 takes 15 points off the middle arm and cut 2 the next 26.
- **The functions that park on a path of static calls to a long park are 11** (0.1%): the five direct parkers of
  7.6 and six callers in `core` and `ipc`.
- The func-value edges are worth 11.8 points on the TS module (54.2 against 42.4) and 44 points on the
  third-party modules.

### 7.3 The TS module by package

Can-park functions per package (`internal/` dropped from the names); 44 of the 48 packages have at least one in
the upper arm. Columns: upper long-only; middle long-only; middle with lock leaves; middle long-only with cut 1;
with cuts 1 and 2; cuts 1 and 2 with lock leaves; lower with lock leaves; lower long-only.

| package | functions | upper | middle | middle, locks | cut 1 | cuts 1, 2 | cuts 1, 2, locks | lower, locks | lower |
|---|---|---|---|---|---|---|---|---|---|
| `checker` | 2,881 | 2,076 | 1,881 | 1,883 | 1,782 | 1 | 1,759 | 1,729 | 0 |
| `ast` | 2,226 | 616 | 197 | 206 | 2 | 2 | 12 | 10 | 0 |
| `printer` | 768 | 454 | 344 | 401 | 0 | 0 | 307 | 279 | 0 |
| `parser` | 514 | 429 | 377 | 377 | 343 | 0 | 3 | 3 | 0 |
| `transformers/estransforms` | 406 | 318 | 276 | 276 | 0 | 0 | 0 | 0 | 0 |
| `compiler` | 339 | 174 | 121 | 124 | 49 | 45 | 63 | 15 | 0 |
| `binder` | 195 | 121 | 90 | 90 | 76 | 0 | 73 | 73 | 0 |
| `transformers/declarations` | 180 | 118 | 90 | 90 | 77 | 0 | 78 | 47 | 0 |
| `transformers/tstransforms` | 141 | 110 | 98 | 98 | 31 | 0 | 31 | 0 | 0 |
| `transformers/moduletransforms` | 124 | 102 | 81 | 81 | 45 | 0 | 45 | 0 | 0 |
| `tsoptions` | 192 | 87 | 58 | 58 | 33 | 23 | 28 | 11 | 0 |
| `scanner` | 157 | 68 | 38 | 46 | 9 | 0 | 11 | 3 | 0 |
| `contentmapper` | 78 | 55 | 45 | 46 | 27 | 25 | 29 | 13 | 0 |
| `module` | 105 | 54 | 44 | 44 | 26 | 26 | 36 | 0 | 0 |
| `ipc` | 44 | 20 | 20 | 20 | 15 | 15 | 15 | 4 | 4 |
| `core` | 166 | 12 | 8 | 8 | 8 | 8 | 8 | 7 | 7 |
| the other 32 | 879 | 282 | 213 | 227 | 56 | 36 | 100 | 35 | 0 |
| total | 9,395 | 5,096 | 3,981 | 4,075 | 2,579 | 181 | 2,598 | 2,229 | 11 |

- **The checker is 65% marked in the generator's reach (1,881 of 2,881) and the parser 73%.** These are the hot
  paths of a compiler. In a run their functions park on a contended `SourceFile` lock or a blocked output pipe,
  and nowhere else.
- **With the three edges out, one function of the checker is left** (`NewChecker`) and none of the parser,
  binder, scanner or printer. What stays marked is the program driver (`compiler`: `BindSourceFiles`, `Emit`,
  `ForEachCheckerParallel` and the like), the module resolver, the configuration and response-file readers
  (`tsoptions`), the file layer (`vfs`, `bundled`, `contentmapper`'s host), `ipc`, and `core`'s parallel helpers.
- Of the 3,981 functions of the middle arm, 3,586 are pointer-receiver methods, 9 value-receiver methods and 386
  plain functions; of the 181, 121, 2 and 58.

### 7.4 The three edges

Each was named by the tool's shortest-path witness (`-why`), read on the checker's, the parser's and the binder's
entry functions.

1. **Locks on static paths.** `(*checker.Checker).checkSourceFile` reaches `(*ast.Node).EagerJSDoc` through
   `checkSourceElements`, `checkSourceElement` and `checkSourceElementWorker`, all static calls, and that function
   takes `file.jsdocMu.RLock()` when the file has lazy JSDoc (in `internal/ast/ast.go`). `getTypeOfSymbol` reaches the same lock through the checker's `error` method and the scanner's
   error-range helper. It is not one lock: with `EagerJSDoc` cut the lower arm still reads 2,021 for the module
   (1,644 in the checker), and the next witnesses are `(*ast.SourceFile).resolveJSDoc` under the scanner's
   token-position helper and `(*ast.SourceFile).ECMALineMap` under the printer's comment writer. `ast.SourceFile`
   keeps its lazily built tables behind several locks, and the checker and the printer read them everywhere: 60% of
   the checker (1,729 functions) is on a path of static calls to one of them. The parser is not:
   `parseStatement` cannot park in the lower arm.
2. **`fmt`'s method hook (cut 1).** `debug.Assert` calls `fmt.Sprint` on its slow path;
   `(*fmt.pp).handleMethods` calls `Error`, `String`, `Format` or `GoString` on the operand through an
   interface; a `String` method in the closure (the witness is `glob.group`'s, through `(*glob.Glob).String`)
   calls `fmt.Fprint`, which writes to an `io.Writer`; one `io.Writer` is `*os.File`; and its `Write` ends in
   the poller's wait. The parser and the binder reach the same hub through `(*ast.Node).Text`, which calls
   `fmt.Sprintf`. So every function that formats a value is marked, because some `String` method somewhere
   prints.
3. **`fmt`'s rune reader under `math/big` (cut 2).** `(*scanner.Scanner).Scan` reads a BigInt literal with
   `(*big.Int).SetString`; `math/big` scans through an `io.ByteScanner`; one implementer is its own
   `byteReader`, the adapter over `fmt.ScanState` that `big.Int`'s `Scan` method uses; that calls
   `(*fmt.ss).ReadRune` and `(*fmt.readRune).ReadRune`, which read an `io.Reader`, one of which is `*os.File`.
   So the scanner, and with it the parser and the checker, is marked through an adapter that `SetString`
   never builds (it scans a `strings.Reader`). After cut 1, `fmt.Sprintf` cannot park and `fmt.Fprintf` can
   (through the writer), which is the right pair.

In the upper arm two more merges show: a generic helper that calls its callback (`core.Find` reaches every
callback handed to it anywhere), and `(*sync.Once).doSlow`, which reaches every function ever passed to a `Once`,
the checker pool's parallel `createCheckers` among them. Those are func-value edges and are outside the
generator's reach.

**The poller's wait has no twin in the spike.** Edges 2 and 3 end in it. On Windows a blocking file or console
write is a native call that the seam brackets (5.10); the monitor takes the carrier back (5.5), and nothing
awaits. So the long park at the end of edges 2 and 3 is not a leaf for the generator as built. Whether it marks
the same paths through a lock on the way (`(*poll.FD).Write` takes a `Mutex` for a file) was not read. A
landing form that twins the poller, which is what the thread cost of 3.5 asks of sockets, meets both edges in
full.

### 7.5 The cut list: a requirement of R2 on a program of this size

Without a cut, the twin rule marks the hot path of the program for parks that do not happen. The measured price
of a marked function that never parks is small per call (6 ns from one twin into another, about 20 ns for an
interface call on an implementer without a twin, no allocation, 6.4) and real in size (the assembly of
`net/http` grows 14.3% with 21% of its candidates twinned, 6.3); neither was read on this program. A cut removes
the marking at its source. It is a declaration that an edge is not a park for the generator's purposes:

| cut | what it declares | what it costs when the edge does park |
|---|---|---|
| lock leaves (`Mutex`, `RWMutex`, the file-descriptor lock) | a lock is not a twin leaf: a caller of `Lock` is not marked for that call | a goroutine that finds the lock held blocks its thread and hands its P off, as any untwinned blocking call does (3.5); one thread per goroutine blocked on a lock at the same instant |
| `(*fmt.pp).handleMethods` | `fmt` calling an operand's `Error`, `String`, `Format` or `GoString` is not a park | a `String` method that parks (it sends on a channel, or prints to a pipe that is full) runs its plain body and holds its carrier for that long: the untwinned-dependency case of 5.6, Go's output, one more thread |
| `(*fmt.ss).ReadRune`, `(*fmt.readRune).ReadRune` | `fmt`'s scanner reading its source is not a park | `fmt.Fscan` on a reader that parks (a pipe, a socket, standard input) holds its carrier while it waits |

- **A cut is safe by construction.** Behind a cut the plain body runs. The program prints what Go prints; the
  cost is a held carrier while the edge is blocked, and the monitor's retake (5.5) covers the starvation case.
- **A cut is not consumer-visible.** It changes no signature and no emitted file outside the twins; its one
  observable effect is the thread count of a program that does park behind it.
- **The form of the declaration was not designed.** The two `fmt` cuts are functions of the standard library and
  could be refused through the refuse file of 4.6 as it stands (a row inside a function refuses it, and a
  refused candidate has no twin for a caller to await); that was not tried. The lock cut is a
  choice of leaf set in golib. A consumer's module needs a way to add its own; the three found here took one
  witness each to name, and the tool's `-cut` arm with a negative control (7.8) is the method.
- **Contended locks behind the cut were not measured**, and the lock cut is the one with a real cost: a program
  that parks many goroutines on one mutex holds a thread for each. Go treats a contended mutex as a park. A
  sharper rule than a cut (await a lock's twin only in a function that is a twin for another reason) keeps
  6.4's try-first path in twins and adds nothing to the marking. It was not built.

### 7.6 Park points, `go` statements, closures and func values

| reading | count |
|---|---|
| functions that hold a long park point, whole closure | 45 (channel receive 31, send 21, `select` 20, the poller's wait 2, `time.Sleep` 2, `Cond` 1, `WaitGroup` 1; a function can hold several) |
| the same with lock leaves | 49 (2 on a `sync` lock, 2 on the file-descriptor lock) |
| of them in the TS module | 5: `(*ipc.AsyncConn).Call`, `.handleResponse`, `.recordRequestError`, `(*core.LimitedSemaphore).Acquire`, `.TryAcquire` |

| whole closure, sites that can reach a park | of | upper | middle | middle, cut 1 | middle, cuts 1 and 2 | lower |
|---|---|---|---|---|---|---|
| closures (func literals) | 1,466 | 670 | 499 | 386 | 127 | 38 |
| `go` statements | 29 | 26 | 25 | 24 | 24 | 24 |
| interface call sites | 2,453 | 597 | 426 | 229 | 110 | 67 |
| func-value call sites | 2,197 | 958 | 666 | 419 | 124 | 12 |

(All long-only. VTA leaves 239 interface sites and 302 func-value sites without a callee.)

- **The long parks of the whole closure sit in 45 functions**, and all but six at most are channel operations and
  `select`, the leaves the spike twinned first (3.1). The two `sync` waits and the poller's wait are the leaf
  kinds it did not twin by hand. This is the count 3.5 asked B5 for: the calls that park for long are few.
- **The func-value gap of 6.9 on this program**: with the cuts, 127 of 1,466 func literals can reach a park and
  124 of 2,197 calls through a func value can call a parker. The TS module's own parallelism is this shape: its
  work items are func literals handed to `core`'s work group, which starts each through `sync.WaitGroup.Go`.
  The `go` statement is in `sync`, on a func value. A `go` on a literal is twinned at the site (4.1); a literal
  handed to a helper and started there runs the plain body.
- At least 24 of the 29 `go` statements start something that can park, in every arm.

### 7.7 The exported names, for the list of section 1

Every package of the TS module is under `internal/`, so the module has no surface a consumer can import; the
counts below are names exported across its own packages (an exported function, or an exported method of an
exported type) inside each can-park set. They are what 1.7 owed for a real module.

| can-park set, TS module | functions | exported methods | exported functions |
|---|---|---|---|
| upper, long only | 5,096 | 808 | 240 |
| middle, long only | 3,981 | 367 | 168 |
| middle, cuts 1 and 2 | 181 | 48 | 28 |

Under R2 with the hidden interface twin none of these changes shape. Under a path that makes parking visible
in a signature (row S1 of section 1), these are the signatures that turn task-shaped: 535 in the generator's
reach, 76 with the cuts.

### 7.8 Controls

| control | reading |
|---|---|
| positive: `(*core.LimitedSemaphore).Acquire`, `(*sync.WaitGroup).Wait` | CAN PARK (channel send; `WaitGroup` wait), each with its path |
| negative: `strings.ToUpper` | CANNOT PARK |
| the patched tool with no new flag | 7,106 and 7,182, the unpatched tool's counts |
| `-static-only` with `-no-func-values` | 311 and 2,652, the lower arm's counts |
| the tool with `-cut` built in, no cut named | 7,106 (upper) and 4,867 (middle) |
| negative cut: `-cut strings.ToUpper` on the middle arm | 4,867 and 3,981, unchanged |
| after cut 1 | `fmt.Sprintf` CANNOT PARK; `fmt.Fprintf` CAN PARK |
| after cuts 1 and 2 | `checkSourceFile`, `getTypeOfSymbol`, `parseStatement`, `(*binder.Binder).bind`: CANNOT PARK |
| the lock edge is not one lock: the lower arm with lock leaves, `(*ast.Node).EagerJSDoc` cut | 2,021 for the module, from 2,229; the checker 1,644, from 1,729 |

### 7.9 The generator's refusals: the readings in hand

B5 ran no generator. The refusal rate the plan asks for stands on B2's and B4's reports:

| reading | candidates | twins | refused candidates that reach a park | share of the parkers refused |
|---|---|---|---|---|
| the standard-library scout, 55 packages (4.6) | 5,333 | 372 | 49 | 11.6% (49 of 421) |
| `net/http` (6.3) | 1,209 | 255 | 39 | 13.3% (39 of 294) |

- In `net/http` 32 of the 39 are by-ref receivers and 4 are by-ref parameters; 359 of its 1,209 candidates take
  their receiver by reference. In the scout 39 of the 49 are unsafe functions and 9 are by-ref; 43 of the 49
  are in `syscall` and 6 in `runtime`.
- On the TS module 90% of the middle arm is pointer-receiver methods (7.3). Which of them the converter emits
  with a by-ref receiver is a property of the emitted C#, not of the Go source, and was not read.
- The count of refused parkers across the whole corpus, which 6.3 left to B5, was not run.

### 7.10 Predictions and verdicts

P6 and P7 are the plan's lines; the others were written before each run and are in the lane's prediction file
with their times.

| id | prediction | reading | verdict |
|---|---|---|---|
| P7 | the can-park upper share on the TS closure is under 15% | 54.2% of the TS module, 42.0% of the closure | MISS |
| P7 | 50% that the run under twins fits the spike | not attempted (7.12) | did not fit |
| P6 | the generator's hard refusals are at most 2% | 11.6% and 13.3% of the functions that reach a park (7.9) | MISS on the readings in hand |
| B5-C1 | upper, long only, TS module: 20 to 45%; 60% that P7 reads MISS | 54.2% | MISS, high; the P7 line HIT |
| B5-C2 | lower, long only, TS module: under 3% | 0.1% (11 functions) | HIT; the lower arm with lock leaves (23.7%) was not predicted |
| B5-C3 | 15 to 80 direct parkers in the TS module | 5 | MISS, low |
| B5-C4 | middle, long only: 25 to 50%; 60% within 15 points of the upper | 42.4%; 11.8 points under | HIT, both |
| B5-C5 | middle with lock leaves: 40 to 55% | 43.4% | HIT |
| B5-C6 | middle with cut 1: 10 to 30%; 50% under 15%; at most two more cuts | 27.5%; not under 15%; one more cut reads 1.9% | HIT; the 15% line did not come in |
| tool controls | the counts of 7.8 | as predicted | HIT |
| B5-G1, B5-G2, B5-R1 | the generator on the converted TS closure, and its run | not run | no verdict |

Stop lines: none is keyed on these shares, and none fired. K1, K2 and K4 were settled in sections 4 to 6, K3 by
the coordinator's ruling on B4 (not fired: the bar is a program over 2 times, and the 3.0 times of 6.4 is the
floor kernel of a copy loop, carried as the worst case).

### 7.11 Findings

1. **The twin rule as built marks the hot path of a large program.** 42% of the TypeScript module is in the
   generator's reach of a park, 65% of its checker and 73% of its parser. P7 expected under 15%.
2. **Three edges do it, and none is a park in practice**: lock leaves, `fmt` calling an operand's methods, and
   `fmt`'s rune reader under `math/big`. Without them the rule marks 1.9% of the module: the driver, the
   resolver, the configuration and file layer, and none of the bodies that do the compiling.
3. **R2 on a program of this size needs a declared does-not-park set, or a sharper rule for locks** (7.5). The
   set is short here (three entries, one witness each), safe by construction, and not consumer-visible. Its
   form is not designed, and a consumer's own module needs a way to add to it.
4. **The edges are in the standard library, so the set travels**: every program that formats a value meets
   cut 1, and every program that calls `(*big.Int).SetString` or `fmt.Sscan` meets cut 2. A set shipped with
   the converted `fmt` covers them for everyone.
5. **The long park at the end of two of the three edges has no twin in the spike** (the poller's wait, 7.4).
   A landing form that twins the poller meets both in full. The lock edge is a leaf today for `Mutex`.
6. **The parks that last are few**: 45 functions in a closure of 16,923 hold one, 5 of them in the TS module,
   and channel operations and `select` are nearly all of them.
7. **The func-value gap on this program is 127 literals and 124 call sites** with the cuts (7.6), and the
   module's own parallel work is queued as func literals.
8. **The refusal rate is 12 to 13% of the functions that reach a park, against a 2% line** (7.9). By-ref
   receivers and parameters are 36 of the 39 in `net/http`. This, and not the cut list, is the generator's own
   open cost. Closing it asks for a twin over the box where the plain form takes the reference, as B1's hand
   twin did (3.8); the generator does that for a `ref` local derived from a box (4.1) and not for a receiver or
   a parameter.

### 7.12 What B5 did not measure

- **The TS closure converted, built and run under twins** (B5-G1, B5-G2, B5-R1, and P7's second line). The
  converted tree builds only with the TS session's hand patches over dozens of emitted files, and those are on
  that session's branch and box, not here. The generator's own counts on this program (twins, awaits, refusals
  by reason, build wall, assembly size) and the type-check wall with and without twins need that tree. It is an
  open item for the final answer, and a job for the TS session if the coordinator wants it.
- **The middle arm with the generator's own interface rule** (by interface type, per project) and with the
  spike's own leaf set.
- **The cost of the marking on this program**: the twin-call adder on the checker's path and the growth of its
  assembly without cuts.
- **A contended lock behind the lock cut**, on any platform.
- **`GoTwinsIfaceAll` on a real closure** (4.9, item 12). The census gives a proxy only: of 2,453 interface
  call sites, 426 can reach a parker in the generator's reach and 110 with the cuts. Those are sites, not
  interface members.
- **The refused parkers across the corpus** (6.3), and the refusal classes on the TS module.
- **The 27 packages of the closure that the census does not count** (the runtime family, and packages without a
  counted function), and the difference between this closure's 48 TS packages with counted functions and the
  TS session's own package count for `checkdemo`.
- **Any other `GOOS`.** The closure and the leaves were read for windows.

### 7.13 Gates (Windows x64, dotnet SDK 10.0.400 / runtime 10.0.11, go1.26.10; M:N switches unset)

B5 changes two files, both under `docs/` (the BOARD rows of `85989b9947` and this section), so its gates are the
docs guards. Run on the tree as committed with 7.1 to 7.12 (`5370328e35`), 23:45 to 00:01 CST.

| gate | reading |
|---|---|
| `go test ./...` in `src/go2cs` (the converter suite and the repo guards) | 11 packages ok, 0 failed (986 s) |

- GenTests, GolibTests and the solution-integrity gate were not run again: no file they read changes after the
  readings of 6.11.
- The repo guards ran once more after this subsection was placed. That reading is in the phase's DONE post,
  since writing it here would change the file they read.
