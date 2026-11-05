# Go vs transpiled C# — runtime performance comparison

> **Measured on:** the results below were measured on G-LAPTOP, an AMD Ryzen 5 PRO 6650U running
> Windows 11. The C# columns are Windows x64 (`win-x64`): each program is converted by go2cs from
> Go 1.26.10 and references the published `go.*` 1.26.10.2 packages from nuget.org, and it runs on
> .NET 10 (SDK 10.0.400, runtime 10.0.11). The Go column is the same programs built by Go 1.26.10 for
> `windows/amd64`. The `win-x64` Native AOT images were compiled on a second machine (SDK 10.0.403,
> ILC 10.0.14) and timed on G-LAPTOP. The tables under History were measured on the same
> `win-x64` machine, and each names its own toolchain.

<!-- Source: docs/phase4/DATA-hop126-perf.md section 1 (host, toolchain, RID per line, the legs) and section 3 (the
     table). Runner 226cd9a25d (master ce5072e657). -->

A small, targeted benchmark suite answering the question people ask first: **how fast is transpiled
C# compared to the original Go?** — startup time and run time, under the normal JIT runtime, a
**ReadyToRun** publish (what `dotnet publish` of a converted program gives you), and **Native AOT**
(self-contained, the closest deployment analog to a Go binary).

