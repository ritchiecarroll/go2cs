# DESIGN — managed profiling: sizing the `runtime/pprof` and `net/http/pprof` rows

> **⚠ 2026-10-13: SUPERSEDED.** Both rows this note sizes have since banked: `net/http/pprof` at
> 15 + 0 on 2026-09-28 (`c753fb2b71`), and `runtime/pprof` at 145 + 7 on 2026-10-01 with TRAIN K
> (`133ca704eb`). Class C did not stay an owner decision: a CPU sampler was built, an in-process
> EventPipe session (`src/core/runtime/cpusampler_impl.cs`, guarded by
> `src/tests/GolibTests/EventPipeSamplerTests.cs`). The current counts and disclosures are on the
> roster, [`../ValidatedTestPackages.md`](../ValidatedTestPackages.md). This note stays on the record for
> its divergence mechanisms (§2) and increment shapes (§4). Its readings, predictions and asks are
> as of 2026-09-26 and are not current.

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

Class A's 16 = `TestAtomicLoadStore64` (the infra-error, first alphabetically) plus 15
"cpu profiling already in use". It is the same leak ruling `f545b18d4d` described on Windows, with a
linux root. **The registry comment at `src/go2cs/manualTypeOperations.go:651-662` states that "the linux
and darwin bodies are signal/timer based and stay converted." Measured on linux, that is false:**
the converted `setProcessCPUProfilerTimer` reaches `getsig → sigaction → sysSigaction → rt_sigaction`,
a PartialStubGenerator throw, from `SetCPUProfileRate` after `cpu.profiling` is set.

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
| **C** no CPU sampler | 11 (after I1) | A profile starts, stops and parses with zero samples; Go's `testCPUProfile` then skips via `CPUProfilingBroken`. Owner ruling #1 makes a Go=pass / C#=skip caused by OUR missing feature a feature gap, never a disclosure. | **OWNER.** Either (a) rule the CPU sampler out of scope for the managed runtime (as E4 did for the tracer), which makes these 11 disclosable; or (b) commission a sampler. (b) is possible in principle through an in-process EventPipe session with the SampleProfiler provider (a new package dependency, CLR stacks mapped to Go frames); it is not sized here and would be the largest item in either row. |
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
