# Known issues

This page lists specific bugs, each with a workaround. For the limits that come with running Go on .NET, and
the rough spots real code hits today, see [Limitations](Limitations.md).

Each entry describes go2cs as it is on the master branch and in the 1.26.11.1 release. Where the two differ, the
entry says which has the issue. Issues that affect only releases before 1.26.11.1 are at the end of the page, under
[Issues in older versions](#issues-in-older-versions).
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (owner rule of 2026-11-22: lead with the current state in
     present tense; issues of older versions move to the bottom with their text and anchors). Every entry was read
     against master 97cca467a2 and the 1.26.11.1 tag (nuget-1.26.11.1, ec3e170e4b). Changes:
     - this paragraph, and a new last section "Issues in older versions";
     - moved there, text and comments unchanged: "With go2cs 1.26.10.2 or earlier, ... built Debug", "With the go.*
       1.26.10.1 packages, a Native AOT program that uses the network can exit with an error" and "With the go.*
       1.24.13.7 packages or earlier, a Native AOT publish can take hours or not finish" (each heading kept, so each
       anchor still resolves);
     - modernc sqlite seed: a -recurse run derives the seed on master (train 2, claude/c2-recurse-importer-seed, merge
       00b0dc37dd); 1.26.11.1 still needs the flag;
     - M:N: master only (no GO2CS_MN in golib at the tag); GO2CS_MN_PROCS's "goes away in a later release" restated;
     - stale local root and the Native AOT error assertion: "Fixed in the next go2cs release" restated as fixed on
       master (merges 6379495c3b, train 1, and 42d78181d4, train 3) and still present in 1.26.11.1 (UX-8);
     - Ubuntu .NET: "It becomes the default if ..." restated as the present rule (UX-8);
     - probe cache: master only (-probe-cache and GO2CS_PROBE_CACHE absent at the tag, merge dcfc9c99a1); 1.26.11.1
       probes into GOCACHE;
     - Δ-led names: the refusal is master only (merge e25362aedc, train 4); 1.26.11.1 converts them and drops the Δ
       silently; "Fix owed" restated as still open (the three golib projections are unchanged at 97cca467a2);
     - runtime.Caller: tests compiled unoptimized is master only (GoTestOptimize absent at the tag). -->

## Writes through the address of another package's pointer or interface variable are lost

Taking the address of a variable that another package exports gives a pointer to the variable itself when its
type is a value type. When its type is a pointer or an interface (`&otherpkg.ErrMissing`, for an
`ErrMissing error`), the pointer refers to a copy, and so does the address of `runtime.MemProfileRate` and of a
variable declared with a two-argument `//go:linkname`.
[Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go) describes the cases and the
workarounds.

## On Apple silicon, the standard library is the `amd64` build {#on-apple-silicon-runtimegoarch-reports-amd64}

