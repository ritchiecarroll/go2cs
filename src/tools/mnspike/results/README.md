# M:N spike, P0 and P1 results (lane G, 2026-11-09)

The R5 spike's evidence for docs/phase4/DESIGN-mn-scheduler.md §8 item 5, on the `claude/g-mn-spike-*` line only
(never master). The record proper is P5's `docs/phase4/DATA-mn-spike.md`. Tree: master `d25c05efd2`; Go go1.26.10;
.NET 10.0.400; host G-LAPTOP (Windows x64, 12 logical CPUs, 30.8 GB).

## P0, red baselines on O0 (today's 1:1 model)

| Gate | File | Reading |
|---|---|---|
| G2, 10^5 parked on osx-arm64 | (C1's run 42497561630, DarwinGoroutineThreadCapProbe) | the first failing `go` at 2,027 live goroutines |
| G3, 10^6 parked on Windows under 20 GB | `G3-red-O0.txt` (`../Measure-ParkStorm.ps1`, `../parkstorm`) | STOPPED-MEMORY at 617 s: 20,480 MB, 360,269 threads, 300,000 started |
| G5, a visible breakpoint binding in a twin | `G5-twinline-false.txt` (`../twinlines`, no `#line`) | MISSES, 4 of 4 visible lines unbound |

`G5-twinline-true.txt` is the same instrument on the `#line` twin: BINDS, 4 of 4. The instrument reads both ways.

## P1, the can-park census (`../canpark`, x/tools v0.52: go/ssa + callgraph/vta)

Leaves: the park inventory's rows 1-7 and 9, in Go terms. The runtime family neither parks nor propagates (rows 11-12:
converted-dead in go2cs). `-long-only` drops Mutex/RWMutex and fdMutex contention. Two bounds:

- the UPPER bound propagates through VTA's interface and func-value edges. It is sound but context-insensitive:
  `sync.Once.Do`'s parameter merges every closure passed to it, and `fmt.Fprintf`'s `io.Writer` merges every writer
  (see `P1-controls-witnesses.txt`);
- the LOWER bound (`-static-only`) propagates through static calls only, and misses real dynamic parks such as
  `io.Copy` from a pipe.

| std, `-exclude-runtime -long-only` | windows | linux |
|---|---|---|
| upper: can-park functions | 5,223 / 15,702 = 33.3% | 5,266 / 15,609 = 33.7% |
| lower | 597 = 3.8% | 716 = 4.6% |

Modules (`P1-modules-*.txt`): gojq 191 / 425 (44.9%) upper, 0 lower; timefmt-go 8 / 21 upper, 0 lower. Both are
inflated by the `sync.Once.Do` / `fmt` edges above.

## P1b, the twin-refusal scan (`../twinscan`, Roslyn over the emitted C#)

| Population | Matched to C# | HARD (no twin) | Transforms (mechanical, twin only) |
|---|---|---|---|
| can-park set, windows | 4,966 of 5,223 | 8 (0.2%): unsafe 7, stackalloc 2, other ref 1 | boxed ref local 1,992; ref param 1,323; defer frame 479; span param 204 |
| direct parkers (the common park paths), windows | 110 of 117 | **0** | boxed ref local 69; defer frame 37; ref param 28 |
| can-park set, linux | 5,007 of 5,266 | 1 (0.0%) | boxed ref local 1,986; ref param 1,323; defer frame 483; span param 205 |
| direct parkers, linux | 108 of 115 | **0** | boxed ref local 67; defer frame 35; ref param 28 |

The box shapes were taken from a census of every scanned method's ref-local initializers (`TWINSCAN_SHAPES=1`), not
assumed.

## G1

Neither of COORD's stop triggers fires:
- the can-park share is at most 33.7% of std (the upper bound), against the 60% line;
- no common park path is left untwinned: 0 hard refusals among the direct parkers on either GOOS.

## P2, the scheduler prototype (`../mnsched`, `P2-arms.txt`)