Each benchmark is a tiny Go program (same shape as the
[behavioral tests](https://github.com/ritchiecarroll/go2cs/tree/master/src/tests/Behavioral)),
chosen to exercise one Go construct with a real C# cost model — slices, strings, maps, channels,
interface dispatch — plus raw compute loops where the two runtimes should be close. This is not an
exhaustive benchmark game; it gives the common expected range of differences. The short version:
transpiled C# is **usually slower than Go, but not universally** — maps run faster than Go in every
C# column, and so do recursive function calls (Fib) under the JIT and ReadyToRun, while the rest
ranges from about 2× on tight compute under the JIT to the duck-typed interface asserts and the boxed
field addresses, the honest outliers at the other end.

To choose a deployment and runtime settings for your own program, see
[Tuning converted code](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md): it explains which knobs
exist, what each trades, and why the suite's diagnostic runs with tiering off are not advice. The suite itself
is not tuned: each program is converted with go2cs's default settings and built from the project file the converter
writes, the way a user would. The [fairness notes](#methodology--fairness-notes) say exactly what that covers.

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
| **IfaceShellChurn** | 1M iterations of the same duck-typed asserts as IfaceShell, but every assert sees a NEW dynamic value (a fresh struct, a fresh pointer, and `errors.Is` over a fresh `*fs.PathError`). IfaceShell is the case a shell cache wins on; this is the case it can only lose on, so the pair bounds what a cache costs. `run-shell-churn-bound.ps1` holds that cost to 1.15x time and 1.25x peak working set against the same row built with the cache compiled out. |
| **RefLower** | A pointer-heavy hot loop: pointer parameters passed on to pointer parameters, address-taken locals and field addresses — the shapes the converter's ref-lowering turns from heap boxes into native C# `ref`s. |
| **FieldView** | Field addresses that stay boxed: methods on `*element` taking `*element` operands and returning the receiver (crypto/internal/fiat's P-256 shape), every operand an `&p.f` and its own call site, over a pointer and over `&pts[i]`. Measures the field-view cache (golib `ж.Views.cs`) matching each site to its field's one view, on a pointer's box and on fresh element boxes. |
| **TlsHandshake** | 64 full TLS 1.3 handshakes over loopback, one fresh connection each, after one warm-up handshake; the certificate is parsed before the clock starts. Isolates the handshake latency that `net/http`'s h2 write-deadline tests bound only indirectly. |

Every benchmark prints a deterministic **checksum** (verified byte-identical across Go, C# JIT, and
C# AOT before anything is measured) plus its own workload time measured in-program via
`time.Now().UnixNano()` — so the headline numbers exclude process startup, which is reported
separately by the Startup row.

## Methodology / fairness notes

- **Nothing is tuned for these benchmarks.** Each program is converted with the converter's default settings and
  built from the project file the converter writes, with no setting added for any one benchmark. The Go column is a
  plain `go build`. The JIT column is a plain `Release` build of the converted project. The ReadyToRun column is the
  converted project's own publish, `dotnet publish -c Release -r win-x64`, whose self-contained, partial-trim,
  ReadyToRun shape the converter's project file sets. The Native AOT column adds only the standard Native AOT publish
  properties to that publish: `-p:PublishAot=true` with the same runtime identifier, on the project's own
  self-contained, partial-trim shape. No compiler, garbage-collector or tiering setting is added, and the runner sets
  no runtime environment variable for a timed run. None of the opt-in settings in
  [Tuning converted code](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md) is applied:
  `-hoist-literals` ([string literals](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md#string-literals)),
  `GoReadyToRunUserCode` ([deployments](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md#choosing-a-deployment)),
  or the tiered-compilation, dynamic PGO and quick-JIT-for-loops settings
  ([runtime settings](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md#net-runtime-settings)). The
  diagnostic runs with tiering off never appear in the table.
- **The table is what a user gets.** Each program references the published `go.*` packages from nuget.org, as
  published, and is converted with that release's defaults: the program's own string literals stay plain, and calls
  are counted from the start (a call-counting delay of 0). The tables under History were converted with the defaults
  of their own releases, and each says how those differ.
- **Four variants of the identical program:** the Go binary (`go build`, default optimized), the
  transpiled C# built `Release` framework-dependent (JIT column), the same C# published by the converted
  project's own `dotnet publish` (ReadyToRun column), and the same C# published with `PublishAot=true`
  self-contained, partial trim (Native AOT column).
- **Median of 5 runs** (configurable; 11 for Startup, Sort, Channel, IfaceCall, Iface and RefLower, and for
  FieldView under Native AOT) after 1 discarded warmup, single-shot process executions — the
  way a Go CLI actually runs. The JIT and ReadyToRun columns deliberately include in-process tiered-JIT warmup inside
  the workload; long-running server workloads would look better for them than these numbers do.
- **Peak memory** is the process peak working set, polled while it runs. Both wall time and in-program
  workload time are captured; tables report workload time (Startup row: wall). The current table reports time only;
  the Go 1.23.1 tables under History report peak memory too.
- Benchmarks avoid nondeterminism (no `math/rand`; inline xorshift/LCG generators), so outputs are
  byte-comparable, and print timing on a filtered `elapsed_ns:` line.
- **Published tables come from a single designated host per era** — the Environment line names the
  part — so the History section's cross-toolchain comparisons (e.g., .NET 9 → 10) are always
  same-machine. Ratios from different hardware are not comparable and are never mixed.
- **The AOT column's numbers are runtime numbers; producing them is expensive, and we say so.**
  Each Native AOT publish compiles the entire converted-stdlib closure whole-program, which is also
  exactly what buys the column's lean images and runtime memory wins. For the current table each
  publish took about 9 to 10 minutes on the machine that compiled it, with the compiler peaking at 12.4 to
  13.5 GB, and TlsHandshake's took about 26 minutes at a 24 GB peak. The
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

Requirements: Go 1.26.10 (the release go2cs converts), a .NET SDK matching the corpus target
framework (`net10.0`, named by `src/Directory.Build.props`), and — for the AOT column — a native
linker and toolchain, which differs by host:

| Host | Native AOT prerequisite |
|:--|:--|
| **Windows** | MSVC C++ build tools — Visual Studio 2022's "Desktop development with C++" workload, which supplies the `link.exe` ILC needs. The runner prepends the VS Installer directory to `PATH` so the SDK's `vswhere` probe finds it. |
| **Linux** | `clang` and `zlib1g-dev` (Debian/Ubuntu: `sudo apt install clang zlib1g-dev`; Fedora: `clang zlib-devel`). ILC shells out to `clang` for the native link step. |
| **macOS** | The Xcode command line tools (`xcode-select --install`), which supply `clang` and `ld`. |

Deliberately **not** scripted: `run-performance.ps1` does not install any of these. A benchmark
harness that silently mutates the machine's toolchain is not a harness anyone should trust with a
performance claim. `--no-aot` drops the whole column and needs none of them.
<!-- Rationale: [PLAN-linux-operation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/PLAN-linux-operation.md)
     (an internal record; its visible link was removed 2026-10-26 under owner ruling OQ-1 on visitor pages). -->

The environment line of the results block reads the CPU name from the registry on Windows and from
`/proc/cpuinfo` on Linux, so a published table always names the part that produced it.

The runner (`PerformanceRunner`), a dependency-free console app structured like the behavioral suite's
`BehavioralRunner`, runs **Transpile → Build → Verify → Measure**. Verify requires all three binaries
to produce identical (timing-filtered) stdout before anything is timed, so the table can never
silently report a benchmark that computes something different in C#.

## Results

The table below has all 17 of the suite's programs, measured against the published `go.*` 1.26.10.2
packages. The same programs measured in the same session against `go.*` 1.24.13.7 are
[under History](#go-124137-measured-in-the-same-session), with the older tables.

<!-- PERF-RESULTS:BEGIN -->

**Environment:** AMD Ryzen 5 PRO 6650U with Radeon Graphics (G-LAPTOP) · Microsoft Windows 10.0.26300 · go1.26.10 · .NET SDK 10.0.400, runtime 10.0.11 · `go.*` 1.26.10.2 · 2026-11-04/05

**Columns**, one RID and shape each:

- **Go:** `windows/amd64`, default `GOAMD64`, built by Go 1.26.10.
- **C# (JIT):** `win-x64`, framework-dependent `Release` build with its apphost, on .NET runtime 10.0.11.
- **C# (ReadyToRun):** `win-x64`, the converted project's own `dotnet publish -c Release -r win-x64`: self-contained,
  partial trim, with golib and the `go.*` packages compiled ReadyToRun and the program's own code left IL-only, so the
  JIT compiles it as it runs.
- **C# (Native AOT):** `win-x64`, the same publish with `-p:PublishAot=true`: self-contained, partial trim, the
  default instruction set. Compiled on a second machine (.NET SDK 10.0.403, ILC 10.0.14) and timed on G-LAPTOP once
  its SHA-256, size and converted-source hash matched.

Median of 5 runs (11 for Startup, Sort, Channel, IfaceCall, Iface and RefLower, and for FieldView under Native AOT),
1 discarded warmup. Workload time is measured in-program and excludes process startup; the Startup row is pure
process wall time. Ratios are relative to Go: the JIT and ReadyToRun ratios to the Go column, timed in the same
window, and the Native AOT ratio to the same Go binaries timed again in the Native AOT window.

**Execution time** (milliseconds -- lower is better):

| Benchmark | Go | C# (JIT) | C# (ReadyToRun) | C# (Native AOT) |
|---|---:|---:|---:|---:|
| Startup | 26.1 | 506.4 (19.40×) | 371.4 (14.23×) | 158.8 (6.40×) |
| Fib | 115.1 | 96.9 (0.84×) | 101.3 (0.88×) | 177.5 (1.52×) |
| Sieve | 62.8 | 134.0 (2.13×) | 135.7 (2.16×) | 295.0 (5.13×) |
| MatMul | 123.0 | 217.0 (1.76×) | 215.7 (1.75×) | 726.8 (6.17×) |
| String | 103.8 | 340.1 (3.28×) | 655.7 (6.32×) | 1,396.5 (13.24×) |
| StringView | 9.6 | 21.9 (2.28×) | 21.7 (2.26×) | 19.2 (1.85×) |
| StringMatch | 174.7 | 776.2 (4.44×) | 789.2 (4.52×) | 1,274.4 (7.33×) |
| Map | 661.7 | 251.0 (0.38×) | 252.0 (0.38×) | 285.6 (0.44×) |
| Sort | 140.1 | 252.5 (1.80×) | 252.4 (1.80×) | 543.6 (4.00×) |
| Channel | 39.8 | 78.5 (1.97×) | 83.7 (2.10×) | 103.6 (2.58×) |
| IfaceCall | 184.4 | 336.2 (1.82×) | 345.2 (1.87×) | 559.6 (3.12×) |
| Iface | 90.4 | 375.3 (4.15×) | 376.6 (4.17×) | 572.8 (6.80×) |
| IfaceShell | 22.3 | 602.7 (27.03×) | 581.9 (26.09×) | 1,177.5 (52.75×) |
| IfaceShellChurn | 36.9 | 1,083.4 (29.36×) | 1,073.1 (29.08×) | 1,712.0 (46.93×) |
| FieldView | 41.7 | 1,725.3 (41.37×) | 1,911.4 (45.84×) | 2,347.1 (57.76×) |
| RefLower | 240.6 | 510.2 (2.12×) | 536.3 (2.23×) | 644.5 (2.69×) |
| TlsHandshake | 128.2 | 1,057.9 (8.25×) | 1,078.7 (8.41×) | 2,534.2 (20.41×) |

**Notes on this table:**

- **The package version floats.** A converted project asks for the newest matching release (`GoStdLibVersion`
  `1.26.*`), so this table's builds resolved `go.*` 1.26.10.2, the newest on the day. To reproduce it after a later
  1.26 release publishes, pin the version on every build and publish: `-p:GoStdLibVersion=1.26.10.2`.
- **The ReadyToRun column's shape is this release's.** From `go.*` 1.26.10.2 on, a converted program publishes its own
  code IL-only and golib and the packages ReadyToRun. Under 1.24.13.7 the program's own code was ReadyToRun too.
- **Calls are counted from the start.** The converted project sets a call-counting delay of 0
  (`GoCallCountingDelayMs`), so under the JIT and ReadyToRun the runtime starts optimizing hot methods at once. That
  buys a faster steady state at the cost of a slightly longer ReadyToRun start, measured at about 30 ms on this
  machine. `<GoCallCountingDelayMs>runtime</GoCallCountingDelayMs>` restores .NET's own default
  ([Tuning converted code](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md#net-runtime-settings)).
  Native AOT has no tiers, so the setting does not reach it.
- **TlsHandshake's Native AOT cell is its first reading.** Under `go.*` 1.24.13.7 that program's Native AOT publish
  cannot link (`LNK1183`, an object file past the COFF format's 4 GiB limit), so the History table reads n/a there.
- **Two rows are slower under full optimization than with 1.24.13.7.** IfaceShellChurn and StringMatch take longer
  under Native AOT than they did with the 1.24.13.7 packages, while both are faster under the JIT and ReadyToRun. Both causes are
  identified: StringMatch pays a per-use conversion of its string literals, and IfaceShellChurn pays its
  interface-shell cache on every miss. A program that hits StringMatch's case can convert with `-hoist-literals`
  ([string literals](https://github.com/ritchiecarroll/go2cs/blob/master/docs/Tuning.md#string-literals)).

<!-- Sources, all docs/phase4/DATA-hop126-perf.md: the table, section 3 (ratios as that section states them, which
     divide the rounded medians; the runner's own report, DATA-hop126-perf/w1-runner-report.txt, divides unrounded
     ones, e.g. Startup JIT 19.44x). Environment: w1-runner-report.txt's Environment line, section 1's toolchain
     (SDK 10.0.400, runtime 10.0.11; AOT on SDK 10.0.403, ILC 10.0.14). Windows: section 2 (window 1 2026-11-04,
     window 2 2026-11-05). Run counts: section 2's notes. The float and the pin: section 1's notes. The R2R shape:
     section 1 ("ReadyToRun shape per leg"). TlsHandshake: section 1 (AOT images) and section 6 item 7. The two
     full-optimization rows: section 4 (Native AOT regressions IfaceShellChurn and StringMatch, +11.7% and +10.4%;
     JIT and R2R improve) and section 6 item 4. The ReadyToRun start cost of the delay: docs/Tuning.md's source
     comment (seat #53, PerfStartup 312.6 -> 342.3 ms, ReadyToRun publish, default against
     GoCallCountingDelayMs=runtime). -->

<!-- PERF-RESULTS:END -->

### Reading the results

- **Startup:** Go wins cold process start decisively. The JIT pays runtime load plus
  assembly-loading and Go package initialization for the full converted-stdlib closure the binary
  references. ReadyToRun precompiles golib and the packages and starts in about three quarters of
  the JIT's time; **Native AOT removes the JIT-on-the-fly cost and starts about three times faster
  than the JIT**, but still runs the same package initializers, so a gap to Go remains. For CLI-shaped
  programs AOT is the deployment story on time.
- **ReadyToRun against the JIT:** past start-up, the ReadyToRun column runs within about a tenth of
  the JIT on most rows, because the program's own code is compiled by the JIT in both. String is the
  exception, at about twice the JIT's time.
- **Memory:** the Go 1.23.1 tables under History report peak memory. Both C# columns there carry the
  cost of the full converted standard library, so they sit well above Go on every row but Map. The
  JIT column's floor is the .NET runtime plus loaded assemblies; under .NET 10 the AOT column's floor
  is lower than the JIT's on every row. Reducing both floors is optimization surface (trimming
  eligibility, lazy package init), not a semantic cost.
- **Function calls / integers (Fib):** the closest compute workload: faster than Go under the JIT
  and ReadyToRun, and about 1.5× Go under Native AOT.
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
  `@string` (see the [strings reference](https://github.com/ritchiecarroll/go2cs/blob/master/docs/ConversionStrategies-Reference/strings.md#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring)).
  The converter hoists one `sstring` view per call rather than re-materializing it per comparison,
  since the JIT won't lift a `ref struct` view out of a loop on its own. **Runs at about 2× Go in
  every C# column, fastest under Native AOT** — and the number to watch as the eligibility surface widens.
  <!-- Design detail: [DESIGN-string-literal-allocation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-string-literal-allocation.md)
       (an internal record; visible link removed 2026-10-26, OQ-1). -->
- **StringMatch:** literal-heavy hot paths — switch-on-string dispatch, `strings.HasPrefix` against
  a literal prefix, literal returns, literal map-key counters. The instrument for the same
  literal-comparison optimizations StringView exercises (span operators, `u8` casts, hoisted
  literals).
  <!-- Design detail: [DESIGN-string-literal-allocation.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-string-literal-allocation.md)
       (an internal record; visible link removed 2026-10-26, OQ-1). -->
- **Map:** transpiled C# is *faster than Go* in every C# column — `map<K,V>` rides .NET's
  heavily-optimized `Dictionary`, and the JIT and ReadyToRun builds win by the widest margin on this
  insert/lookup/delete churn.
- **Sort:** the runtime's `sort.Interface` shim (`Interface<T>`) binds `Len`/`Less`/`Swap` via
  reflection-created delegates — cached, but a delegate hop per comparison.
- **Channel:** `channel<T>` + goroutine emulation over managed threading vs Go's runtime scheduler —
  real unbuffered rendezvous, single-fire select, operand-once hoisting. Currently about 2× to 2.6× on
  this producer→consumer churn: the rendezvous rides managed synchronization primitives where Go's
  scheduler hands off directly, a cost that planned cooperative-scheduler work addresses. Notably
  machine-sensitive — measure on your own hardware before drawing conclusions.
  <!-- Pre-2026-10-26 wording: "a cost the cooperative-scheduler arc
       ([DESIGN-cooperative-scheduler.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-cooperative-scheduler.md))
       owns" (an internal record; visible link removed under OQ-1). -->
- **IfaceCall:** pure interface method dispatch — no asserts, no shell construction, just calling
  through an interface value built once, in a megamorphic hot loop. The floor for what *calling*
  through an interface costs, distinct from *obtaining* one (the two rows below).
- **Iface — the everyday interface story:** dispatch through statically-known interface values, a
  comma-ok assertion, and a type switch over a closed set, all resolved by generated adapters and
  ordinary casts — the compile-time (nominal) machinery, with no runtime shell construction.
  <!-- Mechanism notes: [DESIGN-iface-shell-caching.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-iface-shell-caching.md)
       (an internal record; visible link removed 2026-10-26, OQ-1). -->
- **IfaceShell — the one operation C# has no native answer for:** satisfying an interface
  **structurally at run time**. Go resolves this with a cached itab lookup — two machine words, a
  nanosecond hash probe. C# has no two-word interface value, so go2cs constructs a wrapper ("shell")
  that forwards to the concrete value on every assertion; the ratio is the price of that
  construction plus, on the value-typed tier, a reflective forwarded call. An assertion the
  converter can resolve at compile time (the Iface row above) skips this path via a generated
  adapter instead. AOT can be *slower* here, unlike Startup: its generic shell tier can degrade to
  the reflective object shell, and AOT's reflective invokers can't emit IL stubs.
  <!-- Optimization directions: [DESIGN-iface-shell-caching.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-iface-shell-caching.md)
       (an internal record; visible link removed 2026-10-26, OQ-1). -->
- **RefLower:** the ж-bound hot path — pointer parameters feeding pointer parameters, address-taken
  locals, field addresses — after the converter's ref-lowering replaced its heap boxes with native `ref`
  (before that work this shape ran about 25× Go; the lowering brought the JIT within 3×). Native AOT
  trails the JIT here by about a quarter.
  <!-- Pre-2026-10-26 wording: "after the ref-lowering arc ... before that arc ... a priced open question for the
       arc's next phase ([DESIGN-zh-box-reduction.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DESIGN-zh-box-reduction.md))"
       (an internal record; visible link removed under OQ-1). -->
- **IfaceShellChurn:** the same duck-typed asserts as IfaceShell, but every assert sees a new
  dynamic value, so no shell is reused. It runs at about 29× Go under the JIT and ReadyToRun and
  about 47× under Native AOT.
- **FieldView:** field addresses that stay boxed, each `&p.f` operand matched to its field's one
  view. The highest ratio in the table: about 41× Go under the JIT, 46× under ReadyToRun and 58×
  under Native AOT.
- **TlsHandshake:** 64 full TLS 1.3 handshakes over loopback, at about 8× Go under the JIT and
  ReadyToRun. Native AOT takes about 2.4 times the JIT's time here.

<!-- The ratios in Reading the results are read from the table above (docs/phase4/DATA-hop126-perf.md section 3):
     Startup ReadyToRun/JIT 371.4/506.4 = 0.73, JIT/AOT 506.4/158.8 = 3.19; ReadyToRun within ~10% of the JIT on 14 of
     the 16 workload rows, the two outside it being String 655.7/340.1 = 1.93 and FieldView 1,911.4/1,725.3 = 1.108; RefLower
     AOT/JIT 644.5/510.2 = 1.26; TlsHandshake AOT/JIT 2,534.2/1,057.9 = 2.40 (section 6 item 7). Section 6 item 7's
     "the only row where AOT trails JIT by more than 2x" does not hold against section 3 (Sieve 2.20, MatMul 3.35,
     String 4.11), so the page does not repeat it. -->

### What the AOT column costs to produce — the honesty footnote

The table above shows what a Native AOT binary does at **run time**. It does not show what that
binary costs to **build**, and the cost is large enough that omitting it would mislead:

- **Build time.** Each benchmark is its own self-contained publish, and ILC compiles the **entire
  converted-stdlib closure** (~three hundred packages) into it, largely single-threaded (~1.1–1.3
  effective cores regardless of machine). Measured at the move to .NET 10: **hours per publish** on
  laptop-class hardware — roughly an order of magnitude over the .NET 9 era on identical input —
  so a full AOT re-baseline was days serial, overnight when publishes ran concurrently on a
  high-memory machine.
  <!-- Measured figures and the run series: [DATA-hopN-perf.md](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/DATA-hopN-perf.md)
       (an internal record; visible link removed 2026-10-26, OQ-1). -->
- **Build memory.** A publish peaked at roughly **15–18 GB working set**, and at 22.2 GB when
  re-measured on 2026-10-13 (re-measure at each Go or .NET release move — the floor moves with the
  corpus). A 16 GB machine swaps; concurrent publishes need that much *each*. The produced images
  are ~300 MB self-contained, sized by the closure rather than the benchmark — while the same
  binaries **run** in tens of megabytes.
- **Since 2026-10-14.** The build time and memory above were measured before golib's generic-cycle
  fix of that day. The fix cut a 31-package consumer's publish from about an hour to under four
  minutes on a hosted Linux runner
  ([Known issues](https://github.com/ritchiecarroll/go2cs/blob/master/docs/KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)).
  For the current table, built after the fix, each of this suite's publishes, which compile the
  whole converted standard library, took about 9 to 10 minutes on a second, higher-memory machine at a 12.4 to
  13.5 GB compiler peak, for an image of about 182 to 184 MB; TlsHandshake's took about 26 minutes
  at a 24 GB peak, for a 324 MB image.
  <!-- Source: docs/phase4/DATA-hop126-perf/aot-manifest-i9-1262.tsv, the 17 go1.26.10.2 lines (the template-default
       drop): wall_s 541-572 (TlsHandshake 1,534), peak_ws_gb (ilc.exe) 12.40-13.48 (TlsHandshake 23.97), size
       181,559,296-183,959,552 bytes (TlsHandshake 323,766,784). Build host -m:4, affinity 0xF, serial. -->- **Why it is this way.** ILC is a **whole-program** compiler: reachability, the generic
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

Prior tables live here so cross-release and cross-toolchain comparisons stay honest: same
benchmarks, same methodology, same host. Each table names its toolchain and says what else differed
from the current one.

### `go.*` 1.24.13.7, measured in the same session

The same 17 programs, each converted by the go2cs release that shipped `go.*` 1.24.13.7 and
referencing those packages from nuget.org, timed in the same session and on the same `win-x64`
machine as the current table, the two releases alternating run by run. Milliseconds, median, with
the current table's run counts; compare each cell with the current table's C# column of the same
name.

| Benchmark | C# (JIT) | C# (ReadyToRun) | C# (Native AOT, folding off) |
|---|---:|---:|---:|
| Startup | 607.8 | 333.6 | 580.6 |
| Fib | 181.8 | 179.6 | 178.9 |
| Sieve | 135.2 | 343.4 | 320.6 |
| MatMul | 214.3 | 634.5 | 732.1 |
| String | 732.8 | 1,621.8 | 1,394.2 |
| StringView | 18.9 | 645.6 | 24.1 |
| StringMatch | 896.0 | 1,469.5 | 1,154.8 |
| Map | 455.7 | 659.5 | 307.0 |
| Sort | 429.1 | 361.7 | 542.6 |
| Channel | 108.7 | 162.9 | 116.9 |
| IfaceCall | 388.2 | 1,510.7 | 520.1 |
| Iface | 533.8 | 844.8 | 564.3 |
| IfaceShell | 936.3 | 991.6 | 1,505.0 |
| IfaceShellChurn | 1,531.0 | 1,229.9 | 1,533.3 |
| FieldView | 9,449.3 | 6,341.7 | 6,055.6 |
| RefLower | 636.8 | 660.0 | 1,875.5 |
| TlsHandshake | 2,405.6 | 1,680.2 | n/a (LNK1183) |

How this table differs from the current one:

- **Converter defaults.** That release hoisted the string literals in a program's own code into named
  fields, and left .NET's own call-counting delay in place.
- **The ReadyToRun shape.** That release's publish compiled the program's own code ReadyToRun too, not
  only golib and the packages.
- **The Native AOT column is published with folding off.** With the 1.24.13.7 packages, a Native AOT
  publish needs `IlcFoldIdenticalMethodBodies=false` to finish at all
  ([Known issues](https://github.com/ritchiecarroll/go2cs/blob/master/docs/KnownIssues.md#with-the-go-124137-packages-or-earlier-a-native-aot-publish-can-take-hours-or-not-finish)).
  The current table's Native AOT column is the project's default publish.
- **TlsHandshake has no Native AOT cell.** Even with folding off, that program's publish fails at link
  (`LNK1183`): the compiler's object file passes the COFF format's 4 GiB limit.

<!-- Source: docs/phase4/DATA-hop126-perf.md section 3, the three 1.24.13.7 columns and the folding-off note beneath
     the table; section 1 (the legs; the ReadyToRun shape per leg; TlsHandshake's refusal, LNK1183, a 6.59 GB COFF
     object past 4 GiB). The converter defaults read at that release's record b0ef977fda: src/go2cs/csproj-template.xml
     carries no GoCallCountingDelayMs, and src/go2cs/hoistedLiteralOperations.go hoists with no opt-in flag (main.go
     has no -hoist-literals). The 1.24.13.7 leg's Go column (go1.24.13) is in w1-runner-report.txt for window 1;
     section 3 does not carry it, so the page does not either. -->

### Go 1.23.1, .NET 10 (SDK 10.0.400) — 2026-08-25

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

How this table differs from the current one:

- **Fourteen programs.** TlsHandshake, IfaceShellChurn and FieldView joined the suite after it was
  measured, and it has no ReadyToRun column.
- **A project reference, not the packages.** It was built against the standard library in this
  repository, by project reference. The current table references the published `go.*` packages from
  nuget.org.
- **Hoisted literals.** Its converter hoisted the string literals in the program's own code into named
  fields, the default then; the program's own literals now stay plain unless you pass `-hoist-literals`.
- **.NET's own call-counting delay.** It predates the converter's call-counting setting, so its JIT
  column ran with .NET's default delay.

#### Compile provenance of the Go 1.23.1 table

The Native AOT column above is **mixed-provenance**, deliberately and under a measured licence. A
single Native AOT publish of this corpus costs ~3.3 h on the measurement host, so the fourteen-cell
AOT ladder was completed by publishing eight of the binaries on a second, higher-memory machine and
adopting them here, each SHA-256-verified on receipt:

| provenance | rows |
|:--|:--|
| **canon** — published on the measurement host itself | Startup, Fib, Sieve, MatMul, String, RefLower |
| **farm-adopted** — published on the second machine, hash-verified, then measured here | StringView, StringMatch, Map, Sort, Channel, IfaceCall, Iface, IfaceShell |

Every row, whatever its provenance, was **measured on the canon host only**, and every adopted
binary passed the suite's own Verify phase (output-identical to Go) before it was timed. The
adoption is licensed by a direct A/B rather than by assumption: one benchmark published on both
machines measured **0.25 %** apart, against a **1.19 %** empirical null established by two
known-equivalent binaries from the same host — that is, the cross-machine difference is smaller
than the host's own run-to-run spread. The raw data, including all four binary hashes, is kept with the
project's measurement records.
<!-- Raw data: [`docs/phase4/evidence-aot-farm-ab-session.md`](https://github.com/ritchiecarroll/go2cs/blob/master/docs/phase4/evidence-aot-farm-ab-session.md)
     (an internal record; visible link removed 2026-10-26, OQ-1). -->

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

**What the move to .NET 10 changed.** Two results dominate, and they are why this table is kept:

- **The Native AOT working-set penalty inverted, and did so universally.** Under .NET 9, AOT's peak
  working set was *higher than the JIT's on all fourteen rows* (Startup 76.9 MB vs 46.1). Under
  .NET 10 it is *lower on all fourteen* (Startup 12.5 vs 47.4; Fib 14.1 vs 48.5; IfaceShell 35.7
  vs 68.4). The self-contained floor that made Native AOT the memory-expensive choice is gone.
  Both C# variants still sit well above Go, which holds 4–6 MB on most rows.
- **AOT startup more than halved** — 79.2 ms to 36.7 ms, 3.07× to 1.60× Go — while **JIT startup
  regressed ~14 %** (245.3 to 279.4 ms). The two variants moving in opposite directions on the same
  benchmark is what identifies that regression as runtime-load-side rather than
  converted-closure-side: the closure is identical in both.

Elsewhere the move is mostly favourable and often within the noise band. The largest single JIT gain
is String (11.56× to 6.78× Go), with StringMatch (5.00× to 4.70×) and IfaceShell (40.90× to 34.88×)
following it down; the AOT column's time cells are broadly flat outside Startup.

## Performance-floor exploration

A floor exploration compared stock-SDK trim profiles with bflat and found that the floor is a
trim-rooting question, not a toolchain question.
<!-- Concluded 2026-08-16. The exploration and its results section:
     [`docs/PLAN-bflat-perf-exploration.md`](https://github.com/ritchiecarroll/go2cs/blob/master/docs/PLAN-bflat-perf-exploration.md)
     (an internal record; visible link removed 2026-10-26, OQ-1). -->
[`run-performance-floor.ps1`](https://github.com/ritchiecarroll/go2cs/blob/master/src/tests/Performance/run-performance-floor.ps1)
is its measurement harness and is not part of the canonical suite above. (Links are absolute
because this file mirrors verbatim to `docs/Performance.md` after each canonical run.)

<!-- AUTO-COPIED from src/tests/Performance/README.md by run-performance.ps1 -- edit that file, not this one. -->