The packages ship for five platforms: `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64`. The converted standard library
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
     src/core/runtime/linux-amd64/extern.cs:375 (`GOARCHᵇ = "amd64";`) and, until the run-time GOARCH seat of 2026-11-09,
     windows' and darwin's extern.cs at the same line;
     src/core/internal/goarch/amd64/zgoarch_amd64.cs (GOARCH "amd64", IsAmd64 1, IsArm64 0) is the only zgoarch file; and no
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
<!-- Amended 2026-11-09 (G, the linux/arm64 flavour's corpus apply): the two constants' paths above are re-cited to
     where the apply moved the files, content unchanged (runtime's linux/extern.cs to linux-amd64/, internal/goarch's
     zgoarch_amd64.cs to amd64/). "The only zgoarch file" and "no *_arm64.cs file" describe the tree as read on
     2026-11-08: the apply lays a linux/arm64 flavour out beside the three (src/core/runtime/linux-arm64/extern.cs,
     src/core/internal/goarch/linux-arm64/zgoarch_arm64.cs). It compiles there and does not run yet, and no package
     ships for it. The darwin and windows flavours are as this entry describes them. -->


<a id="the-packages-do-not-ship-linux-arm64"></a>
## Converting modernc sqlite for Windows needs a seed for libc's `largeInteger` {#converting-modernc-sqlite-for-windows-needs-a-seed-for-libcs-largeinteger}

A converted `modernc.org/sqlite` program on Windows panics the first time it opens a database in WAL mode when
libc was converted without a seed for its `largeInteger` type. On the master branch, a `-recurse` conversion of the
sqlite module seeds libc by itself. With go2cs 1.26.11.1, and for a libc converted on its own, the conversion needs
`-inline-arrays=modernc.org/libc#largeInteger`. The panic names both delegate types:

```text
panic: go2cs: func cookie 0x... holds a
  System.Func<ж<libc_package+TLS>, uintptr, libc_package+largeIntegerᴛ1, uintptr, UInt32, Int32>,
  which cannot be read as
  System.Func<ж<libc_package+TLS>, uintptr, sqlite.lib.sqlite3_package+TLARGE_INTEGERᴛ1, uintptr, UInt32, Int32>
```

Go calls sqlite's `TLARGE_INTEGER` and libc's `largeInteger` one type. sqlite converts its spelling whole through
`unsafe.Pointer`, so it holds that struct's arrays inline. libc never does, so without the seed its spelling keeps
golib's `array<T>`, and the two are different C# types. A function word that libc builds over its own type is
then called through sqlite's type, and the call fails. The seed makes libc lay the type out as sqlite does, so both
packages share one lift. A database that is never opened in WAL mode does not reach the call.

**To avoid it**, convert the sqlite module and libc together (`-recurse`). On the master branch that is enough:
the run seeds libc's `largeInteger` because sqlite admits the same struct, and emits what the flag emits. With go2cs
1.26.11.1, also pass `-inline-arrays=modernc.org/libc#largeInteger`; the same value is harmless on other targets,
where libc declares no `largeInteger`. A libc converted on its own (as a separate package) is seeded by neither
route, so it needs the flag on every release, or sqlite on Windows hits the panic again when built against it.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: the -recurse importer seed landed in train 2
     (claude/c2-recurse-importer-seed, merge 00b0dc37dd). Its rule: docs/ConversionStrategies-Reference/slices-and-arrays.md,
     "A struct read from native memory holds its arrays inline, and seeds make two packages agree" (a package converted
     on its own is not seeded by its future importers). Its census, the BOARD row of 2026-11-17 (C2, ruling C item 3):
     windows/amd64 derives 1 seed, libc#largeInteger, and recovers "the flag's, line for line". The text before this
     pass said the flag was always needed, which is 1.26.11.1's state (no inlineArrayImporterSeed.go at the tag). -->

<!-- Source: COORD ruling C, 2026-11-17 12:14Z (C2, claude/c2-c-inline-seed). G's windows WAL read at 6bcc05d11f
     (mailbox 01d7b332fe): delete pass equal to Go, WAL pass panics at _winHandleSeek <- _winHandleTruncate <-
     _winLockSharedMemory <- _winShmMap with the message above; it was ruling A's known limit. Census at bd2a09c423:
     sqlite/lib <- libc largeIntegerᴛ1 need=true have=false (valueclone record), so sqlite/lib kept its own lift.
     The seed's end-to-end arm is TestAdmittedUnionByValueCrossingWithTheSeedReadsAsGo (libc's union verbatim). -->

## Platforms the packages do not ship

The go.* packages ship `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64`. A package
whose code differs by platform carries one folder per platform under `runtimes/`, and a platform without one gets
the Windows build in `lib/`. A build resolves its platform the way .NET does at run time, so `linux-musl-x64`
(Alpine) uses the `linux-x64` folder. For a platform that resolves to no folder, the build reports it:
- **`GO2CS3003`, an error**, on another operating system, or on Linux with another architecture: linux-riscv64 would
  run the Windows build, and the `linux-x64` build makes x64 system calls. The message names the platform and the
  first package without a folder for it.
- **`GO2CS3004`, a warning**, on a Windows platform the packages do not ship (`win-arm64`): the Windows build is
  that operating system's, so the program runs. What Go decides by architecture when the package was converted
  follows `amd64`.

A cross-build for a shipped platform from an unshipped host is not affected: it names its platform with
`-r <rid>` (`RuntimeIdentifier`), and that platform has its folder.

**To avoid it**, build for one of the five shipped platforms. A build that only needs to compile, and knowingly wants
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
<!-- Amended 2026-11-10 (P2, seat S6 of DESIGN-linux-arm64-flavour.md, COORD ruling): the packages ship linux-arm64
     (push-nuget.ps1 builds the linux/arm64 flavour by its GOOS/GOARCH pair, branch claude/p2-la64-s6-packaging), so
     the heading, which named linux-arm64 as unshipped, became "Platforms the packages do not ship", and GO2CS3003's
     example moved to linux-riscv64, as S0's gate did. The old anchor is kept as an explicit id: go.lib.targets in the
     packages already published links to it. -->

## On linux-arm64, a few standard-library members throw when called {#on-linux-arm64-15-standard-library-members-throw-when-called}

The linux-arm64 build of the standard library has a few members that Go implements in arm64 assembly and that have
no managed body yet, so each throws when called: `getisar0` and `getpfr0` in `internal/cpu`; `getisar0`, `getisar1`,
`getpfr0` and `getzfr0` in `vendor/golang.org/x/sys/cpu`; `load_g`, `save_g`, `spillArgs`, `unspillArgs`,
`asmcgocall_no_g`, `emptyfunc`, `panicBounds` and `panicExtend` in `runtime`; and `rawVforkSyscall` in `syscall`.
Most of them exist only in the linux-arm64 build; the rest are routines that the other builds also leave without a
body.
<!-- 2026-11-15 (P4, docs audit Q3): the heading and text no longer carry the census counts (15; nine and six), which
     no standing process re-measures; the explicit id keeps the old anchor, which published pages link. -->

