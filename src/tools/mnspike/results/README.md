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
