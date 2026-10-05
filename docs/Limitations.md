# Limitations: where the seams are

So you're about to point go2cs at your own Go code. Before you spend an evening on it, here is where it
will fail, where it will fight you, and where it will quietly do something a little different from Go --
better you hear it from me than from a stack trace. This is not a bug list: specific bugs with
workarounds live in [Known issues](KnownIssues.md). These are the limits that come with running Go on
.NET, plus the rough spots real code hits today.

## The short version

**Good odds tonight:** pure Go that builds with Go 1.24 and `CGO_ENABLED=0`, leans mostly on the
standard library, and runs on x64 Windows or Linux. **A longer evening:** lots of third-party modules,
`unsafe`, assembly fast paths, or tests that pull in testify or read `os.Args`.

* **You need Go 1.24 itself on your PATH**, the release go2cs tracks, which upstream no longer patches.
  A dependency that asks for a newer Go is refused: pin it to a release that still builds.
* **cgo is refused**, loudly. `CGO_ENABLED=0`, or a build tag that deselects the file, is the way
  through.
* **Assembly, `//go:linkname` and deep `unsafe` tricks compile clean, then fail at run time.** Many
  libraries ship a pure-Go fallback behind a build tag, and you have to pass that tag yourself.
* **Goroutines are real threads.** Thousands are fine. Hundreds of thousands are not.
* **It's usually slower than Go**, starts slower and uses more memory. Native AOT fixes the startup, not
  the speed.
* **Third-party modules still turn up converter bugs**, which surface as C# compile errors.
* **One conversion targets one operating system.** The standard library is validated on x64 Windows and
  Linux. macOS runs but isn't validated yet, and ARM Linux isn't there.
* **"Validated" means each standard-library package's own `Test` functions matched `go test`.** Not
  examples, benchmarks, fuzzing, speed, or your dependencies.