**To avoid it**, do not call these members. All of them are internal to the standard library, so a program reaches
one only through a standard-library path; if yours does, report the call path.
<!-- Added 2026-11-11 (P2, seat S6, COORD ruling Q2 of the S6 plan delta): the count and the names are the 15 stubbed
     rows of the complete linux-arm64 declaration table in docs/phase4/DATA-la64-s3-census.md ("Amendment, 2026-11-10",
     57 declarations, 42 bodied, 15 stubbed), re-read at master 50dd9a065c by G (docs/phase4/DATA-la64-s7-bank-plan.md
     section 9.2 on claude/g-la64-s7-e1, member for member). Six of the 15 are each architecture's own assembly and are
     stubbed at linux/amd64 too; nine exist only at linux/arm64. -->

## On macOS, a `go` statement past the system's thread limit ends the program

By default each goroutine is an operating-system thread, and macOS caps how many threads one process may run. When a
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

## On Linux, a program with more than about 16,000 goroutines alive ends

By default each goroutine is an operating-system thread, and on Linux each .NET thread uses about four of the process's memory
mappings. Linux caps the mappings at the `vm.max_map_count` setting, 65,530 by default. When a program has about
16,000 goroutines alive (measured on `linux-x64`, Ubuntu 22.04 under WSL, at the default setting), the next `go` statement fails with `Fatal error. Failed to create RW mapping for RX memory`,
and the program ends; nothing can catch it. A Go goroutine is not a thread, so the limit does not apply to Go itself.

**To avoid it**, bound how many goroutines are alive at once, with a worker pool for example. Two settings may raise
the limit, but neither has been measured: `DOTNET_EnableWriteXorExecute=0` in the environment, which should cut the
mappings each thread uses, and a larger `vm.max_map_count` on the machine.

**Under the M:N scheduler** (experimental, on the master branch only: `GoTwins=true` and `GO2CS_MN=1`), a goroutine parked in a twin holds no
thread, so the limit counts only the goroutines blocked in calls the twins do not cover: a `sync.WaitGroup` or
`sync.Cond` wait, a Lock reached through a `sync.Locker` interface, a cgo or system call. About 16,000 of those blocked
at once end the program the same way (`PAL_SEHException`, then an abort); the same bound avoids it.
<!-- Source (the M:N sentence): G's s_p convoy row, 2026-11-22, docs/phase4/DESIGN-mn-convoy.md section 7.
     MnBlockReturn (n goroutines blocked on one sync.WaitGroup), linux flavour, Ubuntu 22.04 under WSL, default
     vm.max_map_count 65530: n=20,000 aborted (PAL_SEHException, SIGABRT); a live n=10,000 run peaked at 40,427
     mappings with 9,985 threads (about 4 per thread), so the limit falls near 16,000. -->
<!-- Source: lane R's M:N R6 run, 2026-11-09, at master 0bbe3ab6e5: ParkStorm (DESIGN-mn-scheduler-sizing.md Appendix A),
     20,000 goroutines, linux flavour, Release, on R-WSL (Ubuntu 22.04, kernel 6.18, vm.max_map_count 65530). All six
     runs (bounded reserve 16 MiB and 1 MiB, three each) aborted at 16,226-16,229 threads. The record is the dated O1
     amendment in docs/phase4/DESIGN-mn-scheduler.md. -->

## Goroutines on a shared pool of threads (M:N) are experimental, and off unless you build and run for them {#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them}

By default each goroutine is an operating-system thread, which is what the two sections above describe. go2cs also has
an experimental M:N scheduler, in which goroutines take turns on a small pool of threads as Go's do. The scheduler is
on the master branch only; go2cs 1.26.11.1 does not have it. It is inert unless both halves are switched on:
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: `git grep GO2CS_MN nuget-1.26.11.1 -- src/core/golib` finds
     nothing, and src/Directory.Build.targets has no GoTwinsFor at the tag; the M:N work landed in trains 1 to 7. -->


- **The build**: the projects are built with twins, an async copy of each function that can wait without holding a
  thread. The `GoTwinsFor` property names the projects to build with twins, separated by `;`. Set it in the
  environment: on the `dotnet build` command line each `;` must be written `%3B`, or MSBuild ends the property there.
  `GoTwins=true` in one project's own file builds that project with twins. These properties are read by
  `src/Directory.Build.targets`, so they work only in a project built inside a go2cs repository clone. The published
  `go.*` packages are not built with twins, and a project that references them gets no twins from them.
- **The run**: the environment sets `GO2CS_MN=1`. How many goroutines run at once follows `GOMAXPROCS`, as in Go: the
  `GOMAXPROCS` environment variable at start, and `runtime.GOMAXPROCS(n)` while the program runs. `GO2CS_MN_PROCS=<n>`
  pins the count instead; it is a measurement switch, not a supported setting.
  <!-- 2026-11-23 .5 docs pass (P1), UX-8: was "it is a measurement switch and goes away in a later release"; no
       release removing it is scheduled, so the sentence states what it is now. -->


