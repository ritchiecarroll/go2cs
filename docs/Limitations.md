# Limitations: where the seams are

So you're about to point go2cs at your own Go code. Before you spend an evening on it, here is where it
will fail, where it will fight you, and where it will quietly do something a little different from Go --
better you hear it from me than from a stack trace. Specific bugs with workarounds live in
[Known issues](KnownIssues.md). This page is the limits that come with running Go on .NET, plus the rough
spots real code hits today.

## The short version

**Good odds tonight:** pure Go that builds with Go 1.24 and `CGO_ENABLED=0`, leans mostly on the
standard library, and runs on x64 Windows or Linux. **A longer evening:** lots of third-party modules,
`unsafe`, assembly fast paths, or tests that pull in testify or read `os.Args`.

* **You need Go 1.24 itself on your PATH**, the release go2cs tracks, and **cgo is refused**.
* **Assembly, `//go:linkname` and deep `unsafe` tricks compile clean, then fail at run time.** Many
  libraries ship a pure-Go fallback behind a build tag, and you have to pass that tag yourself.
* **Goroutines are real threads.** Thousands are fine. Hundreds of thousands are not.
* **It's usually slower than Go**, starts slower and uses more memory.
* **One conversion targets one operating system**, and the standard library is validated on x64
  Windows and Linux only.