* **Most failures are loud. A few differences are silent**: see the last bullet under
  [what behaves differently](#what-converts-but-doesnt-behave-exactly-like-go).

## What won't make the trip

Refused outright:

* **cgo.** A package whose build selects a file with `import "C"` stops the whole run:
  `error GO2CS2001: ... cgo has no C# conversion yet`. With no pure-Go path it is out of reach: there is
  a [plan](PLAN-cgo-interop.md) and a place on the [Roadmap](Roadmap.md#cgo), and nothing you can use
  today.
* **Go newer than the library tracks.** If your `go.mod`, or any dependency's, declares a newer `go`
  line, you get `package requires newer Go version` or a refusal naming both releases. Current module
  releases often want a newer Go already, so set `GOTOOLCHAIN=local` and pin older versions until
  `go build ./...` passes, as the [README walkthrough](README.md#converting-a-real-world-module) does.

Converted, and still not going to work:

* **Hand-written assembly.** A function whose body lives in a `.s` file becomes a stub. The project
  builds clean, and the first call throws a `NotImplementedException` saying
  `no implementation reached this compilation`. Convert with the library's pure-Go build tag
  ([below](#third-party-modules-where-they-trip-today)); with no fallback, the body has to be written by
  hand in C#. `//go:linkname` into the runtime gets the same stub, outside a short list the converter
  knows.
* **One generics form.** `type Cell[T any] = struct{ Row, Col T }`, a generic alias of an anonymous
  struct or interface, gets a warning, and every use is a C# compile error. Name the struct instead.
* **Code that doesn't type-check for the target** still converts, best effort, with the literal text
  `invalid type` where a type is missing (`error GO2CS1003`), and the C# build stops there.

## What converts, but doesn't behave exactly like Go

* **Goroutines are operating-system threads** ([why](ConversionStrategies.md#goroutines)). Every
  goroutine holds a thread of its own for as long as it lives, the ceiling is roughly ten thousand live
  goroutines where Go manages a million, and `runtime.GOMAXPROCS(1)` does not make anything run one at a
  time. Bound your own fan-out
  with a worker pool, and remember that a `net/http` server spends one on every open connection.
* **A deadlock usually hangs.** Go prints `all goroutines are asleep - deadlock!`. Converted code
  reports only the case it can prove, every goroutine parked on a nil channel or in `select {}`.
  Otherwise break in with a debugger: each goroutine is a thread named `goroutine-N`.
* **Stacks don't grow.** A goroutine's thread reserves 256 MB of address space (`GO2CS_GOROUTINE_STACK`
  changes it), and `main` gets the first thread's stack, usually far less. Recursion past that ends the
  process with no panic and no deferred calls, so run deep recursion in a goroutine.
* **`recover()` catches Go panics, not every .NET exception.** Nil dereference, divide by zero and index
  out of range recover as in Go. Any other .NET exception, that `NotImplementedException` included, runs
  your deferred calls and keeps unwinding; `recover()` returns nil.
* **`unsafe` works over plain numbers and stops at anything .NET's garbage collector tracks**
  ([the rules](ConversionStrategies.md#unsafepointer-and-uintptr)). Pointer arithmetic over an array of
  numbers or a struct of plain scalars aliases real memory, and `unsafe.String`, `StringData`,
  `SliceData` and a same-type `unsafe.Slice(&s[i], n)` are supported. A pointer to anything holding a
  string, slice, map, interface, pointer or fixed-size array gets a stand-in number, not an address:
  arithmetic on it panics unless the offset lands exactly on a field of the type you ask for. So the
  older tricks don't convert mechanically: the `*(*string)(unsafe.Pointer(&b))` pun, a hand-built slice
  header, an interface read as two machine words. The standard library's own cases are rewritten by
  hand. Yours are not.
* **Garbage collection is .NET's.** Finalizers and weak pointers work, but an object your function just
  stopped using may not be collectible until the function returns, so "drop the reference, call
  `runtime.GC()`, expect the finalizer" can wait forever. `GOGC`, `GOMEMLIMIT` and `debug.SetGCPercent`
  are remembered and change nothing.
* **Runtime introspection is thinner than Go's.** A dump of all goroutines has frames for the calling
  goroutine only, and by default `pprof.StartCPUProfile` gives a valid profile with no samples: profile
  with .NET's tools. Facts Go's linker bakes in are gone, too. `debug.ReadBuildInfo()` answers
  `ok == false`, and `runtime.GOROOT()` is empty unless `GOROOT` is set, so on Windows
  `time.LoadLocation("Europe/Berlin")` fails until you import `time/tzdata`.
* **One slice tops out near two billion elements**, the limit of a .NET array: `make([]byte, 3<<30)`
  panics.
* **A few quiet ones**, each rare, where converted code gives a different answer and says nothing:
  * A variadic function that keeps, returns or captures its `...T` parameter, or `append`s or `copy`s
    into it, works on a private copy: after `f(s...)`, its writes and the slice it returns are not the
    caller's `s`.
  * Copying a struct by value copies everything except a fixed-size array inside an embedded struct:
    both copies share that one array.
  * A slice of one number type laid over another's storage, as in
    `unsafe.Slice((*uint64)(unsafe.Pointer(&b[0])), n)`, is a copy: reads are right, and writes through
    it never reach `b`.
  * A type switch's `case int:` also catches an `int32` or `rune` when the switch has no `int32` case of
    its own (`case uint:` likewise for `uint32`).

## What works, but costs you something

* **Speed.** Converted code is usually slower than the Go it came from: most compute lands within a
  small multiple of Go's time, string-heavy work takes several times longer, maps run at parity or
  better, and ranging over a function iterator hands every value across two goroutines. The
  [performance comparison](Performance.md) has the numbers, but it is micro-benchmarks from one Windows
  machine, with no HTTP server, TLS or JSON in it. Measure your own hot path, in Release.
* **Startup, memory and shipping.** A converted program loads .NET and one DLL (a .NET "assembly") per
  converted Go package it uses, then runs their initializers; even a small app loads dozens. And you
  ship a .NET app, not Go's small static binary: `dotnet build` output needs the .NET 10 runtime on the
  target, and `dotnet publish -r <runtime>` gives a self-contained folder. Leave the trim mode alone: a
  full trim removes members the runtime library reaches by reflection.
* **Asking at run time whether a value satisfies an interface** (`v.(fmt.Stringer)` on a value the
  converter couldn't pair with that interface at compile time) is the slowest thing converted code does,
  by a wide margin. Ordinary interface calls and assertions to concrete types cost a small multiple of
  Go's.
* **Allocation.** Go's compiler keeps values off the heap that converted code still allocates, so expect
  more garbage, and expect `testing.AllocsPerRun` assertions to fail. They are one of the largest
  families of [disclosed test differences](ValidatedTestPackages.md#disclosures) in the standard
  library.
* **Crypto, most hashing and `math/big` run Go's portable code**, the way Go builds with `-tags purego`:
  correct, validated, and slower than Go's hand-tuned assembly. A TLS handshake is one place you will
  notice ([Roadmap](Roadmap.md#performance)).
* **Native AOT** (.NET's ahead-of-time compile to one native executable) works, starts much faster than
  the JIT and holds less memory. But once running it is slower than the JIT on most rows of the
  performance comparison, and every publish compiles your program plus every converted package it
  references: hours, and more memory than most laptops have
  ([what a publish costs](Performance.md#what-the-aot-column-costs-to-produce--the-honesty-footnote)).
  Validation runs on the JIT, and the runtime library generates code at run time in places, which AOT
  can't do: test `reflect`-heavy paths there before you trust it.

## Third-party modules: where they trip today

Third-party modules are just getting started: the [validated modules](ValidatedModules.md) page is
short, and a `vendor/` folder or a `go.work` workspace of your own has no test behind it yet.

* **A plain `-recurse` (your module plus its dependencies) applies no build tags for you**, while the
  standard library is converted with `purego`. So a dependency with an assembly or `unsafe` fast path
  converts the fast path. Pass its portable tag: `purego` is the common one, and some libraries use
  their own. Under `-tests` (convert and run a package's Go tests) an explicit `-tags` replaces the
  default, so include `purego,math_big_pure_go` with yours.
* **A test suite that imports testify isn't there yet.** Its bundled go-spew does pointer arithmetic
  into `reflect.Value` at load, which the runtime library refuses by design, so the converted test
  program dies at startup. go-spew's `safe` tag selects its fallback, and nothing passes it for you.
  testify also trips the alias bug below.
* **Tests that read `os.Args`** see the flags of the C# program that runs the converted tests.
  `flag.Parse()` is fine; a CLI framework that walks `os.Args[1:]` itself, as cobra and pflag do, meets
  arguments it doesn't know, and its tests diverge from `go test`.
* **Converter bugs that surface as C# compile errors.** The ones I know of sit in corners of the type
  system: an exported alias of an unexported type used from another package (`CS0122`; logrus and
  testify each have one), a parenthesized type in a type assertion, a function-local pointer type or a
  defined type over another package's type used as a conversion, and a promoted method with a parameter
  named `target`. In your own code, reshape the Go. In a dependency, point a `replace` at a patched
  copy, or fix the generated `.cs` and mark the file `[module: go.GoManualConversion]` so the next
  conversion leaves it alone. There will be others: [tell me](#when-you-hit-a-seam).
* **One bad dependency blocks everything above it.** A package that fails to convert is reported and
  skipped, the run still exits 0, and every project that imports it then fails to build. Look for the
  summary line, `Recursive conversion complete in ...: N/M packages converted (K failed: ...)`, not the
  exit code (a timing table follows it). `-recurse=module`
  [converts only your packages](README.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later),
  and its output does not build until the dependencies convert.
* **The [Target Atlas](https://go2cs.net/TargetAtlas.html) is a forecast.** It grades popular Go modules
  from a static scan of their source, not by converting them. Use it to rule modules out, and read an A
  as "worth trying".

## What "validated" means, and what it doesn't

It means a package's own Go `Test` functions were converted, run, and compared with `go test` verdict
for verdict, by full test name. That's a real bar. It is also narrower than the word sounds:

* **Tests only.** Examples, benchmarks and fuzz targets convert but never run. No race detector either.
* **Matched is not the same as passed.** A test that skips on both sides, for lack of root say, counts
  as a match and measures nothing.
* **Disclosed differences are real differences.** Where a Go test asserts something a managed runtime
  can't satisfy, or doesn't yet, its exact failure is pinned and listed on the package's proof page
  (Go's verdict beside go2cs's, test by test), linked from its row on
  [Validated Test Packages](ValidatedTestPackages.md)
  ([Disclosures](ValidatedTestPackages.md#disclosures) explains the kinds). Read the list for a package
  you lean on.
* **One configuration.** Suites run as a Release build with .NET's tiered JIT off (every method fully
  optimized on first use). `dotnet run -c Debug` and a ReadyToRun (precompiled) publish are different
  configurations, where timing-, GC- and stack-frame-sensitive code can answer differently;
  `-test-tiered` and `-test-config Debug` let you check.
* **Not every function has a body, and not every package is listed.** A function Go backs with assembly
  that no test reaches is still a throwing stub, a package with no tests of its own isn't on the page,
  and a handful that can't be validated are
  [listed with their reasons](ValidatedTestPackages.md#excluded-packages).
* **Nothing about your dependencies.** `go2cs -tests -recurse` tests the packages of the module you
  point it at, never its dependencies, so run it on each dependency you lean on. It wants a local
  standard library (`-go2cspath` pointing at your clone's `src` folder), not NuGet, an output root
  outside the module, and a `-test-timeout` above the two-minute default for a slow suite.

## Platforms and toolchain

* **The Go release it tracks is out of upstream support.** The converted standard library carries none
  of the Go security fixes issued since ([Roadmap](Roadmap.md#go-releases)).
* **One Go release, one .NET release.** `go version` has to print exactly the release in
  [Requirements](README.md#requirements), both when you `go install` the converter and when you convert.
  A converter built with a newer Go takes that release as its own: nothing is refused, and the generated
  projects ask NuGet for `go.*` packages that don't exist. `go2cs -version` should show one release on
  `go.build`, `go.toolchain` and `go.packages`. Converted projects and the `go.*` packages target
  `net10.0` only.
* **You build the converter yourself**, from a clone, with `go install`, and the `go.*` package version
  floats. Rebuild after every pull, and pass `-p:GoStdLibVersion=<exact version>` when a build has to
  stay put: a mismatch shows up as C# errors about runtime members that don't exist.
* **Where it runs.** The standard library is validated on x64 Windows and x64 Linux (glibc, on
  Microsoft's build of .NET: Ubuntu's own package has an entry in [Known issues](KnownIssues.md), and
  Alpine's musl is untested). macOS, Intel and Apple silicon, has packages and runs the README
  walkthrough, but isn't validated package by package yet. Anywhere else (ARM Linux, say) the Windows
  flavor of the library loads, warns on stderr that it is `running the "windows" build`, and follows
  Windows rules or fails wherever it touches the OS. `runtime.GOARCH` reads `amd64` everywhere.
* **One conversion, one operating system.** Go picks build-tagged files at conversion time, so convert
  once per OS (`-platforms linux/amd64`), into separate output folders. The publish profiles written
  beside every executable don't change that: only the one for the OS you converted for is good, and the
  ARM Linux, ARM Windows and 32-bit ones have no library flavor behind them.
* **A local standard library off Windows** needs `-p:GoTargetOS=linux` (or `darwin`) on every `dotnet`
  command, because it defaults to `windows`
  ([building against one](README.md#optional-build-against-a-local-standard-library)). NuGet picks the
  flavor for you.

## Reading and debugging the output

* **You debug the C#.** Breakpoints, stepping and compiler errors are on the generated `.cs`; nothing
  maps them back to the `.go` for you. A Go panic's own traceback does name the Go file and line.
* **Some of the letters aren't on your keyboard.** `ж<T>`, `Ꮡx`, `goǃ(...)`, `ᐸꟷ(ch)`: several look like
  punctuation and are letters, so an ASCII search finds nothing. Keep the
  [glyph table](ConversionStrategies.md#reading-converted-code-names-and-glyphs) open.
* **Don't hand-edit converted files, the `.csproj` included.** The next conversion rewrites them (in a
  project file only an `<ItemGroup Label="GoHandOwnReferences">` block survives), so pass build settings
  with `-p:`. For code, fix the Go, keep your own C# in a separate file beside it, or mark a whole file
  `[module: go.GoManualConversion]` ([how](ConversionStrategies.md#manually-converted-declarations)) and
  merge later Go changes yourself. And the converter never deletes: a renamed Go file leaves its old
  `.cs` behind, still compiled.
* **Start with `go2cs -recurse=nuget <module> <out>`**, the one form whose output restores and builds
  with nothing staged. A bare `go2cs main.go` or package conversion writes beside the Go source, and its
  project expects a runtime staged under the go2cs root (`-go2cspath`, default `~/go2cs`). Go comments
  are dropped unless you pass `-comments`.
* **Calling converted Go from your own C#?** See [Rough edges](ConsumingGoFromCSharp.md#rough-edges).
  The road runs one way: C# calls converted Go, and Go source can't call a .NET API. Loading the runtime
  library also switches the console to UTF-8, and any unhandled exception then ends the process with
  exit code 2.

## What you might expect to break, and doesn't

The language itself (generics, closures, struct embedding, range-over-func, typed nil pointers in
interfaces, randomized map order), `defer`/`panic`/`recover`, channels and `select`, `reflect`
(`encoding/json` and `text/template` pass their own suites on it), and `net/http` with HTTP/2 and
`crypto/tls` over real sockets.

## When you hit a seam

1. **Start from a clean Go build**: `GOTOOLCHAIN=local`, then `go build ./...` for the same OS and tags.
2. **Read the conversion's warnings and its summary line**, not the exit code. `-debug` adds a stack
   trace.
3. **A clean build that throws `NotImplementedException`** wants a pure-Go build tag. **A C# compile
   error in converted code** is a converter bug until proven otherwise.
4. **A behavior difference in the standard library?** Find the package on
   [Validated Test Packages](ValidatedTestPackages.md), open its proof page, and check its disclosures.
5. **Check [Known issues](KnownIssues.md)**, then
   [open an issue](https://github.com/ritchiecarroll/go2cs/issues) with the output of `go2cs -version`,
   your OS, the error text, and the smallest Go that reproduces it.

If you find a seam that isn't on this page, that's the one I most want to hear about.