Without `GO2CS_MN=1` a program built with twins runs on threads as usual, and `runtime.GOMAXPROCS` reads and sets the
value Go's does but caps nothing: the operating system decides how many threads run at once. With it, a goroutine that
waits inside code built without twins keeps its thread while it waits. The other `GO2CS_MN_*` variables the scheduler
reads are measurement switches, not settings: none is supported, and they may change or go away. The diagnostics
`GO2CS0014`, `GO2CS0015`, `GO2CS0017`, `GO2CS0018`, `GO2CS0020` and `GO2CS0021` ([Diagnostic codes](DiagnosticCodes.md)) come only from a build with twins.<!-- 2026-11-23 .5 docs pass (P1): the old range "GO2CS0014 through GO2CS0018" took in GO2CS0016, the GoFrame rule, which every build runs (DiagnosticCodes.md rows 0014-0021 at 97cca467a2). -->

**A plain read and a call in the same expression can see different values with and without twins.** In an expression
such as `fmt.Println(a, f())`, Go orders only the calls: the read of `a` may come before or after `f()`, and the gc
compiler reads it after. The converted code reads operands left to right, as C# does, so when `f` changes `a` it prints
the value `a` had before the call. Built with twins and run with `GO2CS_MN=1`, a call that can wait is sometimes
evaluated ahead of the plain reads in its expression, as gc evaluates it, and then the program prints the value after
the call. Both orders are allowed by the Go specification; a program whose output depends on the difference relies on
unspecified behavior.

## A nugetgo package built on 1.26.10.3 beside the go.* 1.26.11.1 packages fails at restore {#a-nugetgo-package-built-on-126103-beside-the-go-126111-packages-fails-at-restore}

**Symptom.** `dotnet restore`, or the restore a `dotnet build` starts with, stops with `NU1107`, a version conflict
on one `go.*` package (`go.internal.bytealg`, for one). The project references a `nugetgo.*` revision built on
1.26.10.3 and `go.*` packages at 1.26.11.1.

**Cause.** Each `go.*` package pins the `go.*` packages it depends on to its own exact version. A `nugetgo.*`
revision built on 1.26.10.3 accepts any `go.*` version from 1.26.10.3 up to 1.27.0. When nothing else in the project
asks for a newer version of one of those packages, NuGet takes the lowest, 1.26.10.3, and that package's exact pins
conflict with the 1.26.11.1 pins of the `go.*` packages the project references. Restore fails before the build
runs, so the break-floor check, which would name the old package (`GO2CS3002`), is never reached.

**Workaround.** Move each `nugetgo.*` reference to its 1.26.11.1 revision, as
[Upgrading from 1.26.10.3](NEWS.md) says. A revision built on 1.26.10.3 is below the break floor in any case: if
you reference its `go.*` dependencies at 1.26.11.1 directly, restore succeeds and the build stops with
`GO2CS3002`, naming that package.

<!-- Found 2026-11-20 by P2's nugetgo restore rehearsal (wall W-ng-ctl, mailbox 01:31Z and 01:41Z): the floor
     control, google/uuid's revision 3 (built on 1.26.10.3) beside go.fmt 1.26.11.1, failed at RESTORE with NU1107 on
     go.internal.bytealg: uuid rev 3 references go.bytes at [1.26.10.3, 1.27.0), go.fmt's closure does not lift
     go.bytes, so NuGet took 1.26.10.3, whose exact pin go.internal.bytealg (= 1.26.10.3) collides with go.os
     1.26.11.1's (= 1.26.11.1). P2's fix (5377c56403) has the control consumer reference the revision's direct go.*
     dependencies at the release, after which the build refuses it with GO2CS3002 as planned. First post-.4 docs
     batch, COORD 2026-11-20 07:19Z. -->

## A `-recurse=nuget` conversion reads an older local go2cs root and emits names the packages do not have

**Symptom.** A program converted with `go2cs -recurse=nuget` does not build against the published `go.*` packages,
with `CS0426` on a standard-library name that carries a `Δ` prefix the packages no longer have, for example
`reflect_package.ΔKind`. The conversion itself reports no warning and exits 0. The names affected are the
standard-library names that earlier go2cs releases prefixed with `Δ` and the published packages no longer do, about
170 across about 70 packages, among them `time.Second`, `time.Month`, `reflect.Type`, `reflect.Value` and
`os.Signal`, so most programs converted on such a machine meet it.

**Fixed on the master branch**: under `-recurse=nuget` the converter reads standard-library names only from the
metadata built into it, and says once when it finds a local go2cs standard library that it does not read. go2cs
1.26.11.1 still has the issue; with that release, use the workaround below.
<!-- 2026-11-23 .5 docs pass (P1), UX-8, at master 97cca467a2: was "Fixed in the next go2cs release". The fix is on
     master since 2026-11-20, train 1 (claude/i9-nuget-metadata-precedence, merge 6379495c3b); publishedStdLibRecord
     is in src/go2cs at 97cca467a2. -->


