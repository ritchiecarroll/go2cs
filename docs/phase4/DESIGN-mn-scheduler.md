# DESIGN — M:N goroutine scheduling: the options weighed, and a recommendation to rule on

> **STATUS: PROPOSED (lane G, 2026-11-09; branch `claude/g-mn-design`, cut from master `0904811dcf`).** Design only:
> no code lands from this document. It answers the owner's post-`.3` M:N order (§1), weighs every option the order
> names, the runtime green-threads route and async at every level included, and asks the rulings in §9. It builds on
> lane R's [sizing note](../DESIGN-mn-scheduler-sizing.md) and the [cooperative scheduler](DESIGN-cooperative-scheduler.md).
> Their **measurements stand**; their option tables are superseded by §5 and §6 here. The native-call seam it depends
> on is lane H's DESIGN-native-call-seam, on `claude/h-cgo-foundation` (§7). Amended by dated blocks, never rewritten.

## 1. The ask, the tenets, and how this document decides

The owner opened this work after `.3` (2026-11-08, relayed by COORD; wording his, spelling corrected):

- *"M:N scheduling right after .3 sounds appropriate."*
- On async: *"I don't want to wholesale exclude ideas related to async: if there are suggestions, even if impactful
  to visual parity, they should be 'weighed', but the idea will be measured based on project tenets, parity versus
  behavior, for example. If behavior can be better matched at some visual parity expense, it will get a strong
  consideration."*
- On green threads: *"For the post .3 M:N exploration, we should take a hard look at the Green Threads experiment
  [dotnet/runtimelab `feature/green-threads`]. Basically, take it as inspiration: a hard look at a full runtime
  threading emulation of goroutines. My gut tells me this would provide the best parity. We'll let the task explore
  its options, but this seems like a very good path to examine."*

How each option is judged, in this order:

1. **Behavioural parity.** This is the owner's lead criterion. Does a Go program that parks many goroutines, blocks in
   `select`, sleeps or waits on a socket behave as it does under Go: the counts it reaches, the memory it holds, its
   latency, and whether it runs at all on macOS (§2)?
2. **Visual parity.** What a reader of the converted C# sees change, and how much of it.
3. **The other tenets.** The library tenet (converted Go libraries are consumed by ordinary .NET programs, through
   NuGet and nugetgo), Native AOT, defer/recover, the `ж` pin model, P/Invoke and cgo, and how much existing work an
   option invalidates.

The owner's gut leans to green threads, so that route (§3, §4, O2) gets the most rigorous treatment. Its strongest
honest case is stated as well as its showstoppers.

## 2. The facts every option must answer

**The macOS thread cap.** C1 measured it on GitHub-hosted runners on 2026-11-08 (run 42497561630; recorded in
[Limitations](../Limitations.md) and [KnownIssues](../KnownIssues.md)):

| RID | macOS | `kern.num_taskthreads` | First `go` statement to fail |
|---|---|---|---|
| osx-arm64 | 15.8.2 | 2048 | 2,027 live goroutines |
| osx-x64 | 15.7.9 | 3072 | 3,051 live goroutines |

The failing `go` statement throws `System.OutOfMemoryException` from the thread start. The cap counts **threads, not
memory**, so no stack-reserve change lifts it. The only thing that lifts it is a parked goroutine holding **no** OS
thread, which is M:N's definition. Today a program on a Mac cannot hold even a few thousand idle connections.

