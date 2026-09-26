# DESIGN — managed profiling: sizing the `runtime/pprof` and `net/http/pprof` rows

> **STATUS: PROPOSED (lane P2, 2026-09-26). A SIZING, not a cut. Docs only; no code lands until COORD
> rules on this note.** Base: master `db1bd885a2`. Every reading below was taken by P2 on a linux
> cloud container (Ubuntu 24.04, x64), go1.24.13 (GOROOT `/usr/local/go1.24.13`, as `go env GOROOT`
> prints it), .NET SDK 10.0.112, `GoTargetOS=linux`, `CGO_ENABLED=0`, through the converter's own
> `-tests -test-action all -test-config Release` (TC0, the configuration of record), `-test-timeout 10m`.
> No pwsh on the box, so the sweep wrapper was not used; the invocation is the one
> `Invoke-SweepRow` makes. **No Windows or darwin reading was taken.** The roster's columns are the
> Windows record, so every "moves" below is a LINUX prediction or measurement and says which.
>
> Companions: ledger `553cdcbae3` (i9's 1.24.13 evidence), `f545b18d4d` (the CPU state-leak ruling),
> `8fc439415f` (blockevent refuses by name; the managed bucket store BANKED as an arc);
> [`CENSUS-runtime-pprof-doors.md`](CENSUS-runtime-pprof-doors.md) (the 1.23.12 door census, whose
> classes this note re-derives at 1.24.13 on linux);
> [`AUDIT-runtime-pprof-vacuous-passes.md`](AUDIT-runtime-pprof-vacuous-passes.md);
> [`DESIGN-cooperative-scheduler.md`](DESIGN-cooperative-scheduler.md) (its Stage B, park-time stacks);
> [`DESIGN-pprof-linkname-push.md`](DESIGN-pprof-linkname-push.md) (the five forwarders).

---

## 1. The readings, at master, on linux

| row | Go rows | C# verdicts | matched | differ | disclosed (host-fatal) | gated | wall |
|---|---:|---:|---:|---:|---:|---:|---:|
| `runtime/pprof` | 163 | 153 | 127 | 36 | 6 entries (22 Go rows withdrawn by `-skip`) | 1 (`TestFakeMapping`) | 338 s |
| `net/http/pprof` | 15 | 15 | 11 | 4 | 0 | 0 | 161 s |

`runtime/pprof` C#: pass 126, fail 23, skip 3, infrastructure-error 1. Go: pass 162, skip 1
(`TestMapping`, cgo; matched). `net/http/pprof` C#: pass 11, fail 2, skip 1, infrastructure-error 1.

**Against the older readings.** i9 read runtime/pprof on Windows at `f545b18d4d` as 155 verdicts, 37
differ; the 1.23.12 census read 120 matched. At master on linux, `TestConvertCPUProfile`,
`TestConvertMemProfile` (+2) and `TestGoroutineCounts` now match, and the CPU class's keystone is a
DIFFERENT symbol: `rt_sigaction`, not `asmcgocall` (§2, class A).

**Production drift from a `-tests` run, observed, not new:** the conversion rewrote
`runtime/pprof/package_init.cs` and `net/http/pprof/pprof.cs` (the `initᴛᴛtests` hook). The
measurement worktree was never committed.

---

## 2. Every divergence, by mechanism

36 + 4 differing rows, and the 22 withdrawn rows, sort into eight mechanisms. The per-row list is the
appendix.

| class | mechanism | runtime/pprof | net/http/pprof | first symbol / message |
|---|---|---:|---:|---|
| **A** | CPU-profiler setters are converted on linux and throw, leaking `cpu.profiling` | 16 | 1 | `rt_sigaction` via `setProcessCPUProfilerTimer → getsig` |
| **C** | no CPU sampler (a profile with zero samples) | *11 after A* | 0 | `CPUProfilingBroken` skip, "ignoring failure" |
| **B** | block/mutex bucket store is Go-layout memory | 1 | 0 | `saveblockevent` named refusal |
| **F** | no block/mutex EVENTS fed from the managed primitives | 1 | 1 | "did not see any samples", "mutex profile is not working" |
| **S** | `runtime.Stack(all)` cannot render a parked goroutine's frames | 22 withdrawn | 0 | the six host-fatal entries (`awaitBlockedGoroutine`) |
| **M** | no memory-profile records | 6 (+1 gated) | 0 | empty `heap profile: 0: 0` |
| **I** | inlining probe cannot see Go's inliner | 12 | 0 | "Can't determine whether ... was inlined" |
| **T** | no execution tracer (E4 capability) | 0 | 2 | `/debug/pprof/trace` 500, and the parent |
| **G** | goroutine-profile label race, INTERMITTENT (queued 2026-09-26, not yet sized) | 0 to 3 | 0 | "profile #1's goroutines with label loop-i:0; 0 != 1" |

Class A's 16 = `TestAtomicLoadStore64` (the infra-error, first alphabetically) plus 15
"cpu profiling already in use". It is the same leak ruling `f545b18d4d` described on Windows, with a
linux root. **The registry comment at `src/go2cs/manualTypeOperations.go:651-662` states that "the linux
and darwin bodies are signal/timer based and stay converted." Measured on linux, that is false:**
the converted `setProcessCPUProfilerTimer` reaches `getsig → sigaction → sysSigaction → rt_sigaction`,
a PartialStubGenerator throw, from `SetCPUProfileRate` after `cpu.profiling` is set.

**Class G, queued 2026-09-26 (COORD, mailbox 08:44:56Z).** `TestGoroutineProfileConcurrency`
matched in the two full runs at master and under probe A, then failed in I1's reading (parent plus
launches `#13` and `#77`) and in the class-I seat's (parent plus `#10`). Run alone with `-run`, it
fails 3 to 5 rows on EVERY run of both the I1 host and the probe A host (8 runs each), so it is a race
in the goroutine profile's label read that I1 does not move. COORD ruled it a separate finding: queued
here for later sizing, not chased now.

`TestCPUProfileMultithreadMagnitude` (+2 subtests) is in class A only on linux: its first statement
skips every other GOOS (the audit classes it VACUOUS on Windows). **Linux exercises three rows the
Windows record does not.**

---

## 3. Probe A, measured (local, never committed, reverted)