A standalone spike assembly: GOMAXPROCS P tokens held by carrier threads, twinned parks as awaitables (a parked
goroutine is a stored continuation), the blocked-worker HANDOFF for untwinned parks (the seam's Enter/Exit shape), and
CARRIER INJECTION (a sysmon thread adds a carrier and a temporary P when a goroutine runs past 10 ms, net of GC pause,
while work is queued). Surplus carriers retire after 100 ms idle. It does not sit over golib's goroutine identity; the
spike measures the mechanism, and wiring it into golib is the real build's work.

| Arm | Mechanism OFF | Mechanism ON |
|---|---|---|
| a1, a spinner on one P (preemption parity) | FAIL: livelock, 10 s (handoff also off) | PASS: 1 injection, 0.03 s |
| a2, an untwinned park on one P (the §4.E trap) | FAIL: deadlock, 10 s (injection held off: it masks, see below) | PASS: 1 handoff, 0.01 s |
| a4, twinned and untwinned paths mixed | FAIL: stalled at 201 / 100,000 | PASS: 100,000 / 100,000, 2.6 s |
| a3, 10^5 twinned parks | | PASS: 0.13 s, 69 MB, 23 threads |
| a3, 10^6 twinned parks (3 runs) | | PASS: 1.22-1.30 s, 472-483 MB, 33-38 threads while parked, settling to 12 carriers |

Two findings, both kept:
- INJECTION ALONE RESCUES a2 after its 10 ms threshold. It plays Go's sysmon retaking a P from a long syscall: a
  backstop for any block the handoff does not see.
- The FIRST a3 at 10^6 read FAIL by its own criterion (40 carriers > procs + 2). The diagnosis: 28-31 FALSE injections
  that coincided with 560-620 ms of total GC pause; with injection off, exactly 12 carriers. Two changes, the criterion
  unchanged:
  - the overrun is measured net of the GC pause accrued since the item started (11-16 injections remain, from
    oversubscription: a descheduled carrier's wall time also grows);
  - surplus carriers retire after 100 ms idle. On macOS every thread counts against kern.num_taskthreads, so the
    budget must come back.

O0 for comparison (P0's G3): 10^6 never parked; the run stopped at 20 GB and 617 s.

## P3, the hand twins

### ParkStorm's twin (`../parkstorm-twin`, on the P2 scheduler): G3, measured (`P3-G3-parkstorm-twin.txt`)

The twin follows `../parkstorm/main.go` line for line, as go2cs-gen would emit it: the goroutine's `<-release` becomes
`await release.RecvAsync()`, and main's waits stay synchronous. Same harness, same host:

| Program | 10^5 parked | 10^6 parked |
|---|---|---|
| O0, today's converted ParkStorm | (sizing note: ~6 GB, minutes) | NEVER PARKED: 20 GB at 617 s, 300,000 started (`G3-red-O0.txt`) |
| the twin on the P2 scheduler | 0.11 s, 66 MB, 22 threads | 0.95-1.04 s, 498-519 MB, 25-31 threads (6 runs) |
| native Go, go1.26.10 | 0.37 s, 868 MB, 15 threads | 3.77 s, 8,551 MB, 14-15 threads |

G3 (10^6 under 20 GB) reads GREEN on the twin. The twin is about 16x lighter than Go here only because ParkStorm's
goroutine body is trivial: a twin's state machine holds just the locals live across its park. A real goroutine's twin
is as heavy as its live locals, against Go's 2 KB starting stack.

### The delegate path and G4 (`../twindispatch`, `P3-G4-dispatch.txt`), floors over 5 processes, ns per op

| Arm | Floor | Read |
|---|---|---|
| A0, a plain synchronous delegate call | 2.59 | the baseline |
| T2, ConditionalWeakTable by delegate INSTANCE, lookup and twin call | 16.03 | +13.4 over A0, the cheapest lookup |
| T1, Dictionary by MethodInfo | 25.14 | +22.6 |
| T3, per-delegate-type cache by method handle | 40.24 | +37.7: `Delegate.Method` dominates |
| C+, T3 with a calibrated ~20 ns spin (positive control) | 51.82 | the instrument reads the injected cost |
| R0, make a func literal | 13.12 | R2's baseline |
| R2, make a func literal AND register its twin in T2 | 621.96 | **+609 per registration** |
| RT, one park/resume round trip on the P2 scheduler | 952.22 | what a lookup is read against |

Read against the census's dynamic sites (windows upper):
- the 974 can-park INTERFACE sites need no table: their twin is a member of the partial interface, one more virtual call;
- the 639 can-park FUNC-VALUE sites (360 more unresolved) take a lookup: +13 to +38 ns against a ~950 ns park/resume
  round trip.

**The cost that matters is R2.** Only T2 serves func LITERALS, because a closure's compiler-generated class cannot carry
a twin method. Registering a literal's twin costs about 0.6 us, close to a whole park/resume round trip, at every
creation of a can-park literal (P1: 681 of 1,481 closures can park, upper bound). Named functions and method values
use T1 or T3 at no creation cost. This is O6's "func values" constraint, quantified. It is a cost, not a showstopper.
A cheaper literal path would need the converter's cooperation at the literal site, which is a P5 question.

### The P2 arms after P3's `Go()` change (`P3-arms-rerun.txt`)

The change gives each goroutine one awaiting root frame and drops the per-goroutine `ContinueWith`. Every arm reads as
in P2: a1, a2 and a4 FAIL with their mechanism off and PASS with it on. a3 at 10^6 parks in 0.94-0.99 s with 7
injections.