* **Most failures are loud. A few differences are silent**: see the last bullet under
  [what behaves differently](#what-converts-but-doesnt-behave-exactly-like-go).

## What won't make the trip

* **cgo.** A package whose build selects a file with `import "C"` stops the whole run:
  `error GO2CS2001: ... cgo has no C# conversion yet`. With no pure-Go path it is out of reach today
  ([plan](PLAN-cgo-interop.md), [Roadmap](Roadmap.md#cgo)).
* **Go newer than the library tracks.** A newer `go` line in your `go.mod`, or any dependency's, gets
  `package requires newer Go version` or a refusal naming both releases. Set `GOTOOLCHAIN=local` and pin
  older versions until `go build ./...` passes, as the
  [README walkthrough](README.md#converting-a-real-world-module) does.
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

* **Goroutines are operating-system threads** ([why](ConversionStrategies.md#goroutines)). Each holds a
  thread for as long as it lives, the ceiling is roughly ten thousand live goroutines where Go manages a
  million, and `runtime.GOMAXPROCS(1)` does not make anything run one at a time. Bound your fan-out with
  a worker pool, and remember that a `net/http` server spends one on every open connection.
* **A deadlock usually hangs** where Go prints `all goroutines are asleep - deadlock!`. Break in with a
  debugger: each goroutine is a thread named `goroutine-N`.
* **Stacks don't grow.** A goroutine's thread reserves 256 MB (`GO2CS_GOROUTINE_STACK` changes it), and
  `main` gets the first thread's stack, usually far less. Recursion past that ends the process with no
  panic and no deferred calls, so run deep recursion in a goroutine.
* **`recover()` catches Go panics, not every .NET exception.** Nil dereference, divide by zero and index
  out of range recover as in Go. Any other .NET exception, that `NotImplementedException` included, runs
  your deferred calls and keeps unwinding; `recover()` returns nil.
* **`unsafe` works over plain numbers and stops at anything .NET's garbage collector tracks**
  ([the rules](ConversionStrategies.md#unsafepointer-and-uintptr)). A pointer to anything holding a
  string, slice, map, interface, pointer or fixed-size array gets a stand-in number, not an address, so
  the older tricks (the `*(*string)(unsafe.Pointer(&b))` pun, a hand-built slice header) don't convert
  mechanically.
* **Garbage collection is .NET's.** An object your function just stopped using may not be collectible
  until the function returns, so "drop the reference, call `runtime.GC()`, expect the finalizer" can
  wait forever. `GOGC` and `GOMEMLIMIT` change nothing.
* **Runtime introspection is thinner than Go's.** A dump of all goroutines has frames for the calling
  goroutine only, `pprof.StartCPUProfile` gives a profile with no samples (profile with .NET's tools),
  and `debug.ReadBuildInfo()` answers `ok == false`. On Windows, `time.LoadLocation("Europe/Berlin")`
  fails until you import `time/tzdata` (or set `ZONEINFO`).
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
  it uses. You ship a .NET app, not Go's small static binary: `dotnet build` output needs the .NET 10
  runtime, and `dotnet publish -r <runtime>` gives a self-contained folder. Leave the trim mode alone:
  a full trim removes members the runtime library reaches by reflection.
* **Allocation.** Converted code allocates where Go's compiler doesn't, so expect more garbage and
  failing `testing.AllocsPerRun` assertions.
* **Crypto, most hashing and `math/big` run Go's portable code**, the way Go builds with `-tags purego`:
  correct, validated, and slower than Go's assembly. A TLS handshake is where you will notice.
* **Native AOT** starts much faster and holds less memory, but once running it is slower than the JIT
  on most rows of the comparison, and a publish compiles every converted package your program
  references: hours, and more memory than most laptops have
  ([the cost](Performance.md#what-the-aot-column-costs-to-produce--the-honesty-footnote)).

## Third-party modules: where they trip today

The [validated modules](ValidatedModules.md) page is still short.

* **A plain `-recurse` (your module plus its dependencies) applies no build tags for you**, while the
  standard library is converted with Go's portable tags, `purego` and `math_big_pure_go`. So a
  dependency with an assembly or `unsafe` fast path converts the fast path: pass its portable tag
  (`purego` is the common one). Under `-tests` an explicit `-tags` replaces the default, so include
  `purego,math_big_pure_go` with yours.
* **A test suite that imports testify isn't there yet.** Its bundled go-spew does pointer arithmetic
  into `reflect.Value` at load, which the runtime library refuses by design, so the converted test
  program dies at startup. go-spew's `safe` tag selects its fallback, and nothing passes it for you.
* **Tests that read `os.Args`** see the flags of the C# program that runs them. `flag.Parse()` is fine;
  a CLI framework that walks `os.Args[1:]` itself, as cobra and pflag do, diverges from `go test`.
* **Converter bugs that surface as C# compile errors**, usually in corners of the type system. In your
  own code, reshape the Go. In a dependency, point a `replace` at a patched copy, or fix the generated
  `.cs` and mark the file `[module: go.GoManualConversion]` so the next conversion leaves it alone. New
  ones still turn up: [tell me](#when-you-hit-a-seam).
* **One bad dependency blocks everything above it.** A package that fails to convert is reported and
  skipped, the run still exits 0, and every project that imports it fails to build. Read the summary
  line, `Recursive conversion complete in ...: N/M packages converted (K failed: ...)`, not the exit
  code. `-recurse=module`
  [converts only your packages](README.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later).

> **Note -- known today, and temporary.** These converter bugs are being worked through and may be
> resolved in a future release: an exported alias of an unexported type used from another package
> (`CS0122`; logrus and testify each have one), a parenthesized type in a type assertion, a
> function-local pointer type used as a conversion, a defined type over another package's type used as
> a conversion, and a promoted method with a parameter named `target`. One more is silent rather than
> a compile error: a method on a named map type that has the name and parameters of a map operation
> the runtime library also provides (`Add(key, value)`, `Remove(key)`, `Clear()`, `ContainsKey(key)`,
> and `Send(value)` on a named channel type) loses to the library's member, so the Go method does not
> run.

## What "validated" means, and what it doesn't

A package's own Go `Test` functions were converted, run, and compared with `go test` verdict for
verdict, by full test name. That's a real bar, and narrower than the word sounds:

* **Tests only.** Examples, benchmarks and fuzz targets convert but never run. No race detector either.
* **Matched is not passed.** A test that skips on both sides counts as a match and measures nothing.
* **Disclosed differences are real differences.** Where a Go test asserts something a managed runtime
  can't satisfy, or doesn't yet, its exact failure is pinned and listed on the package's proof page,
  linked from [Validated Test Packages](ValidatedTestPackages.md)
  ([Disclosures](ValidatedTestPackages.md#disclosures) explains the kinds). Read the list for a package
  you lean on.
* **Nothing about your dependencies.** `go2cs -tests -recurse` tests the packages of the module you
  point it at, never its dependencies, so run it on each dependency you lean on.

## Platforms and toolchain

* **The Go release it tracks is out of upstream support.** The converted standard library carries none
  of the Go security fixes issued since ([Roadmap](Roadmap.md#go-releases)).
* **One Go release, one .NET release, and you build the converter yourself.** `go version` has to print
  exactly the release in [Requirements](README.md#requirements) when you `go install` the converter and
  when you convert; `go2cs -version` should show that one release throughout. Rebuild after every pull.
  Converted projects and the `go.*` packages target `net10.0`.
* **Where it runs.** The standard library is validated on x64 Windows and x64 Linux (glibc, on
  Microsoft's build of .NET: Ubuntu's own package has an entry in [Known issues](KnownIssues.md)).
  macOS, Intel and Apple silicon, has packages and runs the README walkthrough, but isn't validated
  package by package yet. Anywhere else the Windows flavor of the library loads, warns that it is
  `running the "windows" build`, and follows Windows rules or fails wherever it touches the OS.
* **One conversion, one operating system.** Go picks build-tagged files at conversion time, so convert
  once per OS (`-platforms linux/amd64`), into separate output folders.

## Reading and debugging the output

* **You debug the C#.** Breakpoints, stepping and compiler errors are on the generated `.cs`. A Go
  panic's own traceback does name the Go file and line.
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
