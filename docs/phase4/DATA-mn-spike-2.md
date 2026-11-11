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
