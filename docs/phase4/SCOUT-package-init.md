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

## 1. Result in one paragraph

A converted hello spends its start-up compiling, not computing. Built with `dotnet build` (the shape `dotnet run`, the
behavioral tests and every in-tree measurement use), its package initializers take 359 ms and 313 ms of that is the
JIT compiling 2,413 methods; the initializers' own work is 46 ms. Published as the project template publishes it
(golib and the packages ReadyToRun), the initializers still take 238 ms: 101 ms compiling 1,114 methods that
ReadyToRun cannot precompile, and 137 ms of type loading, method preparation and execution. `runtime` is half of
either. Three findings carry most of the available gain, in order of gain over risk: **(a)** the template's
call-counting delay of 0 costs 13 to 22 ms on four CPUs and about 160 ms on one, and a delay of 10 ms gets it all
back (§5.1); **(b)** every pointer box, slice and map over a package's own struct type is compiled at start-up in
both shapes, 648 methods and 54 to 58 ms on hello (§5.2); **(c)** a built program is not precompiled at all, which
is 115 ms against the published one (§5.3).

Measured 2026-11-14 on one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400), Release, at master
`a6cefba6ee`. This VM session is slower than the design's (hello built: 459 to 477 ms here, 336 ms there), so
compare within this document only. Instruments and raw rows: `docs/phase4/data/package-init/`.

## 2. The three shapes

Wall time of one process, medians of interleaved rounds (`multi.py`), four CPUs unless said.

| program | wall | CPU time | on one CPU (`taskset -c 0`) |
|:--|--:|--:|--:|
| plain C# hello (the floor) | 26 ms | 26 ms | |
| converted hello, `dotnet build` (JIT only) | 459 to 477 ms | 695 to 723 ms | 626 to 640 ms |
| converted hello, `dotnet publish` (template default: ReadyToRun for golib and the packages) | 328 to 351 ms | 528 to 551 ms | 462 ms |

CPU time above wall time is the background compiler, which the call-counting delay of 0 starts at once (§5.1).

## 3. Per package, exclusive

The hook runs each of the 63 assemblies' module initializers in dependency order, so each row is that package's own
work. Medians of 12 runs (build) and 10 runs (publish). "rest" is wall minus JIT: type loading, method preparation
and the initializer's own execution. The published rows come from an untrimmed, hook-enabled copy of the publish
(328 ms shipped, 338 ms the copy).

| package | build: wall | JIT | rest | methods compiled | publish: wall | JIT | rest | methods compiled |
|:--|--:|--:|--:|--:|--:|--:|--:|--:|
| runtime | 183.6 | 161.2 | 22.4 | 1,196 | 126.6 | 51.1 | 75.4 | 642 |
| internal/cpu | 48.7 | 42.2 | 6.5 | 241 | 33.7 | 18.9 | 14.8 | 145 |
| internal/godebug | 23.4 | 20.8 | 2.6 | 163 | 12.9 | 6.8 | 6.2 | 64 |
| os | 23.1 | 20.5 | 2.6 | 211 | 14.2 | 6.7 | 7.5 | 81 |
| golib | 16.5 | 10.6 | 5.9 | 51 | 12.6 | 3.6 | 9.1 | 23 |
| syscall | 14.7 | 13.8 | 0.9 | 137 | 10.3 | 5.4 | 4.9 | 65 |
| unicode | 8.4 | 7.9 | 0.5 | 48 | 5.5 | 3.2 | 2.3 | 39 |
| time | 6.9 | 6.6 | 0.4 | 60 | 4.3 | 1.6 | 2.6 | 22 |
| internal/poll | 4.7 | 4.4 | 0.3 | 44 | 3.9 | 1.9 | 2.0 | 21 |
| the other 54 | 29.0 | | | | 13.9 | | | |
| **all 63** | **359.0** | **312.8** | **46.2** | **2,413** | **237.9** | **100.9** | **137.0** | **1,114** |