**Scale on Windows and Linux** ([sizing note §1](../DESIGN-mn-scheduler-sizing.md#1-the-gap-measured), ParkStorm,
R-LAPTOP): 10,000 parked goroutines take about 600 MB and 4.4 s to start, where Go takes 99 MB and 0.07 s. 100,000
take minutes and about 6 GB. 1,000,000 never finished starting. Go parks a goroutine in about 8.5 KB; go2cs needs
52–59 KB, almost all of it the OS thread.

**What M:N cannot change.** A goroutine blocked **inside a native call** holds its OS thread under every scheduler,
Go's included. A native frame cannot move (seam assumption M1, §7). Go bounds those threads with `sched.maxmcount`
(10,000; `runtime/proc.go:863`, go1.26.10). So on macOS the cap still bounds how many native calls can block **at
once**, under any option here. M:N frees parked goroutines, not blocked syscalls.

**Preemption.** Go has preempted a long-running goroutine asynchronously since Go 1.14, by signal (`asyncPreempt`,
`runtime/preempt.go`). Today's 1:1 model gets the same behaviour from the OS scheduler: a goroutine in a tight loop
cannot starve another. **Every cooperative M:N option is a parity LOSS here.** A goroutine that spins with no park
point holds its worker, and with `GOMAXPROCS` workers all spinning, a runnable goroutine never runs. That is Go's
pre-1.14 behaviour, and a spin-wait on a flag another goroutine must set becomes a livelock. §6 names each option's
answer; §8 recommends one that keeps today's guarantee.

## 3. runtimelab green threads, first-class

Three primary sources, all read on 2026-11-09:

- **The results write-up:** dotnet/runtimelab issue #2398, *Green Thread Experiment Results*, opened by the
  experiment's author (GitHub `davidwrighton`) on 2023-09-08, closed 2025-01-22.
  <https://github.com/dotnet/runtimelab/issues/2398>
- **The design and technical report:** `docs/design/features/greenthreads.md` on `feature/green-threads`.
  <https://github.com/dotnet/runtimelab/blob/feature/green-threads/docs/design/features/greenthreads.md>
- **What Microsoft did instead:** runtime-async, dotnet/runtime issue #109632.
  <https://github.com/dotnet/runtime/issues/109632>

### 3.1 How it worked

| Concern | The prototype's answer (greenthreads.md) |
|---|---|
| **Stacks** | Segmented. Each segment carries a ~4 KB red zone, for a 12 KB minimum. A pre-prolog in every managed function compares the stack it needs with the TEB's limit and calls `_morestack` to add a segment. A contiguous, moving stack was ruled out because byrefs converted to pointers cannot be updated when a stack moves. The design names the **hot-split** hazard (a loop that grows and shrinks across a boundary) and does not mitigate it. |
| **Switching** | Yield and resume patch the return addresses of well-known frames. Green threads run on thread-pool threads, and their stacks come from a pool. |
| **Scheduling API** | `Thread.IsGreenThread`, `Task.RunAsGreenThread` (also written `RunOnGreenThread` in the doc), and a `Task.Wait()` that **yields** when called on a green thread. |
| **Blocking I/O** | Synchronous socket and file APIs check `Thread.IsGreenThread`. On a green thread they call their `*Async` counterpart and wait on it through the yielding `Task.Wait`. APIs with no async counterpart, and libraries that are async-only (`System.Threading.Channels`, `System.IO.Pipelines`, most of `System.Net.Http`), are listed as gaps. |
| **P/Invoke** | Native code cannot run on a green segment, so **every P/Invoke transitions to the OS thread's stack** through a thunk. A native→managed callback cannot yield and falls back to a blocking wait. Native code that relies on OS thread state can break, because the next call may land on another OS thread. |
| **Thread statics** | The **reuse model**: a green thread sees the statics of whatever OS thread runs it, "mirror[ing] how our ThreadPool gets the best performance". There are no per-green-thread statics. |
| **Locks / `Monitor`** | Not covered. |
| **GC** | Byrefs are reported only by stack walks, which is why segments cannot move. Segment scanning appeared conservative; precise scanning is listed as unexplored. |
| **Exceptions** | SEH "does not safely handle cross stack segments"; not validated. |
| **Diagnostics** | ETW, WinDbg and TTD "fail to work reliably". Linux `perf` with split stacks is unsupported. |
| **Platforms** | **Windows only**, built and tested. Each OS and architecture needs hand-written assembly, and a Linux prototype is listed as unexplored. |
| **Security** | Intel CET shadow stacks are "not simple to make compatible", and the Windows implementation of the time could not work with them. User APCs and hardware exceptions that run code on the thread's stack are not handled well. |

### 3.2 What it measured

| Measurement | Result |
|---|---|
| ASP.NET plaintext (#2398) | 162,019 rps on green threads vs 178,620 rps async |
| P/Invoke, 10⁸ calls (#2398) | about 300 ms on an OS thread, about **1,800 ms** on a green thread |
| Yield + resume | about 500 ns (two OS context switches: about 8 µs) |
| Await continuation | about 110 ns on an OS thread; slower on a green thread |
| Green thread creation / reuse | about 3 µs / 600 ns; with a very deep stack about 120 µs / 24 µs |
| In-process socket round trip | dedicated threads 57 µs; async 64 µs; green thread with sync waits 67 µs |
| Deep call stacks (#2398) | slower on green threads than the equivalent async chains |

The write-up's own verdict: *"The prototype proved that implementing green threads in .NET and ASP.NET Core would be
viable."* It also found that green threads were too unstable for P90/P95/P99 latency measurement.

### 3.3 Why it stopped

> "We have chosen to place the green threads experiment on hold and instead keep improving the existing (async/await)
> model." (#2398)

The stated reason is *"primarily due to concerns about introducing a new programming model"*: two concurrency models
that compose badly (async code called from a green thread needs sync-over-async), interop that is "complex and
comparatively slow", thread-local and native-thread-state surprises, and security mitigations that are "quite
challenging" to support. Making green threads faster in some scenarios would make them slower in others. The issue
points to runtime-async (#109632) as the follow-on.

### 3.4 Runtime-async, the route Microsoft took

Runtime-async moves async-method implementation from the C# compiler into the runtime. The runtime, not a compiler
state machine, keeps the suspended state. As #109632 reads on 2026-11-09:

- It is **opt-in**: a preview in .NET 10 (`EnablePreviewFeatures`, `Features=runtime-async=on`, `DOTNET_RuntimeAsync=1`),
  and a feature flag in .NET 11. It is CoreCLR only. The issue is open on the 12.0.0 milestone.
- Native AOT, ReadyToRun, PGO, the interpreter and stack-trace formatting are tracked as open items.
- Methods are still **async-shaped**: a runtime-async method still returns `Task`/`ValueTask`, and its callers still
  `await` it. It lowers the cost of an async method; it does **not** remove the signature change or the colouring.

So the CLR's answer to "blocking at scale" is async, now cheaper. It offers no stack switching, and no runtime-provided
continuation that synchronous code can use.

## 4. What goroutines at the runtime-threading level would take for go2cs

The owner's question is what a **full runtime threading emulation** of goroutines would take. There are three ways to
read it.

**(a) Fork CoreCLR and ship it.** Rebase runtimelab's prototype from its 2023 base onto .NET 10, then finish it:
Linux and macOS (hand-written switch and `_morestack` assembly per OS and architecture), CET, SEH across segments,
`Monitor`, diagnostics. Ship the result as a custom runtime pack, which an application must publish self-contained to
use. golib's park primitives (`SemaphoreSlim`, `ManualResetEventSlim`, the timer waits) would then yield through the
fork's API when `Thread.IsGreenThread` is true.

**(b) A custom host over the stock runtime.** A host controls how the runtime starts, not how it executes managed
frames. Stack switching lives in the JIT's prologs, the stack walker, the GC and the unwinder. **A host cannot supply
it**, so (b) reduces to (a).

**(c) golib-only stackful emulation, inspired by green threads.** This would mean stackful coroutines over a small
thread pool, with no runtime change. **It is infeasible.** The CLR offers no supported stack switch, and switching by
hand breaks GC stack walks (sizing note §4.D). A coroutine that must keep its frames must therefore keep a real thread.
That is today's thread per goroutine, and it is exactly the conclusion `Coro.cs` already records for `iter.Pull`
(`src/core/golib/runtime/Coro.cs`, its "Why a rendezvous and not a state machine" remark).

So "runtime green threads" means (a), and O2 in §5 is (a).

## 5. The options

Sizing is in lane-weeks, as judgement, the sizing note's convention. Each option names its behaviour at the
native-call seam; §7 collects these.

### O0 — status quo: 1:1, reused threads (baseline)

A goroutine owns a reused OS thread for its life (batch 31). Converted code blocks the thread at every park.
**Behaviour:** full Go semantics for preemption, `LockOSThread`, defer/recover and thread-bound state; capped at the
§2 numbers. **Visual:** none. **Seam:** inert accounting (S-a..S-d). **Sizing:** 0.

### O1 — 1:1 with smaller reserves (sizing note option A): mostly landed

**What is at master.** Seat #3 landed the converter half of option A, inverted (`b8587f92c5`). The converter proves
which `go` statements cannot recurse (`src/go2cs/goroutineStackAnalysis.go`) and records them in the package's info
file. go2cs-gen intercepts those starts, which run on a **16 MiB** reserve (`Goroutine.BoundedStackReserve`). Every
other goroutine keeps the **256 MiB** default (`Goroutine.StackReserve`). Both can be overridden by environment
variable (`GO2CS_GOROUTINE_BOUNDED_STACK`, `GO2CS_GOROUTINE_STACK`).

**The remainder.** At 10⁵ parked, the sizing note measured a 1 MiB reserve at 4.6 times faster than 256 MiB. The
landed half captures that only for the goroutines proved bounded, and only down to 16 MiB. What is left:

- a census of how many `go` sites the analysis proves bounded across the corpus (not measured);
- ParkStorm at 1 MiB vs 16 MiB for the bounded pool, to decide whether 16 is worth lowering;
- whether the unproven default can come down. It cannot safely: an under-reserved recursive goroutine dies with an
  uncatchable stack overflow where Go would grow its stack.

**Behaviour:** Windows and Linux move toward about 10⁵ parked. **macOS gains nothing**: the cap counts threads.
**Visual:** none. **Seam:** as O0. **Sizing:** 1 lane-week (the census plus one measurement), plus 0.5 to retune.

### O2 — runtime green threads (§4a)

**Mechanism.** A forked CoreCLR, shipped as a runtime pack, with golib's parks made green-aware.

**Its strongest honest case.**

- **Zero visual cost.** Converted code stays exactly as it is: synchronous, Go-shaped, with every `GoFrame`, `ж`
  pin, `ref` local and `fixed` region intact. No other M:N option keeps all of that, and this is the parity the
  owner's gut is pointing at.
- **Library opt-in softens the library tenet.** Converted libraries keep targeting stock .NET and run 1:1 there.
  Only an **application** that needs 10⁵ goroutines, or Mac-scale fan-out, would opt in by publishing self-contained
  against the fork. golib detects `Thread.IsGreenThread` and yields at its parks, so **one library binary serves
  both**. That is a real softening: nugetgo packages do not fork, and the cost lands on the consumer who asked for
  scale.
- **Go's own model.** A goroutine is a small growable stack that yields at its parks. That is literally what the
  prototype built.

**Its showstoppers, plainly.**

1. **The consumer runs a forked runtime.** Even with the opt-in, the application must ship a runtime Microsoft does
   not service. Every .NET security patch has to be re-merged into the fork and re-released by this project, for as
   long as anyone runs it. That is an ongoing obligation, not a one-time cost.
2. **No Native AOT.** Native AOT is a different runtime (ILC plus its own runtime). The prototype is JIT-CoreCLR only.
   go2cs validates AOT on every release (aot-smoke), so green threads would be a second-class mode with no AOT.
3. **Windows-only, in 2023.** Linux and macOS, the platform the thread cap hurts most, need new assembly per OS and
   architecture, and Microsoft never prototyped them.
4. **CET.** Since .NET 9 the apphost is marked CET-compatible by default
   ([breaking change](https://learn.microsoft.com/en-us/dotnet/core/compatibility/interop/9.0/cet-support)). The
   prototype's return-address patching is exactly what shadow stacks forbid. Opting out (`CetCompat=false`) trades
   away a hardware security mitigation.
5. **P/Invoke is about 6× slower**, and go2cs is syscall-dense: `os`, `net` and `syscall` all go through the seam's
   funnels.
6. **Diagnostics.** ETW, WinDbg and TTD are unreliable on segmented stacks, and the prototype's write-up reports bugs
   that corrupt memory across threads.
7. **The expertise and scale.** This is runtime C++ and assembly work on a branch Microsoft stopped, which the fleet
   has never done.

**Preemption:** none in the prototype. A green thread yields only at a park, so O2 has the same cooperative parity
loss as the async options, unless the fork also builds async preemption. **Seam:** a P/Invoke already hops to the OS
stack, and the carrier blocks in native with the green thread pinned to it (M1). `Enter` releases the P so another
carrier runs, and `Exit` continues on the same carrier (M2 as written). **Sizing:** 40–80 lane-weeks to a
cross-platform, CET-compatible fork, then a standing maintenance cost per .NET servicing release. Both are judgement
with wide error.

### O3 — golib-only stackful emulation (§4c)

**Recorded infeasible.** Without a runtime stack switch it collapses to O0.

### O4 — selective async lowering, VISIBLE (sizing note option B)

**Mechanism.** The converter computes the **can-park set**: the functions that can reach a park through direct calls,
interface methods and func values. It emits those functions as `async`, and every call to them as `await`.
Goroutines whose body can park run as tasks on a golib scheduler with `GOMAXPROCS` workers. Runtime-async (§3.4) is
the cost model.

**Behaviour.** A parked goroutine holds no thread, so the macOS cap and the 10⁵ wall go. Timers and netpoll map onto
.NET's own completion. **Visual:** every can-park function's signature becomes `Task`-shaped, and every call to one
reads `await`. If the can-park set reaches most of `io`, `fmt` and `net`, as is likely through `io.Reader`, that is
most functions. **Other tenets:**

- the defer frame needs a third generation (`GoFrame` is a `ref struct`);
- recover's slots and every thread-bound entry move to the goroutine;
- a `ref` local or `ref struct` live across a park, and any park inside `fixed`/`unsafe`, is a compile error. The
  emission has to restructure each one (C# 13 allows such locals in async methods only where they are not live
  across an `await`:
  [spec](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-13.0/ref-unsafe-in-iterators-async));
- every golden and corpus file re-baselines.

**Seam:** `Enter` releases the worker's P (the worker blocks in native), and `Exit` reacquires one or **parks the
thread** (M2 as written). **Preemption:** cooperative; §6 gives the answer. **Sizing:** sizing note S3 + S4,
**18–28 lane-weeks**.

### O5 — full async lowering, VISIBLE (sizing note option C)

Every function becomes async. Its behaviour is O4's without the census, but it taxes the roughly 99% of calls that
never park, and every signature changes. **Weighed, not recommended:** O4 or O6 reach the same behaviour at a
strictly smaller visual and runtime cost. **Sizing:** comparable to O4, plus a universal performance regression.

### O6 — HIDDEN async: generated async twins

**Mechanism.** The visible converted `.cs` stays synchronous and Go-shaped. **go2cs-gen** reads each can-park
function's syntax and emits an **async twin** beside it, in the same `partial` package class: `ValueTask`-shaped,
with each park lowered to an `await` on golib's park and each can-park call routed to the callee's twin. A goroutine
whose entry is in the can-park set runs its entry's twin on the M:N scheduler. Converted package classes and
interfaces are already `partial` (`partial class io_package`, `partial interface Reader`), and go2cs-gen already emits
members into them. This is **green threads at the compiler level**: thread-shaped code for the reader, a parked
goroutine that holds no thread for the program.

**The fallback that makes it safe at any coverage.** A twin calls a synchronous function wherever it cannot route
to a twin (a constraint below blocks one, or the target has none). If that synchronous code parks, golib's park sees
"on a worker, not in a twin". It treats the wait exactly as the seam treats a blocking native call: release the P,
let the scheduler wake or start another worker, block this thread, and reacquire a P on return. So a missing twin
**costs a thread, never correctness**, and a goroutine with no twins at all behaves exactly as O0 (that is O7). The
sizing note's §4.E deadlock (a capped pool whose actions wait on each other) cannot happen, because a blocked worker
always hands its P on.

**Its hard constraints** (COORD note 3, each with its answer):

| Constraint | What it means for a twin | Answer | Showstopper? |
|---|---|---|---|
| `ref` locals and `ref struct` locals (`Span<T>`, `GoFrame`) live across a park | C# 13 allows them in an async method only where they are not live across an `await` | The twin uses a class-based frame instead of `GoFrame`. Where a converted `ref`/`Span` local is genuinely live across a park, that function gets **no twin** and takes the fallback | No: costs coverage, measured by the S2 census |
| `ж` pins, `fixed` and `unsafe` regions across a park | `await` inside an unsafe context is an error | **No twin**: the fallback. A pin held across a park is also a GC hazard under any async option (cooperative scheduler §8.3), so refusing it is correct | No |
| defer / recover inside a twin | `try`/`finally` across `await` is legal; recover's thread-local slots are not coherent across a thread hop | The panic and recover slots and every `ThreadStateCensusTests` entry move onto the `Goroutine` object. **Every M:N option needs this**, O2 included | No, but it is the largest piece of shared work |
| Dynamic dispatch: interface methods | A twin calling `r.Read` through `io.Reader` must reach the implementation's twin | go2cs-gen adds a twin member to the `partial interface` and to each implementation it already generates interface glue for | No |
| Dynamic dispatch: func values | A Go func value is a delegate. `net/http`'s `HandlerFunc` is `public delegate void HandlerFunc(...)`, so **every HTTP handler is reached through a delegate** | A side table from a delegate's target method to its twin, filled by the generator, looked up at the dynamic call; a miss takes the fallback | **The hardest one.** Not a showstopper, but its lookup cost and coverage decide O6. S2 measures both |
| Two bodies per can-park function | IL size, Native AOT binary size, compile time, generator time in the IDE | Twins exist only for the can-park set. Their cost is the S2 census times the state-machine overhead; runtime-async, once AOT supports it, shrinks it | No, if the can-park share is moderate. **It is if** the share approaches all functions, as O5 |
| Debugging | Under M:N the code that runs is the twin, so a breakpoint set in the visible `.cs` would not bind | The generator emits `#line` directives that map each twin line to the visible file's line, so one breakpoint binds both bodies | No, but S2 must prove it in the debugger |
| Stack traces | They show twin names and `MoveNext` frames | Name twins as `<name>` plus a fixed suffix; runtime-async improves traces further | Cosmetic |

**Behaviour.** Like O4 wherever twins cover the path; like O0 wherever they do not. **Visual:** **none in the
converted `.cs`.** Generated files grow, and a debugger reader sees twin frames. **Seam:** as O4. **Sizing:** O4's
scheduler, frame, state-migration and handoff work, minus most of the golden re-baseline (visible emission does not
change), plus the generator's twin emitter and the dynamic-dispatch tables: **16–26 lane-weeks**.

### O7 — hybrid placement

Not an option of its own: it is the placement rule O6's fallback implies, and O4 can use it too. A goroutine runs on
the M:N scheduler when its entry is in the twin (or async) set, and on a dedicated 1:1 thread otherwise. A
`LockOSThread` goroutine always gets a dedicated thread, as Go gives it a dedicated M (`runtime/proc.go:5640`,
`lockOSThread`). Adoption is then **incremental**: every widening of coverage moves goroutines off threads, and no
step can break a program that worked.

## 6. The weigh table

"✔" means parity with Go, "≈" partial, "✘" a loss or no change from today's limit.

| | O0 | O1 | O2 green threads | O4 visible async | O5 full async | O6 hidden twins (+O7) |
|---|---|---|---|---|---|---|
| **Parked goroutines** | ✘ ~10⁴ | ≈ ~10⁵ (Win/Linux) | ✔ | ✔ | ✔ | ✔ where twins cover; ≈ elsewhere |
| **macOS cap (2,027 / 3,051)** | ✘ | ✘ | ✔ | ✔ | ✔ | ✔ where twins cover |
| **Concurrent blocking native calls** | thread each | thread each | thread each | thread each | thread each | thread each: Go's own bound, every option |
| **select / timers / netpoll** | ✘ threads | ✘ threads | ✔ | ✔ | ✔ | ✔ where twins cover |
| **Preemption (Go: async since 1.14)** | ✔ (OS) | ✔ (OS) | ✘ cooperative | ✘ cooperative, unless §8.3 | ✘ same | ✘ same; §8.3 restores it |
| **LockOSThread** | ✔ | ✔ | ≈ needs a pinned carrier | ✔ (O7) | ≈ | ✔ (O7) |
| **Stack depth (Go grows to 1 GB)** | ≈ 256 MiB reserve | ≈ smaller where proved bounded | ✔ grows by segment | ✔ heap state | ✔ | ✔ in twins |
| **defer / recover** | ✔ | ✔ | ✔ unchanged | ✘→✔ third frame generation | same as O4 | ✔ twin-only frame; visible unchanged |
| **`ж` pins, `ref` locals across a park** | ✔ | ✔ | ✔ | ✘ restructure emission | ✘ | ✔ by fallback (no twin) |
| **Visible change** | none | none | **none** | **every can-park signature and call** | **every function** | **none** (generated files only) |
| **Library tenet (nugetgo consumers)** | ✔ | ✔ | ✘ forked runtime; ≈ app opt-in | ✔ | ✔ | ✔ |
| **Native AOT** | ✔ | ✔ | ✘ | ✔ | ✔ | ✔ |
| **Blast radius** | — | small | runtime fork | every golden and corpus file | everything | generator + golib; goldens unchanged |
| **Sizing (lane-weeks)** | 0 | ~1.5 | 40–80 + maintenance | 18–28 | 20–30 + perf tax | 16–26 |

**Reading it.** O2 and O6 are the only options with **no visible change**, which is why the owner's instinct points
the right way. O2 buys it with a forked runtime and AOT. O6 buys it with a generator and a coverage question S2 can
measure. O4 is the fallback if O6's dynamic-dispatch or debugging constraints fail in S2: it buys the same behaviour
at the visual cost the owner's latest ruling allows to be weighed.

## 7. The native-call seam, per option

The seam (`GoNativeCall`, H's DESIGN-native-call-seam on `claude/h-cgo-foundation`, increments S-a..S-d built
red-first) is a **named dependency of every option**. Its contract holds under each (its §12, G's read):

| Option | `Enter(hint)` | `Exit()` | Callbacks (`ReenterManaged` / `ReturnToNative`) |
|---|---|---|---|
| O0, O1 | inert accounting (no P exists) | inert | inert unless a transition is open (S-c) |
| O2 | release the P; the carrier blocks in native with the green thread pinned to it | fast path: retake the P; slow path: **park the thread** (M2) | the goroutine is locked to its thread for the callback (M7); a callback cannot yield (§3.1) |
| O4, O5, O6 | release the worker's P; the scheduler wakes or starts another worker (`Blocks` at once, `MayBlock` after the sysmon threshold) | fast path, any idle P, or **park the thread** (M2 as written; the goroutine is in synchronous frames below the twin) | M7 under every option: a park inside a callback blocks that thread and never unmounts the goroutine |

Three things hold under every option:

- **Raw class (M8) never enters**, so `[SuppressGCTransition]` calls stay off the slow path.
- golib's own `nanosleep` and timer waits become **park sites**, not seam calls (seam §3.3).
- O6's fallback reuses the seam's handoff for **managed** blocking waits. One handoff path serves both native and
  untwinned managed blocking, so the scheduler has a single "this worker is blocked" transition to get right.

## 8. Recommendation

1. **O2 is not the product path.** Record it with §3's evidence and O2's showstoppers. Set an explicit revisit
   trigger: Microsoft ships runtime-supported stack switching, or continuations that synchronous code can use, on
   CoreCLR **and** Native AOT. The owner's instinct is right about the destination (no visible change), and O6 reaches
   it without forking the runtime.
2. **O6 + O7 is the leading candidate**, gated on an S2 spike, because it is the only no-visible-change option that
   keeps Native AOT and the library tenet. **O4 is the held fallback** if S2 fails O6 on dynamic dispatch or
   debugging.
3. **Preemption: keep today's guarantee with sysmon-style carrier injection, not inserted yield checks.** A monitor
   that sees a worker run one goroutine past a threshold (Go's sysmon uses 10 ms, `forcePreemptNS`,
   `runtime/proc.go:6628`, go1.26.10) adds a carrier. A spinning
   goroutine then costs an extra thread, as it does today, and never starves anyone. No emission changes, and
   `GOMAXPROCS` stays as approximate as it is now.
4. **Finish O1's remainder** (the bounded-site census and one ParkStorm measurement, about 1.5 lane-weeks). It is
   independent and cheap. It lifts nothing on macOS.
5. **The S2 spike, 3–4 lane-weeks, no emission change:**
   - the **can-park census** through the converter's own analysis: the share of functions, `go` bodies, interface
     methods and **func-value edges** that can park, and how many twins the constraints in §5 O6 refuse;
   - a **hand-written twin prototype** of ParkStorm and of a `net/http`-shaped handler path through `HandlerFunc`,
     on the golib scheduler, with the seam's handoff exercised;
   - gates, measured on Windows **and** the hosted osx-arm64 runner:
     - 10⁵ parked on macOS, where O0 stops at 2,027;
     - 10⁶ parked on Windows under 20 GB;
     - the delegate side-table cost per dynamic call;
     - a breakpoint set in the visible `.cs` binding inside a twin.

Red-first throughout: each spike gate is shown failing on O0 before the prototype reads it green.

## 9. Rulings asked

Each is one decision, with the recommended answer first.

- **R1.** Runtime green threads (O2) are recorded as **not the product path**, with a revisit trigger. **Recommended:
  yes.**
- **R2.** Hidden async twins (O6 + O7) are the **leading M:N candidate**, subject to S2. **Recommended: yes.**
- **R3.** Visible selective async (O4) is the **held fallback** if S2 fails O6. **Recommended: yes**, under the
  owner's 2026-11-08 async ruling, which already allows its visual cost to be weighed.
- **R4.** Preemption is preserved by **carrier injection**, not inserted yield checks. **Recommended: yes.**
- **R5.** **Schedule the S2 spike** (3–4 lane-weeks, no emission change), with §8.5's gates. **Recommended: yes.**
- **R6.** **Schedule O1's remainder** (about 1.5 lane-weeks), independent of R2–R5. **Recommended: yes.**

## Appendix — sources

| Source | Read |
|---|---|
| dotnet/runtimelab #2398, *Green Thread Experiment Results* — <https://github.com/dotnet/runtimelab/issues/2398> | 2026-11-09 |
| `docs/design/features/greenthreads.md`, dotnet/runtimelab `feature/green-threads` — <https://github.com/dotnet/runtimelab/blob/feature/green-threads/docs/design/features/greenthreads.md> | 2026-11-09 |
| dotnet/runtime #109632, runtime-async — <https://github.com/dotnet/runtime/issues/109632> | 2026-11-09 |
| C# 13, ref and unsafe in iterators and async methods — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-13.0/ref-unsafe-in-iterators-async> | 2026-11-09 |
| .NET 9 breaking change, CET supported by default — <https://learn.microsoft.com/en-us/dotnet/core/compatibility/interop/9.0/cet-support> | 2026-11-09 |
| Go runtime, go1.26.10: `runtime/proc.go` (`sched.maxmcount` :863, `exitsyscallNoP` :5067, `lockOSThread` :5640, `sysmon` :6486, `retake` :6630), `runtime/cgocall.go` (`lockOSThread()` in `cgocallbackg` :330), `runtime/preempt.go` (`asyncPreempt`), `runtime/stack.go` (`stackMin` :78) | 2026-11-09 |
| go2cs: [sizing note](../DESIGN-mn-scheduler-sizing.md), [cooperative scheduler](DESIGN-cooperative-scheduler.md), DESIGN-native-call-seam (`claude/h-cgo-foundation`), seat #3 `b8587f92c5`, C1's run 42497561630 | master `0904811dcf` |
