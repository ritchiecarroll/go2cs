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
| **ReadyToRun** | `dotnet publish -c Release` of a converted program, which turns it on | golib and the standard library ahead of time, then the JIT can still recompile hot methods; the program's own code as it runs |
| **Native AOT** | `dotnet publish -r <rid> -p:PublishAot=true` | entirely ahead of time, to a native executable with no JIT |

Which wins depends on what the program spends its time on. The `win-x64` comparisons below come from the
performance comparison's current table, measured on one Windows machine (an AMD Ryzen 5 PRO 6650U) against the
`go.*` 1.26.10.2 packages; a bullet that uses another source names it.

- **Start time.** On `win-x64`, a Native AOT program started in about a third of the time the same program took
  under the JIT, though still slower than Go, and a ReadyToRun publish started in about three quarters of the JIT's
  time. On `linux-x64`, ReadyToRun cut a converted program's median start time by about a third against the JIT.
- **Tight loops.** On `win-x64`, the JIT ran slice-indexing and floating-point loops faster than Native AOT, which
  took about 2.2 and 3.3 times as long. On a pointer-heavy loop, Native AOT took about a quarter longer than the JIT.
  Native AOT has no dynamic profile-guided optimization or on-stack replacement to tune a hot loop as it runs.
- **Maps.** On `win-x64`, converted map code ran faster than Go under the JIT, ReadyToRun and Native AOT, and fastest
  under the JIT and ReadyToRun.
- **Interfaces.** On `win-x64`, calling interface methods and asserting to concrete types took about 1.5 to 1.7 times
  as long under Native AOT as under the JIT. The one interface path with no compile-time answer, asserting a value to
  an interface it satisfies only structurally, took about twice as long under Native AOT as under the JIT.
- **Memory.** On `win-x64`, in the performance comparison's Go 1.23.1 table under History, a Native AOT program's
  peak working set was lower than the JIT's on every benchmark. The current table reports time only.
- **Build cost.** A Native AOT publish compiles every converted package the program references, so it is slow and the
  executable is large. [Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
  has the measured cost and its platform.

<!-- Sources from 2026-11-09. The start-time, tight-loop, map and interface bullets: Performance.md's current table
     (G-LAPTOP, AMD Ryzen 5 PRO 6650U, Windows 10.0.26300, win-x64, SDK 10.0.400 / runtime 10.0.11, go.* 1.26.10.2,
     2026-11-04/05). Startup JIT 506.4, ReadyToRun 371.4 (0.73), AOT 158.8 ms (0.31; Go 26.1); Sieve JIT 134.0 vs AOT
     295.0 (2.20), MatMul 217.0 vs 726.8 (3.35), RefLower 510.2 vs 644.5 (1.26); Map Go 661.7, JIT 251.0, ReadyToRun
     252.0, AOT 285.6; IfaceCall 336.2 vs 559.6 (1.66), Iface 375.3 vs 572.8 (1.53), IfaceShell 602.7 vs 1,177.5
     (1.95). The comment below is the earlier derivation; it still sources the memory bullet, the linux-x64 figure
     and the deployment table's commands. -->
<!-- Sources. The win-x64 rows: Performance.md's .NET 10 table (AMD Ryzen 5 PRO 6650U, Windows 10.0.26200, .NET SDK
     10.0.400, 2026-08-25): Startup JIT 279.4 ms vs AOT 36.7 ms (Go 22.9); Sieve JIT 126.1 vs AOT 242.4, MatMul 216.3 vs
     594.9, RefLower 643.8 vs 1,852.3; Map Go 631.2, JIT 422.3, AOT 248.8; IfaceCall 398.1 vs 439.2, Iface 557.4 vs
     476.8, IfaceShell 757.0 vs 1,255.2; peak working set lower under AOT on all fourteen rows; "ILC lacks the JIT's
     dynamic PGO/OSR loop optimizations" (Reading the results, Sieve/MatMul). The linux-x64 ReadyToRun figure:
     ConsumingGoFromCSharp.md, "Startup is faster from dotnet publish" (PerfStartup at a5aa6a23f7, median 488 ms JIT,
     329 ms ReadyToRun, -33%). The deployment table's commands: the converter's Exe template
     sets <PublishReadyToRun>true</PublishReadyToRun> (src/go2cs/csproj-template.xml), applied only by dotnet publish. -->

