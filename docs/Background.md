# Background

I've been programming in C# for many years, and I still like to keep an eye on new tech, whatever the language. Go kept showing up. The easiest way for me to try something new is to wire it into code I already have, so this project [started](README.md#milestones) as a way to bring Go into my C# world -- and to learn a thing or two about Go along the way. These days the reasons for a project like this are a lot better than "I was curious" -- and the best part is, I can *show* you most of them.

The converter took two tries to get right. The first was written in C# and read Go through an ANTLR grammar. It got surprisingly far, but a grammar only knows what Go code *looks like*, and a faithful conversion needs to know what it *means*: every type, every method set, every interface a value satisfies. So I did what every developer dreads and started over. Today's converter is written in Go and reads code with the same packages Go's own tools use (`go/ast` and `go/types`). Turns out nobody understands Go quite like Go does.

## Why convert Go to C#?

**To use Go code from inside .NET.** Plenty of shops are all-in on .NET. When the library they need is written in Go, the usual options are a separate process, a native wrapper, or a hand rewrite kept in sync forever. go2cs adds a fourth: convert it and reference it like any other .NET assembly. The converted standard library ships on [NuGet](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), and one command (`-recurse=nuget`) turns a whole Go app, dependencies and all, into a solution you can build and run. The README [walks through one](README.md#converting-a-real-world-module), and the converted app's output matches `go run`.

**To migrate with your eyes open.** Compiling is not the same as working, and I try never to blur the two. So the yardstick here is **Go's own tests**: a package's real `_test.go` suite is converted to C#, run, and compared verdict for verdict against `go test`. You can [reproduce that yourself](README.md#try-it-yourself--validate-a-converted-test-suite) from a fresh clone. That turns migration from a leap of faith into a plan: move a package at a time, and keep the receipts.

**To give Go code another place to run.** Converted code is ordinary .NET, so it deploys like any other .NET app, and published with [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/) it becomes one self-contained executable, just like a Go binary. Converting is fast and cheap, so you can even keep writing Go and convert again: *code in Go, run in .NET* -- something until now unheard of.

**To get C# you can read.** Receiver methods become extension methods, multiple returns become tuples, embedded structs become promoted fields, and the machinery behind Go's semantics stays out of sight. Judge for yourself with the README's [side-by-side table](README.md#real-standard-library-conversions-side-by-side), or the construct-by-construct map in [Conversion Strategies](ConversionStrategies.md). Readable code means a team can *maintain* it -- fix a bug in the C# on a Tuesday afternoon -- instead of owning machine output nobody can review.

**To learn.** Because the output mirrors the Go, it doubles as a phrasebook between the two languages. The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) puts the Tour of Go on one side of the window and the generated C# on the other, live. That's the tool I wanted for myself when I started.

## Where it stands

**What works today:**

* The whole Go standard library converts and compiles as .NET assemblies.
* Every standard-library package that *can* be validated passes its own Go test suite in C#, on Windows and on Linux. Each one has a [proof page](ValidatedTestPackages.md), Go's verdict beside go2cs's, test by test, and the few that can't be validated are [listed with their reasons](ValidatedTestPackages.md#excluded-packages).
* Underneath, hundreds of small Go programs check the language features one by one, comparing the C# output against Go's.
* Real third-party Go modules convert too, and `go2cs -tests -recurse` judges a whole module the way the standard library is judged: by its own test suites ([validated modules](ValidatedModules.md)). [nugetgo.net](https://nugetgo.net) is the community registry for converted modules.

Of course, everything is not peaches and cream. A tool like this is worth exactly as much as its worst *undisclosed* defect, so here are mine:

* **It's usually slower than Go.** Go wins most benchmarks, starts faster and travels lighter on memory. [More below](#how-fast-is-converted-code).
* **Some Go tests ask for the impossible, or the not-yet.** The impossible ones check things no managed runtime can promise, like the inner workings of Go's own runtime, or whether an object a test just stopped using can already be collected. The not-yet ones count allocations Go's compiler avoids and go2cs doesn't, and each carries a plan to retire it. None are quietly skipped: each one is pinned by its exact failure signature in a [committed file](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/bytes/go2cs_test_disclosures.json), so any *other* failure of that test still counts.
* **Hand-written assembly doesn't make the trip.** .NET can't run Go's assembly code, so the converted library reproduces Go built with its portable fallbacks (`-tags purego`).
* **Not every Go module converts cleanly yet.** Real-world code still turns up converter bugs. The [README](README.md#converting-a-real-world-module) shows how to convert a module's own code first and deal with its dependencies later.

Bugs that would bite you are written up in the open, in [Known issues](KnownIssues.md).

## How fast is converted code?

**`TL;DR`**: Go is usually faster. That doesn't mean your converted code is going to be super slow -- it should run as fast as other comparable .NET applications -- it just may not run as fast as the Go it came from.

Most compute lands within a small multiple of Go's time. String-heavy work takes several times longer, and the one thing C# has no native answer for -- checking at run time whether a value satisfies an interface -- is the big outlier. And it isn't always slower: maps, and the string comparisons the converter can optimize, run at parity with Go, or faster.

Why the gap? Go's strings, slices and interfaces don't map one-to-one onto .NET's, so a runtime library emulates them, and the converter aims for C# a Go programmer can read and follow, not hyper-tuned C#. Startup is the other cost: Go compiles straight to machine code, while .NET compiles as it runs ([JIT](https://en.wikipedia.org/wiki/Just-in-time_compilation)) unless you publish with Native AOT, which closes most of that gap at the cost of a slower, heavier build. You don't have to take my word for any of this: the [performance comparison](Performance.md) measures Go, C# on the JIT, and C# with Native AOT, methodology and all.

Need Go's own speed? You can [export Go functions](https://pkg.go.dev/cmd/cgo#hdr-C_references_to_Go) as C, build them into a native library, and [import them](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.dllimportattribute) in C#. go2cs is for when you want the code itself.

---

If you code in Go and ever need to build a C# app, I hope this gives you a head start: bring your existing code along, and keep using the standard library you already know.
