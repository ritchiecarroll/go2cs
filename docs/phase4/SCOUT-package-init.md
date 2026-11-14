# SCOUT: package initialization at start-up (finding F1)

Scout, lane P5, opened 2026-11-14 at master `a6cefba6ee`. Measure and attribute only; no fix code. Brief: COORD's
post of 2026-11-14 00:24Z. Origin: finding F1 of `docs/phase4/DESIGN-startup-method-sets.md` (a converted hello
spends about 300 of its 336 ms before `main` in package module initializers, `go.runtime` 172 ms inclusive).

## 0. Predictions, written before any measurement of this scout

What I had seen when I wrote these: the design's one sampled trace of hello's start-up thread (inclusive times per
frame, about 330 samples) and its wall times. Nothing else. The numbers below are for the converted hello on the
design's VM class (cloud linux/amd64, 4 vCPU, Release, JIT, framework-dependent), and are scored as worded.

| # | prediction | falsified if |
|:--|:--|:--|
| P1 | JIT compilation is the largest single kind: 55% to 75% of the time before `main`. | under 40%, or another kind is larger |
| P2 | Running the initializers' own code (static data, map and table builds, `init` bodies), JIT excluded, is under 20% of the time before `main`. | over 30% |
| P3 | Registrations (`ᴛRegisterAdapters`, `GoDeferredRegistrations`) cost under 10 ms in total on hello. | over 20 ms |
| P4 | `runtime` is the largest package by exclusive time, 90 to 140 ms; the five largest packages hold over 70% of the time before `main`. | `runtime` under 60 ms or not first; five largest under 55% |
| P5 | Precompiling the converted closure and golib (ReadyToRun) removes 40% to 60% of hello's wall time (336 ms to between 135 and 200 ms). | removes under 25% |
| P6 | golib's own start-up (its module initializer and static constructors, before any package's) is under 25 ms. | over 50 ms |
| P7 | No single map or table build costs over 10 ms of execution (JIT excluded). | one does |

Method, fixed before measuring:
- **Per package, exclusive:** a start-up hook runs each assembly's module initializer in dependency order, leaf
  first, and records wall time, JIT time, methods compiled, IL bytes compiled and bytes allocated on that thread
  for each. An initializer that runs after its dependencies' is charged only its own work.
- **Per kind:** sampled stacks of the start-up thread, each sample classed by its innermost converted or golib
  frame; per-method JIT durations from the runtime's JIT events, classed the same way.
- **Controls:** the same program with the hook absent (the hook's own cost), a plain C# hello (the floor), and an
  A/A pair for every interleaved comparison.
- **What-ifs, measured not built:** ReadyToRun for the closure; tiered-compilation settings.