**Cause.** Under `-recurse=nuget` the converter still looks for a local go2cs root: `-go2cspath` when it is given,
else the `GO2CSPATH` environment variable, else `~/go2cs` (`%USERPROFILE%\go2cs` on Windows). When that folder holds
a go2cs standard library, the converter reads each imported package's names from it, ahead of the metadata built
into the converter, which is the metadata that matches the published packages. A root from an older go2cs release,
such as one left by an earlier `deploy-core` or a go2cs clone, supplies that release's names. On a machine with no
such folder and `GO2CSPATH` unset, the emission is correct.

**Workaround** with go2cs 1.26.11.1 and earlier. Make the run unable to find an old root: unset `GO2CSPATH` and pass `-go2cspath` an empty folder, or
move the old `~/go2cs` aside for the run. Then convert again. Measured on `windows-amd64` and `linux-x64`.

<!-- Found 2026-11-20 by the sqlite-cs staging at the 1.26.11.1 release converter (5dd5cafd5b): i9 (COORD inbox
     20261120T073737Z-i9, windows-amd64) traced CS0426 reflect_package.ΔKind to a stale ~/go2cs deploy whose
     core/reflect/package_info.cs carries the pre-OQ2 GoTypeAlias("Kind", "ΔKind") record, read by
     importOperations.go loadImportedTypeAliases before the embedded stdlib-metadata.txt (which has no Δ); a GO2CSPATH
     pointed at an empty folder gave reflect.Kind. P5 (20261120T073911Z-P5, linux-x64) reproduced it in three arms on
     the sqlite-cs demo: no root and the release tree as root both clean, a pre-OQ2 clone as -go2cspath gives the
     ΔKind alias. resolveGo2CSPath skips the root warning for nugetRefs, so the run is silent. The .5 fix (i9: a
     PublishedStdLib dependency reads the embedded record first) retires this entry. First .5 docs item, COORD
     2026-11-20 07:40Z.
     The fix, claude/i9-nuget-metadata-precedence (954b184a3d, then this note): publishedStdLibRecord is read before any
     on-disk package_info.cs for a PublishedStdLib dependency, and resolveGo2CSPath gives a one-time notice when
     -recurse=nuget finds a core\ tree under its root (keyed on core\, since i9's stale root had no core\golib).
     The scope above is the census in docs/phase4/data/nuget-metadata-precedence/census-oq2.txt: the embedded record at
     780cb9b34f (the last before OQ2) against 47ce71a520 lists 208 Delta-renames dropped by OQ2 in 91 sections, 169
     distinct names in 72 packages once the per-platform sections are folded, and 0 left at 47ce71a520. COORD
     2026-11-20 11:36Z: the census goes here, and the entry reads "fixed in the next release" at landing. -->

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
machine) it costs about 90 ms (16%) more startup time, and steady-state loop speed stays within measurement noise. It is
not the default, because no converted Go program is known to reproduce the fault.
<!-- 2026-11-23 .5 docs pass (P1), UX-8: was "It becomes the default if a converted Go program ever reproduces the
     fault." The owner's ruling of 2026-09-30 (opt-in, not default; the comment below) is the present rule. -->
 [Tuning converted code](Tuning.md#net-runtime-settings) lists the runtime settings and
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

## Under Native AOT, a type assertion to `error` that the method set decides can answer false

**Symptom.** In a Native AOT executable (`PublishAot`), a Go type assertion to `error` answers `ok == false` for a value
whose method set has `Error() string` when no compile-time adapter for that pair exists. Measured with
`var p any = new(runtime.PanicNilError); _, ok := p.(error)`: Go and the JIT answer `true`, Native AOT answers `false`.
No exception is raised, so the program goes on with the wrong answer. A failing assertion still answers correctly.

**Fixed on the master branch**: `error` declares a second run-time shell that holds the value without a generic
type closed over it, so Native AOT has code for it and the assertion answers as Go's does. The go.* 1.26.11.1
packages still have the issue; with them, use the workaround below.
<!-- 2026-11-23 .5 docs pass (P1), UX-8, at master 97cca467a2: was "Fixed in the next go2cs release". The fix is on
     master since 2026-11-21, train 3 (claude/p2-aot-structural-hit, merges a9ee17b1eb, 82110652f9, 42d78181d4);
     golib's error.cs stamps errorObjectShell at 97cca467a2 and not at nuget-1.26.11.1. -->


**Cause.** The runtime library finds the method (its method-set check answers `true`), then builds the interface value
from `error`'s run-time shell, `error<T>`, closed over the value's type with `MakeGenericType`. Native AOT has no code
for that instantiation, and `error` declared no second shell that needs none, so the assertion read as a miss.
Inferred from the code, not measured: interfaces the converter generates declare that second shell, so this reaches
`error` and not them.

**Workaround** with the go.* 1.26.11.1 packages. Publish without Native AOT: the converted project's default
`dotnet publish` (ReadyToRun) and a framework-dependent build run the same code under the JIT, which answers as Go
does.

<!-- Found 2026-11-21 by P2 while adding a judged true line to the os-matrix aot-smoke consumer
     (src/tests/PackageTests/PackageSymbols) for the train 1 ReceiverKeyOf crash fix. linux-x64, ILC 10.0.14: the
     consumer read "false" for the PanicNilError assertion at the fix, at master before train 1 (1fa80494ee) and
     "true" under the JIT. A diagnostic image read builtin.Implements<error> true, p.(runtime.Error) true (a compile-time
     adapter) and p.(error) false. error.cs, AdapterBinder.cs and builtin.cs are unchanged between nuget-1.26.11.1 and
     1fa80494ee, so the go.* 1.26.11.1 packages carry it (inferred). Its fix is seat claude/p2-aot-structural-hit.
     The fix, claude/p2-aot-structural-hit (05728981b3): golib's errorObjectShell, stamped as error's object shell, holds
     the value as object and forwards Error through the binder's GoShellBinding. The consumer, judged on both lines,
     read "false; false" at the red arm 61ce8a52fc and "false; true" at the fix under Native AOT (linux-x64, ILC
     10.0.14), the image 32 bytes larger; GolibTests ErrorObjectShellTests, 4 tests, fail with the shell unstamped. -->

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
that gates each release publishes a C# consumer of the packages and runs it on `linux-x64`, `linux-arm64`,
`win-x64` and `osx-arm64`. On `osx-x64` the publish is read when it finishes inside the hosted runner's time limit, and reported as
not measured when it does not. A project that sets no trim mode of its own gets the packages' default, `partial`,
which is the setting a converted project carries in its generated project file.

A full trim (`TrimMode` set to `full`) produces a program that runs as well. That is measured on `win-x64`, on
one development machine, with the 1.24.13.5 packages; the hosted check measures the default.

**What it costs.** With `partial`, Native AOT compiles every `go.*` assembly the program references in
full, not only the parts the program uses. With the published 1.26.11.1 packages, on `linux-x64` (a 4-core cloud
machine with 15 GB and .NET SDK 10.0.404), the README walkthrough's program, which uses github.com/fatih/color, took
18 minutes 9 seconds to publish; the executable is 154.5 MB and runs in 0.21 seconds. A converted project pays the same
cost, because it uses the same setting. With the 1.26.10.3 packages the walkthrough's program does not publish at
all on `linux-x64`: the compiler stops at the native string table's 2 GiB ceiling
([how to recognize it](Tuning.md#native-aot-method-body-folding)). A build, `dotnet run` and a publish without
Native AOT are not affected. For the 1.24.13.7 packages and earlier, see
[Issues in older versions](#with-the-go-124137-packages-or-earlier-a-native-aot-publish-can-take-hours-or-not-finish).
<!-- 2026-11-23 .5 docs pass (P1): was "see the section that follows"; that section moved to the end of the page. -->


<!-- 2026-11-20 (P4, first post-.4 docs batch, COORD 07:19Z): the cost sentence read "On a 4-core Linux cloud machine,
     converted from source at the tree of the coming 1.26.11.1 release, a hello world that imports `strings` and `fmt`
     took about 19 minutes to publish, and the README walkthrough's program, which uses github.com/fatih/color, about 26
     minutes" (P1's readings, the comment below). Replaced by P3's cold first-user pass on the PUBLISHED 1.26.11.1
     packages (mailbox 2026-11-19 23:12Z): a new empty HOME, nuget.org the only source, `dotnet publish -r linux-x64
     -p:PublishAot=true` on the colordemo, rc 0 in 18 min 9 s, 154.5 MB, the executable ran in 0.21 s with rc 0 and Go's
     line; 12 warnings, all of the documented classes (8 IL2104, 3 IL3053, 1 IL3000). -->

<!-- 2026-11-16 (P4, P3's docs item 3 and COORD's AOT ruling): the cost sentence's figures were "under four minutes
     on a hosted CI runner for Linux and for Windows, about six minutes on a Mac with Apple silicon and about eleven on
     an Intel Mac (measured 2026-10-14, with packages converted from Go 1.24.13)", a reading of the 1.24.13 packages
     that no 1.26 run repeats. Replaced by P1's readings, 2026-11-16, a linux-x64 cloud box (4 cores, SDK 10.0.403,
     ILCompiler 10.0.14), local -recurse at master 0c1d295ea8 with <PublishAot>true</PublishAot> in the program's
     csproj: the strings/fmt hello 1146 s, colordemo (fatih/color v1.18.0) 1545 s, both executables ran, 0 'will
     always throw'. The 1.26.10.3 failure: P3 runs A and B (2026-11-15) and P1 row 2 (2026-11-16): ILC fails at
     StringTableBuilder.CreateIndex after 1509-1599 s with 6,188 'will always throw' lines; batch 308's lever G
     (merge 055f4e6230) removed the generic cycle that caused it.
     2026-10-26: re-read at a6214f0c5d. The visible costs are the hosted aot-smoke readings after golib's generic-cycle
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

## A conversion keeps a Go build cache of its own

<!-- Measured 2026-11-20 on linux/amd64, go1.26.11, converter 47ce71a520, github.com/yuin/goldmark v1.7.13 with
     -recurse=module (11 packages): first run 33.0 s and 227 MB of cache, second run 7.3 s and no growth; the
     standard-library closure under the probe's flags is about 60 MB per target. A -recurse conversion of a 97-package
     module filled about 12 GB. Ruled by COORD 2026-11-20 09:41Z (board hh): a dedicated persistent cache, separate
     from GOCACHE.
     Measured the same day on one very large package of another module: its probe entries took about 2,273 MB
     (about 200 MB of compiled object per target). Plain-statement ask from COORD 10:19Z. -->
For each package it converts, go2cs runs `go build` (and `go test -c` for test files) with `-gcflags=-m` for four
targets, so it emits the same escape decisions Go makes. Those builds go into their own cache, not your `GOCACHE`:
`go2cs/probe-cache` under the user cache directory, or the directory named by `-probe-cache` or `GO2CS_PROBE_CACHE`.
The first conversion builds the standard library there for each target, about 60 MB each, and later conversions of
any module reuse it. Each converted package adds its own entries, which grow with the size of its compiled code: about 5 MB
for a goldmark package, and about 2.3 GB for one very large package. The separate cache moves this cost out of
your `GOCACHE`; it does not make it smaller. The first conversion of a large module needs that much free disk
once (a 97-package module needed about 12 GB), and later conversions reuse it.

Go removes entries unused for five days. To reclaim the space at once, delete the directory; the next conversion
rebuilds what it needs:

```
rm -rf ~/.cache/go2cs/probe-cache
```

On Windows, the directory is `%LocalAppData%\go2cs\probe-cache`; on macOS, `~/Library/Caches/go2cs/probe-cache`.

With no user cache directory (neither `HOME` nor `XDG_CACHE_HOME` set), the cache is `go2cs-probe-cache-<uid>` under
the temporary directory, created with mode 0700. Because that directory is shared, go2cs uses it only while it is a
directory owned by you that no other user can open; otherwise the run stops before its first probe and asks for
`-probe-cache`. A run that does not probe, such as `-test-action build`, never needs the directory.

The separate cache is on the master branch. go2cs 1.26.11.1 runs the same builds in your own `GOCACHE`, so they take
the same space there, and it has neither `-probe-cache` nor `GO2CS_PROBE_CACHE`.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: the probe cache landed in train 1 (claude/p4-probe-cache,
     merge dcfc9c99a1). At nuget-1.26.11.1, src/go2cs has no probe-cache flag or GO2CS_PROBE_CACHE, and the escape
     probe's go build runs with the caller's environment, so its entries go to the default GOCACHE. -->


## A Go struct field, method or type whose name starts with `Δ` refuses its package {#delta-led-field-method-type-refused}

**Symptom.** Converting a package that declares a struct field, a method (an interface method included) or a named type
whose name starts with `Δ`, such as `type P struct{ Δt float64 }`, stops with `error GO2CS2015`, which names the
identifier and what Go would report for it. A `Δ`-led local, parameter, package variable, constant or function converts.

**Cause.** go2cs renames a Go name that collides with something it generates by adding a leading `Δ`, and the runtime
library reads a leading `Δ` back as that marker: it drops it when it gives a converted field, method or type its Go name.
A Go name that itself starts with `Δ` would then lose it: `P.Δt` would reflect and json-encode as `t`, unexported, so
`json.Marshal(P{1})` would write `{}` where Go writes `{"Δt":1}`, and `reflect.TypeOf(ΔMatrix{}).Name()` would read
`Matrix`. The refusal turns that silent difference into a stop at conversion.

The refusal is on the master branch. go2cs 1.26.11.1 does not refuse these names: it converts them, and the field,
method or type then reflects and encodes without its leading `Δ`, with no warning.

**Workaround.** Rename the field, method or type so it does not start with `Δ` (`Dt`, `DeltaT`). A struct tag keeps the
encoded name: `` Dt float64 `json:"Δt"` ``.

**Still open.** The runtime library is not yet told which `Δ` go2cs added. Until it is, a user's `Δ`-led field, method
or type cannot read back under its own name, and the refusal stays.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: was "Fix owed. The runtime library is told which Δ go2cs
     added, so ... this refusal is retired." (UX-8). The three projection sites named below still drop a leading Δ
     at 97cca467a2, and no cure has landed. The refusal (GO2CS2015, src/go2cs/markerGlyphRefusal.go) landed in train 4
     (claude/i9-marker-glyph-refusal, merge e25362aedc); at nuget-1.26.11.1 the file does not exist and the same
     projections drop the Δ. -->


<!-- P3 code read of the R1 seat (inbox/COORD/20261122T045652Z-P3.md, finding 2), ruled by COORD 045750Z: refuse now
     (2a), durable cure as its own .5 seat (2b). The three projection sites: src/core/golib/GoReflect.FieldAccess.cs:404
     `if (projected.StartsWith(ShadowVarMarker, StringComparison.Ordinal))` (fields), GoReflect.TypeNaming.cs:718 `if (name.StartsWith(ShadowVarMarker, StringComparison.Ordinal))` (type names), runtime/TypeExtensions.GoMethodSets.cs:709 `emittedName.StartsWith(ShadowVarMarker, StringComparison.Ordinal)` (methods). The
     refusal: src/go2cs/markerGlyphRefusal.go, deltaProjectionReason. -->

## In optimized code, `runtime.Caller` can report an earlier line of the calling function {#in-optimized-code-runtimecaller-can-report-an-earlier-line-of-the-calling-function}

<!-- Measured 2026-11-22 on linux-x64 (.NET 10), go1.26.11, converter/generator at claude/c2-m1-test-noopt, with a
     converted program whose function keeps runtime.Caller(0)'s line in a local across a three-line method chain and
     then reads its own line from a slice literal; Go answers line+5. Release, default tiering: +5 on the first call
     and after 500 calls, 0 after 200,500 calls and a pause (tiered up). Release with DOTNET_TieredCompilation=0: 0 from
     the first call. Debug build: +5. Release with DOTNET_JITMinOpts=1: +5. Cost, crypto/sha256 over 64 MiB in the same
     program, two runs each: Release default 0.71 s, Release TieredCompilation=0 1.4-1.6 s, Release JITMinOpts=1
     23.4-23.9 s, Debug 26.7-27.1 s (Go: 63 ms). COORD 211538Z and the owner (212210Z) ruled: test functions only are
     compiled unoptimized; production keeps its optimizations, with these routes documented.
     Same-name calls, measured 2026-11-22 with a C# probe (three chained calls of one method, each callee reading its
     caller's StackFrame.GetILOffset): Release gives 5, 5, 5 under the tiered JIT, TieredCompilation=0 and JITMinOpts
     alike; Debug 7, 13, 18; Release IL in an assembly marked DisableOptimizations 5, 11, 16. A converted test chain
     read [23 23] under JITMinOpts and [23 24], Go's answer, with its test assembly compiled Optimize=false (COORD
     23:57Z, option 2: the test assembly switch, go.lib.symbols.targets). -->
A function compiled with optimizations can lose the exact position of a call it makes. A `runtime.Caller` that reads
that function's frame, from a function it calls, then reports the line of the function's first statement instead of
the line of the call. Go always reports the call's line. This is measured on `linux-x64` and follows from how .NET's
optimizing JIT records positions, so it is expected on every platform.

The loss needs a particular shape, measured here as a function that keeps `runtime.Caller`'s result in a variable
across a method chain written over several lines. Most functions report Go's lines. Under the default runtime
settings a converted program reports the right line until the JIT recompiles the function with full optimization,
which happens after it has run many times. With `DOTNET_TieredCompilation=0` it is compiled optimized from the start.

A `Release` build also gives every call in one statement the position of the statement. Two calls to the same function
in one statement, such as a method chain written over several lines (`b.On(1).` then `On(2)` on the next line), then
both report the line of the first call. This holds under every JIT setting, because the positions a frame can report
are fixed when the assembly is compiled.

On the master branch, converted Go tests are not affected. The test assembly go2cs writes for a package's tests is
compiled without optimization, so `t.Logf`, testify's failure locations and every other line read from test code match
Go, while the package under test stays optimized. To build a test assembly optimized, for a benchmark run, set the
MSBuild property `GoTestOptimize` to `true` (`-p:GoTestOptimize=true`). go2cs 1.26.11.1 compiles test assemblies
optimized and has no `GoTestOptimize`, so lines read from test code can show the same loss there.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: GoTestOptimize (go.lib.symbols.targets) is in src at
     97cca467a2 and nowhere at nuget-1.26.11.1; it landed in train 6 (the C2 stack, claude/c2-m2-test-switch). -->


To get Go's lines in a program, use one of these, at a large cost in speed:
- a `Debug` build, which reports both cases as Go does: in the measurement above, about 38 times slower than the
  default `Release` build on a `crypto/sha256` workload;
- `DOTNET_JITMinOpts=1` in the program's environment, which keeps a `Release` build but turns off JIT optimization: it
  reports the call's line instead of the function's first statement, but not two calls of one function in one
  statement apart; about 33 times slower on the same workload.

## Issues in older versions {#issues-in-older-versions}

These issues affect only go2cs and `go.*` releases before 1.26.11.1. Each entry names the release that fixed it.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: the three entries below moved here unchanged, in their old
     order, by the owner rule of 2026-11-22. Each fix is in nuget-1.26.11.1: the no-solution local-reference output
     (1.26.10.3), golib's AOT-safe system-call marshalling (1.26.10.2, seat #18) and golib's generic-cycle fix (e2move,
     merge b1ba1380b4, an ancestor of nuget-1.26.10.1). -->

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
