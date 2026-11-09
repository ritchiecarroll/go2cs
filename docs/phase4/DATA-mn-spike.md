# DATA — the M:N spike (R5): hidden async twins, measured

> **STATUS: RECORD (lane G, 2026-11-09; branch `claude/g-mn-spike-record`, off master `58cfeb3d0f`).** It is the evidence for
> [DESIGN-mn-scheduler](DESIGN-mn-scheduler.md) ruling R5 ("schedule the S2 spike, outside the `.4` release goals").
> The spike's code and raw results live on the spike branch line only, never on master:
> - `claude/g-mn-spike-p0p1` `b328b55d08`: P0 and P1;
> - `claude/g-mn-spike-p2` `404c18a5ae`: P2;
> - `claude/g-mn-spike-p3` `490e08670a`: P3;
> - `claude/g-mn-spike-p4` `dff452c7c1`: P4.
>
> Each phase lives under `src/tools/mnspike` at its ref, with `src/tools/mnspike/results/` holding every reading this
> record quotes. Host: G-LAPTOP (Windows x64, 12 logical CPUs, 30.8 GB); go1.26.10; .NET 10.0.400. The macOS readings
> come from COORD's dispatch of the hosted runners. Amended by dated blocks, never rewritten.

## 1. The answer

| Gate | What it asks | Today (O0) | The twin prototype | Verdict |
|---|---|---|---|---|
| **G1** (COORD's stop line) | the can-park share of functions under 60%, and no common park path left untwinned | — | at most 33.3% / 33.7% (windows / linux); 0 hard refusals among the direct parkers | **SILENT**: the spike proceeds |
| **G2** | 10^5 parked on the hosted osx-arm64 runner | dies at 2,028 (cap 2,048); osx-x64 at 3,052 (cap 3,072) | 100,000 parked in 0.11 s on 3 carriers (osx-arm64); 0.43 s on 7 (osx-x64) | **GREEN** (§5) |
| **G3** | 10^6 parked on Windows under 20 GB | NEVER PARKED: 20 GB at 617 s, 300,000 started | 0.95-1.04 s, 498-519 MB, 25-31 threads. **Caveat in the line: lighter than Go's 8,551 MB only because ParkStorm's goroutine body is trivial; a real twin holds its live locals, against Go's 2 KB starting stack** | **GREEN** |
| **G4** | the delegate-table cost | — | lookup +13 to +38 ns per dynamic call; **a func literal's twin registration +609 ns per creation**, against a ~950 ns park/resume round trip | **QUANTIFIED**: a cost, not a showstopper |
| **G5** | a breakpoint in the visible `.cs` binds inside its twin | MISSES: 4 of 4 lines unbound without `#line` | BINDS: 4 of 4 with `#line` | **GREEN** by the PDB proxy, proven both ways; **manual check pending** (an owner or IDE item, offered by COORD) |

## 2. P0: every gate red on O0 first

- **G3:** today's converted ParkStorm at 10^6 (`parkstorm/`, the sizing note's Appendix A program; `Measure-ParkStorm.ps1`,
  which stops a run past 20 GB or 600 s) reached STOPPED-MEMORY at 617 s, holding 20,480 MB and 360,269 threads with
  only 300,000 goroutines started. That matches the sizing note's R-LAPTOP reading.
- **G5:** `twinlines/` reads its own portable PDB. A hand twin without `#line` leaves every breakpoint-able visible line
  without a twin sequence point. The same instrument on the `#line` twin reads all four bound, so it reads both ways.
- **G2:** C1's run 42497561630 (the first failing `go` at 2,027 on osx-arm64, 3,051 on osx-x64). The P4 probe re-reads it
  in the same job as the twin (§5).

## 3. P1: which functions can park (`canpark`, x/tools v0.52: go/ssa + callgraph/vta)