Loading the 63 assemblies costs a further 35 to 38 ms in either shape. The initializers allocate 1.0 MB in total.
After the initializers, `main` itself (the first `fmt.Println`) is about 30 to 35 ms in the sampled traces.

## 4. Per kind

**What the JIT compiles on the start-up thread** (per-method durations from the runtime's JIT events,
`kinds-build.txt`, `kinds-publish.txt`; one run each, and the event listener itself adds methods that are excluded):

| kind | build: methods | ms | publish: methods | ms |
|:--|--:|--:|--:|--:|
| package static constructors (static data, map and table builds) | 13 | 72.2 | 0 | 0 |
| module initializers | 57 | 19.3 | 1 | 0.1 |
| package functions the initializers call | 786 | 75.3 | 9 | 2.4 |
| golib generic types over a package's own type (`ж<T>`, `StandardBox<T>`, `slice<T>`, `array<T>`, `StaticBox<T>`) | 648 | 53.7 | 648 | 58.2 |
| golib generic types over golib or framework types | 232 | 20.5 | 68 | 7.1 |
| golib, not generic | 377 | 53.6 | 99 | 15.7 |
| framework generic types over Go value types (`ConcurrentDictionary`, `Dictionary`, `List`) | 235 | 28.7 | 230 | 33.3 |
| framework, other | 61 | 7.2 | 55 | 6.8 |

`runtime`'s package static constructor alone is 26,069 bytes of IL and 55 ms of JIT in the built shape. In both
shapes `ж<T>` is the largest generic: 356 methods and 35 ms built, 319 and 33 ms published.

**The brief's kinds, as measured:**

| kind | hello, built | hello, published | how read |
|:--|:--|:--|:--|
| JIT of the initializers and what they call | 313 ms | 101 ms | hook, JIT time on the thread |
| static data, map and table builds, `init` bodies (execution) | inside 46 ms | inside 137 ms | wall minus JIT; not split further |
| registrations (`ᴛRegisterAdapters`, `GoDeferredRegistrations`) | no sample | no sample | sampled traces: neither frame appears in hello's start-up |
| golib start-up | 16.5 ms | 12.6 ms (19 ms inclusive in the trace) | hook row; trace |
| assembly loading | 38 ms | 35 ms | hook, load column |

