# Call-counting delay: predictions (written 2026-11-13 21:51 CST, before any timed run)

Box: G-LAPTOP (AMD Ryzen 5 PRO 6650U, 6 cores / 12 threads), win-x64, .NET 10.0.11, master a6cefba6ee. JIT build and
the converted project's own publish (ReadyToRun, self-contained, partial trim) from the Performance suite's runner,
transpile/build/verify only. The delay is set per run with DOTNET_TC_CallCountingDelayMs (it overrides
runtimeconfig.json in both directions: seat #53, 9.4), so one build serves every arm. Driver: seat #53's drive.py,
one discarded warm-up round, 15 interleaved rounds (order reversed every other round), an A/A twin of the base arm,
OUTSIDE = a ratio range disjoint from the A/A range. One-CPU arms: the driver pins itself to CPU 0 and its children
inherit it (.NET then sees one processor).

Prior on this box (seat #53, 9.4, against .NET's 100 ms default): the short hot phase Hash.String reads 0.286 at 0
ms, 0.948 at 10 and 0.706 at 20; PerfTlsHandshake's wall -32.6% at 0; PerfStartup's wall +24 ms at 0 (JIT build).

## Steady state, JIT build, workload time (elapsed_ns), d10 against d0

- C1 The 16 suite rows other than TlsHandshake (Fib, Sieve, MatMul, String, StringView, StringMatch, Map, Sort,
  Channel, IfaceCall, Iface, IfaceShell, IfaceShellChurn, RefLower, FieldView, DeferPanic) each read d10/d0 INSIDE
  their A/A band. Their hot loops run 0.3 to 5 s; 10 ms more before counting is noise there. Confidence 80%.
- C2 TlsHandshake reads d10 SLOWER, OUTSIDE the band, by 10 to 35%: its 64 handshakes are a stream of short phases
  that keep compiling new methods, and .NET extends the delay while tier-0 compilation continues, so 10 ms keeps much of
  the code at tier 0 for most of the run. Confidence 60%.
- C3 Seat #53's short hot phase (Hash.String) reads d10 about 3 times d0 (2.5 to 3.5), OUTSIDE. Confidence 75%.

## Start-up, process wall time (ms), arms d0 / d10 / d20 / d100

- C4 PerfStartup, JIT build, all CPUs: d10 faster than d0 by 10 to 30 ms (OUTSIDE or at the band's edge); d20 and
  d100 within 10 ms of d10.
- C5 PerfStartup, JIT build, ONE CPU: d10 faster than d0 by 50 to 150 ms, OUTSIDE.
- C6 PerfStartup, published (ReadyToRun): all CPUs d10 faster by 15 to 35 ms; ONE CPU faster by 80 to 200 ms.
- C7 PerfMidStart (81 packages: json, regexp, url, template), both builds: the same direction as PerfStartup and
  larger in milliseconds, by 1.5 to 3 times.
- C8 On one CPU every program at d0 is the slowest arm; d10, d20 and d100 read within 15% of each other.

## Decision rule (COORD's, sharpened)

Switch the template default from 0 to 10 ms only if all three hold:
1. every one of the 16 suite steady-state rows reads d10/d0 inside its A/A band;
2. TlsHandshake and the Hash.String hot phase (the large app and the short hot phase that motivated 0) read inside
   their bands too, so 10 gives back none of seat #53's win;
3. start-up improves OUTSIDE the band on ONE CPU for both PerfStartup and PerfMidStart.
Otherwise the default stays 0, the record names each row that moves, and the owner decides; the Tuning page states
the trade either way. PREDICTED OUTCOME: the rule FAILS on 2 (C2, C3), so the default stays 0 and the page leads
with the choice: 0 for long-running services and benchmarks, a delay (10 or `runtime`) for short-lived tools and
1-2 CPU containers.
