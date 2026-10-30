# Tuning converted code

Converted Go runs on .NET, and .NET gives you choices: how the program is compiled and deployed, how the runtime
compiles it while it runs, and a few converter options that change what it costs. This page says which choices matter,
what each one trades, and which to leave alone. Every claim names the platform it was measured on. For the numbers,
see the [performance comparison](Performance.md), which is re-measured as the converter changes.

**In short:** keep the .NET defaults. Publish with ReadyToRun or Native AOT when start time matters, and prefer the JIT over
Native AOT when tight loops dominate. Do not turn tiered compilation or dynamic PGO off for speed: on `win-x64`,
the default runtime ran a pointer-heavy benchmark in roughly 40% less time than with either one off.

## Choosing a deployment

A converted program can run three ways. All three run the same converted code; they differ in when it is compiled.

| Deployment | How you get it | Compiled |
|:--|:--|:--|
| **JIT** | `dotnet run`, `dotnet build`, or a framework-dependent `dotnet publish` | as each method first runs, then recompiled hotter as it keeps running |
| **ReadyToRun** | `dotnet publish -c Release` of a converted program, which turns it on | ahead of time, then the JIT can still recompile hot methods |
| **Native AOT** | `dotnet publish -r <rid> -p:PublishAot=true` | entirely ahead of time, to a native executable with no JIT |

Which wins depends on what the program spends its time on. The `win-x64` comparisons below come from the
performance comparison's table, measured on an earlier converter release; that page dates it.

- **Start time.** On `win-x64`, a Native AOT program started in roughly an eighth of the time the same program took
  under the JIT, though still slower than Go. On `linux-x64`, ReadyToRun cut a converted program's median start time by about a
  third against the JIT.
- **Tight loops.** On `win-x64`, the JIT ran slice-indexing, floating-point and pointer-heavy loops faster than
  Native AOT, which took roughly two to three times as long on each. Native AOT has no dynamic profile-guided optimization or
  on-stack replacement to tune a hot loop as it runs.
- **Maps.** On `win-x64`, converted map code ran faster than Go under both the JIT and Native AOT, and fastest under
  Native AOT.
- **Interfaces.** On `win-x64`, calling interface methods and asserting to concrete types took within about 15% of
  the same time under the JIT and Native AOT, with neither ahead on both. The one interface path with no compile-time
  answer, asserting a value to an interface it satisfies only structurally, took about two thirds longer under Native
  AOT than under the JIT.
- **Memory.** On `win-x64`, a Native AOT program's peak working set was lower than the JIT's on every benchmark.
- **Build cost.** A Native AOT publish compiles every converted package the program references, so it is slow and the
  executable is large. [Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
  has the measured cost and its platform.

<!-- Sources. The win-x64 rows: Performance.md's .NET 10 table (AMD Ryzen 5 PRO 6650U, Windows 10.0.26200, .NET SDK
     10.0.400, 2026-08-25): Startup JIT 279.4 ms vs AOT 36.7 ms (Go 22.9); Sieve JIT 126.1 vs AOT 242.4, MatMul 216.3 vs
     594.9, RefLower 643.8 vs 1,852.3; Map Go 631.2, JIT 422.3, AOT 248.8; IfaceCall 398.1 vs 439.2, Iface 557.4 vs
     476.8, IfaceShell 757.0 vs 1,255.2; peak working set lower under AOT on all fourteen rows; "ILC lacks the JIT's
     dynamic PGO/OSR loop optimizations" (Reading the results, Sieve/MatMul). The linux-x64 ReadyToRun figure:
     ConsumingGoFromCSharp.md, "Startup is faster from dotnet publish" (PerfStartup at a5aa6a23f7, median 488 ms JIT,
     329 ms ReadyToRun, -33%). The deployment table's commands: the converter's Exe template
     sets <PublishReadyToRun>true</PublishReadyToRun> (src/go2cs/csproj-template.xml), applied only by dotnet publish. -->

If you publish ReadyToRun as a single file, also set
`<PublishReadyToRunComposite>false</PublishReadyToRunComposite>`
([Known issues](KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory)).

## .NET runtime settings

Under the JIT and ReadyToRun, .NET compiles code in tiers. A method first gets a quick compile; one that keeps running
is recompiled with full optimization, guided by a profile the runtime collected while it ran (dynamic PGO). Native
AOT has no tiers and no dynamic PGO.

**Leave tiered compilation and dynamic PGO on.** On `win-x64`, a converted benchmark that takes field addresses in a
hot loop took roughly 40% less time with the defaults than with tiered compilation off, and about as much less than
with dynamic PGO off. Turning tiering off compiles every method once at full optimization, but without the profile
that the tiers collect, and the profile is worth more.

<!-- Source: the parity seat #32 amendment's FieldView-jit arms (BOARD, 2026-10-30): win-x64 (G-LAPTOP), 15
     interleaved rounds per arm, inner time medians: default tiering 2,015 ms (D) / 2,109 ms (T);
     DOTNET_TieredCompilation=0 3,546 / 3,525; DOTNET_TieredPGO=0 3,541 / 3,533. The default takes about 43% less time
     than either setting. -->