**A single-file publish compiles ReadyToRun one assembly at a time.** For a self-contained single-file ReadyToRun
publish, the .NET SDK's default is one composite image of the program and the framework. A converted program's project
also compresses the bundle, so every start decompresses that image first. The project therefore sets
`<PublishReadyToRunComposite>false</PublishReadyToRunComposite>` unless you set it yourself, as the publish profiles
go2cs writes already do. Start-up wall time of a `dotnet publish -p:PublishSingleFile=true` on `win-x64`:

| ReadyToRun | Bundle | Start-up | Also |
|---|---|---|---|
| composite | compressed | 3,480 ms | a 571 MB executable; a string-matching loop ran 27% slower |
| one assembly at a time | compressed | 470 ms | |
| composite | uncompressed | 320 ms | a 1.70 GB executable, and about 700 s to publish |

To build the composite image anyway, pass `-p:PublishReadyToRunComposite=true`. On `linux-x64` the composite image can
also run the ReadyToRun compiler out of memory
([Known issues](KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory)).

<!-- Sources. G's measurement, win-x64, a converted program published with a profile-less `dotnet publish
     -p:PublishSingleFile=true` (docs/phase4/DATA-hop126-perf §11.8). The SDK default: Microsoft.NET.CrossGen.targets
     sets PublishReadyToRunComposite to true for a single-file ReadyToRun publish and forces it to false unless the
     publish is self-contained, which `dotnet publish` makes a single-file publish. The template's line:
     src/go2cs/csproj-template.xml, guarded by TestTemplateTurnsTheCompositeReadyToRunImageOffUnlessAsked and
     TestPlainSingleFilePublishEvaluatesReadyToRunPerAssembly (src/go2cs/singleFileComposite_test.go). -->

**Your own code is compiled as it runs.** A ReadyToRun publish of a converted program precompiles golib and the
standard library, which is where start time is won. It leaves the program's own assemblies, every project built from
your module, to the JIT. ReadyToRun compiles each assembly on its own: precompiled code cannot inline a call into
another assembly, and a loop that runs only once never leaves its precompiled form. On `win-x64` (an AMD Ryzen 5 PRO
6650U), against a publish that precompiled everything:

- a published loop calling `strconv.FormatBool` ran in about an eighth of the time;
- a string-matching benchmark's hot phase took about two thirds of the time;
- start time stayed within run-to-run variation, and the published folder came out a few tens of KB smaller.

To precompile your own code as well, set `<GoReadyToRunUserCode>true</GoReadyToRunUserCode>`. That is rarely
worth it. Across the [performance suite](Performance.md), on the same machine, precompiling the program's own code
makes 11 of the 17 programs slower than the default ReadyToRun publish and none faster. FieldView, Fib, RefLower,
Sort, Startup and TlsHandshake stay within measurement noise. Medians in ms; the ratio is precompiled-user-code
against the default:

| Benchmark | default ReadyToRun | `GoReadyToRunUserCode` | ratio |
|:--|--:|--:|--:|
| StringView | 21.8 | 605.2 | 27.792 |
| IfaceCall | 339.6 | 1458.1 | 4.294 |
| String | 653.5 | 1995.5 | 3.054 |
| MatMul | 213.8 | 589.4 | 2.757 |
| Sieve | 139.5 | 265.2 | 1.901 |
| Iface | 376.1 | 660.4 | 1.756 |
| StringMatch | 804.7 | 1215.2 | 1.510 |
| Channel | 79.9 | 117.1 | 1.466 |
| IfaceShell | 574.6 | 728.4 | 1.268 |
| IfaceShellChurn | 1071.7 | 1188.1 | 1.109 |
| Map | 248.9 | 268.9 | 1.080 |

<!-- Source: docs/phase4/DATA-hop126-perf.md section 10.x (G-LAPTOP, win-x64, SDK 10.0.400 / runtime 10.0.11,
     window 1): R2Ru against the template's R2R over the 17 programs, moved rows only, numbers as the record gives
     them. Floor per program = the larger of the two arms' IQRs and window 1's 2.8% JIT null (no per-arm A/A in
     this window), stated as such in the record. Raw: docs/phase4/DATA-hop126-perf/w1-r2ru-rest.tsv and w1-r2ru-review.tsv, the
     reader sweep/r2ruread.sh and its verdict sweep/r2ru-verdict.tsv (it reproduces the verdict byte for byte). -->

<!-- Source: docs/phase4/DESIGN-jit-inlining-tiering-publish.md section 9 (seat #53, G-LAPTOP, .NET 10.0.11, 15
     interleaved rounds, A/A band per program): r2rmod main loop 10.205 -> 1.190 ns/call (0.119), its library
     package's loop 5.421 -> 0.848 (0.156); PerfStringMatch hot phase 1,142 -> 725 ms (0.643); PerfStartup and
     PerfTlsHandshake wall inside the band (-0.0%, +0.3%); sizes -18 KB, -23 KB, -29 KB. -->


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

Across the [performance suite](Performance.md), on `win-x64` (an AMD Ryzen 5 PRO 6650U), turning
dynamic PGO off with `DOTNET_TieredPGO=0` makes 13 of the 17 programs slower under the JIT and none faster; Startup,
Fib, Sieve and Channel stay within measurement noise. Turning it off is not recommended. Medians in ms, in-program
time; the ratio is PGO off against the default:

| Benchmark | default | `DOTNET_TieredPGO=0` | ratio |
|:--|--:|--:|--:|
| String | 339.0 | 1465.3 | 4.322 |
| IfaceCall | 358.3 | 807.0 | 2.252 |
| MatMul | 213.2 | 393.7 | 1.847 |
| FieldView | 1723.7 | 2917.6 | 1.693 |
| TlsHandshake | 1071.6 | 1513.4 | 1.412 |
| Iface | 374.7 | 528.0 | 1.409 |
| StringView | 20.9 | 29.4 | 1.405 |
| StringMatch | 791.2 | 1057.9 | 1.337 |
| IfaceShellChurn | 1076.8 | 1432.7 | 1.331 |
| RefLower | 511.3 | 645.7 | 1.263 |
| IfaceShell | 600.4 | 738.2 | 1.230 |
| Sort | 255.1 | 305.5 | 1.197 |
| Map | 253.6 | 264.6 | 1.044 |

<!-- Source: docs/phase4/DATA-hop126-perf.md section 10, the tuning sweep (G-LAPTOP, win-x64, AMD Ryzen 5 PRO 6650U,
     SDK 10.0.400 / runtime 10.0.11; 11 interleaved rounds per program, every arm twice as its own A/A; a cell moves
     only beyond the larger of its two arms' A/A gaps and IQRs). The table "TieredPGO=0 under JIT (JG/JP)": the moved
     rows only, numbers as the record gives them, each the mean of its arm's two A/A medians (variant = JIT plain under DOTNET_TieredPGO=0, baseline = JIT plain
     with the defaults). Only the JIT was swept with PGO off. Raw: docs/phase4/DATA-hop126-perf/sweep/sweep-verdict.tsv
     (rows JG/JP), samples sww-arms-1.tsv and sww-arms-2.tsv. -->

The settings, if you need them, each in three equivalent forms: a property in the project file, a value in
`runtimeconfig.json`, or an environment variable for one run.

| Setting | Project file | `runtimeconfig.json` | Environment |
|:--|:--|:--|:--|
| Tiered compilation | `<TieredCompilation>false</TieredCompilation>` | `"System.Runtime.TieredCompilation": false` | `DOTNET_TieredCompilation=0` |
| Dynamic PGO | `<TieredPGO>false</TieredPGO>` | `"System.Runtime.TieredPGO": false` | `DOTNET_TieredPGO=0` |
| Quick JIT for loops | `<TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops>` | `"System.Runtime.TieredCompilation.QuickJitForLoops": false` | `DOTNET_TC_QuickJitForLoops=0` |
| Call-counting delay | `<GoCallCountingDelayMs>0</GoCallCountingDelayMs>` (a converted program's own property) | `"System.Runtime.TieredCompilation.CallCountingDelayMs": 0` | `DOTNET_TC_CallCountingDelayMs=0` |
| Server garbage collection | `<ServerGarbageCollection>true</ServerGarbageCollection>` | `"System.GC.Server": true` | `DOTNET_gcServer=1` |
| Dynamic adaptation (DATAS) | `<GarbageCollectionAdaptationMode>0</GarbageCollectionAdaptationMode>` | `"System.GC.DynamicAdaptationMode": 0` | `DOTNET_GCDynamicAdaptationMode=0` |

Microsoft documents every runtime compilation setting in
[Runtime configuration options for compilation](https://learn.microsoft.com/dotnet/core/runtime-config/compilation). The garbage collector's settings are in
[Runtime configuration options for garbage collection](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector).

**Server garbage collection is an option for an allocation-heavy, long-running program.** Server GC keeps one heap
and one collector thread per core, and collects in parallel. DATAS, which .NET turns on by default with server GC,
shrinks the heap count to fit the live data; turning it off keeps a heap on every core. On `linux-x64`, on a 4-vCPU
cloud VM, the converted TypeScript type checker takes about a third less time over a slice of its compiler tests with
both settings above, and its peak memory is about 15% higher. The cost is memory. It grows with the core count,
because each core has its own heap, so measure on the machine you deploy to. The settings take effect only in an
executable: in a library, the host application's own settings decide. A converted project keeps .NET's default,
workstation GC, because most programs are small or short-lived and do not repay the memory.

<!-- Source: P4 perf scout, branch claude/p4-ts-perf-scout 895e08794d, docs/phase4/SCOUT-ts-perf.md section 4 and
     docs/phase4/data/ts-perf/experiments.tsv + runtime-knobs.tsv. linux-x64, 4 vCPU Xeon 2.1 GHz, 15 GB, .NET SDK
     10.0.400, JIT Release; converted TypeScript at go2cs master 651c4ad147, checkdemo over the first 25 units of
     units-101.txt, 1 warm-up + median of 3, same output hash on every run. Base (workstation concurrent GC) 35.83 s
     wall, 1,252 MB peak RSS; DOTNET_gcServer=1 + DOTNET_GCDynamicAdaptationMode=0: 24.37 s (-32%), 1,447 MB (+15.6%);
     server GC with DATAS on: 25.3 s (-29%). Owner ruling 2026-11-13: an option in this guide, on in the TypeScript
     project as a tuned change, not the converted-project default. Spellings checked against the Microsoft page
     linked above (Workstation vs. server; Dynamic adaptation to application sizes). -->

**A converted program counts calls from the start.** .NET waits until about 100 ms pass with no new method compiled
before it counts calls toward optimizing a method. Until then, a short busy phase runs the standard library's small
functions unoptimized. A converted program sets that delay to 0. This applies wherever code is JIT-compiled: under
`dotnet run` and `dotnet build`, and in a ReadyToRun publish, whose methods are still recompiled as they get hot. It
does not apply to Native AOT. On `win-x64` (an AMD Ryzen 5 PRO 6650U):

- Under the JIT, a TLS handshake benchmark that loads over a hundred packages ran about a third faster (about 4.2 s
  down to 2.8 s). A trivial program took about 24 ms, roughly 5%, longer to start.
- Published with ReadyToRun, the same benchmark ran about 30% faster, and the trivial program took about 30 ms longer
  to start.

**It trades start time for steady-state speed.** Hot methods reach optimized code sooner, and the program pays for it
in a slightly longer start, most visibly under ReadyToRun. When start time matters more to your program, restore
.NET's own delay in the converted project file:

```xml
<GoCallCountingDelayMs>runtime</GoCallCountingDelayMs>
```

or for one build or publish, `dotnet publish -c Release -p:GoCallCountingDelayMs=runtime`. Any other value is a delay
in milliseconds. The setting applies to an executable project; the environment variable overrides it for one run. A
C# application that references converted packages keeps .NET's default unless it sets the `runtimeconfig.json` value
itself.

<!-- The property and its values: src/go2cs/csproj-template.xml, the GoCallCountingDelayMs PropertyGroup line (empty ->
     0) and the RuntimeHostConfigurationOption ItemGroup, conditioned on OutputType != Library and the value != runtime;
     the template's comment: "Another value is a delay in milliseconds; `runtime` keeps .NET's own default." -->


<!-- Source: docs/phase4/DESIGN-jit-inlining-tiering-publish.md section 9 (seat #53 P2, G-LAPTOP, .NET 10.0.11, 15
     interleaved rounds, A/A band per program and metric). JIT build: PerfTlsHandshake wall 4,161 -> 2,804 ms
     (-32.6%), PerfStartup +24.05 ms (+5.5%); the short hot phase Hash.String 36.97 -> 10.54 ns (0.286). ReadyToRun
     publish (default against GoCallCountingDelayMs=runtime): PerfTlsHandshake 3,287 -> 2,309 ms, PerfStartup
     312.6 -> 342.3 ms. Override order measured: the env var beats runtimeconfig.json in both directions. -->

**Before and after, across the performance suite.** On `win-x64` (an AMD Ryzen 5 PRO 6650U), the
[performance suite's](Performance.md) programs ran with the template's delay of 0 and with .NET's own 100 ms. These
programs run for about 0.1 to 2.5 s, so the delay decides how much of each run executes before the optimized code
arrives. In a long-running service the delay is only a start-up transient, so the table overstates what the setting
costs once a service is warm. With the delay at 100, 13 of the 17 programs are slower under the JIT and 11 under
ReadyToRun; Startup is faster under both. Medians in ms, in-program time except Startup's process wall time; the
ratio is delay 100 against delay 0:

| Benchmark | JIT, delay 0 | JIT, delay 100 | ratio | ReadyToRun, delay 0 | ReadyToRun, delay 100 | ratio |
|:--|--:|--:|--:|--:|--:|--:|
| Startup | 502.0 | 483.5 | 0.963 | 371.5 | 345.5 | 0.930 |
| Fib | 97.6 | 213.4 | 2.186 | 101.4 | 213.2 | 2.102 |
| String | 339.0 | 734.3 | 2.166 | | | within noise |
| TlsHandshake | 1071.6 | 2053.3 | 1.916 | 1078.2 | 1688.7 | 1.566 |
| Map | 253.6 | 470.9 | 1.857 | 248.7 | 466.1 | 1.874 |
| Sort | 255.1 | 434.8 | 1.704 | 255.0 | 437.6 | 1.716 |
| Channel | 80.7 | 126.1 | 1.563 | | | within noise |
| Iface | 374.7 | 558.8 | 1.491 | 380.9 | 616.7 | 1.619 |
| RefLower | 511.3 | 752.4 | 1.472 | 536.8 | 761.2 | 1.418 |
| IfaceShellChurn | 1076.8 | 1574.1 | 1.462 | 1090.9 | 1269.0 | 1.163 |
| StringMatch | 791.2 | 1125.8 | 1.423 | 838.8 | 1096.0 | 1.307 |
| IfaceCall | 358.3 | 435.8 | 1.216 | 349.0 | 506.5 | 1.451 |
| IfaceShell | 600.4 | 723.0 | 1.204 | 575.0 | 770.4 | 1.340 |
| FieldView | 1723.7 | 1959.8 | 1.137 | 1936.5 | 2089.3 | 1.079 |

MatMul, Sieve and StringView stay within measurement noise on both. Native AOT has no tiers, so the delay does not
apply to it.

<!-- Source: docs/phase4/DATA-hop126-perf.md section 10, the tuning sweep (G-LAPTOP, win-x64, AMD Ryzen 5 PRO 6650U,
     SDK 10.0.400 / runtime 10.0.11; 11 interleaved rounds per program, every arm twice as its own A/A; a cell moves
     only beyond the larger of its two arms' A/A gaps and IQRs). The tables "Delay 100 under JIT (JD/JP)" and "Delay
     100 under ReadyToRun (RD/RP)": the moved rows only, numbers as the record gives them, each the mean of its arm's two A/A medians (variant = delay 100,
     baseline = the template's 0). The D arms are copies of the plain JIT and R2R folders with only runtimeconfig.json's
     CallCountingDelayMs changed from 0 to 100. The headline and the start-up-transient framing: the same section
     ("Fib 213.4 against 213.0, String 734.3 against 735.1, Map 470.9 against 473.3, Sort 434.8 against 450.0"). Raw:
     docs/phase4/DATA-hop126-perf/sweep/sweep-verdict.tsv (rows JD/JP and RD/RP), samples sww-arms-1.tsv and
     sww-arms-2.tsv. -->


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
C#'s UTF-16 to Go's UTF-8 once and caches the result, so each later evaluation is a lookup that allocates nothing. A
string constant is one field, as Go declares it.

`-hoist-literals` is the optimization option. It gives each literal whose words make a meaningful name a named field,
`F(helloˢ)`, and it reads every such field, and every string or other struct-typed constant, through a small accessor
under the same name; every other literal (`"%v"`, `", "`) keeps the plain cached form, so no literal is copied on each
evaluation. The JIT and Native AOT both inline that accessor into the code that reads it; they would not inline a
function that reads the field directly. The standard library is converted this way already, so code that calls into it
gets the benefit without the flag. A constant block shows the difference:

```go
const (
	Version = "1.0"
	Name    = "server"
)
```

<!-- illustration: the shape src/go2cs/staticReadAccessor_test.go asserts for a package-level string const -->
```csharp
// without the flag
public static readonly @string Version = "1.0"u8;
public static readonly @string Name = "server"u8;