The leaves are the park inventory's rows 1-7 and 9 ([cooperative scheduler §6](DESIGN-cooperative-scheduler.md)), in Go
terms: channel send and receive, a blocking `select`, `sync`'s semaphore family and `Cond`, `time.Sleep`, and
`internal/poll`'s waits. The runtime family neither parks nor propagates (rows 11-12, converted-dead in go2cs).

| std, runtime excluded, long parks only | windows | linux |
|---|---|---|
| **UPPER** (through VTA's interface and func-value edges) | 5,223 / 15,702 = **33.3%** | 5,266 / 15,609 = **33.7%** |
| **LOWER** (static calls only) | 597 = 3.8% | 716 = 4.6% |
| `go` statements that can park (upper) | 105 / 118 | 103 / 116 |
| can-park interface call sites (upper; unresolved) | 974 / 3,944 (508) | 907 / 3,957 (508) |
| can-park func-value call sites (upper; unresolved) | 639 / 2,200 (360) | 637 / 2,203 (363) |

**How to read the bounds.**
- The upper bound is sound but context-insensitive: `sync.Once.Do`'s parameter merges every closure passed to it,
  and `fmt.Fprintf`'s `io.Writer` merges every writer. That inflates pure libraries: timefmt-go reads 38.1% upper and
  0% lower.
- The lower bound misses real dynamic parks (`io.Copy` from a pipe).
- The twin set lies between the two, and is at most about 34%.
- Controls hold in both modes, with witness paths:
  - `strings.ToUpper`, `strconv.Itoa` and `sort.Ints` cannot park;
  - `(*sync.WaitGroup).Wait`, `(*net/http.conn).serve` and `(*os.File).Read` can.

**Two corrections to the instrument, both found by witnesses before any number was reported.**
1. Without the runtime rule, every allocation "parked" through `mallocgc` → `gcStart` → a channel receive.
2. The lower bound's witness walker ignored its own mode.

## 4. P1b: what a twin cannot be made for (`twinscan`, Roslyn over the emitted C#)

| Population | Matched to C# | HARD (no twin: the fallback) | Mechanical twin transforms |
|---|---|---|---|
| can-park set, windows | 4,966 / 5,223 | **8 (0.2%)**: `unsafe` 7, `stackalloc` 2, other `ref` 1 | boxed `ref` locals 1,992; `ref` params 1,323; defer frames 479; span params 204 |
| direct parkers (the common park paths), windows | 110 / 117 | **0** | boxed 69; defer 37; `ref` params 28 |
| can-park set, linux | 5,007 / 5,266 | 1 | boxed 1,986; `ref` params 1,323; defer 483; span params 205 |
| direct parkers, linux | 108 / 115 | **0** | boxed 67; defer 35; `ref` params 28 |

- A **boxed `ref` local** is `ref var b = ref Ꮡb.DerefOrNull()` or `ref heap<T>(..., out var Ꮡx)`. Its box exists, so the
  twin reads through it.
- The box shapes were taken from a census of every scanned method's `ref`-local initializers. A first, guessed pattern
  missed `heap<T>` and read 7.5% hard.

## 5. P4: G2 on the hosted macOS runners

`MnSpikeTwinParkProbe` is spike-branch GolibTests with the prototype compiled in by link. COORD dispatched it as
`stage=golib-tests goos=darwin`, filter `FullyQualifiedName~MnSpikeTwinParkProbe`, ref `claude/g-mn-spike-p4`.
- Run 42599838013 (`dff452c7c1`) leaked the budget arm's threads into the twin arm. Its lines also started `READ `,
  which the workflow does not keep (it keeps `READING `), so only a failed arm's line reached the log.
- The arms were isolated at `795155c67c`: each joins its threads back to its own starting count and asserts it.
- The table is run 42602551537, at that tip.

| Arm | osx-arm64 (3 Ps) | osx-x64 (4 Ps) |
|---|---|---|
| O0, a thread per goroutine, 10^5 | **RED**: 2,028 started, `kern.num_taskthreads` 2,048, OutOfMemoryException at the `go` statement, 1.08 s; threads 20 -> 84 after the join | **RED**: 3,052 started, cap 3,072, 20.52 s; 20 -> 87 |
| the twin, 10^5 | **GREEN**: 100,000 / 100,000 parked in 0.11 s on 3 carriers, 0 injections, 89 process threads | **GREEN**: 100,000 / 100,000 in 0.43 s on 7 carriers, 3 injections, 93 process threads |
| concurrent untwinned blocks before the OS refuses a thread | **1,963** of the arm's 4,000 (threads at max 2,048; 2 refused carrier starts; all finished; 88 -> 84) | **2,987** of 4,000 (threads at max 3,072; 4 refused; all finished; 89 -> 84) |

osx-x64's 3 injections came with no leaked threads, and GC pause is netted out. They are oversubscription during the
park storm on a 4-core runner: §8 item 1's case for measuring a carrier's own CPU time.

**The macOS thread budget** (COORD's ask). Every SYNCHRONOUS block holds one OS thread:
- an untwinned park (the fallback);
- a blocking native call, under any option (seam M1).

So a macOS process holds, at most:

> concurrent untwinned blocks + concurrent blocking native calls ≤ `kern.num_taskthreads` − idle baseline threads

Measured, the budget is the cap minus about 85 baseline threads (the .NET runtime's own, the test host, the scheduler's
carriers and monitor):

| Host | `kern.num_taskthreads` | concurrent untwinned blocks plus blocking native calls, at most |
|---|---|---|
| hosted osx-arm64 (Apple silicon) | 2,048 | **about 1,960** |
| hosted osx-x64 (Intel) | 3,072 | **about 2,990** |

So under O6 a macOS program can park any number of goroutines in twinned code (10^5 in 0.11 s, above). Only the
goroutines blocked SYNCHRONOUSLY at the same moment count against the cap: in untwinned code, or in a blocking native
call. That is the number a user plans against, and it is Go's own shape too, since a Go goroutine in a blocking syscall
holds an M. On Windows, locally, 1,500 untwinned blocks took the process from 31 to 1,519 threads: one thread per block
there too.

## 6. P2: the scheduler prototype (`mnsched`, standalone)

The mechanisms:
- GOMAXPROCS P tokens held by carrier threads;
- twinned parks as awaitables, so a parked goroutine is a stored continuation;
- the **blocked-worker handoff** for untwinned parks, in the native-call seam's Enter/Exit shape;
- **carrier injection**: a sysmon thread adds a carrier and a temporary P when one item runs past 10 ms;
- surplus carriers retire after 100 ms idle.

It does not sit over golib's goroutine identity. The spike measures the mechanism; wiring it in is the real build's
work.

| Arm | Mechanism OFF | Mechanism ON |
|---|---|---|
| a1, a spinner on one P (preemption parity, R4) | FAIL: livelock, 10 s | PASS: 1 injection, 0.03 s |
| a2, an untwinned receive on one P with the sender queued (the sizing note's §4.E trap) | FAIL: deadlock, 10 s (injection held off) | PASS: 1 handoff, 0.01 s |
| a4, twinned and untwinned paths mixed | FAIL: stalled at 201 / 100,000 | PASS: 100,000 / 100,000 |
| a3, 10^6 twinned parks | — | PASS: about 1 s, about 500 MB, settling to 12 carriers |

Findings:
1. **Injection alone rescues a2 after 10 ms.** It plays Go's sysmon retaking a P from a long syscall: a backstop for any
   block the handoff does not see. It also masks the handoff, so the a2 arm holds it off.
2. **A sysmon that charges GC pause to a goroutine injects falsely.** The first a3 at 10^6 failed its own criterion:
   40 carriers, from 28-31 injections that coincided with 560-620 ms of GC pause, against exactly 12 carriers with
   injection off. Measuring overruns NET of GC pause, and retiring idle surplus carriers, fixed it with the criterion
   unchanged.
3. **A missing twin costs threads in proportion to concurrent synchronous blocks** (a4: about 600 carriers under about
   117,000 handoffs). That is the budget §5 states for macOS.

## 7. P3: the hand twins

**ParkStorm's twin** (`parkstorm-twin/`) follows `parkstorm/main.go` line for line: `<-release` becomes
`await release.RecvAsync()`. Same harness, same host:

| Program | 10^5 | 10^6 |
|---|---|---|
| O0 | (sizing note: ~6 GB, minutes) | never parked: 20 GB at 617 s |
| the twin | 0.11 s, 66 MB, 22 threads | 0.95-1.04 s, 498-519 MB, 25-31 threads |
| native Go | 0.37 s, 868 MB, 15 threads | 3.77 s, 8,551 MB, 15 threads |

**G4, the delegate tables** (`twindispatch/`, floors over 5 processes, ns per op):

| Arm | ns | over A0 |
|---|---|---|
| **R2: a func literal plus its twin's registration** | **621.96** | **+609 over R0's 13.12** |
| A0, a plain delegate call | 2.59 | — |
| T2, ConditionalWeakTable by delegate instance | 16.03 | +13.4 |
| T1, Dictionary by MethodInfo | 25.14 | +22.6 |
| T3, per-delegate-type cache by method handle (`Delegate.Method` dominates) | 40.24 | +37.7 |
| C+, the positive control (T3 with a ~20 ns spin) | 51.82 | read |
| RT, one park/resume round trip on the prototype | 952.22 | the yardstick |

- The 974 can-park **interface** sites need no table: their twin is a member of the `partial interface`.
- The 639 can-park **func-value** sites pay a lookup.
- **Func literals need T2.** A closure's compiler-generated class cannot carry a twin method, so its twin lambda is
  registered at creation, at about 0.6 us each: close to a whole park/resume, at every creation of a can-park literal
  (681 of 1,481 closures can park, upper bound).

## 8. What the real build must take from the spike

1. **The sysmon must read a goroutine as stuck only when the goroutine itself ran long.** Two kinds of time are not the
   goroutine running:
   - GC pause: 28-31 false injections at 10^6 before the pause was netted out (§6);
   - oversubscription by threads that have nothing to do with it. That caused 11-16 residual injections at 10^6, and
     on osx-x64 the ~3,000 threads another arm leaked pushed the twin to 20 carriers on 4 Ps (run 42599838013, §5).
   A wall-clock overrun measured on the carrier catches both. The real sysmon should measure the carrier THREAD's own
   CPU time across the item (golib already reads per-thread CPU time, `ThreadCpuTime`), falling back to wall time net
   of GC pause where a platform cannot read it.
   It must also return the thread budget: idle surplus carriers retire. On macOS every thread counts against
   `kern.num_taskthreads`.
2. **Every synchronous block takes the handoff,** and injection stays on as the backstop for the blocks the handoff
   cannot see.
3. **Func literals are the expensive edge.**
   - OPEN ITEM: can the converter cooperate at the literal site (for example, by emitting the literal so a generated
     twin can share its capture) and remove the 0.6 us registration?
   - Named functions and method values carry no creation cost.
4. **Twins must live in a golib-owned scheduler over golib's goroutine identity,** with the thread-bound state moved onto
   the goroutine. The spike did not do this. It remains the largest shared piece of work, beside the class-based defer
   frame.

**Open, not measured here:**
- G5's manual check in an IDE debugger (the automated PDB proxy reads both ways). COORD offers it to the owner.
- The linux leg: G2 and the budget were read on macOS and Windows, not on linux.

## 9. Go / no-go, and sizing

**GO** for O6 + O7: hidden async twins, with the handoff and carrier injection as specified. Every gate reads green or
quantified:
- G1 silent;
- G2 and G3 green;
- G4 costed, with the func-literal registration as its real cost;
- G5 green by the PDB proxy, its manual IDE check pending. DESIGN-mn-scheduler's O6 estimate (16-26 lane-weeks) stands. The spike adds two named
items:
- the literal-registration question (§8 item 3);
- the golib wiring (§8 item 4).

Scheduling is the owner's, after `.4` per his ruling.
