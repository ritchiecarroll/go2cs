# Limitations: where the seams are

So you're about to point go2cs at your own Go code. Before you spend an evening on it, here is where it
will fail, where it will fight you, and where it will quietly do something a little different from Go --
better you hear it from me than from a stack trace. Specific bugs with workarounds live in
[Known issues](KnownIssues.md). This page is the limits that come with running Go on .NET, plus the rough
spots real code hits today.

## The short version

**Good odds tonight:** pure Go that builds with the Go release go2cs tracks and `CGO_ENABLED=0`, leans mostly on the
standard library, and runs on x64 Windows or Linux. **A longer evening:** lots of third-party modules,
`unsafe`, assembly fast paths, or tests that pull in testify.

* **You need the Go release go2cs tracks on your PATH**, exactly the one in
  [Requirements](README.md#requirements), and **cgo is refused**.
* **Assembly, `//go:linkname` and deep `unsafe` tricks compile clean, then fail at run time.** Many
  libraries ship a pure-Go fallback behind a build tag, and you have to pass that tag yourself.
* **Each live goroutine holds an operating-system thread.** Thousands alive at once are fine. A hundred
  thousand are impractical, and Go's million is out of reach.
* **It's usually slower than Go**, starts slower and uses more memory.
* **One conversion targets one operating system.** The standard library is validated on x64
  Windows and Linux, and its macOS validation is in progress.
* **Most failures are loud. A few differences are silent**: see the last bullet under
  [what behaves differently](#what-converts-but-doesnt-behave-exactly-like-go).

## What won't make the trip

* **cgo.** A package whose build selects a file with `import "C"` stops the whole run:
  `error GO2CS2001: ... cgo has no C# conversion yet`. With no pure-Go path it is out of reach today
  ([plan](PLAN-cgo-interop.md), [Roadmap](Roadmap.md#cgo)).
* **A self-referential constraint over a pointer, when a method shares the element's storage.**
  `Walk[*Node]` under `Walk[N Tree[N]]` with `Children() []N` stops the run with `error GO2CS2002`,
  naming the site and the method. A pointer element reaches the generic code through a proxy object, and
  a `[]N`, `map[K]N`, `chan N` or `*N` the proxy passed along would be a copy, not Go's shared value.
  The same function over a value element converts, and so does a constraint whose methods pass only
  `N`, funcs and arrays ([how](ConversionStrategies-Reference/generic-constraints.md#what-a-proxy-forwards-and-the-instantiations-go2cs-refuses)).
  No Go 1.26 standard-library package reaches it.
  <!-- The refusal exits non-zero (docs/DiagnosticCodes.md, GO2CS2002), and the converted standard library is
       a complete -stdlib conversion of Go 1.26.10, so no package in it reaches the refusal. -->
* **Go newer than the library tracks.** A newer `go` line in your `go.mod`, or any dependency's, gets
  `package requires newer Go version` or a refusal naming both releases. Set `GOTOOLCHAIN=local` and pin
  older versions until `go build ./...` passes, as the
  [module walkthrough](ConvertingAModule.md#step-by-step) does.
* **Hand-written assembly.** A function whose body lives in a `.s` file usually becomes a stub. The
  project builds clean, and the first call throws a `NotImplementedException` saying
  `no implementation reached this compilation`. Convert with the library's pure-Go build tag
  ([below](#third-party-modules-where-they-trip-today)), or write the body in C#. `//go:linkname` into
  the runtime gets the same stub, outside a short list the converter knows.
* **One generics form.** `type Cell[T any] = struct{ Row, Col T }`, a generic alias of an anonymous
  struct or interface, gets a warning, and every use is a C# compile error. Name the struct instead.
* **Code that doesn't type-check for the target** still converts, with the literal text `invalid type`
  where a type is missing (`error GO2CS1003`), and the C# build stops there.

## What converts, but doesn't behave exactly like Go

* **Goroutines are operating-system threads** ([why](ConversionStrategies.md#goroutines)). A goroutine
  holds its thread for as long as it lives. When it ends, the thread waits to run the next `go` statement,
  so starting goroutines one after another is cheap; holding many alive at once is not. Measured on one
  Windows x64 machine (16 hardware threads, 31 GB), 10,000 goroutines parked at once took about 600 MB and
  four seconds to start, where Go took 100 MB and under a tenth of a second. 100,000 took minutes and about
  6 GB, and a million had not finished starting after ten minutes. `runtime.GOMAXPROCS(1)` does not make anything run one at a time.
  Bound your fan-out with a worker pool, and remember that a `net/http` server spends one on every open
  connection. [Sizing M:N scheduling](DESIGN-mn-scheduler-sizing.md) has the measurements and what removing
  the limit would take.
  <!-- Thread reuse: src/core/golib/runtime/Goroutine.cs:1100 `GoroutineThreadPool.Run(() => Run(body, goroutine, bubble));`.
       The figures are lane R's ParkStorm reading in docs/DESIGN-mn-scheduler-sizing.md §1 (Windows x64, 16 hardware
       threads, 31 GB RAM, Go 1.26.10, Release, today's 256 MiB stack reserve): 10,000 parked at 622 MB and 4.4 s against
       Go's 99 MB and 0.07 s; 100,000 at 5,904 MB and 302 s; 1,000,000 never parked within 600 s. -->
* **A deadlock usually hangs** where Go prints `all goroutines are asleep - deadlock!`. The message
  appears only when every goroutine, `main` included, waits on a nil channel or an empty `select {}`. A
  deadlock on an ordinary channel or lock blocks forever. Break in with a debugger: each goroutine is a
  thread named `goroutine-N`.
  <!-- src/core/golib/runtime/Goroutine.cs:883 `internal static bool IsForeverWait(WaitReason reason) =>`
       and :937 `internal static bool IsProvableDeadlock(IEnumerable<(WaitReason Reason, bool Readied, bool IsMain)> users)`. -->
* **Stacks don't grow.** A goroutine's thread reserves 256 MB (`GO2CS_GOROUTINE_STACK` changes it), and
  `main` gets the first thread's stack, usually far less. Recursion past that ends the process with no
  panic and no deferred calls, so run deep recursion in a goroutine.
* **`recover()` catches Go panics, not every .NET exception.** Nil dereference, divide by zero and index
  out of range recover as in Go. Any other .NET exception, that `NotImplementedException` included, runs
  your deferred calls and keeps unwinding; `recover()` returns nil. As in Go, `recover()` stops a panic
  only when the deferred function calls it directly, with two exceptions
  ([how](ConversionStrategies.md#defer--panic--recover)). An exported function that calls `recover()`
  stops the panic even when a deferred function calls it. An unexported one that another package defers
  through a func value returns nil.
  <!-- docs/ConversionStrategies.md, the recover-in-helper paragraph below the catchError sample; the read:
       src/core/golib/builtin.cs:369 `public static object? recover(bool calledByDefer) => calledByDefer ? recover() : null;`. -->
* **A crash's traceback is sometimes shorter than Go's.** An unrecovered panic prints Go's panic lines,
  the whole chain included (`panic: message [recovered, repanicked]`, and one TAB-indented line per
  earlier panic), and exits with status 2. A goroutine that panics while `main` returns still gets its
  report, as in Go. The traceback below the panic lines shows Go's frames for a panic in a function with
  one deferred call, and for a panic raised again by a named deferred function. The named function's
  frame shows the line it is declared on rather than the line that panics. It is shorter than Go's in
  three cases:
  * A panic raised again inside a deferred closure shows one frame, the function the first panic left,
    where Go shows the closure and the frames above it.
  * When several functions on the stack each defer a call, only the function that panicked is shown.
  * A goroutine's crash is labelled `goroutine 1`, and Go's `created by` line is missing.
  <!-- Measured 2026-10-24 by C2 on linux-x64, .NET SDK 10.0.112, converter and corpus at f8fe15c97e (batch 36), seven
       one-file programs converted with -recurse and built with -p:GoTargetOS=linux, each compared with go run under
       Go 1.26.10. Identical frames and lines: a no-import `panic("message")`; a panic in work() with one deferred call,
       called from main (work, then main). A named deferred handler that recovers and panics again: the same three
       frames (handler, panic, main.main), with handler at its declaration line 3 where Go prints line 5, and `panic()`
       where Go prints its argument words. A deferred closure that does the same: converted main.main:8 alone; Go
       main.main.func1, panic, main.main. Three functions each deferring a call, panicking in the innermost: converted
       main.c alone; Go c, b, a, main. A goroutine panic, with main sleeping or waiting on a WaitGroup the goroutine's
       deferred Done releases: exit 2 and the report in both, converted `goroutine 1 [running]` with no `created by`
       line where Go prints `goroutine 18` and `created by main.main in goroutine 1`. The chain is rendered by
       src/core/golib/runtime/CrashReport.cs:259 `// printpanics, oldest first: a panic whose older neighbour it re-raised is not printed again.`
       and :275 `text.Append(repanicked[i] ? " [recovered, repanicked]" : " [recovered]");`. The closure frame stays
       until a post-hop converter change (COORD, 2026-10-24); the goroutine report on main's return is batch 35's. -->
* **`unsafe` works over plain numbers and stops at anything .NET's garbage collector tracks**
  ([the rules](ConversionStrategies.md#unsafepointer-and-uintptr)). A pointer to anything holding a
  string, slice, map, interface, pointer or fixed-size array gets a stand-in number, not an address, so
  the older tricks (the `*(*string)(unsafe.Pointer(&b))` pun, a hand-built slice header) don't convert
  mechanically.
* **The address of another package's variable is the address of a copy.** When code takes the address of a
  variable that a different package exports and that is not itself a pointer, the pointer it gets refers to a
  copy, so writes through it never reach the variable. That covers `&otherpkg.V`, a pointer-receiver method
  value used as a value (`f := otherpkg.V.Reset`), and the common `flag.BoolVar(&otherpkg.Debug, "debug", false,
  "...")`, where parsing the flag leaves `otherpkg.Debug` unchanged. Calling the method directly
  (`otherpkg.V.Reset()`) and reading the variable both work, and inside one package all of these work. To avoid it,
  keep the variable and the code that takes its address in the same package, or have the package export a pointer
  (`var V = &T{}`) or an accessor function instead of the value.
  <!-- Diagnosed by lane R on windows/amd64 and linux/amd64 at 9b0269c2e8 (COORD inbox, 2026-10-25T19:24:56Z):
       `f := lib.G.Inc` emits `Ꮡ(lib.G).Inc`, a box of a copy (Go prints 1, converted 0); `&lib.G2` and
       `bump(&lib.Count)` likewise. An importer binds the owner's own box only when the owner boxes the variable, which
       it decides from its own syntax: src/go2cs/globalAddressOperations.go:237 (`func importedGlobalIsBoxed(varObj *types.Var) bool {`),
       with exported variables boxed only for a box-only pointer method at
       :188 (`func markExportedBoxOnlyGlobals(pkg *types.Package, into map[types.Object]bool) {`). The same-package method
       value is fixed for the 1.26 publication (R's seat F1); the cross-package case is ruled a documented limitation
       (owner, 2026-10-25), with the fix in the BOARD row of that date. -->
* **Garbage collection is .NET's.** An object your function just stopped using may not be collectible
  until the function returns, so "drop the reference, call `runtime.GC()`, expect the finalizer" can
  wait forever. `GOGC` and `GOMEMLIMIT` are read and reported, by `debug.SetGCPercent` for example, but
  .NET's collector does not follow them.
  <!-- src/core/runtime/managed_impl.cs:1268 `internal static void gcinitController() => ᏑgcController.init(readGOGC(), readGOMEMLIMIT());`;
       nothing in src/core/runtime or src/core/golib sets a GCSettings value or a GC heap limit from them. -->
* **Runtime introspection is thinner than Go's.** A dump of all goroutines lists every goroutine and
  what it waits on, but has frames for the calling goroutine only. `pprof.StartCPUProfile` gives a
  profile with no samples (profile with .NET's tools). `debug.ReadBuildInfo()` reports the program's
  module, its dependencies and its `GOOS`, `GOARCH`, `CGO_ENABLED` and `DefaultGODEBUG`, but no
  `-compiler`, `-buildmode`, `GOAMD64` or `vcs.*` settings. A replaced dependency shows without its
  replacement, a converted test program lists no dependencies, and a program converted outside a module
  reports nothing ([how](ConversionStrategies.md#package-conversion)). On Windows,
  `time.LoadLocation("Europe/Berlin")` fails until you import `time/tzdata` (or set `ZONEINFO`).
  <!-- Goroutine dump: docs/ConversionStrategies-Reference/manual-conversions.md, "runtime.Stack(all: true) enumerates
       every goroutine" (another goroutine prints `[stack unavailable: ...]`). CPU samples need the opt-in companion
       src/core/go2cs.CpuProfiler, which only generated test projects set and which is not packaged
       (src/core/go2cs.CpuProfiler/go2cs.CpuProfiler.csproj:31 `<IsPackable>false</IsPackable>`). Build info:
       docs/ConversionStrategies.md, "A main package's project also carries the program's build info"; the test host's
       missing dep lines and the missing => lines are BOARD rows of 2026-10-22 (H). -->
* **A few quiet ones**, each rare, where converted code gives a different answer and says nothing:
  * A variadic function that keeps, returns or captures its `...T` parameter, or `append`s or `copy`s
    into it, works on a private copy: after `f(s...)`, its writes are not the caller's `s`.
  * Copying a struct by value shares any fixed-size array inside an embedded struct.
  * A slice of one number type laid over another's storage
    (`unsafe.Slice((*uint64)(unsafe.Pointer(&b[0])), n)`) is a copy: writes through it never reach `b`.
  * A type switch's `case int:` also catches an `int32` or `rune` when the switch has no `int32` case of
    its own (`case uint:` likewise for `uint32`).

## What works, but costs you something

* **Speed.** Most compute lands within a small multiple of Go's time, string-heavy work takes several
  times longer, maps run at parity or better, and asking at run time whether a value satisfies an
  interface (`v.(fmt.Stringer)`) is the slowest thing converted code does. The
  [performance comparison](Performance.md) has the numbers. Measure your own hot path, in Release.
* **Startup, memory and shipping.** A converted program loads .NET and one DLL per converted Go package
  it uses, and the JIT compiles each method the first time it runs. Package-level tables are built the
  first time they are read rather than at startup. You ship a .NET app, not Go's small static binary:
  `dotnet build` output needs the .NET 10 runtime, and `dotnet publish -r <runtime>` gives a
  self-contained folder, compiled ahead of time with ReadyToRun, which starts faster: on linux-x64 it
  cut a converted program's median startup by about a third. `dotnet run` and `dotnet build` never get
  ReadyToRun ([how](ConsumingGoFromCSharp.md#rough-edges)). The trim mode is set for you: a converted
  project carries `partial` in its project file, and a C# project you write yourself gets the same
  default from the packages. A trimmed publish runs, and its output matches the untrimmed build's. It
  warns, measured on linux-x64: a project that uses the `go.*` packages gets one IL2104 for each package
  with trim warnings, eight for a program that uses `fmt`, and `-p:TrimmerSingleWarn=false` lists the
  hundred or so behind them, most of them golib's, `runtime`'s and `reflect`'s use of reflection. A
  project built from the converted source lists them all. A single-file publish needs ReadyToRun turned
  off ([Known issues](KnownIssues.md#a-single-file-publish-with-readytorun-runs-out-of-memory)), and a
  Native AOT publish is slow either way
  ([Known issues](KnownIssues.md)).
  <!-- ReadyToRun: src/go2cs/csproj-template.xml:58 `<PublishReadyToRun>true</PublishReadyToRun>`; the startup figure is
       P1's PerfStartup reading cited in docs/ConsumingGoFromCSharp.md (linux-x64, median 488 ms JIT, 329 ms ReadyToRun).
       Lazy tables: docs/ConversionStrategies.md, the [GoLazy] holder paragraphs (a package var whose initializer
       depends on nothing that changes is built on its first read). Trim warnings: P1's reconciliation, linux-x64,
       go1.26.10, .NET SDK 10.0.112, at a8821c781a (claude/p1-trim-recon ddf7c054d7,
       docs/phase4/DATA-trim-recon-p1-2026-10-24.md §2-§4). A fmt program converted with -recurse=nuget against a local
       feed of the closure's packages: rc 0 and 8 IL2104, one each for errors, internal.reflectlite, internal.sync, golib,
       reflect, runtime, time and unsafe; with -p:TrimmerSingleWarn=false, 110 (golib 41, runtime 40, reflect 14, time 6,
       unsafe 5, three others). The same program built with project references logs the 110 directly, which is C2's
       reading of the same day. In every arm the trimmed stderr equals the untrimmed build's. -->
* **Allocation.** Converted code allocates where Go's compiler doesn't, so expect more garbage and
  failing `testing.AllocsPerRun` assertions.
* **Crypto, most hashing and `math/big` run Go's portable code**, the way Go builds with `-tags purego`:
  correct, validated, and slower than Go's assembly. A TLS handshake is where you will notice.
* **Native AOT** starts much faster and holds less memory, but once running it is slower than the JIT
  on most rows of the comparison, and a publish compiles every converted package your program
  references, which takes hours and a lot of memory
  ([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
  has the measurements).

## Third-party modules: where they trip today

The [validated modules](ValidatedModules.md) page is still short.

* **A plain `-recurse` (your module plus its dependencies) applies one build tag for you, `safe`**,
  while the standard library is converted with Go's portable tags, `purego` and `math_big_pure_go`.
  So a dependency with an assembly fast path converts the fast path unless its fallback is behind
  `safe`: pass its portable tag (`purego` is the common one). An explicit `-tags` replaces every
  default, so include `safe` with yours, and under `-tests` include `purego,math_big_pure_go` too.
* **A test suite that imports testify depends on the `safe` tag.** Its bundled go-spew does pointer
  arithmetic into `reflect.Value` at load, which the runtime library refuses by design, and go-spew's
  `safe` tag selects its fallback. `-recurse` passes that tag for you
  ([Build tags for a module](ConvertingAModule.md#build-tags-for-a-module)). Drop it, with
  `-module-safe-tag=false` or an explicit `-tags` that leaves it out, and the converted test program
  dies at startup.
* **A hung test is stopped, where `go test` waits.** A converted test still running 15 minutes after it
  started is reported as a failure and marked abandoned, and the run moves on to the next test
  (`-test-deadline` changes the limit, [Command line](CommandLine.md)). Its goroutines keep running, so
  every later result says it ran after the abandoned test.
  <!-- docs/CommandLine.md, the -test-deadline row (default 15m; 0 turns it off). A converted test's argv has the
       shape go test gives a test binary:
       src/core/testing/TestOptions.cs:236-238 `keeps argv exactly the shape…gives a…test binary`. pflag's suite
       reads identical to its published proof at 9dcf0bafe1 (docs/phase4/DATA-p2-nugetgo-trial-2026-10-24.md §3). -->
* **Converter bugs that surface as C# compile errors**, usually in corners of the type system. In your
  own code, reshape the Go. In a dependency, point a `replace` at a patched copy, or fix the generated
  `.cs` and mark the file `[module: go.GoManualConversion]` so the next conversion leaves it alone. New
  ones still turn up: [tell me](#when-you-hit-a-seam).
* **One bad dependency blocks everything above it.** A package that fails to convert is reported and
  skipped, the run still exits 0, and every project that imports it fails to build. Read the summary
  line, `Recursive conversion complete in ...: N/M packages converted (K failed: ...)`, not the exit
  code. `-recurse=module`
  [converts only your packages](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later).

## What "validated" means, and what it doesn't

A package's own Go `Test` functions were converted, run, and compared with `go test` verdict for
verdict, by full test name. That's a real bar, and narrower than the word sounds:

* **Tests only.** Examples, benchmarks and fuzz targets never run. No race detector either.
  <!-- src/go2cs/testConversion.go:3916 `entry.Kind, entry.Status, entry.Reason = "fuzz", "unsupported", "fuzz execution is deferred to Phase 4D"`;
       Example- and Benchmark-only test files are left out of the compile set, :792
       `// Phase-4D file exclusion (option-a ruling): drop Example/Benchmark-only test files from the`. -->
* **Matched is not passed.** A test that skips on both sides counts as a match and measures nothing.
* **Disclosed differences are real differences.** Where a Go test asserts something a managed runtime
  can't satisfy, or doesn't yet, its exact failure is pinned and listed on the package's proof page,
  linked from [Validated Test Packages](ValidatedTestPackages.md)
  ([Disclosures](ValidatedTestPackages.md#disclosures) explains the kinds). Read the list for a package
  you lean on.
* **Nothing about your dependencies.** `go2cs -tests -recurse` tests the packages of the module you
  point it at, never its dependencies, so run it on each dependency you lean on.

## Platforms and toolchain

* **Go's security fixes arrive with go2cs releases.** The converted standard library is a conversion of
  the Go release in [Requirements](README.md#requirements). A fix Go issues later reaches you when go2cs
  converts and publishes that patch. A project converted with `-recurse=nuget` takes it on its next
  restore, because its `GoStdLibVersion` is the Go minor with a float, such as `1.26.*`
  ([Roadmap](Roadmap.md#go-releases)).
  <!-- The float: src/go2cs/toolchainResolution.go:525 `return strings.TrimPrefix(minor, "go") + ".*"`; the release
       lines: docs/PLAN-version-lines.md (master carries the current Go line with patch hops). -->
* **One Go release, one .NET release.** `go version` has to print exactly the release in
  [Requirements](README.md#requirements) when you convert, and when you `go install` the converter from a
  clone; `go2cs -version` should show that one release throughout. Rebuild after every pull. Each
  [GitHub release](https://github.com/ritchiecarroll/go2cs/releases) also carries the converter prebuilt
  for six platforms. Converted projects and the `go.*` packages target `net10.0`.
  <!-- .github/workflows/release-binaries.yml: a nuget-N.N.N.N tag builds the converter for six platforms and drafts a
       GitHub Release with the archives, SHA256SUMS and the attestation bundle. -->
* **Where it runs.** The standard library is validated on x64 Windows and x64 Linux (glibc, on
  Microsoft's build of .NET: Ubuntu's own package has an entry in [Known issues](KnownIssues.md)).
  macOS, Intel and Apple silicon, has packages and runs the README walkthrough, and is being validated
  package by package: a [roster](ValidatedTestPackages.md) row shows a macOS count once its package
  validates on both chips. Anywhere else the Windows flavor of the library loads, warns that it is
  `running the "windows" build`, and follows Windows rules or fails wherever it touches the OS.
* **One conversion, one operating system.** Go picks build-tagged files at conversion time, so convert
  once per OS (`-platforms linux/amd64`), into separate output folders.

## Reading and debugging the output

* **You debug the C#.** Breakpoints, stepping and compiler errors are on the generated `.cs`. A Go
  panic's own traceback does name the Go file and line, read from the program's `.pdb` files: a publish
  with `-p:DebugType=none` keeps the frames and loses every file and line.
  <!-- P1's reconciliation, linux-x64 (claude/p1-trim-recon ddf7c054d7, docs/phase4/DATA-trim-recon-p1-2026-10-24.md §5
       finding 2): "With -p:DebugType=none the traceback keeps its frames but loses every file:line line." -->
* **Some of the letters aren't on your keyboard.** `ж<T>`, `Ꮡx`, `goǃ(...)`, `ᐸꟷ(ch)`: several look like
  punctuation and are letters, so an ASCII search finds nothing. Keep the
  [glyph table](ConversionStrategies.md#reading-converted-code-names-and-glyphs) open.
* **Don't hand-edit converted files, the `.csproj` included.** The next conversion rewrites them, so pass
  build settings with `-p:`. For code, fix the Go, keep your own C# in a separate file, or mark a whole
  file `[module: go.GoManualConversion]` ([how](ConversionStrategies.md#manually-converted-declarations))
  and merge later Go changes yourself. A renamed Go file leaves its old `.cs` behind, still compiled.
* **Start with `go2cs -recurse=nuget <module> <out>`**, the one form whose output restores and builds
  with nothing staged. Go comments are dropped unless you pass `-comments`.
* **Calling converted Go from your own C#?** See [Rough edges](ConsumingGoFromCSharp.md#rough-edges).
  The road runs one way: Go source can't call a .NET API.

## What you might expect to break, and doesn't

The language itself (generics, closures, struct embedding, range-over-func, typed nil pointers in
interfaces, randomized map order), `defer`/`panic`/`recover`, channels and `select`, `reflect`
(`encoding/json` and `text/template` pass their own suites on it), and `net/http` with HTTP/2 and
`crypto/tls` over real sockets.

## When you hit a seam

1. **Start from a clean Go build**: `GOTOOLCHAIN=local`, then `go build ./...` for the same OS and tags.
2. **Read the conversion's warnings and its summary line**, not the exit code.
3. **A clean build that throws `NotImplementedException`** wants a pure-Go build tag. **A C# compile
   error in converted code** is a converter bug until proven otherwise.
4. **A behavior difference in the standard library?** Check the package's disclosures on
   [Validated Test Packages](ValidatedTestPackages.md).
5. **Check [Known issues](KnownIssues.md)**, then
   [open an issue](https://github.com/ritchiecarroll/go2cs/issues) with the output of `go2cs -version`,
   your OS, the error text, and the smallest Go that reproduces it.

If you find a seam that isn't on this page, that's the one I most want to hear about.
