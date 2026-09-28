# CENSUS — runtime's divergences by root-cause family at the TRAIN G union (read-only, 2026-09-28)

> **Record type:** CENSUS (point-in-time, read-only). Amend with dated blocks; never rewrite; never execute from.
> **Lane:** G (G-LAPTOP and its WSL linux arm). **Asked by:** COORD, after the union's linux row was recorded MET
> (ledger 2026-09-28 00:06).
> **Read at:** the TRAIN G union `claude/coord-trainG` `e6fc210500`, the FULL linux runtime row (WSL, Release,
> TC0, `-test-timeout 120m`, cgo off), 10,883 results. Windows is a **PROXY**, stated: G's own windows row at
> the sink guard `c6af425062` (base `4fb6e460c6`, before TRAIN F and TRAIN G), set against G's linux row at the
> **same tree** so that "what windows adds" is a same-tree difference; the i7 battery's windows union row
> supersedes it when it lands.
> **Method:** every one of the row's 228 test divergences (the 229th error line is the host's command line) was
> read from its OWN C# failure text in the results file, then grouped by MECHANISM, not by test name. Where a
> mechanism is inferred rather than read at the code, the family says so.

## 0. Totals

| Bucket | Families | Rows | What it takes |
|---|---:|---:|---|
| (a) fixable, OWNED | 5 | 25 | R's B panic frames (re-cut pending); P2's D + E line and start-line work, and class F (held); C1's runtime.Error sizing then its cut; P1's /cpu/classes residual (held, P1 out) |
| (a) fixable, UNOWNED | 11 | 99 | 11 seats; one of them (A16, the page allocator's L2 chunk block) carries 75 rows alone |
| (b) disclosable under the bar | 7 | 53 | one manifest seat, but B1's 25 alloc rows each owe a READING before any label (the alloc-meter rules) |
| (c) needs a ruling | 5 | 51 | five rulings, each of which becomes a seat (implement or disclose) |
| **total** | **28** | **228** | 220 Go-pass/C#-fail, 5 Go-pass/C#-infrastructure-error, 2 Go-skip/C#-pass, 1 Go-pass/C#-skip |

**The other two packages left for 100% (section 5):** runtime/pprof has 9 divergences and net/http/pprof has 1, all
at the union. Both bank on P2's held work (class F, M4, /serial) plus one 3-entry disclosure. **P2's class F is the
highest-leverage single seat in the three packages:** it touches runtime (A3), runtime/pprof and net/http/pprof's
only divergence.

**Seats to a bankable linux runtime row: about 23.** That is 17 fixing seats (6 owned, because A2 is P2's D and E;
11 unowned), one disclosure seat for bucket (b) after its alloc readings, and the 5 ruling outcomes. Three
families carry 124 of the 228 rows (54%): A16 (75), B1 (25) and C3 (24).

**What windows adds, on the proxy: 14 rows in 3 families, so 2 seats plus 5 disclosures** (section 4). Linux has
11 rows windows does not, and 10 of them are unix-only tests (the debug-call family, clone, getpid, a linux-only
memmove test, the unix goroutine profile). The 11th, TestPseudoRandomSend, failed at `4fb6e460c6` and
`c6af425062` and PASSES at master `e798b3e7fb` and at the union, so TRAIN F cleared it. It is not in the union's
set.

The row also carries 7 matched disclosures (TestCaller, TestCrashWhileTracing, TestEmptySlice, TestEmptyString,
TestFunctionAlignmentTraceback, TestPanicSystemstack, TestPinnerConstStringData), 22 identical skips, and 297
excluded declarations (benchmarks and examples). None of those are counted below.

## 1. (a) Fixable

### Owned

| # | Family | Rows | Mechanism (one line) | Evidence (test: C# failure text) | Owner / size | Blocks banking? |
|---|---|---:|---|---|---|---|
| A1 | Panic-path frames | 11 | `runtime.Callers` from a deferred call during a panic lacks Go's `gopanic` / `panicmem` / `sigpanic` / `panicdivide` / `deferreturn` frames and the panicking wrapper frame | TestCallersNilPointerPanic: `wanted [... func1 runtime.gopanic runtime.panicmem runtime.sigpanic ...], got [... func1 runtime_test.TestCallersNilPointerPanic testing.tRunner]`; TestStackWrapperStackPanic/sigpanic/CallersFrames: `panicking wrapper runtime_test.I.M missing from stack trace` | R's B (NOT ACCEPTED as it stands, ledger 2026-09-28 00:01; re-cut pending) / L | yes |
| A2 | Line numbers and start lines | 9 | Composite-literal and multi-line positions collapse to the statement's first line; `runtime.Frame` reports every caller as INLINED and inline frames carry start line 0; one end-line off by one | TestLineNumber: `lineVar1 on firstLine+0 want firstLine+2`; TestStartLine/normal: `caller runtime_test.normalFunc inlined got true want false`; TestStartLine/inline: `start line got 0 want 49`; TestCallersEndlineno: `callerLine(1) returned 322, but want 323` | P2's D (TestLineNumber) and E (*Func / start lines), HELD for P2's return / M + M | yes |
| A3 | Runtime-lock contention in the mutex profile | 3 | runtime/metrics sees lock contention time and runtime/pprof's mutex profile sees none | TestRuntimeLockMetricsAndProfile/runtime.lock/sample-1: `no increase in mutex profile lock contention growth in runtime/pprof's view (0.000000s) ... runtime/metrics' view (0.338442s)` | P2's class F (mutex-block aggregation), HELD / M | yes |
| A4 | runtime.Error factories | 1 | Runtime panics recover as plain strings, not values implementing `runtime.Error` | TestRuntimePanicWithRuntimeError (infrastructure-error): `recovered value assignment to entry in nil map(type string) does not implement runtime.Error` | C1 sizes, then P1's cut (held) / M | yes |
| A5 | GC CPU-time classes | 1 | `/cpu/classes/gc/{mark/assist,mark/dedicated,mark/idle}` stay 0; only the pause class is fed | TestReadMetricsConsistency: `found no time spent on GC work: ... gcAssist:0, gcDedicated:0, gcIdle:0, gcPause:231.105374` | P1's /cpu/classes residual (P1 out: effectively unowned) / S-M | yes |

### Unowned

| # | Family | Rows | Mechanism (one line) | Evidence (test: C# failure text) | Size | Blocks banking? |
|---|---|---:|---|---|---|---|
| A6 | Traceback printer shows CLR frames | 9 | The printed traceback carries host frames (`System.Threading.ExecutionContext.RunInternal()`) that the tests' exact frame lists and elision counts reject | TestTracebackInlined/simple, TestTracebackElision/elided=0: `missing source line (line 12): System.Threading.ExecutionContext.RunInternal()` | M (the traceback printer; coordinate with R's B, which owns frame shaping) | yes |
| A7 | Traceback decoration | 2 | A generic function prints without Go's `[...]`, and a goroutine's `created by ... in goroutine N` line is missing its parent id | TestTracebackGeneric: `want "testTracebackGenericFn[...](", got ... runtime_test.testTracebackGenericFn()`; TestTracebackParentChildGoroutines: `did not see parent (goroutine N) and child (goroutine M) IDs in stack` | S | yes |
| A8 | A panic inside the runtime package must be fatal | 2 | Go's panicCheck1 turns a bounds or nil fault raised IN the runtime package into a throw that `recover` cannot catch; here it is an ordinary recoverable panic, so the re-exec child exits 0 | TestRuntimePanic, TestTracebackRuntimeMethod: `child process did not fail` (child: `runtime.PanicForTesting(nil, 1)` recovered, and `(*runtime.Func)(nil).Entry()`) | M (a golib test of the raising frame's package on the panic path only) | yes |
| A9 | runtime.GoroutineProfile refuses | 2 | `goroutineProfileWithLabels` refuses by name, while runtime/pprof's goroutine profile already has a managed body to route to | TestGoroutineProfile, TestGoroutineProfileTrivial (and TestSchedPauseMetrics/runtime.GoroutineProfile, counted in C2): `goroutineProfileWithLabels: ... neither exists in the managed model (runtime/pprof's goroutine profile has its own managed body)` | S | yes |
| A10 | A zero-constructed defined struct loses its array | 1 | `type TimeHistogram timeHistogram` constructed with `new TimeHistogram()` leaves the `counts [N]atomic.Uint64` field unallocated (length 0). This is a golib / generator shape and may reach other packages | TestTimeHistogram: `index out of range [1] with length 0 at go.array'1.get_Item ... FieldRefWrappers ... getFieldRef` | S (generator or golib), plus a corpus census of the shape | yes |
| A11 | Cleanup runs before the finalizer | 1 | Go runs an object's finalizer first and its cleanup only once the object is unreachable again; here the order is reversed | TestCleanupAfterFinalizer: `result 2, want 1` / `result 1, want 2` | S | yes |
| A12 | Re-exec child shape | 2 | The `-test.v` child's output lacks Go's `PASS\n` line; `go run mklockrank.go` runs from the converted package directory, outside GOROOT, where Go's internal-import rule refuses it | TestFinalizerRegisterABI: `... PASS TestFinalizerRegisterABI PASS (exit status <nil>)`; TestLockRankGenerated: `mklockrank.go:16:2: use of internal package internal/dag not allowed` | S (the testing host; the i9's area). TestLockRankGenerated's working-directory half may need a ruling | yes |
| A13 | Map iteration is not randomized | 2 | The map iteration order is the same on every range; Go randomizes the starting bucket and offset per iteration | TestMapIterOrder: `Map with n=3 elements had consistent iteration order: [0 1 2]` | S (golib map). State its cost on the map benchmarks | yes |
| A14 | Untyped-constant arithmetic past 32 bits | 2 | A constant expression above 2^31 comes out wrong. TestGCCPULimiter's two sides PRINT EQUAL (14,000,000,000), so the fault is the RHS `(uint64)(procs * CapacityPerProc)`, an `UntypedInt * UntypedFloat` in golib. TestMemmoveOverflow's `unsafe.Slice(p, 3<<30)` reads a negative length. The root is LOCATED, not yet rooted; the second test is inferred to share it | TestGCCPULimiter: `Iteration 1 unexpected capacity: 14000000000`; TestMemmoveOverflow (linux only): `panic: len is negative at go.unsafe_package.Slice` | S. TestMemmoveOverflow then meets a SECOND blocker: a copy over a native window of more than 2^31 bytes | yes |
| A15 | No forced periodic GC | 1 | Go's sysmon forces a GC after `forcegcperiod`; with the period set to 0, `NumGC` never moves | TestPeriodicGC: `no periodic GC: got 0 GCs, want >= 2` | S-M (a sysmon-shaped timer) | yes |
| A16 | Page allocator L2 chunk blocks | **75** | `pageAlloc.grow` stores a `sysAlloc`'d L2 block into `p.chunks[l1]` through a `uintptr` (mpagealloc.go:402-420). `pallocData`'s `[8]uint64` fields are managed `go.array` values, so the native view over that block is refused | TestPageAllocAlloc/StraddlePallocChunkPages*5/4 (and all 74 others): `native-backed slice: element type pallocData contains managed references and cannot alias native memory at go.slice'1.OverNativeMemory` | M: a hand-own of the L2 allocation as a managed `[N]pallocData` (only the test allocators reach it). ⚠ This is the FIRST blocker only; an unmasking probe must size what is behind it before the seat is predicted | yes |

