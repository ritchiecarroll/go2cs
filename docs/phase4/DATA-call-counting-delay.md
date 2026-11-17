# DATA: the call-counting delay, start-up against steady state, on many CPUs and on one (2026-11-14)

> - **Status:** a measurement record. It changes no default: the decision rule below FAILS, so the template keeps
>   `GoCallCountingDelayMs` at 0, and the owner decides with the trade table in hand.
> - **Read:** 2026-11-14 04:13 to 04:41 UTC, one perf window ("G PERF WINDOW OPEN/CLOSED"), by G, at master
>   `a6cefba6ee`. The bracket on `C:\go2cs-perf` (file count and newest mtime) read the same before and after each
>   half.
> - **Box:** G-LAPTOP: AMD Ryzen 5 PRO 6650U, 6 cores and 12 threads, Windows x64, .NET 10.0.11 (SDK 10.0.400),
>   go1.26.10.
> - **Why:** P5's F1 scout (`claude/p5-f1-package-init` `face9d0108`, `docs/phase4/SCOUT-package-init.md`, finding
>   1) read seat #53's delay of 0 starting the optimizing JIT during start-up, at the largest cost on one CPU.
> - **Data:** [`data/call-counting-delay/`](data/call-counting-delay/): the driver, the arms, every run's values,
>   each run's readout, the two scratch programs and the predictions as written before the runs.

## 1. What the delay is

Under the JIT and ReadyToRun, .NET recompiles a method that keeps running with full optimization. It starts counting
a method's calls only after a quiet period with no new method compiled: 100 ms by default. Seat #53 set a converted
program's delay to 0 (`docs/phase4/DESIGN-jit-inlining-tiering-publish.md`, section 9): counting starts at once, so a
short hot phase reaches optimized code sooner, and the program pays for the optimizing work during its own start.
Native AOT has no tiers, so none of this applies to it.

## 2. Method

- **Binaries.** The Performance suite's own JIT build of every program (its runner's transpile, build and verify
  phases, no timed phase), and the converted project's own publish (`dotnet publish -c Release -r win-x64`:
  self-contained, partial trim, ReadyToRun) of the two start-up programs. Both carry the template's delay of 0.