To size class A with a number instead of a prediction, P2 replaced the two emitted linux setters in
the measurement worktree with the Windows seat's shape (Go's plan9 no-sampler shape, `os3_plan9.go`:
the process setter does nothing, the thread setter records `m.profilehz`) and re-ran both rows. The
file was restored with `git checkout` afterwards.

| row | before | after probe A | moved |
|---|---|---|---|
| `runtime/pprof` | 127 matched / 36 differ | **132 / 31** | 5 fail/infra → pass: `TestAtomicLoadStore64`, `TestCPUProfileMultithreadMagnitude`, `TestCPUProfileWithFork`, `TestGoroutineSwitch`, `TestTracebackAll`. 11 fail → skip (class C). 0 rows moved the wrong way. |
| `net/http/pprof` | 11 / 4 | **12 / 3** | `TestHandlers//debug/pprof/profile?seconds=1` infra → pass |

The row wall went from 338 s to 550 s: each class-C test now retries until its own ~10 s deadline
before `CPUProfilingBroken` skips it. That is Go's code path, not a hang, but it is 212 s more row wall
under the 600 s default and must be priced at bank time.

---

## 4. The arc, cut into red-first increments

Order is by value per cost. Each increment names its red arm, its seams, its reviewer and the rows
it is predicted to move (linux). "Predicted" means read from code, not measured, unless it says probe.

### I1 — the linux and darwin CPU-profiler setters in Go's no-sampler shape (class A)

- **What:** widen `manualConversionFuncs["runtime"]` rows `setProcessCPUProfiler` /
  `setThreadCPUProfiler` from `goosWindows` to all three targets, with the linux and darwin bodies in
  the shape `windows/cpuprof_windows_impl.cs` already has. It is the linux/darwin twin of ruling
  `f545b18d4d` (a), not new design.
- **Red:** `TestAtomicLoadStore64` reads `infrastructure-error: rt_sigaction` on linux today.
- **Moves (probe, linux):** runtime/pprof +5 pass, 11 fail → skip; net/http/pprof +1 pass.
  Darwin: not measured (darwin's setters call `setProcessCPUProfilerTimer` and
  `setThreadCPUProfilerHz` in `signal_unix.go`, the same signal path, so the same throw is predicted).
- **Gates:** a CONVERTER registry change, so CNR plus the two-seeded, three-target footprint
  (`corpus-reconvert`). Predicted footprint: the per-GOOS `os_linux.cs` / `os_darwin.cs` emissions lose
  the two bodies, plus the new impl files; windows byte-identical.
- **Overlap:** P1's `runtime` row on linux reaches `SetCPUProfileRate` too (any runtime test that
  starts a CPU profile), so I1 can move rows there. Named for P1; nothing in P1's ruled items touches
  these two functions.
- **Independent of the rest of the arc.** Recommended as the first cut.

### I2 — a managed bucket store (class B)

- **What:** ruling `8fc439415f` (B), unbanked. Hand-own `newBucket`, `stkbucket` and the accessors
  `stk()`, `bp()`, `mp()` so a bucket's stack and record live in managed storage (a side record per
  bucket box, and `buckhash` as a managed map from the stack hash), and give `saveblockevent` Go's
  `callers` branch body (the branch its own header says was measured working at `8fc439415f`). The
  converted readers (`blockProfileInternal`, the mutex reader, `memProfileInternal`) keep walking
  `bbuckets`/`xbuckets`/`mbuckets` through `allnext` and the accessors, so they stay converted.
  `pprof_blockProfileInternal` and `pprof_mutexProfileInternal` already forward into them (the five
  forwarders).
- **Red:** `TestBlockProfileBias` panics with the named refusal. GolibTests' `RuntimeBlockEventTests`
  guards the refusal today and inverts in this cut.
- **Moves (predicted, linux):** `TestBlockProfileBias` → pass, provided its frames symbolize as
  `runtime/pprof.blockInfrequentLong` / `blockFrequentShort` through the same `callers` projection
  the goroutine profile uses (a hypothesis; the red arm settles it).
- **Seams:** `runtime` only (`mprof_impl.cs` plus registry rows). A converter registry change, so CNR.
  No golib change.
- **Serves three bucket types.** The same store is what I4 and I6 record into; it is cut once.

### I3 — block events fed from the park seam (class F, block half)

