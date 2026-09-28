# Go vs transpiled C# — runtime performance comparison

A small, targeted benchmark suite answering the question people ask first: **how fast is transpiled
C# compared to the original Go?** — startup time and memory, on both the normal JIT runtime and
**Native AOT** (self-contained, the closest deployment analog to a Go binary).

Each benchmark is a tiny Go program (same shape as the
[behavioral tests](https://github.com/ritchiecarroll/go2cs/tree/master/src/tests/Behavioral)),
chosen to exercise one Go construct with a real C# cost model — slices, strings, maps, channels,
interface dispatch — plus raw compute loops where the two runtimes should be close. This is not an
exhaustive benchmark game; it gives the common expected range of differences. The short version:
transpiled C# is **usually slower than Go, but not universally** — maps and the stack-string
path run at parity or faster in both C# variants (maps dramatically so under Native AOT), while
the rest ranges from ~1.5× on tight compute to the structural-interface assert, the honest
outlier at the other end.

## The benchmarks

| Benchmark | What it exercises |
|---|---|
| **Startup** | Empty workload: pure process start + runtime init + one `fmt` round-trip. Wall time. |
| **Fib** | Recursive Fibonacci (`fib(34)` ×5): function-call and integer-op overhead. |
| **Sieve** | Sieve of Eratosthenes to 10M ×3: slice allocation, indexing, tight loops (`slice<T>` bounds/header emulation). |
| **MatMul** | 256×256 `float64` matrix multiply ×4: floating-point throughput, nested slice-of-slice access. |
| **String** | 10M iterations of byte-slice append → `string` conversion, indexing, concatenation (`@string` emulation). |
| **StringView** | 20M iterations of keyword checks `string(buf) == "null"/"true"/"false"` over a fixed buffer — the idiom the converter's stack-string (`sstring`) emission optimizes: a zero-copy view compared against a `u8` literal span, no per-comparison allocation. |
| **StringMatch** | 20M iterations of literal-string hot paths: switch-on-string dispatch, `strings.HasPrefix` with a literal prefix, literal returns, literal map-key counters — the shapes where a naive conversion would allocate a fresh `@string` per evaluation while Go allocates nothing (string literals live in RODATA). Instrument for the literal-comparison optimizations (span operators, `u8` casts, hoisted literals). |
| **Map** | 2M inserts + 2M comma-ok lookups + 1M deletes on `map[int]int` (`map<K,V>` emulation). |
| **Sort** | `sort.Ints` on 2M deterministic pseudo-random ints (`sort.Interface` dispatch through the runtime's reflection-bound `Interface<T>`). |
| **Channel** | 1M ints producer→consumer through a buffered channel with one goroutine (`channel<T>` + goroutine scheduling emulation). |
| **IfaceCall** | 50M iterations of pure interface method dispatch — interface values of statically-known types built once, called in a megamorphic hot loop; no asserts, no switches. The row that answers "what does calling an interface method cost?" |
| **Iface** | 20M iterations of the **common** interface cases: method dispatch, concrete comma-ok assertions, and a type switch over a closed set — all resolved by the compile-time (nominal) machinery: generated adapters and cast-shaped asserts. What ordinary Go interface code costs. |
| **IfaceShell** | 5M iterations × 2 duck-typed interface asserts + forwarded calls — one on a value-typed dynamic value (the reflective **object shell**), one on a pointer-sourced one (the delegate-bound **generic shell**). The one path with no compile-time answer, and the only shared mechanism whose Native AOT behavior is otherwise unexercised. |

Every benchmark prints a deterministic **checksum** (verified byte-identical across Go, C# JIT, and
C# AOT before anything is measured) plus its own workload time measured in-program via
`time.Now().UnixNano()` — so the headline numbers exclude process startup, which is reported
separately by the Startup row.

## Methodology / fairness notes

- **Three variants of the identical program:** the Go binary (`go build`, default optimized), the
  transpiled C# built `Release` framework-dependent (JIT column), and the same C# published with
  `PublishAot=true` self-contained, partial trim (Native AOT column).
- **Median of 5 runs** (configurable) after 1 discarded warmup, single-shot process executions — the
  way a Go CLI actually runs. The JIT column deliberately includes in-process tiered-JIT warmup inside
  the workload; long-running server workloads would look better for the JIT than these numbers do.
- **Peak memory** is the process peak working set, polled while it runs. Both wall time and in-program
  workload time are captured; tables report workload time (Startup row: wall).
- Benchmarks avoid nondeterminism (no `math/rand`; inline xorshift/LCG generators), so outputs are
  byte-comparable, and print timing on a filtered `elapsed_ns:` line.
- **Published tables come from a single designated host per era** — the Environment line names the
  part — so the History section's cross-toolchain comparisons (e.g., .NET 9 → 10) are always
  same-machine. Ratios from different hardware are not comparable and are never mixed.
- **The AOT column's numbers are runtime numbers; producing them is expensive, and we say so.**
  Each Native AOT publish compiles the entire converted-stdlib closure whole-program — hours per
  publish, largely single-threaded, at a **15–18 GB** build-time working-set peak (re-measured per
  hop) — which is also exactly what buys the column's lean images and runtime memory wins. The
  full disclosure, the reasoning, and the compile-farm mitigation live in the suite README's
  ["What the AOT column costs to produce"](https://github.com/ritchiecarroll/go2cs/blob/master/src/tests/Performance/README.md#what-the-aot-column-costs-to-produce--the-honesty-footnote)
  section.

## Running it

```powershell
cd src/tests/Performance
./run-performance.ps1                    # full run: transpile, build (incl. AOT), verify, measure
./run-performance.ps1 --no-aot           # skip AOT publishes, faster while iterating
./run-performance.ps1 --filter Map       # one benchmark
./run-performance.ps1 --runs 10 --update-readme   # refresh the results block below
```

Requirements: Go toolchain, a .NET SDK matching the corpus target framework (`net10.0`, named by
`src/Directory.Build.props`), and — for the AOT column — a native linker and toolchain, which differs
by host:

| Host | Native AOT prerequisite |
|:--|:--|
| **Windows** | MSVC C++ build tools — Visual Studio 2022's "Desktop development with C++" workload, which supplies the `link.exe` ILC needs. The runner prepends the VS Installer directory to `PATH` so the SDK's `vswhere` probe finds it. |
| **Linux** | `clang` and `zlib1g-dev` (Debian/Ubuntu: `sudo apt install clang zlib1g-dev`; Fedora: `clang zlib-devel`). ILC shells out to `clang` for the native link step. |
| **macOS** | The Xcode command line tools (`xcode-select --install`), which supply `clang` and `ld`. |

Deliberately **not** scripted: `run-performance.ps1` does not install any of these. A benchmark
harness that silently mutates the machine's toolchain is not a harness anyone should trust with a
performance claim. `--no-aot` drops the whole column and needs none of them (F13,
[PLAN-linux-operation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/PLAN-linux-operation.md)).

The environment line of the results block reads the CPU name from the registry on Windows and from
`/proc/cpuinfo` on Linux, so a published table always names the part that produced it.

The runner (`PerformanceRunner`), a dependency-free console app structured like the behavioral suite's
`BehavioralRunner`, runs **Transpile → Build → Verify → Measure**. Verify requires all three binaries
to produce identical (timing-filtered) stdout before anything is timed, so the table can never
silently report a benchmark that computes something different in C#.

## Results

<!-- PERF-RESULTS:BEGIN -->

**Environment:** AMD Ryzen 5 PRO 6650U with Radeon Graphics · Microsoft Windows 10.0.26200 · go1.23.1 · .NET SDK 10.0.400 · 2026-08-25

C# builds: JIT = framework-dependent `Release`; Native AOT = `-p:PublishAot=true` self-contained, partial trim. Median of 5 runs (1 discarded warmup). Workload time is measured in-program and excludes process startup; the Startup row is pure process wall time. Ratios are relative to Go.

**Execution time** (milliseconds -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 22.9 | 279.4 (12.20×) | 36.7 (1.60×) |
| Fib | 120.4 | 163.7 (1.36×) | 178.0 (1.48×) |
| Sieve | 68.5 | 126.1 (1.84×) | 242.4 (3.54×) |
| MatMul | 107.3 | 216.3 (2.02×) | 594.9 (5.55×) |
| String | 103.6 | 701.7 (6.78×) | 1,272.5 (12.29×) |
| StringView | 19.3 | 19.5 (1.01×) | 22.0 (1.14×) |
| StringMatch | 199.2 | 935.7 (4.70×) | 1,156.7 (5.81×) |
| Map | 631.2 | 422.3 (0.67×) | 248.8 (0.39×) |
| Sort | 141.3 | 405.9 (2.87×) | 473.5 (3.35×) |
| Channel | 39.4 | 92.2 (2.34×) | 113.3 (2.87×) |
| IfaceCall | 182.2 | 398.1 (2.19×) | 439.2 (2.41×) |
| Iface | 94.8 | 557.4 (5.88×) | 476.8 (5.03×) |
| IfaceShell | 21.7 | 757.0 (34.88×) | 1,255.2 (57.84×) |
| RefLower | 234.4 | 643.8 (2.75×) | 1,852.3 (7.90×) |

**Peak memory** (working set, MB -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 4.3 | 47.4 | 12.5 |
| Fib | 5.7 | 48.5 | 14.1 |
| Sieve | 26.4 | 67.3 | 33.8 |
| MatMul | 11.0 | 54.4 | 20.4 |
| String | 5.7 | 57.2 | 23.0 |
| StringView | 5.7 | 48.7 | 14.1 |
| StringMatch | 5.7 | 57.2 | 22.9 |
| Map | 158.8 | 164.2 | 130.4 |
| Sort | 21.9 | 64.7 | 29.4 |
| Channel | 5.7 | 53.4 | 19.3 |
| IfaceCall | 5.7 | 48.0 | 14.1 |
| Iface | 5.7 | 48.4 | 14.4 |
| IfaceShell | 5.7 | 68.4 | 35.7 |
| RefLower | 5.8 | 48.3 | 15.3 |

<!-- PERF-RESULTS:END -->

### Reading the results

- **Startup:** Go wins cold process start decisively. The JIT pays runtime load plus
  assembly-loading and Go package initialization for the full converted-stdlib closure the binary
  references; **Native AOT removes the JIT-on-the-fly cost and starts several times faster than
  the JIT**, but still runs the same package initializers, so a gap to Go remains. For CLI-shaped
  programs AOT is the deployment story on time — see the memory note below for its trade.
- **Memory:** the working-set columns carry the cost of the full converted standard library. The
  JIT column's floor is the .NET runtime plus loaded assemblies; the AOT column's is *higher* —
  the self-contained binary maps the whole compiled closure into the process — so AOT currently
  trades memory for its startup and per-benchmark wins. Reducing both floors is optimization
  surface (trimming eligibility, lazy package init), not a semantic cost.
- **Function calls / integers (Fib):** the closest compute workload — ~1.6× under the JIT and
  ~1.5× under Native AOT, the one tight loop where AOT leads the JIT rather than trailing it.
- **Slices & floats (Sieve, MatMul):** the gap is `slice<T>` header emulation and bounds checks the
  JIT can't always elide, compounded on nested `[][]float64` access. **AOT is *slower* than the JIT
  here** — ILC lacks the JIT's dynamic PGO/OSR loop optimizations, trading tight-loop throughput for
  AOT's startup and memory wins.
- **String:** the price of materialization — every `[]byte`→`string` round-trip is an allocation +
  copy through the `@string` emulation, versus Go's `append` chain inlining to a few instructions. This
  benchmark's conversions are all **ineligible** for the stack-string optimization (its `s` is a
  concat operand and its buffer is mutated), so they stay `@string` — see StringView for the eligible case.
- **StringView:** the same `[]byte`→`string` cost, but for the subset the converter proves
  non-escaping and read/compare-only, where it emits a zero-copy stack string (`sstring`) instead of
  `@string` (see [ConversionStrategies-Reference](https://github.com/ritchiecarroll/go2cs/blob/master/docs/ConversionStrategies-Reference.md)).
  The converter hoists one `sstring` view per call rather than re-materializing it per comparison,
  since the JIT won't lift a `ref struct` view out of a loop on its own. **Runs at parity with Go
  or better in both C# variants** — and the number to watch as the eligibility surface widens; arc
  detail in
  [DESIGN-string-literal-allocation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-string-literal-allocation.md).
- **StringMatch:** literal-heavy hot paths — switch-on-string dispatch, `strings.HasPrefix` against
  a literal prefix, literal returns, literal map-key counters. The instrument for the same
  literal-comparison optimizations StringView exercises (span operators, `u8` casts, hoisted
  literals); arc detail in
  [DESIGN-string-literal-allocation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-string-literal-allocation.md).
- **Map:** transpiled C# is *faster than Go* — `map<K,V>` rides .NET's heavily-optimized
  `Dictionary`, and the AOT build wins by the widest margin on this insert/lookup/delete churn.
- **Sort:** the runtime's `sort.Interface` shim (`Interface<T>`) binds `Len`/`Less`/`Swap` via
  reflection-created delegates — cached, but a delegate hop per comparison.
- **Channel:** `channel<T>` + goroutine emulation over managed threading vs Go's runtime scheduler —
  real unbuffered rendezvous, single-fire select, operand-once hoisting. Currently ~2.3–2.8× on
  this producer→consumer churn: the rendezvous rides managed synchronization primitives where Go's
  scheduler hands off directly, a cost the cooperative-scheduler arc
  ([DESIGN-cooperative-scheduler.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-cooperative-scheduler.md))
  owns. Notably machine-sensitive — measure on your own hardware before drawing conclusions.
- **IfaceCall:** pure interface method dispatch — no asserts, no shell construction, just calling
  through an interface value built once, in a megamorphic hot loop. The floor for what *calling*
  through an interface costs, distinct from *obtaining* one (the two rows below).
- **Iface — the everyday interface story:** dispatch through statically-known interface values, a
  comma-ok assertion, and a type switch over a closed set, all resolved by generated adapters and
  ordinary casts — the compile-time (nominal) machinery, with no runtime shell construction.
  Mechanism notes in
  [DESIGN-iface-shell-caching.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-iface-shell-caching.md).
- **IfaceShell — the one operation C# has no native answer for:** satisfying an interface
  **structurally at run time**. Go resolves this with a cached itab lookup — two machine words, a
  nanosecond hash probe. C# has no two-word interface value, so go2cs constructs a wrapper ("shell")
  that forwards to the concrete value on every assertion; the ratio is the price of that
  construction plus, on the value-typed tier, a reflective forwarded call. An assertion the
  converter can resolve at compile time (the Iface row above) skips this path via a generated
  adapter instead. AOT can be *slower* here, unlike Startup: its generic shell tier can degrade to
  the reflective object shell, and AOT's reflective invokers can't emit IL stubs. Optimization
  directions:
  [DESIGN-iface-shell-caching.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-iface-shell-caching.md).
- **RefLower:** the ж-bound hot path — pointer parameters feeding pointer parameters, address-taken
  locals, field addresses — after the ref-lowering arc replaced its heap boxes with native `ref`
  (before that arc this shape ran ~25× Go; the lowering brought the JIT to ~2.9×). AOT currently
  trails the JIT here by a wide margin (~8×) — ILC's codegen of the ref-lowered loop is a priced
  open question for the arc's next phase
  ([DESIGN-zh-box-reduction.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-zh-box-reduction.md)).

### What the AOT column costs to produce — the honesty footnote

The table above shows what a Native AOT binary does at **run time**. It does not show what that
binary costs to **build**, and the cost is large enough that omitting it would mislead:

- **Build time.** Each benchmark is its own self-contained publish, and ILC compiles the **entire
  converted-stdlib closure** (~three hundred packages) into it, largely single-threaded (~1.1–1.3
  effective cores regardless of machine). Measured at the .NET 10 hop: **hours per publish** on
  laptop-class hardware — roughly an order of magnitude over the .NET 9 era on identical input —
  so a full 14-row AOT re-baseline is days serial, overnight when publishes run concurrently on a
  high-memory box (measured figures and the run series:
  [DATA-hopN-perf.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DATA-hopN-perf.md)).
- **Build memory.** A publish peaks at roughly **15–18 GB working set** (re-measure at each hop —
  the floor moves with the corpus). A 16 GB machine swaps; concurrent publishes need that much
  *per lane*. The produced images are ~300 MB self-contained, sized by the closure rather than the
  benchmark — while the same binaries **run** in tens of megabytes.
- **Why it is this way.** ILC is a **whole-program** compiler: reachability, the generic
  instantiation closure, cross-assembly inlining and tree-shaking are all computed per-application
  from that application's roots. Nothing is reusable between applications and there is no
  incremental mode — which is also precisely what pays for the AOT column's wins (the dead-code-free
  images and the runtime working-set reductions the memory row shows). The compile cost and the
  lean output are two ends of one design choice, so quoting the wins without the costs would be
  quoting half the trade.
- **Contrast, and a future direction.** Go's toolchain compiles packages **separately into
  reusable, cached per-package objects** and links quickly, so its equivalent of this table's
  whole column rebuilds in seconds — the model a future ILC-side improvement would want (a
  persistent compilation cache, or per-assembly pre-optimized inputs; ReadyToRun is today's
  cacheable cousin, at materially weaker optimization). Until something like that exists, the
  working mitigation here is a **compile farm**: publishes parallelized across a high-memory
  machine, binaries adopted onto the measuring host only after measurement-identity is proven —
  hashes cannot certify AOT builds, which are size-deterministic but not byte-deterministic
  (fresh module IDs each rebuild).

## History

Prior toolchain tables live here so cross-toolchain comparisons stay honest: same benchmarks, same
harness, same methodology, same host — only the toolchain moved.

### Compile provenance of the current (.NET 10) table

The Native AOT column above is **mixed-provenance**, deliberately and under a measured licence. A
single Native AOT publish of this corpus costs ~3.3 h on the measurement host, so the fourteen-cell
AOT ladder was completed by publishing eight of the binaries on a second fleet machine and adopting
them here, each SHA-256-verified on receipt:

| provenance | rows |
|:--|:--|
| **canon** — published on the measurement host itself | Startup, Fib, Sieve, MatMul, String, RefLower |
| **farm-adopted** — published on the fleet's i9-13900K, hash-verified, then measured here | StringView, StringMatch, Map, Sort, Channel, IfaceCall, Iface, IfaceShell |

Every row, whatever its provenance, was **measured on the canon host only**, and every adopted
binary passed the suite's own Verify phase (output-identical to Go) before it was timed. The
adoption is licensed by a direct A/B rather than by assumption: one benchmark published on both
machines measured **0.25 %** apart, against a **1.19 %** empirical null established by two
known-equivalent binaries from the same host — that is, the cross-machine difference is smaller
than the host's own run-to-run spread. Raw data, including all four binary hashes:
[`docs/phase4/evidence-aot-farm-ab-session.md`](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/evidence-aot-farm-ab-session.md).

### .NET 9 (SDK 9.0.316) — 2026-08-24

**Environment:** AMD Ryzen 5 PRO 6650U with Radeon Graphics · Microsoft Windows 10.0.26200 · go1.23.1 · .NET SDK 9.0.316 · 2026-08-24

**Execution time** (milliseconds -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 25.8 | 245.3 (9.52×) | 79.2 (3.07×) |
| Fib | 119.8 | 180.3 (1.51×) | 175.3 (1.46×) |
| Sieve | 71.7 | 128.3 (1.79×) | 265.4 (3.70×) |
| MatMul | 105.6 | 209.3 (1.98×) | 598.5 (5.67×) |
| String | 104.6 | 1,208.3 (11.56×) | 1,600.3 (15.31×) |
| StringView | 19.5 | 21.7 (1.12×) | 18.9 (0.97×) |
| StringMatch | 198.6 | 992.8 (5.00×) | 1,279.8 (6.44×) |
| Map | 630.5 | 546.4 (0.87×) | 230.7 (0.37×) |
| Sort | 135.0 | 461.4 (3.42×) | 497.8 (3.69×) |
| Channel | 46.6 | 96.0 (2.06×) | 130.9 (2.81×) |
| IfaceCall | 181.8 | 395.8 (2.18×) | 427.2 (2.35×) |
| Iface | 96.5 | 538.7 (5.58×) | 462.7 (4.80×) |
| IfaceShell | 21.3 | 871.9 (40.90×) | 1,319.0 (61.87×) |
| RefLower | 239.1 | 624.7 (2.61×) | 1,890.7 (7.91×) |

**Peak memory** (working set, MB -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 2.5 | 46.1 | 76.9 |
| Fib | 5.3 | 46.1 | 75.8 |
| Sieve | 25.9 | 65.6 | 96.6 |
| MatMul | 10.6 | 53.4 | 83.4 |
| String | 5.2 | 55.8 | 85.7 |
| StringView | 5.3 | 46.8 | 77.1 |
| StringMatch | 5.3 | 56.1 | 87.0 |
| Map | 157.9 | 165.8 | 187.1 |
| Sort | 21.4 | 62.4 | 92.2 |
| Channel | 5.3 | 52.0 | 84.3 |
| IfaceCall | 5.3 | 46.3 | 77.4 |
| Iface | 5.3 | 46.6 | 77.4 |
| IfaceShell | 5.3 | 66.6 | 96.3 |
| RefLower | 5.3 | 46.9 | 77.1 |

**What the .NET 10 hop changed.** Two results dominate, and they are why this table is kept:

- **The Native AOT working-set penalty inverted, and did so universally.** Under .NET 9, AOT's peak
  working set was *higher than the JIT's on all fourteen rows* (Startup 76.9 MB vs 46.1). Under
  .NET 10 it is *lower on all fourteen* (Startup 12.5 vs 47.4; Fib 14.1 vs 48.5; IfaceShell 35.7
  vs 68.4). The self-contained floor that made Native AOT the memory-expensive choice is gone.
  Both C# variants still sit well above Go, which holds 4–6 MB on most rows.
- **AOT startup more than halved** — 79.2 ms to 36.7 ms, 3.07× to 1.60× Go — while **JIT startup
  regressed ~14 %** (245.3 to 279.4 ms). The two variants moving in opposite directions on the same
  benchmark is what identifies that regression as runtime-load-side rather than
  converted-closure-side: the closure is identical in both.

Elsewhere the hop is mostly favourable and often within the noise band. The largest single JIT gain
is String (11.56× to 6.78× Go), with StringMatch (5.00× to 4.70×) and IfaceShell (40.90× to 34.88×)
following it down; the AOT column's time cells are broadly flat outside Startup.

## Performance-floor exploration

The dated floor exploration (stock-SDK trim profiles vs bflat; concluded 2026-08-16 — the floor
is a trim-rooting question, not a toolchain question) lives in
[`docs/PLAN-bflat-perf-exploration.md`](https://github.com/ritchiecarroll/go2cs/blob/master/docs/PLAN-bflat-perf-exploration.md),
results section included.
[`run-performance-floor.ps1`](https://github.com/ritchiecarroll/go2cs/blob/master/src/tests/Performance/run-performance-floor.ps1)
is its measurement harness and is not part of the canonical suite above. (Links are absolute
because this file mirrors verbatim to `docs/Performance.md` after each canonical run.)

<!-- AUTO-COPIED from src/tests/Performance/README.md by run-performance.ps1 -- edit that file, not this one. -->