- **The arms set the delay per run** with `DOTNET_TC_CallCountingDelayMs`, which overrides the runtimeconfig value
  (measured in seat #53), so one build serves every arm: 0 (the base), 0 again (its A/A twin), 10, 20 and 100.
- **The driver** is seat #53's (`drive.py`): one discarded warm-up round, then 15 rounds, every arm once a round, the
  order reversed every other round; every arm must print the base's checksum. It strips every tiering variable from
  the environment and sets only the arm's. Added for this record: `CCD_AFFINITY`, which pins the driver to CPU 0
  before any run; every program it starts inherits the mask, and .NET then reports one processor (checked: a child's
  `Environment.ProcessorCount` reads 1).
- **The statistic.** Each arm's median over the 15 rounds, its interquartile range, and its per-round ratio to the
  base. OUTSIDE (the driver's rule): the ratio's range lies wholly outside the A/A twin's. The tables also mark a move
  whose IQR does not overlap the base's.
- **Programs.** Steady state: the 17 suite rows, reading the in-program workload time (`elapsed_ns`), and seat #53's
  short hot phase (`PerfHotPhase`, its `hash_ns`: `crypto.Hash.String` in a 2,000,000-call loop right after start).
  Start-up: `PerfStartup` (one `fmt` line, 65 packages) and `PerfMidStart` (a scratch program: one pass through
  `encoding/json`, `regexp`, `net/url` and `text/template`, 81 packages), reading the process wall time, in both
  shapes.
- **One CPU:** start-up in every arm, and steady state for the two rows that moved most on many CPUs (TlsHandshake
  and the hot phase).

## 3. Readings

### 3.1 Steady state, all 12 logical CPUs, workload time (ms; the hot phase in ns per call)

| row | delay 0 | delay 10 | change | IQRs overlap? | driver's rule |
|---|--:|--:|--:|:--:|:--:|
| Fib | 98.5 | 122.7 | +25% | no | inside |
| Sieve | 146.0 | 145.7 | 0% | yes | inside |
| MatMul | 217.4 | 214.4 | -1% | yes | inside |
| String | 1,004.9 | 1,303.5 | +30% | no | inside |
| StringView | 22.1 | 19.1 | -13% | no | inside |
| StringMatch | 676.2 | 817.4 | +21% | no | inside |
| Map | 248.4 | 306.9 | +24% | no | OUTSIDE |
| Sort | 255.8 | 324.3 | +27% | no | OUTSIDE |
| Channel | 79.4 | 106.5 | +34% | no | OUTSIDE |
| IfaceCall | 337.3 | 351.2 | +4% | yes | inside |
| Iface | 374.9 | 393.7 | +5% | no | inside |
| IfaceShell | 507.8 | 532.5 | +5% | no | inside |
| IfaceShellChurn | 970.8 | 1,095.9 | +13% | no | inside |
| RefLower | 506.9 | 546.8 | +8% | no | inside |
| FieldView | 1,710.8 | 1,728.5 | +1% | yes | inside |
| DeferPanic | 747.1 | 882.4 | +18% | no | inside |
| TlsHandshake | 1,216.8 | 1,712.6 | +41% | no | OUTSIDE |
| hot phase, `Hash.String` (ns) | 11.1 | 24.6 | x2.2 | no | OUTSIDE |

The hot phase at 20 ms reads 37.8 ns and at 100 ms 38.9 ns: by 20 ms it has given back the whole of seat #53's win
(its own reading on this box: 10.5 ns at 0 against 37.0 at .NET's default).

### 3.2 Steady state, ONE CPU

| row | delay 0 | delay 10 | change |
|---|--:|--:|--:|
| TlsHandshake (ms) | 2,388 | 4,541 | x1.90, OUTSIDE |
| hot phase, `Hash.String` (ns) | 53.8 | 38.9 | -28%, OUTSIDE |

On one CPU a delay of 0 SLOWS the short hot phase: the background compile competes with the program for the only
CPU (20 ms: 39.4 ns; 100 ms: 41.1 ns). The long TLS run still loses heavily at 10 ms.

### 3.3 Start-up, process wall time (ms)

| program | shape | CPUs | 0 | 10 | 20 | 100 | 10 against 0 |
|---|---|:--:|--:|--:|--:|--:|--:|
| PerfStartup | JIT build | 12 | 470 | 445 | 444 | 442 | -25 |
| PerfStartup | published | 12 | 346 | 316 | 314 | 313 | -30, OUTSIDE |
| PerfMidStart | JIT build | 12 | 821 | 763 | 760 | 763 | -58 |
| PerfMidStart | published | 12 | 554 | 506 | 502 | 506 | -49, OUTSIDE |
| PerfStartup | JIT build | 1 | 609 | 463 | 461 | 466 | -146 |
| PerfStartup | published | 1 | 426 | 326 | 325 | 323 | -100, OUTSIDE |
| PerfMidStart | JIT build | 1 | 1,004 | 776 | 774 | 775 | -228, OUTSIDE |
| PerfMidStart | published | 1 | 678 | 510 | 508 | 510 | -169, OUTSIDE |

Every start-up arm's IQR at 10, 20 and 100 ms lies wholly below the base's. 10, 20 and 100 ms read within a few
milliseconds of each other in every row: the start-up cost is the delay of 0 itself.

## 4. The predictions, scored as written

- C1 (the 16 suite rows other than TlsHandshake inside their bands at 10): FALSE. Map, Sort and Channel move OUTSIDE,
  and seven more move by IQR (+5 to +30%); StringView moves the other way (-13%).
- C2 (TlsHandshake 10 to 35% slower at 10): the direction TRUE, the size 41%, just above the range.
- C3 (the hot phase about 3 times at 10): FALSE as worded: 2.2 times at 10, 3.4 at 20, 3.5 at 100.
- C4 (PerfStartup JIT, many CPUs, 10 to 30 ms faster at 10): TRUE, 25 ms.
- C5 (PerfStartup JIT, one CPU, 50 to 150 ms faster): TRUE, 146 ms.
- C6 (PerfStartup published: 15 to 35 ms on many CPUs, 80 to 200 on one): TRUE, 30 and 100 ms.
- C7 (PerfMidStart 1.5 to 3 times PerfStartup's gain): TRUE, 1.6 to 2.3 times.
- C8 (on one CPU, 0 the slowest arm and 10/20/100 within 15%): TRUE.

## 5. The decision rule

Posted with the predictions: switch the default from 0 to 10 ms only if every suite row holds within its band at 10,
TlsHandshake and the hot phase hold too, and start-up improves on one CPU. **The rule FAILS on its first two parts**:
at 10 ms, four rows move OUTSIDE the band on many CPUs (Map, Sort, Channel, TlsHandshake), seven more move by IQR, and
the hot phase loses most of seat #53's win. The third part passes with room. So the default stays 0, no template
change is made, and the trade is the owner's to weigh:

- **0 (today's default)** keeps long-running work fast: a delay of 10 ms makes TlsHandshake 41% slower on 12 CPUs and
  1.9 times slower on one, and the suite's map, sort, channel and string rows 20 to 34% slower.
- **A delay (10, 20 or .NET's 100; they read alike)** starts a program faster: 25 to 58 ms on 12 CPUs, 100 to 228 ms
  on one, which is a quarter of a short program's wall time on one CPU.

The Tuning page now states this trade as a choice by workload (docs/Tuning.md, "Start-up or steady state").

## 6. Not measured

- Native AOT (no tiers), linux and macOS on this box (the scout's linux VM readings agree in direction: 462 ms at 0
  against 300 at 10 on one CPU), two or four CPUs (only 12 and 1), and the suite's other rows on one CPU.
- The seat #53 microbench's i9 numbers the order cites (17 to 2.3 ns) are the i9's; this record reads its own.