- **What:** Go calls `blockevent` from `chanrecv`, `chansend`, `selectgo`, `semacquire1` (for sync
  primitives) and `notifyListWait`. In this corpus every one of those waits passes through ONE
  golib seam: `Goroutine.Park(reason)`, whose outermost scope already calls runtime's
  `parkTransition` hook (installed by `stubs_impl.cs`'s module initializer). The feed is: stamp
  `cputicks()` when the outermost park enters, and on leave, when `blocksampled` says so, call
  `blockevent(cycles, skip)` on the WAKEE's own thread, filtered by Go's list of block-profiled
  reasons (channel send/receive, select, `sync.Mutex`/`RWMutex` lock, `WaitGroup` wait, `Cond` wait;
  not sleep, not IO wait, not synctest's own parks). The stack is the wakee's own, captured with
  `callers`, which the CLR can walk.
- **Red:** `TestMutexBlockFullAggregation` fails "did not see any samples in block profile" (the
  parent row; its `/block` subtest is VACUOUS per the audit and passes on both sides).
- **Moves (predicted):** needs I4 as well for that row's mutex half. With I4:
  `TestMutexBlockFullAggregation` → pass.
- **Seams:** golib `Goroutine` (a park-entry stamp) and runtime's `parkTransition`. **R reviews**: it
  is R's park seam, and the golib field is a golib API change.
- **Cost to price:** one `cputicks()` per blocking park always; the stack capture only above a
  block profile rate of 0 (Go's default is 0).

### I4 — mutex events from the hand-owned `sync.Mutex` / `RWMutex` (class F, mutex half)

- **What:** Go records `mutexevent` in `semrelease1`, on the UNLOCKER, with the delay the dequeued
  waiter waited (plus Go's tail-average term for the remaining waiters). The hand-owned `sync.Mutex`
  is a `SemaphoreSlim`, not a runtime semaphore, so it needs its own record: while
  `SetMutexProfileFraction > 0`, `Lock`'s park stamps the waiter's start, and `Unlock` computes Go's
  `dt` and calls `mutexevent`. `sync` already references `runtime` (`sync.csproj`), so this is a
  direct call through a Go-prefixed public helper, no hook.
- **Red:** `TestMutexBlockFullAggregation` ("did not see any samples in mutex profile") and
  net/http/pprof `TestDeltaProfile` (skip "mutex profile is not working", which the owner ruled
  2026-09-07 is WORK, not a `platform-skip`).
- **Moves (predicted):** `TestDeltaProfile` → pass; with I3, `TestMutexBlockFullAggregation` → pass.
- **Seams:** `sync/mutex.cs`, `sync/rwmutex.cs`. **R reviews** (sync).
- **P1 overlap:** P1's ruled `semacquire1`/`semrelease1` hand-own over `RuntimeSemaphore`
  (`claude/p1-runtime-sema`) is where Go records `mutexevent` for runtime-semaphore users
  (`internal/sync.Mutex`). I4 should land after it and add the event in that body too, or P1 leaves
  the call site named. Neither lane should write the other's file.

> **AMENDED 2026-09-26 (COORD, mailbox 09:09:54Z, from R's review of P1's semaphore seat; P1's answer
> 08:57:05Z).** The P1-overlap bullet above is WITHDRAWN: no contention in this corpus reaches
> `runtime.semrelease1`. P1's `runtime/sema_impl.cs` forwards `semacquire1`/`semrelease1` to golib's
> `RuntimeSemaphore` and names Go's profile branch as DROPPED, and only the runtime's OWN semaphores
> (worldsema, gcsema, traceAdvanceSema) and export_test reach it. The seams I4 records at are therefore
> the two managed wait primitives, both of which R reviews:
> - **`sync.Mutex`**: the hand-owned `SemaphoreSlim` gate (`sync/mutex.cs:65-86`). `Lock`'s wait stamps
>   the waiter's start, and `Unlock` computes Go's `dt` and calls `mutexevent`.
> - **Everything on golib `RuntimeSemaphore`** (`golib/runtime/RuntimeSemaphore.cs`: `Acquire` parks at
>   :111, `Release` dequeues at :135): `sync.RWMutex`, `WaitGroup` and the other `sync/runtime_impl.cs`
>   pulls, `internal/sync`, and `internal/poll`. The waiter's enqueue time is kept at `Acquire`'s park and
>   read at `Release`'s dequeue, which is Go's `semrelease1` accounting moved to where the queue lives.
>   `RuntimeSemaphore` keeps only the FIFO of gates today (P1), so this is a golib change. Hooking the
>   pull companions instead of the semaphore is the alternative, and R decides between them.
> Neither lane writes the other's file: P1's `sema_impl.cs` stays as it is.

**After I1 + I2 + I4, `net/http/pprof` is predicted bankable on linux** if class T is disclosed (§5):
its only remaining divergences would be `/debug/pprof/trace` and the `TestHandlers` parent that rides
it. Not measured; the Windows reading is owed at bank time.

### I5 — park-time stacks in `runtime.Stack(all)` (class S)

- **What:** `DESIGN-cooperative-scheduler.md` Stage B, which the comment above `ForeignStackPlaceholder` in `runtime/managed_impl.cs` names as the
  replacement for `ForeignStackPlaceholder`: the parking goroutine captures its OWN stack when its
  outermost park begins, and `runtime.Stack(all)` renders it in Go's frame format. Stage B was waiting
  on the synthetic-PC registry, which has since landed (`golib/GoSyntheticPC.cs`). It shares I3's
  capture point.
- **Why it belongs to this arc:** the six host-fatal entries (22 Go rows: `TestBlockProfile`,
  `TestMutexProfile`, `TestMutexProfileRateAdjust`, `TestProfileRecordNullPadding`,
  `TestProfilerStackDepth`, `TestBlockMutexProfileInlineExpansion`) all die in
  `awaitBlockedGoroutine`, which polls `runtime.Stack(buf, true)` for a parked goroutine's
  `runtime/pprof.<fName>` frame. The entries' own retirement condition is exactly this. Once they run,
  their assertions are block/mutex profile content, which is I2-I4.
- **The disclosure note calls this capability "absent BY CONSTRUCTION".** That holds for a RUNNING
  goroutine (the CLR has no supported cross-thread walk). It does not hold for a PARKED one, which
  can capture its own stack before it blocks; every goroutine these tests wait for is parked.
- **Red:** an arm with one goroutine parked on a channel receive inside a named function, and a
  `runtime.Stack(buf, true)` read from another goroutine that must contain that function's frame.
- **Costs and risks, all unmeasured:**
  1. One CLR `StackTrace` per blocking park, always on (a goroutine that parked before a consumer
     asked cannot be captured later). Must be priced against the Performance suite before a ruling on
     always-on versus a knob.
  2. **Blast radius on banked rows:** every banked row whose tests read `runtime.Stack(_, true)` or a
     `debug=2` goroutine profile sees frames where it saw a placeholder. `net/http`'s
     `interestingGoroutines` filter is the named example. A census of those rows is owed before the cut.
  3. The CLR omits JIT-inlined frames. `TestBlockMutexProfileInlineExpansion` wants
     `inlineA → inlineB → inlineC` all present; whether converted functions keep their frames at
     Release TC0 is an open question the red arm answers.
- **Seams:** golib `Goroutine`, `runtime/managed_impl.cs`. **R reviews.** This is the largest
  increment and could reasonably be its own seat under R rather than P2; COORD's call.

### I6 — memory-profile records (class M)

- **What:** feed `mProf_Malloc` from golib's `AllocationCounter` charge sites, which already see each
  golib allocation and its type (`GoReflect.TypeLayout` gives Go's size); sample by `MemProfileRate`
  with Go's per-thread countdown; record frees through a weak-keyed table whose finalizer calls
  `mProf_Free`; and make `runtime.GC()` advance the profile cycle (`mProf_NextCycle`, `mProf_Flush`)
  after a collection and a finalizer drain, as Go's does after sweep. `pprof_memProfileInternal`'s
  honest `(0, true)` then forwards to the real reader.
- **Red:** `TestMemoryProfiler` `/debug=1` (empty where Go shows `32: 1024 [32: 1024]` for
  `allocatePersistent1K`). The companion's own laundering check, `TestFakeMapping`, is gated; its
  gate lifts in this cut and it must then pass on content, not on emptiness.
- **Moves (predicted, low confidence):** `TestMemoryProfiler` (3 rows) and `TestFakeMapping` if the
  recorded sizes equal Go's size classes. **At risk:** `TestHeapRuntimeFrames` (Go's samples come
  from map bucket growth; here growth happens inside the BCL `Dictionary`, which golib does not
  charge) and the two generics rows (they match frame NAMES such as
  `genericAllocFunc[go.shape.uint32]`, a Go GC-shape name the managed symbolizer does not produce).
- **Seams:** golib `AllocationCounter` on every charge site (a hot path), `runtime` GC hand-own,
  `runtime/pprof/pprof_impl.cs`. **R reviews** golib. Largest runtime cost of the arc; recommended
  last and ruled separately.

---

## 5. What the arc does not reach, and the decisions only the owner can make

| class | rows | why the arc does not move it | decision owed |
|---|---:|---|---|
| **C** no CPU sampler | 11 (after I1) | A profile starts, stops and parses with zero samples; Go's `testCPUProfile` then skips via `CPUProfilingBroken`. Owner ruling #1 makes a Go=pass / C#=skip caused by OUR missing feature a feature gap, never a disclosure. | **OWNER.** Either (a) rule the CPU sampler out of scope for the managed runtime (as E4 did for the tracer), which makes these 11 disclosable; or (b) commission a sampler. (b) is possible in principle through an in-process EventPipe session with the SampleProfiler provider (a new package dependency, CLR stacks mapped to Go frames); it is not sized here and would be the largest item in either row. **Sized in §9 (2026-09-26).** |
| **I** inlining probe | 12 (1 skip + TestTryAdd skip + 10 absent) | `findInlinedCall` scans a function's PC span with `FuncForPC` for a different name. A synthetic-PC span resolves to its own function by construction, and Go's inlining decisions are a compiler property the converted program does not carry. | **OWNER**, and a PIPELINE question: the 10 absent subtests have no C# verdict, so no entry can absorb them today. Either an owner ruling that makes the family structural plus a comparison that admits Go-only rows under a disclosed parent, or the rows stay red. |
| **T** execution tracer | 2 (net/http/pprof) | `runtime/trace` is excluded under E4 (2026-09-07). | **COORD**: for a single test the route is a disclosure; `/debug/pprof/trace` returns Go's own refusal text, and the `TestHandlers` parent rides it only if every other leaf matches. |

So **`runtime/pprof` is not bankable by the arc alone**: after I1-I6 at best, 23 rows (C and I) still
need the two owner decisions above. **`net/http/pprof` is reachable with I1, I2 and I4 plus the T
disclosure.**

---

## 6. Overlaps and reviewers

- **R:** I3 and I5 (golib `Goroutine`, the park seam), I4 (`sync`), I6 (golib `AllocationCounter`).
- **P1:** I1 moves rows in the runtime candidate on linux; I4 meets P1's `semrelease1` hand-own. The
  runtime row's lock-profile sample (`mLockProfile` from the managed `lock2`) is the third feed ruling
  `8fc439415f` named; it records into the I2 store but its tests are P1's.
- **C1** (the five forwarders, `DESIGN-pprof-linkname-push.md`): I2 relies on them unchanged.

## 7. Proposed order and asks

1. **Cut I1 now** (P2, `claude/p2-cpuprof-setters-unix`, from master): converter registry + two impl
   files, CNR, footprint, runtime/pprof and net/http/pprof readings before/after on linux.
2. I2, then I4: net/http/pprof reaches the bank on linux.
3. I3, then I5, each after R's review of the golib shape.
4. I6 last, separately ruled.
5. **Owner decisions** on classes C and I, and a COORD ruling on the T disclosure.

## 8. Not measured, stated

- No Windows or darwin reading of either row at master. The Windows record may already differ from
  linux on class A (Windows has the setter seat; the roster still names `asmcgocall` as
  net/http/pprof's keystone, which predates it).
- Probe A is one run of each row, not repeated.
- Every "moves" under I2-I6 is a code read. No red arm for them exists yet.
- The cost of I3's stamp, I5's per-park stack capture and I6's charge-site hook is unmeasured.

---

## 9. Addendum, 2026-09-26: sizing class C, an in-process CPU sampler

> **STATUS: SIZING for the owner's ruling (COORD, mailbox 2026-09-26, class C: "SIZE IT FIRST").
> Docs only.** Until the owner rules, ruling #1 stands: the 11 class C rows are a feature gap, not
> disclosures, and nothing below is designed around. Base: I1 (`claude/p2-cpuprof-setters-unix`
> `d87c34ccc5`), which makes a CPU profile start, stop and parse with zero samples on every target.

### 9.1 What was measured

A standalone probe on the P2 linux container (.NET SDK 10.0.112, `linux-x64`, published the way the
`-tests` host is published: self-contained, single-file, JIT, trimming off). It is not in the tree.
The probe opens an EventPipe session on its OWN process with `DiagnosticsClient(Environment.ProcessId)`
from `Microsoft.Diagnostics.NETCore.Client`. The session enables the `Microsoft-DotNETCore-SampleProfiler`
provider plus the runtime provider's `Jit | Loader` keywords, with rundown. The probe runs a
`[NoInlining]` hog loop on 1 or 4 threads, stops the session and parses the stream with `TraceLog`
from `Microsoft.Diagnostics.Tracing.TraceEvent`.

| question | measured answer |
|---|---|
| Can a process open a sampling session on itself? | **Yes.** Session start 72-84 ms; stop and drain 24-119 ms; parse 0.38-0.43 s for a 2 s run (590-623 KiB of trace). |
| Do samples land on the hot threads? | **Yes.** 4 hog threads: frames of the hog function on all 4 threads (3,051-3,826 samples in 2 s). |
| Does a frame map back to the converted method? | **Yes, exactly.** The frame's metadata token and module resolve through `Module.ResolveMethod` to the same `MethodBase` object as `typeof(Hog).GetMethod("cpuHog1")`. That `MethodBase` is what `GoSyntheticPC.Of` is keyed by. |
| Can a sample tell running from waiting? | **Yes.** Every sample carries a thread-sample type: all hog samples read `Managed`, and every `External` sample (5,726 of 11,561 in one run) is on a non-hog stack. |
| What does it cost running code? | **About 5%.** Iterations of a hot loop in 2 s with vs without the session: ratios 0.952, 0.950 and 0.936 over three runs. |
| How close is the sample count to CPU time? | **Not close enough yet.** Samples come at about 1 ms per thread. Samples whose stack holds the hog driver, against the process's CPU time: 0.93 in one 4-thread run and 0.77 in another. |
| What does the dependency weigh? | `Microsoft.Diagnostics.NETCore.Client` 0.2.745401: one 152 KB DLL. `Microsoft.Diagnostics.TraceEvent` 3.2.6: a 3.3 MB DLL plus three Windows-only helpers (20 MB package). |

### 9.2 The stack-mapping path

1. **Frames to functions.** Each sampled frame carries an instruction address. The session's
   `MethodLoad` and rundown events name the method that owns it, as a metadata token plus a module.
   `Module.ResolveMethod(token)` gives the `MethodBase`. `GoSyntheticPC.Of(method)` gives the synthetic
   PC Go's profile builder already resolves (`runtime.FuncForPC`, `CallersFrames`), and `GoNameOf`
   spells it `runtime/pprof.cpuHog1`, `time.now`, and so on. **No new name table is needed.** A frame
   whose method is not a converted Go function (framework code, golib) is dropped, or recorded as one
   placeholder frame, the way Go records a non-Go frame.
2. **Samples to the profile.** Go's signal handler calls `cpuprof.add(&gp.labels, stk)` (cpuprof.go:106),
   which writes one record to `cpuprof.log` under `prof.signalLock`. A managed sampler writes the same
   record, from its own thread, with the synthetic PCs as `stk`. Everything downstream is already
   converted and running: `readProfile`, pprof's `profileBuilder`, the proto encoder.
3. **Labels.** In this runtime a goroutine is a dedicated thread for its whole life
   (`runtime/stubs_impl.cs:126`), so a sample's thread id names its goroutine. Labels change over time
   (`pprof.Do` sets and restores them), so reading the goroutine's labels when the sample is converted
   would be wrong. The label seam is `runtime_setProfLabel`: while profiling is on, it would also emit
   one event from a small `EventSource` into the same session. That event carries its thread id and
   timestamp for free, and the sampler joins each sample to the latest label event on its thread.
4. **Rate.** Go's profile values each record as `1e9 / hz` ns: 10 ms at the default 100 Hz. The
   SampleProfiler samples about every 1 ms. Writing every sample would overstate CPU time about tenfold.
   The sampler keeps only Managed-state samples and thins them per thread to one per `1 / hz` of
   elapsed time. The 0.77 to 0.93 ratio above says this still needs work before the magnitude test can
   hold its 10% tolerance.
5. **When samples arrive.** Samples are read after `StopCPUProfile` stops the session, then written
   before the profile's end-of-data. Go's `profileWriter` keeps reading until end-of-data, so the
   profile carries them. Streaming them live (`TraceLog.CreateFromEventPipeSession`) is possible but
   not needed by any of the 11 rows.

### 9.3 Dependency and cost

- **Packages.** The two above, referenced by `runtime` (or by a small golib companion that `runtime`
  calls). `TraceEvent` is the heavy one: its 3.3 MB would ride in every converted program that
  references `runtime`, which is every one. The lighter alternative is a minimal reader of the
  documented nettrace format, owned by the tree. That trades the package for about a thousand lines to
  maintain, and is not sized here.
- **Where it cannot run.** The session uses the runtime's diagnostic IPC socket, which
  `DOTNET_EnableDiagnostics=0` turns off. Native AOT's EventPipe support for the SampleProfiler was
  not measured: the `-tests` host is JIT, but the performance tree also runs AOT. On either, the
  sampler must fall back to I1's zero-sample profile, never throw.
- **Runtime cost.** About 5% on running code while a profile is on, and nothing while it is off. The
  label event costs one `IsEnabled` check per `setProfLabel` while off.
- **Work, by piece.** The session and its lifetime tied to `setcpuprofilerate` (small). Frame
  resolution and the synthetic-PC join (small, given 9.1). Rate thinning and the magnitude calibration
  (medium; it is the one unknown). The label event and the join (small). The dependency or the owned
  nettrace reader (the largest single choice). A GolibTests arm per step, red first. **Reviewers:** R
  (golib, the label seam) and the owner (the dependency).

### 9.4 Which of the 11 rows it moves (linux; every entry is a PREDICTION from reading the test)

| row | needs | prediction |
|---|---|---|
| `TestCPUProfile` | samples in `cpuHog1` | **moves** |
| `TestCPUProfileMultithreaded` | samples in `cpuHog1` and `cpuHog2` on two threads | **moves** |
| `TestMathBigDivide` | enough samples; no stack match (`matches` is nil) | **moves** |
| `TestCPUProfileLabel` | `cpuHogger` samples labeled `key=value` | **moves with the label join** (9.2 step 3) |
| `TestLabelRace` | the same, under concurrent `pprof.Do` | **moves with the label join** |
| `TestCPUProfileMultithreadMagnitude/serial` and `/parallel` | the profile's CPU time within 10% of `getrusage` (40% on some builders) | **uncertain**: rests on the thinning calibration (9.2 step 4) |
| `TestCPUProfileRecursion` | a frame named `inlinedCallee` and at most one recursion frame per location | **uncertain**: the JIT may inline the small callee, which removes the frame the test needs. The converter's `[MethodImpl(NoInlining)]` closure (`callerInliningAnalysis.go`) is a possible lever, not sized. |
| `TestTimeVDSO` | samples in `time.now` (hand-owned in `time/time_impl.cs:86`, so it is named `time.now`) | **uncertain**: the same JIT-inlining question for a small method |
| `TestMorestack` | a stack holding `runtime.newstack` | **does not move**: there are no segmented stacks, so no such frame exists |
| `TestLabelSystemstack` | labeled samples under `runtime.systemstack` during GC | **does not move**: the managed GC never runs on a `systemstack` frame |

**Net: 5 predicted to move, 4 uncertain, 2 do not.** The 2 that cannot move would then be Go=pass /
C#=fail on a STRUCTURAL property (no segmented stacks, no systemstack), which is the ground on which
the owner ruled class I structural. That is a separate owner question, and it arises only after a
sampler exists. `net/http/pprof` has no class C row.

### 9.5 Not measured, stated

- The probe is not the converted runtime: no `cpuprof.add`, no synthetic-PC join and no label event
  were exercised end to end, and none of the 11 rows was run against a sampler.
- Windows and darwin: not run. EventPipe and the SampleProfiler exist on both in CoreCLR, but the
  diagnostic IPC transport differs (a named pipe on Windows).
- Native AOT: not run.
- The 5% overhead is one hot loop on one thread; parallel load was not measured.

---

## 10. Addendum, 2026-09-26: sizing the targeted NoInlining option for profile stacks

> **STATUS: SIZING for COORD (mailbox 2026-09-26 10:17Z: "SIZE the TARGETED option first, as a docs
> addendum, no code").** Base: caller-pc-spans (`claude/p2-caller-pc-spans` `96ce90f997`). There,
> `TestBlockProfileBias` names real call sites but still fails, because the JIT inlines
> `blockFrequentShort` and the CLR `StackTrace` drops inlined frames. With `DOTNET_JitNoInline=1` the
> row passes (linux). The question is whether marking only the functions a profile can record
> `[MethodImpl(NoInlining)]` is small enough to be a targeted fix.

### 10.1 The population

`computeNoInliningClosure` (`src/go2cs/callerInliningAnalysis.go`) today seeds on direct
`runtime.Caller` / `runtime.Callers` users and extends only through thin single-statement forwarders.
To keep a block or mutex profile's frames honest it would have to cover every function that can be on
the stack when `saveblockevent` records, because the profile keeps up to `debug.profstackdepth` (128)
frames. That is every blocking site plus every function that can call one, transitively.

A census tool (not in the tree) loaded `std` with its tests through `golang.org/x/tools/go/packages`
(GOOS=linux, GOARCH=amd64, CGO_ENABLED=0, Go 1.24.13; `cmd/` and `vendor/` skipped). It counted every
`FuncDecl` and `FuncLit` once, by position. **Seeds:** a channel send, receive or `select`, a range over
a channel, a call to `sync.Mutex` / `RWMutex` Lock/Unlock/RLock/RUnlock, `Cond.Wait`, `WaitGroup.Wait`,
or `runtime.blockevent`. **Edges:** static calls to a declared function or method (generic calls through
their origin), and a function literal counted as a callee of the function that defines it. The closure
is every function that reaches a seed over those edges.

| population | functions | seeds | closure | closure share | closure with ≤3 statements |
|---|---|---|---|---|---|
| production | 18,811 | 521 | **5,018** | 27% | 2,427 |
| test files | 20,628 | 822 | 10,536 | 51% | 5,144 |
| all | 39,439 | 1,343 | 15,554 | 39% | 7,571 |

**Packages:** 418 of the 531 production packages hold at least one closure member; 53 hold a seed.
The largest are `net/http` 576, `go/types` 444, `net` 245, `runtime` 164, `math/big` 152,
`crypto/tls` 148, `database/sql` 120, `encoding/gob` 119, `go/parser` 111 (no seed of its own) and
`testing` 105.

**This is a lower bound.** Calls through an interface or a function value are not followed, so a
function that blocks inside an `io.Reader` implementation does not pull in the callers of `Read`.

**Control, with the same tool:** seeding on `runtime.Caller` / `Callers` and taking the FULL transitive
closure gives 3,942 production functions in 385 packages. The converter does not do that. It stops at
thin forwarders, and the converted production tree carries **183** `MethodImplOptions.NoInlining` sites
in 35 files (`src/core` minus golib and the `*_impl.cs` hand-owns, at `e4d73f6e22`). The widened
closure is about **27 times** today's marking.

### 10.2 JIT cost, estimated

Not measured. `[MethodImpl(NoInlining)]` keeps every call to a marked method as a real call: a call and
a return, plus the constant propagation and struct promotion the JIT loses across the boundary. That
costs a few nanoseconds per call and matters only on small, hot methods, which are the ones the JIT
inlines. 2,427 of the 5,018 production members have three statements or fewer. The closure includes
every function that takes a `sync` lock or touches a channel, and every static caller of those, on
every hot path in `net`, `net/http`, `crypto/tls` and `database/sql`. The cost would land on every
converted program, whether or not it ever takes a profile. The only measured end of the range is
`DOTNET_JitNoInline=1`, which marks everything. Its wall time was not recorded.

### 10.3 Verdict

**The targeted option collapses into the corpus-wide one.** 27% of production functions in 418 of 531
packages is "every function NoInlining" in all but name, which is the option already rejected. Honest
profile frames under JIT inlining need what Go's own toolchain records and the CLR does not expose: an
inline tree beside each call site (Go's `expandInlinedFrames` reads it). The CLR `StackTrace` has no
inlined-frame data. By the reading of the class-I ruling, a row that fails only for that reason is
**STRUCTURAL**. `TestBlockProfileBias` would join that family, and its proof is the passing
`DOTNET_JitNoInline=1` run above.

A narrower variant was also counted: marking test-file functions only. It covers 10,536 of 20,628 test
functions and leaves production frames unmarked. It would plausibly move `TestBlockProfileBias`
(`blockFrequentShort` is in `pprof_test.go`), but it is an instance fix. It makes a test's frames
honest, not a program's profile. Not recommended; the count is here so the ruling can weigh it.

### 10.4 The extra `runtime/pprof.blockevent` frame

`pprof_test.go:1221` declares `//go:linkname blockevent runtime.blockevent` with no body. Go binds the
name to the runtime symbol, so no `runtime/pprof` frame exists. The converter emits the pull as a
forwarder method (`writeLinknameForwarder`, `src/go2cs/visitFuncDecl.go:2417`). That is a real
`go.*` / `<pkg>_package` method, so the managed `callers` counts it as a Go frame. In the
`DOTNET_JitNoInline=1` run, the profile's first frame was `runtime/pprof.blockevent`, not
`blockFrequentShort`. The row still passed. Why its check tolerates the extra frame was not traced.
Every skip count taken through a pull is also off by one.

**Population:** 150 bodyless two-argument `//go:linkname` pulls in 29 production packages, and 9 in
test files. That is a source-text count over `GOROOT/src`, with every build-tag variant counted.
**Proposed shape, not cut:** the converter marks each forwarder it emits (an attribute), and
`captureCallers` skips a marked frame the way it already skips non-Go frames. A pull then leaves no
frame, as in Go. It is small and independent of 10.3. It would be a separate increment with a red
GolibTests arm first, and R would review the `captureCallers` change.

### 10.5 Not measured, stated

- The JIT cost in 10.2 is an estimate. No benchmark was run with the widened closure or with
  `DOTNET_JitNoInline=1`.
- The census is static and linux-only. Windows and darwin file sets differ slightly, and dynamic call
  edges would only enlarge the closure.
- The test-files-only variant's effect on `TestBlockProfileBias` is a prediction. It was not run.
- The forwarder fix is not written, and the rows it would move were not identified.

## 11. Addendum, 2026-09-26: the class C sampler, opt-in only

> **STATUS: DESIGN for COORD and R (mailbox 2026-09-26 14:17Z). The owner ruled "BUILD the CPU
> sampler, OPT-IN ONLY"; the sampler and its trace reader must never ride in an ordinary converted
> program.** Docs only. Base: section 9 (the sizing) and I1 (`claude/p2-cpuprof-setters-unix`
> `d87c34ccc5`, in TRAIN C), which makes every target's `setProcessCPUProfiler` a hand-owned no-op.

### 11.1 The seam in Go's own start and stop path

`SetCPUProfileRate` (cpuprof.go) holds `cpuprof.lock`. On start it opens `cpuprof.log` and calls
`setcpuprofilerate(hz)`. On stop it calls `setcpuprofilerate(0)`, then `addExtra()`, then
`cpuprof.log.close()`. `setcpuprofilerate` (proc.go:5631) takes `prof.signalLock` and calls
`setProcessCPUProfiler(hz)` **before** it stores the new `prof.hz`. So inside `setProcessCPUProfiler(0)`:

- the log is still open and `prof.hz` still reads the old rate;
- `prof.signalLock` is held, so `cpuprof.add` would spin forever. The drain must write with
  `cpuprof.log.write(tag, time, hdr, stk)` directly, which is what `add` does once it holds the lock.

`setProcessCPUProfiler` is already hand-owned on all three targets (windows by the earlier seat, linux
and darwin by I1). It is the one place a sampler starts and stops. Samples are drained there on stop,
with their own timestamps, before `addExtra` and `close`. `profBuf` overflow counts as lost records
the same way Go counts them.

### 11.2 The options, weighed

| option | what an ordinary program carries | how a program opts in | where it cannot work |
|---|---|---|---|
| (i) `runtime` loads a companion by name when `hz > 0` | a by-name load path in `runtime` | copy `go2cs.CpuProfiler.dll` beside the program | trimming and Native AOT drop or refuse a by-name load; the copy is manual |
| (ii) an MSBuild property adds the companion and a registration item | the registration seam in `runtime` only | `<GoCpuProfiler>true</GoCpuProfiler>` in the program's csproj | Native AOT and `DOTNET_EnableDiagnostics=0` (fallback, 11.4) |
| (iii) an owned nettrace reader, about a thousand lines | nothing extra | combines with (i) or (ii) | none beyond (ii)'s; it replaces the 3.3 MB `TraceEvent` in opted-in builds |

**Recommendation: (ii), with `TraceEvent` inside the companion. (iii) is deferred.**

- (ii) is explicit and safe under trimming and AOT, and loads nothing by name.
- `TraceEvent` rides only in builds that opt in, so the owner's rule holds without (iii).
- (iii) becomes worth its maintenance only if an opted-in build's size matters. The -tests hosts of
  the two opted-in packages are the only such builds today.

### 11.3 The shape of (ii)

- **Companion:** a new project, `src/core/go2cs.CpuProfiler`. It references `runtime` and golib, plus
  `Microsoft.Diagnostics.NETCore.Client` and `Microsoft.Diagnostics.TraceEvent`. It owns the EventPipe
  session, the stream parse, frame resolution through `GoSyntheticPC`, the label join (9.2 step 3) and
  the rate thinning.
- **Registration:** the property adds two items to the program's build: a `ProjectReference` to the
  companion, and one linked `Compile` item from the companion's folder. That item holds a
  `[ModuleInitializer]` that calls `runtime_package.GoRegisterCpuSampler(...)`. A module initializer in
  the program's OWN assembly always runs, which a referenced assembly's does not until something loads
  it. The items live in a `.targets` file the converter's csproj templates import, conditioned on the
  property.
- **`runtime`'s seam:** a public sampler interface (`Start(hz)`, `Stop(write)`), one static slot, the
  register method, and a call from each target's `setProcessCPUProfiler`. When no sampler is
  registered, the setter stays I1's no-op, which is exactly today's zero-sample profile.
- **Default-build footprint:**
  - no added assembly, package reference or file in an ordinary program;
  - zero bytes in every assembly except `runtime.dll`;
  - `runtime.dll` grows by the seam's IL, **estimated under 2 KB and not measured**. The seam's
    first cut owes the measured delta as a before-and-after size of `runtime.dll`, per target.

  Zero bytes in `runtime.dll` too would need the drain writer and the setter hook to live outside
  `runtime`, which has no seam to hang them on. I do not propose it.
- **How a user opts in:** set `<GoCpuProfiler>true</GoCpuProfiler>` in the program's csproj. Then
  `pprof.StartCPUProfile`, `testing`'s `-test.cpuprofile` and `net/http/pprof`'s `/debug/pprof/profile`
  all record samples. Without the property they return a valid, empty profile, as they do today.
- **The -tests host:** the converter emits the property into the generated test csproj for a table of
  packages, starting with `runtime/pprof` and `net/http/pprof`. The table is explicit on purpose. Every
  test binary imports `runtime/pprof` through `testing/internal/testdeps`, so an import-closure rule
  would opt in every package and put `TraceEvent` in every test host. A later row whose tests profile
  CPU joins the table.

### 11.4 Where it falls back

- **`DOTNET_EnableDiagnostics=0`:** the in-process session cannot open. `Start` catches the failure,
  and the profile completes with zero samples. It never throws.
- **Native AOT:** EventPipe's SampleProfiler support was not measured (9.3). The companion treats a
  failed or empty session the same way.
- **The sampler thread itself:** it runs in the companion, not in Go code, so it has no Go frames.
  Its own samples are dropped. Go has no such thread; the signal handler samples in place.

### 11.5 Sizing the magnitude calibration (the one unknown)

`TestCPUProfileMultithreadMagnitude` compares the SUM OF ALL SAMPLES in the profile against the
process's user plus system time from `getrusage`, within 10% (40% on some builders). It also requires
samples in `cpuHog1`. The probe (9.1) read 0.77 to 0.93 of CPU time. **A cause, inferred and not
measured:** the SampleProfiler samples managed threads only. CPU spent on the runtime's native
threads (JIT and tiered compilation, and the GC's background threads) is in `getrusage` and in no
sample.

Two calibrations, each with its falsifier:

1. **Time-weighted thinning alone (9.2 step 4).** Keep only Managed-state samples and weight each at
   `1/hz`, thinned per thread by elapsed time. This is honest about what was sampled, and it misses
   the native-thread share. *Falsifier:* if the probe's ratio stays below 0.90 with the hog on 1 and
   4 threads, thinning alone cannot hold 10%.
2. **Thinning plus a remainder record.** At stop, take the process's CPU delta over the profile
   (`getrusage` on unix, `GetProcessTimes` on windows), subtract the sampled total, and write the
   difference as one extra record. Go already writes such records with pseudo-frames:
   `runtime._ExternalCode` and `runtime._System` from `addExtra`. The total then matches `getrusage`
   up to rounding, as Go's own profile does, because Go counts non-Go time too.
   *Risk:* a sampler that undersamples the hog hides inside the remainder. The guard is a GolibTests
   arm that asserts the remainder stays under a stated share of the total on the hog probe.
   *Falsifier:* if the share exceeds that bound on an idle-free hog run, the sampler is missing hog
   time, not native time.

**Proposal:** measure 1 first, since it costs one probe run per thread count. Take 2 only if 1 fails
its falsifier. The measurement is the calibration cut's red arm. **Effort:** medium, as 9.3 said. The
remainder record is small; the unknown is how stable the share is under parallel load, which 9.5
already lists as unmeasured.

### 11.6 Cuts, red first, in order

| cut | piece | red arm | moves | reviewer |
|---|---|---|---|---|
| C1 | `runtime` seam: interface, slot, register, setter calls, drain writer | GolibTests: a fake sampler registered through the seam gets `Start(hz)` and `Stop`, and its records reach `readProfile` | none (a fake) | R |
| C2 | companion project, the property and `.targets`, the -tests table, session lifetime, the 11.4 fallbacks | a program built WITHOUT the property carries no companion (its output directory lists none); `DOTNET_EnableDiagnostics=0` gives a valid empty profile | none | R, the owner (dependency) |
| C3 | frame resolution and the synthetic-PC join | `TestCPUProfile` on linux | `TestCPUProfile`, `TestCPUProfileMultithreaded`, `TestMathBigDivide` (predicted, 9.4) | R |
| C4 | label event and join (`runtime_setProfLabel`) | `TestCPUProfileLabel` | it and `TestLabelRace` (predicted) | R (label seam) |
| C5 | magnitude calibration (11.5) | the probe ratio arm, then `/serial` and `/parallel` | the two magnitude rows (uncertain, 9.4) | R |

`TestCPUProfileRecursion` and `TestTimeVDSO` stay uncertain on JIT inlining (9.4). Section 10's ruling
makes an inlined-frame miss structural, so each is read once C3 lands and is classed from that
reading. `TestMorestack` and `TestLabelSystemstack` do not move (9.4).

### 11.7 Not measured, stated

- The `runtime.dll` size delta of the seam (11.3) is an estimate. C1 measures it.
- The native-thread cause of the 0.77 to 0.93 ratio (11.5) is inferred, not measured.
- Native AOT and Windows/darwin session behavior: as 9.5.
- The `.targets` import in the converter's csproj templates is designed, not written. C2 is where its
  corpus footprint is measured: every emitted csproj gains one `Import` line or none, depending on
  whether the templates already import a shared targets file.

## Appendix — the per-row list (linux, master `db1bd885a2`)

`runtime/pprof`, Go=pass for every row below.

| row | C# at master | C# with probe A | class |
|---|---|---|---|
| TestAtomicLoadStore64 | infrastructure-error (`rt_sigaction`) | pass | A |
| TestCPUProfile | fail (already in use) | skip | A → C |
| TestCPUProfileLabel | fail | skip | A → C |
| TestCPUProfileMultithreadMagnitude | fail | pass | A |
| TestCPUProfileMultithreadMagnitude/parallel | fail | skip | A → C |
| TestCPUProfileMultithreadMagnitude/serial | fail | skip | A → C |
| TestCPUProfileMultithreaded | fail | skip | A → C |
| TestCPUProfileRecursion | fail | skip | A → C |
| TestCPUProfileWithFork | fail | pass | A |
| TestGoroutineSwitch | fail | pass | A |
| TestLabelRace | fail | skip | A → C |
| TestLabelSystemstack | fail | skip | A → C |
| TestMathBigDivide | fail | skip | A → C |
| TestMorestack | fail | skip | A → C |
| TestTimeVDSO | fail | skip | A → C |
| TestTracebackAll | fail | pass | A |
| TestBlockProfileBias | fail (saveblockevent refusal) | fail | B |
| TestMutexBlockFullAggregation | fail (no mutex / block samples) | fail | F |
| TestMemoryProfiler, /debug=1, /proto | fail (empty profile) | fail | M |
| TestHeapRuntimeFrames | fail (no growMap sample) | fail | M |
| TestGenericsHashKeyInPprofBuilder | fail (empty profile) | fail | M |
| TestGenericsInlineLocations | fail (empty profile) | fail | M |
| TestCPUProfileInlining | skip (inlining probe) | skip | I |
| TestTryAdd | skip (inlining probe) | skip | I |
| TestTryAdd/* (10 subtests) | absent | absent | I |

Withdrawn (host-fatal, class S): TestBlockMutexProfileInlineExpansion (3 Go rows), TestBlockProfile
(3), TestMutexProfile (4), TestMutexProfileRateAdjust (1), TestProfileRecordNullPadding (6),
TestProfilerStackDepth (5). Gated: TestFakeMapping (M).

`net/http/pprof`, Go=pass for every row below.

| row | C# at master | C# with probe A | class |
|---|---|---|---|
| TestHandlers//debug/pprof/profile?seconds=1 | infrastructure-error (`rt_sigaction`) | pass | A |
| TestHandlers//debug/pprof/trace | fail (500, no execution tracer) | fail | T |
| TestHandlers | fail (parent) | fail | T |
| TestDeltaProfile | skip ("mutex profile is not working") | skip | F |

-- P2, 2026-09-26