## 2. (b) Disclosable under the exclusion bar

Each class named here is an existing manifest class. The bar refuses "merely hard, unimplemented, or expensive",
and each family below states why it is none of those.

| # | Family | Rows | Mechanism (one line) | Evidence | Class | Why it is not merely unimplemented |
|---|---|---:|---|---|---|---|
| B1 | Escape analysis and allocation counts | 25 | Go's compiler keeps these conversions, concatenations and maps off the heap; the CLR boxes or allocates. 14 are TestZeroConvT2x's zero-value conversions | TestZeroConvT2x/E16: `AllocsPerRun measured 24,000 allocated BYTES over 1,000 run(s) ... charged none of it`; TestConcatTempString: `counted 2,000 go2cs-runtime object allocations (88,000 bytes) over 1,000 run(s)` | alloc-count-semantics, DEFERRED or STRUCTURAL, per entry | ⚠ NOT YET: the label is decided by the METER, per entry. Each of the 25 owes its reading (bytes versus count, the per-entry unit note), and ruling #1 forbids disclosing a want-zero that is satisfiable in principle. Until each reading is taken, these rows are (b)-candidates, not (b) |
| B2 | Compiler inlining decisions | 1 | The test asserts that Go's compiler INLINED a function; go2cs keeps Go's function boundaries one-for-one | TestStackWrapperStackInlinePanic: `inlinablePanic not inlined` | runtime-capability | Keeping function boundaries one-for-one is the project's chosen design, so the concept is absent for every function |
| B3 | No Go machine code | 2 | An assembly function has no body, and `objdump` of the test binary finds no Go text | TestStartLineAsm (infrastructure-error): `AsmFunc: no implementation reached this compilation`; TestUnsafePoint: `can't objdump exit status 1` | runtime-capability (TestFunctionAlignmentTraceback is the precedent) | Assembly never converts, and the binary is a CLR assembly |
| B4 | Frame data the CLR walk does not carry | 2 | A traceback's argument words, and an inline tree at a PC | TestTracebackArgs: `want "testTracebackArgs1(0x1, 0x2, 0x3, 0x4, 0x5)"`; TestInlineUnwinder: `failed to resolve tiuTest at PC 0xffff800000022000` | runtime-capability | No function has either: frames carry no argument words, and nothing is inlined, so no function has an inline tree |
| B5 | Debugger call injection | 6 | `debugCallV2` injects a call into a stopped goroutine by signal and register state; no thread id resolves | TestDebugCall and 5 siblings (linux only): `missing tid` | runtime-capability | The protocol is a register-level ABI on Go machine code |
| B6 | No Go scheduler or stack substrate | 7 | Raw OS thread creation, the g0 stack, stack shrinking and moving, systemstack frames, signalling a specific M, the runtime netpoller | TestNewOSProc0 (infrastructure-error): `clone: no implementation`; TestG0StackOverflow: `GetCallerSP: no implementation`; TestSystemstackFramePointerAdjust: `shrinkstack: goroutines are CLR threads with no Go stack to shrink`; TestGCTestMoveStackOnNextCall: `old stack pointer X, new stack pointer X`; TestTracebackSystemstack (infrastructure-error): `GetCallerPC: no implementation`; TestNetpollBreak: `the managed host has no runtime poller to break`; TestSignalM (infrastructure-error): `getpid: no implementation` | runtime-capability | The subjects are the replaced representation: goroutines are CLR threads and I/O is the CLR's. TestSignalM is the weakest member (a thread-directed signal is expressible), so it is argued per entry |
| B7 | No Go heap layout | 10 | Heap arenas, spans and size classes, the tiny allocator, GC pointer bitmaps, swiss-map groups, and hashing a pointer-bearing value's bytes at Go layout offsets | TestArenaCollision, TestGCTestPointerClass, TestLFStack(+Stress), TestStringW: nil dereference in `spanOf` / `KeepNArenaHints` / `mallocgcTiny`; TestTinyAlloc: `no bytes allocated within the same 8-byte chunk`; TestGCInfo: `bad GC program for bss eface: want [0 1] got [1 1]`; TestGroupSizeZero: nil dereference in the map group size; TestSmhasherAvalanche: `dereference of a managed pointer with no address (*eface over ... the order token ...)` | runtime-capability | The CLR heap has no Go spans, classes or bitmaps. LFStack is the weakest member (a token-packed lock-free stack is expressible; `lfnodeValidate` asks the Go heap), so it is argued per entry |

