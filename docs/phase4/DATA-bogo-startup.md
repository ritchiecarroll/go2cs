# DATA — the BoGo shim's per-launch cost, four arms and a Native AOT attempt

> **State: MEASURED (2026-11-05), linux/amd64, one 4-core cloud box, master `586cf44726`.** Measurement only: no
> converter, golib or pipeline change rides with this note. Sections are **appended and superseded, never
> overwritten.** Raw readings and the scripts that took them are in [`data/bogo-startup/`](data/bogo-startup/).

## 1. Why this was measured

`crypto/tls`'s `TestBogoSuite` drives BoringSSL's runner, and the runner re-executes the converted test host as its
shim once per case: about 3,527 launches for the 3,526 cases the suite runs. The runner is itself a `go test`
package with Go's default 10-minute wall. The converted run does not finish inside it, so the row banks 1,393 + 2
with the root disclosed `host-limit`, and the fan-out rows are withdrawn (BOARD, 2026-10-25, G's row and P1's
amendment). Go's own suite passes in 32 to 44 s, at about 85 ms per shim launch.

1.26.10.2 seat #9 (batch 62, `382983f1df`) measured four arms on this box class and made `crypto/tls`'s host
ReadyToRun (`readyToRunTestPackages`, `src/go2cs/testConversion.go:348`). So the arm COORD named "published
ReadyToRun" is what the pipeline builds today. This note re-measures the four arms against today's build, with 24
launches each, the image sizes, and one more arm, Native AOT.

## 2. Host, toolchain, method

| | |
|:--|:--|
| box | Intel Xeon @ 2.10 GHz, 4 logical processors, 15 GB, no swap, kernel 6.18 |
| toolchains | go1.26.10, .NET SDK 10.0.403 |
| tree | master `586cf44726`, `go2cs -tests -test-action convert` then `build` (the pipeline's own publish) |
| runner | BoringSSL at the module version `bogo_shim_test.go` pins (`v0.0.0-20250620172916-f51d8b099832`) |

- **One launch** (`launch.sh`): the runner runs one handshake case, `CurveTest-Client-MLKEM-TLS13` (Go's own
  `assertResults` names it), at `-num-workers=1`, with the runner arguments `bogo_shim_test.go` spells. Each
  invocation launches the shim twice: once as the `-is-handshaker-supported` probe (start, init, exit), then for
  the handshake. The shim is a wrapper (`shimwrap.sh`) that times the host from exec to exit. There are 25
  invocations per arm; the first is a warm-up and is dropped (`stats.py`).
- **Whole BoGo** (`suite.sh`): the runner exactly as `bogo_shim_test.go` drives it, with its default workers (4
  here) and its 10-minute wall. Every launch is timed by the same wrapper.
- **The arms**, one axis each from today's build:

| arm | host | child's tiering |
|:--|:--|:--|
| (a) today | ReadyToRun composite with `--enable-generic-cycle-detection`, single file | off (`DOTNET_TieredCompilation=0`, which the pipeline sets on the host and every child inherits) |
| (b) | the same project published with `PublishReadyToRun=false` (JIT only) | off |
| (c) | (b)'s host | the .NET defaults: the variable removed from the shim's own environment only |
| (d) | (a)'s host | the .NET defaults, as in (c) |
| (e) | Native AOT (`PublishAot=true`, not single file), scoped to the host project | not applicable |

## 3. One launch

| arm | image | handshake median | p25 to p75 | min to max | probe median | nonzero exits |
|:--|--:|--:|:--|:--|--:|--:|
| (a) today | 512 MB | **1.18 s** | 1.17 to 1.22 | 1.12 to 1.23 | 1.01 s | 0 of 48 |
| (b) JIT | 150 MB | 5.38 s | 5.31 to 5.44 | 5.18 to 5.52 | 3.62 s | 0 of 48 |
| (c) JIT, tiered child | 150 MB | 2.79 s | 2.70 to 2.87 | 2.67 to 3.12 | 2.09 s | 0 of 48 |
| (d) ReadyToRun, tiered child | 512 MB | 1.23 s | 1.20 to 1.27 | 1.12 to 1.36 | 1.07 s | 0 of 48 |
| (e) Native AOT | — | not built | | | | |

n = 24 handshakes and 24 probes per arm. The image is the single-file host's size; (c) and (d) reuse the hosts of
(b) and (a).

**(e) did not build.** ILC ran for 2,986 s, then exited with `Insufficient memory to continue the execution of the
program`. Sampled used memory peaked at 12,703 MB on the 15 GB box. ILC had reported 142 trim and AOT warnings by
then: 47 IL2026, 26 IL2070, 16 IL3050, 16 IL2075, and fewer of others. As stated before the run, the arm stopped at
its first failure. A reading of (e) needs a larger box, or golib's generic nest broken first (BOARD, 2026-10-27,
"crossgen2 still expands golib's array/ж generic nest"). ILC on this closure hitting the same nest is plausible but
not shown.