The settings, if you need them, each in three equivalent forms: a property in the project file, a value in
`runtimeconfig.json`, or an environment variable for one run.

| Setting | Project file | `runtimeconfig.json` | Environment |
|:--|:--|:--|:--|
| Tiered compilation | `<TieredCompilation>false</TieredCompilation>` | `"System.Runtime.TieredCompilation": false` | `DOTNET_TieredCompilation=0` |
| Dynamic PGO | `<TieredPGO>false</TieredPGO>` | `"System.Runtime.TieredPGO": false` | `DOTNET_TieredPGO=0` |
| Quick JIT for loops | `<TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops>` | `"System.Runtime.TieredCompilation.QuickJitForLoops": false` | `DOTNET_TC_QuickJitForLoops=0` |

Microsoft documents every runtime compilation setting in
[Runtime configuration options for compilation](https://learn.microsoft.com/dotnet/core/runtime-config/compilation).

**One setting to change on one runtime.** Quick JIT for loops is what lets the JIT replace a running loop with an
optimized one. Ubuntu's packaged .NET 10, rather than Microsoft's build, can fail in that path on Linux;
[Known issues](KnownIssues.md#ubuntus-packaged-net-10-can-end-a-converted-program-early) describes the fault
and when it applies. Turning quick JIT for loops off avoids it. On
`linux-x64`, under Canonical's .NET 10 build, that cost a converted program roughly a sixth more start time, and its
loop speed stayed within measurement noise.

<!-- Source: KnownIssues.md, the on-stack-replacement entry: Canonical's .NET 10.0.12 on 4-core Linux, PerfStartup wall
     median 561 ms default vs 653 ms with QuickJitForLoops off (+16%); PerfSieve 238 vs 249 ms, inside the spread. -->

**Two places in go2cs turn tiering off on purpose, and neither is advice.**

- **Converted test hosts.** A `-tests` host runs with tiered compilation off by default, so every method is compiled
  once at full optimization and a test verdict cannot depend on when the runtime promotes a method.
  [`-test-tiered`](CommandLine.md) turns it back on. That trades speed for determinism, which a test needs and a
  program does not; a production program should not copy it.
- **The performance runner's diagnostic arms.** The [performance suite](Performance.md) can add a run with tiered
  compilation off, and measurements of individual changes also read runs with dynamic PGO off. They separate a cost
  the tiers recover from a cost the code always pays. They are measurement axes, not settings to deploy: a change
  that reads smaller with tiering off is still faster with the defaults.

<!-- The test host: BuildingGo2cs.md ("run with the CLR's tiered JIT off (DOTNET_TieredCompilation=0)") and
     CommandLine.md's -test-tiered row. The runner: src/tests/Performance/PerformanceRunner, `--tc0` ("Add the
     diagnostic TC0 arm (the JIT binary, DOTNET_TieredCompilation=0)"; ReleaseRun.cs: "diagnostic arms ... recorded,
     never in the published table"). -->

## Converter and runtime options

Most converter options change how the emitted C# reads, or which code is converted. They do not change what the
program costs to run. These do.

### String literals

By default a converted program keeps a string literal where Go has it: `F("hello")`. golib converts each literal from
C#'s UTF-16 to Go's UTF-8 once and caches the result, so each later evaluation is a lookup that allocates nothing.
`-hoist-literals` gives the literals named `static readonly` fields instead, `F(helloˢ)`, so an evaluation is a
plain field read. The standard library is already converted this way, so code that calls into it gets the benefit
without the flag.

**When it helps:** a tight loop that calls a small function, not inlined, that returns or passes a literal.

- On `win-x64` (an Intel Core i9-13900K), a loop calling a small standard-library function that returns a literal
  took roughly twice as long with plain literals under the JIT, once the JIT had optimized the function, and roughly
  1.7 to 1.8 times as long under Native AOT. A smaller such function took up to a quarter longer under Native AOT.
- On `win-x64` (an AMD Ryzen 5 PRO 6650U):
  - a literal returned from a small function called in a hot loop took roughly 1.7 to 1.8 times as long plain,
    under both the JIT and Native AOT;
  - a literal argument inside the loop took roughly 1.6 times as long under the JIT, and a sixth longer under Native
    AOT;
  - a local assigned from a literal took between a tenth and a seventh longer.
- In whole programs on the i9: a literal-heavy string-matching benchmark took roughly an eighth more time plain under
  the JIT, and the same time under Native AOT. Parsing URL queries took about the same time either way.

**When it does not:** a literal evaluated rarely costs one lookup either way. A literal passed as an `any` argument,
`fmt.Println("x")`, measured the same both ways: both forms pass one pre-boxed value. And hoisting is not always
faster. On the i9 under the JIT, once the function was optimized, a small standard-library function ran faster plain,
because the JIT inlined the plain form into the caller's loop and did not inline the hoisted one. Measure your own hot path.

**What it costs:** the emitted C# names each literal as a field, declared above the function that first uses it, so
the code reads less like the Go. [Plain literals](ConversionStrategies-Reference/strings.md#a-literal-in-a-string-slot-is-a-plain-c-literal)
describes both forms.

<!-- Sources: docs/phase4/DESIGN-literal-hoisting-survival.md section 9 (seat #52, 2026-10-30).
     i9-13900K, win-x64, .NET 10.0.14, 15 interleaved rounds x 2 passes, A/A band. Test 3, a user loop of 10M calls
     (master aaec1d4ca8): Hash.String plain/hoisted 2.029 and 2.023 on the steady JIT, 1.686 and 1.782 on AOT;
     FormatBool 1.121 and 1.250 on AOT, and 0.86 on the steady JIT (plain inlined, hoisted not; cause unexplained).
     The JIT axis: "steady JIT" = DOTNET_TC_CallCountingDelayMs=0. Under default tiering both arms stayed at Tier0
     (about 28 ns) and read equal; the i9 is investigating why the callees never left Tier0.
     Test 1, PerfStringMatch (893d48976f): plain/hybrid 1.13 on the JIT, 1.004 to 1.007 on AOT. Test 2, net/url
     ParseQuery: flat on the JIT; AOT plain/hybrid 1.02, which the i9 reads as layout.
     Ryzen 5 PRO 6650U (G-LAPTOP), win-x64, .NET 10.0.11, LiteralSiteBench, 15 interleaved rounds, per-round medians:
     return C2 1.707 (JIT) and 1.772 (AOT); argument C1 1.637 and 1.169; local C5 1.148 and 1.112; any slot C6 1.008
     and 1.003, inside the A/A band. Data: docs/phase4/data/seat52-literal-classes. -->

### Goroutine stacks

Each goroutine runs on its own thread, and the thread reserves a fixed stack, because a .NET stack cannot grow the
way a Go stack does. The reserve is 256 MiB, or 16 MiB for a goroutine the converter proves cannot recurse
([how](ConversionStrategies.md#goroutines)). It is address space, not memory, but it is what a thread start pays for:
on `win-x64`, parking 100,000 goroutines at once took roughly five times as long with the 256 MiB reserve as with
16 MiB. On `linux-x64`, at 10,000 goroutines, the reserve made no measurable difference, and a kernel limit on
memory mappings, not the reserve, stopped thread creation at around 16,000.

`GO2CS_GOROUTINE_STACK` sets the 256 MiB reserve and `GO2CS_GOROUTINE_BOUNDED_STACK` the 16 MiB one, as a byte
count with an optional `KiB`, `MiB` or `GiB` suffix; `0` takes .NET's default thread stack. Lower the first only
for a program that starts many goroutines at once and never recurses deeply in one: recursion past the reserve ends
the process with no panic and no deferred calls ([Limitations](Limitations.md)).

<!-- Source: BOARD, R's 1.26.10.2 seat #3 row (2026-10-27), ParkStorm, Release, one run at a time. win-x64 (R-LAPTOP,
     16 hardware threads), N=100,000, time to park / private MB: 256 MiB 361 s / 5,900; 16 MiB 70 s / 5,650 (1 MiB
     67 s, 4 MiB 66 s, 64 MiB 86 s). linux-x64 (WSL Ubuntu 22.04, 10 cores): N=10,000 parks in 5.7 s (default) /
     6.4 s (16 MiB); N=20,000 stops at about 16,200 threads at both reserves (vm.max_map_count 65,530). The variables
     and their syntax: src/core/golib/runtime/Goroutine.cs, ResolveStackReserve and BoundedReserve. -->

### What else was considered

| Option | Why it is not a tuning option |
|:--|:--|
| `-var`, `-uco`, `-indent`, `-comments`, `-provenance`, `-license`, `-tree` | Change how the emitted C# reads or what metadata it carries; the program does the same work. |
| `-goroot`, `-gopath`, `-go2cspath`, `-stdlib`, `-recurse`, `-csproj`, `-tags`, `-platforms`, `-cgo` | Choose which Go code is converted and for which platforms, not how it runs. |
| `-debug`, `-convert-timeout`, `-platform-census`, `-version`, `-nuget-map-*`, `-module-*` | Govern the conversion itself. |
| `-tests`, `-test-*` | Converted test hosts only; `-test-tiered` is covered [above](#net-runtime-settings). |
| `GOGC`, `GOMEMLIMIT` | Read and reported, but .NET's collector does not follow them ([Limitations](Limitations.md)). |
| `GO2CS_GC_PAUSE_HISTORY=0` | Changes what `runtime` reports about garbage collection, not how the program runs. |
| golib's `FieldViewCache.Disabled` | A switch for one test, which checks that `&s.f` stays one pointer as it does in Go. Setting it breaks that. |
