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
