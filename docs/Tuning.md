# Tuning converted code

Converted Go runs on .NET, and .NET gives you choices: how the program is compiled and deployed, how the runtime
compiles it while it runs, and a few converter options that change what it costs. This page says which choices matter,
what each one trades, and which to leave alone. Every claim names the platform it was measured on. For the numbers,
see the [performance comparison](Performance.md), which is re-measured as the converter changes.

**In short:** keep the .NET defaults. Publish with ReadyToRun or Native AOT when start time matters, and prefer the JIT over
Native AOT when tight loops dominate. Do not turn tiered compilation or dynamic PGO off for speed: on `win-x64`,
the default runtime ran a pointer-heavy benchmark roughly 40% faster than with either one off.

## Choosing a deployment

A converted program can run three ways. All three run the same converted code; they differ in when it is compiled.

| Deployment | How you get it | Compiled |
|:--|:--|:--|
| **JIT** | `dotnet run`, `dotnet build`, or a framework-dependent `dotnet publish` | as each method first runs, then recompiled hotter as it keeps running |
| **ReadyToRun** | `dotnet publish -c Release` of a converted program, which turns it on | ahead of time, then the JIT can still recompile hot methods |
| **Native AOT** | `dotnet publish -r <rid> -p:PublishAot=true` | entirely ahead of time, to a native executable with no JIT |

Which wins depends on what the program spends its time on. The `win-x64` comparisons below come from the
performance comparison's table, measured on an earlier converter release; that page dates it.

- **Start time.** On `win-x64`, a Native AOT program starts several times faster than the same program under the JIT,
  though still slower than Go. On `linux-x64`, ReadyToRun cut a converted program's median start time by about a
  third against the JIT.
- **Tight loops.** On `win-x64`, the JIT ran slice-indexing, floating-point and pointer-heavy loops faster than
  Native AOT, by a wide margin on the pointer-heavy one. Native AOT has no dynamic profile-guided optimization or
  on-stack replacement to tune a hot loop as it runs.
- **Maps.** On `win-x64`, converted map code ran faster than Go under both the JIT and Native AOT, and fastest under
  Native AOT.
- **Interfaces.** On `win-x64`, calling interface methods and asserting to concrete types cost about the same under
  the JIT and Native AOT. The one interface path with no compile-time answer, asserting a value to an interface it
  satisfies only structurally, ran slower under Native AOT than under the JIT.
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
     329 ms ReadyToRun, -33%). The deployment table's commands: the converter's Exe template sets
     <PublishReadyToRun>true</PublishReadyToRun> (src/go2cs/csproj-template.xml), applied only by dotnet publish. -->

If you publish ReadyToRun as a single file, also set
`<PublishReadyToRunComposite>false</PublishReadyToRunComposite>`
([Known issues](KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory)).

## .NET runtime settings

Under the JIT and ReadyToRun, .NET compiles code in tiers. A method first gets a quick compile; one that keeps running
is recompiled with full optimization, guided by a profile the runtime collected while it ran (dynamic PGO). Native
AOT has no tiers and no dynamic PGO.

**Leave tiered compilation and dynamic PGO on.** On `win-x64`, a converted benchmark that takes field addresses in a
hot loop ran roughly 40% faster with the defaults than with tiered compilation off, and roughly as much faster than
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
