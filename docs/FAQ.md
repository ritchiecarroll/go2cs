# Frequently asked questions

Short, direct answers for a Go developer or a C# developer meeting go2cs for the first time. Each answer
links to the page that has the detail. The limits are collected in [Limitations](Limitations.md), and
specific bugs with their workarounds are in [Known issues](KnownIssues.md).

## Why convert Go to C#?

Mostly to use Go code from inside .NET. When the library you need is written in Go, the usual choices are
a separate process, a native wrapper, or a hand rewrite that has to be kept in step forever. go2cs adds
another: convert it, and call it as managed code. Converted code is also a way to run a Go program
anywhere .NET runs, and because the C# mirrors the Go, a way to learn one language from the other.
[Background](Background.md#why-convert-go-to-c) has the longer answer.

If you write Go and have no .NET in your life, go2cs gives you nothing Go does not already do better.
It is for the place where the two meet.

## Is converted code slower than Go?

Usually, yes. Most compute lands within a small multiple of Go's time, string-heavy work takes several
times longer, and maps run at parity with Go or better. Asking at run time whether a value satisfies an
interface is the slowest thing converted code does. A converted program also starts slower and uses more
memory than the Go it came from. The [performance comparison](Performance.md) has the measurements and
says which machine and platform produced them. Some optimization is already in, such as
[stack strings](ConversionStrategies.md#strings-string-and-sstring) and
[stack slices](ConversionStrategies.md#slices-and-arrays), and the [Roadmap](Roadmap.md#performance)
lists the work planned to narrow the gap.

## Does converted code really behave like Go?

That is measured, not claimed. Each standard-library package's own Go test suite is converted to C#, run,
and compared with `go test`, verdict for verdict, on Windows and Linux. A difference is never hidden: it
is disclosed by name, with its reason, on the package's row in
[Validated Test Packages](ValidatedTestPackages.md).
[What "validated" means, and what it doesn't](Limitations.md#what-validated-means-and-what-it-doesnt)
says what that proves.

Some things do differ, and you should know them before you start. A goroutine is an operating-system
thread, so thousands are fine and hundreds of thousands are not. A deadlock usually hangs where Go would
report it. A goroutine's stack does not grow.
[What converts, but doesn't behave exactly like Go](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
lists them all.

## What does not convert?

- **cgo.** A package that imports `"C"` is refused, by name, and the run stops
  ([the plan](Roadmap.md#cgo)).
- **Hand-written assembly.** A function whose body lives in a `.s` file becomes a stub that throws when
  it is called. Many libraries ship a pure-Go fallback behind a build tag, and converting with that tag
  is the usual answer.
- **Code that needs a newer Go than go2cs tracks.** The converter refuses it and names both releases.
- **One form of generic type alias**, an alias of an anonymous struct or interface.

[What won't make the trip](Limitations.md#what-wont-make-the-trip) has the detail and the workarounds.

## Which Go and .NET versions does it use?

Exactly one Go release and one .NET release at a time, both named in the README's
[Requirements](README.md#requirements). The Go release it tracks today is out of upstream support, so
the converted standard library carries none of the Go security fixes issued since. The
[Roadmap](Roadmap.md#go-releases) says which Go releases come next and in what order.

## Which platforms are supported?

Windows and Linux on x64, where the converted standard library is validated against Go's own tests.
macOS, on Intel and Apple silicon, has packages and is being validated package by package; a row on the
[roster](ValidatedTestPackages.md) shows a macOS count once its package validates there. One conversion
targets one operating system, so you convert once for each. [Platforms](Roadmap.md#platforms) has the
rest.

## What does the generated C# look like?

Like the Go, on purpose. A Go method is a C# extension method, multiple results are a tuple, and an
embedded struct's fields are promoted. Names keep Go's spelling, so they do not follow .NET naming
conventions, and a few Go ideas with no C# spelling use letters you will not find on your keyboard, such
as `ж<T>` for a pointer. The [glyph table](ConversionStrategies.md#reading-converted-code-names-and-glyphs)
explains each one. To judge it on real code, see
[Real standard-library conversions, side by side](README.md#real-standard-library-conversions-side-by-side):
converted files beside their Go source, from simple to complex.

It is readable, and it is still generated code. Do not edit a converted file by hand, because the next
conversion rewrites it: change the Go, or mark a file as manually converted so the converter leaves it
alone. You debug the C#, with breakpoints and stepping on the generated files.
[Reading and debugging the output](Limitations.md#reading-and-debugging-the-output) covers this, and the
[Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) shows Go and its
C# side by side.

## How do I call a converted Go package from C#?

Reference it like any other library. A Go package becomes a .NET assembly: a published one is a NuGet
`PackageReference`, and one you convert yourself is a `ProjectReference`. Go's strings, slices, maps,
errors and multiple results each have a C# shape, and
[Consuming converted Go from C#](ConsumingGoFromCSharp.md) shows every one with working code, rough
edges included. The road runs one way: Go source cannot call a .NET API.

## Why not call Go through a native library instead?

You can, and if you need Go's own speed it is the better choice: export the Go functions as C, build a
native library, and import it with P/Invoke. The cost is a native binary to build and ship for every
platform, a C toolchain, two runtimes and two garbage collectors in one process, and a boundary where
every string and slice is copied or pinned. Converted code is managed code in one runtime. It builds
with `dotnet build`, deploys as part of your application, and you can step into it in the debugger.
It is slower than native Go, and that is the trade.

## Can it convert a module with third-party dependencies?

Yes. `go2cs -recurse=nuget` converts a module and every third-party package it imports, in dependency
order, and writes a solution that restores and builds
([Converting a real-world Go module](ConvertingAModule.md)). Not every module converts cleanly yet. A
dependency that uses assembly or `unsafe` shortcuts may need its pure-Go build tag, and real code still
turns up converter bugs. When one dependency blocks you, `-recurse=module` converts your own packages
first. [Third-party modules: where they trip today](Limitations.md#third-party-modules-where-they-trip-today)
is the honest list, and [Validated Modules](ValidatedModules.md) names the modules that pass their own
tests.

## Does it work with Native AOT?

It runs, and it costs you. A converted program can be published with Native AOT and starts much faster
than the same program under the JIT. The publish itself is slow and the executable is large, because
every converted package the program references is compiled in full, and once running the program is
slower than the JIT on most measurements.
[Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
has the measured cost and the platform it was measured on, and the
[Roadmap](Roadmap.md#native-aot-and-trimming) has the work under way to reduce it.

## What does converted code depend on at run time?

Three kinds of NuGet package, all open source: the runtime library ([`go.lib`](https://www.nuget.org/packages/go.lib)),
the converted standard-library packages your code imports (`go.<package>`), and the source generators
([`go.gen`](https://www.nuget.org/packages/go.gen)), which run only when you compile. A converted program
needs the .NET runtime, or you publish it self-contained. It does not need Go installed to run. You need
Go only to convert.

## What license covers the code go2cs generates?

Not the converter's. The converter is licensed under AGPL-3.0 with an output exception that says so as a
binding term: code produced by go2cs stays under the license of the Go source it came from, and running
the converter creates no AGPL obligation. The runtime library and the source generators are MIT, and the
converted standard library is BSD-3-Clause, as Go's is.
[LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md) is the authority.

## Can it convert C# to Go?

No. A source conversion from C# to Go would need so many restrictions on the C# that it would not be
practical. To call compiled .NET code from Go instead, see
[go-dotnet](https://github.com/matiasinsaurralde/go-dotnet), which hosts the .NET runtime, or
[embedding Mono through cgo](https://www.mono-project.com/docs/advanced/embedding/).