## 3. (c) Needs a ruling

| # | Family | Rows | Mechanism | Evidence | The question |
|---|---|---:|---|---|---|
| C2 | What stopTheWorld means in the managed host | 18 | `stopTheWorld` refuses, so every test-only "slow" export that stops the world panics, and `/sched/pauses/*` never gets a sample | TestDebugLog and 5 siblings: `stopTheWorld: the managed host cannot stop the world (reason: ResetDebugLog (test))`; TestReadMemStats, TestReadMetrics, TestPageAccounting, TestPageCacheLeak: the same refusal; TestSchedPauseMetrics/runtime.GC: `/sched/pauses/stopping/gc:seconds sample count 0 did not increase` (plus its WriteHeapDump and GoroutineProfile subtests) | Is a stop-the-world a global goroutine barrier, a coarse lock with pause accounting, or a refusal? The debuglog six probably pass under any non-refusing model. The slow-versus-fast MemStats comparisons need C6's answer as well |
| C3 | User arenas | 24 | `newUserArena` refuses: there is no Go heap to carve arena chunks from | TestUserArena/Alloc/[]int_(cap_0) and 23 more: `newUserArena: the managed host has no Go heap to allocate user arena chunks from` | Implement arenas over managed chunks (allocation, clone and liveness are expressible; the fault-on-access-after-free is not), or disclose? This is the arena GOEXPERIMENT's surface |
| C4 | The hash algorithm (AES) | 5 | Go uses `aeshash` on AES hardware; the converted runtime always runs the fallback memhash. So Go SKIPS the two equality tests where we pass, and the `aes` subtest SKIPS here where Go passes | TestMemHash32Equality, TestMemHash64Equality: Go skip, C# pass; TestMemHashGlobalSeed/aes: C# `No AES`; /noaes: `cmd.Output got err exit status 1` | Implement aeshash with System.Runtime.Intrinsics (it is bit-reproducible), or treat the CPU-feature condition as a platform condition? (Manifest rule 13 requires a platform condition in Go's own source) |
| C5 | Recorded source-path form | 3 | The re-exec child opens its own source through `runtime.Caller`'s file, which is recorded relative (`runtime/traceback_system_test.go`) where Go records the absolute GOROOT path | TestTracebackSystem/panic: `panic: open runtime/traceback_system_test.go: no such file or directory` | Does the position map record absolute GOROOT paths (a pushed-surface identifier question), or does the host resolve the relative form against GOROOT when opening? |
| C6 | MemStats fields with no managed source | 1 | Twelve MemStats fields read 0 | TestMemStats: `Mallocs = 0: zero value ... MSpanInuse = 0 ... GCSys = 0 ... OtherSys = 0` | Which fields may be synthesized (Mallocs and Frees from golib's counter) and which are representational (MSpan*, MCache*)? The test asserts ALL of them non-zero, so a partial answer still fails the row |

## 4. What windows adds (proxy: `c6af425062`, same tree as linux)

Windows reads 255 divergences and linux 252 at that tree. Windows has 14 the linux row does not:

| # | Family | Rows | Mechanism | Evidence | Bucket / size |
|---|---|---:|---|---|---|
| W1 | Windows callbacks through syscall | 8 | The generated wrapper hands a reference-bearing argument to native code as a managed pointer token, and the marshal refuses it by name, as designed | Test64BitReturnStdCall, TestCallback, TestCallbackGC, TestBlockingCallback, TestCallbackPanic{,Locked,Loop}, TestRegisterClass: `syscall: argument N is a managed pointer token, not an address ... Hand-own this wrapper against a blittable mirror` | (a) unowned, M (hand-owned wrappers against blittable mirrors, as the refusal names) |
| W2 | SEH unwinding of Go frames | 5 | Go's SEH function-table lookup over its own machine code | TestSehLookupFunctionEntry, TestSehUnwind{,DoublePanic,NilPointerPanic,Panic} (infrastructure-error): `GetCallerPC: no implementation` | (b) runtime-capability |
| W3 | TestNumCPU | 1 | `asmcgocall` has no body | TestNumCPU (infrastructure-error): `asmcgocall: no implementation reached this compilation` | (a) or (b); unread, S |

So windows adds about 2 seats (W1, W3) and 5 disclosures (W2). The i7 battery's full windows union row replaces
this proxy. The proxy predates TRAIN F and TRAIN G, so the 19 moves TRAIN G made on linux may also move on windows.

## 5. runtime/pprof and net/http/pprof (linux, at the union)

Both rows were read at the union `e6fc210500` on WSL (Release, TC0, `-test-timeout 60m`). **runtime/pprof:** 154
results; C# 142 pass / 5 fail / 7 skip; 8 matched disclosures; **9 divergences** (5 Go-pass/C#-fail, 4
Go-pass/C#-skip). **net/http/pprof:** 15 results; 14 pass / 1 skip; **1 divergence**.

| # | Family | Rows | Mechanism | Evidence | Bucket / owner |
|---|---|---:|---|---|---|
| P-a | Mutex and block profile aggregation | 3 | The mutex and block profiles record no, or mis-attributed, samples. This is the same root as runtime's A3 | runtime/pprof TestMutexBlockFullAggregation: `did not see any samples in mutex profile for this test`; TestBlockProfileBias: `block profile is missing expected functions`; net/http/pprof TestDeltaProfile (Go pass, C# skip): `mutex profile is not working` (manifest rule 13 already ruled this skip NOT disclosable) | (a) P2's class F, HELD. One seat clears net/http/pprof's only divergence and two of runtime/pprof's |
| P-b | Heap-profile frames and generic shapes | 3 | The heap profile lacks the allocating function's leaf frame and Go's `[go.shape.T]` instantiation names | TestGenericsHashKeyInPprofBuilder: `want = ...;runtime/pprof.genericAllocFunc[go.shape.uint64] [1 64 0 0]`; TestGenericsInlineLocations; TestHeapRuntimeFrames | (a) P2's M4 (frames), HELD |
| P-c | CPU-profile magnitude | 1 | Profile CPU time and rusage differ by more than Go's 10% limit, so Go's own "ignoring failure on linux/amd64" skip fires | TestCPUProfileMultithreadMagnitude/serial: `CPU usage reports are too different (limit -10.0%, got -12.1%)` | (a) P2's /serial, HELD |
| P-d | CPU-profile frames that do not exist here | 3 | The expected frame is an inlined callee, `runtime.systemstack`, or `runtime.newstack`. None exists in the managed model, so Go's issue-13841 skip fires | TestCPUProfileRecursion: `inlinedCallee has 0 samples out of 32`; TestLabelSystemstack: `no samples in expected functions runtime.systemstack;key=value`; TestMorestack: `no samples in expected functions runtime.newstack,runtime/pprof.growstack` | (b) runtime-capability (same reasoning as B2 and B6). ⚠ Go's skip lists only some OSes, so on windows these rows FAIL rather than skip: the disclosure must be read on both |

So **runtime/pprof banks after P2's class F, M4 and /serial plus one 3-entry disclosure**, and **net/http/pprof
banks after class F alone**. Class F is the one seat that moves all three packages at once (runtime A3,
runtime/pprof P-a, net/http/pprof TestDeltaProfile).

## 6. Not measured here

- Anything behind a family's first blocker: A16 above all, and TestMemmoveOverflow's second blocker.
- A14's root (located at the `UntypedInt * UntypedFloat` right-hand side, not read in golib).
- B1's per-entry meter readings, which decide whether each alloc row is disclosable at all.
- The windows union row (the proxy predates TRAIN F and G).
- Darwin.
