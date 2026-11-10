# Known issues

This page lists specific bugs, each with a workaround. For the limits that come with running Go on .NET, and
the rough spots real code hits today, see [Limitations](Limitations.md).

## Writes through the address of another package's pointer or interface variable are lost

Taking the address of a variable that another package exports gives a pointer to the variable itself when its
type is a value type. When its type is a pointer or an interface (`&otherpkg.ErrMissing`, for an
`ErrMissing error`), the pointer refers to a copy, and so does the address of `runtime.MemProfileRate` and of a
variable declared with a two-argument `//go:linkname`.
[Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go) describes the cases and the
workarounds.

## On Apple silicon, the standard library is the `amd64` build {#on-apple-silicon-runtimegoarch-reports-amd64}

The packages ship for four platforms: `win-x64`, `linux-x64`, `osx-x64` and `osx-arm64`. The converted standard library
for macOS is built once, for `darwin/amd64`, and the same packages serve Intel and Apple-silicon Macs. On an
Apple-silicon Mac a converted program runs as a native arm64 .NET process:
- `runtime.GOARCH` reads `arm64`, as Go's own program on the same Mac does. It is read from the process when the
  program starts, and the `GOARCH` setting that `debug.ReadBuildInfo` returns and `go/build`'s default context
  follow it;
- everything Go decides by architecture at build time still follows `amd64`. Go's `_arm64.go` files are not in the
  packages, `internal/goarch` reports `amd64`, and a constant Go computes from the architecture keeps its `amd64`
  value.

So the converted runtime reads `runtime.GOARCH` as `arm64` beside `goarch.GOARCH` as `amd64` on the same Mac. That
asymmetry is deliberate: each runtime branch on `GOARCH` was checked and reads correctly on Apple silicon. The
processor features that the standard library reads at run time come from the real processor, so `crypto/tls`, for
one, prefers AES-GCM on Apple silicon as Go does there.

**To avoid it**, rely on `runtime.GOARCH` for the machine's architecture, never on the presence of an
architecture-specific path in the standard library.
<!-- Source: BOARD 2026-11-05 (C1, master reds seat), "runtime.GOARCH reads amd64 on Apple silicon", with its two
     2026-11-06 amendments. Measured on the GitHub-hosted osx-arm64 runner: master's darwin osx-arm64 behavioral red
     (run 41964964302, BuildInfoSettings, stdout mismatch C# vs Go); osx-x64, linux and windows agree because their host
     is amd64. Constants, read at master 72043c1a66 on 2026-11-08, the same in all three flavours:
     src/core/runtime/linux/extern.cs:375 (`GOARCHᵇ = "amd64";`) and, until the run-time GOARCH seat of 2026-11-09,
     windows' and darwin's extern.cs at the same line;
     src/core/internal/goarch/zgoarch_amd64.cs (GOARCH "amd64", IsAmd64 1, IsArm64 0) is the only zgoarch file; and no
     *_arm64.cs file exists anywhere under src/core. debug.ReadBuildInfo's GOARCH setting is composed from runtime.GOARCH
     (src/core/runtime/debug/stubs_impl.cs:199 (`GoModInfoAttribute.Compose(…runtime_package.GOARCH.ToString()`)). The run-time feature read is batch 216
     (C1, claude/c1-aes-arm64): internal/cpu's ARM64 flags are filled on an arm64 process, so crypto/tls's
     hasGCMAsmARM64 holds. Open and owned after 1.26.10.3: the BOARD row's fix shapes (a run-time GOARCH for the shared
     darwin flavour, with its census of compile-time GOARCH branches, or a per-architecture darwin build). -->
<!-- Amended 2026-11-09 (G, the run-time GOARCH seat, claude/g-goarch-runtime): the first fix shape landed, for darwin
     only. The heading was "On Apple silicon, `runtime.GOARCH` reports `amd64`"; its old id is kept explicitly, because
     NEWS.md (1.26.10.3) links it. runtime.GOARCH and internal/buildcfg's defaultGOARCH are displaced for darwin
     (src/go2cs/manualTypeOperations.go manualConversionConsts) and read the process architecture
     (src/core/runtime/darwin/goarch_darwin_impl.cs, src/core/internal/buildcfg/darwin/zbootstrap_darwin_impl.cs).
     The census of record: docs/phase4/DESIGN-comparator-arms.md §5, layer 2, plus census E (consts folded from
     runtime.GOARCH: defaultGOARCH incoherent, 4 inert or equivalent). The linux and windows flavours still read amd64 on an
     arm64 host (BOARD 2026-11-09), and no package ships for those. -->