// with -hoist-literals
public static ref readonly @string Version => ref Versionᵇ;
private static readonly @string Versionᵇ = "1.0"u8;
public static ref readonly @string Name => ref Nameᵇ;
private static readonly @string Nameᵇ = "server"u8;
```

Every use still reads `Version` and `Name`, so the Go function bodies are unchanged.

**When it helps:** a small function that returns or passes a literal or a constant, called from a tight loop.

- On `win-x64` (an Intel Core i9-13900K), the standard library's `FormatBool`, which returns one of two literals,
  took 0.66 ns per call through the accessor under the JIT, against 1.41 ns when it read the field directly and
  1.07 ns with a plain literal. Under Native AOT it took 0.75 to 1.02 ns through the accessor, against 1.13 to
  1.29 ns reading the field.
- On the same machine, a loop calling a small standard-library function that returns a literal
  took roughly twice as long with plain literals under the JIT, once the JIT had optimized the function, and roughly
  1.7 to 1.8 times as long under Native AOT. A smaller such function took up to a quarter longer under Native AOT.
- On `win-x64` (an AMD Ryzen 5 PRO 6650U):
  - a literal returned from a small function called in a hot loop took roughly 1.7 to 1.8 times as long plain,
    under both the JIT and Native AOT;
  - a literal argument inside the loop took roughly 1.6 times as long under the JIT, and a sixth longer under Native
    AOT;
  - a local assigned from a literal took between a tenth and a seventh longer.
- In whole programs, a literal-heavy string-matching benchmark runs faster hoisted under Native AOT on both machines
  measured on `win-x64`: about 9% faster on an AMD Ryzen 5 PRO 6650U and about 5% on an Intel Core i9-13900K. On
  the Ryzen it runs about a quarter faster hoisted under the JIT. Parsing URL queries took about the same time
  either way.

The second and third bullets, and the URL-query reading, were measured against the direct field read; the accessor
reads faster than that field.

**Before and after, across the performance suite.** Of the [performance suite's](Performance.md) 17 programs, only
StringMatch moves beyond measurement noise with `-hoist-literals`. The other 16 stay within noise under both the JIT
and ReadyToRun, including TlsHandshake and StringView, which also get hoisted fields. Native AOT was timed for
StringMatch only. Medians in ms, in-program time; the ratio is hoisted against plain:

| Machine (`win-x64`) | Deployment | plain | `-hoist-literals` | ratio |
|:--|:--|--:|--:|--:|
| AMD Ryzen 5 PRO 6650U | JIT | 791.2 | 594.7 | 0.752 |
| AMD Ryzen 5 PRO 6650U | ReadyToRun | 838.8 | 566.8 | 0.676 |
| AMD Ryzen 5 PRO 6650U | Native AOT | 1288.5 | 1171.8 | 0.909 |
| Intel i9-13900K | Native AOT | 582.1 | 550.0 | 0.945 |

<!-- Source: the JIT and ReadyToRun rows: docs/phase4/DATA-hop126-perf.md section 10, the tuning sweep (G-LAPTOP,
     win-x64, AMD Ryzen 5 PRO 6650U, SDK 10.0.400 / runtime 10.0.11; all 17 programs converted twice by the 1.26.10.2
     converter 7871e564, plain and -hoist-literals, against go.* 1.26.10.2; 11 interleaved rounds, every arm twice as
     its own A/A; a cell moves only beyond the larger of its two arms' A/A gaps and IQRs). The tables "Hoist under JIT
     (JH/JP)" and "Hoist under ReadyToRun (RH/RP)": StringMatch moved (floors 10.5% and 9.5%), every other row within.
     Hoisting creates fields in StringMatch (7), TlsHandshake (3) and StringView (1) only. Raw:
     docs/phase4/DATA-hop126-perf/sweep/sweep-verdict.tsv (rows JH/JP and RH/RP), samples sww-arms-1.tsv and
     sww-arms-2.tsv. The Native AOT rows: section 9.4, the cross-box pair (the same four PerfStringMatch images on both
     boxes, PROTOCOL v3, arm = mean of its A and AA medians): P and H on G (v2 run) 1288.5 / 1171.8, H/P 0.909, floor
     1.2%; on the i9 (run3) 582.1 / 550.0, H/P 0.945, floor 1.8%. Raw: docs/phase4/DATA-hop126-perf/xbox/. -->

**When it does not:** a literal evaluated rarely costs one lookup either way. On `win-x64` (an AMD Ryzen 5 PRO
6650U), a literal passed as an `any` argument, `fmt.Println("x")`, measured the same both ways: both forms pass one
pre-boxed value. Measure your own hot path.

**What it costs:** one more declaration line per literal and per constant. The literal's field is declared above the
function that first uses it, and each constant becomes an accessor plus a private field. Go function bodies are
unchanged. [Plain literals](ConversionStrategies-Reference/strings.md#a-literal-in-a-string-slot-is-a-plain-c-literal)
describes both forms.

<!-- The accessor: docs/phase4/DESIGN-jit-inlining-tiering-publish.md section 1 and its s.2.1 AOT row (seat #53).
     i9-13900K, win-x64, tree aaec1d4ca8, framework-dependent: FormatBool HR2 (the accessor) 0.66 ns, plain 1.07,
     hoisted ldsfld 1.41, Go 0.73-1.02 on the JIT; Native AOT HR2 0.75 / 1.02 against H 1.13 / 1.29 (two passes), HR2/H
     0.66 / 0.79, below the +-9% A/A band. The ldsfld refusal: "ldsfld of value class", FailAlways.
     Sources: docs/phase4/DESIGN-literal-hoisting-survival.md section 9 (seat #52, 2026-10-30).
     i9-13900K, win-x64, .NET 10.0.14, 15 interleaved rounds x 2 passes, A/A band. Test 3, a user loop of 10M calls
     (master aaec1d4ca8): Hash.String plain/hoisted 2.029 and 2.023 on the steady JIT, 1.686 and 1.782 on AOT;
     FormatBool 1.121 and 1.250 on AOT, and 0.86 on the steady JIT (plain inlined, hoisted not; cause unexplained).
     The JIT axis: "steady JIT" = DOTNET_TC_CallCountingDelayMs=0. Under default tiering both arms stayed at Tier0
     (about 28 ns) and read equal; the i9 is investigating why the callees never left Tier0.
     Test 1, PerfStringMatch (893d48976f): plain/hybrid 1.13 on the JIT, 1.004 to 1.007 on AOT. Test 2, net/url
     ParseQuery: flat on the JIT; AOT plain/hybrid 1.02, which the i9 reads as layout.
     Ryzen 5 PRO 6650U (G-LAPTOP), win-x64, .NET 10.0.11, LiteralSiteBench, 15 interleaved rounds, per-round medians:
     return C2 1.707 (JIT) and 1.772 (AOT); argument C1 1.637 and 1.169; local C5 1.148 and 1.112; any slot C6 1.008
     and 1.003, inside the A/A band. Data: docs/phase4/data/seat52-literal-classes.
     2026-11-05, the whole-program bullet re-measured at the 1.26.10.2 shipping commit (converter 7871e564), the SAME
     four PerfStringMatch Native AOT images timed on both boxes under one protocol (G's cross-box PROTOCOL v3, A/A per
     arm, 21 rounds; the i9's runs 1-2 recorded invalid, run 3 valid): hoisted/plain 0.909 (G-LAPTOP, floor 1.2%) and
     0.945 (i9, floor 1.8%). On G alone, the same pair under the default JIT read 0.752 and under TC0 0.794
     (docs/phase4/DATA-hop126-perf.md section 8.4.4; the cross-box pair is section 9.4). The 10-30 Test 1 reading (AOT 1.004) was its
     own images (893d48976f) and does not reproduce on the i9 with these. The owner kept plain as the default for user
     programs (2026-11-05, "Keep plain"); -hoist-literals stays the opt-in. -->

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

### Options that do not change run-time cost

| Option | Why it is not a tuning option |
|:--|:--|
| `-var`, `-uco`, `-indent`, `-comments`, `-provenance`, `-license`, `-tree` | Change how the emitted C# reads or what metadata it carries; the program does the same work. |
| `-goroot`, `-gopath`, `-go2cspath`, `-stdlib`, `-recurse`, `-csproj`, `-tags`, `-platforms`, `-cgo` | Choose which Go code is converted and for which platforms, not how it runs. |
| `-debug`, `-convert-timeout`, `-allow-stale-converter`, `-platform-census`, `-platform-stage`, `-ref-census`, `-version`, `-nuget-map`, `-nuget-map-*`, `-module-*` | Govern the conversion itself, or run an analysis that emits no C#. |
| `-dual-recv`, `-dual-recv-params` | Experimental, for converter development, and off by default: eligible pointer-receiver methods also emit a form that takes the receiver by reference. Their output is not validated, and their run-time cost is not measured. |
| `-tests`, `-test-*` | Converted test hosts only; `-test-tiered` is covered [above](#net-runtime-settings). |
| `GOGC`, `GOMEMLIMIT` | Read and reported, but .NET's collector does not follow them ([Limitations](Limitations.md)). |
| `GO2CS_GC_PAUSE_HISTORY=0` | Changes what `runtime` reports about garbage collection, not how the program runs. |
| golib's `FieldViewCache.Disabled` | A switch for one test, which checks that `&s.f` stays one pointer as it does in Go. Setting it breaks that. |