The published shape's 137 ms of "rest" is mostly not managed execution: the sampled trace of the published start-up
thread reads 80 ms of managed CPU time and 261 ms in native code, with 36 ms exclusively under `runtime`'s module
initializer and 32 ms under the program's (type loads, method entry preparation, assembly loads). This scout did
not split it further; a native profile (`perf` with the runtime's map files) would.

## 5. The large items

For each: what Go does, what could change, and the risk to initialization order. Order is observable in Go and
none of the options below reorders a package's initializer or runs it later than Go would, except where said.

### 5.1 The call-counting delay of 0 (config; candidate for THIS release)

The project template sets `System.Runtime.TieredCompilation.CallCountingDelayMs` to 0 (`GoCallCountingDelayMs`,
seat #53, 2026-10-30), so the runtime starts recompiling hot methods at the optimizing tier during start-up. The
background compiler then competes with the initializers.

| published hello | delay 0 (today) | 10 ms | 30 ms | 100 ms (.NET's default) |
|:--|--:|--:|--:|--:|
| wall, four CPUs | 336 | 322 | 313 | 320 |
| CPU time, four CPUs | 528 | 308 | 288 | 289 |
| wall, one CPU | 462 | 300 | 305 | 317 |

Built hello: 477 to 460 ms on four CPUs at 100 ms (CPU 723 to 444), and 640 to 437 ms on one CPU. The A/A control
on four CPUs read -4 ms. `Performance/README.md` records the trade knowingly ("a slightly longer ReadyToRun start,
measured at about 30 ms on this machine"), which agrees with the four-CPU reading here. What it does not record is
the one-CPU case, where the cost is about five times that (a container with a CPU limit, a small cloud instance, a
CI runner).

- **Option:** a default of 10 ms. It returns the whole start-up cost on this VM and still starts optimizing 90 ms
  sooner than .NET's default. **Owed before it lands:** the steady-state rows of the performance suite at 0 and at
  10, by the lane that owns them; this scout measured start-up only.
- **Gain:** 13 to 22 ms on four CPUs, about 160 ms on one, and 220 to 280 ms of CPU time everywhere.
- **Risk:** none to semantics or order. A performance trade the owner already ruled once, so it is the owner's call.

### 5.2 Generic golib types over package types are compiled at start-up in every shape

Go's compiler emits the code for `*T`, `[]T` and `map[K]V` at compile time. Here each is a golib generic type, and
an instantiation over a struct is compiled per type. ReadyToRun compiles each assembly on its own, so a package's
image cannot hold `ж<T>` code for its own `T` (the generic's IL is golib's): all 648 such methods are compiled by
the JIT in both shapes, 54 to 58 ms on hello, 319 to 356 of them `ж<T>` members (`viewOf`, `viewOfSlow`,
`tryFindView`, `publishView`, the static constructor) and 143 to 185 `StandardBox<T>`.

- **Options:** (1) move what does not depend on `T`'s layout out of the generic box types into non-generic code
  compiled once (the per-`T` residue should be a few small members); (2) create the box of a package-level
  variable on first address-take, not in the static constructor (`StaticBox<T>`: 86 methods); (3) compile each
  package with golib inside its version bubble, so its image holds these instantiations, which ties a package's
  image to the exact golib build.
- **Gain:** up to about 50 ms on hello in both shapes, more on programs with more types. (1) is the likely half.
- **Risk:** (1) and (2) are golib-internal and do not touch order; (2) changes when a box is created, which nothing
  in Go observes. (3) is a servicing constraint, not a semantic one. All three need a design; queued.

### 5.3 A built program is not precompiled

`dotnet publish` gives the packages ReadyToRun code; `dotnet build` and `dotnet run` do not, and neither do the
behavioral harness or in-tree measurements. That is 115 ms on hello (459 against 344 ms), 164 ms on one CPU. Go
has no such difference: `go run` and `go build` produce the same compiled code.

- **Option:** ship ReadyToRun images inside the `go.*` packages (per RID), or compile them at restore or first
  build into a cache. **Gain:** about 115 ms for every `dotnet run`. **Risk:** none to semantics; package size
  and a per-RID packaging matrix. Queued; packaging lane.

### 5.4 Framework collections over Go value types

230 methods and 29 to 33 ms in both shapes, 100 of them `ConcurrentDictionary` instantiations keyed by a Go value
type (`go.uintptr`, `UIntPtr`), which no precompiled image holds. They are golib's and the hand-owned `runtime`'s
start-up tables.

- **Options:** key those tables by a primitive the framework already carries code for, or create them on first use.
  **Gain:** up to about 30 ms. **Risk:** golib-internal; no order effect. Queued, small.

### 5.5 `runtime`, `internal/cpu`, `internal/godebug`, `os`: work Go does at link time or in assembly

- `runtime` (127 to 184 ms): its package static constructor is one 26 KB method that builds every package-level
  variable. Go lays those out as initialized data at link time. Published, the constructor is precompiled and the
  remaining cost is the 642 generic methods of §5.2 and §5.4 plus type loading. A further split of `runtime`'s 75 ms
  of non-JIT time needs the native profile named in §4.
- `internal/cpu` (34 to 49 ms): Go reads CPUID in assembly and fills a struct. The converted
  `initX86FeatureDetection` and `registerX86Options` run as converted code and append to a slice of options through
  boxes (40 ms inclusive in the published trace). A hand-owned fast path that fills the same struct from
  `System.Runtime.Intrinsics` at the same point in the order would remove most of it. Order: unchanged if it runs
  where `internal/cpu.Initialize` runs today.
- `internal/godebug` (13 to 23 ms) and `os` (14 to 23 ms): setting parsing and registration, and the three standard
  files (`newFile`), with reflection over custom attributes on the way (11 ms of JIT built). Not rooted further.
- `unicode` (5.5 to 8.5 ms): range tables built at run time; Go has them as read-only data. Pure data with no
  observable initializer, so building each table on first use is order-safe.

Each of these is a queued item of 10 to 40 ms; none is cheap and safe enough for this release without its own
seat.

### 5.6 Ranked

| # | item | gain on hello | risk | when |
|:--|:--|:--|:--|:--|
| 1 | call-counting delay 0 to 10 ms (§5.1) | 13 to 22 ms (4 CPUs), about 160 ms (1 CPU), 220+ ms CPU | none to semantics; steady-state check owed | **this release, on the owner's call** |
| 2 | thinner generic boxes; lazy static boxes (§5.2) | up to about 50 ms, both shapes | golib-internal | queued, needs a design |
| 3 | framework tables keyed by primitives or lazy (§5.4) | up to about 30 ms, both shapes | golib-internal | queued, small |
| 4 | `internal/cpu` fast path (§5.5) | 25 to 40 ms | hand-own; order held by placement | queued |
| 5 | precompiled packages for `dotnet build` and `dotnet run` (§5.3) | about 115 ms, built shape only | packaging | queued, packaging lane |
| 6 | `unicode` tables on first use; `godebug`, `os` roots (§5.5) | 5 to 20 ms each | low; `godebug` and `os` unrooted | queued |

## 6. Predictions scored, as worded

| # | predicted | measured | score |
|:--|:--|:--|:--|
| P1 | JIT is the largest kind, 55% to 75% of the time before `main` | built: 313 of 359 ms of initializer time (87%), about 73% of the whole process before `main`; published: 101 of 238 ms (42%), where type loading and preparation is larger | HIT for the built shape at the top of the range or above it by the initializer denominator; **MISS for the published shape**, which the prediction did not separate |
| P2 | the initializers' own execution under 20% | built: at most 46 of 359 ms (13%) | HIT (an upper bound: the 46 ms includes type loading) |
| P3 | registrations under 10 ms | no sample in either trace | HIT |
| P4 | `runtime` first, 90 to 140 ms; five largest over 70% | first; 184 ms built, 127 ms published; five largest 82% and 84% | MISS on the range (built is above it), HIT on rank and share |
| P5 | ReadyToRun removes 40% to 60% of wall | it removes 25% (459 to 344 ms) | **MISS**, at the falsifier's edge. The half that remains is §5.2, §5.4 and type loading |
| P6 | golib start-up under 25 ms | 16.5 ms built, 12.6 ms published | HIT |
| P7 | no table build over 10 ms of execution | not read per table; bounded by each package's non-JIT time (0.5 ms for `unicode`, 22 ms for all of `runtime` built) | not falsified; a bound, not a reading |

What the predictions missed as a class: I treated "the program" as one shape. The shipped shape was already
precompiled, and the published program's remaining cost is a different mix from the built one's.

## 7. Limits

- One program (hello), one VM, one OS. `wide` and the TypeScript hello of P4's probe were not measured here; P4's
  split, when it lands, is the cross-check for §5.2 on a program with many more types.
- The per-kind JIT table is one run per shape with an event listener attached; the per-package table is the
  10- and 12-run median without it. Use the first for proportions and the second for milliseconds.
- The published per-package rows come from an untrimmed copy with start-up hooks enabled; the shipped publish
  trims the framework and disables hooks, and reads 10 ms faster.
- Native AOT was not measured: it has no JIT and no tiers, so §5.1 to §5.3 do not apply to it, and its start-up
  needs its own reading.
- Every gain in §5.6 is an estimate from an attribution, not from a built change. None is additive with certainty:
  §5.2 and §5.4 overlap with §5.5's `runtime` and `internal/cpu` rows.

— P5 (2026-11-14)