<!-- Amended 2026-11-09 (G, the same seat, by P2's CENSUS-goarch-linux-windows.md): the windows flavour does the same
     (src/core/runtime/windows/goarch_windows_impl.cs, src/core/internal/buildcfg/windows/zbootstrap_windows_impl.cs),
     so Windows on Arm reads arm64. That is unmeasured on an arm64 Windows host: the os-matrix has no windows-arm64
     runner, and no package ships for it. linux keeps amd64: its syscall numbers and struct layouts are linux/amd64's,
     so a linux/arm64 host needs a flavour of its own (owner ruling, 2026-11-09: in .4). -->


## The packages do not ship linux-arm64

linux-arm64 is not supported. The go.* packages ship `win-x64`, `linux-x64`, `osx-x64` and `osx-arm64`. A package
whose code differs by platform carries one folder per platform under `runtimes/`, and a platform without one gets
the Windows build in `lib/`. A build resolves its platform the way .NET does at run time, so `linux-musl-x64`
(Alpine) uses the `linux-x64` folder. For a platform that resolves to no folder, the build reports it:
- **`GO2CS3003`, an error**, on another operating system, or on Linux with another architecture: linux-arm64 would
  run the Windows build, and the `linux-x64` build makes x64 system calls. The message names the platform and the
  first package without a folder for it.
- **`GO2CS3004`, a warning**, on a Windows platform the packages do not ship (`win-arm64`): the Windows build is
  that operating system's, so the program runs. What Go decides by architecture when the package was converted
  follows `amd64`.

A cross-build for a shipped platform from an unshipped host is not affected: it names its platform with
`-r <rid>` (`RuntimeIdentifier`), and that platform has its folder.

**To avoid it**, build for one of the four shipped platforms. A build that only needs to compile, and knowingly wants
the Windows build (a test harness that never runs the program, say), passes `-p:GoAllowReferenceFlavor=true`. The
program then runs the Windows build wherever it runs. The property counts only on the command line: set in a
project file it changes nothing, so a library cannot switch the check off for the projects that build it.

A portable app built on a shipped platform and then RUN on another one is out of the build's sight. At start-up
it prints a warning on standard error instead: for another operating system, and for the Linux build on another
architecture (`linux-x64` run on `linux-arm64`).
<!-- Source: DESIGN-linux-arm64-flavour.md section 0 (lane G, claude/g-la64-design 7831a23193), SEAT 0, cut by C2 as
     claude/c2-la64-seat0 on master 0c553534df, with REVIEW-linux-arm64-flavour.md F4 folded in (COORD ruling R2,
     2026-11-09: its own target, RID-split by runtimes/ presence, RID-graph aware, win-arm64 a warning, the run-time
     architecture check). Mechanism: src/core/golib/buildTransitive/go.lib.targets, GoRefuseUnshippedRid, over
     _GoResolveRidFlavors (a breadth-first walk of $(RuntimeIdentifierGraphPath); measured: linux-musl-x64 imports
     linux-musl and linux-x64, win-arm64 imports only win, in SDK 10.0.403's PortableRuntimeIdentifierGraph.json).
     Gate: src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1, read red-first against go.* 1.26.10.3 on
     2026-11-09: against master's go.lib.targets the UNSHIPPED, swap-off, project-file opt-out, ALPINE and WIN-ARM64
     arms fail (the linux-arm64 build BUILT against lib/; the Alpine build failed CS0426 against lib/); with the change
     all ten arms pass, and each of the swap-off and project-file arms fails alone against a copy mutated in that one
     respect. The run-time check is GoHostOfRecord.Describe (src/core/golib/GoHostOfRecord.cs), called through
     Verify(GOOS, GOARCH) from src/core/runtime/hostofrecord_impl.cs, gated by src/tests/GolibTests/HostOfRecordTests.cs
     (red first: the linux/amd64-on-arm64 case failed before the architecture check). The linux-arm64 flavour itself
     is the design's seat S6, which turns the error into a no-op for that platform. -->

## On macOS, a `go` statement past the system's thread limit ends the program

Each goroutine is an operating-system thread, and macOS caps how many threads one process may run. When a
program already has nearly that many goroutines alive, the next `go` statement throws
`System.OutOfMemoryException`, and the program ends unless something catches it. On the macOS 15 machines measured
the limit was about 2,000 goroutines on Apple silicon and about 3,000 on Intel; a Mac's own limit is the
`kern.num_taskthreads` setting.

**To avoid it**, bound how many goroutines are alive at once, with a worker pool for example.
[Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go) has the details, under
*Goroutines are operating-system threads*.
<!-- Source: C1's run 42497561630 (claude/c1-darwin-threadcap a85e0cf11), on GitHub-hosted macOS runners: osx-arm64
     (macOS 15.8.2, kern.num_taskthreads 2048, first failure at 2,027 live goroutines) and osx-x64 (macOS 15.7.9, 3072,
     first failure at 3,051). The figures and their provenance sit beside the Limitations bullet. -->

## With go2cs 1.26.10.2 or earlier, a program converted with local standard-library references runs its standard library built Debug

**Symptom.** A program converted by go2cs 1.26.10.2 or earlier with plain `-recurse` or `-recurse=module`
references the standard library as local projects. Built with `dotnet build -c Release` in its folder, its
standard-library assemblies come out built Debug, unoptimized, and it runs slower than the same program built from
its `.csproj` (about 21 times slower on a TLS handshake under the JIT, on `win-x64`, on one development machine).
`dotnet publish -c Release -r <rid>` in the folder can fail with
`ILLink : error IL1034: Root assembly 'golib' ... does not have entry point`, and when it succeeds the published
program carries the same Debug assemblies. A `-recurse=nuget` conversion is not affected.

**Cause.** The converter wrote a `.slnx` beside each project, and in a folder that holds both, `dotnet build` and
`dotnet publish` take the solution. A solution build gives its configuration only to the projects the solution
lists, which were the program, `golib` and the analyzer, so every other standard-library project it built took its
default configuration, Debug. Publishing the solution also published `golib` as a project of its own.

**Fix.** Re-convert with go2cs 1.26.10.3 or later. It writes no solution for a local-reference conversion, and it
removes the one an earlier converter wrote, if that file is unchanged.

**Workaround.** Build or publish the **project**, not a solution: run the command in a folder that holds no `.sln`
or `.slnx`, or name the `.csproj` (`dotnet build <app>.csproj -c Release`). Do not build these conversions through
a solution: the core projects a solution does not list build Debug. In Visual Studio, open the `.csproj`.

<!-- Found 2026-11-06 by G and root-caused there: read from DebuggableAttribute, a converted TLS closure was 160 of
     163 assemblies Debug and a Sieve closure 64 of 67, and building the .csproj instead gave 0 Debug and the TLS
     program's release time, 1,070 ms against about 23 s. G also traced the IL1034 publish failure to the same
     solution. Measured on H at master df7c09ab7b, with a fmt + strings program (66 core projects) converted with
     plain -recurse. Today's output: a bare `dotnet build -c Release` gave 45 of 46 standard-library assemblies Debug
     (only golib, which the solution listed, was Release), and a bare `dotnet publish -c Release -r win-x64` failed
     IL1034. With the publish-guarded golib.csproj (batch 223) the publish succeeded but shipped the same 45 Debug
     assemblies. Three remedies were measured. (a) ShouldUnsetParentConfigurationAndPlatform=false in the generated
     Directory.Build.props: 0 Debug in the output, but 63 core projects were also built a second time in Debug
     (+70% wall). (b) The solution listing the whole core closure: 0 Debug, but the solution publish published all 66
     core libraries in their own right, and two builds of one library raced on its deps.json (MSB4018). (c) No
     solution: 0 Debug and no Debug build, and the bare publish succeeded even without the golib guard, its output
     equal to go run. (c) was ruled, scoped to local references. Under -recurse=nuget the i9 measured, as relayed by
     the coordinator, a bare build through the generated solution: 0 Debug, and the bare publish succeeded. -->

## Ubuntu's packaged .NET 10 can end a converted program early

On Ubuntu's own build of .NET 10 (the `dotnet` installed with `apt`, runtime identifier `ubuntu.*-x64`), a
converted program that raises and recovers Go panics inside a hot loop can stop in one of two ways under the
default tiered compilation:
- a fatal `Internal CLR error. (0x80131506)`;
- more often, a SILENT exit with code 0 and truncated output.

The fault needs the JIT's on-stack replacement. Microsoft's build of the same .NET version does not show it, and
neither does Windows. No converted Go program is known to reproduce it; only a synthetic reproduction does.

**To avoid it**, run on Microsoft's .NET build, or turn off quick JIT for loops, which removes on-stack
replacement. A build property does it without touching the converted project:

```shell
dotnet build -p:TieredCompilationQuickJitForLoops=false
```

The built program's `runtimeconfig.json` then carries
`"System.Runtime.TieredCompilation.QuickJitForLoops": false`. Or set it in the converted executable's project
file, though the next conversion rewrites that file:

```xml
<TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops>
```

Converted executable projects carry this line, commented out. On `linux-x64` (Canonical's .NET 10, one 4-core
machine) it costs about 90 ms (16%) more startup time, and steady-state loop speed stays within measurement noise. It becomes the default if a converted Go program
ever reproduces the fault. [Tuning converted code](Tuning.md#net-runtime-settings) lists the runtime settings and
what each trades.
<!-- Source of the 90 ms (16%) figure, found 2026-10-26: the fleet mailbox post C2 -> COORD of 2026-09-30T10:57Z
     (commit 80f928db5a on origin/claude/mailbox, file docs/phase4/inbox/COORD/20260930T105729Z-C2.md, not on master).
     Measured on a 4-core Linux machine on Canonical's .NET 10.0.12, PerfStartup and PerfSieve built Release from master
     f819887fa3, default vs runtimeconfig System.Runtime.TieredCompilation.QuickJitForLoops=false (the arms differ ONLY
     in runtimeconfig.json), interleaved, one discarded warm-up, every run rc 0. PerfStartup wall, n=40 each: default
     median 561 ms (p10 524, p90 617), opt-in median 653 ms (p10 592, p90 686): +92 ms median, about +16%. PerfSieve
     in-program workload, n=20 each: 238 ms vs 249 ms median, +11 ms (+4.7%), inside the spread (one benchmark, one
     machine). The same post measured the opt-in at 0/96 BAD against 64/96 BAD for the default on the OSR repro
     (claude/c2-osr-repro); the owner ruled it opt-in, not default, on 2026-09-30. -->

## With the go.* 1.26.10.1 packages, a Native AOT program that uses the network can exit with an error

**Symptom.** A Native AOT executable (`PublishAot`) that references the `go.*` 1.26.10.1 packages exits with code 2
the first time it passes a pointer to certain Go structs to a system call. It prints
`System.NotSupportedException: 'System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences[System.UInt16]()' is missing native code`
(the type in brackets varies). On Windows, a program that uses `net` or `crypto/tls` reaches it while setting up a
socket, in `syscall.WSAIoctl`. On Linux and macOS, the same system-call marshalling runs for a pointer to a struct that
holds a pointer, slice, string or array, and fails the same way. The same program run under the JIT (`dotnet run`, or
the converted project's default ReadyToRun publish) is not affected. Measured on `win-x64`, `linux-x64` and `osx-arm64`.

**Cause.** The runtime library's system-call marshalling sorted a struct's fields with a generic method it closed at run
time, and a Native AOT image has no code for such a method.

**Fix.** Move to the `go.*` 1.26.10.2 packages or later.

**Workaround on 1.26.10.1.** Publish without Native AOT: the converted project's default `dotnet publish` (ReadyToRun) and a
framework-dependent build run the same code under the JIT.

<!-- Found 2026-10-27 by the performance re-baseline's AOT window (PLAN-perf-rebaseline, window 2): both 1.26.10.1
     PerfTlsHandshake images published on the i9 (the template default and the folding-off build, ILC 10.0.14) exited 2
     on G-LAPTOP with that exception, the stack running syscall.WSAIoctl -> ж`1.op_Implicit (to uintptr) ->
     FieldRefBox`1.StorageKind -> CrossesAsItsRoot -> NativeStructMarshal.WidestAtSameAddress -> HasDeepImage ->
     deepRefusal -> Leaf.containsReferences. golib's NativeStructMarshal closed IsReferenceOrContainsReferences<T>,
     Unsafe.SizeOf<T>, its leaf copiers and NativeBox<T> over run-time Types with MakeGenericMethod, under file-wide
     IL3050 pragmas, and golib built without the AOT analyzer. Fixed in 1.26.10.2 seat #18, whose GolibAotProbe
     scenarios (src/tests/PackageTests) read rc 2 at the base and rc 0 at the fix under Native AOT on win-x64 (G-LAPTOP),
     and on linux-x64 (ubuntu) and osx-arm64 (macos-15) hosted runners, run 40611703805, each equal to the JIT run. -->

## A Native AOT publish of a program that references the packages is slow and large

<!-- Measured 2026-10-06 against go.* 1.24.13.4 from nuget.org by C1 (mailbox bdfb01d50d, d543355364,
     3ae3f16305): a C# consumer of go.lib, go.runtime and go.sort, 31 go.* assemblies in its closure, linux-x64.
     With no trim mode of its own and PublishAot: exit 2 at startup, TypeInitializationException over an
     InvalidOperationException from internal/cpu's initialiser ("... occupies 32 bytes and reports no instance
     fields -- its field metadata was removed, most likely by trimming"). The same with TrimMode=full. With the
     trim mode set to partial in its own project file: the publish ended after 2 h 24 min on a 4-core, 15 GB
     machine (about 70 min inside a hosted CI leg, run 37495989079), the executable ran (exit 0) and was 249 MB.
     WINDOWS (C1, mailbox 079b80b37b; hosted aot-smoke run 37523439849, win-x64): with go.lib's packed default
     of partial the publish succeeded (71 min) and the executable exited 2 at startup with
     "System.InvalidOperationException: There is no metadata token available for the given member"; linux-x64 in
     the same run passed (56 min). golib reads FieldInfo.MetadataToken at four sites to order a struct's fields
     (the two header/slice boxes and GoLibcCall); which one windows reaches is not yet read from a stack.
     A trimmed non-AOT publish (PublishTrimmed, self-contained, no trim mode) ran. A converted project is not
     affected: src/go2cs/csproj-template.xml sets the mode. The fix for the failure is the seat
     claude/c1-golib-trim-default (go.lib's packed targets set the template's default when the consumer set
     none); delete the failure half of this section when a release carries it. The compile time stays until the
     runtime library's reflection is annotated for a full trim (the IL trim warning rows on the BOARD).
     2026-10-07 (COORD): WINDOWS read from a stack by the i9 (ledger 2026-10-06 20:40): the failing site is the
     converted runtime's internCallerFrame, which keyed a call site by its method's metadata token, reached at the
     first runtime.Caller; a converted program that never asks for its caller ran under Native AOT on windows with
     output identical to Go. The fix is the i9's seat claude/i9-aot-metadata-token for the train after TRAIN Q.
     macOS: C1's hosted aot-smoke, the same consumer with the partial trim mode, osx-x64 PASS three times (runs
     37523439849, 37559598601, 37564030683; publish 256 to 293 min); osx-arm64 cancelled twice by the six-hour
     job limit inside the publish (37523439849, 37559598601).
     2026-10-09 (R, for the 1.24.13.5 release): the FAILURE half of this entry is deleted. Both fixes are in
     master 56f0f1f254: the trim default is ab6aa8f443 (go.lib's packed targets default TrimMode=partial; TRAIN Q,
     merge be7b209245), and the Windows failure is a0cafdb588 (no metadata token is read on any path a Native AOT
     program takes; TRAIN FL row 11, merge 3db77316b1). Hosted aot-smoke 37870614219 at 56f0f1f254: linux-x64,
     win-x64 and osx-x64 pass, osx-arm64 not measured. The full-trim reading is G's, from its logs: win-x64,
     Windows 11, .NET SDK 10.0.400, one development machine, exit 0 with 0 refused types (ledger 2026-10-08
     19:36). The section "Publishing a converted program twice loses file and line numbers in stack traces" was
     deleted in the same commit: its fix is a395442f7b (dependency symbol files stay beside a single-file host
     on every publish), in master through the merge 4d9046eb70. -->

A program that references the `go.*` packages and publishes with Native AOT (`PublishAot`) runs. The check
that gates each release publishes a C# consumer of the packages and runs it on `linux-x64`, `win-x64` and
`osx-arm64`. On `osx-x64` the publish is read when it finishes inside the hosted runner's time limit, and reported as
not measured when it does not. A project that sets no trim mode of its own gets the packages' default, `partial`,
which is the setting a converted project carries in its generated project file.

A full trim (`TrimMode` set to `full`) produces a program that runs as well. That is measured on `win-x64`, on
one development machine, with the 1.24.13.5 packages; the hosted check measures the default.

**What it costs.** With `partial`, Native AOT compiles every `go.*` assembly the program references in
full, not only the parts the program uses. For a small program that references about thirty of them, the
publish took under four minutes on a hosted CI runner for Linux and for Windows, about six minutes on a Mac
with Apple silicon and about eleven on an Intel Mac (measured 2026-10-14, with packages converted from Go
1.24.13). A converted project pays the same cost, because it uses the same setting. A build, `dotnet run` and a publish without
Native AOT are not affected. For the 1.24.13.7 packages and earlier, see the section that follows.

<!-- 2026-10-26: re-read at a6214f0c5d. The visible costs are the hosted aot-smoke readings after golib's generic-cycle
     fix (e2move, merge b1ba1380b4, 2026-10-14), from the BOARD's 2026-10-14 entry under "the AOT-cost row" and
     docs/CIMatrix.md's aot-smoke budget row: 0 ILC cycle cut-offs on every RID; linux 224 s, windows 230 s, osx-x64 654 s
     on the e2move tip (run 38575258530); linux 212 s, windows 167 s, osx-arm64 354 s from master (run 38587876788).
     Before it: linux 56 min, windows 71 min, osx-x64 293 min, osx-arm64 cancelled at the 6 h job limit (37523439849),
     and about 70 min on hosted linux (37495989079). The executable size (243,136,000 bytes on one machine, below) has
     not been re-measured since the fix. These readings were taken at a Go 1.24.13 tree; whether to re-measure at
     1.26.10.1 is an owner question.
     MOVED HERE from the visible text, all measured BEFORE the fix and not re-measured since: "more than two hours on a
     four-core machine" for the same consumer (the 2 h 24 min reading in the comment above); a program that uses fmt and
     reflect ("genprobe", the comment below) published in about five and a half hours, with ILC at about 9.5 GB, to an
     executable of about 370 MB; and the performance suite's programs peaked at 15 to 18 GB while they published
     (docs/Performance.md), re-measured at 22.2 GB working set on 2026-10-13 (docs/phase4/DATA-hopN-perf.md). -->

<!-- Measured 2026-10-10 by G (ledger: COORD's stage-3 table, row A): "genprobe", a short fmt + reflect program
     (generic struct printing, reflect.TypeOf / Field / DeepEqual), converted, 58 Go packages in its closure,
     published with Native AOT and TrimMode=partial against master 56f0f1f254 (the release tree), .NET SDK
     10.0.400, win-x64, Windows 11, AMD 6 cores / 12 threads, 31 GB. Run ALONE: rc 0, wall 19,579 s (5 h 26 min),
     executable 386,929,664 bytes, output identical to Go's. ILC worked on one core after its first hour, private
     memory 8.7 to 9.4 GB; light concurrent work in its first two hours makes the wall an upper bound by minutes, not
     hours. Two such publishes run side by side on the same machine (ILC 8.7 and 10.3 GB, 1.3 GB left free) were
     both still compiling at the four-hour ceiling. The ~250 MB consumer above measured 243,136,000 bytes on the same
     machine and tree. -->

## With the go.* 1.24.13.7 packages or earlier, a Native AOT publish can take hours or not finish

**Symptom.** A Native AOT publish (`PublishAot`) of a program that references the `go.*` 1.24.13.7 packages, or an
earlier release, does not finish or takes hours. The ILC compiler holds about 14 GB of memory and prints tens of
thousands of warnings that a method will always throw because a type from `golib` failed to load. Measured on
`win-x64`, on one development machine.

**Cause.** These releases were built before the fix to the runtime library's generic cycle (see
[the Native AOT section](#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)). Native
AOT expands that cycle until it reaches its nesting limit, and compiles each method that reaches past the limit to
throw, which is what those warnings report.

**Fix.** Move to the `go.*` 1.26.10.1 packages or later. They carry the fix, and the 1.24.13.7 packages' deprecation
notice already points to them.

**Workaround on 1.24.13.7.** Turn off identical-method folding in the project file:

```xml
<IlcFoldIdenticalMethodBodies>false</IlcFoldIdenticalMethodBodies>
```

The publish then finishes for most programs, at a cost: ILC peaked at about 28 GB, and the executable was 3.3 times
the size of the same program built against 1.26.10.1. A very large program can still fail at link (`LNK1183`,
"extended relocation count 9 less than 65535") when the compiler's object file passes 4 GiB, the COFF object
format's limit, and the workaround has nothing more to offer it: move to 1.26.10.1. Measured on `win-x64`: every program
of the performance suite but the TLS handshake published this way; that one failed at link.

<!-- Measured 2026-10-27 on the i9 (win-x64), relayed by COORD; the attribution is G's, settled by the i9's count.
     The 1.24.13.7 leg of the performance re-baseline (PLAN-perf-rebaseline) published PerfChannel and PerfFib with the
     template's default Native AOT recipe: each reached the 4 h cap with ILC plateaued at ~13.9 GB and no image, and
     printed 56,190 "will always throw" lines. With IlcFoldIdenticalMethodBodies=false, PerfChannel published: rc 0 in
     1,622 s, ILC peak 28.15 GB, image 563,323,392 bytes against 170,638,848 for the same program on 1.26.10.1 (3.3x).
     Cause, read at the tree: nuget-1.24.13.7 (57ca7c1616, 2026-10-13 23:53 -0500) does not contain golib's generic-cycle
     fix, e2move (merge b1ba1380b4, 2026-10-14 08:51 -0500); `git merge-base --is-ancestor b1ba1380b4` reads false for
     nuget-1.24.13.6 and nuget-1.24.13.7 and true for nuget-1.26.10.1. The i9's count over the PerfFib default log: all
     56,190 lines are "Failed to load type ..." over golib 1.24.13.7, and 46,626 of them (83.0%; 85.7% with array`1's
     nested types) declare on array`1, slice`1 or ж`1, mostly five-level nests. The log has no separate cut-off line;
     the cut-off shows only as these lines. The release gate's aot-smoke consumer published within its bound before the
     fix (windows 71 min, run 37523439849), so the stall depends on the program; the perf suite's programs exceed it.
     Amended 2026-10-27 (COORD's ruling; the i9's aot-foldoff2 queue, win-x64): with IlcFoldIdenticalMethodBodies=false
     the 1.24.13.7 leg published 16 of 17 programs. PerfTlsHandshake failed at link: LNK1183 "extended relocation
     count 9 less than 65535", because ILC wrote a 6,594,692,833-byte object, past COFF's 4 GiB; ILC peaked at 51 GB
     private and printed 97,161 "will always throw" lines against the 56,190 above. The same program on 1.26.10.1
     writes a 2.67 GB object and links (338.5 MB image). The default recipe stalls on that leg as well, so the
     performance re-baseline's 1.24 TLS AOT cell reads n/a with this reason. -->

## A single-file publish with composite ReadyToRun runs out of memory

A converted program's project turns ReadyToRun on for `dotnet publish`. When the publish is also a single file
(`-p:PublishSingleFile=true`), the .NET SDK compiles ReadyToRun as a single composite image of the program and the
framework by default, and on that image the ReadyToRun compiler, crossgen2, expands golib's nested generic types
until it runs out of memory: past 10 GB on a linux-x64 machine, failing with `Insufficient memory` and leaving
the build hanging. Compiled one assembly at a time, it does not. The publish profiles go2cs writes beside a
converted program (`-p:PublishProfile=linux-x64` and the others) set
`<PublishReadyToRunComposite>false</PublishReadyToRunComposite>`, so a profile publish keeps ReadyToRun and is not
affected. A program converted by a go2cs earlier than 1.26.10.2 has profiles that turn ReadyToRun off instead,
since go2cs never overwrites a profile; replace their `<PublishReadyToRun>false</PublishReadyToRun>` with that
line to get ReadyToRun's faster start back. A project converted by go2cs 1.26.10.3 or later carries the same line,
so a plain single-file publish is not affected either. A project converted by an earlier go2cs does not; on its command line,
turn the composite image off yourself:

```
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishReadyToRunComposite=false
```

A publish that is not a single file never makes a composite image, so it is not affected.

<!-- P1's reconciliation, linux-x64, go1.26.10, .NET SDK 10.0.112, at a8821c781a (claude/p1-trim-recon ddf7c054d7,
     docs/phase4/DATA-trim-recon-p1-2026-10-24.md §5 finding 3): "-p:PublishSingleFile=true with the csproj's ReadyToRun
     on ran crossgen2 to 10 GB RSS and failed with "Insufficient memory". MSBuild then hung, and it was stopped by PID.
     With ReadyToRun off, the single-file publish succeeded." The template's switch:
     src/go2cs/csproj-template.xml:58 `<PublishReadyToRun>true</PublishReadyToRun>`.
     THE PROFILE FIX, 2026-10-25 (P1, claude/p1-singlefile-r2r, COORD's ruling (a)): the nine embedded profiles gain
     `<PublishReadyToRun>false</PublishReadyToRun>`. A condition on the template's line cannot do it: `dotnet msbuild
     -getProperty` with the shipped linux-x64 profile reads PublishProfileImported=true and PublishReadyToRun=true under
     Condition="'$(PublishSingleFile)'!='true'", because the SDK imports a profile after the project body. Arms, linux-x64,
     .NET SDK 10.0.113, a converted fmt+strings program, `dotnet publish -p:PublishProfile=linux-x64 -p:GoTargetOS=linux`
     in an 11 GiB memory cgroup: BEFORE (the hop-tip converter's profile) crossgen2 "System.OutOfMemoryException:
     Insufficient memory to continue the execution of the program", NETSDK1096, cgroup peak 10,973 MiB, no cgroup OOM
     kill; crossgen2 then stayed alive, idle, until it was stopped by PID. AFTER (the fixed converter's profile)
     PublishReadyToRun=false, publish rc 0 in 438 s from a purged tree, cgroup peak 3,422 MiB, one 20,677,627-byte
     executable whose output equals `go run`. The template's own line is deferred: it would move one line in 1,312
     committed csproj.
     PER-ASSEMBLY FIX, 2026-10-27 (P1, seat #14, COORD's ruling after G's crossgen2 S0): only a COMPOSITE image runs
     out of memory, and the SDK turns composite on when PublishSingleFile and PublishReadyToRun are both set
     (Microsoft.NET.CrossGen.targets). The nine profiles swap PublishReadyToRun=false for
     PublishReadyToRunComposite=false. Same program, cgroup and linux flavor as above, .NET SDK 10.0.113. Four arms
     through the 9bb7bb7bf8 converter's profile with -p overrides, one after another on one tree: ReadyToRun off (as
     shipped) publish 451 s from a purged tree, peak 3,018 MiB, 20,699,137 bytes, median run 1.02 s; PER ASSEMBLY
     54 s on the built tree, peak 3,145 MiB, 34,586,464 bytes, median run 0.71 s; COMPOSITE WITH crossgen2's
     generic-cycle-detection flag 158 s, peak 6,172 MiB, 89,475,594 bytes, median run 1.13 s; COMPOSITE WITHOUT
     the flag (the control) crossgen2 "Insufficient memory" at the 11 GiB cap, no exit before the 2,700 s timeout.
     THE GATE, the fixed converter's own profile with no override (seat tip over f57648805c): `dotnet msbuild
     -getProperty` reads PublishReadyToRun=true and PublishReadyToRunComposite=false; publish rc 0 in 505 s from a
     purged tree, peak 3,069 MiB, 34,286,516 bytes, median run 0.76 s. Every arm that published printed `go run`'s
     output. Median run = the middle two of 10 warm runs. -->

<!-- RETIRED 2026-11-05 (C2, the .3 queue's S6): "On Linux, a converted test suite can skip its unix-socket tests
     under a long TMPDIR". The entry read: "The converted test host keeps each run's temporary files under
     `go2cs-tests/<package>/<run id>/.tmp` inside `TMPDIR`, about 60 bytes deeper than Go's own `t.TempDir`. Linux
     limits a unix socket's path to 108 bytes, so a test that binds a socket there can fail to bind under a long
     `TMPDIR` and is reported as skipped where Go passes it (`bufio`'s `TestCopyUnixpacket`, for example). Run
     converted test suites with a short `TMPDIR`, such as `/tmp`; 10 bytes or fewer leaves every socket path in
     range."
     The host's run root is now `go2cs-t/<8 hex>` (src/core/testing/TestHost.cs, RunRootParentName), so a
     t.TempDir's parent sits 21 bytes below the temp path, against 50 bytes plus the package name before.
     Measured on linux/amd64 at master 1e99f5bb44, bufio -tests -test-action all under the 19-byte
     TMPDIR=/tmp/c2s6-tmpdir-xx: the old host SKIPPED TestCopyUnixpacket ("bind: invalid argument", socket path
     116 bytes) and the package did not match; the new host PASSED it (82 bytes) and validated bufio at 81 + 1,
     the banked linux row. GolibTests' HostRunRootLengthTests is the guard (red at the old host: 55 bytes).
     Census of GOROOT's tests at go1.26.10: bufio's TestCopyUnixpacket is the only test that binds a socket inside
     t.TempDir; net, os and nettest bind theirs under os.MkdirTemp(""), which the run root does not touch, and
     unixpacket does not exist on darwin or windows. Residual, stated: the socket path is now 63 bytes plus TMPDIR
     against Go's 42, so a linux TMPDIR of 45 to 65 bytes still skips that one test where Go passes. -->
