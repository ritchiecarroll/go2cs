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

Usually, yes. In the [performance comparison](Performance.md), measured on Windows x64 (`win-x64`), most
compute lands within a small multiple of Go's time, string-heavy work takes several times longer, and maps
run at parity with Go or better. Asking at run time whether a value satisfies an interface is the slowest
thing converted code does. A converted program also starts slower and uses more memory than the Go it came
from. The comparison has the measurements and says which machine and toolchain produced them. go2cs applies
some optimizations, such as
[stack strings](ConversionStrategies.md#strings-string-and-sstring) and
[stack slices](ConversionStrategies.md#slices-and-arrays), and the [Roadmap](Roadmap.md#performance)
lists the work planned to narrow the gap. [Tuning converted code](Tuning.md) says which deployment and runtime settings
to choose.

## Does converted code really behave like Go?

That is measured, not claimed. Each standard-library package's own Go test suite is converted to C#, run,
and compared with `go test`, verdict for verdict, on Windows, Linux and macOS. A difference is never hidden: it
is disclosed by name, with its reason, on the package's row in
[Validated Test Packages](ValidatedTestPackages.md).
[What "validated" means, and what it doesn't](Limitations.md#what-validated-means-and-what-it-doesnt)
says what that proves.

Some things do differ, and you should know them before you start. By default a goroutine is an
operating-system thread, so thousands are fine and hundreds of thousands are not. A deadlock usually hangs where Go would
report it. A goroutine's stack does not grow.
[What converts, but doesn't behave exactly like Go](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)
lists them all.

## Did AI write this?

Not the foundation. I started go2cs in 2018 as a one-person, nights-and-weekends project, believing that Go's
simplicity could be brought to .NET. The runtime library, the conversion strategies and the shape of the generated
C# all come from that hand-built groundwork.

What changed recently is pace. Leaning on Claude took go2cs from an experiment to a tool in a matter of months — the
entire Go standard library now converts and compiles clean, and
[every package that can be validated](ValidatedTestPackages.md) passes its own Go test suite in C#.

The bar is the same whether I wrote the code or Claude did, and nothing lands because it looked right. More than a
thousand converted programs are compiled and their output compared against `go run`, every converter change is
measured by re-transpiling the corpus and byte-comparing the result, and each validated package carries a proof page
showing Go's verdicts beside C#'s.

Read about the journey: [How AI is used in this project](AIAssistance.md).

## What does not convert?

- **Parts of cgo.** `go2cs -cgo` converts a package whose C uses are calls of C library functions, on
  a machine with a C compiler. It was measured on linux/amd64, darwin/amd64, darwin/arm64 and
  windows/amd64.
  Other C uses are refused, by name, with the phase that covers them: Go memory handed to C, C code
  written inline in the Go file, and callbacks into Go. Without `-cgo`, a package that imports `"C"`
  stops the run ([what converts, and what comes next](Roadmap.md#cgo)).
  <!-- Heading was "Most of cgo" until 2026-11-20: true before `-cgo` converted anything, stale once it did (owner,
       first post-.4 docs batch, COORD 07:19Z). The README's FAQ list reads "Parts of cgo" to match. -->
- **Hand-written assembly.** A function whose body lives in a `.s` file becomes a stub that throws when
  it is called. Many libraries ship a pure-Go fallback behind a build tag, and converting with that tag
  is the usual answer.
- **Code that needs a newer Go than go2cs tracks.** The converter refuses it and names both releases.
- **One form of generic type alias**, an alias of an anonymous struct or interface.
- **A generic call over a pointer element whose constraint shares the element's storage.** `Walk[*Node]`
  under `Walk[N Tree[N]]`, where `Children()` returns `[]N`, is refused with `GO2CS2002`, naming the site
  and the method. The same code over a value element converts. Two related shapes are refused as
  `GO2CS2005` and `GO2CS2006` ([diagnostic codes](DiagnosticCodes.md)).

[What won't make the trip](Limitations.md#what-wont-make-the-trip) has the detail and the workarounds.

## Which Go and .NET versions does it use?

One Go release and one .NET release at a time, both named in the README's
[Requirements](README.md#requirements). The Go release it tracks is supported upstream. The
[Roadmap](Roadmap.md#go-releases) says which Go releases come next and in what order.

The packages target .NET 10 and are validated on it. A newer .NET SDK also builds a converted program, and on a
machine that has no .NET 10 runtime the program runs on the newer runtime, which the packages are not validated
on. The README's [Requirements](README.md#requirements) says how to require .NET 10 instead.
<!-- 2026-11-23 .5 docs pass (P1), UX-12, at master 97cca467a2: the FAQ said "exactly one ... .NET release at a
     time", which the README's Requirements contradicts since go.lib's buildTransitive targets give an executable
     RollForward=Major when no .NET 10 runtime is present (src/core/golib/buildTransitive/go.lib.targets:444 `<RollForward>Major</RollForward>`;
     src/tests/PackageTests/RollForward). Owner's release-readiness item UX-12 (COORD 2026-11-22 083506Z). -->

## Which platforms are supported?

Windows on x64, Linux on x64 and Arm64, and macOS on Intel and Apple silicon: the converted standard
library is validated against Go's own tests on each, and the [roster](ValidatedTestPackages.md) shows
each package's counts. One conversion targets one operating system, so you convert
once for each. [Platforms](Roadmap.md#platforms) has the rest.

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
order, and writes a solution that restores and builds. A dependency already published as a converted NuGet
package is referenced instead of converted
([Converting a real-world Go module](ConvertingAModule.md)). Not every module converts cleanly yet. A
dependency that uses assembly or `unsafe` shortcuts may need its pure-Go build tag, and real code still
turns up converter bugs. When one dependency blocks you, `-recurse=module,nuget` converts your own packages
first. [Third-party modules: where they trip today](Limitations.md#third-party-modules-where-they-trip-today)
is the honest list, and [Validated Modules](ValidatedModules.md) names the modules that pass their own
tests.

## Does it work with Native AOT?

It runs, and it costs you. A converted program can be published with Native AOT and starts much faster
than the same program under the JIT. The publish itself is slow and the executable is large, because
every converted package the program references is compiled in full. Once running, the program is slower
than under the JIT on most rows of the [performance comparison](Performance.md), measured on `win-x64`.
[Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
has the measured cost and the platform it was measured on, and the
[Roadmap](Roadmap.md#native-aot-and-trimming) has the work under way to reduce it.
[Tuning converted code](Tuning.md#choosing-a-deployment) compares Native AOT with the JIT and ReadyToRun,
and says how to publish: put `<PublishAot>true</PublishAot>` in the program's own project file and name that
project to `dotnet publish`. On Linux the publish needs `clang` and `zlib1g-dev`.

## What does converted code depend on at run time?

Three kinds of NuGet package, all open source: the runtime library ([`go.lib`](https://www.nuget.org/packages/go.lib)),
the converted standard-library packages your code imports (`go.<package>`), and the source generators
([`go.gen`](https://www.nuget.org/packages/go.gen)), which run only when you compile. They are built and versioned
together, so a project uses one version of all of them. A converted program
needs the .NET runtime, or you publish it self-contained. It does not need Go installed to run.
Converting uses a Go toolchain, which the go2cs dotnet tool downloads on first use.

## Do I need Go installed to build or run converted code?

No. Go is needed only to convert; building and running the converted C# need .NET alone. The
[dotnet tool guide](DotnetTool.md#the-go-it-converts-with) names the two conversion steps that need more.

## What license covers the code go2cs generates?

Not the converter's. [LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md) is the
authority, and says, under *Generated output*:

> Code generated by go2cs is not subject to the go2cs converter's AGPL license merely
> because it was produced by go2cs. Rights in generated output remain governed by the
> rights applicable to the input source and any separately licensed components
> incorporated into or referenced by the output. `src/go2cs/LICENSE-EXCEPTION` states
> this as a binding additional permission.

And of the converter's own license:

> The public converter grant is **AGPL-3.0-only**, not “or later.” The full text is
> in `src/go2cs/LICENSE` and `LICENSES/AGPL-3.0-only.txt`. The copyright holders
> grant one additional permission under AGPL section 7: the **go2cs Converter
> Output Exception** in `src/go2cs/LICENSE-EXCEPTION`, which states that the
> converter's output is never a covered work merely because go2cs produced it and
> carries the MIT grant for the embedded templates and other original scaffolding
> reproduced in that output.

The same page's table gives the rest: the runtime library and the source generators are MIT, and the
converted standard library is BSD-3-Clause, with Go's upstream notices.

Running the converter, locally or in your own build, triggers no AGPL obligation; see the README's
[License](README.md#license) section.

## Can it convert C# to Go?

No. A source conversion from C# to Go would need so many restrictions on the C# that it would not be
practical. To call compiled .NET code from Go instead, see
[go-dotnet](https://github.com/matiasinsaurralde/go-dotnet), which hosts the .NET runtime, or
[embedding Mono through cgo](https://www.mono-project.com/docs/advanced/embedding/).

## Why does Go stop with "unexpected NUL in input"?

The Go file is not UTF-8. It is almost always a file made in Windows PowerShell 5.1 with `>` or
`Out-File`, which write UTF-16, and then edited in place. Go source must be UTF-8, so `go build` fails
before go2cs is involved at all.
[The walkthrough's note](ConvertingAModule.md#step-by-step) shows how to create the file as UTF-8 and a
one-line command that re-saves a file that is already UTF-16.