## 4. Whole BoGo at the 600 s wall

| arm | wall | launches | runner counter (failed/unimplemented/done/started of 6,473) | handshake median under load (p25 to p75) | concurrency | projected full run |
|:--|--:|--:|:--|:--|--:|--:|
| (a) today | 601 s | 1,950 | 0/889/1938/1942 | 1.22 s (1.15 to 1.30) | 3.97 | **about 1,085 s** |
| (d) | 600 s | 1,818 | 0/779/1806/1810 | 1.30 s (1.23 to 1.40) | 3.97 | about 1,165 s |
| (b) | not run | | | | | about 4,740 s (single-worker median) |
| (c) | not run | | | | | about 2,460 s (single-worker median) |

- The projection is 3,527 launches at the measured launch rate: 1,950 in 601 s for (a), 1,818 in 600 s for (d).
  It agrees with launches times the under-load median over the concurrency (3,527 x 1.22 s / 3.97 = 1,084 s). For
  (b) and (c), the single-worker median times 3,527 over 4 is a lower bound.
- No case failed in either run. The exit codes the wrapper saw were 0, 89 (the runner's "unimplemented"), 1 (cases
  whose expected outcome is a shim error) and 141 (SIGPIPE, 4 in each run). The runner exited 1 at its wall
  without writing results, which is the arm the `host-limit` disclosure pins.
- To clear 600 s here, a launch must average 600 x 3.97 / 3,527 = **0.68 s** under load. (a) is 1.8 times that.
- Against seat #9 at `e6b7f24f81`: (a) reached 1,950 launches at the wall against 1,695, and projects about
  1,085 s against about 1,260 s. The single-worker medians match within 2% in every arm.

## 5. Where today's 1.18 s goes

- **The handshake costs 0.17 s; the probe, a launch that does no TLS work, costs 1.01 s.** So about 85% of a
  launch in (a) is start-up and init.
- **The .NET runtime's floor is about 28 ms.** A trivial self-contained single-file ReadyToRun program on the same
  box and toolchain reads 26 to 40 ms over 10 runs (median 28 ms; 84 MB image).
- **Fixture staging is about 10 ms.** The host copies its 122 fixture files (1.1 MB) into the run root on every
  launch, and `cp -r` of the same set takes 10 ms.
- **The rest is loading the closure.** Under `strace` the probe opens the single-file bundle 247 times before
  fixture staging starts, once per assembly load. Those loads and the package inits they run are about 0.95 s of
  the 1.01 s. This is read from timestamps, not profiled per assembly.

## 6. The prediction, scored

The prediction was posted before the first run (mailbox `16f6b5628`).

| predicted | measured | score |
|:--|:--|:--|
| (a) 1.1 to 1.4 s, probe about 1.0 s | 1.18 s, probe 1.01 s | MET |
| (b) 4.8 to 6.0 s | 5.38 s | MET |
| (c) 2.6 to 3.3 s | 2.79 s | MET |
| (d) within 10% of (a) | 1.23 s, +4% | MET |
| ordering a < d < c < b | a < d < c < b | MET |
| (a) projected 1,100 to 1,400 s | about 1,085 s | MISSED, 15 s under the band |
| (a) 1,500 to 1,900 launches at the wall | 1,950 | MISSED, 50 above the band |
| no ReadyToRun or JIT arm clears 600 s here | none does | MET |
| (e) odds below even that it publishes and handshakes | ILC out of memory | consistent; a stated odds is not scored |
| images: (b) and (c) about 150 MB, (a) and (d) about 500 MB | 150 MB and 512 MB | MET |

Both misses are on the side of today's host doing better than seat #9's reading, and their cause is not isolated.
The single-worker medians did not move. The whole-run rate did, from 1,695 to 1,950 launches in the same wall.

## 7. Recommendation

- **Keep (a), the pipeline as it is.** It is the fastest arm, the one launch and the whole run both.
- **Do not adopt a tiered child.** On a ReadyToRun host, (d) is 4% slower alone and 7% slower under load. On a JIT
  host it would halve the launch, but (a) is already 2.4 times faster than that. It would also turn tiering on for
  timing verdicts, which the Release ruling keeps off.
- **No arm measured here brings BoGo inside 600 s on 4 cores.** If launches scaled linearly with workers, an 8-core
  host would run (a) in about 3,527 x 1.22 / 8 = 540 s. That is inside 600 s but above the 480 s bank rule (seat #9's
  ruling), and it is an extrapolation, not a reading.
- **The remaining lever is the host's start-up, not the JIT.** About 0.95 s of the 1.18 s is loading about 240
  assemblies and running their package inits. Two later-seat candidates, neither sized here:
  - a per-assembly breakdown of the probe launch, from an EventPipe or `perf` trace, to see whether a few package
    inits dominate;
  - Native AOT for this one host, re-tried on a box with more memory, or after golib's generic nest is broken.

— P1 (2026-11-05)
