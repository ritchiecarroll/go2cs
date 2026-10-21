# DESIGN — the goroutine-leak profile over the managed runtime (PROPOSED)

> **STATUS: PROPOSED, NOT RULED.** Written 2026-10-21 by C1 at COORD's request. The seat this note describes
> needs an owner ruling first: it adds a third-party runtime dependency, costs about 0.6 s plus a full-process
> dump per profile call, and has no Native AOT path. Until that ruling, `runtime/pprof`'s
> `TestGoroutineLeakProfileConcurrency` is filed as a **disclosure** for the re-bank (the text is in
> [The disclosure, and its retirement](#the-disclosure-and-its-retirement)). Nothing in golib lands for this note.
> Measured at hop tip `9924b9621c`, go1.26.10, on a linux box.

## What Go does

Go 1.26 adds a `goroutineleak` profile to `runtime/pprof` (`pprof.go:194`). Its writer, `writeGoroutineLeak`
(`pprof.go:789`), takes `goroutineLeakProfileLock`, runs `runtime_goroutineLeakGC` and then writes the
goroutines the runtime has marked leaked, through `pprof_goroutineLeakProfileWithLabels`. The detection lives
in Go's collector:

- `goroutineLeakGC` (`runtime/mgc.go:594`) sets `work.goroutineLeak.pending` and runs GC cycles until one
  of them has done leak detection.
- That cycle marks from the roots with the stacks of goroutines blocked on concurrency primitives **withheld**
  from the root set. A blocked goroutine whose blocking object turns out reachable is restored as a root, and
  marking repeats until nothing changes.
- The goroutines still unreached are moved to `_Gleaked` (`mgc.go:1318`), and `goroutineleakcount`
  (`proc.go:5720`) reports how many. The next detection cycle first resets them to `_Gwaiting`
  (`mgcmark.go:78`), so each profile call reflects the current state.

`TestGoroutineLeakProfileConcurrency` (`pprof_test.go:1581`) leaks six goroutines, each blocked forever on
`<-make(chan struct{})`, and then requires:

- **the leak set**: exactly 6, in sequential calls and in calls that overlap each other and the ordinary
  goroutine profile;
- **each leaked goroutine's interior stack**: `runtime.gopark`, `runtime.chanrecv`, `runtime.chanrecv1`,
  `runtime/pprof.goroutineLeakExample`, and optionally `runtime/pprof.TestGoroutineLeakProfileConcurrency.func4`
  (`pprof_test.go:1633-1657`).

## What go2cs does today

Both pushes are refused by name in the converter's `linknamePushTargets` (`src/go2cs/linknameOperations.go`).
`runtime_goroutineLeakGC` is refused because the pushed body would spin forever waiting for a cycle that never
runs. `runtime_goroutineleakcount` is refused because any count would be a fabricated 0. On linux the test
panics on the first refusal, inside `writeGoroutineLeak`. Its four subtests get no C# verdict.

## Why golib cannot answer it alone

A goroutine is a CLR thread, and the CLR's collector roots **every** thread's stack. The blocked goroutine's
own frames therefore keep its channel alive, which is exactly the root Go withholds. Two tricks were
considered and rejected:

- **Weak references to the blocking object.** Tier-0 code reports every local as live until the method
  returns, and shared generic code keeps `this` alive for its generic context. The channel stays reachable from
  the blocked frames whatever golib's own wait does.
- **Asking the collector.** The CLR exposes no reachability query, and no API removes one thread's stack from
  the root set.

The answer therefore needs an explicit walk of the heap, with roots attributed to the thread that holds them.

## The probe

ClrMD (`Microsoft.Diagnostics.Runtime` 4.2.751201, MIT) was run against its own process on the linux box.
The probe was a JIT console host with three threads each blocked on a private object (the leaked shape) and one
blocked on an object a static field also holds (the reachable shape).

| Step | Result |
|:--|:--|
| `DataTarget.CreateSnapshotAndAttach(own pid)` | works; `createdump` writes a full core file of the process (230,342,656 bytes for this small host) and reports it in about 0.9 s |
| `ClrInfo.CreateRuntime(<runtime dir>/libmscordaccore.so, ignoreMismatch: true)` | works with the DAC the runtime ships. The default lookup failed with "Could not find matching DAC", so the path must be passed |
| snapshot plus attach, end to end | 617 ms on the first round |
| `ClrThread.EnumerateStackRoots()` | attributes each blocked object to the thread that holds it. The reachable object also appears on the main thread, through a tier-0 local, which over-approximates reachability in the same direction Go's conservative scan does |
| `ClrThread.EnumerateStackTrace()` | walks a **foreign, blocked** thread's managed frames (`P.LeakExample(Chan)` under `System.Threading.Monitor.Wait`) |

The last row is also the walk the managed goroutine profile says it cannot do today (`runtime/managed_impl.cs`,
`goroutineProfileWithLabels`: "The managed runtime cannot walk a foreign thread's stack").

## The proposed shape

1. **Detection.** `runtime_goroutineLeakGC` runs inside the stop-the-world pair the other profiles use. It
   snapshots the process and marks from statics, handles and the stack roots of every thread that is **not** a
   parked leak candidate. The candidates are goroutines parked for one of Go's eligible reasons: channel send
   and receive, `select`, a nil channel, and the `sync` waits (Mutex, RWMutex, Cond, WaitGroup, semacquire).
   It then runs Go's fixpoint over the candidates' blocking objects, which golib's `Park` already records, and
   flags the candidates that stay unreached. Flags reset at the start of each detection, as Go resets
   `_Gleaked`.
2. **Count.** `runtime_goroutineleakcount` answers the number of flagged goroutines from the last detection.
3. **Records.** `pprof_goroutineLeakProfileWithLabels` returns the flagged goroutines with their snapshot
   stacks and labels. Converted frames are named through `GoPositionMap`, which already spells closures as
   `Outer.funcN`. golib's park path is named through a declared table that maps it to the Go runtime frames it
   stands for: `Goroutine.Park` to `runtime.gopark`, the receive entry and its body to `runtime.chanrecv1` and
   `runtime.chanrecv`, and likewise for send, `select` and the `sync` waits.
4. **Converter.** Both `linknamePushTargets` rows flip from refused to honored, against the hand-owned bodies.
5. **Packaging.** ClrMD lives in a separate assembly that only this profile loads, not in golib, so no other
   program carries the dependency or its load cost.

## Costs that need a ruling

- **A third-party runtime dependency**: ClrMD and what it pulls in, in a component converted programs can load.
- **About 0.6 s and a full-process dump on disk per profile call.** Go's cost is one GC cycle. The dump goes
  to the system temporary directory and scales with the process. The probe's was 230 MB, and a test host is
  larger. A host without room for it, or without `createdump`, cannot answer the profile.
- **No Native AOT path.** ClrMD needs the runtime's DAC, and a Native AOT program ships none. Under AOT the
  profile keeps its disclosure even after this seat lands.
- **Windows is not measured.** There the snapshot is `PssCaptureSnapshot` rather than `createdump`. The seat
  would measure it before claiming it.

## Size

Three to five lane-days:

- the snapshot driver;
- the mark fixpoint;
- the thread-to-goroutine map;
- the frame-name table;
- three hand-owns and two converter rows;
- gates on linux and windows.

It does not fit before the re-bank.

## Possible upside, not claimed

The same per-thread walk could lift `runtime/pprof`'s six `host-fatal` withdrawals, the tests that wait for
another goroutine's interior frames in `runtime.Stack(all)`. That would need its own measurement; this note
does not claim it.

## The disclosure, and its retirement

Filed for the re-bank (COORD, 2026-10-21):

> `TestGoroutineLeakProfileConcurrency`, **runtime-capability**: Go finds leaked goroutines with its own
> collector, which marks the heap with blocked goroutines' stacks withheld from the roots. The CLR's collector
> roots every thread's stack and offers no reachability query, so the managed runtime cannot tell which blocked
> goroutines are unreachable without a heap walk that golib does not have. The profile refuses by name rather
> than reporting a fabricated zero.

**Retirement plan:** the seat above, once the owner rules on its dependency and per-call cost. Native AOT keeps
the row until a walk exists that does not need the DAC.
